#!/bin/sh
set -x
rm -f /var/run/httpd2/httpd.pid;
if [ -z "$MODULES" ]
then
  MODULES="rewrite ssl deflate filter"
fi

for module in $MODULES
do
  a2enmod $module
done

/usr/sbin/httpd2 -D NO_DETACH -k start

if [ -n  "$BOOTUPCHECKURL" ]
then
  until wget -c $BOOTUPCHECKURL >/dev/null  2>&1
  do
    echo "Wait for start up apache service"
    sleep 1;
  done
fi
