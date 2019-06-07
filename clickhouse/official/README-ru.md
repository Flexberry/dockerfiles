# Описание

Образ [flexberry/clickhouse-official](https://hub.docker.com/r/flexberry/clickhouse-official) основан на стандартом образе [yandex/clickhouse-server](https://hub.docker.com/r/yandex/clickhouse-server).

## Функционал

Образ [flexberry/clickhouse-official](https://hub.docker.com/r/flexberry/clickhouse-official) поддерживает следующие возможности:
- Установка параметров конфигурационных файлов
- Интеграция с базой данных Postgres:
  * Настройка ODBC-драйвера на указаную базу данных Postgres;
  - автоматическое создание clickhouse-словарей из базы данных postres;
  - автоматическое создание clickhouse-таблиц для указанных словарей
  
## Быстрый старт

### Запуск контейнера в режиме docker-compose

1. Создайте каталог для запуска образа в режиме `docker-compose` (например `testch`) 
2. Скопируйте файл [docker-compose.yml](https://github.com/Flexberry/dockerfiles/blob/master/clickhouse/official/docker-compose.yml) в созданный каталог
или создайте файл самостоятельно на основе шаблона
```
version: '3.2'

services:
  clickhouse:
    image: flexberry/clickhouse-official
    ports:
      - 8123:8123
      - 9000:9000
    volumes:
      - clickhousedb:/var/lib/clickhouse
      
volumes:
  clickhousedb:
```

Если у Вас порт TCP-порты `8123`, `9000` уже заняты другими приложениями укажите в mapping'е портов свободные порты. Например порты 18123, :
```
...
    - 18123:8123
    - 19000:9000
```

3. Перейдите созданный каталог (`testch`) и запустите команду:
```
docker-compose up -d
```
Проверить запуск контейнера можно командой
```
docker-compose ps
```
В списке запущенных контейнеров будет контейнер типа:
```
testch_clickhouse_1   /entrypoint.sh   Up      0.0.0.0:8123->8123/tcp, 0.0.0.0:9000->9000/tcp, 9009/tcp
```
Где `testch` - имя каталога запуска контейнера.

База данных контейнера располагается в именованом томе. Проверить наличие именованого тома можно командой:
```
docker volume ls
```
В списке томов отобразиться том:
```
local               testch_clickhousedb
```
Где `testch` - имя каталога запуска контейнера.


### Работа с контейнером

#### Работа из клиентского приложения

Сконфигурируйте приложение для работы с базой данных указав:
- IP-адрес clickhouse-сервера (`127.0.0.1` если приложение запущено на том компьютере);
- TCP-порты clickhouse-сервера (по умолчанию `8123` для REST-интерфеса, `9000` для бинарного интерфейса);
- имя пользователя - `default`;
- пароль - `` (пустая строка)

#### Работа из встрокнного терминального клиента clickhouse-client

Образ включает в себя терминальный клиент `clickhouse-client`.
Запустить его можно следующей командой:
- Linux
```
docker-compose exec clickhouse clickhouse-client
```
- Windows
```
 winpty docker-compose exec clickhouse clickhouse-client
```
После запуска клиент выедет подсказку:
```
ClickHouse client version 19.5.3.8 (official build).
Connecting to localhost:9000 as user default.
Connected to ClickHouse server version 19.5.3 revision 54417.

... :) 
```

Документация по работе с clickhouse-client приведена на странице
[Клиент командной строки](https://clickhouse.yandex/docs/ru/single/#klient-komandnoi-stroki).


#### Экранно-ориентированные интерфейсы для работы с clickhouse-server



## Запуск с конфигурацией

### Установка параметров файлов конфигурации

Дял установки параметров файлов конфигурации определите в среде две переменные:
- CLICKHOUSECONFIG - описание корректируемых значений в общем файле конфигурации /etc/clickhouse-server/config.xml
- CLICKHOUSEUSERCONFIG - описание корректируемых значений в файле конфигурации запросов пользователей /etc/clickhouse-server/users.xml.

Корректирумые значения описываются в виде:
```
<xpath_тега1>=<значение1[,<xpath_тега2=<значение2,...]
```

В большинстве случаев в качестве `xpath_тега` можно использовать имя тега (например `max_memory_usage`).
Если в XML-файле конфигурации указанный тег встречается неоднократно в раном контексте
для указания нужного тега необходимо указать контекст в виде xpath-подпути до тега
(например `client/disableProtocols` или `yandex/openSSL/client/disableProtocols`).
Начальные слеши (`/`) указывать не надо - при корректировке XML-файла XSLT-процессором перед указанным xpath-выражением добавляется тропа `//`.
Пример описания корректируемых значений:
```
CLICKHOUSECONFIG=http_port=18123,default_session_timeout=120
CLICKHOUSEUSERCONFIG=max_memory_usage=32000000000,quotas/interval/default/duration=7200
```
### Создание внешних словарей на основе таблиц базы данных Postgres

Для создания внешних словарей на основе таблиц базы данных Postgres определите следующие переменные среды:
- PGServername - домен или IP-адрес сервера Postgres;
- PGPort - порт по которому принимает соединение сервер Postgres (по умолчанию 5432); 
- PGProtocol - тип протокола (версия Postgres сервера) (по умолчанию 9.6)
- PGDatabase - имя базы данных;
- PGUserName - имя пользователя Postgres;
- PGPassword - пароль пользователя Postgres;
- PGDicrionaries - список таблиц для которых необходимо создать внешние словари и соответствующие таблицы ClickHouse; 

Формат список таблиц внешних словарей:
```
имя_таблицы1[/ключевое_поле11[,ключевое_поле12...]],[имя_таблицы2...]
```
Если не указан `список ключевых полей` в качестве ключевого поля принимается поле с именем `primarykey`.

Перед запуском сервере ClickHouse а контейнере образа создаются следующие файлы:
- /etc/odbc.ini - инициализционный файл сервиса ODBC описывающий коннект с сервером Postgres(имя коннекта postgresConnection);
- /etc/clickhouse-server/&lt;PGDatabase&gt;_dictionary.xml - описание внешних словарей, перечисленных в переменной PGDicrionaries  (где &lt;PGDatabase&gt; - имя базы данных).
 
 После запуска сервера ClickHouse  в базе данных PGDatabase создаются перечисленные в переменной PGDicrionaries таблицы ClickHouse типа Dictionary.
 
 Для формирования типа атрибутов таблиц ClickHouse производится обращение к описанию атрибутов указанных Postgres-таблиц.
 Маппинг типов атрибутов Postgres в типы атрибутов ClickHouse:
 
 Тип Postgres | Тип ClickHouse | Значение по умолчанию
-------------|----------------|----------------------
timestamp .* | DateTime | 0000-00-00 00:00:00
character .* | String | '' (пустая строка)
 uuid | UUID | 00000000-0000-0000-0000-000000000000
integer [8, 16, 32, 64] | Int8, Int16, Int32, Int64 | -1
boolean | Int8 | 0
numeric | Float64 | 0.0

Так как Clickhouse для таблиц типа Dictionary не поддерживается тип NULL при  импорте таких значений они заменяются на значение по умолчанию.

