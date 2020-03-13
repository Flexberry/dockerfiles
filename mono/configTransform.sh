#!/bin/bash
set -x
exec 2>&1
for XMLTEMPLATE in $XMLTEMPLATES
do
  vars=`xsltproc - $XMLTEMPLATE  <<EOF | sort -u
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:beans="http://www.springframework.org/schema/beans" xmlns:util="http://www.springframework.org/schema/util">
  <xsl:output method="text" encoding="UTF-8"/>
  <xsl:template match="node()">
    <xsl:for-each select="//*/@*[starts-with(.,'%%') and substring(.,string-length(.)-1)='%%']">
      <xsl:value-of select="substring(substring(.,1,string-length(.)-2),3)" />
      <xsl:text>&#010;</xsl:text>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
EOF
`
#   echo "List of variables: $vars" >&2

  if [ -z "$vars" ]
  then
    continue;
  fi

  for var in $vars
  do
    value=${!var}
    if [ -z "$value" ]
    then
      echo "Variable \"$var\" doesn't exist in environment" >&2
      exit 1
    fi
  done

  TMPFILE=/tmp/config$$.xml
  {
  cat <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:beans="http://www.springframework.org/schema/beans" xmlns:util="http://www.springframework.org/schema/util">
  <xsl:output method="xml" version="1.0"
              encoding="UTF-8" indent="yes"/>
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>
EOF
  for var in $vars
  do
    value=${!var}
    cat <<EOF
  <xsl:template match="//*/@*[.='%%$var%%']">
    <xsl:attribute name="{name()}">$value</xsl:attribute>
  </xsl:template>
EOF
  done
  cat <<EOF
</xsl:stylesheet>
EOF
  } |
  xsltproc - $XMLTEMPLATE >$TMPFILE
  if [ $? -eq 0 ]
  then
    mv $TMPFILE $XMLTEMPLATE
  else
    rm $TMPFILE
    exit 1
  fi
done
exit 0
