function updateMenu_accountsprefs() {

   if ((parent.theMenu) && (parent.theMenu.amBusy == false))
   {
      var level0ID = parent.theMenu.currentSubRoot;

      var level1ID;
      var level2ID;
      var level3ID;
     level2ID = parent.theMenu.findEntry('data/accountsprefs.html', 'url', 'right');
     if ((level2ID != -1) && (parent.theMenu.entry[level2ID].FirstChild < 0))
      {
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Spuštění klienta aplikace iFolder", "../doc/user/data/bw08fxi.html", "zHTML xD.0000.0000.0002.0001.", "Spuštění klienta aplikace iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konfigurace účtu aplikace iFolder", "../doc/user/data/accounts.html", "zHTML xD.0000.0000.0002.0002.", "Konfigurace účtu aplikace iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Přihlášení k účtu aplikace iFolder", "../doc/user/data/login.html", "zHTML xD.0000.0000.0002.0003.", "Přihlášení k účtu aplikace iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Zobrazení a změna nastavení účtu aplikace iFolder", "../doc/user/data/accountdetails.html", "zHTML xD.0000.0000.0002.0004.", "Zobrazení a změna nastavení účtu aplikace iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Odstranění účtu aplikace iFolder", "../doc/user/data/bvwsp7c.html", "zHTML xD.0000.0000.0002.0005.", "Odstranění účtu aplikace iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konfigurace předvoleb aplikace iFolder pro klienta", "../doc/user/data/preferences.html", "zHTML xD.0000.0000.0002.0006.", "Konfigurace předvoleb aplikace iFolder pro klienta");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konfigurace místní brány firewall pro provoz aplikace iFolder", "../doc/user/data/bvwppw5.html", "zHTML xD.0000.0000.0002.0007.", "Konfigurace místní brány firewall pro provoz aplikace iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konfigurace místního antivirového programu pro provoz aplikace iFolder", "../doc/user/data/bwbirb9.html", "zHTML xD.0000.0000.0002.0008.", "Konfigurace místního antivirového programu pro provoz aplikace iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_accountsprefs();", 1000);
   }

}

updateMenu_accountsprefs();
