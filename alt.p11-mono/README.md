# How to use this image
This image [flexberry/alt.p11-mono](https://hub.docker.com/r/flexberry/alt.p11-mono/) is based on customized [flexberry/alt.p11](https://hub.docker.com/r/flexberry/alt.p11/) image.
The image adds Mono to the base image and can run stand-alone Mono console apps with an additional feature to make possible to transform application configuration using environment variables.

## Create a Dockerfile in your Mono app project
```
FROM flexberry/alt.p11-mono:latest

COPY /app /app
WORKDIR /app

RUN mono MyConsoleApp.exe
```

## How to use config transform
Ensure XMLTEMPLATES environment variable is set for best experience. `configTransform.sh` script searches for a substrings of declared pattern `%%VariableName%%` and replaces them with environment variable values.

```
FROM flexberry/alt.p11-mono:latest

ENV XMLTEMPLATES="/app/MyConsoleApp.exe.config"

COPY test /app
WORKDIR /app

RUN mono MyConsoleApp.exe
```
