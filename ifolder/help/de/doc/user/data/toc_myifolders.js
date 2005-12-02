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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Richtlinien für das Suchen nach iFolder-Ordnern", "../doc/user/data/bx08x4y.html", "zHTML xD.0000.0000.0003.0001.", "Richtlinien für das Suchen nach iFolder-Ordnern");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Richtlinien für nicht zu synchronisierende Dateitypen und -größen", "../doc/user/data/bwensdb.html", "zHTML xD.0000.0000.0003.0002.", "Richtlinien für nicht zu synchronisierende Dateitypen und -größen");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Benennungskonventionen für iFolder-Ordner und zugehörige Ordner und Dateien", "../doc/user/data/bwagrrn.html", "zHTML xD.0000.0000.0003.0003.", "Benennungskonventionen für iFolder-Ordner und zugehörige Ordner und Dateien");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Erstellen von iFolder-Ordnern", "../doc/user/data/createifolder.html", "zHTML xD.0000.0000.0003.0004.", "Erstellen von iFolder-Ordnern");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Freigeben von iFolder-Ordnern", "../doc/user/data/sharewith.html", "zHTML xD.0000.0000.0003.0005.", "Freigeben von iFolder-Ordnern");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Einrichten verfügbarer iFolder-Ordner", "../doc/user/data/setupifolder.html", "zHTML xD.0000.0000.0003.0006.", "Einrichten verfügbarer iFolder-Ordner");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Anzeigen und Konfigurieren der Eigenschaften von iFolder-Ordnern", "../doc/user/data/propifolders.html", "zHTML xD.0000.0000.0003.0007.", "Anzeigen und Konfigurieren der Eigenschaften von iFolder-Ordnern");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Synchronisieren von Dateien", "../doc/user/data/sync.html", "zHTML xD.0000.0000.0003.0008.", "Synchronisieren von Dateien");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Auflösen von Dateikonflikten", "../doc/user/data/conflicts.html", "zHTML xD.0000.0000.0003.0009.", "Auflösen von Dateikonflikten");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Zurücksetzen von iFolder-Ordnern auf normale Ordner", "../doc/user/data/reverting.html", "zHTML xD.0000.0000.0003.0010.", "Zurücksetzen von iFolder-Ordnern auf normale Ordner");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Verschieben von iFolder-Ordnern", "../doc/user/data/movelocation.html", "zHTML xD.0000.0000.0003.0011.", "Verschieben von iFolder-Ordnern");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Löschen von iFolder-Ordnern", "../doc/user/data/deleting.html", "zHTML xD.0000.0000.0003.0012.", "Löschen von iFolder-Ordnern");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_myifolders();", 1000);
   }

}

updateMenu_myifolders();
