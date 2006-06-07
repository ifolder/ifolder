function updateMenu_accountsprefs() {

   if ((parent.theMenu) && (parent.theMenu.amBusy == false))
   {
      var level0ID = parent.theMenu.currentSubRoot;

      var level1ID;
      var level2ID;
      var level3ID;
     level2ID = parent.theMenu.findEntry('data/accountsprefs.html', 'url', 'right');
     if ((level2ID != -1) && (parent.theMenu.entry[level2ID].FirstChild < 0))
      {
      level3ID = parent.theMenu.addChild(level2ID, "Document", "啟動 iFolder 用戶端", "../doc/user/data/bw08fxi.html", "zHTML xD.0000.0000.0002.0001.", "啟動 iFolder 用戶端");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "設定 iFolder 帳戶", "../doc/user/data/accounts.html", "zHTML xD.0000.0000.0002.0002.", "設定 iFolder 帳戶");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "登入 iFolder 帳戶", "../doc/user/data/login.html", "zHTML xD.0000.0000.0002.0003.", "登入 iFolder 帳戶");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "檢視和修改 iFolder 帳戶設定", "../doc/user/data/accountdetails.html", "zHTML xD.0000.0000.0002.0004.", "檢視和修改 iFolder 帳戶設定");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "刪除 iFolder 帳戶", "../doc/user/data/bvwsp7c.html", "zHTML xD.0000.0000.0002.0005.", "刪除 iFolder 帳戶");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "設定用戶端的 iFolder 優先設定", "../doc/user/data/preferences.html", "zHTML xD.0000.0000.0002.0006.", "設定用戶端的 iFolder 優先設定");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "設定 iFolder 交通的本地防火牆設定", "../doc/user/data/bvwppw5.html", "zHTML xD.0000.0000.0002.0007.", "設定 iFolder 交通的本地防火牆設定");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "設定 iFolder 交通的本地病毒掃描器設定", "../doc/user/data/bwbirb9.html", "zHTML xD.0000.0000.0002.0008.", "設定 iFolder 交通的本地病毒掃描器設定");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_accountsprefs();", 1000);
   }

}

updateMenu_accountsprefs();