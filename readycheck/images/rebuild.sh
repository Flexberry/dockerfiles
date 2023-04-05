#!/bin/sh

if [ $# -eq 0 ]
then
  set -- $(echo */Dockerfile)
fi
ifs=$IFS
for dockerfile
do
  dir=$(dirname $dockerfile)
  pushd $dir >/dev/null 2>&1
  ln -f ../readycheck ./readycheck
  popd >/dev/null 2>&1
  set -- $(grep FROM $dockerfile)
  image=$2
  IFS=/
  set -- $image
  IFS=$ifs
  Image=$1
  shift
  while [ $# -gt 1 ]
  do
    Image+="/$1"
    shift
  done
  echo -ne "\n\n\n----------------\nКаталог $dir образ $image:\n"
  Image+="/readycheck/$1"
  docker build --no-cache -t $Image $dir
  if ! docker run -it $Image curl --help >/dev/null 2>&1
  then
    echo -ne  "\n\n\n!!!!!!!!!!!!!!\nКаталог $dir образ $image: команда curl отсутсвует\n"
    continue
  fi

done
