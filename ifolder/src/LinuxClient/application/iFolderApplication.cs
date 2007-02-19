/***********************************************************************
 *  $RCSfile: iFolderApplication.cs,v $
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
		private Gdk.Pixbuf			DownloadingPixbuf;
		private Gdk.Pixbuf			UploadingPixbuf;
		private Gtk.EventBox		eBox;
		private Egg.TrayIcon			tIcon;
		private iFolderWebService	ifws;
		private SimiasWebService	simws;
		private iFolderData			ifdata;

		private iFolderAppState 		CurrentState;
		private Gtk.ThreadNotify	iFolderAppStateChanged;
		private SimiasEventBroker	simiasEventBroker;
		private	iFolderLoginDialog	LoginDialog;

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
		
		///
		/// D-Bus variables
		///
//        private static Service service;
//        private static Connection connection;
        
        ///
        /// NetworkManager variables
        ///
//        private static NetworkDetect networkDetect;
		
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

			eBox = new EventBox();
			eBox.ButtonPressEvent +=
				new ButtonPressEventHandler(trayapp_clicked);

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

			// Register for network events generated by NetworkManager (via D-Bus)
//			networkDetect = NetworkDetect.Instance;
//			if (networkDetect != null)
//				networkDetect.StateChanged +=
//					new NetworkStateChangedHandler(OnNetworkStateChanged);

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

			if(ifws == null)
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
					catch(Exception e)
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
						Console.WriteLine(e);
						ifws = null;
					}
				}
			}

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
				Console.WriteLine(e);
			}

			CurrentState = iFolderAppState.Stopped;
			iFolderAppStateChanged.WakeupMain();
		}

		private void OnDomainNeedsCredentialsEvent(object sender, DomainEventArgs args)
		{
			ReLogin(args.DomainID);
		}
		
		private void OnClientUpgradeAvailableEvent(object sender, DomainClientUpgradeAvailableEventArgs args)
		{
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
				string.Format(Util.GS("A newer version \"{0}\" of the iFolder Client is available."), args.NewClientVersion));
			}
			int rc = ClientUpgradeDialog.Run();
			ClientUpgradeDialog.Hide();
			ClientUpgradeDialog.Destroy();
			if (rc == -8)
			{
				bool bUpdateRunning = false;
				Gtk.Window win = new Gtk.Window("");
				string initialPath = (string)System.IO.Path.GetTempPath();
				Console.WriteLine("Initial Path: {0}", initialPath);
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
					cp.Destroy();
				win.Hide();
				win.Destroy();
				if( rc1 != (int) ResponseType.Ok)
				{
					ClientUpgradeDialog = null;
					return;
				}
				try
				{
					bUpdateRunning = ifws.RunClientUpdate(args.DomainID, selectedFolder);
				}
				catch(Exception e)
				{
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
					if( domainController.GetDomain(args.DomainID) != null)
						domainController.RemoveDomain(args.DomainID, false);
				}

			}
			else //if(rc == -9)
			{
				if(DomainController.upgradeStatus.statusCode == StatusCodes.ServerOld || DomainController.upgradeStatus.statusCode == StatusCodes.UpgradeNeeded )
				{
					// Deny login
					if( domainController.GetDomain(args.DomainID) != null)
						domainController.RemoveDomain(args.DomainID, false);
				}
			}

			ClientUpgradeDialog = null;
		}

		private void ReLogin(string domainID)
		{
			if (LoginDialog != Util.CurrentModalWindow) return;

			if (LoginDialog == null)
			{
				DomainInformation dom = domainController.GetDomain(domainID);
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
				Console.WriteLine("Login dialog response");
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
									// Login was successful so close the Login dialog
									Console.WriteLine("Login dialog response- success");
									LoginDialog.Hide();
									LoginDialog.Destroy();
									LoginDialog = null;
                                                // Check if any recovery agent present;
                                                // if( domainController.GetRAList(DomainID) == null)
                                                // {
                                                        // No recovery agent present;
                                                 //       return;
                                                // }
                                                int result;
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
								Console.WriteLine(" remember Option true. Checking for passphrase existence");
								string passphrasecheck,uid;
								simws.GetPassPhrase( DomainID, out uid, out passphrasecheck);
								if(passphrasecheck == null || passphrasecheck == "")
									ShowVerifyDialog( DomainID, simws);
							}
						}
						else
						{
							ShowEnterPassPhraseDialog(DomainID, simws);
						}
//                                              string[] array = domainController.GetRAList( DomainID);
									break;
								case StatusCodes.InvalidCertificate:
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

		private bool ShowEnterPassPhraseDialog(string DomainID, SimiasWebService simws)
		{
			bool status = false;
			int result;	
			EnterPassPhraseDialog epd = new EnterPassPhraseDialog(DomainID);
			try
			{
			do
			{
				result = epd.Run();
				epd.Hide();
				if( result == (int)ResponseType.Cancel || result == (int) ResponseType.DeleteEvent)
					return true; //return status;
				if( epd.PassPhrase != epd.RetypedPassPhrase )
				{
					Console.WriteLine("PassPhrases do not match");
					// show an error message
					iFolderMsgDialog dialog = new iFolderMsgDialog(
						null,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.None,
						Util.GS("PassPhrase mismatch"),
						Util.GS("The PassPhrase and retyped PassPhrase are not same"),
						Util.GS("Please enter the passphrase again"));
						dialog.Run();
						dialog.Hide();
						dialog.Destroy();
						dialog = null;
				}
				else
					break;
			}while( result != (int)ResponseType.Cancel );
			
			if( epd.PassPhrase == epd.RetypedPassPhrase && (result !=(int)ResponseType.Cancel ))
			{
				// Check the recovery agent
				string publicKey = ""; // needed to be found
				Status passPhraseStatus = simws.SetPassPhrase( DomainID,epd.PassPhrase, epd.RecoveryAgent, publicKey);
				if(passPhraseStatus.statusCode == StatusCodes.Success)
				{
					status = true;
					simws.StorePassPhrase( DomainID, epd.PassPhrase, CredentialType.Basic, epd.ShouldSavePassPhrase);
				}
				else 
				{
					// error setting the passphrase
					// show an error message
					iFolderMsgDialog dialog = new iFolderMsgDialog(
						null,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.None,
						Util.GS("Error setting the PassPhrase"),
						Util.GS("Unable to set the passphrase"),
						Util.GS("Please try again"));
						dialog.Run();
						dialog.Hide();
						dialog.Destroy();
						dialog = null;
				}
			}
			}
			catch(Exception e)
			{
				return true;
			}
			return true;
//			return status;
		}

		private bool ShowVerifyDialog(string DomainID, SimiasWebService simws)
		{
			bool status = false;
			int result;
			Status passPhraseStatus= null;
			VerifyPassPhraseDialog vpd = new VerifyPassPhraseDialog();
			// vpd.TransientFor = this;
			try
			{
			do
			{
				result = vpd.Run();
				vpd.Hide();
				// Verify PassPhrase..  If correct store passphrase and set a local property..
				if( result == (int)ResponseType.Cancel || result == (int)ResponseType.DeleteEvent)
					return true; //return status;
				if( result == (int)ResponseType.Ok)
				{
					passPhraseStatus =  simws.ValidatePassPhrase(DomainID, vpd.PassPhrase);
				}
				if( passPhraseStatus != null)
				{
					if( passPhraseStatus.statusCode == StatusCodes.PassPhraseInvalid)  // check for invalid passphrase
					{
						// Display an error Message
						Console.WriteLine("Invalid Passphrase");
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
			if( passPhraseStatus != null && passPhraseStatus.statusCode == StatusCodes.Success)
			{
				status = true;
				try
				{
					simws.StorePassPhrase( DomainID, vpd.PassPhrase, CredentialType.Basic, vpd.ShouldSavePassPhrase);
				}
				catch(Exception ex) {}
			}
			else //if(result == (int)ResponseType.Cancel)
			{
				Console.WriteLine(" cancelled passphrase entry");
				simws.StorePassPhrase(DomainID, "a", CredentialType.None, false);
				string uid, passphrasecheck;
				simws.GetPassPhrase(DomainID, out uid, out passphrasecheck);
				if(passphrasecheck == "" || passphrasecheck == null)
					Console.WriteLine(" Cancel clicked at the time of login-- confirmed");
				else
					Console.WriteLine(" cancel clicked is not confirmed: {0}", passphrasecheck);
			}
			}
			catch(Exception e)
			{
				return true;
			}
			return true;
//			return status;
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
//					case SyncStatus.UpdateConflict:
//					case SyncStatus.FileNameConflict:
//						// Conflicts are handled in the OniFolderChangedEvent method
//						break;
//					case SyncStatus.Policy:
//						message = Util.GS("A policy prevented complete synchronization.");
//						break;
//					case SyncStatus.Access:
//						message = Util.GS("Insuficient rights prevented complete synchronization.");
//						break;
//					case SyncStatus.Locked:
//						message = Util.GS("The iFolder is locked.");
//						break;
					case SyncStatus.PolicyQuota:
//						message = Util.GS("The iFolder is full.  Click here to view the Synchronization Log.");
						message = Util.GS("The iFolder is full.\n\nClick <a href=\"ShowSyncLog\">here</a> to view the Synchronization Log.");
						break;
//					case SyncStatus.PolicySize:
//						message = Util.GS("A size restriction policy prevented complete synchronization.");
//						break;
//					case SyncStatus.PolicyType:
//						message = Util.GS("A file type restriction policy prevented complete synchronization.");
//						break;
//					case SyncStatus.DiskFull:
//						if (args.Direction == Simias.Client.Event.Direction.Uploading)
//						{
//							message = Util.GS("Insufficient disk space on the server prevented complete synchronization.");
//						}
//						else
//						{
//							message = Util.GS("Insufficient disk space on this computer prevented complete synchronization.");
//						}
//						break;
					case SyncStatus.ReadOnly:
						message = Util.GS("You have Read-only access to this iFolder.  Files that you place in this iFolder will not be synchronized.\n\nClick <a href=\"ShowSyncLog\">here</a> to view the Synchronization Log.");
						break;
//					default:
//						message = Util.GS("iFolder synchronization failed.");
//						break;
				}
				
				if (message != null)
				{
					Hashtable collectionSyncErrors = null;
					if (synchronizationErrors.ContainsKey(args.CollectionID))
					{
						collectionSyncErrors = (Hashtable)synchronizationErrors[args.CollectionID];
					}
					else
					{
						collectionSyncErrors = new Hashtable();
						synchronizationErrors[args.CollectionID] = collectionSyncErrors;
					}
					
					if (!collectionSyncErrors.ContainsKey(args.Status))
					{
						collectionSyncErrors[args.Status] = message;
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
					break;
				}
				case Simias.Client.Event.Action.StopSync:
				{
					currentIconAnimationDirection = 0;
					gAppIcon.Pixbuf = RunningPixbuf;

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
									ArrayList keysToClear = new ArrayList();
									foreach(SyncStatus syncStatusKey in errors)
									{
										string errMsg = (string) collectionSyncErrors[syncStatusKey];
										if (errMsg != null && errMsg.Length > 0)
										{
											NotifyWindow notifyWin = new NotifyWindow(
												tIcon, string.Format(Util.GS("Incomplete Synchronization: {0}"), ifHolder.iFolder.Name),
												errMsg,
												Gtk.MessageType.Warning, 10000);
											notifyWin.LinkClicked +=
												new LinkClickedEventHandler(OnNotifyWindowLinkClicked);
											notifyWin.ShowAll();
											
											// Set this message to "" so that
											// the notification bubble isn't
											// popped-up on every sync cycle.
											keysToClear.Add(syncStatusKey);
										}
									}
									
									// Clear out all the keys whose messages
									// were just notified to the user.
									foreach(SyncStatus syncStatusKey in keysToClear)
									{
										collectionSyncErrors[syncStatusKey] = "";
									}
								}
							}
						}
//					}

					collectionSynchronizing = null;

					break;
				}
			}
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
			switch(CurrentState)
			{
				case iFolderAppState.Starting:
					gAppIcon.Pixbuf = StartingPixbuf;
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

					gAppIcon.Pixbuf = RunningPixbuf;

					// Make sure the iFolder and Synchronization Windows are
					// initialized so that they begin receiving events as soon
					// as possible.
					iFolderWindow ifwin = Util.GetiFolderWindow();
					LogWindow logwin = Util.GetLogWindow(simiasManager);

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
						bool stopped = simiasManager.Stop();
					}
					catch(Exception e)
					{
						// ignore
						Console.WriteLine(e);
					}

					Application.Quit();
					break;
			}
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
					Util.LoadiFolderWindows();
			}
			else
				Console.WriteLine("DomainController instance is null");

			return false;	// Prevent this from being called over and over by GLib.Timeout
		}
		
		private void ShowAddAccountWizard()
		{
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
					new MenuItem (Util.GS("Account Settings..."));
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

			trayMenu.Append(new SeparatorMenuItem());



			ImageMenuItem prefs_item = new ImageMenuItem (
											Util.GS("Preferences"));
			prefs_item.Image = new Gtk.Image(Gtk.Stock.Preferences,
											Gtk.IconSize.Menu);
			trayMenu.Append(prefs_item);
			prefs_item.Activated +=
					new EventHandler(show_preferences);


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


		private void quit_ifolder(object o, EventArgs args)
		{
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
					bool stopped = simiasManager.Stop();
				}
				catch(Exception e)
				{
					Console.WriteLine(e.Message);
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
		
//		private void OnNetworkStateChanged(object o, NetworkStateChangedArgs args)
//		{
//			Console.WriteLine("OnNetworkStateChanged: {0}", args.Connected);
//		}
		
/*		
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
				Console.WriteLine("RegisterWithDBus() called with a null application.");
				return;
			}

			try
			{
				connection = Bus.GetSessionBus();
				if (connection == null)
				{
					Console.WriteLine("Could not get a connection to the D-Bus session");
					return;
				}
				
				service = new Service(connection, "com.novell.iFolder");
				
				if (service == null)
				{
					Console.WriteLine("Could not create a D-Bus service instance.");
					return;
				}
				
				service.RegisterObject(theApp, "/com/novell/iFolder/Application");
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				Console.WriteLine("Could not connect to D-Bus.  D-Bus support will be disabled for this instance: " + e.Message);
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
		public static void Main (string[] args)
		{
			Process[] processes =
				System.Diagnostics.Process.GetProcessesByName("iFolderClient");

			if(processes.Length > 1)
			{
				// Send a message to the first instance to show its main window
//				Connection connection = Bus.GetSessionBus();
//				Service service = Service.Get(connection, "com.novell.iFolder");
//				iFolderApplication initialApp = service.GetObject(typeof(iFolderApplication), "/opt/novell/iFolder/Application") as iFolderApplication
				Console.WriteLine("iFolder is already running!");
				return;
			}

			///
			/// Attempt to use D-Bus to prevent the user from running multiple
			/// copies of iFolder.  Instead, just present the main iFolder
			/// window to them.
			///
/*
			try
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
				Console.WriteLine(e);
				Process[] processes =
					System.Diagnostics.Process.GetProcessesByName("iFolderClient");
				if (processes.Length > 1)
				{
					Console.WriteLine("iFolder is already running.  If you were trying " +
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
				Console.WriteLine(bigException);
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
//				// Stop Simias (dereference iFolder's handle at least)
//				if(application.EventBroker != null)
//					application.EventBroker.Deregister();

				bool stopped = application.SimiasManager.Stop();

//				UnregisterWithDBus();
				Application.Quit();
			}
		}
	}
}
