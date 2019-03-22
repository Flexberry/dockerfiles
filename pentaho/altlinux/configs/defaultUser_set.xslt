<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:beans="http://www.springframework.org/schema/beans" xmlns:util="http://www.springframework.org/schema/util">
  <xsl:output method="xml" version="1.0"
              encoding="UTF-8" indent="yes"/>
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="//util:map[@id='defaultUserRoleMappings']/beans:entry[@key]"></xsl:template>

</xsl:stylesheet>
