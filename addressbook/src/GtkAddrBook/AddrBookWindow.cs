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
		[Glade.Widget] Gtk.Window abMainWin;
		[Glade.Widget] TreeView	BookTreeView;
		[Glade.Widget] TreeView	ContactTreeView;
		[Glade.Widget] Button CreateContactButton;
		[Glade.Widget] Entry SearchEntry;
		[Glade.Widget] Gtk.TextView ContactTextView;

		Manager abMan;
		AddressBook	curAddrBook;
		ListStore BookTreeStore;
		ListStore ContactTreeStore;
		ListStore GroupTreeStore;
		Pixbuf	UserCardPixBuf;
		Pixbuf	CurCardPixBuf;
		Pixbuf	BookPixBuf;
		Pixbuf	BlankHeadPixBuf;
		ContactPicker cp;
		TextBuffer ContactTextBuffer;
		Hashtable	listHash;

		public event EventHandler AddrBookClosed;

		public AddrBookWindow () 
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

			listHash = new Hashtable();

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

			ContactTextBuffer = ContactTextView.Buffer;
			CreateTags(ContactTextBuffer);
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
			if(listHash.ContainsKey(iter))
			{
				((CellRendererText) cell).Text = ((ListData)listHash[iter]).Name;
			}
			else
			{
				Contact cnt = (Contact) ContactTreeStore.GetValue(iter,0);
				string userName;
				Pixbuf	cPhoto;

				userName = cnt.FN;
				if(userName == null)
					userName = cnt.UserName;

//				cPhoto = GetScaledPhoto(cnt, 24);
//				if(cPhoto == null)
//				{
					if(cnt.IsCurrentUser)
						cPhoto = CurCardPixBuf;
					else
						cPhoto =  UserCardPixBuf;
//				}
				listHash.Add(iter, new ListData(cPhoto, userName));

				((CellRendererText) cell).Text = userName;
//				Console.WriteLine("Getting Name : " + userName);
			}
		}

		private void ContactCellPixbufDataFunc (
				Gtk.TreeViewColumn tree_column, Gtk.CellRenderer cell,
				Gtk.TreeModel tree_model, Gtk.TreeIter iter)
		{
			if(listHash.ContainsKey(iter))
			{
				((CellRendererPixbuf) cell).Pixbuf = ((ListData)listHash[iter]).Icon;
			}
			else
			{
				Contact cnt = (Contact) ContactTreeStore.GetValue(iter,0);
				string userName;
				Pixbuf cPhoto;

				userName = cnt.FN;
				if(userName == null)
					userName = cnt.UserName;
				
//				cPhoto = GetScaledPhoto(cnt, 24);
//				if(cPhoto == null)
//				{
					if(cnt.IsCurrentUser)
						cPhoto = CurCardPixBuf;
					else
						cPhoto =  UserCardPixBuf;
//				}
				listHash.Add(iter, new ListData(cPhoto, userName));

				((CellRendererPixbuf) cell).Pixbuf = cPhoto;

//				Console.WriteLine("Getting Icon for : " + userName);
			}
/*
			if(((CellRendererPixbuf)cell).Pixbuf == null)
			{

			Contact cnt = (Contact) ContactTreeStore.GetValue(iter,0);
			Pixbuf cPhoto = GetScaledPhoto(cnt, 24);
			if(cPhoto != null)
				((CellRendererPixbuf) cell).Pixbuf = cPhoto;
			else
			{
				if(cnt.IsCurrentUser)
					((CellRendererPixbuf) cell).Pixbuf = CurCardPixBuf;
				else
					((CellRendererPixbuf) cell).Pixbuf = UserCardPixBuf;
			}
			}
*/
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
			listHash.Clear();

			if(SearchEntry.Text.Length > 0)
			{
				Hashtable idHash = new Hashtable();

				// First Name Search
				IABList clist = curAddrBook.SearchFirstName(SearchEntry.Text, Simias.Storage.Property.Operator.Begins);
				foreach(Contact c in clist)
				{
					idHash.Add(c.Identity, c);
				}

				// Last Name Search
				clist = curAddrBook.SearchLastName(SearchEntry.Text, Simias.Storage.Property.Operator.Begins);
				foreach(Contact c in clist)
				{
					if(!idHash.Contains(c.Identity))
						idHash.Add(c.Identity, c);
				}

				// UserName Name Search
				clist = curAddrBook.SearchUsername(SearchEntry.Text, Simias.Storage.Property.Operator.Begins);
				foreach(Contact c in clist)
				{
					if(!idHash.Contains(c.Identity))
						idHash.Add(c.Identity, c);
				}

				// Email Name Search
				clist = curAddrBook.SearchEmail(SearchEntry.Text, Simias.Storage.Property.Operator.Begins);
				foreach(Contact c in clist)
				{
					if(!idHash.Contains(c.Identity))
						idHash.Add(c.Identity, c);
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
				Contact newContact = new Contact();

				ContactEditor ce = new ContactEditor(abMainWin, newContact, true);
				ce.ContactEdited +=
					new ContactEditEventHandler(CreateContactEventHandler);
				ce.ShowAll();
			}
		}

		public void onCreateGroup(object o, EventArgs args)
		{
		}

		public void DeleteSelectedBooks()
		{
			TreeSelection tSelect = BookTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
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
				listHash.Clear();
			}
		}

		public void on_book_key_press(object o, KeyPressEventArgs args)
		{
			switch(args.Event.hardware_keycode)
			{
				case 22:
				case 107:
					{
						TreeSelection tSelect = BookTreeView.Selection;
						if(tSelect.CountSelectedRows() > 0)
						{
							MessageDialog dialog = new MessageDialog(	abMainWin,
									DialogFlags.Modal | DialogFlags.DestroyWithParent,
									MessageType.Question,
									ButtonsType.YesNo,
									"Do you want to delete the selected Address Books?");

							dialog.Response += new ResponseHandler(DeleteBookResponse);
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

				tSelect.GetSelected(out tModel, out iter);
				Contact c = (Contact) 
					ContactTreeStore.GetValue(iter,0);

				ClearBuffer();
				Insert("\n");
				Pixbuf cPhoto = GetScaledPhoto(c, 64);
				if(cPhoto != null)
					InsertPixbuf(cPhoto);
				else
					InsertPixbuf(BlankHeadPixBuf);

				Insert("\n\n");
				if(c.FN != null)
				{
					InsertWithTag(c.FN, "heading");
				}
				else
				{
					InsertWithTag(c.UserName, "heading");
				}
				Insert("\n");
				if(c.EMail != null)
					Insert(c.EMail);
			}
			else
			{
				ClearBuffer();
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
				curAddrBook = (AddressBook) BookTreeStore.GetValue(iter,0);

				CreateContactButton.Sensitive = true;
				ContactTreeStore.Clear();
				listHash.Clear();

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

		public void CreateContactEventHandler(object o,
				ContactEditEventArgs args)
		{
			Contact contact = args.ABContact;

			if(args.isNew)
			{
				curAddrBook.AddContact(contact);
			}
			else
			{
				on_contact_selection_changed(o, args);
			}

			contact.Commit();

			if(args.isNew)
			{
				ContactTreeStore.AppendValues(contact);
			}
		}

		public void on_contact_key_press(object o, KeyPressEventArgs args)
		{
			switch(args.Event.hardware_keycode)
			{
				case 22:
				case 107:
					{
						TreeSelection tSelect = ContactTreeView.Selection;
						if(tSelect.CountSelectedRows() > 0)
						{
							MessageDialog dialog = new MessageDialog(	abMainWin,
									DialogFlags.Modal | DialogFlags.DestroyWithParent,
									MessageType.Question,
									ButtonsType.YesNo,
									"Do you want to delete the selected Contacts?");

							dialog.Response += new ResponseHandler(DeleteContactResponse);
							dialog.Title = "Denali Delete Contacts";
							dialog.Show();
						}
						break;					
					}
			}
		}

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

		private void on_contact_row_activated(object obj,
				RowActivatedArgs args)
		{
			TreeSelection tSelect = ContactTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				Contact c = (Contact) 
					ContactTreeStore.GetValue(iter,0);

				ContactEditor ce = new ContactEditor(
						abMainWin, c, false);
				ce.ContactEdited +=
					new ContactEditEventHandler(
							CreateContactEventHandler);
				ce.ShowAll();
			}
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
		private void CreateTags(TextBuffer buffer)
		{
			TextTag tag  = new TextTag("heading");
			tag.Weight = Pango.Weight.Bold;
			tag.Size = (int) Pango.Scale.PangoScale * 15;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("bold");
			tag.Weight = Pango.Weight.Bold;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("big");
			tag.Size = (int) Pango.Scale.PangoScale * 20;
			buffer.TagTable.Add(tag);
		}

		private void InsertWithTag(string insertText, string tagName)
		{
			TextIter insertIter, beginIter, endIter;
			int begin, end;

			begin = ContactTextBuffer.CharCount;
			ContactTextBuffer.GetIterAtMark(out insertIter,
					ContactTextBuffer.InsertMark);
			ContactTextBuffer.Insert (insertIter, insertText);
			end = ContactTextBuffer.CharCount;
			ContactTextBuffer.GetIterAtOffset (out endIter, end);
			ContactTextBuffer.GetIterAtOffset (out beginIter, begin);
			ContactTextBuffer.ApplyTag (tagName, beginIter, endIter);
		}

		private void Insert(string insertText)
		{
			TextIter insertIter;

			ContactTextBuffer.GetIterAtMark(out insertIter,
					ContactTextBuffer.InsertMark);
			ContactTextBuffer.Insert (insertIter, insertText);
		}

		private void InsertPixbuf(Pixbuf pixbuf)
		{
			TextIter insertIter;

			ContactTextBuffer.GetIterAtMark(out insertIter, 
					ContactTextBuffer.InsertMark);
			ContactTextBuffer.InsertPixbuf (insertIter, pixbuf);
		}

		private void ClearBuffer()
		{
			ContactTextBuffer.Text = "";
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

		public void on_search_key_press(object o, KeyPressEventArgs args)
		{
			switch(args.Event.hardware_keycode)
			{
				case 36: // Enter key
					SearchAddrBook();
					break;					
			}
		}

		public void on_search_button_clicked(object o, EventArgs args)
		{
			SearchAddrBook();
		}
	}
}
