/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004-2006 Novell, Inc.
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Novell.iFolderCom;
using Novell.FormsTrayApp;
using Simias.Client;
using Simias.Client.Authentication;
using Simias.Client.Event;

namespace Novell.Wizard
{

	/// <summary>
	/// Class for the wizard page where the iFolder location is selected.
	/// </summary>
	public class VerifyPage : Novell.Wizard.InteriorPageTemplate
	{
		#region Class Members

		private System.Windows.Forms.Label label1;
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label serverAddress;
		private System.Windows.Forms.Label username;
		private System.Windows.Forms.Label rememberPassword;
		private System.Windows.Forms.Label defaultAccount;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label defaultDescription;
		private AccountWizard wizard;
		private static System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(Novell.FormsTrayApp.FormsTrayApp));

		#endregion

		#region Constructor

		/// <summary>
		/// Constructs a VerifyPage object.
		/// </summary>
		public VerifyPage( bool hideDefaultAccount )
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			defaultDescription.Visible = defaultAccount.Visible = !hideDefaultAccount;
		}

		#endregion

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.defaultDescription = new System.Windows.Forms.Label();
			this.serverAddress = new System.Windows.Forms.Label();
			this.username = new System.Windows.Forms.Label();
			this.rememberPassword = new System.Windows.Forms.Label();
			this.defaultAccount = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(50, 128);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(94, 16);
			this.label1.TabIndex = 3;
			this.label1.Text = Resource.GetString("UName")+":";//"User name:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(40, 80);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(440, 16);
			this.label2.TabIndex = 0;
			this.label2.Text = Resource.GetString("AccountVerify");//"Please verify that the information you have entered is correct.";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(50, 104);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(88, 16);
			this.label3.TabIndex = 1;
			this.label3.Text = Resource.GetString("ServerNameText");//"Server address:";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(50, 152);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(148, 16);
			this.label4.TabIndex = 5;
			this.label4.Text = Resource.GetString("RememberPasswd")+":";//"Remember password:";
			// 
			// defaultDescription
			// 
			this.defaultDescription.Location = new System.Drawing.Point(50, 176);
			this.defaultDescription.Name = "defaultDescription";
			this.defaultDescription.Size = new System.Drawing.Size(150, 16);
			this.defaultDescription.TabIndex = 7;
			this.defaultDescription.Text = Resource.GetString("DefaultServer");//"Make default account:";
			// 
			// serverAddress
			// 
			this.serverAddress.Location = new System.Drawing.Point(238, 104);
			this.serverAddress.Name = "serverAddress";
			this.serverAddress.Size = new System.Drawing.Size(304, 16);
			this.serverAddress.TabIndex = 2;
			// 
			// username
			// 
			this.username.Location = new System.Drawing.Point(238, 128);
			this.username.Name = "username";
			this.username.Size = new System.Drawing.Size(304, 16);
			this.username.TabIndex = 4;
			// 
			// rememberPassword
			// 
			this.rememberPassword.Location = new System.Drawing.Point(238, 152);
			this.rememberPassword.Name = "rememberPassword";
			this.rememberPassword.Size = new System.Drawing.Size(168, 16);
			this.rememberPassword.TabIndex = 6;
			// 
			// defaultAccount
			// 
			this.defaultAccount.Location = new System.Drawing.Point(238, 176);
			this.defaultAccount.Name = "defaultAccount";
			this.defaultAccount.Size = new System.Drawing.Size(168, 16);
			this.defaultAccount.TabIndex = 8;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(40, 256);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(432, 16);
			this.label6.TabIndex = 9;
			this.label6.Text = Resource.GetString("ClickConnect");//"Click Connect to validate your connection with the server.";
			// 
			// VerifyPage
			// 
			this.Controls.Add(this.label6);
			this.Controls.Add(this.defaultAccount);
			this.Controls.Add(this.rememberPassword);
			this.Controls.Add(this.username);
			this.Controls.Add(this.serverAddress);
			this.Controls.Add(this.defaultDescription);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.HeaderSubTitle = "";
			this.HeaderTitle = "";
			this.Name = "VerifyPage";
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.label3, 0);
			this.Controls.SetChildIndex(this.label4, 0);
			this.Controls.SetChildIndex(this.defaultDescription, 0);
			this.Controls.SetChildIndex(this.serverAddress, 0);
			this.Controls.SetChildIndex(this.username, 0);
			this.Controls.SetChildIndex(this.rememberPassword, 0);
			this.Controls.SetChildIndex(this.defaultAccount, 0);
			this.Controls.SetChildIndex(this.label6, 0);
			this.ResumeLayout(false);

		}
		#endregion

		#region Overridden Methods

		internal override void ActivatePage(int previousIndex)
		{
			base.ActivatePage (previousIndex);

			wizard = (AccountWizard)this.Parent;
			serverAddress.Text = wizard.ServerPage.ServerAddress;
			username.Text = wizard.IdentityPage.Username;
			rememberPassword.Text = wizard.IdentityPage.RememberPassword ? Resource.GetString("YesText") : Resource.GetString("NoText");
			defaultAccount.Text = wizard.ServerPage.DefaultServer ? Resource.GetString("YesText") : Resource.GetString("NoText");
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		internal override int ValidatePage(int currentIndex)
		{
			if ( !wizard.ConnectToServer() )
			{
				return currentIndex;
			}

			return base.ValidatePage (currentIndex);
		}

		#endregion
	}
}

