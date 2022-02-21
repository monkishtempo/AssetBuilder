<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:efn="e24:Functions"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl efn"
				xmlns:w="http://schemas.microsoft.com/office/word/2003/wordml"
>
  <xsl:output method="xml" indent="yes"/>
  <xsl:variable name="documents">
    <WordML Orientation="Portrait" LeftMargin="30" RightMargin="30" PageSize="Letter" />
  </xsl:variable>

  <xsl:template match="/*">
    <xsl:element name="Report">
      <xsl:variable name="doc" select="." />
      <xsl:for-each select="msxsl:node-set($documents)/WordML">
        <xsl:element name="WordML">
          <xsl:for-each select="@*">
            <xsl:attribute name="{name()}">
              <xsl:value-of select="."/>
            </xsl:attribute>
          </xsl:for-each>
          <w:hdr>
            <w:p>
              <w:pPr>
                <w:jc w:val="center"/>
                <w:spacing w:after="0" w:line="240" w:lineRule="auto"/>
              </w:pPr>
              <w:r>
                <w:rPr>
                  <w:color w:val="5895D8"/>
                  <w:sz w:val="32"/>
                  <w:szCs w:val="32"/>
                </w:rPr>
                <xsl:if test="position() > 1">
                  <w:br w:type="page"/>
                </xsl:if>
                <w:t>
                  <xsl:value-of select="$doc/Table/Title"/>
                </w:t>
              </w:r>
            </w:p>
            <w:p>
              <w:pPr>
                <w:pStyle w:val="Heading2"/>
                <w:jc w:val="center"/>
                <w:spacing w:after="0" w:line="240" w:lineRule="auto"/>
              </w:pPr>
              <w:r>
                <w:rPr>
                  <w:color w:val="5895D8"/>
                  <w:sz w:val="26"/>
                  <w:szCs w:val="26"/>
                </w:rPr>
                <w:t>
                  <xsl:value-of select="$doc/Table/SubTitle"/>
                </w:t>
              </w:r>
            </w:p>
            <w:p>
              <w:pPr>
                <w:pStyle w:val="Heading2"/>
                <w:jc w:val="center"/>
              </w:pPr>
              <w:r>
                <w:rPr>
                  <w:color w:val="5895D8"/>
                  <w:sz w:val="26"/>
                  <w:szCs w:val="26"/>
                </w:rPr>
                <w:t>
                  <xsl:value-of select="$doc/Table/SubTitle2"/>
                </w:t>
              </w:r>
            </w:p>
          </w:hdr>
          <xsl:variable name="width">
            <xsl:variable name="count" select="count($doc/Table1[1]/*)" />
            <xsl:variable name="enum" select="22" />
            <xsl:variable name="denom" select="100" />
            <xsl:for-each select="$doc/Table1[1]/*">
              <xsl:element name="Column">
                <xsl:attribute name="width">
                  <xsl:choose>
                    <xsl:when test="position() = last()">
                      <xsl:value-of select="number((5000 div $denom) * ($denom - (($count - 1) * $enum)))"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="number((5000 div $denom) * $enum)"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:attribute>
              </xsl:element>
            </xsl:for-each>
          </xsl:variable>
          <w:tbl>
            <w:tblPr>
              <w:sz w:val="16"/>
              <w:tblW w:w="5000" w:type="pct"/>
              <w:tblLook w:val="04A0" w:firstRow="1" w:lastRow="0" w:firstColumn="1" w:lastColumn="0" w:noHBand="0" w:noVBand="1"/>
              <w:tblBorders>
                <w:top w:val="single" w:sz="4" w:space="0" w:color="auto"/>
                <w:left w:val="single" w:sz="4" w:space="0" w:color="auto"/>
                <w:bottom w:val="single" w:sz="4" w:space="0" w:color="auto"/>
                <w:right w:val="single" w:sz="4" w:space="0" w:color="auto"/>
                <w:insideH w:val="single" w:sz="4" w:space="0" w:color="auto"/>
                <w:insideV w:val="single" w:sz="4" w:space="0" w:color="auto"/>
              </w:tblBorders>
            </w:tblPr>
            <w:tblGrid>
              <xsl:for-each select="$doc/Table1[1]/*">
                <xsl:variable name="pos" select="position()" />
                <xsl:element name="w:gridCol">
                  <xsl:attribute name="w:w">
                    <xsl:value-of select="msxsl:node-set($width)/Column[$pos]/@width"/>
                  </xsl:attribute>
                </xsl:element>
              </xsl:for-each>
            </w:tblGrid>
            <w:tr>
              <w:trPr>
                <w:tblHeader/>
              </w:trPr>
              <xsl:for-each select="$doc/Table1[1]/*">
                <xsl:call-template name="cell">
                  <xsl:with-param name="width" select="$width" />
                  <xsl:with-param name="data">
                    <xsl:call-template name="replace">
                      <xsl:with-param name="text" select="name()"/>
                      <xsl:with-param name="find">_x0020_</xsl:with-param>
                      <xsl:with-param name="replace">
                        <xsl:text> </xsl:text>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:with-param>
                  <xsl:with-param name="style">underline</xsl:with-param>
                  <xsl:with-param name="vMerge">none</xsl:with-param>
                </xsl:call-template>
              </xsl:for-each>
            </w:tr>
            <xsl:for-each select="$doc/Table1[not(preceding-sibling::Table1/*[2] = *[2])]">
              <w:tr>
                <xsl:for-each select="*">
                  <xsl:variable name="pos" select="position()" />
                  <w:tc>
                    <w:tcPr>
                      <xsl:element name="w:tcW">
                        <xsl:attribute name="w:w">
                          <xsl:value-of select="msxsl:node-set($width)/Column[$pos]/@width"/>
                        </xsl:attribute>
                        <xsl:attribute name="w:type">pct</xsl:attribute>
                      </xsl:element>
                    </w:tcPr>
                    <xsl:choose>
                      <xsl:when test="position() = last()">
                        <xsl:for-each select="$doc/Table1[*[2] = current()/../*[2]]/*[position() = last()]">
                          <w:p>
                            <w:pPr>
                              <w:spacing w:after="0" w:line="240" w:lineRule="auto"/>
                            </w:pPr>
                            <w:r>
                              <w:rPr>
                                <w:sz w:val="16" />
                              </w:rPr>
                              <w:t>
                                <xsl:value-of select="."/>
                              </w:t>
                            </w:r>
                          </w:p>
                        </xsl:for-each>
                      </xsl:when>
                      <xsl:otherwise>
                        <w:p>
                          <w:pPr>
                            <w:spacing w:after="0" w:line="240" w:lineRule="auto"/>
                          </w:pPr>
                          <w:r>
                            <w:rPr>
                              <w:sz w:val="16" />
                            </w:rPr>
                            <w:t>
                              <xsl:value-of select="."/>
                            </w:t>
                          </w:r>
                        </w:p>
                      </xsl:otherwise>
                    </xsl:choose>
                  </w:tc>
                </xsl:for-each>
              </w:tr>
            </xsl:for-each>
          </w:tbl>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template name="cell">
    <xsl:param name="width" />
    <xsl:param name="data" />
    <xsl:param name="style" />
    <xsl:param name="vMerge" />
    <xsl:element name="w:tc">
      <xsl:if test="$width">
        <xsl:element name="w:tcPr">
          <xsl:element name="w:tcW">
            <xsl:attribute name="w:w">
              <xsl:value-of select="$width"/>
            </xsl:attribute>
            <xsl:attribute name="w:type">pct</xsl:attribute>
          </xsl:element>
          <xsl:if test="$vMerge != 'none'">
            <xsl:element name="w:vMerge">
              <xsl:if test="$vMerge = 'restart'">
                <xsl:attribute name="w:val">restart</xsl:attribute>
              </xsl:if>
            </xsl:element>
          </xsl:if>
        </xsl:element>
      </xsl:if>
      <xsl:element name="w:p">
        <w:pPr>
          <w:spacing w:after="0" w:line="240" w:lineRule="auto"/>
        </w:pPr>
        <xsl:if test="$vMerge != 'merge'">
          <xsl:element name="w:r">
            <w:rPr>
              <w:sz w:val="16"/>
              <w:szCs w:val="16"/>
              <xsl:if test="contains($style, 'bold')">
                <w:b/>
              </xsl:if>
              <xsl:if test="contains($style, 'italic')">
                <w:i/>
              </xsl:if>
              <xsl:if test="contains($style, 'underline')">
                <w:u w:val="single"/>
              </xsl:if>
            </w:rPr>
            <xsl:if test="$vMerge = 'merge'">
              <w:t> </w:t>
            </xsl:if>
            <xsl:element name="w:t">
              <xsl:value-of select="$data"/>
            </xsl:element>
          </xsl:element>
        </xsl:if>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="replace">
    <xsl:param name="text"/>
    <xsl:param name="find"/>
    <xsl:param name="replace"/>
    <xsl:choose>
      <xsl:when test="contains($text, $find)">
        <xsl:variable name="before" select="substring-before($text, $find)"/>
        <xsl:variable name="after" select="substring-after($text, $find)"/>
        <xsl:value-of select="$before" disable-output-escaping="yes"/>
        <xsl:value-of select="$replace" disable-output-escaping="yes"/>
        <xsl:call-template name="replace">
          <xsl:with-param name="text" select="$after"/>
          <xsl:with-param name="find" select="$find"/>
          <xsl:with-param name="replace" select="$replace"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$text" disable-output-escaping="yes"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>