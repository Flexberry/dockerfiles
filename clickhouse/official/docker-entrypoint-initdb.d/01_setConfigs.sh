#!/bin/sh

# Example:
# CLICKHOUSECONFIG="http_port=18123,default_session_timeout=120"
# CLICKHOUSEUSERCONFIG="max_memory_usage=32000000000,duration=7200"

xsltHead() {
  cat <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:beans="http://www.springframework.org/schema/beans" xmlns:util="http://www.springframework.org/schema/util">
  <xsl:output method="xml" indent="yes" encoding="UTF-8"/>
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>
EOF
}


xsltTail() {
  cat <<EOF
</xsl:stylesheet>
EOF
}

clikchouseConfigXSLT() {
  ifs="$IFS"
  IFS=,
  set -- $*
  IFS="$ifs"
  for assign
  do
    IFS="="
    set -- $assign
    xpath=$1
    shift
    value="$*"
    cat <<EOF
  <xsl:template match="//$xpath">
    <xsl:copy>
        <xsl:text>$value</xsl:text>
    </xsl:copy>
  </xsl:template>
EOF
  done
}


#MAIN
ConfigXSLTFILE=/tmp/ConfigXSLTFILE.xslt
(
xsltHead;
clikchouseConfigXSLT ${CLICKHOUSECONFIG}
xsltTail;
) > $ConfigXSLTFILE

xsltproc $ConfigXSLTFILE /etc/clickhouse-server/config.xml >/tmp/config.xml

if xmllint --noout /etc/clickhouse-server/config.xml
then
  mv /tmp/config.xml /etc/clickhouse-server/config.xml
fi

UserConfigXSLTFILE=/tmp/UserConfigXSLTFILE.xslt
(
xsltHead;
clikchouseConfigXSLT ${CLICKHOUSEUSERCONFIG}
xsltTail;
) > $UserConfigXSLTFILE

xsltproc $UserConfigXSLTFILE /etc/clickhouse-server/users.xml > /tmp/users.xml

if xmllint --noout /etc/clickhouse-server/users.xml
then
  mv /tmp/users.xml /etc/clickhouse-server/users.xml
fi
