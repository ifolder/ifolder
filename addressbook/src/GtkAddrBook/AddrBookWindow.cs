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
using System.IO;

using Gtk;
using Gdk;
using Glade;
using GtkSharp;
using GLib;

namespace Novell.iFolder
{
	public class ListData
	{
		private readonly Pixbuf pb;
		private readonly string name;

		public ListData(Pixbuf pb, string name)
		{
			this.pb = pb;
			this.name = name;
		}

		public Pixbuf Icon
		{     
			get { return pb;}      
		}

		public string Name
		{     
			get { return name;}      
		}
	}

	public class AddrBookWindow 
	{
		[Glade.Widget] internal Gtk.Window abMainWin;
		[Glade.Widget] internal TreeView	BookTreeView;
		[Glade.Widget] internal TreeView	ContactTreeView;
		[Glade.Widget] internal Button CreateContactButton;
		[Glade.Widget] internal Button ExportButton;
		[Glade.Widget] internal Entry SearchEntry;
		[Glade.Widget] internal Gtk.TextView PictureTextView;
		[Glade.Widget] internal Gtk.TextView LabelTextView;
		[Glade.Widget] internal Gtk.TextView ValueTextView;
		[Glade.Widget] internal Gtk.TextView TitleTextView;

		Manager abMan;
		AddressBook	curAddrBook;
		ListStore BookTreeStore;
		ListStore ContactTreeStore;
		//ListStore GroupTreeStore;
		Pixbuf	UserCardPixBuf;
		Pixbuf	CurCardPixBuf;
		Pixbuf	BookPixBuf;
		Pixbuf	BlankHeadPixBuf;
		ContactPicker cp;
		uint		searchTimeoutID;
		//Timer searchTimer;

		public event EventHandler AddrBookClosed;

		public AddrBookWindow (Manager abMan) 
		{
			this.abMan = abMan;
			Init();
		}

		public AddrBookWindow () 
		{
			Init();
		}

		public void Init () 
		{
			if(abMan == null)
			{
				try
				{
					abMan = Manager.Connect( );
				}
				catch(Exception e)
				{
					Console.WriteLine("Unable to connect to Addr Book: " + e);
				}
			}

			Glade.XML gxml = new Glade.XML ("addressbook.glade",
					"abMainWin", null);

			gxml.Autoconnect (this);


			// ****************************************
			// Book Tree View Setup
			// ****************************************
			BookTreeStore = new ListStore(typeof(AddressBook));
			BookTreeView.Model = BookTreeStore;

			CellRendererPixbuf bcrp = new CellRendererPixbuf();
			TreeViewColumn btvc = new TreeViewColumn();
			btvc.PackStart(bcrp, false);
			btvc.SetCellDataFunc(bcrp, new TreeCellDataFunc(
						BookCellPixbufDataFunc));

			CellRendererText bcrt = new CellRendererText();
			btvc.PackStart(bcrt, false);
			btvc.SetCellDataFunc(bcrt, new TreeCellDataFunc(
						BookCellTextDataFunc));
			btvc.Title = "Books";
			BookTreeView.AppendColumn(btvc);
			BookTreeView.Selection.Changed +=
				new EventHandler(on_book_selection_changed);

			// ****************************************
			// Contact Tree View Setup
			// ****************************************
			ContactTreeStore = new ListStore(typeof(Contact));
			ContactTreeView.Model = ContactTreeStore;
			CellRendererPixbuf ccrp = new CellRendererPixbuf();
			TreeViewColumn ctvc = new TreeViewColumn();
			ctvc.PackStart(ccrp, false);
			ctvc.SetCellDataFunc(ccrp, new TreeCellDataFunc(
						ContactCellPixbufDataFunc));

			CellRendererText ccrt = new CellRendererText();
			ctvc.PackStart(ccrt, false);
			ctvc.SetCellDataFunc(ccrt, new TreeCellDataFunc(
						ContactCellTextDataFunc));
			ctvc.Title = "Contacts";
			ContactTreeView.AppendColumn(ctvc);

			ContactTreeView.Selection.Changed += 
				new EventHandler(on_contact_selection_changed);

			AddLabelTags();
			AddTitleTags();

			if(abMan != null)
			{
				try
				{
					foreach(AddressBook ab in abMan)
					{
						BookTreeStore.AppendValues(ab);
					}

					curAddrBook = abMan.OpenDefaultAddressBook();

					foreach(Contact c in curAddrBook)
					{
						ContactTreeStore.AppendValues(c);
					}
				}
				catch(Exception e)
				{
					Console.WriteLine(
							"Unable to connect to the Address Book: " + e);
				}
			}

			UserCardPixBuf = new Pixbuf("contact.png");
			CurCardPixBuf = new Pixbuf("contact_me.png");
			BookPixBuf = new Pixbuf("book.png");
			BlankHeadPixBuf = new Pixbuf("blankhead.png");

			TreeSelection tSelect = ContactTreeView.Selection;
			tSelect.SelectPath(new TreePath("0"));

			searchTimeoutID = 0;
		}

		private bool SearchCallback()
		{
			SearchAddrBook();
			return false;
		}


		private void BookCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			AddressBook book = (AddressBook) BookTreeStore.GetValue(iter,0);
			if(book.Default)
				((CellRendererText) cell).Text = "Default";
			else
				((CellRendererText) cell).Text = book.Name;
		}

		private void BookCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			((CellRendererPixbuf) cell).Pixbuf = BookPixBuf;
		}

		private void ContactCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Contact cnt = (Contact) ContactTreeStore.GetValue(iter,0);
			if(cnt != null)
			{
				if( (cnt.FN != null) && (cnt.FN.Length > 0) )
					((CellRendererText) cell).Text = cnt.FN;
				else
					((CellRendererText) cell).Text = cnt.UserName;
			}
			else
			{
				((CellRendererText) cell).Text = "unknown contact";
			}
		}

		private void ContactCellPixbufDataFunc (
				Gtk.TreeViewColumn tree_column, Gtk.CellRenderer cell,
				Gtk.TreeModel tree_model, Gtk.TreeIter iter)
		{
			Contact cnt = (Contact) ContactTreeStore.GetValue(iter,0);

			if(cnt != null)
			{
				if(cnt.IsCurrentUser)
					((CellRendererPixbuf) cell).Pixbuf = CurCardPixBuf;
				else
					((CellRendererPixbuf) cell).Pixbuf = UserCardPixBuf;
			}
			else
			{
				((CellRendererPixbuf) cell).Pixbuf = UserCardPixBuf;
			}
		}

		public void ShowAll()
		{
			abMainWin.ShowAll();
		}

		public void on_abMainWin_delete (object o, DeleteEventArgs args) 
		{
			//			Application.Quit ();
			//			abMainWin = null;
			args.RetVal = true;
			on_quit(o, args);
		}

		/// <summary>
		/// Standard Event handler to respond to a change in the entry
		/// widget for the search field.
		/// </summary>
		private void SearchAddrBook()
		{
			ContactTreeStore.Clear();

			if(SearchEntry.Text.Length > 0)
			{
				Hashtable idHash = new Hashtable();

				// First Name Search
				IABList clist = curAddrBook.SearchFirstName(SearchEntry.Text,
						Simias.Storage.Property.Operator.Begins);
				foreach(Contact c in clist)
				{
					idHash.Add(c.ID, c);
				}

				// Last Name Search
				clist = curAddrBook.SearchLastName(SearchEntry.Text, 
						Simias.Storage.Property.Operator.Begins);
				foreach(Contact c in clist)
				{
					if(!idHash.Contains(c.ID))
						idHash.Add(c.ID, c);
				}

				// UserName Name Search
				clist = curAddrBook.SearchUsername(SearchEntry.Text, 
						Simias.Storage.Property.Operator.Begins);
				foreach(Contact c in clist)
				{
					if(!idHash.Contains(c.ID))
						idHash.Add(c.ID, c);
				}

				// Email Name Search
				clist = curAddrBook.SearchEmail(SearchEntry.Text, 
						Simias.Storage.Property.Operator.Begins);
				foreach(Contact c in clist)
				{
					if(!idHash.Contains(c.ID))
						idHash.Add(c.ID, c);
				}

				foreach(Contact c in idHash.Values)
				{
					ContactTreeStore.AppendValues(c);
				}
			}
			else
			{
				foreach(Contact c in curAddrBook)
				{
					ContactTreeStore.AppendValues(c);
				}
			}
			TreeSelection tSelect = ContactTreeView.Selection;
			tSelect.SelectPath(new TreePath("0"));

			//TreeSelection tSelect = ContactTreeView.Selection;
		}

		public void onCreateBook(object o, EventArgs args)
		{
			BookEditor be = new BookEditor();
			be.BookEdited += new BookEditEventHandler(CreateBookEventHandler);
			be.ShowAll();
		}

		public void onCreateContact(object o, EventArgs args)
		{
			if(CreateContactButton.Sensitive == true)
			{
				ContactEditor ce = new ContactEditor(abMainWin);

				ce.ContactCreated +=
					new ContactCreatedEventHandler(ContactCreatedEventHandler);

				ce.ShowAll();
			}
		}

		public void onImportVCard(object o, EventArgs args)
		{
			FileSelection fs = new FileSelection ("Select VCard to Import...");

			int rc = fs.Run ();
			fs.Hide ();
			if(rc == -5)
			{
				Contact			newContact = null;
				StreamReader	reader = null;
				bool hasmore = true;

				try
				{
					reader = new StreamReader(fs.Filename);
					if (reader != null)
					{
						while(hasmore)
						{
							newContact = curAddrBook.ImportVCard(reader);
							if(newContact != null)
							{
								ContactTreeStore.AppendValues(newContact);
							}
							try
							{
								String newline = reader.ReadLine();
								if(newline == null)
									hasmore = false;
							}
							catch(Exception e)
							{
								// we blew reading, let's quit
								hasmore = false;
							}
						}
					}
				}
				catch(Exception e)
				{
					Console.WriteLine(e);
				}
				finally
				{
					if (reader != null)
					{
						reader.Close();
					}
				}
			}
		}

		public void onExportVCard(object o, EventArgs args)
		{
			TreeSelection tSelect = ContactTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				if(tModel != null)
					tModel = null;
				Contact cnt = (Contact) ContactTreeStore.GetValue(iter,0);

				if(cnt != null)
				{
					string fileName = cnt.FN + ".vcf";

					FileSelection fs = new FileSelection ("Export VCard to...");

					fs.Filename = fileName;
					int rc = fs.Run ();
					fs.Hide ();
					if(rc == -5)
					{
						cnt.ExportVCard(fs.Filename);
					}
				}
			}
		}

		public void onCreateGroup(object o, EventArgs args)
		{
		}

		public void DeleteSelectedBooks()
		{	
			TreeSelection tSelect = ContactTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				if(tModel != null)
					tModel = null;
				AddressBook ab = (AddressBook) BookTreeStore.GetValue(iter,0);
				if(ab.Default)
				{
					MessageDialog med = new MessageDialog(abMainWin,
							DialogFlags.DestroyWithParent | DialogFlags.Modal,
							MessageType.Error,
							ButtonsType.Close,
							"Deleting the default address book ain't right and we ain't gonna let you do it.");
					med.Title = "This ain't right";
					med.Run();
					med.Hide();
					return;
				}

				BookTreeStore.Remove(out iter);

				try
				{
					ab.Delete();
				}
				catch(ApplicationException e)
				{
					Console.WriteLine(e);
				}

				ContactTreeStore.Clear();
			}
		}

		public void on_book_key_press(object o, KeyPressEventArgs args)
		{
			switch(args.Event.HardwareKeycode)
			{
				case 22:
				case 107:
					{
						TreeSelection tSelect = BookTreeView.Selection;
						if(tSelect.CountSelectedRows() > 0)
						{
							MessageDialog dialog = new MessageDialog(abMainWin,
									DialogFlags.Modal | 
											DialogFlags.DestroyWithParent,
									MessageType.Question,
									ButtonsType.YesNo,
									"Do you want to delete the selected Address Books?");

							dialog.Response += new ResponseHandler(
									DeleteBookResponse);
							dialog.Title = "Denali Delete Books";
							dialog.Show();
						}
						break;					
					}
			}
		}

		public void DeleteBookResponse(object sender, ResponseArgs args)
		{
			MessageDialog dialog = (MessageDialog) sender;

			switch((ResponseType)args.ResponseId)
			{
				case ResponseType.Yes:
					DeleteSelectedBooks();
					break;
				default:
					break;
			}
			dialog.Destroy();
		}

		private void on_contact_selection_changed(object o, EventArgs args)
		{
			TreeSelection tSelect = ContactTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;
				ExportButton.Sensitive = true;

				tSelect.GetSelected(out tModel, out iter);
				if(tModel != null)
					tModel = null;
				Contact c = (Contact) 
					ContactTreeStore.GetValue(iter,0);

				DisplayContactDetails(c);
			}
			else
			{
				ClearContactDetails();
				ExportButton.Sensitive = false;
			}
		}

		public void on_book_selection_changed(object o, EventArgs args)
		{
			TreeSelection tSelect = BookTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				if(tModel != null)
					tModel = null;
				curAddrBook = (AddressBook) BookTreeStore.GetValue(iter,0);

				CreateContactButton.Sensitive = true;
				ContactTreeStore.Clear();

				foreach(Contact cont in curAddrBook)
				{
					if(cont != null)
					{
						ContactTreeStore.AppendValues(cont);
					}
					else
						Console.WriteLine("We were retuned a NULL contact.");
				}
			}
		}

		public void ContactCreatedEventHandler(object o,
				ContactEventArgs args)
		{
			Contact contact = args.Contact;

			curAddrBook.AddContact(contact);

			contact.Commit();

			ContactTreeStore.AppendValues(contact);
		}

		public void ContactEditedEventHandler(object o,
				ContactEventArgs args)
		{
			Contact contact = args.Contact;

			contact.Commit();

			on_contact_selection_changed(o, args);
		}

		public void on_contact_key_press(object o, KeyPressEventArgs args)
		{
			switch(args.Event.HardwareKeycode)
			{
				case 22:
				case 107:
					{
						TreeSelection tSelect = ContactTreeView.Selection;
						if(tSelect.CountSelectedRows() > 0)
						{
							MessageDialog dialog = new MessageDialog(abMainWin,
									DialogFlags.Modal | 
											DialogFlags.DestroyWithParent,
									MessageType.Question,
									ButtonsType.YesNo,
									"Do you want to delete the selected Contacts?");

							dialog.Response += new ResponseHandler(
									DeleteContactResponse);
							dialog.Title = "Denali Delete Contacts";
							dialog.Show();
						}
						break;					
					}
			}
		}

		/*
		   public void on_contact_button_press(object o, ButtonPressEventArgs args)
		   {
		   Console.WriteLine("Hey, buttonwas pressed");
		//			if(args.Event.Button == 3)
		//			{
		Menu cMenu = new Menu();

		MenuItem edit_item = new MenuItem ("Edit Contact");
		cMenu.Append (edit_item);
		//edit_item.Activated += new EventHandler(show_browser);
		MenuItem delete_item = new MenuItem ("Delete Contact");
		cMenu.Append (delete_item);
		//delete_item.Activated += new EventHandler(show_colbrowser);

		cMenu.Append(new SeparatorMenuItem());
		MenuItem export_item = new MenuItem ("Export VCard");
		//export_item.Activated += new EventHandler(quit_ifolder);
		cMenu.Append (export_item);

		cMenu.ShowAll();

		cMenu.Popup(null, null, null, IntPtr.Zero, 3, Gtk.Global.CurrentEventTime);
		//			}
		}
		 */

		public void DeleteContactResponse(object sender, ResponseArgs args)
		{
			MessageDialog dialog = (MessageDialog) sender;

			switch((ResponseType)args.ResponseId)
			{
				case ResponseType.Yes:
					DeleteSelectedContacts();
					break;
				default:
					break;
			}
			dialog.Destroy();
		}

		public void CreateBookEventHandler(object o, BookEditEventArgs args)
		{
			if(abMan != null)
			{
				AddressBook ab = new AddressBook(args.NewName);

				abMan.AddAddressBook(ab);
				ab.Commit();
				BookTreeStore.AppendValues(ab);
			}
			else
			{
				Console.WriteLine("Not c  onnected to an addresbook store");
			}
		}

		public void DeleteSelectedContacts()
		{
			TreeSelection tSelect = ContactTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				if(tModel != null)
					tModel = null;
				Contact cnt = (Contact) ContactTreeStore.GetValue(iter,0);
				if(cnt.IsCurrentUser)
				{
					MessageDialog med = new MessageDialog(abMainWin,
							DialogFlags.DestroyWithParent | DialogFlags.Modal,
							MessageType.Error,
							ButtonsType.Close,
							"Deleting yourself ain't right and we ain't gonna let you do it.");
					med.Title = "This ain't right";
					med.Run();
					med.Hide();
					return;
				}

				ContactTreeStore.Remove(out iter);

				try
				{
					cnt.Delete();
				}
				catch(ApplicationException e)
				{
					Console.WriteLine(e);
				}
			}
		}

		/*
		   private AddressBook GetSelectedAddressBook()
		   {
		   AddressBook ab = null;

		   TreeSelection tSelect = BookTreeView.Selection;
		   if(tSelect.CountSelectedRows() == 1)
		   {
		   TreeModel tModel;
		   TreeIter iter;

		   tSelect.GetSelected(out tModel, out iter);
		   ab = (AddressBook) BookTreeStore.GetValue(iter,0);
		   }
		   return ab;
		   }
		 */

		internal void EditSelectedContact()
		{
			TreeSelection tSelect = ContactTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				if(tModel != null)
					tModel = null;

				Contact c = (Contact) 
					ContactTreeStore.GetValue(iter,0);

				ContactEditor ce = new ContactEditor(abMainWin, c);
				ce.ContactEdited +=
					new ContactEditedEventHandler(
							ContactEditedEventHandler);
				ce.ShowAll();
			}
		}

		internal void on_editContactButton_clicked(object obj, EventArgs args)
		{
			EditSelectedContact();
		}


		internal void on_contact_row_activated(object obj,
				RowActivatedArgs args)
		{
			EditSelectedContact();
		}

		public void on_quit(object o, EventArgs args)
		{
			// Close out the contact picker
			if(cp != null)
			{
				cp.Close();
				cp = null;
			}

			abMainWin.Hide();
			abMainWin.Destroy();
			abMainWin = null;

			if(AddrBookClosed != null)
			{
				EventArgs e = new EventArgs();
				AddrBookClosed(this, e);
			}
		}

		public void on_show_picker(object o, EventArgs args)
		{
			if( (cp == null) || (!cp.IsValid()) )
				cp = new ContactPicker(abMainWin);

			cp.ShowAll();
		}

		// This is code to setup the HTML Text view thingy
		private void AddTitleTags()
		{
			TextTag tag  = new TextTag("title");
			tag.Weight = Pango.Weight.Bold;
			tag.Size = (int) Pango.Scale.PangoScale * 12;
			TitleTextView.Buffer.TagTable.Add(tag);
			/*

			   tag  = new TextTag("bold");
			   tag.Weight = Pango.Weight.Bold;
			   buffer.TagTable.Add(tag);

			   tag  = new TextTag("big");
			   tag.Size = (int) Pango.Scale.PangoScale * 20;
			   buffer.TagTable.Add(tag);

			   tag  = new TextTag("grey");
			   tag.Foreground = "grey";
			   buffer.TagTable.Add(tag);
			 */
		}

		private void AddTitles(string title1, string title2, string title3)
		{
			TextIter insertIter, beginIter, endIter;
			int begin, end;

			insertIter = TitleTextView.Buffer.GetIterAtMark(
					TitleTextView.Buffer.InsertMark);
			TitleTextView.Buffer.Insert (insertIter, "\n");

			if(title1 != null)
			{
				begin = TitleTextView.Buffer.CharCount;
				insertIter = TitleTextView.Buffer.GetIterAtMark(
						TitleTextView.Buffer.InsertMark);
				TitleTextView.Buffer.Insert (insertIter, title1+"\n");
				end = TitleTextView.Buffer.CharCount;
				endIter = TitleTextView.Buffer.GetIterAtOffset(end);
				beginIter = TitleTextView.Buffer.GetIterAtOffset(begin);
				TitleTextView.Buffer.ApplyTag ("title", beginIter, endIter);
			}
			if(title2 != null)
			{
				insertIter = TitleTextView.Buffer.GetIterAtMark(
						TitleTextView.Buffer.InsertMark);
				TitleTextView.Buffer.Insert (insertIter, title2+"\n");
			}
			if(title3 != null)
			{
				insertIter = TitleTextView.Buffer.GetIterAtMark(
						TitleTextView.Buffer.InsertMark);
				TitleTextView.Buffer.Insert (insertIter, title3+"\n");
			}
		}

		// This is code to setup the HTML Text view thingy
		private void AddLabelTags()
		{
			TextTag tag = new TextTag("label");
			tag.Foreground = "grey";
			tag.Justification = Justification.Right;
			LabelTextView.Buffer.TagTable.Add(tag);
		}

		private void AddLabeledValue(string label, string value)
		{
			AddLabel(label);
			AddValue(value);
		}

		private void AddLabel(string label)
		{
			TextIter insertIter, beginIter, endIter;
			int begin, end;

			if(label == null)
			{
				insertIter = LabelTextView.Buffer.GetIterAtMark(
						LabelTextView.Buffer.InsertMark);
				LabelTextView.Buffer.Insert (insertIter, "\n");
			}
			else
			{
				begin = LabelTextView.Buffer.CharCount;
				insertIter = LabelTextView.Buffer.GetIterAtMark(
						LabelTextView.Buffer.InsertMark);
				LabelTextView.Buffer.Insert (insertIter, label+"\n");
				end = LabelTextView.Buffer.CharCount;
				endIter = LabelTextView.Buffer.GetIterAtOffset(end);
				beginIter = LabelTextView.Buffer.GetIterAtOffset(begin);
				LabelTextView.Buffer.ApplyTag ("label", beginIter, endIter);
			}
		}

		private void AddValue(string value)
		{
			TextIter insertIter;
			if(value == null)
			{
				insertIter = ValueTextView.Buffer.GetIterAtMark(
						ValueTextView.Buffer.InsertMark);
				ValueTextView.Buffer.Insert (insertIter, "\n");
			}
			else
			{

				insertIter = ValueTextView.Buffer.GetIterAtMark(
						ValueTextView.Buffer.InsertMark);
				ValueTextView.Buffer.Insert (insertIter, value + "\n");
			}
		}

		private void AddPicture(Pixbuf pixbuf)
		{
			TextIter insertIter;

			if(pixbuf != null)
			{
			insertIter = PictureTextView.Buffer.GetIterAtMark(
					PictureTextView.Buffer.InsertMark);
			PictureTextView.Buffer.Insert (insertIter, "\n");

			insertIter = PictureTextView.Buffer.GetIterAtMark( 
					PictureTextView.Buffer.InsertMark);
			PictureTextView.Buffer.InsertPixbuf (insertIter, pixbuf);
			}
			else
				Console.WriteLine("THE pixbuf was null!");
		}

		private Pixbuf GetScaledPhoto(Contact c, int height)
		{
			Pixbuf pb = null;

			try
			{
				int newWidth, newHeight;
				//		Console.WriteLine("getting photo for : " + c.FN);

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

		public void on_search_changed(object o, EventArgs args)
		{
			if(searchTimeoutID != 0)
			{
				Gtk.Timeout.Remove(searchTimeoutID);
				searchTimeoutID = 0;
			}

			searchTimeoutID = Gtk.Timeout.Add(500, new Gtk.Function(
					SearchCallback));
		}

		public void on_search_button_clicked(object o, EventArgs args)
		{
			SearchAddrBook();
		}

		public void ClearContactDetails()
		{
			PictureTextView.Buffer.Clear();
			LabelTextView.Buffer.Clear();
			ValueTextView.Buffer.Clear();
			TitleTextView.Buffer.Clear();
		}

		// This should probably be broken out into it's own class
		// This method is going to get very large
		public void DisplayContactDetails(Contact c)
		{
			bool addBlank = false;
			ClearContactDetails();
			if(c != null)
			{

				//------------------------
				// Display the photo
				//------------------------
				Pixbuf cPhoto = GetScaledPhoto(c, 64);
				if(cPhoto != null)
					AddPicture(cPhoto);
				else
					AddPicture(BlankHeadPixBuf);

				//------------------------
				// Display the title 
				//------------------------
				if(c.FN != null)
				{
					AddTitles(c.FN, c.Title, null);
				}
				else
				{
					AddTitles(c.UserName, c.Title, null);
				}

				//------------------------
				// Add Telephone numbers
				//------------------------
				foreach(Telephone phone in c.GetTelephoneNumbers())
				{
					addBlank = true;
					if((phone.Types & PhoneTypes.home) == PhoneTypes.home)
					{
						AddLabeledValue("home", phone.Number);
					}
					if((phone.Types & PhoneTypes.work) == PhoneTypes.work)
					{
						AddLabeledValue("work", phone.Number);
					}
					if((phone.Types & PhoneTypes.other) == PhoneTypes.other)
					{
						AddLabeledValue("other", phone.Number);
					}
					if((phone.Types & PhoneTypes.cell) == PhoneTypes.cell)
					{
						AddLabeledValue("mobile", phone.Number);
					}
					if((phone.Types & PhoneTypes.pager) == PhoneTypes.pager)
					{
						AddLabeledValue("pager", phone.Number);
					}
					if((phone.Types & PhoneTypes.fax) == PhoneTypes.fax)
					{
						AddLabeledValue("fax", phone.Number);
					}
				}

				//------------------------
				// Add blank separator line
				//------------------------
				if(addBlank)
				{
					AddLabeledValue(null, null);
					addBlank = false;
				}

				//------------------------
				// Add email addresses
				//------------------------
				foreach(Email e in c.GetEmailAddresses())
				{
					addBlank = true;
					if((e.Types & EmailTypes.personal) == EmailTypes.personal)
					{
						AddLabeledValue("home", e.Address);
					}
					if((e.Types & EmailTypes.work) == EmailTypes.work)
					{
						AddLabeledValue("work", e.Address);
					}
					if((e.Types & EmailTypes.other) == EmailTypes.other)
					{
						AddLabeledValue("other", e.Address);
					}
					if((e.Types & EmailTypes.preferred) == EmailTypes.preferred)
					{
						AddLabeledValue("preferred", e.Address);
					}
				}

				//------------------------
				// Add blank separator line
				//------------------------
				if(addBlank)
				{
					AddLabeledValue(null, null);
					addBlank = false;
				}

				//------------------------
				// Add Addresses
				//------------------------
				foreach(Address addr in c.GetAddresses())
				{
					string addressLine = "";
					if((addr.Types & AddressTypes.home) ==
							AddressTypes.home)
					{
						if(addr.Street != null)
							AddLabeledValue("home", addr.Street);
					}	
					else if((addr.Types & AddressTypes.work) ==
							AddressTypes.work)
					{
						if(addr.Street != null)
							AddLabeledValue("work", addr.Street);
					}
					else
					{
						if(addr.Street != null)
							AddLabeledValue("other", addr.Street);
					}

					if((addr.MailStop != null) && (addr.MailStop.Length > 0))
						AddLabeledValue(null, "MS: " + addr.MailStop);
					if(addr.Locality != null)
						addressLine = addr.Locality + " ";
					if(addr.Region != null)
						addressLine += addr.Region + " ";
					if(addr.PostalCode != null)
						addressLine += addr.PostalCode + " ";
					if(addressLine.Length > 0)
						AddLabeledValue(null, addressLine);
					if((addr.Country != null) && (addr.Country.Length > 0))
						AddLabeledValue(null, addr.Country);

					AddLabeledValue(null, null);
				}
			}
		}
	}
}
