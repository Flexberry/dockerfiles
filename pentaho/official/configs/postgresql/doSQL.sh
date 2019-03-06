#!/bin/sh
# Execure SQL-reauests from file
export PGPASSWORD=$DB_ADMIN_PASS
echo -ne "Wait connection to database  $DB_ADMIN $DB_HOST:$DB_PORT..."
until psql -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT -f $1; do sleep 1; done
echo
