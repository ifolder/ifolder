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
	/// This is the properties page for sharing an iFolder
	/// </summary>
	public class iFolderPropSharingPage : VBox
	{
		private iFolderWebService	ifws;
		private iFolder				ifolder;

		private Gtk.TreeView		MemberTreeView;
		private ListStore			MemberTreeStore;
		private Gdk.Pixbuf			ContactPixBuf;

		/// <summary>
		/// Default constructor for iFolderPropSharingPage
		/// </summary>
		public iFolderPropSharingPage(	iFolder ifolder, 
										iFolderWebService iFolderWS)
			: base()
		{
			this.ifws = iFolderWS;
			this.ifolder = ifolder;
			InitializeWidgets();
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void InitializeWidgets()
		{
			this.Spacing = 6;
			this.BorderWidth = 6;
			
			// Create the main TreeView and add it to a scrolled
			// window, then add it to the main vbox widget
			MemberTreeView = new TreeView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.Add(MemberTreeView);
			this.PackStart(sw, true, true, 0);


			// Setup the iFolder TreeView
			MemberTreeStore = new ListStore(typeof(Member1));
			MemberTreeView.Model = MemberTreeStore;

			CellRendererPixbuf mcrp = new CellRendererPixbuf();
			TreeViewColumn MemberColumn = new TreeViewColumn();
			MemberColumn.PackStart(mcrp, false);
			MemberColumn.SetCellDataFunc(mcrp,
					new TreeCellDataFunc(MemberCellPixbufDataFunc));

			CellRendererText mcrt = new CellRendererText();
			MemberColumn.PackStart(mcrt, false);
			MemberColumn.SetCellDataFunc(mcrt,
					new TreeCellDataFunc(MemberCellTextDataFunc));
			MemberColumn.Title = "Members";
			MemberTreeView.AppendColumn(MemberColumn);

			MemberTreeView.AppendColumn("State",
					new CellRendererText(),
					new TreeCellDataFunc(StateCellTextDataFunc));

			MemberTreeView.AppendColumn("Access",
					new CellRendererText(),
					new TreeCellDataFunc(AccessCellTextDataFunc));

//			MemberTreeView.Selection.Changed +=
//				new EventHandler(OnMemberSelectionChanged);

			ContactPixBuf = 
					new Gdk.Pixbuf(Util.ImagesPath("contact.png"));
//			CurContactPixBuf = 
//					new Gdk.Pixbuf(Util.ImagesPath("contact_me.png"));
//			InvContactPixBuf = 
//					new Gdk.Pixbuf(Util.ImagesPath("invited-contact.png"));



		}




		private void MemberCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Member1 member = (Member1) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = member.Name;
		}




		private void MemberCellPixbufDataFunc(Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Member1 member = (Member1) tree_model.GetValue(iter,0);
			((CellRendererPixbuf) cell).Pixbuf = ContactPixBuf;
		}




		private void StateCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Member1 member = (Member1) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = "Dude";
		}


		private void AccessCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Member1 member = (Member1) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = "No Access";
		}

	}
}
