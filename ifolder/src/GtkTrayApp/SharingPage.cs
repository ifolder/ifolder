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
using System.Collections;
using System.Drawing;
using Novell.AddressBook;
using Simias.Storage;

using Gtk;
using Gdk;
using Gnome;
using Glade;
using GtkSharp;
using GLib;

namespace Novell.iFolder
{
	public class SharingListHolder
	{
		private Access.Rights	rights;
		private string			identity;
		private Contact			contact;

		public SharingListHolder( Access.Rights rights,
				string identity, Contact contact)
		{
			this.rights = rights;
			this.identity = identity;
			this.contact = contact;
		}

		public Access.Rights Rights
		{
			get
			{
				return(rights);
			}

			set
			{
				this.rights = value;
			}
		}

		public string Identity
		{
			get
			{
				return(identity);
			}
		}

		public Contact ABContact
		{
			get
			{
				return contact;
			}
		}
	}



	public class SharingPage
	{
		[Glade.Widget] TreeView		ContactTreeView;
		[Glade.Widget] Button		addSharingButton;
		[Glade.Widget] Button		removeSharingButton;
		[Glade.Widget] RadioButton	FullControlRB;
		[Glade.Widget] RadioButton	ReadWriteRB;
		[Glade.Widget] RadioButton	ReadOnlyRB;

		Gtk.VBox SharingVBox;
		iFolder  ifldr;
		ListStore ContactTreeStore;
		Pixbuf	ContactPixBuf;
		Pixbuf	CurContactPixBuf;
		ContactPicker cp;
		Novell.AddressBook.Manager 	abMan;
		AddressBook	dAddrBook;
		ArrayList	guidList;

		public SharingPage (iFolder ifolder)
		{
			ifldr = ifolder;

			Glade.XML gxml = new Glade.XML ("ifolder.glade", 
					"SharingPropertyPage", null);
			gxml.Autoconnect (this);

			SharingVBox = (Gtk.VBox) gxml.GetWidget("SharingPropertyPage");

			ContactTreeStore = new ListStore(typeof(SharingListHolder));
			ContactTreeView.Model = ContactTreeStore;
			CellRendererPixbuf ccrp = new CellRendererPixbuf();
			TreeViewColumn ctvc = new TreeViewColumn();
			ctvc.PackStart(ccrp, false);
			ctvc.SetCellDataFunc(ccrp, 
					new TreeCellDataFunc(ContactCellPixbufDataFunc));

			CellRendererText ccrt = new CellRendererText();
			ctvc.PackStart(ccrt, false);
			ctvc.SetCellDataFunc(ccrt, 
					new TreeCellDataFunc(ContactCellTextDataFunc));
			ctvc.Title = "Contacts";
			ContactTreeView.AppendColumn(ctvc);

			ContactTreeView.AppendColumn("Access", 
					new CellRendererText(), 
					new TreeCellDataFunc(AccessCellTextDataFunc));
			
			ContactTreeView.Selection.Changed += 
					new EventHandler(on_selection_changed);

			ContactPixBuf = new Pixbuf("contact.png");
			CurContactPixBuf = new Pixbuf("contact_me.png");

			guidList = new ArrayList();

			abMan = Novell.AddressBook.Manager.Connect( );
			if(abMan == null)
				Console.WriteLine("What is up with ABMan?");

			dAddrBook = abMan.OpenDefaultAddressBook();

			ICSList acl = ifldr.GetShareAccess();

			foreach(AccessControlEntry ace in acl)
			{
				if(ace.WellKnown != true)
				{
					try
					{
						Contact con = dAddrBook.GetContact(ace.Id);
						SharingListHolder slh = new SharingListHolder(
								ace.Rights, ace.Id, con);
						ContactTreeStore.AppendValues(slh);
						guidList.Add(ace.Id);
					}
					catch(Exception e)
					{
						SharingListHolder slh = new SharingListHolder(
								ace.Rights, ace.Id, null);
						ContactTreeStore.AppendValues(slh);
					}
				}
			}
			if(ifldr.IsShareable())
				addSharingButton.Sensitive = true;
			else
			{
				addSharingButton.Sensitive = false;

				FullControlRB.Sensitive = false;
				ReadWriteRB.Sensitive = false;
				ReadOnlyRB.Sensitive = false;
			}
			// Always set remove to false until someone is selected
			removeSharingButton.Sensitive = false;
		}

		public Gtk.Widget GetWidget()
		{
			return SharingVBox;
		}

		private void ContactCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			SharingListHolder slh = (SharingListHolder)
				ContactTreeStore.GetValue(iter,0);
			if(slh.ABContact != null)
			{
				string userName = slh.ABContact.FN;
				if(userName == null)
					userName = slh.ABContact.UserName;
				((CellRendererText) cell).Text = userName;
			}
			else
				((CellRendererText) cell).Text = slh.Identity;
		}

		private void ContactCellPixbufDataFunc(Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			SharingListHolder slh = (SharingListHolder) ContactTreeStore.GetValue(iter,0);
			if( (slh != null) && (slh.ABContact != null) && slh.ABContact.IsCurrentUser)
				((CellRendererPixbuf) cell).Pixbuf = CurContactPixBuf;
			else
				((CellRendererPixbuf) cell).Pixbuf = ContactPixBuf;
		}

		private void AccessCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			SharingListHolder slh = (SharingListHolder)
				ContactTreeStore.GetValue(iter,0);
			switch(slh.Rights)
			{
				case Access.Rights.Deny:
					((CellRendererText) cell).Text = "No Access";
					break;
				case Access.Rights.ReadOnly:
					((CellRendererText) cell).Text = "Read Only";
					break;
				case Access.Rights.ReadWrite:
					((CellRendererText) cell).Text = "Read / Write";
					break;
				case Access.Rights.Admin:
					((CellRendererText) cell).Text = "Full Control";
					break;
			}
		}

		private void on_selection_changed(object o, EventArgs args)
		{
			TreeSelection tSelect = ContactTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				tModel = null;
				SharingListHolder slh = (SharingListHolder) 
						ContactTreeStore.GetValue(iter,0);

				// Check the identity here
				// If it is the owner, dont' let them deny themselves
				if( (!ifldr.IsShareable()) ||
					(slh.Identity == ifldr.OwnerIdentity) )
				{
					removeSharingButton.Sensitive = false;
					FullControlRB.Sensitive = false;
					ReadWriteRB.Sensitive = false;
					ReadOnlyRB.Sensitive = false;
				}
				else
				{
					removeSharingButton.Sensitive = true;
					FullControlRB.Sensitive = true;
					ReadWriteRB.Sensitive = true;
					ReadOnlyRB.Sensitive = true;
				}

				switch(slh.Rights)
				{
					case Access.Rights.Deny:
						break;
					case Access.Rights.ReadOnly:
						ReadOnlyRB.Active = true;
						break;
					case Access.Rights.ReadWrite:
						ReadWriteRB.Active = true;
						break;
					case Access.Rights.Admin:
						FullControlRB.Active = true;
						break;
				}
			}
		}

		private void on_add_sharing(object o, EventArgs args) 
		{
			bool editContact = false;
			Contact owner = null;

			try
			{
				owner = dAddrBook.GetContact(ifldr.OwnerIdentity);
				if(owner.GetPreferredName() == null)
					editContact = true;
				if(owner.EMail == null)
					editContact = true;
			}
			catch(Exception e)
			{
				editContact = true;
			}

			if(editContact)
			{
				MessageDialog md = new MessageDialog(null,
						DialogFlags.DestroyWithParent | DialogFlags.Modal,
						MessageType.Error,
						ButtonsType.YesNo,
						"Your contact information does not contain your preferred name and email address.  You will not be able to invite others to your iFolder without this information.  Would you like to edit that information now?");
				int result = md.Run();
				md.Hide();

				if(result == -8)
				{
					ContactEditor ce;
					if(owner == null)
					{
						MessageDialog med = new MessageDialog(null,
							DialogFlags.DestroyWithParent | DialogFlags.Modal,
							MessageType.Error,
							ButtonsType.Close,
							"Your identity in iFolder is corrupt and you will not be able to share with other people.  Please contact Brady Anderson (banderson@novell.com) for assistance.");
						med.Run();
						med.Hide();
						return;
					}
					else
						ce = new ContactEditor((Gtk.Window)GetWidget().Toplevel, owner, false);

					ce.ContactEdited += new ContactEditEventHandler(
							CreateContactEventHandler);
					ce.ShowAll();
				}
			}
			else
			{
				if( (cp == null) || (!cp.IsValid()) )
				{
					cp = new ContactPicker((Gtk.Window)GetWidget().Toplevel);
					cp.ContactsPicked += new ContactsPickedEventHandler(
							onContactsPicked);
				}

				cp.ShowAll();
			}
		}

		public void CreateContactEventHandler(object o,
				ContactEditEventArgs args)
		{
			Contact contact = args.ABContact;
			contact.Commit();
		}

		public void onContactsPicked(object o, ContactsPickedEventArgs args)
		{
			Contact c = args.contact;

			if(!guidList.Contains(c.Identity))
			{
				try
				{
					ifldr.Share(c.ID, Access.Rights.ReadWrite, true);

					SharingListHolder slh = new SharingListHolder(
							Access.Rights.ReadWrite, c.ID, c);
					ContactTreeStore.AppendValues(slh);
					guidList.Add(c.ID);
				}
				catch(Exception e)
				{
					Console.WriteLine(e);
					Console.WriteLine("Didn't share with contact: " + c.UserName);
				}
			}
			else
			{
				Console.WriteLine("Identity Exists: " + c.Identity);
			}
		}

		private void SetCurrentAccessRights(Access.Rights rights)
		{
			TreeSelection tSelect = ContactTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				SharingListHolder slh = (SharingListHolder)
						ContactTreeStore.GetValue(iter,0);

				ifldr.SetShareAccess(slh.Identity, rights);
				slh.Rights = rights;
				tModel.SetValue(iter, 0, slh);
			}
		}

		private void on_remove_sharing(object o, EventArgs args) 
		{
			TreeSelection tSelect = ContactTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				tModel = null;
				SharingListHolder slh = (SharingListHolder)
						ContactTreeStore.GetValue(iter,0);

				ifldr.RemoveUserAccess(slh.Identity);
				ContactTreeStore.Remove(out iter);
				guidList.Remove(slh.Identity);
				removeSharingButton.Sensitive = false;
				FullControlRB.Sensitive = false;
				ReadWriteRB.Sensitive = false;
				ReadOnlyRB.Sensitive = false;
			}
		}

		private void on_readwrite_clicked(object o, EventArgs args)
		{
			SetCurrentAccessRights(Access.Rights.ReadWrite);
		}

		private void on_readonly_clicked(object o, EventArgs args)
		{
			SetCurrentAccessRights(Access.Rights.ReadOnly);
		}

		private void on_fullcontrol_clicked(object o, EventArgs args)
		{
			SetCurrentAccessRights(Access.Rights.Admin);
		}

		private void on_unrealize(object o, EventArgs args) 
		{
			// Close out the contact picker
			if(cp != null)
			{
				cp.Close();
				cp = null;
			}
		}
	}
}
