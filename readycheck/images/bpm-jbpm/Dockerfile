FROM dh.ics.perm.ru/zs-dms-ose/jbpm

COPY readycheck /bin

RUN bzip2 -d /var/lib/rpm/*.bz2; apt-get update; apt-get install -y curl

CMD readycheck && ./start_jbpm-wb.sh

