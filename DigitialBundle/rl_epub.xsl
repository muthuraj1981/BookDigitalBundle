Leninponraj
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:db="http://docbook.org/ns/docbook" xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops"  xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:exsl="http://exslt.org/common" xmlns:mml="http://www.w3.org/1998/Math/MathML" extension-element-prefixes="exsl" version="2.0"  xpath-default-namespace="http://www.w3.org/1999/xhtml" exclude-result-prefixes="db xlink exsl mml #default">
			
	<xsl:param name="bid" select="//db:biblioid[@role='epub']"/>
	<xsl:param name="toclevel" select="3"/>
	<xsl:param name="mathCnt" select="count(//db:equation)"></xsl:param>
	<xsl:variable name="intncx" as="element()">
		<navMap>
			<navPoint>
				<navLabel>
					<text>Cover</text>
				</navLabel>
				<content src="xhtml/cover.xhtml#cover-page"/>
			</navPoint>
			<navPoint>
				<navLabel>
					<text>Half-Title</text>
				</navLabel>
				<content src="xhtml/halftitle.xhtml#half_1"/>
			</navPoint>
			<xsl:if test="//db:cover">
				<navPoint>
					<navLabel>
						<text>Series</text>
					</navLabel>
					<content src="xhtml/series.xhtml"/>
				</navPoint>
			</xsl:if>
			<xsl:if test="//db:dedication">
				<navPoint>
					<navLabel>
						<text>Dedication</text>
					</navLabel>
					<content src="xhtml/dedication.xhtml"/>
				</navPoint>
			</xsl:if>
			<xsl:if test="//db:glossary">
				<navPoint>
					<navLabel>
						<text>Glossary</text>
					</navLabel>
					<content src="xhtml/glossary.xhtml"/>
				</navPoint>
			</xsl:if>
			<navPoint>
				<navLabel>
					<text>Title</text>
				</navLabel>
				<content src="xhtml/title.xhtml#title_1"/>
			</navPoint>
			<navPoint>
				<navLabel>
					<text>Copyright</text>
				</navLabel>
				<content src="xhtml/copyright.xhtml#copy"/>
			</navPoint>
			<navPoint>
				<navLabel>
					<text>Contents</text>
				</navLabel>
				<content src="xhtml/contents.xhtml#re_contents"/>
			</navPoint>
			<xsl:for-each select="//db:preface|//db:acknowledgements|//db:abbreviation">
				<navPoint>
					<navLabel>
						<text>
							<xsl:value-of select="db:info/db:title" />
						</text>
					</navLabel>
					<content src="xhtml/{substring-after(substring-after(@xml:id,'-'),'-')}.xhtml#{substring-after(substring-after(@xml:id,'-'),'-')}"/>
				</navPoint>
			</xsl:for-each>
			<xsl:for-each select="//db:chapter[not(@role='notes' or contains(@xml:id, 'note'))]|//db:appendix|//db:part[child::db:label]">
				<navPoint>
					<navLabel>
						<text>
							<xsl:apply-templates select="db:info/db:title" mode="ncx"/>
						</text>
					</navLabel>
					<content src="xhtml/{substring-after(substring-after(@xml:id,'-'),'-')}.xhtml#{substring-after(substring-after(@xml:id,'-'),'-')}"/>
				</navPoint>
					
					<xsl:if test="db:section">
						<xsl:if test="$toclevel=1">
							<xsl:for-each select="db:section">
								<navPoint>
									<navLabel>
										<text>
											<xsl:apply-templates select="db:info/db:title" mode="ncx"/>
										</text>
									</navLabel>
									<content src="xhtml/{concat(substring-after(substring-after((ancestor::db:chapter|ancestor::db:appendix)/@xml:id,'-'),'-'),'.xhtml#',@xml:id)}"/>
								</navPoint>
							</xsl:for-each>
						</xsl:if>
						<xsl:if test="$toclevel=2">
							<xsl:for-each select="db:section|db:section/db:section">
								<navPoint>
									<navLabel>
										<text>
											<xsl:apply-templates select="db:info/db:title" mode="ncx"/>
										</text>
									</navLabel>
									<content src="xhtml/{concat(substring-after(substring-after((ancestor::db:chapter|ancestor::db:appendix)/@xml:id,'-'),'-'),'.xhtml#',@xml:id)}"/>
								</navPoint>
							</xsl:for-each>
						</xsl:if>
						<xsl:if test="$toclevel=3">
							<xsl:for-each select="db:section[child::db:info/db:title]|db:section/db:section[child::db:info/db:title]|db:section/db:section/db:section[child::db:info/db:title]">
								<navPoint>
									<navLabel>
										<text>
											<xsl:apply-templates select="db:info/db:title" mode="ncx"/>
										</text>
									</navLabel>
									<content src="xhtml/{concat(substring-after(substring-after((ancestor::db:chapter|ancestor::db:appendix)/@xml:id,'-'),'-'),'.xhtml#',@xml:id)}"/>
								</navPoint>
							</xsl:for-each>
						</xsl:if>
					</xsl:if>
				
			</xsl:for-each>
			<xsl:if test="//db:footnote[@role='end-bk1-note']">
				<navPoint>
					<navLabel>
						<text>Notes</text>
					</navLabel>
					<content src="xhtml/notes.xhtml#notes"/>
				</navPoint>
			</xsl:if>
			<xsl:if test="//db:bibliography[not(ancestor::db:chapter)]">
				<navPoint>
					<navLabel>
						<text>
							<xsl:value-of select="//db:bibliography[not(ancestor::db:chapter)]/db:info/db:title"/>
						</text>
					</navLabel>
					<content src="xhtml/bibliography.xhtml#bib"/>
				</navPoint>
				<xsl:if test="//db:bibliography[not(ancestor::db:chapter)]/db:bibliodiv">
					<xsl:if test="$toclevel=1">
						<xsl:for-each select="//db:bibliography[not(ancestor::db:chapter)]/db:bibliodiv[concat('sec',substring-after(@xml:id,'sec'))='sec1']">
							<navPoint>
								<navLabel>
									<text>
										<xsl:apply-templates select="db:info/db:title" mode="ncx"/>
									</text>
								</navLabel>
								<content src="xhtml/{concat('bibliography.xhtml#',@xml:id)}"/>
							</navPoint>
						</xsl:for-each>
					</xsl:if>
					<xsl:if test="$toclevel=2">
						<xsl:for-each select="//db:bibliography[not(ancestor::db:chapter)]/db:bibliodiv[concat('sec',substring-after(@xml:id,'sec'))='sec1']|//db:bibliography[not(ancestor::db:chapter)]/db:bibliodiv[concat('sec',substring-after(@xml:id,'sec'))='sec2']">
							<navPoint>
								<navLabel>
									<text>
										<xsl:apply-templates select="db:info/db:title" mode="ncx"/>
									</text>
								</navLabel>
								<content src="xhtml/{concat('bibliography.xhtml#',@xml:id)}"/>
							</navPoint>
						</xsl:for-each>
					</xsl:if>
					<xsl:if test="$toclevel=3">
						<xsl:for-each select="//db:bibliography[not(ancestor::db:chapter)]/db:bibliodiv[concat('sec',substring-after(@xml:id,'sec'))='sec1']|//db:bibliography[not(ancestor::db:chapter)]/db:bibliodiv[concat('sec',substring-after(@xml:id,'sec'))='sec2']|//db:bibliography[not(ancestor::db:chapter)]/db:bibliodiv[concat('sec',substring-after(@xml:id,'sec'))='sec3']">
							<navPoint>
								<navLabel>
									<text>
										<xsl:apply-templates select="db:info/db:title" mode="ncx"/>
									</text>
								</navLabel>
								<content src="xhtml/{concat('bibliography.xhtml#',@xml:id)}"/>
							</navPoint>
						</xsl:for-each>
					</xsl:if>
				</xsl:if>
			</xsl:if>
			<xsl:if test="//db:indexterm">
				<navPoint>
					<navLabel>
						<text>Index</text>
					</navLabel>
					<content src="xhtml/index.xhtml#index"/>
				</navPoint>
			</xsl:if>
			
		</navMap>
	</xsl:variable>

	
	<xsl:template match="db:bibliography[ancestor::db:chapter]">
		<section>
		<h2 id="{./@xml:id}">
			<xsl:attribute name="class">H1</xsl:attribute>
			<xsl:apply-templates select="./child::db:title/node()"/>
		</h2>
		<ol class="ol-1">
			<xsl:apply-templates/>
		</ol>
		</section>
	</xsl:template>
	
	<xsl:template match="db:deletedfigure">
		<figure class="image-tt">
			<xsl:apply-templates/>
		</figure>
	</xsl:template>
	
	<xsl:template match="db:p[@role='float']">
		<p class="TT">
			<xsl:attribute name="id"><xsl:value-of select="./@xml:id"></xsl:value-of></xsl:attribute>
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	
	<xsl:template match="db:br">
		<br>
			<xsl:apply-templates/>
		</br>
	</xsl:template>
	
	<xsl:template match="db:deletedimg">
		<img>
			<xsl:attribute name="src"><xsl:value-of select="./@src"></xsl:value-of></xsl:attribute>
			<xsl:attribute name="alt"><xsl:value-of select="./@alt"></xsl:value-of></xsl:attribute>
			<xsl:apply-templates/>
		</img>
	</xsl:template>
	
	<xsl:template match="db:bibliography[ancestor::db:chapter]/db:title"/>
	
	<!--<xsl:template match="db:bibliography/db:title">
		<h2 id="{parent::db:bibliography/@xml:id}">
			<xsl:attribute name="class">H1</xsl:attribute>
			<xsl:apply-templates/>
		</h2>
	</xsl:template>-->
	<xsl:variable name="lbibcount">
		<xsl:value-of select="count(//db:bibliography[not(ancestor::db:chapter)])"/>
	</xsl:variable>
	<xsl:include href="ent.xsl"/>
	<xsl:key name="images" match="db:figure|db:informaltable|db:table" use="@xml:id"/>
	<xsl:key name="ill" match="db:figure" use="db:caption/db:para"/>
	<xsl:template match="/">
		<xsl:apply-templates/>
		

		<xsl:result-document href="{translate($bid,'-','')}/mimetype" method="text">
			<xsl:text>application/epub+zip</xsl:text>
		</xsl:result-document>
		<!--<xsl:result-document href="{translate($bid,'-','')}/oebps-page-map.xml" method="xml" encoding="UTF-8" standalone="yes">
			<container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
				<rootfiles>
					<rootfile full-path="OEBPS/oebps-page-map.xml" media-type="application/oebps-package+xml"/>
				</rootfiles>
			</container>
		</xsl:result-document>-->

		

		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/styles/stylesheet.css" method="text">
			<xsl:text>
@page
{
	margin : 0.5em;
}
tbody, thead, tfoot, tr, td, th {
	border-style : inherit;
	border-width : inherit;
	border-color : inherit;
}
.leftFloat {
	float : left;
}
.rightFloat {
	float : right;
}
span.underline {
	text-decoration:underline;
}
span.smallcaps {
	font-variant:small-caps;
}
span.bold {
	font-weight : bold;
}

.strike
{
text-decoration : line-through;
}


span.uppercase
{
	text-transform : uppercase;
}
span.italic {
	font-style : italic;
}
span.superscript {
	vertical-align : super;
	font-size : 0.75em;
}
span.subscript {
	vertical-align : sub;
	font-size : 0.75em;
}
.right {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : right;
	color : #000000;
	text-indent : 0em;
	margin-top : 1em;
	margin-bottom : 0.00em;
}
.center {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin-top : 1em;
	margin-bottom : 0.00em;
}

.CT1 {
	font-weight : normal;
	font-style : normal;
	font-size : 1.5em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.23;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 0.5em 0em 0.5em 0em;
}
.CT-a {
	font-weight : normal;
	font-style : normal;
	font-size : 1.5em;
	text-decoration : none;
	line-height : 0.64;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin-top : 3em;
	margin-bottom : 1em;
}
.FM-HT {
	font-weight : bold;
	font-style : normal;
	font-size : 2em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.14;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 2em 0em 0em 0em;
}
.Half-Title {
	font-weight : normal;
	font-style : normal;
	font-size : 1.5em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.19;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 3em 0em 1em 0em;
}

.EDI
{
	font-weight : normal;
	font-style : normal;
	font-size : 1.1em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 0em 0em;
}

.Book-Title {
	font-weight : bold;
	font-style : normal;
	font-size : 2em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.14;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 2em 0em 0.2em 0em;
}
.Sub-Title {
	font-weight : normal;
	font-style : normal;
	font-size : 1.4em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.25;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 5em 0em;
}

.Book-Author
{
	font-weight : normal;
	font-style : normal;
	font-size : 1.1em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 3em 0em 12em 0em;
}
.Book-Author1
{
	font-weight : normal;
	font-style : normal;
	font-size : 1.1em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 3em 0em 0em 0em;
}

.PUB {
	font-weight : normal;
	font-style : normal;
	font-size : 0.85em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.1;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 0em 0em;
}

.Dedi-TXT1 {
	font-weight : normal;
	font-style : italic;
	font-size : 1.0em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.5;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 8em 0em 0.5em 0em;
}
.Dedi-TXT {
	font-weight : normal;
	font-style : italic;
	font-size : 1em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.5;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 1em 0em;
}


.Copy-TXT {
	font-weight : normal;
	font-style : normal;
	font-size : 0.75em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.29;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0.7em 0em 0em 0em;
}
.Copy-TXT1 {
	font-weight : normal;
	font-style : normal;
	font-size : 0.75em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.29;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 3.30em 0em 0em 0em;
}
.Copy-TXI {
	font-weight : normal;
	font-style : normal;
	font-size : 0.75em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.29;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0em;
}

.TOC-CA1 {
	font-weight : normal;
	font-style : italic;
	font-size : 1em;
	text-decoration : none;
	font-variant : normal;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 0em 0em;
}
.TOC-CA {
	font-weight : normal;
	font-style : italic;
	font-size : 1em;
	text-decoration : none;
	font-variant : normal;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 0em 2.3em;
}

.TOC-FM {
	font-weight : normal;
	font-style : normal;
	font-size : 1em;
	text-decoration : none;
	font-variant : normal;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0.5em 0em 0em 0em;
}
.TOC-BM1 {
	font-weight : normal;
	font-style : normal;
	font-size : 1.0em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.56;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 0em 0em;
}
.TOC-BM {
	font-weight : normal;
	font-style : normal;
	font-size : 1.0em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.56;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 1em 0em 0em 0em;
}
.TOC-CH1 {
	font-weight : normal;
	font-style : normal;
	font-size : 1em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : -2.5em;
	margin : 0.5em 0em 0.1em 2.5em;
}
.TOC-PT {
	font-weight : bold;
	font-style : normal;
	font-size : 1.08em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 1em 0em 0.7em 0em;
	text-transform : uppercase;
}

.TOC-CH {
	font-weight : normal;
	font-style : normal;
	font-size : 1em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : -2em;
	margin : 0.5em 0em 0.1em 2.2em;
}

.TOC-CH-sec1 {
	font-weight : normal;
	font-style : normal;
	font-size : 1em;
	text-decoration : none;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : -2em;
	margin : 0em 0em 0em 3.9em;
}
.disabled-link
{
	pointer-events: none;
}

.TOC-CH-sec2 {
	font-weight : normal;
	font-style : normal;
	font-size : 1em;
	text-decoration : none;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : -2em;
	margin : 0em 0em 0em 5em;
}
.TOC-CH-sec2a {
	font-weight : normal;
	font-style : normal;
	font-size : 1em;
	text-decoration : none;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : -2em;
	margin : 0em 0em 0em 5.5em;
}
.TOC-CH-sec3 {
	font-weight : normal;
	font-style : normal;
	font-size : 1em;
	text-decoration : none;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : -2em;
	margin : 0em 0em 0em 7.3em;
}
.TOC-CH-sec3a {
	font-weight : normal;
	font-style : normal;
	font-size : 1em;
	text-decoration : none;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : -2em;
	margin : 0em 0em 0em 8.2em;
}


.TXT {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 0em;
}


.TXT1 {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 1em 0em 0em 0em;
}


.contri {
	font-weight : normal;
	font-style : normal;
	font-size : 1em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : -1em;
	margin : 1em 0em 0em 1em;
}
.contri1 {
	font-weight : normal;
	font-style : normal;
	font-size : 1em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : -1em;
	margin : 0em 0em 0em 1em;
}

.TXI {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : 1.2em;
	margin : 0em;
}
.TXI-l {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin-top : 0em;
	margin-bottom : 0em;
	margin-left : 2em;
}
.TXT-DC {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 0em;
}
.H1 {
	font-weight : bold;
	font-style : normal;
	font-size : 1em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : center;
	color : #000000;
	margin : 1em 0em 0.4em 1.5em;
	text-transform : uppercase;
}
.H2 {
	font-weight : bold;
	font-style : normal;
	font-size : 1em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.17;
	text-align : left;
	color : #000000;
	margin : 1em 0em 0.4em 0em;
}
.H2-Underline {
	font-weight : bold;
	font-style : normal;
	font-size : 1.08em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.38;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	border-bottom : solid 0.1em;
	margin : 1.4em 0em 0.5em 0em;
}

.H3 {
	font-weight : normal;
	font-style : italic;
	font-size : 0.92em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.23;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 1em 0em 0.3em 0em;
}
.H3-sub {
	font-weight : bold;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 0em;
}
.H4 {
	font-weight : normal;
	font-style : italic;
	font-size : 1em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 1.2em 0em 0em 0em;
}

.Box-Head {
	font-weight : bold;
	font-style : normal;
	font-size : 0.96em;
	text-decoration : none;
	text-transform : uppercase;
	line-height : 1.17;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 0em 1.2em 0.5em 1.2em;
}
.SWAP-BOX-Head {
	font-weight : bold;
	font-style : normal;
	font-size : 1.17em;
	text-decoration : none;
	text-transform : uppercase;
	line-height : 0.96;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	padding : 1em;
	margin-top : 0em;
	margin-bottom : 0.6em;
	background-color : #D7D7D7;
}

.Box-TXT {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0em 1.2em 0.5em 1.2em;
}
.Swap-Box-text {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : -1.2em;
	margin : 0em 1.2em 0.5em 2.5em;
}
.Stat-source {
	font-weight : normal;
	font-style : normal;
	font-size : 0.79em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.89;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 0.2em 0em;
}

.box_h
{
	font-weight : bold;
	font-style : normal;
	font-size : 0.88em;
	text-transform : uppercase;
	font-variant : normal;
	line-height : 1.35;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 0.5em 0em 0.5em 0em;
}



.CN {
	font-weight : bold;
	font-style : italic;
	font-size : 1.5em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.13;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 2em 0em 0.5em 0em;
}
.CN1 {
	font-weight : bold;
	font-style : normal;
	font-size : 1.8em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.13;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0.1em 0em 3em 0em;
}
.CN1a {
	font-weight : normal;
	font-style : italic;
	font-size : 1.8em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.13;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0.1em 0em 2em 0em;
}


.CT {
	font-weight : normal;
	font-style : normal;
	font-size : 1em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.23;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 0.5em 0em 3em 0em;
}
.CST {
	font-weight : bold;
	font-style : italic;
	font-size : 1.4em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.13;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 0.5em 0em 0em 0em;
}
.CSTa {
	font-weight : bold;
	font-style : normal;
	font-size : 1.4em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.13;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 0.5em 0em 0em 0em;
}
.CST1 {
	font-weight : bold;
	font-style : italic;
	font-size : 1.4em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.13;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 0.5em 0em 3em 0em;
}
.CST1a {
	font-weight : bold;
	font-style : normal;
	font-size : 1.4em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.13;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 0.5em 0em 3em 0em;
}
.PST {
	font-weight : normal;
	font-style : normal;
	font-size : 1.4em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.13;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 0.8em 0em 0em 0em;
	text-transform : uppercase;
}
.EXT-not1 {
	font-weight : normal;
	font-style : normal;
	font-size : 0.67em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 0.2em 0em 0.2em 2em;
}
.EXT-not2 {
	font-weight : normal;
	font-style : normal;
	font-size : 0.67em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 0.2em 0em 0.2em 1em;
}
.EXT-not {
	font-weight : normal;
	font-style : normal;
	font-size : 0.67em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 1em 0em 1em 0.2em;
}
.EXT-bib {
	font-weight : normal;
	font-style : normal;
	font-size : 0.75em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 1em 0em 1em 0em;
}
.EXT {
	font-weight : normal;
	font-style : normal;
	font-size : 0.83em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 1em 0em 1em 1em;
}
.EXTF {
	font-weight : normal;
	font-style : normal;
	font-size : 0.83em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 1em 0em 0em 1em;
}
.EXTM {
	font-weight : normal;
	font-style : normal;
	font-size : 0.83em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 0.2em 0em 0em 1em;
}
.EXTL {
	font-weight : normal;
	font-style : normal;
	font-size : 0.83em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 0.2em 0em 1em 1em;
}
.EXT-F {
	font-weight : normal;
	font-style : normal;
	font-size : 0.83em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : 2em;
	margin : 1em 0em 0em 1em;
}
.EXT-M {
	font-weight : normal;
	font-style : normal;
	font-size : 0.83em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : 2em;
	margin : 0.2em 0em 0em 1em;
}
.EXT-L {
	font-weight : normal;
	font-style : normal;
	font-size : 0.83em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : 2em;
	margin : 0.2em 0em 1em 1em;
}
.EXT-F1 {
	font-weight : normal;
	font-style : normal;
	font-size : 0.83em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : -2em;
	margin : 1em 0em 0em 3em;
}
.EXT-M1 {
	font-weight : normal;
	font-style : normal;
	font-size : 0.83em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : -2em;
	margin : 0.2em 0em 0em 3em;
}
.EXT-L1 {
	font-weight : normal;
	font-style : normal;
	font-size : 0.83em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : -2em;
	margin : 0.2em 0em 1em 3em;
}
.FN {
	font-weight : normal;
	font-style : normal;
	font-size : 0.67em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 0.2em 0em;
}
.FN1 {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : 1.0em;
	margin : 0em 0em 0.2em 1em;
}
.FN2 {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : 0.6em;
	margin : 0em 0em 0.2em 1em;
}
.FN3 {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.2;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 0.2em 1em;
}
.FC {
	font-weight : normal;
	font-style : normal;
	font-size : 0.75em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.33;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 1em 0em;
}
.FS {
	font-weight : normal;
	font-style : italic;
	font-size : 0.75em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.33;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 1em 0em;
}

li.BLFa {
	font-weight : normal;
	font-style : normal;
	font-size : 1em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : -1em;
	margin-right : 0em;
	margin-top : 0.5em;
	margin-bottom : 0em;
	margin-left : 1em;
}
li.BLa {
	font-weight : normal;
	font-style : normal;
	font-size : 1em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : -1em;
	margin-right : 0em;
	margin-left : 1em;
	margin-top : 0em;
	margin-bottom : 0em;
}

li.BLLa {
	font-weight : normal;
	font-style : normal;
	font-size : 1em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : -1em;
	margin-right : 0em;
	margin-top : 0em;
	margin-bottom : 0.5em;
	margin-left : 1em;
}

li.BLF {
	font-weight : normal;
	font-style : normal;
	font-size : 1.0em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin-right : 0em;
	margin-top : 0.5em;
	margin-bottom : 0em;
	margin-left : 0em;
}
li.BL {
	font-weight : normal;
	font-style : normal;
	font-size : 1.0em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin-right : 0em;
	margin-left : 0em;
	margin-top : 0em;
	margin-bottom : 0em;
}

li.BLL {
	font-weight : normal;
	font-style : normal;
	font-size : 1.0em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin-right : 0em;
	margin-top : 0em;
	margin-bottom : 0.5em;
	margin-left : 0em;
}

.NLFa {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : -1em;
	margin : 1em 0em 0em 1em;
}
.NLF {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 1em 0em 0em 0em;
}
.NL {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 0em 0em;
}
.NLa {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : -1em;
	margin : 0em 0em 0em 1em;
}
.NLLa {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : -1em;
	margin : 0em 0em 1em 1em;
}
.NLL {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 1em 0em;
}

li.Box-BL, .Box-BL {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	text-indent : -1em;
	margin-left : 2em;
	color : #000000;
	margin-right : 1.2em;
	margin-top : 0em;
	margin-bottom : 0.5em;
}
.Box-NL {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : -1.2em;
	margin : 0em 1.2em 0.5em 2.2em;
}
.Recipe-Head {
	font-weight : bold;
	font-style : normal;
	font-size : 1.33em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.13;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 1em 0em 0.5em 0em;
}
.Recipe-caption {
	font-weight : normal;
	font-style : italic;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 0.2em 0em;
}
.Recipe-TXT {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0.2em 0em 0em 0em;
}
.Artline {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin-top : 1em;
	margin-bottom : 1em;
}

.IND-F {
	font-weight : normal;
	font-style : normal;
	font-size : 0.75em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.22;
	text-align : left;
	color : #000000;
	text-indent : -2em;
	margin : 1em 0em 0em 2em;
}
.IND-1 {
	font-weight : normal;
	font-style : normal;
	font-size : 0.75em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : -1.8em;
	margin : 0em 0em 0em 1.8em;
}
.IND-2 {
	font-weight : normal;
	font-style : normal;
	font-size : 0.75em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : -1em;
	margin : 0em 0em 0em 2.5em;
}
.IND-3 {
	font-weight : normal;
	font-style : normal;
	font-size : 0.75em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : -1em;
	margin : 0em 0em 0em 3.5em;
}

.IND-1F {
	font-weight : normal;
	font-style : normal;
	font-size : 0.75em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : -1.5em;
	margin : 1.2em 0em 0em 1.5em;
}

table.TABL {
	border-collapse : collapse;
	border-width : 0em;
	width : 100%;
	border-style : solid;
	border-color : #000000;
	margin-top : 1em;
	margin-bottom : 1em;
}
td.vert-top
{
	vertical-align : top;
}

.image-fig
{
margin-top : 1em;
margin-bottom : 0.5em;
text-align : center;
}

.tit_img
{
margin-top : 1em;
margin-bottom : 0em;
text-align : center;
}
.img
{
margin-top : 1em;
margin-bottom : 0em;
text-align : center;
}

img
{
max-width : 100%;
max-height : 100%;
}

a
{
text-decoration : none;
}
div.box
{
margin-top : 1em;
margin-bottom : 1em;
padding : 1em;
background-color : #D7D7D7;
}
div.box1
{
margin-top : 1em;
margin-bottom : 1em;
margin-left : 0em;
margin-right : 0em;
border : solid 0.1em;
padding-top: 0.5em;
padding-bottom: 0.5em;
padding-left: 0.5em;
padding-right: 0.5em;
}
div.box2
{
margin-top : 1em;
margin-bottom : 1em;
margin-left : 0em;
margin-right : 0em;
background-color : #ECECEE;
border : solid 0.1em;
padding-top: 0.5em;
padding-bottom: 0.5em;
padding-left: 0.5em;
padding-right: 0.5em;
}

div.box3
{
margin-top : 1em;
margin-bottom : 1em;
margin-left : 0em;
margin-right : 0em;
background-color : #ECECEE;
padding-top: 0.5em;
padding-bottom: 0.5em;
padding-left: 0.5em;
padding-right: 0.5em;
}

ul.ul {
font-weight : normal;
font-style : normal;
font-size :0.83em;
text-decoration : none;
font-variant : normal;
line-height : 1.4;
text-align : left;
color : #000000;
text-indent : 0em;
margin-top : 1em;
margin-bottom : 1em;
margin-left : 1em;
margin-right : 0em;
padding-left : 1em;
padding-right : 0em;
}


ol.ol
{
	font-weight : normal;
	font-style : normal;
	font-size : 0.83em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin-top : 1em;
	margin-bottom : 1em;
	margin-left : 1em;
	margin-right : 0em;
	padding-left : 1em;
	padding-right : 0em;
}

ul.ul-1 {
font-weight : normal;
font-style : normal;
font-size :0.83em;
text-decoration : none;
font-variant : normal;
line-height : 1.4;
text-align : left;
color : #000000;
text-indent :-1.3em;
margin-top : 0.3em;
margin-bottom : 0.00em;
margin-left : 1.3em;
padding-left : 0.00em;
list-style-type: none;
}

ol.ol-1 {
font-weight : normal;
font-style : normal;
font-size :0.83em;
text-decoration : none;
font-variant : normal;
line-height : 1.4;
text-align : left;
color : #000000;
text-indent :-1.3em;
margin-top : 1em;
margin-bottom : 1em;
margin-left : 1.3em;
padding-left : 1.00em;
list-style-type: none;
}


.FMT {
	font-weight : bold;
	font-style : normal;
	font-size : 1.8em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.17;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 2em 0em 3em 0em;
}
.FMT1 {
	font-weight : bold;
	font-style : normal;
	font-size : 1.8em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.17;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 2em 0em 0em 0em;
}

.page
{
page-break-before : always;
padding-top : 3.30em;
}

.EPG1 {
	color:#231f20;
	font-size:0.85em;
	font-style:normal;
	font-variant:normal;
	font-weight:normal;
	line-height:1.415;
	margin-bottom:0em;
	margin-top:0em;
	margin-left:1em;
	margin-right:0em;
	text-align:left;
	text-decoration:none;
	text-indent:0em;
}
.EPG {
	color:#231f20;
	font-size:0.85em;
	font-style:normal;
	font-variant:normal;
	font-weight:normal;
	line-height:1.415;
	margin-bottom:0em;
	margin-top:0em;
	margin-left:0em;
	margin-right:0em;
	text-align:left;
	text-decoration:none;
	text-indent:0em;
}

.C-Epg-Au {
	color:#231f20;
	font-size:0.85em;
	font-style:normal;
	font-variant:normal;
	font-weight:normal;
	line-height:1.415;
	margin-bottom:3.5em;
	margin-left:0em;
	margin-right:0em;
	margin-top:0em;
	text-align:center;
	text-decoration:none;
	text-indent:0em;
}

.gloss {
	font-weight : normal;
	font-style : normal;
	font-size : 0.83em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.22;
	text-align : left;
	color : #000000;
	text-indent : -1em;
	margin : 0em 0em 0em 1em;
}

.Bib-Glos-txt {
	font-weight : normal;
	font-style : normal;
	font-size : 0.75em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.22;
	text-align : left;
	color : #000000;
	text-indent : -1em;
	margin : 0em 0em 0em 1em;
}

.Biblio1 {
	color:#231f20;
	font-size:0.75em;
	font-style:normal;
	font-variant:normal;
	font-weight:300;
	line-height:1.222;
	margin-bottom:0em;
	margin-top:1em;
	margin-left:1em;
	margin-right:0em;
	text-align:left;
	text-decoration:none;
	text-indent:-1em;
}

.Biblio {
	color:#231f20;
	font-size:0.83em;
	font-style:normal;
	font-variant:normal;
	font-weight:300;
	line-height:1.222;
	margin-bottom:0em;
	margin-top:0em;
	margin-left:1em;
	margin-right:0em;
	text-align:left;
	text-decoration:none;
	text-indent:-1em;
}


.CA {
	font-size:1.2em;
	font-style:normal;
	font-variant:normal;
	line-height:1.091;
	text-align:center;
	text-decoration:none;
	text-indent:0em;
	margin : 0.5em 0em 3em 0em;
}


.PN {
	font-size:1.167em;
	font-style:italic;
	font-variant:normal;
	font-weight:normal;
	line-height:1.286;
	text-align:center;
	text-decoration:none;
	text-indent:0em;
	margin : 3em 0em 1em 0em;
}
.PT {
	
	color:#231f20;
	font-size:1.633em;
	font-style:normal;
	font-variant:normal;
	font-weight:bold;
	line-height:1.182;
	text-align:center;
	text-decoration:none;
	text-indent:0em;
	margin : 0em 0em 1.5em 0em;
	text-transform : uppercase;
}

.FM_Seriesh
{
font-weight : bold;
font-style : normal;
font-size : 1.05em;
text-decoration : none;
font-variant : normal;
line-height : 1.41;
text-align : center;
color : #000000;
text-indent : 0em;
margin : 3em 0em 1.0em 0em;
}
.FM_Seriesh1
{
font-weight : normal;
font-style : normal;
font-size : 0.85em;
text-decoration : none;
font-variant : normal;
line-height : 1.41;
text-align : center;
color : #000000;
text-indent : 0em;
margin : 1em 0em 1.0em 0em;
}
.FM_Seriest
{
font-weight : bold;
font-style : normal;
font-size : 0.88em;
text-decoration : none;
font-variant : normal;
line-height : 1.41;
text-align : left;
color : #000000;
text-indent : 0em;
margin : 1em 0em 0em 0em;
}
.FM_SeriestL
{
font-weight : bold;
font-style : normal;
font-size : 0.85em;
text-decoration : none;
font-variant : normal;
line-height : 1.41;
text-align : left;
color : #000000;
text-indent : 0em;
margin : 3em 0em 0.5em 0em;
}
.FM_Series
{
font-weight : normal;
font-style : normal;
font-size : 0.85em;
text-decoration : none;
font-variant : normal;
line-height : 1.41;
text-align : left;
color : #000000;
text-indent : 0em;
margin : 0em 0em 0em 0em;
}
.FM_SeriesL
{
font-weight : normal;
font-style : normal;
font-size : 0.85em;
text-decoration : none;
font-variant : normal;
line-height : 1.41;
text-align : left;
color : #000000;
text-indent : -1em;
margin : 0em 0em 0em 1em;
}

.TXT-con {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 1em 0em 0em 0em;
}


.TT {
	font-weight : bold;
	font-style : normal;
	font-size : 0.75em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.24;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 1em 0em 0.3em 0em;
}

.img-TT {
	font-weight : bold;
	font-style : normal;
	font-size : 0.71em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.24;
	text-align : justify;
	color : #000000;
	text-indent : 0em;
	margin : 1em 0em 0em 0em;
}

.TCH
{
	font-stretch : condensed;
	font-weight : normal;
	font-style : italic;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.57;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 0em 0em;
}
.TCHL
{
	font-stretch : condensed;
	font-weight : normal;
	font-style : italic;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.57;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 0em 0em;
}

.TCH1 {
	font-weight : normal;
	font-style : italic;
	font-size : 0.75em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.24;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0em;
}
.TB {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.24;
	text-align : left;
	color : #000000;
	text-indent : -1em;
	margin : 0em 0em 0em 1em;
}
.TBa {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.24;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 0em 0em;
}
.TBF {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.24;
	text-align : left;
	color : #000000;
	text-indent : -1em;
	margin : 0em 0em 0em 1em;
}
.TBFL {
	font-stretch : condensed;
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.33;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0em 0em 0.7em 0em;
	border-top: solid 0.1em;
	border-bottom: solid 0.1em;
}
.TBL {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.24;
	text-align : left;
	color : #000000;
	text-indent : -1em;
	margin : 0em 0em 0em 1em;
}

.TFN {
	font-weight : normal;
	font-style : normal;
	font-size : 0.75em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.24;
	text-align : left;
	color : #000000;
	text-indent : -0.5em;
	margin : 0em 0em 1em 0.5em;
}

.TB-cen {
	font-weight : normal;
	font-style : normal;
	font-size : 0.71em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.24;
	text-align : center;
	color : #000000;
	text-indent : -1em;
	margin : 0em 0em 0em 1em;
}

.TB-ext {
	font-weight : normal;
	font-style : normal;
	font-size : 0.71em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.24;
	text-align : left;
	color : #000000;
	text-indent : -1em;
	margin : 0em 0em 0em 2em;
}

td.TBF {
	padding-top : 0.3em;
	padding-bottom : 0.2em;
	padding-left : 0.3em;
	padding-right : 0.2em;
	vertical-align : top;
	border-top : solid 0.1em #000000;
}
td.TBF1 {
	padding-top : 0.3em;
	padding-bottom : 0.2em;
	padding-left : 0.3em;
	padding-right : 0.2em;
	vertical-align : top;
	border-top : solid 0.1em #000000;
	border-bottom : solid 0.1em #000000;
}
td.TB {
	padding-top : 0.2em;
	padding-bottom : 0.2em;
	padding-left : 0.3em;
	padding-right : 0.2em;
	vertical-align : top;
}
td.TBL
{
	padding-top : 0.2em;
	padding-bottom : 0.3em;
	padding-left : 0.3em;
	padding-right : 0.3em;
	vertical-align : top;
	border-bottom : solid 0.1em #000000;
}
th.TCH {
	padding-left : 0.3em;
	padding-right : 0.3em;
	vertical-align : bottom;
	border-top : solid 0.1em;
	font-weight: bold;
}
th.TCH1 {
	padding-left : 0.3em;
	padding-right : 0.3em;
	vertical-align : bottom;
	font-weight: bold;
}
td.TT {
	padding-top : 0em;
	padding-bottom : 0.2em;
	padding-left : 0em;
	padding-right : 0.2em;
	vertical-align : top;
}

.eqn-r {
	font-size : 0.88em;
	text-align : right;
	text-indent : 0em;
	margin : 0em;
}
td.t-eqn {
	font-size : 0.88em;
	text-align : center;
	text-indent : 0em;
	margin : 0em;
}			
td.t-eqn1 {
	font-size : 0.88em;
	text-align : right;
	text-indent : 0em;
	vertical-align : middle;
	margin : 0em 0em 0em 0em;
}
table.TABL1 {
	border-collapse : collapse;
	width : 100%;
	margin : 0.5em 0em 0.5em 0em;
}			

table.TABLB
{
border-collapse : collapse;
border: solid 0.1em;
margin : 0.5em 0em 1em 0em;
}

.Unicode
{
	font-family:"Courier Std";
}

ul.ul-Unicode
{
font-family:"Courier Std";
font-weight : normal;
font-style : normal;
font-size : 0.83em;
text-decoration : none;
font-variant : normal;
line-height : 1.4;
text-align : left;
color : #000000;
text-indent :-1.3em;
margin-top : 0.3em;
margin-bottom : 0.00em;
margin-left : 1.3em;
padding-left : 1.00em;
list-style-type: none;
}

			
h1 {
	font-weight : bold;
	font-style : normal;
	font-size : 1.5em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.13;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 2em 0em 0.5em 0em;
}
h2 {
	font-weight : normal;
	font-style : italic;
	font-size : 1em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.23;
	text-align : center;
	color : #000000;
	text-indent : 0em;
	margin : 0.5em 0em 1em 0em;
}

span.cs_bl
{
padding-right:1em;
font-weight:bold;
}
.TXTF {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : justify;
	color : #000000;
	text-indent : -4.6em;
	margin : 0em 0em 0em 4.6em;
}
.TT {
	font-weight : bold;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.24;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0.5em 0em 0.5em 0em;
}
.TT1 {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.24;
	text-align : left;
	color : #000000;
	text-indent : 0em;
	margin : 0.5em 0em 0.5em 0em;
}
.Copy-TXI1 {
	font-weight : normal;
	font-style : normal;
	font-size : 0.75em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.29;
	text-align : left;
	color : #000000;
	text-indent : -1em;
	margin : 0em 0em 0em 1em;
}
.BL {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : -0.6em;
	margin : 0em 0em 0em 0.8em;
}
.BLF {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : -0.6em;
	margin : 1em 0em 0em 0.8em;
}
.BLL {
	font-weight : normal;
	font-style : normal;
	font-size : 0.88em;
	text-decoration : none;
	font-variant : normal;
	line-height : 1.35;
	text-align : left;
	color : #000000;
	text-indent : -0.6em;
	margin : 0em 0em 1em 0.8em;
}
			</xsl:text>
		</xsl:result-document>
		<xsl:result-document href="{translate($bid,'-','')}/META-INF/container.xml" method="xml" encoding="UTF-8" standalone="yes">
			<container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
				<rootfiles>
					<rootfile full-path="OEBPS/content.opf" media-type="application/oebps-package+xml"/>
				</rootfiles>
			</container>
		</xsl:result-document>
		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/xhtml/cover.xhtml" method="xml" indent="yes">
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<meta charset="utf-8"/>
					<title>Cover</title>
				</head>
				<body epub:type="cover" id="cover-page">
					<img id="cover-image" role="doc-cover" src="../images/cover.jpg" alt=""/>
				</body>
			</html>
		</xsl:result-document>
		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/xhtml/index.xhtml" method="xml" indent="yes" use-character-maps="hex">
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<title>Index</title>
					<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
				</head>
				<body epub:type="backmatter">
					<section>
						<h1 epub:type="title" class="FMT">	
						<a id="index" href="contents.xhtml#re_index">Index</a>
						</h1>
						<ol class="ol-1">
							<xsl:apply-templates select="descendant::db:indexterm" mode="index"></xsl:apply-templates>
						</ol>
					</section>
				</body>
			</html>			
		</xsl:result-document>
		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/xhtml/toc.xhtml" method="xml" indent="yes" use-character-maps="hex">
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<meta charset="utf-8"/>
					<title>Contents</title>
					<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
				</head>
				<body epub:type="frontmatter">
					<nav xmlns:epub="http://www.idpf.org/2007/ops" epub:type="toc" id="toc" role="doc-toc">
						<ol class="CT-TOC1">
							<li>
								<a href="cover.xhtml#cover-page">Cover</a>
							</li>
							<li>
								<a href="halftitle.xhtml#half_1">Half-Title</a>
							</li>
							<xsl:if test="//db:cover">
								<li>
									<a href="series.xhtml#series_1">Series</a>
								</li>
							</xsl:if>
							<li>
								<a href="title.xhtml#title_1">Title</a>
							</li>
							<li>
								<a href="copyright.xhtml#copy">Copyright</a>
							</li>
							<xsl:if test="//db:dedication">
								<li>
									<a href="dedication.xhtml#ded">Dedication</a>
								</li>
							</xsl:if>
							<li>
								<a href="contents.xhtml#re_contents">Contents</a>
							</li>
							
							<xsl:for-each select="//db:preface|//db:acknowledgements|db:abbreviation">
								<li>
									<a href="{substring-after(substring-after(@xml:id,'-'),'-')}.xhtml#{substring-after(substring-after(@xml:id,'-'),'-')}">
										<xsl:value-of select="db:info/db:title"/>
									</a>
								</li>
							</xsl:for-each>
							<xsl:for-each select="//db:chapter[not(@role='notes' or contains(@xml:id, 'note'))]|//db:appendix|//db:part[child::db:label]|//db:index">
								<li>
									<a href="{substring-after(substring-after(@xml:id,'-'),'-')}.xhtml#{substring-after(substring-after(@xml:id,'-'),'-')}">
										<xsl:value-of select="@label"/>
										<xsl:text> </xsl:text>
										<xsl:apply-templates select="db:info/db:title" mode="toc"/>
										<xsl:if test="db:info/db:subtitle">
											<xsl:apply-templates select="db:info/db:subtitle" mode="toc"/>
										</xsl:if>
									</a>
									
									
									<xsl:if test="db:section|db:bibliography">
										<xsl:if test="$toclevel=1">
											<xsl:for-each select="db:section">
												<li>
													<xsl:apply-templates select="db:info/db:title" mode="toc"/>
												</li>
											</xsl:for-each>
										</xsl:if>
										<xsl:if test="$toclevel=2">
											<xsl:for-each select="db:section/db:section">
												<li>
													<xsl:apply-templates select="db:info/db:title" mode="toc"/>
												</li>
											</xsl:for-each>
										</xsl:if>
										<xsl:if test="$toclevel=3">
											<ol>
												<xsl:for-each select="db:section[child::db:info/db:title]|db:section/db:section[child::db:info/db:title]|db:section/db:section/db:section[child::db:info/db:title]">
													<li>
														<xsl:apply-templates select="db:info/db:title|db:bibliography/db:title" mode="toc"/>
													</li>
												</xsl:for-each>
												<xsl:for-each select="db:bibliography">
													<li>
														<a href="{substring-after(substring-after(./parent::db:chapter/@xml:id,'-'),'-')}.xhtml#{./@xml:id}">
															<xsl:apply-templates select="./db:title" mode="toc"/>
														</a>
													</li>
												</xsl:for-each>
											</ol>
										</xsl:if>
									</xsl:if>
									<!--<xsl:if test="db:bibliography">
										<li>
											<a href="{substring-after(substring-after(@xml:id,'-'),'-')}.xhtml#{./db:bibliography/@xml:id}">
												<xsl:apply-templates select="./db:bibliography/db:title" mode="toc"/>
											</a>
										</li>
									</xsl:if>-->
								</li>
							</xsl:for-each>
							<xsl:if test="//db:footnote[@role='end-bk1-note']">
								<li>
									<a href="notes.xhtml#notes">Notes</a>
								</li>
							</xsl:if>
							
							<xsl:if test="//db:bibliography[not(ancestor::db:chapter)]">
								<li>
									<a href="bibliography.xhtml#bib">
										<xsl:value-of select="//db:bibliography[not(ancestor::db:chapter)]/db:info/db:title"/>
									</a>
									<xsl:if test="//db:bibliography[not(ancestor::db:chapter)]/db:bibliodiv">
										<ol class="CL-TOC2">
											<xsl:if test="$toclevel=1">
												<xsl:for-each select="//db:bibliography[not(ancestor::db:chapter)]/db:bibliodiv">
													<li>
														<xsl:apply-templates select="db:info/db:title" mode="toc"/>
													</li>
												</xsl:for-each>
											</xsl:if>
											<xsl:if test="$toclevel=2">
												<xsl:for-each select="//db:bibliography[not(ancestor::db:chapter)]/db:bibliodiv|//db:bibliography[not(ancestor::db:chapter)]/db:bibliodiv/db:bibliodiv">
													<li>
														<xsl:apply-templates select="db:info/db:title" mode="toc"/>
													</li>
												</xsl:for-each>
											</xsl:if>
											<xsl:if test="$toclevel=3">
												<xsl:for-each select="//db:bibliography[not(ancestor::db:chapter)]/db:bibliodiv|//db:bibliography[not(ancestor::db:chapter)]/db:bibliodiv/db:bibliodiv|//db:bibliography[not(ancestor::db:chapter)]/db:bibliodiv/db:bibliodiv/db:bibliodiv">
													<li>
														<xsl:apply-templates select="db:info/db:title" mode="toc"/>
													</li>
												</xsl:for-each>
											</xsl:if>
										</ol>
									</xsl:if>
								</li>
							</xsl:if>
						<!--	<xsl:if test="//db:indexterm">
								<li>
									<a href="index.xhtml#index">Index</a>
								</li>
							</xsl:if>-->
							
						</ol>
					</nav>
					
				</body>
			</html>
		</xsl:result-document>
		
				
		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/toc.ncx" method="xml" indent="yes" use-character-maps="hex"> 
			<ncx xmlns="http://www.daisy.org/z3986/2005/ncx/" version="2005-1" xml:lang="en">
				<head>
					<meta name="dtb:uid" content="ISBN:{$bid}"/>
					<meta name="dtb:depth" content="1"/>
					<meta name="dtb:totalPageCount" content=""/>
					<meta name="dtb:maxPageNumber" content="0"/>
				</head>
				<docTitle>
					<text>
						<xsl:apply-templates select="db:book/db:info/db:title" mode="ncx"/>
					</text>
				</docTitle>
				<xsl:copy>
					<xsl:apply-templates select="exsl:node-set($intncx)"/>
				</xsl:copy>
			</ncx>
		</xsl:result-document>
		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/xhtml/contents.xhtml" method="xml" indent="yes" use-character-maps="hex">
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<meta charset="utf-8"/>
					<title>Contents</title>
					<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
				</head>
				<body epub:type="frontmatter">
					<section>
						<h1 epub:type="title" class="FMT" id="re_contents">Contents</h1>
					<!--<xsl:if test="//db:acknowledgements">
						<p class="TOC-FM">
							<a href="ack.xhtml#ack" id="re_ack">Acknowledgements</a>
						</p>
					</xsl:if>-->
						<xsl:for-each select="//db:preface|//db:acknowledgements|//db:abbreviation">
						<p class="TOC-FM">
							<a href="{substring-after(substring-after(@xml:id,'-'),'-')}.xhtml#{substring-after(substring-after(@xml:id,'-'),'-')}" id="re_{substring-after(substring-after(@xml:id,'-'),'-')}">
								<xsl:if test="@label">
									<xsl:value-of select="@label"/>
									<xsl:text> </xsl:text>	
								</xsl:if>
								<xsl:value-of select="db:info/db:title"/>
							</a>
						</p>
					</xsl:for-each>
					<xsl:for-each select="//db:chapter[not(@role='notes' or contains(@xml:id, 'note'))]|//db:appendix|//db:part[child::db:label]|//db:index">
						<xsl:variable name="classattrib">
						<xsl:choose>
							<xsl:when test="name()='part'">TOC-PT</xsl:when>
							<xsl:otherwise>TOC-CH</xsl:otherwise>
						</xsl:choose>
						</xsl:variable>
						<p class="{$classattrib}">
							<a href="{substring-after(substring-after(@xml:id,'-'),'-')}.xhtml#{substring-after(substring-after(@xml:id,'-'),'-')}" id="re_{substring-after(substring-after(@xml:id,'-'),'-')}">
								<span class="cs_bl"><xsl:value-of select="@label"/><xsl:value-of select="child::db:label"/></span>
								<xsl:text> </xsl:text>
								<xsl:apply-templates select="db:info/db:title" mode="toc"/>
								<xsl:if test="db:info/db:subtitle">
									<xsl:apply-templates select="db:info/db:subtitle" mode="toc"/>
								</xsl:if>	
							</a>
						</p>
						<xsl:if test="db:info[child::db:author]">
							<p class="TOC-CA">
								<xsl:value-of select="db:info[child::db:author]"></xsl:value-of>
							</p>
						</xsl:if>
						<xsl:if test="db:section">
							<xsl:for-each select=".//child::db:section">
								
								<xsl:variable name="seclevel">
									<xsl:value-of select="substring-after(./@role,'H')"></xsl:value-of>
								</xsl:variable>
								<p class="TOC-CH-sec{$seclevel}" id="re_{@xml:id}">
									<xsl:apply-templates select="db:info/db:title" mode="toc"/>
								</p>
							</xsl:for-each>
						</xsl:if>
						<xsl:if test="db:bibliography">
							<p class="TOC-CH-sec1" id="re_{./db:bibliography/@xml:id}">
								<a href="{substring-after(substring-after(@xml:id,'-'),'-')}.xhtml#{./db:bibliography/@xml:id}">
									<xsl:apply-templates select="./db:bibliography/db:title" mode="toc"/>
								</a>
							</p>
						</xsl:if>
					</xsl:for-each>
					<xsl:if test="//db:footnote[@role='end-bk1-note']">
						<p class="TOC-CH">
							<a href="notes.xhtml#notes" id="re_notes">Notes</a>
						</p>
					</xsl:if>
					<xsl:if test="//db:bibliography[not(ancestor::db:chapter)]/db:info/db:title">
						<p class="TOC-CH">
							<a href="bibliography.xhtml#bib" id="re_bib">
								<xsl:value-of select="//db:bibliography[not(ancestor::db:chapter)]/db:info/db:title"/>
							</a>
						</p>
					</xsl:if>
					<!--<xsl:if test="//db:indexterm">
						<p class="TOC-CH">
							<a href="index.xhtml#index" id="re_index">Index</a>
						</p>
					</xsl:if>-->
					</section>
				</body>
			</html>
		</xsl:result-document>
		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/content.opf" method="xml" indent="yes" use-character-maps="hex">
			<package xmlns="http://www.idpf.org/2007/opf" version="3.0" unique-identifier="p{translate($bid,'-','')}">
				<metadata xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:opf="http://www.idpf.org/2007/opf">
					<dc:identifier id="p{translate($bid,'-','')}"><xsl:value-of select="concat('ISBN:',$bid)"/></dc:identifier>
					<dc:title>
						<xsl:apply-templates select="db:book/db:info/db:title" mode="opf"/>
					</dc:title>
					<dc:format>application/epub</dc:format>
					<dc:language>en-US</dc:language>
					<dc:creator>
						<xsl:apply-templates select="//db:authorgroup[parent::db:info]"/>
					</dc:creator>
					<dc:publisher>
						<xsl:value-of select="//db:info/db:bibliomisc[@role='imprint']"/>
					</dc:publisher>
					<dc:date><xsl:value-of select="format-date(current-date(), '[Y0001]-[M01]-[D01]')"/></dc:date>
					<meta property="dcterms:modified">
						<xsl:value-of select="format-dateTime(current-dateTime(),'[Y0001]-[M01]-[D01]T[H01]:[m01]:[s01]Z')"/>
					</meta>
					<meta name="cover" content="cover-image"/>
				</metadata>
				<manifest>
					<item id="ncx" href="toc.ncx" media-type="application/x-dtbncx+xml"/>
					<item id="toc" properties="nav" href="xhtml/toc.xhtml" media-type="application/xhtml+xml"/>
					<item id="stylesheet" href="styles/stylesheet.css" media-type="text/css"/>
					<item id="cover-page" href="xhtml/cover.xhtml" media-type="application/xhtml+xml"/>
					<item id="halftitle" href="xhtml/halftitle.xhtml" media-type="application/xhtml+xml"/>
					<xsl:if test="//db:cover">
						<item id="series" href="xhtml/series.xhtml" media-type="application/xhtml+xml"/>
					</xsl:if>
					<xsl:if test="//db:dedication">
						<item id="ded" href="xhtml/dedication.xhtml" media-type="application/xhtml+xml"/>
					</xsl:if>
					<xsl:if test="//db:glossary">
						<item id="glo" href="xhtml/glossary.xhtml" media-type="application/xhtml+xml"/>
					</xsl:if>
					<xsl:if test="//db:abbreviation">
						<item id="glo" href="xhtml/abbrev.xhtml" media-type="application/xhtml+xml"/>
					</xsl:if>
					<item id="title" href="xhtml/title.xhtml" media-type="application/xhtml+xml"/>
					<item id="copyright" href="xhtml/copyright.xhtml" media-type="application/xhtml+xml"/>
					<item id="contents" href="xhtml/contents.xhtml" media-type="application/xhtml+xml"/>
					<xsl:if test="//db:acknowledgements">
						<item id="ack" href="xhtml/ack.xhtml" media-type="application/xhtml+xml"/>
					</xsl:if>
					<xsl:for-each select="//db:preface">
						<item id="{substring-after(substring-after(@xml:id,'-'),'-')}" href="xhtml/{substring-after(substring-after(@xml:id,'-'),'-')}.xhtml" media-type="application/xhtml+xml"/>
					</xsl:for-each>
					<xsl:for-each select="//db:chapter[not(@role='notes' or contains(@xml:id, 'note'))]|//db:appendix|//db:part[child::db:label]">
						<item id="{substring-after(substring-after(@xml:id,'-'),'-')}" href="xhtml/{substring-after(substring-after(@xml:id,'-'),'-')}.xhtml" media-type="application/xhtml+xml"/>
					</xsl:for-each>
					<xsl:if test="//db:footnote[@role='end-bk1-note']">
						<item id="notes" href="xhtml/notes.xhtml" media-type="application/xhtml+xml"/>
					</xsl:if>
					<xsl:if test="//db:bibliography[not(ancestor::db:chapter)]">
						<item id="bib" href="xhtml/bibliography.xhtml" media-type="application/xhtml+xml"/>
					</xsl:if>
					<item id="index" href="xhtml/index.xhtml" media-type="application/xhtml+xml"/>
					
					<xsl:for-each-group select="//db:figure/db:mediaobject/db:imageobject/db:imagedata" group-by="@fileref">
						<item id="{substring-after(@fileref,'/')}" href="{@fileref}" media-type="{@format}"/>
					</xsl:for-each-group>
					<item id="cover-image" href="images/cover.jpg" media-type="image/jpeg"/>
				</manifest>
				<spine toc="ncx">
					<itemref idref="cover-page"/>
					<itemref idref="halftitle"/>
					<xsl:if test="//db:cover">
						<itemref idref="series"/>
					</xsl:if>
					<xsl:if test="//db:glossary">
						<itemref idref="glo"/>
					</xsl:if>
					<itemref idref="title"/>
					<itemref idref="copyright"/>
					<xsl:if test="//db:dedication">
						<itemref idref="ded"/>
					</xsl:if>
					<itemref idref="contents"/>
					<xsl:if test="//db:acknowledgements">
						<itemref idref="ack"/>
					</xsl:if>
					<xsl:for-each select="//db:preface">
						<itemref idref="{substring-after(substring-after(@xml:id,'-'),'-')}"/>
					</xsl:for-each>
					<xsl:for-each select="//db:chapter[not(@role='notes' or contains(@xml:id, 'note') or contains(@xml:id, 'about'))]|//db:appendix|//db:part[child::db:label]"> 
						<itemref idref="{substring-after(substring-after(@xml:id,'-'),'-')}"/>
					</xsl:for-each>
					<xsl:if test="//db:footnote[@role='end-bk1-note']">
						<itemref idref="notes"/>						
					</xsl:if>					
					<xsl:if test="//db:bibliography[not(ancestor::db:chapter)]">
						<itemref idref="bib"/>
					</xsl:if>
					<itemref idref="index"/>
					<itemref idref="about"/> 
					
				</spine>
				<guide>
					<reference type="text" title="Half-Title" href="xhtml/halftitle.xhtml"></reference>
					<reference type="title" title="Title Page" href="xhtml/title.xhtml"></reference>
					<reference type="copyright-page" title="Copyright" href="xhtml/copyright.xhtml"></reference>
					<reference type="toc" title="Table of Contents" href="xhtml/contents.xhtml"></reference>
				</guide>
			</package>
		</xsl:result-document>
		
	
		
	</xsl:template>
	<xsl:template match="db:biblioid|db:pagenums"/>
	
	<xsl:template match="db:chapter[contains(@xml:id,'note')]">
		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/xhtml/notes.xhtml" method="xml" indent="yes" use-character-maps="hex">				
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<title>Notes</title>
					<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
				</head>
				<body epub:type="bodymatter">																		
					<section>
						<h1 class="FMT">
							<a id="notes" href="contents.xhtml#re_notes">Notes</a>
						</h1>
						<xsl:for-each select="db:section">
							<section>
								<h2 class="H1">
									<xsl:apply-templates select="./db:info/db:title"/>
								</h2>
								<ol class="ol-1">
									<xsl:apply-templates select=".//db:footnote/node()" mode="note"/>
								</ol>
							</section>
						</xsl:for-each>
					</section>
				</body>
			</html>			
		</xsl:result-document>
	</xsl:template>
	
	
	<xsl:template match="db:chapter[not(contains(@xml:id, 'note'))]|db:appendix|db:part">
		<xsl:for-each select="self::db:chapter|self::db:appendix|self::db:part">
			<xsl:result-document method="xml" href="{translate($bid,'-','')}/OEBPS/xhtml/{substring-after(substring-after(@xml:id,'-'),'-')}.xhtml" encoding="utf-8"  indent="yes" use-character-maps="hex">
				<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
				<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
					<head>
						<title>
							<xsl:choose>
								<xsl:when test="@label">
									<xsl:text>Chapter </xsl:text>
									<xsl:value-of select="@label"/> - <xsl:value-of select="db:info/db:title"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="db:info/db:title"/>
								</xsl:otherwise>
							</xsl:choose>
						</title>
						<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
					</head>
					<xsl:variable name="chapid"><xsl:value-of select="substring-after(substring-after(@xml:id,'-'),'-')"></xsl:value-of></xsl:variable>
					<body> 
					<xsl:choose>
						<xsl:when test="contains($chapid,'bib') or contains($chapid,'about')">
							<xsl:attribute name="epub:type">backmatter</xsl:attribute>
						</xsl:when>
						<xsl:otherwise>
							<xsl:attribute name="epub:type">bodymatter</xsl:attribute>
						</xsl:otherwise>
					</xsl:choose>
						<xsl:variable name="chaprole">
							<xsl:choose>
								<xsl:when test="$chapid='intro'">introduction</xsl:when>
								<xsl:when test="$chapid='conc'">conclusion</xsl:when>
								<xsl:when test="contains($chapid,'part')">part</xsl:when>
								<xsl:when test="contains($chapid,'bib')">bibliography</xsl:when>
								<xsl:when test="contains($chapid,'about')">bibliography</xsl:when>
								<xsl:otherwise>chapter</xsl:otherwise>
							</xsl:choose>
						</xsl:variable>
						
						<!--<section epub:type="{$chaprole}" role="doc-{$chaprole}">-->
						<section>
							<!--<xsl:attribute name="epub:type"><xsl:value-of select="$chaprole"/></xsl:attribute>
							<xsl:if test="not (contains($chapid,'bib'))">
								<xsl:attribute name="role"><xsl:value-of select="$chaprole"/></xsl:attribute>
							</xsl:if>-->
							<xsl:apply-templates select="node() except (db:info[child::db:author],db:bibliography,.//db:section[@id='1'])" />
							
						<xsl:if test="descendant::db:footnote[@role='end-bk-note']">
							<section>
							<h2>
								<xsl:choose>
									<xsl:when test="count(./descendant::db:footnote[@role='end-bk-note'])=1">
										<xsl:text>Note</xsl:text>		
									</xsl:when>
									<xsl:otherwise>
										<xsl:text>Notes</xsl:text>
									</xsl:otherwise>
								</xsl:choose>
							</h2>
							<ol class="ol-1">
								<xsl:apply-templates select="descendant::db:footnote[@role='end-bk-note']" mode="cnote"/>
							</ol>
							</section>
						</xsl:if>
							<xsl:apply-templates select="db:bibliography|.//db:section[@id='1']"/>
						</section>
					</body>
				</html>			
			</xsl:result-document>
		</xsl:for-each>
	</xsl:template>
	
		
	<xsl:template match="db:chapter/db:info[db:title]|db:appendix/db:info|//db:abbreviation/db:info[db:title]">
		<h1 class="FMT">
			<xsl:apply-templates select="processing-instruction('page')"/>
			<a id="{substring-after(substring-after((ancestor::db:chapter|ancestor::db:abbreviation|ancestor::db:appendix)/@xml:id,'-'),'-')}" href="contents.xhtml#re_{substring-after(substring-after((ancestor::db:chapter|ancestor::db:abbreviation|ancestor::db:appendix)/@xml:id,'-'),'-')}">
				<xsl:choose>
					<xsl:when test=".//parent::db:info/parent::db:chapter/@label">
						<span epub:type="ordinal"><i>Chapter <xsl:value-of select=".//parent::db:info/parent::db:chapter/@label"/></i></span><br/><xsl:text disable-output-escaping="yes"> </xsl:text><xsl:apply-templates select="node() except (processing-instruction('page'),db:footnote,db:subtitle)"/>	
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="node() except (processing-instruction('page'),db:footnote,db:subtitle)"/>
					</xsl:otherwise>
				</xsl:choose>
			</a>
			<xsl:apply-templates select="db:footnote"/>
		</h1>
		<xsl:if test="./child::db:subtitle">
			<h2 class="CST1" epub:type="subtitle" role="doc-subtitle">
				<a href="contents.xhtml#re_{substring-after(substring-after(ancestor::db:chapter/@xml:id,'-'),'-')}">
					<xsl:apply-templates select="./child::db:subtitle"/>
				</a>
			</h2>	
		</xsl:if>
		<xsl:apply-templates select="./following-sibling::db:info[child::db:author]"/>
	</xsl:template>

	<xsl:template match="//db:chapter/db:info/db:label" />
	<xsl:template match="//db:part/db:label" />
	<xsl:template match="db:part[child::db:label]/db:info/db:title">
		<xsl:if test="./ancestor::db:part/child::db:label">
			<h1 class="PN">
				<xsl:apply-templates select="processing-instruction('page')"/>
				<a href="contents.xhtml#re_{substring-after(substring-after(ancestor::db:part/@xml:id,'-'),'-')}" id="{substring-after(substring-after(ancestor::db:part/@xml:id,'-'),'-')}"><xsl:value-of select="./ancestor::db:part/child::db:label"/></a>
			</h1>
		</xsl:if>
		<h2 epub:type="title" class="PT">
			<a href="contents.xhtml#re_{substring-after(substring-after(ancestor::db:part/@xml:id,'-'),'-')}">
				<xsl:apply-templates select="node() except (processing-instruction('page'),db:footnote)"/>
			</a>
		</h2>
	</xsl:template>
	<!--<xsl:template match="db:chapter/db:info/db:subtitle">
		<p class="CST">
			<a href="contents.xhtml#re_{substring-after(substring-after(ancestor::db:chapter/@xml:id,'-'),'-')}">
				<xsl:apply-templates/>
			</a>
		</p>
	</xsl:template>-->
	<xsl:template match="db:chapter/db:info/db:subtitle" mode="toc">
		<xsl:text>: </xsl:text>
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="db:chapter/db:info/db:title|db:part/db:info/db:title|db:appendix/db:info/db:title|db:index/db:info/db:title" mode="toc">
		<xsl:text> </xsl:text>
		<xsl:apply-templates select="node() except (processing-instruction(), db:footnote)"/>
	</xsl:template>
	<xsl:template match="db:chapter/db:info/db:title|db:appendix/db:info/db:title" mode="ncx">
		<xsl:value-of select="ancestor::db:chapter/@label|ancestor::db:appendix/@label"/>
		<xsl:text> </xsl:text>
		<xsl:value-of select="."/>
		<xsl:for-each select="following-sibling::db:authorgroup/db:author">
			<xsl:choose>
				<xsl:when test="position() = last() and not(position()=1)">
					<xsl:text>and </xsl:text>
					<xsl:apply-templates/>			
				</xsl:when>
				<xsl:when test="position() = last() - 1  and position()=1">
					<xsl:apply-templates/>			
				</xsl:when>
				<xsl:when test="position() = last()  and position()=1">
					<xsl:apply-templates/>			
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates/>
					<xsl:text>, </xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>
	<xsl:template match="db:part/db:info/db:title" mode="ncx">
		<xsl:value-of select="@label"/>
		<xsl:text>PART </xsl:text>
		<xsl:value-of select="."/>
		<xsl:for-each select="following-sibling::db:authorgroup/db:author">
			<xsl:choose>
				<xsl:when test="position() = last() and not(position()=1)">
					<xsl:text>and </xsl:text>
					<xsl:apply-templates/>			
				</xsl:when>
				<xsl:when test="position() = last() - 1 and not(position()=1)">
					<xsl:apply-templates/>			
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates/>
					<xsl:text>, </xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>
	<xsl:template match="db:epigraph/db:para">
		<p class="EPG">
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	
	
	<!--<xsl:template match="db:dedication/db:para">
		<p class="Dedi-TXT1" id="ded_1">
			<xsl:apply-templates select="processing-instruction('page')"/>
			<xsl:apply-templates select="node() except processing-instruction('page')"/>
		</p>
	</xsl:template>-->
	
	<!--<xsl:template match="db:tp">
		<xsl:variable name="tppos">
			<xsl:choose>
				<xsl:when test="count(./preceding-sibling::db:tp) = 0">TBF</xsl:when>
				<xsl:when test="((count(./following-sibling::db:tp)!=0) and (count(./preceding-sibling::db:tp)!=0))">TB</xsl:when>
				<xsl:otherwise>TBL</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<p class="{$tppos}">
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	-->
	
	<xsl:template match="db:attribution">
		<p class="C-Epg-Au">
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	<xsl:template match="db:source" mode="#all">
		<p class="C-Epg-Au">
			<xsl:apply-templates/>
		</p>
	</xsl:template>

	<xsl:template match="db:sidebar">
		<div class="box1">
		
		
			<xsl:apply-templates/>
		
		</div>
	</xsl:template>
	
	<xsl:template match="db:example">
		<div class="example">
			<xsl:apply-templates />
		</div>
	</xsl:template>
	
	<xsl:template match="db:example/db:info/db:title" >
		<p class="EXTF">
			<xsl:apply-templates />
		</p>
	</xsl:template>
		
	<xsl:template match="db:sidebar/db:info/db:title">
		<p class="box_h">
			
			
			<xsl:apply-templates/>
			
		</p>
	</xsl:template>

	
	<xsl:template match="db:epigraph">
		<blockquote epub:type="epigraph" role="doc-epigraph">
		<xsl:apply-templates select="db:para, db:attribution"/>
		</blockquote>
	</xsl:template>
	<xsl:template match="processing-instruction('page')" mode="#all">
		<span epub:type="pagebreak" role="doc-pagebreak" aria-label="{substring-before(substring-after(.,'value=&quot;'),'&quot;')}" id="{concat('p',substring-before(substring-after(.,'value=&quot;'),'&quot;'))}"/>
	</xsl:template>
	
	
	<xsl:template match="db:para" mode="#all">
		
		<xsl:choose>
			<xsl:when test="./parent::db:blockquote">
				<xsl:apply-templates/>
			</xsl:when>
			<xsl:when test="./parent::db:listitem">
				<xsl:apply-templates/>
			</xsl:when>
			<xsl:when test="./preceding-sibling::*[1][local-name()='blockquote']">
				<p>
					<xsl:attribute name="class">TXT</xsl:attribute>
					<xsl:apply-templates/>
				</p>
			</xsl:when>
			<xsl:when test="./parent::db:footnote[@role='end-ch-note']">
				<xsl:apply-templates/>
			</xsl:when>
			<xsl:when test="./preceding-sibling::*[1][local-name()='dialogue']">
				<p>
					<xsl:attribute name="class">TXT</xsl:attribute>
					<xsl:apply-templates/>
				</p>
			</xsl:when>
			<xsl:when test="./preceding-sibling::*[1][local-name()='orderedlist']">
				<p>
					<xsl:attribute name="class">TXT</xsl:attribute>
					<xsl:apply-templates/>
				</p>
			</xsl:when>
			<xsl:when test="./parent::db:example">
				<xsl:choose>
					<xsl:when test="count(./parent::db:example/child::para) = 1">
						<p>
							<xsl:attribute name="class">EXT-F</xsl:attribute>
							<xsl:apply-templates/>
						</p>
					</xsl:when>
					<xsl:otherwise>
						<xsl:choose>
							<xsl:when test="position()=2">
								<p>
									<xsl:attribute name="class">EXT-F</xsl:attribute>
									<xsl:apply-templates/>
								</p>	
							</xsl:when>
							<xsl:when test="position() = last()">
								<p>
									<xsl:attribute name="class">EXT-L</xsl:attribute>
									<xsl:apply-templates/>
								</p>	
							</xsl:when>
							<xsl:otherwise>
								<p>
									<xsl:attribute name="class">EXT-M</xsl:attribute>
									<xsl:apply-templates/>
								</p>	
							</xsl:otherwise>
						</xsl:choose>		
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:when test="./preceding-sibling::*[1][local-name()='itemizedlist']">
				<p>
					<xsl:attribute name="class">TXT</xsl:attribute>
					<xsl:apply-templates/>
				</p>
			</xsl:when>
			<xsl:when test="./parent::db:glossdef">
				<xsl:apply-templates/>
			</xsl:when>
			<xsl:when test="./parent::db:dedication and position()=1">
				<p class="Dedi-TXT1" id="ded">
					<xsl:apply-templates select="processing-instruction('page')"/>
					<xsl:apply-templates select="node() except processing-instruction('page')"/>
				</p>
			</xsl:when>
			<xsl:when test="./parent::db:dedication and position()&gt;1">
				<p class="Dedi-TXT">
					<xsl:apply-templates select="processing-instruction('page')"/>
					<xsl:apply-templates select="node() except processing-instruction('page')"/>
				</p>
			</xsl:when>
				
			<xsl:otherwise>
				<p>
					<xsl:choose>
						<xsl:when test="contains(./ancestor::db:chapter/@xml:id,'abocon')">
							<xsl:attribute name="class">TXT-con</xsl:attribute>
							<xsl:if test="@role and @role!='TXT'">
								<xsl:attribute name="role"><xsl:value-of select="@role"/></xsl:attribute>
							</xsl:if>
							<xsl:apply-templates/>
						</xsl:when>
						<xsl:otherwise>
							
							<xsl:choose>
								<xsl:when test="@role='bib_text'">
									<xsl:attribute name="class">Biblio</xsl:attribute>		
								</xsl:when>
								<xsl:otherwise>
									<xsl:attribute name="class">TXI</xsl:attribute>
									<xsl:if test="@role and @role!='TXT'">
										<xsl:attribute name="role"><xsl:value-of select="@role"/></xsl:attribute>	
									</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
							<xsl:apply-templates/>		
						</xsl:otherwise>
					</xsl:choose>
				</p>		
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<xsl:template match="db:chapter/db:para[1]|db:section/db:para[1]|db:acknowledgements/db:para[1]">
		<xsl:choose>
			<xsl:when test="contains(./ancestor::db:chapter/@xml:id,'abocon')">
				<p class="TXT-con">
					<xsl:if test="@role and @role!='TXT'">
						<xsl:attribute name="role"><xsl:value-of select="@role"/></xsl:attribute>
					</xsl:if>
					<xsl:apply-templates/>
				</p>
			</xsl:when>
			<xsl:otherwise>
				<p class="TXT">
					<xsl:choose>
						<xsl:when test="@role='bib_text'">
							<xsl:attribute name="class">Biblio</xsl:attribute>		
						</xsl:when>
						<xsl:otherwise>
							<xsl:attribute name="class">TXT</xsl:attribute>
							<xsl:if test="@role and @role!='TXT'">
								<xsl:attribute name="role"><xsl:value-of select="@role"/></xsl:attribute>	
							</xsl:if>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:apply-templates/>
				</p>		
			</xsl:otherwise>
		</xsl:choose>
		
	</xsl:template>


	<xsl:template match="db:blockquote/db:source">
		<p class="BS">
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	
	<!--<xsl:template match="db:figure/db:figsource">
		<p class="FS">
			<xsl:apply-templates/>
		</p>
	</xsl:template>-->
	
	
	
	<xsl:template match="db:table/db:tblsource">
		<tfoot>
			<tr>
				<td colspan="{count(./ancestor::db:table/db:tgroup/child::db:colspec)}"><xsl:apply-templates/></td>
			</tr>
		</tfoot>
	</xsl:template>
	<xsl:template match="db:table/db:tblfn">
		<p class="TFN">
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	


	<xsl:template match="db:emphasis" mode="#all">
		<xsl:choose>
			<xsl:when test="@role='italic'">
				<i><xsl:apply-templates/></i>		
				<!--<xsl:choose>
					<xsl:when test="./parent::db:title">
						<cite><xsl:apply-templates/></cite>
					</xsl:when>
					<xsl:when test="./ancestor::db:caption">
						<cite><xsl:apply-templates/></cite>
					</xsl:when>
					<xsl:otherwise>
						<i><xsl:apply-templates/></i>		
					</xsl:otherwise>
				</xsl:choose>-->
				
			</xsl:when>
			<xsl:when test="@role='bold'">
				<b><xsl:apply-templates/></b>
			</xsl:when>	
			<xsl:when test="@role='underline'">
				<span class="underline">
					<xsl:apply-templates/>
				</span>
			</xsl:when>
			<xsl:when test="@role='smallcaps'">
				<span class="smallcaps">
					<xsl:apply-templates/>
				</span>
			</xsl:when>
			<xsl:when test="@role='strike'">
				<span class="strike">
					<xsl:apply-templates/>
				</span>
			</xsl:when>
		</xsl:choose>
	</xsl:template>
	<xsl:template match="db:footnote[@role='end-bk1-note']" mode="#all">
		<sup>
			<a href="{concat('notes.xhtml#',concat(concat(substring-after(substring-after(ancestor::db:chapter/@xml:id,'-'),'-'),'_f-'),@label))}" id="{concat('f-',@label)}">
				<xsl:value-of select="@label"/>
			</a>
		</sup>
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="db:footnote[@role='end-ch-note'][@cue='true']" mode="#all">
		<sup>
			<a epub:type="noteref" role="doc-noteref" href="{concat('#',concat(substring-after(substring-after((ancestor::db:part[child::db:label])/@xml:id,'-'),'-'),'n-',@seq))}" id="{concat('re_n-',@seq)}">
				<xsl:value-of select="@label"/>
			</a>
		</sup>
	</xsl:template>
	
	<xsl:template match="db:footnote[@role='end-ch-note'][not (@cue)]" mode="#all">
		<aside id="{concat(substring-after(substring-after((ancestor::db:part[child::db:label])/@xml:id,'-'),'-'),'n-',@seq)}" epub:type="footnote" role="doc-footnote" class="FN1">
			<a href="{concat('#',concat('re_n-',@seq))}">
				<xsl:value-of select="@label"/>
			</a><xsl:text disable-output-escaping="yes"> </xsl:text>
			<xsl:apply-templates/>
		</aside>	
	</xsl:template>
	
	
	<xsl:template match="db:footnote[@role='end-bk-note']">
		<sup>
			<!--<a href="{concat('#fn-',@label)}" id="{concat('fnt-',@label)}">-->
			<!--<a href="{concat('#f-',@id)}" id="{concat('re_f-',@id)}">--><!--23-02-2021-->
			<a href="{concat('#',concat(substring-after(substring-after((ancestor::db:part[child::db:label])/@xml:id,'-'),'-'),'fn-',@id))}" id="{concat('fnt-',@id)}">
				<xsl:value-of select="@label"/>
			</a>
		</sup>
	</xsl:template>

	<!--<xsl:template match="db:chapter" name="sec">
		<xsl:param name="level" select="1"/>
		<xsl:param name="content" select="*"/>
		<xsl:for-each-group select="$content" group-starting-with="*[./db:section/@role=concat('H',$level)]">
			<section class="fff">
				<xsl:apply-templates select="$content"/>
			</section>
			<!-\-<xsl:choose>
				<xsl:when test="$level>6">
					<xsl:apply-templates select="$content"/>
				</xsl:when>
				<xsl:when test="./@role=concat('H',$level)">
					<section class="fff">
						<xsl:call-template name="sec">
							<xsl:with-param name="level" select="$level+1"/>
							<xsl:with-param name="content" select="current-group()"/>
						</xsl:call-template>
					</section>
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="sec">
						<xsl:with-param name="level" select="$level+1"/>
						<xsl:with-param name="content" select="current-group()"/>
					</xsl:call-template>
				</xsl:otherwise>
			</xsl:choose>-\->
		</xsl:for-each-group>
	</xsl:template>
	-->
	
	<xsl:template match="db:section">
		<xsl:choose>
			<xsl:when test="./db:info/db:title[text()='Introduction']">
				<section>
					<xsl:apply-templates/>
				</section>
			</xsl:when>
			<xsl:when test="./db:info/db:title[text()='Conclusion']">
				<section>
					<xsl:apply-templates/>
				</section>
			</xsl:when>
			
			<xsl:otherwise>
				<section>
					<xsl:apply-templates/>
				</section>
			</xsl:otherwise>
		</xsl:choose>
		
	</xsl:template>
	
	<xsl:template match="db:section/db:info/db:title">
		<xsl:choose>
			<xsl:when test="./ancestor::db:chapter[child::db:info/db:minitoc]">
				<h2 class="H1" id="{parent::db:info/parent::db:section/@xml:id}">
					<a href="#re_{parent::db:info/parent::db:section/@xml:id}"><xsl:apply-templates/></a>
				</h2>
			</xsl:when>
			<xsl:otherwise>
		<h2 class="H1" id="{parent::db:info/parent::db:section/@xml:id}">
			<xsl:apply-templates/>
		</h2>
			</xsl:otherwise>
		</xsl:choose>
		
	</xsl:template>
	
	
	<xsl:template match="db:section/db:section/db:info/db:title">
		<h3 class="H2" id="{parent::db:info/parent::db:section/@xml:id}">
			<xsl:apply-templates/>	
		</h3>
	</xsl:template>
	<xsl:template match="db:section/db:section/db:section/db:info/db:title">
		<h4 class="H3" id="{parent::db:info/parent::db:section/@xml:id}">
			<xsl:apply-templates/>
		</h4>
	</xsl:template>
	
	<xsl:template match="db:section/db:section/db:section/db:section/db:info/db:title">
		<h5 class="H4" id="{parent::db:info/parent::db:section/@xml:id}">
			<xsl:apply-templates/>
		</h5>
	</xsl:template>
	
	<xsl:template match="db:section/db:info/db:title" mode="toc">
		<a href="{concat(substring-after(substring-after((ancestor::db:chapter|ancestor::db:appendix)/@xml:id,'-'),'-'),'.xhtml#',parent::db:info/parent::db:section/@xml:id)}">
			<xsl:apply-templates select="node() except db:footnote"/>
		</a>
	</xsl:template>
	<xsl:template match="db:section/db:info/db:title" mode="ncx">
		<xsl:value-of select="."/>
	</xsl:template>
	<xsl:template match="db:bibliodiv/db:info/db:title">
		<p class="H1" id="{parent::db:info/parent::db:bibliodiv/@xml:id}">
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	
	<xsl:template match="db:bibliodiv">
		<h3 class="H2" id="{@xml:id}">
			<xsl:apply-templates select="./child::db:title/node()"/>
		</h3>
		<ol>
			<xsl:apply-templates select="node() except db:title"/>
		</ol>
	</xsl:template>
	
	<!--<xsl:template match="db:bibliodiv/db:title">
		<h3 class="H2" id="{parent::db:bibliodiv/@xml:id}">
			<xsl:apply-templates/>
		</h3>
	</xsl:template>-->
	
	<xsl:template match="db:bibliodiv/db:info/db:title" mode="toc">
		<a href="{concat('bibliography.xhtml#',parent::db:info/parent::db:bibliodiv/@xml:id)}">
			<xsl:apply-templates/>
		</a>
	</xsl:template>
	<xsl:template match="db:bibliodiv/db:info/db:title" mode="ncx">
		<xsl:value-of select="."/>
	</xsl:template>
	<!--<xsl:template match="db:primary" mode="index">
		<p class="IND-1">
			<xsl:apply-templates/>
			<xsl:text> </xsl:text>
			<xsl:if test="not(following-sibling::db:see or following-sibling::db:seealso)">
				<xsl:choose>
					<xsl:when test="(ancestor::db:chapter|ancestor::db:appendix)/@xml:id">
						<a href="{substring-after(substring-after((ancestor::db:chapter|ancestor::db:appendix)/@xml:id,'-'),'-')}.xhtml#page_{substring-before(substring-after(preceding::processing-instruction('page')[1],'value=&quot;'),'&quot;')}">here</a>
					</xsl:when>
					<xsl:otherwise>
						<a href="{substring-after(substring-after(ancestor::db:part[@label]/@xml:id,'-'),'-')}.xhtml#page_{substring-before(substring-after(preceding::processing-instruction('page')[1],'value=&quot;'),'&quot;')}">here</a>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
			<xsl:apply-templates select="following-sibling::db:see|following-sibling::db:seealso"  mode="index"/>
		</p>
	</xsl:template>
	<xsl:template match="db:secondary" mode="index">
		<p class="IND-2">
			<xsl:apply-templates/>
			<xsl:text> </xsl:text>
			<xsl:if test="not(following-sibling::db:see or following-sibling::db:seealso)">
				<xsl:choose>
					<xsl:when test="(ancestor::db:chapter|ancestor::db:appendix)/@xml:id">
						<a href="{substring-after(substring-after((ancestor::db:chapter|ancestor::db:appendix)/@xml:id,'-'),'-')}.xhtml#page_{substring-before(substring-after(preceding::processing-instruction('page')[1],'value=&quot;'),'&quot;')}">here</a>
					</xsl:when>
					<xsl:otherwise>
						<a href="{substring-after(substring-after(ancestor::db:part[@label]/@xml:id,'-'),'-')}.xhtml#page_{substring-before(substring-after(preceding::processing-instruction('page')[1],'value=&quot;'),'&quot;')}">here</a>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
			<xsl:apply-templates select="following-sibling::db:see|following-sibling::db:seealso" mode="index"/>
		</p>
	</xsl:template>
	<xsl:template match="db:tertiary" mode="index">
		<p class="IND-F">
			<xsl:apply-templates/>
			<xsl:text> </xsl:text>
			<xsl:if test="not(following-sibling::db:see or following-sibling::db:seealso)">
				<xsl:choose>
					<xsl:when test="(ancestor::db:chapter|ancestor::db:appendix)/@xml:id">
						<a href="{substring-after(substring-after((ancestor::db:chapter|ancestor::db:appendix)/@xml:id,'-'),'-')}.xhtml#page_{substring-before(substring-after(preceding::processing-instruction('page')[1],'value=&quot;'),'&quot;')}">here</a>
					</xsl:when>
					<xsl:otherwise>
						<a href="{substring-after(substring-after(ancestor::db:part[@label]/@xml:id,'-'),'-')}.xhtml#page_{substring-before(substring-after(preceding::processing-instruction('page')[1],'value=&quot;'),'&quot;')}">here</a>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
			<xsl:apply-templates select="following-sibling::db:see|following-sibling::db:seealso"  mode="index"/>
		</p>
		</xsl:template>-->
	<xsl:template match="db:emphasis" mode="index">
		<xsl:variable name="fontstyle">
			<xsl:choose>
				<xsl:when test="@role='italic'">
					<xsl:text>italic</xsl:text>
				</xsl:when>
				<xsl:when test="@role='underline'">
					<xsl:text>underline</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>bold</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<span class="{$fontstyle}">
			<xsl:apply-templates/>
		</span>
	</xsl:template>
			
	<xsl:template match="db:primary" mode="index">
		<li epub:type="index-entry">
			<xsl:attribute name="class"><xsl:value-of select="./@class"></xsl:value-of></xsl:attribute>
			<xsl:attribute name="id"><xsl:value-of select="./parent::db:indexterm/@xml:id"></xsl:value-of></xsl:attribute>
			<xsl:apply-templates mode="index"/>
			<!--<a href="#see"><xsl:apply-templates select="following-sibling::db:see-entry"/></a>-->
		</li>
	</xsl:template>
	
	<xsl:template match="db:see-also-entry"  mode="index">
		<a>
			<xsl:attribute name="href"><xsl:value-of select="concat('#',./@rid)"></xsl:value-of></xsl:attribute>
			<xsl:apply-templates mode="index"/>
		</a>
	</xsl:template>
	
	<xsl:template match="db:secondary" mode="index">
		<li epub:type="index-entry" class="IND-2">
			<xsl:apply-templates mode="index"/>
		</li>
	</xsl:template>
	<xsl:template match="db:tertiary" mode="index">
		<li epub:type="index-entry" class="IND-3">
			<xsl:apply-templates mode="index"/>
		</li>
	</xsl:template>
	
	<xsl:template match="db:index" mode="index">
		<ul epub:type="index-entry-list">
			<xsl:apply-templates mode="index"/>
		</ul>
	</xsl:template>	
	
	
	<xsl:template match="db:see" mode="index">
		<span class="italic">See</span>
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="db:seealso" mode="index">
		<span class="italic">See also</span>
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="db:indexterm" mode="index">
		<!--<xsl:apply-templates select="* except (db:see|db:seealso|db:see-entry)" mode="index"/>-->
		<xsl:apply-templates mode="index"/>
	</xsl:template>
	<!--<xsl:template match="db:figure/db:mediaobject/db:imageobject/db:imagedata">
		<p class="image-fig" id="{substring-before(substring-after(@fileref,'/'),'.jpg')}">
			<img src="../{@fileref}" alt=""/>
		</p>
	</xsl:template>-->
	<xsl:template match="db:informalfigure/db:mediaobject/db:imageobject/db:imagedata">
		<img src="../{@fileref}" alt="{./@alt}"/>
	</xsl:template>
	<xsl:template match="db:see-entry"  mode="index">
		<a>
			<xsl:attribute name="href"><xsl:value-of select="concat('#',./@rid)"></xsl:value-of></xsl:attribute>
			<xsl:apply-templates mode="index"/>
		</a>
	</xsl:template>
	
	<!--<xsl:template match="db:graphic">
		<p class="image-fig" id="{substring-after(@xlinkhref,'/')}">
			<img src="../{@xlinkhref}" alt=""/>
		</p>
	</xsl:template>-->
	
	<xsl:template match="db:graphic">
		<figure class="image-fig">
			<img src="../images/{@xlink:href}.jpg" alt=""/>
		</figure>
	</xsl:template>
	
	<xsl:template match="db:figure">
		<xsl:variable name="ifile" select="substring-after(substring-after(ancestor::db:book//db:preface[contains(@xml:id,'fig') or contains(@xml:id,'map') or contains(@xml:id,'ill')]/@xml:id, '-'),'-')"/>
		<figure id="{translate(substring-before(substring-after(./db:mediaobject/db:imageobject/db:imagedata/@fileref,'/'),'.jpg'),'fig','f')}">
			<img src="../{./db:mediaobject/db:imageobject/db:imagedata/@fileref}" alt="{./db:mediaobject/db:imageobject/db:imagedata/@alt}"/>
			<figcaption>
				<p class="FC">
				<b>
					<a id="" href="{$ifile}.xhtml#ill_{replace(substring-after(./db:mediaobject/db:imageobject/db:imagedata/@fileref,'/'),'.jpg','')}">
						<xsl:value-of select="./db:label"/>
					</a>
				</b>
				<xsl:text> </xsl:text>
				<xsl:apply-templates select="./db:caption"/>
				<xsl:if test="./db:figsource">
					<xsl:text disable-output-escaping="yes"> </xsl:text><!-- add space-->
					<xsl:apply-templates select="./db:figsource"/>
				</xsl:if>
				</p>
			</figcaption>
			<!--<p class="FC">
				<span class="bold">
					<xsl:if test="$ifile = ''">
						<xsl:value-of select="./db:label"/>
					</xsl:if>	
					<xsl:if test="not($ifile = '')">
						<a href="{$ifile}.xhtml#ill_{substring-after(./db:mediaobject/db:imageobject/db:imagedata/@fileref,'/')}">
							<xsl:value-of select="./db:label"/>
						</a>	
					</xsl:if>
				</span>
				<xsl:text> </xsl:text>
				<xsl:apply-templates select="./db:caption"/>
			</p>-->
		</figure>
	</xsl:template>
	
	<!--<xsl:template match="db:figure/db:caption">
		<xsl:variable name="ifile" select="substring-after(substring-after(ancestor::db:book//db:preface[contains(@xml:id,'fig') or contains(@xml:id,'map') or contains(@xml:id,'ill')]/@xml:id, '-'),'-')"/>
		<p class="FC">
			<!-\-<xsl:apply-templates select="./parent::db:figure/db:label"/>-\->
			<span class="bold">
				<xsl:if test="$ifile = ''">
					<xsl:value-of select="ancestor::db:figure/db:label"/>
				</xsl:if>	
				<xsl:if test="not($ifile = '')">
					<a href="{$ifile}.xhtml#ill_{substring-after(preceding-sibling::db:mediaobject/db:imageobject/db:imagedata/@fileref,'/')}">
						<xsl:value-of select="ancestor::db:figure/db:label"/>
					</a>	
				</xsl:if>
			</span>
			<xsl:text> </xsl:text>
			<xsl:apply-templates/>
		</p>
	</xsl:template>-->
	
	
	<!--<xsl:template match="db:figure">
		
		<xsl:apply-templates select="node() except (db:label)"/>
		
	</xsl:template>-->
	
	
	<xsl:template match="db:equation/db:caption">
		<span class="right">
			<xsl:apply-templates/>
		</span>
	</xsl:template>
	
	<xsl:template match="db:figure/db:caption/db:para">
		<xsl:apply-templates/>
	</xsl:template>
	<!--<xsl:template match="db:caption/db:para">
		<xsl:apply-templates/>
	</xsl:template>-->
	<!--<xsl:template match="db:figure/db:caption/db:para/db:emphasis">
		<xsl:apply-templates/>
	</xsl:template>-->
	<xsl:template match="db:book/db:info/db:title" mode="opf">
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="db:book/db:info/db:title">
		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/xhtml/title.xhtml" method="xml" use-character-maps="hex" indent="yes">
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<title>Title</title>
					<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
				</head>
				<body epub:type="frontmatter">
					<section>

					<p class="Book-Title" id="title_1">
						<xsl:apply-templates/>
					</p>
					<p class="Sub-Title">
						<xsl:apply-templates select="./parent::db:info/db:info/db:subtitle"/>
					</p>
					<p class="Book-Author">
						<xsl:apply-templates select="//db:authorgroup[parent::db:info]"/>
					</p>
					
					<p class="PUB"><xsl:value-of select="./ancestor::db:info/db:bibliomisc[@role='imprint']"/></p>
					<p class="PUB"><span class="italic"><xsl:value-of select="./ancestor::db:info/db:biblioset/db:bibliomisc[@role='imprint']"/></span></p>
					
					</section>
				</body>
			</html>
		</xsl:result-document>
		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/xhtml/halftitle.xhtml" method="xml" use-character-maps="hex" indent="yes">
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<title>Halftitle</title>
					<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
				</head>
				<body epub:type="frontmatter">
					<section>
					<p class="FM-HT" id="half_1">
						<xsl:apply-templates/>
					</p>
					</section>
				</body>
			</html>
		</xsl:result-document>
		<xsl:apply-templates select="db:cover[1]"/>
	</xsl:template>
	<xsl:template match="db:preface[not(contains(@xml:id,'title') or contains(@xml:id,'fig') or contains(@xml:id,'map') or contains(@xml:id,'ill'))]">
		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/xhtml/{substring-after(substring-after(@xml:id,'-'),'-')}.xhtml" method="xml" use-character-maps="hex" indent="yes">
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<title>
						<xsl:value-of select="./db:info/db:title"/>
					</title>
					<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
				</head>
				<body epub:type="frontmatter">
					<xsl:variable name="chapid"><xsl:value-of select="substring-after(substring-after(@xml:id,'-'),'-')"></xsl:value-of></xsl:variable>
					<xsl:variable name="chaprole">
						<xsl:choose>
							<xsl:when test="contains($chapid,'Contributors')">Contributors</xsl:when>
							<xsl:when test="contains($chapid,'preface')">preface</xsl:when>
							<xsl:otherwise>preface</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>
					<xsl:text>&#xa;</xsl:text>
					<xsl:choose>
						<xsl:when test="$chaprole='preface'">
						 	<xsl:text disable-output-escaping="yes">&lt;section epub:type="preface" role="doc-preface"&gt;</xsl:text>			
						</xsl:when>
						<xsl:when test="$chaprole='Contributors'">
							<xsl:text disable-output-escaping="yes">&lt;section epub:type="contributors"&gt;</xsl:text>
						</xsl:when>
					</xsl:choose>
						<xsl:text>&#xa;</xsl:text>
						<h1 epub:type="title" class="FMT">
						<a id="{substring-after(substring-after(@xml:id,'-'),'-')}" href="contents.xhtml#re_{substring-after(substring-after(@xml:id,'-'),'-')}">
							<xsl:apply-templates select="./db:info/db:title/processing-instruction('page')"/>
							<xsl:value-of select="db:info/db:title"/>
						</a>
						</h1>
					<xsl:apply-templates select="node() except (db:info)"/>
					<xsl:if test="descendant::db:footnote[@role='end-bk-note']">
						<h2 id="note-{substring-after(substring-after(@xml:id,'-'),'-')}" class="H1">
							<xsl:text>Notes</xsl:text>
						</h2>
						<aside epub:type="footnotes">
							<xsl:apply-templates select="descendant::db:footnote[@role='end-bk-note']" mode="cnote"/>
						</aside>
					</xsl:if>
					<xsl:text disable-output-escaping="yes">&lt;/section&gt;</xsl:text>
				</body>
			</html>
		</xsl:result-document>
	</xsl:template>
	<xsl:template match="db:abbreviation">
		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/xhtml/abbrev.xhtml" method="xml" use-character-maps="hex" indent="yes">
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<title>
						<xsl:value-of select="db:info/db:title"/>
					</title>
					<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
				</head>
				<body epub:type="frontmatter">
					<section>
						<xsl:apply-templates/>
					</section>
				</body>
			</html>
		</xsl:result-document>
	</xsl:template>
	<xsl:template match="db:acknowledgements">
		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/xhtml/ack.xhtml" method="xml" use-character-maps="hex" indent="yes">
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<title>
						<xsl:value-of select="db:info/db:title"/>
					</title>
					<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
				</head>
				<body epub:type="frontmatter">
					<section>
						<xsl:apply-templates/>
					</section>
				</body>
			</html>
		</xsl:result-document>
	</xsl:template>
	<!--<xsl:template match="db:author">
		<p class="TXT">
			<xsl:apply-templates/>
		</p>
	</xsl:template>-->
	<!--<xsl:template match="db:personname/db:surname">
		<p class="TXT">
			<xsl:apply-templates/>
		</p>
	</xsl:template>-->
	
	
	
	
	<xsl:template match="db:acknowledgements/db:info/db:title">
		<h1 epub:type="title" class="FMT">
			<xsl:apply-templates select="processing-instruction('page')"/>			
			<a id="ack" href="contents.xhtml#re_ack">
				<xsl:value-of select="."/>
			</a>
		</h1>
	</xsl:template>
	<xsl:template match="db:glossary[not (./ancestor::db:chapter)]">
		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/xhtml/glossary.xhtml" method="xml" use-character-maps="hex" indent="yes">
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<title>
						<xsl:value-of select="db:info/db:title"/>
					</title>
					<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
				</head>
				<body epub:type="frontmatter">
					<section>
						
					<xsl:apply-templates/>
					</section>
				</body>
			</html>
		</xsl:result-document>
	</xsl:template>
	<xsl:template match="db:glossary/db:info/db:title">
		<h1 epub:type="title" class="FMT">
			<a id="glossary" href="contents.xhtml#re_glossary">
				<xsl:value-of select="."/>
			</a>
		</h1>
	</xsl:template>
	
	<xsl:template match="db:glossentry">
		<p class="gloss">
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	
	<xsl:template match="db:glossterm">
		<span class="bold">
			<xsl:apply-templates/>
		</span>
	</xsl:template>
	
	<xsl:template match="db:dedication">
		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/xhtml/dedication.xhtml" method="xml" use-character-maps="hex" indent="yes">
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<title>
						<xsl:choose>
							<xsl:when test="./db:info/db:title">
								<xsl:value-of select="./db:info/db:title"></xsl:value-of>
							</xsl:when>
							<xsl:otherwise><xsl:text disable-output-escaping="yes">Dedication</xsl:text></xsl:otherwise>
						</xsl:choose>
					</title>
					<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
				</head>
				<body epub:type="frontmatter">
					<section>
					<xsl:apply-templates select="node() except (db:info/db:title)"/>
					</section>
				</body>
			</html>
		</xsl:result-document>
	</xsl:template>
	<xsl:template match="db:copyright">
		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/xhtml/copyright.xhtml" method="xml" use-character-maps="hex" indent="yes">
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<title>Copyright</title>
					<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
				</head>
				<body>
					<a id="copy"/>
					<xsl:apply-templates select="./child::db:holder|following-sibling::db:legalnotice/db:para|following-sibling::db:biblioset[@role='isbns']/db:biblioid|preceding-sibling::db:biblioset|preceding-sibling::db:edition|preceding-sibling::db:pubdate"/>
				</body>
			</html>
		</xsl:result-document>
	</xsl:template>
	<xsl:template match="db:holder">
		<p class="Copy-TXT">
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	<xsl:template match="db:legalnotice/db:para">
		<xsl:variable name="headin">
			<xsl:choose>
				<xsl:when test="db:emphasis[@role='bold']">
					<xsl:text>Copy-TXT</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>Copy-TXT</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<p class="{$headin}">
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	
	<xsl:template match="db:superscript[not (parent::db:footnote[@role='end-bk1-note'])]">
		<sup><xsl:apply-templates/></sup>
		
		
	</xsl:template>
	
	<xsl:template match="db:subscript">
		<sub><xsl:apply-templates/></sub>
		
		
	</xsl:template>
	
	<xsl:template match="db:sup">
		<sup><xsl:apply-templates/></sup>
		
	</xsl:template>
	
	<xsl:template match="db:sub">
		<sub><xsl:apply-templates/></sub>
		
		
	</xsl:template>
	
	<xsl:template match="db:biblioid[@class='isbn']">
		<p class="Copy-TXT">
			<xsl:if test="@role='hardback'">
				<xsl:text>ISBN: HB: </xsl:text>
			</xsl:if>
			<xsl:if test="@role='paperback'">
				<xsl:text>PB: </xsl:text>
			</xsl:if>
			<xsl:if test="@role='epdf'">
				<xsl:text>ePDF: </xsl:text>
			</xsl:if>
			<xsl:if test="@role='epub'">
				<xsl:text>ePub: </xsl:text>
			</xsl:if>
			<xsl:if test="@role='xml'">
				<xsl:text>XML: </xsl:text>
			</xsl:if>
			<xsl:apply-templates/>
		</p>
	</xsl:template>
		
	
	
	<xsl:template match="db:informaltable">
		<table>
			<xsl:apply-templates/>
		</table>
	</xsl:template>
	<!--<xsl:template match="db:table/db:info/db:title">
		<caption>
			<xsl:apply-templates/>
		</caption>
	</xsl:template>-->
	
	
	
	<!--<xsl:template match="db:table/db:info/db:title">
		<xsl:variable name="ifile" select="substring-after(substring-after(ancestor::db:book//db:preface[contains(@xml:id,'tab')]/@xml:id, '-'),'-')"/>
		<p class="TT">
			
			<!-\-<xsl:apply-templates select="./parent::db:table/db:label"/>-\->
			<!-\-<xsl:apply-templates select="./preceding-sibling::db:label"/>-\->
			<xsl:choose>
				<xsl:when test="$ifile = ''">
					<xsl:value-of select="./preceding-sibling::db:label"/>
				</xsl:when>
				<xsl:when test="not($ifile = '')">
					<a href="{$ifile}.xhtml#ill">
						Table <xsl:value-of select="./preceding-sibling::db:label"/>
					</a>	
				</xsl:when>
			</xsl:choose>
			<xsl:text> </xsl:text>
			<xsl:apply-templates/>
		</p>
	</xsl:template>-->
	

<!--	<xsl:template match="db:table">
		
		<xsl:apply-templates select="node() except (db:label)"/>
		
	</xsl:template>-->


	
	<xsl:template match="db:tblfoot">
		<tfoot>
			<xsl:apply-templates/>		
		</tfoot>
	</xsl:template>
	
	<xsl:template match="db:tblfn">
		<tr>
			<td class="TFN" colspan="{count(./ancestor::db:table/db:tgroup/child::db:colspec)}">
				<xsl:apply-templates/>
			</td>
		</tr>
	</xsl:template>
	
	<xsl:template match="db:tblsource">
		<p class="TFN">
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	
	
	
	
	
	<!--<xsl:template match="db:table">
		<xsl:variable name="ifile" select="substring-after(substring-after(ancestor::db:book//db:preface[contains(@xml:id,'tab')]/@xml:id, '-'),'-')"/>
		<xsl:choose>
			<xsl:when test="./child::db:info/db:title">
				<p class="TT">
					<xsl:if test="@xml:id">
						<xsl:attribute name="id">
							<xsl:value-of select="translate(@xml:id,'T','t')"></xsl:value-of>
						</xsl:attribute>		
					</xsl:if>
					<xsl:choose>
						<xsl:when test="$ifile = ''">
							<xsl:value-of select="./child::db:label"/>
						</xsl:when>
						<xsl:when test="not($ifile = '')">
							<a href="{$ifile}.xhtml#ill">
								Table <xsl:value-of select="./child::db:label"/>
							</a>	
						</xsl:when>
					</xsl:choose>
					<xsl:text> </xsl:text>
					<xsl:apply-templates select="./child::db:info/db:title"/>
				</p>		
			</xsl:when>
		</xsl:choose>
		<table class="TABL">
			<xsl:apply-templates/>
		</table>
		</xsl:template>-->
	
	<xsl:template match="db:table" mode="#all">
		<xsl:variable name="ifile" select="substring-after(substring-after(ancestor::db:book//db:preface[contains(@xml:id,'tab') or contains(@xml:id,'fig')]/@xml:id, '-'),'-')"/>
		<table class="TABL">
			<xsl:if test="not (@class)">
				<caption class="TT" id="{translate(@xml:id,'T','t')}">
					<a href="{$ifile}.xhtml#ill_{./@xml:id}">
						<xsl:value-of select="./db:label"/><xsl:text disable-output-escaping="yes"> </xsl:text>
					</a>
					<xsl:apply-templates select="./child::db:info/db:title"/>
				</caption>
			</xsl:if>
			<xsl:apply-templates select="node() except (./child::db:label,./child::db:info/db:title)"/>
		</table>
	</xsl:template>
	
	<xsl:template match="db:table/db:info|db:table/db:label"/>
	
	<xsl:template match="db:tr">
		<tr>
			<xsl:apply-templates/>
		</tr>
	</xsl:template>
	<xsl:template match="db:td">
		<td>
			<xsl:attribute name="class"><xsl:value-of select="@class"></xsl:value-of></xsl:attribute>
				
			<xsl:apply-templates/>
		</td>
	</xsl:template>
	
	<xsl:template match="db:row">
		<tr>
			<xsl:apply-templates/>
		</tr>
	</xsl:template>
	<xsl:template match="db:thead/db:row/db:entry">
		<xsl:variable name="pos" select="position()"/>
		<xsl:variable name="ntd" select="count(parent::db:row/db:entry)"/>
		<xsl:choose>
			<xsl:when test="count(./db:tp)=0">
				<th class="TCH"><p class="TCH"><xsl:if test="@morerows">
					<xsl:attribute name="rowspan">
						<xsl:value-of select="@morerows"/>
					</xsl:attribute>
				</xsl:if>
					<xsl:if test="@rowspan">
						<xsl:attribute name="rowspan">
							<xsl:value-of select="@rowspan"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:if test="@colspan">
						<xsl:attribute name="colspan">
							<xsl:value-of select="@colspan"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:apply-templates/>
					<xsl:apply-templates select="parent::db:row/following-sibling::node()[local-name()='page' and position()=2 and ($ntd * 2)= $pos]"/>
				</p></th>
			</xsl:when>
			<xsl:otherwise>
				<th class="TCH"><p class="TCH">
					<xsl:if test="@morerows">
						<xsl:attribute name="rowspan">
							<xsl:value-of select="@morerows"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:if test="@rowspan">
						<xsl:attribute name="rowspan">
							<xsl:value-of select="@rowspan"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:if test="@colspan">
						<xsl:attribute name="colspan">
							<xsl:value-of select="@colspan"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:for-each select="db:tp">
						<xsl:apply-templates/><br/>
					</xsl:for-each>
					<xsl:apply-templates select="node() except db:tp"/>
					<xsl:apply-templates select="parent::db:row/following-sibling::node()[local-name()='page' and position()=2 and ($ntd * 2)= $pos]"/>
				</p></th>
			</xsl:otherwise>
		</xsl:choose>
		
	</xsl:template>
	
	<xsl:template match="db:tbody/db:row/db:entry">
		<xsl:variable name="entrypos">
			<xsl:choose>
				<xsl:when test="./count(parent::db:row/preceding-sibling::db:row) = 0">TBF</xsl:when>
				<xsl:when test="(./count(parent::db:row/preceding-sibling::db:row) &gt;= 1) and ((./count(parent::db:row/parent::db:tbody/child::db:row))-1)!=(./count(parent::db:row/preceding-sibling::db:row))">TB</xsl:when>
				<xsl:otherwise>TBL</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		
		<xsl:variable name="pos" select="position()"/>
		<xsl:variable name="ntd" select="count(parent::db:row/db:entry)"/>
		<xsl:choose>
			<xsl:when test="count(./db:tp)=0">
				<td class="{$entrypos}">
					<xsl:if test="@morerows">
						<xsl:attribute name="rowspan">
							<xsl:value-of select="@morerows"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:if test="@rowspan">
						<xsl:attribute name="rowspan">
							<xsl:value-of select="@rowspan"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:if test="@colspan">
						<xsl:attribute name="colspan">
							<xsl:value-of select="@colspan"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:choose>
						<xsl:when test="./child::db:orderedlist">
							<xsl:apply-templates/>
								<xsl:apply-templates select="parent::db:row/following-sibling::node()[local-name()='page' and position()=2 and ($ntd * 2)= $pos]"/>
						</xsl:when>
						<xsl:otherwise>
							<p class="{$entrypos}"><xsl:apply-templates/>
								<xsl:apply-templates select="parent::db:row/following-sibling::node()[local-name()='page' and position()=2 and ($ntd * 2)= $pos]"/>
							</p>
						</xsl:otherwise>
					</xsl:choose>
					</td>
			</xsl:when>
			<xsl:otherwise>
				<td class="{$entrypos}">
					<xsl:if test="@morerows">
						<xsl:attribute name="rowspan">
							<xsl:value-of select="@morerows"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:if test="@rowspan">
						<xsl:attribute name="rowspan">
							<xsl:value-of select="@rowspan"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:if test="@colspan">
						<xsl:attribute name="colspan">
							<xsl:value-of select="@colspan"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:for-each select="db:tp">
						<p class="{$entrypos}">
							<xsl:apply-templates/>
						</p>
					</xsl:for-each>
					<xsl:apply-templates select="node() except db:tp"/>
					<xsl:apply-templates select="parent::db:row/following-sibling::node()[local-name()='page' and position()=2 and ($ntd * 2)= $pos]"/>
				</td>
			</xsl:otherwise>
		</xsl:choose>
		
	</xsl:template>
	<xsl:template match="db:tbody">
		<tbody>
			<xsl:apply-templates select="node() except processing-instruction('page')"/>
		</tbody>
	</xsl:template>
	<xsl:template match="db:tfoot">
		<tfoot>
			<xsl:apply-templates/>
		</tfoot>
	</xsl:template>
	<xsl:template match="db:thead">
		<thead>
			<xsl:apply-templates/>
		</thead>
	</xsl:template>
	
	<!--<xsl:template match="db:orderedlist">
		<ol>
		<xsl:choose>
			<xsl:when test="count(db:listitem/db:para) = 1">
				<li class="NLF">
					<xsl:apply-templates/>
				</li>
			</xsl:when>
			<xsl:otherwise>
				<xsl:for-each select="db:listitem/db:para">
					<xsl:if test="position() = 1">
						<li class="NLF">
							<xsl:apply-templates/>
						</li>
					</xsl:if>
					<xsl:if test="position() = last()">
						<li class="NLL">
							<xsl:apply-templates/>
						</li>
					</xsl:if>
					<xsl:if test="position()!=last() and position()!=1">
						<li class="NL">
							<xsl:apply-templates/>
						</li>
					</xsl:if>
					
				</xsl:for-each>
				
			</xsl:otherwise>
		</xsl:choose>
		</ol>
	</xsl:template>-->

	
	<!--<xsl:template match="db:orderedlist">
	
		<xsl:apply-templates/>
		
	</xsl:template>-->
	
	
	
	
	
	<!--<xsl:template match="db:orderedlist/db:listitem/db:para">
		<xsl:variable name="lstyle">
			<xsl:if test="position()=1">
				<xsl:text>NLF</xsl:text>
			</xsl:if>
			<xsl:if test="position()=last()">
				<xsl:text>NLL</xsl:text>
			</xsl:if>
			<xsl:if test="position()!=last() and position()!=1">
				<xsl:text>NL</xsl:text>
			</xsl:if>
		</xsl:variable>
		<p class="{lstyle}">
			<xsl:apply-templates/>
		</p>
		</xsl:template>-->
	
<!--<xsl:template match="db:itemizedlist">
		<ul>
		<xsl:choose>
			<xsl:when test="count(db:listitem/db:para) = 1">
				<li class="BLF">
					<xsl:apply-templates/>
				</li>
			</xsl:when>
			<xsl:otherwise>
				<xsl:for-each select="db:listitem/db:para">
					<xsl:if test="position() = 1">
						<li class="BLF">
							<xsl:apply-templates/>
						</li>
					</xsl:if>
					<xsl:if test="position() = last()">
						<li class="BLL">
							<xsl:apply-templates/>
						</li>
					</xsl:if>
					<xsl:if test="position()!=last() and position()!=1">
						<li class="BL">
							<xsl:apply-templates/>
						</li>
					</xsl:if>
					
				</xsl:for-each>
				
			</xsl:otherwise>
		</xsl:choose>
		</ul>
</xsl:template>-->
	
	
	<xsl:template match="db:itemizedlist/db:listitem/db:para" mode="#all">
		<xsl:variable name="itempos">
			<xsl:choose>
				<xsl:when test="./count(parent::db:listitem/preceding-sibling::db:listitem) = 0">BLF</xsl:when>
				<xsl:when test="(./count(parent::db:listitem/preceding-sibling::db:listitem) &gt;= 1) and ((./count(parent::db:listitem/parent::db:itemizedlist/child::db:listitem))-1)!=(./count(parent::db:listitem/preceding-sibling::db:listitem))">BL</xsl:when>
				<xsl:otherwise>BLL</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<li class="{$itempos}">
			<xsl:apply-templates/>
		</li>
		<!--<li class="BLF">
			<xsl:apply-templates/>
		</li>-->
	</xsl:template>
	
	<xsl:template match="db:orderedlist/db:listitem/db:para" mode="withstart">
		<li>
			<xsl:apply-templates select="node() except db:token"/>
		</li>
	</xsl:template>
	
	<xsl:template match="db:orderedlist/db:listitem/db:para" mode="withoutstart">
		<li>
			<xsl:apply-templates select="db:token"/><xsl:text disable-output-escaping="yes"> </xsl:text> <xsl:apply-templates select="node() except db:token"/>
		</li>
	</xsl:template>
		
	
	
	<!--23-02-2021-->
	<!--<xsl:template match="db:orderedlist/db:listitem/db:para">
		<xsl:variable name="orderpos">
			<xsl:choose>
				<xsl:when test="./parent::db:listitem/parent::db:orderedlist[ancestor::db:listitem]">
					<xsl:choose>
						<xsl:when test="./count(parent::db:listitem/preceding-sibling::db:listitem) = 0">NLF1</xsl:when>
						<xsl:when test="(./count(parent::db:listitem/preceding-sibling::db:listitem) &gt;= 1) and ((./count(parent::db:listitem/parent::db:orderedlist/child::db:listitem))-1)!=(./count(parent::db:listitem/preceding-sibling::db:listitem))">NL1</xsl:when>
						<xsl:otherwise>NLL1</xsl:otherwise>
					</xsl:choose>		
				</xsl:when>
				<xsl:otherwise>
					<xsl:choose>
				<xsl:when test="./count(parent::db:listitem/preceding-sibling::db:listitem) = 0">NLF</xsl:when>
				<xsl:when test="(./count(parent::db:listitem/preceding-sibling::db:listitem) &gt;= 1) and ((./count(parent::db:listitem/parent::db:orderedlist/child::db:listitem))-1)!=(./count(parent::db:listitem/preceding-sibling::db:listitem))">NL</xsl:when>
				<xsl:otherwise>NLL</xsl:otherwise>
			</xsl:choose>
				</xsl:otherwise>
			</xsl:choose>
			
		</xsl:variable>
		<li class="{$orderpos}">
			<xsl:apply-templates/>
		</li>
	</xsl:template>-->
	
	<xsl:template match="db:itemizedlist[(@mark)]" mode="#all">
		<ul class="ul">
			<xsl:apply-templates/>
		</ul>
	</xsl:template>
	
	<xsl:template match="db:itemizedlist[not (@mark)]" mode="#all">
		<ul class="ul-1">
			<xsl:apply-templates/>
		</ul>
	</xsl:template>
	
	<xsl:template match="db:orderedlist[@start]">
		<xsl:variable name="vartype">
			<xsl:value-of select="translate(db:listitem[1]/db:para/db:token,'.)(  ','')"></xsl:value-of>
		</xsl:variable>
		<ol type="1">
			<xsl:if test="./@start and ./@start&gt;1">
				<xsl:attribute name="start"><xsl:value-of select="./@start"/></xsl:attribute>
			</xsl:if>
			<xsl:apply-templates mode="withstart"/>
		</ol>
	</xsl:template>
	
	<xsl:template match="db:orderedlist[not (@start)]">
		<ol class="ol-1">
			<xsl:apply-templates mode="withoutstart"/>
		</ol>
	</xsl:template>
	
	<xsl:template match="db:token">
		<xsl:apply-templates/>
	</xsl:template>
	
	
	<!--<xsl:template match="db:itemizedlist[@mark='disc']">
		<ul>
		<xsl:choose>
		<xsl:when test="count(db:listitem/db:para) = 1">
		<li class="BLF">
		<xsl:apply-templates/>
		</li>
		</xsl:when>
		<xsl:otherwise>
		<xsl:for-each select="db:listitem/db:para">
		<xsl:if test="position() = 1">
		<li class="BLF">
		<xsl:apply-templates/>
		</li>
		</xsl:if>
		<xsl:if test="position() = last()">
		<li class="BLL">
		<xsl:apply-templates/>
		</li>
		</xsl:if>
		<xsl:if test="position()!=last() and position()!=1">
		<li class="BL">
		<xsl:apply-templates/>
		</li>
		</xsl:if>
		
		</xsl:for-each>
		
		</xsl:otherwise>
		</xsl:choose>
		</ul>
		</xsl:template>-->
	

	
<!--	<xsl:template match="db:listitem/db:para">
		<li>
			<xsl:apply-templates/>
		</li>
	</xsl:template>-->
	
<!--<xsl:template match="db:listitem/db:para"/>-->
	
	
<!--	<xsl:template match="db:unorderedlist/db:listitem/db:para">
		<xsl:variable name="ulstyle">
			<xsl:if test="position()=1">
				<xsl:text>BLF</xsl:text>
			</xsl:if>
			<xsl:if test="position()=last()">
				<xsl:text>BLL</xsl:text>
			</xsl:if>
			<xsl:if test="position()!=last() and position()!=1">
				<xsl:text>BL</xsl:text>
			</xsl:if>
		</xsl:variable>
		<p class="{ulstyle}">
			<xsl:apply-templates/>
		</p>
	</xsl:template>
-->
	<xsl:template match="db:link[@role='figure']">
		<a aria-label="figure" href="{substring-after(substring-after(key('images', @linkend)/(ancestor::db:chapter|ancestor::db:appendix|ancestor::db:part[@label])/@xml:id,'-'),'-')}.xhtml#{translate(substring-before(substring-after(key('images', @linkend)/db:mediaobject/db:imageobject/db:imagedata/@fileref,'/'),'.jpg'),'fig','f')}">
			<xsl:apply-templates/>
		</a>
	</xsl:template>
	
	<xsl:template match="db:link[@role='chapter']">
		<a aria-label="chapter" href="{concat(concat(./@linkend,'.xhtml#'),./@linkend)}">
			<xsl:apply-templates/>
		</a>
	</xsl:template>
	
	<xsl:template match="db:link[@role='table']">
		<a aria-label="table" href="{substring-after(substring-after(key('images', @linkend)/(ancestor::db:chapter|ancestor::db:appendix|ancestor::db:part[@label])/@xml:id,'-'),'-')}.xhtml#{translate(key('images', @linkend)/@xml:id,'T','t')}">
			<xsl:apply-templates/>
		</a>
	</xsl:template>
	
	<xsl:template match="db:link[@role='xref']">
		<a href="{substring-after(substring-after(key('images', @linkend)/(ancestor::db:chapter|ancestor::db:appendix|ancestor::db:part[@label])/@xml:id,'-'),'-')}.xhtml#{key('images', @linkend)/@xml:id}">
			<xsl:apply-templates/>
		</a>
	</xsl:template>
	
	
<!--	<xsl:template match="db:link[@role='bibr']">
		<a href="{substring-after(substring-after(key('images', @linkend)/(ancestor::db:chapter|ancestor::db:appendix|ancestor::db:part[@label])/@xml:id,'-'),'-')}.xhtml#{key('images', @linkend)/@xml:id}">
			<xsl:apply-templates/>
		</a>
		</xsl:template>-->
	
	<xsl:template match="db:link[@role='bibr']">
		<a>
			<xsl:choose>
				<xsl:when test="./@href">
					<xsl:attribute name="href">#re_<xsl:value-of select="./@href"></xsl:value-of></xsl:attribute>		
				</xsl:when>
				<xsl:when test="./@linkend">
					<xsl:attribute name="id"><xsl:value-of select="./@linkend"></xsl:value-of></xsl:attribute>
					<xsl:attribute name="href">#re_<xsl:value-of select="./@linkend"></xsl:value-of></xsl:attribute>
				</xsl:when>
			</xsl:choose>
			<xsl:apply-templates/>
		</a>
	</xsl:template>
	
	
	
	<xsl:template match="db:link[@role='bib']">
		<xsl:choose>
			<xsl:when test="$lbibcount &gt; 0">
				<a href="bibliography.xhtml#{@linkend}">
					<xsl:apply-templates/>
				</a>
			</xsl:when>
			<xsl:otherwise>
				<a href="#{@linkend}">
					<xsl:apply-templates/>
				</a>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!--<xsl:template match="db:link[@role='bibr']">
		<xsl:choose>
			<xsl:when test="$lbibcount &gt; 0">
				<a href="bibliography.xhtml#{@linkend}">
					<xsl:apply-templates/>
				</a>
			</xsl:when>
			<xsl:otherwise>
				<a href="#{@linkend}">
					<xsl:apply-templates/>
				</a>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>-->
	
	
	<xsl:template match="db:link[@role='page']" mode="index">
		
				<a>
					<xsl:attribute name="href"><xsl:value-of select="./@href"></xsl:value-of></xsl:attribute>
					<xsl:apply-templates/>
				</a>
			
	</xsl:template>
	
	
	<!--<xsl:template match="db:chapter/db:bibliomixed">
		<p id="{@xml:id}" class="Biblio">
			<xsl:apply-templates/>
		</p>
	</xsl:template>-->
	
	<xsl:template match="db:bibliomixed/db:pagenums">
		
		<xsl:apply-templates/>
		
	</xsl:template>
	
	
	
	<xsl:template match="db:bibliomset/db:pagenums">
		
		<xsl:apply-templates/>
		
	</xsl:template>
	
	<xsl:template match="db:cover/db:bibliolist/db:bibliomixed[@role='series']">
		<xsl:apply-templates/>
	</xsl:template> 
	<xsl:template match="db:footnote[@role='end-bk-note']/node()" mode="note">
		<xsl:variable name="ftstyle">
			<xsl:if test="matches(parent::db:footnote/@label,'^\d{1}$')">
				<xsl:text>FN1</xsl:text>
			</xsl:if>
			<xsl:if test="matches(parent::db:footnote/@label,'^\d{2}$')">
				<xsl:text>FN2</xsl:text>
			</xsl:if>
			<xsl:if test="matches(parent::db:footnote/@label,'^\d{3}$')">
				<xsl:text>FN3</xsl:text>
			</xsl:if>
		</xsl:variable>
		<p class="{$ftstyle}">
			<xsl:if test="not(preceding-sibling::db:para)">
				<a href="{concat(concat(./parent::db:footnote/@chapxmlid,'.xhtml#'),'f-',parent::db:footnote/@label)}" id="{concat(./parent::db:footnote/@chapxmlid,'_f-',parent::db:footnote/@label)}">
					<xsl:value-of select="./parent::db:footnote/@dispftid"></xsl:value-of><xsl:text disable-output-escaping="yes"> </xsl:text>
				</a>
			</xsl:if>
			<xsl:apply-templates/>
		</p>
	</xsl:template>
		
		<xsl:template match="db:footnote[@role='end-bk-note']/child::node()" mode="cnote">
		<xsl:variable name="ftstyle">
			<xsl:if test="matches(parent::db:footnote/@id,'^\d{1}$')">
				<xsl:text>FN1</xsl:text>
			</xsl:if>
			<xsl:if test="matches(parent::db:footnote/@id,'^\d{2}$')">
				<xsl:text>FN2</xsl:text>
			</xsl:if>
			<xsl:if test="matches(parent::db:footnote/@id,'^\d{3}$')">
				<xsl:text>FN3</xsl:text>
			</xsl:if>
		</xsl:variable>
			<li class="{$ftstyle}">
			<xsl:if test="not(preceding-sibling::db:para)">
				<xsl:choose>
					<xsl:when test="(ancestor::db:chapter|ancestor::db:appendix|ancestor::db:preface)/@xml:id">
						<a href="{concat('#fnt-',parent::db:footnote/@id)}" id="{concat('fn-',parent::db:footnote/@id)}">
							<xsl:value-of select="parent::db:footnote/@id"/><xsl:text disable-output-escaping="yes">.</xsl:text>
						</a><xsl:text disable-output-escaping="yes"> </xsl:text><!-- add space-->
					</xsl:when>
					<xsl:otherwise>
						<a href="{concat(substring-after(substring-after((ancestor::db:part[child::db:label])/@xml:id,'-'),'-'),'.xhtml#re_n-',parent::db:footnote/@label)}" id="{concat(substring-after(substring-after((ancestor::db:part[child::db:label])/@xml:id,'-'),'-'),'n-',parent::db:footnote/@label)}">
							<xsl:value-of select="parent::db:footnote/@id"/><xsl:text disable-output-escaping="yes">.</xsl:text>
						</a>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
			<xsl:apply-templates/>
			</li>
	</xsl:template>
	
	
	
	
	<!--<xsl:template match="db:footnote[@role='end-bk-note']" mode="cnote">
		
			<a href="{concat('#fnt-',@label)}" id="{concat('fn-',@label)}">
				<b>
					<xsl:value-of select="@label"/>
					<xsl:text>. </xsl:text>
				</b>
			</a>
		

		<xsl:apply-templates/>
	</xsl:template>-->
	<xsl:template match="db:bibliography[not(ancestor::db:chapter)]">
		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/xhtml/bibliography.xhtml" method="xml" indent="yes" use-character-maps="hex">				
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<title>
						<xsl:value-of select="db:info/db:title"/>
					</title>
					<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
				</head>
				<body epub:type="backmatter">
					<section>
						
						<h1 epub:type="title" class="FMT">
						<a id="bib" href="contents.xhtml#re_bib">
							<xsl:value-of select="db:info/db:title"/>
						</a>
					</h1>
					<xsl:apply-templates/>
					</section>
				</body>
			</html>			
		</xsl:result-document>
	</xsl:template>
	<xsl:template match="db:bibliomixed">
		<li epub:type="biblioentry" role="doc-biblioentry" class="Biblio">
			<xsl:apply-templates/>
		</li>
	</xsl:template>
	
	<xsl:template match="db:bibliomixed/db:label">
		<a href="#{./parent::db:bibliomixed/@xml:id}" id="re_{./parent::db:bibliomixed/@xml:id}">
			<xsl:apply-templates/>
		</a>
	</xsl:template>
	
	
	<xsl:template match="db:surname">
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="db:bibliography[not(ancestor::db:chapter)]/db:info/db:title|db:footnote/db:blockquote|db:footnote[@role='end-bk1-note']/db:bibliolist/db:bibliomixed"/>
	<xsl:template match="db:bibliography[ancestor::db:chapter]/db:info/db:title">
		<section>
		<h2 class="H1">
			<xsl:apply-templates/>
		</h2>
		</section>
	</xsl:template>
	<xsl:template match="db:poetry">
		<xsl:choose>
			<xsl:when test="count(db:linegroup/db:line) = 1">
				<p class="EXT">
					<xsl:apply-templates/>
				</p>
			</xsl:when>
			<xsl:when test="count(db:linegroup/db:line)&gt;1">
				
				<xsl:for-each select="db:linegroup/db:line">
					<xsl:if test="position() = 1">
						<p class="EXT-F">
							<xsl:apply-templates/>
						</p>
					</xsl:if>
					<xsl:if test="position() = last()">
						<p class="EXT-L">
							<xsl:apply-templates/>
						</p>
					</xsl:if>
					<xsl:if test="position()!=last() and position()!=1">
						<p class="EXT-M">
							<xsl:apply-templates/>
						</p>
					</xsl:if>
					
				</xsl:for-each>
				
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	
	<xsl:template match="db:poem">
		<aside epub:type="sidebar">
			<xsl:apply-templates/>
		</aside>
	</xsl:template>
	
	<!--<xsl:template match="db:blockquote">
		<blockquote>
			<xsl:apply-templates/>
		</blockquote>
	</xsl:template>
	
	<xsl:template match="db:blockquote/db:para">
		<p class="EXT">
			<xsl:apply-templates />
		</p>
	</xsl:template>-->
	<!--<xsl:template match="db:blockquote">
		<blockquote>
			<xsl:for-each select="./child::node()">
				<xsl:choose>
					<xsl:when test="local-name()='para'">
						<p>
							<xsl:apply-templates />
						</p>
					</xsl:when>
					<xsl:when test="local-name()='orderedlist'">
						<ol class="ol-1">
							<xsl:apply-templates/>
						</ol>
					</xsl:when>
					<xsl:when test="local-name()='itemizedlist'">
						<xsl:choose>
							<xsl:when test="./@mark">
								<ul class="ul">
									<xsl:apply-templates/>
								</ul>
							</xsl:when>
							<xsl:when test="not (./@mark)">
								<ul class="ul-1">
									<xsl:apply-templates/>
								</ul>
							</xsl:when>
						</xsl:choose>
					</xsl:when>
					<xsl:when test="local-name()='source'">				
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:for-each>
			<xsl:apply-templates select="db:source"/>
		</blockquote>
		</xsl:template>-->
	
	
	<xsl:template match="//db:token"/>
				
	
	
	<xsl:template match="db:blockquote" mode="#all">
		<blockquote>
				<xsl:choose>
			<xsl:when test="count(db:para) = 1">
				<p class="EXT">
					<xsl:apply-templates select="node() except db:source"/>
				</p>
			</xsl:when>
			<xsl:when test="count(db:para)&gt;1">
				<xsl:for-each select="./child::node()">
					<xsl:choose>
						<xsl:when test="local-name()='para'">
							<xsl:if test="position() = 1">
								<p class="EXT-F">
									<xsl:apply-templates/>
								</p>
							</xsl:if>
							<xsl:if test="position() = last()">
								<p class="EXT-L">
									<xsl:apply-templates/>
								</p>
							</xsl:if>
							<xsl:if test="position()!=last() and position()!=1">
								<p class="EXT-M">
									<xsl:apply-templates/>
								</p>
							</xsl:if>		
						</xsl:when>
						<xsl:when test="local-name()='orderedlist'">
							<ol class="ol-1">
								<xsl:apply-templates/>
							</ol>
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:for-each>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates/>
			</xsl:otherwise>
					
				</xsl:choose>
		</blockquote>
		<xsl:apply-templates select="db:source"/>
	</xsl:template>
	
	<xsl:template match="db:footnote/db:blockquote/db:para" mode="cnote">
		<p>
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	<xsl:template match="db:footnote/db:blockquote/db:para" mode="note">
		<p>
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	<xsl:template match="db:footnote[@role='end-bk-note']/db:bibliolist/db:bibliomixed" mode="note">
		<aside epub:type="footnotes">
			<p epub:type="footnote" class="FN1">
			<a href="{concat(substring-after(substring-after((ancestor::db:chapter|ancestor::db:appendix|ancestor::db:part[@label])[1]/@xml:id,'-'),'-'),'.xhtml#fnt-',ancestor::db:footnote/@label)}" id="{concat(substring-after(substring-after((ancestor::db:chapter|ancestor::db:appendix|ancestor::db:part[@label])[1]/@xml:id,'-'),'-'),'_f-',ancestor::db:footnote/@label)}">
				<b>
					<xsl:value-of select="ancestor::db:footnote/@label"/>
					<xsl:text>. </xsl:text>
				</b>
			</a>
			<xsl:apply-templates/>
		</p>
		</aside>
	</xsl:template>
	
	
	
	<xsl:template match="db:line">
		<xsl:variable name="lines">
			<xsl:choose>
				<xsl:when test="./parent::db:poem">
					<xsl:choose>
						<xsl:when test="./count(preceding-sibling::db:line) = 0">EXTF</xsl:when>
						<xsl:when test="(./count(preceding-sibling::db:line)!= 0) and (./count(following-sibling::db:line)!=0)">EXTM</xsl:when>
						<xsl:otherwise>EXTL</xsl:otherwise>
					</xsl:choose>	
				</xsl:when>
				<xsl:when test="./ancestor::db:dialogue">
					<xsl:choose>
						<xsl:when test="./count(parent::db:speech/preceding-sibling::db:speech) = 0">EXTF</xsl:when>
						<xsl:when test="(./count(parent::db:speech/preceding-sibling::db:speech) &gt;= 1) and ((./count(parent::db:speech/parent::db:dialogue/child::db:speech))-1)!=(./count(parent::db:speech/preceding-sibling::db:speech))">EXTM</xsl:when>
						<xsl:otherwise>EXTL</xsl:otherwise>	
					</xsl:choose>
				</xsl:when>
			</xsl:choose>
			
<!--		<xsl:if test="./parent::db:speech/position() = 1">EXTF</xsl:if>
			<xsl:if test="./parent::db:speech/position() = last()">EXTL</xsl:if>
			<xsl:if test="./parent::db:speech/position() != 1 and ./parent::db:speech/position() != last()">EXTM</xsl:if>
-->		</xsl:variable>
		<p class="{$lines}">
			<xsl:apply-templates/>
			<!--<xsl:apply-templates select="./preceding-sibling::db:speaker/node()"/><xsl:apply-templates/>-->
		</p>
	</xsl:template>
	
	<xsl:template match="db:speaker"/>
	
	<!--<xsl:template match="db:speaker">
		<xsl:variable name="speaker">
			<xsl:if test="position() = 1">EXTF</xsl:if>
			<xsl:if test="position() = last()">EXTL</xsl:if>
			<xsl:if test="position() != 1 and position() != last()">EXTM</xsl:if>
		</xsl:variable>
		<p class="{$speaker}">
			<xsl:apply-templates/>
		</p>
	</xsl:template>-->
	<xsl:template match="db:link[@xlink:xref]/db:uri">
		<xsl:choose>
			<xsl:when test="matches(@xlink:xref, '^http:')">
				<a href="{@xlink:href}">
					<xsl:apply-templates/>
				</a>
			</xsl:when>
			<xsl:otherwise>
				<a href="http://{@xlink:href}">
					<xsl:apply-templates/>
				</a>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	
	<xsl:template match="db:link[@xlink:href]/db:uri">
		<xsl:choose>
			<xsl:when test="@valid='false'">
				<a class="disabled-link" href="{@xlink:href}">
					<xsl:apply-templates/>
				</a>		
			</xsl:when>
			<xsl:otherwise>
				<a href="{@xlink:href}">
					<xsl:apply-templates/>
				</a>
			</xsl:otherwise>
		</xsl:choose>
		
		
	</xsl:template>
	
	<xsl:template match="db:uri[not (parent::db:link)]">
		<xsl:choose>
			<xsl:when test="@valid='false'">
				<a class="disabled-link">
					<xsl:attribute name="href">
						<xsl:value-of select="./@xlink:href" />
					</xsl:attribute>
					<xsl:apply-templates/>
				</a>
			</xsl:when>
			<xsl:otherwise>
				<a>
					<xsl:attribute name="href">
						<xsl:value-of select="./@xlink:href" />
					</xsl:attribute>
					<xsl:apply-templates/>
				</a>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	
	<xsl:template match="db:biblioset/db:address">
		<p class="Copy-TXT">
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	<xsl:template match="db:phrase">
		<p class="Copy-TXT">
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	<xsl:template match="db:printhistory/db:para">
		<p class="Copy-TXT">
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	<xsl:template match="db:bibliomisc[@role='imprint']"/>
		
		<!--<p class="Copy-TXT">
			<xsl:apply-templates/>
		</p>-->
		
		
		<!--<xsl:template match="db:equation/db:mediaobject/db:imageobject/db:imagedata">
			<p class="image-fig" id="{substring-after(@fileref,'/')}">
				<img src="../{@fileref}" alt=""/>
			</p>
		</xsl:template>
		
		
	<xsl:template match="db:inlineequation/db:inlinemediaobject/db:imageobject/db:imagedata">
		
			<img src="../{@fileref}" alt=""/>
		
	</xsl:template>-->
	
	<xsl:template match="db:a">
		<span epub:type="pagebreak" title="">
			<xsl:attribute name="id"><xsl:value-of select="./@id"/></xsl:attribute>
		<xsl:apply-templates/>
		</span>
	</xsl:template>

	
	<xsl:template match="db:equation">
		<p class="image-fig" id="{substring-after(./descendant::db:imagedata/@fileref,'/')}">
			<img src="../{./descendant::db:imagedata/@fileref}" alt=""/>
			<xsl:apply-templates/>
		</p>
	</xsl:template>
		
		
	<xsl:template match="db:inlineequation/db:inlinemediaobject/db:imageobject/db:imagedata">
		<img src="../{@fileref}" alt=""/>
	</xsl:template>
	
	<xsl:template match="mml:math">
		
		<xsl:copy-of select="."/>
		
		
	</xsl:template>
	
		
	<!--<xsl:template match="db:chapter/db:info/db:authorgroup|db:appendix/db:info/db:authorgroup">
		<p class="CA">
			<xsl:for-each select="db:author">
				<xsl:choose>
					<xsl:when test="position() = last() and not(position()=1)">
						<xsl:text>and </xsl:text>
						<xsl:apply-templates/>			
					</xsl:when>
					<xsl:when test="position() = last() - 1 and position()=1">
						<xsl:apply-templates/>			
					</xsl:when>
					<xsl:when test="position() = last() and position()=1">
						<xsl:apply-templates/>			
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates/>
						<xsl:text>, </xsl:text>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:for-each>
		</p>
		</xsl:template>-->
	
	<xsl:template match="db:chapter/db:info[child::db:author]">
		<p class="CA"><xsl:apply-templates/></p>
	</xsl:template>
	<xsl:template match="navMap">
		<navMap xmlns="http://www.daisy.org/z3986/2005/ncx/">
			<xsl:apply-templates/>
		</navMap>
	</xsl:template>
	<xsl:template match="navLabel">
		<navLabel xmlns="http://www.daisy.org/z3986/2005/ncx/">
			<xsl:apply-templates/>
		</navLabel>
	</xsl:template>
	<xsl:template match="content">
		<content xmlns="http://www.daisy.org/z3986/2005/ncx/" src="{@src}">
			<xsl:apply-templates/>
		</content>
	</xsl:template>
	<xsl:template match="text">
		<text xmlns="http://www.daisy.org/z3986/2005/ncx/">
			<xsl:apply-templates/>
		</text>
	</xsl:template>
	<xsl:template match="navPoint">
		<xsl:variable name="id" select="concat('ncx', generate-id())" />
		<navPoint xmlns="http://www.daisy.org/z3986/2005/ncx/" id="ncx{position()}" playOrder="{position()}">
			<xsl:apply-templates/>
		</navPoint>
	</xsl:template>
	
	<xsl:template match="navPointroot">
		<xsl:apply-templates/>
	</xsl:template>
	
	<xsl:template name="incrementValue">
		<xsl:param name="value"/>
		<xsl:if test="$value &lt;= 10">
			<xsl:value-of select="$value"/>
			<xsl:call-template name="incrementValue">
				<xsl:with-param name="value" select="$value + 1"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>
	<xsl:template match="db:cover[1]">
		<xsl:result-document href="{translate($bid,'-','')}/OEBPS/xhtml/series.xhtml" method="xml" indent="yes" use-character-maps="hex">				
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<title>SERIES</title>
					<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
				</head>
				<body epub:type="frontmatter">
					<section>
					
					<xsl:apply-templates/>
					<xsl:apply-templates select="following-sibling::db:cover"/>
					
					</section>
				</body>
			</html>
		</xsl:result-document>
	</xsl:template>
	<xsl:template match="db:cover/db:bibliolist/db:bibliomixed/db:bibliomisc[@role='description']">
		<p class="FM_Series">
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	
	<xsl:template match="db:cover/db:bibliolist/db:bibliomixed/db:bibliomisc[@role='heading1']">
		<p class="FM_Seriest" id="series_1">
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	<xsl:template match="db:cover/db:bibliolist/db:bibliomixed/db:bibliomisc[@role='heading2']">
		<p class="FM_Seriest">
			<xsl:apply-templates/>
		</p>
	</xsl:template>
	<xsl:template match="db:alt"/>

	<xsl:template match="db:minitoc/db:para">
		<p class="TOC-CH-sec" id="re_{@rid}">
			<a href="{substring-after(substring-after(./ancestor::db:chapter/@xml:id,'-'),'-')}.xhtml#{@rid}"><xsl:apply-templates/></a>
		</p>
	</xsl:template>
	
	
	
	<xsl:template match="db:indexterm|db:footnote[@role='end-bk1-note']/db:para|db:dedication/db:info/db:title"/>	
	<xsl:template match="db:part[@label]">
		<xsl:result-document method="xml" href="{translate($bid,'-','')}/OEBPS/xhtml/{substring-after(substring-after(@xml:id,'-'),'-')}.xhtml" encoding="utf-8"  indent="yes" use-character-maps="hex">
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<title>
						<xsl:text>PART </xsl:text>
						<xsl:value-of select="@label"/>
						<xsl:value-of select="db:info/db:title"/>
					</title>
					<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
				</head>
				<body epub:type="bodymatter">
					<section>
					<p class="PN">
						<a href="contents.xhtml#re_{substring-after(substring-after(@xml:id,'-'),'-')}">
							<xsl:text>PART </xsl:text>
							<xsl:value-of select="@label"/>
						</a>
					</p>
					<xsl:apply-templates select="(db:info/db:title , db:partintro)"/>
					</section>
				</body>
			</html>			
		</xsl:result-document>
		<xsl:apply-templates select="node() except db:partintro"/>
	</xsl:template>	
	<xsl:template match="db:preface[(contains(@xml:id,'tab'))]">
		<xsl:result-document method="xml" href="{translate($bid,'-','')}/OEBPS/xhtml/{substring-after(substring-after(@xml:id,'-'),'-')}.xhtml" encoding="utf-8"  indent="yes" use-character-maps="hex">
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<title>table</title>
					<!--<title> 						
						<xsl:value-of select="db:table/db:info/db:title"/>
						</title>-->					
					<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
				</head>
				<body epub:type="bodymatter">
					<section>
						
						<h1 epub:type="title" class="FMT" id="{substring-after(substring-after(@xml:id,'-'),'-')}">
						<a href="contents.xhtml#re_{substring-after(substring-after(@xml:id,'-'),'-')}">
							<xsl:value-of select="db:info/db:title"/>
						</a>
					</h1>
					<xsl:for-each select="ancestor::db:book//db:chapter">
						<xsl:if test="descendant::db:table/db:label and descendant::db:table/db:info/db:title">
							<p class="TXT">Chapter <xsl:value-of select="@label"/>
							</p>
							<xsl:for-each select="descendant::db:table">
								<p class="TXT"> 
									<a href="{substring-after(substring-after(ancestor::db:chapter/@xml:id,'-'),'-')}.xhtml#{attribute::xml:id}" id="{attribute::xml:id}">
										<xsl:value-of select="db:label"/><xsl:apply-templates select="db:info/db:title" mode="ill"/>
									</a>
								</p>
								
							</xsl:for-each>
						</xsl:if>
						
						
					</xsl:for-each>
					</section>
				</body>
			</html>			
		</xsl:result-document>
	</xsl:template>
	<xsl:template match="db:preface[(contains(@xml:id,'fig'))]">
		<xsl:result-document method="xml" href="{translate($bid,'-','')}/OEBPS/xhtml/{substring-after(substring-after(@xml:id,'-'),'-')}.xhtml" encoding="utf-8"  indent="yes" use-character-maps="hex">
			<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
			<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="en" lang="en">
				<head>
					<title>
						<xsl:value-of select="./db:info/db:title"/>
					</title>
					<link href="../styles/stylesheet.css" rel="stylesheet" type="text/css"/>
				</head>
				<body>
					<p class="CT" id="{substring-after(substring-after(@xml:id,'-'),'-')}">
						<a href="contents.xhtml#re_{substring-after(substring-after(@xml:id,'-'),'-')}">
							<xsl:value-of select="db:info/db:title"/>
						</a>
					</p>
					<xsl:for-each select="ancestor::db:book//db:chapter">
						<xsl:if test="descendant::db:figure/db:label and descendant::db:figure/db:caption">
							<xsl:for-each select="descendant::db:figure">
								<p class="TXT">
									<a href="{substring-after(substring-after(ancestor::db:chapter/@xml:id,'-'),'-')}.xhtml#{substring-after(db:mediaobject/db:imageobject/db:imagedata/@fileref,'/')}" id="ill_{substring-after(db:mediaobject/db:imageobject/db:imagedata/@fileref,'/')}">
										<xsl:value-of select="db:label"/>
									</a><xsl:text disable-output-escaping="yes"> </xsl:text><xsl:apply-templates select="db:caption/db:para/node()"/>
								</p>
							</xsl:for-each>
						</xsl:if>
					</xsl:for-each>
					<xsl:for-each select="ancestor::db:book//db:chapter">
						<xsl:if test="descendant::db:table/db:label and descendant::db:table/db:info/db:title">
							<xsl:for-each select="descendant::db:table">
								<p class="TXT"> 
									<a href="{substring-after(substring-after(ancestor::db:chapter/@xml:id,'-'),'-')}.xhtml#{attribute::xml:id}" id="{attribute::xml:id}">
										<xsl:value-of select="db:label"/><xsl:apply-templates select="db:info/db:title" mode="ill"/>
									</a>
								</p>
								
							</xsl:for-each>
						</xsl:if>
					</xsl:for-each>
				</body>
			</html>			
		</xsl:result-document>
	</xsl:template>
</xsl:stylesheet>
