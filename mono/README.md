# How to use this image
This image [flexberry/mono](https://hub.docker.com/r/flexberry/mono/) is based on official [mono](https://hub.docker.com/_/mono/) image and will run stand-alone Mono console apps with an additional feature to make possible to transform application configuration using environment variables.

## Create a Dockerfile in your Mono app project
```
FROM flexberry/mono:latest

COPY /app /app
WORKDIR /app

RUN mono MyConsoleApp.exe
```

## How to use config transform
Ensure XMLTEMPLATES environment variable is set for best experience. `configTransform.sh` script searches for a substrings of declared pattern `%%VariableName%%` and replaces them with environmment variable values.
```
FROM flexberry/mono:latest

ENV XMLTEMPLATES="/app/MyConsoleApp.exe.config"

COPY test /app
WORKDIR /app

RUN mono MyConsoleApp.exe
```