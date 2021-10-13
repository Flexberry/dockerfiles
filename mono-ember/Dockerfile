FROM mono:6

MAINTAINER mail@flexberry.ru

RUN \
  echo  "deb http://ftp.us.debian.org/debian/ buster main contrib non-free\ndeb-src http://ftp.us.debian.org/debian/ buster main contrib non-free\n" >> /etc/apt/sources.list; \
  apt-get update;

RUN \
  apt-get install -y yarnpkg; \
  cd /usr/bin/; ln -sf ../lib/nodejs/yarn/bin/yarn.js yarn; \
  yarn global add bower ; \
	yarn global add ember-cli@3.1.4 ;

RUN apt-get -y install git mc htop sudo xvfb fontconfig ttf-mscorefonts-installer; \
  fc-cache -vf

CMD /bin/bash


