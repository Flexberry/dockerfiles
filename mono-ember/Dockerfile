FROM mono:latest

MAINTAINER mail@flexberry.ru

RUN \
	echo  "deb http://ftp.us.debian.org/debian/ jessie main contrib non-free\ndeb-src http://ftp.us.debian.org/debian/ jessie main contrib non-free\n" >> /etc/apt/sources.list; \
  apt-get update ; \
    apt-get install -y mc htop mono-4.0-service git sudo ; \
	curl -sL https://deb.nodesource.com/setup_10.x | sudo -E bash -; \
	apt-get -y install nodejs ; \
    curl -sL https://dl.yarnpkg.com/debian/pubkey.gpg | apt-key add -  ; \
    echo "deb https://dl.yarnpkg.com/debian/ stable main" | tee /etc/apt/sources.list.d/yarn.list ; \
    apt-get update && apt-get -y install yarn ; \
	yarn global add bower ; \
	yarn global add ember-cli@3.1.4 ; \
	apt-get -y install xvfb fontconfig ttf-mscorefonts-installer; \
  fc-cache -vf


CMD /bin/bash


