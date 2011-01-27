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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Net;
using Microsoft.Win32;
using System.Security.Permissions;

using Novell.iFolder.Web;
using Novell.FormsTrayApp;
using Novell.iFolderCom;
using Novell.Win32Util;
using Simias.Client;
using Simias.Client.Authentication;
using Simias.Client.Event;

[assembly: RegistryPermissionAttribute(SecurityAction.RequestMinimum,
Write = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Novell iFolder")]

namespace Novell.Wizard
{
	/// <summary>
	/// Enumeration used to tell which wizard buttons are active.
	/// </summary>
	[Flags]
	public enum MigrationWizardButtons
	{
		/// <summary>
		/// None of the buttons are enabled.
		/// </summary>
		None = 0,

		/// <summary>
		/// The Back button is enabled.
		/// </summary>
		Back = 1,

		/// <summary>
		/// The Next button is enabled.
		/// </summary>
		Next = 2,

		/// <summary>
		/// The Cancel button is enabled.
		/// </summary>
		Cancel = 4,
	}

	/// <summary>
	/// Summary description for MigrationWizard.
	/// </summary>
	public class MigrationWizard : System.Windows.Forms.Form
	{
		#region Class Members
		//DomainInformation domainInfo;
		private string UserName;
		private string location;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Button next;
		private System.Windows.Forms.Button back;
		private System.Windows.Forms.Button btnHelp;
		internal System.Windows.Forms.GroupBox groupBox1;
		private MigrationWelcomePage welcomePage;
	//	private WelcomePage welcomePage;
		private MigrationServerPage serverPage;
		private MigrationIdentityPage identityPage;
		private MigrationPassphrasePage passphrasePage;
		private MigrationPassphraseVerifyPage passphraseVerifyPage;
		private MigrationVerifyPage verifyPage;
		private MigrationCompletionPage completionPage;
	//	private IdentityPage identityPage;
	//	private VerifyPage verifyPage;
	//	private CompletionPage completionPage;
		private MigrationBaseWizardPage[] pages;
		internal const int maxPages = 7;
		private int currentIndex = 0;
		private bool encryptedOriginal;
		private iFolderWebService ifws;
		private SimiasWebService simiasWebService;
		private static System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(Novell.FormsTrayApp.FormsTrayApp));

        /// <summary>
        /// Gets the simias web service
        /// </summary>
		public SimiasWebService simws
		{
			get
			{
				return this.simiasWebService;
			}
		}
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		/// <summary>
		/// Constructs an MigrationWizard object.
		/// </summary>
		public MigrationWizard( string Uname, string path, bool encryptedOriginal, iFolderWebService ifws, SimiasWebService simiasWebService )
		{
			//
			// Required for Windows Form Designer support
			//
			this.encryptedOriginal = encryptedOriginal;
			this.UserName = Uname;
			this.location = path;
			this.ifws = ifws;
			this.simiasWebService = simiasWebService;
			InitializeComponent();
						
			// Initialize the wizard pages ... I had to move this here so that
			// dev studio wouldn't wipe it out (when it was in InitializeComponent()).
			this.welcomePage = new MigrationWelcomePage();
			

			this.serverPage = new MigrationServerPage(path);
			this.identityPage = new MigrationIdentityPage(ifws);
			this.passphrasePage = new MigrationPassphrasePage();
			this.passphraseVerifyPage = new MigrationPassphraseVerifyPage();
			this.verifyPage = new MigrationVerifyPage();
			this.completionPage = new MigrationCompletionPage();
			//
			// welcomePage
			// 
			// TODO: Localize
			
			this.welcomePage.ActionText = Resource.GetString("MigrationWelcomepgAT");//"This wizard will guide you through migrating your iFolder account to 3.x.";
			this.welcomePage.DescriptionText = Resource.GetString("WelcomePageAction");//"To continue, click Next.";
			this.welcomePage.Location = new System.Drawing.Point(0, 0);
			this.welcomePage.Name = "welcomePage";
			this.welcomePage.Size = new System.Drawing.Size(496, 304);
			this.welcomePage.TabIndex = 1;
			this.welcomePage.WelcomeTitle = Resource.GetString("MigrationWelcomeTitle");//"Welcome to the iFolder Migration Assistant";
			
		
			// 
			// serverPage
			// 
			// TODO: Localize
			
			//this.serverPage.HeaderSubTitle = "Enter the name of your iFolder server.";
			this.serverPage.HeaderTitle = Resource.GetString("MigrationOptions");//"Migration Options";
			this.serverPage.Location = new System.Drawing.Point(0, 0);
			this.serverPage.Name = "serverPage";
			this.serverPage.Size = new System.Drawing.Size(496, 304);
			this.serverPage.TabIndex = 1;
			
			// 
			// identityPage
			// 
			// TODO: Localize
			//this.identityPage.HeaderSubTitle = "Enter your iFolder username and password.";
			this.identityPage.HeaderTitle = Resource.GetString("ServerInfo");//"Server Information";
			this.identityPage.Location = new System.Drawing.Point(0, 0);
			this.identityPage.Name = "identityPage";
			this.identityPage.Size = new System.Drawing.Size(496, 304);
			this.identityPage.TabIndex = 1;
			//
			// passphrasePage
			//
			this.passphrasePage.HeaderTitle = "Passphrase Entry";
			this.passphrasePage.Location = new Point(0, 0);
			this.passphrasePage.Name = "passphrasePage";
			this.passphrasePage.Size = new Size(496,304);
			this.passphrasePage.TabIndex = 1;
			//
			// passphraseVerifyPage
			//
			this.passphraseVerifyPage.HeaderTitle = "Passphrase";
			this.passphraseVerifyPage.Location = new Point(0, 0);
			this.passphraseVerifyPage.Size = new Size(496, 304);
			
			// 
			// verifyPage
			// 
			// TODO: Localize
			//this.verifyPage.HeaderSubTitle = "Verify the iFolder account information.";
			this.verifyPage.HeaderTitle = Resource.GetString("MigrateVerify");//"Verify and Migrate";
			this.verifyPage.Location = new System.Drawing.Point(0, 0);
			this.verifyPage.Name = "verifyPage";
			this.verifyPage.Size = new System.Drawing.Size(496, 304);
			this.verifyPage.TabIndex = 1;
			
			//
			// completionPage
			//
			// TODO: Localize
			this.completionPage.DescriptionText = Resource.GetString("CompletionPageDT");//"Description...";
			this.completionPage.Location = new System.Drawing.Point(0, 0);
			this.completionPage.Name = "completionPage";
			this.completionPage.Size = new System.Drawing.Size(496, 304);
			this.completionPage.TabIndex = 1;
			this.completionPage.WelcomeTitle = Resource.GetString("MigrationCompleteWT");//"Completing the iFolder Migration Wizard";
			/*
			this.Controls.Add(this.welcomePage);
			this.Controls.Add(this.serverPage);
			this.Controls.Add(this.identityPage);
			this.Controls.Add(this.verifyPage);
			this.Controls.Add(this.completionPage);
			*/
			this.Controls.Add(this.welcomePage);
			this.Controls.Add(this.serverPage);
			this.Controls.Add(this.identityPage);
			this.Controls.Add(this.passphrasePage);
			this.Controls.Add(this.passphraseVerifyPage);
			this.Controls.Add(this.verifyPage);
			this.Controls.Add(this.completionPage);
			// Load the application icon.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"ifolder_app.ico"));
			}
			catch {} // Ignore
		

			// Put the wizard pages in order.
			pages = new MigrationBaseWizardPage[maxPages];
			pages[0] = this.welcomePage;
			pages[1] = this.serverPage;
			pages[2] = this.identityPage;
			pages[3] = this.passphrasePage;
			pages[4] = this.passphraseVerifyPage;
			pages[5] = this.verifyPage;
			pages[6] = this.completionPage;
			/*
			pages[1] = this.serverPage;
			pages[2] = this.identityPage;
			pages[3] = this.verifyPage;
			pages[4] = this.completionPage;
			*/
			
			try
			{
				// Load the watermark.
				// TODO:
				Image image = Image.FromFile(Path.Combine(Application.StartupPath, "invitewiz.png"));
				this.welcomePage.Watermark = image;
				this.completionPage.Watermark = image;

				// TODO:
				image = Image.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder_invite_32.png"));
				this.serverPage.Thumbnail = image;
				this.verifyPage.Thumbnail = image;
				this.identityPage.Thumbnail = image;
			}
			catch {} // Ignore.

			for(int i=0;i<maxPages;i++)
				pages[i].Hide();

			// Activate the first wizard page.
			pages[0].ActivatePage(0);			
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.cancel = new System.Windows.Forms.Button();
			this.next = new System.Windows.Forms.Button();
			this.back = new System.Windows.Forms.Button();
			this.btnHelp = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.SuspendLayout();
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancel.Location = new System.Drawing.Point(416, 318);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(72, 23);
			this.cancel.TabIndex = 3;
			this.cancel.Text = Resource.GetString("CancelText");//"Cancel";
			// 
			// next
			// 
			this.next.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.next.Location = new System.Drawing.Point(328, 318);
			this.next.Name = "next";
			this.next.Size = new System.Drawing.Size(72, 23);
			this.next.TabIndex = 2;
			this.next.Text = Resource.GetString("NextText")+" >";
			this.next.Click += new System.EventHandler(this.next_Click);
			// 
			// back
			// 
			this.back.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.back.Location = new System.Drawing.Point(253, 318);
			this.back.Name = "back";
			this.back.Size = new System.Drawing.Size(72, 23);
			this.back.TabIndex = 1;
			this.back.Text = "< "+Resource.GetString("BackText");
			this.back.Click += new System.EventHandler(this.back_Click);
			// 
			// btnHelp
			// 
			this.btnHelp.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnHelp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.btnHelp.Location = new System.Drawing.Point(16, 318);
			this.btnHelp.Name = Resource.GetString("BackText");//"back";
			this.btnHelp.Size = new System.Drawing.Size(72, 23);
			this.btnHelp.TabIndex = 1;
			this.btnHelp.Text = Resource.GetString("menuHelp.Text");
			this.btnHelp.Click += new System.EventHandler(this.help_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(101)), ((System.Byte)(163)), ((System.Byte)(237)));
			this.groupBox1.Location = new System.Drawing.Point(0, 302);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(496, 4);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "";
			// 
			// MigrationWizard
			// 
			this.AcceptButton = this.next;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(496, 348);
			this.ControlBox = false;
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnHelp);
			this.Controls.Add(this.back);
			this.Controls.Add(this.next);
			this.Controls.Add(this.cancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MigrationWizard";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = Resource.GetString("MigrationTitle");//"iFolder Migration Assistant";
			this.Activated += new System.EventHandler(this.MigrationWizard_Activated);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers

        /// <summary>
        /// Event Handler for migration wizard activated event
        /// </summary>
        private void MigrationWizard_Activated(object sender, System.EventArgs e)
		{
			Novell.CustomUIControls.ShellNotifyIcon.SetForegroundWindow(Handle);
		}

        /// <summary>
        /// Event Handler for back click event
        /// </summary>
        private void back_Click(object sender, System.EventArgs e)
		{
			// Check if we're on the completion page.
			if ((currentIndex == (maxPages - 1)) ||
				(currentIndex == (maxPages - 2)))
			{
				this.next.Text = Resource.GetString("NextText")+" >";
			}
			
			int previousIndex = this.pages[currentIndex].DeactivatePage();
			this.pages[previousIndex].ActivatePage(0);
			currentIndex = previousIndex;
		}

        /// <summary>
        /// Event Handler for next click event
        /// </summary>
        private void next_Click(object sender, System.EventArgs e)
		{
			// Check if we're on the last page.
			if (currentIndex == (maxPages - 1))
			{
				// Exit
				return;
			}
			
			System.Resources.ResourceManager resManager = new System.Resources.ResourceManager(typeof(Connecting));
			
			if( currentIndex == 3 )// Set Passphrase
			{
				
				if( this.passphrasePage.Passphrase != this.passphrasePage.RetypePassphrase)
				{
					MessageBox.Show(Resource.GetString("TypeRetypeMisMatch"));
				}
				else
				{
					string publicKey = "";
					string ragent = null;
					if( this.passphrasePage.RecoveryAgent != null && this.passphrasePage.RecoveryAgent != "None")
					{
						// Show the certificate.....
						byte[] CertificateObj = this.simws.GetRACertificateOnClient(this.identityPage.domain.ID, this.passphrasePage.RecoveryAgent);
						System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(CertificateObj);
						//	MyMessageBox mmb = new MyMessageBox( "Verify Certificate", "Verify Certificate", cert.ToString(true), MyMessageBoxButtons.YesNo, MyMessageBoxIcon.Question, MyMessageBoxDefaultButton.Button2 );
						MyMessageBox mmb = new MyMessageBox( string.Format(resManager.GetString("verifyCert"), this.passphrasePage.RecoveryAgent), resManager.GetString("verifyCertTitle"), cert.ToString(true), MyMessageBoxButtons.YesNo, MyMessageBoxIcon.Question, MyMessageBoxDefaultButton.Button2);
						DialogResult messageDialogResult = mmb.ShowDialog();
						mmb.Dispose();
						mmb.Close();
						if( messageDialogResult != DialogResult.OK )
							return;
						else
						{
							ragent = this.passphrasePage.RecoveryAgent;
							publicKey = Convert.ToBase64String(cert.GetPublicKey());
						}
					}
					Status passPhraseStatus = null;
					try
					{
						passPhraseStatus = this.simiasWebService.SetPassPhrase( this.identityPage.domain.ID, this.passphrasePage.Passphrase, null, publicKey);
					}
					catch(Exception ex)
					{
						MessageBox.Show( Resource.GetString("IsPassphraseSetException")+ex.Message);
						return;
					}
					if(passPhraseStatus.statusCode == StatusCodes.Success)
					{
						this.simiasWebService.StorePassPhrase( this.identityPage.domain.ID, this.passphrasePage.Passphrase, CredentialType.Basic, this.passphrasePage.RememberPassphrase);
						Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(Resource.GetString("SetPassphraseSuccess")/*"Successfully set the passphrase"*/, "", "", MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
						mmb.ShowDialog();
						mmb.Dispose();
						this.Dispose();
						this.Close();
					}
					else 
					{
						// Unable to set the passphrase
						Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(Resource.GetString("IsPassphraseSetException")/*"Unable to set the passphrase"*/, ""/*"Error setting the passphrase"*/, ""/*Resource.GetString("TryAgain")*//*"Please try again"*/, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
						mmb.Dispose();
						return;
					}
				}
			}
			else if(currentIndex == 4)// Validate passphrase
			{				
				Status passPhraseStatus = null;
				try
				{
					passPhraseStatus =  this.simiasWebService.ValidatePassPhrase(this.identityPage.domain.ID, this.passphraseVerifyPage.Passphrase);
				}
				catch(Exception ex)
				{
					MessageBox.Show(resManager.GetString("ValidatePPError")/*"Unable to validate the Passphrase. {0}"*/, ex.Message);
					return;
				}
				if( passPhraseStatus != null)
				{
					if( passPhraseStatus.statusCode == StatusCodes.PassPhraseInvalid)  // check for invalid passphrase
					{
						Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(Resource.GetString("InvalidPPText")/*"Invalid the passphrase"*/, Resource.GetString("VerifyPP")/*"Passphrase Invalid"*/, ""/*Resource.GetString("TryAgain")*//*"Please try again"*/, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
						mmb.Dispose();	
						return;
					}
					else if(passPhraseStatus.statusCode == StatusCodes.Success)
					{
						try
						{
							this.simiasWebService.StorePassPhrase( this.identityPage.domain.ID, this.passphraseVerifyPage.Passphrase, CredentialType.Basic, this.passphraseVerifyPage.RememberPassphrase);
						}
						catch(Exception ) 
						{
							MessageBox.Show("Unable to store Passphrase");
							return;
						}
					}
				}
			}
			
			int nextIndex = this.pages[currentIndex].ValidatePage(currentIndex);
			if( nextIndex == 4 )
			{
				// Set the passphrase
				nextIndex = 5;
			}
			else if( nextIndex == 3)
			{
				if( this.identityPage.Encrypion == false )
				{
					// if 2.x is encrypted make a prompt
					if( this.encryptedOriginal == true )
					{
						MyMessageBox mmb1 = new MyMessageBox(Resource.GetString("EncryptTotext"), Resource.GetString("MigrationAlert"), "", MyMessageBoxButtons.YesNo, MyMessageBoxIcon.Warning, MyMessageBoxDefaultButton.Button1);
						DialogResult res = mmb1.ShowDialog();
						if( res == DialogResult.No )
							nextIndex = currentIndex;
						else
							nextIndex = 5;
					}
					else
						nextIndex = 5;
				}
				else // encryption selected.. 
				{
					try
					{
						string passphrasecheck = this.simiasWebService.GetPassPhrase(this.identityPage.domain.ID);
						if( passphrasecheck!= null && passphrasecheck != "")
						{
							Status status = this.simiasWebService.ValidatePassPhrase(this.identityPage.domain.ID, passphrasecheck);
							if( status != null && status.statusCode == StatusCodes.Success)
							{
								// Passphrase validated.
								nextIndex = 5;
							}
						}
						else if(this.simiasWebService.IsPassPhraseSet(this.identityPage.domain.ID) == true)
						{
							//MessageBox.Show("passphrase set");
							nextIndex = 4;
						}						
					}
					catch(Exception )
					{
						MessageBox.Show("Unable to get passphrase. \nLogin to the domain and try again.");
						// Stay in the same page
						nextIndex = currentIndex;
					}
				}

			}
			
			if (nextIndex != currentIndex)
			{
				this.pages[currentIndex].DeactivatePage();
				this.pages[nextIndex].ActivatePage(currentIndex);
				if( nextIndex == 5)
				{
					this.pages[nextIndex].PreviousIndex = 2;
				}

				currentIndex = nextIndex;

				if (currentIndex == (maxPages - 2))
				{
					next.Text = Resource.GetString("MigrateText");
					this.verifyPage.UpdateDetails();
				}
				else if (currentIndex == (maxPages - 1))
				{
					// We're on the completion page ... change the Next 
					// button to a Finish button.
					next.DialogResult = DialogResult.OK;
					next.Text = Resource.GetString("FinishText");//"&Finish";
				}
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the identity page.
		/// </summary>
		
		public MigrationIdentityPage MigrationIdentityPage
		{
			get { return identityPage; }
		}

		/// <summary>
		/// Gets the maximum pages in the wizard.
		/// </summary>
		public int MaxPages
		{
			get { return maxPages; }
		}

		/// <summary>
		/// Gets the server page.
		/// </summary>
		public MigrationServerPage MigrationServerPage
		{
			get { return serverPage; }
		}
		
		/// <summary>
		/// Gets the summary text.
		/// </summary>
		
		public string SummaryText
		{
			get
			{
				StringBuilder sb = new StringBuilder(Resource.GetString("MigrationSuccessMsg")/*"Congratulations, your account is migrated successfully.\n\n"*/);
				return sb.ToString();				
			}
		}
		
		/// <summary>
		/// Gets or sets flags determining which wizard buttons are enabled or disabled.
		/// </summary>
		public MigrationWizardButtons MigrationWizardButtons
		{
			get
			{
				MigrationWizardButtons migrationWizardButtons = MigrationWizardButtons.None;
				migrationWizardButtons |= cancel.Enabled ? MigrationWizardButtons.Cancel : MigrationWizardButtons.None;
				migrationWizardButtons |= next.Enabled ? MigrationWizardButtons.Next : MigrationWizardButtons.None;
				migrationWizardButtons |= back.Enabled ? MigrationWizardButtons.Back : MigrationWizardButtons.None;
				return migrationWizardButtons;
			}
			set
			{
				MigrationWizardButtons migrationWizardButtons = value;
				cancel.Enabled = ((migrationWizardButtons & MigrationWizardButtons.Cancel) == MigrationWizardButtons.Cancel);
				next.Enabled = ((migrationWizardButtons & MigrationWizardButtons.Next) == MigrationWizardButtons.Next);
				back.Enabled = ((migrationWizardButtons & MigrationWizardButtons.Back) == MigrationWizardButtons.Back);
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		///  Method for copying data from one directory to another
		/// </summary>
		/// <param name="source" description ="source directory"></param>
		/// <param name="destination" description = "destination directory"></param>
		public bool CopyDirectory(DirectoryInfo source, DirectoryInfo destination) 
		{
			if (!destination.Exists) 
			{
				try
				{
					destination.Create();
				}
				catch(Exception )
				{
				}
			}
			if(!source.Exists)
			{
				return false;
			}
			// Copy all files.
			try
			{
				FileInfo[] files = source.GetFiles();
				foreach (FileInfo file in files) 
				{
					file.CopyTo(System.IO.Path.Combine(destination.FullName, file.Name));
				}
			}
			catch(Exception )
			{
			}
			try
			{
				// Process sub-directories.
				DirectoryInfo[] dirs = source.GetDirectories();
				foreach (DirectoryInfo dir in dirs) 
				{
					// Get destination directory.
					string destinationDir = System.IO.Path.Combine(destination.FullName, dir.Name);
					// Call CopyDirectory() recursively.
					CopyDirectory(dir, new DirectoryInfo(destinationDir));
				}
				return true;
			}
			catch(Exception )
			{
			}
			return false;
		}		

		/// <summary>
		/// Method that performs migration..
		/// </summary>
		public bool MigrateFolder()
		{
			/*
			 * Check if the destination folder can be an iFolder
			 * Copy the folder if needed
			 * If yes create the iFolder, else stay at the same page
			 * if the folder is to be removed from 2.x domain remove.
			 */
			DomainItem domain = this.MigrationIdentityPage.domain;
			bool shared = this.MigrationIdentityPage.SSL;
			string encryptionAlgorithm = this.MigrationIdentityPage.Encrypion? "BlowFish" : "" ;
			string destination;
			
			// Migration Option is true if the folder is to be removed from 2.x domain
			try
			{
				if(this.MigrationServerPage.MigrationOption == false)
				{
					destination = this.MigrationServerPage.HomeLocation;
					DirectoryInfo dir = new DirectoryInfo(destination);
					if( dir.Exists == false)
					{
						this.verifyPage.CloseWaitDialog();
						MessageBox.Show(Resource.GetString("ErrDirCreate"));						
						return false;
					}
					
					// Create the ifolder directory
					if( this.MigrationServerPage.CopyParentDirectory)
					{
						DirectoryInfo di = new DirectoryInfo(this.location);
						destination = destination+"\\"+di.Name;
						di = new DirectoryInfo(destination);
						if( di.Exists )
						{
							this.verifyPage.CloseWaitDialog();
							MessageBox.Show(Resource.GetString("DirExists")/*"The directory exists already"*/, Resource.GetString("MigrationTitle"), MessageBoxButtons.OK);
							return false;
						}
						else
						{
							try
							{
								di.Create();
							}
							catch(Exception ex)
							{
								this.verifyPage.CloseWaitDialog();
								MessageBox.Show(ex.ToString(), Resource.GetString("ErrDirCreate"), MessageBoxButtons.OK);
								return false;
							}
						}
					}
					//Check that the final path is already 3.6 ifolder, we don't do a 2.x check here
					if(ifws.CanBeiFolder(destination)== false)
					{	
						this.verifyPage.CloseWaitDialog();
						MessageBox.Show(Resource.GetString("CannotBeiFolder")/*"The folder can not be converted into ifolder"*/,Resource.GetString("MigrationTitle")/*"Error creating iFolder"*/,MessageBoxButtons.OK);
						return false; // can't be an iFolder
					}

					// Copy the contents
					if(!CopyDirectory(new DirectoryInfo(location), new DirectoryInfo(destination)))
					{
						this.verifyPage.CloseWaitDialog();
						MessageBox.Show(Resource.GetString("CannotCopy")/*"Unable to copy the folder"*/, Resource.GetString("MigrationTitle")/*"Error copying the folder"*/, MessageBoxButtons.OK);
						return false;	// unable to copy..
					}				
				}
				else
				{
					destination = this.location;
					if(ifws.CanBeiFolder(destination)== false)	// Display a message box
					{	
						this.verifyPage.CloseWaitDialog();
						MessageBox.Show(Resource.GetString("CannotBeiFolder")/*"The folder can not be converted into ifolder"*/,Resource.GetString("MigrationTitle")/*"Error creating iFolder"*/,MessageBoxButtons.OK);
						return false; // can't be an iFolder
					}
				}
				
				if(shared)
				{
					if( ifws.CreateiFolderInDomain(destination, domain.ID) == null)
					{
						this.verifyPage.CloseWaitDialog();
						MessageBox.Show(Resource.GetString("MigrationConvert")/*Unable to convert to an iFolder*/, Resource.GetString("MigrationTitle")/*"Error creating iFolder"*/, MessageBoxButtons.OK);
						return false;
					}
				}
				else
				{
					string passphrase = this.simiasWebService.GetPassPhrase(this.identityPage.domain.ID);
					if( ifws.CreateiFolderInDomainEncr(destination, domain.ID, false, encryptionAlgorithm, passphrase) == null)
					{
						this.verifyPage.CloseWaitDialog();
						MessageBox.Show(Resource.GetString("MigrationConvert")/*Unable to convert to an iFolder*/, Resource.GetString("MigrationTitle")/*"Error creating iFolder"*/, MessageBoxButtons.OK);
						return false;
					}
				}
			}
			catch(Exception )
			{
				this.verifyPage.CloseWaitDialog();
				MessageBox.Show(Resource.GetString("CannotBeiFolder")/*"The folder can not be converted into ifolder"*/,Resource.GetString("MigrationTitle")/*"Error creating iFolder"*/,MessageBoxButtons.OK);
				return false;
			}
			
			if(this.MigrationServerPage.MigrationOption == true)
			{
				//remove the 2.x registry entry for the migarted (not copy) ifolder
				string iFolderRegistryKey = @"Software\Novell iFolder";
				RegistryKey iFolderKey = Registry.LocalMachine.OpenSubKey(iFolderRegistryKey, true);
				try
				{
					iFolderKey.DeleteSubKeyTree(UserName);
				}
				catch(Exception ex)
				{
					this.verifyPage.CloseWaitDialog();
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(ex.Message, Resource.GetString("MigrationTitle"),"", MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
					mmb.Close();
				}
			}	
			return true;
		}
		
        /// <summary>
        /// FixPath
        /// </summary>
        /// <param name="path">string path</param>
        /// <returns>string Path which is fixed</returns>
		public string FixPath(string path)
		{
			if (path[1].Equals(':'))
			{
				string root = path.Substring(0, 2);
				path = path.Replace(root, root.ToUpper());
			}

			try
			{
				string parent = path;
				string temp = string.Empty;
				while (true)
				{
					string file = Path.GetFileName(parent);
					parent = Path.GetDirectoryName(parent);
					if ((parent == null) || parent.Equals(string.Empty))
					{
						string psub = path.Substring(3);
						if (string.Compare(psub, temp, true) == 0)
							path = path.Replace(psub, temp);
						break;
					}

					string[] dirs = Directory.GetFileSystemEntries(parent, file);
					if (dirs.Length == 1)
					{
						temp = Path.Combine(Path.GetFileName(dirs[0]), temp);
					}
				}
			}
			catch {}

			return path;
		}
		

		#endregion
        /// <summary>
        /// Event Handler for help click event
        /// </summary>
        private void help_Click( object o, EventArgs args)
		{
			string helpFile = Path.Combine(Path.Combine(Path.Combine(Application.StartupPath, "help"), iFolderAdvanced.GetLanguageDirectory()), @"migration.html");
			new iFolderComponent().ShowHelp(Application.StartupPath, helpFile);
		}
	}
}
