ARG BASEIMAGE
FROM $BASEIMAGE

ARG TGZFILE
ADD /$TGZFILE /

COPY /addNginxUser.sh /bin/

RUN  \
  addNginxUser.sh;\
  sed  -i '/user/a daemon off;' /etc/nginx/nginx.conf

CMD /etc/init.d/nginx start


