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

namespace Novell.Wizard
{
	/// <summary>
	/// Class for the wizard page where the invitation is accepted or declined.
	/// </summary>
	public class IdentityPage : Novell.Wizard.InteriorPageTemplate
	{
		#region Class Members
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox username;
		private System.Windows.Forms.TextBox password;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckBox rememberPassword;
		private System.ComponentModel.IContainer components = null;
		#endregion

		/// <summary>
		/// Constructs an IdentityPage object.
		/// </summary>
		public IdentityPage()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.username = new System.Windows.Forms.TextBox();
			this.password = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.rememberPassword = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(40, 96);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(416, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Enter your iFolder username and password (for example, jsmith).";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(50, 122);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(62, 14);
			this.label3.TabIndex = 1;
			this.label3.Text = "&Username:";
			// 
			// username
			// 
			this.username.Location = new System.Drawing.Point(112, 120);
			this.username.Name = "username";
			this.username.Size = new System.Drawing.Size(344, 20);
			this.username.TabIndex = 2;
			this.username.Text = "";
			this.username.TextChanged += new System.EventHandler(this.username_TextChanged);
			// 
			// password
			// 
			this.password.Location = new System.Drawing.Point(112, 144);
			this.password.Name = "password";
			this.password.PasswordChar = '*';
			this.password.Size = new System.Drawing.Size(344, 20);
			this.password.TabIndex = 4;
			this.password.Text = "";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(50, 146);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(64, 16);
			this.label4.TabIndex = 3;
			this.label4.Text = "&Password:";
			// 
			// rememberPassword
			// 
			this.rememberPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.rememberPassword.Location = new System.Drawing.Point(112, 176);
			this.rememberPassword.Name = "rememberPassword";
			this.rememberPassword.Size = new System.Drawing.Size(344, 24);
			this.rememberPassword.TabIndex = 5;
			this.rememberPassword.Text = "&Remember my password";
			// 
			// IdentityPage
			// 
			this.Controls.Add(this.rememberPassword);
			this.Controls.Add(this.password);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.username);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label1);
			this.HeaderSubTitle = "HeaderSubTitle";
			this.HeaderTitle = "HeaderTitle";
			this.Name = "IdentityPage";
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label3, 0);
			this.Controls.SetChildIndex(this.username, 0);
			this.Controls.SetChildIndex(this.label4, 0);
			this.Controls.SetChildIndex(this.password, 0);
			this.Controls.SetChildIndex(this.rememberPassword, 0);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers

		private void username_TextChanged(object sender, System.EventArgs e)
		{
			// Enable the buttons.
			if (username.Text != "")
			{
				((AccountWizard)this.Parent).WizardButtons = WizardButtons.Next | WizardButtons.Back | WizardButtons.Cancel;
			}
			else
			{
				((AccountWizard)this.Parent).WizardButtons = WizardButtons.Back | WizardButtons.Cancel;
			}
		}

		#endregion

		#region Overridden Methods

		internal override void ActivatePage(int previousIndex)
		{
			base.ActivatePage (previousIndex);

			// TODO:

			// Enable the buttons
			((AccountWizard)(this.Parent)).WizardButtons = WizardButtons.Next | WizardButtons.Back | WizardButtons.Cancel;
			username.Focus();
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
			// TODO:
			AccountWizard wiz = (AccountWizard)this.Parent;

			return base.ValidatePage (currentIndex);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the value of the password entered.
		/// </summary>
		public string Password
		{
			get
			{
				return password.Text;
			}
		}

		/// <summary>
		/// Gets a value indicating if the remember password option is checked.
		/// </summary>
		public bool RememberPassword
		{
			get
			{
				return rememberPassword.Checked;
			}
		}

		/// <summary>
		/// Gets the value of the username entered.
		/// </summary>
		public string Username
		{
			get
			{
				return username.Text;
			}
		}

		#endregion
	}
}

