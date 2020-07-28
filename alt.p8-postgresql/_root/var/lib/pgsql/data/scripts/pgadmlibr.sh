#!/bin/bash
# pgadmlibr.sh - библиотека общих функций, используемых в различных сценариях на bash для выполнения административных действий с PostgreSQL.
# Автор:  Новиков А.М.
# Версия: 1.0
# Дата:   30.04.2020

# История изменений
# Версия: 1.1
# Дата:   15.05.2020
# Обеспечивается протоколирование выполняемых над таблицами операций.
# Добавлены функции checkoperprotocol(), operprotocol_start(), operprotocol_end(), getuuid(), изменена функция finish().

# Глобальные переменные, которые могут использоваться в данной библиотеке и которые нужно объявить до использования её функций:
# PROTOCOL, LOG_FILE, CONNECT, PSQL, RESCODE, SQL, OPERPRIMARYKEY, OPERPROTOCOL

# Для использования библиотеки pgadmlibr.sh в некотором сценарии необходимое после раздела с объявлением всех переменных (по умолчанию - глобальных),
# которые могут использоваться в вызываемых функциях библиотеки, выполнить команду source:
# . ./pgadmlibr.sh

# Целесообразно подключиться к библиотеке с выполнением проверок, например:
#CALLDIR=`dirname $0`
#PGADMLIBR=$CALLDIR/pgadmlibr.sh
#if [ -f $PGADMLIBR ]; then
#    echo Подключение библиотеки $PGADMLIBR >>$LOG_FILE
#    . $PGADMLIBR
#    res=$?
#    if [ $res -ne 0 ]; then
#        echo Досрочное завершение из-за ошибки при подключении библиотеки $PGADMLIBR >>$LOG_FILE
#        return 100
#    fi
#else
#    echo Не найден файл библиотеки $PGADMLIBR >>$LOG_FILE
#    exit 101
#fi

# execsql - функция для выполнения заданной sql-команды - одиночного оператора или их последовательности.
# Если при вызове функции не заданы позиционные параметры 4, 5, 6 и 7 для передачи текста sql-команды, вызова psql, подключения к серверу и вывода в лог-файл,
# тогда используются глобальные переменные SQL, PSQL, CONNECT и LOG_FILE соответственно.
# Функция не использует никакие другие глобальные переменные и не изменяет значения указанных глобальных переменных,
# кроме RESCODE, через которую передаётся результат выполнения sql-команды (оператора select в ней, например).
# Параметры вызова:
# DBNAME=$1     - имя БД для подключения
# MSGTEXT=$2    - дополнительный текст для протокола или empty
# PROTOCOL=$3   - вести протокол в лог-файле (1) или нет (0)
# SQL=$4        - текст sql-команды, если параметр не задан, используется глобальная переменная SQL
# Коды возврата:
# 0             - успешное выполнение команды в psql
# #0            - код ошибки psql
# RESCODE       - глобальная переменная, через которую возвращается результат sql-команды. Перед её выполнением очищается.
execsql()
{
    RESCODE=
    local DBNAME=$1
    local MSGTEXT=$2
    if [ "$MSGTEXT" == "empty" ]; then
        MSGTEXT=
    fi
    if [ "$3" != "" ]; then
        local PROTOCOL=$3
    fi
    if [ "$PROTOCOL" == "" ]; then
        PROTOCOL=1
    fi
    if [ "$4" != "" ]; then
        local SQL=$4
    fi
    if [ "$5" != "" ]; then
        local PSQL=$5
    fi
    if [ "$6" != "" ]; then
        local CONNECT=$6
    fi
    if [ "$7" != "" ]; then
        local LOG_FILE=$7
    fi

    if [ "$DBNAME" == "" ]; then
        echo $0: Не задана БД для подключения >>$LOG_FILE
        return 10
    fi
    if [ "$SQL" == "" ]; then
        echo $0: Не задан текст sql-команды для выполнения >>$LOG_FILE
        return 20
    fi
    
    # Заплатка, чтоб не лепить ещё один позиционный параметр к вызову функции
    local INTRANSACTION="-1"
    local ISVACUUM=${SQL:0:6}
    if [ "$ISVACUUM" == "vacuum" ]; then
        INTRANSACTION=
    fi
    
    if [ "$MSGTEXT" != "" ]; then
        echo $MSGTEXT >>$LOG_FILE
    fi
    if [ $PROTOCOL -eq 1 ]; then
        echo $PSQL $CONNECT -d $DBNAME -t -q $INTRANSACTION -c "$SQL" >>$LOG_FILE
    fi
    RESCODE=`$PSQL $CONNECT -d $DBNAME -t -q $INTRANSACTION -c "$SQL"`
    local res=$?
    if [ $PROTOCOL -eq 1 ]; then
        echo $0: Выполнена sql-команда, код завершения $res >>$LOG_FILE
    fi
    if [ $res -ne 0 ]; then
        echo $0: Досрочное завершение из-за ошибки psql $res при выполнении sql-команды >>$LOG_FILE
        return $res
    fi
    return 0
}

# checkdb - функция для проверки существования в текущем кластере PostgreSQL заданной БД.
# Косвенно при этом проверяется возможность подключения к PostgreSQL.
# Если при вызове функции не заданы позиционные параметры 3, 4 и 5 для вызова psql, подключения к  серверу и вывода в лог-файл,
# тогда используются глобальные переменные PSQL, CONNECT и LOG_FILE.
# Функция не использует никакие другие глобальные переменные и не изменяет значения указанных глобальных переменных,
# кроме RESCODE, применяемой по внутренних целях.
# Параметры вызова:
# DBNAME=$1     - проверяемая БД
# PROTOCOL=$2   - вести протокол в лог-файле (1) или нет (0)
# Коды возврата:
# 0             - заданная БД существует
# #0            - код ошибки psql или прочие проверок
# RESCODE       - глобальная переменная, применяемая во внутренних целях
checkdb()
{
    RESCODE=
    local DBNAME=$1
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

    if [ "$DBNAME" == "" ]; then
        echo $0: Не задана БД >>$LOG_FILE
        return 10
    fi

    if [ "$DBNAME" == "postgres" ]; then
        echo $0: БД postgres не предусмотрена в качестве БД для опрераций>>$LOG_FILE
        return 20
    fi
    
    local SQL="select coalesce((select oid::integer from pg_catalog.pg_database where datname='$DBNAME' and not datistemplate),-1) as oid"
    execsql postgres "Проверка наличия БД $DBNAME" $PROTOCOL "$SQL" "$PSQL" "$CONNECT" "$LOG_FILE"
    local res=$?
    if [ $res -ne 0 ]; then
        return $res
    fi
    if [ $RESCODE -le 0 ]; then
        echo $0: Досрочное завершение из-за отсутствия заданной БД $DBNAME в текущем кластере PostgreSQL >>$LOG_FILE
        return 30
    fi

    return 0
}

# checkchema - функция для проверки существования заданной схемы в заданной БД и её создания в случае отсутствия.
# Бывает полезным хранить свои объекты не в общей схеме public, а в отдельной схеме.
# Тогда необходимо хотя бы проверять её существование в текущей БД и создавать её в случае отсутствия, есил это задано.
# Если при вызове функции не заданы позиционные параметры 5, 6 и 7 для вызова psql, подключения к  серверу и вывода в лог-файл,
# тогда используются глобальные переменные PSQL, CONNECT и LOG_FILE.
# Функция не использует никакие другие глобальные переменные и не изменяет значения указанных глобальных переменных,
# кроме RESCODE, применяемой по внутренних целях.
# Параметры вывода:
# DBNAME=$1             - имя БД, в которой проверяется наличие схемы
# SCHEMANAME=$2         - имя схемы
# CREATEIFNOTEXIST=$3   - 1/0, 1 - создать схему в заданной БД, если её там нет
# PROTOCOL=$4           - вести протокол в лог-файле (1) или нет (0)
# Коды возврата:
# 0                     - заданная схема существует (или успешно создана)
# #0                    - код ошибки psql или прочие проверок
# RESCODE               - глобальная переменная, применяемая во внутренних целях
checkchema()
{
    local DBNAME=$1
    local SCHEMANAME=$2
    local CREATEIFNOTEXIST=$3 # 1/0
    if [ "$CREATEIFNOTEXIST" == "" ]; then
        CREATEIFNOTEXIST=1
    fi
    if [ "$4" != "" ]; then
        local PROTOCOL=$4
    fi
    if [ "$PROTOCOL" == "" ]; then
        PROTOCOL=1
    fi
    if [ "$5" != "" ]; then
        local PSQL=$5
    fi
    if [ "$6" != "" ]; then
        local CONNECT=$6
    fi
    if [ "$7" != "" ]; then
        local LOG_FILE=$7
    fi

    if [ "$SCHEMANAME" == "" ]; then
        echo $0: Не задано имя схемы для проверки >>$LOG_FILE
        return 10
    fi
    
    local SQL="select coalesce((select 1 from pg_catalog.pg_namespace where nspname = '$SCHEMANAME'),0)"
    execsql "$DBNAME" "Проверка существования схемы $SCHEMANAME в БД $DBNAME" $PROTOCOL "$SQL" "$PSQL" "$CONNECT" "$LOG_FILE"
    local res=$?
    if [ $res -ne 0 ]; then
        return $res
    fi

    if [ $RESCODE -ne 1 ]; then
        if [ $CREATEIFNOTEXIST -eq 1 ]; then
            SQL="create schema \"$SCHEMANAME\";"
            execsql "$DBNAME" "Создание схемы $SCHEMANAME в БД $DBNAME" $PROTOCOL "$SQL" "$PSQL" "$CONNECT" "$LOG_FILE"
            res=$?
            if [ $res -ne 0 ]; then
                return $res
            else
                echo $0: Схема $SCHEMANAME успешно создана в БД $DBNAME >>$LOG_FILE
                return 0
            fi
        else
            return 20
        fi
    fi

    return 0
}

# checkoperprotocol - функция для проверки существования таблицы протоколирования операций adm.operprotocol и её создания в случае отсутствия.
# Если при вызове функции не заданы позиционные параметры 4, 5 и 6 для вызова psql, подключения к  серверу и вывода в лог-файл,
# тогда используются глобальные переменные PSQL, CONNECT и LOG_FILE.
# Функция не использует никакие другие глобальные переменные и не изменяет значения указанных глобальных переменных,
# кроме RESCODE, применяемой по внутренних целях.
# Параметры вывода:
# ADMDBNAME=$1          - имя административной БД, в схеме adm которой проверяется наличие таблицы operprotocol
# CREATEIFNOTEXIST=$2   - 1/0, 1 - создать таблицу adm.operprotocol в заданной БД, если её там нет
# PROTOCOL=$3           - вести протокол в лог-файле (1) или нет (0)
# Коды возврата:
# 0                     - таблица adm.operprotocol существует (или успешно создана)
# #0                    - код ошибки psql или прочие проверок
# RESCODE               - глобальная переменная, применяемая во внутренних целях
checkoperprotocol()
{
    RESCODE=
    local ADMDBNAME=$1
    local CREATEIFNOTEXIST=$2 # 1/0
    if [ "$CREATEIFNOTEXIST" == "" ]; then
        CREATEIFNOTEXIST=1
    fi
    if [ "$3" != "" ]; then
        local PROTOCOL=$3
    fi
    if [ "$PROTOCOL" == "" ]; then
        PROTOCOL=1
    fi
    if [ "$4" != "" ]; then
        local PSQL=$4
    fi
    if [ "$5" != "" ]; then
        local CONNECT=$5
    fi
    if [ "$6" != "" ]; then
        local LOG_FILE=$6
    fi

    local SQL="select coalesce((select 1 from information_schema.tables where table_schema = 'adm' and table_name = 'operprotocol'),0)"
    execsql "$ADMDBNAME" "Проверка существования таблицы adm.operprotocol в БД $ADMDBNAME" $PROTOCOL "$SQL" "$PSQL" "$CONNECT" "$LOG_FILE"
    local res=$?
    if [ $res -ne 0 ]; then
        return $res
    fi

    if [ $RESCODE -ne 1 ]; then
        if [ $CREATEIFNOTEXIST -eq 1 ]; then
            SQL="create table adm.operprotocol(
primarykey   uuid not null primary key DEFAULT public.uuid_generate_v4(),
operprotocol uuid null,
operagent    character varying(255) not null,
operdb       name not null,
opername     character varying(255) not null,
operobject   character varying(255) null,
operstart    timestamp(3) with time zone not null DEFAULT clock_timestamp(),
operend      timestamp(3) with time zone null,
operpunct    smallint not null,
opernotes    text null
);
create index operprotocol_operagent  on adm.operprotocol(operagent);
create index operprotocol_operdb     on adm.operprotocol(operdb);
create index operprotocol_opername   on adm.operprotocol(opername);
create index operprotocol_operobject on adm.operprotocol(operobject);
create index operprotocol_operstart  on adm.operprotocol(operstart);
create index operprotocol_operend    on adm.operprotocol(operend);
create index operprotocol_fk         on adm.operprotocol(operprotocol) where operprotocol is not null;
"
            execsql "$ADMDBNAME" "Создание таблицы adm.operprotocol в БД $ADMDBNAME" $PROTOCOL "$SQL" "$PSQL" "$CONNECT" "$LOG_FILE"
            res=$?
            if [ $res -ne 0 ]; then
                return $res
            else
                echo $0: Таблица adm.operprotocol успешно создана в БД $ADMDBNAME >>$LOG_FILE
                return 0
            fi
        else
            return 20
        fi
    fi

    return 0
}

# operprotocol_start - функция для установки отметки о начале новой операции в таблице протоколирования операций adm.operprotocol.
# Если при вызове функции не заданы позиционные параметры 11, 12 и 13 для вызова psql, подключения к  серверу и вывода в лог-файл,
# тогда используются глобальные переменные PSQL, CONNECT и LOG_FILE.
# Функция не использует никакие другие глобальные переменные и не изменяет значения указанных глобальных переменных,
# кроме RESCODE, применяемой по внутренних целях, а также OPERPRIMARYKEY, через которую возвращается первичный ключ добавленной записи.
# Параметры вывода:
# ADMDBNAME=$1          - имя административной БД, в схеме adm которой д.б. таблица operprotocol
#                       - если имя БД задано как empty, протоколирование не выполняется
# OPERAGENT=$2          - наименование агента, выполняющего протоколирование операций (напр., наименование сценария без пути)
#                       - если задано default, используется имя файла из $0
# OPERDB=$3             - имя целевой БД, для которой регистрируется операция
# OPERNAME=$4           - наименование операции (в т.ч. название sql-команды)
# OPEROBJECT=$5         - наименование объекта операции (в т.ч. название таблицы и т.п.
# OPERSTART=$6          - дата начала операции или null, по умолчанию null
# OPEREND=$7            - дата окончания операции или null, по умолчанию null
# OPERNOTES=$8          - некоторые заметки по регистрируемой операции, по умолчанию null
# OPERPROTOCOL=$9       - ссылка на главную запись по отношению к создаваемой, по умолчанию используется глобальная переменная OPERPROTOCOL
# PROTOCOL=$10          - вести протокол в лог-файле (1) или нет (0)
# Коды возврата:
# 0                     - успешное завершение
# #0                    - код ошибки psql или прочие проверок
# RESCODE               - глобальная переменная, применяемая во внутренних целях
# OPERPRIMARYKEY        - глобальная переменная, содержащая primarykey добавленной записи в таблицу adm.operprotocol
operprotocol_start()
{
    RESCODE=
    OPERPRIMARYKEY=
    local ADMDBNAME=$1
    if [ "$ADMDBNAME" == "empty" ]; then
        return 0
    fi
    local OPERAGENT=$2
    if [ "$OPERAGENT" == "default" ]; then
        OPERAGENT=`basename $0`
    fi
    local OPERDB=$3
    local OPERNAME=$4
    local OPEROBJECT=$5
    if [ "$OPEROBJECT" == "" ]; then
        OPEROBJECT=null
    else
        OPEROBJECT="'$OPEROBJECT'"
    fi
    local OPERSTART=$6
    if [ "$OPERSTART" == "" ]; then
        OPERSTART=null
    fi
    if [ "$OPERSTART" == "null" ]; then
        OPERSTART="clock_timestamp()"
    else
        OPERSTART="'$OPERSTART'"
    fi
    local OPEREND=$7
    if [ "$OPEREND" == "" ]; then
        OPEREND=null
    fi
    if [ "$OPEREND" != "null" ]; then
        OPEREND="'$OPEREND'"
    fi
    local OPERNOTES=$8
    if [ "$OPERNOTES" == "" ]; then
        OPERNOTES=null
    fi
    if [ "$OPERNOTES" != "null" ]; then
        OPERNOTES="'$OPERNOTES'"
    fi
    if [ "$9" != "" ]; then
        local OPERPROTOCOL=$9
    fi
    local LOC_OPERPROTOCOL=$OPERPROTOCOL
    if [ "$LOC_OPERPROTOCOL" == "" ]; then
        LOC_OPERPROTOCOL=null
    fi
    if [ "$LOC_OPERPROTOCOL" != "null" ]; then
        LOC_OPERPROTOCOL="trim('$LOC_OPERPROTOCOL')::uuid"
    fi
    if [ "${10}" != "" ]; then
        local PROTOCOL=${10}
    fi
    if [ "$PROTOCOL" == "" ]; then
        PROTOCOL=1
    fi
    if [ "${11}" != "" ]; then
        local PSQL=${11}
    fi
    if [ "${12}" != "" ]; then
        local CONNECT=${12}
    fi
    if [ "${13}" != "" ]; then
        local LOG_FILE=${13}
    fi

    getuuid "$ADMDBNAME" $PROTOCOL "$PSQL" "$CONNECT" "$LOG_FILE"
    local res=$?
    if [ $res -ne 0 ]; then
        return $res
    fi
    local PRIMARYKEY=$RESCODE

    local SQL="insert into adm.operprotocol
(primarykey, operprotocol, operagent, operdb, opername, operobject, operstart, operend, operpunct, opernotes) 
values(trim('$PRIMARYKEY')::uuid, $LOC_OPERPROTOCOL, '$OPERAGENT', '$OPERDB', '$OPERNAME', $OPEROBJECT, $OPERSTART::timestamp(3) with time zone, $OPEREND::timestamp(3) with time zone, 0, $OPERNOTES);"
    execsql "$ADMDBNAME" "Регистрация новой операции $OPERDB, $OPERNAME, $OPEROBJECT в таблице adm.operprotocol в БД $ADMDBNAME" $PROTOCOL "$SQL" "$PSQL" "$CONNECT" "$LOG_FILE"
    local res=$?
    if [ $res -eq 0 ]; then
        OPERPRIMARYKEY=$PRIMARYKEY
    fi

    return $res
}

# operprotocol_end - функция для установки отметки о завершении заданной операции (или её этапа) в таблице протоколирования операций adm.operprotocol.
# Последовательный вызов данной функции для одного и того же значения первичного ключа приводит к установке текущего времени в столбце,
# увеличению счётчика operpunct на 1, добавления нового непустого значения в параметре OPERNOTES к столбцу opernotes через точку с пробелом.
# Если при вызове функции не заданы позиционные параметры 5, 6 и 7 для вызова psql, подключения к  серверу и вывода в лог-файл,
# тогда используются глобальные переменные PSQL, CONNECT и LOG_FILE.
# Функция не использует никакие другие глобальные переменные и не изменяет значения указанных глобальных переменных,
# кроме RESCODE, применяемой по внутренних целях.
# Параметры вывода:
# ADMDBNAME=$1          - имя административной БД, в схеме adm которой проверяется наличие таблицы operprotocol
#                       - если имя БД задано как empty, протоколирование не выполняется
# OPERNOTES=$2          - некоторые заметки по регистрируемой операции, по умолчанию null
# OPERPRIMARYKEY=$3     - первичный ключ, по умолчанию берётся из глобальной переменной OPERPRIMARYKEY
#                       - если первичный ключ не задан или равен null/empty, протоколирование не выполняется
# PROTOCOL=$4           - вести протокол в лог-файле (1) или нет (0)
# Коды возврата:
# 0                     - заданная схема существует (или успешно создана)
# #0                    - код ошибки psql или прочие проверок
# RESCODE               - глобальная переменная, применяемая во внутренних целях, содержащая primarykey добавленной записи в таблицу adm.operprotocol
operprotocol_end()
{
    RESCODE=
    local ADMDBNAME=$1
    if [ "$ADMDBNAME" == "empty" ]; then
        return 0
    fi

    local OPERNOTES=$2
    if [ "$OPERNOTES" == "" ]; then
        OPERNOTES=null
    fi
    if [[ "$OPERNOTES" != "null" && "$OPERNOTES" != "empty" ]]; then
        OPERNOTES="'$OPERNOTES'"
    fi
    if [ "$3" != "" ]; then
        local OPERPRIMARYKEY=$3
    fi
    if [ "$OPERPRIMARYKEY" == "" ]; then
        OPERPRIMARYKEY=null
    fi
    if [[ "$OPERPRIMARYKEY" == "null" || "$OPERPRIMARYKEY" == "empty" ]]; then
        return 0
    fi
    
    if [ "$4" != "" ]; then
        local PROTOCOL=$4
    fi
    if [ "$PROTOCOL" == "" ]; then
        PROTOCOL=1
    fi
    if [ "$5" != "" ]; then
        local PSQL=$5
    fi
    if [ "$6" != "" ]; then
        local CONNECT=$6
    fi
    if [ "$7" != "" ]; then
        local LOG_FILE=$7
    fi

    local SQL="
do \$\$
declare
    cnt bigint;
    oper_notes text = trim($OPERNOTES);
begin
    update adm.operprotocol
       set operend   = clock_timestamp(),
           operpunct = operpunct+1,
           opernotes = case when nullif(opernotes,'') is null then oper_notes
                            when oper_notes is not null then rtrim(opernotes) || case when right(rtrim(opernotes),1) = '.' then ' ' else '. ' end || oper_notes
                            else opernotes
                       end
     where primarykey = trim('$OPERPRIMARYKEY')::uuid;

    get diagnostics cnt = ROW_COUNT;
    if cnt = 0 then
        raise exception 'В таблице протоколирования операций adm.operprotocol в БД $ADMDBNAME отсутствует запись для primarykey=$OPERPRIMARYKEY';
    end if;
end\$\$;
"
    execsql "$ADMDBNAME" "Завершение операции в таблице adm.operprotocol в БД $ADMDBNAME по primarykey=$OPERPRIMARYKEY" $PROTOCOL "$SQL" "$PSQL" "$CONNECT" "$LOG_FILE"
    local res=$?
    return $res
}

# checkextension - функция для проверки, установлен ли в заданной БД заданный пакет расширения.
# Если при вызове функции не заданы позиционные параметры 5, 6 и 7 для вызова psql, подключения к  серверу и вывода в лог-файл,
# тогда используются глобальные переменные PSQL, CONNECT и LOG_FILE.
# Функция не использует никакие другие глобальные переменные и не изменяет значения указанных глобальных переменных,
# кроме RESCODE, применяемой по внутренних целях.
# Параметры вывода:
# DBNAME=$1             - имя БД, в которой проверяется наличие схемы
# EXTENSIONNAME=$2      - имя пакета расширения
# CREATEIFNOTEXIST=$3   - 1/0, 1 - установить пакет расширения в заданной БД, если его там нет
# PROTOCOL=$4           - вести протокол в лог-файле (1) или нет (0)
# Коды возврата:
# 0                     - заданный пакет расширения уже был установлен или успешно установлен в результате вызова функции
# #0                    - код ошибки psql или прочие проверок
# RESCODE               - глобальная переменная, применяемая во внутренних целях
checkextension()
{
    local DBNAME=$1
    local EXTENSIONNAME=$2
    local CREATEIFNOTEXIST=$3 # 1/0
    if [ "$CREATEIFNOTEXIST" == "" ]; then
        CREATEIFNOTEXIST=1
    fi
    if [ "$4" != "" ]; then
        local PROTOCOL=$4
    fi
    if [ "$PROTOCOL" == "" ]; then
        PROTOCOL=1
    fi
    if [ "$5" != "" ]; then
        local PSQL=$5
    fi
    if [ "$6" != "" ]; then
        local CONNECT=$6
    fi
    if [ "$7" != "" ]; then
        local LOG_FILE=$7
    fi

    if [ "$EXTENSIONNAME" == "" ]; then
        echo $0: Не задано имя пакета расширения для проверки >>$LOG_FILE
        return 10
    fi

    local SQL="select coalesce((select 1 from pg_extension where extname = '$EXTENSIONNAME'), 0)"
    execsql "$DBNAME" "Проверка установленного пакета расширения $EXTENSIONNAME в БД $DBNAME" $PROTOCOL "$SQL" "$PSQL" "$CONNECT" "$LOG_FILE"
    local res=$?
    if [ $res -ne 0 ]; then
        return $res
    else
        if [ $RESCODE -eq 0 ]; then
            SQL="create extension \"$EXTENSIONNAME\";"
            execsql "$DBNAME" "Установка неустановленного пакета расширения $EXTENSIONNAME в БД $DBNAME" $PROTOCOL "$SQL" "$PSQL" "$CONNECT" "$LOG_FILE"
            res=$?
            if [ $res -ne 0 ]; then
                return $res
            else
                echo $0: Пакет расширения $EXTENSIONNAME успешно установлен в БД $DBNAME >>$LOG_FILE
            fi
        fi
    fi
    return 0
}

# checksharedpreloadlibraries - функция для проверки, установлена ли в файле конфигурации PostgreSQL необходимая разделяемая библиотека,
# загружаемая при запуске PostgreSQL.
# Например, прежде чем устанавливать в БД пакет расширения pg_stat_statements, соответствующая библиотека д.б. задана в параметре
# конфигурации shared_preload_libraries.
# Если при вызове функции не заданы позиционные параметры 4, 5 и 6 для вызова psql, подключения к  серверу и вывода в лог-файл,
# тогда используются глобальные переменные PSQL, CONNECT и LOG_FILE.
# Функция не использует никакие другие глобальные переменные и не изменяет значения указанных глобальных переменных,
# кроме RESCODE, применяемой по внутренних целях и для передачи результата.
# Параметры вывода:
# DBNAME=$1             - имя БД для подключения
# LIBRARYNAME=$2        - имя пакета расширения
# PROTOCOL=$3           - вести протокол в лог-файле (1) или нет (0)
# Коды возврата:
# 0                     - проверки выполнились успешно
# #0                    - код ошибки psql или прочие проверок
# RESCODE               - глобальная переменная: 1/0 - 1 означает, что библиотека установлена
checksharedpreloadlibraries()
{
    local DBNAME=$1
    local LIBRARYNAME=$2
    if [ "$3" != "" ]; then
        local PROTOCOL=$3
    fi
    if [ "$PROTOCOL" == "" ]; then
        PROTOCOL=1
    fi
    if [ "$4" != "" ]; then
        local PSQL=$4
    fi
    if [ "$5" != "" ]; then
        local CONNECT=$5
    fi
    if [ "$6" != "" ]; then
        local LOG_FILE=$6
    fi

    if [ "$LIBRARYNAME" == "" ]; then
        echo $0: Не задано имя библиотеки для проверки >>$LOG_FILE
        return 10
    fi
    
    local SQL="select coalesce(
(select 1 from pg_catalog.pg_settings 
  where name = '$LIBRARYNAME' 
    and (setting = '$LIBRARYNAME' or
         setting similar to '%[^_a-z0-9]*$LIBRARYNAME[^_a-z0-9]*%' or
         setting similar to '%[^_a-z0-9]*$LIBRARYNAME%' or
         setting similar to '$LIBRARYNAME[^_a-z0-9]*%')), 0)"
    execsql "$DBNAME" "Проверка задания разделяемой библиотеки $LIBRARYNAME в файле конфигурации PostgreSQL" $PROTOCOL "$SQL" "$PSQL" "$CONNECT" "$LOG_FILE"
    res=$?
    if [ $res -eq 0 ]; then
        if [ $RESCODE -eq 1 ]; then
            echo $0: Разделяемая библиотека $LIBRARYNAME установлена в файле конфигурации PostgreSQL >>$LOG_FILE
        fi
    fi
    return $res
}

# getdboids1 - функция, выдающая список через запятую значений oid заданных БД.
# Если при вызове функции не заданы позиционные параметры 4, 5 и 6 для вызова psql, подключения к  серверу и вывода в лог-файл,
# тогда используются глобальные переменные PSQL, CONNECT и LOG_FILE.
# Функция не использует никакие другие глобальные переменные и не изменяет значения указанных глобальных переменных,
# кроме RESCODE, применяемой по внутренних целях и для передачи результата.
# Параметры вывода:
# DBNAME=$1      - имя БД для подключения
# DBLIST=$2      - список БД через пробел
# PROTOCOL=$3    - вести протокол в лог-файле (1) или нет (0)
# Коды возврата:
# 0              - проверки выполнились успешно
# #0             - код ошибки psql или прочие проверок
# RESCODE        - глобальная переменная, содержащая список слитных значений oid через запятую
getdboids1()
{
    RESCODE=
    local DBNAME=$1
    local DBLIST=$2
    if [ "$3" != "" ]; then
        local PROTOCOL=$3
    fi
    if [ "$PROTOCOL" == "" ]; then
        PROTOCOL=1
    fi
    if [ "$4" != "" ]; then
        local PSQL=$4
    fi
    if [ "$5" != "" ]; then
        local CONNECT=$5
    fi
    if [ "$6" != "" ]; then
        local LOG_FILE=$6
    fi

    if [ "$DBLIST" == "" ]; then
        echo $0: Не задан список БД для определения oid >>$LOG_FILE
        return 10
    fi
    local SQL="
do \$\$
declare
    db_list  text = 'DBLIST';
        db_del   text = ' ';
        db_name  text;
        db_index integer = 1;
        db_oid   oid;
begin
    create temporary table t_oids(
        id oid
    )
    on commit drop;

    db_name = split_part(db_list, db_del, db_index);
    while coalesce(db_name,'') <> ''
    loop
        if db_name <> 'postgres' then
            db_oid = null;
            select oid into db_oid from pg_catalog.pg_database where datname = db_name;
            if db_oid is not null and not exists(select 1 from t_oids where id = db_oid) then
                insert into t_oids(id) values(db_oid);
            end if;
            db_index = db_index + 1;
            db_name = split_part(db_list, db_del, db_index);
        end if;
    end loop;
end\$\$;
select string_agg(id::varchar,',') from t_oids;
"
    execsql "$DBNAME" "Получение списка oid для списка БД $DBLIST" $PROTOCOL "$SQL" "$PSQL" "$CONNECT" "$LOG_FILE"
    res=$?
    if [ $res -eq 0 ]; then
        echo $0: Для списка БД $DBLIST получен список oid $RESCODE >>$LOG_FILE
    fi

    return $res
}

# getdboids2 - функция, выдающая список через запятую значений oid для БД из таблицы adm.dbmaintenance.
# Если при вызове функции не заданы позиционные параметры 4, 5 и 6 для вызова psql, подключения к  серверу и вывода в лог-файл,
# тогда используются глобальные переменные PSQL, CONNECT и LOG_FILE.
# Функция не использует никакие другие глобальные переменные и не изменяет значения указанных глобальных переменных,
# кроме RESCODE, применяемой по внутренних целях и для передачи результата.
# Параметры вывода:
# ADMDBNAME=$1   - имя административной БД для подключения
# DBCOND=$2      - дополнительное условие на записи таблицы adm.dbmaintenance или пусто
#                - это м.б. один из её столбцов - savesysstat, savepgstmtstat - или произвольное условие, 
#                - но для ссылки на столбцы таблицы adm.dbmaintenance используется алиас "a"
# PROTOCOL=$3    - вести протокол в лог-файле (1) или нет (0)
# Коды возврата:
# 0              - проверки выполнились успешно
# #0             - код ошибки psql или прочие проверок
# RESCODE        - глобальная переменная, содержащая список слитных значений oid через запятую
getdboids2()
{
    RESCODE=
    local ADMDBNAME=$1
    local DBCOND=$2
    if [ "$3" != "" ]; then
        local PROTOCOL=$3
    fi
    if [ "$PROTOCOL" == "" ]; then
        PROTOCOL=1
    fi
    if [ "$4" != "" ]; then
        local PSQL=$4
    fi
    if [ "$5" != "" ]; then
        local CONNECT=$5
    fi
    if [ "$6" != "" ]; then
        local LOG_FILE=$6
    fi

    if [ "$DBCOND" != "" ]; then
        case "$DBCOND" in
            savesysstat)
                DBCOND=" and a.$DBCOND"
                ;;
            savepgstmtstat)
                DBCOND=" and a.$DBCOND"
                ;;
            *)
            DBCOND=" and \($DBCOND\)"
        esac
    fi

    local SQL="
select string_agg(b.oid::varchar, ',') as oids
   from adm.dbmaintenance a
        inner join pg_catalog.pg_database b
            on b.datname = a.datname
 where a.actual $DBCOND"
  
    execsql "$ADMDBNAME" "Получение списка oid для обслуживаемых БД" $PROTOCOL "$SQL" "$PSQL" "$CONNECT" "$LOG_FILE"
    res=$?
    if [ $res -eq 0 ]; then
        echo $0: Для списка БД из $ADMDBNAME.adm.dbmaintenance по дополнительному условию $DBCOND получен список oid $RESCODE >>$LOG_FILE
    fi

    return $res
}

# getclocktimestamp - функция, выдающая текущее время на сервере БД (результат функции clock_timestamp()).
# Если при вызове функции не заданы позиционные параметры 4, 5 и 6 для вызова psql, подключения к  серверу и вывода в лог-файл,
# тогда используются глобальные переменные PSQL, CONNECT и LOG_FILE.
# Функция не использует никакие другие глобальные переменные и не изменяет значения указанных глобальных переменных,
# кроме RESCODE, применяемой по внутренних целях и для передачи результата.
# Параметры вывода:
# DBNAME=$1      - БД для подключения
# FORMAT=$2      - формат выдачи времени в виде строки с форматом для функции to_char() или один из синонимов:
#                - human_ms_utc  = DD.MM.YYYY HH24:MI:SS.MSOF
#                - human_utc     = DD.MM.YYYY HH24:MI:SSOF
#                - human_ms      = DD.MM.YYYY HH24:MI:SS.MS
#                - human         = DD.MM.YYYY HH24:MI:SS
#                - sorted_ms_utc = YYYY-MM-DD HH24:MI:SS.MSOF
#                - sorted_utc    = YYYY-MM-DD HH24:MI:SSOF
#                - sorted_ms     = YYYY-MM-DD HH24:MI:SS.MS
#                - sorted        = YYYY-MM-DD HH24:MI:SS
#                - по умолчанию human
# PROTOCOL=$3    - вести протокол в лог-файле (1) или нет (0)
# Коды возврата:
# 0              - проверки выполнились успешно
# #0             - код ошибки psql или прочие проверок
# RESCODE        - глобальная переменная, содержащая текстовое значение функции clock_timestamp(), отформатированное в заданном формате
getclocktimestamp()
{
    RESCODE=
    local DBNAME=$1
    local FORMAT=$2
    if [ "$FORMAT" == "" ]; then
        FORMAT=human
    fi
    if [ "$3" != "" ]; then
        local PROTOCOL=$3
    fi
    if [ "$PROTOCOL" == "" ]; then
        PROTOCOL=1
    fi
    if [ "$4" != "" ]; then
        local PSQL=$4
    fi
    if [ "$5" != "" ]; then
        local CONNECT=$5
    fi
    if [ "$6" != "" ]; then
        local LOG_FILE=$6
    fi

    case "$FORMAT" in
        human_ms_utc)
            FORMAT="DD.MM.YYYY HH24:MI:SS.MSOF"
            ;;
        human_utc)
            FORMAT="DD.MM.YYYY HH24:MI:SSOF"
            ;;
        human_ms)
            FORMAT="DD.MM.YYYY HH24:MI:SS.MS"
            ;;
        human)
            FORMAT="DD.MM.YYYY HH24:MI:SS"
            ;;
        sorted_ms_utc)
            FORMAT="YYYY-MM-DD HH24:MI:SS.MSOF"
            ;;
        sorted_utc)
            FORMAT="YYYY-MM-DD HH24:MI:SSOF"
            ;;
        sorted_ms)
            FORMAT="YYYY-MM-DD HH24:MI:SS.MS"
            ;;
        sorted)
            FORMAT="YYYY-MM-DD HH24:MI:SS"
            ;;
    esac
    
    local SQL="select to_char(clock_timestamp(),'$FORMAT') as clocktimestamp"
    execsql "$DBNAME" "Получение текущего времени с сервера БД $DBNAME" $PROTOCOL "$SQL" "$PSQL" "$CONNECT" "$LOG_FILE"
    res=$?
    return $res
}

# getuuid - функция, выдающая значение типа uuid (результат функции public.uuid_generate_v4()).
# Если при вызове функции не заданы позиционные параметры 3, 4 и 5 для вызова psql, подключения к  серверу и вывода в лог-файл,
# тогда используются глобальные переменные PSQL, CONNECT и LOG_FILE.
# Функция не использует никакие другие глобальные переменные и не изменяет значения указанных глобальных переменных,
# кроме RESCODE, применяемой по внутренних целях и для передачи результата.
# Параметры вывода:
# DBNAME=$1      - БД для подключения, в которой д.б. установлен пакет uuid-ossp, но его наличие не проверяется
# PROTOCOL=$2    - вести протокол в лог-файле (1) или нет (0)
# Коды возврата:
# 0              - проверки выполнились успешно
# #0             - код ошибки psql или прочие проверок
# RESCODE        - глобальная переменная, содержащая текстовое значение функции public.uuid_generate_v4()
getuuid()
{
    RESCODE=
    local DBNAME=$1
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

    local SQL="select public.uuid_generate_v4()::varchar as RESCODE"
    execsql "$DBNAME" "Получение значения типа uuid из БД $DBNAME" $PROTOCOL "$SQL" "$PSQL" "$CONNECT" "$LOG_FILE"
    res=$?
    return $res
}

# finish - функция для отметки в заданном лог-файле о завершении главного сценария.

# Если при вызове функции не заданы позиционные параметры 6, 7 и 8 для вызова psql, подключения к  серверу и вывода в лог-файл,
# тогда используются глобальные переменные PSQL, CONNECT и LOG_FILE.
# Функция не использует никакие другие глобальные переменные и не изменяет значения указанных глобальных переменных,
# кроме RESCODE, применяемой по внутренних целях.
# Параметры вывода:
# SCRIPTNAME=$1
# MSGTEXT=$2
# ADMDBNAME=$3          - имя административной БД, если имя БД задано как empty, протоколирование не выполняется
# OPERPRIMARYKEY=$4     - первичный ключ, по умолчанию берётся из глобальной переменной OPERPRIMARYKEY
#                       - если первичный ключ не задан или равен null/empty, протоколирование не выполняется
# PROTOCOL=$5           - вести протокол sql-команд в лог-файле (1) или нет (0)
finish()
{
    RESCODE=
    local SCRIPTNAME=$1
    local MSGTEXT=$2
    if [ "$MSGTEXT" == "empty" ]; then
        MSGTEXT=
    fi
    
    local ADMDBNAME=$3
    if [ "$ADMDBNAME" == "" ]; then
        ADMDBNAME=empty
    fi

    if [ "$4" != "" ]; then
        local OPERPRIMARYKEY=$4
    fi
    if [ "$5" != "" ]; then
        local PROTOCOL=$5
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
    
    operprotocol_end "$ADMDBNAME" "$MSGTEXT" "$OPERPRIMARYKEY" $PROTOCOL "$PSQL" "$CONNECT" "$LOG_FILE"
    
    local CURDATE=`/bin/date '+%d.%m.%Y %H:%M:%S.%N'`

    # В крайнем случае лог-файл м.б. не задан
    if [ "$LOG_FILE" != "" ]; then
        if [ "$MSGTEXT" != "" ]; then
            echo $MSGTEXT >>$LOG_FILE
        fi
        echo Завершение $SCRIPTNAME: $CURDATE. Время работы  $SECONDS сек>>$LOG_FILE
        return 0
    else
        if [ "$MSGTEXT" != "" ]; then
            echo $MSGTEXT
        fi
        echo Завершение $SCRIPTNAME: $CURDATE. Время работы  $SECONDS сек
    fi
    return 0
}
