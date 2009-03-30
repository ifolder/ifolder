/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Bruce Getter <bgetter@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Novell.FormsTrayApp;

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
		private System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(Novell.FormsTrayApp.FormsTrayApp));
        private int defaultTextXPos = 50;
        private int defaultTextYPos = 115;
        private int defaultSpacing = 16;
        private int maxTextWidth = 440;
        private int defaultTextWidth = 80;
        private SizeF strSize;

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
            this.strSize = new SizeF();
			this.SuspendLayout();
            Graphics graphics = CreateGraphics();

			// 
			// label1
			// 
            this.label1.Location = new System.Drawing.Point(this.defaultTextXPos - 10, this.defaultTextYPos);
            this.label1.Name = "label1";
            this.label1.TabIndex = 0;
            this.label1.Text = Resource.GetString("IdpgLbl1txt");//"Enter your iFolder username and password (for example, jsmith).";
            this.strSize = graphics.MeasureString(this.label1.Text, this.label1.Font);
            this.label1.Size = new System.Drawing.Size(this.maxTextWidth, ((int)this.strSize.Width / this.maxTextWidth + 1) * 16);
			// 
			// label3
			// 
            this.label3.Location = new System.Drawing.Point(this.defaultTextXPos, this.label1.Location.Y + this.label1.Size.Height + this.defaultSpacing);
            this.label3.Name = "label3";
            this.label3.TabIndex = 1;
            this.label3.Text = Resource.GetString("UName");//"&Username:";
            this.strSize = graphics.MeasureString(this.label3.Text, this.label3.Font);
            this.label3.Size = new System.Drawing.Size(this.defaultTextWidth, ((int)this.strSize.Width / this.defaultTextWidth + 1) * 16);			
			// 
			// username
			// 
            this.username.Location = new System.Drawing.Point(this.defaultTextXPos + this.defaultTextWidth, this.label3.Location.Y - 2);
            this.username.Name = "username";
            this.username.Size = new System.Drawing.Size(320, 20);
            this.username.TabIndex = 2;
            this.username.Text = "";
            this.username.TextChanged += new System.EventHandler(this.username_TextChanged);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(this.defaultTextXPos, this.label3.Location.Y + this.label3.Size.Height + this.defaultSpacing);
            this.label4.Name = "label4";
            this.label4.TabIndex = 3;
            this.label4.Text = Resource.GetString("PasswordTxt");//"&Password:";
            this.strSize = graphics.MeasureString(this.label4.Text, this.label4.Font);
            this.label4.Size = new System.Drawing.Size(this.defaultTextWidth, ((int)this.strSize.Width / this.defaultTextWidth + 1) * 16);
            // 
            // password
            // 
            this.password.Location = new System.Drawing.Point(this.defaultTextXPos + this.defaultTextWidth, this.label4.Location.Y - 2);
            this.password.Name = "password";
            this.password.PasswordChar = '*';
            this.password.Size = new System.Drawing.Size(320, 20);
            this.password.TabIndex = 4;
            this.password.Text = string.Empty;			
			// 
			// rememberPassword
			// 
            this.rememberPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rememberPassword.Location = new System.Drawing.Point(this.defaultTextXPos + this.defaultTextWidth, this.label4.Location.Y + this.label4.Size.Height + this.defaultSpacing);
            this.rememberPassword.Name = "rememberPassword";
            this.rememberPassword.TabIndex = 5;
            this.rememberPassword.Text = Resource.GetString("RememberPasswd");//"&Remember password";
            this.strSize = graphics.MeasureString(this.rememberPassword.Text, this.rememberPassword.Font);
            this.rememberPassword.Size = new System.Drawing.Size(300, ((int)(this.strSize.Width / 300) + 1) * 16);

            graphics.Dispose();
			// 
			// IdentityPage
			// 
			this.Controls.Add(this.rememberPassword);
			this.Controls.Add(this.password);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.username);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label1);
			this.HeaderTitle = "";
			this.Name = "IdentityPage";
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label3, 0);
			this.Controls.SetChildIndex(this.username, 0);
			this.Controls.SetChildIndex(this.label4, 0);
			this.Controls.SetChildIndex(this.password, 0);
			this.Controls.SetChildIndex(this.rememberPassword, 0);
			this.ResumeLayout(false);
            this.PerformLayout();
		}
		#endregion

		#region Event Handlers
        /// <summary>
        /// Event handler for username text changed event
        /// </summary>
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
        /// <summary>
        /// Activate Page
        /// </summary>
        /// <param name="previousIndex">previous Page</param>
		internal override void ActivatePage(int previousIndex)
		{
			base.ActivatePage (previousIndex);

			// Enable/disable the buttons
			username_TextChanged( this, null );
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

        /// <summary>
        /// validate page
        /// </summary>
        /// <param name="currentIndex">current index</param>
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

