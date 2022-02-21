<?xml version="1.0"?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:param name="html">yes</xsl:param>
	
	<xsl:template match="/">
		<DIV STYLE="font-family:Courier; font-size:10pt; margin-bottom:2em">
			<xsl:apply-templates/>
		</DIV>
	</xsl:template>

	<xsl:template match="*">
		<DIV STYLE="margin-left:1em; color:red">
			&lt;<xsl:value-of select="name()"/>/&gt;
		</DIV>
	</xsl:template>

	<xsl:template match="node()">
		<DIV STYLE="margin-left:1em">
			<SPAN STYLE="color:blue">
				<xsl:text>&lt;</xsl:text>
			</SPAN>
			<SPAN STYLE="color:maroon">
				<xsl:value-of select="name()"/>
				<xsl:apply-templates select="@*"/>
			</SPAN>
			<xsl:choose>
				<xsl:when test="*|text()|comment()|processing-instruction()">
					<SPAN STYLE="color:blue">
						<xsl:text>&gt;</xsl:text>
					</SPAN>
					<xsl:apply-templates select="node()"/>
					<SPAN STYLE="color:blue">
						<xsl:text>&lt;</xsl:text>
					</SPAN>
					<SPAN STYLE="color:maroon">
						<xsl:text>/</xsl:text>
						<xsl:value-of select="name()"/>
					</SPAN>
					<SPAN STYLE="color:blue">
						<xsl:text>&gt;</xsl:text>
					</SPAN>
				</xsl:when>
				<xsl:otherwise>
					<SPAN STYLE="color:blue">
						<xsl:text> /&gt;</xsl:text>
					</SPAN>
				</xsl:otherwise>
			</xsl:choose>
		</DIV>
	</xsl:template>

	<xsl:template match="@*">
		<SPAN STYLE="color:red">
			<xsl:text> </xsl:text>
			<xsl:value-of select="name()" />
		</SPAN>
		<SPAN STYLE="color:blue">
			<xsl:text>="</xsl:text>
		</SPAN>
		<SPAN STYLE="color:blue;font-weight:bold;">
			<xsl:value-of select="."/>
		</SPAN>
		<SPAN STYLE="color:blue">
			<xsl:text>"</xsl:text>
		</SPAN>
	</xsl:template>

	<xsl:template match="processing-instruction()">
		<DIV STYLE="margin-left:1em; color:maroon">
			&lt;?<xsl:value-of select="name()"/><xsl:apply-templates select="@*"/>?&gt;
		</DIV>
	</xsl:template>

	<xsl:template match="comment()">
		<div style="margin:0px 0px 0px 1em;width:100%;color:red;">
			<xsl:text>&lt;![CDATA[</xsl:text>
			<xsl:call-template name="replace-breaks">
				<xsl:with-param name="text" select="." />
			</xsl:call-template>
			<xsl:text>]]&gt;</xsl:text>
		</div>
	</xsl:template>

	<xsl:template match="text()">
		<xsl:choose>
			<xsl:when test="contains(., '&#xA;')">
				<div style="margin:0px 0px 0px 0px;width:100%;">
					<xsl:call-template name="replace-breaks">
						<xsl:with-param name="text" select="." />
					</xsl:call-template>
				</div>
			</xsl:when>
			<xsl:otherwise>
				<span>
					<xsl:call-template name="ouput">
						<xsl:with-param name="text" select="." />
					</xsl:call-template>
				</span>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="replace-breaks">
		<xsl:param name="text"/>
		<xsl:choose>
			<xsl:when test="contains($text, '&#xA;')">
				<xsl:variable name="before" select="substring-before($text, '&#xA;')"/>
				<xsl:variable name="after" select="substring-after($text, '&#xA;')"/>
				<xsl:call-template name="ouput">
					<xsl:with-param name="text" select="$before" />
				</xsl:call-template>
				<xsl:if test="string-length($after)>4">
					<br/>
					<xsl:call-template name="replace-breaks">
						<xsl:with-param name="text" select="$after"/>
					</xsl:call-template>
				</xsl:if>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="ouput">
					<xsl:with-param name="text" select="$text" />
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="ouput">
		<xsl:param name="text"/>
		<xsl:choose>
			<xsl:when test="$html='yes'">
				<xsl:value-of select="$text" disable-output-escaping="yes"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$text"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>
