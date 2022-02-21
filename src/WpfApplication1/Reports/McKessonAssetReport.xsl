<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:efn="e24:Functions"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl efn"
				xmlns:w="http://schemas.microsoft.com/office/word/2003/wordml"
>
  <xsl:param name="Consumer">false</xsl:param>
  <xsl:output method="xml" indent="yes"/>
  <xsl:variable name="documents">
    <WordML Orientation="Portrait" LeftMargin="30" RightMargin="30" PageSize="Letter" DeleteLastSection="True" />
  </xsl:variable>
  <xsl:variable name="QuestionFields">
    <xsl:choose>
      <xsl:when test="$Consumer='true'">
        <Field name="Consumer Short Version" value="ConsQuestAbrv" column="Content" namecolumn="Field" />
        <Field name="Consumer Question" value="ConsQuestion" column="Content" namecolumn="Field" />
        <Field name="Consumer Rationale" value="ConsRationale" column="Content" namecolumn="Field" />
        <Field name="Consumer Unsure" value="ConsUnsureText" column="Content" namecolumn="Field" />
      </xsl:when>
      <xsl:otherwise>
        <Field name="Clinical Question" value="Clinical_Statement" column="Content" namecolumn="Field" />
        <Field name="Clinical Rationale" value="ClinicalRationale" column="Content" namecolumn="Field" />
        <Field name="Clinical Lay Question" value="LayQuestion" column="Content" namecolumn="Field" />
        <Field name="Clinical Unsure" value="UnsureInstruction" column="Content" namecolumn="Field" />
      </xsl:otherwise>
    </xsl:choose>
    <Field name="Yes leads to" value="YesNextNodes" column="Content" namecolumn="Field" type="nextnode" />
    <Field name="   Yes WOCB" value="YesNextNodes" column="Content" namecolumn="Field" type="watchout" />
    <Field name="No leads to" value="NoNextNodes" column="Content" namecolumn="Field" type="nextnode" />
    <Field name="   No WOCB" value="NoNextNodes" column="Content" namecolumn="Field" type="watchout" />
  </xsl:variable>
  <xsl:variable name="ConclusionFields">
    <xsl:choose>
      <xsl:when test="$Consumer='true'">
        <Field name="Consumer Rationale" value="ClinicalRationale" column="Content" namecolumn="Field" />
        <Field name="Consumer Message" value="ConsMessage" column="Content" namecolumn="Field" />
      </xsl:when>
      <xsl:otherwise>
        <Field name="Clinical Issues" value="ClinicalIssues" column="Content" namecolumn="Field" />
        <Field name="Clinical Rationale" value="ClinicalRationale" column="Content" namecolumn="Field" />
        <Field name="Clinical Message" value="MessageToPatient" column="Content" namecolumn="Field" />
        <!--<Field name="Symptom Pattern" value="SymptomPattern" column="Content" namecolumn="Field" />-->
      </xsl:otherwise>
    </xsl:choose>
    <Field name="Type" value="Type" column="Content" namecolumn="Field" />
    <Field name="Time Parameter" value="TimeParameter" column="Content" namecolumn="Field" />
    <Field name="Consumer Interim Selfcare" value="ConsInterimSC" column="Content" namecolumn="Field" />
    <Field name="Selfcare Name" value="SelfCareName" column="Content" namecolumn="Field" />
    <Field name="Recommendation" value="Recommendation" column="Content" namecolumn="Field" />
    <Field name="Destination Algorithm" value="DestinationAlgorithm" column="Content" namecolumn="Field" />
    <Field name="Destination #" value="TransferQuestionNumber" column="Content" namecolumn="Field" />
    <Field name="Destination Question" value="DestinationQuestion" column="Content" namecolumn="Field" />
    <!--<Field name="ConsRationale" value="ConsRationale" column="Content" namecolumn="Field" />-->
    <!--<Field name="ConsMessage" value="ConsMessage" column="Content" namecolumn="Field" />-->
    <Field name="Next #" value="NextNodes" column="Content" namecolumn="Field" type="nextnode" />
  </xsl:variable>
  <xsl:variable name="QuestionHeaderRow">
    <Column name="#" width="250" prefix="Q" value="NodeID" row="1" style="bold" />
    <Column name="Type" width="500" prefix="Question" row="1" style="bold" />
    <Column name="Field" width="900" />
    <Column name="Content" width="3350" style="bold" />
  </xsl:variable>
  <xsl:variable name="ConclusionHeaderRow">
    <Column name="#" width="250" value="NodeID" row="1" style="bold" />
    <Column name="Type" width="500" value="SubType" row="1" style="bold" />
    <Column name="Field" width="900" />
    <Column name="Content" width="3350" style="bold" />
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
            <xsl:variable name="header">
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
                    <!--<xsl:if test="position() > 1">
                      <w:br w:type="page"/>
                    </xsl:if>-->
                    <w:t>Algorithm Asset – Clinical</w:t>
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
                      <xsl:value-of select="Algo_Name"/>
                    </w:t>
                  </w:r>
                </w:p>
              </w:hdr>
            </xsl:variable>
            <xsl:variable name="algoid" select="AlgoID" />
            <xsl:variable name="tables">
              <xsl:element name="Table">
                <xsl:copy-of select="msxsl:node-set($QuestionFields)/Field"/>
                <xsl:copy-of select="msxsl:node-set($QuestionHeaderRow)/Column"/>
                <xsl:for-each select="$doc/Table1[AlgoID = $algoid]">
                  <xsl:element name="Asset">
                    <xsl:copy-of select="*"/>
                  </xsl:element>
                </xsl:for-each>
              </xsl:element>
              <xsl:element name="Table">
                <xsl:copy-of select="msxsl:node-set($ConclusionFields)/Field"/>
                <xsl:copy-of select="msxsl:node-set($ConclusionHeaderRow)/Column"/>
                <xsl:for-each select="$doc/Table2[AlgoID = $algoid]">
                  <xsl:element name="Asset">
                    <xsl:copy-of select="*"/>
                  </xsl:element>
                </xsl:for-each>
              </xsl:element>
              <!--<xsl:element name="Table">
                <xsl:copy-of select="msxsl:node-set($QuestionFields)/Field"/>
                <xsl:copy-of select="msxsl:node-set($QuestionRow)/Column"/>
                <xsl:copy-of select="$doc/Table2[AlgoID = $algoid]"/>
              </xsl:element>-->
            </xsl:variable>
            <xsl:for-each select="msxsl:node-set($tables)/Table">
              <xsl:variable name="tablelast">
                <xsl:choose>
                  <xsl:when test="position()=last()">last</xsl:when>
                  <xsl:otherwise></xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <xsl:for-each select="Asset">
                <xsl:variable name="last">
                  <xsl:choose>
                    <xsl:when test="$tablelast = 'last' and position()=last()">last</xsl:when>
                    <xsl:otherwise></xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <xsl:variable name="asset" select="." />
                <w:tbl>
                  <w:tblPr>
                    <w:tblStyle w:val="TableGrid"/>
                    <w:tblW w:w="5000" w:type="pct"/>
                    <w:tblLook w:val="04A0"/>
                  </w:tblPr>
                  <w:tblGrid>
                    <xsl:for-each select="../Column">
                      <xsl:element name="w:gridCol">
                        <xsl:attribute name="w:w">
                          <xsl:value-of select="@width"/>
                        </xsl:attribute>
                      </xsl:element>
                    </xsl:for-each>
                  </w:tblGrid>
                  <w:tr>
                    <xsl:for-each select="../Column">
                      <xsl:call-template name="cell">
                        <xsl:with-param name="width" select="@width" />
                        <xsl:with-param name="data" select="@name" />
                        <xsl:with-param name="style">underline</xsl:with-param>
                      </xsl:call-template>
                    </xsl:for-each>
                  </w:tr>
                  <xsl:variable name="fields">
                    <xsl:for-each select="../Field">
                      <xsl:element name="Field">
                        <xsl:copy-of select="@*" />
                        <xsl:variable name="value" select="$asset/*[name() = current()/@value]" />
                        <xsl:if test="$value != ''">
                          <xsl:element name="Value">
                            <xsl:value-of select="$value" />
                          </xsl:element>
                        </xsl:if>
                      </xsl:element>
                    </xsl:for-each>
                  </xsl:variable>
                  <!--<w:p>
                    <w:r>
                      <w:t>
                        <xsl:for-each select="msxsl:node-set($fields)/Field[Value]">
                          <xsl:value-of select="@value"/>
                        </xsl:for-each>
                      </w:t>
                    </w:r>
                  </w:p>-->
                  <xsl:for-each select="msxsl:node-set($fields)/Field[Value]">
                    <xsl:variable name="field" select="." />
                    <xsl:variable name="row" select="position()"/>
                    <xsl:element name="w:tr">
                      <xsl:for-each select="$asset/../Column">
                        <xsl:call-template name="cell">
                          <xsl:with-param name="width" select="@width" />
                          <xsl:with-param name="data">
                            <xsl:choose>
                              <xsl:when test="@name = $field/@column">
                                <xsl:choose>
                                  <xsl:when test="$field/@type = 'nextnode'">
                                    <xsl:variable name="xml" select="$asset/*[name() = $field/@value]" />
                                    <xsl:if test="$xml">
                                      <xsl:variable name="nodes" select="efn:Parse($xml)" />
                                      <xsl:for-each select="$nodes/root/node[not(@SubType='WatchoutCondition')][1]">
                                        <xsl:value-of select="@Prefix"/>
                                        <xsl:value-of select="@NodeID"/>
                                      </xsl:for-each>
                                    </xsl:if>
                                  </xsl:when>
                                  <xsl:when test="$field/@type = 'watchout'">
                                    <xsl:variable name="xml" select="$asset/*[name() = $field/@value]" />
                                    <xsl:if test="$xml">
                                      <xsl:variable name="nodes" select="efn:Parse($xml)" />
                                      <xsl:for-each select="$nodes/root/node[@SubType='WatchoutCondition'][1]">
                                        <xsl:value-of select="@AssetName"/>
                                      </xsl:for-each>
                                    </xsl:if>
                                  </xsl:when>
                                  <xsl:otherwise>
                                    <xsl:value-of select="$asset/*[name() = $field/@value]" />
                                  </xsl:otherwise>
                                </xsl:choose>
                              </xsl:when>
                              <xsl:when test="@name = $field/@namecolumn">
                                <xsl:value-of select="$field/@name" />
                              </xsl:when>
                              <xsl:otherwise>
                                <xsl:if test="not(@row) or @row = $row">
                                  <xsl:value-of select="@prefix" />
                                  <xsl:value-of select="$asset/*[name() = current()/@value]" />
                                </xsl:if>
                              </xsl:otherwise>
                            </xsl:choose>
                          </xsl:with-param>
                          <xsl:with-param name="style" select="@style" />
                        </xsl:call-template>
                      </xsl:for-each>
                    </xsl:element>
                  </xsl:for-each>
                </w:tbl>
                <xsl:if test="$last != 'last'">
                  <w:p/>
                </xsl:if>
              </xsl:for-each>
            </xsl:for-each>
            <w:p>
              <w:pPr>
                <w:sectPr>
                  <xsl:copy-of select="$header"/>
                </w:sectPr>
              </w:pPr>
              <w:r>
                <w:t></w:t>
              </w:r>
            </w:p>
          </xsl:for-each>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template name="cell">
    <xsl:param name="width" />
    <xsl:param name="data" />
    <xsl:param name="style" />
    <xsl:element name="w:tc">
      <xsl:if test="$width">
        <xsl:element name="w:tcPr">
          <xsl:element name="w:tcW">
            <xsl:attribute name="w:w">
              <xsl:value-of select="$width"/>
            </xsl:attribute>
            <xsl:attribute name="w:type">pct</xsl:attribute>
          </xsl:element>
        </xsl:element>
      </xsl:if>
      <xsl:element name="w:p">
        <w:pPr>
          <w:keepNext/>
          <w:spacing w:after="0" w:line="240" w:lineRule="auto"/>
          <xsl:if test="$data = ''">
            <w:rPr>
              <w:sz w:val="16"/>
              <w:szCs w:val="16"/>
            </w:rPr>
          </xsl:if>
        </w:pPr>
        <xsl:if test="$data != ''">
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
            <w:t>
              <xsl:value-of select="$data"/>
            </w:t>
          </xsl:element>
        </xsl:if>
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
