#!/bin/sh

set -x
exec  2>/tmp/dumpRestoreAll.log
set >&2

/etc/init.d/postgresql start; \
sleep 5; \
until echo  "\l" |  psql -U postgres; do sleep 5; done ; \

export PGPASSWORD="$RESTORE_PASSWORD"
pg_dumpall -h $RESTORE_HOST -p $RESTORE_PORT -U $RESTORE_USER --clean --if-exists |
grep -v 'CREATE ROLE postgres
DROP ROLE IF EXISTS postgres
ALTER ROLE postgres' |
psql -U postgres

/etc/init.d/postgresql stop; \
sleep 5; \
while [ -f /var/lib/pgsql/data/postmaster.pid ]; do sleep 5; done; \
killall postgres; \
while [ -f /tmp/.s.PGSQL.5432.lock ]; do sleep 5; done;
