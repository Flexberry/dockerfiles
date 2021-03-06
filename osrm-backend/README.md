# Описание
[Образ flexberry/osrm-backend](https://hub.docker.com/r/flexberry/osrm-backend) поддерживает функционал 
образа [osrm/osrm-backend](https://hub.docker.com/r/osrm/osrm-backend) с 
- автоматической загрузкой PBF-файла из [выгрузок](http://osm.sbin.ru/osm_dump/);
- генерацией osrm-файлов для постоения трасс типа `car`, `foot`, `bicycle`;
- запуском сервисов `osrm-routed` для указанных типов на портах `5000`, `5001`, `5002`.

## Быстрый старт

### Запуск контейнера  в режиме docker-compose

1. Создайте каталог для запуска образа в режиме `docker-compose` (например `osrm`) и перейдите в него
2. Скопируйте файл [docker-compose.yml](https://raw.githubusercontent.com/Flexberry/dockerfiles/master/osrm-backend/docker-compose.yml) в файл `docker-compose.yml` в созданном каталоге
или создайте файл самостоятельно на основе шаблона
```
version: "3.2"
services:
  osrm_backend:
    image: flexberry/osrm-backend
    ports: 
      - 5000:5000
      - 5001:5001
      #- 5002:5002
    environment:
      - Services=$Services
      - OSMREG=$OSMREG
    volumes:
      - osrmdata:/data  
      
volumes:
  osrmdata:
```

3. Скопируйте файл [.env](https://raw.githubusercontent.com/Flexberry/dockerfiles/master/osrm-backend/.env) в файл `docker-compose.yml` в созданном каталоге
или создайте файл самостоятельно на основе шаблона
```
#Services=car foot bicycle
Services=car foot
#OSMREG=RU-PER
OSMREG=RU-PER`
```

Значение переменных:
- `Services` - список запускаемых типов поддеживаемых маршрутов. 
   По умолчанию поддерживаются маршруты: `car` (`автомобильный`), `foor`(`пешком`), `bicycle`(`велосипед`).
- `OSMREG` - аббревиатура `PBF`-файла на сервере [выгрузок](http://osm.sbin.ru/osm_dump/) (по умолчанию `RU-PER`.

4. Запустить сервис:
```
docker-compose -p osrm up -d
```
5. Проконтролировать процесс запуска сервиса:
```
docker-compose logs -f osrm_backend
```
При запуске сервис 
- Проверяет наличие в каталоге  `/data` (каталог должен быть монтирован на именованный том или подкаталог HOST-системы)
подкаталога, указанного в переменной `OSMREG` (по умолчанию `RU-PER`). Если каталог отсутствует, он создается
(по умолчанию `/data/RU-PER`).
- Проверяет наличие в подкататоге (по умолчанию `/data/RU-PER`) PBF-файла `$OSMREG.osm.pbf` (по умолчанию `/data/RU-PER/RU-PER.osm.pbf`).
  Если файл отсутсвует, скачивает его с `http://osm.sbin.ru/osm_dump/$OSMREG.osm.pbf` (по умолчанию `http://osm.sbin.ru/osm_dump/RU-PER.osm.pbf`).
- Проверяет наличие в подкаталоге `/data/$OSMREG/` (по умолчанию `/data/RU-PER`) подкаталогов, указанных 
  в переменной `Services`. Если каталог отсутсвует:
    * создается подкаталог типа маршрута (по умолчанию `/data/RU-PER/car/`, `/data/RU-PER/foot/`, `/data/RU-PER/bicycle/`).
    * создаются файлы необходимые для поддержки [REST-API](https://github.com/Project-OSRM/osrm-backend/blob/master/docs/http.md) по данному типу маршутов.

- Для каждого типа маршрута, заданого в переменной `Services` (по умолчанию `car` (`автомобильный`), `foor`(`пешком`), `bicycle`(`велосипед`))
  запускается сервис `osrm-routed`, прослушивающий соединение на портах `5000`, `5001`,`5002` соответственно. 

При первоначальном запуске процесс скачивания PBF-файла и инициализации графов занимает десятки минут.
При повторных (при наличии сгенерированных файлов) - менее секунды.

## Тестирование функционала с помощью docker-образа osrm/osrm-frontend

1. Создайте отдельный каталог и скопируйте в него файл
[docker-compose-frontend.yml](https://raw.githubusercontent.com/Flexberry/dockerfiles/master/osrm-backend/docker-compose-frontend.yml)
  под именем `docker-compose.yml`.
  Если на backend'е запущены не все сервисы закомментиуете неиспользуемые docker frontend-сервисы 
  (`osrm_frontend_car`, `osrm_frontend_foot`,`osrm_frontend_bicycle`).
  Если `osrm-backend` запущен на другом сервере, замените `localhost` на IP-адрес или домен сервера, на котором функционирует `osrm-backend`.
  
2. Запустите серсисы командой
```
docker-compose up -d
```
Запуск каждого сервера может занимать определенное время (несколько минут).

3. Наберите в баузере URL's:
  - http://localhost:9966/ - построение автомобильных маршрутов;
  - http://localhost:9967/ - построение пеших маршрутов;
  - http://localhost:9968/ - построение велосипедных маршрутов.

  Если `frontend` запускается не на локальной машине, указите вместо `localhost` IP-адрес или домен сервера, на котором запущен frontend
  
4. Проверьте работу `frontend` и `backend`.
![Пример запроса трасс](./traces.png)


  
  

  



