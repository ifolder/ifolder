/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2005 Novell, Inc.
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
 *  Author: Mike Lasky
 *
 ***********************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Simias.Client
{
	/// <summary>
	/// System Manager
	/// </summary>
	public class Manager
	{
		#region Class Members

		/// <summary>
		/// The name of the simias mapping file.
		/// </summary>
		private const string MappingFile = "SimiasDirectoryMapping";

		/// <summary>
		/// The default mapping directories for the specific platforms.
		/// </summary>
		private static string DefaultWindowsMappingDir = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData ), "Simias" );
		private const string DefaultLinuxMappingDir = "/etc/simias";

		#endregion

		#region Properties

		/// <summary>
		/// Gets the default Simias data path.
		/// </summary>
		private static string DefaultSimiasDataPath
		{
			get 
			{
				string path = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
				if ( ( path == null ) || ( path.Length == 0 ) )
				{
					path = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
				}

				return Path.Combine( path, "simias" );
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the path to the Simias.exe application file.
		/// </summary>
		/// <returns>The path to the Simias.exe application file if successful. Otherwise a null is returned.</returns>
		private static string GetSimiasApplicationPath()
		{
			string applicationPath = null;

			// Look for the application mapping file in the current directory first.
			string tempPath = Path.Combine( Directory.GetCurrentDirectory(), MappingFile );
			if ( !File.Exists( tempPath ) )
			{
				// The file is not in the current directory. Look in the well known place.
				tempPath = Path.Combine( 
					( MyEnvironment.Platform == MyPlatformID.Windows ) ? DefaultWindowsMappingDir : DefaultLinuxMappingDir, 
					MappingFile );
			}

			// Open the file and get the mapping contents.
			try
			{
				using ( StreamReader sr = new StreamReader( tempPath ) )
				{
					applicationPath = sr.ReadLine();
				}
			}
			catch
			{}

			return applicationPath;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Starts up the simias web service process.
		/// </summary>
		static public string Start()
		{
			string webServiceUri = null;

			// Get the path to the simias executable.
			string simiasApp = GetSimiasApplicationPath();
			if ( simiasApp == null )
			{
				throw new ApplicationException( "Cannot locate Simias application path." );
			}

			// Create the process structure.
			Process simiasProcess = new Process();
			simiasProcess.StartInfo.FileName = simiasApp;
			simiasProcess.StartInfo.CreateNoWindow = true;
			simiasProcess.StartInfo.RedirectStandardOutput = true;
			simiasProcess.StartInfo.UseShellExecute = false;
			simiasProcess.Start();

			// Wait for the process to exit, so we can tell if things started successfully.
			simiasProcess.WaitForExit( 15000 );

			// See if the process is still running.
			if ( simiasProcess.HasExited )
			{
				// Get the exit code to see if it was successful.
				if ( simiasProcess.ExitCode == 0 )
				{
					// Read the uri that was printed to stdout.
					webServiceUri = simiasProcess.StandardOutput.ReadLine();
				}
				else
				{
					throw new ApplicationException( String.Format( "The Simias process returned an error: {0}", simiasProcess.ExitCode ) );
				}
			}
			else
			{
				simiasProcess.Kill();
				throw new ApplicationException( "Timed out waiting for Simias process to start." );
			}

			return webServiceUri;
		}

		/// <summary>
		/// Starts up the simias web service process.
		/// </summary>
		static public void Start( string applicationPath )
		{
		}

		/// <summary>
		/// Starts up the simias web service process.
		/// </summary>
		public static void Start( string applicationPath, string simiasDataPath )
		{
		}

		/// <summary>
		/// Starts up the simias web service process.
		/// </summary>
		public static void Start( string applicationPath, string simiasDataPath, int port )
		{
		}

		/// <summary>
		/// Starts up the simias web service process.
		/// </summary>
		public static void Start( string applicationPath, string simiasDataPath, int port, bool isServer )
		{
		}

		/// <summary>
		/// Shuts down the simias web service process.
		/// </summary>
		static public bool Stop()
		{
			bool stopped = false;

			// Get the path to the simias executable.
			string simiasApp = GetSimiasApplicationPath();
			if ( simiasApp == null )
			{
				throw new ApplicationException( "Cannot locate Simias application path." );
			}

			// Create the process structure.
			Process simiasProcess = new Process();
			simiasProcess.StartInfo.FileName = simiasApp;
			simiasProcess.StartInfo.CreateNoWindow = true;
			simiasProcess.StartInfo.RedirectStandardOutput = true;
			simiasProcess.StartInfo.UseShellExecute = false;
			simiasProcess.StartInfo.Arguments = "--stop";
			simiasProcess.Start();

			// Wait for the process to exit, so we can tell if things started successfully.
			simiasProcess.WaitForExit( 15000 );

			// See if the process is still running.
			if ( simiasProcess.HasExited )
			{
				// Get the exit code to see if it was successful.
				stopped = ( simiasProcess.ExitCode == 0 ) ? true : false;
			}
			else
			{
				simiasProcess.Kill();
				throw new ApplicationException( "Timed out waiting for Simias process to start." );
			}

			return stopped;
		}
		#endregion
	}
}
