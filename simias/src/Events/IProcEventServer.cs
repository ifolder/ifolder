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
 *  Author: Mike Lasky
 *
 ***********************************************************************/
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Simias;
using Simias.Service;

namespace Simias.Event
{
	/// <summary>
	/// Service that pushes Simias events into another process. Off-box
	/// processes are not allowed to register.
	/// </summary>
	public class IProcEventServer : IThreadService
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( IProcEventServer ) );

		/// <summary>
		/// Tags used to set the port number in the Simias configuration file.
		/// </summary>
		private static string EventServiceTag = "InterProcessEventService";
		private static string PortTag = "Port";

		/// <summary>
		/// Socket error that indicates the server closed its connection.
		/// </summary>
		private const int SocketClosed = 10004;

		/// <summary>
		/// Holds the simias configuration.
		/// </summary>
		private Configuration config;

		/// <summary>
		/// Socket used to handle registration requests.
		/// </summary>
		private Socket regSocket;
		#endregion

		#region Properties
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		public IProcEventServer()
		{
			// Start the server listening on a dynamic port.
			regSocket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Event registration service.
		/// </summary>
		private void RegistrationService()
		{
			bool exitLoop = false;

			while ( !exitLoop )
			{
				try
				{
					// Wait for an incoming request.
					Socket clientSocket = regSocket.Accept();
					try
					{
						// Make sure that this request came from the loopback address. Off-box requests are not supported.
						if ( IPAddress.IsLoopback( ( clientSocket.RemoteEndPoint as IPEndPoint ).Address ) )
						{
							// Setup an asynchronous receive so all requests get processed in a different execution thread.
							IProcEventSubscriber sub = new IProcEventSubscriber( clientSocket );
							clientSocket.BeginReceive( sub.ReceiveBuffer, 0, sub.ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback( IProcEventSubscriber.MessageHandler ), sub );
						}
						else
						{
							clientSocket.Close();
						}
					}
					catch ( SocketException e )
					{
						log.Error( e, "Socket exception" );
						clientSocket.Close();
					}
					catch ( Exception e )
					{
						log.Error( e, "Cannot begin message processing due to unknown exception." );
						clientSocket.Close();
					}
				}
				catch ( SocketException e )
				{
					// Check to see if the socket was closed underneath us.
					if ( e.ErrorCode == SocketClosed )
					{
						exitLoop = true;
					}
					else
					{
						log.Error( e, "Socket exception" );
					}
				}
			}
		}
		#endregion

		#region IThreadService Members
		/// <summary>
		/// Starts the server listening for incoming event registration requests.
		/// </summary>
		/// <param name="config">Configuration file object that indicates which Collection Store to use.</param>
		public void Start( Configuration config )
		{
			// Save this for the stop.
			this.config = config;

			// Start the server listening.
			regSocket.Bind( new IPEndPoint( IPAddress.Loopback, 0 ) );
			regSocket.Listen( 10 );

			// Start a thread which will process the registration requests.
			Thread thread = new Thread( new ThreadStart( RegistrationService ) );
			thread.Start();

			// Get the port that the server is listening on and write to the configuration file.
			IPEndPoint ep = regSocket.LocalEndPoint as IPEndPoint;
			config.Set( EventServiceTag, PortTag, ep.Port.ToString() );
		}

		/// <summary>
		/// Stops the server from listening for incoming event registration requests.
		/// </summary>
		public void Stop()
		{
			// Remove the port from the configuration file so that no new callers can register.
			config.DeleteKey( EventServiceTag, PortTag );
			regSocket.Close();
		}

		/// <summary>
		/// Pauses a service's execution.
		/// </summary>
		public void Pause()
		{
		}

		/// <summary>
		/// Resumes a paused service. 
		/// </summary>
		public void Resume()
		{
		}

		/// <summary>
		/// Custom.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public void Custom( int message, string data )
		{
		}
		#endregion
	}
}
