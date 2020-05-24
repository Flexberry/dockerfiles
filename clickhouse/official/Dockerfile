FROM yandex/clickhouse-server:20.4

ENV LANG ru_RU.UTF-8

MAINTAINER mail@flexberry.ru

RUN \
    apt-get update; \
    apt-get install -y less vim psmisc telnet postgresql-client unixodbc odbcinst odbc-postgresql xsltproc libxml2-utils python3-pip; \
    pip3 install clickhouse-driver psycopg2-binary

COPY /docker-entrypoint-initdb.d /docker-entrypoint-initdb.d

RUN \
   mkdir -p /opt/clickhouse-bulk; \
   cd /opt/clickhouse-bulk; \
   wget -O - --no-check-certificate https://github.com/nikepan/clickhouse-bulk/releases/download/v1.2.2/clickhouse-bulk_1.2.2_linux_amd64.tar.gz /opt/clickhouse-bulk | tar xvzf -

# ENTRYPOINT "/entrypoint.sh gosu clickhouse:clickhouse /usr/bin/clickhouse-server --config-file=/etc/clickhouse-server/config.xml"
