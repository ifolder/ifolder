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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolderの配置に関するガイドライン", "../doc/user/data/bx08x4y.html", "zHTML xD.0000.0000.0003.0001.", "iFolderの配置に関するガイドライン");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "同期を行わないファイルのタイプとサイズに関するガイドライン", "../doc/user/data/bwensdb.html", "zHTML xD.0000.0000.0003.0002.", "同期を行わないファイルのタイプとサイズに関するガイドライン");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolderとそのフォルダおよびファイルの命名規則", "../doc/user/data/bwagrrn.html", "zHTML xD.0000.0000.0003.0003.", "iFolderとそのフォルダおよびファイルの命名規則");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolderの作成", "../doc/user/data/createifolder.html", "zHTML xD.0000.0000.0003.0004.", "iFolderの作成");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolderの共有", "../doc/user/data/sharewith.html", "zHTML xD.0000.0000.0003.0005.", "iFolderの共有");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "使用可能なiFolderのセットアップ", "../doc/user/data/setupifolder.html", "zHTML xD.0000.0000.0003.0006.", "使用可能なiFolderのセットアップ");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolderのプロパティの表示と設定", "../doc/user/data/propifolders.html", "zHTML xD.0000.0000.0003.0007.", "iFolderのプロパティの表示と設定");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "ファイルの同期", "../doc/user/data/sync.html", "zHTML xD.0000.0000.0003.0008.", "ファイルの同期");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "ファイルの競合の解決", "../doc/user/data/conflicts.html", "zHTML xD.0000.0000.0003.0009.", "ファイルの競合の解決");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolderを標準フォルダに戻す", "../doc/user/data/reverting.html", "zHTML xD.0000.0000.0003.0010.", "iFolderを標準フォルダに戻す");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolderの移動", "../doc/user/data/movelocation.html", "zHTML xD.0000.0000.0003.0011.", "iFolderの移動");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolderの削除", "../doc/user/data/deleting.html", "zHTML xD.0000.0000.0003.0012.", "iFolderの削除");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_myifolders();", 1000);
   }

}

updateMenu_myifolders();
