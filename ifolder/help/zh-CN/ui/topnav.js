self.document.writeln('<table width="100%" border="0" align="center">\n<tr>\n<td align="left" width="33%">');
if (typeof prev_link != 'undefined')
{
self.document.writeln('  <a href="'+ prev_link + '" class="navigation"><img src="../../../ui/images/prev.gif" border=0 alt="Previous"></a>');
self.document.writeln('  <img src="../../../ui/images/virt_dot-line.gif" border="0" alt="">');
}
if (typeof next_link != 'undefined')
{
self.document.writeln('  <a href="'+ next_link + '" class="navigation"><img src="../../../ui/images/next.gif" border=0 alt="Next"></a>');
}
self.document.writeln('  </td>\n<td align="center" width=33%>&nbsp;</td>');
self.document.writeln('  <td align="right" width="33%">'); 
self.document.writeln('  </td>');
self.document.writeln('  </table>'); 