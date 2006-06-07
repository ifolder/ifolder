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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Pravidla pro umístění složek iFolder", "../doc/user/data/bx08x4y.html", "zHTML xD.0000.0000.0003.0001.", "Pravidla pro umístění složek iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Pravidla pro typy a velikost souborů, které nemají být synchronizovány", "../doc/user/data/bwensdb.html", "zHTML xD.0000.0000.0003.0002.", "Pravidla pro typy a velikost souborů, které nemají být synchronizovány");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Zásady pojmenování pro složku iFolder a její složky a soubory", "../doc/user/data/bwagrrn.html", "zHTML xD.0000.0000.0003.0003.", "Zásady pojmenování pro složku iFolder a její složky a soubory");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Vytvoření složky iFolder", "../doc/user/data/createifolder.html", "zHTML xD.0000.0000.0003.0004.", "Vytvoření složky iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Sdílení složky iFolder", "../doc/user/data/sharewith.html", "zHTML xD.0000.0000.0003.0005.", "Sdílení složky iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Nastavení dostupné složky iFolder", "../doc/user/data/setupifolder.html", "zHTML xD.0000.0000.0003.0006.", "Nastavení dostupné složky iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Zobrazení a konfigurace vlastností složky iFolder", "../doc/user/data/propifolders.html", "zHTML xD.0000.0000.0003.0007.", "Zobrazení a konfigurace vlastností složky iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Synchronizace souborů", "../doc/user/data/sync.html", "zHTML xD.0000.0000.0003.0008.", "Synchronizace souborů");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Vyřešení konfliktů souborů", "../doc/user/data/conflicts.html", "zHTML xD.0000.0000.0003.0009.", "Vyřešení konfliktů souborů");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Změna složky iFolder na normální složku", "../doc/user/data/reverting.html", "zHTML xD.0000.0000.0003.0010.", "Změna složky iFolder na normální složku");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Přesunutí složky iFolder", "../doc/user/data/movelocation.html", "zHTML xD.0000.0000.0003.0011.", "Přesunutí složky iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Odstranění složky iFolder", "../doc/user/data/deleting.html", "zHTML xD.0000.0000.0003.0012.", "Odstranění složky iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_myifolders();", 1000);
   }

}

updateMenu_myifolders();
