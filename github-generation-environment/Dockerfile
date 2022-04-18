FROM flexberry/mono-ember

# Добавляем сертификат letsencrypt. Для того, чтобы при генерации была возможность загрузить crp с хоста.
RUN cd /etc/ssl/certs && \
	openssl x509 -enddate -noout -in DST_Root_CA_X3.pem && \
	openssl x509 -enddate -noout -in ISRG_Root_X1.pem && \
	sed -i '/^mozilla\/DST_Root_CA_X3/s/^/!/' /etc/ca-certificates.conf \
		&& update-ca-certificates -f

RUN apt-get update && \
	apt-get install -y jq

# Предварительная установка node_modules.
COPY config/package.json /opt/package.json

RUN cd opt && \
	yarn
	