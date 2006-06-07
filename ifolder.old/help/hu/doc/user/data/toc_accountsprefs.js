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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Az iFolder ügyfél elindítása", "../doc/user/data/bw08fxi.html", "zHTML xD.0000.0000.0002.0001.", "Az iFolder ügyfél elindítása");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolder-fiók konfigurálása", "../doc/user/data/accounts.html", "zHTML xD.0000.0000.0002.0002.", "iFolder-fiók konfigurálása");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Bejelentkezés egy iFolder-fiókba", "../doc/user/data/login.html", "zHTML xD.0000.0000.0002.0003.", "Bejelentkezés egy iFolder-fiókba");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Az iFolder fiókbeállítások megtekintése és módosítása", "../doc/user/data/accountdetails.html", "zHTML xD.0000.0000.0002.0004.", "Az iFolder fiókbeállítások megtekintése és módosítása");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolder-fiók törlése", "../doc/user/data/bvwsp7c.html", "zHTML xD.0000.0000.0002.0005.", "iFolder-fiók törlése");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Az ügyfél iFolder-beállításainak megadása", "../doc/user/data/preferences.html", "zHTML xD.0000.0000.0002.0006.", "Az ügyfél iFolder-beállításainak megadása");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "A helyi tűzfal beállítása az iFolder-forgalomhoz", "../doc/user/data/bvwppw5.html", "zHTML xD.0000.0000.0002.0007.", "A helyi tűzfal beállítása az iFolder-forgalomhoz");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "A helyi vírusellenőrző beállítása az iFolder-forgalomhoz", "../doc/user/data/bwbirb9.html", "zHTML xD.0000.0000.0002.0008.", "A helyi vírusellenőrző beállítása az iFolder-forgalomhoz");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_accountsprefs();", 1000);
   }

}

updateMenu_accountsprefs();
