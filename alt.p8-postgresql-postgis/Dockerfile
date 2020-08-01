FROM flexberry/alt.p8-dev as dev

ENV REPODATA=2019/05/17

RUN \
  bzip2 -d /var/lib/rpm/*.bz2; \
  apt-get update; \
  apt-get install -y apt-repo;
  
  
RUN \
  apt-repo rm all;  \
  apt-repo add http://ftp.altlinux.org/pub/distributions/archive/p8/date/$REPODATA x86_64-i586 classic; \
  apt-repo add http://ftp.altlinux.org/pub/distributions/archive/p8/date/$REPODATA x86_64 classic; \
  apt-repo add http://ftp.altlinux.org/pub/distributions/archive/p8/date/$REPODATA noarch classic; \
  apt-get update; \
  apt-get install -y postgresql9.6-devel libxml2-devel libgeos-devel libproj-devel libgdal-devel bison; 
  
RUN \  
  cd /root/; \
  git clone https://github.com/postgis/postgis; 
  
RUN \    
  cd /root/postgis; \
  git checkout stable-2.5; \
  ./autogen.sh; ./configure; \
  make; make check; make install; \
  tar cvzf /tmp/postgis.tgz /usr/local/lib /usr/lib64/pgsql /usr/share/pgsql/contrib/postgis-2.5 /usr/bin/raster2pgsql /usr/bin/pgsql2shp /usr/bin/shp2pgsql; \
  mkdir /dockerLayer; cd /dockerLayer;\
  tar xvfz /tmp/postgis.tgz; \
  mv ./usr/local/lib ./usr/lib64; 
  
  
FROM flexberry/alt.p8-postgresql 
# :9.6-1.1.1

MAINTAINER mail@flexberry.ru

USER root

COPY --from=dev /dockerLayer /

#RUN \
#  bzip2 -d /var/lib/rpm/*.bz2; \
#  apt-get -y update && \
#  apt-get install -y postgresql-postgis; \
#  rm -f /var/cache/apt/archives/*.rpm /var/cache/apt/*.bin /var/lib/apt/lists/*.*; \
#  bzip2 -9 /var/lib/rpm/*

EXPOSE 5432
