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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuring iFolder Accounts", "../doc/user/data/accounts.html", "zHTML xD.0000.0000.0002.0001.", "Configuring iFolder Accounts");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuring iFolder Preferences", "../doc/user/data/preferences.html", "zHTML xD.0000.0000.0002.0002.", "Configuring iFolder Preferences");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Creating an iFolder", "../doc/user/data/createifolder.html", "zHTML xD.0000.0000.0002.0003.", "Creating an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Sharing an iFolder", "../doc/user/data/sharewith.html", "zHTML xD.0000.0000.0002.0004.", "Sharing an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Setting Up an Available iFolder", "../doc/user/data/setupifolder.html", "zHTML xD.0000.0000.0002.0005.", "Setting Up an Available iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Viewing Properties of an iFolder", "../doc/user/data/properties.html.html", "zHTML xD.0000.0000.0002.0006.", "Viewing Properties of an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Synchronizing Files", "../doc/user/data/sync.html", "zHTML xD.0000.0000.0002.0007.", "Synchronizing Files");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Resolving File Conflicts", "../doc/user/data/conflicts.html", "zHTML xD.0000.0000.0002.0008.", "Resolving File Conflicts");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Reverting an iFolder to a Normal Folder", "../doc/user/data/reverting.html", "zHTML xD.0000.0000.0002.0009.", "Reverting an iFolder to a Normal Folder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Deleting an iFolder", "../doc/user/data/deleting.html", "zHTML xD.0000.0000.0002.0010.", "Deleting an iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_myifolders();", 1000);
   }

}

updateMenu_myifolders();
