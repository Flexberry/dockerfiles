ARG MONO_VERSION=6.6

FROM flexberry/mono:${MONO_VERSION}

LABEL MAINTAINER=mail@flexberry.ru

RUN apt-get update && \
    apt-get install -y gnupg2 lsb-release && \
    echo "deb http://nginx.org/packages/mainline/debian `lsb_release -cs` nginx" | tee /etc/apt/sources.list.d/nginx.list && \
    curl -fsSL https://nginx.org/keys/nginx_signing.key | apt-key add - && \
    apt-get update && \
    apt-get install -y mono-fastcgi-server4 nginx procps && \ 
    apt-get remove -y gnupg2 lsb-release && \
    apt-get autoremove -y && apt-get clean && \
    rm -rf /var/lib/apt/lists/* /tmp/*
  
EXPOSE 80

COPY nginx.conf /etc/nginx/nginx.conf
COPY start_nginx_mono.sh /bin/start_nginx_mono.sh

# Ensure XMLTEMPLATES environment variable set for best experience.
CMD /bin/configTransform.sh \
  && /bin/start_nginx_mono.sh
