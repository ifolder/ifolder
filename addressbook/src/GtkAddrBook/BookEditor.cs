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
using System.Drawing;

using Gtk;
using Gdk;
using Gnome;
using Glade;
using GtkSharp;
using GLib;

namespace Novell.iFolder
{
	public class BookEditEventArgs : EventArgs
	{
		private readonly string newBookName;
		private readonly string oldBookName;
		private readonly bool changed;

		//Constructor.
		//
		public BookEditEventArgs(bool changed, string newName)
		{
			this.newBookName = newName;
			this.changed = changed;
		}

		public string NewName
		{     
			get { return newBookName;}      
		}

		public bool Changed
		{
			get {return changed;}
		}

		// The AlarmText property that contains the wake-up message.
		//
/*
		public string AlarmText 
		{
			get 
			{
				if (snoozePressed)
				{
					return ("Wake Up!!! Snooze time is over.");
				}
				else 
				{
					return ("Wake Up!");
				}
			}
		}  
*/
	}

	// Delegate declaration
	//
	public delegate void BookEditEventHandler(object sender, BookEditEventArgs e);

	public class BookEditor
	{
		[Glade.Widget] Gtk.Entry beName;

		Gtk.Window 		bewin = null;
		public event BookEditEventHandler BookEdited;

		public BookEditor () 
		{
			Glade.XML gxml = new Glade.XML ("addressbook.glade", "abBook", null);
			gxml.Autoconnect (this);

			bewin = (Gtk.Window) gxml.GetWidget("abBook");
		}

		protected virtual void OnBookEdit(BookEditEventArgs e)
		{
			if(BookEdited != null)
			{
				BookEdited(this, e);
			}
		}

		public void ShowAll()
		{
			if(bewin != null)
			{
				bewin.ShowAll();
			}
		}

		public void on_save(object o, EventArgs args)
		{
			BookEditEventArgs e = new BookEditEventArgs(true, beName.Text);
			OnBookEdit(e);
			bewin.Hide();
			bewin.Destroy();
			bewin = null;
		}

		public void on_cancel(object o, EventArgs args)
		{
			bewin.Hide();
			bewin.Destroy();
			bewin = null;
		}

		public void onKeyPressed(object o, KeyPressEventArgs args)
		{
			switch(args.Event.hardware_keycode)
			{
				case 9:
					on_cancel(o, args);
					break;
				case 36:
					on_save(o, args);
					break;					
			}
		}
	}
}
