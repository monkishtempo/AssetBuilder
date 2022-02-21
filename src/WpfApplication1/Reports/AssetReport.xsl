<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:efn="e24:Functions"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
				xmlns:w="http://schemas.microsoft.com/office/word/2003/wordml"
>
	<xsl:output method="xml" indent="yes"/>
	<xsl:param name="assets" />
	<xsl:param name="currentDate" />
	<xsl:param name="addedColumns">|0Title|1Algo_Name|1Word_Merge|1WM2|2Expert Statement|2Lay Statement|2Question|2Explanation|3Expert Statement|3Lay Statement|3Answer|3Explanation|4Expert Statement|4Lay Statement|4Explanation|4More Detail|4Bullets|5Bullet|</xsl:param>
	<xsl:param name="libraryColumns">|AGE_TEXT|Data Set|Category|Sub Category 1|Sub Category 2|</xsl:param>
	<xsl:param name="detailColumns">|Question Type|Answer Type|Information|State|Store if negative|Minimum value|Maximum value|Multiplier|Silent|</xsl:param>
	<xsl:param name="merge" />
	<xsl:variable name="imerge">
		<xsl:choose>
			<xsl:when test="contains($merge, 'Merge')">Merge</xsl:when>
			<xsl:otherwise></xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<xsl:variable name="isplit">
		<xsl:choose>
			<xsl:when test="contains($merge, 'Split')">Split</xsl:when>
			<xsl:otherwise></xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<xsl:variable name="documents">
		<WordML Orientation="Portrait" />
	</xsl:variable>
	<xsl:variable name="fill">
		<Colour Parent="Table1" Source="Expert_Statement" Color="D9D9D9" />
		<Colour Parent="Table1" Condition="Question_x0020_Type" Value="User/Language Check" Color="D9D9D9" />
		<Colour Parent="Table1" Condition="Question_x0020_Type" Value="Counter Check" Color="D9D9D9" />
		<Colour Parent="Table1" Condition="Question_x0020_Type" Value="Counter Set" Color="D9D9D9" />
		<Colour Parent="Table1" Condition="Question_x0020_Type" Value="Conclusion Check" Color="D9D9D9" />
		<Colour Parent="Table1" Condition="Question_x0020_Type" Value="Calculated Question Check" Color="D9D9D9" />
		<Colour Parent="Table2" Source="Expert_Statement" Color="D9D9D9" />
		<Colour Parent="Table2" Condition="Answer_x0020_Type" Value="Value: Sort &lt; &gt; =" Color="D9D9D9" />
		<Colour Parent="Table2" Condition="Answer_x0020_Type" Value="Derived from conclusion" Color="D9D9D9" />
		<Colour Parent="Table2" Condition="Answer_x0020_Type" Value="Derived from question" Color="D9D9D9" />
		<Colour Parent="Table2" Condition="Answer_x0020_Type" Value="Value: Calculated" Color="D9D9D9" />
		<Colour Parent="Table2" Condition="Answer_x0020_Type" Value="Age" Color="D9D9D9" />
		<Colour Parent="Table3" Source="Expert_Statement" Color="D9D9D9" />
	</xsl:variable>
	<xsl:variable name="colour">
		<Colour Parent="Table" Source="Algo_Name" Color="FF0000" />
		<Colour Parent="Table" Source="Word_Merge" Color="FF0000" />
		<Colour Parent="Table" Source="WM2" Color="FF0000" />
		<Colour Parent="Table1" Source="Lay_Statement" Condition="Question_x0020_Type" Value="1 Answer" Color="FF0000" />
		<Colour Parent="Table1" Source="Lay_Statement" Condition="Question_x0020_Type" Value="&gt;1 Answer" Color="FF0000" />
		<Colour Parent="Table1" Source="Lay_Statement" Condition="Question_x0020_Type" Value="User Entry" Color="FF0000" />
		<Colour Parent="Table1" Source="Lay_Statement" Condition="Question_x0020_Type" Value="Table: Non-exclusive" Color="FF0000" />
		<Colour Parent="Table1" Source="Lay_Statement" Condition="Question_x0020_Type" Value="Table: Exclusive" Color="FF0000" />
		<Colour Parent="Table1" Source="Lay_Statement" Condition="Question_x0020_Type" Value="Free Text" Color="FF0000" />
		<Colour Parent="Table1" Source="Lay_Statement" Condition="Question_x0020_Type" Value="Gender Check" Color="FF0000" />
		<Colour Parent="Table1" Source="Lay_Statement" Condition="Question_x0020_Type" Value="Image Question" Color="FF0000" />
		<Colour Parent="Table1" Source="Question" Condition="Question_x0020_Type" Value="1 Answer" Color="FF0000" />
		<Colour Parent="Table1" Source="Question" Condition="Question_x0020_Type" Value="&gt;1 Answer" Color="FF0000" />
		<Colour Parent="Table1" Source="Question" Condition="Question_x0020_Type" Value="User Entry" Color="FF0000" />
		<Colour Parent="Table1" Source="Question" Condition="Question_x0020_Type" Value="Table: Non-exclusive" Color="FF0000" />
		<Colour Parent="Table1" Source="Question" Condition="Question_x0020_Type" Value="Table: Exclusive" Color="FF0000" />
		<Colour Parent="Table1" Source="Question" Condition="Question_x0020_Type" Value="Free Text" Color="FF0000" />
		<Colour Parent="Table1" Source="Question" Condition="Question_x0020_Type" Value="Gender Check" Color="FF0000" />
		<Colour Parent="Table1" Source="Question" Condition="Question_x0020_Type" Value="Image Question" Color="FF0000" />
		<Colour Parent="Table1" Source="Explanation" Condition="Question_x0020_Type" Value="1 Answer" Color="FF0000" />
		<Colour Parent="Table1" Source="Explanation" Condition="Question_x0020_Type" Value="&gt;1 Answer" Color="FF0000" />
		<Colour Parent="Table1" Source="Explanation" Condition="Question_x0020_Type" Value="User Entry" Color="FF0000" />
		<Colour Parent="Table1" Source="Explanation" Condition="Question_x0020_Type" Value="Table: Non-exclusive" Color="FF0000" />
		<Colour Parent="Table1" Source="Explanation" Condition="Question_x0020_Type" Value="Table: Exclusive" Color="FF0000" />
		<Colour Parent="Table1" Source="Explanation" Condition="Question_x0020_Type" Value="Free Text" Color="FF0000" />
		<Colour Parent="Table1" Source="Explanation" Condition="Question_x0020_Type" Value="Gender Check" Color="FF0000" />
		<Colour Parent="Table1" Source="Explanation" Condition="Question_x0020_Type" Value="Image Question" Color="FF0000" />
		<Colour Parent="Table2" Source="Lay_Statement" Condition="Answer_x0020_Type" Value="Basic defined" Color="FF0000" />
		<Colour Parent="Table2" Source="Lay_Statement" Condition="Answer_x0020_Type" Value="Multiple Category" Color="FF0000" />
		<Colour Parent="Table2" Source="Lay_Statement" Condition="Answer_x0020_Type" Value="Age" Color="FF0000" />
		<Colour Parent="Table2" Source="Lay_Statement" Condition="Answer_x0020_Type" Value="Body part" Color="FF0000" />
		<Colour Parent="Table2" Source="Lay_Statement" Condition="Answer_x0020_Type" Value="Value: Choose" Color="FF0000" />
		<Colour Parent="Table2" Source="Lay_Statement" Condition="Answer_x0020_Type" Value="Value: Enter" Color="FF0000" />
		<Colour Parent="Table2" Source="Lay_Statement" Condition="Answer_x0020_Type" Value="Text: Enter" Color="FF0000" />
		<Colour Parent="Table2" Source="Lay_Statement" Condition="Answer_x0020_Type" Value="Date/Time: Enter" Color="FF0000" />
		<Colour Parent="Table2" Source="Answer" Condition="Answer_x0020_Type" Value="Basic defined" Color="FF0000" />
		<Colour Parent="Table2" Source="Answer" Condition="Answer_x0020_Type" Value="Multiple Category" Color="FF0000" />
		<Colour Parent="Table2" Source="Answer" Condition="Answer_x0020_Type" Value="Age" Color="FF0000" />
		<Colour Parent="Table2" Source="Answer" Condition="Answer_x0020_Type" Value="Body part" Color="FF0000" />
		<Colour Parent="Table2" Source="Answer" Condition="Answer_x0020_Type" Value="Value: Choose" Color="FF0000" />
		<Colour Parent="Table2" Source="Answer" Condition="Answer_x0020_Type" Value="Value: Enter" Color="FF0000" />
		<Colour Parent="Table2" Source="Answer" Condition="Answer_x0020_Type" Value="Text: Enter" Color="FF0000" />
		<Colour Parent="Table2" Source="Answer" Condition="Answer_x0020_Type" Value="Date/Time: Enter" Color="FF0000" />
		<Colour Parent="Table2" Source="Explanation" Condition="Answer_x0020_Type" Value="Basic defined" Color="FF0000" />
		<Colour Parent="Table2" Source="Explanation" Condition="Answer_x0020_Type" Value="Multiple Category" Color="FF0000" />
		<Colour Parent="Table2" Source="Explanation" Condition="Answer_x0020_Type" Value="Age" Color="FF0000" />
		<Colour Parent="Table2" Source="Explanation" Condition="Answer_x0020_Type" Value="Body part" Color="FF0000" />
		<Colour Parent="Table2" Source="Explanation" Condition="Answer_x0020_Type" Value="Value: Choose" Color="FF0000" />
		<Colour Parent="Table2" Source="Explanation" Condition="Answer_x0020_Type" Value="Value: Enter" Color="FF0000" />
		<Colour Parent="Table2" Source="Explanation" Condition="Answer_x0020_Type" Value="Text: Enter" Color="FF0000" />
		<Colour Parent="Table2" Source="Explanation" Condition="Answer_x0020_Type" Value="Date/Time: Enter" Color="FF0000" />
		<Colour Parent="Table3" Source="Lay_Statement" Condition="Silent" Value="false" Color="FF0000" />
		<Colour Parent="Table3" Source="Explanation" Condition="Silent" Value="false" Color="FF0000" />
		<Colour Parent="Table3" Source="More_Detail" Condition="Silent" Value="false" Color="FF0000" />
    <Colour Parent="Table4" Source="Bullet" Color="FF0000" />
    <Colour Parent="Table5" Source="Title" Color="FF0000" />
  </xsl:variable>
	<xsl:variable name="replace">
    <Replace Source="TitleID" Target="Title ID"/>
    <Replace Source="AlgoID" Target="Algo ID"/>
		<Replace Source="QuestionID" Target="Question ID"/>
		<Replace Source="AnsID" Target="Answer ID"/>
		<Replace Source="RecID" Target="Conclusion ID"/>
		<Replace Source="BPID" Target="Bullet ID"/>
		<Replace Source="State" Target="History"/>
		<Replace Parent="Table" Source="Algo_Name" Target="Algo Name"/>
		<Replace Parent="Table" Source="Word_Merge" Target="Word Merge 1"/>
		<Replace Parent="Table" Source="WM2" Target="Word Merge 2"/>
		<Replace Parent="Table1" Source="Expert_Statement" Target="Expert"/>
		<Replace Parent="Table1" Source="Lay_Statement" Target="Summary"/>
		<Replace Parent="Table2" Source="Expert_Statement" Target="Expert"/>
		<Replace Parent="Table2" Source="Lay_Statement" Target="Summary"/>
		<Replace Parent="Table3" Source="Expert_Statement" Target="Expert"/>
		<Replace Parent="Table3" Source="Lay_Statement" Target="Lay"/>
	</xsl:variable>
	<xsl:variable name="trans">
		<Replace Source="Expert_Statement" Target="Clinical_Statement"/>
		<Replace Source="Expert_Statement" Target="Possible_Condition"/>
		<Replace Source="Expert_Statement" Target="Clinical_Answer"/>
		<Replace Source="Bullet" Target="BP_TEXT"/>
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
					<xsl:variable name="num" select="position()"/>
					<xsl:for-each select="$doc">
						<xsl:call-template name="Paragraph">
							<xsl:with-param name="Text">
								<xsl:text>Asset Report</xsl:text>
								<xsl:if test="$currentDate">
									<xsl:text> - </xsl:text>
									<xsl:value-of select="$currentDate"/>
								</xsl:if>
							</xsl:with-param>
							<xsl:with-param name="Align">center</xsl:with-param>
							<xsl:with-param name="Bold">1</xsl:with-param>
							<xsl:with-param name="Underline">single</xsl:with-param>
							<xsl:with-param name="FontSize">8</xsl:with-param>
						</xsl:call-template>
						<xsl:element name="w:p">
							<xsl:call-template name="Range">
								<xsl:with-param name="Text">Fields in </xsl:with-param>
							</xsl:call-template>
							<xsl:call-template name="Range">
								<xsl:with-param name="Color">FF0000</xsl:with-param>
								<xsl:with-param name="Text">red text</xsl:with-param>
							</xsl:call-template>
							<xsl:call-template name="Range">
								<xsl:with-param name="Text"> may be displayed to the user in Q&amp;A, information icons, summary statements, or reports.</xsl:with-param>
							</xsl:call-template>
						</xsl:element>
						<xsl:if test="$imerge = 'Merge'">
							<xsl:element name="w:p">
								<xsl:call-template name="Range">
									<xsl:with-param name="Text">Translated fields are displayed in </xsl:with-param>
								</xsl:call-template>
								<xsl:call-template name="Range">
									<xsl:with-param name="Color">0000FF</xsl:with-param>
									<xsl:with-param name="Text">blue text</xsl:with-param>
								</xsl:call-template>
								<xsl:call-template name="Range">
									<xsl:with-param name="Text">.</xsl:with-param>
								</xsl:call-template>
							</xsl:element>
						</xsl:if>
						<xsl:if test="*/root and $isplit = 'Split'">
							<xsl:call-template name="Paragraph">
								<xsl:with-param name="Text">Do not translate grey fields</xsl:with-param>
							</xsl:call-template>
						</xsl:if>
						<xsl:for-each select="*">
							<xsl:variable name="asset" select="." />
							<xsl:variable name="idName" select="name(*[1])" />
							<xsl:variable name="idNum" select="count(msxsl:node-set($replace)/Replace[@Source=$idName]/preceding-sibling::Replace)" />
							<xsl:element name="w:p">
								<xsl:element name="w:pPr">
									<xsl:element name="w:keepNext">
										<xsl:attribute name="w:val">on</xsl:attribute>
									</xsl:element>
								</xsl:element>
								<xsl:call-template name="Range">
									<xsl:with-param name="Text">
										<xsl:choose>
											<xsl:when test="msxsl:node-set($replace)/Replace[@Source=$idName]">
												<xsl:value-of select="msxsl:node-set($replace)/Replace[@Source=$idName]/@Target" />
											</xsl:when>
											<xsl:otherwise>
												<xsl:value-of select="$idName" />
											</xsl:otherwise>
										</xsl:choose>
									</xsl:with-param>
									<xsl:with-param name="Bold">1</xsl:with-param>
								</xsl:call-template>
								<xsl:call-template name="Range">
									<xsl:with-param name="Text">
										<xsl:text>: </xsl:text>
										<xsl:value-of select="*[1]"/>
									</xsl:with-param>
								</xsl:call-template>
								<xsl:for-each select="*">
									<xsl:variable name="name">
										<xsl:call-template name="replace">
											<xsl:with-param name="text" select="name()"/>
											<xsl:with-param name="find">_x0020_</xsl:with-param>
											<xsl:with-param name="replace">
												<xsl:text> </xsl:text>
											</xsl:with-param>
										</xsl:call-template>
									</xsl:variable>
									<xsl:if test="(contains($libraryColumns, concat ('|',$name,'|')) or contains($detailColumns, concat ('|',$name,'|'))) and . != ''">
										<xsl:call-template name="Range">
											<xsl:with-param name="Text">
												<xsl:text> - </xsl:text>
											</xsl:with-param>
										</xsl:call-template>
										<xsl:call-template name="Range">
											<xsl:with-param name="Bold">1</xsl:with-param>
											<xsl:with-param name="Text">
												<xsl:choose>
													<xsl:when test="msxsl:node-set($replace)/Replace[@Source=$name]">
														<xsl:value-of select="msxsl:node-set($replace)/Replace[@Source=$name]/@Target" />
													</xsl:when>
													<xsl:otherwise>
														<xsl:value-of select="$name" />
													</xsl:otherwise>
												</xsl:choose>
											</xsl:with-param>
										</xsl:call-template>
										<xsl:call-template name="Range">
											<xsl:with-param name="Text">
												<xsl:text>: </xsl:text>
												<xsl:value-of select="."/>
											</xsl:with-param>
										</xsl:call-template>
									</xsl:if>
								</xsl:for-each>
							</xsl:element>
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
									<w:tblLayout w:type="Fixed"/>
								</xsl:element>
								<xsl:for-each select="*">
									<xsl:variable name="name">
										<xsl:call-template name="replace">
											<xsl:with-param name="text" select="name()"/>
											<xsl:with-param name="find">_x0020_</xsl:with-param>
											<xsl:with-param name="replace">
												<xsl:text> </xsl:text>
											</xsl:with-param>
										</xsl:call-template>
									</xsl:variable>
									<xsl:variable name="_name">
										<xsl:call-template name="replace">
											<xsl:with-param name="text" select="name()"/>
											<xsl:with-param name="find">_x0020_</xsl:with-param>
											<xsl:with-param name="replace">
												<xsl:text>_</xsl:text>
											</xsl:with-param>
										</xsl:call-template>
									</xsl:variable>
									<xsl:if test="contains($addedColumns, concat ('|',$idNum,$name,'|')) and (. != '' or ../root/*[name() = $_name or name() = msxsl:node-set($trans)/Replace[@Source = $_name]/@Target] != '')">
										<xsl:variable name="parent" select="name(..)" />
										<xsl:variable name="colrule" select="msxsl:node-set($colour)/Colour[@Parent=$parent and @Source=$_name]" />
										<xsl:variable name="fillrule" select="msxsl:node-set($fill)/Colour[@Parent=$parent and (not(@Source) or @Source=$_name)]" />
										<xsl:element name="w:tr">
											<xsl:element name="w:tc">
												<xsl:element name="w:tcPr">
													<xsl:element name="w:tcW">
														<xsl:attribute name="w:w">1500</xsl:attribute>
														<xsl:attribute name="w:type">dxa</xsl:attribute>
													</xsl:element>
												</xsl:element>
												<xsl:call-template name="Paragraph">
													<xsl:with-param name="Text">
														<xsl:choose>
															<xsl:when test="msxsl:node-set($replace)/Replace[@Parent=$parent and @Source=$_name]">
																<xsl:value-of select="msxsl:node-set($replace)/Replace[@Parent=$parent and @Source=$_name]/@Target" />
															</xsl:when>
															<xsl:otherwise>
																<xsl:value-of select="$name" />
															</xsl:otherwise>
														</xsl:choose>
													</xsl:with-param>
													<xsl:with-param name="Color">
														<xsl:if test="$colrule[not(@Condition) or $asset/*[name()=$colrule/@Condition]=$colrule/@Value]">
															<xsl:value-of select="$colrule[not(@Condition) or $asset/*[name()=$colrule/@Condition]=$colrule/@Value]/@Color"/>
														</xsl:if>
													</xsl:with-param>
													<xsl:with-param name="Bold">1</xsl:with-param>
												</xsl:call-template>
											</xsl:element>
											<xsl:variable name="lang" select="../root/*[name() = $_name or name() = msxsl:node-set($trans)/Replace[@Source = $_name]/@Target]" />
											<xsl:element name="w:tc">
												<xsl:element name="w:tcPr">
													<xsl:element name="w:tcW">
														<xsl:choose>
															<xsl:when test="../root and $isplit = 'Split'">
																<xsl:attribute name="w:w">3500</xsl:attribute>
															</xsl:when>
															<xsl:otherwise>
																<xsl:attribute name="w:w">7000</xsl:attribute>
															</xsl:otherwise>
														</xsl:choose>
														<xsl:attribute name="w:type">dxa</xsl:attribute>
													</xsl:element>
												</xsl:element>
												<xsl:call-template name="Paragraphs">
													<xsl:with-param name="text">
														<xsl:choose>
															<xsl:when test="$imerge = 'Merge' and $lang != ''">
																<xsl:value-of select="$lang"/>
															</xsl:when>
															<xsl:otherwise>
																<xsl:value-of select="."/>
															</xsl:otherwise>
														</xsl:choose>
													</xsl:with-param>
													<xsl:with-param name="Color">
														<xsl:if test="$imerge = 'Merge' and $lang != ''">
															<xsl:text>0000FF</xsl:text>
														</xsl:if>
													</xsl:with-param>
												</xsl:call-template>
											</xsl:element>
											<xsl:if test="../root and $isplit = 'Split'">
												<xsl:element name="w:tc">
													<xsl:element name="w:tcPr">
														<xsl:element name="w:tcW">
															<xsl:attribute name="w:w">3500</xsl:attribute>
															<xsl:attribute name="w:type">dxa</xsl:attribute>
														</xsl:element>
														<xsl:if test="$fillrule[not(@Condition) or $asset/*[name()=$fillrule/@Condition]=$fillrule/@Value]">
															<xsl:element name="w:shd">
																<xsl:attribute name="w:val">clear</xsl:attribute>
																<xsl:attribute name="w:color">auto</xsl:attribute>
																<xsl:attribute name="w:fill">
																	<xsl:value-of select="$fillrule[not(@Condition) or $asset/*[name()=$fillrule/@Condition]=$fillrule/@Value]/@Color" />
																</xsl:attribute>
															</xsl:element>
														</xsl:if>
													</xsl:element>
													<xsl:call-template name="Paragraphs">
														<xsl:with-param name="text">
															<xsl:if test="$imerge != 'Merge'">
																<xsl:value-of select="$lang" />
															</xsl:if>
														</xsl:with-param>
													</xsl:call-template>
												</xsl:element>
											</xsl:if>
										</xsl:element>
									</xsl:if>
								</xsl:for-each>
							</xsl:element>
							<xsl:element name="w:p" />
						</xsl:for-each>
					</xsl:for-each>
				</xsl:element>
			</xsl:for-each>
		</xsl:element>
	</xsl:template>

	<xsl:template name="Paragraph">
		<xsl:param name="Text"/>
		<xsl:param name="Color"/>
		<xsl:param name="Bold"/>
		<xsl:param name="Italic"/>
		<xsl:param name="Underline"/>
		<xsl:param name="FontSize" select="8"/>
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
				<xsl:with-param name="Color">
					<xsl:value-of select="$Color"/>
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

	<xsl:template name="Range">
		<xsl:param name="Text"/>
		<xsl:param name="Color"/>
		<xsl:param name="Bold"/>
		<xsl:param name="Italic"/>
		<xsl:param name="Underline"/>
		<xsl:param name="FontSize" select="8"/>
		<xsl:element name="w:r">
			<xsl:element name="w:rPr">
				<xsl:if test="$Color != ''">
					<xsl:element name="w:color">
						<xsl:attribute name="w:val">
							<xsl:value-of select="$Color"/>
						</xsl:attribute>
					</xsl:element>
				</xsl:if>
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

	<xsl:template name="Paragraphs">
		<xsl:param name="text"/>
		<xsl:param name="Color"/>
		<xsl:choose>
			<xsl:when test="contains($text, '&#xA;')">
				<xsl:variable name="before" select="substring-before($text, '&#xA;')"/>
				<xsl:variable name="after" select="substring-after($text, '&#xA;')"/>
				<xsl:call-template name="Paragraph">
					<xsl:with-param name="Text" select="$before" />
					<xsl:with-param name="Color" select="$Color"/>
				</xsl:call-template>
				<xsl:if test="string-length($after)>4">
					<xsl:call-template name="Paragraphs">
						<xsl:with-param name="text" select="$after"/>
						<xsl:with-param name="Color" select="$Color"/>
					</xsl:call-template>
				</xsl:if>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="Paragraph">
					<xsl:with-param name="Text" select="$text"/>
					<xsl:with-param name="Color" select="$Color"/>
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
</xsl:stylesheet>
