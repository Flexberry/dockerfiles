<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" version="1.0"
              encoding="UTF-8" indent="yes"/>
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/pentaho-system/login-show-users-list/text()">false</xsl:template>

  <xsl:template match="/pentaho-system/login-show-sample-users-hint/text()">false</xsl:template>

</xsl:stylesheet>
