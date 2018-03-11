FROM kafnevod/altlinux.p8

ENV LANG ru_RU.UTF-8

MAINTAINER kaf@nevod.ru

RUN export http_proxy=$http_proxy;\
  bzip2 -d /var/lib/rpm/*.bz2; \
  apt-get update && \
  apt-get install -y apache2; \
  cd /etc/httpd2/conf/sites-available/; \
  mkdir /conf; \
  mv vhosts.conf /conf; \
  ln -sf /conf/vhosts.conf .; \
  a2ensite vhosts; \
  a2enmod rewrite; \
  rm -f /var/cache/apt/archives/*.rpm /var/cache/apt/*.bin /var/lib/apt/lists/*.*; \
  bzip2 -9 /var/lib/rpm/*

EXPOSE 80

CMD [ "/usr/sbin/httpd2", "-D", "NO_DETACH", "-k", "start" ]
#ENTRYPOINT [ "/usr/sbin/httpd2", "-D", "NO_DETACH", "-k", "start" ]
