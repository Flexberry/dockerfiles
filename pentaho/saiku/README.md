# Flexberry/pentaho

This image is based on Dockerfile  [zhicwu/biserver-ce:8.0 Dockerfile](https://hub.docker.com/r/zhicwu/biserver-ce/)

The image provides additional functionality:
- deeper Russification of the Saiku OLAP interface;
- the ability to work with the NoSQL database ClickHouse (ClickHouse 0.1 driver);


## Running an image in different modes

This image provides the ability to run in three main modes:
- container;
- docker-compose service;
- docker-swarm service

Configuration variables to run the image in all specified modes are placed in the file `.env`:
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
If necessary, you can make adjustments to these variables.
Generally, adjustments are required for port relocations
(`SERVER_HTTP_PORT`,
`SERVER_AJP_PORT`,
`SERVER_EXT_PORT`,
`SERVER_PROM_PORT` variables)
if standard ports are already occupied by another service running.

The variable DISABLED_TOOLS contains a list of disabled (experimental) tools. Currently it is:
- saikuplus - business graphics saikuplyus;
- exportpdf - export tables to PDF.

The delimiter is a symbol `-`.


## Run in container mode

To run into an image in container mode, you can use the script `runContainer.sh`:
```sh
#!/bin/sh
. .env

docker run -d \
  --name $BI_CONTAINER_NAME \
  -p "${SERVER_PROM_PORT}:1234" \
  -p "${SERVER_HTTP_PORT}:8080" \
  -p "${SERVER_AJP_PORT}:8009" \
  -vhidden:/biserver-ce/.pentaho/ \
  -v hsqldb:/biserver-ce/data/hsqldb/ \
  -v logs:/biserver-ce/tomcat/logs \
  -v repository:/biserver-ce/pentaho-solutions/system/jackrabbit/repository \
  -v tmp:/biserver-ce/tmp \
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
  flexberry/pentaho-saiku$BI_IMAGE_TAG
```

For all modifiable files and directories of the container, at initial startup, the named volumes are created:
- hidden - hidden pentaho server files;
- hsqldb - internal hsql database to store current settings;
- repository - user file system;
- logs - server logs;
- tmp - temporarty files.

When restarting, the image uses data from the specified named volumes.
Thus, the user file and current settings are saved when the container is restarted.

To stop the container, use the command:
```sh
$ docker stop bi_pentaho

```
To resume work:

```sh
$ docker start bi_pentaho

```
To remove a (working) container:
```sh
$ docker rm -f bi_pentaho

```


## Run in docker-compose service mode

Use the configuration file `docker-compose.yml` to run the image in the docker-compose service mode:
```
version: '3.2'

services:
  pentaho:
    image: flexberry/pentaho-saiku
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

Run service by command:
```sh
$ docker-compose up -d
```

For all modifiable files and directories of the container, at initial startup, the named volumes are created:
- pentaho_hidden - hidden pentaho server files;
- pentaho_hsqldb - internal hsql database to store current settings;
- pentaho_repository - user file system;
- pentaho_logs - server logs;
- pentaho_tmp - temporarty files.

When restarting, the image uses data from the specified named volumes.
Thus, the user file and current settings are saved when the container is restarted.

To stop the service, use the command:
```sh
$ docker-compose down
```

In case the `pentaho` service should work on the server with other docker-services (for example, Postgres, MySQL, ClickHouse databases, ...)
description of additional services can be added to the file `docker-compose.yml`.


## Start in docker swarm service mode

To run in cluster mode, you can use the above file `docker-compose.yml`.

Run service by command:
```sh
docker-compose config | docker stack deploy -c - pentaho
```
where `pentaho` - name of service stack.

The `docker-compose config` command configures the` docker-compose.yml` file according to the variables described in the `.env` file.
The configured configuration file is passed to the `docker stack deploy` command to deploy the service stack.

For all modifiable files and directories of the container, at initial startup, the named volumes are created:
- pentaho_hidden - hidden pentaho server files;
- pentaho_hsqldb - internal hsql database to store current settings;
- pentaho_repository - user file system;
- pentaho_logs - server logs;
- pentaho_tmp - temporarty files.

These names coincide with the names of the volumes used when running in  mode docker-compose service.
Thus, they can be used in  both launch modes.

When restarting, the image uses data from the specified named volumes.
Thus, the user file and current settings are saved when the container is restarted.

To stop the stack of services, use the command:
```sh
$ docker stack rm pentaho
```

In case the `pentaho` service should work in a cluster with other docker-services (for example, Postgres, MySQL, ClickHouse databases, ...)
A description of additional services can be added to the `docker-compose.yml` file.


