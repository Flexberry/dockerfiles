<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" version="1.0"
    doctype-public='-//The Apache Software Foundation//DTD Jackrabbit 2.0//EN'
    doctype-system='http://jackrabbit.apache.org/dtd/repository-2.0.dtd'
    encoding="UTF-8" indent="yes"/>
  <xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>
  <!-- Identity template, copies everything as is -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <!-- Set attributes for target elements -->
  <xsl:template match="/Repository/FileSystem/param[@name='password']/@value">
    <xsl:attribute name="{name()}"><xsl:value-of select='$JCR_PASS'/></xsl:attribute>
  </xsl:template>
  <xsl:template match="/Repository/FileSystem/param[@name='url']/@value">
   <xsl:attribute name="{name()}"><xsl:value-of select='$JCR_URL'/></xsl:attribute>
  </xsl:template>

  <xsl:template match="/Repository/DataStore/param[@name='password']/@value">
    <xsl:attribute name="{name()}"><xsl:value-of select='$JCR_PASS'/></xsl:attribute>
  </xsl:template>
  <xsl:template match="/Repository/DataStore/param[@name='url']/@value">
    <xsl:attribute name="{name()}"><xsl:value-of select='$JCR_URL'/></xsl:attribute>
  </xsl:template>

</xsl:stylesheet>
