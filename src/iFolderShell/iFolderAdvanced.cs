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
using System.Net;

namespace Novell.iFolderCom
{
	/// <summary>
	/// iFolder Advanced dialog.
	/// </summary>
	[ComVisible(false)]
	public class iFolderAdvanced : System.Windows.Forms.Form
	{
		#region Class Members
		private const string member = "Member";
		private const double megaByte = 1048576;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Button apply;
		private System.Windows.Forms.ListView shareWith;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.Button remove;
		private System.Windows.Forms.Button add;

		//private Hashtable subscrHT;
		//private EventSubscriber subscriber;
		private int okDelta;
		private int initTabTop;
		private int initHeight;
		private bool accessClick;
		private iFolder ifolder;
		private iFolderUser currentUser;
		private ListViewItem ownerLvi;
		private ListViewItem newOwnerLvi;
		private iFolderWebService ifWebService;
		private ArrayList removedList;
		private string loadPath;
		private Control currentControl;
		private Control firstControl;
		private Control lastControl;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.HelpProvider helpProvider1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown syncInterval;
		private System.Windows.Forms.Label label6;
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
		private System.Windows.Forms.CheckBox setLimit;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label used;
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
		private Novell.Forms.Controls.GaugeChart gaugeChart;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.TextBox limitEdit;
		private System.Windows.Forms.Label limitLabel;
		private System.Windows.Forms.Label limit;
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

			apply.Enabled = remove.Enabled = /*accept.Enabled = decline.Enabled =*/ false;

			syncInterval.TextChanged += new EventHandler(syncInterval_ValueChanged);
			okDelta = ok.Top - tabControl1.Bottom;
			initTabTop = tabControl1.Top;
			initHeight = this.Height;

			currentControl = firstControl = this.ifolders;
			lastControl = this.apply;

			this.StartPosition = FormStartPosition.CenterParent;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
/*				if (subscriber != null)
				{
					subscriber.Dispose();
				}*/

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
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.objectCount = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.byteCount = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.syncInterval = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.autoSync = new System.Windows.Forms.CheckBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.gaugeChart = new Novell.Forms.Controls.GaugeChart();
			this.label7 = new System.Windows.Forms.Label();
			this.limitEdit = new System.Windows.Forms.TextBox();
			this.setLimit = new System.Windows.Forms.CheckBox();
			this.label10 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.availableUnits = new System.Windows.Forms.Label();
			this.available = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.usedUnits = new System.Windows.Forms.Label();
			this.used = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.limitLabel = new System.Windows.Forms.Label();
			this.limit = new System.Windows.Forms.Label();
			this.tabSharing = new System.Windows.Forms.TabPage();
			this.access = new System.Windows.Forms.Button();
			this.add = new System.Windows.Forms.Button();
			this.remove = new System.Windows.Forms.Button();
			this.shareWith = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
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
			this.groupBox4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.syncInterval)).BeginInit();
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
			this.tabControl1.TabIndex = 4;
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
			this.groupBox1.Controls.Add(this.groupBox4);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.syncInterval);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.autoSync);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(400, 184);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Synchronization";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.objectCount);
			this.groupBox4.Controls.Add(this.label2);
			this.groupBox4.Controls.Add(this.label8);
			this.groupBox4.Controls.Add(this.byteCount);
			this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox4.Location = new System.Drawing.Point(16, 104);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(368, 72);
			this.groupBox4.TabIndex = 4;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Statistics";
			// 
			// objectCount
			// 
			this.objectCount.Location = new System.Drawing.Point(272, 48);
			this.objectCount.Name = "objectCount";
			this.objectCount.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.objectCount.Size = new System.Drawing.Size(80, 16);
			this.objectCount.TabIndex = 3;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(184, 16);
			this.label2.TabIndex = 0;
			this.label2.Text = "Amount to upload:";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(8, 48);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(184, 16);
			this.label8.TabIndex = 2;
			this.label8.Text = "Files/folders to synchronize:";
			// 
			// byteCount
			// 
			this.byteCount.Location = new System.Drawing.Point(272, 24);
			this.byteCount.Name = "byteCount";
			this.byteCount.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.byteCount.Size = new System.Drawing.Size(80, 16);
			this.byteCount.TabIndex = 1;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 24);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(336, 24);
			this.label5.TabIndex = 0;
			this.label5.Text = "This will set the sync setting for this iFolder.";
			// 
			// syncInterval
			// 
			this.syncInterval.Increment = new System.Decimal(new int[] {
																		   5,
																		   0,
																		   0,
																		   0});
			this.syncInterval.Location = new System.Drawing.Point(144, 56);
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
			this.label6.Location = new System.Drawing.Point(216, 56);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(48, 16);
			this.label6.TabIndex = 3;
			this.label6.Text = "seconds";
			// 
			// autoSync
			// 
			this.autoSync.Checked = true;
			this.autoSync.CheckState = System.Windows.Forms.CheckState.Checked;
			this.autoSync.Enabled = false;
			this.autoSync.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.autoSync.Location = new System.Drawing.Point(16, 56);
			this.autoSync.Name = "autoSync";
			this.autoSync.Size = new System.Drawing.Size(160, 16);
			this.autoSync.TabIndex = 1;
			this.autoSync.Text = "Sync to host every:";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.gaugeChart);
			this.groupBox3.Controls.Add(this.label7);
			this.groupBox3.Controls.Add(this.limitEdit);
			this.groupBox3.Controls.Add(this.setLimit);
			this.groupBox3.Controls.Add(this.label10);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Controls.Add(this.availableUnits);
			this.groupBox3.Controls.Add(this.available);
			this.groupBox3.Controls.Add(this.label9);
			this.groupBox3.Controls.Add(this.usedUnits);
			this.groupBox3.Controls.Add(this.used);
			this.groupBox3.Controls.Add(this.label1);
			this.groupBox3.Controls.Add(this.limitLabel);
			this.groupBox3.Controls.Add(this.limit);
			this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox3.Location = new System.Drawing.Point(8, 208);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(400, 112);
			this.groupBox3.TabIndex = 1;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Disk space";
			// 
			// gaugeChart
			// 
			this.gaugeChart.Location = new System.Drawing.Point(312, 24);
			this.gaugeChart.Name = "gaugeChart";
			this.gaugeChart.Size = new System.Drawing.Size(16, 72);
			this.gaugeChart.TabIndex = 10;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(264, 80);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(24, 16);
			this.label7.TabIndex = 9;
			this.label7.Text = "MB";
			// 
			// limitEdit
			// 
			this.limitEdit.Enabled = false;
			this.limitEdit.Location = new System.Drawing.Point(160, 50);
			this.limitEdit.Name = "limitEdit";
			this.limitEdit.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.limitEdit.Size = new System.Drawing.Size(96, 20);
			this.limitEdit.TabIndex = 4;
			this.limitEdit.Text = "";
			this.limitEdit.TextChanged += new System.EventHandler(this.limitEdit_TextChanged);
			// 
			// setLimit
			// 
			this.setLimit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.setLimit.Location = new System.Drawing.Point(16, 52);
			this.setLimit.Name = "setLimit";
			this.setLimit.Size = new System.Drawing.Size(144, 16);
			this.setLimit.TabIndex = 3;
			this.setLimit.Text = "Limit size to:";
			this.setLimit.CheckedChanged += new System.EventHandler(this.setLimit_CheckedChanged);
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(336, 80);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(48, 16);
			this.label10.TabIndex = 12;
			this.label10.Text = "Empty";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(336, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(48, 16);
			this.label3.TabIndex = 11;
			this.label3.Text = "Full";
			// 
			// availableUnits
			// 
			this.availableUnits.Location = new System.Drawing.Point(264, 24);
			this.availableUnits.Name = "availableUnits";
			this.availableUnits.Size = new System.Drawing.Size(24, 16);
			this.availableUnits.TabIndex = 2;
			this.availableUnits.Text = "MB";
			// 
			// available
			// 
			this.available.Location = new System.Drawing.Point(168, 80);
			this.available.Name = "available";
			this.available.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.available.Size = new System.Drawing.Size(88, 16);
			this.available.TabIndex = 8;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(16, 80);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(144, 16);
			this.label9.TabIndex = 7;
			this.label9.Text = "Available space:";
			// 
			// usedUnits
			// 
			this.usedUnits.Location = new System.Drawing.Point(264, 52);
			this.usedUnits.Name = "usedUnits";
			this.usedUnits.Size = new System.Drawing.Size(24, 16);
			this.usedUnits.TabIndex = 6;
			this.usedUnits.Text = "MB";
			// 
			// used
			// 
			this.used.Location = new System.Drawing.Point(168, 24);
			this.used.Name = "used";
			this.used.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.used.Size = new System.Drawing.Size(88, 16);
			this.used.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(144, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "iFolder size:";
			// 
			// limitLabel
			// 
			this.limitLabel.Location = new System.Drawing.Point(16, 52);
			this.limitLabel.Name = "limitLabel";
			this.limitLabel.Size = new System.Drawing.Size(100, 16);
			this.limitLabel.TabIndex = 2;
			this.limitLabel.Text = "iFolder limit:";
			// 
			// limit
			// 
			this.limit.Location = new System.Drawing.Point(168, 52);
			this.limit.Name = "limit";
			this.limit.Size = new System.Drawing.Size(88, 16);
			this.limit.TabIndex = 5;
			this.limit.Text = "label4";
			// 
			// tabSharing
			// 
			this.tabSharing.Controls.Add(this.access);
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
			this.access.Location = new System.Drawing.Point(8, 360);
			this.access.Name = "access";
			this.access.TabIndex = 1;
			this.access.Text = "Access...";
			this.access.Click += new System.EventHandler(this.access_Click);
			// 
			// add
			// 
			this.add.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.helpProvider1.SetHelpString(this.add, "Click to add contacts to share this iFolder with.");
			this.add.Location = new System.Drawing.Point(253, 360);
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
			this.remove.Location = new System.Drawing.Point(333, 360);
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
																						this.columnHeader2,
																						this.columnHeader3});
			this.shareWith.ContextMenu = this.contextMenu1;
			this.shareWith.FullRowSelect = true;
			this.helpProvider1.SetHelpString(this.shareWith, "Lists the contacts that this iFolder is currently being shared with.");
			this.shareWith.HideSelection = false;
			this.shareWith.Location = new System.Drawing.Point(8, 8);
			this.shareWith.Name = "shareWith";
			this.helpProvider1.SetShowHelp(this.shareWith, true);
			this.shareWith.Size = new System.Drawing.Size(400, 344);
			this.shareWith.TabIndex = 0;
			this.shareWith.View = System.Windows.Forms.View.Details;
			this.shareWith.KeyDown += new System.Windows.Forms.KeyEventHandler(this.shareWith_KeyDown);
			this.shareWith.MouseDown += new System.Windows.Forms.MouseEventHandler(this.shareWith_MouseDown);
			this.shareWith.SelectedIndexChanged += new System.EventHandler(this.shareWith_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 107;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Status";
			this.columnHeader2.Width = 219;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Access";
			this.columnHeader3.Width = 70;
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
			this.conflicts.TabIndex = 3;
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
			this.ok.TabIndex = 5;
			this.ok.Text = "OK";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancel.Location = new System.Drawing.Point(278, 512);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 6;
			this.cancel.Text = "Cancel";
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// apply
			// 
			this.apply.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.apply.Location = new System.Drawing.Point(357, 512);
			this.apply.Name = "apply";
			this.apply.TabIndex = 7;
			this.apply.Text = "&Apply";
			this.apply.Click += new System.EventHandler(this.apply_Click);
			// 
			// ifolders
			// 
			this.ifolders.Location = new System.Drawing.Point(64, 16);
			this.ifolders.Name = "ifolders";
			this.ifolders.Size = new System.Drawing.Size(336, 21);
			this.ifolders.TabIndex = 1;
			this.ifolders.SelectedIndexChanged += new System.EventHandler(this.ifolders_SelectedIndexChanged);
			// 
			// ifolderLabel
			// 
			this.ifolderLabel.Location = new System.Drawing.Point(8, 18);
			this.ifolderLabel.Name = "ifolderLabel";
			this.ifolderLabel.Size = new System.Drawing.Size(56, 16);
			this.ifolderLabel.TabIndex = 0;
			this.ifolderLabel.Text = "iFolder:";
			// 
			// open
			// 
			this.open.Location = new System.Drawing.Point(408, 15);
			this.open.Name = "open";
			this.open.Size = new System.Drawing.Size(24, 23);
			this.open.TabIndex = 2;
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
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "iFolderAdvanced";
			this.ShowInTaskbar = false;
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tabControl1_KeyDown);
			this.Load += new System.EventHandler(this.iFolderAdvanced_Load);
			this.tabControl1.ResumeLayout(false);
			this.tabGeneral.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.syncInterval)).EndInit();
			this.groupBox3.ResumeLayout(false);
			this.tabSharing.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Private Methods
		private void showConflictMessage(bool show)
		{
			if (show)
			{
				// Display the conflicts message.
				conflicts.Visible = pictureBox1.Visible = true;

				// Move the controls back to the original position.
				tabControl1.Top = initTabTop;
				this.Height = initHeight;
			}
			else
			{
				// Hide the conflicts message.
				conflicts.Visible = pictureBox1.Visible = false;

				// Move the controls up so we don't have dead space.
				tabControl1.Top = conflicts.Top;
				this.Height = initHeight - (initTabTop - conflicts.Top);
			}
			
			// Relocate the ok, cancel, and apply buttons.
			ok.Top = cancel.Top = apply.Top = tabControl1.Bottom + okDelta;
		}

		private void updateDiskQuotaDisplay()
		{
			try
			{
				connectToWebService();
				DiskSpace diskSpace = ifWebService.GetiFolderDiskSpace(ifolder.ID);
				double usedSpace = Math.Round(diskSpace.UsedSpace/megaByte, 2);
				used.Text = usedSpace.ToString();
				if (diskSpace.Limit != 0)
				{
					limitEdit.Text = limit.Text = ((double)Math.Round(diskSpace.Limit/megaByte, 2)).ToString();
					setLimit.Checked = true;

					available.Text = ((double)Math.Round(diskSpace.AvailableSpace/megaByte, 2)).ToString();

					gaugeChart.MaxValue = diskSpace.Limit / megaByte;
					gaugeChart.Used = usedSpace;
					gaugeChart.BarColor = SystemColors.ActiveCaption;
				}
				else
				{
					setLimit.Checked = false;
					available.Text = limit.Text = limitEdit.Text = "";
					gaugeChart.Used = 0;
				}
			}
			catch
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered while reading disk space restrictions.");

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

			// Change the pointer to an hourglass.
			Cursor = Cursors.WaitCursor;

			updateDiskQuotaDisplay();

			// Get the refresh interval.
			syncInterval.Value = (decimal)ifolder.SyncInterval;

			// Show/hide the collision message.
			showConflictMessage(ifolder.HasConflicts);

			try
			{
				// Get the sync node and byte counts.
				SyncSize syncSize = ifWebService.CalculateSyncSize(ifolder.ID);
				objectCount.Text = syncSize.SyncNodeCount.ToString();
				byteCount.Text = syncSize.SyncByteCount.ToString();
			}
			catch (WebException ex)
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered while querying for sync statistics.");
			}
			catch (Exception ex)
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered while querying for sync statistics.");
			}

			shareWith.Items.Clear();
			shareWith.BeginUpdate();

			try
			{
				// Load the member list.
				connectToWebService();
				iFolderUser[] ifolderUsers = ifWebService.GetiFolderUsers(ifolder.ID);
				foreach (iFolderUser ifolderUser in ifolderUsers)
				{
					if (ifolderUser.UserID.Equals(ifolder.CurrentUserID))
					{
						// Keep track of the current user
						currentUser = ifolderUser;
					}

					ShareListMember slMember = new ShareListMember();
					slMember.iFolderUser = ifolderUser;

					string[] items = new string[3];

					items[0] = ifolderUser.Name;
					items[1] = stateToString(ifolderUser.State, ifolderUser.UserID);
					int imageIndex = 1;
					items[2] = rightsToString(ifolderUser.Rights/*, out imageIndex*/);

					if ((currentUser != null) && currentUser.UserID.Equals(ifolderUser.UserID))
					{
						imageIndex = 0;
					}
					else if ((ifolderUser.State != null) && !ifolderUser.State.Equals(member))
					{
						imageIndex = 2;
					}

					ListViewItem lvitem = new ListViewItem(items, imageIndex);
					lvitem.Tag = slMember;

					if (ifolderUser.UserID.Equals(ifolder.OwnerID))
					{
						// Keep track of the current (or old) owner.
						ownerLvi = lvitem;
					}

					// TODO: track events for subscriptions.

					shareWith.Items.Add(lvitem);
				}

				// Clear the hashtable.
/*				lock (subscrHT)
				{
					subscrHT.Clear();
				}*/

				// Set up the event handlers for the POBox.
				// TODO: we still can't get events into explorer ... this may work once we are in the GAC.
/*				subscriber = new EventSubscriber(poBox.ID);
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
*/
				// Select the first item in the list.
				shareWith.Items[0].Selected = true;
			}
			catch (WebException ex)
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered while reading iFolder members.");

				if (ex.Status == WebExceptionStatus.ConnectFailure)
				{
					ifWebService = null;
				}
			}
			catch (Exception ex)
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered while reading iFolder members.");
			}

			shareWith.EndUpdate();

			// Enable/disable the buttons.
			add.Enabled = remove.Enabled = menuFullControl.Enabled = 
				menuReadWrite.Enabled = menuReadOnly.Enabled = access.Enabled = 
				setLimit.Visible = limitEdit.Visible = currentUser != null ? currentUser.Rights.Equals("Admin") : false;

			limitLabel.Visible = limit.Visible = !setLimit.Visible;

			// Restore the cursor.
			Cursor = Cursors.Default;
		}

		private string rightsToString(string rights/*, out int imageIndex*/)
		{
			string rightsString = null;

			// TODO: Localize
			switch (rights)
			{
				case "Admin":
				{
					rightsString = "Full Control";
					//imageIndex = 3;
					break;
				}
				case "ReadWrite":
				{
					rightsString = "Read/Write";
					//imageIndex = 2;
					break;
				}
				case "ReadOnly":
				{
					rightsString = "Read Only";
					//imageIndex = 1;
					break;
				}
				default:
				{
					rightsString = "Unknown";
					//imageIndex = 4;
					break;
				}
			}

			return rightsString;
		}

		private string stateToString(string state, string userID)
		{
			string stateString;

			// TODO: Localize
			switch (state)
			{
				case "Invited":
					stateString = "Invited";
					break;
				case "WaitSync":
					stateString = "Waiting to sync";
					break;
				case "AccessRequest":
					stateString = "Access requested";
					break;
				case "Declined":
					stateString = "Declined";
					break;
				case "Member":
					stateString = userID.Equals(ifolder.OwnerID) ? "Owner" : "";
					break;
				default:
					stateString = "Unknown";
					break;
			}

			return stateString;
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
					connectToWebService();
					ShareListMember oldOwner = (ShareListMember)ownerLvi.Tag;
					ShareListMember newOwner = (ShareListMember)newOwnerLvi.Tag;
					ifWebService.ChangeOwner(ifolder.ID, newOwner.iFolderUser.UserID, oldOwner.iFolderUser.Rights);
					oldOwner.Changed = newOwner.Changed = false;
				}
				catch (WebException e)
				{
					// TODO: Localize
					MessageBox.Show("An error was encountered while attempting to change the owner of this iFolder.");

					if (e.Status == WebExceptionStatus.ConnectFailure)
					{
						ifWebService = null;
					}
				}
				catch (Exception e)
				{
					// TODO: Localize
					MessageBox.Show("An error was encountered while attempting to change the owner of this iFolder.");
				}
			}

			//string sendersEmail = null;

			foreach (ListViewItem lvitem in shareWith.Items)
			{
				ShareListMember slMember = (ShareListMember)lvitem.Tag;
				try
				{
					connectToWebService();

					// Process added and changed members.
					if (slMember.Added)
					{
						// TODO: we'll get the email a different way in the future.
/*						if (ifolder.Domain.Equals(Domain.WorkGroupDomainID) &&
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
						}*/

						// Send the invitation.
						ifWebService.InviteUser(ifolder.ID, slMember.iFolderUser.UserID, slMember.iFolderUser.Rights);

						// TODO: Localize
						lvitem.SubItems[1].Text = "Invited";

						// Update the state.
						slMember.Added = false;
					}
					else if (slMember.Changed)
					{
						ifWebService.SetUserRights(ifolder.ID, slMember.iFolderUser.UserID, slMember.iFolderUser.Rights);

						// Reset the flags.
						slMember.Changed = false;
					}
				}
				catch (WebException e)
				{
					// TODO: Localize
					MessageBox.Show("An error was encountered while trying to commit changes for: " + slMember.iFolderUser.Name);
					if (e.Status == WebExceptionStatus.ConnectFailure)
					{
						ifWebService = null;
					}
				}
				catch (Exception e)
				{
					// TODO: Localize
					MessageBox.Show("An error was encountered while trying to commit changes for: " + slMember.iFolderUser.Name);
				}
			}

			// process the removedList
			if (removedList != null)
			{
				foreach (ShareListMember slMember in removedList)
				{
					connectToWebService();
					try
					{
						if (slMember.iFolderUser.State.Equals("Member"))
						{
							// Delete the member.
							ifWebService.RemoveiFolderUser(ifolder.ID, slMember.iFolderUser.UserID);
						}
						else
						{
							// Delete the subscription.
							ifWebService.RemoveSubscription(ifolder.Domain, slMember.iFolderUser.ID, ifolder.CurrentUserID);
						}
					}
					catch (WebException e)
					{
						// TODO: Localize
						MessageBox.Show("Remove failed with the following exception: \n\n" + e.Message, "Remove Failure");
						if (e.Status == WebExceptionStatus.ConnectFailure)
						{
							ifWebService = null;
						}
					}
					catch (Exception e)
					{
						//TODO: Localize
						MessageBox.Show("Remove failed with the following exception: \n\n" + e.Message, "Remove Failure");
					}
				}

				// Clear the list.
				removedList.Clear();
			}

			try
			{
				connectToWebService();

				// Update the sync interval.
				if (ifolder.SyncInterval != (int)syncInterval.Value)
				{
					ifWebService.SetiFolderSyncInterval(ifolder.ID, (int)syncInterval.Value);
				}

				// Update the disk quota policy.
				if (setLimit.Checked)
				{
					ifWebService.SetiFolderDiskSpaceLimit(ifolder.ID, (long)(long.Parse(limitEdit.Text) * megaByte));
				}
				else
				{
					ifWebService.SetiFolderDiskSpaceLimit(ifolder.ID, 0);
				}

				updateDiskQuotaDisplay();
			}
			catch (WebException e)
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered while committing policy changes.");
				if (e.Status == WebExceptionStatus.ConnectFailure)
				{
					ifWebService = null;
				}
			}
			catch (Exception e)
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered while committing policy changes.");
			}

			// Disable the apply button.
			apply.Enabled = false;

			// Restore the cursor.
			Cursor = Cursors.Default;
		}

		private void updateSelectedListViewItems(string rights)
		{
			foreach (ListViewItem lvi in shareWith.SelectedItems)
			{
				updateListViewItem(lvi, rights);
			}
		}

		private void updateListViewItem(ListViewItem lvi, string rights)
		{
			ShareListMember slMember = (ShareListMember)lvi.Tag;

			//int imageIndex;
			string access = rightsToString(rights/*, out imageIndex*/);

			try
			{
				if (slMember.iFolderUser.UserID.Equals(currentUser.UserID) ||
					slMember.iFolderUser.UserID.Equals(ifolder.OwnerID) ||
					((newOwnerLvi != null) && lvi.Equals(this.newOwnerLvi)))
				{
					// Don't allow current user, owner, or new owner to be modified.
				}
				else
				{
					if (!slMember.iFolderUser.Rights.Equals(rights))
					{
						// Mark this item as changed.
						slMember.Changed = true;

						// Set the rights.
						slMember.iFolderUser.Rights = rights;

						// Change the subitem text.
						lvi.SubItems[2].Text = access;

						// Enable the apply button.
						apply.Enabled = true;
					}

					// Don't change the image if this item is not a member.
					if (slMember.iFolderUser.State.Equals(member))
					{
						//lvi.ImageIndex = imageIndex;
						lvi.SubItems[1].Text = stateToString(slMember.iFolderUser.State, slMember.iFolderUser.UserID);
					}
				}
			}
			catch{}
		}
		#endregion

		#region Properties
		public iFolderWebService iFolderWebService
		{
			set { ifWebService = value; }
		}

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
				//contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_contact_read.ico")));
				//contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_contact_read_write.ico")));
				//contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_contact_full.ico")));
				contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_contact_card.ico")));
				contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "inviteduser.ico")));

				//Assign the ImageList objects to the books ListView.
				shareWith.SmallImageList = contactsImageList;

//				pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
				pictureBox1.Image = new Icon(new Icon(Path.Combine(basePath, "ifolderconflict.ico")), 32, 32).ToBitmap();

				Bitmap bitmap = new Bitmap(Path.Combine(basePath, "OpenFolder.bmp"));
				bitmap.MakeTransparent(bitmap.GetPixel(0,0));
				this.open.Image = bitmap;
			}
			catch {} // non-fatal ... just missing some graphics.

			// Hashtable used to store subscriptions in.
			//subscrHT = new Hashtable();

			try
			{
				connectToWebService();

				// Add all iFolders to the drop-down list.
				iFolder[] ifolderArray = ifWebService.GetAlliFolders();
				foreach (iFolder i in ifolderArray)
				{
					if ((i.Type != null) && i.Type.Equals("iFolder") && 
						(i.State != null) && i.State.Equals("Local"))
					{
						iFolderInfo ifolderInfo = new iFolderInfo();
						ifolderInfo.LocalPath = i.UnManagedPath;
						ifolderInfo.ID = i.ID;
						ifolders.Items.Add(ifolderInfo);

						// Set the passed in iFolder as the selected one.
						if ((ifolder != null) && ifolder.ID.Equals(ifolderInfo.ID))
						{
							ifolders.SelectedItem = ifolderInfo;
						}
					}
				}
			}
			catch (WebException ex)
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered reading iFolder data.");
				if (ex.Status == WebExceptionStatus.ConnectFailure)
				{
					ifWebService = null;
				}
			}
			catch (Exception ex)
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered reading iFolder data.");
			}
		}

		private void shareWith_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (shareWith.SelectedItems.Count == 1)
			{
				// Only one item is selected ... get the ListViewItem for the selected item.
				ListViewItem lvi = shareWith.SelectedItems[0];

				// TODO: handle subscriptions.
				// Enable the accept and decline buttons if the subscription state is "Pending".
				//accept.Enabled = decline.Enabled = lvi.SubItems[1].Text.Equals("TODO:");
			}
			else
			{
				// Multiple items are selected ... disable
				// the accept and decline buttons.
				//accept.Enabled = decline.Enabled = false;
			}

			if ((shareWith.SelectedItems.Count == 1) && 
				(((ShareListMember)shareWith.SelectedItems[0].Tag).iFolderUser.UserID.Equals(currentUser.UserID) ||
				((ShareListMember)shareWith.SelectedItems[0].Tag).iFolderUser.UserID.Equals(ifolder.OwnerID) ||
				((newOwnerLvi != null) && shareWith.SelectedItems[0].Equals(newOwnerLvi))))
			{
				// The current member, owner or new owner is the only one selected, disable the access control
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
					(shareWith.SelectedItems.Count != 0 && currentUser.Rights.Equals("Admin"));
			}
		}

		private void add_Click(object sender, System.EventArgs e)
		{
			Picker picker = new Picker();
			picker.LoadPath = loadPath;
			picker.iFolderWebService = ifWebService;
			DialogResult result = picker.ShowDialog();
			if (result == DialogResult.OK)
			{
				// Unselect all items.
				shareWith.SelectedItems.Clear();

				// Enable the apply button.
				if (picker.AddedUsers.Count > 0)
					apply.Enabled = true;

				foreach (ListViewItem lvi in picker.AddedUsers)
				{
					iFolderUser user = (iFolderUser)((ListViewItem)lvi.Tag).Tag;
					ShareListMember slMember = new ShareListMember();
					slMember.Added = true;
					slMember.iFolderUser = new iFolderUser();
					slMember.iFolderUser.Name = user.Name;
					slMember.iFolderUser.UserID = user.UserID;
					slMember.iFolderUser.Rights = "ReadWrite";
					slMember.iFolderUser.State = "Inviting";

					string[] items = new string[3];
					items[0] = user.Name;
					// TODO: Localize
					items[1] = "Ready to invite";
					items[2] = rightsToString(slMember.iFolderUser.Rights);
					ListViewItem lvitem = new ListViewItem(items, 2);
					lvitem.Tag = slMember;
					lvitem.Selected = true;
					shareWith.Items.Add(lvitem);
				}
			}

/*			if (!IsCurrentUserValid())
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
*/
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

/*					if (shareMember == null)
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
*/		}

		private void remove_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem lvi in shareWith.SelectedItems)
			{
				ShareListMember slMember = (ShareListMember)lvi.Tag;

				try
				{
					// Don't allow the current user to be removed.
					if (!currentUser.UserID.Equals(slMember.iFolderUser.UserID) ||
						!slMember.iFolderUser.State.Equals("Member"))
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
/*						if (slMember.Subscription != null)
						{
							lock (subscrHT)
							{
								subscrHT.Remove(slMember.Subscription.ID);
							}
						}*/

						// Enable the apply button.
						apply.Enabled = true;
					}
				}
				catch (Exception ex)
				{
				}
			}
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			processChanges();
			Close();
		}

		private void accept_Click(object sender, System.EventArgs e)
		{
/*			ListViewItem lvi = this.shareWith.SelectedItems[0];
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
*/		}

		private void decline_Click(object sender, System.EventArgs e)
		{
/*			ListViewItem lvi = this.shareWith.SelectedItems[0];
			ShareListMember slMember = (ShareListMember)lvi.Tag;
			slMember.Subscription.Decline();
			poBox.Commit(slMember.Subscription);
			lvi.Remove();
*/		}

		private void apply_Click(object sender, System.EventArgs e)
		{
			this.processChanges();

			try
			{
				connectToWebService();

				// Reload the collection.
				string id = ifolder.ID;
				ifolder = null;
				ifolder = ifWebService.GetiFolder(id);
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

		private void conflicts_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			ConflictResolver conflictResolver = new ConflictResolver();
			conflictResolver.iFolder = ifolder;
			conflictResolver.iFolderWebService = ifWebService;
			conflictResolver.LoadPath = loadPath;
			conflictResolver.ConflictsResolved += new Novell.iFolderCom.ConflictResolver.ConflictsResolvedDelegate(conflictResolver_ConflictsResolved);
			conflictResolver.ShowDialog();		
		}

		private void conflictResolver_ConflictsResolved(object sender, EventArgs e)
		{
			showConflictMessage(false);
		}

/*		private void subscriber_NodeCreated(NodeEventArgs args)
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
				//logger.Debug(ex, "OnNodeCreated");
			}
		}*/

/*		private void subscriber_NodeDeleted(NodeEventArgs args)
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
		}*/

/*		private void subscriber_NodeChanged(NodeEventArgs args)
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
						//logger.Debug(ex, "OnNodeChanged");
					}
				}
			}
		}*/

		private void setLimit_CheckedChanged(object sender, System.EventArgs e)
		{
			limitEdit.Enabled = setLimit.Checked;
			if (setLimit.Focused)
			{
				apply.Enabled = true;
			}
		}

		private void limitEdit_TextChanged(object sender, System.EventArgs e)
		{
			if (limitEdit.Focused)
			{
				apply.Enabled = true;
			}
		}

		private void ifolders_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (apply.Enabled)
			{
				MyMessageBox mmb = new MyMessageBox();
				// TODO: Localize
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

			try
			{
				connectToWebService();
				ifolder = ifWebService.GetiFolder(((iFolderInfo)ifolders.SelectedItem).ID);
				//TODO: Localize
				this.Text = "iFolder Properties for " + Path.GetFileName(ifolder.UnManagedPath);
				refreshData();
			}
			catch (WebException ex)
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered while reading iFolder data.");
				if (ex.Status == WebExceptionStatus.ConnectFailure)
				{
					ifWebService = null;
				}
			}
			catch (Exception ex)
			{
				// TODO: Localize
				MessageBox.Show("An error was encountered while reading iFolder data.");
			}
		}

		private void open_Click(object sender, System.EventArgs e)
		{
			try
			{
				System.Diagnostics.Process.Start(((iFolderInfo)ifolders.SelectedItem).LocalPath);
			}
			catch (Exception ex)
			{
				// TODO: Localize
				MessageBox.Show("Unable to open iFolder: " + ifolder.Name);
			}
		}

		private void access_Click(object sender, System.EventArgs e)
		{
			UserProperties userProperties = new UserProperties();
			userProperties.OwnerCanBeSet = (currentUser.UserID.Equals(ifolder.OwnerID) && (shareWith.SelectedItems.Count == 1));
			if (shareWith.SelectedItems.Count == 1)
			{
				ListViewItem lvi = shareWith.SelectedItems[0];
				// TODO: Localize
				userProperties.Title = "Properties for " + lvi.Text;
				userProperties.Rights = ((ShareListMember)lvi.Tag).iFolderUser.Rights;
				userProperties.CanBeOwner = ((ShareListMember)lvi.Tag).iFolderUser.State.Equals(member);
				userProperties.IsOwner = newOwnerLvi != null ? lvi.Equals(newOwnerLvi) : lvi.Equals(ownerLvi);
			}

			if (DialogResult.OK == userProperties.ShowDialog())
			{
				updateSelectedListViewItems(userProperties.Rights);

				if (shareWith.SelectedItems.Count == 1)
				{
					// Update the owner.
					if (userProperties.IsOwner)
					{
						ListViewItem lvi = shareWith.SelectedItems[0];
						// TODO: Localize
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

						// Disable the remove and access buttons.
						access.Enabled = remove.Enabled = false;

						// Enable the apply button.
						apply.Enabled = true;
					}
				}
			}
		}

		private void shareWith_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			// Only display the access menu when clicking in the Access column.
			if (e.Button.Equals(MouseButtons.Right))
			{
				accessClick = e.X > (columnHeader1.Width + columnHeader2.Width);
			}
		}

		private void contextMenu1_Popup(object sender, System.EventArgs e)
		{
			if (accessClick)
			{
				menuFullControl.Visible = menuReadWrite.Visible = menuReadOnly.Visible = shareWith.SelectedItems.Count != 0;

				if (shareWith.SelectedItems.Count == 1)
				{
					ShareListMember slMember = (ShareListMember)shareWith.SelectedItems[0].Tag;
					switch (slMember.iFolderUser.Rights)
					{
						case "Admin":
							menuFullControl.Checked = true;
							menuReadWrite.Checked = menuReadOnly.Checked = false;
							break;
						case "ReadWrite":
							menuReadWrite.Checked = true;
							menuFullControl.Checked = menuReadOnly.Checked = false;
							break;
						case "ReadOnly":
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
			else
			{
				menuFullControl.Visible = menuReadWrite.Visible = menuReadOnly.Visible = false;
			}
		}

		private void menuFullControl_Click(object sender, System.EventArgs e)
		{
			updateSelectedListViewItems("Admin");
		}

		private void menuReadWrite_Click(object sender, System.EventArgs e)
		{
			updateSelectedListViewItems("ReadWrite");
		}

		private void menuReadOnly_Click(object sender, System.EventArgs e)
		{
			updateSelectedListViewItems("ReadOnly");
		}

		private void shareWith_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				remove_Click(this, new System.EventArgs());
			}
		}

		private void tabControl1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			// Change focus when dialog is displayed non-modal ... for some reason this doesn't
			// happen automatically when the form is displayed from the shell extension.
			if (!this.Modal && e.KeyCode == Keys.Tab)
			{
				try
				{
					// The focus does change for a ComboBox ... hmmm ... so if the current control
					// is a ComboBox, we will skip to the next control after the ComboBox.
					bool skip = currentControl.GetType().Equals(typeof(System.Windows.Forms.ComboBox));
					while (true)
					{
						currentControl = this.GetNextControl(currentControl, !e.Shift);
						if (currentControl == null)
						{
							currentControl = e.Shift ? lastControl : firstControl;
						}

						if (currentControl.CanFocus)
						{
							Type type = currentControl.GetType();

							// Labels, TabPages and GroupBoxes can't really have the focus.
							if (!type.Equals(typeof(System.Windows.Forms.Label)) &&
								!type.Equals(typeof(System.Windows.Forms.TabPage)) &&
								!type.Equals(typeof(System.Windows.Forms.GroupBox)))
							{
								if (skip)
								{
									skip = false;
									continue;
								}
								else
								{
									break;
								}
							}
						}
					}

					currentControl.Focus();
				}
				catch
				{
					// Ignore.
				}
			}
		}
		#endregion

		private void connectToWebService()
		{
			if (ifWebService == null)
			{
//				DateTime currentTime = DateTime.Now;
//				if ((currentTime.Ticks - ticks) > delta)
				{
//					ticks = currentTime.Ticks;
					ifWebService = new iFolderWebService();
				}
			}
		}
	}
}
