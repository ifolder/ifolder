/****************************************************************************
 |
 | Copyright (c) [2007] Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 | Author: Ramesh Sunder <sramesh@novell.com>
 |
 |***************************************************************************/

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
	public class DefaultiFolderPage : Novell.Wizard.InteriorPageTemplate
	{
		#region Class Members
		enum SecurityState
		{
			encryption = 1,
			enforceEncryption = 2,
			SSL = 4,
			enforceSSL = 8
		}
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox serverAddress;
		private System.Windows.Forms.CheckBox defaultServer;
		private System.Windows.Forms.Label defaultDescription;
		private System.ComponentModel.IContainer components = null;
		private System.Resources.ResourceManager resManager;
		
		private System.Windows.Forms.RadioButton encryptionCheckButton;
		private System.Windows.Forms.RadioButton sslCheckButton;
		private System.Windows.Forms.TextBox LocationEntry;
		private System.Windows.Forms.CheckBox CreateDefault;
		private Button BrowseButton;
		private string DefaultPath;
		private SimiasWebService simws;
		private iFolderWebService ifws;
		private DomainInformation domainInfo;
		private bool upload;
		private string defaultiFolderID;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructs a ServerPage object.
		/// </summary>
		public DefaultiFolderPage(iFolderWebService ifws, SimiasWebService simws, DomainInformation domainInfo )
		{
			// This call is required by the Windows Form Designer.
			this.resManager = new System.Resources.ResourceManager(typeof(Novell.FormsTrayApp.FormsTrayApp));
			InitializeComponent();
			this.simws = simws;
			this.domainInfo = domainInfo;
			this.ifws = ifws;
		}

		#endregion

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.encryptionCheckButton = new RadioButton();
			this.sslCheckButton = new RadioButton();
			this.LocationEntry = new TextBox();
			this.CreateDefault = new CheckBox();
			this.BrowseButton = new Button();
			this.label1 = new Label();
			this.label2 = new Label();

			//	row1
			// CreateDefault
			//
			this.CreateDefault.Location = new Point( 40, 106);
			this.CreateDefault.Text = this.resManager.GetString("CreateDefaultiFolder");//"Create Default iFolder";
			this.CreateDefault.Size = new Size(416, 24);
			this.CreateDefault.TabIndex = 1;
			this.CreateDefault.CheckedChanged += new EventHandler(CreateDefault_CheckedChanged);
			
			// row2
			// locationentry, location label, browse button
			//
			this.label1.Location = new System.Drawing.Point(56, 152);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(50, 20);
			this.label1.Text = this.resManager.GetString("LocationText") + " "; //"Location:"+ " ";
			// 
			// locationentry
			//
			this.LocationEntry.Location = new Point(112, 150);
			this.LocationEntry.Multiline = false;
			this.LocationEntry.Size = new Size(280, 20);
			this.LocationEntry.TabIndex = 2;
			this.LocationEntry.Text = string.Empty;
			//
			// browsebutton
			//
			this.BrowseButton.Text = this.resManager.GetString("BrowseText"); //"Browse";
			this.BrowseButton.Location = new Point( 406, 150);
			this.BrowseButton.TabIndex = 3;
			this.BrowseButton.Size = new Size( 75, 20);
			this.BrowseButton.Click += new EventHandler(BrowseButton_Click);

			// row 3
			// encryptioncheckbutton, ssl checkbutton
			//
			this.label2.Text = this.resManager.GetString("Security")+ ": "; //"Security:";
			this.label2.Location = new Point( 56, 194);
			this.label2.Size = new Size( 50, 20);
		
			this.encryptionCheckButton.Text = this.resManager.GetString("EncryptedText");//"Encrypted";
			this.encryptionCheckButton.Location = new Point(150, 194);
			this.encryptionCheckButton.Size = new Size(125, 20);
			this.encryptionCheckButton.TabIndex = 4;

			this.sslCheckButton.Text = this.resManager.GetString("SharableText");//"Shared";
			this.sslCheckButton.Size = new Size(125, 20);
			this.sslCheckButton.TabIndex = 5;
			this.sslCheckButton.Location = new Point(280, 194);

			this.Controls.Add(this.CreateDefault);
			this.Controls.Add(this.LocationEntry);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.BrowseButton);
			this.Controls.Add(this.encryptionCheckButton);
			this.Controls.Add(this.sslCheckButton);
			this.ResumeLayout(false);
			this.Load += new EventHandler(DefaultiFolderPage_Load);
		}
		#endregion

		#region Event Handlers


		#endregion

		#region Overridden Methods

		internal override void ActivatePage(int previousIndex)
		{
			base.ActivatePage (previousIndex);

			// Enable/disable the buttons.
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
			bool status = false;
			if( this.CreateDefault.Checked == false)
				status = true;
			else if( upload ) // Create iFolder
			{
				iFolderWeb ifolder = null;
				ifolder = CreateDefaultiFolder( this.sslCheckButton.Checked );
				if( ifolder != null)
				{
					status = true;
					this.simws.DefaultAccount( domainInfo.ID, ifolder.ID);
				}
			}
			else	// Download iFolder
			{
				iFolderWeb ifolder= this.ifws.GetiFolder( this.defaultiFolderID );
				if( ifolder != null)
				{
					status = DownloadiFolder(ifolder);
				}
				else
				{
					status = false;
				}
			}
			if( status == true)
				return base.ValidatePage (currentIndex);
			else
				return currentIndex;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the the server domain Information
		/// </summary>
		public DomainInformation DomainInfo
		{
			get
			{
				return this.domainInfo;
			}
			set
			{
				this.domainInfo = value;
			}
		}

		/// <summary>
		/// Gets the server address entered.
		/// </summary>
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

		public string defaultPath
		{
			get
			{
				return this.DefaultPath;
			}
			set
			{
				this.DefaultPath = value;
			}
		}

		public string DefaultiFolderID
		{
			get
			{
				return this.defaultiFolderID;
			}
			set
			{
				this.defaultiFolderID = value;
			}
		}

		#endregion
		
		private void CreateDefault_CheckedChanged(object sender, EventArgs e)
		{
			if( this.CreateDefault.Checked == true)
			{
				this.label1.Enabled = true;
				this.label2.Enabled = true;
				this.sslCheckButton.Enabled = this.encryptionCheckButton.Enabled = true;
				this.LocationEntry.Enabled = this.BrowseButton.Enabled = true;
			}
			else
			{
				this.label1.Enabled = false;
				this.label2.Enabled = false;
				this.sslCheckButton.Enabled = this.encryptionCheckButton.Enabled = false;
				this.LocationEntry.Enabled = this.BrowseButton.Enabled = false;
			}
		}

		private bool DownloadiFolder( iFolderWeb defaultiFolder)
		{
			bool status = false;
			if( defaultiFolder.encryptionAlgorithm == null || defaultiFolder.encryptionAlgorithm == "")
			{
				// unencrypted...
				status = true;
			}
			else
			{
				// encrypted.. Check for passphrase
				string passphrasecheck = null;
				passphrasecheck = simws.GetPassPhrase(domainInfo.ID);
				if( passphrasecheck == null || passphrasecheck =="")
				{
					VerifyPassphraseDialog vpd = new VerifyPassphraseDialog(domainInfo.ID, this.simws);
					vpd.ShowDialog();
					status = vpd.PassphraseStatus;
				}
				else
				{
					status = true;
				}
			}
			if( status == true )
			{
				try
				{
					DirectoryInfo di = new DirectoryInfo( this.LocationEntry.Text );
					di.Create();
					iFolderWeb ifolder = this.ifws.AcceptiFolderInvitation( defaultiFolder.DomainID, defaultiFolder.ID, this.LocationEntry.Text);
					AccountWizard wiz = (AccountWizard)this.Parent;
					wiz.UpdateDisplay( ifolder, this.LocationEntry.Text+"/"+ifolder.Name );
				}
				catch( Exception ex )
				{
					DisplayErrorMesg(ex);
					return false;
				}
				return true;
			}
			else return status;
		}

		private iFolderWeb CreateDefaultiFolder( bool shared)
		{
			try
			{
				DirectoryInfo di = new DirectoryInfo( this.LocationEntry.Text );
				di.Create();
			}
			catch( Exception ex)
			{
				// Unable to create the folder
				DisplayErrorMesg(ex);
				return null;
			}
			if( shared)
			{
				iFolderWeb ifolder = null;
				try
				{
					ifolder = this.ifws.CreateiFolderInDomain(this.LocationEntry.Text, this.domainInfo.ID);
				}
				catch( Exception ex)
				{
					DisplayErrorMesg(ex);
					return null;
				}
				return ifolder;
			}
			else
			{
				string algorithm = "BlowFish";
				bool passPhraseStatus = false;
				bool passphraseStatus = false;
				try
				{
					passphraseStatus = simws.IsPassPhraseSet(domainInfo.ID);
				}
				catch(Exception ex)
				{
				//	MessageBox.Show("Unable to contact the server.");
					return null;
				}
				if(passphraseStatus == true)
				{
					// if passphrase not given during login
					string passphrasecheck = null;
					passphrasecheck = simws.GetPassPhrase(domainInfo.ID);
					if( passphrasecheck == null || passphrasecheck =="")
					{
						VerifyPassphraseDialog vpd = new VerifyPassphraseDialog(domainInfo.ID, this.simws);
						vpd.ShowDialog();
						passPhraseStatus = vpd.PassphraseStatus;
					}
					else
					{
						passPhraseStatus = true;
					}
				}
				else
				{
					// Passphrase not enterd at the time of login...
					EnterPassphraseDialog enterPassPhrase= new EnterPassphraseDialog(domainInfo.ID, this.simws);
					enterPassPhrase.ShowDialog();
					passPhraseStatus = enterPassPhrase.PassphraseStatus;
				}
				if( passPhraseStatus == true)
				{
					// check for passphrase existence and display corresponding dialogs.
					string Passphrase = simws.GetPassPhrase(domainInfo.ID);
					iFolderWeb ifolder = null;
					try
					{
						ifolder = this.ifws.CreateiFolderInDomainEncr(this.LocationEntry.Text, domainInfo.ID, false, algorithm, Passphrase);
					}
					catch( Exception ex)
					{
						DisplayErrorMesg(ex);
						return null;
					}
					return ifolder;
				}
			}
			return null;
		}
	
		private void DisplayErrorMesg( Exception ex)
		{
			MyMessageBox mmb;
			string message;
			System.Resources.ResourceManager resourceManager =new System.Resources.ResourceManager(typeof(CreateiFolder));
			string caption = resourceManager.GetString("pathInvalidErrorTitle");

			if (ex.Message.IndexOf("InvalidCharactersPath") != -1)
			{
				message = resourceManager.GetString("invalidCharsError");
			}
			else if (ex.Message.IndexOf("AtOrInsideStorePath") != -1)
			{
				message = resourceManager.GetString("pathInStoreError");
			}
			else if (ex.Message.IndexOf("ContainsStorePath") != -1)
			{
				message = resourceManager.GetString("pathContainsStoreError");
			}
			else if (ex.Message.IndexOf("SystemDirectoryPath") != -1)
			{
				message = resourceManager.GetString("systemDirError");
			}
			else if (ex.Message.IndexOf("SystemDrivePath") != -1)
			{
				message = resourceManager.GetString("systemDriveError");
			}
			else if (ex.Message.IndexOf("IncludesWinDirPath") != -1)
			{
				message = resourceManager.GetString("winDirError");
			}
			else if (ex.Message.IndexOf("IncludesProgFilesPath") != -1)
			{
				message = resourceManager.GetString("progFilesDirError");
			}
			else if (ex.Message.IndexOf("ContainsCollectionPath") != -1)
			{
				message = resourceManager.GetString("containsiFolderError");
			}
			else if (ex.Message.IndexOf("AtOrInsideCollectionPath") != -1)
			{
				message = resourceManager.GetString("pathIniFolderError");
			}
			else if (ex.Message.IndexOf("RootOfDrivePath") != -1)
			{
				message = resourceManager.GetString("rootDriveError");
			}
			else if (ex.Message.IndexOf("NotFixedDrivePath") != -1)
			{
				message = resourceManager.GetString("networkPathError");
			}
			else if (ex.Message.IndexOf("PathExists") != -1)
			{
				message = resourceManager.GetString("pathExistsError");
			}
			else if( this.upload == true)
			{
				message = resourceManager.GetString("iFolderCreateError");
				caption = resourceManager.GetString("errorTitle");
			}
			else
			{
				message = resourceManager.GetString("acceptError");
				caption = resourceManager.GetString("errorTitle");
			}

			mmb = new MyMessageBox(message, caption, string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
			mmb.ShowDialog();
		}

		private void DefaultiFolderPage_Load(object sender, EventArgs e)
		{
			this.CreateDefault.Checked = true;
			string str = "";
			str = this.simws.GetDefaultiFolder( this.domainInfo.ID);
			if( str == null || str == "")
			{
				this.upload = true;
				this.defaultPath += "\\" + this.resManager.GetString("DefaultDirName");
				int SecurityPolicy = this.ifws.GetSecurityPolicy(domainInfo.ID);
				this.encryptionCheckButton.Checked = true;
				this.encryptionCheckButton.Enabled = this.sslCheckButton.Enabled = false;
				this.sslCheckButton.Checked = false;
				if(SecurityPolicy !=0)
				{
					if( (SecurityPolicy & (int)SecurityState.encryption) == (int) SecurityState.encryption)
					{
						if( (SecurityPolicy & (int)SecurityState.enforceEncryption) == (int) SecurityState.enforceEncryption)
							encryptionCheckButton.Checked = true;
						else
						{
							encryptionCheckButton.Enabled = true;
							sslCheckButton.Enabled = true;
						}
					}
					else
						sslCheckButton.Checked = true;
				}
				else
					sslCheckButton.Checked = true;
			}	
			else
			{
				this.upload = false;
				this.defaultiFolderID = str;
				this.CreateDefault.Text = "Download Default iFolder";
				this.label2.Visible = this.encryptionCheckButton.Visible = this.sslCheckButton.Visible = false;
				
			}
			if( this.DefaultPath != null && this.DefaultPath.Length > 0)
			{
				this.LocationEntry.Text = this.DefaultPath;
			}
		}

		private void BrowseButton_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.Description = this.resManager.GetString("chooseFolder");
			if(folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				this.LocationEntry.Text = folderBrowserDialog.SelectedPath;
			}
		}
	}
}

