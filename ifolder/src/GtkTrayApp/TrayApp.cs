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
using Novell.AddressBook;
using Novell.AddressBook.UI.gtk;
using System.Collections;
using Simias;
using Simias.Sync;
using Simias.Domain;
using System.Diagnostics;
using System.Threading;

using Gtk;
using Gdk;
using Gnome;
using Glade;
using GtkSharp;
using GLib;
using Egg;

namespace Novell.iFolder
{
	public enum ServiceStates : uint
	{
		stopped = 0x0001,
		started = 0x0002,
		starting = 0x0003,
		stopping = 0x0004
	}


	public class TrayApplication 
	{
		//private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(TrayApplication));
		
		static Gtk.Image gAppIcon;
		//static Gdk.PixbufAnimation gSyncAnimation;
		static Gdk.Pixbuf ifNormalPixbuf;
		static Gdk.Pixbuf ifStartingPixbuf;
		static Gdk.Pixbuf ifStoppingPixbuf;
		static Gtk.EventBox eBox;
		static TrayIcon tIcon;
		static Configuration conf;
//		static Simias.Service.Manager sManager = null;
		static Gtk.ThreadNotify ServicesStateNotify;
		static ServiceStates serviceState;
//		static Mutex TrayMutex;

		static iFolderWebService ifws;

		public static void Main (string[] args)
		{
			conf = Configuration.GetConfiguration();
			Process[] processes = 
				System.Diagnostics.Process.GetProcessesByName("iFolderGtkApp");

			if(processes.Length > 1)
			{
				Console.WriteLine("iFolder is already running!");
				return;
			}

			Gnome.Program program =
				new Program("iFolder", "0.10.0", Modules.UI, args);
			
			serviceState = ServiceStates.stopped;
			SimiasLogManager.Configure(conf);
//			TrayMutex = new Mutex();

			// This is my huge try catch block to catch any exceptions
			// that are not caught
			try
			{
				tIcon = new TrayIcon("iFolder");

				eBox = new EventBox();

				eBox.ButtonPressEvent += 
					new ButtonPressEventHandler(trayapp_clicked);

				ifNormalPixbuf = new Pixbuf(Util.ImagesPath("ifolder.png"));
				ifStartingPixbuf = 
					new Pixbuf(Util.ImagesPath("ifolder-startup.png"));
				ifStoppingPixbuf = 
					new Pixbuf(Util.ImagesPath("ifolder-shutdown.png"));

				gAppIcon = new Gtk.Image(ifStartingPixbuf);
	
				//gSyncAnimation = new Gdk.PixbufAnimation("ifolder.gif");

				eBox.Add(gAppIcon);

				tIcon.Add(eBox);

				tIcon.ShowAll();	

//				iFolderManager.CreateDefaultExclusions(conf);
//				sManager = new Simias.Service.Manager(conf);

				ServicesStateNotify = 
				new Gtk.ThreadNotify(new Gtk.ReadyEvent(ServiceStateChange));

				System.Threading.Thread servicesThread =
					new System.Threading.Thread(new ThreadStart(StartServices));

				servicesThread.Start();

				program.Run();
//				Application.Run();
			}
			catch(Exception bigException)
			{
//				if(sManager != null)
//					sManager.StopServices();

				iFolderCrashDialog cd = new iFolderCrashDialog(bigException);
				cd.Run();
				cd.Hide();
				cd.Destroy();
				cd = null;
//				sManager.WaitForServicesStopped();
				Application.Quit();
			}
		}

		static private void SetServiceState(ServiceStates state)
		{
//			lock(TrayMutex)
//			{
				serviceState = state;
//			}
		}

		static private ServiceStates GetServiceState()
		{
//			lock(TrayMutex)
//			{
				return serviceState;
//			}
		}

		static private void StartServices()
		{
			SetServiceState(ServiceStates.starting);

			try
			{
				ifws = new iFolderWebService();
				// TODO: change this to some kind of init code
				ifws.IsiFolder("/initiFolder");
			}
			catch(Exception e)
			{
			}

//			SyncProperties props = new SyncProperties(conf);
//			props.LogicFactory = typeof(SynkerA);

//			sManager.StartServices();
//			sManager.WaitForServicesStarted();

			SetServiceState(ServiceStates.started);
			ServicesStateNotify.WakeupMain();
		}

		static private void StopServices()
		{
//			sManager.WaitForServicesStarted();

			SetServiceState(ServiceStates.stopping);

//			sManager.StopServices();
//			sManager.WaitForServicesStopped();

			SetServiceState(ServiceStates.stopped);
			ServicesStateNotify.WakeupMain();
		}

		static void ServiceStateChange()
		{
			ServiceStates curState = GetServiceState();
			switch(curState)
			{
				case ServiceStates.starting:
					gAppIcon.Pixbuf = ifStartingPixbuf;
					break;
				default:
				case ServiceStates.started:
				{
					if(ifws == null)
					{
						iFolderMsgDialog mDialog = 
							new iFolderMsgDialog(Gtk.Stock.DialogError,
							"iFolder Connect Error",
							"iFolder is Unable to locate the Simias Process",
							"The Simias process must be running in order for iFolder to run.  Start the Simias process and try again");
						mDialog.Run();
						mDialog.Hide();
						mDialog.Destroy();
						mDialog = null;
					}

					gAppIcon.Pixbuf = ifNormalPixbuf;
					break;
				}
				case ServiceStates.stopping:
					gAppIcon.Pixbuf = ifStoppingPixbuf;
					break;
				case ServiceStates.stopped:
					Application.Quit();
					break;
			}
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
			Menu trayMenu = new Menu();

//			MenuItem ifolder_browser_item = 
//					new MenuItem ("iFolder Browser");
//			trayMenu.Append (ifolder_browser_item);
//			ifolder_browser_item.Activated += 
//					new EventHandler(show_ifolder_browser);

			MenuItem connect_item = new MenuItem ("iFolder Server Connect...");
			trayMenu.Append (connect_item);
			connect_item.Activated += new EventHandler(show_server_info);

			trayMenu.Append(new SeparatorMenuItem());
			
			MenuItem iFolders_item = new MenuItem ("iFolders");
			trayMenu.Append (iFolders_item);
			iFolders_item.Activated += 
					new EventHandler(show_properties);
			
			MenuItem messages_item = 
					new MenuItem ("Subscriptions");
			trayMenu.Append (messages_item);
			messages_item.Activated +=
					new EventHandler(show_messages);

			MenuItem InvWizard_item = new MenuItem ("Invitation Assistant");
			trayMenu.Append (InvWizard_item);
			InvWizard_item.Activated += new EventHandler(show_invwizard);

			MenuItem AddrBook_item = new MenuItem ("Address Book");
			trayMenu.Append (AddrBook_item);
			AddrBook_item.Activated += new EventHandler(show_AddrBook);

//			MenuItem tracewin_item = new MenuItem ("Show Trace Window");
//			trayMenu.Append (tracewin_item);
//			tracewin_item.Activated += new EventHandler(show_tracewin);

			trayMenu.Append(new SeparatorMenuItem());

			MenuItem about_item = new MenuItem ("About...");
			trayMenu.Append (about_item);
			about_item.Activated += 
					new EventHandler(show_about);
			MenuItem help_item = new MenuItem ("Help");
			trayMenu.Append (help_item);
			help_item.Activated += 
					new EventHandler(show_help);

			trayMenu.Append(new SeparatorMenuItem());
			
			
#if DEBUG
			MenuItem colBrowser_item = 
					new MenuItem ("Collection Browser");
			trayMenu.Append (colBrowser_item);
			colBrowser_item.Activated += 
					new EventHandler(show_colbrowser);
			MenuItem rbBrowser_item = new MenuItem ("Reunion Browser");
			trayMenu.Append (rbBrowser_item);
			rbBrowser_item.Activated += new EventHandler(show_rbbrowser);

			trayMenu.Append(new SeparatorMenuItem());
#endif			
			

			MenuItem properties_item = new MenuItem ("Properties");
			trayMenu.Append (properties_item);
			properties_item.Activated += 
					new EventHandler(show_properties);

			trayMenu.Append(new SeparatorMenuItem());

			MenuItem quit_item = new MenuItem ("Exit");
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
			ServiceStates curState = GetServiceState();
			if(curState == ServiceStates.stopping)
			{
				System.Environment.Exit(1);
			}
			else
			{
				SetServiceState(ServiceStates.stopping);
				ServiceStateChange();

				System.Threading.Thread stopThread =
					new System.Threading.Thread(new ThreadStart(StopServices));

				stopThread.Start();
			}
		}

		static void show_server_info(object o, EventArgs args)
		{
//			SimiasLogManager.Configure(conf);
/*
CRG: WebService Fixup
			ServerInfoDialog sid = new ServerInfoDialog();
			int rc = sid.Run();
			if(rc == -5)
			{
				Console.WriteLine("Connecting to server...");
				DomainAgent da = new DomainAgent(conf);
				da.Attach(sid.Address, sid.Name, sid.Password);
			}
*/
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
			if(ifws != null)
			{
				iFolderWindow win;

				win = new iFolderWindow(ifws);
				win.ShowAll();
			}
/*
			ApplicationProperties propDialog;

			propDialog = new ApplicationProperties();
			propDialog.Run();
*/
		}

		static void show_rbbrowser(object o, EventArgs args)
		{
/*
CRG: WebService Fixup
			ReunionBrowser browser;

			browser = new ReunionBrowser();
			browser.ShowAll();
*/
		}
		
		static void show_messages(object o, EventArgs args)
		{
			POBoxViewer boxViewer;
			
			boxViewer = new POBoxViewer();
			boxViewer.ShowAll();
		}

		static void show_colbrowser(object o, EventArgs args)
		{
/*
CRG: WebService Fixup
			CollectionBrowser browser;

			browser = new CollectionBrowser();
			browser.ShowAll();
*/
		}

		static void show_AddrBook(object o, EventArgs args)
		{
			ContactBrowser cb = new ContactBrowser();
			cb.ShowAll();
		}

		static void show_invwizard(object o, EventArgs args)
		{
			InvitationAssistant iAss;

			iAss = new InvitationAssistant();
			iAss.ShowAll();
		}
	}
}
