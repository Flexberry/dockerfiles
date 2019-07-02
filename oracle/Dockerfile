
FROM wnameless/oracle-xe-11g
RUN export http_proxy=$http_proxy;\
	apt-get update && apt-get -y  install mc htop;

COPY  /create.sql /tmp

RUN \
	export ORACLE_ALLOW_REMOTE=true; \
	export ORACLE_HOME=/u01/app/oracle/product/11.2.0/xe; \
	export ORACLE_SID=XE; \
	export PATH=/u01/app/oracle/product/11.2.0/xe/bin:$PATH; \
	/usr/sbin/startup.sh ; \	
	cd /tmp; sqlplus system/oracle @create.sql; \
	/etc/init.d/oracle-xe stop;

