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

namespace Novell.AddressBook.UI.gtk
{
	public class ContactPicker
	{
		[Glade.Widget] internal TreeView	BookTreeView;
		[Glade.Widget] internal TreeView	ContactTreeView;
		[Glade.Widget] internal Gtk.Entry	SearchEntry;
		[Glade.Widget] internal Gtk.Button	AddButton;
		[Glade.Widget] internal Gtk.Button	RemoveButton;
		[Glade.Widget] internal Gtk.Button	NewBookButton;
		[Glade.Widget] internal Gtk.Button	NewGroupButton;
		[Glade.Widget] internal Gtk.Button	NewContactButton;
		[Glade.Widget] internal Gtk.Button	HelpButton;
		[Glade.Widget] internal TreeView	SelectedTreeView;

		private Manager 	abMan;
		private AddressBook curAddrBook;
		private Gtk.Dialog	cpDialog;
		private ListStore	BookTreeStore;
		private ListStore	ContactTreeStore;
		private ListStore	SelectedTreeStore;
		private Pixbuf		UserCardPixBuf;
		private Pixbuf		CurCardPixBuf;
		private Pixbuf		BookPixBuf;
		private uint		searchTimeoutID;
		private Hashtable	selectedContacts;

		public ICollection Contacts
		{
			get
			{
				return selectedContacts.Values;
			}
		}

		public Manager AddrBookManager
		{
			set
			{
				abMan = value;
			}
		}

		public Gtk.Window TransientFor
		{
			set
			{
				if(cpDialog != null)
					cpDialog.TransientFor = value;
			}
		}

		public ContactPicker () 
		{
			InitUI();
		}

		private void InitUI () 
		{
			Glade.XML gxml = new Glade.XML ("contact-picker.glade",
					"ContactPickerDialog", null);
			gxml.Autoconnect (this);

			cpDialog = (Gtk.Dialog) gxml.GetWidget("ContactPickerDialog");

			// Setup the Book TreeView
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


			// Setup the Contact TreeView
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
			ContactTreeView.Selection.Mode = SelectionMode.Multiple;
			ContactTreeView.Selection.Changed += new EventHandler(
					on_contact_selection_changed);


			// Setup the Selected TreeView
			SelectedTreeStore = new ListStore(typeof(Contact));
			SelectedTreeView.Model = SelectedTreeStore;
			CellRendererPixbuf scrp = new CellRendererPixbuf();
			TreeViewColumn stvc = new TreeViewColumn();
			stvc.PackStart(scrp, false);
			stvc.SetCellDataFunc(scrp, new TreeCellDataFunc(
					ContactCellPixbufDataFunc));

			CellRendererText scrt = new CellRendererText();
			stvc.PackStart(scrt, false);
			stvc.SetCellDataFunc(scrt, new TreeCellDataFunc(
					ContactCellTextDataFunc));
			stvc.Title = "Picked";
			SelectedTreeView.AppendColumn(stvc);
			SelectedTreeView.Selection.Mode = SelectionMode.Multiple;
			SelectedTreeView.Selection.Changed += new EventHandler(
					on_selectedTreeView_selection_changed);

			UserCardPixBuf = new Pixbuf("contact.png");
			CurCardPixBuf = new Pixbuf("contact_me.png");
			BookPixBuf = new Pixbuf("book.png");

			searchTimeoutID = 0;
			selectedContacts = new Hashtable();
		}


		private void PopulateWidgets () 
		{
			if(abMan == null)
			{
				try
				{
					abMan = Manager.Connect( );
				}
				catch(Exception e)
				{
					Console.WriteLine(
							"Unable to connect to the Address Book: " + e);
				}
			}

			if(abMan != null)
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
			else
			{
				Console.WriteLine("AddresBook == null");
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
			Contact cnt = (Contact) tree_model.GetValue(iter,0);
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




		private void ContactCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Contact cnt = (Contact) tree_model.GetValue(iter,0);

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


		public int Run()
		{
			int rc = 0;
			if(cpDialog != null)
			{
				PopulateWidgets();
				rc = cpDialog.Run();
				cpDialog.Hide();
				cpDialog.Destroy();
				cpDialog = null;
			}
			return rc;
		}



		private void on_contact_row_activated(object obj,
				RowActivatedArgs args)
		{
			on_AddButton_clicked(obj, args);
		}




		public void on_AddButton_clicked(object o, EventArgs args)
		{
			TreeModel tModel;

			TreeSelection tSelect = ContactTreeView.Selection;
			Array treePaths = tSelect.GetSelectedRows(out tModel);
			// remove compiler warning
			if(tModel != null)
				tModel = null;

			foreach(TreePath tPath in treePaths)
			{
				TreeIter iter;

				if(ContactTreeStore.GetIter(out iter, tPath))
				{
					Contact c = (Contact) ContactTreeStore.GetValue(iter,0);
					if(!selectedContacts.ContainsKey(c.ID))
					{
						selectedContacts.Add(c.ID, c);
						SelectedTreeStore.AppendValues(c);
					}
				}
			}
		}



		public void on_RemoveButton_clicked(object o, EventArgs args)
		{
			TreeModel tModel;
			Queue	iterQueue;

			iterQueue = new Queue();
			TreeSelection tSelect = SelectedTreeView.Selection;
			Array treePaths = tSelect.GetSelectedRows(out tModel);
			// remove compiler warning
			if(tModel != null)
				tModel = null;
			
			// We can't remove anything while getting the iters
			// because it will change the paths and we'll remove
			// the wrong stuff.
			foreach(TreePath tPath in treePaths)
			{
				TreeIter iter;

				if(SelectedTreeStore.GetIter(out iter, tPath))
				{
					iterQueue.Enqueue(iter);
				}
			}
			
			// Now that we have all of the TreeIters, loop and
			// remove them all
			while(iterQueue.Count > 0)
			{
				TreeIter iter = (TreeIter) iterQueue.Dequeue();
				Contact c = (Contact) SelectedTreeStore.GetValue(iter,0);
				selectedContacts.Remove(c.ID);
				SelectedTreeStore.Remove(ref iter);
			}
		}


		public void on_NewBookButton_clicked(object o, EventArgs args)
		{
			BookEditor be = new BookEditor();
			be.TransientFor = cpDialog;

			int rc = be.Run();

			if((rc == -5) && (abMan != null))
			{
				AddressBook ab = new AddressBook(be.Name);

				abMan.AddAddressBook(ab);
				ab.Commit();
				BookTreeStore.AppendValues(ab);
			}
		}



		public void on_NewGroupButton_clicked(object o, EventArgs args)
		{
			MessageDialog dialog = new MessageDialog(	cpDialog,
				DialogFlags.Modal | DialogFlags.DestroyWithParent,
				MessageType.Info,
				ButtonsType.Close,
				"Groups are not that important to me, kinda like OS X\rWe'll add this feature in the future.");

			dialog.Title = "Scotty's Helpful Hints";
			dialog.Run();
			dialog.Hide();
			dialog.Destroy();
		}



		public void on_NewContactButton_clicked(object o, EventArgs args)
		{
			ContactEditor ce = new ContactEditor();
			ce.TransientFor = cpDialog;
			ce.Contact = new Contact();

			int rc = ce.Run();

			if(rc == -5)
			{
				curAddrBook.AddContact(ce.Contact);

				ce.Contact.Commit();

				ContactTreeStore.AppendValues(ce.Contact);
			}
		}




		public void on_HelpButton_clicked(object o, EventArgs args)
		{
			MessageDialog dialog = new MessageDialog(	cpDialog,
				DialogFlags.Modal | DialogFlags.DestroyWithParent,
				MessageType.Info,
				ButtonsType.Close,
				"Help will be in this iteration, just not yet.");

			dialog.Title = "Scotty's Helpful Hints";
			dialog.Run();
			dialog.Hide();
			dialog.Destroy();
		}



		private void SearchAddrBook() 
		{
			ContactTreeStore.Clear();

			if(SearchEntry.Text.Length > 0)
			{
				Hashtable idHash = new Hashtable();

				// First Name Search
				IABList clist = curAddrBook.SearchFirstName(SearchEntry.Text,
						Simias.Storage.SearchOp.Begins);
				foreach(Contact c in clist)
				{
					idHash.Add(c.ID, c);
				}

				// Last Name Search
				clist = curAddrBook.SearchLastName(SearchEntry.Text,
						Simias.Storage.SearchOp.Begins);
				foreach(Contact c in clist)
				{
					if(!idHash.Contains(c.ID))
						idHash.Add(c.ID, c);
				}

				// UserName Name Search
				clist = curAddrBook.SearchUsername(SearchEntry.Text,
						Simias.Storage.SearchOp.Begins);
				foreach(Contact c in clist)
				{
					if(!idHash.Contains(c.ID))
						idHash.Add(c.ID, c);
				}

				// Email Name Search
				clist = curAddrBook.SearchEmail(SearchEntry.Text,
						Simias.Storage.SearchOp.Begins);
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
		}




		private int AddrBookSort(TreeModel model, TreeIter iterA, 
				TreeIter iterB)
		{
		

			return 0;
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
						MessageDialog dialog = new MessageDialog(	cpDialog,
						DialogFlags.Modal | DialogFlags.DestroyWithParent,
						MessageType.Question,
						ButtonsType.YesNo,
						"Do you want to delete the selected Contacts?");

						dialog.Title = "Delete Contacts";
						dialog.TransientFor = cpDialog;
						int rc = dialog.Run();
						dialog.Hide();
						dialog.Destroy();

						if(rc == (int)ResponseType.Yes)
						{
							DeleteSelectedContacts();
						}
					}
					break;					
				}
			}
		}




		public void DeleteSelectedContacts()
		{
			TreeModel tModel;
			Queue	iterQueue;

			iterQueue = new Queue();
			TreeSelection tSelect = ContactTreeView.Selection;
			Array treePaths = tSelect.GetSelectedRows(out tModel);
			// remove compiler warning
			if(tModel != null)
				tModel = null;
			
			// We can't remove anything while getting the iters
			// because it will change the paths and we'll remove
			// the wrong stuff.
			foreach(TreePath tPath in treePaths)
			{
				TreeIter iter;

				if(ContactTreeStore.GetIter(out iter, tPath))
				{
					iterQueue.Enqueue(iter);
				}
			}
			
			// Now that we have all of the TreeIters, loop and
			// remove them all
			while(iterQueue.Count > 0)
			{
				TreeIter iter = (TreeIter) iterQueue.Dequeue();
				Contact c = (Contact) ContactTreeStore.GetValue(iter,0);
				if(c.IsCurrentUser)
				{
					MessageDialog med = new MessageDialog(cpDialog,
							DialogFlags.DestroyWithParent | DialogFlags.Modal,
							MessageType.Error,
							ButtonsType.Close,
							"Deleting yourself ain't right and we ain't gonna let you do it.");
					med.Title = "This ain't right";
					med.Run();
					med.Hide();
					med.Destroy();
				}
				else
				{
					ContactTreeStore.Remove(ref iter);

					try
					{
						c.Delete();
					}
					catch(ApplicationException e)
					{
						Console.WriteLine(e);
					}
				}
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
							MessageDialog dialog = new MessageDialog(cpDialog,
									DialogFlags.Modal | 
									DialogFlags.DestroyWithParent,
									MessageType.Question,
									ButtonsType.YesNo,
									"Do you want to delete the selected Address Books?");

							dialog.Title = "Delete Books";
							dialog.TransientFor = cpDialog;
							int rc = dialog.Run();
							dialog.Hide();
							dialog.Destroy();

							if(rc == (int)ResponseType.Yes)
							{
								DeleteSelectedBooks();
							}
						}
						break;					
					}
			}
		}




		public void DeleteSelectedBooks()
		{
			TreeSelection tSelect = BookTreeView.Selection;
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
					MessageDialog med = new MessageDialog(cpDialog,
							DialogFlags.DestroyWithParent | DialogFlags.Modal,
							MessageType.Error,
							ButtonsType.Close,
							"Deleting the default address book ain't right and we ain't gonna let you do it.");
					med.Title = "This ain't right";
					med.Run();
					med.Hide();
					med.Destroy();
					return;
				}

				BookTreeStore.Remove(ref iter);

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

				//CreateContactButton.Sensitive = true;
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




		public void on_contact_selection_changed(object o, EventArgs args)
		{
			TreeSelection tSelect = ContactTreeView.Selection;

			if(tSelect.CountSelectedRows() > 0)
			{
				AddButton.Sensitive = true;
			}
			else
			{
				AddButton.Sensitive = false;
			}
		}



		public void on_selectedTreeView_selection_changed(
				object o, EventArgs args)
		{
			TreeSelection tSelect = SelectedTreeView.Selection;

			if(tSelect.CountSelectedRows() > 0)
			{
				RemoveButton.Sensitive = true;
			}
			else
			{
				RemoveButton.Sensitive = false;
			}
		}



		public void on_SearchEntry_changed(object o, EventArgs args)
		{
			if(searchTimeoutID != 0)
			{
				Gtk.Timeout.Remove(searchTimeoutID);
				searchTimeoutID = 0;
			}

			searchTimeoutID = Gtk.Timeout.Add(500, new Gtk.Function(
					SearchCallback));
		}




		private bool SearchCallback()
		{
			SearchAddrBook();
			return false;
		}
	}
}
