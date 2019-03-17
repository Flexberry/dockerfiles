#!/bin/sh

# Скрипт автосборки образов по протоколам https://cloud.docker.com/
# обеспечивает сборку и размещение образов на основе локального или guthub.com git-репозитория
# Формат вызова:
# autobuild.sh [local|github] <каталог_репозитория_обаза> [версия сборки]
# Скрипт запусается из родительского каталога локального git-репозитория
# Каталог репозитория сборки включает имяя git-репозитория и тропу до каталога, где находится Dockerfile (напроимер dockerfiles/pentaho/official/)
# и кроме основных файлов и каталогов для docker-сборки должен включать в себя файл .autobuid формата:
# IMAGE=<имя_образа_без_префикса_flexberry/>
# VERSION=<версия_сервиса(если_есть)>
# BUILD=<версия_сборки>
# Переменные IMAGE, VERSION (если есть) за


ifs=$IFS
dir=$1
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
. $autobuildFile
# echo "dir=$dir repository=$repository subdir=$subdir IMAGE=$IMAGE"
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


lastTag=`git for-each-ref --format='%(*creatordate:raw)%(creatordate:raw) %(refname)' refs/tags | sort -nr | grep "refs/tags/${IMAGE}_" | head -1`
if [ -z "$lastTag" ]
then
  echo "Не найдены теги для образа $IMAGE. Теги дожны иметь вид $IMAGE_[ВерсияCервиса-]ВерсияСборки"
  exit 2
fi

set -- $lastTag
lastTag=$3
lastTag=${lastTag:10}
git checkout $lastTag >/dev/null 2>&1


IFS=_
set -- $lastTag
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



