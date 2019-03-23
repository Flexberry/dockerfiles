#!/bin/sh
export IMAGE_NAME
export ALIASES_NAMES

##СКРИПТ ПРОИЗВОДИТ ФОРМИРОВНИЕ ДОПОЛНИТЕЛНОГО СПИСКА АЛИАСОВ ОБРАЗА ALIASES_NAMES
setListImageAliases() {
  additionalTags=$*
  set -x
  ifs=$IFS
  IFS=:
  IFS=:/
  set -- $IMAGE_NAME
  IFS=$ifs
  IMAGE="$2/$3"
  gitTag=$4
  IMAGE_NAME="$IMAGE:$gitTag"

  IFS=-
  set -- $gitTag
  IFS=$ifs
  version=
  case $# in
    2)
      version=$1
      build=$2
      echo "Service version $version, build version $build"
      fullTag="${version}-"
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
  setListImageAliases;
  for image in $ALIASES_NAMES
  do
    docker tag $IMAGE_NAME $image
  done
}

pushAliases() {
  setListImageAliases;
  for image in $ALIASES_NAMES
  do
    docker push $image
  done  
}