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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Guidelines for the Location and Use of iFolders", "../doc/user/data/locguide.html", "zHTML xD.0000.0000.0003.0001.", "Guidelines for the Location and Use of iFolders");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Guidelines for File Types and Sizes to Not Synchronize", "../doc/user/data/typeguide.html", "zHTML xD.0000.0000.0003.0002.", "Guidelines for File Types and Sizes to Not Synchronize");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Naming Conventions for an iFolder and Its Folders and Files", "../doc/user/data/nameconv.html", "zHTML xD.0000.0000.0003.0003.", "Naming Conventions for an iFolder and Its Folders and Files");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Understanding iFolder Icons", "../doc/user/data/ificons.html", "zHTML xD.0000.0000.0003.0004.", "Understanding iFolder Icons");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Creating and Uploading an iFolder", "../doc/user/data/createifolder.html", "zHTML xD.0000.0000.0003.0005.", "Creating and Uploading an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Sharing an iFolder", "../doc/user/data/sharewith.html", "zHTML xD.0000.0000.0003.0006.", "Sharing an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Viewing and Hiding Available iFolders", "../doc/user/data/viewhide.html", "zHTML xD.0000.0000.0003.0007.", "Viewing and Hiding Available iFolders");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Downloading an Available iFolder", "../doc/user/data/setupifolder.html", "zHTML xD.0000.0000.0003.0008.", "Downloading an Available iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Viewing and Configuring Properties of an iFolder", "../doc/user/data/propifolders.html", "zHTML xD.0000.0000.0003.0009.", "Viewing and Configuring Properties of an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Managing Passphrase for Encrypted iFolders", "../doc/user/data/managingpassphrse.html", "zHTML xD.0000.0000.0003.0010.", "Managing Passphrase for Encrypted iFolders");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Setting an iFolder Quota", "../doc/user/data/ifquota.html", "zHTML xD.0000.0000.0003.0011.", "Setting an iFolder Quota");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Synchronizing Files", "../doc/user/data/sync.html", "zHTML xD.0000.0000.0003.0012.", "Synchronizing Files");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Resolving File Conflicts", "../doc/user/data/conflicts.html", "zHTML xD.0000.0000.0003.0013.", "Resolving File Conflicts");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Reverting an iFolder to a Normal Folder", "../doc/user/data/reverting.html", "zHTML xD.0000.0000.0003.0014.", "Reverting an iFolder to a Normal Folder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Moving an iFolder", "../doc/user/data/movelocation.html", "zHTML xD.0000.0000.0003.0015.", "Moving an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Deleting an iFolder", "../doc/user/data/deleting.html", "zHTML xD.0000.0000.0003.0016.", "Deleting an iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_myifolders();", 1000);
   }

}

updateMenu_myifolders();
