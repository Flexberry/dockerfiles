# Описание
Данный образ поддерживает выполнение консольных приложений на Mono и основан на кастомизированном образе [flexberry/alt.p10](https://hub.docker.com/r/flexberry/alt.p10/).

## Создание Dockerfile в проекте Mono-приложения
```
FROM flexberry/alt.p10-mono:latest

COPY /app /app
WORKDIR /app

RUN mono MyConsoleApp.exe
```

## Трансформация конфигов через переменные окружения
Для лучшего опыта работы установите переменную окружения `XMLTEMPLATES`. Скрипт `configTransform.sh` ищет подстроки по шаблону `%%VariableName%%` и заменяет их значениями переменных окружения.

```
FROM flexberry/alt.p10-mono:latest

ENV XMLTEMPLATES="/app/MyConsoleApp.exe.config"

COPY test /app
WORKDIR /app

RUN mono MyConsoleApp.exe
```
