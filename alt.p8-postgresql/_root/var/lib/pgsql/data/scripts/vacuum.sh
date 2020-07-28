#!/bin/bash
DEBUG_MODE=0 # режим отладки, если больше 0
PROTOCOL=1 # 1/0 - 1 протоколировать выполняемые sql-команды
# Сценарий vacuum.sh, предназначенный для регулярного запуска с целью сжатия/анализа таблиц БД командами vacuum, analyze или cluster

# Автор:  Новиков А.М.
# Версия: 1.0
# Дата:   15.04.2020

# История изменений
# Версия: 1.1
# Дата:   23.04.2020
# Первым параметром сделан параметр WORKDIR, задающий рабочую директорию.
# Вторым параметром сделан параметр PGSOCKET для управления подключением к PostgreSQL.
# Изменен способ выборки таблиц для обхода случая краха системного процесса PostgreSQL.
# Перед выполнением sql-оператора, выбирающего имена таблиц, преддварительно удаляется файл результата.
# Некоторые мелочи...

# Версия: 1.2
# Дата:   30.04.2020
# Ряд внутренних действий выражен через библиотеку pgadmlibr.sh версии 1.0 и иные мелочи.

# Версия: 1.3
# Дата:   15.05.2020
# Обеспечивается протоколирование выполняемых над таблицами операций.
# Начиная с 3-го параметра все параметры сдвинуты "вправо". Добавлен 3-й параметр ADMDBNAME.
# Используется версия 1.1 и выше библиотеки pgadmlibr.sh.

WORKDIR=$1      # если параметр равен calldir, в каталоге, откуда вызван сценарий, будет создан подкаталог log, если его нет,
                # который будет использован в качестве рабочего,
                # иначе строка рассматривается как каталог, который должен существовать, но м.б. создан в случае отсутствия
PGSOCKET=$2     # если =1, подключение через со
ADMDBNAME=$3    # Административная БД, в таблице adm.operprotocol которой протоколируются выполняемые над таблицами операции.
                # Если задано имя empty, протоколирование не выполняется.
DBNAME=$4       # БД, для которой выполняется обработка
                # Сценарий vacuum.sh будет проверять наличие данной БД на сервере PostgreSQL,
                # к которому задано подключение. В случае её отсутствия прекратит работу.
                # Также он не будет обрабатывать таблицы в системной БД postgres.
CALLMODE=$5     # режим вызова: full/normal/analyze/cluster/cluster_an, по умолчанию normal.
                # full          = vacuum full analyze
                # normal        = vacuum analyze
                # analyze       = analyze
                # cluster       = cluster
                # cluster_an    = cluster, затем analyze
SCHEMASLIST=$6  # список схем, только для таблиц в которых должна выполняться команда обработки,
                # по умолчанию все, кроме служебных схем information_schema, pg_catalog
                # или параметр tables_in_file, означающий, что следующий параметр задаёт
                # полное имя файла с именами таблиц для обработки.
                # предполагается, что схемы с таким именем в обрабатываемой БД нет, но даже если
                # вдруг она есть, выполнение сценария будет, как описано для этого случая.
                # если параметр не задан в строке вызова или задан, но в виде строки all_schemas,
                # его значением считается значение по умолчанию.
TABLESLIST=$7   # список таблиц, только для которых должна выполняться команда обработки,
                # по умолчанию все таблицы.
                # или через этот параметр задаётся имя файла со списком таблиц в виде схема.таблица,
                # если SCHEMASLIST=tables_in_file
                # имена схемы и таблицы в этом файле д.б. в двойных кавычках, если необходимо,
                # по обычным правилам записи имён в PostgreSQL.
                # этот вариант используется, если надо задать собственный порядок обслуживания таблиц
                # или если последовательность таблиц неудобно задавать через параметр.
                # в случае этого варианта параметра TABLESLIST следующие параметры уже не используются.
                # если параметр не задан в строке вызова или задан, но в виде строки all_tables,
                # его значением считается значение по умолчанию.
TABLESIZEMIN=$8 # ограничение на минимальный размер таблицы для участия в операции, в Кб,
                # по умолчанию 1
TABLESIZEMAX=$9 # ограничение на максимальный размер таблицы для участия в операции, в Кб,
                # по умолчанию максимальное целое со знаком 2147483647
VACUUMORDER=${10} # порядок обработки таблиц (полезен длч CALLMODE=full):
                # alphaasc/alphadesc/tablesizeasc/tablesizedesc - по умолчанию tablesizeasc
                # alphaasc      - сортировка по возрастанию по объединённому имени схема.таблица
                # alphadesc     - сортировка по убыванию по объединённому имени схема.таблица
                # tablesizeasc  - сортировка по возрастанию по объёму таблицы, включая toast-таблицу и индексы
                # tablesizedesc - сортировка по убыванию по объёму таблицы, включая toast-таблицу и индексы
                # для режимов CALLMODE=full/cluster/cluster_an может оказаться полезным задание
                # сортировки по варианту tablesizeasc, т.к. в этом случае сначала физически
                # перестраиваться будут меньшие по суммарному объёму таблицы, освобождая при этом
                # своё неиспользуемое пространство и возвращая его в файловую систему.
                # Т.о. большие по объёму таблицы будут перестраиваться под конец операции,
                # когда для операции над ними будет заведомо больше пространства в файловой системе,
                # чем если бы они сжимались самыми первыми

if [ "$WORKDIR" == "" ]; then
    echo Не заданы параметры вызова
    exit 10
fi

CALLDIR=`dirname $0`
if [ "$WORKDIR" == "calldir" ]; then
    WORKDIR=$CALLDIR/log
fi

if [ "$WORKDIR" != "calldir" ]; then
    if ! [ -d $WORKDIR ]; then
        mkdir $WORKDIR
        # Если при отладке запускали под root или другим пользователем, обеспечим всё для postgres
        chown postgres:postgres $WORKDIR
    fi
fi

NEEDPROTOCOL=1
if [ "$ADMDBNAME" == "" ]; then
    ADMDBNAME=empty
fi
if [ "$ADMDBNAME" == "empty" ]; then
    NEEDPROTOCOL=0
fi

if [ "$CALLMODE" == "" ]; then
    CALLMODE=normal
fi
if [ "$SCHEMASLIST" == "" ]; then
    SCHEMASLIST=*
fi
if [ "$SCHEMASLIST" == "all_schemas" ]; then
    SCHEMASLIST=*
fi
if [ "$TABLESLIST" == "" ]; then
    TABLESLIST=*
fi
if [ "$TABLESLIST" == "all_tables" ]; then
    TABLESLIST=*
fi
if [ "$TABLESIZEMIN" == "" ]; then
    TABLESIZEMIN=0
fi
if [ "$TABLESIZEMAX" == "" ]; then
    TABLESIZEMAX=2147483647
fi
if [ "$VACUUMORDER" == "" ]; then
    VACUUMORDER=tablesizeasc
fi

YYYYMMDD=`/bin/date '+%Y%m%d'`
LOG_FILE=$WORKDIR/vacuum_$YYYYMMDD.log
RES_FILE=$WORKDIR/vacuum_tables.txt

CURDATE=`/bin/date '+%d.%m.%Y %H:%M:%S.%N'`
CALLCMD="$0 WORKDIR=$WORKDIR PGSOCKET=$PGSOCKET ADMDBNAME=$ADMDBNAME DBNAME=$DBNAME CALLMODE=$CALLMODE SCHEMASLIST=$SCHEMASLIST TABLESLIST=$TABLESLIST TABLESIZEMIN=$TABLESIZEMIN TABLESIZEMAX=$TABLESIZEMAX VACUUMORDER=$VACUUMORDER"
echo $CURDATE Запуск $CALLCMD >>$LOG_FILE
echo >>$LOG_FILE

if [ "$PGSOCKET" == "1" ]; then
    CONNECT="--username=postgres --no-password"
else
    CONNECT="--host=localhost --port=5432 --username=postgres --no-password"
fi

command2=
case "$CALLMODE" in
    full)
        command="vacuum full analyze"
        ;;
    normal)
        command="vacuum analyze"
        ;;
    analyze)
        command="analyze"
        ;;
    cluster_an)
        command="cluster" # ВНИМАНИЕ! Таблицы для этой операции уже должны иметь установленный индекс, по которому выполняется кластеризация
        command2="analyze"
        ;;
    cluster)
        command="cluster" # ВНИМАНИЕ! Таблицы для этой операции уже должны иметь установленный индекс, по которому выполняется кластеризация
        ;;
    *)
        command="vacuum analyze"
        echo Неизвестный режим вызова CALLMODE=$CALLMODE, принимается $command >>$LOG_FILE
esac

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

if [ $NEEDPROTOCOL -eq 1 ]; then
    # Будет выполняться протоколирование операций над таблицами
    # Проверим наличие заданной БД $ADMDBNAME
    checkdb $ADMDBNAME
    res=$?
    if [ $res -ne 0 ]; then
        finish "$0" "Досрочное завершение из-за проблем при проверке заданной БД $ADMDBNAME"
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
    operprotocol_start "$ADMDBNAME" default "$DBNAME" "$CALLMODE" "" null null "$CALLCMD"
    res=$?
    if [ $res -eq 0 ]; then
        OPERPROTOCOL=$OPERPRIMARYKEY
    else
        echo Ошибка $res при регистрации операции. Протоколирование более не ведётся >>$LOG_FILE
        ADMDBNAME=empty
        OPERPRIMARYKEY=
        OPERPROTOCOL=
    fi
fi

# Проверим наличие заданной БД $DBNAME
checkdb $DBNAME
res=$?
if [ $res -ne 0 ]; then
    finish "$0" "Досрочное завершение из-за проблем при проверке заданной БД $DBNAME" "$ADMDBNAME" "$OPERPROTOCOL"
    exit 20
fi

if [ "$SCHEMASLIST" == "tables_in_file" ]; then
    RES_FILE="$TABLESLIST"
    if [ "$RES_FILE" == "*" ]; then
        finish "$0" "Досрочное завершение из-за незаданного файла со списком таблиц для обработки" "$ADMDBNAME" "$OPERPROTOCOL"
        exit 30
    fi
    echo Список таблиц для операции $command $command2 будет извлекаться из файла $RES_FILE >>$LOG_FILE
else
    # alphaasc/alphadesc/tablesizeasc/tablesizedesc
    case "$VACUUMORDER" in
        alphaasc)
            ORDERBY=full_table_name
            INDEXEXPR=full_table_name
            ;;
        alphadesc)
            ORDERBY="full_table_name desc"
            INDEXEXPR="full_table_name desc"
            ;;
        tablesizeasc)
            ORDERBY=table_size_kb
            INDEXEXPR="(substr(full_table_name, strpos(full_table_name, '; -- ') + 5)::numeric)"
            ;;
        tablesizedesc)
            ORDERBY="table_size_kb desc"
            INDEXEXPR="(substr(full_table_name, strpos(full_table_name, '; -- ') + 5)::numeric) desc"
            ;;
        *)
            ORDERBY=table_size_kb
            INDEXEXPR="(substr(full_table_name, strpos(full_table_name, '; -- ') + 5)::numeric)"
            echo Неизвестный порядок сортировки таблиц VACUUMORDER=$VACUUMORDER, принимается порядок по увеличению размера таблиц >>$LOG_FILE
    esac

# Предыдущий вариант оператора, который работающий, но на некоторых инсталляциях PostgreSQL приводил к краху PostgreSQL
#    SQL="copy (
#with s as (
#select chr(34) || table_schema || chr(34) || chr(46) || chr(34) || table_name || chr(34) as full_table_name, table_schema, table_name
#from information_schema.tables
#where table_catalog='$DBNAME' and table_type = 'BASE TABLE'
#  and table_schema not in ('information_schema','pg_catalog')
#  and (strpos(',$SCHEMASLIST,', ','||table_schema||',') > 0 or strpos('$SCHEMASLIST', '*') > 0)
#  and (strpos(',$TABLESLIST,',  ','||table_name  ||',') > 0 or strpos('$TABLESLIST',  '*') > 0)
#),
#t as (
#select full_table_name, pg_total_relation_size(full_table_name) / 1024 as table_size_kb
#from s
#)
#select full_table_name
#from t
#where t.table_size_kb between $TABLESIZEMIN and $TABLESIZEMAX
#order by $ORDERBY)
#to '$RES_FILE'
#"

# Новый вариант запроса, позволяющий получить файл с именами таблиц (который не приводит к краху PostgreSQL в некоторых инсталляциях),
# но будет ли соблюден порядок строк в файле с порядком строк в запросе, это вопрос!
    SQL="create temporary table t_tablelistforvacuum on commit drop
as
with s as (
select chr(34) || table_schema || chr(34) || chr(46) || chr(34) || table_name || chr(34) as full_table_name, table_schema, table_name
from information_schema.tables
where table_catalog='$DBNAME' and table_type = 'BASE TABLE'
  and table_schema not in ('information_schema','pg_catalog')
  and (strpos(',$SCHEMASLIST,', ','||table_schema||',') > 0 or strpos('$SCHEMASLIST', '*') > 0)
  and (strpos(',$TABLESLIST,',  ','||table_name  ||',') > 0 or strpos('$TABLESLIST',  '*') > 0)
),
t as (
select full_table_name, pg_total_relation_size(full_table_name) / 1024 as table_size_kb
from s
)
select full_table_name || '; -- ' || table_size_kb::varchar(20) as full_table_name
from t
where t.table_size_kb between $TABLESIZEMIN and $TABLESIZEMAX
order by $ORDERBY;
create index i_tablelistforvacuum on t_tablelistforvacuum($INDEXEXPR);
cluster t_tablelistforvacuum using i_tablelistforvacuum;
copy t_tablelistforvacuum to '$RES_FILE';"

    if [ -f $RES_FILE ]; then
        echo Удаление существующего файла-результата rm -f "$RES_FILE" >>$LOG_FILE
        rm -f "$RES_FILE"
    fi
    execsql "$DBNAME" "Получим список таблиц для операции $command $command2 над БД $DBNAME"
    res=$?
    if [ $res -ne 0 ]; then
        finish "$0" "Досрочное завершение из-за проблем при получении списка таблиц для операции над БД $DBNAME" "$ADMDBNAME" "$OPERPROTOCOL"
        exit $res
    fi

    if [ -f $RES_FILE ]; then
        # Если при отладке запускали под root или другим пользователем, обеспечим всё для postgres
        echo chown postgres:postgres $RES_FILE >>$LOG_FILE
        chown postgres:postgres $RES_FILE
    fi
fi

if [ ! -f $RES_FILE ]; then
    finish "$0" "Досрочное завершение из-за отсутствия файла с выбранными таблицами $RES_FILE" "$ADMDBNAME" "$OPERPROTOCOL"
    exit 40
fi

if [ ! -s $RES_FILE ]; then
    finish "$0" "Файл с выбранными таблицами $RES_FILE имеет нулевую длину" "$ADMDBNAME" "$OPERPROTOCOL"
    exit 50
fi

# Перебираем имена таблиц и запускаем нужную команду
i=1
PREVLINE=fict
CURLINE=
while true; do
    # Считываем i-ую строку файла
    CURLINE=`head -n $i $RES_FILE | tail -n 1`
    #echo $i-я строка:$CURLINE

    # Если выходим за конец файла, будет возвращаться последняя строка
    if [ "$CURLINE" == "$PREVLINE" ]; then
        # По определению, в файле $RES_FILE не м.б. повторяющихся строк, выходим
        break
    fi

    # Основная команда для обработки таблицы
    operprotocol_start "$ADMDBNAME" default "$DBNAME" "$command" "$CURLINE"
    SQL="$command $CURLINE"
    if [ $DEBUG_MODE -eq 0 ]; then
        execsql "$DBNAME"
    else
        echo DEBUG $SQL >>$LOG_FILE
    fi
    operprotocol_end "$ADMDBNAME"
    
    if [ "$command2" != "" ]; then
        # Задана дополнительная команда для обработки таблицы
        operprotocol_start "$ADMDBNAME" default "$DBNAME" "$command2" "$CURLINE"
        SQL="$command2 $CURLINE"
        if [ $DEBUG_MODE -eq 0 ]; then
            execsql "$DBNAME"
        else
            echo DEBUG $SQL >>$LOG_FILE
        fi
        operprotocol_end "$ADMDBNAME"
    fi

    PREVLINE=$CURLINE
    i=$[$i+1]
done
echo Обработано таблиц $i >>$LOG_FILE

finish "$0" "Успешное завершение работы" "$ADMDBNAME" "$OPERPROTOCOL"
exit 0
