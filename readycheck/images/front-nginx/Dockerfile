FROM dh.ics.perm.ru/zs-dms-ose/dms-front

COPY readycheck /bin

COPY nginx.conf /etc/nginx/nginx.conf

CMD readycheck && nginx -g 'daemon off;'

