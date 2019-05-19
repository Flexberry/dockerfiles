#!/bin/sh

dictXML() {
  table=$1
  echo "\t<dictionary>"
  echo "\t\t<name>$table</name>"
  echo "\t\t<source>"
  echo "\t\t\t<odbc>"
  echo "\t\t\t\t<connection_string>DSN=postgresConnection</connection_string>"
  echo "\t\t\t\t<table>$table</table>"
  echo "\t\t\t</odbc>"
  echo "\t\t</source>"
  echo "\t\t<layout>"
  echo "\t\t\t<complex_key_hashed />"
  echo "\t\t</layout>"
  echo "\t\t<structure>"
  echo "\t\t\t<key>"
  echo "\t\t\t\t<attribute>"
  echo "\t\t\t\t\t<name>primarykey</name>"
  echo "\t\t\t\t\t<type>UUID</type>"
  echo "\t\t\t\t</attribute>"
  echo "\t\t\t</key>"



  echo "SELECT column_name,ordinal_position,is_nullable,data_type,numeric_precision FROM information_schema.COLUMNS  WHERE table_catalog='БезопасныйГород'  AND table_name='$table' ORDER BY ordinal_position;" |
  isql postgresConnection -b -x0x09 |
  while read str
  do
    IFS=`echo  "\t"`
    #echo $str
    set -- $str
    attr=$1
    if [ $attr = 'primarykey' ]
    then
      continue
    fi
    isnull=$3
    type=$4
    prec=$5
    echo "\t\t\t<attribute>"
    echo "\t\t\t\t<name>$attr</name>"
    IFS=' '
    set -- $type
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
  done
  echo "\t\t</structure>"
  echo "\t</dictionary>"
  echo 
}

echo "<?xml version='1.0' encoding='UTF-8'?>"
echo "<yandex>"
for dict 
do
  dictXML $dict
done
echo "</yandex>"


