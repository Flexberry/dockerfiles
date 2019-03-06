#!/bin/bash

IMAGE_NAME="flexberry/pentaho"
IMAGE_TAG="8.2"

# Build the container image.
echo "Building the Docker container for $IMAGE_NAME:$IMAGE_TAG.."
docker build --rm -t $IMAGE_NAME:$IMAGE_TAG .
echo "Build done"
