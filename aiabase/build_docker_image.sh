#!/bin/sh
export REPO=flexberry/aiabase

docker build --no-cache -t $REPO:latest .