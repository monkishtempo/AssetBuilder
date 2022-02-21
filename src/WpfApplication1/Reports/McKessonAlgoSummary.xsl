<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
				xmlns:w="http://schemas.microsoft.com/office/word/2003/wordml"
>
  <xsl:param name="Consumer">false</xsl:param>
  <xsl:output method="xml" indent="yes"/>
  <xsl:variable name="documents">
    <WordML Orientation="Portrait" PageSize="Letter" />
  </xsl:variable>
  <xsl:variable name="rows">
    <xsl:choose>
      <xsl:when test="$Consumer='true'">
        <Row name="Consumer Title" property="ConsName" />
        <Row name="Consumer Purpose" property="ConsDescription" />
      </xsl:when>
      <xsl:otherwise>
        <Row name="Clinical Purpose" value="ClinicalPurpose" />
      </xsl:otherwise>
    </xsl:choose>
    <Row name="Category" property="Category" />
    <Row name="Module" property="Module" />
    <Row name="" />
    <Row name="Keywords" table="Keyword" columns="4" />
    <Row name="" />
    <Row name="Related Algorithms" table="RelatedAlgo" columns="2" />
    <Row name="" />
    <Row name="Clinical Associated Selfcare" table="Selfcare" columns="2" />
    <Row name="" />
    <Row name="Anticipated Call Distribution" function="ACD" />
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
          <xsl:for-each select="$doc/Table">
            <xsl:variable name="algo" select="." />
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
                  <xsl:text>Algorithm Overview – </xsl:text>
                  <xsl:if test="$Consumer!='true'">
                    <xsl:text>Clinical</xsl:text>
                  </xsl:if>
                </w:t>
              </w:r>
              <xsl:if test="$Consumer='true'">
                <w:r>
                  <w:rPr>
                    <!--<w:color w:val="FF3300"/>-->
                    <w:color w:val="5895D8"/>
                    <w:sz w:val="32"/>
                    <w:szCs w:val="32"/>
                  </w:rPr>
                  <w:t>
                    <xsl:text>Consumer</xsl:text>
                  </w:t>
                </w:r>
              </xsl:if>
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
                  <xsl:choose>
                    <xsl:when test="$Consumer='true'">
                      <xsl:value-of select="Algo_Name"/>
                      <!--<xsl:value-of select="$doc/Table1[AlgoID = $algo/AlgoID and PropertyName = 'ConsName']/PropertyValue"/>-->
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="Algo_Name"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </w:t>
              </w:r>
            </w:p>
            <w:tbl>
              <w:tblPr>
                <w:tblStyle w:val="TableGrid"/>
                <w:tblW w:w="0" w:type="auto"/>
                <w:tblBorders>
                  <w:top w:val="none" w:sz="0" w:space="0" w:color="auto"/>
                  <w:left w:val="none" w:sz="0" w:space="0" w:color="auto"/>
                  <w:bottom w:val="none" w:sz="0" w:space="0" w:color="auto"/>
                  <w:right w:val="none" w:sz="0" w:space="0" w:color="auto"/>
                  <w:insideH w:val="none" w:sz="0" w:space="0" w:color="auto"/>
                  <w:insideV w:val="none" w:sz="0" w:space="0" w:color="auto"/>
                </w:tblBorders>
                <w:tblLook w:val="04A0" w:firstRow="1" w:lastRow="0" w:firstColumn="1" w:lastColumn="0" w:noHBand="0" w:noVBand="1"/>
              </w:tblPr>
              <xsl:for-each select="msxsl:node-set($rows)/Row">
                <w:tr>
                  <w:tc>
                    <w:tcPr>
                      <w:tcW w:w="2000" w:type="dxa"/>
                    </w:tcPr>
                    <w:p>
                      <w:pPr>
                        <w:spacing w:after="0" w:line="240" w:lineRule="auto"/>
                      </w:pPr>
                      <w:r>
                        <w:rPr>
                          <w:sz w:val="18"/>
                          <w:szCs w:val="18"/>
                        </w:rPr>
                        <w:t>
                          <xsl:value-of select="@name"/>
                        </w:t>
                      </w:r>
                    </w:p>
                  </w:tc>
                  <w:tc>
                    <w:p>
                      <w:pPr>
                        <w:spacing w:after="0" w:line="240" w:lineRule="auto"/>
                      </w:pPr>
                      <w:r>
                        <w:rPr>
                          <w:sz w:val="18"/>
                          <w:szCs w:val="18"/>
                          <w:b/>
                        </w:rPr>
                        <w:t>
                          <xsl:if test="@property">
                            <xsl:value-of select="$doc/Table1[AlgoID = $algo/AlgoID and PropertyName = current()/@property]/PropertyValue"/>
                          </xsl:if>
                          <xsl:if test="@value">
                            <xsl:value-of select="$algo/*[name() = current()/@value]"/>
                          </xsl:if>
                          <xsl:if test="@table">
                            <xsl:call-template name="table">
                              <xsl:with-param name="columns" select="@columns" />
                              <xsl:with-param name="data" select="$doc/Table2[AlgoID = $algo/AlgoID and Type = current()/@table]" /> <!-- [$Consumer!='true' or not(Consumer) or Consumer='Y'] -->
                            </xsl:call-template>
                          </xsl:if>
                          <xsl:if test="@function='ACD'">
                            <xsl:variable name="data">
                              <Table2>
                                <Value>Activate emergency procedure</Value>
                              </Table2>
                              <Table2>
                                <Value>
                                  <xsl:call-template name="percent">
                                    <xsl:with-param name="value" select="$doc/Table1[AlgoID = $algo/AlgoID and PropertyName = 'ACD_AEP']/PropertyValue" />
                                  </xsl:call-template>
                                </Value>
                              </Table2>
                              <Table2>
                                <Value>Urgent care</Value>
                              </Table2>
                              <Table2>
                                <Value>
                                  <xsl:call-template name="percent">
                                    <xsl:with-param name="value" select="$doc/Table1[AlgoID = $algo/AlgoID and PropertyName = 'ACD_UC-GM']/PropertyValue" />
                                  </xsl:call-template>
                                </Value>
                              </Table2>
                              <Table2>
                                <Value>Speak to provider</Value>
                              </Table2>
                              <Table2>
                                <Value>
                                  <xsl:call-template name="percent">
                                    <xsl:with-param name="value" select="$doc/Table1[AlgoID = $algo/AlgoID and PropertyName = 'ACD_STP']/PropertyValue" />
                                  </xsl:call-template>
                                </Value>
                              </Table2>
                              <Table2>
                                <Value>Early illness appointment</Value>
                              </Table2>
                              <Table2>
                                <Value>
                                  <xsl:call-template name="percent">
                                    <xsl:with-param name="value" select="$doc/Table1[AlgoID = $algo/AlgoID and PropertyName = 'ACD_EIA']/PropertyValue" />
                                  </xsl:call-template>
                                </Value>
                              </Table2>
                              <Table2>
                                <Value>Routine illness appointment</Value>
                              </Table2>
                              <Table2>
                                <Value>
                                  <xsl:call-template name="percent">
                                    <xsl:with-param name="value" select="$doc/Table1[AlgoID = $algo/AlgoID and PropertyName = 'ACD_RIA']/PropertyValue" />
                                  </xsl:call-template>
                                </Value>
                              </Table2>
                              <Table2>
                                <Value>Access selfcare instructions</Value>
                              </Table2>
                              <Table2>
                                <Value>
                                  <xsl:call-template name="percent">
                                    <xsl:with-param name="value" select="$doc/Table1[AlgoID = $algo/AlgoID and PropertyName = 'ACD_ASC']/PropertyValue" />
                                  </xsl:call-template>
                                </Value>
                              </Table2>
                            </xsl:variable>
                            <xsl:call-template name="table">
                              <xsl:with-param name="columns" select="2" />
                              <xsl:with-param name="data" select="msxsl:node-set($data)/Table2" />
                            </xsl:call-template>
                          </xsl:if>
                        </w:t>
                      </w:r>
                    </w:p>
                  </w:tc>
                </w:tr>
              </xsl:for-each>
            </w:tbl>
          </xsl:for-each>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template name="table">
    <xsl:param name="columns" />
    <xsl:param name="data" />
    <w:tbl>
      <w:tblCellMar>
        <w:left w:w="0" w:type="dxa"/>
      </w:tblCellMar>
      <xsl:call-template name="row">
        <xsl:with-param name="columns" select="$columns" />
        <xsl:with-param name="pos" select="1" />
        <xsl:with-param name="data" select="$data" />
      </xsl:call-template>
    </w:tbl>
  </xsl:template>

  <xsl:template name="row">
    <xsl:param name="columns" />
    <xsl:param name="pos" />
    <xsl:param name="data" />
    <w:tr>
      <xsl:call-template name="cell">
        <xsl:with-param name="columns" select="$columns" />
        <xsl:with-param name="pos" select="$pos" />
        <xsl:with-param name="data" select="$data" />
      </xsl:call-template>
    </w:tr>
    <xsl:if test="count($data) >= ($pos + $columns)">
      <xsl:call-template name="row">
        <xsl:with-param name="columns" select="$columns" />
        <xsl:with-param name="pos" select="$pos + $columns" />
        <xsl:with-param name="data" select="$data" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="cell">
    <xsl:param name="columns" />
    <xsl:param name="pos" />
    <xsl:param name="data" />
    <xsl:element name="w:tc">
      <xsl:element name="w:tcPr">
        <xsl:element name="w:tcW">
          <xsl:attribute name="w:w">
            <xsl:value-of select="7000 div $columns"/>
          </xsl:attribute>
          <xsl:attribute name="w:type">dxa</xsl:attribute>
        </xsl:element>
      </xsl:element>
      <xsl:element name="w:p">
        <w:pPr>
          <w:spacing w:after="0" w:line="240" w:lineRule="auto"/>
        </w:pPr>
        <xsl:element name="w:r">
          <w:rPr>
            <w:sz w:val="18"/>
            <w:szCs w:val="18"/>
            <w:b/>
            <xsl:if test="$Consumer='true' and $data[$pos]/Consumer!='Y'">
              <w:i/>
              <w:color w:val="888888"/>
            </xsl:if>
          </w:rPr>
          <xsl:element name="w:t">
            <xsl:value-of select="$data[$pos]/Value"/>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>
    <xsl:if test="$pos mod $columns > 0">
      <xsl:call-template name="cell">
        <xsl:with-param name="columns" select="$columns" />
        <xsl:with-param name="pos" select="$pos + 1" />
        <xsl:with-param name="data" select="$data" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="percent">
    <xsl:param name="value" />
    <xsl:choose>
      <xsl:when test="$value > 0">
        <xsl:value-of select="$value"/>
      </xsl:when>
      <xsl:otherwise>0</xsl:otherwise>
    </xsl:choose>
    <xsl:text>%</xsl:text>
  </xsl:template>
</xsl:stylesheet>
