# dockerfiles

В данном репозитории располагаются Dockerfile's сторонных сервисов (apache-mono, postgres, oracle, pentaho, ...)
собираемых под docker-доменом [flexberry/](https://hub.docker.com/u/flexberry).

Рекомендации по сборке и тегированию docker-образов для механизма автосборки для этого домена описаны в документе:
[Инструкция по настройке автосборки образов в dockerhub Flexberry репозитории ](https://github.com/Flexberry/dockerfiles/blob/master/AUTOBUILD.md)

Кроме этого в рамках docker-домена [flexberry/](https://hub.docker.com/u/flexberry) собираются собственные сервисы,
исходные коды которых и Dockerfile для сборки образов расположены в отдельных github-репозиториях:
- github-репозиторий: [Flexberry Service Bus](https://github.com/Flexberry/NewPlatform.Flexberry.ServiceBus)
  - образ [flexberry/flexberry-service-bus](https://github.com/Flexberry/NewPlatform.Flexberry.ServiceBus/tree/develop/Docker/flexberry-service-bus)
  - образ [flexberry/flexberry-service-bus-postgres-db](https://github.com/Flexberry/NewPlatform.Flexberry.ServiceBus/tree/develop/Docker/flexberry-service-bus-postgres-db)
- образ [flexberry/servicebuseditor](https://github.com/Flexberry/NewPlatform.Flexberry.ServiceBus.Editor/tree/develop/Docker)
  
