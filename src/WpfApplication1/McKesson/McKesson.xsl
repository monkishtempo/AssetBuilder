<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:efn="e24:Functions"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>
  <xsl:variable name="hs" select="20" />
  <xsl:variable name="vs" select="38" />
  <xsl:variable name="qho" select="5" />
  <xsl:variable name="qvo" select="113" />
  <xsl:variable name="yho" select="6.5" />
  <xsl:variable name="yvo" select="113" />
  <xsl:variable name="nho" select="5" />
  <xsl:variable name="nvo" select="111.5" />
  <xsl:variable name="aho" select="6.5" />
  <xsl:variable name="avo" select="113" />
  <xsl:variable name="cs" select="2.1" />
  <xsl:variable name="QuestionElements">
    <Elements>
      <ClinicalQuestion ReferenceInd="algo_text_ref" ReferenceCode="ClinicalQuestion" ReferenceValue="val:Clinical_Statement"/>
      <ClinicalRationale ReferenceInd="algo_text_ref" ReferenceCode="ClinicalRationale" ReferenceValue="properties"/>
      <LayQuestion ReferenceInd="algo_text_ref" ReferenceCode="LayQuestion" ReferenceValue="properties"/>
      <TextNote ReferenceInd="algo_text_ref" ReferenceCode="TextNote" ReferenceValue="properties"/>
      <UnsureInstruction ReferenceInd="algo_text_ref" ReferenceCode="UnsureInstruction" ReferenceValue="properties"/>
      <ConsQuestion ReferenceInd="algo_text_ref" ReferenceCode="ConsQuestion" ReferenceValue="properties"/>
      <ConsRationale ReferenceInd="algo_text_ref" ReferenceCode="ConsRationale" ReferenceValue="properties"/>
      <ConsUnsureText ReferenceInd="algo_text_ref" ReferenceCode="ConsUnsureText" ReferenceValue="properties"/>
      <ConsQuestAbrv ReferenceInd="algo_text_ref" ReferenceCode="ConsQuestAbrv" ReferenceValue="properties"/>
      <ClinQuestAbrv ReferenceInd="algo_text_ref" ReferenceCode="ClinQuestAbrv" ReferenceValue="properties"/>
    </Elements>
  </xsl:variable>
  <xsl:variable name="TransferElements">
    <Attributes>
      <SelfCareName ReferenceCode="SelfCareName" ReferenceValue="properties" />
      <TransferAlgoName ReferenceCode="TransferAlgoName" ReferenceValue="val:Algo_Name" />
      <TransferQuestionNumber ReferenceCode="TransferQuestionNumber" ReferenceValue="properties" />
    </Attributes>
    <Elements>
      <Type ReferenceInd="action_type_ref" ReferenceCode="TRA"/>
      <IntervalQuantity ReferenceInd="interval_quantity" ReferenceCode="properties"/>
      <IntervalUnit ReferenceInd="interval_unit" ReferenceCode="properties"/>
      <IntervalInMinutes ReferenceInd="interval_in_minutes" ReferenceCode="properties"/>
      <Position />
      <TransferType ReferenceInd="algo_gui_ref" ReferenceCode="properties"/>
      <ClinicalRationale ReferenceInd="algo_text_ref" ReferenceCode="ClinicalRationale" ReferenceValue="properties"/>
      <TextNote ReferenceInd="algo_text_ref" ReferenceCode="TextNote" Referencevalue=""/>
      <ConsRationale ReferenceInd="algo_text_ref" ReferenceCode="ConsRationale" ReferenceValue="properties"/>
      <ConsMessage ReferenceInd="algo_text_ref" ReferenceCode="ConsMessage" ReferenceValue="properties"/>
      <ConsInterimSC ReferenceInd="algo_text_ref" ReferenceCode="ConsInterimSC" ReferenceValue="properties"/>
      <ClinicalIssues ReferenceInd="algo_text_ref" ReferenceCode="ClinicalIssues" ReferenceValue="properties" optional="true"/>
      <MessageToPatient ReferenceInd="algo_text_ref" ReferenceCode="MessageToPatient" ReferenceValue="properties" optional="true"/>
      <SymptomPattern ReferenceInd="algo_text_ref" ReferenceCode="SymptomPattern" ReferenceValue="properties" optional="true"/>
    </Elements>
  </xsl:variable>
  <xsl:variable name="ConclusionElements">
    <Attributes>
      <SelfCareName ReferenceCode="SelfCareName" ReferenceValue="isSelfcare?val:Possible_Condition|properties" />
      <TransferAlgoName ReferenceCode="TransferAlgoName" ReferenceValue="val:Algo_Name" />
      <TransferQuestionNumber ReferenceCode="TransferQuestionNumber" ReferenceValue="properties" />
    </Attributes>
    <Elements>
      <Type ReferenceInd="action_type_ref" ReferenceCode="properties"/>
      <IntervalQuantity ReferenceInd="interval_quantity" ReferenceCode="properties"/>
      <IntervalUnit ReferenceInd="interval_unit" ReferenceCode="properties"/>
      <IntervalInMinutes ReferenceInd="interval_in_minutes" ReferenceCode="properties"/>
      <Position />
      <TransferType ReferenceInd="algo_gui_ref" ReferenceCode="properties"/>
      <ClinicalRationale ReferenceInd="algo_text_ref" ReferenceCode="ClinicalRationale" ReferenceValue="properties"/>
      <TextNote ReferenceInd="algo_text_ref" ReferenceCode="TextNote" Referencevalue=""/>
      <ConsRationale ReferenceInd="algo_text_ref" ReferenceCode="ConsRationale" ReferenceValue="properties"/>
      <ConsMessage ReferenceInd="algo_text_ref" ReferenceCode="ConsMessage" ReferenceValue="properties"/>
      <ConsInterimSC ReferenceInd="algo_text_ref" ReferenceCode="ConsInterimSC" ReferenceValue="properties"/>
      <ClinicalIssues ReferenceInd="algo_text_ref" ReferenceCode="ClinicalIssues" ReferenceValue="isSelfcare?properties|val:Possible_Condition"/>
      <MessageToPatient ReferenceInd="algo_text_ref" ReferenceCode="MessageToPatient" ReferenceValue="properties"/>
      <SymptomPattern ReferenceInd="algo_text_ref" ReferenceCode="SymptomPattern" ReferenceValue="properties"/>
    </Elements>
  </xsl:variable>

  <xsl:template match="/NewDataSet">
    <xsl:variable name="nds" select="." />
    <xsl:for-each select="Table">
      <xsl:variable name="xml" select="efn:Parse(../Table7[AlgoID = current()/AlgoID]/data)/root" />
      <xsl:variable name="props" select="../Table8[AlgoID = current()/AlgoID and DataID = concat(current()/AlgoID,':',current()/NodeID)]" />
      <xsl:element name="Algorithm">
        <xsl:attribute name="Name">
          <xsl:value-of select="Algo_Name"/>
        </xsl:attribute>
        <xsl:for-each select="$props[not(starts-with(PropertyName,'ACD')) and PropertyName != 'Module' and PropertyName != 'Category']">
          <xsl:attribute name="{PropertyName}">
            <xsl:value-of select="PropertyValue"/>
          </xsl:attribute>
        </xsl:for-each>
        <!--<xsl:attribute name="Version">
          <xsl:text>undefined</xsl:text>
        </xsl:attribute>
        <xsl:attribute name="Description">
          <xsl:text>undefined</xsl:text>
        </xsl:attribute>
        <xsl:attribute name="NoRelatedAlgorithms">
          <xsl:text>undefined</xsl:text>
        </xsl:attribute>
        <xsl:attribute name="NoAnticipatedCallDistributions">
          <xsl:text>undefined</xsl:text>
        </xsl:attribute>
        <xsl:attribute name="TargetOldestAge">
          <xsl:value-of select="Algo_Name"/>
        </xsl:attribute>
        <xsl:attribute name="TargetYoungestAge">
          <xsl:value-of select="Algo_Name"/>
        </xsl:attribute>
        <xsl:attribute name="ConsName">
          <xsl:text>undefined</xsl:text>
        </xsl:attribute>
        <xsl:attribute name="ConsDescription">
          <xsl:text>undefined</xsl:text>
        </xsl:attribute>
        <xsl:attribute name="ConsFlag">
          <xsl:text>undefined</xsl:text>
        </xsl:attribute>-->
        <xsl:attribute name="FirstQuestionNumber">
          <xsl:value-of select="NextNodeID"/>
        </xsl:attribute>
        <xsl:element name="Module">
          <xsl:attribute name="ReferenceInd">algo_type_ref</xsl:attribute>
          <xsl:attribute name="ReferenceCode">
            <xsl:value-of select="$props[PropertyName = 'Module']/PropertyValue"/>
          </xsl:attribute>
        </xsl:element>
        <xsl:element name="Category">
          <xsl:attribute name="ReferenceInd">algo_cat_ref</xsl:attribute>
          <xsl:attribute name="ReferenceCode">
            <xsl:value-of select="$props[PropertyName = 'Category']/PropertyValue"/>
          </xsl:attribute>
        </xsl:element>
        <xsl:call-template name="Keywords">
          <xsl:with-param name="rem" select="Keywords"/>
        </xsl:call-template>
        <xsl:for-each select="$props[starts-with(PropertyName,'ACD')]">
          <xsl:element name="AnticipatedCallDistribution">
            <xsl:attribute name="ReferenceInd">action_type_ref</xsl:attribute>
            <xsl:attribute name="ReferenceCode">
              <xsl:value-of select="substring(PropertyName, 5)"/>
            </xsl:attribute>
            <xsl:attribute name="Percentage">
              <xsl:value-of select="PropertyValue"/>
            </xsl:attribute>
          </xsl:element>
        </xsl:for-each>
        <!--<xsl:element name="AnticipatedCallDistribution">
          <xsl:attribute name="ReferenceInd">action_type_ref</xsl:attribute>
          <xsl:attribute name="ReferenceCode">ASC</xsl:attribute>
          <xsl:attribute name="Percentage">undefined</xsl:attribute>
        </xsl:element>
        <xsl:element name="AnticipatedCallDistribution">
          <xsl:attribute name="ReferenceInd">action_type_ref</xsl:attribute>
          <xsl:attribute name="ReferenceCode">EIA</xsl:attribute>
          <xsl:attribute name="Percentage">undefined</xsl:attribute>
        </xsl:element>
        <xsl:element name="AnticipatedCallDistribution">
          <xsl:attribute name="ReferenceInd">action_type_ref</xsl:attribute>
          <xsl:attribute name="ReferenceCode">STP</xsl:attribute>
          <xsl:attribute name="Percentage">undefined</xsl:attribute>
        </xsl:element>
        <xsl:element name="AnticipatedCallDistribution">
          <xsl:attribute name="ReferenceInd">action_type_ref</xsl:attribute>
          <xsl:attribute name="ReferenceCode">UC-GM</xsl:attribute>
          <xsl:attribute name="Percentage">undefined</xsl:attribute>
        </xsl:element>-->
        <xsl:for-each select="../Table5[AlgoID = current()/AlgoID and not(AlgoID = preceding-sibling::Table5/AlgoID)]">
          <xsl:sort select="Algo_Name"/>
          <xsl:element name="RelatedAlgo">
            <xsl:attribute name="Name">
              <xsl:value-of select="Algo_Name"/>
            </xsl:attribute>
          </xsl:element>
        </xsl:for-each>
        <xsl:for-each select="../Table1[AlgoID = current()/AlgoID]">
          <xsl:sort select="NodeID" data-type="number"/>
          <xsl:variable name="val" select="." />
          <xsl:variable name="qxml" select="$xml/Question[@AlgoID = current()/AlgoID and @NodeID = current()/NodeID]" />
          <xsl:variable name="qprops" select="../Table8[AlgoID = $val/AlgoID and DataID = concat($val/AlgoID,':',$val/NodeID)]" />
          <xsl:element name="Question">
            <xsl:attribute name="Number">
              <xsl:value-of select="NodeID"/>
            </xsl:attribute>
            <xsl:attribute name="NodeId">
              <xsl:value-of select="AlgoID * 100000 + NodeID"/>
            </xsl:attribute>
            <xsl:call-template name="Position">
              <xsl:with-param name="axml" select="$qxml" />
            </xsl:call-template>
            <xsl:call-template name="Elements">
              <xsl:with-param name="val" select="$val" />
              <xsl:with-param name="nodeset" select="$QuestionElements" />
              <xsl:with-param name="axml" select="$qxml" />
              <xsl:with-param name="props" select="$qprops" />
            </xsl:call-template>
            <xsl:for-each select="../Table2[AlgoID = current()/AlgoID and NodeID = current()/NodeID]">
              <xsl:sort select="AssetID" data-type="number" order="descending"/>
              <xsl:variable name="algoid" select="AlgoID" />
              <xsl:variable name="nextnodeid" select="NextNodeID" />
              <xsl:element name="Answer">
                <xsl:variable name="next" select="efn:NextNode(.,$algoid,$nextnodeid)" />
                <!--<xsl:variable name="na" select="efn:NextAction($next,$next/AlgoID,$next/NodeID,$next/NextNodeID)" />-->
                <xsl:variable name="nq" select="efn:NextQuestion($next,$next/AlgoID,$next/NextNodeID)" />
                <xsl:variable name="actions" select="efn:NextConclusions($next,$next/AlgoID,$next/NextNodeID)" />
                <!--<xsl:choose>
                    <xsl:when test="name($next) = 'Table3' and ../Table8[AlgoID = $next/AlgoID and DataID = concat($next/AlgoID,':',$next/NodeID) and PropertyName = 'LanguageRef']">
                      <xsl:value-of select="../*[AlgoID = $next/AlgoID and NodeID = $next/NextNodeID and name() != 'Table2']" />
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:copy-of select="msxsl:node-set($next)[1]" />
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>-->
                <!--<xsl:choose>
                    <xsl:when test="name($na) = 'Table3'">
                      <xsl:value-of select="../*[AlgoID = $na/AlgoID and NodeID = $na/NextNodeID and name() != 'Table2']" />
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$na" />
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>-->
                <xsl:attribute name="AENextQuestionNumber">
                  <xsl:if test="local-name($next) = 'Table1'">
                    <xsl:value-of select="$nq/NodeID"/>
                  </xsl:if>
                </xsl:attribute>
                <xsl:attribute name="PHANextQuestionNumber">
                  <xsl:if test="local-name($nq) = 'Table1'">
                    <xsl:value-of select="$nq/NodeID"/>
                  </xsl:if>
                </xsl:attribute>
                <xsl:element name="AnswerAnswer">
                  <xsl:attribute name="ReferenceInd">answer_ref</xsl:attribute>
                  <xsl:attribute name="ReferenceCode">
                    <xsl:value-of select="efn:ToUpper(Clinical_Answer)"/>
                  </xsl:attribute>
                </xsl:element>
                <xsl:for-each select="$actions/*">
                  <xsl:variable name="cAlgoID" select="AlgoID" />
                  <xsl:variable name="cNodeID" select="NodeID" />
                  <xsl:variable name="cNextNodeID" select="NextNodeID" />
                  <xsl:variable name="ctprops" select="$nds/Table8[AlgoID = $cAlgoID and DataID = concat($cAlgoID,':',$cNodeID)]" />
                  <xsl:if test="$ctprops[PropertyName = 'LanguageRef'] or Information = 'true'">
                    <xsl:element name="WatchoutCondition">
                      <xsl:attribute name="LanguageRef">
                        <xsl:choose>
                          <xsl:when test="not($ctprops[PropertyName = 'LanguageRef'])">
                            <xsl:text>ENGL</xsl:text>
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:value-of select="$ctprops[PropertyName = 'LanguageRef']/PropertyValue"/>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:attribute>
                      <xsl:attribute name="Description">
                        <xsl:value-of select="Possible_Condition"/>
                      </xsl:attribute>
                    </xsl:element>
                  </xsl:if>
                  <xsl:if test="not($ctprops[PropertyName = 'LanguageRef'] or Information = 'true')">
                    <!--<xsl:variable name="naprops" select="../Table8[AlgoID = $na/AlgoID and DataID = concat($na/AlgoID,':',$na/NodeID)]" />-->
                    <xsl:element name="Action">
                      <xsl:variable name="endpoint">
                        <xsl:value-of select="boolean($nds/Table6[AlgoID = $cAlgoID and NodeID = $cNextNodeID])"/>
                      </xsl:variable>
                      <xsl:attribute name="Number">
                        <xsl:value-of select="NodeID"/>
                      </xsl:attribute>
                      <xsl:attribute name="GUIActionSeq">
                        <xsl:value-of select="position()"/>
                      </xsl:attribute>
                      <xsl:attribute name="GUIEndpointFlag">
                        <xsl:choose>
                          <xsl:when test="$endpoint='true'">True</xsl:when>
                          <xsl:otherwise>False</xsl:otherwise>
                        </xsl:choose>
                      </xsl:attribute>
                      <xsl:attribute name="NodeId">
                        <xsl:value-of select="AlgoID * 100000 + NodeID"/>
                      </xsl:attribute>
                      <xsl:attribute name="NextQuestionNumber">
                        <xsl:choose>
                          <xsl:when test="$endpoint='true'"></xsl:when>
                          <xsl:otherwise>
                            <xsl:if test="name($nq) = 'Table1' and $nq/NodeID > 0">
                              <xsl:value-of select="$nq/NodeID"/>
                            </xsl:if>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:attribute>
                      <xsl:attribute name="NoSearchCodes">False</xsl:attribute>
                      <xsl:call-template name="Elements">
                        <xsl:with-param name="val" select="." />
                        <xsl:with-param name="nodeset">
                          <xsl:choose>
                            <xsl:when test="local-name() = 'Table5'">
                              <xsl:copy-of select="$TransferElements" />
                            </xsl:when>
                            <xsl:when test="local-name() = 'Table3'">
                              <xsl:copy-of select="$ConclusionElements" />
                            </xsl:when>
                          </xsl:choose>
                        </xsl:with-param>
                        <xsl:with-param name="props" select="$ctprops"/>
                        <xsl:with-param name="axml" select="$xml/*[@AlgoID = $cAlgoID and @NodeID = $cNodeID]" />
                      </xsl:call-template>
                    </xsl:element>
                  </xsl:if>
                </xsl:for-each>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="Keywords">
    <xsl:param name="rem" />
    <xsl:variable name="kwb" select="substring-before($rem, ',')" />
    <xsl:variable name="kw">
      <xsl:choose>
        <xsl:when test="$kwb = ''">
          <xsl:value-of select="$rem"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$kwb"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="kws" select="substring-after($rem, ',')" />
    <xsl:if test="$kw != ''">
      <xsl:element name="AlgoKeyword">
        <xsl:attribute name="Id">
          <xsl:value-of select="efn:KeywordID($kw)"/>
        </xsl:attribute>
        <xsl:attribute name="Description">
          <xsl:value-of select="efn:Trim($kw)"/>
        </xsl:attribute>
      </xsl:element>
    </xsl:if>
    <xsl:if test="$kws != ''">
      <xsl:call-template name="Keywords">
        <xsl:with-param name="rem">
          <xsl:value-of select="$kws"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="Elements">
    <xsl:param name="val" />
    <xsl:param name="nodeset" />
    <xsl:param name="axml" />
    <xsl:param name="props" />
    <xsl:for-each select="msxsl:node-set($nodeset)/Attributes/*/@ReferenceValue">
      <xsl:attribute name="{name(..)}">
        <xsl:call-template name="PropertyValue">
          <xsl:with-param name="val" select="$val" />
          <xsl:with-param name="props" select="$props" />
        </xsl:call-template>
      </xsl:attribute>
    </xsl:for-each>
    <xsl:for-each select="msxsl:node-set($nodeset)/Elements/*">
      <xsl:choose>
        <xsl:when test="name() = 'Position'">
          <xsl:call-template name="Position">
            <xsl:with-param name="axml" select="$axml" />
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="element">
            <xsl:element name="{name()}">
              <xsl:for-each select="@*[name() != 'optional']">
                <xsl:attribute name="{name()}">
                  <xsl:call-template name="replace-breaks">
                    <xsl:with-param name="text">
                      <xsl:call-template name="PropertyValue">
                        <xsl:with-param name="val" select="$val" />
                        <xsl:with-param name="props" select="$props" />
                      </xsl:call-template>
                    </xsl:with-param>
                  </xsl:call-template>
                </xsl:attribute>
              </xsl:for-each>
            </xsl:element>
          </xsl:variable>
          <xsl:if test="not(@optional and msxsl:node-set($element)/*/@ReferenceValue = '')">
            <xsl:copy-of select="$element"/>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="PropertyValue">
    <xsl:param name="val" />
    <xsl:param name="props" />
    <xsl:variable name="test">
      <xsl:choose>
        <xsl:when test="substring-before(., '?') = 'isSelfcare'">
          <xsl:choose>
            <xsl:when test="$val[Silent='true']">
              <xsl:value-of select="substring-before(substring-after(., '?'), '|')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="substring-after(., '|')"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="."/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$test = 'properties'">
        <xsl:variable name="pn" select="name(..)" />
        <xsl:value-of select="$props[PropertyName = $pn]/PropertyValue"/>
      </xsl:when>
      <xsl:when test="substring-before($test, ':') = 'val'">
        <xsl:variable name="name" select="substring-after($test, ':')" />
        <xsl:value-of select="$val/*[name() = $name]"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$test" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="Position">
    <xsl:param name="axml" />
    <xsl:element name="Position">
      <xsl:attribute name="Height">
        <xsl:value-of select="80"/>
      </xsl:attribute>
      <xsl:attribute name="Left">
        <xsl:value-of select="round(($axml/@PinX - $qho) * $hs)"/>
      </xsl:attribute>
      <xsl:attribute name="Top">
        <xsl:value-of select="round(($qvo - $axml/@PinY) * $vs)"/>
      </xsl:attribute>
      <xsl:attribute name="Width">
        <xsl:value-of select="84"/>
      </xsl:attribute>
    </xsl:element>
  </xsl:template>

  <xsl:template name="replace-breaks">
    <xsl:param name="text"/>
    <xsl:choose>
      <xsl:when test="contains($text, '&#xA;')">
        <xsl:variable name="before" select="substring-before($text, '&#xA;')"/>
        <xsl:variable name="after" select="substring-after($text, '&#xA;')"/>
        <xsl:value-of select="$before" disable-output-escaping="yes"/>
        <xsl:if test="string-length($after)>4">
          <xsl:text>&#xD;&#xA;</xsl:text>
          <xsl:call-template name="replace-breaks">
            <xsl:with-param name="text" select="$after"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$text" disable-output-escaping="yes"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
