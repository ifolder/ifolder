/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Bruce Getter <bgetter@novell.com>
*                 $Modified by: Satyam <ssutapalli@novell.com> 16-05-2008 AddToAcceptedFolders functionality added to show iFolders from AutoAccount creation
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Threading;
using Microsoft.Win32;
using Novell.iFolderCom;
using Novell.Win32Util;
using Simias.Client;
using Simias.Client.Event;
using Novell.iFolder.Web;
using System.Reflection;
using TrayApp.Properties;
using Simias.Client.Authentication;
using Novell.Wizard;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for GlobalProperties.
	/// </summary>
	public partial class GlobalProperties : System.Windows.Forms.Form
	{
		#region Class Members
        private const string myiFoldersX = "MyiFoldersX";
        private const string myiFoldersY = "MyiFoldersY";

        // Delegates used to marshal back to the control's creation thread.
        private delegate void SyncCollectionDelegate(CollectionSyncEventArgs syncEventArgs);
        private SyncCollectionDelegate syncCollectionDelegate;
        private delegate void SyncFileDelegate(FileSyncEventArgs syncEventArgs);
        private SyncFileDelegate syncFileDelegate;

        public delegate void AddDomainToListDelegate(DomainInformation domainInfo);
        public AddDomainToListDelegate addDomainToListDelegate;

        //Refresh Delegate to ensure other operations happen during refresh time.
        public delegate void RefreshiFoldersDelegate();
        public RefreshiFoldersDelegate refreshiFoldersDelegate;

        /// <summary>
        /// Delegate for node create and change events.
        /// </summary>
        public delegate void CreateChangeEventDelegate(iFolderWeb ifolder, string eventData);
        /// <summary>
        /// Delegate used to service node create and change events.
        /// </summary>
        public CreateChangeEventDelegate createChangeEventDelegate;

        /// <summary>
        /// Delegate for node delete events.
        /// </summary>
        public delegate void DeleteEventDelegate(NodeEventArgs args);
        /// <summary>
        /// Delegate used to service node delete events.
        /// </summary>
        public DeleteEventDelegate deleteEventDelegate;

        /// <summary>
        /// Delegate used when a domain account is removed.
        /// </summary>
        public delegate void RemoveDomainDelegate(object sender, DomainRemoveEventArgs e);
        /// <summary>
        /// Occurs when a domain account is removed.
        /// </summary>
        public event RemoveDomainDelegate RemoveDomain;

        private Hashtable iFolderListViews = new Hashtable();
        private Hashtable acceptedFolders = new Hashtable();
        private ImageList largeImageList;
        private ImageList smallImageList;
        private ImageList largeMenuImageList;        
        private TileListViewItem selectedItem;
        private bool hide = true;
        private NoiFolderMessage infoMessage;
        private int minWidth;
        // Refresh thread to get the latest catalog entries
        private Thread refreshThread;
        private bool inRefresh = false;

        System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(GlobalProperties));
        private Preferences preferences;
        private SyncLog syncLog;
        private iFolderWeb[] ifolderArray;
        private System.Timers.Timer updateEnterpriseTimer;
        private Hashtable ht;
        private uint objectsToSync;
        private bool startSync;
        private iFolderWebService ifWebService;
        private SimiasWebService simiasWebService;
        private IProcEventClient eventClient;
        private bool initialConnect = false;
        private bool shutdown = false;
        private bool initialPositionSet = false;
        private DomainInformation defaultDomainInfo = null;
        private DomainInformation selectedDomain = null;
        private Manager simiasManager;
        private string domainList;

        private System.ComponentModel.IContainer components;

        public  string DownloadPath;
        private MyMessageBox fileExitDlg;

        private System.Windows.Forms.Timer searchTimer;
        private System.Windows.Forms.Timer refreshTimer;
        private const double megaByte = 1048576;
        private Domain Currentdomain = null;
        private int comboBoxSelectedIndex = -1;
        private bool thumbnailView = false;
        private string selectediFolderID = null ;
        private string selectedDomainID = null;
        private bool IsDomainMaster = false;

        private enum Index
        {
            B=0,
            KB=1,
            MB=2,
            GB=3
        }

        #endregion

        public Manager simManager
        {
            set
            {
                this.simiasManager = value;
            }
        }
        public void PluginEnhancedMenu()
        {
            //Image Menu Items plugin
            System.Object[] args = new System.Object[0];
            System.Object[] param = new System.Object[1];
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(GlobalProperties));
            try
            { 
                //load the plugin Dll from plugind lib
                Assembly enhancedMenu = Assembly.LoadFrom(Path.Combine(Application.StartupPath,@"lib\plugins\EnhancedMenuItems.dll"));
                if (enhancedMenu != null)// If the dll not there revert to old menu.
                {
                    Type type = enhancedMenu.GetType("EnhancedMenuItems.IconMenuItems");
                    if (null!=type)
                    {
                        System.Object enhancedMenuItemCreator = Activator.CreateInstance(type, args);
                        MethodInfo method = type.GetMethod("CreateMenu");
                        param[0] = menuAction;
                        bool status = (bool)method.Invoke(enhancedMenuItemCreator, param);

                        /*associate the corresponding event handlers*/
                        this.menuActionOpen.Visible = false; //Hide the old menu item
                        this.menuActionOpen = menuAction.MenuItems.Find("iMenuActionOpen", true)[0];
                        this.menuActionOpen.Click += new System.EventHandler(this.menuOpen_Click);
                       
                        this.menuActionCreate.Visible = false;
                        this.menuActionCreate = menuAction.MenuItems.Find("iMenuActionCreate", true)[0];
                        this.menuActionCreate.Click += new System.EventHandler(this.menuCreate_Click);

                        this.menuActionRemove.Visible = false;
                        this.menuActionRemove = menuAction.MenuItems.Find("iMenuActionRemove", true)[0];
                        this.menuActionRemove.Click += new System.EventHandler(this.menuRemove_Click);

                        this.menuActionMerge.Visible = false;
                        this.menuActionMerge = menuAction.MenuItems.Find("iMenuActionMerge", true)[0];
                        this.menuActionMerge.Click += new System.EventHandler(this.menuMerge_Click);

                        this.menuActionAccept.Visible = false;
                        this.menuActionAccept = menuAction.MenuItems.Find("iMenuActionAccept", true)[0];
                        this.menuActionAccept.Click += new System.EventHandler(this.menuAccept_Click);

                        this.menuActionShare.Visible = false;
                        this.menuActionShare = menuAction.MenuItems.Find("iMenuActionShare", true)[0];
                        this.menuActionShare.Click += new System.EventHandler(this.menuShare_Click);

                        this.menuActionResolve.Visible = false;
                        this.menuActionResolve = menuAction.MenuItems.Find("iMenuActionResolve", true)[0];
                        this.menuActionResolve.Click += new System.EventHandler(this.menuResolve_Click);

                        this.menuActionSync.Visible = false;
                        this.menuActionSync = menuAction.MenuItems.Find("iMenuActionSync", true)[0];
                        this.menuActionSync.Click += new System.EventHandler(this.menuSyncNow_Click);

                        this.menuActionRevert.Visible = false;
                        this.menuActionRevert = menuAction.MenuItems.Find("iMenuActionRevert", true)[0];
                        this.menuActionRevert.Click += new System.EventHandler(this.menuRevert_Click);

                        this.menuActionProperties.Visible = false;
                        this.menuActionProperties = menuAction.MenuItems.Find("iMenuActionProperties", true)[0];
                        this.menuActionProperties.Click += new System.EventHandler(this.menuProperties_Click);

                        this.menuAction.MenuItems.Remove(menuItem4);
                        this.menuAction.MenuItems.Add(menuItem4);

                        this.menuAction.MenuItems.Remove(migrationMenuItem);
                        this.menuAction.MenuItems.Add(migrationMenuItem);

                        this.menuAction.MenuItems.Remove(menuSeparator);
                        this.menuAction.MenuItems.Add(menuSeparator);

                        this.menuAction.MenuItems.Remove(menuActionClose);
                        this.menuAction.MenuItems.Add(menuActionClose);

                        this.menuAction.MenuItems.Remove(menuActionExit); ;
                        this.menuAction.MenuItems.Add(menuActionExit);
                   
                        /*Help Menu Items*/
                        enhancedMenuItemCreator = Activator.CreateInstance(type, args);
                        method = type.GetMethod("CreateMenu");
                        param[0] = menuHelp;
                        status = (bool)method.Invoke(enhancedMenuItemCreator, param);
                        
                        this.menuHelpHelp.Visible = false;
                        this.menuHelpHelp = menuHelp.MenuItems.Find("iMenuHelpHelp", true)[0];
                        this.menuHelpHelp.Click += new System.EventHandler(this.menuHelpHelp_Click);

                        this.menuHelpUpgrade.Visible = false;
                        this.menuHelpUpgrade = menuHelp.MenuItems.Find("iMenuHelpUpgrade",true)[0];
                        this.menuHelpUpgrade.Click += new System.EventHandler(this.menuHelpUpgrade_Click);

                        this.menuHelpAbout.Visible = false;
                        this.menuHelpAbout = menuHelp.MenuItems.Find("iMenuHelpAbout", true)[0];
                        this.menuHelpAbout.Click += new System.EventHandler(this.menuHelpAbout_Click);

                        /*Edit Menu Items*/
                        enhancedMenuItemCreator = Activator.CreateInstance(type, args);
                        method = type.GetMethod("CreateMenu");
                        param[0] = menuEdit;
                        status = (bool)method.Invoke(enhancedMenuItemCreator, param);

                        this.menuViewAccounts.Visible = false;
                        this.menuViewAccounts = menuEdit.MenuItems.Find("iMenuViewAccounts", true)[0];
                        this.menuViewAccounts.Click += new System.EventHandler(this.menuViewAccounts_Click);

                        this.menuEditPrefs.Visible = false;
                        this.menuEditPrefs = menuEdit.MenuItems.Find("iMenuEditPrefs", true)[0];
                        this.menuEditPrefs.Click += new System.EventHandler(this.menuEditPrefs_Click);

                        /*View Menu items*/
                        enhancedMenuItemCreator = Activator.CreateInstance(type, args);
                        method = type.GetMethod("CreateMenu");
                        param[0] = menuView;
                        status = (bool)method.Invoke(enhancedMenuItemCreator, param);

                        this.menuViewRefresh.Visible = false;
                        this.menuViewRefresh = menuView.MenuItems.Find("iMenuViewRefresh", true)[0];
                        this.menuViewRefresh.Click += new System.EventHandler(this.menuRefresh_Click);
                     
                        this.menuViewLog.Visible = false;
                        this.menuViewLog = menuView.MenuItems.Find("iMenuViewLog", true)[0];
                        this.menuViewLog.Click += new System.EventHandler(this.menuViewLog_Click);

                        this.menuView.MenuItems.Remove(menuItem3);
                        this.menuView.MenuItems.Add(menuItem3);

                        this.menuView.MenuItems.Remove(menuViewAvailable);
                        this.menuView.MenuItems.Add(menuViewAvailable);

                        /*Security Menu Items*/
                        enhancedMenuItemCreator = Activator.CreateInstance(type, args);
                        method = type.GetMethod("CreateMenu");
                        param[0] = menuSecurity;
                        status = (bool)method.Invoke(enhancedMenuItemCreator, param);

                        this.menuRecoverKeys.Visible = false;
                        this.menuRecoverKeys = menuSecurity.MenuItems.Find("iMenuRecoverKeys", true)[0];
                        this.menuRecoverKeys.Click += new System.EventHandler(menuRecoverKeys_Click);

                        this.menuResetPassphrase.Visible = false;
                        this.menuResetPassphrase = menuSecurity.MenuItems.Find("iMenuResetPassphrase", true)[0];
                        this.menuResetPassphrase.Click += new System.EventHandler(menuResetPassphrase_Select);

                        this.menuResetPassword.Visible = false;
                        this.menuResetPassword = menuSecurity.MenuItems.Find("iMenuResetPassword", true)[0];
                        this.menuResetPassword.Click += new System.EventHandler(menuResetPassword_Click);  
                        
                    }
                }
            }
            catch (Exception ex)
            {
                //Ignore
            }
        }

        private void ApplyResources1(ComponentResourceManager resources, MenuItem item, string resourcename)
        {
            /* Some the keys in the resx files, start with ">>" and they are autogenerated.
             * While using applyresources for mapping the attribute values for the controls, 
             * those keys that start with ">>" are not being assigned and some these attributes 
             * are not populated at the time of InitializeComponent. This methos is to populate 
             * those attributes whose keys in resx files start with ">>". If something of the attributes
             * in other widgets are not being populated, check whether the resource files keys and the ones that we are passing
             * through ApplyResources are same.
             * */
            resources.ApplyResources(item, ">>" + resourcename);
        }

        /// <summary>
        /// Instantiates a GlobalProperties object.
        /// </summary>
        public GlobalProperties(iFolderWebService ifolderWebService, SimiasWebService simiasWebService, IProcEventClient eventClient)
        {
            syncCollectionDelegate = new SyncCollectionDelegate(syncCollection);
            syncFileDelegate = new SyncFileDelegate(syncFile);
            createChangeEventDelegate = new CreateChangeEventDelegate(createChangeEvent);
            deleteEventDelegate = new DeleteEventDelegate(deleteEvent);
            addDomainToListDelegate = new AddDomainToListDelegate(AddDomainToList);
            //refreshiFoldersDelegate = new RefreshiFoldersDelegate(refreshiFolders);
            refreshiFoldersDelegate = new RefreshiFoldersDelegate(refreshiFoldersInvoke);
            //
            // Required for Windows Form Designer support
            //
            
            InitializeComponent();
            MoreInitialization();
            PluginEnhancedMenu(); // Upgrade to Image Menu Item.

            infoMessage = new NoiFolderMessage();
            panel2.Controls.Add(infoMessage);
            progressBar1.Visible = false;

            ifWebService = ifolderWebService;
            this.simiasWebService = simiasWebService;
            this.eventClient = eventClient;

            // Set up the event handlers for sync events ... these need to be active here so that sync events can
            // be written to the log listbox.
            eventClient.SetEvent(IProcEventAction.AddCollectionSync, new IProcEventHandler(global_collectionSyncHandler));
            eventClient.SetEvent(IProcEventAction.AddFileSync, new IProcEventHandler(global_fileSyncHandler));

            updateEnterpriseTimer = new System.Timers.Timer(1000);
            updateEnterpriseTimer.AutoReset = false;
            updateEnterpriseTimer.Elapsed += new System.Timers.ElapsedEventHandler(updateEnterpriseTimer_Elapsed);
            ht = new Hashtable();

            progressBar1.Minimum = 0;
            
            this.StartPosition = FormStartPosition.CenterScreen;

            // Load the application icon
            try
            {
                this.Icon = new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_16.ico"));

                // TODO: update icons.
                largeImageList = new ImageList();
                largeImageList.ImageSize = new Size(48, 48);
                largeImageList.ColorDepth = ColorDepth.Depth32Bit;
                largeImageList.TransparentColor = Color.Black;
                largeImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder48.png")));
                largeImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-sync48.png")));
                largeImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-download48.png")));
                largeImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-upload48.png")));
                largeImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-waiting48.png")));
                largeImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-conflict48.png")));
                largeImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-error48.png")));
                largeImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\encrypt_ilock_48.gif")));
                largeImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder_user_48.png")));
                largeImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-warning48.png")));
                iFolderView.LargeImageList = largeImageList;

                // TODO: load hot icons.
            }
            catch { } // Non-fatal ... just missing some graphics.
            // Load the application icon



            //TODO: commented out, need to figure why is this giving error. 
            //this.MinimumSize = this.Size;
        }
        void MoreInitialization()
        {           
            this.fileExitDlg = new MyMessageBox(TrayApp.Properties.Resources.exitMessage, 
                TrayApp.Properties.Resources.exitTitle, 
                string.Empty, 
                MyMessageBoxButtons.YesNo, 
                MyMessageBoxIcon.Question);

            try
            {
                smallImageList = new ImageList();
                smallImageList.ImageSize = new Size(32, 32);
                smallImageList.ColorDepth = ColorDepth.Depth32Bit;
                smallImageList.TransparentColor = Color.Black;
                smallImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\newifolder32.png")));
                smallImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-sync32.png")));
                smallImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-download32.png")));
                smallImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-upload32.png")));
                smallImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-waiting32.png")));
                smallImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-conflict32.png")));
                smallImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-error32.png")));
                smallImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\encrypt-ilock32.png")));
                smallImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder_user_32.png")));
                smallImageList.Images.Add(Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-warning32.png")));


                toolStripBtnCreate.Image = Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder48.png"));
                toolStripBtnDelete.Image = Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\delete_48.png"));
                toolStripBtnDownload.Image = Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-download48.png"));
                toolStripBtnMerge.Image = Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\merge48.png"));
                toolStripBtnResolve.Image = Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-conflict48.png"));
                toolStripBtnRevert.Image = Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\revert48.png"));
                toolStripBtnShare.Image = Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\share48.png"));
                toolStripBtnSyncNow.Image = Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-sync48.png"));
                toolStipBtnChangeView.Image = Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\change_view_48.png"));
            }
            catch { }
            listView1.LargeImageList = largeImageList;
            listView1.SmallImageList = smallImageList;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GlobalProperties));
            this.ApplyResources1(resources, this.menuAction, "menuAction");
            this.ApplyResources1(resources, this.menuActionCreate, "menuActionCreate");
            this.ApplyResources1(resources, this.menuActionAccept, "menuActionAccept");
            this.ApplyResources1(resources, this.menuActionMerge, "menuActionMerge");
            this.ApplyResources1(resources, this.menuActionRemove, "menuActionRemove");
            this.ApplyResources1(resources, this.menuItem7, "menuItem7");
            this.ApplyResources1(resources, this.menuActionOpen, "menuActionOpen");
            this.ApplyResources1(resources, this.menuActionShare, "menuActionShare");
            this.ApplyResources1(resources, this.menuActionResolve, "menuActionResolve");
            this.ApplyResources1(resources, this.menuActionSync, "menuActionSync");
            this.ApplyResources1(resources, this.menuActionRevert, "menuActionRevert");
            this.ApplyResources1(resources, this.menuActionProperties, "menuActionProperties");
            this.ApplyResources1(resources, this.menuItem4, "menuItem4");
            this.ApplyResources1(resources, this.migrationMenuItem, "migrationMenuItem");
            this.ApplyResources1(resources, this.migrationMenuSubItem, "migrationMenuSubItem");
            this.ApplyResources1(resources, this.menuSeparator, "menuSeparator");
            this.ApplyResources1(resources, this.menuActionClose, "menuActionClose");
            this.ApplyResources1(resources, this.menuActionExit, "menuActionExit");
            this.ApplyResources1(resources, this.menuEdit, "menuEdit");
            this.ApplyResources1(resources, this.menuViewAccounts, "menuViewAccounts");
            this.ApplyResources1(resources, this.menuEditPrefs, "menuEditPrefs");
            this.ApplyResources1(resources, this.menuView, "menuView");
            this.ApplyResources1(resources, this.menuViewRefresh, "menuViewRefresh");
            this.ApplyResources1(resources, this.menuItem1, "menuItem1");
            this.ApplyResources1(resources, this.menuViewLog, "menuViewLog");
            this.ApplyResources1(resources, this.menuItem3, "menuItem3");
            this.ApplyResources1(resources, this.menuViewAvailable, "menuViewAvailable");
            this.ApplyResources1(resources, this.menuSecurity, "menuSecurity");
            this.ApplyResources1(resources, this.menuRecoverKeys, "menuRecoverKeys");
            this.ApplyResources1(resources, this.menuResetPassphrase, "menuResetPassphrase");
            this.ApplyResources1(resources, this.menuResetPassword, "menuResetPassword");
            this.ApplyResources1(resources, this.menuHelp, "menuHelp");
            this.ApplyResources1(resources, this.menuHelpHelp, "menuHelpHelp");
            this.ApplyResources1(resources, this.menuHelpUpgrade, "menuHelpUpgrade");
            this.ApplyResources1(resources, this.menuHelpAbout, "menuHelpAbout");
            
        }

        void menuResetPassword_Click(object sender, EventArgs e)
        {
            // Show the reset passphrase window
            ResetPassword resetPasswordWindow = new ResetPassword(this.simiasWebService, this.ifWebService);
            if (resetPasswordWindow.DomainCount > 0)
                resetPasswordWindow.ShowDialog();
            else
            {
                System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(FormsTrayApp));
                Novell.iFolderCom.MyMessageBox mmb = 
                    new MyMessageBox(Resource.GetString("NoLoggedInDomainsPasswordText"),
                        Resource.GetString("ResetPasswordError"),
                        "", 
                        MyMessageBoxButtons.OK, 
                        MyMessageBoxIcon.Error);
                mmb.ShowDialog();
                mmb.Dispose();
            }
        }

		#region Properties
		/// <summary>
		/// Set when initially connecting to enterprise so that the Servers tab can be updated after the first sync cycle.
		/// </summary>
		public bool InitialConnect
		{
			set { initialConnect = value; }
		}

		/// <summary>
		/// Set the Preferences dialog.
		/// </summary>
		public Preferences PreferenceDialog
		{
            get { return preferences; }
			set { preferences = value; }
		}

		/// <summary>
		/// Set the SyncLog dialog.
		/// </summary>
		public SyncLog SyncLogDialog
		{
			set { syncLog = value; }
		}

        public iFolderWebService iFWebService
        {
            set
            {
                this.ifWebService = value;
            }
        }

        public SimiasWebService Simws
        {
            set
            {
                this.simiasWebService = value;
            }
        }

        public IProcEventClient EventClient
        {
            set
            {
                this.eventClient = value;
            }
        }
        #endregion

		#region Public Methods
        public bool AcceptiFolder(iFolderWeb ifolder, out bool added)
        {
            return AcceptiFolder(ifolder, out added, false);
        }
        public bool AcceptiFolder(iFolderWeb ifolder, out bool added, bool mergeFolder)
        {
			bool result = false;
            added = false;
            if (ifolder.MigratediFolder > 0)
            {
                if (MigrationWindow.OldiFoldersPresent() == true)
                {
                    // Prompt for the option here...
                    System.Resources.ResourceManager resManager = new System.Resources.ResourceManager(typeof(FormsTrayApp));
                    MyMessageBox mmb = new MyMessageBox(resManager.GetString("MigrationMergePrompt.Text"), resManager.GetString("MigrationAlert"), "", MyMessageBoxButtons.OKCancel, MyMessageBoxIcon.Question);
                    DialogResult res = mmb.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        /// Check if local 2.x iFolders are present and then try to merge with them...
                        Novell.FormsTrayApp.MigrationWindow migrationWindow = new MigrationWindow(this.ifWebService, this.simiasWebService);
                        migrationWindow.Merge = true;
                        migrationWindow.iFolderName = ifolder.Name;
                        migrationWindow.ShowDialog();
                        string loc = migrationWindow.iFolderLocation;
                        string uName = migrationWindow.UserName;
                        if (loc == null || uName == null)
                        {
                            // Cancel clicked...
                            return false;
                        }
                        else
                        {
                            // merge the folder selected with the current iFolder... 
                            result = acceptiFolder(ifolder, loc, out added, true);
                            if (result == true)
                            {
                                MigrationWindow.RemoveRegistryForUser(uName);
                            }
                        }
                        return result;
                    }
                }                
            }
			string selectedPath = string.Empty;			
			FolderBrowserDialog browserDialog = new FolderBrowserDialog();
			Cursor.Current = Cursors.WaitCursor;
			while (true)
			{
				browserDialog.ShowNewFolderButton = true;
				browserDialog.SelectedPath = selectedPath;
                if (!mergeFolder)
                {
                    browserDialog.Description = string.Format(TrayApp.Properties.Resources.acceptDescription, ifolder.Name);
                }
                else
                {
                    browserDialog.Description = string.Format(TrayApp.Properties.Resources.mergeDescription, ifolder.Name);
                }
				DialogResult dialogResult = browserDialog.ShowDialog();
				if ( dialogResult == DialogResult.OK )
				{
					browserDialog.Dispose();
					//ensure UI is re-painted
					Invalidate();
					Update();
                    result = acceptiFolder(ifolder, browserDialog.SelectedPath, out added, mergeFolder);
                    if (result && !added)
                    {
                        break;
                    }
					else if ( result )
					{
                        if (!mergeFolder)
                            DownloadPath = browserDialog.SelectedPath + ifolder.Name;
                        else
                            DownloadPath = browserDialog.SelectedPath;
						break;
					}
				}
				else
				{
					browserDialog.Dispose();
					break;
				}
			}
			Cursor.Current = Cursors.Default;
            refreshAll(); //TODO: check if this is too heavy operation
			return result;
		}

        public iFoldersListView AddDomainToUIList(DomainInformation domainInfo)
        {
            lock (iFolderListViews)
            {
                iFoldersListView ifListView = (iFoldersListView)iFolderListViews[domainInfo.ID];
                if (ifListView == null)
                {
                    // Add the domain.
                    ifListView = new iFoldersListView(domainInfo, largeImageList);
                    ifListView.SelectedIndexChanged += new Novell.FormsTrayApp.iFoldersListView.SelectedIndexChangedDelegate(ifListView_SelectedIndexChanged);
                    ifListView.DoubleClick += new EventHandler(iFolderView_DoubleClick);
                    ifListView.NavigateItem += new Novell.FormsTrayApp.iFoldersListView.NavigateItemDelegate(iFolderView_NavigateItem);

                    iFolderListViews.Add(domainInfo.ID, ifListView);

                    updateWidth();
                    ifListView.Visible = !hide;
                    panel2.Controls.Add(ifListView);
                    updateView();
                }
                return ifListView;
            }
        }

		/// <summary>
		/// Adds the specified domain to the dropdown list.
		/// </summary>
		/// <param name="domainInfo">The DomainInformation object to add to the list.</param>
		public void AddDomainToList(DomainInformation domainInfo)
		{
			// Reset the current default domain if the added domain is set to be the default.
			if (domainInfo.IsDefault)
			{
				if ((defaultDomainInfo != null) && !defaultDomainInfo.ID.Equals(domainInfo.ID))
				{
					defaultDomainInfo.IsDefault = false;
				}

				// Keep track of the default domain.
				defaultDomainInfo = domainInfo;
			}

			// Update the domain list file.
			addDomainToFile(domainInfo);
		}

        public DomainInformation RemoveDomainFromUIList(string domainID, string defaultDomainID)
        {
            DomainInformation domainInfo = null;

            lock (iFolderListViews)
            {
                iFoldersListView ifListView = (iFoldersListView)iFolderListViews[domainID];
                if (ifListView != null)
                {
                    domainInfo = ifListView.DomainInfo;
                    foreach (TileListViewItem tlvi in ifListView.Items)
                    {
                        if (tlvi.Selected)
                        {
                            selectedItem = null;
                            updateMenus(null);
                        }

                        ht.Remove(((iFolderObject)tlvi.Tag).ID);
                    }

                    // Remove the domain.
                    iFolderListViews.Remove(domainID);

                    updateWidth();

                    panel2.Controls.Remove(ifListView);

                    updateView();
                }
                // Reset the default domain.
                if (defaultDomainID != null)
                {
                    ifListView = (iFoldersListView)iFolderListViews[defaultDomainID];
                    if (ifListView != null)
                    {
                        ifListView.DomainInfo.IsDefault = true;
                    }
                }
            }
            return domainInfo;
        }

		/// <summary>
		/// Remove the specified domain from the dropdown list.
		/// </summary>
		/// <param name="domainInfo">The DomainInformation object representing the domain to remove.</param>
		/// <param name="defaultDomainID">The identifier of the default domain.</param>
		public void RemoveDomainFromList(DomainInformation domainInfo, string defaultDomainID)
		{
			RemoveDomainFromList( domainInfo.ID, defaultDomainID );
		}

		/// <summary>
		/// Remove the specified domain from the dropdown list.
		/// </summary>
		/// <param name="domainID">The ID of the domain to remove.</param>
		public void RemoveDomainFromList(string domainID)
		{
			RemoveDomainFromList( domainID, simiasWebService.GetDefaultDomainID() );
		}

		public void RemoveDomainFromList( string domainID, string defaultDomainID )
		{
			try
			{
				if (RemoveDomain != null)
				{
					if (this.simiasWebService.GetRememberOption(domainID) == false)
						this.simiasWebService.StorePassPhrase(domainID, "", CredentialType.None, false);
				}
			}
			catch{}
			
			DomainInformation domainInfo = RemoveDomainFromUIList(domainID, defaultDomainID);

			if (domainInfo != null)
			{
				// Update the domain list file.
				removeDomainFromFile(domainInfo, defaultDomainID);

				if (RemoveDomain != null)
				{
				//	if (this.simiasWebService.GetRememberOption(domainInfo.ID) == false)
				//		this.simiasWebService.StorePassPhrase(domainInfo.ID, "", CredentialType.None, false);
					RemoveDomain(this, new DomainRemoveEventArgs(domainInfo, defaultDomainID));
				}
			}
            
		}

		/// <summary>
		/// Intializes the list of servers displayed in the dropdown list.
		/// </summary>
		public void InitializeServerList()
		{
			// Initialize the domain list file.
			domainList = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
				"domain.list");

			XmlDocument domainsDoc = new XmlDocument();

			try
			{
				if (!File.Exists(domainList))
				{
					domainsDoc.LoadXml("<domains></domains>");
				}
				else
				{
					// Load the domain list file and clear it out.
					domainsDoc.Load(domainList);
					XmlNode node = domainsDoc.SelectSingleNode("/domains");
					if (node == null)
					{
						XmlElement element = domainsDoc.CreateElement("domains");
						domainsDoc.AppendChild(element);
					}
					else
					{
						node.RemoveAll();
					}
				}
			}
			catch
			{
				domainsDoc.LoadXml("<domains></domains>");
			}

			saveXmlFile(domainsDoc);
		}

		/// <summary>
		/// Check the specified ID to see if it is the current user.
		/// </summary>
		/// <param name="userID">The ID of the user to check.</param>
		/// <returns><b>True</b> if the specified user ID is the current user; otherwise, <b>False</b>.</returns>
		public bool IsCurrentUser(string userID)
		{
			bool result = false;

/*			foreach (Domain d in servers.Items)
			{
				if (!d.ShowAll && d.DomainInfo.MemberUserID.Equals(userID))
				{
					result = true;
					break;
				}
			}*/

			return result;
		}

		/// <summary>
		/// Check the specified ID to see if it is a POBox that is registered with the client.
		/// </summary>
		/// <param name="poBoxID">The ID of the POBox to check.</param>
		/// <returns><b>True</b> if the specified POBox ID is registered with the client; otherwise, <b>False</b>.</returns>
		public bool IsPOBox(string poBoxID)
		{
			bool result = false;

/*			foreach (Domain d in servers.Items)
			{
				if (!d.ShowAll && d.DomainInfo.POBoxID.Equals(poBoxID))
				{
					result = true;
					break;
				}
			}*/

			return result;
		}
		
		/// <summary>
		/// Gets if Shutdown or logoff message is received.
		/// </summary>
		public bool MachineShutdown()
		{
			return this.shutdown;
		}
	
		/// <summary>
		/// Updates the display whenever an iFolder is downloaded.
		/// </summary>
		/// <param name="iFolderWeb">The Web object for the downloaded iFolder.</param>
		/// <param name="DownloadPath">The path the downloaded iFolder.</param>
		public void UpdateDisplay( iFolderWeb ifolderWeb, string DownloadPath)
		{
			iFolderObject ifobj = new iFolderObject(ifolderWeb, iFolderState.Initial);
			addiFolderToListView(ifobj);
			if( acceptedFolders.Contains(ifolderWeb.ID) )
				acceptedFolders.Remove(ifolderWeb.ID);
			ifolderWeb.UnManagedPath = DownloadPath;
			TileListViewItem tlvi = new TileListViewItem(ifobj);
			acceptedFolders.Add(ifolderWeb.ID, tlvi);
            ifobj = null;
            tlvi = null;
		}

        /// <summary>
        /// Adds iFolders created by AutoAccount plugin.
        /// </summary>
        /// <param name="iFolderWeb">Details of the iFolder Web object received from AutoAccount plugin.</param>
        public void AddToAcceptedFolders(iFolderWeb ifWeb)
        {
            iFolderObject ifobj = new iFolderObject(ifWeb, iFolderState.Initial);
            addiFolderToListView(ifobj);
            TileListViewItem tlvi = new TileListViewItem(ifobj);
            acceptedFolders.Add(ifWeb.ID, tlvi);
            ifobj = null;
            tlvi = null;
        }
		/// <summary>
		/// Updates the specified domain.
		/// </summary>
		/// <param name="domainInfo">The DomainInformation object representing the domain to update.</param>
		public void UpdateDomain(DomainInformation domainInfo)
		{
		}
		#endregion

		#region Private Methods
		private void addDomainToFile(DomainInformation domainInfo)
		{
			XmlDocument domainsDoc;

			// Load the domain list file.
			domainsDoc = new XmlDocument();
			domainsDoc.Load(domainList);

			XmlElement element = (XmlElement)domainsDoc.SelectSingleNode("/domains");

			bool found = false;

			// Look for a domain with this ID.
			XmlNodeList nodeList = element.GetElementsByTagName("domain");
			foreach (XmlNode node in nodeList)
			{
				string id = ((XmlElement)node).GetAttribute("ID");
				if (id.Equals(domainInfo.ID))
				{
					// The domain is already in the list.
					found = true;
					break;
				}
			}

			if (!found)
			{
				// Add the domain.

				// Create an element.
				XmlElement domain = domainsDoc.CreateElement("domain");

				// Add the attributes.
				domain.SetAttribute("name", domainInfo.Name);
				domain.SetAttribute("ID", domainInfo.ID);

				// Add the element.
				element.AppendChild(domain);
			}

			// Update the default domain.
			if (domainInfo.IsDefault)
			{
				XmlElement defaultDomainElement = (XmlElement)domainsDoc.SelectSingleNode("/domains/defaultDomain");
				if (defaultDomainElement == null)
				{
					defaultDomainElement = domainsDoc.CreateElement("defaultDomain");
					defaultDomainElement.SetAttribute("ID", domainInfo.ID);
					element.AppendChild(defaultDomainElement);
				}
				else
				{
					string id = defaultDomainElement.GetAttribute("ID");
					if (!id.Equals(domainInfo.ID))
					{
						defaultDomainElement.SetAttribute("ID", domainInfo.ID);
					}
				}
			}

			saveXmlFile(domainsDoc);
		}

		private void removeDomainFromFile(DomainInformation domainInfo, string defaultDomainID)
		{
			XmlDocument domainsDoc;

			// Load the domain list file.
			domainsDoc = new XmlDocument();
			domainsDoc.Load(domainList);

			XmlElement element = (XmlElement)domainsDoc.SelectSingleNode("/domains");


			// Look for a domain with this ID.
			XmlNode domainNode = null;
			XmlNodeList nodeList = element.GetElementsByTagName("domain");
			foreach (XmlNode node in nodeList)
			{
				string id = ((XmlElement)node).GetAttribute("ID");
				if (id.Equals(domainInfo.ID))
				{
					domainNode = node;
					break;
				}
			}

			if (domainNode != null)
			{
				// Remove the domain.
				element.RemoveChild(domainNode);
			}

			// Update the default domain.
			if (defaultDomainID != null)
			{
				element = (XmlElement)domainsDoc.SelectSingleNode("/domains/defaultDomain");
				if (!element.GetAttribute("ID").Equals(defaultDomainID))
				{
					element.SetAttribute("ID", defaultDomainID);
				}
			}

			saveXmlFile(domainsDoc);
		}

		private void removeTileListViewItem( TileListViewItem tlvi )
		{
			if( tlvi == null)
				return;
			if ( tlvi.Equals( selectedItem ) )
			{
				selectedItem = null;
				updateMenus( null );
			}
            ht.Remove(((iFolderObject)tlvi.Tag).ID);
        	tlvi.Remove();
            if (tlvi.iFoldersListView.Items.Count == 0)
            {
                iFolderObject ifolderObj = (iFolderObject)tlvi.Tag;
                iFolderWeb ifolder = ifolderObj.iFolderWeb;
             //   RemoveDomainFromUIList(ifolder.DomainID, null);
                refreshAll();
            }
            if (!thumbnailView)
            {
                showiFolderinListView();
            }
			updateView();
		}

		private void saveXmlFile(XmlDocument doc)
		{
			// Save the config file.
			XmlTextWriter xtw = new XmlTextWriter(domainList, System.Text.Encoding.UTF8);
			try
			{
				xtw.Formatting = Formatting.Indented;

				doc.WriteTo(xtw);
			}
			finally
			{
				xtw.Close();
			}
		}

		private void syncCollection(CollectionSyncEventArgs syncEventArgs)
		{
			try
			{
				progressBar1.Visible = false;

				switch (syncEventArgs.Action)
				{
					case Action.StartLocalSync:
					{
                        if (!syncEventArgs.Name.StartsWith("POBox:"))
						{
							statusBar1.Text = string.Format(TrayApp.Properties.Resources.localSync, syncEventArgs.Name);
							lock (ht)
							{
								TileListViewItem tlvi = (TileListViewItem)ht[syncEventArgs.ID];
								if (tlvi != null)
								{
									iFolderObject ifolderObject = (iFolderObject)tlvi.Tag;
									ifolderObject.iFolderState = iFolderState.SynchronizingLocal;
									int imageIndex;
									tlvi.Status = getItemState( ifolderObject, 0, out imageIndex );
									tlvi.ImageIndex = imageIndex;
                                    if (!thumbnailView)
                                    {
                                        ListViewItem item = listView1.FindItemWithText(ifolderObject.iFolderWeb.ID);
                                        listView1.Items[item.Index].ImageIndex = imageIndex;
                                        //FIXME: remove hardcode value 3
                                        listView1.Items[item.Index].SubItems[3].Text = tlvi.Status;
                                    }
                                    
								}
							}
						}
						break;
					}
					case Action.StartSync:
					{
                        this.MenuRevert.Enabled = false;
                        this.menuActionRevert.Enabled = false;
						if (syncEventArgs.Name.StartsWith("POBox:"))
						{
							statusBar1.Text = TrayApp.Properties.Resources.checkingForiFolders;
						}
						else
						{
							statusBar1.Text = string.Format(TrayApp.Properties.Resources.synciFolder, syncEventArgs.Name);
							lock (ht)
							{
								TileListViewItem tlvi = (TileListViewItem)ht[syncEventArgs.ID];
								if (tlvi != null)
								{
									startSync = true;
									iFolderObject ifolderObject = (iFolderObject)tlvi.Tag;
									ifolderObject.iFolderState = iFolderState.Synchronizing;
									tlvi.ItemLocation = ifolderObject.iFolderWeb.UnManagedPath;
									int imageIndex;
									tlvi.Status = getItemState( ifolderObject, 0, out imageIndex );
									tlvi.ImageIndex = imageIndex;
                                    if (!thumbnailView)
                                    {
                                        ListViewItem item = listView1.FindItemWithText(ifolderObject.iFolderWeb.ID);
                                        listView1.Items[item.Index].ImageIndex = imageIndex;
                                        listView1.Items[item.Index].SubItems[3].Text = tlvi.Status;
                                    }
								}
							}
						}
						break;
					}
                    case Action.NoPassphrase:
					{
						lock(ht)
						{
							TileListViewItem tlvi = (TileListViewItem)ht[syncEventArgs.ID];

							if (tlvi != null)
							{
								iFolderObject ifolderObject = (iFolderObject)tlvi.Tag;

								ifolderObject.iFolderState = iFolderState.NoPassphrase;

                                int imageIndex;
								tlvi.Status = getItemState( ifolderObject, 0, out imageIndex );
								tlvi.ImageIndex = imageIndex;
								tlvi.Tag = ifolderObject;
                                if (!thumbnailView)
                                {
                                    ListViewItem item = listView1.FindItemWithText(ifolderObject.iFolderWeb.ID);
                                    listView1.Items[item.Index].ImageIndex = imageIndex;
                                    listView1.Items[item.Index].SubItems[3].Text = tlvi.Status;
                                }
							}
						}

						statusBar1.Text = resourceManager.GetString("statusBar1.Text");
                        syncLog.AddMessageToLog(DateTime.Now,
                            string.Format("Passphrase not provided, will not sync the folder \"{0}\""));

						if (initialConnect)
						{
							initialConnect = false;
							updateEnterpriseTimer.Start();
						}

                        //turing Revert Option to Active
                        this.MenuRevert.Enabled = true;
                        this.menuActionRevert.Enabled = true;

						break;
					}
					case Action.StopSync:
					{
						lock(ht)
						{
							TileListViewItem tlvi = (TileListViewItem)ht[syncEventArgs.ID];

							if (tlvi != null)
							{
								iFolderObject ifolderObject = (iFolderObject)tlvi.Tag;

								uint objectsToSync2 = 0;
								try
								{
									SyncSize syncSize = ifWebService.CalculateSyncSize(syncEventArgs.ID);
									objectsToSync2 = syncSize.SyncNodeCount;
								}
								catch {}

								if (objectsToSync2 == 0)
								{
									ifolderObject.iFolderState = syncEventArgs.Yielded? iFolderState.Synchronizing: syncEventArgs.Connected ? 
										iFolderState.Normal : 
										iFolderState.Disconnected;
								}
								else
								{
									ifolderObject.iFolderState = iFolderState.FailedSync;
								}

								int imageIndex;
								tlvi.Status = getItemState( ifolderObject, objectsToSync2, out imageIndex );
								tlvi.ImageIndex = imageIndex;
								tlvi.Tag = ifolderObject;
                                if (!thumbnailView)
                                {
                                    ListViewItem item = listView1.FindItemWithText(ifolderObject.iFolderWeb.ID);
                                    listView1.Items[item.Index].ImageIndex = imageIndex;
                                    listView1.Items[item.Index].SubItems[3].Text = tlvi.Status;
                                }
							}

							objectsToSync = 0;
						}

						statusBar1.Text = resourceManager.GetString("statusBar1.Text");
						if (initialConnect)
						{
							initialConnect = false;
							updateEnterpriseTimer.Start();
						}

                        //turing Revert Option to Active
                        this.MenuRevert.Enabled = true;
                        this.menuActionRevert.Enabled = true;


						break;
					}
                    
                    case Action.DisabledSync:
                    {
                        lock (ht)
                        {
                            TileListViewItem tlvi = (TileListViewItem)ht[syncEventArgs.ID];
                            if (tlvi != null)
                            {
                                iFolderObject ifolderObject = (iFolderObject)tlvi.Tag;
                                ifolderObject.iFolderState = iFolderState.SyncDisabled;
                                ifolderObject.iFolderWeb.State = "WaitSync";
                                
                                int imageIndex;
                                tlvi.ItemLocation = ifolderObject.iFolderWeb.UnManagedPath;
                                tlvi.Status = getItemState(ifolderObject, 0, out imageIndex);
                                tlvi.ImageIndex = imageIndex;
                                tlvi.Tag = ifolderObject;
                                
                                if (!thumbnailView)
                                {
                                    ListViewItem item = listView1.FindItemWithText(ifolderObject.iFolderWeb.ID);
                                    listView1.Items[item.Index].ImageIndex = imageIndex;
                                    listView1.Items[item.Index].SubItems[3].Text = tlvi.Status;
                                }
                                syncLog.AddMessageToLog(DateTime.Now,
                                string.Format("Synchronization Disable for:{0}", ifolderObject.iFolderWeb.Name));
                            }
                        }
                        break;
                    }
                                   
				}
			}
			catch {}
		}

		private void syncFile(FileSyncEventArgs syncEventArgs)
		{
			try
			{
				    progressBar1.Visible = syncEventArgs.SizeToSync > 0;
					progressBar1.Value = 0;
					progressBar1.Maximum = 200;
                    progressBar1.Value = syncEventArgs.SizeToSync > 0 ? (int)(((syncEventArgs.SizeToSync - syncEventArgs.SizeRemaining) * 200) / syncEventArgs.SizeToSync) : progressBar1.Maximum;

					if (startSync || (objectsToSync <= 0))
					{
						startSync = false;
						SyncSize syncSize = ifWebService.CalculateSyncSize(syncEventArgs.CollectionID);
						objectsToSync = syncSize.SyncNodeCount;
					}

					if (!syncEventArgs.Direction.Equals(Direction.Local))
					{
						lock (ht)
						{
							TileListViewItem tlvi = (TileListViewItem)ht[syncEventArgs.CollectionID];
							if (tlvi != null)
							{
                                //skipping do decrement the objectToSync when it's value is 0
                                if (objectsToSync != 0)
                                {
                                    tlvi.Status = string.Format(TrayApp.Properties.Resources.statusSyncingItems, objectsToSync--);
                                }
							}
						}
					}

					switch (syncEventArgs.ObjectType)
					{
						case ObjectType.File:
							if (syncEventArgs.Delete)
							{
								statusBar1.Text = string.Format(TrayApp.Properties.Resources.deleteClientFile, syncEventArgs.Name);
							}
							else
							{
								switch (syncEventArgs.Direction)
								{
									case Direction.Uploading:
										statusBar1.Text = string.Format(TrayApp.Properties.Resources.uploadFile, syncEventArgs.Name);
										break;
									case Direction.Downloading:
										statusBar1.Text = string.Format(TrayApp.Properties.Resources.downloadFile, syncEventArgs.Name);
										break;
									case Direction.Local:
										statusBar1.Text = string.Format(TrayApp.Properties.Resources.localFile, syncEventArgs.Name);
										break;
									default:
										statusBar1.Text = string.Format(TrayApp.Properties.Resources.syncingFile, syncEventArgs.Name);
										break;
								}
							}
							break;
						case ObjectType.Directory:
							if (syncEventArgs.Delete)
							{
								statusBar1.Text = string.Format(TrayApp.Properties.Resources.deleteClientDir, syncEventArgs.Name);
							}
							else
							{
								switch (syncEventArgs.Direction)
								{
									case Direction.Uploading:
										statusBar1.Text = string.Format(TrayApp.Properties.Resources.uploadDir, syncEventArgs.Name);
										break;
									case Direction.Downloading:
										statusBar1.Text = string.Format(TrayApp.Properties.Resources.downloadDir, syncEventArgs.Name);
										break;
									case Direction.Local:
										statusBar1.Text = string.Format(TrayApp.Properties.Resources.localDir, syncEventArgs.Name);
										break;
									default:
										statusBar1.Text = string.Format(TrayApp.Properties.Resources.syncingDir, syncEventArgs.Name);
										break;
								}
							}
							break;
						case ObjectType.Unknown:
							statusBar1.Text = string.Format(TrayApp.Properties.Resources.deleteUnknown, syncEventArgs.Name);
							break;
					}
			}
			catch {}
		}

		private void deleteEvent(NodeEventArgs args)
		{
			switch (args.Type)
			{
				case "Collection"://NodeTypes.CollectionType:
				case "Subscription"://NodeTypes.SubscriptionType:
                    lock (ht)
					{
						TileListViewItem tlvi = (TileListViewItem)ht[args.Node];
						if (tlvi != null)
						{
							iFolderWeb ifolder = ((iFolderObject)tlvi.Tag).iFolderWeb;
							if ( !ifolder.IsSubscription )
							{
								// Notify the shell.
								Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, ifolder.UnManagedPath, IntPtr.Zero);
							}
							removeTileListViewItem( tlvi );
						}
					}
					break;
				case "Domain"://NodeTypes.DomainType:
					RemoveDomainFromList(args.Node);
					break;
			}
            refreshAll();
		}

		private void createChangeEvent(iFolderWeb ifolder, string eventData)
		{
			if (ifolder != null)
			{
				if (eventData.Equals("NodeCreated"))
				{
					// Next line is commented previously...
				//	if (IsSelected(ifolder.DomainID))
					{
						iFolderObject ifobject = new iFolderObject(ifolder, iFolderState.Normal);
                        addiFolderToListView(ifobject);
                        ifobject = null;

						if ( !ifolder.IsSubscription )
						{
							// Notify the shell.
							Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, ifolder.UnManagedPath, IntPtr.Zero);
						}

						// Check for existing subscriptions when an iFolder gets created and remove them
						// from the list.
						///*
						if (!ifolder.IsSubscription)
						{
							// See if there is a subscription for this ifolder.
							lock (ht)
							{
								TileListViewItem[] lvia = new TileListViewItem[ht.Count];
								ht.Values.CopyTo(lvia, 0);
								/*
								string[] keys = new string[ht.Count];
								(ht.Keys).CopyTo(keys, 0);
									for(int i=0;i<keys.Length;i++)
										lvia[i] = (ListViewItem) ht[keys[i]];
								*/
								foreach(TileListViewItem lvi in lvia)
								{
									iFolderObject ifo = lvi.Tag as iFolderObject;
									if (ifo.iFolderWeb.IsSubscription &&
										(ifo.iFolderWeb.CollectionID == ifolder.CollectionID))
									{
										ht.Remove(ifo.iFolderWeb.ID);
										lvi.Remove();
										break;
									}
								}
                                
							}
						}//*/
					}

				}
				else
				{
					TileListViewItem tlvi;
					lock (ht)
					{
						// Get the corresponding listview item.
						tlvi = (TileListViewItem)ht[ifolder.ID];
					}

                    if (tlvi != null)
					{
						// Update the tag data.
						((iFolderObject)tlvi.Tag).iFolderWeb = ifolder;
						updateListViewItem(tlvi);
					}
				}
			}
		}

		private void addiFolderToListView(iFolderObject ifolderObject)
		{
			iFolderWeb ifolder = ifolderObject.iFolderWeb;
			if ( !ifolder.IsSubscription )
			{
				lock (ht)
				{
					// Add only if it isn't already in the list.
					if (!ht.ContainsKey(ifolder.ID))
					{
						TileListViewItem tlvi = new TileListViewItem( ifolderObject );
						int imageIndex;
						tlvi.Status = getItemState( ifolderObject, 0, out imageIndex );
						tlvi.ImageIndex = imageIndex;
						iFolderView.Items.Add(tlvi);
						iFolderView.Items.Sort();
						// Add the listviewitem to the hashtable.
						ht.Add(ifolder.ID, tlvi);
					}
				}
				// Notify the shell.
				Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, 
                    Win32Window.SHCNF_PATHW, 
                    ifolder.UnManagedPath, 
                    IntPtr.Zero);
			}
			else
			{
				lock( ht )
				{
					if (!ht.ContainsKey(ifolder.ID))
					{
						TileListViewItem tlvi = addiFolderToAvailableListView( ifolderObject );
						ht.Add( ifolder.ID, tlvi );
					}
				}
			}
            if (!thumbnailView)
            {
                showiFolderinListView();
            }
			updateView();
		}

		private TileListViewItem addiFolderToAvailableListView( iFolderObject ifolderObject )
		{
			TileListViewItem tlvi = null;

			lock ( iFolderListViews )
			{
                iFoldersListView ifListView = null;
                ifListView = (iFoldersListView)iFolderListViews[ifolderObject.iFolderWeb.DomainID];
                if (ifListView == null)
                    ifListView = AddDomainToUIList(simiasWebService.GetDomainInformation(ifolderObject.iFolderWeb.DomainID));
                tlvi = ifListView.AddiFolderToListView(ifolderObject);
                ifListView.Items.Sort();
                
                int imageIndex;
                tlvi.Status = getItemState(ifolderObject, 0, out imageIndex);
                tlvi.ImageIndex = imageIndex;
			}

			return tlvi;
		}

		private void updateWidth()
		{
			if ( infoMessage.Visible )
			{
				minWidth = infoMessage.Left + infoMessage.Width;
			}
			else
			{
				minWidth = localiFoldersHeading.Left + localiFoldersHeading.Width;
			}
            
			foreach ( iFoldersListView ifListView in iFolderListViews.Values )
			{
				ifListView.Anchor = AnchorStyles.Left | AnchorStyles.Top;
				if ( minWidth < ifListView.Width )
				{
					minWidth = ifListView.Width;
				}
			}
           
			minWidth += 20;

			foreach ( iFoldersListView ifListView in iFolderListViews.Values )
			{
				ifListView.Width = minWidth;
			}

			//iFolderView.Width = minWidth - iFolderView.Left;
		}

		private void updateView()
		{
			Point point = localiFoldersHeading.Location;
			point.Y += localiFoldersHeading.Height;
			if ( iFolderView.Items.Count == 0 )
			{
				iFolderView.Visible = false;
				if(this.toolStripBtnFilter.Text.Length > 0) // if( this.filter.Text.Length > 0)
					infoMessage.DisplayNoMatches();
				else
					infoMessage.DisplayNoiFolders();
				infoMessage.Visible = true;
				infoMessage.Location = point;
				point.Y += infoMessage.Height;
			}
			else
			{
				infoMessage.Visible = false;
				iFolderView.Visible = true;
                iFolderView.ReCalculateItems();
				iFolderView.Location = point;
				point.Y += iFolderView.Height;
			}

			for (int i = 3; i < panel2.Controls.Count; i++)
			{
				Control control = panel2.Controls[i];
				control.Location = point;
				point.Y += control.Height;
			}
		}

		private void updateListViewItem(TileListViewItem tlvi)
		{
			iFolderObject ifolderObject = (iFolderObject)tlvi.Tag;
			iFolderWeb ifolder = ifolderObject.iFolderWeb;

			if (ifolder.State.Equals("Available"))
			{
				// The iFolder already exists locally ... remove it from the list.
				lock (ht)
				{
					removeTileListViewItem( tlvi );
				}
			}
			else
			{
				int imageIndex;
				tlvi.Status = getItemState( ifolderObject, objectsToSync, out imageIndex );
				tlvi.ImageIndex = imageIndex;
                if (!thumbnailView)
                {
                    ListViewItem item = listView1.FindItemWithText(ifolderObject.iFolderWeb.ID);
                    listView1.Items[item.Index].ImageIndex = imageIndex;
                    listView1.Items[item.Index].SubItems[3].Text = tlvi.Status;
                }
			}
		}

		private string getItemState( iFolderObject ifolderObject, uint objectsToSync, out int imageIndex )
		{
			string status;
			imageIndex = 0;

			if (ifolderObject.iFolderWeb.HasConflicts)
			{
				imageIndex = 5;
				status = TrayApp.Properties.Resources.statusConflicts;
			}
			else
			{
				switch (ifolderObject.iFolderState)
				{
					case iFolderState.Normal:
					{
						switch (ifolderObject.iFolderWeb.State)
						{
							case "Local":
								// change 3 to image index for encryption ifolder icon and shared icon
								if( ifolderObject.iFolderWeb.encryptionAlgorithm == null || ifolderObject.iFolderWeb.encryptionAlgorithm == "")
								{
									if(ifolderObject.iFolderWeb.shared == true)
										imageIndex = 8;
									else
										imageIndex = 0;
								}	
								else
									imageIndex = 7;
								status = string.Format( TrayApp.Properties.Resources.statusSynced, ifolderObject.iFolderWeb.LastSyncTime );
								break;
							case "Available":
							case "WaitConnect":
							case "WaitSync":
								if ( ifolderObject.iFolderWeb.IsSubscription )
								{
									imageIndex = 2;
									// TODO: return the size?
									status = "";
								}
								else
								{
									imageIndex = 4;
									//status = resourceManager.GetString(ifolderObject.iFolderWeb.State);
                                    status = TrayApp.Properties.Resources.WaitSync;
								}
								break;
							default:
								// TODO: what icon to use for unknown status?
								imageIndex = 0;
								status = TrayApp.Properties.Resources.statusUnknown;
								break;
						}
						break;
					}
					case iFolderState.Initial:
						imageIndex = 4;
						status = "Initial "+ifolderObject.iFolderWeb.State;
						break;
					case iFolderState.Disconnected:
                        //changes are done for BUG#419284, to display "Server Unavailable" when LAN is disconnected
                       /* if( (this.simiasWebService.GetDomainInformation(ifolderObject.iFolderWeb.DomainID)).Authenticated )
                        {
                            imageIndex = 4;
                            status = TrayApp.Properties.Resources.WaitSync;
                        }
                        else */
                        {
                            imageIndex = 6;
                            status = TrayApp.Properties.Resources.disconnected;
                        }
        				break;
					case iFolderState.FailedSync:
						imageIndex = 9;
						status = objectsToSync == 0 ?
							TrayApp.Properties.Resources.statusSyncFailure :
							string.Format(TrayApp.Properties.Resources.statusSyncItemsFailed, objectsToSync);
						break;
					case iFolderState.Synchronizing:
						imageIndex = 1;
						status = objectsToSync == 0 ?
							TrayApp.Properties.Resources.statusSyncing :
							string.Format(TrayApp.Properties.Resources.statusSyncingItems, objectsToSync);
						break;
					case iFolderState.SynchronizingLocal:
						imageIndex = 1;
						status = TrayApp.Properties.Resources.preSync;
						break;
                    case iFolderState.NoPassphrase:
                        imageIndex = 9;  //FIXME : need to get new image for this.
                        status = TrayApp.Properties.Resources.noPassphrase;
                        break;
                    case iFolderState.SyncDisabled:
                        imageIndex = 9; //FIXME : need to get new image for this.
                        status = TrayApp.Properties.Resources.syncDisabled;
                        break;
					default:
						// TODO: what icon to use for unknown status?
						imageIndex = 0;
						status = TrayApp.Properties.Resources.statusUnknown;
						break;
				}
			}
			return status;
		}

		public void refreshAll(/*Domain domain*/)
		{
			this.refreshTimer.Stop();
			Cursor.Current = Cursors.WaitCursor;
			if( inRefresh == true)
				return;
			inRefresh = true;
            if (null != refreshThread)
            {
                refreshThread = null;
            }
			refreshThread = new Thread(new ThreadStart(updateiFolders));
            refreshThread.Name = "Refresh iFolder";
			refreshThread.IsBackground = true;
			refreshThread.Priority = ThreadPriority.BelowNormal;
			refreshThread.Start();
		}

		private void updateiFolders()
		{
			bool done = false;
			while(!done)
			{
				try
				{
					ifolderArray = ifWebService.GetAlliFolders();
					done = true;
				}
				catch(Exception e)
				{
					Thread.Sleep(3000);
					done = false;
					continue;
				}
			}
			BeginInvoke(this.refreshiFoldersDelegate);
		}

		public void refreshiFoldersInvoke()
		{
			refreshiFolders(null);
		}

        public void updateifListViewDomainStatus(string domainID, bool status)
        {
            foreach (iFoldersListView ifListView in iFolderListViews.Values)
            {
                if (ifListView.DomainInfo.ID == domainID)
                {
                    ifListView.DomainInfo.Authenticated = status;
                    break; 
                }
            }
        }

        public void UpdateiFolderStatus(bool IsDomainAuthenticated, string domainID)
        {
            TileListViewItem tlvi;
            iFolderObject ifolderObject;
            int imageIndex;
            foreach (iFolderWeb ifolder in ifolderArray)
            {
                if (ifolder.DomainID == domainID && !ifolder.IsSubscription)
                {
                    tlvi = (TileListViewItem)ht[ifolder.ID];
                    ifolderObject = (iFolderObject)tlvi.Tag;
                    if (IsDomainAuthenticated)
                    {
                        ifolderObject.iFolderState = iFolderState.Normal;
                        ifolderObject.iFolderWeb.State = "WaitSync";
                    }
                    else
                    {
                        ifolderObject.iFolderState = iFolderState.Disconnected;
                    }
                    tlvi.Status = getItemState(ifolderObject, 0, out imageIndex);
                    tlvi.ImageIndex = imageIndex;
                }
            }
        }

		public void refreshiFolders(string search/*Domain domain*/)
		{
            if (search == null) search = "";
            search = this.toolStripBtnFilter.Text; // this.filter.Text;
			Cursor.Current = Cursors.WaitCursor;
			Hashtable oldHt = new Hashtable();
            Hashtable tempHt = new Hashtable(); 
			lock(ht)
			{
				// Save the state of the old items so state can be preserved between refreshes.
				foreach ( TileListViewItem tlvi in ht.Values )
				{
					iFolderObject ifolderObject = (iFolderObject)tlvi.Tag;
					tempHt.Add( ifolderObject.ID, ifolderObject.iFolderState );
				}
			}
			try
			{
				panel2.SuspendLayout();

                if (selectedItem != null)
                    updateMenus((iFolderObject)selectedItem.Tag);
                else
                    updateMenus(null);

				// Walk the list of iFolders and add them to the listviews.
				foreach (iFolderWeb ifolder in ifolderArray) //iFolderArray is a class member should be made null for GC
				{
					if( search != null && ((String)ifolder.Name).ToLower().IndexOf(search.ToLower(), 
                        0, ((String)ifolder.Name).Length) < 0)
						continue;
					iFolderState state = iFolderState.Normal;
			        if (tempHt.Contains(ifolder.ID))
                     {
                        state = (iFolderState)tempHt[ifolder.ID];
                     }
                    else
                     {
                        //if not present add
                        iFolderObject ifobj = new iFolderObject(ifolder, state);
                        addiFolderToListView(ifobj);
                        ifobj = null;
                      }
                      oldHt.Add(ifolder.ID, state);
                      if (this.acceptedFolders.Contains(ifolder.ID))
                      {
                          this.acceptedFolders.Remove(ifolder.ID);
                      }
                }
                // Check whether or not the count is same..
				foreach( System.Object obj in this.acceptedFolders.Values)
				{
					TileListViewItem tlv = (TileListViewItem)obj;
					iFolderObject ifobj = (iFolderObject)tlv.Tag;
					if( search != null && ((String)ifobj.iFolderWeb.Name).ToLower().IndexOf(search.ToLower(), 
                        0, ((String)ifobj.iFolderWeb.Name).Length) < 0)
						continue;
					ifobj.iFolderWeb.IsSubscription = false;
					ifobj.iFolderState = iFolderState.Initial;
					addiFolderToListView( ifobj );
                    if(!oldHt.ContainsKey(ifobj.ID))
                        oldHt.Add(ifobj.ID, ifobj.iFolderState);
				}
                string[] ifolders = new string[ht.Count];
                ht.Keys.CopyTo(ifolders, 0);
                foreach (string ifolderid in ifolders)
                {
                   if (oldHt.ContainsKey(ifolderid) == false)
                    {
                        // Remove from the display...
                        removeTileListViewItem((TileListViewItem)ht[ifolderid]);
                    }
                }
			}
			catch (Exception ex)
			{
				Novell.iFolderCom.MyMessageBox mmb = 
                    new MyMessageBox(TrayApp.Properties.Resources.iFolderError, 
                    TrayApp.Properties.Resources.iFolderErrorTitle, 
                    ex.Message, MyMessageBoxButtons.OK, 
                    MyMessageBoxIcon.Information);
				mmb.ShowDialog();
				mmb.Dispose();
			}
			iFolderView.Items.Sort();
			foreach (iFoldersListView ifListView in iFolderListViews.Values)
			{
				ifListView.FinalizeUpdate();
			}
			updateView();
			panel2.ResumeLayout();
            DomainsListUpdate();
            DomainsListUpdateComboBox();
			inRefresh = false;
            oldHt = null;
      	    Cursor.Current = Cursors.Default;
            if (!thumbnailView)
            {
                showiFolderinListView();
            }
			this.refreshTimer.Start(); 
		}

		private void invokeiFolderProperties(TileListViewItem tlvi, int activeTab)
		{
			iFolderAdvanced ifolderAdvanced = new iFolderAdvanced();
			ifolderAdvanced.CurrentiFolder = ((iFolderObject)tlvi.Tag).iFolderWeb;
			ifolderAdvanced.DomainName = ((DomainInformation)this.simiasWebService.GetDomainInformation(((iFolderObject)tlvi.Tag).iFolderWeb.DomainID)).Name;
            ifolderAdvanced.DomainUrl = ((DomainInformation)this.simiasWebService.GetDomainInformation(((iFolderObject)tlvi.Tag).iFolderWeb.DomainID)).HostUrl;
			ifolderAdvanced.LoadPath = Application.StartupPath;
			ifolderAdvanced.ActiveTab = activeTab;
			ifolderAdvanced.EventClient = eventClient;
			ifolderAdvanced.ShowDialog();
			ifolderAdvanced.Dispose();
		}

		private void synciFolder(string iFolderID)
		{
			try
			{
				ifWebService.SynciFolderNow(iFolderID);
			}
			catch (Exception ex)
			{
				Novell.iFolderCom.MyMessageBox mmb = new 
                    MyMessageBox(TrayApp.Properties.Resources.syncError, 
                    TrayApp.Properties.Resources.syncErrorTitle, 
                    ex.Message, MyMessageBoxButtons.OK, 
                    MyMessageBoxIcon.Error);
				mmb.ShowDialog();
				mmb.Dispose();
			}
		}

        private void enableServerFolderLabels(bool visible)
        {
            this.titleName.Visible =
            this.titleAccess.Visible =
            this.titleServerorSize.Visible =
            this.titleEncrypted.Visible = visible;

            this.titleOwner.Visible =
            this.titleLastSyncTime.Visible = !visible;
        }

        private void enableLocalFoldersLabels(bool visible)
        {
            this.titleName.Visible =
            this.titleAccess.Visible =
            this.titleServerorSize.Visible =
            this.titleEncrypted.Visible = 
            this.titleOwner.Visible =
            this.titleLastSyncTime.Visible = visible;
        }
		private void updateMenus(iFolderObject ifolderObject)
		{
			if ( ifolderObject == null )
			{
                initialButtonState(); //set the button state to initial values.
                populateAllInfoWithNA(); //set the labels to Not Available
			}
			else
			{
				iFolderWeb ifolderWeb = ifolderObject.iFolderWeb;

				if ( ifolderWeb.IsSubscription ) //iFolders on the server
				{
                    populateServeriFolderInfo(ifolderWeb);

					// Disable the iFolder related menu items. //TODO
					this.menuActionOpen.Enabled = 
                    this.menuActionProperties.Enabled =
					this.menuActionRevert.Enabled = 
                    this.menuActionShare.Enabled =
					this.menuActionSync.Enabled = 
                    this.menuActionResolve.Enabled = false;
                        
					// Enable the available iFolder related menu items.
                    this.menuActionRemove.Enabled = 
                    this.menuActionAccept.Enabled = 
                    this.menuActionMerge.Enabled = true;

					if( ifolderWeb.CurrentUserID != ifolderWeb.OwnerID)
					{
                        this.MenuRemove.Text = TrayApp.Properties.Resources.RemoveMyMembership;
                        this.menuActionRemove.Text = Resources.RemoveMyMembershipText;
                        toolStripBtnDelete.Image = Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\remove_share_48.png"));
                        toolStripBtnDelete.Text = Resources.remove;
                        toolStripBtnDelete.ToolTipText = Resources.RemoveMyMembership;
                    }
					else
					{
                        this.MenuRemove.Text = TrayApp.Properties.Resources.menuActionRemove;
                        this.menuActionRemove.Text = TrayApp.Properties.Resources.menuRemoveText;
                        toolStripBtnDelete.Image = Bitmap.FromFile(Path.Combine(Application.StartupPath, @"res\delete_48.png"));
                        toolStripBtnDelete.Text = Resources.delete;
                        toolStripBtnDelete.ToolTipText = Resources.menuActionRemove;
					}
					// Show the available iFolder buttons
                    enableRemoteFoldersButtons(true);
				}
				else //local folders
				{
                    populateLocaliFolderInfo(ifolderWeb);

					// Disable the available iFolder related menu items.
                    this.menuActionRemove.Enabled = this.menuActionAccept.Enabled = this.menuActionMerge.Enabled = false;

					// Enable the iFolder related menu items.
					this.menuActionOpen.Enabled = this.menuActionProperties.Enabled =
						this.menuActionRevert.Enabled = this.menuActionShare.Enabled =
						this.menuActionSync.Enabled = true;

                  	this.menuActionResolve.Enabled = this.toolStripBtnResolve.Enabled = this.toolStripBtnResolve.Visible = ifolderWeb.HasConflicts;
				
					if( ifolderObject.iFolderState == iFolderState.Initial )
					{
                        this.toolStripBtnShare.Enabled = false;
						this.CtxMenuOpen.Enabled = this.menuShare.Enabled = false; 
						this.MenuRevert.Enabled = this.menuProperties.Enabled = false;

						this.menuActionOpen.Enabled = this.menuActionShare.Enabled = false;
						this.menuActionRevert.Enabled = this.menuActionProperties.Enabled = false;
						return;
					}
					// Show the local iFolder buttons
                    enableRemoteFoldersButtons(false);
					// Show right-click menu
					this.CtxMenuOpen.Enabled = this.menuShare.Enabled = true; 
					this.MenuRevert.Enabled = this.menuProperties.Enabled = true;
                    
                    //Conditon to enable/disable revert option based on current status of ifolder.
                    //code should get executed at the end of this function
                   
                    if ((ifolderWeb.State == "Local" || ifolderWeb.State == "WaitSync") && ((ifolderObject.iFolderState == iFolderState.SynchronizingLocal) || (ifolderObject.iFolderState == iFolderState.Synchronizing)))
                    {
                        this.toolStripBtnRevert.Enabled = this.menuActionRevert.Enabled = this.MenuRevert.Enabled = false;
                    }
                    else
                    {
                        this.toolStripBtnRevert.Enabled = this.menuActionRevert.Enabled = this.MenuRevert.Enabled = true;
                    }
				}
			}
		}
        private void initialButtonState()
        {
            // Disable all of the item-related menu items.
            this.menuActionOpen.Enabled = this.menuActionAccept.Enabled =
            this.menuActionMerge.Enabled = this.menuActionProperties.Enabled =
            this.menuActionRemove.Enabled = this.menuActionResolve.Enabled = 
            this.menuActionRevert.Enabled = this.menuActionShare.Enabled = 
            this.menuActionSync.Enabled = false;

            this.toolStripBtnCreate.Visible = true;

            this.toolStripBtnDownload.Enabled =
            this.toolStripBtnMerge.Enabled =
            this.toolStripBtnDelete.Enabled =
            this.toolStripBtnSyncNow.Enabled =
            this.toolStripBtnShare.Enabled =
            this.toolStripBtnResolve.Enabled =
            this.toolStripBtnRevert.Enabled = false;
        }
        private void enableRemoteFoldersButtons(bool server)
        {
            this.toolStripBtnCreate.Enabled = true;
            if (server)
            {
                this.toolStripBtnDownload.Enabled =
                this.toolStripBtnMerge.Enabled =
                this.toolStripBtnDelete.Enabled = server;

                this.toolStripBtnSyncNow.Enabled = 
                this.toolStripBtnShare.Enabled = 
                this.toolStripBtnRevert.Enabled = ! server;
            }
            else
            {
                this.toolStripBtnDownload.Enabled =
                this.toolStripBtnMerge.Enabled =
                this.toolStripBtnDelete.Enabled = server;

                this.toolStripBtnSyncNow.Enabled =
                this.toolStripBtnShare.Enabled =
                this.toolStripBtnRevert.Enabled = !server;
            }
        }

        private void populateAllInfoWithNA()
        {
            this.titleName.Text = TrayApp.Properties.Resources.name + ":  " + Resources.na;
            this.titleOwner.Text = TrayApp.Properties.Resources.owner + ":  " + Resources.na;
            this.titleAccess.Text = Resources.access + ":  " + Resources.na;
            this.titleEncrypted.Text = Resources.type + ":  "  +Resources.na;
            this.titleLastSyncTime.Text = Resources.lastSyncTime + ":  "  +Resources.na;
            this.titleServerorSize.Text = Resources.server + ":  "  +Resources.na;
        }
        private void populateLocaliFolderInfo(iFolderWeb ifolderWeb)
        {
            enableLocalFoldersLabels(true);
            //TODO: considring for now, 20 as displayable name lenght, to be changed appropriately
            int displayableLength = 20;
            this.titleName.Text = TrayApp.Properties.Resources.name + ":  " + FormatString(ifolderWeb.Name, displayableLength);
            this.titleOwner.Text = TrayApp.Properties.Resources.owner + ":  " + ifolderWeb.Owner;
            iFolderUser[] ifolderUsers = ifWebService.GetiFolderUsers(ifolderWeb.ID);
            foreach (iFolderUser ifolderUser in ifolderUsers)
            {
                if (ifolderUser.UserID.Equals(ifolderWeb.CurrentUserID))
                    this.titleAccess.Text = Resources.access + ":  " + ifolderUser.Rights.ToString();
            }
            if (ifolderWeb.encryptionAlgorithm != null && ifolderWeb.encryptionAlgorithm != "")
                this.titleEncrypted.Text =  Resources.type + ":  " + Resources.encrypted;
            else
                this.titleEncrypted.Text = Resources.type + ":  " + Resources.regular;

            this.titleLastSyncTime.Text = Resources.lastSyncTime + ":  " + ifolderWeb.LastSyncTime;
            this.titleServerorSize.Text = Resources.server + ":  " +
                    simiasWebService.GetDomainInformation(ifolderWeb.DomainID).Host;
        }

        private void populateServeriFolderInfo(iFolderWeb ifolderWeb)
        {
            enableServerFolderLabels(true);
            int displayableLength = 20;
            this.titleName.Text = TrayApp.Properties.Resources.name + ":  " + FormatString(ifolderWeb.Name, displayableLength);
            this.titleOwner.Text = TrayApp.Properties.Resources.owner + ":  " + ifolderWeb.Owner;

            if (ifolderWeb.encryptionAlgorithm != null && ifolderWeb.encryptionAlgorithm != "")
                this.titleEncrypted.Text = Resources.type + ":  " + Resources.encrypted;
            else
                this.titleEncrypted.Text = Resources.type + ":  " + Resources.regular;
            try
            {
                iFolderUser[] ifolderUsers = ifWebService.GetiFolderUsers(ifolderWeb.ID);
                foreach (iFolderUser ifolderUser in ifolderUsers)
                {
                    if (ifolderUser.UserID.Equals(ifolderWeb.CurrentUserID))
                        this.titleAccess.Text = Resources.access + ":  " + ifolderUser.Rights.ToString();
                }
            }
            catch { } //TODO: Getting incorrect ID at times needs to be fixed.
            this.titleServerorSize.Text = Resources.server + ":  " + 
                simiasWebService.GetDomainInformation(ifolderWeb.DomainID).Host;
            this.titleLastSyncTime.Text = Resources.lastSyncTime + ":  " + Resources.na;
        } 

		private void updateEnterpriseData()
		{
			DomainInformation[] domains;
			try
			{
				domains = simiasWebService.GetDomains(false);
				foreach (DomainInformation di in domains)
				{
					AddDomainToList(di);
				}
			}
			catch
			{
				// TODO: Message?
			}
		}

        private bool acceptiFolder(iFolderWeb ifolder, string path, out bool added, bool mergeFolder)
        {
            bool download = false;
            bool result = true;
            bool repeatFlag = false;
            added = true;
            if (GetDriveType(Path.GetPathRoot(path)) == DRIVE_FIXED)
            {
                // Check to make sure the user has rights to this directory.
                if (Win32Security.AccessAllowed(path))
                {
                    try
                    {

                        if (ifolder.encryptionAlgorithm == null || ifolder.encryptionAlgorithm == "")
                        {
                            // unencrypted...
                            download = true;
                        }
                        else
                        {
                            //this.simiasWebService.GetDomainInformation(((iFolderObject)tlvi.Tag).iFolderWeb.DomainID))
                            string passphrasecheck = null;
                            passphrasecheck = simiasWebService.GetPassPhrase(ifolder.DomainID);
                            if (passphrasecheck == null || passphrasecheck == "")
                            {
                                VerifyPassphraseDialog vpd = new VerifyPassphraseDialog(ifolder.DomainID, simiasWebService);
                                vpd.ShowDialog();
                                download = vpd.PassphraseStatus;
                            }
                            else
                            {
                                download = true;
                            }
                        }
                        if (download)
                        {
                            // Display wait cursor.
                            Cursor = Cursors.WaitCursor;

                            if (mergeFolder == true)
                            {
                                //string mergediFolder = Path.Combine(path, ifolder.Name);
				                string mergediFolder = path;
				                if(System.IO.Path.GetFileName(mergediFolder) != ifolder.Name)
				                {
					                throw new Exception("FolderDoesNotExist");
				                }
                                if (Directory.Exists(mergediFolder) == false)
                                {
                                    throw new Exception("PathDoesNotExist");
                                }
                                if (ifWebService.IsiFolder(mergediFolder) == true)
                                {
                                    throw new Exception("AtOrInsideCollectionPath");
                                }
                                ifWebService.MergeiFolder(ifolder.DomainID, ifolder.ID, mergediFolder);
                                Cursor = Cursors.Default;
                            }
                            else
                            {
                                // Accept the invitation.
                                 DirectoryInfo di = new DirectoryInfo(path);
                                if (di.Name == ifolder.Name)
                                {
                                    path = Directory.GetParent(path).ToString();
                                }
                                if( System.IO.Directory.Exists( Path.Combine(path,ifolder.Name)) )
                                {
                                    MyMessageBox mmb = new MyMessageBox(Resources.OkMergeCancel,Resources.Folderexists, String.Empty, MyMessageBoxButtons.OKCancel, MyMessageBoxIcon.Question, MyMessageBoxDefaultButton.Button1);
		                            if (mmb.ShowDialog() == DialogResult.OK)
				                    {
                                        ifWebService.MergeiFolder(ifolder.DomainID, ifolder.ID, Path.Combine(path, ifolder.Name));
                                    }
                                    else
                                        return false;
			                    }
		                        else
	   	    	                    ifWebService.AcceptiFolderInvitation(ifolder.DomainID, ifolder.ID, path);

                                Cursor = Cursors.Default;
                            }
                        }
                        else
                        {
                            result = true;
                            added = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Cursor = Cursors.Default;
                        added = false;
                        MyMessageBox mmb;
                        if (ex.Message.IndexOf("260") != -1)
                        {
                            mmb = new MyMessageBox("The iFolder path is too long for the File System. Download failed", "Path Too Long", string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
                            mmb.ShowDialog();
                            repeatFlag = true;
                        }
                        else if (ex.Message.IndexOf("PathExists") != -1)
                        {
                            mmb = new MyMessageBox(TrayApp.Properties.Resources.pathExistsError, TrayApp.Properties.Resources.pathInvalidErrorTitle, string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
                            mmb.ShowDialog();
                        }
                        else if (ex.Message.IndexOf("AtOrInsideStorePath") != -1)
                        {
                            mmb = new MyMessageBox(TrayApp.Properties.Resources.pathInStoreError, TrayApp.Properties.Resources.pathInvalidErrorTitle, string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
                            mmb.ShowDialog();
                        }
                        else if (ex.Message.IndexOf("AtOrInsideCollectionPath") != -1)
                        {
                            mmb = new MyMessageBox(TrayApp.Properties.Resources.pathIniFolderError, TrayApp.Properties.Resources.pathInvalidErrorTitle, string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
                            mmb.ShowDialog();
                        }
                        else if (ex.Message.IndexOf("IncludesWinDirPath") != -1)
                        {
                            mmb = new MyMessageBox(TrayApp.Properties.Resources.pathIncludesWinDirError, TrayApp.Properties.Resources.pathInvalidErrorTitle, string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
                            mmb.ShowDialog();
                        }
                        else if (ex.Message.IndexOf("IncludesProgFilesPath") != -1)
                        {
                            mmb = new MyMessageBox(TrayApp.Properties.Resources.PathIncludesProgFilesDirError, TrayApp.Properties.Resources.pathInvalidErrorTitle, string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
                            mmb.ShowDialog();
                        }
                        else if (ex.Message.IndexOf("PathDoesNotExist") != -1)
                        {
                            mmb = new MyMessageBox(TrayApp.Properties.Resources.PathDoesNotExist, TrayApp.Properties.Resources.pathInvalidErrorTitle, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
                            mmb.ShowDialog();
                        }
                        else if (ex.Message.IndexOf("FolderDoesNotExist") != -1)
                        {
                            mmb = new MyMessageBox(TrayApp.Properties.Resources.FolderDoesNotExistError, TrayApp.Properties.Resources.FolderDoesNotExistErrorTitle, TrayApp.Properties.Resources.FolderDoesNotExistErrorDesc, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
                            mmb.ShowDialog();
                        }
                        else
                        {
                            mmb = new MyMessageBox(TrayApp.Properties.Resources.acceptError, string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
                            mmb.ShowDialog();
                        }
                        if (!repeatFlag)
                            result = false;
                    }
                }
                else
                {
                    result = false;
                    MyMessageBox mmb = new MyMessageBox(TrayApp.Properties.Resources.accessDenied, string.Empty, string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
                    mmb.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show(TrayApp.Properties.Resources.networkPath, TrayApp.Properties.Resources.pathInvalidErrorTitle);
                result = false;
            }

            return result;
        }
        #endregion

		#region Event Handlers

		private void button_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			((Button)sender).BackColor = Color.FromKnownColor( KnownColor.ControlLight );
		}

		private void button_MouseEnter(object sender, System.EventArgs e)
		{
			((Button)sender).BackColor = Color.White;
		}

		private void button_MouseLeave(object sender, System.EventArgs e)
		{
			((Button)sender).BackColor = Button.DefaultBackColor;
		}

		private void button_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
		/*	if ( e.Button.Equals( MouseButtons.Left ) )
			{
				if ( e.X < 0 || e.Y < 0 || e.X > create.Width || e.Y > create.Height )
				{
					((Button)sender).BackColor = Button.DefaultBackColor;
				}
				else
				{
					((Button)sender).BackColor = Color.FromKnownColor( KnownColor.ControlLight );
				}
			}
         */ 
		}
        private void iFolderContextMenu_Popup(object sender, CancelEventArgs e)
		{
			if ( selectedItem == null )
			{
				// Display the refresh menu.
				MenuRefresh.Visible = true;

				// Hide the other menu items.
				CtxMenuOpen.Visible = menuSyncNow.Visible = menuShare.Visible =
				MenuRevert.Visible = menuProperties.Visible = MenuRemove.Visible =
                MenuAccept.Visible = MenuMerge.Visible = MenuResolve.Visible = MenuResolveSeperator.Visible =
				menuSeparator1.Visible = menuSeparator2.Visible = false;
			}
			else
			{
				// Hide the refresh menu.
				MenuRefresh.Visible = false;

				iFolderWeb ifolder = ((iFolderObject)selectedItem.Tag).iFolderWeb;

				if ( ifolder.IsSubscription )
				{
					// Hide the iFolder menus.
					CtxMenuOpen.Visible = menuSyncNow.Visible = menuShare.Visible =
						MenuRevert.Visible = menuProperties.Visible = MenuResolve.Visible =
                        MenuResolveSeperator.Visible = menuSeparator2.Visible = false;

					// Display the subscription-related menus based on Refresh thread.
                    if (!inRefresh)
                        MenuAccept.Visible = MenuMerge.Visible = MenuRemove.Visible = menuSeparator1.Visible = true;
                    else
                        MenuAccept.Visible = MenuMerge.Visible = MenuRemove.Visible = menuSeparator1.Visible = false;
                }
				else
				{
					// Hide the subscription-related menus.
                    MenuAccept.Visible = MenuMerge.Visible = MenuRemove.Visible = false;

					// Display the iFolder menus based on Refresh thread.
					CtxMenuOpen.Visible = menuSyncNow.Visible = menuShare.Visible = true;
					if(!inRefresh)
						MenuRevert.Visible = true;
					else
						MenuRevert.Visible = false;
					
					menuProperties.Visible = 
						menuSeparator2.Visible = true;

					MenuResolve.Visible =
                        MenuResolveSeperator.Visible = ifolder.HasConflicts;
				}
			}
		}

		private void panel2_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if ( selectedItem != null )
			{
				selectedItem.Selected = false;
				selectedItem = null;

				// Disable all of the item-related menu items.
                this.menuActionOpen.Enabled = this.menuActionAccept.Enabled = this.menuActionMerge.Enabled = this.menuActionProperties.Enabled =
					this.menuActionRemove.Enabled = this.menuActionResolve.Enabled = this.menuActionRevert.Enabled =
					this.menuActionShare.Enabled = this.menuActionSync.Enabled = false;

                this.toolStripBtnDelete.Enabled = 
                this.toolStripBtnDownload.Enabled = 
                this.toolStripBtnMerge.Enabled = 
                this.toolStripBtnResolve.Enabled = 
                this.toolStripBtnRevert.Enabled = 
                this.toolStripBtnShare.Enabled = 
                this.toolStripBtnSyncNow.Enabled = false;
			}
		}

		private void GlobalProperties_SizeChanged(object sender, System.EventArgs e)
		{
			// Calculate the size that we can use.
			int width;

			if ( panel2.Width > minWidth + 8 )
			{
				width = panel2.Width - 8;
			}
			else
			{
				width = minWidth - 8;
			}

			iFolderView.Width = width;

			foreach ( iFoldersListView ifListView in iFolderListViews.Values )
			{
				ifListView.Width = width;
			}

			updateView();
		}

		private void GlobalProperties_Move(object sender, System.EventArgs e)
		{
			if (initialPositionSet)
			{
				try
				{
					// Create/open the iFolder key.
					RegistryKey regKey = Registry.CurrentUser.CreateSubKey(Preferences.iFolderKey);

					// Set the location values.
					regKey.SetValue(myiFoldersX, Location.X);
					regKey.SetValue(myiFoldersY, Location.Y);
				}
				catch {}
			}
			else
			{
				try
				{
					// Create/open the iFolder key.
					RegistryKey regKey = Registry.CurrentUser.CreateSubKey(Preferences.iFolderKey);

					// Get the location values.
					int x = (int)regKey.GetValue(myiFoldersX);
					int y = (int)regKey.GetValue(myiFoldersY);

					Point point = new Point(x, y);

					// Only set the location if the point is on the screen.
					if (SystemInformation.VirtualScreen.Contains(point))
					{
						this.Location = point;
					}
				}
				catch {}

				initialPositionSet = true;
			}
		}

		private void GlobalProperties_Load(object sender, System.EventArgs e)
		{
			Graphics g = localiFoldersHeading.CreateGraphics();
			try
			{
				SizeF textSize = g.MeasureString(localiFoldersHeading.Text, localiFoldersHeading.Font);
                localiFoldersHeading.Width = panel2.Width; // (int)(textSize.Width * 1.1);
			}
			finally
			{
				g.Dispose();
			}
			refreshAll();
			this.refreshTimer.Start();

			showiFolders_Click( this, null );

            setThumbnailView(thumbnailView);

		}

		private void GlobalProperties_VisibleChanged(object sender, System.EventArgs e)
		{
			if (this.Visible)
			{
/*				InitializeServerList();

				DomainInformation[] domains;
				try
				{
					domains = simiasWebService.GetDomains(false);
					foreach (DomainInformation di in domains)
					{
						AddDomainToList(di);
					}
				}
				catch{}*/

				Activate();
			}
		}

		private void GlobalProperties_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// If we haven't received a shutdown event, hide this dialog and cancel the event.
			if (!shutdown)
			{
				e.Cancel = true;
				Hide();
			}
			else
			{
				FormsTrayApp.CloseApp();
			}
		}

		private void ifListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			TileListView tileListView = ( TileListView )sender;

			if ( selectedItem != null && 
				( tileListView.SelectedItem == null || !tileListView.SelectedItem.Equals( selectedItem ) ) )
			{
				selectedItem.Selected = false;
			}

			selectedItem = tileListView.SelectedItem;
			updateMenus( selectedItem == null ? null : (iFolderObject)selectedItem.Tag );
		}

		private void updateEnterpriseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			updateEnterpriseTimer.Stop();
			updateEnterpriseData();
		}

		private void menuFileExit_Click(object sender, System.EventArgs e)
		{
            //fileExitDlg = new MyMessageBox(TrayApp.Properties.Resources.exitMessage, TrayApp.Properties.Resources.exitTitle, string.Empty, MyMessageBoxButtons.YesNo, MyMessageBoxIcon.Question);
            DialogResult messageDialogResult = fileExitDlg.ShowDialog();
            if (messageDialogResult == DialogResult.Yes)
            {
                this.Dispose();
                this.Close();
                FormsTrayApp.CloseApp();
            }
            else
            {
                //ignore
            }
		}

		private void menuHelpHelp_Click(object sender, System.EventArgs e)
		{
			new iFolderComponent().ShowHelp(Application.StartupPath, string.Empty);
		}

		private void menuHelpAbout_Click(object sender, System.EventArgs e)
		{
			About about = new About();
			about.ShowDialog();
		}

		private void GlobalProperties_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			// Pressing the F5 key will cause a refresh to occur.
			if (e.KeyCode == Keys.F5)
			{
				if( this.inRefresh == false )
					refreshAll(/*(Domain)servers.SelectedItem*/);
			}
			if (e.KeyCode == Keys.F4)
			{
				GC.Collect(); //collect all generations
			}
		}

		private void menuOpen_Click(object sender, System.EventArgs e)
		{
			if ( selectedItem != null )
			{
				iFolderWeb ifolder = ((iFolderObject)selectedItem.Tag).iFolderWeb;

				try
				{
					Process.Start(ifolder.UnManagedPath);
				}
				catch (Exception ex)
				{
					Novell.iFolderCom.MyMessageBox mmb = 
                        new MyMessageBox(string.Format(TrayApp.Properties.Resources.iFolderOpenError, ifolder.Name), 
                            TrayApp.Properties.Resources.openErrorTitle, 
                            ex.Message, 
                            MyMessageBoxButtons.OK, 
                            MyMessageBoxIcon.Error);
					mmb.ShowDialog();
					mmb.Dispose();
				}
			}
		}

		private void menuRevert_Click(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
                
				iFolderWeb ifolder = ((iFolderObject)selectedItem.Tag).iFolderWeb;
				//bool IsMaster = (this.ifWebService.GetUserID( ifolder.DomainID) == ifolder.OwnerID);
				bool IsMaster = (ifolder.CurrentUserID == ifolder.OwnerID);

                IsDomainMaster = IsMaster;

				RevertiFolder revertiFolder = new RevertiFolder();
                
				if( !IsMaster )
					revertiFolder.removeFromServer.Text = TrayApp.Properties.Resources.AlsoRemoveMembership;

                revertiFolder.removeFromServer.Enabled =  simiasWebService.GetDomainInformation(ifolder.DomainID).Authenticated;
				
                if ( revertiFolder.ShowDialog() == DialogResult.Yes )
				{
					//ensure UI is re-painted before Revert is done
					Invalidate();
					Update();
					Cursor.Current = Cursors.WaitCursor;

                    if (revertiFolder.RemoveFromServer)
                         ((iFolderObject)selectedItem.Tag).iFolderState = iFolderState.RevertAndDelete;

					// Delete the iFolder.
					iFolderWeb newiFolder = ifWebService.RevertiFolder(ifolder.ID);
					
					// Notify the shell.
					Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, ifolder.UnManagedPath, IntPtr.Zero);
					
					if (newiFolder != null)
					{
						acceptedFolders.Remove(newiFolder.ID);
						if ( revertiFolder.RemoveFromServer )
						{
							// Remove the iFolder from the server.
                            selectediFolderID = newiFolder.ID;
                            selectedDomainID = newiFolder.DomainID;                   
                			Thread revertiFolderThread;
			                revertiFolderThread = new Thread(new ThreadStart(RemoveiFolderFromServer));
			                revertiFolderThread.Name = "RevertAndDeleteiFolder";
			                revertiFolderThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			                revertiFolderThread.IsBackground = true;
			                revertiFolderThread.Priority = ThreadPriority.BelowNormal;
                            revertiFolderThread.Start();
						}
						else
						{
							//lock ( ht )
							{
                                this.removeTileListViewItem((TileListViewItem)ht[newiFolder.ID]);
                                iFolderObject ifolderobj = new iFolderObject(newiFolder, iFolderState.Normal);
                                addiFolderToListView(ifolderobj);
                                updateView();
							}
						}
					}                  
				}

				revertiFolder.Dispose();
			}
			catch (Exception ex)
			{		
				Cursor.Current = Cursors.Default;
				Novell.iFolderCom.MyMessageBox mmb = 
                    new MyMessageBox(TrayApp.Properties.Resources.iFolderRevertError, 
                        TrayApp.Properties.Resources.revertErrorTitle, 
                        ex.Message, 
                        MyMessageBoxButtons.OK, 
                        MyMessageBoxIcon.Error);
				mmb.ShowDialog();
				mmb.Dispose();
			}

			Cursor.Current = Cursors.Default;
            
		}

        private void RemoveiFolderFromServer()
        {
            
            if (IsDomainMaster)
            {
                string defaultiFolderID = this.simiasWebService.GetDefaultiFolder(selectedDomainID);
                if (defaultiFolderID == selectediFolderID)
                {
                    this.simiasWebService.DefaultAccount(selectedDomainID, null);
                }
                ifWebService.DeleteiFolder(selectedDomainID, selectediFolderID);
            }
            ifWebService.DeclineiFolderInvitation(selectedDomainID, selectediFolderID);

            lock (ht)
			{
					removeTileListViewItem( selectedItem );
			}
				refreshAll();
        }
        public static bool AdvancedConflictResolver(iFolderWebService ifWebService, iFolderWeb selectedItem)
        {
            const string assemblyName = @"lib\plugins\EnhancedConflictResolution.dll";
            const string enhancedConflictResolverClass = "Novell.EnhancedConflictResolution.EnhancedConflictResolver";
            const string showConflictResolverMethod = "Show";
            System.Object[] args = new System.Object[3];

            try
            {
                if (assemblyName != null)
                {
                    Assembly idAssembly = Assembly.LoadFrom(assemblyName);
                    if (idAssembly != null)
                    {
                        Type type = idAssembly.GetType(enhancedConflictResolverClass);
                        if (null != type)
                        {
                            args[0] = ifWebService;
                            args[1] = Application.StartupPath;
                            args[2] = selectedItem;
                            System.Object enhancedConflictResolver = Activator.CreateInstance(type, args);
                            try
                            {
                                MethodInfo method = type.GetMethod(showConflictResolverMethod, Type.EmptyTypes);
                                if (null != method)
                                {
                                    method.Invoke(enhancedConflictResolver, null);
                                    return true;
                                }
                            }
                            catch (Exception e)
                            {
                                FormsTrayApp.log.Info("Message {0}, type {1}, trace {2} ", e.Message, e.GetType(), e.StackTrace);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FormsTrayApp.log.Info("Message {0}, type {1}, trace {2} ", ex.Message, ex.GetType(), ex.StackTrace);
            }
            return false;
        }

		private void menuResolve_Click(object sender, System.EventArgs e)
		{
            		if (!AdvancedConflictResolver(ifWebService, ((iFolderObject)selectedItem.Tag).iFolderWeb))
            		{
                		ConflictResolver conflictResolver = new ConflictResolver();
                		conflictResolver.iFolder = ((iFolderObject)selectedItem.Tag).iFolderWeb;
                		conflictResolver.iFolderWebService = ifWebService;
                		conflictResolver.LoadPath = Application.StartupPath;
                		conflictResolver.Show();
            		}
		}

		private void menuShare_Click(object sender, System.EventArgs e)
		{
			TileListViewItem selected = (TileListViewItem)selectedItem;
		        iFolderWeb curriFolder = ((iFolderObject)selected.Tag).iFolderWeb;

		        if (curriFolder.encryptionAlgorithm != null && curriFolder.encryptionAlgorithm != "")
            		{
		                MyMessageBox cannotShareDialog = new MyMessageBox(TrayApp.Properties.Resources.cannotShareMessage, TrayApp.Properties.Resources.cannotShareTitle, string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Warning);
                		cannotShareDialog.ShowDialog();
		        }
		        else
		        {
                	        invokeiFolderProperties( selectedItem, 1 );
            		}
		}

		private void menuSyncNow_Click(object sender, System.EventArgs e)
		{
            //TODO: check for null object
			synciFolder(((iFolderObject)selectedItem.Tag).iFolderWeb.ID);
		}

		private void menuProperties_Click(object sender, System.EventArgs e)
		{
			invokeiFolderProperties( selectedItem, 0 );
		}

		private void menuCreate_Click(object sender, System.EventArgs e)
		{
			// TODO: Need to update the CreateiFolder dialog.

			// Build the list of domains to pass in to the create dialog.
			ArrayList domains = new ArrayList();
            DomainInformation[] domainsInfo = simiasWebService.GetDomains(false);
            if( defaultDomainInfo == null && domainsInfo!= null && domainsInfo.Length > 0)
                defaultDomainInfo = domainsInfo[0];
			DomainItem selectedDomainItem = null;
            if( defaultDomainInfo != null)
                selectedDomainItem = new DomainItem( defaultDomainInfo.Name, defaultDomainInfo.ID );
            if (null != domainsInfo)
            {
                for (int i = 0; i < domainsInfo.Length; i++)
                {
                    DomainItem domainItem = new DomainItem(domainsInfo[i].Name, domainsInfo[i].ID, domainsInfo[i].Host, domainsInfo[i].HostUrl);
                    domains.Add(domainItem);
                }
            }

			CreateiFolder createiFolder = new CreateiFolder();
            try
            {
                createiFolder.Servers = domains;
                if (selectedDomainItem != null)
                    createiFolder.SelectedDomain = selectedDomainItem;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            //return;
			createiFolder.LoadPath = Application.StartupPath;
			createiFolder.iFolderWebService = ifWebService;
			createiFolder.simiasWebService = this.simiasWebService;

			if ((DialogResult.OK == createiFolder.ShowDialog()) && iFolderComponent.DisplayConfirmationEnabled)
			{
				new iFolderComponent().NewiFolderWizard(Application.StartupPath, createiFolder.iFolderPath);
			}
		}

		private void menuRefresh_Click(object sender, System.EventArgs e)
		{
            refreshAllInvoke();
		}

        private void refreshAllInvoke()
        {
            if (this.inRefresh == false)
                refreshAll(/*(Domain)servers.SelectedItem*/);
        }

		private void menuViewAccounts_Click(object sender, System.EventArgs e)
		{
			if (preferences.Visible)
			{
				preferences.Activate();
			}
			else
			{
				preferences.Show();
			}

			preferences.SelectAccounts(false);
		}

		private void menuViewLog_Click(object sender, System.EventArgs e)
		{
			if (syncLog.Visible)
			{
				syncLog.Activate();
			}
			else
			{
				syncLog.Show();
			}
		}

		private void menuEditPrefs_Click(object sender, System.EventArgs e)
		{
			if (preferences.Visible)
			{
				preferences.Activate();
			}
			else
			{
				preferences.Show();
			}

			preferences.SelectGeneral();
		}

        private void menuMerge_Click(object sender, System.EventArgs e)
        {
            DownloadOrMerge(true);
        }

		private void menuAccept_Click(object sender, System.EventArgs e)
		{
            DownloadOrMerge(false);
        }
        private void DownloadOrMerge( bool mergeFolder)
        {
			bool added;
			bool result;
			if ( selectedItem != null )
			{
				iFolderWeb ifolder = ((iFolderObject)selectedItem.Tag).iFolderWeb;
                
				iFolderObject ifobj = 
                    new iFolderObject(((iFolderObject)selectedItem.Tag).iFolderWeb, iFolderState.Disconnected);
				// Accept the iFolder.
                result = AcceptiFolder(ifolder, out added, mergeFolder);
				if (  result && added )
				{
					ifobj.iFolderWeb.IsSubscription = false;
					lock (ht)
					{
						removeTileListViewItem( selectedItem );					
                        iFolderObject ifolderobj = new iFolderObject(ifolder, iFolderState.Initial);
                        addiFolderToListView(ifolderobj);
                        if( acceptedFolders.Contains(ifobj.iFolderWeb.ID) )
							acceptedFolders.Remove(ifobj.iFolderWeb.ID);
						ifobj.iFolderWeb.UnManagedPath = DownloadPath;
						TileListViewItem tlvi = new TileListViewItem(ifobj);
						acceptedFolders.Add(ifobj.iFolderWeb.ID, tlvi);
                        ifolderobj = null;
                        tlvi = null;
					}
				}
				else if( result && !added )
				{
					// Add syncLog message
					syncLog.AddMessageToLog(DateTime.Now, 
                        string.Format("Download of iFolder \"{0}\" failed",ifobj.iFolderWeb.Name));
				}
			}
		}

        public void AddiFolderToAcceptediFolders(iFolderWeb ifolder, TileListViewItem selecteditem, String Path)
        {
            if (Path != null)
                DownloadPath = Path;
            if (selecteditem != null)
                removeTileListViewItem(selecteditem);
            if (ht.ContainsKey(ifolder.ID))
            {
                removeTileListViewItem((TileListViewItem)ht[ifolder.ID]);
            }
            iFolderObject ifolderobj = new iFolderObject(ifolder, iFolderState.Initial);
            addiFolderToListView(ifolderobj);
            if (acceptedFolders.Contains(ifolderobj.iFolderWeb.ID))
                acceptedFolders.Remove(ifolderobj.iFolderWeb.ID);
            ifolderobj.iFolderWeb.UnManagedPath = DownloadPath;
            TileListViewItem tlvi = new TileListViewItem(ifolderobj);
            acceptedFolders.Add(ifolderobj.iFolderWeb.ID, tlvi);
            ifolderobj = null;
            tlvi = null;
        }

		private void menuRemove_Click(object sender, System.EventArgs e)
		{
			if ( selectedItem != null )
			{
				iFolderWeb ifolder = ((iFolderObject)selectedItem.Tag).iFolderWeb;
				MyMessageBox mmb = null;
				if( ifolder.CurrentUserID == ifolder.OwnerID)
				{
					mmb = new Novell.iFolderCom.MyMessageBox(
						string.Format( TrayApp.Properties.Resources.deleteiFolder, ifolder.Name ),
						TrayApp.Properties.Resources.removeTitle,
						string.Empty,
						MyMessageBoxButtons.YesNo,
						MyMessageBoxIcon.Question,
						MyMessageBoxDefaultButton.Button2);
				}
				else
				{
					mmb = new Novell.iFolderCom.MyMessageBox(
						 TrayApp.Properties.Resources.RemoveMembershipMesg,
						string.Format(TrayApp.Properties.Resources.RemoveMembershipTitle, ifolder.Name ),
						string.Empty,
						MyMessageBoxButtons.YesNo,
						MyMessageBoxIcon.Question,
						MyMessageBoxDefaultButton.Button2);
				}
				if (mmb.ShowDialog() == DialogResult.Yes)
                {
					try
					{
                        Invalidate();
                        Update();
                        
						// check whether the user is the owner...
						//string userID = this.ifWebService.GetUserID(ifolder.DomainID);
						if( ifolder.CurrentUserID == ifolder.OwnerID)
						{
							ifWebService.DeleteiFolder(ifolder.DomainID, ifolder.ID);
						}
						ifWebService.DeclineiFolderInvitation(ifolder.DomainID, ifolder.ID);
                        

						lock (ht)
						{
							removeTileListViewItem( selectedItem );
						}
					}
					catch (Exception ex)
					{
						mmb = new MyMessageBox(TrayApp.Properties.Resources.declineError, TrayApp.Properties.Resources.errorTitle, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
					}
				}

			mmb.Dispose();
			}
		}

		private void showiFolders_Click(object sender, System.EventArgs e)
		{
            hide = !hide;
            menuViewAvailable.Checked = !hide;

            // Hide the server listview controls.
            foreach (iFoldersListView ifListView in iFolderListViews.Values)
            {
                if (!hide == true)
                {
                    if (ifListView.DomainInfo.Authenticated)
                        ifListView.Visible = true;
                    else
                        ifListView.Visible = false;
                }
                else
                    ifListView.Visible = !hide;
            }
		}

		private void iFolderView_LastItemRemoved(object sender, System.EventArgs e)
		{
			updateView();
		}

		private bool iFolderView_NavigateItem(object sender, Novell.FormsTrayApp.NavigateItemEventArgs e)
		{
			bool result = false;

			// Select the appropriate TileListViewItem object in the listview
			// above or below the current listview.
			if ( !hide && iFolderListViews.Count > 0 )
			{
				int index = panel2.Controls.GetChildIndex( sender as Control );
				
				try
				{
					switch ( e.Direction )
					{
						case MoveDirection.Down:
							if ( index == 1 )
							{
								((iFoldersListView)panel2.Controls[3]).MoveToItem( e.Row, e.Column );
							}
							else if ( index != panel2.Controls.Count - 1 )
							{
								((iFoldersListView)panel2.Controls[index+1]).MoveToItem( e.Row, e.Column );
							}
							break;
						case MoveDirection.Right:
							if ( index == 1 ) // iFolders on this computer
							{
								((iFoldersListView)panel2.Controls[3]).MoveToItem( 0, e.Column );
							}
							else if ( index == 3 )
							{
								((TileListView)panel2.Controls[1]).MoveToItem( -1, e.Column );
							}
							else
							{
								((iFoldersListView)panel2.Controls[index-1]).MoveToItem( -1, e.Column );
							}
							break;
						case MoveDirection.Up:
							if ( index == 3 )
							{
								((TileListView)panel2.Controls[1]).MoveToItem( e.Row, e.Column );
							}
							else if ( index != 1 )
							{
								((iFoldersListView)panel2.Controls[index-1]).MoveToItem( e.Row, e.Column );
							}
							break;
					}
				}
				catch {} // Ignore.
			}

			return result;
		}

		private void iFolderView_DoubleClick(object sender, System.EventArgs e)
		{
			if ( selectedItem != null )
			{
				iFolderWeb ifolder = ((iFolderObject)selectedItem.Tag).iFolderWeb;
				if (ifolder.IsSubscription)
				{
					if (ifolder.State.Equals("Available") || ifolder.State.Equals("WaitSync"))
					{
						menuAccept_Click(sender, e);
					}
				}
				else
				{
					menuOpen_Click(sender, e);
				}
			}
		}

		#region Sync Event Handlers
		private void global_collectionSyncHandler(SimiasEventArgs args)
		{
			try
			{
				CollectionSyncEventArgs syncEventArgs = args as CollectionSyncEventArgs;
				BeginInvoke(syncCollectionDelegate, new object[] {syncEventArgs});
			}
			catch {}
		}

		private void global_fileSyncHandler(SimiasEventArgs args)
		{
			try
			{
				FileSyncEventArgs syncEventArgs = args as FileSyncEventArgs;
				BeginInvoke(syncFileDelegate, new object[] {syncEventArgs});
			}
			catch {}
		}
		#endregion

		#endregion

		private const int WM_QUERYENDSESSION = 0x0011;

		/// <summary>
		/// Override of WndProc method.
		/// </summary>
		/// <param name="m">The message to process.</param>
		protected override void WndProc(ref Message m)
		{
			// Keep track if we receive a shutdown message.
			switch (m.Msg)
			{
				case WM_QUERYENDSESSION:
				{
#if DEBUG					
					MessageBox.Show("Shutdown msg got - GP");
#endif
					this.shutdown = true;
					break;
				}
			}

			base.WndProc (ref m);
		}

		#region Win32

		private const uint DRIVE_REMOVABLE = 2;
		private const uint DRIVE_FIXED = 3;

		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		private static extern uint GetDriveType(string rootPathName);

		#endregion

		private void menuMigrateMigrate_Click(object sender, EventArgs e)
		{
			Novell.FormsTrayApp.MigrationWindow migrationWindow = new MigrationWindow(this.ifWebService, this.simiasWebService);
			migrationWindow.ShowDialog();
		}
		
		private void menuResetPassphrase_Select(object sender, EventArgs e)
		{
			// Show the reset passphrase window
			ResetPassphrase resetPassphraseWindow = new ResetPassphrase(this.simiasWebService,this.ifWebService);
			if( resetPassphraseWindow.DomainCount >0)
				resetPassphraseWindow.ShowDialog();
			else
			{
				System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(FormsTrayApp));
                Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(Resource.GetString("NoLoggedInDomainsText")/*"There are no logged-in domains for changing passphrase. For changing passphrase the domain should be connected. Log on to a domain and try."*/, Resource.GetString("ResetError")/*"Reset passphrase error"*/, "", MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);                
				mmb.ShowDialog();
				mmb.Dispose();
			}
		}

		private void menuExportKeys_Select(object sender, EventArgs e)
		{
			// Show export keys dialog
			ExportKeysDialog exportKeys = new ExportKeysDialog(this.ifWebService, this.simiasWebService);
			exportKeys.ShowDialog();
		}

		private void menuImportKeys_Select(object sender, EventArgs e)
		{
			ImportKeysDialog importKeys = new ImportKeysDialog(this.simiasWebService,this.ifWebService);
			if( importKeys.DomainCount > 0)
				importKeys.ShowDialog();
			else
			{
				System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(FormsTrayApp));
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(Resource.GetString("NoLoggedInDomainsTextForImport")/*"There are no logged-in domains for changing passphrase. For changing passphrase the domain should be connected. Log on to a domain and try."*/, Resource.GetString("ImportKeysError")/*"Reset passphrase error"*/, "", MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
				mmb.Dispose();
			}
		}
		
		private void menuClose_Click(object sender, System.EventArgs e)
		{
			Hide();
		}

        private void toolStripBtnFilter_TextUpdate(object sender, EventArgs e)
        {
            if (this.toolStripBtnFilter.Focused)
            {
                this.searchTimer.Stop();
                this.searchTimer.Start();
            }
        }

		private void searchTimer_Tick(object sender, EventArgs e)
		{
			// The timer event has been fired...
			this.inRefresh = false;			
			// Stop the timer.
			searchTimer.Stop();
			refreshiFolders(this.toolStripBtnFilter.Text);
		}

		private void refreshTimer_Tick(object sender, EventArgs e)
		{
			this.refreshAll();
			this.refreshTimer.Stop();
			this.refreshTimer.Start();
		}
 
        
        /// <summary>
        /// This function calculate the DiskQuota for the given Domain using MemberID.
        /// <param name="usermemberid">Domain MemberId of currently highlighted/Selected Domain in ComboBox</param>
        public string CalculateDiskQuota(string usermemberid)
        {
            string id = usermemberid;
            string QuotaAvailable = null;
            string QuotaUsed = null;
            string QuotaInfo = null;
            
            try
            {
                DiskSpace diskSpace = ifWebService.GetUserDiskSpace(id);
                QuotaAvailable = calcAvailableQuota(id);

                QuotaUsed = calcUsedQuota(id);
                //combining both the Available and Used Space in single string
                QuotaInfo = string.Format(Resources.diskSpaceAvailable +
                                        ":  " +
                                        QuotaAvailable +
                                        "            " +
                                        Resources.diskSpaceUsed +
                                        ":  " +
                                        QuotaUsed);
            }
            catch
            {
                QuotaAvailable = null;
                QuotaUsed = null;
                QuotaAvailable = TrayApp.Properties.Resources.notApplicable;
                QuotaAvailable = string.Format( Resources.diskSpaceAvailable + QuotaAvailable);
                QuotaUsed = TrayApp.Properties.Resources.notApplicable;
                QuotaUsed = string.Format(Resources.diskSpaceUsed + QuotaUsed);
                //combining both the Available and Used Space in single string
                QuotaInfo = string.Format(Resources.diskSpaceAvailable +
                                        ":  " +
                                        QuotaAvailable +
                                        "            " +
                                        Resources.diskSpaceUsed +
                                        ":  " +
                                        QuotaUsed);
            }
            return QuotaInfo;
        }

        private string calcUsedQuota(string usermemberid)
        {
            DiskSpace diskSpace = ifWebService.GetUserDiskSpace(usermemberid);
            string QuotaUsed = null;
            //Calculating Used Space
            if (diskSpace.UsedSpace != -1)
            {
                QuotaUsed = ((double)Math.Round(diskSpace.UsedSpace / megaByte, 2)).ToString();
                QuotaUsed += string.Format(" " + Resources.mb);
            }
            else
            {
                QuotaUsed = Resources.notApplicalbeText;

            }
            return QuotaUsed;
        }

        private string calcAvailableQuota(string usermemberid)
        {
            DiskSpace diskSpace = ifWebService.GetUserDiskSpace(usermemberid);
            string QuotaAvailable = null;
            //Calculating Available Space
            if (diskSpace.Limit != -1)
            {
                QuotaAvailable = ((double)Math.Round(diskSpace.AvailableSpace / megaByte, 2)).ToString();
                QuotaAvailable += string.Format(" " + TrayApp.Properties.Resources.freeSpaceUnits);
            }
            else
            {
                QuotaAvailable = Resources.Unlimited;
            }
            return QuotaAvailable;
        }


        /// <summary>
        ///This function clear and then update the ComboBox with latest Domain details
        /// </summary>
        /// <param name="none"></param>
        private void DomainsListUpdate()
        {
            string DomainName = null;
            DomainInformation[] domains;
            //Reading and Initilizing the Domain List.
            domains = this.simiasWebService.GetDomains(false);
            foreach (DomainInformation dw in domains)
            {
                try
                {
                    if (domains != null)
                    {
                        foreach (iFoldersListView ifListView in iFolderListViews.Values)
                        {
                            if (ifListView.DomainInfo.Host == dw.Host)
                            {
                                ifListView.updateqouta(CalculateDiskQuota(dw.MemberUserID));
                                break;
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                catch { }
            }
        }

        private void DomainsListUpdateComboBox()
        {
            int domaincount = 0;
            string DomainName = null;
            DomainInformation[] domains;
            //Reading and Initilizing the Domain List.
            domains = this.simiasWebService.GetDomains(false);
            LoginLogoff.Enabled = domains.Length > 0 ? true:false;
            domainListComboBox.Items.Clear();
            foreach (DomainInformation dw in domains)
            {
                try
                {
                    if (domains != null)
                    {
                        domainListComboBox.Items.Add(dw.Name);
                    }
                }
                catch { }
            }
            if (comboBoxSelectedIndex >= 0)
            {
                if (comboBoxSelectedIndex > (domaincount - 1))
                {
                    if( domainListComboBox.Items.Count >= 1)
                        domainListComboBox.SelectedIndex = 0;
                    comboBoxSelectedIndex = 0;
                }
                else
                {
                    domainListComboBox.SelectedIndex = comboBoxSelectedIndex;
                }
            }
            else
            {
                if (domainListComboBox.Items.Count >= 1)
                    domainListComboBox.SelectedIndex = 0;
                comboBoxSelectedIndex = 0;
            }
        }
        private void serverListComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DomainInformation[] domains;
            //Reading and Initilizing the Domain List.
            domains = this.simiasWebService.GetDomains(false);
            int index = 0;
            foreach (DomainInformation dw in domains)
            {
                try
                {
                    if (index == ((ComboBox)sender).SelectedIndex)
                    {
                        selectedDomain = dw;
                        updateDomainInfo(dw);
                        setAuthState(dw);
                        break;
                    }
                    index++;
                }
                catch { }
            } 
        }

        private void updateDomainInfo(DomainInformation dw)
        {
            titleUser.Text = TrayApp.Properties.Resources.user + ":  " + dw.MemberName;
            titleServer.Text = TrayApp.Properties.Resources.server + ":  " + dw.Host;
            titleAvailable.Text = TrayApp.Properties.Resources.availableQuota + ":  " +
                                                calcAvailableQuota(dw.MemberUserID);
            titleUsed.Text = TrayApp.Properties.Resources.usedQuota + ":  " +
                                                calcUsedQuota(dw.MemberUserID);
            titleNOFolders.Text = TrayApp.Properties.Resources.noifolder + ":  " +
                                                this.ifWebService.GetiFoldersForDomain(dw.ID).Length;
        }
        private void updateDomainInfo(string strValue)
        {
            titleUser.Text = TrayApp.Properties.Resources.user + ":  " + strValue;
            titleServer.Text = TrayApp.Properties.Resources.server + ":  " + strValue;
            titleAvailable.Text = TrayApp.Properties.Resources.availableQuota + ":  " + strValue;
            titleUsed.Text = TrayApp.Properties.Resources.usedQuota + ":  " + strValue;
            titleNOFolders.Text = TrayApp.Properties.Resources.noifolder + ":  " + strValue;
        }
        private void setAuthState(DomainInformation dw)
        {
            if (dw.Authenticated)
            {
                LoginLogoff.Text = TrayApp.Properties.Resources.logoff;
                pictureBox1.Image = new Bitmap(Path.Combine(Application.StartupPath, @"res\ifolder_connect_128.png"));
            }
            else
            {
                LoginLogoff.Text = TrayApp.Properties.Resources.login;
                pictureBox1.Image = new Bitmap(Path.Combine(Application.StartupPath, @"res\ifolder_discon_128.png"));
            }
        }
        
        private void LoginLogoff_Click(object sender, EventArgs e)
        {
            if (simiasWebService.GetDomainInformation(selectedDomain.ID).Authenticated)
            {
                preferences.logoutFromDomain(new Domain(selectedDomain));
                //This changes is only added in case of Logout, as in case of Login
                //Listview will be update only after login completed event. 
                if (!thumbnailView)
                {
                    showiFolderinListView();
                }
            }
            else
            {
                preferences.loginToDomain(new Domain(selectedDomain));
            }
            setAuthState(simiasWebService.GetDomainInformation(selectedDomain.ID));
            
        }

        private void setThumbnailView(bool value)
        {
           updateChangeViewMenuText();
            panel2.SuspendLayout();
            //show/hide panel2 that contains thumnail view.
            this.iFolderView.Visible = value;
            this.localiFoldersHeading.Visible = value;
            panel2.Visible = value;

            //show/hide panel2 that contains detial view.
            panel1.Visible = !value;
            listView1.Visible = !value;

            //ensure UI is re-painted
            //Invalidate();
            this.iFolderView.Items.Sort();
            updateView();
            //panel2.ResumeLayout();

            Update();
             //need to refresh when the view change from list to thumbnail
             //and set focus on listview when changed to listview.
        
             if (!value)
                 setListViewItemSelected();
             //    refreshAllInvoke();
           panel2.ResumeLayout();
           
         
            
          }
        private void showiFolderinListView()
        {
            listView1.SuspendLayout();
            //do not want to refresh when in other view
            if (thumbnailView) return; 
            listView1.Items.Clear();
            // Walk the list of iFolders and add them to the listviews.
            if (ht != null)
            {
                foreach (DictionaryEntry entry in ht)
                {
                    TileListViewItem tlvi = (TileListViewItem)entry.Value;
                    
                    iFolderObject ifObj = (iFolderObject)tlvi.Tag;
                    int imageIndex;
                    tlvi.Status = getItemState(ifObj, 0, out imageIndex);
                    //Fix: Getting random exception while removing last domain when ifolder is syncing.
                    if ((simiasWebService.GetDomainInformation(ifObj.iFolderWeb.DomainID)) == null)
                    {
                        //don't perform any action
                        continue;
                    }
                    string state = null;

                    if (ifObj.iFolderState == iFolderState.RevertAndDelete)
                        state = Resources.deletionInProgress;
                    else
                        state = ifObj.iFolderWeb.IsSubscription ? Resources.availablefordownload : tlvi.Status;

                    ListViewItem listViewItem1 = new ListViewItem(
                    new string[] { 
                    ifObj.iFolderWeb.Name,
                    FormatSize(ifObj.iFolderWeb.iFolderSize),
                    (simiasWebService.GetDomainInformation(ifObj.iFolderWeb.DomainID)).Host,
                    state,
                    ifObj.iFolderWeb.ID},
                    imageIndex,
                    Color.Empty,
                    Color.Empty,
                    new Font("Microsoft Sans Serif", 12.25F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0))));
                    listView1.Items.Add(listViewItem1);
                }
            }
            setListViewItemSelected();
            listView1.ResumeLayout();
        }

        private void toolStripMenuThumbnails_Click(object sender, EventArgs e)
        {
            thumbnailView = !thumbnailView;
            if (!thumbnailView)
            {
                showiFolderinListView();
            }
            setThumbnailView(thumbnailView);
        }

        private void updateChangeViewMenuText()
        {
            if (thumbnailView)
                toolStripMenuThumbnails.Text = Resources.details;
            else
                toolStripMenuThumbnails.Text = Resources.thumbnails;
        }

        private void toolStripMenuLeftPane_Click(object sender, EventArgs e)
        {
          
            if (panel3.Visible)
            {
                panel3.Visible = !panel3.Visible;

                panel1.Left = panel3.Left;
                panel2.Left = panel3.Left;
                tableLayoutPanel1.Left = panel3.Left;
                 
                panel1.Width = panel1.Width + panel3.Width;
                panel2.Width = panel2.Width + panel3.Width;
            }
            else
            {
                panel3.Visible = !panel3.Visible;

                panel1.Left = panel3.Right;
                panel2.Left = panel3.Right;
                tableLayoutPanel1.Left = panel3.Right;

                panel1.Width = panel1.Width - panel3.Width;
                panel2.Width = panel2.Width - panel3.Width;

            }
            tableLayoutPanel1.Width = panel1.Width;
            //first 'ifolder on iFolder' label was getting truncated some times so reseting  iFolderView hieght
            this.iFolderView.ItemHeight = 72;
        }
        
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (listView1.Sorting == SortOrder.Descending)
                listView1.Sorting = SortOrder.Ascending;
            else
                listView1.Sorting = SortOrder.Descending;
            listView1.Sort();
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            // below check is done as this event is generated twice
            if (listView1.SelectedItems == null || listView1.SelectedItems.Count == 0)
            {
                if (selectedItem != null && selectedItem.Selected  )
                {
                    selectedItem.Selected = false;
                    selectedItem = null;
                }
                updateMenus(null);
                return;
            }
            string id = listView1.Items[listView1.SelectedIndices[0]].SubItems[4].Text.ToString();
            TileListViewItem tlvi = (TileListViewItem)ht[id];
            tlvi.Selected = true;
        }
        private void setListViewItemSelected()
        {
            if (listView1 != null && selectedItem != null)
            {
                listView1.Focus();
                iFolderObject ifObj = (iFolderObject)selectedItem.Tag;
                ListViewItem item = listView1.FindItemWithText(ifObj.iFolderWeb.ID);
                listView1.Items[item.Index].Focused = true;
                listView1.Items[item.Index].Selected = true;
            }
        }

        //TODO: picked from WebUtils.cs need to find a better place to keep these 
        // utility methods so that these could be used everywhere.
        public static string FormatSize(long size)
        {
            const int K = 1024;

            string modifier = "";

            double temp;
            double tempsize = (double)size;
            int index = 0;

            // adjust
            while ((index < (int)Index.GB) && ((temp = ((double)tempsize / (double)K)) > 1))
            {
                ++index;
                tempsize = temp;
            }
            // modifier
            switch ((Index)index)
            {
                // B
                case Index.B:
                    modifier = Resources.b;
                    break;

                // KB
                case Index.KB:
                    modifier = Resources.kb;
                    break;

                // MB
                case Index.MB:
                    modifier = Resources.mb;
                    break;

                // GB
                case Index.GB:
                    modifier = Resources.gb;
                    break;
            }

            return String.Format("{0}{1}", Math.Round(tempsize,0), modifier);
        }

        private void menuRecoverKeys_Click(object sender, EventArgs e)
        {
            KeyRecoveryWizard kr = new KeyRecoveryWizard(this.ifWebService, this.simiasWebService, this.simiasManager);
            if (kr.GetLoggedInDomains() == true)
            {
                kr.ShowDialog();
            }
            else
            {
                System.Resources.ResourceManager Resource =
                    new System.Resources.ResourceManager(typeof(FormsTrayApp));
                Novell.iFolderCom.MyMessageBox mmb =
                    new MyMessageBox(Resource.GetString("NoLoggedInDomainsText"),
                        Resource.GetString("ResetError"),
                        "",
                        MyMessageBoxButtons.OK,
                        MyMessageBoxIcon.Error);
                mmb.ShowDialog();
                mmb.Dispose();
            }
        }

        public string FormatString(string original, int displaySize)
        {
            string formated = original;
            if (original.Length > 20)
                formated = original.Substring(0, displaySize) + "...";

            return formated;
        }

        private void menuHelpUpgrade_Click(object sender, EventArgs e)
        {
            bool upgradeAvailable = false;
            bool serverAvailable = false;
            bool oldClient = false;
            string serverVersion = null;
            Novell.FormsTrayApp.UpdateResult update = UpdateResult.Unknown;
            
            DomainInformation[] domains;
            Novell.iFolderCom.MyMessageBox mmb = null;
            System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(FormsTrayApp));

            try
            {
                domains = this.simiasWebService.GetDomains(false);
                foreach (DomainInformation dw in domains)
                {
                    if (dw.Authenticated)
                    {
                        serverAvailable = true;
                        update = (UpdateResult)ifWebService.CheckForUpdate(dw.ID, out serverVersion);
                        if (update != UpdateResult.Latest)
                        {
                            oldClient = true;
                            //Iterate over all the added domain
                            FormsTrayApp.ClientUpdates(dw.ID, out upgradeAvailable);
                        }
                    }
                }

                
                if (!serverAvailable)
                {
                    mmb = new MyMessageBox(Resource.GetString("upgradeServerUnavailable"),
                        Resource.GetString("clientUpgradeTitle"),
                        "",
                        MyMessageBoxButtons.OK,
                        MyMessageBoxIcon.Information);
                    
                }
                else if (!oldClient)
                {
                    mmb = new MyMessageBox(Resource.GetString("upgradeUnavailable"),
                        Resource.GetString("clientUpgradeTitle"),
                        "",
                        MyMessageBoxButtons.OK,
                        MyMessageBoxIcon.Information);
                }
                
            }
            catch {
                mmb = new MyMessageBox(Resource.GetString("operationIncomplete"),
                        Resource.GetString("clientUpgradeTitle"),
                        "",
                        MyMessageBoxButtons.OK,
                        MyMessageBoxIcon.Error);
            }

            if (mmb != null)
            {
                mmb.ShowDialog();
                mmb.Dispose();
            }
        }
      }
	}


