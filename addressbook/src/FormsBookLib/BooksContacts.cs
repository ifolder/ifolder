/***********************************************************************
 *  BooksContacts.cs - A user control implementing listviews of address
 *	books and contacts using Windows.Forms
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Bruce Getter <bgetter@novell.com>
 * 
 ***********************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using Novell.AddressBook;
using Simias.Storage;

namespace Novell.iFolder.FormsBookLib
{
	/// <summary>
	/// Summary description for BooksContacts.
	/// </summary>
	public class BooksContacts : System.Windows.Forms.UserControl
	{
		#region Class Members
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ListView books;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.ListView contacts;
		private System.Windows.Forms.TextBox search;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button createBook;
		private System.Windows.Forms.Button createContact;
		private System.Windows.Forms.ContextMenu contactsContextMenu;
		private System.Windows.Forms.MenuItem editContactMenu;
		private System.Windows.Forms.MenuItem createContactMenu;
		private System.Windows.Forms.MenuItem deleteContactMenu;
		private System.Windows.Forms.ContextMenu booksContextMenu;
		private System.Windows.Forms.MenuItem createBookMenu;
		private System.Windows.Forms.MenuItem deleteBookMenu;

		private Novell.AddressBook.Manager manager = null;
		private Novell.AddressBook.AddressBook addressBook = null;
		private ArrayList selectedContacts;
		private ArrayList validSelectedContacts;
		private System.Windows.Forms.Timer editSearchTimer;
		private System.Windows.Forms.ComboBox searchFor;
		private System.Windows.Forms.ComboBox queryType;
		private System.ComponentModel.IContainer components;
		#endregion

		public BooksContacts()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			this.createContact.Enabled = false;

			// Context menu for contacts list view.
			editContactMenu = new MenuItem("Edit...");
			editContactMenu.Click += new EventHandler(editContact_Click);

			createContactMenu = new MenuItem("&Create...");
			createContactMenu.Click += new EventHandler(createContact_Click);

			deleteContactMenu = new MenuItem("Delete");
			deleteContactMenu.Click += new EventHandler(deleteContact_Click);

			contactsContextMenu = new ContextMenu();
			contactsContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {editContactMenu, createContactMenu, deleteContactMenu});
			contacts.ContextMenu = contactsContextMenu;
			contacts.ContextMenu.Popup += new EventHandler(contactsContextMenu_Popup);

			// Context menu for books listview.
			createBookMenu = new MenuItem("Create...");
			createBookMenu.Click += new EventHandler(createBook_Click);

			deleteBookMenu = new MenuItem("Delete");
			deleteBookMenu.Click += new EventHandler(deleteBook_Click);

			booksContextMenu = new ContextMenu();
			booksContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {createBookMenu, deleteBookMenu});
			books.ContextMenu = booksContextMenu;
			books.ContextMenu.Popup += new EventHandler(booksContextMenu_Popup);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.createBook = new System.Windows.Forms.Button();
			this.createContact = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.contacts = new System.Windows.Forms.ListView();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.books = new System.Windows.Forms.ListView();
			this.search = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.editSearchTimer = new System.Windows.Forms.Timer(this.components);
			this.searchFor = new System.Windows.Forms.ComboBox();
			this.queryType = new System.Windows.Forms.ComboBox();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// createBook
			// 
			this.createBook.Location = new System.Drawing.Point(8, 368);
			this.createBook.Name = "createBook";
			this.createBook.Size = new System.Drawing.Size(24, 23);
			this.createBook.TabIndex = 4;
			this.createBook.Text = "+";
			this.createBook.Click += new System.EventHandler(this.createBook_Click);
			// 
			// createContact
			// 
			this.createContact.Location = new System.Drawing.Point(128, 368);
			this.createContact.Name = "createContact";
			this.createContact.Size = new System.Drawing.Size(24, 23);
			this.createContact.TabIndex = 5;
			this.createContact.Text = "+";
			this.createContact.Click += new System.EventHandler(this.createContact_Click);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.contacts);
			this.panel1.Controls.Add(this.splitter1);
			this.panel1.Controls.Add(this.books);
			this.panel1.Location = new System.Drawing.Point(0, 56);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(304, 304);
			this.panel1.TabIndex = 2;
			// 
			// contacts
			// 
			this.contacts.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contacts.HideSelection = false;
			this.contacts.Location = new System.Drawing.Point(123, 0);
			this.contacts.Name = "contacts";
			this.contacts.Size = new System.Drawing.Size(181, 304);
			this.contacts.TabIndex = 1;
			this.contacts.DoubleClick += new System.EventHandler(this.contacts_DoubleClick);
			this.contacts.SelectedIndexChanged += new System.EventHandler(this.contacts_SelectedIndexChanged);
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(120, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 304);
			this.splitter1.TabIndex = 1;
			this.splitter1.TabStop = false;
			this.splitter1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitter1_SplitterMoved);
			// 
			// books
			// 
			this.books.Dock = System.Windows.Forms.DockStyle.Left;
			this.books.HideSelection = false;
			this.books.Location = new System.Drawing.Point(0, 0);
			this.books.Name = "books";
			this.books.Size = new System.Drawing.Size(120, 304);
			this.books.TabIndex = 0;
			this.books.SelectedIndexChanged += new System.EventHandler(this.books_SelectedIndexChanged);
			// 
			// search
			// 
			this.search.Location = new System.Drawing.Point(184, 24);
			this.search.Name = "search";
			this.search.Size = new System.Drawing.Size(120, 20);
			this.search.TabIndex = 3;
			this.search.Text = "";
			this.search.TextChanged += new System.EventHandler(this.search_TextChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(0, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(48, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Search:";
			// 
			// editSearchTimer
			// 
			this.editSearchTimer.Interval = 500;
			this.editSearchTimer.Tick += new System.EventHandler(this.editSearchTimer_Tick);
			// 
			// searchFor
			// 
			this.searchFor.Items.AddRange(new object[] {
														   "First name",
														   "Last name",
														   "User name",
														   "E-Mail"});
			this.searchFor.Location = new System.Drawing.Point(0, 24);
			this.searchFor.Name = "searchFor";
			this.searchFor.Size = new System.Drawing.Size(96, 21);
			this.searchFor.TabIndex = 1;
			this.searchFor.Text = "First name";
			this.searchFor.SelectedIndexChanged += new System.EventHandler(this.searchFor_SelectedIndexChanged);
			// 
			// queryType
			// 
			this.queryType.Items.AddRange(new object[] {
														   "Begins with",
														   "Ends with",
														   "Contains"});
			this.queryType.Location = new System.Drawing.Point(96, 24);
			this.queryType.Name = "queryType";
			this.queryType.Size = new System.Drawing.Size(88, 21);
			this.queryType.TabIndex = 2;
			this.queryType.Text = "Begins with";
			this.queryType.SelectedIndexChanged += new System.EventHandler(this.queryType_SelectedIndexChanged);
			// 
			// BooksContacts
			// 
			this.Controls.Add(this.queryType);
			this.Controls.Add(this.searchFor);
			this.Controls.Add(this.search);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.createContact);
			this.Controls.Add(this.createBook);
			this.Name = "BooksContacts";
			this.Size = new System.Drawing.Size(496, 400);
			this.Load += new System.EventHandler(this.BooksContacts_Load);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		/// <summary>
		/// Sets the Address Book Manager.
		/// </summary>
		public Novell.AddressBook.Manager CurrentManager
		{
			set
			{
				this.manager = value;
			}
		}

		/// <summary>
		/// Gets the current selected address book.
		/// </summary>
		public Novell.AddressBook.AddressBook SelectedAddressBook
		{
			get
			{
				Novell.AddressBook.AddressBook book = null;
				foreach(ListViewItem lvitem in this.books.SelectedItems)
				{
					// The books listview is single-select so only one 
					// item will be in this collection.
					book = (Novell.AddressBook.AddressBook)lvitem.Tag;
				}

				return book;
			}
		}

		/// <summary>
		/// Gets an ArrayList of contacts selected in the contacts listview.
		/// </summary>
		public ArrayList SelectedContacts
		{
			get
			{
				selectedContacts = new ArrayList();
				foreach (ListViewItem item in contacts.SelectedItems)
				{
					selectedContacts.Add(item);
				}

				return selectedContacts;
			}
		}

		/// <summary>
		/// Gets an ArrayList of valid contacts selected in the contacts listview.
		/// </summary>
		public ArrayList ValidSelectedContacts
		{
			get
			{
				validSelectedContacts = new ArrayList();
				foreach (ListViewItem item in contacts.SelectedItems)
				{
					if (item.ForeColor != Color.Gray)
					{
						validSelectedContacts.Add(item);
					}
				}

				return validSelectedContacts;
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Invokes the AddBook dialog.
		/// </summary>
		public void CreateAddressBook()
		{
			createBook_Click(this, null);
		}

		/// <summary>
		/// Invokes the ContactEditor dialog.
		/// </summary>
		public void CreateContact()
		{
			createContact_Click(this, null);
		}

		/// <summary>
		/// Invokes the ContactEditor dialog populated with the selected contact's information.
		/// </summary>
		public void EditContact()
		{
			editContact_Click(this, null);
		}

		/// <summary>
		/// Deletes the currently selected address book.
		/// </summary>
		public void DeleteAddressBook()
		{
			deleteBook_Click(this, null);
		}

		/// <summary>
		/// Deletes the currently selected contact(s).
		/// </summary>
		public void DeleteContact()
		{
			deleteContact_Click(this, null);
		}
		#endregion

		public delegate void ContactSelectedDelegate(object sender, ContactSelectedEventArgs e);
		public event ContactSelectedDelegate ContactSelected;

		public delegate void ContactDoubleClickedDelegate(object sender, ContactDoubleClickedEventArgs e);
		public event ContactDoubleClickedDelegate ContactDoubleClicked;

		#region Event Handlers
		private void BooksContacts_Load(object sender, EventArgs e)
		{
			// Set properties for the listviews
			this.books.MultiSelect = false;
			this.books.View = View.Details;
			this.books.Columns.Add("Books", this.books.Size.Width - 4, HorizontalAlignment.Left);
			this.contacts.View = View.Details;
			this.contacts.Columns.Add("Contacts", this.contacts.Size.Width - 4, HorizontalAlignment.Left);

			if (manager != null)
			{
				// Put all the address books in the books listview.
				IEnumerator addrBooks= manager.GetAddressBooks();
				while (addrBooks.MoveNext())
				{
					AddressBook.AddressBook book = (AddressBook.AddressBook)addrBooks.Current;

					// If the address book is the default book then label it as such.
					ListViewItem item = new ListViewItem(book.Default ? "Default" : book.Name);
					if (book.Default)
					{
						// Select the default address book.
						item.Selected = true;
					}

					item.Tag = book;
					this.books.Items.Add(item);
				}
			}
		}

		private void books_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// An address book was selected/unselected.
			if (this.books.SelectedItems.Count > 0)
			{
				// Change to a wait cursor.
				Cursor = Cursors.WaitCursor;

				// Get the address book that was selected, since the listview is
				// single select the collection will only contain one item.
				foreach (ListViewItem item in this.books.SelectedItems)
				{
					this.addressBook = (Novell.AddressBook.AddressBook)item.Tag;
				}

				// Clear the contacts listview.
				this.contacts.Items.Clear();
				this.contacts.BeginUpdate();

				// Get each contact in the address book.
				foreach(Contact c in this.addressBook)
				{
					ListViewItem item;
					
					if (c.FN != null)
					{
						// Display full name if it is available...
						item = new ListViewItem(c.FN);
					}
					else
					{
						// ... otherwise, display the user name.
						item = new ListViewItem(c.UserName);
					}

					// Save the contact object in the listview item.
					item.Tag = c;

					// Add the item to the listview.
					this.contacts.Items.Add(item);
				}
				this.contacts.EndUpdate();

				// Restore the cursor.
				Cursor = Cursors.Default;

				this.createContact.Enabled = true;
			}
			else
			{
				Cursor = Cursors.WaitCursor;
				this.contacts.Items.Clear();
				Cursor = Cursors.Default;
				this.createContact.Enabled = false;
			}
		}

		private void contacts_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// A contact was selected/unselected.
			if (this.contacts.SelectedItems.Count > 0)
			{
				bool validSelected = false;

				// Is a single item selected?
				bool singleSelected = this.contacts.SelectedItems.Count == 1;

				// Walk the selected items collection to see if valid items were selected.
				foreach (ListViewItem item in this.contacts.SelectedItems)
				{
					// If the fore color is gray the item is not valid.
					if (item.ForeColor != Color.Gray)
					{
						validSelected = true;
						break;
					}
				}

				// Fire the event that contacts have been selected.
				ContactSelectedEventArgs args = new ContactSelectedEventArgs(validSelected, singleSelected);
				if (ContactSelected != null)
				{
					ContactSelected(this, args);
				}
			}
		}

		private void contacts_DoubleClick(object sender, EventArgs e)
		{
			// A contact was double clicked.
			Contact contact = null;
			ListViewItem lvitem = null;

			// Get the contact that was double clicked.
			foreach (ListViewItem item in this.contacts.SelectedItems)
			{
				contact = (Contact)item.Tag;
				lvitem = item;
			}

			if (contact != null)
			{
				// Fire the event that a contact was double clicked.
				ContactDoubleClickedEventArgs args = new ContactDoubleClickedEventArgs(this.addressBook, contact, lvitem);
				if (ContactDoubleClicked != null)
				{
					ContactDoubleClicked(this, args);
				}
			}
		}

		private void splitter1_SplitterMoved(object sender, SplitterEventArgs e)
		{
			// When the splitter is moved, relocate the createContact button ...
			Point point = this.createContact.Location;
			point.X = this.splitter1.Location.X + 8;
			this.createContact.Location = point;

			// ... and resize the column headers.
			this.contacts.Columns[0].Width = this.contacts.Size.Width - 4;
			this.books.Columns[0].Width = this.books.Size.Width - 4;
		}

		private void createContact_Click(object sender, System.EventArgs e)
		{
			// Invoke the contact editor dialog.
			ContactEditor editor = new ContactEditor();
			editor.CurrentAddressBook = this.addressBook;
			DialogResult result = editor.ShowDialog();
			if (result == DialogResult.OK)
			{
				// Add the new contact to the contacts listview.
				ListViewItem item;
				if (editor.CurrentContact.FN != null)
				{
					item = new ListViewItem(editor.CurrentContact.FN);
				}
				else
				{
					item = new ListViewItem(editor.CurrentContact.UserName);
				}

				item.Tag = editor.CurrentContact;
				this.contacts.Items.Add(item);
			}
		}

		private void createBook_Click(object sender, System.EventArgs e)
		{
			// Invoke the AddBook dialog.
			AddBook addBook = new AddBook();
			DialogResult result = addBook.ShowDialog();
			if (result == DialogResult.OK)
			{
				// Create address book and add it to the books listview.
				Novell.AddressBook.AddressBook addrBook = new Novell.AddressBook.AddressBook(addBook.Name);
				this.manager.AddAddressBook(addrBook);
				ListViewItem item = new ListViewItem(addrBook.Name);
				item.Tag = addrBook;
				this.books.Items.Add(item);
			}
		}

		private void searchFor_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// If the searchFor listbox is changed and there is text in the search edit box,
			// restart the timer.
			if (search.Text != null)
			{
				this.editSearchTimer.Start();
			}
		}

		private void queryType_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// If the queryType listbox is changed and there is text in the search edit box,
			// restart the timer.
			if (search.Text != null)
			{
				this.editSearchTimer.Start();
			}		
		}

		private void search_TextChanged(object sender, System.EventArgs e)
		{
			// Reset the timer when search text is entered.
			this.editSearchTimer.Stop();
			this.editSearchTimer.Start();
		}

		private void editSearchTimer_Tick(object sender, EventArgs e)
		{
			// The timer event has been fired...
			// Stop the timer.
			this.editSearchTimer.Stop();

			// Clear the listview.
			contacts.Items.Clear();

			// Change to a wait cursor.
			Cursor.Current = Cursors.WaitCursor;
			contacts.BeginUpdate();
			if (search.Text != null)
			{
				// Get the type of query to perform.
                Property.Operator queryType;
				switch (this.queryType.Text)
				{
					case "Contains":
					{
						queryType = Property.Operator.Contains;
						break;
					}
					case "Ends with":
					{
						queryType = Property.Operator.Ends;
						break;
					}
					default:
					{
						queryType = Property.Operator.Begins;
						break;
					}
				}

				// Get which field to search against and perform the search.
				IABList	searchResults;
				switch (this.searchFor.Text)
				{
					case "Last name":
					{
						searchResults = addressBook.SearchLastName(search.Text, queryType);
						break;
					}
					case "User name":
					{
						searchResults = addressBook.SearchUsername(search.Text, queryType);
						break;
					}
					case "E-Mail":
					{
						searchResults = addressBook.SearchEmail(search.Text, queryType);
						break;
					}
					default:
					{
						searchResults = addressBook.SearchFirstName(search.Text, queryType);
						break;
					}
				}

				try
				{
					// Put the contacts returned by the search in the contacts listview.
					foreach(Contact c in searchResults)
					{
						ListViewItem item;
				
						if (c.FN != null)
						{
							item = new ListViewItem(c.FN);
						}
						else
						{
							item = new ListViewItem(c.UserName);
						}

						item.Tag = c;
						this.contacts.Items.Add(item);
					}
				}
				catch{}
			}
			else
			{
				// No text was entered in the search edit box, so put all contacts in the listview.
				foreach (Contact c in addressBook)
				{
					ListViewItem item;
						
					if (c.FN != null)
					{
						item = new ListViewItem(c.FN);
					}
					else
					{
						item = new ListViewItem(c.UserName);
					}

					item.Tag = c;
					this.contacts.Items.Add(item);
				}
			}

			contacts.EndUpdate();

			// Restore the cursor.
			Cursor.Current = Cursors.Default;
		}

		private void editContact_Click(object sender, EventArgs e)
		{
			ListViewItem selectedItem = null;

			foreach (ListViewItem lvitem in this.contacts.SelectedItems)
			{
				selectedItem = lvitem;
			}

			if (selectedItem != null)
			{
				ContactEditor editor = new ContactEditor();
				editor.CurrentAddressBook = this.addressBook;
				editor.CurrentContact = (Contact)selectedItem.Tag;
				DialogResult result = editor.ShowDialog();
				if (result == DialogResult.OK)
				{
					selectedItem.Text = editor.CurrentContact.FN;
				}
			}
		}

		private void deleteContact_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem lvitem in this.contacts.SelectedItems)
			{
				Contact contact = (Contact)lvitem.Tag;
				if (!contact.IsCurrentUser)
				{
					contact.Delete();
					lvitem.Remove();
				}
				else
				{
					MessageBox.Show("Deleting your own contact is not allowed.");
				}
			}
		}

		private void contactsContextMenu_Popup(object sender, EventArgs e)
		{
			if (this.contacts.SelectedItems.Count == 0)
			{
				this.createContactMenu.Enabled = true;
				this.editContactMenu.Enabled = false;
				this.deleteContactMenu.Enabled = false;
			}
			else if (this.contacts.SelectedItems.Count == 1)
			{
				this.createContactMenu.Enabled = false;
				this.editContactMenu.Enabled = true;
				this.deleteContactMenu.Enabled = true;
			}
			else
			{
				this.createContactMenu.Enabled = false;
				this.editContactMenu.Enabled = false;
				this.deleteContactMenu.Enabled = true;
			}
		}

		private void deleteBook_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem lvitem in this.books.SelectedItems)
			{
				Novell.AddressBook.AddressBook book = (Novell.AddressBook.AddressBook)lvitem.Tag;

				if (!book.Default)
				{
					if (MessageBox.Show("Are you sure you want to delete this address book?", lvitem.Text.ToString(), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
					{
						book.Delete();
						lvitem.Remove();
					}
				}
				else
				{
					MessageBox.Show("Deleting the default address book is not allowed.");
				}
			}
		}

		private void booksContextMenu_Popup(object sender, EventArgs e)
		{
			this.createBookMenu.Enabled = this.books.SelectedItems.Count == 0;
			this.deleteBookMenu.Enabled = this.books.SelectedItems.Count != 0;
		}
		#endregion
	}

	public class ContactSelectedEventArgs : EventArgs
	{
		public bool validSelected;
		public bool singleSelected;

		public ContactSelectedEventArgs(bool validSelected, bool singleSelected)
		{
			this.validSelected = validSelected;
			this.singleSelected = singleSelected;
		}
	}

	public class ContactDoubleClickedEventArgs : EventArgs
	{
		public Contact contact;
		public Novell.AddressBook.AddressBook addressBook;
		public ListViewItem lvitem;

		public ContactDoubleClickedEventArgs(Novell.AddressBook.AddressBook addressBook, Contact contact, ListViewItem lvitem)
		{
			this.contact = contact;
			this.addressBook = addressBook;
			this.lvitem = lvitem;
		}
	}
}
