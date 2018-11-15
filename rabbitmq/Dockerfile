FROM debian AS builder

RUN \
  apt-get update; \
  apt-get install -y wget unzip; \
  wget "https://dl.bintray.com/rabbitmq/community-plugins/3.7.x/rabbitmq_message_timestamp/rabbitmq_message_timestamp-20170830-3.7.x.zip"; \
  unzip rabbitmq_message_timestamp-20170830-3.7.x.zip;


FROM rabbitmq:3.7.8-management

COPY --from=builder rabbitmq_message_timestamp-20170830-3.7.x.ez /usr/lib/rabbitmq/lib/rabbitmq_server-3.7.8/plugins/rabbitmq_message_timestamp.ez

RUN rabbitmq-plugins enable rabbitmq_message_timestamp
