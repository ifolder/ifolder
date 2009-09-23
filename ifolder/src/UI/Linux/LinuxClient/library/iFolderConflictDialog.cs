/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
  *
  *                 $Author: Calvin Gaisford <cgaisford@novell.com>
  *                 $Modified by: <Modifier>
  *                 $Mod Date: <Date Modified>
  *                 $Revision: 0.0
  *-----------------------------------------------------------------------------
  * This module is used to:
  *        <Description of the functionality of the file >
  *
  *
  *******************************************************************************/

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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="con">Conflict</param>
        /// <param name="iFolderPath">iFolder Path</param>
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
//					Debug.PrintLine("Creating with Local Conflict");
					localConflict = con;
				}
				else
				{
//					Debug.PrintLine("Creating with Server Conflict");
					serverConflict = con;
				}

			}
		}

        /// <summary>
        /// Gets whether if it is a Name Conflict
        /// </summary>
		public bool IsNameConflict
		{
			get{ return isNameConflict; }
		}

        /// <summary>
        /// Gets / Sets the FileConflict
        /// </summary>
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

        /// <summary>
        /// Gets the LocalNameConflict
        /// </summary>
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

        /// <summary>
        /// Gets the ServerNameConflict
        /// </summary>
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
		
        /// <summary>
        /// Gets the Conflict Name
        /// </summary>
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
		
        /// <summary>
        /// Gets the Relative path to the iFolder Conflict
        /// </summary>
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
		
        /// <summary>
        /// Gets the Conflict type
        /// </summary>
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

        /// <summary>
        /// Add Name Conflict
        /// </summary>
        /// <param name="con">Conflict</param>
		public void AddNameConflict(Conflict con)
		{
//			Debug.PrintLine("Adding a new conflict");
			if(!con.IsNameConflict)
				throw new Exception("Cannot add a FileConflict");

			if(	(con.LocalName != null) &&
				(con.LocalName.Length > 0) &&
				(localConflict == null) )
			{
//				Debug.PrintLine("Adding a local Conflict");
				localConflict = con;
			}
			else if(serverConflict == null)
			{
//				Debug.PrintLine("Adding a server Conflict");
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
		protected iFolderWebService	ifws;
		protected SimiasWebService	simws;
		protected iFolderWeb		ifolder;
		protected Gtk.TreeView		ConflictTreeView;
		protected Gtk.ListStore		ConflictTreeStore;
		protected bool				ControlKeyPressed;


		//
		// File Conflict Box
		//
		protected HBox				fileConflictBox;
		protected Gtk.Frame			ServerFrame;
		protected Gtk.Frame			LocalFrame;
		protected Gtk.Frame			ActionFrame; 
		protected Gtk.Label			LocalNameLabel;
		protected Gtk.Label			LocalNameValue;
		protected Gtk.Label			LocalDateLabel;
		protected Gtk.Label			LocalDateValue;
		protected Gtk.Label			LocalSizeLabel;
		protected Gtk.Label			LocalSizeValue;
		protected Gtk.Button		LocalSaveButton;
		protected Gtk.Label			ServerNameLabel;
		protected Gtk.Label			ServerNameValue;
		protected Gtk.Label			ServerDateLabel;
		protected Gtk.Label			ServerDateValue;
		protected Gtk.Label			ServerSizeLabel;
		protected Gtk.Label			ServerSizeValue;
		protected Gtk.Button			ServerSaveButton;

		//
		// Name Conflict Box
		//
		protected VBox				nameConflictBox;
		protected Frame				renameFileFrame;
		protected Label				nameConflictSummary;
		protected Label				nameConflictFileNameLabel;
		protected Entry				nameConflictEntry;
		protected Button			nameEntrySaveButton;
		protected string			oldFileName;

		protected Hashtable			conflictTable;

        /// <summary>
        /// Gets the iFolder Web Object
        /// </summary>
		public iFolderWeb iFolder
		{
			get
			{
				return ifolder;
			}
		}

        // Dummy constructor for Derived class
        public iFolderConflictDialog()
        {

        }
		/// <summary>
		/// Default constructor for iFolderConflictResolver
		/// </summary>
		public iFolderConflictDialog(	Gtk.Window parent,
										iFolderWeb ifolder,
										iFolderWebService iFolderWS,
										SimiasWebService SimiasWS)
			: base()
		{
			this.Title = Util.GS("Resolve Conflicts");
			if(iFolderWS == null)
				throw new ApplicationException("iFolderWebServices was null");
			this.ifws = iFolderWS;
			this.simws = SimiasWS;
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

			// Bind ESC and C-w to close the window
			ControlKeyPressed = false;
			KeyPressEvent += new KeyPressEventHandler(KeyPressHandler);
			KeyReleaseEvent += new KeyReleaseEventHandler(KeyReleaseHandler);
			oldFileName = null;
		}


		/// <summary>
		/// Set up the UI inside the Window
		/// </summary>
		private void InitializeWidgets()
		{
			this.SetDefaultSize (600, 480);
			this.Icon = 
				new Gdk.Pixbuf(Util.ImagesPath("ifolder-warning16.png"));
			VBox vbox = new VBox();
			vbox.Spacing = 10;
			vbox.BorderWidth = 10;
			this.VBox.PackStart(vbox, true, true, 0);

			HBox topbox = new HBox();
			topbox.Spacing = 10;

			Gdk.Pixbuf bigConflict =
				new Gdk.Pixbuf(Util.ImagesPath("ifolder-warning48.png"));
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
			nameValue.UseUnderline = false;
			nameValue.Xalign = 0;
			ifTable.Attach(nameValue, 1,2,0,1);

			Label pathLabel = new Label(Util.GS("Location:"));
			pathLabel.Xalign = 1;
			ifTable.Attach(pathLabel, 0,1,1,2,
				Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, 0, 0);

			Label pathValue = new Label(ifolder.UnManagedPath);
			pathValue.UseUnderline = false;
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
			LocalNameValue.UseUnderline = false;
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
			ServerNameValue.UseUnderline = false;
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

			renameFileFrame = new Frame(Util.GS("Rename"));
			nameConflictBox.PackStart(renameFileFrame, true, true, 0);

			nameConflictSummary = new Label(Util.GS("Enter a new name and click Rename to resolve the conflict."));
			nameConflictSummary.Xalign = 0;
//			nameConflictSummary.ColumnSpacing = 10;
			
			nameConflictInternalBox.PackStart(nameConflictSummary, false, false, 0);

			HBox nameConflictHBox = new HBox();
			nameConflictHBox.Spacing = 10;
			
			nameConflictFileNameLabel = new Label(Util.GS("Name:"));
			nameConflictHBox.PackStart(nameConflictFileNameLabel, false, false, 0);
			
			nameConflictEntry = new Entry();
			nameConflictEntry.CanFocus = true;
			nameConflictEntry.Changed += new EventHandler(OnNameEntryChanged);
			nameConflictEntry.ActivatesDefault = true;
			nameConflictHBox.PackStart(nameConflictEntry, true, true, 0);

			nameConflictInternalBox.PackStart(nameConflictHBox, false, false, 0);
			
			HBox saveButtonBox = new HBox();
			
			nameEntrySaveButton = new Button(Util.GS("Rename"));
			nameEntrySaveButton.Clicked += new EventHandler(RenameFileHandler);
			saveButtonBox.PackStart(nameEntrySaveButton, false, false, 0);
			nameConflictInternalBox.PackEnd(saveButtonBox, false, false, 0);
			
			renameFileFrame.Add(nameConflictInternalBox);

			vbox.PackStart(nameConflictBox, false, false, 0);

			nameConflictBox.Visible = false;
			
			// Set up the iFolder TreeView
			ConflictTreeStore = new ListStore(typeof(ConflictHolder));
			ConflictTreeView.Model = ConflictTreeStore;
			
			// File Name Column
			TreeViewColumn fileNameColumn = new TreeViewColumn();
			fileNameColumn.Title = Util.GS("Name");
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
			pathColumn.Title = Util.GS("Folder");
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
			
			
			

			// Set up Pixbuf and Text Rendering for "iFolder Conflicts" column
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

        /// <summary>
        /// Event Handler for Key Press event
        /// </summary>
        protected void KeyPressHandler(object o, KeyPressEventArgs args)
		{
			args.RetVal = true;
			
			switch(args.Event.Key)
			{
				case Gdk.Key.Escape:
					Respond(ResponseType.Cancel);
					break;
				case Gdk.Key.Control_L:
				case Gdk.Key.Control_R:
					ControlKeyPressed = true;
					args.RetVal = false;
					break;
				case Gdk.Key.W:
				case Gdk.Key.w:
					if (ControlKeyPressed)
						Respond(ResponseType.Cancel);
					else
						args.RetVal = false;
					break;
				default:
					args.RetVal = false;
					break;
			}
		}
		
        /// <summary>
        /// Event Handler for Key Release event
        /// </summary>
        protected void KeyReleaseHandler(object o, KeyReleaseEventArgs args)
		{
			args.RetVal = false;
			
			switch(args.Event.Key)
			{
				case Gdk.Key.Control_L:
				case Gdk.Key.Control_R:
					ControlKeyPressed = false;
					break;
				default:
					break;
			}
		}

        /// <summary>
        /// Event Handler for On realiz widget event
        /// </summary>
        protected void OnRealizeWidget(object o, EventArgs args)
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
					"",
					Util.GS("You have read only access"),
					Util.GS("Your ability to resolve conflicts is limited because you have read-only access to this iFolder.  Name conflicts must be renamed locally.  File conflicts will be overwritten by the version of the file on the server."));
				dg.Run();
				dg.Hide();
				dg.Destroy();
			}
		}

        /// <summary>
        /// Refresh Conflicts
        /// </summary>
		protected void RefreshConflictList()
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

//						Debug.PrintLine("Key = {0}", key);

						if(conflictTable.ContainsKey(key))
						{
//							Debug.PrintLine("Found key, adding to holder");
							ConflictHolder ch = 
								(ConflictHolder)conflictTable[key];
							ch.AddNameConflict(con);
						}
						else
						{
//							Debug.PrintLine("No key, New holder");
							ConflictHolder ch = new ConflictHolder(con, ifolder.UnManagedPath);
							if(con.LocalFullPath != null)
								ConflictTreeStore.AppendValues(ch);
							conflictTable.Add(key, ch);
						}
					}
				}
			}
			catch(Exception ex)
			{
				Debug.PrintLine(ex.Message);
			}
		}




//		private void ConflictCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
//				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
//				Gtk.TreeIter iter)
//		{
//			((CellRendererPixbuf) cell).Pixbuf = ConflictPixBuf;
//		}

        /// <summary>
        /// File Name Cell Text Data Func
        /// </summary>
        protected void FileNameCellTextDataFunc(Gtk.TreeViewColumn col,
											  Gtk.CellRenderer cellRenderer,
											  Gtk.TreeModel model,
											  Gtk.TreeIter iter)
		{
			ConflictHolder conflictHolder = (ConflictHolder) model.GetValue(iter,0);
			((CellRendererText) cellRenderer).Text = conflictHolder.Name;
		}

		protected void PathCellTextDataFunc(Gtk.TreeViewColumn col,
											  Gtk.CellRenderer cellRenderer,
											  Gtk.TreeModel model,
											  Gtk.TreeIter iter)
		{
			ConflictHolder conflictHolder = (ConflictHolder) model.GetValue(iter,0);
			((CellRendererText) cellRenderer).Text = conflictHolder.RelativePath;
		}

		protected void ConflictTypeCellTextDataFunc(Gtk.TreeViewColumn col,
											  Gtk.CellRenderer cellRenderer,
											  Gtk.TreeModel model,
											  Gtk.TreeIter iter)
		{
			ConflictHolder conflictHolder = (ConflictHolder) model.GetValue(iter,0);
			((CellRendererText) cellRenderer).Text = conflictHolder.Type;
		}


        /// <summary>
        /// Event Handler for OnConflictSelectionChanged event
        /// </summary>
        protected void OnConflictSelectionChanged(object o, EventArgs args)
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
						nameConflictSummary.Text = Util.GS("Enter a new name and click Rename to resolve the conflict.");

						// This is a name conflict
						nameConflictBox.Visible = true;
						fileConflictBox.Visible = false;

						// Prefill the entry with the filename and auto-select
						// the text on the left-hand side of the extension.
						nameConflictEntry.Text = ch.Name;
						oldFileName = nameConflictEntry.Text;
						
						this.FocusChild = nameConflictEntry;
						
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


        /// <summary>
        /// Update Files
        /// </summary>
        /// <param name="ch">Conflict Holder</param>
        /// <param name="multiSelect">true on multi select else false</param>
		protected void UpdateFields(ConflictHolder ch, bool multiSelect)
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
                    if( LocalSaveButton.Label == Stock.Save )
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
                    if( LocalSaveButton.Label == Stock.Save )
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


        /// <summary>
        /// Enable Conflicts Control
        /// </summary>
        /// <param name="enable">Enable on true else disable</param>
		protected void EnableConflictControls(bool enable)
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

        /// <summary>
        /// Event Handler for Save Local Handler
        /// </summary>
        private void SaveLocalHandler(object o, EventArgs args)
		{
			ResolveSelectedConflicts(true);
		}

        /// <summary>
        /// Event Handler for Save Server handler
        /// </summary>
        private void SaveServerHandler(object o, EventArgs args)
		{
			ResolveSelectedConflicts(false);
		}

        /// <summary>
        /// Event Handler for Rename Event Handler
        /// </summary>
        protected void RenameFileHandler(object o, EventArgs args)
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
						if (snc != null && ifolder.CurrentUserRights == "ReadOnly")
						{
							ifws.RenameAndResolveConflict(snc.iFolderID,
														  snc.ConflictID,
														  newFileName);
						}
						else
						{
							if (lnc != null)
							{
								//server file rename is not certified so we are not allowing the local file renamed to same name
								// this is a work around later we will fix the sever rename as well
								if(newFileName == oldFileName)
								{
									iFolderMsgDialog dg = new iFolderMsgDialog(
										this,
										iFolderMsgDialog.DialogType.Error,
										iFolderMsgDialog.ButtonSet.Ok,
										"",
										Util.GS("Name Already Exists"),
										Util.GS("The specified name already exists.  Please choose a different name."),
										null);
									dg.Run();
									dg.Hide();
									dg.Destroy();
									return;
								}
							Conflict[] conflictList = ifws.GetiFolderConflicts(lnc.iFolderID);	
								
							ifws.ResolveNameConflict(lnc.iFolderID,lnc.ConflictID,newFileName);

		                                        foreach(Conflict con in conflictList)
                		                        {
                                		                if(con.IsNameConflict && con.ServerName != null)
								if(String.Compare(lnc.LocalFullPath,con.ServerFullPath,true)== 0)
								{
							                        		                                                                                                  if (ifolder.CurrentUserRights == "ReadOnly")
                                                                   {
								        ifws.RenameAndResolveConflict(con.iFolderID,con.ConflictID,con.ServerName);
									break;
								    }
                                                                else
								    {
                                                                        ifws.ResolveNameConflict(con.iFolderID,con.ConflictID,con.ServerName);
									break;
								    }
								}
                                        		}
							ifws.SynciFolderNow(lnc.iFolderID);
						}
							// If this is a name conflict because of case-sensitivity
							// on Linux, there won't be a conflict on the server.
							if (snc != null)
							{
								//server file rename is not certified so we are not allowing the server file rename, rather rename to the same name 
								// this is a work around later we will fix the sever rename as well
								if(newFileName != oldFileName)
								{
									iFolderMsgDialog dg = new iFolderMsgDialog(
										this,
										iFolderMsgDialog.DialogType.Error,
										iFolderMsgDialog.ButtonSet.Ok,
										"",
										Util.GS("Name Already Exists"),
										Util.GS("The specified name already exists.  Please choose a different name."),
										null);
									dg.Run();
									dg.Hide();
									dg.Destroy();
									return;
								}
								
								ifws.ResolveNameConflict(snc.iFolderID,
														 snc.ConflictID,
														 ch.Name);
							}
						}

						ConflictTreeStore.Remove(ref iter);
					}
					catch (Exception e)
					{
						bool bKnownError = true;
						string headerText = Util.GS("iFolder Conflict Error");
						string errText    = Util.GS("An error was encountered while resolving the conflict.");

						if (e.Message.IndexOf("Malformed") >= 0)
						{
							headerText = Util.GS("Invalid Characters in Name");
							errText = string.Format(Util.GS("The specified name contains invalid characters.  Please choose a different name and try again.\n\nNames must not contain any of these characters: {0}"),
													simws.GetInvalidSyncFilenameChars());
						}
						else if (e.Message.IndexOf("already exists") >= 0)
						{
							headerText = Util.GS("Name Already Exists");
							errText = Util.GS("The specified name already exists.  Please choose a different name.");
						}
						else
						{
							//bKnownError = false;
						}

						iFolderMsgDialog dg = new iFolderMsgDialog(
							this,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.Ok,
							"",
							headerText,
							errText,
							bKnownError ? null : e.Message);
						dg.Run();
						dg.Hide();
						dg.Destroy();

						tSelect.SelectIter(iter);
						
						// FIXME: Figure out why if the user clicks the "Save" button the focus doesn't return back to the entry text box.  (i.e., the next line of code isn't really doing anything)
						this.FocusChild = nameConflictEntry;
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
		protected void OnNameEntryChanged(object o, EventArgs args)
		{
			if (nameConflictEntry.Text.Length > 0)
				nameEntrySaveButton.Sensitive = true;
			else
				nameEntrySaveButton.Sensitive = false;
		}

        /// <summary>
        /// Resolve Selected Conflicts
        /// </summary>
        /// <param name="localChangesWin">true if localchanges</param>
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
