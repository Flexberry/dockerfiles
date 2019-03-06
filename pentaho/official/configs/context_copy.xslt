<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>
  <!-- Identity template, copies everything as is -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>
   <!-- Override for target elements -->
  <xsl:template match="Context/Resource[@name='jdbc/Hibernate']">
    <xsl:copy-of select="$file/Context/Resource[@name='jdbc/Hibernate']"/>
  </xsl:template>
  <xsl:template match="Context/Resource[@name='jdbc/Quartz']">
    <xsl:copy-of select="$file/Context/Resource[@name='jdbc/Quartz']"/>
  </xsl:template>
</xsl:stylesheet>
