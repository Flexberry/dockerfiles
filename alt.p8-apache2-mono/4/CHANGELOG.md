# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [4.6.2.7-1.2.0] 2019-04-01

### Added

- `docker-compose.yml` with to variables:
  * `SERVER_HTTP_PORT` - external http port;
  * `MODULES` - list of loadable modules (by default rewrite ssl deflate filter)
  
 - `.env` file
 
 ### Changed
 
 - move to base image flexberry/alt.p8-apache2:2.4.38-1.2
