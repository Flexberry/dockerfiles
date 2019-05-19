#!/bin/sh

#bash
gosu clickhouse:clickhouse /usr/bin/clickhouse-server --config-file=/etc/clickhouse-server/config.xml
