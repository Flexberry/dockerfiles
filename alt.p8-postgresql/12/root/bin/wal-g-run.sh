#!/bin/bash
set -e

confFile= /etc/wal-g.d/server-"$walStorageMode".conf

if [ ! -f $confFile ]
then
  echo "Storage configuration file $confFile does not exist"
  exit 1
fi

source $confFile

/bin/wal-g $1 $2 $3 $4 $5
