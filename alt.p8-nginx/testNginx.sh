#!/bin/sh
set -x
/usr/sbin/nginx &
sleep 10;
/usr/bin/wget http://localhost;

