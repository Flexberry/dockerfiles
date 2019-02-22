FROM openjdk:8-slim

LABEL description="Образ Pentaho Server 8.1, работающего с in-memory базой данных HSQLDB"
LABEL maintainer="Oleg Matvienko <omatvienko@ics.perm.ru>"

RUN sed -i -e 's/main/main contrib/' /etc/apt/sources.list; \
    apt-get update; \
    echo "ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true" | debconf-set-selections; \
	apt-get install -y --no-install-recommends wget unzip apt-transport-https sudo fontconfig ttf-mscorefonts-installer; \
	fc-cache -f -v; \
    apt-get clean && rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*;

ENV PENTAHO_HOME /opt/pentaho

RUN mkdir ${PENTAHO_HOME}; \
    useradd -s /bin/bash -d ${PENTAHO_HOME} pentaho; \
    chown pentaho:pentaho ${PENTAHO_HOME}

ENV BISERVER_VERSION 8.1
ENV BISERVER_TAG 8.1.0.0-365

USER pentaho

RUN wget --progress=dot:giga http://downloads.sourceforge.net/project/pentaho/Pentaho%20${BISERVER_VERSION}/server/pentaho-server-ce-${BISERVER_TAG}.zip -O /tmp/pentaho-server-ce-${BISERVER_TAG}.zip; \
    unzip -q /tmp/pentaho-server-ce-${BISERVER_TAG}.zip -d $PENTAHO_HOME; \
    rm -f /tmp/pentaho-server-ce-${BISERVER_TAG}.zip $PENTAHO_HOME/pentaho-server/promptuser.sh; \
    sed -i -e 's/\(exec ".*"\) start/\1 run/' $PENTAHO_HOME/pentaho-server/tomcat/bin/startup.sh; \
    chmod +x $PENTAHO_HOME/pentaho-server/start-pentaho.sh

COPY configs $PENTAHO_HOME/configs

RUN rm -f -r $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/default-content; \
    cp -fv $PENTAHO_HOME/configs/systemListeners.xml \
       $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/systemListeners.xml; \
	cp -fv $PENTAHO_HOME/configs/pentaho.xml \
       $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/pentaho.xml; 
	#cp -fv $PENTAHO_HOME/configs/applicationContext-spring-security-memory.xml \
    #   $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/applicationContext-spring-security-memory.xml; \
	#cp -fv $PENTAHO_HOME/configs/defaultUser.spring.xml \
    #   $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/defaultUser.spring.xml; \
	#cp -fv $PENTAHO_HOME/configs/defaultUser.spring.properties \
    #   $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/defaultUser.spring.properties; 

WORKDIR $PENTAHO_HOME/pentaho-server
EXPOSE 8080
CMD ["sh", "start-pentaho.sh"]