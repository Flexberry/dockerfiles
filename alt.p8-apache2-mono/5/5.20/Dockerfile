FROM flexberry/alt.p8-apache2:2.4.38

ENV LANG ru_RU.UTF-8
ENV REPODATA=2019/08/06

MAINTAINER mail@flexberry.ru

COPY /root /

RUN  \
  bzip2 -d /var/lib/rpm/*.bz2; \
  apt-get update; \
  apt-get install -y apt-repo; \
  apt-repo rm all;  \
  apt-repo add http://ftp.altlinux.org/pub/distributions/archive/p8/date/$REPODATA x86_64-i586 classic; \
  apt-repo add http://ftp.altlinux.org/pub/distributions/archive/p8/date/$REPODATA x86_64 classic; \
  apt-repo add http://ftp.altlinux.org/pub/distributions/archive/p8/date/$REPODATA noarch classic; \
  apt-get install -y apache2-mod_mono mono-full mono-web; apt-get remove -y mono-monodoc;\
  cd /etc/httpd2/conf/sites-available/; \
  mv vhosts.conf /conf; \
  ln -sf /conf/vhosts.conf .; \
  a2ensite vhosts; \
  rm -f /tmp/* /var/cache/apt/archives/*.rpm /var/cache/apt/*.bin /var/lib/apt/lists/*.* ; \
  bzip2 -9 /var/lib/rpm/*

COPY /change_XMLconfig_from_env.sh /bin

# XMLTEMPLATES may contain list of XML template files, configured by change_XMLconfig_from_env.sh script
#ENV XMLTEMPLATES

# https://github.com/mono/mono/issues/12557
ENV MONO_MANAGED_WATCHER=disabled

CMD /bin/change_XMLconfig_from_env.sh && \
    /bin/startApache.sh

