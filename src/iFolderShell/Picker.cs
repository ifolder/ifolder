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

namespace Novell.iFolderCom
{
	/// <summary>
	/// Summary description for Picker.
	/// </summary>
	[ComVisible(false)]
	public class Picker : System.Windows.Forms.Form
	{
		#region Class Members
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
		private bool first = true;
		private Hashtable ht = null;
		private Hashtable addedHT = null;
		private int fixedWidth;
		private int addOffset;
		private int addedLVOffset;
		private int dividerOffset;
		private double rosterLVRatio;
		private string loadPath;
		private iFolderWebService ifWebService;
		private System.Windows.Forms.ListView rosterLV;
		private System.Windows.Forms.ListView addedLV;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Timer searchTimer;
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

			// Calculate the distance between the add button and the right side of the roster listview.
			addOffset = add.Left - rosterLV.Right;

			// Calculate the distance between the right side of the add button and the added listview.
			addedLVOffset = addedLV.Left - add.Right;

			// Calculate the size of the fixed area of the dialog ...
			fixedWidth = 
				rosterLV.Left +
				addOffset +
				add.Size.Width +
                addedLVOffset +
				this.Right - addedLV.Right;

			// Calculate the size ratio of the roster listview.
			rosterLVRatio = (double)rosterLV.Size.Width / (double)(this.Right - fixedWidth);

			// Calculate the offset of the divider.
			dividerOffset = ok.Top - groupBox1.Top;

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
			this.searchTimer.Interval = 500;
			this.searchTimer.Tick += new System.EventHandler(this.searchTimer_Tick);
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
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.search);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.remove);
			this.Controls.Add(this.add);
			this.Controls.Add(this.addedLV);
			this.Controls.Add(this.rosterLV);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "Picker";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.SizeChanged += new System.EventHandler(this.Picker_SizeChanged);
			this.Load += new System.EventHandler(this.Picker_Load);
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
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
		private void displayUsers(string search)
		{
			Cursor.Current = Cursors.WaitCursor;
			rosterLV.Items.Clear();
			rosterLV.BeginUpdate();

			try
			{
				iFolderUser[] ifolderUsers;
				
				if (search != null)
				{
					ifolderUsers = ifWebService.SearchForiFolderUsers(search);
				}
				else
				{
					ifolderUsers = ifWebService.GetAlliFolderUsers();
				}

				foreach (iFolderUser ifolderUser in ifolderUsers)
				{
					int imageIndex = ifolderUser.UserID.Equals(currentUser.UserID) ? 0 : 1;
					ListViewItem lvi = new ListViewItem(ifolderUser.Name, imageIndex);
					lvi.Tag = ifolderUser;
					rosterLV.Items.Add(lvi);

					if (ht != null)
					{
						ListViewItem item = (ListViewItem)ht[ifolderUser.UserID];
						if (item != null)
						{
							// Use the existing iFolderUser
							lvi.Tag = ((ShareListMember)item.Tag).iFolderUser;
						}

						if (first && (item != null))
						{
							// Only create added items on first pass.
							item = new ListViewItem(lvi.Text, lvi.ImageIndex);
							addedLV.Items.Add(item);
							item.Tag = lvi;
							addedHT.Add(ifolderUser.UserID, item);
							lvi.ForeColor = Color.Gray;
						}
						else
						{
							// Find and update items in the added list.
							item = (ListViewItem)addedHT[ifolderUser.UserID];
							if (item != null)
							{
								item.Tag = lvi;
								addedHT[ifolderUser.UserID] = item;
								lvi.ForeColor = Color.Gray;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(Picker));
				MyMessageBox mmb = new MyMessageBox();
				mmb.Message = resourceManager.GetString("memberReadError");
				mmb.Details = ex.Message;
				mmb.ShowDialog();
			}

			first = false;
			rosterLV.EndUpdate();
			Cursor.Current = Cursors.Default;
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

			// Put the objects in the listview.
			displayUsers(null);
		}

		private void Picker_SizeChanged(object sender, System.EventArgs e)
		{
			// Relocate the divider line.
			groupBox1.Top = ok.Top - dividerOffset;

			// Resize the listviews.
			int x = this.Width - fixedWidth;

			// Calculate the new widths based on the initial ratios.
			rosterLV.Width = (int)(x * rosterLVRatio);
			addedLV.Width = (int)(x * (1 - rosterLVRatio));

			// Relocate the add button.
			add.Left = rosterLV.Right + addOffset;

			// Relocate the remove button.
			remove.Left = add.Left;

			// Relocate the added listview.
			addedLV.Left = add.Right + addedLVOffset;

			// Resize the search box.
			search.Width = rosterLV.Right - search.Left;
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
				iFolderUser selectedUser = (iFolderUser)((ListViewItem)lvi.Tag).Tag;

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
					((ListViewItem)lvi.Tag).ForeColor = Color.Black;

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
			displayUsers(search.Text);
		}

		private void search_TextChanged(object sender, System.EventArgs e)
		{
			// Reset the timer when search text is entered.
			searchTimer.Stop();
			searchTimer.Start();
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
				// Disable the remove button if only one user is selected, and it's the current user or current owner.
				iFolderUser selectedUser = (iFolderUser)((ListViewItem)addedLV.SelectedItems[0].Tag).Tag;
				if (selectedUser.UserID.Equals(currentUser.UserID) || selectedUser.UserID.Equals(currentOwner.UserID))
				{
					remove.Enabled = false;
				}
			}
		}
		#endregion
	}
}
