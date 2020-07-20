# Описание
[Образ flexberry/osrm-backend](https://hub.docker.com/r/flexberry/osrm-backend) поддерживает функционал 
образа [osrm/osrm-backend](https://hub.docker.com/r/osrm/osrm-backend) с 
- автоматической загрузкой PBF-файла из [выгрузок](http://osm.sbin.ru/osm_dump/);
- генерацией osrm-файлов для постоения трасс типа `car`, `foot`, `bicycle`;
- запуском сервисов `osrm-routed` для указанных типов на портах `5000`, `5001`, `5002`.

## Быстрый старт

### Запуск контейнера  в режиме docker-compose


