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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Рекомендации по расположению папок iFolder", "../doc/user/data/bx08x4y.html", "zHTML xD.0000.0000.0003.0001.", "Рекомендации по расположению папок iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Рекомендации по типам и размерам не синхронизируемых файлов", "../doc/user/data/bwensdb.html", "zHTML xD.0000.0000.0003.0002.", "Рекомендации по типам и размерам не синхронизируемых файлов");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Соглашения об именовании папки iFolder, содержащихся в ней папок и файлов", "../doc/user/data/bwagrrn.html", "zHTML xD.0000.0000.0003.0003.", "Соглашения об именовании папки iFolder, содержащихся в ней папок и файлов");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Создание папки iFolder", "../doc/user/data/createifolder.html", "zHTML xD.0000.0000.0003.0004.", "Создание папки iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Совместное использование папки iFolder", "../doc/user/data/sharewith.html", "zHTML xD.0000.0000.0003.0005.", "Совместное использование папки iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Настройка доступной папки iFolder", "../doc/user/data/setupifolder.html", "zHTML xD.0000.0000.0003.0006.", "Настройка доступной папки iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Просмотр и конфигурирование свойств папки iFolder", "../doc/user/data/propifolders.html", "zHTML xD.0000.0000.0003.0007.", "Просмотр и конфигурирование свойств папки iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Синхронизация файлов", "../doc/user/data/sync.html", "zHTML xD.0000.0000.0003.0008.", "Синхронизация файлов");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Разрешение конфликтов файлов", "../doc/user/data/conflicts.html", "zHTML xD.0000.0000.0003.0009.", "Разрешение конфликтов файлов");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Преобразование папки iFolder в стандартную папку", "../doc/user/data/reverting.html", "zHTML xD.0000.0000.0003.0010.", "Преобразование папки iFolder в стандартную папку");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Перемещение папки iFolder", "../doc/user/data/movelocation.html", "zHTML xD.0000.0000.0003.0011.", "Перемещение папки iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Удаление папки iFolder", "../doc/user/data/deleting.html", "zHTML xD.0000.0000.0003.0012.", "Удаление папки iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_myifolders();", 1000);
   }

}

updateMenu_myifolders();
