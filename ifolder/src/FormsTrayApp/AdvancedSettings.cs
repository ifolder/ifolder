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

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for AdvancedSettings.
	/// </summary>
	public class AdvancedSettings : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.TextBox proxy;
		private System.Windows.Forms.NumericUpDown port;
		private System.Windows.Forms.CheckBox useProxy;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AdvancedSettings()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Set the min/max values for port.
			port.Minimum = IPEndPoint.MinPort;
			port.Maximum = IPEndPoint.MaxPort;
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
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.proxy = new System.Windows.Forms.TextBox();
			this.port = new System.Windows.Forms.NumericUpDown();
			this.useProxy = new System.Windows.Forms.CheckBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.groupBox5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.port)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBox5
			// 
			this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox5.Controls.Add(this.proxy);
			this.groupBox5.Controls.Add(this.port);
			this.groupBox5.Controls.Add(this.useProxy);
			this.groupBox5.Controls.Add(this.label7);
			this.groupBox5.Controls.Add(this.label4);
			this.groupBox5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox5.Location = new System.Drawing.Point(8, 8);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(463, 88);
			this.groupBox5.TabIndex = 3;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Proxy";
			// 
			// proxy
			// 
			this.proxy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.proxy.Enabled = false;
			this.proxy.Location = new System.Drawing.Point(88, 56);
			this.proxy.Name = "proxy";
			this.proxy.Size = new System.Drawing.Size(233, 20);
			this.proxy.TabIndex = 2;
			this.proxy.Text = "";
			// 
			// port
			// 
			this.port.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.port.Enabled = false;
			this.port.Location = new System.Drawing.Point(377, 56);
			this.port.Name = "port";
			this.port.Size = new System.Drawing.Size(72, 20);
			this.port.TabIndex = 4;
			// 
			// useProxy
			// 
			this.useProxy.Enabled = false;
			this.useProxy.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.useProxy.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.useProxy.Location = new System.Drawing.Point(16, 24);
			this.useProxy.Name = "useProxy";
			this.useProxy.Size = new System.Drawing.Size(360, 16);
			this.useProxy.TabIndex = 0;
			this.useProxy.Text = "&Use this proxy server to sync iFolders with the host";
			// 
			// label7
			// 
			this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label7.Location = new System.Drawing.Point(345, 58);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(56, 16);
			this.label7.TabIndex = 3;
			this.label7.Text = "Por&t:";
			// 
			// label4
			// 
			this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label4.Location = new System.Drawing.Point(16, 58);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(100, 16);
			this.label4.TabIndex = 1;
			this.label4.Text = "&Proxy host:";
			// 
			// ok
			// 
			this.ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ok.Location = new System.Drawing.Point(317, 112);
			this.ok.Name = "ok";
			this.ok.TabIndex = 4;
			this.ok.Text = "OK";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancel.Location = new System.Drawing.Point(396, 112);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 5;
			this.cancel.Text = "Cancel";
			// 
			// AdvancedSettings
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(480, 142);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.groupBox5);
			this.Name = "AdvancedSettings";
			this.Text = "Advanced Settings";
			this.Load += new System.EventHandler(this.AdvancedSettings_Load);
			this.groupBox5.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.port)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void AdvancedSettings_Load(object sender, System.EventArgs e)
		{
/*			iFolderSettings ifSettings = null;

			try
			{
				ifSettings = ifWebService.GetSettings();

				// Update the proxy settings.
				useProxy.Checked = ifSettings.UseProxy;
				proxy.Text = ifSettings.ProxyHost;
				port.Value = (decimal)ifSettings.ProxyPort;
			}
			catch (Exception ex)
			{
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readProxyError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}*/
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

/*			try
			{
				iFolderSettings ifSettings = ifWebService.GetSettings();

				// Check and update proxy settings.
				if ((useProxy.Checked != ifSettings.UseProxy) ||
					!proxy.Text.Equals(ifSettings.ProxyHost) ||
					(port.Value != (decimal)ifSettings.ProxyPort))
				{
					try
					{
						if (useProxy.Checked)
						{
							ifWebService.SetupProxy(proxy.Text, (int)port.Value);
						}
						else
						{
							ifWebService.RemoveProxy();
						}
					}
					catch (Exception ex)
					{
						Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("saveProxyError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
					}
				}
			}
			catch (Exception ex)
			{
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readiFolderSettingsError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}*/

			Cursor.Current = Cursors.Default;
		}
	}
}
