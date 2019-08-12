<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" version="1.0"
              encoding="UTF-8" indent="yes"/>
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="//@username[parent::node()[@name='jdbc/Hibernate']]">
    <xsl:attribute name="{name()}"><xsl:value-of select='$HIBERNATE_USER'/></xsl:attribute>
  </xsl:template>
  <xsl:template match="//@username[parent::node()[@name='jdbc/Quartz']]">
    <xsl:attribute name="{name()}"><xsl:value-of select='$QUARTZ_USER'/></xsl:attribute>
  </xsl:template>

  <xsl:template match="//@password[parent::node()[@name='jdbc/Hibernate']]">
    <xsl:attribute name="{name()}"><xsl:value-of select='$HIBERNATE_PASS'/></xsl:attribute>
  </xsl:template>
  <xsl:template match="//@password[parent::node()[@name='jdbc/Quartz']]">
    <xsl:attribute name="{name()}"><xsl:value-of select='$QUARTZ_PASS'/></xsl:attribute>
  </xsl:template>

  <xsl:template match="//@url[parent::node()[@name='jdbc/Hibernate']]">
    <xsl:attribute name="{name()}"><xsl:value-of select='$HIBERNATE_URL'/></xsl:attribute>
  </xsl:template>
  <xsl:template match="//@url[parent::node()[@name='jdbc/Quartz']]">
    <xsl:attribute name="{name()}"><xsl:value-of select='$QUARTZ_URL'/></xsl:attribute>
  </xsl:template>

</xsl:stylesheet>
