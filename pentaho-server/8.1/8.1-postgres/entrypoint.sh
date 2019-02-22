#!/bin/bash
set -e

DB_ADMIN_USER=${DB_ADMIN_USER:-postgres}
DB_ADMIN_PASS=${DB_ADMIN_PASS:-p@ssw0rd}
JCR_DB_NAME=${JCR_DB_NAME:-jackrabbit}
JCR_PASS=${JCR_PASS:-password}
HIBERNATE_DB_NAME=${HIBERNATE_DB_NAME:-hibernate}
HIBERNATE_PASS=${HIBERNATE_PASS:-password}
QUARTZ_DB_NAME=${QUARTZ_DB_NAME:-quartz}
QUARTZ_PASS=${QUARTZ_PASS:-password}
DB_HOST=${DB_HOST:-postgres}
DB_PORT=${DB_PORT:-5432}

function wait_database() {
  echo -n "-----> waiting for database server on $DB_HOST:$DB_PORT ..."
  while ! nc -w 1 $DB_HOST $DB_PORT 2>/dev/null
  do
    echo -n .
    sleep 1
  done

  echo '[OK]'
}

function setup_database() {
  echo "-----> setup databases"
  echo "DB_ADMIN_USER: ${DB_ADMIN_USER}"
  echo "DB_ADMIN_PASS: ${DB_ADMIN_PASS}"
  echo "JCR_DB_NAME: ${JCR_DB_NAME}"
  echo "JCR_PASS: ${JCR_PASS}"
  echo "HIBERNATE_DB_NAME: ${HIBERNATE_DB_NAME}"
  echo "HIBERNATE_PASS: ${HIBERNATE_PASS}"
  echo "QUARTZ_DB_NAME: ${QUARTZ_DB_NAME}"
  echo "QUARTZ_PASS: ${QUARTZ_PASS}"
  echo "DB_HOST: ${DB_HOST}"
  echo "DB_PORT: ${DB_PORT}"

  wait_database

  repository=$PENTAHO_HOME/configs/repository.xml
  echo "-----> altering configuration file ${repository}"
  sed -i "s/\*\*port\*\*/${DB_PORT}/g" ${repository}
  sed -i "s/\*\*host\*\*/${DB_HOST}/g" ${repository}
  sed -i "s/\*\*jcr_db_name\*\*/${JCR_DB_NAME}/g" ${repository}
  sed -i "s/\*\*jcr_password\*\*/${JCR_PASS}/g" ${repository}
  cp -fv ${repository} \
    $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/jackrabbit/repository.xml

  context=$PENTAHO_HOME/configs/context.xml
  echo "-----> altering configuration file ${context}"
  sed -i "s/\*\*port\*\*/${DB_PORT}/g" ${context}
  sed -i "s/\*\*host\*\*/${DB_HOST}/g" ${context}
  sed -i "s/\*\*hib_db_name\*\*/${HIBERNATE_DB_NAME}/g" ${context}
  sed -i "s/\*\*hib_password\*\*/${HIBERNATE_PASS}/g" ${context}
  sed -i "s/\*\*quartz_db_name\*\*/${QUARTZ_DB_NAME}/g" ${context}
  sed -i "s/\*\*quartz_password\*\*/${QUARTZ_PASS}/g" ${context}
  cp -fv ${context} \
    $PENTAHO_HOME/pentaho-server/tomcat/webapps/pentaho/META-INF/context.xml

  hibprop=$PENTAHO_HOME/configs/applicationContext-spring-security-hibernate.properties
  echo "-----> altering configuration file ${hibprop}"
  sed -i "s/\*\*port\*\*/${DB_PORT}/g" ${hibprop}
  sed -i "s/\*\*host\*\*/${DB_HOST}/g" ${hibprop}
  sed -i "s/\*\*hib_db_name\*\*/${HIBERNATE_DB_NAME}/g" ${hibprop}
  sed -i "s/\*\*hib_password\*\*/${HIBERNATE_PASS}/g" ${hibprop}
  cp -fv ${hibprop} \
    $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/applicationContext-spring-security-hibernate.properties

  jdbc=$PENTAHO_HOME/configs/jdbc.properties
  echo "-----> altering configuration file ${jdbc}"
  sed -i "s/\*\*port\*\*/${DB_PORT}/g" ${jdbc}
  sed -i "s/\*\*host\*\*/${DB_HOST}/g" ${jdbc}
  sed -i "s/\*\*hib_db_name\*\*/${HIBERNATE_DB_NAME}/g" ${jdbc}
  sed -i "s/\*\*hib_password\*\*/${HIBERNATE_PASS}/g" ${jdbc}
  sed -i "s/\*\*quartz_db_name\*\*/${QUARTZ_DB_NAME}/g" ${jdbc}
  sed -i "s/\*\*quartz_password\*\*/${QUARTZ_PASS}/g" ${jdbc}
  cp -fv ${jdbc} \
    $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/simple-jndi/jdbc.properties

  sed -i 's/hsql/postgresql/g' \
    $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/hibernate/hibernate-settings.xml

  hibpgcfg=$PENTAHO_HOME/configs/postgresql.hibernate.cfg.xml
  echo "-----> altering configuration file ${hibpgcfg}"
  sed -i "s/\*\*port\*\*/${DB_PORT}/g" ${hibpgcfg}
  sed -i "s/\*\*host\*\*/${DB_HOST}/g" ${hibpgcfg}
  sed -i "s/\*\*hib_db_name\*\*/${HIBERNATE_DB_NAME}/g" ${hibpgcfg}
  sed -i "s/\*\*hib_password\*\*/${HIBERNATE_PASS}/g" ${hibpgcfg}
  cp -fv ${hibpgcfg} \
    $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/hibernate/postgresql.hibernate.cfg.xml

  export PGPASSWORD=$DB_ADMIN_PASS

  if ! psql -lqt -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT | grep -w $JCR_DB_NAME; then
    echo "-----> creating database ${JCR_DB_NAME}"

    scriptj=$PENTAHO_HOME/pentaho-server/data/postgresql/create_jcr_postgresql.sql
    echo "-----> altering script ${scriptj}"

	  sed -i "s/jackrabbit/${JCR_DB_NAME}/g" ${scriptj}
	  sed -i "s/'password'/'${JCR_PASS}'/g" ${scriptj}
	
    echo "-----> executing script ${scriptj}"
    psql -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT -f ${scriptj}
  fi

  if ! psql -lqt -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT | grep -w $HIBERNATE_DB_NAME; then
    echo "-----> creating database ${HIBERNATE_DB_NAME}"

    scripth=$PENTAHO_HOME/pentaho-server/data/postgresql/create_repository_postgresql.sql
    echo "-----> altering script ${scripth}"

	  sed -i "s/hibernate/${HIBERNATE_DB_NAME}/g" ${scripth}
	  sed -i "s/'password'/'${HIBERNATE_PASS}'/g" ${scripth}

    echo "-----> executing script ${scripth}"
    psql -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT -f ${scripth}
  fi

  if ! psql -lqt -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT | grep -w $QUARTZ_DB_NAME; then
    echo "-----> creating database ${QUARTZ_DB_NAME}"

    scriptq=$PENTAHO_HOME/pentaho-server/data/postgresql/create_quartz_postgresql.sql
    echo "-----> altering script ${scriptq}"

	  sed -i "s/quartz/${QUARTZ_DB_NAME}/g" ${scriptq}
	  sed -i "s/'password'/'${QUARTZ_PASS}'/g" ${scriptq}
	  sed -i "s/connect ${QUARTZ_DB_NAME} pentaho_user/connect ${QUARTZ_DB_NAME}/g" ${scriptq}
	
    echo "-----> executing script ${scriptq}"
    psql -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT -f ${scriptq}
  
    # http://jira.pentaho.com/browse/BISERVER-10639
    # https://github.com/wmarinho/docker-pentaho/blob/5.3/config/postgresql/biserver-ce/data/postgresql/create_quartz_postgresql.sql#L37
    psql -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT $QUARTZ_DB_NAME -c 'CREATE TABLE "QRTZ" ( NAME VARCHAR(200) NOT NULL, PRIMARY KEY (NAME) );'
  fi
  
  unset PGPASSWORD
}

function setup_tomcat() {
  echo "-----> setup webserver"

  rm -rf "$PENTAHO_HOME/pentaho-server/tomcat/conf/Catalina/*"
  rm -rf "$PENTAHO_HOME/pentaho-server/tomcat/temp/*"
  rm -rf "$PENTAHO_HOME/pentaho-server/tomcat/work/*"

  cp -fv $PENTAHO_HOME/configs/web.xml \
    $PENTAHO_HOME/pentaho-server/tomcat/webapps/pentaho/WEB-INF/web.xml
}

if [ "$1" = 'run' ]; then
  setup_tomcat
  setup_database

  echo "-----> starting pentaho"
  $PENTAHO_HOME/pentaho-server/start-pentaho.sh
else
  exec "$@"
fi