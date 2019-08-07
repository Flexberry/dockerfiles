#!/bin/bash
set -e
set -x

export DB_ADMIN=${DB_ADMIN:-hsql}
export DB_ADMIN_USER=${DB_ADMIN_USER:-postgres}
export DB_ADMIN_PASS=${DB_ADMIN_PASS:-p@ssw0rd}
export JCR_DB_NAME=${JCR_DB_NAME:-jackrabbit}
export JCR_USER=${JCR_USER:-jcr_user}
export JCR_PASS=${JCR_PASS:-password}
export HIBERNATE_DB_NAME=${HIBERNATE_DB_NAME:-hibernate}
export HIBERNATE_USER=${HIBERNATE_USER:-hibuser}
export HIBERNATE_PASS=${HIBERNATE_PASS:-password}
export QUARTZ_DB_NAME=${QUARTZ_DB_NAME:-quartz}
export QUARTZ_USER=${QUARTZ_USER:-pentaho_user}
export QUARTZ_PASS=${QUARTZ_PASS:-password}
export DB_HOST=${DB_HOST:-postgres}
export DB_PORT=${DB_PORT:-5432}


setup_database() {
  echo "-----> setup databases"
  echo "DB_ADMIN: ${DB_ADMIN}"
  echo "DB_ADMIN_USER: ${DB_ADMIN_USER}"
  echo "DB_ADMIN_PASS: ${DB_ADMIN_PASS}"
  echo "JCR_DB_NAME: ${JCR_DB_NAME}"
  echo "JCR_USER: ${JCR_USER}"
  echo "JCR_PASS: ${JCR_PASS}"
  echo "HIBERNATE_DB_NAME: ${HIBERNATE_DB_NAME}"
  echo "HIBERNATE_USER: ${HIBERNATE_USER}"
  echo "HIBERNATE_PASS: ${HIBERNATE_PASS}"
  echo "QUARTZ_DB_NAME: ${QUARTZ_DB_NAME}"
  echo "QUARTZ_USER: ${QUARTZ_USER}"
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
        sed -i "s/jcr_user/${JCR_USER}/g" ${scriptj}
        sed -i "s/'password'/'${JCR_PASS}'/g" ${scriptj}
        echo "-----> executing script ${scriptj}"
        $doSQL ${scriptj}
        ;;

      $HIBERNATE_DB_NAME)
        echo "-----> creating database ${HIBERNATE_DB_NAME}"
        scripth=$PENTAHO_HOME/pentaho-server/data/$CREATE_REPOSITORY_DB
        echo "-----> altering script ${scripth}"
        sed -i "s/hibernate/${HIBERNATE_DB_NAME}/g" ${scripth}
        sed -i "s/hibuser/${HIBERNATE_USER}/g" ${scripth}
        sed -i "s/'password'/'${HIBERNATE_PASS}'/g" ${scripth}
        echo "-----> executing script ${scripth}"
        $doSQL ${scripth}
        ;;

      $QUARTZ_DB_NAME)
        echo "-----> creating database ${QUARTZ_DB_NAME}"
        scriptq=$PENTAHO_HOME/pentaho-server/data/postgresql/create_quartz_postgresql.sql
        echo "-----> altering script ${scriptq}"
        sed -i "s/quartz/${QUARTZ_DB_NAME}/g" ${scriptq}
        sed -i "s/pentaho_user/${QUARTZ_USER}/g" ${scriptq}
        sed -i "s/'password'/'${QUARTZ_PASS}'/g" ${scriptq}
        sed -i "s/connect ${QUARTZ_DB_NAME} ${QUARTZ_USER}/connect ${QUARTZ_DB_NAME}/g" ${scriptq}
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

  if [ ${DB_ADMIN} != 'hsql' ]
  then
    echo "Remove HsqldbStartupListener"
    xmlfile=$PENTAHO_HOME/pentaho-server/tomcat/webapps/pentaho/WEB-INF/web.xml
    xsltproc --novalid  -o $xmlfile $PENTAHO_HOME/configs/web.xslt $xmlfile
  fi
}

urlencode() {
  set +x
  length="${#1}"
  i=0
  while [ $i -lt $length ]
  do
    c="${1:i:1}"
    case $c in
      [a-zA-Z0-9.~_-]) printf "$c" ;;
      *) printf '%%%02X' "'$c"
    esac
    let i=$i+1
  done
  set -x
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
  xmlfile=$PENTAHO_HOME/pentaho-server/pentaho-solutions/system/pentaho.xml
  xsltproc --novalid  -o $xmlfile $PENTAHO_HOME/configs/pentaho_set.xslt $xmlfile
  xmlfile=$PENTAHO_HOME/pentaho-server/pentaho-solutions/system/applicationContext-spring-security-memory.xml
  xsltproc --novalid  -o $xmlfile $PENTAHO_HOME/configs/applicationContext_set.xslt $xmlfile

  if [ -n "$USERS" -o -n "$ADMINPASSWORD" ]
  then
    (
      xmlfile=$PENTAHO_HOME/pentaho-server/pentaho-solutions/system/defaultUser.spring.xml
      xsltproc --novalid  -o $xmlfile $PENTAHO_HOME/configs/defaultUser_set.xslt $xmlfile
      xmlfile=$PENTAHO_HOME/pentaho-server/pentaho-solutions/system/defaultUser.spring.xml
      xsltproc --novalid  -o $xmlfile $PENTAHO_HOME/configs/defaultUser_set.xslt $xmlfile
      sleep 10;
      export XMLFILE="/tmp/body.xml"
      until wget -O - --header='Authorization: Basic YWRtaW46cGFzc3dvcmQ=' --method PUT  'http://127.0.0.1:8080/pentaho/api/userroledao/deleteUsers?userNames=suzi%09pat%09tiffany'; do sleep 1; done
      if [ -n "$USERS" ]
      then
        echo -ne "$USERS\n" |
        while read str
        do
          ifs=$IFS
          IFS=/
          set -- $str
          userpass=$1
          shift
          roles=`echo $* | tr ',' '\t'`
          IFS=:
          set -- $userpass
          user=$1
          password=$2
          IFS=$ifs
          echo "<user><userName>$user</userName><password>$password</password></user>" > $XMLFILE
          URL="http://127.0.0.1:8080/pentaho/api/userroledao/createUser"
          until wget -O - --header='Authorization: Basic YWRtaW46cGFzc3dvcmQ=' --header='Content-type: application/xml' --method=PUT --body-file=$XMLFILE $URL; do sleep 1; done
          Roles=`urlencode "$roles"`
          URL="http://127.0.0.1:8080/pentaho/api/userroledao/assignRoleToUser?userName=$user&roleNames=$Roles"
          until wget -O - --header='Authorization: Basic YWRtaW46cGFzc3dvcmQ=' --method=PUT $URL; do sleep 1; done
        done
      fi
      if [ -n "$ADMINPASSWORD" ]
      then
        URL="http://127.0.0.1:8080/pentaho/api/userroledao/user"
        echo "<ChangePasswordUser><userName>admin</userName><newPassword>$ADMINPASSWORD</newPassword><oldPassword>password</oldPassword></ChangePasswordUser>" > $XMLFILE
        until wget -O - --header='Authorization: Basic YWRtaW46cGFzc3dvcmQ=' --header='Content-type: application/xml' --method=PUT --body-file=$XMLFILE $URL; do sleep 1; done
      fi
      rm -f $XMLFILE
    )&
  fi
  echo "-----> starting pentaho"
  if [ -n "$BI_JAVA_OPTS" ]
  then
    export CATALINA_OPTS="$BI_JAVA_OPTS"
  fi
  $PENTAHO_HOME/pentaho-server/start-pentaho.sh
else
  exec "$@"
fi
