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
using System.Net;
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
		private const string iFolderRun = "DisableAutoStart";
		private const string notifyShareDisabled = "NotifyShareDisable";
		private const string notifyCollisionDisabled = "NotifyCollisionDisabled";
		private const string notifyJoinDisabled = "NotifyJoinDisabled";
		private const string iFolderKey = @"SOFTWARE\Novell\iFolder";
//		private const double megaByte = 1048576;
		private const int maxMessages = 500;
		private System.Timers.Timer updateEnterpriseTimer;
		private short retryCount = 2;
		private Hashtable ht;
		private iFolderWebService ifWebService;
		private IProcEventClient eventClient;
		private bool initialConnect = false;
		private int initialBannerWidth;
		private bool shutdown = false;
		private Domain defaultDomain = null;
		private System.Windows.Forms.NumericUpDown defaultInterval;
		private System.Windows.Forms.CheckBox displayConfirmation;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox autoSync;
		private System.Windows.Forms.PictureBox banner;
		private System.Windows.Forms.CheckBox autoStart;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListView iFolderView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ListBox log;
		private System.Windows.Forms.Button saveLog;
		private System.Windows.Forms.Button clearLog;
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
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.TextBox proxy;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button create;
		private System.Windows.Forms.TabPage tabPage5;
		private System.Windows.Forms.MenuItem menuResolve;
		private System.Windows.Forms.MenuItem menuActionResolve;
		private System.Windows.Forms.MenuItem menuAccept;
		private System.Windows.Forms.MenuItem menuActionAccept;
		private System.Windows.Forms.MenuItem menuActionSeparator2;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.MenuItem menuExit;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuHelp;
		private System.Windows.Forms.MenuItem menuHelpHelp;
		private System.Windows.Forms.MenuItem menuHelpAbout;
		private System.Windows.Forms.CheckBox useProxy;
		private System.Windows.Forms.NumericUpDown port;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label status;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Button apply;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.MenuItem menuRemove;
		private System.Windows.Forms.MenuItem menuActionRemove;
		private System.Windows.Forms.CheckBox notifyShared;
		private System.Windows.Forms.CheckBox notifyCollisions;
		private System.Windows.Forms.CheckBox notifyJoins;
		private System.Windows.Forms.CheckBox defaultServer;
		private System.Windows.Forms.ComboBox servers2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button addAccount;
		private System.Windows.Forms.Button removeAccount;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.CheckBox rememberPassword;
		private System.Windows.Forms.CheckBox autoLogin;
		private System.Windows.Forms.Button advanced;
		private System.Windows.Forms.Button details;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox userName;
		private System.Windows.Forms.TextBox server;
		private System.Windows.Forms.TextBox password;
		private System.Windows.Forms.ListView accounts;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.Button connect;
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

			defaultInterval.TextChanged += new EventHandler(defaultInterval_ValueChanged);
			ShowEnterpriseTab = false;
			progressBar1.Visible = false;

			// Show the first tab page by default.
			tabControl1.SelectedTab = tabPage1;

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

			// Set the min/max values for port.
			port.Minimum = IPEndPoint.MinPort;
			port.Maximum = IPEndPoint.MaxPort;

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
			this.defaultInterval = new System.Windows.Forms.NumericUpDown();
			this.displayConfirmation = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.servers2 = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.create = new System.Windows.Forms.Button();
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
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.cancel = new System.Windows.Forms.Button();
			this.apply = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.autoSync = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.notifyJoins = new System.Windows.Forms.CheckBox();
			this.notifyCollisions = new System.Windows.Forms.CheckBox();
			this.notifyShared = new System.Windows.Forms.CheckBox();
			this.autoStart = new System.Windows.Forms.CheckBox();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.proxy = new System.Windows.Forms.TextBox();
			this.port = new System.Windows.Forms.NumericUpDown();
			this.useProxy = new System.Windows.Forms.CheckBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.clearLog = new System.Windows.Forms.Button();
			this.saveLog = new System.Windows.Forms.Button();
			this.log = new System.Windows.Forms.ListBox();
			this.label6 = new System.Windows.Forms.Label();
			this.tabPage5 = new System.Windows.Forms.TabPage();
			this.accounts = new System.Windows.Forms.ListView();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.details = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.password = new System.Windows.Forms.TextBox();
			this.server = new System.Windows.Forms.TextBox();
			this.userName = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.advanced = new System.Windows.Forms.Button();
			this.autoLogin = new System.Windows.Forms.CheckBox();
			this.rememberPassword = new System.Windows.Forms.CheckBox();
			this.label10 = new System.Windows.Forms.Label();
			this.defaultServer = new System.Windows.Forms.CheckBox();
			this.removeAccount = new System.Windows.Forms.Button();
			this.addAccount = new System.Windows.Forms.Button();
			this.banner = new System.Windows.Forms.PictureBox();
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
			this.connect = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).BeginInit();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.port)).BeginInit();
			this.tabPage3.SuspendLayout();
			this.tabPage5.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// defaultInterval
			// 
			this.defaultInterval.AccessibleDescription = resources.GetString("defaultInterval.AccessibleDescription");
			this.defaultInterval.AccessibleName = resources.GetString("defaultInterval.AccessibleName");
			this.defaultInterval.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("defaultInterval.Anchor")));
			this.defaultInterval.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("defaultInterval.Dock")));
			this.defaultInterval.Enabled = ((bool)(resources.GetObject("defaultInterval.Enabled")));
			this.defaultInterval.Font = ((System.Drawing.Font)(resources.GetObject("defaultInterval.Font")));
			this.defaultInterval.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("defaultInterval.ImeMode")));
			this.defaultInterval.Increment = new System.Decimal(new int[] {
																			  5,
																			  0,
																			  0,
																			  0});
			this.defaultInterval.Location = ((System.Drawing.Point)(resources.GetObject("defaultInterval.Location")));
			this.defaultInterval.Maximum = new System.Decimal(new int[] {
																			86400,
																			0,
																			0,
																			0});
			this.defaultInterval.Minimum = new System.Decimal(new int[] {
																			1,
																			0,
																			0,
																			-2147483648});
			this.defaultInterval.Name = "defaultInterval";
			this.defaultInterval.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("defaultInterval.RightToLeft")));
			this.defaultInterval.Size = ((System.Drawing.Size)(resources.GetObject("defaultInterval.Size")));
			this.defaultInterval.TabIndex = ((int)(resources.GetObject("defaultInterval.TabIndex")));
			this.defaultInterval.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("defaultInterval.TextAlign")));
			this.defaultInterval.ThousandsSeparator = ((bool)(resources.GetObject("defaultInterval.ThousandsSeparator")));
			this.defaultInterval.UpDownAlign = ((System.Windows.Forms.LeftRightAlignment)(resources.GetObject("defaultInterval.UpDownAlign")));
			this.defaultInterval.Visible = ((bool)(resources.GetObject("defaultInterval.Visible")));
			// 
			// displayConfirmation
			// 
			this.displayConfirmation.AccessibleDescription = resources.GetString("displayConfirmation.AccessibleDescription");
			this.displayConfirmation.AccessibleName = resources.GetString("displayConfirmation.AccessibleName");
			this.displayConfirmation.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("displayConfirmation.Anchor")));
			this.displayConfirmation.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("displayConfirmation.Appearance")));
			this.displayConfirmation.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("displayConfirmation.BackgroundImage")));
			this.displayConfirmation.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("displayConfirmation.CheckAlign")));
			this.displayConfirmation.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("displayConfirmation.Dock")));
			this.displayConfirmation.Enabled = ((bool)(resources.GetObject("displayConfirmation.Enabled")));
			this.displayConfirmation.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("displayConfirmation.FlatStyle")));
			this.displayConfirmation.Font = ((System.Drawing.Font)(resources.GetObject("displayConfirmation.Font")));
			this.displayConfirmation.Image = ((System.Drawing.Image)(resources.GetObject("displayConfirmation.Image")));
			this.displayConfirmation.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("displayConfirmation.ImageAlign")));
			this.displayConfirmation.ImageIndex = ((int)(resources.GetObject("displayConfirmation.ImageIndex")));
			this.displayConfirmation.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("displayConfirmation.ImeMode")));
			this.displayConfirmation.Location = ((System.Drawing.Point)(resources.GetObject("displayConfirmation.Location")));
			this.displayConfirmation.Name = "displayConfirmation";
			this.displayConfirmation.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("displayConfirmation.RightToLeft")));
			this.displayConfirmation.Size = ((System.Drawing.Size)(resources.GetObject("displayConfirmation.Size")));
			this.displayConfirmation.TabIndex = ((int)(resources.GetObject("displayConfirmation.TabIndex")));
			this.displayConfirmation.Text = resources.GetString("displayConfirmation.Text");
			this.displayConfirmation.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("displayConfirmation.TextAlign")));
			this.displayConfirmation.Visible = ((bool)(resources.GetObject("displayConfirmation.Visible")));
			this.displayConfirmation.CheckedChanged += new System.EventHandler(this.displayConfirmation_CheckedChanged);
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
			// tabControl1
			// 
			this.tabControl1.AccessibleDescription = resources.GetString("tabControl1.AccessibleDescription");
			this.tabControl1.AccessibleName = resources.GetString("tabControl1.AccessibleName");
			this.tabControl1.Alignment = ((System.Windows.Forms.TabAlignment)(resources.GetObject("tabControl1.Alignment")));
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabControl1.Anchor")));
			this.tabControl1.Appearance = ((System.Windows.Forms.TabAppearance)(resources.GetObject("tabControl1.Appearance")));
			this.tabControl1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabControl1.BackgroundImage")));
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Controls.Add(this.tabPage5);
			this.tabControl1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabControl1.Dock")));
			this.tabControl1.Enabled = ((bool)(resources.GetObject("tabControl1.Enabled")));
			this.tabControl1.Font = ((System.Drawing.Font)(resources.GetObject("tabControl1.Font")));
			this.tabControl1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabControl1.ImeMode")));
			this.tabControl1.ItemSize = ((System.Drawing.Size)(resources.GetObject("tabControl1.ItemSize")));
			this.tabControl1.Location = ((System.Drawing.Point)(resources.GetObject("tabControl1.Location")));
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.Padding = ((System.Drawing.Point)(resources.GetObject("tabControl1.Padding")));
			this.tabControl1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabControl1.RightToLeft")));
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.ShowToolTips = ((bool)(resources.GetObject("tabControl1.ShowToolTips")));
			this.tabControl1.Size = ((System.Drawing.Size)(resources.GetObject("tabControl1.Size")));
			this.tabControl1.TabIndex = ((int)(resources.GetObject("tabControl1.TabIndex")));
			this.tabControl1.Text = resources.GetString("tabControl1.Text");
			this.tabControl1.Visible = ((bool)(resources.GetObject("tabControl1.Visible")));
			// 
			// tabPage1
			// 
			this.tabPage1.AccessibleDescription = resources.GetString("tabPage1.AccessibleDescription");
			this.tabPage1.AccessibleName = resources.GetString("tabPage1.AccessibleName");
			this.tabPage1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabPage1.Anchor")));
			this.tabPage1.AutoScroll = ((bool)(resources.GetObject("tabPage1.AutoScroll")));
			this.tabPage1.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabPage1.AutoScrollMargin")));
			this.tabPage1.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabPage1.AutoScrollMinSize")));
			this.tabPage1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabPage1.BackgroundImage")));
			this.tabPage1.Controls.Add(this.servers2);
			this.tabPage1.Controls.Add(this.label1);
			this.tabPage1.Controls.Add(this.create);
			this.tabPage1.Controls.Add(this.iFolderView);
			this.tabPage1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabPage1.Dock")));
			this.tabPage1.Enabled = ((bool)(resources.GetObject("tabPage1.Enabled")));
			this.tabPage1.Font = ((System.Drawing.Font)(resources.GetObject("tabPage1.Font")));
			this.tabPage1.ImageIndex = ((int)(resources.GetObject("tabPage1.ImageIndex")));
			this.tabPage1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabPage1.ImeMode")));
			this.tabPage1.Location = ((System.Drawing.Point)(resources.GetObject("tabPage1.Location")));
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabPage1.RightToLeft")));
			this.tabPage1.Size = ((System.Drawing.Size)(resources.GetObject("tabPage1.Size")));
			this.tabPage1.TabIndex = ((int)(resources.GetObject("tabPage1.TabIndex")));
			this.tabPage1.Text = resources.GetString("tabPage1.Text");
			this.tabPage1.ToolTipText = resources.GetString("tabPage1.ToolTipText");
			this.tabPage1.Visible = ((bool)(resources.GetObject("tabPage1.Visible")));
			// 
			// servers2
			// 
			this.servers2.AccessibleDescription = resources.GetString("servers2.AccessibleDescription");
			this.servers2.AccessibleName = resources.GetString("servers2.AccessibleName");
			this.servers2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("servers2.Anchor")));
			this.servers2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("servers2.BackgroundImage")));
			this.servers2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("servers2.Dock")));
			this.servers2.Enabled = ((bool)(resources.GetObject("servers2.Enabled")));
			this.servers2.Font = ((System.Drawing.Font)(resources.GetObject("servers2.Font")));
			this.servers2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("servers2.ImeMode")));
			this.servers2.IntegralHeight = ((bool)(resources.GetObject("servers2.IntegralHeight")));
			this.servers2.ItemHeight = ((int)(resources.GetObject("servers2.ItemHeight")));
			this.servers2.Location = ((System.Drawing.Point)(resources.GetObject("servers2.Location")));
			this.servers2.MaxDropDownItems = ((int)(resources.GetObject("servers2.MaxDropDownItems")));
			this.servers2.MaxLength = ((int)(resources.GetObject("servers2.MaxLength")));
			this.servers2.Name = "servers2";
			this.servers2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("servers2.RightToLeft")));
			this.servers2.Size = ((System.Drawing.Size)(resources.GetObject("servers2.Size")));
			this.servers2.TabIndex = ((int)(resources.GetObject("servers2.TabIndex")));
			this.servers2.Text = resources.GetString("servers2.Text");
			this.servers2.Visible = ((bool)(resources.GetObject("servers2.Visible")));
			this.servers2.SelectedIndexChanged += new System.EventHandler(this.servers2_SelectedIndexChanged);
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
			this.contextMenu1.Popup += new System.EventHandler(this.contextMenu1_Popup);
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
			// tabPage2
			// 
			this.tabPage2.AccessibleDescription = resources.GetString("tabPage2.AccessibleDescription");
			this.tabPage2.AccessibleName = resources.GetString("tabPage2.AccessibleName");
			this.tabPage2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabPage2.Anchor")));
			this.tabPage2.AutoScroll = ((bool)(resources.GetObject("tabPage2.AutoScroll")));
			this.tabPage2.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabPage2.AutoScrollMargin")));
			this.tabPage2.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabPage2.AutoScrollMinSize")));
			this.tabPage2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabPage2.BackgroundImage")));
			this.tabPage2.Controls.Add(this.cancel);
			this.tabPage2.Controls.Add(this.apply);
			this.tabPage2.Controls.Add(this.groupBox1);
			this.tabPage2.Controls.Add(this.groupBox3);
			this.tabPage2.Controls.Add(this.groupBox5);
			this.tabPage2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabPage2.Dock")));
			this.tabPage2.Enabled = ((bool)(resources.GetObject("tabPage2.Enabled")));
			this.tabPage2.Font = ((System.Drawing.Font)(resources.GetObject("tabPage2.Font")));
			this.tabPage2.ImageIndex = ((int)(resources.GetObject("tabPage2.ImageIndex")));
			this.tabPage2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabPage2.ImeMode")));
			this.tabPage2.Location = ((System.Drawing.Point)(resources.GetObject("tabPage2.Location")));
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabPage2.RightToLeft")));
			this.tabPage2.Size = ((System.Drawing.Size)(resources.GetObject("tabPage2.Size")));
			this.tabPage2.TabIndex = ((int)(resources.GetObject("tabPage2.TabIndex")));
			this.tabPage2.Text = resources.GetString("tabPage2.Text");
			this.tabPage2.ToolTipText = resources.GetString("tabPage2.ToolTipText");
			this.tabPage2.Visible = ((bool)(resources.GetObject("tabPage2.Visible")));
			// 
			// cancel
			// 
			this.cancel.AccessibleDescription = resources.GetString("cancel.AccessibleDescription");
			this.cancel.AccessibleName = resources.GetString("cancel.AccessibleName");
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cancel.Anchor")));
			this.cancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cancel.BackgroundImage")));
			this.cancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cancel.Dock")));
			this.cancel.Enabled = ((bool)(resources.GetObject("cancel.Enabled")));
			this.cancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("cancel.FlatStyle")));
			this.cancel.Font = ((System.Drawing.Font)(resources.GetObject("cancel.Font")));
			this.cancel.Image = ((System.Drawing.Image)(resources.GetObject("cancel.Image")));
			this.cancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cancel.ImageAlign")));
			this.cancel.ImageIndex = ((int)(resources.GetObject("cancel.ImageIndex")));
			this.cancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cancel.ImeMode")));
			this.cancel.Location = ((System.Drawing.Point)(resources.GetObject("cancel.Location")));
			this.cancel.Name = "cancel";
			this.cancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cancel.RightToLeft")));
			this.cancel.Size = ((System.Drawing.Size)(resources.GetObject("cancel.Size")));
			this.cancel.TabIndex = ((int)(resources.GetObject("cancel.TabIndex")));
			this.cancel.Text = resources.GetString("cancel.Text");
			this.cancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cancel.TextAlign")));
			this.cancel.Visible = ((bool)(resources.GetObject("cancel.Visible")));
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// apply
			// 
			this.apply.AccessibleDescription = resources.GetString("apply.AccessibleDescription");
			this.apply.AccessibleName = resources.GetString("apply.AccessibleName");
			this.apply.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("apply.Anchor")));
			this.apply.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("apply.BackgroundImage")));
			this.apply.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("apply.Dock")));
			this.apply.Enabled = ((bool)(resources.GetObject("apply.Enabled")));
			this.apply.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("apply.FlatStyle")));
			this.apply.Font = ((System.Drawing.Font)(resources.GetObject("apply.Font")));
			this.apply.Image = ((System.Drawing.Image)(resources.GetObject("apply.Image")));
			this.apply.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("apply.ImageAlign")));
			this.apply.ImageIndex = ((int)(resources.GetObject("apply.ImageIndex")));
			this.apply.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("apply.ImeMode")));
			this.apply.Location = ((System.Drawing.Point)(resources.GetObject("apply.Location")));
			this.apply.Name = "apply";
			this.apply.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("apply.RightToLeft")));
			this.apply.Size = ((System.Drawing.Size)(resources.GetObject("apply.Size")));
			this.apply.TabIndex = ((int)(resources.GetObject("apply.TabIndex")));
			this.apply.Text = resources.GetString("apply.Text");
			this.apply.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("apply.TextAlign")));
			this.apply.Visible = ((bool)(resources.GetObject("apply.Visible")));
			this.apply.Click += new System.EventHandler(this.apply_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.AccessibleDescription = resources.GetString("groupBox1.AccessibleDescription");
			this.groupBox1.AccessibleName = resources.GetString("groupBox1.AccessibleName");
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox1.Anchor")));
			this.groupBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox1.BackgroundImage")));
			this.groupBox1.Controls.Add(this.defaultInterval);
			this.groupBox1.Controls.Add(this.autoSync);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBox1.Dock")));
			this.groupBox1.Enabled = ((bool)(resources.GetObject("groupBox1.Enabled")));
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
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
			// autoSync
			// 
			this.autoSync.AccessibleDescription = resources.GetString("autoSync.AccessibleDescription");
			this.autoSync.AccessibleName = resources.GetString("autoSync.AccessibleName");
			this.autoSync.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("autoSync.Anchor")));
			this.autoSync.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("autoSync.Appearance")));
			this.autoSync.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("autoSync.BackgroundImage")));
			this.autoSync.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoSync.CheckAlign")));
			this.autoSync.Checked = true;
			this.autoSync.CheckState = System.Windows.Forms.CheckState.Checked;
			this.autoSync.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("autoSync.Dock")));
			this.autoSync.Enabled = ((bool)(resources.GetObject("autoSync.Enabled")));
			this.autoSync.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("autoSync.FlatStyle")));
			this.autoSync.Font = ((System.Drawing.Font)(resources.GetObject("autoSync.Font")));
			this.autoSync.Image = ((System.Drawing.Image)(resources.GetObject("autoSync.Image")));
			this.autoSync.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoSync.ImageAlign")));
			this.autoSync.ImageIndex = ((int)(resources.GetObject("autoSync.ImageIndex")));
			this.autoSync.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("autoSync.ImeMode")));
			this.autoSync.Location = ((System.Drawing.Point)(resources.GetObject("autoSync.Location")));
			this.autoSync.Name = "autoSync";
			this.autoSync.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("autoSync.RightToLeft")));
			this.autoSync.Size = ((System.Drawing.Size)(resources.GetObject("autoSync.Size")));
			this.autoSync.TabIndex = ((int)(resources.GetObject("autoSync.TabIndex")));
			this.autoSync.Text = resources.GetString("autoSync.Text");
			this.autoSync.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoSync.TextAlign")));
			this.autoSync.Visible = ((bool)(resources.GetObject("autoSync.Visible")));
			this.autoSync.CheckedChanged += new System.EventHandler(this.autoSync_CheckedChanged);
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
			// groupBox3
			// 
			this.groupBox3.AccessibleDescription = resources.GetString("groupBox3.AccessibleDescription");
			this.groupBox3.AccessibleName = resources.GetString("groupBox3.AccessibleName");
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox3.Anchor")));
			this.groupBox3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox3.BackgroundImage")));
			this.groupBox3.Controls.Add(this.notifyJoins);
			this.groupBox3.Controls.Add(this.notifyCollisions);
			this.groupBox3.Controls.Add(this.notifyShared);
			this.groupBox3.Controls.Add(this.autoStart);
			this.groupBox3.Controls.Add(this.displayConfirmation);
			this.groupBox3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBox3.Dock")));
			this.groupBox3.Enabled = ((bool)(resources.GetObject("groupBox3.Enabled")));
			this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox3.Font = ((System.Drawing.Font)(resources.GetObject("groupBox3.Font")));
			this.groupBox3.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBox3.ImeMode")));
			this.groupBox3.Location = ((System.Drawing.Point)(resources.GetObject("groupBox3.Location")));
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBox3.RightToLeft")));
			this.groupBox3.Size = ((System.Drawing.Size)(resources.GetObject("groupBox3.Size")));
			this.groupBox3.TabIndex = ((int)(resources.GetObject("groupBox3.TabIndex")));
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = resources.GetString("groupBox3.Text");
			this.groupBox3.Visible = ((bool)(resources.GetObject("groupBox3.Visible")));
			// 
			// notifyJoins
			// 
			this.notifyJoins.AccessibleDescription = resources.GetString("notifyJoins.AccessibleDescription");
			this.notifyJoins.AccessibleName = resources.GetString("notifyJoins.AccessibleName");
			this.notifyJoins.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("notifyJoins.Anchor")));
			this.notifyJoins.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("notifyJoins.Appearance")));
			this.notifyJoins.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("notifyJoins.BackgroundImage")));
			this.notifyJoins.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyJoins.CheckAlign")));
			this.notifyJoins.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("notifyJoins.Dock")));
			this.notifyJoins.Enabled = ((bool)(resources.GetObject("notifyJoins.Enabled")));
			this.notifyJoins.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("notifyJoins.FlatStyle")));
			this.notifyJoins.Font = ((System.Drawing.Font)(resources.GetObject("notifyJoins.Font")));
			this.notifyJoins.Image = ((System.Drawing.Image)(resources.GetObject("notifyJoins.Image")));
			this.notifyJoins.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyJoins.ImageAlign")));
			this.notifyJoins.ImageIndex = ((int)(resources.GetObject("notifyJoins.ImageIndex")));
			this.notifyJoins.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("notifyJoins.ImeMode")));
			this.notifyJoins.Location = ((System.Drawing.Point)(resources.GetObject("notifyJoins.Location")));
			this.notifyJoins.Name = "notifyJoins";
			this.notifyJoins.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("notifyJoins.RightToLeft")));
			this.notifyJoins.Size = ((System.Drawing.Size)(resources.GetObject("notifyJoins.Size")));
			this.notifyJoins.TabIndex = ((int)(resources.GetObject("notifyJoins.TabIndex")));
			this.notifyJoins.Text = resources.GetString("notifyJoins.Text");
			this.notifyJoins.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyJoins.TextAlign")));
			this.notifyJoins.Visible = ((bool)(resources.GetObject("notifyJoins.Visible")));
			this.notifyJoins.CheckedChanged += new System.EventHandler(this.notifyJoins_CheckedChanged);
			// 
			// notifyCollisions
			// 
			this.notifyCollisions.AccessibleDescription = resources.GetString("notifyCollisions.AccessibleDescription");
			this.notifyCollisions.AccessibleName = resources.GetString("notifyCollisions.AccessibleName");
			this.notifyCollisions.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("notifyCollisions.Anchor")));
			this.notifyCollisions.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("notifyCollisions.Appearance")));
			this.notifyCollisions.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("notifyCollisions.BackgroundImage")));
			this.notifyCollisions.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyCollisions.CheckAlign")));
			this.notifyCollisions.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("notifyCollisions.Dock")));
			this.notifyCollisions.Enabled = ((bool)(resources.GetObject("notifyCollisions.Enabled")));
			this.notifyCollisions.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("notifyCollisions.FlatStyle")));
			this.notifyCollisions.Font = ((System.Drawing.Font)(resources.GetObject("notifyCollisions.Font")));
			this.notifyCollisions.Image = ((System.Drawing.Image)(resources.GetObject("notifyCollisions.Image")));
			this.notifyCollisions.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyCollisions.ImageAlign")));
			this.notifyCollisions.ImageIndex = ((int)(resources.GetObject("notifyCollisions.ImageIndex")));
			this.notifyCollisions.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("notifyCollisions.ImeMode")));
			this.notifyCollisions.Location = ((System.Drawing.Point)(resources.GetObject("notifyCollisions.Location")));
			this.notifyCollisions.Name = "notifyCollisions";
			this.notifyCollisions.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("notifyCollisions.RightToLeft")));
			this.notifyCollisions.Size = ((System.Drawing.Size)(resources.GetObject("notifyCollisions.Size")));
			this.notifyCollisions.TabIndex = ((int)(resources.GetObject("notifyCollisions.TabIndex")));
			this.notifyCollisions.Text = resources.GetString("notifyCollisions.Text");
			this.notifyCollisions.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyCollisions.TextAlign")));
			this.notifyCollisions.Visible = ((bool)(resources.GetObject("notifyCollisions.Visible")));
			this.notifyCollisions.CheckedChanged += new System.EventHandler(this.notifyCollisions_CheckedChanged);
			// 
			// notifyShared
			// 
			this.notifyShared.AccessibleDescription = resources.GetString("notifyShared.AccessibleDescription");
			this.notifyShared.AccessibleName = resources.GetString("notifyShared.AccessibleName");
			this.notifyShared.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("notifyShared.Anchor")));
			this.notifyShared.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("notifyShared.Appearance")));
			this.notifyShared.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("notifyShared.BackgroundImage")));
			this.notifyShared.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyShared.CheckAlign")));
			this.notifyShared.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("notifyShared.Dock")));
			this.notifyShared.Enabled = ((bool)(resources.GetObject("notifyShared.Enabled")));
			this.notifyShared.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("notifyShared.FlatStyle")));
			this.notifyShared.Font = ((System.Drawing.Font)(resources.GetObject("notifyShared.Font")));
			this.notifyShared.Image = ((System.Drawing.Image)(resources.GetObject("notifyShared.Image")));
			this.notifyShared.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyShared.ImageAlign")));
			this.notifyShared.ImageIndex = ((int)(resources.GetObject("notifyShared.ImageIndex")));
			this.notifyShared.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("notifyShared.ImeMode")));
			this.notifyShared.Location = ((System.Drawing.Point)(resources.GetObject("notifyShared.Location")));
			this.notifyShared.Name = "notifyShared";
			this.notifyShared.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("notifyShared.RightToLeft")));
			this.notifyShared.Size = ((System.Drawing.Size)(resources.GetObject("notifyShared.Size")));
			this.notifyShared.TabIndex = ((int)(resources.GetObject("notifyShared.TabIndex")));
			this.notifyShared.Text = resources.GetString("notifyShared.Text");
			this.notifyShared.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyShared.TextAlign")));
			this.notifyShared.Visible = ((bool)(resources.GetObject("notifyShared.Visible")));
			this.notifyShared.CheckedChanged += new System.EventHandler(this.notifyShared_CheckedChanged);
			// 
			// autoStart
			// 
			this.autoStart.AccessibleDescription = resources.GetString("autoStart.AccessibleDescription");
			this.autoStart.AccessibleName = resources.GetString("autoStart.AccessibleName");
			this.autoStart.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("autoStart.Anchor")));
			this.autoStart.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("autoStart.Appearance")));
			this.autoStart.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("autoStart.BackgroundImage")));
			this.autoStart.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoStart.CheckAlign")));
			this.autoStart.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("autoStart.Dock")));
			this.autoStart.Enabled = ((bool)(resources.GetObject("autoStart.Enabled")));
			this.autoStart.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("autoStart.FlatStyle")));
			this.autoStart.Font = ((System.Drawing.Font)(resources.GetObject("autoStart.Font")));
			this.autoStart.Image = ((System.Drawing.Image)(resources.GetObject("autoStart.Image")));
			this.autoStart.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoStart.ImageAlign")));
			this.autoStart.ImageIndex = ((int)(resources.GetObject("autoStart.ImageIndex")));
			this.autoStart.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("autoStart.ImeMode")));
			this.autoStart.Location = ((System.Drawing.Point)(resources.GetObject("autoStart.Location")));
			this.autoStart.Name = "autoStart";
			this.autoStart.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("autoStart.RightToLeft")));
			this.autoStart.Size = ((System.Drawing.Size)(resources.GetObject("autoStart.Size")));
			this.autoStart.TabIndex = ((int)(resources.GetObject("autoStart.TabIndex")));
			this.autoStart.Text = resources.GetString("autoStart.Text");
			this.autoStart.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoStart.TextAlign")));
			this.autoStart.Visible = ((bool)(resources.GetObject("autoStart.Visible")));
			this.autoStart.CheckedChanged += new System.EventHandler(this.autoStart_CheckedChanged);
			// 
			// groupBox5
			// 
			this.groupBox5.AccessibleDescription = resources.GetString("groupBox5.AccessibleDescription");
			this.groupBox5.AccessibleName = resources.GetString("groupBox5.AccessibleName");
			this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox5.Anchor")));
			this.groupBox5.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox5.BackgroundImage")));
			this.groupBox5.Controls.Add(this.proxy);
			this.groupBox5.Controls.Add(this.port);
			this.groupBox5.Controls.Add(this.useProxy);
			this.groupBox5.Controls.Add(this.label7);
			this.groupBox5.Controls.Add(this.label4);
			this.groupBox5.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBox5.Dock")));
			this.groupBox5.Enabled = ((bool)(resources.GetObject("groupBox5.Enabled")));
			this.groupBox5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox5.Font = ((System.Drawing.Font)(resources.GetObject("groupBox5.Font")));
			this.groupBox5.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBox5.ImeMode")));
			this.groupBox5.Location = ((System.Drawing.Point)(resources.GetObject("groupBox5.Location")));
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBox5.RightToLeft")));
			this.groupBox5.Size = ((System.Drawing.Size)(resources.GetObject("groupBox5.Size")));
			this.groupBox5.TabIndex = ((int)(resources.GetObject("groupBox5.TabIndex")));
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = resources.GetString("groupBox5.Text");
			this.groupBox5.Visible = ((bool)(resources.GetObject("groupBox5.Visible")));
			// 
			// proxy
			// 
			this.proxy.AccessibleDescription = resources.GetString("proxy.AccessibleDescription");
			this.proxy.AccessibleName = resources.GetString("proxy.AccessibleName");
			this.proxy.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("proxy.Anchor")));
			this.proxy.AutoSize = ((bool)(resources.GetObject("proxy.AutoSize")));
			this.proxy.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("proxy.BackgroundImage")));
			this.proxy.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("proxy.Dock")));
			this.proxy.Enabled = ((bool)(resources.GetObject("proxy.Enabled")));
			this.proxy.Font = ((System.Drawing.Font)(resources.GetObject("proxy.Font")));
			this.proxy.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("proxy.ImeMode")));
			this.proxy.Location = ((System.Drawing.Point)(resources.GetObject("proxy.Location")));
			this.proxy.MaxLength = ((int)(resources.GetObject("proxy.MaxLength")));
			this.proxy.Multiline = ((bool)(resources.GetObject("proxy.Multiline")));
			this.proxy.Name = "proxy";
			this.proxy.PasswordChar = ((char)(resources.GetObject("proxy.PasswordChar")));
			this.proxy.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("proxy.RightToLeft")));
			this.proxy.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("proxy.ScrollBars")));
			this.proxy.Size = ((System.Drawing.Size)(resources.GetObject("proxy.Size")));
			this.proxy.TabIndex = ((int)(resources.GetObject("proxy.TabIndex")));
			this.proxy.Text = resources.GetString("proxy.Text");
			this.proxy.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("proxy.TextAlign")));
			this.proxy.Visible = ((bool)(resources.GetObject("proxy.Visible")));
			this.proxy.WordWrap = ((bool)(resources.GetObject("proxy.WordWrap")));
			this.proxy.TextChanged += new System.EventHandler(this.proxy_TextChanged);
			// 
			// port
			// 
			this.port.AccessibleDescription = resources.GetString("port.AccessibleDescription");
			this.port.AccessibleName = resources.GetString("port.AccessibleName");
			this.port.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("port.Anchor")));
			this.port.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("port.Dock")));
			this.port.Enabled = ((bool)(resources.GetObject("port.Enabled")));
			this.port.Font = ((System.Drawing.Font)(resources.GetObject("port.Font")));
			this.port.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("port.ImeMode")));
			this.port.Location = ((System.Drawing.Point)(resources.GetObject("port.Location")));
			this.port.Name = "port";
			this.port.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("port.RightToLeft")));
			this.port.Size = ((System.Drawing.Size)(resources.GetObject("port.Size")));
			this.port.TabIndex = ((int)(resources.GetObject("port.TabIndex")));
			this.port.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("port.TextAlign")));
			this.port.ThousandsSeparator = ((bool)(resources.GetObject("port.ThousandsSeparator")));
			this.port.UpDownAlign = ((System.Windows.Forms.LeftRightAlignment)(resources.GetObject("port.UpDownAlign")));
			this.port.Visible = ((bool)(resources.GetObject("port.Visible")));
			this.port.ValueChanged += new System.EventHandler(this.port_ValueChanged);
			// 
			// useProxy
			// 
			this.useProxy.AccessibleDescription = resources.GetString("useProxy.AccessibleDescription");
			this.useProxy.AccessibleName = resources.GetString("useProxy.AccessibleName");
			this.useProxy.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("useProxy.Anchor")));
			this.useProxy.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("useProxy.Appearance")));
			this.useProxy.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("useProxy.BackgroundImage")));
			this.useProxy.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("useProxy.CheckAlign")));
			this.useProxy.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("useProxy.Dock")));
			this.useProxy.Enabled = ((bool)(resources.GetObject("useProxy.Enabled")));
			this.useProxy.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("useProxy.FlatStyle")));
			this.useProxy.Font = ((System.Drawing.Font)(resources.GetObject("useProxy.Font")));
			this.useProxy.Image = ((System.Drawing.Image)(resources.GetObject("useProxy.Image")));
			this.useProxy.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("useProxy.ImageAlign")));
			this.useProxy.ImageIndex = ((int)(resources.GetObject("useProxy.ImageIndex")));
			this.useProxy.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("useProxy.ImeMode")));
			this.useProxy.Location = ((System.Drawing.Point)(resources.GetObject("useProxy.Location")));
			this.useProxy.Name = "useProxy";
			this.useProxy.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("useProxy.RightToLeft")));
			this.useProxy.Size = ((System.Drawing.Size)(resources.GetObject("useProxy.Size")));
			this.useProxy.TabIndex = ((int)(resources.GetObject("useProxy.TabIndex")));
			this.useProxy.Text = resources.GetString("useProxy.Text");
			this.useProxy.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("useProxy.TextAlign")));
			this.useProxy.Visible = ((bool)(resources.GetObject("useProxy.Visible")));
			this.useProxy.CheckedChanged += new System.EventHandler(this.useProxy_CheckedChanged);
			// 
			// label7
			// 
			this.label7.AccessibleDescription = resources.GetString("label7.AccessibleDescription");
			this.label7.AccessibleName = resources.GetString("label7.AccessibleName");
			this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label7.Anchor")));
			this.label7.AutoSize = ((bool)(resources.GetObject("label7.AutoSize")));
			this.label7.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label7.Dock")));
			this.label7.Enabled = ((bool)(resources.GetObject("label7.Enabled")));
			this.label7.Font = ((System.Drawing.Font)(resources.GetObject("label7.Font")));
			this.label7.Image = ((System.Drawing.Image)(resources.GetObject("label7.Image")));
			this.label7.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label7.ImageAlign")));
			this.label7.ImageIndex = ((int)(resources.GetObject("label7.ImageIndex")));
			this.label7.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label7.ImeMode")));
			this.label7.Location = ((System.Drawing.Point)(resources.GetObject("label7.Location")));
			this.label7.Name = "label7";
			this.label7.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label7.RightToLeft")));
			this.label7.Size = ((System.Drawing.Size)(resources.GetObject("label7.Size")));
			this.label7.TabIndex = ((int)(resources.GetObject("label7.TabIndex")));
			this.label7.Text = resources.GetString("label7.Text");
			this.label7.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label7.TextAlign")));
			this.label7.Visible = ((bool)(resources.GetObject("label7.Visible")));
			// 
			// label4
			// 
			this.label4.AccessibleDescription = resources.GetString("label4.AccessibleDescription");
			this.label4.AccessibleName = resources.GetString("label4.AccessibleName");
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label4.Anchor")));
			this.label4.AutoSize = ((bool)(resources.GetObject("label4.AutoSize")));
			this.label4.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label4.Dock")));
			this.label4.Enabled = ((bool)(resources.GetObject("label4.Enabled")));
			this.label4.Font = ((System.Drawing.Font)(resources.GetObject("label4.Font")));
			this.label4.Image = ((System.Drawing.Image)(resources.GetObject("label4.Image")));
			this.label4.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label4.ImageAlign")));
			this.label4.ImageIndex = ((int)(resources.GetObject("label4.ImageIndex")));
			this.label4.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label4.ImeMode")));
			this.label4.Location = ((System.Drawing.Point)(resources.GetObject("label4.Location")));
			this.label4.Name = "label4";
			this.label4.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label4.RightToLeft")));
			this.label4.Size = ((System.Drawing.Size)(resources.GetObject("label4.Size")));
			this.label4.TabIndex = ((int)(resources.GetObject("label4.TabIndex")));
			this.label4.Text = resources.GetString("label4.Text");
			this.label4.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label4.TextAlign")));
			this.label4.Visible = ((bool)(resources.GetObject("label4.Visible")));
			// 
			// tabPage3
			// 
			this.tabPage3.AccessibleDescription = resources.GetString("tabPage3.AccessibleDescription");
			this.tabPage3.AccessibleName = resources.GetString("tabPage3.AccessibleName");
			this.tabPage3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabPage3.Anchor")));
			this.tabPage3.AutoScroll = ((bool)(resources.GetObject("tabPage3.AutoScroll")));
			this.tabPage3.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabPage3.AutoScrollMargin")));
			this.tabPage3.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabPage3.AutoScrollMinSize")));
			this.tabPage3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabPage3.BackgroundImage")));
			this.tabPage3.Controls.Add(this.clearLog);
			this.tabPage3.Controls.Add(this.saveLog);
			this.tabPage3.Controls.Add(this.log);
			this.tabPage3.Controls.Add(this.label6);
			this.tabPage3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabPage3.Dock")));
			this.tabPage3.Enabled = ((bool)(resources.GetObject("tabPage3.Enabled")));
			this.tabPage3.Font = ((System.Drawing.Font)(resources.GetObject("tabPage3.Font")));
			this.tabPage3.ImageIndex = ((int)(resources.GetObject("tabPage3.ImageIndex")));
			this.tabPage3.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabPage3.ImeMode")));
			this.tabPage3.Location = ((System.Drawing.Point)(resources.GetObject("tabPage3.Location")));
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabPage3.RightToLeft")));
			this.tabPage3.Size = ((System.Drawing.Size)(resources.GetObject("tabPage3.Size")));
			this.tabPage3.TabIndex = ((int)(resources.GetObject("tabPage3.TabIndex")));
			this.tabPage3.Text = resources.GetString("tabPage3.Text");
			this.tabPage3.ToolTipText = resources.GetString("tabPage3.ToolTipText");
			this.tabPage3.Visible = ((bool)(resources.GetObject("tabPage3.Visible")));
			// 
			// clearLog
			// 
			this.clearLog.AccessibleDescription = resources.GetString("clearLog.AccessibleDescription");
			this.clearLog.AccessibleName = resources.GetString("clearLog.AccessibleName");
			this.clearLog.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("clearLog.Anchor")));
			this.clearLog.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("clearLog.BackgroundImage")));
			this.clearLog.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("clearLog.Dock")));
			this.clearLog.Enabled = ((bool)(resources.GetObject("clearLog.Enabled")));
			this.clearLog.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("clearLog.FlatStyle")));
			this.clearLog.Font = ((System.Drawing.Font)(resources.GetObject("clearLog.Font")));
			this.clearLog.Image = ((System.Drawing.Image)(resources.GetObject("clearLog.Image")));
			this.clearLog.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("clearLog.ImageAlign")));
			this.clearLog.ImageIndex = ((int)(resources.GetObject("clearLog.ImageIndex")));
			this.clearLog.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("clearLog.ImeMode")));
			this.clearLog.Location = ((System.Drawing.Point)(resources.GetObject("clearLog.Location")));
			this.clearLog.Name = "clearLog";
			this.clearLog.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("clearLog.RightToLeft")));
			this.clearLog.Size = ((System.Drawing.Size)(resources.GetObject("clearLog.Size")));
			this.clearLog.TabIndex = ((int)(resources.GetObject("clearLog.TabIndex")));
			this.clearLog.Text = resources.GetString("clearLog.Text");
			this.clearLog.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("clearLog.TextAlign")));
			this.clearLog.Visible = ((bool)(resources.GetObject("clearLog.Visible")));
			this.clearLog.Click += new System.EventHandler(this.clearLog_Click);
			// 
			// saveLog
			// 
			this.saveLog.AccessibleDescription = resources.GetString("saveLog.AccessibleDescription");
			this.saveLog.AccessibleName = resources.GetString("saveLog.AccessibleName");
			this.saveLog.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("saveLog.Anchor")));
			this.saveLog.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("saveLog.BackgroundImage")));
			this.saveLog.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("saveLog.Dock")));
			this.saveLog.Enabled = ((bool)(resources.GetObject("saveLog.Enabled")));
			this.saveLog.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("saveLog.FlatStyle")));
			this.saveLog.Font = ((System.Drawing.Font)(resources.GetObject("saveLog.Font")));
			this.saveLog.Image = ((System.Drawing.Image)(resources.GetObject("saveLog.Image")));
			this.saveLog.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("saveLog.ImageAlign")));
			this.saveLog.ImageIndex = ((int)(resources.GetObject("saveLog.ImageIndex")));
			this.saveLog.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("saveLog.ImeMode")));
			this.saveLog.Location = ((System.Drawing.Point)(resources.GetObject("saveLog.Location")));
			this.saveLog.Name = "saveLog";
			this.saveLog.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("saveLog.RightToLeft")));
			this.saveLog.Size = ((System.Drawing.Size)(resources.GetObject("saveLog.Size")));
			this.saveLog.TabIndex = ((int)(resources.GetObject("saveLog.TabIndex")));
			this.saveLog.Text = resources.GetString("saveLog.Text");
			this.saveLog.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("saveLog.TextAlign")));
			this.saveLog.Visible = ((bool)(resources.GetObject("saveLog.Visible")));
			this.saveLog.Click += new System.EventHandler(this.saveLog_Click);
			// 
			// log
			// 
			this.log.AccessibleDescription = resources.GetString("log.AccessibleDescription");
			this.log.AccessibleName = resources.GetString("log.AccessibleName");
			this.log.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("log.Anchor")));
			this.log.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("log.BackgroundImage")));
			this.log.ColumnWidth = ((int)(resources.GetObject("log.ColumnWidth")));
			this.log.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("log.Dock")));
			this.log.Enabled = ((bool)(resources.GetObject("log.Enabled")));
			this.log.Font = ((System.Drawing.Font)(resources.GetObject("log.Font")));
			this.log.HorizontalExtent = ((int)(resources.GetObject("log.HorizontalExtent")));
			this.log.HorizontalScrollbar = ((bool)(resources.GetObject("log.HorizontalScrollbar")));
			this.log.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("log.ImeMode")));
			this.log.IntegralHeight = ((bool)(resources.GetObject("log.IntegralHeight")));
			this.log.ItemHeight = ((int)(resources.GetObject("log.ItemHeight")));
			this.log.Location = ((System.Drawing.Point)(resources.GetObject("log.Location")));
			this.log.Name = "log";
			this.log.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("log.RightToLeft")));
			this.log.ScrollAlwaysVisible = ((bool)(resources.GetObject("log.ScrollAlwaysVisible")));
			this.log.Size = ((System.Drawing.Size)(resources.GetObject("log.Size")));
			this.log.TabIndex = ((int)(resources.GetObject("log.TabIndex")));
			this.log.Visible = ((bool)(resources.GetObject("log.Visible")));
			// 
			// label6
			// 
			this.label6.AccessibleDescription = resources.GetString("label6.AccessibleDescription");
			this.label6.AccessibleName = resources.GetString("label6.AccessibleName");
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label6.Anchor")));
			this.label6.AutoSize = ((bool)(resources.GetObject("label6.AutoSize")));
			this.label6.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label6.Dock")));
			this.label6.Enabled = ((bool)(resources.GetObject("label6.Enabled")));
			this.label6.Font = ((System.Drawing.Font)(resources.GetObject("label6.Font")));
			this.label6.Image = ((System.Drawing.Image)(resources.GetObject("label6.Image")));
			this.label6.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label6.ImageAlign")));
			this.label6.ImageIndex = ((int)(resources.GetObject("label6.ImageIndex")));
			this.label6.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label6.ImeMode")));
			this.label6.Location = ((System.Drawing.Point)(resources.GetObject("label6.Location")));
			this.label6.Name = "label6";
			this.label6.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label6.RightToLeft")));
			this.label6.Size = ((System.Drawing.Size)(resources.GetObject("label6.Size")));
			this.label6.TabIndex = ((int)(resources.GetObject("label6.TabIndex")));
			this.label6.Text = resources.GetString("label6.Text");
			this.label6.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label6.TextAlign")));
			this.label6.Visible = ((bool)(resources.GetObject("label6.Visible")));
			// 
			// tabPage5
			// 
			this.tabPage5.AccessibleDescription = resources.GetString("tabPage5.AccessibleDescription");
			this.tabPage5.AccessibleName = resources.GetString("tabPage5.AccessibleName");
			this.tabPage5.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabPage5.Anchor")));
			this.tabPage5.AutoScroll = ((bool)(resources.GetObject("tabPage5.AutoScroll")));
			this.tabPage5.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabPage5.AutoScrollMargin")));
			this.tabPage5.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabPage5.AutoScrollMinSize")));
			this.tabPage5.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabPage5.BackgroundImage")));
			this.tabPage5.Controls.Add(this.accounts);
			this.tabPage5.Controls.Add(this.details);
			this.tabPage5.Controls.Add(this.groupBox2);
			this.tabPage5.Controls.Add(this.removeAccount);
			this.tabPage5.Controls.Add(this.addAccount);
			this.tabPage5.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabPage5.Dock")));
			this.tabPage5.Enabled = ((bool)(resources.GetObject("tabPage5.Enabled")));
			this.tabPage5.Font = ((System.Drawing.Font)(resources.GetObject("tabPage5.Font")));
			this.tabPage5.ImageIndex = ((int)(resources.GetObject("tabPage5.ImageIndex")));
			this.tabPage5.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabPage5.ImeMode")));
			this.tabPage5.Location = ((System.Drawing.Point)(resources.GetObject("tabPage5.Location")));
			this.tabPage5.Name = "tabPage5";
			this.tabPage5.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabPage5.RightToLeft")));
			this.tabPage5.Size = ((System.Drawing.Size)(resources.GetObject("tabPage5.Size")));
			this.tabPage5.TabIndex = ((int)(resources.GetObject("tabPage5.TabIndex")));
			this.tabPage5.Text = resources.GetString("tabPage5.Text");
			this.tabPage5.ToolTipText = resources.GetString("tabPage5.ToolTipText");
			this.tabPage5.Visible = ((bool)(resources.GetObject("tabPage5.Visible")));
			// 
			// accounts
			// 
			this.accounts.AccessibleDescription = resources.GetString("accounts.AccessibleDescription");
			this.accounts.AccessibleName = resources.GetString("accounts.AccessibleName");
			this.accounts.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("accounts.Alignment")));
			this.accounts.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("accounts.Anchor")));
			this.accounts.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("accounts.BackgroundImage")));
			this.accounts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					   this.columnHeader2,
																					   this.columnHeader3});
			this.accounts.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("accounts.Dock")));
			this.accounts.Enabled = ((bool)(resources.GetObject("accounts.Enabled")));
			this.accounts.Font = ((System.Drawing.Font)(resources.GetObject("accounts.Font")));
			this.accounts.FullRowSelect = true;
			this.accounts.HideSelection = false;
			this.accounts.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("accounts.ImeMode")));
			this.accounts.LabelWrap = ((bool)(resources.GetObject("accounts.LabelWrap")));
			this.accounts.Location = ((System.Drawing.Point)(resources.GetObject("accounts.Location")));
			this.accounts.MultiSelect = false;
			this.accounts.Name = "accounts";
			this.accounts.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("accounts.RightToLeft")));
			this.accounts.Size = ((System.Drawing.Size)(resources.GetObject("accounts.Size")));
			this.accounts.TabIndex = ((int)(resources.GetObject("accounts.TabIndex")));
			this.accounts.Text = resources.GetString("accounts.Text");
			this.accounts.View = System.Windows.Forms.View.Details;
			this.accounts.Visible = ((bool)(resources.GetObject("accounts.Visible")));
			this.accounts.SelectedIndexChanged += new System.EventHandler(this.accounts_SelectedIndexChanged);
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = resources.GetString("columnHeader2.Text");
			this.columnHeader2.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader2.TextAlign")));
			this.columnHeader2.Width = ((int)(resources.GetObject("columnHeader2.Width")));
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = resources.GetString("columnHeader3.Text");
			this.columnHeader3.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader3.TextAlign")));
			this.columnHeader3.Width = ((int)(resources.GetObject("columnHeader3.Width")));
			// 
			// details
			// 
			this.details.AccessibleDescription = resources.GetString("details.AccessibleDescription");
			this.details.AccessibleName = resources.GetString("details.AccessibleName");
			this.details.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("details.Anchor")));
			this.details.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("details.BackgroundImage")));
			this.details.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("details.Dock")));
			this.details.Enabled = ((bool)(resources.GetObject("details.Enabled")));
			this.details.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("details.FlatStyle")));
			this.details.Font = ((System.Drawing.Font)(resources.GetObject("details.Font")));
			this.details.Image = ((System.Drawing.Image)(resources.GetObject("details.Image")));
			this.details.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("details.ImageAlign")));
			this.details.ImageIndex = ((int)(resources.GetObject("details.ImageIndex")));
			this.details.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("details.ImeMode")));
			this.details.Location = ((System.Drawing.Point)(resources.GetObject("details.Location")));
			this.details.Name = "details";
			this.details.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("details.RightToLeft")));
			this.details.Size = ((System.Drawing.Size)(resources.GetObject("details.Size")));
			this.details.TabIndex = ((int)(resources.GetObject("details.TabIndex")));
			this.details.Text = resources.GetString("details.Text");
			this.details.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("details.TextAlign")));
			this.details.Visible = ((bool)(resources.GetObject("details.Visible")));
			this.details.Click += new System.EventHandler(this.details_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.AccessibleDescription = resources.GetString("groupBox2.AccessibleDescription");
			this.groupBox2.AccessibleName = resources.GetString("groupBox2.AccessibleName");
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox2.Anchor")));
			this.groupBox2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox2.BackgroundImage")));
			this.groupBox2.Controls.Add(this.connect);
			this.groupBox2.Controls.Add(this.password);
			this.groupBox2.Controls.Add(this.server);
			this.groupBox2.Controls.Add(this.userName);
			this.groupBox2.Controls.Add(this.label9);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.advanced);
			this.groupBox2.Controls.Add(this.autoLogin);
			this.groupBox2.Controls.Add(this.rememberPassword);
			this.groupBox2.Controls.Add(this.label10);
			this.groupBox2.Controls.Add(this.defaultServer);
			this.groupBox2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBox2.Dock")));
			this.groupBox2.Enabled = ((bool)(resources.GetObject("groupBox2.Enabled")));
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Font = ((System.Drawing.Font)(resources.GetObject("groupBox2.Font")));
			this.groupBox2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBox2.ImeMode")));
			this.groupBox2.Location = ((System.Drawing.Point)(resources.GetObject("groupBox2.Location")));
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBox2.RightToLeft")));
			this.groupBox2.Size = ((System.Drawing.Size)(resources.GetObject("groupBox2.Size")));
			this.groupBox2.TabIndex = ((int)(resources.GetObject("groupBox2.TabIndex")));
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = resources.GetString("groupBox2.Text");
			this.groupBox2.Visible = ((bool)(resources.GetObject("groupBox2.Visible")));
			// 
			// password
			// 
			this.password.AccessibleDescription = resources.GetString("password.AccessibleDescription");
			this.password.AccessibleName = resources.GetString("password.AccessibleName");
			this.password.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("password.Anchor")));
			this.password.AutoSize = ((bool)(resources.GetObject("password.AutoSize")));
			this.password.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("password.BackgroundImage")));
			this.password.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("password.Dock")));
			this.password.Enabled = ((bool)(resources.GetObject("password.Enabled")));
			this.password.Font = ((System.Drawing.Font)(resources.GetObject("password.Font")));
			this.password.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("password.ImeMode")));
			this.password.Location = ((System.Drawing.Point)(resources.GetObject("password.Location")));
			this.password.MaxLength = ((int)(resources.GetObject("password.MaxLength")));
			this.password.Multiline = ((bool)(resources.GetObject("password.Multiline")));
			this.password.Name = "password";
			this.password.PasswordChar = ((char)(resources.GetObject("password.PasswordChar")));
			this.password.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("password.RightToLeft")));
			this.password.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("password.ScrollBars")));
			this.password.Size = ((System.Drawing.Size)(resources.GetObject("password.Size")));
			this.password.TabIndex = ((int)(resources.GetObject("password.TabIndex")));
			this.password.Text = resources.GetString("password.Text");
			this.password.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("password.TextAlign")));
			this.password.Visible = ((bool)(resources.GetObject("password.Visible")));
			this.password.WordWrap = ((bool)(resources.GetObject("password.WordWrap")));
			// 
			// server
			// 
			this.server.AccessibleDescription = resources.GetString("server.AccessibleDescription");
			this.server.AccessibleName = resources.GetString("server.AccessibleName");
			this.server.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("server.Anchor")));
			this.server.AutoSize = ((bool)(resources.GetObject("server.AutoSize")));
			this.server.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("server.BackgroundImage")));
			this.server.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("server.Dock")));
			this.server.Enabled = ((bool)(resources.GetObject("server.Enabled")));
			this.server.Font = ((System.Drawing.Font)(resources.GetObject("server.Font")));
			this.server.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("server.ImeMode")));
			this.server.Location = ((System.Drawing.Point)(resources.GetObject("server.Location")));
			this.server.MaxLength = ((int)(resources.GetObject("server.MaxLength")));
			this.server.Multiline = ((bool)(resources.GetObject("server.Multiline")));
			this.server.Name = "server";
			this.server.PasswordChar = ((char)(resources.GetObject("server.PasswordChar")));
			this.server.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("server.RightToLeft")));
			this.server.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("server.ScrollBars")));
			this.server.Size = ((System.Drawing.Size)(resources.GetObject("server.Size")));
			this.server.TabIndex = ((int)(resources.GetObject("server.TabIndex")));
			this.server.Text = resources.GetString("server.Text");
			this.server.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("server.TextAlign")));
			this.server.Visible = ((bool)(resources.GetObject("server.Visible")));
			this.server.WordWrap = ((bool)(resources.GetObject("server.WordWrap")));
			this.server.TextChanged += new System.EventHandler(this.server_TextChanged);
			// 
			// userName
			// 
			this.userName.AccessibleDescription = resources.GetString("userName.AccessibleDescription");
			this.userName.AccessibleName = resources.GetString("userName.AccessibleName");
			this.userName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("userName.Anchor")));
			this.userName.AutoSize = ((bool)(resources.GetObject("userName.AutoSize")));
			this.userName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("userName.BackgroundImage")));
			this.userName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("userName.Dock")));
			this.userName.Enabled = ((bool)(resources.GetObject("userName.Enabled")));
			this.userName.Font = ((System.Drawing.Font)(resources.GetObject("userName.Font")));
			this.userName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("userName.ImeMode")));
			this.userName.Location = ((System.Drawing.Point)(resources.GetObject("userName.Location")));
			this.userName.MaxLength = ((int)(resources.GetObject("userName.MaxLength")));
			this.userName.Multiline = ((bool)(resources.GetObject("userName.Multiline")));
			this.userName.Name = "userName";
			this.userName.PasswordChar = ((char)(resources.GetObject("userName.PasswordChar")));
			this.userName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("userName.RightToLeft")));
			this.userName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("userName.ScrollBars")));
			this.userName.Size = ((System.Drawing.Size)(resources.GetObject("userName.Size")));
			this.userName.TabIndex = ((int)(resources.GetObject("userName.TabIndex")));
			this.userName.Text = resources.GetString("userName.Text");
			this.userName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("userName.TextAlign")));
			this.userName.Visible = ((bool)(resources.GetObject("userName.Visible")));
			this.userName.WordWrap = ((bool)(resources.GetObject("userName.WordWrap")));
			this.userName.TextChanged += new System.EventHandler(this.userName_TextChanged);
			// 
			// label9
			// 
			this.label9.AccessibleDescription = resources.GetString("label9.AccessibleDescription");
			this.label9.AccessibleName = resources.GetString("label9.AccessibleName");
			this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label9.Anchor")));
			this.label9.AutoSize = ((bool)(resources.GetObject("label9.AutoSize")));
			this.label9.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label9.Dock")));
			this.label9.Enabled = ((bool)(resources.GetObject("label9.Enabled")));
			this.label9.Font = ((System.Drawing.Font)(resources.GetObject("label9.Font")));
			this.label9.Image = ((System.Drawing.Image)(resources.GetObject("label9.Image")));
			this.label9.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label9.ImageAlign")));
			this.label9.ImageIndex = ((int)(resources.GetObject("label9.ImageIndex")));
			this.label9.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label9.ImeMode")));
			this.label9.Location = ((System.Drawing.Point)(resources.GetObject("label9.Location")));
			this.label9.Name = "label9";
			this.label9.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label9.RightToLeft")));
			this.label9.Size = ((System.Drawing.Size)(resources.GetObject("label9.Size")));
			this.label9.TabIndex = ((int)(resources.GetObject("label9.TabIndex")));
			this.label9.Text = resources.GetString("label9.Text");
			this.label9.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label9.TextAlign")));
			this.label9.Visible = ((bool)(resources.GetObject("label9.Visible")));
			// 
			// label5
			// 
			this.label5.AccessibleDescription = resources.GetString("label5.AccessibleDescription");
			this.label5.AccessibleName = resources.GetString("label5.AccessibleName");
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label5.Anchor")));
			this.label5.AutoSize = ((bool)(resources.GetObject("label5.AutoSize")));
			this.label5.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label5.Dock")));
			this.label5.Enabled = ((bool)(resources.GetObject("label5.Enabled")));
			this.label5.Font = ((System.Drawing.Font)(resources.GetObject("label5.Font")));
			this.label5.Image = ((System.Drawing.Image)(resources.GetObject("label5.Image")));
			this.label5.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label5.ImageAlign")));
			this.label5.ImageIndex = ((int)(resources.GetObject("label5.ImageIndex")));
			this.label5.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label5.ImeMode")));
			this.label5.Location = ((System.Drawing.Point)(resources.GetObject("label5.Location")));
			this.label5.Name = "label5";
			this.label5.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label5.RightToLeft")));
			this.label5.Size = ((System.Drawing.Size)(resources.GetObject("label5.Size")));
			this.label5.TabIndex = ((int)(resources.GetObject("label5.TabIndex")));
			this.label5.Text = resources.GetString("label5.Text");
			this.label5.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label5.TextAlign")));
			this.label5.Visible = ((bool)(resources.GetObject("label5.Visible")));
			// 
			// advanced
			// 
			this.advanced.AccessibleDescription = resources.GetString("advanced.AccessibleDescription");
			this.advanced.AccessibleName = resources.GetString("advanced.AccessibleName");
			this.advanced.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("advanced.Anchor")));
			this.advanced.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("advanced.BackgroundImage")));
			this.advanced.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("advanced.Dock")));
			this.advanced.Enabled = ((bool)(resources.GetObject("advanced.Enabled")));
			this.advanced.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("advanced.FlatStyle")));
			this.advanced.Font = ((System.Drawing.Font)(resources.GetObject("advanced.Font")));
			this.advanced.Image = ((System.Drawing.Image)(resources.GetObject("advanced.Image")));
			this.advanced.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("advanced.ImageAlign")));
			this.advanced.ImageIndex = ((int)(resources.GetObject("advanced.ImageIndex")));
			this.advanced.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("advanced.ImeMode")));
			this.advanced.Location = ((System.Drawing.Point)(resources.GetObject("advanced.Location")));
			this.advanced.Name = "advanced";
			this.advanced.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("advanced.RightToLeft")));
			this.advanced.Size = ((System.Drawing.Size)(resources.GetObject("advanced.Size")));
			this.advanced.TabIndex = ((int)(resources.GetObject("advanced.TabIndex")));
			this.advanced.Text = resources.GetString("advanced.Text");
			this.advanced.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("advanced.TextAlign")));
			this.advanced.Visible = ((bool)(resources.GetObject("advanced.Visible")));
			// 
			// autoLogin
			// 
			this.autoLogin.AccessibleDescription = resources.GetString("autoLogin.AccessibleDescription");
			this.autoLogin.AccessibleName = resources.GetString("autoLogin.AccessibleName");
			this.autoLogin.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("autoLogin.Anchor")));
			this.autoLogin.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("autoLogin.Appearance")));
			this.autoLogin.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("autoLogin.BackgroundImage")));
			this.autoLogin.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoLogin.CheckAlign")));
			this.autoLogin.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("autoLogin.Dock")));
			this.autoLogin.Enabled = ((bool)(resources.GetObject("autoLogin.Enabled")));
			this.autoLogin.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("autoLogin.FlatStyle")));
			this.autoLogin.Font = ((System.Drawing.Font)(resources.GetObject("autoLogin.Font")));
			this.autoLogin.Image = ((System.Drawing.Image)(resources.GetObject("autoLogin.Image")));
			this.autoLogin.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoLogin.ImageAlign")));
			this.autoLogin.ImageIndex = ((int)(resources.GetObject("autoLogin.ImageIndex")));
			this.autoLogin.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("autoLogin.ImeMode")));
			this.autoLogin.Location = ((System.Drawing.Point)(resources.GetObject("autoLogin.Location")));
			this.autoLogin.Name = "autoLogin";
			this.autoLogin.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("autoLogin.RightToLeft")));
			this.autoLogin.Size = ((System.Drawing.Size)(resources.GetObject("autoLogin.Size")));
			this.autoLogin.TabIndex = ((int)(resources.GetObject("autoLogin.TabIndex")));
			this.autoLogin.Text = resources.GetString("autoLogin.Text");
			this.autoLogin.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoLogin.TextAlign")));
			this.autoLogin.Visible = ((bool)(resources.GetObject("autoLogin.Visible")));
			this.autoLogin.CheckedChanged += new System.EventHandler(this.autoLogin_CheckedChanged);
			// 
			// rememberPassword
			// 
			this.rememberPassword.AccessibleDescription = resources.GetString("rememberPassword.AccessibleDescription");
			this.rememberPassword.AccessibleName = resources.GetString("rememberPassword.AccessibleName");
			this.rememberPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("rememberPassword.Anchor")));
			this.rememberPassword.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("rememberPassword.Appearance")));
			this.rememberPassword.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("rememberPassword.BackgroundImage")));
			this.rememberPassword.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rememberPassword.CheckAlign")));
			this.rememberPassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("rememberPassword.Dock")));
			this.rememberPassword.Enabled = ((bool)(resources.GetObject("rememberPassword.Enabled")));
			this.rememberPassword.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("rememberPassword.FlatStyle")));
			this.rememberPassword.Font = ((System.Drawing.Font)(resources.GetObject("rememberPassword.Font")));
			this.rememberPassword.Image = ((System.Drawing.Image)(resources.GetObject("rememberPassword.Image")));
			this.rememberPassword.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rememberPassword.ImageAlign")));
			this.rememberPassword.ImageIndex = ((int)(resources.GetObject("rememberPassword.ImageIndex")));
			this.rememberPassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("rememberPassword.ImeMode")));
			this.rememberPassword.Location = ((System.Drawing.Point)(resources.GetObject("rememberPassword.Location")));
			this.rememberPassword.Name = "rememberPassword";
			this.rememberPassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("rememberPassword.RightToLeft")));
			this.rememberPassword.Size = ((System.Drawing.Size)(resources.GetObject("rememberPassword.Size")));
			this.rememberPassword.TabIndex = ((int)(resources.GetObject("rememberPassword.TabIndex")));
			this.rememberPassword.Text = resources.GetString("rememberPassword.Text");
			this.rememberPassword.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rememberPassword.TextAlign")));
			this.rememberPassword.Visible = ((bool)(resources.GetObject("rememberPassword.Visible")));
			this.rememberPassword.CheckedChanged += new System.EventHandler(this.rememberPassword_CheckedChanged);
			// 
			// label10
			// 
			this.label10.AccessibleDescription = resources.GetString("label10.AccessibleDescription");
			this.label10.AccessibleName = resources.GetString("label10.AccessibleName");
			this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label10.Anchor")));
			this.label10.AutoSize = ((bool)(resources.GetObject("label10.AutoSize")));
			this.label10.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label10.Dock")));
			this.label10.Enabled = ((bool)(resources.GetObject("label10.Enabled")));
			this.label10.Font = ((System.Drawing.Font)(resources.GetObject("label10.Font")));
			this.label10.Image = ((System.Drawing.Image)(resources.GetObject("label10.Image")));
			this.label10.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label10.ImageAlign")));
			this.label10.ImageIndex = ((int)(resources.GetObject("label10.ImageIndex")));
			this.label10.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label10.ImeMode")));
			this.label10.Location = ((System.Drawing.Point)(resources.GetObject("label10.Location")));
			this.label10.Name = "label10";
			this.label10.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label10.RightToLeft")));
			this.label10.Size = ((System.Drawing.Size)(resources.GetObject("label10.Size")));
			this.label10.TabIndex = ((int)(resources.GetObject("label10.TabIndex")));
			this.label10.Text = resources.GetString("label10.Text");
			this.label10.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label10.TextAlign")));
			this.label10.Visible = ((bool)(resources.GetObject("label10.Visible")));
			// 
			// defaultServer
			// 
			this.defaultServer.AccessibleDescription = resources.GetString("defaultServer.AccessibleDescription");
			this.defaultServer.AccessibleName = resources.GetString("defaultServer.AccessibleName");
			this.defaultServer.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("defaultServer.Anchor")));
			this.defaultServer.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("defaultServer.Appearance")));
			this.defaultServer.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("defaultServer.BackgroundImage")));
			this.defaultServer.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("defaultServer.CheckAlign")));
			this.defaultServer.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("defaultServer.Dock")));
			this.defaultServer.Enabled = ((bool)(resources.GetObject("defaultServer.Enabled")));
			this.defaultServer.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("defaultServer.FlatStyle")));
			this.defaultServer.Font = ((System.Drawing.Font)(resources.GetObject("defaultServer.Font")));
			this.defaultServer.Image = ((System.Drawing.Image)(resources.GetObject("defaultServer.Image")));
			this.defaultServer.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("defaultServer.ImageAlign")));
			this.defaultServer.ImageIndex = ((int)(resources.GetObject("defaultServer.ImageIndex")));
			this.defaultServer.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("defaultServer.ImeMode")));
			this.defaultServer.Location = ((System.Drawing.Point)(resources.GetObject("defaultServer.Location")));
			this.defaultServer.Name = "defaultServer";
			this.defaultServer.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("defaultServer.RightToLeft")));
			this.defaultServer.Size = ((System.Drawing.Size)(resources.GetObject("defaultServer.Size")));
			this.defaultServer.TabIndex = ((int)(resources.GetObject("defaultServer.TabIndex")));
			this.defaultServer.Text = resources.GetString("defaultServer.Text");
			this.defaultServer.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("defaultServer.TextAlign")));
			this.defaultServer.Visible = ((bool)(resources.GetObject("defaultServer.Visible")));
			this.defaultServer.CheckedChanged += new System.EventHandler(this.defaultServer_CheckedChanged);
			// 
			// removeAccount
			// 
			this.removeAccount.AccessibleDescription = resources.GetString("removeAccount.AccessibleDescription");
			this.removeAccount.AccessibleName = resources.GetString("removeAccount.AccessibleName");
			this.removeAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("removeAccount.Anchor")));
			this.removeAccount.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("removeAccount.BackgroundImage")));
			this.removeAccount.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("removeAccount.Dock")));
			this.removeAccount.Enabled = ((bool)(resources.GetObject("removeAccount.Enabled")));
			this.removeAccount.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("removeAccount.FlatStyle")));
			this.removeAccount.Font = ((System.Drawing.Font)(resources.GetObject("removeAccount.Font")));
			this.removeAccount.Image = ((System.Drawing.Image)(resources.GetObject("removeAccount.Image")));
			this.removeAccount.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("removeAccount.ImageAlign")));
			this.removeAccount.ImageIndex = ((int)(resources.GetObject("removeAccount.ImageIndex")));
			this.removeAccount.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("removeAccount.ImeMode")));
			this.removeAccount.Location = ((System.Drawing.Point)(resources.GetObject("removeAccount.Location")));
			this.removeAccount.Name = "removeAccount";
			this.removeAccount.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("removeAccount.RightToLeft")));
			this.removeAccount.Size = ((System.Drawing.Size)(resources.GetObject("removeAccount.Size")));
			this.removeAccount.TabIndex = ((int)(resources.GetObject("removeAccount.TabIndex")));
			this.removeAccount.Text = resources.GetString("removeAccount.Text");
			this.removeAccount.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("removeAccount.TextAlign")));
			this.removeAccount.Visible = ((bool)(resources.GetObject("removeAccount.Visible")));
			this.removeAccount.Click += new System.EventHandler(this.removeAccount_Click);
			// 
			// addAccount
			// 
			this.addAccount.AccessibleDescription = resources.GetString("addAccount.AccessibleDescription");
			this.addAccount.AccessibleName = resources.GetString("addAccount.AccessibleName");
			this.addAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("addAccount.Anchor")));
			this.addAccount.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("addAccount.BackgroundImage")));
			this.addAccount.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("addAccount.Dock")));
			this.addAccount.Enabled = ((bool)(resources.GetObject("addAccount.Enabled")));
			this.addAccount.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("addAccount.FlatStyle")));
			this.addAccount.Font = ((System.Drawing.Font)(resources.GetObject("addAccount.Font")));
			this.addAccount.Image = ((System.Drawing.Image)(resources.GetObject("addAccount.Image")));
			this.addAccount.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("addAccount.ImageAlign")));
			this.addAccount.ImageIndex = ((int)(resources.GetObject("addAccount.ImageIndex")));
			this.addAccount.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("addAccount.ImeMode")));
			this.addAccount.Location = ((System.Drawing.Point)(resources.GetObject("addAccount.Location")));
			this.addAccount.Name = "addAccount";
			this.addAccount.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("addAccount.RightToLeft")));
			this.addAccount.Size = ((System.Drawing.Size)(resources.GetObject("addAccount.Size")));
			this.addAccount.TabIndex = ((int)(resources.GetObject("addAccount.TabIndex")));
			this.addAccount.Text = resources.GetString("addAccount.Text");
			this.addAccount.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("addAccount.TextAlign")));
			this.addAccount.Visible = ((bool)(resources.GetObject("addAccount.Visible")));
			this.addAccount.Click += new System.EventHandler(this.addAccount_Click);
			// 
			// banner
			// 
			this.banner.AccessibleDescription = resources.GetString("banner.AccessibleDescription");
			this.banner.AccessibleName = resources.GetString("banner.AccessibleName");
			this.banner.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("banner.Anchor")));
			this.banner.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("banner.BackgroundImage")));
			this.banner.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("banner.Dock")));
			this.banner.Enabled = ((bool)(resources.GetObject("banner.Enabled")));
			this.banner.Font = ((System.Drawing.Font)(resources.GetObject("banner.Font")));
			this.banner.Image = ((System.Drawing.Image)(resources.GetObject("banner.Image")));
			this.banner.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("banner.ImeMode")));
			this.banner.Location = ((System.Drawing.Point)(resources.GetObject("banner.Location")));
			this.banner.Name = "banner";
			this.banner.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("banner.RightToLeft")));
			this.banner.Size = ((System.Drawing.Size)(resources.GetObject("banner.Size")));
			this.banner.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("banner.SizeMode")));
			this.banner.TabIndex = ((int)(resources.GetObject("banner.TabIndex")));
			this.banner.TabStop = false;
			this.banner.Text = resources.GetString("banner.Text");
			this.banner.Visible = ((bool)(resources.GetObject("banner.Visible")));
			this.banner.Paint += new System.Windows.Forms.PaintEventHandler(this.banner_Paint);
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
			this.menuAction.Popup += new System.EventHandler(this.menuAction_Popup);
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
			this.menuView.Popup += new System.EventHandler(this.menuView_Popup);
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
			// connect
			// 
			this.connect.AccessibleDescription = resources.GetString("connect.AccessibleDescription");
			this.connect.AccessibleName = resources.GetString("connect.AccessibleName");
			this.connect.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("connect.Anchor")));
			this.connect.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("connect.BackgroundImage")));
			this.connect.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("connect.Dock")));
			this.connect.Enabled = ((bool)(resources.GetObject("connect.Enabled")));
			this.connect.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("connect.FlatStyle")));
			this.connect.Font = ((System.Drawing.Font)(resources.GetObject("connect.Font")));
			this.connect.Image = ((System.Drawing.Image)(resources.GetObject("connect.Image")));
			this.connect.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("connect.ImageAlign")));
			this.connect.ImageIndex = ((int)(resources.GetObject("connect.ImageIndex")));
			this.connect.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("connect.ImeMode")));
			this.connect.Location = ((System.Drawing.Point)(resources.GetObject("connect.Location")));
			this.connect.Name = "connect";
			this.connect.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("connect.RightToLeft")));
			this.connect.Size = ((System.Drawing.Size)(resources.GetObject("connect.Size")));
			this.connect.TabIndex = ((int)(resources.GetObject("connect.TabIndex")));
			this.connect.Text = resources.GetString("connect.Text");
			this.connect.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("connect.TextAlign")));
			this.connect.Visible = ((bool)(resources.GetObject("connect.Visible")));
			this.connect.Click += new System.EventHandler(this.connect_Click);
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
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.status);
			this.Controls.Add(this.banner);
			this.Controls.Add(this.tabControl1);
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
			((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).EndInit();
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.port)).EndInit();
			this.tabPage3.ResumeLayout(false);
			this.tabPage5.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
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
		/// Shows/hides the Enterprise tab.
		/// </summary>
		public bool ShowEnterpriseTab
		{
			set
			{
				if (value)
				{
					if (!tabControl1.Controls.Contains(tabPage5))
					{
						tabControl1.Controls.Add(this.tabPage5);
					}
				}
				else
				{
					if (tabControl1.Controls.Contains(tabPage5))
					{
						tabControl1.Controls.Remove(tabPage5);
					}
				}
			}
		}

		/// <summary>
		/// Gets/sets a value indicating if shared iFolder notifications should be displayed.
		/// </summary>
		public bool NotifyShareEnabled
		{
			get
			{
				int notify;
				try
				{
					// Create/open the iFolder key.
					RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

					// Get the notify share value ... default the value to 0 (enabled).
					notify = (int)regKey.GetValue(notifyShareDisabled, 0);
				}
				catch
				{
					return true;
				}

				return (notify == 0);
			}
			set
			{
				// Create/open the iFolder key.
				RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

				if (value)
				{
					// Delete the value.
					regKey.DeleteValue(notifyShareDisabled, false);
				}
				else
				{
					// Set the disable value.
					regKey.SetValue(notifyShareDisabled, 1);
				}
			}
		}

		/// <summary>
		/// Gets/sets a value indicating if iFolder collision notifications should be displayed.
		/// </summary>
		public bool NotifyCollisionEnabled
		{
			get
			{
				int notify;
				try
				{
					// Create/open the iFolder key.
					RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

					// Get the notify share value ... default the value to 0 (enabled).
					notify = (int)regKey.GetValue(notifyCollisionDisabled, 0);
				}
				catch
				{
					return true;
				}

				return (notify == 0);
			}
			set
			{
				// Create/open the iFolder key.
				RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

				if (value)
				{
					// Delete the value.
					regKey.DeleteValue(notifyCollisionDisabled, false);
				}
				else
				{
					// Set the disable value.
					regKey.SetValue(notifyCollisionDisabled, 1);
				}
			}
		}

		/// <summary>
		/// Gets/sets a value indicating if a notification should be displayed when a user joins an iFolder.
		/// </summary>
		public bool NotifyJoinEnabled
		{
			get
			{
				int notify;
				try
				{
					// Create/open the iFolder key.
					RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

					// Get the notify share value ... default the value to 0 (enabled).
					notify = (int)regKey.GetValue(notifyJoinDisabled, 0);
				}
				catch
				{
					return true;
				}

				return (notify == 0);
			}
			set
			{
				// Create/open the iFolder key.
				RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

				if (value)
				{
					// Delete the value.
					regKey.DeleteValue(notifyJoinDisabled, false);
				}
				else
				{
					// Set the disable value.
					regKey.SetValue(notifyJoinDisabled, 1);
				}
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Checks to see if auto-run is enabled for the application.
		/// </summary>
		/// <returns><b>True</b> if auto-run is enabled; otherwise, <b>false</b>.</returns>
		static public bool IsRunEnabled()
		{
			int run;

			try
			{
				// Open the iFolder key.
				RegistryKey regKey = Registry.CurrentUser.OpenSubKey(iFolderKey);

				// Get the autorun value ... default the value to 0 (enabled).
				run = (int)regKey.GetValue(iFolderRun, 0);
			}
			catch
			{
				return true;
			}

			return (run == 0);
		}

		/// <summary>
		/// Adds the specified domain to the dropdown lists.
		/// </summary>
		/// <param name="domainWeb">The DomainWeb object to add to the list.</param>
		public void AddDomainToList(DomainWeb domainWeb)
		{
			bool domainInList = false;

			foreach (ListViewItem lvi in accounts.Items)
			{
				Domain d = (Domain)lvi.Tag;

				if (d.ID.Equals(domainWeb.ID))
				{
					// The domain is already in the list.
					domainInList = true;
				}
			}

			if (!domainInList)
			{
				// Reset the current default domain if the added domain is set to be the default.
				if ((defaultDomain != null) && domainWeb.IsDefault)
				{
					defaultDomain.DomainWeb.IsDefault = false;
				}

				Domain domain = new Domain(domainWeb);
				servers2.Items.Add(domain);

				if (domainWeb.IsDefault)
				{
					defaultDomain = domain;
				}

				try
				{
					iFolderUser ifolderUser = ifWebService.GetiFolderUser(domainWeb.UserID);
					ListViewItem lvi = new ListViewItem(new string[] {ifolderUser.Name, domain.Name});
					lvi.Tag = domain;
					accounts.Items.Add(lvi);
				}
				catch (Exception ex)
				{
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readUserError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}
			}
		}

		/// <summary>
		/// Check the specified ID to see if it is the current user.
		/// </summary>
		/// <param name="userID">The ID of the user to check.</param>
		/// <returns><b>True</b> if the specified user ID is the current user; otherwise, <b>False</b>.</returns>
		public bool IsCurrentUser(string userID)
		{
			bool result = false;

			foreach (ListViewItem lvi in accounts.Items)
			{
				Domain d = (Domain)lvi.Tag;

				if (d.DomainWeb.UserID.Equals(userID))
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

			foreach (ListViewItem lvi in accounts.Items)
			{
				Domain d = (Domain)lvi.Tag;

				if (d.DomainWeb.POBoxID.Equals(poBoxID))
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
			Domain domain = (Domain)servers2.SelectedItem;
			if (domain.ShowAll || domain.ID.Equals(domainID))
			{
				result = true;
			}

			return result;
		}
		#endregion

		#region Private Methods
		private void syncCollection(CollectionSyncEventArgs syncEventArgs)
		{
			try
			{
				progressBar1.Visible = false;

				string message = null;
				switch (syncEventArgs.Action)
				{
					case Action.StartSync:
					{
						message = string.Format(resourceManager.GetString("synciFolder"), syncEventArgs.Name);
						status.Text = message;
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

							message = string.Format(resourceManager.GetString("syncComplete"), syncEventArgs.Name);
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

				// Add message to log.
				addMessageToLog(syncEventArgs.TimeStamp, message);
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

					// Add message to log.
					addMessageToLog(syncEventArgs.TimeStamp, status.Text);
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

		private void addMessageToLog(DateTime dateTime, string message)
		{
			if (message != null)
			{
				log.Items.Add(dateTime.ToString() + " " + message);
				log.SelectedIndex = log.Items.Count - 1;
				saveLog.Enabled = clearLog.Enabled = true;

				// This should only have to execute once.
				while (log.Items.Count > maxMessages)
				{
					log.Items.RemoveAt(0);
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

		/// <summary>
		/// Set the auto-run value in the Windows registery.
		/// </summary>
		/// <param name="disable"><b>True</b> will disable auto-run.</param>
		private void setAutoRunValue(bool disable)
		{
			// Open/create the iFolder key.
			RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

			if (disable)
			{
				// Set the disable value.
				regKey.SetValue(iFolderRun, 1);
			}
			else
			{
				// Delete the value.
				regKey.DeleteValue(iFolderRun, false);
			}
		}

		private void refreshiFolders(Domain domain)
		{
			Cursor.Current = Cursors.WaitCursor;

			iFolderView.Items.Clear();
			iFolderView.SelectedItems.Clear();

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
			servers2.Items.Clear();
			accounts.Items.Clear();
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
			// Load the application icon and banner image.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_loaded.ico"));
				this.banner.Image = Image.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-banner.png"));

				// TODO: need icons for the different states.
				//	- iFolder with conflicts.
				//	- iFolder that is available.
				//	- iFolder that has been requested.
				//	- iFolder that has been invited. (Invitation.ico?)
				this.iFolderView.SmallImageList = new ImageList();
				iFolderView.SmallImageList.Images.Add(new Icon(Path.Combine(Application.StartupPath, @"res\ifolder.ico")));
				iFolderView.SmallImageList.Images.Add(new Icon(Path.Combine(Application.StartupPath, @"res\serverifolder.ico")));
				iFolderView.SmallImageList.Images.Add(new Icon(Path.Combine(Application.StartupPath, @"res\ifolderconflict.ico")));
			}
			catch {} // Non-fatal ... just missing some graphics.

			initialBannerWidth = banner.Width;
			this.MinimumSize = this.Size;
		}

		private void GlobalProperties_VisibleChanged(object sender, System.EventArgs e)
		{
			if (this.Visible)
			{
				servers2.Items.Clear();

				// Add the wild-card domain.
				Domain domain = new Domain(resourceManager.GetString("showAll"));
				servers2.Items.Add(domain);
				servers2.SelectedItem = domain;

				accounts.Items.Clear();

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

				apply.Enabled = cancel.Enabled = false;

				// Update the auto start setting.
				autoStart.Checked = IsRunEnabled();

				notifyShared.Checked = NotifyShareEnabled;
				notifyCollisions.Checked = NotifyCollisionEnabled;
				notifyJoins.Checked = NotifyJoinEnabled;

				// Update the display confirmation setting.
				displayConfirmation.Checked = iFolderComponent.DisplayConfirmationEnabled;

				try
				{
					// Update the default sync interval setting.
					defaultInterval.Value = (decimal)ifWebService.GetDefaultSyncInterval();
					autoSync.Checked = defaultInterval.Value != System.Threading.Timeout.Infinite;
				}
				catch (Exception ex)
				{
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readSyncError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}

				iFolderSettings ifSettings = null;

				try
				{
					ifSettings = ifWebService.GetSettings();

					// Update the proxy settings.
					useProxy.Checked = ifSettings.UseProxy;
					proxy.Text = ifSettings.ProxyHost;
					port.Value = (decimal)ifSettings.ProxyPort;
				}
				catch (Exception ex)
				{
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readProxyError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}

//				updateEnterpriseData();

				if ((ifSettings != null) && ifSettings.HaveEnterprise)
				{
					ShowEnterpriseTab = true;
				}

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

		private void menuAction_Popup(object sender, System.EventArgs e)
		{
			menuActionOpen.Enabled = menuActionShare.Enabled = menuActionSync.Enabled = 
				menuActionRevert.Enabled = menuActionProperties.Enabled = 
				(iFolderView.SelectedItems.Count == 1) && !((iFolderWeb)iFolderView.SelectedItems[0].Tag).IsSubscription;

			menuActionCreate.Enabled = tabControl1.SelectedIndex == 0;

			// Enable/disable resolve menu item.
			menuActionResolve.Visible = (iFolderView.SelectedItems.Count == 1) && ((iFolderWeb)iFolderView.SelectedItems[0].Tag).HasConflicts;

			/*menuActionAccept.Visible = menuActionRemove.Visible = menuActionSeparator2.Visible = 
				(iFolderView.SelectedItems.Count == 1) && ((iFolderWeb)iFolderView.SelectedItems[0].Tag).IsSubscription;*/
		}

		private void menuView_Popup(object sender, System.EventArgs e)
		{
			menuViewRefresh.Enabled = tabControl1.SelectedTab.Equals(tabPage1);
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
				refreshiFolders((Domain)servers2.SelectedItem);
			}
		}

		private void banner_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			// If the banner is wider than it's initial width, fill it in with white.
			if (banner.Width > initialBannerWidth)
			{
				SolidBrush brush = new SolidBrush(Color.White);
				e.Graphics.FillRectangle(brush, initialBannerWidth, 0, banner.Width - initialBannerWidth, banner.Height);
			}
		}

		#region iFolders Tab
		private void servers2_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			refreshiFolders((Domain)servers2.SelectedItem);
		}

		private void contextMenu1_Popup(object sender, System.EventArgs e)
		{
			menuShare.Visible = menuProperties.Visible = menuRevert.Visible = menuSeparator1.Visible = 
				menuSeparator2.Visible = menuSyncNow.Visible = menuOpen.Visible = 
				(iFolderView.SelectedItems.Count == 1) && !((iFolderWeb)iFolderView.SelectedItems[0].Tag).IsSubscription;

			menuResolve.Visible = (iFolderView.SelectedItems.Count == 1) && ((iFolderWeb)iFolderView.SelectedItems[0].Tag).HasConflicts;
			menuRefresh.Visible = menuCreate.Visible = iFolderView.SelectedItems.Count == 0;

			// Display the accept menu item if the selected item is a subscription with state "Available"
			menuAccept.Visible = 
				(iFolderView.SelectedItems.Count == 1) && 
				((iFolderWeb)iFolderView.SelectedItems[0].Tag).IsSubscription &&
				((iFolderWeb)iFolderView.SelectedItems[0].Tag).State.Equals("Available");

			// Display the decline menu item if the selected item is a subscription with state "Available" and from someone else.
			menuRemove.Visible = 
				(iFolderView.SelectedItems.Count == 1) && 
				(!((iFolderWeb)iFolderView.SelectedItems[0].Tag).IsSubscription ||
				((iFolderWeb)iFolderView.SelectedItems[0].Tag).State.Equals("Available"));
			
			if (menuRemove.Visible)
			{
				menuRemove.Text = IsCurrentUser(((iFolderWeb)iFolderView.SelectedItems[0].Tag).OwnerID) ? resourceManager.GetString("deleteAction") : resourceManager.GetString("menuRemove.Text");
			}
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
			createiFolder.Servers = accounts.Items;
			Domain selectedDomain = (Domain)servers2.SelectedItem;
			createiFolder.SelectedDomain = selectedDomain.ShowAll ? defaultDomain : selectedDomain;
			createiFolder.iFolderWebService = ifWebService;

			if ((DialogResult.OK == createiFolder.ShowDialog()) && iFolderComponent.DisplayConfirmationEnabled)
			{
				new iFolderComponent().NewiFolderWizard(Application.StartupPath, createiFolder.iFolderPath);
			}
		}

		private void menuRefresh_Click(object sender, System.EventArgs e)
		{
			refreshiFolders((Domain)servers2.SelectedItem);
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
		#endregion

		#region Log Tab
		private void saveLog_Click(object sender, System.EventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				StreamWriter streamWriter = File.CreateText(saveFileDialog.FileName);
				foreach (string s in log.Items)
				{
					streamWriter.WriteLine(s);
				}

				streamWriter.Flush();
				streamWriter.Close();
			}
		}

		private void clearLog_Click(object sender, System.EventArgs e)
		{
			log.Items.Clear();

			log.Items.Add(DateTime.Now.ToString() + " " + resourceManager.GetString("logEntriesCleared"));

			saveLog.Enabled = clearLog.Enabled = false;
		}
		#endregion

		#region Preferences Tab
		private void apply_Click(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			// Check and update auto start setting.
			if (autoStart.Checked != IsRunEnabled())
			{
				setAutoRunValue(!autoStart.Checked);
			}

			NotifyShareEnabled = notifyShared.Checked;
			NotifyCollisionEnabled = notifyCollisions.Checked;
			NotifyJoinEnabled = notifyJoins.Checked;

			// Check and update display confirmation setting.
			iFolderComponent.DisplayConfirmationEnabled = displayConfirmation.Checked;

			try
			{
				// Check and update default sync interval.
				int currentInterval = ifWebService.GetDefaultSyncInterval();
				if ((defaultInterval.Value != (decimal)currentInterval) ||
					(autoSync.Checked != (currentInterval != System.Threading.Timeout.Infinite)))
				{
					try
					{
						// Save the default sync interval.
						ifWebService.SetDefaultSyncInterval(autoSync.Checked ? (int)defaultInterval.Value : System.Threading.Timeout.Infinite);
					}
					catch (Exception ex)
					{
						Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("saveSyncError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
					}
				}
			}
			catch (Exception ex)
			{
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readSyncError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}

			try
			{
				iFolderSettings ifSettings = ifWebService.GetSettings();

				// Check and update proxy settings.
				if ((useProxy.Checked != ifSettings.UseProxy) ||
					!proxy.Text.Equals(ifSettings.ProxyHost) ||
					(port.Value != (decimal)ifSettings.ProxyPort))
				{
					try
					{
						if (useProxy.Checked)
						{
							ifWebService.SetupProxy(proxy.Text, (int)port.Value);
						}
						else
						{
							ifWebService.RemoveProxy();
						}
					}
					catch (Exception ex)
					{
						Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("saveProxyError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
					}
				}
			}
			catch (Exception ex)
			{
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readiFolderSettingsError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}

			Cursor.Current = Cursors.Default;
			apply.Enabled = cancel.Enabled = false;
		}

		private void cancel_Click(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			// Reset the auto start setting.
			autoStart.Checked = IsRunEnabled();

			notifyShared.Checked = NotifyShareEnabled;
			notifyCollisions.Checked = NotifyCollisionEnabled;
			notifyJoins.Checked = NotifyJoinEnabled;

			// Reset the display confirmation setting.
			displayConfirmation.Checked = iFolderComponent.DisplayConfirmationEnabled;

			// Reset the default sync interval.
			defaultInterval.Value = (decimal)ifWebService.GetDefaultSyncInterval();
			autoSync.Checked = defaultInterval.Value != System.Threading.Timeout.Infinite;

			try
			{
				iFolderSettings ifSettings = ifWebService.GetSettings();

				// Reset the proxy settings.
				useProxy.Checked = ifSettings.UseProxy;
				proxy.Text = ifSettings.ProxyHost;
				port.Value = (decimal)ifSettings.ProxyPort;
			}
			catch (Exception ex)
			{
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readiFolderSettingsError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}

			Cursor.Current = Cursors.Default;
			apply.Enabled = cancel.Enabled = false;
		}

		private void autoStart_CheckedChanged(object sender, System.EventArgs e)
		{
			if (autoStart.Focused)
			{
				apply.Enabled = cancel.Enabled = true;
			}
		}

		private void displayConfirmation_CheckedChanged(object sender, System.EventArgs e)
		{
			if (displayConfirmation.Focused)
			{
				apply.Enabled = cancel.Enabled = true;
			}
		}

		private void autoSync_CheckedChanged(object sender, System.EventArgs e)
		{
			if (autoSync.Focused)
			{
				apply.Enabled = cancel.Enabled = true;
			}

			defaultInterval.Enabled = autoSync.Checked;
		}

		private void defaultInterval_ValueChanged(object sender, System.EventArgs e)
		{
			if (defaultInterval.Focused)
			{
				if (!defaultInterval.Text.Equals(string.Empty))
				{
					defaultInterval.Value = decimal.Parse(defaultInterval.Text);
				}

				apply.Enabled = cancel.Enabled = true;
			}
		}

		private void useProxy_CheckedChanged(object sender, System.EventArgs e)
		{
			if (useProxy.Focused)
			{
				apply.Enabled = cancel.Enabled = true;
			}

			proxy.Enabled = port.Enabled = useProxy.Checked;
		}

		private void proxy_TextChanged(object sender, System.EventArgs e)
		{
			if (proxy.Focused)
			{
				apply.Enabled = cancel.Enabled = true;
			}
		}

		private void port_ValueChanged(object sender, System.EventArgs e)
		{
			if (port.Focused)
			{
				apply.Enabled = cancel.Enabled = true;
			}
		}

		private void notifyShared_CheckedChanged(object sender, System.EventArgs e)
		{
			if (notifyShared.Focused)
			{
				apply.Enabled = cancel.Enabled = true;
			}
		}

		private void notifyCollisions_CheckedChanged(object sender, System.EventArgs e)
		{
			if (notifyCollisions.Focused)
			{
				apply.Enabled = cancel.Enabled = true;
			}
		}

		private void notifyJoins_CheckedChanged(object sender, System.EventArgs e)
		{
			if (notifyJoins.Focused)
			{
				apply.Enabled = cancel.Enabled = true;
			}
		}
		#endregion

		#region Server Tab
		private void defaultServer_CheckedChanged(object sender, System.EventArgs e)
		{
			if (defaultServer.Focused && defaultServer.Checked)
			{
				// Set this domain as the default.
				try
				{
					Domain domain = (Domain)accounts.SelectedItems[0].Tag;
					ifWebService.SetDefaultDomain(domain.DomainWeb.ID);

					// Reset the flag on the current default domain.
					defaultDomain.DomainWeb.IsDefault = false;

					// Set the flag on the new default domain.
					domain.DomainWeb.IsDefault = true;
					defaultDomain = domain;

					// Disable the checkbox so that it cannot be unchecked.
					defaultServer.Enabled = false;
				}
				catch
				{}
			}
		}

		private void addAccount_Click(object sender, System.EventArgs e)
		{
/*			AddAccount addAccount = new AddAccount(ifWebService);
			addAccount.EnterpriseConnect += new Novell.FormsTrayApp.AddAccount.EnterpriseConnectDelegate(addAccount_EnterpriseConnect);
			addAccount.ShowDialog();

			if (addAccount.UpdateStarted)
			{
				// TODO: An update has started ... shutdown.
			}*/

			ListViewItem lvi = new ListViewItem(new string[] {string.Empty, string.Empty});
			accounts.Items.Add(lvi);
			accounts.SelectedItems.Clear();
			lvi.Selected = true;
		}

		private void userName_TextChanged(object sender, System.EventArgs e)
		{
			ListViewItem lvi = accounts.SelectedItems[0];
			lvi.SubItems[0].Text = userName.Text;
		}

		private void server_TextChanged(object sender, System.EventArgs e)
		{
			ListViewItem lvi = accounts.SelectedItems[0];
			lvi.SubItems[1].Text = server.Text;
		}

		private void removeAccount_Click(object sender, System.EventArgs e)
		{
			// TODO:
		}

		private void rememberPassword_CheckedChanged(object sender, System.EventArgs e)
		{
			// TODO:
		}

		private void autoLogin_CheckedChanged(object sender, System.EventArgs e)
		{
			// TODO:
		}

		private void addAccount_EnterpriseConnect(object sender, DomainConnectEventArgs e)
		{
			AddDomainToList(e.DomainWeb);
		}

		private void accounts_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (accounts.SelectedItems.Count == 1)
			{
				ListViewItem lvi = accounts.SelectedItems[0];
				if (lvi != null)
				{
					userName.Text = lvi.SubItems[0].Text;
					server.Text = lvi.SubItems[1].Text;
					password.Text = string.Empty;

					if (lvi.Tag == null)
					{
						// This is a new account.
						userName.ReadOnly = server.ReadOnly = false;
						userName.Enabled = server.Enabled = password.Enabled = 
							rememberPassword.Enabled = autoLogin.Enabled = 
							removeAccount.Enabled = true;
						details.Enabled = defaultServer.Checked = defaultServer.Enabled = false;
						userName.Focus();
					}
					else
					{
						details.Enabled = true;
						userName.ReadOnly = server.ReadOnly = true;

						defaultServer.Checked = ((Domain)lvi.Tag).DomainWeb.IsDefault;
						defaultServer.Enabled = !defaultServer.Checked;

						// TODO: Don't allow the workgroup account to be removed.
						// Don't allow the default account to be removed.
						removeAccount.Enabled = !defaultServer.Checked;

						// TODO: Fill in password with fixed long length of characters if it is currently remembered.
						password.Enabled = true;

						// TODO: set remember password and auto login settings.
					}
				}
			}
			else
			{
				userName.Enabled = server.Enabled = password.Enabled = rememberPassword.Enabled =
					autoLogin.Enabled = defaultServer.Enabled = details.Enabled = 
					removeAccount.Enabled = false;
			}
		}

		private void details_Click(object sender, System.EventArgs e)
		{
			ServerDetails serverDetails = new ServerDetails(this.ifWebService, accounts.Items, (Domain)accounts.SelectedItems[0].Tag);
			serverDetails.ShowDialog();
		}

		private void connect_Click(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				DomainWeb domainWeb = ifWebService.ConnectToDomain(userName.Text, password.Text, server.Text);

				Domain domain = new Domain(domainWeb);
				ListViewItem lvi = accounts.SelectedItems[0];
				lvi.SubItems[1].Text = domainWeb.Name;
				lvi.Tag = domain;
				servers2.Items.Add(domain);

				// Update default.
				if (domainWeb.IsDefault)
				{
					defaultDomain.DomainWeb.IsDefault = false;
					defaultDomain = domain;
					defaultServer.Checked = true;
					defaultServer.Enabled = false;
				}

				// TODO: save state of checkboxes.

				try
				{
					// Check for an update.
					bool updateStarted = FormsTrayApp.CheckForClientUpdate(domainWeb.ID, userName.Text, password.Text);
					if (updateStarted)
					{
						// TODO: Shut down the tray app.
					}

					if (!rememberPassword.Checked)
					{
						password.Text = string.Empty;
					}
				}
				catch (Exception ex)
				{
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("checkUpdateError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
					mmb.ShowDialog();
				}
			}
			catch (Exception ex)
			{
				if (ex.Message.IndexOf("HTTP status 401") != -1)
				{
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("failedAuth"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}
				else
				{
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("serverConnectError"), resourceManager.GetString("serverConnectErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}
			}

			Cursor.Current = Cursors.Default;
		}
		#endregion

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
