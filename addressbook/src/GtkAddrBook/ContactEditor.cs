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
		[Glade.Widget] internal Gtk.Image			userImage;
		[Glade.Widget] internal Gtk.Entry			fullNameEntry;
		[Glade.Widget] internal Gtk.Entry			jobTitleEntry;
		[Glade.Widget] internal Gtk.Entry			organizationEntry;
		[Glade.Widget] internal Gtk.Entry			userIDEntry;

		[Glade.Widget] internal Gtk.Entry			phoneOneEntry;
		[Glade.Widget] internal Gtk.Entry			phoneTwoEntry;
		[Glade.Widget] internal Gtk.Entry			phoneFourEntry;
		[Glade.Widget] internal Gtk.Label			phoneOneLabel;
		[Glade.Widget] internal Gtk.Label			phoneTwoLabel;
		[Glade.Widget] internal Gtk.Label			phoneThreeLabel;
		[Glade.Widget] internal Gtk.Label			phoneFourLabel;

		[Glade.Widget] internal Gtk.Entry			emailEntry;
		[Glade.Widget] internal Gtk.Label			emailLabel;
		[Glade.Widget] internal Gtk.Button			emailButton;
		[Glade.Widget] internal Gtk.CheckButton		emailPreferredButton;

		[Glade.Widget] internal Gtk.Entry			webURLEntry;
		[Glade.Widget] internal Gtk.Entry			blogURLEntry;

		[Glade.Widget] internal Gtk.Label			addrLabel;
		[Glade.Widget] internal Gtk.TextView		addrTextView;
		[Glade.Widget] internal Gtk.Button			addrChangeButton;
		[Glade.Widget] internal Gtk.CheckButton	addrMailingButton;

		[Glade.Widget] internal Gtk.Button			cancelButton;
		[Glade.Widget] internal Gtk.Button			okButton; 

		[Glade.Widget] internal Gtk.Table			generalTabTable;


		private Gtk.Dialog 		contactEditorDialog;
		private Contact			currentContact;
		private Name			preferredName;
		private bool			isNewContact;
		internal Email			currentEmail;
		internal EmailTypes		currentEmailType;


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
			if(currentContact.Organization.Length > 0)
				organizationEntry.Text = currentContact.Organization;

			currentEmail = GetEmailType(EmailTypes.preferred);
			if(currentEmail == null)
				currentEmail = GetEmailType(EmailTypes.work);
			if(currentEmail == null)
				currentEmail = GetEmailType(EmailTypes.personal);
			if(currentEmail == null)
				currentEmail = GetEmailType(EmailTypes.other);

			PopulateCurrentEmail();

			if(currentContact.Url.Length > 0)
				webURLEntry.Text = currentContact.Url;
			if(currentContact.Blog.Length > 0)
				blogURLEntry.Text = currentContact.Blog;
		}



		internal Email GetEmailType(EmailTypes type)
		{
			foreach(Email e in currentContact.GetEmailAddresses())
			{
				if( (e.Types & type) == type)
					return e;
			}
			return null;
		}

		internal void PopulateCurrentEmail()
		{
			if(currentEmail != null)
			{
				emailEntry.Text = currentEmail.Address;
				currentEmailType = currentEmail.Types;
			}
			else
			{
				emailEntry.Text = "";
				if(currentEmailType == 0)
					currentEmailType = EmailTypes.work | EmailTypes.preferred;
			}

			if( (currentEmailType & EmailTypes.work) == EmailTypes.work)
				emailLabel.Text = "Work email:";
			else if( (currentEmailType & EmailTypes.personal) == 
					EmailTypes.personal)
				emailLabel.Text = "Home email:";
			else
				emailLabel.Text = "Other email:";

			if( (currentEmailType & EmailTypes.preferred) == 
					EmailTypes.preferred)
				emailPreferredButton.Active = true;
			else
				emailPreferredButton.Active = false;
		}

		internal void SaveCurrentEmail()
		{
			if(currentEmail == null)
			{
				if(emailEntry.Text.Length > 0)
				{
					currentEmail = new Email(currentEmailType, emailEntry.Text);
					currentContact.AddEmailAddress(currentEmail);
					if(emailPreferredButton.Active)
						SetCurrentDefaultEmail();
				}
			}
			else
			{
				currentEmail.Address = emailEntry.Text;
					if(emailPreferredButton.Active)
						SetCurrentDefaultEmail();
			}
		}

		internal void SetCurrentDefaultEmail()
		{
			foreach(Email e in currentContact.GetEmailAddresses())
			{
				e.Types &= ~EmailTypes.preferred;
			}
			currentEmail.Types |= EmailTypes.preferred;
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




		/// <summary>
		/// Method used to gather data from entry fields that are not tied
		/// in any way to the actual Contact object.
		/// </summary>
		private void SaveContact()
		{
			// UserID
			if(userIDEntry.Text.Length > 0)
				currentContact.UserName = userIDEntry.Text;
			else
				currentContact.UserName = null;
			// Job Title
			if(jobTitleEntry.Text.Length > 0)
				currentContact.Title = jobTitleEntry.Text;
			else
				currentContact.Title = null;
			// Organization
			if(organizationEntry.Text.Length > 0)
				currentContact.Organization = organizationEntry.Text;
			else
				currentContact.Organization = null;
			// Web URL
			if(webURLEntry.Text.Length > 0)
				currentContact.Url = webURLEntry.Text;
			else
				currentContact.Url = null;
			// Blog URL
			if(blogURLEntry.Text.Length > 0)
				currentContact.Blog = blogURLEntry.Text;
			else
				currentContact.Blog = null;
			// Email
			SaveCurrentEmail();
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




		private void on_emailButton_clicked(object o, EventArgs args) 
		{
			Menu mailMenu = new Menu();

			MenuItem work_item = new MenuItem ("Work email");
			mailMenu.Append (work_item);
			work_item.Activated += 
				new EventHandler(handle_work_email);

			MenuItem home_item = new MenuItem ("Home email");
			mailMenu.Append (home_item);
			home_item.Activated += 
				new EventHandler(handle_home_email);

			MenuItem other_item = new MenuItem ("Other email");
			mailMenu.Append (other_item);
			other_item.Activated += 
				new EventHandler(handle_other_email);

			mailMenu.ShowAll();

			mailMenu.Popup(null, null, new MenuPositionFunc(PositionMailMenu),
					IntPtr.Zero, 0, Gtk.Global.CurrentEventTime);
		}

		internal void PositionMailMenu(Menu menu, out int x, out int y,
				out bool push_in)
		{
			emailButton.GdkWindow.GetOrigin(out x, out y);
			x += emailButton.Allocation.X;
			y += emailButton.Allocation.Y;

			push_in = false;
		}

		private void handle_work_email(object o, EventArgs args)
		{
			SaveCurrentEmail();
			currentEmailType = EmailTypes.work;
			currentEmail = GetEmailType(currentEmailType);
			PopulateCurrentEmail();
		}

		private void handle_home_email(object o, EventArgs args)
		{
			SaveCurrentEmail();
			currentEmailType = EmailTypes.personal;
			currentEmail = GetEmailType(currentEmailType);
			PopulateCurrentEmail();
		}

		private void handle_other_email(object o, EventArgs args)
		{
			SaveCurrentEmail();
			currentEmailType = EmailTypes.other;
			currentEmail = GetEmailType(currentEmailType);
			PopulateCurrentEmail();
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
