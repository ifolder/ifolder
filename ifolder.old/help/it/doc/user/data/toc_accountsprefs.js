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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Avvio di una sessione del client iFolder", "../doc/user/data/bw08fxi.html", "zHTML xD.0000.0000.0002.0001.", "Avvio di una sessione del client iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configurazione di un conto iFolder", "../doc/user/data/accounts.html", "zHTML xD.0000.0000.0002.0002.", "Configurazione di un conto iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Login a un conto iFolder", "../doc/user/data/login.html", "zHTML xD.0000.0000.0002.0003.", "Login a un conto iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Visualizzazione e modifica delle impostazioni del conto iFolder", "../doc/user/data/accountdetails.html", "zHTML xD.0000.0000.0002.0004.", "Visualizzazione e modifica delle impostazioni del conto iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Cancellazione di un conto iFolder", "../doc/user/data/bvwsp7c.html", "zHTML xD.0000.0000.0002.0005.", "Cancellazione di un conto iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configurazione delle Preferenze di iFolder per il client", "../doc/user/data/preferences.html", "zHTML xD.0000.0000.0002.0006.", "Configurazione delle Preferenze di iFolder per il client");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configurazione delle impostazioni del firewall locale per il traffico iFolder", "../doc/user/data/bvwppw5.html", "zHTML xD.0000.0000.0002.0007.", "Configurazione delle impostazioni del firewall locale per il traffico iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configurazione delle impostazioni del software antivirus locale per il traffico iFolder", "../doc/user/data/bwbirb9.html", "zHTML xD.0000.0000.0002.0008.", "Configurazione delle impostazioni del software antivirus locale per il traffico iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_accountsprefs();", 1000);
   }

}

updateMenu_accountsprefs();
