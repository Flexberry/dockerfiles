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
  apt-get install -y postgresql9.6-devel protobuf-c-compiler libprotobuf-devel libprotobuf-c-devel  libxml2-devel libgeos-devel libproj-devel libgdal-devel bison; 
  
RUN \  
  cd /root/; \
  wget -c http://postgis.net/stuff/postgis-2.5.4.tar.gz; \
  tar xvzf postgis-2.5.4.tar.gz
  
  
RUN \    
  cd /root/postgis-2.5.4; \
  ./autogen.sh; \
  ./configure --with-pgconfig=/usr/bin/pg_config; \
  make; make check; make install; \
  tar cvzf /tmp/postgis.tgz /usr/local/lib /usr/lib64/pgsql /usr/share/pgsql/extension/ /usr/share/pgsql/contrib/postgis-2.5 /usr/bin/raster2pgsql /usr/bin/pgsql2shp /usr/bin/shp2pgsql; \
  mkdir /dockerLayer; cd /dockerLayer;\
  tar xvfz /tmp/postgis.tgz; \
  mv ./usr/local/lib ./usr/lib64; 
  
  
FROM flexberry/alt.p8-postgresql 

MAINTAINER mail@flexberry.ru

USER root

COPY --from=dev /dockerLayer /

RUN \
  bzip2 -d /var/lib/rpm/*.bz2; \
  apt-get -y update && \
  apt-get install -y libgeos libxml2 libproj libgdal libprotobuf-c; \
  /etc/init.d/postgresql start; \
  rm -f /var/lib/pgsql/data/pg_log/*; \
  sleep 5; \
  until echo  "\l" |  psql -U postgres; do sleep 5; done ; \
  echo  -ne "CREATE EXTENSION postgis;\n" |  psql -U postgres; \
  /etc/init.d/postgresql stop; \
  sleep 5; \
  while [ -f /var/lib/pgsql/data/postmaster.pid ]; do sleep 5; done; \
  killall postgres; \
  while [ -f /tmp/.s.PGSQL.5432.lock ]; do sleep 5; done;


