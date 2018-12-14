FROM flexberry/alt.p8

LABEL description="Pentaho Server 8.2 Image"

ENV BISERVER_VERSION 8.2
ENV BISERVER_TAG 8.2.0.0-342

ENV PENTAHO_HOME /opt/pentaho

RUN \
  bzip2 -d /var/lib/rpm/*bz2; \
  apt-get update; \
  apt-get install -y wget unzip fontconfig;


# RUN sed -i -e 's/main/main contrib/' /etc/apt/sources.list; \
#     apt-get update; \
#     echo "ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true" | debconf-set-selections; \
# 	apt-get install -y --no-install-recommends wget unzip apt-transport-https sudo fontconfig ttf-mscorefonts-installer; \
# 	fc-cache -f -v; \
#     apt-get clean && rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*;

RUN mkdir ${PENTAHO_HOME}; useradd -s /bin/bash -d ${PENTAHO_HOME} pentaho; chown pentaho:pentaho ${PENTAHO_HOME}


ADD /add /


RUN fc-cache -f -v; \
    /usr/bin/unzip -q /tmp/pentaho-server-ce-${BISERVER_TAG}.zip -d  $PENTAHO_HOME; \
    rm -f /tmp/pentaho-server-ce-${BISERVER_TAG}.zip $PENTAHO_HOME/pentaho-server/promptuser.sh; \
    sed -i -e 's/\(exec ".*"\) start/\1 run/' $PENTAHO_HOME/pentaho-server/tomcat/bin/startup.sh; \
    chmod +x $PENTAHO_HOME/pentaho-server/start-pentaho.sh

    #/usr/bin/wget --progress=dot:giga http://downloads.sourceforge.net/project/pentaho/Pentaho%20${BISERVER_VERSION}/server/pentaho-server-ce-${BISERVER_TAG}.zip -O /tmp/pentaho-server-ce-${BISERVER_TAG}.zip; \


RUN \
  bzip2 -d /var/lib/rpm/*bz2; \
  apt-get update; \
  apt-get update; \
  apt-get install -y java-1.8.0-openjdk;

USER pentaho
WORKDIR /opt/pentaho/pentaho-server
EXPOSE 8080
CMD ["sh", "start-pentaho.sh"]
