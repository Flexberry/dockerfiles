FROM flexberry/alt.p8-mono4

ENV LANG ru_RU.UTF-8

MAINTAINER mail@flexberry.net

RUN   bzip2 -d /var/lib/rpm/*.bz2; \
  apt-get update && \
  apt-get install -y mc htop && \
  rm -f /var/cache/apt/archives/*.rpm /var/cache/apt/*.bin /var/lib/apt/lists/*.*; \
  bzip2 -9 /var/lib/rpm/*  

CMD /bin/bash
