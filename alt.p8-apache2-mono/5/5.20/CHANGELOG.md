# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


### Changed

- Correct support BOOTUP_CHECK_URL. Testing URL process called in background before startup httpd 
  See https://github.com/Flexberry/dockerfiles/blob/master/alt.p8-apache2/CHANGELOG.md#2438-123-2019-04-23

## [8.2-6] - 2019-04-10
### Added

- Environment variable ENV MONO_MANAGED_WATCHER=dummy
