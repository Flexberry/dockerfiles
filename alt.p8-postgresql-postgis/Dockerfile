FROM flexberry/alt.p8-postgresql:9.6-1.1.1

MAINTAINER mail@flexberry.ru

USER root

RUN \
  bzip2 -d /var/lib/rpm/*.bz2; \
  apt-get -y update && \
  apt-get install -y postgresql-postgis; \
  rm -f /var/cache/apt/archives/*.rpm /var/cache/apt/*.bin /var/lib/apt/lists/*.*; \
  bzip2 -9 /var/lib/rpm/*

EXPOSE 5432
