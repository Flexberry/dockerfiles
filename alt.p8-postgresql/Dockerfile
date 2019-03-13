FROM flexberry/alt.p8-dev as dev
RUN \
  cd root/; \
  git clone https://github.com/tds-fdw/tds_fdw.git; \
  cd tds_fdw/; \
  bzip2 -d /var/lib/rpm/*; \
  apt-get update; \
  apt-get install -y postgresql9.6-devel libfreetds-devel libfreetds; \
  make USE_PGXS=1; \
  make USE_PGXS=1 install; \
  tar cvzf /tmp/tds_fdw.tgz \
    /usr/lib64/pgsql/tds_fdw.so \
    /usr/share/pgsql/extension/tds_fdw.control \
    /usr/share/pgsql/extension/tds_fdw--2.0.0-alpha.3.sql \
    /usr/share/doc/postgresql/extension/README.tds_fdw.md \
    /etc/freetds/freetds.conf \
    /etc/freetds/locales.conf \
    /usr/lib64/libct.so.4 \
    /usr/lib64/libct.so.4.0.0 \
    /usr/lib64/libsybdb.so.5 \
    /usr/lib64/libsybdb.so.5.0.0


FROM flexberry/alt.p8
ENV LANG ru_RU.UTF-8

MAINTAINER mail@flexberry.ru

RUN export http_proxy=$http_proxy;\
  bzip2 -d /var/lib/rpm/*.bz2; \
  apt-get update && \
  apt-get install -y postgresql9.6-server postgresql9.6-contrib && \
  rm -f /var/cache/apt/archives/*.rpm /var/cache/apt/*.bin /var/lib/apt/lists/*.*; \
  bzip2 -9 /var/lib/rpm/*

RUN  /etc/init.d/postgresql initdb;

COPY data/postgresql.conf data/pg_hba.conf /var/lib/pgsql/data/
COPY /docker-cmd.sh /
COPY --from=dev /tmp/tds_fdw.tgz /tmp

RUN  chmod +x /docker-cmd.sh; \
  cd /; tar xvzf /tmp/tds_fdw.tgz; \
  chown postgres:postgres /var/lib/pgsql/data/postgresql.conf /var/lib/pgsql/data/pg_hba.conf; \
  /etc/init.d/postgresql start; \
  sleep 5; \
  until echo  "\l" |  psql -U postgres; do sleep 5; done ; \
  echo  -ne "ALTER ROLE postgres WITH PASSWORD 'p@ssw0rd';\nCREATE EXTENSION tds_fdw;\n" |  psql -U postgres; \
  /etc/init.d/postgresql stop; \
  sleep 5; \
  while [ -f /var/lib/pgsql/data/postmaster.pid ]; do sleep 5; done; \
  killall postgres; \
  while [ -f /tmp/.s.PGSQL.5432.lock ]; do sleep 5; done;


EXPOSE 5432

CMD /docker-cmd.sh


