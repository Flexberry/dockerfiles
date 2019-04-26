#!/bin/sh

getent group nginx >/dev/null || groupadd -r nginx
getent passwd nginx >/dev/null || useradd -r -g nginx -s /sbin/nologin -d /var/cache/nginx -c "nginx user"  nginx
sed -i 's/user  root;/user nginx;/' /etc/nginx/nginx.conf