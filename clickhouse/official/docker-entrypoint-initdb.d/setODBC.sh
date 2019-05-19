#!/bin/sh

echo "[DEFAULT]
Driver = postgresConnection

[postgresConnection]
Description         = PostgreSQL connection
Driver              = /usr/lib/x86_64-linux-gnu/odbc/psqlodbcw.so
Database            = $PGDatabase 
Servername          = $PGServername
UserName            = $PGUserName
Password            = $PGPassword
Port                = $PGPort
Protocol            = 9.6
ReadOnly            = No
RowVersioning       = No
ShowSystemTables    = No
ConnSettings        =
"
