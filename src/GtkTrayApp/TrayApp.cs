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
using System.Collections;
using Simias;
using Simias.Sync;
using System.Diagnostics;

using Gtk;
using Gdk;
using Glade;
using GtkSharp;
using GLib;
using Egg;

namespace Novell.iFolder
{
	public class TrayApplication 
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(TrayApplication));
		
		static Gtk.Image gAppIcon;
		static Gdk.PixbufAnimation gSyncAnimation;
		static Gdk.Pixbuf gNifPixbuf;
		static Gtk.EventBox eBox;
		static TrayIcon tIcon;
		static GtkTraceWindow twin;
//		static Gtk.ThreadNotify mainThreadNotify;
//		static SyncManagerStates syncState;

		public static void Main (string[] args)
		{
			Application.Init();

			Configuration conf = new Configuration();

            SimiasLogManager.Configure(conf);
            
            tIcon = new TrayIcon("iFolder");

			eBox = new EventBox();

			eBox.ButtonPressEvent += 
					new ButtonPressEventHandler(trayapp_clicked);
			gNifPixbuf = new Pixbuf("ifolder.png");

			gAppIcon = new Gtk.Image(gNifPixbuf);

			gSyncAnimation = new Gdk.PixbufAnimation("ifolder.gif");

			eBox.Add(gAppIcon);

			tIcon.Add(eBox);

			tIcon.ShowAll();

//			syncState = SyncManagerStates.Idle;

//			mainThreadNotify =
//				new Gtk.ThreadNotify(new Gtk.ReadyEvent(ChangeState));

			twin = new GtkTraceWindow();

			Console.WriteLine("iFolder is now running.");
			Application.Run();
		}




/*
		static void ChangeState()
		{
			lock(syncManager)
			{
				switch(syncState)
				{
					case SyncManagerStates.Active:
					{		
						gAppIcon.FromAnimation = gSyncAnimation;
						Console.WriteLine("SyncManager is Active");
						break;
					}
					case SyncManagerStates.Syncing:
					{
						gAppIcon.FromAnimation = gSyncAnimation;
						Console.WriteLine("SyncManager is Syncing");
						break;
					}
					default:
					{
						gAppIcon.Pixbuf = gNifPixbuf;
						Console.WriteLine("SyncManager is Idle");
						break;
					}
				}
			}
		}




		private static void syncManager_ChangedState(SyncManagerStates state)
		{
			lock(syncManager)
			{
				syncState = state;	
				mainThreadNotify.WakeupMain();
			}
		}
*/



		static void trayapp_clicked(object obj, ButtonPressEventArgs args)
		{
			switch(args.Event.Button)
			{
				case 1: // first mouse button
					if(args.Event.Type == Gdk.EventType.TwoButtonPress)
					{
						show_browser(obj, args);
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

			MenuItem browser_item = new MenuItem ("File Browser");
			trayMenu.Append (browser_item);
			browser_item.Activated += new EventHandler(show_browser);
			MenuItem colBrowser_item = new MenuItem ("Collection Browser");
			trayMenu.Append (colBrowser_item);
			colBrowser_item.Activated += new EventHandler(show_colbrowser);

			//			trayMenu.Append(new SeparatorMenuItem());

			MenuItem InvWizard_item = new MenuItem ("Invitation Wizard");
			trayMenu.Append (InvWizard_item);
			InvWizard_item.Activated += new EventHandler(show_invwizard);

			MenuItem AddrBook_item = new MenuItem ("Address Book");
			trayMenu.Append (AddrBook_item);
			AddrBook_item.Activated += new EventHandler(show_AddrBook);

			MenuItem tracewin_item = new MenuItem ("Show Trace Window");
			trayMenu.Append (tracewin_item);
			tracewin_item.Activated += new EventHandler(show_tracewin);

			MenuItem properties_item = new MenuItem ("Properties");
			trayMenu.Append (properties_item);
			properties_item.Activated += new EventHandler(show_properties);

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
			twin.ShowAll();
		}

		static void quit_ifolder(object o, EventArgs args)
		{
//			syncManager.Stop();
			Application.Quit();
		}

		static void show_properties(object o, EventArgs args)
		{
			Console.WriteLine("Show the Properties");
		}

		static void show_browser(object o, EventArgs args)
		{
			FileBrowser browser;

			browser = new FileBrowser();
			browser.ShowAll();
		}

		static void show_colbrowser(object o, EventArgs args)
		{
			CollectionBrowser browser;

			browser = new CollectionBrowser();
			browser.ShowAll();
		}

		static void show_AddrBook(object o, EventArgs args)
		{
			ContactBrowser cb = new ContactBrowser();
			cb.ShowAll();
		}

		static void show_invwizard(object o, EventArgs args)
		{
			InvitationWizard iWiz;

			iWiz = new InvitationWizard();
			iWiz.ShowAll();
		}
	}
}
