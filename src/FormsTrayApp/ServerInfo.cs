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
using Simias;
using Simias.Domain;

namespace Novell.iFolder.FormsTrayApp
{
	/// <summary>
	/// Summary description for ServerInfo.
	/// </summary>
	public class ServerInfo : System.Windows.Forms.Form
	{
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(ServerInfo));
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.TextBox serverIP;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox password;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox userName;
		private Configuration config;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ServerInfo(Configuration config)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			this.config = config;
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
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.userName = new System.Windows.Forms.TextBox();
			this.serverIP = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.password = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ok.Location = new System.Drawing.Point(224, 160);
			this.ok.Name = "ok";
			this.ok.TabIndex = 7;
			this.ok.Text = "OK";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancel.Location = new System.Drawing.Point(304, 160);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 8;
			this.cancel.Text = "Cancel";
			// 
			// userName
			// 
			this.userName.Location = new System.Drawing.Point(120, 64);
			this.userName.Name = "userName";
			this.userName.Size = new System.Drawing.Size(240, 20);
			this.userName.TabIndex = 2;
			this.userName.Text = "";
			// 
			// serverIP
			// 
			this.serverIP.Location = new System.Drawing.Point(120, 128);
			this.serverIP.Name = "serverIP";
			this.serverIP.Size = new System.Drawing.Size(240, 20);
			this.serverIP.TabIndex = 6;
			this.serverIP.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 64);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "User name:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(24, 128);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 16);
			this.label2.TabIndex = 5;
			this.label2.Text = "Server IP address:";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 16);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(360, 32);
			this.label3.TabIndex = 0;
			this.label3.Text = "Please enter your credentials and the enterprise server\'s IP address.";
			// 
			// password
			// 
			this.password.Location = new System.Drawing.Point(120, 96);
			this.password.Name = "password";
			this.password.PasswordChar = '*';
			this.password.Size = new System.Drawing.Size(240, 20);
			this.password.TabIndex = 4;
			this.password.Text = "";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(24, 96);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(100, 16);
			this.label4.TabIndex = 3;
			this.label4.Text = "Password:";
			// 
			// ServerInfo
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(386, 192);
			this.Controls.Add(this.password);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.serverIP);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.userName);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ServerInfo";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Enterprise Server Information";
			this.Load += new System.EventHandler(this.ServerInfo_Load);
			this.Activated += new System.EventHandler(this.ServerInfo_Activated);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers
		private void ok_Click(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			// TODO: call method to connect to server.
			DomainAgent da = new DomainAgent(config);
			da.Attach(serverIP.Text, userName.Text, password.Text);

			Cursor.Current = Cursors.Default;
		}

		private void ServerInfo_Load(object sender, System.EventArgs e)
		{
			// Load the application icon.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_loaded.ico"));
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Loading graphics");
			}
		}

		private void ServerInfo_Activated(object sender, System.EventArgs e)
		{
			userName.Focus();
		}
		#endregion
	}
}
