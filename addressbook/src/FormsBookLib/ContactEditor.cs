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
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox phone3;
		private System.Windows.Forms.TextBox phone2;
		private System.Windows.Forms.TextBox phone1;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.ComboBox phoneSelect2;
		private System.Windows.Forms.ComboBox phoneSelect1;
		private System.Windows.Forms.TextBox phone4;
		private System.Windows.Forms.ComboBox phoneSelect4;
		private System.Windows.Forms.ComboBox phoneSelect3;
		private System.Windows.Forms.ComboBox addressSelect;
		private System.Windows.Forms.CheckBox mailAddress;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TextBox webAddress;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.TextBox eMail;
		private System.Windows.Forms.PictureBox pictureMail;
		private System.Windows.Forms.PictureBox pictureContact;
		private System.Windows.Forms.PictureBox picturePhone;
		private System.Windows.Forms.PictureBox pictureAddress;
		private System.Windows.Forms.PictureBox pictureWeb;
		private Hashtable phoneHT;
		private Hashtable emailHT;
		private Name name;
		private bool nameValidated;
		private AddressEntry homeAddrEntry;
		private AddressEntry workAddrEntry;
		private string loadPath;

		private Novell.AddressBook.AddressBook addressBook = null;
		private Contact contact = null;
		private System.Windows.Forms.Button addr;
		private System.Windows.Forms.TextBox blogAddress;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button fullNameButton;
		private System.Windows.Forms.Button changePicture;
		private System.Windows.Forms.TextBox userId;
		private System.Windows.Forms.TextBox organization;
		private System.Windows.Forms.TextBox fullName;
		private System.Windows.Forms.TextBox jobTitle;
		private System.Windows.Forms.ComboBox emailSelect;
		private System.Windows.Forms.CheckBox preferredEmail;
		private System.Windows.Forms.ListBox addressView;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox department;
		private System.Windows.Forms.TextBox office;
		private System.Windows.Forms.TextBox profession;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TextBox manager;
		private System.Windows.Forms.TextBox assistant;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.TextBox nickname;
		private System.Windows.Forms.TextBox spouse;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.DateTimePicker birthdayPicker;
		private System.Windows.Forms.TextBox birthday;
		private System.Windows.Forms.DateTimePicker anniversaryPicker;
		private System.Windows.Forms.TextBox anniversary;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.TextBox notes;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.TextBox address;

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

			phoneHT = new Hashtable();
			emailHT = new Hashtable();
			homeAddrEntry = new AddressEntry();
			workAddrEntry = new AddressEntry();
			preferredEmail.Enabled = false;
			birthdayPicker.Format = DateTimePickerFormat.Short;
			anniversaryPicker.Format = DateTimePickerFormat.Short;

			// TODO - remove this after rework.
			this.address.Hide();
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
			this.address = new System.Windows.Forms.TextBox();
			this.pictureContact = new System.Windows.Forms.PictureBox();
			this.changePicture = new System.Windows.Forms.Button();
			this.fullNameButton = new System.Windows.Forms.Button();
			this.emailSelect = new System.Windows.Forms.ComboBox();
			this.blogAddress = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.addr = new System.Windows.Forms.Button();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.pictureWeb = new System.Windows.Forms.PictureBox();
			this.webAddress = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.mailAddress = new System.Windows.Forms.CheckBox();
			this.addressSelect = new System.Windows.Forms.ComboBox();
			this.pictureAddress = new System.Windows.Forms.PictureBox();
			this.addressView = new System.Windows.Forms.ListBox();
			this.preferredEmail = new System.Windows.Forms.CheckBox();
			this.phoneSelect3 = new System.Windows.Forms.ComboBox();
			this.phoneSelect4 = new System.Windows.Forms.ComboBox();
			this.picturePhone = new System.Windows.Forms.PictureBox();
			this.phoneSelect1 = new System.Windows.Forms.ComboBox();
			this.phoneSelect2 = new System.Windows.Forms.ComboBox();
			this.userId = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.phone4 = new System.Windows.Forms.TextBox();
			this.phone3 = new System.Windows.Forms.TextBox();
			this.phone2 = new System.Windows.Forms.TextBox();
			this.phone1 = new System.Windows.Forms.TextBox();
			this.pictureMail = new System.Windows.Forms.PictureBox();
			this.eMail = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.organization = new System.Windows.Forms.TextBox();
			this.jobTitle = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.fullName = new System.Windows.Forms.TextBox();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.label15 = new System.Windows.Forms.Label();
			this.notes = new System.Windows.Forms.TextBox();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.anniversary = new System.Windows.Forms.TextBox();
			this.anniversaryPicker = new System.Windows.Forms.DateTimePicker();
			this.birthday = new System.Windows.Forms.TextBox();
			this.birthdayPicker = new System.Windows.Forms.DateTimePicker();
			this.label14 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.spouse = new System.Windows.Forms.TextBox();
			this.label12 = new System.Windows.Forms.Label();
			this.nickname = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.assistant = new System.Windows.Forms.TextBox();
			this.manager = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.profession = new System.Windows.Forms.TextBox();
			this.office = new System.Windows.Forms.TextBox();
			this.department = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Location = new System.Drawing.Point(8, 8);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(736, 312);
			this.tabControl1.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.address);
			this.tabPage1.Controls.Add(this.pictureContact);
			this.tabPage1.Controls.Add(this.changePicture);
			this.tabPage1.Controls.Add(this.fullNameButton);
			this.tabPage1.Controls.Add(this.emailSelect);
			this.tabPage1.Controls.Add(this.blogAddress);
			this.tabPage1.Controls.Add(this.label5);
			this.tabPage1.Controls.Add(this.addr);
			this.tabPage1.Controls.Add(this.groupBox4);
			this.tabPage1.Controls.Add(this.pictureWeb);
			this.tabPage1.Controls.Add(this.webAddress);
			this.tabPage1.Controls.Add(this.label4);
			this.tabPage1.Controls.Add(this.groupBox2);
			this.tabPage1.Controls.Add(this.mailAddress);
			this.tabPage1.Controls.Add(this.addressSelect);
			this.tabPage1.Controls.Add(this.pictureAddress);
			this.tabPage1.Controls.Add(this.addressView);
			this.tabPage1.Controls.Add(this.preferredEmail);
			this.tabPage1.Controls.Add(this.phoneSelect3);
			this.tabPage1.Controls.Add(this.phoneSelect4);
			this.tabPage1.Controls.Add(this.picturePhone);
			this.tabPage1.Controls.Add(this.phoneSelect1);
			this.tabPage1.Controls.Add(this.phoneSelect2);
			this.tabPage1.Controls.Add(this.userId);
			this.tabPage1.Controls.Add(this.label7);
			this.tabPage1.Controls.Add(this.phone4);
			this.tabPage1.Controls.Add(this.phone3);
			this.tabPage1.Controls.Add(this.phone2);
			this.tabPage1.Controls.Add(this.phone1);
			this.tabPage1.Controls.Add(this.pictureMail);
			this.tabPage1.Controls.Add(this.eMail);
			this.tabPage1.Controls.Add(this.groupBox1);
			this.tabPage1.Controls.Add(this.organization);
			this.tabPage1.Controls.Add(this.jobTitle);
			this.tabPage1.Controls.Add(this.label3);
			this.tabPage1.Controls.Add(this.label2);
			this.tabPage1.Controls.Add(this.fullName);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(728, 286);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "General";
			// 
			// address
			// 
			this.address.Location = new System.Drawing.Point(528, 160);
			this.address.Multiline = true;
			this.address.Name = "address";
			this.address.Size = new System.Drawing.Size(192, 95);
			this.address.TabIndex = 52;
			this.address.Text = "";
			this.address.Leave += new System.EventHandler(this.address_Leave);
			this.address.Enter += new System.EventHandler(this.address_Enter);
			// 
			// pictureContact
			// 
			this.pictureContact.Location = new System.Drawing.Point(16, 8);
			this.pictureContact.Name = "pictureContact";
			this.pictureContact.Size = new System.Drawing.Size(60, 75);
			this.pictureContact.TabIndex = 17;
			this.pictureContact.TabStop = false;
			this.pictureContact.DoubleClick += new System.EventHandler(this.pictureContact_DoubleClick);
			// 
			// changePicture
			// 
			this.changePicture.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.changePicture.Location = new System.Drawing.Point(16, 8);
			this.changePicture.Name = "changePicture";
			this.changePicture.Size = new System.Drawing.Size(60, 75);
			this.changePicture.TabIndex = 5;
			this.changePicture.Text = "Photo...";
			this.changePicture.Click += new System.EventHandler(this.changePicture_Click);
			// 
			// fullNameButton
			// 
			this.fullNameButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.fullNameButton.Location = new System.Drawing.Point(92, 14);
			this.fullNameButton.Name = "fullNameButton";
			this.fullNameButton.TabIndex = 0;
			this.fullNameButton.Text = "Full Name...";
			this.fullNameButton.Click += new System.EventHandler(this.fullNameButton_Click);
			// 
			// emailSelect
			// 
			this.emailSelect.BackColor = System.Drawing.SystemColors.Control;
			this.emailSelect.Items.AddRange(new object[] {
															 "Business email:",
															 "Personal email:",
															 "Other email:"});
			this.emailSelect.Location = new System.Drawing.Point(64, 160);
			this.emailSelect.Name = "emailSelect";
			this.emailSelect.Size = new System.Drawing.Size(104, 21);
			this.emailSelect.TabIndex = 6;
			this.emailSelect.Text = "Business email:";
			this.emailSelect.SelectedIndexChanged += new System.EventHandler(this.emailSelect_SelectedIndexChanged);
			// 
			// blogAddress
			// 
			this.blogAddress.Location = new System.Drawing.Point(176, 256);
			this.blogAddress.Name = "blogAddress";
			this.blogAddress.Size = new System.Drawing.Size(192, 20);
			this.blogAddress.TabIndex = 10;
			this.blogAddress.Text = "";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(104, 256);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(80, 16);
			this.label5.TabIndex = 51;
			this.label5.Text = "Blog Address:";
			// 
			// addr
			// 
			this.addr.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.addr.Location = new System.Drawing.Point(424, 192);
			this.addr.Name = "addr";
			this.addr.Size = new System.Drawing.Size(96, 23);
			this.addr.TabIndex = 20;
			this.addr.Text = "Address...";
			this.addr.Click += new System.EventHandler(this.addr_Click);
			// 
			// groupBox4
			// 
			this.groupBox4.Location = new System.Drawing.Point(376, 144);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(344, 4);
			this.groupBox4.TabIndex = 48;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "groupBox4";
			// 
			// pictureWeb
			// 
			this.pictureWeb.Location = new System.Drawing.Point(8, 224);
			this.pictureWeb.Name = "pictureWeb";
			this.pictureWeb.Size = new System.Drawing.Size(48, 48);
			this.pictureWeb.TabIndex = 46;
			this.pictureWeb.TabStop = false;
			// 
			// webAddress
			// 
			this.webAddress.Location = new System.Drawing.Point(176, 224);
			this.webAddress.Name = "webAddress";
			this.webAddress.Size = new System.Drawing.Size(192, 20);
			this.webAddress.TabIndex = 9;
			this.webAddress.Text = "";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(72, 224);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(104, 16);
			this.label4.TabIndex = 45;
			this.label4.Text = "Web page address:";
			// 
			// groupBox2
			// 
			this.groupBox2.Location = new System.Drawing.Point(8, 208);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(362, 4);
			this.groupBox2.TabIndex = 43;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "groupBox2";
			// 
			// mailAddress
			// 
			this.mailAddress.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.mailAddress.Location = new System.Drawing.Point(528, 264);
			this.mailAddress.Name = "mailAddress";
			this.mailAddress.Size = new System.Drawing.Size(160, 16);
			this.mailAddress.TabIndex = 22;
			this.mailAddress.Text = "This is the mailing address";
			// 
			// addressSelect
			// 
			this.addressSelect.BackColor = System.Drawing.SystemColors.Control;
			this.addressSelect.Items.AddRange(new object[] {
															   "Business:",
															   "Home:"});
			this.addressSelect.Location = new System.Drawing.Point(424, 160);
			this.addressSelect.Name = "addressSelect";
			this.addressSelect.Size = new System.Drawing.Size(96, 21);
			this.addressSelect.TabIndex = 19;
			this.addressSelect.Text = "Business:";
			this.addressSelect.SelectedIndexChanged += new System.EventHandler(this.addressSelect_SelectedIndexChanged);
			// 
			// pictureAddress
			// 
			this.pictureAddress.Location = new System.Drawing.Point(374, 160);
			this.pictureAddress.Name = "pictureAddress";
			this.pictureAddress.Size = new System.Drawing.Size(48, 48);
			this.pictureAddress.TabIndex = 40;
			this.pictureAddress.TabStop = false;
			// 
			// addressView
			// 
			this.addressView.Location = new System.Drawing.Point(528, 160);
			this.addressView.Name = "addressView";
			this.addressView.SelectionMode = System.Windows.Forms.SelectionMode.None;
			this.addressView.Size = new System.Drawing.Size(192, 95);
			this.addressView.TabIndex = 21;
			// 
			// preferredEmail
			// 
			this.preferredEmail.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.preferredEmail.Location = new System.Drawing.Point(176, 184);
			this.preferredEmail.Name = "preferredEmail";
			this.preferredEmail.Size = new System.Drawing.Size(168, 16);
			this.preferredEmail.TabIndex = 8;
			this.preferredEmail.Text = "&Preferred email account";
			this.preferredEmail.CheckedChanged += new System.EventHandler(this.preferredEmail_CheckedChanged);
			// 
			// phoneSelect3
			// 
			this.phoneSelect3.BackColor = System.Drawing.SystemColors.Control;
			this.phoneSelect3.Items.AddRange(new object[] {
															  "Business:",
															  "Business fax:",
															  "Mobile:",
															  "Home:",
															  "Pager:"});
			this.phoneSelect3.Location = new System.Drawing.Point(424, 80);
			this.phoneSelect3.Name = "phoneSelect3";
			this.phoneSelect3.Size = new System.Drawing.Size(96, 21);
			this.phoneSelect3.TabIndex = 15;
			this.phoneSelect3.Text = "Home:";
			this.phoneSelect3.SelectedIndexChanged += new System.EventHandler(this.phoneSelect3_SelectedIndexChanged);
			// 
			// phoneSelect4
			// 
			this.phoneSelect4.BackColor = System.Drawing.SystemColors.Control;
			this.phoneSelect4.Items.AddRange(new object[] {
															  "Business:",
															  "Business fax:",
															  "Mobile:",
															  "Home:",
															  "Pager:"});
			this.phoneSelect4.Location = new System.Drawing.Point(424, 112);
			this.phoneSelect4.Name = "phoneSelect4";
			this.phoneSelect4.Size = new System.Drawing.Size(96, 21);
			this.phoneSelect4.TabIndex = 17;
			this.phoneSelect4.Text = "Mobile:";
			this.phoneSelect4.SelectedIndexChanged += new System.EventHandler(this.phoneSelect4_SelectedIndexChanged);
			// 
			// picturePhone
			// 
			this.picturePhone.Location = new System.Drawing.Point(374, 16);
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
															  "Pager:"});
			this.phoneSelect1.Location = new System.Drawing.Point(424, 16);
			this.phoneSelect1.Name = "phoneSelect1";
			this.phoneSelect1.Size = new System.Drawing.Size(96, 21);
			this.phoneSelect1.TabIndex = 11;
			this.phoneSelect1.Text = "Business:";
			this.phoneSelect1.SelectedIndexChanged += new System.EventHandler(this.phoneSelect1_SelectedIndexChanged);
			// 
			// phoneSelect2
			// 
			this.phoneSelect2.BackColor = System.Drawing.SystemColors.Control;
			this.phoneSelect2.Items.AddRange(new object[] {
															  "Business:",
															  "Business fax:",
															  "Mobile:",
															  "Home:",
															  "Pager:"});
			this.phoneSelect2.Location = new System.Drawing.Point(424, 48);
			this.phoneSelect2.Name = "phoneSelect2";
			this.phoneSelect2.Size = new System.Drawing.Size(96, 21);
			this.phoneSelect2.TabIndex = 13;
			this.phoneSelect2.Text = "Business fax:";
			this.phoneSelect2.SelectedIndexChanged += new System.EventHandler(this.phoneSelect2_SelectedIndexChanged);
			// 
			// userId
			// 
			this.userId.Location = new System.Drawing.Point(176, 112);
			this.userId.Name = "userId";
			this.userId.Size = new System.Drawing.Size(192, 20);
			this.userId.TabIndex = 4;
			this.userId.Text = "";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(128, 112);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(56, 16);
			this.label7.TabIndex = 30;
			this.label7.Text = "User ID:";
			// 
			// phone4
			// 
			this.phone4.Location = new System.Drawing.Point(528, 112);
			this.phone4.Name = "phone4";
			this.phone4.Size = new System.Drawing.Size(192, 20);
			this.phone4.TabIndex = 18;
			this.phone4.Text = "";
			this.phone4.TextChanged += new System.EventHandler(this.phone4_TextChanged);
			this.phone4.Leave += new System.EventHandler(this.phone4_Leave);
			// 
			// phone3
			// 
			this.phone3.Location = new System.Drawing.Point(528, 80);
			this.phone3.Name = "phone3";
			this.phone3.Size = new System.Drawing.Size(192, 20);
			this.phone3.TabIndex = 16;
			this.phone3.Text = "";
			this.phone3.TextChanged += new System.EventHandler(this.phone3_TextChanged);
			this.phone3.Leave += new System.EventHandler(this.phone3_Leave);
			// 
			// phone2
			// 
			this.phone2.Location = new System.Drawing.Point(528, 48);
			this.phone2.Name = "phone2";
			this.phone2.Size = new System.Drawing.Size(192, 20);
			this.phone2.TabIndex = 14;
			this.phone2.Text = "";
			this.phone2.TextChanged += new System.EventHandler(this.phone2_TextChanged);
			this.phone2.Leave += new System.EventHandler(this.phone2_Leave);
			// 
			// phone1
			// 
			this.phone1.Location = new System.Drawing.Point(528, 16);
			this.phone1.Name = "phone1";
			this.phone1.Size = new System.Drawing.Size(192, 20);
			this.phone1.TabIndex = 12;
			this.phone1.Text = "";
			this.phone1.TextChanged += new System.EventHandler(this.phone1_TextChanged);
			this.phone1.Leave += new System.EventHandler(this.phone1_Leave);
			// 
			// pictureMail
			// 
			this.pictureMail.Location = new System.Drawing.Point(8, 152);
			this.pictureMail.Name = "pictureMail";
			this.pictureMail.Size = new System.Drawing.Size(48, 48);
			this.pictureMail.TabIndex = 21;
			this.pictureMail.TabStop = false;
			// 
			// eMail
			// 
			this.eMail.Location = new System.Drawing.Point(176, 160);
			this.eMail.Name = "eMail";
			this.eMail.Size = new System.Drawing.Size(192, 20);
			this.eMail.TabIndex = 7;
			this.eMail.Text = "";
			this.eMail.TextChanged += new System.EventHandler(this.eMail_TextChanged);
			this.eMail.Leave += new System.EventHandler(this.eMail_Leave);
			// 
			// groupBox1
			// 
			this.groupBox1.Location = new System.Drawing.Point(8, 144);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(362, 4);
			this.groupBox1.TabIndex = 18;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "groupBox1";
			// 
			// organization
			// 
			this.organization.Location = new System.Drawing.Point(176, 80);
			this.organization.Name = "organization";
			this.organization.Size = new System.Drawing.Size(192, 20);
			this.organization.TabIndex = 3;
			this.organization.Text = "";
			// 
			// jobTitle
			// 
			this.jobTitle.Location = new System.Drawing.Point(176, 48);
			this.jobTitle.Name = "jobTitle";
			this.jobTitle.Size = new System.Drawing.Size(192, 20);
			this.jobTitle.TabIndex = 2;
			this.jobTitle.Text = "";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(104, 80);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(72, 16);
			this.label3.TabIndex = 6;
			this.label3.Text = "Organization:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(128, 50);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(48, 16);
			this.label2.TabIndex = 5;
			this.label2.Text = "Job title:";
			// 
			// fullName
			// 
			this.fullName.Location = new System.Drawing.Point(176, 16);
			this.fullName.Name = "fullName";
			this.fullName.Size = new System.Drawing.Size(192, 20);
			this.fullName.TabIndex = 1;
			this.fullName.Text = "";
			this.fullName.TextChanged += new System.EventHandler(this.fullName_TextChanged);
			this.fullName.Leave += new System.EventHandler(this.fullName_Leave);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.label15);
			this.tabPage2.Controls.Add(this.notes);
			this.tabPage2.Controls.Add(this.groupBox5);
			this.tabPage2.Controls.Add(this.anniversary);
			this.tabPage2.Controls.Add(this.anniversaryPicker);
			this.tabPage2.Controls.Add(this.birthday);
			this.tabPage2.Controls.Add(this.birthdayPicker);
			this.tabPage2.Controls.Add(this.label14);
			this.tabPage2.Controls.Add(this.label13);
			this.tabPage2.Controls.Add(this.spouse);
			this.tabPage2.Controls.Add(this.label12);
			this.tabPage2.Controls.Add(this.nickname);
			this.tabPage2.Controls.Add(this.label11);
			this.tabPage2.Controls.Add(this.groupBox3);
			this.tabPage2.Controls.Add(this.assistant);
			this.tabPage2.Controls.Add(this.manager);
			this.tabPage2.Controls.Add(this.label10);
			this.tabPage2.Controls.Add(this.label9);
			this.tabPage2.Controls.Add(this.profession);
			this.tabPage2.Controls.Add(this.office);
			this.tabPage2.Controls.Add(this.department);
			this.tabPage2.Controls.Add(this.label8);
			this.tabPage2.Controls.Add(this.label6);
			this.tabPage2.Controls.Add(this.label1);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(728, 286);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Details";
			// 
			// label15
			// 
			this.label15.Location = new System.Drawing.Point(8, 184);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(100, 16);
			this.label15.TabIndex = 23;
			this.label15.Text = "Notes:";
			// 
			// notes
			// 
			this.notes.Location = new System.Drawing.Point(8, 200);
			this.notes.Multiline = true;
			this.notes.Name = "notes";
			this.notes.Size = new System.Drawing.Size(712, 80);
			this.notes.TabIndex = 22;
			this.notes.Text = "";
			// 
			// groupBox5
			// 
			this.groupBox5.Location = new System.Drawing.Point(8, 168);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(712, 4);
			this.groupBox5.TabIndex = 21;
			this.groupBox5.TabStop = false;
			// 
			// anniversary
			// 
			this.anniversary.Location = new System.Drawing.Point(424, 136);
			this.anniversary.Name = "anniversary";
			this.anniversary.Size = new System.Drawing.Size(180, 20);
			this.anniversary.TabIndex = 9;
			this.anniversary.Text = "";
			this.anniversary.Leave += new System.EventHandler(this.anniversary_Leave);
			// 
			// anniversaryPicker
			// 
			this.anniversaryPicker.Location = new System.Drawing.Point(456, 136);
			this.anniversaryPicker.Name = "anniversaryPicker";
			this.anniversaryPicker.Size = new System.Drawing.Size(168, 20);
			this.anniversaryPicker.TabIndex = 10;
			this.anniversaryPicker.ValueChanged += new System.EventHandler(this.anniversaryPicker_ValueChanged);
			// 
			// birthday
			// 
			this.birthday.Location = new System.Drawing.Point(424, 112);
			this.birthday.Name = "birthday";
			this.birthday.Size = new System.Drawing.Size(180, 20);
			this.birthday.TabIndex = 7;
			this.birthday.Text = "";
			this.birthday.Leave += new System.EventHandler(this.birthday_Leave);
			// 
			// birthdayPicker
			// 
			this.birthdayPicker.Location = new System.Drawing.Point(456, 112);
			this.birthdayPicker.Name = "birthdayPicker";
			this.birthdayPicker.Size = new System.Drawing.Size(168, 20);
			this.birthdayPicker.TabIndex = 8;
			this.birthdayPicker.ValueChanged += new System.EventHandler(this.birthdayPicker_ValueChanged);
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(336, 136);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(100, 16);
			this.label14.TabIndex = 19;
			this.label14.Text = "Anniversary:";
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(336, 112);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(100, 16);
			this.label13.TabIndex = 18;
			this.label13.Text = "Birthday:";
			// 
			// spouse
			// 
			this.spouse.Location = new System.Drawing.Point(72, 136);
			this.spouse.Name = "spouse";
			this.spouse.Size = new System.Drawing.Size(240, 20);
			this.spouse.TabIndex = 6;
			this.spouse.Text = "";
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(8, 136);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(100, 16);
			this.label12.TabIndex = 15;
			this.label12.Text = "Spouse:";
			// 
			// nickname
			// 
			this.nickname.Location = new System.Drawing.Point(72, 112);
			this.nickname.Name = "nickname";
			this.nickname.Size = new System.Drawing.Size(240, 20);
			this.nickname.TabIndex = 5;
			this.nickname.Text = "";
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(8, 112);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(100, 16);
			this.label11.TabIndex = 14;
			this.label11.Text = "Nickname:";
			// 
			// groupBox3
			// 
			this.groupBox3.Location = new System.Drawing.Point(8, 96);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(712, 4);
			this.groupBox3.TabIndex = 20;
			this.groupBox3.TabStop = false;
			// 
			// assistant
			// 
			this.assistant.Location = new System.Drawing.Point(424, 40);
			this.assistant.Name = "assistant";
			this.assistant.Size = new System.Drawing.Size(296, 20);
			this.assistant.TabIndex = 4;
			this.assistant.Text = "";
			// 
			// manager
			// 
			this.manager.Location = new System.Drawing.Point(424, 16);
			this.manager.Name = "manager";
			this.manager.Size = new System.Drawing.Size(296, 20);
			this.manager.TabIndex = 3;
			this.manager.Text = "";
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(336, 40);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(100, 16);
			this.label10.TabIndex = 17;
			this.label10.Text = "Assistant\'s name:";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(336, 16);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(100, 16);
			this.label9.TabIndex = 16;
			this.label9.Text = "Manager\'s name:";
			// 
			// profession
			// 
			this.profession.Location = new System.Drawing.Point(72, 64);
			this.profession.Name = "profession";
			this.profession.Size = new System.Drawing.Size(240, 20);
			this.profession.TabIndex = 2;
			this.profession.Text = "";
			// 
			// office
			// 
			this.office.Location = new System.Drawing.Point(72, 40);
			this.office.Name = "office";
			this.office.Size = new System.Drawing.Size(240, 20);
			this.office.TabIndex = 1;
			this.office.Text = "";
			// 
			// department
			// 
			this.department.Location = new System.Drawing.Point(72, 16);
			this.department.Name = "department";
			this.department.Size = new System.Drawing.Size(240, 20);
			this.department.TabIndex = 0;
			this.department.Text = "";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(8, 64);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(100, 16);
			this.label8.TabIndex = 13;
			this.label8.Text = "Profession:";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(8, 40);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(100, 16);
			this.label6.TabIndex = 12;
			this.label6.Text = "Office:";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 11;
			this.label1.Text = "Department:";
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ok.Location = new System.Drawing.Point(584, 328);
			this.ok.Name = "ok";
			this.ok.TabIndex = 1;
			this.ok.Text = "OK";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancel.Location = new System.Drawing.Point(664, 328);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 2;
			this.cancel.Text = "Cancel";
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// ContactEditor
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(754, 360);
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
			this.tabPage2.ResumeLayout(false);
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

		/// <summary>
		/// Sets the path where the library was loaded from.
		/// </summary>
		public string LoadPath
		{
			set
			{
				loadPath = value;
			}
		}
		#endregion

		#region Private Methods
		private bool LoadAddresses()
		{
			try
			{
				// Get the e-mail addresses.
				foreach(Email tmpMail in contact.GetEmailAddresses())
				{
					EmailEntry entry = new EmailEntry();
					entry.EMail = tmpMail;

					if ((tmpMail.Types & EmailTypes.work) == EmailTypes.work)
					{
						emailHT.Add((string)emailSelect.Items[0], entry);

						if ((tmpMail.Types & EmailTypes.preferred) == EmailTypes.preferred)
						{
							emailSelect.SelectedIndex = 0;
							eMail.Text = tmpMail.Address;
						}
					}
					else if ((tmpMail.Types & EmailTypes.personal) == EmailTypes.personal)
					{
						emailHT.Add((string)emailSelect.Items[1], entry);
						if ((tmpMail.Types & EmailTypes.preferred) == EmailTypes.preferred)
						{
							emailSelect.SelectedIndex = 1;
							eMail.Text = tmpMail.Address;
						}
					}
					else if ((tmpMail.Types & EmailTypes.other) == EmailTypes.other)
					{
						emailHT.Add((string)emailSelect.Items[2], entry);
						if ((tmpMail.Types & EmailTypes.preferred) == EmailTypes.preferred)
						{
							emailSelect.SelectedIndex = 2;
							eMail.Text = tmpMail.Address;
						}
					}
				}

				// If there is something in the edit box then check and enable the preferred box.
				if (eMail.Text != "")
				{
					preferredEmail.Checked = true;
				}
			}
			catch{}

			// Deal with phone numbers
			try
			{
				foreach(Telephone tmpPhone in contact.GetTelephoneNumbers())
				{
					TelephoneEntry phone = new TelephoneEntry();
					phone.Phone = tmpPhone;

					if ((tmpPhone.Types & (PhoneTypes.work | PhoneTypes.voice)) == (PhoneTypes.work | PhoneTypes.voice))
					{
						phoneHT.Add((string)this.phoneSelect1.Items[0], phone);
						SetPhoneInEdit(0, tmpPhone.Number);
					}
					else if ((tmpPhone.Types & (PhoneTypes.work | PhoneTypes.fax)) == (PhoneTypes.work | PhoneTypes.fax))
					{
						phoneHT.Add((string)this.phoneSelect1.Items[1], phone);
						SetPhoneInEdit(1, tmpPhone.Number);
					}
					else if ((tmpPhone.Types & PhoneTypes.cell) == PhoneTypes.cell)
					{
						phoneHT.Add((string)this.phoneSelect1.Items[2], phone);
						SetPhoneInEdit(2, tmpPhone.Number);
					}
					else if ((tmpPhone.Types & (PhoneTypes.home | PhoneTypes.voice)) == (PhoneTypes.home | PhoneTypes.voice))
					{
						phoneHT.Add((string)this.phoneSelect1.Items[3], phone);
						SetPhoneInEdit(3, tmpPhone.Number);
					}
					else if ((tmpPhone.Types & PhoneTypes.pager) == PhoneTypes.pager)
					{
						phoneHT.Add((string)this.phoneSelect1.Items[4], phone);
						SetPhoneInEdit(4, tmpPhone.Number);
					}
				}
			}
			catch{}

			foreach(Address addr in contact.GetAddresses())
			{
				if((addr.Types & AddressTypes.work) == AddressTypes.work)
				{
					workAddrEntry.Addr = addr;
					DisplayAddress(addr);
				}
				else if((addr.Types & AddressTypes.home) == AddressTypes.home)
				{
					homeAddrEntry.Addr = addr;
				}
			}

			return(true);
		}

		private void SetPhoneInEdit(int index, string number)
		{
			if (phoneSelect1.SelectedIndex == index)
				phone1.Text = number;
			
			if (phoneSelect2.SelectedIndex == index)
				phone2.Text = number;

			if (phoneSelect3.SelectedIndex == index)
				phone3.Text = number;

			if (phoneSelect4.SelectedIndex == index)
				phone4.Text = number;
		}

		private void UpdatePhoneTable(ComboBox select, TextBox phone)
		{
			TelephoneEntry entry = (TelephoneEntry)phoneHT[select.Text];
			if (entry != null)
			{
				if (entry.Phone.Number != phone.Text)
				{
					phoneHT.Remove(select.Text);

					if ((phone.Text == "") && !entry.Add)
					{
						entry.Remove = true;
					}

					entry.Phone.Number = phone.Text;
					phoneHT.Add(select.Text, entry);
				}
			}
			else
			{
				if (phone.Text != "")
				{
					entry = new TelephoneEntry();
					entry.Add = true;
					entry.Phone = new Telephone(phone.Text);

					switch (select.SelectedIndex)
					{
						case 0:
							entry.Phone.Types = (PhoneTypes.preferred | PhoneTypes.work | PhoneTypes.voice);
							break;
						case 1:
							entry.Phone.Types = (PhoneTypes.work | PhoneTypes.fax);
							break;
						case 2:
							entry.Phone.Types = (PhoneTypes.cell | PhoneTypes.voice);
							break;
						case 3:
							entry.Phone.Types = (PhoneTypes.home | PhoneTypes.voice);
							break;
						case 4:
							entry.Phone.Types = PhoneTypes.pager;
							break;
						default:
							entry.Phone.Types = 0;
							break;
					}

					phoneHT.Add(select.Text, entry);
				}
			}
		}

		private EmailEntry GetPreferredEmail()
		{
			foreach (EmailEntry ee in emailHT.Values)
			{
				if ((ee.EMail.Types & EmailTypes.preferred) == EmailTypes.preferred)
				{
					return ee;
				}
			}

			return null;
		}

		private void DisplayAddress(Address addr)
		{
			addressView.Items.Clear();

			if (addr != null)
			{
				if (addr.Street != "")
					addressView.Items.Add(addr.Street);
				if (addr.ExtendedAddress != "")
					addressView.Items.Add(addr.ExtendedAddress);
				if (addr.PostalBox != "")
					addressView.Items.Add("P.O. Box " + addr.PostalBox);
				if (addr.Locality != "" || addr.Region != "" || addr.PostalCode != "")
					addressView.Items.Add(addr.Locality + ", " + addr.Region + " " + addr.PostalCode);
				if (addr.Country != "")
					addressView.Items.Add(addr.Country);
			}
/* REWORK in progress...
			address.Clear();

			if (addr != null)
			{
				string[] fullAddr = new string[5];
				int index = 0;

				if (addr.Street != String.Empty)
					fullAddr[index++] = addr.Street;
				if (addr.ExtendedAddress != String.Empty)
					fullAddr[index++] = addr.ExtendedAddress;
				if (addr.PostalBox != String.Empty)
					fullAddr[index++] = "P.O. Box " + addr.PostalBox;
				if (addr.Locality != String.Empty || 
					addr.Region != String.Empty || 
					addr.PostalCode != String.Empty)
					fullAddr[index++] = addr.Locality + ", " + addr.Region + " " + addr.PostalCode;
				if (addr.Country != String.Empty)
					fullAddr[index++] = addr.Country;

				address.Lines = fullAddr;
			}*/
		}

		private string ConvertDateToString(DateTime dt)
		{
			string date = dt.ToString("s");
			return date.Substring(0, date.IndexOf("T"));
		}
		#endregion

		#region Event Handlers
		private void ContactEditor_Load(object sender, EventArgs e)
		{
			// Get the base path.
			string basePath = Path.Combine(loadPath != null ? loadPath : Application.StartupPath, "res");

			try
			{
				// Load the images.
//				pictureContact.SizeMode = PictureBoxSizeMode.StretchImage;
//				pictureContact.Image = Image.FromFile(Path.Combine(basePath, "blankhead.png"));
				picturePhone.Image = Image.FromFile(Path.Combine(basePath, "cellphone.png"));
				pictureMail.Image = Image.FromFile(Path.Combine(basePath, "ico-mail.png"));
				pictureAddress.Image = Image.FromFile(Path.Combine(basePath, "house.png"));
				pictureWeb.Image = Image.FromFile(Path.Combine(basePath, "globe.png"));
			}
			catch{}

			pictureContact.Hide();

			if (contact != null)
			{
				// Initialize the dialog with the specified contact.
				userId.Text = this.contact.UserName;

				try
				{
					name = this.contact.GetPreferredName();
					fullName.Text = name.FN;
					nameValidated = true;
				}
				catch
				{
					fullName.Text = "";
				}

				jobTitle.Text = contact.Title;
				organization.Text = contact.Organization;
				webAddress.Text = contact.Url;
				blogAddress.Text = contact.Blog;
				nickname.Text = contact.Nickname;
				birthday.Text = contact.Birthday;

				try
				{
					pictureContact.SizeMode = PictureBoxSizeMode.StretchImage;
					pictureContact.Image = Image.FromStream(contact.ExportPhoto());
					pictureContact.Show();
					changePicture.Hide();
				}
				catch{}

				bool results = LoadAddresses();
			}
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			if (phone1.Focused)
				UpdatePhoneTable(phoneSelect1, phone1);
			else if (phone2.Focused)
				UpdatePhoneTable(phoneSelect2, phone2);
			else if (phone3.Focused)
				UpdatePhoneTable(phoneSelect3, phone3);
			else if (phone4.Focused)
				UpdatePhoneTable(phoneSelect4, phone4);
			else if (eMail.Focused)
				eMail_Leave(this, null);

			string	username = null;
//			string	email = null;

			username = userId.Text.Trim();

			// TODO - will e-mail address be a required attribute?
//			email = eMail.Text.Trim();

			if (username != "" &&
//				email != "" &&
				name != null &&
				name.Given != null &&
				name.Given != "" &&
				name.Family != null &&
				name.Family != "")
			{
				if (contact == null)
				{
					// This is a new contact.
					contact = new Contact();
					contact.UserName = username;

					try
					{
						name.Preferred = true;
						contact.AddName(name);
					}
					catch{}

					IDictionaryEnumerator enumerator = emailHT.GetEnumerator();
					while (enumerator.MoveNext())
					{
						contact.AddEmailAddress(((EmailEntry)enumerator.Value).EMail);
					}

					// Add the phone numbers.
					enumerator = phoneHT.GetEnumerator();
					while (enumerator.MoveNext())
					{
						contact.AddTelephoneNumber(((TelephoneEntry)enumerator.Value).Phone);
					}

					contact.Organization = organization.Text.Trim();
					contact.Title = jobTitle.Text.Trim();
					contact.Url = webAddress.Text.Trim();
					contact.Blog = blogAddress.Text.Trim();
					contact.Nickname = nickname.Text.Trim();
					contact.Birthday = birthday.Text.Trim();

					// Update addresses.
					if (homeAddrEntry.Add)
					{
						contact.AddAddress(homeAddrEntry.Addr);
					}
					else if (homeAddrEntry.Remove)
					{
						homeAddrEntry.Addr.Delete();
					}

					if (workAddrEntry.Add)
					{
						contact.AddAddress(workAddrEntry.Addr);
					}
					else if (workAddrEntry.Remove)
					{
						workAddrEntry.Addr.Delete();
					}

					addressBook.AddContact(contact);
					contact.Commit();
				}
				else
				{
					// Update email.
					try
					{
						IDictionaryEnumerator enumerator = emailHT.GetEnumerator();
						while (enumerator.MoveNext())
						{
							EmailEntry entry = (EmailEntry)enumerator.Value;
							if (entry.Add)
							{
								contact.AddEmailAddress(entry.EMail);
							}
							else if (entry.Remove)
							{
								entry.EMail.Delete();
							}
						}
					}
					catch{}

					// Update phone numbers.
					try
					{
						IDictionaryEnumerator enumerator = phoneHT.GetEnumerator();
						while (enumerator.MoveNext())
						{
							TelephoneEntry entry = (TelephoneEntry)enumerator.Value;
							if (entry.Add)
							{
								contact.AddTelephoneNumber(entry.Phone);
							}
							else if (entry.Remove)
							{
								entry.Phone.Delete();
							}
						}
					}
					catch{}

					if (contact.UserName != username)
					{
						contact.UserName = username;
					}

					contact.Title = jobTitle.Text.Trim();
					contact.Organization = organization.Text.Trim();
					contact.Url = webAddress.Text.Trim();
					contact.Blog = blogAddress.Text.Trim();
					contact.Nickname = nickname.Text.Trim();
					contact.Birthday = birthday.Text.Trim();

					// Update addresses.
					if (homeAddrEntry.Add)
					{
						contact.AddAddress(homeAddrEntry.Addr);
					}
					else if (homeAddrEntry.Remove)
					{
						homeAddrEntry.Addr.Delete();
					}

					if (workAddrEntry.Add)
					{
						contact.AddAddress(workAddrEntry.Addr);
					}
					else if (workAddrEntry.Remove)
					{
						workAddrEntry.Addr.Delete();
					}

					contact.Commit();
				}
			}
		}

		private void ContactEditor_Closing(object sender, CancelEventArgs e)
		{
			string user = this.userId.Text.Trim();
			// TODO - will e-mail address be a required attribute?
//			string email = this.eMail.Text.Trim();

			// Make sure the mandatory fields are filled in if OK was clicked.
			if (this.DialogResult == DialogResult.OK &&
				(user == "" ||
//				email == "" ||
				name == null ||
				((name != null) && ((name.Given == null || name.Given == "") ||	(name.Family == null || name.Family == "")))))
			{
				MessageBox.Show("User ID, first name, last name, and e-mail are required attributes.", "Missing Required Attributes", MessageBoxButtons.OK);

				// Set the focus to the edit field that needs filled in.
				if (user == "")
				{
					this.userId.Focus();
				}
				else if (name == null ||
					(name != null) && ((name.Given == null || name.Given == "") || (name.Family == null || name.Family == "")))
				{
					this.fullName.Focus();
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
			this.fullName.Focus();
		}

		private void phone1_Leave(object sender, System.EventArgs e)
		{
			UpdatePhoneTable(phoneSelect1, phone1);
		}

		private void phone2_Leave(object sender, System.EventArgs e)
		{
			UpdatePhoneTable(phoneSelect2, phone2);
		}

		private void phone3_Leave(object sender, System.EventArgs e)
		{
			UpdatePhoneTable(phoneSelect3, phone3);
		}

		private void phone4_Leave(object sender, System.EventArgs e)
		{
			UpdatePhoneTable(phoneSelect4, phone4);
		}

		private void emailSelect_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			EmailEntry entry = (EmailEntry)emailHT[emailSelect.Text];
			if ((entry != null) && !entry.Remove)
			{
				eMail.Text = entry.EMail.Address;
				preferredEmail.Enabled = (entry.EMail.Types & EmailTypes.preferred) != EmailTypes.preferred;
				preferredEmail.Checked = !preferredEmail.Enabled;
			}
			else
			{
				eMail.Text = "";
				preferredEmail.Enabled = false;
				preferredEmail.Checked = false;
			}
		}

		private void eMail_Leave(object sender, System.EventArgs e)
		{
			EmailEntry entry = (EmailEntry)emailHT[emailSelect.Text];
			if (entry != null)
			{
				if (entry.EMail.Address != eMail.Text)
				{
					emailHT.Remove(emailSelect.Text);

					if ((eMail.Text == "") && !entry.Add)
					{
						entry.Remove = true;

						// If this was the preferred address, we need to make a different
						// address the preferred one.
						// TODO - preferred should probably be added to the UI
						if ((entry.EMail.Types & EmailTypes.preferred) == EmailTypes.preferred)
						{
							if (emailHT.Count > 0)
							{
								bool preferredSet = false;
								IEnumerator enumerator = emailHT.Values.GetEnumerator();

								while (enumerator.MoveNext())
								{
									EmailEntry ee = (EmailEntry)enumerator.Current;
									if ((ee.EMail.Types & EmailTypes.work) == EmailTypes.work)
									{
										ee.EMail.Types |= EmailTypes.preferred;
										preferredSet = true;
									}
								}

								if (!preferredSet)
								{
									enumerator.Reset();
									enumerator.MoveNext();
									((EmailEntry)enumerator.Current).EMail.Types |= EmailTypes.preferred;
								}
							}
						}
					}

					entry.EMail.Address = eMail.Text;
					emailHT.Add(emailSelect.Text, entry);
				}
			}
			else
			{
				if (eMail.Text != "")
				{
					entry = new EmailEntry();
					entry.Add = true;
					entry.EMail = new Email();
					entry.EMail.Address = eMail.Text.Trim();

					switch (emailSelect.SelectedIndex)
					{
						case 0:
							entry.EMail.Types = EmailTypes.work;
							break;
						case 1:
							entry.EMail.Types = EmailTypes.personal;
							break;
						case 2:
							entry.EMail.Types = EmailTypes.other;
							break;
						default:
							entry.EMail.Types = 0;
							break;
					}

					// The first entry is set as the preferred address.
					// TODO - preferred should probably be added to the UI
					if (emailHT.Count == 0)
					{
						entry.EMail.Types |= EmailTypes.preferred;
						preferredEmail.Checked = true;
					}

					emailHT.Add(emailSelect.Text, entry);
				}
			}
		}

		private void eMail_TextChanged(object sender, System.EventArgs e)
		{
			if (eMail.Text.Trim() != "")
			{
				if (GetPreferredEmail() == null)
				{
					preferredEmail.Checked = true;
				}

				preferredEmail.Enabled = !preferredEmail.Checked;
			}
			else
			{
				preferredEmail.Enabled = false;
			}
		}

		private void preferredEmail_CheckedChanged(object sender, System.EventArgs e)
		{
			if (preferredEmail.Enabled && preferredEmail.Checked)
			{
				EmailEntry entry = GetPreferredEmail();
				if (entry != null)
				{
					entry.EMail.Types &= ~EmailTypes.preferred;
				}

				entry = (EmailEntry)emailHT[emailSelect.Text];
				if (entry != null)
				{
					entry.EMail.Types |= EmailTypes.preferred;
				}

				preferredEmail.Enabled = false;
			}
		}

		private void phoneSelect1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			TelephoneEntry entry = (TelephoneEntry)phoneHT[this.phoneSelect1.Text];
			if ((entry != null) && !entry.Remove)
			{
				phone1.Text = entry.Phone.Number;
			}
			else
			{
				phone1.Text = "";
			}
		}

		private void phoneSelect2_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			TelephoneEntry entry = (TelephoneEntry)phoneHT[this.phoneSelect2.Text];
			if ((entry != null) && !entry.Remove)
			{
				phone2.Text = entry.Phone.Number;
			}
			else
			{
				phone2.Text = "";
			}
		}

		private void phoneSelect3_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			TelephoneEntry entry = (TelephoneEntry)phoneHT[this.phoneSelect3.Text];
			if ((entry != null) && !entry.Remove)
			{
				phone3.Text = entry.Phone.Number;
			}
			else
			{
				phone3.Text = "";
			}
		}

		private void phoneSelect4_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			TelephoneEntry entry = (TelephoneEntry)phoneHT[this.phoneSelect4.Text];
			if ((entry != null) && !entry.Remove)
			{
				phone4.Text = entry.Phone.Number;
			}
			else
			{
				phone4.Text = "";
			}
		}

		private void fullNameButton_Click(object sender, System.EventArgs e)
		{
			FullName fullNameDlg = new FullName();
			bool newName = false;

			// Initialize.
			if (name == null)
			{
				name = new Name();
				newName = true;
			}

			fullNameDlg.Title = name.Prefix;
			fullNameDlg.FirstName = name.Given;
			fullNameDlg.MiddleName = name.Other;
			fullNameDlg.LastName = name.Family;
			fullNameDlg.Suffix = name.Suffix;

			DialogResult result = fullNameDlg.ShowDialog();
			if (result == DialogResult.OK)
			{
				// Save the information.
				if (name.Prefix != fullNameDlg.Title)
					name.Prefix = fullNameDlg.Title;
				// BUGBUG - Given name changes aren't sticking
				if (name.Given != fullNameDlg.FirstName)
					name.Given = fullNameDlg.FirstName;
				if (name.Other != fullNameDlg.MiddleName)
					name.Other = fullNameDlg.MiddleName;
				if (name.Family != fullNameDlg.LastName)
					name.Family = fullNameDlg.LastName;
				if (name.Suffix != fullNameDlg.Suffix)
					name.Suffix = fullNameDlg.Suffix;

				if (newName && (contact != null))
				{
					name.Preferred = true;
					contact.AddName(name);
				}

				fullName.Text = name.FN;
				nameValidated = true;
			}
		}

		private void phone1_TextChanged(object sender, System.EventArgs e)
		{
			// Make changes echo to any other edit box that is on the same setting.
			if (this.phoneSelect2.SelectedIndex == this.phoneSelect1.SelectedIndex)
			{
				phone2.Text = phone1.Text;
			}
		
			if (this.phoneSelect3.SelectedIndex == this.phoneSelect1.SelectedIndex)
			{
				phone3.Text = phone1.Text;
			}

			if (this.phoneSelect4.SelectedIndex == this.phoneSelect1.SelectedIndex)
			{
				phone4.Text = phone1.Text;
			}
		}

		private void phone2_TextChanged(object sender, System.EventArgs e)
		{
			// Make changes echo to any other edit box that is on the same setting.
			if (this.phoneSelect1.SelectedIndex == this.phoneSelect2.SelectedIndex)
			{
				phone1.Text = phone2.Text;
			}
		
			if (this.phoneSelect3.SelectedIndex == this.phoneSelect2.SelectedIndex)
			{
				phone3.Text = phone2.Text;
			}

			if (this.phoneSelect4.SelectedIndex == this.phoneSelect2.SelectedIndex)
			{
				phone4.Text = phone2.Text;
			}
		}

		private void phone3_TextChanged(object sender, System.EventArgs e)
		{
			// Make changes echo to any other edit box that is on the same setting.
			if (this.phoneSelect1.SelectedIndex == this.phoneSelect3.SelectedIndex)
			{
				phone1.Text = phone3.Text;
			}
		
			if (this.phoneSelect2.SelectedIndex == this.phoneSelect3.SelectedIndex)
			{
				phone2.Text = phone3.Text;
			}

			if (this.phoneSelect4.SelectedIndex == this.phoneSelect3.SelectedIndex)
			{
				phone4.Text = phone3.Text;
			}
		}

		private void phone4_TextChanged(object sender, System.EventArgs e)
		{
			// Make changes echo to any other edit box that is on the same setting.
			if (this.phoneSelect1.SelectedIndex == this.phoneSelect4.SelectedIndex)
			{
				phone1.Text = phone4.Text;
			}
		
			if (this.phoneSelect2.SelectedIndex == this.phoneSelect4.SelectedIndex)
			{
				phone2.Text = phone4.Text;
			}

			if (this.phoneSelect3.SelectedIndex == this.phoneSelect4.SelectedIndex)
			{
				phone3.Text = phone4.Text;
			}
		}

		private void addr_Click(object sender, System.EventArgs e)
		{
			AddressEntry selectedAddr;
			
			if (addressSelect.SelectedIndex == 0)
			{
				if (workAddrEntry.Addr == null)
				{
					workAddrEntry.Addr = new Address();
					workAddrEntry.Add = true;
					workAddrEntry.Addr.Types = AddressTypes.work;
				}
				selectedAddr = workAddrEntry;
			}
			else
			{
				if (homeAddrEntry.Addr == null)
				{
					homeAddrEntry.Addr = new Address();
					homeAddrEntry.Add = true;
					homeAddrEntry.Addr.Types = AddressTypes.home;
				}
				selectedAddr = homeAddrEntry;
			}

			FullAddress addrDlg = new FullAddress();

			// Initialize.
			addrDlg.Street = selectedAddr.Addr.Street;
			addrDlg.ExtendedAddress = selectedAddr.Addr.ExtendedAddress;
			addrDlg.Locality = selectedAddr.Addr.Locality;
			addrDlg.RegionAddr = selectedAddr.Addr.Region;
			addrDlg.PostalBox = selectedAddr.Addr.PostalBox;
			addrDlg.PostalCode = selectedAddr.Addr.PostalCode;
			addrDlg.Country = selectedAddr.Addr.Country;

			DialogResult result = addrDlg.ShowDialog();
			if (result == DialogResult.OK)
			{
				if (addrDlg.Street != "" ||
					addrDlg.ExtendedAddress != "" ||
					addrDlg.Locality != "" ||
					addrDlg.RegionAddr != "" ||
					addrDlg.PostalBox != "" ||
					addrDlg.PostalCode != "" ||
					addrDlg.Country != "")
				{
					selectedAddr.Addr.Street = addrDlg.Street;
					selectedAddr.Addr.ExtendedAddress = addrDlg.ExtendedAddress;
					selectedAddr.Addr.Locality = addrDlg.Locality;
					selectedAddr.Addr.Region = addrDlg.RegionAddr;
					selectedAddr.Addr.PostalBox = addrDlg.PostalBox;
					selectedAddr.Addr.PostalCode = addrDlg.PostalCode;
					selectedAddr.Addr.Country = addrDlg.Country;
				}
				else
				{
					if (selectedAddr.Add)
					{
						selectedAddr.Add = false;
					}
					else
					{
						selectedAddr.Remove = true;
					}
				}

				DisplayAddress(selectedAddr.Addr);
			}
			else
			{
				if (selectedAddr.Add)
					selectedAddr.Add = false;
			}
		}

		private void addressSelect_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (addressSelect.SelectedIndex == 0)
				DisplayAddress(workAddrEntry.Addr);
			else
				DisplayAddress(homeAddrEntry.Addr);		
		}

		private void changePicture_Click(object sender, System.EventArgs e)
		{
			// BUGBUG - need to newup a contact on create so that the photo can be imported.
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Title = "Add Contact Picture";
			openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF";

			if(openFileDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					contact.ImportPhoto(openFileDialog.OpenFile());
					pictureContact.Image = Image.FromStream(contact.ExportPhoto());
					pictureContact.Show();
					changePicture.Hide();
				}
				catch
				{
					pictureContact.Hide();
				}
			}
		}

		private void fullName_Leave(object sender, System.EventArgs e)
		{
			// Parse name...
			// TODO - much more to be done here ... this is just a starting point.

			// If the Cancel button was clicked, we're done.
			if (cancel.Focused)
				return;

			if (name == null)
			{
				name = new Name();
			}

			// Trim whitespace from beginning and end of string.
			string tmpName = fullName.Text.Trim();

			// Parse the string.
			if (!FullNameParser.Parse(tmpName, ref name) && !nameValidated)
			{
				// If the name didn't parse correctly and the name has not been
				// validated by the user, popup the full name dialog.
				fullNameButton_Click(this, null);
			}
		}

		private void pictureContact_DoubleClick(object sender, System.EventArgs e)
		{
			changePicture_Click(sender, e);
		}

		private void birthdayPicker_ValueChanged(object sender, System.EventArgs e)
		{
            birthday.Text = ConvertDateToString(birthdayPicker.Value);
		}

		private void birthday_Leave(object sender, System.EventArgs e)
		{
//			if (birthday.Text.Trim() == String.Empty)
//			{
//				birthday.Text = "None";
//			}		
		}

		private void anniversaryPicker_ValueChanged(object sender, System.EventArgs e)
		{
			anniversary.Text = ConvertDateToString(anniversaryPicker.Value);
		}

		private void anniversary_Leave(object sender, System.EventArgs e)
		{
//			if (anniversary.Text == String.Empty)
//			{
//				anniversary.Text = "None";
//			}
		}

		private void address_Enter(object sender, System.EventArgs e)
		{
			// Remove accept button so that <Enter> will go to the next line in the
			// multi-line edit box.
			this.AcceptButton = null;
		}

		private void address_Leave(object sender, System.EventArgs e)
		{
/* REWORK in progress...
			// We're done with the multi-line edit box, set the ok button back to the
			// accept button.
			this.AcceptButton = ok;
		
			AddressEntry selectedAddr;
			
			if (addressSelect.SelectedIndex == 0)
			{
				if (workAddrEntry.Addr == null)
				{
					workAddrEntry.Addr = new Address();
					workAddrEntry.Add = true;
					workAddrEntry.Addr.Types = AddressTypes.work;
				}
				selectedAddr = workAddrEntry;
			}
			else
			{
				if (homeAddrEntry.Addr == null)
				{
					homeAddrEntry.Addr = new Address();
					homeAddrEntry.Add = true;
					homeAddrEntry.Addr.Types = AddressTypes.home;
				}
				selectedAddr = homeAddrEntry;
			}

			// Parse the address.
			AddressParser.Parse(address.Lines, ref selectedAddr);
			int index = address.Lines.Length;
			while (index-- > 0)
			{
				int comma = address.Lines[index].IndexOf(",");
				if (comma != -1 && comma != 0)
				{
					// The city comes first.
					selectedAddr.Addr.Locality = address.Lines[index].Substring(0, comma);

					// Check for a zip code on this line.
					string tmp = address.Lines[index].Substring(comma + 1).Trim();
					int space = tmp.LastIndexOf(" ");
					if (space != -1)
					{
						string tmpZip = tmp.Substring(space + 1);
						int g;
						try
						{
							g = int.Parse(tmpZip, NumberStyles.Number);
						}
						catch{}

						bool bOOM = true;
					}
					
					selectedAddr.Addr.Region = address.Lines[index].Substring(comma + 1);
					break;
				}
			}*/
		}

		private void fullName_TextChanged(object sender, System.EventArgs e)
		{
			nameValidated = false;
		}

		private void cancel_Click(object sender, System.EventArgs e)
		{
//			contact.Rollback();
		}
		#endregion
	}
}
