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
		private const double megaByte = 1048576;
		private const int maxMessages = 500;
		private System.Timers.Timer updateEnterpriseTimer;
		private short retryCount = 2;
		private Hashtable ht;
		private iFolderWebService ifWebService;
		private IProcEventClient eventClient;
		private string currentUserID;
		private string currentPOBoxID;
		private bool initialConnect = false;
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
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label enterpriseName;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label freeSpace;
		private System.Windows.Forms.Label usedSpace;
		private System.Windows.Forms.Label totalSpace;
		private Novell.iFolderCom.GaugeChart gaugeChart1;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label18;
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
		private System.Windows.Forms.Label userName;
		private System.Windows.Forms.CheckBox useProxy;
		private System.Windows.Forms.NumericUpDown port;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label status;
		private System.Windows.Forms.TextBox enterpriseDescription;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Button apply;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.MenuItem menuRemove;
		private System.Windows.Forms.MenuItem menuActionRemove;
		private System.Windows.Forms.CheckBox notifyShared;
		private System.Windows.Forms.CheckBox notifyCollisions;
		private System.Windows.Forms.CheckBox notifyJoins;
		private System.Windows.Forms.Label totalSpaceUnits;
		private System.Windows.Forms.Label usedSpaceUnits;
		private System.Windows.Forms.Label freeSpaceUnits;
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
			this.enterpriseDescription = new System.Windows.Forms.TextBox();
			this.userName = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.label18 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.gaugeChart1 = new Novell.iFolderCom.GaugeChart();
			this.totalSpaceUnits = new System.Windows.Forms.Label();
			this.usedSpaceUnits = new System.Windows.Forms.Label();
			this.freeSpaceUnits = new System.Windows.Forms.Label();
			this.totalSpace = new System.Windows.Forms.Label();
			this.usedSpace = new System.Windows.Forms.Label();
			this.freeSpace = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.enterpriseName = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
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
			this.groupBox6.SuspendLayout();
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
			this.tabPage5.Controls.Add(this.enterpriseDescription);
			this.tabPage5.Controls.Add(this.userName);
			this.tabPage5.Controls.Add(this.label10);
			this.tabPage5.Controls.Add(this.groupBox6);
			this.tabPage5.Controls.Add(this.label9);
			this.tabPage5.Controls.Add(this.enterpriseName);
			this.tabPage5.Controls.Add(this.label8);
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
			// enterpriseDescription
			// 
			this.enterpriseDescription.AccessibleDescription = resources.GetString("enterpriseDescription.AccessibleDescription");
			this.enterpriseDescription.AccessibleName = resources.GetString("enterpriseDescription.AccessibleName");
			this.enterpriseDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("enterpriseDescription.Anchor")));
			this.enterpriseDescription.AutoSize = ((bool)(resources.GetObject("enterpriseDescription.AutoSize")));
			this.enterpriseDescription.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("enterpriseDescription.BackgroundImage")));
			this.enterpriseDescription.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("enterpriseDescription.Dock")));
			this.enterpriseDescription.Enabled = ((bool)(resources.GetObject("enterpriseDescription.Enabled")));
			this.enterpriseDescription.Font = ((System.Drawing.Font)(resources.GetObject("enterpriseDescription.Font")));
			this.enterpriseDescription.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("enterpriseDescription.ImeMode")));
			this.enterpriseDescription.Location = ((System.Drawing.Point)(resources.GetObject("enterpriseDescription.Location")));
			this.enterpriseDescription.MaxLength = ((int)(resources.GetObject("enterpriseDescription.MaxLength")));
			this.enterpriseDescription.Multiline = ((bool)(resources.GetObject("enterpriseDescription.Multiline")));
			this.enterpriseDescription.Name = "enterpriseDescription";
			this.enterpriseDescription.PasswordChar = ((char)(resources.GetObject("enterpriseDescription.PasswordChar")));
			this.enterpriseDescription.ReadOnly = true;
			this.enterpriseDescription.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("enterpriseDescription.RightToLeft")));
			this.enterpriseDescription.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("enterpriseDescription.ScrollBars")));
			this.enterpriseDescription.Size = ((System.Drawing.Size)(resources.GetObject("enterpriseDescription.Size")));
			this.enterpriseDescription.TabIndex = ((int)(resources.GetObject("enterpriseDescription.TabIndex")));
			this.enterpriseDescription.Text = resources.GetString("enterpriseDescription.Text");
			this.enterpriseDescription.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("enterpriseDescription.TextAlign")));
			this.enterpriseDescription.Visible = ((bool)(resources.GetObject("enterpriseDescription.Visible")));
			this.enterpriseDescription.WordWrap = ((bool)(resources.GetObject("enterpriseDescription.WordWrap")));
			// 
			// userName
			// 
			this.userName.AccessibleDescription = resources.GetString("userName.AccessibleDescription");
			this.userName.AccessibleName = resources.GetString("userName.AccessibleName");
			this.userName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("userName.Anchor")));
			this.userName.AutoSize = ((bool)(resources.GetObject("userName.AutoSize")));
			this.userName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("userName.Dock")));
			this.userName.Enabled = ((bool)(resources.GetObject("userName.Enabled")));
			this.userName.Font = ((System.Drawing.Font)(resources.GetObject("userName.Font")));
			this.userName.Image = ((System.Drawing.Image)(resources.GetObject("userName.Image")));
			this.userName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("userName.ImageAlign")));
			this.userName.ImageIndex = ((int)(resources.GetObject("userName.ImageIndex")));
			this.userName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("userName.ImeMode")));
			this.userName.Location = ((System.Drawing.Point)(resources.GetObject("userName.Location")));
			this.userName.Name = "userName";
			this.userName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("userName.RightToLeft")));
			this.userName.Size = ((System.Drawing.Size)(resources.GetObject("userName.Size")));
			this.userName.TabIndex = ((int)(resources.GetObject("userName.TabIndex")));
			this.userName.Text = resources.GetString("userName.Text");
			this.userName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("userName.TextAlign")));
			this.userName.Visible = ((bool)(resources.GetObject("userName.Visible")));
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
			// groupBox6
			// 
			this.groupBox6.AccessibleDescription = resources.GetString("groupBox6.AccessibleDescription");
			this.groupBox6.AccessibleName = resources.GetString("groupBox6.AccessibleName");
			this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox6.Anchor")));
			this.groupBox6.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox6.BackgroundImage")));
			this.groupBox6.Controls.Add(this.label18);
			this.groupBox6.Controls.Add(this.label17);
			this.groupBox6.Controls.Add(this.gaugeChart1);
			this.groupBox6.Controls.Add(this.totalSpaceUnits);
			this.groupBox6.Controls.Add(this.usedSpaceUnits);
			this.groupBox6.Controls.Add(this.freeSpaceUnits);
			this.groupBox6.Controls.Add(this.totalSpace);
			this.groupBox6.Controls.Add(this.usedSpace);
			this.groupBox6.Controls.Add(this.freeSpace);
			this.groupBox6.Controls.Add(this.label13);
			this.groupBox6.Controls.Add(this.label12);
			this.groupBox6.Controls.Add(this.label11);
			this.groupBox6.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBox6.Dock")));
			this.groupBox6.Enabled = ((bool)(resources.GetObject("groupBox6.Enabled")));
			this.groupBox6.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox6.Font = ((System.Drawing.Font)(resources.GetObject("groupBox6.Font")));
			this.groupBox6.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBox6.ImeMode")));
			this.groupBox6.Location = ((System.Drawing.Point)(resources.GetObject("groupBox6.Location")));
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBox6.RightToLeft")));
			this.groupBox6.Size = ((System.Drawing.Size)(resources.GetObject("groupBox6.Size")));
			this.groupBox6.TabIndex = ((int)(resources.GetObject("groupBox6.TabIndex")));
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = resources.GetString("groupBox6.Text");
			this.groupBox6.Visible = ((bool)(resources.GetObject("groupBox6.Visible")));
			// 
			// label18
			// 
			this.label18.AccessibleDescription = resources.GetString("label18.AccessibleDescription");
			this.label18.AccessibleName = resources.GetString("label18.AccessibleName");
			this.label18.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label18.Anchor")));
			this.label18.AutoSize = ((bool)(resources.GetObject("label18.AutoSize")));
			this.label18.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label18.Dock")));
			this.label18.Enabled = ((bool)(resources.GetObject("label18.Enabled")));
			this.label18.Font = ((System.Drawing.Font)(resources.GetObject("label18.Font")));
			this.label18.Image = ((System.Drawing.Image)(resources.GetObject("label18.Image")));
			this.label18.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label18.ImageAlign")));
			this.label18.ImageIndex = ((int)(resources.GetObject("label18.ImageIndex")));
			this.label18.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label18.ImeMode")));
			this.label18.Location = ((System.Drawing.Point)(resources.GetObject("label18.Location")));
			this.label18.Name = "label18";
			this.label18.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label18.RightToLeft")));
			this.label18.Size = ((System.Drawing.Size)(resources.GetObject("label18.Size")));
			this.label18.TabIndex = ((int)(resources.GetObject("label18.TabIndex")));
			this.label18.Text = resources.GetString("label18.Text");
			this.label18.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label18.TextAlign")));
			this.label18.Visible = ((bool)(resources.GetObject("label18.Visible")));
			// 
			// label17
			// 
			this.label17.AccessibleDescription = resources.GetString("label17.AccessibleDescription");
			this.label17.AccessibleName = resources.GetString("label17.AccessibleName");
			this.label17.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label17.Anchor")));
			this.label17.AutoSize = ((bool)(resources.GetObject("label17.AutoSize")));
			this.label17.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label17.Dock")));
			this.label17.Enabled = ((bool)(resources.GetObject("label17.Enabled")));
			this.label17.Font = ((System.Drawing.Font)(resources.GetObject("label17.Font")));
			this.label17.Image = ((System.Drawing.Image)(resources.GetObject("label17.Image")));
			this.label17.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label17.ImageAlign")));
			this.label17.ImageIndex = ((int)(resources.GetObject("label17.ImageIndex")));
			this.label17.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label17.ImeMode")));
			this.label17.Location = ((System.Drawing.Point)(resources.GetObject("label17.Location")));
			this.label17.Name = "label17";
			this.label17.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label17.RightToLeft")));
			this.label17.Size = ((System.Drawing.Size)(resources.GetObject("label17.Size")));
			this.label17.TabIndex = ((int)(resources.GetObject("label17.TabIndex")));
			this.label17.Text = resources.GetString("label17.Text");
			this.label17.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label17.TextAlign")));
			this.label17.Visible = ((bool)(resources.GetObject("label17.Visible")));
			// 
			// gaugeChart1
			// 
			this.gaugeChart1.AccessibleDescription = resources.GetString("gaugeChart1.AccessibleDescription");
			this.gaugeChart1.AccessibleName = resources.GetString("gaugeChart1.AccessibleName");
			this.gaugeChart1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("gaugeChart1.Anchor")));
			this.gaugeChart1.AutoScroll = ((bool)(resources.GetObject("gaugeChart1.AutoScroll")));
			this.gaugeChart1.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("gaugeChart1.AutoScrollMargin")));
			this.gaugeChart1.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("gaugeChart1.AutoScrollMinSize")));
			this.gaugeChart1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("gaugeChart1.BackgroundImage")));
			this.gaugeChart1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("gaugeChart1.Dock")));
			this.gaugeChart1.Enabled = ((bool)(resources.GetObject("gaugeChart1.Enabled")));
			this.gaugeChart1.Font = ((System.Drawing.Font)(resources.GetObject("gaugeChart1.Font")));
			this.gaugeChart1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("gaugeChart1.ImeMode")));
			this.gaugeChart1.Location = ((System.Drawing.Point)(resources.GetObject("gaugeChart1.Location")));
			this.gaugeChart1.Name = "gaugeChart1";
			this.gaugeChart1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("gaugeChart1.RightToLeft")));
			this.gaugeChart1.Size = ((System.Drawing.Size)(resources.GetObject("gaugeChart1.Size")));
			this.gaugeChart1.TabIndex = ((int)(resources.GetObject("gaugeChart1.TabIndex")));
			this.gaugeChart1.Visible = ((bool)(resources.GetObject("gaugeChart1.Visible")));
			// 
			// totalSpaceUnits
			// 
			this.totalSpaceUnits.AccessibleDescription = resources.GetString("totalSpaceUnits.AccessibleDescription");
			this.totalSpaceUnits.AccessibleName = resources.GetString("totalSpaceUnits.AccessibleName");
			this.totalSpaceUnits.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("totalSpaceUnits.Anchor")));
			this.totalSpaceUnits.AutoSize = ((bool)(resources.GetObject("totalSpaceUnits.AutoSize")));
			this.totalSpaceUnits.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("totalSpaceUnits.Dock")));
			this.totalSpaceUnits.Enabled = ((bool)(resources.GetObject("totalSpaceUnits.Enabled")));
			this.totalSpaceUnits.Font = ((System.Drawing.Font)(resources.GetObject("totalSpaceUnits.Font")));
			this.totalSpaceUnits.Image = ((System.Drawing.Image)(resources.GetObject("totalSpaceUnits.Image")));
			this.totalSpaceUnits.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("totalSpaceUnits.ImageAlign")));
			this.totalSpaceUnits.ImageIndex = ((int)(resources.GetObject("totalSpaceUnits.ImageIndex")));
			this.totalSpaceUnits.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("totalSpaceUnits.ImeMode")));
			this.totalSpaceUnits.Location = ((System.Drawing.Point)(resources.GetObject("totalSpaceUnits.Location")));
			this.totalSpaceUnits.Name = "totalSpaceUnits";
			this.totalSpaceUnits.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("totalSpaceUnits.RightToLeft")));
			this.totalSpaceUnits.Size = ((System.Drawing.Size)(resources.GetObject("totalSpaceUnits.Size")));
			this.totalSpaceUnits.TabIndex = ((int)(resources.GetObject("totalSpaceUnits.TabIndex")));
			this.totalSpaceUnits.Text = resources.GetString("totalSpaceUnits.Text");
			this.totalSpaceUnits.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("totalSpaceUnits.TextAlign")));
			this.totalSpaceUnits.Visible = ((bool)(resources.GetObject("totalSpaceUnits.Visible")));
			// 
			// usedSpaceUnits
			// 
			this.usedSpaceUnits.AccessibleDescription = resources.GetString("usedSpaceUnits.AccessibleDescription");
			this.usedSpaceUnits.AccessibleName = resources.GetString("usedSpaceUnits.AccessibleName");
			this.usedSpaceUnits.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("usedSpaceUnits.Anchor")));
			this.usedSpaceUnits.AutoSize = ((bool)(resources.GetObject("usedSpaceUnits.AutoSize")));
			this.usedSpaceUnits.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("usedSpaceUnits.Dock")));
			this.usedSpaceUnits.Enabled = ((bool)(resources.GetObject("usedSpaceUnits.Enabled")));
			this.usedSpaceUnits.Font = ((System.Drawing.Font)(resources.GetObject("usedSpaceUnits.Font")));
			this.usedSpaceUnits.Image = ((System.Drawing.Image)(resources.GetObject("usedSpaceUnits.Image")));
			this.usedSpaceUnits.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("usedSpaceUnits.ImageAlign")));
			this.usedSpaceUnits.ImageIndex = ((int)(resources.GetObject("usedSpaceUnits.ImageIndex")));
			this.usedSpaceUnits.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("usedSpaceUnits.ImeMode")));
			this.usedSpaceUnits.Location = ((System.Drawing.Point)(resources.GetObject("usedSpaceUnits.Location")));
			this.usedSpaceUnits.Name = "usedSpaceUnits";
			this.usedSpaceUnits.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("usedSpaceUnits.RightToLeft")));
			this.usedSpaceUnits.Size = ((System.Drawing.Size)(resources.GetObject("usedSpaceUnits.Size")));
			this.usedSpaceUnits.TabIndex = ((int)(resources.GetObject("usedSpaceUnits.TabIndex")));
			this.usedSpaceUnits.Text = resources.GetString("usedSpaceUnits.Text");
			this.usedSpaceUnits.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("usedSpaceUnits.TextAlign")));
			this.usedSpaceUnits.Visible = ((bool)(resources.GetObject("usedSpaceUnits.Visible")));
			// 
			// freeSpaceUnits
			// 
			this.freeSpaceUnits.AccessibleDescription = resources.GetString("freeSpaceUnits.AccessibleDescription");
			this.freeSpaceUnits.AccessibleName = resources.GetString("freeSpaceUnits.AccessibleName");
			this.freeSpaceUnits.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("freeSpaceUnits.Anchor")));
			this.freeSpaceUnits.AutoSize = ((bool)(resources.GetObject("freeSpaceUnits.AutoSize")));
			this.freeSpaceUnits.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("freeSpaceUnits.Dock")));
			this.freeSpaceUnits.Enabled = ((bool)(resources.GetObject("freeSpaceUnits.Enabled")));
			this.freeSpaceUnits.Font = ((System.Drawing.Font)(resources.GetObject("freeSpaceUnits.Font")));
			this.freeSpaceUnits.Image = ((System.Drawing.Image)(resources.GetObject("freeSpaceUnits.Image")));
			this.freeSpaceUnits.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("freeSpaceUnits.ImageAlign")));
			this.freeSpaceUnits.ImageIndex = ((int)(resources.GetObject("freeSpaceUnits.ImageIndex")));
			this.freeSpaceUnits.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("freeSpaceUnits.ImeMode")));
			this.freeSpaceUnits.Location = ((System.Drawing.Point)(resources.GetObject("freeSpaceUnits.Location")));
			this.freeSpaceUnits.Name = "freeSpaceUnits";
			this.freeSpaceUnits.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("freeSpaceUnits.RightToLeft")));
			this.freeSpaceUnits.Size = ((System.Drawing.Size)(resources.GetObject("freeSpaceUnits.Size")));
			this.freeSpaceUnits.TabIndex = ((int)(resources.GetObject("freeSpaceUnits.TabIndex")));
			this.freeSpaceUnits.Text = resources.GetString("freeSpaceUnits.Text");
			this.freeSpaceUnits.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("freeSpaceUnits.TextAlign")));
			this.freeSpaceUnits.Visible = ((bool)(resources.GetObject("freeSpaceUnits.Visible")));
			// 
			// totalSpace
			// 
			this.totalSpace.AccessibleDescription = resources.GetString("totalSpace.AccessibleDescription");
			this.totalSpace.AccessibleName = resources.GetString("totalSpace.AccessibleName");
			this.totalSpace.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("totalSpace.Anchor")));
			this.totalSpace.AutoSize = ((bool)(resources.GetObject("totalSpace.AutoSize")));
			this.totalSpace.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("totalSpace.Dock")));
			this.totalSpace.Enabled = ((bool)(resources.GetObject("totalSpace.Enabled")));
			this.totalSpace.Font = ((System.Drawing.Font)(resources.GetObject("totalSpace.Font")));
			this.totalSpace.Image = ((System.Drawing.Image)(resources.GetObject("totalSpace.Image")));
			this.totalSpace.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("totalSpace.ImageAlign")));
			this.totalSpace.ImageIndex = ((int)(resources.GetObject("totalSpace.ImageIndex")));
			this.totalSpace.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("totalSpace.ImeMode")));
			this.totalSpace.Location = ((System.Drawing.Point)(resources.GetObject("totalSpace.Location")));
			this.totalSpace.Name = "totalSpace";
			this.totalSpace.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("totalSpace.RightToLeft")));
			this.totalSpace.Size = ((System.Drawing.Size)(resources.GetObject("totalSpace.Size")));
			this.totalSpace.TabIndex = ((int)(resources.GetObject("totalSpace.TabIndex")));
			this.totalSpace.Text = resources.GetString("totalSpace.Text");
			this.totalSpace.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("totalSpace.TextAlign")));
			this.totalSpace.Visible = ((bool)(resources.GetObject("totalSpace.Visible")));
			// 
			// usedSpace
			// 
			this.usedSpace.AccessibleDescription = resources.GetString("usedSpace.AccessibleDescription");
			this.usedSpace.AccessibleName = resources.GetString("usedSpace.AccessibleName");
			this.usedSpace.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("usedSpace.Anchor")));
			this.usedSpace.AutoSize = ((bool)(resources.GetObject("usedSpace.AutoSize")));
			this.usedSpace.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("usedSpace.Dock")));
			this.usedSpace.Enabled = ((bool)(resources.GetObject("usedSpace.Enabled")));
			this.usedSpace.Font = ((System.Drawing.Font)(resources.GetObject("usedSpace.Font")));
			this.usedSpace.Image = ((System.Drawing.Image)(resources.GetObject("usedSpace.Image")));
			this.usedSpace.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("usedSpace.ImageAlign")));
			this.usedSpace.ImageIndex = ((int)(resources.GetObject("usedSpace.ImageIndex")));
			this.usedSpace.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("usedSpace.ImeMode")));
			this.usedSpace.Location = ((System.Drawing.Point)(resources.GetObject("usedSpace.Location")));
			this.usedSpace.Name = "usedSpace";
			this.usedSpace.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("usedSpace.RightToLeft")));
			this.usedSpace.Size = ((System.Drawing.Size)(resources.GetObject("usedSpace.Size")));
			this.usedSpace.TabIndex = ((int)(resources.GetObject("usedSpace.TabIndex")));
			this.usedSpace.Text = resources.GetString("usedSpace.Text");
			this.usedSpace.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("usedSpace.TextAlign")));
			this.usedSpace.Visible = ((bool)(resources.GetObject("usedSpace.Visible")));
			// 
			// freeSpace
			// 
			this.freeSpace.AccessibleDescription = resources.GetString("freeSpace.AccessibleDescription");
			this.freeSpace.AccessibleName = resources.GetString("freeSpace.AccessibleName");
			this.freeSpace.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("freeSpace.Anchor")));
			this.freeSpace.AutoSize = ((bool)(resources.GetObject("freeSpace.AutoSize")));
			this.freeSpace.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("freeSpace.Dock")));
			this.freeSpace.Enabled = ((bool)(resources.GetObject("freeSpace.Enabled")));
			this.freeSpace.Font = ((System.Drawing.Font)(resources.GetObject("freeSpace.Font")));
			this.freeSpace.Image = ((System.Drawing.Image)(resources.GetObject("freeSpace.Image")));
			this.freeSpace.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("freeSpace.ImageAlign")));
			this.freeSpace.ImageIndex = ((int)(resources.GetObject("freeSpace.ImageIndex")));
			this.freeSpace.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("freeSpace.ImeMode")));
			this.freeSpace.Location = ((System.Drawing.Point)(resources.GetObject("freeSpace.Location")));
			this.freeSpace.Name = "freeSpace";
			this.freeSpace.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("freeSpace.RightToLeft")));
			this.freeSpace.Size = ((System.Drawing.Size)(resources.GetObject("freeSpace.Size")));
			this.freeSpace.TabIndex = ((int)(resources.GetObject("freeSpace.TabIndex")));
			this.freeSpace.Text = resources.GetString("freeSpace.Text");
			this.freeSpace.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("freeSpace.TextAlign")));
			this.freeSpace.Visible = ((bool)(resources.GetObject("freeSpace.Visible")));
			// 
			// label13
			// 
			this.label13.AccessibleDescription = resources.GetString("label13.AccessibleDescription");
			this.label13.AccessibleName = resources.GetString("label13.AccessibleName");
			this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label13.Anchor")));
			this.label13.AutoSize = ((bool)(resources.GetObject("label13.AutoSize")));
			this.label13.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label13.Dock")));
			this.label13.Enabled = ((bool)(resources.GetObject("label13.Enabled")));
			this.label13.Font = ((System.Drawing.Font)(resources.GetObject("label13.Font")));
			this.label13.Image = ((System.Drawing.Image)(resources.GetObject("label13.Image")));
			this.label13.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label13.ImageAlign")));
			this.label13.ImageIndex = ((int)(resources.GetObject("label13.ImageIndex")));
			this.label13.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label13.ImeMode")));
			this.label13.Location = ((System.Drawing.Point)(resources.GetObject("label13.Location")));
			this.label13.Name = "label13";
			this.label13.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label13.RightToLeft")));
			this.label13.Size = ((System.Drawing.Size)(resources.GetObject("label13.Size")));
			this.label13.TabIndex = ((int)(resources.GetObject("label13.TabIndex")));
			this.label13.Text = resources.GetString("label13.Text");
			this.label13.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label13.TextAlign")));
			this.label13.Visible = ((bool)(resources.GetObject("label13.Visible")));
			// 
			// label12
			// 
			this.label12.AccessibleDescription = resources.GetString("label12.AccessibleDescription");
			this.label12.AccessibleName = resources.GetString("label12.AccessibleName");
			this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label12.Anchor")));
			this.label12.AutoSize = ((bool)(resources.GetObject("label12.AutoSize")));
			this.label12.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label12.Dock")));
			this.label12.Enabled = ((bool)(resources.GetObject("label12.Enabled")));
			this.label12.Font = ((System.Drawing.Font)(resources.GetObject("label12.Font")));
			this.label12.Image = ((System.Drawing.Image)(resources.GetObject("label12.Image")));
			this.label12.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label12.ImageAlign")));
			this.label12.ImageIndex = ((int)(resources.GetObject("label12.ImageIndex")));
			this.label12.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label12.ImeMode")));
			this.label12.Location = ((System.Drawing.Point)(resources.GetObject("label12.Location")));
			this.label12.Name = "label12";
			this.label12.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label12.RightToLeft")));
			this.label12.Size = ((System.Drawing.Size)(resources.GetObject("label12.Size")));
			this.label12.TabIndex = ((int)(resources.GetObject("label12.TabIndex")));
			this.label12.Text = resources.GetString("label12.Text");
			this.label12.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label12.TextAlign")));
			this.label12.Visible = ((bool)(resources.GetObject("label12.Visible")));
			// 
			// label11
			// 
			this.label11.AccessibleDescription = resources.GetString("label11.AccessibleDescription");
			this.label11.AccessibleName = resources.GetString("label11.AccessibleName");
			this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label11.Anchor")));
			this.label11.AutoSize = ((bool)(resources.GetObject("label11.AutoSize")));
			this.label11.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label11.Dock")));
			this.label11.Enabled = ((bool)(resources.GetObject("label11.Enabled")));
			this.label11.Font = ((System.Drawing.Font)(resources.GetObject("label11.Font")));
			this.label11.Image = ((System.Drawing.Image)(resources.GetObject("label11.Image")));
			this.label11.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label11.ImageAlign")));
			this.label11.ImageIndex = ((int)(resources.GetObject("label11.ImageIndex")));
			this.label11.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label11.ImeMode")));
			this.label11.Location = ((System.Drawing.Point)(resources.GetObject("label11.Location")));
			this.label11.Name = "label11";
			this.label11.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label11.RightToLeft")));
			this.label11.Size = ((System.Drawing.Size)(resources.GetObject("label11.Size")));
			this.label11.TabIndex = ((int)(resources.GetObject("label11.TabIndex")));
			this.label11.Text = resources.GetString("label11.Text");
			this.label11.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label11.TextAlign")));
			this.label11.Visible = ((bool)(resources.GetObject("label11.Visible")));
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
			// enterpriseName
			// 
			this.enterpriseName.AccessibleDescription = resources.GetString("enterpriseName.AccessibleDescription");
			this.enterpriseName.AccessibleName = resources.GetString("enterpriseName.AccessibleName");
			this.enterpriseName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("enterpriseName.Anchor")));
			this.enterpriseName.AutoSize = ((bool)(resources.GetObject("enterpriseName.AutoSize")));
			this.enterpriseName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("enterpriseName.Dock")));
			this.enterpriseName.Enabled = ((bool)(resources.GetObject("enterpriseName.Enabled")));
			this.enterpriseName.Font = ((System.Drawing.Font)(resources.GetObject("enterpriseName.Font")));
			this.enterpriseName.Image = ((System.Drawing.Image)(resources.GetObject("enterpriseName.Image")));
			this.enterpriseName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("enterpriseName.ImageAlign")));
			this.enterpriseName.ImageIndex = ((int)(resources.GetObject("enterpriseName.ImageIndex")));
			this.enterpriseName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("enterpriseName.ImeMode")));
			this.enterpriseName.Location = ((System.Drawing.Point)(resources.GetObject("enterpriseName.Location")));
			this.enterpriseName.Name = "enterpriseName";
			this.enterpriseName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("enterpriseName.RightToLeft")));
			this.enterpriseName.Size = ((System.Drawing.Size)(resources.GetObject("enterpriseName.Size")));
			this.enterpriseName.TabIndex = ((int)(resources.GetObject("enterpriseName.TabIndex")));
			this.enterpriseName.Text = resources.GetString("enterpriseName.Text");
			this.enterpriseName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("enterpriseName.TextAlign")));
			this.enterpriseName.Visible = ((bool)(resources.GetObject("enterpriseName.Visible")));
			// 
			// label8
			// 
			this.label8.AccessibleDescription = resources.GetString("label8.AccessibleDescription");
			this.label8.AccessibleName = resources.GetString("label8.AccessibleName");
			this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label8.Anchor")));
			this.label8.AutoSize = ((bool)(resources.GetObject("label8.AutoSize")));
			this.label8.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label8.Dock")));
			this.label8.Enabled = ((bool)(resources.GetObject("label8.Enabled")));
			this.label8.Font = ((System.Drawing.Font)(resources.GetObject("label8.Font")));
			this.label8.Image = ((System.Drawing.Image)(resources.GetObject("label8.Image")));
			this.label8.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label8.ImageAlign")));
			this.label8.ImageIndex = ((int)(resources.GetObject("label8.ImageIndex")));
			this.label8.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label8.ImeMode")));
			this.label8.Location = ((System.Drawing.Point)(resources.GetObject("label8.Location")));
			this.label8.Name = "label8";
			this.label8.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label8.RightToLeft")));
			this.label8.Size = ((System.Drawing.Size)(resources.GetObject("label8.Size")));
			this.label8.TabIndex = ((int)(resources.GetObject("label8.TabIndex")));
			this.label8.Text = resources.GetString("label8.Text");
			this.label8.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label8.TextAlign")));
			this.label8.Visible = ((bool)(resources.GetObject("label8.Visible")));
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
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.KeyPreview = true;
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.Menu = this.mainMenu1;
			this.MinimizeBox = false;
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
			this.groupBox6.ResumeLayout(false);
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
					addiFolderToListView(ifolder);

					if (ifolder.State.Equals("Local"))
					{
						// Notify the shell.
						Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, ifolder.UnManagedPath, IntPtr.Zero);
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
					items[2] = stateToString(ifolder.State, ifolder.HasConflicts, out imageIndex);

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
				string status = stateToString(ifolder.State, ifolder.HasConflicts, out imageIndex);
				if (!lvi.SubItems[2].Text.Equals(statusSync) && !lvi.SubItems[2].Text.Equals(statusSyncFail))
				{
					lvi.SubItems[2].Text = status;
				}
				lvi.ImageIndex = imageIndex;
			}
		}

		private string stateToString(string state, bool conflicts, out int imageIndex)
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
					imageIndex = 1;
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

		private void refreshiFolders()
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
				iFolderWeb[] ifolderArray = ifWebService.GetAlliFolders();
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

		private void updateEnterpriseData(iFolderSettings ifSettings)
		{
			currentUserID = ifSettings.CurrentUserID;
			currentPOBoxID = ifSettings.DefaultPOBoxID;

			try
			{
				iFolderUser ifolderUser = ifWebService.GetiFolderUser(ifSettings.CurrentUserID);
				userName.Text = ifolderUser.Name;
			}
			catch (Exception e)
			{
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readUserError"), string.Empty, e.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}

			enterpriseName.Text = ifSettings.EnterpriseName;
			enterpriseDescription.Text = ifSettings.EnterpriseDescription;

			try
			{
				// Get the disk space.
				DiskSpace diskSpace = ifWebService.GetUserDiskSpace(ifSettings.CurrentUserID);
				if (diskSpace.Limit != 0)
				{
					usedSpaceUnits.Text = freeSpaceUnits.Text = totalSpaceUnits.Text = 
						resourceManager.GetString("freeSpaceUnits.Text");
					totalSpace.Text = ((double)Math.Round(diskSpace.Limit/megaByte, 2)).ToString();

					double used = Math.Round(diskSpace.UsedSpace/megaByte, 2);
					usedSpace.Text = used.ToString();
					freeSpace.Text = ((double)Math.Round(diskSpace.AvailableSpace/megaByte, 2)).ToString();

					// Set up the gauge chart.
					gaugeChart1.MaxValue = diskSpace.Limit / megaByte;
					gaugeChart1.Used = used;
					gaugeChart1.BarColor = SystemColors.ActiveCaption;
				}
				else
				{
					usedSpaceUnits.Text = freeSpaceUnits.Text = totalSpaceUnits.Text =
						resourceManager.GetString("notApplicable");
					usedSpace.Text = freeSpace.Text = totalSpace.Text = "";
					gaugeChart1.Used = 0;
				}
			}
			catch (Exception e)
			{
				if (retryCount-- > 0)
				{
					updateEnterpriseTimer.Start();
				}
				else
				{
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readQuotaError"), string.Empty, e.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
					mmb.ShowDialog();
				}
			}

			// Cause the gauge chart to be redrawn.
			gaugeChart1.Invalidate(true);
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

			// Update the iFolders listview.
			refreshiFolders();
		}

		private void GlobalProperties_VisibleChanged(object sender, System.EventArgs e)
		{
			if (this.Visible)
			{
				apply.Enabled = cancel.Enabled = false;

				// Update the auto start setting.
				autoStart.Checked = IsRunEnabled();

				notifyShared.Checked = NotifyShareEnabled;
				notifyCollisions.Checked = NotifyCollisionEnabled;
				notifyJoins.Checked = NotifyJoinEnabled;

				// Update the display confirmation setting.
				displayConfirmation.Checked = iFolderComponent.DisplayConfirmationEnabled;

				iFolderSettings ifSettings = null;

				try
				{
					ifSettings = ifWebService.GetSettings();
					currentUserID = ifSettings.CurrentUserID;
					currentPOBoxID = ifSettings.DefaultPOBoxID;
				}
				catch (Exception ex)
				{
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readiFolderSettingsError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}

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

				try
				{
					if (ifSettings == null)
					{
						ifSettings = ifWebService.GetSettings();
					}

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

				if (ifSettings.HaveEnterprise)
				{
					ShowEnterpriseTab = true;
					updateEnterpriseData(ifSettings);
				}

				Activate();
			}
		}

		private void GlobalProperties_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// Hide the dialog.
			e.Cancel = true;
			Hide();
		}

		private void updateEnterpriseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			updateEnterpriseTimer.Stop();
			iFolderSettings ifSettings = ifWebService.GetSettings();
			updateEnterpriseData(ifSettings);
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
			new iFolderComponent().ShowHelp(Application.StartupPath);
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
				refreshiFolders();
			}
		}

		#region iFolders Tab
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
				menuRemove.Text = ((iFolderWeb)iFolderView.SelectedItems[0].Tag).OwnerID.Equals(currentUserID) ? resourceManager.GetString("deleteAction") : resourceManager.GetString("menuRemove.Text");
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
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

			folderBrowserDialog.Description = resourceManager.GetString("chooseFolder");

			while (true)
			{
				if(folderBrowserDialog.ShowDialog() == DialogResult.OK)
				{
					try
					{
						if (ifWebService.CanBeiFolder(folderBrowserDialog.SelectedPath))
						{
							// Create the iFolder.
							iFolderWeb ifolder = ifWebService.CreateLocaliFolder(folderBrowserDialog.SelectedPath);

							// Notify the shell.
							Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, folderBrowserDialog.SelectedPath, IntPtr.Zero);

							// Add the iFolder to the listview.
							addiFolderToListView(ifolder);

							// Display the new iFolder intro dialog.
							if (iFolderComponent.DisplayConfirmationEnabled)
							{
								new iFolderComponent().NewiFolderWizard(Application.StartupPath, folderBrowserDialog.SelectedPath);
							}
							break;
						}
						else
						{
							MessageBox.Show(resourceManager.GetString("invalidFolder"));
						}
					}
					catch (Exception ex)
					{
						Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("iFolderCreateError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
					}
				}
				else
				{
					break;
				}
			}
		}

		private void menuRefresh_Click(object sender, System.EventArgs e)
		{
			refreshiFolders();
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

				if (ifolder.OwnerID.Equals(currentUserID))
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
						ifWebService.DeclineiFolderInvitation(ifolder.ID);
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
						ifWebService.DeclineiFolderInvitation(newiFolder.ID);
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
	}
}
