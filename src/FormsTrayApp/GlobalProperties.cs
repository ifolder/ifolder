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
		// Delegates used to marshal back to the control's creation thread.
		private delegate void SyncCollectionDelegate(CollectionSyncEventArgs syncEventArgs);
		private SyncCollectionDelegate syncCollectionDelegate;
		private delegate void SyncFileDelegate(FileSyncEventArgs syncEventArgs);
		private SyncFileDelegate syncFileDelegate;

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
		public delegate void DeleteEventDelegate(string ID);
		/// <summary>
		/// Delegate used to service node delete events.
		/// </summary>
		public DeleteEventDelegate deleteEventDelegate;

		System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(GlobalProperties));
		private System.Timers.Timer updateEnterpriseTimer;
		private short retryCount = 2;
		private Hashtable ht;
		private iFolderWebService ifWebService;
		private IProcEventClient eventClient;
		private bool initialConnect = false;
		private bool shutdown = false;
		private Domain defaultDomain = null;
		private System.Windows.Forms.ListView iFolderView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem menuOpen;
		private System.Windows.Forms.MenuItem menuCreate;
		private System.Windows.Forms.MenuItem menuShare;
		private System.Windows.Forms.MenuItem menuRevert;
		private System.Windows.Forms.MenuItem menuProperties;
		private System.Windows.Forms.MenuItem menuRefresh;
		private System.Windows.Forms.MenuItem menuSeparator1;
		private System.Windows.Forms.MenuItem menuSeparator2;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
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
		private System.Windows.Forms.Label status;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.MenuItem menuRemove;
		private System.Windows.Forms.MenuItem menuActionRemove;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox servers;
		private System.Windows.Forms.ToolBar toolBar1;
		private System.Windows.Forms.ToolBarButton toolBarCreate;
		private System.Windows.Forms.ToolBarButton toolBarSetup;
		private System.Windows.Forms.ToolBarButton toolBarResolve;
		private System.Windows.Forms.ToolBarButton toolBarSync;
		private System.Windows.Forms.ToolBarButton toolBarShare;
		private System.ComponentModel.IContainer components;
		#endregion

		/// <summary>
		/// Instantiates a GlobalProperties object.
		/// </summary>
		public GlobalProperties(iFolderWebService ifolderWebService, IProcEventClient eventClient)
		{
			syncCollectionDelegate = new SyncCollectionDelegate(syncCollection);
			syncFileDelegate = new SyncFileDelegate(syncFile);
			createChangeEventDelegate = new CreateChangeEventDelegate(createChangeEvent);
			deleteEventDelegate = new DeleteEventDelegate(deleteEvent);

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			progressBar1.Visible = false;

			ifWebService = ifolderWebService;
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
			this.servers = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.iFolderView = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
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
			this.menuView = new System.Windows.Forms.MenuItem();
			this.menuViewRefresh = new System.Windows.Forms.MenuItem();
			this.menuHelp = new System.Windows.Forms.MenuItem();
			this.menuHelpHelp = new System.Windows.Forms.MenuItem();
			this.menuHelpAbout = new System.Windows.Forms.MenuItem();
			this.status = new System.Windows.Forms.Label();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.toolBar1 = new System.Windows.Forms.ToolBar();
			this.toolBarCreate = new System.Windows.Forms.ToolBarButton();
			this.toolBarSetup = new System.Windows.Forms.ToolBarButton();
			this.toolBarShare = new System.Windows.Forms.ToolBarButton();
			this.toolBarResolve = new System.Windows.Forms.ToolBarButton();
			this.toolBarSync = new System.Windows.Forms.ToolBarButton();
			this.SuspendLayout();
			// 
			// servers
			// 
			this.servers.AccessibleDescription = resources.GetString("servers.AccessibleDescription");
			this.servers.AccessibleName = resources.GetString("servers.AccessibleName");
			this.servers.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("servers.Anchor")));
			this.servers.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("servers.BackgroundImage")));
			this.servers.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("servers.Dock")));
			this.servers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.servers.Enabled = ((bool)(resources.GetObject("servers.Enabled")));
			this.servers.Font = ((System.Drawing.Font)(resources.GetObject("servers.Font")));
			this.servers.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("servers.ImeMode")));
			this.servers.IntegralHeight = ((bool)(resources.GetObject("servers.IntegralHeight")));
			this.servers.ItemHeight = ((int)(resources.GetObject("servers.ItemHeight")));
			this.servers.Location = ((System.Drawing.Point)(resources.GetObject("servers.Location")));
			this.servers.MaxDropDownItems = ((int)(resources.GetObject("servers.MaxDropDownItems")));
			this.servers.MaxLength = ((int)(resources.GetObject("servers.MaxLength")));
			this.servers.Name = "servers";
			this.servers.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("servers.RightToLeft")));
			this.servers.Size = ((System.Drawing.Size)(resources.GetObject("servers.Size")));
			this.servers.TabIndex = ((int)(resources.GetObject("servers.TabIndex")));
			this.servers.Text = resources.GetString("servers.Text");
			this.servers.Visible = ((bool)(resources.GetObject("servers.Visible")));
			this.servers.SelectedIndexChanged += new System.EventHandler(this.servers_SelectedIndexChanged);
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
			// iFolderView
			// 
			this.iFolderView.AccessibleDescription = resources.GetString("iFolderView.AccessibleDescription");
			this.iFolderView.AccessibleName = resources.GetString("iFolderView.AccessibleName");
			this.iFolderView.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("iFolderView.Alignment")));
			this.iFolderView.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("iFolderView.Anchor")));
			this.iFolderView.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("iFolderView.BackgroundImage")));
			this.iFolderView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						  this.columnHeader1,
																						  this.columnHeader4,
																						  this.columnHeader5});
			this.iFolderView.ContextMenu = this.contextMenu1;
			this.iFolderView.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("iFolderView.Dock")));
			this.iFolderView.Enabled = ((bool)(resources.GetObject("iFolderView.Enabled")));
			this.iFolderView.Font = ((System.Drawing.Font)(resources.GetObject("iFolderView.Font")));
			this.iFolderView.FullRowSelect = true;
			this.iFolderView.HideSelection = false;
			this.iFolderView.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("iFolderView.ImeMode")));
			this.iFolderView.LabelWrap = ((bool)(resources.GetObject("iFolderView.LabelWrap")));
			this.iFolderView.Location = ((System.Drawing.Point)(resources.GetObject("iFolderView.Location")));
			this.iFolderView.MultiSelect = false;
			this.iFolderView.Name = "iFolderView";
			this.iFolderView.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("iFolderView.RightToLeft")));
			this.iFolderView.Size = ((System.Drawing.Size)(resources.GetObject("iFolderView.Size")));
			this.iFolderView.TabIndex = ((int)(resources.GetObject("iFolderView.TabIndex")));
			this.iFolderView.Text = resources.GetString("iFolderView.Text");
			this.iFolderView.View = System.Windows.Forms.View.Details;
			this.iFolderView.Visible = ((bool)(resources.GetObject("iFolderView.Visible")));
			this.iFolderView.DoubleClick += new System.EventHandler(this.iFolderView_DoubleClick);
			this.iFolderView.SelectedIndexChanged += new System.EventHandler(this.iFolderView_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = resources.GetString("columnHeader1.Text");
			this.columnHeader1.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader1.TextAlign")));
			this.columnHeader1.Width = ((int)(resources.GetObject("columnHeader1.Width")));
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = resources.GetString("columnHeader4.Text");
			this.columnHeader4.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader4.TextAlign")));
			this.columnHeader4.Width = ((int)(resources.GetObject("columnHeader4.Width")));
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = resources.GetString("columnHeader5.Text");
			this.columnHeader5.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader5.TextAlign")));
			this.columnHeader5.Width = ((int)(resources.GetObject("columnHeader5.Width")));
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
			// menuView
			// 
			this.menuView.Enabled = ((bool)(resources.GetObject("menuView.Enabled")));
			this.menuView.Index = 1;
			this.menuView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuViewRefresh});
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
			// menuHelp
			// 
			this.menuHelp.Enabled = ((bool)(resources.GetObject("menuHelp.Enabled")));
			this.menuHelp.Index = 2;
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
			// status
			// 
			this.status.AccessibleDescription = resources.GetString("status.AccessibleDescription");
			this.status.AccessibleName = resources.GetString("status.AccessibleName");
			this.status.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("status.Anchor")));
			this.status.AutoSize = ((bool)(resources.GetObject("status.AutoSize")));
			this.status.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("status.Dock")));
			this.status.Enabled = ((bool)(resources.GetObject("status.Enabled")));
			this.status.Font = ((System.Drawing.Font)(resources.GetObject("status.Font")));
			this.status.Image = ((System.Drawing.Image)(resources.GetObject("status.Image")));
			this.status.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("status.ImageAlign")));
			this.status.ImageIndex = ((int)(resources.GetObject("status.ImageIndex")));
			this.status.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("status.ImeMode")));
			this.status.Location = ((System.Drawing.Point)(resources.GetObject("status.Location")));
			this.status.Name = "status";
			this.status.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("status.RightToLeft")));
			this.status.Size = ((System.Drawing.Size)(resources.GetObject("status.Size")));
			this.status.TabIndex = ((int)(resources.GetObject("status.TabIndex")));
			this.status.Text = resources.GetString("status.Text");
			this.status.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("status.TextAlign")));
			this.status.Visible = ((bool)(resources.GetObject("status.Visible")));
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
			// toolBar1
			// 
			this.toolBar1.AccessibleDescription = resources.GetString("toolBar1.AccessibleDescription");
			this.toolBar1.AccessibleName = resources.GetString("toolBar1.AccessibleName");
			this.toolBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("toolBar1.Anchor")));
			this.toolBar1.Appearance = ((System.Windows.Forms.ToolBarAppearance)(resources.GetObject("toolBar1.Appearance")));
			this.toolBar1.AutoSize = ((bool)(resources.GetObject("toolBar1.AutoSize")));
			this.toolBar1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("toolBar1.BackgroundImage")));
			this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						this.toolBarCreate,
																						this.toolBarSetup,
																						this.toolBarShare,
																						this.toolBarResolve,
																						this.toolBarSync});
			this.toolBar1.ButtonSize = ((System.Drawing.Size)(resources.GetObject("toolBar1.ButtonSize")));
			this.toolBar1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("toolBar1.Dock")));
			this.toolBar1.DropDownArrows = ((bool)(resources.GetObject("toolBar1.DropDownArrows")));
			this.toolBar1.Enabled = ((bool)(resources.GetObject("toolBar1.Enabled")));
			this.toolBar1.Font = ((System.Drawing.Font)(resources.GetObject("toolBar1.Font")));
			this.toolBar1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("toolBar1.ImeMode")));
			this.toolBar1.Location = ((System.Drawing.Point)(resources.GetObject("toolBar1.Location")));
			this.toolBar1.Name = "toolBar1";
			this.toolBar1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("toolBar1.RightToLeft")));
			this.toolBar1.ShowToolTips = ((bool)(resources.GetObject("toolBar1.ShowToolTips")));
			this.toolBar1.Size = ((System.Drawing.Size)(resources.GetObject("toolBar1.Size")));
			this.toolBar1.TabIndex = ((int)(resources.GetObject("toolBar1.TabIndex")));
			this.toolBar1.TextAlign = ((System.Windows.Forms.ToolBarTextAlign)(resources.GetObject("toolBar1.TextAlign")));
			this.toolBar1.Visible = ((bool)(resources.GetObject("toolBar1.Visible")));
			this.toolBar1.Wrappable = ((bool)(resources.GetObject("toolBar1.Wrappable")));
			this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
			// 
			// toolBarCreate
			// 
			this.toolBarCreate.Enabled = ((bool)(resources.GetObject("toolBarCreate.Enabled")));
			this.toolBarCreate.ImageIndex = ((int)(resources.GetObject("toolBarCreate.ImageIndex")));
			this.toolBarCreate.Text = resources.GetString("toolBarCreate.Text");
			this.toolBarCreate.ToolTipText = resources.GetString("toolBarCreate.ToolTipText");
			this.toolBarCreate.Visible = ((bool)(resources.GetObject("toolBarCreate.Visible")));
			// 
			// toolBarSetup
			// 
			this.toolBarSetup.Enabled = ((bool)(resources.GetObject("toolBarSetup.Enabled")));
			this.toolBarSetup.ImageIndex = ((int)(resources.GetObject("toolBarSetup.ImageIndex")));
			this.toolBarSetup.Text = resources.GetString("toolBarSetup.Text");
			this.toolBarSetup.ToolTipText = resources.GetString("toolBarSetup.ToolTipText");
			this.toolBarSetup.Visible = ((bool)(resources.GetObject("toolBarSetup.Visible")));
			// 
			// toolBarShare
			// 
			this.toolBarShare.Enabled = ((bool)(resources.GetObject("toolBarShare.Enabled")));
			this.toolBarShare.ImageIndex = ((int)(resources.GetObject("toolBarShare.ImageIndex")));
			this.toolBarShare.Text = resources.GetString("toolBarShare.Text");
			this.toolBarShare.ToolTipText = resources.GetString("toolBarShare.ToolTipText");
			this.toolBarShare.Visible = ((bool)(resources.GetObject("toolBarShare.Visible")));
			// 
			// toolBarResolve
			// 
			this.toolBarResolve.Enabled = ((bool)(resources.GetObject("toolBarResolve.Enabled")));
			this.toolBarResolve.ImageIndex = ((int)(resources.GetObject("toolBarResolve.ImageIndex")));
			this.toolBarResolve.Text = resources.GetString("toolBarResolve.Text");
			this.toolBarResolve.ToolTipText = resources.GetString("toolBarResolve.ToolTipText");
			this.toolBarResolve.Visible = ((bool)(resources.GetObject("toolBarResolve.Visible")));
			// 
			// toolBarSync
			// 
			this.toolBarSync.Enabled = ((bool)(resources.GetObject("toolBarSync.Enabled")));
			this.toolBarSync.ImageIndex = ((int)(resources.GetObject("toolBarSync.ImageIndex")));
			this.toolBarSync.Text = resources.GetString("toolBarSync.Text");
			this.toolBarSync.ToolTipText = resources.GetString("toolBarSync.ToolTipText");
			this.toolBarSync.Visible = ((bool)(resources.GetObject("toolBarSync.Visible")));
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
			this.Controls.Add(this.toolBar1);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.status);
			this.Controls.Add(this.servers);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.iFolderView);
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
			this.VisibleChanged += new System.EventHandler(this.GlobalProperties_VisibleChanged);
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
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds the specified domain to the dropdown list.
		/// </summary>
		/// <param name="domainWeb">The DomainWeb object to add to the list.</param>
		public void AddDomainToList(DomainWeb domainWeb)
		{
			Domain domain = null;
			foreach (Domain d in servers.Items)
			{
				if (d.ID.Equals(domainWeb.ID))
				{
					// The domain is already in the list.
					domain = d;
				}
			}

			if (domain == null)
			{
				// The domain isn't in the list ... add it.
				domain = new Domain(domainWeb);
				servers.Items.Add(domain);
			}

			// Reset the current default domain if the added domain is set to be the default.
			if (domainWeb.IsDefault)
			{
				if (defaultDomain != null)
				{
					defaultDomain.DomainWeb.IsDefault = false;
				}

				// Keep track of the default domain.
				defaultDomain = domain;
			}
		}

		/// <summary>
		/// Remove the specified domain from the dropdown list.
		/// </summary>
		/// <param name="domainWeb">The domainWeb object representing the domain to remove.</param>
		public void RemoveDomainFromList(DomainWeb domainWeb, string defaultDomainID)
		{
			Domain domain = null;
			Domain showAllDomain = null;
			
			foreach (Domain d in servers.Items)
			{
				if (d.ID.Equals(domainWeb.ID))
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
					d.DomainWeb.IsDefault = true;                    
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
		}

		public void InitializeServerList()
		{
			servers.Items.Clear();

			// Add the wild-card domain.
			Domain domain = new Domain(resourceManager.GetString("showAll"));
			servers.Items.Add(domain);
			servers.SelectedItem = domain;
		}

		/// <summary>
		/// Check the specified ID to see if it is the current user.
		/// </summary>
		/// <param name="userID">The ID of the user to check.</param>
		/// <returns><b>True</b> if the specified user ID is the current user; otherwise, <b>False</b>.</returns>
		public bool IsCurrentUser(string userID)
		{
			bool result = false;

			foreach (Domain d in servers.Items)
			{
				if (!d.ShowAll && d.DomainWeb.UserID.Equals(userID))
				{
					result = true;
					break;
				}
			}

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

			foreach (Domain d in servers.Items)
			{
				if (!d.ShowAll && d.DomainWeb.POBoxID.Equals(poBoxID))
				{
					result = true;
					break;
				}
			}

			return result;
		}

		/// <summary>
		/// Checks to see if the specified Domain ID is currently selected in the dropdown list.
		/// </summary>
		/// <param name="domainID">The ID of the Domain.</param>
		/// <returns><b>True</b> if the specified Domain or the wild-card Domain is the currently 
		/// selected domain; otherwise, <b>False</b>.</returns>
		public bool IsSelected(string domainID)
		{
			bool result = false;

			try
			{
				Domain domain = (Domain)servers.SelectedItem;
				if (domain.ShowAll || domain.ID.Equals(domainID))
				{
					result = true;
				}
			}
			catch {}

			return result;
		}

		public void UpdateDomain(DomainWeb domainWeb)
		{
			foreach (Domain d in servers.Items)
			{
				if (d.ID.Equals(domainWeb.ID))
				{
					d.DomainWeb = domainWeb;
					break;
				}
			}

			if ((iFolderView.SelectedItems.Count == 1) && 
				!((iFolderWeb)iFolderView.SelectedItems[0].Tag).IsSubscription &&
				((iFolderWeb)iFolderView.SelectedItems[0].Tag).DomainID.Equals(domainWeb.ID))
			{
				menuSyncNow.Visible = menuActionSync.Enabled = toolBarSync.Enabled = domainWeb.IsEnabled;
			}
		}
		#endregion

		#region Private Methods
		private void syncCollection(CollectionSyncEventArgs syncEventArgs)
		{
			try
			{
				progressBar1.Visible = false;

				switch (syncEventArgs.Action)
				{
					case Action.StartSync:
					{
						status.Text = string.Format(resourceManager.GetString("synciFolder"), syncEventArgs.Name);
						lock (ht)
						{
							ListViewItem lvi = (ListViewItem)ht[syncEventArgs.ID];
							if (lvi != null)
							{
								lvi.SubItems[2].Text = resourceManager.GetString("statusSyncing");
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
								lvi.SubItems[2].Text = syncEventArgs.Successful ? resourceManager.GetString("statusOK") : resourceManager.GetString("statusSyncFailure");
							}
						}

						status.Text = resourceManager.GetString("status.Text");
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
					progressBar1.Maximum = (int)syncEventArgs.SizeToSync;

					switch (syncEventArgs.ObjectType)
					{
						case ObjectType.File:
							status.Text = syncEventArgs.Delete ? 
								string.Format(resourceManager.GetString("deleteClientFile"), syncEventArgs.Name) :
								string.Format(resourceManager.GetString(syncEventArgs.Direction == Direction.Uploading ? "uploadFile" : "downloadFile"), syncEventArgs.Name);
							break;
						case ObjectType.Directory:
							status.Text = syncEventArgs.Delete ? 
								string.Format(resourceManager.GetString("deleteClientDir"), syncEventArgs.Name) :
								string.Format(resourceManager.GetString(syncEventArgs.Direction == Direction.Uploading ? "uploadDir" : "downloadDir"), syncEventArgs.Name);
							break;
						case ObjectType.Unknown:
							status.Text = string.Format(resourceManager.GetString("deleteUnknown"), syncEventArgs.Name);
							break;
					}
				}
				else
				{
					status.Text = syncEventArgs.Name;
					progressBar1.Value = syncEventArgs.SizeToSync > 0 ? (int)(syncEventArgs.SizeToSync - syncEventArgs.SizeRemaining) : progressBar1.Maximum;
				}
			}
			catch {}
		}

		private void deleteEvent(string ID)
		{
			lock (ht)
			{
				ListViewItem lvi = (ListViewItem)ht[ID];
				if (lvi != null)
				{
					lvi.Remove();
					ht.Remove(ID);
				}
			}
		}

		private void createChangeEvent(iFolderWeb ifolder, string eventData)
		{
			if (ifolder != null)
			{
				if (eventData.Equals("NodeCreated"))
				{
					if (IsSelected(ifolder.DomainID))
					{
						addiFolderToListView(ifolder);

						if (ifolder.State.Equals("Local"))
						{
							// Notify the shell.
							Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, ifolder.UnManagedPath, IntPtr.Zero);
						}
					}
				}
				else
				{
					ListViewItem lvi;
					lock (ht)
					{
						// Get the corresponding listview item.
						lvi = (ListViewItem)ht[ifolder.ID];
					}

					if (lvi != null)
					{
						// Update the tag data.
						lvi.Tag = ifolder;
						updateListViewItem(lvi);
					}
				}
			}
		}

		private void addiFolderToListView(iFolderWeb ifolder)
		{
			lock (ht)
			{
				// Add only if it isn't already in the list.
				if (ht[ifolder.ID] == null)
				{
					string[] items = new string[3];
					int imageIndex;

					items[0] = ifolder.Name;
					items[1] = ifolder.IsSubscription ? ifolder.Owner : ifolder.UnManagedPath;
					items[2] = stateToString(ifolder.State, ifolder.HasConflicts, ifolder.IsSubscription, out imageIndex);

					ListViewItem lvi = new ListViewItem(items, imageIndex);
					lvi.Tag = ifolder;
					iFolderView.Items.Add(lvi);

					// Add the listviewitem to the hashtable.
					ht.Add(ifolder.ID, lvi);
				}
			}
		}

		private void updateListViewItem(ListViewItem lvi)
		{
			iFolderWeb ifolder = (iFolderWeb)lvi.Tag;

			if (ifolder.State.Equals("Available") && (ifWebService.GetiFolder(ifolder.CollectionID) != null))
			{
				// The iFolder already exists locally ... remove it from the list.
				lock (ht)
				{
					ht.Remove(((iFolderWeb)lvi.Tag).ID);
				}

				lvi.Remove();
			}
			else
			{
				int imageIndex;
				lvi.SubItems[0].Text = ifolder.Name;
				lvi.SubItems[1].Text = ifolder.IsSubscription ? "" : ifolder.UnManagedPath;
				string statusSync = resourceManager.GetString("statusSyncing");
				string statusSyncFail = resourceManager.GetString("statusSyncFailure");
				string status = stateToString(ifolder.State, ifolder.HasConflicts, ifolder.IsSubscription, out imageIndex);
				if (!lvi.SubItems[2].Text.Equals(statusSync) && !lvi.SubItems[2].Text.Equals(statusSyncFail))
				{
					lvi.SubItems[2].Text = status;
				}
				lvi.ImageIndex = imageIndex;
			}
		}

		private string stateToString(string state, bool conflicts, bool isSubscription, out int imageIndex)
		{
			string status;

			switch (state)
			{
				case "Local":
					if (conflicts)
					{
						imageIndex = 2;
						status = resourceManager.GetString("statusConflicts");
					}
					else
					{					
						imageIndex = 0;
						status = resourceManager.GetString("statusOK");
					}
					break;
				case "Available":
				case "WaitConnect":
				case "WaitSync":
					imageIndex = isSubscription ? 1 : 0;
					status = resourceManager.GetString(state);
					break;
				default:
					// TODO: what icon to use for unknown status?
					imageIndex = 1;
					status = resourceManager.GetString("statusUnknown");
					break;
			}

			return status;
		}

		private void refreshiFolders(Domain domain)
		{
			Cursor.Current = Cursors.WaitCursor;

			iFolderView.Items.Clear();
			iFolderView.SelectedItems.Clear();

			// Disable/hide the menu items and toolbar buttons.
			menuShare.Visible = menuActionShare.Enabled = toolBarShare.Enabled =
				menuProperties.Visible = menuActionProperties.Enabled = /*toolBarProperties.Enabled =*/
				menuRevert.Visible = menuActionRevert.Enabled = /*toolBarRevert.Enabled =*/
				menuSyncNow.Visible = menuActionSync.Enabled = toolBarSync.Enabled =
				menuOpen.Visible = menuActionOpen.Enabled = /*toolBarOpen.Enabled =*/
				menuSeparator1.Visible = menuSeparator2.Visible =
				menuResolve.Visible = menuActionResolve.Visible = toolBarResolve.Enabled =
				menuAccept.Visible = menuActionAccept.Visible = toolBarSetup.Enabled =
				menuActionSeparator2.Visible =
				menuRemove.Visible = menuActionRemove.Visible = /*toolBarRemove.Enabled =*/
				menuActionSeparator2.Visible = false;

			// Show the refresh and create menu items.
			menuRefresh.Visible = menuCreate.Visible = true;

			lock(ht)
			{
				ht.Clear();
			}

			iFolderView.BeginUpdate();

			try
			{
				iFolderWeb[] ifolderArray = domain.ShowAll ? 
					ifWebService.GetAlliFolders() : 
					ifWebService.GetiFoldersForDomain(domain.ID);
				foreach (iFolderWeb ifolder in ifolderArray)
				{
					addiFolderToListView(ifolder);
				}
			}
			catch (Exception ex)
			{
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("iFolderError"), resourceManager.GetString("iFolderErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
				mmb.ShowDialog();
			}

			iFolderView.EndUpdate();
			Cursor.Current = Cursors.Default;
		}

		private void invokeiFolderProperties(ListViewItem lvi, int activeTab)
		{
			iFolderAdvanced ifolderAdvanced = new iFolderAdvanced();
			ifolderAdvanced.CurrentiFolder = (iFolderWeb)lvi.Tag;
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
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("syncError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}
		}

		private void updateEnterpriseData()
		{
			servers.Items.Clear();
			DomainWeb[] domains;
			try
			{
				domains = ifWebService.GetDomains();
				foreach (DomainWeb dw in domains)
				{
					AddDomainToList(dw);
				}
			}
			catch
			{
				// TODO: Message?
			}
		}
		#endregion

		#region Event Handlers
		private void GlobalProperties_Load(object sender, System.EventArgs e)
		{
			// Load the application icon
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_loaded.ico"));

				// TODO: need icons for the different states.
				//	- iFolder with conflicts.
				//	- iFolder that is available.
				//	- iFolder that has been requested.
				//	- iFolder that has been invited. (Invitation.ico?)
				this.iFolderView.SmallImageList = new ImageList();
				iFolderView.SmallImageList.Images.Add(new Icon(Path.Combine(Application.StartupPath, @"res\ifolder.ico")));
				iFolderView.SmallImageList.Images.Add(new Icon(Path.Combine(Application.StartupPath, @"res\serverifolder.ico")));
				iFolderView.SmallImageList.Images.Add(new Icon(Path.Combine(Application.StartupPath, @"res\ifolderconflict.ico")));

				// TODO: add icons for toolbar buttons.
				toolBar1.ImageList = new ImageList();
			}
			catch {} // Non-fatal ... just missing some graphics.

			this.MinimumSize = this.Size;
		}

		private void GlobalProperties_VisibleChanged(object sender, System.EventArgs e)
		{
			if (this.Visible)
			{
				InitializeServerList();

				DomainWeb[] domains;
				try
				{
					domains = ifWebService.GetDomains();
					foreach (DomainWeb dw in domains)
					{
						AddDomainToList(dw);
					}
				}
				catch{}

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
			// TODO:
			MessageBox.Show("This hasn't been implemented yet.");
		}

		private void GlobalProperties_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			// Pressing the F5 key will cause a refresh to occur.
			if (e.KeyCode == Keys.F5)
			{
				refreshiFolders((Domain)servers.SelectedItem);
			}
		}

		private void servers_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			refreshiFolders((Domain)servers.SelectedItem);
		}

		private void menuOpen_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = iFolderView.SelectedItems[0];
			iFolderWeb ifolder = (iFolderWeb)lvi.Tag;

			try
			{
				Process.Start(ifolder.UnManagedPath);
			}
			catch (Exception ex)
			{
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(string.Format(resourceManager.GetString("iFolderOpenError"), ifolder.Name), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}
		}

		private void menuRevert_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = iFolderView.SelectedItems[0];

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				iFolderWeb ifolder = (iFolderWeb)lvi.Tag;

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

					lvi.Tag = newiFolder;

					lock (ht)
					{
						ht.Remove(ifolder.ID);
						ht.Add(newiFolder.ID, lvi);
					}

					updateListViewItem(lvi);
				}
			}
			catch (Exception ex)
			{		
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("iFolderRevertError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}

			Cursor.Current = Cursors.Default;
		}

		private void menuResolve_Click(object sender, System.EventArgs e)
		{
			ConflictResolver conflictResolver = new ConflictResolver();
			conflictResolver.iFolder = (iFolderWeb)iFolderView.SelectedItems[0].Tag;
			conflictResolver.iFolderWebService = ifWebService;
			conflictResolver.LoadPath = Application.StartupPath;
			conflictResolver.Show();		
		}

		private void menuShare_Click(object sender, System.EventArgs e)
		{
			invokeiFolderProperties(iFolderView.SelectedItems[0], 1);
		}

		private void menuSyncNow_Click(object sender, System.EventArgs e)
		{
			synciFolder(((iFolderWeb)iFolderView.SelectedItems[0].Tag).ID);
		}

		private void menuProperties_Click(object sender, System.EventArgs e)
		{
			invokeiFolderProperties(iFolderView.SelectedItems[0], 0);
		}

		private void menuCreate_Click(object sender, System.EventArgs e)
		{
			CreateiFolder createiFolder = new CreateiFolder();
			createiFolder.Servers = servers.Items;
			Domain selectedDomain = (Domain)servers.SelectedItem;
			createiFolder.SelectedDomain = selectedDomain.ShowAll ? defaultDomain : selectedDomain;
			createiFolder.iFolderWebService = ifWebService;

			if ((DialogResult.OK == createiFolder.ShowDialog()) && iFolderComponent.DisplayConfirmationEnabled)
			{
				new iFolderComponent().NewiFolderWizard(Application.StartupPath, createiFolder.iFolderPath);
			}
		}

		private void menuRefresh_Click(object sender, System.EventArgs e)
		{
			refreshiFolders((Domain)servers.SelectedItem);
		}

		private void menuAccept_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = iFolderView.SelectedItems[0];
			iFolderWeb ifolder = (iFolderWeb)lvi.Tag;

			AcceptInvitation acceptInvitation = new AcceptInvitation(ifWebService, ifolder);
			// TODO: get iFolder from acceptInvitation and update the listviewitem with it.
			acceptInvitation.ShowDialog();
		}

		private void menuRemove_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = iFolderView.SelectedItems[0];
			iFolderWeb ifolder = (iFolderWeb)lvi.Tag;
			try
			{
				string message, caption;

				if (IsCurrentUser(ifolder.OwnerID))
				{
					message = resourceManager.GetString("deleteiFolder") + "\n\n" + 
						resourceManager.GetString("removePrompt");
					caption = resourceManager.GetString("removeTitle");
				}
				else
				{
					message = resourceManager.GetString("removeiFolder")  + "\n\n" + 
						resourceManager.GetString("removePrompt");
					caption = resourceManager.GetString("removeTitle");
				}

				if (ifolder.IsSubscription)
				{
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
					MyMessageBox mmb = new Novell.iFolderCom.MyMessageBox(
						message,
						caption,
						string.Empty,
						MyMessageBoxButtons.YesNo,
						MyMessageBoxIcon.Question,
						MyMessageBoxDefaultButton.Button2);
					if (mmb.ShowDialog() == DialogResult.Yes)
					{
						// Revert the iFolder.
						iFolderWeb newiFolder = ifWebService.RevertiFolder(ifolder.ID);

						// Notify the shell.
						Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, ifolder.UnManagedPath, IntPtr.Zero);

						// Update the listview item.
						lvi.Tag = newiFolder;

						lock (ht)
						{
							ht.Remove(ifolder.ID);
							ht.Add(newiFolder.ID, lvi);
						}

						updateListViewItem(lvi);

						// Decline the invitation.
						ifWebService.DeclineiFolderInvitation(newiFolder.DomainID, newiFolder.ID);
					}
				}
			}
			catch (Exception ex)
			{
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("declineError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}
		}

		private void iFolderView_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			iFolderWeb ifolderWeb = null;

			if (iFolderView.SelectedItems.Count == 1)
			{
				ifolderWeb = (iFolderWeb)iFolderView.SelectedItems[0].Tag;
			}

			menuShare.Visible = menuActionShare.Enabled = toolBarShare.Enabled =
				menuProperties.Visible = menuActionProperties.Enabled = /*toolBarProperties.Enabled =*/
				menuRevert.Visible = menuActionRevert.Enabled = /*toolBarRevert.Enabled =*/
				menuSyncNow.Visible = menuActionSync.Enabled = toolBarSync.Enabled =
				menuOpen.Visible = menuActionOpen.Enabled = /*toolBarOpen.Enabled =*/
				menuSeparator1.Visible = menuSeparator2.Visible = 				
				(ifolderWeb != null) && !ifolderWeb.IsSubscription;

			Domain selectedDomain = (Domain)servers.SelectedItem;
			if (!selectedDomain.ShowAll)
			{
				menuSyncNow.Visible = menuActionSync.Enabled = toolBarSync.Enabled = selectedDomain.DomainWeb.IsEnabled;
			}
			else if (toolBarSync.Enabled)
			{
				foreach (Domain d in servers.Items)
				{
					if (d.ID.Equals(ifolderWeb.DomainID))
					{
						menuSyncNow.Visible = menuActionSync.Enabled = toolBarSync.Enabled = d.DomainWeb.IsEnabled;
						break;
					}
				}
			}

			menuResolve.Visible = menuActionResolve.Visible = toolBarResolve.Enabled =
				(ifolderWeb != null) && ifolderWeb.HasConflicts;

			menuRefresh.Visible = menuCreate.Visible = iFolderView.SelectedItems.Count == 0;

			// Display the accept menu item if the selected item is a subscription with state "Available"
			menuAccept.Visible = menuActionAccept.Visible = toolBarSetup.Enabled =
				menuActionSeparator2.Visible =
				(ifolderWeb != null) &&
				ifolderWeb.IsSubscription &&
				ifolderWeb.State.Equals("Available");

			// Display the decline menu item if the selected item is a subscription with state "Available" and from someone else.
			menuRemove.Visible = menuActionRemove.Visible = /*toolBarRemove.Enabled =*/
				menuActionSeparator2.Visible =
				(ifolderWeb != null) &&
				(!ifolderWeb.IsSubscription ||
				ifolderWeb.State.Equals("Available"));

			if (menuRemove.Visible)
			{
				if (IsCurrentUser(((iFolderWeb)iFolderView.SelectedItems[0].Tag).OwnerID))
				{
					menuRemove.Text = menuActionRemove.Text = /*toolBarRemove.ToolTipText =*/
						resourceManager.GetString("deleteAction");
				}
				else
				{
					menuRemove.Text = resourceManager.GetString("menuRemove.Text");
					menuActionRemove.Text = resourceManager.GetString("menuActionRemove.Text");
					/*toolBarRemove.ToolTipText = resourceManager.GetString("toolBarRemove.ToolTipText");*/
				}
			}
		}

		private void iFolderView_DoubleClick(object sender, System.EventArgs e)
		{
			if (iFolderView.SelectedItems.Count == 1)
			{
				ListViewItem lvi = iFolderView.SelectedItems[0];
				iFolderWeb ifolder = (iFolderWeb)lvi.Tag;
				if (ifolder.IsSubscription)
				{
					menuAccept_Click(sender, e);
				}
				else
				{
					menuOpen_Click(sender, e);
				}
			}
		}

		private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch (toolBar1.Buttons.IndexOf(e.Button))
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
					synciFolder(((iFolderWeb)iFolderView.SelectedItems[0].Tag).ID);
					break;
/*				case 1: // Open
					menuOpen_Click(this, new EventArgs());
					break;
				case 3: // Properties
					invokeiFolderProperties(iFolderView.SelectedItems[0], 0);
					break;
				case 6: // Revert
					menuRevert_Click(this, new EventArgs());
					break;
				case 7: // Remove
					menuRemove_Click(this, new EventArgs());
					break;
				case 12: // Refresh
					refreshiFolders((Domain)servers.SelectedItem);
					break;*/
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
	}
}
