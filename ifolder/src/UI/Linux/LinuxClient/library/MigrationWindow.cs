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
	public class MigrationWindow : Window
	{
//              private Gtk.Window                              topLevelWindow;
                private iFolderWebService               ifws;
		private SimiasWebService 		simws;
                private iFolderTreeView         AccTreeView;
                private ListStore                       AccTreeStore;
                private Button                          MigrateButton;
                private Button                          DetailsButton;
                private Button                          HelpButton;
                private Hashtable curDomains;

		/// <summary>
		/// Default constructor for LogWindow
		/// </summary>
		public MigrationWindow( Gtk.Window topWindow, iFolderWebService ifws, SimiasWebService simws)
			: base (Util.GS("iFolder Migration"))
		{
//                      this.topLevelWindow = topWindow;
			this.Modal = false;
                        this.ifws = ifws;
			this.simws = simws;
                        curDomains = new Hashtable();
			CreateWidgets();
			PopulateWidgets();
		}

        /// <summary>
        /// Destructor
        /// </summary>
		~MigrationWindow()
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

			VBox vbox = new VBox (false, 0);
			this.Add (vbox);

                        vbox.Spacing = 10;
                        vbox.BorderWidth = 10;

                        // Set up the iFolder2.x Accounts tree view in a scrolled window
                        AccTreeView = new iFolderTreeView();
                        ScrolledWindow sw = new ScrolledWindow();
                        sw.ShadowType = Gtk.ShadowType.EtchedIn;
                        sw.Add(AccTreeView);
                        vbox.PackStart(sw, true, true, 0);

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

			HBox buttonBox = new HBox(false, 0);
			vbox.PackStart(buttonBox,false,false,0);
			
			HelpButton = new Button(Gtk.Stock.Help);
			buttonBox.PackStart(HelpButton,false,false,0);
			this.HelpButton.Clicked += new EventHandler(OnHelpClicked);

			DetailsButton = new Button(Gtk.Stock.Cancel);
                        buttonBox.PackEnd(DetailsButton,false,false,0);
                        this.DetailsButton.Clicked += new EventHandler(OnCancelClicked);

			MigrateButton = new Button("_Migrate");
                        buttonBox.PackEnd(MigrateButton,false,false,2);
                        MigrateButton.Clicked += new EventHandler(OnMigrateAccount);
				
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
        /// Populate iFolder List
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
                private string GetName(string path)
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
//                      string loc = GetHomeLocation(id);
                        ((CellRendererText) cell).Text = uname;
                }

                /// <summary>
                /// Get Hoem Location
                /// </summary>
                /// <param name="path">Path</param>
                /// <returns>Location string</returns>
                private string GetHomeLocation(string path)
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
			/// Fix for #327650
			try
			{
                        	if( System.IO.File.Exists(userDir+"/folderpath"))
                        	{
                                	StreamReader  reader = new StreamReader(userDir+"/encryptionstatus");
                                	homeLoc = reader.ReadLine();
                        	}
			}
			catch
			{
				homeLoc = null;
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

        /// <summary>
        /// Event Handler for Cancel Button Clicked
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
		private void OnCancelClicked(object o, EventArgs args)
		{
			this.Destroy();
		}

                private void OnMigrateAccount(object o, EventArgs args)
                {
                        TreeSelection tSelect = AccTreeView.Selection;

                        if(tSelect.CountSelectedRows() == 1)
                        {
                                TreeModel tModel;
                                TreeIter iter;

                                tSelect.GetSelected(out tModel, out iter);
                                string id = (string) tModel.GetValue(iter, 0);
				string status = GetEncryptionStatus(id);
				bool stat = false;;
				if( status == null)
					stat = false;
				else if( status == "BLWF")
					stat = true;
				else
					stat = false;
                                MigrationWizard migratewiz = new MigrationWizard(GetName(id), GetHomeLocation(id), ifws, simws, this, stat);

                                // migratewiz.TransientFor = topLevelWindow;
                                if (!Util.RegisterModalWindow(migratewiz))
                                {
                                        try
                                        {
                                                Util.CurrentModalWindow.Present();
                                        }
                                        catch{}
                                        migratewiz.Destroy();
                                        return;
                                }
                                migratewiz.ShowAll();
                        }
                }


                private void OnAccTreeRowActivated(object o, RowActivatedArgs args)
                {
                        OnDetailsClicked(o, args);
                }


                public void AccSelectionChangedHandler(object o, EventArgs args)
                {
                        UpdateWidgetSensitivity();
                }


                private void OnDetailsClicked(object o, EventArgs args)
                {
                        return;
                }
                
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
		/// Event Handler for Help Button Clicked
		/// </summary>
		/// <param name="o"></param>
		/// <param name="args"></param>
		private void OnHelpClicked(object o, EventArgs args)
                {
			Util.ShowHelp("migration.html", this);
                }


	}
}
