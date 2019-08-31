FROM flexberry/alt.p8-dev as dev

ENV REPODATA=2019/05/17

RUN \
  bzip2 -d /var/lib/rpm/*.bz2; \
  apt-get update; \
  apt-get install -y apt-repo; \
  apt-repo rm all;  \
  apt-repo add http://ftp.altlinux.org/pub/distributions/archive/p8/date/$REPODATA x86_64-i586 classic; \
  apt-repo add http://ftp.altlinux.org/pub/distributions/archive/p8/date/$REPODATA x86_64 classic; \
  apt-repo add http://ftp.altlinux.org/pub/distributions/archive/p8/date/$REPODATA noarch classic; \
  apt-get update; \
  apt-get install -y postgresql9.6-devel ; 
  
RUN  cd /root/; \
  git clone https://github.com/timescale/timescaledb; 
  
RUN  cd /root/timescaledb; \
  git checkout 1.4.1; \
  ./bootstrap; \
  cd build; make; \
  make install; 


  
RUN  cd /; tar cvzf /tmp/timescaledb.tgz \
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

FROM flexberry/alt.p8
ENV LANG ru_RU.UTF-8

MAINTAINER mail@flexberry.ru

RUN export http_proxy=$http_proxy;\
  bzip2 -d /var/lib/rpm/*.bz2; \
  apt-get update && \
  apt-get install -y postgresql9.6-contrib postgresql9.6-server && \
  rm -f /var/cache/apt/archives/*.rpm /var/cache/apt/*.bin /var/lib/apt/lists/*.*; \
  bzip2 -9 /var/lib/rpm/*

RUN  /etc/init.d/postgresql initdb;

COPY data/postgresql.conf data/pg_hba.conf /var/lib/pgsql/data/
COPY /docker-cmd.sh /
COPY --from=dev /dockerLayer /

#RUN  chmod +x /docker-cmd.sh; \
#  chown postgres:postgres /var/lib/pgsql/data/postgresql.conf /var/lib/pgsql/data/pg_hba.conf; \
#  /etc/init.d/postgresql start; \
#  sleep 5; \
#  until echo  "\l" |  psql -U postgres; do sleep 5; done ; \
#  echo  -ne "ALTER ROLE postgres WITH PASSWORD 'p@ssw0rd';\nCREATE EXTENSION tds_fdw;\n" |  psql -U postgres; \
#  /etc/init.d/postgresql stop; \
#  sleep 5; \
#  while [ -f /var/lib/pgsql/data/postmaster.pid ]; do sleep 5; done; \
#  killall postgres; \
#  while [ -f /tmp/.s.PGSQL.5432.lock ]; do sleep 5; done;


EXPOSE 5432

CMD /docker-cmd.sh


