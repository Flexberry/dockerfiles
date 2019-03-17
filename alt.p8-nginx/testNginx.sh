#!/bin/sh
# until /usr/bin/wget http://localhost; do sleep 10; done
/etc/init.d/nginx start;
sleep 10;
/usr/bin/wget http://localhost;

