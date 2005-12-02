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
      level1ID = parent.theMenu.addChild(level0ID, "", "iFolder súgó", "../doc/user/data/front.html", "zFLDR xB.0000.0000.", "iFolder súgó");
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "Az iFolder áttekintése", "../doc/user/data/overview.html", "zFLDR xC.0000.0000.0001.", "Az iFolder áttekintése");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../doc/user/data/overview.html","../doc/user/data/toc_overview.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "Az iFolder-fiókok és a beállítások kezelése", "../doc/user/data/accountsprefs.html", "zFLDR xC.0000.0000.0002.", "Az iFolder-fiókok és a beállítások kezelése");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../doc/user/data/accountsprefs.html","../doc/user/data/toc_accountsprefs.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "iFolder mappák kezelése", "../doc/user/data/myifolders.html", "zFLDR xC.0000.0000.0003.", "iFolder mappák kezelése");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../doc/user/data/myifolders.html","../doc/user/data/toc_myifolders.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Document", "Jogi közlemények", "../doc/user/data/legal.html", "zHTML xC.0000.0000.0004.", "Jogi közlemények");
      parent.theMenu.entry[level1ID].setSelected(true);
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_front();", 1000);
   }

}

updateMenu_front();
