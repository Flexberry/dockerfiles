FROM flexberry/alt.p8

ENV LANG ru_RU.UTF-8

MAINTAINER mail@flexberry.ru

RUN export http_proxy=$http_proxy;\
  bzip -d /var/lib/rpm/*.bz2; \
  apt-get update; \
  apt-get install -y ansible python-module-pip openssh-clients; \
  adduser ansible; \
  ssh-keygen -q -t rsa -N '' -f /root/.ssh/id_rsa;\
  echo "    StrictHostKeyChecking no" >> /etc/openssh/ssh_config ; \
  cd /bin; wget -c https://raw.githubusercontent.com/appuio/ansible-ini2yaml/master/ini2yaml; \
  chmod a+x /bin/ini2yaml

COPY /root /


