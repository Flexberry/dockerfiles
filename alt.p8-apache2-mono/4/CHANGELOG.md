# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [4.6.2.7-1.3.6] 2019-04-23

### Changed

- Correct support BOOTUP_CHECK_URL. Testing URL process called in background before startup httpd 


## [4.6.2.7-1.3.3] 2019-04-05

### Added

- support of BOOTUP_CHECK_URL. Contain a local URL of the form http:/0.0.0.0:/. If this variable is present after the start of the WEB server, the startup script waits for the service to be available at this URL.

 ### Changed

- Uniquing list variables.
- Debug is redirected to stdout

## [4.6.2.7-1.3.0] 2019-04-03

### Added

- A mechanism for configuring XML files has been added, which allows setting in the specified files the values of the arguments , containing the pattern %%ENVIRONMENT_VARIABLE%%

## [4.6.2.7-1.2.0] 2019-04-01

### Added

- `docker-compose.yml` with to variables:
  * `SERVER_HTTP_PORT` - external http port;
  * `MODULES` - list of loadable modules (by default rewrite ssl deflate filter)
  
 - `.env` file
 
 ### Changed
 
 - move to base image flexberry/alt.p8-apache2:2.4.38-1.2
