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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
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
using Simias.Client.Event;
using Simias.Client;
using Simias.Client.Authentication;


namespace Novell.iFolder
{
	public enum iFolderState : uint
	{
		Starting		= 0x0001,
		Stopping		= 0x0002,
		Running			= 0x0003,
		Stopped			= 0x0004
	}


	public class iFolderApplication : Gnome.Program
	{
		private Gtk.Image			gAppIcon;
		private Gdk.Pixbuf			RunningPixbuf;
		private Gdk.Pixbuf			StartingPixbuf;
		private Gdk.Pixbuf			StoppingPixbuf;
		private Gdk.PixbufAnimation	SyncingPixbuf;
		private Gtk.EventBox		eBox;
		private TrayIcon			tIcon;
		private iFolderWebService	ifws;
		private iFolderData			ifdata;
		private iFolderWindow 		ifwin;
		private LogWindow			logwin;
		private PreferencesWindow	prefswin;

		private iFolderState 		CurrentState;
		private Gtk.ThreadNotify	iFolderStateChanged;
		private SimiasEventBroker	EventBroker;
		private	iFolderLoginDialog	LoginDialog;
		private bool				logwinShown;

		public iFolderApplication(string[] args)
			: base("ifolder", "1.0", Modules.UI, args)
		{

			Util.InitCatalog();

			tIcon = new TrayIcon("iFolder");

			eBox = new EventBox();
			eBox.ButtonPressEvent += 
				new ButtonPressEventHandler(trayapp_clicked);

			RunningPixbuf = 
					new Pixbuf(Util.ImagesPath("ifolder24.png"));
			StartingPixbuf = 
					new Pixbuf(Util.ImagesPath("ifolder-startup.png"));
			StoppingPixbuf = 
					new Pixbuf(Util.ImagesPath("ifolder-shutdown.png"));
			SyncingPixbuf =
					new Gdk.PixbufAnimation(Util.ImagesPath("ifolder24.gif"));

			gAppIcon = new Gtk.Image(RunningPixbuf);

			eBox.Add(gAppIcon);
			tIcon.Add(eBox);
			tIcon.ShowAll();	

			LoginDialog = null;

			iFolderStateChanged = new Gtk.ThreadNotify(
							new Gtk.ReadyEvent(OniFolderStateChanged));

			logwin = new LogWindow();
			logwin.Destroyed += 
					new EventHandler(LogWindowDestroyedHandler);
			logwinShown = false;
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

			CurrentState = iFolderState.Starting;
			iFolderStateChanged.WakeupMain();

			if(ifws == null)
			{
				Simias.Client.Manager.Start();

				ifws = new iFolderWebService();
				ifws.Url = 
					Simias.Client.Manager.LocalServiceUrl.ToString() +
						"/iFolder.asmx";

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

					// Wait and ping it again
					System.Threading.Thread.Sleep(10);
				}

				try
				{
					// Setup to have data ready for events
					ifdata = iFolderData.GetData();

					EventBroker = new SimiasEventBroker();

					EventBroker.Register();
				}
				catch(Exception e)
				{
					Console.WriteLine(e);
					ifws = null;
				}
			}

			CurrentState = iFolderState.Running;
			iFolderStateChanged.WakeupMain();
		}




		private void StopiFolder()
		{
			CurrentState = iFolderState.Stopping;
			iFolderStateChanged.WakeupMain();

			try
			{
				if(EventBroker != null)
					EventBroker.Deregister();
				Simias.Client.Manager.Stop();
			}
			catch(Exception e)
			{
				// ignore
				Console.WriteLine(e);
			}

			CurrentState = iFolderState.Stopped;
			iFolderStateChanged.WakeupMain();
		}

		private DomainInformation GetDomainInformation(string DomainID)
		{
			DomainInformation di = null;

			try
			{
				SimiasWebService simiasSvc = 
								new SimiasWebService();
				simiasSvc.Url =
					Simias.Client.Manager.LocalServiceUrl.ToString() +
					"/Simias.asmx";

				di = simiasSvc.GetDomainInformation(DomainID);
			}
			catch{}

			return di;
		}

		private void OnSimiasNotifyEvent(object o, NotifyEventArgs args)
		{
			switch(args.EventData)
			{
				case "Domain-Up":
				{
					// See if credentials have already been set in
					// this process before showing the user the
					// login dialog.
					DomainAuthentication domainAuth =
						new DomainAuthentication(
							"iFolder",
							args.Message,
							null);

					Status status =
						domainAuth.Authenticate();

					if( (status.statusCode == StatusCodes.Success) ||
						(status.statusCode == StatusCodes.SuccessInGrace))
					{
						// DEBUG
						Console.WriteLine("Domain-Up: Credentials valid.");
					}
					else
					{
						// DEBUG
						Console.WriteLine("Domain-Up: Need credentials.");
						ReLogin(args.Message);
					}

					break;
				}
			}
		}

/*		private void OnShowReLogin(object o, EventArgs args)
		{
			ReLogin(redomainID);
		}
*/
		private void ReLogin(string domainID)
		{
			if(LoginDialog == null)
			{
				DomainInformation di = 
					GetDomainInformation(domainID);
				if(di != null)
				{
					bool authenticated = false;
					string userID;
					string credentials;

					LoginDialog = new iFolderLoginDialog(
						di.ID, di.Name, di.MemberName);

					LoginDialog.Response += new ResponseHandler(
						OnReLoginDialogResponse);

					// See if there is a password saved on this domain.
					SimiasWebService simiasWebService = 
						new SimiasWebService();

					simiasWebService.Url = 
						Simias.Client.Manager.LocalServiceUrl.ToString() + 
						"/Simias.asmx";

					CredentialType credType = 
						simiasWebService.GetDomainCredentials(
							domainID, 
							out userID, 
							out credentials);

					if ((credType == CredentialType.Basic) && 
						(credentials != null))
					{
						// There are credentials that were saved on the domain.
						// Use them to authenticate. If the authentication 
						// fails for any reason, pop up and ask for new 
						// credentials.
						DomainAuthentication domainAuth = 
							new DomainAuthentication(
								"iFolder",
								domainID, 
								credentials);

						Status status = 
							domainAuth.Authenticate();

						if( (status.statusCode == StatusCodes.Success) ||
							(status.statusCode == StatusCodes.SuccessInGrace))
						{
							authenticated = true;
						}
						else if (status.statusCode == StatusCodes.InvalidCredentials)
						{
							// There are bad credentials stored. Remove them.
							simiasWebService.SetDomainCredentials(domainID, null, CredentialType.None);
						}
					}

					if (!authenticated)
						LoginDialog.ShowAll();
				}
			}
			else
				LoginDialog.Present();
		}

		private void OnReLoginDialogResponse(object o, ResponseArgs args)
		{
			switch(args.ResponseId)
			{
				case Gtk.ResponseType.Ok:
				{
					Status status;
					DomainAuthentication cAuth = new DomainAuthentication(
							"iFolder", 
							LoginDialog.Domain, 
							LoginDialog.Password);
					status = cAuth.Authenticate();
					if( (status.statusCode == StatusCodes.Success) ||
						(status.statusCode == StatusCodes.SuccessInGrace))
					{
						LoginDialog.Hide();
						LoginDialog.Destroy();
						LoginDialog = null;
					}
					else
					{
						iFolderMsgDialog mDialog = new iFolderMsgDialog(
							LoginDialog, //tIcon, 
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.Ok,
							Util.GS("iFolder Connect Error"),
							Util.GS("Unable to Authenticate"),
							Util.GS("The password is invalid.  Please try again."));
						mDialog.Run();
						mDialog.Hide();
						mDialog.Destroy();
						mDialog = null;
					}
					break;
				}
				case Gtk.ResponseType.Cancel:
				{
					LoginDialog.Hide();
					LoginDialog.Destroy();
					LoginDialog = null;
					break;
				}
			}
		}


		private void OniFolderFileSyncEvent(object o, FileSyncEventArgs args)
		{
			if(ifwin != null)
				ifwin.HandleFileSyncEvent(args);
			if(logwin != null)
				logwin.HandleFileSyncEvent(args);
		}




		private void OniFolderSyncEvent(object o, CollectionSyncEventArgs args)
		{
			switch(args.Action)
			{
				case Action.StartSync:
				{
					gAppIcon.FromAnimation = SyncingPixbuf;
					break;
				}
				case Action.StopSync:
				{
					gAppIcon.Pixbuf = RunningPixbuf;
					break;
				}
			}
			if(ifwin != null)
				ifwin.HandleSyncEvent(args);

			if(logwin != null)
				logwin.HandleSyncEvent(args);
		}


		private void OniFolderAddedEvent(object o, iFolderAddedEventArgs args)
		{
			// don't notify us of our own iFolders
			iFolderHolder ifHolder = ifdata.GetiFolder(args.iFolderID, false);

			if(!ifdata.IsCurrentUser(ifHolder.iFolder.OwnerID))
			{
				if(ifHolder.iFolder.IsSubscription &&
					(ClientConfig.Get(ClientConfig.KEY_NOTIFY_IFOLDERS, "true") 
						 == "true"))
				{
					NotifyWindow notifyWin = new NotifyWindow(
							tIcon, 
							string.Format(Util.GS("New iFolder \"{0}\""), 
								ifHolder.iFolder.Name),
							string.Format(Util.GS("{0} has invited you to participate in this shared iFolder"), ifHolder.iFolder.Owner),
							Gtk.MessageType.Info, 10000);
					notifyWin.ShowAll();
				}
			}

			if(ifwin != null)
				ifwin.iFolderCreated(args.iFolderID);
		}




		private void OniFolderChangedEvent(object o, 
									iFolderChangedEventArgs args)
		{
			// don't notify us of our own iFolders
			iFolderHolder ifHolder = ifdata.GetiFolder(args.iFolderID, false);

			if(ifHolder.iFolder.IsSubscription)
			{
				if(ifwin != null)
					ifwin.iFolderChanged(args.iFolderID);
			}
			else
			{
				if(ClientConfig.Get(ClientConfig.KEY_NOTIFY_COLLISIONS, 
						"true") == "true")
				{
					NotifyWindow notifyWin = new NotifyWindow(
						tIcon, Util.GS("Action Required"),
						string.Format(Util.GS("A collision has been detected in iFolder \"{0}\""), ifHolder.iFolder.Name),
						Gtk.MessageType.Info, 10000);
					notifyWin.ShowAll();
				}

				if(ifwin != null)
					ifwin.iFolderHasConflicts(args.iFolderID);
			}
		}




		private void OniFolderDeletedEvent(object o, 
									iFolderDeletedEventArgs args)
		{
			if(ifwin != null)
				ifwin.iFolderDeleted(args.iFolderID);
		}


		private void OniFolderUserAddedEvent(object o,
									iFolderUserAddedEventArgs args)
		{
			if(ClientConfig.Get(ClientConfig.KEY_NOTIFY_USERS, "true") 
							== "true")
			{
				string username;
				iFolderHolder ifHolder = ifdata.GetiFolder(args.iFolderID, 
						false);

				if( (args.iFolderUser.FN != null) &&
					(args.iFolderUser.FN.Length > 0) )
					username = args.iFolderUser.FN;
				else
					username = args.iFolderUser.Name;

				NotifyWindow notifyWin = new NotifyWindow(
					tIcon, Util.GS("New iFolder User"), 
					string.Format(Util.GS("{0} has joined the iFolder \"{1}\""), username, ifHolder.iFolder.Name),
					Gtk.MessageType.Info, 10000);

				notifyWin.ShowAll();
			}
							
			// TODO: update any open windows?
//			if(ifwin != null)
//			ifwin.NewiFolderUser(ifolder, newuser);
		}




		// ThreadNotify Method that will react to a fired event
		private void OniFolderStateChanged()
		{
			switch(CurrentState)
			{
				case iFolderState.Starting:
					gAppIcon.Pixbuf = StartingPixbuf;
					break;

				case iFolderState.Running:
/*					if(ifSettings != null)
					{
						ifwin = new iFolderWindow(ifws, ifSettings);

						if(!ifSettings.HaveEnterprise)
							OnJoinEnterprise(null, null);
					}
*/

					if(EventBroker != null)
					{
						EventBroker.iFolderAdded +=
							new iFolderAddedEventHandler(
												OniFolderAddedEvent);
						EventBroker.iFolderChanged +=
							new iFolderChangedEventHandler(
												OniFolderChangedEvent);
						EventBroker.iFolderDeleted +=
							new iFolderDeletedEventHandler(
												OniFolderDeletedEvent);
						EventBroker.iFolderUserAdded +=
							new iFolderUserAddedEventHandler(
												OniFolderUserAddedEvent);
						EventBroker.CollectionSyncEventFired +=
							new CollectionSyncEventHandler(
												OniFolderSyncEvent);
						EventBroker.FileSyncEventFired +=
							new FileSyncEventHandler(
												OniFolderFileSyncEvent);
						
						EventBroker.NotifyEventFired +=
							new NotifyEventHandler(
												OnSimiasNotifyEvent);
					}

					gAppIcon.Pixbuf = RunningPixbuf;
					break;

				case iFolderState.Stopping:
					gAppIcon.Pixbuf = StoppingPixbuf;
					break;

				case iFolderState.Stopped:
					Application.Quit();
					break;
			}
		}




		private bool CheckWebService()
		{
			if(ifws == null)
			{
				try
				{
					ifws = new iFolderWebService();
					ifws.Ping();
				}
				catch(System.Net.WebException we)
				{
					ifws = null;

					if(we.Message == "Error: ConnectFailure")
					{
						iFolderMsgDialog mDialog = new iFolderMsgDialog(
							null,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.Ok,
							Util.GS("iFolder Connect Error"),
							Util.GS("Unable to locate Simias Process"),
							Util.GS("The Simias process must be running in order for iFolder to run.  Start the Simias process and try again"));
						mDialog.Run();
						mDialog.Hide();
						mDialog.Destroy();
						mDialog = null;
					}
					else
						throw we;
				}
				catch(Exception e)
				{
					ifws = null;

					iFolderExceptionDialog ied = new iFolderExceptionDialog(
													null, e);
					ied.Run();
					ied.Hide();
					ied.Destroy();
					ied = null;
				}
			}
			return(ifws != null);
		}





		private void trayapp_clicked(object obj, ButtonPressEventArgs args)
		{
			if(CurrentState == iFolderState.Starting)
				return;

			switch(args.Event.Button)
			{
				case 1: // first mouse button
					if(args.Event.Type == Gdk.EventType.TwoButtonPress)
					{
						showiFolderWindow(obj, args);
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
					new MenuItem (Util.GS("My iFolders..."));
			trayMenu.Append (iFolders_item);
			iFolders_item.Activated += 
					new EventHandler(showiFolderWindow);


			MenuItem accounts_item = 
					new MenuItem (Util.GS("Accounts"));
			trayMenu.Append (accounts_item);
			accounts_item.Activated += 
					new EventHandler(show_accounts);


			MenuItem logview_item = 
					new MenuItem (Util.GS("Synchronization Log"));
			trayMenu.Append (logview_item);
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
		}




		private void quit_ifolder(object o, EventArgs args)
		{
			if(ifwin != null)
			{
				ifwin.Destroyed -= new EventHandler(OniFolderWindowDestroyed);
				ifwin.Hide();
				ifwin.Destroy();
				ifwin = null;
			}

			if(logwin != null)
			{
				logwin.Destroyed -=
					new EventHandler(LogWindowDestroyedHandler);
				logwin.Hide();
				logwin.Destroy();
				logwin = null;
			}

			if(prefswin != null)
			{
				prefswin.Destroyed -=
					new EventHandler(PrefsWinDestroyedHandler);
				prefswin.Hide();
				prefswin.Destroy();
				prefswin = null;
			}

			if(CurrentState == iFolderState.Stopping)
			{
				System.Environment.Exit(1);
			}
			else
			{
				System.Threading.Thread stopThread = 
					new System.Threading.Thread(new ThreadStart(StopiFolder));
				stopThread.Start();
			}
		}




		private void show_help(object o, EventArgs args)
		{
			Util.ShowHelp("front.html", null);
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
			if(CheckWebService())
			{
				if(prefswin == null)
				{
					prefswin = new PreferencesWindow(ifws);
					
					prefswin.Destroyed +=
							new EventHandler(PrefsWinDestroyedHandler);
					prefswin.ShowAll();
					prefswin.CurrentPage = page;
				}
				else
				{
					prefswin.Present();
					prefswin.CurrentPage = page;
				}
			}
		}




		private void showiFolderWindow(object o, EventArgs args)
		{
			if(CheckWebService())
			{
				if(ifwin == null)
				{
					ifwin = new iFolderWindow(ifws);
					ifwin.Destroyed += 
							new EventHandler(OniFolderWindowDestroyed);
					ifwin.ShowAll();
				}
				else
					ifwin.Present();
			}
		}




		private void showLogWindow(object o, EventArgs args)
		{
			if(logwin == null)
			{
				logwin = new LogWindow();
				logwin.Destroyed += 
						new EventHandler(LogWindowDestroyedHandler);
				logwin.ShowAll();
				logwinShown = true;
			}
			else
			{
				if(logwinShown)
					logwin.Present();
				else
				{
					logwinShown = true;
					logwin.ShowAll();
				}
			}
		}




		private void OniFolderWindowDestroyed(object o, EventArgs args)
		{
			ifwin = null;
		}




		private void LogWindowDestroyedHandler(object o, EventArgs args)
		{
			logwin = null;
			logwinShown = false;
		}


		private void PrefsWinDestroyedHandler(object o, EventArgs args)
		{
			prefswin = null;
		}


		public static void Main (string[] args)
		{
			Process[] processes = 
				System.Diagnostics.Process.GetProcessesByName("iFolderGtkApp");

			if(processes.Length > 1)
			{
				Console.WriteLine("iFolder is already running!");
				return;
			}

			try
			{
				iFolderApplication app = new iFolderApplication(args);
				app.Run();
			}
			catch(Exception bigException)
			{
				Console.WriteLine(bigException);
				iFolderCrashDialog cd = new iFolderCrashDialog(bigException);
				cd.Run();
				cd.Hide();
				cd.Destroy();
				cd = null;
				Application.Quit();
			}
		}


	}
}
