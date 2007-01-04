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

using Novell.FormsTrayApp;
using Novell.iFolderCom;
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
		DomainInformation domainInfo;
		private string UserName;
		private string location;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Button next;
		private System.Windows.Forms.Button back;
		internal System.Windows.Forms.GroupBox groupBox1;
		private MigrationWelcomePage welcomePage;
	//	private WelcomePage welcomePage;
		private MigrationServerPage serverPage;
		private MigrationIdentityPage identityPage;
		private MigrationVerifyPage verifyPage;
		private MigrationCompletionPage completionPage;
	//	private IdentityPage identityPage;
	//	private VerifyPage verifyPage;
	//	private CompletionPage completionPage;
		private MigrationBaseWizardPage[] pages;
		internal const int maxPages = 5;
		private int currentIndex = 0;
		private iFolderWebService ifws;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		/// <summary>
		/// Constructs an MigrationWizard object.
		/// </summary>
		public MigrationWizard( string Uname, string path, iFolderWebService ifws )
		{
			//
			// Required for Windows Form Designer support
			//
			this.UserName = Uname;
			this.location = path;
			this.ifws = ifws;
			InitializeComponent();
						
			// Initialize the wizard pages ... I had to move this here so that
			// dev studio wouldn't wipe it out (when it was in InitializeComponent()).
			this.welcomePage = new MigrationWelcomePage();
			

			this.serverPage = new MigrationServerPage(path);
			this.identityPage = new MigrationIdentityPage(ifws);
			this.verifyPage = new MigrationVerifyPage();
			this.completionPage = new MigrationCompletionPage();
			//
			// welcomePage
			// 
			// TODO: Localize
			
			this.welcomePage.DescriptionText = "This wizard will guide you through migrating your iFolder account to 3.x.";
			this.welcomePage.ActionText = "To continue, click Next.";
			this.welcomePage.Location = new System.Drawing.Point(0, 0);
			this.welcomePage.Name = "welcomePage";
			this.welcomePage.Size = new System.Drawing.Size(496, 304);
			this.welcomePage.TabIndex = 1;
			this.welcomePage.WelcomeTitle = "Welcome to the iFolder Migration Wizard";
			
		
			// 
			// serverPage
			// 
			// TODO: Localize
			
			this.serverPage.HeaderSubTitle = "Enter the name of your iFolder server.";
			this.serverPage.HeaderTitle = "Choose an iFolder Server";
			this.serverPage.Location = new System.Drawing.Point(0, 0);
			this.serverPage.Name = "serverPage";
			this.serverPage.Size = new System.Drawing.Size(496, 304);
			this.serverPage.TabIndex = 1;
			
			// 
			// identityPage
			// 
			// TODO: Localize
			this.identityPage.HeaderSubTitle = "Enter your iFolder username and password.";
			this.identityPage.HeaderTitle = "iFolder Account Information";
			this.identityPage.Location = new System.Drawing.Point(0, 0);
			this.identityPage.Name = "identityPage";
			this.identityPage.Size = new System.Drawing.Size(496, 304);
			this.identityPage.TabIndex = 1;
			
			// 
			// verifyPage
			// 
			// TODO: Localize
			this.verifyPage.HeaderSubTitle = "Verify the iFolder account information.";
			this.verifyPage.HeaderTitle = "Verify iFolder Account Information";
			this.verifyPage.Location = new System.Drawing.Point(0, 0);
			this.verifyPage.Name = "verifyPage";
			this.verifyPage.Size = new System.Drawing.Size(496, 304);
			this.verifyPage.TabIndex = 1;
			
			//
			// completionPage
			//
			// TODO: Localize
			this.completionPage.DescriptionText = "Description...";
			this.completionPage.Location = new System.Drawing.Point(0, 0);
			this.completionPage.Name = "completionPage";
			this.completionPage.Size = new System.Drawing.Size(496, 304);
			this.completionPage.TabIndex = 1;
			this.completionPage.WelcomeTitle = "Completing the iFolder Invitation Wizard";
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
			pages[3] = this.verifyPage;
			pages[4] = this.completionPage;
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
			//pages[0].Show();
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
			this.cancel.Text = "Cancel";
			// 
			// next
			// 
			this.next.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.next.Location = new System.Drawing.Point(328, 318);
			this.next.Name = "next";
			this.next.Size = new System.Drawing.Size(72, 23);
			this.next.TabIndex = 2;
			this.next.Text = "&Next >";
			this.next.Click += new System.EventHandler(this.next_Click);
			// 
			// back
			// 
			this.back.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.back.Location = new System.Drawing.Point(253, 318);
			this.back.Name = "back";
			this.back.Size = new System.Drawing.Size(72, 23);
			this.back.TabIndex = 1;
			this.back.Text = "< &Back";
			this.back.Click += new System.EventHandler(this.back_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Location = new System.Drawing.Point(0, 302);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(496, 4);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "groupBox1";
			// 
			// MigrationWizard
			// 
			this.AcceptButton = this.next;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(496, 348);
			this.ControlBox = false;
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.back);
			this.Controls.Add(this.next);
			this.Controls.Add(this.cancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MigrationWizard";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "iFolder Account Wizard";
			this.Activated += new System.EventHandler(this.MigrationWizard_Activated);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers

		private void MigrationWizard_Activated(object sender, System.EventArgs e)
		{
			Novell.CustomUIControls.ShellNotifyIcon.SetForegroundWindow(Handle);
		}

		private void back_Click(object sender, System.EventArgs e)
		{
			// Check if we're on the completion page.
			if ((currentIndex == (maxPages - 1)) ||
				(currentIndex == (maxPages - 2)))
			{
				// TODO: Localize
				this.next.Text = "&Next >";
			}

			int previousIndex = this.pages[currentIndex].DeactivatePage();
			this.pages[previousIndex].ActivatePage(0);

			currentIndex = previousIndex;
		}
/*
		private void connecting_EnterpriseConnect(object sender, DomainConnectEventArgs e)
		{
			if (EnterpriseConnect != null)
			{
				// Fire the event telling that a new domain has been added.
				EnterpriseConnect(this, new DomainConnectEventArgs( e.DomainInfo ));
			}
		}
*/
		private void next_Click(object sender, System.EventArgs e)
		{
			// Check if we're on the last page.
			if (currentIndex == (maxPages - 1))
			{
				// Exit
				return;
			}

			int nextIndex = this.pages[currentIndex].ValidatePage(currentIndex);
			if (nextIndex != currentIndex)
			{
				this.pages[currentIndex].DeactivatePage();
				this.pages[nextIndex].ActivatePage(currentIndex);

				currentIndex = nextIndex;

				if (currentIndex == (maxPages - 2))
				{
					// TODO: Localize
					next.Text = "&Connect";
					this.verifyPage.UpdateDetails();
				}
				else if (currentIndex == (maxPages - 1))
				{
					// We're on the completion page ... change the Next 
					// button to a Finish button.
					next.DialogResult = DialogResult.OK;
					// TODO: Localize
					next.Text = "&Finish";
				}
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Delegate used when successfully connected to Enterprise Server.
		/// </summary>
		public delegate void EnterpriseConnectDelegate(object sender, DomainConnectEventArgs e);
		/// <summary>
		/// Occurs when successfully connected to enterprise.
		/// </summary>
		public event EnterpriseConnectDelegate EnterpriseConnect;

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
				// TODO: Localize
				StringBuilder sb = new StringBuilder("Congratulations, your account is migrated successfully to:\n\n");

		//		sb.AppendFormat( "{0}\n", domainInfo.Name );
		//		sb.AppendFormat( "({0})\n\n", serverPage.ServerAddress );
				sb.Append( "You can now add folders to the new server. You may also download folders from the server and have them be synchronized to your computer." );

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
				catch(Exception e)
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
			catch(Exception e)
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
			catch(Exception e)
			{
			}
			return false;
		}		


		/// <summary>
		/// Method that performs migration..
		/// 
		public bool MigrateFolder()
		{
			// TODO: Add code here to Migrate..
			/*
			 * Check if the destination folder can be an iFolder
			 * Copy the folder if needed
			 * If yes create the iFolder, else stay at the same page
			 * if the folder is to be removed from 2.x domain remove.
			 */
			DomainItem domain = this.MigrationIdentityPage.domain;
			bool encr = this.MigrationIdentityPage.Encrypion;
			bool ssl = this.MigrationIdentityPage.SSL;
			string encryptionAlgorithm = encr? "BlowFish" : "" ;
			int securityStatus = 0;
			securityStatus += encr ? 1:0;
			securityStatus += ssl ? 2:0;
			string destination;
			// Migration Option is true if the folder is to be removed from 2.x domain
			if( !this.MigrationServerPage.MigrationOption)
			{
				destination = this.MigrationServerPage.HomeLocation;
				if(ifws.CanBeiFolder(destination)== false)
					return false; // can't be an iFolder
				// Copy the 2.x folder to destination
				if(!CopyDirectory(new DirectoryInfo(location), new DirectoryInfo(destination)))
					return false;	// unable to copy..
			}
			else
			{
				destination = this.location;
				if(ifws.CanBeiFolder(destination) == false)
					return false;  // can't be an iFolder...
			}

			if( ifws.CreateiFolderInDomainEncr(destination, domain.ID, ssl, encryptionAlgorithm) == null)
				return false;	// error creating iFolder
			if(this.MigrationServerPage.MigrationOption)
			{
				// TODO: Remove from server..(remove registry contents)
				string iFolderRegistryKey = @"Software\Novell iFolder";
				RegistryKey iFolderKey = Registry.LocalMachine.OpenSubKey(iFolderRegistryKey, true);
					try
					{
						iFolderKey.DeleteSubKeyTree(UserName);
					}
					catch(Exception ex)
					{
						Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox("Changed error msg", "Not able to delete key", ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
						mmb.Dispose();
					}
			}			
			return true;
		}

		/// <summary>
		/// Method used to connect to the Simias server.
		/// </summary>
		/// <returns><b>True</b> if successfully connected; otherwise, <b>False</b> is returned.</returns>
		/*
		public bool ConnectToServer()
		{
			bool result = false;

			Connecting connecting = new Connecting( simiasWebService, simiasManager, serverPage.ServerAddress, identityPage.Username, identityPage.Password, serverPage.DefaultServer, identityPage.RememberPassword );
			connecting.EnterpriseConnect += new Novell.FormsTrayApp.Connecting.EnterpriseConnectDelegate(connecting_EnterpriseConnect);
			if ( connecting.ShowDialog() == DialogResult.OK )
			{
				domainInfo = connecting.DomainInformation;
				result = true;
			}

			return result;
		}
		*/

		#endregion
	}
}
