var key = document.location.pathname.replace(/\\/g,'/');
if(key.length > 0)
{
	var i; var j = 0;
	for (i=key.length-1;i>0;i--)	{
		if(key.charAt(i)=='\\' || key.charAt(i)=='/') {j++;}
		if(j>3) break;
	}
	self.document.writeln('<script language="javascript1.2" type="text/javascript" src="http://localhost/docadd/comments.php?page='+'doc_'+ key.substring(i+1)+'"></script>');
}

self.document.writeln('<table border="0" align="center" width="90%">\n<tr>\n<td width="33%">&nbsp;</td>\n<td align="center" width="33%">');
if (typeof prev_link != 'undefined')
{
self.document.writeln('  <a href="'+ prev_link + '" class="navigation"><img src="../../../docui2002/images/prev.gif" border=0 alt="Previous"></a>');
self.document.writeln('  <img src="../../../docui2002/images/virt_dot-line.gif" border="0" alt="">');
}
if (typeof next_link != 'undefined')
{
self.document.writeln('  <a href="'+ next_link + '" class="navigation"><img src="../../../docui2002/images/next.gif" border=0 alt="Next"></a>');

}
self.document.writeln('  </td>\n');
self.document.writeln('  <td width="33%">&nbsp;</td>');
self.document.writeln('  </table>');
    