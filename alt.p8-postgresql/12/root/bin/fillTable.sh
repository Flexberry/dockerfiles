#!/bin/sh
echo "create database test1;
\c test1
CREATE TABLE indexing_table(created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW());
" | psql -U postgres

while true; do
  psql -U postgres -d test1 -c "INSERT INTO indexing_table(created_at) VALUES (CURRENT_TIMESTAMP);"
  sleep 1;
done
