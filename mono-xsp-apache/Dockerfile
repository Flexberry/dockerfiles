ARG MONO_VERSION=6.10

FROM mono:${MONO_VERSION}

LABEL MAINTAINER=mail@flexberry.ru

ENV LANG ru_RU.UTF-8

RUN apt-get update \
  && apt-get install -y xsltproc mono-xsp libapache2-mod-mono libapache2-mod-proxy-msrpc libapache2-mod-proxy-uwsgi\
  && apt-get autoremove -y \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/* /tmp/*

COPY /configTransform.sh /bin

COPY /start_xsp.sh /bin

# Ensure XMLTEMPLATES and BOOTUP_CHECK_URL environment variables are set for best experience.
CMD /bin/configTransform.sh \
  && /bin/start_xsp.sh

