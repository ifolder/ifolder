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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Indicazioni per l'ubicazione delle cartelle iFolder", "../doc/user/data/bx08x4y.html", "zHTML xD.0000.0000.0003.0001.", "Indicazioni per l'ubicazione delle cartelle iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Indicazioni relative a tipi e dimensioni dei file da non sincronizzare", "../doc/user/data/bwensdb.html", "zHTML xD.0000.0000.0003.0002.", "Indicazioni relative a tipi e dimensioni dei file da non sincronizzare");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Convenzioni di denominazione per le cartelle iFolder e i relativi file e cartelle", "../doc/user/data/bwagrrn.html", "zHTML xD.0000.0000.0003.0003.", "Convenzioni di denominazione per le cartelle iFolder e i relativi file e cartelle");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Creazione di una cartella iFolder", "../doc/user/data/createifolder.html", "zHTML xD.0000.0000.0003.0004.", "Creazione di una cartella iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Condivisione di una cartella iFolder", "../doc/user/data/sharewith.html", "zHTML xD.0000.0000.0003.0005.", "Condivisione di una cartella iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configurazione di una cartella iFolder disponibile", "../doc/user/data/setupifolder.html", "zHTML xD.0000.0000.0003.0006.", "Configurazione di una cartella iFolder disponibile");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Visualizzazione e configurazione delle proprietà di una cartella iFolder", "../doc/user/data/propifolders.html", "zHTML xD.0000.0000.0003.0007.", "Visualizzazione e configurazione delle proprietà di una cartella iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Sincronizzazione dei file", "../doc/user/data/sync.html", "zHTML xD.0000.0000.0003.0008.", "Sincronizzazione dei file");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Risoluzione dei conflitti di file", "../doc/user/data/conflicts.html", "zHTML xD.0000.0000.0003.0009.", "Risoluzione dei conflitti di file");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Annullamento della conversione di una cartella normale in cartella iFolder", "../doc/user/data/reverting.html", "zHTML xD.0000.0000.0003.0010.", "Annullamento della conversione di una cartella normale in cartella iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Spostamento di una cartella iFolder", "../doc/user/data/movelocation.html", "zHTML xD.0000.0000.0003.0011.", "Spostamento di una cartella iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Cancellazione di una cartella iFolder", "../doc/user/data/deleting.html", "zHTML xD.0000.0000.0003.0012.", "Cancellazione di una cartella iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_myifolders();", 1000);
   }

}

updateMenu_myifolders();
