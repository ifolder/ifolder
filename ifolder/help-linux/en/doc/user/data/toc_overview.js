﻿function updateMenu_overview() {

   if ((parent.theMenu) && (parent.theMenu.amBusy == false))
   {
      var level0ID = parent.theMenu.currentSubRoot;

      var level1ID;
      var level2ID;
      var level3ID;
     level2ID = parent.theMenu.findEntry('data/overview.html', 'url', 'right');
     if ((level2ID != -1) && (parent.theMenu.entry[level2ID].FirstChild < 0))
      {
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolder Sharing", "../doc/user/data/ovwsharing.html", "zHTML xD.0000.0000.0001.0001.", "iFolder Sharing");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Benefits of iFolder", "../doc/user/data/ovwbenefits.html", "zHTML xD.0000.0000.0001.0002.", "Benefits of iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Key Features of iFolder", "../doc/user/data/ovwfeatures.html", "zHTML xD.0000.0000.0001.0003.", "Key Features of iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Cross-Platform Considerations", "../doc/user/data/ovwxplat.html", "zHTML xD.0000.0000.0001.0004.", "Cross-Platform Considerations");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "What’s Next", "../doc/user/data/ovwnext.html", "zHTML xD.0000.0000.0001.0005.", "What’s Next");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_overview();", 1000);
   }

}

updateMenu_overview();