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
  <xsl:template match="Repository/FileSystem">
    <xsl:copy-of select="$file/Repository/FileSystem"/>
  </xsl:template>
  <xsl:template match="Repository/DataStore">
    <xsl:copy-of select="$file/Repository/DataStore"/>
  </xsl:template>
  <xsl:template match="Repository/PersistenceManager">
    <xsl:copy-of select="$file/Repository/PersistenceManager"/>
  </xsl:template>
</xsl:stylesheet>
