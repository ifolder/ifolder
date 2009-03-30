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
      level1ID = parent.theMenu.addChild(level0ID, "", "Справка iFolder", "../doc/user/data/front.html", "zFLDR xB.0000.0000.", "Справка iFolder");
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "Обзор iFolder", "../doc/user/data/overview.html", "zFLDR xC.0000.0000.0001.", "Обзор iFolder");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../doc/user/data/overview.html","../doc/user/data/toc_overview.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "Управление учетными записями и настройками iFolder", "../doc/user/data/accountsprefs.html", "zFLDR xC.0000.0000.0002.", "Управление учетными записями и настройками iFolder");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../doc/user/data/accountsprefs.html","../doc/user/data/toc_accountsprefs.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "Управление папками iFolder", "../doc/user/data/myifolders.html", "zFLDR xC.0000.0000.0003.", "Управление папками iFolder");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../doc/user/data/myifolders.html","../doc/user/data/toc_myifolders.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Document", "Юридическая информация", "../doc/user/data/legal.html", "zHTML xC.0000.0000.0004.", "Юридическая информация");
      parent.theMenu.entry[level1ID].setSelected(true);
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_front();", 1000);
   }

}

updateMenu_front();
