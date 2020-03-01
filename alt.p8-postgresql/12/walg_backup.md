# Бекапирования системой wal-g

Если в файла описания параметров настройки сервиса `.env`
определена переменная `WALG` со значениями `file`, `s3`, то при работе базы данных
включается режим формирования бекапов:
- файловую систему при значении `file`;
- в облачный сервис поддерживаюший протокол `S3` (например [minio](https://hub.docker.com/r/minio/minio)).

При некорректных значениях переменной `WAL_G` бекап не производится.

## Параметры бекапа
Переменные определяющие общие параметы бекапа хранятся в файле конфигурации
`/etc/wal-g.d/server.conf`.

Имя переменной | Описание |Значение по умолчанию
-------------|----------|------------------
TOTAL_BG_UPLOADED_LIMIT | Количество WAL-файлов для загрузки во время одного сканирования  | 1024
WALG_SENTINEL_USER_DATA | his setting allows backup automation tools to add extra information to JSON sentinel file during backup-push. This setting can be used e.g. to give user-defined names to backups.  | '' - пустая строка
WALG_PREVENT_WAL_OVERWRITE | If this setting is specified, during wal-push WAL-G will check the existence of WAL before uploading it. If the different file is already archived under the same name, WAL-G will return the non-zero exit code to prevent PostgreSQL from removing WAL.  | 1
WALG_DELTA_ORIGIN | To configure base for next delta backup (only if WALG_DELTA_MAX_STEPS is not exceeded). WALG_DELTA_ORIGIN can be LATEST (chaining increments), LATEST_FULL (for bases where volatile part is compact and chaining has no meaning - deltas overwrite each other). Defaults to LATEST.  | LATEST
WALG_DELTA_MAX_STEPS | Delta-backup is the difference between previously taken backup and present state. WALG_DELTA_MAX_STEPS determines how many delta backups can be between full backups. Defaults to 0. Restoration process will automatically fetch all necessary deltas and base backup and compose valid restored backup (you still need WALs after start of last backup to restore consistent cluster). Delta computation is based on ModTime of file system and LSN number of pages in datafiles.  | 7
WALG_TAR_SIZE_THRESHOLD |  To configure the size of one backup bundle (in bytes). Smaller size causes granularity and more optimal, faster recovering. It also increases the number of storage requests, so it can costs you much money. Default size is 1 GB (1 << 30 - 1 bytes). | 109051904
WALG_UPLOAD_CONCURRENCY |  To configure how many concurrency streams to use during backup uploading, use WALG_UPLOAD_CONCURRENCY. By default, WAL-G uses 16 streams. | 16
WALG_DOWNLOAD_CONCURRENCY | o configure how many goroutines to use during backup-fetch and wal-fetch, use WALG_DOWNLOAD_CONCURRENCY. By default, WAL-G uses the minimum of the number of files to extract and 10.  | 10
WALG_UPLOAD_DISK_CONCURRENCY | To configure how many concurrency streams are reading disk during backup-push. By default, WAL-G uses 1 stream.  | 1
WALG_COMPRESSION_METHOD |   | brotli
OPLOG_ARCHIVE_TIMEOUT |   | 60
OPLOG_ARCHIVE_AFTER_SIZE |   | 33554432

Подробое описание полей см. на странице [WAL-G for PostgreSQL](https://github.com/wal-g/wal-g/blob/master/PostgreSQL.md)

### Бекап файловой системы по протоколу S3

Переменные определяющие параметы бекапа по протоколу S3 хранятся в файле конфигурации
`/etc/wal-g.d/server-s3.conf`. 
Параметы по умолчанию указаны в нижеприведенной таблицы.
Они могут быть переодпределены в окружении при вызова контейнера.
Переменные `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY` должны быть определены в окружении.

Имя переменной | Описание |Значение по умолчанию
-------------|----------|------------------
**AWS_ACCESS_KEY_ID** | Идентификатор ключа доступа к `S3`(`minio`) серверу | Нет
**AWS_SECRET_ACCESS_KEY**| Секретный ключ доступа к `S3`(`minio`) серверу | Нет
WALE_S3_PREFIX | `URL` каталога обласного сервиса `S3` для бекапа  | s3://pg-backups
AWS_ENDPOINT | `URL` `S3`(`minio`) сервера | http://ip-s3:9000

### Бекап файловой системы по протоколу file

Переменные определяющие параметы бекапа по протоколу S3 хранятся в файле конфигурации
`/etc/wal-g.d/server-file.conf`. 
Параметы по умолчанию указаны в нижеприведенной таблицы.
Они могут быть переодпределены в окружении при вызова контейнера.

Имя переменной | Описание |Значение по умолчанию
-------------|----------|------------------
WALG_FILE_PREFIX | Тропа до каталога бекапа | /var/lib/pgsql/data/sar/

Имейте в виду - переменная `WALG_FILE_PREFIX` должна указывать на каталог, который находится вне контейера.
Иначе при удалеии контейнеры данные бекапа будут утеряны.




