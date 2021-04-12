# Описание
[Образ flexberry/mssql-fts](https://hub.docker.com/r/flexberry/mssql-fts/) поддерживает функционал 
MSSQL-сервера с модулем полнотекстового поиска - `fts` (`Full Text Search`)

## Запуск контейнера  в режиме docker-compose

Пример `docker-compose-yml` файла:
```
version: "3.2"
services:
  mssql-fts:
    image: flexberry/mssql-fts
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=<...>  
    ports:
     - 1443:1443
    volumes:
     - db:/var/opt/mssql/data/

volumes:
  db:
```

Где:
- `<...>` - пароль администратора длиной не менее 8-ми символов.
