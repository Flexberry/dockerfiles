#!/bin/bash
set -e
set -x
exec >>/var/lib/pgsql/data/pg_log/wall-fetch.log 2>&1
echo -ne "\n\n\--------------------------------"
date
confFile=/etc/wal-g.d/server-"$WALG".conf

if [ ! -f $confFile ]
then
  echo "Storage configuration file $confFile does not exist"
  exit 1
fi

. /etc/wal-g.d/server.conf
. $confFile

/bin/wal-g wal-fetch $1 $2
