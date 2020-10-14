ARG MONO_VERSION=6.12

FROM mono:${MONO_VERSION}

LABEL MAINTAINER=mail@flexberry.ru

ENV LANG ru_RU.UTF-8

RUN apt-get update \
  && apt-get install -y xsltproc \
  && apt-get autoremove -y \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/* /tmp/*
  
COPY /configTransform.sh /bin

# Ensure XMLTEMPLATES environment variable is set for best experience.
CMD /bin/configTransform.sh
