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
//using Simias;
//using Simias.Sync;
//using Simias.Domain;
using System.Diagnostics;
using System.Threading;

using Gtk;
using Gdk;
using Gnome;
//using Glade;
using GtkSharp;
using GLib;
using Egg;
using Simias.Client.Event;
using Simias.Client;



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
		private iFolderWindow 		ifwin;
		private iFolderSettings		ifSettings;

		private iFolderState 		CurrentState;
		private Gtk.ThreadNotify	iFolderStateChanged;
		private SimiasEventBroker	EventBroker;
		private	iFolderLoginDialog	LoginDialog;

		public iFolderApplication(string[] args)
			: base("iFolder", "1.0", Modules.UI, args)
		{

			Util.InitCatalog();

			tIcon = new TrayIcon("iFolder");

			eBox = new EventBox();
			eBox.ButtonPressEvent += 
				new ButtonPressEventHandler(trayapp_clicked);

			RunningPixbuf = 
					new Pixbuf(Util.ImagesPath("ifolder.png"));
			StartingPixbuf = 
					new Pixbuf(Util.ImagesPath("ifolder-startup.png"));
			StoppingPixbuf = 
					new Pixbuf(Util.ImagesPath("ifolder-shutdown.png"));
			SyncingPixbuf =
					new Gdk.PixbufAnimation(Util.ImagesPath("ifolder.gif"));

			gAppIcon = new Gtk.Image(RunningPixbuf);

			eBox.Add(gAppIcon);
			tIcon.Add(eBox);
			tIcon.ShowAll();	

			LoginDialog = null;

			iFolderStateChanged = new Gtk.ThreadNotify(
							new Gtk.ReadyEvent(OniFolderStateChanged));
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
			CurrentState = iFolderState.Starting;
			iFolderStateChanged.WakeupMain();

			if(ifws == null)
			{
				try
				{
					Simias.Client.Manager.Start();

					ifws = new iFolderWebService();
					ifws.Url = 
						Simias.Client.Manager.LocalServiceUrl.ToString() +
							"/iFolder.asmx";

					ifws.Ping();
	
					ifSettings = ifws.GetSettings();

					EventBroker = new SimiasEventBroker();

					EventBroker.Register();
				}
				catch(Exception e)
				{
					Console.WriteLine(e);
					ifws = null;
					ifSettings = null;
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


		private void OniFolderSyncEvent(object o, CollectionSyncEventArgs args)
		{
			switch(args.Action)
			{
				case Action.StartSync:
				{
					gAppIcon.FromAnimation = SyncingPixbuf;
					// TODO: Add a log entry that we started
					break;
				}
				case Action.StopSync:
				{
					gAppIcon.Pixbuf = RunningPixbuf;
					// TODO: Add a log entry that we stopped
					break;
				}
			}
		}


		private void OniFolderAddedEvent(object o, iFolderAddedEventArgs args)
		{
			if(args.iFolder.IsSubscription)
			{
				NotifyWindow notifyWin = new NotifyWindow(
						tIcon, 
						string.Format(Util.GS("New iFolder \"{0}\""), 
													args.iFolder.Name),
						Util.GS("This iFolder is available to sync on this computer"),
						Gtk.MessageType.Info, 10000);
				notifyWin.ShowAll();
			}

			if(ifwin != null)
				ifwin.iFolderCreated(args.iFolder);
		}


		private void OniFolderChangedEvent(object o, 
									iFolderChangedEventArgs args)
		{
			if(args.iFolder.IsSubscription)
			{
				if(ifwin != null)
					ifwin.iFolderChanged(args.iFolder);
			}
			else
			{
				NotifyWindow notifyWin = new NotifyWindow(
						tIcon, Util.GS("Action Required"),
						string.Format(Util.GS("A collision has been detected in iFolder \"{0}\""), args.iFolder.Name),
						Gtk.MessageType.Info, 10000);
				notifyWin.ShowAll();

				if(ifwin != null)
					ifwin.iFolderHasConflicts(args.iFolder);
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
			NotifyWindow notifyWin = new NotifyWindow(
				tIcon, Util.GS("New iFolder User"), 
				string.Format(Util.GS("{0} has joined an iFolder"), args.iFolderUser.Name),
				Gtk.MessageType.Info, 10000);

			notifyWin.ShowAll();
							
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
					if( (ifSettings != null) && (!ifSettings.HaveEnterprise) )
						OnJoinEnterprise(null, null);

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
	
					ifSettings = ifws.GetSettings();
				}
				catch(System.Net.WebException we)
				{
					ifSettings = null;
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
					ifSettings = null;
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
						if( (ifSettings != null) && 
							(!ifSettings.HaveEnterprise) )
							OnJoinEnterprise(obj, args);
						else
							show_properties(obj, args);
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
			AccelGroup agrp = new AccelGroup();
			Menu trayMenu = new Menu();

			if( (ifSettings != null) && (ifSettings.HaveEnterprise) )
			{
				MenuItem iFolders_item = 
						new MenuItem (Util.GS("My iFolders..."));
				trayMenu.Append (iFolders_item);
				iFolders_item.Activated += 
						new EventHandler(show_properties);
			}
			
			if( (ifSettings != null) && (!ifSettings.HaveEnterprise) )
			{
				MenuItem connect_item = 
						new MenuItem (Util.GS("Join Enterprise Server"));
				trayMenu.Append (connect_item);
				connect_item.Activated += new EventHandler(OnJoinEnterprise);
			}

			ImageMenuItem help_item = new ImageMenuItem (Gtk.Stock.Help, agrp);
			trayMenu.Append (help_item);
			help_item.Activated += 
					new EventHandler(show_help);

			trayMenu.Append(new SeparatorMenuItem());


			ImageMenuItem quit_item = new ImageMenuItem (Gtk.Stock.Quit, agrp);
			quit_item.Activated += new EventHandler(quit_ifolder);
			trayMenu.Append (quit_item);

			trayMenu.ShowAll();

			trayMenu.Popup(null, null, null, IntPtr.Zero, 3, 
					Gtk.Global.CurrentEventTime);
		}




		private void quit_ifolder(object o, EventArgs args)
		{
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



		private void OnJoinEnterprise(object o, EventArgs args)
		{
			if(LoginDialog == null)
			{
				LoginDialog = new iFolderLoginDialog();
				LoginDialog.Response +=
					new ResponseHandler(OnLoginDialogResponse);
				LoginDialog.ShowAll();
			}
			else
				LoginDialog.Present();
		}


		private void OnLoginDialogResponse(object o, ResponseArgs args)
		{
			switch(args.ResponseId)
			{
				case Gtk.ResponseType.Ok:
				{
					LoginDialog.Hide();
					try
					{
						iFolderSettings tmpSettings;
						tmpSettings = ifws.ConnectToEnterpriseServer(
														LoginDialog.UserName,
														LoginDialog.Password,
														LoginDialog.Host);
						ifSettings = tmpSettings;
	
						EventBroker.RefreshSettings();
						
						if(ifwin != null)
						{
							ifwin.GlobalSettings = ifSettings;
						}
					}
					catch(Exception e)
					{
						iFolderExceptionDialog ied = new iFolderExceptionDialog(
														null, e);
						ied.Run();
						ied.Hide();
						ied.Destroy();
						ied = null;
					}
					LoginDialog.Destroy();
					LoginDialog = null;
					break;
				}
				default:
				{
					LoginDialog.Hide();
					LoginDialog.Destroy();
					LoginDialog = null;
					break;
				}
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




		private void show_properties(object o, EventArgs args)
		{
			if(CheckWebService())
			{
				if(ifwin == null)
				{
					ifwin = new iFolderWindow(ifws, ifSettings);
					ifwin.ShowAll();
				}
				else
				{
					// this will raise the window to the front
					ifwin.Present();
				}
//				iFolderWindow win;

//				win = new iFolderWindow(ifws);
//				win.ShowAll();
			}
/*
			ApplicationProperties propDialog;

			propDialog = new ApplicationProperties();
			propDialog.Run();
*/
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
