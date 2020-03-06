FROM flexberry/alt.p8-dev:1.0.2 AS dev

RUN \
  cd /opt; git clone https://github.com/wal-g/wal-g;

RUN \
  bzip2 -d /var/lib/rpm/*bz2; apt-get update; apt-get install -y liblzo2-devel-static;

RUN \
  cd /opt/wal-g/; \
  make install && make deps && make pg_build


FROM flexberry/alt.p8
ENV LANG ru_RU.UTF-8

MAINTAINER mail@flexberry.ru

RUN export http_proxy=$http_proxy;\
  bzip2 -d /var/lib/rpm/*.bz2; \
  echo "rpm http://repo.postgrespro.ru/pg_probackup/rpm/latest/altlinux-p8 x86_64 vanilla" > /etc/apt/sources.list.d/pg_probackup.list; \
  apt-get update && \
  apt-get install -y postgresql12-server postgresql12-contrib pg_probackup-12 && \
  rm -f /var/cache/apt/archives/*.rpm /var/cache/apt/*.bin /var/lib/apt/lists/*.*; \
  bzip2 -9 /var/lib/rpm/*

RUN  /etc/init.d/postgresql initdb;

COPY data/postgresql.conf data/pg_hba.conf /var/lib/pgsql/data/
COPY /docker-cmd.sh /

RUN  chmod +x /docker-cmd.sh; \
  chown postgres:postgres /var/lib/pgsql/data/postgresql.conf /var/lib/pgsql/data/pg_hba.conf; \
  /etc/init.d/postgresql start; \
  sleep 5; \
  until echo  "\l" |  psql -U postgres; do sleep 5; done ; \
  echo  -ne "ALTER ROLE postgres WITH PASSWORD 'p@ssw0rd';" |  psql -U postgres; \
  /etc/init.d/postgresql stop; \
  sleep 5; \
  while [ -f /var/lib/pgsql/data/postmaster.pid ]; do sleep 5; done; \
  killall postgres; \
  while [ -f /tmp/.s.PGSQL.5432.lock ]; do sleep 5; done;

COPY --from=dev /opt/wal-g/main/pg/wal-g /bin/
# COPY opt/wal-g/main/pg/wal-g /bin/
COPY /root/ /

EXPOSE 5432

CMD /docker-cmd.sh


