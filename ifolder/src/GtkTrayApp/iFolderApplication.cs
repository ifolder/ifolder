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

namespace Novell.iFolder
{
	public enum iFolderEvent : uint
	{
		Starting		= 0x0001,
		Stopping		= 0x0002,
		Running			= 0x0003,
		Stopped			= 0x0004,
		Syncing			= 0x0005,
		NewFolder		= 0x0006,
		NewMember		= 0x0007,
		NewConflict		= 0x0008
	}

	public class iFolderApplication : Gnome.Program
	{
		private Gtk.Image			gAppIcon;
		private Gdk.Pixbuf			RunningPixbuf;
		private Gdk.Pixbuf			StartingPixbuf;
		private Gdk.Pixbuf			StoppingPixbuf;
//		private Gdk.Pixbuf			EventPixbuf;
//		private Gdk.Pixbuf			SyncingPixbuf;
		private Gtk.EventBox		eBox;
		private TrayIcon			tIcon;
		private iFolderWebService	ifws;
		private iFolderWindow 		ifwin;
		private iFolderSettings		ifSettings;

		private iFolderEvent 			CurrentEvent;
		private Gtk.ThreadNotify		iFolderEventNotify;
//		private System.Diagnostics.Process SimiasProcess = null;


		public iFolderApplication(string[] args)
			: base("iFolder", "1.0", Modules.UI, args)
		{
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

			gAppIcon = new Gtk.Image(RunningPixbuf);

			eBox.Add(gAppIcon);
			tIcon.Add(eBox);
			tIcon.ShowAll();	

			iFolderEventNotify = new Gtk.ThreadNotify(
							new Gtk.ReadyEvent(OniFolderEventFired));
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
			CurrentEvent = iFolderEvent.Starting;
			iFolderEventNotify.WakeupMain();

/*			Process[] processes = 
				System.Diagnostics.Process.GetProcessesByName("SimiasApp");

			if(processes.Length > 1)
			{
				Console.WriteLine("Simias is already running!");
				return;
			}
			else
			//if(SimiasProcess == null)
			{
				SimiasProcess = new Process();
				SimiasProcess.StartInfo.RedirectStandardOutput = false;
				SimiasProcess.StartInfo.CreateNoWindow = true;
				SimiasProcess.StartInfo.UseShellExecute = false;
				SimiasProcess.StartInfo.FileName = 
					"/home/calvin/test/bin/simias";
				SimiasProcess.Start();
			}
*/

			if(ifws == null)
			{
				try
				{
					ifws = new iFolderWebService();
	
					ifSettings = ifws.GetSettings();
				}
				catch(Exception e)
				{
					ifws = null;
					ifSettings = null;
				}
			}

			CurrentEvent = iFolderEvent.Running;
			iFolderEventNotify.WakeupMain();
		}




		private void StopiFolder()
		{
			CurrentEvent = iFolderEvent.Stopping;
			iFolderEventNotify.WakeupMain();

/*			if(SimiasProcess != null)
			{
				SimiasProcess.CloseMainWindow();
				if(!SimiasProcess.WaitForExit(10000))
				{
					SimiasProcess.Kill();
					if(!SimiasProcess.WaitForExit(100))
					{
						Console.WriteLine("The process won't die");
					}
				}
			}
*/

			CurrentEvent = iFolderEvent.Stopped;
			iFolderEventNotify.WakeupMain();
		}




		// ThreadNotify Method that will react to a fired event
		private void OniFolderEventFired()
		{
			switch(CurrentEvent)
			{
				case iFolderEvent.Starting:
					gAppIcon.Pixbuf = StartingPixbuf;
					break;

				case iFolderEvent.Running:
					gAppIcon.Pixbuf = RunningPixbuf;
					break;

				case iFolderEvent.Stopping:
					gAppIcon.Pixbuf = StoppingPixbuf;
					break;

				case iFolderEvent.Stopped:
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
	
					// TODO: change this to some kind of init code
					ifSettings = ifws.GetSettings();
					//ifws.Ping();
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
							"iFolder Connect Error",
							"Unable to locate Simias Process",
							"The Simias process must be running in order for iFolder to run.  Start the Simias process and try again");
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
			switch(args.Event.Button)
			{
				case 1: // first mouse button
					if(args.Event.Type == Gdk.EventType.TwoButtonPress)
					{
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

			MenuItem Notify_item = new MenuItem ("Notify Me");
			trayMenu.Append (Notify_item);
			Notify_item.Activated += 
					new EventHandler(show_notify);
			MenuItem iFolders_item = new MenuItem ("My iFolders...");
			trayMenu.Append (iFolders_item);
			iFolders_item.Activated += 
					new EventHandler(show_properties);
			
			if( (ifSettings != null) && (!ifSettings.HaveEnterprise) )
			{
				MenuItem connect_item = new MenuItem ("Join Enterprise Server");
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
			if(CurrentEvent == iFolderEvent.Stopping)
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
			iFolderLoginDialog loginDialog = new iFolderLoginDialog();

			int rc = loginDialog.Run();
			loginDialog.Hide();
			loginDialog.Destroy();
			if(rc == -5)
			{
				try
				{
					iFolderSettings tmpSettings;
					tmpSettings = ifws.ConnectToEnterpriseServer(
													loginDialog.UserName,
													loginDialog.Password,
													loginDialog.Host);
					ifSettings = tmpSettings;

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



		private void show_notify(object o, EventArgs args)
		{
			NotifyWindow notifyWin = new NotifyWindow(tIcon,
									"New Notification!",
									"You are looking at a spiffy new notification");
			notifyWin.ShowAll();
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
