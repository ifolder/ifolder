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

namespace Simias
{
	public class GtkTraceWindow : TraceListener
	{
		private Gtk.Window win;
		private Gtk.TreeStore store;
		private Gtk.TreeView tv;
		private Gtk.TextView LogTextView;
		private TextBuffer LogTextBuffer;

		bool pause = false;

		public GtkTraceWindow()
		{
			store = new TreeStore(typeof(string));
			LogTextBuffer = new TextBuffer(null);
			win = null;
			
			Trace.Listeners.Add(this);
		}

		public void ShowAll()
		{
			if(win == null)
			{
				win = new Gtk.Window ("Denali Trace Window");
				win.DeleteEvent += new DeleteEventHandler (Window_Delete);
				//win.ButtonPressEvent += new ButtonPressEventHandler(window_clicked);
				win.SetDefaultSize(640, 240);

				ScrolledWindow sw = new ScrolledWindow();
				win.Add(sw);

//				LogTextView = new TextView(LogTextBuffer);
//				LogTextView.Editable = false;
				//LogTextView.WrapMode = WrapMode.Word;
//				LogTextView.CursorVisible = false;
//				sw.Add(LogTextView);

				tv = new TreeView();
				tv.Model = store;
				tv.HeadersVisible = false;
				tv.AppendColumn("Messages", new CellRendererText(), "text", 0);
				sw.Add(tv);

			}
			win.ShowAll();
		}
		
		private void Window_Delete (object obj, DeleteEventArgs args)
		{
			Console.WriteLine("Dude, you are going away!");
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
			Console.WriteLine(message);
/*
				AppendLog(message);
				AppendLog("\r\n");

				if(win == null)
					return;

				if(!pause)
				{
					TextIter endIter;

					LogTextBuffer.GetIterAtOffset(out endIter, -1);
		 			LogTextView.ScrollToIter(endIter, 0, true, 0, 1);
				//	LogTextView.MoveVisually(endIter, 1);
				}

				//PumpMessages();
*/
			TreeIter ti = store.AppendValues(message);

			if(win == null)
				return;

			if(!pause)
			{
				tv.ScrollToCell(store.GetPath(ti), null, false, 0, 0);
			}
		}

		public bool Pause
		{
			set
			{
				pause = value;
			}
		}
		
		private void window_clicked(object obj, ButtonPressEventArgs args)
		{
			Console.WriteLine("Ney, we clicked");
			switch(args.Event.button)
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

		private void AppendLog(string insertText)
		{
			TextIter endIter;

			LogTextBuffer.GetIterAtOffset(out endIter, -1);
			LogTextBuffer.Insert (endIter, insertText);
		}

		private void PumpMessages()
		{
			while(Application.EventsPending())
				Application.RunIteration(false);
		}
	}
}
