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
using System.IO;
using System.Drawing;
using System.Collections;
using Simias.Storage;
using Simias.POBox;
using Simias;
using Novell.AddressBook;
using Novell.AddressBook.UI.gtk;

using Gtk;
using Gdk;
using Glade;
using GtkSharp;
using GLib;


namespace Novell.iFolder
{
	public class iFolderProperties 
	{
		[Glade.Widget] private Gtk.Dialog 		PropDialog = null;
		[Glade.Widget] private Gtk.Notebook		propNoteBook = null;

		// General Page members
		[Glade.Widget] private Gtk.CheckButton AutoSyncCheckButton = null;
		[Glade.Widget] private Gtk.SpinButton  RefreshSpinButton = null;


		// Sharing Page members
		[Glade.Widget] private TreeView		ContactTreeView = null;
		[Glade.Widget] private Button		addSharingButton = null;
		[Glade.Widget] private Button		removeSharingButton = null;
		[Glade.Widget] private RadioButton	FullControlRB = null;
		[Glade.Widget] private RadioButton	ReadWriteRB = null;
		[Glade.Widget] private RadioButton	ReadOnlyRB = null;
		private ListStore ContactTreeStore;
		private Pixbuf	ContactPixBuf;
		private Pixbuf	CurContactPixBuf;
		private Pixbuf	InvContactPixBuf;
		private Novell.AddressBook.Manager 	abMan;
//		private AddressBook	dAddrBook;
		private ArrayList	guidList;

		// All Properties Page members
		[Glade.Widget] private TreeView	PropertyTreeView = null;
		private ListStore PropertyTreeStore;
		private Pixbuf	PropertyPixBuf;
//		private Node	node;

		private int activeTag = 0;
		private iFolder ifolder;
		private POBox	pobox;
		private string ifolderPath;

		public Gtk.Window TransientFor
		{
			set
			{
				if(PropDialog != null)
					PropDialog.TransientFor = value;
			}
		}

		public int ActiveTag
		{
			set
			{
				activeTag = value;
			}
		}

		public iFolder CurrentiFolder
		{
			get
			{
				return ifolder;
			}

			set
			{
				ifolder = value;
			}
		}

		public string iFolderPath
		{
			get
			{
				return ifolderPath;
			}

			set
			{
				ifolderPath = value;
			}
		}

		public iFolderProperties() 
		{
		}

		public void InitGlade()
		{
			Glade.XML gxml = 
					new Glade.XML (Util.GladePath("ifolder-properties.glade"), 
					"PropDialog", 
					null);
			gxml.Autoconnect (this);

			Init_Sharing_Page();
			Init_All_Properties_Page();
		}

		public int Run()
		{
			int rc = 0;

			if(ifolder == null)
			{
				iFolderManager ifmgr;

				ifmgr = iFolderManager.Connect();

				if(ifmgr.IsiFolder(ifolderPath))
				{
					ifolder = ifmgr.GetiFolderByPath(ifolderPath);
				}
			}

			if(ifolder != null)
			{
				if(pobox == null)
				{
					pobox = POBox.GetPOBox(ifolder.StoreReference,
								ifolder.StoreReference.DefaultDomain);
				}

				InitGlade();
				PopulateWidgets();

				if(PropDialog != null)
				{
					if(propNoteBook.NPages >= activeTag)
						propNoteBook.CurrentPage = activeTag;

					while(rc == 0)
					{
						rc = PropDialog.Run();
						if(rc == -11) // help
						{
							rc = 0;
							switch(propNoteBook.CurrentPage)
							{
								case 1:
									Util.ShowHelp("bq6lwlu.html", null);
									break;
								case 2:
									Util.ShowHelp("bq6lwlj.html", null);
									break;
								default:
									Util.ShowHelp("front.html", null);
									break;
							}
						}
					}

					PropDialog.Hide();
					PropDialog.Destroy();
					PropDialog = null;
				}
			}
			else
			{
				Console.WriteLine("That ain't an iFolder");
			}
			return rc;
		}

		private void PopulateWidgets()
		{
			AutoSyncCheckButton.Active = true;
			RefreshSpinButton.Value = ifolder.RefreshInterval;
		}

		private void on_AutoSyncCheckButton_toggled(object o, EventArgs args)
		{
			if(AutoSyncCheckButton.Active == true)
				Console.WriteLine("Auto Sync == true");
			else
				Console.WriteLine("Auto Sync == false");
		}

		private void on_RefreshSpinButton_changed(object o, EventArgs args)
		{
			ifolder.RefreshInterval = (int)RefreshSpinButton.Value;
		}



		//----------------------------------------------
		// Sharing Page methods
		//----------------------------------------------

		private void Init_Sharing_Page()
		{
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

			ContactTreeView.AppendColumn("State", 
					new CellRendererText(), 
					new TreeCellDataFunc(StateCellTextDataFunc));

			ContactTreeView.AppendColumn("Access", 
					new CellRendererText(), 
					new TreeCellDataFunc(AccessCellTextDataFunc));
			
			ContactTreeView.Selection.Changed += 
					new EventHandler(on_selection_changed);

			ContactPixBuf = new Pixbuf(Util.ImagesPath("contact.png"));
			CurContactPixBuf = new Pixbuf(Util.ImagesPath("contact_me.png"));
			InvContactPixBuf = new Pixbuf(Util.ImagesPath("invited-contact.png"));

			guidList = new ArrayList();

			abMan = Novell.AddressBook.Manager.Connect( );
			if(abMan == null)
				Console.WriteLine("What is up with ABMan?");

//			dAddrBook = abMan.OpenDefaultAddressBook();
			
			ICSList memList = ifolder.GetMemberList();
			foreach(ShallowNode sNode in memList)
			{
				Member m = new Member(ifolder, sNode);
				SharingListHolder slh = new SharingListHolder(
					m, null);

				ContactTreeStore.AppendValues(slh);
				guidList.Add(m.UserID);
			}

			ICSList poList = pobox.Search(
					Subscription.SubscriptionCollectionIdProperty,
					ifolder.ID,
					SearchOp.Equal);
			
			foreach(ShallowNode sNode in poList)
			{
				Subscription sub = new Subscription(pobox, sNode);
				SharingListHolder slh = new SharingListHolder(
					null, sub);

				ContactTreeStore.AppendValues(slh);
			}

			if(ifolder.IsShareable(ifolder.GetCurrentMember()))
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

		private void ContactCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			SharingListHolder slh = (SharingListHolder)
				ContactTreeStore.GetValue(iter,0);
			if(slh.Member != null)
			{
				((CellRendererText) cell).Text = slh.Member.Name;
			}
			else if(slh.Subscription != null)
			{
				((CellRendererText) cell).Text = slh.Subscription.ToName;
			}
			else
				((CellRendererText) cell).Text = "null user";
		}

		private void ContactCellPixbufDataFunc(Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			SharingListHolder slh = 
					(SharingListHolder) ContactTreeStore.GetValue(iter,0);
			if( (slh != null) && (slh.Member != null) &&
				(ifolder.GetCurrentMember().UserID == slh.Member.UserID) )
				((CellRendererPixbuf) cell).Pixbuf = CurContactPixBuf;
			else if( (slh != null) && (slh.Subscription != null) )
				((CellRendererPixbuf) cell).Pixbuf = InvContactPixBuf;
			else
				((CellRendererPixbuf) cell).Pixbuf = ContactPixBuf;
		}

		private void StateCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			SharingListHolder slh = 
					(SharingListHolder) ContactTreeStore.GetValue(iter,0);
			if( (slh != null) && (slh.Member != null) )
			{
				if(ifolder.GetCurrentMember().UserID == slh.Member.UserID)
					((CellRendererText) cell).Text = "Owner";
				else
					((CellRendererText) cell).Text = "Member";
			}
			else if( (slh != null) && (slh.Subscription != null) )
				((CellRendererText) cell).Text = slh.Subscription.SubscribeState.ToString();
			else
				((CellRendererText) cell).Text = "";
		}


		private void AccessCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Access.Rights rights = 0;

			SharingListHolder slh = (SharingListHolder)
				ContactTreeStore.GetValue(iter,0);
			if(slh.Member != null)
			{
				rights = slh.Member.Rights;
			}
			else if(slh.Subscription != null)
			{
				rights = slh.Subscription.SubscriptionRights;
			}

			switch(rights)
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
				if(tModel != null)
					tModel = null;
				SharingListHolder slh = (SharingListHolder) 
					ContactTreeStore.GetValue(iter,0);

				// Check the identity here
				// If it is the owner, dont' let them deny themselves
				Member curMember = ifolder.GetCurrentMember();
				Access.Rights curRights = 0;
				if(slh.Member != null)
					curRights = slh.Member.Rights;
				else if(slh.Subscription != null)
					curRights = slh.Subscription.SubscriptionRights;

				if( (ifolder.IsShareable(curMember) != true) ||
					( (slh.Member != null) && 
					(slh.Member.UserID == curMember.UserID) ) )
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

				switch(curRights)
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
/*
			try
			{
				owner = dAddrBook.GetContact(ifolder.OwnerIdentity);
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
					{
						ContactEditor ce = new ContactEditor();
						ce.TransientFor = PropDialog;
						ce.Contact = owner;
						ce.Run();
						owner.Commit();
					}
				}
			}
			else
			{
*/
				ContactPicker cp = new ContactPicker();
				cp.TransientFor = PropDialog;
				cp.AddrBookManager = abMan;
				if(cp.Run() == -5)
				{
					foreach(Contact c in cp.Contacts)
					{
						ShareWithContact(c);
					}
				}
//			}
		}

		public void ShareWithContact(Contact c)
		{

			Subscription sub = pobox.CreateSubscription(ifolder,
										ifolder.GetCurrentMember());

			sub.FromAddress = String.Format("{0}@{1}", 
									System.Environment.UserName, 
									Simias.MyDns.GetHostName());

			sub.SubscriptionRights = Access.Rights.ReadWrite;
			sub.ToName = c.FN;
			sub.ToAddress = c.EMail;
			pobox.AddMessage(sub);

			SharingListHolder slh = new SharingListHolder(null, sub);

			ContactTreeStore.AppendValues(slh);
/*
			if(!guidList.Contains(c.ID))
			{
				try
				{
					ifolder.SetRights(c, iFolder.Rights.ReadWrite);
					guidList.Add(c.ID);
					SharingListHolder slh = new SharingListHolder(
							iFolder.Rights.ReadWrite, c);
					ContactTreeStore.AppendValues(slh);
				}
				catch(Exception e)
				{
					Console.WriteLine(e);
					Console.WriteLine("Error in SetRights on : " + c.UserName);
					return;
				}

				try
				{
					ifolder.Invite(c);
				}
				catch(Exception e)
				{
					Console.WriteLine(e);
					return;
				}
			}
			else
			{
				Console.WriteLine("Identity Exists: " + c.ID);
			}
*/
		}

		private void SetCurrentAccessRights(Access.Rights rights)
		{
/*
			TreeSelection tSelect = ContactTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				SharingListHolder slh = (SharingListHolder)
					ContactTreeStore.GetValue(iter,0);

				ifolder.SetRights(slh.Contact, rights);
				slh.Rights = rights;
				tModel.SetValue(iter, 0, slh);
			}
*/
		}

		private void on_remove_sharing(object o, EventArgs args) 
		{
			TreeSelection tSelect = ContactTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);

				SharingListHolder slh = (SharingListHolder)
					tModel.GetValue(iter,0);

//				ifolder.RemoveRights(slh.Contact);
				ContactTreeStore.Remove(ref iter);
				if(slh.Member != null)
					guidList.Remove(slh.Member.UserID);
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

		//----------------------------------------------
		// All Properties Page methods
		//----------------------------------------------

		private void Init_All_Properties_Page()
		{
			PropertyTreeStore = new ListStore(typeof(Simias.Storage.Property));
			PropertyTreeView.Model = PropertyTreeStore;
			CellRendererPixbuf pcrp = new CellRendererPixbuf();
			TreeViewColumn ptvc = new TreeViewColumn();
			ptvc.PackStart(pcrp, false);
			ptvc.SetCellDataFunc(pcrp, new TreeCellDataFunc(PropertyCellPixbufDataFunc));

			CellRendererText pcrt = new CellRendererText();
			ptvc.PackStart(pcrt, false);
			ptvc.SetCellDataFunc(pcrt, new TreeCellDataFunc(PropertyCellTextDataFunc));
			ptvc.Title = "Properties";
			PropertyTreeView.AppendColumn(ptvc);

			PropertyTreeView.AppendColumn("Values", 
					new CellRendererText(), 
					new TreeCellDataFunc(ValueCellTextDataFunc));

			PropertyPixBuf = new Pixbuf(Util.ImagesPath("property.png"));
			foreach(Simias.Storage.Property prop in ifolder.Properties)
			{
				PropertyTreeStore.AppendValues(prop);
			}
		}

		private void PropertyCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Simias.Storage.Property prop = 
				(Simias.Storage.Property)
				PropertyTreeStore.GetValue(iter,0);
			if(prop != null)
			{
				((CellRendererText) cell).Text = prop.Name;
			}
			else
				((CellRendererText) cell).Text = "** unknown **";
		}

		private void PropertyCellPixbufDataFunc(Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			((CellRendererPixbuf) cell).Pixbuf = PropertyPixBuf;
		}

		private void ValueCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Simias.Storage.Property prop = 
				(Simias.Storage.Property)
				PropertyTreeStore.GetValue(iter,0);
			if(prop != null)
			{
				((CellRendererText) cell).Text = prop.Value.ToString();
			}
			else
				((CellRendererText) cell).Text = "** unknown **";
		}
	}




	public class SharingListHolder
	{
		private Member			member;
		private Subscription	sub;

		public SharingListHolder( Member member, Subscription sub)
		{
			this.member = member;
			this.sub = sub;
		}

		public Member Member
		{
			get
			{
				return(member);
			}

			set
			{
				this.member = value;
			}
		}

		public Subscription Subscription
		{
			get
			{
				return sub;
			}
		}
	}
}
