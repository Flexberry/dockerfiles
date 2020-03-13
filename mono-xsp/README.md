# How to use this image
This image [flexberry/mono-xsp](https://hub.docker.com/r/flexberry/mono-xsp/) is based on customized [flexberry/mono](https://hub.docker.com/r/flexberry/mono-xsp/) image.
The image adds XSP to the base image and is ready to run an ASP.NET application with the XSP web server.

## Create a Dockerfile in your Mono web app project
```
FROM flexberry/mono-xsp:latest

COPY /app /app
WORKDIR /app
```

## How to use booting up
Ensure BOOTUP_CHECK_URL environment variable is set for best experience. `start_xsp.sh` script makes possible to boot up web server instantly in the background.
```
FROM flexberry/mono-xsp:latest

ENV BOOTUP_CHECK_URL="http://0.0.0.0/"

COPY test /app
WORKDIR /app
```