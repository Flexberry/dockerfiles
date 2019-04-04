FROM flexberry/alt.p8-apache2:2.4.38-1.2

ENV LANG ru_RU.UTF-8

MAINTAINER mail@flexberry.ru

COPY rpms.tgz apache2_mono4.tgz /tmp/


RUN  \
  cd /tmp;\
  bzip2 -d /var/lib/rpm/*.bz2; \
  tar xzf rpms.tgz; \
  apt-get update && apt-get install -y *.rpm xsltproc; \
  tar xzf apache2_mono4.tgz -C /; \
  cd /etc/httpd2/conf/sites-available/; \
  mv vhosts.conf /conf; \
  ln -sf /conf/vhosts.conf .; \
  a2ensite vhosts; \
  rm -f /tmp/* /var/cache/apt/archives/*.rpm /var/cache/apt/*.bin /var/lib/apt/lists/*.* ; \
  bzip2 -9 /var/lib/rpm/*

COPY /change_XMLconfig_from_env.sh /bin

# XMLTEMPLATES may contain list of XML template files, configured by change_XMLconfig_from_env.sh script
#ENV XMLTEMPLATES

CMD /bin/change_XMLconfig_from_env.sh && \
    /bin/startApache.sh

