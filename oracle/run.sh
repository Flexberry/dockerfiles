
docker rm -f oracle-rtctest
docker run -d  \
	--name oracle-rtctest \
	-p 49160:22 \
	-p 1521:1521 \
	-e ORACLE_ALLOW_REMOTE=true \
	dh.ics.perm.ru/kaf/oracle-xe-11g-rtctest

#        -v /home/kaf/docker/oracle/u01/app/oracle/oradata/XE/:/u01/app/oracle/oradata/XE/ \

