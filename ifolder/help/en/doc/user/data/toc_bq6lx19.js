function updateMenu_bq6lx19() {

   if ((parent.theMenu) && (parent.theMenu.amBusy == false))
   {
      var level0ID = parent.theMenu.currentSubRoot;

      var level1ID;
      var level2ID;
      var level3ID;
     level2ID = parent.theMenu.findEntry('data/bq6lx19.html', 'url', 'right');
     if ((level2ID != -1) && (parent.theMenu.entry[level2ID].FirstChild < 0))
      {
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Accessing the Address Book", "../doc/user/data/bq6lx1a.html", "zHTML xD.0000.0000.0002.0001.", "Accessing the Address Book");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Adding a Contact", "../doc/user/data/bq6lx1b.html", "zHTML xD.0000.0000.0002.0002.", "Adding a Contact");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Finding a Contact", "../doc/user/data/bq6lx1g.html", "zHTML xD.0000.0000.0002.0003.", "Finding a Contact");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Viewing Contact Information", "../doc/user/data/bq6lx1l.html", "zHTML xD.0000.0000.0002.0004.", "Viewing Contact Information");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Modifying Contact Information", "../doc/user/data/bq6lx1r.html", "zHTML xD.0000.0000.0002.0005.", "Modifying Contact Information");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Using the Contact Picker", "../doc/user/data/bq6lx1x.html", "zHTML xD.0000.0000.0002.0006.", "Using the Contact Picker");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Deleting a Contact", "../doc/user/data/bq6lx2f.html", "zHTML xD.0000.0000.0002.0007.", "Deleting a Contact");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Adding an Address Book", "../doc/user/data/bqa8m8i.html", "zHTML xD.0000.0000.0002.0008.", "Adding an Address Book");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "Deleting an Address Book", "../doc/user/data/bqa8sw8.html", "zHTML xD.0000.0000.0002.0009.", "Deleting an Address Book");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_bq6lx19();", 1000);
   }

}

updateMenu_bq6lx19();
