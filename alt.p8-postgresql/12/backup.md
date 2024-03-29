# Режимы восстановления базы данных

Образ поддеживает два режима бекапа:
- Восстановление с удаленного postgres-сервера средствами DUMP/RESTORE. Данный режим включается пи наличии переменной `BACKUP_RESTORE`.
- Восстановление из `WAL-G` архива . Данный режим включается при наличии переменной `BACKUP_WALG`.

Переменные `BACKUP_RESTORE`, `BACKUP_WALG` содержат уникальный идентификатор
в формат YYYY-mm-dd** **HH:MM:SS.
Где:
- `YYYY` - год;
- `mm` - месяц;
- `dd` - день;
- `HH` - час;
- `MM` - минута;
- `SS` - секунда.

Между месяцем и часом **должен** присутствовать пробел.
Например `2020-03-15 09:52:54`.

Цель идентификатора - исключить повторное восстановление базы данных при перезагрузке сервиса базы данных. Если идентификатор совпадает с предыдущим восстановление базы данных не производится.

## Переменные режима восстановления базы данных по протоколу DUMP/RESTORE

- **`BACKUP_RESTORE='уникальный идентификтор`** - инициирует бекап DUMP/RESTORE;
- **`RESTORE_HOST`** - IP-адрес или домен сервера `postgresql` с которого производится бекап;
- `RESTORE_PORT` - порт сервера `postgresql` (по умолчанию 5432);
- `RESTORE_USER` - имя пользователя `postgresql` от имени которого поизводится бекап (по умолчанию `postgres`);
- **`RESTORE_PASSWORD`** - пароль пользователя `postgresql` от имени которого производится бекап. 

**Жирным шрифтом** выделены обязательые переменные.

На стороне сервера на который производится бекап запускается команда:
```
export PGPASSWORD='...';
pg_dumpall -h ... -p ... -U ... --clean --if-exists | psql -U postgres
```
### Варианты запуска восстановления базы данных с удаленного сервера

#### Запуск восстановления базы данных с удаленного сервера в режиме контейнера

#### Запуск восстановления базы данных с удаленного сервера в режиме сервиса (docker-compose, swarm)

```
docker service update --force \
  --env-add BACKUP_RESTORE='`date "+%Y-%m-%d %H:%M:%S"`'\
  --env-add RESTORE_HOST={{ip}} \
  --env-add RESTORE_PORT={{port}} \
  --env-add RESTORE_USER={{login}} \
  --env-add RESTORE_PASSWORD={{password}} \
  {{name}}

```

## Переменные режима восстановления базы данных по протоколу WALG

Переменные режима `WALG` описаны в разделе
[Бекапирования системой wal-g](walg_backup.md). 
На момент запуска восстановления архив `WALG` уже должен существовать.

### Варианты запуска восстановления базы данных 

#### Запуск восстановления базы данных в режиме коннтейнера

Так так восстановление базы данных единовременная операция, то переменную 
определяющие параметры восстановления не стоит вносить в скрипт запуска контейнера базы данных `postgres`,
а экспортировать их в качестве переменных среды.

При запуске по протоколу `file`  см. [Запуск по протоколу file в режиме контейнера](walg_backup.md#запуск-по-протоколу-file-в-режиме-контейнера):
```
export BACKUP_WALG=`date "+%Y-%m-%d %H:%M:%S"`
runFileMode.sh
```

При запуске по протоколу `S3`см. [Запуск по протоколу s3 в режиме контейнера](walg_backup.md#запуск--по-протоколу-s3-в-режиме-контейнера):
```
export BACKUP_WALG=`date "+%Y-%m-%d %H:%M:%S"`
runS3Mode.sh
```

После восстановления базы данных 
значение переменной `BACKUP_WALG` сохранится и
сервер базы данных запустится в
обычном режиме бекапа (`file` или `s3`).

При повторном запуске скрипта `runFileMode.sh` или `runS3Mode.sh` 
если значение переменной `BACKUP_WALG`  непустое и совпадает с
сохраненным значением `BACKUP_WALG`, то операция восстановления базы данных не производится.

#### Запуск восстановления базы данных в режиме сервиса (docker-compose, swarm)

Так так восстановление базы данных единовременная операция, то переменную 
определяющие параметры восстановления не стоит вносить в YML-файл `docker-compose.yml`
а экспортировать их в качестве переменных среды.

При запуске в режиме `dockere-compose`:
```
export BACKUP_WALG=`date "+%Y-%m-%d %H:%M:%S"`
docker-compose up -d
```

При запуске в режиме `docker swarm`:
```
docker service update --force  --env-add BACKUP_WALG='`date "+%Y-%m-%d %H:%M:%S"`' <имя_сервиса>
```

После восстановления базы данных 
значение переменной `BACKUP_WALG` сохранится и
сервер базы данных запустится в
обычном режиме бекапа (`file` или `s3`).

Сервис базы данных может по какой-либо причине перезапуститься (команды `docker-compose up -d`, `docker service update ...`, перезагрузка системы или docker-кластера). В этом случае значение переменной `BACKUP_WALG` не изменится и повторного восстановления базы данных не произойдет.

