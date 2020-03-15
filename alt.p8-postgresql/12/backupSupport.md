# Включение бекапа в различных вариантах запуска образа

## Режим контейнера

## Архивироваие в файловую систему


## Архивирование в S3(minio)

## Запуск режима архивирования 

```
#!/bin/sh
docker run -d \
     -v 12_db:/var/lib/pgsql/data/ \
     -v 12_backups:/var/lib/pgsql/backups \
     -e WALG=s3 \
     -e WALE_S3_PREFIX=s3://pg-backups \
     -e AWS_ENDPOINT=http://192.168.100.6:29000 \
     -e AWS_ACCESS_KEY_ID=minio \
     -e AWS_SECRET_ACCESS_KEY=minio123 \
     flexberry/alt.p8-postgresql:12 
```

## Запуск режима восстановления

```

```


## Режим сервиса (docker-compose, docker swarm)

## Запуск режима архивирования 

```
services:
  AdapterDb:
    image: "dh.ics.perm.ru/esb/adapter-postgres"
    volumes:
      - PostgresDB:/var/lib/pgsql/data/
      - PostgresBackup:/var/lib/pgsql/backups
      - ${PWD}/set_monitor_time.sql:/docker-initdb.d/set_monitor_time.sql
    ports:
     - "${DATABASE_PORT}:5432"
    environment:
      - WALG=file
      - WALG_FILE_PREFIX=/var/lib/pgsql/backups

```


## Запуск режима восстановления

```
docker service update --force \
  --env-add BACKUP_RESTORE=`date +%Y%m%dT%H%M%S` \
  --env-add RESTORE_HOST={{ip}} \
  --env-add RESTORE_PORT={{port}} \
  --env-add RESTORE_USER={{login}} \
  --env-add RESTORE_PASSWORD={{password}} 
  {{name}}

```

