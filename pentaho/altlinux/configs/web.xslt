<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <!-- Identity template, copies everything as is -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>
   <!-- Remove for target elements -->
  <xsl:template match="web-app/context-param[param-name/text()='hsqldb-databases']">
  </xsl:template>
  <xsl:template match="web-app/listener[listener-class/text()='org.pentaho.platform.web.http.context.HsqldbStartupListener']">
  </xsl:template>
</xsl:stylesheet>
