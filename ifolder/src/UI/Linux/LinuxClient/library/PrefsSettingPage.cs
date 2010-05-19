/*****************************************************************************
*
* Copyright (c) [2010] Novell, Inc.
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
  *                 $Author: Vikash Mehta <mvikash@novell.com>
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
	/// A VBox with the ability to create and manage ifolder voilation
	/// </summary>
	public class PrefsSettingPage: VBox
	{

		private Gtk.CheckButton         NotifyCheckButton;	
		private Gtk.TreeView            PolicyTreeView;
		private Gtk.ListStore           PolicyTreeStore;


		private string[] policyVoilation = {
                        "When quota policy is violated",
                        "When file size policy is violated",
                        "When file exclusion policy is violated",
                        "When disk is full",
                        "When required permissions are unavailable",
                        "When file path exceeds optimal limit"
		 };	

		private enum policyTypes{
	  		QuotaViolation,
			FileSizeViolation,
			FileExclusionViolation,
			DiskFull,
			PermissionUnavailable,
			ExceedsPathSize
		 };
		/// <summary>
		/// Default constructor for iFolderAccountsPage
		/// </summary>
		public PrefsSettingPage( Gtk.Window topWindow )
			: base()
		{
                        InitializeWidgets();
                        this.Realized += new EventHandler(OnRealizeWidget);

		}

        	/// <summary>
	        /// Destructor
        	/// </summary>
		~PrefsSettingPage()
		{
		}

        	/// <summary>
	        /// Initialize Widgets
        	/// </summary>
		private void InitializeWidgets()
		{
			//this.Spacing = 10;
			//this.BorderWidth = 10;
			this.Spacing = Util.SectionSpacing;
                        this.BorderWidth = Util.DefaultBorderWidth;
			NotifyCheckButton = new CheckButton(Util.GS("Display _Notification"));
                        this.PackStart(NotifyCheckButton, false, false, 0);

                        ScrolledWindow sw = new ScrolledWindow();
                        sw.ShadowType = Gtk.ShadowType.None;
                        PolicyTreeView = new TreeView();
                        sw.Add(PolicyTreeView);
                        PolicyTreeView.HeadersVisible = false;
			
			PolicyTreeView.Selection.Mode = SelectionMode.Multiple;

                        // Set up the iFolder TreeView
                        PolicyTreeStore = new ListStore(typeof(string));
                        PolicyTreeView.Model = PolicyTreeStore;
			
			this.PackStart(sw, false, false, 0);

                        CellRendererText logcr = new CellRendererText();
                        logcr.Xpad = 10;
                        PolicyTreeView.AppendColumn(Util.GS("Log"), logcr, "text", 0);
			
		}
	

        	/// <summary>
	        /// Polpuate Widgets
        	/// </summary>
                private void OnRealizeWidget(object o, EventArgs args)
                {
                        PopulateWidgets();
                }

                /// <summary>
                /// Set the Values in the Widgets
                /// </summary>
                private void PopulateWidgets()
		{

			TreePath path = null;
			string indexToSelect = null;

			//Add Policy Voilation
			AddMessage();

			//Read registry and select row
			if((bool)ClientConfig.Get(ClientConfig.KEY_SHOW_QUOTA_VIOLATION))
			{
				indexToSelect = ((int)policyTypes.QuotaViolation).ToString();
				path = new TreePath(indexToSelect);
				PolicyTreeView.Selection.SelectPath(path);	
			}
			if((bool)ClientConfig.Get(ClientConfig.KEY_SHOW_FILE_SIZE_VOILATION))
			{
				indexToSelect = ((int)policyTypes.FileSizeViolation).ToString();
				path = new TreePath(indexToSelect);
				PolicyTreeView.Selection.SelectPath(path);	
			}
			if((bool)ClientConfig.Get(ClientConfig.KEY_SHOW_EXCLUSION_VOILATION))
			{
				indexToSelect = ((int)policyTypes.FileExclusionViolation).ToString();
				path = new TreePath(indexToSelect);
				PolicyTreeView.Selection.SelectPath(path);	
			}
			if((bool)ClientConfig.Get(ClientConfig.KEY_SHOW_DISK_FULL))
			{
				indexToSelect = ((int)policyTypes.DiskFull).ToString();
				path = new TreePath(indexToSelect);
				PolicyTreeView.Selection.SelectPath(path);	
			}
			if((bool)ClientConfig.Get(ClientConfig.KEY_SHOW_PERMISSION_UNAVAILABLE))
			{
				indexToSelect = ((int)policyTypes.PermissionUnavailable).ToString();
				path = new TreePath(indexToSelect);
				PolicyTreeView.Selection.SelectPath(path);	
			}
			if((bool)ClientConfig.Get(ClientConfig.KEY_SHOW_EXCEEDS_PATH_SIZE))
			{
				indexToSelect = ((int)policyTypes.ExceedsPathSize).ToString();
				path = new TreePath(indexToSelect);
				PolicyTreeView.Selection.SelectPath(path);	
			}

		}
	

        	/// <summary>
	        /// Get Selected Rows and clear All Registry and Set Registry for Selected Rows.
        	/// </summary>
		public void GetSelectedRow()
		{
			string rowNumber = null;
			int index = 0;
		
			//Update registry entry only when Display Notification checkbox is checked	
			if(!NotifyCheckButton.Active)
			{
				return;
			}

			//Unselect All registry entry
			ClientConfig.Set(ClientConfig.KEY_SHOW_QUOTA_VIOLATION , false);
			ClientConfig.Set(ClientConfig.KEY_SHOW_FILE_SIZE_VOILATION , false);
			ClientConfig.Set(ClientConfig.KEY_SHOW_EXCLUSION_VOILATION , false);
			ClientConfig.Set(ClientConfig.KEY_SHOW_DISK_FULL , false);
			ClientConfig.Set(ClientConfig.KEY_SHOW_PERMISSION_UNAVAILABLE , false);
			ClientConfig.Set(ClientConfig.KEY_SHOW_EXCEEDS_PATH_SIZE , false);
			
			//TODO: Move this code to close button handler
		  	Gtk.TreePath[] paths = PolicyTreeView.Selection.GetSelectedRows ();
                        for (int n=0; n<paths.Length; n++) {

				//Convert paths to integer	
				rowNumber = paths[n].ToString();
				index = int.Parse(rowNumber);

				//Switch case to set each selected row registry entry to TRUE
				switch(index)
				{
					case (int)policyTypes.QuotaViolation:
						ClientConfig.Set(ClientConfig.KEY_SHOW_QUOTA_VIOLATION , true);
					break;
					case (int)policyTypes.FileSizeViolation:
						ClientConfig.Set(ClientConfig.KEY_SHOW_FILE_SIZE_VOILATION , true);
					break;
					case (int)policyTypes.FileExclusionViolation:
						ClientConfig.Set(ClientConfig.KEY_SHOW_EXCLUSION_VOILATION , true);
					break;
					case (int)policyTypes.DiskFull:
						ClientConfig.Set(ClientConfig.KEY_SHOW_DISK_FULL , true);
					break;
					case (int)policyTypes.PermissionUnavailable:
						ClientConfig.Set(ClientConfig.KEY_SHOW_PERMISSION_UNAVAILABLE , true);
					break;
					case (int)policyTypes.ExceedsPathSize:
						ClientConfig.Set(ClientConfig.KEY_SHOW_EXCEEDS_PATH_SIZE , true);
					break;
					default:
					break;
				}
			
                        }
		}

        	/// <summary>
	        /// AddMessage to PolicyListStore
        	/// </summary>
               	private void AddMessage()
                {
			foreach(string str in policyVoilation)
			{
				PolicyTreeStore.AppendValues(string.Format( "{0}", Util.GS(str) ) );
			}
                }

	
	}
}
