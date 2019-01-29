FROM flexberry/alt.p8-dev as dev


USER root

RUN cd /root;\
  git clone https://github.com/Flexberry/saiku; \
  cd saiku; \
  git checkout flexberry-pentaho;

COPY /root /

RUN  cd /root/saiku; \
  mvn  package -DskipTests

COPY /add /root/saiku/saiku-bi-platform-plugin-p7.1/target/dist/saiku/



USER pentaho

FROM flexberry/pentaho:8.2
LABEL description="Pentaho Server 8.2 Image with saiku (OLAP)"
COPY --from=dev --chown=pentaho /root/saiku/saiku-bi-platform-plugin-p7.1/target/dist/ /opt/pentaho/pentaho-server/pentaho-solutions/system
