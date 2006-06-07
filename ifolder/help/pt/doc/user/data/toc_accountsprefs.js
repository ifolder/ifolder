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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Iniciando o Cliente iFolder", "../doc/user/data/bw08fxi.html", "zHTML xD.0000.0000.0002.0001.", "Iniciando o Cliente iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configurando uma conta do iFolder", "../doc/user/data/accounts.html", "zHTML xD.0000.0000.0002.0002.", "Configurando uma conta do iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Efetuando login em uma conta do iFolder", "../doc/user/data/login.html", "zHTML xD.0000.0000.0002.0003.", "Efetuando login em uma conta do iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Visualizando e modificando configurações da conta do iFolder", "../doc/user/data/accountdetails.html", "zHTML xD.0000.0000.0002.0004.", "Visualizando e modificando configurações da conta do iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Apagando uma conta do iFolder", "../doc/user/data/bvwsp7c.html", "zHTML xD.0000.0000.0002.0005.", "Apagando uma conta do iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configurando preferências do iFolder para o cliente", "../doc/user/data/preferences.html", "zHTML xD.0000.0000.0002.0006.", "Configurando preferências do iFolder para o cliente");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Definindo configurações de firewall local para o tráfego do iFolder", "../doc/user/data/bvwppw5.html", "zHTML xD.0000.0000.0002.0007.", "Definindo configurações de firewall local para o tráfego do iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Definindo configurações do programa de varredura de vírus local para o tráfego do iFolder", "../doc/user/data/bwbirb9.html", "zHTML xD.0000.0000.0002.0008.", "Definindo configurações do programa de varredura de vírus local para o tráfego do iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_accountsprefs();", 1000);
   }

}

updateMenu_accountsprefs();
