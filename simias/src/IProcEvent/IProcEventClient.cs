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
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;

using Simias.Event;
using Simias.Storage;

namespace Simias.IProcEvent
{
	/// <summary>
	/// Delegate used to indicate events to the client subscriber.
	/// </summary>
	public delegate void IProcEventHandler( SimiasEventArgs args );

	/// <summary>
	/// Delegate used to indicate an error in the event processing.
	/// </summary>
	public delegate void IProcEventError( SimiasException e, object errorContext );

	/// <summary>
	/// Structure used to queue items on the event action queue.
	/// </summary>
	internal struct EventActionQueueItem
	{
		#region Structure Members
		/// <summary>
		/// Action to take regarding the event.
		/// </summary>
		public IProcEventAction action;

		/// <summary>
		/// Delegate that gets called when specified event happens or is to be removed.
		/// </summary>
		public IProcEventHandler handler;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the structure.
		/// </summary>
		/// <param name="action">Action to take regarding the event.</param>
		/// <param name="handler">Delegate that gets called when specified event happens or is
		/// to be removed.</param>
		public EventActionQueueItem( IProcEventAction action, IProcEventHandler handler )
		{
			this.action = action;
			this.handler = handler;
		}
		#endregion
	}

	/// <summary>
	/// Summary description for Event.
	/// </summary>
	public class IProcEventClient
	{
		#region Class Members
		/// <summary>
		/// States used to track client initialization and shutdown.
		/// </summary>
		private enum ClientState
		{
			Initializing,
			Registering,
			Running,
			Shutdown
		}

		/// <summary>
		/// Used to keep track of the state of the registration thread.
		/// </summary>
		private enum RegThreadState
		{
			Initializing,
			Terminated,
			TerminationAck
		}

		/// <summary>
		/// Tags used to get the host and port number in the IProcConfig configuration file.
		/// </summary>
		private static string HostTag = "Host";
		private static string PortTag = "Port";

		/// <summary>
		/// Error returned when the Registration thread terminates with a receive outstanding.
		/// </summary>
		private const int ownerThreadTerminated = 995;

		/// <summary>
		/// Winsock error that we care about.
		/// </summary>
		private const int WSAECONNREFUSED = 10061;

		/// <summary>
		/// File name and path of the IProcEvent configuration file.
		/// </summary>
		private static string configFileName = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), "IProcEvent.cfg" );

		/// <summary>
		/// Initialization state of the client.
		/// </summary>
		private ClientState state;

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
		private event IProcEventHandler onCreatedNodeEvent = null;
		private event IProcEventHandler onDeletedNodeEvent = null;
		private event IProcEventHandler onChangedNodeEvent = null;
		private event IProcEventHandler onSyncEvent = null;

		/// <summary>
		/// Delegate and context used to indicate an error.
		/// </summary>
		private IProcEventError errorCallback = null;
		private object errorContext = null;

		/// <summary>
		/// Queues used to keep track of SetEvent() and SetFilter() requests before the client has registered.
		/// </summary>
		private Queue eventActionQueue = new Queue();
		private Queue eventFilterQueue = new Queue();

		/// <summary>
		/// Tells the state of the registration thread. This is a work around for the error
		/// that gets thrown when the registration thread exits with a pending async receive.
		/// </summary>
		private RegThreadState regThreadState;

		/// <summary>
		/// Event that gets used to signal the registration service when the configuration file
		/// changes.
		/// </summary>
		private ManualResetEvent regServiceEvent;
		#endregion

		#region Properties
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="errorHandler">Delegate that gets called if an error occurs. A null may be passed in
		/// if the application does not care to be notified of errors.</param>
		/// <param name="context">Context that is passed to the error handler.</param>
		public IProcEventClient( IProcEventError errorHandler, object context )
		{
			// Create a socket to communicate with the event server on.
			eventSocket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
			state = ClientState.Initializing;
			
			// Save the error handling information.
			errorCallback = errorHandler;
			errorContext = context;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Callback that gets notified when the configuration file changes.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="args">The FileSystemEventArgs that contains the event data.</param>
		private void ConfigFileChanged( object source, FileSystemEventArgs args )
		{
			// The configuration file has changed. Wake up the registration thread.
			regServiceEvent.Set();
		}

		/// <summary>
		/// Returns the address of the host where the event service is running.
		/// </summary>
		/// <returns>An IPEndPoint object that contains the host address.</returns>
		private IPEndPoint GetHostAddress()
		{
			IPEndPoint ep = null;

			// Wait until the service is listening. Then get the contents of the configuration file. 
			// If the server happens to be writing to the file while this process is reading an 
			// exception will occur and this process will try again after a short wait interval.
			while ( ep == null )
			{
				if ( File.Exists( configFileName ) )
				{
					try
					{
						XmlDocument document = new XmlDocument();
						document.Load( configFileName );
						IPAddress hostAddress = IPAddress.Parse( document.DocumentElement[ HostTag ].InnerText );
						int hostPort = Convert.ToInt32( document.DocumentElement[ PortTag ].InnerText );
						ep = new IPEndPoint( hostAddress, hostPort );
					}
					catch 
					{
						// Wait and then try again if the service has not been shutdown.
						Thread.Sleep( 1000 );
						if ( state == ClientState.Shutdown )
						{
							break;
						}
					}
				}
				else
				{
					// Wait and then try again if the service has not been shutdown.
					Thread.Sleep( 1000 );
					if ( state == ClientState.Shutdown )
					{
						break;
					}
				}
			}

			return ep;
		}

		/// <summary>
		/// Processes messages from the event server.
		/// </summary>
		/// <param name="result">The result of the asynchronous operation.</param>
		private void MessageHandler( IAsyncResult result )
		{
			// Make sure that the client is not shutting down.
			if ( state != ClientState.Shutdown )
			{
				try
				{
					// End the pending read operation.
					int bytesReceived = eventSocket.EndReceive( result );
					if ( bytesReceived > 0 )
					{
						try
						{
							// Keep track of how much data is in the buffer.
							bufferLength += bytesReceived;
							int bytesToProcess = bufferLength;
							int msgIndex = 0;

							// Process as much as is available. 
							while ( bytesToProcess > 0 )
							{
								// There needs to be atleast 4 bytes for the message length.
								if ( bytesToProcess >= 4 )
								{
									// Get the length of the message, add in the prepended length.
									int msgLength = BitConverter.ToInt32( receiveBuffer, msgIndex ) + 4;
							
									// See if the entire message is represented in the buffer.
									if ( bytesToProcess >= msgLength )
									{
										// Process the message received from the client. Skip the message length.
										ProcessMessage( new ASCIIEncoding().GetString( receiveBuffer, msgIndex + 4, msgLength - 4 ) );
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
								Buffer.BlockCopy( receiveBuffer, msgIndex, receiveBuffer, 0, bytesToProcess );
							}

							// Repost the buffer.
							eventSocket.BeginReceive( receiveBuffer, bufferLength, receiveBuffer.Length - bufferLength, SocketFlags.None, new AsyncCallback( MessageHandler ), null );
						}
						catch ( Exception e )
						{
							Shutdown( new SimiasException( "Error processing event message from server.", e ) );
						}
					}
					else
					{
						// The server has gone away or has terminated our connection. Clean up here.
						Shutdown( null );
					}
				}
				catch ( SocketException e )
				{
					// Check to see if this exception was caused by the registration thread going away.
					// If it was, just repost the receive.
					if ( ( e.ErrorCode == ownerThreadTerminated ) && ( regThreadState == RegThreadState.Terminated ) )
					{
						regThreadState = RegThreadState.TerminationAck;
						eventSocket.BeginReceive( receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, new AsyncCallback( MessageHandler ), null );
					}
					else
					{
						Shutdown( new SimiasException( "Socket receive failed", e ) );
					}
				}
			}
		}

		/// <summary>
		/// Dequeues any event action items and submits them to the server.
		/// </summary>
		private void ProcessEventActionQueue()
		{
			lock ( eventActionQueue )
			{
				// Go through each item on the queue and submit it to the server.
				foreach ( EventActionQueueItem qi in eventActionQueue )
				{
					SetEvent( qi.action, qi.handler );
				}

				// Clear the queue.
				eventActionQueue.Clear();
			}
		}

		/// <summary>
		/// Dequeues any event filter items and submits them to the server.
		/// </summary>
		private void ProcessEventFilterQueue()
		{
			lock ( eventFilterQueue )
			{
				// Go through each item on the queue and submit it to the server.
				foreach ( IProcEventFilter[] filters in eventFilterQueue )
				{
					SetFilter( filters );
				}

				// Clear the queue.
				eventFilterQueue.Clear();
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
		/// Queues up the event action set request until the client has registered.
		/// </summary>
		/// <param name="action">Action to take regarding the event.</param>
		/// <param name="handler">Delegate that gets called when specified event happens or is
		/// to be removed.</param>
		private void QueueEventAction( IProcEventAction action, IProcEventHandler handler )
		{
			lock( eventActionQueue )
			{
				eventActionQueue.Enqueue( new EventActionQueueItem( action, handler ) );
			}
		}

		/// <summary>
		/// Queues up the event filter set request until the client has registered.
		/// </summary>
		/// <param name="filters">An array of EventFilter objects.</param>
		private void QueueEventFilters( IProcEventFilter[] filters )
		{
			lock( eventFilterQueue )
			{
				eventFilterQueue.Enqueue( filters );
			}
		}

		/// <summary>
		/// Thread service routine that registers with the event service and then dies.
		/// </summary>
		private void RegistrationService()
		{
			try
			{
				// Connect to the event service and send an initialization message.
				IPEndPoint host = GetHostAddress();
				if ( state != ClientState.Shutdown )
				{
					// Sit here until we connect.
					while ( !eventSocket.Connected )
					{
						try
						{
							// Connect to the server.
							eventSocket.Connect( host );
						}
						catch ( SocketException e )
						{
							// See if this is a case of the server not listening on the socket anymore.
							// It may have gone down hard and left the configuration file with an invalid
							// socket. Keep watching the file until the socket changes and it can be
							// connected.
							if ( e.ErrorCode == WSAECONNREFUSED )
							{
								// Create an event that we can wait on.
								regServiceEvent = new ManualResetEvent( false );

								// Set a watcher on the file to monitor it when it changes.
								FileSystemWatcher fsw = new FileSystemWatcher( Path.GetDirectoryName( configFileName ) );
								fsw.Filter = Path.GetFileName( configFileName );
								fsw.NotifyFilter = NotifyFilters.LastWrite;
								fsw.Changed += new FileSystemEventHandler( ConfigFileChanged );
								fsw.EnableRaisingEvents = true;

								// Wait for the change to occur.
								regServiceEvent.WaitOne();

								// Turn off the file monitoring.
								fsw.EnableRaisingEvents = false;

								// Get the new information from the configuration file.
								host = GetHostAddress();
								if ( state == ClientState.Shutdown )
								{
									// We were told to shutdown, just bail from here.
									return;
								}
							}
							else
							{
								throw e;
							}
						}
					}

					// Get the local end point information
					localEndPoint = eventSocket.LocalEndPoint as IPEndPoint;

					// Post the received before the registration message is sent.
					eventSocket.BeginReceive( receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, new AsyncCallback( MessageHandler ), null );

					// Register our client.
					SendMessage( new IProcEventRegistration( localEndPoint, true ).ToBuffer() );

					// Set the state as running.
					state = ClientState.Running;

					// Check to see if any set filter items have been queue.
					ProcessEventFilterQueue();

					// Check to see if any set event actions have been queued.
					ProcessEventActionQueue();
				}
			}
			catch ( SocketException e )
			{
				Shutdown( new SimiasException( "Error registering with the event service.", e ) );
			}

			// This thread is going away.
			regThreadState = RegThreadState.Terminated;
		}

		/// <summary>
		/// Sends an error message to the error handling delegate if registered.
		/// </summary>
		/// <param name="exception">Error to hand to the delegate.</param>
		private void ReportError( SimiasException exception )
		{
			if ( errorCallback != null )
			{
				errorCallback( exception, errorContext );
			}
		}

		/// <summary>
		/// Sends the specified message to the server.
		/// </summary>
		/// <param name="message">Message to send to the server.</param>
		private void SendMessage( byte[] message )
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

		/// <summary>
		/// Closes the socket and cleans up the client.
		/// </summary>
		/// <param name="exception">The exception that occurred if there was an error. Otherwise
		/// this parameter is null.</param>
		private void Shutdown( SimiasException exception )
		{
			lock ( this )
			{
				if ( state != ClientState.Shutdown )
				{
					state = ClientState.Shutdown;
					try
					{
						if ( eventSocket.Connected )
						{
							try
							{
								// Try and be graceful about things.
								eventSocket.Shutdown( SocketShutdown.Both );
							}
							catch {}
						}

						eventSocket.Close();
					}
					catch {}
				}
			}

			// Inform the application if an error occurred.
			if ( exception != null )
			{
				ReportError( exception );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Deregisters this client with the server.
		/// </summary>
		public void Deregister()
		{
			if ( state == ClientState.Running )
			{
				// Deregister our client.
				SendMessage( new IProcEventRegistration( localEndPoint, false ).ToBuffer() );
			}

			Shutdown( null );
			errorCallback = null;
			localEndPoint = null;
		}

		/// <summary>
		/// Registers this client with the server to listen for simias events.
		/// </summary>
		public void Register()
		{
			// Don't let registration happen multiple times.
			if ( state == ClientState.Initializing )
			{
				// Set the states to registering.
				state = ClientState.Registering;
				regThreadState = RegThreadState.Initializing;
			
				// Start a thread which will process the registration request.
				Thread thread = new Thread( new ThreadStart( RegistrationService ) );
				thread.Start();
			}
		}

		/// <summary>
		/// Starts subscribing to or unsubscribing from the specified event.
		/// </summary>
		/// <param name="action">Action to take regarding the event.</param>
		/// <param name="handler">Delegate that gets called when specified event happens or is
		/// to be removed.</param>
		public void SetEvent( IProcEventAction action, IProcEventHandler handler )
		{
			// If the client hasn't registered with the server yet, queue this request for later.
			if ( state == ClientState.Registering )
			{
				QueueEventAction( action, handler );
			}
			else if ( state == ClientState.Running )
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
			else
			{
				ReportError( new SimiasException( "The client is in an invalid state for this operation." ) );
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
			// If the client hasn't registered with the server yet, queue this request for later.
			if ( state == ClientState.Registering )
			{
				QueueEventFilters( filters );
			}
			else if ( state == ClientState.Running )
			{
				IProcEventListener message = new IProcEventListener();
				foreach ( IProcEventFilter filter in filters )
				{
					message.AddFilter( filter );
				}

				// Send the message.
				SendMessage( message.ToBuffer() );
			}
			else
			{
				ReportError( new SimiasException( "The client is in an invalid state for this operation." ) );
			}
		}
		#endregion
	}
}
