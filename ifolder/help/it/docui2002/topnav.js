function addtofav(linkUrl,linkTitle){
   if (!document.all) {
     top.location.replace(linkUrl);
     alert('Please press CTRL-D to bookmark this page');     
   }
   else external.AddFavorite(linkUrl,linkTitle);
   return false;
}


self.document.writeln('<table width="100%" border="0" align="center">\n<tr>\n<td align="left" width="33%">');
if (typeof prev_link != 'undefined')
{
self.document.writeln('  <a href="'+ prev_link + '" class="navigation"><img src="../../../docui2002/images/prev.gif" border=0 alt="Previous"></a>');
self.document.writeln('  <img src="../../../docui2002/images/virt_dot-line.gif" border="0" alt="">');
}
if (typeof next_link != 'undefined')
{
self.document.writeln('  <a href="'+ next_link + '" class="navigation"><img src="../../../docui2002/images/next.gif" border=0 alt="Next"></a>');
}
self.document.writeln('  </td>\n<td align="center" width=33%>&nbsp;</td>');
self.document.writeln('  <td align="right" width="33%">'); 
document.write('<a href="#" class="navigation" onClick="window.open(\'http://www.novell.com/inc/rater.jsp?url=\' + ');
document.write('document.location.href,\'_blank\',\'height=165,width=225,menubar=no,status=no\');">');
self.document.writeln('  <img src="../../../docui2002/images/body_feedback.gif" border=0 alt="Feedback"></a>');
self.document.writeln('  <img src="../../../docui2002/images/virt_dot-line.gif" border="0" alt="">');
self.document.writeln('  <a href="javascript:;" class="navigation" onClick="addtofav(window.location.href,document.title); return false"><img src="../../../docui2002/images/body_bookmark.gif" border=0 alt="Bookmark this page"></a>');
self.document.writeln('  </td>');
self.document.writeln('  </table>'); 