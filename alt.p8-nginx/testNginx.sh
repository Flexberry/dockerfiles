#!/bin/sh
set -x
/etc/init.d/nginx start &;
sleep 10;
/usr/bin/wget http://localhost;

