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
      level1ID = parent.theMenu.addChild(level0ID, "", "iFolder Help", "../doc/user/data/front.html", "zFLDR xB.0000.0000.", "iFolder Help");
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "Managing iFolders", "../doc/user/data/bq6lwle.html", "zFLDR xC.0000.0000.0001.", "Managing iFolders");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../doc/user/data/bq6lwle.html","../doc/user/data/toc_bq6lwle.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "Managing iFolder Contacts", "../doc/user/data/bq6lx19.html", "zFLDR xC.0000.0000.0002.", "Managing iFolder Contacts");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../doc/user/data/bq6lx19.html","../doc/user/data/toc_bq6lx19.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Document", "Legal Notices", "../doc/user/data/legal.html", "zHTML xC.0000.0000.0003.", "Legal Notices");
      parent.theMenu.entry[level1ID].setSelected(true);
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_front();", 1000);
   }

}

updateMenu_front();
