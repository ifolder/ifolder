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
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
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
using System.IO;
using Novell.AddressBook;
using Simias;
using Simias.Storage;

namespace Novell.iFolder.FormsBookLib
{
	/// <summary>
	/// User control used to display address books and contacts.
	/// </summary>
	public class BooksContacts : System.Windows.Forms.UserControl
	{
		#region Class Members
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(BooksContacts));
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ListView books;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.ListView contacts;
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
		private System.ComponentModel.IContainer components;
		private string loadPath;
		private System.Windows.Forms.HelpProvider helpProvider1;
		private string filter;
		#endregion

		/// <summary>
		/// Initializes a new instance of the BooksContacts class.
		/// </summary>
		public BooksContacts()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

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
			this.panel1 = new System.Windows.Forms.Panel();
			this.contacts = new System.Windows.Forms.ListView();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.books = new System.Windows.Forms.ListView();
			this.helpProvider1 = new System.Windows.Forms.HelpProvider();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.Controls.Add(this.contacts);
			this.panel1.Controls.Add(this.splitter1);
			this.panel1.Controls.Add(this.books);
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(368, 304);
			this.panel1.TabIndex = 2;
			// 
			// contacts
			// 
			this.contacts.Dock = System.Windows.Forms.DockStyle.Fill;
			this.helpProvider1.SetHelpString(this.contacts, "Displays the contacts found in the selected address book.");
			this.contacts.HideSelection = false;
			this.contacts.Location = new System.Drawing.Point(123, 0);
			this.contacts.Name = "contacts";
			this.helpProvider1.SetShowHelp(this.contacts, true);
			this.contacts.Size = new System.Drawing.Size(245, 304);
			this.contacts.TabIndex = 1;
			this.contacts.KeyDown += new System.Windows.Forms.KeyEventHandler(this.contacts_KeyDown);
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
			this.helpProvider1.SetHelpString(this.books, "Displays the available address books.");
			this.books.HideSelection = false;
			this.books.Location = new System.Drawing.Point(0, 0);
			this.books.Name = "books";
			this.helpProvider1.SetShowHelp(this.books, true);
			this.books.Size = new System.Drawing.Size(120, 304);
			this.books.TabIndex = 0;
			this.books.KeyDown += new System.Windows.Forms.KeyEventHandler(this.books_KeyDown);
			this.books.SelectedIndexChanged += new System.EventHandler(this.books_SelectedIndexChanged);
			// 
			// BooksContacts
			// 
			this.Controls.Add(this.panel1);
			this.Name = "BooksContacts";
			this.Size = new System.Drawing.Size(368, 304);
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

		/// <summary>
		/// Gets/sets the path where the assembly is loaded from.
		/// </summary>
		public string LoadPath
		{
			get
			{
				return this.loadPath;
			}

			set
			{
				this.loadPath = value;
			}
		}

		/// <summary>
		/// Gets the image list for the contact objects.
		/// </summary>
		public ImageList ContactImageList
		{
			get
			{
				return contacts.SmallImageList;
			}
		}

		/// <summary>
		/// Gets/sets the filter used when performing searches.
		/// </summary>
		public string Filter
		{
			get
			{
				return filter;
			}

			set
			{
				filter = value;
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds a contact to the contact listview.
		/// </summary>
		/// <param name="contact">The contact to add to the listview.</param>
		/// <param name="imageIndex">The index of the image to display for this contact.</param>
		/// <param name="selected">Indicates if the contact is to be selected.</param>
		public void AddContactToListView(Contact contact, int imageIndex, bool selected)
		{
			if (imageIndex == -1)
			{
				imageIndex = contact.IsCurrentUser ? 0 : 1;
			}

			ListViewItem item;

			if (contact.FN != null && contact.FN != String.Empty)
			{
				item = new ListViewItem(contact.FN, imageIndex);
			}
			else
			{
				item = new ListViewItem(contact.UserName, imageIndex);
			}

			item.Tag = contact;
			contacts.Items.Add(item);

			if (selected)
			{
				contacts.SelectedItems.Clear();
				item.Selected = true;
			}
		}

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

		/// <summary>
		/// Searches the selected address book for contacts based on the filter passed in.
		/// </summary>
		/// <param name="filter">The string used for the search filter. </param>
		public void FilterList(string filter)
		{
			if (filter != null)
				this.filter = filter;

			// Clear the listview.
			contacts.Items.Clear();

			// Change to a wait cursor.
			Cursor.Current = Cursors.WaitCursor;
			contacts.BeginUpdate();
			if (Filter != null && Filter != String.Empty)
			{
				// Hashtable used to store search results in.
				Hashtable results = new Hashtable();

				try
				{
					// Search the first name.
					IABList searchResults = addressBook.SearchFirstName(Filter, SearchOp.Contains);
					foreach(Contact c in searchResults)
					{
						// Put the contacts returned by the search in the hashtable.
						results.Add(c.ID, c);
					}

					// Search the last name.
					searchResults = addressBook.SearchLastName(Filter, SearchOp.Contains);
					foreach(Contact c in searchResults)
					{
						// Add the contact to the hashtable if it already isn't there.
						if (!results.ContainsKey(c.ID))
							results.Add(c.ID, c);
					}

					// Search the user name.
					searchResults = addressBook.SearchUsername(Filter, SearchOp.Contains);
					foreach(Contact c in searchResults)
					{
						// Add the contact to the hashtable if it already isn't there.
						if (!results.ContainsKey(c.ID))
							results.Add(c.ID, c);
					}

					// Search eMail.
					searchResults = addressBook.SearchEmail(Filter, SearchOp.Contains);
					foreach(Contact c in searchResults)
					{
						// Add the contact to the hashtable if it already isn't there.
						if (!results.ContainsKey(c.ID))
							results.Add(c.ID, c);
					}

					// Go through the hashtable and add the items to the listview.
					IDictionaryEnumerator enumerator = results.GetEnumerator();
					while (enumerator.MoveNext())
					{
						Contact c = (Contact) enumerator.Value;

						AddContactToListView(c, -1, false);
					}
				}
				catch (SimiasException e)
				{
					e.LogError();
					MessageBox.Show("An error was encountered while searching.  Please see the log file for additional information.", "Search Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				catch (Exception e)
				{
					logger.Debug(e, "Searching - filter = {0}", filter);
					MessageBox.Show("An error was encountered while searching.  Please see the log file for additional information.", "Search Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
			else
			{
				// No text was entered in the search edit box, so put all contacts in the listview.
				foreach (Contact c in addressBook)
				{
					AddContactToListView(c, -1, false);
				}
			}

			contacts.EndUpdate();

			// Restore the cursor.
			Cursor.Current = Cursors.Default;

			// Fire the contact selected event.
			contacts_SelectedIndexChanged(this, null);
		}
		#endregion

		#region Events
		/// <summary>
		/// Delegate used for the Contact Selected event.
		/// </summary>
		public delegate void ContactSelectedDelegate(object sender, ContactSelectedEventArgs e);
		/// <summary>
		/// Occurs when a contact has been selected.
		/// </summary>
		public event ContactSelectedDelegate ContactSelected;

		/// <summary>
		/// Delegate used for the Contact Double Clicked event.
		/// </summary>
		public delegate void ContactDoubleClickedDelegate(object sender, ContactDoubleClickedEventArgs e);
		/// <summary>
		/// Occurs when a contact has been double-clicked.
		/// </summary>
		public event ContactDoubleClickedDelegate ContactDoubleClicked;

		/// <summary>
		/// Delegate used for the Book Selected event.
		/// </summary>
		public delegate void BookSelectedDelegate(object sender, EventArgs e);
		/// <summary>
		/// Occurs when a book has been selected.
		/// </summary>
		public event BookSelectedDelegate BookSelected;
		#endregion

		#region Event Handlers
		private void BooksContacts_Load(object sender, EventArgs e)
		{
			// Image lists...
			try
			{
				// Create the books ImageList object.
				ImageList booksImageList = new ImageList();

				// Initialize the ImageList objects with icons.
				string basePath = Path.Combine(loadPath != null ? loadPath : Application.StartupPath, "res");
				booksImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_add_bk.ico")));

				//Assign the ImageList objects to the books ListView.
				books.SmallImageList = booksImageList;

				// Create the contacts ImageList object.
				ImageList contactsImageList = new ImageList();

				// Initialize the ImageList objects with icons.
				contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_me_card.ico")));
				contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_contact_card.ico")));
				contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_add_bk.ico")));
				contactsImageList.Images.Add(Image.FromFile(Path.Combine(basePath, "vcardimport.png")));
				contactsImageList.Images.Add(Image.FromFile(Path.Combine(basePath, "vcardexport.png")));

				//Assign the ImageList objects to the books ListView.
				contacts.SmallImageList = contactsImageList;
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Loading images");
			}

			// Set properties for the listviews
			this.books.MultiSelect = false;
			this.books.View = View.Details;
			this.books.Columns.Add("Books", this.books.Size.Width - 4, HorizontalAlignment.Left);
			this.contacts.View = View.Details;
			this.contacts.Columns.Add("Contacts", this.contacts.Size.Width - 4, HorizontalAlignment.Left);

			try
			{
				// Put all the address books in the books listview.
				AddressBook.AddressBook defaultBook = manager.OpenDefaultAddressBook();
				IEnumerator addrBooks= manager.GetAddressBooks().GetEnumerator();

				while (addrBooks.MoveNext())
				{
					AddressBook.AddressBook book = (AddressBook.AddressBook)addrBooks.Current;

					// If the address book is the default book then label it as such.
					ListViewItem item;
					if (book.ID == defaultBook.ID)
					{
						item = new ListViewItem("Default", 0);

						// Select the default address book.
						item.Selected = true;
					}
					else
					{
						item = new ListViewItem(book.Name, 0);
					}

					item.Tag = book;
					this.books.Items.Add(item);
				}
			}
			catch (SimiasException ex)
			{
				ex.LogFatal();
				MessageBox.Show("A fatal error occurred during initialization.  Please see the log file for additional information.", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}
			catch (Exception ex)
			{
				logger.Fatal(ex, "Initializing");
				MessageBox.Show("A fatal error occurred during initialization.  Please see the log file for additional information.", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}
		}

		private void books_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// An address book was selected/unselected.
			if (this.books.SelectedItems.Count > 0)
			{
				// Get the address book that was selected, since the listview is
				// single select the collection will only contain one item.
				addressBook = (Novell.AddressBook.AddressBook)books.SelectedItems[0].Tag;

				// Populate the contact list view.
				FilterList(null);
			}

			// Fire the BookSelected event.
			EventArgs args = new EventArgs();
			if (BookSelected != null)
			{
				BookSelected(this, args);
			}
		}

		private void contacts_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// A contact was selected/unselected.
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
			// When the splitter is moved, resize the column headers.
			this.contacts.Columns[0].Width = this.contacts.Size.Width - 4;
			this.books.Columns[0].Width = this.books.Size.Width - 4;
		}

		private void createContact_Click(object sender, System.EventArgs e)
		{
			// Invoke the contact editor dialog.
			ContactEditor editor = new ContactEditor();
			editor.LoadPath = LoadPath;
			editor.CurrentAddressBook = this.addressBook;
			DialogResult result = editor.ShowDialog();
			if (result == DialogResult.OK)
			{
				// Add the new contact to the contacts listview.
				AddContactToListView(editor.CurrentContact, 1, true);
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
				try
				{
					Novell.AddressBook.AddressBook addrBook = new Novell.AddressBook.AddressBook(addBook.Name);
					this.manager.AddAddressBook(addrBook);
					ListViewItem item = new ListViewItem(addrBook.Name, 0);
					item.Tag = addrBook;
					this.books.Items.Add(item);
				}
				catch (SimiasException ex)
				{
					ex.LogError();
					MessageBox.Show("An error occurred while creating the address book.  Please see the log file for additional information.", "Create Address Book Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				catch (Exception ex)
				{
					logger.Debug(ex, "Creating address book");
					MessageBox.Show("An error occurred while creating the address book.  Please see the log file for additional information.", "Create Address Book Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
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
				editor.LoadPath = LoadPath;
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
					try
					{
						contact.Delete();
						lvitem.Remove();
					}
					catch (SimiasException ex)
					{
						ex.LogError();
						MessageBox.Show("An error occurred while deleting the contact.  Please see the log file for additional information.", "Delete Contact Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
					catch (Exception ex)
					{
						logger.Debug(ex, "Deleting contact");
						MessageBox.Show("An error occurred while deleting the contact.  Please see the log file for additional information.", "Delete Contact Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
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
						try
						{
							book.Delete();
							lvitem.Remove();
						}
						catch (SimiasException ex)
						{
							ex.LogError();
							MessageBox.Show("An error occurred while deleting the address book.  Please see the log file for additional information.", "Delete Address Book Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						}
						catch (Exception ex)
						{
							logger.Debug(ex, "Deleting address book");
							MessageBox.Show("An error occurred while deleting the address book.  Please see the log file for additional information.", "Delete Address Book Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						}
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

		private void contacts_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				deleteContact_Click(this, null);
			}
		}

		private void books_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				deleteBook_Click(this, null);
			}
		}
		#endregion
	}

	/// <summary>
	/// Event args used by the Contact Selected event.
	/// </summary>
	public class ContactSelectedEventArgs : EventArgs
	{
		private bool validSelected;
		private bool singleSelected;

		/// <summary>
		/// Initializes a new instance of the ContactSelectedEventArgs class.
		/// </summary>
		/// <param name="validSelected">Indicates if the selected contact can be placed in the picked list.</param>
		/// <param name="singleSelected">Indicates if there is only a single contact selected.</param>
		public ContactSelectedEventArgs(bool validSelected, bool singleSelected)
		{
			this.validSelected = validSelected;
			this.singleSelected = singleSelected;
		}

		#region Properties
		/// <summary>
		/// Gets a value indicating if the selected contact can be placed in the picked list.
		/// </summary>
		public bool ValidSelected
		{
			get
			{
				return validSelected;
			}
		}

		/// <summary>
		/// Gets a value indicating if a single contact is selected.
		/// </summary>
		public bool SingleSelected
		{
			get
			{
				return singleSelected;
			}
		}
		#endregion
	}

	/// <summary>
	/// Event args used by the Contact Double Clicked event.
	/// </summary>
	public class ContactDoubleClickedEventArgs : EventArgs
	{
		private Contact contact;
		private Novell.AddressBook.AddressBook addressBook;
		private ListViewItem lvitem;

		/// <summary>
		/// Initializes a new instance of the ContactDoubleClickedEventArgs class.
		/// </summary>
		/// <param name="addressBook">The book that contains the selected contact.</param>
		/// <param name="contact">The contact associated with the selected item.</param>
		/// <param name="lvitem">The item that was double-clicked</param>
		public ContactDoubleClickedEventArgs(Novell.AddressBook.AddressBook addressBook, Contact contact, ListViewItem lvitem)
		{
			this.contact = contact;
			this.addressBook = addressBook;
			this.lvitem = lvitem;
		}

		#region Properties
		/// <summary>
		/// Gets the selected contact.
		/// </summary>
		public Contact Contact
		{
			get
			{
				return contact;
			}
		}

		/// <summary>
		/// Gets the selected address book.
		/// </summary>
		public Novell.AddressBook.AddressBook AddressBook
		{
			get
			{
				return addressBook;
			}
		}

		/// <summary>
		/// Gets the selected listview item.
		/// </summary>
		public ListViewItem LVitem
		{
			get
			{
				return lvitem;
			}
		}
		#endregion
	}
}
