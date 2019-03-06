# Description
Based on [openjdk:8-slim](https://hub.docker.com/_/openjdk).

flexberry/pentaho:8.1 image contains [Pentaho Server 8.1](https://sourceforge.net/projects/pentaho/files/Pentaho%208.1/server/), working with HSQLDB with some modifications:
- [Microsoft TrueType fonts](https://packages.debian.org/ru/sid/ttf-mscorefonts-installer) installed
- Sample data creation files removed
- Updates checking task removed
- Sample users dropdown removed from Pentaho Server login screen 
- *TODO:*
  - Change default passwords for sample users on start (commented corresponding config files copying in Dockerfile). Should implement encryption of provided passwords on docker build by the same algorithm as sample passwords were encrypted.

Pentaho Server is exposed on 8080 port.

# Example
```
docker run \
    --name pentaho \
    -p 8080:8080 \
    -d flexberry/pentaho:8.1
```

```
docker service create \
    --name pentaho \
    -p 8080:8080 \
    flexberry/pentaho:8.1
```