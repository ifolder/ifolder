function updateMenu_bq1qyz2() {

   if ((parent.theMenu) && (parent.theMenu.amBusy == false))
   {
      var level0ID = parent.theMenu.currentSubRoot;

      var level1ID;
      var level2ID;
      var level3ID;
     level2ID = parent.theMenu.findEntry('data/bq1qyz2.html', 'url', 'right');
     if ((level2ID != -1) && (parent.theMenu.entry[level2ID].FirstChild < 0))
      {
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Workgroup and Enterprise Server Sharing", "../doc/user/data/bv1g15h.html", "zHTML xD.0000.0000.0001.0001.", "Workgroup and Enterprise Server Sharing");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Benefits of iFolder", "../doc/user/data/bq1r10z.html", "zHTML xD.0000.0000.0001.0002.", "Benefits of iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Key Features of iFolder", "../doc/user/data/bq1r110.html", "zHTML xD.0000.0000.0001.0003.", "Key Features of iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "What's Next", "../doc/user/data/bq1r11b.html", "zHTML xD.0000.0000.0001.0004.", "What's Next");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_bq1qyz2();", 1000);
   }

}

updateMenu_bq1qyz2();
