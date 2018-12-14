FROM flexberry/alt.p8

LABEL description="Image for compiling source codes for multistage build"

#Basics
RUN \
  if [ -f /var/lib/rpm/Packages.bz2 ]; then bzip2 -d /var/lib/rpm/*bz2; apt-get update; fi ;\
  apt-get install -y time vim-console git;

#PHP
RUN \
  if [ -f /var/lib/rpm/Packages.bz2 ]; then bzip2 -d /var/lib/rpm/*bz2; apt-get update; fi ;\
  apt-get install -y  php5 pear php5-devel composer;


#Python
RUN \
  if [ -f /var/lib/rpm/Packages.bz2 ]; then bzip2 -d /var/lib/rpm/*bz2; apt-get update; fi ;\
  apt-get install -y  python python-module-pip;


#NPM
RUN \
  if [ -f /var/lib/rpm/Packages.bz2 ]; then bzip2 -d /var/lib/rpm/*bz2; apt-get update; fi ;\
  apt-get install -y npm;


# C, C++
RUN \
  if [ -f /var/lib/rpm/Packages.bz2 ]; then bzip2 -d /var/lib/rpm/*bz2; apt-get update; fi ;\
  apt-get install -y gcc5 libssl-devel make \
    gcc5-c++ libevent-devel cmake ctest zlib-devel;

#Java
RUN \
  if [ -f /var/lib/rpm/Packages.bz2 ]; then bzip2 -d /var/lib/rpm/*bz2; apt-get update; fi ;\
  apt-get install -y  java-1.8.0-openjdk-devel maven;


RUN \
  rm -f /var/cache/apt/archives/*.rpm /var/cache/apt/*.bin /var/lib/apt/lists/*.*; \
  bzip2 -9 /var/lib/rpm/*;
