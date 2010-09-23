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

using Novell.iFolder.Web;
using Novell.FormsTrayApp;
using Novell.iFolderCom;

namespace Novell.Wizard
{
    public enum SecurityState
    {
        encryption = 1,
        enforceEncryption = 2,
        SSL = 4,
        enforceSSL = 8
    }
	/// <summary>
	/// Class for the wizard page where the invitation file is selected.
	/// </summary>
	public class DefaultiFolderPage : Novell.Wizard.InteriorPageTemplate
	{
		#region Class Members

		private Label label1;
		private Label label2;
		private System.ComponentModel.IContainer components = null;
		private System.Resources.ResourceManager resManager;
		
		private RadioButton encryptionCheckButton; // encrypted radio button
		private RadioButton sslCheckButton; // regular radio button 
		private TextBox LocationEntry;
		private CheckBox CreateDefault;
        private CheckBox SecureSync;
		private Button BrowseButton;
		private string DefaultPath;
		private SimiasWebService simws;
		private iFolderWebService ifws;
		private DomainInformation domainInfo;
		private bool upload;
		private string defaultiFolderID;
        private int defaultTextWidth = 75;
        private int defaultTextXPos = 50;
        private int defaultTextYPos = 105;
        private int maxTextWidth = 440;
        private int defaultSpacing = 16;
        private int defaultValuePos = 125;
        private SizeF strSize;

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
            this.SecureSync = new CheckBox();
			this.BrowseButton = new Button();
			this.label1 = new Label();
            this.label2 = new Label();
            this.strSize = new SizeF();
            this.SuspendLayout();

            //	row1
			// CreateDefault
			//
            this.CreateDefault.Location = new Point(this.defaultTextXPos - 10, this.defaultTextYPos);
			this.CreateDefault.Text = this.resManager.GetString("CreateDefaultiFolder");//"Create Default iFolder";
			this.CreateDefault.TabIndex = 1;
			this.CreateDefault.CheckedChanged += new EventHandler(CreateDefault_CheckedChanged);
            this.CreateDefault.AutoSize = true;
            
			// row2
			// locationentry, location label, browse button
			//
            this.label1.Location = new Point(this.defaultTextXPos - 10, this.CreateDefault.Location.Y + this.CreateDefault.Size.Height + this.defaultSpacing);
			this.label1.Name = "label1";
			this.label1.Text = this.resManager.GetString("LocationText") + " "; //"Location:"+ " ";
            this.label1.AutoSize = true;
            // 
			// locationentry
			//
            this.LocationEntry.Location = new Point(this.defaultValuePos, this.label1.Location.Y - 2);
			this.LocationEntry.Multiline = false;
			this.LocationEntry.Size = new Size(280, 20);
			this.LocationEntry.TabIndex = 2;
			this.LocationEntry.Text = string.Empty;
			//
			// browsebutton
			//
            this.BrowseButton.Location = new Point(this.defaultValuePos + this.LocationEntry.Width + 4, this.label1.Location.Y - 4);
            this.BrowseButton.Text = this.resManager.GetString("BrowseText"); //"Browse";			
            this.BrowseButton.TabIndex = 3;
            this.BrowseButton.Click += new EventHandler(BrowseButton_Click);
            Graphics graphics = this.BrowseButton.CreateGraphics();
            int width = Convert.ToInt32(graphics.MeasureString(this.BrowseButton.Text, this.BrowseButton.Font).Width) + 3;
            if (width > this.BrowseButton.Width)
            {
                /// Adjusting the length of the location text box based on the size of the browse button...
                this.LocationEntry.Width -= width - this.BrowseButton.Width;
                this.BrowseButton.Location = new Point(this.defaultValuePos + this.LocationEntry.Width + 4, this.label1.Location.Y - 4);
                this.BrowseButton.Width = width;
            }
            graphics.Dispose();
			// row 3
			// encryptioncheckbutton, ssl checkbutton
			//
            this.label2.Location = new Point(this.defaultTextXPos - 10, this.label1.Location.Y + this.label1.Size.Height + this.defaultSpacing);
            this.label2.Text = this.resManager.GetString("Security") + ": "; //"Security:";			
            this.label1.AutoSize = true;

            this.encryptionCheckButton.Location = new Point(this.defaultValuePos + 2, this.label2.Location.Y);
			this.encryptionCheckButton.Text = this.resManager.GetString("EncryptedText");//"Encrypted";
            this.encryptionCheckButton.TabIndex = 4;
            
            this.sslCheckButton.Location = new Point(this.defaultValuePos + this.encryptionCheckButton.Width + 10, this.label2.Location.Y);
            this.sslCheckButton.Text = this.resManager.GetString("SharableText");//"Shared";
            this.sslCheckButton.TabIndex = 5;

            this.SecureSync.Location = new Point(this.sslCheckButton.Location.X + this.sslCheckButton.Width + 10, this.label2.Location.Y - 3);
            this.SecureSync.Text = this.resManager.GetString("SecureSync"); //Secure Sync
            this.SecureSync.TabIndex = 6;

            this.Controls.Add(this.CreateDefault);
			this.Controls.Add(this.LocationEntry);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.BrowseButton);
			this.Controls.Add(this.encryptionCheckButton);
			this.Controls.Add(this.sslCheckButton);
            this.Controls.Add(this.SecureSync);
			this.ResumeLayout(false);
            this.PerformLayout();
             
            this.Load += new EventHandler(DefaultiFolderPage_Load);            
        }
		#endregion

		#region Event Handlers

		#endregion

		#region Overridden Methods

        /// <summary>
        /// Activate Page
        /// </summary>
        /// <param name="previousIndex">previous Page</param>
        internal override void ActivatePage(int previousIndex)
		{
			base.ActivatePage (previousIndex);

			// Enable/disable the buttons.
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
        /// <param name="currentIndex">current index</param>
        internal override int ValidatePage(int currentIndex)
		{            
			// TODO:
			bool status = false;
            System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(CreateiFolder));
			if( this.CreateDefault.Checked == false)
				status = true;
			else if( upload ) // Create iFolder
			{
                if (ifws.GetLimitPolicyStatus(domainInfo.ID) == 1)
                {
                    iFolderWeb ifolder = null;
                    ifolder = CreateDefaultiFolder(this.sslCheckButton.Checked);
                    if (ifolder != null)
                    {
                        status = true;
                        this.simws.DefaultAccount(domainInfo.ID, ifolder.ID);
                    }
                }
                else
                {
                    MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("ifolderlimiterror"), resourceManager.GetString("errorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
                    mmb.ShowDialog();
                    status = true;
                }
			}
			else	// Download iFolder
			{
				iFolderWeb ifolder= this.ifws.GetMinimaliFolder( this.defaultiFolderID, 1 );
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
        /// Gets / Sets the default Path
        /// </summary>
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

        /// <summary>
        /// Gets / Sets the default iFolder ID
        /// </summary>
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
		/// <summary>
		/// Event Handler for create default checked changed event
		/// </summary>
		private void CreateDefault_CheckedChanged(object sender, EventArgs e)
		{
			if( this.CreateDefault.Checked == true)
			{
				this.label1.Enabled = true;
				this.label2.Enabled = true;
				this.sslCheckButton.Enabled = this.encryptionCheckButton.Enabled = true;
				this.LocationEntry.Enabled = this.BrowseButton.Enabled = this.SecureSync.Enabled = true;
                if( domainInfo.HostUrl.StartsWith(Uri.UriSchemeHttps))
                {
                    this.SecureSync.Checked = true;
                    this.SecureSync.Enabled = false;
                }

                //update agin about encryption or regular based on policy setting
                int SecurityPolicy = this.ifws.GetSecurityPolicy(domainInfo.ID);
                this.encryptionCheckButton.Checked = true;
                this.encryptionCheckButton.Enabled = this.sslCheckButton.Enabled = false;
                this.sslCheckButton.Checked = false;
                if (SecurityPolicy != 0)
                {
                    if ((SecurityPolicy & (int)SecurityState.encryption) == (int)SecurityState.encryption)
                    {
                        if ((SecurityPolicy & (int)SecurityState.enforceEncryption) == (int)SecurityState.enforceEncryption)
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
				this.label1.Enabled = false;
				this.label2.Enabled = false;
				this.sslCheckButton.Enabled = this.SecureSync.Enabled = this.encryptionCheckButton.Enabled = false;
				this.LocationEntry.Enabled = this.BrowseButton.Enabled = this.SecureSync.Checked = false;
			}
		}

        /// <summary>
        /// Download iFolder
        /// </summary>
        /// <param name="defaultiFolder"></param>
        private bool DownloadiFolder(iFolderWeb defaultiFolder)
        {
            bool status = false;
            if (defaultiFolder.encryptionAlgorithm == null || defaultiFolder.encryptionAlgorithm == "")
            {
                // unencrypted...
                status = true;
            }
            else
            {
                // encrypted.. Check for passphrase
                string passphrasecheck = null;
                passphrasecheck = simws.GetPassPhrase(domainInfo.ID);
                if (passphrasecheck == null || passphrasecheck == "")
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
            if (status == true)
            {
                try
                {
                    string downloadpath = this.LocationEntry.Text;
                    DirectoryInfo di = new DirectoryInfo(downloadpath);
                    if (di.Name == defaultiFolder.Name)
                    {
                        downloadpath = Directory.GetParent(this.LocationEntry.Text).ToString();
                        di = new DirectoryInfo(downloadpath);
                    }

                    di.Create();
                    iFolderWeb ifolder = null;
                    if (System.IO.Directory.Exists(Path.Combine(downloadpath, defaultiFolder.Name)))
                    {
                        MyMessageBox mmb = new MyMessageBox(resManager.GetString("selectoption"), resManager.GetString("alreadyexists"), String.Empty, MyMessageBoxButtons.OKCancel, MyMessageBoxIcon.Question, MyMessageBoxDefaultButton.Button1);
                        if (mmb.ShowDialog() == DialogResult.OK)
                        {
                            ifolder = this.ifws.MergeiFolder(defaultiFolder.DomainID, defaultiFolder.ID, Path.Combine(downloadpath, defaultiFolder.Name));
                        }
                        else
                            return false;
                    }
                    else
                        ifolder = this.ifws.AcceptiFolderInvitation(defaultiFolder.DomainID, defaultiFolder.ID, downloadpath);
                    AccountWizard wiz = (AccountWizard)this.Parent;
                    if (ifolder != null && wiz != null)
                    {
                        wiz.GlobalProps.AddiFolderToAcceptediFolders(ifolder, null, downloadpath);
                    }
                    wiz.UpdateDisplay(ifolder, Path.Combine(downloadpath, ifolder.Name));
                }
                catch (Exception ex)
                {
                    DisplayErrorMesg(ex);
                    return false;
                }
                return true;
            }
            else return status;
        }

        /// <summary>
        /// Create Default iFolder
        /// </summary>
        /// <param name="shared">true if shared else encryoted</param>
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
					//ifolder = this.ifws.CreateiFolderInDomain(this.LocationEntry.Text, this.domainInfo.ID);
                    ifolder = this.ifws.CreateiFolderInDomainEncr(this.LocationEntry.Text, domainInfo.ID, this.SecureSync.Checked,null,null);
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
					EnterPassphraseDialog enterPassPhrase= new EnterPassphraseDialog(domainInfo.ID, this.simws,this.ifws);
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
						ifolder = this.ifws.CreateiFolderInDomainEncr(this.LocationEntry.Text, domainInfo.ID,this.SecureSync.Checked, algorithm, Passphrase);
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
	
        /// <summary>
        /// Display Error MEssage
        /// </summary>
        /// <param name="ex">Exception</param>
		private void DisplayErrorMesg( Exception ex)
		{
			MyMessageBox mmb;
			string message;
			System.Resources.ResourceManager resourceManager =new System.Resources.ResourceManager(typeof(CreateiFolder));
			System.Resources.ResourceManager resourcemanager = new System.Resources.ResourceManager(typeof(GlobalProperties));
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
				message = resourcemanager.GetString("pathExistsError");
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

        /// <summary>
        /// Event Handler for default iFolder load event
        /// </summary>
        private void DefaultiFolderPage_Load(object sender, EventArgs e)
        {
            Graphics graphics = CreateGraphics();

            this.strSize = graphics.MeasureString(this.CreateDefault.Text, this.CreateDefault.Font);
            this.CreateDefault.Size = new Size(this.maxTextWidth, ((int)this.strSize.Width / this.maxTextWidth + 1) * 20);
            this.strSize = graphics.MeasureString(this.label1.Text, this.label1.Font);
            this.label1.Size = new Size(this.defaultTextWidth, ((int)this.strSize.Width / this.defaultTextWidth + 1) * 16);

            this.strSize = graphics.MeasureString(this.BrowseButton.Text, this.BrowseButton.Font);
            //this.BrowseButton.Size = new Size(80, ((int)this.strSize.Width / 80 + 1) * 16 + 10);

            int maxheight = 0;
            this.strSize = graphics.MeasureString(this.label2.Text, this.label2.Font);
            maxheight = Math.Max(maxheight, ((int)this.strSize.Width / this.defaultTextWidth + 1));

            this.strSize = graphics.MeasureString(this.encryptionCheckButton.Text, this.encryptionCheckButton.Font);
            maxheight = Math.Max(maxheight, ((int)this.strSize.Width / this.defaultTextWidth + 1));

            this.strSize = graphics.MeasureString(this.sslCheckButton.Text, this.sslCheckButton.Font);
            maxheight = Math.Max(maxheight, ((int)this.strSize.Width / this.defaultTextWidth + 1));

            this.strSize = graphics.MeasureString(this.SecureSync.Text, this.SecureSync.Font);
            maxheight = Math.Max(maxheight, ((int)this.SecureSync.Width / this.defaultTextWidth + 1));
            maxheight *= 18;

            this.encryptionCheckButton.Size = new Size(this.defaultTextWidth, maxheight);
            this.sslCheckButton.Size = new Size(this.defaultTextWidth, maxheight);
            this.SecureSync.Size = new Size(this.SecureSync.Width, maxheight);
            this.label2.Size = new Size(this.defaultTextWidth, maxheight);

            this.label2.TextAlign = ContentAlignment.MiddleLeft;
            this.SecureSync.TextAlign = ContentAlignment.MiddleLeft;
            this.encryptionCheckButton.TextAlign = ContentAlignment.MiddleLeft;
            this.sslCheckButton.TextAlign = ContentAlignment.MiddleLeft;
            graphics.Dispose();
			this.CreateDefault.Checked = true;
			string str = "";
			str = this.simws.GetDefaultiFolder( this.domainInfo.ID);
			if( str == null || str == "")
			{
                this.SecureSync.Visible = true;
				this.upload = true;				
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
				this.CreateDefault.Text = this.resManager.GetString("DownloadDefaultiFolder");
				this.label2.Visible = this.encryptionCheckButton.Visible = this.sslCheckButton.Visible = this.SecureSync.Visible = false;
				
			}
			if( this.DefaultPath != null && this.DefaultPath.Length > 0)
			{
				this.LocationEntry.Text = this.DefaultPath;
			}
		}

        /// <summary>
        /// Event Handler for Browse Button Click event
        /// </summary>
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





