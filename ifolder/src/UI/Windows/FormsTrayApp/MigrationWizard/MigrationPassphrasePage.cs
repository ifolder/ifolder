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

namespace Novell.Wizard
{
	/// <summary>
	/// Class for the wizard page where the invitation file is selected.
	/// </summary>
	public class MigrationPassphrasePage : Novell.Wizard.MigrationInteriorPageTemplate
	{
		#region Class Members

		//		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(SelectInvitationPage));
		private System.Windows.Forms.Label EnterPPTitle;
		private System.Windows.Forms.Label EnterPPLabel;
		private System.Windows.Forms.Label RetypePPLabel;
		private System.Windows.Forms.Label RememberPPLabel;
		private System.Windows.Forms.TextBox EnterPPText;
		private System.Windows.Forms.TextBox RetypePPText;
		private System.Windows.Forms.CheckBox RememberPPCheck;
		private System.Windows.Forms.Label RecoveryAgentTitle;
		private System.Windows.Forms.Label RecoveryAgentLabel;
		private System.Windows.Forms.ComboBox RecoveryAgentCombo;
		
		//private System.Windows.Forms.RadioButton removeFromServer;
		//private System.Windows.Forms.RadioButton copyToServer;

		//private System.Windows.Forms.CheckBox copyOption;

		//private System.Windows.Forms.TextBox iFolderLocation;
		//private System.Windows.Forms.Button browseButton;
		//private System.Windows.Forms.CheckBox defaultServer;
		//private System.Windows.Forms.Label defaultDescription;
		private System.ComponentModel.IContainer components = null;

		//private System.Windows.Forms.Label label1, label2;

		//private string homeLocation;
		//private string prevLoc;
		private static System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(Novell.FormsTrayApp.FormsTrayApp));

		#endregion

		#region Constructor

		/// <summary>
		/// Constructs a ServerPage object.
		/// </summary>
		public MigrationPassphrasePage()
		{
			// This call is required by the Windows Form Designer.
			//this.homeLocation = "Home Location";
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
			EnterPPTitle = new Label();
			EnterPPLabel = new Label();
			RetypePPLabel = new Label();
			this.EnterPPText = new TextBox();
			this.RetypePPText = new TextBox();
			this.RecoveryAgentTitle = new Label();
			this.RecoveryAgentLabel = new Label();
			this.RecoveryAgentCombo = new ComboBox();
			RememberPPLabel = new Label();
			RememberPPCheck = new CheckBox();

			//EnterPPTitle
			EnterPPTitle.Text = Resource.GetString("PassPhraseTitle"); //Please enter passphrase details here
			EnterPPTitle.Location = new System.Drawing.Point(40, 96);
			EnterPPTitle.Size = new System.Drawing.Size(416, 16);
			//EnterPPLabel
			EnterPPLabel.Text = Resource.GetString("EnterPassPhrase"); //"Enter Passphrase:"
			EnterPPLabel.Location = new System.Drawing.Point(80, 122);
			EnterPPLabel.Size = new System.Drawing.Size(120, 16);
			// EnterPPText
			this.EnterPPText.Text = "";
			this.EnterPPText.Location = new System.Drawing.Point(200, 122);
			this.EnterPPText.Size = new System.Drawing.Size(260, 16);
			//RetypePPLabel
			RetypePPLabel.Text = Resource.GetString("ReTypePassPhrase"); //Retype Passphrase:
			RetypePPLabel.Location = new System.Drawing.Point(80, 146);
			RetypePPLabel.Size = new System.Drawing.Size(120, 16);
			//RetypePPText
			this.RetypePPText.Text = "";
			this.RetypePPText.Location = new System.Drawing.Point(200, 146);
			this.RetypePPText.Size = new System.Drawing.Size(260, 16);
			//RememberPPLabel
			RememberPPLabel.Text = Resource.GetString("RememberPassPhrase"); //Remember Passphrase
			RememberPPLabel.Location = new System.Drawing.Point(200, 170);
			RememberPPLabel.Size = new System.Drawing.Size(350, 16);
			//RememberPPCheck
			RememberPPCheck.Text = Resource.GetString("RememberPassPhrase"); //Remember Passphrase
			RememberPPCheck.Location = new System.Drawing.Point(200, 170);
			RememberPPCheck.Size = new System.Drawing.Size(350, 16);
			//RecoveryAgentTitle
			this.RecoveryAgentTitle.Text = Resource.GetString("SelectRecoveryAgent"); //Select Recovery Agent:
			this.RecoveryAgentTitle.Location = new System.Drawing.Point(40, 196);
			this.RecoveryAgentTitle.Size = new System.Drawing.Size(416, 16);
			//RecoveryAgentLabel
			this.RecoveryAgentLabel.Text = Resource.GetString("RecoveryAgent"); //Recovery Agent:
			this.RecoveryAgentLabel.Location = new System.Drawing.Point(80, 220);
			this.RecoveryAgentLabel.Size = new System.Drawing.Size(120, 16);
			//RecoveryAgentCombo
			this.RecoveryAgentCombo.Location = new System.Drawing.Point(200, 220);
			this.RecoveryAgentCombo.Size = new System.Drawing.Size(260, 16);
			this.SuspendLayout();
			/*
			// 
			// label1
			// 
			
			this.label1.Location = new System.Drawing.Point(40, 208);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(50, 24);
			this.label1.TabIndex = 1;
			this.label1.Text = "This is PASSPHRASE PAGE";
			// 
			// iFolderLocation
			// 
		
			this.iFolderLocation.Location = new System.Drawing.Point(96, 208);
			this.iFolderLocation.Name = "iFolderLocation";
			this.iFolderLocation.Size = new System.Drawing.Size(280, 20);
			this.iFolderLocation.TabIndex = 2;
			this.iFolderLocation.Text = this.homeLocation;
			this.iFolderLocation.Enabled = false;

			///
			/// Browse button
			/// 

			this.browseButton.Location = new Point(390, 208);
			this.browseButton.Size = new Size(75, 20);
			this.browseButton.Name = "browseButton";
			this.browseButton.Text = "&Browse";
			this.browseButton.Enabled = false;
			this.browseButton.Click += new EventHandler(browseButton_Click);
		
			//this.serverAddress.TextChanged += new System.EventHandler(this.serverAddress_TextChanged);
			this.removeFromServer.Location = new Point(96, 120);
			this.removeFromServer.Name = "removeFromServer";
			this.removeFromServer.Size = new Size(320, 20);
			this.removeFromServer.TabIndex = 2;
			this.removeFromServer.Text = "Migrate the ifolder and disconnect it from 2.x domain";
			this.removeFromServer.Checked = true;
			this.removeFromServer.CheckedChanged += new EventHandler(removeClicked);
			
			//  Copy from server radio button

			this.copyToServer.Location = new Point(96, 144);
			this.copyToServer.Name = "copyToServer";
			this.copyToServer.Size = new Size(320, 20);
			this.copyToServer.Text = "Create a copy and connect it to 3.x server";
			this.copyToServer.Checked = false;
			this.copyToServer.CheckedChanged +=new EventHandler(copyToServer_CheckedChanged);
			

			// CopyOption.
			this.copyOption.Location = new Point(112, 168);  // change 144
			this.copyOption.Name = "copyOption";
			this.copyOption.Size = new Size(320,20);
			this.copyOption.Text = "Copy only the iFolder content";
			this.copyOption.Enabled = false;
			this.copyOption.Visible = true;
			this.copyOption.Checked = false;
			
			// 
			// label2
			// 
			
			this.label2.Location = new System.Drawing.Point(40, 96);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(416, 24);
			this.label2.TabIndex = 1;
			//this.label2.Text = "Enter the name of your iFolder server (for example, ifolder.example.net).";
			this.label2.Text = "Select one of the following options";
			
			// 
			// defaultDescription
			//
			*/ 
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
			this.Controls.Add(this.EnterPPTitle);
			this.Controls.Add(this.EnterPPLabel);
			this.Controls.Add(this.RememberPPCheck);
			this.Controls.Add(this.RetypePPLabel);
			this.Controls.Add(this.RetypePPText);
			this.Controls.Add(this.EnterPPText);
			this.Controls.Add(this.RecoveryAgentLabel);
			this.Controls.Add(this.RecoveryAgentTitle);
			this.Controls.Add(this.RecoveryAgentCombo);
			this.Load+= new EventHandler(MigrationPassphrasePage_Load);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers

		private void browseButton_Click(object sender, System.EventArgs args)
		{
		}
		private void removeClicked(object sender, System.EventArgs args)
		{
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
        /// ACtiavte Page
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
        /// Deactivate Page
        /// </summary>
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
        /// Validate Page
        /// </summary>
        /// <param name="currentIndex">current index of page</param>
        internal override int ValidatePage(int currentIndex)
		{
			// TODO:

			return base.ValidatePage (currentIndex);
		}

		#endregion

		#region Properties

        /// <summary>
        /// Gets the Passphrase
        /// </summary>
		public string Passphrase
		{
			get
			{
				return this.EnterPPText.Text;
			}
		}

        /// <summary>
        /// Gets the Re Type Passphrase
        /// </summary>
		public string RetypePassphrase
		{
			get
			{
				return this.RetypePPText.Text;
			}
		}

        /// <summary>
        /// Gets the RA
        /// </summary>
		public string RecoveryAgent
		{
			get
			{
				if( this.RecoveryAgentCombo.SelectedIndex >= 0)
					return (string)(this.RecoveryAgentCombo.SelectedText);
				return null;
				//return this.RecoveryAgentCombo.SelectedItem;
			}
		}

        /// <summary>
        /// Gets the remember Passphrase
        /// </summary>
		public bool RememberPassphrase
		{
			get
			{
				return this.RememberPPCheck.Checked;
			}
		}

		#endregion

		private void copyToServer_CheckedChanged(object sender, EventArgs e)
		{
		}

        /// <summary>
        /// Event Handler for migration Pasphrase load event
        /// </summary>
        private void MigrationPassphrasePage_Load(object sender, EventArgs e)
		{
			string[] rAgents= ((MigrationWizard)this.Parent).simws.GetRAListOnClient(((MigrationWizard)this.Parent).MigrationIdentityPage.domain.ID/*DomainID*/);
			foreach( string rAgent in rAgents)
			{
				this.RecoveryAgentCombo.Items.Add( rAgent );
				//MessageBox.Show(String.Format("Adding {0}", rAgent));
			}
			this.RecoveryAgentCombo.Items.Add("None");
		}
	}
	/*
	internal class EnterPP : System.Windows.Forms.Form
	{
		#region class members

		private System.Windows.Forms.Label EnterPPTitle;
		private System.Windows.Forms.Label EnterPPLabel;
		private System.Windows.Forms.Label RetypePPLabel;
		private System.Windows.Forms.Label RememberPPLabel;
		#endregion
		#region constructor

		public EnterPP()
		{
			EnterPPTitle = new Label();
			EnterPPLabel = new Label();
			RetypePPLabel = new Label();
			RememberPPLabel = new Label();

			//EnterPPTitle
			EnterPPTitle.Text = "Enter Passphrase:";
			EnterPPTitle.Location = new System.Drawing.Point(40, 96);
			EnterPPTitle.Size = new System.Drawing.Size(416, 16);
			//EnterPPLabel
			EnterPPLabel.Text = "Passphrase";
			EnterPPLabel.Location = new System.Drawing.Point(80, 122);
			EnterPPLabel.Size = new System.Drawing.Size(90, 16);
			//RetypePPLabel
			RetypePPLabel.Text = "Retype Passphrase";
			RetypePPLabel.Location = new System.Drawing.Point(80, 146);
			RetypePPLabel.Size = new System.Drawing.Size(90, 16);
			//RememberPPLabel
			RememberPPLabel.Text = "Remember Passphrase";
			RememberPPLabel.Location = new System.Drawing.Point(80, 170);
			RememberPPLabel.Size = new System.Drawing.Size(200, 16);
			this.Controls.Add(EnterPPTitle);
			this.Controls.Add(this.EnterPPLabel);
			this.Controls.Add(this.RetypePPLabel);
			this.Controls.Add(this.RememberPPLabel);

		}
		#endregion
		
	}
	*/
}

