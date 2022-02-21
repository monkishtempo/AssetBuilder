<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
    xmlns:w="http://schemas.microsoft.com/office/word/2003/wordml"
>
  <xsl:output method="xml" indent="yes"/>
  <xsl:param name="excludedAlgoList" />
  <xsl:variable name="documents">
    <WordML Orientation="Portrait" />
  </xsl:variable>
  <xsl:variable name="cr" select="'&#10;'" />

  <xsl:template match="/*">
    <xsl:element name="Report">
      <xsl:variable name="doc" select="/" />
      <xsl:for-each select="msxsl:node-set($documents)/WordML">
        <xsl:element name="WordML">
          <xsl:for-each select="@*">
            <xsl:attribute name="{name()}">
              <xsl:value-of select="."/>
            </xsl:attribute>
          </xsl:for-each>
          <xsl:for-each select="$doc">
            <xsl:call-template name="Paragraph">
              <xsl:with-param name="Text">TextAsset Report</xsl:with-param>
              <xsl:with-param name="Align">center</xsl:with-param>
              <xsl:with-param name="Bold">1</xsl:with-param>
              <xsl:with-param name="Underline">single</xsl:with-param>
              <xsl:with-param name="FontSize">14</xsl:with-param>
            </xsl:call-template>
            <xsl:call-template name="Paragraph">
              <xsl:with-param name="Text">
                <xsl:call-template name="algo-names"/>
              </xsl:with-param>
              <xsl:with-param name="Align">center</xsl:with-param>
              <xsl:with-param name="Bold">1</xsl:with-param>
              <xsl:with-param name="Underline">single</xsl:with-param>
              <xsl:with-param name="FontSize">14</xsl:with-param>
            </xsl:call-template>
            <xsl:if test="$excludedAlgoList">
              <xsl:call-template name="Paragraph">
                <xsl:with-param name="Text">
                  <xsl:text> Excluding </xsl:text>
                  <xsl:value-of select="$excludedAlgoList"/>
                </xsl:with-param>
                <xsl:with-param name="Align">center</xsl:with-param>
                <xsl:with-param name="FontSize">10</xsl:with-param>
              </xsl:call-template>
            </xsl:if>
            <xsl:for-each select="/NewDataSet/Table1">
              <xsl:call-template name="Paragraph">
                <xsl:with-param name="Text">
                  <xsl:text>AssetType: </xsl:text>
                  <xsl:value-of select="AssetType"/>
                  <xsl:text>&#9;TextAssetID: </xsl:text>
                  <xsl:value-of select="TextAsset"/>
                </xsl:with-param>
                <xsl:with-param name="FontSize">10</xsl:with-param>
                <xsl:with-param name="Bold">1</xsl:with-param>
              </xsl:call-template>
              <xsl:call-template name="Paragraph">
                <xsl:with-param name="Text">
                  <xsl:text>AssetIDs: </xsl:text>
                  <xsl:value-of select="AssetIDs"/>
                </xsl:with-param>
                <xsl:with-param name="FontSize">10</xsl:with-param>
                <xsl:with-param name="Bold">1</xsl:with-param>
              </xsl:call-template>
              <xsl:call-template name="Paragraph">
                <xsl:with-param name="Text">
                  <xsl:value-of select="Content"/>
                </xsl:with-param>
                <xsl:with-param name="FontSize">10</xsl:with-param>
              </xsl:call-template>
            </xsl:for-each>
          </xsl:for-each>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template name="Range">
    <xsl:param name="Text"/>
    <xsl:param name="Bold"/>
    <xsl:param name="Italic"/>
    <xsl:param name="Underline"/>
    <xsl:param name="FontSize"/>
    <xsl:element name="w:r">
      <xsl:element name="w:rPr">
        <xsl:if test="$Bold > 0">
          <xsl:element name="w:b"/>
        </xsl:if>
        <xsl:if test="$Italic > 0">
          <xsl:element name="w:i"/>
        </xsl:if>
        <xsl:if test="$Underline != ''">
          <xsl:element name="w:u">
            <xsl:attribute name="w:val">
              <xsl:value-of select="$Underline"/>
            </xsl:attribute>
          </xsl:element>
        </xsl:if>
        <xsl:if test="$FontSize > 0">
          <xsl:element name="w:sz">
            <xsl:attribute name="w:val">
              <xsl:value-of select="$FontSize*2"/>
            </xsl:attribute>
          </xsl:element>
        </xsl:if>
      </xsl:element>
      <xsl:element name="w:t">
        <xsl:value-of select="$Text"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="Paragraph">
    <xsl:param name="Text"/>
    <xsl:param name="Bold"/>
    <xsl:param name="Italic"/>
    <xsl:param name="Underline"/>
    <xsl:param name="FontSize"/>
    <xsl:param name="Align"/>
    <xsl:param name="Border"/>
    <xsl:param name="Indent"/>
    <xsl:element name="w:p">
      <xsl:element name="w:pPr">
        <xsl:if test="$Align">
          <xsl:element name="w:jc">
            <xsl:attribute name="w:val">
              <xsl:value-of select="$Align"/>
            </xsl:attribute>
          </xsl:element>
        </xsl:if>
      </xsl:element>
      <xsl:call-template name="Range">
        <xsl:with-param name="Text">
          <xsl:value-of select="$Text"/>
        </xsl:with-param>
        <xsl:with-param name="Bold">
          <xsl:value-of select="$Bold"/>
        </xsl:with-param>
        <xsl:with-param name="Italic">
          <xsl:value-of select="$Italic"/>
        </xsl:with-param>
        <xsl:with-param name="Underline">
          <xsl:value-of select="$Underline"/>
        </xsl:with-param>
        <xsl:with-param name="FontSize">
          <xsl:value-of select="$FontSize"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:element>
  </xsl:template>

  <xsl:template name="algo-names">
    <xsl:for-each select="/NewDataSet/Table">
      <xsl:if test="position()>1 and position() &lt; last()">
        <xsl:text>, </xsl:text>
      </xsl:if>
      <xsl:if test="position()>1 and position()=last()">
        <xsl:text> and </xsl:text>
      </xsl:if>
      <xsl:value-of select="Algo_Name"/>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
