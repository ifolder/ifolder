function updateMenu_overview() {

   if ((parent.theMenu) && (parent.theMenu.amBusy == false))
   {
      var level0ID = parent.theMenu.currentSubRoot;

      var level1ID;
      var level2ID;
      var level3ID;
     level2ID = parent.theMenu.findEntry('data/overview.html', 'url', 'right');
     if ((level2ID != -1) && (parent.theMenu.entry[level2ID].FirstChild < 0))
      {
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Wsp&#243;&#322;u&#380;ytkowanie iFolder&#243;w", "../doc/user/data/bx3aueg.html", "zHTML xD.0000.0000.0001.0001.", "Wsp&#243;&#322;u&#380;ytkowanie iFolder&#243;w");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Zalety programu iFolder", "../doc/user/data/bq1r10z.html", "zHTML xD.0000.0000.0001.0002.", "Zalety programu iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Podstawowe funkcje programu iFolder", "../doc/user/data/bq1r110.html", "zHTML xD.0000.0000.0001.0003.", "Podstawowe funkcje programu iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Kwestie dotycz&#261;ce wykorzystania r&#243;&#380;nych platform", "../doc/user/data/bwcku86.html", "zHTML xD.0000.0000.0001.0004.", "Kwestie dotycz&#261;ce wykorzystania r&#243;&#380;nych platform");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_overview();", 1000);
   }

}

updateMenu_overview();

