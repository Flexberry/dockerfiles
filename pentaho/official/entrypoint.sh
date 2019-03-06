#!/bin/sh
set -e

export DB_ADMIN=${DB_ADMIN:-hsql}
export DB_ADMIN_USER=${DB_ADMIN_USER:-postgres}
export DB_ADMIN_PASS=${DB_ADMIN_PASS:-p@ssw0rd}
export JCR_DB_NAME=${JCR_DB_NAME:-jackrabbit}
export JCR_PASS=${JCR_PASS:-password}
export HIBERNATE_DB_NAME=${HIBERNATE_DB_NAME:-hibernate}
export HIBERNATE_PASS=${HIBERNATE_PASS:-password}
export QUARTZ_DB_NAME=${QUARTZ_DB_NAME:-quartz}
export QUARTZ_PASS=${QUARTZ_PASS:-password}
export DB_HOST=${DB_HOST:-postgres}
export DB_PORT=${DB_PORT:-5432}

# function wait_database() {
#   echo -n "-----> waiting for database server on $DB_HOST:$DB_PORT ..."
#   while ! nc -w 1 $DB_HOST $DB_PORT 2>/dev/null
#   do
#     echo -n .
#     sleep 1
#   done
#
#   echo '[OK]'
# }

setup_database() {
  echo "-----> setup databases"
  echo "DB_ADMIN: ${DB_ADMIN}"
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

  . $PENTAHO_HOME/configs/${DB_ADMIN}/environment.sh
  HIBERNATE_URL="jdbc:${JDBCType}://${DB_HOST}:${DB_PORT}/${HIBERNATE_DB_NAME}"
  QUARTZ_URL="jdbc:${JDBCType}://${DB_HOST}:${DB_PORT}/${QUARTZ_DB_NAME}"
  JCR_URL="jdbc:${JDBCType}://${DB_HOST}:${DB_PORT}/${JCR_DB_NAME}"

  templateXML="$PENTAHO_HOME/configs/${DB_ADMIN}/repository.xml"
  contentXML="$PENTAHO_HOME/pentaho-server/pentaho-solutions/system/jackrabbit/repository.xml"
  contentCopyXSLT="$PENTAHO_HOME/configs/repository_copy.xslt"
  contentSetXSLT="$PENTAHO_HOME/configs/repository_set.xslt"
  xsltproc  --novalid --param file "document('$templateXML')" -o ${contentXML} $contentCopyXSLT ${contentXML}
  xsltproc  --novalid \
    --stringparam  JCR_URL ${JCR_URL} \
    --stringparam  JCR_PASS ${JCR_PASS} \
    -o ${contentXML} $contentSetXSLT ${contentXML}

  templateXML="$PENTAHO_HOME/configs/${DB_ADMIN}/context.xml"
  contentXML="$PENTAHO_HOME/pentaho-server/tomcat/webapps/pentaho/META-INF/context.xml"
  contentCopyXSLT="$PENTAHO_HOME/configs/context_copy.xslt"
  contentSetXSLT="$PENTAHO_HOME/configs/context_set.xslt"
  xsltproc  --novalid --param file "document('$templateXML')" -o ${contentXML} $contentCopyXSLT ${contentXML}
  xsltproc  --novalid \
    --stringparam  HIBERNATE_URL ${HIBERNATE_URL} \
    --stringparam  HIBERNATE_PASS ${HIBERNATE_PASS} \
    --stringparam QUARTZ_URL ${QUARTZ_URL} \
    --stringparam  QUARTZ_PASS ${QUARTZ_PASS} \
    -o ${contentXML} $contentSetXSLT ${contentXML}

  contentXML="$PENTAHO_HOME/pentaho-server/pentaho-solutions/system/hibernate/hibernate-settings.xml"
  contentSetXSLT="$PENTAHO_HOME/configs/hibernate-settings_set.xslt"
  xsltproc  --novalid --stringparam DB_ADMIN ${DB_ADMIN} -o $contentXML $contentSetXSLT $contentXML

  contentXML="$PENTAHO_HOME/pentaho-server/pentaho-solutions/system/hibernate/${DB_ADMIN}.hibernate.cfg.xml"
  contentSetXSLT="$PENTAHO_HOME/configs/hibernate_set.xslt"
  xsltproc  --novalid  \
    --stringparam  HIBERNATE_URL ${HIBERNATE_URL} \
    --stringparam HIBERNATE_PASS ${HIBERNATE_PASS} \
    -o $contentXML $contentSetXSLT $contentXML




  echo "jdbc.driver=${Driver}
jdbc.url=jdbc:${UriType}://${DB_HOST}:/${DB_PORT}/${HIBERNATE_DB_NAME}
jdbc.username=hibuser
jdbc.password=${HIBERNATE_PASS}
hibernate.dialect=org.hibernate.dialect.${Dialect}
" > $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/applicationContext-spring-security-hibernate.properties

  echo "
Hibernate/type=javax.sql.DataSource
Hibernate/driver=${Driver}
Hibernate/url=jdbc:${UriType}://${DB_HOST}:${DB_PORT}/${HIBERNATE_DB_NAME}
Hibernate/user=hibuser
Hibernate/password=${HIBERNATE_PASS}
Quartz/type=javax.sql.DataSource
Quartz/driver=${Driver}
Quartz/url=jdbc:${UriType}://${DB_HOST}:${DB_PORT}/${QUARTZ_DB_NAME}
Quartz/user=pentaho_user
Quartz/password=${QUARTZ_PASS}
" >  $PENTAHO_HOME/pentaho-server/pentaho-solutions/system/simple-jndi/jdbc.properties

  missingTablesScript="$PENTAHO_HOME/configs/${DB_ADMIN}/missingTables.sh"
  doSQL="$PENTAHO_HOME/configs/${DB_ADMIN}/doSQL.sh"
  if [ -x $missingTablesScript -a -x $doSQL ]
  then
    missingTables=`$missingTablesScript $JCR_DB_NAME $HIBERNATE_DB_NAME $QUARTZ_DB_NAME`
    for missingTable in $missingTables
    do
      case $missingTable in
       $JCR_DB_NAME)
        echo "-----> creating database ${JCR_DB_NAME}"
        scriptj=$PENTAHO_HOME/pentaho-server/data/$CREATE_JCR_DB
        echo "-----> altering script ${scriptj}"
        sed -i "s/jackrabbit/${JCR_DB_NAME}/g" ${scriptj}
        sed -i "s/'password'/'${JCR_PASS}'/g" ${scriptj}
        echo "-----> executing script ${scriptj}"
        $doSQL ${scriptj}
        ;;

      $HIBERNATE_DB_NAME)
        echo "-----> creating database ${HIBERNATE_DB_NAME}"
        scripth=$PENTAHO_HOME/pentaho-server/data/$CREATE_REPOSITORY_DB
        echo "-----> altering script ${scripth}"
        sed -i "s/hibernate/${HIBERNATE_DB_NAME}/g" ${scripth}
        sed -i "s/'password'/'${HIBERNATE_PASS}'/g" ${scripth}
        echo "-----> executing script ${scripth}"
        $doSQL ${scripth}
        ;;

      $QUARTZ_DB_NAME)
        echo "-----> creating database ${QUARTZ_DB_NAME}"
        scriptq=$PENTAHO_HOME/pentaho-server/data/postgresql/create_quartz_postgresql.sql
        echo "-----> altering script ${scriptq}"
        sed -i "s/quartz/${QUARTZ_DB_NAME}/g" ${scriptq}
        sed -i "s/'password'/'${QUARTZ_PASS}'/g" ${scriptq}
        sed -i "s/connect ${QUARTZ_DB_NAME} pentaho_user/connect ${QUARTZ_DB_NAME}/g" ${scriptq}
        echo "-----> executing script ${scriptq}"
        echo $CREATE_QUARTZ_SQL >> ${scriptq};
        $doSQL ${scriptq}
        ;;
      esac
    done
  fi
}

setup_tomcat() {
  echo "-----> setup webserver"

  rm -rf "$PENTAHO_HOME/pentaho-server/tomcat/conf/Catalina/*"
  rm -rf "$PENTAHO_HOME/pentaho-server/tomcat/temp/*"
  rm -rf "$PENTAHO_HOME/pentaho-server/tomcat/work/*"

  if [ ${DM_ADMIN} != 'hsql' ]
  then
    echo "Remove HsqldbStartupListener"
    $xmlfile=$PENTAHO_HOME/pentaho-server/tomcat/webapps/pentaho/WEB-INF/web.xml
    xsltproc --novalid  -o $xmlfile $PENTAHO_HOME/configs/web.xslt $xmlfile
  fi
}

if [ "$1" = 'run' ]; then
  if [ -n ${DB_ADMIN} -a  ${DB_ADMIN} != 'hsql' ]
  then
    configDir=$PENTAHO_HOME/configs/${DB_ADMIN}
    if [ -d $configDir ]
    then
      setup_tomcat
      setup_database
    else
      echo "ADMINISTRATION DATA SETTINGS PENTAHO SERVER FOR BASE $ {DB_ADMIN} MISSING!";
      echo "STANDARD BASE hsql IS USED!"
    fi
  fi

  echo "-----> starting pentaho"
  $PENTAHO_HOME/pentaho-server/start-pentaho.sh
else
  exec "$@"
fi
