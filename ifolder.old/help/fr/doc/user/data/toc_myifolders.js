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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Directives pour la localisation des dossiers iFolder", "../doc/user/data/bx08x4y.html", "zHTML xD.0000.0000.0003.0001.", "Directives pour la localisation des dossiers iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Directives sur les types et les tailles de fichier à ne pas synchroniser", "../doc/user/data/bwensdb.html", "zHTML xD.0000.0000.0003.0002.", "Directives sur les types et les tailles de fichier à ne pas synchroniser");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Conventions de dénomination des fichiers et des dossiers d'un compte iFolder", "../doc/user/data/bwagrrn.html", "zHTML xD.0000.0000.0003.0003.", "Conventions de dénomination des fichiers et des dossiers d'un compte iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Création d'un dossier iFolder", "../doc/user/data/createifolder.html", "zHTML xD.0000.0000.0003.0004.", "Création d'un dossier iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Partage d'un dossier iFolder", "../doc/user/data/sharewith.html", "zHTML xD.0000.0000.0003.0005.", "Partage d'un dossier iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuration d'un dossier iFolder disponible", "../doc/user/data/setupifolder.html", "zHTML xD.0000.0000.0003.0006.", "Configuration d'un dossier iFolder disponible");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Affichage et configuration des propriétés d'un dossier iFolder", "../doc/user/data/propifolders.html", "zHTML xD.0000.0000.0003.0007.", "Affichage et configuration des propriétés d'un dossier iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Synchronisation des fichiers", "../doc/user/data/sync.html", "zHTML xD.0000.0000.0003.0008.", "Synchronisation des fichiers");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Résolution des conflits de fichiers", "../doc/user/data/conflicts.html", "zHTML xD.0000.0000.0003.0009.", "Résolution des conflits de fichiers");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Conversion d'un dossier iFolder en dossier normal", "../doc/user/data/reverting.html", "zHTML xD.0000.0000.0003.0010.", "Conversion d'un dossier iFolder en dossier normal");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Déplacement d'un dossier iFolder", "../doc/user/data/movelocation.html", "zHTML xD.0000.0000.0003.0011.", "Déplacement d'un dossier iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Suppression d'un dossier iFolder", "../doc/user/data/deleting.html", "zHTML xD.0000.0000.0003.0012.", "Suppression d'un dossier iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_myifolders();", 1000);
   }

}

updateMenu_myifolders();
