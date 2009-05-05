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
using Novell.FormsTrayApp;
using Novell.iFolderCom;

namespace Novell.Wizard
{
	/// <summary>
	/// Class for the wizard page where the invitation file is selected.
	/// </summary>
	public class MigrationServerPage : Novell.Wizard.MigrationInteriorPageTemplate
	{
		#region Class Members

//		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(SelectInvitationPage));
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;

		private System.Windows.Forms.RadioButton removeFromServer;
		private System.Windows.Forms.RadioButton copyToServer;

		private System.Windows.Forms.CheckBox copyOption;

		private System.Windows.Forms.TextBox iFolderLocation;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.CheckBox defaultServer;
		private System.Windows.Forms.Label defaultDescription;
		private System.ComponentModel.IContainer components = null;
		private static System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(CreateiFolder));
		private static System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(Novell.FormsTrayApp.FormsTrayApp));
		private string homeLocation;
		private string prevLoc;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructs a ServerPage object.
		/// </summary>
		public MigrationServerPage(string loc)
		{
			// This call is required by the Windows Form Designer.
			this.homeLocation = loc;
			InitializeComponent();

			//defaultDescription.Visible = defaultServer.Visible = !makeDefaultAccount;
			//defaultServer.Checked = makeDefaultAccount;
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
			this.removeFromServer = new RadioButton();
			this.copyToServer = new RadioButton();
			this.iFolderLocation = new TextBox();
			this.copyOption = new CheckBox();
			this.browseButton = new Button();
			this.defaultDescription = new System.Windows.Forms.Label();
			this.defaultServer = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();

			// 
			// label1
			// 
			
			this.label1.Location = new System.Drawing.Point(40, 215);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(50, 24);
			this.label1.TabIndex = 1;
			this.label1.Text = Resource.GetString("LocationText");//"Location:";
			// 
			// iFolderLocation
			// 
		
			this.iFolderLocation.Location = new System.Drawing.Point(96, 213);
			this.iFolderLocation.Name = "iFolderLocation";
			this.iFolderLocation.Size = new System.Drawing.Size(280, 20);
			this.iFolderLocation.TabIndex = 2;
			this.iFolderLocation.Text = this.homeLocation;
			this.iFolderLocation.Enabled = false;
            this.iFolderLocation.TextChanged += new EventHandler(iFolderLocation_TextChanged);

			///
			/// Browse button
			/// 

			this.browseButton.Location = new Point(390, 213);
			this.browseButton.Size = new Size(75, 20);
			this.browseButton.Name = "browseButton";
			this.browseButton.Text = resourceManager.GetString("browse.Text");//"&Browse";
			this.browseButton.Enabled = false;
			this.browseButton.Click += new EventHandler(browseButton_Click);
		
			//this.serverAddress.TextChanged += new System.EventHandler(this.serverAddress_TextChanged);
			this.removeFromServer.Location = new Point(96, 125);
			this.removeFromServer.Name = "removeFromServer";
			this.removeFromServer.Size = new Size(320, 20);
			this.removeFromServer.TabIndex = 2;
			this.removeFromServer.Text = Resource.GetString("MigrateNRemove");//"Migrate the ifolder and disconnect it from 3.x domain";
			this.removeFromServer.Checked = true;
			this.removeFromServer.CheckedChanged += new EventHandler(removeClicked);
			
			//  Copy from server radio button

			this.copyToServer.Location = new Point(96, 150);
			this.copyToServer.Name = "copyToServer";
			this.copyToServer.Size = new Size(320, 20);
			this.copyToServer.Text = Resource.GetString("MigrateNCopy");//"Create a copy and connect it to 3.x server";
			this.copyToServer.Checked = false;
			this.copyToServer.CheckedChanged +=new EventHandler(copyToServer_CheckedChanged);
			

			// CopyOption.
			this.copyOption.Location = new Point(112, 175);  // change 144
			this.copyOption.Name = "copyOption";
			this.copyOption.Size = new Size(320,20);
			this.copyOption.Text = Resource.GetString("CopyParent");//"Copy the parent folder";
			this.copyOption.Enabled = false;
			this.copyOption.Visible = true;
			this.copyOption.Checked = false;
			
			// 
			// label2
			// 
			
			this.label2.Location = new System.Drawing.Point(40, 100);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(416, 24);
			this.label2.TabIndex = 1;
			//this.label2.Text = "Enter the name of your iFolder server (for example, ifolder.example.net).";
			this.label2.Text = Resource.GetString("SelectAnOption");//"Select one of the following options";
			
			// 
			// defaultDescription
			// 
			/*
			this.defaultDescription.Location = new System.Drawing.Point(40, 168);
			this.defaultDescription.Name = "defaultDescription";
			this.defaultDescription.Size = new System.Drawing.Size(416, 32);
			this.defaultDescription.TabIndex = 3;
			this.defaultDescription.Text = "Setting this iFolder server as your default server will allow iFolder to automati" +
				"cally select this server when adding new iFolders.";
			*/
			// 
			// defaultServer
			// 
			/*
			this.defaultServer.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.defaultServer.Location = new System.Drawing.Point(50, 208);
			this.defaultServer.Name = "defaultServer";
			this.defaultServer.Size = new System.Drawing.Size(406, 24);
			this.defaultServer.TabIndex = 4;
			this.defaultServer.Text = "Make this my &default server.";
			*/
			// 
			// ServerPage
			//
			/*
			this.Controls.Add(this.defaultServer);
			this.Controls.Add(this.serverAddress);
			this.Controls.Add(this.defaultDescription);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.removeFromServer);
			this.HeaderSubTitle = "HeaderSubTitle";
			this.HeaderTitle = "HeaderTitle";
			this.Name = "ServerPage";
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.defaultDescription, 0);
			this.Controls.SetChildIndex(this.serverAddress, 0);
			this.Controls.SetChildIndex(this.defaultServer, 0);
			*/
			this.Controls.Add(this.removeFromServer);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.copyToServer);
			this.Controls.Add(this.iFolderLocation);
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.copyOption);
			this.ResumeLayout(false);

		}

        /// <summary>
        /// Event Handler for iFOlderlocation text changed event
        /// </summary>
        void iFolderLocation_TextChanged(object sender, EventArgs e)
        {
            // Check if the iFolder Location is not a proper entry...
            if (this.copyToServer.Checked == true)
            {
                if (this.iFolderLocation.Text == null || this.iFolderLocation.Text == string.Empty)
                {
                    // Disble the next button...
                    ((MigrationWizard)this.Parent).MigrationWizardButtons = MigrationWizardButtons.Back | MigrationWizardButtons.Cancel;
                }
                else
                    ((MigrationWizard)this.Parent).MigrationWizardButtons = MigrationWizardButtons.Next | MigrationWizardButtons.Back | MigrationWizardButtons.Cancel;
            }
            else
                ((MigrationWizard)this.Parent).MigrationWizardButtons = MigrationWizardButtons.Next | MigrationWizardButtons.Back | MigrationWizardButtons.Cancel;
        }
		#endregion

		#region Event Handlers

        /// <summary>
        /// Event Handler for Browse button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
		private void browseButton_Click(object sender, System.EventArgs args)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

			folderBrowserDialog.Description = /*resourceManager.GetString(*/"chooseFolder";//);
			folderBrowserDialog.SelectedPath = this.iFolderLocation.Text;

			if(folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				this.iFolderLocation.Text = folderBrowserDialog.SelectedPath;
			}
		}

        /// <summary>
        /// Event Handler for remove button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
		private void removeClicked(object sender, System.EventArgs args)
		{
			if(removeFromServer.Checked == true)
			{
				this.browseButton.Enabled = false;
				this.iFolderLocation.Enabled = false;
				this.prevLoc = this.iFolderLocation.Text;
				this.iFolderLocation.Text = homeLocation;
				
			}
			else
			{
				this.browseButton.Enabled = true;
				this.iFolderLocation.Enabled = true;
				this.iFolderLocation.Text = this.prevLoc;
			}
		}
		/*
		private void serverAddress_TextChanged(object sender, System.EventArgs e)
		{
			// Enable the buttons.
			if (serverAddress.Text != "")
			{
				((AccountWizard)this.Parent).WizardButtons = WizardButtons.Next | WizardButtons.Back | WizardButtons.Cancel;
			}
			else
			{
				((AccountWizard)this.Parent).WizardButtons = WizardButtons.Back | WizardButtons.Cancel;
			}
		}
		*/
		#endregion

		#region Overridden Methods

        /// <summary>
        /// Activate Page
        /// </summary>
        /// <param name="previousIndex">previous index</param>
		internal override void ActivatePage(int previousIndex)
		{
			((MigrationWizard)this.Parent).MigrationWizardButtons = MigrationWizardButtons.Next | MigrationWizardButtons.Back | MigrationWizardButtons.Cancel;
			base.ActivatePage (previousIndex);

			// Enable/disable the buttons.
		//	serverAddress_TextChanged(this, null);

			//serverAddress.Focus();
		}

        /// <summary>
        /// Deactivate page
        /// </summary>
        /// <returns>previous index</returns>
		internal override int DeactivatePage()
		{
			// TODO - implement this...
			return base.DeactivatePage ();
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
        /// <param name="currentIndex">current Index</param>
        internal override int ValidatePage(int currentIndex)
		{
			// TODO:

			return base.ValidatePage (currentIndex);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the Migration option
		/// </summary>
		public bool MigrationOption
		{
			get
			{
				return this.removeFromServer.Checked;
			}
		}

        /// <summary>
        /// Gets the CopyParantdirectory
        /// </summary>
		public bool CopyParentDirectory
		{
			get
			{
				return this.copyOption.Checked;
			}
		}

        /// <summary>
        /// Gets the home location
        /// </summary>
		public string HomeLocation
		{
			get
			{
				return this.iFolderLocation.Text;
			}
		}

		/// <summary>
		/// Gets the server address entered.
		/// </summary>
		/*
		public string ServerAddress
		{
			get { return serverAddress.Text; }
		}

		/// <summary>
		/// Gets a value indicating if this server should be the default server.
		/// </summary>
		public bool DefaultServer
		{
			get { return defaultServer.Checked; }
		}
		*/
		#endregion

        /// <summary>
        /// Event Handler for copy to Server check boz checked changed event
        /// </summary>
        private void copyToServer_CheckedChanged(object sender, EventArgs e)
		{
			if(this.copyToServer.Checked == true)
				this.copyOption.Enabled = true;
			else
				this.copyOption.Enabled = false;
		}
	}
}

