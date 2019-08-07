#!/bin/bash
# Execure SQL-requests from $1 file
set -x
echo "doSQL: File $1:";
cat $1 >&1
export PGPASSWORD=$DB_ADMIN_PASS
echo -ne "Wait connection to database  $DB_ADMIN $DB_HOST:$DB_PORT..."
until psql -U $DB_ADMIN_USER -h $DB_HOST -p $DB_PORT -d postgres -f $1; do sleep 1; done
echo
