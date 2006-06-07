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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Démarrage du Client iFolder", "../doc/user/data/bw08fxi.html", "zHTML xD.0000.0000.0002.0001.", "Démarrage du Client iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuration d'un compte iFolder", "../doc/user/data/accounts.html", "zHTML xD.0000.0000.0002.0002.", "Configuration d'un compte iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Connexion à un compte iFolder", "../doc/user/data/login.html", "zHTML xD.0000.0000.0002.0003.", "Connexion à un compte iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Affichage et modification des paramètres de compte iFolder", "../doc/user/data/accountdetails.html", "zHTML xD.0000.0000.0002.0004.", "Affichage et modification des paramètres de compte iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Suppression d'un compte iFolder", "../doc/user/data/bvwsp7c.html", "zHTML xD.0000.0000.0002.0005.", "Suppression d'un compte iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuration des préférences iFolder du Client", "../doc/user/data/preferences.html", "zHTML xD.0000.0000.0002.0006.", "Configuration des préférences iFolder du Client");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuration des paramètres du pare-feu local pour le trafic iFolder", "../doc/user/data/bvwppw5.html", "zHTML xD.0000.0000.0002.0007.", "Configuration des paramètres du pare-feu local pour le trafic iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuration des paramètres du scanner de virus local pour le trafic iFolder", "../doc/user/data/bwbirb9.html", "zHTML xD.0000.0000.0002.0008.", "Configuration des paramètres du scanner de virus local pour le trafic iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_accountsprefs();", 1000);
   }

}

updateMenu_accountsprefs();
