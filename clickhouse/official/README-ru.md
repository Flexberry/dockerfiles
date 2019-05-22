# Описание

Образ [flexberry/clickhouse-official](https://hub.docker.com/r/flexberry/clickhouse-official) основан на стандартом образе [yandex/clickhouse-server](https://hub.docker.com/r/yandex/clickhouse-server).

## Функционал

Образ [flexberry/clickhouse-official](https://hub.docker.com/r/flexberry/clickhouse-official) поддерживает следующие возможности:
- Установка параметров конфигурационных файлов
- Интеграция с базой данных Postgres:
  * Настройка ODBC-драйвера на указаную базу данных Postgres;
  - автоматическое создание clickhouse-словарей из базы данных postres;
  - автоматическое создание clickhouse-таблиц для указанных словарей

## Запуск

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
Начальные слеши (`/`) указывать не надо - при кооректировке XML-файла XSLT-процессором перед указанным xpath-выражением добавляется тропа `//`.
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

