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
using System.Xml;
using System.Runtime.InteropServices;

using Novell.Win32Util;

namespace Novell.iFolderCom
{
	/// <summary>
	/// Summary description for CreateiFolder.
	/// </summary>
	[ComVisible(false)]
	public class CreateiFolder : System.Windows.Forms.Form
	{
		#region Class Members
		enum SecurityState
		{
			encryption = 1,
			enforceEncryption = 2,
			SSL = 4,
			enforceSSL = 8
		}
		System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(CreateiFolder));
		private iFolderWebService ifWebService;
		private SimiasWebService simws;
		private bool successful;
		private DomainItem selectedDomain;
		private string loadPath;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.ComboBox servers;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button browse;
		private System.Windows.Forms.TextBox ifolderPath;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.RadioButton encryption;
		private System.Windows.Forms.RadioButton ssl;

		
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		/// <summary>
		/// Constructs a CreateiFolder object.
		/// </summary>
		public CreateiFolder()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Adjust the multi-selector controls based on the length of the strings.
			int delta = calculateSize(label1, 0);
			delta = calculateSize(label2, delta);

			label1.Width = label2.Width += delta;
			int temp = servers.Left;
			servers.Left = ifolderPath.Left = label1.Left + label1.Width;
			servers.Width = ifolderPath.Width -= servers.Left - temp;
		}

		#region Properties
		/// <summary>
		/// Sets the web service to use.
		/// </summary>
		public iFolderWebService iFolderWebService
		{
			set { ifWebService = value; }
		}

		public SimiasWebService simiasWebService
		{
			set { simws = value; }
		}

		/// <summary>
		/// Sets the collection of servers to display in the dropdown list.
		/// </summary>
		public ArrayList Servers
		{
			set
			{
				foreach (DomainItem d in value)
				{
					servers.Items.Add(d);
				}
			}
		}

		/// <summary>
		/// Sets the domain that will be initially selected in the dropdown list.
		/// </summary>
		public DomainItem SelectedDomain
		{
			set { selectedDomain = value; }
		}

		/// <summary>
		/// Sets/gets the path where the iFolder will be (or was) created.
		/// </summary>
		public string iFolderPath
		{
			get { return ifolderPath.Text; }
			set { ifolderPath.Text = value; }
		}

		/// <summary>
		/// Sets the load path of the assembly.
		/// </summary>
		public string LoadPath
		{
			set { loadPath = value; }
		}
		#endregion

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(CreateiFolder));
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.servers = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.ifolderPath = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.browse = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.encryption = new System.Windows.Forms.RadioButton();
			this.ssl = new System.Windows.Forms.RadioButton();
			this.SuspendLayout();
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
			this.ok.Click += new System.EventHandler(this.ok_Click);
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
			// ifolderPath
			// 
			this.ifolderPath.AccessibleDescription = resources.GetString("ifolderPath.AccessibleDescription");
			this.ifolderPath.AccessibleName = resources.GetString("ifolderPath.AccessibleName");
			this.ifolderPath.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ifolderPath.Anchor")));
			this.ifolderPath.AutoSize = ((bool)(resources.GetObject("ifolderPath.AutoSize")));
			this.ifolderPath.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ifolderPath.BackgroundImage")));
			this.ifolderPath.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ifolderPath.Dock")));
			this.ifolderPath.Enabled = ((bool)(resources.GetObject("ifolderPath.Enabled")));
			this.ifolderPath.Font = ((System.Drawing.Font)(resources.GetObject("ifolderPath.Font")));
			this.ifolderPath.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ifolderPath.ImeMode")));
			this.ifolderPath.Location = ((System.Drawing.Point)(resources.GetObject("ifolderPath.Location")));
			this.ifolderPath.MaxLength = ((int)(resources.GetObject("ifolderPath.MaxLength")));
			this.ifolderPath.Multiline = ((bool)(resources.GetObject("ifolderPath.Multiline")));
			this.ifolderPath.Name = "ifolderPath";
			this.ifolderPath.PasswordChar = ((char)(resources.GetObject("ifolderPath.PasswordChar")));
			this.ifolderPath.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ifolderPath.RightToLeft")));
			this.ifolderPath.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("ifolderPath.ScrollBars")));
			this.ifolderPath.Size = ((System.Drawing.Size)(resources.GetObject("ifolderPath.Size")));
			this.ifolderPath.TabIndex = ((int)(resources.GetObject("ifolderPath.TabIndex")));
			this.ifolderPath.Text = resources.GetString("ifolderPath.Text");
			this.ifolderPath.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ifolderPath.TextAlign")));
			this.ifolderPath.Visible = ((bool)(resources.GetObject("ifolderPath.Visible")));
			this.ifolderPath.WordWrap = ((bool)(resources.GetObject("ifolderPath.WordWrap")));
			this.ifolderPath.TextChanged += new System.EventHandler(this.ifolderPath_TextChanged);
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
			// browse
			// 
			this.browse.AccessibleDescription = resources.GetString("browse.AccessibleDescription");
			this.browse.AccessibleName = resources.GetString("browse.AccessibleName");
			this.browse.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("browse.Anchor")));
			this.browse.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("browse.BackgroundImage")));
			this.browse.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("browse.Dock")));
			this.browse.Enabled = ((bool)(resources.GetObject("browse.Enabled")));
			this.browse.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("browse.FlatStyle")));
			this.browse.Font = ((System.Drawing.Font)(resources.GetObject("browse.Font")));
			this.browse.Image = ((System.Drawing.Image)(resources.GetObject("browse.Image")));
			this.browse.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("browse.ImageAlign")));
			this.browse.ImageIndex = ((int)(resources.GetObject("browse.ImageIndex")));
			this.browse.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("browse.ImeMode")));
			this.browse.Location = ((System.Drawing.Point)(resources.GetObject("browse.Location")));
			this.browse.Name = "browse";
			this.browse.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("browse.RightToLeft")));
			this.browse.Size = ((System.Drawing.Size)(resources.GetObject("browse.Size")));
			this.browse.TabIndex = ((int)(resources.GetObject("browse.TabIndex")));
			this.browse.Text = resources.GetString("browse.Text");
			this.browse.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("browse.TextAlign")));
			this.browse.Visible = ((bool)(resources.GetObject("browse.Visible")));
			this.browse.Click += new System.EventHandler(this.browse_Click);
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
			// encryption
			// 
			this.encryption.AccessibleDescription = resources.GetString("encryption.AccessibleDescription");
			this.encryption.AccessibleName = resources.GetString("encryption.AccessibleName");
			this.encryption.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("encryption.Anchor")));
			this.encryption.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("encryption.Appearance")));
			this.encryption.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("encryption.BackgroundImage")));
			this.encryption.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("encryption.CheckAlign")));
			this.encryption.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("encryption.Dock")));
			this.encryption.Enabled = ((bool)(resources.GetObject("encryption.Enabled")));
			this.encryption.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("encryption.FlatStyle")));
			this.encryption.Font = ((System.Drawing.Font)(resources.GetObject("encryption.Font")));
			this.encryption.Image = ((System.Drawing.Image)(resources.GetObject("encryption.Image")));
			this.encryption.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("encryption.ImageAlign")));
			this.encryption.ImageIndex = ((int)(resources.GetObject("encryption.ImageIndex")));
			this.encryption.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("encryption.ImeMode")));
			this.encryption.Location = ((System.Drawing.Point)(resources.GetObject("encryption.Location")));
			this.encryption.Name = "encryption";
			this.encryption.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("encryption.RightToLeft")));
			this.encryption.Size = ((System.Drawing.Size)(resources.GetObject("encryption.Size")));
			this.encryption.TabIndex = ((int)(resources.GetObject("encryption.TabIndex")));
			this.encryption.Text = resources.GetString("encryption.Text");
			this.encryption.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("encryption.TextAlign")));
			this.encryption.Visible = ((bool)(resources.GetObject("encryption.Visible")));
			this.encryption.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
			// 
			// ssl
			// 
			this.ssl.AccessibleDescription = resources.GetString("ssl.AccessibleDescription");
			this.ssl.AccessibleName = resources.GetString("ssl.AccessibleName");
			this.ssl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ssl.Anchor")));
			this.ssl.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("ssl.Appearance")));
			this.ssl.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ssl.BackgroundImage")));
			this.ssl.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ssl.CheckAlign")));
			this.ssl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ssl.Dock")));
			this.ssl.Enabled = ((bool)(resources.GetObject("ssl.Enabled")));
			this.ssl.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("ssl.FlatStyle")));
			this.ssl.Font = ((System.Drawing.Font)(resources.GetObject("ssl.Font")));
			this.ssl.Image = ((System.Drawing.Image)(resources.GetObject("ssl.Image")));
			this.ssl.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ssl.ImageAlign")));
			this.ssl.ImageIndex = ((int)(resources.GetObject("ssl.ImageIndex")));
			this.ssl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ssl.ImeMode")));
			this.ssl.Location = ((System.Drawing.Point)(resources.GetObject("ssl.Location")));
			this.ssl.Name = "ssl";
			this.ssl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ssl.RightToLeft")));
			this.ssl.Size = ((System.Drawing.Size)(resources.GetObject("ssl.Size")));
			this.ssl.TabIndex = ((int)(resources.GetObject("ssl.TabIndex")));
			this.ssl.Text = resources.GetString("ssl.Text");
			this.ssl.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ssl.TextAlign")));
			this.ssl.Visible = ((bool)(resources.GetObject("ssl.Visible")));
			// 
			// CreateiFolder
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
			this.Controls.Add(this.ssl);
			this.Controls.Add(this.encryption);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.browse);
			this.Controls.Add(this.ifolderPath);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.servers);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "CreateiFolder";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Closing += new System.ComponentModel.CancelEventHandler(this.CreateiFolder_Closing);
			this.Load += new System.EventHandler(this.CreateiFolder_Load);
			this.Activated += new System.EventHandler(this.CreateiFolder_Activated);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers
		private void CreateiFolder_Load(object sender, System.EventArgs e)
		{
			this.Icon = new Icon(Path.Combine(loadPath, @"res\ifolder_16.ico"));
			this.ok.Enabled = false;
			this.encryption.Checked = false;
			this.encryption.Enabled = this.ssl.Enabled = false;
			this.ssl.Checked = true;
			if (servers.Items.Count == 0)
			{
				try
				{
					XmlDocument domainsDoc = new XmlDocument();
					domainsDoc.Load(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "domain.list"));

					XmlElement element = (XmlElement)domainsDoc.SelectSingleNode("/domains");

					// Get the ID of the default domain.
					XmlElement defaultDomainElement = (XmlElement)domainsDoc.SelectSingleNode("/domains/defaultDomain");
					string defaultID = defaultDomainElement.GetAttribute("ID");

					// Get the domains.
					// Look for a domain with this ID.
					XmlNodeList nodeList = element.GetElementsByTagName("domain");
					foreach (XmlNode node in nodeList)
					{
						string name = ((XmlElement)node).GetAttribute("name");
						string id = ((XmlElement)node).GetAttribute("ID");

						DomainItem domainItem = new DomainItem(name, id);
						servers.Items.Add(domainItem);
						if (id.Equals(defaultID))
						{
							selectedDomain = domainItem;
						}
					}

					if (selectedDomain != null)
					{
						servers.SelectedItem = selectedDomain;
					}
					else
							servers.SelectedIndex = 0;
				}
				catch
				{
				}
			}
			else
			{
				if (selectedDomain != null)
				{
					servers.SelectedItem = selectedDomain;
				}
				else if (servers.Items.Count > 0)
				{
					servers.SelectedIndex = 0;
				}
			}
			if (!ifolderPath.Text.Equals(string.Empty))
			{
				ifolderPath.ReadOnly = true;
				browse.Enabled = false;
			}
		}

		private void CreateiFolder_Activated(object sender, System.EventArgs e)
		{
			if (ifolderPath.ReadOnly)
			{
				servers.Focus();
				servers.SelectedIndex = 0;
			}
			else
			{
				ifolderPath.Focus();
				servers.SelectedIndex = 0;
			}
		}

		private void browse_Click(object sender, System.EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

			folderBrowserDialog.Description = resourceManager.GetString("chooseFolder");
			folderBrowserDialog.SelectedPath = ifolderPath.Text;

			if(folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				ifolderPath.Text = folderBrowserDialog.SelectedPath;
			}
		}

		private void ifolderPath_TextChanged(object sender, System.EventArgs e)
		{
			ok.Enabled = (ifolderPath.Text.Length > 0) && (servers.Items.Count != 0);
		}

		private void servers_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			ok.Enabled = (ifolderPath.Text.Length > 0) && (servers.SelectedItem != null);
			// Change the corresponding encryption check boxes
			DomainItem domain = (DomainItem) servers.SelectedItem;
			int SecurityPolicy = ifWebService.GetSecurityPolicy(domain.ID);
			this.encryption.Checked = false;
			this.encryption.Enabled = this.ssl.Enabled = false;
			this.ssl.Checked = true;
			if(SecurityPolicy !=0)
			{
				if( (SecurityPolicy & (int)SecurityState.encryption) == (int) SecurityState.encryption)
				{
					if( (SecurityPolicy & (int)SecurityState.enforceEncryption) == (int) SecurityState.enforceEncryption)
						encryption.Checked = true;
					else
					{
						encryption.Enabled = true;
						ssl.Enabled = true;
					}
				}
				else
					ssl.Checked = true;
				/*
				if( (SecurityPolicy & (int)SecurityState.SSL) == (int) SecurityState.SSL)
				{
					if( (SecurityPolicy & (int)SecurityState.enforceSSL) == (int) SecurityState.enforceSSL)
						ssl.Checked = true;
					else
						ssl.Enabled = true;
				}
				*/
			}
			else
				ssl.Checked = true;
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			successful = true;
			try
			{
				try
				{
					Uri uriPath = new Uri( 
						ifolderPath.Text.EndsWith(Path.DirectorySeparatorChar.ToString()) ?
						ifolderPath.Text :
						ifolderPath.Text + Path.DirectorySeparatorChar.ToString());

					if (ifolderPath.Text.StartsWith(@"\\"))
					{
						throw new Exception("Invalid path");
					}
				}
				catch
				{
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("invalidFolder"), resourceManager.GetString("errorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
					successful = false;
					return;
				}

				if (!Directory.Exists(ifolderPath.Text))
				{
					MyMessageBox mmb =new MyMessageBox(resourceManager.GetString("createPrompt"), resourceManager.GetString("createPromptTitle"), string.Empty, MyMessageBoxButtons.YesNo, MyMessageBoxIcon.Question);
					if (mmb.ShowDialog() == DialogResult.Yes)
					{
						string parent = Path.GetDirectoryName(ifolderPath.Text);
						while ((parent != null) && !parent.Equals(string.Empty))
						{
							if (Directory.Exists(parent))
							{
								ifolderPath.Text = ifolderPath.Text.Replace(parent, FixPath(parent));
								break;
							}

							parent = Path.GetDirectoryName(parent);
						}

						Directory.CreateDirectory(ifolderPath.Text);
					}
					else
					{
						successful = false;
					}
				}
				else
				{
					ifolderPath.Text = FixPath( ifolderPath.Text );
				}

				if (successful)
				{
					if (Win32Security.AccessAllowed(ifolderPath.Text))
					{
						Cursor.Current = Cursors.WaitCursor;
						DomainItem domainItem = (DomainItem)servers.SelectedItem;

						// Create the iFolder.
						// TODO item. this web service call should be changed to constain the encr_status
						//Added by Ramesh
						//	iFolderWeb ifolder = ifWebService.CreateiFolderInDomain(ifolderPath.Text, domainItem.ID);
						
						//Console.WriteLine("encryption_status: {0}", encryption_status.SelectedIndex);
						//string str= encryption_status.SelectedIndex==0 ? "0" : "1";
						//MyMessageBox mmb = new MyMessageBox("encryption: Calling createiFolder()", "Check", string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						//mmb.ShowDialog();
						int temp=ifWebService.GetSecurityPolicy(domainItem.ID);
						/*
						int encr_status = 0;
						if(this.encryption.Checked)
							encr_status +=1;
						if(this.ssl.Checked)
							encr_status +=2;
						*/
						iFolderWeb ifolder;
						if( this.ssl.Checked)
						{
							// Non-encrypted folder
							ifolder = this.ifWebService.CreateiFolderInDomain(ifolderPath.Text, domainItem.ID);
						}
						else
						{
							// encrypted folder
							string algorithm = (this.encryption.Checked)? "BlowFish" : "";
							bool passPhraseStatus = false;
							bool passphraseStatus = false;
							try
							{
								passphraseStatus = simws.IsPassPhraseSet(domainItem.ID);
							}
							catch(Exception ex)
							{
								string caption = resourceManager.GetString("errorTitle");
								MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("LoginToDomain")/*"Unable to talk to the server. First login to server"*/ , caption, string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
								mmb.ShowDialog();
								successful = false;
								return;
							}
							if(passphraseStatus == true)
							{
								// if passphrase not given during login
								string passphrasecheck = null;
									passphrasecheck = simws.GetPassPhrase(domainItem.ID);
									if( passphrasecheck == null || passphrasecheck =="")
									{
										VerifyPassphraseDialog vpd = new VerifyPassphraseDialog(domainItem.ID, this.simws);
										vpd.ShowDialog();
										passPhraseStatus = vpd.PassphraseStatus;
									}
								else
								{
									passPhraseStatus = true;
								}
							}
							else
							{
								// Passphrase not enterd at the time of login...
								EnterPassphraseDialog enterPassPhrase= new EnterPassphraseDialog(domainItem.ID, this.simws);
								enterPassPhrase.ShowDialog();
								passPhraseStatus = enterPassPhrase.PassphraseStatus;
							}
							if( passPhraseStatus == false)
							{
								// No Passphrase
								successful = false;
								MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("PPForEncryption")/*"Passphrase needs to be supplied for encrypting the iFolder"*/, resourceManager.GetString("$this.Text")/*"Passphrase error"*/, string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
								mmb.ShowDialog();
							}
							else
							{
								// check for passphrase existence and display corresponding dialogs.
								string Passphrase = simws.GetPassPhrase(domainItem.ID);
								ifolder = this.ifWebService.CreateiFolderInDomainEncr(ifolderPath.Text, domainItem.ID, false, algorithm, Passphrase);
							}

						}
						/*
						bool SSL = this.ssl.Checked;
						string algorithm = (this.encryption.Checked)? "BlowFish" : "";
						iFolderWeb ifolder = ifWebService.CreateiFolderInDomainEncr(ifolderPath.Text, domainItem.ID, SSL, algorithm );
						*/
						// Notify the shell.
						Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, ifolderPath.Text, IntPtr.Zero);

						Cursor.Current = Cursors.Default;
					}
					else
					{
						successful = false;
						MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("accessDenied"), resourceManager.GetString("accessErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
					}
				}
			}
			catch (Exception ex)
			{
				successful = false;
				Cursor.Current = Cursors.Default;
				MyMessageBox mmb;
				string message;
				string caption = resourceManager.GetString("pathInvalidErrorTitle");

				if (ex.Message.IndexOf("InvalidCharactersPath") != -1)
				{
					message = resourceManager.GetString("invalidCharsError");
				}
				else if (ex.Message.IndexOf("AtOrInsideStorePath") != -1)
				{
					message = resourceManager.GetString("pathInStoreError");
				}
				else if (ex.Message.IndexOf("ContainsStorePath") != -1)
				{
					message = resourceManager.GetString("pathContainsStoreError");
				}
				else if (ex.Message.IndexOf("SystemDirectoryPath") != -1)
				{
					message = resourceManager.GetString("systemDirError");
				}
				else if (ex.Message.IndexOf("SystemDrivePath") != -1)
				{
					message = resourceManager.GetString("systemDriveError");
				}
				else if (ex.Message.IndexOf("IncludesWinDirPath") != -1)
				{
					message = resourceManager.GetString("winDirError");
				}
				else if (ex.Message.IndexOf("IncludesProgFilesPath") != -1)
				{
					message = resourceManager.GetString("progFilesDirError");
				}
				else if (ex.Message.IndexOf("ContainsCollectionPath") != -1)
				{
					message = resourceManager.GetString("containsiFolderError");
				}
				else if (ex.Message.IndexOf("AtOrInsideCollectionPath") != -1)
				{
					message = resourceManager.GetString("pathIniFolderError");
				}
				else if (ex.Message.IndexOf("RootOfDrivePath") != -1)
				{
					message = resourceManager.GetString("rootDriveError");
				}
				else if (ex.Message.IndexOf("NotFixedDrivePath") != -1)
				{
					message = resourceManager.GetString("networkPathError");
				}
				else
				{
					message = resourceManager.GetString("iFolderCreateError");
					caption = resourceManager.GetString("errorTitle");
				}

				mmb = new MyMessageBox(message, caption, string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}
		}

		private void CreateiFolder_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if ((this.DialogResult == DialogResult.OK) && !successful)
			{
				e.Cancel = true;
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Converts a path to the proper case.
		/// </summary>
		/// <param name="path">The path to convert.</param>
		/// <returns>The path in it's proper case according to the file system.</returns>
		public static string FixPath(string path)
		{
			if (path[1].Equals(':'))
			{
				string root = path.Substring(0, 2);
				path = path.Replace(root, root.ToUpper());
			}

			try
			{
				string parent = path;
				string temp = string.Empty;
				while (true)
				{
					string file = Path.GetFileName(parent);
					parent = Path.GetDirectoryName(parent);
					if ((parent == null) || parent.Equals(string.Empty))
					{
						string psub = path.Substring(3);
						if (string.Compare(psub, temp, true) == 0)
							path = path.Replace(psub, temp);
						break;
					}

					string[] dirs = Directory.GetFileSystemEntries(parent, file);
					if (dirs.Length == 1)
					{
						temp = Path.Combine(Path.GetFileName(dirs[0]), temp);
					}
				}
			}
			catch {}

			return path;
		}
		#endregion

		#region Private Methods

		private void SetSecurityState()
		{
			DomainItem domain = (DomainItem) servers.SelectedItem;
			this.encryption.Checked = this.ssl.Checked = false;
			this.encryption.Enabled = this.ssl.Enabled = false;
			if(domain == null)
				return;
			int securityPolicy = ifWebService.GetSecurityPolicy(domain.ID);

			if( (securityPolicy & 0x0001) == 0x01)
			{
				if( (securityPolicy & 0x0010) == 0x0010)
					this.encryption.Checked = true;
				else
					this.encryption.Enabled = true;
			}
			if( (securityPolicy & 0x0100) == 0x0100)
			{
			if( (securityPolicy & 0x01000) == 0x01000)
				this.ssl.Checked = true;
			else
				this.ssl.Enabled = true;
			}

		}
		private int calculateSize(Control control, int delta)
		{
			int size;
			Graphics g = control.CreateGraphics();
			try
			{
				SizeF textSize = g.MeasureString(control.Text, control.Font);
				size = (int)Math.Ceiling(textSize.Width) - control.Width;
			}
			finally
			{
				g.Dispose();
			}

			return (int)Math.Max(delta, size);
		}
		private void checkBox1_CheckedChanged(object sender, System.EventArgs e)
		{
		
		}
		#endregion

	}

	/// <summary>
	/// Summary description for VerifyPassphraseDialog.
	/// </summary>
	internal class VerifyPassphraseDialog : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.PictureBox waterMark;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label lblPassphrase;
		private System.Windows.Forms.TextBox Passphrase;
		private System.Windows.Forms.CheckBox savePassphrase;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private SimiasWebService simws;
		private string DomainID;
		private bool status;
		private System.ComponentModel.Container components = null;

		public bool PassphraseStatus
		{
			get
			{
				return status;
			}
		}

		public VerifyPassphraseDialog(string domainID, SimiasWebService simws)
		{
			this.DomainID = domainID;
			this.simws = simws;
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(CreateiFolder));
			this.panel1 = new System.Windows.Forms.Panel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.waterMark = new System.Windows.Forms.PictureBox();
			this.lblPassphrase = new System.Windows.Forms.Label();
			this.Passphrase = new System.Windows.Forms.TextBox();
			this.savePassphrase = new System.Windows.Forms.CheckBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.Blue;
			this.panel1.Controls.Add(this.waterMark);
			this.panel1.Controls.Add(this.pictureBox1);
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(448, 65);
			this.panel1.TabIndex = 1;
			// 
			// waterMark
			// 
			//this.waterMark.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(101)), ((System.Byte)(163)), ((System.Byte)(237)));
			this.waterMark.Location = new System.Drawing.Point(0,0);
			this.waterMark.Name = "waterMark";
			this.waterMark.Size = new System.Drawing.Size(159, 65);
			this.waterMark.TabIndex = 0;
			this.waterMark.TabStop = false;
			//
			//pictureBox1
			//
			this.pictureBox1.Location = new System.Drawing.Point(159, 0);
			this.pictureBox1.Size = new System.Drawing.Size(320, 65);
			// 
			// lblPassphrase
			// 
			this.lblPassphrase.Location = new System.Drawing.Point(16, 76);
			this.lblPassphrase.Name = "lblPassphrase";
			this.lblPassphrase.Size = new System.Drawing.Size(140, 20);
			this.lblPassphrase.TabIndex = 2;
			this.lblPassphrase.Text = resources.GetString("EnterPPText");//"Enter Passphrase:";
			this.lblPassphrase.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// Passphrase
			// 
			this.Passphrase.Location = new System.Drawing.Point(156, 76);
			this.Passphrase.Name = "Passphrase";
			this.Passphrase.Size = new System.Drawing.Size(240, 20);
			this.Passphrase.TabIndex = 3;
			this.Passphrase.Text = "";
			this.Passphrase.TextChanged +=new EventHandler(Passphrase_TextChanged);
			// 
			// savePassphrase
			// 
			this.savePassphrase.Location = new System.Drawing.Point(156, 104);
			this.savePassphrase.Name = "savePassphrase";
			this.savePassphrase.Size = new System.Drawing.Size(240, 20);
			this.savePassphrase.TabIndex = 4;
			this.savePassphrase.Text = resources.GetString("RememberPPText");//"Remember Passphrase";
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(239, 134);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(72, 24);
			this.btnCancel.TabIndex = 5;
			this.btnCancel.Text = resources.GetString("cancel.Text");//"&Cancel";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(319, 134);
			this.btnOk.Name = "btnOk";
			this.btnOk.TabIndex = 6;
			this.btnOk.Text = resources.GetString("ok.Text");//"&Ok";
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// VerifyPassphraseDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(420, 178);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.savePassphrase);
			this.Controls.Add(this.Passphrase);
			this.Controls.Add(this.lblPassphrase);
			this.Controls.Add(this.panel1);
			this.Name = "VerifyPassphraseDialog";
			this.Text = resources.GetString("VerifyPPTitle");//"Verify Passphrase";
			this.Load += new System.EventHandler(this.VerifyPassphraseDialog_Load);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			simws.StorePassPhrase(DomainID, "", CredentialType.None, false);
			status = false;
			this.Dispose();
			this.Close();
		}

		private void btnOk_Click(object sender, System.EventArgs e)
		{
			Status passPhraseStatus =  simws.ValidatePassPhrase(this.DomainID, this.Passphrase.Text);
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(CreateiFolder));
			if( passPhraseStatus != null)
			{
				if( passPhraseStatus.statusCode == StatusCodes.PassPhraseInvalid)  // check for invalid passphrase
				{
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resources.GetString("ValidatePPError")/*"Unable to validate the passphrase"*/, resources.GetString("VerifyPPTitle")/*"Passphrase Invalid"*/, resources.GetString("TryAgain")/*"Please try again"*/, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
					mmb.Dispose();	
				}
				else if(passPhraseStatus.statusCode == StatusCodes.Success)
				{
					try
					{
						simws.StorePassPhrase( DomainID, this.Passphrase.Text, CredentialType.Basic, this.savePassphrase.Checked);
						status = true;
						this.Dispose();
						this.Close();
					}
					catch(Exception ex) 
					{
						// TODO: Show error Messahe
					//	MessageBox.Show("Unable to store passphrase. "+ex.Message);
						status = false;
					}
				}
			}
		}

		private void VerifyPassphraseDialog_Load(object sender, System.EventArgs e)
		{
			this.btnOk.Enabled = false;
			//this.waterMark.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder48.png"));
			this.waterMark.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder-banner.png"));
			this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
			this.pictureBox1.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder-banner-scaler.png"));
		}

		private void Passphrase_TextChanged(object sender, EventArgs e)
		{
			if( this.Passphrase.Text.Length > 0)
				this.btnOk.Enabled = true;
			else
				this.btnOk.Enabled = false;
		}
	}

	/// <summary>
	/// Summary description for EnterPassphraseDialog.
	/// </summary>
	internal class EnterPassphraseDialog : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.PictureBox waterMark;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.ComboBox RecoveryAgentCombo;
		private System.Windows.Forms.Label lblRecoveryAgent;
		private System.Windows.Forms.TextBox Passphrase;
		private System.Windows.Forms.TextBox RetypePassphrase;
		private System.Windows.Forms.Label lblPassphrase;
		private System.Windows.Forms.Label lblRetypePassphrase;
		private System.Windows.Forms.CheckBox savePassphrase;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private System.ComponentModel.Container components = null;
		private SimiasWebService simws;
		private string DomainID;
		private bool	status;

		public bool PassphraseStatus
		{
			get
			{
				return status;
			}
		}

		public EnterPassphraseDialog(string domainID, SimiasWebService simws)
		{
			this.DomainID = domainID;
			this.simws = simws;
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(CreateiFolder));
			this.panel1 = new System.Windows.Forms.Panel();
			this.waterMark = new System.Windows.Forms.PictureBox();
			this.pictureBox1 = new PictureBox();
			this.RecoveryAgentCombo = new System.Windows.Forms.ComboBox();
			this.lblRecoveryAgent = new System.Windows.Forms.Label();
			this.Passphrase = new System.Windows.Forms.TextBox();
			this.RetypePassphrase = new System.Windows.Forms.TextBox();
			this.lblPassphrase = new System.Windows.Forms.Label();
			this.lblRetypePassphrase = new System.Windows.Forms.Label();
			this.savePassphrase = new System.Windows.Forms.CheckBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.Blue;
			this.panel1.Controls.Add(this.waterMark);
			this.panel1.Controls.Add(this.pictureBox1);
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(448, 65);
			this.panel1.TabIndex = 1;
			// 
			// waterMark
			// 
			//this.waterMark.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(101)), ((System.Byte)(163)), ((System.Byte)(237)));
			this.waterMark.Location = new System.Drawing.Point(0, 0);
			this.waterMark.Name = "waterMark";
			this.waterMark.Size = new System.Drawing.Size(159, 65);
			this.waterMark.TabIndex = 0;
			this.waterMark.TabStop = false;
			//
			//pictureBox1
			//
			this.pictureBox1.Location = new Point(159, 0);
			this.pictureBox1.Size = new Size(320, 65);
			this.pictureBox1.Name = "pictureBox1";
			// 
			// RecoveryAgentCombo
			// 
			this.RecoveryAgentCombo.Location = new System.Drawing.Point(156, 76);
			this.RecoveryAgentCombo.Name = "RecoveryAgentCombo";
			this.RecoveryAgentCombo.Size = new System.Drawing.Size(240, 21);
			this.RecoveryAgentCombo.TabIndex = 2;
			// 
			// lblRecoveryAgent
			// 
			this.lblRecoveryAgent.Location = new System.Drawing.Point(16, 76);
			this.lblRecoveryAgent.Name = "lblRecoveryAgent";
			this.lblRecoveryAgent.Size = new System.Drawing.Size(140, 20);
			this.lblRecoveryAgent.TabIndex = 3;
			this.lblRecoveryAgent.Text = resources.GetString("RecoveryAgent");//"Recovery Agent";
			this.lblRecoveryAgent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// Passphrase
			// 
			this.Passphrase.Location = new System.Drawing.Point(156, 104);
			this.Passphrase.Name = "Passphrase";
			this.Passphrase.Size = new System.Drawing.Size(240, 20);
			this.Passphrase.TabIndex = 4;
			this.Passphrase.Text = "";
			this.Passphrase.TextChanged += new System.EventHandler(this.Passphrase_TextChanged);
			// 
			// RetypePassphrase
			// 
			this.RetypePassphrase.Location = new System.Drawing.Point(156, 132);
			this.RetypePassphrase.Name = "RetypePassphrase";
			this.RetypePassphrase.Size = new System.Drawing.Size(240, 20);
			this.RetypePassphrase.TabIndex = 5;
			this.RetypePassphrase.Text = "";
			this.RetypePassphrase.TextChanged += new System.EventHandler(this.RetypePassphrase_TextChanged);
			// 
			// lblPassphrase
			// 
			this.lblPassphrase.Location = new System.Drawing.Point(16, 104);
			this.lblPassphrase.Name = "lblPassphrase";
			this.lblPassphrase.Size = new System.Drawing.Size(120, 20);
			this.lblPassphrase.TabIndex = 6;
			this.lblPassphrase.Text = resources.GetString("EnterPPText");//"Enter Passphrase";
			// 
			// lblRetypePassphrase
			// 
			this.lblRetypePassphrase.Location = new System.Drawing.Point(16, 132);
			this.lblRetypePassphrase.Name = "lblRetypePassphrase";
			this.lblRetypePassphrase.Size = new System.Drawing.Size(120, 20);
			this.lblRetypePassphrase.TabIndex = 7;
			this.lblRetypePassphrase.Text = resources.GetString("RetypePP");//"Retype Passphrase";
			// 
			// savePassphrase
			// 
			this.savePassphrase.Location = new System.Drawing.Point(156, 160);
			this.savePassphrase.Name = "savePassphrase";
			this.savePassphrase.Size = new System.Drawing.Size(240, 20);
			this.savePassphrase.TabIndex = 8;
			this.savePassphrase.Text = resources.GetString("RememberPPText");//"Remember Passphrase";
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(239, 200);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 9;
			this.btnCancel.Text = resources.GetString("cancel.Text");//"&Cancel";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(319, 200);
			this.btnOk.Name = "btnOk";
			this.btnOk.TabIndex = 10;
			this.btnOk.Text = "&Ok";
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// EnterPassphraseDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(420, 242);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.savePassphrase);
			this.Controls.Add(this.lblRetypePassphrase);
			this.Controls.Add(this.lblPassphrase);
			this.Controls.Add(this.RetypePassphrase);
			this.Controls.Add(this.Passphrase);
			this.Controls.Add(this.lblRecoveryAgent);
			this.Controls.Add(this.RecoveryAgentCombo);
			this.Controls.Add(this.panel1);
			this.Name = "EnterPassphraseDialog";
			this.Text = resources.GetString("EnterPPTitle");//"Enter Passphrase";
			this.Load += new System.EventHandler(this.EnterPassphraseDialog_Load);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			simws.StorePassPhrase(DomainID, "", CredentialType.None, false);
			status = false;
			this.Dispose();
			this.Close();
		}

		private void btnOk_Click(object sender, System.EventArgs e)
		{
			// Check for passphrase
			/*	If passphrase is valid try setting the passphrase.
			 *	if successful try storing on local place
			 *	if success close dialog.
			 *	else show dialog again..
			 * 
			 */
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(CreateiFolder));
			if( this.Passphrase.Text == this.RetypePassphrase.Text)
			{
				string publicKey = "";
				string ragent = null;
				if( this.RecoveryAgentCombo.SelectedItem != null && this.RecoveryAgentCombo.SelectedText != "None")
				{
					// Show the certificate.....
					byte[] CertificateObj = this.simws.GetRACertificateOnClient(DomainID, this.RecoveryAgentCombo.SelectedText);
					System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(CertificateObj);
					MyMessageBox mmb = new MyMessageBox( "Verify Certificate", "Verify Certificate", cert.ToString(true), MyMessageBoxButtons.YesNo, MyMessageBoxIcon.Question, MyMessageBoxDefaultButton.Button2 );
					DialogResult messageDialogResult = mmb.ShowDialog();
					mmb.Dispose();
					mmb.Close();
					if( messageDialogResult != DialogResult.OK )
						return;
					else
					{
						ragent = this.RecoveryAgentCombo.SelectedText;
						publicKey = cert.GetPublicKeyString();
					}
					//return;
				}
				Status passPhraseStatus = null;
				try
				{
					passPhraseStatus = simws.SetPassPhrase( DomainID, this.Passphrase.Text, null, publicKey);
				}
				catch(Exception ex)
				{
					MessageBox.Show(resources.GetString("ErrorSetPP")/*"Unable to set passphrase. "*/+ ex.Message);
				}
				if(passPhraseStatus.statusCode == StatusCodes.Success)
				{
					simws.StorePassPhrase( DomainID, this.Passphrase.Text, CredentialType.Basic, this.savePassphrase.Checked);
					string passphr = simws.GetPassPhrase(DomainID);
					//MessageBox.Show("Passphrase is set & stored", passphr, MessageBoxButtons.OK);
					this.status= simws.IsPassPhraseSet(DomainID);
					if( status == true)
					{
						Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resources.GetString("SetPPSuccess")/*"Successfully set the passphrase"*/, resources.GetString("EnterPPTitle")/*"Enter passphrase"*/, "",MyMessageBoxButtons.OK, MyMessageBoxIcon.None);
						mmb.ShowDialog();
						mmb.Dispose();
						this.Dispose();
						this.Close();
					}
				}
				else 
				{
					// Unable to set the passphrase
					status = false;
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resources.GetString("ErrorSetPP")/*"Unable to set the passphrase"*/, resources.GetString("EnterPPTitle")/*"Error setting the passphrase"*/, resources.GetString("TryAgain")/*"Please try again"*/, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
					mmb.Dispose();
				}
			}
			else
			{
				MessageBox.Show(resources.GetString("PPSDontMatch")/*"Passphrase and re-typed passphrase doesn't match"*/);
				status = false;
			}
		}

		private void EnterPassphraseDialog_Load(object sender, System.EventArgs e)
		{
			this.btnOk.Enabled = false;
			//this.waterMark.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder48.png"));
			this.waterMark.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder-banner.png"));
			this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
			this.pictureBox1.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder-banner-scaler.png"));
			string[] rAgents= this.simws.GetRAListOnClient(DomainID);
			foreach( string rAgent in rAgents)
			{
				this.RecoveryAgentCombo.Items.Add( rAgent ); 
				//MessageBox.Show(String.Format("Adding {0}", rAgent));
			}
		}

		private void Passphrase_TextChanged(object sender, System.EventArgs e)
		{
			UpdateSensitivity();
		}

		private void RetypePassphrase_TextChanged(object sender, System.EventArgs e)
		{
			UpdateSensitivity();
		}

		private void UpdateSensitivity()
		{
			if( this.Passphrase.Text.Length > 0 && this.Passphrase.Text == this.RetypePassphrase.Text)
				this.btnOk.Enabled = true;
			else
				this.btnOk.Enabled = false;
		}
	}
}

