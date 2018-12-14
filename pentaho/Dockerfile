FROM flexberry/alt.p8-jdk

LABEL description="Pentaho Server 8.2 Image"

ENV BISERVER_VERSION 8.2
ENV BISERVER_TAG 8.2.0.0-342

ENV PENTAHO_HOME /opt/pentaho
ENV ZIPFILE /tmp/pentaho-server-ce-${BISERVER_TAG}.zip

RUN mkdir ${PENTAHO_HOME}; useradd -s /bin/bash -d ${PENTAHO_HOME} pentaho; chown pentaho:pentaho ${PENTAHO_HOME}


ADD /add /


RUN set -x; fc-cache -f -v; \
    if [ ! -f $ZIPFILE ]; \
    then \
      /usr/bin/wget --progress=dot:giga http://downloads.sourceforge.net/project/pentaho/Pentaho%20${BISERVER_VERSION}/server/pentaho-server-ce-${BISERVER_TAG}.zip -O $ZIPFILE; \
    fi; \
    /usr/bin/unzip $ZIPFILE -d $PENTAHO_HOME; \
    rm -f $ZIPFILE /tmp/pentaho-server-ce-${BISERVER_TAG}.zip $PENTAHO_HOME/pentaho-server/promptuser.sh; \
    sed -i -e 's/\(exec ".*"\) start/\1 run/' $PENTAHO_HOME/pentaho-server/tomcat/bin/startup.sh; \
    chmod +x $PENTAHO_HOME/pentaho-server/start-pentaho.sh



USER root
WORKDIR /opt/pentaho/pentaho-server
EXPOSE 8080
CMD ["sh", "start-pentaho.sh"]
