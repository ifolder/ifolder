﻿function updateMenu_front() {

   if ((parent.theMenu) && (parent.theMenu.amBusy == false))
   {
      var level0ID = parent.theMenu.currentSubRoot;

      var level1ID;
      var level2ID;
      var level3ID;
     level1ID = parent.theMenu.findEntry('data/front.html', 'url', 'right');
     if (level1ID == -1)
      {
      level1ID = parent.theMenu.addChild(level0ID, "", "iFolderヘルプ", "../doc/user/data/front.html", "zFLDR xB.0000.0000.", "iFolderヘルプ");
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "iFolderの概要", "../doc/user/data/overview.html", "zFLDR xC.0000.0000.0001.", "iFolderの概要");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../doc/user/data/overview.html","../doc/user/data/toc_overview.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "iFolderアカウントと初期設定の管理", "../doc/user/data/accountsprefs.html", "zFLDR xC.0000.0000.0002.", "iFolderアカウントと初期設定の管理");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../doc/user/data/accountsprefs.html","../doc/user/data/toc_accountsprefs.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Folder", "iFolderの管理", "../doc/user/data/myifolders.html", "zFLDR xC.0000.0000.0003.", "iFolderの管理");
      parent.theMenu.entry[level2ID].FirstChild = -2;
      parent.theMenu.entry[level2ID].onToggle = 'parent.theMenu.loadScript("../doc/user/data/myifolders.html","../doc/user/data/toc_myifolders.html", true)';
      level2ID = parent.theMenu.addChild(level1ID, "Document", "保証と著作権", "../doc/user/data/legal.html", "zHTML xC.0000.0000.0004.", "保証と著作権");
      parent.theMenu.entry[level1ID].setSelected(true);
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_front();", 1000);
   }

}

updateMenu_front();
