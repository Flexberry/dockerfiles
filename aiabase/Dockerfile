FROM flexberry/mono-xsp:latest

# Xvfb is installed for Winforms.
RUN mkdir -p /usr/share/man/man1 && \
    apt-get update && \
    DEBIAN_FRONTEND=noninteractive apt-get install -y xorg xvfb \
        wget python3 python3-pip && \
    pip3 install -U pip setuptools wheel && \
    pip3 install -U spacy==3.2.3 && \
	pip uninstall click -y && pip install click==8.0.4 && \
    python3 -m spacy download ru_core_news_lg && \
	python3 -m spacy download en_core_web_trf

