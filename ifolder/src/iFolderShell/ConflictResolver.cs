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
	/// Summary description for ConflictResolver.
	/// </summary>
	[ComVisible(false)]
	public class ConflictResolver : System.Windows.Forms.Form
	{
		#region Class Members
		System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(ConflictResolver));
		private iFolderWebService ifWebService;
		private iFolderWeb ifolder;
		private string loadPath;
		private bool initial = true;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label ifolderName;
		private System.Windows.Forms.Label ifolderPath;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label localName;
		private System.Windows.Forms.Label localDate;
		private System.Windows.Forms.Label localSize;
		private System.Windows.Forms.Button saveLocal;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button saveServer;
		private System.Windows.Forms.Label serverSize;
		private System.Windows.Forms.Label serverDate;
		private System.Windows.Forms.Label serverName;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Button close;
		private System.Windows.Forms.Button help;
		private System.Windows.Forms.ListView conflictsView;
		#endregion
		private System.Windows.Forms.ToolTip toolTip1;
		private System.ComponentModel.IContainer components;

		/// <summary>
		/// Instantiates a ConflictResolver object.
		/// </summary>
		public ConflictResolver()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ConflictResolver));
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.ifolderName = new System.Windows.Forms.Label();
			this.ifolderPath = new System.Windows.Forms.Label();
			this.conflictsView = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.saveLocal = new System.Windows.Forms.Button();
			this.localSize = new System.Windows.Forms.Label();
			this.localDate = new System.Windows.Forms.Label();
			this.localName = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.saveServer = new System.Windows.Forms.Button();
			this.serverSize = new System.Windows.Forms.Label();
			this.serverDate = new System.Windows.Forms.Label();
			this.serverName = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.close = new System.Windows.Forms.Button();
			this.help = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
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
			this.toolTip1.SetToolTip(this.label1, resources.GetString("label1.ToolTip"));
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
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
			this.toolTip1.SetToolTip(this.label2, resources.GetString("label2.ToolTip"));
			this.label2.Visible = ((bool)(resources.GetObject("label2.Visible")));
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
			this.toolTip1.SetToolTip(this.label3, resources.GetString("label3.ToolTip"));
			this.label3.Visible = ((bool)(resources.GetObject("label3.Visible")));
			// 
			// ifolderName
			// 
			this.ifolderName.AccessibleDescription = resources.GetString("ifolderName.AccessibleDescription");
			this.ifolderName.AccessibleName = resources.GetString("ifolderName.AccessibleName");
			this.ifolderName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ifolderName.Anchor")));
			this.ifolderName.AutoSize = ((bool)(resources.GetObject("ifolderName.AutoSize")));
			this.ifolderName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ifolderName.Dock")));
			this.ifolderName.Enabled = ((bool)(resources.GetObject("ifolderName.Enabled")));
			this.ifolderName.Font = ((System.Drawing.Font)(resources.GetObject("ifolderName.Font")));
			this.ifolderName.Image = ((System.Drawing.Image)(resources.GetObject("ifolderName.Image")));
			this.ifolderName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ifolderName.ImageAlign")));
			this.ifolderName.ImageIndex = ((int)(resources.GetObject("ifolderName.ImageIndex")));
			this.ifolderName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ifolderName.ImeMode")));
			this.ifolderName.Location = ((System.Drawing.Point)(resources.GetObject("ifolderName.Location")));
			this.ifolderName.Name = "ifolderName";
			this.ifolderName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ifolderName.RightToLeft")));
			this.ifolderName.Size = ((System.Drawing.Size)(resources.GetObject("ifolderName.Size")));
			this.ifolderName.TabIndex = ((int)(resources.GetObject("ifolderName.TabIndex")));
			this.ifolderName.Text = resources.GetString("ifolderName.Text");
			this.ifolderName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ifolderName.TextAlign")));
			this.toolTip1.SetToolTip(this.ifolderName, resources.GetString("ifolderName.ToolTip"));
			this.ifolderName.Visible = ((bool)(resources.GetObject("ifolderName.Visible")));
			// 
			// ifolderPath
			// 
			this.ifolderPath.AccessibleDescription = resources.GetString("ifolderPath.AccessibleDescription");
			this.ifolderPath.AccessibleName = resources.GetString("ifolderPath.AccessibleName");
			this.ifolderPath.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ifolderPath.Anchor")));
			this.ifolderPath.AutoSize = ((bool)(resources.GetObject("ifolderPath.AutoSize")));
			this.ifolderPath.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ifolderPath.Dock")));
			this.ifolderPath.Enabled = ((bool)(resources.GetObject("ifolderPath.Enabled")));
			this.ifolderPath.Font = ((System.Drawing.Font)(resources.GetObject("ifolderPath.Font")));
			this.ifolderPath.Image = ((System.Drawing.Image)(resources.GetObject("ifolderPath.Image")));
			this.ifolderPath.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ifolderPath.ImageAlign")));
			this.ifolderPath.ImageIndex = ((int)(resources.GetObject("ifolderPath.ImageIndex")));
			this.ifolderPath.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ifolderPath.ImeMode")));
			this.ifolderPath.Location = ((System.Drawing.Point)(resources.GetObject("ifolderPath.Location")));
			this.ifolderPath.Name = "ifolderPath";
			this.ifolderPath.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ifolderPath.RightToLeft")));
			this.ifolderPath.Size = ((System.Drawing.Size)(resources.GetObject("ifolderPath.Size")));
			this.ifolderPath.TabIndex = ((int)(resources.GetObject("ifolderPath.TabIndex")));
			this.ifolderPath.Text = resources.GetString("ifolderPath.Text");
			this.ifolderPath.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ifolderPath.TextAlign")));
			this.toolTip1.SetToolTip(this.ifolderPath, resources.GetString("ifolderPath.ToolTip"));
			this.ifolderPath.Visible = ((bool)(resources.GetObject("ifolderPath.Visible")));
			this.ifolderPath.Paint += new System.Windows.Forms.PaintEventHandler(this.ifolderPath_Paint);
			// 
			// conflictsView
			// 
			this.conflictsView.AccessibleDescription = resources.GetString("conflictsView.AccessibleDescription");
			this.conflictsView.AccessibleName = resources.GetString("conflictsView.AccessibleName");
			this.conflictsView.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("conflictsView.Alignment")));
			this.conflictsView.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("conflictsView.Anchor")));
			this.conflictsView.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("conflictsView.BackgroundImage")));
			this.conflictsView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							this.columnHeader1});
			this.conflictsView.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("conflictsView.Dock")));
			this.conflictsView.Enabled = ((bool)(resources.GetObject("conflictsView.Enabled")));
			this.conflictsView.Font = ((System.Drawing.Font)(resources.GetObject("conflictsView.Font")));
			this.conflictsView.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("conflictsView.ImeMode")));
			this.conflictsView.LabelWrap = ((bool)(resources.GetObject("conflictsView.LabelWrap")));
			this.conflictsView.Location = ((System.Drawing.Point)(resources.GetObject("conflictsView.Location")));
			this.conflictsView.Name = "conflictsView";
			this.conflictsView.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("conflictsView.RightToLeft")));
			this.conflictsView.Size = ((System.Drawing.Size)(resources.GetObject("conflictsView.Size")));
			this.conflictsView.TabIndex = ((int)(resources.GetObject("conflictsView.TabIndex")));
			this.conflictsView.Text = resources.GetString("conflictsView.Text");
			this.toolTip1.SetToolTip(this.conflictsView, resources.GetString("conflictsView.ToolTip"));
			this.conflictsView.View = System.Windows.Forms.View.Details;
			this.conflictsView.Visible = ((bool)(resources.GetObject("conflictsView.Visible")));
			this.conflictsView.SelectedIndexChanged += new System.EventHandler(this.conflictsView_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = resources.GetString("columnHeader1.Text");
			this.columnHeader1.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader1.TextAlign")));
			this.columnHeader1.Width = ((int)(resources.GetObject("columnHeader1.Width")));
			// 
			// groupBox1
			// 
			this.groupBox1.AccessibleDescription = resources.GetString("groupBox1.AccessibleDescription");
			this.groupBox1.AccessibleName = resources.GetString("groupBox1.AccessibleName");
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox1.Anchor")));
			this.groupBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox1.BackgroundImage")));
			this.groupBox1.Controls.Add(this.saveLocal);
			this.groupBox1.Controls.Add(this.localSize);
			this.groupBox1.Controls.Add(this.localDate);
			this.groupBox1.Controls.Add(this.localName);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.label4);
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
			this.toolTip1.SetToolTip(this.groupBox1, resources.GetString("groupBox1.ToolTip"));
			this.groupBox1.Visible = ((bool)(resources.GetObject("groupBox1.Visible")));
			// 
			// saveLocal
			// 
			this.saveLocal.AccessibleDescription = resources.GetString("saveLocal.AccessibleDescription");
			this.saveLocal.AccessibleName = resources.GetString("saveLocal.AccessibleName");
			this.saveLocal.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("saveLocal.Anchor")));
			this.saveLocal.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("saveLocal.BackgroundImage")));
			this.saveLocal.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("saveLocal.Dock")));
			this.saveLocal.Enabled = ((bool)(resources.GetObject("saveLocal.Enabled")));
			this.saveLocal.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("saveLocal.FlatStyle")));
			this.saveLocal.Font = ((System.Drawing.Font)(resources.GetObject("saveLocal.Font")));
			this.saveLocal.Image = ((System.Drawing.Image)(resources.GetObject("saveLocal.Image")));
			this.saveLocal.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("saveLocal.ImageAlign")));
			this.saveLocal.ImageIndex = ((int)(resources.GetObject("saveLocal.ImageIndex")));
			this.saveLocal.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("saveLocal.ImeMode")));
			this.saveLocal.Location = ((System.Drawing.Point)(resources.GetObject("saveLocal.Location")));
			this.saveLocal.Name = "saveLocal";
			this.saveLocal.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("saveLocal.RightToLeft")));
			this.saveLocal.Size = ((System.Drawing.Size)(resources.GetObject("saveLocal.Size")));
			this.saveLocal.TabIndex = ((int)(resources.GetObject("saveLocal.TabIndex")));
			this.saveLocal.Text = resources.GetString("saveLocal.Text");
			this.saveLocal.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("saveLocal.TextAlign")));
			this.toolTip1.SetToolTip(this.saveLocal, resources.GetString("saveLocal.ToolTip"));
			this.saveLocal.Visible = ((bool)(resources.GetObject("saveLocal.Visible")));
			this.saveLocal.Click += new System.EventHandler(this.saveLocal_Click);
			// 
			// localSize
			// 
			this.localSize.AccessibleDescription = resources.GetString("localSize.AccessibleDescription");
			this.localSize.AccessibleName = resources.GetString("localSize.AccessibleName");
			this.localSize.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("localSize.Anchor")));
			this.localSize.AutoSize = ((bool)(resources.GetObject("localSize.AutoSize")));
			this.localSize.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("localSize.Dock")));
			this.localSize.Enabled = ((bool)(resources.GetObject("localSize.Enabled")));
			this.localSize.Font = ((System.Drawing.Font)(resources.GetObject("localSize.Font")));
			this.localSize.Image = ((System.Drawing.Image)(resources.GetObject("localSize.Image")));
			this.localSize.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("localSize.ImageAlign")));
			this.localSize.ImageIndex = ((int)(resources.GetObject("localSize.ImageIndex")));
			this.localSize.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("localSize.ImeMode")));
			this.localSize.Location = ((System.Drawing.Point)(resources.GetObject("localSize.Location")));
			this.localSize.Name = "localSize";
			this.localSize.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("localSize.RightToLeft")));
			this.localSize.Size = ((System.Drawing.Size)(resources.GetObject("localSize.Size")));
			this.localSize.TabIndex = ((int)(resources.GetObject("localSize.TabIndex")));
			this.localSize.Text = resources.GetString("localSize.Text");
			this.localSize.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("localSize.TextAlign")));
			this.toolTip1.SetToolTip(this.localSize, resources.GetString("localSize.ToolTip"));
			this.localSize.Visible = ((bool)(resources.GetObject("localSize.Visible")));
			// 
			// localDate
			// 
			this.localDate.AccessibleDescription = resources.GetString("localDate.AccessibleDescription");
			this.localDate.AccessibleName = resources.GetString("localDate.AccessibleName");
			this.localDate.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("localDate.Anchor")));
			this.localDate.AutoSize = ((bool)(resources.GetObject("localDate.AutoSize")));
			this.localDate.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("localDate.Dock")));
			this.localDate.Enabled = ((bool)(resources.GetObject("localDate.Enabled")));
			this.localDate.Font = ((System.Drawing.Font)(resources.GetObject("localDate.Font")));
			this.localDate.Image = ((System.Drawing.Image)(resources.GetObject("localDate.Image")));
			this.localDate.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("localDate.ImageAlign")));
			this.localDate.ImageIndex = ((int)(resources.GetObject("localDate.ImageIndex")));
			this.localDate.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("localDate.ImeMode")));
			this.localDate.Location = ((System.Drawing.Point)(resources.GetObject("localDate.Location")));
			this.localDate.Name = "localDate";
			this.localDate.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("localDate.RightToLeft")));
			this.localDate.Size = ((System.Drawing.Size)(resources.GetObject("localDate.Size")));
			this.localDate.TabIndex = ((int)(resources.GetObject("localDate.TabIndex")));
			this.localDate.Text = resources.GetString("localDate.Text");
			this.localDate.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("localDate.TextAlign")));
			this.toolTip1.SetToolTip(this.localDate, resources.GetString("localDate.ToolTip"));
			this.localDate.Visible = ((bool)(resources.GetObject("localDate.Visible")));
			// 
			// localName
			// 
			this.localName.AccessibleDescription = resources.GetString("localName.AccessibleDescription");
			this.localName.AccessibleName = resources.GetString("localName.AccessibleName");
			this.localName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("localName.Anchor")));
			this.localName.AutoSize = ((bool)(resources.GetObject("localName.AutoSize")));
			this.localName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("localName.Dock")));
			this.localName.Enabled = ((bool)(resources.GetObject("localName.Enabled")));
			this.localName.Font = ((System.Drawing.Font)(resources.GetObject("localName.Font")));
			this.localName.Image = ((System.Drawing.Image)(resources.GetObject("localName.Image")));
			this.localName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("localName.ImageAlign")));
			this.localName.ImageIndex = ((int)(resources.GetObject("localName.ImageIndex")));
			this.localName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("localName.ImeMode")));
			this.localName.Location = ((System.Drawing.Point)(resources.GetObject("localName.Location")));
			this.localName.Name = "localName";
			this.localName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("localName.RightToLeft")));
			this.localName.Size = ((System.Drawing.Size)(resources.GetObject("localName.Size")));
			this.localName.TabIndex = ((int)(resources.GetObject("localName.TabIndex")));
			this.localName.Text = resources.GetString("localName.Text");
			this.localName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("localName.TextAlign")));
			this.toolTip1.SetToolTip(this.localName, resources.GetString("localName.ToolTip"));
			this.localName.Visible = ((bool)(resources.GetObject("localName.Visible")));
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
			this.toolTip1.SetToolTip(this.label6, resources.GetString("label6.ToolTip"));
			this.label6.Visible = ((bool)(resources.GetObject("label6.Visible")));
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
			this.toolTip1.SetToolTip(this.label5, resources.GetString("label5.ToolTip"));
			this.label5.Visible = ((bool)(resources.GetObject("label5.Visible")));
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
			this.toolTip1.SetToolTip(this.label4, resources.GetString("label4.ToolTip"));
			this.label4.Visible = ((bool)(resources.GetObject("label4.Visible")));
			// 
			// groupBox2
			// 
			this.groupBox2.AccessibleDescription = resources.GetString("groupBox2.AccessibleDescription");
			this.groupBox2.AccessibleName = resources.GetString("groupBox2.AccessibleName");
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox2.Anchor")));
			this.groupBox2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox2.BackgroundImage")));
			this.groupBox2.Controls.Add(this.saveServer);
			this.groupBox2.Controls.Add(this.serverSize);
			this.groupBox2.Controls.Add(this.serverDate);
			this.groupBox2.Controls.Add(this.serverName);
			this.groupBox2.Controls.Add(this.label10);
			this.groupBox2.Controls.Add(this.label11);
			this.groupBox2.Controls.Add(this.label12);
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
			this.toolTip1.SetToolTip(this.groupBox2, resources.GetString("groupBox2.ToolTip"));
			this.groupBox2.Visible = ((bool)(resources.GetObject("groupBox2.Visible")));
			// 
			// saveServer
			// 
			this.saveServer.AccessibleDescription = resources.GetString("saveServer.AccessibleDescription");
			this.saveServer.AccessibleName = resources.GetString("saveServer.AccessibleName");
			this.saveServer.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("saveServer.Anchor")));
			this.saveServer.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("saveServer.BackgroundImage")));
			this.saveServer.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("saveServer.Dock")));
			this.saveServer.Enabled = ((bool)(resources.GetObject("saveServer.Enabled")));
			this.saveServer.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("saveServer.FlatStyle")));
			this.saveServer.Font = ((System.Drawing.Font)(resources.GetObject("saveServer.Font")));
			this.saveServer.Image = ((System.Drawing.Image)(resources.GetObject("saveServer.Image")));
			this.saveServer.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("saveServer.ImageAlign")));
			this.saveServer.ImageIndex = ((int)(resources.GetObject("saveServer.ImageIndex")));
			this.saveServer.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("saveServer.ImeMode")));
			this.saveServer.Location = ((System.Drawing.Point)(resources.GetObject("saveServer.Location")));
			this.saveServer.Name = "saveServer";
			this.saveServer.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("saveServer.RightToLeft")));
			this.saveServer.Size = ((System.Drawing.Size)(resources.GetObject("saveServer.Size")));
			this.saveServer.TabIndex = ((int)(resources.GetObject("saveServer.TabIndex")));
			this.saveServer.Text = resources.GetString("saveServer.Text");
			this.saveServer.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("saveServer.TextAlign")));
			this.toolTip1.SetToolTip(this.saveServer, resources.GetString("saveServer.ToolTip"));
			this.saveServer.Visible = ((bool)(resources.GetObject("saveServer.Visible")));
			this.saveServer.Click += new System.EventHandler(this.saveServer_Click);
			// 
			// serverSize
			// 
			this.serverSize.AccessibleDescription = resources.GetString("serverSize.AccessibleDescription");
			this.serverSize.AccessibleName = resources.GetString("serverSize.AccessibleName");
			this.serverSize.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("serverSize.Anchor")));
			this.serverSize.AutoSize = ((bool)(resources.GetObject("serverSize.AutoSize")));
			this.serverSize.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("serverSize.Dock")));
			this.serverSize.Enabled = ((bool)(resources.GetObject("serverSize.Enabled")));
			this.serverSize.Font = ((System.Drawing.Font)(resources.GetObject("serverSize.Font")));
			this.serverSize.Image = ((System.Drawing.Image)(resources.GetObject("serverSize.Image")));
			this.serverSize.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("serverSize.ImageAlign")));
			this.serverSize.ImageIndex = ((int)(resources.GetObject("serverSize.ImageIndex")));
			this.serverSize.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("serverSize.ImeMode")));
			this.serverSize.Location = ((System.Drawing.Point)(resources.GetObject("serverSize.Location")));
			this.serverSize.Name = "serverSize";
			this.serverSize.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("serverSize.RightToLeft")));
			this.serverSize.Size = ((System.Drawing.Size)(resources.GetObject("serverSize.Size")));
			this.serverSize.TabIndex = ((int)(resources.GetObject("serverSize.TabIndex")));
			this.serverSize.Text = resources.GetString("serverSize.Text");
			this.serverSize.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("serverSize.TextAlign")));
			this.toolTip1.SetToolTip(this.serverSize, resources.GetString("serverSize.ToolTip"));
			this.serverSize.Visible = ((bool)(resources.GetObject("serverSize.Visible")));
			// 
			// serverDate
			// 
			this.serverDate.AccessibleDescription = resources.GetString("serverDate.AccessibleDescription");
			this.serverDate.AccessibleName = resources.GetString("serverDate.AccessibleName");
			this.serverDate.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("serverDate.Anchor")));
			this.serverDate.AutoSize = ((bool)(resources.GetObject("serverDate.AutoSize")));
			this.serverDate.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("serverDate.Dock")));
			this.serverDate.Enabled = ((bool)(resources.GetObject("serverDate.Enabled")));
			this.serverDate.Font = ((System.Drawing.Font)(resources.GetObject("serverDate.Font")));
			this.serverDate.Image = ((System.Drawing.Image)(resources.GetObject("serverDate.Image")));
			this.serverDate.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("serverDate.ImageAlign")));
			this.serverDate.ImageIndex = ((int)(resources.GetObject("serverDate.ImageIndex")));
			this.serverDate.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("serverDate.ImeMode")));
			this.serverDate.Location = ((System.Drawing.Point)(resources.GetObject("serverDate.Location")));
			this.serverDate.Name = "serverDate";
			this.serverDate.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("serverDate.RightToLeft")));
			this.serverDate.Size = ((System.Drawing.Size)(resources.GetObject("serverDate.Size")));
			this.serverDate.TabIndex = ((int)(resources.GetObject("serverDate.TabIndex")));
			this.serverDate.Text = resources.GetString("serverDate.Text");
			this.serverDate.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("serverDate.TextAlign")));
			this.toolTip1.SetToolTip(this.serverDate, resources.GetString("serverDate.ToolTip"));
			this.serverDate.Visible = ((bool)(resources.GetObject("serverDate.Visible")));
			// 
			// serverName
			// 
			this.serverName.AccessibleDescription = resources.GetString("serverName.AccessibleDescription");
			this.serverName.AccessibleName = resources.GetString("serverName.AccessibleName");
			this.serverName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("serverName.Anchor")));
			this.serverName.AutoSize = ((bool)(resources.GetObject("serverName.AutoSize")));
			this.serverName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("serverName.Dock")));
			this.serverName.Enabled = ((bool)(resources.GetObject("serverName.Enabled")));
			this.serverName.Font = ((System.Drawing.Font)(resources.GetObject("serverName.Font")));
			this.serverName.Image = ((System.Drawing.Image)(resources.GetObject("serverName.Image")));
			this.serverName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("serverName.ImageAlign")));
			this.serverName.ImageIndex = ((int)(resources.GetObject("serverName.ImageIndex")));
			this.serverName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("serverName.ImeMode")));
			this.serverName.Location = ((System.Drawing.Point)(resources.GetObject("serverName.Location")));
			this.serverName.Name = "serverName";
			this.serverName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("serverName.RightToLeft")));
			this.serverName.Size = ((System.Drawing.Size)(resources.GetObject("serverName.Size")));
			this.serverName.TabIndex = ((int)(resources.GetObject("serverName.TabIndex")));
			this.serverName.Text = resources.GetString("serverName.Text");
			this.serverName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("serverName.TextAlign")));
			this.toolTip1.SetToolTip(this.serverName, resources.GetString("serverName.ToolTip"));
			this.serverName.Visible = ((bool)(resources.GetObject("serverName.Visible")));
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
			this.toolTip1.SetToolTip(this.label10, resources.GetString("label10.ToolTip"));
			this.label10.Visible = ((bool)(resources.GetObject("label10.Visible")));
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
			this.toolTip1.SetToolTip(this.label11, resources.GetString("label11.ToolTip"));
			this.label11.Visible = ((bool)(resources.GetObject("label11.Visible")));
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
			this.toolTip1.SetToolTip(this.label12, resources.GetString("label12.ToolTip"));
			this.label12.Visible = ((bool)(resources.GetObject("label12.Visible")));
			// 
			// close
			// 
			this.close.AccessibleDescription = resources.GetString("close.AccessibleDescription");
			this.close.AccessibleName = resources.GetString("close.AccessibleName");
			this.close.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("close.Anchor")));
			this.close.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("close.BackgroundImage")));
			this.close.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("close.Dock")));
			this.close.Enabled = ((bool)(resources.GetObject("close.Enabled")));
			this.close.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("close.FlatStyle")));
			this.close.Font = ((System.Drawing.Font)(resources.GetObject("close.Font")));
			this.close.Image = ((System.Drawing.Image)(resources.GetObject("close.Image")));
			this.close.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("close.ImageAlign")));
			this.close.ImageIndex = ((int)(resources.GetObject("close.ImageIndex")));
			this.close.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("close.ImeMode")));
			this.close.Location = ((System.Drawing.Point)(resources.GetObject("close.Location")));
			this.close.Name = "close";
			this.close.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("close.RightToLeft")));
			this.close.Size = ((System.Drawing.Size)(resources.GetObject("close.Size")));
			this.close.TabIndex = ((int)(resources.GetObject("close.TabIndex")));
			this.close.Text = resources.GetString("close.Text");
			this.close.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("close.TextAlign")));
			this.toolTip1.SetToolTip(this.close, resources.GetString("close.ToolTip"));
			this.close.Visible = ((bool)(resources.GetObject("close.Visible")));
			this.close.Click += new System.EventHandler(this.close_Click);
			// 
			// help
			// 
			this.help.AccessibleDescription = resources.GetString("help.AccessibleDescription");
			this.help.AccessibleName = resources.GetString("help.AccessibleName");
			this.help.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("help.Anchor")));
			this.help.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("help.BackgroundImage")));
			this.help.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("help.Dock")));
			this.help.Enabled = ((bool)(resources.GetObject("help.Enabled")));
			this.help.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("help.FlatStyle")));
			this.help.Font = ((System.Drawing.Font)(resources.GetObject("help.Font")));
			this.help.Image = ((System.Drawing.Image)(resources.GetObject("help.Image")));
			this.help.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("help.ImageAlign")));
			this.help.ImageIndex = ((int)(resources.GetObject("help.ImageIndex")));
			this.help.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("help.ImeMode")));
			this.help.Location = ((System.Drawing.Point)(resources.GetObject("help.Location")));
			this.help.Name = "help";
			this.help.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("help.RightToLeft")));
			this.help.Size = ((System.Drawing.Size)(resources.GetObject("help.Size")));
			this.help.TabIndex = ((int)(resources.GetObject("help.TabIndex")));
			this.help.Text = resources.GetString("help.Text");
			this.help.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("help.TextAlign")));
			this.toolTip1.SetToolTip(this.help, resources.GetString("help.ToolTip"));
			this.help.Visible = ((bool)(resources.GetObject("help.Visible")));
			this.help.Click += new System.EventHandler(this.help_Click);
			// 
			// ConflictResolver
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.help);
			this.Controls.Add(this.close);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.conflictsView);
			this.Controls.Add(this.ifolderPath);
			this.Controls.Add(this.ifolderName);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "ConflictResolver";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
			this.Load += new System.EventHandler(this.ConflictResolver_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Private Methods
		private void resolveConflicts(bool localWins)
		{
			foreach (ListViewItem lvi in conflictsView.SelectedItems)
			{
				Conflict conflict = (Conflict)lvi.Tag;
				if (!conflict.IsNameConflict)
				{
					try
					{
						ifWebService.ResolveFileConflict(ifolder.ID, conflict.ConflictID, localWins);
						lvi.Remove();
					}
					catch (Exception ex)
					{
						MyMessageBox mmb = new MyMessageBox();
						mmb.Message = resourceManager.GetString("conflictResolveError");
						mmb.Details = ex.Message;
						mmb.ShowDialog();
					}
				}
				else if (!localWins)
				{
					/*try
					{
						// TODO: prompt for new name?
						ifWebService.ResolveNameConflict(ifolder.ID, conflict.ConflictID, newName);
						lvi.Remove();
					}
					catch (WebException e)
					{
						// TODO:
					}
					catch (Exception e)
					{
						// TODO:
					}*/
				}
			}

			if (conflictsView.Items.Count == 0 && ConflictsResolved != null)
			{
				// If all the conflicts have been resolved, fire the ConflictsResolved event.
				ConflictsResolved(this, new EventArgs());
			}
		}
		#endregion

		#region Properties
		/// <summary>
		/// Sets the iFolderWebService object to use.
		/// </summary>
		public iFolderWebService iFolderWebService
		{
			set { ifWebService = value; }
		}

		/// <summary>
		/// Sets the iFolder to resolve conflicts for.
		/// </summary>
		public iFolderWeb iFolder
		{
			set { ifolder = value; }
		}

		/// <summary>
		/// Sets the load path of the assembly.
		/// </summary>
		public string LoadPath
		{
			set { loadPath = value; }
		}
		#endregion

		#region Events
		/// <summary>
		/// Delegate used when conflicts have been resolved.
		/// </summary>
		public delegate void ConflictsResolvedDelegate(object sender, EventArgs e);
		/// <summary>
		/// Occurs when all conflicts have been resolved.
		/// </summary>
		public event ConflictsResolvedDelegate ConflictsResolved;
		#endregion

		#region Event Handlers
		private void ConflictResolver_Load(object sender, System.EventArgs e)
		{
			try
			{
				string basePath = loadPath != null ? Path.Combine(loadPath, "res") : Path.Combine(Application.StartupPath, "res");
				this.Icon = new Icon(Path.Combine(basePath, "ifolderconflict.ico"));
			}
			catch
			{
				// Ignore
			}

			try
			{
				// Display the iFolder info.
				ifolderName.Text = ifolder.Name;

				// Put the conflicts in the listview.
				Conflict[] conflicts = ifWebService.GetiFolderConflicts(ifolder.ID);
				foreach (Conflict conflict in conflicts)
				{
					// TODO: what name to use ... and icon.
					ListViewItem lvi = new ListViewItem(conflict.LocalName);
					lvi.Tag = conflict;
					this.conflictsView.Items.Add(lvi);
				}
			}
			catch (Exception ex)
			{
				MyMessageBox mmb = new MyMessageBox();
				mmb.Message = resourceManager.GetString("conflictReadError");
				mmb.Details = ex.Message;
				mmb.ShowDialog();
			}
		}

		private void conflictsView_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// Enable/disable the save buttons.
			saveLocal.Enabled = saveServer.Enabled = conflictsView.SelectedItems.Count > 0;

			if (conflictsView.SelectedItems.Count == 1)
			{
				Conflict conflict = (Conflict)conflictsView.SelectedItems[0].Tag;

				// Fill in the server data.
				serverName.Text = conflict.ServerName;
				serverDate.Text = conflict.ServerDate;
				serverSize.Text = conflict.ServerSize;

				// For name conflicts there is not a local version.
				if (conflict.IsNameConflict)
				{
					saveLocal.Enabled = false;
					localName.Text = localDate.Text = localSize.Text = "";
				}
				else
				{
					localName.Text = conflict.LocalName;
					localDate.Text = conflict.LocalDate;
					localSize.Text = conflict.LocalSize;
				}
			}
			else
			{
				// Clear the data fields.
				localName.Text = serverName.Text =
					localDate.Text = serverDate.Text =
					localSize.Text = serverSize.Text = "";
			}
		}

		private void saveLocal_Click(object sender, System.EventArgs e)
		{
			resolveConflicts(true);
		}

		private void saveServer_Click(object sender, System.EventArgs e)
		{
			resolveConflicts(false);
		}

		private void help_Click(object sender, System.EventArgs e)
		{
			new iFolderComponent().ShowHelp(loadPath);
		}

		private void ifolderPath_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			// Only do this one time.
			if (initial)
			{
				initial = false;

				// Measure the string.
				SizeF stringSize = e.Graphics.MeasureString(ifolder.UnManagedPath, ifolderPath.Font);
				if (stringSize.Width > ifolderPath.Width)
				{
					// Calculate the length of the string that can be displayed ... this will get us in the
					// ballpark.
					int length = (int)(ifolderPath.Width * ifolder.UnManagedPath.Length / stringSize.Width);
					string tmp = String.Empty;

					// Remove one character at a time until we fit in the box.  This should only loop 3 or 4 times at most.
					while (stringSize.Width > ifolderPath.Width)
					{
						tmp = ifolder.UnManagedPath.Substring(0, length) + "...";
						stringSize = e.Graphics.MeasureString(tmp, ifolderPath.Font);
						length -= 1;
					}

					// Set the truncated string in the display.
					ifolderPath.Text = tmp;

					// Set up a tooltip to display the complete path.
					toolTip1.SetToolTip(ifolderPath, ifolder.UnManagedPath);
				}
				else
				{
					// The path fits ... no truncation needed.
					ifolderPath.Text = ifolder.UnManagedPath;
				}
			}
		}

		private void close_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
		#endregion
	}
}
