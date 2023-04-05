FROM dh.ics.perm.ru/zs-dms-ose/bpm-activityservice

COPY readycheck /bin/readycheck

CMD readycheck && update-ca-certificates && /bin/configTransform.sh && dotnet ZS.DMS.ActivityServices.dll

