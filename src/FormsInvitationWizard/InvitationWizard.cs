/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
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
using Simias.Agent;
using Novell.iFolder;
using System.Text;

namespace Novell.iFolder.InvitationWizard
{
	/// <summary>
	/// Enumeration used to tell which wizard buttons are active.
	/// </summary>
	[Flags]
	public enum WizardButtons
	{
		None = 0,
		Back = 1,
		Next = 2,
		Cancel = 4,
	}

	/// <summary>
	/// Summary description for InvitationWizard.
	/// </summary>
	public class InvitationWizard : System.Windows.Forms.Form
	{
		#region Class Members
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Button next;
		private System.Windows.Forms.Button back;
		internal System.Windows.Forms.GroupBox groupBox1;
		private Novell.iFolder.InvitationWizard.WelcomePage welcomePage;
		private SelectInvitationPage selectInvitationPage;
		private AcceptDeclinePage acceptDeclinePage;
		private SelectiFolderLocationPage selectiFolderLocationPage;
		private CompletionPage completionPage;
		private BaseWizardPage[] pages;
		internal const int maxPages = 5;
		private int currentIndex = 0;
		private Invitation invitation;
		private string invitationFile;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		public InvitationWizard(string invitationFile)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// This stuff is here just in case dev studio cuts it out of the InitializeComponent
			// method ... I added it to that method.
//			this.selectInvitationPage = new SelectInvitationPage();
//			this.selectiFolderLocationPage = new SelectiFolderLocationPage();
			//
			// selectInvitationPage
			//
//			this.selectInvitationPage.Location = new System.Drawing.Point(0, 0);
//			this.selectInvitationPage.Size = new System.Drawing.Size(496, 304);
//			this.selectInvitationPage.TabIndex = 6;
			//
			// selectiFolderLocationPage
			//
//			this.selectiFolderLocationPage.Location = new System.Drawing.Point(0, 0);
//			this.selectiFolderLocationPage.Size = new System.Drawing.Size(496, 304);
//			this.selectiFolderLocationPage.TabIndex = 7;
			//
			// InvitationWizard
			//
//			this.Controls.Add(this.selectInvitationPage);
//			this.Controls.Add(this.selectiFolderLocationPage);

			// Load the application icon.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, "Invitation.ico"));
			}
			catch{}

			// Put the wizard pages in order.
			pages = new BaseWizardPage[maxPages];
			pages[0] = this.welcomePage;
			pages[1] = this.selectInvitationPage;
			pages[2] = this.acceptDeclinePage;
			pages[3] = this.selectiFolderLocationPage;
			pages[4] = this.completionPage;

			// Set the current directory to the install directory.
			string[] args = Environment.GetCommandLineArgs();
			Directory.SetCurrentDirectory(Path.GetDirectoryName(args[0]));

			try
			{
				// Load the watermark.
				Image image = Image.FromFile("invitewiz.png");
				this.welcomePage.Watermark = image;
				this.completionPage.Watermark = image;
			}
			catch{}

			foreach (BaseWizardPage page in pages)
			{
				page.Hide();
			}

			// Activate the first wizard page.
			pages[0].ActivatePage(0);

			invitation = new Invitation();
			this.invitationFile = invitationFile;

			if (this.invitationFile != "")
			{
				try
				{
					invitation.Load(this.invitationFile);
				}
				catch (Exception)
				{
					// TODO - resource strings.
					MessageBox.Show("An invalid iFolder invitation file was specified on the command-line.\n\n" + this.invitationFile, "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Information);
					this.invitationFile = "";
				}
			}

//			this.selectInvitationPage.Hide();
//			this.selectiFolderLocationPage.Hide();
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
			this.welcomePage = new Novell.iFolder.InvitationWizard.WelcomePage();
			this.selectInvitationPage = new Novell.iFolder.InvitationWizard.SelectInvitationPage();
			this.acceptDeclinePage = new Novell.iFolder.InvitationWizard.AcceptDeclinePage();
			this.selectiFolderLocationPage = new Novell.iFolder.InvitationWizard.SelectiFolderLocationPage();
			this.completionPage = new CompletionPage();
			this.SuspendLayout();
			// 
			// cancel
			// 
			this.cancel.Location = new System.Drawing.Point(416, 318);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(72, 23);
			this.cancel.TabIndex = 3;
			this.cancel.Text = "Cancel";
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// next
			// 
			this.next.Location = new System.Drawing.Point(328, 318);
			this.next.Name = "next";
			this.next.Size = new System.Drawing.Size(72, 23);
			this.next.TabIndex = 2;
			this.next.Text = "Next >";
			this.next.Click += new System.EventHandler(this.next_Click);
			// 
			// back
			// 
			this.back.Location = new System.Drawing.Point(251, 318);
			this.back.Name = "back";
			this.back.Size = new System.Drawing.Size(72, 23);
			this.back.TabIndex = 1;
			this.back.Text = "< Back";
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
			// welcomePage
			// 
			this.welcomePage.DescriptionText = "This wizard will help you accept an invitation to a shared iFolder and place it in the appropriate location.\n\nTo continue, click Next.";
			this.welcomePage.Location = new System.Drawing.Point(0, 0);
			this.welcomePage.Name = "welcomePage";
			this.welcomePage.Size = new System.Drawing.Size(496, 304);
			this.welcomePage.TabIndex = 1;
			this.welcomePage.WelcomeTitle = "Welcome to the iFolder Invitation Wizard";
			// 
			// selectInvitationPage
			// 
			this.selectInvitationPage.HeaderSubTitle = "Select an iFolder invitation to accept on this computer.";
			this.selectInvitationPage.HeaderTitle = "Choose iFolder Invitation";
			this.selectInvitationPage.Location = new System.Drawing.Point(0, 0);
			this.selectInvitationPage.Name = "selectInvitationPage";
			this.selectInvitationPage.Size = new System.Drawing.Size(496, 304);
			this.selectInvitationPage.TabIndex = 1;
			// 
			// acceptDeclinePage
			// 
			this.acceptDeclinePage.HeaderSubTitle = "Choose to accept or decline this iFolder invitation.";
			this.acceptDeclinePage.HeaderTitle = "Accept iFolder Invitation";
			this.acceptDeclinePage.Location = new System.Drawing.Point(0, 0);
			this.acceptDeclinePage.Name = "acceptDeclinePage";
			this.acceptDeclinePage.Size = new System.Drawing.Size(496, 304);
			this.acceptDeclinePage.TabIndex = 1;
			// 
			// selectiFolderLocationPage
			// 
			this.selectiFolderLocationPage.HeaderSubTitle = "Select a location for the shared iFolder to be placed on this computer.";
			this.selectiFolderLocationPage.HeaderTitle = "Choose iFolder Location";
			this.selectiFolderLocationPage.Location = new System.Drawing.Point(0, 0);
			this.selectiFolderLocationPage.Name = "selectiFolderLocationPage";
			this.selectiFolderLocationPage.Size = new System.Drawing.Size(496, 304);
			this.selectiFolderLocationPage.TabIndex = 1;
			//
			// completionPage
			//
			this.completionPage.DescriptionText = "Description...";
			this.completionPage.Location = new System.Drawing.Point(0, 0);
			this.completionPage.Name = "completionPage";
			this.completionPage.Size = new System.Drawing.Size(496, 304);
			this.completionPage.TabIndex = 1;
			this.completionPage.WelcomeTitle = "Completing the iFolder Invitation Wizard";
			// 
			// InvitationWizard
			// 
			this.AcceptButton = this.next;
			this.CancelButton = this.cancel;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(496, 348);
			this.Controls.Add(this.welcomePage);
			this.Controls.Add(this.selectInvitationPage);
			this.Controls.Add(this.acceptDeclinePage);
			this.Controls.Add(this.selectiFolderLocationPage);
			this.Controls.Add(this.completionPage);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.back);
			this.Controls.Add(this.next);
			this.Controls.Add(this.cancel);
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "InvitationWizard";
			this.Text = "iFolder Invitation Wizard";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) 
		{
			string invitationFile = "";

			if (args.Length > 0)
			{
				invitationFile = args[0].ToString();
			}

			Application.Run(new InvitationWizard(invitationFile));
		}

		#region Event Handlers
		private void next_Click(object sender, System.EventArgs e)
		{
			// Check if we're on the completion page.
			if (currentIndex == (maxPages - 1))
			{
				if (this.acceptDeclinePage.Accept)
				{
					// Accept the invitation
					iFolderManager manager = iFolderManager.Connect();
					manager.AcceptInvitation(invitation, invitation.RootPath);
				}

				// Exit
				Application.Exit();
				return;
			}

			int nextIndex = this.pages[currentIndex].ValidatePage(currentIndex);
			if (nextIndex != currentIndex)
			{
				this.pages[currentIndex].DeactivatePage();
				this.pages[nextIndex].ActivatePage(currentIndex);

				currentIndex = nextIndex;

				if (currentIndex == (maxPages - 1))
				{
					// We're on the completion page ... change the Next 
					// button to a Finish button.
					this.next.Text = "Finish";
				}
			}
		}

		private void back_Click(object sender, System.EventArgs e)
		{
			// Check if we're on the completion page.
			if (currentIndex == (maxPages - 1))
			{
				this.next.Text = "Next >";
			}

			int previousIndex = this.pages[currentIndex].DeactivatePage();
			this.pages[previousIndex].ActivatePage(0);

			currentIndex = previousIndex;
		}

		private void cancel_Click(object sender, System.EventArgs e)
		{
			Application.Exit();		
		}
		#endregion

		#region Properties
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

		public Invitation Invitation
		{
			get
			{
				return invitation;
			}

			set
			{
				invitation = value;
			}
		}

		public int MaxPages
		{
			get
			{
				return maxPages;
			}
		}

		public string InvitationFile
		{
			get
			{
				return invitationFile;
			}
		}

		public string SummaryText
		{
			get
			{
				// TODO - may need to re-work this ... especially for localization (not good to cat strings together).
				StringBuilder sb = new StringBuilder("You have successfully completed the iFolder Invitation Wizard.\n\n");
				sb.AppendFormat("You have choosen to {0} the {1} iFolder from {2}.\n\n", this.acceptDeclinePage.Accept ? "accept" : "decline", this.Invitation.CollectionName, this.Invitation.FromName);
				if (this.acceptDeclinePage.Accept)
				{
					sb.AppendFormat("The iFolder will be created in {0}.\n\n", this.Invitation.RootPath);
				}

				sb.Append("To close this wizard and process your request, click Finish.");

				return sb.ToString();				
			}
		}
		#endregion
	}
}
