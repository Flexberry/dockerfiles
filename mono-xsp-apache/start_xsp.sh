#!/bin/sh
set -x
rm -f /var/run/apache2/apache2.pid;
if [ -z "$MODULES" ]
then
  MODULES="proxy_html rewrite ssl deflate filter"
fi

for module in $MODULES
do
  a2enmod $module
done


rm -f /var/run/apache2/apache2.pid
service apache2 start


xsp4 --port 81 --nonstop 2>&1
