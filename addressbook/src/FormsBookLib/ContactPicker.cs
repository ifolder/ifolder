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
		private System.Windows.Forms.ListView addedContacts;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;

		private Novell.AddressBook.AddressBook addressBook = null;
		private Novell.AddressBook.Manager manager = null;
		private ArrayList contactList;
		private Novell.iFolder.FormsBookLib.BooksContacts booksContacts;
		private System.Windows.Forms.Button remove;
		private System.Windows.Forms.TextBox search;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ToolBar toolBar1;
		private System.Windows.Forms.ToolBarButton newBook;
		private System.Windows.Forms.ToolBarButton newGroup;
		private System.Windows.Forms.ToolBarButton newContact;
		private System.Windows.Forms.Timer editSearchTimer;
		private System.Windows.Forms.GroupBox groupBox1;
		private int fixedWidth;
		private double booksContactsRatio;
		private System.ComponentModel.IContainer components;
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
			this.ok.Enabled = false;

			// This is the size of the fixed area of the dialog ...
			// Add button width
			// +8 (width of border)
			// +8 (width on left side)
			// +8 (width on right side)
			// +16 (width between listview and right side of Add)
			// +16 (width between left side of Add and listview)
			fixedWidth = add.Size.Width + 56;

			// Calculate the size ratio of the booksContacts listview.
			booksContactsRatio = (double)booksContacts.Size.Width / (double)(this.Size.Width - fixedWidth);
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
			this.components = new System.ComponentModel.Container();
			this.add = new System.Windows.Forms.Button();
			this.addedContacts = new System.Windows.Forms.ListView();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.booksContacts = new Novell.iFolder.FormsBookLib.BooksContacts();
			this.remove = new System.Windows.Forms.Button();
			this.search = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.toolBar1 = new System.Windows.Forms.ToolBar();
			this.newBook = new System.Windows.Forms.ToolBarButton();
			this.newGroup = new System.Windows.Forms.ToolBarButton();
			this.newContact = new System.Windows.Forms.ToolBarButton();
			this.editSearchTimer = new System.Windows.Forms.Timer(this.components);
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.SuspendLayout();
			// 
			// add
			// 
			this.add.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.add.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.add.Location = new System.Drawing.Point(328, 120);
			this.add.Name = "add";
			this.add.TabIndex = 2;
			this.add.Text = "Add ->";
			this.add.Click += new System.EventHandler(this.add_Click);
			// 
			// addedContacts
			// 
			this.addedContacts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.addedContacts.Location = new System.Drawing.Point(416, 88);
			this.addedContacts.Name = "addedContacts";
			this.addedContacts.Size = new System.Drawing.Size(176, 352);
			this.addedContacts.TabIndex = 6;
			this.addedContacts.KeyDown += new System.Windows.Forms.KeyEventHandler(this.addedContacts_KeyDown);
			this.addedContacts.DoubleClick += new System.EventHandler(this.addedContacts_DoubleClick);
			this.addedContacts.SelectedIndexChanged += new System.EventHandler(this.addedContacts_SelectedIndexChanged);
			// 
			// ok
			// 
			this.ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ok.Location = new System.Drawing.Point(438, 462);
			this.ok.Name = "ok";
			this.ok.TabIndex = 7;
			this.ok.Text = "OK";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancel.Location = new System.Drawing.Point(518, 462);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 8;
			this.cancel.Text = "Cancel";
			// 
			// booksContacts
			// 
			this.booksContacts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left)));
			this.booksContacts.Filter = null;
			this.booksContacts.LoadPath = null;
			this.booksContacts.Location = new System.Drawing.Point(8, 88);
			this.booksContacts.Name = "booksContacts";
			this.booksContacts.Size = new System.Drawing.Size(304, 352);
			this.booksContacts.TabIndex = 11;
			this.booksContacts.ContactDoubleClicked += new Novell.iFolder.FormsBookLib.BooksContacts.ContactDoubleClickedDelegate(this.booksContacts_ContactDoubleClicked);
			this.booksContacts.ContactSelected += new Novell.iFolder.FormsBookLib.BooksContacts.ContactSelectedDelegate(this.booksContacts_ContactSelected);
			// 
			// remove
			// 
			this.remove.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.remove.Location = new System.Drawing.Point(328, 152);
			this.remove.Name = "remove";
			this.remove.TabIndex = 12;
			this.remove.Text = "Remove";
			this.remove.Click += new System.EventHandler(this.remove_Click);
			// 
			// search
			// 
			this.search.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.search.Location = new System.Drawing.Point(56, 56);
			this.search.Name = "search";
			this.search.Size = new System.Drawing.Size(536, 20);
			this.search.TabIndex = 13;
			this.search.Text = "";
			this.search.TextChanged += new System.EventHandler(this.search_TextChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 56);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 14;
			this.label1.Text = "Search:";
			// 
			// toolBar1
			// 
			this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						this.newBook,
																						this.newGroup,
																						this.newContact});
			this.toolBar1.DropDownArrows = true;
			this.toolBar1.Location = new System.Drawing.Point(0, 0);
			this.toolBar1.Name = "toolBar1";
			this.toolBar1.ShowToolTips = true;
			this.toolBar1.Size = new System.Drawing.Size(600, 42);
			this.toolBar1.TabIndex = 15;
			this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
			// 
			// newBook
			// 
			this.newBook.Text = "New Book";
			this.newBook.ToolTipText = "Create address book";
			// 
			// newGroup
			// 
			this.newGroup.Text = "New Group";
			this.newGroup.ToolTipText = "Create group";
			// 
			// newContact
			// 
			this.newContact.Text = "New Contact";
			this.newContact.ToolTipText = "Create contact";
			// 
			// editSearchTimer
			// 
			this.editSearchTimer.Interval = 500;
			this.editSearchTimer.Tick += new System.EventHandler(this.editSearchTimer_Tick);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Location = new System.Drawing.Point(4, 448);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(592, 4);
			this.groupBox1.TabIndex = 16;
			this.groupBox1.TabStop = false;
			// 
			// ContactPicker
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(600, 494);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.toolBar1);
			this.Controls.Add(this.search);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.remove);
			this.Controls.Add(this.addedContacts);
			this.Controls.Add(this.add);
			this.Controls.Add(this.booksContacts);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(576, 528);
			this.Name = "ContactPicker";
			this.ShowInTaskbar = false;
			this.Text = "Contact Picker";
			this.SizeChanged += new System.EventHandler(this.ContactPicker_SizeChanged);
			this.Load += new System.EventHandler(this.ContactPicker_Load);
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
				this.booksContacts.CurrentManager = value;
			}
		}

		public string LoadPath
		{
			get
			{
				return this.booksContacts.LoadPath;
			}

			set
			{
				this.booksContacts.LoadPath = value;
			}
		}
		#endregion

		#region Event Handlers
		private void add_Click(object sender, System.EventArgs e)
		{
			IEnumerator contactEnumerator = booksContacts.ValidSelectedContacts.GetEnumerator();
			while (contactEnumerator.MoveNext())
			{
				ListViewItem item = (ListViewItem)contactEnumerator.Current;
				ListViewItem addedItem = new ListViewItem(item.Text);
				addedItem.ImageIndex = item.ImageIndex;
				addedItem.Tag = item;
				addedContacts.Items.Add(addedItem);
				item.ForeColor = Color.Gray;
			}

			this.ok.Enabled = addedContacts.Items.Count > 0;
			this.add.Enabled = false;
		}

		private void booksContacts_ContactDoubleClicked(object sender, ContactDoubleClickedEventArgs e)
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

			this.ok.Enabled = addedContacts.Items.Count > 0;
		}

		private void edit_Click(object sender, System.EventArgs e)
		{
			ContactEditor editor = new ContactEditor();
			editor.LoadPath = LoadPath;
			IEnumerator contactEnumerator = booksContacts.SelectedContacts.GetEnumerator();
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

		private void booksContacts_ContactSelected(object sender, ContactSelectedEventArgs e)
		{
			this.add.Enabled = e.validSelected;
		}

		private void addedContacts_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.remove.Enabled = addedContacts.SelectedItems.Count > 0;
		}

		private void ContactPicker_Load(object sender, System.EventArgs e)
		{
			this.addedContacts.SmallImageList = this.booksContacts.ContactImageList;
			this.toolBar1.ImageList = addedContacts.SmallImageList;
			toolBar1.Buttons[0].ImageIndex = 2;
			toolBar1.Buttons[1].ImageIndex = 1;
			toolBar1.Buttons[2].ImageIndex = 1;
		}

		private void addedContacts_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				foreach (ListViewItem item in addedContacts.SelectedItems)
				{
					((ListViewItem)item.Tag).ForeColor = Color.Black;
					addedContacts.Items.Remove(item);
				}

				this.ok.Enabled = addedContacts.Items.Count > 0;
			}
		}

		private void editSearchTimer_Tick(object sender, System.EventArgs e)
		{
			// The timer event has been fired...
			// Stop the timer.
			this.editSearchTimer.Stop();

			// Filter the contact list view.
			booksContacts.FilterList(search.Text);
		}

		private void search_TextChanged(object sender, System.EventArgs e)
		{
			// Reset the timer when search text is entered.
			this.editSearchTimer.Stop();
			this.editSearchTimer.Start();
		}

		private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch (e.Button.Text)
			{
				case "New Book":
					booksContacts.CreateAddressBook();
					break;
				case "New Group":
					break;
				case "New Contact":
					booksContacts.CreateContact();
					break;
				default:
					break;
			}
		}

		private void ContactPicker_SizeChanged(object sender, System.EventArgs e)
		{
			// Relocate the divider line when the size is changed.
			Point point = groupBox1.Location;
			point.Y = ok.Location.Y - 12;
			groupBox1.Location = point;


			// Resize listviews
			int x = this.Size.Width - fixedWidth;
			Size booksContactsSize = booksContacts.Size;
			Size pickedSize = addedContacts.Size;

			// Calculate the new widths based on the initial ratios.
			booksContactsSize.Width = (int)(x * booksContactsRatio);
			pickedSize.Width = (int)(x * (1 - booksContactsRatio));

			booksContacts.Size = booksContactsSize;
			addedContacts.Size = pickedSize;

			// Relocate the add button.
			point = add.Location;
			point.X = booksContacts.Location.X + booksContacts.Size.Width + 16;
			add.Location = point;

			// Relocate the remove button.
			point = remove.Location;
			point.X = add.Location.X;
			remove.Location = point;

			// Relocate the addedContacts listview.
			point = addedContacts.Location;
			point.X = this.Size.Width - addedContacts.Size.Width - 16;
			addedContacts.Location = point;
		}
		#endregion
	}
}
