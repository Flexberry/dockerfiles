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

POSTGRES_PARAMS=
if [ -n "$REPLICATIONMODE" ]
then
  case $REPLICATIONMODE in
    master)
        POSTGRES_PARAMS="-c synchronous_commit=local -c archive_mode=on -c wal_level=replica -c archive_command='cp %p /var/lib/pgsql/pg-archive/%f' -c max_wal_senders=5 -c wal_keep_segments=12 -c synchronous_standby_names=node1"
        echo "host replication replication 0.0.0.0/0 md5" >> /var/lib/pgsql/data/pg_hba.conf
        chown postgres:postgres /var/lib/pgsql/data/pg_hba.conf
      ;;
    slave | forceslave)
      if [ -z "$MASTERPORT" ]
      then
        MASTERPORT=5432
      fi
      POSTGRES_PARAMS='-c hot_standby=on'
      if [ ! -f /var/lib/pgsql/data/recovery.conf ]
      then
        rm -rf /var/lib/pgsql/data/* /var/lib/pgsql/data/.??*
        echo "$MASTERHOST:$MASTERPORT:*:replication:Hw572BbvG7g4cwq5" > /var/lib/pgsql/.pgpass
        chown -R postgres:postgres /var/lib/pgsql/.pgpass;
        chmod 600 /var/lib/pgsql/.pgpass;
        until su -c "export MASTERHOST=$MASTERHOST; export MASTERPORT=$MASTERPORT; pg_basebackup -w -h $MASTERHOST -U replication -D /var/lib/pgsql/data -P --xlog -p $MASTERPORT" -s /bin/sh postgres;
        do
          sleep 5
        done
        echo "
standby_mode=on
primary_conninfo = 'host=$MASTERHOST port=$MASTERPORT user=replication password=Hw572BbvG7g4cwq5 application_name=node1'
trigger_file='/tmp/postgresql.trigger.5432'
" > /var/lib/pgsql/data/recovery.conf
        chown postgres:postgres /var/lib/pgsql/data/recovery.conf
      fi
      ;;
  esac

fi

exec su -c "/usr/bin/postgres -D /var/lib/pgsql/data $POSTGRES_PARAMS" -s /bin/sh postgres


