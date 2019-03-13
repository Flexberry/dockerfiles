# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [8.2-6] - 2019-03-12
### Added
- To correctly support the export of Cyrillic reports to PDF, Microsoft TrueType fonts are installed.
- Removed files for creating default data in the launched Pentaho Server.
- Removed the task of periodically checking for updates.
- Removed the drop-down menu with test users from the login screen in Pentaho Server.
- The ability to set the names of passwords and the role of new users with the removal of standard users joe, pat, suzy, tiffany;
- The ability to change the admin user password is provided.
- Added database drivers: postgresql-42.2.5, clickhouse-0.1.50 (driver compilation is performed when creating an image);
- The image allows you to use the built-in HSQLDB database as an administrative database, as well as using an external relational database (currently postgresql) in production.

### Changed

### Removed



