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

		private System.Windows.Forms.TextBox iFolderLocation;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.CheckBox defaultServer;
		private System.Windows.Forms.Label defaultDescription;
		private System.ComponentModel.IContainer components = null;

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
			this.browseButton = new Button();
			this.defaultDescription = new System.Windows.Forms.Label();
			this.defaultServer = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// iFolderLocation
			// 
		
			this.iFolderLocation.Location = new System.Drawing.Point(96, 184);
			this.iFolderLocation.Name = "iFolderLocation";
			this.iFolderLocation.Size = new System.Drawing.Size(280, 20);
			this.iFolderLocation.TabIndex = 2;
			this.iFolderLocation.Text = this.homeLocation;

			///
			/// Browse button
			/// 

			this.browseButton.Location = new Point(390, 184);
			this.browseButton.Size = new Size(75, 20);
			this.browseButton.Name = "browseButton";
			this.browseButton.Text = "&Browse";
			this.browseButton.Click += new EventHandler(browseButton_Click);
		
			//this.serverAddress.TextChanged += new System.EventHandler(this.serverAddress_TextChanged);
			this.removeFromServer.Location = new Point(96, 120);
			this.removeFromServer.Name = "removeFromServer";
			this.removeFromServer.Size = new Size(320, 20);
			this.removeFromServer.TabIndex = 2;
			this.removeFromServer.Text = "Migrate the folder and remove from 2.x domain";
			this.removeFromServer.Checked = true;
			this.removeFromServer.CheckedChanged += new EventHandler(removeClicked);
			
			//  Copy from server radio button

			this.copyToServer.Location = new Point(96, 144);
			this.copyToServer.Name = "copyToServer";
			this.copyToServer.Size = new Size(320, 20);
			this.copyToServer.Text = "Create a c new copy and connect to 3.x server";

			// 
			// label1
			// 
			
			this.label1.Location = new System.Drawing.Point(40, 184);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(45, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Location:";
			
			// 
			// label2
			// 
			
			this.label2.Location = new System.Drawing.Point(40, 96);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(416, 24);
			this.label2.TabIndex = 1;
			//this.label2.Text = "Enter the name of your iFolder server (for example, ifolder.example.net).";
			this.label2.Text = "Select one among the following options";
			
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
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers

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

		internal override void ActivatePage(int previousIndex)
		{
			((MigrationWizard)this.Parent).MigrationWizardButtons = MigrationWizardButtons.Next | MigrationWizardButtons.Back | MigrationWizardButtons.Cancel;
			base.ActivatePage (previousIndex);

			// Enable/disable the buttons.
		//	serverAddress_TextChanged(this, null);

			//serverAddress.Focus();
		}

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

		internal override int ValidatePage(int currentIndex)
		{
			// TODO:

			return base.ValidatePage (currentIndex);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the Migration option
		/// 
		
		public bool MigrationOption
		{
			get
			{
				return this.removeFromServer.Checked;
			}
		}

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
	}
}

