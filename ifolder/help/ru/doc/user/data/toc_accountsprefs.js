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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Запуск клиента iFolder", "../doc/user/data/bw08fxi.html", "zHTML xD.0000.0000.0002.0001.", "Запуск клиента iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Конфигурирование учетной записи iFolder", "../doc/user/data/accounts.html", "zHTML xD.0000.0000.0002.0002.", "Конфигурирование учетной записи iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Регистрация с помощью учетной записи iFolder", "../doc/user/data/login.html", "zHTML xD.0000.0000.0002.0003.", "Регистрация с помощью учетной записи iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Просмотр и изменение настроек учетной записи iFolder", "../doc/user/data/accountdetails.html", "zHTML xD.0000.0000.0002.0004.", "Просмотр и изменение настроек учетной записи iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Удаление учетной записи iFolder", "../doc/user/data/bvwsp7c.html", "zHTML xD.0000.0000.0002.0005.", "Удаление учетной записи iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Конфигурирование настроек iFolder для клиента", "../doc/user/data/preferences.html", "zHTML xD.0000.0000.0002.0006.", "Конфигурирование настроек iFolder для клиента");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Конфигурирование параметров локального брандмауэра для трафика iFolder", "../doc/user/data/bvwppw5.html", "zHTML xD.0000.0000.0002.0007.", "Конфигурирование параметров локального брандмауэра для трафика iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Конфигурирование параметров локальной антивирусной программы для трафика iFolder", "../doc/user/data/bwbirb9.html", "zHTML xD.0000.0000.0002.0008.", "Конфигурирование параметров локальной антивирусной программы для трафика iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_accountsprefs();", 1000);
   }

}

updateMenu_accountsprefs();
