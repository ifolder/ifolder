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
using System.IO;
using System.Diagnostics;
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
		private System.Windows.Forms.MenuItem menuHelpContents;
		private System.Windows.Forms.MenuItem menuHelpAbout;
		private ArrayList selectedContacts;
		private Contact contact;
		private System.Windows.Forms.Panel detailsView;
		private System.Windows.Forms.Splitter splitter1;
		private Novell.iFolder.FormsBookLib.BooksContacts booksContacts;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.TextBox search;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ToolBar toolBar1;
		private System.Windows.Forms.ToolBarButton toolBarButton1;
		private System.Windows.Forms.ToolBarButton toolBarButton2;
		private System.Windows.Forms.ToolBarButton toolBarButton3;
		private System.Windows.Forms.ToolBarButton toolBarButton4;
		private System.Windows.Forms.ToolBarButton toolBarButton5;
		private System.Windows.Forms.Timer editSearchTimer;
		private System.ComponentModel.IContainer components;
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

			// Load the application icon.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"res\address_app.ico"));
			}
			catch{}

			this.booksContacts.ContactSelected += new Novell.iFolder.FormsBookLib.BooksContacts.ContactSelectedDelegate(booksContacts_ContactSelected);
			this.booksContacts.BookSelected += new Novell.iFolder.FormsBookLib.BooksContacts.BookSelectedDelegate(booksContacts_BookSelected);
			this.detailsView.Paint += new PaintEventHandler(detailsView_Paint);

			// Disable the vCard import and export buttons.
			toolBar1.Buttons[3].Enabled = false;
			toolBar1.Buttons[4].Enabled = false;
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
			this.menuHelpContents = new System.Windows.Forms.MenuItem();
			this.menuHelpAbout = new System.Windows.Forms.MenuItem();
			this.detailsView = new System.Windows.Forms.Panel();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.booksContacts = new Novell.iFolder.FormsBookLib.BooksContacts();
			this.panel1 = new System.Windows.Forms.Panel();
			this.search = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.toolBar1 = new System.Windows.Forms.ToolBar();
			this.toolBarButton1 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton2 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton3 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton4 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton5 = new System.Windows.Forms.ToolBarButton();
			this.editSearchTimer = new System.Windows.Forms.Timer(this.components);
			this.panel1.SuspendLayout();
			this.SuspendLayout();
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
			this.menuFileNewAddressBook.Click += new System.EventHandler(this.menuFileNewAddressBook_Click);
			// 
			// menuFileNewContact
			// 
			this.menuFileNewContact.Index = 1;
			this.menuFileNewContact.Text = "&Contact";
			this.menuFileNewContact.Click += new System.EventHandler(this.menuFileNewContact_Click);
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
			this.menuFileExit.Click += new System.EventHandler(this.menuFileExit_Click);
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
			this.menuEdit.Popup += new System.EventHandler(this.menuEdit_Popup);
			// 
			// menuEditContact
			// 
			this.menuEditContact.Index = 0;
			this.menuEditContact.Text = "&Contact";
			this.menuEditContact.Click += new System.EventHandler(this.menuEditContact_Click);
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
			this.menuEditDelete.Click += new System.EventHandler(this.menuEditDelete_Click);
			// 
			// menuTools
			// 
			this.menuTools.Index = 2;
			this.menuTools.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuToolsImportVCard,
																					  this.menuToolsExportVCard});
			this.menuTools.Text = "&Tools";
			this.menuTools.Popup += new System.EventHandler(this.menuTools_Popup);
			// 
			// menuToolsImportVCard
			// 
			this.menuToolsImportVCard.Index = 0;
			this.menuToolsImportVCard.Text = "&Import vCard";
			this.menuToolsImportVCard.Select += new System.EventHandler(this.menuToolsImportVCard_Select);
			// 
			// menuToolsExportVCard
			// 
			this.menuToolsExportVCard.Index = 1;
			this.menuToolsExportVCard.Text = "E&xport vCard";
			this.menuToolsExportVCard.Select += new System.EventHandler(this.menuToolsExportVCard_Select);
			// 
			// menuHelp
			// 
			this.menuHelp.Index = 3;
			this.menuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuHelpContents,
																					 this.menuHelpAbout});
			this.menuHelp.Text = "&Help";
			// 
			// menuHelpContents
			// 
			this.menuHelpContents.Index = 0;
			this.menuHelpContents.Text = "&Contents";
			this.menuHelpContents.Click += new System.EventHandler(this.menuHelpContents_Click);
			// 
			// menuHelpAbout
			// 
			this.menuHelpAbout.Index = 1;
			this.menuHelpAbout.Text = "&About";
			// 
			// detailsView
			// 
			this.detailsView.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.detailsView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.detailsView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailsView.Location = new System.Drawing.Point(307, 0);
			this.detailsView.Name = "detailsView";
			this.detailsView.Size = new System.Drawing.Size(341, 384);
			this.detailsView.TabIndex = 1;
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(304, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 384);
			this.splitter1.TabIndex = 3;
			this.splitter1.TabStop = false;
			// 
			// booksContacts
			// 
			this.booksContacts.Dock = System.Windows.Forms.DockStyle.Left;
			this.booksContacts.Filter = null;
			this.booksContacts.LoadPath = null;
			this.booksContacts.Location = new System.Drawing.Point(0, 0);
			this.booksContacts.Name = "booksContacts";
			this.booksContacts.Size = new System.Drawing.Size(304, 384);
			this.booksContacts.TabIndex = 2;
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.Controls.Add(this.detailsView);
			this.panel1.Controls.Add(this.splitter1);
			this.panel1.Controls.Add(this.booksContacts);
			this.panel1.Location = new System.Drawing.Point(8, 88);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(648, 384);
			this.panel1.TabIndex = 1;
			// 
			// search
			// 
			this.search.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.search.Location = new System.Drawing.Point(56, 56);
			this.search.Name = "search";
			this.search.Size = new System.Drawing.Size(600, 20);
			this.search.TabIndex = 3;
			this.search.Text = "";
			this.search.TextChanged += new System.EventHandler(this.search_TextChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 56);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 4;
			this.label1.Text = "Search:";
			// 
			// toolBar1
			// 
			this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						this.toolBarButton1,
																						this.toolBarButton2,
																						this.toolBarButton3,
																						this.toolBarButton4,
																						this.toolBarButton5});
			this.toolBar1.DropDownArrows = true;
			this.toolBar1.Location = new System.Drawing.Point(0, 0);
			this.toolBar1.Name = "toolBar1";
			this.toolBar1.ShowToolTips = true;
			this.toolBar1.Size = new System.Drawing.Size(664, 42);
			this.toolBar1.TabIndex = 5;
			this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
			// 
			// toolBarButton1
			// 
			this.toolBarButton1.Text = "New Book";
			this.toolBarButton1.ToolTipText = "Create Address Book";
			// 
			// toolBarButton2
			// 
			this.toolBarButton2.Text = "New Group";
			this.toolBarButton2.ToolTipText = "Create Group";
			// 
			// toolBarButton3
			// 
			this.toolBarButton3.Text = "New Contact";
			this.toolBarButton3.ToolTipText = "Create Contact";
			// 
			// toolBarButton4
			// 
			this.toolBarButton4.Text = "Import vCard";
			// 
			// toolBarButton5
			// 
			this.toolBarButton5.Text = "Export vCard";
			// 
			// editSearchTimer
			// 
			this.editSearchTimer.Interval = 500;
			this.editSearchTimer.Tick += new System.EventHandler(this.editSearchTimer_Tick);
			// 
			// FormsAddrBook
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(664, 481);
			this.Controls.Add(this.toolBar1);
			this.Controls.Add(this.search);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.panel1);
			this.Menu = this.mainMenu1;
			this.MinimumSize = new System.Drawing.Size(544, 480);
			this.Name = "FormsAddrBook";
			this.Text = "iFolder Address Book";
			this.Load += new System.EventHandler(this.FormsAddrBook_Load);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		[STAThread]
		static void Main() 
		{
			Application.Run(new FormsAddrBook());
		}

		#region Private Methods
		private void importVCard()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();

			openFileDialog.Filter = "vcf files (*.vcf)|*.vcf";
			openFileDialog.RestoreDirectory = true ;

			if(openFileDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					Cursor.Current = Cursors.WaitCursor;
					Contact contact = selectedBook.ImportVCard(openFileDialog.FileName);
					Cursor.Current = Cursors.Default;
					booksContacts.AddContactToListView(contact, 1, true);
				}
				catch{}
			}
		}

		private void exportVCard()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();

			saveFileDialog.Filter = "vcf files (*.vcf)|*.vcf";
			saveFileDialog.RestoreDirectory = true;

			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					Cursor.Current = Cursors.WaitCursor;
					contact.ExportVCard(saveFileDialog.FileName);
					Cursor.Current = Cursors.Default;
				}
				catch{}
			}
		}
		#endregion

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

		private void menuHelpContents_Click(object sender, System.EventArgs e)
		{
			// TODO - need to use locale-specific path
			string helpPath = Path.Combine(Application.StartupPath, @"help\en\doc\user\data\front.html");

			try
			{
				Process.Start(helpPath);
			}
			catch
			{
				MessageBox.Show("Unable to open help file: \n" + helpPath, "Help File Not Found");
			}
		}

		private void booksContacts_ContactSelected(object sender, ContactSelectedEventArgs e)
		{
			if (e.singleSelected)
			{
				contact = (Contact)((ListViewItem)booksContacts.SelectedContacts[0]).Tag;

				// Enable the vCard export button.
				toolBar1.Buttons[4].Enabled = true;
			}
			else
			{
				contact = null;

				// Disable the vCard export button.
				toolBar1.Buttons[4].Enabled = false;
			}

			detailsView.Invalidate();
		}

		private void booksContacts_BookSelected(object sender, EventArgs e)
		{
			selectedBook = booksContacts.SelectedAddressBook;
			toolBar1.Buttons[3].Enabled = selectedBook != null;
		}

		private void detailsView_Paint(object sender, PaintEventArgs e)
		{
			if (contact != null)
			{
				// Display the photo.
				Image image;
				Rectangle rect = new Rectangle(8, 8, 60, 75);

				float x = 75;
				float y = 8;
				int delta = 10;
				try
				{
					image = Image.FromStream(contact.ExportPhoto());
					e.Graphics.DrawImage(image, rect);
				}
				catch
				{
					try
					{
						// There was a problem loading the image ... use the default image.
						// Get the base path.
						string basePath = Path.Combine(Application.StartupPath, "res");

						image = Image.FromFile(Path.Combine(basePath, "blankhead.png"));
						e.Graphics.DrawImage(image, rect);
					}
					catch{}
				}

				// Display the full name.
				Font boldFont = new Font(Font.FontFamily, 14, FontStyle.Bold);
				e.Graphics.DrawString(contact.FN, boldFont, SystemBrushes.WindowText, x, y);
				y += boldFont.Size + delta;

				// Display the title.
				e.Graphics.DrawString(contact.Title, Font, SystemBrushes.WindowText, x, y);

				y = 90;
				try
				{
					// Display the phone numbers.
					StringFormat format = new StringFormat(StringFormatFlags.DirectionRightToLeft);
					foreach(Telephone phone in contact.GetTelephoneNumbers())
					{
						if ((phone.Types & (PhoneTypes.work | PhoneTypes.voice)) == (PhoneTypes.work | PhoneTypes.voice))
						{
							e.Graphics.DrawString("work", Font, SystemBrushes.ControlDark, x-1, y, format);
							e.Graphics.DrawString(phone.Number, Font, SystemBrushes.WindowText, x, y);
							y += Font.Size + 5;
						}
					}

					foreach(Telephone phone in contact.GetTelephoneNumbers())
					{
						if ((phone.Types & (PhoneTypes.work | PhoneTypes.fax)) == (PhoneTypes.work | PhoneTypes.fax))
						{
							e.Graphics.DrawString("fax", Font, SystemBrushes.ControlDark, x-1, y, format);
							e.Graphics.DrawString(phone.Number, Font, SystemBrushes.WindowText, x, y);
							y += Font.Size + 5;
						}
					}

					foreach(Telephone phone in contact.GetTelephoneNumbers())
					{
						if ((phone.Types & PhoneTypes.cell) == PhoneTypes.cell)
						{
							e.Graphics.DrawString("mobile", Font, SystemBrushes.ControlDark, x-1, y, format);
							e.Graphics.DrawString(phone.Number, Font, SystemBrushes.WindowText, x, y);
							y += Font.Size + 5;
						}
					}

					foreach(Telephone phone in contact.GetTelephoneNumbers())
					{
						if ((phone.Types & PhoneTypes.pager) == PhoneTypes.pager)
						{
							e.Graphics.DrawString("pager", Font, SystemBrushes.ControlDark, x-1, y, format);
							e.Graphics.DrawString(phone.Number, Font, SystemBrushes.WindowText, x, y);
							y += Font.Size + 5;
						}
					}

					foreach(Telephone phone in contact.GetTelephoneNumbers())
					{
						if ((phone.Types & (PhoneTypes.home | PhoneTypes.voice)) == (PhoneTypes.home | PhoneTypes.voice))
						{
							e.Graphics.DrawString("home", Font, SystemBrushes.ControlDark, x-1, y, format);
							e.Graphics.DrawString(phone.Number, Font, SystemBrushes.WindowText, x, y);
							y += Font.Size + 5;
						}
					}

					// Display the e-mail addresses.
					y += delta;
					foreach(Email email in contact.GetEmailAddresses())
					{
						if ((email.Types & EmailTypes.work) == EmailTypes.work)
						{
							e.Graphics.DrawString("work", Font, SystemBrushes.ControlDark, x-1, y, format);
							e.Graphics.DrawString(email.Address, Font, SystemBrushes.WindowText, x, y);
							y += Font.Size + 5;
						}
					}

					foreach(Email email in contact.GetEmailAddresses())
					{
						if ((email.Types & EmailTypes.personal) == EmailTypes.personal)
						{
							e.Graphics.DrawString("home", Font, SystemBrushes.ControlDark, x-1, y, format);
							e.Graphics.DrawString(email.Address, Font, SystemBrushes.WindowText, x, y);
							y += Font.Size + 5;
						}
					}

					foreach(Email email in contact.GetEmailAddresses())
					{
						if ((email.Types & EmailTypes.other) == EmailTypes.other)
						{
							e.Graphics.DrawString("other", Font, SystemBrushes.ControlDark, x-1, y, format);
							e.Graphics.DrawString(email.Address, Font, SystemBrushes.WindowText, x, y);
							y += Font.Size + 5;
						}
					}

					// Display the mailing addresses.
					y += delta;
					foreach(Address addr in contact.GetAddresses())
					{
						if((addr.Types & AddressTypes.work) == AddressTypes.work)
						{
							e.Graphics.DrawString("work", Font, SystemBrushes.ControlDark, x-1, y, format);
							e.Graphics.DrawString(addr.Street, Font, SystemBrushes.WindowText, x, y);
							y += Font.Size + 5;
							if (addr.ExtendedAddress != "")
							{
								e.Graphics.DrawString(addr.ExtendedAddress, Font, SystemBrushes.WindowText, x, y);
								y += Font.Size + 5;
							}
							
							if (addr.PostalBox != "")
							{
								e.Graphics.DrawString("P.O. Box " + addr.PostalBox, Font, SystemBrushes.WindowText, x, y);
								y += Font.Size + 5;
							}
							
							if (addr.Locality != "" ||
								addr.Region != "" ||
								addr.PostalCode != "")
							{
								string cityState = addr.Locality + ", " + addr.Region + " " + addr.PostalCode;
								e.Graphics.DrawString(cityState.Trim(), Font, SystemBrushes.WindowText, x, y);
								y += Font.Size + 5;
							}

							if (addr.Country != "")
							{
								e.Graphics.DrawString(addr.Country, Font, SystemBrushes.WindowText, x, y);
								y += Font.Size + 5;
							}

							y += delta;
						}
					}

					foreach(Address addr in contact.GetAddresses())
					{
						if((addr.Types & AddressTypes.home) == AddressTypes.home)
						{
							e.Graphics.DrawString("home", Font, SystemBrushes.ControlDark, x-1, y, format);
							e.Graphics.DrawString(addr.Street, Font, SystemBrushes.WindowText, x, y);
							y += Font.Size + 5;
							if (addr.ExtendedAddress != "")
							{
								e.Graphics.DrawString(addr.ExtendedAddress, Font, SystemBrushes.WindowText, x, y);
								y += Font.Size + 5;
							}
							
							if (addr.PostalBox != "")
							{
								e.Graphics.DrawString("P.O. Box " + addr.PostalBox, Font, SystemBrushes.WindowText, x, y);
								y += Font.Size + 5;
							}
							
							if (addr.Locality != "" ||
								addr.Region != "" ||
								addr.PostalCode != "")
							{
								string cityState = addr.Locality + ", " + addr.Region + " " + addr.PostalCode;
								e.Graphics.DrawString(cityState.Trim(), Font, SystemBrushes.WindowText, x, y);
								y += Font.Size + 5;
							}

							if (addr.Country != "")
							{
								e.Graphics.DrawString(addr.Country, Font, SystemBrushes.WindowText, x, y);
								y += Font.Size + 5;
							}

							y += delta;
						}
					}
				}
				catch{}
			}
			else
			{
				// Clear the display.
				e.Graphics.Clear(Color.White);
			}
		}

		private void editSearchTimer_Tick(object sender, System.EventArgs e)
		{
			// The timer event has been fired...
			// Stop the timer.
			editSearchTimer.Stop();

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
				case "Import vCard":
					importVCard();
					break;
				case "Export vCard":
					exportVCard();
					break;
				default:
					break;
			}
		}

		private void FormsAddrBook_Load(object sender, System.EventArgs e)
		{
			toolBar1.ImageList = this.booksContacts.ContactImageList;
			toolBar1.Buttons[0].ImageIndex = 2;
			toolBar1.Buttons[1].ImageIndex = 1;
			toolBar1.Buttons[2].ImageIndex = 1;
			toolBar1.Buttons[3].ImageIndex = 3;
			toolBar1.Buttons[4].ImageIndex = 4;
		}

		private void menuTools_Popup(object sender, System.EventArgs e)
		{
			selectedContacts = booksContacts.SelectedContacts;
			menuToolsExportVCard.Enabled = selectedContacts.Count == 1;

			menuToolsImportVCard.Enabled = selectedBook != null;
		}

		private void menuToolsImportVCard_Select(object sender, System.EventArgs e)
		{
			importVCard();
		}

		private void menuToolsExportVCard_Select(object sender, System.EventArgs e)
		{
			exportVCard();
		}
		#endregion
	}
}
