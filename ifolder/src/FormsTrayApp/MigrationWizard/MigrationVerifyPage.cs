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
using Novell.FormsTrayApp;

namespace Novell.Wizard
{

	/// <summary>
	/// Class for the wizard page where the iFolder location is selected.
	/// </summary>
	public class MigrationVerifyPage : Novell.Wizard.MigrationInteriorPageTemplate
	{
		#region Class Members

//		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(SelectiFolderLocationPage));
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

		//	defaultDescription.Visible = defaultAccount.Visible = !hideDefaultAccount;
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
			this.label1.Location = new System.Drawing.Point(50, 128);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(94, 16);
			this.label1.TabIndex = 3;
			this.label1.Text = Resource.GetString("LocationText");//"location:";
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
			this.label4.Size = new System.Drawing.Size(118, 16);
			this.label4.TabIndex = 5;
			this.label4.Text = Resource.GetString("MigrationOptions")+":";//"Migration option:";
			// 
			// defaultDescription
			// 
			this.defaultDescription.Location = new System.Drawing.Point(50, 176);
			this.defaultDescription.Name = "defaultDescription";
			this.defaultDescription.Size = new System.Drawing.Size(118, 16);
			this.defaultDescription.TabIndex = 7;
			this.defaultDescription.Text = Resource.GetString("Security") + ":";//"Security:";
			// 
			// serverAddress
			// 
			this.serverAddress.Location = new System.Drawing.Point(176, 104);
			this.serverAddress.Name = "serverAddress";
			this.serverAddress.Size = new System.Drawing.Size(304, 16);
			this.serverAddress.TabIndex = 2;
			// 
			// location
			// 
			this.location.Location = new System.Drawing.Point(176, 128);
			this.location.Name = "location";
			this.location.Size = new System.Drawing.Size(304, 16);
			this.location.TabIndex = 4;
			// 
			// migrationOption
			// 
			this.migrationOption.Location = new System.Drawing.Point(176, 152);
			this.migrationOption.Name = "migrationOption";
			this.migrationOption.Size = new System.Drawing.Size(304, 16);
			this.migrationOption.TabIndex = 6;
			// 
			// security
			// 
			this.security.Location = new System.Drawing.Point(176, 176);
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

		internal override void ActivatePage(int previousIndex)
		{
			base.ActivatePage (previousIndex);

			
			/*
			wizard = (AccountWizard)this.Parent;
			serverAddress.Text = wizard.ServerPage.ServerAddress;
			location.Text = wizard.IdentityPage.location;
			// TODO: Localize
			migrationOption.Text = wizard.IdentityPage.migrationOption ? "Yes" : "No";
			security.Text = wizard.ServerPage.DefaultServer ? "Yes" : "No";
			*/
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
			wizard = (MigrationWizard)this.Parent;
			if ( !wizard.MigrateFolder() )
			{
				// Unable to migrate the folder
				return currentIndex;
			}
			return base.ValidatePage (currentIndex);
		}

		#endregion

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

		private void MigrationVerifyPage_Load(object sender, EventArgs e)
		{
			UpdateDetails();
			/*
			MigrationWizard wizard = (MigrationWizard)this.Parent;
			this.serverAddress.Text = wizard.MigrationIdentityPage.DomainName;
			this.location.Text = wizard.MigrationServerPage.HomeLocation;
			if(wizard.MigrationServerPage.MigrationOption)
				this.migrationOption.Text = "Remove from 2.x domain";
			else
				this.migrationOption.Text = "Create a copy of the folder and connect to 3.x to domain";
			this.security.Text = "";
			if( wizard.MigrationIdentityPage.Encrypion)
			{
				this.security.Text = "Encrypt the folder";
				if( wizard.MigrationIdentityPage.SSL)
					this.security.Text += " and use secure channel for data transfer";
			}
			else if(wizard.MigrationIdentityPage.SSL)
			{
				this.security.Text = "Use secure channel for data transfer";
			}
			else
			{
				this.security.Text = "None";
			}
			*/
		}

		private void security_MouseHover(object sender, EventArgs e)
		{
			
		}
	}
}

