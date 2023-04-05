FROM flexberry/alt.p8-postgresql

MAINTAINER mail@flexberry.ru

USER root


RUN \
  bzip2 -d /var/lib/rpm/*.bz2; \
  apt-get -y update

RUN \
  apt-get install -y libgeos libxml2 libproj libgdal libprotobuf-c postgresql-postgis; \
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


