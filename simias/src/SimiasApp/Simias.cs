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
using System.Threading;
using System.Web;
using System.Xml;

using Simias.Client;

namespace Mono.ASPNET
{
	/// <summary>
	/// Implements the web server functionality for Simias.
	/// </summary>
	public class SimiasWebServer : IDisposable
	{
		#region Class Members

		/// <summary>
		/// Mutex used to force only one controlling process at a time to run.
		/// </summary>
		private static Mutex ControllerMutex = new Mutex( false, "SimiasControllerMutex" );
		private const int ControllerMutexTimeout = 15 * 1000;

		/// <summary>
		/// Path to running application. Remove the quotes that get added to the string.
		/// </summary>
		private static string ApplicationPath;

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
			///  Show version information.
			/// </summary>
			Version,

			/// <summary>
			/// Show help.
			/// </summary>
			Help
		}

		/// <summary>
		/// Object disposed flag.
		/// </summary>
		private bool disposed = false;

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
		private string portRange;

		/// <summary>
		/// Used shared Simias data directory.
		/// </summary>
		private string simiasDataPath = DefaultSimiasDataPath;

		/// <summary>
		/// Default command is Start if no other commands are specified.
		/// </summary>
		private SimiasCommand command = SimiasCommand.Start;

		#endregion

		#region Properties

		/// <summary>
		/// Builds a unique mutex name for the Simias process.
		/// </summary>
		private string ExitMutexName
		{
			get { return "SimiasExitProcessMutex_" + simiasDataPath.Replace( '\\', '/' ); }
		}

		/// <summary>
		/// Returns whether the current OS is Windows.
		/// </summary>
		private bool IsWindows
		{
			get 
			{ 
				switch ( Environment.OSVersion.Platform )
				{
					case PlatformID.Win32NT:
					case PlatformID.Win32S:
					case PlatformID.Win32Windows:
					case PlatformID.WinCE:
					{
						return true;
					}

					default:
					{
						return false;
					}
				}
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

			int[] range = GetPortRange( portRange );
			if ( range != null )
			{
				if ( ( port >= range[ 0 ] ) && ( port <= range[ range.Length - 1 ] ) )
				{
					isInRange = true;
				}
			}

			return isInRange;
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
							portRange = args[ ++i ];
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

					case "--noexec":
					{
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

					case "--version":
					{
						if ( command != SimiasCommand.Start )
						{
							// A different command was specified earlier. Show an error.
							Console.WriteLine( "Invalid command line parameters. More than one command was specified." );
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

			return status;
		}

		/// <summary>
		/// Pings the Simias web service to start the server running.
		/// </summary>
		/// <param name="uri">The URI of the web service to ping.</param>
		/// <returns>True if ping was successful, otherwise false is returned.</returns>
		private bool PingWebService( Uri uri )
		{
			bool pingStatus = false;

			try
			{
				SimiasWebService svc = new SimiasWebService();
				svc.Url = uri.ToString() + "/Simias.asmx";

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
		/// Gets the port from the configuration file.
		/// </summary>
		/// <returns>Port number if successful, otherwise -1 is returned.</returns>
		private int ReadXspPortFromFile()
		{
			int port = -1;

			try
			{
				XmlDocument document = new XmlDocument();
				document.Load( Path.Combine( simiasDataPath, PortConfigurationFileName ) );
				port = Convert.ToInt32( document.DocumentElement[ PortTag ].InnerText );
			}
			catch
			{}

			return port;
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
			Console.WriteLine( "    --noexec:" );
			Console.WriteLine( "        Does not detach new Simias.exe child process. Runs Simias services" );
			Console.WriteLine( "        from the current process. This is documented for debugging purposes" );
			Console.WriteLine( "        only." );
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
			Console.WriteLine( "    --version:" );
			Console.WriteLine( "        Displays the version information and exits." );
			Console.WriteLine();							
			Console.WriteLine( "    --help:" );
			Console.WriteLine( "        Displays this help for Simias.exe." );
			Console.WriteLine();
		}

		/// <summary>
		/// Shows the Simias.exe version information.
		/// </summary>
		private void ShowVersion()
		{
			AssemblyName name = Assembly.GetExecutingAssembly().GetName();
			Console.WriteLine( name.FullName );
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
				StartSimiasServer();
			}
			else
			{
				// Virtual path.
				string virtualPath = runAsServer ? "/simias10" : "/simias10/" + Environment.UserName;

				// See if there is a port configured in the specified data area.
				int port = ReadXspPortFromFile();
				if ( port != -1 )
				{
					// Build the URI for web services.
					UriBuilder ub = new UriBuilder( Uri.UriSchemeHttp, IPAddress.Loopback.ToString(), port, virtualPath );

					// There has been a port configured previously. Check to see if the Simias services 
					// are already running on this port or range.
					if ( PingWebService( ub.Uri ) && CanShareSimiasService( ub.Uri ) )
					{
						// There is already services running on this port. If there was a port specified
						// on the command line, see if it is the same port or in the same port range.
						if ( IsPortInRange( port ) )
						{
							// The service is already running, don't start a new one, just use the old one.
							Console.WriteLine( ub.Uri.ToString() );
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
						port = GetXspPort( portRange );
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
					port = GetXspPort( portRange );
					if ( port != -1 )
					{
						// Start the child service.
						status = StartSimiasChild( port );
					}
					else
					{
						// No ports available.
						if ( portRange == null )
						{
							Console.Error.WriteLine( "There are no available ports in the dynamic range." );
						}
						else
						{
							Console.Error.WriteLine( "Port or Range: {0} is not available.", portRange );
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

			try
			{
				// Set up the process info to start the Simias server process.
				Process serverProcess = new Process();

				// Setup the child process information.
				serverProcess.StartInfo.FileName = ApplicationPath;
				serverProcess.StartInfo.UseShellExecute = false;
				serverProcess.StartInfo.CreateNoWindow = !showConsole ? true : false;
				serverProcess.StartInfo.RedirectStandardOutput = true;
				serverProcess.StartInfo.Arguments = 
					String.Format(
						"--port {0} {1}--datadir \"{2}\" --noexec --start {3}", 
						port,
						runAsServer ? "--runasserver " : String.Empty,
						simiasDataPath,
						verbose ? " --verbose" : String.Empty );

				serverProcess.Start();

				// Wait for the web service uri to be written to the child's stdout descriptor.
				// Then write it to the parent's stdout descriptor.
				Console.WriteLine( serverProcess.StandardOutput.ReadLine() );
			}
			catch
			{
				status = SimiasStatus.ProcessFailure;
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

			// Get the port to listen on.
			int[] range = GetPortRange( portRange );
			if ( range != null )
			{
				// Build the virtual server path.
				string virtualPath = runAsServer ? "/simias10" : "/simias10/" + Environment.UserName;

				// Build the URI for web services and write it to stdout.
				UriBuilder ub = new UriBuilder( Uri.UriSchemeHttp, IPAddress.Loopback.ToString(), range[ 0 ], virtualPath );

				// Split the application path into root and web paths.
				int index = ApplicationPath.LastIndexOf( "web" );
				if ( index != -1 )
				{
					// Get the parent directory to the "web/bin" directory.
					string rootPath = ApplicationPath.Substring( 0, index ).TrimEnd( new char[] { Path.DirectorySeparatorChar } );

					// See which platform we are running on.
					ArrayList args = new ArrayList();
					if ( !IsWindows )
					{
						// mono requires the application path as it's first argument.
						args.Add( ApplicationPath );
					}

					// Build the argument list for the Xsp server.
					args.Add( "--root" );
					args.Add( rootPath );
					args.Add( "--applications" );
					args.Add( String.Format( "{0}:web", ub.Uri.AbsolutePath ) );
					args.Add( "--port" );
					args.Add( ub.Port.ToString() );
					args.Add( "--nonstop" );

					if ( verbose )
					{
						args.Add( "--verbose" );
					}

					// Start the Xsp server.
					Server.Start( args.ToArray( typeof( string ) ) as string[] );

					// Wait for the server listener to start.
					Thread.Sleep( 100 );
					PingWebService( ub.Uri );
					Console.WriteLine( ub.Uri );

					// Initialize the process end mutex and wait for it to become signalled.
					Mutex mutex = new Mutex( false, ExitMutexName );
					mutex.WaitOne();

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
		/// Stops the simias services for the specified --datadir if no other applications are referencing
		/// the Simias services.
		/// </summary>
		/// <returns>SimiasStatus</returns>
		private SimiasStatus StopSimias()
		{
			SimiasStatus status = SimiasStatus.Success;

			// Virtual path.
			string virtualPath = runAsServer ? "/simias10" : "/simias10/" + Environment.UserName;

			// See if there is a port configured in the specified data area.
			int port = ReadXspPortFromFile();
			if ( port != -1 )
			{
				// Build the URI for web services.
				UriBuilder ub = new UriBuilder( Uri.UriSchemeHttp, IPAddress.Loopback.ToString(), port, virtualPath );

				// There has been a port configured previously. Check to see if the Simias services 
				// are already running on this port or range.
				if ( PingWebService( ub.Uri ) && CanShareSimiasService( ub.Uri ) )
				{
					SimiasWebService svc = new SimiasWebService();
					svc.Url = ub.Uri.ToString() + "/Simias.asmx";
					Simias.Client.LocalService.Start( svc, ub.Uri, simiasDataPath );
					svc.StopSimiasProcess();
				}
			}

			return status;
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
			SimiasStatus status = SimiasStatus.Success;
			SimiasWebServer server = new SimiasWebServer();
			try
			{
				status = server.ParseConfigurationParameters( args );
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
							break;
						}

						case SimiasCommand.KillAll:
						{
							break;
						}

						case SimiasCommand.Start:
						{
							// If this is a controller process, acquire the controller mutex so 
							// that this process is the only Simias controller running.
							bool acquired = ( server.noExec == false ) ? 
								ControllerMutex.WaitOne( ControllerMutexTimeout, false ) : true;

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
										ControllerMutex.ReleaseMutex();
									}
								}
							}
							else
							{
								// Timed out waiting for other controller processes to finish.
								Console.WriteLine( "Error: Timed out waiting for other controller processes." );
								status = SimiasStatus.ControllerTimeout;
							}

							break;
						}

						case SimiasCommand.Stop:
						{
							server.StopSimias();
							break;
						}

						case SimiasCommand.Version:
						{
							server.ShowVersion();
							break;
						}
					}
				}
			}
			finally
			{
				// Don't wait for the GC to clean up.
				server.Dispose();
			}

			return ( int )status;
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Allows for quick release of managed and unmanaged resources.
		/// Called by applications.
		/// </summary>
		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		/// Dispose( bool disposing ) executes in two distinct scenarios.
		/// If disposing equals true, the method has been called directly
		/// or indirectly by a user's code. Managed and unmanaged resources
		/// can be disposed.
		/// If disposing equals false, the method has been called by the 
		/// runtime from inside the finalizer and you should not reference 
		/// other objects. Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing">Specifies whether called from the finalizer or from the application.</param>
		private void Dispose( bool disposing )
		{
			// Check to see if Dispose has already been called.
			if ( !disposed )
			{
				// Protect callers from accessing the freed members.
				disposed = true;

				// If disposing equals true, dispose all managed and unmanaged resources.
				if ( disposing )
				{
					// Dispose managed resources.
					ControllerMutex.Close();
				}
			}
		}
		
		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// It gives your base class the opportunity to finalize.
		/// Do not provide destructors in types derived from this class.
		/// </summary>
		~SimiasWebServer()      
		{
			Dispose( false );
		}

		#endregion
	}
}