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

	public class TrayApplication 
	{
		//private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(TrayApplication));
		
		static Gtk.Image			gAppIcon;
		static Gdk.Pixbuf			ifNormalPixbuf;
		static Gtk.EventBox			eBox;
		static TrayIcon				tIcon;
		static Gtk.ThreadNotify		iFolderStateNotify;
		static iFolderWebService	ifws;
		static iFolderWindow 		ifwin;
		static iFolderSettings		ifSettings;

		public static void Main (string[] args)
		{
			Process[] processes = 
				System.Diagnostics.Process.GetProcessesByName("iFolderGtkApp");

			if(processes.Length > 1)
			{
				Console.WriteLine("iFolder is already running!");
				return;
			}

			Gnome.Program program =
				new Program("iFolder", "0.10.0", Modules.UI, args);
			
			// This is my huge try catch block to catch any exceptions
			// that are not caught
			try
			{
				tIcon = new TrayIcon("iFolder");

				eBox = new EventBox();
				eBox.ButtonPressEvent += 
					new ButtonPressEventHandler(trayapp_clicked);

				ifNormalPixbuf = new Pixbuf(Util.ImagesPath("ifolder.png"));

				gAppIcon = new Gtk.Image(ifNormalPixbuf);
				//gSyncAnimation = new Gdk.PixbufAnimation("ifolder.gif");
				eBox.Add(gAppIcon);
				tIcon.Add(eBox);
				tIcon.ShowAll();	

				iFolderStateNotify = new Gtk.ThreadNotify(
								new Gtk.ReadyEvent(iFolderStateChange));

				CheckWebService();

				program.Run();
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


		static private bool CheckWebService()
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



		// Call this method to activate this method
		// iFolderStateNotify.WakeupMain();
		static void iFolderStateChange()
		{
			// This will be done when we get the
			// event system
		}




		static void trayapp_clicked(object obj, ButtonPressEventArgs args)
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




		static void show_tray_menu()
		{
			AccelGroup agrp = new AccelGroup();
			Menu trayMenu = new Menu();

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

		static void show_tracewin(object o, EventArgs args)
		{
//			twin.ShowAll();
		}

		static void quit_ifolder(object o, EventArgs args)
		{
			Application.Quit();
		}

		static void OnJoinEnterprise(object o, EventArgs args)
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

		static void show_help(object o, EventArgs args)
		{
			Util.ShowHelp("front.html", null);
		}

		static void show_about(object o, EventArgs args)
		{
			Util.ShowAbout();
		}

		static void show_properties(object o, EventArgs args)
		{
			if(CheckWebService())
			{
				if(ifwin == null)
				{
					ifwin = new iFolderWindow(ifws);
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
	}
}
