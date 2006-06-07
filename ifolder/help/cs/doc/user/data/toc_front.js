function updateMenu_front() {

   if ((parent.theMenu) && (parent.theMenu.amBusy == false))
   {
      var level0ID = parent.theMenu.currentSubRoot;

      var level1ID;
      var level2ID;
      var level3ID;
     level1ID = parent.theMenu.findEntry('data/front.html', 'url', 'right');
     if (level1ID == -1)
      {
      level1ID = parent.theMenu.addChild(level0ID, "", "Nápověda aplikace iFolder", "../doc/user/data/front.html", "zFLDR xB.0000.0000.", "Nápověda aplikace iFolder");
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "Přehled aplikace iFolder", "../doc/user/data/overview.html", "zFLDR xC.0000.0000.0001.", "Přehled aplikace iFolder");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../doc/user/data/overview.html","../doc/user/data/toc_overview.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "Správa účtů aplikace iFolder a předvoleb", "../doc/user/data/accountsprefs.html", "zFLDR xC.0000.0000.0002.", "Správa účtů aplikace iFolder a předvoleb");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../doc/user/data/accountsprefs.html","../doc/user/data/toc_accountsprefs.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "Správa složek iFolder", "../doc/user/data/myifolders.html", "zFLDR xC.0000.0000.0003.", "Správa složek iFolder");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../doc/user/data/myifolders.html","../doc/user/data/toc_myifolders.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Document", "Právní upozornění", "../doc/user/data/legal.html", "zHTML xC.0000.0000.0004.", "Právní upozornění");
      parent.theMenu.entry[level1ID].setSelected(true);
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_front();", 1000);
   }

}

updateMenu_front();
