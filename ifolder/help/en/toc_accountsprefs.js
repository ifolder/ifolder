function updateMenu_accountsprefs() {
  if ((parent.theMenu) && (parent.theMenu.amBusy == false))
  {
    var level0ID = parent.theMenu.currentSubRoot;
    var level1ID;
    var level2ID;
    var level3ID;
    var level4ID;
    level2ID = parent.theMenu.findEntry('/accountsprefs.html', 'url', 'right');
    if ((level2ID != -1) && (parent.theMenu.entry[level2ID].FirstChild < 0))
    {
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Starting the iFolder Client", "../startclient.html", "zHTML xD.0000.0000.0002.0001.", "Starting the iFolder Client");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuring an iFolder Account", "../accounts.html", "zHTML xD.0000.0000.0002.0002.", "Configuring an iFolder Account");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Logging In to an iFolder Account", "../login.html", "zHTML xD.0000.0000.0002.0003.", "Logging In to an iFolder Account");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Viewing and Modifying iFolder Account Settings", "../accountdetails.html", "zHTML xD.0000.0000.0002.0004.", "Viewing and Modifying iFolder Account Settings");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuring iFolder Preferences for the Client", "../preferences.html", "zHTML xD.0000.0000.0002.0005.", "Configuring iFolder Preferences for the Client");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuring Local Firewall Settings for iFolder Traffic", "../firewall.html", "zHTML xD.0000.0000.0002.0006.", "Configuring Local Firewall Settings for iFolder Traffic");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Configuring Local Virus Scanner Settings for iFolder Traffic", "../virusscan.html", "zHTML xD.0000.0000.0002.0007.", "Configuring Local Virus Scanner Settings for iFolder Traffic");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Deleting an iFolder Account", "../delacct.html", "zHTML xD.0000.0000.0002.0008.", "Deleting an iFolder Account");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Logging Out of an iFolder Account", "../logout.html", "zHTML xD.0000.0000.0002.0009.", "Logging Out of an iFolder Account");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Exiting the iFolder Client", "../stopclient.html", "zHTML xD.0000.0000.0002.0010.", "Exiting the iFolder Client");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "What’s Next", "../b9lvmk9.html", "zHTML xD.0000.0000.0002.0011.", "What’s Next");
      parent.theMenu.reload();
    }
  } else {
     setTimeout("updateMenu_accountsprefs();", 100);
  }
}

updateMenu_accountsprefs();
