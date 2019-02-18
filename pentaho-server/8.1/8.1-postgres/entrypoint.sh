#!/bin/bash
set -e

DB_ADMIN_USER=${DB_ADMIN_USER:-postgres}
DB_ADMIN_PASS=${DB_ADMIN_PASS:-p@ssw0rd}
JCR_PASS=${JCR_PASS:-password}
HIBERNATE_PASS=${HIBERNATE_PASS:-password}
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
  echo "JCR_PASS: ${JCR_PASS}"
  echo "HIBERNATE_PASS: ${HIBERNATE_PASS}"
  echo "QUARTZ_PASS: ${QUARTZ_PASS}"
  echo "DB_HOST: ${DB_HOST}"
  echo "DB_PORT: ${DB_PORT}"

  wait_database

  sed -i "s/\*\*port\*\*/${DB_PORT}/g" $PENTAHO_HOME/configs/repository.xml 
  sed -i "s/\*\*host\*\*/${DB_HOST}/g" $PENTAHO_HOME/configs/repository.xml   
  sed -i "s/\*\*jcr_password\*\*/${JCR_PASS}/g" $PENTAHO_HOME/configs/repository.xml 
  cp -fv $PENTAHO_HOME/configs/repository.xml \
    $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/jackrabbit/repository.xml

  sed -i "s/\*\*port\*\*/${DB_PORT}/g" $PENTAHO_HOME/configs/context.xml 
  sed -i "s/\*\*host\*\*/${DB_HOST}/g" $PENTAHO_HOME/configs/context.xml   
  sed -i "s/\*\*hib_password\*\*/${HIBERNATE_PASS}/g" $PENTAHO_HOME/configs/context.xml   
  sed -i "s/\*\*quartz_password\*\*/${QUARTZ_PASS}/g" $PENTAHO_HOME/configs/context.xml 
  cp -fv $PENTAHO_HOME/configs/context.xml \
    $PENTAHO_HOME/pentaho-server/tomcat/webapps/pentaho/META-INF/context.xml

  sed -i "s/\*\*port\*\*/${DB_PORT}/g" $PENTAHO_HOME/configs/applicationContext-spring-security-hibernate.properties  
  sed -i "s/\*\*host\*\*/${DB_HOST}/g" $PENTAHO_HOME/configs/applicationContext-spring-security-hibernate.properties    
  sed -i "s/\*\*hib_password\*\*/${HIBERNATE_PASS}/g" $PENTAHO_HOME/configs/applicationContext-spring-security-hibernate.properties  
  cp -fv $PENTAHO_HOME/configs/applicationContext-spring-security-hibernate.properties \
    $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/applicationContext-spring-security-hibernate.properties

  sed -i "s/\*\*port\*\*/${DB_PORT}/g" $PENTAHO_HOME/configs/jdbc.properties  
  sed -i "s/\*\*host\*\*/${DB_HOST}/g" $PENTAHO_HOME/configs/jdbc.properties    
  sed -i "s/\*\*hib_password\*\*/${HIBERNATE_PASS}/g" $PENTAHO_HOME/configs/jdbc.properties    
  sed -i "s/\*\*quartz_password\*\*/${QUARTZ_PASS}/g" $PENTAHO_HOME/configs/jdbc.properties  
  cp -fv $PENTAHO_HOME/configs/jdbc.properties \
    $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/simple-jndi/jdbc.properties

  sed -i 's/hsql/postgresql/g' \
    $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/hibernate/hibernate-settings.xml

  sed -i "s/localhost/${DB_HOST}/g" $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/hibernate/postgresql.hibernate.cfg.xml 
  sed -i "s/5432/${DB_PORT}/g" $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/hibernate/postgresql.hibernate.cfg.xml  
  sed -i "s/>password</>${HIBERNATE_PASS}</g" $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/hibernate/postgresql.hibernate.cfg.xml

  export PGPASSWORD=$DB_ADMIN_PASS
  if ! psql -lqt -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT | grep -w hibernate; then
    echo "-----> creating databases"

	sed -i "s/connect quartz pentaho_user/connect quartz/g" $PENTAHO_HOME/pentaho-server/data/postgresql/create_quartz_postgresql.sql
	
    psql -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT -f $PENTAHO_HOME/pentaho-server/data/postgresql/create_jcr_postgresql.sql
    psql -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT -f $PENTAHO_HOME/pentaho-server/data/postgresql/create_quartz_postgresql.sql
    psql -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT -f $PENTAHO_HOME/pentaho-server/data/postgresql/create_repository_postgresql.sql
  
    # http://jira.pentaho.com/browse/BISERVER-10639
    # https://github.com/wmarinho/docker-pentaho/blob/5.3/config/postgresql/biserver-ce/data/postgresql/create_quartz_postgresql.sql#L37
    psql -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT quartz -c 'CREATE TABLE "QRTZ" ( NAME VARCHAR(200) NOT NULL, PRIMARY KEY (NAME) );'
  fi
  
  psql -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT -c "ALTER USER jcr_user WITH PASSWORD '${JCR_PASS}'"
  psql -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT -c "ALTER USER pentaho_user WITH PASSWORD '${QUARTZ_PASS}'"
  psql -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT -c "ALTER USER hibuser WITH PASSWORD '${HIBERNATE_PASS}'"
  
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