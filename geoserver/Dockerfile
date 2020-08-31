FROM tomcat

RUN rm -rf /usr/local/tomcat/webapps/* && \
    wget https://sourceforge.net/projects/geoserver/files/GeoServer/2.17.1/geoserver-2.17.1-war.zip -O /tmp/geoserver.zip && \
    unzip /tmp/geoserver.zip -d /tmp/ && \
    unzip /tmp/geoserver.war -d /usr/local/tomcat/webapps/geoserver && \
    wget https://build.geoserver.org/geoserver/2.17.x/community-latest/geoserver-2.17-SNAPSHOT-sec-keycloak-plugin.zip -O /tmp/keycloak-plugin.zip && \
    unzip -o /tmp/keycloak-plugin.zip -d /usr/local/tomcat/webapps/geoserver/WEB-INF/lib/ && \
    rm -rf /tmp/*
