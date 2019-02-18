# Description
Based on [openjdk:8u171](https://hub.docker.com/_/openjdk).

Flexberry/pentaho-8.1 image contains [Pentaho Server 8.1](https://sourceforge.net/projects/pentaho/files/Pentaho%208.1/server/), working with HSQLDB with some modifications:
- [Microsoft TrueType fonts] installed(https://packages.debian.org/ru/sid/ttf-mscorefonts-installer)
- Sample data creation files removed
- Updates checking task removed
- Sample users dropdown removed from Pentaho Server login screen 
- *TODO:*
  - Change default passwords for sample users on start (commented corresponding config files copying in Dockerfile). Should implement encryption of provided passwords on docker build by the same algorithm as sample passwords were encrypted.

Pentaho Server is exposed on 8080 port.