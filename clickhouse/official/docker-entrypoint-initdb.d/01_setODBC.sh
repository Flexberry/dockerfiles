#!/bin/sh

if [ -z "$PGPort" ]
then
  PGPort=5432
fi  

if [ -z "$PGProtocol" ]
then
  PGProtocol=9.6
fi

echo "[DEFAULT]
Driver = postgresConnection

[postgresConnection]
Description         = PostgreSQL connection
Driver              = /usr/lib/x86_64-linux-gnu/odbc/psqlodbcw.so
Database            = ${PGDatabase} 
Servername          = ${PGServername}
UserName            = ${PGUserName}
Password            = ${PGPassword}
Port                = ${PGPort}
Protocol            = ${PGProtocol}
ReadOnly            = No
RowVersioning       = No
ShowSystemTables    = No
ConnSettings        =
" >/etc/ODBC.ini
