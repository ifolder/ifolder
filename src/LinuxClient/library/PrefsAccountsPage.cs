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
 *  Authors:
 *		Calvin Gaisford <cgaisford@novell.com>
 *		Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/

using System;
using System.IO;
using System.Collections;
using Gtk;
using Simias.Client.Event;
using Simias.Client;
using Simias.Client.Authentication;

using Novell.iFolder.Events;
using Novell.iFolder.Controller;

namespace Novell.iFolder
{

	/// <summary>
	/// A VBox with the ability to create and manage ifolder accounts
	/// </summary>
	public class PrefsAccountsPage : VBox
	{
		private Gtk.Window				topLevelWindow;
		private SimiasWebService		simws;

		private iFolderTreeView		AccTreeView;
		private ListStore			AccTreeStore;

		private Button 				AddButton;
		private Button				RemoveButton;
		private Button				DetailsButton;
		private bool				NewAccountMode;
 
 		private Frame		detailsFrame;
		private Label		nameLabel;
		private Entry		nameEntry;
		private Label		passLabel;
		private Entry		passEntry;
		private Label		serverLabel;
		private Entry		serverEntry;
		private CheckButton	savePasswordButton;
		private CheckButton	autoLoginButton;
		private CheckButton	defaultAccButton;
		private Button		loginButton;

		private string				curDomainPassword;
		private string				curDomain;

		private Hashtable			curDomains;
		
		private DomainController	domainController;
		
		// Hashtable used to keep track of the details
		// dialogs that are spawned so that we don't open
		// multiple for the same account.
		private Hashtable			detailsDialogs;
		
		/// <summary>
		/// Default constructor for iFolderAccountsPage
		/// </summary>
		public PrefsAccountsPage( Gtk.Window topWindow )
			: base()
		{
			this.topLevelWindow = topWindow;
			this.simws = new SimiasWebService();
			simws.Url = Simias.Client.Manager.LocalServiceUrl.ToString() +
					"/Simias.asmx";
			LocalService.Start(simws);

			curDomains = new Hashtable();

			InitializeWidgets();

			domainController = DomainController.GetDomainController();
			if (domainController != null)
			{
				domainController.DomainAdded +=
					new DomainAddedEventHandler(OnDomainAddedEvent);
				domainController.DomainDeleted +=
					new DomainDeletedEventHandler(OnDomainDeletedEvent);
				domainController.DomainLoggedIn +=
					new DomainLoggedInEventHandler(OnDomainLoggedInEvent);
				domainController.DomainLoggedOut +=
					new DomainLoggedOutEventHandler(OnDomainLoggedOutEvent);
				domainController.DomainActivated +=
					new DomainActivatedEventHandler(OnDomainActivatedEvent);
				domainController.DomainInactivated +=
					new DomainInactivatedEventHandler(OnDomainInactivatedEvent);
				domainController.NewDefaultDomain +=
					new DomainNewDefaultEventHandler(OnNewDefaultDomainEvent);
				domainController.DomainInGraceLoginPeriod +=
					new DomainInGraceLoginPeriodEventHandler(OnDomainInGraceLoginPeriodEvent);
			}
			
			detailsDialogs = new Hashtable();

			this.Realized += new EventHandler(OnRealizeWidget);
		}
		
		~PrefsAccountsPage()
		{
			if (domainController != null)
			{
				// Unregister for domain events
				domainController.DomainAdded -=
					new DomainAddedEventHandler(OnDomainAddedEvent);
				domainController.DomainDeleted -=
					new DomainDeletedEventHandler(OnDomainDeletedEvent);
				domainController.DomainLoggedIn -=
					new DomainLoggedInEventHandler(OnDomainLoggedInEvent);
				domainController.DomainLoggedOut -=
					new DomainLoggedOutEventHandler(OnDomainLoggedOutEvent);
				domainController.DomainActivated -=
					new DomainActivatedEventHandler(OnDomainActivatedEvent);
				domainController.DomainInactivated -=
					new DomainInactivatedEventHandler(OnDomainInactivatedEvent);
				domainController.NewDefaultDomain -=
					new DomainNewDefaultEventHandler(OnNewDefaultDomainEvent);
				domainController.DomainInGraceLoginPeriod -=
					new DomainInGraceLoginPeriodEventHandler(OnDomainInGraceLoginPeriodEvent);
			}
		} 



		/// <summary>
		/// Set up the widgets
		/// </summary>
		/// <returns>
		/// Widget to display
		/// </returns>
		private void InitializeWidgets()
		{
			this.Spacing = 10;
			this.BorderWidth = 10;
			
			NewAccountMode = false;

			// Set up the Accounts tree view in a scrolled window
			AccTreeView = new iFolderTreeView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			sw.Add(AccTreeView);
			this.PackStart(sw, true, true, 0);

			AccTreeStore = new ListStore(typeof(string));
			AccTreeView.Model = AccTreeStore;

			// Server Column
			TreeViewColumn serverColumn = new TreeViewColumn();
			serverColumn.Title = Util.GS("System Name");
			CellRendererText servercr = new CellRendererText();
			servercr.Xpad = 5;
			serverColumn.PackStart(servercr, false);
			serverColumn.SetCellDataFunc(servercr,
										 new TreeCellDataFunc(ServerCellTextDataFunc));
			serverColumn.Resizable = true;
			serverColumn.MinWidth = 150;
			AccTreeView.AppendColumn(serverColumn);

			// Username Column
			TreeViewColumn nameColumn = new TreeViewColumn();
			nameColumn.Title = Util.GS("Username");
			CellRendererText ncrt = new CellRendererText();
			nameColumn.PackStart(ncrt, false);
			nameColumn.SetCellDataFunc(ncrt,
									   new TreeCellDataFunc(NameCellTextDataFunc));
			nameColumn.Resizable = true;
			nameColumn.MinWidth = 150;
			AccTreeView.AppendColumn(nameColumn);

			// Status Column
			TreeViewColumn statusColumn = new TreeViewColumn();
			statusColumn.Title = Util.GS("Status");
			CellRendererText statusCellRendererText = new CellRendererText();
			statusColumn.PackStart(statusCellRendererText, false);
			statusColumn.SetCellDataFunc(statusCellRendererText,
										 new TreeCellDataFunc(StatusCellTextDataFunc));
			statusColumn.Resizable = true;
			AccTreeView.AppendColumn(statusColumn);
			

			AccTreeView.Selection.Mode = SelectionMode.Single;
			AccTreeView.Selection.Changed +=
				new EventHandler(AccSelectionChangedHandler);
			
			// Set up buttons for add/remove/accept/decline
			HButtonBox buttonBox = new HButtonBox();
			buttonBox.Spacing = 10;
			buttonBox.Layout = ButtonBoxStyle.End;
			this.PackStart(buttonBox, false, false, 0);

			AddButton = new Button(Gtk.Stock.Add);
			buttonBox.PackStart(AddButton);
			AddButton.Clicked += new EventHandler(OnAddAccount);

			RemoveButton = new Button(Gtk.Stock.Remove);
			buttonBox.PackStart(RemoveButton);
			RemoveButton.Clicked += new EventHandler(OnRemoveAccount);

			DetailsButton = new Button(Util.GS("_Details"));
			buttonBox.PackStart(DetailsButton);
			DetailsButton.Clicked += new EventHandler(OnDetailsClicked);


			detailsFrame = new Frame(Util.GS("Account Settings"));
			this.PackStart(detailsFrame, false, false, 0);

			VBox vbox = new VBox();
			vbox.BorderWidth = 10;
			detailsFrame.Add(vbox);


			Table loginTable;
			loginTable = new Table(5,2,false);

			loginTable.RowSpacing = 10;
			loginTable.ColumnSpacing = 10;
			loginTable.Homogeneous = false;


			// 1. Server host
			serverLabel = new Label(Util.GS("Server:"));
			serverLabel.Xalign = 1;
			loginTable.Attach(serverLabel, 0,1,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			serverEntry = new Entry();
			serverEntry.Changed += new EventHandler(OnFieldsChanged);
			serverEntry.ActivatesDefault = true;
			loginTable.Attach(serverEntry, 1,2,0,1,
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);


			// 2. Username
			nameLabel = new Label(Util.GS("User Name:"));
			nameLabel.Xalign = 1; 
			loginTable.Attach(nameLabel, 0,1,1,2,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			nameEntry = new Entry();
			nameEntry.Changed += new EventHandler(OnFieldsChanged);
			nameEntry.ActivatesDefault = true;
			nameEntry.WidthChars = 35;
			loginTable.Attach(nameEntry, 1,2,1,2, 
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);


			// 3. Password
			passLabel = new Label(Util.GS("Password:"));
			passLabel.Xalign = 1;
			loginTable.Attach(passLabel, 0,1,2,3,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			passEntry = new Entry();
			passEntry.Changed += new EventHandler(OnFieldsChanged);
			passEntry.Activated += new EventHandler(OnPassEntryActivated);
			passEntry.FocusOutEvent += 
					new FocusOutEventHandler(OnPassEntryFoucusOut);
			passEntry.ActivatesDefault = true;
			passEntry.Visibility = false;
			loginTable.Attach(passEntry, 1,2,2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);


			VBox optBox = new VBox();

			savePasswordButton = 
				new CheckButton(Util.GS(
					"Remember _password"));
			optBox.PackStart(savePasswordButton, false, false,0);
			savePasswordButton.Toggled += 
							new EventHandler(OnSavePasswordToggled);


			autoLoginButton = 
				new CheckButton(Util.GS(
					"_Enable account"));
			optBox.PackStart(autoLoginButton, false, false,0);
			autoLoginButton.Toggled += 
							new EventHandler(OnAutoLoginToggled);

			defaultAccButton = 
				new CheckButton(Util.GS(
					"De_fault account"));
			optBox.PackStart(defaultAccButton, false, false,0);
			defaultAccButton.Toggled += 
							new EventHandler(OnDefAccToggled);

			loginTable.Attach(optBox, 1,2,3,4,
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);


			vbox.PackStart(loginTable, true, true, 0);


			HButtonBox loginBox = new HButtonBox();
			loginBox.Spacing = 10;
			loginBox.Layout = ButtonBoxStyle.End;

			loginButton =
				new Button(Util.GS("_Log In"));
			loginBox.PackStart(loginButton, false, false, 0);
			loginButton.Clicked += new EventHandler(OnLoginButtonPressed);
			loginButton.CanDefault = true;


			AccTreeView.RowActivated += new RowActivatedHandler(
						OnAccTreeRowActivated);



			vbox.PackStart(loginBox, false, false, 0);

		}




		/// <summary>
		/// Set the Values in the Widgets
		/// </summary>
		private void PopulateWidgets()
		{
			PopulateDomains();
			
			UpdateWidgetSensitivity();
		}

		private bool SetFocusToServerEntry()
		{
			serverEntry.HasFocus = true;
			return false;
		}



		private void PopulateDomains()
		{
			DomainInformation[] domains = domainController.GetDomains();

			foreach(DomainInformation dom in domains)
			{
				// only show Domains that are slaves (not on this machine)
				if(dom.IsSlave)
				{
					TreeIter iter = AccTreeStore.AppendValues(dom.ID);
					curDomains[dom.ID] = iter;
				}
			}
		}




		private void OnRealizeWidget(object o, EventArgs args)
		{
			PopulateWidgets();
		}




		private void ServerCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			string domainID = (string) tree_model.GetValue(iter, 0);
			DomainInformation dom = domainController.GetDomain(domainID);
			if (dom != null)
				((CellRendererText) cell).Text = dom.Name;
			else
				((CellRendererText) cell).Text = "";
		}




		private void NameCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			string domainID = (string) tree_model.GetValue(iter, 0);
			DomainInformation dom = domainController.GetDomain(domainID);
			if (dom != null)
				((CellRendererText) cell).Text = dom.MemberName;
			else
				((CellRendererText) cell).Text = "";
		}




		private void StatusCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			string domainID = (string) tree_model.GetValue(iter, 0);
			DomainInformation dom = domainController.GetDomain(domainID);
			if (dom != null)
			{
				if (dom.Active)
				{
					if (dom.Authenticated)
					{
						((CellRendererText) cell).Text = Util.GS("Logged in");
					}
					else
					{
						((CellRendererText) cell).Text = Util.GS("Logged out");
					}
				}
				else
				{
					((CellRendererText) cell).Text = Util.GS("Disabled");
				}
			}
			else
				((CellRendererText) cell).Text = "";
		}


		private void OnAddAccount(object o, EventArgs args)
		{
			EnterNewAccountMode();
		}

		private void OnRemoveAccount(object o, EventArgs args)
		{
			TreeSelection tSelect = AccTreeView.Selection;

			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				string domainID = (string) tModel.GetValue(iter, 0);
				DomainInformation dom = domainController.GetDomain(domainID);

				RemoveAccountDialog rad = new RemoveAccountDialog(dom);
				rad.TransientFor = topLevelWindow;
				int rc = rad.Run();
				rad.Hide();
				if((ResponseType)rc == ResponseType.Yes)
				{
					try
					{
						domainController.RemoveDomain(dom.ID, rad.RemoveiFoldersFromServer);
					}
					catch(Exception e)
					{
						iFolderExceptionDialog ied = 
							new iFolderExceptionDialog( topLevelWindow, e);
						ied.Run();
						ied.Hide();
						ied.Destroy();
						rad.Destroy();	// Clean up before bailing
						return;
					}

					serverEntry.Text = "";
					nameEntry.Text = "";
					passEntry.Text = "";

					EnableEntry(nameEntry, false);
					nameLabel.Sensitive = false;
					EnableEntry(passEntry, false);
					passLabel.Sensitive = false;
					EnableEntry(serverEntry, false);
					serverLabel.Sensitive = false;

					savePasswordButton.Sensitive = false;
					autoLoginButton.Sensitive = false;
					defaultAccButton.Sensitive = false;
					loginButton.Sensitive = false;

					AddButton.Sensitive = false;
					RemoveButton.Sensitive = false;
					DetailsButton.Sensitive = false;
				}

				rad.Destroy();
			}
		}


		private void OnAccTreeRowActivated(object o, RowActivatedArgs args)
		{
			if(!NewAccountMode)
				OnDetailsClicked(o, args);
		}


		private void OnDetailsClicked(object o, EventArgs args)
		{
			TreeSelection tSelect = AccTreeView.Selection;

			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				string domainID = (string) tModel.GetValue(iter, 0);
				DomainInformation dom = domainController.GetDomain(domainID);

				AccountDialog accDialog = null;
				if (detailsDialogs.ContainsKey(domainID))
				{
					accDialog = (AccountDialog) detailsDialogs[domainID];
					accDialog.Present();
				}
				else
				{
					accDialog = new AccountDialog(dom);
					detailsDialogs[domainID] = accDialog;
					accDialog.SetPosition(WindowPosition.Center);
					accDialog.Destroyed += 
							new EventHandler(OnAccountDialogDestroyedEvent);

					accDialog.ShowAll();
				}
			}
		}


		private void OnAccountDialogDestroyedEvent(object o, EventArgs args)
		{
			AccountDialog accDialog = (AccountDialog) o;
			if (accDialog != null)
			{
				string domainID = accDialog.DomainID;
				if (domainID != null && detailsDialogs.ContainsKey(domainID))
				{
					detailsDialogs.Remove(domainID);
				}
			}
		}
		



		public void AccSelectionChangedHandler(object o, EventArgs args)
		{
			if(NewAccountMode)
			{
				if( (nameEntry.Text.Length > 0) ||
					(passEntry.Text.Length > 0 ) ||
					(serverEntry.Text.Length > 0) )
				{
					iFolderMsgDialog dialog = new iFolderMsgDialog(
						topLevelWindow,
						iFolderMsgDialog.DialogType.Question,
						iFolderMsgDialog.ButtonSet.YesNo,
						"",
						Util.GS("Lose current settings?"),
						Util.GS("You are currently creating a new account.  By selecting a different account you cancel the operation."));
					int rc = dialog.Run();
					dialog.Hide();
					dialog.Destroy();
					if(rc == -8)
					{
						NewAccountMode = false;
					}
					else
					{
						// disconnect the handler so we can unselect and
						// not get called again
						AccTreeView.Selection.Changed -=
							new EventHandler(AccSelectionChangedHandler);
						AccTreeView.Selection.UnselectAll();
						// hook'r back up
						AccTreeView.Selection.Changed +=
							new EventHandler(AccSelectionChangedHandler);
	
						return;
					}
				}
				else
				{
					// There isn't any data in the fields so just let them
					// go ahead and quit new account mode
					NewAccountMode = false;
				}
			}
			else
			{
				// LAME LAME LAME
				// This Retarted event is called before the other widget
				// looses it's focus so we have to deal with it here
				// This is a hack to save the password
				if( (curDomainPassword != passEntry.Text) && 
						(savePasswordButton.Active == true ) )
				{
					SavePasswordNow();
				}
			}

			UpdateWidgetSensitivity();
		}

		private void OnLoginButtonPressed(object o, EventArgs args)
		{
			TreeSelection tSelect = AccTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				if (loginButton.Label == Util.GS("_Log In"))
					OnLoginAccount(o, args);
				else
					OnLogoutAccount(o, args);
			}
			else
			{
				// This must be a new account attempting to login for the first time
				OnLoginAccount(o, args);
			}
		}
		
		private void OnLogoutAccount(object o, EventArgs args)
		{
			TreeSelection tSelect = AccTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				string domainID = (string) tModel.GetValue(iter, 0);
				DomainInformation dom = domainController.GetDomain(domainID);
				try
				{
					DomainAuthentication domainAuth = new DomainAuthentication("iFolder", dom.ID, null);
					domainAuth.Logout();

					dom.Authenticated = false;
					UpdateDomainStatus(dom.ID);

					// FIXME: Update the login button
					
				}
				catch (Exception ex)
				{
					iFolderMsgDialog dg = new iFolderMsgDialog(
						topLevelWindow,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.Ok,
						"",
						Util.GS("Unable to log out of the iFolder Server"),
						Util.GS("An error was encountered while logging out of the iFolder Server.  If the problem persists, please contact your network administrator."));
					dg.Run();
					dg.Hide();
					dg.Destroy();
				}
			}				
		}

		private void OnLoginAccount(object o, EventArgs args)
		{
			DomainInformation dom = null;
			if (NewAccountMode)
			{
				try
				{
					dom = domainController.AddDomain(
							serverEntry.Text,
							nameEntry.Text,
							passEntry.Text,
							savePasswordButton.Active,
							defaultAccButton.Active);
				}
				catch (DomainAccountAlreadyExistsException e1)
				{
					iFolderMsgDialog dg = new iFolderMsgDialog(
						topLevelWindow,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.Ok,
						"",
						Util.GS("An account already exists"),
						Util.GS("An account for this server already exists on the local machine.  Only one account per server is allowed."));
					dg.Run();
					dg.Hide();
					dg.Destroy();
				}
				catch (Exception e2)
				{
					iFolderMsgDialog dg2 = new iFolderMsgDialog(
						topLevelWindow,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.Ok,
						"",
						Util.GS("Unable to connect to the iFolder Server"),
						Util.GS("An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator."),
						Util.GS(e2.Message));
					dg2.Run();
					dg2.Hide();
					dg2.Destroy();
				}
			}
			else
			{
				// Existing account
				try
				{
					dom = domainController.UpdateDomainHostAddress(curDomain, serverEntry.Text);
				}
				catch (Exception e)
				{
					iFolderMsgDialog dg = new iFolderMsgDialog(
						topLevelWindow,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.Ok,
						"",
						Util.GS("Unable to connect to the iFolder Server"),
						Util.GS("An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator."),
						Util.GS(e.Message));
					dg.Run();
					dg.Hide();
					dg.Destroy();
				}
			}
			
			if (dom == null) return;	// Shouldn't happen, but just in case...

			switch (dom.StatusCode)
			{
				case StatusCodes.InvalidCertificate:
				{
					byte[] byteArray = simws.GetCertificate(serverEntry.Text);
					System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(byteArray);

					iFolderMsgDialog dialog = new iFolderMsgDialog(
						topLevelWindow,
						iFolderMsgDialog.DialogType.Question,
						iFolderMsgDialog.ButtonSet.YesNo,
						"",
						string.Format(Util.GS("iFolder cannot verify the identity of the iFolder Server \"{0}\"."), serverEntry.Text),
						string.Format(Util.GS("The certificate for this iFolder Server was signed by an unknown certifying authority.  You might be connecting to a server that is pretending to be \"{0}\" which could put your confidential information at risk.   Before accepting this certificate, you should check with your system administrator.  Do you want to accept this certificate permanently and continue to connect?"), serverEntry.Text),
						cert.ToString(true));
					int rc = dialog.Run();
					dialog.Hide();
					dialog.Destroy();
					if(rc == -8) // User clicked the Yes button
					{
						simws.StoreCertificate(byteArray, serverEntry.Text);
						OnLoginAccount(o, args);
					}
					break;
				}
				case StatusCodes.Success:
				case StatusCodes.SuccessInGrace:
					Status authStatus = domainController.AuthenticateDomain(dom.ID, passEntry.Text, savePasswordButton.Active);

					if (authStatus != null)
					{
						if (authStatus.statusCode == StatusCodes.Success ||
							authStatus.statusCode == StatusCodes.SuccessInGrace)
						{
							if (NewAccountMode)
							{
								NewAccountMode = false;
							}
							else
							{
								UpdateWidgetSensitivity();
							}
						}
						else
						{
							Util.ShowLoginError(topLevelWindow, authStatus.statusCode);
						}
					}
					else
					{
						Util.ShowLoginError(topLevelWindow, StatusCodes.Unknown);
					}
					break;
				default:
					// We failed to connect
					Util.ShowLoginError(topLevelWindow, dom.StatusCode);
					break;
			}
		}
		
		private void UpdateWidgetSensitivity()
		{
			if (curDomains.Count == 0)
				curDomain = null;

			// Temporarily disable the OnFieldsChanged handler
			nameEntry.Changed 	-= new EventHandler(OnFieldsChanged);
			serverEntry.Changed	-= new EventHandler(OnFieldsChanged);
			passEntry.Changed	-= new EventHandler(OnFieldsChanged);

			TreeSelection tSelect = AccTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;
	
				tSelect.GetSelected(out tModel, out iter);
				string domainID = (string) tModel.GetValue(iter, 0);
				DomainInformation dom = domainController.GetDomain(domainID);
				if (dom == null) return;	// Prevent null pointer
					
				curDomain = dom.ID;
	
				// Set the control states
				AddButton.Sensitive				= true;
				RemoveButton.Sensitive			= true;
				DetailsButton.Sensitive			= true;
	
				if (dom.Active && !dom.Authenticated)
				{
					EnableEntry(serverEntry, true);
					serverLabel.Sensitive		= true; 
				}
				else
				{
					EnableEntry(serverEntry, false);
					serverLabel.Sensitive		= false;
				}
				if (dom.Host != null)
					serverEntry.Text			= dom.Host;
				else
					serverEntry.Text			= "";

				EnableEntry(nameEntry, false);

				nameLabel.Sensitive				= false;
				if (dom.MemberName != null)
					nameEntry.Text				= dom.MemberName;
				else
					nameEntry.Text				= "";

				EnableEntry(passEntry, true);

				passLabel.Sensitive				= true;

				string password = domainController.GetDomainPassword(dom.ID);
				if (password != null)
				{
					curDomainPassword			= password;
					passEntry.Text				= password;
					savePasswordButton.Active	= true;
				}
				else
				{
					curDomainPassword			= "";
					passEntry.Text				= "";
					savePasswordButton.Active	= false;
				}

				if (passEntry.Text.Length == 0)
					savePasswordButton.Sensitive	= false;
				else
					savePasswordButton.Sensitive	= true;
					
				autoLoginButton.Sensitive		= true;
				autoLoginButton.Active			= dom.Active;

				defaultAccButton.Active			= dom.IsDefault;
				defaultAccButton.Sensitive		= !dom.IsDefault;
	
				if (dom.Active)
				{
					if (dom.Authenticated)
					{
						loginButton.Label		= Util.GS("_Log Out");
						loginButton.Sensitive	= true;
					}
					else
					{
						loginButton.Label		= Util.GS("_Log In");

						if (passEntry.Text.Length > 0)
							loginButton.Sensitive = true;
						else
							loginButton.Sensitive = false;
					}
				}
				else
				{
					loginButton.Sensitive		= false;
					loginButton.Label			= Util.GS("_Log In");
				}
			}
			else
			{
				// Nothing is selected
				AddButton.Sensitive				= true;
				RemoveButton.Sensitive			= false;
				DetailsButton.Sensitive			= false;
					
				serverLabel.Sensitive			= false;
				EnableEntry(serverEntry, false);
				serverEntry.Text				= "";
					
				nameLabel.Sensitive				= false;
				EnableEntry(nameEntry, false);
				nameEntry.Text					= "";
					
				passLabel.Sensitive				= false;
				EnableEntry(passEntry, false);
				passEntry.Text					= "";
					
				savePasswordButton.Sensitive	= false;
				savePasswordButton.Active		= false;
					
				autoLoginButton.Sensitive		= false;
				autoLoginButton.Active			= false;
					
				defaultAccButton.Sensitive		= false;
				defaultAccButton.Active			= false;
					
				loginButton.Label				= Util.GS("_Log In");
				loginButton.Sensitive			= false;
			}

			// Hook the OnFieldsChanged handlers back up
			nameEntry.Changed 	+= new EventHandler(OnFieldsChanged);
			serverEntry.Changed	+= new EventHandler(OnFieldsChanged);
			passEntry.Changed	+= new EventHandler(OnFieldsChanged);
		}
		
		private void EnterNewAccountMode()
		{
			NewAccountMode = true;

			// Temporarily disable the selection handler so we can
			// select the new account without causing the handler
			// to be called.
			AccTreeView.Selection.Changed -=
				new EventHandler(AccSelectionChangedHandler);
			
			TreeSelection treeSelection = AccTreeView.Selection;
			treeSelection.UnselectAll();
			
			// hook'r back up
			AccTreeView.Selection.Changed +=
				new EventHandler(AccSelectionChangedHandler);
			
			AddButton.Sensitive				= false;
			RemoveButton.Sensitive			= false;
			DetailsButton.Sensitive			= false;
			
			serverLabel.Sensitive			= true;
			EnableEntry(serverEntry, true);
			serverEntry.Text				= "";
			
			nameLabel.Sensitive				= true;
			EnableEntry(nameEntry, true);
			nameEntry.Text					= "";
			
			passLabel.Sensitive				= true;
			EnableEntry(passEntry, true);
			passEntry.Text					= "";
			
			savePasswordButton.Sensitive	= true;
			savePasswordButton.Active		= false;
			
			autoLoginButton.Sensitive		= false;
			autoLoginButton.Active			= true;
			
			if (curDomains.Count == 0)
			{
				defaultAccButton.Sensitive	= false;
				defaultAccButton.Active		= true;
			}
			else
			{
				defaultAccButton.Sensitive	= true;
				defaultAccButton.Active		= false;
			}
			
			loginButton.Label				= Util.GS("_Log In");
			loginButton.Sensitive			= false;
			
			// Put the cursor focus on the serverEntry widget
			// This is a big hack, but I couldn't find any other way to
			// force Gtk to make the server entry get the focus.
			GLib.Idle.Add(SetFocusToServerEntry);
		}

		private void OnFieldsChanged(object obj, EventArgs args)
		{
			if(NewAccountMode)
			{
				loginButton.Label			= Util.GS("_Log In");

				if( (nameEntry.Text.Length > 0) &&
					(passEntry.Text.Length > 0 ) &&
					(serverEntry.Text.Length > 0) )
				{
					loginButton.Sensitive	= true;
				}
				else
					loginButton.Sensitive	= false;
			}
			else
			{
				DomainInformation dom = domainController.GetDomain(curDomain);
				if (dom == null) return;

				if (passEntry.Text.Length > 0)
				{
					savePasswordButton.Sensitive = true;
					if (dom.Active)
					{
						if (!dom.Authenticated)
							loginButton.Sensitive = true;
					}
				}
				else
				{
					savePasswordButton.Sensitive = false;
					if (dom.Active)
					{
						if (!dom.Authenticated)
							loginButton.Sensitive = false;
					}
					
					if (savePasswordButton.Active)
					{
						savePasswordButton.Active = false;
						SavePasswordNow();
					}
				}
			}
		}


		private void OnSavePasswordToggled(object obj, EventArgs args)
		{

			// If we are creating a new account, do nothing
			// it will be handled at creation time
			if(	(!NewAccountMode) && (savePasswordButton.HasFocus == true) )
			{
				DomainInformation dom = domainController.GetDomain(curDomain);
				if (dom == null) return;

				SavePasswordNow();
			}
		}


		private void OnPassEntryActivated(object o, EventArgs args)
		{
			if (loginButton.Label == Util.GS("_Log In"))
			{
				OnLoginAccount(null, null);
			}
			else if ( (!NewAccountMode) && (curDomainPassword != passEntry.Text) && 
						(savePasswordButton.Active == true ) )
			{
				SavePasswordNow();
			}
		}


		private void OnPassEntryFoucusOut(object o, FocusOutEventArgs args)
		{
			if( (!NewAccountMode) && (curDomainPassword != passEntry.Text) && 
						(savePasswordButton.Active == true ) )
			{
				SavePasswordNow();
			}
		}


		private void SavePasswordNow()
		{
			try
			{
				if( (savePasswordButton.Active == true) &&
						(passEntry.Text.Length > 0) )
				{
					domainController.SetDomainPassword(
						curDomain, passEntry.Text);
				}
				else
				{
					domainController.ClearDomainPassword(curDomain);
				}
				curDomainPassword = passEntry.Text;
			}
			catch (Exception ex)
			{
				// Ignore this error for now 
			}
		}


		private void OnAutoLoginToggled(object obj, EventArgs args)
		{
			// If we are creating a new account, do nothing
			// it will be handled at creation time
			if(!NewAccountMode && autoLoginButton.HasFocus == true)
			{
				DomainInformation dom = domainController.GetDomain(curDomain);
				if (dom == null) return;
				if(autoLoginButton.Active != dom.Active)
				{
					try
					{
						if(autoLoginButton.Active)
						{
							domainController.ActivateDomain(curDomain);
						}
						else
						{
							domainController.InactivateDomain(curDomain);
						}
					}
					catch (Exception ex)
					{
						// Ignore this error for now 
					}
				}
			}
		}

		private void OnDefAccToggled(object obj, EventArgs args)
		{
			// If we are creating a new account, do nothing
			// it will be handled at creation time
			if(!NewAccountMode && defaultAccButton.HasFocus == true)
			{
				// The default account checkbox is not sensitive (enabled) when
				// the default account is highlighted, so the only state that
				// needs to be handled here is when a user activates the
				// checkbox.
				if (!defaultAccButton.Active) return;

				try
				{
					domainController.SetDefaultDomain(curDomain);
				}
				catch (Exception e)
				{
					// FIXME: Popup a real error message to the user
					Console.WriteLine(e.Message);
				}
			}
		}

		public void OnDomainAddedEvent(object sender, DomainEventArgs args)
		{
			if (curDomains.ContainsKey(args.DomainID))
			{
				// Somehow we've already got this domain in our list, so
				// just update it.
				TreeIter iter = (TreeIter)curDomains[args.DomainID];
				AccTreeStore.SetValue(iter, 0, args.DomainID);
			}
			else
			{
				// This is a new domain we don't have in the list yet
				TreeIter iter = AccTreeStore.AppendValues(args.DomainID);
				curDomains[args.DomainID] = iter;

				// Highlight the new account
				if (NewAccountMode)
				{
					TreeSelection tSelect = AccTreeView.Selection;
					if(tSelect == null || tSelect.CountSelectedRows() == 0)
					{
						// Temporarily disable the selection handler so we can
						// select the new account without causing the handler
						// to be called.
						AccTreeView.Selection.Changed -=
							new EventHandler(AccSelectionChangedHandler);

						tSelect.SelectIter(iter);

						// hook'r back up
						AccTreeView.Selection.Changed +=
							new EventHandler(AccSelectionChangedHandler);
					}
				}
			}
		}
		
		public void OnDomainDeletedEvent(object sender, DomainEventArgs args)
		{
			if (curDomains.ContainsKey(args.DomainID))
			{
				TreeIter iter = (TreeIter)curDomains[args.DomainID];
				AccTreeStore.Remove(ref iter);
				curDomains.Remove(args.DomainID);
			}
		}
		
		public void OnDomainLoggedInEvent(object sender, DomainEventArgs args)
		{
			UpdateDomainStatus(args.DomainID);
		}
		
		public void OnDomainLoggedOutEvent(object sender, DomainEventArgs args)
		{
			UpdateDomainStatus(args.DomainID);
		}

		public void OnDomainActivatedEvent(object sender, DomainEventArgs args)
		{
			if (curDomain != null)
			{
				if (passEntry.Text.Length > 0)
					loginButton.Sensitive = true;
			}
			
			UpdateDomainStatus(args.DomainID);
		}

		public void OnDomainInactivatedEvent(object sender, DomainEventArgs args)
		{
			if (curDomain != null)
			{
				loginButton.Sensitive = false;
			}

			UpdateDomainStatus(args.DomainID);
		}

		public void OnNewDefaultDomainEvent(object sender, NewDefaultDomainEventArgs args)
		{
			TreeIter iter;
			DomainInformation dom;

			iter = (TreeIter)curDomains[args.NewDomainID];
			dom = domainController.GetDomain(args.NewDomainID);
			if (dom != null)
			{
				AccTreeStore.SetValue(iter, 0, dom.ID);
			}
			
			UpdateWidgetSensitivity();
		}
		
		public void OnDomainInGraceLoginPeriodEvent(object sender, DomainInGraceLoginPeriodEventArgs args)
		{
			DomainInformation dom = domainController.GetDomain(args.DomainID);
			iFolderMsgDialog dg =
				new iFolderMsgDialog(
					topLevelWindow,
					iFolderMsgDialog.DialogType.Error,
					iFolderMsgDialog.ButtonSet.Ok,
					dom != null ? dom.Name : "",
					Util.GS("Your password has expired"),
					string.Format(Util.GS("You have {0} grace logins remaining."), args.RemainingGraceLogins));
			dg.Run();
			dg.Hide();
			dg.Destroy();
		}
		
		/// <summary>
		/// This should be called anytime the authentication status of a domain
		/// changes because of events external to this page.
		/// </summary>
		public void UpdateDomainStatus(string domainID)
		{
			if (curDomains.ContainsKey(domainID))
			{
				TreeIter iter = (TreeIter)curDomains[domainID];
				DomainInformation dom = domainController.GetDomain(domainID);
				if (dom != null)
				{
					AccTreeStore.SetValue(iter, 0, dom.ID);
				}
				else
				{
					// Remove the domain from the list
					AccTreeStore.Remove(ref iter);
					curDomains.Remove(domainID);
				}
			}
			
			UpdateWidgetSensitivity();
		}

		public bool AllowLeavingAccountsPage()
		{
			if(NewAccountMode)
			{
				if( (nameEntry.Text.Length > 0) ||
					(passEntry.Text.Length > 0 ) ||
					(serverEntry.Text.Length > 0) )
				{
					iFolderMsgDialog dialog = new iFolderMsgDialog(
						topLevelWindow,
						iFolderMsgDialog.DialogType.Question,
						iFolderMsgDialog.ButtonSet.YesNo,
						"",
						Util.GS("Lose current settings?"),
						Util.GS("You are currently creating a new account.  By closing the window or changing to a different page you cancel the operation."));
					int rc = dialog.Run();
					dialog.Hide();
					dialog.Destroy();
					if(rc == -8)
					{
						NewAccountMode = false;
						UpdateWidgetSensitivity();

						return true;
					}

					return false;
				}

				NewAccountMode = false;
				UpdateWidgetSensitivity();
			}
			
			return true;
		}
		
		private void EnableEntry(Entry entry, bool bEnable)
		{
			if (bEnable)
			{
				entry.Sensitive = true;
				entry.Editable = true;
			}
			else
			{
				entry.Sensitive = false;
				entry.Editable = false;
			}
		}
	}
}
