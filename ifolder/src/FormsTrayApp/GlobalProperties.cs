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
using Microsoft.Win32;
using Simias;
using Simias.Sync;
using Simias.Service;
using Novell.iFolder;
using Novell.iFolder.iFolderCom;
using Novell.iFolder.Win32Util;

namespace Novell.iFolder.FormsTrayApp
{
	/// <summary>
	/// Summary description for GlobalProperties.
	/// </summary>
	public class GlobalProperties : System.Windows.Forms.Form
	{
		#region Class Members
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(FormsTrayApp));
		private const string iFolderRun = "iFolder";

		private Manager serviceManager = null;
		private iFolderManager manager = null;
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
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.PictureBox banner;
		private System.Windows.Forms.CheckBox autoStart;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListView iFolderView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button syncNow;
		private System.Windows.Forms.ListBox log;
		private System.Windows.Forms.Button saveLog;
		private System.Windows.Forms.Button clearLog;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem menuOpen;
		private System.Windows.Forms.MenuItem menuCreate;
		private System.Windows.Forms.Label objectCount;
		private System.Windows.Forms.Label byteCount;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.ListView services;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ContextMenu contextMenu2;
		private System.Windows.Forms.MenuItem menuStart;
		private System.Windows.Forms.MenuItem menuPause;
		private System.Windows.Forms.MenuItem menuStop;
		private System.Windows.Forms.MenuItem menuRestart;
		private System.Windows.Forms.MenuItem menuShare;
		private System.Windows.Forms.MenuItem menuRevert;
		private System.Windows.Forms.MenuItem menuProperties;
		private System.Windows.Forms.MenuItem menuRefresh;
		private System.Windows.Forms.MenuItem menuSeparator1;
		private System.Windows.Forms.MenuItem menuSeparator2;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		/// <summary>
		/// Instantiates a GlobalProperties object.
		/// </summary>
		public GlobalProperties()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

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
			}
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
			this.label1 = new System.Windows.Forms.Label();
			this.defaultInterval = new System.Windows.Forms.NumericUpDown();
			this.displayConfirmation = new System.Windows.Forms.CheckBox();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.objectCount = new System.Windows.Forms.Label();
			this.byteCount = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.iFolderView = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.menuOpen = new System.Windows.Forms.MenuItem();
			this.menuCreate = new System.Windows.Forms.MenuItem();
			this.menuRefresh = new System.Windows.Forms.MenuItem();
			this.menuSeparator1 = new System.Windows.Forms.MenuItem();
			this.menuRevert = new System.Windows.Forms.MenuItem();
			this.menuShare = new System.Windows.Forms.MenuItem();
			this.menuSeparator2 = new System.Windows.Forms.MenuItem();
			this.menuProperties = new System.Windows.Forms.MenuItem();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.autoSync = new System.Windows.Forms.CheckBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.autoStart = new System.Windows.Forms.CheckBox();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.clearLog = new System.Windows.Forms.Button();
			this.saveLog = new System.Windows.Forms.Button();
			this.log = new System.Windows.Forms.ListBox();
			this.syncNow = new System.Windows.Forms.Button();
			this.label6 = new System.Windows.Forms.Label();
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.services = new System.Windows.Forms.ListView();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.contextMenu2 = new System.Windows.Forms.ContextMenu();
			this.menuStart = new System.Windows.Forms.MenuItem();
			this.menuPause = new System.Windows.Forms.MenuItem();
			this.menuStop = new System.Windows.Forms.MenuItem();
			this.menuRestart = new System.Windows.Forms.MenuItem();
			this.banner = new System.Windows.Forms.PictureBox();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).BeginInit();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.tabPage4.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 80);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(184, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Sync to host every:";
			// 
			// defaultInterval
			// 
			this.defaultInterval.Increment = new System.Decimal(new int[] {
																			  5,
																			  0,
																			  0,
																			  0});
			this.defaultInterval.Location = new System.Drawing.Point(144, 80);
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
			this.displayConfirmation.Text = "Display iFolder creation confirmation.";
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Location = new System.Drawing.Point(288, 432);
			this.ok.Name = "ok";
			this.ok.TabIndex = 5;
			this.ok.Text = "OK";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(368, 432);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 6;
			this.cancel.Text = "Cancel";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(216, 80);
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
			this.tabControl1.Controls.Add(this.tabPage4);
			this.tabControl1.Location = new System.Drawing.Point(8, 65);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(434, 359);
			this.tabControl1.TabIndex = 8;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.groupBox4);
			this.tabPage1.Controls.Add(this.iFolderView);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(426, 333);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "iFolders";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.objectCount);
			this.groupBox4.Controls.Add(this.byteCount);
			this.groupBox4.Controls.Add(this.label5);
			this.groupBox4.Controls.Add(this.label4);
			this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox4.Location = new System.Drawing.Point(8, 240);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(408, 72);
			this.groupBox4.TabIndex = 1;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Synchronization";
			// 
			// objectCount
			// 
			this.objectCount.Location = new System.Drawing.Point(296, 48);
			this.objectCount.Name = "objectCount";
			this.objectCount.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.objectCount.Size = new System.Drawing.Size(100, 16);
			this.objectCount.TabIndex = 3;
			// 
			// byteCount
			// 
			this.byteCount.Location = new System.Drawing.Point(296, 24);
			this.byteCount.Name = "byteCount";
			this.byteCount.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.byteCount.Size = new System.Drawing.Size(100, 16);
			this.byteCount.TabIndex = 1;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 48);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(272, 16);
			this.label5.TabIndex = 2;
			this.label5.Text = "Files/folders to synchronize:";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 24);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(272, 16);
			this.label4.TabIndex = 0;
			this.label4.Text = "Amount to upload:";
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
			this.iFolderView.Size = new System.Drawing.Size(408, 208);
			this.iFolderView.TabIndex = 0;
			this.iFolderView.View = System.Windows.Forms.View.Details;
			this.iFolderView.DoubleClick += new System.EventHandler(this.iFolderView_DoubleClick);
			this.iFolderView.SelectedIndexChanged += new System.EventHandler(this.iFolderView_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 100;
			// 
			// contextMenu1
			// 
			this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuOpen,
																						 this.menuCreate,
																						 this.menuRefresh,
																						 this.menuSeparator1,
																						 this.menuRevert,
																						 this.menuShare,
																						 this.menuSeparator2,
																						 this.menuProperties});
			this.contextMenu1.Popup += new System.EventHandler(this.contextMenu1_Popup);
			// 
			// menuOpen
			// 
			this.menuOpen.Index = 0;
			this.menuOpen.Text = "&Open...";
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
			// menuSeparator1
			// 
			this.menuSeparator1.Index = 3;
			this.menuSeparator1.Text = "-";
			this.menuSeparator1.Visible = false;
			// 
			// menuRevert
			// 
			this.menuRevert.Index = 4;
			this.menuRevert.Text = "Revert to a normal folder";
			this.menuRevert.Visible = false;
			this.menuRevert.Click += new System.EventHandler(this.menuRevert_Click);
			// 
			// menuShare
			// 
			this.menuShare.Index = 5;
			this.menuShare.Text = "&Share with...";
			this.menuShare.Visible = false;
			this.menuShare.Click += new System.EventHandler(this.menuShare_Click);
			// 
			// menuSeparator2
			// 
			this.menuSeparator2.Index = 6;
			this.menuSeparator2.Text = "-";
			this.menuSeparator2.Visible = false;
			// 
			// menuProperties
			// 
			this.menuProperties.Index = 7;
			this.menuProperties.Text = "Properties...";
			this.menuProperties.Visible = false;
			this.menuProperties.Click += new System.EventHandler(this.menuProperties_Click);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.groupBox1);
			this.tabPage2.Controls.Add(this.groupBox3);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(426, 333);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Preferences";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.groupBox2);
			this.groupBox1.Controls.Add(this.autoSync);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(16, 128);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(398, 184);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Synchronization";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.defaultInterval);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(16, 48);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(366, 112);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Synchronize to host";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(336, 48);
			this.label3.TabIndex = 0;
			this.label3.Text = "This value sets the default value for how often the host of an iFolder will be co" +
				"ntacted  to sync files.";
			// 
			// autoSync
			// 
			this.autoSync.Checked = true;
			this.autoSync.CheckState = System.Windows.Forms.CheckState.Checked;
			this.autoSync.Enabled = false;
			this.autoSync.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.autoSync.Location = new System.Drawing.Point(16, 24);
			this.autoSync.Name = "autoSync";
			this.autoSync.Size = new System.Drawing.Size(368, 16);
			this.autoSync.TabIndex = 0;
			this.autoSync.Text = "Automatic sync";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.autoStart);
			this.groupBox3.Controls.Add(this.displayConfirmation);
			this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox3.Location = new System.Drawing.Point(16, 24);
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
			this.autoStart.Text = "Startup iFolder at login.";
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.clearLog);
			this.tabPage3.Controls.Add(this.saveLog);
			this.tabPage3.Controls.Add(this.log);
			this.tabPage3.Controls.Add(this.syncNow);
			this.tabPage3.Controls.Add(this.label6);
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(426, 333);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Log";
			// 
			// clearLog
			// 
			this.clearLog.Enabled = false;
			this.clearLog.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.clearLog.Location = new System.Drawing.Point(88, 288);
			this.clearLog.Name = "clearLog";
			this.clearLog.TabIndex = 4;
			this.clearLog.Text = "Clear";
			// 
			// saveLog
			// 
			this.saveLog.Enabled = false;
			this.saveLog.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.saveLog.Location = new System.Drawing.Point(8, 288);
			this.saveLog.Name = "saveLog";
			this.saveLog.TabIndex = 3;
			this.saveLog.Text = "Save...";
			// 
			// log
			// 
			this.log.HorizontalScrollbar = true;
			this.log.Location = new System.Drawing.Point(8, 48);
			this.log.Name = "log";
			this.log.ScrollAlwaysVisible = true;
			this.log.Size = new System.Drawing.Size(408, 225);
			this.log.TabIndex = 2;
			// 
			// syncNow
			// 
			this.syncNow.Enabled = false;
			this.syncNow.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.syncNow.Location = new System.Drawing.Point(320, 12);
			this.syncNow.Name = "syncNow";
			this.syncNow.Size = new System.Drawing.Size(96, 23);
			this.syncNow.TabIndex = 1;
			this.syncNow.Text = "Sync now";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(8, 16);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(296, 16);
			this.label6.TabIndex = 0;
			this.label6.Text = "This log shows current iFolder activity.";
			// 
			// tabPage4
			// 
			this.tabPage4.Controls.Add(this.services);
			this.tabPage4.Location = new System.Drawing.Point(4, 22);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Size = new System.Drawing.Size(426, 333);
			this.tabPage4.TabIndex = 3;
			this.tabPage4.Text = "Services";
			// 
			// services
			// 
			this.services.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					   this.columnHeader2,
																					   this.columnHeader3});
			this.services.ContextMenu = this.contextMenu2;
			this.services.FullRowSelect = true;
			this.services.Location = new System.Drawing.Point(8, 16);
			this.services.MultiSelect = false;
			this.services.Name = "services";
			this.services.Size = new System.Drawing.Size(408, 208);
			this.services.TabIndex = 0;
			this.services.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Service";
			this.columnHeader2.Width = 209;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Status";
			this.columnHeader3.Width = 195;
			// 
			// contextMenu2
			// 
			this.contextMenu2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuStart,
																						 this.menuPause,
																						 this.menuStop,
																						 this.menuRestart});
			this.contextMenu2.Popup += new System.EventHandler(this.contextMenu2_Popup);
			// 
			// menuStart
			// 
			this.menuStart.Enabled = false;
			this.menuStart.Index = 0;
			this.menuStart.Text = "Start";
			this.menuStart.Click += new System.EventHandler(this.menuStart_Click);
			// 
			// menuPause
			// 
			this.menuPause.Enabled = false;
			this.menuPause.Index = 1;
			this.menuPause.Text = "Pause";
			// 
			// menuStop
			// 
			this.menuStop.Enabled = false;
			this.menuStop.Index = 2;
			this.menuStop.Text = "Stop";
			this.menuStop.Click += new System.EventHandler(this.menuStop_Click);
			// 
			// menuRestart
			// 
			this.menuRestart.Enabled = false;
			this.menuRestart.Index = 3;
			this.menuRestart.Text = "Restart";
			this.menuRestart.Click += new System.EventHandler(this.menuRestart_Click);
			// 
			// banner
			// 
			this.banner.Location = new System.Drawing.Point(0, 0);
			this.banner.Name = "banner";
			this.banner.Size = new System.Drawing.Size(450, 65);
			this.banner.TabIndex = 9;
			this.banner.TabStop = false;
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
			// GlobalProperties
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(450, 464);
			this.Controls.Add(this.banner);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "GlobalProperties";
			this.Text = "Global iFolder Properties";
			this.Load += new System.EventHandler(this.GlobalProperties_Load);
			((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).EndInit();
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
			this.tabPage4.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		public Manager ServiceManager
		{
			set
			{
				serviceManager = value;
			}
		}
		#endregion

		#region Private Methods
		private void AddiFolderToListView(iFolder ifolder)
		{
			string status = ifolder.HasCollisions() ? "Conflicts exist" : "OK";
			ListViewItem lvi = new ListViewItem(new string[] {ifolder.Name, ifolder.LocalPath, status}, 0);
			lvi.Tag = ifolder.ID;
			iFolderView.Items.Add(lvi);
		}

		private bool IsRunEnabled()
		{
			RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
			string run = (string)runKey.GetValue(iFolderRun);
			return (run != null);
		}

		static public void SetRunValue(bool enable)
		{
			RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

			if (enable)
			{
				runKey.SetValue(iFolderRun, Path.Combine(Application.StartupPath, "iFolderApp.exe"));
			}
			else
			{
				runKey.DeleteValue(iFolderRun, false);
			}
		}

		private void refreshiFolders()
		{
			iFolderView.Items.Clear();
			iFolderView.SelectedItems.Clear();

			iFolderView.BeginUpdate();

			foreach (iFolder ifolder in manager)
			{
				AddiFolderToListView(ifolder);
			}

			iFolderView.EndUpdate();
		}

		private void invokeiFolderProperties(ListViewItem lvi, string activeTab)
		{
			iFolder ifolder = null;

			try
			{
				ifolder = manager.GetiFolderById((string)lvi.Tag);

				string windowName = "Advanced iFolder Properties for " + Path.GetFileName(ifolder.LocalPath);

				// Search for existing window and bring it to foreground ...
				Win32Window win32Window = Win32Util.Win32Window.FindWindow(null, windowName);
				if (win32Window != null)
				{
					win32Window.BringWindowToTop();
				}
				else
				{
					iFolderAdvanced ifolderAdvanced = new iFolderAdvanced();
					ifolderAdvanced.Name = ifolder.LocalPath;
					ifolderAdvanced.Text = windowName;
					ifolderAdvanced.CurrentiFolder = ifolder;
					ifolderAdvanced.ActiveTab = activeTab;

					ifolderAdvanced.ShowDialog();
				}
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Sharing");
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
				this.iFolderView.SmallImageList = new ImageList();
				iFolderView.SmallImageList.Images.Add(new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_loaded.ico")));
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Loading graphics");
			}

			try
			{
				// Display the default sync interval.
				defaultInterval.Value = (decimal)manager.DefaultRefreshInterval;

				// Initialize displayConfirmation.
				Configuration config = new Configuration();
				string showWizard = config.Get("iFolderShell", "Show wizard", "true");
				displayConfirmation.Checked = showWizard == "true";

				autoStart.Checked = IsRunEnabled();

				refreshiFolders();

				foreach (ServiceCtl svc in serviceManager)
				{
					ListViewItem lvi = new ListViewItem(new string[] {
                                                                         svc.Name,
																		 svc.State.ToString()}, 0);
					lvi.Tag = svc;
					services.Items.Add(lvi);
				}
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Initializing");
			}
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				// Save the default sync interval.
				manager.DefaultRefreshInterval = (int)defaultInterval.Value;

				// Save the auto start value.
				SetRunValue(autoStart.Checked);

				Configuration config = new Configuration();
				if (displayConfirmation.Checked)
				{
					config.Set("iFolderShell", "Show wizard", "true");
				}
				else
				{
					config.Set("iFolderShell", "Show wizard", "false");
				}
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Saving settings");
			}

			Cursor.Current = Cursors.Default;
		}

		private void iFolderView_DoubleClick(object sender, System.EventArgs e)
		{
			menuOpen_Click(sender, e);
		}

		private void iFolderView_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (iFolderView.SelectedItems.Count == 1)
			{
				Cursor.Current = Cursors.WaitCursor;

				ListViewItem lvi = iFolderView.SelectedItems[0];
				try
				{
					// Get the sync node and byte counts.
					uint nodeCount;
					ulong bytesToSend;
					iFolder ifolder = manager.GetiFolderById((string)lvi.Tag);
					SyncSize.CalculateSendSize(ifolder, out nodeCount, out bytesToSend);
					objectCount.Text = nodeCount.ToString();
					byteCount.Text = bytesToSend.ToString();
				}
				catch (SimiasException ex)
				{
					ex.LogError();
				}
				catch (Exception ex)
				{
					logger.Debug(ex, "Selecting iFolder");
				}

				Cursor.Current = Cursors.Default;
			}
			else
			{
				objectCount.Text = byteCount.Text = "";
			}
		}

		private void contextMenu1_Popup(object sender, System.EventArgs e)
		{
			menuShare.Visible = menuProperties.Visible = menuRevert.Visible = 
				menuSeparator1.Visible = menuSeparator2.Visible = 
				menuOpen.Visible = iFolderView.SelectedItems.Count == 1;
			menuRefresh.Visible = menuCreate.Visible = iFolderView.SelectedItems.Count == 0;
		}

		private void menuOpen_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = iFolderView.SelectedItems[0];
			iFolder ifolder = null;

			try
			{
				ifolder = manager.GetiFolderById((string)lvi.Tag);
				Process.Start(ifolder.LocalPath);
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Opening iFolder");
				if (ifolder == null)
				{
					MessageBox.Show("The selected iFolder is no longer valid and will be removed from the list.");
					iFolderView.Items.Remove(lvi);
				}
			}
		}

		private void menuRevert_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = iFolderView.SelectedItems[0];

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				iFolder ifolder = manager.GetiFolderById((string)lvi.Tag);
				string path = ifolder.LocalPath;

				// Delete the iFolder.
				manager.DeleteiFolderById((string)lvi.Tag);

				// Notify the shell.
				Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, path, IntPtr.Zero);

				lvi.Remove();
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Reverting");
			}

			Cursor.Current = Cursors.Default;
		}

		private void menuShare_Click(object sender, System.EventArgs e)
		{
			invokeiFolderProperties(iFolderView.SelectedItems[0], "share");
		}

		private void menuProperties_Click(object sender, System.EventArgs e)
		{
			invokeiFolderProperties(iFolderView.SelectedItems[0], null);
		}

		private void menuCreate_Click(object sender, System.EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

			while (true)
			{
				if(folderBrowserDialog.ShowDialog() == DialogResult.OK)
				{
					try
					{
						if (manager.CanBeiFolder(folderBrowserDialog.SelectedPath))
						{
							// Create the iFolder.
							iFolder ifolder = manager.CreateiFolder(folderBrowserDialog.SelectedPath);

							// Notify the shell.
							Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, folderBrowserDialog.SelectedPath, IntPtr.Zero);

							// Add the iFolder to the listview.
							AddiFolderToListView(ifolder);
							break;
						}
						else
						{
							MessageBox.Show("An invalid folder was specified");
						}
					}
					catch (SimiasException ex)
					{
						ex.LogError();
					}
					catch (Exception ex)
					{
						logger.Debug(ex, "Creating iFolder");
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

		private void contextMenu2_Popup(object sender, System.EventArgs e)
		{
			if (services.SelectedItems.Count == 1)
			{
				menuStart.Visible = menuPause.Visible = menuStop.Visible = menuRestart.Visible = true;
				ListViewItem lvi = services.SelectedItems[0];
				switch (lvi.SubItems[1].Text)
				{
					case "Stopped":
						menuStart.Enabled = true;
						menuStop.Enabled = menuRestart.Enabled = false;
						break;
					case "Running":
						menuStart.Enabled = false;
						menuStop.Enabled = menuRestart.Enabled = true;
						break;
				}
			}
			else
			{
				menuStart.Visible = menuPause.Visible = menuStop.Visible = menuRestart.Visible = false;
			}
		}

		private void menuStart_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = services.SelectedItems[0];
			ServiceCtl svc = (ServiceCtl)lvi.Tag;

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				svc.Start();
			}
			catch{}

			lvi.SubItems[1].Text = svc.State.ToString();

			Cursor.Current = Cursors.Default;
		}

		private void menuRestart_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = services.SelectedItems[0];
			ServiceCtl svc = (ServiceCtl)lvi.Tag;

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				svc.Stop();
				lvi.SubItems[1].Text = svc.State.ToString();
				svc.Start();
			}
			catch{}

			lvi.SubItems[1].Text = svc.State.ToString();

			Cursor.Current = Cursors.Default;
		}

		private void menuStop_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = services.SelectedItems[0];
			ServiceCtl svc = (ServiceCtl)lvi.Tag;

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				svc.Stop();
			}
			catch{}

			lvi.SubItems[1].Text = svc.State.ToString();

			Cursor.Current = Cursors.Default;
		}
		#endregion
	}
}
