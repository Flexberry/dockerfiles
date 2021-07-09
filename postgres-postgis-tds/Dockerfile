# postgis https://github.com/appropriate/docker-postgis/blob/master/9.6-2.5/Dockerfile
FROM mdillon/postgis:9.6

ENV POSTGRES_INITDB_ARGS="--lc-collate=ru_RU.UTF-8 --lc-ctype=ru_RU.UTF-8 --lc-time=ru_RU.UTF-8 --lc-numeric=ru_RU.UTF-8"
# via tds_fdw https://github.com/kirychenkav/docker-postgres-tds-fdw/blob/master/9.6-1.0.8/Dockerfile
RUN  buildDependencies="ca-certificates \
    libsybdb5 \
    freetds-dev \
    freetds-common \
    freetds-bin \
    wget \
    gcc \
    make \
    postgresql-server-dev-$PG_MAJOR" \
  && apt-get update \
  && apt-get install -y ${buildDependencies} \
  && wget https://github.com/tds-fdw/tds_fdw/archive/master.tar.gz -O tds_fdw.tar.gz \
  && tar -xvzf tds_fdw.tar.gz \
  && cd tds_fdw-master \
  && make USE_PGXS=1 \
  && make USE_PGXS=1 install \
  && cd .. && rm -rf tds_fdw-master \
  && apt-get remove --yes ca-certificates wget gcc make freetds-dev postgresql-server-dev-$PG_MAJOR && apt-get --yes autoremove \
  && rm -rf /var/lib/apt/lists/* \
&& sed -i '/# TDS protocol version/a \\ttds version=7.0' /etc/freetds/freetds.conf \
&& localedef -i ru_RU -c -f UTF-8 -A /usr/share/locale/locale.alias ru_RU.UTF-8 \
