/***********************************************************************
 *  ContactPicker.cs - A contact picker implemented using Windows.Forms
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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Novell.AddressBook;

namespace Novell.iFolder.FormsBookLib
{
	/// <summary>
	/// Summary description for ContactPicker.
	/// </summary>
	public class ContactPicker : System.Windows.Forms.Form
	{
		#region Class Members
		private System.Windows.Forms.Button add;
		private System.Windows.Forms.Button remove;
		private System.Windows.Forms.ListView addedContacts;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Button edit;

		private Novell.AddressBook.AddressBook addressBook = null;
		private Novell.AddressBook.Manager manager = null;
		private ArrayList contactList;
		private Novell.iFolder.FormsBookLib.BooksContacts booksContacts1;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		public ContactPicker()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			addedContacts.View = View.List;
			this.add.Enabled = false;
			this.remove.Enabled = false;
			this.edit.Enabled = false;
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.add = new System.Windows.Forms.Button();
			this.remove = new System.Windows.Forms.Button();
			this.addedContacts = new System.Windows.Forms.ListView();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.edit = new System.Windows.Forms.Button();
			this.booksContacts1 = new Novell.iFolder.FormsBookLib.BooksContacts();
			this.SuspendLayout();
			// 
			// add
			// 
			this.add.Location = new System.Drawing.Point(336, 128);
			this.add.Name = "add";
			this.add.TabIndex = 2;
			this.add.Text = "Add >";
			this.add.Click += new System.EventHandler(this.add_Click);
			// 
			// remove
			// 
			this.remove.Location = new System.Drawing.Point(336, 160);
			this.remove.Name = "remove";
			this.remove.TabIndex = 3;
			this.remove.Text = "< Remove";
			this.remove.Click += new System.EventHandler(this.remove_Click);
			// 
			// addedContacts
			// 
			this.addedContacts.Location = new System.Drawing.Point(432, 56);
			this.addedContacts.Name = "addedContacts";
			this.addedContacts.Size = new System.Drawing.Size(152, 304);
			this.addedContacts.TabIndex = 6;
			this.addedContacts.DoubleClick += new System.EventHandler(this.addedContacts_DoubleClick);
			this.addedContacts.SelectedIndexChanged += new System.EventHandler(this.addedContacts_SelectedIndexChanged);
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Location = new System.Drawing.Point(432, 368);
			this.ok.Name = "ok";
			this.ok.TabIndex = 7;
			this.ok.Text = "OK";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(512, 368);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 8;
			this.cancel.Text = "Cancel";
			// 
			// edit
			// 
			this.edit.Location = new System.Drawing.Point(336, 216);
			this.edit.Name = "edit";
			this.edit.TabIndex = 9;
			this.edit.Text = "Edit...";
			this.edit.Click += new System.EventHandler(this.edit_Click);
			// 
			// booksContacts1
			// 
			this.booksContacts1.Location = new System.Drawing.Point(8, 0);
			this.booksContacts1.Name = "booksContacts1";
			this.booksContacts1.Size = new System.Drawing.Size(304, 400);
			this.booksContacts1.TabIndex = 10;
			this.booksContacts1.ContactDoubleClicked += new Novell.iFolder.FormsBookLib.BooksContacts.ContactDoubleClickedDelegate(this.booksContacts1_ContactDoubleClicked);
			this.booksContacts1.ContactSelected += new Novell.iFolder.FormsBookLib.BooksContacts.ContactSelectedDelegate(this.booksContacts1_ContactSelected);
			// 
			// ContactPicker
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(594, 400);
			this.Controls.Add(this.booksContacts1);
			this.Controls.Add(this.edit);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.addedContacts);
			this.Controls.Add(this.remove);
			this.Controls.Add(this.add);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ContactPicker";
			this.ShowInTaskbar = false;
			this.Text = "Contact Picker";
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		public ArrayList GetContactList
		{
			get
			{
				return contactList;
			}
		}

		public Manager CurrentManager
		{
			set
			{
				this.manager = value;
				this.booksContacts1.CurrentManager = value;
			}
		}
		#endregion

		#region Event Handlers
		private void add_Click(object sender, System.EventArgs e)
		{
			IEnumerator contactEnumerator = this.booksContacts1.ValidSelectedContacts.GetEnumerator();
			while (contactEnumerator.MoveNext())
			{
				ListViewItem item = (ListViewItem)contactEnumerator.Current;
				ListViewItem addedItem = new ListViewItem(item.Text);
				addedItem.Tag = item;
				addedContacts.Items.Add(addedItem);
				item.ForeColor = Color.Gray;
			}

			this.add.Enabled = false;
		}

		private void booksContacts1_ContactDoubleClicked(object sender, ContactDoubleClickedEventArgs e)
		{
			add_Click(sender, e);
		}

		private void remove_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem item in addedContacts.SelectedItems)
			{
				((ListViewItem)item.Tag).ForeColor = Color.Black;
				addedContacts.Items.Remove(item);
			}
		}

		private void edit_Click(object sender, System.EventArgs e)
		{
			ContactEditor editor = new ContactEditor();
			IEnumerator contactEnumerator = this.booksContacts1.SelectedContacts.GetEnumerator();
			ListViewItem item = null;
			while (contactEnumerator.MoveNext())
			{
				item = (ListViewItem)contactEnumerator.Current;
			}

			if (item != null)
			{
				editor.CurrentContact = (Contact)item.Tag;
				editor.CurrentAddressBook = this.addressBook;
				DialogResult result = editor.ShowDialog();
				if (result == DialogResult.OK)
				{
					Name name = editor.CurrentContact.GetPreferredName();
					item.Text = name.Given + " " + name.Family;
				}
			}
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			contactList = new ArrayList();
			foreach (ListViewItem item in this.addedContacts.Items)
			{
				contactList.Add(((ListViewItem)item.Tag).Tag);
			}
			Close();
		}

		private void addedContacts_DoubleClick(object sender, EventArgs e)
		{
			remove_Click(sender, e);
		}

		private void newBook_Click(object sender, System.EventArgs e)
		{
			AddBook addBook = new AddBook();
			DialogResult result = addBook.ShowDialog();
			if (result == DialogResult.OK)
			{
				// Create address book and add it to the list.
				Novell.AddressBook.AddressBook addrBook = new Novell.AddressBook.AddressBook(addBook.Name);
				this.manager.AddAddressBook(addrBook);
				ListViewItem item = new ListViewItem(addrBook.Name);
				item.Tag = addrBook;
			}
		}

		private void booksContacts1_ContactSelected(object sender, ContactSelectedEventArgs e)
		{
			this.add.Enabled = e.validSelected;
			this.edit.Enabled = e.singleSelected;
		}

		private void addedContacts_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.remove.Enabled = addedContacts.SelectedItems.Count > 0;
		}
		#endregion
	}
}
