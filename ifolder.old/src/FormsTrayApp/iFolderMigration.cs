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
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.IO;
using Microsoft.Win32;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for iFolderMigration.
	/// </summary>
	public class iFolderMigration
	{
		#region Class Members

		private static readonly string iFolderRegistryKey = @"Software\Novell iFolder";
		private static readonly string proxyRegistryKey = "Proxy";
		private static readonly string migratedValueName = "Migrated";
		#endregion

		/// <summary>
		/// Constructs an iFolderMigration object.
		/// </summary>
		public iFolderMigration()
		{
		}

		#region Public Methods
		/// <summary>
		/// Checks to see if an old iFolder client is installed on the machine.
		/// </summary>
		/// <returns>This method returns <b>false</b> if an old iFolder client was not found or if the user has already turned down the chance to migrate the settings; otherwise, <b>true</b>.</returns>
		public bool CanBeMigrated()
		{
			// Open the registry key.
			RegistryKey iFolderKey = Registry.LocalMachine.OpenSubKey(iFolderRegistryKey);

			if (iFolderKey != null)
			{
				try
				{
					// The iFolder client is installed ... see if it has already been migrated.
					object migratedValue = iFolderKey.GetValue(migratedValueName);
					if (migratedValue != null)
					{
						if ((int)migratedValue == 0)
						{
							// The user doesn't want to migrate.
							return false;
						}
					}
					else
					{
						// It hasn't been migrated yet.
						return true;
					}
				}
				finally
				{
					iFolderKey.Close();
				}
			}

			return false;
		}

		/// <summary>
		/// Sets the migrated value in the Windows registry.
		/// </summary>
		/// <param name="migrated">A value indicating the state of the migration.</param>
		public void SetMigratedValue(int migrated)
		{
			RegistryKey iFolderKey = Registry.LocalMachine.OpenSubKey(iFolderRegistryKey, true);
			if (iFolderKey != null)
			{
				iFolderKey.SetValue(migratedValueName, migrated);

				iFolderKey.Close();
			}
		}

		/// <summary>
		/// Migrates the settings from the old iFolder client to the new iFolder client.
		/// </summary>
		public void MigrateSettings()
		{
			try
			{
				// Get the proxy setting.
				RegistryKey proxyKey = Registry.LocalMachine.OpenSubKey(Path.Combine(iFolderRegistryKey, proxyRegistryKey));
				if (proxyKey != null)
				{
					string proxyValue = (string)proxyKey.GetValue("Address");

					if (proxyValue != null)
					{
						// Set the proxy setting.
//						Simias.Channels.SimiasChannelFactory.SetProxy(proxyValue);
					}

					proxyKey.Close();
				}

				// TODO: any other settings?

				// Set the state in the registry.
				SetMigratedValue(1);
			}
			catch
			{
				//logger.Debug(e, "Migrating settings");
			}
		}
		#endregion
	}
}
