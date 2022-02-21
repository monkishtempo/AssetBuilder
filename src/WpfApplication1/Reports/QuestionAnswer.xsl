<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:efn="e24:Functions"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
	<xsl:output method="xml" indent="yes"/>
	<xsl:param name="excludedAlgoList" />
	<xsl:variable name="documents">
		<Document Orientation="Portrait" />
	</xsl:variable>
	<xsl:variable name="trans">
		<Replace Source="Expert_Statement" Target="Clinical_Statement"/>
		<Replace Source="Expert_Statement" Target="Possible_Condition"/>
		<Replace Source="Expert_Statement" Target="Clinical_Answer"/>
		<Replace Source="Bullet" Target="BP_TEXT"/>
	</xsl:variable>
	<xsl:variable name="addedColumns">|Algo_Name|Expert Statement|Lay Statement|Question|Answer|Bullet|</xsl:variable>

	<xsl:template match="/*">
		<xsl:element name="Report">
			<xsl:variable name="doc" select="." />
			<xsl:for-each select="msxsl:node-set($documents)/Document">
				<xsl:element name="Document">
					<xsl:for-each select="@*">
						<xsl:attribute name="{name()}">
							<xsl:value-of select="."/>
						</xsl:attribute>
					</xsl:for-each>
					<xsl:variable name="num" select="position()"/>
					<xsl:for-each select="$doc">
						<xsl:element name="Content">
							<xsl:attribute name="FontSize">14</xsl:attribute>
							<xsl:attribute name="FontWeight">Bold</xsl:attribute>
							<xsl:attribute name="Align">Centre</xsl:attribute>
							<xsl:text>Question Answer Report&#10;</xsl:text>
							<xsl:call-template name="algo-names" />
						</xsl:element>
						<xsl:if test="$excludedAlgoList">
							<xsl:element name="Content">
								<xsl:attribute name="FontSize">10</xsl:attribute>
								<xsl:attribute name="Align">Centre</xsl:attribute>
								<xsl:text> Excluding </xsl:text>
								<xsl:value-of select="$excludedAlgoList"/>
							</xsl:element>
						</xsl:if>
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
		<xsl:element name="Content">
			<xsl:attribute name="FontSize">10</xsl:attribute>
			<xsl:attribute name="FontWeight">Bold</xsl:attribute>
			<xsl:text>Question # </xsl:text>
			<xsl:value-of select="*[1]"/>
		</xsl:element>
		<xsl:element name="Content">
			<xsl:attribute name="FontSize">10</xsl:attribute>
			<xsl:attribute name="FontStyle">Italic</xsl:attribute>
			<xsl:text>Expert Statement: </xsl:text>
			<xsl:choose>
				<xsl:when test="root[1]/*[1] != ''">
					<xsl:value-of select="root[1]/*[1]"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="*[2]"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:element>
		<xsl:if test="*[3] != *[2] or root[1]/*[1] != root[1]/*[2]">
			<xsl:element name="Content">
				<xsl:attribute name="FontSize">10</xsl:attribute>
				<xsl:attribute name="FontStyle">Italic</xsl:attribute>
				<xsl:text>Lay Statement: </xsl:text>
				<xsl:choose>
					<xsl:when test="root[1]/*[2] != ''">
						<xsl:value-of select="root[1]/*[2]"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="*[3]"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:element>
		</xsl:if>
		<xsl:element name="Content">
			<xsl:attribute name="FontSize">10</xsl:attribute>
			<xsl:attribute name="FontStyle">Italic</xsl:attribute>
			<xsl:attribute name="Break">0</xsl:attribute>
			<xsl:text>Question: </xsl:text>
		</xsl:element>
		<xsl:element name="Content">
			<xsl:attribute name="FontSize">10</xsl:attribute>
			<xsl:attribute name="FontWeight">Bold</xsl:attribute>
			<xsl:choose>
				<xsl:when test="root[1]/*[3] != ''">
					<xsl:value-of select="root[1]/*[3]"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="*[4]"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:element>
		<xsl:element name="Content">
			<xsl:attribute name="FontSize">10</xsl:attribute>
			<xsl:attribute name="FontStyle">Italic</xsl:attribute>
			<xsl:attribute name="Break">0</xsl:attribute>
			<xsl:text>More Detail: </xsl:text>
		</xsl:element>
		<xsl:element name="Content">
			<xsl:attribute name="FontSize">10</xsl:attribute>
			<xsl:attribute name="FontWeight">Bold</xsl:attribute>
			<xsl:choose>
				<xsl:when test="root[1]/*[4] != ''">
					<xsl:value-of select="root[1]/*[4]"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="*[5]"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:element>
		<xsl:element name="Content">
			<xsl:attribute name="FontSize">10</xsl:attribute>
			<xsl:text>Question Type: </xsl:text>
			<xsl:value-of select="*[6]"/>
		</xsl:element>
		<xsl:element name="Content">
			<xsl:attribute name="FontSize">10</xsl:attribute>
			<xsl:text>History Type: </xsl:text>
			<xsl:value-of select="*[7]"/>
		</xsl:element>
		<xsl:element name="Content">
			<xsl:attribute name="FontSize">10</xsl:attribute>
			<xsl:text>Summary: </xsl:text>
			<xsl:value-of select="*[8]"/>
			<xsl:text>, </xsl:text>
			<xsl:value-of select="*[9]"/>
		</xsl:element>
		<xsl:element name="Content">
			<xsl:attribute name="FontSize">10</xsl:attribute>
			<xsl:text>Possible Answers:</xsl:text>
		</xsl:element>
	</xsl:template>

	<xsl:template name="Answers">
		<xsl:param name="id"/>
		<xsl:element name="Table">
			<xsl:attribute name="Indent">30</xsl:attribute>
			<xsl:for-each select="../Table1[QuestionID = $id]">
				<xsl:element name="Row">
					<xsl:if test="AnswerID">
						<xsl:element name="Cell">
							<xsl:if test="position()=1">
								<xsl:attribute name="Width">0.1</xsl:attribute>
							</xsl:if>
							<xsl:element name="Content">
								<xsl:attribute name="FontSize">10</xsl:attribute>
								<xsl:attribute name="FontStyle">Italic</xsl:attribute>
								<xsl:value-of select="AnswerID"/>
							</xsl:element>
						</xsl:element>
					</xsl:if>
					<xsl:element name="Cell">
						<xsl:if test="position()=1">
							<xsl:attribute name="Width">0.7</xsl:attribute>
						</xsl:if>
						<xsl:call-template name="Answer"/>
					</xsl:element>
				</xsl:element>
			</xsl:for-each>
		</xsl:element>
	</xsl:template>

	<xsl:template name="Answer">
		<xsl:if test="*[10] != *[12] or root[2]/*[1] != root[2]/*[3]">
			<xsl:element name="Content">
				<xsl:attribute name="FontSize">10</xsl:attribute>
				<xsl:attribute name="FontStyle">Italic</xsl:attribute>
				<xsl:text>Expert Statement: </xsl:text>
				<xsl:choose>
					<xsl:when test="root[2]/*[1] != ''">
						<xsl:value-of select="root[2]/*[1]"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="*[10]"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:element>
		</xsl:if>
		<xsl:if test="*[11] != *[10] or root[2]/*[2] != root[2]/*[1]">
			<xsl:element name="Content">
				<xsl:attribute name="FontSize">10</xsl:attribute>
				<xsl:attribute name="FontStyle">Italic</xsl:attribute>
				<xsl:text>Lay Statement: </xsl:text>
				<xsl:choose>
					<xsl:when test="root[2]/*[2] != ''">
						<xsl:value-of select="root[2]/*[2]"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="*[11]"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:element>
		</xsl:if>
		<xsl:element name="Content">
			<xsl:attribute name="FontSize">10</xsl:attribute>
			<xsl:attribute name="FontStyle">Italic</xsl:attribute>
			<xsl:attribute name="Break">0</xsl:attribute>
			<xsl:text>Answer: </xsl:text>
		</xsl:element>
		<xsl:element name="Content">
			<xsl:attribute name="FontSize">10</xsl:attribute>
			<xsl:attribute name="FontWeight">Bold</xsl:attribute>
			<xsl:attribute name="Break">0</xsl:attribute>
			<xsl:choose>
				<xsl:when test="root[2]/*[3] != ''">
					<xsl:value-of select="root[2]/*[3]"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="*[12]"/>
				</xsl:otherwise>
			</xsl:choose>
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
