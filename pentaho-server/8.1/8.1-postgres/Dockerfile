FROM flexberry/pentaho:8.1

LABEL description="Pentaho Server 8.1, working with PostgreSQL"
LABEL maintainer="Oleg Matvienko <omatvienko@ics.perm.ru>"

USER root

RUN apt-get update; \
    apt-get install -y --no-install-recommends netcat postgresql-client; \
    apt-get clean && rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*;

ENV POSTGRESQL_DRIVER_VERSION 42.2.5

# Update PostgreSQL JDBC Driver
RUN rm -f $PENTAHO_HOME/pentaho-server/tomcat/lib/postgre*.jar && \
    wget --progress=dot:giga https://jdbc.postgresql.org/download/postgresql-${POSTGRESQL_DRIVER_VERSION}.jar -O $PENTAHO_HOME/pentaho-server/tomcat/lib/postgresql-${POSTGRESQL_DRIVER_VERSION}.jar

COPY configs $PENTAHO_HOME/configs
COPY entrypoint.sh /entrypoint.sh

RUN chmod +x /entrypoint.sh && \
    chown pentaho:pentaho ${PENTAHO_HOME}/configs
USER pentaho

ENTRYPOINT ["/entrypoint.sh"]
CMD ["run"]