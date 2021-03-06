# Описание

Данный образ поддерживает функционал [apache-mono (версия 4.6.2.7)](https://github.com/Flexberry/dockerfiles/blob/master/alt.p8-apache2/README-ru.md) сервера и является базовым образом для создания специализированных образов запуска `apache-mono` приложений. 

## Переменные среды

- `XMLTEMPLATES` - список корректируемых XML-файлов.
- Переменные, перечисленные в корректируемых XML-файлах.
- `MODULES` - наследуемая переменная. Содержит список инициализируемых apache-модулей. См. [https://github.com/Flexberry/dockerfiles/blob/master/alt.p8-apache2/README-ru.md](https://github.com/Flexberry/dockerfiles/blob/master/alt.p8-apache2/README-ru.md).
- `BOOTUP_CHECK_URL` - наследуемая переменная, Содержит локальный `URL` вида `http:/0.0.0.0:<PORT>/<PATH>`. При наличии этой переменной после запуска WEB-сервера стартовый скрипт ожидает доступность сервиса по этому `URL` и только после этого заканчивает работу. См. [https://github.com/Flexberry/dockerfiles/blob/master/alt.p8-apache2/README-ru.md](https://github.com/Flexberry/dockerfiles/blob/master/alt.p8-apache2/README-ru.md).


## Функционал

Начиная с версии `4.6.2.7-1.3.0` поддерживается возможность настройки аргументов тегов XML-файлов.
Если аргумент тега XML-файла содежит шаблон типа `%%ИМЯ_ПЕРЕМЕННОЙ%%`,
то значение данного аргумента заменяется на значение указанной переменной.

Список корректируемых файлов указывается в переменной среды `XMLTEMPLATES`.
Имена файлов в списке разделяются пробелами.
Если переменная `XMLTEMPLATES` пуста или не инициализирована корректировки файлов не производятся

При запуске контейнера образа должны быть определены все переменные, указанные в шаблонах.
Если хоть одна переменная не определена или имеет пустое значение контейнер завершает свою работу.
Переменные среды могут быть заданы следующими способами:
- инициализация в операторе `ENV` файла `Dockerfile` при создании подобраза. Например:
  ```
  FROM flexberry/alt.p8-apache2-mono:4.6.2.7-1.3
  ...
  ENV XMLTEMPLATES "/var/www/web-api/app/Web.config"
  ...
  ```
- инициализация при запуске контейнера через флаг `-e`. Например:
  ```
  $ docker run -e "XMLTEMPLATES=/var/www/web-api/app/Web.config" ...
  ```
  
- инициализацией в YML-файле. Например:
  ```
  services:
    monoservice:
      image: ...
        environment:
          - XMLTEMPLATES=/var/www/web-api/app/Web.config

> Рекомендуется переменную `XMLTEMPLATES` и `default`'ные значения переменных используемых для коректировки инициализироать в `Dockerfile` дочернего образа. Текущие значения переменных целесообазнее указывать при запуске контейнера/сервиса в параметрах или `yml-файлах`.

## Изменение стартового функционала

При запуске контейнера/сервиса производится выполнение следующих команд оператора `CMD`:
```
CMD /bin/change_XMLconfig_from_env.sh && \
    /bin/startApache.sh
```
Скрипт `change_XMLconfig_from_env.sh` в файлах, перечисленных в переменной `XMLTEMPLATES`производит корректировку аргументов.
В случае успешного завершения запускает скрипт `/bin/startApache.sh`:
```
#!/bin/sh
set -x
rm -f /var/run/httpd2/httpd.pid;
if [ -z "$MODULES" ]
then
  MODULES="rewrite ssl deflate filter"
fi

for module in $MODULES
do
  a2enmod $module
done

/usr/sbin/httpd2 -D NO_DETACH -k start

if [ -n  "$BOOTUP_CHECK_URL" ]
then
  until wget -c $BOOTUP_CHECK_URL >/dev/null  2>&1
  do
    echo "Wait for start up apache service"
    sleep 1;
  done
fi

/usr/sbin/httpd2 -D NO_DETACH -k start
```
Данный скрипт загружает указанные модули WEB-сервера `apache2:2.4.38` и запускает сам WEB-сервис, 
который инициирует запуск сервиса `mono`.

Если в дочерних образах необходимо запустить дополнительные сервисы необходимо  поместить в `/bin/startApache.sh` подобраза собственный вариант скрипта. 


## Пример

Рассмотрим корректировку XML-файла конфигурации  `/var/www/web-api/app/Web.config` вида: 
```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <appSettings>
    <add key="DefaultConnectionStringName" value="DefConnStr" />
    <add key="ActivityServicesApiUrl" value="%%ACTIVITY_SERVICES_API_URL%%" />
  </appSettings>
  <connectionStrings>
    <add name="DefConnStr" connectionString="%%BPM_CONNECTION_STRING%%" />
    <add name="AgentSyncConnStr" connectionString="%%DMS_CONNECTION_STRING%%" />
  </connectionStrings>
  <quartz>
    <add key="quartz.scheduler.instanceName" value="FlowpointFlexberryTimerClient" />
    <add key="quartz.scheduler.instanceId" value="AUTO" />
    <add key="quartz.scheduler.proxy" value="true" />
    <add key="quartz.scheduler.proxy.address" value="%%BPM_TIMER_URL%%" />
    <add key="quartz.threadPool.type" value="Quartz.Simpl.SimpleThreadPool, Quartz" />
    <add key="quartz.threadPool.threadCount" value="0" />
  </quartz>
</configuration>
```

Имя корректируемого файла конфигурации указывается в переменной `XMLTEMPLATES` файла `Dockerfile` при создании подобраза:
  ```
  FROM flexberry/alt.p8-apache2-mono:4.6.2.7-1.3
  ...
  ENV XMLTEMPLATES "/var/www/web-api/app/Web.config"
  ENV ACTIVITY_SERVICES_API_URL=http://localhost:1234/...
  ...
  ```
Значения переменных 
`ACTIVITY_SERVICES_API_URL`, `JBPM_API_URL`,  `BPM_CONNECTION_STRING`, `DMS_CONNECTION_STRING`, `BPM_TIMER_URL`указываются в YML-файле описания сервиса:
```
services:
  monoservice:
    image: ...
      environment:
        - ACTIVITY_SERVICES_API_URL=http://...
        - JBPM_API_URL=http://...
        - BPM_CONNECTION_STRING=Server=SrvBPM;Port=5432;...
        - DMS_CONNECTION_STRING=Server=SrvDMS;Port=5432;...
        - BPM_TIMER_URL=http://...
  ```
