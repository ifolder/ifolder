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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolder 定位准则", "../doc/user/data/bx08x4y.html", "zHTML xD.0000.0000.0003.0001.", "iFolder 定位准则");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "不适合同步的文件类型和大小的准则", "../doc/user/data/bwensdb.html", "zHTML xD.0000.0000.0003.0002.", "不适合同步的文件类型和大小的准则");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "iFolder 及其文件夹和文件的命名约定", "../doc/user/data/bwagrrn.html", "zHTML xD.0000.0000.0003.0003.", "iFolder 及其文件夹和文件的命名约定");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "创建 iFolder", "../doc/user/data/createifolder.html", "zHTML xD.0000.0000.0003.0004.", "创建 iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "共享 iFolder", "../doc/user/data/sharewith.html", "zHTML xD.0000.0000.0003.0005.", "共享 iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "设置可用 iFolder", "../doc/user/data/setupifolder.html", "zHTML xD.0000.0000.0003.0006.", "设置可用 iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "查看和配置 iFolder 的属性", "../doc/user/data/propifolders.html", "zHTML xD.0000.0000.0003.0007.", "查看和配置 iFolder 的属性");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "同步文件", "../doc/user/data/sync.html", "zHTML xD.0000.0000.0003.0008.", "同步文件");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "解决文件冲突", "../doc/user/data/conflicts.html", "zHTML xD.0000.0000.0003.0009.", "解决文件冲突");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "将 iFolder 恢复为常规文件夹", "../doc/user/data/reverting.html", "zHTML xD.0000.0000.0003.0010.", "将 iFolder 恢复为常规文件夹");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "移动 iFolder", "../doc/user/data/movelocation.html", "zHTML xD.0000.0000.0003.0011.", "移动 iFolder");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "删除 iFolder", "../doc/user/data/deleting.html", "zHTML xD.0000.0000.0003.0012.", "删除 iFolder");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_myifolders();", 1000);
   }

}

updateMenu_myifolders();