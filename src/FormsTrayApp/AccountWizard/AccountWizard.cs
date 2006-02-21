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

using Novell.FormsTrayApp;
using Novell.iFolderCom;
using Simias.Client;
using Simias.Client.Authentication;
using Simias.Client.Event;

namespace Novell.Wizard
{
	/// <summary>
	/// Enumeration used to tell which wizard buttons are active.
	/// </summary>
	[Flags]
	public enum WizardButtons
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
	/// Summary description for AccountWizard.
	/// </summary>
	public class AccountWizard : System.Windows.Forms.Form
	{
		#region Class Members
//		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(InvitationWizard));
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Button next;
		private System.Windows.Forms.Button back;
		internal System.Windows.Forms.GroupBox groupBox1;
		private WelcomePage welcomePage;
		private ServerPage serverPage;
		private IdentityPage identityPage;
		private VerifyPage verifyPage;
		private CompletionPage completionPage;
		private BaseWizardPage[] pages;
		internal const int maxPages = 5;
		private int currentIndex = 0;
		private Manager simiasManager;
		private SimiasWebService simiasWebService;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		/// <summary>
		/// Constructs an AccountWizard object.
		/// </summary>
		public AccountWizard( SimiasWebService simiasWebService, Manager simiasManager, bool firstAccount )
		{
			this.simiasManager = simiasManager;
			this.simiasWebService = simiasWebService;

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Initialize the wizard pages ... I had to move this here so that
			// dev studio wouldn't wipe it out (when it was in InitializeComponent()).
			this.welcomePage = new WelcomePage();
			this.serverPage = new ServerPage( firstAccount );
			this.identityPage = new IdentityPage();
			this.verifyPage = new VerifyPage( firstAccount );
			this.completionPage = new CompletionPage();
			//
			// welcomePage
			// 
			// TODO: Localize
			this.welcomePage.DescriptionText = "This wizard will guide you through setting up your iFolder account.";
			this.welcomePage.ActionText = "To continue, click Next.";
			this.welcomePage.Location = new System.Drawing.Point(0, 0);
			this.welcomePage.Name = "welcomePage";
			this.welcomePage.Size = new System.Drawing.Size(496, 304);
			this.welcomePage.TabIndex = 1;
			this.welcomePage.WelcomeTitle = "Welcome to the iFolder Account Wizard";
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
			pages = new BaseWizardPage[maxPages];
			pages[0] = this.welcomePage;
			pages[1] = this.serverPage;
			pages[2] = this.identityPage;
			pages[3] = this.verifyPage;
			pages[4] = this.completionPage;

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

			foreach (BaseWizardPage page in pages)
			{
				page.Hide();
			}

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
			// AccountWizard
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
			this.Name = "AccountWizard";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "iFolder Account Wizard";
			this.Activated += new System.EventHandler(this.AccountWizard_Activated);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers

		private void AccountWizard_Activated(object sender, System.EventArgs e)
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

		private void connecting_EnterpriseConnect(object sender, DomainConnectEventArgs e)
		{
			if (EnterpriseConnect != null)
			{
				// Fire the event telling that a new domain has been added.
				EnterpriseConnect(this, new DomainConnectEventArgs( e.DomainInfo ));
			}
		}

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

		public IdentityPage IdentityPage
		{
			get { return identityPage; }
		}

		/// <summary>
		/// Gets the maximum pages in the wizard.
		/// </summary>
		public int MaxPages
		{
			get
			{
				return maxPages;
			}
		}

		public ServerPage ServerPage
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
				StringBuilder sb = new StringBuilder("Congratulations, you are now connected to:\n\n");

				sb.AppendFormat( "{0}\n", "Server name" );
				sb.AppendFormat( "({0})\n\n", serverPage.ServerAddress );
				sb.Append( "You can now add folders to be synchronized to the server.  You may also download folders from the server and have them be synchronized to your computer." );

				return sb.ToString();				
			}
		}

		/// <summary>
		/// Gets or sets flags determining which wizard buttons are enabled or disabled.
		/// </summary>
		public WizardButtons WizardButtons
		{
			get
			{
				WizardButtons wizardButtons = WizardButtons.None;
				wizardButtons |= cancel.Enabled ? WizardButtons.Cancel : WizardButtons.None;
				wizardButtons |= next.Enabled ? WizardButtons.Next : WizardButtons.None;
				wizardButtons |= back.Enabled ? WizardButtons.Back : WizardButtons.None;
				return wizardButtons;
			}
			set
			{
				WizardButtons wizardButtons = value;
				cancel.Enabled = ((wizardButtons & WizardButtons.Cancel) == WizardButtons.Cancel);
				next.Enabled = ((wizardButtons & WizardButtons.Next) == WizardButtons.Next);
				back.Enabled = ((wizardButtons & WizardButtons.Back) == WizardButtons.Back);
			}
		}

		#endregion

		#region Public Methods

		public bool ConnectToServer()
		{
			bool result = false;

			Connecting connecting = new Connecting( simiasWebService, simiasManager, serverPage.ServerAddress, identityPage.Username, identityPage.Password, serverPage.DefaultServer, identityPage.RememberPassword );
			connecting.EnterpriseConnect += new Novell.FormsTrayApp.Connecting.EnterpriseConnectDelegate(connecting_EnterpriseConnect);
			if ( connecting.ShowDialog() == DialogResult.OK )
			{
				result = true;
			}

			return result;
		}

		#endregion
	}
}
