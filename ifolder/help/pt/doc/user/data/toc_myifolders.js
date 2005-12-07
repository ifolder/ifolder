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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Diretrizes para localização de pastas iFolder", "../doc/user/data/bx08x4y.html", "zHTML xD.0000.0000.0003.0001.", "Diretrizes para localização de pastas iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Diretrizes para tipos e tamanhos de arquivos não sincronizados", "../doc/user/data/bwensdb.html", "zHTML xD.0000.0000.0003.0002.", "Diretrizes para tipos e tamanhos de arquivos não sincronizados");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Convenções de nomeação para um iFolder e suas pastas e arquivos", "../doc/user/data/bwagrrn.html", "zHTML xD.0000.0000.0003.0003.", "Convenções de nomeação para um iFolder e suas pastas e arquivos");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Criando uma pasta iFolder", "../doc/user/data/createifolder.html", "zHTML xD.0000.0000.0003.0004.", "Criando uma pasta iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Compartilhando uma pasta do iFolder", "../doc/user/data/sharewith.html", "zHTML xD.0000.0000.0003.0005.", "Compartilhando uma pasta do iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configurando uma pasta iFolder disponível", "../doc/user/data/setupifolder.html", "zHTML xD.0000.0000.0003.0006.", "Configurando uma pasta iFolder disponível");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Visualizando e configurando propriedades de uma pasta iFolder", "../doc/user/data/propifolders.html", "zHTML xD.0000.0000.0003.0007.", "Visualizando e configurando propriedades de uma pasta iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Sincronizando arquivos", "../doc/user/data/sync.html", "zHTML xD.0000.0000.0003.0008.", "Sincronizando arquivos");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Resolvendo conflitos de arquivos", "../doc/user/data/conflicts.html", "zHTML xD.0000.0000.0003.0009.", "Resolvendo conflitos de arquivos");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Revertendo uma pasta iFolder para uma pasta normal", "../doc/user/data/reverting.html", "zHTML xD.0000.0000.0003.0010.", "Revertendo uma pasta iFolder para uma pasta normal");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Movendo uma pasta iFolder", "../doc/user/data/movelocation.html", "zHTML xD.0000.0000.0003.0011.", "Movendo uma pasta iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Apagando uma pasta iFolder", "../doc/user/data/deleting.html", "zHTML xD.0000.0000.0003.0012.", "Apagando uma pasta iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_myifolders();", 1000);
   }

}

updateMenu_myifolders();
