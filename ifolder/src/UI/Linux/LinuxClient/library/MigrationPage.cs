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
using Simias.Client.Event;
using Simias.Client;
using Simias.Client.Authentication;

using Novell.iFolder.Events;
using Novell.iFolder.Controller;
using Novell.iFolder.DomainProvider;

namespace Novell.iFolder
{
	/// <summary>
	/// A VBox with the ability to create and manage ifolder accounts
	/// </summary>
	public class MigrationPage : VBox
	{
		private Gtk.Window				topLevelWindow;
//		private iFolderWebService 		ifws;
		private iFolderTreeView		AccTreeView;
		private ListStore			AccTreeStore;
//		private Button 				AddButton;
		private Button				MigrateButton;
		private Button				DetailsButton;
		private Hashtable curDomains;
		
		/// <summary>
		/// Default constructor for iFolderAccountsPage
		/// </summary>
		
		public MigrationPage( Gtk.Window topWindow, iFolderWebService ifws )
			: base()
		{
			this.topLevelWindow = topWindow;
//			this.ifws = ifws;
			curDomains = new Hashtable();
			
			InitializeWidgets();

			this.Realized += new EventHandler(OnRealizeWidget);
		}
		
        /// <summary>
        /// Destructor
        /// </summary>
		~MigrationPage()
		{
		}

		/// <summary>
		/// Set up the widgets
		/// </summary>
		/// <returns>
		/// Widget to display
		/// </returns>
		private void InitializeWidgets()
		{
			this.Spacing = 10;
			this.BorderWidth = 10;
			
			// Set up the iFolder2.x Accounts tree view in a scrolled window
			AccTreeView = new iFolderTreeView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			sw.Add(AccTreeView);
			this.PackStart(sw, true, true, 0);

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
			nameColumn.MinWidth = 150;
			AccTreeView.AppendColumn(nameColumn);

			AccTreeView.Selection.Mode = SelectionMode.Single;
			AccTreeView.Selection.Changed +=
				new EventHandler(AccSelectionChangedHandler);

			// Status column
			TreeViewColumn statusColumn = new TreeViewColumn();
			statusColumn.Title = Util.GS("Status");
			CellRendererText stat = new CellRendererText();
			stat.Xpad = 5;
			statusColumn.PackStart(stat, false);
			statusColumn.SetCellDataFunc(stat,new TreeCellDataFunc(statusCellTextDataFunc));
			statusColumn.Resizable = true;
			statusColumn.MinWidth = 150;
			AccTreeView.AppendColumn(statusColumn);
			
			// Set up buttons for add/remove/accept/decline
			HButtonBox buttonBox = new HButtonBox();
			buttonBox.Spacing = 10;
			buttonBox.Layout = ButtonBoxStyle.End;
			this.PackStart(buttonBox, false, false, 0);

			//AddButton = new Button("Add");
//			buttonBox.PackStart(AddButton);

			MigrateButton = new Button("_Migrate");
			buttonBox.PackStart(MigrateButton);
			MigrateButton.Clicked += new EventHandler(OnMigrateAccount);

			DetailsButton = new Button(Gtk.Stock.Properties);
			buttonBox.PackStart(DetailsButton);
			DetailsButton.Clicked += new EventHandler(OnDetailsClicked);

			AccTreeView.RowActivated += new RowActivatedHandler(
						OnAccTreeRowActivated);
		}




		/// <summary>
		/// Set the Values in the Widgets
		/// </summary>
		public void PopulateWidgets()
		{
			PopulateiFolderList();
			UpdateWidgetSensitivity();
		}

        /// <summary>
        /// Populate iFolderList
        /// </summary>
		private void PopulateiFolderList()
		{
			string str = Mono.Unix.UnixEnvironment.EffectiveUser.HomeDirectory;
			if(!System.IO.Directory.Exists(str+"/.novell/ifolder"))
				return;		// ifolder2.x is not present;
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
			/*
			if(curDomains.Count == 0)
			{
				this.Hide();
				this.Destroy();
			}
			*/
		}

        /// <summary>
        /// Event Handler for Realize Widget
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
		private void OnRealizeWidget(object o, EventArgs args)
		{
			PopulateWidgets();
		}

        /// <summary>
        /// Get Name
        /// </summary>
        private string GetName(string path)
		{
			char[] seps={'/'};
			string[] parts = path.Split(seps);
			Debug.PrintLine(parts[parts.Length-1]);
			return parts[parts.Length-1];
		}

		private void ServerCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			string id = (string) tree_model.GetValue(iter, 0);
			string uname = GetName(id);
//			string loc = GetHomeLocation(id);
			((CellRendererText) cell).Text = uname;
		}


        /// <summary>
        /// Get Home Location
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

		private void statusCellTextDataFunc(Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			string id = (string) tree_model.GetValue(iter, 0);
			string HomeLocation = GetHomeLocation(id);
//			if( HomeLocation == "")
//			{
//				AccTreeStore.Remove(ref iter);
//				return;
//			}
			try
			{
				System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(HomeLocation);
			
			if(d.Exists)
				((CellRendererText) cell).Text = "Exists";
			else
				((CellRendererText) cell).Text = "Folder does not Exist";
			}
			catch(Exception)
			{
			}


		}

		private void NameCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			string id = (string) tree_model.GetValue(iter, 0);
			string HomeLocation = GetHomeLocation(id);
			((CellRendererText) cell).Text = HomeLocation;
		}



		private void OnMigrateAccount(object o, EventArgs args)
		{
			TreeSelection tSelect = AccTreeView.Selection;

			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
//				string id = (string) tModel.GetValue(iter, 0);
				MigrationWizard migratewiz = null; //new MigrationWizard(GetName(id), GetHomeLocation(id), ifws, this);

				migratewiz.TransientFor = topLevelWindow;
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
				/*
					if(curDomains.Count == 0)
					{
						this.Hide();
						this.Destroy();
					}
				*/

			}
		}


		private void OnAccTreeRowActivated(object o, RowActivatedArgs args)
		{
			OnDetailsClicked(o, args);
		}


		private void OnDetailsClicked(object o, EventArgs args)
		{
			return;
		}

	/*	private void OnAccountDialogDestroyedEvent(object o, EventArgs args)
		{
		}*/

		public void AccSelectionChangedHandler(object o, EventArgs args)
		{
			UpdateWidgetSensitivity();
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
					MigrateButton.Sensitive			= false;
					DetailsButton.Sensitive			= false;
				}	
				if(tSelect.CountSelectedRows() == 1)
				{
					MigrateButton.Sensitive = true;
					DetailsButton.Sensitive = true;
				}
				else
				{
					// Nothing is selected
					//AddButton.Sensitive			= false;
					MigrateButton.Sensitive			= false;
					DetailsButton.Sensitive			= false;
				}
			}
			else
			{
				MigrateButton.Sensitive = false;
				DetailsButton.Sensitive = false;
			}
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
			/*
			if(curDomains.Count == 0)
			{
				this.Hide();
				this.Destroy();
			}
			*/

		}
	
		
	}
}
