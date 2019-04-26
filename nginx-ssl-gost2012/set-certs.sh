#!/bin/sh

export PATH=/opt/cprocsp/bin/amd64:/opt/cprocsp/sbin/amd64:/opt/cprocsp/cp-openssl-1.1.0/bin/amd64:/sbin:/usr/sbin:/usr/local/sbin:/bin:/usr/bin:/usr/local/bin
export LD_LIBRARY_PATH=/usr/local/lib:/opt/cprocsp/lib/amd64/:/opt/cprocsp/cp-openssl-1.1.0/lib/amd64/

cd /root;
cryptsrv;
./install-certs.sh;
tar cvzf /tmp/nginx.tgz /etc/init.d/nginx \
  /usr/sbin/nginx \
  /etc/nginx \
  /opt/cprocsp/ \
  /usr/local/lib \
  /usr/local/bin \
  /usr/local/share/ \
  /usr/local/include \
  /var/opt/cprocsp/ \
  /etc/opt/cprocsp/ \
  /var/log/nginx/ \
  /var/cache/nginx
  
  