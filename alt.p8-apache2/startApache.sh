#!/bin/sh
rm -f /var/run/httpd2/httpd.pid;
/usr/sbin/httpd2 -D NO_DETACH -k start
