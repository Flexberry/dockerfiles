#!/bin/sh
export REPO=flexberry/github-generation-environment

docker build --no-cache -t $REPO:latest .