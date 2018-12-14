FROM flexberry/alt.p8

LABEL description="Parent image for all java images"

RUN \
  bzip2 -d /var/lib/rpm/*bz2; \
  apt-get update; \
  apt-get install -y java-1.8.0-openjdk; \
  rm -f /var/cache/apt/archives/*.rpm /var/cache/apt/*.bin /var/lib/apt/lists/*.*; \
  bzip2 -9 /var/lib/rpm/*;
