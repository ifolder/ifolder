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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Wytyczne dotycz&#261;ce rozmieszczania iFolder&#243;w", "../doc/user/data/bx08x4y.html", "zHTML xD.0000.0000.0003.0001.", "Wytyczne dotycz&#261;ce rozmieszczania iFolder&#243;w");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Wytyczne dotycz&#261;ce plik&#243;w okre&#347;lonego typu lub rozmiaru, kt&#243;re nie powinny by&#263; synchronizowane", "../doc/user/data/bwensdb.html", "zHTML xD.0000.0000.0003.0002.", "Wytyczne dotycz&#261;ce plik&#243;w okre&#347;lonego typu lub rozmiaru, kt&#243;re nie powinny by&#263; synchronizowane");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konwencje nazewnictwa iFolder&#243;w oraz zawartych w nich folder&#243;w i plik&#243;w", "../doc/user/data/bwagrrn.html", "zHTML xD.0000.0000.0003.0003.", "Konwencje nazewnictwa iFolder&#243;w oraz zawartych w nich folder&#243;w i plik&#243;w");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Tworzenie iFolderu", "../doc/user/data/createifolder.html", "zHTML xD.0000.0000.0003.0004.", "Tworzenie iFolderu");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Wsp&#243;&#322;u&#380;ytkowanie iFolderu", "../doc/user/data/sharewith.html", "zHTML xD.0000.0000.0003.0005.", "Wsp&#243;&#322;u&#380;ytkowanie iFolderu");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konfigurowanie dost&#281;pnego iFolderu", "../doc/user/data/setupifolder.html", "zHTML xD.0000.0000.0003.0006.", "Konfigurowanie dost&#281;pnego iFolderu");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Wy&#347;wietlanie i konfigurowanie w&#322;a&#347;ciwo&#347;ci iFolderu", "../doc/user/data/propifolders.html", "zHTML xD.0000.0000.0003.0007.", "Wy&#347;wietlanie i konfigurowanie w&#322;a&#347;ciwo&#347;ci iFolderu");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Synchronizowanie plik&#243;w", "../doc/user/data/sync.html", "zHTML xD.0000.0000.0003.0008.", "Synchronizowanie plik&#243;w");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Rozwi&#261;zywanie konflikt&#243;w plik&#243;w", "../doc/user/data/conflicts.html", "zHTML xD.0000.0000.0003.0009.", "Rozwi&#261;zywanie konflikt&#243;w plik&#243;w");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Przywracanie iFolderu do postaci zwyk&#322;ego folderu", "../doc/user/data/reverting.html", "zHTML xD.0000.0000.0003.0010.", "Przywracanie iFolderu do postaci zwyk&#322;ego folderu");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Przenoszenie iFolderu", "../doc/user/data/movelocation.html", "zHTML xD.0000.0000.0003.0011.", "Przenoszenie iFolderu");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Usuwanie iFolderu", "../doc/user/data/deleting.html", "zHTML xD.0000.0000.0003.0012.", "Usuwanie iFolderu");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_myifolders();", 1000);
   }

}

updateMenu_myifolders();

