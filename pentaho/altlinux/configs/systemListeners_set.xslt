<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:beans="http://www.springframework.org/schema/beans">
  <xsl:output method="xml" version="1.0"
              encoding="UTF-8" indent="yes"/>
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="//beans:bean[@id='versionCheckerSystemListener']"></xsl:template>

</xsl:stylesheet>
