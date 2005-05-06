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
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;

using Simias.Client.Event;

namespace Simias.Client
{
	/// <summary>
	/// System Manager
	/// </summary>
	public class Manager
	{
		#region Class Members
		/// <summary>
		/// XML configuation tags.
		/// </summary>
		private const string CFG_Section = "ServiceManager";
		private const string CFG_Services = "Services";
		private const string CFG_WebServicePath = "WebServicePath";
		private const string CFG_ShowOutput = "WebServiceOutput";
		private const string CFG_WebServiceUri = "WebServiceUri";
		private const string CFG_WebServicePortRange = "WebServicePortRange";

		static private Process webProcess = null;
		static private EventHandler appDomainUnloadEvent;
		static private IProcEventClient eventClient = null;
		#endregion

		#region Constructor
		// TODO: Remove this when the mono heap doesn't grow forever.
		/// <summary>
		/// Static constructor for the Manager class.
		/// </summary>
		static Manager()
		{
			// Setup an event listener waiting for a restart event.
			if ( MyEnvironment.Mono )
			{
				eventClient = new IProcEventClient( new IProcEventError( ErrorHandler ), null );
				eventClient.SetEvent( IProcEventAction.AddNotifyMessage, new IProcEventHandler( EventHandler ) );
				eventClient.Register();
			}
		}
		// TODO: End
		#endregion

		#region Properties
		/// <summary>
		/// Gets the path to the web service directory. Returns a null if the web service path
		/// does not exist.
		/// </summary>
		static public string LocalServicePath
		{
			get
			{
				Configuration config = new Configuration();
				return config.Get( CFG_Section, CFG_WebServicePath );
			}
		}

		/// <summary>
		/// Gets the port number to talk to the web service on. Returns a -1 if the web service port
		/// does not exist.
		/// </summary>
		static public int LocalServicePort
		{
			get
			{
				Configuration config = new Configuration();
				string uriString = config.Get( CFG_Section, CFG_WebServiceUri );
				return ( uriString != null ) ? new Uri( uriString ).Port : -1;
			}
		}

		/// <summary>
		/// Gets the local service url so that applications can talk to the local webservice.
		/// Returns a null if the local service url does not exist.
		/// </summary>
		static public Uri LocalServiceUrl
		{
			get
			{
				Configuration config = new Configuration();
				string uriString = config.Get( CFG_Section, CFG_WebServiceUri );
				return ( uriString != null ) ? new Uri( uriString ) : null;
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the path portion of the uri.
		/// </summary>
		/// <param name="uri">Uri to a web resource.</param>
		/// <returns>A string containing the virtual path.</returns>
		static private string GetVirtualPath( Uri uri )
		{
			string escString = uri.PathAndQuery;
			StringBuilder sb = new StringBuilder( escString.Length );
			for ( int i = 0; i < escString.Length; )
			{
				sb.Append( Uri.HexUnescape( escString, ref i ) );
			}

			return sb.ToString();
		}


		/// <summary>
		/// Callback that gets notified when the XSP process terminates.
		/// </summary>
		static private void XspProcessExited(object sender, EventArgs e)
		{
			lock( typeof( Manager ) )
			{
				if ( webProcess != null )
				{
					webProcess = null;
					Start();
				}
			}
		}

		/// <summary>
		/// Checks to see if the specified port is available.
		/// </summary>
		/// <param name="port">Port number to check for availability.</param>
		/// <returns>The port number if it is available. Otherwise a -1 is returned.</returns>
		static private int AvailablePortCheck( int port )
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
		/// Error handler for event listener.
		/// </summary>
		/// <param name="ex">The error that occurred.</param>
		/// <param name="context">Context</param>
		static private void ErrorHandler( ApplicationException ex, object context )
		{
			// Don't inform about any errors. The event service should heal itself.
		}

		/// <summary>
		/// Event handler that subscribes to NotifyEvents.
		/// </summary>
		/// <param name="args">Event data.</param>
		static private void EventHandler( SimiasEventArgs args )
		{
			NotifyEventArgs nea = args as NotifyEventArgs;
			if ( nea.EventData == "Simias-Restart" )
			{
				// Restart the SimiasApp.exe service.
				Stop();
				Start();
				Ping();
			}
		}

		/// <summary>
		/// Gets a specified range of ports to use as the local listener.
		/// </summary>
		/// <param name="config">Configuration object.</param>
		/// <returns>An array of integers that represent a range of TCP port numbers.
		/// A null is returned if no port range was specified.</returns>
		static private int[] GetPortRange( Configuration config )
		{
			int[] portRange = null;

			// See if there is a range specified.
			string rangeString = config.Get( CFG_Section, CFG_WebServicePortRange );
			if ( rangeString != null )
			{
				// See if there is a range separator.
				int sep = rangeString.IndexOf( '-' );
				if ( sep != -1 )
				{
					// Get the start and end values.
					int start = Convert.ToInt32( rangeString.Substring( 0, sep ).Trim() );
					int end = Convert.ToInt32( rangeString.Substring( sep + 1 ).Trim() );

					// Make sure the range is valid.
					if ( end >= start )
					{
						// Fill the array with the range of port numbers.
						portRange = new int[ ( end - start ) + 1 ];
						for ( int i = start; i <= end; ++i )
						{
							portRange[ i - start ] = i;
						}
					}
					else
					{
						throw new ApplicationException( "An invalid port range was specified." );
					}
				}
				else
				{
					// No range was specified, just a single port.
					portRange = new int[ 1 ] { Convert.ToInt32( rangeString.Trim() ) };
				}
			}

			return portRange;
		}

		/// <summary>
		/// Gets a port to use to start the web server.
		/// </summary>
		static private int GetXspPort( Configuration config )
		{
			// See if there is a port range specified.
			int[] portRange = GetPortRange( config );
			if ( portRange == null )
			{
				// Just use a single dynamic port.
				portRange = new int[ 1 ] { 0 };
			}

			// Loop through looking for an available port.
			foreach( int port in portRange )
			{
				// Make sure that the socket is available.
				int boundPort = AvailablePortCheck( port );
				if ( boundPort == -1 )
				{
					continue;
				}

				return boundPort;
			}

			// No available port could be found.
			throw new ApplicationException( "No ports available" );
		}

		/// <summary>
		/// Starts the simias web service.
		/// </summary>
		static private void Ping()
		{
			SimiasWebService service = new SimiasWebService();
			service.Url = Manager.LocalServiceUrl + "/Simias.asmx";

			// Stay in the ping loop until the service comes up successfully.
			bool serviceStarted = false;
			while ( !serviceStarted )
			{
				try
				{
					service.PingSimias();
					serviceStarted = true;
				}
				catch
				{
					Thread.Sleep( 100 );
				}
			}
		}

		/// <summary>
		/// Sets the specified URI in the configuration file. Normally, applications outside of the Simias
		/// web service process should not modify the configuration file. It is okay to do so here because
		/// the Simias process has not been started yet.
		/// </summary>
		static public void SetWebServiceUri( Configuration config, Uri uri )
		{
			bool updatedFile = false;
			XmlDocument document = new XmlDocument();
			document.Load( config.ConfigPath );

			foreach ( XmlElement section in document.DocumentElement )
			{
				// Only look at section nodes for the ServiceManager section.
				if ( ( section.Name == Configuration.SectionTag ) && ( section.GetAttribute( Configuration.NameAttr ) == CFG_Section ) )
				{
					XmlElement uriElement = null;
					foreach( XmlElement setting in section )
					{
						// Now look for an existing element for the uri property.
						if ( ( setting.Name == Configuration.SettingTag ) && ( setting.GetAttribute( Configuration.NameAttr ) == CFG_WebServiceUri ) )
						{
							uriElement = setting;
							break;
						}
					}

					// Check to see if an existing element was found.
					if ( uriElement == null )
					{
						uriElement = document.CreateElement( Configuration.SettingTag );
						uriElement.SetAttribute( Configuration.NameAttr, CFG_WebServiceUri );
						section.AppendChild( uriElement );
					}

					// Set the element value attribute.
					uriElement.SetAttribute( Configuration.ValueAttr, uri.ToString() );
					updatedFile = true;
					break;
				}
			}

			// Check to see if the file needs to be written back out.
			if ( updatedFile )
			{
				XmlTextWriter xtw = new XmlTextWriter(config.ConfigPath, Encoding.UTF8);
				try
				{
					xtw.Formatting = Formatting.Indented;
					document.WriteTo(xtw);
				}
				finally
				{
					xtw.Close();
				}
			}
			else
			{
				throw new ApplicationException( String.Format( "{0} did not get updated in {1}", CFG_WebServiceUri, config.ConfigPath ) );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Starts up the simias web service.
		/// </summary>
		static public void Start()
		{
			lock ( typeof( Manager ) )
			{
				// Make sure the process is not already started.
				if ( webProcess == null )
				{
					// Set up the process info to start the XSP process.
					webProcess = new Process();
					appDomainUnloadEvent = new EventHandler( XspProcessExited );
					webProcess.Exited += appDomainUnloadEvent;

					// Get the web service path from the configuration file.
					Configuration config = new Configuration();
					string webPath = config.Get( CFG_Section, CFG_WebServicePath );
					if ( webPath == null )
					{
						throw new ApplicationException( String.Format( "There is no {0} entry in {1}", CFG_WebServicePath, config.ConfigPath ) );
					}

					// Build a path to the web server application.
					string webApp = Path.Combine( webPath, String.Format( "bin{0}SimiasApp.exe", Path.DirectorySeparatorChar ) );

					webProcess.StartInfo.FileName = MyEnvironment.DotNet ? webApp : "mono";
					webProcess.StartInfo.UseShellExecute = false;
					webProcess.StartInfo.RedirectStandardInput = true;
					webProcess.StartInfo.CreateNoWindow = true;
					webProcess.EnableRaisingEvents = true;

					// See if process output is to be shown.
					string showOutput = config.Get( CFG_Section, CFG_ShowOutput );
					if ( ( showOutput != null ) && ( String.Compare( showOutput, "True", true ) == 0 ) )
					{
						webProcess.StartInfo.CreateNoWindow = false;
					}

					if ( !Path.IsPathRooted( webPath ) )
					{
						throw new ApplicationException( String.Format( "Web service path must be absolute: {0}", webPath ) );
					}

					// See if there is already a uri specified in the configuration file.
					Uri uri = null;
					string virtualRoot = null;

					string webUriString = config.Get( CFG_Section, CFG_WebServiceUri );
					if ( webUriString != null )
					{
						// Make sure that the specified port is available.
						UriBuilder ub = new UriBuilder( webUriString );
						if ( AvailablePortCheck( ub.Port ) == -1 )						
						{
							// The port is in use by another application. Allocate a new port.
							ub.Port = GetXspPort( config );

							// Set the uri with the new port back into the config file.
							SetWebServiceUri( config, ub.Uri );
						}
				
						uri = ub.Uri;
						virtualRoot = GetVirtualPath( uri );
					}
					else
					{
						// Get the dynamic port that xsp should use and write it out to the config file.
						virtualRoot = String.Format( "/simias10/{0}", Environment.UserName );
						uri = new Uri( new UriBuilder( "http", IPAddress.Loopback.ToString(), GetXspPort( config ), virtualRoot ).ToString() );
						SetWebServiceUri( config, uri );
					}

					// Strip off the volume if it exists and the file name and make the path absolute from the root.
					string appPath = String.Format( "{0}{1}", Path.DirectorySeparatorChar, webPath.Remove( 0, Path.GetPathRoot( webPath ).Length ) );
					webProcess.StartInfo.Arguments = String.Format( "{0} --applications \"{1}\":\"{2}\" --port {3}", MyEnvironment.DotNet ? String.Empty : "\"" + webApp + "\" ", virtualRoot, appPath, uri.Port.ToString() );
					webProcess.Start();
				}
			}
		}

		/// <summary>
		/// Shuts down the XSP process.
		/// </summary>
		static public void Stop()
		{
			lock ( typeof( Manager ) )
			{
				if ( webProcess != null )
				{
					// Remove the exit event handler before shutting down the process.
					webProcess.Exited -= appDomainUnloadEvent;

					// Tell XSP to terminate and wait for it to exit.
					webProcess.StandardInput.WriteLine( "" );
					if (!webProcess.WaitForExit(10000))
					{
						if (webProcess.HasExited == false)
							webProcess.Kill();
					}
					webProcess = null;
				}
			}
		}
		#endregion
	}
}
