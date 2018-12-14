FROM flexberry/alt.p8-dev
#as dev

ADD /add /

USER root

RUN cd /root;\
  git clone https://github.com/Flexberry/saiku; \
  cd saiku; \
  git checkout flexberry-pentaho;

  #mvn install



#FROM flexberry/pentaho:8.2
#LABEL description="Pentaho Server 8.2 Image with saiku (OLAP)"

