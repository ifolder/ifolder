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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Directrices para ubicar carpetas iFolder", "../doc/user/data/bx08x4y.html", "zHTML xD.0000.0000.0003.0001.", "Directrices para ubicar carpetas iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Directrices para tipos y tamaños de archivo que no deben sincronizarse", "../doc/user/data/bwensdb.html", "zHTML xD.0000.0000.0003.0002.", "Directrices para tipos y tamaños de archivo que no deben sincronizarse");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Convenciones de denominación de una carpeta iFolder y de sus carpetas y archivos", "../doc/user/data/bwagrrn.html", "zHTML xD.0000.0000.0003.0003.", "Convenciones de denominación de una carpeta iFolder y de sus carpetas y archivos");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Creación de una carpeta iFolder", "../doc/user/data/createifolder.html", "zHTML xD.0000.0000.0003.0004.", "Creación de una carpeta iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Compartimiento de una carpeta iFolder", "../doc/user/data/sharewith.html", "zHTML xD.0000.0000.0003.0005.", "Compartimiento de una carpeta iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuración de una carpeta iFolder disponible", "../doc/user/data/setupifolder.html", "zHTML xD.0000.0000.0003.0006.", "Configuración de una carpeta iFolder disponible");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Visualización y configuración de las propiedades de una carpeta iFolder", "../doc/user/data/propifolders.html", "zHTML xD.0000.0000.0003.0007.", "Visualización y configuración de las propiedades de una carpeta iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Sincronización de archivos", "../doc/user/data/sync.html", "zHTML xD.0000.0000.0003.0008.", "Sincronización de archivos");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Resolución de conflictos con los archivos", "../doc/user/data/conflicts.html", "zHTML xD.0000.0000.0003.0009.", "Resolución de conflictos con los archivos");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Volver a colocar una carpeta iFolder en una carpeta normal", "../doc/user/data/reverting.html", "zHTML xD.0000.0000.0003.0010.", "Volver a colocar una carpeta iFolder en una carpeta normal");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Movimiento de una carpeta iFolder", "../doc/user/data/movelocation.html", "zHTML xD.0000.0000.0003.0011.", "Movimiento de una carpeta iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Supresión de una carpeta iFolder", "../doc/user/data/deleting.html", "zHTML xD.0000.0000.0003.0012.", "Supresión de una carpeta iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_myifolders();", 1000);
   }

}

updateMenu_myifolders();


