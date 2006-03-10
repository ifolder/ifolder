/***********************************************************************
 *  $RCSfile: iFolderPropUnsynchronizedPage.cs,v $
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
 *  Author: Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/

using System;
using Gtk;
using System.Collections;

using Simias.Client;

namespace Novell.iFolder
{

	/// <summary>
	/// This is the properties page for viewing unsynchronized files in an iFolder
	/// </summary>
	public class iFolderPropUnsynchronizedPage : VBox
	{
		#region Class Members

		private iFolderWebService	ifws;
		private SimiasWebService	simws;
		private iFolderWeb			ifolder;

		private iFolderTreeView	UnsynchronizedTreeView;
		private iFolderTreeView	IgnoredTreeView;

		private ListStore			UnsynchronizedListStore;
		private ListStore			IgnoredListStore;
		
		private Gdk.Pixbuf			DirNodePixbuf;
		private Gdk.Pixbuf			FileNodePixbuf;
		private Gdk.Pixbuf			PolicyPixbuf;
		private Gdk.Pixbuf			NodePixbuf;
		
		private Notebook			DetailsNotebook;

		private int				PolicyTypePage;
		private int				PolicyQuotaPage;
		private int				PolicySizePage;
		private int				FileConflictPage;
		private int				NameConflictPage;
		private int				UnknownReasonPage;
		
		// PolicyTypePage Members
		private Button				PolicyTypeIgnoreButton;
		private Button				PolicyTypeMoveButton;
		private Button				PolicyTypeDeleteButton;
		
		// NameConflictPage Members
		private Entry				NameConflictRenameEntry;
		private Button				NameConflictRenameButton;
		
		private Gtk.Window			topLevelWindow;
		
		#endregion

		/// <summary>
		/// Default constructor for iFolderPropSharingPage
		/// </summary>
		public iFolderPropUnsynchronizedPage(	Gtk.Window topWindow,
										iFolderWebService iFolderWS,
										SimiasWebService SimiasWS)
			: base()
		{
			this.ifws = iFolderWS;
			this.simws = SimiasWS;
			this.topLevelWindow = topWindow;

			InitializeWidgets();
		}




		public void UpdateiFolder(iFolderWeb ifolder)
		{
			this.ifolder = ifolder;

			RefreshList();
			UpdateWidgets();
		}




		/// <summary>
		/// Set up the UI inside the Window
		/// </summary>
		private void InitializeWidgets()
		{
			this.Spacing = 10;
			this.BorderWidth = 10;

			DirNodePixbuf = Util.LoadIcon("gnome-fs-directory", 16);
			FileNodePixbuf = Util.LoadIcon("gnome-mime-text", 16);
			// FIXME: Replace PolicyPixbuf and NodePixbuf with icons that make sense
			PolicyPixbuf = Util.LoadIcon("gnome-mime-text", 16);
			NodePixbuf = Util.LoadIcon("gnome-mime-text", 16);
			
//			HBox hbox = new HBox(false, 10);
//			this.PackStart(hbox, true, true, 0);
			HPaned splitter = new HPaned();
			this.PackStart(splitter, true, true, 0);
			
			VBox vbox = new VBox(false, 5);
//			hbox.PackStart(vbox, false, false, 0);
			splitter.Pack1(vbox, true, true);
			
			Label l = new Label(Util.GS("_Unsynchronized Files:"));
			vbox.PackStart(l, false, false, 0);
			
			UnsynchronizedTreeView = new iFolderTreeView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			sw.Add(UnsynchronizedTreeView);
			vbox.PackStart(sw, true, true, 0);
			l.MnemonicWidget = UnsynchronizedTreeView;

			UnsynchronizedListStore = new ListStore(typeof(UnsynchronizedNode));
			UnsynchronizedTreeView.Model = UnsynchronizedListStore;
			UnsynchronizedTreeView.HeadersVisible = false;

			CellRendererPixbuf mcrp = new CellRendererPixbuf();
			TreeViewColumn NameColumn = new TreeViewColumn();
			NameColumn.PackStart(mcrp, false);
			NameColumn.Spacing = 2;
			NameColumn.SetCellDataFunc(mcrp,
					new TreeCellDataFunc(NameCellPixbufDataFunc));

			CellRendererText mcrt = new CellRendererText();
			NameColumn.PackStart(mcrt, false);
			NameColumn.SetCellDataFunc(mcrt,
					new TreeCellDataFunc(NameCellTextDataFunc));
//			NameColumn.Title = Util.GS("Name");
			UnsynchronizedTreeView.AppendColumn(NameColumn);
			NameColumn.Resizable = true;

/*
			CellRendererText nodeTypeCR = new CellRendererText();
			nodeTypeCR.Xpad = 5;
			TreeViewColumn nodeTypeColumn = 
			UnsynchronizedTreeView.AppendColumn(Util.GS("NodeType"),
					nodeTypeCR,
					new TreeCellDataFunc(NodeTypeCellTextDataFunc));
			nodeTypeColumn.Resizable = true;
			nodeTypeColumn.MinWidth = 150;

			CellRendererText syncStatusCR = new CellRendererText();
			syncStatusCR.Xpad = 5;
			TreeViewColumn syncStatusColumn = 
			UnsynchronizedTreeView.AppendColumn(Util.GS("Access"),
					syncStatusCR,
					new TreeCellDataFunc(SyncStatusCellTextDataFunc));
			syncStatusColumn.Resizable = true;
*/

			UnsynchronizedTreeView.Selection.Mode = SelectionMode.Single;
			UnsynchronizedTreeView.Selection.Changed +=
				new EventHandler(OnUnsynchronizedTreeViewSelectionChanged);

//			UnsynchronizedTreeView.ButtonPressEvent += new ButtonPressEventHandler(
//						OnUnsynchronizedTreeViewButtonPressed);
			
//			UnsynchronizedTreeView.RowActivated += new RowActivatedHandler(
//						OnUnsynchronizedTreeViewRowActivated);

			// FIXME: Set up the different types of Pixbufs
			
			
			DetailsNotebook = new Notebook();
//			hbox.PackStart(DetailsNotebook, true, true, 0);
			splitter.Pack2(DetailsNotebook, true, false);
			DetailsNotebook.ShowBorder = false;
			DetailsNotebook.ShowTabs = false;
			DetailsNotebook.Sensitive = false;
			
			PolicyTypePage = DetailsNotebook.AppendPage(CreatePolicyTypePage(), new Label("PolicyType"));	// NOTE: The names of these pages are only here for debug purposes.  They will not be shown in the released code and should NOT be localized/translated.
			PolicyQuotaPage = DetailsNotebook.AppendPage(CreatePolicyQuotaPage(), new Label("PolicyQuota"));
			PolicySizePage = DetailsNotebook.AppendPage(CreatePolicySizePage(), new Label("PolicySize"));
			FileConflictPage = DetailsNotebook.AppendPage(CreateFileConflictPage(), new Label("FileConflict"));
			NameConflictPage = DetailsNotebook.AppendPage(CreateNameConflictPage(), new Label("NameConflict"));
			UnknownReasonPage = DetailsNotebook.AppendPage(CreateUnknownReasonPage(), new Label("Unknown"));
		}
		
		private Widget CreatePolicyTypePage()
		{
			Frame f = new Frame(Util.GS("Invalid file type"));

			VBox vbox = new VBox(false, 0);
			f.Add(vbox);
			vbox.BorderWidth = 10;
			
			Label l = new Label(Util.GS("A policy exists on this iFolder which prevents this type of file from synchronizing to the server.\n\nPlease move, delete, or ignore this file."));
			vbox.PackStart(l, false, false, 0);
			l.LineWrap = true;
			l.Xalign = 0;
			
			// Spacer
			l = new Label("");
			vbox.PackStart(l, true, true, 0);
			
			HBox hbox = new HBox(false, 10);
			vbox.PackStart(hbox, false, false, 5);

			// Spacer
			l = new Label("");
			hbox.PackStart(l, true, true, 0);
			
			//
			// Move Button
			//
			HBox buttonHBox = new HBox(false, 5);
			Image image = new Image(Stock.Save, IconSize.Button);
			buttonHBox.PackStart(image, false, false, 0);
			l = new Label(Util.GS("_Move"));
			buttonHBox.PackStart(l, true, true, 0);
			
			PolicyTypeMoveButton = new Button(buttonHBox);
			hbox.PackStart(PolicyTypeMoveButton, false, false, 0);
			
			//
			// Delete Button
			//			
			PolicyTypeDeleteButton = new Button(Stock.Delete);
			hbox.PackStart(PolicyTypeDeleteButton, false, false, 0);

			//
			// Ignore Button
			//
			buttonHBox = new HBox(false, 5);
			image = new Image(Stock.Cancel, IconSize.Button);
			buttonHBox.PackStart(image, false, false, 0);
			l = new Label(Util.GS("_Ignore"));
			buttonHBox.PackStart(l, true, true, 0);
			
			PolicyTypeIgnoreButton = new Button(buttonHBox);
			hbox.PackStart(PolicyTypeIgnoreButton, false, false, 0);

			return f;
		}
		
		private Widget CreatePolicyQuotaPage()
		{
			Frame f = new Frame(Util.GS("PolicyQuota"));
			return f;
		}
		
		private Widget CreatePolicySizePage()
		{
			Frame f = new Frame(Util.GS("File exceeded size limits"));
			return f;
		}
		
		private Widget CreateFileConflictPage()
		{
			Frame f = new Frame(Util.GS("File Conflict"));
			return f;
		}
		
		private Widget CreateNameConflictPage()
		{
			Frame f = new Frame(Util.GS("Invalid File Name"));

			VBox vbox = new VBox(false, 0);
			f.Add(vbox);
			vbox.BorderWidth = 10;
			
			Label l = new Label(
					Util.GS("The name of this file contains invalid characters.\n\nThe following characters cannot be used (some operating systems are not able to use files with these characters in the name):") +
					"\n\n" +
					"\t\\\t" + Util.GS("Backslash") + "\n" +
					"\t:\t" + Util.GS("Colon") + "\n" +
					"\t*\t" + Util.GS("Asterisk") + "\n" +
					"\t?\t" + Util.GS("Question mark") + "\n" +
					"\t\"\t" + Util.GS("Double quotation mark") + "\n" +
					"\t<\t" + Util.GS("Less-than sign") + "\n" +
					"\t>\t" + Util.GS("Greater-than sign") + "\n" +
					"\t|\t" + Util.GS("Vertical line"));
			vbox.PackStart(l, false, false, 0);
			l.LineWrap = true;
			l.Xalign = 0;
			
			// Spacer
			l = new Label("");
			vbox.PackStart(l, true, true, 0);
			
			l = new Label(Util.GS("Re_name to:"));
			vbox.PackStart(l, false, false, 0);
			l.Xalign = 0;
			
			HBox hbox = new HBox(false, 10);
			vbox.PackStart(hbox, false, false, 5);
			
			NameConflictRenameEntry = new Entry();
			hbox.PackStart(NameConflictRenameEntry, true, true, 0);
			l.MnemonicWidget = NameConflictRenameEntry;
			
			HBox buttonHBox = new HBox(false, 5);
			Image image = new Image(Stock.Save, IconSize.Button);
			buttonHBox.PackStart(image, false, false, 0);
			l = new Label(Util.GS("_Rename"));
			buttonHBox.PackStart(l, true, true, 0);
			
			NameConflictRenameButton = new Button(buttonHBox);
			hbox.PackStart(NameConflictRenameButton, false, false, 0);
			NameConflictRenameButton.Sensitive = false;

//			NameConflictRenameButton.Clicked +=
//				new EventHandler(OnNameConflictRenameButtonClicked);
			
			return f;
		}
		
		private Widget CreateUnknownReasonPage()
		{
			Frame f = new Frame(Util.GS("Uknown Reason"));
			Label l = new Label("FIXME: I need to find out from Russ to know if we don't really know why something didn't synchronize if we can just let a user delete a node.");
			f.Add(l);
			l.LineWrap = true;
			return f;
		}
		
		private void RefreshList()
		{
			UnsynchronizedListStore.Clear();
			
			UnsynchronizedNode[] nodes = ifws.GetUnsynchronizedNodes(ifolder.ID);
			foreach(UnsynchronizedNode node in nodes)
			{
				TreeIter iter = UnsynchronizedListStore.AppendValues(node);
			}
		}
		


		private void UpdateWidgets()
		{
//			OnUserSelectionChanged(null, null);
		}




		private void NameCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			UnsynchronizedNode node = (UnsynchronizedNode) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = node.Name;
		}




		private void NameCellPixbufDataFunc(Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Gdk.Pixbuf pixbuf = null;

			UnsynchronizedNode node = (UnsynchronizedNode) tree_model.GetValue(iter, 0);
			switch (node.Type)
			{
				case "DirNode":
					pixbuf = DirNodePixbuf;
					break;
				case "FileNode":
					pixbuf = FileNodePixbuf;
					break;
				case "Policy":
					pixbuf = PolicyPixbuf;
					break;
				case "Node":
				default:
					pixbuf = NodePixbuf;
					break;
			}

			((CellRendererPixbuf) cell).Pixbuf = pixbuf;
		}




		private void NodeTypeCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			UnsynchronizedNode node = (UnsynchronizedNode) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = node.Type;
		}




		private void SyncStatusCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			UnsynchronizedNode node = (UnsynchronizedNode) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = node.SyncStatus;
		}



		private void OnUnsynchronizedTreeViewSelectionChanged(object o, EventArgs args)
		{
			TreeSelection tSelect = UnsynchronizedTreeView.Selection;
			if (tSelect.CountSelectedRows() < 1)
			{
				DetailsNotebook.Sensitive = false;
			}
			else
			{
				DetailsNotebook.Sensitive = true;
				
				// Determine the correct notebook page to show
				TreeIter iter;
				if (tSelect.GetSelected(out iter))
				{
					UnsynchronizedNode node = (UnsynchronizedNode)
						UnsynchronizedListStore.GetValue(iter, 0);
					if (node != null)
						SelectPageBySelection(node);
				}
			}
		}
		
		private void SelectPageBySelection(UnsynchronizedNode node)
		{
			switch (node.SyncStatus)
			{
				case "PolicyType":
					DetailsNotebook.CurrentPage = PolicyTypePage;
					break;
				case "PolicyQuota":
					DetailsNotebook.CurrentPage = PolicyQuotaPage;
					break;
				case "PolicySize":
					DetailsNotebook.CurrentPage = PolicySizePage;
					break;
				case "Collision":
					// FIXME: Retrieve the collision and determine what kind of
					// conflict is it to know which page to switch to
//					DetailsNotebook.CurrentPage = FileConflictPage;
					DetailsNotebook.CurrentPage = NameConflictPage;
					NameConflictRenameEntry.Text= node.Name;
					break;
				default:
					DetailsNotebook.CurrentPage = UnknownReasonPage;
					break;
			}
		}



/*
		public void OnUnsynchronizedTreeViewButtonPressed(object obj, 
											ButtonPressEventArgs args)
		{
			switch(args.Event.Button)
			{
				case 1: // first mouse button
					break;
				case 2: // second mouse button
					break;
				case 3: // third mouse button
				{
					TreePath tPath = null;
					TreeViewColumn tColumn = null;

					if(UnsynchronizedTreeView.GetPathAtPos(	(int)args.Event.X,
													(int)args.Event.Y,
													out tPath,
													out tColumn) == true)
					{
						TreeSelection tSelect = UnsynchronizedTreeView.Selection;

						if (tSelect.CountSelectedRows() > 0)
						{
							Menu rightsMenu = new Menu();
			
							RadioMenuItem adminItem = 
								new RadioMenuItem (Util.GS("Full Control"));
							rightsMenu.Append(adminItem);

							RadioMenuItem rwItem = 
								new RadioMenuItem (adminItem.Group, 
													Util.GS("Read/Write"));
							rightsMenu.Append(rwItem);

							RadioMenuItem roItem = 
								new RadioMenuItem (adminItem.Group, 
													Util.GS("Read Only"));
							rightsMenu.Append(roItem);

							if (ifolder.CurrentUserRights != "Admin"
								|| SelectionHasOwnerOrCurrent())
							{
								adminItem.Sensitive = false;
								rwItem.Sensitive = false;
								roItem.Sensitive = false;
							}

							// Get the Value of the actual user selected
							TreeIter iter;
		
							if(UserTreeStore.GetIter(out iter, tPath))
							{
								iFolderUser user = (iFolderUser) 
										UserTreeStore.GetValue(iter, 0);
								if(user.Rights == "ReadWrite")
									rwItem.Active = true;
								else if(user.Rights == "Admin")
									adminItem.Active = true;
								else
									roItem.Active = true;
							}

							adminItem.Activated += new EventHandler(
									OnAdminRightsMenu);
							rwItem.Activated += new EventHandler(
									OnRWRightsMenu);
							roItem.Activated += new EventHandler(
									OnRORightsMenu);

							rightsMenu.ShowAll();

							rightsMenu.Popup(null, null, null, 
								IntPtr.Zero, 3, 
								Gtk.Global.CurrentEventTime);
						}
					}
					break;
				}
			}
		}
		
		private void OnUnsynchronizedTreeViewRowActivated(object o, RowActivatedArgs args)
		{
			TreeSelection tSelect = UnsynchronizedTreeView.Selection;
			if (tSelect.CountSelectedRows() == 1)
			{
				if (ifolder.CurrentUserRights == "Admin"
					&& !SelectionHasOwnerOrCurrent())
				{
					OnAccessClicked(null, null);
				}
			}
		}
*/
	}
}
