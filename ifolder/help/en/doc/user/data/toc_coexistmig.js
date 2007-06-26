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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Coexistence of Novell iFolder 2.1x and iFolder Clients", "../doc/user/data/coexistence.html", "zHTML xD.0000.0000.0002.0001.", "Coexistence of Novell iFolder 2.1x and iFolder Clients");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Migrating from iFolder 2.1x to iFolder 3.6", "../doc/user/data/migration.html", "zHTML xD.0000.0000.0002.0002.", "Migrating from iFolder 2.1x to iFolder 3.6");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Upgrading iFolder 3.4 client to iFolder 3.6", "../doc/user/data/upgrading.html", "zHTML xD.0000.0000.0002.0003.", "Upgrading iFolder 3.4 client to iFolder 3.6");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_coexistmig();", 1000);
   }

}

updateMenu_coexistmig();
