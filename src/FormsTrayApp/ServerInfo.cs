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
using System.Net;

using Novell.iFolderCom;
using Novell.iFolder.Install;
using Simias.Client;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for ServerInfo.
	/// </summary>
	public class ServerInfo : System.Windows.Forms.Form
	{
		System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(ServerInfo));
		bool initialLogin = true;
		string domainID;
		bool cancelled = false;
		bool updateStarted = false;
		iFolderSettings ifolderSettings;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.TextBox serverIP;
		private System.Windows.Forms.TextBox password;
		private System.Windows.Forms.TextBox userName;
		private System.Windows.Forms.PictureBox banner;
		private iFolderWebService ifWebService;
		private System.Windows.Forms.Label userLabel1;
		private System.Windows.Forms.Label serverLabel1;
		private System.Windows.Forms.Label passwordLabel1;
		private System.Windows.Forms.Label serverLabel2;
		private System.Windows.Forms.Label userLabel2;
		private System.Windows.Forms.Label passwordLabel2;
		private System.Windows.Forms.Label serverName;
		private System.Windows.Forms.Label userName2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Constructs a ServerInfo object.
		/// </summary>
		/// <param name="ifolderWebService">The iFolderWebService object to use.</param>
		/// <param name="domainID">The ID of the domain.</param>
		public ServerInfo(iFolderWebService ifolderWebService, string domainID)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.domainID = domainID;

			if (!this.domainID.Equals(string.Empty))
			{
				initialLogin = false;
				ok.Enabled = true;
				userLabel1.Visible = passwordLabel1.Visible = serverLabel1.Visible = false;
				userLabel2.Visible = passwordLabel2.Visible = serverLabel2.Visible = true;

				userName.Visible = serverIP.Visible = false;
				serverName.Visible = userName2.Visible = true;

				password.Top = serverIP.Top;
			}

			ifWebService = ifolderWebService;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ServerInfo));
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.userName = new System.Windows.Forms.TextBox();
			this.serverIP = new System.Windows.Forms.TextBox();
			this.userLabel1 = new System.Windows.Forms.Label();
			this.serverLabel1 = new System.Windows.Forms.Label();
			this.password = new System.Windows.Forms.TextBox();
			this.passwordLabel1 = new System.Windows.Forms.Label();
			this.banner = new System.Windows.Forms.PictureBox();
			this.serverLabel2 = new System.Windows.Forms.Label();
			this.userLabel2 = new System.Windows.Forms.Label();
			this.passwordLabel2 = new System.Windows.Forms.Label();
			this.serverName = new System.Windows.Forms.Label();
			this.userName2 = new System.Windows.Forms.Label();
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
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// userName
			// 
			this.userName.AccessibleDescription = resources.GetString("userName.AccessibleDescription");
			this.userName.AccessibleName = resources.GetString("userName.AccessibleName");
			this.userName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("userName.Anchor")));
			this.userName.AutoSize = ((bool)(resources.GetObject("userName.AutoSize")));
			this.userName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("userName.BackgroundImage")));
			this.userName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("userName.Dock")));
			this.userName.Enabled = ((bool)(resources.GetObject("userName.Enabled")));
			this.userName.Font = ((System.Drawing.Font)(resources.GetObject("userName.Font")));
			this.userName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("userName.ImeMode")));
			this.userName.Location = ((System.Drawing.Point)(resources.GetObject("userName.Location")));
			this.userName.MaxLength = ((int)(resources.GetObject("userName.MaxLength")));
			this.userName.Multiline = ((bool)(resources.GetObject("userName.Multiline")));
			this.userName.Name = "userName";
			this.userName.PasswordChar = ((char)(resources.GetObject("userName.PasswordChar")));
			this.userName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("userName.RightToLeft")));
			this.userName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("userName.ScrollBars")));
			this.userName.Size = ((System.Drawing.Size)(resources.GetObject("userName.Size")));
			this.userName.TabIndex = ((int)(resources.GetObject("userName.TabIndex")));
			this.userName.Text = resources.GetString("userName.Text");
			this.userName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("userName.TextAlign")));
			this.userName.Visible = ((bool)(resources.GetObject("userName.Visible")));
			this.userName.WordWrap = ((bool)(resources.GetObject("userName.WordWrap")));
			this.userName.TextChanged += new System.EventHandler(this.userName_TextChanged);
			// 
			// serverIP
			// 
			this.serverIP.AccessibleDescription = resources.GetString("serverIP.AccessibleDescription");
			this.serverIP.AccessibleName = resources.GetString("serverIP.AccessibleName");
			this.serverIP.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("serverIP.Anchor")));
			this.serverIP.AutoSize = ((bool)(resources.GetObject("serverIP.AutoSize")));
			this.serverIP.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("serverIP.BackgroundImage")));
			this.serverIP.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("serverIP.Dock")));
			this.serverIP.Enabled = ((bool)(resources.GetObject("serverIP.Enabled")));
			this.serverIP.Font = ((System.Drawing.Font)(resources.GetObject("serverIP.Font")));
			this.serverIP.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("serverIP.ImeMode")));
			this.serverIP.Location = ((System.Drawing.Point)(resources.GetObject("serverIP.Location")));
			this.serverIP.MaxLength = ((int)(resources.GetObject("serverIP.MaxLength")));
			this.serverIP.Multiline = ((bool)(resources.GetObject("serverIP.Multiline")));
			this.serverIP.Name = "serverIP";
			this.serverIP.PasswordChar = ((char)(resources.GetObject("serverIP.PasswordChar")));
			this.serverIP.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("serverIP.RightToLeft")));
			this.serverIP.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("serverIP.ScrollBars")));
			this.serverIP.Size = ((System.Drawing.Size)(resources.GetObject("serverIP.Size")));
			this.serverIP.TabIndex = ((int)(resources.GetObject("serverIP.TabIndex")));
			this.serverIP.Text = resources.GetString("serverIP.Text");
			this.serverIP.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("serverIP.TextAlign")));
			this.serverIP.Visible = ((bool)(resources.GetObject("serverIP.Visible")));
			this.serverIP.WordWrap = ((bool)(resources.GetObject("serverIP.WordWrap")));
			this.serverIP.TextChanged += new System.EventHandler(this.serverIP_TextChanged);
			// 
			// userLabel1
			// 
			this.userLabel1.AccessibleDescription = resources.GetString("userLabel1.AccessibleDescription");
			this.userLabel1.AccessibleName = resources.GetString("userLabel1.AccessibleName");
			this.userLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("userLabel1.Anchor")));
			this.userLabel1.AutoSize = ((bool)(resources.GetObject("userLabel1.AutoSize")));
			this.userLabel1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("userLabel1.Dock")));
			this.userLabel1.Enabled = ((bool)(resources.GetObject("userLabel1.Enabled")));
			this.userLabel1.Font = ((System.Drawing.Font)(resources.GetObject("userLabel1.Font")));
			this.userLabel1.Image = ((System.Drawing.Image)(resources.GetObject("userLabel1.Image")));
			this.userLabel1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("userLabel1.ImageAlign")));
			this.userLabel1.ImageIndex = ((int)(resources.GetObject("userLabel1.ImageIndex")));
			this.userLabel1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("userLabel1.ImeMode")));
			this.userLabel1.Location = ((System.Drawing.Point)(resources.GetObject("userLabel1.Location")));
			this.userLabel1.Name = "userLabel1";
			this.userLabel1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("userLabel1.RightToLeft")));
			this.userLabel1.Size = ((System.Drawing.Size)(resources.GetObject("userLabel1.Size")));
			this.userLabel1.TabIndex = ((int)(resources.GetObject("userLabel1.TabIndex")));
			this.userLabel1.Text = resources.GetString("userLabel1.Text");
			this.userLabel1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("userLabel1.TextAlign")));
			this.userLabel1.Visible = ((bool)(resources.GetObject("userLabel1.Visible")));
			// 
			// serverLabel1
			// 
			this.serverLabel1.AccessibleDescription = resources.GetString("serverLabel1.AccessibleDescription");
			this.serverLabel1.AccessibleName = resources.GetString("serverLabel1.AccessibleName");
			this.serverLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("serverLabel1.Anchor")));
			this.serverLabel1.AutoSize = ((bool)(resources.GetObject("serverLabel1.AutoSize")));
			this.serverLabel1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("serverLabel1.Dock")));
			this.serverLabel1.Enabled = ((bool)(resources.GetObject("serverLabel1.Enabled")));
			this.serverLabel1.Font = ((System.Drawing.Font)(resources.GetObject("serverLabel1.Font")));
			this.serverLabel1.Image = ((System.Drawing.Image)(resources.GetObject("serverLabel1.Image")));
			this.serverLabel1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("serverLabel1.ImageAlign")));
			this.serverLabel1.ImageIndex = ((int)(resources.GetObject("serverLabel1.ImageIndex")));
			this.serverLabel1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("serverLabel1.ImeMode")));
			this.serverLabel1.Location = ((System.Drawing.Point)(resources.GetObject("serverLabel1.Location")));
			this.serverLabel1.Name = "serverLabel1";
			this.serverLabel1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("serverLabel1.RightToLeft")));
			this.serverLabel1.Size = ((System.Drawing.Size)(resources.GetObject("serverLabel1.Size")));
			this.serverLabel1.TabIndex = ((int)(resources.GetObject("serverLabel1.TabIndex")));
			this.serverLabel1.Text = resources.GetString("serverLabel1.Text");
			this.serverLabel1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("serverLabel1.TextAlign")));
			this.serverLabel1.Visible = ((bool)(resources.GetObject("serverLabel1.Visible")));
			// 
			// password
			// 
			this.password.AccessibleDescription = resources.GetString("password.AccessibleDescription");
			this.password.AccessibleName = resources.GetString("password.AccessibleName");
			this.password.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("password.Anchor")));
			this.password.AutoSize = ((bool)(resources.GetObject("password.AutoSize")));
			this.password.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("password.BackgroundImage")));
			this.password.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("password.Dock")));
			this.password.Enabled = ((bool)(resources.GetObject("password.Enabled")));
			this.password.Font = ((System.Drawing.Font)(resources.GetObject("password.Font")));
			this.password.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("password.ImeMode")));
			this.password.Location = ((System.Drawing.Point)(resources.GetObject("password.Location")));
			this.password.MaxLength = ((int)(resources.GetObject("password.MaxLength")));
			this.password.Multiline = ((bool)(resources.GetObject("password.Multiline")));
			this.password.Name = "password";
			this.password.PasswordChar = ((char)(resources.GetObject("password.PasswordChar")));
			this.password.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("password.RightToLeft")));
			this.password.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("password.ScrollBars")));
			this.password.Size = ((System.Drawing.Size)(resources.GetObject("password.Size")));
			this.password.TabIndex = ((int)(resources.GetObject("password.TabIndex")));
			this.password.Text = resources.GetString("password.Text");
			this.password.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("password.TextAlign")));
			this.password.Visible = ((bool)(resources.GetObject("password.Visible")));
			this.password.WordWrap = ((bool)(resources.GetObject("password.WordWrap")));
			// 
			// passwordLabel1
			// 
			this.passwordLabel1.AccessibleDescription = resources.GetString("passwordLabel1.AccessibleDescription");
			this.passwordLabel1.AccessibleName = resources.GetString("passwordLabel1.AccessibleName");
			this.passwordLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("passwordLabel1.Anchor")));
			this.passwordLabel1.AutoSize = ((bool)(resources.GetObject("passwordLabel1.AutoSize")));
			this.passwordLabel1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("passwordLabel1.Dock")));
			this.passwordLabel1.Enabled = ((bool)(resources.GetObject("passwordLabel1.Enabled")));
			this.passwordLabel1.Font = ((System.Drawing.Font)(resources.GetObject("passwordLabel1.Font")));
			this.passwordLabel1.Image = ((System.Drawing.Image)(resources.GetObject("passwordLabel1.Image")));
			this.passwordLabel1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("passwordLabel1.ImageAlign")));
			this.passwordLabel1.ImageIndex = ((int)(resources.GetObject("passwordLabel1.ImageIndex")));
			this.passwordLabel1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("passwordLabel1.ImeMode")));
			this.passwordLabel1.Location = ((System.Drawing.Point)(resources.GetObject("passwordLabel1.Location")));
			this.passwordLabel1.Name = "passwordLabel1";
			this.passwordLabel1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("passwordLabel1.RightToLeft")));
			this.passwordLabel1.Size = ((System.Drawing.Size)(resources.GetObject("passwordLabel1.Size")));
			this.passwordLabel1.TabIndex = ((int)(resources.GetObject("passwordLabel1.TabIndex")));
			this.passwordLabel1.Text = resources.GetString("passwordLabel1.Text");
			this.passwordLabel1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("passwordLabel1.TextAlign")));
			this.passwordLabel1.Visible = ((bool)(resources.GetObject("passwordLabel1.Visible")));
			// 
			// banner
			// 
			this.banner.AccessibleDescription = resources.GetString("banner.AccessibleDescription");
			this.banner.AccessibleName = resources.GetString("banner.AccessibleName");
			this.banner.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("banner.Anchor")));
			this.banner.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("banner.BackgroundImage")));
			this.banner.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("banner.Dock")));
			this.banner.Enabled = ((bool)(resources.GetObject("banner.Enabled")));
			this.banner.Font = ((System.Drawing.Font)(resources.GetObject("banner.Font")));
			this.banner.Image = ((System.Drawing.Image)(resources.GetObject("banner.Image")));
			this.banner.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("banner.ImeMode")));
			this.banner.Location = ((System.Drawing.Point)(resources.GetObject("banner.Location")));
			this.banner.Name = "banner";
			this.banner.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("banner.RightToLeft")));
			this.banner.Size = ((System.Drawing.Size)(resources.GetObject("banner.Size")));
			this.banner.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("banner.SizeMode")));
			this.banner.TabIndex = ((int)(resources.GetObject("banner.TabIndex")));
			this.banner.TabStop = false;
			this.banner.Text = resources.GetString("banner.Text");
			this.banner.Visible = ((bool)(resources.GetObject("banner.Visible")));
			// 
			// serverLabel2
			// 
			this.serverLabel2.AccessibleDescription = resources.GetString("serverLabel2.AccessibleDescription");
			this.serverLabel2.AccessibleName = resources.GetString("serverLabel2.AccessibleName");
			this.serverLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("serverLabel2.Anchor")));
			this.serverLabel2.AutoSize = ((bool)(resources.GetObject("serverLabel2.AutoSize")));
			this.serverLabel2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("serverLabel2.Dock")));
			this.serverLabel2.Enabled = ((bool)(resources.GetObject("serverLabel2.Enabled")));
			this.serverLabel2.Font = ((System.Drawing.Font)(resources.GetObject("serverLabel2.Font")));
			this.serverLabel2.Image = ((System.Drawing.Image)(resources.GetObject("serverLabel2.Image")));
			this.serverLabel2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("serverLabel2.ImageAlign")));
			this.serverLabel2.ImageIndex = ((int)(resources.GetObject("serverLabel2.ImageIndex")));
			this.serverLabel2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("serverLabel2.ImeMode")));
			this.serverLabel2.Location = ((System.Drawing.Point)(resources.GetObject("serverLabel2.Location")));
			this.serverLabel2.Name = "serverLabel2";
			this.serverLabel2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("serverLabel2.RightToLeft")));
			this.serverLabel2.Size = ((System.Drawing.Size)(resources.GetObject("serverLabel2.Size")));
			this.serverLabel2.TabIndex = ((int)(resources.GetObject("serverLabel2.TabIndex")));
			this.serverLabel2.Text = resources.GetString("serverLabel2.Text");
			this.serverLabel2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("serverLabel2.TextAlign")));
			this.serverLabel2.Visible = ((bool)(resources.GetObject("serverLabel2.Visible")));
			// 
			// userLabel2
			// 
			this.userLabel2.AccessibleDescription = resources.GetString("userLabel2.AccessibleDescription");
			this.userLabel2.AccessibleName = resources.GetString("userLabel2.AccessibleName");
			this.userLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("userLabel2.Anchor")));
			this.userLabel2.AutoSize = ((bool)(resources.GetObject("userLabel2.AutoSize")));
			this.userLabel2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("userLabel2.Dock")));
			this.userLabel2.Enabled = ((bool)(resources.GetObject("userLabel2.Enabled")));
			this.userLabel2.Font = ((System.Drawing.Font)(resources.GetObject("userLabel2.Font")));
			this.userLabel2.Image = ((System.Drawing.Image)(resources.GetObject("userLabel2.Image")));
			this.userLabel2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("userLabel2.ImageAlign")));
			this.userLabel2.ImageIndex = ((int)(resources.GetObject("userLabel2.ImageIndex")));
			this.userLabel2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("userLabel2.ImeMode")));
			this.userLabel2.Location = ((System.Drawing.Point)(resources.GetObject("userLabel2.Location")));
			this.userLabel2.Name = "userLabel2";
			this.userLabel2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("userLabel2.RightToLeft")));
			this.userLabel2.Size = ((System.Drawing.Size)(resources.GetObject("userLabel2.Size")));
			this.userLabel2.TabIndex = ((int)(resources.GetObject("userLabel2.TabIndex")));
			this.userLabel2.Text = resources.GetString("userLabel2.Text");
			this.userLabel2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("userLabel2.TextAlign")));
			this.userLabel2.Visible = ((bool)(resources.GetObject("userLabel2.Visible")));
			// 
			// passwordLabel2
			// 
			this.passwordLabel2.AccessibleDescription = resources.GetString("passwordLabel2.AccessibleDescription");
			this.passwordLabel2.AccessibleName = resources.GetString("passwordLabel2.AccessibleName");
			this.passwordLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("passwordLabel2.Anchor")));
			this.passwordLabel2.AutoSize = ((bool)(resources.GetObject("passwordLabel2.AutoSize")));
			this.passwordLabel2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("passwordLabel2.Dock")));
			this.passwordLabel2.Enabled = ((bool)(resources.GetObject("passwordLabel2.Enabled")));
			this.passwordLabel2.Font = ((System.Drawing.Font)(resources.GetObject("passwordLabel2.Font")));
			this.passwordLabel2.Image = ((System.Drawing.Image)(resources.GetObject("passwordLabel2.Image")));
			this.passwordLabel2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("passwordLabel2.ImageAlign")));
			this.passwordLabel2.ImageIndex = ((int)(resources.GetObject("passwordLabel2.ImageIndex")));
			this.passwordLabel2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("passwordLabel2.ImeMode")));
			this.passwordLabel2.Location = ((System.Drawing.Point)(resources.GetObject("passwordLabel2.Location")));
			this.passwordLabel2.Name = "passwordLabel2";
			this.passwordLabel2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("passwordLabel2.RightToLeft")));
			this.passwordLabel2.Size = ((System.Drawing.Size)(resources.GetObject("passwordLabel2.Size")));
			this.passwordLabel2.TabIndex = ((int)(resources.GetObject("passwordLabel2.TabIndex")));
			this.passwordLabel2.Text = resources.GetString("passwordLabel2.Text");
			this.passwordLabel2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("passwordLabel2.TextAlign")));
			this.passwordLabel2.Visible = ((bool)(resources.GetObject("passwordLabel2.Visible")));
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
			this.serverName.Visible = ((bool)(resources.GetObject("serverName.Visible")));
			// 
			// userName2
			// 
			this.userName2.AccessibleDescription = resources.GetString("userName2.AccessibleDescription");
			this.userName2.AccessibleName = resources.GetString("userName2.AccessibleName");
			this.userName2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("userName2.Anchor")));
			this.userName2.AutoSize = ((bool)(resources.GetObject("userName2.AutoSize")));
			this.userName2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("userName2.Dock")));
			this.userName2.Enabled = ((bool)(resources.GetObject("userName2.Enabled")));
			this.userName2.Font = ((System.Drawing.Font)(resources.GetObject("userName2.Font")));
			this.userName2.Image = ((System.Drawing.Image)(resources.GetObject("userName2.Image")));
			this.userName2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("userName2.ImageAlign")));
			this.userName2.ImageIndex = ((int)(resources.GetObject("userName2.ImageIndex")));
			this.userName2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("userName2.ImeMode")));
			this.userName2.Location = ((System.Drawing.Point)(resources.GetObject("userName2.Location")));
			this.userName2.Name = "userName2";
			this.userName2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("userName2.RightToLeft")));
			this.userName2.Size = ((System.Drawing.Size)(resources.GetObject("userName2.Size")));
			this.userName2.TabIndex = ((int)(resources.GetObject("userName2.TabIndex")));
			this.userName2.Text = resources.GetString("userName2.Text");
			this.userName2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("userName2.TextAlign")));
			this.userName2.Visible = ((bool)(resources.GetObject("userName2.Visible")));
			// 
			// ServerInfo
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
			this.Controls.Add(this.password);
			this.Controls.Add(this.userName2);
			this.Controls.Add(this.userName);
			this.Controls.Add(this.serverName);
			this.Controls.Add(this.serverIP);
			this.Controls.Add(this.serverLabel1);
			this.Controls.Add(this.passwordLabel2);
			this.Controls.Add(this.passwordLabel1);
			this.Controls.Add(this.userLabel2);
			this.Controls.Add(this.userLabel1);
			this.Controls.Add(this.serverLabel2);
			this.Controls.Add(this.banner);
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
			this.Name = "ServerInfo";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.ServerInfo_Load);
			this.Activated += new System.EventHandler(this.ServerInfo_Activated);
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets a value that indicates if the dialog was dismissed with the Cancel button.
		/// </summary>
		public bool Cancelled
		{
			get { return cancelled; }
		}

		/// <summary>
		/// Gets a value that indicates if a client update has started.
		/// </summary>
		public bool UpdateStarted
		{
			get { return updateStarted; }
		}

		/// <summary>
		/// Gets the iFolderSettings for the default domain.
		/// </summary>
		public iFolderSettings ifSettings
		{
			get { return ifolderSettings; }
		}
		#endregion

		#region Events
		/// <summary>
		/// Delegate used when successfully connected to Enterprise Server.
		/// </summary>
		public delegate void EnterpriseConnectDelegate(object sender, EventArgs e);
		/// <summary>
		/// Occurs when all successfully connected to enterprise.
		/// </summary>
		public event EnterpriseConnectDelegate EnterpriseConnect;
		#endregion

		#region Event Handlers
		private void cancel_Click(object sender, System.EventArgs e)
		{
			cancelled = true;
			Close();
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			if (initialLogin)
			{
				try
				{
					if (ifWebService != null)
					{
						ifWebService.ConnectToEnterpriseServer(userName.Text, password.Text, serverIP.Text);

						try
						{
							ifolderSettings = ifWebService.GetSettings();
							domainID = ifolderSettings.DefaultDomainID;
						}
						catch {}

						if (EnterpriseConnect != null)
						{
							EnterpriseConnect(this, new EventArgs());
						}

						try
						{
							checkForClientUpdate();
						}
						catch (Exception ex)
						{
							MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("checkUpdateError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
							mmb.ShowDialog();
						}

						password.Clear();
						Close();
					}
				}
				catch (Exception ex)
				{
					if (ex.Message.IndexOf("HTTP status 401") != -1)
					{
						MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("failedAuth"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
					}
					else
					{
						MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("serverConnectError"), resourceManager.GetString("serverConnectErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
					}
				}
			}
			else
			{
				try
				{
					DomainAuthentication domainAuth = new DomainAuthentication(domainID, password.Text);
					AuthenticationStatus authStatus = domainAuth.Authenticate();
					MyMessageBox mmb;
					switch (authStatus)
					{
						case AuthenticationStatus.Success:
							try
							{
								checkForClientUpdate();
							}
							catch (Exception ex)
							{
								mmb = new MyMessageBox(resourceManager.GetString("checkUpdateError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
								mmb.ShowDialog();
							}

							password.Clear();
							Close();
							break;
						case AuthenticationStatus.InvalidCredentials:
							mmb = new MyMessageBox(resourceManager.GetString("badPassword"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
							mmb.ShowDialog();
							break;
						default:
							mmb = new MyMessageBox(string.Format(resourceManager.GetString("serverReconnectError"), authStatus), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
							mmb.ShowDialog();
							break;
					}
				}
				catch (Exception ex)
				{
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("serverConnectError"), resourceManager.GetString("serverConnectErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}
			}

			password.Focus();
			if (!password.Text.Equals(string.Empty))
			{
				password.SelectAll();
			}
			Cursor.Current = Cursors.Default;
		}

		private void ServerInfo_Load(object sender, System.EventArgs e)
		{
			// Load the application icon.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_loaded.ico"));
				this.banner.Image = Image.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-banner.png"));
			}
			catch
			{
				// Ignore.
			}

			try
			{
				SimiasWebService simiasWebService = new SimiasWebService();
				simiasWebService.Url = Simias.Client.Manager.LocalServiceUrl.ToString() + "/Simias.asmx";

				DomainInformation domainInfo = simiasWebService.GetDomainInformation(domainID);

				if (domainInfo != null)
				{
					serverName.Text = domainInfo.Name;
					userName2.Text = domainInfo.MemberName;
				}
			}
			catch (Exception ex)
			{
				Novell.iFolderCom.MyMessageBox mmb = new Novell.iFolderCom.MyMessageBox(resourceManager.GetString("domainInfoReadError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}
		}

		private void ServerInfo_Activated(object sender, System.EventArgs e)
		{
			userName.Focus();
		}

		private void userName_TextChanged(object sender, System.EventArgs e)
		{
			ok.Enabled = !userName.Text.Equals(string.Empty) && !serverIP.Text.Equals(string.Empty);
		}

		private void serverIP_TextChanged(object sender, System.EventArgs e)
		{
			ok.Enabled = !userName.Text.Equals(string.Empty) && !serverIP.Text.Equals(string.Empty);
		}
		#endregion

		#region Private Methods
		private void checkForClientUpdate()
		{
			ClientUpgrade cUpgrade = new ClientUpgrade(domainID, userName.Text.Equals(string.Empty) ? userName2.Text : userName.Text, password.Text);
			string version = cUpgrade.CheckForUpdate();
			if ( version != null )
			{
				// Pop up a dialog here and ask if the user wants to update the client.
				MyMessageBox mmb = new MyMessageBox(string.Format(resourceManager.GetString("clientUpgradePrompt"), version), resourceManager.GetString("clientUpgradeTitle"), string.Empty, MyMessageBoxButtons.YesNo, MyMessageBoxIcon.Question);
				DialogResult result = mmb.ShowDialog();
				if ( result == DialogResult.Yes )
				{
					updateStarted = cUpgrade.RunUpdate();
					if ( updateStarted == false )
					{
						mmb = new MyMessageBox(resourceManager.GetString("clientUpgradeFailure"), resourceManager.GetString("clientUpgradeTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
						mmb.ShowDialog();
					}
				}
			}
		}
		#endregion
	}
}
