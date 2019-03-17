#!/bin/sh

# Скрипт автосборки образов по протоколам https://cloud.docker.com/
# обеспечивает сборку и размещение образов на основе локального или guthub.com git-репозитория
# Формат вызова:
# autobuild.sh [local|github] <каталог_репозитория_образа> [версия_сборки]
# Скрипт запусается из родительского каталога локального git-репозитория
# Каталог репозитория сборки включает имяя git-репозитория и тропу до каталога, где находится Dockerfile (напроимер dockerfiles/pentaho/official/)
# и кроме основных файлов и каталогов для docker-сборки должен включать в себя файл .autobuid формата:
# IMAGE=<имя_образа_без_префикса_flexberry/>
# VERSION=<версия_сервиса(если_есть)>
#
# Eсли первый параметр - local, сборка производится в локальном репозитории в каталоге <каталог_репозитория_образа>.
# При наличии параметра версия_сборки перед сборкой производится переключение на указанный git-тег
# (формируется из IMAGE, VERSION и  версия_сборки).
#
# Eсли первый параметр - github, создается (если е создан до этого каталог .autobuild) и в нем (если не сделано до этого)
# произвоится клониование указанного препозитоия из gitub.com/Flexberry.
# Если параметр версия_сборки не указан он берется из последней успешной локальной сборки 
# После клонирования производится переключение на указанный git-тег (формируется из IMAGE, VERSION и  версия_сборки).
#
# После сборки при наличии файлов *test.yml производится тестирование собранного образа согласно этим файлам
# При успешном тестировании производится линковка собранного образа со всеми его алиасами
# Если параметр версия_сборки указан не был, перед линковкой запрашивается ввод версии сборки.
# Из IMAGE, VERSION и  версия_сборки  формирутся полные тег и передается в gihub-репозитоий
# По окончании сборки запрашивается необходимость передачи сформированных образов в hub.docker.com.
# В случае утведительного ответа собанные образы передаются в hub.docker.com.

source=$1
dir=$2
build=$3

ifs=$IFS
IFS=/
set -- $dir
repository=$1
shift
subdir=`echo "$*"`
IFS=$ifs

autobuildFile="$dir/.autobuild"
if [ ! -f  $autobuildFile ]
then
  echo "Отсутствует autobuildFile $autobuildFile";
  exit 1;
fi
IMAGE=
VERSION=
. $autobuildFile
# echo "dir=$dir repository=$repository subdir=$subdir IMAGE=$IMAGE VERSION=$VERSION"
if [ -n "$VERSION" ]
then
  gitTagPrefix="${IMAGE}_${VERSION}-"
else
  gitTagPrefix="${IMAGE}_"
fi

case $source in
  local)
    ;;
  github)
    ;;
  *)
    echo "Не указан источник сборки local или github"
    echo "Формат:"
    echo "$0 [local|github] <каталог_репозитория_образа> [версия_сборки]"
    exit 1
esac


echo "Собирается образ $IMAGE репозитория $repository в поддиректории $subdir"

if [ ! -d .autobuild ]
then
  mkdir .autobuild
fi

cd .autobuild
if [ ! -d $repository ]
then
  git clone https://github.com/Flexberry/$repository
fi
cd $repository
git checkout master >/dev/null 2>&1
git pull >/dev/null 2>&1
git pull --tags >/dev/null 2>&1


lastGitTag=`git for-each-ref --format='%(*creatordate:raw)%(creatordate:raw) %(refname)' refs/tags | sort -nr | grep "refs/tags/${IMAGE}_" | head -1`
if [ -z "$lastGitTag" ]
then
  echo "Не найдены теги для образа $IMAGE. Теги дожны иметь вид $IMAGE_[ВерсияCервиса-]ВерсияСборки"
  exit 2
fi

set -- $lastGitTag
lastGitTag=$3
lastGitTag=${lastGitTag:10}
git checkout $lastGitTag >/dev/null 2>&1


IFS=_
set -- $lastGitTag
image=$1
gitTag=$2

IFS=-
set -- $gitTag
IFS=$ifs
version=
case $# in
  2)
    version=$1
    build=$2
    echo "Версия сервиса $version, версия сборки $build"
    fullTag="${version}-"
    ;;
  1)
    build=$1
    echo "Версия сборки $build"
    fullTag=
  ;;
  *)
    echo "Неверный формат версии образа в теге. Теги дожны иметь вид ${IMAGE}_[ВерсияCервиса-]ВерсияСборки"
    exit 3
esac

fullImage="flexberry/$IMAGE:$fullTag$build"
latestImage="flexberry/$IMAGE:latest"
versionImage=
if [ -n "$version" ]
then
  versionImage="flexberry/$IMAGE:$version"
fi

IFS=.
set -- $build
IFS=$ifs
if [ $# -ne 3 ]
then
  echo "Неверный формат версии сборки. Версия сборки имеет вид MAJOR.MINOR.PATCH (См. https://semver.org/spec/v2.0.0.html)"
  exit 4
fi
major=$1
minor=$2
patch=$3
majorImage="flexberry/$IMAGE:$fullTag$major"
minorImage="flexberry/$IMAGE:$fullTag$major.$minor"


cd $subdir
echo "Создание основного образа $fullImage"
docker build --no-cache -t $fullImage .
docker login -u kafnevod -p tais1993
echo "Размещение основного образа $fullImage в репозитории"
docker push $fullImage

for image in $latestImage $majorImage $minorImage $versionImage
do
  docker tag $fullImage $image
  docker push $image
  echo "Размещение алиаса $image в репозитории"
done



