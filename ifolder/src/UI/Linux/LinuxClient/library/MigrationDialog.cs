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
  *                 $Author: Ramesh Sunder <sramesh@novell.com>
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
using System.IO;
using System.Collections;
using Gtk;
using Simias.Client;
using Simias.Client.Event;
using Novell.iFolder.Events;
using Novell.iFolder.Controller;

namespace Novell.iFolder
{
	/// <summary>
	/// This is the iFolder Log Window
	/// </summary>
	public class MigrationDialog : Dialog
	{
                //private Gtk.Window                              topLevelWindow;
                //private iFolderWebService               ifws;
		//private SimiasWebService 		simws;
                private iFolderTreeView         AccTreeView;
                private ListStore                       AccTreeStore;
                private Button                          MigrateButton;
                private Button                          DetailsButton;
		private String iFolderName;
		private string userName;
		private bool cancelled;
                private Hashtable curDomains;

		private String MergeLocation;

        /// <summary>
        /// Gets / Sets Merge Path
        /// </summary>
		public string MergePath
		{
			get
			{
				return this.MergeLocation;
			}
			set
			{
				this.MergeLocation = value;
			}
		}

        /// <summary>
        /// Gets / Sets UserName
        /// </summary>
		public string UserName
		{
			get
			{
				return this.userName;
			}
			set
			{
				this.userName = value;
			}
		}

        /// <summary>
        /// Gets / Sets Cancelled
        /// </summary>
		public bool Cancelled
		{
			get
			{
				return this.cancelled;
			}
			set
			{
				this.cancelled = value;
			}
		}
		/// <summary>
                /// Default constructor for LogWindow
                /// </summary>
                public MigrationDialog( Gtk.Window topWindow, iFolderWebService ifws, SimiasWebService simws, String ifolderName)
                        : this (topWindow, ifws, simws, false, "")
                {
		}

		/// <summary>
		/// Default constructor for LogWindow
		/// </summary>
		public MigrationDialog( Gtk.Window topWindow, iFolderWebService ifws, SimiasWebService simws, bool ShowForMerge, String ifolderName)
			//: base (Util.GS("iFolder Migration"))
		{
                       // this.topLevelWindow = topWindow;
			this.Modal = false;
                        //this.ifws = ifws;
			//this.simws = simws;
			this.iFolderName = ifolderName;
                        curDomains = new Hashtable();
			CreateWidgets();
			PopulateWidgets();
		}

        /// <summary>
        /// Destructor
        /// </summary>
		~MigrationDialog()
		{
		}

		/// <summary>
		/// Set up the UI inside the Window
		/// </summary>
		private void CreateWidgets()
		{
			this.SetDefaultSize (500, 400);
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));
			this.WindowPosition = Gtk.WindowPosition.Center;

			//VBox vbox = this.VBox; //new VBox (false, 0);
			//this.Add (vbox);

                        this.VBox.Spacing = 10;
                        this.VBox.BorderWidth = 10;

                        // Set up the iFolder2.x Accounts tree view in a scrolled window
                        AccTreeView = new iFolderTreeView();
                        ScrolledWindow sw = new ScrolledWindow();
                        sw.ShadowType = Gtk.ShadowType.EtchedIn;
                        sw.Add(AccTreeView);
                        this.VBox.PackStart(sw, true, true, 0);

                        AccTreeStore = new ListStore(typeof(string));
                        AccTreeView.Model = AccTreeStore;

                        // User Name Column
                        TreeViewColumn serverColumn = new TreeViewColumn();
                        serverColumn.Title = Util.GS("User Name");
                        CellRendererText servercr = new CellRendererText();
                        servercr.Xpad = 5;
                        serverColumn.PackStart(servercr, false);
                        serverColumn.SetCellDataFunc(servercr,
                                                                                 new TreeCellDataFunc(ServerCellTextDataFunc));
                        serverColumn.Resizable = true;
                        serverColumn.MinWidth = 150;
                        AccTreeView.AppendColumn(serverColumn);

                        // Home Location Column
                        TreeViewColumn nameColumn = new TreeViewColumn();
                        nameColumn.Title = Util.GS("Home Location");
                        CellRendererText ncrt = new CellRendererText();
                        nameColumn.PackStart(ncrt, false);
                        nameColumn.SetCellDataFunc(ncrt,
                                                                           new TreeCellDataFunc(NameCellTextDataFunc));
                        nameColumn.Resizable = true;
                        nameColumn.MinWidth = 175;
                        AccTreeView.AppendColumn(nameColumn);

                        AccTreeView.Selection.Mode = SelectionMode.Single;
                        AccTreeView.Selection.Changed +=
                                new EventHandler(AccSelectionChangedHandler);
			
                        // Status column
                        TreeViewColumn statusColumn = new TreeViewColumn();
			statusColumn.Title = Util.GS("Status");
			CellRendererText scrt = new CellRendererText();
			statusColumn.PackStart(scrt, false);
			statusColumn.SetCellDataFunc(scrt, new TreeCellDataFunc(StatusCellTextDataFunc));
                        statusColumn.Resizable = true;
                        statusColumn.MinWidth = 75;
                        AccTreeView.AppendColumn(statusColumn);
			MigrateButton = (Button) this.AddButton(Stock.Ok, ResponseType.Ok);
			MigrateButton.Label = Util.GS("_Merge");
			DetailsButton = (Button) this.AddButton( Stock.Cancel, ResponseType.Cancel);
			MigrateButton.Clicked += new EventHandler(OnMigrateAccount);
			
/*
                        // Set up buttons for add/remove/accept/decline
                        HButtonBox buttonBox = new HButtonBox();
                        buttonBox.Spacing = 10;
                        buttonBox.Layout = ButtonBoxStyle.End;
                        this.VBox.PackStart(buttonBox, false, false, 0);

                        MigrateButton = new Button("_Merge");
                        buttonBox.PackStart(MigrateButton);
                        MigrateButton.Clicked += new EventHandler(OnMigrateAccount);

                        DetailsButton = new Button(Gtk.Stock.Cancel);
                        buttonBox.PackStart(DetailsButton);
			this.DetailsButton.Clicked += new EventHandler(OnCancelClicked);
*/			
		}

                /// <summary>
                /// Populate Widgets
                /// </summary>
                public void PopulateWidgets()
                {
                        PopulateiFolderList();
                        UpdateWidgetSensitivity();
                }

                /// <summary>
                /// Populate iFolders List
                /// </summary>
                private void PopulateiFolderList()
                {
                        string str = Mono.Unix.UnixEnvironment.EffectiveUser.HomeDirectory;
                        if(!System.IO.Directory.Exists(str+"/.novell/ifolder"))
                                return;         // ifolder2.x is not present;
                        string[] dirs;
                        dirs = System.IO.Directory.GetDirectories(str+"/.novell/ifolder");
                        str = str+"/.novell/ifolder";
                        for(int i=0;i<dirs.Length;i++)
                        {
                                if(dirs[i] != str+"/reg" && dirs[i] != str+"/Save")
                                {
                                        TreeIter iter = AccTreeStore.AppendValues(dirs[i]);
                                        curDomains[i] = iter;
                                }
                        }
                }

                /// <summary>
                /// Update Widget Sensitivity
                /// </summary>
                private void UpdateWidgetSensitivity()
                {
                        if(curDomains.Count > 0)
                        {
                                TreeSelection tSelect = AccTreeView.Selection;
                                if( tSelect == null)
                                {
                                        MigrateButton.Sensitive                 = false;
                                        DetailsButton.Sensitive                 = true;
                                }
                                if(tSelect.CountSelectedRows() == 1)
                                {
                                        MigrateButton.Sensitive = true;
                                        DetailsButton.Sensitive = true;
                                }
                                else
                                {
                                        MigrateButton.Sensitive                 = false;
                                        DetailsButton.Sensitive                 = true;
                                }
                        }
                        else
                        {
                                MigrateButton.Sensitive = false;
                                DetailsButton.Sensitive = true;
                        }
                }

                /// <summary>
                /// Get Name
                /// </summary>
                /// <param name="path">Path</param>
                /// <returns>Name</returns>
                private static string GetName(string path)
                {
                        char[] seps={'/'};
                        string[] parts = path.Split(seps);
                        return parts[parts.Length-1];
                }

                private void ServerCellTextDataFunc (Gtk.TreeViewColumn tree_column,
                                Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
                                Gtk.TreeIter iter)
                {
                        string id = (string) tree_model.GetValue(iter, 0);
                        string uname = GetName(id);
//                        string loc = GetHomeLocation(id);
                        ((CellRendererText) cell).Text = uname;
                }

                /// <summary>
                /// Get Home Location
                /// </summary>
                /// <param name="path">Path</param>
                /// <returns>Location string</returns>
                private static string GetHomeLocation(string path)
                {
                        char[] seps={'/'};
                        string[] parts = path.Split(seps);
                        string userDir="";
                        int i;
                        for(i=0;i<parts.Length-1;i++)
                                userDir+="/"+parts[i];
                        userDir+="/reg/"+parts[i];
                        string homeLoc="";
                        if( System.IO.File.Exists(userDir+"/folderpath"))
                        {
                                StreamReader  reader = new StreamReader(userDir+"/folderpath");
                                homeLoc = reader.ReadLine();
                        }
                        return homeLoc;
                }

                /// <summary>
                /// Get Encryption Status
                /// </summary>
                /// <param name="path">Path</param>
                /// <returns>Encryption Status</returns>
                private string GetEncryptionStatus(string path)
                {
                        char[] seps={'/'};
                        string[] parts = path.Split(seps);
                        string userDir="";
                        int i;
                        for(i=0;i<parts.Length-1;i++)
                                userDir+="/"+parts[i];
                        userDir+="/reg/"+parts[i];
                        string homeLoc="";
                        if( System.IO.File.Exists(userDir+"/folderpath") && System.IO.File.Exists(userDir+"/encryptionstatus"))
                        {
                                StreamReader  reader = new StreamReader(userDir+"/encryptionstatus");
                                homeLoc = reader.ReadLine();
                        }
                        return homeLoc;
                }

		private void StatusCellTextDataFunc( Gtk.TreeViewColumn tree_column,
					Gtk.CellRenderer cell, Gtk.TreeModel tree_model,	
	                                Gtk.TreeIter iter)
		{
			string id = (string) tree_model.GetValue(iter, 0);
			string status = GetEncryptionStatus(id);
			if(status == null)
				status = Util.GS("Not encrypted");
			else if( status == "BLWF")
				status = Util.GS("Encrypted");
			else
				status = Util.GS("Not encrypted");
			((CellRendererText) cell).Text = status;
		}
                private void NameCellTextDataFunc (Gtk.TreeViewColumn tree_column,
                                Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
                                Gtk.TreeIter iter)
                {
                        string id = (string) tree_model.GetValue(iter, 0);
                        string HomeLocation = GetHomeLocation(id);
                        ((CellRendererText) cell).Text = HomeLocation;
                }


	//never used
        /// <summary>
        /// Event Handler for Cancel Button Clicked Event
        /// </summary>
/*        private void OnCancelClicked(object o, EventArgs args)
		{
			this.Destroy();
			this.MergePath = null;
			this.Cancelled = true;
		}*/

                /// <summary>
                /// Event Handler on MIgrate Account event
                /// </summary>
                private void OnMigrateAccount(object o, EventArgs args)
                {
                        TreeSelection tSelect = AccTreeView.Selection;

                        if(tSelect.CountSelectedRows() == 1)
                        {
                                TreeModel tModel;
                                TreeIter iter;

                                tSelect.GetSelected(out tModel, out iter);
                                string id = (string) tModel.GetValue(iter, 0);
/*				string status = GetEncryptionStatus(id);
				bool stat = false;;
				if( status == null)
					stat = false;
				else if( status == "BLWF")
					stat = true;
				else
					stat = false;*/
				String Location = GetHomeLocation(id);
				this.UserName = GetName( id);
				DirectoryInfo dir = new DirectoryInfo( Location );
				if( dir.Exists == true && dir.Name != this.iFolderName )
				{
					// Prompt for renaming...
					iFolderMsgDialog dlg = new iFolderMsgDialog( null, iFolderMsgDialog.DialogType.Info, iFolderMsgDialog.ButtonSet.YesNo,
                                                                                                Util.GS("Migration Alert"), Util.GS("The name of the iFolder on the server and on your local machine are different.") , Util.GS("Do you want to rename the iFolder on the server to the name of iFolder on your local machine?"));
					int res = dlg.Run();
					dlg.Hide();
					dlg.Destroy();
					if( res == (int)ResponseType.No)
					{
						this.MergePath = null;
						this.Cancelled = true;
					}
					else
					{
						// Move this to the name as that on the server...
						this.MergePath = System.IO.Path.Combine( dir.Parent.FullName, this.iFolderName );
						try
						{
							dir.MoveTo( this.MergePath);
						}
						catch(Exception ex)
						{
							iFolderMsgDialog dlg1 = new iFolderMsgDialog( null, iFolderMsgDialog.DialogType.Error, iFolderMsgDialog.ButtonSet.Ok, Util.GS("Migration Alert"), Util.GS("The folder cannot be renamed"), Util.GS("Error: "+ex.Message ));
							dlg1.Run();
							dlg1.Hide();
							dlg1.Destroy();
							this.MergePath = null;
						}
					}
				}
				else
					this.MergePath = GetHomeLocation(id);
				
                        }
                }

		//not been used
             /*   private void OnAccTreeRowActivated(object o, RowActivatedArgs args)
                {
                        OnDetailsClicked(o, args);
                } */

                public void AccSelectionChangedHandler(object o, EventArgs args)
                {
                        UpdateWidgetSensitivity();
                }

		//not doing anything
                /// <summary>
                /// Event Handler for Details Clicked Event
                /// </summary>
                /*private void OnDetailsClicked(object o, EventArgs args)
                {
                        return;
                }*/
                
                /// <summary>
                /// Remove Item
                /// </summary>
                public void RemoveItem()
                {
                        TreeSelection tSelect = this.AccTreeView.Selection;
                        TreeModel tModel;
                        TreeIter iter;

                        tSelect.GetSelected(out tModel, out iter);
                        string id = (string) tModel.GetValue(iter, 0);

                        curDomains.Remove(id);
                        AccTreeStore.Remove(ref iter);
                }

        /// <summary>
        /// Remove from Registry
        /// </summary>
        /// <param name="UserName">User Name</param>
		public static void RemoveFromRegistry(string UserName)
		{
			string str = Mono.Unix.UnixEnvironment.EffectiveUser.HomeDirectory;
			try
			{
			//	Console.WriteLine("Removing dir: {0}", str+"/.novell/ifolder/"+UserName);	
			//	Console.WriteLine("Removing dir: {0}", str+"/.novell/ifolder/reg/"+UserName );
			//	return;
				if(System.IO.Directory.Exists(str+"/.novell/ifolder/"+UserName))
			        	System.IO.Directory.Delete(str+"/.novell/ifolder/"+UserName, true);
				if(System.IO.Directory.Exists(str+"/.novell/ifolder/reg/"+UserName))
				        System.IO.Directory.Delete(str+"/.novell/ifolder/reg/"+UserName, true);
			}
			catch(Exception)
			{
			}
		}

        /// <summary>
        /// Can be Migrated
        /// </summary>
        /// <param name="ifolderName">iFolder Name</param>
        /// <returns>true if it can be Migrated</returns>
		public static bool CanBeMigrated( String ifolderName )
		{
                        string str = Mono.Unix.UnixEnvironment.EffectiveUser.HomeDirectory;
                        if(!System.IO.Directory.Exists(str+"/.novell/ifolder"))
                                return false;         // ifolder2.x is not present;
                        string[] dirs;
                        dirs = System.IO.Directory.GetDirectories(str+"/.novell/ifolder");
                        str = str+"/.novell/ifolder";
                        for(int i=0;i<dirs.Length;i++)
                        {
                                if(dirs[i] != str+"/reg" && dirs[i] != str+"/Save")
                                {
//					String UserName = GetName( dirs[i] );
					String HomeLoc = GetHomeLocation( dirs[i] );
					DirectoryInfo dInfo = new DirectoryInfo( HomeLoc );
					if( dInfo != null)
					{
						return true;
					}
                                }
                        }
			return false;
		}

	}
}
