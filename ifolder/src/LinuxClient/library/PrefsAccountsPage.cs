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

namespace Novell.iFolder
{

	/// <summary>
	/// A VBox with the ability to create and manage ifolder accounts
	/// </summary>
	public class PrefsAccountsPage : VBox
	{
		private Gtk.Window				topLevelWindow;
		private iFolderData				ifdata;
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
		private DomainInformation	curDomain;

		private Hashtable			curDomains;

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

			ifdata = iFolderData.GetData();
			
			curDomains = new Hashtable();

			InitializeWidgets();
			this.Realized += new EventHandler(OnRealizeWidget);
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

			AccTreeStore = new ListStore(typeof(DomainInformation));
			AccTreeView.Model = AccTreeStore;

			// Server Column
			TreeViewColumn serverColumn = new TreeViewColumn();
			serverColumn.Title = Util.GS("Server");
			CellRendererText servercr = new CellRendererText();
			servercr.Xpad = 5;
			serverColumn.PackStart(servercr, false);
			serverColumn.SetCellDataFunc(servercr,
										 new TreeCellDataFunc(ServerCellTextDataFunc));
			serverColumn.Resizable = true;
			serverColumn.MinWidth = 150;
			AccTreeView.AppendColumn(serverColumn);

			// User Name Column
			TreeViewColumn nameColumn = new TreeViewColumn();
			nameColumn.Title = Util.GS("User Name");
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
			serverLabel = new Label(Util.GS("Server host:"));
			serverLabel.Xalign = 1;
			loginTable.Attach(serverLabel, 0,1,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			serverEntry = new Entry();
			serverEntry.Changed += new EventHandler(OnFieldsChanged);
			serverEntry.ActivatesDefault = true;
			loginTable.Attach(serverEntry, 1,2,0,1,
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);


			// 2. User name
			nameLabel = new Label(Util.GS("User name:"));
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
					"_Remember password"));
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
				new Button(Util.GS("_Log in"));
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

 			detailsFrame.Sensitive = false;
			nameEntry.Sensitive = false;
			nameLabel.Sensitive = false;
			passEntry.Sensitive = false;
			passLabel.Sensitive = false;
			serverEntry.Sensitive = false;
			serverLabel.Sensitive = false;

			savePasswordButton.Sensitive = false;
			autoLoginButton.Sensitive = false;
			defaultAccButton.Sensitive = false;
			loginButton.Sensitive = false;

			RemoveButton.Sensitive = false;
			DetailsButton.Sensitive = false;

			// If there aren't any domains, have the "Add" button
			// be pressed automatically so the user doesn't have to
			// do that unnecessary step.
			if (curDomains.Count == 0)
			{
				OnAddAccount(null, null);
				serverEntry.HasFocus = true;

				// This is a big hack, but I couldn't find any other way to
				// force Gtk to make the server entry get the focus.
				GLib.Idle.Add(SetFocusToServerEntry);
			}
		}


		private bool SetFocusToServerEntry()
		{
			serverEntry.HasFocus = true;
			return false;
		}



		private void PopulateDomains()
		{
			DomainInformation[] domains = ifdata.GetDomains();

			foreach(DomainInformation dom in domains)
			{
				// only show Domains that are slaves (not on this machine)
				if(dom.IsSlave)
				{
					TreeIter iter = AccTreeStore.AppendValues(dom);
					curDomains[dom.ID] = iter;
				}
			}
		}




		private void OnRealizeWidget(object o, EventArgs args)
		{
			PopulateWidgets();

			// Select the first item in the TreeView
			if (AccTreeView.Selection != null)
			{
				AccTreeView.Selection.SelectPath(new TreePath("0"));
			}
		}




		private void ServerCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			DomainInformation dom = (DomainInformation) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = dom.Name;
		}




		private void NameCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			DomainInformation dom = (DomainInformation) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = dom.MemberName;
		}




		private void StatusCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			DomainInformation dom = (DomainInformation) tree_model.GetValue(iter,0);
			
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




		private void OnAddAccount(object o, EventArgs args)
		{
			bool bOtherDomainsExist = false;

			if(NewAccountMode == true)
			{
				// This shouldn't be possible but hey, deal with it
				return;
			}

			if (curDomains.Count > 0)
				bOtherDomainsExist = true;
			
			AccTreeView.Selection.UnselectAll();

			NewAccountMode = true;

			AddButton.Sensitive = false;
			RemoveButton.Sensitive = false;
			DetailsButton.Sensitive = false;

			// Set the control states
 			detailsFrame.Sensitive = true;
			nameEntry.Sensitive = true;
			nameEntry.Editable = true;
			nameLabel.Sensitive = true;
			passEntry.Sensitive = true;
			passEntry.Editable = true;
			passLabel.Sensitive = true;
			serverEntry.Sensitive = true;
			serverEntry.Editable = true;
			serverLabel.Sensitive = true; 

			savePasswordButton.Sensitive = true;
			autoLoginButton.Sensitive = false;

			// If there are other domains, at least one of them is already
			// marked as the default so we can give the user the option to
			// change that.  Otherwise, we need to force the new domain to
			// be the default.
			if (bOtherDomainsExist)
				defaultAccButton.Sensitive = true;
			else
				defaultAccButton.Sensitive = false;

			loginButton.Sensitive = false; 


			// set the control values
			savePasswordButton.Active = false;
			autoLoginButton.Active = true;
			if (bOtherDomainsExist)
				defaultAccButton.Active = false;
			else
				defaultAccButton.Active = true;

			nameEntry.Text = "";
			serverEntry.Text = "";
			passEntry.Text = "";

			loginButton.HasDefault = true;
			serverEntry.HasFocus = true;
		}



		private void OnRemoveAccount(object o, EventArgs args)
		{
			TreeSelection tSelect = AccTreeView.Selection;

			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				DomainInformation dom = 
					(DomainInformation) tModel.GetValue(iter, 0);

				RemoveAccountDialog rad = new RemoveAccountDialog(dom);
				rad.TransientFor = topLevelWindow;
				int rc = rad.Run();
				rad.Hide();
				if((ResponseType)rc == ResponseType.Yes)
				{
					try
					{
						simws.LeaveDomain(dom.ID, !(rad.RemoveiFoldersFromServer));
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

					ifdata.RemoveDomain(dom.ID);

					AccTreeStore.Remove(ref iter);
					curDomains.Remove(dom.ID);
					curDomain = null;
					detailsFrame.Sensitive = false;
					nameEntry.Sensitive = false;
					nameEntry.Text = "";
					nameLabel.Sensitive = false;
					passEntry.Sensitive = false;
					passEntry.Text = "";
					passLabel.Sensitive = false;
					serverEntry.Sensitive = false;
					serverLabel.Sensitive = false;
					serverEntry.Text = "";

					savePasswordButton.Sensitive = false;
					autoLoginButton.Sensitive = false;
					defaultAccButton.Sensitive = false;
					loginButton.Sensitive = false;

					RemoveButton.Sensitive = false;
					DetailsButton.Sensitive = false;

					// If the domain that we just removed was the default and
					// there are still remaining accounts, find out from Simias
					// what the new default domain is and update the UI.
					if (dom.IsDefault && curDomains.Count > 0)
					{
						try
						{
							string newDefaultDomainID = simws.GetDefaultDomainID();
							iter = (TreeIter)curDomains[newDefaultDomainID];
							
							dom = (DomainInformation) tModel.GetValue(iter, 0);
							dom.IsDefault = true;
						}
						catch {}
					}
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
				DomainInformation dom = 
						(DomainInformation) tModel.GetValue(iter, 0);

				AccountDialog accDialog = new AccountDialog(dom);
				accDialog.TransientFor = topLevelWindow;
				accDialog.Run();
				accDialog.Hide();
				accDialog.Destroy();
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
						Util.GS("iFolder Confirmation"),
						Util.GS("Lose current settings?"),
						Util.GS("You are currently creating a new account.  By selecing a different account you cancel the operation.  Do you wish to continue and loose the current settings?"));
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
					// go ahead and quit
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


			TreeSelection tSelect = AccTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				DomainInformation dom = 
						(DomainInformation) tModel.GetValue(iter, 0);

				curDomain = dom;

				// Set the control states
				AddButton.Sensitive = true;
				RemoveButton.Sensitive = true;
				DetailsButton.Sensitive = true;

 				detailsFrame.Sensitive = true;

				if (dom.Active && !dom.Authenticated)
				{
					serverEntry.Sensitive = true;
					serverEntry.Editable = true;
					serverLabel.Sensitive = true; 
				}
				else
				{
					serverEntry.Sensitive = true;
					serverEntry.Editable = false;
					serverLabel.Sensitive = false;
				}
				nameEntry.Editable = false;
				nameEntry.Sensitive = true;
				nameLabel.Sensitive = false;

				passEntry.Sensitive = true;
				passEntry.Editable = true;
				passLabel.Sensitive = true;

				savePasswordButton.Sensitive = true;
				autoLoginButton.Sensitive = true;

				// set the control values
				try
				{
					string userID;
					string credentials;
					CredentialType credType = simws.GetDomainCredentials(
						dom.ID, out userID, out credentials);
					if( (credentials != null) &&
						(credType == CredentialType.Basic) )
					{
						curDomainPassword = credentials;
						passEntry.Text = credentials;
						savePasswordButton.Active = true;
					}
					else
					{
						throw new Exception("Invalid Creds");
					}
				}
				catch(Exception e)
				{
					curDomainPassword = "";
					passEntry.Text = "";
					savePasswordButton.Active = false;
				}

				autoLoginButton.Active = dom.Active;
				defaultAccButton.Active = dom.IsDefault;
				defaultAccButton.Sensitive = !dom.IsDefault;

				if(dom.MemberName != null)
					nameEntry.Text = dom.MemberName;
				else
					nameEntry.Text = "";
				if(dom.Host != null)
					serverEntry.Text = dom.Host;
				else
					serverEntry.Text = "";
				
				if (dom.Active)
				{
					if (dom.Authenticated)
					{
						loginButton.Label = Util.GS("_Log out");
						loginButton.Sensitive = true;
					}
					else
					{
						loginButton.Label = Util.GS("_Log in");

						if (passEntry.Text.Length > 0)
							loginButton.Sensitive = true;
						else
							loginButton.Sensitive = false;
					}
				}
				else
				{
					loginButton.Sensitive = false;
					loginButton.Label = Util.GS("_Log in");
				}
			}
		}

		private void OnLoginButtonPressed(object o, EventArgs args)
		{
			TreeSelection tSelect = AccTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				if (loginButton.Label == Util.GS("_Log in"))
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
				DomainInformation dom = 
						(DomainInformation) tModel.GetValue(iter, 0);
				try
				{
					DomainAuthentication domainAuth = new DomainAuthentication("iFolder", dom.ID, null);
					domainAuth.Logout();

					dom.Authenticated = false;
					UpdateDomainStatus(dom.ID);

					// Update the login button
					AccSelectionChangedHandler(null, null);
				}
				catch (Exception ex)
				{
					iFolderMsgDialog dg = new iFolderMsgDialog(
						topLevelWindow,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.Ok,
						Util.GS("iFolder Error"),
						Util.GS("Unable to log out of iFolder Server"),
						Util.GS("An error was encountered while logging out of the iFolder server.  If the problem persists, please contact your network administrator."));
					dg.Run();
					dg.Hide();
					dg.Destroy();
				}
			}				
		}

		private void OnLoginAccount(object o, EventArgs args)
		{
			iFolderMsgDialog dg;
			
			try
			{
				DomainInformation domainInfo;
				if (NewAccountMode)
				{
					domainInfo = simws.ConnectToDomain(nameEntry.Text, passEntry.Text, serverEntry.Text);
				}
				else
				{
					domainInfo = curDomain;

					// The user has changed the server's host
					simws.SetDomainHostAddress(domainInfo.ID, serverEntry.Text);
					domainInfo.StatusCode = StatusCodes.Success;
				}
				
				// Just in case...
				if (domainInfo == null) return;
				
				switch (domainInfo.StatusCode)
				{
					case StatusCodes.InvalidCertificate:
					{
						byte[] byteArray = simws.GetCertificate(serverEntry.Text);
						System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(byteArray);

						iFolderMsgDialog dialog = new iFolderMsgDialog(
							topLevelWindow,
							iFolderMsgDialog.DialogType.Question,
							iFolderMsgDialog.ButtonSet.YesNo,
							Util.GS("Unable to Verify Identity"),
							Util.GS("Unable to Verify Identity"),
							string.Format(Util.GS("Unable to verify the identity of {0} as a trusted site.  Before accepting this certificate, you should examine the certificate by clicking Show Details.  Do you want to accept this certificate?"), serverEntry.Text),
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
						// Set the credentials in the current process.
						DomainAuthentication domainAuth = new DomainAuthentication("iFolder", domainInfo.ID, passEntry.Text);
						Status authStatus = domainAuth.Authenticate();
		
						switch (authStatus.statusCode)
						{
							case StatusCodes.Success:
							case StatusCodes.SuccessInGrace:
								TreeSelection sel = AccTreeView.Selection;

								domainInfo.Authenticated = true;

								if (NewAccountMode)
								{
									ifdata.AddDomain(domainInfo);
									NewAccountMode = false;
									TreeIter iter = AccTreeStore.AppendValues(domainInfo);
									curDomains[domainInfo.ID] = iter;
									curDomain = domainInfo;
									
									if (savePasswordButton.Active)
									{
										SavePasswordNow();
									}
										
									if (defaultAccButton.Active)
									{
										if (ifdata.SetDefaultDomain(curDomain))
										{
											defaultAccButton.Sensitive = false;
										}
									}
										
									sel.SelectIter(iter);
								}
								else
								{
									UpdateDomainStatus(domainInfo.ID);

									TreeIter iter = (TreeIter)curDomains[domainInfo.ID];
									sel.SelectIter(iter);

									// Update the login button
									AccSelectionChangedHandler(null, null);
								}

								if (authStatus.RemainingGraceLogins < authStatus.TotalGraceLogins)
								{
									dg = new iFolderMsgDialog(
										topLevelWindow,
										iFolderMsgDialog.DialogType.Error,
										iFolderMsgDialog.ButtonSet.Ok,
										Util.GS("iFolder Error"),
										Util.GS("Expired Password"),
										string.Format(Util.GS("Your password has expired.  You have {0} grace logins remaining."), authStatus.RemainingGraceLogins));
									dg.Run();
									dg.Hide();
									dg.Destroy();
								}
								
								break;
							case StatusCodes.InvalidCredentials:
							case StatusCodes.InvalidPassword:
								dg = new iFolderMsgDialog(
									topLevelWindow,
									iFolderMsgDialog.DialogType.Error,
									iFolderMsgDialog.ButtonSet.Ok,
									Util.GS("iFolder Error"),
									Util.GS("Unable to Connect to iFolder Server"),
									Util.GS("The user name or password is invalid.  Please try again."));
								dg.Run();
								dg.Hide();
								dg.Destroy();
								break;
							case StatusCodes.AccountDisabled:
								dg = new iFolderMsgDialog(
									topLevelWindow,
									iFolderMsgDialog.DialogType.Error,
									iFolderMsgDialog.ButtonSet.Ok,
									Util.GS("iFolder Error"),
									Util.GS("Unable to Connect to iFolder Server"),
									Util.GS("The user account is disabled.  Please contact your network administrator for assistance."));
								dg.Run();
								dg.Hide();
								dg.Destroy();
								break;
							case StatusCodes.AccountLockout:
								dg = new iFolderMsgDialog(
									topLevelWindow,
									iFolderMsgDialog.DialogType.Error,
									iFolderMsgDialog.ButtonSet.Ok,
									Util.GS("iFolder Error"),
									Util.GS("Unable to Connect to iFolder Server"),
									Util.GS("The user account has been locked out.  Please contact your network administrator for assistance."));
								dg.Run();
								dg.Hide();
								dg.Destroy();
								break;
							default:
								dg = new iFolderMsgDialog(
									topLevelWindow,
									iFolderMsgDialog.DialogType.Error,
									iFolderMsgDialog.ButtonSet.Ok,
									Util.GS("iFolder Error"),
									Util.GS("Unable to Connect to iFolder Server"),
									Util.GS("An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator."),
									string.Format("{0}: {1}", Util.GS("Authentication Status Code"), authStatus.statusCode));
								dg.Run();
								dg.Hide();
								dg.Destroy();
								break;
						}
						break;
					case StatusCodes.InvalidCredentials:
					case StatusCodes.InvalidPassword:
					case StatusCodes.UnknownUser:
						dg = new iFolderMsgDialog(
							topLevelWindow,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.Ok,
							Util.GS("iFolder Error"),
							Util.GS("Unable to Connect to iFolder Server"),
							Util.GS("The user name or password is invalid.  Please try again."));
						dg.Run();
						dg.Hide();
						dg.Destroy();
						break;
					case StatusCodes.AccountDisabled:
						dg = new iFolderMsgDialog(
							topLevelWindow,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.Ok,
							Util.GS("iFolder Error"),
							Util.GS("Unable to Connect to iFolder Server"),
							Util.GS("The user account is disabled.  Please contact your network administrator for assistance."));
						dg.Run();
						dg.Hide();
						dg.Destroy();
						break;
					case StatusCodes.AccountLockout:
						dg = new iFolderMsgDialog(
							topLevelWindow,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.Ok,
							Util.GS("iFolder Error"),
							Util.GS("Unable to Connect to iFolder Server"),
							Util.GS("The user account has been locked out.  Please contact your network administrator for assistance."));
						dg.Run();
						dg.Hide();
						dg.Destroy();
						break;
					case StatusCodes.UnknownDomain:
						dg = new iFolderMsgDialog(
							topLevelWindow,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.Ok,
							Util.GS("iFolder Error"),
							Util.GS("Unable to Connect to iFolder Server"),
							Util.GS("Unable to contact the specified server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator."));
						dg.Run();
						dg.Hide();
						dg.Destroy();
						break;
					default:
						dg = new iFolderMsgDialog(
							topLevelWindow,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.Ok,
							Util.GS("iFolder Error"),
							Util.GS("Unable to Connect to iFolder Server"),
							Util.GS("An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator."),
							string.Format("{0}: {1}", Util.GS("Authentication Status Code"), domainInfo.StatusCode));
						dg.Run();
						dg.Hide();
						dg.Destroy();
						break;
				}
			}
			catch (Exception ex)
			{
				dg = new iFolderMsgDialog(
					topLevelWindow,
					iFolderMsgDialog.DialogType.Error,
					iFolderMsgDialog.ButtonSet.Ok,
					Util.GS("iFolder Error"),
					Util.GS("Unable to Connect to iFolder Server"),
					Util.GS("An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator."),
					Util.GS(ex.Message));
				dg.Run();
				dg.Hide();
				dg.Destroy();
			}
		}


		private void OnFieldsChanged(object obj, EventArgs args)
		{
			if(NewAccountMode)
			{
				loginButton.Label = Util.GS("_Log in");

				if( (nameEntry.Text.Length > 0) &&
					(passEntry.Text.Length > 0 ) &&
					(serverEntry.Text.Length > 0) )
				{
					loginButton.Sensitive = true;
				}
				else
					loginButton.Sensitive = false;
			}
			else
			{
				if (passEntry.Text.Length > 0)
					loginButton.Sensitive = true;
				else
					loginButton.Sensitive = false;
			}
		}


		private void OnSavePasswordToggled(object obj, EventArgs args)
		{
			// If we are creating a new account, do nothing
			// it will be handled at creation time
			if(	(!NewAccountMode) && (savePasswordButton.HasFocus == true) )
			{
				SavePasswordNow();
			}
		}


		private void OnPassEntryActivated(object o, EventArgs args)
		{
			if( (!NewAccountMode) && (curDomainPassword != passEntry.Text) && 
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
					simws.SetDomainCredentials(curDomain.ID, 
							passEntry.Text, CredentialType.Basic);
				}
				else
				{
					simws.SetDomainCredentials(curDomain.ID, null,
							CredentialType.None);
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
			if(!NewAccountMode)
			{
				if(autoLoginButton.Active != curDomain.Active)
				{
					try
					{
						if(autoLoginButton.Active)
						{
							simws.SetDomainActive(curDomain.ID);
							curDomain.Active = true;
							
							if (passEntry.Text.Length > 0)
								loginButton.Sensitive = true;
							
							UpdateDomainStatus(curDomain.ID);
						}
						else
						{
							simws.SetDomainInactive(curDomain.ID);
							curDomain.Active = false;
							loginButton.Sensitive = false;
							
							UpdateDomainStatus(curDomain.ID);
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
			if(!NewAccountMode)
			{
				if( (defaultAccButton.Active != curDomain.IsDefault) &&
					(defaultAccButton.Active == true ) )
				{
					defaultAccButton.Sensitive = 
							ifdata.SetDefaultDomain(curDomain);
				}
			}
		}
		
		/// <summary>
		/// This should be called anytime the authentication status of a domain
		/// changes because of events external to this page.
		/// </summary>
		public void UpdateDomainStatus(string domainID)
		{
			if (curDomains.Contains(domainID))
			{
				TreeIter iter = (TreeIter)curDomains[domainID];
				DomainInformation dom = ifdata.GetDomain(domainID);
				if (dom != null)
				{
					AccTreeStore.SetValue(iter, 0, dom);

					// Use AccSelectionChangedHandler() to update the
					// status and login button based on the state of the domain.
					AccSelectionChangedHandler(null, null);
					// 
//					if (dom.Active)
//					{
//						if (dom.Authenticated)
//						{
//							loginButton.Label = Util.GS("_Log out");
//							loginButton.Sensitive = true;
//					}
				}
				else
				{
					// Remove the domain from the list
					AccTreeStore.Remove(ref iter);
					curDomains.Remove(domainID);
				}
			}
			else
			{
				// Rebuild the entire list
				curDomains.Clear();
				AccTreeStore.Clear();
	
				PopulateDomains();
			}
		}
	}
}
