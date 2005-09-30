/**********************************************************************
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
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;

using Simias.Client;

namespace Mono.ASPNET
{
	/// <summary>
	/// Implements the web server functionality for Simias.
	/// </summary>
	public class SimiasWebServer
	{
		#region Class Members

		/// <summary>
		/// Mutex used to force only one controlling process at a time to run.
		/// </summary>
		private static string ControllerMutexName = "SimiasControllerMutex";
		private const int ControllerMutexTimeout = 15 * 1000;
		private static FileStream mutexFileStream = null;


		/// <summary>
		/// Path to running application. Remove the quotes that get added to the string.
		/// </summary>
		private static string ApplicationPath;

		/// <summary>
		/// Virtual path used in uri to contact the web service.
		/// </summary>
		private static string VirtualPath = "/simias10";

		/// <summary>
		/// Prefix added to create a process file name.
		/// </summary>
		private static string ProcessFilePrefix = "Simias_";

		/// <summary>
		/// Tags used to set the port number in the port configuration file.
		/// </summary>
		private static string PortConfigurationFileName = "xspport.cfg";
		private static string PortConfigurationTag = "XspPortConfiguration";
		private static string PortTag = "Port";

		/// <summary>
		/// Default server port.
		/// </summary>
		private const int DefaultServerPort = 8080;

		/// <summary>
		/// Return program status values.
		/// </summary>
		private enum SimiasStatus
		{
			InvalidSimiasApplicationPath = -6,
			ServiceNotAvailable = -5,
			ProcessFailure = -4,
			NoAvailablePorts = -3,
			ControllerTimeout = -2,
			InvalidCommandLine = -1,
			Success = 0
		}

		/// <summary>
		/// Commands for Simias services.
		/// </summary>
		private enum SimiasCommand
		{
			/// <summary>
			/// Start the Simias services.
			/// </summary>
			Start,

			/// <summary>
			/// Stop the Simias services nicely.
			/// </summary>
			Stop,

			/// <summary>
			/// Stop the Simias services immediately.
			/// </summary>
			Kill,

			/// <summary>
			/// Stop all Simias services in the user's context immediately.
			/// </summary>
			KillAll,

			/// <summary>
			/// Show information about the currently executing Simias processes.
			/// </summary>
			Info,

			/// <summary>
			///  Show version information.
			/// </summary>
			Version,

			/// <summary>
			/// Show help.
			/// </summary>
			Help
		}

		/// <summary>
		/// Debug flag specified on the --noexec option.
		/// </summary>
		private bool debug = false;

		/// <summary>
		/// Execute Simias services in a child process.
		/// </summary>
		private bool noExec = false;

		/// <summary>
		/// Run Simias services in a client configuration.
		/// </summary>
		private bool runAsServer = false;

		/// <summary>
		/// Don't show Simias services console output.
		/// </summary>
		private bool showConsole = false;

		/// <summary>
		/// Prints extra data for debugging purposes.
		/// </summary>
		private bool verbose = false;

		/// <summary>
		/// Port or range of ports.
		/// </summary>
		private string portRangeString;

		/// <summary>
		/// Port used as an IPC between AppDomains.
		/// </summary>
		private string ipcPortString;

		/// <summary>
		/// Used shared Simias data directory.
		/// </summary>
		private string simiasDataPath = DefaultSimiasDataPath;

		/// <summary>
		/// Default command is Start if no other commands are specified.
		/// </summary>
		private SimiasCommand command = SimiasCommand.Start;

		/// <summary>
		/// Socket used to communicate with the XSP server running in a different
		/// application domain.
		/// </summary>
		private Socket ipcServerSocket = null;
		private Socket ipcMessageSocket = null;
		private bool ipcIsClosed = false;

		/// <summary>
		/// Event used to hold the main execution thread until signaled to shut down.
		/// </summary>
		private ManualResetEvent stopServerEvent = new ManualResetEvent( false );

		#endregion

		#region Properties

		/// <summary>
		/// Gets the full name of the Simias Controller mutex.
		/// </summary>
		private static string ControllerMutex
		{
			get
			{
				string path = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
				if ( ( path == null ) || ( path.Length == 0 ) )
				{
					path = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
				}

				return String.Format( "{0}.{1}", Path.Combine( path, ControllerMutexName ), Process.GetCurrentProcess().Id );
			}
		}

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

		#region Constructor

		/// <summary>
		/// Initializes an instance of this object.
		/// </summary>
		public SimiasWebServer()
		{
			string[] args = Environment.GetCommandLineArgs();
			ApplicationPath = args[ 0 ];
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Acquires the controller mutex that allows only one controller at a time to run.
		/// </summary>
		/// <returns>True if the controller was acquired. Otherwise false is returned.</returns>
		private static bool AcquireControllerMutex()
		{
			bool processWaitTimeout = false;
			string fileName = ControllerMutex;
			string processIDString = Path.GetExtension( fileName );

			// Create this processes mutex file. This will hold this controller's place in line.
			mutexFileStream = File.Create( fileName );

			// Search for any existing mutex files.
			string[] fileList = Directory.GetFiles( Path.GetDirectoryName( fileName ), Path.GetFileNameWithoutExtension( fileName ) + ".*" );
			foreach ( string file in fileList )
			{
				string fileExt = Path.GetExtension( file );

				// If this is the file that was just created, skip it.
				if ( processIDString != fileExt )
				{
					try
					{
						// Try and open the file to see if the process has just gone away. If the file cannot be
						// opened, the corresponding controller process is still running.
						using ( FileStream fs = new FileStream( file, FileMode.Open, FileAccess.ReadWrite, FileShare.None ) )
						{}

						// The file was opened successfully. The process must have terminated abnormally.
						// Clean up the file.
						File.Delete( file );
					}
					catch ( System.IO.FileNotFoundException )
					{
						// The file must have been deleted by the time we went to open it. Proceed normally.
					}
					catch ( System.IO.IOException )
					{
						// The file is in use. Wait for the process to exit before going on.
						int processID = Convert.ToInt32( fileExt.TrimStart( new char[] { '.' } ) );
						try
						{
							// Attach to the running process.
							Process process = Process.GetProcessById( processID );
							process.WaitForExit( SimiasWebServer.ControllerMutexTimeout );
							if ( !process.HasExited )
							{
								processWaitTimeout = true;

								// We can't acquire the mutex because another controller is still
								// running and we have timed out. Clean up our own mutex file.
								ReleaseControllerMutex();
								break;
							}
						}
						catch ( ArgumentException )
						{}
					}
				}
			}

			return processWaitTimeout ? false : true;
		}

		/// <summary>
		/// Increments the reference count that keeps Simias services running.
		/// </summary>
		/// <param name="webServiceUri">The uri that references the web service.</param>
		/// <returns>The current reference count.</returns>
		private int AddReference( Uri webServiceUri )
		{
			return AddReference( webServiceUri, simiasDataPath );
		}

		/// <summary>
		/// Increments the reference count that keeps Simias services running.
		/// </summary>
		/// <param name="webServiceUri">The uri that references the web service.</param>
		/// <param name="dataPath">Directory path to the Simias data area.</param>
		/// <returns>The current reference count.</returns>
		private int AddReference( Uri webServiceUri, string dataPath )
		{
			int refCount = -1;

			try
			{
				SimiasWebService svc = new SimiasWebService();
				svc.Url = webServiceUri.ToString() + "/Simias.asmx";
				Simias.Client.LocalService.Start( svc, webServiceUri, dataPath );
				refCount = svc.AddSimiasReference();
			}
			catch {}

			return refCount;
		}

		/// <summary>
		/// Checks to see if the specified port is available.
		/// </summary>
		/// <param name="port">Port number to check for availability.</param>
		/// <returns>The port number if it is available. Otherwise a -1 is returned.</returns>
		private int AvailablePortCheck( int port )
		{
			int boundPort = -1;

			Socket s = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
			try
			{
				try
				{
					s.Bind( new IPEndPoint( IPAddress.Loopback, port ) );
					boundPort = ( s.LocalEndPoint as IPEndPoint ).Port;
				}
				catch( SocketException )
				{}
			}
			finally
			{
				s.Close();
			}

			return boundPort;
		}

		/// <summary>
		/// Checks if specified web service is shareable by this application instance.
		/// </summary>
		/// <param name="uri">URI to the Simias web service.</param>
		/// <returns>True if service can be shared, otherwise false is returned.</returns>
		private bool CanShareSimiasService( Uri uri )
		{
			bool shareable = false;

			try
			{
				SimiasWebService svc = new SimiasWebService();
				svc.Url = uri.ToString() + "/Simias.asmx";
				Simias.Client.LocalService.Start( svc, uri, simiasDataPath );
				shareable = svc.CanShareService( simiasDataPath, !runAsServer );
			}
			catch {}

			return shareable;
		}

		/// <summary>
		/// Creates a unique file for this process in the temporary directory that the
		/// kill all command uses to stop all running simias processes.
		/// </summary>
		private void CreateProcessFile()
		{
			try
			{
				using( File.Create( CreateProcessFileName() ) ) {}
			}
			catch
			{}
		}

		/// <summary>
		/// Creates a unique name for this simias process.
		/// </summary>
		/// <returns>An absolute path to a unique Simias process file.</returns>
		private string CreateProcessFileName()
		{
			string s1 = simiasDataPath.Replace( ':', '@' ).Replace( Path.DirectorySeparatorChar, '_' );
			return Path.Combine( Path.GetTempPath(), String.Format( "{0}{1}.tmp", ProcessFilePrefix, s1 ) );
		}

		/// <summary>
		/// Deletes the unique file for this process in the temporary directory that the
		/// kill all command uses to stop all running simias processes.
		/// </summary>
		private void DeleteProcessFile()
		{
			try
			{
				string pfName = CreateProcessFileName();
				if ( File.Exists( pfName ) )
				{
					File.Delete( pfName );
				}
			}
			catch
			{}
		}

		/// <summary>
		/// Gets a specified range of ports to use as the local listener.
		/// </summary>
		/// <param name="portString">String that contains port or port range.</param>
		/// <returns>An array of integers that represent a range of TCP port numbers.
		/// A null is returned if no port range was specified.</returns>
		private int[] GetPortRange( string portString )
		{
			int[] range = null;

			if ( portString != null )
			{
				// See if there is a range separator.
				int sep = portString.IndexOf( '-' );
				if ( sep != -1 )
				{
					// Get the start and end values.
					int start = Convert.ToInt32( portString.Substring( 0, sep ).Trim() );
					int end = Convert.ToInt32( portString.Substring( sep + 1 ).Trim() );

					// Make sure the range is valid.
					if ( end >= start )
					{
						// Fill the array with the range of port numbers.
						range = new int[ ( end - start ) + 1 ];
						for ( int i = start; i <= end; ++i )
						{
							range[ i - start ] = i;
						}
					}
				}
				else
				{
					// No range was specified, just a single port.
					range = new int[ 1 ] { Convert.ToInt32( portString.Trim() ) };
				}
			}

			return range;
		}

		/// <summary>
		/// Gets the Simias data path from the specified encoded name.
		/// </summary>
		/// <param name="processFileName">Name created by CreateProcessFileName().</param>
		/// <returns>The directory path to the Simias data area.</returns>
		private string GetProcessFileName( string processFileName )
		{
			string s1 = Path.GetFileNameWithoutExtension( processFileName ).Substring( ProcessFilePrefix.Length );
			return s1.Replace( '@', ':' ).Replace( '_', Path.DirectorySeparatorChar );
		}

		/// <summary>
		/// Attaches a process object to the Simias process specified by the uri.
		/// </summary>
		/// <param name="uri">URI to the Simias web service.</param>
		/// <param name="dataPath">Directory path to the Simias data area.</param>
		/// <returns>Process object if successful. Otherwise a null is returned.</returns>
		private Process GetSimiasProcess( Uri uri, string dataPath )
		{
			Process process = null;

			try
			{
				SimiasWebService svc = new SimiasWebService();
				svc.Url = uri.ToString() + "/Simias.asmx";
				Simias.Client.LocalService.Start( svc, uri, dataPath );
				process = Process.GetProcessById( svc.GetSimiasProcessID() );
			}
			catch {}

			return process;
		}

		/// <summary>
		/// Gets a port to use to start the web server.
		/// </summary>
		/// <param name="ports">Range of ports to use.</param>
		private int GetXspPort( string ports )
		{
			int boundPort = -1;

			// See if there is a port or range specified.
			int[] range = GetPortRange( ports );
			if ( range == null )
			{
				// See if there was a port specified previously.
				boundPort = ReadXspPortFromFile();
				if ( boundPort == -1 )
				{
					// Just use default server port or if a client, a single dynamic port.
					boundPort = AvailablePortCheck( runAsServer ? DefaultServerPort : 0 );
				}
				else
				{
					// Check to see if the port in the file is available. If the port is not
					// available and Simias is running as a server, return an error.
					boundPort = AvailablePortCheck( boundPort );
					if ( ( boundPort == -1 ) && !runAsServer )
					{
						// The port in the file was not available and no port was specified
						// on the command line. Allocate a new dynamic port.
						boundPort = AvailablePortCheck( 0 );
					}
				}
			}
			else
			{
				// Loop through looking for an available port.
				for ( int i = 0; ( boundPort == -1 ) && ( i < range.Length ); ++i )
				{
					// Make sure that the socket is available.
					boundPort = AvailablePortCheck( range[ i ] );
				}
			}

			if ( boundPort != -1 )
			{
				// Write the port to the configuration file.
				WriteXspPortToFile( boundPort );
			}

			return boundPort;
		}

		/// <summary>
		/// Determines if the specified port in with in the specified port range.
		/// </summary>
		/// <param name="port">Port to check to see if it is in the range.</param>
		/// <returns>True if the port is in the range, otherwise false is returned.</returns>
		private bool IsPortInRange( int port )
		{
			bool isInRange = false;

			if ( portRangeString != null )
			{
				int[] range = GetPortRange( portRangeString );
				if ( range != null )
				{
					if ( ( port >= range[ 0 ] ) && ( port <= range[ range.Length - 1 ] ) )
					{
						isInRange = true;
					}
				}
			}
			else
			{
				// There was no port range specified. Therefore the current port is valid.
				isInRange = true;
			}

			return isInRange;
		}

		/// <summary>
		/// Checks if specified web service is running from the specified simiasDataPath.
		/// </summary>
		/// <param name="uri">URI to the Simias web service.</param>
		/// <returns>True if service is the same, otherwise false is returned.</returns>
		private bool IsSameService( Uri uri )
		{
			return IsSameService( uri, simiasDataPath );
		}

		/// <summary>
		/// Checks if specified web service is running from the specified simiasDataPath.
		/// </summary>
		/// <param name="uri">URI to the Simias web service.</param>
		/// <param name="dataPath">Directory path to the Simias data area.</param>
		/// <returns>True if service is the same, otherwise false is returned.</returns>
		private bool IsSameService( Uri uri, string dataPath )
		{
			bool sameService = false;
			bool ignoreCase = ( MyEnvironment.Platform == MyPlatformID.Windows ) ? true : false;

			try
			{
				SimiasWebService svc = new SimiasWebService();
				svc.Url = uri.ToString() + "/Simias.asmx";
				Simias.Client.LocalService.Start( svc, uri, dataPath );
				if ( String.Compare( dataPath, svc.GetSimiasDataPath(), ignoreCase ) == 0 )
				{
					sameService = true;
				}
			}
			catch 
			{}

			return sameService;
		}

		/// <summary>
		/// Stops all of the Simias services immediately.
		/// </summary>
		/// <returns>SimiasStatus</returns>
		private SimiasStatus KillAllSimias()
		{
			SimiasStatus status = SimiasStatus.Success;

			// Find all of the temp Simias_* files.
			string[] simiasFiles = Directory.GetFiles( Path.GetTempPath(), "Simias_*" );
			if ( simiasFiles.Length > 0 )
			{
				// Kill the process for each of the files in the list.
				foreach ( string file in simiasFiles )
				{
					// Get the Simias data path from the file name.
					string processFileName = GetProcessFileName( file );

					// See if there is a port configured in the specified data area.
					int port = ReadXspPortFromFile( processFileName );
					if ( port != -1 )
					{
						// Build the URI for web services.
						UriBuilder ub = new UriBuilder( Uri.UriSchemeHttp, IPAddress.Loopback.ToString(), port, VirtualPath );

						// There has been a port configured previously. Check to see if the Simias services 
						// are already running on this port or range.
						if ( PingWebService( ub.Uri, false ) && IsSameService( ub.Uri, processFileName ) )
						{
							KillSimiasServer( ub.Uri, processFileName );
						}
					}
				}
			}
			else
			{
				Console.Error.WriteLine( "Error: No Simias processes are running." );
				status = SimiasStatus.ServiceNotAvailable;
			}

			return status;
		}

		/// <summary>
		/// Stops the simias services immediately for the specified --datadir.
		/// </summary>
		/// <returns>SimiasStatus</returns>
		private SimiasStatus KillSimias()
		{
			SimiasStatus status = SimiasStatus.Success;

			// See if there is a port configured in the specified data area.
			int port = ReadXspPortFromFile();
			if ( port != -1 )
			{
				// Build the URI for web services.
				UriBuilder ub = new UriBuilder( Uri.UriSchemeHttp, IPAddress.Loopback.ToString(), port, VirtualPath );

				// There has been a port configured previously. Check to see if the Simias services 
				// are already running on this port or range.
				if ( PingWebService( ub.Uri, false ) && IsSameService( ub.Uri ) )
				{
					if ( !KillSimiasServer( ub.Uri ) )
					{
						Console.Error.WriteLine( "Error: Cannot contact {0} to kill the service.", ub.Uri );
						status = SimiasStatus.ServiceNotAvailable;
					}
				}
				else
				{
					Console.Error.WriteLine( "Error: No service found for {0}.", simiasDataPath );
					status = SimiasStatus.ServiceNotAvailable;
				}
			}

			return status;
		}

		/// <summary>
		/// Stops the Simias service immediately. Does not honor the reference count.
		/// </summary>
		/// <param name="webServiceUri">The uri that references the web service.</param>
		/// <returns>True if the webservice responded to the kill command. Otherwise false is returned.</returns>
		private bool KillSimiasServer( Uri webServiceUri )
		{
			return KillSimiasServer( webServiceUri, simiasDataPath );
		}

		/// <summary>
		/// Stops the Simias service immediately. Does not honor the reference count.
		/// </summary>
		/// <param name="webServiceUri">The uri that references the web service.</param>
		/// <param name="dataPath">Directory path to the Simias data area.</param>
		/// <returns>True if the webservice responded to the kill command. Otherwise false is returned.</returns>
		private bool KillSimiasServer( Uri webServiceUri, string dataPath )
		{
			bool status = false;

			try
			{
				SimiasWebService svc = new SimiasWebService();
				svc.Url = webServiceUri.ToString() + "/Simias.asmx";
				Simias.Client.LocalService.Start( svc, webServiceUri, dataPath );
				svc.StopSimiasProcess();
				status = true;
			}
			catch {}

			return status;
		}

		/// <summary>
		/// Parses the command line parameters and environment variables to get
		/// the configuration for Simias.
		/// </summary>
		/// <param name="args">Command line parameters.</param>
		/// <returns>Zero if successful. Otherwise a specific</returns>
		private SimiasStatus ParseConfigurationParameters( string[] args )
		{
			SimiasStatus status = SimiasStatus.Success;
			for ( int i = 0; ( status == SimiasStatus.Success ) && ( i < args.Length ); ++i )
			{
				switch ( args[ i ].ToLower() )
				{
					case "--port":
					{
						if ( ( i + 1 ) < args.Length )
						{
							portRangeString = args[ ++i ];
						}
						else
						{
							Console.Error.WriteLine( "Invalid command line parameters. No port or range was specified." );
							status = SimiasStatus.InvalidCommandLine;
						}

						break;
					}

					case "--runasserver":
					{
						runAsServer = true;
						break;
					}

					case "--datadir":
					{
						if ( ( i + 1 ) < args.Length )
						{
							simiasDataPath = args[ ++i ];
						}
						else
						{
							Console.Error.WriteLine( "Invalid command line parameters. No path was specified." );
							status = SimiasStatus.InvalidCommandLine;
						}

						break;
					}

					case "--ipcport":
					{
						if ( ( i + 1 ) < args.Length )
						{
							ipcPortString = args[ ++i ];
						}
						else
						{
							Console.Error.WriteLine( "Invalid command line parameters. No IPC port was specified." );
							status = SimiasStatus.InvalidCommandLine;
						}

						break;
					}

					case "--noexec":
					{
						if ( ( ( i + 1 ) < args.Length ) && ( args[ i + 1 ].ToLower() == "debug" ) )
						{
							debug = true;
							++i;
						}

						noExec = true;
						break;
					}

					case "--showconsole":
					{
						showConsole = true;
						break;
					}

					case "--start":
					{
						if ( command != SimiasCommand.Start )
						{
							// A different command was specified earlier. Show an error.
							Console.Error.WriteLine( "Invalid command line parameters. More than one command was specified." );
							status = SimiasStatus.InvalidCommandLine;
						}

						break;
					}

					case "--stop":
					{
						if ( command != SimiasCommand.Start )
						{
							// A different command was specified earlier. Show an error.
							Console.Error.WriteLine( "Invalid command line parameters. More than one command was specified." );
							status = SimiasStatus.InvalidCommandLine;
						}
						else
						{
							command = SimiasCommand.Stop;
						}

						break;
					}

					case "--kill":
					{
						if ( command != SimiasCommand.Start )
						{
							// A different command was specified earlier. Show an error.
							Console.Error.WriteLine( "Invalid command line parameters. More than one command was specified." );
							status = SimiasStatus.InvalidCommandLine;
						}
						else
						{
							if ( ( ( i + 1 ) < args.Length ) && ( args[ i + 1 ].ToLower() == "all" ) )
							{
								command = SimiasCommand.KillAll;
								++i;
							}
							else
							{
								command = SimiasCommand.Kill;
							}
						}

						break;
					}

					case "--info":
					{
						if ( command != SimiasCommand.Start )
						{
							// A different command was specified earlier. Show an error.
							Console.Error.WriteLine( "Invalid command line parameters. More than one command was specified." );
							status = SimiasStatus.InvalidCommandLine;
						}
						else
						{
							command = SimiasCommand.Info;
						}

						break;
					}

					case "--version":
					{
						if ( command != SimiasCommand.Start )
						{
							// A different command was specified earlier. Show an error.
							Console.Error.WriteLine( "Invalid command line parameters. More than one command was specified." );
							status = SimiasStatus.InvalidCommandLine;
						}
						else
						{
							command = SimiasCommand.Version;
						}

						break;
					}

					case "--help":
					{
						if ( command != SimiasCommand.Start )
						{
							// A different command was specified earlier. Show an error.
							Console.Error.WriteLine( "Invalid command line parameters. More than one command was specified." );
							status = SimiasStatus.InvalidCommandLine;
						}
						else
						{
							command = SimiasCommand.Help;
						}

						break;
					}

					case "--verbose":
					{
						verbose = true;
						break;
					}

					default:
					{
						// Unknown command line option.
						Console.Error.WriteLine( "{0} is an invalid command line option.", args[ i ] );
						status = SimiasStatus.InvalidCommandLine;
						break;
					}
				}
			}

			// If --noexec was specified, make sure that --port and --datadir were also specified.
			if ( noExec )
			{
				if ( ( Environment.CommandLine.IndexOf( "--port " ) == -1 ) ||
					 ( Environment.CommandLine.IndexOf( "--datadir " ) == -1 ) ||
					 ( Environment.CommandLine.IndexOf( "--ipcport " ) == -1 ) )
				{
					Console.Error.WriteLine( "Error: Invalid command line parameters.\nMust specify --port, --datadir and --ipcport when using --noexec." );
					status = SimiasStatus.InvalidCommandLine;
				}
			}

			return status;
		}

		/// <summary>
		/// Pings the Simias web service to start the server running.
		/// </summary>
		/// <param name="uri">The URI of the web service to ping.</param>
		/// <param name="waitForDefaultTimeout">If true, the method waits the default
		/// amount of time for the web service to answer. This should be set to true
		/// if starting the web service. If just checking for an already running web
		/// service, a quicker timeout can be used and waitForDefaultTimeout should
		/// be set to false.</param>
		/// <returns>True if ping was successful, otherwise false is returned.</returns>
		private bool PingWebService( Uri uri, bool waitForDefaultTimeout )
		{
			bool pingStatus = false;

			try
			{
				SimiasWebService svc = new SimiasWebService();
				svc.Url = uri.ToString() + "/Simias.asmx";
				if ( !waitForDefaultTimeout )
				{
					// Don't wait very long. The web service should already be up.
					svc.Timeout = 500;
				}

				if ( verbose )
				{
					Console.Error.WriteLine( "Pinging {0}", svc.Url );
				}

				svc.PingSimias();
				pingStatus = true;
			}
			catch {}

			return pingStatus;
		}

		/// <summary>
		/// Processes IPC messages from the XSP server.
		/// </summary>
		/// <param name="message">String containing message.</param>
		private void ProcessIpcMessage( string message )
		{
			switch ( message.ToLower() )
			{
				case "stop_server":
				{
					stopServerEvent.Set();
					break;
				}

				default:
				{
					Console.Error.WriteLine( "Error: Received unknown message: {0}", message );
					break;
				}
			}
		}

		/// <summary>
		/// Gets the port from the configuration file.
		/// </summary>
		/// <returns>Port number if successful, otherwise -1 is returned.</returns>
		private int ReadXspPortFromFile()
		{
			return ReadXspPortFromFile( simiasDataPath );
		}

		/// <summary>
		/// Gets the port from the configuration file.
		/// </summary>
		/// <param name="dataPath">The directory path to the Simias data.</param>
		/// <returns>Port number if successful, otherwise -1 is returned.</returns>
		private int ReadXspPortFromFile( string dataPath )
		{
			int port = -1;

			try
			{
				XmlDocument document = new XmlDocument();
				document.Load( Path.Combine( dataPath, PortConfigurationFileName ) );
				port = Convert.ToInt32( document.DocumentElement[ PortTag ].InnerText );
			}
			catch
			{}

			return port;
		}

		/// <summary>
		/// Releases the controller mutex that allows only one controller at a time to run.
		/// </summary>
		private static void ReleaseControllerMutex()
		{
			if ( mutexFileStream != null )
			{
				mutexFileStream.Close();
				try
				{
					File.Delete( mutexFileStream.Name );
				}
				catch
				{}

				mutexFileStream = null;
			}
		}

		/// <summary>
		/// Decrements the Simias service reference count and signals the server to stop if the count goes to zero.
		/// </summary>
		/// <param name="webServiceUri">The uri that references the web service.</param>
		/// <returns>The current reference count.</returns>
		private int RemoveReference( Uri webServiceUri )
		{
			return RemoveReference( webServiceUri, simiasDataPath );
		}

		/// <summary>
		/// Decrements the Simias service reference count and signals the server to stop if the count goes to zero.
		/// </summary>
		/// <param name="webServiceUri">The uri that references the web service.</param>
		/// <param name="dataPath">Directory path to the Simias data area.</param>
		/// <returns>The current reference count.</returns>
		private int RemoveReference( Uri webServiceUri, string dataPath )
		{
			int refCount = -1;

			try
			{
				SimiasWebService svc = new SimiasWebService();
				svc.Url = webServiceUri.ToString() + "/Simias.asmx";
				Simias.Client.LocalService.Start( svc, webServiceUri, dataPath );
				refCount = svc.RemoveSimiasReference();
			}
			catch {}

			return refCount;
		}

		/// <summary>
		/// The IPC mechanism for communicating with the server.
		/// </summary>
		private void ServerMessageIpc()
		{
			// Allocate a socket to listen for requests on.
			ipcServerSocket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

			// Bind the socket to the IPC port on the loopback address.
			ipcServerSocket.Bind( new IPEndPoint( IPAddress.Loopback, Convert.ToInt32( ipcPortString ) ) );
			ipcServerSocket.Listen( 10 ); 

			byte[] buffer = new byte[ 512 ];
			int bufferLength = 0;

			while ( !ipcIsClosed )
			{
				try
				{
					// Wait for an incoming request.
					ipcMessageSocket = ipcServerSocket.Accept();

					// Make sure that this request came from the loopback address. Off-box requests are not supported.
					if ( IPAddress.IsLoopback( ( ipcMessageSocket.RemoteEndPoint as IPEndPoint ).Address ) )
					{
						bool connectionClosed = false;
						while ( !ipcIsClosed && !connectionClosed )
						{
							// Get the data.
							int bytesReceived = ipcMessageSocket.Receive( buffer );
							if ( bytesReceived > 0 )
							{
								// Keep track of how much data is in the buffer.
								bufferLength += bytesReceived;
								int bytesToProcess = bufferLength;
								int msgIndex = 0;

								// Process as much as is available. 
								while ( bytesToProcess > 0 )
								{
									// There needs to be at least 4 bytes for the message length.
									if ( bytesToProcess >= 4 )
									{
										// Get the length of the message, add in the prepended length.
										int msgLength = BitConverter.ToInt32( buffer, msgIndex ) + 4;
					
										// See if the entire message is represented in the buffer.
										if ( bytesToProcess >= msgLength )
										{
											// Process the message received from the client. Skip the message length.
											ProcessIpcMessage( new UTF8Encoding().GetString( buffer, msgIndex + 4, msgLength - 4 ) );
											msgIndex += msgLength;
											bytesToProcess -= msgLength;
										}
										else
										{
											break;
										}
									}
									else
									{
										break;
									}
								}

								// Update how much data is left in the buffer.
								bufferLength = bytesToProcess;

								// If any data has been processed at the beginning of the buffer, the unprocessed
								// data needs to be moved to the front.
								if ( ( bytesToProcess > 0 ) && ( msgIndex > 0 ) )
								{
									// Move any existing data up.
									Buffer.BlockCopy( buffer, msgIndex, buffer, 0, bytesToProcess );
								}
							}
							else
							{
								// The connection has closed.
								connectionClosed = true;
							}
						}
					}
				}
				catch ( SocketException se )
				{
					if ( !ipcIsClosed )
					{
						Console.Error.WriteLine( "Error: IPC Socket - {0}", se.Message );
					}
				}
			}
		}

		/// <summary>
		/// Shows the help for Simias.exe
		/// </summary>
		private void ShowHelp()
		{
			Console.WriteLine();
			Console.WriteLine( "Simias is a specialized object data store whose primary application is to" );
			Console.WriteLine( "associate searchable metadata with file system entries. The metadata and" );
			Console.WriteLine( "associated file system entries comprise a collection, which can be shared from" );
			Console.WriteLine( "a master data store to multiple slave data stores.  The collection data will be" );
			Console.WriteLine( "kept synchronized between all of the data stores. Collections may also be made" );
			Console.WriteLine( "up of objects that do not model a file system, but that have defined" );
			Console.WriteLine( "relationships to each other." );
			Console.WriteLine();
			Console.WriteLine( "Command line arguments:" );
			Console.WriteLine( "    --port [Port number] || [Port number start-Port number end]:" );
			Console.WriteLine( "        The TCP port or port range to listen on. The first available port" );
			Console.WriteLine( "        specified in the port range will be used. The starting port number" );
			Console.WriteLine( "        must be less than or equal to the ending port number in a specified" );
			Console.WriteLine( "        port range." );
			Console.WriteLine( "        Default value: 8080 when running as a server or dynamic port is used" );
			Console.WriteLine( "        when running as a client." );
			Console.WriteLine( "        Example: --port 8080 OR --port 8080-8086" );
			Console.WriteLine();
			Console.WriteLine( "    --ipcport [ Port number ]:" );
			Console.WriteLine( "        The TCP port to use between the Simias.exe application and the XSP" );
			Console.WriteLine( "        server for communication across the application domains. IPC" );
			Console.WriteLine( "        messages will be sent across this port to tell the XSP server to shut" );
			Console.WriteLine( "        down and other needed operations. This parameter must be specified if" );
			Console.WriteLine( "        --noexec is specified." );
			Console.WriteLine( "        Default value: None" );
			Console.WriteLine();							
			Console.WriteLine( "    --runasserver:" );
			Console.WriteLine( "        Simias is to run in a server configuration." );
			Console.WriteLine( "        Default value: None" );
			Console.WriteLine();							
			Console.WriteLine( "    --datadir [Path to Simias data directory]:" );
			Console.WriteLine( "        Specifies the directory path where the Simias data will be stored." );
			Console.WriteLine( "        This parameter is required if --runasserver is specified." );
			Console.WriteLine( "        Default value: Shared simias data directory." );
			Console.WriteLine( "            Windows: \"[SystemDrive:]\\Documents and Settings\\[UserName]" );
			Console.WriteLine( "                       \\Local Settings\\Application Data\"" );
			Console.WriteLine( "            Linux and OSX: ~/.local/share" );
			Console.WriteLine( "        Example: --datadir \"/home/mlasky/simias/shared\"" );
			Console.WriteLine();							
			Console.WriteLine( "    --noexec [debug]:" );
			Console.WriteLine( "        Does not detach new Simias.exe child process. Runs Simias services" );
			Console.WriteLine( "        from the current process. Must also specify --port and --datadir" );
			Console.WriteLine( "        when using this option. This is documented for debugging purposes" );
			Console.WriteLine( "        only. The debug flag forces the port to be written to the configuration" );
			Console.WriteLine( "        file." );
			Console.WriteLine( "        Default: None" );
			Console.WriteLine();							
			Console.WriteLine( "    --showconsole:" );
			Console.WriteLine( "        Displays a console for viewing console output from Simias. Has no" );
			Console.WriteLine( "        effect if --noexec is specified." );
			Console.WriteLine( "        Default: None" );
			Console.WriteLine();
			Console.WriteLine( "    --verbose:" );
			Console.WriteLine( "        Prints extra messages to stderr. Useful for debugging purposes." );
			Console.WriteLine( "        Default: None" );
			Console.WriteLine();
			Console.WriteLine( "    --start:" );
			Console.WriteLine( "        Starts Simias services if they are not currently running on the" );
			Console.WriteLine( "        specified --port and --datadir. This is the default command that will" );
			Console.WriteLine( "        be run, if no other command is specified." );
			Console.WriteLine();							
			Console.WriteLine( "    --stop:" );
			Console.WriteLine( "        Stops Simias services from executing if no other applications are" );
			Console.WriteLine( "        referencing Simias services. Must specify --datadir to stop a specified" );
			Console.WriteLine( "        Simias process. To stop the shared Simias services, do not specify" );
			Console.WriteLine( "        --datadir." );
			Console.WriteLine();							
			Console.WriteLine( "    --kill [ all ]:" );
			Console.WriteLine( "        Stops Simias services immediately, even if other applications are" );
			Console.WriteLine( "        referencing Simias services. Must specify --datadir to stop a specified" );
			Console.WriteLine( "        Simias process immediately. To stop the shared Simias services" );
			Console.WriteLine( "        immediately, do not specify --datadir. To stop all Simias services" );
			Console.WriteLine( "        immediately, specify the 'all' value." );
			Console.WriteLine();
			Console.WriteLine( "    --info:" );
			Console.WriteLine( "        Displays information regarding the currently executing Simias" );
			Console.WriteLine( "        processes." );
			Console.WriteLine();							
			Console.WriteLine( "    --version:" );
			Console.WriteLine( "        Displays the version information and exits." );
			Console.WriteLine();							
			Console.WriteLine( "    --help:" );
			Console.WriteLine( "        Displays this help for Simias.exe." );
			Console.WriteLine();
		}

		/// <summary>
		/// Displays information about the currently executing Simias processes.
		/// </summary>
		private void ShowInfo()
		{
			// Find all of the temp Simias_* files.
			string[] simiasFiles = Directory.GetFiles( Path.GetTempPath(), "Simias_*" );
			if ( simiasFiles.Length > 0 )
			{
				// Kill the process for each of the files in the list.
				foreach ( string file in simiasFiles )
				{
					// Convert the file name to a Simias data directory.
					string dataPath = GetProcessFileName( file );

					// See if there is a port configured in the specified data area.
					int port = ReadXspPortFromFile( dataPath );
					if ( port != -1 )
					{
						// Build the URI for web services.
						UriBuilder ub = new UriBuilder( Uri.UriSchemeHttp, IPAddress.Loopback.ToString(), port, VirtualPath );

						// There has been a port configured previously. Check to see if the Simias services 
						// are already running on this port or range.
						if ( PingWebService( ub.Uri, false ) && IsSameService( ub.Uri, dataPath ) )
						{
							int refCount = AddReference( ub.Uri, dataPath );
							try
							{
								Process process = GetSimiasProcess( ub.Uri, dataPath );

								Console.WriteLine( "Simias process:   {0}", process.Id );
								Console.WriteLine( "Data directory:   {0}", dataPath );
								Console.WriteLine( "Web Service Uri:  {0}", ub.Uri );

								if ( verbose )
								{
									Console.WriteLine( "Reference count:  {0}", refCount - 1 );
									Console.WriteLine( "Start time:       {0}", process.StartTime );
									Console.WriteLine( "{0}", process.MainModule.FileVersionInfo );
								}

								Console.WriteLine();
							}
							finally
							{
								RemoveReference( ub.Uri, dataPath );
							}
						}
						else
						{
							Console.WriteLine( "Simias process terminated abnormally." );
							Console.WriteLine( "Data directory:   {0}", dataPath );
							Console.WriteLine( "Web Service Uri:  {0}", ub.Uri );
						}
					}
					else
					{
						Console.WriteLine( "Simias process terminated abnormally." );
						Console.WriteLine( "No port information." );
						Console.WriteLine( "Data directory:   {0}", dataPath );
					}
				}
			}
			else
			{
				Console.WriteLine( "No Simias processes are running." );
			}
		}

		/// <summary>
		/// Shows the Simias.exe version information.
		/// </summary>
		private void ShowVersion()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			string version = assembly.GetName().Version.ToString();

			object[] att = assembly.GetCustomAttributes( typeof( AssemblyCopyrightAttribute ), false );
			string copyright = ( ( AssemblyCopyrightAttribute ) att[ 0 ] ).Copyright;

			Console.WriteLine ("{0} {1}\n{2}", Path.GetFileName( assembly.Location ), version, copyright );
		}

		/// <summary>
		/// Starts the server listening for messages from the XSP server.
		/// </summary>
		private void StartServerIpc()
		{
			Thread thread = new Thread( new ThreadStart( ServerMessageIpc ) );
			thread.IsBackground = true;
			thread.Start();
		}

		/// <summary>
		/// Creates a child process and executes Simias.exe in that process with the specified
		/// configuration.
		/// </summary>
		/// <returns>SimiasStatus</returns>
		private SimiasStatus StartSimias()
		{
			SimiasStatus status = SimiasStatus.Success;

			if ( noExec )
			{
				// Either this is the Simias child process or --noexec was specified on the command line
				// for the controller process.
				status = StartSimiasServer();
			}
			else
			{
				// See if there is a port configured in the specified data area.
				int port = ReadXspPortFromFile();
				if ( port != -1 )
				{
					// Build the URI for web services.
					UriBuilder ub = new UriBuilder( Uri.UriSchemeHttp, IPAddress.Loopback.ToString(), port, VirtualPath );

					// There has been a port configured previously. Check to see if the Simias services 
					// are already running on this port or range.
					if ( PingWebService( ub.Uri, false ) && CanShareSimiasService( ub.Uri ) )
					{
						// There is already services running on this port. If there was a port specified
						// on the command line, see if it is the same port or in the same port range.
						if ( IsPortInRange( port ) )
						{
							// Increment the reference count for this instance, so that another application
							// won't shut down this process on us.
							AddReference( ub.Uri );

							// The service is already running, don't start a new one, just use the old one.
							Console.WriteLine( ub.Uri );
							Console.WriteLine( simiasDataPath );
						}
						else
						{
							// The service is already running in the specified data area, but the ports are
							// in conflict. Return an error.
							Console.Error.WriteLine( "Error: The service is not available on the specified port or range." );
							status = SimiasStatus.ServiceNotAvailable;
						}
					}
					else
					{
						// There either is no service running, the database paths point to different directories or
						// The service is running in the wrong configuration ( client vs. server ). See if the port
						// read from the file is available and start a new service.
						port = GetXspPort( portRangeString );
						if ( port != -1 )
						{
							status = StartSimiasChild( port );
						}
						else
						{
							Console.Error.WriteLine( "Error: No ports are available." );
							status = SimiasStatus.NoAvailablePorts;
						}
					}
				}
				else
				{
					// No port has been configured for this data area.
					port = GetXspPort( portRangeString );
					if ( port != -1 )
					{
						// Start the child service.
						status = StartSimiasChild( port );
					}
					else
					{
						// No ports available.
						if ( portRangeString == null )
						{
							Console.Error.WriteLine( "There are no available ports in the dynamic range." );
						}
						else
						{
							Console.Error.WriteLine( "Port or Range: {0} is not available.", portRangeString );
						}

						status = SimiasStatus.NoAvailablePorts;
					}
				}
			}

			return status;
		}

		/// <summary>
		/// Starts the Simias.exe child process that will run detached and provide the Simias services.
		/// </summary>
		/// <param name="port">Port number to pass a command line argument.</param>
		/// <returns>SimiasStatus</returns>
		private SimiasStatus StartSimiasChild( int port )
		{
			SimiasStatus status = SimiasStatus.Success;

			// Get a dynamic port to use as the IPC between application domains.
			int ipcPort = AvailablePortCheck( 0 );
			if ( ipcPort != -1 )
			{
				try
				{
					// Set up the process info to start the Simias server process.
					Process serverProcess = new Process();

					// Setup the child process information.
					serverProcess.StartInfo.FileName = MyEnvironment.DotNet ? ApplicationPath : "mono";
					serverProcess.StartInfo.UseShellExecute = false;
					serverProcess.StartInfo.CreateNoWindow = !showConsole ? true : false;
					serverProcess.StartInfo.RedirectStandardOutput = true;
					serverProcess.StartInfo.Arguments = 
						String.Format(
						"{0}--port {1} --ipcport {2} {3}--datadir \"{4}\" --noexec --start{5}", 
						MyEnvironment.DotNet ? String.Empty : ApplicationPath + " ",
						port,
						ipcPort,
						runAsServer ? "--runasserver " : String.Empty,
						simiasDataPath,
						verbose ? " --verbose" : String.Empty );

					serverProcess.Start();

					// Wait for the web service uri to be written to the child's stdout descriptor.
					// Then write it to the parent's stdout descriptor.
					Console.WriteLine( serverProcess.StandardOutput.ReadLine() );
					Console.WriteLine( serverProcess.StandardOutput.ReadLine() );
				}
				catch
				{
					status = SimiasStatus.ProcessFailure;
				}
			}
			else
			{
				Console.Error.WriteLine( "Error: Cannot allocate an IPC socket." );
				status = SimiasStatus.NoAvailablePorts;
			}

			return status;
		}

		/// <summary>
		/// Starts the XSP web service that provides Simias services.
		/// </summary>
		/// <returns>SimiasStatus</returns>
		private SimiasStatus StartSimiasServer()
		{
			SimiasStatus status = SimiasStatus.Success;

			// If the data directory has not been created, do it now.
			if ( !Directory.Exists( simiasDataPath ) )
			{
				Directory.CreateDirectory( simiasDataPath );
			}

			// Get the port to listen on.
			int[] range = GetPortRange( portRangeString );
			if ( range != null )
			{
				// Build the URI for web services and write it to stdout.
				UriBuilder ub = new UriBuilder( Uri.UriSchemeHttp, IPAddress.Loopback.ToString(), range[ 0 ], VirtualPath );

				// If the debug flag is set, write the port to the configuration file.
				if ( debug )
				{
					WriteXspPortToFile( ub.Port );
				}

				// Split the application path into root and web paths.
				string appPath = ( MyEnvironment.Platform != MyPlatformID.Windows ) ? ApplicationPath : ApplicationPath.ToLower();
				int index =  appPath.LastIndexOf( "web" );
				if ( index != -1 )
				{
					// Get the parent directory to the "web/bin" directory.
					string rootPath = ApplicationPath.Substring( 0, index ).TrimEnd( new char[] { Path.DirectorySeparatorChar } );

					// Build the argument list for the Xsp server.
					ArrayList args = new ArrayList();
					args.Add( "--root" );
					args.Add( rootPath );
					args.Add( "--applications" );
					args.Add( String.Format( "{0}:{1}", ub.Uri.AbsolutePath, ApplicationPath.Substring( index, 3 ) ) );
					args.Add( "--port" );
					args.Add( ub.Port.ToString() );
					args.Add( "--nonstop" );

					if ( verbose )
					{
						args.Add( "--verbose" );
					}

					// Start the Xsp server.
					Server.Start( args.ToArray( typeof( string ) ) as string[] );

					// Start the IPC mechanism listening for messages.
					StartServerIpc();

					// Wait for the server listener to start.
					Thread.Sleep( 100 );
					PingWebService( ub.Uri, true );

					// Write out the web service uri and data store path to stdout.
					Console.WriteLine( ub.Uri );
					Console.WriteLine( simiasDataPath );

					// Increment the reference count for this instance, so that another application
					// won't shut down this process on us.
					AddReference( ub.Uri );

					// Create a temporary file to provide information about this process.
					CreateProcessFile();

					// Wait for a message to stop the server.
					WaitForShutdown();

					// Get rid of the temporary file.
					DeleteProcessFile();

					// Stop the server IPC mechanism.
					StopServerIpc();

					// Stop the server before exiting.
					Server.Stop();
				}
				else
				{
					Console.Error.WriteLine( "Error: Invalid path for Simias web services: {0}", ApplicationPath );
					status = SimiasStatus.InvalidSimiasApplicationPath;
				}
			}
			else
			{
				Console.Error.WriteLine( "Error: No listening port specified on the command line." );
				status = SimiasStatus.InvalidCommandLine;
			}

			return status;
		}

		/// <summary>
		/// Stops the server listening for messages from the XSP server.
		/// </summary>
		private void StopServerIpc()
		{
			ipcIsClosed = true;

			if ( ipcServerSocket != null )
			{
				try
				{
					ipcServerSocket.Shutdown( SocketShutdown.Both );
				}
				catch
				{}

				ipcServerSocket.Close();
				ipcServerSocket = null;
			}

			if ( ipcMessageSocket != null )
			{
				try
				{
					ipcMessageSocket.Shutdown( SocketShutdown.Receive );
				}
				catch
				{}

				ipcMessageSocket.Close();
				ipcMessageSocket = null;
			}
		}

		/// <summary>
		/// Stops the simias services for the specified --datadir if no other applications are referencing
		/// the Simias services.
		/// </summary>
		/// <returns>SimiasStatus</returns>
		private SimiasStatus StopSimias()
		{
			SimiasStatus status = SimiasStatus.Success;

			// See if there is a port configured in the specified data area.
			int port = ReadXspPortFromFile();
			if ( port != -1 )
			{
				// Build the URI for web services.
				UriBuilder ub = new UriBuilder( Uri.UriSchemeHttp, IPAddress.Loopback.ToString(), port, VirtualPath );

				// There has been a port configured previously. Check to see if the Simias services 
				// are already running on this port or range.
				if ( PingWebService( ub.Uri, false ) && IsSameService( ub.Uri ) )
				{
					if ( RemoveReference( ub.Uri ) == -1 )
					{
						Console.Error.WriteLine( "Error: Cannot contact {0} to shutdown the service.", ub.Uri );
						status = SimiasStatus.ServiceNotAvailable;
					}
				}
				else
				{
					Console.Error.WriteLine( "Error: No service found for {0}.", simiasDataPath );
					status = SimiasStatus.ServiceNotAvailable;
				}
			}

			return status;
		}

		/// <summary>
		/// Waits for a shutdown message from the XSP server that runs in a different application
		/// domain.
		/// </summary>
		private void WaitForShutdown()
		{
			// Wait for the server to indicate to shutdown.
			stopServerEvent.WaitOne();
		}

		/// <summary>
		/// Writes the port that Xsp is to listen on to a configuration file.
		/// </summary>
		/// <param name="port">Port to write to the configuration file.</param>
		private void WriteXspPortToFile( int port )
		{
			// Get the host and port that the server is listening on and put it into an XML document.
			XmlDocument document = new XmlDocument();
			document.AppendChild( document.CreateElement( PortConfigurationTag ) );

			// Create the port node.
			XmlElement portElement = document.CreateElement( PortTag );
			portElement.InnerText = port.ToString();
			document.DocumentElement.AppendChild( portElement );

			// See if the local application data directory has been created.
			if ( !Directory.Exists( simiasDataPath ) )
			{
				Directory.CreateDirectory( simiasDataPath );
			}

			// Write the port number to the configuration file.
			string configFileName = Path.Combine( simiasDataPath, PortConfigurationFileName );
			using( FileStream fs = File.Open( configFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None ) )
			{
				document.Save( fs );
			}
		}
	
		#endregion

		#region Main

		/// <summary>
		/// Program entry point.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		/// <returns>0 >= Successful, otherwise an error is indicated.</returns>
		public static int Main( string[] args )
		{
			SimiasWebServer server = new SimiasWebServer();
			SimiasStatus status = server.ParseConfigurationParameters( args );
			if ( status == SimiasStatus.Success )
			{
				switch ( server.command )
				{
					case SimiasCommand.Help:
					{
						server.ShowHelp();
						break;
					}

					case SimiasCommand.Kill:
					{
						server.KillSimias();
						break;
					}

					case SimiasCommand.KillAll:
					{
						server.KillAllSimias();
						break;
					}

					case SimiasCommand.Start:
					{
						// If this is a controller process, acquire the controller mutex so 
						// that this process is the only Simias controller running.
						bool acquired = ( server.noExec == false ) ? AcquireControllerMutex() : true;
						if ( acquired )
						{
							try
							{
								server.StartSimias();
							}
							finally
							{
								// If this is a controller process, release the mutex to let other controllers
								// run.
								if ( server.noExec == false )
								{
									ReleaseControllerMutex();
								}
							}
						}
						else
						{
							// Timed out waiting for other controller processes to finish.
							Console.Error.WriteLine( "Error: Timed out waiting for other controller processes." );
							status = SimiasStatus.ControllerTimeout;
						}

						break;
					}

					case SimiasCommand.Stop:
					{
						server.StopSimias();
						break;
					}

					case SimiasCommand.Info:
					{
						server.ShowInfo();
						break;
					}

					case SimiasCommand.Version:
					{
						server.ShowVersion();
						break;
					}
				}
			}

			return ( int )status;
		}

		#endregion
	}
}