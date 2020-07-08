FROM flexberry/alt.p8-dev AS dev
#COPY Mondrian /opt

RUN \
  cd /opt; git clone https://github.com/pentaho/mondrian; \
  cd mondrian; \
  mvn compile