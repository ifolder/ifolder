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

using Novell.iFolderCom;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for ServerDetails.
	/// </summary>
	public class ServerDetails : System.Windows.Forms.Form
	{
		private const double megaByte = 1048576;
		private System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(ServerDetails));
		private iFolderWebService ifWebService;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label label17;
		private Novell.iFolderCom.GaugeChart gaugeChart1;
		private System.Windows.Forms.Label totalSpaceUnits;
		private System.Windows.Forms.Label usedSpaceUnits;
		private System.Windows.Forms.Label freeSpaceUnits;
		private System.Windows.Forms.Label totalSpace;
		private System.Windows.Forms.Label usedSpace;
		private System.Windows.Forms.Label freeSpace;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox enterpriseDescription;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox servers;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Constructs a ServerDetails object.
		/// </summary>
		/// <param name="ifolderWebService">The iFolderWebService object to use.</param>
		/// <param name="servers">A collection of servers to put in the dropdown list.</param>
		/// <param name="selectedDomain">The server to select in the dropdown list.</param>
		public ServerDetails(iFolderWebService ifolderWebService, ListView.ListViewItemCollection servers, Domain selectedDomain)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			ifWebService = ifolderWebService;

			foreach (ListViewItem lvi in servers)
			{
				if (lvi.Tag != null)
				{
					this.servers.Items.Add((Domain)lvi.Tag);
				}
			}

			this.servers.SelectedItem = selectedDomain;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ServerDetails));
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
			this.enterpriseDescription = new System.Windows.Forms.TextBox();
			this.ok = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.servers = new System.Windows.Forms.ComboBox();
			this.groupBox6.SuspendLayout();
			this.SuspendLayout();
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
			// servers
			// 
			this.servers.AccessibleDescription = resources.GetString("servers.AccessibleDescription");
			this.servers.AccessibleName = resources.GetString("servers.AccessibleName");
			this.servers.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("servers.Anchor")));
			this.servers.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("servers.BackgroundImage")));
			this.servers.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("servers.Dock")));
			this.servers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.servers.Enabled = ((bool)(resources.GetObject("servers.Enabled")));
			this.servers.Font = ((System.Drawing.Font)(resources.GetObject("servers.Font")));
			this.servers.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("servers.ImeMode")));
			this.servers.IntegralHeight = ((bool)(resources.GetObject("servers.IntegralHeight")));
			this.servers.ItemHeight = ((int)(resources.GetObject("servers.ItemHeight")));
			this.servers.Location = ((System.Drawing.Point)(resources.GetObject("servers.Location")));
			this.servers.MaxDropDownItems = ((int)(resources.GetObject("servers.MaxDropDownItems")));
			this.servers.MaxLength = ((int)(resources.GetObject("servers.MaxLength")));
			this.servers.Name = "servers";
			this.servers.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("servers.RightToLeft")));
			this.servers.Size = ((System.Drawing.Size)(resources.GetObject("servers.Size")));
			this.servers.TabIndex = ((int)(resources.GetObject("servers.TabIndex")));
			this.servers.Text = resources.GetString("servers.Text");
			this.servers.Visible = ((bool)(resources.GetObject("servers.Visible")));
			this.servers.SelectedIndexChanged += new System.EventHandler(this.servers_SelectedIndexChanged);
			// 
			// ServerDetails
			// 
			this.AcceptButton = this.ok;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.enterpriseDescription);
			this.Controls.Add(this.servers);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.groupBox6);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "ServerDetails";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.ServerDetails_Load);
			this.groupBox6.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers
		private void ServerDetails_Load(object sender, System.EventArgs e)
		{
			// Load the icon.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_loaded.ico"));
			}
			catch {} // Non-fatal ...
		}

		private void servers_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			Domain selectedDomain = (Domain)servers.SelectedItem;
			enterpriseDescription.Text = selectedDomain.DomainWeb.Description;

			try
			{
				// Get the disk space.
				DiskSpace diskSpace = ifWebService.GetUserDiskSpace(selectedDomain.DomainWeb.UserID);
				double used = 0;
				if (diskSpace.UsedSpace != 0)
				{
					usedSpaceUnits.Text = resourceManager.GetString("freeSpaceUnits.Text");
					used = Math.Round(diskSpace.UsedSpace/megaByte, 2);
					usedSpace.Text = used.ToString();
				}
				else
				{
					usedSpaceUnits.Text = resourceManager.GetString("notApplicable");
					usedSpace.Text = "";
				}

				if (diskSpace.Limit != 0)
				{
					usedSpaceUnits.Text = freeSpaceUnits.Text = totalSpaceUnits.Text = 
						resourceManager.GetString("freeSpaceUnits.Text");
					totalSpace.Text = ((double)Math.Round(diskSpace.Limit/megaByte, 2)).ToString();

					freeSpace.Text = ((double)Math.Round(diskSpace.AvailableSpace/megaByte, 2)).ToString();

					// Set up the gauge chart.
					gaugeChart1.MaxValue = diskSpace.Limit / megaByte;
					gaugeChart1.Used = used;
					gaugeChart1.BarColor = SystemColors.ActiveCaption;
				}
				else
				{
					freeSpaceUnits.Text = totalSpaceUnits.Text =
						resourceManager.GetString("notApplicable");
					freeSpace.Text = totalSpace.Text = "";
					gaugeChart1.Used = 0;
				}
			}
			catch
			{
				usedSpace.Text = freeSpace.Text = totalSpace.Text = resourceManager.GetString("statusUnknown");
			}

			// Cause the gauge chart to be redrawn.
			gaugeChart1.Invalidate(true);
		}
		#endregion
	}
}
