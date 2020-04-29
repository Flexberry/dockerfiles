FROM flexberry/alt.p9

ENV LANG ru_RU.UTF-8

MAINTAINER mail@flexberry.ru

RUN export http_proxy=$http_proxy;\
  bzip2 -d /var/lib/rpm/*.bz2; \
  apt-get update && \
  apt-get install -y apache2 apache2-mod_ssl; \
  cd /etc/httpd2/conf/sites-available/; \
  mkdir /conf; \
  mv vhosts.conf /conf; \
  rm -f /var/cache/apt/archives/*.rpm /var/cache/apt/*.bin /var/lib/apt/lists/*.*; \
  bzip2 -9 /var/lib/rpm/*

COPY /startApache.sh /bin/startApache.sh

EXPOSE 80

CMD [ "/bin/startApache.sh" ]
#ENTRYPOINT [ "/usr/sbin/httpd2", "-D", "NO_DETACH", "-k", "start" ]

