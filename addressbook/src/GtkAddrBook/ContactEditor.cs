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
using Novell.AddressBook;

using Gtk;
using Gdk;
using Gnome;
using Glade;
using GtkSharp;
using GLib;

namespace Novell.iFolder
{

	public class ContactEditEventArgs : EventArgs
	{
		private Contact c;
		private bool isNewContact;

		//Constructor.
		//
		public ContactEditEventArgs(Contact con, bool newContact)
		{
			this.c = con;
			this.isNewContact = newContact;
		}

		public bool isNew
		{
			get { return isNewContact;}      
		}

		public Contact ABContact 
		{     
			get { return c;}      
		}
	}

	// Delegate declaration
	//
	public delegate void ContactEditEventHandler(object sender, ContactEditEventArgs e);

	public class ContactEditor
	{
		[Glade.Widget] Gtk.Entry ceUserName;
		[Glade.Widget] Gtk.Entry ceFirstName;
		[Glade.Widget] Gtk.Entry ceLastName;
		[Glade.Widget] Gtk.Entry ceEmail;
		[Glade.Widget] Gtk.Button ceChangeImage;
		[Glade.Widget] Gtk.Image ceImage;
		[Glade.Widget] Gtk.Table ceGeneralTabTable;

		Gtk.Window 		cewin;
		Contact			contact;
		bool			isNew;

		public event ContactEditEventHandler ContactEdited;

		public ContactEditor (Gtk.Window parentwin, Contact con, bool newContact) 
		{
			contact = con;
			isNew = newContact;

			InitGlade();

			cewin.TransientFor = parentwin;

			if(isNew)
				ceChangeImage.Sensitive = false;
			
			if(!newContact)
			{
				try
				{
					Name prefName = con.GetPreferredName();

					ceFirstName.Text = prefName.Given; 
					ceLastName.Text = prefName.Family;
					ceEmail.Text = con.EMail;
				}
				catch(Exception e)
				{}
				ceUserName.Text = con.UserName;
			}
		}

		protected void InitGlade()
		{
			Glade.XML gxml = new Glade.XML ("addressbook.glade", 
					"abContact", null);
			gxml.Autoconnect (this);

			cewin = (Gtk.Window) gxml.GetWidget("abContact");

			//Gtk.Image tmpImg = (Gtk.Image) gxml.GetWidget("image4");
			GLib.List tl = new GLib.List((IntPtr) 0, typeof(Gtk.Widget));

			tl.Append(ceUserName.Handle);
			tl.Append(ceFirstName.Handle);
			tl.Append(ceLastName.Handle);
			tl.Append(ceEmail.Handle);

			ceGeneralTabTable.SetFocusChain(tl);
		
			Pixbuf pb = GetScaledPhoto(contact, 64);
			if(pb != null)
				ceImage.FromPixbuf = pb;
		}

		protected virtual void OnContactEdit(ContactEditEventArgs e)
		{
			if(ContactEdited != null)
			{
				ContactEdited(this, e);
			}
		}

		public void ShowAll()
		{
			if(cewin != null)
				cewin.ShowAll();
		}

		public void onSave(object o, EventArgs args)
		{
			if( (ceUserName.Text.Length > 0) &&
					(ceFirstName.Text.Length > 0) &&
					(ceLastName.Text.Length > 0) &&
					(ceEmail.Text.Length > 0) )
			{
				contact.UserName = ceUserName.Text;
/*
				if(isNew)
				{
					contact.CreateName(ceFirstName.Text, ceLastName.Text, true);
				}
				else
				{
*/
					try
					{
						Name prefName = contact.GetPreferredName();
						prefName.Family = ceLastName.Text;
						prefName.Given = ceFirstName.Text;
//						prefName.Commit();
					}
					catch(Exception except)
					{
						// probably didn't have a prefName
						// Try creating one and getting it
						Name prefName = new 
								Name(ceFirstName.Text, ceLastName.Text);
						prefName.Preferred = true;
						contact.AddName(prefName);
					}
//				}

				contact.EMail = ceEmail.Text;
				ContactEditEventArgs e = new ContactEditEventArgs(contact, 
						isNew);
				OnContactEdit(e);
				cewin.Hide();
				cewin.Destroy();
				cewin = null;
			}
		}

		public void onCancel(object o, EventArgs args) 
		{
			cewin.Hide();
			cewin.Destroy();
			cewin = null;
		}

		public void on_changeImage_clicked(object o, EventArgs args) 
		{
			FileSelection fs = new FileSelection("Choose a new Image");
			int retVal = fs.Run();
			Console.WriteLine("Result was : " + retVal);
			fs.Hide();
			if(retVal == -5)
			{
				int newWidth, newHeight;

				Console.WriteLine("Selected File: " + fs.Filename);
				if(contact.ImportPhoto(fs.Filename))
				{
					Pixbuf pb = GetScaledPhoto(contact, 64);
					if(pb != null)
						ceImage.FromPixbuf = pb;
				}
			}
		}

		public void onKeyPressed(object o, KeyPressEventArgs args)
		{
			switch(args.Event.hardware_keycode)
			{
				case 9:
					onCancel(o, args);
					break;
				case 36:
					onSave(o, args);
					break;					
			}
		}

		private Pixbuf GetScaledPhoto(Contact c, int height)
		{
			Pixbuf pb = null;
			
			try
			{
				int newWidth, newHeight;

				pb = new Pixbuf(c.ExportPhoto());

				newHeight = height;
				newWidth = height;

				if(pb.Height != pb.Width)
				{
					int perc = (height * 1000) / pb.Height;
					newWidth = pb.Width * perc / 1000;
				}

				pb = pb.ScaleSimple(newWidth, newHeight, 
						InterpType.Bilinear);
			}
			catch(Exception e)
			{
				pb = null;
			}
			return pb;
		}
	}
}
