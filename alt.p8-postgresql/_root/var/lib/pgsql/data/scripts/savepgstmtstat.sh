#!/bin/bash
# Сценарий savepgstmtstat.sh, предназначенный для регулярного запуска с целью сохранения текущего содержимого представления статистики pg_stat_statements.

# Автор:  Новиков А.М.
# Версия: 1.0
# Дата:   30.04.2020

# История изменений
# Версия: 1.1
# Дата:   21.05.2020
# Обеспечивается протоколирование выполняемых над представлением pg_stat_statements операций.
# В режиме отладки только генерируются команды на сохранение статистики, но не выполняются.
# Используется версия 1.1 и выше библиотеки pgadmlibr.sh, версия 1.0 и выше библиотеки pgadmstatlibr.sh.

WORKDIR=$1      # Если параметр равен calldir, в каталоге, откуда вызван сценарий, будет создан подкаталог log, если его нет,
                # который будет использован в качестве рабочего,
                # иначе строка рассматривается как каталог, который должен существовать, но м.б. создан в случае отсутствия
PGSOCKET=$2     # Если =1, подключение через сокет (это влияет на строку подключения)
ADMDBNAME=$3    # Административная БД, в схеме adm которой сохраняется информация
                # Сценарий savepgstmtstat.sh будет проверять наличие данной БД на сервере PostgreSQL,
                # к которому задано подключение. В случае её отсутствия прекратит работу.
DBLIST=$4       # Дополнительный список БД, для которых должно выполняться сохранение текущего содержимого представления pg_stat_statements.
                # Если пусто, используется текущее содержимое таблицы adm.dbmaintenance.
STATRESET=$5    # 0/1 - сброс содержимого представления статистики pg_stat_statements после успешного сохранения.  По умолчанию 1.
DEBUG_MODE=$6   # Режим отладки, если больше 0. Управляет фактической возможностью сохранения и сброса содержимого pg_stat_statements. По умолчанию 0.
PROTOCOL=$7     # 0/1 - протоколирование выполняемых sql-команд. По умолчанию 1.

if [ "$WORKDIR" == "" ]; then
    echo Не заданы параметры вызова
    exit 10
fi

CALLDIR=`dirname $0`
if [ "$WORKDIR" == "calldir" ]; then
    WORKDIR=$CALLDIR/log
fi

if [ "$WORKDIR" != "calldir" ]; then
    if [ ! -d $WORKDIR ]; then
        mkdir $WORKDIR
        # Если при отладке запускали под root или другим пользователем, обеспечим всё для postgres
        chown postgres:postgres $WORKDIR
    fi
fi

if [ "$STATRESET" == "" ]; then
    STATRESET=1
fi
if [ "$DEBUG_MODE" == "" ]; then
    DEBUG_MODE=0
fi
if [ "$PROTOCOL" == "" ]; then
    PROTOCOL=1
fi

YYYYMMDD=`/bin/date '+%Y%m%d'`
LOG_FILE=$WORKDIR/savepgstmtstat_$YYYYMMDD.log
RES1_FILE=$WORKDIR/savepgstmtstat_dbnames.txt

CURDATE=`/bin/date '+%d.%m.%Y %H:%M:%S.%N'`
CALLCMD="$0 WORKDIR=$WORKDIR PGSOCKET=$PGSOCKET ADMDBNAME=$ADMDBNAME DBLIST=\"$DBLIST\" STATRESET=$STATRESET DEBUG_MODE=$DEBUG_MODE PROTOCOL=$PROTOCOL"
echo $CURDATE Запуск $CALLCMD >>$LOG_FILE

echo >>$LOG_FILE

if [ "$PGSOCKET" == "1" ]; then
    CONNECT="--username=postgres --no-password"
else
    CONNECT="--host=localhost --port=5432 --username=postgres --no-password"
fi

# PROTOCOL, LOG_FILE, CONNECT и следующие глобальные переменные могут использоваться в функциях подгружаемой библиотеки pgadmlibr.sh
PSQL=/usr/bin/psql
RESCODE=
SQL=
OPERPRIMARYKEY=
OPERPROTOCOL=

# Целесообразно подключиться к библиотеке pgadmlibr.sh с выполнением проверок, например:
PGADMLIBR=$CALLDIR/pgadmlibr.sh
if [ -f $PGADMLIBR ]; then
    echo Подключение библиотеки $PGADMLIBR >>$LOG_FILE
    . $PGADMLIBR
    res=$?
    if [ $res -ne 0 ]; then
        CURDATE=`/bin/date '+%d.%m.%Y %H:%M:%S.%N'`
        echo Досрочное завершение из-за ошибки при подключении библиотеки $PGADMLIBR >>$LOG_FILE
        echo Завершение $0: $CURDATE. Время работы  $SECONDS сек>>$LOG_FILE
        exit 100
    fi
else
    CURDATE=`/bin/date '+%d.%m.%Y %H:%M:%S.%N'`
    echo Досрочное завершение из-за отсутствия библиотеки $PGADMLIBR >>$LOG_FILE
    echo Завершение $0: $CURDATE. Время работы  $SECONDS сек>>$LOG_FILE
    exit 101
fi

# Целесообразно подключиться к библиотеке pgadmstatlibr.sh с выполнением проверок, например:
PGADMSTATLIBR=$CALLDIR/pgadmstatlibr.sh
if [ -f $PGADMSTATLIBR ]; then
    echo Подключение библиотеки $PGADMSTATLIBR >>$LOG_FILE
    . $PGADMSTATLIBR
    res=$?
    if [ $res -ne 0 ]; then
        finish "$0" "Досрочное завершение из-за ошибки при подключении библиотеки $PGADMSTATLIBR"
        exit 200
    fi
else
    finish "$0" "Досрочное завершение из-за отсутствия библиотеки $PGADMSTATLIBR"
    exit 201
fi

# Проверим наличие заданной административной БД $ADMDBNAME
checkdb "$ADMDBNAME"
res=$?
if [ $res -ne 0 ]; then
    finish "$0" "Досрочное завершение из-за проблем при проверке заданной административной БД $ADMDBNAME"
    exit 20
fi

# Проверим наличие схемы adm в административной БД $ADMDBNAME  и создадим её в случае отсутствия
checkchema "$ADMDBNAME" adm 1
res=$?
if [ $res -ne 0 ]; then
    finish "$0" "Досрочное завершение из-за проблем при проверке схемы adm в заданной административной БД $ADMDBNAME"
    exit 30
fi

# Проверим наличие установленного пакета расширения uuid-ossp в административной БД $ADMDBNAME и установим его в случае отсутствия
checkextension $ADMDBNAME "uuid-ossp" 1
res=$?
if [ $res -ne 0 ]; then
    finish "$0" "Досрочное завершение из-за проблем при проверке пакета расширения uuid-ossp в заданной административной БД $ADMDBNAME"
    exit 40
fi

# Проверим наличие в административной БД $ADMDBNAME таблицы протоколирования операций adm.operprotocol и создадим её в случае отсутствия.
checkoperprotocol $ADMDBNAME 1
res=$?
if [ $res -ne 0 ]; then
    finish "$0" "Досрочное завершение из-за проблем при проверке таблицы протоколирования операций adm.operprotocol в заданной административной БД $ADMDBNAME"
    exit 50
fi

# Зарегистрируем начало работы
PROTDBNAME=empty
operprotocol_start "$ADMDBNAME" default "" "Сохранение содержимого представления статистики pg_stat_statements" "" null null "$CALLCMD"
res=$?
if [ $res -eq 0 ]; then
    PROTDBNAME="$ADMDBNAME"
    OPERPROTOCOL=$OPERPRIMARYKEY
else
    echo Ошибка $res при регистрации операции. Протоколирование более не ведётся >>$LOG_FILE
    OPERPRIMARYKEY=
    OPERPROTOCOL=
fi

# Проверим задание разделяемой библиотеки pg_stat_statements в файле конфигурации PostgreSQL
checksharedpreloadlibraries postgres "pg_stat_statements"
res=$?
if [ $res -ne 0 ]; then
    finish "$0" "Досрочное завершение из-за проблем при проверке заданиz разделяемой библиотеки pg_stat_statements в файле конфигурации PostgreSQL" "$PROTDBNAME" "$OPERPROTOCOL"
    exit 50
fi

# Проверим наличие установленного пакета расширения pg_stat_statements в административной БД $ADMDBNAME и установим его в случае отсутствия
checkextension "$ADMDBNAME" "pg_stat_statements" 1
res=$?
if [ $res -ne 0 ]; then
    finish "$0" "Досрочное завершение из-за проблем при проверке пакета расширения pg_stat_statements в заданной административной БД $ADMDBNAME" "$PROTDBNAME" "$OPERPROTOCOL"
    exit 60
fi

# Проверим наличие в заданной административной БД $ADMDBNAME служебной таблицы adm.dbmaintenance со списком обслуживаемых БД,
# создадим её при отсутствии,
# Нв случай версии PostgreSQL 12 и выше сгенерим сразу же скрипт на сброс статистики только по обслуживаемым БД.
STATRESETTEMPLATE=
STATRESETSCRIPT=
PRONARGS=
if [ $STATRESET -eq 1 ]; then
    # Задан сброс статистики после её сохранения. Поймём, будет ли возможен выборочный сброс статистики?
    checkpgstatstatementsreset "$ADMDBNAME"
    PRONARGS=$RESCODE
    if [ "$PRONARGS" == "" ]; then
        PRONARGS=0
    fi
    if [ $PRONARGS -gt 0 ]; then
        # Да, функция pg_stat_statements_reset() позволяет задавать oid БД для сброса статистики только по ней
        STATRESETTEMPLATE="perform pg_stat_statements_reset(null, ((DBID)), null); -- ((DBNAME))"
    fi
fi
checkdbmaintenance "$ADMDBNAME" savepgstmtstat "$DBLIST" "$STATRESETTEMPLATE"
res=$?
if [ $res -ne 0 ]; then
    finish "$0" "Досрочное завершение из-за проблем при проверке наличия в заданной административной БД $ADMDBNAME служебной таблицы adm.dbmaintenance" "$PROTDBNAME" "$OPERPROTOCOL"
    exit 70
fi
if [ $STATRESET -eq 1 ]; then
    if [ $PRONARGS -gt 0 ]; then
        # Здесь ожидается сгенерированный скрипт
        STATRESETSCRIPT="$RESCODE"
        if [ "$STATRESETSCRIPT" == "" ]; then
            echo "ПРЕДУПРЕЖДЕНИЕ! Не сгенерированы команды для выборочного сброса статистики только по обслуживаемым БД. Сброс статистики будет полным." >>$LOG_FILE
            STATRESETSCRIPT="perform pg_stat_statements_reset();"
        fi
    else
        STATRESETSCRIPT="perform pg_stat_statements_reset();"
    fi
fi

# Однако в случае отладки не будем выполнять сброс статистики. Поскольку в $STATRESETSCRIPT м.б. несколько строк, делаем так:
if [[ $DEBUG_MODE -gt 0 && "$STATRESETSCRIPT" != "" ]]; then
    STATRESETSCRIPT="
-- DEBUG_MODE=$DEBUG_MODE
if 1=0 then
$STATRESETSCRIPT
end if;
"
fi

# Известно, что в разных версиях PostgreSQL структура представления pg_stat_statements м.б. разной
SQL="select coalesce((
select 1
  from information_schema.tables v
    inner join information_schema.columns c
        on  c.table_catalog = v.table_catalog
        and c.table_schema  = v.table_schema
        and c.table_name    = v.table_name
 where v.table_catalog = '$ADMDBNAME'
   and v.table_name = 'pg_stat_statements'
   and v.table_type = 'VIEW'
   and c.column_name= 'mean_time'
), 0)
"
execsql "$ADMDBNAME" "Проверка структуры представления pg_stat_statements"
res=$?
if [ $res -ne 0 ]; then
    finish "$0" "Досрочное завершение из-за проблем при проверке структуры представления pg_stat_statements" "$PROTDBNAME" "$OPERPROTOCOL"
    exit 80
fi
MEAN_TIME_EXISTS=$RESCODE

NEWCOLUMNS=
if [ $MEAN_TIME_EXISTS -eq 0 ]; then
    # Старая структура pg_stat_statements, как в версии PostgreSQL 9.4
#   NEWCOLUMNS="0::double precision as min_time, 0::double precision as max_time, 0::double precision as mean_time, 0::double precision as stddev_time, "
    NEWCOLUMNS="s.total_time/s.calls as min_time, s.total_time/s.calls as max_time, s.total_time/s.calls as mean_time, null::double precision as stddev_time, "
fi

# Проверим наличие в заданной административной БД $ADMDBNAME накопительных таблмц для сохранения содержимого представления pg_stat_statements на разных стадиях обработки
# и создадим в случае их отсутствия
#   Стадии обработки:
# 1 Копирование текущего содержимого pg_stat_statements во временную таблицу t_pg_stat_statements с доп.столбцами md5queryid, queryprec, queryprecid.
# 2 В таблице t_pg_stat_statements выполняется улучшение текста запроса в столбце queryprec, вычисляется новый хеш-код запроса queryprecid.
# 3 Запрос из t_pg_stat_statements сохраняется в таблице adm.pg_stat_statement_query, если его там ещё нет, что проверяется по совокупности столбцов userid, dbid, queryid, md5queryid, queryprecid.
#   А если такой запрос уже есть в adm.pg_stat_statement_query, у соответствующей записи изменяется счётчик регистрации regcount и время последней регистрации запроса lastregtime.
#   Таблица adm.pg_stat_statement_query используется только для сохранения текстов запросов, но не для сохранения статистических данных.
# 4 Статистические данные из t_pg_stat_statements сохраняются в сыром виде в таблице adm.pg_stat_statement_rawdata для даты их выборки из pg_stat_statements.
# 5 Статистические данные из t_pg_stat_statements сохраняются в обобщённом и подсуммированном по dbid и queryprecid виде в таблице adm.pg_stat_statement_data для даты их выборки из pg_stat_statements.
#   При этом учитывается разница по структуре представления pg_stat_statements в разных версиях PostgreSQL.

SQL="do \$\$
begin
    -- Проверка наличия накопительной таблицы для текстов запросов
    if not exists(select 1 from information_schema.tables where table_schema = 'adm' and table_name = 'pg_stat_statement_query') then
        create table adm.pg_stat_statement_query(
            primarykey          uuid not null primary key default public.uuid_generate_v4(),
            userid              oid not null,
            dbid                oid not null,
            queryid             bigint not null,
            md5queryid          varchar(32) not null,
            queryprecid         varchar(32) not null,
            changecount         int not null,
            inlistcount         int not null,
            query               text not null,
            queryprec           text null,
            createtime          timestamp(3) with time zone not null,
            regcount            bigint not null,
            lastregtime         timestamp(3) with time zone not null
        );
        create unique index pg_stat_statement_query_unique      on adm.pg_stat_statement_query(userid, dbid, queryid, md5queryid, queryprecid);
        create        index pg_stat_statement_query_dbid        on adm.pg_stat_statement_query(dbid);
        create        index pg_stat_statement_query_queryprecid on adm.pg_stat_statement_query(queryprecid);
        create        index pg_stat_statement_query_createtime  on adm.pg_stat_statement_query(createtime);
        create        index pg_stat_statement_query_lasttime    on adm.pg_stat_statement_query(lastregtime);
    end if;

    -- Проверка наличия накопительной таблицы для сырой статистики запросов
    if not exists(select 1 from information_schema.tables where table_schema = 'adm' and table_name = 'pg_stat_statement_rawdata') then
        create table adm.pg_stat_statement_rawdata(
            primarykey          uuid not null primary key default public.uuid_generate_v4(),
            registrationtime    timestamp(3) with time zone not null,
            userid              oid not null,
            dbid                oid not null,
            queryid             bigint not null,
            md5queryid          varchar(32) not null,
            queryprecid         varchar(32) not null,
            calls               bigint not null,
            total_time          double precision not null,
            min_time            double precision not null,
            max_time            double precision not null,
            mean_time           double precision not null,
            stddev_time         double precision     null,
            rows                bigint not null,
            shared_blks_hit     bigint not null,
            shared_blks_read    bigint not null,
            shared_blks_dirtied bigint not null,
            shared_blks_written bigint not null,
            local_blks_hit      bigint not null,
            local_blks_read     bigint not null,
            local_blks_dirtied  bigint not null,
            local_blks_written  bigint not null,
            temp_blks_read      bigint not null,
            temp_blks_written   bigint not null,
            blk_read_time       double precision not null,
            blk_write_time      double precision not null
        );
        create unique index pg_stat_statement_rawdata_unique            on adm.pg_stat_statement_rawdata(userid, dbid, queryid, md5queryid, queryprecid, registrationtime);
        create        index pg_stat_statement_rawdata_userid            on adm.pg_stat_statement_rawdata(userid);
        create        index pg_stat_statement_rawdata_dbid              on adm.pg_stat_statement_rawdata(dbid);
        create        index pg_stat_statement_rawdata_queryid           on adm.pg_stat_statement_rawdata(queryid);
        create        index pg_stat_statement_rawdata_registrationtime  on adm.pg_stat_statement_rawdata(registrationtime);
        alter table adm.pg_stat_statement_rawdata add constraint pg_stat_statement_rawdata_query_fk foreign key (userid, dbid, queryid, md5queryid, queryprecid)
                    references adm.pg_stat_statement_query(userid, dbid, queryid, md5queryid, queryprecid);
    end if;

    -- Проверка наличия накопительной таблицы для обобщённой и подсуммированной статистики запросов
    if not exists(select 1 from information_schema.tables where table_schema = 'adm' and table_name = 'pg_stat_statement_data') then
        create table adm.pg_stat_statement_data(
            primarykey          uuid not null primary key default public.uuid_generate_v4(),
            dbid                oid not null,
            queryprecid         varchar(32) not null,
            registrationtime    timestamp(3) with time zone not null,
            calls               bigint not null,
            total_time          double precision not null,
            min_time            double precision not null,
            max_time            double precision not null,
            mean_time           double precision not null,
            stddev_time         double precision     null,
            rows                bigint not null,
            shared_blks_hit     bigint not null,
            shared_blks_read    bigint not null,
            shared_blks_dirtied bigint not null,
            shared_blks_written bigint not null,
            local_blks_hit      bigint not null,
            local_blks_read     bigint not null,
            local_blks_dirtied  bigint not null,
            local_blks_written  bigint not null,
            temp_blks_read      bigint not null,
            temp_blks_written   bigint not null,
            blk_read_time       double precision not null,
            blk_write_time      double precision not null,
            cnt                 int not null
        );
        create unique index pg_stat_statement_data_unique           on adm.pg_stat_statement_data(dbid, queryprecid, registrationtime);
        create        index pg_stat_statement_data_dbid             on adm.pg_stat_statement_data(dbid);
        create        index pg_stat_statement_data_queryprecid      on adm.pg_stat_statement_data(queryprecid);
        create        index pg_stat_statement_data_registrationtime on adm.pg_stat_statement_data(registrationtime);
    end if;

    -- Потом окончательно подумать, как установить ограничения внешнего ключа между этими таблицами
end\$\$;
"
execsql "$ADMDBNAME" "Проверка наличия накопительных таблиц adm.pg_stat_statement_query, adm.pg_stat_statement_rawdata и adm.pg_stat_statement_data"
res=$?
if [ $res -ne 0 ]; then
    finish "$0" "Досрочное завершение из-за проблем при проверке наличия накопительных таблиц adm.pg_stat_statement_query, adm.pg_stat_statement_rawdata и adm.pg_stat_statement_data" "$PROTDBNAME" "$OPERPROTOCOL"
    exit 90
fi

# Получим список oid обслуживаемых БД, для которых должно выполняться сохранение представления pg_stat_statements
getdboids2 "$ADMDBNAME" savepgstmtstat
res=$?
if [ $res -ne 0 ]; then
    finish "$0" "Досрочное завершение из-за проблем при получении списка oid для обслуживаемых БД" "$PROTDBNAME" "$OPERPROTOCOL"
    exit 120
fi
DBOIDS=$RESCODE
if [ "$DBOIDS" == "" ]; then
    finish "$0" "Досрочное завершение из-за пустого списка oid для обслуживаемых БД" "$PROTDBNAME" "$OPERPROTOCOL"
    exit 130
fi

operprotocol_start "$PROTDBNAME" default "" "Сохранение представления статистики pg_stat_statements для баз данных $DBOIDS и сброс содержимого pg_stat_statements"

# Получим текущее время с сервера БД
getclocktimestamp "$ADMDBNAME" sorted_ms_utc
CLOCKTIMESTAMP="$RESCODE"

# Выполним сохранение новых sql-запросов из представления pg_stat_statements только для обслуживаемых БД
SQL="do \$\$
declare
    registration_time   timestamp(3) with time zone;
    change_count        int;
    inlist_count        int;
    query_prec0         text;
    query_prec          text;
    p1                  int;
    p2                  int;
    s                   text;
    cntloop             int;
    maxcntloop1         int = 50000;
    maxcntloop2         int = 50000;
    curs cursor for select primarykey, changecount, inlistcount, queryprec from t_pg_stat_statements;
begin
    registration_time = '$CLOCKTIMESTAMP'::timestamp(3) with time zone;
    create temporary table t_pg_stat_statements on commit drop
    as
    select public.uuid_generate_v4() as primarykey, md5(s.query) as md5queryid, s.*, $NEWCOLUMNS query as queryprec, md5(null::text) as queryprecid,
           null::integer as changecount, null::integer as inlistcount, registration_time as createtime, 1::bigint as regcount, registration_time as lastregtime,
           false as newquery
      from pg_stat_statements s
            inner join pg_catalog.pg_database d
                on d.oid = s.dbid
     where s.dbid in ($DBOIDS);
    alter table t_pg_stat_statements add constraint t_pg_stat_statements_pkey primary key (primarykey);
    analyze t_pg_stat_statements;

    -- Проходим по всем операторам, улучшаем текст запроса, вычисляем новый ид.запроса queryprecid и пр.
    for r in curs loop
        change_count = 0;
        query_prec0 = r.queryprec;
        -- Улучшим текст оператора
        query_prec  = trim(replace(replace(replace(replace(replace(replace(replace(replace(replace(replace(replace(replace(replace(query_prec0,
                      'timestamp?','?'), chr(13)||chr(10), ' '),chr(13),' '),chr(10),' '),chr(9),' '),'  ',' '),'  ',' '),'  ',' '),'  ',' '),'( ','('),' )',')'),', ',','),' ,',','));
        if query_prec similar to '\/\* [0-9][0-9]\*\/%' then
            query_prec = ltrim(substring(query_prec,8));
        end if;
        if length(query_prec0) <> length(query_prec) then
            change_count = 1;
        else
            if query_prec0 <> query_prec then
                change_count = 1;
            end if;
        end if;

        -- строковые константы приводим к ?
        cntloop = 0;
        p1 = strpos(query_prec,'''');
        while p1 > 1
        loop
            cntloop = cntloop + 1;
            if cntloop >= maxcntloop1 then
                -- защита от зацикливания
                exit;
            end if;
            p2 = strpos(substr(query_prec,p1+1),'''');
            if p2 > 0 then
                -- сворачиваем найденную константу в ?
                p2 = p1 + p2;
                query_prec = substr(query_prec,1,p1-1) || '?' || ltrim(substr(query_prec,p2+1));
                query_prec = trim(replace(replace(replace(replace(replace(query_prec,'  ',' '),'( ','('),' )',')'),', ',','),' ,',','));
                change_count = change_count + 1;
            else
                exit;
            end if;
            p1 = strpos(query_prec,'''');
        end loop;

        -- конструкция  in (?,...,?) заменяется на  = ?
        cntloop = 0;
        inlist_count = 0;
        p1 = strpos(lower(query_prec), ' in (?,');
        while p1 > 1
        loop
            cntloop = cntloop + 1;
            if cntloop >= maxcntloop2 then
                -- защита от зацикливания
                inlist_count = -(inlist_count + (p2+1-p1-5+1)/2);
                -- Зацикливается, возможно, на такой конструкции:
                -- where reldatabase in (?, (SELECT oid FROM pg_database WHERE datname = current_database()))
                exit;
            end if;
            p2 = strpos(query_prec, ',?)');

            if p2 > 1 and p2 < p1 then
                p2 = strpos(substr(query_prec,p1+1), ',?)');
                if p2 > 0 then
                    p2 = p1 + p2;
                end if;
            end if;

            if p2 > p1 then
                s = substr(query_prec, p1+5, p2+1-p1-5+1);
                if replace(replace(s,'?',''),',','') = ''::text then
                    -- сворачиваем перечисление этих кодов в 1 код
                    query_prec = substr(query_prec,1,p1-1) || ' = ? ' || ltrim(substr(query_prec,p2+3));
                    query_prec = trim(replace(replace(replace(replace(replace(query_prec,'  ',' '),'( ','('),' )',')'),', ',','),' ,',','));
                    change_count = change_count + 1;
                    inlist_count = inlist_count + (p2+1-p1-5+1)/2; -- подсчитаем примерно, сколько в списках IN всего элементов
                end if;
            else
                exit;
            end if;
            p1 = strpos(lower(query_prec), ' in (?,');
        end loop;

        if change_count = 0 then
            query_prec = null;
        end if;
        update t_pg_stat_statements
           set changecount = change_count,
               inlistcount = inlist_count,
               queryprec   = query_prec,
               queryprecid = md5(coalesce(query_prec,query))
         where current of curs;
    end loop;

    create unique index t_pg_stat_statements_unique on t_pg_stat_statements(userid, dbid, queryid, md5queryid, queryprecid);
    update t_pg_stat_statements t
       set newquery = true
     where not exists(select 1 from adm.pg_stat_statement_query q
                       where q.userid     = t.userid
                         and q.dbid       = t.dbid
                         and q.queryid    = t.queryid
                         and q.md5queryid = t.md5queryid
                         and q.queryprecid= t.queryprecid);

    -- Для ранее зарегистрированных запросов, точно таких же, что и вновь выбранных, поставим отметку, когда они были в последний раз отмечены
    update adm.pg_stat_statement_query q
       set regcount = q.regcount + 1,
           lastregtime = t.lastregtime
      from t_pg_stat_statements t
     where not t.newquery
       and q.userid     = t.userid
       and q.dbid       = t.dbid
       and q.queryid    = t.queryid
       and q.md5queryid = t.md5queryid
       and q.queryprecid= t.queryprecid;

    -- Добавим в накопительную таблицу тексты вновь выявленных запросов, которых в ней ещё нет
    insert into adm.pg_stat_statement_query(primarykey, userid, dbid, queryid, md5queryid, queryprecid, changecount, inlistcount,
                                            query, queryprec, createtime, regcount, lastregtime)
    select primarykey, userid, dbid, queryid, md5queryid, queryprecid, changecount, inlistcount,
           query, queryprec, createtime, regcount, lastregtime
      from t_pg_stat_statements t
     where t.newquery;

    -- Добавим в накопительную таблицу сырую статистическую информацию по выбранным сейчас запросам, но без столбца с текстом запроса
    insert into adm.pg_stat_statement_rawdata(registrationtime, userid, dbid, queryid, md5queryid, queryprecid,
                                              calls, total_time, min_time, max_time, mean_time, stddev_time, rows,
                                              shared_blks_hit, shared_blks_read, shared_blks_dirtied, shared_blks_written,
                                              local_blks_hit,  local_blks_read,  local_blks_dirtied,  local_blks_written,
                                              temp_blks_read, temp_blks_written, blk_read_time, blk_write_time)
    select lastregtime as registrationtime, userid, dbid, queryid, md5queryid, queryprecid,
           calls, total_time, min_time, max_time, mean_time, stddev_time, rows,
           shared_blks_hit, shared_blks_read, shared_blks_dirtied, shared_blks_written,
           local_blks_hit,  local_blks_read,  local_blks_dirtied,  local_blks_written,
           temp_blks_read, temp_blks_written, blk_read_time, blk_write_time
      from t_pg_stat_statements;

    -- Добавим в основную накопительную таблицу обработанную статистическую информацию по выбранным сейчас запросам
    with s as (
    select dbid, queryprecid,
           sum(calls) as calls, sum(total_time) as total_time, min(min_time) as min_time, max(max_time) as max_time, avg(mean_time) as mean_time, sum(rows) as rows,
           sum(shared_blks_hit) as shared_blks_hit, sum(shared_blks_read)  as shared_blks_read,  sum(shared_blks_dirtied) as shared_blks_dirtied,  sum(shared_blks_written) as shared_blks_written,
           sum(local_blks_hit)  as local_blks_hit,  sum(local_blks_read)   as local_blks_read,   sum(local_blks_dirtied)  as local_blks_dirtied,   sum(local_blks_written)  as local_blks_written,
           sum(temp_blks_read)  as temp_blks_read,  sum(temp_blks_written) as temp_blks_written, sum(blk_read_time) as blk_read_time, sum(blk_write_time) as blk_write_time,
           count(*) as cnt
      from t_pg_stat_statements
     group by dbid, queryprecid
    )
    insert into adm.pg_stat_statement_data(dbid, queryprecid, registrationtime,
                                           calls, total_time, min_time, max_time, mean_time, stddev_time, rows,
                                           shared_blks_hit, shared_blks_read, shared_blks_dirtied, shared_blks_written,
                                           local_blks_hit,  local_blks_read,  local_blks_dirtied,  local_blks_written,
                                           temp_blks_read, temp_blks_written, blk_read_time, blk_write_time, cnt)
    select dbid, queryprecid, registration_time as registrationtime,
           calls, total_time, min_time, max_time, mean_time, null as stddev_time, rows,
           shared_blks_hit, shared_blks_read, shared_blks_dirtied, shared_blks_written,
           local_blks_hit,  local_blks_read,  local_blks_dirtied,  local_blks_written,
           temp_blks_read, temp_blks_written, blk_read_time, blk_write_time, cnt
      from s;

    $STATRESETSCRIPT
end\$\$;
select 'Зарегистрировано новых запросов ' || coalesce((select count(*) from t_pg_stat_statements where newquery), 0)::varchar ||
       ', уже было ранее зарегистрированных запросов ' || coalesce((select count(*) from t_pg_stat_statements where not newquery), 0)::varchar ||
       ', добавлено новых обработанных записей статистики ' || coalesce((select count(*) from adm.pg_stat_statement_data where registrationtime = '$CLOCKTIMESTAMP'::timestamp(3) with time zone),0)::varchar as resmsg;
"

if [ "$DEBUG_MODE" -eq 0 ]; then
    execsql "$ADMDBNAME" "Сохранение новых sql-запросов по обслуживаемым БД из представления pg_stat_statements в БД $ADMDBNAME и сбросу содержимого pg_stat_statements"
    res=$?
    if [ $res -ne 0 ]; then
        finish "$0" "Досрочное завершение из-за проблем при сохранении новых sql-запросов по обслуживаемым БД из представления pg_stat_statements в БД $ADMDBNAME" "$PROTDBNAME" "$OPERPROTOCOL"
        exit 140
    else
        echo Результат выделения sql-запросов по обслуживаемым БД $RESCODE >>$LOG_FILE
    fi
else
    echo "Сформирован блок команд для сохранения новых sql-запросов по обслуживаемым БД из представления pg_stat_statements в БД $ADMDBNAME и сбросу содержимого pg_stat_statements" >>$LOG_FILE
    echo DEBUG $SQL >>$LOG_FILE
fi

operprotocol_end "$PROTDBNAME"

# После массовых изменений таблицы, особенно с большими текстовыми столбцами, полезно её сжать и проанализировать

operprotocol_start "$PROTDBNAME" default "" "Выполнение vacuum analyze для накопительных таблиц adm.pg_stat_statement_query, adm.pg_stat_statement_rawdata, adm.pg_stat_statement_data в БД $ADMDBNAME"

if [ "$DEBUG_MODE" -eq 0 ]; then
    SQL="vacuum analyze adm.pg_stat_statement_query"
    execsql "$ADMDBNAME" "Сжатие и анализ таблицы adm.pg_stat_statement_query после добавления новых запросов и обработки"

    SQL="vacuum analyze adm.pg_stat_statement_rawdata"
    execsql "$ADMDBNAME" "Сжатие и анализ таблицы adm.pg_stat_statement_rawdata после добавления новых запросов и обработки"

    SQL="vacuum analyze adm.pg_stat_statement_data"
    execsql "$ADMDBNAME" "Сжатие и анализ таблицы adm.pg_stat_statement_data после добавления новых запросов и обработки"
else
    echo "После фактического изменения накопительных таблиц adm.pg_stat_statement_query, adm.pg_stat_statement_rawdata, adm.pg_stat_statement_data в БД $ADMDBNAME для них будет выполнена команда vacuum analyze" >>$LOG_FILE
fi

operprotocol_end "$PROTDBNAME"

finish "$0" "Успешное завершение работы" "$PROTDBNAME" "$OPERPROTOCOL"
exit 0
