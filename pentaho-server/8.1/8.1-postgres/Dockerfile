FROM Flexberry/pentaho-8.1:latest

LABEL description="Pentaho Server 8.1, working with PostgreSQL"
MAINTAINER Oleg Matvienko <omatvienko@ics.perm.ru>

ENV PENTAHO_HOME /opt/pentaho
ENV POSTGRESQL_DRIVER_VERSION 42.2.3

USER root

RUN apt-get update; \
    apt-get install netcat postgresql-client -y; \
    apt-get clean && rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*; 

# Update PostgreSQL JDBC Driver
RUN rm -f $PENTAHO_HOME/pentaho-server/tomcat/lib/postgre*.jar && \
    /usr/bin/wget --progress=dot:giga https://jdbc.postgresql.org/download/postgresql-${POSTGRESQL_DRIVER_VERSION}.jar -O $PENTAHO_HOME/pentaho-server/tomcat/lib/postgresql-${POSTGRESQL_DRIVER_VERSION}.jar

COPY configs $PENTAHO_HOME/configs
COPY entrypoint.sh /entrypoint.sh

RUN chmod +x /entrypoint.sh && \
    chown pentaho:pentaho ${PENTAHO_HOME}/configs
USER pentaho

ENTRYPOINT ["/entrypoint.sh"]
CMD ["run"]