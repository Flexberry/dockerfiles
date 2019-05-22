#!/bin/sh

isKey() {
  attr=$1
  ifs="$IFS"
  IFS=,
  set -- $2
  IFS="$ifs"
  for key
  do
    if [ "$attr" = "$key" ]
    then
      return 0
    fi
  done
  return 1
}

writeAttr() {
  notKeyTag=$1
  str=$2
  IFS=`echo  "\t"`
  #echo $str
  set -- $str
  attr=$1
  if isKey $attr $keys
  then
    if [ -n "$notKeyTag" ]
    then
      continue
    fi
  else
   if [ -z "$notKeyTag" ]
    then
      continue
    fi
  fi
  isnull=$3
  if [ -z "$notKeyTag" ]
  then
    isnull=''
  else
    isnull='YES'
  fi
  type=$4
  prec=$5
  echo "\t\t\t<attribute>"
  echo "\t\t\t\t<name>$attr</name>"
  ifs="$IFS"
  IFS=' '
  set -- $type
  IFS="$ifs"
  type0=$1
  if [ $type0 = 'timestamp' ]
  then
    echo "\t\t\t\t<type>DateTime</type>"
    if [ "$isnull" = 'YES' ]
    then
      echo "\t\t\t\t<null_value>0000-00-00 00:00:00</null_value>"
    fi
  fi
  if [ $type0 = 'character' ]
  then
    echo "\t\t\t\t<type>String</type>"
    if [ "$isnull" = 'YES' ]
    then
      echo "\t\t\t\t<null_value></null_value>"
    fi
  fi
  if [ $type0 = 'uuid' ]
  then
    echo "\t\t\t\t<type>UUID</type>"
    if [ "$isnull" = 'YES' ]
    then
      echo "\t\t\t\t<null_value>00000000-0000-0000-0000-000000000000</null_value>"
    fi
  fi
  if [ $type0 = 'integer' ]
  then
    echo "\t\t\t\t<type>Int$prec</type>"
    if [ "$isnull" = 'YES' ]
    then
      echo "\t\t\t\t<null_value>-1</null_value>"
    fi
  fi
  if [ $type0 = 'boolean' ]
  then
    echo "\t\t\t\t<type>Int8</type>"
    if [ "$isnull" = 'YES' ]
    then
      echo "\t\t\t\t<null_value>0</null_value>"
    fi
  fi
  if [ $type0 = 'numeric' ]
  then
    echo "\t\t\t\t<type>Float64</type>"
    if [ "$isnull" = 'YES' ]
    then
      echo "\t\t\t\t<null_value>0.0</null_value>"
    fi
  fi
  echo "\t\t\t</attribute>"
}

dictXML() {
  dict=$1
  keys=$2
  if [ -z "$keys" ]
  then
    keys='primarykey'
  fi
  refresh=$3
  echo "\t<dictionary>"
  echo "\t\t<name>$dict</name>"
  echo "\t\t<source>"
  echo "\t\t\t<odbc>"
  echo "\t\t\t\t<connection_string>DSN=postgresConnection</connection_string>"
  echo "\t\t\t\t<table>$dict</table>"
  echo "\t\t\t</odbc>"
  echo "\t\t</source>"
  echo "\t\t<layout>"
  echo "\t\t\t<complex_key_hashed />"
  echo "\t\t</layout>"
  echo "\t\t<structure>"

  TMPFile=/tmp/setDict.$$

  until isql postgresConnection </dev/null >/dev/null 2>&1; do sleep 5; done

  echo "SELECT column_name,ordinal_position,is_nullable,data_type,numeric_precision FROM information_schema.COLUMNS  WHERE table_catalog='${PGDatabase}' AND table_name='$dict' ORDER BY ordinal_position;" |
  isql postgresConnection -b -x0x09  >$TMPFile

  echo "\t\t\t<key>"
  while read str
  do
    writeAttr ""  "$str"
  done <$TMPFile

  echo "\t\t\t</key>"

  while read str
  do
    writeAttr "Y"  "$str"
  done <$TMPFile

  echo "\t\t</structure>"
  echo "\t\t<lifetime>300</lifetime>"
  echo "\t</dictionary>"
  echo
}

(
echo "<?xml version='1.0' encoding='UTF-8'?>"
echo "<yandex>"
ifs="$IFS"

for dictDesc in $PGDicrionaries
do
  IFS=/
  set -- $dictDesc
  IFS="$ifs"
  dictXML $*
done
echo "</yandex>"
) > /etc/clickhouse-server/${PGDatabase}_dictionary.xml

