FROM flexberry/mono-ember

MAINTAINER mail@flexberry.ru

ENV LANG ru_RU.UTF-8

RUN apt-get update ; \
	apt install -y locales ; \
	echo "LANG=ru_RU.UTF-8" > /etc/default/locale ; \
	echo "ru_RU.UTF-8 UTF-8" > /etc/locale.gen  ; \
	dpkg-reconfigure --frontend=noninteractive locales ; \
    apt-get install -y monodevelop mono-dbg cifs-utils mate-terminal mono-xsp4 ; \
	git clone --depth=50 --branch=mono-5.10.0.160 https://github.com/mono/mono.git /build/mono-5.10.0.160 ; \
	git checkout mono-5.10.0.160 ; \
	rm /*

CMD /bin/bash


