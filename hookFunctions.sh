#!/bin/sh
export IMAGE_NAME
export ALIASES_NAMES

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

# СКРИПТ РАЗБИВАЕТ DOCKER-ТЕГ На ЧАСТИ: version, build, prerelease
# version, build - опциональны и могут быть пустыми после анализа
parseDockerTag() {
  IFS=-
  set -- $1
  IFS=$ifs
  version=
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
      echo "Incorrect verison format in tag. Tags must have a look ${IMAGE}_[ServiceVersion-]BuildVersion"
      exit 3
  esac
}

##СКРИПТ ПРОИЗВОДИТ ФОРМИРОВНИЕ ДОПОЛНИТЕЛНОГО СПИСКА АЛИАСОВ ОБРАЗА ALIASES_NAMES
setListImageAliases() {
  additionalTags=$*
  ifs=$IFS
  IFS=:
  IFS=:/+
  set -- $IMAGE_NAME
  IFS=$ifs
  IMAGE="$2/$3"
#   if isPreRelease $4
#   then
#     ALIASES_NAMES=
#     return
#   fi
  IFS=+
  set -- $4
  IFS=$ifs
  dockerTag=$1
  meta=$2
  IMAGE_NAME="$IMAGE:$dockerTag"

  parseDockerTag $dockerTag

  latestImage="$IMAGE:latest"
  versionImage=
  if [ -n "$version" ]
  then
    versionImage="$IMAGE:$version"
  fi
  IFS=.
  set -- $build
  IFS=$ifs
  major=$1
  minor=$2
  patch=$3

  majorImage="$IMAGE:$fullTag$major"
  minorImage="$IMAGE:$fullTag$major.$minor"

  ALIASES_NAMES="$majorImage $minorImage $versionImage"
  for additionalTag in $additionalTags
  do
    ALIASES_NAMES="$ALIASES_NAMES $IMAGE:$additionalTag"
  done
}

setAliases() {
  setListImageAliases $*;
  for image in $ALIASES_NAMES
  do
    docker tag $IMAGE_NAME $image
  done
}

pushAliases() {
  setListImageAliases $*;
  for image in $ALIASES_NAMES
  do
    docker push $image
  done
}
