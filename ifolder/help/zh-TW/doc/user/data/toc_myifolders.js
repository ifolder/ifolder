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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "放置 iFolder 的指示", "../doc/user/data/bx08x4y.html", "zHTML xD.0000.0000.0003.0001.", "放置 iFolder 的指示");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "檔案類型和大小不同步的指示", "../doc/user/data/bwensdb.html", "zHTML xD.0000.0000.0003.0002.", "檔案類型和大小不同步的指示");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolder 及其資料夾和檔案的命名慣例", "../doc/user/data/bwagrrn.html", "zHTML xD.0000.0000.0003.0003.", "iFolder 及其資料夾和檔案的命名慣例");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "建立 iFolder", "../doc/user/data/createifolder.html", "zHTML xD.0000.0000.0003.0004.", "建立 iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "共用 iFolder", "../doc/user/data/sharewith.html", "zHTML xD.0000.0000.0003.0005.", "共用 iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "設定可用的 iFolder", "../doc/user/data/setupifolder.html", "zHTML xD.0000.0000.0003.0006.", "設定可用的 iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "檢視和設定 iFolder 的內容", "../doc/user/data/propifolders.html", "zHTML xD.0000.0000.0003.0007.", "檢視和設定 iFolder 的內容");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "同步化檔案", "../doc/user/data/sync.html", "zHTML xD.0000.0000.0003.0008.", "同步化檔案");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "解決檔案衝突", "../doc/user/data/conflicts.html", "zHTML xD.0000.0000.0003.0009.", "解決檔案衝突");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "將 iFolder 恢復為一般資料夾", "../doc/user/data/reverting.html", "zHTML xD.0000.0000.0003.0010.", "將 iFolder 恢復為一般資料夾");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "移動 iFolder", "../doc/user/data/movelocation.html", "zHTML xD.0000.0000.0003.0011.", "移動 iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "刪除 iFolder", "../doc/user/data/deleting.html", "zHTML xD.0000.0000.0003.0012.", "刪除 iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_myifolders();", 1000);
   }

}

updateMenu_myifolders();