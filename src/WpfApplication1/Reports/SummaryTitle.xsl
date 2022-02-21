<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
				xmlns:w="http://schemas.microsoft.com/office/word/2003/wordml"
>
	<xsl:output method="xml" indent="yes"/>
	<xsl:variable name="documents">
		<WordML Orientation="Portrait" />
	</xsl:variable>
	<xsl:variable name="cr" select="'&#10;'" />

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
					<xsl:variable name="num" select="position()"/>
					<xsl:for-each select="$doc">
						<xsl:call-template name="Paragraph">
							<xsl:with-param name="Text">Summary Title Report</xsl:with-param>
							<xsl:with-param name="Align">center</xsl:with-param>
							<xsl:with-param name="Bold">1</xsl:with-param>
							<xsl:with-param name="Underline">single</xsl:with-param>
							<xsl:with-param name="FontSize">28</xsl:with-param>
						</xsl:call-template>
						<xsl:call-template name="Paragraph">
							<xsl:with-param name="Text">
								<xsl:call-template name="algo-names"/>
							</xsl:with-param>
							<xsl:with-param name="Align">center</xsl:with-param>
							<xsl:with-param name="Bold">1</xsl:with-param>
							<xsl:with-param name="Underline">single</xsl:with-param>
							<xsl:with-param name="FontSize">12</xsl:with-param>
						</xsl:call-template>
						<xsl:if test="Table1">
							<xsl:call-template name="SummaryPoints"/>
						</xsl:if>
					</xsl:for-each>
				</xsl:element>
			</xsl:for-each>
		</xsl:element>
	</xsl:template>

	<xsl:template name="SummaryPoints">
		<xsl:element name="w:tbl">
			<xsl:element name="w:tblPr">
				<xsl:element name="w:tblW">
					<xsl:attribute name="w:w">9220</xsl:attribute>
					<xsl:attribute name="w:type">dxa</xsl:attribute>
				</xsl:element>
			</xsl:element>
			<xsl:element name="w:tblGrid">
				<xsl:element name="w:gridCol">
					<xsl:attribute name="w:w">7380</xsl:attribute>
				</xsl:element>
				<xsl:element name="w:gridCol">
					<xsl:attribute name="w:w">920</xsl:attribute>
				</xsl:element>
				<xsl:element name="w:gridCol">
					<xsl:attribute name="w:w">920</xsl:attribute>
				</xsl:element>
			</xsl:element>
			<xsl:element name="w:tr">
				<xsl:element name="w:tc">
					<xsl:element name="w:tcPr">
						<xsl:element name="w:tcW">
							<xsl:attribute name="w:w">7380</xsl:attribute>
							<xsl:attribute name="w:type">dxa</xsl:attribute>
						</xsl:element>
					</xsl:element>
					<xsl:call-template name="Paragraph">
						<xsl:with-param name="Bold">1</xsl:with-param>
						<xsl:with-param name="Underline">single</xsl:with-param>
						<xsl:with-param name="Text">SummaryText</xsl:with-param>
					</xsl:call-template>
				</xsl:element>
				<xsl:element name="w:tc">
					<xsl:element name="w:tcPr">
						<xsl:element name="w:tcW">
							<xsl:attribute name="w:w">920</xsl:attribute>
							<xsl:attribute name="w:type">dxa</xsl:attribute>
						</xsl:element>
					</xsl:element>
					<xsl:call-template name="Paragraph">
						<xsl:with-param name="Bold">1</xsl:with-param>
						<xsl:with-param name="Underline">single</xsl:with-param>
						<xsl:with-param name="Text">QuestionID</xsl:with-param>
					</xsl:call-template>
				</xsl:element>
				<xsl:element name="w:tc">
					<xsl:element name="w:tcPr">
						<xsl:element name="w:tcW">
							<xsl:attribute name="w:w">920</xsl:attribute>
							<xsl:attribute name="w:type">dxa</xsl:attribute>
						</xsl:element>
					</xsl:element>
					<xsl:call-template name="Paragraph">
						<xsl:with-param name="Bold">1</xsl:with-param>
						<xsl:with-param name="Underline">single</xsl:with-param>
						<xsl:with-param name="Text">AnswerID</xsl:with-param>
					</xsl:call-template>
				</xsl:element>
			</xsl:element>
			<xsl:for-each select="Table1">
				<xsl:variable name="id" select="QuestionID"/>
				<xsl:if test="not(preceding-sibling::Table1[QuestionID=$id])">
					<xsl:element name="w:tr">
						<xsl:element name="w:tc">
							<xsl:element name="w:tcPr">
								<xsl:element name="w:tcW">
									<xsl:attribute name="w:w">9220</xsl:attribute>
									<xsl:attribute name="w:type">dxa</xsl:attribute>
								</xsl:element>
								<xsl:element name="w:gridSpan">
									<xsl:attribute name="w:val">3</xsl:attribute>
								</xsl:element>
							</xsl:element>
							<xsl:call-template name="Paragraph">
								<xsl:with-param name="Bold">1</xsl:with-param>
								<xsl:with-param name="Underline">single</xsl:with-param>
								<xsl:with-param name="Text">
									<xsl:text>Question # </xsl:text>
									<xsl:value-of select="$id"/>
									<xsl:text>: </xsl:text>
									<xsl:value-of select="Clinical_Statement"/>
								</xsl:with-param>
							</xsl:call-template>
						</xsl:element>
					</xsl:element>
				</xsl:if>
				<xsl:element name="w:tr">
					<xsl:element name="w:tc">
						<xsl:element name="w:tcPr">
							<xsl:element name="w:tcW">
								<xsl:attribute name="w:w">7380</xsl:attribute>
								<xsl:attribute name="w:type">dxa</xsl:attribute>
							</xsl:element>
						</xsl:element>
						<xsl:call-template name="Paragraph">
							<xsl:with-param name="Text">
								<xsl:value-of select="SummaryText"/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:element>
					<xsl:element name="w:tc">
						<xsl:element name="w:tcPr">
							<xsl:element name="w:tcW">
								<xsl:attribute name="w:w">920</xsl:attribute>
								<xsl:attribute name="w:type">dxa</xsl:attribute>
							</xsl:element>
						</xsl:element>
						<xsl:call-template name="Hyperlink">
							<xsl:with-param name="Link">
								<xsl:text>assetbuilder:Question.</xsl:text>
								<xsl:value-of select="QuestionID"/>
							</xsl:with-param>
							<xsl:with-param name="Text">
								<xsl:value-of select="QuestionID"/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:element>
					<xsl:element name="w:tc">
						<xsl:element name="w:tcPr">
							<xsl:element name="w:tcW">
								<xsl:attribute name="w:w">920</xsl:attribute>
								<xsl:attribute name="w:type">dxa</xsl:attribute>
							</xsl:element>
						</xsl:element>
						<xsl:call-template name="Hyperlink">
							<xsl:with-param name="Link">
								<xsl:text>assetbuilder:Answer.</xsl:text>
								<xsl:value-of select="AnswerID"/>
							</xsl:with-param>
							<xsl:with-param name="Text">
								<xsl:value-of select="AnswerID"/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:element>
				</xsl:element>
			</xsl:for-each>
		</xsl:element>
	</xsl:template>

	<xsl:template name="Range">
		<xsl:param name="Text"/>
		<xsl:param name="Style"/>
		<xsl:param name="Bold"/>
		<xsl:param name="Italic"/>
		<xsl:param name="Underline"/>
		<xsl:param name="FontSize"/>
		<xsl:param name="Colour"/>
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
				<xsl:if test="$Style != ''">
					<xsl:element name="w:rStyle">
						<xsl:attribute name="w:val">
							<xsl:value-of select="$Style"/>
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
				<xsl:if test="$Colour != ''">
					<xsl:element name="w:color">
						<xsl:attribute name="w:val">
							<xsl:value-of select="$Colour"/>
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

	<xsl:template name="Hyperlink">
		<xsl:param name="Link"/>
		<xsl:param name="Text"/>
		<xsl:element name="w:p">
			<xsl:element name="w:hlink">
				<xsl:attribute name="w:dest">
					<xsl:value-of select="$Link"/>
				</xsl:attribute>
				<xsl:call-template name="Range">
					<xsl:with-param name="Text">
						<xsl:value-of select="$Text"/>
					</xsl:with-param>
					<xsl:with-param name="Style">Hyperlink</xsl:with-param>
					<xsl:with-param name="Underline">single</xsl:with-param>
					<xsl:with-param name="Colour">0000FF</xsl:with-param>
				</xsl:call-template>
			</xsl:element>
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
      <xsl:text> (</xsl:text>
      <xsl:value-of select="AlgoID"/>
      <xsl:text>)</xsl:text>
    </xsl:for-each>
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