/***********************************************************************
 *  $RCSfile: ClientConfig.cs,v $
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
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Authors:
 *		Calvin Gaisford <cgaisford@novell.com>
 *		Bruce Getter <bgetter@novell.com>
 *		Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/
	 
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Threading;

namespace Novell.iFolder
{
	/// <summary>
	/// Configuration class for simias components.
	/// </summary>
	public class ClientConfig
	{
		#region Class Members
		///
		/// /apps/ifolder/notification
		///
		public const string KEY_SHOW_CREATION = "/apps/ifolder/notification/show_created_dialog";
		public const string KEY_NOTIFY_IFOLDERS = "/apps/ifolder/notification/new_ifolders";
		public const string KEY_NOTIFY_COLLISIONS = "/apps/ifolder/notification/collisions";
		public const string KEY_NOTIFY_USERS = "/apps/ifolder/notification/new_users";
//		public static string KEY_NOTIFY_SYNC_ERRORS = "/apps/ifolder/notification/sync_errors";

		///
		/// /apps/ifolder/synchronization
		///
		// Valid values are "Seconds", "Minutes", "Hours", and "Days"
		public const string KEY_SYNC_UNIT = "/apps/ifolder/synchronization/unit";

		///
		/// /apps/ifolder/ui/main_window
		///		
		public const string KEY_IFOLDER_WINDOW_X_POS	= "/apps/ifolder/ui/main_window/left";
		public const string KEY_IFOLDER_WINDOW_Y_POS	= "/apps/ifolder/ui/main_window/top";
		public const string KEY_IFOLDER_WINDOW_WIDTH	= "/apps/ifolder/ui/main_window/width";
		public const string KEY_IFOLDER_WINDOW_HEIGHT	= "/apps/ifolder/ui/main_window/height";
		public const string KEY_IFOLDER_WINDOW_VISIBLE = "/apps/ifolder/ui/main_window/visible";
		public const string KEY_SHOW_SERVER_IFOLDERS	= "/apps/ifolder/ui/main_window/show_available_ifolders";

		private static GConf.Client				client = null;
		private static GConf.NotifyEventHandler	SettingChangedHandler;
		
		#endregion

		#region Properties
		public static GConf.Client Client
		{
			get
			{
				if (client == null)
				{
					client = new GConf.Client();
					
					SettingChangedHandler =
						new GConf.NotifyEventHandler(OnSettingChanged);
					client.AddNotify("/apps/ifolder", SettingChangedHandler);
				}
				
				return client;
			}
		}
		
		#endregion

		#region Events		
		public static event GConf.NotifyEventHandler SettingChanged;
		#endregion
		
		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		static ClientConfig()
		{
		}
		#endregion

		#region Private Methods
		private static object GetDefault(string key)
		{
			switch(key)
			{
				case KEY_SHOW_CREATION:
				case KEY_NOTIFY_IFOLDERS:
				case KEY_NOTIFY_COLLISIONS:
				case KEY_NOTIFY_USERS:
//				case KEY_NOTIFY_SYNC_ERRORS:
				case KEY_SHOW_SERVER_IFOLDERS:
				case KEY_IFOLDER_WINDOW_VISIBLE:
					return true;

				case KEY_SYNC_UNIT:
					return "Minutes";

				case KEY_IFOLDER_WINDOW_X_POS:
				case KEY_IFOLDER_WINDOW_Y_POS:
					return 0;
				case KEY_IFOLDER_WINDOW_WIDTH:
					return 640;
				case KEY_IFOLDER_WINDOW_HEIGHT:
					return 480;
			}
			
			return null;
		}
		
		private static void OnSettingChanged(object sender, GConf.NotifyEventArgs args)
		{
			if (SettingChanged != null)
				SettingChanged(sender, args);
		}
		#endregion

		#region Public Methods
		public static object Get(string key)
		{
			try
			{
				return Client.Get(key);
			}
			catch (GConf.NoSuchKeyException)
			{
				object defaultValue = GetDefault(key);
				
				if (defaultValue != null)
					Client.Set(key, defaultValue);
				
				return defaultValue;
			}
		}
		
		public static void Set(string key, object value)
		{
			Client.Set(key, value);
		}
		
		/// <summary>
		/// Checks for existence of a specified key.
		/// </summary>
		/// <param name="key">The key to check for existence.</param>
		/// <returns>True if the key exists, otherwise false.</returns>
		public static bool Exists(string key)
		{
			try
			{
				Client.Get(key);
			}
			catch (GConf.NoSuchKeyException)
			{
				return false;
			}
			
			return true;
		}

		/// <summary>
		/// Deletes the specified key from the default section.
		/// </summary>
		/// <param name="key">Key to delete.</param>
		public static void DeleteKey(string key)
		{
			try
			{
				Client.Set(key, null);
			}
			catch{}
		}

		#endregion
	}
}


