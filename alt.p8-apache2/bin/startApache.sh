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

# run bootup in foreground.
exec /bin/bootupApache.sh &

strace -ff /usr/sbin/httpd2 -D NO_DETACH -k start 2>/tmp/apache.log