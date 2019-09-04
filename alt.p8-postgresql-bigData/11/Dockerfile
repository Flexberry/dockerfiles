FROM flexberry/alt.p8-dev as dev

ENV REPODATA=/2019/08/27/

RUN   bzip2 -d /var/lib/rpm/*.bz2; \
  apt-get update; \
  apt-get install -y apt-repo; \
  apt-repo rm all;  \
  apt-repo add http://ftp.altlinux.org/pub/distributions/archive/p8/date/$REPODATA x86_64-i586 classic; \
  apt-repo add http://ftp.altlinux.org/pub/distributions/archive/p8/date/$REPODATA x86_64 classic; \
  apt-repo add http://ftp.altlinux.org/pub/distributions/archive/p8/date/$REPODATA noarch classic; \
  apt-get update; \
  apt-get install -y postgresql11-devel ; 

  
RUN  cd /root/; \
  git clone https://github.com/postgrespro/pg_pathman; \
  apt-get install -y postgresql11-devel; \
  cd /root/pg_pathman; \
  make install USE_PGXS=1; \
  
RUN  cd /; tar cvzf /tmp/pg_pathman.tgz \
    /usr/lib64/pgsql/pg_pathman.so \
    /usr/share/pgsql/extension/pg_pathman.control \
    /usr/share/pgsql/extension/pg_pathman--*sql; \
  mkdir /dockerLayer; cd /dockerLayer; \
  tar xvzf /tmp/pg_pathman.tgz  
  
  
RUN  cd /root/; \
  git clone https://github.com/timescale/timescaledb; 
  
RUN  cd /root/timescaledb; \
  apt-get install -y postgresql11-devel; \
  git checkout 1.4.1; \
  ./bootstrap; \
  cd build; make; \
  make install; 
  
RUN  \
  cd /; tar cvzf /tmp/timescaledb.tgz \
  /usr/share/pgsql/extension/timescaledb.control \
  /usr/share/pgsql/extension/timescaledb--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--1.4.0--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--1.4.0--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--1.3.2--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--1.3.1--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--1.3.0--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--1.2.2--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--1.2.1--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--1.2.0--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--1.1.1--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--1.1.0--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--1.0.1--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--1.0.0--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--1.0.0-rc3--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--1.0.0-rc2--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--1.0.0-rc1--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.12.1--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.12.0--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.11.0--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.10.1--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.10.0--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.9.2--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.9.1--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.9.0--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.8.0--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.7.1--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.7.0--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.6.1--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.6.0--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.5.0--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.4.2--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.4.1--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.4.0--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.3.0--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.2.0--1.4.1.sql \
  /usr/share/pgsql/extension/timescaledb--0.1.0--1.4.1.sql \
  /usr/lib64/pgsql/timescaledb-1.4.1.so \
  /usr/lib64/pgsql/timescaledb.so \
  /usr/lib64/pgsql/timescaledb-tsl-1.4.1.so; \
  mkdir /dockerLayer; cd /dockerLayer; \
  tar xvzf /tmp/timescaledb.tgz
  
RUN \  
  apt-get install -y postgresql11-devel libfreetds-devel libfreetds; \
  cd root/; \
  git clone https://github.com/tds-fdw/tds_fdw.git; \
  cd tds_fdw/; \
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
    /usr/lib64/libsybdb.so.5.0.0; \
    mkdir /dockerLayer; cd /dockerLayer; \
    tar xvzf /tmp/tds_fdw.tgz

RUN \
  cd /root/; \
  git clone git://github.com/Percona-Lab/clickhousedb_fdw.git;

RUN \
  apt-get install -y postgresql11-devel libunixODBC-devel libltdl3-devel libunixODBC-devel libpoco-devel  libstdc++5-devel-static


RUN \
  cd /root; \
  git clone --recursive https://github.com/yandex/clickhouse-odbc; \
  cd clickhouse-odbc; \
  mkdir -p build; cd build && cmake .. && make -j $(nproc || sysctl -n hw.ncpu || echo 4); \
  make install


RUN \
  cp /usr/local/lib64/odbc/libclickhouseodbc.so /usr/lib64/clickhouse-1.0.so ; \
  cd /root/clickhousedb_fdw; \
  make -f lib/Makefile; \
  make USE_PGXS=1 install

RUN \
  tar cvzf /tmp/clickhouse_fdw.tgz \
    /usr/lib64/libclickhouse-1.0.so \
     /usr/lib64/libodbc.so.2.0.0 \
     /usr/lib64/libodbc.so.2 \
     /usr/lib64/libodbc.so \
     /usr/lib64/libltdl.so.7* \
    /usr/lib64/pgsql/clickhousedb_fdw.so \
    /usr/share/pgsql/extension/clickhousedb_fdw.control \
    /usr/share/pgsql/extension/clickhousedb_fdw--1.0.sql;

RUN cd /dockerLayer; \
    tar xvzf /tmp/clickhouse_fdw.tgz



FROM flexberry/alt.p8
ENV LANG ru_RU.UTF-8

MAINTAINER mail@flexberry.ru

RUN export http_proxy=$http_proxy;\
  bzip2 -d /var/lib/rpm/*.bz2; \
  apt-get update && \
  apt-get install -y postgresql11-server postgresql11-contrib && \
  rm -f /var/cache/apt/archives/*.rpm /var/cache/apt/*.bin /var/lib/apt/lists/*.*; \
  bzip2 -9 /var/lib/rpm/*

RUN  /etc/init.d/postgresql initdb;

COPY data/postgresql.conf data/pg_hba.conf /var/lib/pgsql/data/
COPY /docker-cmd.sh /
COPY --from=dev /dockerLayer /

RUN  chmod +x /docker-cmd.sh; \
  chown postgres:postgres /var/lib/pgsql/data/postgresql.conf /var/lib/pgsql/data/pg_hba.conf; \
  /etc/init.d/postgresql start; \
  sleep 5; \
  until echo  "\l" |  psql -U postgres; do sleep 5; done ; \
  echo  -ne "ALTER ROLE postgres WITH PASSWORD 'p@ssw0rd';\nCREATE EXTENSION tds_fdw;\nCREATE EXTENSION clickhousedb_fdw;\nCREATE EXTENSION timescaledb;\n\nCREATE EXTENSION pg_pathman;\n" |  psql -U postgres; \
  /etc/init.d/postgresql stop; \
  sleep 5; \
  while [ -f /var/lib/pgsql/data/postmaster.pid ]; do sleep 5; done; \
  killall postgres; \
  while [ -f /tmp/.s.PGSQL.5432.lock ]; do sleep 5; done;


EXPOSE 5432

CMD /docker-cmd.sh


