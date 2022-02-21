<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:efn="e24:Functions"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
				xmlns:w="http://schemas.microsoft.com/office/word/2003/wordml"
>
	<xsl:param name="count"/>
	<xsl:output method="xml" indent="yes"/>
	<xsl:variable name="documents">
		<WordML Orientation="Portrait" />
	</xsl:variable>
	<xsl:variable name="addedColumns">|Algo_Name|Expert Statement|Lay Statement|Question|Answer|Bullet|</xsl:variable>

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
							<xsl:with-param name="Text">
								<xsl:text>Most Popular Question Answer Report (</xsl:text>
								<xsl:value-of select="$count"/>
								<xsl:text> or more instances)</xsl:text>
							</xsl:with-param>
							<xsl:with-param name="Align">center</xsl:with-param>
							<xsl:with-param name="Bold">1</xsl:with-param>
							<xsl:with-param name="Underline">single</xsl:with-param>
							<xsl:with-param name="FontSize">10</xsl:with-param>
						</xsl:call-template>
						<xsl:for-each select="Table1[not(QuestionID = preceding-sibling::Table1/QuestionID)]">
							<xsl:variable name="id" select="QuestionID"/>
							<xsl:call-template name="Question" />
							<xsl:call-template name="Answers">
								<xsl:with-param name="id" select="$id"/>
							</xsl:call-template>
						</xsl:for-each>
					</xsl:for-each>
				</xsl:element>
			</xsl:for-each>
			<xsl:for-each select="msxsl:node-set($documents)/Compare">
				<xsl:element name="Compare">
					<xsl:for-each select="@*">
						<xsl:attribute name="{name()}">
							<xsl:value-of select="."/>
						</xsl:attribute>
					</xsl:for-each>
				</xsl:element>
			</xsl:for-each>
		</xsl:element>
	</xsl:template>

	<xsl:template name="Question">
		<xsl:element name="w:p"/>
		<xsl:call-template name="Paragraph">
			<xsl:with-param name="FontSize">10</xsl:with-param>
			<xsl:with-param name="Bold">1</xsl:with-param>
			<xsl:with-param name="Text">
				<xsl:text>Question # </xsl:text>
				<xsl:value-of select="*[1]"/>
			</xsl:with-param>
		</xsl:call-template>
		<xsl:call-template name="Paragraph">
			<xsl:with-param name="FontSize">10</xsl:with-param>
			<xsl:with-param name="Italic">1</xsl:with-param>
			<xsl:with-param name="Text">
				<xsl:text>Expert Statement: </xsl:text>
				<xsl:value-of select="*[2]"/>
			</xsl:with-param>
		</xsl:call-template>
		<xsl:if test="*[3] != *[2]">
			<xsl:call-template name="Paragraph">
				<xsl:with-param name="FontSize">10</xsl:with-param>
				<xsl:with-param name="Italic">1</xsl:with-param>
				<xsl:with-param name="Text">
					<xsl:text>Lay Statement: </xsl:text>
					<xsl:value-of select="*[3]"/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
		<xsl:element name="w:p">
			<xsl:call-template name="Range">
				<xsl:with-param name="FontSize">10</xsl:with-param>
				<xsl:with-param name="Italic">1</xsl:with-param>
				<xsl:with-param name="Text">Question: </xsl:with-param>
			</xsl:call-template>
			<xsl:call-template name="Range">
				<xsl:with-param name="FontSize">10</xsl:with-param>
				<xsl:with-param name="Bold">1</xsl:with-param>
				<xsl:with-param name="Text">
					<xsl:value-of select="*[4]"/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:element>
		<xsl:call-template name="Paragraph">
			<xsl:with-param name="Text">
				<xsl:text>Question Type: </xsl:text>
				<xsl:value-of select="*[6]"/>
			</xsl:with-param>
		</xsl:call-template>
		<xsl:call-template name="Paragraph">
			<xsl:with-param name="Text">
				<xsl:text>History Type: </xsl:text>
				<xsl:value-of select="*[7]"/>
			</xsl:with-param>
		</xsl:call-template>
		<xsl:call-template name="Paragraph">
			<xsl:with-param name="Text">
				<xsl:text>Summary: </xsl:text>
				<xsl:value-of select="*[8]"/>
				<xsl:text>, </xsl:text>
				<xsl:value-of select="*[9]"/>
			</xsl:with-param>
		</xsl:call-template>
		<xsl:call-template name="Paragraph">
			<xsl:with-param name="Text">
				<xsl:text>Possible Answers:</xsl:text>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="Answers">
		<xsl:param name="id"/>
		<xsl:element name="w:tbl">
			<xsl:element name="w:tblPr">
				<xsl:element name="w:tblInd">
					<xsl:attribute name="w:w">720</xsl:attribute>
					<xsl:attribute name="w:type">dxa</xsl:attribute>
				</xsl:element>
				<w:tblBorders>
					<w:top w:val="single" w:sz="4" w:space="0" w:color="auto"/>
					<w:left w:val="single" w:sz="4" w:space="0" w:color="auto"/>
					<w:bottom w:val="single" w:sz="4" w:space="0" w:color="auto"/>
					<w:right w:val="single" w:sz="4" w:space="0" w:color="auto"/>
					<w:insideH w:val="single" w:sz="4" w:space="0" w:color="auto"/>
					<w:insideV w:val="single" w:sz="4" w:space="0" w:color="auto"/>
				</w:tblBorders>
			</xsl:element>
			<xsl:for-each select="../Table1[QuestionID = $id]">
				<xsl:element name="w:tr">
					<xsl:element name="w:tc">
						<xsl:element name="w:tcPr">
							<xsl:element name="w:tcW">
								<xsl:attribute name="w:w">8500</xsl:attribute>
								<xsl:attribute name="w:type">dxa</xsl:attribute>
							</xsl:element>
						</xsl:element>
						<xsl:call-template name="Answer"/>
					</xsl:element>
				</xsl:element>
			</xsl:for-each>
		</xsl:element>
	</xsl:template>

	<xsl:template name="Answer">
		<xsl:if test="*[10] != *[12]">
			<xsl:element name="w:p">
				<xsl:call-template name="Range">
					<xsl:with-param name="FontSize">10</xsl:with-param>
					<xsl:with-param name="Italic">1</xsl:with-param>
					<xsl:with-param name="Text">Expert Statement: </xsl:with-param>
				</xsl:call-template>
				<xsl:call-template name="Range">
					<xsl:with-param name="FontSize">10</xsl:with-param>
					<xsl:with-param name="Text">
						<xsl:value-of select="*[10]"/>
					</xsl:with-param>
				</xsl:call-template>
			</xsl:element>
		</xsl:if>
		<xsl:if test="*[11] != *[10]">
			<xsl:element name="w:p">
				<xsl:call-template name="Range">
					<xsl:with-param name="FontSize">10</xsl:with-param>
					<xsl:with-param name="Italic">1</xsl:with-param>
					<xsl:with-param name="Text">Lay Statement: </xsl:with-param>
				</xsl:call-template>
				<xsl:call-template name="Range">
					<xsl:with-param name="FontSize">10</xsl:with-param>
					<xsl:with-param name="Text">
						<xsl:value-of select="*[11]"/>
					</xsl:with-param>
				</xsl:call-template>
			</xsl:element>
		</xsl:if>
		<xsl:element name="w:p">
			<xsl:call-template name="Range">
				<xsl:with-param name="FontSize">10</xsl:with-param>
				<xsl:with-param name="Italic">1</xsl:with-param>
				<xsl:with-param name="Text">Answer: </xsl:with-param>
			</xsl:call-template>
			<xsl:call-template name="Range">
				<xsl:with-param name="FontSize">10</xsl:with-param>
				<xsl:with-param name="Bold">1</xsl:with-param>
				<xsl:with-param name="Text">
					<xsl:value-of select="*[12]"/>
				</xsl:with-param>
			</xsl:call-template>
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
