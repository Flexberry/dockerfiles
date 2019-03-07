# Description
Based on [openjdk:8-slim](https://hub.docker.com/_/openjdk).

## Features

- To correctly support the export of Cyrillic reports in PDF  [Microsoft TrueType fonts](https://packages.debian.org/ru/sid/ttf-mscorefonts-installer) installed
- Sample data creation files removed
- Updates checking task removed
- Sample users dropdown removed from Pentaho Server login screen
- The image allows you to use both the built-in HSQLDB database as an administrative database and, when used in production, an external relation database (postgresql).
- *TODO:* Add support additional databases Oracle, MySQL, ClickHouse, ...


## Running

The type of administrative base is set by the environment variable DB_ADMIN.
If the variable is not set, the built-in base HSQLDB is used.

If the variable is set to `postgresql`, the Postgres database is used as the administrative base.

PostgreSQL connection parameters are set by the environment variables:
- DB_HOST — server address with PostgreSQL
- DB_PORT - server port with PostgreSQL
- DB_ADMIN_USER - username with administrator rights
- DB_ADMIN_PASS - password of the user with administrator rights
- JCR_PASS - password for the user to access the database of the Jackrabbit repository
- HIBERNATE_PASS - password for the user to access the Hibernate database
- QUARTZ_PASS - password for the user to access the Quartz database

By default, Pentaho server is available on port 8080.
