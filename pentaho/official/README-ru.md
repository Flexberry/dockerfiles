# Описание
Основан на образе [openjdk:8-slim](https://hub.docker.com/_/openjdk).

## Функционал

- Для корректной поддержки экспорта кириллических отчетов в PDF установлены [шрифты Microsoft TrueType](https://packages.debian.org/ru/sid/ttf-mscorefonts-installer).
- Удалены файлы для создания данных по-умолчанию в запускаемом `Pentaho Server`.
- Удалено задание периодической проверки наличия обновлений.
- Убрано выпадающее меню с тестовыми пользователями с экрана логина в `Pentaho Server`.
- Обеспечена возможность задавать имена пароли и роли новых пользователей с удалением стандартных пользователей `joe`, `pat`, `suzy`, `tiffany`;
- Обеспечена возможность смены пароля пользователя `admin`
- Образ позволяет в качестве административной баз данных использовать как встроенную базу `HSQLDB`, так и при использовани в `production` внешнюю реляционную базу (в настоящий момент `postgresql`).
- *TODO:* Добавить поддержку баз Oracle, MySQL, ClickHouse, ...

По умолчнию `Pentaho server` доступен на порту `8080`.

## Запуск

Тип административной базы задается переменной `DB_ADMIN` среды.
Если переменная не задана используется встроенная база `HSQLDB`.

Если переменная имеет значение `postgresql` в качестве административной базы используется база данных `Postgres`.

### Настрока административной базы данных

Параметры подключения к `PostgreSQL` задаются переменными среды:
- `DB_HOST` - адрес сервера с PostgreSQL (по умолчанию `postgres`);
- `DB_PORT` - порт сервера с PostgreSQL (по умолчанию `5432`);
- `DB_ADMIN_USER` - имя пользователя БД с правами администратора (по умолчанию `postgres`);
- `DB_ADMIN_PASS` - пароль пользователя БД с правами администратора (по умолчанию `p@ssw0rd`);
- `JCR_DB_NAME` - имя БД Jackrabbit-репозитория (по умолчанию `jackrabbit`);
- `JCR_PASS` - пароль для пользователя `jcr_user` для доступа к БД Jackrabbit-репозитория (по умолчанию `password`);
- `HIBERNATE_DB_NAME` - имя БД Hibernate (по умолчанию `hibernate`);
- `HIBERNATE_PASS` - пароль для пользователя `hibuser` для доступа к БД `Hibernate` (по умолчанию `password`);
- `QUARTZ_DB_NAME` - имя БД Quartz (по умолчанию `quartz`);
- `QUARTZ_PASS` - пароль для пользователя `pentaho_user` для доступа к БД Quartz (по умолчанию `password`).

*Рекомендуется перед запуском удалить из базы данных удалить перечисленные выше базы и пользователей*

### Настройка авторизации и аутентификации

При запуске образа есть возможность:
- указать список пользователей `pentaho`, их пароли и роли;
- заменить пароль пользвателя `admin` с правами администратора.

#### Определение списка пользователей

Переменная `USERS` определяет список пользователей pentaho, их пароли и ролию
Формат данной переменной:
```
<userNameA>:<passwordA>/<RoleA1>[,...<RoleAN>][\n<userNameB>:<passwordB>/<RoleB1>[,...<RoleBN>]]
```
Имя пользователя и пароль разделяются двоеточием. Список ролей пользователя указывается после косой `/` и раздаляется запятыми.

*Не рекомендуется в именах пользователей и паролях использовать символы форматирования XML:`>`, `<`, `&`, ...*

Разделителем между описаниями пользователей служит символ перевода строки `\n`.

Данный образ поддерживает следующие роли:
- `Administrator` - административные функции;
- `Power User` - размещение, выполнение отчетов;
- `Business Analyst` - выполнение отчетов;
- `Report Author` - размещение отчетов;
- `Anonymous` - пользователь с минимальными праами

**Пробелы в именах ролей обязательны!**

Пример описания пользователя `user` с минимальными правами:
```
USERS=user:12345/Anonymous
```
Пример описания трех пользователей:
```
USERS=power:password/Power User\nreporter:qwerty/Business Analyst,Report Author\nuser:12345/Anonymous
```

*При непустом значении переменной `USERS` стандартные пользователи `joe`, `pat`, `suzy`, `tiffany` удаляются из системы* 

#### Замена пароля пользователя admin

При наличии непустой переменной `ADMINPASSWORD` после инициализации сервера `pentaho` производится смена пароля пользователя `admin` на указанную строку.
Например:
```
ADMINPASSWORD=qwerty
```

### Настройка параметров виртуальной java-машины

Для оптимизациии работы и более тонкой настройкой сервера на тукущую конфигурацию образ поддерживает 
настройку параметров java-машины через переменную `BI_JAVA_OPTS`. Указанные параметры передаются стартовому скрипту `tomcat-сервера` обечпечивающего функционирование сервера `pentaho`.

### Запуск сервера в режиме docker-compose

Пример файла описания переменных `.env` и файла конфигурации `docker-compose.yml` приведен ниже:

Пример файла описания переменных `.env`:
```
BI_IMAGE_TAG=:8.2

#BI_JAVA_OPTS="-Xms4096m -Xmx6144m -XX:MaxMetaspaceSize=256m -Djava.security.egd=file:/dev/./urandom -Dsun.rmi.dgc.client.gcInterval=3600000 -Dsun.rmi.dgc.server.gcInterval=3600000 -Dfile.encoding=utf8 -DDI_HOME=\"$DI_HOME\""
BI_JAVA_OPTS=

#DB_ADMIN=postgresql
#DB_HOST=postgres
#DB_PORT=5432
#DB_ADMIN_USER=postgres
#DB_ADMIN_PASS=p@ssw0rd
#JCR_DB_NAME=jackrabbit
#JCR_PASS=password
#HIBERNATE_DB_NAME=hibernate
#HIBERNATE_PASS=password
#QUARTZ_DB_NAME=quartz
#QUARTZ_PASS=password

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

#USERS=power:password/Power User\nreporter:qwerty/Business Analyst,Report Author\nuser:12345/Anonymous
USERS=

#ADMINPASSWORD=qwerty
ADMINPASSWORD=
```

Пример файла конфигурации `docker-compose.yml`:
```
version: '3.2'

services:
  pentaho:
    image: flexberry/pentaho-official$BI_IMAGE_TAG
    ports:
      - "${SERVER_HTTP_PORT}:8080"
    volumes:
      - hsqldb:/biserver-ce/data/hsqldb/
      - repository:/biserver-ce/pentaho-solutions/system/jackrabbit/repository
      - logs:/biserver-ce/tomcat/logs
      - hidden:/biserver-ce/.pentaho/
      - tmp:/biserver-ce/tmp

    extra_hosts:
      - "postgres:10.130.2.87"

    environment:
      BI_JAVA_OPTS: '${BI_JAVA_OPTS}'
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
Вынесение части каталогов в именованые тома `hsqldb`, `repository`, `logs`, `hidden`, `tmp` обеспечивает сохранение текущих данных `pentaho` при перезапуске образа. 

При наобходимости именоване тома `logs`, `hidden`, `tmp` можно не использовать. 

Запуск `pentaho-сервера` в режиме `docker-compose` производится командой:
```
$ docker-compose up -d
```

останов командой:
```
$ docker-compose down
```

### Запуск сервера в режиме swarm

Запуск производится командами:
```
$ docker-compose config | docker stack deploy -c - PENTAHO
```

Где `PENTAHO` - имя стека сервисов.


