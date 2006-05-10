// created on 7/23/2004 at 1:37 PM
<!-- getRSS.xsl: retrieve RSS feed(s) and convert to HTML. -->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
     xmlns:dc="http://purl.org/dc/elements/1.1/" version="1.0">

  <xsl:output method="html"/>

  <xsl:template match="RSSChannels">
    <html><head><title>Today's Headlines</title></head>
    <style><xsl:comment>

p         { font-size: 8pt;
            font-family: arial,helvetica; }

h1        { font-size: 12pt;
            font-family: arial,helvetica; 
            font-weight: bold; }

a:link    { color:blue;
            font-weight: bold;
            text-decoration: none; }

a:visited { font-weight: bold;
            color: darkblue;
            text-decoration: none; }

   </xsl:comment></style> 
   <body>
     <xsl:apply-templates/>
   </body></html>
  </xsl:template>

  <xsl:template match="RSSChannel">
    <xsl:apply-templates select="document(@src)"/>
  </xsl:template>

  <!-- Named template outputs HTML a element with href link and RSS
       description as title to show up in mouseOver message. -->
  <xsl:template name="a-element">
    <xsl:element name="a">
      <xsl:attribute name="href">
        <xsl:apply-templates select="*[local-name()='link']"/>
      </xsl:attribute>
      <xsl:attribute name="title">
        <xsl:apply-templates select="*[local-name()='description']"/>
      </xsl:attribute>
      <xsl:value-of select="*[local-name()='title']"/>
    </xsl:element>
  </xsl:template>

  <!-- Output RSS channel name as HTML a link inside of h1 element. -->
  <xsl:template match="*[local-name()='channel']">
    <xsl:element name="h1">
      <xsl:call-template name="a-element"/>
    </xsl:element> 
    <!-- Following line for RSS .091 -->
    <xsl:apply-templates select="*[local-name()='item']"/>
  </xsl:template>

  <!-- Output RSS item as HTML a link inside of p element. -->
  <xsl:template match="*[local-name()='item']">
    <xsl:element name="p">
      <xsl:call-template name="a-element"/>
      <xsl:text> </xsl:text>
      <xsl:if test="dc:date"> <!-- Show date if available -->
        <xsl:text>( </xsl:text>
        <xsl:value-of select="dc:date"/>
        <xsl:text>) </xsl:text>
      </xsl:if>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>