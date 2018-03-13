#!/bin/sh
set -x

if [ -z "$POSTGRES_logging_collector" ]; then POSTGRES_logging_collector=on; fi
if [ -z "$POSTGRES_log_directory" ]; then POSTGRES_log_directory=pg_log; fi
if [ -z "$POSTGRES_log_filename" ]; then POSTGRES_log_filename=\'postgresql-%u_%H.log\'; fi
if [ -z "$POSTGRES_log_rotation_age" ]; then POSTGRES_log_rotation_age=1h ;fi
if [ -z "$POSTGRES_log_rotation_size" ]; then POSTGRES_log_rotation_size=0; fi

for confVar in `echo ${!POSTGRES*}` 
do
  confValue=${!confVar}
  key=${confVar:9}
  if [ "$confValue" ]
  then
    sedExp="$sedExp -e s|#$key.*=.*|$key.*=.*| -e s|^$key.*=.*|$key=$confValue|"
  fi
done
#echo "$sedExp"
ErrFile=/tmp/sed.log
ConfFile=/var/lib/pgsql/data/postgresql.conf
TmpConfFile=/tmp/postgresql.conf
sed $sedExp <$ConfFile >$TmpConfFile 2>$ErrFile;

if [ -s $ErrFile ]
then
  cat $ErrFile
else
  mv $TmpConfFile $ConfFile
fi

exec su -c '/usr/bin/postgres -D /var/lib/pgsql/data' -s /bin/sh postgres


