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
  *							 Bruce Getter <bgetter@novell.com>
  *                          Boyd Timothy <btimothy@novell.com>
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
		/// /apps/ifolder3/notification
		///
		public const string KEY_SHOW_CREATION = "/apps/ifolder3/notification/show_created_dialog";
		public const string KEY_NOTIFY_IFOLDERS = "/apps/ifolder3/notification/new_ifolders";
		public const string KEY_NOTIFY_POLICY_VOILATION = "/apps/ifolder3/notification/policy_violation";
		public const string KEY_NOTIFY_COLLISIONS = "/apps/ifolder3/notification/collisions";
		public const string KEY_NOTIFY_USERS = "/apps/ifolder3/notification/new_users";
//		public static string KEY_NOTIFY_SYNC_ERRORS = "/apps/ifolder3/notification/sync_errors";
		public const string KEY_NOTIFY_MIGRATION_2_X = "/apps/ifolder3/notification/migrate2x";
//		public const string KEY_SHOW_NETWORK_ERRORS = "/apps/ifolder3/notification/network";
		public const string KEY_SHOW_SYNC_LOG = "/apps/ifolder3/notification/synclog";

		///
		/// /apps/ifolder3/synchronization
		///
		// Valid values are "Seconds", "Minutes", "Hours", and "Days"
		public const string KEY_SYNC_UNIT = "/apps/ifolder3/synchronization/unit";

		///
		/// /apps/ifolder3/ui/main_window
		///		
		public const string KEY_IFOLDER_WINDOW_X_POS		= "/apps/ifolder3/ui/main_window/left";
		public const string KEY_IFOLDER_WINDOW_Y_POS		= "/apps/ifolder3/ui/main_window/top";
		public const string KEY_IFOLDER_WINDOW_WIDTH		= "/apps/ifolder3/ui/main_window/width";
		public const string KEY_IFOLDER_WINDOW_HEIGHT		= "/apps/ifolder3/ui/main_window/height";
		public const string KEY_IFOLDER_WINDOW_VISIBLE	    = "/apps/ifolder3/ui/main_window/visible";
		public const string KEY_IFOLDER_WINDOW_HIDE	        = "/apps/ifolder3/ui/main_window/hide";
		public const string KEY_SHOW_SERVER_IFOLDERS		= "/apps/ifolder3/ui/main_window/show_available_ifolders";
		
		///
		/// /apps/ifolder3/account
		///
		public const string KEY_IFOLDER_ACCOUNT_PREFILL			= "/apps/ifolder3/account/prefill";
		public const string KEY_IFOLDER_ACCOUNT_SERVER_ADDRESS	= "/apps/ifolder3/account/server_address";
		public const string KEY_IFOLDER_ACCOUNT_USER_NAME			= "/apps/ifolder3/account/user_name";
		public const string KEY_IFOLDER_ACCOUNT_PASSWORD			= "/apps/ifolder3/account/clear_text_password_for_testing_only";
		public const string KEY_IFOLDER_ACCOUNT_REMEMBER_PASSWORD= "/apps/ifolder3/account/remember_password";
		
		///
		/// /apps/ifolder3/debug
		///
		public const string KEY_IFOLDER_DEBUG_COLOR_PALETTE		= "/apps/ifolder3/debug/color_palette";
		public const string KEY_IFOLDER_DEBUG_IFOLDER_DATA		= "/apps/ifolder3/debug/ifolder_data";
		public const string KEY_IFOLDER_DEBUG_PRINT_SIMIAS_EVENT_ERRORS = "/apps/ifolder3/debug/print_simias_event_errors";

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
					client.AddNotify("/apps/ifolder3", SettingChangedHandler);
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
//				case KEY_SHOW_NETWORK_ERRORS:
					return true;

				case KEY_IFOLDER_WINDOW_HIDE:
				case KEY_SHOW_SYNC_LOG:
				case KEY_NOTIFY_POLICY_VOILATION:		
					return false;

				case KEY_SYNC_UNIT:
					return "Minutes";

				case KEY_IFOLDER_WINDOW_X_POS:
				case KEY_IFOLDER_WINDOW_Y_POS:
					return 0;
				case KEY_IFOLDER_WINDOW_WIDTH:
					return 640;
				case KEY_IFOLDER_WINDOW_HEIGHT:
					return 480;

				case KEY_IFOLDER_ACCOUNT_SERVER_ADDRESS:
				case KEY_IFOLDER_ACCOUNT_USER_NAME:
				case KEY_IFOLDER_ACCOUNT_PASSWORD:
					return "";

				case KEY_IFOLDER_ACCOUNT_PREFILL:
				case KEY_IFOLDER_ACCOUNT_REMEMBER_PASSWORD:
					return false;
				
				case KEY_IFOLDER_DEBUG_COLOR_PALETTE:
				case KEY_IFOLDER_DEBUG_IFOLDER_DATA:
				case KEY_IFOLDER_DEBUG_PRINT_SIMIAS_EVENT_ERRORS:
					return false;
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


