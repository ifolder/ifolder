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
using System.Text;
using System.Threading;
using System.Xml;

using Simias;
using Simias.Event;
using Simias.Storage;

namespace Simias.IProcEvent
{
	/// <summary>
	/// Delegate used to indicate events to the client subscriber.
	/// </summary>
	public delegate void InterProcessEventHandler( SimiasEventArgs args );

	/// <summary>
	/// Delegate used to indicate an error in the event processing.
	/// </summary>
	public delegate void InterProcessEventError( SimiasException e, object errorContext );

	/// <summary>
	/// Summary description for Event.
	/// </summary>
	public class IProcEventClient
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private ISimiasLog log;

		/// <summary>
		/// Tags used to get the port number in the Simias configuration file.
		/// </summary>
		private static string EventServiceTag = "InterProcessEventService";
		private static string PortTag = "Port";

		/// <summary>
		/// Total time in milliseconds to wait for the event server to register its service.
		/// </summary>
		private static TimeSpan WaitForServiceTimeout = new TimeSpan( 0, 1, 0 );

		/// <summary>
		/// Indicates that the client has shutdown the socket.
		/// </summary>
		private bool shutDown = false;

		/// <summary>
		/// Socket used to communicate with the event server.
		/// </summary>
		private Socket eventSocket;

		/// <summary>
		/// Buffer used to receive the socket messages.
		/// </summary>
		private byte[] receiveBuffer = new byte[ 512 ];

		/// <summary>
		/// Indicates the current amount of data in the buffer to be processed.
		/// </summary>
		private int bufferLength = 0;

		/// <summary>
		/// Contains the local endpoint address for this client.
		/// </summary>
		private IPEndPoint localEndPoint = null;

		/// <summary>
		/// Events that hold delegates that indicate a subscribed for event.
		/// </summary>
		private event InterProcessEventHandler onCreatedNodeEvent;
		private event InterProcessEventHandler onDeletedNodeEvent;
		private event InterProcessEventHandler onChangedNodeEvent;
		private event InterProcessEventHandler onSyncEvent;

		/// <summary>
		/// Delegate and context used to indicate an error.
		/// </summary>
		private InterProcessEventError errorCallback = null;
		private object errorContext = null;
		#endregion

		#region Properties
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		public IProcEventClient()
		{
			// Create a socket to communicate with the event server on.
			eventSocket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

			// Initialize the logger.
			SimiasLogManager.Configure( Configuration.GetConfiguration() );
			log = SimiasLogManager.GetLogger( typeof( IProcEventClient ) );
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Returns the address of the host where the event service is running.
		/// </summary>
		/// <returns>An IPEndPoint object that contains the host address.</returns>
		private IPEndPoint GetHostAddress()
		{
			// Get the configuration file.
			Configuration config = Configuration.GetConfiguration();
			log.Info( "Waiting for interprocess event service." );

			// Wait until there is a service listening.
			DateTime timeout = DateTime.Now;
			string portString = config.Get( EventServiceTag, PortTag, null );
			while ( portString == null )
			{
				Thread.Sleep( 1000 );
				if ( DateTime.Now.Subtract( timeout ) >= WaitForServiceTimeout )
				{
					Shutdown( new SimiasException( "No remote event service is available." ) );
					return null;
				}

				// Force the configuration to reload from the file. Since this process is not 
				// in the simias process any changes to the configuration file will not be 
				// visible across processes without actually reloading the file.
				config.Refresh();
				portString = config.Get( EventServiceTag, PortTag, null );
			}

			log.Info( "Interprocess event service is active." );
			
			// Build an IPEndPoint object with the loopback address.
			return new IPEndPoint( IPAddress.Loopback, Convert.ToInt32( portString ) );
		}

		/// <summary>
		/// Processes messages from the event server.
		/// </summary>
		/// <param name="result">The result of the asynchronous operation.</param>
		static private void MessageHandler( IAsyncResult result )
		{
			// Get the EventClient object as context for this request.
			IProcEventClient client = ( IProcEventClient )result.AsyncState;
			
			// Make sure that the client is not shutting down.
			if ( !client.shutDown )
			{
				try
				{
					// End the pending read operation.
					int bytesReceived = client.eventSocket.EndReceive( result );
					if ( bytesReceived > 0 )
					{
						try
						{
							// Keep track of how much data is in the buffer.
							client.bufferLength += bytesReceived;
							int bytesToProcess = client.bufferLength;
							int msgIndex = 0;

							// Process as much as is available. 
							while ( bytesToProcess > 0 )
							{
								// There needs to be atleast 4 bytes for the message length.
								if ( bytesToProcess >= 4 )
								{
									// Get the length of the message, add in the prepended length.
									int msgLength = BitConverter.ToInt32( client.receiveBuffer, msgIndex ) + 4;
							
									// See if the entire message is represented in the buffer.
									if ( bytesToProcess >= msgLength )
									{
										// Process the message received from the client. Skip the message length.
										client.ProcessMessage( new ASCIIEncoding().GetString( client.receiveBuffer, msgIndex + 4, msgLength - 4 ) );
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
							client.bufferLength = bytesToProcess;

							// If any data has been processed at the beginning of the buffer, the unprocessed
							// data needs to be moved to the front.
							if ( ( bytesToProcess > 0 ) && ( msgIndex > 0 ) )
							{
								// Move any existing data up.
								Buffer.BlockCopy( client.receiveBuffer, msgIndex, client.receiveBuffer, 0, bytesToProcess );
							}
						}
						catch ( Exception e )
						{
							log.Error( e, "Error processing event message from server." );
						}

						// Repost the buffer.
						client.eventSocket.BeginReceive( client.receiveBuffer, client.bufferLength, client.receiveBuffer.Length - client.bufferLength, SocketFlags.None, new AsyncCallback( MessageHandler ), client );
					}
					else
					{
						// The server has gone away or has terminated our connection. Clean up here.
						log.Debug( "Server has terminated the connection." );
						client.Shutdown( null );
					}
				}
				catch ( SocketException e )
				{
					client.Shutdown( new SimiasException( "Socket receive failed", e ) );
				}
			}
		}

		/// <summary>
		/// Processes messages received from the server.
		/// </summary>
		/// <param name="message">Message received from the server.</param>
		private void ProcessMessage( string message )
		{
			XmlDocument document = new XmlDocument();
			document.LoadXml( message );
			IProcEventData ed = new IProcEventData( document );
			switch ( ed.Type )
			{
				case "NodeEventArgs":
				{
					// Get the node arguments from the document.
					NodeEventArgs nodeArgs = ed.ToNodeEventArgs();

					// Determine the type of event that occurred.
					switch ( ( EventType )Enum.Parse( typeof( EventType ), nodeArgs.EventData ) )
					{
						case EventType.NodeChanged:
						{
							if ( onChangedNodeEvent != null )
							{
								onChangedNodeEvent( nodeArgs );
							}

							break;
						}

						case EventType.NodeCreated:
						{
							if ( onCreatedNodeEvent != null )
							{
								onCreatedNodeEvent( nodeArgs );
							}

							break;
						}

						case EventType.NodeDeleted:
						{
							if ( onDeletedNodeEvent != null )
							{
								onDeletedNodeEvent( nodeArgs );
							}

							break;
						}
					}
			
					break;
				}

				case "SyncEventArgs":
				{
					break;
				}
			}
		}

		/// <summary>
		/// Sends the specified message to the server.
		/// </summary>
		/// <param name="message">Message to send to the server.</param>
		private void SendMessage( byte[] message )
		{
			if ( !shutDown )
			{
				try
				{
					eventSocket.Send( message );
				}
				catch ( SocketException e )
				{
					Shutdown( new SimiasException( "Failed to send message to server.", e ) );
				}
			}
		}

		/// <summary>
		/// Closes the socket and cleans up the client.
		/// </summary>
		/// <param name="exception">The exception that occurred if there was an error. Otherwise
		/// this parameter is null.</param>
		private void Shutdown( SimiasException exception )
		{
			lock ( this )
			{
				if ( !shutDown )
				{
					shutDown = true;
					if ( eventSocket != null )
					{
						try
						{
							if ( eventSocket.Connected )
							{
								try
								{
									// Try and be graceful about things.
									eventSocket.Shutdown( SocketShutdown.Both );
								}
								catch ( SocketException e )
								{
									log.Error( e, "Error sending shutdown request." );
								}
							}

							eventSocket.Close();
						}
						catch ( SocketException e )
						{
							log.Error( e, "Error shutting down the client." );
						}
					}
				}
			}

			// Inform the application that an error occurred if a callback was registered.
			if ( ( errorCallback != null ) && ( exception != null ) )
			{
				errorCallback( exception, errorContext );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Deregisters this client with the server.
		/// </summary>
		public void Deregister()
		{
			if ( !shutDown && ( localEndPoint != null ) )
			{
				// Deregister our client.
				SendMessage( new IProcEventRegistration( localEndPoint, false ).ToBuffer() );
				Shutdown( null );
			}

			errorCallback = null;
			localEndPoint = null;
		}

		/// <summary>
		/// Registers this client with the server to listen for simias events.
		/// </summary>
		/// <param name="errorHandler">Delegate that gets called if an error occurs. A null may be passed in
		/// if the application does not care to be notified of errors.</param>
		/// <param name="context">Context that is passed to the error handler.</param>
		public void Register( InterProcessEventError errorHandler, object context )
		{
			// Keep the error callback delegate.
			errorCallback = errorHandler;
			errorContext = context;
			
			try
			{
				// Connect to the event service and send an initialization message.
				IPEndPoint host = GetHostAddress();
				if ( !shutDown && ( host != null ) )
				{
					eventSocket.Connect( host );

					// Get the local end point information
					localEndPoint = eventSocket.LocalEndPoint as IPEndPoint;

					// Post the received before the registration message is sent.
					eventSocket.BeginReceive( receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, new AsyncCallback( MessageHandler ), this );

					// Register our client.
					SendMessage( new IProcEventRegistration( localEndPoint, true ).ToBuffer() );
				}
			}
			catch ( SocketException e )
			{
				Shutdown( new SimiasException( "Error registering with the event service.", e ) );
			}
		}

		/// <summary>
		/// Starts subscribing to or unsubscribing from the specified event.
		/// </summary>
		/// <param name="action">Action to take regarding the event.</param>
		/// <param name="handler">Delegate that gets called when specified event happens or is
		/// to be removed.</param>
		public void SetEvent( IProcEventAction action, InterProcessEventHandler handler )
		{
			bool duplicateSubscriber = false;

			// Build the listener message.
			IProcEventListener message = new IProcEventListener();
			message.AddEvent( action );

			// Set the handler for the proper event type.
			switch ( action )
			{
				case IProcEventAction.AddNodeCreated:
				{
					duplicateSubscriber = ( onCreatedNodeEvent != null ) ? true : false;
					onCreatedNodeEvent += handler;
					break;
				}

				case IProcEventAction.AddNodeDeleted:
				{
					duplicateSubscriber = ( onDeletedNodeEvent != null ) ? true : false;
					onDeletedNodeEvent += handler;
					break;
				}

				case IProcEventAction.AddNodeChanged:
				{
					duplicateSubscriber = ( onChangedNodeEvent != null ) ? true : false;
					onChangedNodeEvent += handler;
					break;
				}

				case IProcEventAction.AddSync:
				{
					duplicateSubscriber = ( onSyncEvent != null ) ? true : false;
					onSyncEvent += handler;
					break;
				}

				case IProcEventAction.RemoveNodeCreated:
				{
					onCreatedNodeEvent -= handler;
					duplicateSubscriber = ( onCreatedNodeEvent != null ) ? true : false;
					break;
				}

				case IProcEventAction.RemoveNodeDeleted:
				{
					onDeletedNodeEvent -= handler;
					duplicateSubscriber = ( onDeletedNodeEvent != null ) ? true : false;
					break;
				}

				case IProcEventAction.RemoveNodeChanged:
				{
					onChangedNodeEvent -= handler;
					duplicateSubscriber = ( onChangedNodeEvent != null ) ? true : false;
					break;
				}

				case IProcEventAction.RemoveSync:
				{
					onSyncEvent -= handler;
					duplicateSubscriber = ( onSyncEvent != null ) ? true : false;
					break;
				}
			}

			// Send the message if necessary.
			if ( !duplicateSubscriber )
			{
				SendMessage( message.ToBuffer() );
			}
		}

		/// <summary>
		/// Sets the specified filters for the subscriber.
		/// </summary>
		/// <param name="filter">An EventFilter object.</param>
		public void SetFilter( IProcEventFilter filter )
		{
			SetFilter( new IProcEventFilter[] { filter } );
		}

		/// <summary>
		/// Sets the specified filters for the subscriber.
		/// </summary>
		/// <param name="filters">An array of EventFilter objects.</param>
		public void SetFilter( IProcEventFilter[] filters )
		{
			IProcEventListener message = new IProcEventListener();
			foreach ( IProcEventFilter filter in filters )
			{
				message.AddFilter( filter );
			}

			// Send the message.
			SendMessage( message.ToBuffer() );
		}
		#endregion
	}
}
