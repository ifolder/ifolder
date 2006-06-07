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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Pokyny pre umiestnenie priečinkov iFolder", "../doc/user/data/bx08x4y.html", "zHTML xD.0000.0000.0003.0001.", "Pokyny pre umiestnenie priečinkov iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Pokyny pre typy a veľkosti súborov, ktoré sa nemajú synchronizovať", "../doc/user/data/bwensdb.html", "zHTML xD.0000.0000.0003.0002.", "Pokyny pre typy a veľkosti súborov, ktoré sa nemajú synchronizovať");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Pravidlá pomenovania priečinka iFolder a jeho priečinkov a súborov", "../doc/user/data/bwagrrn.html", "zHTML xD.0000.0000.0003.0003.", "Pravidlá pomenovania priečinka iFolder a jeho priečinkov a súborov");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Vytvorenie priečinka iFolder", "../doc/user/data/createifolder.html", "zHTML xD.0000.0000.0003.0004.", "Vytvorenie priečinka iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Zdieľanie priečinka iFolder", "../doc/user/data/sharewith.html", "zHTML xD.0000.0000.0003.0005.", "Zdieľanie priečinka iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Nastavenie dostupného priečinka iFolder", "../doc/user/data/setupifolder.html", "zHTML xD.0000.0000.0003.0006.", "Nastavenie dostupného priečinka iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Zobrazenie a konfigurácia vlastností priečinka iFolder", "../doc/user/data/propifolders.html", "zHTML xD.0000.0000.0003.0007.", "Zobrazenie a konfigurácia vlastností priečinka iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Synchronizácia súborov", "../doc/user/data/sync.html", "zHTML xD.0000.0000.0003.0008.", "Synchronizácia súborov");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Riešenie konfliktov súborov", "../doc/user/data/conflicts.html", "zHTML xD.0000.0000.0003.0009.", "Riešenie konfliktov súborov");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Zmena priečinka iFolder späť na normálny priečinok", "../doc/user/data/reverting.html", "zHTML xD.0000.0000.0003.0010.", "Zmena priečinka iFolder späť na normálny priečinok");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Premiestnenie priečinka iFolder", "../doc/user/data/movelocation.html", "zHTML xD.0000.0000.0003.0011.", "Premiestnenie priečinka iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Odstránenie priečinka iFolder", "../doc/user/data/deleting.html", "zHTML xD.0000.0000.0003.0012.", "Odstránenie priečinka iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_myifolders();", 1000);
   }

}

updateMenu_myifolders();
