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
using Gtk;

namespace Novell.iFolder
{

	/// <summary>
	/// This is the properties dialog for an iFolder.
	/// </summary>
	public class iFolderMemberSelector : Dialog
	{
		private iFolderWebService	ifws;
		private iFolder				ifolder;
		private Gtk.TreeView		MemberTreeView;
		private Gtk.ListStore		MemberTreeStore;
		private Gdk.Pixbuf			MemberPixBuf;

		/// <summary>
		/// Default constructor for iFolderPropertiesDialog
		/// </summary>
		public iFolderMemberSelector(	Gtk.Window parent,
										iFolder ifolder, 
										iFolderWebService iFolderWS)
			: base()
		{
			this.Title = "iFolder Member Selector";
			if(iFolderWS == null)
				throw new ApplicationException("iFolderWebServices was null");
			this.ifws = iFolderWS;
			this.ifolder = ifolder;
			this.HasSeparator = false;
			this.BorderWidth = 6;
			this.Resizable = true;
			this.Modal = true;
			if(parent != null)
				this.TransientFor = parent;

			InitializeWidgets();
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void InitializeWidgets()
		{
			this.SetDefaultSize (200, 400);
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("contact.png"));


			// Create the main TreeView and add it to a scrolled
			// window, then add it to the main vbox widget
			MemberTreeView = new TreeView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.Add(MemberTreeView);
			this.VBox.PackStart(sw, true, true, 0);


			// Setup the iFolder TreeView
			MemberTreeStore = new ListStore(typeof(Member));
			MemberTreeView.Model = MemberTreeStore;

			// Setup Pixbuf and Text Rendering for "iFolders" column
			CellRendererPixbuf mcrp = new CellRendererPixbuf();
			TreeViewColumn memberColumn = new TreeViewColumn();
			memberColumn.PackStart(mcrp, false);
			memberColumn.SetCellDataFunc(mcrp, new TreeCellDataFunc(
						MemberCellPixbufDataFunc));
			CellRendererText mcrt = new CellRendererText();
			memberColumn.PackStart(mcrt, false);
			memberColumn.SetCellDataFunc(mcrt, new TreeCellDataFunc(
						MemberCellTextDataFunc));
			memberColumn.Title = "iFolder Users";
			memberColumn.Resizable = true;
			MemberTreeView.AppendColumn(memberColumn);


//			MemberTreeView.Selection.Changed += new EventHandler(
//						OnMemberSelectionChanged);

//			MemberTreeView.ButtonPressEvent += new ButtonPressEventHandler(
//						OnMemberButtonPressed);

//			MemberTreeView.RowActivated += new RowActivatedHandler(
//						OnMemberRowActivated);

			MemberPixBuf = 
				new Gdk.Pixbuf(Util.ImagesPath("contact.png"));

			this.AddButton(Stock.Close, ResponseType.Ok);
			this.AddButton(Stock.Help, ResponseType.Help);

			RefreshMemberList();
		}




		private void RefreshMemberList()
		{
			Member[] memlist = ifws.GetAllMembers();
			foreach(Member mem in memlist)
			{
				MemberTreeStore.AppendValues(mem);
			}
		}




		private void MemberCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Member member = (Member) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = member.Name;
		}




		private void MemberCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Member member = (Member) tree_model.GetValue(iter,0);
			((CellRendererPixbuf) cell).Pixbuf = MemberPixBuf;
		}





	}
}
