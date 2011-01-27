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
*                 $Author: Ramesh Sunder <sramesh@novell.com>
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
using System.IO;
using Novell.iFolderCom;
using Novell.FormsTrayApp;
using Simias.Client;
using Simias.Client.Authentication;
using Simias.Client.Event;
//using Novell.FormsTrayApp;
using System.Threading;

namespace Novell.Wizard
{

	/// <summary>
	/// Class for the wizard page where the iFolder location is selected.
	/// </summary>
	public class MigrationVerifyPage : Novell.Wizard.MigrationInteriorPageTemplate
	{
		#region Class Members

		private System.Windows.Forms.Label label1;
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label serverAddress;
		private System.Windows.Forms.Label location;
		private System.Windows.Forms.Label migrationOption;
		private System.Windows.Forms.Label security;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label defaultDescription;
		private MigrationWizard wizard;
		private Connecting waitWindow;
		private static System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(Novell.FormsTrayApp.FormsTrayApp));

		#endregion

		#region Constructor

		/// <summary>
		/// Constructs a VerifyPage object.
		/// </summary>
		public MigrationVerifyPage( )
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();		
			waitWindow = null;
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
			this.location = new System.Windows.Forms.Label();
			this.migrationOption = new System.Windows.Forms.Label();
			this.security = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(50, 196);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(94, 16);
			this.label1.TabIndex = 3;
			this.label1.Text = Resource.GetString("LocationText");//"location:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(40, 100);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(440, 16);
			this.label2.TabIndex = 0;
			this.label2.Text = Resource.GetString("AccountVerify");//"Please verify that the information you have entered is correct.";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(50, 124);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(88, 16);
			this.label3.TabIndex = 1;
			this.label3.Text = Resource.GetString("DomainName")+":";//"Domain Name:";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(50, 172);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(118, 16);
			this.label4.TabIndex = 5;
			this.label4.Text = Resource.GetString("MigrationOptions")+":";//"Migration option:";
			// 
			// defaultDescription
			// 
			this.defaultDescription.Location = new System.Drawing.Point(50, 148);
			this.defaultDescription.Name = "defaultDescription";
			this.defaultDescription.Size = new System.Drawing.Size(118, 16);
			this.defaultDescription.TabIndex = 7;
			this.defaultDescription.Text = Resource.GetString("Security") + ":";//"Security:";
			// 
			// serverAddress
			// 
			this.serverAddress.Location = new System.Drawing.Point(176, 124);
			this.serverAddress.Name = "serverAddress";
			this.serverAddress.Size = new System.Drawing.Size(304, 16);
			this.serverAddress.TabIndex = 2;
			// 
			// location
			// 
			this.location.Location = new System.Drawing.Point(176, 196);
			this.location.Name = "location";
			this.location.Size = new System.Drawing.Size(304, 48);
			this.location.TabIndex = 4;
			// 
			// migrationOption
			// 
			this.migrationOption.Location = new System.Drawing.Point(176, 172);
			this.migrationOption.Name = "migrationOption";
			this.migrationOption.Size = new System.Drawing.Size(304, 16);
			this.migrationOption.TabIndex = 6;
			// 
			// security
			// 
			this.security.Location = new System.Drawing.Point(176, 148);
			this.security.Name = "security";
			this.security.Size = new System.Drawing.Size(304, 16);
			this.security.TabIndex = 8;
			this.security.MouseHover += new EventHandler(security_MouseHover);
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(40, 256);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(432, 16);
			this.label6.TabIndex = 9;
			this.label6.Text = Resource.GetString("ClickMigrate");//"Click Migrate to migrate your data.";
			// 
			// VerifyPage
			// 
			this.Controls.Add(this.label6);
			this.Controls.Add(this.security);
			this.Controls.Add(this.migrationOption);
			this.Controls.Add(this.location);
			this.Controls.Add(this.serverAddress);
			this.Controls.Add(this.defaultDescription);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.HeaderSubTitle = "HeaderSubTitle";
			this.HeaderTitle = "HeaderTitle";
			this.Name = "VerifyPage";
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.label3, 0);
			this.Controls.SetChildIndex(this.label4, 0);
			this.Controls.SetChildIndex(this.defaultDescription, 0);
			this.Controls.SetChildIndex(this.serverAddress, 0);
			this.Controls.SetChildIndex(this.location, 0);
			this.Controls.SetChildIndex(this.migrationOption, 0);
			this.Controls.SetChildIndex(this.security, 0);
			this.Controls.SetChildIndex(this.label6, 0);
			this.Load += new EventHandler(MigrationVerifyPage_Load);
			this.ResumeLayout(false);

		}
		#endregion

		#region Overridden Methods

        /// <summary>
        /// Activate page
        /// </summary>
        /// <param name="previousIndex">previous index</param>
		internal override void ActivatePage(int previousIndex)
		{
			base.ActivatePage (previousIndex);
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
        /// Validate page
        /// </summary>
        /// <param name="currentIndex">Current Index</param>
        /// <returns></returns>
		internal override int ValidatePage(int currentIndex)
		{
			Thread thrd = new Thread(new ThreadStart(ShowWaitDialog));
            thrd.Name = "Migration Wz Wait Dialog";
			thrd.Start();
			wizard = (MigrationWizard)this.Parent;
			bool result = wizard.MigrateFolder();
			CloseWaitDialog();

			if ( !result )
			{
				// Unable to migrate the folder
				return currentIndex;
			}
			return base.ValidatePage (currentIndex);
		}

        /// <summary>
        /// ShowWait Dialog
        /// </summary>
		private void ShowWaitDialog()
		{
			waitWindow = new Connecting();
			waitWindow.ShowDialog();
		}

        /// <summary>
        /// CloseWaitDialog
        /// </summary>
		public void CloseWaitDialog()
		{
			if( waitWindow!=null)
			{
				waitWindow.Close();
				waitWindow = null;
			}
		}

		#endregion

        /// <summary>
        /// UpdateDetails
        /// </summary>
		public void UpdateDetails()
		{
			MigrationWizard wizard = (MigrationWizard)this.Parent;
			this.serverAddress.Text = wizard.MigrationIdentityPage.DomainName;
			this.location.Text = wizard.MigrationServerPage.HomeLocation;
			if(wizard.MigrationServerPage.MigrationOption)
				this.migrationOption.Text = Resource.GetString("MigrateNRemove");//"Remove from 2.x domain";
			else
				this.migrationOption.Text = Resource.GetString("MigrateNCopy");//"Create a copy of the folder and connect to 3.x to domain";
			this.security.Text = "";
			if( wizard.MigrationIdentityPage.Encrypion)
			{
				this.security.Text = Resource.GetString("EncryptedText");//"Encrypt the folder";
				if( wizard.MigrationIdentityPage.SSL)
					this.security.Text += " and use secure channel for data transfer";
			}
			else if(wizard.MigrationIdentityPage.SSL)
			{
				this.security.Text = Resource.GetString("SharableText");//"Use secure channel for data transfer";
			}
			else
			{
				this.security.Text = "None";
			}
		}

        /// <summary>
        /// Event Handler for Migrationverifypage load event
        /// </summary>
        private void MigrationVerifyPage_Load(object sender, EventArgs e)
		{
			UpdateDetails();			
		}

        /// <summary>
        /// Event handler for security Mouse Hover event
        /// </summary>
        private void security_MouseHover(object sender, EventArgs e)
		{
			
		}
	}
}

