FROM flexberry/alt.p8-postgresql

COPY . /docker/

ARG TAG=1.2.0-beta03-postgresql-db
RUN cd /docker/host/ && wget https://raw.githubusercontent.com/Flexberry/NewPlatform.Flexberry.ServiceBus/$TAG/Flexberry%20Service%20Bus/SQL/PostgreSQL/CreateNewDB.sql && \
  /bin/sh /docker/host/createdb.sh

MAINTAINER mail@flexberry.net

