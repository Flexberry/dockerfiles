#!/bin/sh

# Скрипт автосборки образов по протоколам https://cloud.docker.com/
# обеспечивает сборку и тестирование образов на основе локального  git-репозитория
#
# Формат вызова:
# testAutobuild.sh <каталог_репозитория_образа> [версия_сборки]
#
# Скрипт запусается из родительского каталога локального git-репозитория
# Каталог репозитория сборки включает имя git-репозитория (первый поддиректорий) и тропу (последующие поддиректории) до каталога, где находится Dockerfile (напроимер dockerfiles/pentaho/official/)
# и кроме основных файлов и каталогов для docker-сборки должен включать в себя файл .autobuid формата:
# IMAGE=<имя_образа_без_префикса_flexberry/>
# VERSION=<версия_сервиса(если_есть)>
#
# При отсутсвии параметра версия_сборки формируется следующая PATCH'евая (последнее число в триаде x.y.z) версия образа
# и сборка производится в текущей ветке репозитория (разрвботчик имеет возможность указать другую МИНОРНУЮ или МАЖОРНУЮ версию сборки)
#
# При наличии параметра версия_сборки перед сборкой производится переключение на указанный git-тег
# (формируется из IMAGE, VERSION и  версия_сборки).
#
#
# После сборки при наличии файлов *test.yml производится тестирование собранного образа согласно этим файлам
# При успешном тестировании производится линковка собранного образа со всеми его алиасами
# сформированный git-тег добавляется git-репозиторий и передается (`git pull`) на githib.com

isPreRelease() {
  if=$IFS
  IFS=" 0123456789."
  set -- $1
  v=`echo $*`
  IFS=$ifs
  if [ -n "$v" ]
  then
    return 0
  fi
  return 1
}

parceVersion() {
  IFS=-
  set -- $1
  IFS=$ifs
  prerelease=$2
  IFS=.
  set -- $1
  IFS=$ifs
  echo $* $prerelease
}

# СКРИПТ РАЗБИВАЕТ DOCKER-ТЕГ На ЧАСТИ: version, build, prerelease
# version, build - опциональны и могут быть пустыми после анализа
parseDockerTag() {
  IFS=-
  set -- $1
  IFS=$ifs
  version=
  build=
  prerelease=
  case $# in
    3)
      version=$1
      build=$2
      prerelease="$3"
      echo "Service version $version, build version $build, prerelease $prerelease"
      fullTag="${version}-"
      ;;
    2)
      if isPreRelease $2
      then
        build=$1
        prerelease="$2"
        echo "Build version $build , prerelease $prerelease"
        fullTag=
      else
        version=$1
        build=$2
        echo "Service version $version, build version $build"
        fullTag="${version}-"
      fi
      ;;
    1)
      build=$1
      echo "Build version $build"
      fullTag=
    ;;
    *)
      echo "Incorrect verison format in tag. Tags must have a look [ServiceVersion-]BuildVersion[-Prereleaseversion]"
  esac
}

parentImagesFromDockerfile() {
   grep '^FROM ' Dockerfile |
   while read from image tail
   do
    echo $image
   done
}

getLastGitTag() {
  gitTagPrefix=$1
  lastGitTag=`git for-each-ref --format='%(*creatordate:raw)%(creatordate:raw) %(refname)' refs/tags | sort -nr | grep "refs/tags/${IMAGE}_" | head -1`
  if [ -z "$lastGitTag" ]
  then
    echo "Не найдены теги для образа $IMAGE. Теги дожны иметь вид $IMAGE_[ВерсияCервиса-]ВерсияСборки"
    exit 2
  fi
  set -- $lastGitTag
  lastGitTag=$3
  lastGitTag=${lastGitTag:10}
  echo $lastGitTag
}

getBuildFromGitTag() {
  IFS=_
  set -- $1
  image=$1
  gitTag=$2

  IFS=-
  set -- $gitTag
  shift
  IFS=$ifs
  echo $*
}

############ MAIN #################
export version
export build
export prerelease

case $# in
  1) ;;
  2) ;;
  *)
    echo "НЕВЕРНОЕ ЧИСЛО ПАРАМЕТРОВ"
    echo "Формат вызова: testAutobuild.sh <каталог_репозитория_образа> [версия_сборки]"
    exit 1
esac

dir=$1
if [ ! -d $dir ]
then
  echo "Каталог сборки '$dir' отсутствует!"
  echo "Формат вызова: testAutobuild.sh <каталог_репозитория_образа> [версия_сборки]"
  exit 1
fi
cd $dir
PARAMBUILD=$2

ifs=$IFS
IFS=/
set -- $dir
repository=$1
shift
subdir=`echo "$*"`
IFS=$ifs

autobuildFile="./.autobuild"
if [ ! -f  $autobuildFile ]
then
  echo "Отсутствует autobuildFile $autobuildFile";
  exit 1;
fi
IMAGE=
VERSION=
. $autobuildFile
# echo "dir=$dir repository=$repository subdir=$subdir IMAGE=$IMAGE VERSION=$VERSION"
echo "------------------------------------------
СБОРКА ОБРАЗА ИЗ КАТАЛОГА $subdir РЕПОЗИТОРИЯ $repository.
ИМЯ ОБРАЗА: $IMAGE"

dockerTagPrefix=
if [ -n "$VERSION" ]
then
  echo "ВЕРСИЯ: $VERSION";
  dockerTagPrefix="${VERSION}-"
  gitTagPrefix="${IMAGE}_${dockerTagPrefix}"
  imageNamePrefix="flexberry/${IMAGE}:${dockerTagPrefix}"
  versionImageName="flexberry/${IMAGE}:${VERSION}"
else
  imageNamePrefix="flexberry/${IMAGE}:"
fi
gitTagPrefix="${IMAGE}_${dockerTagPrefix}"

latestImageName="flexberry/${IMAGE}:latest"

if [ -z "$PARAMBUILD" ]
then
  echo "ВЕРСИЯ СБОРКИ НЕ УКАЗАНА"
  buildsFile="./.builds"
  gitTag=`getLastGitTag $gitTagPrefix`
  set -- `getBuildFromGitTag $gitTag`
  LASTBUILD=$1
  prerelease=$2
  echo "ПРЕДЫДУЩАЯ ВЕРСИЯ $LASTBUILD В GIT-РЕПОЗИТОРИИ С ТЕГОМ $gitTag"
  set -- `parceVersion $LASTBUILD`
  major=$1
  minor=$2
  patch=$3
  if [ -n "$prerelease" ]
  then
    echo "ПОСЛЕДНЯЯ ВЕРСИЯ  $major.$minor.$patch БЫЛА ПРЕДРЕЛИЗНОЙ: $prerelease. ВВЕДИТЕ:";
    echo "<ENTER>, ЕСЛИ ПЛАНИРУЕТЕ СОБРАТЬ РЕЛИЗНУЮ ВЕРСИЮ  $major.$minor.$patch";
    echo "ИМЯ СЛЕДУЮШЕЙ ПРЕДРЕЛИЗНОЙ ВЕРСИИ (ДОЛЖНО СОДЕРЖАТЬ АЛФАВИТНЫЕ СИМВОЛЫ [A-Z][a-z], например alpha.0.9) :"
    echo "ПОЛНОЕ ИМЯ СЛЕДУЩЕЙ ВЕРСИИ (1.0.0 или 1.0.0-alpha.0.9)";
    read reply
    if [ -z "$reply" ]
    then
      build="$major.$minor.$patch"
    else
      if isPreRelease $reply
      then
        build="$major.$minor.$patch-$reply"
      fi
    fi
  else
    let patch=$patch+1
    build="${major}.${minor}.${patch}"
    echo "СЛЕДУЮШАЯ PATCH-ВЕРСИЯ GIT-ТЕГА: $build"
    echo "ЕСЛИ ЭТО PATCH-РЕЛИЗ НАЖМИТЕ <ENTER> ИЛИ ВВЕДИТЕ ПОЛНОЕ ИМЯ СЛЕДУЩЕЙ ВЕРСИИ (1.0.0 или 1.0.0.-alpha.0.9)";
    read reply
    if [ -n "$reply" ]
    then
      build=
    fi
  fi

  if [ -n "$build" ]
  then
    BUILD=$build
  else
    while [ -z "$build" ]
    do
      parseDockerTag $reply
    done
#     gitTagPrefix="${IMAGE}_${reply}"
    BUILD=$build
  fi
  echo "ПРИНИМАЕТСЯ ВЕРСИЯ СБОРКИ $BUILD"
  gitTag="${gitTagPrefix}${BUILD}"
  if [ -n "$prerelease" ]
  then
#     gitTag="$gitTag-$prerelease"
    echo "С ПРЕДРЕЛИЗНОЙ ВЕРСИЕЙ $prerelease"
  fi
else  # Указана версия сборки
  parseDockerTag $PARAMBUILD
  BUILD=$build
  echo "ПРИНИМАЕТСЯ ВЕРСИЯ СБОРКИ $BUILD"
  parceVersion $BUILD
  git commit -a
  git checkout master
  gitTag="${gitTagPrefix}${BUILD}"
  if [ -n "$prerelease" ]
  then
    gitTag="$gitTag-$prerelease"
    echo "С ПРЕДРЕЛИЗНОЙ ВЕРСИЕЙ $prerelease"
  fi
  git checkout $gitTag
fi


echo "СОБИРАЕТСЯ ОБРАЗ $IMAGE РЕПОЗИТОРИЯ $repository В ПОДДИРЕКТОРИИ $subdir С GIT-ТЕГОМ $gitTag"


for parentImages in `parentImagesFromDockerfile`
do
  echo "ЗАГРУЗКА РОДИТЕЛЬСКОГО ОБРАЗА $parentImages"
  docker pull $parentImages
done

fullImageName="${imageNamePrefix}${BUILD}"
if [ -n "$prerelease" ]
then
  fullImageName="$fullImageName-$prerelease"
fi
set -- ` parceVersion $BUILD`
major=$1
minor=$2
patch=$3
majorImageName="${imageNamePrefix}${major}"
minorImageName="${imageNamePrefix}${major}.${minor}"


echo "СОЗДАНИЕ ОСНОВНОГО ОБРАЗА $fullImageName"
docker build -t $fullImageName .

for testFile in *.test.yml
do
  if [ ! -f $testFile ]
  then
    break
  fi
  echo -ne "\n\nТЕСТИРОВАНИЕ ОБРАЗА ФАЙЛОМ КОНИГУРАЦИИ $testFile ..."
  if docker-compose -f $testFile up --build --exit-code-from sut
  then
    echo "ТЕСТИРОВАНИЕ ЗАКОНЧИЛОСЬ УСПЕШНО!"
  else
    echo "ТЕСТИРОВАНИЕ ЗАКОНЧИЛОСЬ С ОШИБКОЙ";
    exit $?
  fi
done

if [ -z "$PARAMBUILD" ]
then
  echo -ne "\nPULL..."
  git pull;
  echo -ne "COMMIT..."
  if git commit -a
  then
    echo -ne "\nPUSH..."
    git push;
  fi
  echo "НАВЕШИВАНИЕ GIT-ТЕГА $gitTag"
  git tag $gitTag
  echo -ne "\nPUSH TAGS ..."
  git push --tags
  echo ""
fi

echo "НАСТРОЙКИ AUTOBUILD ДЛЯ ОБРАЗА $IMAGE: https://cloud.docker.com/u/flexberry/repository/docker/flexberry/$IMAGE/builds
Source Type: Tag
Source: /^${gitTagPrefix}([0-9]+).([0-9]+).([0-9]+)([0-9a-zA-Z.-]*)\$/
Docker Tag: ${dockerTagPrefix}{\\1}.{\\2}.{\\3}{\\4}
Dockerfile location: Dockerfile
Build Context: /${subdir}
";


if [ -x hooks/pre_push ]
then
  echo "Выполнить скрипт hooks/pre_push(Y/n)? "
  read reply
  if [ -z "$reply" -o "$reply" = 'y' -o  "$reply" = 'Y' ]
  then
    export IMAGE_NAME="/$fullImageName"
    ./hooks/pre_push
  fi
fi
if [ -x hooks/post_push ]
then
  echo "Выполнить скрипт hooks/post_push(y/N)? "
  read reply
  if [ "$reply" = 'y' -o  "$reply" = 'Y' ]
  then
    export IMAGE_NAME="/$fullImageName"
    ./hooks/post_push
  fi
fi
exit



