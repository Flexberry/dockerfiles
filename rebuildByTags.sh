#!/bin/sh

for tag
do
  git push --delete origin $tag
  git tag -d $tag
  git tag $tag
  git push --tags
done
