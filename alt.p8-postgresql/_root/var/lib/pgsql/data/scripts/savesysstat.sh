#!/bin/bash
# Сценарий savesysstat.sh, предназначенный для регулярного запуска с целью сохранения текущего содержимого системных представлений статистики.
# Зависит от библиотек pgadmlibr.sh и pgadmstatlibr.sh.

# Автор:  Новиков А.М.
# Версия: 1.0
# Дата:   30.04.2020

# История изменений
# Версия: 1.1
# Дата:   19.05.2020
# Обеспечивается протоколирование выполняемых над системными представлениями статистики операций.
# В режиме отладки только генерируются команды на сохранение статистики, но не выполняются.
# Используется версия 1.1 и выше библиотеки pgadmlibr.sh, версия 1.0 и выше библиотеки pgadmstatlibr.sh.

WORKDIR=$1      # Если параметр равен calldir, в каталоге, откуда вызван сценарий, будет создан подкаталог log, если его нет,
                # который будет использован в качестве рабочего.
                # Иначе строка рассматривается как каталог, который должен существовать, но м.б. создан в случае отсутствия.
PGSOCKET=$2     # Если =1, подключение через сокет (это влияет на строку подключения)
ADMDBNAME=$3    # Административная БД, в схеме adm которой сохраняется информация из системных представлений статистики и ведётся протоколирование операций.
                # Сценарий savesysstat.sh будет проверять наличие данной БД на сервере PostgreSQL,
                # к которому задано подключение. В случае её отсутствия прекратит работу.
DBLIST=$4       # Дополнительный список БД, для которых должно выполняться сохранение текущего содержимого системных представлений статистики.
                # Если пусто, используется текущее содержимое таблицы adm.dbmaintenance.
STATRESET1=$5   # 0/1 - сброс содержимого системных представлений статистики на уровне БД после успешного сохранения.
                # По умолчанию 1 - сброс
STATRESET2=$6   # 0/1 - сброс содержимого системных представлений статистики на уровне кластера PostgreSQL после успешного сохранения.
                # По умолчанию 0 - оставить
DEBUG_MODE=$7   # режим отладки, если больше 0. Управляет диагностическими сообщениями в лог-файле, а также фактической возможностью сохранения и сброса
                # содержимого представлений статистики. По умолчанию 0.
PROTOCOL=$8     # 0/1 - протоколирование выполняемых sql-команд. По умолчанию 1.

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

if [ "$STATRESET1" == "" ]; then
    STATRESET1=1
fi
if [ "$STATRESET2" == "" ]; then
    STATRESET2=0
fi

if [ "$DEBUG_MODE" == "" ]; then
    DEBUG_MODE=0
fi
if [ "$PROTOCOL" == "" ]; then
    PROTOCOL=1
fi

YYYYMMDD=`/bin/date '+%Y%m%d'`
LOG_FILE=$WORKDIR/savesysstat_$YYYYMMDD.log
RES1_FILE=$WORKDIR/savesysstat_dbnames.txt
RES2_FILE=$WORKDIR/savesysstat_viewnames.txt
RES3_FILE=$WORKDIR/savesysstat_viewcontent.txt

CURDATE=`/bin/date '+%d.%m.%Y %H:%M:%S.%N'`
CALLCMD="$0 WORKDIR=$WORKDIR PGSOCKET=$PGSOCKET ADMDBNAME=$ADMDBNAME DBLIST=\"$DBLIST\" STATRESET1=$STATRESET1 STATRESET2=$STATRESET2 DEBUG_MODE=$DEBUG_MODE PROTOCOL=$PROTOCOL"
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
checkdb $ADMDBNAME
res=$?
if [ $res -ne 0 ]; then
    finish "$0" "Досрочное завершение из-за проблем при проверке заданной административной БД $ADMDBNAME"
    exit 20
fi

# Проверим наличие схемы adm в административной БД $ADMDBNAME  и создадим её в случае отсутствия
checkchema $ADMDBNAME adm 1
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
operprotocol_start "$ADMDBNAME" default "" "Сохранение содержимого системных представлений статистики" "" null null "$CALLCMD"
res=$?
if [ $res -eq 0 ]; then
    PROTDBNAME="$ADMDBNAME"
    OPERPROTOCOL=$OPERPRIMARYKEY
else
    echo Ошибка $res при регистрации операции. Протоколирование более не ведётся >>$LOG_FILE
    OPERPRIMARYKEY=
    OPERPROTOCOL=
fi

# Проверка наличия в БД $ADMDBNAME служебной таблицы adm.dbmaintenance, хранящей список БД, для которых должно выполняться сохранение статистики
checkdbmaintenance "$ADMDBNAME" savesysstat "$DBLIST"
res=$?
if [ $res -ne 0 ]; then
    finish "$0" "Досрочное завершение из-за проблем при проверке наличия в заданной административной БД $ADMDBNAME служебной таблицы adm.dbmaintenance" "$PROTDBNAME" "$OPERPROTOCOL"
    exit 50
fi

# Проверка наличия в БД $ADMDBNAME таблицы adm.allsysstat, содержащей список системных представлений статистики, из которых должно выполняться сохранение их содержимого >>$LOG_FILE
# Прямо здесь формируем скрипт на такую проверку и выполняем его
SQL="
do \$\$
begin
    -- Создание списка представлений статистики для их регулярного сохранения
    if not exists(select 1 from information_schema.tables where table_schema = 'adm' and table_name = 'allsysstat') then
        create table adm.allsysstat(
            primarykey  uuid not null primary key default public.uuid_generate_v4(),
            statname    name not null,
            actual      boolean not null default true,
            dbonly      boolean not null,
            description varchar(255) null,
            createtime  timestamp(3) with time zone DEFAULT clock_timestamp()
        );
    end if;

    -- Добавляем необходимые представления статистики, если их ещё нет в таблице adm.allsysstat.
    -- Чтобы в дальнейшем какие-то из них исключить из обработки, они уже д.б. там со значением actual = false.
    with s as (
    select 'pg_stat_bgwriter'       as statname, true  as dbonly, 'Только одна строка со статистикой о работе фонового процесса записи.' as description
    union all
    select 'pg_stat_database'       as statname, true  as dbonly, 'Одна строка для каждой базы данных со статистикой на уровне базы.' as description
    union all
    select 'pg_stat_all_tables'     as statname, false as dbonly, 'По одной строке на каждую таблицу в текущей базе данных со статистикой по обращениям к этой таблице.' as description
    union all
    select 'pg_stat_all_indexes'    as statname, false as dbonly, 'По одной строке для каждого индекса в текущей базе данных со статистикой по обращениям к этому индексу.' as description
    union all
    select 'pg_statio_all_tables'   as statname, false as dbonly, 'По одной строке для каждой таблицы в текущей базе данных со статистикой по операциям ввода/вывода для этой таблицы.' as description
    union all
    select 'pg_statio_all_indexes'  as statname, false as dbonly, 'По одной строке для каждого индекса в текущей базе данных со статистикой по операциям ввода/вывода для этого индекса.' as description
    )
    insert into adm.allsysstat(statname, dbonly, description)
    select statname, dbonly, description from s
     where not exists(select 1 from adm.allsysstat a where a.statname = s.statname);
end\$\$;
"
execsql "$ADMDBNAME" "Проверка наличия в БД $ADMDBNAME таблицы adm.allsysstat, содержащей список системных представлений статистики, из которых должно выполняться сохранение их содержимого"
res=$?
if [ $res -ne 0 ]; then
    finish "$0" "Досрочное завершение из-за проблем при проверке таблицы adm.allsysstat в заданной административной БД $ADMDBNAME" "$PROTDBNAME" "$OPERPROTOCOL"
    exit 60
fi

# Проверка наличия в БД $ADMDBNAME накопительных таблиц, для которых должно выполняться сохранение статистики из заданных БД и кластера PostgreSQL в целом >>$LOG_FILE
# Прямо здесь формируем скрипт на такую проверку и выполняем его
SQL="
do \$\$
declare
    r         record;
    statname  name;
    sqlcmd    text;
    dbnamecol text = '';
begin
    -- Перебираем системные представления, помеченные как актуальные, но для которых не созданы накопительные таблицы
    for r in select s.* from adm.allsysstat s
          where s.actual
            and not exists(select 1 from information_schema.tables t
                    where t.table_schema = 'adm' and t.table_name = s.statname)
    loop
        statname = 'adm.\"' || r.statname || '\"';
        if not r.dbonly then
            dbnamecol = '''''::name as datname,';
        end if;
        sqlcmd = '
create table ' || statname || '
as
select public.uuid_generate_v4() as primarykey,' || dbnamecol || ' *, clock_timestamp() as createtime
from pg_catalog.\"' || r.statname || '\"
with no data;

alter table ' || statname || ' add constraint ' || r.statname|| '_pkey  primary key (primarykey);

alter table ' || statname || ' alter column primarykey set default public.uuid_generate_v4();

alter table ' || statname || ' alter column createtime set default clock_timestamp();

create index ' || r.statname || '_createtime on ' || statname || '(createtime);
';
        if not r.dbonly or r.statname = 'pg_stat_database' then
            sqlcmd = sqlcmd || '
create index ' || r.statname || '_datname on ' || statname || '(datname);
';
        end if;

        execute sqlcmd;
    end loop;
end\$\$;
"
execsql "$ADMDBNAME" "Проверка наличия в БД $ADMDBNAME накопительных таблиц, для которых должно выполняться сохранение статистики из заданных БД и кластера PostgreSQL в целом"
res=$?
if [ $res -ne 0 ]; then
    finish "$0" "Досрочное завершение из-за проблем при проверке накопительных таблиц в заданной административной БД $ADMDBNAME" "$PROTDBNAME" "$OPERPROTOCOL"
    exit 70
fi

operprotocol_start "$PROTDBNAME" default "" "Сохранение системных представлений статистики для кластера PostgreSQL в целом"

# Перебираем системные представления статистики для кластера PostgreSQL в целом и сохраняем их содержимое >>$LOG_FILE
# Прямо здесь формируем скрипт и выполняем его. Получим sql-команды для сохранения статистик
SQL="
select 'insert into adm.\"' || statname || '\" select public.uuid_generate_v4() as primarykey, *, clock_timestamp() as createtime from pg_catalog.\"' || statname || '\" s' ||
    case when statname = 'pg_stat_database' then ' where exists(select 1 from adm.dbmaintenance d where d.datname = s.datname and d.actual and d.savesysstat)'
         else ''
    end || '; analyze adm.\"' || statname || '\";'
from adm.allsysstat
where dbonly
order by statname;
"
execsql "$ADMDBNAME" "Перебираем системные представления статистики для кластера PostgreSQL в целом и генерируем sql-команды для сохранения их содержимого"
res=$?
if [ $res -ne 0 ]; then
    finish "$0" "Досрочное завершение из-за проблем при выборке системных представлений статистики уровня PostgreSQL из заданной административной БД $ADMDBNAME" "$PROTDBNAME" "$OPERPROTOCOL"
    exit 80
fi
SQL="$RESCODE"

if [ "$DEBUG_MODE" -eq 0 ]; then
    execsql "$ADMDBNAME" "Выполнение сформированного блока команд для сохранения содержимого системных представлений статистики по кластеру PostgreSQL вцелом"
    res=$?
    if [ $res -ne 0 ]; then
        finish "$0" "Досрочное завершение из-за проблем при сохранении содержимого системных представлений статистики уровня PostgreSQL в заданной административной БД $ADMDBNAME" "$PROTDBNAME" "$OPERPROTOCOL"
        exit 90
    fi
else
    echo "Сформирован блок команд для сохранения содержимого системных представлений статистики по кластеру PostgreSQL вцелом" >>$LOG_FILE
    echo DEBUG $SQL >>$LOG_FILE
fi

operprotocol_end "$PROTDBNAME"

if [ -f $RES1_FILE ]; then
    echo Удаление существующего файла-результата rm -f "$RES1_FILE" >>$LOG_FILE
    rm -f "$RES1_FILE"
fi
# Выбираем заданные БД для сохранения содержимого системных представлений статистики в контексте каждой БД >>$LOG_FILE
# Прямо здесь формируем скрипт и выполняем его. Получим sql-команды для сохранения статистик
SQL="
create temporary table t_dblistforsavesysstat on commit drop
as
select datname from adm.dbmaintenance where actual and savesysstat order by datname;
copy t_dblistforsavesysstat to '$RES1_FILE'
"
execsql "$ADMDBNAME" "Выбираем заданные БД для сохранения содержимого системных представлений статистики в контексте каждой БД"
res=$?
if [ $res -ne 0 ]; then
    finish "$0" "Досрочное завершение из-за проблем при выборке системных представлений статистики уровня БД из заданной административной БД $ADMDBNAME" "$PROTDBNAME" "$OPERPROTOCOL"
    exit 120
fi

if [ ! -f $RES1_FILE ]; then
    finish "$0" "Досрочное завершение из-за отсутствия файла с выбранными БД $RES1_FILE" "$PROTDBNAME" "$OPERPROTOCOL"
    exit 130
fi

if [ ! -s $RES1_FILE ]; then
    echo Файл с выбранными БД $RES1_FILE имеет нулевую длину>>$LOG_FILE
else
    # Перебираем имена БД
    i=1
    PREVLINE=fict
    CURLINE=
    while true; do
        # Считываем i-ую строку файла
        CURLINE=`head -n $i $RES1_FILE | tail -n 1`

        # Если выходим за конец файла, будет возвращаться последняя строка
        if [ "$CURLINE" == "$PREVLINE" ]; then
            # По определению, в файле $RES1_FILE не м.б. повторяющихся строк, выходим
            break
        fi
        #echo $i-я строка:$CURLINE

        operprotocol_start "$PROTDBNAME" default "$CURLINE" "Сохранение системных представлений статистики в контексте БД"

        if [ -f $RES2_FILE ]; then
            rm -f "$RES2_FILE"
        fi
        # Для выбранной БД $CURLINE сохраняем содержимое системных представлений статистики>>$LOG_FILE
        # Прямо здесь формируем скрипт и выполняем его.
        SQL="
        create temporary table t_viewlistforsavesysstat on commit drop
        as
        select statname from adm.allsysstat where actual and not dbonly order by statname;
        copy t_viewlistforsavesysstat to '$RES2_FILE'
        "
        execsql "$ADMDBNAME" "Выбираем список системных представлений статистики для сохранения содержимого в контексте заданной БД"
        res=$?
        if [ $res -ne 0 ]; then
            finish "$0" "Досрочное завершение из-за проблем при выборе списка системных представлений статистики для сохранения содержимого в контексте заданной БД" "$PROTDBNAME" "$OPERPROTOCOL"
            exit 150
        fi

        if [ ! -f $RES2_FILE ]; then
            finish "$0" "Досрочное завершение из-за отсутствия файла с выбранными БД $RES2_FILE" "$PROTDBNAME" "$OPERPROTOCOL"
            exit 160
        fi
        #cat $RES2_FILE

        echo В файле $RES2_FILE имеем список представлений статистики для сохранения >>$LOG_FILE
        j=1
        PREVLINE2=fict
        CURLINE2=
        while true; do
            # Считываем j-ую строку файла
            CURLINE2=`head -n $j $RES2_FILE | tail -n 1`

            # Если выходим за конец файла, будет возвращаться последняя строка
            if [ "$CURLINE2" == "$PREVLINE2" ]; then
                # По определению, в файле $RES2_FILE не м.б. повторяющихся строк, выходим
                break
            fi

            #echo $j-я строка:$CURLINE2
            if [ $DEBUG_MODE -ge 1 ]; then
                echo DB=$CURLINE VIEW=$CURLINE2 >>$LOG_FILE
            fi

            if [ -f $RES3_FILE ]; then
                rm -f "$RES3_FILE"
            fi
            SQL="
            create temporary table t_viewcontentforsavesysstat on commit drop
            as
            select '$CURLINE'::name as datname, *, clock_timestamp() as createtime
              from pg_catalog.\"$CURLINE2\";
            copy t_viewcontentforsavesysstat to '$RES3_FILE' with (format csv, delimiter ';');
            "

            if [ "$DEBUG_MODE" -eq 0 ]; then
                execsql "$CURLINE" "Выгрузка содержимого представления статистики $CURLINE2 для БД $CURLINE в файл $RES3_FILE"
                res=$?
                if [ $res -ne 0 ]; then
                    finish "$0" "Досрочное завершение из-за проблем при выгрузке содержимого представления статистики $CURLINE2 для БД $CURLINE в файл $RES3_FILE" "$PROTDBNAME" "$OPERPROTOCOL"
                    exit $res
                fi

                if [ ! -f $RES3_FILE ]; then
                    finish "$0" "Досрочное завершение из-за отсутствия файла $RES3_FILE с содержимым представления $CURLINE2" "$PROTDBNAME" "$OPERPROTOCOL"
                    exit 170
                fi
            else
                echo "Сформирован блок команд для выгрузки содержимого представления статистики $CURLINE2 для БД $CURLINE в файл $RES3_FILE" >>$LOG_FILE
                echo DEBUG $SQL >>$LOG_FILE
            fi

            # Загружаем содержимое представления статистики $CURLINE2 для БД $CURLINE из файла $RES3_FILE в БД $ADMDBNAME
            SQL="
            create temporary table t_viewcontentforsavesysstat on commit drop
            as
            select '$CURLINE'::name as datname, *, clock_timestamp() as createtime
              from pg_catalog.\"$CURLINE2\" with no data;
            copy t_viewcontentforsavesysstat from '$RES3_FILE' with (format csv, delimiter ';');
            insert into adm.\"$CURLINE2\" select public.uuid_generate_v4() as primarykey, * from t_viewcontentforsavesysstat;
            analyze adm.\"$CURLINE2\";"

            if [ "$DEBUG_MODE" -eq 0 ]; then
                execsql "$ADMDBNAME" "Загружаем содержимое представления статистики $CURLINE2 для БД $CURLINE из файла $RES3_FILE в БД $ADMDBNAME"
                res=$?
                if [ $res -ne 0 ]; then
                    finish "$0" "Досрочное завершение из-за проблем при загрузке содержимого представления статистики $CURLINE2 для БД $CURLINE из файла $RES3_FILE в БД $ADMDBNAME" "$PROTDBNAME" "$OPERPROTOCOL"
                    exit 180
                fi
            else
                echo "Сформирован блок команд для загрузки содержимого представления статистики $CURLINE2 для БД $CURLINE из файла $RES3_FILE в БД $ADMDBNAME" >>$LOG_FILE
                echo DEBUG $SQL >>$LOG_FILE
            fi

            PREVLINE2=$CURLINE2
            j=$[$j+1]
        done

        operprotocol_end "$PROTDBNAME"

        PREVLINE=$CURLINE
        i=$[$i+1]
    done
fi

if [ $STATRESET1 -eq 1 ]; then
    echo Сброс статистики в обслуживаемых БД. Перебираем имена обслуживаемых БД из ранее заполненного файла $RES1_FILE >>$LOG_FILE
    i=1
    PREVLINE=fict
    CURLINE=
    while true; do
        # Считываем i-ую строку файла
        CURLINE=`head -n $i $RES1_FILE | tail -n 1`

        # Если выходим за конец файла, будет возвращаться последняя строка
        if [ "$CURLINE" == "$PREVLINE" ]; then
            # По определению, в файле $RES1_FILE не м.б. повторяющихся строк, выходим
            break
        fi

        operprotocol_start "$PROTDBNAME" default "$CURLINE" "Сброс системных представлений статистики в контексте БД"

        #echo $i-я строка:$CURLINE
        SQL="select pg_stat_reset();"
        if [ $DEBUG_MODE -eq 0 ]; then
            execsql "$CURLINE" "Сброс статистики на уровне БД $CURLINE"
        else
            echo DEBUG $SQL -- БД $CURLINE >>$LOG_FILE
        fi

        operprotocol_end "$PROTDBNAME"

        PREVLINE=$CURLINE
        i=$[$i+1]
    done
fi

if [ $STATRESET2 -eq 1 ]; then
    echo Проверяем возможность сброса статистики на уровне кластера PostgreSQL >>$LOG_FILE

    SQL="select coalesce((select 1 from adm.allsysstat where statname='pg_stat_bgwriter' and actual), -1)"
    execsql "$ADMDBNAME" "Проверяем сохранение содержимого представления статистики pg_stat_bgwriter в БД $ADMDBNAME"
    res=$?
    if [ $res -ne 0 ]; then
        finish "$0" "Досрочное завершение из-за проблем при проверке сохранения представления статистики pg_stat_bgwriter в БД $ADMDBNAME" "$PROTDBNAME" "$OPERPROTOCOL"
        exit $res
    fi

    if [ $RESCODE -eq 1 ]; then
        operprotocol_start "$PROTDBNAME" default "" "Сброс системных представлений статистики для кластера PostgreSQL в целом"

        SQL="select pg_stat_reset_shared('bgwriter');"
        if [ $DEBUG_MODE -eq 0 ]; then
            execsql "$ADMDBNAME" "Сброс статистики на уровне кластера PostgreSQL в представлении pg_stat_bgwriter"
        else
            echo DEBUG $SQL >>$LOG_FILE
        fi

        operprotocol_end "$PROTDBNAME"
    fi
fi

finish "$0" "Успешное завершение работы" "$PROTDBNAME" "$OPERPROTOCOL"
exit 0
