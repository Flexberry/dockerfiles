<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" version="1.0"
              doctype-public='-//Hibernate/Hibernate Configuration DTD//EN'
              doctype-system='http://www.hibernate.org/dtd/hibernate-configuration-3.0.dtd'
              encoding="UTF-8" indent="yes"/>
  <!-- Identity template, copies everything as is -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>
   <!-- Override for target elements -->
  <xsl:template match="hibernate-configuration/session-factory/property[@name='connection.url']/text()"
    ><xsl:value-of select='$HIBERNATE_URL'/>
  </xsl:template>
  <xsl:template match="hibernate-configuration/session-factory/property[@name='connection.username']/text()"
    ><xsl:value-of select='$HIBERNATE_USER'/>
  </xsl:template>
  <xsl:template match="hibernate-configuration/session-factory/property[@name='connection.password']/text()"
    ><xsl:value-of select='$HIBERNATE_PASS'/>
  </xsl:template>
</xsl:stylesheet>
