<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN">
<% 
    String url = request.getHeader("referer");
    if (url==null) 
    {
        url = request.getParameter("url");
    }
%>
<html>
<head>
	<title>Rate this article</title>
	<meta http-equiv="Content-Type" content="text/html; charset=Windows-1252">
	<STYLE type="text/css">
	<!--
	.form {font-family: Arial, Helvetica, sans-serif; font-size: 11px; font-weight: normal; width:270px;}
	.main {FONT-FAMILY: Arial, Helvetica, Sans-serif; FONT-SIZE: 10px; FONT-STYLE: NORMAL; LINE-HEIGHT: 12px}
	.head {FONT-FAMILY: Arial, Helvetica, Sans-serif; FONT-SIZE: 9px; FONT-STYLE: NORMAL}
	.sub {FONT-FAMILY: Arial, Helvetica, sans-serif; FONT-SIZE: 12px}
	.table{FONT-FAMILY: Arial, Helvetica, sans-serif; FONT-SIZE: 10px} 
	-->
	</STYLE>
	<SCRIPT LANGUAGE="javascript" TYPE="text/javascript"> 
	<!--
	var isNS6=((navigator.userAgent.toLowerCase().indexOf('mozilla')!=-1)&&(parseInt(navigator.appVersion)>=5));
	var isNS4=(document.layers)?1:0;
	var isIE4=(document.all)?1:0;
	var isMAC=(navigator.appVersion.indexOf("Macintosh") > -1);
	var isLinux=(navigator.appVersion.toUpperCase().indexOf("LINUX") > -1);
	
	function resize() 
	{
	  self.setResizable = true;
	  if(isNS4){commonHeight=100}else{commonHeight=80}  
	  if (isNS4 && !isMAC) {var outerWidth = 220 + 0; var outerHeight = commonHeight + 200;}
	  if (isNS4 && isMAC) {var outerWidth = 220; var outerHeight = commonHeight + 98;}
	  if (isNS4 && isLinux) {var outerWidth = 220 + 0; var outerHeight = commonHeight + 108;}
	  if (isIE4 && !isMAC) {var outerWidth = 220 + 12; var outerHeight = commonHeight + 102;}
	  if (isIE4 && isMAC) {var outerWidth = 220 + 0; var outerHeight = commonHeight + 90;}
	  if (isNS6 && !isMAC) {var outerWidth = 220 + 9; var outerHeight = commonHeight + 110;}
	  if (isNS6 && isMAC) {var outerWidth = 220 + 0; var outerHeight = commonHeight + 90;}
	  
		if (isNS4)
		{
			self.setResizable(true);
			self.outerHeight=500;
			self.outerWidth=300;
			self.setResizable(false);
			resizeSelf();
		}
		else
		{
			self.resizeTo(outerWidth,outerHeight);
		}
	}
	
	function rate(rating)
	{
			document.forms[0].rater.value=rating;
	}

	function checkRater()
	{
			var rating = document.ratepage.rater.value;
				if (rating.length < 1)
				{
					alert ("You must click a rating");
					return false;	
				}
			return true;
	}
	-->
    </SCRIPT>
<link rel="stylesheet" href="/inc/novell_style.css">
</head>
<body onload="resize();" leftmargin="0" topmargin="0" marginwidth="0" marginheight="0" background="/img/t3_leftnavbg.gif">
<form name="ratepage" onsubmit="return checkRater();" method="post" action="/servlet/com.novell.webresults.feedback.FeedbackServlet">
<input type="hidden" name="new_rating_url" value="<%= url %>">
<input type="hidden" name="new_rating" value="1">
<input type="hidden" name="rater" value=""> 

<table width="225" border="0" cellspacing="0" cellpadding="0">
  <tr> 
    <td background="/img/t2_bg_1.gif">
      <table width="100%" border="0" cellspacing="0" cellpadding="0">
        <tr>
          <td width="23"><img src="/img/t2_arrow_1.gif" height="20" width="20"></td>
          <td class="hdrRegionText">rate this article</td> 
        </tr>
      </table>
    </td>
  </tr>
  <tr> 
    <td background="/img/t3_leftnavbg.gif"> 
      <table width="200" border="0" cellspacing="0" cellpadding="0" align="center">
        <tr> 
          <td colspan="6"><img src="/img/spacer.gif" width="20" height="10"></td>
        </tr>
        <tr> 
          <td width="20"> 
            <div align="center"> 
              <input type="radio" name="rating" onclick="rate(this.value);" value="1" background="/img/t3_leftnavbg.gif">
            </div>
          </td>
          <td width="20"> 
            <div align="center"> 
              <input type="radio" name="rating" onclick="rate(this.value);" value="2" background="/img/t3_leftnavbg.gif">
            </div>
          </td>
          <td width="20"> 
            <div align="center"> 
              <input type="radio" name="rating" onclick="rate(this.value);" value="3" background="/img/t3_leftnavbg.gif">
            </div>
          </td>
          <td width="20"> 
            <div align="center"> 
              <input type="radio" name="rating" onclick="rate(this.value);" value="4" background="/img/t3_leftnavbg.gif">
            </div>
          </td>
          <td width="20">  
            <div align="center"> 
              <input type="radio" name="rating" onclick="rate(this.value);" value="5" background="/img/t3_leftnavbg.gif">
            </div>
          </td>
          <td class="bodyCopy">
            <div align="right">5 = most useful</div>
          </td>
        </tr>
        <tr> 
          <td class="bodyCopy"> 
            <div align="center">1</div>
          </td>
          <td class="bodyCopy"> 
            <div align="center">2</div>
          </td>
          <td class="bodyCopy"> 
            <div align="center">3</div>
          </td>
          <td class="bodyCopy"> 
            <div align="center">4</div>
          </td>
          <td class="bodyCopy">  
            <div align="center">5</div>
          </td>
          <td class="bodyCopy">
            <div align="right"><input type="image" name="submitButton" src="/img/rater_submit.gif" border="0"></div>
          </td>
        </tr>
		<tr> 
          <td colspan="6" class="bodyCopy" valign="bottom" height="20">Comments?</td>
        </tr>
		<tr> 
          	<td colspan="6" class="bodyCopy"><textarea class="bodyCopy" name="COMMENT" rows="3" cols="20"></textarea></td>
        </tr>
        <tr> 
          <td colspan="6"><img src="/img/spacer.gif" width="30" height="10"></td>
        </tr>
      </table>
    </td>
  </tr>
</table>



<!--
<TABLE WIDTH="375" BORDER="0" CELLSPACING="0" CELLPADDING="0">
  <TR>
    <TD BGCOLOR="#333333"><IMG height=35 src="img/novell_head.gif" width=84></TD>
  </TR>
  <TR>
	<TD BGCOLOR="#999999" CLASS="SUB"><IMG height=1 src="img/spacer.gif" width=16>Rate this page</TD>
  </TR>
</TABLE>

<table cellspacing="0" border="1"><tr><td bgcolor="cccccc"> 
<table width="370" border="0" cellspacing="0" cellpadding="0" BGCOLOR="#cccccc">
	  <tr>
	    <td colspan="2" class="main">Please rate this page on a scale from 1-5 with 5 being the best.
		</td>
	  </tr>
</table>
<table width="370">
	<tr>
		<td><div align="center">1</div></td>
		<td><div align="center">2</div></td>
		<td><div align="center">3</div></td>
		<td><div align="center">4</div></td>
		<td><div align="center">5</div></td>
	</tr>
	<tr>
		<td><div align="center"><input type="radio" name="rating" value="1"></div></td>
		<td><div align="center"><input type="radio" name="rating" value="2"></div></td>
		<td><div align="center"><input type="radio" name="rating" value="3"></div></td>
		<td><div align="center"><input type="radio" name="rating" value="4"></div></td>
		<td><div align="center"><input type="radio" name="rating" value="5"></div></td>
 </tr>
</table>
<TABLE CLASS="SUB" WIDTH="375" BORDER="0" CELLSPACING="0" CELLPADDING="0" bgcolor="#333333">
  <TR>
    <TD width="" height="40">&nbsp;</TD>
	<TD><div align="center"><input type="submit" name="Submit" value="Submit Rating" class="SUB"></div></TD>
  </TR>

</table>

-->
<input type="hidden" value="true" name="submit">
</td></td></table>
</form>
</body>
</html>
