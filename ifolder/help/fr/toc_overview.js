function updateMenu_overview() {
  if ((parent.theMenu) && (parent.theMenu.amBusy == false))
  {
    var level0ID = parent.theMenu.currentSubRoot;
    var level1ID;
    var level2ID;
    var level3ID;
    var level4ID;
    level2ID = parent.theMenu.findEntry('/overview.html', 'url', 'right');
    if ((level2ID != -1) && (parent.theMenu.entry[level2ID].FirstChild < 0))
    {
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Benefits of iFolder", "../benefits.html", "zHTML xD.0000.0000.0001.0001.", "Benefits of iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "The iFolder Client", "../bawqltp.html", "zHTML xD.0000.0000.0001.0002.", "The iFolder Client");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolder Account", "../bawqmpe.html", "zHTML xD.0000.0000.0001.0003.", "iFolder Account");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Cross-Platform Considerations", "../ovwxplat.html", "zHTML xD.0000.0000.0001.0004.", "Cross-Platform Considerations");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Key Features of iFolder", "../keyfeatures.html", "zHTML xD.0000.0000.0001.0005.", "Key Features of iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "What’s Next", "../ovwnext.html", "zHTML xD.0000.0000.0001.0006.", "What’s Next");
      parent.theMenu.reload();
    }
  } else {
     setTimeout("updateMenu_overview();", 100);
  }
}

updateMenu_overview();
