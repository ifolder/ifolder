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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Uruchamianie klienta programu iFolder", "../doc/user/data/bw08fxi.html", "zHTML xD.0000.0000.0002.0001.", "Uruchamianie klienta programu iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konfigurowanie konta programu iFolder", "../doc/user/data/accounts.html", "zHTML xD.0000.0000.0002.0002.", "Konfigurowanie konta programu iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Logowanie do konta programu iFolder", "../doc/user/data/login.html", "zHTML xD.0000.0000.0002.0003.", "Logowanie do konta programu iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Przegl&#261;danie i modyfikowanie ustawie&#324; konta programu iFolder", "../doc/user/data/accountdetails.html", "zHTML xD.0000.0000.0002.0004.", "Przegl&#261;danie i modyfikowanie ustawie&#324; konta programu iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Usuwanie konta programu iFolder", "../doc/user/data/bvwsp7c.html", "zHTML xD.0000.0000.0002.0005.", "Usuwanie konta programu iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konfigurowanie preferencji iFolder&#243;w dla klienta", "../doc/user/data/preferences.html", "zHTML xD.0000.0000.0002.0006.", "Konfigurowanie preferencji iFolder&#243;w dla klienta");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konfigurowanie lokalnej zapory w celu umo&#380;liwienia komunikacji programu iFolder", "../doc/user/data/bvwppw5.html", "zHTML xD.0000.0000.0002.0007.", "Konfigurowanie lokalnej zapory w celu umo&#380;liwienia komunikacji programu iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Konfigurowanie lokalnego skanera antywirusowego w celu umo&#380;liwienia komunikacji programu iFolder", "../doc/user/data/bwbirb9.html", "zHTML xD.0000.0000.0002.0008.", "Konfigurowanie lokalnego skanera antywirusowego w celu umo&#380;liwienia komunikacji programu iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_accountsprefs();", 1000);
   }

}

updateMenu_accountsprefs();

