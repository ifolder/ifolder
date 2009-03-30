function updateMenu_overview() {

   if ((parent.theMenu) && (parent.theMenu.amBusy == false))
   {
      var level0ID = parent.theMenu.currentSubRoot;

      var level1ID;
      var level2ID;
      var level3ID;
     level2ID = parent.theMenu.findEntry('data/overview.html', 'url', 'right');
     if ((level2ID != -1) && (parent.theMenu.entry[level2ID].FirstChild < 0))
      {
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Condivisione con iFolder", "../doc/user/data/bx3aueg.html", "zHTML xD.0000.0000.0001.0001.", "Condivisione con iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Vantaggi di iFolder", "../doc/user/data/bq1r10z.html", "zHTML xD.0000.0000.0001.0002.", "Vantaggi di iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Funzioni principali di iFolder", "../doc/user/data/bq1r110.html", "zHTML xD.0000.0000.0001.0003.", "Funzioni principali di iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Considerazioni sul supporto multipiattaforma", "../doc/user/data/bwcku86.html", "zHTML xD.0000.0000.0001.0004.", "Considerazioni sul supporto multipiattaforma");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_overview();", 1000);
   }

}

updateMenu_overview();
