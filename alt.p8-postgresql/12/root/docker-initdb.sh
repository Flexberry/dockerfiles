#!/bin/sh

/etc/init.d/postgresql start; \
sleep 5; \
until echo  "\l" |  psql -U postgres; do sleep 5; done ; \
for sqlFile in /docker-initdb.d/*.sql; \
do \
  echo "Process $sqlFile"; \
  psql -U postgres < $sqlFile; \
done ; \
/etc/init.d/postgresql stop; \
sleep 5; \
while [ -f /var/lib/pgsql/data/postmaster.pid ]; do sleep 5; done; \
killall postgres; \
while [ -f /tmp/.s.PGSQL.5432.lock ]; do sleep 5; done;
