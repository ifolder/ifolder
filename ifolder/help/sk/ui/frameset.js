if(self.name=="menu"){
self.location.href="menu.htm";
}else{
initialise();
var thePage=pageFromSearch(frontFile,theMenu,true);
self.document.writeln("  <frameset cols=\"230,*,0\" rows=\"100%\" onResize=\"defaultResizeHandler();\" frameborder=\"yes\" border=\"5\" bordercolor=\"#BABDB6\" framespacing=\"5\" onLoad=\"loaded = true\">");
self.document.writeln("    <frameset cols=\"100%\" rows=\"*,0\" frameborder=\"no\" border=\"0\" framespacing=\"0\">");
self.document.writeln("      <frame name=\"menu\" src=\"ui/menu.htm\" scrolling=\"auto\" marginwidth=\"1\" marginheight=\"1\" application=\"yes\">");
self.document.writeln("      <frame name=\"menuChildLoader\" src=\"ui/blank.html\" scrolling=\"no\" marginwidth=\"0\" marginheight=\"0\" application=\"yes\">");
self.document.writeln("    </frameset>");
self.document.writeln("    <frame name=\"text\" src=\""+thePage+"\" scrolling=\"auto\" application=\"yes\">");
self.document.writeln("  </frameset>");
}

