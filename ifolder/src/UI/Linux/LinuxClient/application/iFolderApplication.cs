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
using System.Collections;
using System.Diagnostics;
using System.Threading;
using Gtk;
using Gdk;
using Gnome;
using GtkSharp;
using GLib;
using Egg;
//using DBus;
//using NetworkManager;
using Simias.Client.Event;
using Simias.Client;
using Simias.Client.Authentication;
using Novell.iFolder.Events;
using Novell.iFolder.Controller;
using System.Reflection;
using System.IO;

namespace Novell.iFolder
{
	public enum iFolderAppState : uint
	{
		Starting		= 0x0001,
		Stopping		= 0x0002,
		Running			= 0x0003,
		Stopped			= 0x0004
	}

//	[Interface("com.novell.iFolder")]
	public class iFolderApplication : Gnome.Program
	{
		private static iFolderApplication	application = null;

		private Gtk.Image			gAppIcon;
		private Gdk.Pixbuf			RunningPixbuf;
		private Gdk.Pixbuf			StartingPixbuf;
		private Gdk.Pixbuf			StoppingPixbuf;
//		private Gdk.Pixbuf			NwDisconnPixBuf;

		private Gdk.Pixbuf			DownloadingPixbuf;
		private Gdk.Pixbuf			UploadingPixbuf;
		private Gtk.EventBox		eBox;
		private Egg.TrayIcon			tIcon;
		private iFolderWebService	ifws;
		private SimiasWebService	simws;
		private iFolderData			ifdata;
		private TimerCallback 			timerDelegate;
		private Timer 				splashTimer;
		private iFolderAppState 		CurrentState;
		private Gtk.ThreadNotify	iFolderAppStateChanged;
		private SimiasEventBroker	simiasEventBroker;
		private	iFolderLoginDialog	LoginDialog;
//		private ListStore               accTreeStore;


		/// This variables is used to track the state of the iFolder
		/// Icon in the notification panel.  The animation should
		/// only occur when files/folders are being uploaded/downloaded.
		// 0 = Not animating, 1 = uploading, -1 = downloading
		private int					currentIconAnimationDirection;
		
		/// The following variables are used to keep track of the
		/// iFolder that is currently synchronizing and any errors
		/// encountered during a sync cycle so that the application
		/// is able to notify the user at the end when there's a
		/// problem.
		private string				collectionSynchronizing;
		private Hashtable			synchronizationErrors;

		private DomainController	domainController;
		private Simias.Client.Manager				simiasManager;

		private NotifyWindow		startingUpNotifyWindow;
		private NotifyWindow		shuttingDownNotifyWindow;
		
		private bool				forceShutdown;
		
		private iFolderMsgDialog	ClientUpgradeDialog;
//		private iFolderNetworkDialog iFoldernetworkdlg;

		private Gtk.Window startupWind;

                private iFolderMsgDialog  quitDlg;
                private bool quit_iFolder;
                private Status ClientUpgradeStatus;
		private string NewClientVersion;
		private string NewClientDomainID;
		
		///
		/// D-Bus variables
		///
//        	private static Service service;
//	        private static Connection connection;
//		public bool connect_flag;

        
        	///
	        /// NetworkManager variables
        	///
//	        private static NetworkDetect networkDetect;

		
		public SimiasEventBroker	EventBroker
		{
			get
			{
				return this.EventBroker;
			}
		}
		
		public Simias.Client.Manager				SimiasManager
		{
			get
			{
				return this.simiasManager;
			}
		}
		
		public iFolderAppState State
		{
			get
			{
				return this.CurrentState;
			}
		}

		public iFolderApplication(string[] args)
			: base("ifolder", "1.0", Modules.UI, args)
		{
			Util.InitCatalog();
			
			Util.SetQuitiFolderDelegate(new QuitiFolderDelegate(QuitiFolder));

			tIcon = new Egg.TrayIcon("iFolder");

			currentIconAnimationDirection = 0;
			this.ClientUpgradeStatus = null;
			this.NewClientVersion = null;
			this.NewClientDomainID = null;

			eBox = new EventBox();
			eBox.ButtonPressEvent +=
				new ButtonPressEventHandler(trayapp_clicked);

                        quitDlg  = new iFolderMsgDialog(
                        null,
                        iFolderMsgDialog.DialogType.Question,
                        iFolderMsgDialog.ButtonSet.YesNo,
                        Util.GS("Exit Novell iFolder"),"",
                         Util.GS("If you exit the Novell iFolder application, changes in your iFolder will no longer be tracked.\nThe next time you login, Novell iFolder will reconcile any differences between your iFolder and Server.\n\nAre you sure you want to exit the Application ?")
                        );
                         quitDlg.Response += new ResponseHandler(YesNo_Clicked);

			RunningPixbuf =
					new Pixbuf(Util.ImagesPath("ifolder16.png"));
			StartingPixbuf =
					new Pixbuf(Util.ImagesPath("ifolder-waiting16.png"));
			StoppingPixbuf =
					new Pixbuf(Util.ImagesPath("ifolder-waiting16.png"));
			DownloadingPixbuf =
					new Pixbuf(Util.ImagesPath("ifolder-download16.png"));
			UploadingPixbuf =
					new Pixbuf(Util.ImagesPath("ifolder-upload16.png"));
//			NwDisconnPixBuf = 
//	 			        new Pixbuf(Util.ImagesPath("ifolder_dis2_16.png"));

			gAppIcon = new Gtk.Image(RunningPixbuf);

			eBox.Add(gAppIcon);
			tIcon.Add(eBox);
			tIcon.ShowAll();	

			LoginDialog = null;

			collectionSynchronizing = null;
			synchronizationErrors = new Hashtable();

			iFolderAppStateChanged = new Gtk.ThreadNotify(
							new Gtk.ReadyEvent(OniFolderAppStateChanged));

			// Create the simias manager object and set any command line
			// configuration in the object.
			simiasManager = Util.CreateSimiasManager(args);

			//Register for network events generated by NetworkManager (via D-Bus)
//			networkDetect = NetworkDetect.Instance;
//			if (networkDetect != null)
//				networkDetect.StateChanged +=
//					new NetworkStateChangedHandler(OnNetworkStateChanged);
/*			if( networkDetect.isNull())
			{
				if ((bool)ClientConfig.Get(ClientConfig.KEY_SHOW_NETWORK_ERRORS))
				{
					iFoldernetworkdlg = new iFolderNetworkDialog();
                                        int createRC;
                                        do
                                        {
	                                        createRC = iFoldernetworkdlg.Run();
                                        }while(createRC != (int)Gtk.ResponseType.Ok);

                                        iFoldernetworkdlg.Hide();

                                        if(iFoldernetworkdlg.HideDialog)
                                        {
                                                ClientConfig.Set(
                                                ClientConfig.KEY_SHOW_NETWORK_ERRORS, false);
                                        }

				}
			}
*/
			startingUpNotifyWindow = null;
			shuttingDownNotifyWindow = null;
			
			forceShutdown = false;

			ClientUpgradeDialog = null;
		}


		static public iFolderApplication GetApplication()
		{
			return application;
		}


		public new void Run()
		{
			System.Threading.Thread startupThread =
					new System.Threading.Thread(new ThreadStart(StartiFolder));
			startupThread.Start();


			base.Run();
		}



		private void StartiFolder()
		{
			bool simiasRunning = false;
			
			CurrentState = iFolderAppState.Starting;
			iFolderAppStateChanged.WakeupMain();

			//if(ifws == null)
			{
				simiasManager.Start();

				string localServiceUrl = simiasManager.WebServiceUri.ToString();
				ifws = new iFolderWebService();
				ifws.Url = localServiceUrl + "/iFolder.asmx";
				LocalService.Start(ifws, simiasManager.WebServiceUri, simiasManager.DataPath);
				
				simws = new SimiasWebService();
				simws.Url = localServiceUrl + "/Simias.asmx";
				LocalService.Start(simws, simiasManager.WebServiceUri, simiasManager.DataPath);

				// wait for simias to start up
				while(!simiasRunning)
				{
					try
					{
						ifws.Ping();
						simiasRunning = true;
					}
					catch(Exception)
					{
						simiasRunning = false;
					}
					
					if (forceShutdown)
					{
						// the user is canceling startup
						QuitiFolder();
						return;
					}

					// Wait and ping it again
					System.Threading.Thread.Sleep(10);
				}
				//Initialize the logging infrastructure
		            	LogInit(simiasManager.DataPath);
				
				if (forceShutdown)
					QuitiFolder();
				else
				{
					try
					{
						simiasEventBroker = SimiasEventBroker.GetSimiasEventBroker();
						// Set up to have data ready for events
						ifdata = iFolderData.GetData();
						domainController = DomainController.GetDomainController();
					}
					catch(Exception e)
					{
						Debug.PrintLine(e.Message);
						ifws = null;
					}
				}
			}
			CleanUpPassphrase();
			CurrentState = iFolderAppState.Running;
			iFolderAppStateChanged.WakeupMain();
		}




		private void StopiFolder()
		{
			CurrentState = iFolderAppState.Stopping;
			iFolderAppStateChanged.WakeupMain();

			try
			{
				if(simiasEventBroker != null)
					simiasEventBroker.Deregister();
			}
			catch(Exception e)
			{
				// ignore
				Debug.PrintLine(e.Message);
			}

			CurrentState = iFolderAppState.Stopped;
			iFolderAppStateChanged.WakeupMain();
		}

		private void OnDomainNeedsCredentialsEvent(object sender, DomainEventArgs args)
		{
			ReLogin(args.DomainID);
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
		/// This method should only be called during first time login (or after exit/login)
		private void ShowClientUpgradeMessageBox()
		{
			if(this.NewClientVersion == null || this.ClientUpgradeStatus == null || this.NewClientDomainID == null)
			{
				return;
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

		private void ReLogin(string domainID)
		{
			if (LoginDialog != Util.CurrentModalWindow) return;

			if (LoginDialog == null)
			{
				DomainInformation dom = domainController.GetDomain(domainID);
				//Random Authentication exception found during Login
				//Due race condition, Login window prompted even if domain got connected with another therad
				//Verifing domain authentication status before prompting Login Dialog		
				if(dom.Authenticated)	
				{
					return;
				}
				
				if (dom != null)
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
						new ResponseHandler(OnReLoginDialogResponse);

					LoginDialog.ShowAll();
				}
			}
			else
			{
				LoginDialog.Present();
			}
		}

		private void OnReLoginDialogResponse(object o, ResponseArgs args)
		{
			switch (args.ResponseId)
			{
				case Gtk.ResponseType.Ok:
					DomainInformation dom = domainController.GetDomain(LoginDialog.Domain);
					if (dom == null)
					{
						iFolderMsgDialog dialog = new iFolderMsgDialog(
							null,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.None,
							Util.GS("Account Error"),
							Util.GS("This account has been removed from your computer."),
							Util.GS("If you wish to connect to this account again, please add it in the Account Settings Dialog."));
						dialog.Run();
						dialog.Hide();
						dialog.Destroy();
						dialog = null;
						
						LoginDialog.Hide();
						LoginDialog.Destroy();
						LoginDialog = null;
						break;
					}

					try
					{
						string DomainID = LoginDialog.Domain;
						Status status =
							domainController.AuthenticateDomain(
								LoginDialog.Domain,
								LoginDialog.Password,
								LoginDialog.ShouldSavePassword);
						if (status != null)
						{
							switch(status.statusCode)
							{
								case StatusCodes.Success:
								case StatusCodes.SuccessInGrace:
									ifdata.Refresh();
									// Login was successful so close the Login dialog
									Debug.PrintLine("Login dialog response- success");
									LoginDialog.Hide();
									LoginDialog.Destroy();
									LoginDialog = null;
                                                // Check if any recovery agent present;
                                                // if( domainController.GetRAList(DomainID) == null)
                                                // {
                                                        // No recovery agent present;
                                                 //       return;
                                                // }
						ShowClientUpgradeMessageBox();
                                                int result;
						int policy = ifws.GetSecurityPolicy(DomainID);
						if( policy % 2 == 0)
							break;
                                                bool passphraseStatus = simws.IsPassPhraseSet(DomainID);
						if(passphraseStatus == true)
				//		if( false)
						{
							bool rememberOption = simws.GetRememberOption(DomainID);
							if( rememberOption == false)
							{
								ShowVerifyDialog( DomainID, simws);
							}
							else 
							{
								Debug.PrintLine(" remember Option true. Checking for passphrase existence");
								string passphrasecheck;
								passphrasecheck= simws.GetPassPhrase(DomainID);
								if(passphrasecheck == null || passphrasecheck == "")
									ShowVerifyDialog( DomainID, simws);
							}
						}
						else
						{
							iFolderWindow.ShowEnterPassPhraseDialog(DomainID, simws);
						}
//                                              string[] array = domainController.GetRAList( DomainID);
									break;
								case StatusCodes.InvalidCertificate:
									if( status.UserName != null)
									{
										dom.Host = status.UserName;
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

									Gdk.Pixbuf certPixbuf = Util.LoadIcon("gnome-mime-application-x-x509-ca-cert", 48);
									if (certPixbuf != null && dialog.Image != null)
										dialog.Image.Pixbuf = certPixbuf;

									int rc = dialog.Run();
									dialog.Hide();
									dialog.Destroy();
									if(rc == -8) // User clicked the Yes button
									{
										simws.StoreCertificate(byteArray, dom.Host);
										OnReLoginDialogResponse(o, args);
									}
									else
									{
										// Prevent the auto login feature from being called again
										domainController.DisableDomainAutoLogin(LoginDialog.Domain);

										LoginDialog.Hide();
										LoginDialog.Destroy();
										LoginDialog = null;
									}
									break;
								case StatusCodes.UserAlreadyMoved:
									OnReLoginDialogResponse(o, args);
									break;
								default:
									Util.ShowLoginError(LoginDialog, status.statusCode);
									break;
							}
						}
					}
					catch(Exception e)
					{
						iFolderMsgDialog dialog = new iFolderMsgDialog(
							null,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.None,
							Util.GS("Account Error"),
							Util.GS("Unable to connect to the iFolder Server"),
							Util.GS("An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator."),
							e.Message);
						dialog.Run();
						dialog.Hide();
						dialog.Destroy();
						dialog = null;
					}

					break;
				case Gtk.ResponseType.Cancel:
				case Gtk.ResponseType.DeleteEvent:
					// Prevent the auto login feature from being called again
					domainController.DisableDomainAutoLogin(LoginDialog.Domain);

					LoginDialog.Hide();
					LoginDialog.Destroy();
					LoginDialog = null;
					break;
			}
		}

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
					break; //return true; //return status;
				if( result == (int)ResponseType.Ok)
				{
					passPhraseStatus =  simws.ValidatePassPhrase(DomainID, vpd.PassPhrase);
				}
				if( passPhraseStatus != null)
				{
					if( passPhraseStatus.statusCode == StatusCodes.PassPhraseInvalid)  // check for invalid passphrase
					{
						// Display an error Message
						Debug.PrintLine("Invalid Passphrase");
						Debug.PrintLine("Invalid Passphrase");
						iFolderMsgDialog dialog = new iFolderMsgDialog(
							null,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.None,
							Util.GS("Invalid PassPhrase"),
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
			}
			else if( passPhraseStatus != null && passPhraseStatus.statusCode == StatusCodes.Success)
			{
				try
				{
					simws.StorePassPhrase( DomainID, vpd.PassPhrase, CredentialType.Basic, vpd.ShouldSavePassPhrase);
					status = simws.IsPassPhraseSet(DomainID);
				}
				catch(Exception) {}
			}
			}
			catch(Exception)
			{
				return false;
			}
			return status;
		}


		private void OniFolderFileSyncEvent(object o, FileSyncEventArgs args)
		{
			if (args == null || args.CollectionID == null || args.Name == null)
				return;	// Prevent an exception

			try
			{
				// Animate the iFolder Icon only when we're actually uploading
				// or downloading a file/directory.
				if (args.ObjectType == ObjectType.File || args.ObjectType == ObjectType.Directory)
				{
					if (args.Direction == Simias.Client.Event.Direction.Uploading
						&& currentIconAnimationDirection != 1)
					{
						gAppIcon.Pixbuf = UploadingPixbuf;
						currentIconAnimationDirection = 1;
					}
					else if (args.Direction == Simias.Client.Event.Direction.Downloading
								&& currentIconAnimationDirection != -1)
					{
						gAppIcon.Pixbuf = DownloadingPixbuf;
						currentIconAnimationDirection = -1;
					}
				}
			}
			catch {}

			// Keep track of certain error conditions during a sync and notify
			// the user when there is a problem at the end of a sync cycle.
			if (args.Status != SyncStatus.Success)
			{
				string message = null;

				switch(args.Status)
				{
					case SyncStatus.Success:
						// Clear all synchronization errors (if any old ones
						// exist) since the this just synchronized successfully.
						if (synchronizationErrors.ContainsKey(args.CollectionID))
							synchronizationErrors.Remove(args.CollectionID);

						break;
					case SyncStatus.UpdateConflict:
					case SyncStatus.FileNameConflict:
						// Conflicts are handled in the OniFolderChangedEvent method
						break;
					case SyncStatus.Access:
						if( (bool)ClientConfig.Get(ClientConfig.KEY_SHOW_PERMISSION_UNAVAILABLE)  )
							message = Util.GS("Insuficient rights prevented complete synchronization.");
						break;
					case SyncStatus.Locked:
						message = Util.GS("The iFolder is locked.");
						break;
					case SyncStatus.PolicyQuota:
						 if( (bool)ClientConfig.Get(ClientConfig.KEY_SHOW_QUOTA_VIOLATION)  || (bool)ClientConfig.Get(ClientConfig.KEY_SHOW_QUOTA_VIOLATION))
						 {
							 message = Util.GS("The iFolder is full.  Click here to view the Synchronization Log.");
							 message = Util.GS("The iFolder is full.\n\nClick <a href=\"ShowSyncLog\">here</a> to view the Synchronization Log.");
						}
						break;
					case SyncStatus.PolicySize:
						if( (bool)ClientConfig.Get(ClientConfig.KEY_SHOW_FILE_SIZE_VOILATION)   )
							message = Util.GS("A size restriction policy prevented complete synchronization.");
						break;
					case SyncStatus.PolicyType:
						if( (bool)ClientConfig.Get(ClientConfig.KEY_SHOW_EXCLUSION_VOILATION))
							message = Util.GS("A file type restriction policy prevented complete synchronization.");
						break;
					case SyncStatus.DiskFull:
						if( (bool)ClientConfig.Get(ClientConfig.KEY_SHOW_DISK_FULL)  )
						{
							if (args.Direction == Simias.Client.Event.Direction.Uploading)
							{
								message = Util.GS("Insufficient disk space on the server prevented complete synchronization.");
							}
							else
							{
								message = Util.GS("Insufficient disk space on this computer prevented complete synchronization.");
							}
						}
						break;
					case SyncStatus.ReadOnly:
						if( (bool)ClientConfig.Get(ClientConfig.KEY_SHOW_PERMISSION_UNAVAILABLE)  )
						{
							message = Util.GS("You have Read-only access to this iFolder.  Files that you place in this iFolder will not be synchronized.\n\nClick <a href=\"ShowSyncLog\">here</a> to view the Synchronization Log.");
						}
						break;
				        case SyncStatus.IOError:
						if (args.Direction == Simias.Client.Event.Direction.Uploading)
						    message = Util.GS("Unable to read files from the folder. Verify the permissions on your local folder.");
						else
						    message = Util.GS("Unable to write files in the folder. Verify the permissions on your local folder.");
						break;
					case SyncStatus.PathTooLong:
						if( (bool)ClientConfig.Get(ClientConfig.KEY_SHOW_EXCEEDS_PATH_SIZE)  )
						{	
							if (args.Direction == Simias.Client.Event.Direction.Downloading)	
								message =  string.Format(Util.GS("Cannot download the file (0) because the file path exceeds the optimal limit."),args.Name);
						}
						break;
					default:
						message = Util.GS("iFolder synchronization failed.");
						break;
				}
				
				if (message != null)
				{
					Hashtable collectionSyncErrors = null;
					// Displays the file name also along with the reason for ...
				//	message = String.Format("Cannot synchronize: {0}. ", args.Name) + message;
					Debug.PrintLine(String.Format("Synchronization errors: {0}, {1}, {2}", args.Name, message, args.CollectionID));
					if (synchronizationErrors.ContainsKey(args.CollectionID))
					{
						collectionSyncErrors = (Hashtable)synchronizationErrors[args.CollectionID];
						Debug.PrintLine(String.Format("collection sync errors exist: {0}", collectionSyncErrors.Count));
					}
					else
					{
						collectionSyncErrors = new Hashtable();
						synchronizationErrors[args.CollectionID] = collectionSyncErrors;
					}
					
					if (!collectionSyncErrors.ContainsKey(args.Status))
					{
						collectionSyncErrors[args.Status] = message;
					// per iFolder log message
					//	if( iFolderHolderSyncing != null)
					//		iFolderHolderSyncing.iflog.LogMessage(message);
						Debug.PrintLine(String.Format("collection sync error count: {0}", collectionSyncErrors.Count));
					}
				}
			}
		}




		private void OniFolderSyncEvent(object o, CollectionSyncEventArgs args)
		{
			if (args == null || args.ID == null || args.Name == null)
				return;	// Prevent an exception

			switch(args.Action)
			{
				case Simias.Client.Event.Action.StartSync:
				{
					collectionSynchronizing = args.ID;
				//	iFolderHolderSyncing = ifdata.GetiFolder( collectionSynchronizing);
				// per iFolder synchronization log
				//	if( iFolderHolderSyncing != null)
				//		iFolderHolderSyncing.iflog = new iFolderLogWindow();
					break;
				}
				case Simias.Client.Event.Action.StopSync:
				{
					currentIconAnimationDirection = 0;
					//gAppIcon.Pixbuf = RunningPixbuf;

//					if((bool)ClientConfig.Get(ClientConfig.KEY_NOTIFY_SYNC_ERRORS))
//					{
						if (collectionSynchronizing != null)
						{
							iFolderHolder ifHolder = ifdata.GetiFolder(collectionSynchronizing);
							if (ifHolder != null)
							{
								if (synchronizationErrors.ContainsKey(ifHolder.iFolder.ID))
								{
									Hashtable collectionSyncErrors = (Hashtable)synchronizationErrors[ifHolder.iFolder.ID];
									ICollection errors = collectionSyncErrors.Keys;
									Debug.PrintLine(String.Format("Number of errors: {0}", errors.Count));
									ArrayList keysToClear = new ArrayList();
									bool showErrorBaloon = false;
									bool showGenericBaloon = true;
									foreach(SyncStatus syncStatusKey in errors)
									{
										string errMsg = (string) collectionSyncErrors[syncStatusKey];
										if (errMsg != null && errMsg.Length > 0)
										{
												showErrorBaloon = true;
										
										        	// Shows baloon for every file not synced..
												NotifyWindow notifyWin = new NotifyWindow(
												tIcon, string.Format(Util.GS("Incomplete Synchronization: {0}"), ifHolder.iFolder.Name),
												errMsg,
												Gtk.MessageType.Warning, 5000);
												notifyWin.LinkClicked +=
												new LinkClickedEventHandler(OnNotifyWindowLinkClicked);
												notifyWin.ShowAll();
												showGenericBaloon = false;
											
												// Set this message to "" so that
												// the notification bubble isn't
												// popped-up on every sync cycle.
												keysToClear.Add(syncStatusKey);
										}
									}
									if( showErrorBaloon == true)
									{
										if( showGenericBaloon )
										{
											NotifyWindow notifyWin = new NotifyWindow(
											tIcon, string.Format(Util.GS("Incomplete Synchronization: {0}"), ifHolder.iFolder.Name),
											Util.GS("Synchronization log contains the information regarding the files that are not synchronized")/*errMsg*/,
											Gtk.MessageType.Warning, 5000);
											notifyWin.LinkClicked +=
											new LinkClickedEventHandler(OnNotifyWindowLinkClicked);
											notifyWin.ShowAll();
										}
									}
									
									// Clear out all the keys whose messages
									// were just notified to the user.
									foreach(SyncStatus syncStatusKey in keysToClear)
									{
										collectionSyncErrors.Remove( syncStatusKey);
							//			collectionSyncErrors[syncStatusKey] = null;
									}
									Debug.PrintLine(String.Format("After removing keys count: {0}", collectionSyncErrors.Count));
								}
							}
						}
//					}

					collectionSynchronizing = null;

					break;
				}
			}
			gAppIcon.Pixbuf = RunningPixbuf;
		}


		private void OniFolderAddedEvent(object o, iFolderAddedEventArgs args)
		{
			if (args == null || args.iFolderID == null)
				return;	// Prevent an exception
		
			// don't notify us of our own iFolders
			iFolderHolder ifHolder = ifdata.GetiFolder(args.iFolderID);

			if (ifHolder == null)
				return;
			
			if (ifHolder.iFolder == null)
				return;

			if(!ifdata.IsCurrentUser(ifHolder.iFolder.OwnerID))
			{
				if(ifHolder.iFolder.IsSubscription &&
					((bool)ClientConfig.Get(ClientConfig.KEY_NOTIFY_IFOLDERS)))
				{
					NotifyWindow notifyWin = new NotifyWindow(
							tIcon,
							string.Format(Util.GS("New iFolder \"{0}\""),
								ifHolder.iFolder.Name),
							string.Format(Util.GS("{0} has invited you to participate in this shared iFolder.\n\nClick <a href=\"SetUpiFolder:{1}\">here</a> to set up this iFolder."),
										  ifHolder.iFolder.Owner, ifHolder.iFolder.CollectionID),
							Gtk.MessageType.Info, 10000);
					notifyWin.LinkClicked +=
						new LinkClickedEventHandler(OnNotifyWindowLinkClicked);
					notifyWin.ShowAll();
				}
			}

//			iFolderWindow ifwin = Util.GetiFolderWindow();
//			if(ifwin != null)
//				ifwin.iFolderCreated(args.iFolderID);
		}




		private void OniFolderChangedEvent(object o,
									iFolderChangedEventArgs args)
		{
			if (args == null || args.iFolderID == null)
				return;	// Prevent an exception
		
			// don't notify us of our own iFolders
			iFolderHolder ifHolder = ifdata.GetiFolder(args.iFolderID);

			// You can put the client into a state where we no longer have the iFolderHolder
			// around.  One way to do this is to create an iFolder, wait for it to start
			// synchronizing and while it's synchronizing, right-click, select "Delete",
			// and then answer, "Yes".  To prevent this from blowing up the client entirely
			// I'm adding in the following check for null.
			//
			// When the user deletes the iFolder, we remove it from the iFolderData
			// structure so when we receive this event, it would, naturally be null.
			if (ifHolder == null)
				return;

			// Also, just in case...
			if (ifHolder.iFolder == null)
				return;

			if(ifHolder.iFolder.IsSubscription)
			{
//				iFolderWindow ifwin = Util.GetiFolderWindow();
//				if(ifwin != null)
//					ifwin.iFolderChanged(args.iFolderID);
			}
			else
			{
				// this may be called when a subscription gets updated
				if(ifHolder.iFolder.HasConflicts)
				{
					if((bool)ClientConfig.Get(ClientConfig.KEY_NOTIFY_COLLISIONS))
					{
						string message = string.Format(
							Util.GS("A conflict has been detected in this iFolder.\n\nClick <a href=\"ResolveiFolderConflicts:{0}\">here</a> to resolve the conflicts.\nWhat is a <a href=\"ShowConflictHelp\">conflict</a>?"),
							args.iFolderID);

						Hashtable collectionSyncErrors = null;
						if (synchronizationErrors.ContainsKey(args.iFolderID))
							collectionSyncErrors = (Hashtable)synchronizationErrors[args.iFolderID];
						else
						{
							collectionSyncErrors = new Hashtable();
							synchronizationErrors[args.iFolderID] = collectionSyncErrors;
						}
						
						if (!collectionSyncErrors.ContainsKey(SyncStatus.FileNameConflict))
						{
							collectionSyncErrors[SyncStatus.FileNameConflict] = message;
						}
					}

//					iFolderWindow ifwin = Util.GetiFolderWindow();
//					if(ifwin != null)
//						ifwin.iFolderHasConflicts(args.iFolderID);
				}
			}
		}




		private void OniFolderDeletedEvent(object o,
									iFolderDeletedEventArgs args)
		{
			if (args == null || args.iFolderID == null)
				return;	// Prevent an exception
			
//			iFolderWindow ifwin = Util.GetiFolderWindow();
//			if(ifwin != null)
//				ifwin.iFolderDeleted(args.iFolderID);
		}


		private void OniFolderUserAddedEvent(object o,
									iFolderUserAddedEventArgs args)
		{
			if (args == null || args.iFolderID == null || args.iFolderUser == null)
				return;	// Prevent an exception
			
			if((bool)ClientConfig.Get(ClientConfig.KEY_NOTIFY_USERS))
			{
				string username;
				iFolderHolder ifHolder = ifdata.GetiFolder(args.iFolderID);

				if( (args.iFolderUser.FN != null) &&
					(args.iFolderUser.FN.Length > 0) )
					username = args.iFolderUser.FN;
				else
					username = args.iFolderUser.Name;

				NotifyWindow notifyWin = new NotifyWindow(
					tIcon, Util.GS("New iFolder User"),
					string.Format(Util.GS("{0} has joined the iFolder \"{1}\""), username, ifHolder.iFolder.Name),
					Gtk.MessageType.Info, 10000);
				notifyWin.LinkClicked +=
					new LinkClickedEventHandler(OnNotifyWindowLinkClicked);
				notifyWin.ShowAll();
			}
							
			// TODO: update any open windows?
//			if(ifwin != null)
//			ifwin.NewiFolderUser(ifolder, newuser);
		}




		// ThreadNotify Method that will react to a fired event
		private void OniFolderAppStateChanged()
		{
			/*The following lines of code commented. Their functionality has been implemented into a sepearte function splashTimeEevnt */
			/*if(CurrentState != iFolderAppState.Starting)
			{
				if( this.startupWind != null)
				{
					startupWind.Destroy();
					startupWind = null;
				}
			}*/

			// If the state is starting show the splash screen...
			switch(CurrentState)
			{
				case iFolderAppState.Starting:
					gAppIcon.Pixbuf = StartingPixbuf;
					if(!(bool)ClientConfig.Get(ClientConfig.KEY_IFOLDER_WINDOW_HIDE))
						ShowStartupScreen();
					break;

				case iFolderAppState.Running:
					if(simiasEventBroker != null)
					{
						simiasEventBroker.iFolderAdded +=
							new iFolderAddedEventHandler(
												OniFolderAddedEvent);
						simiasEventBroker.iFolderChanged +=
							new iFolderChangedEventHandler(
												OniFolderChangedEvent);
						simiasEventBroker.iFolderDeleted +=
							new iFolderDeletedEventHandler(
												OniFolderDeletedEvent);
						simiasEventBroker.iFolderUserAdded +=
							new iFolderUserAddedEventHandler(
												OniFolderUserAddedEvent);
						simiasEventBroker.CollectionSyncEventFired +=
							new CollectionSyncEventHandler(
												OniFolderSyncEvent);
						simiasEventBroker.FileSyncEventFired +=
							new FileSyncEventHandler(
												OniFolderFileSyncEvent);
					}
					
					if (domainController != null)
					{
						domainController.DomainNeedsCredentials +=
							new DomainNeedsCredentialsEventHandler(OnDomainNeedsCredentialsEvent);
						domainController.DomainClientUpgradeAvailable +=
							new DomainClientUpgradeAvailableEventHandler(OnClientUpgradeAvailableEvent);
					}

					if (startingUpNotifyWindow != null)
					{
						NotifyWindow notifyWin = startingUpNotifyWindow;
						notifyWin.Hide();
						notifyWin.Destroy();
					}

/*					if (networkDetect.Connected)
					{
	 					gAppIcon.Pixbuf = RunningPixbuf;
					}
					else
					{					
						gAppIcon.Pixbuf = NwDisconnPixBuf;
					}
*/

					gAppIcon.Pixbuf = RunningPixbuf;

					// Make sure the iFolder and Synchronization Windows are
					// initialized so that they begin receiving events as soon
					// as possible.

					Util.GetiFolderWindow();
	                                Util.GetLogWindow(simiasManager);

					// Bring up the accounts dialog if there are no domains
//					GLib.Timeout.Add(500, new GLib.TimeoutHandler(PromptIfNoDomains));
					
					// Load the iFolder Windows in the state they were in last
					GLib.Timeout.Add(100, new GLib.TimeoutHandler(ShowiFolderWindows));

					break;

				case iFolderAppState.Stopping:
					gAppIcon.Pixbuf = StoppingPixbuf;
					break;

				case iFolderAppState.Stopped:
					// Start up a thread that will guarantee we completely
					// exit after 10 seconds.
					//ThreadPool.QueueUserWorkItem(new WaitCallback(GuaranteeShutdown));
					System.Threading.Thread th = new System.Threading.Thread (new System.Threading.ThreadStart (GuaranteeShutdown));
					th.IsBackground = true;
					th.Start ();

					try
					{
						/*bool stopped = */simiasManager.Stop();
					}
					catch(Exception e)
					{
						// ignore
						Debug.PrintLine(e.Message);
					}

					Application.Quit();
					break;
			}
		}

		private void ShowStartupScreen()
		{
			
			
			timerDelegate = new TimerCallback(splashTimerTick);
			/*Close the splash screen after 3 secs and never call this event again*/
			splashTimer = new Timer(timerDelegate,null,3000,System.Threading.Timeout.Infinite);
			startupWind = new Gtk.Window(Gtk.WindowType.Popup);
			Gtk.Image image = new Gtk.Image(new Gdk.Pixbuf(Util.ImagesPath("ifolder-startup-nl.png")));
			startupWind.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));
			startupWind.WindowPosition = Gtk.WindowPosition.Center;
			//startupWind.set_keep_above(false); requires higher version of GTK
			VBox vbox = new VBox (false, 0);
			startupWind.Add (vbox);
			vbox.PackStart(image, false, false, 0);
			startupWind.ShowAll();
		}
		private void splashTimerTick(object sender)
        	{
			/*one time entry*/
        	        if (this.startupWind != null)
            		{	
				this.startupWind.Destroy();
                		this.startupWind = null;
            		}                   
            		splashTimer.Dispose();
        	}
		static private void GuaranteeShutdown()
		{
			// Sleep for 10 seconds and if the process is still
			// running, we'll kill it.  If the process stops appropriately
			// before this is finished sleeping, this thread will terminate
			// before forcing a shutdown anyway.
			System.Threading.Thread.Sleep(10000);
			System.Environment.Exit(1);
		}

		private bool ShowiFolderWindows()
		{
			if (domainController == null)
				domainController = DomainController.GetDomainController();
			
			if (domainController != null)
			{
				DomainInformation[] domains = domainController.GetDomains();
				if (domains.Length < 1)
				{
					// Launch the Add Account Wizard
					ShowAddAccountWizard();
				}
				else
				{
					if(! (bool)ClientConfig.Get(ClientConfig.KEY_IFOLDER_WINDOW_HIDE)) 
						Util.LoadiFolderWindows();

					Debug.PrintLine("Showing the migration prompt");
					if( Util.ShowMigrationPrompt() )
					{
						//  Prompt for migration...
						string str = Mono.Unix.UnixEnvironment.EffectiveUser.HomeDirectory;
						if(System.IO.Directory.Exists(str+"/.novell/ifolder"))
						{
							string[] dirs;
							dirs = System.IO.Directory.GetDirectories(str+"/.novell/ifolder");
							if( dirs.Length > 2)
							{
								iFolderMsgDialog dlg = new iFolderMsgDialog( null, iFolderMsgDialog.DialogType.Info, iFolderMsgDialog.ButtonSet.OkCancel, 
												Util.GS("Migration Alert"), Util.GS("There are 2.x iFolders on this machine.") , Util.GS("Do you want to migrate them?") );
								CheckButton dontShowAgain = new CheckButton(Util.GS("Don't show this message again"));
								dlg.ExtraWidget = dontShowAgain;
								int res = dlg.Run();
								dlg.Hide();
								dlg.Destroy();
								if( ((CheckButton)(dlg.ExtraWidget)).Active == true)
								{
									Debug.PrintLine("The check box is checked");
									Util.DontShowMigrationPrompt();
								}
								else
										Debug.PrintLine("The check box is not checked");
								if( res == (int)ResponseType.Ok)
								{
									// Call Migration window.....
									MigrationWindow migrationWindow = new MigrationWindow( Util.GetiFolderWindow(), ifws, simws);
									migrationWindow.ShowAll();
								}
							}
						}
					}
				}
			}
			else
				Debug.PrintLine("DomainController instance is null");

			return false;	// Prevent this from being called over and over by GLib.Timeout
		}
	    
        private void ShowAddAccountWizard()
        {
            bool status = false;
            const string assemblyName = "plugins/Novell.AutoAccount.AutoAccountCreator";
            const string autoAccountClass = "Novell.AutoAccount.AutoAccount";
            const string autoAccountCreateAccountsMethod = "CreateAccounts";
            const string autoAccountPrefMethod = "SetPreferences";
            const string autoAccountFilePath = "AutoAccountFilePath";


            string filePathValue;
            System.Object[] args = new System.Object[1];
            System.Object[] prefArgs = new System.Object[1];
            
            try
            {
                Assembly idAssembly = Assembly.Load( assemblyName );
                if ( idAssembly != null )
                {
                    Type type = idAssembly.GetType( autoAccountClass );
                    if( null != type )
                    {
                        args[0] = simws;
                        System.Object autoAccount = Activator.CreateInstance(type,args);
                            
                        MethodInfo method = type.GetMethod(autoAccountCreateAccountsMethod);
                        status = (Boolean)method.Invoke(autoAccount, null);
                        iFolderWindow.log.Info("Account Creation Status {0}", status);
                        if( status )
                        {
                            method = type.GetMethod( autoAccountPrefMethod );
                            prefArgs[0] = ifws;
                            method.Invoke(autoAccount, prefArgs);
                            PropertyInfo info  = type.GetProperty(autoAccountFilePath);
                            filePathValue = (string)info.GetValue( autoAccount, null );
                            iFolderWindow.log.Debug("File path value is {0}", filePathValue );
                            System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePathValue);
			    if( File.Exists(filePathValue+".backup"))
			    {
				File.Delete(filePathValue+".backup");
			    }
                            fileInfo.MoveTo(filePathValue+".backup");
                        }
                    }
                }
            }
            catch( Exception e )
            {
                iFolderWindow.log.Info("Error: {0}", e.Message);
                iFolderWindow.log.Debug("Exception type {0}\nStackTrace {1}", e.GetType(), e.StackTrace);
            }

            DomainInformation[] domains = null;
            if(null != domainController)
                domains = domainController.GetDomains();
            if(!status) 
            {
                if(null != domains && domains.Length < 1)
                {
                    iFolderWindow.log.Debug("Starting account wizard from client...");
                    AddAccountWizard aaw = new AddAccountWizard(simws);
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
                else
                {
                    iFolderWindow.log.Debug("Domain count is greater than or equal to 1 now...");
                }
            }
            else
            {
                if(null != domains && domains.Length >= 1)
                	Util.ShowiFolderWindow();
            }
        }
		
		private void OnNotifyWindowLinkClicked(object sender, LinkClickedEventArgs args)
		{
			if (args.LinkID != null)
			{
				if (args.LinkID.Equals("ShowSyncLog"))
					Util.ShowLogWindow(simiasManager);
				else if (args.LinkID.StartsWith("SetUpiFolder"))
				{
					int colonPos = args.LinkID.IndexOf(':');
					if (colonPos > 0)
					{
						string ifolderID = args.LinkID.Substring(colonPos + 1);
						iFolderWindow ifwin = Util.GetiFolderWindow();
						ifwin.DownloadiFolder(ifolderID);
					}
				}
				else if (args.LinkID.StartsWith("ResolveiFolderConflicts"))
				{
					int colonPos = args.LinkID.IndexOf(':');
					if (colonPos > 0)
					{
						string ifolderID = args.LinkID.Substring(colonPos + 1);
						iFolderWindow ifwin = Util.GetiFolderWindow();
						ifwin.ResolveConflicts(ifolderID);
					}
				}
				else if (args.LinkID.Equals("ShowAccountsPage"))
				{
					showPrefsPage(1);
				}
				else if (args.LinkID.Equals("ShowConflictHelp"))
				{
					Util.ShowHelp("conflicts.html", null);
				}
				else if (args.LinkID.Equals("CancelStartup"))
				{
					ForceShutdown();
				}
			}

			NotifyWindow notifyWindow = sender as NotifyWindow;
			notifyWindow.Hide();
			notifyWindow.Destroy();
		}
		
		private void OnStartingUpNotifyWindowHidden(object o, EventArgs args)
		{
			startingUpNotifyWindow = null;
		}

		private void OnShuttingDownNotifyWindowHidden(object o, EventArgs args)
		{
			shuttingDownNotifyWindow = null;
		}

                private void YesNo_Clicked(object o, ResponseArgs args)
                {
                        if (args.ResponseId == Gtk.ResponseType.Yes)
                        {
                             	quit_iFolder = true;
				quitDlg.Hide();
                        }
                        else
                        {
                              	quit_iFolder = false;
                              	quitDlg.Hide();
 
                        }
                }

		private void trayapp_clicked(object obj, ButtonPressEventArgs args)
		{
			// Prevent the trayapp context menu from showing if we're
			// starting up or shutting down.
			if (CurrentState == iFolderAppState.Starting)
			{
				if (startingUpNotifyWindow == null)
				{
					startingUpNotifyWindow = new NotifyWindow(
						tIcon, Util.GS("iFolder is starting"),
						Util.GS("Please wait for iFolder to start...\n\nPress <a href=\"CancelStartup\">here</a> to cancel."),
						Gtk.MessageType.Info, 0);
					startingUpNotifyWindow.LinkClicked +=
						new LinkClickedEventHandler(OnNotifyWindowLinkClicked);
					startingUpNotifyWindow.Hidden +=
						new EventHandler(OnStartingUpNotifyWindowHidden);
					startingUpNotifyWindow.ShowAll();
				}

				return;
			}
			else if (CurrentState == iFolderAppState.Stopping)
			{
				if (shuttingDownNotifyWindow == null)
				{
					shuttingDownNotifyWindow = new NotifyWindow(
						tIcon, Util.GS("iFolder is shutting down"),
						"",
						Gtk.MessageType.Info, 0);
					shuttingDownNotifyWindow.Hidden +=
						new EventHandler(OnShuttingDownNotifyWindowHidden);
					shuttingDownNotifyWindow.ShowAll();
				}

				return;
			}
			
			// If a modal window is active in the client, don't allow anything
			// else to be shown.  When the user clicks on the tray app icon,
			// just force the modal window to be presented to the user.
			if (Util.CurrentModalWindow != null)
			{
				// Put a try/catch around this just in case the window was
				// suddenly closed and we end up with a object reference
				// exception.
				try
				{
					Util.CurrentModalWindow.Present();
					return;
				}
				catch{}
			}

			switch(args.Event.Button)
			{
				case 1: // first mouse button
					// If there are no accounts set up, launch the account
					// wizard.
					DomainInformation[] domains = domainController.GetDomains();
					if (domains.Length < 1)
					{
						// Launch the Add Account Wizard
						ShowAddAccountWizard();
					}
					else
					{
						// When the user clicks on the iFolder icon, show the
						// iFolder window if it's not already showing and hide
						// it if it's not showing.  This behavior is used by
						// Beagle and Gaim.
						iFolderWindow ifwin = Util.GetiFolderWindow();
						if (ifwin == null || !ifwin.IsActive)
							Util.ShowiFolderWindow();
						else
							ifwin.CloseWindow();
					}
					
					break;
				case 2: // second mouse button
					break;
				case 3: // third mouse button
					show_tray_menu();
					break;
			}
		}




		private void show_tray_menu()
		{
//			AccelGroup agrp = new AccelGroup();
			Menu trayMenu = new Menu();


			MenuItem iFolders_item =
					new MenuItem (Util.GS("iFolders"));
			trayMenu.Append (iFolders_item);
			DomainInformation[] domains = domainController.GetDomains();
			if (domains.Length < 1)
				iFolders_item.Sensitive = false;
			iFolders_item.Activated +=
					new EventHandler(showiFolderWindow);


			MenuItem accounts_item =
					new MenuItem (Util.GS("Account Settings"));
			trayMenu.Append (accounts_item);
			accounts_item.Activated +=
					new EventHandler(show_accounts);


			MenuItem logview_item =
					new MenuItem (Util.GS("Synchronization Log"));
			trayMenu.Append (logview_item);
			if (domains.Length < 1)
				logview_item.Sensitive = false;
			logview_item.Activated +=
					new EventHandler(showLogWindow);


			ImageMenuItem prefs_item = new ImageMenuItem (
											Util.GS("Preferences"));
			prefs_item.Image = new Gtk.Image(Gtk.Stock.Preferences,
											Gtk.IconSize.Menu);
			trayMenu.Append(prefs_item);
			prefs_item.Activated +=
					new EventHandler(show_preferences);

			
			trayMenu.Append(new SeparatorMenuItem());


			ImageMenuItem help_item = new ImageMenuItem (
											Util.GS("Help"));
			help_item.Image = new Gtk.Image(Gtk.Stock.Help,
											Gtk.IconSize.Menu);
			trayMenu.Append(help_item);
			help_item.Activated +=
					new EventHandler(show_help);


			ImageMenuItem about_item = new ImageMenuItem (
											Util.GS("About"));
			about_item.Image = new Gtk.Image(Gnome.Stock.About,
											Gtk.IconSize.Menu);
			trayMenu.Append(about_item);
			about_item.Activated +=
					new EventHandler(show_about);

			if((bool)ClientConfig.Get(ClientConfig.KEY_IFOLDER_DEBUG_IFOLDER_DATA))
			{
				trayMenu.Append(new SeparatorMenuItem());
				MenuItem ifolderDataDebug =
						new MenuItem ("Print iFolderData Debug Information");
				trayMenu.Append (ifolderDataDebug);
				ifolderDataDebug.Activated +=
						new EventHandler(PrintiFolderDataDebugState);
			}

/*			if( (ifSettings != null) && (!ifSettings.HaveEnterprise) )
			{
				MenuItem connect_item =
						new MenuItem (Util.GS("Join Enterprise Server"));
				trayMenu.Append (connect_item);
				connect_item.Activated += new EventHandler(OnJoinEnterprise);
			}

			if(!ShowReLoginWindow)
			{
				MenuItem show_relogin =
						new MenuItem(Util.GS("Login to iFolder Server"));
				trayMenu.Append(show_relogin);
				show_relogin.Activated += new EventHandler(OnShowReLogin);
			}
*/

			trayMenu.Append(new SeparatorMenuItem());

			ImageMenuItem quit_item = new ImageMenuItem (
											Util.GS("Quit"));
			quit_item.Image = new Gtk.Image(Gtk.Stock.Quit,
											Gtk.IconSize.Menu);
			trayMenu.Append(quit_item);
			quit_item.Activated +=
					new EventHandler(quit_ifolder);


			trayMenu.ShowAll();

			trayMenu.Popup(null, null, null, IntPtr.Zero, 3,
					Gtk.Global.CurrentEventTime);
//			trayMenu.Popup(null, null, null, IntPtr.Zero, 3,
//					Gtk.Global.CurrentEventTime);
		}
		
//		[Method]
		public virtual void ShowWindow()
		{
			// If a modal window is active in the client, don't allow anything
			// else to be shown.  When the user clicks on the tray app icon,
			// just force the modal window to be presented to the user.
			if (Util.CurrentModalWindow != null)
			{
				// Put a try/catch around this just in case the window was
				// suddenly closed and we end up with a object reference
				// exception.
				try
				{
					Util.CurrentModalWindow.Present();
					return;
				}
				catch{}
			}

			// If there are no accounts set up, launch the account
			// wizard.
			DomainInformation[] domains = domainController.GetDomains();
			if (domains.Length < 1)
			{
				// Launch the Add Account Wizard
				ShowAddAccountWizard();
			}
			else
			{
				// When the user clicks on the iFolder icon, show the
				// iFolder window if it's not already showing and hide
				// it if it's not showing.  This behavior is used by
				// Beagle and Gaim.
				iFolderWindow ifwin = Util.GetiFolderWindow();
				if (ifwin == null || !ifwin.IsActive)
					Util.ShowiFolderWindow();
				else
					ifwin.Present();
			}
		}

		private void PrintiFolderDataDebugState(object o, EventArgs args)
		{
			ifdata.PrintDebugState();
		}

		public void QuitiFolder()
		{
			quit_ifolder(null, null);
		}

		private void CleanUpPassphrase()
		{
			Debug.PrintLine("In cleanup passphrase");
			try
			{
			DomainInformation[] domains = domainController.GetDomains();
			foreach( DomainInformation domain in domains)
			{
				Debug.PrintLine("Removing Passphrase");
				bool rememberOption = simws.GetRememberOption(domain.ID);
				if( rememberOption == false)
				{
					simws.StorePassPhrase(domain.ID, "", CredentialType.None, false);
				}
			}
			}
			catch(Exception)
			{
			}
			
		}


		private void quit_ifolder(object o, EventArgs args)
		{

			
                        quitDlg.Run();
	                if (quit_iFolder)
        	        {

				CleanUpPassphrase();
				if (simiasEventBroker != null)
					simiasEventBroker.AbortEventProcessing();

				if (!forceShutdown)
				{
					Util.SaveiFolderWindows();
					Util.CloseiFolderWindows();
				}
	
				if(CurrentState == iFolderAppState.Stopping)
				{
					try
					{
						/*bool stopped =*/ simiasManager.Stop();
					}
					catch(Exception e)
					{
						Debug.PrintLine(e.Message);
					}

					System.Environment.Exit(1);
				}
				else
				{
					System.Threading.Thread stopThread =
					new System.Threading.Thread(new ThreadStart(StopiFolder));
					stopThread.Start();
				}
			}
		}
		
		private void ForceShutdown()
		{
			forceShutdown = true;
		}




		private void show_help(object o, EventArgs args)
		{
			Util.ShowHelp(Util.HelpMainPage, null);
		}



		private void show_about(object o, EventArgs args)
		{
			Util.ShowAbout();
		}



		private void show_preferences(object o, EventArgs args)
		{
			showPrefsPage(0);
		}




		private void show_accounts(object o, EventArgs args)
		{
			showPrefsPage(1);
		}

		private void showPrefsPage(int page)
		{
			Util.ShowPrefsPage(page, simiasManager);
		}

		private void showiFolderWindow(object o, EventArgs args)
		{
			Util.ShowiFolderWindow();
		}

		private void showLogWindow(object o, EventArgs args)
		{
			Util.ShowLogWindow(simiasManager);
		}
		
/*		private void OnNetworkStateChanged(object o, NetworkStateChangedArgs args)
		{
			Debug.PrintLine("OnNetworkStateChanged: ");
			Debug.PrintLine(args.Connected.ToString());

	
			if (!args.Connected) 
			{			
			   gAppIcon.Pixbuf = NwDisconnPixBuf;
			    
                        }
			else
			{
			  gAppIcon.Pixbuf = RunningPixbuf;
			}  			  			  
	
			
			
		
		        iFolderWindow ifwin = Util.GetiFolderWindow();
			//ifwin.UpdateSensitivity();
			ifwin.UpdateNetworkSensitivity(args.Connected);
		
			if (args.Connected)
			{
				ifwin.ShowAvailableiFolders();
				ifwin.GetInstance_cmi().Sensitive = true;
				ifwin.GetInstance_button().Sensitive = true;
			}
			else 
			{
				ifwin.HideAvailableiFolders();
				ifwin.GetInstance_cmi().Sensitive = false;
				ifwin.GetInstance_button().Sensitive = false;
			}
	
				
			iFolderViewGroup ifvg = iFolderWindow.GetiFolderViewGroupInstance();
			iFolderViewItem[] viewItems = ifvg.Items;
			
			foreach(iFolderViewItem item in viewItems)
			{
				iFolderHolder holder = item.Holder;
				if (!args.Connected)
					holder.State = iFolderState.NetworkCableDisc;
				else
					holder.State = iFolderState.Normal;	
			}

			// Update the item on the main thread
			//GLib.Idle.Add(ifwin.UpdateLocalViewItemsMainThread);

			
		}
		
		
		public static iFolderApplication FindInstance()
		{
			Connection connection = Bus.GetSessionBus();
			Service service = Service.Get(connection, "com.novell.iFolder");
			return service.GetObject(
						typeof(iFolderApplication),
						"/com/novell/iFolder/Application")
							as iFolderApplication;
		}
		
		private static void RegisterWithDBus(iFolderApplication theApp)
		{
			if (theApp == null)
			{
				Debug.PrintLine("RegisterWithDBus() called with a null application.");
				return;
			}

			try
			{
				connection = Bus.GetSessionBus();
				if (connection == null)
				{
					Debug.PrintLine("Could not get a connection to the D-Bus session");
					return;
				}
				
				service = new Service(connection, "com.novell.iFolder");
				
				if (service == null)
				{
					Debug.PrintLine("Could not create a D-Bus service instance.");
					return;
				}
				
				service.RegisterObject(theApp, "/com/novell/iFolder/Application");
			}
			catch(Exception e)
			{
				//Debug.PrintLine(e);
				//Debug.PrintLine("Could not connect to D-Bus.  D-Bus support will be disabled for this instance: " + e.Message);
			}
		}
		
		private static void UnregisterWithDBus()
		{
			try
			{
				if (application != null && service != null)
					service.UnregisterObject(application);
			}
			catch{}
		}
*/
        public static void LogInit(string path)
        {
			iFolderLogManager.LogConfDirPath = path;	
            if (!Directory.Exists(iFolderLogManager.LogConfDirPath))
            {
                Directory.CreateDirectory(iFolderLogManager.LogConfDirPath);
            }
			if(!File.Exists(iFolderLogManager.LogConfFilePath))
	            File.Copy(Path.Combine(SimiasSetup.sysconfdir, iFolderLogManager.LogConfFileName), iFolderLogManager.LogConfFilePath);
            iFolderLogManager.Configure(iFolderLogManager.LogConfDirPath);
            iFolderWindow.log = iFolderLogManager.GetLogger(typeof(System.Object));
        }

		public static void Main (string[] args)
		{
			try
			{
				System.Diagnostics.Process[] processes =
					System.Diagnostics.Process.GetProcessesByName("iFolderClient");

				if(processes.Length > 1)
				{
					// Send a message to the first instance to show its main window
//					Connection connection = Bus.GetSessionBus();
//					Service service = Service.Get(connection, "com.novell.iFolder");
//					iFolderApplication initialApp = service.GetObject(typeof(iFolderApplication), "/opt/novell/iFolder/Application") as iFolderApplication
					Debug.PrintLine("iFolder is already running!");
					return;
				}
			}
			catch (Exception)
			{
				Console.WriteLine("\n GetProcessesByName failed to fetch the list of processes");
			}

			///
			/// Attempt to use D-Bus to prevent the user from running multiple
			/// copies of iFolder.  Instead, just present the main iFolder
			/// window to them.
			///

/*			try
			{
				iFolderApplication existingApp = iFolderApplication.FindInstance();
				if (existingApp != null)
				{
					try
					{
						existingApp.ShowWindow();
					}
					catch(DBus.DBusException)
					{
						return;
					}
					return;
				}
			}
			catch(Exception e)
			{
				//Debug.PrintLine(e);
				//Process[] processes =
					System.Diagnostics.Process.GetProcessesByName("iFolderClient");
				if (processes.Length > 1)
				{
					Debug.PrintLine("iFolder is already running.  If you were trying " +
									  "to control the already running instance of iFolder, D-Bus must be enabled.  " +
									  "iFolder could not connect to your D-Bus Session Bus.");
					return;
				}
			}
*/
			try
			{
				application = new iFolderApplication(args);
//				RegisterWithDBus(application);

				application.Run();

//				UnregisterWithDBus();
			}
			catch(Exception bigException)
			{
				Debug.PrintLine(bigException.Message);
				iFolderCrashDialog cd = new iFolderCrashDialog(bigException);
				if (!Util.RegisterModalWindow(cd))
				{
					// Since we're exiting, destroy the existing modal window
					try
					{
						Util.CurrentModalWindow.Destroy();
						Util.RegisterModalWindow(cd);
					}
					catch{}
				}
				cd.Run();
				cd.Destroy();
				cd = null;

				// FIXME: For some reason, the simias process will runaway if
				// the a call to Deregister the event broker is made here.
				// Stop Simias (dereference iFolder's handle at least)
//				if(application.EventBroker != null)
//					application.EventBroker.Deregister();

				/*bool stopped =*/ application.SimiasManager.Stop();

//				UnregisterWithDBus();
				Application.Quit();
			}
		}
	}
}
