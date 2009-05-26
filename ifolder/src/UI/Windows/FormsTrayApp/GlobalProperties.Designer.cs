using System;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using Novell.iFolderCom;
using System.Windows.Forms;

namespace Novell.FormsTrayApp
{
	public partial class GlobalProperties
	{
		#region Windows Form Designer generated code
		
		private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GlobalProperties));
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuAction = new System.Windows.Forms.MenuItem();
            this.menuActionCreate = new System.Windows.Forms.MenuItem();
            this.menuActionAccept = new System.Windows.Forms.MenuItem();
            this.menuActionMerge = new System.Windows.Forms.MenuItem();
            this.menuActionRemove = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuActionOpen = new System.Windows.Forms.MenuItem();
            this.menuActionShare = new System.Windows.Forms.MenuItem();
            this.menuActionResolve = new System.Windows.Forms.MenuItem();
            this.menuActionSync = new System.Windows.Forms.MenuItem();
            this.menuActionRevert = new System.Windows.Forms.MenuItem();
            this.menuActionProperties = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.migrationMenuItem = new System.Windows.Forms.MenuItem();
            this.migrationMenuSubItem = new System.Windows.Forms.MenuItem();
            this.menuSeparator = new System.Windows.Forms.MenuItem();
            this.menuActionClose = new System.Windows.Forms.MenuItem();
            this.menuActionExit = new System.Windows.Forms.MenuItem();
            this.menuEdit = new System.Windows.Forms.MenuItem();
            this.menuViewAccounts = new System.Windows.Forms.MenuItem();
            this.menuEditPrefs = new System.Windows.Forms.MenuItem();
            this.menuView = new System.Windows.Forms.MenuItem();
            this.menuViewRefresh = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuViewLog = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuViewAvailable = new System.Windows.Forms.MenuItem();
            this.menuSecurity = new System.Windows.Forms.MenuItem();
            this.menuRecoverKeys = new System.Windows.Forms.MenuItem();
            this.menuExportKeys = new System.Windows.Forms.MenuItem();
            this.menuImportKeys = new System.Windows.Forms.MenuItem();
            this.menuResetPassphrase = new System.Windows.Forms.MenuItem();
            this.menuResetPassword = new System.Windows.Forms.MenuItem();
            this.menuHelp = new System.Windows.Forms.MenuItem();
            this.menuHelpHelp = new System.Windows.Forms.MenuItem();
            this.menuHelpAbout = new System.Windows.Forms.MenuItem();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.searchTimer = new System.Windows.Forms.Timer(this.components);
            this.refreshTimer = new System.Windows.Forms.Timer(this.components);
            this.toolStripiFolderActions = new System.Windows.Forms.ToolStrip();
            this.toolStripBtnCreate = new System.Windows.Forms.ToolStripButton();
            this.toolStripBtnDownload = new System.Windows.Forms.ToolStripButton();
            this.toolStripBtnSyncNow = new System.Windows.Forms.ToolStripButton();
            this.toolStripBtnShare = new System.Windows.Forms.ToolStripButton();
            this.toolStripBtnResolve = new System.Windows.Forms.ToolStripButton();
            this.toolStripBtnMerge = new System.Windows.Forms.ToolStripButton();
            this.toolStripBtnFilter = new System.Windows.Forms.ToolStripComboBox();
            this.toolStipBtnChangeView = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuThumbnails = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuDetails = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuLeftPane = new System.Windows.Forms.ToolStripMenuItem();
            this.iFolderContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CtxMenuOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuResolveSeperator = new System.Windows.Forms.ToolStripSeparator();
            this.MenuResolve = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuAccept = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuMerge = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSyncNow = new System.Windows.Forms.ToolStripMenuItem();
            this.menuShare = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuRevert = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.panel3 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.LoginLogoff = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.filter = new System.Windows.Forms.TextBox();
            this.titleAvailable = new System.Windows.Forms.Label();
            this.titleUsed = new System.Windows.Forms.Label();
            this.titleDiskQuota = new System.Windows.Forms.Label();
            this.titleNOFolders = new System.Windows.Forms.Label();
            this.titleServer = new System.Windows.Forms.Label();
            this.titleUser = new System.Windows.Forms.Label();
            this.domainListComboBox = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.titleName = new System.Windows.Forms.Label();
            this.titleOwner = new System.Windows.Forms.Label();
            this.titleRemainingToSync = new System.Windows.Forms.Label();
            this.titleLastSyncTime = new System.Windows.Forms.Label();
            this.titleAutomaticSync = new System.Windows.Forms.Label();
            this.titleAccess = new System.Windows.Forms.Label();
            this.valueName = new System.Windows.Forms.Label();
            this.valueAccess = new System.Windows.Forms.Label();
            this.valueRemainingToSync = new System.Windows.Forms.Label();
            this.valueLastSyncTime = new System.Windows.Forms.Label();
            this.valueAutomaticSync = new System.Windows.Forms.Label();
            this.titleEncrypted = new System.Windows.Forms.Label();
            this.valueEncrypted = new System.Windows.Forms.Label();
            this.directorySearcher1 = new System.DirectoryServices.DirectorySearcher();
            this.panel2 = new System.Windows.Forms.Panel();
            this.localiFoldersHeading = new System.Windows.Forms.RichTextBox();
            this.iFolderView = new Novell.FormsTrayApp.TileListView();
            this.toolStripiFolderActions.SuspendLayout();
            this.iFolderContextMenu.SuspendLayout();
            this.panel3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuAction,
            this.menuEdit,
            this.menuView,
            this.menuSecurity,
            this.menuHelp});
            // 
            // menuAction
            // 
            this.menuAction.Index = 0;
            this.menuAction.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuActionCreate,
            this.menuActionAccept,
            this.menuActionMerge,
            this.menuActionRemove,
            this.menuItem7,
            this.menuActionOpen,
            this.menuActionShare,
            this.menuActionResolve,
            this.menuActionSync,
            this.menuActionRevert,
            this.menuActionProperties,
            this.menuItem4,
            this.migrationMenuItem,
            this.menuSeparator,
            this.menuActionClose,
            this.menuActionExit});
            resources.ApplyResources(this.menuAction, "menuAction");
            // 
            // menuActionCreate
            // 
            this.menuActionCreate.Index = 0;
            resources.ApplyResources(this.menuActionCreate, "menuActionCreate");
            this.menuActionCreate.Click += new System.EventHandler(this.menuCreate_Click);
            // 
            // menuActionAccept
            // 
            resources.ApplyResources(this.menuActionAccept, "menuActionAccept");
            this.menuActionAccept.Index = 1;
            this.menuActionAccept.Click += new System.EventHandler(this.menuAccept_Click);
            // 
            // menuActionMerge
            // 
            resources.ApplyResources(this.menuActionMerge, "menuActionMerge");
            this.menuActionMerge.Index = 2;
            this.menuActionMerge.Click += new System.EventHandler(this.menuMerge_Click);
            // 
            // menuActionRemove
            // 
            resources.ApplyResources(this.menuActionRemove, "menuActionRemove");
            this.menuActionRemove.Index = 3;
            this.menuActionRemove.Click += new System.EventHandler(this.menuRemove_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 4;
            resources.ApplyResources(this.menuItem7, "menuItem7");
            // 
            // menuActionOpen
            // 
            resources.ApplyResources(this.menuActionOpen, "menuActionOpen");
            this.menuActionOpen.Index = 5;
            this.menuActionOpen.Click += new System.EventHandler(this.menuOpen_Click);
            // 
            // menuActionShare
            // 
            resources.ApplyResources(this.menuActionShare, "menuActionShare");
            this.menuActionShare.Index = 6;
            this.menuActionShare.Click += new System.EventHandler(this.menuShare_Click);
            // 
            // menuActionResolve
            // 
            resources.ApplyResources(this.menuActionResolve, "menuActionResolve");
            this.menuActionResolve.Index = 7;
            this.menuActionResolve.Click += new System.EventHandler(this.menuResolve_Click);
            // 
            // menuActionSync
            // 
            resources.ApplyResources(this.menuActionSync, "menuActionSync");
            this.menuActionSync.Index = 8;
            this.menuActionSync.Click += new System.EventHandler(this.menuSyncNow_Click);
            // 
            // menuActionRevert
            // 
            resources.ApplyResources(this.menuActionRevert, "menuActionRevert");
            this.menuActionRevert.Index = 9;
            this.menuActionRevert.Click += new System.EventHandler(this.menuRevert_Click);
            // 
            // menuActionProperties
            // 
            resources.ApplyResources(this.menuActionProperties, "menuActionProperties");
            this.menuActionProperties.Index = 10;
            this.menuActionProperties.Click += new System.EventHandler(this.menuProperties_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 11;
            resources.ApplyResources(this.menuItem4, "menuItem4");
            // 
            // migrationMenuItem
            // 
            this.migrationMenuItem.Index = 12;
            this.migrationMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.migrationMenuSubItem});
            resources.ApplyResources(this.migrationMenuItem, "migrationMenuItem");
            // 
            // migrationMenuSubItem
            // 
            this.migrationMenuSubItem.Index = 0;
            resources.ApplyResources(this.migrationMenuSubItem, "migrationMenuSubItem");
            this.migrationMenuSubItem.Click += new System.EventHandler(this.menuMigrateMigrate_Click);
            // 
            // menuSeparator
            // 
            this.menuSeparator.Index = 13;
            resources.ApplyResources(this.menuSeparator, "menuSeparator");
            // 
            // menuActionClose
            // 
            this.menuActionClose.Index = 14;
            resources.ApplyResources(this.menuActionClose, "menuActionClose");
            this.menuActionClose.Click += new System.EventHandler(this.menuClose_Click);
            // 
            // menuActionExit
            // 
            this.menuActionExit.Index = 15;
            resources.ApplyResources(this.menuActionExit, "menuActionExit");
            this.menuActionExit.Click += new System.EventHandler(this.menuFileExit_Click);
            // 
            // menuEdit
            // 
            this.menuEdit.Index = 1;
            this.menuEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuViewAccounts,
            this.menuEditPrefs});
            resources.ApplyResources(this.menuEdit, "menuEdit");
            // 
            // menuViewAccounts
            // 
            this.menuViewAccounts.Index = 0;
            resources.ApplyResources(this.menuViewAccounts, "menuViewAccounts");
            this.menuViewAccounts.Click += new System.EventHandler(this.menuViewAccounts_Click);
            // 
            // menuEditPrefs
            // 
            this.menuEditPrefs.Index = 1;
            resources.ApplyResources(this.menuEditPrefs, "menuEditPrefs");
            this.menuEditPrefs.Click += new System.EventHandler(this.menuEditPrefs_Click);
            // 
            // menuView
            // 
            this.menuView.Index = 2;
            this.menuView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuViewRefresh,
            this.menuItem1,
            this.menuViewLog,
            this.menuItem3,
            this.menuViewAvailable});
            resources.ApplyResources(this.menuView, "menuView");
            // 
            // menuViewRefresh
            // 
            this.menuViewRefresh.Index = 0;
            resources.ApplyResources(this.menuViewRefresh, "menuViewRefresh");
            this.menuViewRefresh.Click += new System.EventHandler(this.menuRefresh_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 1;
            resources.ApplyResources(this.menuItem1, "menuItem1");
            // 
            // menuViewLog
            // 
            this.menuViewLog.Index = 2;
            resources.ApplyResources(this.menuViewLog, "menuViewLog");
            this.menuViewLog.Click += new System.EventHandler(this.menuViewLog_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 3;
            resources.ApplyResources(this.menuItem3, "menuItem3");
            // 
            // menuViewAvailable
            // 
            this.menuViewAvailable.Checked = true;
            this.menuViewAvailable.Index = 4;
            resources.ApplyResources(this.menuViewAvailable, "menuViewAvailable");
            this.menuViewAvailable.Click += new System.EventHandler(this.showiFolders_Click);
            // 
            // menuSecurity
            // 
            this.menuSecurity.Index = 3;
            this.menuSecurity.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuRecoverKeys,
            this.menuResetPassphrase,
            this.menuResetPassword});
            resources.ApplyResources(this.menuSecurity, "menuSecurity");
            // 
            // menuRecoverKeys
            // 
            this.menuRecoverKeys.Index = 0;
            this.menuRecoverKeys.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuExportKeys,
            this.menuImportKeys});
            resources.ApplyResources(this.menuRecoverKeys, "menuRecoverKeys");
            // 
            // menuExportKeys
            // 
            this.menuExportKeys.Index = 0;
            resources.ApplyResources(this.menuExportKeys, "menuExportKeys");
            this.menuExportKeys.Click += new System.EventHandler(this.menuExportKeys_Select);
            // 
            // menuImportKeys
            // 
            this.menuImportKeys.Index = 1;
            resources.ApplyResources(this.menuImportKeys, "menuImportKeys");
            this.menuImportKeys.Click += new System.EventHandler(this.menuImportKeys_Select);
            // 
            // menuResetPassphrase
            // 
            this.menuResetPassphrase.Index = 1;
            resources.ApplyResources(this.menuResetPassphrase, "menuResetPassphrase");
            this.menuResetPassphrase.Click += new System.EventHandler(this.menuResetPassphrase_Select);
            // 
            // menuResetPassword
            // 
            this.menuResetPassword.Index = 2;
            resources.ApplyResources(this.menuResetPassword, "menuResetPassword");
            this.menuResetPassword.Click += new System.EventHandler(this.menuResetPassword_Click);
            // 
            // menuHelp
            // 
            this.menuHelp.Index = 4;
            this.menuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuHelpHelp,
            this.menuHelpAbout});
            resources.ApplyResources(this.menuHelp, "menuHelp");
            // 
            // menuHelpHelp
            // 
            this.menuHelpHelp.Index = 0;
            resources.ApplyResources(this.menuHelpHelp, "menuHelpHelp");
            this.menuHelpHelp.Click += new System.EventHandler(this.menuHelpHelp_Click);
            // 
            // menuHelpAbout
            // 
            this.menuHelpAbout.Index = 1;
            resources.ApplyResources(this.menuHelpAbout, "menuHelpAbout");
            this.menuHelpAbout.Click += new System.EventHandler(this.menuHelpAbout_Click);
            // 
            // progressBar1
            // 
            resources.ApplyResources(this.progressBar1, "progressBar1");
            this.progressBar1.Name = "progressBar1";
            // 
            // statusBar1
            // 
            resources.ApplyResources(this.statusBar1, "statusBar1");
            this.statusBar1.Name = "statusBar1";
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // searchTimer
            // 
            this.searchTimer.Interval = 1000;
            this.searchTimer.Tick += new System.EventHandler(this.searchTimer_Tick);
            // 
            // refreshTimer
            // 
            this.refreshTimer.Interval = 300000;
            this.refreshTimer.Tick += new System.EventHandler(this.refreshTimer_Tick);
            // 
            // toolStripiFolderActions
            // 
            this.toolStripiFolderActions.BackColor = System.Drawing.Color.LightGray;
            this.toolStripiFolderActions.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripiFolderActions.ImageScalingSize = new System.Drawing.Size(48, 48);
            this.toolStripiFolderActions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripBtnCreate,
            this.toolStripBtnDownload,
            this.toolStripBtnSyncNow,
            this.toolStripBtnShare,
            this.toolStripBtnResolve,
            this.toolStripBtnMerge,
            this.toolStripBtnFilter,
            this.toolStipBtnChangeView});
            resources.ApplyResources(this.toolStripiFolderActions, "toolStripiFolderActions");
            this.toolStripiFolderActions.Name = "toolStripiFolderActions";
            // 
            // toolStripBtnCreate
            // 
            resources.ApplyResources(this.toolStripBtnCreate, "toolStripBtnCreate");
            this.toolStripBtnCreate.Name = "toolStripBtnCreate";
            this.toolStripBtnCreate.Tag = "Create";
            this.toolStripBtnCreate.Click += new System.EventHandler(this.menuCreate_Click);
            // 
            // toolStripBtnDownload
            // 
            resources.ApplyResources(this.toolStripBtnDownload, "toolStripBtnDownload");
            this.toolStripBtnDownload.Name = "toolStripBtnDownload";
            this.toolStripBtnDownload.Click += new System.EventHandler(this.menuAccept_Click);
            // 
            // toolStripBtnSyncNow
            // 
            resources.ApplyResources(this.toolStripBtnSyncNow, "toolStripBtnSyncNow");
            this.toolStripBtnSyncNow.Name = "toolStripBtnSyncNow";
            this.toolStripBtnSyncNow.Click += new System.EventHandler(this.menuSyncNow_Click);
            // 
            // toolStripBtnShare
            // 
            resources.ApplyResources(this.toolStripBtnShare, "toolStripBtnShare");
            this.toolStripBtnShare.Name = "toolStripBtnShare";
            this.toolStripBtnShare.Click += new System.EventHandler(this.menuShare_Click);
            // 
            // toolStripBtnResolve
            // 
            resources.ApplyResources(this.toolStripBtnResolve, "toolStripBtnResolve");
            this.toolStripBtnResolve.Name = "toolStripBtnResolve";
            this.toolStripBtnResolve.Click += new System.EventHandler(this.menuResolve_Click);
            // 
            // toolStripBtnMerge
            // 
            resources.ApplyResources(this.toolStripBtnMerge, "toolStripBtnMerge");
            this.toolStripBtnMerge.Name = "toolStripBtnMerge";
            this.toolStripBtnMerge.Click += new System.EventHandler(this.menuMerge_Click);
            // 
            // toolStripBtnFilter
            // 
            this.toolStripBtnFilter.Name = "toolStripBtnFilter";
            resources.ApplyResources(this.toolStripBtnFilter, "toolStripBtnFilter");
            this.toolStripBtnFilter.TextUpdate += new System.EventHandler(this.toolStripBtnFilter_TextUpdate);
            // 
            // toolStipBtnChangeView
            // 
            this.toolStipBtnChangeView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuThumbnails,
            this.toolStripMenuDetails,
            this.toolStripMenuLeftPane});
            resources.ApplyResources(this.toolStipBtnChangeView, "toolStipBtnChangeView");
            this.toolStipBtnChangeView.Name = "toolStipBtnChangeView";
            // 
            // toolStripMenuThumbnails
            // 
            this.toolStripMenuThumbnails.Name = "toolStripMenuThumbnails";
            resources.ApplyResources(this.toolStripMenuThumbnails, "toolStripMenuThumbnails");
            // 
            // toolStripMenuDetails
            // 
            this.toolStripMenuDetails.Name = "toolStripMenuDetails";
            resources.ApplyResources(this.toolStripMenuDetails, "toolStripMenuDetails");
            // 
            // toolStripMenuLeftPane
            // 
            this.toolStripMenuLeftPane.Checked = true;
            this.toolStripMenuLeftPane.CheckOnClick = true;
            this.toolStripMenuLeftPane.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuLeftPane.Name = "toolStripMenuLeftPane";
            resources.ApplyResources(this.toolStripMenuLeftPane, "toolStripMenuLeftPane");
            // 
            // iFolderContextMenu
            // 
            this.iFolderContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CtxMenuOpen,
            this.MenuResolveSeperator,
            this.MenuResolve,
            this.MenuAccept,
            this.MenuMerge,
            this.menuSeparator1,
            this.MenuRemove,
            this.menuSyncNow,
            this.menuShare,
            this.MenuRevert,
            this.menuSeparator2,
            this.menuProperties,
            this.MenuRefresh});
            this.iFolderContextMenu.Name = "iFolderContextMenu";
            resources.ApplyResources(this.iFolderContextMenu, "iFolderContextMenu");
            this.iFolderContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.iFolderContextMenu_Popup);
            // 
            // CtxMenuOpen
            // 
            this.CtxMenuOpen.Name = "CtxMenuOpen";
            resources.ApplyResources(this.CtxMenuOpen, "CtxMenuOpen");
            this.CtxMenuOpen.Click += new System.EventHandler(this.menuOpen_Click);
            // 
            // MenuResolveSeperator
            // 
            this.MenuResolveSeperator.Name = "MenuResolveSeperator";
            resources.ApplyResources(this.MenuResolveSeperator, "MenuResolveSeperator");
            // 
            // MenuResolve
            // 
            this.MenuResolve.Name = "MenuResolve";
            resources.ApplyResources(this.MenuResolve, "MenuResolve");
            this.MenuResolve.Click += new System.EventHandler(this.menuResolve_Click);
            // 
            // MenuAccept
            // 
            this.MenuAccept.Name = "MenuAccept";
            resources.ApplyResources(this.MenuAccept, "MenuAccept");
            this.MenuAccept.Click += new System.EventHandler(this.menuCreate_Click);
            // 
            // MenuMerge
            // 
            this.MenuMerge.Name = "MenuMerge";
            resources.ApplyResources(this.MenuMerge, "MenuMerge");
            this.MenuMerge.Click += new System.EventHandler(this.menuMerge_Click);
            // 
            // menuSeparator1
            // 
            this.menuSeparator1.Name = "menuSeparator1";
            resources.ApplyResources(this.menuSeparator1, "menuSeparator1");
            // 
            // MenuRemove
            // 
            this.MenuRemove.Name = "MenuRemove";
            resources.ApplyResources(this.MenuRemove, "MenuRemove");
            this.MenuRemove.Click += new System.EventHandler(this.menuRemove_Click);
            // 
            // menuSyncNow
            // 
            this.menuSyncNow.Name = "menuSyncNow";
            resources.ApplyResources(this.menuSyncNow, "menuSyncNow");
            this.menuSyncNow.Click += new System.EventHandler(this.menuSyncNow_Click);
            // 
            // menuShare
            // 
            this.menuShare.Name = "menuShare";
            resources.ApplyResources(this.menuShare, "menuShare");
            this.menuShare.Click += new System.EventHandler(this.menuShare_Click);
            // 
            // MenuRevert
            // 
            this.MenuRevert.Name = "MenuRevert";
            resources.ApplyResources(this.MenuRevert, "MenuRevert");
            this.MenuRevert.Click += new System.EventHandler(this.menuRevert_Click);
            // 
            // menuSeparator2
            // 
            this.menuSeparator2.Name = "menuSeparator2";
            resources.ApplyResources(this.menuSeparator2, "menuSeparator2");
            // 
            // menuProperties
            // 
            this.menuProperties.Name = "menuProperties";
            resources.ApplyResources(this.menuProperties, "menuProperties");
            this.menuProperties.Click += new System.EventHandler(this.menuProperties_Click);
            // 
            // MenuRefresh
            // 
            this.MenuRefresh.Name = "MenuRefresh";
            resources.ApplyResources(this.MenuRefresh, "MenuRefresh");
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.BackColor = System.Drawing.Color.DarkGray;
            this.panel3.Controls.Add(this.groupBox2);
            this.panel3.Name = "panel3";
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.LoginLogoff);
            this.groupBox2.Controls.Add(this.pictureBox1);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.filter);
            this.groupBox2.Controls.Add(this.titleAvailable);
            this.groupBox2.Controls.Add(this.titleUsed);
            this.groupBox2.Controls.Add(this.titleDiskQuota);
            this.groupBox2.Controls.Add(this.titleNOFolders);
            this.groupBox2.Controls.Add(this.titleServer);
            this.groupBox2.Controls.Add(this.titleUser);
            this.groupBox2.Controls.Add(this.domainListComboBox);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // LoginLogoff
            // 
            resources.ApplyResources(this.LoginLogoff, "LoginLogoff");
            this.LoginLogoff.Name = "LoginLogoff";
            this.LoginLogoff.UseVisualStyleBackColor = true;
            this.LoginLogoff.Click += new System.EventHandler(this.LoginLogoff_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.ErrorImage = null;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // filter
            // 
            resources.ApplyResources(this.filter, "filter");
            this.filter.Name = "filter";
            // 
            // titleAvailable
            // 
            resources.ApplyResources(this.titleAvailable, "titleAvailable");
            this.titleAvailable.Name = "titleAvailable";
            // 
            // titleUsed
            // 
            resources.ApplyResources(this.titleUsed, "titleUsed");
            this.titleUsed.Name = "titleUsed";
            // 
            // titleDiskQuota
            // 
            resources.ApplyResources(this.titleDiskQuota, "titleDiskQuota");
            this.titleDiskQuota.Name = "titleDiskQuota";
            // 
            // titleNOFolders
            // 
            resources.ApplyResources(this.titleNOFolders, "titleNOFolders");
            this.titleNOFolders.Name = "titleNOFolders";
            // 
            // titleServer
            // 
            resources.ApplyResources(this.titleServer, "titleServer");
            this.titleServer.Name = "titleServer";
            // 
            // titleUser
            // 
            resources.ApplyResources(this.titleUser, "titleUser");
            this.titleUser.Name = "titleUser";
            // 
            // domainListComboBox
            // 
            this.domainListComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.domainListComboBox.FormattingEnabled = true;
            resources.ApplyResources(this.domainListComboBox, "domainListComboBox");
            this.domainListComboBox.Name = "domainListComboBox";
            this.domainListComboBox.SelectedIndexChanged += new System.EventHandler(this.serverListComboBox_SelectedIndexChanged);
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.DarkGray;
            this.tableLayoutPanel1.Controls.Add(this.titleName, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.titleOwner, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.titleRemainingToSync, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.titleLastSyncTime, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.titleAutomaticSync, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.titleAccess, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.valueName, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.valueAccess, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.valueRemainingToSync, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.valueLastSyncTime, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.valueAutomaticSync, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.titleEncrypted, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.valueEncrypted, 3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // titleName
            // 
            resources.ApplyResources(this.titleName, "titleName");
            this.titleName.Name = "titleName";
            // 
            // titleOwner
            // 
            resources.ApplyResources(this.titleOwner, "titleOwner");
            this.titleOwner.Name = "titleOwner";
            // 
            // titleRemainingToSync
            // 
            resources.ApplyResources(this.titleRemainingToSync, "titleRemainingToSync");
            this.titleRemainingToSync.Name = "titleRemainingToSync";
            // 
            // titleLastSyncTime
            // 
            resources.ApplyResources(this.titleLastSyncTime, "titleLastSyncTime");
            this.titleLastSyncTime.Name = "titleLastSyncTime";
            // 
            // titleAutomaticSync
            // 
            resources.ApplyResources(this.titleAutomaticSync, "titleAutomaticSync");
            this.titleAutomaticSync.Name = "titleAutomaticSync";
            // 
            // titleAccess
            // 
            resources.ApplyResources(this.titleAccess, "titleAccess");
            this.titleAccess.Name = "titleAccess";
            // 
            // valueName
            // 
            resources.ApplyResources(this.valueName, "valueName");
            this.valueName.Name = "valueName";
            // 
            // valueAccess
            // 
            resources.ApplyResources(this.valueAccess, "valueAccess");
            this.valueAccess.Name = "valueAccess";
            // 
            // valueRemainingToSync
            // 
            resources.ApplyResources(this.valueRemainingToSync, "valueRemainingToSync");
            this.valueRemainingToSync.Name = "valueRemainingToSync";
            // 
            // valueLastSyncTime
            // 
            resources.ApplyResources(this.valueLastSyncTime, "valueLastSyncTime");
            this.valueLastSyncTime.Name = "valueLastSyncTime";
            // 
            // valueAutomaticSync
            // 
            resources.ApplyResources(this.valueAutomaticSync, "valueAutomaticSync");
            this.valueAutomaticSync.Name = "valueAutomaticSync";
            // 
            // titleEncrypted
            // 
            resources.ApplyResources(this.titleEncrypted, "titleEncrypted");
            this.titleEncrypted.Name = "titleEncrypted";
            // 
            // valueEncrypted
            // 
            resources.ApplyResources(this.valueEncrypted, "valueEncrypted");
            this.valueEncrypted.Name = "valueEncrypted";
            // 
            // directorySearcher1
            // 
            this.directorySearcher1.ClientTimeout = System.TimeSpan.Parse("-00:00:01");
            this.directorySearcher1.ServerPageTimeLimit = System.TimeSpan.Parse("-00:00:01");
            this.directorySearcher1.ServerTimeLimit = System.TimeSpan.Parse("-00:00:01");
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.BackColor = System.Drawing.Color.LightGray;
            this.panel2.ContextMenuStrip = this.iFolderContextMenu;
            this.panel2.Controls.Add(this.iFolderView);
            this.panel2.Controls.Add(this.localiFoldersHeading);
            this.panel2.Name = "panel2";
            this.panel2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseDown);
            // 
            // localiFoldersHeading
            // 
            resources.ApplyResources(this.localiFoldersHeading, "localiFoldersHeading");
            this.localiFoldersHeading.BackColor = System.Drawing.Color.LightGray;
            this.localiFoldersHeading.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.localiFoldersHeading.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.localiFoldersHeading.ForeColor = System.Drawing.SystemColors.Desktop;
            this.localiFoldersHeading.Name = "localiFoldersHeading";
            this.localiFoldersHeading.ReadOnly = true;
            this.localiFoldersHeading.TabStop = false;
            // 
            // iFolderView
            // 
            resources.ApplyResources(this.iFolderView, "iFolderView");
            this.iFolderView.BackColor = System.Drawing.Color.White;
            this.iFolderView.ContextMenuStrip = this.iFolderContextMenu;
            this.iFolderView.HorizontalSpacing = 5;
            this.iFolderView.ItemHeight = 72;
            this.iFolderView.ItemWidth = 280;
            this.iFolderView.LargeImageList = null;
            this.iFolderView.Name = "iFolderView";
            this.iFolderView.SelectedItem = null;
            this.iFolderView.VerticleSpacing = 5;
            this.iFolderView.DoubleClick += new System.EventHandler(this.iFolderView_DoubleClick);
            this.iFolderView.NavigateItem += new Novell.FormsTrayApp.TileListView.NavigateItemDelegate(this.iFolderView_NavigateItem);
            this.iFolderView.SelectedIndexChanged += new Novell.FormsTrayApp.TileListView.SelectedIndexChangedDelegate(this.ifListView_SelectedIndexChanged);
            // 
            // GlobalProperties
            // 
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.LightGray;
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.toolStripiFolderActions);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.groupBox1);
            this.KeyPreview = true;
            this.Menu = this.mainMenu1;
            this.Name = "GlobalProperties";
            this.Load += new System.EventHandler(this.GlobalProperties_Load);
            this.SizeChanged += new System.EventHandler(this.GlobalProperties_SizeChanged);
            this.VisibleChanged += new System.EventHandler(this.GlobalProperties_VisibleChanged);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.GlobalProperties_Closing);
            this.Move += new System.EventHandler(this.GlobalProperties_Move);
            this.Resize += new System.EventHandler(this.GlobalProperties_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GlobalProperties_KeyDown);
            this.toolStripiFolderActions.ResumeLayout(false);
            this.toolStripiFolderActions.PerformLayout();
            this.iFolderContextMenu.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
		#endregion

		protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private MainMenu mainMenu1;
        private GroupBox groupBox1;
        private MenuItem menuActionAccept;
        private MenuItem menuAction;
        private MenuItem menuActionOpen;
        private MenuItem menuActionCreate;
        private MenuItem menuActionMerge;
        private MenuItem menuItem4;
        private MenuItem menuItem7;
        private MenuItem menuActionShare;
        private MenuItem menuActionResolve;
        private MenuItem menuActionSync;
        private MenuItem menuActionRevert;
        private MenuItem menuActionRemove;
        private MenuItem menuActionProperties;
        private MenuItem menuSeparator;
        private MenuItem menuEdit;
        private MenuItem menuViewAccounts;
        private MenuItem menuEditPrefs;
        private MenuItem menuView;
        private MenuItem menuViewRefresh;
        private MenuItem menuItem1;
        private MenuItem menuViewLog;
        private MenuItem menuItem3;
        private MenuItem menuViewAvailable;
        private MenuItem menuSecurity;
        private MenuItem menuRecoverKeys;
        private MenuItem menuExportKeys;
        private MenuItem menuImportKeys;
        private MenuItem menuResetPassphrase;
        private MenuItem menuResetPassword;
        private MenuItem menuHelp;
        private MenuItem menuHelpHelp;
        private MenuItem menuHelpAbout;
        private ToolStrip toolStripiFolderActions;
        private ToolStripButton toolStripBtnCreate;
        private ToolStripButton toolStripBtnDownload;
        private ToolStripButton toolStripBtnSyncNow;
        private ToolStripButton toolStripBtnShare;
        private ToolStripComboBox toolStripBtnFilter;
        private ToolStripDropDownButton toolStipBtnChangeView;
        private ToolStripButton toolStripBtnResolve;
        private ToolStripButton toolStripBtnMerge;


        private System.Windows.Forms.MenuItem migrationMenuItem;
        private System.Windows.Forms.MenuItem migrationMenuSubItem;
        private System.Windows.Forms.MenuItem menuActionClose;
        private System.Windows.Forms.MenuItem menuActionExit;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.StatusBar statusBar1;
        private System.Windows.Forms.MenuItem menuClose;


        private System.Windows.Forms.MenuItem MigrationMenuItem;
        private System.Windows.Forms.MenuItem MigrationMenuSubItem;
        private Panel panel3;
        private TextBox filter;
        private ContextMenuStrip iFolderContextMenu;
        private ToolStripMenuItem CtxMenuOpen;
        private ToolStripSeparator MenuResolveSeperator;
        private ToolStripMenuItem MenuResolve;
        private ToolStripMenuItem MenuAccept;
        private ToolStripMenuItem MenuMerge;
        private ToolStripSeparator menuSeparator1;
        private ToolStripMenuItem MenuRemove;
        private ToolStripMenuItem menuSyncNow;
        private ToolStripMenuItem menuShare;
        private ToolStripMenuItem MenuRevert;
        private ToolStripSeparator menuSeparator2;
        private ToolStripMenuItem menuProperties;
        private ToolStripMenuItem MenuRefresh;
        private System.DirectoryServices.DirectorySearcher directorySearcher1;
        private TableLayoutPanel tableLayoutPanel1;
        private Label titleName;
        private Label titleOwner;
        private Label titleEncrypted;
        private Label titleRemainingToSync;
        private Label titleLastSyncTime;
        private Label titleAutomaticSync;
        private Label titleAccess;
        private Panel panel2;
        private TileListView iFolderView;
        private RichTextBox localiFoldersHeading;
        private ToolStripMenuItem toolStripMenuThumbnails;
        private ToolStripMenuItem toolStripMenuDetails;
        private ToolStripMenuItem toolStripMenuLeftPane;
        private Label valueName;
        private Label valueAccess;
        private Label valueRemainingToSync;
        private Label valueEncrypted;
        private Label valueLastSyncTime;
        private Label valueAutomaticSync;
        private ComboBox domainListComboBox;
        private GroupBox groupBox2;
        private Label titleDiskQuota;
        private Label titleNOFolders;
        private Label titleServer;
        private Label titleUser;
        private Label titleAvailable;
        private Label titleUsed;
        private Label label1;
        private PictureBox pictureBox1;
        private Button LoginLogoff;

	}
}
