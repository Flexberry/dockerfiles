#!/bin/bash
# pgadmstatlibr.sh - библиотека общих функций, используемых в различных сценариях на bash для выполнения административных действий с PostgreSQL по сбору статистики.
# Зависит от библиотеки pgadmlibr.sh, которая д.б. предварительно установлена в сценарии, вызывающем функции данной библиотеки.
# Автор:  Новиков А.М.
# Версия: 1.0
# Дата:   30.04.2020

# Для использования библиотеки pgadmstatlibr.sh в некотором сценарии необходимое после раздела с объявлением всех переменных (по умолчанию - глобальных),
# которые могут использоваться в вызываемых функциях библиотеки, выполнить команду source:
# . ./pgadmstatlibr.sh

# Целесообразно подключиться к библиотеке с выполнением проверок, например:
#CALLDIR=`dirname $0`
#PGADMSTATLIBR=$CALLDIR/pgadmstatlibr.sh
#if [ -f $PGADMSTATLIBR ]; then
#    echo Подключение библиотеки $PGADMSTATLIBR >>$LOG_FILE
#    . $PGADMLIBR
#    res=$?
#    if [ $res -ne 0 ]; then
#        echo Досрочное завершение из-за ошибки при подключении библиотеки $PGADMSTATLIBR >>$LOG_FILE
#        return 200
#    fi
#else
#    echo Не найден файл библиотеки $PGADMSTATLIBR >>$LOG_FILE
#    exit 201
#fi

# checkdbmaintenance - функция для проверки наличия в заданной административной БД служебной таблицы adm.dbmaintenance,
# хранящей список БД, для которых должно выполняться сохранение статистики
# Если при вызове функции не заданы позиционные параметры 6, 7 и 8 для вызова psql, подключения к  серверу и вывода в лог-файл,
# тогда используются глобальные переменные PSQL, CONNECT и LOG_FILE.
# Функция не использует никакие другие глобальные переменные и не изменяет значения указанных глобальных переменных,
# кроме RESCODE, применяемой по внутренних целях.
# Параметры вызова:
# ADMDBNAME=$1  - административная БД
# STATCOL=$2    - столбец (savesysstat или savepgstmtstat) в таблице adm.dbmaintenance, определяющий цель проверки на момент вызова функции
# DBLIST=$3     - дополнительный список БД для обслуживания, возможно, пустой
# TEMPLATE=$4   - если не пусто (или не empty), это некий шаблон, в котором для каждой обслуживаемой БД заменяются специальные значения на атрибуты БД, а именно:
#               - ((DBID))   - на oid базы
#               - ((DBNAME)) - на имя базы
# PROTOCOL=$5   - вести протокол в лог-файле (1) или нет (0)
# Коды возврата:
# 0             - заданная БД существует
# #0            - код ошибки psql или прочие проверок
# RESCODE       - глобальная переменная, применяемая во внутренних целях.
#               - в случае задания параметра TEMPLATE через неё возвращается совокупность строк для каждой обслуживаемой БД с выполненными макроподстановками
#               - (этим способом можно сгенерировать, например, некоторую последовательность команд для каждой обслуживаемой БД)
checkdbmaintenance()
{
    RESCODE=
    local ADMDBNAME=$1
    local STATCOL=$2
    local DBLIST=$3
    local TEMPLATE=$4
    local RESTEMPLATE=
    if [ "$TEMPLATE" == "empty" ]; then
        TEMPLATE=
    fi
    if [ "$TEMPLATE" != "" ]; then
        RESTEMPLATE="select restemplate from t_dbmaintenance_template limit 1;"
    fi
    if [ "$5" != "" ]; then
        local PROTOCOL=$5
    fi
    if [ "$PROTOCOL" == "" ]; then
        PROTOCOL=1
    fi
    if [ "$6" != "" ]; then
        local PSQL=$6
    fi
    if [ "$7" != "" ]; then
        local CONNECT=$7
    fi
    if [ "$8" != "" ]; then
        local LOG_FILE=$8
    fi

    if [ "$ADMDBNAME" == "" ]; then
        echo $0: Не задана БД >>$LOG_FILE
        return 10
    fi

    local savesysstatval=
    local savepgstmtstatval=
    case "$STATCOL" in
        savesysstat)
            savesysstatval=true
            savepgstmtstatval=false
            ;;
        savepgstmtstat)
            savesysstatval=false
            savepgstmtstatval=true
            ;;
        *)
            echo Неизвестное значение столбца статистики $STATCOL >>$LOG_FILE
            return 20
    esac
    
    local SQL="
    do \$\$
    declare
        db_list text = '$DBLIST';
        db_del  text = ' ';
        db_name text;
        db_index integer = 1;
        db_template text = '$TEMPLATE';
        res_template text= '';
        cnt int = 0;
        r record;
    begin
        if not exists(select 1 from information_schema.tables where table_schema = 'adm' and table_name = 'dbmaintenance') then
            -- создаём пустую таблицу для хранения имён баз данных в текущем инстансе, предназначенных для сохранения статистики
            create table adm.dbmaintenance(
                primarykey      uuid not null primary key default public.uuid_generate_v4(),
                datname         name not null,
                actual          boolean not null default true,  -- надо ли вообще обслуживать данную БД
                savesysstat     boolean null,                   -- надо ли для БД сохранять системные представления статистики
                savepgstmtstat  boolean null,                   -- надо ли для БД сохранять представление pg_stat_statements
                description     varchar(255) null,
                createtime      timestamp(3) with time zone DEFAULT clock_timestamp()
            );
        end if;

        -- Заполним таблицу новыми БД, если их ещё нет в ней
        db_name = split_part(db_list, db_del, db_index);
        while coalesce(db_name,'') <> ''
        loop
            if not exists(select 1 from adm.dbmaintenance where datname = db_name) then
                insert into adm.dbmaintenance (datname, actual, savesysstat, savepgstmtstat) values (db_name, true, $savesysstatval, $savepgstmtstatval);
            else
                update adm.dbmaintenance
                   set $STATCOL = true
                 where datname = db_name and actual and not $STATCOL;
            end if;
            db_index = db_index + 1;
            db_name = split_part(db_list, db_del, db_index);
        end loop;

        -- Установим признак актуальности для тех БД, которые объявлены актуальными, но не трогая неактуальные БД.
        -- Это позволит далее работать только с существующими БД.
        update adm.dbmaintenance a
           set actual = exists(select 1 from pg_catalog.pg_database d where d.datname = a.datname and not d.datistemplate)
         where actual;
        
        if length(db_template) > 0 then
            -- Сгенерируем текст по заданному шаблону
            for r in select d.oid, d.datname
                       from adm.dbmaintenance a
                            inner join pg_catalog.pg_database d
                                on d.datname = a.datname 
                      where a.actual and not d.datistemplate
                      order by d.datname
            loop
                cnt = cnt + 1;
                res_template = case when cnt = 1 then '' else res_template || '
' end || replace(replace(db_template,'((DBID))', r.oid::varchar),'((DBNAME))',r.datname);
            end loop;
            
            -- Обеспечим возврат сгенерированного текста
            create temporary table t_dbmaintenance_template(
                restemplate text
            )
            on commit drop;
            insert into t_dbmaintenance_template values(res_template);
        end if;
    end\$\$;
    $RESTEMPLATE
    "
    execsql "$ADMDBNAME" "Проверка наличия в БД $ADMDBNAME служебной таблицы adm.dbmaintenance, хранящей список БД, для которых должно выполняться сохранение статистики"
    res=$?
    return $res
}
# checkpgstatstatementsreset - функция для возврата числа аргументов в функции pg_stat_statements_reset() из пакета расщирения pg_stat_statements. 
# Начиная с PostgreSQL 12 содержит параметры, позволяющие выборочный сброс статистики, накопленной данным пакетом.
# Если при вызове функции не заданы позиционные параметры 3, 4 и 5 для вызова psql, подключения к  серверу и вывода в лог-файл,
# тогда используются глобальные переменные PSQL, CONNECT и LOG_FILE.
# Функция не использует никакие другие глобальные переменные и не изменяет значения указанных глобальных переменных,
# кроме RESCODE, применяемой по внутренних целях.
# Параметры вызова:
# ADMDBNAME=$1  - административная БД, в которой д.б. установлен пакет расширения pg_stat_statements.
# PROTOCOL=$2   - вести протокол в лог-файле (1) или нет (0)
# Коды возврата:
# 0             - заданная БД существует
# #0            - код ошибки psql или прочие проверок
# RESCODE       - глобальная переменная, через которую возвращается результат проверки:
# -1            - пакет расщирения pg_stat_statements не установлен (не найдена функция pg_stat_statements_reset)
# >0            - кол-во параметров у функции pg_stat_statements_reset
checkpgstatstatementsreset()
{
    RESCODE=
    local ADMDBNAME=$1
    if [ "$2" != "" ]; then
        local PROTOCOL=$2
    fi
    if [ "$PROTOCOL" == "" ]; then
        PROTOCOL=1
    fi
    if [ "$3" != "" ]; then
        local PSQL=$3
    fi
    if [ "$4" != "" ]; then
        local CONNECT=$4
    fi
    if [ "$5" != "" ]; then
        local LOG_FILE=$5
    fi

    local SQL="select coalesce((select pronargs from pg_catalog.pg_proc where proname='pg_stat_statements_reset'), -1) as pronargs"
    execsql "$ADMDBNAME" "Проверка количества параметров для функции pg_stat_statements_reset()"
    res=$?
    return $res
}
