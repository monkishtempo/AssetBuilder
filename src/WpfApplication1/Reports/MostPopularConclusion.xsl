<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
				xmlns:w="http://schemas.microsoft.com/office/word/2003/wordml"
>
	<xsl:param name="count"/>
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
							<xsl:with-param name="Text">
								<xsl:text>Most Popular Conclusion Report (</xsl:text>
								<xsl:value-of select="$count"/>
								<xsl:text> or more instances)</xsl:text>
							</xsl:with-param>
							<xsl:with-param name="Align">center</xsl:with-param>
							<xsl:with-param name="Bold">1</xsl:with-param>
							<xsl:with-param name="Underline">single</xsl:with-param>
							<xsl:with-param name="FontSize">10</xsl:with-param>
						</xsl:call-template>
						<xsl:call-template name="Conclusions"/>
					</xsl:for-each>
				</xsl:element>
			</xsl:for-each>
		</xsl:element>
	</xsl:template>

	<xsl:template name="Conclusions">
		<xsl:for-each select="Table1">
			<xsl:element name="w:p"/>
			<xsl:call-template name="Paragraph">
				<xsl:with-param name="Text">
					<xsl:text>Conclusion # </xsl:text>
					<xsl:value-of select="*[1]"/>
					<xsl:text> Ages : </xsl:text>
					<xsl:value-of select="Ages"/>
				</xsl:with-param>
				<xsl:with-param name="Bold">1</xsl:with-param>
			</xsl:call-template>
			<xsl:for-each select="*[position()>1 and position() &lt; 6][. != '' and . != preceding-sibling::*[1]]">
				<xsl:variable name="name">
					<xsl:call-template name="replace">
						<xsl:with-param name="text" select="name()"/>
						<xsl:with-param name="find">_x0020_</xsl:with-param>
						<xsl:with-param name="replace">
							<xsl:text> </xsl:text>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:variable>
				<xsl:element name="w:p">
					<xsl:call-template name="Range">
						<xsl:with-param name="Bold">1</xsl:with-param>
						<xsl:with-param name="Text">
							<xsl:value-of select="$name"/>
							<xsl:text>: </xsl:text>
						</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="Range">
						<xsl:with-param name="Text">
							<xsl:choose>
								<xsl:when test="contains(., $cr)">
									<xsl:value-of select="substring-before(., $cr)"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="."/>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:element>
				<xsl:if test="contains(., $cr)">
					<xsl:call-template name="OtherParagraphs">
						<xsl:with-param name="Text">
							<xsl:value-of select="substring-after(., $cr)"/>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:if>
			</xsl:for-each>
			<xsl:call-template name="CarePoints"/>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="CarePoints">
		<xsl:if test="../Table2[*[1] = current()/*[1]]">
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
				<xsl:for-each select="../Table2[*[1] = current()/*[1]]">
					<xsl:element name="w:tr">
						<xsl:element name="w:tc">
							<xsl:call-template name="Paragraph">
								<xsl:with-param name="Text">
									<xsl:value-of select="BPID"/>
								</xsl:with-param>
							</xsl:call-template>
						</xsl:element>
						<xsl:element name="w:tc">
							<xsl:call-template name="Paragraph">
								<xsl:with-param name="Text">
									<xsl:value-of select="BULLET"/>
								</xsl:with-param>
							</xsl:call-template>
						</xsl:element>
					</xsl:element>
				</xsl:for-each>
			</xsl:element>
		</xsl:if>
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

	<xsl:template name="OtherParagraphs">
		<xsl:param name="Text"/>
		<xsl:param name="Bold"/>
		<xsl:param name="Italic"/>
		<xsl:param name="Underline"/>
		<xsl:param name="FontSize"/>
		<xsl:param name="Align"/>
		<xsl:param name="Border"/>
		<xsl:param name="Indent"/>
		<xsl:choose>
			<xsl:when test="contains($Text, $cr)">
				<xsl:variable name="before" select="substring-before($Text, $cr)"/>
				<xsl:variable name="after" select="substring-after($Text, $cr)"/>
				<xsl:call-template name="Paragraph">
					<xsl:with-param name="Text" select="$before"/>
					<xsl:with-param name="Bold" select="$Bold"/>
					<xsl:with-param name="Italic" select="$Italic"/>
					<xsl:with-param name="Underline" select="$Underline"/>
					<xsl:with-param name="FontSize" select="$FontSize"/>
					<xsl:with-param name="Align" select="$Align"/>
					<xsl:with-param name="Border" select="$Border"/>
					<xsl:with-param name="Indent" select="$Indent"/>
				</xsl:call-template>
				<xsl:call-template name="OtherParagraphs">
					<xsl:with-param name="Text" select="$after"/>
					<xsl:with-param name="Bold" select="$Bold"/>
					<xsl:with-param name="Italic" select="$Italic"/>
					<xsl:with-param name="Underline" select="$Underline"/>
					<xsl:with-param name="FontSize" select="$FontSize"/>
					<xsl:with-param name="Align" select="$Align"/>
					<xsl:with-param name="Border" select="$Border"/>
					<xsl:with-param name="Indent" select="$Indent"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="Paragraph">
					<xsl:with-param name="Text" select="$Text"/>
					<xsl:with-param name="Bold" select="$Bold"/>
					<xsl:with-param name="Italic" select="$Italic"/>
					<xsl:with-param name="Underline" select="$Underline"/>
					<xsl:with-param name="FontSize" select="$FontSize"/>
					<xsl:with-param name="Align" select="$Align"/>
					<xsl:with-param name="Border" select="$Border"/>
					<xsl:with-param name="Indent" select="$Indent"/>
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
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