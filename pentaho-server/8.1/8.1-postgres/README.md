# Description
Based on Flexberry/pentaho-8.1

Flexberry/pentaho-8.1-postgres image contains [Pentaho Server 8.1](https://sourceforge.net/projects/pentaho/files/Pentaho%208.1/server/), working with PostgreSQL. Modifications are described in readme file for base image.

Pentaho Server is exposed on 8080 port.

## Runnibg
Settings to connect to PostgreSQL should be provided:
- DB_HOST - PostgreSQL host
- DB_PORT - port
- DB_ADMIN_USER - db admin username
- DB_ADMIN_PASS - db admin password
- JCR_PASS - password that will be used for jackrabbit db user
- HIBERNATE_PASS - password that will be used for hibernate db user
- QUARTZ_PASS - password that will be used for quartz db user

jackrabbit, hibernate, quartz databases are created automatically on container start.