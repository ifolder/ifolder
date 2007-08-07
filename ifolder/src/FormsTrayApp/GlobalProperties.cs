/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

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

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for GlobalProperties.
	/// </summary>
	public class GlobalProperties : System.Windows.Forms.Form
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
		private string domainList;
		private System.Windows.Forms.MenuItem menuOpen;
		private System.Windows.Forms.MenuItem menuShare;
		private System.Windows.Forms.MenuItem menuRevert;
		private System.Windows.Forms.MenuItem menuProperties;
		private System.Windows.Forms.MenuItem menuSyncNow;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuAction;
		private System.Windows.Forms.MenuItem menuView;
		private System.Windows.Forms.MenuItem menuViewRefresh;
		private System.Windows.Forms.MenuItem menuActionOpen;
		private System.Windows.Forms.MenuItem menuActionCreate;
		private System.Windows.Forms.MenuItem menuActionRevert;
		private System.Windows.Forms.MenuItem menuActionShare;
		private System.Windows.Forms.MenuItem menuActionSync;
		private System.Windows.Forms.MenuItem menuActionProperties;
		private System.Windows.Forms.MenuItem menuResolve;
		private System.Windows.Forms.MenuItem menuActionResolve;
		private System.Windows.Forms.MenuItem menuActionAccept;
		private System.Windows.Forms.MenuItem menuExit;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuHelp;
		private System.Windows.Forms.MenuItem menuHelpHelp;
		private System.Windows.Forms.MenuItem menuHelpAbout;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.MenuItem menuActionRemove;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuViewAccounts;
		private System.Windows.Forms.MenuItem menuViewLog;
		private System.Windows.Forms.MenuItem menuEdit;
		private System.Windows.Forms.MenuItem menuEditPrefs;
		private System.Windows.Forms.MenuItem menuSecurity;
		private System.Windows.Forms.MenuItem menuResetPassphrase;
		private System.Windows.Forms.MenuItem menuRecoverKeys;
		private System.Windows.Forms.MenuItem menuImportKeys;
		private System.Windows.Forms.MenuItem menuExportKeys;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.RichTextBox localiFoldersHeading;
		private System.Windows.Forms.Button showiFolders;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox filter;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button create;
		private Novell.FormsTrayApp.TileListView iFolderView;
		private System.Windows.Forms.Panel iFolderActions;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button open;
		private System.Windows.Forms.Button syncNow;
		private System.Windows.Forms.Button share;
		private System.Windows.Forms.Button revert;
		private System.Windows.Forms.Button properties;
		private System.Windows.Forms.MenuItem menuClose;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuViewAvailable;
		private System.Windows.Forms.ContextMenu iFolderContextMenu;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem menuSeparator;
		private System.Windows.Forms.MenuItem MigrationMenuItem;
		private System.Windows.Forms.MenuItem MigrationMenuSubItem;
		private System.Windows.Forms.Button accept;
		private System.Windows.Forms.Button remove;
		private System.Windows.Forms.Button resolve;
		private System.Windows.Forms.MenuItem menuResolveSeparator;
		private System.Windows.Forms.MenuItem menuSeparator1;
		private System.Windows.Forms.MenuItem menuSeparator2;
		private System.Windows.Forms.MenuItem menuAccept;
		private System.Windows.Forms.MenuItem menuRemove;
		private System.Windows.Forms.MenuItem menuRefresh;
		private System.ComponentModel.IContainer components;
		private string DownloadPath;

		private System.Windows.Forms.Timer searchTimer;
		private System.Windows.Forms.Timer refreshTimer;
		#endregion

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

			infoMessage = new NoiFolderMessage();
			panel2.Controls.Add( infoMessage );
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
				largeImageList.ImageSize = new Size( 48, 48 );
				largeImageList.ColorDepth = ColorDepth.Depth32Bit;
				largeImageList.TransparentColor = Color.Black;
				largeImageList.Images.Add( Bitmap.FromFile( Path.Combine( Application.StartupPath, @"res\ifolder48.png" ) ) );
				largeImageList.Images.Add( Bitmap.FromFile( Path.Combine( Application.StartupPath, @"res\ifolder-sync48.png" ) ) );
				largeImageList.Images.Add( Bitmap.FromFile( Path.Combine( Application.StartupPath, @"res\ifolder-download48.png" ) ) );
				largeImageList.Images.Add( Bitmap.FromFile( Path.Combine( Application.StartupPath, @"res\ifolder-upload48.png" ) ) );
				largeImageList.Images.Add( Bitmap.FromFile( Path.Combine( Application.StartupPath, @"res\ifolder-waiting48.png" ) ) );
				largeImageList.Images.Add( Bitmap.FromFile( Path.Combine( Application.StartupPath, @"res\ifolder-warning48.png" ) ) );
				largeImageList.Images.Add( Bitmap.FromFile( Path.Combine( Application.StartupPath, @"res\ifolder-error48.png" ) ) );
				largeImageList.Images.Add( Bitmap.FromFile( Path.Combine( Application.StartupPath, @"res\encrypt_ilock_48.gif" ) ) );
				largeImageList.Images.Add( Bitmap.FromFile( Path.Combine( Application.StartupPath, @"res\ifolder_user_48.png" ) ) );

				iFolderView.LargeImageList = largeImageList;

				// TODO: load hot icons.
			}
			catch {} // Non-fatal ... just missing some graphics.

			this.MinimumSize = this.Size;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(GlobalProperties));
			this.iFolderContextMenu = new System.Windows.Forms.ContextMenu();
			this.menuOpen = new System.Windows.Forms.MenuItem();
			this.menuResolveSeparator = new System.Windows.Forms.MenuItem();
			this.menuResolve = new System.Windows.Forms.MenuItem();
			this.menuAccept = new System.Windows.Forms.MenuItem();
			this.menuSeparator1 = new System.Windows.Forms.MenuItem();
			this.menuRemove = new System.Windows.Forms.MenuItem();
			this.menuSyncNow = new System.Windows.Forms.MenuItem();
			this.menuShare = new System.Windows.Forms.MenuItem();
			this.menuRevert = new System.Windows.Forms.MenuItem();
			this.menuSeparator2 = new System.Windows.Forms.MenuItem();
			this.menuProperties = new System.Windows.Forms.MenuItem();
			this.menuRefresh = new System.Windows.Forms.MenuItem();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuAction = new System.Windows.Forms.MenuItem();
			this.menuActionCreate = new System.Windows.Forms.MenuItem();
			this.menuActionAccept = new System.Windows.Forms.MenuItem();
			this.menuActionRemove = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.menuSeparator = new System.Windows.Forms.MenuItem();
			this.MigrationMenuItem = new System.Windows.Forms.MenuItem();
			this.MigrationMenuSubItem = new System.Windows.Forms.MenuItem();
			this.menuActionOpen = new System.Windows.Forms.MenuItem();
			this.menuActionShare = new System.Windows.Forms.MenuItem();
			this.menuActionResolve = new System.Windows.Forms.MenuItem();
			this.menuActionSync = new System.Windows.Forms.MenuItem();
			this.menuActionRevert = new System.Windows.Forms.MenuItem();
			this.menuActionProperties = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.menuClose = new System.Windows.Forms.MenuItem();
			this.menuExit = new System.Windows.Forms.MenuItem();
			this.menuEdit = new System.Windows.Forms.MenuItem();
			this.menuViewAccounts = new System.Windows.Forms.MenuItem();
			this.menuEditPrefs = new System.Windows.Forms.MenuItem();
			this.menuSecurity = new MenuItem();
			this.menuResetPassphrase = new MenuItem();
			this.menuRecoverKeys = new MenuItem();
			this.menuImportKeys = new MenuItem();
			this.menuExportKeys = new MenuItem();
			this.menuView = new System.Windows.Forms.MenuItem();
			this.menuViewRefresh = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuViewLog = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.menuViewAvailable = new System.Windows.Forms.MenuItem();
			this.menuHelp = new System.Windows.Forms.MenuItem();
			this.menuHelpHelp = new System.Windows.Forms.MenuItem();
			this.menuHelpAbout = new System.Windows.Forms.MenuItem();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.iFolderActions = new System.Windows.Forms.Panel();
			this.resolve = new System.Windows.Forms.Button();
			this.remove = new System.Windows.Forms.Button();
			this.accept = new System.Windows.Forms.Button();
			this.properties = new System.Windows.Forms.Button();
			this.revert = new System.Windows.Forms.Button();
			this.share = new System.Windows.Forms.Button();
			this.syncNow = new System.Windows.Forms.Button();
			this.open = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.create = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.filter = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.showiFolders = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.iFolderView = new TileListView();
			this.localiFoldersHeading = new System.Windows.Forms.RichTextBox();
			this.searchTimer = new System.Windows.Forms.Timer(this.components);
			this.refreshTimer = new System.Windows.Forms.Timer(this.components);
			this.panel1.SuspendLayout();
			this.iFolderActions.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// iFolderContextMenu
			// 
			this.iFolderContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							   this.menuOpen,
																							   this.menuResolveSeparator,
																							   this.menuResolve,
																							   this.menuAccept,
																							   this.menuSeparator1,
																							   this.menuRemove,
																							   this.menuSyncNow,
																							   this.menuShare,
																							   this.menuRevert,
																							   this.menuSeparator2,
																							   this.menuProperties,
																							   this.menuRefresh});
			this.iFolderContextMenu.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("iFolderContextMenu.RightToLeft")));
			this.iFolderContextMenu.Popup += new System.EventHandler(this.iFolderContextMenu_Popup);
			// 
			// menuOpen
			// 
			this.menuOpen.DefaultItem = true;
			this.menuOpen.Enabled = ((bool)(resources.GetObject("menuOpen.Enabled")));
			this.menuOpen.Index = 0;
			this.menuOpen.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuOpen.Shortcut")));
			this.menuOpen.ShowShortcut = ((bool)(resources.GetObject("menuOpen.ShowShortcut")));
			this.menuOpen.Text = resources.GetString("menuOpen.Text");
			this.menuOpen.Visible = ((bool)(resources.GetObject("menuOpen.Visible")));
			this.menuOpen.Click += new System.EventHandler(this.menuOpen_Click);
			// 
			// menuResolveSeparator
			// 
			this.menuResolveSeparator.Enabled = ((bool)(resources.GetObject("menuResolveSeparator.Enabled")));
			this.menuResolveSeparator.Index = 1;
			this.menuResolveSeparator.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuResolveSeparator.Shortcut")));
			this.menuResolveSeparator.ShowShortcut = ((bool)(resources.GetObject("menuResolveSeparator.ShowShortcut")));
			this.menuResolveSeparator.Text = resources.GetString("menuResolveSeparator.Text");
			this.menuResolveSeparator.Visible = ((bool)(resources.GetObject("menuResolveSeparator.Visible")));
			// 
			// menuResolve
			// 
			this.menuResolve.Enabled = ((bool)(resources.GetObject("menuResolve.Enabled")));
			this.menuResolve.Index = 2;
			this.menuResolve.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuResolve.Shortcut")));
			this.menuResolve.ShowShortcut = ((bool)(resources.GetObject("menuResolve.ShowShortcut")));
			this.menuResolve.Text = resources.GetString("menuResolve.Text");
			this.menuResolve.Visible = ((bool)(resources.GetObject("menuResolve.Visible")));
			this.menuResolve.Click += new System.EventHandler(this.menuResolve_Click);
			// 
			// menuAccept
			// 
			this.menuAccept.DefaultItem = true;
			this.menuAccept.Enabled = ((bool)(resources.GetObject("menuAccept.Enabled")));
			this.menuAccept.Index = 3;
			this.menuAccept.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuAccept.Shortcut")));
			this.menuAccept.ShowShortcut = ((bool)(resources.GetObject("menuAccept.ShowShortcut")));
			this.menuAccept.Text = resources.GetString("menuAccept.Text");
			this.menuAccept.Visible = ((bool)(resources.GetObject("menuAccept.Visible")));
			this.menuAccept.Click += new System.EventHandler(this.menuAccept_Click);
			// 
			// menuSeparator1
			// 
			this.menuSeparator1.Enabled = ((bool)(resources.GetObject("menuSeparator1.Enabled")));
			this.menuSeparator1.Index = 4;
			this.menuSeparator1.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuSeparator1.Shortcut")));
			this.menuSeparator1.ShowShortcut = ((bool)(resources.GetObject("menuSeparator1.ShowShortcut")));
			this.menuSeparator1.Text = resources.GetString("menuSeparator1.Text");
			this.menuSeparator1.Visible = ((bool)(resources.GetObject("menuSeparator1.Visible")));
			// 
			// menuRemove
			// 
			this.menuRemove.Enabled = ((bool)(resources.GetObject("menuRemove.Enabled")));
			this.menuRemove.Index = 5;
			this.menuRemove.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuRemove.Shortcut")));
			this.menuRemove.ShowShortcut = ((bool)(resources.GetObject("menuRemove.ShowShortcut")));
			this.menuRemove.Text = resources.GetString("menuRemove.Text");
			this.menuRemove.Visible = ((bool)(resources.GetObject("menuRemove.Visible")));
			this.menuRemove.Click += new System.EventHandler(this.menuRemove_Click);
			// 
			// menuSyncNow
			// 
			this.menuSyncNow.Enabled = ((bool)(resources.GetObject("menuSyncNow.Enabled")));
			this.menuSyncNow.Index = 6;
			this.menuSyncNow.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuSyncNow.Shortcut")));
			this.menuSyncNow.ShowShortcut = ((bool)(resources.GetObject("menuSyncNow.ShowShortcut")));
			this.menuSyncNow.Text = resources.GetString("menuSyncNow.Text");
			this.menuSyncNow.Visible = ((bool)(resources.GetObject("menuSyncNow.Visible")));
			this.menuSyncNow.Click += new System.EventHandler(this.menuSyncNow_Click);
			// 
			// menuShare
			// 
			this.menuShare.Enabled = ((bool)(resources.GetObject("menuShare.Enabled")));
			this.menuShare.Index = 7;
			this.menuShare.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuShare.Shortcut")));
			this.menuShare.ShowShortcut = ((bool)(resources.GetObject("menuShare.ShowShortcut")));
			this.menuShare.Text = resources.GetString("menuShare.Text");
			this.menuShare.Visible = ((bool)(resources.GetObject("menuShare.Visible")));
			this.menuShare.Click += new System.EventHandler(this.menuShare_Click);
			// 
			// menuRevert
			// 
			this.menuRevert.Enabled = ((bool)(resources.GetObject("menuRevert.Enabled")));
			this.menuRevert.Index = 8;
			this.menuRevert.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuRevert.Shortcut")));
			this.menuRevert.ShowShortcut = ((bool)(resources.GetObject("menuRevert.ShowShortcut")));
			this.menuRevert.Text = resources.GetString("menuRevert.Text");
			this.menuRevert.Visible = ((bool)(resources.GetObject("menuRevert.Visible")));
			this.menuRevert.Click += new System.EventHandler(this.menuRevert_Click);
			// 
			// menuSeparator2
			// 
			this.menuSeparator2.Enabled = ((bool)(resources.GetObject("menuSeparator2.Enabled")));
			this.menuSeparator2.Index = 9;
			this.menuSeparator2.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuSeparator2.Shortcut")));
			this.menuSeparator2.ShowShortcut = ((bool)(resources.GetObject("menuSeparator2.ShowShortcut")));
			this.menuSeparator2.Text = resources.GetString("menuSeparator2.Text");
			this.menuSeparator2.Visible = ((bool)(resources.GetObject("menuSeparator2.Visible")));
			// 
			// menuProperties
			// 
			this.menuProperties.Enabled = ((bool)(resources.GetObject("menuProperties.Enabled")));
			this.menuProperties.Index = 10;
			this.menuProperties.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuProperties.Shortcut")));
			this.menuProperties.ShowShortcut = ((bool)(resources.GetObject("menuProperties.ShowShortcut")));
			this.menuProperties.Text = resources.GetString("menuProperties.Text");
			this.menuProperties.Visible = ((bool)(resources.GetObject("menuProperties.Visible")));
			this.menuProperties.Click += new System.EventHandler(this.menuProperties_Click);
			// 
			// menuRefresh
			// 
			this.menuRefresh.Enabled = ((bool)(resources.GetObject("menuRefresh.Enabled")));
			this.menuRefresh.Index = 11;
			this.menuRefresh.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuRefresh.Shortcut")));
			this.menuRefresh.ShowShortcut = ((bool)(resources.GetObject("menuRefresh.ShowShortcut")));
			this.menuRefresh.Text = resources.GetString("menuRefresh.Text");
			this.menuRefresh.Visible = ((bool)(resources.GetObject("menuRefresh.Visible")));
			this.menuRefresh.Click += new System.EventHandler(this.menuRefresh_Click);
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuAction,
																					  this.menuEdit,
																					  this.menuView,
																					  this.menuSecurity,
																					  this.menuHelp});
			this.mainMenu1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("mainMenu1.RightToLeft")));
			// 
			// menuAction
			// 
			this.menuAction.Enabled = ((bool)(resources.GetObject("menuAction.Enabled")));
			this.menuAction.Index = 0;
			this.menuAction.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					   this.menuActionCreate,
																					   this.menuActionAccept,
																					   this.menuActionRemove,
																					   this.menuItem7,
																					   this.menuActionOpen,
																					   this.menuActionShare,
																					   this.menuActionResolve,
																					   this.menuActionSync,
																					   this.menuActionRevert,
																					   this.menuActionProperties,
																					   this.menuItem4,
																					   this.MigrationMenuItem,
																					   this.menuSeparator,
																					   this.menuClose,
																					   this.menuExit});
			this.menuAction.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuAction.Shortcut")));
			this.menuAction.ShowShortcut = ((bool)(resources.GetObject("menuAction.ShowShortcut")));
			this.menuAction.Text = resources.GetString("menuAction.Text");
			this.menuAction.Visible = ((bool)(resources.GetObject("menuAction.Visible")));
			// 
			// menuActionCreate
			// 
			this.menuActionCreate.Enabled = ((bool)(resources.GetObject("menuActionCreate.Enabled")));
			this.menuActionCreate.Index = 0;
			this.menuActionCreate.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionCreate.Shortcut")));
			this.menuActionCreate.ShowShortcut = ((bool)(resources.GetObject("menuActionCreate.ShowShortcut")));
			this.menuActionCreate.Text = resources.GetString("menuActionCreate.Text");
			this.menuActionCreate.Visible = ((bool)(resources.GetObject("menuActionCreate.Visible")));
			this.menuActionCreate.Click += new System.EventHandler(this.menuCreate_Click);
			// 
			// menuActionAccept
			// 
			this.menuActionAccept.Enabled = ((bool)(resources.GetObject("menuActionAccept.Enabled")));
			this.menuActionAccept.Index = 1;
			this.menuActionAccept.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionAccept.Shortcut")));
			this.menuActionAccept.ShowShortcut = ((bool)(resources.GetObject("menuActionAccept.ShowShortcut")));
			this.menuActionAccept.Text = resources.GetString("menuActionAccept.Text");
			this.menuActionAccept.Visible = ((bool)(resources.GetObject("menuActionAccept.Visible")));
			this.menuActionAccept.Click += new System.EventHandler(this.menuAccept_Click);
			// 
			// menuActionRemove
			// 
			this.menuActionRemove.Enabled = ((bool)(resources.GetObject("menuActionRemove.Enabled")));
			this.menuActionRemove.Index = 2;
			this.menuActionRemove.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionRemove.Shortcut")));
			this.menuActionRemove.ShowShortcut = ((bool)(resources.GetObject("menuActionRemove.ShowShortcut")));
			this.menuActionRemove.Text = resources.GetString("menuActionRemove.Text");
			this.menuActionRemove.Visible = ((bool)(resources.GetObject("menuActionRemove.Visible")));
			this.menuActionRemove.Click += new System.EventHandler(this.menuRemove_Click);
			// 
			// menuItem7
			// 
			this.menuItem7.Enabled = ((bool)(resources.GetObject("menuItem7.Enabled")));
			this.menuItem7.Index = 3;
			this.menuItem7.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuItem7.Shortcut")));
			this.menuItem7.ShowShortcut = ((bool)(resources.GetObject("menuItem7.ShowShortcut")));
			this.menuItem7.Text = resources.GetString("menuItem7.Text");
			this.menuItem7.Visible = ((bool)(resources.GetObject("menuItem7.Visible")));
			//
			// menuSeparator
			//
			this.menuSeparator.Enabled = true;
			this.menuSeparator.Visible = true;
			this.MigrationMenuItem.Index = 12;
			this.menuSeparator.Text = resources.GetString("menuItem7.Text");

			//
			// MigrationmenuItem
			//
			this.MigrationMenuItem.Enabled = true;
			this.MigrationMenuItem.Visible = true;
			this.MigrationMenuItem.Index = 11;
			this.MigrationMenuItem.Text = resources.GetString("menuMigration");//"Migration";
			this.MigrationMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					   this.MigrationMenuSubItem });

			// 
			// MigrationMenuSubItem
			//
			this.MigrationMenuSubItem.Enabled = true;
			this.MigrationMenuSubItem.Visible = true;
			this.MigrationMenuSubItem.Text = resources.GetString("MigrationSubMenu");//"Migrate From 2.x";
			this.MigrationMenuSubItem.Click += new System.EventHandler(this.menuMigrateMigrate_Click);

			// 
			// menuActionOpen
			// 
			this.menuActionOpen.Enabled = ((bool)(resources.GetObject("menuActionOpen.Enabled")));
			this.menuActionOpen.Index = 4;
			this.menuActionOpen.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionOpen.Shortcut")));
			this.menuActionOpen.ShowShortcut = ((bool)(resources.GetObject("menuActionOpen.ShowShortcut")));
			this.menuActionOpen.Text = resources.GetString("menuActionOpen.Text");
			this.menuActionOpen.Visible = ((bool)(resources.GetObject("menuActionOpen.Visible")));
			this.menuActionOpen.Click += new System.EventHandler(this.menuOpen_Click);
			// 
			// menuActionShare
			// 
			this.menuActionShare.Enabled = ((bool)(resources.GetObject("menuActionShare.Enabled")));
			this.menuActionShare.Index = 5;
			this.menuActionShare.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionShare.Shortcut")));
			this.menuActionShare.ShowShortcut = ((bool)(resources.GetObject("menuActionShare.ShowShortcut")));
			this.menuActionShare.Text = resources.GetString("menuActionShare.Text");
			this.menuActionShare.Visible = ((bool)(resources.GetObject("menuActionShare.Visible")));
			this.menuActionShare.Click += new System.EventHandler(this.menuShare_Click);
			// 
			// menuActionResolve
			// 
			this.menuActionResolve.Enabled = ((bool)(resources.GetObject("menuActionResolve.Enabled")));
			this.menuActionResolve.Index = 6;
			this.menuActionResolve.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionResolve.Shortcut")));
			this.menuActionResolve.ShowShortcut = ((bool)(resources.GetObject("menuActionResolve.ShowShortcut")));
			this.menuActionResolve.Text = resources.GetString("menuActionResolve.Text");
			this.menuActionResolve.Visible = ((bool)(resources.GetObject("menuActionResolve.Visible")));
			this.menuActionResolve.Click += new System.EventHandler(this.menuResolve_Click);
			// 
			// menuActionSync
			// 
			this.menuActionSync.Enabled = ((bool)(resources.GetObject("menuActionSync.Enabled")));
			this.menuActionSync.Index = 7;
			this.menuActionSync.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionSync.Shortcut")));
			this.menuActionSync.ShowShortcut = ((bool)(resources.GetObject("menuActionSync.ShowShortcut")));
			this.menuActionSync.Text = resources.GetString("menuActionSync.Text");
			this.menuActionSync.Visible = ((bool)(resources.GetObject("menuActionSync.Visible")));
			this.menuActionSync.Click += new System.EventHandler(this.menuSyncNow_Click);
			// 
			// menuActionRevert
			// 
			this.menuActionRevert.Enabled = ((bool)(resources.GetObject("menuActionRevert.Enabled")));
			this.menuActionRevert.Index = 8;
			this.menuActionRevert.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionRevert.Shortcut")));
			this.menuActionRevert.ShowShortcut = ((bool)(resources.GetObject("menuActionRevert.ShowShortcut")));
			this.menuActionRevert.Text = resources.GetString("menuActionRevert.Text");
			this.menuActionRevert.Visible = ((bool)(resources.GetObject("menuActionRevert.Visible")));
			this.menuActionRevert.Click += new System.EventHandler(this.menuRevert_Click);
			// 
			// menuActionProperties
			// 
			this.menuActionProperties.Enabled = ((bool)(resources.GetObject("menuActionProperties.Enabled")));
			this.menuActionProperties.Index = 9;
			this.menuActionProperties.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionProperties.Shortcut")));
			this.menuActionProperties.ShowShortcut = ((bool)(resources.GetObject("menuActionProperties.ShowShortcut")));
			this.menuActionProperties.Text = resources.GetString("menuActionProperties.Text");
			this.menuActionProperties.Visible = ((bool)(resources.GetObject("menuActionProperties.Visible")));
			this.menuActionProperties.Click += new System.EventHandler(this.menuProperties_Click);
			// 
			// menuItem4
			// 
			this.menuItem4.Enabled = ((bool)(resources.GetObject("menuItem4.Enabled")));
			this.menuItem4.Index = 10;
			this.menuItem4.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuItem4.Shortcut")));
			this.menuItem4.ShowShortcut = ((bool)(resources.GetObject("menuItem4.ShowShortcut")));
			this.menuItem4.Text = resources.GetString("menuItem4.Text");
			this.menuItem4.Visible = ((bool)(resources.GetObject("menuItem4.Visible")));
			// 
			// menuClose
			// 
			this.menuClose.Enabled = ((bool)(resources.GetObject("menuClose.Enabled")));
			this.menuClose.Index = 13;
			this.menuClose.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuClose.Shortcut")));
			this.menuClose.ShowShortcut = ((bool)(resources.GetObject("menuClose.ShowShortcut")));
			this.menuClose.Text = resources.GetString("menuClose.Text");
			this.menuClose.Visible = ((bool)(resources.GetObject("menuClose.Visible")));
			this.menuClose.Click += new System.EventHandler(this.menuClose_Click);
			// 
			// menuExit
			// 
			this.menuExit.Enabled = ((bool)(resources.GetObject("menuExit.Enabled")));
			this.menuExit.Index = 14;
			this.menuExit.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuExit.Shortcut")));
			this.menuExit.ShowShortcut = ((bool)(resources.GetObject("menuExit.ShowShortcut")));
			this.menuExit.Text = resources.GetString("menuExit.Text");
			this.menuExit.Visible = ((bool)(resources.GetObject("menuExit.Visible")));
			this.menuExit.Click += new System.EventHandler(this.menuFileExit_Click);
			// 
			// menuEdit
			// 
			this.menuEdit.Enabled = ((bool)(resources.GetObject("menuEdit.Enabled")));
			this.menuEdit.Index = 1;
			this.menuEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuViewAccounts,
																					 this.menuEditPrefs});
			this.menuEdit.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuEdit.Shortcut")));
			this.menuEdit.ShowShortcut = ((bool)(resources.GetObject("menuEdit.ShowShortcut")));
			this.menuEdit.Text = resources.GetString("menuEdit.Text");
			this.menuEdit.Visible = ((bool)(resources.GetObject("menuEdit.Visible")));
			// 
			// menuViewAccounts
			// 
			this.menuViewAccounts.Enabled = ((bool)(resources.GetObject("menuViewAccounts.Enabled")));
			this.menuViewAccounts.Index = 0;
			this.menuViewAccounts.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuViewAccounts.Shortcut")));
			this.menuViewAccounts.ShowShortcut = ((bool)(resources.GetObject("menuViewAccounts.ShowShortcut")));
			this.menuViewAccounts.Text = resources.GetString("menuViewAccounts.Text");
			this.menuViewAccounts.Visible = ((bool)(resources.GetObject("menuViewAccounts.Visible")));
			this.menuViewAccounts.Click += new System.EventHandler(this.menuViewAccounts_Click);
			// 
			// menuEditPrefs
			// 
			this.menuEditPrefs.Enabled = ((bool)(resources.GetObject("menuEditPrefs.Enabled")));
			this.menuEditPrefs.Index = 1;
			this.menuEditPrefs.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuEditPrefs.Shortcut")));
			this.menuEditPrefs.ShowShortcut = ((bool)(resources.GetObject("menuEditPrefs.ShowShortcut")));
			this.menuEditPrefs.Text = resources.GetString("menuEditPrefs.Text");
			this.menuEditPrefs.Visible = ((bool)(resources.GetObject("menuEditPrefs.Visible")));
			this.menuEditPrefs.Click += new System.EventHandler(this.menuEditPrefs_Click);
			// 
			// menuView
			// 
			this.menuView.Enabled = ((bool)(resources.GetObject("menuView.Enabled")));
			this.menuView.Index = 2;
			this.menuView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuViewRefresh,
																					 this.menuItem1,
																					 this.menuViewLog,
																					 this.menuItem3,
																					 this.menuViewAvailable});
			this.menuView.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuView.Shortcut")));
			this.menuView.ShowShortcut = ((bool)(resources.GetObject("menuView.ShowShortcut")));
			this.menuView.Text = resources.GetString("menuView.Text");
			this.menuView.Visible = ((bool)(resources.GetObject("menuView.Visible")));
			// 
			// menuViewRefresh
			// 
			this.menuViewRefresh.Enabled = ((bool)(resources.GetObject("menuViewRefresh.Enabled")));
			this.menuViewRefresh.Index = 0;
			this.menuViewRefresh.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuViewRefresh.Shortcut")));
			this.menuViewRefresh.ShowShortcut = ((bool)(resources.GetObject("menuViewRefresh.ShowShortcut")));
			this.menuViewRefresh.Text = resources.GetString("menuViewRefresh.Text");
			this.menuViewRefresh.Visible = ((bool)(resources.GetObject("menuViewRefresh.Visible")));
			this.menuViewRefresh.Click += new System.EventHandler(this.menuRefresh_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Enabled = ((bool)(resources.GetObject("menuItem1.Enabled")));
			this.menuItem1.Index = 1;
			this.menuItem1.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuItem1.Shortcut")));
			this.menuItem1.ShowShortcut = ((bool)(resources.GetObject("menuItem1.ShowShortcut")));
			this.menuItem1.Text = resources.GetString("menuItem1.Text");
			this.menuItem1.Visible = ((bool)(resources.GetObject("menuItem1.Visible")));
			// 
			// menuViewLog
			// 
			this.menuViewLog.Enabled = ((bool)(resources.GetObject("menuViewLog.Enabled")));
			this.menuViewLog.Index = 2;
			this.menuViewLog.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuViewLog.Shortcut")));
			this.menuViewLog.ShowShortcut = ((bool)(resources.GetObject("menuViewLog.ShowShortcut")));
			this.menuViewLog.Text = resources.GetString("menuViewLog.Text");
			this.menuViewLog.Visible = ((bool)(resources.GetObject("menuViewLog.Visible")));
			this.menuViewLog.Click += new System.EventHandler(this.menuViewLog_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Enabled = ((bool)(resources.GetObject("menuItem3.Enabled")));
			this.menuItem3.Index = 3;
			this.menuItem3.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuItem3.Shortcut")));
			this.menuItem3.ShowShortcut = ((bool)(resources.GetObject("menuItem3.ShowShortcut")));
			this.menuItem3.Text = resources.GetString("menuItem3.Text");
			this.menuItem3.Visible = ((bool)(resources.GetObject("menuItem3.Visible")));
			// 
			// menuViewAvailable
			// 
			this.menuViewAvailable.Checked = true;
			this.menuViewAvailable.Enabled = ((bool)(resources.GetObject("menuViewAvailable.Enabled")));
			this.menuViewAvailable.Index = 4;
			this.menuViewAvailable.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuViewAvailable.Shortcut")));
			this.menuViewAvailable.ShowShortcut = ((bool)(resources.GetObject("menuViewAvailable.ShowShortcut")));
			this.menuViewAvailable.Text = resources.GetString("menuViewAvailable.Text");
			this.menuViewAvailable.Visible = ((bool)(resources.GetObject("menuViewAvailable.Visible")));
			this.menuViewAvailable.Click += new System.EventHandler(this.showiFolders_Click);
			
			//
			//menuSecurity
			//
			this.menuSecurity.Enabled = true;
			this.menuSecurity.Index = 3;
			this.menuSecurity.Text = resources.GetString("SecurityText");//"Security";
			this.menuSecurity.MenuItems.AddRange( new System.Windows.Forms.MenuItem[] {
																						  this.menuRecoverKeys,
																						  this.menuResetPassphrase
																					  });
			this.menuSecurity.Visible = true;

			//
			//menuRecoverKeys
			//
			this.menuRecoverKeys.Index = 0;
			this.menuRecoverKeys.Enabled = true;
			this.menuRecoverKeys.Text = resources.GetString("KeyRecoveryText");//"Key Recovery";
			this.menuRecoverKeys.Visible = true;
			this.menuRecoverKeys.MenuItems.AddRange( new System.Windows.Forms.MenuItem[] {
																							 this.menuExportKeys,
																							 this.menuImportKeys
																						 });
			//
			//menuExportKeys
			//
			this.menuExportKeys.Index = 0;
			this.menuExportKeys.Enabled = true;
			this.menuExportKeys.Visible = true;
			this.menuExportKeys.Text = resources.GetString("menuExportKeysText");//"Export Encrypted Keys";
			this.menuExportKeys.Click += new EventHandler(menuExportKeys_Select);

			//
			// menuImportKeys
			//
			this.menuImportKeys.Index = 1;
			this.menuImportKeys.Enabled = true;
			this.menuImportKeys.Text = resources.GetString("menuImportKeysText");//"Import Decrypted Keys";
			this.menuImportKeys.Visible = true;
			this.menuImportKeys.Click += new EventHandler(menuImportKeys_Select);

			//
			// menuResetPassphrase
			//
			this.menuResetPassphrase.Index = 1;
			this.menuResetPassphrase.Enabled = true;
			this.menuResetPassphrase.Text = resources.GetString("ResetPPText");//"Reset Passphrase";
			this.menuResetPassphrase.Visible = true;
			this.menuResetPassphrase.Click += new EventHandler(menuResetPassphrase_Select);

			// 
			// menuHelp
			// 
			this.menuHelp.Enabled = ((bool)(resources.GetObject("menuHelp.Enabled")));
			this.menuHelp.Index = 4;
			this.menuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuHelpHelp,
																					 this.menuHelpAbout});
			this.menuHelp.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuHelp.Shortcut")));
			this.menuHelp.ShowShortcut = ((bool)(resources.GetObject("menuHelp.ShowShortcut")));
			this.menuHelp.Text = resources.GetString("menuHelp.Text");
			this.menuHelp.Visible = ((bool)(resources.GetObject("menuHelp.Visible")));
			// 
			// menuHelpHelp
			// 
			this.menuHelpHelp.Enabled = ((bool)(resources.GetObject("menuHelpHelp.Enabled")));
			this.menuHelpHelp.Index = 0;
			this.menuHelpHelp.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuHelpHelp.Shortcut")));
			this.menuHelpHelp.ShowShortcut = ((bool)(resources.GetObject("menuHelpHelp.ShowShortcut")));
			this.menuHelpHelp.Text = resources.GetString("menuHelpHelp.Text");
			this.menuHelpHelp.Visible = ((bool)(resources.GetObject("menuHelpHelp.Visible")));
			this.menuHelpHelp.Click += new System.EventHandler(this.menuHelpHelp_Click);
			// 
			// menuHelpAbout
			// 
			this.menuHelpAbout.Enabled = ((bool)(resources.GetObject("menuHelpAbout.Enabled")));
			this.menuHelpAbout.Index = 1;
			this.menuHelpAbout.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuHelpAbout.Shortcut")));
			this.menuHelpAbout.ShowShortcut = ((bool)(resources.GetObject("menuHelpAbout.ShowShortcut")));
			this.menuHelpAbout.Text = resources.GetString("menuHelpAbout.Text");
			this.menuHelpAbout.Visible = ((bool)(resources.GetObject("menuHelpAbout.Visible")));
			this.menuHelpAbout.Click += new System.EventHandler(this.menuHelpAbout_Click);
			// 
			// progressBar1
			// 
			this.progressBar1.AccessibleDescription = resources.GetString("progressBar1.AccessibleDescription");
			this.progressBar1.AccessibleName = resources.GetString("progressBar1.AccessibleName");
			this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("progressBar1.Anchor")));
			this.progressBar1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("progressBar1.BackgroundImage")));
			this.progressBar1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("progressBar1.Dock")));
			this.progressBar1.Enabled = ((bool)(resources.GetObject("progressBar1.Enabled")));
			this.progressBar1.Font = ((System.Drawing.Font)(resources.GetObject("progressBar1.Font")));
			this.progressBar1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("progressBar1.ImeMode")));
			this.progressBar1.Location = ((System.Drawing.Point)(resources.GetObject("progressBar1.Location")));
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("progressBar1.RightToLeft")));
			this.progressBar1.Size = ((System.Drawing.Size)(resources.GetObject("progressBar1.Size")));
			this.progressBar1.TabIndex = ((int)(resources.GetObject("progressBar1.TabIndex")));
			this.progressBar1.Text = resources.GetString("progressBar1.Text");
			this.progressBar1.Visible = ((bool)(resources.GetObject("progressBar1.Visible")));
			// 
			// statusBar1
			// 
			this.statusBar1.AccessibleDescription = resources.GetString("statusBar1.AccessibleDescription");
			this.statusBar1.AccessibleName = resources.GetString("statusBar1.AccessibleName");
			this.statusBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("statusBar1.Anchor")));
			this.statusBar1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("statusBar1.BackgroundImage")));
			this.statusBar1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("statusBar1.Dock")));
			this.statusBar1.Enabled = ((bool)(resources.GetObject("statusBar1.Enabled")));
			this.statusBar1.Font = ((System.Drawing.Font)(resources.GetObject("statusBar1.Font")));
			this.statusBar1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("statusBar1.ImeMode")));
			this.statusBar1.Location = ((System.Drawing.Point)(resources.GetObject("statusBar1.Location")));
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("statusBar1.RightToLeft")));
			this.statusBar1.Size = ((System.Drawing.Size)(resources.GetObject("statusBar1.Size")));
			this.statusBar1.TabIndex = ((int)(resources.GetObject("statusBar1.TabIndex")));
			this.statusBar1.Text = resources.GetString("statusBar1.Text");
			this.statusBar1.Visible = ((bool)(resources.GetObject("statusBar1.Visible")));
			// 
			// groupBox1
			// 
			this.groupBox1.AccessibleDescription = resources.GetString("groupBox1.AccessibleDescription");
			this.groupBox1.AccessibleName = resources.GetString("groupBox1.AccessibleName");
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox1.Anchor")));
			this.groupBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox1.BackgroundImage")));
			this.groupBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBox1.Dock")));
			this.groupBox1.Enabled = ((bool)(resources.GetObject("groupBox1.Enabled")));
			this.groupBox1.Font = ((System.Drawing.Font)(resources.GetObject("groupBox1.Font")));
			this.groupBox1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBox1.ImeMode")));
			this.groupBox1.Location = ((System.Drawing.Point)(resources.GetObject("groupBox1.Location")));
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBox1.RightToLeft")));
			this.groupBox1.Size = ((System.Drawing.Size)(resources.GetObject("groupBox1.Size")));
			this.groupBox1.TabIndex = ((int)(resources.GetObject("groupBox1.TabIndex")));
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = resources.GetString("groupBox1.Text");
			this.groupBox1.Visible = ((bool)(resources.GetObject("groupBox1.Visible")));
			// 
			// panel1
			// 
			this.panel1.AccessibleDescription = resources.GetString("panel1.AccessibleDescription");
			this.panel1.AccessibleName = resources.GetString("panel1.AccessibleName");
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panel1.Anchor")));
			this.panel1.AutoScroll = ((bool)(resources.GetObject("panel1.AutoScroll")));
			this.panel1.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panel1.AutoScrollMargin")));
			this.panel1.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panel1.AutoScrollMinSize")));
			this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
			this.panel1.Controls.Add(this.iFolderActions);
			this.panel1.Controls.Add(this.create);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.filter);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.showiFolders);
			this.panel1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panel1.Dock")));
			this.panel1.Enabled = ((bool)(resources.GetObject("panel1.Enabled")));
			this.panel1.Font = ((System.Drawing.Font)(resources.GetObject("panel1.Font")));
			this.panel1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panel1.ImeMode")));
			this.panel1.Location = ((System.Drawing.Point)(resources.GetObject("panel1.Location")));
			this.panel1.Name = "panel1";
			this.panel1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panel1.RightToLeft")));
			this.panel1.Size = ((System.Drawing.Size)(resources.GetObject("panel1.Size")));
			this.panel1.TabIndex = ((int)(resources.GetObject("panel1.TabIndex")));
			this.panel1.Text = resources.GetString("panel1.Text");
			this.panel1.Visible = ((bool)(resources.GetObject("panel1.Visible")));
			// 
			// iFolderActions
			// 
			this.iFolderActions.AccessibleDescription = resources.GetString("iFolderActions.AccessibleDescription");
			this.iFolderActions.AccessibleName = resources.GetString("iFolderActions.AccessibleName");
			this.iFolderActions.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("iFolderActions.Anchor")));
			this.iFolderActions.AutoScroll = ((bool)(resources.GetObject("iFolderActions.AutoScroll")));
			this.iFolderActions.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("iFolderActions.AutoScrollMargin")));
			this.iFolderActions.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("iFolderActions.AutoScrollMinSize")));
			this.iFolderActions.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("iFolderActions.BackgroundImage")));
			this.iFolderActions.Controls.Add(this.resolve);
			this.iFolderActions.Controls.Add(this.remove);
			this.iFolderActions.Controls.Add(this.accept);
			this.iFolderActions.Controls.Add(this.properties);
			this.iFolderActions.Controls.Add(this.revert);
			this.iFolderActions.Controls.Add(this.share);
			this.iFolderActions.Controls.Add(this.syncNow);
			this.iFolderActions.Controls.Add(this.open);
			this.iFolderActions.Controls.Add(this.label3);
			this.iFolderActions.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("iFolderActions.Dock")));
			this.iFolderActions.Enabled = ((bool)(resources.GetObject("iFolderActions.Enabled")));
			this.iFolderActions.Font = ((System.Drawing.Font)(resources.GetObject("iFolderActions.Font")));
			this.iFolderActions.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("iFolderActions.ImeMode")));
			this.iFolderActions.Location = ((System.Drawing.Point)(resources.GetObject("iFolderActions.Location")));
			this.iFolderActions.Name = "iFolderActions";
			this.iFolderActions.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("iFolderActions.RightToLeft")));
			this.iFolderActions.Size = ((System.Drawing.Size)(resources.GetObject("iFolderActions.Size")));
			this.iFolderActions.TabIndex = ((int)(resources.GetObject("iFolderActions.TabIndex")));
			this.iFolderActions.Text = resources.GetString("iFolderActions.Text");
			this.iFolderActions.Visible = ((bool)(resources.GetObject("iFolderActions.Visible")));
			// 
			// resolve
			// 
			this.resolve.AccessibleDescription = resources.GetString("resolve.AccessibleDescription");
			this.resolve.AccessibleName = resources.GetString("resolve.AccessibleName");
			this.resolve.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("resolve.Anchor")));
			this.resolve.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("resolve.BackgroundImage")));
			this.resolve.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("resolve.Dock")));
			this.resolve.Enabled = ((bool)(resources.GetObject("resolve.Enabled")));
			this.resolve.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("resolve.FlatStyle")));
			this.resolve.Font = ((System.Drawing.Font)(resources.GetObject("resolve.Font")));
			this.resolve.Image = ((System.Drawing.Image)(resources.GetObject("resolve.Image")));
			this.resolve.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("resolve.ImageAlign")));
			this.resolve.ImageIndex = ((int)(resources.GetObject("resolve.ImageIndex")));
			this.resolve.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("resolve.ImeMode")));
			this.resolve.Location = ((System.Drawing.Point)(resources.GetObject("resolve.Location")));
			this.resolve.Name = "resolve";
			this.resolve.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("resolve.RightToLeft")));
			this.resolve.Size = ((System.Drawing.Size)(resources.GetObject("resolve.Size")));
			this.resolve.TabIndex = ((int)(resources.GetObject("resolve.TabIndex")));
			this.resolve.Text = resources.GetString("resolve.Text");
			this.resolve.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("resolve.TextAlign")));
			this.resolve.Visible = ((bool)(resources.GetObject("resolve.Visible")));
			this.resolve.Click += new System.EventHandler(this.menuResolve_Click);
			this.resolve.MouseEnter += new System.EventHandler(this.button_MouseEnter);
			this.resolve.MouseMove += new System.Windows.Forms.MouseEventHandler(this.button_MouseMove);
			this.resolve.MouseLeave += new System.EventHandler(this.button_MouseLeave);
			this.resolve.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
			// 
			// remove
			// 
			this.remove.AccessibleDescription = resources.GetString("remove.AccessibleDescription");
			this.remove.AccessibleName = resources.GetString("remove.AccessibleName");
			this.remove.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("remove.Anchor")));
			this.remove.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("remove.BackgroundImage")));
			this.remove.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("remove.Dock")));
			this.remove.Enabled = ((bool)(resources.GetObject("remove.Enabled")));
			this.remove.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("remove.FlatStyle")));
			this.remove.Font = ((System.Drawing.Font)(resources.GetObject("remove.Font")));
			this.remove.Image = ((System.Drawing.Image)(resources.GetObject("remove.Image")));
			this.remove.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("remove.ImageAlign")));
			this.remove.ImageIndex = ((int)(resources.GetObject("remove.ImageIndex")));
			this.remove.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("remove.ImeMode")));
			this.remove.Location = ((System.Drawing.Point)(resources.GetObject("remove.Location")));
			this.remove.Name = "remove";
			this.remove.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("remove.RightToLeft")));
			this.remove.Size = ((System.Drawing.Size)(resources.GetObject("remove.Size")));
			this.remove.TabIndex = ((int)(resources.GetObject("remove.TabIndex")));
			this.remove.Text = resources.GetString("remove.Text");
			this.remove.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("remove.TextAlign")));
			this.remove.Visible = ((bool)(resources.GetObject("remove.Visible")));
			this.remove.Click += new System.EventHandler(this.menuRemove_Click);
			this.remove.MouseEnter += new System.EventHandler(this.button_MouseEnter);
			this.remove.MouseMove += new System.Windows.Forms.MouseEventHandler(this.button_MouseMove);
			this.remove.MouseLeave += new System.EventHandler(this.button_MouseLeave);
			this.remove.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
			// 
			// accept
			// 
			this.accept.AccessibleDescription = resources.GetString("accept.AccessibleDescription");
			this.accept.AccessibleName = resources.GetString("accept.AccessibleName");
			this.accept.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("accept.Anchor")));
			this.accept.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("accept.BackgroundImage")));
			this.accept.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("accept.Dock")));
			this.accept.Enabled = ((bool)(resources.GetObject("accept.Enabled")));
			this.accept.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("accept.FlatStyle")));
			this.accept.Font = ((System.Drawing.Font)(resources.GetObject("accept.Font")));
			this.accept.Image = ((System.Drawing.Image)(resources.GetObject("accept.Image")));
			this.accept.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("accept.ImageAlign")));
			this.accept.ImageIndex = ((int)(resources.GetObject("accept.ImageIndex")));
			this.accept.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("accept.ImeMode")));
			this.accept.Location = ((System.Drawing.Point)(resources.GetObject("accept.Location")));
			this.accept.Name = "accept";
			this.accept.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("accept.RightToLeft")));
			this.accept.Size = ((System.Drawing.Size)(resources.GetObject("accept.Size")));
			this.accept.TabIndex = ((int)(resources.GetObject("accept.TabIndex")));
			this.accept.Text = resources.GetString("accept.Text");
			this.accept.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("accept.TextAlign")));
			this.accept.Visible = ((bool)(resources.GetObject("accept.Visible")));
			this.accept.Click += new System.EventHandler(this.menuAccept_Click);
			this.accept.MouseEnter += new System.EventHandler(this.button_MouseEnter);
			this.accept.MouseMove += new System.Windows.Forms.MouseEventHandler(this.button_MouseMove);
			this.accept.MouseLeave += new System.EventHandler(this.button_MouseLeave);
			this.accept.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
			// 
			// properties
			// 
			this.properties.AccessibleDescription = resources.GetString("properties.AccessibleDescription");
			this.properties.AccessibleName = resources.GetString("properties.AccessibleName");
			this.properties.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("properties.Anchor")));
			this.properties.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("properties.BackgroundImage")));
			this.properties.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("properties.Dock")));
			this.properties.Enabled = ((bool)(resources.GetObject("properties.Enabled")));
			this.properties.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("properties.FlatStyle")));
			this.properties.Font = ((System.Drawing.Font)(resources.GetObject("properties.Font")));
			this.properties.Image = ((System.Drawing.Image)(resources.GetObject("properties.Image")));
			this.properties.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("properties.ImageAlign")));
			this.properties.ImageIndex = ((int)(resources.GetObject("properties.ImageIndex")));
			this.properties.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("properties.ImeMode")));
			this.properties.Location = ((System.Drawing.Point)(resources.GetObject("properties.Location")));
			this.properties.Name = "properties";
			this.properties.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("properties.RightToLeft")));
			this.properties.Size = ((System.Drawing.Size)(resources.GetObject("properties.Size")));
			this.properties.TabIndex = ((int)(resources.GetObject("properties.TabIndex")));
			this.properties.Text = resources.GetString("properties.Text");
			this.properties.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("properties.TextAlign")));
			this.properties.Visible = ((bool)(resources.GetObject("properties.Visible")));
			this.properties.Click += new System.EventHandler(this.menuProperties_Click);
			this.properties.MouseEnter += new System.EventHandler(this.button_MouseEnter);
			this.properties.MouseMove += new System.Windows.Forms.MouseEventHandler(this.button_MouseMove);
			this.properties.MouseLeave += new System.EventHandler(this.button_MouseLeave);
			this.properties.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
			// 
			// revert
			// 
			this.revert.AccessibleDescription = resources.GetString("revert.AccessibleDescription");
			this.revert.AccessibleName = resources.GetString("revert.AccessibleName");
			this.revert.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("revert.Anchor")));
			this.revert.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("revert.BackgroundImage")));
			this.revert.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("revert.Dock")));
			this.revert.Enabled = ((bool)(resources.GetObject("revert.Enabled")));
			this.revert.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("revert.FlatStyle")));
			this.revert.Font = ((System.Drawing.Font)(resources.GetObject("revert.Font")));
			this.revert.Image = ((System.Drawing.Image)(resources.GetObject("revert.Image")));
			this.revert.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("revert.ImageAlign")));
			this.revert.ImageIndex = ((int)(resources.GetObject("revert.ImageIndex")));
			this.revert.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("revert.ImeMode")));
			this.revert.Location = ((System.Drawing.Point)(resources.GetObject("revert.Location")));
			this.revert.Name = "revert";
			this.revert.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("revert.RightToLeft")));
			this.revert.Size = ((System.Drawing.Size)(resources.GetObject("revert.Size")));
			this.revert.TabIndex = ((int)(resources.GetObject("revert.TabIndex")));
			this.revert.Text = resources.GetString("revert.Text");
			this.revert.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("revert.TextAlign")));
			this.revert.Visible = ((bool)(resources.GetObject("revert.Visible")));
			this.revert.Click += new System.EventHandler(this.menuRevert_Click);
			this.revert.MouseEnter += new System.EventHandler(this.button_MouseEnter);
			this.revert.MouseMove += new System.Windows.Forms.MouseEventHandler(this.button_MouseMove);
			this.revert.MouseLeave += new System.EventHandler(this.button_MouseLeave);
			this.revert.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
			// 
			// share
			// 
			this.share.AccessibleDescription = resources.GetString("share.AccessibleDescription");
			this.share.AccessibleName = resources.GetString("share.AccessibleName");
			this.share.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("share.Anchor")));
			this.share.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("share.BackgroundImage")));
			this.share.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("share.Dock")));
			this.share.Enabled = ((bool)(resources.GetObject("share.Enabled")));
			this.share.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("share.FlatStyle")));
			this.share.Font = ((System.Drawing.Font)(resources.GetObject("share.Font")));
			this.share.Image = ((System.Drawing.Image)(resources.GetObject("share.Image")));
			this.share.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("share.ImageAlign")));
			this.share.ImageIndex = ((int)(resources.GetObject("share.ImageIndex")));
			this.share.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("share.ImeMode")));
			this.share.Location = ((System.Drawing.Point)(resources.GetObject("share.Location")));
			this.share.Name = "share";
			this.share.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("share.RightToLeft")));
			this.share.Size = ((System.Drawing.Size)(resources.GetObject("share.Size")));
			this.share.TabIndex = ((int)(resources.GetObject("share.TabIndex")));
			this.share.Text = resources.GetString("share.Text");
			this.share.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("share.TextAlign")));
			this.share.Visible = ((bool)(resources.GetObject("share.Visible")));
			this.share.Click += new System.EventHandler(this.menuShare_Click);
			this.share.MouseEnter += new System.EventHandler(this.button_MouseEnter);
			this.share.MouseMove += new System.Windows.Forms.MouseEventHandler(this.button_MouseMove);
			this.share.MouseLeave += new System.EventHandler(this.button_MouseLeave);
			this.share.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
			// 
			// syncNow
			// 
			this.syncNow.AccessibleDescription = resources.GetString("syncNow.AccessibleDescription");
			this.syncNow.AccessibleName = resources.GetString("syncNow.AccessibleName");
			this.syncNow.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("syncNow.Anchor")));
			this.syncNow.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("syncNow.BackgroundImage")));
			this.syncNow.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("syncNow.Dock")));
			this.syncNow.Enabled = ((bool)(resources.GetObject("syncNow.Enabled")));
			this.syncNow.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("syncNow.FlatStyle")));
			this.syncNow.Font = ((System.Drawing.Font)(resources.GetObject("syncNow.Font")));
			this.syncNow.Image = ((System.Drawing.Image)(resources.GetObject("syncNow.Image")));
			this.syncNow.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("syncNow.ImageAlign")));
			this.syncNow.ImageIndex = ((int)(resources.GetObject("syncNow.ImageIndex")));
			this.syncNow.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("syncNow.ImeMode")));
			this.syncNow.Location = ((System.Drawing.Point)(resources.GetObject("syncNow.Location")));
			this.syncNow.Name = "syncNow";
			this.syncNow.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("syncNow.RightToLeft")));
			this.syncNow.Size = ((System.Drawing.Size)(resources.GetObject("syncNow.Size")));
			this.syncNow.TabIndex = ((int)(resources.GetObject("syncNow.TabIndex")));
			this.syncNow.Text = resources.GetString("syncNow.Text");
			this.syncNow.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("syncNow.TextAlign")));
			this.syncNow.Visible = ((bool)(resources.GetObject("syncNow.Visible")));
			this.syncNow.Click += new System.EventHandler(this.menuSyncNow_Click);
			this.syncNow.MouseEnter += new System.EventHandler(this.button_MouseEnter);
			this.syncNow.MouseMove += new System.Windows.Forms.MouseEventHandler(this.button_MouseMove);
			this.syncNow.MouseLeave += new System.EventHandler(this.button_MouseLeave);
			this.syncNow.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
			// 
			// open
			// 
			this.open.AccessibleDescription = resources.GetString("open.AccessibleDescription");
			this.open.AccessibleName = resources.GetString("open.AccessibleName");
			this.open.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("open.Anchor")));
			this.open.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("open.BackgroundImage")));
			this.open.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("open.Dock")));
			this.open.Enabled = ((bool)(resources.GetObject("open.Enabled")));
			this.open.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("open.FlatStyle")));
			this.open.Font = ((System.Drawing.Font)(resources.GetObject("open.Font")));
			this.open.Image = ((System.Drawing.Image)(resources.GetObject("open.Image")));
			this.open.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("open.ImageAlign")));
			this.open.ImageIndex = ((int)(resources.GetObject("open.ImageIndex")));
			this.open.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("open.ImeMode")));
			this.open.Location = ((System.Drawing.Point)(resources.GetObject("open.Location")));
			this.open.Name = "open";
			this.open.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("open.RightToLeft")));
			this.open.Size = ((System.Drawing.Size)(resources.GetObject("open.Size")));
			this.open.TabIndex = ((int)(resources.GetObject("open.TabIndex")));
			this.open.Text = resources.GetString("open.Text");
			this.open.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("open.TextAlign")));
			this.open.Visible = ((bool)(resources.GetObject("open.Visible")));
			this.open.Click += new System.EventHandler(this.menuOpen_Click);
			this.open.MouseEnter += new System.EventHandler(this.button_MouseEnter);
			this.open.MouseMove += new System.Windows.Forms.MouseEventHandler(this.button_MouseMove);
			this.open.MouseLeave += new System.EventHandler(this.button_MouseLeave);
			this.open.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
			// 
			// label3
			// 
			this.label3.AccessibleDescription = resources.GetString("label3.AccessibleDescription");
			this.label3.AccessibleName = resources.GetString("label3.AccessibleName");
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label3.Anchor")));
			this.label3.AutoSize = ((bool)(resources.GetObject("label3.AutoSize")));
			this.label3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label3.Dock")));
			this.label3.Enabled = ((bool)(resources.GetObject("label3.Enabled")));
			this.label3.Font = ((System.Drawing.Font)(resources.GetObject("label3.Font")));
			this.label3.ForeColor = System.Drawing.SystemColors.Desktop;
			this.label3.Image = ((System.Drawing.Image)(resources.GetObject("label3.Image")));
			this.label3.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label3.ImageAlign")));
			this.label3.ImageIndex = ((int)(resources.GetObject("label3.ImageIndex")));
			this.label3.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label3.ImeMode")));
			this.label3.Location = ((System.Drawing.Point)(resources.GetObject("label3.Location")));
			this.label3.Name = "label3";
			this.label3.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label3.RightToLeft")));
			this.label3.Size = ((System.Drawing.Size)(resources.GetObject("label3.Size")));
			this.label3.TabIndex = ((int)(resources.GetObject("label3.TabIndex")));
			this.label3.Text = resources.GetString("label3.Text");
			this.label3.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label3.TextAlign")));
			this.label3.Visible = ((bool)(resources.GetObject("label3.Visible")));
			// 
			// create
			// 
			this.create.AccessibleDescription = resources.GetString("create.AccessibleDescription");
			this.create.AccessibleName = resources.GetString("create.AccessibleName");
			this.create.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("create.Anchor")));
			this.create.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("create.BackgroundImage")));
			this.create.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("create.Dock")));
			this.create.Enabled = ((bool)(resources.GetObject("create.Enabled")));
			this.create.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("create.FlatStyle")));
			this.create.Font = ((System.Drawing.Font)(resources.GetObject("create.Font")));
			this.create.Image = ((System.Drawing.Image)(resources.GetObject("create.Image")));
			this.create.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("create.ImageAlign")));
			this.create.ImageIndex = ((int)(resources.GetObject("create.ImageIndex")));
			this.create.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("create.ImeMode")));
			this.create.Location = ((System.Drawing.Point)(resources.GetObject("create.Location")));
			this.create.Name = "create";
			this.create.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("create.RightToLeft")));
			this.create.Size = ((System.Drawing.Size)(resources.GetObject("create.Size")));
			this.create.TabIndex = ((int)(resources.GetObject("create.TabIndex")));
			this.create.Text = resources.GetString("create.Text");
			this.create.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("create.TextAlign")));
			this.create.Visible = ((bool)(resources.GetObject("create.Visible")));
			this.create.Click += new System.EventHandler(this.menuCreate_Click);
			this.create.MouseEnter += new System.EventHandler(this.button_MouseEnter);
			this.create.MouseMove += new System.Windows.Forms.MouseEventHandler(this.button_MouseMove);
			this.create.MouseLeave += new System.EventHandler(this.button_MouseLeave);
			this.create.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
			// 
			// label2
			// 
			this.label2.AccessibleDescription = resources.GetString("label2.AccessibleDescription");
			this.label2.AccessibleName = resources.GetString("label2.AccessibleName");
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label2.Anchor")));
			this.label2.AutoSize = ((bool)(resources.GetObject("label2.AutoSize")));
			this.label2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label2.Dock")));
			this.label2.Enabled = ((bool)(resources.GetObject("label2.Enabled")));
			this.label2.Font = ((System.Drawing.Font)(resources.GetObject("label2.Font")));
			this.label2.ForeColor = System.Drawing.SystemColors.Desktop;
			this.label2.Image = ((System.Drawing.Image)(resources.GetObject("label2.Image")));
			this.label2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label2.ImageAlign")));
			this.label2.ImageIndex = ((int)(resources.GetObject("label2.ImageIndex")));
			this.label2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label2.ImeMode")));
			this.label2.Location = ((System.Drawing.Point)(resources.GetObject("label2.Location")));
			this.label2.Name = "label2";
			this.label2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label2.RightToLeft")));
			this.label2.Size = ((System.Drawing.Size)(resources.GetObject("label2.Size")));
			this.label2.TabIndex = ((int)(resources.GetObject("label2.TabIndex")));
			this.label2.Text = resources.GetString("label2.Text");
			this.label2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label2.TextAlign")));
			this.label2.Visible = ((bool)(resources.GetObject("label2.Visible")));
			// 
			// filter
			// 
			this.filter.AccessibleDescription = resources.GetString("filter.AccessibleDescription");
			this.filter.AccessibleName = resources.GetString("filter.AccessibleName");
			this.filter.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("filter.Anchor")));
			this.filter.AutoSize = ((bool)(resources.GetObject("filter.AutoSize")));
			this.filter.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("filter.BackgroundImage")));
			this.filter.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("filter.Dock")));
			this.filter.Enabled = ((bool)(resources.GetObject("filter.Enabled")));
			this.filter.Font = ((System.Drawing.Font)(resources.GetObject("filter.Font")));
			this.filter.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("filter.ImeMode")));
			this.filter.Location = ((System.Drawing.Point)(resources.GetObject("filter.Location")));
			this.filter.MaxLength = ((int)(resources.GetObject("filter.MaxLength")));
			this.filter.Multiline = ((bool)(resources.GetObject("filter.Multiline")));
			this.filter.Name = "filter";
			this.filter.PasswordChar = ((char)(resources.GetObject("filter.PasswordChar")));
			this.filter.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("filter.RightToLeft")));
			this.filter.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("filter.ScrollBars")));
			this.filter.Size = ((System.Drawing.Size)(resources.GetObject("filter.Size")));
			this.filter.TabIndex = ((int)(resources.GetObject("filter.TabIndex")));
			this.filter.Text = resources.GetString("filter.Text");
			this.filter.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("filter.TextAlign")));
			this.filter.Visible = ((bool)(resources.GetObject("filter.Visible")));
			this.filter.WordWrap = ((bool)(resources.GetObject("filter.WordWrap")));
			this.filter.TextChanged +=new EventHandler(filter_TextChanged);
			// 
			// label1
			// 
			this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
			this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
			this.label1.ForeColor = System.Drawing.SystemColors.Desktop;
			this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
			this.label1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.ImageAlign")));
			this.label1.ImageIndex = ((int)(resources.GetObject("label1.ImageIndex")));
			this.label1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label1.ImeMode")));
			this.label1.Location = ((System.Drawing.Point)(resources.GetObject("label1.Location")));
			this.label1.Name = "label1";
			this.label1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label1.RightToLeft")));
			this.label1.Size = ((System.Drawing.Size)(resources.GetObject("label1.Size")));
			this.label1.TabIndex = ((int)(resources.GetObject("label1.TabIndex")));
			this.label1.Text = resources.GetString("label1.Text");
			this.label1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.TextAlign")));
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
			// 
			// showiFolders
			// 
			this.showiFolders.AccessibleDescription = resources.GetString("showiFolders.AccessibleDescription");
			this.showiFolders.AccessibleName = resources.GetString("showiFolders.AccessibleName");
			this.showiFolders.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("showiFolders.Anchor")));
			this.showiFolders.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("showiFolders.BackgroundImage")));
			this.showiFolders.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("showiFolders.Dock")));
			this.showiFolders.Enabled = ((bool)(resources.GetObject("showiFolders.Enabled")));
			this.showiFolders.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("showiFolders.FlatStyle")));
			this.showiFolders.Font = ((System.Drawing.Font)(resources.GetObject("showiFolders.Font")));
			this.showiFolders.Image = ((System.Drawing.Image)(resources.GetObject("showiFolders.Image")));
			this.showiFolders.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("showiFolders.ImageAlign")));
			this.showiFolders.ImageIndex = ((int)(resources.GetObject("showiFolders.ImageIndex")));
			this.showiFolders.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("showiFolders.ImeMode")));
			this.showiFolders.Location = ((System.Drawing.Point)(resources.GetObject("showiFolders.Location")));
			this.showiFolders.Name = "showiFolders";
			this.showiFolders.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("showiFolders.RightToLeft")));
			this.showiFolders.Size = ((System.Drawing.Size)(resources.GetObject("showiFolders.Size")));
			this.showiFolders.TabIndex = ((int)(resources.GetObject("showiFolders.TabIndex")));
			this.showiFolders.Text = resources.GetString("showiFolders.Text");
			this.showiFolders.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("showiFolders.TextAlign")));
			this.showiFolders.Visible = ((bool)(resources.GetObject("showiFolders.Visible")));
			this.showiFolders.Click += new System.EventHandler(this.showiFolders_Click);
			this.showiFolders.MouseEnter += new System.EventHandler(this.button_MouseEnter);
			this.showiFolders.MouseMove += new System.Windows.Forms.MouseEventHandler(this.button_MouseMove);
			this.showiFolders.MouseLeave += new System.EventHandler(this.button_MouseLeave);
			this.showiFolders.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
			// 
			// panel2
			// 
			this.panel2.AccessibleDescription = resources.GetString("panel2.AccessibleDescription");
			this.panel2.AccessibleName = resources.GetString("panel2.AccessibleName");
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panel2.Anchor")));
			this.panel2.AutoScroll = ((bool)(resources.GetObject("panel2.AutoScroll")));
			this.panel2.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panel2.AutoScrollMargin")));
			this.panel2.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panel2.AutoScrollMinSize")));
			this.panel2.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.panel2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel2.BackgroundImage")));
			this.panel2.ContextMenu = this.iFolderContextMenu;
			this.panel2.Controls.Add(this.iFolderView);
			this.panel2.Controls.Add(this.localiFoldersHeading);
			this.panel2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panel2.Dock")));
			this.panel2.Enabled = ((bool)(resources.GetObject("panel2.Enabled")));
			this.panel2.Font = ((System.Drawing.Font)(resources.GetObject("panel2.Font")));
			this.panel2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panel2.ImeMode")));
			this.panel2.Location = ((System.Drawing.Point)(resources.GetObject("panel2.Location")));
			this.panel2.Name = "panel2";
			this.panel2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panel2.RightToLeft")));
			this.panel2.Size = ((System.Drawing.Size)(resources.GetObject("panel2.Size")));
			this.panel2.TabIndex = ((int)(resources.GetObject("panel2.TabIndex")));
			this.panel2.Text = resources.GetString("panel2.Text");
			this.panel2.Visible = ((bool)(resources.GetObject("panel2.Visible")));
			this.panel2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel2_MouseDown);

			//
			//iFolderView
			//
			this.iFolderView.AccessibleDescription = resources.GetString("iFolderView.AccessibleDescription");
			this.iFolderView.AccessibleName = resources.GetString("iFolderView.AccessibleName");
			this.iFolderView.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("iFolderView.Anchor")));
			this.iFolderView.AutoScroll = ((bool)(resources.GetObject("iFolderView.AutoScroll")));
			this.iFolderView.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("iFolderView.AutoScrollMargin")));
			this.iFolderView.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("iFolderView.AutoScrollMinSize")));
			this.iFolderView.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.iFolderView.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("iFolderView.BackgroundImage")));
			this.iFolderView.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("iFolderView.Dock")));
			this.iFolderView.Enabled = ((bool)(resources.GetObject("iFolderView.Enabled")));
			this.iFolderView.Font = ((System.Drawing.Font)(resources.GetObject("iFolderView.Font")));
			this.iFolderView.HorizontalSpacing = 5;
			this.iFolderView.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("iFolderView.ImeMode")));
			this.iFolderView.ItemHeight = 72;
			this.iFolderView.ItemWidth = 280;
			this.iFolderView.LargeImageList = null;
			this.iFolderView.Location = ((System.Drawing.Point)(resources.GetObject("iFolderView.Location")));
			this.iFolderView.Name = "iFolderView";
			this.iFolderView.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("iFolderView.RightToLeft")));
			this.iFolderView.SelectedItem = null;
			this.iFolderView.Size = ((System.Drawing.Size)(resources.GetObject("iFolderView.Size")));
			this.iFolderView.TabIndex = ((int)(resources.GetObject("iFolderView.TabIndex")));
			this.iFolderView.VerticleSpacing = 5;
			this.iFolderView.Visible = ((bool)(resources.GetObject("iFolderView.Visible")));
			this.iFolderView.LastItemRemoved += new Novell.FormsTrayApp.TileListView.LastItemRemovedDelegate(this.iFolderView_LastItemRemoved);
			this.iFolderView.NavigateItem += new Novell.FormsTrayApp.TileListView.NavigateItemDelegate(this.iFolderView_NavigateItem);
			this.iFolderView.DoubleClick += new System.EventHandler(this.iFolderView_DoubleClick);
			this.iFolderView.SelectedIndexChanged += new Novell.FormsTrayApp.TileListView.SelectedIndexChangedDelegate(this.ifListView_SelectedIndexChanged);

			// 
			// localiFoldersHeading
			// 
			this.localiFoldersHeading.AccessibleDescription = resources.GetString("localiFoldersHeading.AccessibleDescription");
			this.localiFoldersHeading.AccessibleName = resources.GetString("localiFoldersHeading.AccessibleName");
			this.localiFoldersHeading.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("localiFoldersHeading.Anchor")));
			this.localiFoldersHeading.AutoSize = ((bool)(resources.GetObject("localiFoldersHeading.AutoSize")));
			this.localiFoldersHeading.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("localiFoldersHeading.BackgroundImage")));
			this.localiFoldersHeading.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.localiFoldersHeading.BulletIndent = ((int)(resources.GetObject("localiFoldersHeading.BulletIndent")));
			this.localiFoldersHeading.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.localiFoldersHeading.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("localiFoldersHeading.Dock")));
			this.localiFoldersHeading.Enabled = ((bool)(resources.GetObject("localiFoldersHeading.Enabled")));
			this.localiFoldersHeading.Font = ((System.Drawing.Font)(resources.GetObject("localiFoldersHeading.Font")));
			this.localiFoldersHeading.ForeColor = System.Drawing.SystemColors.Desktop;
			this.localiFoldersHeading.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("localiFoldersHeading.ImeMode")));
			this.localiFoldersHeading.Location = ((System.Drawing.Point)(resources.GetObject("localiFoldersHeading.Location")));
			this.localiFoldersHeading.MaxLength = ((int)(resources.GetObject("localiFoldersHeading.MaxLength")));
			this.localiFoldersHeading.Multiline = ((bool)(resources.GetObject("localiFoldersHeading.Multiline")));
			this.localiFoldersHeading.Name = "localiFoldersHeading";
			this.localiFoldersHeading.ReadOnly = true;
			this.localiFoldersHeading.RightMargin = ((int)(resources.GetObject("localiFoldersHeading.RightMargin")));
			this.localiFoldersHeading.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("localiFoldersHeading.RightToLeft")));
			this.localiFoldersHeading.ScrollBars = ((System.Windows.Forms.RichTextBoxScrollBars)(resources.GetObject("localiFoldersHeading.ScrollBars")));
			this.localiFoldersHeading.Size = ((System.Drawing.Size)(resources.GetObject("localiFoldersHeading.Size")));
			this.localiFoldersHeading.TabIndex = ((int)(resources.GetObject("localiFoldersHeading.TabIndex")));
			this.localiFoldersHeading.TabStop = false;
			this.localiFoldersHeading.Text = resources.GetString("localiFoldersHeading.Text");
			this.localiFoldersHeading.Visible = ((bool)(resources.GetObject("localiFoldersHeading.Visible")));
			this.localiFoldersHeading.WordWrap = ((bool)(resources.GetObject("localiFoldersHeading.WordWrap")));
			this.localiFoldersHeading.ZoomFactor = ((System.Single)(resources.GetObject("localiFoldersHeading.ZoomFactor")));
			// 
			// searchTimer
			// 
			this.searchTimer.Interval = 1000;
			this.searchTimer.Tick += new EventHandler(searchTimer_Tick);
			//
			// RefreshTimer
			//
			this.refreshTimer.Interval = 300000;  // 5 mins default
			this.refreshTimer.Tick += new EventHandler(refreshTimer_Tick);
			// 
			// GlobalProperties
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.statusBar1);
			this.Controls.Add(this.groupBox1);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.KeyPreview = true;
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.Menu = this.mainMenu1;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "GlobalProperties";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GlobalProperties_KeyDown);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.GlobalProperties_Closing);
			this.SizeChanged += new System.EventHandler(this.GlobalProperties_SizeChanged);
			this.Load += new System.EventHandler(this.GlobalProperties_Load);
			this.Move += new System.EventHandler(this.GlobalProperties_Move);
			this.Resize += new System.EventHandler(this.GlobalProperties_SizeChanged);
			this.VisibleChanged += new System.EventHandler(this.GlobalProperties_VisibleChanged);
			this.panel1.ResumeLayout(false);
			this.iFolderActions.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

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
			set { preferences = value; }
		}

		/// <summary>
		/// Set the SyncLog dialog.
		/// </summary>
		public SyncLog SyncLogDialog
		{
			set { syncLog = value; }
		}
		#endregion

		#region Public Methods
		public bool AcceptiFolder( iFolderWeb ifolder )
		{
			bool result = false;

			string selectedPath = string.Empty;
			FolderBrowserDialog browserDialog = new FolderBrowserDialog();
			Cursor.Current = Cursors.WaitCursor;
			while (true)
			{
				browserDialog.ShowNewFolderButton = true;
				browserDialog.SelectedPath = selectedPath;
				browserDialog.Description = string.Format( resourceManager.GetString("acceptDescription"), ifolder.Name );
				DialogResult dialogResult = browserDialog.ShowDialog();
				if ( dialogResult == DialogResult.OK )
				{
					browserDialog.Dispose();
					//ensure UI is re-painted
					Invalidate();
					Update();
					result = acceptiFolder( ifolder, browserDialog.SelectedPath );
					if ( result )
					{
						DownloadPath = browserDialog.SelectedPath + ifolder.Name;;
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

			return result;
		}

		/// <summary>
		/// Adds the specified domain to the dropdown list.
		/// </summary>
		/// <param name="domainInfo">The DomainInformation object to add to the list.</param>
		public void AddDomainToList(DomainInformation domainInfo)
		{
			lock ( iFolderListViews )
			{
				iFoldersListView ifListView = (iFoldersListView)iFolderListViews[ domainInfo.ID ];
				if ( ifListView == null )
				{
					// Add the domain.
					ifListView = new iFoldersListView( domainInfo, largeImageList );
					ifListView.SelectedIndexChanged += new Novell.FormsTrayApp.iFoldersListView.SelectedIndexChangedDelegate(ifListView_SelectedIndexChanged);
					ifListView.DoubleClick += new EventHandler(iFolderView_DoubleClick);
					ifListView.NavigateItem += new Novell.FormsTrayApp.iFoldersListView.NavigateItemDelegate(iFolderView_NavigateItem);

					iFolderListViews.Add( domainInfo.ID, ifListView );

					updateWidth();

					ifListView.Visible = !hide;
					panel2.Controls.Add( ifListView );
					updateView();
				}
			}

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
			DomainInformation domainInfo = null;

			lock ( iFolderListViews )
			{
				iFoldersListView ifListView = (iFoldersListView)iFolderListViews[ domainID ];
				if ( ifListView != null )
				{
					domainInfo = ifListView.DomainInfo;
					foreach (TileListViewItem tlvi in ifListView.Items)
					{
						if ( tlvi.Selected )
						{
							selectedItem = null;
							updateMenus(null);
						}

						ht.Remove( ((iFolderObject)tlvi.Tag).ID );
					}

					// Remove the domain.
					iFolderListViews.Remove( domainID );

					updateWidth();

					panel2.Controls.Remove( ifListView );
					updateView();
				}

				// Reset the default domain.
				if ( defaultDomainID != null )
				{
					ifListView = (iFoldersListView)iFolderListViews[ defaultDomainID ];
					if ( ifListView != null )
					{
						ifListView.DomainInfo.IsDefault = true;
					}
				}
			}

			if (domainInfo != null)
			{
				// Update the domain list file.
				removeDomainFromFile(domainInfo, defaultDomainID);

				if (RemoveDomain != null)
				{
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
		/// Updates the specified domain.
		/// </summary>
		/// <param name="domainInfo">The DomainInformation object representing the domain to update.</param>
		public void UpdateDomain(DomainInformation domainInfo)
		{
/*			foreach (Domain d in servers.Items)
			{
				if (d.ID.Equals(domainInfo.ID))
				{
					d.DomainInfo = domainInfo;
					break;
				}
			}

			if ((iFolderView.SelectedItems.Count == 1) && 
				!((iFolderObject)iFolderView.SelectedItems[0].Tag).iFolderWeb.IsSubscription &&
				((iFolderObject)iFolderView.SelectedItems[0].Tag).iFolderWeb.DomainID.Equals(domainInfo.ID))
			{
				menuSyncNow.Visible = menuActionSync.Enabled = toolBarSync.Enabled = domainInfo.Active;
			}*/
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
			ht.Remove( ((iFolderObject)tlvi.Tag).ID );
			tlvi.Remove();
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
							statusBar1.Text = string.Format(resourceManager.GetString("localSync"), syncEventArgs.Name);
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
								}
							}
						}
						break;
					}
					case Action.StartSync:
					{
						if (syncEventArgs.Name.StartsWith("POBox:"))
						{
							statusBar1.Text = resourceManager.GetString("checkingForiFolders");
						}
						else
						{
							statusBar1.Text = string.Format(resourceManager.GetString("synciFolder"), syncEventArgs.Name);
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
								}
							}
						}
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
									ifolderObject.iFolderState = syncEventArgs.Connected ? 
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
							}

							objectsToSync = 0;
						}

						statusBar1.Text = resourceManager.GetString("statusBar1.Text");
						if (initialConnect)
						{
							initialConnect = false;
							updateEnterpriseTimer.Start();
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
				if (syncEventArgs.SizeRemaining == syncEventArgs.SizeToSync)
				{
					progressBar1.Visible = syncEventArgs.SizeToSync > 0;
					progressBar1.Value = 0;
					progressBar1.Maximum = 200;

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
								tlvi.Status = string.Format(resourceManager.GetString("statusSyncingItems"), objectsToSync--);
							}
						}
					}

					switch (syncEventArgs.ObjectType)
					{
						case ObjectType.File:
							if (syncEventArgs.Delete)
							{
								statusBar1.Text = string.Format(resourceManager.GetString("deleteClientFile"), syncEventArgs.Name);
							}
							else
							{
								switch (syncEventArgs.Direction)
								{
									case Direction.Uploading:
										statusBar1.Text = string.Format(resourceManager.GetString("uploadFile"), syncEventArgs.Name);
										break;
									case Direction.Downloading:
										statusBar1.Text = string.Format(resourceManager.GetString("downloadFile"), syncEventArgs.Name);
										break;
									case Direction.Local:
										statusBar1.Text = string.Format(resourceManager.GetString("localFile"), syncEventArgs.Name);
										break;
									default:
										statusBar1.Text = string.Format(resourceManager.GetString("syncingFile"), syncEventArgs.Name);
										break;
								}
							}
							break;
						case ObjectType.Directory:
							if (syncEventArgs.Delete)
							{
								statusBar1.Text = string.Format(resourceManager.GetString("deleteClientDir"), syncEventArgs.Name);
							}
							else
							{
								switch (syncEventArgs.Direction)
								{
									case Direction.Uploading:
										statusBar1.Text = string.Format(resourceManager.GetString("uploadDir"), syncEventArgs.Name);
										break;
									case Direction.Downloading:
										statusBar1.Text = string.Format(resourceManager.GetString("downloadDir"), syncEventArgs.Name);
										break;
									case Direction.Local:
										statusBar1.Text = string.Format(resourceManager.GetString("localDir"), syncEventArgs.Name);
										break;
									default:
										statusBar1.Text = string.Format(resourceManager.GetString("syncingDir"), syncEventArgs.Name);
										break;
								}
							}
							break;
						case ObjectType.Unknown:
							statusBar1.Text = string.Format(resourceManager.GetString("deleteUnknown"), syncEventArgs.Name);
							break;
					}
				}
				else
				{
					statusBar1.Text = syncEventArgs.Name;
					progressBar1.Value = syncEventArgs.SizeToSync > 0 ? (int)(((syncEventArgs.SizeToSync - syncEventArgs.SizeRemaining) * 200) / syncEventArgs.SizeToSync) : progressBar1.Maximum;
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
						addiFolderToListView(new iFolderObject(ifolder, iFolderState.Normal));

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
			//MessageBox.Show("Called addifoldertolistview");
			iFolderWeb ifolder = ifolderObject.iFolderWeb;
			if ( !ifolder.IsSubscription )
			{
			//	MessageBox.Show("Not a subscription");
				lock (ht)
				{
					// Add only if it isn't already in the list.
					if (ht[ifolder.ID] == null)
					{
						TileListViewItem tlvi = new TileListViewItem( ifolderObject );
						int imageIndex;
						tlvi.Status = getItemState( ifolderObject, 0, out imageIndex );
						tlvi.ImageIndex = imageIndex;
						iFolderView.Items.Add(tlvi);
						iFolderView.Items.Sort();
						// Add the listviewitem to the hashtable.
						ht.Add(ifolder.ID, tlvi);
			//			MessageBox.Show(String.Format("Added a new Item to iFolderView: hashtable count: {0}", ht.Count));
					}
					else
					{
			//			MessageBox.Show("The folder is already present in ht:");
					}
				}

				// Notify the shell.
				Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, ifolder.UnManagedPath, IntPtr.Zero);
			}
			else
			{
				lock( ht )
				{
					if (ht[ifolder.ID] == null)
					{
						TileListViewItem tlvi = addiFolderToAvailableListView( ifolderObject );
						ht.Add( ifolder.ID, tlvi );
					}
				}
			}

			updateView();
		}

		private TileListViewItem addiFolderToAvailableListView( iFolderObject ifolderObject )
		{
			TileListViewItem tlvi;

			lock ( iFolderListViews )
			{
				iFoldersListView ifListView = (iFoldersListView)iFolderListViews[ ifolderObject.iFolderWeb.DomainID ];
				// TODO: what if the listview isn't in the list?

				tlvi = ifListView.AddiFolderToListView( ifolderObject );
				ifListView.Items.Sort();
				int imageIndex;
				tlvi.Status = getItemState( ifolderObject, 0, out imageIndex );
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

			iFolderView.Width = minWidth - iFolderView.Left;
		}

		private void updateView()
		{
			Point point = localiFoldersHeading.Location;
			point.Y += localiFoldersHeading.Height;
			if ( iFolderView.Items.Count == 0 )
			{
				iFolderView.Visible = false;
				if( this.filter.Text.Length > 0)
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

			if (ifolder.State.Equals("Available") &&
				(ifWebService.GetiFolder(ifolder.CollectionID) != null))
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
			}
		}

		private string getItemState( iFolderObject ifolderObject, uint objectsToSync, out int imageIndex )
		{
			string status;
			imageIndex = 0;

			if (ifolderObject.iFolderWeb.HasConflicts)
			{
				imageIndex = 6;
				status = resourceManager.GetString("statusConflicts");
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
								status = string.Format( resourceManager.GetString("statusSynced"), ifolderObject.iFolderWeb.LastSyncTime );
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
									status = resourceManager.GetString(ifolderObject.iFolderWeb.State);
								}
								break;
							default:
								// TODO: what icon to use for unknown status?
								imageIndex = 0;
								status = resourceManager.GetString("statusUnknown");
								break;
						}
						break;
					}
					case iFolderState.Initial:
						imageIndex = 4;
						status = "Initial "+ifolderObject.iFolderWeb.State;
						break;
					case iFolderState.Disconnected:
						imageIndex = 5;
						status = resourceManager.GetString("disconnected");
						break;
					case iFolderState.FailedSync:
						imageIndex = 6;
						status = objectsToSync == 0 ?
							resourceManager.GetString("statusSyncFailure") :
							string.Format(resourceManager.GetString("statusSyncItemsFailed"), objectsToSync);
						break;
					case iFolderState.Synchronizing:
						imageIndex = 1;
						status = objectsToSync == 0 ?
							resourceManager.GetString("statusSyncing") :
							string.Format(resourceManager.GetString("statusSyncingItems"), objectsToSync);
						break;
					case iFolderState.SynchronizingLocal:
						imageIndex = 1;
						status = resourceManager.GetString("preSync");
						break;
					default:
						// TODO: what icon to use for unknown status?
						imageIndex = 0;
						status = resourceManager.GetString("statusUnknown");
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
			refreshThread = new Thread(new ThreadStart(updateiFolders));
			refreshThread.IsBackground = true;
			refreshThread.Priority = ThreadPriority.BelowNormal;
			refreshThread.Start();

//			refreshiFolders(/*domain*/);

			// Call to sync the POBoxes.
/*			if (domain.ShowAll)
			{
				foreach (Domain d in servers.Items)
				{
					if (!d.ShowAll)
					{
						try
						{
							ifWebService.SynciFolderNow(d.DomainInfo.POBoxID);
						}
						catch {}
					}
				}
			}
			else
			{
				try
				{
					ifWebService.SynciFolderNow(domain.DomainInfo.POBoxID);
				}
				catch {}
			}*/
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

		public void refreshiFolders(string search/*Domain domain*/)
		{
			search = this.filter.Text;
			Cursor.Current = Cursors.WaitCursor;
			Hashtable oldHt = new Hashtable();
			lock(ht)
			{
				// Save the state of the old items so state can be preserved between refreshes.
				foreach ( TileListViewItem tlvi in ht.Values )
				{
					iFolderObject ifolderObject = (iFolderObject)tlvi.Tag;
					oldHt.Add( ifolderObject.ID, ifolderObject.iFolderState );
				}					
				ht.Clear();
			}
			try
			{
				// Get the list of iFolders - now the calling thread does this and also handles exception
			//	if( search != null)
			//		ifolderArray = ifWebService.GetAlliFolders();
				panel2.SuspendLayout();
				iFolderView.Items.Clear();
				selectedItem = null;
				updateMenus( null );

				// Clear the available iFolder views.
				foreach (iFoldersListView ifListView in iFolderListViews.Values)
				{
					ifListView.InitializeUpdate();
				//	ifListView.setVisible = false;
				}
				
				// Walk the list of iFolders and add them to the listviews.
				foreach (iFolderWeb ifolder in ifolderArray) //iFolderArray is a class member should be made null for GC
				{
					if( search != null && ((String)ifolder.Name).ToLower().IndexOf(search.ToLower(), 0, ((String)ifolder.Name).Length) < 0)
						continue;
					iFolderState state = iFolderState.Normal;
					if (!ifolder.IsSubscription)
					{
						if ( oldHt.Contains( ifolder.ID ) )
						{
							state = (iFolderState)oldHt[ ifolder.ID ];
						}
					}
					if( this.acceptedFolders.Contains(ifolder.ID))
					{
						this.acceptedFolders.Remove(ifolder.ID);
					}
					addiFolderToListView(new iFolderObject(ifolder, state));
				}
				// Check whether or not the count is same..
				foreach( System.Object obj in this.acceptedFolders.Values)
				{
					TileListViewItem tlv = (TileListViewItem)obj;
					iFolderObject ifobj = (iFolderObject)tlv.Tag;
					if( search != null && ((String)ifobj.iFolderWeb.Name).ToLower().IndexOf(search.ToLower(), 0, ((String)ifobj.iFolderWeb.Name).Length) < 0)
						continue;
					ifobj.iFolderWeb.IsSubscription = false;
					ifobj.iFolderState = iFolderState.Initial;
					addiFolderToListView( ifobj );
				}
			}
			catch (Exception ex)
			{
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("iFolderError"), resourceManager.GetString("iFolderErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
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
			inRefresh = false;
		//	ifolderArray = null;
			Cursor.Current = Cursors.Default;
			this.refreshTimer.Start();
		}

		private void invokeiFolderProperties(TileListViewItem tlvi, int activeTab)
		{
			iFolderAdvanced ifolderAdvanced = new iFolderAdvanced();
			ifolderAdvanced.CurrentiFolder = ((iFolderObject)tlvi.Tag).iFolderWeb;
			ifolderAdvanced.DomainName = ((DomainInformation)this.simiasWebService.GetDomainInformation(((iFolderObject)tlvi.Tag).iFolderWeb.DomainID)).Name;
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
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("syncError"), resourceManager.GetString("syncErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
				mmb.Dispose();
			}
		}

		private void updateMenus(iFolderObject ifolderObject)
		{
			if ( ifolderObject == null )
			{
				// Hide the button panels.
				iFolderActions.Visible = false;

				// Disable all of the item-related menu items.
				this.menuActionOpen.Enabled = this.menuActionAccept.Enabled = this.menuActionProperties.Enabled =
					this.menuActionRemove.Enabled = this.menuActionResolve.Enabled = this.menuActionRevert.Enabled =
					this.menuActionShare.Enabled = this.menuActionSync.Enabled = false;
			}
			else
			{
				iFolderWeb ifolderWeb = ifolderObject.iFolderWeb;
				iFolderActions.Visible = true;

				if ( ifolderWeb.IsSubscription )
				{
					// Disable the iFolder related menu items.
					this.menuActionOpen.Enabled = this.menuActionProperties.Enabled =
						this.menuActionRevert.Enabled = this.menuActionShare.Enabled =
						this.menuActionSync.Enabled = this.menuActionResolve.Enabled = false;

					// Hide the local iFolder buttons
					this.open.Visible = this.syncNow.Visible = this.share.Visible =
						this.revert.Visible = this.properties.Visible = this.resolve.Visible = false;

					// Enable the available iFolder related menu items.
					this.menuActionRemove.Enabled = this.menuActionAccept.Enabled = true;
					if( ifolderWeb.CurrentUserID != ifolderWeb.OwnerID)
					{
						this.menuRemove.Text = this.remove.Text = this.menuActionRemove.Text = this.resourceManager.GetString("RemoveMyMembership");
					}
					else
					{
						this.menuRemove.Text = this.remove.Text = this.menuActionRemove.Text = this.resourceManager.GetString("menuActionRemove.Text");
					}
					// Show the available iFolder buttons
					this.accept.Visible = this.remove.Visible = true;
				}
				else
				{
					// Disable the available iFolder related menu items.
					this.menuActionRemove.Enabled = this.menuActionAccept.Enabled = false;

					// Hide the available iFolder buttons
					this.accept.Visible = this.remove.Visible = false;

					// Enable the iFolder related menu items.
					this.menuActionOpen.Enabled = this.menuActionProperties.Enabled =
						this.menuActionRevert.Enabled = this.menuActionShare.Enabled =
						this.menuActionSync.Enabled = true;

					int buttonDelta = resolve.Top - open.Top;

					if ( ifolderWeb.HasConflicts )
					{
						this.menuActionResolve.Enabled = this.resolve.Visible = true;

						// Move the buttons
						syncNow.Top = resolve.Top + buttonDelta;
						share.Top = syncNow.Top + buttonDelta;
						revert.Top = share.Top + buttonDelta;
						properties.Top = revert.Top + buttonDelta;
					}
					else
					{
						this.menuActionResolve.Enabled = this.resolve.Visible = false;

						// Move the buttons
						syncNow.Top = resolve.Top;
						share.Top = syncNow.Top + buttonDelta;
						revert.Top = share.Top + buttonDelta;
						properties.Top = revert.Top + buttonDelta;
					}
					if( ifolderObject.iFolderState == iFolderState.Initial )
					{
						this.open.Visible = this.share.Visible = this.revert.Visible = this.properties.Visible = false;

						this.menuOpen.Enabled = this.menuShare.Enabled = false; 
						this.menuRevert.Enabled = this.menuProperties.Enabled = false;

						this.menuActionOpen.Enabled = this.menuActionShare.Enabled = false;
						this.menuActionRevert.Enabled = this.menuActionProperties.Enabled = false;
						return;
					}
					// Show the local iFolder buttons
					this.open.Visible = this.syncNow.Visible = this.share.Visible =
					this.revert.Visible = this.properties.Visible = true;
					// Show right-click menu
					this.menuOpen.Enabled = this.menuShare.Enabled = true; 
					this.menuRevert.Enabled = this.menuProperties.Enabled = true;
				}
			}
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

		private bool acceptiFolder( iFolderWeb ifolder, string path )
		{
			bool result = true;

			if (GetDriveType(Path.GetPathRoot(path)) == DRIVE_FIXED)
			{
				// Check to make sure the user has rights to this directory.
				if (Win32Security.AccessAllowed(path))
				{
					try
					{
						// Display wait cursor.
						Cursor = Cursors.WaitCursor;

						// Accept the invitation.
						ifWebService.AcceptiFolderInvitation(ifolder.DomainID, ifolder.ID, path);
						Cursor = Cursors.Default;
					}
					catch (Exception ex)
					{
						Cursor = Cursors.Default;
						MyMessageBox mmb;

						if (ex.Message.IndexOf("PathExists") != -1)
						{
							mmb = new MyMessageBox(resourceManager.GetString("pathExistsError"), resourceManager.GetString("pathInvalidErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
							mmb.ShowDialog();
						}
						else if (ex.Message.IndexOf("AtOrInsideStorePath") != -1)
						{
							mmb = new MyMessageBox(resourceManager.GetString("pathInStoreError"), resourceManager.GetString("pathInvalidErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
							mmb.ShowDialog();
						}
						else if (ex.Message.IndexOf("AtOrInsideCollectionPath") != -1)
						{
							mmb = new MyMessageBox(resourceManager.GetString("pathIniFolderError"), resourceManager.GetString("pathInvalidErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
							mmb.ShowDialog();
						}
						else if (ex.Message.IndexOf("IncludesWinDirPath") != -1)
						{
							mmb = new MyMessageBox(resourceManager.GetString("pathIncludesWinDirError"), resourceManager.GetString("pathInvalidErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
							mmb.ShowDialog();
						}
						else if (ex.Message.IndexOf("IncludesProgFilesPath") != -1)
						{
							mmb = new MyMessageBox(resourceManager.GetString("pathIncludesProgFilesDirError"), resourceManager.GetString("pathInvalidErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
							mmb.ShowDialog();
						}
						else
						{
							mmb = new MyMessageBox(resourceManager.GetString("acceptError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
							mmb.ShowDialog();
						}

						result = false;
					}
				}
				else
				{
					result = false;
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("accessDenied"), string.Empty, string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}
			}
			else
			{
				MessageBox.Show(resourceManager.GetString("networkPath"), resourceManager.GetString("pathInvalidErrorTitle"));
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
			if ( e.Button.Equals( MouseButtons.Left ) )
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
		}

		private void iFolderContextMenu_Popup(object sender, System.EventArgs e)
		{
			if ( selectedItem == null )
			{
				// Display the refresh menu.
				menuRefresh.Visible = true;

				// Hide the other menu items.
				menuOpen.Visible = menuSyncNow.Visible = menuShare.Visible =
					menuRevert.Visible = menuProperties.Visible = menuRemove.Visible =
					menuAccept.Visible = menuResolve.Visible = menuResolveSeparator.Visible =
					menuSeparator1.Visible = menuSeparator2.Visible = false;
			}
			else
			{
				// Hide the refresh menu.
				menuRefresh.Visible = false;

				iFolderWeb ifolder = ((iFolderObject)selectedItem.Tag).iFolderWeb;

				if ( ifolder.IsSubscription )
				{
					// Hide the iFolder menus.
					menuOpen.Visible = menuSyncNow.Visible = menuShare.Visible =
						menuRevert.Visible = menuProperties.Visible = menuResolve.Visible = 
						menuResolveSeparator.Visible = menuSeparator2.Visible = false;

					// Display the subscription-related menus based on Refresh thread.
					if(!inRefresh)
						menuAccept.Visible = menuRemove.Visible = menuSeparator1.Visible = true;
					else
						menuAccept.Visible = menuRemove.Visible = menuSeparator1.Visible = false;
				}
				else
				{
					// Hide the subscription-related menus.
					menuAccept.Visible = menuRemove.Visible = false;

					// Display the iFolder menus based on Refresh thread.
					menuOpen.Visible = menuSyncNow.Visible = menuShare.Visible = true;
					if(!inRefresh)
						menuRevert.Visible = true;
					else
						menuRevert.Visible = false;
					
					menuProperties.Visible = 
						menuSeparator2.Visible = true;

					menuResolve.Visible = 
						menuResolveSeparator.Visible = ifolder.HasConflicts;
				}
			}
		}

		private void panel2_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if ( selectedItem != null )
			{
				selectedItem.Selected = false;
				selectedItem = null;
				// Disable the iFolder actions...
				// Hide the button panels.
				iFolderActions.Visible = false;

				// Disable all of the item-related menu items.
				this.menuActionOpen.Enabled = this.menuActionAccept.Enabled = this.menuActionProperties.Enabled =
					this.menuActionRemove.Enabled = this.menuActionResolve.Enabled = this.menuActionRevert.Enabled =
					this.menuActionShare.Enabled = this.menuActionSync.Enabled = false;
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
				localiFoldersHeading.Width = (int)(textSize.Width * 1.1);
			}
			finally
			{
				g.Dispose();
			}

			iFolderActions.Visible = false;

//			Call the refresh thread
			this.refreshTimer.Interval = Math.Abs(this.ifWebService.GetDefaultSyncInterval()* 1000);
		//	MessageBox.Show(string.Format("Setting default sync interval to {0}. Calling refreash.", this.refreshTimer.Interval));
			refreshAll();
			this.refreshTimer.Start();

			showiFolders_Click( this, null );
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
			this.Dispose();
			this.Close();
			FormsTrayApp.CloseApp();
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
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(string.Format(resourceManager.GetString("iFolderOpenError"), ifolder.Name), resourceManager.GetString("openErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
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

				RevertiFolder revertiFolder = new RevertiFolder();
				if( !IsMaster )
					revertiFolder.removeFromServer.Text = this.resourceManager.GetString("AlsoRemoveMembership");
				if ( revertiFolder.ShowDialog() == DialogResult.Yes )
				{
					//ensure UI is re-painted before Revert is done
					Invalidate();
					Update();
					Cursor.Current = Cursors.WaitCursor;
					
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
							if( IsMaster )
							{
								string defaultiFolderID = this.simiasWebService.GetDefaultiFolder( newiFolder.DomainID );
								if( defaultiFolderID == newiFolder.ID)
								{
									//MessageBox.Show("Setting default iFolder ID to null");
									this.simiasWebService.DefaultAccount( newiFolder.DomainID, null );
								}
								ifWebService.DeleteiFolder(newiFolder.DomainID, newiFolder.ID);
							}
							ifWebService.DeclineiFolderInvitation( newiFolder.DomainID, newiFolder.ID );
						}
						else
						{
							// Add the item to the available iFolders list.
							TileListViewItem tlvi = addiFolderToAvailableListView( new iFolderObject( newiFolder, iFolderState.Normal ) );

							lock ( ht )
							{
							//	acceptedFolders.Remove(newiFolder.ID);
								ht.Remove(newiFolder.ID);
								ht.Add( newiFolder.ID, tlvi );
							}
						}
					}

					lock (ht)
					{
						removeTileListViewItem( selectedItem );
					}
				}
				updateView();
				revertiFolder.Dispose();
			}
			catch (Exception ex)
			{		
				Cursor.Current = Cursors.Default;
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("iFolderRevertError"), resourceManager.GetString("revertErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
				mmb.Dispose();
			}

			Cursor.Current = Cursors.Default;
		}

		private void menuResolve_Click(object sender, System.EventArgs e)
		{
			ConflictResolver conflictResolver = new ConflictResolver();
			conflictResolver.iFolder = ((iFolderObject)selectedItem.Tag).iFolderWeb;
			conflictResolver.iFolderWebService = ifWebService;
			conflictResolver.LoadPath = Application.StartupPath;
			conflictResolver.Show();
		}

		private void menuShare_Click(object sender, System.EventArgs e)
		{
			invokeiFolderProperties( selectedItem, 1 );
		}

		private void menuSyncNow_Click(object sender, System.EventArgs e)
		{
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
			DomainItem selectedDomainItem = new DomainItem( defaultDomainInfo.Name, defaultDomainInfo.ID );
			foreach ( iFoldersListView ifListView in iFolderListViews.Values )
			{
				DomainItem domainItem = new DomainItem(ifListView.DomainInfo.Name, ifListView.DomainInfo.ID);
				domains.Add(domainItem);
			}

			CreateiFolder createiFolder = new CreateiFolder();
			createiFolder.Servers = domains;
			createiFolder.SelectedDomain = selectedDomainItem;
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
			if( this.inRefresh == false )
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

		private void menuAccept_Click(object sender, System.EventArgs e)
		{
			if ( selectedItem != null )
			{
				iFolderWeb ifolder = ((iFolderObject)selectedItem.Tag).iFolderWeb;
				iFolderObject ifobj = new iFolderObject(((iFolderObject)selectedItem.Tag).iFolderWeb, iFolderState.Disconnected);
//				iFolderWeb ifolderWeb = ((iFolderObject)selectedItem.Tag).iFolderWeb;
				//TileListViewItem tlvi = new TileListViewItem(ifobj);
				
				// Accept the iFolder.
				if ( AcceptiFolder( ifolder ) )
				{
					ifobj.iFolderWeb.IsSubscription = false;
					lock (ht)
					{
						//MessageBox.Show("Ramesh: Removing from the tilelistview");
						removeTileListViewItem( selectedItem );
						//MessageBox.Show("Ramesh: Adding to list view");					
						addiFolderToListView(new iFolderObject(ifolder, iFolderState.Initial));
						if( acceptedFolders.Contains(ifobj.iFolderWeb.ID) )
							acceptedFolders.Remove(ifobj.iFolderWeb.ID);
						ifobj.iFolderWeb.UnManagedPath = DownloadPath;
						TileListViewItem tlvi = new TileListViewItem(ifobj);
						acceptedFolders.Add(ifobj.iFolderWeb.ID, tlvi);
					//	iFolderView.Items.Add(tlvi);
					//	ht.Add( ifobj.iFolderWeb.DomainID, tlvi);
					//	addiFolderToAvailableListView();
					}
				}
			}
		}

		public void UpdateDisplay( iFolderWeb ifolderWeb, string DownloadPath)
		{
			iFolderObject ifobj = new iFolderObject(ifolderWeb, iFolderState.Initial);
			addiFolderToListView(new iFolderObject(ifolderWeb, iFolderState.Initial));
			if( acceptedFolders.Contains(ifolderWeb.ID) )
				acceptedFolders.Remove(ifolderWeb.ID);
			ifolderWeb.UnManagedPath = DownloadPath;
			TileListViewItem tlvi = new TileListViewItem(ifobj);
			acceptedFolders.Add(ifolderWeb.ID, tlvi);
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
						string.Format( resourceManager.GetString("deleteiFolder"), ifolder.Name ),
						resourceManager.GetString("removeTitle"),
						string.Empty,
						MyMessageBoxButtons.YesNo,
						MyMessageBoxIcon.Question,
						MyMessageBoxDefaultButton.Button2);
				}
				else
				{
					mmb = new Novell.iFolderCom.MyMessageBox(
						 this.resourceManager.GetString("RemoveMembershipMesg") ,
						string.Format(this.resourceManager.GetString("RemoveMembershipTitle"), ifolder.Name ),
						string.Empty,
						MyMessageBoxButtons.YesNo,
						MyMessageBoxIcon.Question,
						MyMessageBoxDefaultButton.Button2);
				}
				if (mmb.ShowDialog() == DialogResult.Yes)
				{
					try
					{
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
						mmb = new MyMessageBox(resourceManager.GetString("declineError"), resourceManager.GetString("errorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
					}
				}

				mmb.Dispose();
			}
		}

		private void showiFolders_Click(object sender, System.EventArgs e)
		{
			hide = !hide;

			showiFolders.Text = hide ? resourceManager.GetString("viewAvailableiFolders") :
				resourceManager.GetString("showiFolders.Text");
			menuViewAvailable.Checked = !hide;

			// Hide the server listview controls.
			foreach ( iFoldersListView ifListView in iFolderListViews.Values )
			{
				ifListView.Visible = !hide;
			}

			updateView();
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
					this.shutdown = true;
					break;
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
			ResetPassphrase resetPassphraseWindow = new ResetPassphrase(this.simiasWebService);
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
			ImportKeysDialog importKeys = new ImportKeysDialog(this.simiasWebService);
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

		private void filter_TextChanged(object sender, EventArgs e)
		{
			// filter text changed.....
			//if( this.filter.Text.Length > 0)
			if( this.filter.Focused)
			{
				this.searchTimer.Stop();
				this.searchTimer.Start();
			}
				//refreshiFolders(this.filter.Text);
		}

		private void searchTimer_Tick(object sender, EventArgs e)
		{
			// The timer event has been fired...
			// Stop the timer.
			searchTimer.Stop();
			//searchText = this.filter.Text;
			refreshiFolders(this.filter.Text);
		}

		private void refreshTimer_Tick(object sender, EventArgs e)
		{
		//	this.refreshTimer.Stop();
			if( this.Focused)
				this.refreshAll();
			else
			{
				this.refreshTimer.Stop();
				this.refreshTimer.Start();
			}
		//	this.refreshTimer.Start();
		}
	}
}

