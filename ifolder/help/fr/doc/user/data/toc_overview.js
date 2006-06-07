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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Partage de dossiers iFolder", "../doc/user/data/bx3aueg.html", "zHTML xD.0000.0000.0001.0001.", "Partage de dossiers iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Avantages de iFolder", "../doc/user/data/bq1r10z.html", "zHTML xD.0000.0000.0001.0002.", "Avantages de iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Fonctionnalités clés de iFolder", "../doc/user/data/bq1r110.html", "zHTML xD.0000.0000.0001.0003.", "Fonctionnalités clés de iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Remarques relatives aux différentes plates-formes", "../doc/user/data/bwcku86.html", "zHTML xD.0000.0000.0001.0004.", "Remarques relatives aux différentes plates-formes");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_overview();", 1000);
   }

}

updateMenu_overview();
