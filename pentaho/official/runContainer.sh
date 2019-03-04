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
