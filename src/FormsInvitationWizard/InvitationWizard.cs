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
using Simias;
using Simias.Sync;
using Simias.POBox;
using Simias.Storage;
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
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(InvitationWizard));
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
		private SubscriptionInfo subInfo;
		private Subscription subscription;
		private string invitationFile;
		private Store store;
		private POBox poBox = null;

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

			// Initialize the wizard pages ... I had to move this here so that
			// dev studio wouldn't wipe it out (when it was in InitializeComponent()).
			this.welcomePage = new Novell.iFolder.InvitationWizard.WelcomePage();
			this.selectInvitationPage = new Novell.iFolder.InvitationWizard.SelectInvitationPage();
			this.acceptDeclinePage = new Novell.iFolder.InvitationWizard.AcceptDeclinePage();
			this.selectiFolderLocationPage = new Novell.iFolder.InvitationWizard.SelectiFolderLocationPage();
			this.completionPage = new CompletionPage();
			//
			// welcomePage
			// 
			this.welcomePage.DescriptionText = "Use this wizard to accept or decline an invitation to participate in a shared iFolder. If you accept, it helps you place the iFolder in an appropriate location on this computer. If you decline, it revokes the invitation. The iFolder client uses your current Windows logon to establish local ownership of the shared iFolder.";
			this.welcomePage.ActionText = "Click Next to continue, or click Cancel to exit.";
			this.welcomePage.Location = new System.Drawing.Point(0, 0);
			this.welcomePage.Name = "welcomePage";
			this.welcomePage.Size = new System.Drawing.Size(496, 304);
			this.welcomePage.TabIndex = 1;
			this.welcomePage.WelcomeTitle = "Welcome to the iFolder Invitation Wizard";
			// 
			// selectInvitationPage
			// 
			this.selectInvitationPage.HeaderSubTitle = "Specify the location of the iFolder Invitation to accept or decline.";
			this.selectInvitationPage.HeaderTitle = "Select an iFolder Invitation";
			this.selectInvitationPage.Location = new System.Drawing.Point(0, 0);
			this.selectInvitationPage.Name = "selectInvitationPage";
			this.selectInvitationPage.Size = new System.Drawing.Size(496, 304);
			this.selectInvitationPage.TabIndex = 1;
			// 
			// acceptDeclinePage
			// 
			this.acceptDeclinePage.HeaderSubTitle = "Accept or decline the invitation to participate in this shared iFolder.";
			this.acceptDeclinePage.HeaderTitle = "Respond to the iFolder Invitation";
			this.acceptDeclinePage.Location = new System.Drawing.Point(0, 0);
			this.acceptDeclinePage.Name = "acceptDeclinePage";
			this.acceptDeclinePage.Size = new System.Drawing.Size(496, 304);
			this.acceptDeclinePage.TabIndex = 1;
			// 
			// selectiFolderLocationPage
			// 
			this.selectiFolderLocationPage.HeaderSubTitle = "Specify the location on this computer to place the shared iFolder.";
			this.selectiFolderLocationPage.HeaderTitle = "Select iFolder Location";
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

			this.Controls.Add(this.welcomePage);
			this.Controls.Add(this.selectInvitationPage);
			this.Controls.Add(this.acceptDeclinePage);
			this.Controls.Add(this.selectiFolderLocationPage);
			this.Controls.Add(this.completionPage);

			// Load the application icon.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, "Invitation.ico"));
			}
			catch (Exception e)
			{
				logger.Debug(e, "Loading icon");
			}

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
			catch (Exception e)
			{
				logger.Debug(e, "Loading watermark");
			}

			foreach (BaseWizardPage page in pages)
			{
				page.Hide();
			}

			// Activate the first wizard page.
			pages[0].ActivatePage(0);

			this.invitationFile = invitationFile;

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
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// next
			// 
			this.next.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.next.Location = new System.Drawing.Point(328, 318);
			this.next.Name = "next";
			this.next.Size = new System.Drawing.Size(72, 23);
			this.next.TabIndex = 2;
			this.next.Text = "Next >";
			this.next.Click += new System.EventHandler(this.next_Click);
			// 
			// back
			// 
			this.back.FlatStyle = System.Windows.Forms.FlatStyle.System;
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
			// InvitationWizard
			// 
			this.AcceptButton = this.next;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(496, 348);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.back);
			this.Controls.Add(this.next);
			this.Controls.Add(this.cancel);
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "InvitationWizard";
			this.Text = "iFolder Invitation Wizard";
			this.Load += new System.EventHandler(this.InvitationWizard_Load);
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
		private void InvitationWizard_Load(object sender, System.EventArgs e)
		{
			store = new Store(new Configuration());

			if (this.invitationFile != "")
			{
				try
				{
					subInfo = new SubscriptionInfo(invitationFile);

					if (!ConvertSubscriptionInfo(subInfo, out subscription))
					{
						// The Subscription object was just created ... we're done for now.
						// TODO: change the message text ... and maybe we should go to a finished page?
						MessageBox.Show("The invitation has been successfully added to your message box.  Once the invitation has synchronized you will be able to accept or decline it.", "Invitation Added");
						Application.Exit();
					}
					else
					{
						// Check the state of the subscription.  If it is ready, proceed; otherwise, quit.
						if (subscription.SubscriptionState != SubscriptionStates.Received)
						{
							// TODO: change the message text  ... and maybe we should go to a finished page?
							MessageBox.Show("The invitation is not ready yet.  Please check back later.", "Invitation Not Ready");
							Application.Exit();
						}
					}
				}
				catch (SimiasException ex)
				{
					ex.LogError();
					MessageBox.Show("An invalid iFolder invitation file was specified on the command-line.  Please see the log file for additional information.\n\n" + this.invitationFile, "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					this.invitationFile = "";
				}
				catch (Exception ex)
				{
					// TODO - resource strings.
					logger.Debug(ex, "Invalid file");
					MessageBox.Show("An invalid iFolder invitation file was specified on the command-line.  Please see the log file for additional information.\n\n" + this.invitationFile, "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					this.invitationFile = "";
				}
			}
		}

		private void next_Click(object sender, System.EventArgs e)
		{
			// Check if we're on the completion page.
			if (currentIndex == (maxPages - 1))
			{
				Cursor.Current = Cursors.WaitCursor;

				// Accept/decline the invitation
				try
				{
					subscription.SubscriptionState = SubscriptionStates.Replied;
					subscription.SubscriptionDisposition = acceptDeclinePage.Accept ? SubscriptionDispositions.Accepted : SubscriptionDispositions.Declined;
					poBox.Commit(subscription);
				}
				catch (SimiasException ex)
				{
					ex.LogFatal();
					MessageBox.Show("An exception occurred while accepting the iFolder invitation.  Please view the log file for additional information.", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
				}
				catch (Exception ex)
				{
					logger.Fatal(ex, "Accepting invitation");
					MessageBox.Show("An exception occurred while accepting the iFolder invitation.  Please view the log file for additional information.", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
				}

				Cursor.Current = Cursors.Default;

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

		public Subscription Subscription
		{
			get
			{
				return subscription;
			}

			set
			{
				subscription = value;
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
				StringBuilder sb = new StringBuilder("The wizard is ready to respond to the iFolder Invitation.\n\n");
				if (this.acceptDeclinePage.Accept)
				{
					sb.AppendFormat("You accepted the invitation to participate in the {0} iFolder shared by {1}.\n\n", subscription.SubscriptionCollectionName, subscription.FromName);
					sb.AppendFormat("The planned location for the shared iFolder is {0}.", Path.Combine(subscription.CollectionRoot, subscription.SubscriptionCollectionName));
				}
				else
				{
					sb.AppendFormat("You declined the invitation to participate in the {0} iFolder shared by {1}.\n\n", subscription.SubscriptionCollectionName, subscription.FromName);
					sb.Append("The invitation will be revoked and cannot be accepted at another computer.");
				}

				return sb.ToString();				
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Creates a Subscription object from a SubscriptionInfo object or gets the Subscription object from
		/// the POBox, if one exists.
		/// </summary>
		/// <param name="subscriptionInfo">The SubscriptionInfo object used to create/find the Subscription object.</param>
		/// <param name="subscription">A new or existing Subscription object based on the SubscriptionInfo object.</param>
		/// <returns>True if an existing Subscription object was found.  False if a new Subscription object was created.</returns>
		public bool ConvertSubscriptionInfo(SubscriptionInfo subscriptionInfo, out Subscription subscription)
		{
			// Check for existing Subscription object in the POBox.
			poBox = POBox.GetPOBox(store, subscriptionInfo.DomainID);
			Node node = poBox.GetNodeByID(subscriptionInfo.SubscriptionID);
			if (node != null)
			{
				subscription = new Subscription(node);
				return true;
			}
			else
			{
				// TODO: what should we name these?
				subscription = new Subscription("Subscription Name", subscriptionInfo);
				poBox.AddMessage(subscription);
				return false;
			}
		}
		#endregion
	}
}
