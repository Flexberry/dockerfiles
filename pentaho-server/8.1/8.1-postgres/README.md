# Description
Based on flexberry/pentaho:8.1

flexberry/pentaho:8.1-postgres image contains [Pentaho Server 8.1](https://sourceforge.net/projects/pentaho/files/Pentaho%208.1/server/), working with PostgreSQL. Modifications are described in readme file for base image.

Pentaho Server is exposed on 8080 port.

# Running
Settings to connect to PostgreSQL should be provided:
- DB_HOST - PostgreSQL host (default `postgres`)
- DB_PORT - port (default `5432`)
- DB_ADMIN_USER - db admin username (default `postgres`)
- DB_ADMIN_PASS - db admin password (default `p@ssw0rd`)
- JCR_DB_NAME - database name for jackrabbit (default `jackrabbit`)
- JCR_PASS - password that will be used for jackrabbit db user (default `password`)
- HIBERNATE_DB_NAME - database name for hibernate (default `hibernate`)
- HIBERNATE_PASS - password that will be used for hibernate db user (default `password`)
- QUARTZ_DB_NAME - database name for quartz (default `quartz`)
- QUARTZ_PASS - password that will be used for quartz db user (default `password`)

jackrabbit, hibernate, quartz databases are created automatically on container start, if not exist (separately).

# Example
```
docker run \
    --name pentaho \
    -p 8080:8080 \
    -e "DB_HOST=postgres" \
    -e "DB_PORT=5432" \
    -e "DB_ADMIN_USER=postgres" \
    -e "DB_ADMIN_PASS=p@ssw0rd" \
    -d flexberry/pentaho:8.1-postgres
```

```
docker service create \
    --name pentaho \
    -p 8080:8080 \
    -e "DB_HOST=postgres" \
    -e "DB_PORT=5432" \
    -e "DB_ADMIN_USER=postgres" \
    -e "DB_ADMIN_PASS=p@ssw0rd" \
    -e "JCR_DB_NAME=jackrabbit" \
    -e "JCR_PASS=password" \
    -e "HIBERNATE_DB_NAME=hibernate" \
    -e "HIBERNATE_PASS=password" \
    -e "QUARTZ_DB_NAME=quartz" \
    -e "QUARTZ_PASS=password" \
    flexberry/pentaho:8.1-postgres
```