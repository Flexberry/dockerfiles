#!/bin/sh

/etc/init.d/mysqld start;
/usr/sbin/httpd2 -D NO_DETACH -k start
