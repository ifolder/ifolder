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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Viewing Animated Demonstrations of iFolder Tasks", "../doc/user/data/demos.html", "zHTML xD.0000.0000.0002.0001.", "Viewing Animated Demonstrations of iFolder Tasks");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Logging In to the iFolder Client", "../doc/user/data/login.html", "zHTML xD.0000.0000.0002.0002.", "Logging In to the iFolder Client");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuring an iFolder Account", "../doc/user/data/accounts.html", "zHTML xD.0000.0000.0002.0003.", "Configuring an iFolder Account");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Viewing Account Details", "../doc/user/data/accountdetails.html", "zHTML xD.0000.0000.0002.0004.", "Viewing Account Details");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuring Preferences for the Client", "../doc/user/data/preferences.html", "zHTML xD.0000.0000.0002.0005.", "Configuring Preferences for the Client");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Creating an iFolder", "../doc/user/data/createifolder.html", "zHTML xD.0000.0000.0002.0006.", "Creating an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Sharing an iFolder", "../doc/user/data/sharewith.html", "zHTML xD.0000.0000.0002.0007.", "Sharing an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Setting Up an Available iFolder", "../doc/user/data/setupifolder.html", "zHTML xD.0000.0000.0002.0008.", "Setting Up an Available iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Viewing and Configuring Properties of an iFolder", "../doc/user/data/propifolders.html", "zHTML xD.0000.0000.0002.0009.", "Viewing and Configuring Properties of an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Synchronizing Files", "../doc/user/data/sync.html", "zHTML xD.0000.0000.0002.0010.", "Synchronizing Files");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Resolving File Conflicts", "../doc/user/data/conflicts.html", "zHTML xD.0000.0000.0002.0011.", "Resolving File Conflicts");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Reverting an iFolder to a Normal Folder", "../doc/user/data/reverting.html", "zHTML xD.0000.0000.0002.0012.", "Reverting an iFolder to a Normal Folder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Moving an iFolder", "../doc/user/data/movelocation.html", "zHTML xD.0000.0000.0002.0013.", "Moving an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Deleting an iFolder", "../doc/user/data/deleting.html", "zHTML xD.0000.0000.0002.0014.", "Deleting an iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_myifolders();", 1000);
   }

}

updateMenu_myifolders();
