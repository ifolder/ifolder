function addtofav(linkUrl,linkTitle){
   if (!document.all) {
     top.location.replace(linkUrl);
     alert('Please press CTRL-D to bookmark this page');     
   }
   else external.AddFavorite(linkUrl,linkTitle);
   return false;
}

document.write('<a href="#" class="navigation" onClick="window.open(\'http://www.novell.com/inc/rater.jsp?url=\' + ');
document.write('document.location.href,\'_blank\',\'height=165,width=225,menubar=no,status=no\');">');
self.document.writeln('  <img src="../../../ui/images/body_feedback.gif" border=0 alt="Pripomienky"></a>');
self.document.writeln('  <img src="../../../ui/images/virt_dot-line.gif" border="0" alt="">');
self.document.writeln('  <a href="javascript:;" class="navigation" onClick="addtofav(window.location.href,document.title); return false"><img src="../../../ui/images/body_bookmark.gif" border=0 alt="Vytvoriť záložku tejto stránky"></a>');
