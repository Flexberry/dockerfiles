#!/bin/sh

IMAGE_NAME="flexberry/mono"
IMAGE_TAG="${1:=latest}"

# Build the container image.
echo "Building image $IMAGE_NAME:$IMAGE_TAG."
docker build --build-arg MONO_VERSION=$1 --rm -t $IMAGE_NAME:$IMAGE_TAG .
echo "Build done"
