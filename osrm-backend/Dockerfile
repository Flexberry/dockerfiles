FROM osrm/osrm-backend

RUN \
  apt-get update && apt-get install -y wget vim net-tools psmisc tcpdump less

COPY /entrypoint.sh /

ENTRYPOINT /entrypoint.sh

EXPOSE 5000 5001 5002

