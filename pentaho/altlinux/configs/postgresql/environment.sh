export PGPASSWORD=$DB_ADMIN_PASS

JDBCType=postgresql
Driver=org.postgresql.Driver
Dialect=PostgreSQLDialect
CREATE_JCR_DB=postgresql/create_jcr_postgresql.sql
CREATE_REPOSITORY_DB=postgresql/create_repository_postgresql.sql
CREATE_QUARTZ_DB=postgresql/create_quartz_postgresql.sql
CREATE_QUARTZ_SQL='CREATE TABLE "QRTZ" ( NAME VARCHAR(200) NOT NULL, PRIMARY KEY (NAME) );'
