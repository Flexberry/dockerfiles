#!/bin/sh
# From the list of database names specified in the parameters, the script lists the missing databases.
export PGPASSWORD=$DB_ADMIN_PASS
echo -ne "Wait connection to database  $DB_ADMIN $DB_HOST:$DB_PORT..."
until psql -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT </dev/null ; do sleep 1; done
echo
list=`psql -lqt -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT`
ret=''
for db
do
  l=`echo $list | grep $db`
  if [ -z "$l" ]
  then
    ret="$ret $db"
  fi
done
echo $ret
