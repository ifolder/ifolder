function addtofav(linkUrl,linkTitle){
   if (!document.all) {
     top.location.replace(linkUrl);
     alert('Нажмите "CTRL-D", чтобы создать закладку для этой страницы');     
   }
   else external.AddFavorite(linkUrl,linkTitle);
   return false;
}

document.write('<a href="#" class="navigation" onClick="window.open(\'http://www.novell.com/inc/rater.jsp?url=\' + ');
document.write('document.location.href,\'_blank\',\'height=165,width=225,menubar=no,status=no\');">');
self.document.writeln('  <img src="../../../docui2002/images/body_feedback.gif" border=0 alt="Предложения и пожелания"></a>');
self.document.writeln('  <img src="../../../docui2002/images/virt_dot-line.gif" border="0" alt="">');
self.document.writeln('  <a href="javascript:;" class="navigation" onClick="addtofav(window.location.href,document.title); return false"><img src="../../../docui2002/images/body_bookmark.gif" border=0 alt="Создание закладки для страницы"></a>');
