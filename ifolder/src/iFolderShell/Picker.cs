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

namespace Novell.iFolderCom
{
	/// <summary>
	/// Summary description for Picker.
	/// </summary>
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
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
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
			this.SuspendLayout();
			// 
			// rosterLV
			// 
			this.rosterLV.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left)));
			this.rosterLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					   this.columnHeader1});
			this.rosterLV.Location = new System.Drawing.Point(8, 48);
			this.rosterLV.Name = "rosterLV";
			this.rosterLV.Size = new System.Drawing.Size(200, 288);
			this.rosterLV.TabIndex = 0;
			this.rosterLV.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 196;
			// 
			// addedLV
			// 
			this.addedLV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.addedLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					  this.columnHeader2});
			this.addedLV.Location = new System.Drawing.Point(295, 48);
			this.addedLV.Name = "addedLV";
			this.addedLV.Size = new System.Drawing.Size(200, 288);
			this.addedLV.TabIndex = 1;
			this.addedLV.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Name";
			this.columnHeader2.Width = 196;
			// 
			// add
			// 
			this.add.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.add.Location = new System.Drawing.Point(214, 64);
			this.add.Name = "add";
			this.add.TabIndex = 2;
			this.add.Text = "Add ->";
			this.add.Click += new System.EventHandler(this.add_Click);
			// 
			// remove
			// 
			this.remove.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.remove.Location = new System.Drawing.Point(214, 96);
			this.remove.Name = "remove";
			this.remove.TabIndex = 3;
			this.remove.Text = "Remove";
			this.remove.Click += new System.EventHandler(this.remove_Click);
			// 
			// search
			// 
			this.search.Location = new System.Drawing.Point(56, 16);
			this.search.Name = "search";
			this.search.Size = new System.Drawing.Size(152, 20);
			this.search.TabIndex = 4;
			this.search.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 18);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 5;
			this.label1.Text = "Search:";
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Location = new System.Drawing.Point(8, 344);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(488, 4);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			// 
			// ok
			// 
			this.ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ok.Location = new System.Drawing.Point(336, 360);
			this.ok.Name = "ok";
			this.ok.TabIndex = 7;
			this.ok.Text = "OK";
			// 
			// cancel
			// 
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancel.Location = new System.Drawing.Point(420, 360);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 8;
			this.cancel.Text = "Cancel";
			// 
			// Picker
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(504, 390);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.search);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.remove);
			this.Controls.Add(this.add);
			this.Controls.Add(this.addedLV);
			this.Controls.Add(this.rosterLV);
			this.MinimumSize = new System.Drawing.Size(512, 424);
			this.Name = "Picker";
			this.Text = "Picker";
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

		public ListView.ListViewItemCollection AddedUsers
		{
			get
			{
				return addedLV.Items;
			}
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
			try
			{
				iFolderUser[] ifolderUsers = ifWebService.GetAlliFolderUsers();

				foreach (iFolderUser ifolderUser in ifolderUsers)
				{
					ListViewItem lvi = new ListViewItem(ifolderUser.Name, 1);
					lvi.Tag = ifolderUser;
					rosterLV.Items.Add(lvi);
				}
			}
			catch (WebException ex)
			{
			}
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
					ListViewItem item = new ListViewItem(lvi.Text, lvi.ImageIndex);
					item.Tag = lvi;
					addedLV.Items.Add(item);
					lvi.ForeColor = Color.Gray;
				}
			}
		}

		private void remove_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem lvi in addedLV.SelectedItems)
			{
				((ListViewItem)lvi.Tag).ForeColor = Color.Black;
				lvi.Remove();
			}
		}
		#endregion
	}
}
