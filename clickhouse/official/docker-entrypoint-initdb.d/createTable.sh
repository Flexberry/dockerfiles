#!/bin/sh

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
    if [ $type0 = 'timestamp' ]
    then
      echo "\"$attr\" DateTime"
    fi
    if [ $type0 = 'character' ]
    then
      echo "\"$attr\" String"
    fi
    if [ $type0 = 'uuid' ]
    then
      echo "\"$attr\" UUID"
    fi
    if [ $type0 = 'integer' ]
    then
      echo "\"$attr\" Int"$prec 
    fi
    if [ $type0 = 'boolean' ]
    then
      echo "\"$attr\" Int8" 
    fi
    if [ $type0 = 'numeric' ]
    then
      echo "\"$attr\" Float64" 
    fi
  done
  echo ")  Engine = Dictionary(\"$table\");"
}

createTable $1 $2
