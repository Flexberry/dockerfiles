#!/bin/bash
set -e
set -x
exec >>/tmp/wal-g-push.log 2>&1
echo -ne "\n\n\--------------------------------"
date
confFile=/etc/wal-g.d/server-"$WALG".conf

if [ ! -f $confFile ]
then
  echo "Storage configuration file $confFile does not exist"
  exit 1
fi

. $confFile

/bin/wal-g wal-push $1
exit 0
