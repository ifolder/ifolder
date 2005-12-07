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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolderクライアントの起動", "../doc/user/data/bw08fxi.html", "zHTML xD.0000.0000.0002.0001.", "iFolderクライアントの起動");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolderアカウントの設定", "../doc/user/data/accounts.html", "zHTML xD.0000.0000.0002.0002.", "iFolderアカウントの設定");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolderアカウントへのログイン", "../doc/user/data/login.html", "zHTML xD.0000.0000.0002.0003.", "iFolderアカウントへのログイン");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolderアカウント設定の表示と変更", "../doc/user/data/accountdetails.html", "zHTML xD.0000.0000.0002.0004.", "iFolderアカウント設定の表示と変更");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolderアカウントの削除", "../doc/user/data/bvwsp7c.html", "zHTML xD.0000.0000.0002.0005.", "iFolderアカウントの削除");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "クライアントのiFolder初期設定を行う", "../doc/user/data/preferences.html", "zHTML xD.0000.0000.0002.0006.", "クライアントのiFolder初期設定を行う");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolderトラフィックにローカルファイアウォールを設定する", "../doc/user/data/bvwppw5.html", "zHTML xD.0000.0000.0002.0007.", "iFolderトラフィックにローカルファイアウォールを設定する");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolderトラフィックにローカルウィルススキャナを設定する", "../doc/user/data/bwbirb9.html", "zHTML xD.0000.0000.0002.0008.", "iFolderトラフィックにローカルウィルススキャナを設定する");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_accountsprefs();", 1000);
   }

}

updateMenu_accountsprefs();
