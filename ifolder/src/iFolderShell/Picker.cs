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
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Novell.iFolderCom
{
	/// <summary>
	/// Summary description for Picker.
	/// </summary>
	[ComVisible(false)]
	public class Picker : System.Windows.Forms.Form
	{
		#region Class Members
		System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(Picker));
		private string searchContext;
		private string searchText = null;
		private int itemsViewable;
		private int index;
		private bool stopThread = false;
		private bool noSearch = true;
		private Thread worker = null;
		/// <summary>
		/// Event used to signal that there is work to do.
		/// </summary>
		protected AutoResetEvent workEvent = null;

		/// <summary>
		/// Event used to signal that the thread has started.
		/// </summary>
		protected AutoResetEvent waitEvent = null;

		private delegate void AddLVItemEventDelegate(iFolderUser[] ifolderUsers);
		private AddLVItemEventDelegate addLVItemEventDelegate;

		private delegate void BeginAddLVItemEventDelegate();
		private BeginAddLVItemEventDelegate beginAddLVItemEventDelegate;

		private System.Windows.Forms.Button add;
		private System.Windows.Forms.Button remove;
		private System.Windows.Forms.TextBox search;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private ArrayList removedList;
		private iFolderUser currentUser;
		private iFolderUser currentOwner;
		private Hashtable ht = null;
		private Hashtable addedHT = null;
		private string loadPath;
		private iFolderWebService ifWebService;
		private string domainID;
		private System.Windows.Forms.ListView rosterLV;
		private System.Windows.Forms.ListView addedLV;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Timer searchTimer;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox attributeName;
		private System.ComponentModel.IContainer components;
		#endregion

		/// <summary>
		/// Constructs a Picker object.
		/// </summary>
		public Picker()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			removedList = new ArrayList();
			addedHT = new Hashtable();

			addLVItemEventDelegate = new AddLVItemEventDelegate(addLVItemEvent);
			beginAddLVItemEventDelegate = new BeginAddLVItemEventDelegate(beginAddLVItemEvent);

			workEvent = new AutoResetEvent(false);
			waitEvent = new AutoResetEvent(false);

			this.StartPosition = FormStartPosition.CenterParent;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Picker));
			this.rosterLV = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.addedLV = new System.Windows.Forms.ListView();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.add = new System.Windows.Forms.Button();
			this.remove = new System.Windows.Forms.Button();
			this.search = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.searchTimer = new System.Windows.Forms.Timer(this.components);
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel4 = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.attributeName = new System.Windows.Forms.ComboBox();
			this.panel1.SuspendLayout();
			this.panel4.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// rosterLV
			// 
			this.rosterLV.AccessibleDescription = resources.GetString("rosterLV.AccessibleDescription");
			this.rosterLV.AccessibleName = resources.GetString("rosterLV.AccessibleName");
			this.rosterLV.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("rosterLV.Alignment")));
			this.rosterLV.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("rosterLV.Anchor")));
			this.rosterLV.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("rosterLV.BackgroundImage")));
			this.rosterLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					   this.columnHeader1});
			this.rosterLV.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("rosterLV.Dock")));
			this.rosterLV.Enabled = ((bool)(resources.GetObject("rosterLV.Enabled")));
			this.rosterLV.Font = ((System.Drawing.Font)(resources.GetObject("rosterLV.Font")));
			this.rosterLV.HideSelection = false;
			this.rosterLV.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("rosterLV.ImeMode")));
			this.rosterLV.LabelWrap = ((bool)(resources.GetObject("rosterLV.LabelWrap")));
			this.rosterLV.Location = ((System.Drawing.Point)(resources.GetObject("rosterLV.Location")));
			this.rosterLV.Name = "rosterLV";
			this.rosterLV.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("rosterLV.RightToLeft")));
			this.rosterLV.Size = ((System.Drawing.Size)(resources.GetObject("rosterLV.Size")));
			this.rosterLV.TabIndex = ((int)(resources.GetObject("rosterLV.TabIndex")));
			this.rosterLV.Text = resources.GetString("rosterLV.Text");
			this.rosterLV.View = System.Windows.Forms.View.Details;
			this.rosterLV.Visible = ((bool)(resources.GetObject("rosterLV.Visible")));
			this.rosterLV.DoubleClick += new System.EventHandler(this.add_Click);
			this.rosterLV.SelectedIndexChanged += new System.EventHandler(this.rosterLV_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = resources.GetString("columnHeader1.Text");
			this.columnHeader1.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader1.TextAlign")));
			this.columnHeader1.Width = ((int)(resources.GetObject("columnHeader1.Width")));
			// 
			// addedLV
			// 
			this.addedLV.AccessibleDescription = resources.GetString("addedLV.AccessibleDescription");
			this.addedLV.AccessibleName = resources.GetString("addedLV.AccessibleName");
			this.addedLV.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("addedLV.Alignment")));
			this.addedLV.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("addedLV.Anchor")));
			this.addedLV.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("addedLV.BackgroundImage")));
			this.addedLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					  this.columnHeader2});
			this.addedLV.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("addedLV.Dock")));
			this.addedLV.Enabled = ((bool)(resources.GetObject("addedLV.Enabled")));
			this.addedLV.Font = ((System.Drawing.Font)(resources.GetObject("addedLV.Font")));
			this.addedLV.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("addedLV.ImeMode")));
			this.addedLV.LabelWrap = ((bool)(resources.GetObject("addedLV.LabelWrap")));
			this.addedLV.Location = ((System.Drawing.Point)(resources.GetObject("addedLV.Location")));
			this.addedLV.Name = "addedLV";
			this.addedLV.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("addedLV.RightToLeft")));
			this.addedLV.Size = ((System.Drawing.Size)(resources.GetObject("addedLV.Size")));
			this.addedLV.TabIndex = ((int)(resources.GetObject("addedLV.TabIndex")));
			this.addedLV.Text = resources.GetString("addedLV.Text");
			this.addedLV.View = System.Windows.Forms.View.Details;
			this.addedLV.Visible = ((bool)(resources.GetObject("addedLV.Visible")));
			this.addedLV.DoubleClick += new System.EventHandler(this.remove_Click);
			this.addedLV.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.addedLV_ColumnClick);
			this.addedLV.SelectedIndexChanged += new System.EventHandler(this.addedLV_SelectedIndexChanged);
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = resources.GetString("columnHeader2.Text");
			this.columnHeader2.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader2.TextAlign")));
			this.columnHeader2.Width = ((int)(resources.GetObject("columnHeader2.Width")));
			// 
			// add
			// 
			this.add.AccessibleDescription = resources.GetString("add.AccessibleDescription");
			this.add.AccessibleName = resources.GetString("add.AccessibleName");
			this.add.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("add.Anchor")));
			this.add.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("add.BackgroundImage")));
			this.add.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("add.Dock")));
			this.add.Enabled = ((bool)(resources.GetObject("add.Enabled")));
			this.add.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("add.FlatStyle")));
			this.add.Font = ((System.Drawing.Font)(resources.GetObject("add.Font")));
			this.add.Image = ((System.Drawing.Image)(resources.GetObject("add.Image")));
			this.add.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("add.ImageAlign")));
			this.add.ImageIndex = ((int)(resources.GetObject("add.ImageIndex")));
			this.add.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("add.ImeMode")));
			this.add.Location = ((System.Drawing.Point)(resources.GetObject("add.Location")));
			this.add.Name = "add";
			this.add.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("add.RightToLeft")));
			this.add.Size = ((System.Drawing.Size)(resources.GetObject("add.Size")));
			this.add.TabIndex = ((int)(resources.GetObject("add.TabIndex")));
			this.add.Text = resources.GetString("add.Text");
			this.add.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("add.TextAlign")));
			this.add.Visible = ((bool)(resources.GetObject("add.Visible")));
			this.add.Click += new System.EventHandler(this.add_Click);
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
			this.remove.Click += new System.EventHandler(this.remove_Click);
			// 
			// search
			// 
			this.search.AccessibleDescription = resources.GetString("search.AccessibleDescription");
			this.search.AccessibleName = resources.GetString("search.AccessibleName");
			this.search.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("search.Anchor")));
			this.search.AutoSize = ((bool)(resources.GetObject("search.AutoSize")));
			this.search.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("search.BackgroundImage")));
			this.search.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("search.Dock")));
			this.search.Enabled = ((bool)(resources.GetObject("search.Enabled")));
			this.search.Font = ((System.Drawing.Font)(resources.GetObject("search.Font")));
			this.search.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("search.ImeMode")));
			this.search.Location = ((System.Drawing.Point)(resources.GetObject("search.Location")));
			this.search.MaxLength = ((int)(resources.GetObject("search.MaxLength")));
			this.search.Multiline = ((bool)(resources.GetObject("search.Multiline")));
			this.search.Name = "search";
			this.search.PasswordChar = ((char)(resources.GetObject("search.PasswordChar")));
			this.search.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("search.RightToLeft")));
			this.search.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("search.ScrollBars")));
			this.search.Size = ((System.Drawing.Size)(resources.GetObject("search.Size")));
			this.search.TabIndex = ((int)(resources.GetObject("search.TabIndex")));
			this.search.Text = resources.GetString("search.Text");
			this.search.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("search.TextAlign")));
			this.search.Visible = ((bool)(resources.GetObject("search.Visible")));
			this.search.WordWrap = ((bool)(resources.GetObject("search.WordWrap")));
			this.search.TextChanged += new System.EventHandler(this.search_TextChanged);
			this.search.Enter += new System.EventHandler(this.search_Enter);
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
			// ok
			// 
			this.ok.AccessibleDescription = resources.GetString("ok.AccessibleDescription");
			this.ok.AccessibleName = resources.GetString("ok.AccessibleName");
			this.ok.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ok.Anchor")));
			this.ok.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ok.BackgroundImage")));
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ok.Dock")));
			this.ok.Enabled = ((bool)(resources.GetObject("ok.Enabled")));
			this.ok.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("ok.FlatStyle")));
			this.ok.Font = ((System.Drawing.Font)(resources.GetObject("ok.Font")));
			this.ok.Image = ((System.Drawing.Image)(resources.GetObject("ok.Image")));
			this.ok.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ok.ImageAlign")));
			this.ok.ImageIndex = ((int)(resources.GetObject("ok.ImageIndex")));
			this.ok.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ok.ImeMode")));
			this.ok.Location = ((System.Drawing.Point)(resources.GetObject("ok.Location")));
			this.ok.Name = "ok";
			this.ok.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ok.RightToLeft")));
			this.ok.Size = ((System.Drawing.Size)(resources.GetObject("ok.Size")));
			this.ok.TabIndex = ((int)(resources.GetObject("ok.TabIndex")));
			this.ok.Text = resources.GetString("ok.Text");
			this.ok.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ok.TextAlign")));
			this.ok.Visible = ((bool)(resources.GetObject("ok.Visible")));
			// 
			// cancel
			// 
			this.cancel.AccessibleDescription = resources.GetString("cancel.AccessibleDescription");
			this.cancel.AccessibleName = resources.GetString("cancel.AccessibleName");
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cancel.Anchor")));
			this.cancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cancel.BackgroundImage")));
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
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
			// 
			// searchTimer
			// 
			this.searchTimer.Interval = 1000;
			this.searchTimer.Tick += new System.EventHandler(this.searchTimer_Tick);
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
			this.panel1.Controls.Add(this.panel4);
			this.panel1.Controls.Add(this.panel3);
			this.panel1.Controls.Add(this.panel2);
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
			// panel4
			// 
			this.panel4.AccessibleDescription = resources.GetString("panel4.AccessibleDescription");
			this.panel4.AccessibleName = resources.GetString("panel4.AccessibleName");
			this.panel4.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panel4.Anchor")));
			this.panel4.AutoScroll = ((bool)(resources.GetObject("panel4.AutoScroll")));
			this.panel4.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panel4.AutoScrollMargin")));
			this.panel4.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panel4.AutoScrollMinSize")));
			this.panel4.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel4.BackgroundImage")));
			this.panel4.Controls.Add(this.addedLV);
			this.panel4.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panel4.Dock")));
			this.panel4.Enabled = ((bool)(resources.GetObject("panel4.Enabled")));
			this.panel4.Font = ((System.Drawing.Font)(resources.GetObject("panel4.Font")));
			this.panel4.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panel4.ImeMode")));
			this.panel4.Location = ((System.Drawing.Point)(resources.GetObject("panel4.Location")));
			this.panel4.Name = "panel4";
			this.panel4.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panel4.RightToLeft")));
			this.panel4.Size = ((System.Drawing.Size)(resources.GetObject("panel4.Size")));
			this.panel4.TabIndex = ((int)(resources.GetObject("panel4.TabIndex")));
			this.panel4.Text = resources.GetString("panel4.Text");
			this.panel4.Visible = ((bool)(resources.GetObject("panel4.Visible")));
			// 
			// panel3
			// 
			this.panel3.AccessibleDescription = resources.GetString("panel3.AccessibleDescription");
			this.panel3.AccessibleName = resources.GetString("panel3.AccessibleName");
			this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panel3.Anchor")));
			this.panel3.AutoScroll = ((bool)(resources.GetObject("panel3.AutoScroll")));
			this.panel3.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panel3.AutoScrollMargin")));
			this.panel3.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panel3.AutoScrollMinSize")));
			this.panel3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel3.BackgroundImage")));
			this.panel3.Controls.Add(this.add);
			this.panel3.Controls.Add(this.remove);
			this.panel3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panel3.Dock")));
			this.panel3.Enabled = ((bool)(resources.GetObject("panel3.Enabled")));
			this.panel3.Font = ((System.Drawing.Font)(resources.GetObject("panel3.Font")));
			this.panel3.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panel3.ImeMode")));
			this.panel3.Location = ((System.Drawing.Point)(resources.GetObject("panel3.Location")));
			this.panel3.Name = "panel3";
			this.panel3.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panel3.RightToLeft")));
			this.panel3.Size = ((System.Drawing.Size)(resources.GetObject("panel3.Size")));
			this.panel3.TabIndex = ((int)(resources.GetObject("panel3.TabIndex")));
			this.panel3.Text = resources.GetString("panel3.Text");
			this.panel3.Visible = ((bool)(resources.GetObject("panel3.Visible")));
			// 
			// panel2
			// 
			this.panel2.AccessibleDescription = resources.GetString("panel2.AccessibleDescription");
			this.panel2.AccessibleName = resources.GetString("panel2.AccessibleName");
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panel2.Anchor")));
			this.panel2.AutoScroll = ((bool)(resources.GetObject("panel2.AutoScroll")));
			this.panel2.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panel2.AutoScrollMargin")));
			this.panel2.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panel2.AutoScrollMinSize")));
			this.panel2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel2.BackgroundImage")));
			this.panel2.Controls.Add(this.rosterLV);
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
			// attributeName
			// 
			this.attributeName.AccessibleDescription = resources.GetString("attributeName.AccessibleDescription");
			this.attributeName.AccessibleName = resources.GetString("attributeName.AccessibleName");
			this.attributeName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("attributeName.Anchor")));
			this.attributeName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("attributeName.BackgroundImage")));
			this.attributeName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("attributeName.Dock")));
			this.attributeName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.attributeName.Enabled = ((bool)(resources.GetObject("attributeName.Enabled")));
			this.attributeName.Font = ((System.Drawing.Font)(resources.GetObject("attributeName.Font")));
			this.attributeName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("attributeName.ImeMode")));
			this.attributeName.IntegralHeight = ((bool)(resources.GetObject("attributeName.IntegralHeight")));
			this.attributeName.ItemHeight = ((int)(resources.GetObject("attributeName.ItemHeight")));
			this.attributeName.Items.AddRange(new object[] {
															   resources.GetString("attributeName.Items"),
															   resources.GetString("attributeName.Items1"),
															   resources.GetString("attributeName.Items2")});
			this.attributeName.Location = ((System.Drawing.Point)(resources.GetObject("attributeName.Location")));
			this.attributeName.MaxDropDownItems = ((int)(resources.GetObject("attributeName.MaxDropDownItems")));
			this.attributeName.MaxLength = ((int)(resources.GetObject("attributeName.MaxLength")));
			this.attributeName.Name = "attributeName";
			this.attributeName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("attributeName.RightToLeft")));
			this.attributeName.Size = ((System.Drawing.Size)(resources.GetObject("attributeName.Size")));
			this.attributeName.TabIndex = ((int)(resources.GetObject("attributeName.TabIndex")));
			this.attributeName.Text = resources.GetString("attributeName.Text");
			this.attributeName.Visible = ((bool)(resources.GetObject("attributeName.Visible")));
			this.attributeName.SelectedIndexChanged += new System.EventHandler(this.attributeName_SelectedIndexChanged);
			// 
			// Picker
			// 
			this.AcceptButton = this.ok;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.cancel;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.attributeName);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.search);
			this.Controls.Add(this.label1);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "Picker";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Picker_Closing);
			this.SizeChanged += new System.EventHandler(this.Picker_SizeChanged);
			this.Load += new System.EventHandler(this.Picker_Load);
			this.panel1.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		public string DomainID
		{
			set {domainID = value;}
		}

		/// <summary>
		/// Gets/sets the path where the assembly was loaded from.
		/// </summary>
		public string LoadPath
		{
			get {return loadPath; }
			set {loadPath = value; }
		}

		/// <summary>
		/// Set the web service to use.
		/// </summary>
		public iFolderWebService iFolderWebService
		{
			set {ifWebService = value; }
		}

		/// <summary>
		/// Gets the list of added users.
		/// </summary>
		public ListView.ListViewItemCollection AddedUsers
		{
			get
			{
				return addedLV.Items;
			}
		}

		/// <summary>
		/// Sets the hashtable containing existing (already added) users.
		/// </summary>
		public Hashtable Ht
		{
			set { ht = value; }
		}

		/// <summary>
		/// Sets the current user for this iFolder.
		/// </summary>
		public iFolderUser CurrentUser
		{
			set { currentUser = value; }
		}

		/// <summary>
		/// Sets the current owner for this iFolder.
		/// </summary>
		public iFolderUser CurrentOwner
		{
			set { currentOwner = value; }
		}

		/// <summary>
		/// Gets an array of removed users.
		/// </summary>
		public ArrayList RemovedList
		{
			get { return removedList; }
		}
		#endregion

		#region Private Methods
		private void addLVItemEvent(iFolderUser[] ifolderUsers)
		{
			rosterLV.BeginUpdate();

			try
			{
				if (ifolderUsers != null)
				{
					foreach (iFolderUser ifolderUser in ifolderUsers)
					{
						int imageIndex = ifolderUser.UserID.Equals(currentUser.UserID) ? 0 : 1;
						string name = (ifolderUser.FN != null) && !ifolderUser.FN.Equals(string.Empty) ? ifolderUser.FN : ifolderUser.Name;
						ListViewItem lvi = new ListViewItem(name, imageIndex);
						//ListViewItem lvi = rosterLV.Items[index++];
						lvi.Text = name;
						lvi.ImageIndex = imageIndex;
						lvi.Tag = ifolderUser;
						rosterLV.Items.Add(lvi);

						// Find and update items in the added list.
						ListViewItem item = (ListViewItem)addedHT[ifolderUser.UserID];
						if (item != null)
						{
							item.Tag = lvi;
							addedHT[ifolderUser.UserID] = item;
							lvi.ForeColor = Color.Gray;
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (ex.Message.IndexOf("The initial synchronization of the roster has not completed.") != -1)
				{
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("rosterNotSynced"), string.Empty, string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
					mmb.ShowDialog();
				}
				else
				{
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("memberReadError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}
			}

			rosterLV.EndUpdate();
		}

		private void beginAddLVItemEvent()
		{
			rosterLV.Items.Clear();
		}

		private void displayUsers(string search)
		{
			Cursor.Current = Cursors.WaitCursor;
			rosterLV.Items.Clear();
			rosterLV.BeginUpdate();
			int totalMembers = 0;
			index = 0;

			try
			{
				iFolderUser[] ifolderUsers;
				
				if (searchContext != null)
				{
					ifWebService.FindCloseiFolderMembers(domainID, searchContext);
					searchContext = null;
				}

				if ((search != null) && !search.Equals(string.Empty))
				{
					string attribute;
					switch (attributeName.SelectedIndex)
					{
						case 0:
							attribute = "Given";
							break;
						case 1:
							attribute = "Family";
							break;
						default:
							attribute = "FN";
							break;
					}

					ifWebService.FindFirstSpecificiFolderMembers(
						domainID,
						attribute,
						search,
						SearchType.Begins,
						25,
						out searchContext,
						out ifolderUsers,
						out totalMembers);
				}
				else
				{
					ifWebService.FindFirstiFolderMembers(
						domainID,
						25,
						out searchContext,
						out ifolderUsers,
						out totalMembers);
				}

/*				if (totalMembers > 0)
				{
					ListViewItem[] lvItems = new ListViewItem[totalMembers];
					foreach (ListViewItem lvi in lvItems)
					{
						lvItems[index] = new ListViewItem("t" + index.ToString());
						index++;
					}

					index = 0;
					rosterLV.Items.AddRange(lvItems);
				}*/

				if (ifolderUsers != null)
				{
					foreach (iFolderUser ifolderUser in ifolderUsers)
					{
						int imageIndex = ifolderUser.UserID.Equals(currentUser.UserID) ? 0 : 1;
						string name = (ifolderUser.FN != null) && !ifolderUser.FN.Equals(string.Empty) ? ifolderUser.FN : ifolderUser.Name;
						ListViewItem lvi = new ListViewItem(name, imageIndex);
//						ListViewItem lvi = rosterLV.Items[index++];
						lvi.Text = name;
						lvi.ImageIndex = imageIndex;
						lvi.Tag = ifolderUser;
						rosterLV.Items.Add(lvi);

						// Find and update items in the added list.
						ListViewItem item = (ListViewItem)addedHT[ifolderUser.UserID];
						if (item != null)
						{
							item.Tag = lvi;
							addedHT[ifolderUser.UserID] = item;
							lvi.ForeColor = Color.Gray;
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (ex.Message.IndexOf("The initial synchronization of the roster has not completed.") != -1)
				{
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("rosterNotSynced"), string.Empty, string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
					mmb.ShowDialog();
				}
				else
				{
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("memberReadError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}
			}

			rosterLV.EndUpdate();
			Cursor.Current = Cursors.Default;

//			if (totalMembers > 25)
//			{
//				stopThread = false;
//				workEvent.Set();
//			}
		}
		
		
		private void searchThreadProc()
		{
			while (true)
			{
				int totalMembers = 0;
				bool moreUsers = true;

				try
				{
					BeginInvoke(beginAddLVItemEventDelegate);

					while (!stopThread && moreUsers)
					{
						iFolderUser[] ifolderUsers;
				
						if (searchContext == null)
						{
							if ((searchText != null) && !searchText.Equals(string.Empty))
							{
								string attribute;
								switch (attributeName.SelectedIndex)
								{
									case 1:
										attribute = "Given";
										break;
									case 2:
										attribute = "Family";
										break;
									default:
										attribute = "FN";
										break;
								}

								moreUsers = ifWebService.FindFirstSpecificiFolderMembers(
									domainID,
									attribute,
									searchText,
									SearchType.Begins,
									25,
									out searchContext,
									out ifolderUsers,
									out totalMembers);
							}
							else
							{
								moreUsers = ifWebService.FindFirstiFolderMembers(
									domainID,
									25,
									out searchContext,
									out ifolderUsers,
									out totalMembers);
							}
						}
						else
						{
							moreUsers = ifWebService.FindNextiFolderMembers(
								domainID,
								ref searchContext,
								25,
								out ifolderUsers);
						}

						if (ifolderUsers != null)
						{
							// Signal the event so the wait cursor can be removed.
							waitEvent.Set();

							// BeginInvoke ...
							BeginInvoke(addLVItemEventDelegate, new object[] {ifolderUsers});
						}
					}
				}
				catch (Exception ex)
				{
					if (ex.Message.Equals("Thread was being aborted."))
					{
						// Ignore.
					}
					else if (ex.Message.IndexOf("The initial synchronization of the roster has not completed.") != -1)
					{
						MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("rosterNotSynced"), string.Empty, string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
						mmb.ShowDialog();
					}
					else
					{
						MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("memberReadError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
					}
				}

				if (searchContext != null)
				{
					try
					{
						ifWebService.FindCloseiFolderMembers(domainID, searchContext);
						searchContext = null;
					}
					catch {}
				}

				// Go to sleep until there is work to do.
				workEvent.WaitOne();
			}
		}		
		#endregion

		#region Public Methods
		public iFolderUser GetiFolderUserFromListViewItem(ListViewItem lvi)
		{
			iFolderUser ifolderUser = null;

			Type type = lvi.Tag.GetType();
			if (type.FullName.Equals(typeof(ListViewItem).ToString()))
			{
				ifolderUser = (iFolderUser)((ListViewItem)lvi.Tag).Tag;
			}
			else if (type.FullName.Equals(typeof(iFolderUser).ToString()))
			{
				ifolderUser = (iFolderUser)lvi.Tag;
			}

			return ifolderUser;
		}
		#endregion

		#region Event Handlers
		private void Picker_Load(object sender, System.EventArgs e)
		{
			// Load the images.
			try
			{
				string basePath = Path.Combine(loadPath != null ? loadPath : Application.StartupPath, "res");

				Icon = new Icon(Path.Combine(basePath, "ifolder_contact_card.ico"));

				// Initialize the ImageList objects with icons.
				rosterLV.SmallImageList = new ImageList();
				rosterLV.SmallImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_me_card.ico")));
				rosterLV.SmallImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_contact_card.ico")));
				addedLV.SmallImageList = rosterLV.SmallImageList;
			}
			catch {}

			Cursor.Current = Cursors.WaitCursor;

			if (ht != null)
			{
				// Add the existing users to the added list.
				foreach (DictionaryEntry entry in ht)
				{
					ListViewItem lvi = (ListViewItem)entry.Value;
					ListViewItem item = new ListViewItem(lvi.Text, lvi.ImageIndex == 0 ? 0 : 1);
					addedLV.Items.Add(item);
					item.Tag = ((ShareListMember)lvi.Tag).iFolderUser;
					addedHT.Add(((ShareListMember)lvi.Tag).iFolderUser.UserID, item);
				}
			}

			Cursor.Current = Cursors.Default;

			search.Text = resourceManager.GetString("searchPrompt");
			search.SelectAll();

			// TODO: need to add event handler for selection changed and tie it in with the timer.
			attributeName.SelectedIndex = 0;

			ListViewItem lvItem = new ListViewItem("Test", 0);
			rosterLV.Items.Add(lvItem);
			Rectangle rect = rosterLV.GetItemRect(0);
			itemsViewable = rosterLV.ClientSize.Height / rect.Height;
			rosterLV.Items.Clear();

			// Set waitcursor.
			Cursor.Current = Cursors.WaitCursor;
			waitEvent.WaitOne();
			Cursor.Current = Cursors.Default;
		}

		private void Picker_SizeChanged(object sender, System.EventArgs e)
		{
			panel2.Width = (panel1.Width - panel3.Width) / 2;
		}

		private void add_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem lvi in rosterLV.SelectedItems)
			{
				if (lvi.ForeColor != Color.Gray)
				{
					ok.Enabled = true;

					// Put the item in the added list.
					ListViewItem item = new ListViewItem(lvi.Text, lvi.ImageIndex);
					item.Tag = lvi;
					addedLV.Items.Add(item);

					// Change the fore-color of added items so that they can't be added again.
					lvi.ForeColor = Color.Gray;

					addedHT.Add(((iFolderUser)lvi.Tag).UserID, item);

					iFolderUser selectedUser = (iFolderUser)lvi.Tag;

					// Remove the item from the removed list.
					if (removedList.Contains(selectedUser))
					{
						removedList.Remove(selectedUser);
					}
				}
			}
		}

		private void remove_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem lvi in addedLV.SelectedItems)
			{
				iFolderUser selectedUser = GetiFolderUserFromListViewItem(lvi);

				// Remove the user if it is not the current user or current owner.
				if (!selectedUser.UserID.Equals(currentUser.UserID) && !selectedUser.UserID.Equals(currentOwner.UserID))
				{
					ok.Enabled = true;

					if (ht[selectedUser.UserID] != null)
					{
						// Add the item to the removed list.
						if (!removedList.Contains(selectedUser))
						{
							removedList.Add(selectedUser);
						}
					}

					// Change the color so it can be added again.
					Type type = lvi.Tag.GetType();
					if (type.FullName.Equals(typeof(ListViewItem).ToString()))
					{
						((ListViewItem)lvi.Tag).ForeColor = Color.Black;
					}

					// Remove the item from the list view and the hashtable.
					lvi.Remove();
					addedHT.Remove(selectedUser.UserID);
				}
			}
		}

		private void searchTimer_Tick(object sender, System.EventArgs e)
		{
			// The timer event has been fired...
			// Stop the timer.
			searchTimer.Stop();

			// Filter the user list view.
//			displayUsers(search.Text);

			searchText = search.Text;
			// Signal that there is work to do.
			stopThread = false;
			workEvent.Set();
		}

		private void search_TextChanged(object sender, System.EventArgs e)
		{
			if (search.Focused && !noSearch)
			{
				stopThread = true;

				// Reset the timer when search text is entered.
				searchTimer.Stop();
				searchTimer.Start();
			}
		}

		private void attributeName_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (attributeName.Focused)
			{
				if (!search.Text.Equals(resourceManager.GetString("searchPrompt")))
				{
					stopThread = true;

					searchTimer.Stop();
					searchTimer.Start();
				}
			}
		}

		private void rosterLV_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// Enable/disable the add button based on the state of the selected item(s).
			if (rosterLV.SelectedItems.Count > 0)
			{
				ListViewItem lvi = rosterLV.SelectedItems[0];
				add.Enabled = !lvi.ForeColor.Equals(Color.Gray);
			}
			else
			{
				add.Enabled = false;
			}
		}

		private void addedLV_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// Enable the remove button if one or more added item is selected.
			remove.Enabled = addedLV.SelectedItems.Count > 0;

			if (addedLV.SelectedItems.Count == 1)
			{
				iFolderUser selectedUser = GetiFolderUserFromListViewItem(addedLV.SelectedItems[0]);

				try
				{
					// Disable the remove button if only one user is selected, and it's the current user or current owner.
					if ((selectedUser != null) && (selectedUser.UserID.Equals(currentUser.UserID) || selectedUser.UserID.Equals(currentOwner.UserID)))
					{
						remove.Enabled = false;
					}
				}
				catch
				{
					remove.Enabled = false;
				}
			}
		}

		private void addedLV_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			switch (addedLV.Sorting)
			{
				case SortOrder.None:
				{
					addedLV.Sorting = SortOrder.Ascending;
					break;
				}
				case SortOrder.Ascending:
				{
					addedLV.Sorting = SortOrder.Descending;
					break;
				}
				case SortOrder.Descending:
				{
					addedLV.Sorting = SortOrder.Ascending;
					break;
				}
			}
		}

		private void Picker_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if ((worker != null) && worker.IsAlive)
			{
				worker.Abort();
			}

			if (searchContext != null)
			{
				try
				{
					ifWebService.FindCloseiFolderMembers(domainID, searchContext);
					searchContext = null;
				}
				catch {}
			}
		}

		private void search_Enter(object sender, System.EventArgs e)
		{
			if (search.Text.Equals(resourceManager.GetString("searchPrompt")))
			{
				search.Clear();
				noSearch = false;
			}
		}
		#endregion

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated (e);

			if (worker == null)
			{
				worker = new Thread(new ThreadStart(searchThreadProc));
				worker.Start();
			}
		}
	}
}
