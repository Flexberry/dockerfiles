FROM yandex/clickhouse-server:19.5

ENV LANG ru_RU.UTF-8

MAINTAINER mail@flexberry.ru

RUN \
    apt-get update; \
    apt-get install -y less vim psmisc telnet postgresql-client unixodbc odbcinst odbc-postgresql xsltproc

COPY /docker-entrypoint-initdb.d /docker-entrypoint-initdb.d
COPY /startClickhouse.sh /startClickhouse.sh

ENTRYPOINT ["/entrypoint.sh", "/startClickhouse.sh"]
