FROM flexberry/alt.p8-dev as clickhouse
RUN cd /root; \
  git clone https://github.com/yandex/clickhouse-jdbc; \
  cd clickhouse-jdbc/; \
  mvn -q package assembly:single -DskipTests=true;



FROM flexberry/alt.p8-jdk

LABEL description=" Pentaho Server 8.2  image with the ability to transfer administrative tables to the Postgres database"
LABEL maintainer="Oleg Matvienko <omatvienko@ics.perm.ru>, Alexey Kostarev <kaf@ics.perm.ru>"

ENV PENTAHO_HOME /opt/pentaho

RUN mkdir ${PENTAHO_HOME}; \
    useradd -s /bin/bash -d ${PENTAHO_HOME} pentaho; \
    chown pentaho:pentaho ${PENTAHO_HOME}

ENV BISERVER_VERSION 8.2
ENV BISERVER_TAG 8.2.0.0-342

RUN wget --progress=dot:giga http://downloads.sourceforge.net/project/pentaho/Pentaho%20${BISERVER_VERSION}/server/pentaho-server-ce-${BISERVER_TAG}.zip -O /tmp/pentaho-server-ce-${BISERVER_TAG}.zip; \
    unzip -q /tmp/pentaho-server-ce-${BISERVER_TAG}.zip -d $PENTAHO_HOME; \
    rm -f /tmp/pentaho-server-ce-${BISERVER_TAG}.zip $PENTAHO_HOME/pentaho-server/promptuser.sh; \
    sed -i -e 's/\(exec ".*"\) start/\1 run/' $PENTAHO_HOME/pentaho-server/tomcat/bin/startup.sh; \
    chmod +x $PENTAHO_HOME/pentaho-server/start-pentaho.sh

RUN  \
    bzip2 -d /var/lib/rpm/*bz2; \
    apt-get update; \
    apt-get install -y fonts-ttf-ms; \
    fc-cache -f -v; \
    apt-get install -y xsltproc; \
    bzip2 -9 /var/lib/rpm/*


ENV POSTGRESQL_DRIVER_VERSION 42.2.5
RUN \
  rm -f $PENTAHO_HOME/pentaho-server/tomcat/lib/postgre*.jar && \
  wget --progress=dot:giga https://jdbc.postgresql.org/download/postgresql-${POSTGRESQL_DRIVER_VERSION}.jar -O $PENTAHO_HOME/pentaho-server/tomcat/lib/postgresql-${POSTGRESQL_DRIVER_VERSION}.jar


ENV CLICKHOUSE_DRIVER_VERSION=0.1
COPY --from=clickhouse --chown=pentaho /root/clickhouse-jdbc/target/clickhouse-jdbc-${CLICKHOUSE_DRIVER_VERSION}-SNAPSHOT-jar-with-dependencies.jar /opt/pentaho/pentaho-server/pentaho-solutions/system;



#RUN apt-get update; apt-get install -y less vim libxml2-utils

COPY configs $PENTAHO_HOME/configs

COPY entrypoint.sh /entrypoint.sh

# USER pentaho

WORKDIR $PENTAHO_HOME/pentaho-server
EXPOSE 8080

ENTRYPOINT ["/entrypoint.sh"]
CMD ["run"]
