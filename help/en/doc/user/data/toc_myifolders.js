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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Viewing Animated Demonstrations of iFolder Tasks", "../doc/user/data/demos.html", "zHTML xD.0000.0000.0003.0001.", "Viewing Animated Demonstrations of iFolder Tasks");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Naming Conventions for an iFolder and Its Folders and Files", "../doc/user/data/bwagrrn.html", "zHTML xD.0000.0000.0003.0002.", "Naming Conventions for an iFolder and Its Folders and Files");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Creating an iFolder", "../doc/user/data/createifolder.html", "zHTML xD.0000.0000.0003.0003.", "Creating an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Sharing an iFolder", "../doc/user/data/sharewith.html", "zHTML xD.0000.0000.0003.0004.", "Sharing an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Setting Up an Available iFolder", "../doc/user/data/setupifolder.html", "zHTML xD.0000.0000.0003.0005.", "Setting Up an Available iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Viewing and Configuring Properties of an iFolder", "../doc/user/data/propifolders.html", "zHTML xD.0000.0000.0003.0006.", "Viewing and Configuring Properties of an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Synchronizing Files", "../doc/user/data/sync.html", "zHTML xD.0000.0000.0003.0007.", "Synchronizing Files");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Resolving File Conflicts", "../doc/user/data/conflicts.html", "zHTML xD.0000.0000.0003.0008.", "Resolving File Conflicts");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Reverting an iFolder to a Normal Folder", "../doc/user/data/reverting.html", "zHTML xD.0000.0000.0003.0009.", "Reverting an iFolder to a Normal Folder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Moving an iFolder", "../doc/user/data/movelocation.html", "zHTML xD.0000.0000.0003.0010.", "Moving an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Deleting an iFolder", "../doc/user/data/deleting.html", "zHTML xD.0000.0000.0003.0011.", "Deleting an iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_myifolders();", 1000);
   }

}

updateMenu_myifolders();
