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
using System.Collections;

using Gtk;
using Gdk;
using Gnome;
using Glade;
using GtkSharp;
using GLib;

namespace Novell.iFolder
{
	public class CPListData
	{
		private readonly Pixbuf pb;
		private readonly string name;

		public CPListData(Pixbuf pb, string name)
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

	public class ContactsPickedEventArgs : EventArgs
	{
		private readonly string ContactName;
		private readonly Contact c;

		//Constructor.
		//
		public ContactsPickedEventArgs(Contact contact)
		{
			this.c = contact;
		}

		public Contact contact
		{     
			get { return c;}      
		}
	}

	// Delegate declaration
	//
	public delegate void ContactsPickedEventHandler(object sender,
			ContactsPickedEventArgs e);

	public class ContactPicker
	{
		[Glade.Widget] Gtk.Entry ceFullName;
		[Glade.Widget] TreeView	BookTreeView;
		[Glade.Widget] TreeView	ContactTreeView;
		[Glade.Widget] Gtk.Entry SearchEntry;

		Manager 	abMan;
		AddressBook curAddrBook;
		Gtk.Window cpwin;
		ListStore BookTreeStore;
		ListStore ContactTreeStore;
		Pixbuf	UserCardPixBuf;
		Pixbuf	CurCardPixBuf;
		Pixbuf	BookPixBuf;
		Hashtable	listHash;

		public event ContactsPickedEventHandler ContactsPicked;

		public ContactPicker (Gtk.Window parentWin) 
		{
			Int32 pX, pY, pdX, pdY;

			parentWin.GetPosition(out pX, out pY);
			parentWin.GetSize(out pdX, out pdY);
			InitUI();
			cpwin.Move(pX + pdX + 10, pY);
		}

		public ContactPicker () 
		{
			InitUI();
		}

		private void InitUI () 
		{
			Glade.XML gxml = new Glade.XML ("addressbook.glade",
					"abContactPicker", null);
			gxml.Autoconnect (this);

			cpwin = (Gtk.Window) gxml.GetWidget("abContactPicker");
			cpwin.Move(2,2);

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
			BookTreeView.Selection.Changed += new EventHandler(
					on_book_selection_changed);

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

//			TreeSelection tSel = ContactTreeView.Selection;

//			tSel.Mode = SelectionMode.Multiple;

	//		ContactTreeStore.SetDefaultSortFunc(new TreeIterCompareFunc(AddrBookSort), null, null);
/*
			ContactTreeView.AppendColumn("Email",
					new CellRendererText(),
					new TreeCellDataFunc(EmailCellTextDataFunc));
*/

//			ContactTreeView.RulesHint = true;

			UserCardPixBuf = new Pixbuf("contact.png");
			CurCardPixBuf = new Pixbuf("contact_me.png");
			BookPixBuf = new Pixbuf("book.png");

//			Gtk.Image tmpImg = (Gtk.Image) gxml.GetWidget("image4");

			listHash = new Hashtable();

			if(abMan == null)
			{
				try
				{
					abMan = Manager.Connect( );

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
				((CellRendererText) cell).Text = 
						((CPListData)listHash[iter]).Name;
			}
			else
			{
				Contact cnt = (Contact) ContactTreeStore.GetValue(iter,0);
				string userName;
				Pixbuf	cPhoto;

				userName = cnt.FN;
				if(userName == null)
					userName = cnt.UserName;

/*
				Pixbuf cPhoto = GetScaledPhoto(cnt, 24);
				if(cPhoto == null)
				{
*/
					if(cnt.IsCurrentUser)
						cPhoto = CurCardPixBuf;
					else
						cPhoto =  UserCardPixBuf;
//				}
				listHash.Add(iter, new CPListData(cPhoto, userName));

				((CellRendererText) cell).Text = userName;
//				Console.WriteLine("Getting Name : " + userName);
			}
/*

			Contact cnt = (Contact) ContactTreeStore.GetValue(iter,0);
			string userName;

			userName = cnt.FN;
			if(userName == null)
				userName = cnt.UserName;

			((CellRendererText) cell).Text = userName;
*/
		}

		private void ContactCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			if(listHash.ContainsKey(iter))
			{
				((CellRendererPixbuf) cell).Pixbuf = 
						((CPListData)listHash[iter]).Icon;
			}
			else
			{
				Contact cnt = (Contact) ContactTreeStore.GetValue(iter,0);
				string userName;
				Pixbuf cPhoto;

				userName = cnt.FN;
				if(userName == null)
					userName = cnt.UserName;
				
/*
				cPhoto = GetScaledPhoto(cnt, 24);
				if(cPhoto == null)
				{
*/					if(cnt.IsCurrentUser)
						cPhoto = CurCardPixBuf;
					else
						cPhoto =  UserCardPixBuf;
//				}
				listHash.Add(iter, new CPListData(cPhoto, userName));

				((CellRendererPixbuf) cell).Pixbuf = cPhoto;

//				Console.WriteLine("Getting Icon for : " + userName);
			}
/*

			Contact cnt = (Contact) ContactTreeStore.GetValue(iter,0);
			Pixbuf pb = GetScaledPhoto(cnt, 24);
			if(pb != null)
				((CellRendererPixbuf) cell).Pixbuf = pb;
			else
			{
				if(cnt.IsCurrentUser)
					((CellRendererPixbuf) cell).Pixbuf = CurCardPixBuf;
				else
					((CellRendererPixbuf) cell).Pixbuf = UserCardPixBuf;
			}
*/
		}
/*
		private void EmailCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Contact cnt = (Contact) ContactTreeStore.GetValue(iter,0);
			string userEmail;

			userEmail = cnt.EMail;
			if(userEmail == null)
				userEmail = "";

			((CellRendererText) cell).Text = userEmail;
		}
*/

		protected virtual void OnContactsPicked(ContactsPickedEventArgs e)
		{
			if(ContactsPicked != null)
			{
				ContactsPicked(this, e);
			}
		}

		public void ShowAll()
		{
			if(cpwin != null)
			{
				cpwin.Present();
			}
		}

		public bool IsValid()
		{
			if(cpwin != null)
				return true;
			else
				return false;
		}

		private void on_contact_row_activated(object obj,
				RowActivatedArgs args)
		{
			onAdd(obj, args);
		}

		public void onAdd(object o, EventArgs args)
		{
			TreeSelection tSelect = ContactTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				Contact c = (Contact) ContactTreeStore.GetValue(iter,0);

				ContactsPickedEventArgs e = new 
						ContactsPickedEventArgs(c);

				OnContactsPicked(e);
			}

/*
			cpwin.Hide();
			cpwin.Destroy();
			cpwin = null;
*/
		}
		
		public void Close()
		{
			if(cpwin != null)
			{
				cpwin.Hide();
				cpwin.Destroy();
			}
			cpwin = null;
		}

		public void onCancel(object o, EventArgs args) 
		{
			Close();
		}

		public void on_delete (object o, DeleteEventArgs args) 
		{
			Close();
		}

		public void on_add_contact_clicked(object o, EventArgs args) 
		{
			Contact newContact = new Contact();

			ContactEditor ce = new ContactEditor(cpwin, newContact, true);
			ce.ContactEdited +=
				new ContactEditEventHandler(CreateContactEventHandler);
			ce.ShowAll();
		}

		public void on_add_book_clicked(object o, EventArgs args)
		{
			BookEditor be = new BookEditor();
			be.BookEdited += new BookEditEventHandler(CreateBookEventHandler);
			be.ShowAll();
		}

		public void onKeyPressed(object o, KeyPressEventArgs args)
		{
			switch(args.Event.hardware_keycode)
			{
				case 9:
					onCancel(o, args);
					break;
				case 36:
					onAdd(o, args);
					break;					
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

			contact.Commit();

			if(args.isNew)
			{
				ContactTreeStore.AppendValues(contact);
			}
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

		private void SearchAddrBook() 
		{
			ContactTreeStore.Clear();
			listHash.Clear();

			if(SearchEntry.Text.Length > 0)
			{
				Hashtable idHash = new Hashtable();

				// First Name Search
				IABList clist = curAddrBook.SearchFirstName(SearchEntry.Text,
						Simias.Storage.Property.Operator.Begins);
				foreach(Contact c in clist)
				{
					idHash.Add(c.Identity, c);
				}

				// Last Name Search
				clist = curAddrBook.SearchLastName(SearchEntry.Text,
						Simias.Storage.Property.Operator.Begins);
				foreach(Contact c in clist)
				{
					if(!idHash.Contains(c.Identity))
						idHash.Add(c.Identity, c);
				}

				// UserName Name Search
				clist = curAddrBook.SearchUsername(SearchEntry.Text,
						Simias.Storage.Property.Operator.Begins);
				foreach(Contact c in clist)
				{
					if(!idHash.Contains(c.Identity))
						idHash.Add(c.Identity, c);
				}

				// Email Name Search
				clist = curAddrBook.SearchEmail(SearchEntry.Text,
						Simias.Storage.Property.Operator.Begins);
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
		
		private int AddrBookSort(TreeModel model, TreeIter iterA, 
				TreeIter iterB)
		{
		

			return 0;
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
							MessageDialog dialog = new MessageDialog(	cpwin,
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
					MessageDialog med = new MessageDialog(cpwin,
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
							MessageDialog dialog = new MessageDialog(	cpwin,
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
					MessageDialog med = new MessageDialog(cpwin,
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

		public void on_book_selection_changed(object o, EventArgs args)
		{
			TreeSelection tSelect = BookTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				curAddrBook = (AddressBook) BookTreeStore.GetValue(iter,0);

				//CreateContactButton.Sensitive = true;
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
