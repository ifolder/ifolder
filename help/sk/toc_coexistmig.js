function updateMenu_coexistmig() {
  if ((parent.theMenu) && (parent.theMenu.amBusy == false))
  {
    var level0ID = parent.theMenu.currentSubRoot;
    var level1ID;
    var level2ID;
    var level3ID;
    var level4ID;
    level2ID = parent.theMenu.findEntry('/coexistmig.html', 'url', 'right');
    if ((level2ID != -1) && (parent.theMenu.entry[level2ID].FirstChild < 0))
    {
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Migrating from iFolder 2.x to iFolder 3.7", "../migration.html", "zHTML xD.0000.0000.0004.0001.", "Migrating from iFolder 2.x to iFolder 3.7");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Upgrading iFolder 3.x Clients", "../bctryt7.html", "zHTML xD.0000.0000.0004.0002.", "Upgrading iFolder 3.x Clients");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Coexistence of Novell iFolder 2.x and iFolder Clients", "../bctrxs4.html", "zHTML xD.0000.0000.0004.0003.", "Coexistence of Novell iFolder 2.x and iFolder Clients");
      parent.theMenu.reload();
    }
  } else {
     setTimeout("updateMenu_coexistmig();", 100);
  }
}

updateMenu_coexistmig();
