FROM flexberry/alt.p8-jdk

LABEL description="Pentaho Server 8.2 Image"

ENV BISERVER_VERSION 8.2
ENV BISERVER_TAG 8.2.0.0-342

ENV PENTAHO_HOME /biserver-ce
ENV ZIPFILE /tmp/pentaho-server-ce-${BISERVER_TAG}.zip

ENV POSTGRESQL_DRIVER_VERSION=42.2.5

RUN mkdir ${PENTAHO_HOME}; useradd -s /bin/bash -d ${PENTAHO_HOME} pentaho; chown pentaho:pentaho ${PENTAHO_HOME}


ADD /add.tgz /
ADD /fonts.tgz /

RUN set -x; fc-cache -f -v; \
    if [ ! -f $ZIPFILE ]; \
    then \
      /usr/bin/wget --progress=dot:giga http://downloads.sourceforge.net/project/pentaho/Pentaho%20${BISERVER_VERSION}/server/pentaho-server-ce-${BISERVER_TAG}.zip -O $ZIPFILE; \
    fi; \
    /usr/bin/unzip  $ZIPFILE -d / 2>/dev/null; \
    rm -rf $PENTAHO_HOME; \
    mv /pentaho-server  $PENTAHO_HOME; \
    rm -f $ZIPFILE  $PENTAHO_HOME/promptuser.sh; \
    sed -i -e 's/\(exec ".*"\) start/\1 run/' $PENTAHO_HOME/tomcat/bin/startup.sh; \
    chmod +x $PENTAHO_HOME/start-pentaho.sh

RUN echo "Install databases drivers"; \
     rm -f /biserver-ce/tomcat/lib/postgresql-*.jar; \
     wget --progress=dot:giga https://jdbc.postgresql.org/download/postgresql-${POSTGRESQL_DRIVER_VERSION}.jar -O /biserver-ce/tomcat/lib/postgresql-${POSTGRESQL_DRIVER_VERSION}.jar



USER root
WORKDIR /biserver-ce
EXPOSE 8080
CMD ["sh", "start-pentaho.sh"]
