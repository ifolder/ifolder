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
using System.Collections;
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

		[Glade.Widget] internal Gtk.Entry			workPhoneEntry;
		[Glade.Widget] internal Gtk.Entry			mobilePhoneEntry;
		[Glade.Widget] internal Gtk.Entry			homePhoneEntry;

		[Glade.Widget] internal Gtk.Button			phoneButton;

		[Glade.Widget] internal Gtk.Entry			workEmailEntry;
		[Glade.Widget] internal Gtk.Entry			personalEmailEntry;
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
		internal bool			isNewContact;

		internal Name			preferredName;
		internal Email			workEmail;
		internal Email			personalEmail;
		internal Telephone		workPhone;
		internal Telephone		mobilePhone;
		internal Telephone		homePhone;
		internal Address		preferredAddress;

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




		/// <summary>
		/// Method used to load the glade resources and setup default
		/// behaviors in for the ContactEditor dialog
		/// </summary>
		private void Init()
		{
			Glade.XML gxml = new Glade.XML ("contact-editor.glade", 
					"contactEditor", null);

			gxml.Autoconnect (this);

			contactEditorDialog = (Gtk.Dialog) gxml.GetWidget("contactEditor");
	

			Widget[] widArray = new Widget[16];

			widArray[0] = fullNameEntry;

			widArray[1] = jobTitleEntry;
			widArray[2] = organizationEntry;
			widArray[3] = userIDEntry;

			widArray[4] = workEmailEntry;
			widArray[5] = personalEmailEntry;
			widArray[6] = webURLEntry;
			widArray[7] = blogURLEntry;

			widArray[8] = workPhoneEntry;
			widArray[9] = mobilePhoneEntry;
			widArray[10] = homePhoneEntry;

			widArray[11] = streetEntry;
			widArray[12] = cityEntry;
			widArray[13] = stateEntry;
			widArray[14] = zipEntry;
			widArray[15] = countryEntry;

			generalTabTable.FocusChain = widArray;

			fullNameEntry.HasFocus = true;

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
	



		/// <summary>
		/// Method to notify show the dialog
		/// </summary>
		public void ShowAll()
		{
			if(contactEditorDialog != null)
			{
				contactEditorDialog.Visible = true;
				contactEditorDialog.Present();
			}
		}




		/// <summary>
		/// Method to notify delegates that the edit was done on this
		/// contact
		/// </summary>
		private void NotifyContactEdit()
		{
			ContactEventArgs cArgs = new ContactEventArgs(currentContact);

			if(isNewContact && (ContactCreated != null) )
				ContactCreated(this, cArgs);
			else if(ContactEdited != null)
				ContactEdited(this, cArgs);
		}




		/// <summary>
		/// Method used to populate all of the data from the current
		/// contact into the UI controls
		/// </summary>
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

			PopulateEmails();
			PopulatePhoneNumbers();

		}




		/// <summary>
		/// Method used to retrieve a specific type of phone number
		/// from the current contact
		/// </summary>
		internal Telephone GetPhoneType(PhoneTypes type)
		{
			foreach(Telephone tel in currentContact.GetTelephoneNumbers())
			{
				if( (tel.Types & type) == type)
					return tel;
			}
			return null;
		}




		/// <summary>
		/// Method used to populate the phone controls in the dialog
		/// with the data from the currentContact
		/// </summary>
		internal void PopulatePhoneNumbers()
		{
			workPhone = null;
			mobilePhone = null;
			homePhone = null;

			workPhone = GetPhoneType(PhoneTypes.preferred);
			if(workPhone != null)
			{
				// Check to see if the default is work 
				if( (workPhone.Types & PhoneTypes.work) == PhoneTypes.work)
				{
					workPhoneEntry.Text = workPhone.Number;
				}
				// Check to see if the default is mobile
				else if( (workPhone.Types & PhoneTypes.cell) == 
						PhoneTypes.cell)
				{
					mobilePhone = workPhone;
					workPhone = null;
					mobilePhoneEntry.Text = mobilePhone.Number;
				}
				// Check to see if the default is home
				else if( (homePhone.Types & PhoneTypes.home) == 
						PhoneTypes.home)
				{
					homePhone = workPhone;
					workPhone = null;
					homePhoneEntry.Text = homePhone.Number;
				}
				// The default ie niether work, mobile or home, reset
				else
				{
					workPhone = null;
				}
			}

			// if it turns out that the default above was not work
			// read it now
			if(workPhone == null)
			{
				workPhone = GetPhoneType(PhoneTypes.work);
				if(workPhone != null)
					workPhoneEntry.Text = workPhone.Number;
				else
					workPhoneEntry.Text = "";
			}

			// if it turns out that the default above was not mobile 
			// read it now
			if(mobilePhone == null)
			{
				mobilePhone = GetPhoneType(PhoneTypes.cell);
				if(mobilePhone != null)
					mobilePhoneEntry.Text = mobilePhone.Number;
				else
					mobilePhoneEntry.Text = "";
			}

			// if it turns out that the default above was not home
			// read it now
			if(homePhone == null)
			{
				homePhone = GetPhoneType(PhoneTypes.home);
				if(homePhone != null)
					homePhoneEntry.Text = homePhone.Number;
				else
					homePhoneEntry.Text = "";
			}
		}




		/// <summary>
		/// Method used to save phone numbers to the current contact
		/// the current contact still needs to be persisted
		/// </summary>
		internal void SavePhoneNumbers()
		{
			// --------------------------------
			// Work Phone
			// --------------------------------
			if(workPhoneEntry.Text.Length > 0)
			{
				if(workPhone == null)
				{
					workPhone = new Telephone(workPhoneEntry.Text,
							PhoneTypes.work);
					currentContact.AddTelephoneNumber(workPhone);
				}
				else
				{
					workPhone.Number = workPhoneEntry.Text;
				}
			}
			// If there is no text, check to see if we need to delete
			else
			{
				if(workPhone != null)
				{
					workPhone.Delete();
				}
			}


			// --------------------------------
			// Mobile Phone
			// --------------------------------
			if(mobilePhoneEntry.Text.Length > 0)
			{
				if(mobilePhone == null)
				{
					mobilePhone = new Telephone(mobilePhoneEntry.Text,
							PhoneTypes.cell);
					currentContact.AddTelephoneNumber(mobilePhone);
				}
				else
				{
					mobilePhone.Number = mobilePhoneEntry.Text;
				}
			}
			// If there is no text, check to see if we need to delete
			else
			{
				if(mobilePhone != null)
				{
					mobilePhone.Delete();
				}
			}


			// --------------------------------
			// Home Phone
			// --------------------------------
			if(homePhoneEntry.Text.Length > 0)
			{
				if(homePhone == null)
				{
					homePhone = new Telephone(homePhoneEntry.Text,
							PhoneTypes.home);
					currentContact.AddTelephoneNumber(homePhone);
				}
				else
				{
					homePhone.Number = homePhoneEntry.Text;
				}
			}
			// If there is no text, check to see if we need to delete
			else
			{
				if(homePhone != null)
				{
					homePhone.Delete();
				}
			}
		}




		/// <summary>
		/// Method used to populate the email controls in the dialog
		/// with the data from the currentContact
		/// </summary>
		internal void PopulateEmails()
		{
			workEmail = null;
			personalEmail = null;

			workEmail = GetEmailType(EmailTypes.preferred);
			if(workEmail != null)
			{
				// Check to see if the default is work 
				if( (workEmail.Types & EmailTypes.work) == EmailTypes.work)
				{
					workEmailEntry.Text = workEmail.Address;
				}
				// Check to see if the default is personal
				else if( (workEmail.Types & EmailTypes.personal) == 
						EmailTypes.personal)
				{
					personalEmail = workEmail;
					workEmail = null;
					personalEmailEntry.Text = personalEmail.Address;
				}
				// The default ie niether work nor personal, reset workEmail
				else
				{
					workEmail = null;
				}
			}

			// if it turns out that the default above was not work
			// read it now
			if(workEmail == null)
			{
				workEmail = GetEmailType(EmailTypes.work);
				if(workEmail != null)
					workEmailEntry.Text = workEmail.Address;
				else
					workEmailEntry.Text = "";
			}

			// if it turns out that the default above was not personal 
			// read it now
			if(personalEmail == null)
			{
				personalEmail = GetEmailType(EmailTypes.personal);
				if(personalEmail != null)
					personalEmailEntry.Text = personalEmail.Address;
				else
					personalEmailEntry.Text = "";
			}
		}




		/// <summary>
		/// Method used to retrieve a specific type of email from
		/// the current contact
		/// </summary>
		internal Email GetEmailType(EmailTypes type)
		{
			foreach(Email e in currentContact.GetEmailAddresses())
			{
				if( (e.Types & type) == type)
					return e;
			}
			return null;
		}




		/// <summary>
		/// Method used to store the edited data to the currentContact
		/// the contact must still be committed to persist the data
		/// in the store
		/// </summary>
		internal void SaveCurrentEmails()
		{
			// --------------------------------
			// Work Email
			// --------------------------------
			if(workEmailEntry.Text.Length > 0)
			{
				if(workEmail == null)
				{
					workEmail = new Email(EmailTypes.work,
							workEmailEntry.Text);
					currentContact.AddEmailAddress(workEmail);
				}
				else
				{
					workEmail.Address = workEmailEntry.Text;
				}
			}
			// If there is no text, check to see if we need to delete
			else
			{
				if(workEmail != null)
				{
					workEmail.Delete();
				}
			}

			// --------------------------------
			// Personal Email
			// --------------------------------
			if(personalEmailEntry.Text.Length > 0)
			{
				if(personalEmail == null)
				{
					personalEmail = new Email(EmailTypes.personal,
							personalEmailEntry.Text);
					currentContact.AddEmailAddress(personalEmail);
				}
				else
				{
					personalEmail.Address = personalEmailEntry.Text;
				}
			}
			// If there is no text, check to see if we need to delete
			else
			{
				if(personalEmail != null)
				{
					personalEmail.Delete();
				}
			}
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
			SaveCurrentEmails();
			SavePhoneNumbers();
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
