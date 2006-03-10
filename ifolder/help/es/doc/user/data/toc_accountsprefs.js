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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Inicio del Cliente iFolder", "../doc/user/data/bw08fxi.html", "zHTML xD.0000.0000.0002.0001.", "Inicio del Cliente iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuración de una cuenta de iFolder", "../doc/user/data/accounts.html", "zHTML xD.0000.0000.0002.0002.", "Configuración de una cuenta de iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Entrada a una cuenta de iFolder", "../doc/user/data/login.html", "zHTML xD.0000.0000.0002.0003.", "Entrada a una cuenta de iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Visualización y modificación los ajustes de las cuentas de iFolder", "../doc/user/data/accountdetails.html", "zHTML xD.0000.0000.0002.0004.", "Visualización y modificación los ajustes de las cuentas de iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Supresión de una cuenta de iFolder", "../doc/user/data/bvwsp7c.html", "zHTML xD.0000.0000.0002.0005.", "Supresión de una cuenta de iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuración de las preferencias de iFolder del cliente", "../doc/user/data/preferences.html", "zHTML xD.0000.0000.0002.0006.", "Configuración de las preferencias de iFolder del cliente");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuración de los ajustes del cortafuegos local para el tráfico de iFolder", "../doc/user/data/bvwppw5.html", "zHTML xD.0000.0000.0002.0007.", "Configuración de los ajustes del cortafuegos local para el tráfico de iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuración de los ajustes del navegador de virus local para el tráfico de iFolder", "../doc/user/data/bwbirb9.html", "zHTML xD.0000.0000.0002.0008.", "Configuración de los ajustes del navegador de virus local para el tráfico de iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_accountsprefs();", 1000);
   }

}

updateMenu_accountsprefs();


