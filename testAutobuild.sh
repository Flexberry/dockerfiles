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

parceVersion() {
  IFS=.
  set -- $1
  IFS=$ifs
  if [ $# -ne 3 ]
  then
    echo "Неверный формат версии сборки. Версия сборки имеет вид MAJOR.MINOR.PATCH (См. https://semver.org/spec/v2.0.0.html)"
    exit 4
  fi
  echo $*
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
  IFS=$ifs
  echo $2
}

############ MAIN #################
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
  if [ -f $buildsFile ]
  then
    LASTBUILD=
    . $buildsFile
    echo "ПРЕДЫДУЩАЯ ВЕРСИЯ СБОРКИ  В ФАЙЛЕ $buildsFile $LASTBUILD"
  fi
  if [ -z "$LASTBUILD" ]
  then
    gitTag=`getLastGitTag $gitTagPrefix`
    LASTBUILD=`getBuildFromGitTag $gitTag`
    echo "ВЕРСИЯ СБОРКИ НЕДОСТУПНА. ПРЕДЫДУЩАЯ ВЕРСИЯ $LASTBUILD В GIT-РЕПОЗИТОРИИ"
  fi
  set -- `parceVersion $LASTBUILD`
  major=$1
  minor=$2
  patch=$3
  let patch=$patch+1
  BUILD="${major}.${minor}.${patch}"
  echo "СЛЕДУЮШАЯ PATCH-ВЕРСИЯ GIT-ТЕГА: $BUILD"
  echo "ЕСЛИ ЭТО PATCH-РЕЛИЗ НАЖМИТЕ <ENTER> ИЛИ ВВЕДИТЕ МИНОРНУЮ (МАЖОР.МИНОР) ИЛИ МАЖОРНУЮ (МАЖОР) ВЕРСИЮ";
  while read majorminor
  do
    if [ -z "$majorminor" ]
    then
      break;
    fi
    IFS=.
    set -- $majorminor
    IFS=$ifs
    case $# in
    1)
      BUILD="${majorminor}.0.0"
      break
      ;;
    2)
      BUILD="${majorminor}.0"
      break
      ;;
    *)
      echo "НЕВЕРНЫЙ ФОРМАТ ВЕРСИИ. КОРРЕКТНЫЙ ФОРМАТ: МИНОРНАЯ (МАЖОР.МИНОР) ИЛИ МАЖОРНАЯ (МАЖОР)"
    esac
  done
  gitTag="${gitTagPrefix}${BUILD}"
  echo "ПРИНИМАЕТСЯ ВЕРСИЯ СБОРКИ $BUILD"
else  # Указана версия сборки
  BUILD=$build
  parceVersion $BUILD
  git commit -a
  git checkout master
  gitTag="${gitTagPrefix}${BUILD}"
  git checkout $gitTag
fi

echo "СОБИРАЕТСЯ ОБРАЗ $IMAGE РЕПОЗИТОРИЯ $repository В ПОДДИРЕКТОРИИ $subdir С GIT-ТЕГОМ $gitTag"


for parentImages in `parentImagesFromDockerfile`
do
  echo "ЗАГРУЗКА РОДИТЕЛЬСКОГО ОБРАЗА $parentImages"
  docker pull $parentImages
done

fullImageName="${imageNamePrefix}${BUILD}"
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
  echo -ne "\n\nТЕСТИРОВАНИЕ ОБРАЗА ФАЙЛОМ КОНИГУРАЦИИ $testFile ..."
  if docker-compose -f $testFile up --build --exit-code-from sut
  then
    echo "ТЕСТИРОВАНИЕ ЗАКОНЧИЛОСЬ УСПЕШНО!"
  else
    echo "ТЕСТИРОВАНИЕ ЗАКОНЧИЛОСЬ С ОШИБКОЙ";
    exit $?
  fi
done

for image in $latestImageName $majorImageName $minorImageName $versionImageName
do
  echo "СОЗДАНИЕ АЛИАСА $image"
  docker tag $fullImageName $image
done

if [ -z "$PARAMBUILD" ]
then
  echo -ne "COMMIT..."
  git commit -a
  echo -ne "\nPULL..."
  git pull;
  echo -ne "\nPUSH..."
  git push;
  echo "НАВЕШИВАНИЕ GIT-ТЕГА $gitTag"
  git tag $gitTag
  echo -ne "\nPUSH TAGS ..."
  git push --tags
  echo ""
fi

echo "НАСТРОЙКИ AUTOBUILD ДЛЯ ОБРАЗА $IMAGE: https://cloud.docker.com/u/flexberry/repository/docker/flexberry/$IMAGE/builds
Source Type: Tag 
Source: /^${gitTagPrefix}-([0-9]+).([0-9]+).([0-9]+)\$/
Docker Tag: ${dockerTagPrefix}{\\1}.{\\2}.{\\3}
Dockerfile location: Dockerfile
Build Context: ${subdir}
";
echo -ne "ПЕРЕДАТЬ СОЗДАННЫЕ ОБРАЗЫ НА hub.docker.com(y/N)? ";
read reply
if [ "$reply" != 'Y' ]
then
  exit 0;
fi

for image in $fullImageName $latestImageName $majorImageName $minorImageName $versionImageName
do
  echo "ПЕРЕДАЧА ОБРАЗА $image"
  docker push $image
done

