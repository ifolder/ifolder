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
using System.Text;
using Novell.AddressBook;

using Gtk;
using Gdk;
using Gnome;
using Glade;
using GtkSharp;
using GLib;

namespace Novell.iFolder
{
	public class ContactEventArgs : EventArgs
	{
		private Contact contact;

		public ContactEventArgs(Contact contact)
		{
			this.contact = contact;
		}

		public Contact ABContact 
		{     
			get { return contact;}      
		}
	}

	// Delegate declaration
	public delegate void ContactEditedEventHandler(object sender, 
			ContactEventArgs args);

	public delegate void ContactCreatedEventHandler(object sender, 
			ContactEventArgs args);

	public class ContactEditor
	{
		// Glade "autoconnected" members
		[Glade.Widget] protected Gtk.Image			userImage;
		[Glade.Widget] protected Gtk.Entry			fullNameEntry;
		[Glade.Widget] protected Gtk.Entry			jobTitleEntry;
		[Glade.Widget] protected Gtk.Entry			organizationEntry;
		[Glade.Widget] protected Gtk.Entry			userIDEntry;

		[Glade.Widget] protected Gtk.Entry			phoneOneEntry;
		[Glade.Widget] protected Gtk.Entry			phoneTwoEntry;
		[Glade.Widget] protected Gtk.Entry			phoneFourEntry;
		[Glade.Widget] protected Gtk.Label			phoneOneLabel;
		[Glade.Widget] protected Gtk.Label			phoneTwoLabel;
		[Glade.Widget] protected Gtk.Label			phoneThreeLabel;
		[Glade.Widget] protected Gtk.Label			phoneFourLabel;

		[Glade.Widget] protected Gtk.Entry			emailEntry;
		[Glade.Widget] protected Gtk.Label			emailLabel;
		[Glade.Widget] protected Gtk.CheckButton	emailHTMLButton;

		[Glade.Widget] protected Gtk.Entry			webURLEntry;
		[Glade.Widget] protected Gtk.Button			webURLButton;
		[Glade.Widget] protected Gtk.Entry			blogURLEntry;
		[Glade.Widget] protected Gtk.Button			blogURLButton;

		[Glade.Widget] protected Gtk.Label			addrLabel;
		[Glade.Widget] protected Gtk.TextView		addrTextView;
		[Glade.Widget] protected Gtk.Button			addrChangeButton;
		[Glade.Widget] protected Gtk.CheckButton	addrMailingButton;

		[Glade.Widget] protected Gtk.Button			cancelButton;
		[Glade.Widget] protected Gtk.Button			okButton; 

		[Glade.Widget] protected Gtk.Table			generalTabTable;


		private Gtk.Dialog 		contactEditorDialog;
		private Contact			currentContact;
		private Name			preferredName;
		private bool			isNewContact;

		public event ContactEditedEventHandler	ContactEdited;
		public event ContactCreatedEventHandler	ContactCreated;




		public ContactEditor (Gtk.Window parentwin, Contact contact)
		{
			if(contact == null)
			{
				currentContact = new Contact();
				isNewContact = true;
			}
			else
			{
				currentContact = contact;
				isNewContact = false;
			}

			Init();

			contactEditorDialog.TransientFor = parentwin;
		}




		public ContactEditor (Gtk.Window parentwin) :
			this(parentwin, null)
		{
		}




		private void Init()
		{
			Glade.XML gxml = new Glade.XML ("contact-editor.glade", 
					"contactEditor", null);

			gxml.Autoconnect (this);

			contactEditorDialog = (Gtk.Dialog) gxml.GetWidget("contactEditor");

/*			GLib.List tl = new GLib.List((IntPtr) 0, typeof(Gtk.Widget));

			tl.Append(ceUserName.Handle);
			tl.Append(ceFirstName.Handle);
			tl.Append(ceLastName.Handle);
			tl.Append(ceEmail.Handle);

			generalTabTable.FocusChain = (tl);
*/		
			try
			{
				preferredName = currentContact.GetPreferredName();
			}
			catch(Exception e)
			{
				preferredName = new Name("", "");
				preferredName.Preferred = true;
				currentContact.AddName(preferredName);
			}

			PopulateWidgets();
		}
		



		private void PopulateWidgets()
		{
			fullNameEntry.Text = preferredName.FN;

			Pixbuf pb = GetScaledPhoto(currentContact, 64);
			if(pb != null)
				userImage.FromPixbuf = pb;

			if(currentContact.UserName.Length > 0)
				userIDEntry.Text = currentContact.UserName;
			if(currentContact.Title.Length > 0)
				jobTitleEntry.Text = currentContact.Title;
			if(currentContact.Url.Length > 0)
				webURLEntry.Text = currentContact.Url;
		}

		public void ShowAll()
		{
			if(contactEditorDialog != null)
				contactEditorDialog.ShowAll();
		}




		private void NotifyContactEdit()
		{
			ContactEventArgs cArgs = new ContactEventArgs(currentContact);

			if(isNewContact && (ContactCreated != null) )
				ContactCreated(this, cArgs);
			else if(ContactEdited != null)
				ContactEdited(this, cArgs);
		}




		private void SaveContact()
		{
			if(userIDEntry.Text.Length > 0)
				currentContact.UserName = userIDEntry.Text;
			if(jobTitleEntry.Text.Length > 0)
				currentContact.Title = jobTitleEntry.Text;
			if(webURLEntry.Text.Length > 0)
				currentContact.Url = webURLEntry.Text;
		}



		/*
		   private Novell.AddressBook.Name ParseFullNameEntry(string name)
		   {
		   Novell.AddressBook.Name ABName = null;

		   if(name.Length > 0)
		   {
		   string curString;
		   int curIndex;
		   string lastName = null;

		   ArrayList nameList = new ArrayList();

		   curString = name;

		   curIndex = curString.IndexOf(',');
		   if(curIndex != -1)
		   {
		   lastName = curString.Substring(0, curIndex);
		   curString = curString.Substring(curIndex + 1).Trim();
		   }

		   curIndex = 0;

		   while(curIndex != -1)
		   {
		   string nameString;

		   curIndex = curString.IndexOf(' ');
		   if(curIndex != -1)
		   {
		   nameString = curString.Substring(0, curIndex);
		   curString = curString.Substring(curIndex + 1).Trim();
		   }
		   else
		   nameString = curString;

		   if(nameString.Length > 0)
		   nameList.Add(nameString);
		   }
		   if(lastName != null)
		   nameList.Add(lastName);

		   foreach(string s in nameList)
		   {
		   Console.WriteLine(s);
		   }

		   }
		   return ABName;
		   }
		 */




		private void CloseDialog()
		{
			contactEditorDialog.Hide();
			contactEditorDialog.Destroy();
			contactEditorDialog = null;
		}




		private void on_contactEditor_delete_event(object o,
				DeleteEventArgs args) 
		{
			CloseDialog();
		}




		private void on_cancelButton_clicked(object o, EventArgs args) 
		{
			CloseDialog();
		}




		private void on_okButton_clicked(object o, EventArgs args) 
		{
			SaveContact();
			NotifyContactEdit();
			CloseDialog();
		}




		private void on_fullNameButton_clicked(object o, EventArgs args) 
		{
			NameEditor ne = new NameEditor(contactEditorDialog, preferredName);
			if(ne.Run() == -5)
			{
				fullNameEntry.Text = preferredName.FN;
			}
		}




		private void on_changeImageButton_clicked(object o, EventArgs args) 
		{
			FileSelection fs = new FileSelection("Choose a new Image");

			// setup file selector to be modal to the ContactEditor
			fs.Modal = true;
			fs.TransientFor = contactEditorDialog;


			int retVal = fs.Run();

			fs.Hide();

			// if they selected a file, try to import it
			if(retVal == -5)
			{
				if(currentContact.ImportPhoto(fs.Filename))
				{
					Pixbuf pb = GetScaledPhoto(currentContact, 64);
					if(pb != null)
						userImage.FromPixbuf = pb;
				}
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
