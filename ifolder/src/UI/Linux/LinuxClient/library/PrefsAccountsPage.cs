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
  *                 $Author: Calvin Gaisford <cgaisford@novell.com>
  *                          Boyd Timothy <btimothy@novell.com>
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
using System.IO;
using System.Collections;

using Gtk;
using Simias.Client.Event;
using Simias.Client;
using Simias.Client.Authentication;

using Novell.iFolder.Events;
using Novell.iFolder.Controller;
using Novell.iFolder.DomainProvider;

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

		private CellRendererToggle	onlineToggleButton;

		private Button 				AddButton;
		private Button				RemoveButton;
		private Button				DetailsButton;

		private Hashtable			curDomains;
		
		// Keep track of which domains have been removed so that we can
		// remove them from the UI immediately and not wait until we
		// receive an event from simias.
		private Hashtable			removedDomains;
		
		private DomainController	domainController;
		
		// Hashtable used to keep track of the details
		// dialogs that are spawned so that we don't open
		// multiple for the same account.
		private Hashtable			detailsDialogs;
		
		private Manager				simiasManager;
		
		private iFolderWaitDialog	WaitDialog;
		
		private DomainProviderUI domainProviderUI;
		
		private iFolderLoginDialog	LoginDialog;

		private iFolderWebService       ifws;
		private iFolderMsgDialog        ClientUpgradeDialog;
		private Status ClientUpgradeStatus;
		private string NewClientVersion;
		private string NewClientDomainID;


		/// <summary>
		/// Default constructor for iFolderAccountsPage
		/// </summary>
		public PrefsAccountsPage( Gtk.Window topWindow )
			: base()
		{
			this.topLevelWindow = topWindow;
			this.simiasManager = Util.GetSimiasManager();
			string localServiceUrl = simiasManager.WebServiceUri.ToString();
			ifws = new iFolderWebService();
			ifws.Url = localServiceUrl + "/iFolder.asmx";
			LocalService.Start(ifws, simiasManager.WebServiceUri, simiasManager.DataPath);

			this.simws = new SimiasWebService();
			simws.Url = simiasManager.WebServiceUri.ToString() +
					"/Simias.asmx";
			LocalService.Start(simws, simiasManager.WebServiceUri, simiasManager.DataPath);
			ClientUpgradeDialog = null;

			this.ClientUpgradeStatus = null;
			this.NewClientVersion = null;
			this.NewClientDomainID = null;

			curDomains = new Hashtable();
			
			removedDomains = new Hashtable();

			InitializeWidgets();
			
			domainProviderUI = DomainProviderUI.GetDomainProviderUI();

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
				domainController.DomainClientUpgradeAvailable +=
					new DomainClientUpgradeAvailableEventHandler(OnClientUpgradeAvailableEvent);
			}
			
			detailsDialogs = new Hashtable();

			this.Realized += new EventHandler(OnRealizeWidget);
		}
		
        /// <summary>
        /// Destructor
        /// </summary>
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
				domainController.DomainClientUpgradeAvailable -=
					new DomainClientUpgradeAvailableEventHandler(OnClientUpgradeAvailableEvent);
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
			
			// Set up the Accounts tree view in a scrolled window
			AccTreeView = new iFolderTreeView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			sw.Add(AccTreeView);
			this.PackStart(sw, true, true, 0);

			AccTreeStore = new ListStore(typeof(string));
			AccTreeView.Model = AccTreeStore;

			// Online Column
			TreeViewColumn onlineColumn = new TreeViewColumn();
			onlineColumn.Title = Util.GS("Online");
			onlineToggleButton = new CellRendererToggle();
			onlineToggleButton.Xpad = 5;
			onlineToggleButton.Xalign = 0.5F;
			onlineColumn.PackStart(onlineToggleButton, true);
			onlineColumn.SetCellDataFunc(onlineToggleButton,
				new TreeCellDataFunc(OnlineCellToggleDataFunc));
			
			onlineToggleButton.Toggled += new ToggledHandler(OnlineToggled);
			AccTreeView.AppendColumn(onlineColumn);

			// Server Column
			TreeViewColumn serverColumn = new TreeViewColumn();
			serverColumn.Title = Util.GS("Account Name");
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
			nameColumn.Title = Util.GS("User Name");
			CellRendererText ncrt = new CellRendererText();
			nameColumn.PackStart(ncrt, false);
			nameColumn.SetCellDataFunc(ncrt,
									   new TreeCellDataFunc(NameCellTextDataFunc));
			nameColumn.Resizable = true;
			nameColumn.MinWidth = 150;
			AccTreeView.AppendColumn(nameColumn);

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

			DetailsButton = new Button(Gtk.Stock.Properties);
			buttonBox.PackStart(DetailsButton);
			DetailsButton.Clicked += new EventHandler(OnDetailsClicked);

			AccTreeView.RowActivated += new RowActivatedHandler(
						OnAccTreeRowActivated);
		}




		/// <summary>
		/// Set the Values in the Widgets
		/// </summary>
		private void PopulateWidgets()
		{
			PopulateDomains();
			
			UpdateWidgetSensitivity();
		}

        /// <summary>
        /// Populate Domains
        /// </summary>
		private void PopulateDomains()
		{
			DomainInformation[] domains = domainController.GetDomains();

			foreach(DomainInformation dom in domains)
			{
				string domainID = dom.ID;

				// only show Domains that are slaves (not on this machine) and
				// those for which an IDomainProviderUI has been written
				if(dom.IsSlave ||
					domainProviderUI.GetProviderForID(domainID) != null)
				{
					TreeIter iter = AccTreeStore.AppendValues(domainID);
					curDomains[domainID] = iter;
				}
			}
		}
		/*private void nothing()
		{
			return;
		}*/

        /// <summary>
        /// Event Handler for Realize Widget
        /// </summary>
        private void OnRealizeWidget(object o, EventArgs args)
		{
			PopulateWidgets();
		}




		private void OnlineCellToggleDataFunc(Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			string domainID = (string) tree_model.GetValue(iter, 0);
			DomainInformation dom = domainController.GetDomain(domainID);
			
			IDomainProviderUI provider = domainProviderUI.GetProviderForID(domainID);
			if (provider != null)
			{
				if (dom.Active)
					((CellRendererToggle) cell).Active = true;
				else
					((CellRendererToggle) cell).Active = false;
			}
			else
			{
				if (dom != null && dom.Authenticated)
					((CellRendererToggle) cell).Active = true;
				else
					((CellRendererToggle) cell).Active = false;
			}
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


        /// <summary>
        /// Event Handler for Add Account event
        /// </summary>
        private void OnAddAccount(object o, EventArgs args)
		{
			AddAccountWizard aaw = new AddAccountWizard(simws);
			aaw.TransientFor = topLevelWindow;
			if (!Util.RegisterModalWindow(aaw))
			{
				try
				{
					Util.CurrentModalWindow.Present();
				}
				catch{}
				aaw.Destroy();
				return;
			}
			aaw.ShowAll();
		}

        /// <summary>
        /// Event Handler for Remove Account
        /// </summary>
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
						removedDomains[dom.ID] = dom.ID;
                        
						domainController.RemoveDomain(dom.ID, rad.RemoveiFoldersFromServer);

						RemoveDomain(dom.ID);
					}
					catch(Exception e)
					{
						if (removedDomains.ContainsKey(dom.ID))
							removedDomains.Remove(dom.ID);

						iFolderExceptionDialog ied = 
							new iFolderExceptionDialog( topLevelWindow, e);
						ied.Run();
						ied.Hide();
						ied.Destroy();
						rad.Destroy();	// Clean up before bailing
						return;
					}

					AddButton.Sensitive = true;
					RemoveButton.Sensitive = false;
					DetailsButton.Sensitive = false;
				}

				rad.Destroy();
			}
		}


		private void OnAccTreeRowActivated(object o, RowActivatedArgs args)
		{
			OnDetailsClicked(o, args);
		}

        /// <summary>
        /// Event Handler for Details Clicked
        /// </summary>
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
					IDomainProviderUI provider = domainProviderUI.GetProviderForID(domainID);
					if (provider != null)
						accDialog = provider.CreateAccountDialog(topLevelWindow, dom);
					else
						accDialog = new EnterpriseAccountDialog(topLevelWindow, dom);
					
					if (accDialog != null)
					{
						detailsDialogs[domainID] = accDialog;
						accDialog.SetPosition(WindowPosition.Center);
						accDialog.Destroyed += 
								new EventHandler(OnAccountDialogDestroyedEvent);
	
						accDialog.ShowAll();
					}
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
			UpdateWidgetSensitivity();
		}
		
		private void OnlineToggled(object o, ToggledArgs args)
		{
			// Disable the ability for the user to toggle the checkbox
			onlineToggleButton.Activatable = false;

			TreeIter iter;
			TreePath path = new TreePath(args.Path);
			if (AccTreeStore.GetIter(out iter, path))
			{
				string domainID = (string)AccTreeStore.GetValue(iter, 0);
				DomainInformation dom = domainController.GetDomain(domainID);
				IDomainProviderUI provider = domainProviderUI.GetProviderForID(domainID);
				if (provider != null)
				{
					// FIXME: Add some functionality into the provider interface so we know what to do instead of just inactivating the account
					if (dom.Active)
						domainController.InactivateDomain(dom.ID);
					else
						domainController.ActivateDomain(dom.ID);
				}
				else
				{
					if (dom != null)
					{
						if (!dom.Authenticated)
							LoginDomain(dom);
						else
							LogoutDomain(dom);
					}
				}
				UpdateDomainStatus(dom.ID);
			}
			
			// Reenable the ability for the user to toggle the checkbox
			onlineToggleButton.Activatable = true;
		}
		
        /// <summary>
        /// Login to Domain
        /// </summary>
        /// <param name="dom">Domain Information</param>
		public void LoginDomain(DomainInformation dom)
		{
			try
			{
				LoginDialog =
					new iFolderLoginDialog(dom.ID, dom.Name, dom.MemberName);
				if (!Util.RegisterModalWindow(LoginDialog))
				{
					LoginDialog.Destroy();
					LoginDialog = null;
					return;
				}
				
				LoginDialog.Response +=
					new ResponseHandler(OnLoginDialogResponse);
				
				LoginDialog.ShowAll();
				
				string password = domainController.GetDomainPassword(dom.ID);
				if (password != null)
				{
					LoginDialog.Hide();
					LoginDialog.Password = password;
					LoginDialog.Respond(Gtk.ResponseType.Ok);
				}
			}
			catch
			{
				Util.ShowLoginError(topLevelWindow, StatusCodes.Unknown);

				UpdateDomainStatus(dom.ID);
			}
		}
		
        /// <summary>
        /// Event Handler for Login Dialog Response event
        /// </summary>
        private void OnLoginDialogResponse(object o, ResponseArgs args)
		{
			switch (args.ResponseId)
			{
				case Gtk.ResponseType.Ok:
					DomainInformation dom = domainController.GetDomain(LoginDialog.Domain);

					if (WaitDialog != null)
					{
						WaitDialog.Hide();
						WaitDialog.Destroy();
						WaitDialog = null;
					}
					
					VBox vbox = new VBox(false, 0);
					Image connectingImage = new Image(Util.ImagesPath("ifolder-add-account48.png"));
					vbox.PackStart(connectingImage, false, false, 0);
		
					WaitDialog = 
						new iFolderWaitDialog(
							topLevelWindow,
							vbox,
							iFolderWaitDialog.ButtonSet.None,
							Util.GS("Connecting..."),
							Util.GS("Connecting..."),
							Util.GS("Please wait while your iFolder account is connecting."));
	
					if (!Util.RegisterModalWindow(WaitDialog))
					{
						try
						{
							Util.CurrentModalWindow.Present();
						}
						catch{}
						WaitDialog.Destroy();
						return;
					}
					WaitDialog.Show();
					
					DomainLoginThread domainLoginThread =
						new DomainLoginThread(domainController);
					
					domainLoginThread.Completed +=
						new DomainLoginCompletedHandler(OnDomainLoginCompleted);
						
					domainLoginThread.Login(dom.ID, LoginDialog.Password, LoginDialog.ShouldSavePassword);

					break;
				case Gtk.ResponseType.Cancel:
				case Gtk.ResponseType.DeleteEvent:
					LoginDialog.Hide();
					LoginDialog.Destroy();
					LoginDialog = null;
					break;
			}
		}
	
	
		/// Now thw event handler does not show the upgrade dialog box directly, it will store the relevant informations
		/// Just after this, there will be successful login event, so there ShowClientUpgradeMessageBox will be called
		/// to show the upgrade dialog box with all the informations stored here
		private void OnClientUpgradeAvailableEvent(object sender, DomainClientUpgradeAvailableEventArgs args) {

                        this.ClientUpgradeStatus = DomainController.upgradeStatus;
                        this.NewClientVersion = args.NewClientVersion;
			this.NewClientDomainID = args.DomainID;
		}

		/// This method is called by Successful login handler, it is called before passphrase verify invocation
		/// The variable used in this method should have been captured during the ClientUpgrade Event handler
		/// This method should only be called during toggling of checkbox on prefs/account page (logout/login)
		public void ShowClientUpgradeMessageBox()
		{
			if(this.NewClientVersion == null || this.ClientUpgradeStatus == null || this.NewClientDomainID == null)
			{
				return; // no handler was generated/caught for ClientUpgradeAvailable
			}

			if (ClientUpgradeDialog != null)
				return;	// This dialog is already showing
			if(DomainController.upgradeStatus.statusCode == StatusCodes.ServerOld)
			{
				ClientUpgradeDialog = new iFolderMsgDialog(
				null,
				iFolderMsgDialog.DialogType.Info,
				iFolderMsgDialog.ButtonSet.Ok,
				Util.GS("iFolder Server Older"),
				Util.GS("The server is running an older version."),
				string.Format(Util.GS("The server needs to be upgraded to be connected from this client")));
			
			}
			else if(DomainController.upgradeStatus.statusCode == StatusCodes.UpgradeNeeded)
			{
				ClientUpgradeDialog = new iFolderMsgDialog(
				null,
				iFolderMsgDialog.DialogType.Info,
				iFolderMsgDialog.ButtonSet.AcceptDeny,
				Util.GS("iFolder Client Upgrade"),
				Util.GS("Would you like to download new iFolder Client?"),
				string.Format(Util.GS("The client needs to be upgraded to be connected to the server")));
			}
			else  
			{
				ClientUpgradeDialog = new iFolderMsgDialog(
				null,
				iFolderMsgDialog.DialogType.Info,
				iFolderMsgDialog.ButtonSet.AcceptDeny,
				Util.GS("iFolder Client Upgrade"),
				Util.GS("Would you like to download new iFolder Client?"),
				string.Format(Util.GS("A newer version \"{0}\" of the iFolder Client is available."), this.NewClientVersion));
			}

			int rc = ClientUpgradeDialog.Run();
			ClientUpgradeDialog.Hide();
			ClientUpgradeDialog.Destroy();
			ClientUpgradeDialog = null;
			
			if (rc == -8)
			{
				bool bUpdateRunning = false;
				Gtk.Window win = new Gtk.Window("");
				string initialPath = (string)System.IO.Path.GetTempPath();
				
				Debug.PrintLine(String.Format("Initial Path: {0}", initialPath));
				CopyLocation cp = new CopyLocation(win, (string)System.IO.Path.GetTempPath());
				string selectedFolder = "";
	                        int rc1 = 0;
        	                do
                	        {
                        	        rc1 = cp.Run();
                                	cp.Hide();
	                                if(rc1 ==(int)ResponseType.Ok)
        	                        {
                	                        selectedFolder = cp.iFolderPath.Trim();
                                		cp.Destroy();
	                                        cp = null;
        	                                break;
                	                }
                       		 }while( rc1 == (int)ResponseType.Ok);
				if( cp != null)
				{
					cp.Destroy();
					cp=null;
				}
				win.Hide();
				win.Destroy();
				win=null;
				if( rc1 != (int) ResponseType.Ok)
				{
					Debug.PrintLine("OnClientUpgradeAvailableEvent return");
					ClientUpgradeDialog = null;
					return;
				}
				try
				{
					if(ifws !=null)
					{
						Debug.PrintLine("ifws.RunClientUpdate");
						bUpdateRunning = ifws.RunClientUpdate(this.NewClientDomainID, selectedFolder);
					}
				}
				catch(Exception e)
				{
					Debug.PrintLine(String.Format("ifws.RunClientUpdate exception :{0}", e.Message));
					ClientUpgradeDialog = null;
					return;
				}
				
				if (bUpdateRunning)
				{
				ClientUpgradeDialog = new iFolderMsgDialog(
				null,
				iFolderMsgDialog.DialogType.Info,
				iFolderMsgDialog.ButtonSet.Ok,
				Util.GS("Download Complete..."),
				Util.GS("Download Finished "),
				string.Format(Util.GS("The new client rpm's have been downloaded.")));
				ClientUpgradeDialog.Run();
				ClientUpgradeDialog.Hide();
				ClientUpgradeDialog.Destroy();
				//	QuitiFolder();
				}
				else
				{
					iFolderMsgDialog dialog = new iFolderMsgDialog(
						null,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.None,
						Util.GS("Upgrade Failure"),
						Util.GS("The iFolder client upgrade failed."),
						Util.GS("Please contact your system administrator."));
					dialog.Run();
					dialog.Hide();
					dialog.Destroy();
					dialog = null;
				}
				
				if( DomainController.upgradeStatus.statusCode == StatusCodes.UpgradeNeeded )
				{
					// Deny login
					if( domainController.GetDomain(this.NewClientDomainID) != null)
						domainController.RemoveDomain(this.NewClientDomainID, false);
				}

			}
			else //if(rc == -9)
			{
				if(DomainController.upgradeStatus.statusCode == StatusCodes.ServerOld || DomainController.upgradeStatus.statusCode == StatusCodes.UpgradeNeeded )
				{
					// Deny login
					if( domainController.GetDomain(this.NewClientDomainID) != null)
						domainController.RemoveDomain(this.NewClientDomainID, false);
				}
			}
			ClientUpgradeDialog = null;
			this.ClientUpgradeStatus = null;
			this.NewClientVersion = null;
			this.NewClientDomainID = null;			
		}

		
        /// <summary>
        /// Event handler for Domain Login COmpleted
        /// </summary>
        private void OnDomainLoginCompleted(object o, DomainLoginCompletedArgs args)
		{
			if (WaitDialog != null)
			{
				WaitDialog.Hide();
				WaitDialog.Destroy();
				WaitDialog = null;
			}

			Status authStatus = args.AuthenticationStatus;

			if (authStatus != null)
			{
				switch (authStatus.statusCode)
				{
					case StatusCodes.Success:
					case StatusCodes.SuccessInGrace:
						if (LoginDialog != null)
						{
							LoginDialog.Hide();
							LoginDialog.Destroy();
							LoginDialog = null;
						}
						// Check if any recovery agent present;
						// if( domainController.GetRAList(args.DomainID) == null)
						// {
							// No recovery agent present;
					//		return;
						// }
						ShowClientUpgradeMessageBox();
						iFolderWebService ifws = DomainController.GetiFolderService();
						int policy = ifws.GetSecurityPolicy(args.DomainID);
						if( policy % 2 == 0)
							break;
						bool passphraseStatus = simws.IsPassPhraseSet(args.DomainID);
						if(passphraseStatus == true)
						{
							bool rememberOption = simws.GetRememberOption(args.DomainID);
							if( rememberOption == false)
							{
								ShowVerifyDialog( args.DomainID, simws);
							}
							else 
							{
								Debug.PrintLine(" remember Option true. Checking for passphrase existence");
								string passphrasecheck = 	simws.GetPassPhrase(args.DomainID);
								if(passphrasecheck == null || passphrasecheck == "")
								{
									Debug.PrintLine("BugBug: Passphrase doesn't exist");
									ShowVerifyDialog( args.DomainID, simws);
								}
							}
						}
						else
						{
							iFolderWindow.ShowEnterPassPhraseDialog(args.DomainID, simws);
						}
						
//						string[] array = domainController.GetRAList( args.DomainID);
						iFolderData ifdata = iFolderData.GetData();
                        ifdata.Refresh();
						UpdateWidgetSensitivity();
						break;
					case StatusCodes.InvalidCertificate:
						DomainInformation dom = domainController.GetDomain(args.DomainID);
						if( authStatus.UserName != null)
						{
							dom.Host = authStatus.UserName;
						}
						byte[] byteArray = simws.GetCertificate(dom.Host);
						System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(byteArray);

						iFolderMsgDialog dialog = new iFolderMsgDialog(
							null,
							iFolderMsgDialog.DialogType.Question,
							iFolderMsgDialog.ButtonSet.YesNo,
							"",
							Util.GS("Accept the certificate of this server?"),
							string.Format(Util.GS("iFolder is unable to verify \"{0}\" as a trusted server.  You should examine this server's identity certificate carefully."), dom.Host),
							cert.ToString(true));

						Gdk.Pixbuf certPixbuf = new Gdk.Pixbuf(Util.ImagesPath("ifolder-application-x-x509-ca-cert_48.png"));
						if (certPixbuf != null && dialog.Image != null)
							dialog.Image.Pixbuf = certPixbuf;

						int rc = dialog.Run();
						dialog.Hide();
						dialog.Destroy();
						if(rc == -8) // User clicked the Yes button
						{
							simws.StoreCertificate(byteArray, dom.Host);
							LoginDialog.Respond(Gtk.ResponseType.Ok);
						}
						else
						{
							LoginDialog.Respond(Gtk.ResponseType.Cancel);
						}
						break;
						case StatusCodes.UserAlreadyMoved:
							LoginDialog.Respond(Gtk.ResponseType.Ok);
							break;
					default:
						Util.ShowLoginError(topLevelWindow, authStatus.statusCode);
						
						if (LoginDialog != null)
							LoginDialog.Present();
	
						UpdateDomainStatus(args.DomainID);
						break;
				}
				//UpdateiFolderWindowOnLoginComplete();
			}
			else
			{
				Util.ShowLoginError(topLevelWindow, StatusCodes.Unknown);
				
				if (LoginDialog != null)
					LoginDialog.Present();

				UpdateDomainStatus(args.DomainID);
			}

			iFolderWindow ifwin = Util.GetiFolderWindow();
			ifwin.UpdateServerInfoForSelectedDomain();	
			ifwin.UpdateListViewItems();
		}

        /// <summary>
        /// Show Verify Passphrase Dialog
        /// </summary>
        /// <param name="DomainID">Domain ID</param>
        /// <param name="simws">Simias WebService</param>
        /// <returns>true on success</returns>
		private bool ShowVerifyDialog(string DomainID, SimiasWebService simws)
		{
			bool status = false;
			int result;
			Status passPhraseStatus= null;
			VerifyPassPhraseDialog vpd = new VerifyPassPhraseDialog();
			if (!Util.RegisterModalWindow(vpd))
			{
				vpd.Destroy();
				vpd = null;
				return false;
			}
			// vpd.TransientFor = this;
			try
			{
			do
			{
				result = vpd.Run();
				vpd.Hide();
				// Verify PassPhrase..  If correct store passphrase and set a local property..
				if( result == (int)ResponseType.Cancel || result == (int)ResponseType.DeleteEvent)
					break;
				if( result == (int)ResponseType.Ok)
				{
					passPhraseStatus =  simws.ValidatePassPhrase(DomainID, vpd.PassPhrase);
				}
				if( passPhraseStatus != null)
				{
					if( passPhraseStatus.statusCode == StatusCodes.PassPhraseInvalid)  // check for invalid passphrase
					{
						// Display an error Message
						iFolderMsgDialog dialog = new iFolderMsgDialog(
							null,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.None,
							Util.GS("Invalid passPhrase"),
							Util.GS("The PassPhrase entered is invalid"),
							Util.GS("Please re-enter the passphrase"));
							dialog.Run();
							dialog.Hide();
							dialog.Destroy();
							dialog = null;
						passPhraseStatus = null;
					}
					else if(passPhraseStatus.statusCode == StatusCodes.Success)
						break;
				}
			}while( result != (int)ResponseType.Cancel && result !=(int)ResponseType.DeleteEvent);
			if(result == (int)ResponseType.Cancel || result ==(int)ResponseType.DeleteEvent)
			{
				status = false;
				simws.StorePassPhrase(DomainID, "", CredentialType.None, false);
				/*  Testing purpose
				string uid, passphrasecheck;
				simws.GetPassPhrase(DomainID, out uid, out passphrasecheck);
				*/
			}
			else if( passPhraseStatus != null && passPhraseStatus.statusCode == StatusCodes.Success)
			{
				try
				{
					simws.StorePassPhrase( DomainID, vpd.PassPhrase, CredentialType.Basic, vpd.ShouldSavePassPhrase);
					status = true;
				}
				catch(Exception) 
				{
					return false;
				}
			}
			}
			catch(Exception)
			{
				return false;
			}
			return status;
		}
		
        /// <summary>
        /// Logout
        /// </summary>
        /// <param name="dom">Domain Information</param>
		public void LogoutDomain(DomainInformation dom)
		{
			try
			{
				domainController.LogoutDomain(dom.ID);

				dom.Authenticated = false;

				// In the new accounts model, when a user makes an
				// account go offline, we should also disable the account.
//				if (dom.Active)
//				{
//					domainController.InactivateDomain(dom.ID);
//					UpdateDomainStatus(dom.ID);
//				}
//				else
//				{
					//UpdateDomainStatus(dom.ID);
					iFolderData ifdata = iFolderData.GetData();
					ifdata.Refresh();
//				}
			}
			catch (Exception)
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

        /// <summary>
        /// Update Widget Sensitivity
        /// </summary>
		private void UpdateWidgetSensitivity()
		{
			TreeSelection tSelect = AccTreeView.Selection;

			// Nothing is selected
			AddButton.Sensitive= true;
			RemoveButton.Sensitive= false;
			DetailsButton.Sensitive= false;
			if(tSelect != null)	
			{	
				if(tSelect.CountSelectedRows() == 1)
				{
					TreeModel tModel;
					TreeIter iter;
	
					tSelect.GetSelected(out tModel, out iter);
					string domainID = (string) tModel.GetValue(iter, 0);
					DomainInformation dom = domainController.GetDomain(domainID);
					if (dom == null) return;	// Prevent null pointer
				
					IDomainProviderUI provider = domainProviderUI.GetProviderForID(domainID);
					if (provider != null)
					{
						if (provider.CanDelete)
							RemoveButton.Sensitive = true;
						else
							RemoveButton.Sensitive = false;
					
						if (provider.HasDetails)
							DetailsButton.Sensitive = true;
						else
							DetailsButton.Sensitive = false;
					}
					else
					{
						RemoveButton.Sensitive	= true;
						DetailsButton.Sensitive	= true;
					}

					// Set the control states
					AddButton.Sensitive= true;
				}
			}
		}
		
		public void OnDomainAddedEvent(object sender, DomainEventArgs args)
		{
			if (removedDomains.ContainsKey(args.DomainID))
				removedDomains.Remove(args.DomainID);
		
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
				TreeSelection tSelect = AccTreeView.Selection;
				if(tSelect != null)
					tSelect.SelectIter(iter);
			}
		}
		
        /// <summary>
        /// Remove Domain
        /// </summary>
        /// <param name="domainID">Domain ID</param>
		private void RemoveDomain(string domainID)
		{
			if (curDomains.ContainsKey(domainID))
			{
				TreeIter iter = (TreeIter)curDomains[domainID];
				AccTreeStore.Remove(ref iter);
				curDomains.Remove(domainID);
			}
			
			if (curDomains.Count == 0)
			{
				// Hide the iFolder Window if it's visible
				iFolderWindow ifwin = Util.GetiFolderWindow();
				if (ifwin.Visible)
					ifwin.CloseWindow();
			}
		}
		
		public void OnDomainDeletedEvent(object sender, DomainEventArgs args)
		{
			if (removedDomains.ContainsKey(args.DomainID))
			{
				removedDomains.Remove(args.DomainID);
				return;
			}
			else
			{
				RemoveDomain(args.DomainID);
			}
		}
		
		public void OnDomainLoggedInEvent(object sender, DomainEventArgs args)
		{
			//UpdateDomainStatus(args.DomainID);
			//iFolderData ifdata = iFolderData.GetData();
			//ifdata.Refresh();
		}
		
		public void OnDomainLoggedOutEvent(object sender, DomainEventArgs args)
		{
			//UpdateDomainStatus(args.DomainID);
		}

		public void OnDomainActivatedEvent(object sender, DomainEventArgs args)
		{
			//UpdateDomainStatus(args.DomainID);
		}

		public void OnDomainInactivatedEvent(object sender, DomainEventArgs args)
		{
			//UpdateDomainStatus(args.DomainID);
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

		public void ToggelDomainState(DomainInformation domainInfo, bool login)
		{
			// disable the ability for the user to toggle the checkbox
			onlineToggleButton.Activatable = false;
			//TODO: Refere code from function OnlineToggled and add needed code	
			IDomainProviderUI provider = domainProviderUI.GetProviderForID(domainInfo.ID);
			if(provider != null)
			{
				if (domainInfo.Active)
					domainController.InactivateDomain(domainInfo.ID);
				else
					domainController.ActivateDomain(domainInfo.ID);
			}
			else
			{
				if(true == login)
				{
 					LoginDomain(domainInfo);
				}
				else
				{
 					LogoutDomain(domainInfo);
				}
			}
			iFolderWindow ifwin = Util.GetiFolderWindow();
		        ifwin.UpdateiFolderCount(domainInfo);	
		        UpdateDomainStatus(domainInfo.ID);

			// Reenable the ability for the user to toggle the checkbox
			onlineToggleButton.Activatable = true;

		}

	}
}
