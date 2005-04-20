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
	/// This is a holder class for iFolders so the client can place
	/// extra data with an iFolder about it's status and such.
	/// </summary>
	public class ConflictHolder
	{
		private Conflict	localConflict = null;
		private Conflict	serverConflict = null;
		private string		ifPath = null;
		private bool		isNameConflict;

		public ConflictHolder(Conflict con, string iFolderPath)
		{
			ifPath = (iFolderPath == null) ? "iFolderPathUnknown" : iFolderPath;
		
			if(!con.IsNameConflict)
			{
				this.isNameConflict = false;
				localConflict = con;
			}
			else
			{
				this.isNameConflict = true;
				if( (con.LocalName != null) &&
					(con.LocalName.Length > 0) )
				{
//					Console.WriteLine("Creating with Local Conflict");
					localConflict = con;
				}
				else
				{
//					Console.WriteLine("Creating with Server Conflict");
					serverConflict = con;
				}

			}
		}

		public bool IsNameConflict
		{
			get{ return isNameConflict; }
		}

		public Conflict FileConflict
		{
			get
			{
				if(!isNameConflict)
					return localConflict;
				else
					return null;
			}

			set
			{ 
				if(!value.IsNameConflict)
					this.localConflict = value; 
				else
					throw new Exception("NameConflict cannot be set there");
			}
		}

		public Conflict LocalNameConflict
		{
			get
			{
				if(isNameConflict)
					return localConflict;
				else
					return null;
			}
		}

		public Conflict ServerNameConflict
		{
			get
			{
				if(isNameConflict)
					return serverConflict;
				else
					return null;
			}
		}
		
		public string Name
		{
			get
			{
				if(!isNameConflict)
					return localConflict.LocalName;
				else
				{
					if(localConflict != null)
						return localConflict.LocalName;
					else
						return serverConflict.ServerName;
				}
			}
		}
		
		public string RelativePath
		{
			get
			{
				if(!isNameConflict)
					return ParseRelativePath(localConflict.LocalFullPath);
				else
				{
					if(localConflict != null)
						return ParseRelativePath(localConflict.LocalFullPath);
					else
						return ParseRelativePath(serverConflict.ServerFullPath);
				}
			}
		}
		
		public string Type
		{
			get
			{
				if(isNameConflict)
					return Util.GS("Name");
				else
					return Util.GS("File");
			}
		}


		public void AddNameConflict(Conflict con)
		{
//			Console.WriteLine("Adding a new conflict");
			if(!con.IsNameConflict)
				throw new Exception("Cannot add a FileConflict");

			if(	(con.LocalName != null) &&
				(con.LocalName.Length > 0) &&
				(localConflict == null) )
			{
//				Console.WriteLine("Adding a local Conflict");
				localConflict = con;
			}
			else if(serverConflict == null)
			{
//				Console.WriteLine("Adding a server Conflict");
				serverConflict = con;
			}
			else
				throw new Exception("Can't add additional conflicts");
		}

		/// <summary>
		/// This function takes a full file path and makes it relative to the
		/// location of the iFolder so that it doesn't take up as much room
		/// in the TreeView.
		///
		/// For example, if "file.txt" exists at the "root" of the iFolder,
		/// the relative path should be "/".  Also, if "anotherfile.txt" is
		/// inside the "testing" directory, the relative path would be
		/// "/testing/".
		/// </summary>
		private string ParseRelativePath(string fullPath)
		{
			if (fullPath == null) return "";
			
			if (fullPath.StartsWith(ifPath))
			{
				if (fullPath.Length > ifPath.Length)
				{
					string tmpPath = fullPath.Substring(ifPath.Length);
					
					// Now we have to take off the name off the file name
					int lastSlashPos = tmpPath.LastIndexOf('/');
					if (lastSlashPos <= 0)
						return "";
					
					return tmpPath.Substring(1, lastSlashPos);
				}
			}

			return "";
		}
	}






	/// <summary>
	/// This is the conflict resolver for iFolder
	/// </summary>
	public class iFolderConflictDialog : Dialog
	{
		private iFolderWebService	ifws;
		private iFolderWeb			ifolder;
		private Gtk.TreeView		ConflictTreeView;
		private Gtk.ListStore		ConflictTreeStore;
//		private Gdk.Pixbuf			ConflictPixBuf;


		//
		// File Conflict Box
		//
		private HBox				fileConflictBox;
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

		//
		// Name Conflict Box
		//
		private VBox				nameConflictBox;
		private Frame				renameFileFrame;
		private Label				nameConflictSummary;
		private Label				nameConflictFileNameLabel;
		private Entry				nameConflictEntry;
		private Button				nameEntrySaveButton;

		private Hashtable			conflictTable;

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
			conflictTable = new Hashtable();

			InitializeWidgets();
			EnableConflictControls(false);
			this.Realized += new EventHandler(OnRealizeWidget);
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void InitializeWidgets()
		{
			this.SetDefaultSize (600, 480);
			this.Icon = 
				new Gdk.Pixbuf(Util.ImagesPath("conflict24.png"));
			VBox vbox = new VBox();
			vbox.Spacing = 10;
			vbox.BorderWidth = 10;
			this.VBox.PackStart(vbox, true, true, 0);

			HBox topbox = new HBox();
			topbox.Spacing = 10;

			Gdk.Pixbuf bigConflict =
				new Gdk.Pixbuf(Util.ImagesPath("conflict32.png"));
			Image conflictImage = new Image(bigConflict);
			conflictImage.Yalign = 0;
			topbox.PackStart(conflictImage, false, false, 0);

			VBox textbox = new VBox();
			textbox.Spacing = 10;

			Label l = new Label("<span weight=\"bold\" size=\"larger\">" +
								Util.GS("This iFolder contains conflicts") +
								"</span>");
			l.LineWrap = false;
			l.UseMarkup = true;
			l.Selectable = false;
			l.Xalign = 0;
			l.Yalign = 0;
			textbox.PackStart(l, true, true, 0);

			Table ifTable = new Table(2,2,false);
			ifTable.ColumnSpacing = 10;
			ifTable.Homogeneous = false;

			Label nameLabel = new Label(Util.GS("Name:"));
			nameLabel.Xalign = 1;
			ifTable.Attach(nameLabel, 0,1,0,1,
				Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, 0, 0);

			Label nameValue = new Label(ifolder.Name);
			nameValue.Xalign = 0;
			ifTable.Attach(nameValue, 1,2,0,1);

			Label pathLabel = new Label(Util.GS("Path:"));
			pathLabel.Xalign = 1;
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

			//
			// File Conflict Box
			//
			fileConflictBox = new HBox();
			fileConflictBox.Spacing = 10;

			LocalFrame = new Frame(Util.GS("Local Version"));
			fileConflictBox.PackStart(LocalFrame, true, true, 0);

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
			fileConflictBox.PackStart(ServerFrame, true, true, 0);

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

			vbox.PackStart(fileConflictBox, false, false, 0);


			//
			// Name Conflict Box
			//
			nameConflictBox = new VBox();
			nameConflictBox.Spacing = 10;
			
			VBox nameConflictInternalBox = new VBox();
			nameConflictInternalBox.Spacing = 10;
			nameConflictInternalBox.BorderWidth = 10;

			renameFileFrame = new Frame(Util.GS("Rename File"));
			nameConflictBox.PackStart(renameFileFrame, true, true, 0);

			nameConflictSummary = new Label(Util.GS("Enter a new name for this file and click save to resolve it."));
			nameConflictSummary.Xalign = 0;
//			nameConflictSummary.ColumnSpacing = 10;
			
			nameConflictInternalBox.PackStart(nameConflictSummary, false, false, 0);

			HBox nameConflictHBox = new HBox();
			nameConflictHBox.Spacing = 10;
			
			nameConflictFileNameLabel = new Label(Util.GS("File Name:"));
			nameConflictHBox.PackStart(nameConflictFileNameLabel, false, false, 0);
			
			nameConflictEntry = new Entry();
			nameConflictEntry.CanFocus = true;
			nameConflictEntry.Changed += new EventHandler(OnNameEntryChanged);
			nameConflictEntry.ActivatesDefault = true;
			nameConflictHBox.PackStart(nameConflictEntry, true, true, 0);

			nameConflictInternalBox.PackStart(nameConflictHBox, false, false, 0);
			
			HBox saveButtonBox = new HBox();
			
			nameEntrySaveButton = new Button(Stock.Save);
			nameEntrySaveButton.Clicked += new EventHandler(RenameFileHandler);
			saveButtonBox.PackStart(nameEntrySaveButton, false, false, 0);
			nameConflictInternalBox.PackEnd(saveButtonBox, false, false, 0);
			
			renameFileFrame.Add(nameConflictInternalBox);

			vbox.PackStart(nameConflictBox, false, false, 0);

			nameConflictBox.Visible = false;
			
			// Setup the iFolder TreeView
			ConflictTreeStore = new ListStore(typeof(ConflictHolder));
			ConflictTreeView.Model = ConflictTreeStore;
			
			// File Name Column
			TreeViewColumn fileNameColumn = new TreeViewColumn();
			fileNameColumn.Title = Util.GS("File Name");
			CellRendererText fileNameCR = new CellRendererText();
			fileNameCR.Xpad = 5;
			fileNameColumn.PackStart(fileNameCR, false);
			fileNameColumn.SetCellDataFunc(fileNameCR,
										   new TreeCellDataFunc(FileNameCellTextDataFunc));
			fileNameColumn.Resizable = true;
			fileNameColumn.MinWidth = 150;
			ConflictTreeView.AppendColumn(fileNameColumn);
			
			// Path Column
			TreeViewColumn pathColumn = new TreeViewColumn();
			pathColumn.Title = Util.GS("Path in iFolder");
			CellRendererText pathCR = new CellRendererText();
			pathCR.Xpad = 5;
			pathColumn.PackStart(pathCR, false);
			pathColumn.SetCellDataFunc(pathCR,
										   new TreeCellDataFunc(PathCellTextDataFunc));
			pathColumn.Resizable = true;
			pathColumn.MinWidth = 300;
			pathColumn.Sizing = TreeViewColumnSizing.Autosize;
			ConflictTreeView.AppendColumn(pathColumn);
			
			// Conflict Type Column
			TreeViewColumn conflictTypeColumn = new TreeViewColumn();
			conflictTypeColumn.Title = Util.GS("Conflict Type");
			CellRendererText conflictTypeCR = new CellRendererText();
			conflictTypeCR.Xpad = 5;
			conflictTypeColumn.PackStart(conflictTypeCR, false);
			conflictTypeColumn.SetCellDataFunc(conflictTypeCR,
										   new TreeCellDataFunc(ConflictTypeCellTextDataFunc));
			conflictTypeColumn.Resizable = false;
			conflictTypeColumn.FixedWidth = 100;
			ConflictTreeView.AppendColumn(conflictTypeColumn);
			
			
			

			// Setup Pixbuf and Text Rendering for "iFolder Conflicts" column
//			CellRendererPixbuf mcrp = new CellRendererPixbuf();
//			TreeViewColumn memberColumn = new TreeViewColumn();
//			memberColumn.PackStart(mcrp, false);
//			memberColumn.SetCellDataFunc(mcrp, new TreeCellDataFunc(
//						ConflictCellPixbufDataFunc));
//			CellRendererText mcrt = new CellRendererText();
//			memberColumn.PackStart(mcrt, false);
//			memberColumn.SetCellDataFunc(mcrt, new TreeCellDataFunc(
//						ConflictCellTextDataFunc));
//			memberColumn.Title = Util.GS("iFolder Conflicts");
//			memberColumn.Resizable = true;
//			ConflictTreeView.AppendColumn(memberColumn);
			ConflictTreeView.Selection.Mode = SelectionMode.Multiple;

			ConflictTreeView.Selection.Changed += new EventHandler(
						OnConflictSelectionChanged);

//			ConflictPixBuf = 
//				new Gdk.Pixbuf(Util.ImagesPath("conflict24.png"));

			this.AddButton(Stock.Close, ResponseType.Ok);
			this.AddButton(Stock.Help, ResponseType.Help);

			RefreshConflictList();
		}

		private void OnRealizeWidget(object o, EventArgs args)
		{
			// Select the first item in the TreeView
			if (ConflictTreeView.Selection != null)
			{
				ConflictTreeView.Selection.SelectPath(new TreePath("0"));
			}

			// If the user is a read-only member of this iFolder, let them know
			// that the	only way they can resolve conflicts is by saving the
			// server version.
			if (ifolder.CurrentUserRights == "ReadOnly")
			{
				iFolderMsgDialog dg = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Warning,
					iFolderMsgDialog.ButtonSet.Ok,
					Util.GS("Read Only Membership"),
					Util.GS("Read Only Membership"),
					Util.GS("Your ability to resolve conflicts is limited because you have read-only rights to this iFolder.  Name conflicts must be renamed locally.  File conflicts will be overwritten with by the version of the file on the server."));
				dg.Run();
				dg.Hide();
				dg.Destroy();
			}
		}

		private void RefreshConflictList()
		{
			conflictTable.Clear();

			try
			{
				Conflict[] conflictList = ifws.GetiFolderConflicts(ifolder.ID);
				foreach(Conflict con in conflictList)
				{
					if(!con.IsNameConflict)
					{
						ConflictHolder ch = new ConflictHolder(con, ifolder.UnManagedPath);
						ConflictTreeStore.AppendValues(ch);
					}
					else
					{
						String key = "";
						if( (con.LocalFullPath != null) &&
							(con.LocalFullPath.Length > 0) )
							key = con.LocalFullPath;
						else
							key = con.ServerFullPath;

//						Console.WriteLine("Key = {0}", key);

						if(conflictTable.ContainsKey(key))
						{
//							Console.WriteLine("Found key, adding to holder");
							ConflictHolder ch = 
								(ConflictHolder)conflictTable[key];
							ch.AddNameConflict(con);
						}
						else
						{
//							Console.WriteLine("No key, New holder");
							ConflictHolder ch = new ConflictHolder(con, ifolder.UnManagedPath);
							ConflictTreeStore.AppendValues(ch);
							conflictTable.Add(key, ch);
						}
					}
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}
		}




//		private void ConflictCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
//				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
//				Gtk.TreeIter iter)
//		{
//			((CellRendererPixbuf) cell).Pixbuf = ConflictPixBuf;
//		}

		private void FileNameCellTextDataFunc(Gtk.TreeViewColumn col,
											  Gtk.CellRenderer cellRenderer,
											  Gtk.TreeModel model,
											  Gtk.TreeIter iter)
		{
			ConflictHolder conflictHolder = (ConflictHolder) model.GetValue(iter,0);
			((CellRendererText) cellRenderer).Text = conflictHolder.Name;
		}

		private void PathCellTextDataFunc(Gtk.TreeViewColumn col,
											  Gtk.CellRenderer cellRenderer,
											  Gtk.TreeModel model,
											  Gtk.TreeIter iter)
		{
			ConflictHolder conflictHolder = (ConflictHolder) model.GetValue(iter,0);
			((CellRendererText) cellRenderer).Text = conflictHolder.RelativePath;
		}

		private void ConflictTypeCellTextDataFunc(Gtk.TreeViewColumn col,
											  Gtk.CellRenderer cellRenderer,
											  Gtk.TreeModel model,
											  Gtk.TreeIter iter)
		{
			ConflictHolder conflictHolder = (ConflictHolder) model.GetValue(iter,0);
			((CellRendererText) cellRenderer).Text = conflictHolder.Type;
		}



		private void OnConflictSelectionChanged(object o, EventArgs args)
		{
			bool bHasNameConflict = false;
			bool bHasFileConflict = false;

			TreeSelection tSelect = ConflictTreeView.Selection;
			int selectedRows = tSelect.CountSelectedRows();
			if(selectedRows > 0)
			{
				EnableConflictControls(true);
				TreeModel tModel;
				ConflictHolder ch = null;

				Array treePaths = tSelect.GetSelectedRows(out tModel);

				foreach(TreePath tPath in treePaths)
				{
					TreeIter iter;
					if(ConflictTreeStore.GetIter(out iter, tPath))
					{
						ch = (ConflictHolder) tModel.GetValue(iter, 0);
						if (ch.IsNameConflict)
							bHasNameConflict = true;
						else
							bHasFileConflict = true;
					}
				}
				
				if (selectedRows == 1)
				{
					if (bHasNameConflict)
					{
						nameConflictSummary.Text = Util.GS("Enter a new name for this file and click save to resolve it.");

						// This is a name conflict
						nameConflictBox.Visible = true;
						fileConflictBox.Visible = false;

						// Prefill the entry with the filename and auto-select
						// the text on the left-hand side of the extension.
						nameConflictEntry.Text = ch.Name;
						
/* FIXME: Get GrabFocus() and preselection of filename working
						nameConflictEntry.GrabFocus();

						if (ch.Name.Length > 0)
						{
							int lastDotPos = ch.Name.LastIndexOf('.');
							if (lastDotPos > 1)
								nameConflictEntry.SelectRegion(0, lastDotPos);
							else
								nameConflictEntry.SelectRegion(0, ch.Name.Length);
						}
*/
					}
					else
					{
						// This is a file conflict
						fileConflictBox.Visible = true;
						nameConflictBox.Visible = false;
					}

					UpdateFields(ch, false);
				}
				else
				{
					// We're dealing with multiple selections here
					if (bHasFileConflict)
					{
						// Allow name conflicts to be multi-selected with file conflicts
						fileConflictBox.Visible = true;
						nameConflictBox.Visible = false;
						UpdateFields(ch, true);
					}
					else
					{
						// There are multiple name conflicts selected
						nameConflictBox.Visible = true;
						fileConflictBox.Visible = false;

						nameConflictSummary.Text = Util.GS("Name conflicts must be resolved individually.");
						nameConflictEntry.Text = "";
						EnableConflictControls(false);
					}
				}
			}
		}



		private void UpdateFields(ConflictHolder ch, bool multiSelect)
		{
			if(ch == null)
			{
				EnableConflictControls(false);
				LocalNameValue.Text = "";
				LocalDateValue.Text = "";
				LocalSizeValue.Text = "";
			
				ServerNameValue.Text = "";
				ServerDateValue.Text = "";
				ServerSizeValue.Text = "";
				
				nameConflictEntry.Text = "";

				return;
			}

			if(multiSelect)
			{
				EnableConflictControls(true);
				LocalNameValue.Text = Util.GS("Multiple selected");
				LocalDateValue.Text = "";
				LocalSizeValue.Text = "";
			
				ServerNameValue.Text = Util.GS("Multiple selected");
				ServerDateValue.Text = "";
				ServerSizeValue.Text = "";
				
				if (ifolder.CurrentUserRights == "ReadOnly")
				{
					LocalSaveButton.Sensitive = false;
				}
				return;
			}
			
			if(!ch.IsNameConflict)
			{
				EnableConflictControls(true);
				LocalNameValue.Text = ch.FileConflict.LocalName;
				LocalDateValue.Text = ch.FileConflict.LocalDate;
				LocalSizeValue.Text = ch.FileConflict.LocalSize;
			
				ServerNameValue.Text = ch.FileConflict.ServerName;
				ServerDateValue.Text = ch.FileConflict.ServerDate;
				ServerSizeValue.Text = ch.FileConflict.ServerSize;

				if (ifolder.CurrentUserRights == "ReadOnly")
				{
					LocalSaveButton.Sensitive = false;
				}
				return;
			}

			EnableConflictControls(true);

//			if(ch.LocalNameConflict != null)
//			{
//				LocalNameValue.Text = ch.LocalNameConflict.LocalName;
//				LocalDateValue.Text = ch.LocalNameConflict.LocalDate;
//				LocalSizeValue.Text = ch.LocalNameConflict.LocalSize;
//			}
//			else
//			{
//				LocalNameValue.Text = "";
//				LocalDateValue.Text = "";
//				LocalSizeValue.Text = "";
//				LocalSaveButton.Sensitive = false;
//			}
//
//			if(ch.ServerNameConflict != null)
//			{
//				ServerNameValue.Text = ch.ServerNameConflict.ServerName;
//				ServerDateValue.Text = ch.ServerNameConflict.ServerDate;
//				ServerSizeValue.Text = ch.ServerNameConflict.ServerSize;
//			}
//			else
//			{
//				ServerNameValue.Text = "";
//				ServerDateValue.Text = "";
//				ServerSizeValue.Text = "";
//				ServerSaveButton.Sensitive = false;
//			}
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

			nameConflictSummary.Sensitive = enable;
			nameConflictFileNameLabel.Sensitive = enable;
			nameConflictEntry.Sensitive = enable;
			if (nameConflictEntry.Text.Length > 0)
				nameEntrySaveButton.Sensitive = enable;
			else
				nameEntrySaveButton.Sensitive = false;
		}


		private void SaveLocalHandler(object o, EventArgs args)
		{
			ResolveSelectedConflicts(true);
		}


		private void SaveServerHandler(object o, EventArgs args)
		{
			ResolveSelectedConflicts(false);
		}

		private void RenameFileHandler(object o, EventArgs args)
		{
			string newFileName = nameConflictEntry.Text;

			TreeModel tModel;
			ConflictHolder ch = null;
			
			TreeSelection tSelect = ConflictTreeView.Selection;
			if (tSelect.CountSelectedRows() == 1)
			{
				Array treePaths = tSelect.GetSelectedRows(out tModel);
				TreeIter iter;
				if (ConflictTreeStore.GetIter(out iter, (TreePath)treePaths.GetValue(0)))
				{
					ch = (ConflictHolder) tModel.GetValue(iter, 0);
					Conflict lnc = ch.LocalNameConflict;
					Conflict snc = ch.ServerNameConflict;
					
					try
					{
						if (ifolder.CurrentUserRights == "ReadOnly")
						{
							ifws.RenameAndResolveConflict(snc.iFolderID,
														  snc.ConflictID,
														  newFileName);
						}
						else
						{
							ifws.ResolveNameConflict(lnc.iFolderID,
													 lnc.ConflictID,
													 newFileName);
							// If this is a name conflict because of case-sensitivity
							// on Linux, there won't be a conflict on the server.
							if (snc != null)
							{
								ifws.ResolveNameConflict(snc.iFolderID,
														 snc.ConflictID,
														 ch.Name);
							}
						}

						ConflictTreeStore.Remove(ref iter);
					}
					catch (Exception e)
					{
						iFolderExceptionDialog ied =
							new iFolderExceptionDialog(null, e);
						ied.Run();
						ied.Hide();
						ied.Destroy();
						ied = null;
						return;
					}
					
					UpdateFields(null, false);
				}
			}
		}

		/// <summary>
		/// Disable the Save button if there's no text in the entry or
		/// enable it if the entry length is greater than 0.
		/// </summary>
		private void OnNameEntryChanged(object o, EventArgs args)
		{
			if (nameConflictEntry.Text.Length > 0)
				nameEntrySaveButton.Sensitive = true;
			else
				nameEntrySaveButton.Sensitive = false;
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

			if(iterQueue.Count > 0)
			{
				// Now that we have all of the TreeIters, loop and
				// remove them all
				while(iterQueue.Count > 0)
				{
					TreeIter iter = (TreeIter) iterQueue.Dequeue();
	
					ConflictHolder ch = (ConflictHolder) tModel.GetValue(iter, 0);
					if(!ch.IsNameConflict)
					{
						try
						{
							ifws.ResolveFileConflict(	
											ch.FileConflict.iFolderID, 
											ch.FileConflict.ConflictID, 
											localChangesWin);
		
							ConflictTreeStore.Remove(ref iter);
						}
						catch
						{}
					}
				}
				UpdateFields(null, false);
			}
		}

	}
}
