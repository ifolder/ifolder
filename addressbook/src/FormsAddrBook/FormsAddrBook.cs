/***********************************************************************
 *  FormsAddrBook.cs - An address book implemented using Windows.Forms
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
using Novell.iFolder.FormsBookLib;
using Novell.AddressBook;

namespace Novell.iFolder.FormsAddrBook
{
	/// <summary>
	/// Summary description for FormsAddrBook.
	/// </summary>
	public class FormsAddrBook : System.Windows.Forms.Form
	{
		#region Class Members
		private BooksContacts booksContacts = null;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ListView listView1;
		private Novell.AddressBook.Manager manager = null;
		private Novell.AddressBook.AddressBook selectedBook;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuFile;
		private System.Windows.Forms.MenuItem menuFileNew;
		private System.Windows.Forms.MenuItem menuFileExit;
		private System.Windows.Forms.MenuItem menuEdit;
		private System.Windows.Forms.MenuItem menuEditContact;
		private System.Windows.Forms.MenuItem menuFileNewAddressBook;
		private System.Windows.Forms.MenuItem menuFileNewContact;
		private System.Windows.Forms.MenuItem menuEditDelete;
		private System.Windows.Forms.MenuItem menuFileDivider;
		private System.Windows.Forms.MenuItem menuEditDivider;
		private System.Windows.Forms.MenuItem menuEditCut;
		private System.Windows.Forms.MenuItem menuEditCopy;
		private System.Windows.Forms.MenuItem menuEditPaste;
		private System.Windows.Forms.MenuItem menuTools;
		private System.Windows.Forms.MenuItem menuToolsImportVCard;
		private System.Windows.Forms.MenuItem menuToolsExportVCard;
		private System.Windows.Forms.MenuItem menuHelp;
		private System.Windows.Forms.MenuItem menuHelpAbout;
		private ArrayList selectedContacts;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		public FormsAddrBook()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			manager = Manager.Connect();
			this.booksContacts.CurrentManager = manager;
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
			this.panel1 = new System.Windows.Forms.Panel();
			this.booksContacts = new Novell.iFolder.FormsBookLib.BooksContacts();
			this.listView1 = new System.Windows.Forms.ListView();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuFile = new System.Windows.Forms.MenuItem();
			this.menuFileNew = new System.Windows.Forms.MenuItem();
			this.menuFileNewAddressBook = new System.Windows.Forms.MenuItem();
			this.menuFileNewContact = new System.Windows.Forms.MenuItem();
			this.menuFileDivider = new System.Windows.Forms.MenuItem();
			this.menuFileExit = new System.Windows.Forms.MenuItem();
			this.menuEdit = new System.Windows.Forms.MenuItem();
			this.menuEditContact = new System.Windows.Forms.MenuItem();
			this.menuEditDivider = new System.Windows.Forms.MenuItem();
			this.menuEditCut = new System.Windows.Forms.MenuItem();
			this.menuEditCopy = new System.Windows.Forms.MenuItem();
			this.menuEditPaste = new System.Windows.Forms.MenuItem();
			this.menuEditDelete = new System.Windows.Forms.MenuItem();
			this.menuTools = new System.Windows.Forms.MenuItem();
			this.menuToolsImportVCard = new System.Windows.Forms.MenuItem();
			this.menuToolsExportVCard = new System.Windows.Forms.MenuItem();
			this.menuHelp = new System.Windows.Forms.MenuItem();
			this.menuHelpAbout = new System.Windows.Forms.MenuItem();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.Controls.Add(this.booksContacts);
			this.panel1.Controls.Add(this.listView1);
			this.panel1.Location = new System.Drawing.Point(8, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(552, 400);
			this.panel1.TabIndex = 1;
			// 
			// booksContacts
			// 
			this.booksContacts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left)));
 			this.booksContacts.Location = new System.Drawing.Point(0, 0);
			this.booksContacts.Name = "booksContacts";
			this.booksContacts.Size = new System.Drawing.Size(304, 400);
			this.booksContacts.TabIndex = 0;
			this.booksContacts.ContactDoubleClicked += new Novell.iFolder.FormsBookLib.BooksContacts.ContactDoubleClickedDelegate(this.booksContacts_ContactDoubleClicked);
			// 
			// listView1
			// 
			this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.listView1.Location = new System.Drawing.Point(307, 56);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(237, 304);
			this.listView1.TabIndex = 2;
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuFile,
																					  this.menuEdit,
																					  this.menuTools,
																					  this.menuHelp});
			// 
			// menuFile
			// 
			this.menuFile.Index = 0;
			this.menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuFileNew,
																					 this.menuFileDivider,
																					 this.menuFileExit});
			this.menuFile.Text = "&File";
			// 
			// menuFileNew
			// 
			this.menuFileNew.Index = 0;
			this.menuFileNew.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						this.menuFileNewAddressBook,
																						this.menuFileNewContact});
			this.menuFileNew.Text = "&New";
			// 
			// menuFileNewAddressBook
			// 
			this.menuFileNewAddressBook.Index = 0;
			this.menuFileNewAddressBook.Text = "Address &Book";
			this.menuFileNewAddressBook.Click += new EventHandler(menuFileNewAddressBook_Click);
			// 
			// menuFileNewContact
			// 
			this.menuFileNewContact.Index = 1;
			this.menuFileNewContact.Text = "&Contact";
			this.menuFileNewContact.Click += new EventHandler(menuFileNewContact_Click);
			// 
			// menuFileDivider
			// 
			this.menuFileDivider.Index = 1;
			this.menuFileDivider.Text = "-";
			// 
			// menuFileExit
			// 
			this.menuFileExit.Index = 2;
			this.menuFileExit.Text = "E&xit";
			this.menuFileExit.Click += new EventHandler(menuFileExit_Click);
			// 
			// menuEdit
			// 
			this.menuEdit.Index = 1;
			this.menuEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuEditContact,
																					 this.menuEditDivider,
																					 this.menuEditCut,
																					 this.menuEditCopy,
																					 this.menuEditPaste,
																					 this.menuEditDelete});
			this.menuEdit.Text = "&Edit";
			this.menuEdit.Popup += new EventHandler(menuEdit_Popup);
			// 
			// menuEditContact
			// 
			this.menuEditContact.Index = 0;
			this.menuEditContact.Text = "&Contact";
			this.menuEditContact.Click += new EventHandler(menuEditContact_Click);
			// 
			// menuEditDivider
			// 
			this.menuEditDivider.Index = 1;
			this.menuEditDivider.Text = "-";
			// 
			// menuEditCut
			// 
			this.menuEditCut.Index = 2;
			this.menuEditCut.Text = "Cu&t";
			// 
			// menuEditCopy
			// 
			this.menuEditCopy.Index = 3;
			this.menuEditCopy.Text = "&Copy";
			// 
			// menuEditPaste
			// 
			this.menuEditPaste.Index = 4;
			this.menuEditPaste.Text = "&Paste";
			// 
			// menuEditDelete
			// 
			this.menuEditDelete.Index = 5;
			this.menuEditDelete.Text = "&Delete";
			this.menuEditDelete.Click += new EventHandler(menuEditDelete_Click);
			// 
			// menuTools
			// 
			this.menuTools.Index = 2;
			this.menuTools.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuToolsImportVCard,
																					  this.menuToolsExportVCard});
			this.menuTools.Text = "&Tools";
			// 
			// menuToolsImportVCard
			// 
			this.menuToolsImportVCard.Index = 0;
			this.menuToolsImportVCard.Text = "&Import vCard";
			// 
			// menuToolsExportVCard
			// 
			this.menuToolsExportVCard.Index = 1;
			this.menuToolsExportVCard.Text = "E&xport vCard";
			// 
			// menuHelp
			// 
			this.menuHelp.Index = 3;
			this.menuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuHelpAbout});
			this.menuHelp.Text = "&Help";
			// 
			// menuHelpAbout
			// 
			this.menuHelpAbout.Index = 0;
			this.menuHelpAbout.Text = "&About";
			// 
			// FormsAddrBook
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(560, 401);
			this.Controls.Add(this.panel1);
			this.Menu = this.mainMenu1;
			this.Name = "FormsAddrBook";
			this.Text = "Denali Address Book";
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		[STAThread]
		static void Main() 
		{
			Application.Run(new FormsAddrBook());
		}

		#region Event Handlers
		private void booksContacts_ContactDoubleClicked(object sender, ContactDoubleClickedEventArgs e)
		{
			ContactEditor editor = new ContactEditor();
			editor.CurrentAddressBook = e.addressBook;
			editor.CurrentContact = e.contact;
			DialogResult result = editor.ShowDialog();
			if (result == DialogResult.OK)
			{
				if (e.lvitem != null)
				{
					e.lvitem.Text = editor.CurrentContact.FN;
				}
			}
		}

		private void menuFileNewAddressBook_Click(object sender, EventArgs e)
		{
			this.booksContacts.CreateAddressBook();
		}

		private void menuFileNewContact_Click(object sender, EventArgs e)
		{
			this.booksContacts.CreateContact();
		}

		private void menuEdit_Popup(object sender, EventArgs e)
		{
			selectedContacts = this.booksContacts.SelectedContacts;
			this.menuEditContact.Enabled = selectedContacts.Count == 1;

			if (selectedContacts.Count > 0)
			{
				this.menuEditDelete.Enabled = true;
			}
			else
			{
				selectedBook = this.booksContacts.SelectedAddressBook;
				this.menuEditDelete.Enabled = selectedBook != null;
			}
		}

		private void menuEditContact_Click(object sender, EventArgs e)
		{
			this.booksContacts.EditContact();
		}

		private void menuEditDelete_Click(object sender, EventArgs e)
		{
			if (selectedContacts.Count > 0)
			{
				this.booksContacts.DeleteContact();
			}
			else if (selectedBook != null)
			{
				this.booksContacts.DeleteAddressBook();
			}
		}

		private void menuFileExit_Click(object sender, EventArgs e)
		{
            this.Close();
		}
		#endregion
	}
}
