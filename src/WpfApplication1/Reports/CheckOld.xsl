<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:efn="e24:Functions"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
	<xsl:output method="xml" indent="yes"/>
	<xsl:param name="excludedAlgoList" />
	<xsl:param name="year" />
	<xsl:param name="date" />
	<xsl:param name="assets" />
	<xsl:param name="commentType" />
	<xsl:variable name="documents">
		<Document Orientation="Landscape" LeftMargin="30"/>
		<Document Orientation="Landscape" LeftMargin="30"/>
		<Compare Original="2" Revised="1" />
	</xsl:variable>
	<xsl:variable name="addedColumns">|Algo_Name|Expert Statement|Lay Statement|Question|Answer|Bullet|Explanation|More Detail|</xsl:variable>
	<xsl:variable name="libraryColumns">|AGE_TEXT|Data Set|Category|Sub Category 1|Sub Category 2|</xsl:variable>
	<xsl:variable name="detailColumns">|Word_Merge|Question Type|Answer Type|Information|WM2|State|Store if negative|Minimum value|Maximum value|Multiplier|Silent|</xsl:variable>

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
						<xsl:call-template name="firstPage" />
						<xsl:call-template name="compare">
							<xsl:with-param name="current" select="Table"/>
							<xsl:with-param name="old" select="Table1"/>
							<xsl:with-param name="title">ALGOS</xsl:with-param>
							<xsl:with-param name="doc" select="$num" />
						</xsl:call-template>
						<xsl:call-template name="compare">
							<xsl:with-param name="current" select="Table2"/>
							<xsl:with-param name="old" select="Table3"/>
							<xsl:with-param name="title">QUESTIONS</xsl:with-param>
							<xsl:with-param name="doc" select="$num" />
						</xsl:call-template>
						<xsl:call-template name="compare">
							<xsl:with-param name="current" select="Table4"/>
							<xsl:with-param name="old" select="Table5"/>
							<xsl:with-param name="title">ANSWERS</xsl:with-param>
							<xsl:with-param name="doc" select="$num" />
						</xsl:call-template>
						<xsl:call-template name="compare">
							<xsl:with-param name="current" select="Table6"/>
							<xsl:with-param name="old" select="Table7"/>
							<xsl:with-param name="title">CONCLUSIONS</xsl:with-param>
							<xsl:with-param name="doc" select="$num" />
						</xsl:call-template>
						<xsl:call-template name="compare">
							<xsl:with-param name="current" select="Table8"/>
							<xsl:with-param name="old" select="Table9"/>
							<xsl:with-param name="title">BULLETS</xsl:with-param>
							<xsl:with-param name="doc" select="$num" />
						</xsl:call-template>
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

	<xsl:template name="compare">
		<xsl:param name="current" />
		<xsl:param name="old" />
		<xsl:param name="title" />
		<xsl:param name="doc" />
		<LineBreak />
		<xsl:element name="Content">
			<xsl:value-of select="$title"/>
		</xsl:element>
		<!--<xsl:if test="$current[*[1] = $old/*[1]]">-->
			<xsl:element name="Table">
				<xsl:for-each select="$current[*[1] = $old/*[1] and efn:ContainsAny(ReportComments, $commentType)]">
					<xsl:call-template name="row">
						<xsl:with-param name="doc" select="$doc"/>
						<xsl:with-param name="old" select="$old"/>
						<xsl:with-param name="rownum" select="position()"/>
						<xsl:with-param name="heading">Updated</xsl:with-param>
					</xsl:call-template>
				</xsl:for-each>
				<xsl:for-each select="$current[not(*[1] = $old/*[1]) and efn:newer(datetimestamp, $date) and efn:ContainsAny(ReportComments, $commentType)]">
					<xsl:call-template name="row">
						<xsl:with-param name="doc" select="$doc"/>
						<xsl:with-param name="old" select="$old"/>
						<xsl:with-param name="rownum" select="position()+count($current[*[1] = $old/*[1]])"/>
						<xsl:with-param name="heading">Added</xsl:with-param>
					</xsl:call-template>
				</xsl:for-each>
			</xsl:element>
		<!--</xsl:if>-->
	</xsl:template>

	<xsl:template name="row">
		<xsl:param name="doc" />
		<xsl:param name="old" />
		<xsl:param name="rownum" />
		<xsl:param name="heading" />
		<xsl:variable name="count" select="count(datetimestamp/preceding-sibling::*)+1" />
		<xsl:variable name="id" select="*[1]" />
		<xsl:variable name="previous" select="$old[*[1] = $id]" />
		<xsl:element name="Row">
			<xsl:element name="Cell">
				<xsl:if test="$rownum=1">
					<xsl:attribute name="Width">0.2</xsl:attribute>
				</xsl:if>
				<xsl:element name="Content">
					<xsl:value-of select="ReportComments"/>
				</xsl:element>
			</xsl:element>
			<xsl:element name="Cell">
				<xsl:if test="$rownum=1">
					<xsl:attribute name="Width">0.6</xsl:attribute>
				</xsl:if>
				<xsl:element name="Content">
					<xsl:attribute name="FontWeight">Bold</xsl:attribute>
					<xsl:value-of select="$heading"/>
					<xsl:text> </xsl:text>
					<xsl:value-of select="name(*)"/>
					<xsl:text> </xsl:text>
					<xsl:value-of select="*"/>
					<xsl:text> </xsl:text>
					<xsl:value-of select="*[2]"/>
				</xsl:element>
				<xsl:if test="$heading = 'Added'">
					<xsl:for-each select="*[position() &lt; $count]">
						<xsl:variable name="name">
							<xsl:call-template name="replace">
								<xsl:with-param name="text" select="name()"/>
								<xsl:with-param name="find">_x0020_</xsl:with-param>
								<xsl:with-param name="replace">
									<xsl:text> </xsl:text>
								</xsl:with-param>
							</xsl:call-template>
						</xsl:variable>
						<xsl:if test="contains($libraryColumns, concat('|', $name, '|'))">
							<xsl:if test="not(contains($name,'Category'))">
								<xsl:element name="Content">
									<xsl:attribute name="FontWeight">Bold</xsl:attribute>
									<xsl:attribute name="Break">0</xsl:attribute>
									<xsl:text>Library: </xsl:text>
								</xsl:element>
							</xsl:if>
							<xsl:element name="Content">
								<xsl:attribute name="Break">0</xsl:attribute>
								<xsl:if test="contains($name,'Category')">
									<xsl:text>, </xsl:text>
								</xsl:if>
								<xsl:value-of select="."/>
							</xsl:element>
						</xsl:if>
						<xsl:if test="contains($detailColumns, concat('|', $name, '|'))">
							<xsl:variable name="sp" select="string-length(substring-before($detailColumns, concat('|', $name, '|')))" />
							<xsl:if test="$sp &lt; 40">
								<xsl:element name="Content">
									<xsl:text>.</xsl:text>
								</xsl:element>
								<xsl:element name="Content">
									<xsl:attribute name="FontWeight">Bold</xsl:attribute>
									<xsl:attribute name="Break">0</xsl:attribute>
									<xsl:text>Detail: </xsl:text>
								</xsl:element>
							</xsl:if>
							<xsl:element name="Content">
								<xsl:attribute name="Break">0</xsl:attribute>
								<xsl:if test="$sp > 40">
									<xsl:text>, </xsl:text>
								</xsl:if>
								<xsl:choose>
									<xsl:when test=".='true'">
										<xsl:value-of select="$name"/>
									</xsl:when>
									<xsl:when test=".='false'">
										<xsl:text>not </xsl:text>
										<xsl:value-of select="$name"/>
									</xsl:when>
									<xsl:when test="$name='Multiplier'">
										<xsl:choose>
											<xsl:when test=".='1'">
												<xsl:text>Normal/Days</xsl:text>
											</xsl:when>
											<xsl:when test=".='7'">
												<xsl:text>Weeks</xsl:text>
											</xsl:when>
											<xsl:when test=".='30.4375'">
												<xsl:text>Months</xsl:text>
											</xsl:when>
											<xsl:when test=".='365.25'">
												<xsl:text>Years</xsl:text>
											</xsl:when>
										</xsl:choose>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="."/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:element>
						</xsl:if>
					</xsl:for-each>
					<xsl:element name="Content">
						<xsl:text>.</xsl:text>
					</xsl:element>
				</xsl:if>
				<xsl:for-each select="*[position() &lt; $count]">
					<xsl:variable name="pos" select="position()"/>
					<xsl:variable name="name">
						<xsl:call-template name="replace">
							<xsl:with-param name="text" select="name()"/>
							<xsl:with-param name="find">_x0020_</xsl:with-param>
							<xsl:with-param name="replace">
								<xsl:text> </xsl:text>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:variable>
					<xsl:if test="$previous/*[$pos] != . or ($heading = 'Added' and contains($addedColumns, concat('|', $name, '|')) and . != '')">
						<xsl:call-template name="field">
							<xsl:with-param name="doc" select="$doc" />
							<xsl:with-param name="previous" select="$previous" />
							<xsl:with-param name="pos" select="$pos" />
							<xsl:with-param name="name" select="$name" />
						</xsl:call-template>
					</xsl:if>
				</xsl:for-each>
			</xsl:element>
			<xsl:element name="Cell">
				<xsl:if test="$rownum=1">
					<xsl:attribute name="Width">0.2</xsl:attribute>
				</xsl:if>
				<xsl:element name="Content">
					<xsl:text>__Agree&#10;__Disagree (please give reason and suggestion for change)</xsl:text>
				</xsl:element>
			</xsl:element>
		</xsl:element>
	</xsl:template>

	<xsl:template name="field">
		<xsl:param name="doc" />
		<xsl:param name="previous" />
		<xsl:param name="pos" />
		<xsl:param name="name" />
		<xsl:element name="Content">
			<xsl:attribute name="FontWeight">Bold</xsl:attribute>
			<xsl:attribute name="Break">0</xsl:attribute>
			<xsl:value-of select="$name"/>
			<xsl:text>: </xsl:text>
		</xsl:element>
		<xsl:element name="Content">
			<xsl:choose>
				<xsl:when test="$doc=1">
					<xsl:value-of select="."/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$previous/*[$pos]"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:element>
	</xsl:template>

	<xsl:template name="firstPage">
		<xsl:variable name="break">
			<xsl:choose>
				<xsl:when test="$excludedAlgoList">1</xsl:when>
				<xsl:otherwise>2</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:element name="Content">
			<xsl:attribute name="FontWeight">Bold</xsl:attribute>
			<xsl:attribute name="FontSize">14</xsl:attribute>
			<xsl:attribute name="Align">Centre</xsl:attribute>
			<xsl:attribute name="Break">
				<xsl:value-of select="$break"/>
			</xsl:attribute>
			<xsl:value-of select="$year"/>
			<xsl:choose>
				<xsl:when test="$assets">
					<xsl:text> REVIEW&#10;ASSETS: </xsl:text>
					<xsl:value-of select="$assets"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text> REVIEW&#10;ALGORITHM NAME: </xsl:text>
					<xsl:call-template name="algo-names"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:element>
		<xsl:if test="$excludedAlgoList">
			<xsl:element name="Content">
				<xsl:attribute name="Align">Centre</xsl:attribute>
				<xsl:attribute name="FontSize">10</xsl:attribute>
				<xsl:attribute name="Break">1</xsl:attribute>
				<xsl:text> Excluding </xsl:text>
				<xsl:value-of select="$excludedAlgoList"/>
			</xsl:element>
		</xsl:if>
		<xsl:element name="Content">
			<xsl:attribute name="FontWeight">Bold</xsl:attribute>
			<xsl:attribute name="FontSize">14</xsl:attribute>
			<xsl:attribute name="Break">1</xsl:attribute>
			<xsl:text>Date of literature review:&#10;Date of draft medical review and changes:&#10;Date of revisions based on reviews:&#10;Reviewer:&#10;Revisions completed by:&#10;Report generated by:</xsl:text>
		</xsl:element>
		<Content FontWeight="Bold" FontSize="12" Break="0">Review hints: </Content>
		<Content FontSize="12" Break="2">By using the "Display for review" field on your toolbar (probably currently showing "Final Showing Markup") you can look at the text in its original form ("Original"), its changed form ("Final"), the original form with insertions and deletions ("Original Showing Markup") or in the final form with insertions and deletions ("Final Showing Markup").</Content>
		<Content FontSize="12" FontWeight="Bold" FontStyle="Italic" Break="2">To track your own changes automatically, turn on "Track Changes" under the Tools menu option on your toolbar.</Content>
		<Content FontSize="12" FontWeight="Bold" Break="2">Summary of review: </Content>
		<Content FontSize="12">Please let us know if there are any of the following: </Content>
		<BulletList FontSize="12">
			<Content>references and their implications that you are aware of that have not been included</Content>
			<Content>changes you feel need to be made to the logic after you run test scenarios on the doctor's staging site</Content>
			<Content>changes to any of the conclusion text</Content>
			<Content>other problems that you come across</Content>
		</BulletList>
		<PageBreak />
		<Table Rows="2" Columns="3">
			<Row>
				<Cell Width="0.2">
					<Content FontWeight="Bold" FontSize="14" Break="0">Reason for change or addition</Content>
				</Cell>
				<Cell Width="0.6">
					<Content FontWeight="Bold" FontSize="14" Break="0">Changes made</Content>
				</Cell>
				<Cell Width="0.2">
					<Content FontWeight="Bold" FontSize="14" Break="0">Agree or Disagree</Content>
				</Cell>
			</Row>
			<Row>
				<Cell>
					<Content Break="0">Evaluation of user data</Content>
				</Cell>
			</Row>
		</Table>
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
