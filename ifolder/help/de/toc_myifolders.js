function updateMenu_myifolders() {
  if ((parent.theMenu) && (parent.theMenu.amBusy == false))
  {
    var level0ID = parent.theMenu.currentSubRoot;
    var level1ID;
    var level2ID;
    var level3ID;
    var level4ID;
    level2ID = parent.theMenu.findEntry('/myifolders.html', 'url', 'right');
    if ((level2ID != -1) && (parent.theMenu.entry[level2ID].FirstChild < 0))
    {
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Guidelines for the Location and Use of iFolders", "../locguide.html", "zHTML xD.0000.0000.0003.0001.", "Guidelines for the Location and Use of iFolders");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Guidelines for File Types and Sizes to Not Synchronize", "../typeguide.html", "zHTML xD.0000.0000.0003.0002.", "Guidelines for File Types and Sizes to Not Synchronize");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Naming Conventions for an iFolder and Its Folders and Files", "../nameconv.html", "zHTML xD.0000.0000.0003.0003.", "Naming Conventions for an iFolder and Its Folders and Files");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Understanding iFolder Icons", "../ificons.html", "zHTML xD.0000.0000.0003.0004.", "Understanding iFolder Icons");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Creating and Uploading an iFolder", "../createifolder.html", "zHTML xD.0000.0000.0003.0005.", "Creating and Uploading an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Sharing an iFolder", "../sharewith.html", "zHTML xD.0000.0000.0003.0006.", "Sharing an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Viewing and Hiding Available iFolders", "../viewhide.html", "zHTML xD.0000.0000.0003.0007.", "Viewing and Hiding Available iFolders");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Merging iFolders", "../merge.html", "zHTML xD.0000.0000.0003.0008.", "Merging iFolders");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Downloading an Available iFolder", "../setupifolder.html", "zHTML xD.0000.0000.0003.0009.", "Downloading an Available iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Viewing and Configuring Properties of an iFolder", "../propifolders.html", "zHTML xD.0000.0000.0003.0010.", "Viewing and Configuring Properties of an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Managing Passphrase for Encrypted iFolders", "../managingpassphrse.html", "zHTML xD.0000.0000.0003.0011.", "Managing Passphrase for Encrypted iFolders");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Setting an iFolder Quota", "../ifquota.html", "zHTML xD.0000.0000.0003.0012.", "Setting an iFolder Quota");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Synchronizing Files", "../sync.html", "zHTML xD.0000.0000.0003.0013.", "Synchronizing Files");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Resolving File Conflicts", "../conflicts.html", "zHTML xD.0000.0000.0003.0014.", "Resolving File Conflicts");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Moving an iFolder", "../movelocation.html", "zHTML xD.0000.0000.0003.0015.", "Moving an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Reverting an iFolder to a Normal Folder", "../reverting.html", "zHTML xD.0000.0000.0003.0016.", "Reverting an iFolder to a Normal Folder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Removing Membership From a Shared iFolder", "../bbb7rj2.html", "zHTML xD.0000.0000.0003.0017.", "Removing Membership From a Shared iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Deleting an iFolder", "../deleting.html", "zHTML xD.0000.0000.0003.0018.", "Deleting an iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "What’s Next", "../bbbcabq.html", "zHTML xD.0000.0000.0003.0019.", "What’s Next");
      parent.theMenu.reload();
    }
  } else {
     setTimeout("updateMenu_myifolders();", 100);
  }
}

updateMenu_myifolders();
