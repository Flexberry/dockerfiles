FROM flexberry/mono-xsp:latest

RUN mkdir -p /usr/share/man/man1 && \
    apt-get update && \
    # Xvfb is installed for Winforms.
    DEBIAN_FRONTEND=noninteractive apt-get install -y xorg xvfb \
        wget python3 python3-pip && \
    # Tools for spaCy.
    pip3 install -U pip setuptools wheel && \
    # SpaCy.
    pip3 install -U spacy==3.2.3 && \
    # Hack to remove damaged version of click (dependency of spaCy).
	pip uninstall click -y && pip install click==8.0.4 && \
    # Russian dictionary for spaCy.
    python3 -m spacy download ru_core_news_lg && \
    # English dictionary for spaCy.
	python3 -m spacy download en_core_web_trf

