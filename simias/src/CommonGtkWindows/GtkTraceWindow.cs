/***********************************************************************
 *  GtkTraceWindow.cs - Gtk-Sharp Trace Window
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/

using Gtk;
using Gdk;
using GtkSharp;
using System;
using System.Diagnostics;
using System.Collections;

namespace Simias
{
	public class GtkTraceWindow : TraceListener
	{
		private Gtk.Window win;
		private Gtk.TreeStore store;
		private Gtk.TreeView tv;
		private Gtk.Button stopButton;
		private Gtk.Button startButton;
		private Gtk.ThreadNotify threadNotify;
		private Queue messageQueue;

		bool pause = false;

		public GtkTraceWindow()
		{
			store = new TreeStore(typeof(string));
			win = null;

			messageQueue = new Queue();

			threadNotify = 
				new Gtk.ThreadNotify(new Gtk.ReadyEvent(ProcessTraceQueue));

			Trace.Listeners.Add(this);
		}

		void ProcessTraceQueue()
		{
			lock(messageQueue)
			{
				Console.WriteLine("Main Wokeup and is processing queue");

				while(messageQueue.Count > 0)
				{
					string message = (string) messageQueue.Dequeue();

					TreeIter ti = store.AppendValues(message);

					if( (win != null) && (!pause) )
					{
						tv.ScrollToCell(store.GetPath(ti), 
								null, false, 0, 0);
					}
				}
			}
		}


		public void ShowAll()
		{
			if(win == null)
			{
				win = new Gtk.Window ("Denali Trace Window");
				win.DeleteEvent += new DeleteEventHandler (Window_Delete);
				win.SetDefaultSize(640, 480);

				VBox vb = new VBox(false, 10);
				HBox hb = new HBox(false, 10);

				Button closeButton = new Button(Stock.Close);
				hb.PackEnd(closeButton, false, false, 5);
				closeButton.Clicked += 
					new EventHandler (on_closeButton_clicked);

				stopButton = new Button(Stock.Stop);
				hb.PackEnd(stopButton, false, false, 5);
				stopButton.Clicked += 
					new EventHandler (on_stopstartButton_clicked);
				startButton = new Button(Stock.Execute);
				startButton.Hide();
				hb.PackEnd(startButton, false, false, 5);
				startButton.Clicked += 
					new EventHandler (on_stopstartButton_clicked);


				ScrolledWindow sw = new ScrolledWindow();
				vb.PackStart(sw, true, true, 0);
				vb.PackStart(hb, false, false, 5);

				win.Add(vb);

				tv = new TreeView();
				tv.Model = store;
				tv.HeadersVisible = false;
				tv.AppendColumn("Messages", new CellRendererText(), "text", 0);
				sw.Add(tv);
			}
			win.ShowAll();
			startButton.Hide();
		}

		private void Window_Delete (object obj, DeleteEventArgs args)
		{
			win.Hide();
			win.Destroy();
			win = null;
			args.RetVal = true;
		}

		public override void Write(string message)
		{
			WriteLine(message);
		}

		public override void WriteLine(string message)
		{
			lock(messageQueue)
			{
				messageQueue.Enqueue(message);
				Console.WriteLine("Calling to WakeupMain");
				threadNotify.WakeupMain();
			}
		}

		public bool Pause
		{
			set
			{
				pause = value;
			}
		}


		private void on_closeButton_clicked(object obj, EventArgs args)
		{
			win.Hide();
		}


		private void on_stopstartButton_clicked(object obj, EventArgs args)
		{
			pause = !pause;
			if(pause == true)
			{
				stopButton.Hide();
				startButton.ShowAll();
			}
			else
			{
				stopButton.ShowAll();
				startButton.Hide();
			}

		}


		private void window_clicked(object obj, ButtonPressEventArgs args)
		{
			Console.WriteLine("Ney, we clicked");
			switch(args.Event.Button)
			{
				case 1: // first mouse button
					/*
					   if(args.Event.type == Gdk.EventType.TwoButtonPress)
					   {
					   show_browser(obj, args);
					   }
					 */
					break;
				case 2: // second mouse button
					break;
				case 3: // third mouse button
					show_menu();
					break;
			}
		}

		private void show_menu()
		{
			Menu pMenu = new Menu();

			MenuItem pause_item;

			if(pause)
				pause_item = new MenuItem ("Pause");
			else
				pause_item = new MenuItem ("Continue");

			pMenu.Append (pause_item);
			pause_item.Activated += new EventHandler(pause_window);

			MenuItem clear_item = new MenuItem ("Clear");
			pMenu.Append (clear_item);
			clear_item.Activated += new EventHandler(clear_all);

			pMenu.Append(new SeparatorMenuItem());

			MenuItem copy_item = new MenuItem ("Copy");
			pMenu.Append (copy_item);
			copy_item.Activated += new EventHandler(copy_contents);

			pMenu.ShowAll();

			pMenu.Popup(null, null, null, IntPtr.Zero, 3, Gtk.Global.CurrentEventTime);
		}

		private void pause_window(object o, EventArgs args)
		{
			pause = !pause;
		}

		private void clear_all(object o, EventArgs args)
		{
			store.Clear();
		}

		private void copy_contents(object o, EventArgs args)
		{
		}

		/*
		   private void PumpMessages()
		   {
		   while(Application.EventsPending())
		   Application.RunIteration(false);
		   }
		 */
	}
}
