#!/bin/sh

createDB() {
  db=$1
  echo "CREATE DATABASE IF NOT EXISTS \"$1\";"
}

createTable() {
  db=$1
  table=$2
  echo "CREATE TABLE \"$db\".\"$table\" ("
  echo "SELECT column_name,ordinal_position,is_nullable,data_type,numeric_precision FROM information_schema.COLUMNS  WHERE table_catalog='$db'  AND table_name='$table' ORDER BY ordinal_position;" |
  isql postgresConnection -b -x0x09 |
  while read str
  do
    if [ "$first" = 'yes' ]
    then
      echo ','
    else
      first='yes'
    fi
    IFS=`echo  "\t"`
    set -- $str
    attr=$1
    isnull=$3
    type=$4
    prec=$5
    IFS=' '
    set -- $type
    type0=$1
    echo -n "\"$attr\" "
    if [ $type0 = 'timestamp' ]
    then
      echo -n "DateTime"
    fi
    if [ $type0 = 'character' ]
    then
      echo -n "String"
    fi
    if [ $type0 = 'uuid' ]
    then
      echo -n "UUID"
    fi
    if [ $type0 = 'integer' ]
    then
      echo -n " Int"$prec
    fi
    if [ $type0 = 'boolean' ]
    then
      echo -n "Int8"
    fi
    if [ $type0 = 'numeric' ]
    then
      echo -n "Float64"
    fi
  done
  echo ")  Engine = Dictionary(\"$table\");"
}

set -x
exec 2>/tmp/createDictionaries.log

(
until echo "show databases" | clickhouse-client >/dev/null; do sleep 5; done

while echo "select last_exception  from system.dictionaries" | clickhouse-client | grep Exception; do sleep 0.2; echo "system reload dictionaries" | clickhouse-client; done

createDB ${PGDatabase} | clickhouse-client
for dictDesc in $PGDicrionaries
do
  IFS=/
  set -- $dictDesc
  IFS="$ifs"
  dict=$1
  createTable ${PGDatabase} $dict | clickhouse-client -d ${PGDatabase}
done
) &
