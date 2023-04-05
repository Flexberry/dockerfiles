FROM dh.ics.perm.ru/zs-dms-ose/esb

COPY readycheck /bin

CMD /bin/readycheck  && \
  /bin/change_XMLconfig_from_env.sh && \
  adduser test; \
  userdel -r flexberry; adduser flexberry;\
  su -l -c "sh /opt/start-flexberry-service-bus.sh" flexberry



