function syncToc()
{     
   if ((self != top) && (parent.theMenu)) {   
              self.document.writeln('<sc'+'ript language="javaScript">\n'); 
              self.document.writeln('function syncTocPage()\n');
              self.document.writeln('{\n');
              self.document.writeln('	var eID = parent.theMenu.findEntry(location.pathname, "url", "right", 0);\n'); 
              self.document.writeln('   if ((self != top) && (parent.theMenu) && (parent.theMenu.amBusy == false) && (eID >= 0) && (parent.theMenu.entry[eID].FirstChild != -2)) {\n');  
              self.document.writeln('      parent.theMenu.selectEntry(eID);\n'); 
              self.document.writeln('      if (parent.theMenu.setEntry(eID, true)) {\n');  
              self.document.writeln('         parent.theMenu.refresh(); }\n'); 
              self.document.writeln('   }\n');
              self.document.writeln('   else {\n');  
              self.document.writeln('      if(parent.theMenu.firstEntry > -1) {\n'); 
              self.document.writeln('         var menuURLItems = parent.theMenu.entry[parent.theMenu.firstEntry].url.split("/"); \n');  
              self.document.writeln('         var pathName = location.pathname.replace(/\\\\/g,"/"); \n');  
              self.document.writeln('         var pageURLItems = pathName.split("/"); \n');  
			  self.document.writeln('         for(i=0;i<menuURLItems.length;i++) {\n');
			  self.document.writeln('             if(menuURLItems[i] != "..") break; }\n');
			  self.document.writeln('         var idx = pageURLItems.length - (menuURLItems.length - i); \n');
			  self.document.writeln('         if(pageURLItems[idx+1] != menuURLItems[i+1]) { \n');
	          self.document.writeln('            if (parseInt(navigator.appVersion) >= 3) {top.location.replace(location.pathname);} else {top.location.href = location.pathname;} \n');
			  self.document.writeln('         } else if(pageURLItems[idx] != menuURLItems[i]) { \n');
	          self.document.writeln('            if (parseInt(navigator.appVersion) >= 3) {top.location.replace(location.pathname);} else {top.location.href = location.pathname;} \n');
			  self.document.writeln('         } \n');
			  self.document.writeln('      } \n');
              self.document.writeln('      setTimeout("syncTocPage()",300);\n');
              self.document.writeln('   }\n');
              self.document.writeln('}\n');                 
              self.document.writeln('syncTocPage();\n'); 
              self.document.writeln('</sc'+'ript>\n');

   } 
   else 
   {
        if (typeof top.navPrinting == 'undefined') 
        {
            top.navPrinting = false;  
        }

        if ((navigator.appName + navigator.appVersion.substring(0, 1)) == "Netscape4") {
		top.navPrinting = (self.innerHeight == 0) && (self.innerWidth == 0);}

        if (self.location.href.indexOf("/NSearch/HighlightServlet") >= 0)
		{
			top.navPrinting = true;
		}

		if (!top.navPrinting) {
			var newLoc = "../../index.html?page=" + escape(self.location.pathname)+self.location.hash;
	        if (parseInt(navigator.appVersion) >= 3) {self.location.replace(newLoc);} else {self.location.href = newLoc;}
	}
   }  
                                                           

}
syncToc();
top.document.title = document.title;