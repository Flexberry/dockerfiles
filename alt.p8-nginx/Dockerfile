FROM flexberry/alt.p8

ENV LANG ru_RU.UTF-8

MAINTAINER mail@flexberry.ru

RUN \
  bzip2 -d /var/lib/rpm/*.bz2; \
  apt-get update && \
  apt-get install -y nginx; \
  mkdir -p /var/www/html; \
  echo "<html><body>test</body></html>" > /var/www/html/index.html; \
  echo "daemon off;" >> /etc/nginx/nginx.conf; \
  cd /etc/nginx/sites-enabled.d/; sed -i -e 's/localhost:80/*:80/' ../sites-available.d/default.conf; ln -sf ../sites-available.d/default.conf .; \
  rm -f /var/cache/apt/archives/*.rpm /var/cache/apt/*.bin /var/lib/apt/lists/*.*; \
  bzip2 -9 /var/lib/rpm/*

COPY testNginx.sh /bin
  
CMD /usr/sbin/nginx