FROM dh.ics.perm.ru/zs-dms-ose/runtime-timer-quartznet-service

COPY readycheck /bin

RUN  apt-get install -y curl

CMD readycheck && /bin/change_XMLconfig_from_env.sh &&  mono-service ./NewPlatform.Flexberry.BPM.Runtime.QuartzNetTimer.Service.exe --no-daemon

