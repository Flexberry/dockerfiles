FROM dh.ics.perm.ru/zs-dms-ose/web-api

COPY readycheck /bin

CMD readycheck && /bin/configTransform.sh   && /bin/start_xsp.sh

