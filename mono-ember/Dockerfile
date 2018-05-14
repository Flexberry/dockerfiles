FROM mono

ENV LANG ru_RU.UTF-8

MAINTAINER mail@flexberry.ru

RUN apt-get update ; \
    apt-get install -y mc htop mono-4.0-service git ; \
	curl -sL http://deb.nodesource.com/setup_6.x -o nodesource_setup.sh ; \
	bash nodesource_setup.sh ; \
	apt-get -y install nodejs ; \
	yarn global add bower ; \
	yarn global add ember-cli@2.4.3 ; \
	apt-get -y install xvfb

CMD /bin/bash


