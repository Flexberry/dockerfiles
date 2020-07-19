#!/bin/bash

set -x
if [ -z "$Services" ]
then
  Services='car foot bicycle'
fi
if [ -z "$OSMREG" ]
then
  OSMREG=RU-PER
fi

OSMREGDIR=/data/$OSMREG
if [ ! -d $OSMREGDIR ]
then
  mkdir -p $OSMREGDIR
fi

cd $OSMREGDIR
PBFFile=$OSMREG.osm.pbf
if [ ! -f $PBFFile ]
then 
  wget -c http://osm.sbin.ru/osm_dump/$PBFFile
fi 

for service in $Services
do
  if [ ! -d $service ]
  then
    mkdir -p $service
    cd $service
    ln -sf ../$PBFFile .
    osrm-extract -p /opt/$service.lua ./$OSMREG.osm.pbf
    osrm-partition ./$OSMREG.osrm 
    osrm-customize ./$OSMREG.osrm
    cd ..
  fi
done

cd /

port=5000
for service in $Services
do
  osrm-routed -p $port --algorithm mld $OSMREGDIR/$service/$OSMREG.osrm &
  let port=$port+1
done

while :;
do
  sleep 86400
done
