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
using System.Runtime.InteropServices;
using System.Net;
using Microsoft.Win32;
using Novell.iFolderCom;
using Novell.Win32Util;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for GlobalProperties.
	/// </summary>
	public class GlobalProperties : System.Windows.Forms.Form
	{
		#region Class Members
		private const string iFolderRun = "iFolder";

		private const double megaByte = 1048576;
		//private Hashtable ht;
		//private EventSubscriber subscriber;
		private iFolderWebService ifWebService;
		private string currentUserID;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown defaultInterval;
		private System.Windows.Forms.CheckBox displayConfirmation;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
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
		private System.Windows.Forms.MenuItem menuEnabled;
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
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label16;
		private Novell.Forms.Controls.GaugeChart gaugeChart1;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label enterpriseDescription;
		private System.Windows.Forms.MenuItem menuResolve;
		private System.Windows.Forms.MenuItem menuActionResolve;
		private System.Windows.Forms.MenuItem menuAccept;
		private System.Windows.Forms.MenuItem menuDecline;
		private System.Windows.Forms.MenuItem menuDelete;
		private System.Windows.Forms.MenuItem menuActionAccept;
		private System.Windows.Forms.MenuItem menuActionDecline;
		private System.Windows.Forms.MenuItem menuActionDelete;
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
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		/// <summary>
		/// Instantiates a GlobalProperties object.
		/// </summary>
		public GlobalProperties(iFolderWebService ifolderWebService)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			ShowEnterpriseTab = false;

			// Show the first tab page by default.
			tabControl1.SelectedTab = tabPage1;

			ifWebService = ifolderWebService;

			// Set the min/max values for port.
			port.Minimum = IPEndPoint.MinPort;
			port.Maximum = IPEndPoint.MaxPort;

			// Set up the event handlers to watch for iFolder creates/deletes.
/*			subscriber = new EventSubscriber();
			subscriber.NodeChanged += new NodeEventHandler(subscriber_NodeChanged);
			subscriber.NodeCreated += new NodeEventHandler(subscriber_NodeCreated);
			subscriber.NodeDeleted += new NodeEventHandler(subscriber_NodeDeleted);

			ht = new Hashtable();

			try
			{
				manager = iFolderManager.Connect();
			}
			catch (SimiasException e)
			{
				e.LogFatal();
			}
			catch (Exception e)
			{
				logger.Fatal(e, "Fatal error initializing");
			}*/
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
			this.defaultInterval = new System.Windows.Forms.NumericUpDown();
			this.displayConfirmation = new System.Windows.Forms.CheckBox();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
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
			this.menuDecline = new System.Windows.Forms.MenuItem();
			this.menuDelete = new System.Windows.Forms.MenuItem();
			this.menuSeparator1 = new System.Windows.Forms.MenuItem();
			this.menuShare = new System.Windows.Forms.MenuItem();
			this.menuResolve = new System.Windows.Forms.MenuItem();
			this.menuSyncNow = new System.Windows.Forms.MenuItem();
			this.menuRevert = new System.Windows.Forms.MenuItem();
			this.menuSeparator2 = new System.Windows.Forms.MenuItem();
			this.menuProperties = new System.Windows.Forms.MenuItem();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.autoSync = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
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
			this.userName = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.label18 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.gaugeChart1 = new Novell.Forms.Controls.GaugeChart();
			this.label16 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.totalSpace = new System.Windows.Forms.Label();
			this.usedSpace = new System.Windows.Forms.Label();
			this.freeSpace = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.enterpriseDescription = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.enterpriseName = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.banner = new System.Windows.Forms.PictureBox();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuAction = new System.Windows.Forms.MenuItem();
			this.menuActionCreate = new System.Windows.Forms.MenuItem();
			this.menuActionSeparator1 = new System.Windows.Forms.MenuItem();
			this.menuActionAccept = new System.Windows.Forms.MenuItem();
			this.menuActionDecline = new System.Windows.Forms.MenuItem();
			this.menuActionSeparator2 = new System.Windows.Forms.MenuItem();
			this.menuActionDelete = new System.Windows.Forms.MenuItem();
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
			this.defaultInterval.Increment = new System.Decimal(new int[] {
																			  5,
																			  0,
																			  0,
																			  0});
			this.defaultInterval.Location = new System.Drawing.Point(152, 78);
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
			this.defaultInterval.Size = new System.Drawing.Size(64, 20);
			this.defaultInterval.TabIndex = 2;
			// 
			// displayConfirmation
			// 
			this.displayConfirmation.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.displayConfirmation.Location = new System.Drawing.Point(16, 48);
			this.displayConfirmation.Name = "displayConfirmation";
			this.displayConfirmation.Size = new System.Drawing.Size(368, 24);
			this.displayConfirmation.TabIndex = 1;
			this.displayConfirmation.Text = "Show &confirmation dialog when creating iFolders.";
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ok.Location = new System.Drawing.Point(288, 496);
			this.ok.Name = "ok";
			this.ok.TabIndex = 5;
			this.ok.Text = "OK";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancel.Location = new System.Drawing.Point(368, 496);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 6;
			this.cancel.Text = "Cancel";
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(224, 80);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 16);
			this.label2.TabIndex = 3;
			this.label2.Text = "seconds";
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Controls.Add(this.tabPage5);
			this.tabControl1.Location = new System.Drawing.Point(8, 72);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(434, 416);
			this.tabControl1.TabIndex = 8;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.create);
			this.tabPage1.Controls.Add(this.iFolderView);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(426, 390);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "iFolders";
			// 
			// create
			// 
			this.create.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.create.Location = new System.Drawing.Point(341, 360);
			this.create.Name = "create";
			this.create.TabIndex = 1;
			this.create.Text = "Create";
			this.create.Click += new System.EventHandler(this.menuCreate_Click);
			// 
			// iFolderView
			// 
			this.iFolderView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						  this.columnHeader1,
																						  this.columnHeader4,
																						  this.columnHeader5});
			this.iFolderView.ContextMenu = this.contextMenu1;
			this.iFolderView.FullRowSelect = true;
			this.iFolderView.HideSelection = false;
			this.iFolderView.Location = new System.Drawing.Point(8, 16);
			this.iFolderView.MultiSelect = false;
			this.iFolderView.Name = "iFolderView";
			this.iFolderView.Size = new System.Drawing.Size(408, 336);
			this.iFolderView.TabIndex = 0;
			this.iFolderView.View = System.Windows.Forms.View.Details;
			this.iFolderView.DoubleClick += new System.EventHandler(this.menuOpen_Click);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 100;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Location";
			this.columnHeader4.Width = 240;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Status";
			this.columnHeader5.Width = 64;
			// 
			// contextMenu1
			// 
			this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuOpen,
																						 this.menuCreate,
																						 this.menuRefresh,
																						 this.menuAccept,
																						 this.menuDecline,
																						 this.menuDelete,
																						 this.menuSeparator1,
																						 this.menuShare,
																						 this.menuResolve,
																						 this.menuSyncNow,
																						 this.menuRevert,
																						 this.menuSeparator2,
																						 this.menuProperties});
			this.contextMenu1.Popup += new System.EventHandler(this.contextMenu1_Popup);
			// 
			// menuOpen
			// 
			this.menuOpen.Index = 0;
			this.menuOpen.Text = "&Open";
			this.menuOpen.Visible = false;
			this.menuOpen.Click += new System.EventHandler(this.menuOpen_Click);
			// 
			// menuCreate
			// 
			this.menuCreate.Index = 1;
			this.menuCreate.Text = "&Create iFolder";
			this.menuCreate.Click += new System.EventHandler(this.menuCreate_Click);
			// 
			// menuRefresh
			// 
			this.menuRefresh.Index = 2;
			this.menuRefresh.Text = "&Refresh list";
			this.menuRefresh.Click += new System.EventHandler(this.menuRefresh_Click);
			// 
			// menuAccept
			// 
			this.menuAccept.Index = 3;
			this.menuAccept.Text = "Setup iFolder";
			this.menuAccept.Visible = false;
			this.menuAccept.Click += new System.EventHandler(this.menuAccept_Click);
			// 
			// menuDecline
			// 
			this.menuDecline.Index = 4;
			this.menuDecline.Text = "Decline";
			this.menuDecline.Visible = false;
			this.menuDecline.Click += new System.EventHandler(this.menuDecline_Click);
			// 
			// menuDelete
			// 
			this.menuDelete.Index = 5;
			this.menuDelete.Text = "Remove iFolder";
			this.menuDelete.Visible = false;
			this.menuDelete.Click += new System.EventHandler(this.menuDelete_Click);
			// 
			// menuSeparator1
			// 
			this.menuSeparator1.Index = 6;
			this.menuSeparator1.Text = "-";
			this.menuSeparator1.Visible = false;
			// 
			// menuShare
			// 
			this.menuShare.Index = 7;
			this.menuShare.Text = "&Share with...";
			this.menuShare.Visible = false;
			this.menuShare.Click += new System.EventHandler(this.menuShare_Click);
			// 
			// menuResolve
			// 
			this.menuResolve.Index = 8;
			this.menuResolve.Text = "Resolve conflicts";
			this.menuResolve.Visible = false;
			this.menuResolve.Click += new System.EventHandler(this.menuResolve_Click);
			// 
			// menuSyncNow
			// 
			this.menuSyncNow.Index = 9;
			this.menuSyncNow.Text = "Sync now";
			this.menuSyncNow.Visible = false;
			this.menuSyncNow.Click += new System.EventHandler(this.menuSyncNow_Click);
			// 
			// menuRevert
			// 
			this.menuRevert.Index = 10;
			this.menuRevert.Text = "Revert to a normal folder";
			this.menuRevert.Visible = false;
			this.menuRevert.Click += new System.EventHandler(this.menuRevert_Click);
			// 
			// menuSeparator2
			// 
			this.menuSeparator2.Index = 11;
			this.menuSeparator2.Text = "-";
			this.menuSeparator2.Visible = false;
			// 
			// menuProperties
			// 
			this.menuProperties.Index = 12;
			this.menuProperties.Text = "Properties";
			this.menuProperties.Visible = false;
			this.menuProperties.Click += new System.EventHandler(this.menuProperties_Click);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.groupBox1);
			this.tabPage2.Controls.Add(this.groupBox3);
			this.tabPage2.Controls.Add(this.groupBox5);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(426, 390);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Preferences";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.defaultInterval);
			this.groupBox1.Controls.Add(this.autoSync);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(16, 112);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(398, 112);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Synchronization";
			// 
			// autoSync
			// 
			this.autoSync.Checked = true;
			this.autoSync.CheckState = System.Windows.Forms.CheckState.Checked;
			this.autoSync.Enabled = false;
			this.autoSync.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.autoSync.Location = new System.Drawing.Point(16, 80);
			this.autoSync.Name = "autoSync";
			this.autoSync.Size = new System.Drawing.Size(160, 16);
			this.autoSync.TabIndex = 1;
			this.autoSync.Text = "S&ync to host every:";
			this.autoSync.CheckedChanged += new System.EventHandler(this.autoSync_CheckedChanged);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(376, 48);
			this.label3.TabIndex = 0;
			this.label3.Text = "This will set the default sync setting for all iFolders.  You can change the sync" +
				" setting for an individual iFolder from the iFolder\'s Property dialog.";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.autoStart);
			this.groupBox3.Controls.Add(this.displayConfirmation);
			this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox3.Location = new System.Drawing.Point(16, 16);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(398, 80);
			this.groupBox3.TabIndex = 0;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Application";
			// 
			// autoStart
			// 
			this.autoStart.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.autoStart.Location = new System.Drawing.Point(16, 20);
			this.autoStart.Name = "autoStart";
			this.autoStart.Size = new System.Drawing.Size(368, 24);
			this.autoStart.TabIndex = 0;
			this.autoStart.Text = "&Startup iFolder at login.";
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.proxy);
			this.groupBox5.Controls.Add(this.port);
			this.groupBox5.Controls.Add(this.useProxy);
			this.groupBox5.Controls.Add(this.label7);
			this.groupBox5.Controls.Add(this.label4);
			this.groupBox5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox5.Location = new System.Drawing.Point(16, 240);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(398, 88);
			this.groupBox5.TabIndex = 2;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Proxy";
			// 
			// proxy
			// 
			this.proxy.Enabled = false;
			this.proxy.Location = new System.Drawing.Point(88, 56);
			this.proxy.Name = "proxy";
			this.proxy.Size = new System.Drawing.Size(168, 20);
			this.proxy.TabIndex = 2;
			this.proxy.Text = "";
			// 
			// port
			// 
			this.port.Enabled = false;
			this.port.Location = new System.Drawing.Point(312, 56);
			this.port.Name = "port";
			this.port.Size = new System.Drawing.Size(72, 20);
			this.port.TabIndex = 4;
			// 
			// useProxy
			// 
			this.useProxy.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useProxy.Location = new System.Drawing.Point(16, 24);
			this.useProxy.Name = "useProxy";
			this.useProxy.Size = new System.Drawing.Size(360, 16);
			this.useProxy.TabIndex = 0;
			this.useProxy.Text = "Use a proxy to sync iFolders.";
			this.useProxy.CheckedChanged += new System.EventHandler(this.useProxy_CheckedChanged);
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(280, 58);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(56, 16);
			this.label7.TabIndex = 3;
			this.label7.Text = "Port:";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 58);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(100, 16);
			this.label4.TabIndex = 1;
			this.label4.Text = "Proxy host:";
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.clearLog);
			this.tabPage3.Controls.Add(this.saveLog);
			this.tabPage3.Controls.Add(this.log);
			this.tabPage3.Controls.Add(this.label6);
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(426, 390);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Log";
			// 
			// clearLog
			// 
			this.clearLog.Enabled = false;
			this.clearLog.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.clearLog.Location = new System.Drawing.Point(344, 352);
			this.clearLog.Name = "clearLog";
			this.clearLog.TabIndex = 3;
			this.clearLog.Text = "&Clear";
			// 
			// saveLog
			// 
			this.saveLog.Enabled = false;
			this.saveLog.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.saveLog.Location = new System.Drawing.Point(264, 352);
			this.saveLog.Name = "saveLog";
			this.saveLog.TabIndex = 2;
			this.saveLog.Text = "&Save";
			// 
			// log
			// 
			this.log.HorizontalScrollbar = true;
			this.log.Location = new System.Drawing.Point(8, 48);
			this.log.Name = "log";
			this.log.ScrollAlwaysVisible = true;
			this.log.Size = new System.Drawing.Size(408, 290);
			this.log.TabIndex = 1;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(8, 16);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(296, 16);
			this.label6.TabIndex = 0;
			this.label6.Text = "This log shows current iFolder activity.";
			// 
			// tabPage5
			// 
			this.tabPage5.Controls.Add(this.userName);
			this.tabPage5.Controls.Add(this.label10);
			this.tabPage5.Controls.Add(this.groupBox6);
			this.tabPage5.Controls.Add(this.enterpriseDescription);
			this.tabPage5.Controls.Add(this.label9);
			this.tabPage5.Controls.Add(this.enterpriseName);
			this.tabPage5.Controls.Add(this.label8);
			this.tabPage5.Location = new System.Drawing.Point(4, 22);
			this.tabPage5.Name = "tabPage5";
			this.tabPage5.Size = new System.Drawing.Size(426, 390);
			this.tabPage5.TabIndex = 3;
			this.tabPage5.Text = "Server";
			// 
			// userName
			// 
			this.userName.Location = new System.Drawing.Point(152, 24);
			this.userName.Name = "userName";
			this.userName.Size = new System.Drawing.Size(264, 16);
			this.userName.TabIndex = 1;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(16, 24);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(120, 16);
			this.label10.TabIndex = 0;
			this.label10.Text = "User name:";
			// 
			// groupBox6
			// 
			this.groupBox6.Controls.Add(this.label18);
			this.groupBox6.Controls.Add(this.label17);
			this.groupBox6.Controls.Add(this.gaugeChart1);
			this.groupBox6.Controls.Add(this.label16);
			this.groupBox6.Controls.Add(this.label15);
			this.groupBox6.Controls.Add(this.label14);
			this.groupBox6.Controls.Add(this.totalSpace);
			this.groupBox6.Controls.Add(this.usedSpace);
			this.groupBox6.Controls.Add(this.freeSpace);
			this.groupBox6.Controls.Add(this.label13);
			this.groupBox6.Controls.Add(this.label12);
			this.groupBox6.Controls.Add(this.label11);
			this.groupBox6.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox6.Location = new System.Drawing.Point(16, 176);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(400, 120);
			this.groupBox6.TabIndex = 6;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "Disk space";
			// 
			// label18
			// 
			this.label18.Location = new System.Drawing.Point(328, 80);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(64, 16);
			this.label18.TabIndex = 11;
			this.label18.Text = "Empty";
			// 
			// label17
			// 
			this.label17.Location = new System.Drawing.Point(328, 24);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(56, 16);
			this.label17.TabIndex = 10;
			this.label17.Text = "Full";
			// 
			// gaugeChart1
			// 
			this.gaugeChart1.Location = new System.Drawing.Point(304, 24);
			this.gaugeChart1.Name = "gaugeChart1";
			this.gaugeChart1.Size = new System.Drawing.Size(16, 72);
			this.gaugeChart1.TabIndex = 9;
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(248, 80);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(24, 16);
			this.label16.TabIndex = 8;
			this.label16.Text = "MB";
			// 
			// label15
			// 
			this.label15.Location = new System.Drawing.Point(248, 56);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(24, 16);
			this.label15.TabIndex = 5;
			this.label15.Text = "MB";
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(248, 32);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(24, 16);
			this.label14.TabIndex = 2;
			this.label14.Text = "MB";
			// 
			// totalSpace
			// 
			this.totalSpace.Location = new System.Drawing.Point(160, 80);
			this.totalSpace.Name = "totalSpace";
			this.totalSpace.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.totalSpace.Size = new System.Drawing.Size(80, 16);
			this.totalSpace.TabIndex = 7;
			// 
			// usedSpace
			// 
			this.usedSpace.Location = new System.Drawing.Point(160, 56);
			this.usedSpace.Name = "usedSpace";
			this.usedSpace.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.usedSpace.Size = new System.Drawing.Size(80, 16);
			this.usedSpace.TabIndex = 4;
			// 
			// freeSpace
			// 
			this.freeSpace.Location = new System.Drawing.Point(160, 32);
			this.freeSpace.Name = "freeSpace";
			this.freeSpace.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.freeSpace.Size = new System.Drawing.Size(80, 16);
			this.freeSpace.TabIndex = 1;
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(16, 80);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(136, 16);
			this.label13.TabIndex = 6;
			this.label13.Text = "Total space on server:";
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(16, 56);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(136, 16);
			this.label12.TabIndex = 3;
			this.label12.Text = "Used space on server:";
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(16, 32);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(136, 16);
			this.label11.TabIndex = 0;
			this.label11.Text = "Free space on server:";
			// 
			// enterpriseDescription
			// 
			this.enterpriseDescription.Location = new System.Drawing.Point(152, 72);
			this.enterpriseDescription.Name = "enterpriseDescription";
			this.enterpriseDescription.Size = new System.Drawing.Size(264, 48);
			this.enterpriseDescription.TabIndex = 5;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(16, 72);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(120, 16);
			this.label9.TabIndex = 4;
			this.label9.Text = "Server description:";
			// 
			// enterpriseName
			// 
			this.enterpriseName.Location = new System.Drawing.Point(152, 48);
			this.enterpriseName.Name = "enterpriseName";
			this.enterpriseName.Size = new System.Drawing.Size(264, 16);
			this.enterpriseName.TabIndex = 3;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(16, 48);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(120, 16);
			this.label8.TabIndex = 2;
			this.label8.Text = "iFolder server:";
			// 
			// banner
			// 
			this.banner.Dock = System.Windows.Forms.DockStyle.Top;
			this.banner.Location = new System.Drawing.Point(0, 0);
			this.banner.Name = "banner";
			this.banner.Size = new System.Drawing.Size(450, 65);
			this.banner.TabIndex = 9;
			this.banner.TabStop = false;
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuAction,
																					  this.menuView,
																					  this.menuHelp});
			// 
			// menuAction
			// 
			this.menuAction.Index = 0;
			this.menuAction.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					   this.menuActionCreate,
																					   this.menuActionSeparator1,
																					   this.menuActionAccept,
																					   this.menuActionDecline,
																					   this.menuActionSeparator2,
																					   this.menuActionDelete,
																					   this.menuActionOpen,
																					   this.menuActionShare,
																					   this.menuActionResolve,
																					   this.menuActionSync,
																					   this.menuActionRevert,
																					   this.menuActionProperties,
																					   this.menuItem4,
																					   this.menuExit});
			this.menuAction.Text = "iFolder";
			this.menuAction.Popup += new System.EventHandler(this.menuAction_Popup);
			// 
			// menuActionCreate
			// 
			this.menuActionCreate.Index = 0;
			this.menuActionCreate.Text = "Create";
			this.menuActionCreate.Click += new System.EventHandler(this.menuCreate_Click);
			// 
			// menuActionSeparator1
			// 
			this.menuActionSeparator1.Index = 1;
			this.menuActionSeparator1.Text = "-";
			// 
			// menuActionAccept
			// 
			this.menuActionAccept.Index = 2;
			this.menuActionAccept.Text = "Accept";
			this.menuActionAccept.Visible = false;
			this.menuActionAccept.Click += new System.EventHandler(this.menuAccept_Click);
			// 
			// menuActionDecline
			// 
			this.menuActionDecline.Index = 3;
			this.menuActionDecline.Text = "Decline";
			this.menuActionDecline.Visible = false;
			this.menuActionDecline.Click += new System.EventHandler(this.menuDecline_Click);
			// 
			// menuActionSeparator2
			// 
			this.menuActionSeparator2.Index = 4;
			this.menuActionSeparator2.Text = "-";
			this.menuActionSeparator2.Visible = false;
			// 
			// menuActionDelete
			// 
			this.menuActionDelete.Index = 5;
			this.menuActionDelete.Text = "Delete";
			this.menuActionDelete.Visible = false;
			this.menuActionDelete.Click += new System.EventHandler(this.menuDelete_Click);
			// 
			// menuActionOpen
			// 
			this.menuActionOpen.Enabled = false;
			this.menuActionOpen.Index = 6;
			this.menuActionOpen.Text = "Open";
			this.menuActionOpen.Click += new System.EventHandler(this.menuOpen_Click);
			// 
			// menuActionShare
			// 
			this.menuActionShare.Enabled = false;
			this.menuActionShare.Index = 7;
			this.menuActionShare.Text = "Share with...";
			this.menuActionShare.Click += new System.EventHandler(this.menuShare_Click);
			// 
			// menuActionResolve
			// 
			this.menuActionResolve.Index = 8;
			this.menuActionResolve.Text = "Resolve conflicts";
			this.menuActionResolve.Visible = false;
			this.menuActionResolve.Click += new System.EventHandler(this.menuResolve_Click);
			// 
			// menuActionSync
			// 
			this.menuActionSync.Enabled = false;
			this.menuActionSync.Index = 9;
			this.menuActionSync.Text = "Sync now";
			this.menuActionSync.Click += new System.EventHandler(this.menuSyncNow_Click);
			// 
			// menuActionRevert
			// 
			this.menuActionRevert.Enabled = false;
			this.menuActionRevert.Index = 10;
			this.menuActionRevert.Text = "Revert to a normal folder";
			this.menuActionRevert.Click += new System.EventHandler(this.menuRevert_Click);
			// 
			// menuActionProperties
			// 
			this.menuActionProperties.Enabled = false;
			this.menuActionProperties.Index = 11;
			this.menuActionProperties.Text = "Properties";
			this.menuActionProperties.Click += new System.EventHandler(this.menuProperties_Click);
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 12;
			this.menuItem4.Text = "-";
			// 
			// menuExit
			// 
			this.menuExit.Index = 13;
			this.menuExit.Text = "Exit";
			this.menuExit.Click += new System.EventHandler(this.menuFileExit_Click);
			// 
			// menuView
			// 
			this.menuView.Index = 1;
			this.menuView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuViewRefresh});
			this.menuView.Text = "View";
			this.menuView.Popup += new System.EventHandler(this.menuView_Popup);
			// 
			// menuViewRefresh
			// 
			this.menuViewRefresh.Enabled = false;
			this.menuViewRefresh.Index = 0;
			this.menuViewRefresh.Text = "Refresh";
			this.menuViewRefresh.Click += new System.EventHandler(this.menuRefresh_Click);
			// 
			// menuHelp
			// 
			this.menuHelp.Index = 2;
			this.menuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuHelpHelp,
																					 this.menuHelpAbout});
			this.menuHelp.Text = "Help";
			// 
			// menuHelpHelp
			// 
			this.menuHelpHelp.Index = 0;
			this.menuHelpHelp.Text = "Help";
			this.menuHelpHelp.Click += new System.EventHandler(this.menuHelpHelp_Click);
			// 
			// menuHelpAbout
			// 
			this.menuHelpAbout.Index = 1;
			this.menuHelpAbout.Text = "About";
			this.menuHelpAbout.Click += new System.EventHandler(this.menuHelpAbout_Click);
			// 
			// GlobalProperties
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(450, 523);
			this.Controls.Add(this.banner);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.KeyPreview = true;
			this.Menu = this.mainMenu1;
			this.Name = "GlobalProperties";
			this.Text = "iFolder";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GlobalProperties_KeyDown);
			this.Load += new System.EventHandler(this.GlobalProperties_Load);
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
		#endregion

		#region Public Methods
		/// <summary>
		/// Set the run value in the Windows registery.
		/// </summary>
		/// <param name="enable"><b>True</b> will set the run value.</param>
		static public void SetRunValue(bool enable)
		{
			RegistryKey runKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");

			if (enable)
			{
				runKey.SetValue(iFolderRun, Path.Combine(Application.StartupPath, "iFolderApp.exe"));
			}
			else
			{
				runKey.DeleteValue(iFolderRun, false);
			}
		}
		#endregion

		#region Private Methods
		private void addiFolderToListView(iFolder ifolder)
		{
			//lock (ht)
			{
				// Add only if it isn't already in the list.
				//if (ht[ifolder.ID] == null)
				{
					string[] items = new string[3];
					int imageIndex;

					items[0] = ifolder.Name;
					items[1] = ifolder.IsSubscription ? "" : ifolder.UnManagedPath;
					items[2] = stateToString(ifolder.State, ifolder.HasConflicts, out imageIndex);

					ListViewItem lvi = new ListViewItem(items, imageIndex);
					lvi.Tag = ifolder;
					iFolderView.Items.Add(lvi);

					// Add the listviewitem to the hashtable.
					//ht.Add(ifolder.ID, lvi);
				}
			}
		}

		private string stateToString(string state, bool conflicts, out int imageIndex)
		{
			string status;

			// TODO: Localize.
			switch (state)
			{
				case "Local":
					if (conflicts)
					{
						imageIndex = 2;
						status = "Has conflicts";
					}
					else
					{					
						imageIndex = 0;
						status = "OK";
					}
					break;
				case "Available":
					imageIndex = 1;
					status = "Available";
					break;
				case "WaitConnect":
					imageIndex = 1;
					status = "Waiting to connect";
					break;
				case "WaitSync":
					imageIndex = 1;
					status = "Waiting to sync";
					break;
				default:
					// TODO: what icon to use for unknown status?
					imageIndex = 1;
					status = "Unknown";
					break;
			}

			return status;
		}

		private bool isRunEnabled()
		{
			string run = null;

			try
			{
				RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
				run = (string)runKey.GetValue(iFolderRun);
			}
			catch
			{
				return false;
			}

			return (run != null);
		}

		private void refreshiFolders()
		{
			Cursor.Current = Cursors.WaitCursor;

			iFolderView.Items.Clear();
			iFolderView.SelectedItems.Clear();

/*			lock(ht)
			{
				ht.Clear();
			}*/

			iFolderView.BeginUpdate();

			try
			{
				iFolder[] ifolderArray = ifWebService.GetAlliFolders();
				foreach (iFolder ifolder in ifolderArray)
				{
					addiFolderToListView(ifolder);
				}
			}
			catch
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered while reading the iFolders.", "iFolder Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			iFolderView.EndUpdate();
			Cursor.Current = Cursors.Default;
		}

		private void invokeiFolderProperties(ListViewItem lvi, int activeTab)
		{
			new iFolderComponent().InvokeAdvancedDlg(Application.StartupPath, lvi.SubItems[1].Text, activeTab, true);
		}

		private void synciFolder(string iFolderID)
		{
			try
			{
				ifWebService.SynciFolderNow(iFolderID);
			}
			catch
			{
				// TODO: Localize.
				MessageBox.Show("An error was encountered while sending the sync command.");
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

			try
			{
				iFolderSettings ifSettings = ifWebService.GetSettings();
				currentUserID = ifSettings.CurrentUserID;
				displayConfirmation.Checked = ifSettings.DisplayConfirmation;
				if (ifSettings.HaveEnterprise)
				{
					ShowEnterpriseTab = true;
					iFolderUser ifolderUser = ifWebService.GetiFolderUser(ifSettings.CurrentUserID);
					userName.Text = ifolderUser.Name;
					enterpriseName.Text = ifSettings.EnterpriseName;
					enterpriseDescription.Text = ifSettings.EnterpriseDescription;

					// Get the disk space.
					DiskSpace diskSpace = ifWebService.GetUserDiskSpace(ifSettings.CurrentUserID);
					if (diskSpace.Limit != 0)
					{
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
						usedSpace.Text = freeSpace.Text = totalSpace.Text = "";
						gaugeChart1.Used = 0;
					}

					// Cause the gauge chart to be redrawn.
					gaugeChart1.Invalidate(true);
				}

				// Display the default sync interval.
				defaultInterval.Value = (decimal)ifWebService.GetDefaultSyncInterval();

				autoStart.Checked = isRunEnabled();

				refreshiFolders();

				useProxy.Checked = ifSettings.UseProxy;
				proxy.Text = ifSettings.ProxyHost;
				port.Value = (decimal)ifSettings.ProxyPort;
			}
			catch (WebException ex)
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered while reading iFolder data.");
			}
			catch (Exception ex)
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered while reading iFolder data.");
			}
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				// Save the default sync interval.
				ifWebService.SetDefaultSyncInterval((int)defaultInterval.Value);

				// Save the auto start value.
				SetRunValue(autoStart.Checked);

				// Save the display confirmation setting.
				ifWebService.SetDisplayConfirmation(displayConfirmation.Checked);

				// Save the proxy settings.
				if (useProxy.Checked)
				{
					ifWebService.SetupProxy(proxy.Text, (int)port.Value);
				}
				else
				{
					ifWebService.RemoveProxy();
				}
			}
			catch (WebException ex)
			{
				// TODO: change message displayed
				MessageBox.Show(ex.Message);
			}
			catch (Exception ex)
			{
				// TODO: change message displayed
				MessageBox.Show(ex.Message);
			}

			Cursor.Current = Cursors.Default;
		}

		private void menuFileExit_Click(object sender, System.EventArgs e)
		{
			this.ok_Click(this, e);

			this.Close();
		}

		private void menuAction_Popup(object sender, System.EventArgs e)
		{
			menuActionOpen.Enabled = menuActionShare.Enabled = menuActionSync.Enabled = 
				menuActionRevert.Enabled = menuActionProperties.Enabled = 
				(iFolderView.SelectedItems.Count == 1) && !((iFolder)iFolderView.SelectedItems[0].Tag).IsSubscription;

			// Enable/disable resolve menu item.
			menuActionResolve.Visible = (iFolderView.SelectedItems.Count == 1) && ((iFolder)iFolderView.SelectedItems[0].Tag).HasConflicts;

			/*menuActionAccept.Visible = menuActionDecline.Visible = menuActionDelete.Visible = menuActionSeparator2.Visible = 
				(iFolderView.SelectedItems.Count == 1) && ((iFolder)iFolderView.SelectedItems[0].Tag).IsSubscription;*/
		}

		private void menuView_Popup(object sender, System.EventArgs e)
		{
			menuViewRefresh.Enabled = tabControl1.SelectedTab.Equals(tabPage1);
		}

		private void menuHelpHelp_Click(object sender, System.EventArgs e)
		{
			// TODO - need to use locale-specific path
			string helpPath = Path.Combine(Application.StartupPath, @"help\en\doc\user\data\front.html");

			try
			{
				Process.Start(helpPath);
			}
			catch (Exception ex)
			{
				// TODO: Localize
				MessageBox.Show("Unable to open help file: \n" + helpPath, "Help File Not Found");
			}
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
				(iFolderView.SelectedItems.Count == 1) && !((iFolder)iFolderView.SelectedItems[0].Tag).IsSubscription;

			menuResolve.Visible = (iFolderView.SelectedItems.Count == 1) && ((iFolder)iFolderView.SelectedItems[0].Tag).HasConflicts;
			menuRefresh.Visible = menuCreate.Visible = iFolderView.SelectedItems.Count == 0;

			menuAccept.Visible = /*menuDecline.Visible =*/ menuDelete.Visible = 
				(iFolderView.SelectedItems.Count == 1) && ((iFolder)iFolderView.SelectedItems[0].Tag).IsSubscription;
		}

		private void menuOpen_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = iFolderView.SelectedItems[0];
			iFolder ifolder = (iFolder)lvi.Tag;

			try
			{
				Process.Start(ifolder.UnManagedPath);
			}
			catch (Exception ex)
			{
				// TODO: Localize
				MessageBox.Show("Unable to open iFolder: " + ifolder.Name);
			}
		}

		private void menuRevert_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = iFolderView.SelectedItems[0];

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				iFolder ifolder = (iFolder)lvi.Tag;

				// Delete the iFolder.
				ifWebService.DeleteiFolder(ifolder.ID);

				// Notify the shell.
				Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, ifolder.UnManagedPath, IntPtr.Zero);

/*				lock (ht)
				{
					ht.Remove((string)lvi.Tag);
				}*/

				lvi.Remove();
			}
			catch (WebException ex)
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered while reverting the iFolder.");
			}
			catch (Exception ex)
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered while reverting the iFolder.");
			}

			Cursor.Current = Cursors.Default;
		}

		private void menuResolve_Click(object sender, System.EventArgs e)
		{
			new iFolderComponent().InvokeConflictResolverDlg(Application.StartupPath, iFolderView.SelectedItems[0].SubItems[1].Text);
		}

		private void menuShare_Click(object sender, System.EventArgs e)
		{
			invokeiFolderProperties(iFolderView.SelectedItems[0], 1);
		}

		private void menuSyncNow_Click(object sender, System.EventArgs e)
		{
			synciFolder(((iFolder)iFolderView.SelectedItems[0].Tag).ID);
		}

		private void menuProperties_Click(object sender, System.EventArgs e)
		{
			invokeiFolderProperties(iFolderView.SelectedItems[0], 0);
		}

		private void menuCreate_Click(object sender, System.EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

			// TODO: Localize
			folderBrowserDialog.Description = "Choose a folder to convert to an iFolder.";

			while (true)
			{
				if(folderBrowserDialog.ShowDialog() == DialogResult.OK)
				{
					try
					{
						if (ifWebService.CanBeiFolder(folderBrowserDialog.SelectedPath) && 
							((GlobalProperties.GetDriveType(Path.GetPathRoot(folderBrowserDialog.SelectedPath)) & DRIVE_REMOTE) != DRIVE_REMOTE))
						{
							// Create the iFolder.
							iFolder ifolder = ifWebService.CreateLocaliFolder(folderBrowserDialog.SelectedPath);

							// Notify the shell.
							Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, folderBrowserDialog.SelectedPath, IntPtr.Zero);

							// Add the iFolder to the listview.
							addiFolderToListView(ifolder);

							// Display the new iFolder intro dialog.
							new iFolderComponent().NewiFolderWizard(Application.StartupPath, folderBrowserDialog.SelectedPath);
							break;
						}
						else
						{
							MessageBox.Show("An invalid folder was specified");
						}
					}
					catch (WebException ex)
					{
						// TODO: Localize
						MessageBox.Show("An error was encountered while creating the iFolder.");
					}
					catch (Exception ex)
					{
						// TODO: Localize
						MessageBox.Show("An error was encountered while creating the iFolder.");
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
			iFolder ifolder = (iFolder)iFolderView.SelectedItems[0].Tag;

			AcceptInvitation acceptInvitation = new AcceptInvitation(ifWebService, ifolder);
			acceptInvitation.ShowDialog();
		}

		private void menuDecline_Click(object sender, System.EventArgs e)
		{
			// TODO:
			MessageBox.Show("This action is not yet implemented.");
		}

		private void menuDelete_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = iFolderView.SelectedItems[0];
			iFolder ifolder = (iFolder)lvi.Tag;

			try
			{
				ifWebService.RemoveSubscription(ifolder.Domain, ifolder.ID, currentUserID);
				lvi.Remove();
			}
			catch (WebException ex)
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered while removing the subscription.");
			}
			catch (Exception ex)
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered while removing the subscription.");
			}
		}
		#endregion

		#region Log Tab
		#endregion

		#region Preferences Tab
		private void useProxy_CheckedChanged(object sender, System.EventArgs e)
		{
			proxy.Enabled = port.Enabled = useProxy.Checked;
		}

		private void autoSync_CheckedChanged(object sender, System.EventArgs e)
		{
			defaultInterval.Enabled = autoSync.Checked;
		}
		#endregion

		#region Subscriber Handlers
/*		private void subscriber_NodeCreated(NodeEventArgs args)
		{
			try
			{
				iFolder ifolder = manager.GetiFolderById(args.ID);
				if (ifolder != null)
				{
					addiFolderToListView(ifolder);
				}
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				//logger.Debug(ex, "OnNodeCreated");
			}
		}

		private void subscriber_NodeDeleted(NodeEventArgs args)
		{
			lock (ht)
			{
				ListViewItem lvi = (ListViewItem)ht[args.Node];
				if (lvi != null)
				{
					lvi.Remove();
					ht.Remove(args.Node);
				}
			}
		}

		private void subscriber_NodeChanged(NodeEventArgs args)
		{
			// TODO: implement this if needed.
		}*/
		#endregion

		#endregion

		internal static readonly uint DRIVE_REMOTE = 4;

		[DllImport("kernel32.dll")]
		internal static extern uint GetDriveType(string rootPathName);

		private void cancel_Click(object sender, System.EventArgs e)
		{
		
		}
	}
}
