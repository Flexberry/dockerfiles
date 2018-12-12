# Flexberry/pentaho

Даннай образ основан на Dockerfile  [zhicwu/biserver-ce:8.0 Dockerfile](https://hub.docker.com/r/zhicwu/biserver-ce/)

Образ обеспечивает дополнительный функционал:
- более глубокая русификация OLAP-интерфейса Saiku;
- возможность работы с NoSQL базой данных ClickHouse (ClickHouse 0.1 driver);


## Запуск образа в различных режимах

Для данного образа обеспечивается возможность запуска в трех основных режимах:
- контейнер;
- сервис docker-compose;
- сервис docker-swarm

Конфигурационные перемененные для запуска образа во всех указанных режимах находятся в файле `.env`:
```sh
BI_IMAGE_TAG=:latest
BI_CONTAINER_NAME=bi_pentaho

BI_JAVA_OPTS="-javaagent:./jmx-exporter.jar=1234:/dev/null -XX:+UseG1GC -XX:+UseStringDeduplication -Xms4096m -Xmx4096m -XX:+AlwaysPreTouch -XX:+ScavengeBeforeFullGC -XX:+DisableExplicitGC -XX:+PreserveFramePointer -Djava.security.egd=file:/dev/./urandom -Djava.awt.headless=true -Dpentaho.karaf.root.copy.dest.folder=../../tmp/osgi/karaf -Dpentaho.karaf.root.transient=false -XX:ErrorFile=../logs/jvm_error.log -verbose:gc -Xloggc:../logs/gc.log -XX:+PrintGCDetails -XX:+PrintGCTimeStamps -XX:+PrintGCDateStamps -XX:+PrintHeapAtGC -XX:+PrintAdaptiveSizePolicy -XX:+PrintStringDeduplicationStatistics -XX:+PrintTenuringDistribution -XX:+UseGCLogFileRotation -XX:NumberOfGCLogFiles=2 -XX:GCLogFileSize=64M -XX:OnOutOfMemoryError=/usr/bin/oom_killer -Dsun.rmi.dgc.client.gcInterval=3600000 -Dsun.rmi.dgc.server.gcInterval=3600000 -Dfile.encoding=utf8 -DDI_HOME=\"$DI_HOME\""

# add current user id(same as $UID) if you run with non-root user
HOST_USER_ID=

# leave this empty to use hsqldb or mysql5 if you prefer external database
STORAGE_TYPE=

LOCALE_LANGUAGE=ru
LOCALE_COUNTRY=RU

PDI_HADOOP_CONFIG=hdp25

PDI_MAX_LOG_LINES=10000
PDI_MAX_LOG_TIMEOUT=1440
PDI_MAX_OBJ_TIMEOUT=240

SERVER_NAME=pentaho-ce
SERVER_HOST=
SERVER_HTTP_PORT=8080
SERVER_AJP_PORT=8009
SERVER_EXT_PORT=443
SERVER_PROM_PORT=12345
#DISABLED_TOOLS=saikuplus-exportpdf
DISABLED_TOOLS=saikuplus
#DISABLED_TOOLS=""
```
В случае необходимости можно произвести корректировку указанных переменных.
В основном корректировки требуются для перенезначения портов (переменные
`SERVER_HTTP_PORT`,
`SERVER_AJP_PORT`,
`SERVER_EXT_PORT`,
`SERVER_PROM_PORT`)
в том случае, если стандартные порты уже заняты другим работающем сервисом.

Переменная DISABLED_TOOLS содержит список выключаемых (эксперементальных) инструментов. На настоящий момент это:
- saikuplus - деловая графика sailuplus;
- exportpdf - экспорт таблиц в PDF. 

Разделителем служит символ `-`.


## Запуск в режиме контейнера

Для запуска в образа в режиме контейнера можно воспользоваться скриптом `runContainer.sh`:
```sh
#!/bin/sh
. .env

docker run -d \
  --name $BI_CONTAINER_NAME \
  -p "${SERVER_PROM_PORT}:1234" \
  -p "${SERVER_HTTP_PORT}:8080" \
  -p "${SERVER_AJP_PORT}:8009" \
  -v  hidden:/biserver-ce/.pentaho/ \
  -v  hsqldb:/biserver-ce/data/hsqldb/ \
  -v  logs:/biserver-ce/tomcat/logs \
  -v  repository:/biserver-ce/pentaho-solutions/system/jackrabbit/repository \
  -v  tmp:/biserver-ce/tmp \
  -e APPLY_PATCHES='Y' \
  -e BI_JAVA_OPTS="${BI_JAVA_OPTS}" \
  -e HOST_USER_ID=${HOST_USER_ID} \
  -e STORAGE_TYPE=${STORAGE_TYPE} \
  -e PDI_HADOOP_CONFIG=${PDI_HADOOP_CONFIG} \
  -e PDI_MAX_LOG_LINES=${PDI_MAX_LOG_LINES} \
  -e PDI_MAX_LOG_TIMEOUT=${PDI_MAX_LOG_TIMEOUT} \
  -e PDI_MAX_OBJ_TIMEOUT=${PDI_MAX_OBJ_TIMEOUT} \
  -e SERVER_NAME=${SERVER_NAME} \
  -e SERVER_HOST=${SERVER_HOST} \
  -e SERVER_PORT=${SERVER_EXT_PORT} \
  flexberry/pentaho$BI_IMAGE_TAG
```

Для всех изменяемых файлов и каталогов контейнера при первоначальном запуске создаются именованые тома:
- hidden - скрытые файлы сервера pentaho;
- hsqldb - внутренняя база данных типа  hsql для хранения текущих настроек;
- repository - файловая система пользователей;
- logs - логи сервера;
- tmp - временные файлы сервера.

При повторных запусках образ использует данные из указанным именованых томов.
Таким образом файлы пользователя и текущие настройки сохраняются при перезапуске контейнера.

Для остановки работы контейнера воспользуйтесь командой:
```sh
$ docker stop bi_pentaho

```

Для возобновления работы:

```sh
$ docker start bi_pentaho

```

Для удаления (работающего) контейнера:
```sh
$ docker rm -f bi_pentaho

```


## Запуск в режиме сервиса docker-compose

Для запуска образа в режиме сервиса docker-compose воспользуйтесь файлом конфигурации `docker-compose.yml`:
```
version: '3.2'

services:
  pentaho:
    image: flexberry/pentaho
    command: /biserver-ce/start-pentaho.sh
    ports:
      - "${SERVER_PROM_PORT}:1234"
      - "${SERVER_HTTP_PORT}:8080"
      - "${SERVER_AJP_PORT}:8009"
    volumes:
      - hidden:/biserver-ce/.pentaho/
      - hsqldb:/biserver-ce/data/hsqldb/
      - logs:/biserver-ce/tomcat/logs
      - repository:/biserver-ce/pentaho-solutions/system/jackrabbit/repository
      - tmp:/biserver-ce/tmp

    environment:
      APPLY_PATCHES: 'Y'
      BI_JAVA_OPTS: '${BI_JAVA_OPTS}'
      HOST_USER_ID: ${HOST_USER_ID}
      STORAGE_TYPE: ${STORAGE_TYPE}
      PDI_HADOOP_CONFIG: ${PDI_HADOOP_CONFIG}
      PDI_MAX_LOG_LINES: ${PDI_MAX_LOG_LINES}
      PDI_MAX_LOG_TIMEOUT: ${PDI_MAX_LOG_TIMEOUT}
      PDI_MAX_OBJ_TIMEOUT: ${PDI_MAX_OBJ_TIMEOUT}
      SERVER_NAME: ${SERVER_NAME}
      SERVER_HOST: ${SERVER_HOST}
      SERVER_PORT: ${SERVER_EXT_PORT}

volumes:
  hidden:
  hsqldb:
  repository:
  logs:
  tmp:
```

Запуск производится командой:
```sh
$ docker-compose up -d
```

Для всех изменяемых файлов и каталогов контейнера при первоначальном запуске создаются именованые тома:
- pentaho_hidden - скрытые файлы сервера pentaho;
- pentaho_hsqldb - внутренняя база данных типа  hsql для хранения текущих настроек;
- pentaho_repository - файловая система пользователей;
- pentaho_logs - логи сервера;
- pentaho_tmp - временные файлы сервера.

При повторных запусках образ использует данные из указанным именованых томов.
Таким образов файлы пользователя и текущие настройки сохраняются при перезапуске контейнера.

Для остановки работы сервиса воспользуйтесь командой:
```sh
$ docker-compose down
```

В том случае, если сервис `pentaho` должен работать на сервере  с другими docker-сервисами (например базами данных Postgres, MySQL, ClickHouse, ...)
описание дополнительных сервисов может быть добавлено в файл `docker-compose.yml`.


## Запуск в режиме сервиса docker swarm

Для запуске в кластерном режиме swarm можно использовать опсанный выше файл `docker-compose.yml`.

Запуск произвоодится командой:
```sh
docker-compose config | docker stack deploy -c - pentaho
```
где `pentaho` - имя стека сервисов.

Команда `docker-compose config` производит настройку файла `docker-compose.yml` согласно переменным, описанным в файле `.env`.
Настроенный файл конфигурации передается команде `docker stack deploy` для разворачивания стека сервисов.

Для всех изменяемых файлов и каталогов контейнера при первоначальном запуске создаются именованые тома:
- pentaho_hidden - скрытые файлы сервера pentaho;
- pentaho_hsqldb - внутренняя база данных типа  hsql для хранения текущих настроек;
- pentaho_repository - файловая система пользователей;
- pentaho_logs - логи сервера;
- pentaho_tmp - временные файлы сервера.

Данные имена совпадают с именами томов, используемых при запуске в режиме docker-compose сервиса.
Таким образом они могут быть использованы во обоих режимах запуска образа.

При повторных запусках образ использует данные из указанным именованых томов.
Таким образов файлы пользователя и текущие настройки сохраняются при перезапуске контейнера.

Для остановки работы стека сервисов воспользуйтесь командой:
```sh
$ docker stack rm pentaho
```

В том случае, если сервис `pentaho` должен работать в кластере  с другими docker-сервисами (например базами данных Postgres, MySQL, ClickHouse, ...)
описание дополнительных сервисов может быть добавлено в файл `docker-compose.yml`.


