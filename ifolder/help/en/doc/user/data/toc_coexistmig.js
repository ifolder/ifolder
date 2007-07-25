function updateMenu_coexistmig() {

   if ((parent.theMenu) && (parent.theMenu.amBusy == false))
   {
      var level0ID = parent.theMenu.currentSubRoot;

      var level1ID;
      var level2ID;
      var level3ID;
     level2ID = parent.theMenu.findEntry('data/coexistmig.html', 'url', 'right');
     if ((level2ID != -1) && (parent.theMenu.entry[level2ID].FirstChild < 0))
      {
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Migrating from iFolder 2.x to iFolder 3.6", "../doc/user/data/migration.html", "zHTML xD.0000.0000.0004.0001.", "Migrating from iFolder 2.x to iFolder 3.6");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Upgrading And Coexistence of iFolder Clients", "../doc/user/data/upgrading.html", "zHTML xD.0000.0000.0004.0002.", "Upgrading And Coexistence of iFolder Clients");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_coexistmig();", 1000);
   }

}

updateMenu_coexistmig();
