function ulinks(){
if(!document.getElementsByTagName){
return;
}
var _1=document.getElementsByTagName("a");
for(var i=0;i<_1.length;i++){
var _3=_1[i];
if(_3.getAttribute("href")&&_3.className=="ulink"){
_3.target="_blank";
}
}
}
window.onload=ulinks;
self.document.writeln("<table width=\"100%\" border=\"0\" align=\"center\">\n<tr>\n<td align=\"left\" width=\"33%\">");
if(typeof prev_link!="undefined"){
self.document.writeln("<a href=\""+prev_link+"\" class=\"nav\"><img src=\""+html2ui+"images/prev.png\" align=\"absmiddle\" border=\"0\" alt=\""+prevstr+"\">&nbsp;"+prevstr+"</a>");
self.document.writeln("&nbsp;<img src=\""+html2ui+"images/virt_dot-line.gif\" align=\"absmiddle\" border=\"0\" alt=\"\">&nbsp;");
}
if(typeof next_link!="undefined"){
self.document.writeln("<a href=\""+next_link+"\" class=\"nav\">"+nextstr+"&nbsp;<img src=\""+html2ui+"images/next.png\" align=\"absmiddle\" border=0 alt=\""+nextstr+"\"></a>");
}
self.document.writeln("</td>\n<td align=\"center\" width=33%>&nbsp;</td>");
self.document.writeln("<td align=\"right\" width=\"33%\">");
self.document.writeln("</td>");
self.document.writeln("</table>");

