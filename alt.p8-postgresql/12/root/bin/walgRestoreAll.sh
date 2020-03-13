#!/bin/sh
set -x
exec  2>/tmp/walgRestoreAll.log
set >&2

. /etc/wal-g.d/server.conf
. /etc/wal-g.d/server-$WALG.conf
if backup-list.sh >/dev/null
then
  cd /var/lib/pgsql/data
  rm -rf *
  su -c backup-fetch.sh -s /bin/sh postgres
  su -c 'touch /var/lib/pgsql/data/recovery.signal' -s /bin/sh postgres

  POSTGRES_PARAMS="-c restore_command='/bin/wal-fetch.sh %f %p' -c recovery_target_timeline=latest"
  su -c "/usr/bin/postgres -D /var/lib/pgsql/data $POSTGRES_PARAMS"  -s /bin/sh postgres &

  sleep 5; \
  while [ -f /var/lib/pgsql/data/recovery.signal ]; do sleep 5; done ; \

  /etc/init.d/postgresql stop; \
  sleep 5; \
  while [ -f /var/lib/pgsql/data/postmaster.pid ]; do sleep 5; done; \
  killall postgres; \
  while [ -f /tmp/.s.PGSQL.5432.lock ]; do sleep 5; done;
else
  echo "Режим бекапа BACKUP_WALG: В архиве нет данных для восстановления";
fi
