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
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author:
 *		Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using Gtk;

using Novell.iFolder.Controller;

namespace Novell.iFolder
{
	public class AddAccountWizard : Window
	{
		private Notebook			WizardNotebook;
		private DomainController	domainController;
		private SimiasWebService	simws;
		private bool				ControlKeyPressed;
		
		///
		/// Indexes for the WizardNotebook to assist in navigating.
		///
		public enum AccountWizardPageIndex
		{
			IntroductoryPageIndex = 0,
			ServerInformationPageIndex,
			UserInformationPageIndex,
			ConnectPageIndex,
			SummaryPageIndex
		}

		///
		/// Navigation Buttons
		///
		private Button		NextButton;
		private Button		BackButton;
		private Button		CancelButton;
		private Button		ConnectButton;
		private Button		FinishButton;
		
		///
		/// Introductory Page Widgets
		///
		
		///
		/// Server Information Page Widgets
		///
		private Entry		ServerNameEntry;
		private Label		MakeDefaultLabel;
		private CheckButton	DefaultServerCheckButton;
		
		///
		/// User Information Page Widgets
		///
		private Entry		UserNameEntry;
		private Entry		PasswordEntry;	// set Visibility = false;
		private CheckButton	RememberPasswordCheckButton;
		
		///
		/// Connect Page Widgets
		///
		private Label		ServerNameVerifyLabel;
		private Label		UserNameVerifyLabel;
		private Label		RememberPasswordVerifyLabel;
		private Label		MakeDefaultPromptLabel;
		private Label		MakeDefaultVerifyLabel;
		
		///
		/// Summary Page Widgets
		///
		DomainInformation	ConnectedDomain;
		private Label		SummaryMessageLabel;


		public AddAccountWizard(SimiasWebService simws) : base(WindowType.Toplevel)
		{
			this.Title = Util.GS("Account Wizard");
			this.SetDefaultSize(500, 450);
			this.Resizable = false;
			this.Modal = true;
			this.WindowPosition = Gtk.WindowPosition.Center;
//			this.HasSeparator = false;

			this.simws = simws;

			domainController = DomainController.GetDomainController();
			
			ConnectedDomain = null;
			
			this.Add(CreateWidgets());

			// Bind ESC and C-w to close the window
			ControlKeyPressed = false;
			KeyPressEvent += new KeyPressEventHandler(KeyPressHandler);
			KeyReleaseEvent += new KeyReleaseEventHandler(KeyReleaseHandler);
		}
		
		private Widget CreateWidgets()
		{
			VBox vbox = new VBox(false, 0);
			WizardNotebook = new Notebook();
			vbox.PackStart(WizardNotebook, true, true, 0);
			WizardNotebook.ShowTabs = false;
			
			WizardNotebook.AppendPage(CreateIntroductoryPage(), null);
			WizardNotebook.AppendPage(CreateServerInformationPage(), null);
			WizardNotebook.AppendPage(CreateUserInformationPage(), null);
			WizardNotebook.AppendPage(CreateConnectPage(), null);
			WizardNotebook.AppendPage(CreateSummaryPage(), null);

			WizardNotebook.SwitchPage +=
				new SwitchPageHandler(OnWizardSwitchedPage);

			vbox.PackEnd(CreateActionButtons(), false, false, 0);
			
			return vbox;
		}
		
		private Widget CreateActionButtons()
		{
			HButtonBox hButtonBox = new HButtonBox();
			hButtonBox.Layout = ButtonBoxStyle.End;

			// Set up the response buttons
			// FIXME: Add the correct icons onto these buttons
			CancelButton = new Button(Stock.Cancel);
			hButtonBox.PackStart(CancelButton, false, false, 0);
			CancelButton.Clicked += new EventHandler(OnCancelButton);

			BackButton = new Button(Util.GS("_Back"));
			hButtonBox.PackStart(BackButton, false, false, 0);
			BackButton.Visible = false;
			BackButton.NoShowAll = true;
			BackButton.Clicked += new EventHandler(OnBackButton);

			NextButton = new Button(Util.GS("_Next"));
			hButtonBox.PackStart(NextButton, false, false, 0);
			NextButton.Clicked += new EventHandler(OnNextButton);

			ConnectButton = new Button(Util.GS("Co_nnect"));
			hButtonBox.PackStart(ConnectButton, false, false, 0);
			ConnectButton.Visible = false;
			ConnectButton.NoShowAll = true;
			ConnectButton.Clicked += new EventHandler(OnConnectButton);

			FinishButton = new Button(Util.GS("_Finish"));
			hButtonBox.PackStart(FinishButton, false, false, 0);
			FinishButton.Visible = false;
			FinishButton.NoShowAll = true;
			FinishButton.Clicked += new EventHandler(OnFinishButton);
			
			return hButtonBox;
		}
		
		///
		/// Introductory Page
		///
		private Widget CreateIntroductoryPage()
		{
			VBox vbox = new VBox(false, 6);
			
			///
			/// Header
			///
			HBox headerHBox = new HBox(false, 0);
			vbox.PackStart(headerHBox, false, false, 0);
			
			Label l = new Label(string.Format(
				"<span size=\"xx-large\">{0}</span>",
				Util.GS("Creating a New iFolder Account")));
			headerHBox.PackStart(l, true, true, 0);
			l.UseMarkup = true;
			l.UseUnderline = false;
			l.Xalign = 0.0F;
			l.Yalign = 0.5F;
			l.Xpad = 12;
			l.Ypad = 12;
			
			Image img = new Image(Util.ImagesPath("add-account.png"));
			headerHBox.PackStart(img, false, false, 0);
			img.SetAlignment(0.5F, 0.5F);
			img.Xpad = 12;
			img.Ypad = 12;
			
			HSeparator hsep = new HSeparator();
			vbox.PackStart(hsep, false, false, 0);
			
			///
			/// Content area
			///
			l = new Label(Util.GS("This wizard will guide you through the creation of a new iFolder account.\n\nIt will require some information, such as an iFolder Server address, user name, and password."));
			vbox.PackStart(l, true, true, 0);

			l.LineWrap = true;
			l.Xpad = 12;
			l.Ypad = 6;
			l.Xalign = 0.0F;
			l.Yalign = 0.0F;
			
			return vbox;
		}
		
		///
		/// Server Information Page (1 of 3)
		///
		private Widget CreateServerInformationPage()
		{
			VBox vbox = new VBox(false, 6);
			
			///
			/// Header
			///
			HBox headerHBox = new HBox(false, 6);
			vbox.PackStart(headerHBox, false, false, 0);
			
			Label l = new Label(string.Format(
				"<span size=\"xx-large\">{0}</span>",
				Util.GS("Server Information")));
			headerHBox.PackStart(l, true, true, 0);
			l.UseMarkup = true;
			l.UseUnderline = false;
			l.Xalign = 0.0F;
			l.Yalign = 0.5F;
			l.Xpad = 12;
			l.Ypad = 12;
			
			Image img = new Image(Util.ImagesPath("add-account.png"));
			headerHBox.PackStart(img, false, false, 0);
			img.SetAlignment(0.5F, 0.5F);
			img.Xpad = 12;
			img.Ypad = 12;

			HSeparator hsep = new HSeparator();
			vbox.PackStart(hsep, false, false, 0);
			
			///
			/// Content
			///
			Table table = new Table(4, 3, false);
			vbox.PackStart(table, false, false, 0);
			table.ColumnSpacing = 6;
			table.RowSpacing = 6;
			table.BorderWidth = 12;

			// Row 1
			l = new Label(Util.GS("Enter the name of your iFolder Server (for example, \"ifolder.example.net\")."));
			table.Attach(l, 0,3, 0,1,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.LineWrap = true;
			l.Xalign = 0.0F;

			// Row 2
			table.Attach(new Label(""), 0,1, 1,2,
				AttachOptions.Fill, 0,12,0); // spacer
			l = new Label(Util.GS("iFolder _Server:"));
			table.Attach(l, 1,2, 1,2,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			ServerNameEntry = new Entry();
			table.Attach(ServerNameEntry, 2,3, 1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.MnemonicWidget = ServerNameEntry;
			ServerNameEntry.Changed += new EventHandler(OnFieldChanged);
			
			// Row 3
			MakeDefaultLabel = new Label(Util.GS("Setting this iFolder Server as your default server will allow iFolder to automatically select this server when adding new folders."));
			table.Attach(MakeDefaultLabel, 0,3, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			MakeDefaultLabel.LineWrap = true;
			MakeDefaultLabel.Xalign = 0.0F;
			
			// Row 4
			DefaultServerCheckButton = new CheckButton(Util.GS("Make this my _default server"));
			table.Attach(DefaultServerCheckButton, 1,3, 3,4,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			
			return vbox;
		}
		
		///
		/// User Information Page (2 of 3)
		///
		private Widget CreateUserInformationPage()
		{
			VBox vbox = new VBox(false, 6);
			
			///
			/// Header
			///
			HBox headerHBox = new HBox(false, 6);
			vbox.PackStart(headerHBox, false, false, 0);
			
			Label l = new Label(string.Format(
				"<span size=\"xx-large\">{0}</span>",
				Util.GS("User Information")));
			headerHBox.PackStart(l, true, true, 0);
			l.UseMarkup = true;
			l.UseUnderline = false;
			l.Xalign = 0.0F;
			l.Yalign = 0.5F;
			l.Xpad = 12;
			l.Ypad = 12;
			
			Image img = new Image(Util.ImagesPath("add-account.png"));
			headerHBox.PackStart(img, false, false, 0);
			img.SetAlignment(0.5F, 0.5F);
			img.Xpad = 12;
			img.Ypad = 12;

			HSeparator hsep = new HSeparator();
			vbox.PackStart(hsep, false, false, 0);
			
			///
			/// Content
			///
			Table table = new Table(6, 3, false);
			vbox.PackStart(table, false, false, 0);
			table.ColumnSpacing = 6;
			table.RowSpacing = 6;
			table.BorderWidth = 12;

			// Row 1
			l = new Label(Util.GS("Enter your iFolder user name (for example, \"jsmith\")."));
			table.Attach(l, 0,3, 0,1,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.LineWrap = true;
			l.Xalign = 0.0F;

			// Row 2
			table.Attach(new Label(""), 0,1, 1,2,
				AttachOptions.Fill, 0,12,0); // spacer
			l = new Label(Util.GS("_User Name:"));
			table.Attach(l, 1,2, 1,2,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			UserNameEntry = new Entry();
			table.Attach(UserNameEntry, 2,3, 1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.MnemonicWidget = UserNameEntry;
			UserNameEntry.Changed += new EventHandler(OnFieldChanged);

			// Row 3
			l = new Label(Util.GS("Enter your password."));
			table.Attach(l, 0,3, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.Xalign = 0.0F;
			
			// Row 4
			l = new Label(Util.GS("_Password:"));
			table.Attach(l, 1,2, 3,4,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			PasswordEntry = new Entry();
			table.Attach(PasswordEntry, 2,3, 3,4,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.MnemonicWidget = PasswordEntry;
			PasswordEntry.Visibility = false;
			PasswordEntry.Changed += new EventHandler(OnFieldChanged);

			// Row 5
			l = new Label(Util.GS("Allow iFolder to remember your password so you are not asked for it each time you start iFolder."));
			table.Attach(l, 0,3, 4,5,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.LineWrap = true;
			l.Xalign = 0.0F;
			
			// Row 6
			RememberPasswordCheckButton = new CheckButton(Util.GS("_Remember my password"));
			table.Attach(RememberPasswordCheckButton, 1,3, 5,6,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			
			return vbox;
		}
		
		///
		/// Connect Page (3 of 3)
		///
		private Widget CreateConnectPage()
		{
			VBox vbox = new VBox(false, 6);
			
			///
			/// Header
			///
			HBox headerHBox = new HBox(false, 6);
			vbox.PackStart(headerHBox, false, false, 0);
			
			Label l = new Label(string.Format(
				"<span size=\"xx-large\">{0}</span>",
				Util.GS("Verify and Connect")));
			headerHBox.PackStart(l, true, true, 0);
			l.UseMarkup = true;
			l.UseUnderline = false;
			l.Xalign = 0.0F;
			l.Yalign = 0.5F;
			l.Xpad = 12;
			l.Ypad = 12;
			
			Image img = new Image(Util.ImagesPath("add-account.png"));
			headerHBox.PackStart(img, false, false, 0);
			img.SetAlignment(0.5F, 0.5F);
			img.Xpad = 12;
			img.Ypad = 12;

			HSeparator hsep = new HSeparator();
			vbox.PackStart(hsep, false, false, 0);
			
			///
			/// Content
			///
			Table table = new Table(6, 3, false);
			vbox.PackStart(table, false, false, 0);
			table.ColumnSpacing = 6;
			table.RowSpacing = 6;
			table.BorderWidth = 12;

			// Row 1
			l = new Label(Util.GS("Please verify that the information you've entered is correct."));
			table.Attach(l, 0,3, 0,1,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.LineWrap = true;
			l.Xalign = 0.0F;

			// Row 2
			table.Attach(new Label(""), 0,1, 1,2,
				AttachOptions.Fill, 0,12,0); // spacer
			l = new Label(Util.GS("iFolder Server:"));
			table.Attach(l, 1,2, 1,2,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			ServerNameVerifyLabel = new Label("");
			table.Attach(ServerNameVerifyLabel, 2,3, 1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			ServerNameVerifyLabel.Xalign = 0.0F;

			// Row 3
			l = new Label(Util.GS("User Name:"));
			table.Attach(l, 1,2, 2,3,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			UserNameVerifyLabel = new Label("");
			table.Attach(UserNameVerifyLabel, 2,3, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			UserNameVerifyLabel.Xalign = 0.0F;
			
			// Row 4
			l = new Label(Util.GS("Remember password:"));
			table.Attach(l, 1,2, 3,4,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			RememberPasswordVerifyLabel = new Label("");
			table.Attach(RememberPasswordVerifyLabel, 2,3, 3,4,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			RememberPasswordVerifyLabel.Xalign = 0.0F;
			
			// Row 5
			MakeDefaultPromptLabel = new Label(Util.GS("Make default account:"));
			table.Attach(MakeDefaultPromptLabel, 1,2, 4,5,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			MakeDefaultPromptLabel.Xalign = 0.0F;
			MakeDefaultVerifyLabel = new Label("");
			table.Attach(MakeDefaultVerifyLabel, 2,3, 4,5,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			MakeDefaultVerifyLabel.Xalign = 0.0F;
			
			// Row 6
//			l = new Label(Util.GS("When you press the Connect button, iFolder will attempt to verify your account and connect you to the server."));
//			table.Attach(l, 0,3, 5,6,
//				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
//			l.LineWrap = true;
//			l.Xalign = 0.0F;
			
			return vbox;
		}
		
		///
		/// Summary Page
		///
		private Widget CreateSummaryPage()
		{
			VBox vbox = new VBox(false, 6);
			
			///
			/// Header
			///
			HBox headerHBox = new HBox(false, 6);
			vbox.PackStart(headerHBox, false, false, 0);
			
			Label l = new Label(string.Format(
				"<span size=\"xx-large\">{0}</span>",
				Util.GS("Verify and Connect")));
			headerHBox.PackStart(l, true, true, 0);
			l.UseMarkup = true;
			l.UseUnderline = false;
			l.Xalign = 0.0F;
			l.Yalign = 0.5F;
			l.Xpad = 12;
			l.Ypad = 12;
			
			Image img = new Image(Util.ImagesPath("add-account.png"));
			headerHBox.PackStart(img, false, false, 0);
			img.SetAlignment(0.5F, 0.5F);
			img.Xpad = 12;
			img.Ypad = 12;

			HSeparator hsep = new HSeparator();
			vbox.PackStart(hsep, false, false, 0);
			
			///
			/// Content
			///
			SummaryMessageLabel = new Label("Summary Page not implemented yet.\n\nLet's just say, if you made it this far, your account was successfully added.");
			vbox.PackStart(SummaryMessageLabel, true, true, 0);
			SummaryMessageLabel.LineWrap = true;

			return vbox;
		}
		
		///
		/// Sensitivity Methods
		///
		private void UpdateWidgetSensitivity()
		{
			switch(WizardNotebook.CurrentPage)
			{
				case (int)AccountWizardPageIndex.IntroductoryPageIndex:
					NextButton.Sensitive = true;
					NextButton.Visible = true;
					BackButton.Visible = false;
					CancelButton.Visible = true;
					ConnectButton.Visible = false;
					FinishButton.Visible = false;
					break;
				case (int)AccountWizardPageIndex.ServerInformationPageIndex:
					string currentServerName = ServerNameEntry.Text;
					if (currentServerName != null)
					{
						currentServerName = currentServerName.Trim();
						if (currentServerName.Length > 0)
							NextButton.Sensitive = true;
						else
							NextButton.Sensitive = false;
					}
					else
						NextButton.Sensitive = false;
					NextButton.Visible = true;
					
					BackButton.Visible = true;
					CancelButton.Visible = true;
					ConnectButton.Visible = false;
					FinishButton.Visible = false;
					break;
				case (int)AccountWizardPageIndex.UserInformationPageIndex:
					string currentUserName = UserNameEntry.Text;
					string currentPassword = PasswordEntry.Text;
					if (currentUserName != null && currentPassword != null)
					{
						currentUserName = currentUserName.Trim();
						currentPassword = currentPassword.Trim();
						if (currentUserName.Length > 0 && currentPassword.Length > 0)
							NextButton.Sensitive = true;
						else
							NextButton.Sensitive = false;
					}
					else
						NextButton.Sensitive = false;
					NextButton.Visible = true;

					BackButton.Visible = true;
					CancelButton.Visible = true;
					ConnectButton.Visible = false;
					FinishButton.Visible = false;
					break;
				case (int)AccountWizardPageIndex.ConnectPageIndex:
					NextButton.Visible = false;
					BackButton.Visible = true;
					CancelButton.Visible = true;
					ConnectButton.Visible = true;
					FinishButton.Visible = false;
					break;
				case (int)AccountWizardPageIndex.SummaryPageIndex:
					NextButton.Visible = false;
					BackButton.Visible = false;
					CancelButton.Visible = false;
					ConnectButton.Visible = false;
					FinishButton.Visible = true;
					break;
				default:
					break;
			}
		}
		

		///
		/// Event Handlers
		///

		private void OnWizardSwitchedPage(object o, SwitchPageArgs args)
		{
Console.WriteLine("OnWizardSwitchedPage");
			UpdateWidgetSensitivity();
			
			DomainInformation[] domains;

			switch(WizardNotebook.CurrentPage)
			{
				case (int)AccountWizardPageIndex.IntroductoryPageIndex:
					this.Title = Util.GS("Account Wizard");
					break;
				case (int)AccountWizardPageIndex.ServerInformationPageIndex:
					this.Title = Util.GS("Account Wizard - (1 of 3)");

					domains = domainController.GetDomains();
					if (domains != null && domains.Length > 0)
					{
						MakeDefaultLabel.Visible = true;
						DefaultServerCheckButton.Visible = true;
					}
					else
					{
						DefaultServerCheckButton.Active = true;
						MakeDefaultLabel.Visible = false;
						DefaultServerCheckButton.Visible = false;
					}

					ServerNameEntry.GrabFocus();
					break;
				case (int)AccountWizardPageIndex.UserInformationPageIndex:
					this.Title = Util.GS("Account Wizard - (2 of 3)");
					UserNameEntry.GrabFocus();
					break;
				case (int)AccountWizardPageIndex.ConnectPageIndex:
Console.WriteLine("\tConnect Page");
					this.Title = Util.GS("Account Wizard - (3 of 3)");

Console.WriteLine("\tServer Name: {0}", ServerNameEntry.Text);
					ServerNameVerifyLabel.Text = ServerNameEntry.Text;
					UserNameVerifyLabel.Text = UserNameEntry.Text;

					RememberPasswordVerifyLabel.Text =
						RememberPasswordCheckButton.Active ?
							Util.GS("Yes") :
							Util.GS("No");

					MakeDefaultVerifyLabel.Text =
						DefaultServerCheckButton.Active ?
							Util.GS("Yes") :
							Util.GS("No");

					domains = domainController.GetDomains();
					if (domains != null && domains.Length > 0)
					{
						MakeDefaultPromptLabel.Visible = true;
						MakeDefaultVerifyLabel.Visible = true;
					}
					else
					{
						MakeDefaultPromptLabel.Visible = false;
						MakeDefaultVerifyLabel.Visible = false;
					}

					ConnectButton.GrabFocus();
					break;
				case (int)AccountWizardPageIndex.SummaryPageIndex:
					this.Title = Util.GS("Account Wizard");

					if (ConnectedDomain != null && ConnectedDomain.Name != null && ConnectedDomain.Host != null)
					{
						SummaryMessageLabel.Text = 
							string.Format(
								"Dude, you're connected to:\n{0} ({1})\n\nSummary Page not implemented yet.\n\nLet's just say, if you made it this far, your account was successfully added.",
								ConnectedDomain.Name,
								ConnectedDomain.Host);
					}

					FinishButton.GrabFocus();
					break;
				default:
					break;
			}
		}
		
		private void OnFieldChanged(object o, EventArgs args)
		{
			UpdateWidgetSensitivity();
		}
		
		private void OnCancelButton(object o, EventArgs args)
		{
Console.WriteLine("OnCancelButton");
			CloseDialog();
		}
		
		private void OnBackButton(object o, EventArgs args)
		{
Console.WriteLine("OnBackButton");
			try
			{
				WizardNotebook.CurrentPage = WizardNotebook.CurrentPage - 1;
			}
			catch{}
		}
		
		private void OnNextButton(object o, EventArgs args)
		{
Console.WriteLine("OnNextButton");
			try
			{
				WizardNotebook.CurrentPage = WizardNotebook.CurrentPage + 1;
			}
			catch{}
		}
		
		private void OnConnectButton(object o, EventArgs args)
		{
Console.WriteLine("OnConnectButton");
			string serverName	= ServerNameEntry.Text.Trim();
			string userName		= UserNameEntry.Text.Trim();
			string password		= PasswordEntry.Text;
			
			DomainInformation dom = null;
			try
			{
				dom = domainController.AddDomain(
						serverName,
						userName,
						password,
						RememberPasswordCheckButton.Active,
						DefaultServerCheckButton.Active);
			}
			catch(DomainAccountAlreadyExistsException e1)
			{
				iFolderMsgDialog dg = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Error,
					iFolderMsgDialog.ButtonSet.Ok,
					"",
					Util.GS("An account already exists"),
					Util.GS("An account for this server already exists on the local machine.  Only one account per server is allowed."));
				dg.Run();
				dg.Hide();
				dg.Destroy();
				return;
			}
			catch(Exception e2)
			{
				iFolderMsgDialog dg2 = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Error,
					iFolderMsgDialog.ButtonSet.Ok,
					"",
					Util.GS("Unable to connect to the iFolder Server"),
					Util.GS("An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator."),
					Util.GS(e2.Message));
				dg2.Run();
				dg2.Hide();
				dg2.Destroy();
				return;
			}
			
			if (dom == null) return;	// This shouldn't happen, but just in case...

			Console.WriteLine(dom.StatusCode);

			switch(dom.StatusCode)
			{
				case StatusCodes.InvalidCertificate:
					byte[] byteArray = simws.GetCertificate(serverName);
					System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(byteArray);

					iFolderMsgDialog dialog = new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Question,
						iFolderMsgDialog.ButtonSet.YesNo,
						"",
						string.Format(Util.GS("iFolder cannot verify the identity of the iFolder Server \"{0}\"."), serverName),
						string.Format(Util.GS("The certificate for this iFolder Server was signed by an unknown certifying authority.  You might be connecting to a server that is pretending to be \"{0}\" which could put your confidential information at risk.   Before accepting this certificate, you should check with your system administrator.  Do you want to accept this certificate permanently and continue to connect?"), serverName),
						cert.ToString(true));
					int rc = dialog.Run();
					dialog.Hide();
					dialog.Destroy();
					if(rc == -8) // User clicked the Yes button
					{
						simws.StoreCertificate(byteArray, serverName);
						OnConnectButton(o, args);
						return;
					}
					break;
				case StatusCodes.Success:
				case StatusCodes.SuccessInGrace:
					Status authStatus = domainController.AuthenticateDomain(dom.ID, password, RememberPasswordCheckButton.Active);

					if (authStatus != null)
					{
						if (authStatus.statusCode == StatusCodes.Success ||
							authStatus.statusCode == StatusCodes.SuccessInGrace)
						{
							ConnectedDomain = dom;
							WizardNotebook.CurrentPage = (int)AccountWizardPageIndex.SummaryPageIndex;
							break;
						}
						else
						{
							Util.ShowLoginError(this, authStatus.statusCode);
						}
					}
					else
					{
						Util.ShowLoginError(this, StatusCodes.Unknown);
					}
					break;
				default:
					// Failed to connect
					Util.ShowLoginError(this, dom.StatusCode);
					break;
			}			
		}
		
		private void OnFinishButton(object o, EventArgs args)
		{
Console.WriteLine("OnFinishButton");
			CloseDialog();
		}

		void KeyPressHandler(object o, KeyPressEventArgs args)
		{
			args.RetVal = true;
			
			switch(args.Event.Key)
			{
				case Gdk.Key.Escape:
					CloseDialog();
					break;
				case Gdk.Key.Control_L:
				case Gdk.Key.Control_R:
					ControlKeyPressed = true;
					args.RetVal = false;
					break;
				case Gdk.Key.W:
				case Gdk.Key.w:
					if (ControlKeyPressed)
						CloseDialog();
					else
						args.RetVal = false;
					break;
				default:
					args.RetVal = false;
					break;
			}
		}
		
		void KeyReleaseHandler(object o, KeyReleaseEventArgs args)
		{
			args.RetVal = false;
			
			switch(args.Event.Key)
			{
				case Gdk.Key.Control_L:
				case Gdk.Key.Control_R:
					ControlKeyPressed = false;
					break;
				default:
					break;
			}
		}
		
		public void CloseDialog()
		{
			this.Hide();
			this.Destroy();
		}

		///
		/// Widget Overrides
		///

	}
} 