/***********************************************************************
 *  ContactEditor.cs - A contact editor implemented using Windows.Forms
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
using System.IO;
using Simias.Storage;
using Novell.AddressBook;

namespace Novell.iFolder.FormsBookLib
{
	/// <summary>
	/// Summary description for ContactEditor.
	/// </summary>
	public class ContactEditor : System.Windows.Forms.Form
	{
		#region Class Members
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TextBox userId;
		private System.Windows.Forms.TextBox firstName;
		private System.Windows.Forms.TextBox lastName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox phone3;
		private System.Windows.Forms.TextBox phone2;
		private System.Windows.Forms.TextBox phone1;
		private System.Windows.Forms.TextBox organization;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.ComboBox phoneSelect2;
		private System.Windows.Forms.ComboBox phoneSelect1;
		private System.Windows.Forms.TextBox phone4;
		private System.Windows.Forms.ComboBox phoneSelect4;
		private System.Windows.Forms.ComboBox phoneSelect3;
		private System.Windows.Forms.CheckBox mailHTML;
		private System.Windows.Forms.ComboBox addressSelect;
		private System.Windows.Forms.CheckBox mailAddress;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TextBox webAddress;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.TextBox categories;
		private System.Windows.Forms.Button categoriesButton;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.ListBox address;
		private System.Windows.Forms.TextBox eMail;
		private System.Windows.Forms.PictureBox pictureMail;
		private System.Windows.Forms.PictureBox pictureContact;
		private System.Windows.Forms.PictureBox picturePhone;
		private System.Windows.Forms.PictureBox pictureAddress;
		private System.Windows.Forms.PictureBox pictureWeb;
		private System.Windows.Forms.PictureBox pictureCategories;

		private Novell.AddressBook.AddressBook addressBook = null;
		private Contact contact = null;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		public ContactEditor()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			try
			{
				// Load the images.
				// TODO - get these from the correct path based on install location.
				string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Novell\Denali Client\res");

				pictureContact.Image = Image.FromFile(Path.Combine(basePath, "blankhead.png"));
				picturePhone.Image = Image.FromFile(Path.Combine(basePath, "cellphone.png"));
				pictureMail.Image = Image.FromFile(Path.Combine(basePath, "ico-mail.png"));
				pictureAddress.Image = Image.FromFile(Path.Combine(basePath, "house.png"));
				pictureWeb.Image = Image.FromFile(Path.Combine(basePath, "globe.png"));
				pictureCategories.Image = Image.FromFile(Path.Combine(basePath, "briefcase.png"));
			}
			catch{}
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
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.pictureCategories = new System.Windows.Forms.PictureBox();
			this.categoriesButton = new System.Windows.Forms.Button();
			this.categories = new System.Windows.Forms.TextBox();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.pictureWeb = new System.Windows.Forms.PictureBox();
			this.webAddress = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.mailAddress = new System.Windows.Forms.CheckBox();
			this.addressSelect = new System.Windows.Forms.ComboBox();
			this.pictureAddress = new System.Windows.Forms.PictureBox();
			this.address = new System.Windows.Forms.ListBox();
			this.mailHTML = new System.Windows.Forms.CheckBox();
			this.phoneSelect3 = new System.Windows.Forms.ComboBox();
			this.phoneSelect4 = new System.Windows.Forms.ComboBox();
			this.picturePhone = new System.Windows.Forms.PictureBox();
			this.phoneSelect1 = new System.Windows.Forms.ComboBox();
			this.phoneSelect2 = new System.Windows.Forms.ComboBox();
			this.organization = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.phone4 = new System.Windows.Forms.TextBox();
			this.phone3 = new System.Windows.Forms.TextBox();
			this.phone2 = new System.Windows.Forms.TextBox();
			this.phone1 = new System.Windows.Forms.TextBox();
			this.pictureMail = new System.Windows.Forms.PictureBox();
			this.eMail = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.pictureContact = new System.Windows.Forms.PictureBox();
			this.lastName = new System.Windows.Forms.TextBox();
			this.firstName = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.userId = new System.Windows.Forms.TextBox();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.SuspendLayout();
			//
			// tabControl1
			//
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(648, 336);
			this.tabControl1.TabIndex = 0;
			//
			// tabPage1
			//
			this.tabPage1.Controls.Add(this.pictureCategories);
			this.tabPage1.Controls.Add(this.categoriesButton);
			this.tabPage1.Controls.Add(this.categories);
			this.tabPage1.Controls.Add(this.groupBox5);
			this.tabPage1.Controls.Add(this.groupBox6);
			this.tabPage1.Controls.Add(this.groupBox4);
			this.tabPage1.Controls.Add(this.pictureWeb);
			this.tabPage1.Controls.Add(this.webAddress);
			this.tabPage1.Controls.Add(this.label4);
			this.tabPage1.Controls.Add(this.groupBox2);
			this.tabPage1.Controls.Add(this.mailAddress);
			this.tabPage1.Controls.Add(this.addressSelect);
			this.tabPage1.Controls.Add(this.pictureAddress);
			this.tabPage1.Controls.Add(this.address);
			this.tabPage1.Controls.Add(this.mailHTML);
			this.tabPage1.Controls.Add(this.phoneSelect3);
			this.tabPage1.Controls.Add(this.phoneSelect4);
			this.tabPage1.Controls.Add(this.picturePhone);
			this.tabPage1.Controls.Add(this.phoneSelect1);
			this.tabPage1.Controls.Add(this.phoneSelect2);
			this.tabPage1.Controls.Add(this.organization);
			this.tabPage1.Controls.Add(this.label7);
			this.tabPage1.Controls.Add(this.phone4);
			this.tabPage1.Controls.Add(this.phone3);
			this.tabPage1.Controls.Add(this.phone2);
			this.tabPage1.Controls.Add(this.phone1);
			this.tabPage1.Controls.Add(this.pictureMail);
			this.tabPage1.Controls.Add(this.eMail);
			this.tabPage1.Controls.Add(this.label9);
			this.tabPage1.Controls.Add(this.groupBox1);
			this.tabPage1.Controls.Add(this.pictureContact);
			this.tabPage1.Controls.Add(this.lastName);
			this.tabPage1.Controls.Add(this.firstName);
			this.tabPage1.Controls.Add(this.label3);
			this.tabPage1.Controls.Add(this.label2);
			this.tabPage1.Controls.Add(this.label1);
			this.tabPage1.Controls.Add(this.userId);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(640, 310);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Collaboration";
			//
			// pictureCategories
			//
			this.pictureCategories.Location = new System.Drawing.Point(8, 256);
			this.pictureCategories.Name = "pictureCategories";
			this.pictureCategories.Size = new System.Drawing.Size(48, 48);
			this.pictureCategories.TabIndex = 53;
			this.pictureCategories.TabStop = false;
			//
			// categoriesButton
			//
			this.categoriesButton.Location = new System.Drawing.Point(64, 264);
			this.categoriesButton.Name = "categoriesButton";
			this.categoriesButton.Size = new System.Drawing.Size(96, 23);
			this.categoriesButton.TabIndex = 7;
			this.categoriesButton.Text = "Categories...";
			//
			// categories
			//
			this.categories.Location = new System.Drawing.Point(168, 264);
			this.categories.Name = "categories";
			this.categories.Size = new System.Drawing.Size(160, 20);
			this.categories.TabIndex = 8;
			this.categories.Text = "";
			//
			// groupBox5
			//
			this.groupBox5.Location = new System.Drawing.Point(334, 248);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(296, 4);
			this.groupBox5.TabIndex = 50;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "groupBox5";
			//
			// groupBox6
			//
			this.groupBox6.Location = new System.Drawing.Point(8, 248);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(310, 4);
			this.groupBox6.TabIndex = 49;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "groupBox6";
			//
			// groupBox4
			//
			this.groupBox4.Location = new System.Drawing.Point(334, 112);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(296, 4);
			this.groupBox4.TabIndex = 48;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "groupBox4";
			//
			// pictureWeb
			//
			this.pictureWeb.Location = new System.Drawing.Point(8, 184);
			this.pictureWeb.Name = "pictureWeb";
			this.pictureWeb.Size = new System.Drawing.Size(48, 48);
			this.pictureWeb.TabIndex = 46;
			this.pictureWeb.TabStop = false;
			//
			// webAddress
			//
			this.webAddress.Location = new System.Drawing.Point(168, 192);
			this.webAddress.Name = "webAddress";
			this.webAddress.Size = new System.Drawing.Size(160, 20);
			this.webAddress.TabIndex = 6;
			this.webAddress.Text = "";
			//
			// label4
			//
			this.label4.Location = new System.Drawing.Point(64, 192);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(104, 16);
			this.label4.TabIndex = 45;
			this.label4.Text = "Web page address:";
			//
			// groupBox2
			//
			this.groupBox2.Location = new System.Drawing.Point(8, 176);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(310, 4);
			this.groupBox2.TabIndex = 43;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "groupBox2";
			//
			// mailAddress
			//
			this.mailAddress.Location = new System.Drawing.Point(472, 224);
			this.mailAddress.Name = "mailAddress";
			this.mailAddress.Size = new System.Drawing.Size(160, 16);
			this.mailAddress.TabIndex = 19;
			this.mailAddress.Text = "This is the mailing address";
			//
			// addressSelect
			//
			this.addressSelect.BackColor = System.Drawing.SystemColors.Control;
			this.addressSelect.Items.AddRange(new object[] {
															   "Business:",
															   "Home:"});
			this.addressSelect.Location = new System.Drawing.Point(384, 120);
			this.addressSelect.Name = "addressSelect";
			this.addressSelect.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.addressSelect.Size = new System.Drawing.Size(88, 21);
			this.addressSelect.TabIndex = 17;
			this.addressSelect.Text = "Business:";
			//
			// pictureAddress
			//
			this.pictureAddress.Location = new System.Drawing.Point(328, 128);
			this.pictureAddress.Name = "pictureAddress";
			this.pictureAddress.Size = new System.Drawing.Size(48, 48);
			this.pictureAddress.TabIndex = 40;
			this.pictureAddress.TabStop = false;
			//
			// address
			//
			this.address.Location = new System.Drawing.Point(472, 120);
			this.address.Name = "address";
			this.address.Size = new System.Drawing.Size(160, 95);
			this.address.TabIndex = 18;
			//
			// mailHTML
			//
			this.mailHTML.Location = new System.Drawing.Point(168, 152);
			this.mailHTML.Name = "mailHTML";
			this.mailHTML.Size = new System.Drawing.Size(168, 16);
			this.mailHTML.TabIndex = 5;
			this.mailHTML.Text = "Wants to receive HTML mail";
			//
			// phoneSelect3
			//
			this.phoneSelect3.BackColor = System.Drawing.SystemColors.Control;
			this.phoneSelect3.Items.AddRange(new object[] {
															  "Business:",
															  "Business fax:",
															  "Mobile:",
															  "Home:",
															  "Home fax:"});
			this.phoneSelect3.Location = new System.Drawing.Point(384, 56);
			this.phoneSelect3.Name = "phoneSelect3";
			this.phoneSelect3.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.phoneSelect3.Size = new System.Drawing.Size(88, 21);
			this.phoneSelect3.TabIndex = 13;
			this.phoneSelect3.Text = "Home:";
			//
			// phoneSelect4
			//
			this.phoneSelect4.BackColor = System.Drawing.SystemColors.Control;
			this.phoneSelect4.Items.AddRange(new object[] {
															  "Business:",
															  "Business fax:",
															  "Mobile:",
															  "Home:",
															  "Home fax:"});
			this.phoneSelect4.Location = new System.Drawing.Point(384, 80);
			this.phoneSelect4.Name = "phoneSelect4";
			this.phoneSelect4.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.phoneSelect4.Size = new System.Drawing.Size(88, 21);
			this.phoneSelect4.TabIndex = 15;
			this.phoneSelect4.Text = "Mobile:";
			//
			// picturePhone
			//
			this.picturePhone.Location = new System.Drawing.Point(328, 16);
			this.picturePhone.Name = "picturePhone";
			this.picturePhone.Size = new System.Drawing.Size(48, 48);
			this.picturePhone.TabIndex = 35;
			this.picturePhone.TabStop = false;
			//
			// phoneSelect1
			//
			this.phoneSelect1.BackColor = System.Drawing.SystemColors.Control;
			this.phoneSelect1.Items.AddRange(new object[] {
															  "Business:",
															  "Business fax:",
															  "Mobile:",
															  "Home:",
															  "Home fax:"});
			this.phoneSelect1.Location = new System.Drawing.Point(384, 8);
			this.phoneSelect1.Name = "phoneSelect1";
			this.phoneSelect1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.phoneSelect1.Size = new System.Drawing.Size(88, 21);
			this.phoneSelect1.TabIndex = 9;
			this.phoneSelect1.Text = "Business:";
			//
			// phoneSelect2
			//
			this.phoneSelect2.BackColor = System.Drawing.SystemColors.Control;
			this.phoneSelect2.Items.AddRange(new object[] {
															  "Business:",
															  "Business fax:",
															  "Mobile:",
															  "Home:",
															  "Home fax:"});
			this.phoneSelect2.Location = new System.Drawing.Point(384, 32);
			this.phoneSelect2.Name = "phoneSelect2";
			this.phoneSelect2.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.phoneSelect2.Size = new System.Drawing.Size(88, 21);
			this.phoneSelect2.TabIndex = 11;
			this.phoneSelect2.Text = "Business fax:";
			//
			// organization
			//
			this.organization.Location = new System.Drawing.Point(168, 80);
			this.organization.Name = "organization";
			this.organization.Size = new System.Drawing.Size(160, 20);
			this.organization.TabIndex = 3;
			this.organization.Text = "";
			//
			// label7
			//
			this.label7.Location = new System.Drawing.Point(96, 80);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(72, 16);
			this.label7.TabIndex = 30;
			this.label7.Text = "Organization:";
			//
			// phone4
			//
			this.phone4.Location = new System.Drawing.Point(472, 80);
			this.phone4.Name = "phone4";
			this.phone4.Size = new System.Drawing.Size(160, 20);
			this.phone4.TabIndex = 16;
			this.phone4.Text = "";
			//
			// phone3
			//
			this.phone3.Location = new System.Drawing.Point(472, 56);
			this.phone3.Name = "phone3";
			this.phone3.Size = new System.Drawing.Size(160, 20);
			this.phone3.TabIndex = 14;
			this.phone3.Text = "";
			//
			// phone2
			//
			this.phone2.Location = new System.Drawing.Point(472, 32);
			this.phone2.Name = "phone2";
			this.phone2.Size = new System.Drawing.Size(160, 20);
			this.phone2.TabIndex = 12;
			this.phone2.Text = "";
			//
			// phone1
			//
			this.phone1.Location = new System.Drawing.Point(472, 8);
			this.phone1.Name = "phone1";
			this.phone1.Size = new System.Drawing.Size(160, 20);
			this.phone1.TabIndex = 10;
			this.phone1.Text = "";
			//
			// pictureMail
			//
			this.pictureMail.Location = new System.Drawing.Point(8, 120);
			this.pictureMail.Name = "pictureMail";
			this.pictureMail.Size = new System.Drawing.Size(48, 48);
			this.pictureMail.TabIndex = 21;
			this.pictureMail.TabStop = false;
			//
			// eMail
			//
			this.eMail.Location = new System.Drawing.Point(168, 128);
			this.eMail.Name = "eMail";
			this.eMail.Size = new System.Drawing.Size(160, 20);
			this.eMail.TabIndex = 4;
			this.eMail.Text = "";
			//
			// label9
			//
			this.label9.Location = new System.Drawing.Point(96, 128);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(80, 16);
			this.label9.TabIndex = 20;
			this.label9.Text = "Primary e-mail:";
			//
			// groupBox1
			//
			this.groupBox1.Location = new System.Drawing.Point(8, 112);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(310, 4);
			this.groupBox1.TabIndex = 18;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "groupBox1";
			//
			// pictureContact
			//
			this.pictureContact.Location = new System.Drawing.Point(8, 16);
			this.pictureContact.Name = "pictureContact";
			this.pictureContact.Size = new System.Drawing.Size(48, 48);
			this.pictureContact.TabIndex = 17;
			this.pictureContact.TabStop = false;
			//
			// lastName
			//
			this.lastName.Location = new System.Drawing.Point(168, 56);
			this.lastName.Name = "lastName";
			this.lastName.Size = new System.Drawing.Size(160, 20);
			this.lastName.TabIndex = 2;
			this.lastName.Text = "";
			//
			// firstName
			//
			this.firstName.Location = new System.Drawing.Point(168, 32);
			this.firstName.Name = "firstName";
			this.firstName.Size = new System.Drawing.Size(160, 20);
			this.firstName.TabIndex = 1;
			this.firstName.Text = "";
			//
			// label3
			//
			this.label3.Location = new System.Drawing.Point(104, 56);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 16);
			this.label3.TabIndex = 6;
			this.label3.Text = "Last Name:";
			//
			// label2
			//
			this.label2.Location = new System.Drawing.Point(104, 32);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 16);
			this.label2.TabIndex = 5;
			this.label2.Text = "First Name:";
			//
			// label1
			//
			this.label1.Location = new System.Drawing.Point(120, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(48, 16);
			this.label1.TabIndex = 4;
			this.label1.Text = "User ID:";
			//
			// userId
			//
			this.userId.Location = new System.Drawing.Point(168, 8);
			this.userId.Name = "userId";
			this.userId.Size = new System.Drawing.Size(160, 20);
			this.userId.TabIndex = 0;
			this.userId.Text = "";
			//
			// ok
			//
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Location = new System.Drawing.Point(476, 344);
			this.ok.Name = "ok";
			this.ok.TabIndex = 1;
			this.ok.Text = "OK";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			//
			// cancel
			//
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(560, 344);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 2;
			this.cancel.Text = "Cancel";
			//
			// ContactEditor
			//
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(648, 374);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.tabControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ContactEditor";
			this.ShowInTaskbar = false;
			this.Text = "Contact Editor";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.ContactEditor_Closing);
			this.Load += new System.EventHandler(this.ContactEditor_Load);
			this.Activated += new System.EventHandler(this.ContactEditor_Activated);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the contact that is being edited.
		/// </summary>
		public Contact CurrentContact
		{
			get
			{
				return contact;
			}

			set
			{
				contact = value;
			}
		}

		/// <summary>
		/// Sets the address book that the contact belongs to.
		/// </summary>
		public Novell.AddressBook.AddressBook CurrentAddressBook
		{
			set
			{
				this.addressBook = value;
			}
		}
		#endregion

		#region Private Methods
		private bool LoadAddresses()
		{
			try
			{
				foreach(Email tmpMail in contact.GetEmailAddresses())
				{
					if ((tmpMail.Types & EmailTypes.work) == EmailTypes.work)
					{
						eMail.Text = tmpMail.Address;
					}
				}
			}
			catch{}

			// Deal with phone numbers
/*			try
			{
				foreach(Telephone tmpPhone in contact.GetTelephoneNumbers())
				{
					if ((tmpPhone.Types & PhoneTypes.work) == PhoneTypes.work)
					{
						bPhoneEdit.Text = tmpPhone.Number;
						foundWorkPhone = true;
					}
					else
						if ((tmpPhone.Types & PhoneTypes.home) == PhoneTypes.home)
					{
						hPhoneEdit.Text = tmpPhone.Number;
						foundHomePhone = true;
					}
				}
			}
			catch{}

			foreach(Address addr in contact.GetAddresses())
			{
				if((addr.Types & AddressTypes.work) == AddressTypes.work)
				{
					bStreetEdit.Text = addr.Street;
					bCityEdit.Text = addr.Locality;
					bStateEdit.Text = addr.Region;
					bZipEdit.Text = addr.PostalCode;
					bCountryEdit.Text = addr.Country;
					foundWork = true;
				}
				else
					if((addr.Types & AddressTypes.home) == AddressTypes.home)
				{
					hStreetEdit.Text = addr.Street;
					hCityEdit.Text = addr.Locality;
					hStateEdit.Text = addr.Region;
					hZipEdit.Text = addr.PostalCode;
					hCountryEdit.Text = addr.Country;
					foundHome = true;
				}
			}*/

			return(true);
		}
		#endregion

		#region Event Handlers
		private void ContactEditor_Load(object sender, EventArgs e)
		{
			if (contact != null)
			{
				// Initialize the dialog with the specified contact.
				userId.Text = this.contact.UserName;

				try
				{
					Name prefName = this.contact.GetPreferredName();
					firstName.Text = prefName.Given;
					lastName.Text = prefName.Family;
				}
				catch
				{
					firstName.Text = "";
					lastName.Text = "";
				}

				bool results = LoadAddresses();
			}
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			string	username = null;
			string	first = null;
			string	last = null;
			string	email = null;

			username = userId.Text;
			first = firstName.Text;
			last = lastName.Text;
			email = eMail.Text;

			if (username.Trim() != "" &&
				first.Trim() != "" &&
				last.Trim() != "" &&
				email.Trim() != "")
			{
				if (contact == null)
				{
					// This is a new contact.
					contact = new Contact();
					contact.UserName = username;

					//addressBook.AddContact(contact);

					//abContactList.CreateContact(username);
					//contact.EMail = email;
					if (first != null && last != null)
					{
						try
						{
							Name prefName = new Name(first, last);
							prefName.Preferred = true;
							contact.AddName(prefName);
							//Name prefName = contact.CreateName(first, last, true);
						}
						catch{}
					}

					if (eMail.Text != null && eMail.Text != "")
					{
						Email tmpMail = new Email((EmailTypes.internet | EmailTypes.work), eMail.Text);
						tmpMail.Preferred = true;
						// BUGBUG temp
						tmpMail.Preferred = false;
						tmpMail.Preferred = true;
						contact.AddEmailAddress(tmpMail);
					}

					addressBook.AddContact(contact);
					contact.Commit();
				}
				else
				{
					try
					{
						// First let's just delete all the existing email addresses
						try
						{
							foreach(Email tmpEmail in contact.GetEmailAddresses())
							{
								tmpEmail.Delete();
							}
						}
						catch{}

						if (eMail.Text != null && eMail.Text != "")
						{
							// For the business email address use the helper
							// function directly off the contact
							contact.EMail = eMail.Text;
						}
					}
					catch(System.NullReferenceException)
					{
						//contact.EMail = email;
					}

/*					try
					{
						// First let's just delete all the existing phone numbers
						try
						{
							foreach(Telephone tmpPhone in contact.GetTelephoneNumbers())
							{
								tmpPhone.Delete();
							}
						}
						catch{}

						if (bPhoneEdit.Text != null && bPhoneEdit.Text != "")
						{
							// For the business telephone numbers use the helper function
							// function directly off the contact
							Telephone tmpPhone = new Telephone(bPhoneEdit.Text);
							tmpPhone.Types = (PhoneTypes.preferred | PhoneTypes.work | PhoneTypes.voice);
							contact.AddTelephoneNumber(tmpPhone);
						}

						if (hPhoneEdit.Text != null && hPhoneEdit.Text != "")
						{
							Telephone tmpPhone = new Telephone(hPhoneEdit.Text);
							tmpPhone.Types = (PhoneTypes.home | PhoneTypes.voice);
							contact.AddTelephoneNumber(tmpPhone);
						}
					}
					catch{}*/

					if (first != null && last != null)
					{
						Name prefName;
						try
						{
							prefName = contact.GetPreferredName();
							if (prefName.Given != first)
							{
								prefName.Given = first;
							}

							if (prefName.Family != last)
							{
								prefName.Family = last;
							}
							
							if (prefName.Preferred != true)
							{
								prefName.Preferred = true;
							}
						}
						catch
						{
							try
							{
								prefName = new Name(first, last);
								prefName.Preferred = true;
								contact.AddName(prefName);
							}
							catch{}
						}
					}

					if (contact.UserName != username)
					{
						contact.UserName = username;
					}

					contact.Commit();
				}
			}
		}

		private void ContactEditor_Closing(object sender, CancelEventArgs e)
		{
			string user = this.userId.Text.Trim();
			string first = this.firstName.Text.Trim();
			string last = this.lastName.Text.Trim();
			string email = this.eMail.Text.Trim();

			// Make sure the mandatory fields are filled in if OK was clicked.
			if ((user == "" ||
				first == "" ||
				last == "" ||
				email == "") &&
				this.DialogResult == DialogResult.OK)
			{
				MessageBox.Show("User ID, first name, last name, and e-mail are required attributes.", "Missing Required Attributes", MessageBoxButtons.OK);

				// Set the focus to the edit field that needs filled in.
				if (user == "")
				{
					this.userId.Focus();
				}
				else if (first == "")
				{
					this.firstName.Focus();
				}
				else if (last == "")
				{
					this.lastName.Focus();
				}
				else
				{
					this.eMail.Focus();
				}
				
				// Don't dismiss the dialog.
				e.Cancel = true;
			}
		}

		private void ContactEditor_Activated(object sender, EventArgs e)
		{
			// Set focus to the first edit box on the form.
			this.userId.Focus();
		}
		#endregion
	}
}
