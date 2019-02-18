# Описание 
Основан на образе Flexberry/pentaho-8.1

Образ Flexberry/pentaho-8.1-postgres содержит [Pentaho Server 8.1](https://sourceforge.net/projects/pentaho/files/Pentaho%208.1/server/), работающего с базой PostgreSQL. Модификации настроек Pentaho Server описаны в readme базового образа.

Pentaho server доступен на порту 8080.

## Запуск
Для запуска необходимо указать настройки подключения к PostgreSQL
- DB_HOST - адрес сервера с PostgreSQL
- DB_PORT - порт
- DB_ADMIN_USER - имя пользователя с правами администратора
- DB_ADMIN_PASS - пароль пользователя с правами администратора
- JCR_PASS - пароль для пользователя для доступа к БД Jackrabbit-репозитория
- HIBERNATE_PASS - пароль для пользователя для доступа к БД Hibernate
- QUARTZ_PASS - пароль для пользователя для доступа к БД Quartz

При запуске контейнера автоматически создаются БД jackrabbit, hibernate, quartz.