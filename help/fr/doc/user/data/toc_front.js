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
      level1ID = parent.theMenu.addChild(level0ID, "", "Aide sur iFolder", "../doc/user/data/front.html", "zFLDR xB.0000.0000.", "Aide sur iFolder");
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "Présentation de iFolder", "../doc/user/data/overview.html", "zFLDR xC.0000.0000.0001.", "Présentation de iFolder");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../doc/user/data/overview.html","../doc/user/data/toc_overview.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "Gestion des comptes et des préférences iFolder", "../doc/user/data/accountsprefs.html", "zFLDR xC.0000.0000.0002.", "Gestion des comptes et des préférences iFolder");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../doc/user/data/accountsprefs.html","../doc/user/data/toc_accountsprefs.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "Gestion des dossiers iFolder", "../doc/user/data/myifolders.html", "zFLDR xC.0000.0000.0003.", "Gestion des dossiers iFolder");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../doc/user/data/myifolders.html","../doc/user/data/toc_myifolders.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Document", "Mentions légales", "../doc/user/data/legal.html", "zHTML xC.0000.0000.0004.", "Mentions légales");
      parent.theMenu.entry[level1ID].setSelected(true);
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_front();", 1000);
   }

}

updateMenu_front();
