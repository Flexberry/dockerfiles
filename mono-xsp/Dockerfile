ARG MONO_VERSION=6.12

FROM flexberry/mono:${MONO_VERSION}

LABEL MAINTAINER=mail@flexberry.ru

RUN apt-get update \
  && apt-get install -y mono-xsp \
  && xsp --version \
  && apt-get autoremove -y \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/* /tmp/*
  
COPY /start_xsp.sh /bin

# Ensure XMLTEMPLATES and BOOTUP_CHECK_URL environment variables are set for best experience.
CMD /bin/configTransform.sh \
  && /bin/start_xsp.sh
