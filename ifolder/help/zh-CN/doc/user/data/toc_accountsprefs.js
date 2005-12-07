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
      level3ID = parent.theMenu.addChild(level2ID, "Document", "启动 iFolder 客户程序", "../doc/user/data/bw08fxi.html", "zHTML xD.0000.0000.0002.0001.", "启动 iFolder 客户程序");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "配置 iFolder 帐户", "../doc/user/data/accounts.html", "zHTML xD.0000.0000.0002.0002.", "配置 iFolder 帐户");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "登录 iFolder 帐户", "../doc/user/data/login.html", "zHTML xD.0000.0000.0002.0003.", "登录 iFolder 帐户");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "查看和修改 iFolder 帐户设置", "../doc/user/data/accountdetails.html", "zHTML xD.0000.0000.0002.0004.", "查看和修改 iFolder 帐户设置");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "删除 iFolder 帐户", "../doc/user/data/bvwsp7c.html", "zHTML xD.0000.0000.0002.0005.", "删除 iFolder 帐户");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "配置客户程序的 iFolder 自选设置", "../doc/user/data/preferences.html", "zHTML xD.0000.0000.0002.0006.", "配置客户程序的 iFolder 自选设置");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "为 iFolder 交通配置本地防火墙设置", "../doc/user/data/bvwppw5.html", "zHTML xD.0000.0000.0002.0007.", "为 iFolder 交通配置本地防火墙设置");
      level3ID = parent.theMenu.addChild(level2ID, "Document", "为 iFolder 交通配置本地病毒扫描程序设置", "../doc/user/data/bwbirb9.html", "zHTML xD.0000.0000.0002.0008.", "为 iFolder 交通配置本地病毒扫描程序设置");
      parent.theMenu.reload();

      }
   } else {
      setTimeout("updateMenu_accountsprefs();", 1000);
   }

}

updateMenu_accountsprefs();