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

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for AddAccount.
	/// </summary>
	public class AddAccount : System.Windows.Forms.Form
	{
		private DomainWeb domain;
		private iFolderWebService ifWebService;
		private bool updateStarted;
		private bool successful;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox host;
		private System.Windows.Forms.TextBox userName;
		private System.Windows.Forms.TextBox password;
		private System.Windows.Forms.CheckBox rememberPassword;
		private System.Windows.Forms.CheckBox autoLogin;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Button ok;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AddAccount(iFolderWebService ifolderWebService)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AddAccount));
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.host = new System.Windows.Forms.TextBox();
			this.userName = new System.Windows.Forms.TextBox();
			this.password = new System.Windows.Forms.TextBox();
			this.rememberPassword = new System.Windows.Forms.CheckBox();
			this.autoLogin = new System.Windows.Forms.CheckBox();
			this.cancel = new System.Windows.Forms.Button();
			this.ok = new System.Windows.Forms.Button();
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
			this.label3.Visible = ((bool)(resources.GetObject("label3.Visible")));
			// 
			// host
			// 
			this.host.AccessibleDescription = resources.GetString("host.AccessibleDescription");
			this.host.AccessibleName = resources.GetString("host.AccessibleName");
			this.host.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("host.Anchor")));
			this.host.AutoSize = ((bool)(resources.GetObject("host.AutoSize")));
			this.host.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("host.BackgroundImage")));
			this.host.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("host.Dock")));
			this.host.Enabled = ((bool)(resources.GetObject("host.Enabled")));
			this.host.Font = ((System.Drawing.Font)(resources.GetObject("host.Font")));
			this.host.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("host.ImeMode")));
			this.host.Location = ((System.Drawing.Point)(resources.GetObject("host.Location")));
			this.host.MaxLength = ((int)(resources.GetObject("host.MaxLength")));
			this.host.Multiline = ((bool)(resources.GetObject("host.Multiline")));
			this.host.Name = "host";
			this.host.PasswordChar = ((char)(resources.GetObject("host.PasswordChar")));
			this.host.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("host.RightToLeft")));
			this.host.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("host.ScrollBars")));
			this.host.Size = ((System.Drawing.Size)(resources.GetObject("host.Size")));
			this.host.TabIndex = ((int)(resources.GetObject("host.TabIndex")));
			this.host.Text = resources.GetString("host.Text");
			this.host.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("host.TextAlign")));
			this.host.Visible = ((bool)(resources.GetObject("host.Visible")));
			this.host.WordWrap = ((bool)(resources.GetObject("host.WordWrap")));
			this.host.TextChanged += new System.EventHandler(this.host_TextChanged);
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
			this.userName.TextChanged += new System.EventHandler(this.host_TextChanged);
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
			// rememberPassword
			// 
			this.rememberPassword.AccessibleDescription = resources.GetString("rememberPassword.AccessibleDescription");
			this.rememberPassword.AccessibleName = resources.GetString("rememberPassword.AccessibleName");
			this.rememberPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("rememberPassword.Anchor")));
			this.rememberPassword.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("rememberPassword.Appearance")));
			this.rememberPassword.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("rememberPassword.BackgroundImage")));
			this.rememberPassword.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rememberPassword.CheckAlign")));
			this.rememberPassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("rememberPassword.Dock")));
			this.rememberPassword.Enabled = ((bool)(resources.GetObject("rememberPassword.Enabled")));
			this.rememberPassword.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("rememberPassword.FlatStyle")));
			this.rememberPassword.Font = ((System.Drawing.Font)(resources.GetObject("rememberPassword.Font")));
			this.rememberPassword.Image = ((System.Drawing.Image)(resources.GetObject("rememberPassword.Image")));
			this.rememberPassword.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rememberPassword.ImageAlign")));
			this.rememberPassword.ImageIndex = ((int)(resources.GetObject("rememberPassword.ImageIndex")));
			this.rememberPassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("rememberPassword.ImeMode")));
			this.rememberPassword.Location = ((System.Drawing.Point)(resources.GetObject("rememberPassword.Location")));
			this.rememberPassword.Name = "rememberPassword";
			this.rememberPassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("rememberPassword.RightToLeft")));
			this.rememberPassword.Size = ((System.Drawing.Size)(resources.GetObject("rememberPassword.Size")));
			this.rememberPassword.TabIndex = ((int)(resources.GetObject("rememberPassword.TabIndex")));
			this.rememberPassword.Text = resources.GetString("rememberPassword.Text");
			this.rememberPassword.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rememberPassword.TextAlign")));
			this.rememberPassword.Visible = ((bool)(resources.GetObject("rememberPassword.Visible")));
			// 
			// autoLogin
			// 
			this.autoLogin.AccessibleDescription = resources.GetString("autoLogin.AccessibleDescription");
			this.autoLogin.AccessibleName = resources.GetString("autoLogin.AccessibleName");
			this.autoLogin.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("autoLogin.Anchor")));
			this.autoLogin.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("autoLogin.Appearance")));
			this.autoLogin.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("autoLogin.BackgroundImage")));
			this.autoLogin.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoLogin.CheckAlign")));
			this.autoLogin.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("autoLogin.Dock")));
			this.autoLogin.Enabled = ((bool)(resources.GetObject("autoLogin.Enabled")));
			this.autoLogin.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("autoLogin.FlatStyle")));
			this.autoLogin.Font = ((System.Drawing.Font)(resources.GetObject("autoLogin.Font")));
			this.autoLogin.Image = ((System.Drawing.Image)(resources.GetObject("autoLogin.Image")));
			this.autoLogin.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoLogin.ImageAlign")));
			this.autoLogin.ImageIndex = ((int)(resources.GetObject("autoLogin.ImageIndex")));
			this.autoLogin.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("autoLogin.ImeMode")));
			this.autoLogin.Location = ((System.Drawing.Point)(resources.GetObject("autoLogin.Location")));
			this.autoLogin.Name = "autoLogin";
			this.autoLogin.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("autoLogin.RightToLeft")));
			this.autoLogin.Size = ((System.Drawing.Size)(resources.GetObject("autoLogin.Size")));
			this.autoLogin.TabIndex = ((int)(resources.GetObject("autoLogin.TabIndex")));
			this.autoLogin.Text = resources.GetString("autoLogin.Text");
			this.autoLogin.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoLogin.TextAlign")));
			this.autoLogin.Visible = ((bool)(resources.GetObject("autoLogin.Visible")));
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
			// AddAccount
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
			this.Controls.Add(this.ok);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.autoLogin);
			this.Controls.Add(this.rememberPassword);
			this.Controls.Add(this.password);
			this.Controls.Add(this.userName);
			this.Controls.Add(this.host);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
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
			this.Name = "AddAccount";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Closing += new System.ComponentModel.CancelEventHandler(this.AddAccount_Closing);
			this.Load += new System.EventHandler(this.AddAccount_Load);
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		public DomainWeb Domain
		{
			get { return domain; }
		}

		public bool UpdateStarted
		{
			get { return updateStarted; }
		}
		#endregion

		#region Events
		/// <summary>
		/// Delegate used when successfully connected to Enterprise Server.
		/// </summary>
		public delegate void EnterpriseConnectDelegate(object sender, DomainConnectEventArgs e);
		/// <summary>
		/// Occurs when successfully connected to enterprise.
		/// </summary>
		public event EnterpriseConnectDelegate EnterpriseConnect;
		#endregion

		#region Event handlers
		private void AddAccount_Load(object sender, System.EventArgs e)
		{
			// Load the application icon.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_loaded.ico"));
			}
			catch
			{
				// Ignore.
			}
		}

		private void AddAccount_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.DialogResult.Equals(DialogResult.OK) && !successful)
			{
				e.Cancel = true;
			}
		}

		private void cancel_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			try
			{
				successful = false;
				DomainWeb domain = ifWebService.ConnectToDomain(userName.Text, password.Text, host.Text);

				if (domain != null)
				{
					successful = true;
				}


				if (EnterpriseConnect != null)
				{
					EnterpriseConnect(this, new DomainConnectEventArgs(domain));
				}

				// TODO: save state of checkboxes.

				try
				{
					updateStarted = FormsTrayApp.CheckForClientUpdate(domain.ID, userName.Text, password.Text);
				}
				catch (Exception ex)
				{
					/* TODO:
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("checkUpdateError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
					mmb.ShowDialog();
					*/
				}

				password.Clear();
				Close();
			}
			catch (Exception ex)
			{
				/* TODO:
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
				*/
			}
		}

		private void host_TextChanged(object sender, System.EventArgs e)
		{
			ok.Enabled = !userName.Text.Equals(string.Empty) && !host.Text.Equals(string.Empty);
		}
		#endregion
	}
}
