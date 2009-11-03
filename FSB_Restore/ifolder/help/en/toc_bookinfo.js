function updateMenu_bookinfo() {
  if ((parent.theMenu) && (parent.theMenu.amBusy == false))
  {
    var level0ID = parent.theMenu.currentSubRoot;
    var level1ID;
    var level2ID;
    var level3ID;
    var level4ID;
    level1ID = parent.theMenu.findEntry('/bookinfo.html', 'url', 'right');
    if (level1ID == -1)
    {
      level1ID = parent.theMenu.addChild(level0ID, "", "OES 2 SP1: Novell iFolder 3.7 Cross-Platform Help", "../bookinfo.html", "zFLDR xB.0000.0000.", "OES 2 SP1: Novell iFolder 3.7 Cross-Platform Help");
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "Overview of iFolder", "../overview.html", "zFLDR xC.0000.0000.0001.", "Overview of iFolder");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../overview.html","../toc_overview.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "Managing iFolder Accounts and Preferences", "../accountsprefs.html", "zFLDR xC.0000.0000.0002.", "Managing iFolder Accounts and Preferences");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../accountsprefs.html","../toc_accountsprefs.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "Managing iFolders", "../myifolders.html", "zFLDR xC.0000.0000.0003.", "Managing iFolders");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../myifolders.html","../toc_myifolders.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "Novell iFolder Migration And Upgrade", "../coexistmig.html", "zFLDR xC.0000.0000.0004.", "Novell iFolder Migration And Upgrade");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../coexistmig.html","../toc_coexistmig.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Document", "Legal Notices", "../legal.html", "zHTML xC.0000.0000.0005.", "Legal Notices");
      parent.theMenu.entry[level1ID].setSelected(true);
      parent.theMenu.reload();
    }
  } else {
     setTimeout("updateMenu_bookinfo();", 100);
  }
}

updateMenu_bookinfo();
