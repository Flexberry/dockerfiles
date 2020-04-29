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

if [ -n  "$BOOTUP_CHECK_URL" ]
then
  echo "Booting up apache service $BOOTUP_CHECK_URL"
  until wget -c $BOOTUP_CHECK_URL >/dev/null  2>&1
  do
    echo "Wait for start up apache service"
    sleep 0.1;
  done
  echo "Booted up apache service $BOOTUP_CHECK_URL"
else
  echo "Skipped booting up"
fi &

/usr/sbin/httpd2 -D NO_DETACH -k start 2>&1

