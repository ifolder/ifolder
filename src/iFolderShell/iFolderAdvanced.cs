/***********************************************************************
 *  iFolderAdvanced.cs - The Advanced iFolder properties dialog.
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
using System.Runtime.InteropServices;
using Novell.iFolder;
using Novell.AddressBook;
using Simias.Storage;
using Novell.iFolder.FormsBookLib;

namespace Novell.iFolder.iFolderCom
{
	/// <summary>
	/// iFolder Advanced dialog.
	/// </summary>
	[ComVisible(false)]
	public class iFolderAdvanced : System.Windows.Forms.Form
	{
		#region Class Members
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Button apply;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.ListView shareWith;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button remove;
		private System.Windows.Forms.Button add;
		private System.Windows.Forms.GroupBox accessControlButtons;
		private System.Windows.Forms.RadioButton accessFullControl;
		private System.Windows.Forms.RadioButton accessReadWrite;
		private System.Windows.Forms.RadioButton accessReadOnly;

		private Novell.AddressBook.Manager abManager;
		private Novell.AddressBook.AddressBook defaultAddressBook;
		private iFolder ifolder;
		private ArrayList removedList;
		private System.Windows.Forms.Button reinvite;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		public iFolderAdvanced()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			this.apply.Enabled = false;
			this.remove.Enabled = false;
			this.reinvite.Enabled = false;
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
			this.reinvite = new System.Windows.Forms.Button();
			this.accessControlButtons = new System.Windows.Forms.GroupBox();
			this.accessReadOnly = new System.Windows.Forms.RadioButton();
			this.accessReadWrite = new System.Windows.Forms.RadioButton();
			this.accessFullControl = new System.Windows.Forms.RadioButton();
			this.add = new System.Windows.Forms.Button();
			this.remove = new System.Windows.Forms.Button();
			this.shareWith = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.apply = new System.Windows.Forms.Button();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.accessControlButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Location = new System.Drawing.Point(8, 16);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(344, 400);
			this.tabControl1.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.reinvite);
			this.tabPage1.Controls.Add(this.accessControlButtons);
			this.tabPage1.Controls.Add(this.add);
			this.tabPage1.Controls.Add(this.remove);
			this.tabPage1.Controls.Add(this.shareWith);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(336, 374);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Sharing";
			// 
			// reinvite
			// 
			this.reinvite.Location = new System.Drawing.Point(8, 232);
			this.reinvite.Name = "reinvite";
			this.reinvite.TabIndex = 4;
			this.reinvite.Text = "Re-invite";
			this.reinvite.Click += new System.EventHandler(this.reinvite_Click);
			// 
			// accessControlButtons
			// 
			this.accessControlButtons.Controls.Add(this.accessReadOnly);
			this.accessControlButtons.Controls.Add(this.accessReadWrite);
			this.accessControlButtons.Controls.Add(this.accessFullControl);
			this.accessControlButtons.Location = new System.Drawing.Point(8, 264);
			this.accessControlButtons.Name = "accessControlButtons";
			this.accessControlButtons.Size = new System.Drawing.Size(320, 100);
			this.accessControlButtons.TabIndex = 3;
			this.accessControlButtons.TabStop = false;
			this.accessControlButtons.Text = "Access";
			// 
			// accessReadOnly
			// 
			this.accessReadOnly.Location = new System.Drawing.Point(24, 72);
			this.accessReadOnly.Name = "accessReadOnly";
			this.accessReadOnly.Size = new System.Drawing.Size(280, 16);
			this.accessReadOnly.TabIndex = 2;
			this.accessReadOnly.Text = "Read Only";
			this.accessReadOnly.Click += new System.EventHandler(this.accessButton_Click);
			// 
			// accessReadWrite
			// 
			this.accessReadWrite.Location = new System.Drawing.Point(24, 48);
			this.accessReadWrite.Name = "accessReadWrite";
			this.accessReadWrite.Size = new System.Drawing.Size(280, 16);
			this.accessReadWrite.TabIndex = 1;
			this.accessReadWrite.Text = "Read/Write";
			this.accessReadWrite.Click += new System.EventHandler(this.accessButton_Click);
			// 
			// accessFullControl
			// 
			this.accessFullControl.Location = new System.Drawing.Point(24, 24);
			this.accessFullControl.Name = "accessFullControl";
			this.accessFullControl.Size = new System.Drawing.Size(280, 16);
			this.accessFullControl.TabIndex = 0;
			this.accessFullControl.Text = "Full Control";
			this.accessFullControl.Click += new System.EventHandler(this.accessButton_Click);
			// 
			// add
			// 
			this.add.Location = new System.Drawing.Point(176, 232);
			this.add.Name = "add";
			this.add.TabIndex = 1;
			this.add.Text = "Add...";
			this.add.Click += new System.EventHandler(this.add_Click);
			// 
			// remove
			// 
			this.remove.Location = new System.Drawing.Point(256, 232);
			this.remove.Name = "remove";
			this.remove.TabIndex = 2;
			this.remove.Text = "Remove";
			this.remove.Click += new System.EventHandler(this.remove_Click);
			// 
			// shareWith
			// 
			this.shareWith.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeader1,
																						this.columnHeader2});
			this.shareWith.HideSelection = false;
			this.shareWith.Location = new System.Drawing.Point(8, 8);
			this.shareWith.Name = "shareWith";
			this.shareWith.Size = new System.Drawing.Size(320, 216);
			this.shareWith.TabIndex = 0;
			this.shareWith.View = System.Windows.Forms.View.Details;
			this.shareWith.SelectedIndexChanged += new System.EventHandler(this.shareWith_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Share with";
			this.columnHeader1.Width = 172;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Access";
			this.columnHeader2.Width = 144;
			// 
			// tabPage2
			// 
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(336, 374);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Tab2";
			// 
			// tabPage3
			// 
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(336, 374);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Tab3";
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Location = new System.Drawing.Point(112, 432);
			this.ok.Name = "ok";
			this.ok.TabIndex = 1;
			this.ok.Text = "OK";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(192, 432);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 2;
			this.cancel.Text = "Cancel";
			// 
			// apply
			// 
			this.apply.Location = new System.Drawing.Point(272, 432);
			this.apply.Name = "apply";
			this.apply.TabIndex = 3;
			this.apply.Text = "Apply";
			this.apply.Click += new System.EventHandler(this.apply_Click);
			// 
			// iFolderAdvanced
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(360, 462);
			this.Controls.Add(this.apply);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.tabControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "iFolderAdvanced";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.iFolderAdvanced_Load);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.accessControlButtons.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Private Methods
		private void ProcessChanges()
		{
			// Change the pointer to an hourglass.
			Cursor = Cursors.WaitCursor;

			foreach (ListViewItem lvitem in this.shareWith.Items)
			{
				// If the item is newly added or changed, then process it.
				if (((ShareListContact)lvitem.Tag).Added || ((ShareListContact)lvitem.Tag).Changed)
				{
					// Get the rights for this contact.
					Access.Rights rights;
					switch (lvitem.SubItems[1].Text)
					{
						case "Full Control":
						{
							rights = Access.Rights.Admin;
							break;
						}
						case "Read/Write":
						{
							rights = Access.Rights.ReadWrite;
							break;
						}
						case "Read Only":
						{
							rights = Access.Rights.ReadOnly;
							break;
						}
						default:
						{
							rights = Access.Rights.Deny;
							break;
						}
					}

					// Reset the flags.
					((ShareListContact)lvitem.Tag).Added = false;
					((ShareListContact)lvitem.Tag).Changed = false;

					try
					{
						// Set the ACE and send an invitation.
						ifolder.Share(((ShareListContact)lvitem.Tag).CurrentContact.Identity, rights, true);
					}
					catch (Exception e)
					{
						// TODO
						MessageBox.Show("Share failed with the following exception: \n\n" + e.Message, "Share Failure");
					}
				}
			}

			// process the removedList
			if (this.removedList != null)
			{
				foreach (ShareListContact slContact in this.removedList)
				{
					try
					{
						// Remove the ACE and don't send an invitation.
						ifolder.Share(slContact.CurrentContact.Identity, Access.Rights.Deny, false);

						// Remove this entry from the list.
						removedList.Remove(slContact);
					}
					catch (Exception e)
					{
						//TODO
						MessageBox.Show("Remove failed with the following exception: \n\n" + e.Message, "Remove Failure");
					}
				}
			}

			// Restore the cursor.
			Cursor = Cursors.Default;
		}

		private bool IsCurrentUserValid()
		{
			// TODO - may need to actually check the current user.
			if (this.shareWith.Items.Count == 1)
			{
				Contact currentContact = ((ShareListContact)this.shareWith.Items[0].Tag).CurrentContact;
				if (currentContact.FN == null ||
					currentContact.EMail == null)
				{
					if (MessageBox.Show("Before you can share, you must add some data to your address book entry.  Do you want to add this information now?", "Incomplete Address Book Entry", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
					{
						ContactEditor editor = new ContactEditor();
						editor.CurrentContact = currentContact;
						editor.CurrentAddressBook = defaultAddressBook;
						if (editor.ShowDialog() != DialogResult.OK)
						{
							return false;
						}
					}
					else
					{
						return false;
					}
				}
			}

			return true;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Sets the current Address Book manager.
		/// </summary>
		public Novell.AddressBook.Manager ABManager
		{
			set
			{
				this.abManager = value;
			}
		}

		/// <summary>
		/// Sets the current iFolder.
		/// </summary>
		public iFolder CurrentiFolder
		{
			set
			{
				this.ifolder = value;
			}
		}
		#endregion

		#region Event Handlers
		private void iFolderAdvanced_Load(object sender, EventArgs e)
		{
			defaultAddressBook = abManager.OpenDefaultAddressBook();

			// Enable/disable the Add button.
			this.add.Enabled = ifolder.IsShareable();

			// Get the access control list for the collection.
			ICSList aclList = ifolder.GetShareAccess();
			Contact contact = null;

			// Change the pointer to an hourglass.
			Cursor = Cursors.WaitCursor;

			foreach (AccessControlEntry ace in aclList)
			{
				if (!ace.WellKnown)
				{
					try
					{
						contact = defaultAddressBook.GetContactByIdentity(ace.Id);
					}
					catch{}

					if (contact == null)
					{
						// TODO - Search the other address books for this Id.

						
					}

					if (contact != null)
					{
						string[] items = new string[2];
						if (contact.FN != null)
						{
							items[0] = contact.FN;
						}
						else
						{
							items[0] = contact.UserName;
						}

						switch (ace.Rights)
						{
							case Access.Rights.Admin:
							{
								items[1] = "Full Control";
								break;
							}
							case Access.Rights.ReadWrite:
							{
								items[1] = "Read/Write";
								break;
							}
							case Access.Rights.ReadOnly:
							{
								items[1] = "Read Only";
								break;
							}
							default:
							{
								items[1] = "Unknown";
								break;
							}
						}

						ListViewItem lvitem = new ListViewItem(items);
						ShareListContact shareContact = new ShareListContact();
						shareContact.CurrentContact = contact;
						lvitem.Tag = shareContact;
						shareWith.Items.Add(lvitem);
					}
				}
			}

			// Restore the cursor.
			Cursor = Cursors.Default;
		}

		private void shareWith_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (shareWith.SelectedItems.Count == 0)
			{
				this.accessControlButtons.Enabled = false;
				this.remove.Enabled = false;
				this.reinvite.Enabled = false;
			}
			else if (shareWith.SelectedItems.Count > 1)
			{
				this.accessControlButtons.Enabled = ifolder.IsShareable();
				this.accessReadOnly.Checked = false;
				this.accessReadWrite.Checked = false;
				this.accessFullControl.Checked = false;
			}
			else
			{
				this.reinvite.Enabled = ifolder.IsShareable();

				ListViewItem item = shareWith.SelectedItems[0];

				if (ifolder.IsShareable())
				{
					// If the current user has sufficient rights, enable the Remove button when
					// a different user is selected in the list.
					this.remove.Enabled = !((ShareListContact)item.Tag).CurrentContact.IsCurrentUser;

					// Enable the rights if the current user is not selected.
					this.accessControlButtons.Enabled = !((ShareListContact)item.Tag).CurrentContact.IsCurrentUser;
				}
				else
				{
					this.accessControlButtons.Enabled = false;
				}

				switch (item.SubItems[1].Text)
				{
					case "Full Control":
					{
						this.accessFullControl.Checked = true;
						break;
					}
					case "Read/Write":
					{
						this.accessReadWrite.Checked = true;
						break;
					}
					case "Read Only":
					{
						this.accessReadOnly.Checked = true;
						break;
					}
					default:
					{
						this.accessReadOnly.Checked = false;
						this.accessReadWrite.Checked = false;
						this.accessFullControl.Checked = false;
						break;
					}
				}
			}		
		}

		private void accessButton_Click(object sender, EventArgs e)
		{
			string access;
			if (this.accessFullControl.Checked)
			{
				access = "Full Control";
			}
			else if ( this.accessReadWrite.Checked)
			{
				access = "Read/Write";
			}
			else
			{
				access = "Read Only";
			}

			foreach (ListViewItem item in this.shareWith.SelectedItems)
			{
				if (((ShareListContact)item.Tag).CurrentContact.IsCurrentUser)
				{
					// Don't allow current user to be modified.
				}
				else if (item.SubItems[1].Text != access)
				{
					// Change the subitem text.
					item.SubItems[1].Text = access;

					// Mark this item as changed.
					((ShareListContact)item.Tag).Changed = true;

					// Enable the apply button.
					this.apply.Enabled = true;
				}
			}
		}

		private void add_Click(object sender, System.EventArgs e)
		{
			if (!IsCurrentUserValid())
				return;

			// TODO - Initialize the picker with the names that are already in the share list.
			ContactPicker picker = new ContactPicker();
			picker.CurrentManager = abManager;
			DialogResult result = picker.ShowDialog();
			if (result == DialogResult.OK)
			{
				// Get the list of added items from the picker.
				ArrayList contactList = picker.GetContactList;
				foreach (Contact c in contactList)
				{
					// Enable the apply button.
					this.apply.Enabled = true;

					// Initialize a listview item.
					string[] items = new string[2];
					items[0] = c.FN;
					items[1] = "Read/Write";
					ListViewItem lvitem = new ListViewItem(items);

					ShareListContact shareContact = null;

					// Check to see if this contact was originally in the list.
					if (this.removedList != null)
					{
						foreach (ShareListContact slContact in removedList)
						{
							if (c.ID == slContact.CurrentContact.ID)
							{
								// The name may be different and we don't know what the rights used to be,
								// so create a new object to represent this item.
								shareContact = new ShareListContact();//(c, false, true);
								shareContact.CurrentContact = c;
								shareContact.Added = false;
								shareContact.Changed = true;
								removedList.Remove(slContact);
								break;
							}
						}
					}

					if (shareContact == null)
					{
						// The contact was not in the removed list, so create a new one.
						shareContact = new ShareListContact();//(c, true);
						shareContact.CurrentContact = c;
						shareContact.Added = true;
					}

					lvitem.Tag = shareContact;
					this.shareWith.Items.Add(lvitem);
				}
			}
		}

		private void remove_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem lvitem in this.shareWith.SelectedItems)
			{
				// Don't allow the current user to be removed.
				if (!((ShareListContact)lvitem.Tag).CurrentContact.IsCurrentUser)
				{
					// If this item is not newly added, we need to add it to the removedList.
					if (!((ShareListContact)lvitem.Tag).Added)
					{
						// Make sure the removed list is valid.
						if (this.removedList == null)
						{
							this.removedList = new ArrayList();
						}

						removedList.Add(lvitem.Tag);
					}

					lvitem.Remove();

					// Enable the apply button.
					this.apply.Enabled = true;
				}
			}		
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			this.ProcessChanges();
		}

		private void reinvite_Click(object sender, System.EventArgs e)
		{
			if (!IsCurrentUserValid())
				return;

			// Change the pointer to an hourglass.
			Cursor = Cursors.WaitCursor;
			
			foreach (ListViewItem lvitem in this.shareWith.Items)
			{
				// Get the rights for this contact.
				Access.Rights rights;
				switch (lvitem.SubItems[1].Text)
				{
					case "Full Control":
					{
						rights = Access.Rights.Admin;
						break;
					}
					case "Read/Write":
					{
						rights = Access.Rights.ReadWrite;
						break;
					}
					case "Read Only":
					{
						rights = Access.Rights.ReadOnly;
						break;
					}
					default:
					{
						rights = Access.Rights.Deny;
						break;
					}
				}

				// Reset the listview item since it has been committed.
				((ShareListContact)lvitem.Tag).Added = false;
				((ShareListContact)lvitem.Tag).Changed = false;

				try
				{
					// Set the ACE and send an invitation.
					ifolder.Share(((ShareListContact)lvitem.Tag).CurrentContact.Identity, rights, true);
				}
				catch
				{
					// TODO
				}
			}		

			// Disable the apply button.
			this.apply.Enabled = false;

			// Restore the cursor.
			Cursor = Cursors.Default;
		}

		private void apply_Click(object sender, System.EventArgs e)
		{
			this.ProcessChanges();
		
			// Disable the apply button.
			this.apply.Enabled = false;
		}
		#endregion
	}

	public class ShareListContact
	{
		private Contact contact;
		private bool added = false;
		private bool changed = false;

		#region Constructors
		public ShareListContact()
		{
		}

/*		public ShareListContact(Contact contact)
		{
			this.contact = contact;
		}

		public ShareListContact(Contact contact, bool added)
		{
			this.contact = contact;
			this.added = added;
		}

		public ShareListContact(Contact contact, bool added, bool changed)
		{
			this.contact = contact;
			this.added = added;
			this.changed = changed;
		}*/
		#endregion

		#region Properties
		/// <summary>
		/// Gets and Sets the Added flag.
		/// </summary>
		public bool Added
		{
			get
			{
				return this.added;
			}
			set
			{
				this.added = value;
			}
		}

		/// <summary>
		/// Gets and Sets the Changed flag.
		/// </summary>
		public bool Changed
		{
			get
			{
				return this.changed;
			}
			set
			{
				this.changed = value;
			}
		}

		/// <summary>
		/// Gets and Sets the current contact.
		/// </summary>
		public Contact CurrentContact
		{
			get
			{
				return this.contact;
			}
			set
			{
				this.contact = value;
			}
		}
		#endregion
	}
}
