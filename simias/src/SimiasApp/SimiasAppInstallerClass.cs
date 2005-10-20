/***********************************************************************
 *	$RCSfile$
 *
 *	Copyright (C) 2004 Novell, Inc.
 *
 *	This program is free software; you can redistribute it and/or
 *	modify it under the terms of the GNU General Public
 *	License as published by the Free Software Foundation; either
 *	version 2 of the License, or (at your option) any later version.
 *
 *	This program is distributed in the hope that it will be useful,
 *	but WITHOUT ANY WARRANTY; without even the implied warranty of
 *	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.	See the GNU
 *	General Public License for more details.
 *
 *	You should have received a copy of the GNU General Public
 *	License along with this program; if not, write to the Free
 *	Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *	Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/
#if WINDOWS

using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;

namespace Mono.ASPNET
{
	/// <summary>
	/// Provides custom installation for iFolderApp.exe.
	/// </summary>
	// Set 'RunInstaller' attribute to true.
	[ RunInstallerAttribute( true ) ]
	public class SimiasAppInstallerClass: Installer
	{
		/// <summary>
		/// The default mapping directories for the specific platforms.
		/// </summary>
		private static string DefaultWindowsMappingDir = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData ), "Simias" );
		private const string MappingFile = "SimiasDirectoryMapping";

		/// <summary>
		/// Constructor.
		/// </summary>
		public SimiasAppInstallerClass() : base()
		{
		}

		/// <summary>
		/// Override the 'Install' method.
		/// </summary>
		/// <param name="savedState"></param>
		public override void Install( IDictionary savedState )
		{
			base.Install( savedState );

			// Get the install location for Simias.exe.
			string assemblyPath = Assembly.GetExecutingAssembly().Location;

			// The SimiasDefaultMapping file is in the directory that contains 'web\bin\Simias.exe'.
			string installDir = Path.GetFullPath( Path.Combine( Path.GetDirectoryName( assemblyPath ), "../.." ) );

			// See if the SimiasDirectoryMapping file exists.
			string dirMappingFile = Path.Combine( installDir, MappingFile );
			if ( File.Exists( dirMappingFile ) )
			{
				// Write the install location of Simias to the file.
				try
				{
					// Get the directory where the assembly is running.
					using ( StreamWriter sw = new StreamWriter( dirMappingFile ) )
					{
						sw.WriteLine( assemblyPath );
					}
				}
				catch ( Exception ex )
				{
					if ( Context != null )
					{
						Context.LogMessage( String.Format( "ERROR: Exception {0} writing to {1}", ex.Message, dirMappingFile ) );
					}
				}

				// Copy the changed file to the common application directory.
				string destFile = Path.Combine( DefaultWindowsMappingDir, MappingFile );
				try
				{
					File.Copy( dirMappingFile, destFile, true );
				}
				catch ( Exception ex )
				{
					if ( Context != null )
					{
						Context.LogMessage( String.Format( "ERROR: Exception {0} copying mapping file to {1}", ex.Message, destFile ) );
					}
				}
			}
			else
			{
				if ( Context != null )
				{
					Context.LogMessage( String.Format( "ERROR: Cannot find {0}", dirMappingFile ) );
				}
			}
		}

		/// <summary>
		/// Override the 'Commit' method.
		/// </summary>
		/// <param name="savedState"></param>
		public override void Commit( IDictionary savedState )
		{
			base.Commit( savedState );
		}

		/// <summary>
		/// Override the 'Rollback' method.
		/// </summary>
		/// <param name="savedState"></param>
		public override void Rollback( IDictionary savedState )
		{
			base.Rollback( savedState );
		}

		/// <summary>
		/// Override the 'Uninstall' method.
		/// </summary>
		/// <param name="savedState"></param>
        public override void Uninstall( IDictionary savedState )
		{
			base.Uninstall(savedState);
		}
	}
}

#endif
