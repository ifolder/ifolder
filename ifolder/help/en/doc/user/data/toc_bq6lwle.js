function updateMenu_bq6lwle() {

   if ((parent.theMenu) && (parent.theMenu.amBusy == false))
   {
      var level0ID = parent.theMenu.currentSubRoot;

      var level1ID;
      var level2ID;
      var level3ID;
     level2ID = parent.theMenu.findEntry('data/bq6lwle.html', 'url', 'right');
     if ((level2ID != -1) && (parent.theMenu.entry[level2ID].FirstChild < 0))
      {
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Converting a Folder to an iFolder", "../doc/user/data/bq6lwlf.html", "zHTML xD.0000.0000.0001.0001.", "Converting a Folder to an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Viewing Properties of an iFolder", "../doc/user/data/bq6lwlj.html", "zHTML xD.0000.0000.0001.0002.", "Viewing Properties of an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Sharing an iFolder", "../doc/user/data/bq6lwlu.html", "zHTML xD.0000.0000.0001.0003.", "Sharing an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Accepting an Invitation to Share an iFolder", "../doc/user/data/bq6lwmh.html", "zHTML xD.0000.0000.0001.0004.", "Accepting an Invitation to Share an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Unsharing an iFolder", "../doc/user/data/bq6lwmr.html", "zHTML xD.0000.0000.0001.0005.", "Unsharing an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Deleting an iFolder", "../doc/user/data/bq6lwmy.html", "zHTML xD.0000.0000.0001.0006.", "Deleting an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Reverting an iFolder to a Normal Folder", "../doc/user/data/bq6lwmz.html", "zHTML xD.0000.0000.0001.0007.", "Reverting an iFolder to a Normal Folder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Monitoring iFolder Background Activity with Trace Window", "../doc/user/data/bqa7xnx.html", "zHTML xD.0000.0000.0001.0008.", "Monitoring iFolder Background Activity with Trace Window");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_bq6lwle();", 1000);
   }

}

updateMenu_bq6lwle();
