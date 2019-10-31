# Description

Based on the [openjdk: 8-slim] image (https://hub.docker.com/_/openjdk).

## Functional

- For correct support of exporting Cyrillic reports to PDF, [Microsoft TrueType fonts] are installed (https://packages.debian.org/ru/sid/ttf-mscorefonts-installer).
- Removed files for creating default data in the launched `Pentaho Server`.
- Removed the task of periodically checking for updates.
- Removed the drop-down menu with test users from the login screen in `Pentaho Server`.
- Provided the ability to set the names of passwords and the role of new users with the removal of standard users `joe`,` pat`, `suzy`,` tiffany`;
- Provided the ability to change the password of the user `admin`
- Added database drivers: `postgresql-42.2.5`,` clickhouse-0.1.50` (the driver is compiled when creating an image);
- The image allows you to use the built-in base `HSQLDB` as an administrative database, as well as using an external relational database (currently` postgresql`) in `production`.
- *TODO:* Add support for Oracle databases, MySQL, ClickHouse, ...

Cyrillic fonts supported when exporting to PDF:
- `Andale Mono` Normal, Normal
- `Arial Black` Normal, Normal
- `Arial` Italics, Cursiva
- `Arial` Normal, Normal
- `Arial` Bold, Negreta
- `Arial` Bold Italic, Negreta cursiva
- `Comic Sans MS` Normal, Normal
- `Comic Sans MS` Bold, Negreta
- `Courier New` Italics, Cursiva
- `Courier New` Normal, Normal
- `Courier New` Bold, Negreta
- `Courier New` Bold Italic, Negreta cursiva
- `Georgia` Italics, Cursiva
- `Georgia` Normal, Normal
- `Georgia` Bold, Negreta
- `Georgia` Bold Italic, Negreta cursiva
- `Impact` Normal, Normal
- `Times New Roman` Italics, cursiva
- `Times New Roman` Normal, Normal
- `Times New Roman` Bold, Negreta
- `Times New Roman` Bold Italic, Negreta cursiva
- `Trebuchet MS` Italics, Cursiva
- `Trebuchet MS` Normal, Normal
- `Trebuchet MS` Bold, Negreta
- `Trebuchet MS` Bold Italic, Negreta cursiva
- `Verdana` Italics, Cursiva
- `Verdana` Normal, Normal
- `Verdana` Bold, Negreta
- `Verdana` Bold Italic, Negreta cursiva



By default the `Pentaho server` is available on the port` 8080`.

## Running

The type of administrative base is set by the environment variable `DB_ADMIN`.
If the variable is not set, the built-in base `HSQLDB` is used.

If the variable is set to `postgresql`, the Postgres database is used as the administrative base.

### Administrative Database Settings

Connection parameters to `PostgreSQL` are set by the environment variables:
- `DB_HOST` - server address with PostgreSQL (default` postgres`);
- `DB_PORT` - server port with PostgreSQL (default` 5432`);
- `DB_ADMIN_USER` is the database user name with administrator rights (the default is` postgres`);
- `DB_ADMIN_PASS` - DB user password with administrator rights (the default is` p @ ssw0rd`);
- `JCR_DB_NAME` is the database name of the Jackrabbit repository (the default is` jackrabbit`);
- `JCR_USER` - name of DB user for access to the database of the Jackrabbit repository (the default is `jcr_user`);
- `JCR_PASS` - password for the user` jcr_user` for access to the database of the Jackrabbit repository (the default is `password`);
- `HIBERNATE_DB_NAME` is the name of the Hibernate database (the default is` hibernate`);
- `HIBERNATE_PASS` - password for the` hibuser` user to access the `Hibernate` database (default is` password`);
- `HIBERNATE_USER` - name of DB user for access to the database of the `Hibernate` database (the default is `hibuser`);
- `QUARTZ_DB_NAME` - Quartz database name (default` quartz`);
- `QUARTZ_USER` - name of DB user for access to the database of the Quartz database (the default is `pentaho_user`);
- `QUARTZ_PASS` - password for the` pentaho_user` user for access to the Quartz database (default `password`).

*It is recommended to delete the databases and users listed above from the database before launching them*

### Set up authorization and authentication

When launching an image, you can:
- delete default users;
- specify the list of `pentaho` users , their passwords and roles;
- replace the user password admin `admin`.

#### Deleting default users

If the variable `DELETE_DEFAULT_USERS` has any value, then default users are being deleted from `pentaho`.

#### Defining user list

The variable `USERS` defines the list of pentaho users, their passwords and roles.
The format of this variable is:
```
<userNameA>:<passwordA>/<RoleA1>[,...<RoleAN>][\n<userNameB>:<passwordB>/<RoleB1>[,...<RoleBN>]]
```
The username and password are separated by a colon (`:`). The list of user roles is specified after the slash `/` and is separated by commas.

*It is not recommended to use XML formatting symbols in usernames and passwords: `>`, `<`, `&`, ...*

The delimiter between user descriptions is the newline character `\n`.

This image supports the following roles:
- `Administrator` - administrative functions;
- `Power User` - location, execution of reports;
- `Business Analyst` - execution of reports;
- `Report Author` - placement of reports;
- `Anonymous` - user with minimal rights

*Spaces in role names are required!*

Example description of the user `user` with minimal rights:
```
USERS=user:12345/Anonymous
```
Example description of three users:
```
USERS=power:password/Power User\nreporter:qwerty/Business Analyst, Report Author\nuser:12345/Anonymous
```

*If the variable `USERS` is not empty, standard users` joe`, `pat`,` suzy`, `tiffany` are deleted from the system*

#### Replacing admin user password

If there is a non-empty variable `ADMINPASSWORD`, after the server` pentaho` is initialized, the password of the `admin` user is changed to the specified string.
For example:
```
ADMINPASSWORD=qwerty
```

### Setting java-machine parameters

To optimize the operation and more fine-tuning the server for the current configuration, the image supports
setting the parameters of the java-machine through the variable `BI_JAVA_OPTS`. The specified parameters are passed to the `tomcat-server 'startup script that provides the` pentaho` server operation.

### Starting the server in docker-compose mode

An example of the description file for the variables `.env` and the configuration file` docker-compose.yml` is shown below.

Example variable description file `.env`:
```
BI_IMAGE_TAG=:8.2

#BI_JAVA_OPTS="-Xms4096m -Xmx6144m -XX: MaxMetaspaceSize=256m -Djava.security.egd=file: / dev /./ urandom -Dsun.rmi.dgc.client.gcInterval=3600000 -Dsun.rmi.dgc.c.client.gcInterval=3600000 -Dsun.rmi.dgc.cli.cmi.dgc.client.gcInterval=3,600,000 gcInterval=3,600,000 -Dfile.encoding=utf8 -DDI_HOME=\"$ DI_HOME\""
BI_JAVA_OPTS=

# DB_ADMIN=postgresql
# DB_HOST=postgres
# DB_PORT=5432
# DB_ADMIN_USER=postgres
# DB_ADMIN_PASS=p@ssw0rd
# JCR_DB_NAME=jackrabbit
# JCR_PASS=password
# HIBERNATE_DB_NAME=hibernate
# HIBERNATE_PASS=password
# QUARTZ_DB_NAME=quartz
# QUARTZ_PASS=password

DB_ADMIN=
DB_HOST=
DB_PORT=
DB_ADMIN_USER=
DB_ADMIN_PASS=
JCR_DB_NAME=
JCR_PASS=
HIBERNATE_DB_NAME=
HIBERNATE_PASS=
QUARTZ_DB_NAME=
QUARTZ_PASS=

SERVER_HTTP_PORT=8080

# USERS=power:password/Power User\nreporter:qwerty/Business Analyst,Report Author\nuser:12345/Anonymous
USERS=

# ADMINPASSWORD=qwerty
ADMINPASSWORD=
```

Example configuration file `docker-compose.yml`:
```
version: '3.2'

services:
  pentaho:
    image: flexberry/pentaho-official$BI_IMAGE_TAG
    ports:
      - "${SERVER_HTTP_PORT}:8080"
    volumes:
      - hsqldb: /biserver-ce/data/hsqldb/
      - repository: /biserver-ce/pentaho-solutions/system/jackrabbit/repository
      - logs: /biserver-ce/tomcat/logs
      - hidden: /biserver-ce/.pentaho/
      - tmp: /biserver-ce/tmp

    #extra_hosts:
      # - "postgres: 10.130.2.87"

    environment:
      BI_JAVA_OPTS: '${BI_JAVA_OPTS}'
      DELETE_DEFAULT_USERS: ${DELEFE_DEFAULT_USERS}
      USERS: '${USERS}'
      ADMINPASSWORD: ${ADMINPASSWORD}
      DB_ADMIN: ${DB_ADMIN}
      DB_HOST: ${DB_HOST}
      DB_PORT: ${DB_PORT}
      DB_ADMIN_USER: ${DB_ADMIN_USER}
      DB_ADMIN_PASS: ${DB_ADMIN_PASS}
      JCR_DB_NAME: ${JCR_DB_NAME}
      JCR_PASS: ${JCR_PASS}
      HIBERNATE_DB_NAME: ${HIBERNATE_DB_NAME}
      HIBERNATE_PASS: ${HIBERNATE_PASS}
      QUARTZ_DB_NAME: ${QUARTZ_DB_NAME}
      QUARTZ_PASS: ${QUARTZ_PASS}

volumes:
  hsqldb:
  repository:
  logs:
  tmp:
  hidden:
```
Putting part of the directories into the named volumes `hsqldb`,` repository`, `logs`,` hidden`, `tmp` ensures that the current pentaho data is saved when the image is restarted.

If necessary, the named volumes `logs`,` hidden`, `tmp` can be omitted.

Starting the `pentaho server` in` docker-compose` mode is done with the command:
```
$ docker-compose up -d
```

stop command:
```
$ docker-compose down
```

### Start server in swarm mode

Starting the `pentaho server` in` swarm` mode is done with the commands:
```
$ docker-compose config | docker stack deploy -c - PENTAHO
```

Where `PENTAHO` is the name of the service stack.
