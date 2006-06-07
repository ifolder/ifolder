if (self.name == 'menu') {
	self.location.href = "menu.htm";
} else {
	initialise();
	var thePage = pageFromSearch('treetitl.html', theMenu, true);
	
		self.document.writeln('<frameset cols="100%" rows="62,*,20" onResize="defaultResizeHandler();" frameborder="no" border="0" framespacing="0">');
		self.document.writeln('  <frame name="header" src="../docui2002/hdr_doc.html" scrolling="no" noresize marginwidth="0" marginheight="0" APPLICATION="yes">');
		self.document.writeln('  <frameset cols="230,*,0" rows="100%" frameborder="yes" border="5" bordercolor="#BABDB6" framespacing="5" onLoad="loaded = true">');
		self.document.writeln('    <frameset cols="100%" rows="*,0" frameborder="no" border="0" framespacing="0">');
		self.document.writeln('      <frame name="menu" src="../docui2002/menu.htm" scrolling="auto" marginwidth="1" marginheight="1" APPLICATION="yes">');
		self.document.writeln('      <frame name="menuChildLoader" src="../docui2002/blank.html" scrolling="no" marginwidth="0" marginheight="0" APPLICATION="yes">');
		self.document.writeln('    </frameset>');
		self.document.writeln('    <frame name="text" src="' + thePage +'" scrolling="auto" APPLICATION="yes">');
		self.document.writeln('  </frameset>');
		self.document.writeln('  <frame name="footer" src="/inc/footer_frames.html" scrolling="no" noresize marginwidth="0" marginheight="0" APPLICATION="yes">');
	        self.document.writeln('</frameset>');
}