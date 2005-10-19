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
		private static string DefaultLinuxMappingDir = "/etc/simias";

		/// <summary>
		/// Path to the Simias.exe application.
		/// </summary>
		private string applicationPath = DefaultSimiasApplicationPath;

		/// <summary>
		/// Path to the Simias data area.
		/// </summary>
		private string simiasDataPath = null;

		/// <summary>
		/// Port to listen on. Initialized to use a dynamic port.
		/// </summary>
		private string port = null;

		/// <summary>
		/// Type of configuration to use - server or client.
		/// </summary>
		private bool isServer = false;

		/// <summary>
		/// Flag that creates a console window for Simias.
		/// </summary>
		private bool showConsole = false;

		/// <summary>
		/// Flag that turns on extra debug messages.
		/// </summary>
		private bool verbose = false;

		/// <summary>
		/// Uri to the web service.
		/// </summary>
		private string webServiceUri = null;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the path to the Simias.exe application file.
		/// </summary>
		/// <returns>The path to the Simias.exe application file if successful. 
		/// Otherwise a null is returned.</returns>
		private static string DefaultSimiasApplicationPath
		{
			get
			{
				string applicationPath = null;

				// Look for the application mapping file in the current directory first.
				string tempPath = Path.Combine( Directory.GetCurrentDirectory(), MappingFile );
				if ( !File.Exists( tempPath ) )
				{
					// The file is not in the current directory. Look in the well known place.
					tempPath = Path.Combine( 
						( MyEnvironment.Platform == MyPlatformID.Windows ) ? 
							DefaultWindowsMappingDir : 
							DefaultLinuxMappingDir, 
						MappingFile );
				}

				// Open the file and get the mapping contents.
				try
				{
					using ( StreamReader sr = new StreamReader( tempPath ) )
					{
						applicationPath = Path.GetFullPath( sr.ReadLine() );
					}
				}
				catch
				{}

				return applicationPath;
			}
		}

		/// <summary>
		/// Returns whether the current platform is running on Windows.
		/// </summary>
		private bool IsWindows
		{
			get { return ( MyEnvironment.Platform == MyPlatformID.Windows ) ? true : false; }
		}

		/// <summary>
		/// Getter/Setter for the Simias application path.
		/// </summary>
		public string ApplicationPath
		{
			get { return applicationPath; }
			set { applicationPath = value; }
		}

		/// <summary>
		/// Getter/Setter for the Simias data path.
		/// </summary>
		public string DataPath
		{
			get { return simiasDataPath; }
			set { simiasDataPath = value; }
		}

		/// <summary>
		/// Getter/Setter for the web service port or range. Null indicates to
		/// to use a dynamic port.
		/// </summary>
		public string Port
		{
			get { return port; }
			set { port = value; }
		}

		/// <summary>
		/// Getter/Setter for running in a client or server configuration.
		/// </summary>
		public bool IsServer
		{
			get { return isServer; }
			set { isServer = value; }
		}

		/// <summary>
		/// Getter/Setter for creating a console window when launching Simias.
		/// </summary>
		public bool ShowConsole
		{
			get { return showConsole; }
			set { showConsole = value; }
		}

		/// <summary>
		/// Getter/Setter to turn on extra debug messages.
		/// </summary>
		public bool Verbose
		{
			get { return verbose; }
			set { verbose = value; }
		}

		/// <summary>
		/// Gets the web service URI.
		/// </summary>
		public Uri WebServiceUri
		{
			get { return ( webServiceUri != null ) ? new Uri( webServiceUri ) : null; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes an instance of this object.
		/// </summary>
		public Manager()
		{
		}

		/// <summary>
		/// Initializes an instance of this object.
		/// </summary>
		/// <param name="args">Configuration argument array.</param>
		public Manager( string[] args )
		{
			ParseConfigurationParameters( args );
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Parses the command line parameters to get the configuration for Simias.
		/// </summary>
		/// <param name="args">Array of arguments to set as configuration.</param>
		private void ParseConfigurationParameters( string[] args )
		{
			for ( int i = 0; i < args.Length; ++i )
			{
				switch ( args[ i ].ToLower() )
				{
					case "-p":
					case "--port":
					{
						if ( ( i + 1 ) < args.Length )
						{
							Port = args[ ++i ];
						}
						else
						{
							Console.Error.WriteLine( "Invalid command line parameters. No port or range was specified." );
						}

						break;
					}

					case "-d":
					case "--datadir":
					{
						if ( ( i + 1 ) < args.Length )
						{
							DataPath = args[ ++i ];
						}
						else
						{
							Console.Error.WriteLine( "Invalid command line parameters. No store path was specified." );
						}

						break;
					}

					case "-a":
					case "--apppath":
					{
						if ( ( i + 1 ) < args.Length )
						{
							ApplicationPath = args[ ++i ];
						}
						else
						{
							Console.Error.WriteLine( "Invalid command line parameters. No application path was specified." );
						}

						break;
					}

					case "-i":
					case "--isserver":
					{
						IsServer = true;
						break;
					}

					case "-s":
					case "--showconsole":
					{
						showConsole = true;
						break;
					}

					case "-v":
					case "--verbose":
					{
						verbose = true;
						break;
					}
				}
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Starts the Simias process running.
		/// </summary>
		/// <returns>The URI of the web service if successful.</returns>
		public string Start()
		{
			// Build the arguments string.
			string args = String.Format( "{0}{1}{2}{3}{4}{5}", 
				IsWindows ? String.Empty : String.Format( "\"{0}\" ", applicationPath),
				( simiasDataPath != null ) ? String.Format( "--datadir \"{0}\" ", simiasDataPath ) : String.Empty, 
				( isServer == true ) ? "--runasserver " : String.Empty, 
				( port != null ) ? String.Format( "--port {0}", port ) : String.Empty,
				( showConsole == true ) ? " --showconsole" : String.Empty,
				( verbose == true ) ? " --verbose" : String.Empty );

			// Create the process structure.
			Process simiasProcess = new Process();
			simiasProcess.StartInfo.FileName = IsWindows ? applicationPath : "mono";
			simiasProcess.StartInfo.CreateNoWindow = showConsole ? false : true;
			simiasProcess.StartInfo.RedirectStandardOutput = true;
			simiasProcess.StartInfo.UseShellExecute = false;
			simiasProcess.StartInfo.Arguments = args;
			simiasProcess.Start();

			// Wait for the process to exit, so we can tell if things started successfully.
			simiasProcess.WaitForExit( 15000 );

			// See if the process is still running.
			if ( simiasProcess.HasExited )
			{
				// Get the exit code to see if it was successful.
				if ( simiasProcess.ExitCode == 0 )
				{
					// Read the uri and data path that was printed to stdout.
					webServiceUri = simiasProcess.StandardOutput.ReadLine();
					simiasDataPath = simiasProcess.StandardOutput.ReadLine();
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
		/// Shuts down the simias web service process.
		/// </summary>
		public bool Stop()
		{
			bool stopped = false;

			// Build the arguments string.
			string args = String.Format( "{0}--stop{1}{2}{3}", 
				IsWindows ? String.Empty : String.Format( "\"{0}\" ", applicationPath ),
				( simiasDataPath != null ) ? String.Format( " --datadir \"{0}\"", simiasDataPath ) : String.Empty,
				( showConsole == true ) ? " --showconsole" : String.Empty,
				( verbose == true ) ? " --verbose" : String.Empty );

			// Create the process structure.
			Process simiasProcess = new Process();
			simiasProcess.StartInfo.FileName = IsWindows ? applicationPath : "mono";
			simiasProcess.StartInfo.CreateNoWindow = showConsole ? false : true;
			simiasProcess.StartInfo.RedirectStandardOutput = true;
			simiasProcess.StartInfo.UseShellExecute = false;
			simiasProcess.StartInfo.Arguments = args;
			simiasProcess.Start();

			// Wait for the process to exit, so we can tell if things stopped successfully.
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
