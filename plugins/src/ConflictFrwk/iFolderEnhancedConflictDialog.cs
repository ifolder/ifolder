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
*                 $Author: AShok Singh <siashok@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*****************************************************************************/

using System;
using Gtk;
using System.Collections;
using System.IO;
using Novell.iFolder;

namespace Novell.EnhancedConflictResolution
{

	/// <summary>
	/// This is the conflict resolver for iFolder
	/// </summary>
	public class iFolderEnhancedConflictDialog : iFolderConflictDialog 
	{
		//
		// File Conflict Box
		//
		private Gtk.Frame		ActionFrame;

		private RadioButton		LocalVersion;
		private RadioButton		ServerVersion;
		private Gtk.Button		SaveButton;
		private Entry 			DirectoryPath;
		private Button 			BrowseButton;
		private RadioButton		CbinLocalVersion;
		private RadioButton		CbinServerVersion;
		private CheckButton		EnableConflictBin;

		/// <summary>
		/// Default constructor for iFolderConflictResolver
		/// </summary>
		public iFolderEnhancedConflictDialog(	Gtk.Window parent,
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
            ActionFrame.Sensitive = false;
			this.Realized += new EventHandler(OnRealizeWidget);

			// Bind ESC and C-w to close the window
			ControlKeyPressed = false;
			KeyPressEvent += new KeyPressEventHandler(KeyPressHandler);
			KeyReleaseEvent += new KeyReleaseEventHandler(KeyReleaseHandler);
            this.Response += new ResponseHandler(OnConflictDialogResponse);
		}

        /// <summary>
        /// Event handler for conflict dialog response
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">argument for response</param>
		private void OnConflictDialogResponse(object obj, ResponseArgs args)
		{
			iFolderEnhancedConflictDialog conflictDialog = (iFolderEnhancedConflictDialog) obj;
			if (args.ResponseId == ResponseType.Help)
				Util.ShowHelp("conflicts.html", this);
			else
			{
				if (conflictDialog != null)
				{
					conflictDialog.Hide();
					conflictDialog.Destroy();
					
					/*if (ConflictDialogs.ContainsKey(conflictDialog.iFolder.ID))
						ConflictDialogs.Remove(conflictDialog.iFolder.ID);*/

					conflictDialog = null;
				}
			}
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

			LocalSaveButton = new Button(Stock.Open);
			localTable.Attach(LocalSaveButton, 0,1,3,4,
                 			  Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, 0, 5);
			LocalSaveButton.Clicked += new EventHandler(LocalOpenHandler);
			
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

			ServerSaveButton = new Button(Stock.Open);
			serverTable.Attach(ServerSaveButton, 0,1,3,4,
                       		   Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, 0, 5);
			ServerSaveButton.Clicked += new EventHandler(ServerOpenHandler);
		
			ServerFrame.Add(serverTable);

			//
			// action box starts here
			//

			vbox.PackStart(fileConflictBox, false, false, 0);
			ActionFrame = new Frame(Util.GS("Action"));
			this.VBox.PackStart(ActionFrame, true, true, 0);

			Table actionTable = new Table(3,5,false);
			actionTable.BorderWidth = 5;
			actionTable.ColumnSpacing = 10;			

			Label lable = new Label(Util.GS("Save to iFolder")+Util.GS(":"));
			lable.Xalign = 0;
			actionTable.Attach(lable, 0,1,0,1,
							   AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			
			LocalVersion = new RadioButton(Util.GS("Local Version"));
			actionTable.Attach(LocalVersion, 3,4,0,1,
				Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Fill, 0, 5);

			ServerVersion = new RadioButton(LocalVersion, Util.GS("Server Version"));
			actionTable.Attach(ServerVersion, 4,5,0,1,
                			   Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Fill, 0, 5);

			lable = new Label(Util.GS("Save to Conflict Bin")+Util.GS(":"));
			lable.Xalign = 0;
			actionTable.Attach(lable, 0,1,1,2,
							   AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			
			CbinLocalVersion = new RadioButton(Util.GS("Local Version"));
			actionTable.Attach(CbinLocalVersion, 3,4,1,2,
                			   Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Fill, 0, 5);

			CbinServerVersion = new RadioButton(CbinLocalVersion, Util.GS("Server Version"));
			actionTable.Attach(CbinServerVersion, 4,5,1,2,
                     		   Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Fill, 0, 5);

			EnableConflictBin = new CheckButton(Util.GS("Enable"));
			actionTable.Attach(EnableConflictBin, 1,2,1,2,
                     		   Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Fill, 0, 5);

			LocalVersion.Toggled += new EventHandler(OnSaveToiFolderSelected);
			ServerVersion.Toggled += new EventHandler(OnSaveToiFolderSelected);
			CbinLocalVersion.Toggled += new EventHandler(OnSaveToCBinSelected);
			CbinServerVersion.Toggled += new EventHandler(OnSaveToCBinSelected);
			EnableConflictBin.Toggled += new EventHandler(OnEnableCBinChecked);

			DirectoryPath = new Entry();
			this.DirectoryPath.Changed += new EventHandler(OnDirectoryPathChanged);
			actionTable.Attach(DirectoryPath, 1,3, 2,3,
				AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			lable  = new Label(Util.GS("Conflict Bin Path")+Util.GS(":"));
			actionTable.Attach(lable, 0,1, 2,3,
			AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			lable.LineWrap = true;
			lable.Xalign = 0.0F;
			lable.MnemonicWidget = DirectoryPath;

			BrowseButton = new Button("_Browse");
			actionTable.Attach(BrowseButton, 3,4, 2,3, AttachOptions.Fill, 0,0,0);
			BrowseButton.Clicked += new EventHandler(OnBrowseButtonClicked);

			
			SaveButton = new Button(Stock.Save);
			actionTable.Attach(SaveButton, 4,5,2,3,
				Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, 0, 8);
			SaveButton.Clicked += new EventHandler(SaveHandler);

			ActionFrame.Add(actionTable);
			
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
						OnEnhancedConflictSelectionChanged);

//			ConflictPixBuf = 
//				new Gdk.Pixbuf(Util.ImagesPath("conflict24.png"));

			this.AddButton(Stock.Close, ResponseType.Ok);
			this.AddButton(Stock.Help, ResponseType.Help);

			RefreshConflictList();
		}

		/// <summary>
		///  Controls the visibility/activity of new components added in this( derived ) class 
		/// </summary>
		private void EnableEnhancedConflictControls()
		{
			ActionFrame.Sensitive = true;

			LocalVersion.Sensitive = true;
			ServerVersion.Sensitive = true;
			LocalVersion.Active = true;
			CbinServerVersion.Active = true;
			if(EnableConflictBin.Active == true)
			{
				CbinLocalVersion.Sensitive = true;
				CbinServerVersion.Sensitive = true;
				DirectoryPath.Sensitive = true;	
				BrowseButton.Sensitive = true;
			}
			else
			{
				CbinLocalVersion.Sensitive = false;
				CbinServerVersion.Sensitive = false;
				DirectoryPath.Sensitive = false;	
				BrowseButton.Sensitive = false;
			}
		}

        /// <summary>
        /// To handle if the selection is changed in conflict tree display
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
		private void OnEnhancedConflictSelectionChanged(object o, EventArgs args)
		{
			bool bHasFileConflict = false;
           
            OnConflictSelectionChanged( o, args );
           
			TreeSelection tSelect = ConflictTreeView.Selection;
			int selectedRows = tSelect.CountSelectedRows();
			if(selectedRows > 0)
			{
				EnableEnhancedConflictControls();
				TreeModel tModel;
				ConflictHolder ch = null;

				Array treePaths = tSelect.GetSelectedRows(out tModel);

				foreach(TreePath tPath in treePaths)
				{
					TreeIter iter;
					if(ConflictTreeStore.GetIter(out iter, tPath))
					{
						ch = (ConflictHolder) tModel.GetValue(iter, 0);
                        if(!ch.IsNameConflict)    
							bHasFileConflict = true;
					}
				}
				
				if (selectedRows == 1)
				{
					if (bHasFileConflict)
                        EnableEnhancedConflictControls();
					else
                        ActionFrame.Sensitive = false;
                        
				}
				else
				{
					// We're dealing with multiple selections here
					if (bHasFileConflict)
					{
						EnableEnhancedConflictControls();
					}
					else
					{
                        ActionFrame.Sensitive = false;
					}
				}
			}
		}

        /// <summary>
        /// resolve the conflict in the path
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
		private void SaveHandler(object o, EventArgs args)
		{
            string path = null;

            if(EnableConflictBin.Active == true)
            {
                if(DirectoryPath.Text == String.Empty)
                {
                    iFolderMsgDialog dialog = new iFolderMsgDialog(
                        this,
                        iFolderMsgDialog.DialogType.Error,
                        iFolderMsgDialog.ButtonSet.Ok,
                        "",
                        Util.GS("Path field is Empty"),
                        Util.GS("Specify a valid path for the Conflict Bin"));
                    dialog.Run();
                    dialog.Hide();
                    dialog.Destroy();
                    return;
                }

                try
                {
                    if(!Directory.Exists(DirectoryPath.Text))
                        Directory.CreateDirectory(DirectoryPath.Text);
                }
                catch
                {
                    iFolderMsgDialog dialog = new iFolderMsgDialog(
                        this,
                        iFolderMsgDialog.DialogType.Error,
                        iFolderMsgDialog.ButtonSet.Ok,
                        "",
                        Util.GS("Invalid Conflict Bin Directory"),
                        Util.GS("Specify a valid path for the Conflict Bin"));
                    dialog.Run();
                    dialog.Hide();
                    dialog.Destroy();
                    return;
                }
            
                path = System.IO.Path.GetFullPath(DirectoryPath.Text);
            
                if(!path.EndsWith(Util.GS("/")))
                    path = path + Util.GS("/");
            }       
                
            if(LocalVersion.Active == true)
                ResolveSelectedConflicts(true, path);
            else
                ResolveSelectedConflicts(false, path);
        }

        /// <summary>
        /// Gets the current conflict holder
        /// </summary>
        /// <returns>conflict holder</returns>
        private ConflictHolder GetCurrentConflictHolder()
        {
            TreeModel tModel;
            Queue   iterQueue = GetSelectedItemsInQueue(out tModel);

            if(iterQueue.Count == 1)
            {
                TreeIter iter = (TreeIter) iterQueue.Dequeue();

                ConflictHolder ch = (ConflictHolder) tModel.GetValue(iter, 0);
                if(!ch.IsNameConflict)
                {
                    return ch;        
                }
            }
            return null;
        }
        
        /// <summary>
        /// Open the local file conflict holder
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
        private void LocalOpenHandler(object o, EventArgs args)
        {
            ConflictHolder ch = GetCurrentConflictHolder();
            if( ch != null )
            {
                try
                {
                    System.Diagnostics.Process.Start(ch.FileConflict.LocalFullPath);
                }
                catch(Exception oe)
                {
                    Console.WriteLine("File open error: {0}", oe.Message );
                }
            }
        }
        
        /// <summary>
        /// Open the server file conflict holder
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
        private void ServerOpenHandler(object o, EventArgs args)
        {
            ConflictHolder ch = GetCurrentConflictHolder();
            if( ch != null )
            {
                try
                {
                    System.Diagnostics.Process.Start(ch.FileConflict.ServerFullPath);
                }
                catch(Exception oe)
                {
                    Console.WriteLine("File open error: {0}", oe.Message );
                }
            }
        }

		private void OnDirectoryPathChanged(object o, EventArgs args)
		{
	
		}

        /// <summary>
        /// Event handler for browse button clicked
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
		private void OnBrowseButtonClicked (object o, EventArgs args)
		{
			//change the message to "Select a file..." instead of "Select a folder..."
			FileChooserDialog filedlg = new FileChooserDialog("", Util.GS("Select a folder..."), this, FileChooserAction.SelectFolder, Stock.Cancel, ResponseType.Cancel,Stock.Ok, ResponseType.Ok);
			int res = filedlg.Run();
			string str = filedlg.Filename;
			filedlg.Hide();
			filedlg.Destroy();
			if( res == (int)ResponseType.Ok)
			{
				this.DirectoryPath.Text = str;
			}
			//Otherwise do nothing	
		}
		
        /// <summary>
        /// Even handler for save button selected
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
		private void OnSaveToiFolderSelected (object o, EventArgs args)
		{
			if(LocalVersion.Active == true)
			{
				LocalVersion.Active = true;			
				ServerVersion.Active = false;
				if(EnableConflictBin.Active == true)
				{
					CbinLocalVersion.Active = false;			
					CbinServerVersion.Active = true;
				}
				else
				{
					CbinLocalVersion.Active = false;			
					CbinServerVersion.Active = false;
				}
			}
			else
			{
				LocalVersion.Active = false;			
				ServerVersion.Active = true;	
				if(EnableConflictBin.Active == true)
				{
					CbinLocalVersion.Active = true;			
					CbinServerVersion.Active = false;
				}
				else
				{
					CbinLocalVersion.Active = false;			
					CbinServerVersion.Active = false;
					CbinLocalVersion.Sensitive = false;			
					CbinServerVersion.Sensitive = false;					
				}
			}
		}

        /// <summary>
        /// Event handler for saving to bin selection
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
		private void OnSaveToCBinSelected (object o, EventArgs args)
		{
			if(CbinServerVersion.Active == true)
			{
				LocalVersion.Active = true;			
				ServerVersion.Active = false;			
				CbinLocalVersion.Active = false;			
				CbinServerVersion.Active = true;
			}
			else
			{
				LocalVersion.Active = false;			
				ServerVersion.Active = true;			
				CbinLocalVersion.Active = true;			
				CbinServerVersion.Active = false;
			}
		}
		
        /// <summary>
        /// Event handler for Enabling conflict bin checked
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
		private void OnEnableCBinChecked (object o, EventArgs args)
		{
			if(EnableConflictBin.Active == true)
			{
				DirectoryPath.Sensitive = true;
				BrowseButton.Sensitive = true;
				CbinLocalVersion.Sensitive = true;			
				CbinServerVersion.Sensitive = true;
				if(LocalVersion.Active == true)
				{
					CbinLocalVersion.Active = false;			
					CbinServerVersion.Active = true;
				}
				else
				{
					CbinLocalVersion.Active = true;			
					CbinServerVersion.Active = false;
				}			
			}
			else
			{
				DirectoryPath.Text = String.Empty;
				DirectoryPath.Sensitive = false;			
				BrowseButton.Sensitive = false;			
				CbinLocalVersion.Sensitive = false;			
				CbinServerVersion.Sensitive = false;				
			}
		}
	    
        /// <summary>
        /// Get the selected items
        /// </summary>
        /// <param name="tModel"></param>
        /// <returns>a queue</returns>
        private Queue GetSelectedItemsInQueue(out TreeModel tModel)
        {
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
            return iterQueue;
        }
        
        /// <summary>
        /// Gets the selected Items and resolves the conflict based on whether local wins or server wins
        /// </summary>
        /// <param name="localChangesWin">who has higher priority</param>
        /// <param name="conflictBinPath">path of the conflict bin </param>
		private void ResolveSelectedConflicts(bool localChangesWin, string conflictBinPath)
		{
			TreeModel tModel;
            
			Queue   iterQueue = GetSelectedItemsInQueue(out tModel);

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

                            if( null != conflictBinPath )
                            {
    							ifws.ResolveEnhancedFileConflict(	
											ch.FileConflict.iFolderID, 
											ch.FileConflict.ConflictID, 
											localChangesWin,
											conflictBinPath);
                            }
                            else
                            {
    							ifws.ResolveEnhancedFileConflict(	
											ch.FileConflict.iFolderID, 
											ch.FileConflict.ConflictID, 
											localChangesWin,
											string.Empty);
                            }
		
							ConflictTreeStore.Remove(ref iter);
						}
						catch
						{}
					}
				}
				UpdateFields(null, false);
                EnableEnhancedConflictControls();
			}
		}

	}
}
