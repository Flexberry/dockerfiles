FROM flexberry/alt.p11

LABEL maintainer="mail@flexberry.ru"

ENV LANG="ru_RU.UTF-8"

RUN export http_proxy=$http_proxy;\
  bzip2 -d /var/lib/rpm/*.bz2 && \
  apt-get update && \
  apt-get install -y mono-full mono-web xsltproc && \
  rm -f /var/cache/apt/archives/*.rpm /var/cache/apt/*.bin /var/lib/apt/lists/*.* && \
  bzip2 -9 /var/lib/rpm/*

COPY /configTransform.sh /bin
RUN chmod +x /bin/configTransform.sh

# Ensure XMLTEMPLATES environment variable is set for best experience.
CMD  [ "/bin/configTransform.sh" ]
