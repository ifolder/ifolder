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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Starten des iFolder-Client", "../doc/user/data/bw08fxi.html", "zHTML xD.0000.0000.0002.0001.", "Starten des iFolder-Client");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konfigurieren von iFolder-Konten", "../doc/user/data/accounts.html", "zHTML xD.0000.0000.0002.0002.", "Konfigurieren von iFolder-Konten");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Anmelden bei iFolder-Konten", "../doc/user/data/login.html", "zHTML xD.0000.0000.0002.0003.", "Anmelden bei iFolder-Konten");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Anzeigen und Bearbeiten von iFolder-Konto-Einstellungen", "../doc/user/data/accountdetails.html", "zHTML xD.0000.0000.0002.0004.", "Anzeigen und Bearbeiten von iFolder-Konto-Einstellungen");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Löschen von iFolder-Konten", "../doc/user/data/bvwsp7c.html", "zHTML xD.0000.0000.0002.0005.", "Löschen von iFolder-Konten");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konfigurieren von iFolder-Einstellungen für den Client", "../doc/user/data/preferences.html", "zHTML xD.0000.0000.0002.0006.", "Konfigurieren von iFolder-Einstellungen für den Client");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konfigurieren von lokalen Firewall-Einstellungen für den iFolder-Verkehr", "../doc/user/data/bvwppw5.html", "zHTML xD.0000.0000.0002.0007.", "Konfigurieren von lokalen Firewall-Einstellungen für den iFolder-Verkehr");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konfigurieren von lokalen Virenscanner-Einstellungen für den iFolder-Verkehr", "../doc/user/data/bwbirb9.html", "zHTML xD.0000.0000.0002.0008.", "Konfigurieren von lokalen Virenscanner-Einstellungen für den iFolder-Verkehr");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_accountsprefs();", 1000);
   }

}

updateMenu_accountsprefs();
