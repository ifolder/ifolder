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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Spustenie klienta aplikácie iFolder", "../doc/user/data/bw08fxi.html", "zHTML xD.0000.0000.0002.0001.", "Spustenie klienta aplikácie iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konfigurácia konta aplikácie iFolder", "../doc/user/data/accounts.html", "zHTML xD.0000.0000.0002.0002.", "Konfigurácia konta aplikácie iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Prihlásenie sa na konto aplikácie iFolder", "../doc/user/data/login.html", "zHTML xD.0000.0000.0002.0003.", "Prihlásenie sa na konto aplikácie iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Zobrazenie a zmena nastavenia konta aplikácie iFolder", "../doc/user/data/accountdetails.html", "zHTML xD.0000.0000.0002.0004.", "Zobrazenie a zmena nastavenia konta aplikácie iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Odstránenie konta aplikácie iFolder", "../doc/user/data/bvwsp7c.html", "zHTML xD.0000.0000.0002.0005.", "Odstránenie konta aplikácie iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konfigurácia preferencií aplikácie iFolder pre klienta", "../doc/user/data/preferences.html", "zHTML xD.0000.0000.0002.0006.", "Konfigurácia preferencií aplikácie iFolder pre klienta");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konfigurácia lokálnych nastavení brány firewall pre prevádzku aplikácie iFolder", "../doc/user/data/bvwppw5.html", "zHTML xD.0000.0000.0002.0007.", "Konfigurácia lokálnych nastavení brány firewall pre prevádzku aplikácie iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konfigurácia lokálnych nastavení zisťovania vírusov pre prevádzku aplikácie iFolder", "../doc/user/data/bwbirb9.html", "zHTML xD.0000.0000.0002.0008.", "Konfigurácia lokálnych nastavení zisťovania vírusov pre prevádzku aplikácie iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_accountsprefs();", 1000);
   }

}

updateMenu_accountsprefs();
