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
		[Glade.Widget] internal Gtk.Entry			phoneThreeEntry;

		[Glade.Widget] internal Gtk.Label			phoneOneLabel;
		[Glade.Widget] internal Gtk.Label			phoneTwoLabel;
		[Glade.Widget] internal Gtk.Label			phoneThreeLabel;
		
		[Glade.Widget] internal Gtk.Button			phoneButton;

		[Glade.Widget] internal Gtk.Entry			emailOneEntry;
		[Glade.Widget] internal Gtk.Label			emailOneLabel;
		[Glade.Widget] internal Gtk.Entry			emailTwoEntry;
		[Glade.Widget] internal Gtk.Label			emailTwoLabel;
		[Glade.Widget] internal Gtk.Button			emailButton;

		[Glade.Widget] internal Gtk.Entry			webURLEntry;
		[Glade.Widget] internal Gtk.Entry			blogURLEntry;

		[Glade.Widget] internal Gtk.Entry			streetEntry;
		[Glade.Widget] internal Gtk.Entry			cityEntry;
		[Glade.Widget] internal Gtk.Entry			stateEntry;
		[Glade.Widget] internal Gtk.Entry			zipEntry;
		[Glade.Widget] internal Gtk.Entry			countryEntry;
		[Glade.Widget] internal Gtk.Button			addrButton;

		[Glade.Widget] internal Gtk.Button			cancelButton;
		[Glade.Widget] internal Gtk.Button			okButton; 

		[Glade.Widget] internal Gtk.Table			generalTabTable;


		internal Gtk.Dialog		contactEditorDialog;
		internal Contact		currentContact;
		internal Name			preferredName;
		internal bool			isNewContact;

		internal Email			currentEmail;
		internal Telephone		phoneOne;
		internal Telephone		phoneTwo;
		internal Telephone		phoneThree;
		internal Address		currentAddress;

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

			if(currentContact.Url.Length > 0)
				webURLEntry.Text = currentContact.Url;
			if(currentContact.Blog.Length > 0)
				blogURLEntry.Text = currentContact.Blog;

			PopulateCurrentEmail();
			PopulatePhoneNumbers();

		}


		internal Telephone GetPhoneType(PhoneTypes type)
		{
			foreach(Telephone tel in currentContact.GetTelephoneNumbers())
			{
				if( (tel.Types & type) == type)
					return tel;
			}
			return null;
		}

		internal void PopulatePhoneNumbers()
		{
			phoneOne = GetPhoneType(PhoneTypes.preferred);
			if(phoneOne != null)
			{
				phoneOneEntry.Text = phoneOne.Number;
				SetPhoneLabelText(phoneOneLabel, phoneOne.Types);
			}

			foreach(Telephone tel in currentContact.GetTelephoneNumbers())
			{
				if(phoneOne == null)
				{
					phoneOne = tel;
					phoneOneEntry.Text = tel.Number;
					SetPhoneLabelText(phoneOneLabel, tel.Types);
				}
				else if(phoneOne == tel)
				{
					// do nothing
				}
				else if(phoneTwo == null)
				{
					phoneTwo = tel;
					phoneTwoEntry.Text = tel.Number;
					SetPhoneLabelText(phoneTwoLabel, tel.Types);
				}
				else if(phoneThree == null)
				{
					phoneThree = tel;
					phoneThreeEntry.Text = tel.Number;
					SetPhoneLabelText(phoneThreeLabel, tel.Types);
				}
			}
		}

		internal void SetPhoneLabelText(Label label, PhoneTypes type)
		{
			if( (type & PhoneTypes.work) == PhoneTypes.work)
				label.Text = "Work Phone:";
			else if( (type & PhoneTypes.cell) == PhoneTypes.cell)
				label.Text = "Mobile Phone:";
			else if( (type & PhoneTypes.home) == PhoneTypes.home)
				label.Text = "Home Phone:";
			else if( (type & PhoneTypes.pager) == PhoneTypes.pager)
				label.Text = "Pager Phone:";
			else if( (type & PhoneTypes.fax) == PhoneTypes.fax)
				label.Text = "Fax Phone:";
		}


		internal void PopulateCurrentEmail()
		{
			currentEmail = GetEmailType(EmailTypes.preferred);
			if(currentEmail == null)
				currentEmail = GetEmailType(EmailTypes.work);
			if(currentEmail == null)
				currentEmail = GetEmailType(EmailTypes.personal);
			if(currentEmail == null)
				currentEmail = GetEmailType(EmailTypes.other);

			if(currentEmail != null)
			{
				emailOneEntry.Text = currentEmail.Address;

				if( (currentEmail.Types & EmailTypes.work) ==
						EmailTypes.work)
					emailOneLabel.Text = "Work Email:";
				else if( (currentEmail.Types & EmailTypes.personal) == 
						EmailTypes.personal)
					emailOneLabel.Text = "Home Email:";
				else
					emailOneLabel.Text = "Other Email:";
			}
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

		internal void SaveCurrentEmail()
		{
			if(currentEmail == null)
			{
				if(emailOneEntry.Text.Length > 0)
				{
					currentEmail = new Email(EmailTypes.preferred,
							emailOneEntry.Text);
					currentContact.AddEmailAddress(currentEmail);
					SetCurrentDefaultEmail();
				}
			}
			else
			{
				currentEmail.Address = emailOneEntry.Text;
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
			{
				contactEditorDialog.Visible = true;
				contactEditorDialog.Present();
			}
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


		private void on_emailButton_clicked(object o, EventArgs args) 
		{
			Console.WriteLine("Edit Email here");
		}

		private void on_phoneButton_clicked(object o, EventArgs args) 
		{
			Console.WriteLine("Edit Phone here");
		}

		private void on_addrChangeButton_clicked(object o, EventArgs args) 
		{
			Console.WriteLine("Edit Address here");
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


		private void handle_phone_one_options(object o,
				OptionChangedEventArgs args)
		{
			switch(args.OptionIndex)
			{
				case 0: // Work phone 
					break;
				case 1: // Mobile phone
					break;
				case 2: // Home phone
					break;
				case 3: // Pager
					break;
				case 4: // Fax
					break;
			}
		}


/*
		private void handle_email_options(object o, OptionChangedEventArgs args)
		{
			SaveCurrentEmail();
			switch(args.OptionIndex)
			{
				case 0: // Work email
					currentEmailType = EmailTypes.work;
					currentEmail = GetEmailType(currentEmailType);
					break;
				case 1: // Home email
					currentEmailType = EmailTypes.personal;
					currentEmail = GetEmailType(currentEmailType);
					break;
				case 2: // Other email
					currentEmailType = EmailTypes.other;
					currentEmail = GetEmailType(currentEmailType);
					break;
			}

			PopulateCurrentEmail();
		}
*/

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
