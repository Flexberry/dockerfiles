FROM flexberry/alt.p8-dev as dev

USER root

RUN cd /root;\
  git clone https://github.com/Flexberry/saiku; \
  cd saiku; \
  git checkout flexberry-pentaho;

RUN  cd /root/saiku/saiku-ui; \
  mvn  package -DskipTests; \
  mkdir -p /tmp/build/system/saiku/ui/; \
  cp target/saiku-ui/saiku.min.js /tmp/build/system/saiku/ui/; \
  mkdir -p /tmp/build/system/saiku/ui/js/saiku/plugins/CCC_Chart/; \
  cp target/saiku-ui/js/saiku/plugins/CCC_Chart/pvc-d.js /tmp/build/system/saiku/ui/js/saiku/plugins/CCC_Chart/; \
  mkdir -p /tmp/build/system/saiku/ui/js/saiku/plugins/CCC_Chart2/; \
  cp target/saiku-ui/js/saiku/plugins/CCC_Chart2/pvc.js /tmp/build/system/saiku/ui/js/saiku/plugins/CCC_Chart2/; \
  cp target/saiku-ui/js/saiku/plugins/CCC_Chart2/protovis.js /tmp/build/system/saiku/ui/js/saiku/plugins/CCC_Chart2/; \
  mkdir -p /tmp/build/system/saiku/ui/js/saiku/plugins/I18n/; \
  cp target/saiku-ui/js/saiku/plugins/I18n/plugin.js /tmp/build/system/saiku/ui/js/saiku/plugins/I18n/; \
  mkdir -p /tmp/build/system/saiku/ui/js/saiku/plugins/I18n/po/; \
  cp target/saiku-ui/js/saiku/plugins/I18n/po/ru.json /tmp/build/system/saiku/ui/js/saiku/plugins/I18n/po/;

RUN  cd /root;\
  git clone https://github.com/Flexberry/fop; \
  cd fop/; \
  git checkout addConfigParser;

 RUN cd /root/fop; \
  mvn package; \
  mkdir -p /tmp/build/system/saiku/lib/; \
  cp fop/target/fop-2.4.0-SNAPSHOT.jar /tmp/build/system/saiku/lib/;

COPY /add  /tmp/build/system/saiku/


# USER pentaho

FROM flexberry/alt.p8-pentaho
LABEL description="Pentaho Server 8.2 Image with saiku (OLAP)"
RUN echo "Download plugins..." ;\
  cd /opt/pentaho/pentaho-server/; \
	wget -O btable.zip https://sourceforge.net/projects/btable/files/Version3.0-3.6/BTable-pentaho7-3.6-STABLE.zip/download \
	&& wget -O saiku-chart-plus.zip http://sourceforge.net/projects/saikuchartplus/files/SaikuChartPlus3/saiku-chart-plus-vSaiku3-plugin-pentaho.zip/download \
	&& wget --progress=dot:giga  http://meteorite.bi/downloads/saiku-plugin-p7.1-3.15.zip \
			https://github.com/zhicwu/cte/releases/download/7.1-SNAPSHOT/cte-7.1-SNAPSHOT.zip \
			http://ctools.pentaho.com/files/d3ComponentLibrary/17.07.24/d3ComponentLibrary-17.07.24.zip \
			http://ctools.pentaho.com/files/cdv/17.05.12/7.x/cdv-7.1-17.05.12.zip \
	&& echo "Installing plugins..." \
        && for i in *.zip; do echo "Unpacking $i..." && unzip -q -d pentaho-solutions/system $i && rm -f $i; done

COPY --from=dev  /tmp/build/ /opt/pentaho/pentaho-server/pentaho-solutions/
