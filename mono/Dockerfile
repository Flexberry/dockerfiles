ARG MONO_VERSION=latest

FROM mono:${MONO_VERSION}

LABEL MAINTAINER=mail@flexberry.ru

RUN apt-get update \
  && apt-get install -y xsltproc \
  && apt-get autoremove -y \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/* /tmp/*
  
COPY /configTransform.sh /bin
RUN chmod +x /bin/configTransform.sh

# Ensure XMLTEMPLATES environment variable is set for best experience.
CMD /bin/configTransform.sh