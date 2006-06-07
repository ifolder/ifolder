function updateMenu_myifolders() {

   if ((parent.theMenu) && (parent.theMenu.amBusy == false))
   {
      var level0ID = parent.theMenu.currentSubRoot;

      var level1ID;
      var level2ID;
      var level3ID;
     level2ID = parent.theMenu.findEntry('data/myifolders.html', 'url', 'right');
     if ((level2ID != -1) && (parent.theMenu.entry[level2ID].FirstChild < 0))
      {
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Irányelvek az iFolder mappák elhelyezéséhez", "../doc/user/data/bx08x4y.html", "zHTML xD.0000.0000.0003.0001.", "Irányelvek az iFolder mappák elhelyezéséhez");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Irányelvek – nem szinkronizálandó fájlméretek és fájltípusok", "../doc/user/data/bwensdb.html", "zHTML xD.0000.0000.0003.0002.", "Irányelvek – nem szinkronizálandó fájlméretek és fájltípusok");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Az iFolder mappákra, és a bennük található mappákra és fájlokra vonatkozó elnevezési konvenciók", "../doc/user/data/bwagrrn.html", "zHTML xD.0000.0000.0003.0003.", "Az iFolder mappákra, és a bennük található mappákra és fájlokra vonatkozó elnevezési konvenciók");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolder létrehozása", "../doc/user/data/createifolder.html", "zHTML xD.0000.0000.0003.0004.", "iFolder létrehozása");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolder megosztása", "../doc/user/data/sharewith.html", "zHTML xD.0000.0000.0003.0005.", "iFolder megosztása");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Elérhető iFolder mappa létrehozása", "../doc/user/data/setupifolder.html", "zHTML xD.0000.0000.0003.0006.", "Elérhető iFolder mappa létrehozása");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolder tulajdonságainak megtekintése és beállítása", "../doc/user/data/propifolders.html", "zHTML xD.0000.0000.0003.0007.", "iFolder tulajdonságainak megtekintése és beállítása");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolder mappák szinkronizálása", "../doc/user/data/sync.html", "zHTML xD.0000.0000.0003.0008.", "iFolder mappák szinkronizálása");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Fájlütközések feloldása", "../doc/user/data/conflicts.html", "zHTML xD.0000.0000.0003.0009.", "Fájlütközések feloldása");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolder visszaalakítása normál mappává", "../doc/user/data/reverting.html", "zHTML xD.0000.0000.0003.0010.", "iFolder visszaalakítása normál mappává");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolder áthelyezése", "../doc/user/data/movelocation.html", "zHTML xD.0000.0000.0003.0011.", "iFolder áthelyezése");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolder törlése", "../doc/user/data/deleting.html", "zHTML xD.0000.0000.0003.0012.", "iFolder törlése");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_myifolders();", 1000);
   }

}

updateMenu_myifolders();
