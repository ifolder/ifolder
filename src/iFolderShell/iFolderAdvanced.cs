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
using System.Runtime.InteropServices;
using System.IO;
using Novell.iFolder;
using Novell.AddressBook;
using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;
using Simias.Policy;
using Novell.iFolder.FormsBookLib;

namespace Novell.iFolder.iFolderCom
{
	/// <summary>
	/// iFolder Advanced dialog.
	/// </summary>
	[ComVisible(false)]
	public class iFolderAdvanced : System.Windows.Forms.Form
	{
		// TODO: this is temporary.
		int x = 0;

		#region Class Members
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(iFolderAdvanced));
		private const double megaByte = 1048576;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Button apply;
		private System.Windows.Forms.ListView shareWith;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button remove;
		private System.Windows.Forms.Button add;

		private Hashtable subscrHT;
		private EventSubscriber subscriber;
		private POBox poBox;
		private Novell.AddressBook.Manager abManager;
		private Novell.AddressBook.AddressBook defaultAddressBook;
		private iFolder ifolder;
		private Member currentMember;
		private ListViewItem ownerLvi;
		private ListViewItem newOwnerLvi;
		private iFolderManager ifManager;
		private ArrayList removedList;
		private string loadPath;
//		private Control currentControl;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.HelpProvider helpProvider1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown syncInterval;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.CheckBox autoSync;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Label objectCount;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label byteCount;
		private System.Windows.Forms.LinkLabel conflicts;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.TabPage tabSharing;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.Button accept;
		private System.Windows.Forms.Button decline;
		private System.Windows.Forms.CheckBox setLimit;
		private System.Windows.Forms.TextBox limit;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label used;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label usedUnits;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label available;
		private System.Windows.Forms.Label availableUnits;
		private System.Windows.Forms.ComboBox ifolders;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label ifolderLabel;
		private System.Windows.Forms.Button open;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem menuFullControl;
		private System.Windows.Forms.MenuItem menuReadWrite;
		private System.Windows.Forms.MenuItem menuReadOnly;
		private System.Windows.Forms.Button access;
		private Novell.iFolder.Forms.Controls.GaugeChart gaugeChart;
		private System.ComponentModel.IContainer components;
		#endregion

		/// <summary>
		/// Constructs an iFolderAdvanced object.
		/// </summary>
		public iFolderAdvanced()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Connect to the address book manager
			if (abManager == null)
			{
				abManager = Novell.AddressBook.Manager.Connect();
			}

			apply.Enabled = remove.Enabled = accept.Enabled = decline.Enabled = false;

			syncInterval.TextChanged += new EventHandler(syncInterval_ValueChanged);
//			currentControl = this;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (subscriber != null)
				{
					subscriber.Dispose();
				}

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
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabGeneral = new System.Windows.Forms.TabPage();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label5 = new System.Windows.Forms.Label();
			this.syncInterval = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.autoSync = new System.Windows.Forms.CheckBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.objectCount = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.byteCount = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.gaugeChart = new Novell.iFolder.Forms.Controls.GaugeChart();
			this.limit = new System.Windows.Forms.TextBox();
			this.setLimit = new System.Windows.Forms.CheckBox();
			this.label10 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.availableUnits = new System.Windows.Forms.Label();
			this.available = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.usedUnits = new System.Windows.Forms.Label();
			this.used = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.tabSharing = new System.Windows.Forms.TabPage();
			this.access = new System.Windows.Forms.Button();
			this.decline = new System.Windows.Forms.Button();
			this.accept = new System.Windows.Forms.Button();
			this.add = new System.Windows.Forms.Button();
			this.remove = new System.Windows.Forms.Button();
			this.shareWith = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.menuFullControl = new System.Windows.Forms.MenuItem();
			this.menuReadWrite = new System.Windows.Forms.MenuItem();
			this.menuReadOnly = new System.Windows.Forms.MenuItem();
			this.conflicts = new System.Windows.Forms.LinkLabel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.apply = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.helpProvider1 = new System.Windows.Forms.HelpProvider();
			this.ifolders = new System.Windows.Forms.ComboBox();
			this.ifolderLabel = new System.Windows.Forms.Label();
			this.open = new System.Windows.Forms.Button();
			this.tabControl1.SuspendLayout();
			this.tabGeneral.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.syncInterval)).BeginInit();
			this.groupBox4.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.tabSharing.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabGeneral);
			this.tabControl1.Controls.Add(this.tabSharing);
			this.tabControl1.Location = new System.Drawing.Point(8, 88);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(424, 416);
			this.tabControl1.TabIndex = 0;
			this.tabControl1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tabControl1_KeyDown);
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// tabGeneral
			// 
			this.tabGeneral.Controls.Add(this.groupBox1);
			this.tabGeneral.Controls.Add(this.groupBox3);
			this.tabGeneral.Location = new System.Drawing.Point(4, 22);
			this.tabGeneral.Name = "tabGeneral";
			this.tabGeneral.Size = new System.Drawing.Size(416, 390);
			this.tabGeneral.TabIndex = 1;
			this.tabGeneral.Text = "General";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.groupBox2);
			this.groupBox1.Controls.Add(this.autoSync);
			this.groupBox1.Controls.Add(this.groupBox4);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(400, 240);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Synchronization";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.syncInterval);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(16, 48);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(368, 104);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Synchronize to host";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 24);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(336, 40);
			this.label5.TabIndex = 0;
			this.label5.Text = "This value sets the default value for how often the host of an iFolder will be co" +
				"ntacted  to sync files.";
			// 
			// syncInterval
			// 
			this.syncInterval.Increment = new System.Decimal(new int[] {
																		   5,
																		   0,
																		   0,
																		   0});
			this.syncInterval.Location = new System.Drawing.Point(232, 72);
			this.syncInterval.Maximum = new System.Decimal(new int[] {
																		 86400,
																		 0,
																		 0,
																		 0});
			this.syncInterval.Minimum = new System.Decimal(new int[] {
																		 1,
																		 0,
																		 0,
																		 -2147483648});
			this.syncInterval.Name = "syncInterval";
			this.syncInterval.Size = new System.Drawing.Size(64, 20);
			this.syncInterval.TabIndex = 2;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(304, 72);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(48, 16);
			this.label6.TabIndex = 3;
			this.label6.Text = "seconds";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(16, 72);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(140, 16);
			this.label7.TabIndex = 1;
			this.label7.Text = "Sync to host every:";
			// 
			// autoSync
			// 
			this.autoSync.Checked = true;
			this.autoSync.CheckState = System.Windows.Forms.CheckState.Checked;
			this.autoSync.Enabled = false;
			this.autoSync.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.autoSync.Location = new System.Drawing.Point(16, 24);
			this.autoSync.Name = "autoSync";
			this.autoSync.Size = new System.Drawing.Size(360, 16);
			this.autoSync.TabIndex = 0;
			this.autoSync.Text = "Automatic sync";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.objectCount);
			this.groupBox4.Controls.Add(this.label2);
			this.groupBox4.Controls.Add(this.label8);
			this.groupBox4.Controls.Add(this.byteCount);
			this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox4.Location = new System.Drawing.Point(16, 160);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(368, 72);
			this.groupBox4.TabIndex = 9;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Statistics";
			// 
			// objectCount
			// 
			this.objectCount.Location = new System.Drawing.Point(272, 48);
			this.objectCount.Name = "objectCount";
			this.objectCount.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.objectCount.Size = new System.Drawing.Size(80, 16);
			this.objectCount.TabIndex = 6;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(184, 16);
			this.label2.TabIndex = 2;
			this.label2.Text = "Amount to upload:";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(8, 48);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(184, 16);
			this.label8.TabIndex = 1;
			this.label8.Text = "Files/folders to synchronize:";
			// 
			// byteCount
			// 
			this.byteCount.Location = new System.Drawing.Point(272, 24);
			this.byteCount.Name = "byteCount";
			this.byteCount.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.byteCount.Size = new System.Drawing.Size(80, 16);
			this.byteCount.TabIndex = 7;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.gaugeChart);
			this.groupBox3.Controls.Add(this.limit);
			this.groupBox3.Controls.Add(this.setLimit);
			this.groupBox3.Controls.Add(this.label10);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Controls.Add(this.availableUnits);
			this.groupBox3.Controls.Add(this.available);
			this.groupBox3.Controls.Add(this.label9);
			this.groupBox3.Controls.Add(this.usedUnits);
			this.groupBox3.Controls.Add(this.used);
			this.groupBox3.Controls.Add(this.label1);
			this.groupBox3.Controls.Add(this.label4);
			this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox3.Location = new System.Drawing.Point(8, 256);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(400, 120);
			this.groupBox3.TabIndex = 12;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Quota";
			// 
			// gaugeChart
			// 
			this.gaugeChart.Used = 0;
			this.gaugeChart.Location = new System.Drawing.Point(312, 32);
			this.gaugeChart.MaxValue = 0;
			this.gaugeChart.Name = "gaugeChart";
			this.gaugeChart.Size = new System.Drawing.Size(16, 72);
			this.gaugeChart.TabIndex = 12;
			// 
			// limit
			// 
			this.limit.Enabled = false;
			this.limit.Location = new System.Drawing.Point(160, 32);
			this.limit.Name = "limit";
			this.limit.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.limit.Size = new System.Drawing.Size(96, 20);
			this.limit.TabIndex = 1;
			this.limit.Text = "";
			this.limit.TextChanged += new System.EventHandler(this.limit_TextChanged);
			// 
			// setLimit
			// 
			this.setLimit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.setLimit.Location = new System.Drawing.Point(16, 34);
			this.setLimit.Name = "setLimit";
			this.setLimit.Size = new System.Drawing.Size(144, 16);
			this.setLimit.TabIndex = 0;
			this.setLimit.Text = "Limit size to:";
			this.setLimit.CheckedChanged += new System.EventHandler(this.setLimit_CheckedChanged);
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(336, 88);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(48, 16);
			this.label10.TabIndex = 11;
			this.label10.Text = "Empty";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(336, 32);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(48, 16);
			this.label3.TabIndex = 10;
			this.label3.Text = "Full";
			// 
			// availableUnits
			// 
			this.availableUnits.Location = new System.Drawing.Point(264, 88);
			this.availableUnits.Name = "availableUnits";
			this.availableUnits.Size = new System.Drawing.Size(24, 16);
			this.availableUnits.TabIndex = 8;
			this.availableUnits.Text = "MB";
			// 
			// available
			// 
			this.available.Location = new System.Drawing.Point(168, 88);
			this.available.Name = "available";
			this.available.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.available.Size = new System.Drawing.Size(88, 16);
			this.available.TabIndex = 7;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(16, 88);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(144, 16);
			this.label9.TabIndex = 6;
			this.label9.Text = "Space available:";
			// 
			// usedUnits
			// 
			this.usedUnits.Location = new System.Drawing.Point(264, 64);
			this.usedUnits.Name = "usedUnits";
			this.usedUnits.Size = new System.Drawing.Size(24, 16);
			this.usedUnits.TabIndex = 4;
			this.usedUnits.Text = "MB";
			// 
			// used
			// 
			this.used.Location = new System.Drawing.Point(168, 64);
			this.used.Name = "used";
			this.used.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.used.Size = new System.Drawing.Size(88, 16);
			this.used.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 64);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(144, 16);
			this.label1.TabIndex = 2;
			this.label1.Text = "Space used:";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(264, 34);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(24, 16);
			this.label4.TabIndex = 5;
			this.label4.Text = "MB";
			// 
			// tabSharing
			// 
			this.tabSharing.Controls.Add(this.access);
			this.tabSharing.Controls.Add(this.decline);
			this.tabSharing.Controls.Add(this.accept);
			this.tabSharing.Controls.Add(this.add);
			this.tabSharing.Controls.Add(this.remove);
			this.tabSharing.Controls.Add(this.shareWith);
			this.tabSharing.Location = new System.Drawing.Point(4, 22);
			this.tabSharing.Name = "tabSharing";
			this.tabSharing.Size = new System.Drawing.Size(416, 390);
			this.tabSharing.TabIndex = 0;
			this.tabSharing.Text = "Sharing";
			// 
			// access
			// 
			this.access.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.access.Location = new System.Drawing.Point(333, 360);
			this.access.Name = "access";
			this.access.TabIndex = 6;
			this.access.Text = "Access...";
			this.access.Click += new System.EventHandler(this.access_Click);
			// 
			// decline
			// 
			this.decline.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.decline.Location = new System.Drawing.Point(88, 328);
			this.decline.Name = "decline";
			this.decline.TabIndex = 5;
			this.decline.Text = "&Decline";
			this.decline.Click += new System.EventHandler(this.decline_Click);
			// 
			// accept
			// 
			this.accept.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.helpProvider1.SetHelpString(this.accept, "Click to send another invitation to the selected contacts.");
			this.accept.Location = new System.Drawing.Point(8, 328);
			this.accept.Name = "accept";
			this.helpProvider1.SetShowHelp(this.accept, true);
			this.accept.TabIndex = 1;
			this.accept.Text = "A&ccept";
			this.accept.Click += new System.EventHandler(this.accept_Click);
			// 
			// add
			// 
			this.add.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.helpProvider1.SetHelpString(this.add, "Click to add contacts to share this iFolder with.");
			this.add.Location = new System.Drawing.Point(253, 328);
			this.add.Name = "add";
			this.helpProvider1.SetShowHelp(this.add, true);
			this.add.TabIndex = 2;
			this.add.Text = "A&dd...";
			this.add.Click += new System.EventHandler(this.add_Click);
			// 
			// remove
			// 
			this.remove.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.helpProvider1.SetHelpString(this.remove, "Click to stop sharing this iFolder with the selected contact(s).");
			this.remove.Location = new System.Drawing.Point(333, 328);
			this.remove.Name = "remove";
			this.helpProvider1.SetShowHelp(this.remove, true);
			this.remove.TabIndex = 3;
			this.remove.Text = "&Remove";
			this.remove.Click += new System.EventHandler(this.remove_Click);
			// 
			// shareWith
			// 
			this.shareWith.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeader1,
																						this.columnHeader3,
																						this.columnHeader2});
			this.shareWith.ContextMenu = this.contextMenu1;
			this.shareWith.FullRowSelect = true;
			this.helpProvider1.SetHelpString(this.shareWith, "Lists the contacts that this iFolder is currently being shared with.");
			this.shareWith.HideSelection = false;
			this.shareWith.Location = new System.Drawing.Point(8, 8);
			this.shareWith.Name = "shareWith";
			this.helpProvider1.SetShowHelp(this.shareWith, true);
			this.shareWith.Size = new System.Drawing.Size(400, 312);
			this.shareWith.TabIndex = 0;
			this.shareWith.View = System.Windows.Forms.View.Details;
			this.shareWith.KeyDown += new System.Windows.Forms.KeyEventHandler(this.shareWith_KeyDown);
			this.shareWith.MouseDown += new System.Windows.Forms.MouseEventHandler(this.shareWith_MouseDown);
			this.shareWith.SelectedIndexChanged += new System.EventHandler(this.shareWith_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Share with";
			this.columnHeader1.Width = 107;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Status";
			this.columnHeader3.Width = 219;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Access";
			this.columnHeader2.Width = 70;
			// 
			// contextMenu1
			// 
			this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuFullControl,
																						 this.menuReadWrite,
																						 this.menuReadOnly});
			this.contextMenu1.Popup += new System.EventHandler(this.contextMenu1_Popup);
			// 
			// menuFullControl
			// 
			this.menuFullControl.Index = 0;
			this.menuFullControl.Text = "Full Control";
			this.menuFullControl.Click += new System.EventHandler(this.menuFullControl_Click);
			// 
			// menuReadWrite
			// 
			this.menuReadWrite.Index = 1;
			this.menuReadWrite.Text = "Read/Write";
			this.menuReadWrite.Click += new System.EventHandler(this.menuReadWrite_Click);
			// 
			// menuReadOnly
			// 
			this.menuReadOnly.Index = 2;
			this.menuReadOnly.Text = "Read Only";
			this.menuReadOnly.Click += new System.EventHandler(this.menuReadOnly_Click);
			// 
			// conflicts
			// 
			this.conflicts.LinkArea = new System.Windows.Forms.LinkArea(44, 10);
			this.conflicts.Location = new System.Drawing.Point(64, 48);
			this.conflicts.Name = "conflicts";
			this.conflicts.Size = new System.Drawing.Size(364, 32);
			this.conflicts.TabIndex = 6;
			this.conflicts.TabStop = true;
			this.conflicts.Text = "This iFolder currently contains conflicts.  Click here to resolve the conflicts.";
			this.conflicts.Visible = false;
			this.conflicts.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.conflicts_LinkClicked);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(24, 48);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(32, 32);
			this.pictureBox1.TabIndex = 7;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Visible = false;
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ok.Location = new System.Drawing.Point(198, 512);
			this.ok.Name = "ok";
			this.ok.TabIndex = 1;
			this.ok.Text = "OK";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancel.Location = new System.Drawing.Point(278, 512);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 2;
			this.cancel.Text = "Cancel";
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// apply
			// 
			this.apply.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.apply.Location = new System.Drawing.Point(357, 512);
			this.apply.Name = "apply";
			this.apply.TabIndex = 3;
			this.apply.Text = "&Apply";
			this.apply.Click += new System.EventHandler(this.apply_Click);
			// 
			// ifolders
			// 
			this.ifolders.Location = new System.Drawing.Point(64, 16);
			this.ifolders.Name = "ifolders";
			this.ifolders.Size = new System.Drawing.Size(336, 21);
			this.ifolders.TabIndex = 4;
			this.ifolders.SelectedIndexChanged += new System.EventHandler(this.ifolders_SelectedIndexChanged);
			// 
			// ifolderLabel
			// 
			this.ifolderLabel.Location = new System.Drawing.Point(8, 18);
			this.ifolderLabel.Name = "ifolderLabel";
			this.ifolderLabel.Size = new System.Drawing.Size(56, 16);
			this.ifolderLabel.TabIndex = 5;
			this.ifolderLabel.Text = "iFolder:";
			// 
			// open
			// 
			this.open.Location = new System.Drawing.Point(408, 15);
			this.open.Name = "open";
			this.open.Size = new System.Drawing.Size(24, 23);
			this.open.TabIndex = 8;
			this.open.Click += new System.EventHandler(this.open_Click);
			// 
			// iFolderAdvanced
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(442, 544);
			this.Controls.Add(this.open);
			this.Controls.Add(this.ifolderLabel);
			this.Controls.Add(this.ifolders);
			this.Controls.Add(this.apply);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.conflicts);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "iFolderAdvanced";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.iFolderAdvanced_Load);
			this.tabControl1.ResumeLayout(false);
			this.tabGeneral.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.syncInterval)).EndInit();
			this.groupBox4.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.tabSharing.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Private Methods
		private void updateDiskQuotaDisplay()
		{
			DiskSpaceQuota dsq = DiskSpaceQuota.Get(currentMember, ifolder);

			if (dsq.Limit != 0)
			{
				limit.Text = ((double)Math.Round(dsq.Limit/megaByte, 2)).ToString();
				setLimit.Checked = true;

				double usedSpace = (double)ifolder.StorageSize;
				double availableSpace = dsq.Limit - usedSpace;

				usedSpace = Math.Round(usedSpace/megaByte, 2);
				availableSpace = Math.Round(availableSpace/megaByte, 2);

				gaugeChart.MaxValue = dsq.Limit / megaByte;
				gaugeChart.Used = usedSpace;
				gaugeChart.BarColor = SystemColors.ActiveCaption;

				used.Text = usedSpace.ToString();
				available.Text = availableSpace.ToString();
			}
			else
			{
				setLimit.Checked = false;
				used.Text = available.Text = limit.Text = "";
				gaugeChart.Used = 0;
			}

			gaugeChart.Invalidate(true);
		}

		private void refreshData()
		{
			// Used to keep track of the new owner.
			newOwnerLvi = null;

			add.Enabled = remove.Enabled = menuFullControl.Enabled = 
				menuReadWrite.Enabled = menuReadOnly.Enabled = access.Enabled = 
				setLimit.Enabled = currentMember.Rights.Equals(Access.Rights.Admin);

			try
			{
				updateDiskQuotaDisplay();

				// Get the refresh interval.
				syncInterval.Value = (decimal)ifolder.RefreshInterval;
				Cursor.Current = Cursors.WaitCursor;

				// Show/hide the collision message.
				conflicts.Visible = pictureBox1.Visible = ifolder.HasCollisions();

				// Get the sync node and byte counts.
				uint nodeCount;
				ulong bytesToSend;
				SyncSize.CalculateSendSize(ifolder, out nodeCount, out bytesToSend);
				objectCount.Text = nodeCount.ToString();
				byteCount.Text = bytesToSend.ToString();

				Cursor.Current = Cursors.Default;
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "SyncSize.CalculateSendSize");
			}

			// Change the pointer to an hourglass.
			Cursor = Cursors.WaitCursor;
			shareWith.Items.Clear();
			shareWith.BeginUpdate();

			try
			{
				poBox = POBox.GetPOBox(ifolder.StoreReference, ifolder.Domain);
				ICSList memberList = ifolder.GetMemberList();

				// Load the member list.
				foreach (ShallowNode shallowNode in memberList)
				{
					// TODO: We may want to reconstitute only when necessary ... for example, when the item comes into view
					ShareListMember shareMember = new ShareListMember();
					shareMember.Member = new Member(ifolder, shallowNode);

					string[] items = new string[3];

					Contact contact = abManager.GetContact(shareMember.Member);
					if (contact != null)
					{
						items[0] = contact.FN;
					}
					else
					{
						items[0] = shareMember.Member.Name;
					}

					items[1] = shareMember.Member.IsOwner ? "Owner" : "";

					int imageIndex;
					// Map the rights to a string.
					items[2] = rightsToString(shareMember.Rights, out imageIndex);

					if (currentMember.UserID == shareMember.Member.UserID)
					{
						imageIndex = 0;
					}

					ListViewItem lvitem = new ListViewItem(items, imageIndex);
					lvitem.Tag = shareMember;

					if (shareMember.Member.IsOwner)
					{
						// Keep track of the current (or old) owner.
						ownerLvi = lvitem;
					}

					shareWith.Items.Add(lvitem);
				}

				// Clear the hashtable.
				lock (subscrHT)
				{
					subscrHT.Clear();
				}

				// Set up the event handlers for the POBox.
				// TODO: we still can't get events into explorer ... this may work once we are in the GAC.
				subscriber = new EventSubscriber(poBox.ID);
				subscriber.NodeChanged += new NodeEventHandler(subscriber_NodeChanged);
				subscriber.NodeCreated += new NodeEventHandler(subscriber_NodeCreated);
				subscriber.NodeDeleted += new NodeEventHandler(subscriber_NodeDeleted);

				// Load the stuff from the POBox.
				ICSList messageList = poBox.Search(Subscription.SubscriptionCollectionIDProperty, ifolder.ID, SearchOp.Equal);
				foreach (ShallowNode shallowNode in messageList)
				{
					ShareListMember shareMember = new ShareListMember();
					shareMember.Subscription = new Subscription(poBox, shallowNode);

					// Don't add any subscriptions that are in the ready state.
					if (shareMember.Subscription.SubscriptionState != SubscriptionStates.Ready)
					{
						string[] items = new string[3];
						items[0] = shareMember.Subscription.ToName;
						items[1] = shareMember.Subscription.SubscriptionState.ToString();
						int imageIndex;
						items[2] = rightsToString(shareMember.Rights, out imageIndex);
					
						ListViewItem lvi = new ListViewItem(items, 5);
						lvi.Tag = shareMember;

						shareWith.Items.Add(lvi);

						// Add the listviewitem to the hashtable so we can quickly find it.
						lock (subscrHT)
						{
							subscrHT.Add(shareMember.Subscription.ID, lvi);
						}
					}
				}
			}
			catch (SimiasException ex)
			{
			}
			catch (Exception ex)
			{
			}

			// Select the first item in the list.
			shareWith.Items[0].Selected = true;

			shareWith.EndUpdate();

			// Restore the cursor.
			Cursor = Cursors.Default;
		}

		private string rightsToString(Access.Rights rights, out int imageIndex)
		{
			string rightsString = null;

			switch (rights)
			{
				case Access.Rights.Admin:
				{
					rightsString = "Full Control";
					imageIndex = 3;
					break;
				}
				case Access.Rights.ReadWrite:
				{
					rightsString = "Read/Write";
					imageIndex = 2;
					break;
				}
				case Access.Rights.ReadOnly:
				{
					rightsString = "Read Only";
					imageIndex = 1;
					break;
				}
				default:
				{
					rightsString = "Unknown";
					imageIndex = 4;
					break;
				}
			}

			return rightsString;
		}

		private Access.Rights stringToRights(string rightsString)
		{
			Access.Rights rights = Access.Rights.Deny;

			switch (rightsString)
			{
				case "Full Control":
				{
					rights = Access.Rights.Admin;
					break;
				}
				case "Read/Write":
				{
					rights = Access.Rights.ReadWrite;
					break;
				}
				case "Read Only":
				{
					rights = Access.Rights.ReadOnly;
					break;
				}
			}

			return rights;
		}

		private void processChanges()
		{
			// Change the pointer to an hourglass.
			Cursor = Cursors.WaitCursor;

			// Change the owner.
			if (newOwnerLvi != null)
			{
				try
				{
					ShareListMember oldOwner = (ShareListMember)ownerLvi.Tag;
					ShareListMember newOwner = (ShareListMember)newOwnerLvi.Tag;
					ifolder.Commit(ifolder.ChangeOwner(newOwner.Member, oldOwner.Rights));
					oldOwner.Changed = newOwner.Changed = false;
				}
				catch (SimiasException e)
				{
					e.LogError();
				}
				catch (Exception e)
				{
					logger.Debug(e, "Changing owner");
				}
			}

			string sendersEmail = null;

			foreach (ListViewItem lvitem in this.shareWith.Items)
			{
				try
				{
					ShareListMember slMember = (ShareListMember)lvitem.Tag;

					// Process added and changed members.
					if (slMember.Added)
					{
						// TODO: we'll get the email a different way in the future.
						if (ifolder.Domain.Equals(Domain.WorkGroupDomainID) &&
							(sendersEmail == null))
						{
							// TODO: check for an existing contact for the current user.
							EmailPrompt emailPrompt = new EmailPrompt();
							if (DialogResult.OK == emailPrompt.ShowDialog())
							{
								sendersEmail = emailPrompt.Email;
							}
						}

						// Add the from e-mail address.
						slMember.Subscription.FromAddress = sendersEmail;

						// Add the listviewitem to the hashtable so we can quickly find it.
						lock (subscrHT)
						{
							subscrHT.Add(slMember.Subscription.ID, lvitem);
						}

						// TODO: change this to use an array and add them all at once.
						// Put the subscription in the POBox.
						poBox.AddMessage(slMember.Subscription);

						lvitem.SubItems[1].Text = slMember.Subscription.SubscriptionState.ToString();

						// Update the state.
						slMember.Added = false;
					}
					else if (slMember.Changed)
					{
						if (slMember.Member != null)
						{
							// Commit the rights for this member.
							ifolder.Commit(slMember.Member);
						}
						else
						{
							// Commit the rights on the subscription.
							poBox.Commit(slMember.Subscription);
						}

						// Reset the flags.
						slMember.Changed = false;
					}
				}
				catch (SimiasException e)
				{
					e.LogError();
					// TODO: display message.
				}
				catch (Exception e)
				{
					logger.Debug(e, "In processChanges()");
					// TODO: display message.
				}
			}

			// process the removedList
			if (removedList != null)
			{
				foreach (ShareListMember slMember in removedList)
				{
					try
					{
						if (slMember.Member != null)
						{
							// Delete the member.
							ifolder.Commit(ifolder.Delete(slMember.Member));
						}
						else
						{
							// Delete the subscription.
							poBox.Commit(poBox.Delete(slMember.Subscription));
						}
					}
					catch (SimiasException e)
					{
						e.LogError();
						MessageBox.Show("Remove failed with the following exception: \n\n" + e.Message, "Remove Failure");
					}
					catch (Exception e)
					{
						//TODO
						logger.Debug(e, "Removing member");
						MessageBox.Show("Remove failed with the following exception: \n\n" + e.Message, "Remove Failure");
					}
				}

				// Clear the list.
				removedList.Clear();
			}

			try
			{
				// Update the refresh interval.
				if (ifolder.RefreshInterval != (int)syncInterval.Value)
					ifolder.RefreshInterval = (int)syncInterval.Value;

				// Update the disk quota policy.
				if (setLimit.Checked)
				{
					Simias.Policy.DiskSpaceQuota.Set(ifolder, (long)(long.Parse(limit.Text) * megaByte));
				}
				else
				{
					Simias.Policy.DiskSpaceQuota.Delete(ifolder);
				}

				updateDiskQuotaDisplay();
			}
			catch (SimiasException e)
			{
				e.LogError();
			}
			catch (Exception e)
			{
				logger.Debug(e, "In processChanges()");
			}

			// Disable the apply button.
			apply.Enabled = false;

			// Restore the cursor.
			Cursor = Cursors.Default;
		}

		private bool IsCurrentUserValid()
		{
			// TODO - may need to actually check the current user.
/*			if (this.shareWith.Items.Count == 1)
			{
				try
				{
					Contact currentContact = ((ShareListContact)this.shareWith.Items[0].Tag).CurrentContact;
					if (currentContact.FN == null ||
						currentContact.FN == String.Empty ||
						currentContact.EMail == null ||
						currentContact.EMail == String.Empty)
					{
						MyMessageBox mmb = new MyMessageBox();
						mmb.Text = "Incomplete Address Book Entry";
						mmb.Message = "Before you can share, you must add some data to your address book entry.  Do you want to add this information now?";
						mmb.MessageIcon = SystemIcons.Question.ToBitmap();
						DialogResult result = mmb.ShowDialog();
						//MessageBox.Show(this, , , MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1)
						if (result == DialogResult.Yes)
						{
							ContactEditor editor = new ContactEditor();
							editor.LoadPath = LoadPath;
							editor.CurrentContact = currentContact;
							editor.CurrentAddressBook = defaultAddressBook;
							if (editor.ShowDialog() != DialogResult.OK)
							{
								return false;
							}
						}
						else
						{
							return false;
						}
					}
				}
				catch
				{
					return false;
				}
			}
*/
			return true;
		}

		private void updateSelectedListViewItems(Access.Rights rights)
		{
			foreach (ListViewItem lvi in shareWith.SelectedItems)
			{
				updateListViewItem(lvi, rights);
			}
		}

		private void updateListViewItem(ListViewItem lvi, Access.Rights rights)
		{
			ShareListMember slMember = (ShareListMember)lvi.Tag;

			int imageIndex;
			string access = rightsToString(rights, out imageIndex);

			try
			{
				if ((slMember.Member != null) && currentMember.UserID.Equals(slMember.Member.UserID))
				{
					// Don't allow current user to be modified.
				}
				else
				{
					if (slMember.Rights != rights)
					{
						// Mark this item as changed.
						slMember.Changed = true;

						// Set the rights.
						slMember.Rights = rights;

						// Change the subitem text.
						lvi.SubItems[2].Text = access;

						// Enable the apply button.
						apply.Enabled = true;
					}

					// Don't change the image if this item is not a member.
					if (slMember.Member != null)
					{
						lvi.ImageIndex = imageIndex;
						lvi.SubItems[1].Text = slMember.Member.IsOwner ? "Owner" : "";

						Contact contact = abManager.GetContact(slMember.Member);
						if (contact != null)
						{
							lvi.SubItems[0].Text = contact.FN;
						}
					}
				}
			}
			catch{}
		}
		#endregion

		#region Properties
		/// <summary>
		/// Sets the current iFolder.
		/// </summary>
		public iFolder CurrentiFolder
		{
			set
			{
				this.ifolder = value;
			}
		}

		/// <summary>
		/// The path where the DLL is running from.
		/// </summary>
		public string LoadPath
		{
			get
			{
				return loadPath;
			}

			set
			{
				this.loadPath = value;
			}
		}

		/// <summary>
		/// Sets the name of the tab to be displayed initially.
		/// </summary>
		public int ActiveTab
		{
			set
			{
				try
				{
					tabControl1.SelectedIndex = value;
				}
				catch{}
			}
		}
		#endregion

		#region Event Handlers
		private void iFolderAdvanced_Load(object sender, EventArgs e)
		{
			// TODO - use locale-specific path.
//			helpProvider1.HelpNamespace = Path.Combine(loadPath, @"help\en\doc\user\data\front.html");

			// Image list...
			try
			{
				// Create the ImageList object.
				ImageList contactsImageList = new ImageList();

				// Initialize the ImageList objects with icons.
				string basePath = loadPath != null ? Path.Combine(loadPath, "res") : Path.Combine(Application.StartupPath, "res");
				contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_me_card.ico")));
				contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_contact_read.ico")));
				contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_contact_read_write.ico")));
				contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_contact_full.ico")));
				contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_contact_card.ico")));
				contactsImageList.Images.Add(new Icon(Path.Combine(Path.GetDirectoryName(basePath), "Invitation.ico")));

				//Assign the ImageList objects to the books ListView.
				shareWith.SmallImageList = contactsImageList;

//				pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
				pictureBox1.Image = (Image)SystemIcons.Information.ToBitmap();

				Bitmap bitmap = new Bitmap(Path.Combine(basePath, "OpenFolder.bmp"));
				bitmap.MakeTransparent(bitmap.GetPixel(0,0));
				this.open.Image = bitmap;
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Loading images");
			}

			// Hashtable used to store subscriptions in.
			subscrHT = new Hashtable();

			try
			{
				// Add all iFolders to the drop-down list.
				ifManager = iFolderManager.Connect();
				foreach (iFolder i in ifManager)
				{
					iFolderInfo ifolderInfo = new iFolderInfo();
					ifolderInfo.LocalPath = i.LocalPath;
					ifolderInfo.ID = i.ID;
					ifolders.Items.Add(ifolderInfo);

					// Set the passed in iFolder as the selected one.
					if ((ifolder != null) && ifolder.ID.Equals(ifolderInfo.ID))
					{
						ifolders.SelectedItem = ifolderInfo;
					}
				}
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Reading iFolders");
			}
		}

		private void shareWith_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (shareWith.SelectedItems.Count == 1)
			{
				// Only one item is selected ... get the ListViewItem for the selected item.
				ListViewItem lvi = shareWith.SelectedItems[0];

				// Enable the accept and decline buttons if the subscription state is "Pending".
				accept.Enabled = decline.Enabled = lvi.SubItems[1].Text.Equals(SubscriptionStates.Pending.ToString());
			}
			else
			{
				// Multiple items are selected ... disable
				// the accept and decline buttons.
				accept.Enabled = decline.Enabled = false;
			}

			if ((shareWith.SelectedItems.Count == 1) && 
				(((ShareListMember)shareWith.SelectedItems[0].Tag).Member != null) &&
				((ShareListMember)shareWith.SelectedItems[0].Tag).Member.ID.Equals(currentMember.ID))
			{
				// The current member is the only one selected, disable the access control
				// menus and the remove button.
				remove.Enabled = access.Enabled = menuFullControl.Enabled = 
					menuReadWrite.Enabled = menuReadOnly.Enabled = false;
			}
			else
			{
				// Enable the access control menus and the remove button if one or more
				// items is selected and the current user has admin rights.
				remove.Enabled = access.Enabled = menuFullControl.Enabled = 
					menuReadWrite.Enabled = menuReadOnly.Enabled = 
					(shareWith.SelectedItems.Count != 0 && this.currentMember.Rights.Equals(Access.Rights.Admin));
			}
		}

		private void add_Click(object sender, System.EventArgs e)
		{
			if (!IsCurrentUserValid())
				return;

			// TODO - Initialize the picker with the names that are already in the share list.
			ContactPicker picker = new ContactPicker();
			picker.CurrentManager = abManager;
			picker.LoadPath = loadPath;
			picker.Collection = ifolder;
			DialogResult result = picker.ShowDialog();
			if (result == DialogResult.OK)
			{
				// Unselect all items.
				shareWith.SelectedItems.Clear();

				// Get the list of added items from the picker.
				ArrayList contactList = picker.GetContactList;
				// Enable the apply button.
				if (contactList.Count > 0)
					apply.Enabled = true;

				foreach (Contact c in contactList)
				{
					// Initialize a listview item.
					string[] items = new string[3];
					items[0] = c.FN;
					items[1] = "Inviting";
					items[2] = "Read/Write";
					ListViewItem lvitem = new ListViewItem(items, 5);

					ShareListMember shareMember = null;

					// Check to see if this contact was originally in the list.
/*TODO:					if (this.removedList != null)
					{
						ShareListMember slMemberToRemove = null;

						foreach (ShareListMember slMember in removedList)
						{
							if (c.ID == slContact.CurrentContact.ID)
							{
								// The name may be different and we don't know what the rights used to be,
								// so create a new object to represent this item.
								shareMember = new ShareListContact();//(c, false, true);
								shareMember.CurrentContact = c;
								shareMember.IsMember = slMember.IsMember;
								shareMember.Added = false;
								shareMember.Changed = true;
								slMemberToRemove = slMember;
								break;
							}
						}

						if (slMemberToRemove != null)
							removedList.Remove(slMemberToRemove);
					}*/

					if (shareMember == null)
					{
						// TODO: this needs to be reworked after BETA1 ... for now we will only allow them to choose a contact that is linked to a member.
						if ((!ifolder.Domain.Equals(Domain.WorkGroupDomainID)) &&
							((c.UserID == null) || (c.UserID.Length == 0)))
						{
							MessageBox.Show("Unable to share with the following incomplete user:\n\n" + c.FN, "Share Failure");
						}
						else
						{
							// The contact was not in the removed list, so create a new one.
							shareMember = new ShareListMember();
							shareMember.Added = true;

							shareMember.Subscription = poBox.CreateSubscription(ifolder, currentMember, typeof(iFolder).Name);

							// Add all of the other properties (ToAddress, FromAddress, etc.)
							shareMember.Rights = Access.Rights.ReadWrite;
							shareMember.Subscription.ToName = c.FN;
							shareMember.Subscription.SubscriptionCollectionName = ifolder.Name;

							if (ifolder.Domain.Equals(Domain.WorkGroupDomainID))
							{
								// Check if the contact has already been linked.
								if (c.UserID.Equals(String.Empty))
								{
									// Create a relationship on the Subscription object ... this will be used later to link the member with the contact.
									string collectionId = (string)c.Properties.GetSingleProperty(BaseSchema.CollectionId).Value;
									shareMember.Subscription.Properties.AddProperty("Contact", new Relationship(collectionId, c.ID));
								}
								shareMember.Subscription.ToAddress = c.EMail;
							}
							else
							{
								Novell.AddressBook.AddressBook ab = abManager.GetAddressBook(c.Properties.GetSingleProperty(BaseSchema.CollectionId).ToString());
								shareMember.Subscription.ToIdentity = ab.GetMemberByID(c.UserID).UserID;
							}
						}
					}

					// If we have a valid shareMember, then add it to the listview.
					if (shareMember != null)
					{
						lvitem.Tag = shareMember;

						// Select the contacts that were just added.
						lvitem.Selected = true;

						shareWith.Items.Add(lvitem);
					}
				}
			}
		}

		private void remove_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem lvi in shareWith.SelectedItems)
			{
				ShareListMember slMember = (ShareListMember)lvi.Tag;

				try
				{
					// Don't allow the current user to be removed.
					if (((slMember.Member != null) && !currentMember.UserID.Equals(slMember.Member.UserID)) ||
						(slMember.Member == null))
					{
						// If this item is not newly added, we need to add it to the removedList.
						if (!slMember.Added)
						{
							// Make sure the removed list is valid.
							if (removedList == null)
							{
								removedList = new ArrayList();
							}

							// Add this to the removed list.
							removedList.Add(slMember);
						}

						// Remove the item from the listview.
						lvi.Remove();

						// If this is a subscription, remove it from the hashtable.
						if (slMember.Subscription != null)
						{
							lock (subscrHT)
							{
								subscrHT.Remove(slMember.Subscription.ID);
							}
						}

						// Enable the apply button.
						apply.Enabled = true;
					}
				}
				catch (Exception ex)
				{
					logger.Debug(ex, "Removing contacts");
				}
			}
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			this.processChanges();
			this.Close();
		}

		private void accept_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = this.shareWith.SelectedItems[0];
			ShareListMember slMember = (ShareListMember)lvi.Tag;
			slMember.Member = slMember.Subscription.Accept(ifolder.StoreReference, slMember.Subscription.SubscriptionRights);

			// Take the relationship off the Subscription object
			Property property = slMember.Subscription.Properties.GetSingleProperty("Contact");
			if (property != null)
			{
				Relationship relationship = (Relationship)property.Value;

				// Get the contact from the relationship.
				Novell.AddressBook.AddressBook ab = this.abManager.GetAddressBook(relationship.CollectionID);
				Contact contact = ab.GetContact(relationship.NodeID);

				// Put the Member userID into the Contact userID.
				contact.UserID = slMember.Member.UserID;
				ab.Commit(contact);
			}

			// This is item is now a member so remove it from the subscription list.
			lock (subscrHT)
			{
				subscrHT.Remove(slMember.Subscription.ID);
			}

			poBox.Commit(slMember.Subscription);
			
			updateListViewItem(lvi, slMember.Rights);
		}

		private void decline_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = this.shareWith.SelectedItems[0];
			ShareListMember slMember = (ShareListMember)lvi.Tag;
			slMember.Subscription.Decline();
			poBox.Commit(slMember.Subscription);
			lvi.Remove();
		}

		private void apply_Click(object sender, System.EventArgs e)
		{
			this.processChanges();
		
			// Reload the collection.
			string id = ifolder.ID;
			ifolder = null;
			ifolder = iFolderManager.Connect().GetiFolderById(id);
		}

		private void cancel_Click(object sender, System.EventArgs e)
		{
			this.Close();	
		}

		private void syncInterval_ValueChanged(object sender, System.EventArgs e)
		{
			// Enable the apply button if the user changed the interval.
			if (syncInterval.Focused)
				apply.Enabled = true;		
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (tabControl1.SelectedTab == tabGeneral)
			{
				Cursor.Current = Cursors.WaitCursor;

				try
				{
					conflicts.Visible = pictureBox1.Visible = ifolder.HasCollisions();
				}
				catch (SimiasException ex)
				{
					ex.LogError();
				}
				catch (Exception ex)
				{
					logger.Debug(ex, "HasCollisions");
				}

				Cursor.Current = Cursors.Default;
			}
		}

		private void conflicts_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			ConflictResolver conflictResolver = new ConflictResolver();
			conflictResolver.IFolder = this.ifolder;
			conflictResolver.LoadPath = loadPath;
			conflictResolver.ConflictsResolved += new Novell.iFolder.iFolderCom.ConflictResolver.ConflictsResolvedDelegate(conflictResolver_ConflictsResolved);
			conflictResolver.Show();		
		}

		private void conflictResolver_ConflictsResolved(object sender, EventArgs e)
		{
			conflicts.Visible = pictureBox1.Visible = false;
		}

		private void subscriber_NodeCreated(NodeEventArgs args)
		{
			try
			{
				Node node = poBox.GetNodeByID(args.ID);
				if (node != null)
				{
					ShareListMember slMember = new ShareListMember();
					slMember.Subscription = new Subscription(node);

					lock (subscrHT)
					{
						// If the subscription state is "Ready" and the collection exists locally or if the item is already in the list
						// or if the subscription is not for this ifolder, don't add it to the listview.
						if (((slMember.Subscription.SubscriptionState != SubscriptionStates.Ready) 
							|| (poBox.StoreReference.GetCollectionByID(slMember.Subscription.SubscriptionCollectionID) == null))
							&& (subscrHT[args.ID] == null)
							&& (slMember.Subscription.SubscriptionCollectionID.Equals(ifolder.ID)))
						{
							string[] items = new string[3];
							items[0] = slMember.Subscription.ToName;
							items[1] = slMember.Subscription.SubscriptionState.ToString();
							int imageIndex;
							items[2] = rightsToString(slMember.Rights, out imageIndex);
					
							ListViewItem lvi = new ListViewItem(items, 5);
							lvi.Tag = slMember;

							shareWith.Items.Add(lvi);

							// Add the listviewitem to the hashtable so we can quickly find it.
							subscrHT.Add(slMember.Subscription.ID, lvi);
						}
					}
				}
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "OnNodeCreated");
			}
		}

		private void subscriber_NodeDeleted(NodeEventArgs args)
		{
			lock (subscrHT)
			{
				ListViewItem lvi = (ListViewItem)subscrHT[args.Node];
				if (lvi != null)
				{
					lvi.Remove();
					subscrHT.Remove(args.Node);
				}
			}
		}

		private void subscriber_NodeChanged(NodeEventArgs args)
		{
			// Get the existing item.
			lock (subscrHT)
			{
				ListViewItem lvi = (ListViewItem)subscrHT[args.Node];
				if (lvi != null)
				{
					try
					{
						// Get the node that changed.
						Node node = poBox.GetNodeByID(args.ID);
						if (node != null)
						{
							ShareListMember slMember = (ShareListMember)lvi.Tag;

							// New up a Subscription object based on the node.
							slMember.Subscription = new Subscription(node);

							// If the subscription state is "Ready" and the collection exists locally, remove the listview item; 
							// otherwise, update the status text.
							if ((slMember.Subscription.SubscriptionState != SubscriptionStates.Ready) || 
								(poBox.StoreReference.GetCollectionByID(slMember.Subscription.SubscriptionCollectionID) == null))
							{
								lvi.SubItems[1].Text = slMember.Subscription.SubscriptionState.ToString();
								lvi.Tag = slMember;
								if ((shareWith.SelectedItems.Count == 1) &&
									lvi.Equals(shareWith.SelectedItems[0]) &&
									(slMember.Subscription.SubscriptionState == Simias.POBox.SubscriptionStates.Pending))
									//(((ShareListMember)shareWith.SelectedItems[0].Tag).Subscription != null) &&
									//(((ShareListMember)shareWith.SelectedItems[0].Tag).Subscription.ID.Equals(slMember.Subscription.ID)) &&
								{
									accept.Enabled = decline.Enabled = true;
								}
							}
							else
							{
								lvi.Remove();
							}
						}
					}
					catch (SimiasException ex)
					{
						ex.LogError();
					}
					catch (Exception ex)
					{
						logger.Debug(ex, "OnNodeChanged");
					}
				}
			}
		}

		private void setLimit_CheckedChanged(object sender, System.EventArgs e)
		{
			limit.Enabled = setLimit.Checked;
			if (setLimit.Focused)
			{
				apply.Enabled = true;
			}
		}

		private void limit_TextChanged(object sender, System.EventArgs e)
		{
			if (limit.Focused)
			{
				apply.Enabled = true;
			}
		}

		private void ifolders_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (apply.Enabled)
			{
				MyMessageBox mmb = new MyMessageBox();
				mmb.Message = "Do you want to save the changes you have made to this iFolder?";
				mmb.Caption = "Save Changes";
				mmb.MessageIcon = SystemIcons.Question;
				if (DialogResult.Yes == mmb.ShowDialog())
				{
					processChanges();
				}
				else
				{
					apply.Enabled = false;

					// Clear the removed list.
					if (removedList != null)
					{
						removedList.Clear();
					}
				}
			}

			ifolder = ifManager.GetiFolderById(((iFolderInfo)ifolders.SelectedItem).ID);
			currentMember = ifolder.GetCurrentMember();
			this.Text = "iFolder Properties for " + Path.GetFileName(ifolder.LocalPath);
			refreshData();
		}

		private void open_Click(object sender, System.EventArgs e)
		{
			try
			{
				System.Diagnostics.Process.Start(((iFolderInfo)ifolders.SelectedItem).LocalPath);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void access_Click(object sender, System.EventArgs e)
		{
			UserProperties userProperties = new UserProperties();
			userProperties.OwnerCanBeSet = (currentMember.IsOwner && (shareWith.SelectedItems.Count == 1));
			if (shareWith.SelectedItems.Count == 1)
			{
				ListViewItem lvi = shareWith.SelectedItems[0];
				userProperties.Title = "Properties for " + lvi.Text;
				userProperties.Rights = ((ShareListMember)lvi.Tag).Rights;
				userProperties.CanBeOwner = ((ShareListMember)lvi.Tag).Member != null;
				userProperties.Owner = newOwnerLvi != null ? lvi.Equals(newOwnerLvi) : lvi.Equals(ownerLvi);//lvi.SubItems[1].Text.Equals("Owner");
			}

			if (DialogResult.OK == userProperties.ShowDialog())
			{
				foreach (ListViewItem lvi in shareWith.SelectedItems)
				{
					updateListViewItem(lvi, userProperties.Rights);
				}

				if (shareWith.SelectedItems.Count == 1)
				{
					// TODO: Update the owner.
					if (userProperties.Owner)
					{
						ListViewItem lvi = shareWith.SelectedItems[0];
						lvi.SubItems[1].Text = "Owner";

						if (newOwnerLvi != null)
						{
							// Update the previous "new owner"
							newOwnerLvi.SubItems[1].Text = "";
						}
						else
						{
							// Update the old owner.
							ownerLvi.SubItems[1].Text = "";
						}

						// Keep track of the new owner.
						newOwnerLvi = lvi;
					}
				}
			}
		}

		private void shareWith_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			// TODO: do we want the access menu to only show up when clicking in the Access column?
			if (e.Button.Equals(MouseButtons.Right))
			{
			}
		}

		private void contextMenu1_Popup(object sender, System.EventArgs e)
		{
			menuFullControl.Visible = menuReadWrite.Visible = menuReadOnly.Visible = shareWith.SelectedItems.Count != 0;

			if (shareWith.SelectedItems.Count == 1)
			{
				ShareListMember slMember = (ShareListMember)shareWith.SelectedItems[0].Tag;
				switch (slMember.Rights)
				{
					case Access.Rights.Admin:
						menuFullControl.Checked = true;
						menuReadWrite.Checked = menuReadOnly.Checked = false;
						break;
					case Access.Rights.ReadWrite:
						menuReadWrite.Checked = true;
						menuFullControl.Checked = menuReadOnly.Checked = false;
						break;
					case Access.Rights.ReadOnly:
						menuReadOnly.Checked = true;
						menuFullControl.Checked = menuReadWrite.Checked = false;
						break;
				}
			}
			else if (shareWith.SelectedItems.Count > 1)
			{
				menuFullControl.Checked = menuReadWrite.Checked = menuReadOnly.Checked = false;
			}
		}

		private void menuFullControl_Click(object sender, System.EventArgs e)
		{
			updateSelectedListViewItems(Access.Rights.Admin);
		}

		private void menuReadWrite_Click(object sender, System.EventArgs e)
		{
			updateSelectedListViewItems(Access.Rights.ReadWrite);
		}

		private void menuReadOnly_Click(object sender, System.EventArgs e)
		{
			updateSelectedListViewItems(Access.Rights.ReadOnly);
		}

		private void shareWith_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				remove_Click(this, new System.EventArgs());
			}
		}
		#endregion

		private void tabControl1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			// TODO - change focus when dialog is displayed non-modal
//			if (!this.Modal && e.KeyCode == Keys.Tab)
//			{
//				currentControl = this.GetNextControl(this, true);
//				bool focus = currentControl.Focus();// add.Focus();
//				focus = !focus;
//			}
		}
	}
}
