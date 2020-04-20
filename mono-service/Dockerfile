ARG MONO_VERSION=6.6

FROM flexberry/mono:${MONO_VERSION}

LABEL MAINTAINER=mail@flexberry.ru

RUN apt-get update \
  && apt-get install -y mono-4.0-service \
  && apt-get autoremove -y \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/* /tmp/*
