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
		private iFolderWeb			ifolder;
		private Gtk.TreeView		ConflictTreeView;
		private Gtk.ListStore		ConflictTreeStore;
		private Gdk.Pixbuf			ConflictPixBuf;


		private Gtk.Frame			ServerFrame;
		private Gtk.Frame			LocalFrame;
		private Gtk.Label			LocalNameLabel;
		private Gtk.Label			LocalNameValue;
		private Gtk.Label			LocalDateLabel;
		private Gtk.Label			LocalDateValue;
		private Gtk.Label			LocalSizeLabel;
		private Gtk.Label			LocalSizeValue;
		private Gtk.Button			LocalSaveButton;
		private Gtk.Label			ServerNameLabel;
		private Gtk.Label			ServerNameValue;
		private Gtk.Label			ServerDateLabel;
		private Gtk.Label			ServerDateValue;
		private Gtk.Label			ServerSizeLabel;
		private Gtk.Label			ServerSizeValue;
		private Gtk.Button			ServerSaveButton;


		/// <summary>
		/// Default constructor for iFolderConflictResolver
		/// </summary>
		public iFolderConflictDialog(	Gtk.Window parent,
										iFolderWeb ifolder,
										iFolderWebService iFolderWS)
			: base()
		{
			this.Title = Util.GS("iFolder Conflict Resolver");
			if(iFolderWS == null)
				throw new ApplicationException("iFolderWebServices was null");
			this.ifws = iFolderWS;
			this.ifolder = ifolder;
			this.HasSeparator = false;
			this.Resizable = true;
			this.Modal = true;
			if(parent != null)
				this.TransientFor = parent;

			InitializeWidgets();
			EnableConflictControls(false);
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void InitializeWidgets()
		{
			this.SetDefaultSize (600, 400);
			this.Icon = 
				new Gdk.Pixbuf(Util.ImagesPath("ifolder-collision.png"));
			VBox vbox = new VBox();
			vbox.Spacing = 10;
			vbox.BorderWidth = 10;
			this.VBox.PackStart(vbox, true, true, 0);

			HBox topbox = new HBox();
			topbox.Spacing = 10;

			Image conflictImage = new Image(this.Icon);
			conflictImage.Yalign = 0;
			topbox.PackStart(conflictImage, false, false, 0);

			VBox textbox = new VBox();
			textbox.Spacing = 10;

			Label l = new Label(Util.GS("Select a conflict from the list below.  To resolve the conflict, save the local version or the server version.  The version you save will be synced to this iFolder. "));
			l.LineWrap = true;
			l.Xalign = 0;
			textbox.PackStart(l, true, true, 0);

			Table ifTable = new Table(2,2,false);
			ifTable.ColumnSpacing = 10;
			ifTable.Homogeneous = false;

			Label nameLabel = new Label(Util.GS("iFolder Name:"));
			nameLabel.Xalign = 0;
			ifTable.Attach(nameLabel, 0,1,0,1,
				Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, 0, 0);

			Label nameValue = new Label(ifolder.Name);
			nameValue.Xalign = 0;
			ifTable.Attach(nameValue, 1,2,0,1);

			Label pathLabel = new Label(Util.GS("iFolder Path:"));
			pathLabel.Xalign = 0;
			ifTable.Attach(pathLabel, 0,1,1,2,
				Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, 0, 0);

			Label pathValue = new Label(ifolder.UnManagedPath);
			pathValue.Xalign = 0;
			ifTable.Attach(pathValue, 1,2,1,2);

			textbox.PackStart(ifTable, false, true, 0);

			topbox.PackStart(textbox, true, true, 0);

			vbox.PackStart(topbox, false, true, 0);

			// Create the main TreeView and add it to a scrolled
			// window, then add it to the main vbox widget
			ConflictTreeView = new TreeView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.Add(ConflictTreeView);
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			vbox.PackStart(sw, true, true, 0);


			HBox bottombox = new HBox();
			bottombox.Spacing = 10;

			LocalFrame = new Frame(Util.GS("Local Version"));
			bottombox.PackStart(LocalFrame, true, true, 0);

			Table localTable = new Table(2,4,false);
			localTable.BorderWidth = 10;
			localTable.ColumnSpacing = 10;

			LocalNameLabel = new Label(Util.GS("Name:"));
			LocalNameLabel.Xalign = 0;
			localTable.Attach(LocalNameLabel, 0,1,0,1, 
				Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, 0, 0);

			LocalNameValue = new Label("");
			LocalNameValue.Xalign = 0;
			localTable.Attach(LocalNameValue, 1,2,0,1);

			LocalDateLabel = new Label(Util.GS("Date:"));
			LocalDateLabel.Xalign = 0;
			localTable.Attach(LocalDateLabel, 0,1,1,2,
				Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, 0, 0);

			LocalDateValue = new Label("");
			LocalDateValue.Xalign = 0;
			localTable.Attach(LocalDateValue, 1,2,1,2);

			LocalSizeLabel = new Label(Util.GS("Size:"));
			LocalSizeLabel.Xalign = 0;
			localTable.Attach(LocalSizeLabel, 0,1,2,3,
				Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, 0, 0);

			LocalSizeValue = new Label("");
			LocalSizeValue.Xalign = 0;
			localTable.Attach(LocalSizeValue, 1,2,2,3);

			LocalSaveButton = new Button(Stock.Save);
			localTable.Attach(LocalSaveButton, 0,1,3,4,
				Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, 0, 5);
			LocalSaveButton.Clicked += new EventHandler(SaveLocalHandler);

			LocalFrame.Add(localTable);



			ServerFrame = new Frame(Util.GS("Server Version"));
			bottombox.PackStart(ServerFrame, true, true, 0);

			Table serverTable = new Table(2,4,false);
			serverTable.BorderWidth = 10;
			serverTable.ColumnSpacing = 10;

			ServerNameLabel = new Label(Util.GS("Name:"));
			ServerNameLabel.Xalign = 0;
			serverTable.Attach(ServerNameLabel, 0,1,0,1, 
				Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, 0, 0);

			ServerNameValue = new Label("");
			ServerNameValue.Xalign = 0;
			serverTable.Attach(ServerNameValue, 1,2,0,1);

			ServerDateLabel = new Label(Util.GS("Date:"));
			ServerDateLabel.Xalign = 0;
			serverTable.Attach(ServerDateLabel, 0,1,1,2,
				Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, 0, 0);

			ServerDateValue = new Label("");
			ServerDateValue.Xalign = 0;
			serverTable.Attach(ServerDateValue, 1,2,1,2);

			ServerSizeLabel = new Label(Util.GS("Size:"));
			ServerSizeLabel.Xalign = 0;
			serverTable.Attach(ServerSizeLabel, 0,1,2,3,
				Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, 0, 0);

			ServerSizeValue = new Label("");
			ServerSizeValue.Xalign = 0;
			serverTable.Attach(ServerSizeValue, 1,2,2,3);

			ServerSaveButton = new Button(Stock.Save);
			serverTable.Attach(ServerSaveButton, 0,1,3,4,
				Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, 0, 5);
			ServerSaveButton.Clicked += new EventHandler(SaveServerHandler);

			ServerFrame.Add(serverTable);

			vbox.PackStart(bottombox, false, false, 0);



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
			memberColumn.Title = Util.GS("iFolder Conflicts");
			memberColumn.Resizable = true;
			ConflictTreeView.AppendColumn(memberColumn);
			ConflictTreeView.Selection.Mode = SelectionMode.Multiple;

			ConflictTreeView.Selection.Changed += new EventHandler(
						OnConflictSelectionChanged);

			ConflictPixBuf = 
				new Gdk.Pixbuf(Util.ImagesPath("ifolder.png"));

			this.AddButton(Stock.Close, ResponseType.Ok);
			this.AddButton(Stock.Help, ResponseType.Help);

			RefreshConflictList();
		}




		private void RefreshConflictList()
		{
			try
			{
				Conflict[] conflictList = ifws.GetiFolderConflicts(ifolder.ID);
				foreach(Conflict con in conflictList)
				{
					ConflictTreeStore.AppendValues(con);
				}
			}
			catch(Exception ex)
			{}
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




		private void OnConflictSelectionChanged(object o, EventArgs args)
		{
			TreeSelection tSelect = ConflictTreeView.Selection;
			if(tSelect.CountSelectedRows() > 0)
			{
				TreeModel tModel;
				Conflict con;

				Array treePaths = tSelect.GetSelectedRows(out tModel);

				foreach(TreePath tPath in treePaths)
				{
					TreeIter iter;

					if(ConflictTreeStore.GetIter(out iter, tPath))
					{
						con = (Conflict) tModel.GetValue(iter, 0);
						UpdateFields(con, (tSelect.CountSelectedRows() > 1));
					}
					// we only need the first conflict
					break;
				}
			}
		}



		private void UpdateFields(Conflict con, bool multiSelect)
		{
			if(con == null)
			{
				EnableConflictControls(false);
				LocalNameValue.Text = "";
				LocalDateValue.Text = "";
				LocalSizeValue.Text = "";
			
				ServerNameValue.Text = "";
				ServerDateValue.Text = "";
				ServerSizeValue.Text = "";

				return;
			}
			
			if(!multiSelect)
			{
				EnableConflictControls(true);
				LocalNameValue.Text = con.LocalName;
				LocalDateValue.Text = con.LocalDate;
				LocalSizeValue.Text = con.LocalSize;
			
				ServerNameValue.Text = con.ServerName;
				ServerDateValue.Text = con.ServerDate;
				ServerSizeValue.Text = con.ServerSize;
			}
			else
			{
				EnableConflictControls(true);
				LocalNameValue.Text = Util.GS("Multiple selected");
				LocalDateValue.Text = "";
				LocalSizeValue.Text = "";
			
				ServerNameValue.Text = Util.GS("Multiple selected");
				ServerDateValue.Text = "";
				ServerSizeValue.Text = "";
			}
		}



		private void EnableConflictControls(bool enable)
		{
			ServerFrame.Sensitive = enable;
			LocalFrame.Sensitive = enable;
			LocalNameLabel.Sensitive = enable;
			LocalNameValue.Sensitive = enable;
			LocalDateLabel.Sensitive = enable;
			LocalDateValue.Sensitive = enable;
			LocalSizeLabel.Sensitive = enable;
			LocalSizeValue.Sensitive = enable;
			LocalSaveButton.Sensitive = enable;
			ServerNameLabel.Sensitive = enable;
			ServerNameValue.Sensitive = enable;
			ServerDateLabel.Sensitive = enable;
			ServerDateValue.Sensitive = enable;
			ServerSizeLabel.Sensitive = enable;
			ServerSizeValue.Sensitive = enable;
			ServerSaveButton.Sensitive = enable;
		}


		private void SaveLocalHandler(object o, EventArgs args)
		{
			ResolveSelectedConflicts(true);
		}


		private void SaveServerHandler(object o, EventArgs args)
		{
			ResolveSelectedConflicts(false);
		}

		private void ResolveSelectedConflicts(bool localChangesWin)
		{
			TreeModel tModel;
			Queue   iterQueue;

			iterQueue = new Queue();
			TreeSelection tSelect = ConflictTreeView.Selection;
			Array treePaths = tSelect.GetSelectedRows(out tModel);

			// We can't remove anything while getting the iters
			// because it will change the paths and we'll remove
			// the wrong stuff.
			foreach(TreePath tPath in treePaths)
			{
				TreeIter iter;

				if(tModel.GetIter(out iter, tPath))
				{
					iterQueue.Enqueue(iter);
				}
			}

			// Now that we have all of the TreeIters, loop and
			// remove them all
			while(iterQueue.Count > 0)
			{
				TreeIter iter = (TreeIter) iterQueue.Dequeue();

				Conflict con = (Conflict) tModel.GetValue(iter, 0);

				try
				{
					ifws.ResolveFileConflict(	con.iFolderID, 
												con.ConflictID, 
												localChangesWin);

					ConflictTreeStore.Remove(ref iter);
				}
				catch(Exception ex)
				{}
			}

			UpdateFields(null, false);
		}



	}
}
