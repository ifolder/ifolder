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
using System.Collections;

namespace Novell.iFolder
{

	/// <summary>
	/// This is the conflict resolver for iFolder
	/// </summary>
	public class iFolderConflictDialog : Dialog
	{
		private iFolderWebService	ifws;
		private iFolder				ifolder;
		private Gtk.TreeView		ConflictTreeView;
		private Gtk.ListStore		ConflictTreeStore;
		private Gdk.Pixbuf			ConflictPixBuf;




		/// <summary>
		/// Default constructor for iFolderConflictResolver
		/// </summary>
		public iFolderConflictDialog(	Gtk.Window parent,
										iFolder ifolder,
										iFolderWebService iFolderWS)
			: base()
		{
			this.Title = "iFolder Conflict Resolver";
			if(iFolderWS == null)
				throw new ApplicationException("iFolderWebServices was null");
			this.ifws = iFolderWS;
			this.ifolder = ifolder;
			this.HasSeparator = false;
			this.BorderWidth = 10;
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
			this.SetDefaultSize (300, 400);
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder.png"));


			Label l = new Label("<span weight=\"bold\" size=\"larger\">" +
					"Conflicts were found in the following iFolder:</span>");

			l.LineWrap = false;
			l.UseMarkup = true;
			l.Selectable = false;
			l.Xalign = 0; l.Yalign = 0;
			this.VBox.PackStart(l, false, false, 0);
			

			Table ifTable = new Table(2,2,false);
			ifTable.BorderWidth = 6;
			ifTable.RowSpacing = 6;
			ifTable.ColumnSpacing = 6;

			Label nameLabel = new Label("iFolder Name:");
			nameLabel.Xalign = 0;
			ifTable.Attach(nameLabel, 0,1,0,1);

			Label nameValue = new Label(ifolder.Name);
			nameValue.Xalign = 1;
			ifTable.Attach(nameValue, 1,2,0,1);

			Label pathLabel = new Label("iFolder Path:");
			pathLabel.Xalign = 0;
			ifTable.Attach(pathLabel, 0,1,1,2);

			Label pathValue = new Label(ifolder.UnManagedPath);
			pathValue.Xalign = 1;
			ifTable.Attach(pathValue, 1,2,1,2);

			this.VBox.PackStart(ifTable, true, true, 0);


			// Create the main TreeView and add it to a scrolled
			// window, then add it to the main vbox widget
			ConflictTreeView = new TreeView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.Add(ConflictTreeView);
			this.VBox.PackStart(sw, true, true, 0);


			// Setup the iFolder TreeView
			ConflictTreeStore = new ListStore(typeof(Conflict));
			ConflictTreeView.Model = ConflictTreeStore;

			// Setup Pixbuf and Text Rendering for "iFolder Conflicts" column
			CellRendererPixbuf mcrp = new CellRendererPixbuf();
			TreeViewColumn memberColumn = new TreeViewColumn();
			memberColumn.PackStart(mcrp, false);
			memberColumn.SetCellDataFunc(mcrp, new TreeCellDataFunc(
						ConflictCellPixbufDataFunc));
			CellRendererText mcrt = new CellRendererText();
			memberColumn.PackStart(mcrt, false);
			memberColumn.SetCellDataFunc(mcrt, new TreeCellDataFunc(
						ConflictCellTextDataFunc));
			memberColumn.Title = "iFolder Conflicts";
			memberColumn.Resizable = true;
			ConflictTreeView.AppendColumn(memberColumn);
			ConflictTreeView.Selection.Mode = SelectionMode.Multiple;


//			ConflictTreeView.Selection.Changed += new EventHandler(
//						OnConflictSelectionChanged);

//			ConflictTreeView.ButtonPressEvent += new ButtonPressEventHandler(
//						OnConflictButtonPressed);

//			ConflictTreeView.RowActivated += new RowActivatedHandler(
//						OnConflictRowActivated);

			ConflictPixBuf = 
				new Gdk.Pixbuf(Util.ImagesPath("ifolder.png"));

			this.AddButton(Stock.Cancel, ResponseType.Cancel);
			this.AddButton(Stock.Ok, ResponseType.Ok);
			this.AddButton(Stock.Help, ResponseType.Help);

			RefreshConflictList();
		}




		private void RefreshConflictList()
		{
			Conflict[] conflictList = ifws.GetiFolderConflicts(ifolder.ID);
			foreach(Conflict con in conflictList)
			{
				ConflictTreeStore.AppendValues(con);
			}
		}




		private void ConflictCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Conflict con = (Conflict) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = con.LocalName;
		}




		private void ConflictCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
//			iFolderConflict user = (iFolderConflict) tree_model.GetValue(iter,0);
			((CellRendererPixbuf) cell).Pixbuf = ConflictPixBuf;
		}





	}
}
