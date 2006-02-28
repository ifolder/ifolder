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
		private ImageList largeImageList;
		private ListViewItem selectedItem;
		private bool hide = true;
		private NoiFolderMessage infoMessage;

		System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(GlobalProperties));
		private Preferences preferences;
		private SyncLog syncLog;
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
		private Domain defaultDomain = null;
		private string domainList;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem menuOpen;
		private System.Windows.Forms.MenuItem menuCreate;
		private System.Windows.Forms.MenuItem menuShare;
		private System.Windows.Forms.MenuItem menuRevert;
		private System.Windows.Forms.MenuItem menuProperties;
		private System.Windows.Forms.MenuItem menuRefresh;
		private System.Windows.Forms.MenuItem menuSeparator1;
		private System.Windows.Forms.MenuItem menuSeparator2;
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
		private System.Windows.Forms.MenuItem menuActionSeparator1;
		private System.Windows.Forms.MenuItem menuResolve;
		private System.Windows.Forms.MenuItem menuActionResolve;
		private System.Windows.Forms.MenuItem menuAccept;
		private System.Windows.Forms.MenuItem menuActionAccept;
		private System.Windows.Forms.MenuItem menuActionSeparator2;
		private System.Windows.Forms.MenuItem menuExit;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuHelp;
		private System.Windows.Forms.MenuItem menuHelpHelp;
		private System.Windows.Forms.MenuItem menuHelpAbout;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.MenuItem menuRemove;
		private System.Windows.Forms.MenuItem menuActionRemove;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuViewAccounts;
		private System.Windows.Forms.MenuItem menuViewLog;
		private System.Windows.Forms.MenuItem menuEdit;
		private System.Windows.Forms.MenuItem menuEditPrefs;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.RichTextBox localiFoldersHeading;
		private System.Windows.Forms.ListView iFolderView;
		private System.Windows.Forms.Button showiFolders;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox filter;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button create;
		private System.ComponentModel.IContainer components;
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

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			infoMessage = new NoiFolderMessage();
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
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"ifolder_app.ico"));

				// TODO: need to add the other icons.
				largeImageList = new ImageList();
				largeImageList.ImageSize = new Size( 32, 32 );
				largeImageList.Images.Add( new Icon( Path.Combine( Application.StartupPath, @"ifolder.ico" ) ) );

				// TODO: Are there different icons for local iFolders vs. server iFolders?
				iFolderView.LargeImageList = largeImageList;

				// TODO: need icons for the different states.
				//	- iFolder with conflicts.
				//	- iFolder that is available.
				//	- iFolder that has been requested.
				//	- iFolder that has been invited. (Invitation.ico?)
/*				this.iFolderView.SmallImageList = new ImageList();
				iFolderView.SmallImageList.Images.Add(new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_loaded.ico")));
				iFolderView.SmallImageList.Images.Add(new Icon(Path.Combine(Application.StartupPath, @"res\serverifolder.ico")));
				iFolderView.SmallImageList.Images.Add(new Icon(Path.Combine(Application.StartupPath, @"res\ifolderconflict.ico")));
*/
				// Add the normal image list to the toolbar.
/*				toolBar1.ImageList = new ImageList();
				toolBar1.ImageList.ImageSize = new Size(24, 24);
				toolBar1.ImageList.TransparentColor = Color.White;
				toolBar1.ImageList.Images.AddStrip(Image.FromFile(Path.Combine(Application.StartupPath, @"res\mtoolbar_nor.bmp")));

				// Add the disabled image list to the toolbar.
				toolBar1.DisabledImageList = new ImageList();
				toolBar1.DisabledImageList.ImageSize = new Size(24, 24);
				toolBar1.DisabledImageList.TransparentColor = Color.White;
				toolBar1.DisabledImageList.Images.AddStrip(Image.FromFile(Path.Combine(Application.StartupPath, @"res\mtoolbar_dis.bmp")));

				// Add the hot image list to the toolbar.
				toolBar1.HotImageList = new ImageList();
				toolBar1.HotImageList.ImageSize = new Size(24, 24);
				toolBar1.HotImageList.TransparentColor = Color.White;
				toolBar1.HotImageList.Images.AddStrip(Image.FromFile(Path.Combine(Application.StartupPath, @"res\mtoolbar_hot.bmp")));

				toolBarCreate.ImageIndex = 0;
				toolBarSetup.ImageIndex = 1;
				toolBarShare.ImageIndex = 2;
				toolBarResolve.ImageIndex = 3;
				toolBarSync.ImageIndex = 4;
*/			}
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(GlobalProperties));
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.menuOpen = new System.Windows.Forms.MenuItem();
			this.menuCreate = new System.Windows.Forms.MenuItem();
			this.menuRefresh = new System.Windows.Forms.MenuItem();
			this.menuAccept = new System.Windows.Forms.MenuItem();
			this.menuSeparator1 = new System.Windows.Forms.MenuItem();
			this.menuShare = new System.Windows.Forms.MenuItem();
			this.menuResolve = new System.Windows.Forms.MenuItem();
			this.menuSyncNow = new System.Windows.Forms.MenuItem();
			this.menuRevert = new System.Windows.Forms.MenuItem();
			this.menuRemove = new System.Windows.Forms.MenuItem();
			this.menuSeparator2 = new System.Windows.Forms.MenuItem();
			this.menuProperties = new System.Windows.Forms.MenuItem();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuAction = new System.Windows.Forms.MenuItem();
			this.menuActionCreate = new System.Windows.Forms.MenuItem();
			this.menuActionSeparator1 = new System.Windows.Forms.MenuItem();
			this.menuActionAccept = new System.Windows.Forms.MenuItem();
			this.menuActionRemove = new System.Windows.Forms.MenuItem();
			this.menuActionSeparator2 = new System.Windows.Forms.MenuItem();
			this.menuActionOpen = new System.Windows.Forms.MenuItem();
			this.menuActionShare = new System.Windows.Forms.MenuItem();
			this.menuActionResolve = new System.Windows.Forms.MenuItem();
			this.menuActionSync = new System.Windows.Forms.MenuItem();
			this.menuActionRevert = new System.Windows.Forms.MenuItem();
			this.menuActionProperties = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.menuExit = new System.Windows.Forms.MenuItem();
			this.menuEdit = new System.Windows.Forms.MenuItem();
			this.menuEditPrefs = new System.Windows.Forms.MenuItem();
			this.menuView = new System.Windows.Forms.MenuItem();
			this.menuViewRefresh = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuViewAccounts = new System.Windows.Forms.MenuItem();
			this.menuViewLog = new System.Windows.Forms.MenuItem();
			this.menuHelp = new System.Windows.Forms.MenuItem();
			this.menuHelpHelp = new System.Windows.Forms.MenuItem();
			this.menuHelpAbout = new System.Windows.Forms.MenuItem();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.create = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.filter = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.showiFolders = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.iFolderView = new System.Windows.Forms.ListView();
			this.localiFoldersHeading = new System.Windows.Forms.RichTextBox();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// contextMenu1
			// 
			this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuOpen,
																						 this.menuCreate,
																						 this.menuRefresh,
																						 this.menuAccept,
																						 this.menuSeparator1,
																						 this.menuShare,
																						 this.menuResolve,
																						 this.menuSyncNow,
																						 this.menuRevert,
																						 this.menuRemove,
																						 this.menuSeparator2,
																						 this.menuProperties});
			this.contextMenu1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("contextMenu1.RightToLeft")));
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
			// menuCreate
			// 
			this.menuCreate.Enabled = ((bool)(resources.GetObject("menuCreate.Enabled")));
			this.menuCreate.Index = 1;
			this.menuCreate.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuCreate.Shortcut")));
			this.menuCreate.ShowShortcut = ((bool)(resources.GetObject("menuCreate.ShowShortcut")));
			this.menuCreate.Text = resources.GetString("menuCreate.Text");
			this.menuCreate.Visible = ((bool)(resources.GetObject("menuCreate.Visible")));
			this.menuCreate.Click += new System.EventHandler(this.menuCreate_Click);
			// 
			// menuRefresh
			// 
			this.menuRefresh.Enabled = ((bool)(resources.GetObject("menuRefresh.Enabled")));
			this.menuRefresh.Index = 2;
			this.menuRefresh.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuRefresh.Shortcut")));
			this.menuRefresh.ShowShortcut = ((bool)(resources.GetObject("menuRefresh.ShowShortcut")));
			this.menuRefresh.Text = resources.GetString("menuRefresh.Text");
			this.menuRefresh.Visible = ((bool)(resources.GetObject("menuRefresh.Visible")));
			this.menuRefresh.Click += new System.EventHandler(this.menuRefresh_Click);
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
			// menuShare
			// 
			this.menuShare.Enabled = ((bool)(resources.GetObject("menuShare.Enabled")));
			this.menuShare.Index = 5;
			this.menuShare.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuShare.Shortcut")));
			this.menuShare.ShowShortcut = ((bool)(resources.GetObject("menuShare.ShowShortcut")));
			this.menuShare.Text = resources.GetString("menuShare.Text");
			this.menuShare.Visible = ((bool)(resources.GetObject("menuShare.Visible")));
			this.menuShare.Click += new System.EventHandler(this.menuShare_Click);
			// 
			// menuResolve
			// 
			this.menuResolve.Enabled = ((bool)(resources.GetObject("menuResolve.Enabled")));
			this.menuResolve.Index = 6;
			this.menuResolve.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuResolve.Shortcut")));
			this.menuResolve.ShowShortcut = ((bool)(resources.GetObject("menuResolve.ShowShortcut")));
			this.menuResolve.Text = resources.GetString("menuResolve.Text");
			this.menuResolve.Visible = ((bool)(resources.GetObject("menuResolve.Visible")));
			this.menuResolve.Click += new System.EventHandler(this.menuResolve_Click);
			// 
			// menuSyncNow
			// 
			this.menuSyncNow.Enabled = ((bool)(resources.GetObject("menuSyncNow.Enabled")));
			this.menuSyncNow.Index = 7;
			this.menuSyncNow.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuSyncNow.Shortcut")));
			this.menuSyncNow.ShowShortcut = ((bool)(resources.GetObject("menuSyncNow.ShowShortcut")));
			this.menuSyncNow.Text = resources.GetString("menuSyncNow.Text");
			this.menuSyncNow.Visible = ((bool)(resources.GetObject("menuSyncNow.Visible")));
			this.menuSyncNow.Click += new System.EventHandler(this.menuSyncNow_Click);
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
			// menuRemove
			// 
			this.menuRemove.Enabled = ((bool)(resources.GetObject("menuRemove.Enabled")));
			this.menuRemove.Index = 9;
			this.menuRemove.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuRemove.Shortcut")));
			this.menuRemove.ShowShortcut = ((bool)(resources.GetObject("menuRemove.ShowShortcut")));
			this.menuRemove.Text = resources.GetString("menuRemove.Text");
			this.menuRemove.Visible = ((bool)(resources.GetObject("menuRemove.Visible")));
			this.menuRemove.Click += new System.EventHandler(this.menuRemove_Click);
			// 
			// menuSeparator2
			// 
			this.menuSeparator2.Enabled = ((bool)(resources.GetObject("menuSeparator2.Enabled")));
			this.menuSeparator2.Index = 10;
			this.menuSeparator2.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuSeparator2.Shortcut")));
			this.menuSeparator2.ShowShortcut = ((bool)(resources.GetObject("menuSeparator2.ShowShortcut")));
			this.menuSeparator2.Text = resources.GetString("menuSeparator2.Text");
			this.menuSeparator2.Visible = ((bool)(resources.GetObject("menuSeparator2.Visible")));
			// 
			// menuProperties
			// 
			this.menuProperties.Enabled = ((bool)(resources.GetObject("menuProperties.Enabled")));
			this.menuProperties.Index = 11;
			this.menuProperties.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuProperties.Shortcut")));
			this.menuProperties.ShowShortcut = ((bool)(resources.GetObject("menuProperties.ShowShortcut")));
			this.menuProperties.Text = resources.GetString("menuProperties.Text");
			this.menuProperties.Visible = ((bool)(resources.GetObject("menuProperties.Visible")));
			this.menuProperties.Click += new System.EventHandler(this.menuProperties_Click);
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuAction,
																					  this.menuEdit,
																					  this.menuView,
																					  this.menuHelp});
			this.mainMenu1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("mainMenu1.RightToLeft")));
			// 
			// menuAction
			// 
			this.menuAction.Enabled = ((bool)(resources.GetObject("menuAction.Enabled")));
			this.menuAction.Index = 0;
			this.menuAction.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					   this.menuActionCreate,
																					   this.menuActionSeparator1,
																					   this.menuActionAccept,
																					   this.menuActionRemove,
																					   this.menuActionSeparator2,
																					   this.menuActionOpen,
																					   this.menuActionShare,
																					   this.menuActionResolve,
																					   this.menuActionSync,
																					   this.menuActionRevert,
																					   this.menuActionProperties,
																					   this.menuItem4,
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
			// menuActionSeparator1
			// 
			this.menuActionSeparator1.Enabled = ((bool)(resources.GetObject("menuActionSeparator1.Enabled")));
			this.menuActionSeparator1.Index = 1;
			this.menuActionSeparator1.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionSeparator1.Shortcut")));
			this.menuActionSeparator1.ShowShortcut = ((bool)(resources.GetObject("menuActionSeparator1.ShowShortcut")));
			this.menuActionSeparator1.Text = resources.GetString("menuActionSeparator1.Text");
			this.menuActionSeparator1.Visible = ((bool)(resources.GetObject("menuActionSeparator1.Visible")));
			// 
			// menuActionAccept
			// 
			this.menuActionAccept.Enabled = ((bool)(resources.GetObject("menuActionAccept.Enabled")));
			this.menuActionAccept.Index = 2;
			this.menuActionAccept.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionAccept.Shortcut")));
			this.menuActionAccept.ShowShortcut = ((bool)(resources.GetObject("menuActionAccept.ShowShortcut")));
			this.menuActionAccept.Text = resources.GetString("menuActionAccept.Text");
			this.menuActionAccept.Visible = ((bool)(resources.GetObject("menuActionAccept.Visible")));
			this.menuActionAccept.Click += new System.EventHandler(this.menuAccept_Click);
			// 
			// menuActionRemove
			// 
			this.menuActionRemove.Enabled = ((bool)(resources.GetObject("menuActionRemove.Enabled")));
			this.menuActionRemove.Index = 3;
			this.menuActionRemove.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionRemove.Shortcut")));
			this.menuActionRemove.ShowShortcut = ((bool)(resources.GetObject("menuActionRemove.ShowShortcut")));
			this.menuActionRemove.Text = resources.GetString("menuActionRemove.Text");
			this.menuActionRemove.Visible = ((bool)(resources.GetObject("menuActionRemove.Visible")));
			this.menuActionRemove.Click += new System.EventHandler(this.menuRemove_Click);
			// 
			// menuActionSeparator2
			// 
			this.menuActionSeparator2.Enabled = ((bool)(resources.GetObject("menuActionSeparator2.Enabled")));
			this.menuActionSeparator2.Index = 4;
			this.menuActionSeparator2.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionSeparator2.Shortcut")));
			this.menuActionSeparator2.ShowShortcut = ((bool)(resources.GetObject("menuActionSeparator2.ShowShortcut")));
			this.menuActionSeparator2.Text = resources.GetString("menuActionSeparator2.Text");
			this.menuActionSeparator2.Visible = ((bool)(resources.GetObject("menuActionSeparator2.Visible")));
			// 
			// menuActionOpen
			// 
			this.menuActionOpen.Enabled = ((bool)(resources.GetObject("menuActionOpen.Enabled")));
			this.menuActionOpen.Index = 5;
			this.menuActionOpen.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionOpen.Shortcut")));
			this.menuActionOpen.ShowShortcut = ((bool)(resources.GetObject("menuActionOpen.ShowShortcut")));
			this.menuActionOpen.Text = resources.GetString("menuActionOpen.Text");
			this.menuActionOpen.Visible = ((bool)(resources.GetObject("menuActionOpen.Visible")));
			this.menuActionOpen.Click += new System.EventHandler(this.menuOpen_Click);
			// 
			// menuActionShare
			// 
			this.menuActionShare.Enabled = ((bool)(resources.GetObject("menuActionShare.Enabled")));
			this.menuActionShare.Index = 6;
			this.menuActionShare.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionShare.Shortcut")));
			this.menuActionShare.ShowShortcut = ((bool)(resources.GetObject("menuActionShare.ShowShortcut")));
			this.menuActionShare.Text = resources.GetString("menuActionShare.Text");
			this.menuActionShare.Visible = ((bool)(resources.GetObject("menuActionShare.Visible")));
			this.menuActionShare.Click += new System.EventHandler(this.menuShare_Click);
			// 
			// menuActionResolve
			// 
			this.menuActionResolve.Enabled = ((bool)(resources.GetObject("menuActionResolve.Enabled")));
			this.menuActionResolve.Index = 7;
			this.menuActionResolve.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionResolve.Shortcut")));
			this.menuActionResolve.ShowShortcut = ((bool)(resources.GetObject("menuActionResolve.ShowShortcut")));
			this.menuActionResolve.Text = resources.GetString("menuActionResolve.Text");
			this.menuActionResolve.Visible = ((bool)(resources.GetObject("menuActionResolve.Visible")));
			this.menuActionResolve.Click += new System.EventHandler(this.menuResolve_Click);
			// 
			// menuActionSync
			// 
			this.menuActionSync.Enabled = ((bool)(resources.GetObject("menuActionSync.Enabled")));
			this.menuActionSync.Index = 8;
			this.menuActionSync.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionSync.Shortcut")));
			this.menuActionSync.ShowShortcut = ((bool)(resources.GetObject("menuActionSync.ShowShortcut")));
			this.menuActionSync.Text = resources.GetString("menuActionSync.Text");
			this.menuActionSync.Visible = ((bool)(resources.GetObject("menuActionSync.Visible")));
			this.menuActionSync.Click += new System.EventHandler(this.menuSyncNow_Click);
			// 
			// menuActionRevert
			// 
			this.menuActionRevert.Enabled = ((bool)(resources.GetObject("menuActionRevert.Enabled")));
			this.menuActionRevert.Index = 9;
			this.menuActionRevert.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionRevert.Shortcut")));
			this.menuActionRevert.ShowShortcut = ((bool)(resources.GetObject("menuActionRevert.ShowShortcut")));
			this.menuActionRevert.Text = resources.GetString("menuActionRevert.Text");
			this.menuActionRevert.Visible = ((bool)(resources.GetObject("menuActionRevert.Visible")));
			this.menuActionRevert.Click += new System.EventHandler(this.menuRevert_Click);
			// 
			// menuActionProperties
			// 
			this.menuActionProperties.Enabled = ((bool)(resources.GetObject("menuActionProperties.Enabled")));
			this.menuActionProperties.Index = 10;
			this.menuActionProperties.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuActionProperties.Shortcut")));
			this.menuActionProperties.ShowShortcut = ((bool)(resources.GetObject("menuActionProperties.ShowShortcut")));
			this.menuActionProperties.Text = resources.GetString("menuActionProperties.Text");
			this.menuActionProperties.Visible = ((bool)(resources.GetObject("menuActionProperties.Visible")));
			this.menuActionProperties.Click += new System.EventHandler(this.menuProperties_Click);
			// 
			// menuItem4
			// 
			this.menuItem4.Enabled = ((bool)(resources.GetObject("menuItem4.Enabled")));
			this.menuItem4.Index = 11;
			this.menuItem4.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuItem4.Shortcut")));
			this.menuItem4.ShowShortcut = ((bool)(resources.GetObject("menuItem4.ShowShortcut")));
			this.menuItem4.Text = resources.GetString("menuItem4.Text");
			this.menuItem4.Visible = ((bool)(resources.GetObject("menuItem4.Visible")));
			// 
			// menuExit
			// 
			this.menuExit.Enabled = ((bool)(resources.GetObject("menuExit.Enabled")));
			this.menuExit.Index = 12;
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
																					 this.menuEditPrefs});
			this.menuEdit.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuEdit.Shortcut")));
			this.menuEdit.ShowShortcut = ((bool)(resources.GetObject("menuEdit.ShowShortcut")));
			this.menuEdit.Text = resources.GetString("menuEdit.Text");
			this.menuEdit.Visible = ((bool)(resources.GetObject("menuEdit.Visible")));
			// 
			// menuEditPrefs
			// 
			this.menuEditPrefs.Enabled = ((bool)(resources.GetObject("menuEditPrefs.Enabled")));
			this.menuEditPrefs.Index = 0;
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
																					 this.menuViewAccounts,
																					 this.menuViewLog});
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
			// menuViewAccounts
			// 
			this.menuViewAccounts.Enabled = ((bool)(resources.GetObject("menuViewAccounts.Enabled")));
			this.menuViewAccounts.Index = 2;
			this.menuViewAccounts.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuViewAccounts.Shortcut")));
			this.menuViewAccounts.ShowShortcut = ((bool)(resources.GetObject("menuViewAccounts.ShowShortcut")));
			this.menuViewAccounts.Text = resources.GetString("menuViewAccounts.Text");
			this.menuViewAccounts.Visible = ((bool)(resources.GetObject("menuViewAccounts.Visible")));
			this.menuViewAccounts.Click += new System.EventHandler(this.menuViewAccounts_Click);
			// 
			// menuViewLog
			// 
			this.menuViewLog.Enabled = ((bool)(resources.GetObject("menuViewLog.Enabled")));
			this.menuViewLog.Index = 3;
			this.menuViewLog.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("menuViewLog.Shortcut")));
			this.menuViewLog.ShowShortcut = ((bool)(resources.GetObject("menuViewLog.ShowShortcut")));
			this.menuViewLog.Text = resources.GetString("menuViewLog.Text");
			this.menuViewLog.Visible = ((bool)(resources.GetObject("menuViewLog.Visible")));
			this.menuViewLog.Click += new System.EventHandler(this.menuViewLog_Click);
			// 
			// menuHelp
			// 
			this.menuHelp.Enabled = ((bool)(resources.GetObject("menuHelp.Enabled")));
			this.menuHelp.Index = 3;
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
			// 
			// iFolderView
			// 
			this.iFolderView.AccessibleDescription = resources.GetString("iFolderView.AccessibleDescription");
			this.iFolderView.AccessibleName = resources.GetString("iFolderView.AccessibleName");
			this.iFolderView.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("iFolderView.Alignment")));
			this.iFolderView.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("iFolderView.Anchor")));
			this.iFolderView.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("iFolderView.BackgroundImage")));
			this.iFolderView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.iFolderView.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("iFolderView.Dock")));
			this.iFolderView.Enabled = ((bool)(resources.GetObject("iFolderView.Enabled")));
			this.iFolderView.Font = ((System.Drawing.Font)(resources.GetObject("iFolderView.Font")));
			this.iFolderView.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("iFolderView.ImeMode")));
			this.iFolderView.LabelWrap = ((bool)(resources.GetObject("iFolderView.LabelWrap")));
			this.iFolderView.Location = ((System.Drawing.Point)(resources.GetObject("iFolderView.Location")));
			this.iFolderView.MultiSelect = false;
			this.iFolderView.Name = "iFolderView";
			this.iFolderView.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("iFolderView.RightToLeft")));
			this.iFolderView.Size = ((System.Drawing.Size)(resources.GetObject("iFolderView.Size")));
			this.iFolderView.TabIndex = ((int)(resources.GetObject("iFolderView.TabIndex")));
			this.iFolderView.Text = resources.GetString("iFolderView.Text");
			this.iFolderView.Visible = ((bool)(resources.GetObject("iFolderView.Visible")));
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
			this.localiFoldersHeading.ForeColor = System.Drawing.SystemColors.InactiveCaption;
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
			this.Load += new System.EventHandler(this.GlobalProperties_Load);
			this.Move += new System.EventHandler(this.GlobalProperties_Move);
			this.VisibleChanged += new System.EventHandler(this.GlobalProperties_VisibleChanged);
			this.panel1.ResumeLayout(false);
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
		/// <summary>
		/// Adds the specified domain to the dropdown list.
		/// </summary>
		/// <param name="domainInfo">The DomainInformation object to add to the list.</param>
		public void AddDomainToList(DomainInformation domainInfo)
		{
			Domain domain = null;

			iFoldersListView ifListView = (iFoldersListView)iFolderListViews[ domainInfo.ID ];
			if ( ifListView == null )
			{
				// Add the domain.
				domain = new Domain( domainInfo );
				ifListView = new iFoldersListView( domainInfo, largeImageList );
				ifListView.ItemSelected += new Novell.FormsTrayApp.iFoldersListView.ItemSelectedDelegate(ifListView_ItemSelected);
				iFolderListViews.Add( domainInfo.ID, ifListView );

				if ( !hide )
				{
					Point point = new Point( 8, panel2.Controls[ panel2.Controls.Count - 1 ].Bottom );
					ifListView.Location = point;
					panel2.Controls.Add( ifListView );
				}
			}
			else
			{
				// TODO: Need to rework this to use domainInfo and not Domain
				domain = new Domain( ifListView.DomainInfo );
			}

/*			foreach (Domain d in servers.Items)
			{
				if (d.ID.Equals(domainInfo.ID))
				{
					// The domain is already in the list.
					domain = d;
				}
			}

			if (domain == null)
			{
				// The domain isn't in the list ... add it.
				domain = new Domain(domainInfo);
				servers.Items.Add(domain);
			}
*/
			// Reset the current default domain if the added domain is set to be the default.
			if (domainInfo.IsDefault)
			{
				if ((defaultDomain != null) && !defaultDomain.ID.Equals(domainInfo.ID))
				{
					defaultDomain.DomainInfo.IsDefault = false;
				}

				// Keep track of the default domain.
				defaultDomain = domain;
			}

			// Update the domain list file.
			addDomainToFile(domainInfo);
		}

		/// <summary>
		/// Remove the specified domain from the dropdown list.
		/// </summary>
		/// <param name="domainInfo">The DomainInformation object representing the domain to remove.</param>
		public void RemoveDomainFromList(DomainInformation domainInfo, string defaultDomainID)
		{
			Domain domain = null;
			Domain showAllDomain = null;
			
/*			foreach (Domain d in servers.Items)
			{
				if (d.ID.Equals(domainInfo.ID))
				{
					domain = d;
				}

				if (d.ShowAll)
				{
					showAllDomain = d;
				}

				// Reset the default domain.
				if ((defaultDomainID != null) && d.ID.Equals(defaultDomainID))
				{
					d.DomainInfo.IsDefault = true;                    
				}
			}

			if (domain != null)
			{
				if (servers.SelectedItem.Equals(domain))
				{
					// If this was the selected domain, select the "show all" domain.
					servers.SelectedItem = showAllDomain;
				}
				else if (((Domain)servers.SelectedItem).ShowAll)
				{
					// If the wildcard domain is selected, refresh the list.
					refreshiFolders((Domain)servers.SelectedItem);
				}

				servers.Items.Remove(domain);
			}
*/
			// Update the domain list file.
			removeDomainFromFile(domainInfo, defaultDomainID);
		}

		/// <summary>
		/// Remove the specified domain from the dropdown list.
		/// </summary>
		/// <param name="domainID">The ID of the domain to remove.</param>
		public void RemoveDomainFromList(string domainID)
		{
			Domain domain = null;
			Domain showAllDomain = null;
			
/*			foreach (Domain d in servers.Items)
			{
				if (d.ID.Equals(domainID))
				{
					domain = d;
				}

				if (d.ShowAll)
				{
					showAllDomain = d;
				}
			}

			if (domain != null)
			{
				if (servers.SelectedItem.Equals(domain))
				{
					// If this was the selected domain, select the "show all" domain.
					servers.SelectedItem = showAllDomain;
				}
				else if (((Domain)servers.SelectedItem).ShowAll)
				{
					// If the wildcard domain is selected, refresh the list.
					refreshiFolders((Domain)servers.SelectedItem);
				}

				servers.Items.Remove(domain);

				// Update the domain list file.
				string defaultDomainID = simiasWebService.GetDefaultDomainID();
				removeDomainFromFile(domain.DomainInfo, defaultDomainID);

				if (RemoveDomain != null)
				{
					RemoveDomain(this, new DomainRemoveEventArgs(domain.DomainInfo, defaultDomainID));
				}
			}*/
		}

		/// <summary>
		/// Intializes the list of servers displayed in the dropdown list.
		/// </summary>
		public void InitializeServerList()
		{
/*			servers.Items.Clear();

			// Add the wild-card domain.
			Domain domain = new Domain(resourceManager.GetString("showAll"));
			servers.Items.Add(domain);
			servers.SelectedItem = domain;
*/
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
		private Object deepClone(Object original) 
		{
			MemoryStream stream = new MemoryStream();

			System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = 
				new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

			formatter.Context = new System.Runtime.Serialization.StreamingContext(
				System.Runtime.Serialization.StreamingContextStates.Clone);

			// Serialize the object graph into the stream
			formatter.Serialize(stream, original);

			// Seek to the start of the stream before deserializing
			stream.Position = 0;

			// Deserialize into a new set of objects
			// and return the root of the graph (deep copy) to the caller
			return(formatter.Deserialize(stream));
		}

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
								ListViewItem lvi = (ListViewItem)ht[syncEventArgs.ID];
								if (lvi != null)
								{
									((iFolderObject)lvi.Tag).iFolderState = iFolderState.SynchronizingLocal;
									lvi.SubItems[2].Text = resourceManager.GetString("preSync");
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
								ListViewItem lvi = (ListViewItem)ht[syncEventArgs.ID];
								if (lvi != null)
								{
									startSync = true;
									((iFolderObject)lvi.Tag).iFolderState = iFolderState.Synchronizing;
									lvi.SubItems[2].Text = resourceManager.GetString("statusSyncing");
								}
							}
						}
						break;
					}
					case Action.StopSync:
					{
						lock(ht)
						{
							ListViewItem lvi = (ListViewItem)ht[syncEventArgs.ID];

							if (lvi != null)
							{
								try
								{
									SyncSize syncSize = ifWebService.CalculateSyncSize(syncEventArgs.ID);
									objectsToSync = syncSize.SyncNodeCount;
								}
								catch {}

								if (objectsToSync == 0)
								{
									iFolderObject ifolderObject = (iFolderObject)lvi.Tag;

									if (syncEventArgs.Connected)
									{
										ifolderObject.iFolderState = iFolderState.Normal;
									}
									else
									{
										ifolderObject.iFolderState = iFolderState.Disconnected;
									}

									int imageIndex;
									lvi.SubItems[2].Text = stateToString(ifolderObject, out imageIndex);
								}
								else
								{
									lvi.SubItems[2].Text = string.Format(resourceManager.GetString("statusSyncItemsFailed"), objectsToSync);
									((iFolderObject)lvi.Tag).iFolderState = iFolderState.FailedSync;
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
							ListViewItem lvi = (ListViewItem)ht[syncEventArgs.CollectionID];
							if (lvi != null)
							{
								lvi.SubItems[2].Text = string.Format(resourceManager.GetString("statusSyncingItems"), objectsToSync--);
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
						ListViewItem lvi = (ListViewItem)ht[args.Node];
						if (lvi != null)
						{
							// Notify the shell.
							Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, ((iFolderObject)lvi.Tag).iFolderWeb.UnManagedPath, IntPtr.Zero);

							lvi.Remove();
							ht.Remove(args.Node);
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
//					if (IsSelected(ifolder.DomainID))
					{
						addiFolderToListView(new iFolderObject(ifolder, iFolderState.Normal));

//						if (ifolder.State.Equals("Local"))
//						{
//							// Notify the shell.
//							Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, ifolder.UnManagedPath, IntPtr.Zero);
//						}

						// Check for existing subscriptions when an iFolder gets created and remove them
						// from the list.
/*TODO:						if (!ifolder.IsSubscription)
						{
							// See if there is a subscription for this ifolder.
							lock (ht)
							{
								ListViewItem[] lvia = new ListViewItem[ht.Count];
								ht.Values.CopyTo(lvia, 0);

								foreach(ListViewItem lvi in lvia)
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
						}*/
					}
				}
				else
				{
/*TODO:					ListViewItem lvi;
					lock (ht)
					{
						// Get the corresponding listview item.
						lvi = (ListViewItem)ht[ifolder.ID];
					}

					if (lvi != null)
					{
						// Update the tag data.
						((iFolderObject)lvi.Tag).iFolderWeb = ifolder;
						updateListViewItem(lvi);
					}*/
				}
			}
		}

		private void addiFolderToListView(iFolderObject ifolderObject)
		{
			iFolderWeb ifolder = ifolderObject.iFolderWeb;

			if (ifolder.State.Equals("Local"))
			{
				lock (ht)
				{
					// Add only if it isn't already in the list.
					if (ht[ifolder.ID] == null)
					{
						/*					string[] items = new string[3];
											int imageIndex;

											items[0] = ifolder.Name;
											items[1] = ifolder.IsSubscription ? ifolder.Owner : ifolder.UnManagedPath;
											items[2] = stateToString(ifolderObject, out imageIndex);
						*/
						// TODO: use proper image index.
						ListViewItem lvi = new ListViewItem(ifolder.Name, 0);
						lvi.Tag = ifolderObject;
						iFolderView.Items.Add(lvi);

						// Add the listviewitem to the hashtable.
						ht.Add(ifolder.ID, lvi);
					}
				}

				// Notify the shell.
				Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, ifolder.UnManagedPath, IntPtr.Zero);
			}
			else
			{
				lock ( iFolderListViews )
				{
					iFoldersListView ifListView = (iFoldersListView)iFolderListViews[ ifolder.DomainID ];
					// TODO: what if the listview isn't in the list?

					ifListView.AddiFolderToListView( new iFolderObject( ifolder, iFolderState.Normal ) );
				}
			}
		}

		private void updateView()
		{
			int nextY;
			if ( iFolderView.Items.Count == 0 )
			{
				iFolderView.Visible = false;

				// Show the informational message.
				infoMessage.Location = iFolderView.Location;
				panel2.Controls.Add( infoMessage );
				nextY = infoMessage.Top + infoMessage.Height;
			}
			else
			{
				iFolderView.Visible = true;
				nextY = iFolderView.Top + iFolderView.Height;

				// Hide the informational message.
				panel2.Controls.Remove( infoMessage );
			}

			// Adjust positions of ifolder views
			if ( !hide )
			{
				//TODO: Adjust for no iFolders - noiFolderMessage1.Top + noiFolderMessage1.Height;
				foreach ( iFoldersListView ifListView in iFolderListViews.Values )
				{
					Point point = new Point( 8, nextY );
					ifListView.Location = point;
					nextY = nextY + ifListView.Height;
				}
			}
		}

		private void updateListViewItem(ListViewItem lvi)
		{
			iFolderObject ifolderObject = (iFolderObject)lvi.Tag;
			iFolderWeb ifolder = ifolderObject.iFolderWeb;

			if (ifolder.State.Equals("Available") &&
				(ifWebService.GetiFolder(ifolder.CollectionID) != null))
			{
				// The iFolder already exists locally ... remove it from the list.
				lock (ht)
				{
					ht.Remove(ifolder.ID);
				}

				lvi.Remove();
			}
			else
			{
				int imageIndex;
				lvi.SubItems[0].Text = ifolder.Name;
//				lvi.SubItems[1].Text = ifolder.IsSubscription ? "" : ifolder.UnManagedPath;
//				lvi.SubItems[2].Text = stateToString(ifolderObject, out imageIndex);
//				lvi.ImageIndex = imageIndex;

				// If this item is the only one selected, update the menus.
/*				if (lvi.Selected && (iFolderView.SelectedItems.Count == 1))
				{
					updateMenus(ifolder);
				}
*/			}
		}

		private string stateToString(iFolderObject ifolderObject, out int imageIndex)
		{
			string status;
			imageIndex = 0;

			if (ifolderObject.iFolderWeb.HasConflicts)
			{
				imageIndex = 2;
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
								status = resourceManager.GetString("statusOK");
								break;
							case "Available":
							case "WaitConnect":
							case "WaitSync":
								imageIndex = ifolderObject.iFolderWeb.IsSubscription ? 1 : 0;
								status = resourceManager.GetString(ifolderObject.iFolderWeb.State);
								break;
							default:
								// TODO: what icon to use for unknown status?
								imageIndex = 1;
								status = resourceManager.GetString("statusUnknown");
								break;
						}
						break;
					}
					case iFolderState.Disconnected:
						status = resourceManager.GetString("disconnected");
						break;
					case iFolderState.FailedSync:
						status = objectsToSync == 0 ?
							status = resourceManager.GetString("statusSyncFailure") :
							string.Format(resourceManager.GetString("statusSyncItemsFailed"), objectsToSync);
						break;
					case iFolderState.Synchronizing:
						status = string.Format(resourceManager.GetString("statusSyncingItems"), objectsToSync);
						break;
					case iFolderState.SynchronizingLocal:
						status = resourceManager.GetString("preSync");
						break;
					default:
						status = resourceManager.GetString("statusUnknown");
						break;
				}
			}

			return status;
		}

		private void refreshAll(Domain domain)
		{
			refreshiFolders(/*domain*/);

			// Call to sync the POBoxes.
			if (domain.ShowAll)
			{
/*				foreach (Domain d in servers.Items)
				{
					if (!d.ShowAll)
					{
						try
						{
							ifWebService.SynciFolderNow(d.DomainInfo.POBoxID);
						}
						catch {}
					}
				}*/
			}
			else
			{
				try
				{
					ifWebService.SynciFolderNow(domain.DomainInfo.POBoxID);
				}
				catch {}
			}
		}

		private void refreshiFolders(/*Domain domain*/)
		{
			Cursor.Current = Cursors.WaitCursor;

			iFolderView.Items.Clear();
			iFolderView.SelectedItems.Clear();

			foreach (iFoldersListView ifListView in iFolderListViews.Values)
			{
				ifListView.InitializeUpdate();
			}

			// Disable/hide the menu items and toolbar buttons.
/*			menuShare.Visible = menuActionShare.Enabled = toolBarShare.Enabled =
				menuProperties.Visible = menuActionProperties.Enabled = 
				menuRevert.Visible = menuActionRevert.Enabled = 
				menuSyncNow.Visible = menuActionSync.Enabled = toolBarSync.Enabled =
				menuOpen.Visible = menuActionOpen.Enabled = 
				menuSeparator1.Visible = menuSeparator2.Visible =
				menuResolve.Visible = menuActionResolve.Visible = toolBarResolve.Enabled =
				menuAccept.Visible = menuActionAccept.Visible = toolBarSetup.Enabled =
				menuActionSeparator2.Visible =
				menuRemove.Visible = menuActionRemove.Visible = 
				menuActionSeparator2.Visible = false;
*/
			// Show the refresh and create menu items.
			menuRefresh.Visible = menuCreate.Visible = true;

			Hashtable oldHt;
			lock(ht)
			{
				// Save the old items so that the state can be preserved between refreshes.
				oldHt = (Hashtable)deepClone( ht );

				ht.Clear();
			}

			iFolderView.BeginUpdate();

			try
			{
				iFolderWeb[] ifolderArray = ifWebService.GetAlliFolders();
				foreach (iFolderWeb ifolder in ifolderArray)
				{
					iFolderState state = iFolderState.Normal;
					if (!ifolder.IsSubscription)
					{
						ListViewItem lvi = (ListViewItem)oldHt[ ifolder.ID ];
						if ( lvi != null )
						{
							iFolderObject oldiFolder = (iFolderObject)lvi.Tag;
							state = oldiFolder.iFolderState;
						}
					}

					addiFolderToListView(new iFolderObject(ifolder, state));
				}
			}
			catch (Exception ex)
			{
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("iFolderError"), resourceManager.GetString("iFolderErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
				mmb.ShowDialog();
			}

			iFolderView.EndUpdate();

			foreach (iFoldersListView ifListView in iFolderListViews.Values)
			{
				ifListView.FinalizeUpdate();
			}

			updateView();

			Cursor.Current = Cursors.Default;
		}

		private void invokeiFolderProperties(ListViewItem lvi, int activeTab)
		{
			iFolderAdvanced ifolderAdvanced = new iFolderAdvanced();
			ifolderAdvanced.CurrentiFolder = ((iFolderObject)lvi.Tag).iFolderWeb;
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
			}
		}

		private void updateMenus(iFolderWeb ifolderWeb)
		{
			if (ifolderWeb == null)
			{
/*				menuShare.Visible = menuActionShare.Enabled = toolBarShare.Enabled =
					menuProperties.Visible = menuActionProperties.Enabled = 
					menuRevert.Visible = menuActionRevert.Enabled = 
					menuOpen.Visible = menuActionOpen.Enabled = 
					menuSyncNow.Visible = menuActionSync.Enabled = toolBarSync.Enabled = 
					menuSeparator1.Visible = menuSeparator2.Visible = 				
					menuResolve.Visible = menuActionResolve.Visible = toolBarResolve.Enabled =
					menuAccept.Visible = menuActionAccept.Visible = toolBarSetup.Enabled =
					menuActionSeparator2.Visible =
					menuRemove.Visible = menuActionRemove.Visible = 
					menuActionSeparator2.Visible = false;
*/
			}
			else
			{
/*				menuShare.Visible = menuActionShare.Enabled = toolBarShare.Enabled =
					menuProperties.Visible = menuActionProperties.Enabled = 
					menuOpen.Visible = menuActionOpen.Enabled = 
					menuSeparator1.Visible = menuSeparator2.Visible = 				
					!ifolderWeb.IsSubscription;*/
				menuRevert.Visible = menuActionRevert.Enabled = 
					!ifolderWeb.IsSubscription && !ifolderWeb.Role.Equals("Master");
				if (ifolderWeb.IsSubscription)
				{
//					menuSyncNow.Visible = menuActionSync.Enabled = toolBarSync.Enabled = false;
				}
				else
				{
/*					Domain selectedDomain = (Domain)servers.SelectedItem;
					if (!selectedDomain.ShowAll)
					{
						menuSyncNow.Visible = menuActionSync.Enabled = toolBarSync.Enabled = selectedDomain.DomainInfo.Active;
					}
					else
					{
						foreach (Domain d in servers.Items)
						{
							if (d.ID.Equals(ifolderWeb.DomainID))
							{
								menuSyncNow.Visible = menuActionSync.Enabled = toolBarSync.Enabled = d.DomainInfo.Active;
								break;
							}
						}
					}*/
				}

//				menuResolve.Visible = menuActionResolve.Visible = toolBarResolve.Enabled = ifolderWeb.HasConflicts;

				// Display the accept menu item if the selected item is a subscription with state "Available"
//				menuAccept.Visible = menuActionAccept.Visible = toolBarSetup.Enabled =
//					menuActionSeparator2.Visible = ifolderWeb.IsSubscription &&	ifolderWeb.State.Equals("Available");

				// Display the decline menu item if the selected item is a subscription with state "Available" and from someone else.
				menuRemove.Visible = menuActionRemove.Visible = 
					menuActionSeparator2.Visible = (!ifolderWeb.IsSubscription || ifolderWeb.State.Equals("Available"));

				if (menuRemove.Visible)
				{
					if (IsCurrentUser(ifolderWeb.OwnerID))
					{
						menuRemove.Text = menuActionRemove.Text = 
							resourceManager.GetString("deleteAction");
					}
					else
					{
						menuRemove.Text = resourceManager.GetString("menuRemove.Text");
						menuActionRemove.Text = resourceManager.GetString("menuActionRemove.Text");
					}
				}
			}

//			menuRefresh.Visible = menuCreate.Visible = iFolderView.SelectedItems.Count == 0;
		}

		private void updateEnterpriseData()
		{
//			servers.Items.Clear();
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
		#endregion

		#region Event Handlers
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

			refreshiFolders();

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
		}

		private void ifListView_ItemSelected(object sender, ItemSelectedEventArgs e)
		{
			if ( selectedItem != null && !selectedItem.Equals( e.Item ) )
				selectedItem.Selected = false;

			selectedItem = e.Item;
		}

		private void updateEnterpriseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			updateEnterpriseTimer.Stop();
			updateEnterpriseData();
		}

		private void menuFileExit_Click(object sender, System.EventArgs e)
		{
			this.Close();
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
//				refreshAll((Domain)servers.SelectedItem);
			}
		}

/*		private void servers_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			refreshiFolders((Domain)servers.SelectedItem);
		}*/

		private void menuOpen_Click(object sender, System.EventArgs e)
		{
/*			ListViewItem lvi = iFolderView.SelectedItems[0];
			iFolderWeb ifolder = ((iFolderObject)lvi.Tag).iFolderWeb;

			try
			{
				Process.Start(ifolder.UnManagedPath);
			}
			catch (Exception ex)
			{
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(string.Format(resourceManager.GetString("iFolderOpenError"), ifolder.Name), resourceManager.GetString("openErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}*/
		}

		private void menuRevert_Click(object sender, System.EventArgs e)
		{
/*			ListViewItem lvi = iFolderView.SelectedItems[0];

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				iFolderWeb ifolder = ((iFolderObject)lvi.Tag).iFolderWeb;

				MyMessageBox mmb = new Novell.iFolderCom.MyMessageBox(
					resourceManager.GetString("revertiFolder") + "\n\n" +
					resourceManager.GetString("revertPrompt"), 
					resourceManager.GetString("revertTitle"),
					string.Empty,
					MyMessageBoxButtons.YesNo,
					MyMessageBoxIcon.Question,
					MyMessageBoxDefaultButton.Button2);
				if (mmb.ShowDialog() == DialogResult.Yes)
				{
					// Delete the iFolder.
					iFolderWeb newiFolder = ifWebService.RevertiFolder(ifolder.ID);

					// Notify the shell.
					Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, ifolder.UnManagedPath, IntPtr.Zero);

					if (newiFolder != null)
					{
						lvi.Tag = new iFolderObject(newiFolder, iFolderState.Normal);

						lock (ht)
						{
							ht.Add(newiFolder.ID, lvi);
						}

						updateListViewItem(lvi);
					}
					else
					{
						lvi.Remove();
					}

					lock (ht)
					{
						ht.Remove(ifolder.ID);
					}
				}
			}
			catch (Exception ex)
			{		
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("iFolderRevertError"), resourceManager.GetString("revertErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}

			Cursor.Current = Cursors.Default;*/
		}

		private void menuResolve_Click(object sender, System.EventArgs e)
		{
/*			ConflictResolver conflictResolver = new ConflictResolver();
			conflictResolver.iFolder = ((iFolderObject)iFolderView.SelectedItems[0].Tag).iFolderWeb;
			conflictResolver.iFolderWebService = ifWebService;
			conflictResolver.LoadPath = Application.StartupPath;
			conflictResolver.Show();
*/		}

		private void menuShare_Click(object sender, System.EventArgs e)
		{
//			invokeiFolderProperties(iFolderView.SelectedItems[0], 1);
		}

		private void menuSyncNow_Click(object sender, System.EventArgs e)
		{
//			synciFolder(((iFolderObject)iFolderView.SelectedItems[0].Tag).iFolderWeb.ID);
		}

		private void menuProperties_Click(object sender, System.EventArgs e)
		{
//			invokeiFolderProperties(iFolderView.SelectedItems[0], 0);
		}

		private void menuCreate_Click(object sender, System.EventArgs e)
		{
			// Build the list of domains to pass in to the create dialog.
			ArrayList domains = new ArrayList();
			DomainItem selectedDomainItem = new DomainItem( defaultDomain.Name, defaultDomain.ID );
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

			if ((DialogResult.OK == createiFolder.ShowDialog()) && iFolderComponent.DisplayConfirmationEnabled)
			{
				new iFolderComponent().NewiFolderWizard(Application.StartupPath, createiFolder.iFolderPath);
			}
		}

		private void menuRefresh_Click(object sender, System.EventArgs e)
		{
//			refreshAll((Domain)servers.SelectedItem);
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
/*			ListViewItem lvi = iFolderView.SelectedItems[0];
			iFolderWeb ifolder = ((iFolderObject)lvi.Tag).iFolderWeb;

			AcceptInvitation acceptInvitation = new AcceptInvitation(ifWebService, ifolder);
			// TODO: get iFolder from acceptInvitation and update the listviewitem with it.
			acceptInvitation.ShowDialog();
*/		}

		private void menuRemove_Click(object sender, System.EventArgs e)
		{
/*			ListViewItem lvi = iFolderView.SelectedItems[0];
			iFolderWeb ifolder = ((iFolderObject)lvi.Tag).iFolderWeb;
			try
			{
				string message;
				string caption = resourceManager.GetString("removeTitle");

				if (ifolder.IsSubscription)
				{
					message = resourceManager.GetString("removeiFolder2") + "\n\n" +
						resourceManager.GetString("removePrompt");

					MyMessageBox mmb = new Novell.iFolderCom.MyMessageBox(
						message,
						caption,
						string.Empty,
						MyMessageBoxButtons.YesNo,
						MyMessageBoxIcon.Question,
						MyMessageBoxDefaultButton.Button2);
					if (mmb.ShowDialog() == DialogResult.Yes)
					{
						ifWebService.DeclineiFolderInvitation(ifolder.DomainID, ifolder.ID);
					}
				}
				else
				{
					if (IsCurrentUser(ifolder.OwnerID))
					{
						message = resourceManager.GetString("deleteiFolder") + "\n\n" + 
							resourceManager.GetString("removePrompt");
					}
					else
					{
						message = resourceManager.GetString("removeiFolder")  + "\n\n" + 
							resourceManager.GetString("removePrompt");
					}

					MyMessageBox mmb = new Novell.iFolderCom.MyMessageBox(
						message,
						caption,
						string.Empty,
						MyMessageBoxButtons.YesNo,
						MyMessageBoxIcon.Question,
						MyMessageBoxDefaultButton.Button2);
					if (mmb.ShowDialog() == DialogResult.Yes)
					{
						if (ifolder.Role.Equals("Master"))
						{
							ifWebService.DeleteiFolder(ifolder.ID);
							lvi.Remove();
						}
						else
						{
							// Revert the iFolder.
							iFolderWeb newiFolder = ifWebService.RevertiFolder(ifolder.ID);

							// Notify the shell.
							Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, ifolder.UnManagedPath, IntPtr.Zero);

							if (newiFolder != null)
							{
								// Update the listview item.
								lvi.Tag = new iFolderObject(newiFolder, iFolderState.Normal);

								lock (ht)
								{
									ht.Add(newiFolder.ID, lvi);
								}

								updateListViewItem(lvi);

								// Decline the invitation.
								ifWebService.DeclineiFolderInvitation(newiFolder.DomainID, newiFolder.ID);
							}
							else
							{
								lvi.Remove();
							}
						}

						lock (ht)
						{
							ht.Remove(ifolder.ID);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("declineError"), resourceManager.GetString("errorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}
*/		}

		private void iFolderView_SelectedIndexChanged(object sender, System.EventArgs e)
		{
/*			iFolderWeb ifolderWeb = null;

			if (iFolderView.SelectedItems.Count == 1)
			{
				ifolderWeb = ((iFolderObject)iFolderView.SelectedItems[0].Tag).iFolderWeb;
			}

			updateMenus(ifolderWeb);
*/		}

		private void iFolderView_DoubleClick(object sender, System.EventArgs e)
		{
/*			if (iFolderView.SelectedItems.Count == 1)
			{
				ListViewItem lvi = iFolderView.SelectedItems[0];
				iFolderWeb ifolder = ((iFolderObject)lvi.Tag).iFolderWeb;
				if (ifolder.IsSubscription)
				{
					if (ifolder.State.Equals("Available"))
					{
						menuAccept_Click(sender, e);
					}
				}
				else
				{
					menuOpen_Click(sender, e);
				}
			}
*/		}

		private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
/*			switch (toolBar1.Buttons.IndexOf(e.Button))
			{
				case 0: // Create
					menuCreate_Click(this, new EventArgs());
					break;
				case 1: // Setup
					menuAccept_Click(this, new EventArgs());
					break;
				case 2: // Share
					invokeiFolderProperties(iFolderView.SelectedItems[0], 1);
					break;
				case 3: // Resolve
					menuResolve_Click(this, new EventArgs());
					break;
				case 4: // Sync
					synciFolder(((iFolderObject)iFolderView.SelectedItems[0].Tag).iFolderWeb.ID);
					break;
//				case 1: // Open
//					menuOpen_Click(this, new EventArgs());
//					break;
//				case 3: // Properties
//					invokeiFolderProperties(iFolderView.SelectedItems[0], 0);
//					break;
//				case 6: // Revert
//					menuRevert_Click(this, new EventArgs());
//					break;
//				case 7: // Remove
//					menuRemove_Click(this, new EventArgs());
//					break;
//				case 12: // Refresh
//					refreshiFolders((Domain)servers.SelectedItem);
//					break;
			}*/
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

		private void showiFolders_Click(object sender, System.EventArgs e)
		{
			hide = !hide;

			if ( hide )
			{
				// TODO: Localize
				showiFolders.Text = "View available iFolders";

				// Remove the server listview controls.
				foreach ( iFoldersListView ifListView in iFolderListViews.Values )
				{
					panel2.Controls.Remove( ifListView );
				}
			}
			else
			{
				// TODO: Localize
				showiFolders.Text = "Hide available iFolders";

				// Add the server listview controls.
				foreach ( iFoldersListView ifListView in iFolderListViews.Values )
				{
					panel2.Controls.Add( ifListView );
				}
			}

			updateView();
		}
	}
}
