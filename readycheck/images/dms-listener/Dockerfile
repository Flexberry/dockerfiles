FROM dh.ics.perm.ru/zs-dms-ose/dms-listener

COPY readycheck /bin

CMD readycheck && /bin/configTransform.sh && mono-service ./ZS.DMS.ServiceBusListenerWinServiceHost.exe --no-daemon

