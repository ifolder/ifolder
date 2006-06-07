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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolder-Freigabe", "../doc/user/data/bx3aueg.html", "zHTML xD.0000.0000.0001.0001.", "iFolder-Freigabe");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Vorteile von iFolder", "../doc/user/data/bq1r10z.html", "zHTML xD.0000.0000.0001.0002.", "Vorteile von iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Wichtigste Funktionen von iFolder", "../doc/user/data/bq1r110.html", "zHTML xD.0000.0000.0001.0003.", "Wichtigste Funktionen von iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Plattformübergreifende Überlegungen", "../doc/user/data/bwcku86.html", "zHTML xD.0000.0000.0001.0004.", "Plattformübergreifende Überlegungen");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_overview();", 1000);
   }

}

updateMenu_overview();
