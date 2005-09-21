/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright © Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/
using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;
using System.IO;
using System.Threading;

using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.Event;

namespace Simias.Web
{
	/// <summary>
	/// Summary description for Global.
	/// </summary>
	public class Global : HttpApplication
	{
		#region Class Members

		/// <summary>
		/// Environment variables used by apache.
		/// </summary>
		private static string EnvSimiasRunAsServer = "SimiasRunAsServer";
		private static string EnvSimiasDataPath = "SimiasDataPath";
		private static string EnvSimiasVerbose = "SimiasVerbose";
		
		/// <summary>
		/// Object used to manage the simias services.
		/// </summary>
		static private Simias.Service.Manager serviceManager;
		
		/// <summary>
		/// A thread to keep the application alive long enough to close Simias.
		/// </summary>
		private Thread keepAliveThread;

		/// <summary>
		/// Quit the application flag.
		/// </summary>
		private bool quit;

		/// <summary>
		/// Event used to tell the controlling server process to shut us down.
		/// </summary>
		private static ManualResetEvent shutDownEvent = new ManualResetEvent( false );

		/// <summary>
		/// Specifies whether to run as a client or server.
		/// </summary>
		private static bool runAsServer = false;

		/// <summary>
		/// Prints extra data for debugging purposes.
		/// </summary>
		private static bool verbose = false;

		/// <summary>
		/// Path to the simias data area.
		/// </summary>
		private static string simiasDataPath = null;

		#endregion

		#region Constructor

		/// <summary>
		/// Static constructor that starts only one shutdown thread.
		/// </summary>
		static Global()
		{
			// Check to see if there is an environment variable set to run as a server.
			// If so then we are being started by apache and there are no command line
			// parameters available.
			if ( !ParseEnvironmentVariables() )
			{
				// Parse the command line parameters.
				ParseConfigurationParameters( Environment.GetCommandLineArgs() );
			}

			// Make sure that there is a data path specified.
			if ( simiasDataPath == null )
			{
				ApplicationException apEx = new ApplicationException( "The Simias data path was not specified." );
				Console.Error.WriteLine( apEx.Message );
				throw apEx;
			}

			// Start the shutdown thread running waiting to signal a shutdown.
			Thread thread = new Thread( new ThreadStart( ShutDownThread ) );
			thread.IsBackground = true;
			thread.Start();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Parses the command line parameters to get the configuration for Simias.
		/// </summary>
		/// <param name="args">Command line parameters.</param>
		private static void ParseConfigurationParameters( string[] args )
		{
			for ( int i = 0; i < args.Length; ++i )
			{
				Console.Error.WriteLine( "Args: {0}", args[ i ] );

				switch ( args[ i ].ToLower() )
				{
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
							ApplicationException apEx = new ApplicationException( "Error: The Simias data path was not specified." );
							Console.Error.WriteLine( apEx.Message );
							throw apEx;
						}

						break;
					}

					case "--verbose":
					{
						verbose = true;
						break;
					}
				}
			}
		}

		/// <summary>
		/// Gets the Simias environment variables set by mod-mono-server in the apache process.
		/// </summary>
		/// <returns>True if the environment variables were found. Otherwise false is returned.</returns>
		private static bool ParseEnvironmentVariables()
		{
			bool foundEnv = false;

			if ( Environment.GetEnvironmentVariable( EnvSimiasRunAsServer ) != null )
			{
				runAsServer = true;
				simiasDataPath = Environment.GetEnvironmentVariable( EnvSimiasDataPath );
				if ( simiasDataPath == null )
				{
					ApplicationException apEx = new ApplicationException( "Error: The Simias data path was not specified." );
					Console.Error.WriteLine( apEx.Message );
					throw apEx;
				}

				verbose = ( Environment.GetEnvironmentVariable( EnvSimiasVerbose ) != null ) ? true : false;
				foundEnv = true;
			}

			return foundEnv;
		}

		/// <summary>
		/// Signals a named mutex that the controlling server process is blocked on to
		/// cause the server to shutdown the web service. The reason that this must
		/// happen within a thread is that the mutex can only be released by the thread
		/// that owns it. Since the web services can come in on different threads, the
		/// the thread that signals the mutex may not be the owner.
		/// </summary>
		private static void ShutDownThread()
		{
			string mutexName = simiasDataPath.Replace( '\\', '/' );

			// Create and own the mutex that the controlling server process will wait on.
			Mutex mutex = new Mutex( true, "SimiasExitProcessMutex_" + mutexName );

			// Wait for the shutdown event to be signaled before releasing the mutex.
			shutDownEvent.WaitOne();

			// Tell the controlling server process to shut us down.
			mutex.ReleaseMutex();
			mutex.Close();
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Application_Start
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_Start(Object sender, EventArgs e)
		{
			// update the prefix of the installed directory
			SimiasSetup.prefix = Path.Combine(Server.MapPath(null), "..");
			Environment.CurrentDirectory = SimiasSetup.webbindir;

			if ( verbose )
			{
				Console.Error.WriteLine("Simias Application Path: {0}", Environment.CurrentDirectory);
				Console.Error.WriteLine("Simias Data Path: {0}", simiasDataPath);
				Console.Error.WriteLine("Run in {0} configuration", runAsServer ? "server" : "client" );
			}

			Simias.Storage.Store.GetStore();

			if ( verbose )
			{
				Console.Error.WriteLine("Simias Process Starting");
			}

			serviceManager = Simias.Service.Manager.GetManager();
			serviceManager.StartServices();
			serviceManager.WaitForServicesStarted();

			// Send the simias up event.
			EventPublisher eventPub = new EventPublisher();
			eventPub.RaiseEvent( new NotifyEventArgs("Simias-Up", "The simias service is running", DateTime.Now) );

			if ( verbose )
			{
				Console.Error.WriteLine("Simias Process Running");
			}

			// start keep alive
			// NOTE: We have seen a FLAIM corruption because the database was not given
			// the opportunity to close on shutdown.  The solution is to work with
			// Dispose() methods with the native calls and to control our own life cycle
			// (which in a web application is difficult). It would mean separating the
			// database application from the web application, which is not very practical
			// today.  We can bend the rules in the web application by using a foreground
			// thread. This is a brittle solution, but it seems to work today.
			keepAliveThread = new Thread(new ThreadStart(this.KeepAlive));
			quit = false;
			keepAliveThread.Start();
		}
 
		/// <summary>
		/// Keep Alive Thread
		/// </summary>
		protected void KeepAlive()
		{
			TimeSpan span = TimeSpan.FromSeconds(1);
			
			while(!quit)
			{
				Thread.Sleep(span);
			}
		}

		/// <summary>
		/// Session_Start
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Session_Start(Object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Application_BeginRequest
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_BeginRequest(Object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Application_EndRequest
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_EndRequest(Object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Application_AuthenticateRequest
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_AuthenticateRequest(Object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Application_Error
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_Error(Object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Session_End
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Session_End(Object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Application_End
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_End(Object sender, EventArgs e)
		{
			if ( verbose )
			{
				Console.Error.WriteLine("Simias Process Starting Shutdown");
			}

			if ( serviceManager != null )
			{
				// Send the simias down event and wait for 1/2 second for the message to be routed.
				EventPublisher eventPub = new EventPublisher();
				eventPub.RaiseEvent( new NotifyEventArgs("Simias-Down", "The simias service is terminating", DateTime.Now) );
				Thread.Sleep( 500 );

				serviceManager.StopServices();
				serviceManager.WaitForServicesStopped();
				serviceManager = null;
			}

			if ( verbose )
			{
				Console.Error.WriteLine("Simias Process Shutdown");
			}

			// end keep alive
			// NOTE: an interrupt or abort here is currently causing a hang on Mono
			quit = true;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Causes the controlling server process to shut down the web services.
		/// </summary>
		public static void SimiasProcessExit()
		{
			shutDownEvent.Set();
		}

		#endregion
	}
}

