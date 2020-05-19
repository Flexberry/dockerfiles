# How to use this image
This image [flexberry/mono-service](https://hub.docker.com/r/flexberry/mono-service/) is based on customized [flexberry/mono](https://hub.docker.com/r/flexberry/mono/) image.
The image adds mono-service to the base image and is ready to run an mono applications as service.

## Create a Dockerfile in your Mono service project
```
FROM flexberry/mono-service:latest

COPY /app /app
WORKDIR /app

CMD [ "mono-service",  "./MyApp.exe", "--no-daemon" ]
```