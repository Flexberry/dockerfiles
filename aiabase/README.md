# How to use this image
This image [flexberry/aiabase](https://hub.docker.com/r/flexberry/aiabase/) is based on customized [flexberry/mono-xsp](https://hub.docker.com/r/flexberry/mono-xsp/) image.
The image adds python, SpaCy, SpaCy russian and english dictionaries to the base image and is ready to run AI Assisant utilities.

## Create a Dockerfile in your project
```
FROM flexberry/aiabase:latest

COPY /app /app
WORKDIR /app
```

## How to use booting up
Ensure BOOTUP_CHECK_URL environment variable is set for best experience. `start_xsp.sh` script makes possible to boot up web server instantly in the background.
```
FROM flexberry/aiabase:latest

ENV BOOTUP_CHECK_URL="http://0.0.0.0/"

COPY test /app
WORKDIR /app
```