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
	public class NotificationPrefsBox: VBox
	{
		private Gtk.TreeView            PolicyTreeView;
		private Gtk.ListStore           PolicyTreeStore;


		//Note : Maintain sequence between notificationTypes and notificationDescription
		private string[] notificationDescription = {
                        "When quota policy is violated",
                        "When file size policy is violated",
                        "When file exclusion policy is violated",
                        "When disk is full",
                        "When required permissions are unavailable",
                        "When file path exceeds optimal limit",
			"When iFolders are shared",
			"When conflicts arise"
		 };	

		private enum notificationTypes{
	  		QuotaViolation,
			FileSizeViolation,
			FileExclusionViolation,
			DiskFull,
			PermissionUnavailable,
			ExceedsPathSize,
			NewiFolderShared,
			Conflicts
		 };
		/// <summary>
		/// Default constructor for iFolderAccountsPage
		/// </summary>
		public NotificationPrefsBox( Gtk.Window topWindow )
			: base()
		{
                        InitializeWidgets();
                        this.Realized += new EventHandler(OnRealizeWidget);

		}

        	/// <summary>
	        /// Destructor
        	/// </summary>
		~NotificationPrefsBox()
		{
		}

        	/// <summary>
	        /// Initialize Widgets
        	/// </summary>
		private void InitializeWidgets()
		{
			this.Spacing = Util.SectionSpacing;
                        this.BorderWidth = Util.DefaultBorderWidth;

                        ScrolledWindow sw = new ScrolledWindow();
                        sw.ShadowType = Gtk.ShadowType.None;

                        // Model for Tree View. Enabled & Description
                        PolicyTreeStore = new ListStore(typeof(bool), typeof (string), typeof (notificationTypes));

			//TreeView with Two columns
                        PolicyTreeView = new TreeView(PolicyTreeStore);
			PolicyTreeView.Selection.Mode = SelectionMode.Single;
                        PolicyTreeView.HeadersVisible = true;
                        sw.Add(PolicyTreeView);

			//Toggle column - Whether notification is enabled or not
			CellRendererToggle toggleCell = new CellRendererToggle ();
			toggleCell.Activatable = true;
			toggleCell.Mode = CellRendererMode.Activatable;
			toggleCell.Toggled += preferenceToggled;
			PolicyTreeView.AppendColumn (Util.GS("Enabled"), toggleCell, "active", 0);

			//Description of the notification event
			CellRendererText textCell = new CellRendererText ();
			PolicyTreeView.AppendColumn (Util.GS("Notification"), textCell, "text", 1);

			this.PackStart(sw, true, true, 0);
		}

		private void preferenceToggled (object o, ToggledArgs args)
		{
			TreeIter iter;

			if (PolicyTreeStore.GetIter (out iter, new TreePath(args.Path))) {
				bool old = (bool) PolicyTreeStore.GetValue (iter, 0);
				notificationTypes type = (notificationTypes) PolicyTreeStore.GetValue (iter, 2);
				setNotificationState (type, !old);
				PolicyTreeStore.SetValue(iter, 0, !old);
			}
		}

        	/// <summary>
	        /// Polpuate Widgets
        	/// </summary>
                private void OnRealizeWidget(object o, EventArgs args)
                {
			populateNotificationsModel();
                }

                /// <summary>
                /// Set the Values in the Widgets
                /// </summary>
                private void populateNotificationsModel()
		{
			foreach (notificationTypes type in (notificationTypes[])Enum.GetValues (typeof(notificationTypes)))
				PolicyTreeStore.AppendValues (getNotificationState (type), Util.GS(notificationDescription[(int)type]), type);
		}
	
		private void setNotificationState (notificationTypes type, bool enabled)
		{
			switch(type)
			{
			case notificationTypes.QuotaViolation:
				ClientConfig.Set(ClientConfig.KEY_SHOW_QUOTA_VIOLATION, enabled);
				return;
			case notificationTypes.FileSizeViolation:
				ClientConfig.Set(ClientConfig.KEY_SHOW_FILE_SIZE_VOILATION, enabled);
				return;
			case notificationTypes.FileExclusionViolation:
				ClientConfig.Set(ClientConfig.KEY_SHOW_EXCLUSION_VOILATION, enabled);
				return;
			case notificationTypes.DiskFull:
				ClientConfig.Set(ClientConfig.KEY_SHOW_DISK_FULL, enabled);
				return;
			case notificationTypes.PermissionUnavailable:
				ClientConfig.Set(ClientConfig.KEY_SHOW_PERMISSION_UNAVAILABLE, enabled);
				return;
			case notificationTypes.ExceedsPathSize:
				ClientConfig.Set(ClientConfig.KEY_SHOW_EXCEEDS_PATH_SIZE, enabled);
				return;
			case notificationTypes.NewiFolderShared:
				ClientConfig.Set(ClientConfig.KEY_NOTIFY_IFOLDERS, enabled);
				return;
			case notificationTypes.Conflicts:
				ClientConfig.Set(ClientConfig.KEY_NOTIFY_COLLISIONS, enabled);
				return;
			default:
				return;
			}
		}

		private bool getNotificationState (notificationTypes type)
		{
			switch(type)
			{
			case notificationTypes.QuotaViolation:
				return (bool)ClientConfig.Get(ClientConfig.KEY_SHOW_QUOTA_VIOLATION);
			case notificationTypes.FileSizeViolation:
				return (bool)ClientConfig.Get(ClientConfig.KEY_SHOW_FILE_SIZE_VOILATION);
			case notificationTypes.FileExclusionViolation:
				return (bool)ClientConfig.Get(ClientConfig.KEY_SHOW_EXCLUSION_VOILATION);
			case notificationTypes.DiskFull:
				return (bool)ClientConfig.Get(ClientConfig.KEY_SHOW_DISK_FULL);
			case notificationTypes.PermissionUnavailable:
				return (bool)ClientConfig.Get(ClientConfig.KEY_SHOW_PERMISSION_UNAVAILABLE);
			case notificationTypes.ExceedsPathSize:
				return (bool)ClientConfig.Get(ClientConfig.KEY_SHOW_EXCEEDS_PATH_SIZE);
			case notificationTypes.NewiFolderShared:
				return (bool) ClientConfig.Get(ClientConfig.KEY_NOTIFY_IFOLDERS);
			case notificationTypes.Conflicts:
				return (bool) ClientConfig.Get(ClientConfig.KEY_NOTIFY_COLLISIONS);
			default:
				return true;
			}
		}
	}
};
