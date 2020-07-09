# How to use this image
This image [flexberry/mono-nginx](https://hub.docker.com/r/flexberry/mono-nginx/) is based on customized [flexberry/mono](https://hub.docker.com/r/flexberry/mono/) image.
The image adds nginx and mono-fastcgi-server to the base image and is ready to run an ASP.NET application with the nginx and mono-fastcgi-server.

## Create a Dockerfile in your Mono web app project
```
FROM flexberry/mono-nginx:latest

COPY /app /app
WORKDIR /app
```