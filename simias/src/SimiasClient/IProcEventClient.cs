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
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Xml;

namespace Simias.Client.Event
{
	/// <summary>
	/// Delegate used to indicate events to the client subscriber.
	/// </summary>
	public delegate void IProcEventHandler( SimiasEventArgs args );

	/// <summary>
	/// Delegate used to indicate an error in the event processing.
	/// </summary>
	public delegate void IProcEventError( ApplicationException e, object errorContext );

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
		private const int WSAEHOSTDOWN =10064;

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
		private event IProcEventHandler onCollectionSyncEvent = null;
		private event IProcEventHandler onFileSyncEvent = null;
		private event IProcEventHandler onNotifyEvent = null;

		/// <summary>
		/// Delegate and context used to indicate an error.
		/// </summary>
		private IProcEventError errorCallback = null;
		private object errorContext = null;

		/// <summary>
		/// Queues used to keep track of SetEvent() and SetFilter() requests before the client has registered.
		/// </summary>
		private string subscribeLock = "SubscribeLock";
		private Queue eventActionQueue = new Queue();
		private Queue eventFilterQueue = new Queue();

		/// <summary>
		/// Producer/Consumer queues used to handle node and sync event messages. Sync events have precedence
		/// over node events.
		/// </summary>
		private Queue nodeEventMessageQueue = new Queue();
		private Queue syncEventMessageQueue = new Queue();

		/// <summary>
		/// Event that indicates that a message was placed in the queue.
		/// </summary>
		private ManualResetEvent messageReadyEvent = new ManualResetEvent( false );

		/// <summary>
		/// Tells the state of the registration thread. This is a work around for the error
		/// that gets thrown when the registration thread exits with a pending async receive.
		/// </summary>
		private RegThreadState regThreadState;

		/// <summary>
		/// Watcher on the configuration file to monitor it when it changes.
		/// </summary>
		private FileSystemWatcher fsw = null;

		/// <summary>
		/// Event that gets used to signal the registration service when the configuration file
		/// changes.
		/// </summary>
		private ManualResetEvent regServiceEvent = new ManualResetEvent( false );
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

			// Start the event thread waiting for event messages.
			Thread thread = new Thread( new ThreadStart( EventThread ) );
			thread.IsBackground = true;
			thread.Start();
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
		/// Ends monitoring the configuration file.
		/// </summary>
		private void EndMonitorConfigFileChange()
		{
			// Turn off the file monitoring.
			fsw.EnableRaisingEvents = false;
			fsw.Dispose();
			fsw = null;
		}

		/// <summary>
		/// Callback used to indicate that an event has been indicated to the delegate.
		/// </summary>
		/// <param name="result">Results that contains the delegate.</param>
		private void EventCompleteCallback( IAsyncResult result )
		{
			IProcEventHandler eventDelegate = ( result as AsyncResult ).AsyncDelegate as IProcEventHandler;
			eventDelegate.EndInvoke( result );
		}

		/// <summary>
		/// Thread that indicates node and sync events via registered delegates.
		/// </summary>
		private void EventThread()
		{
			// Stay in the loop until the client has been shutdown.
			while ( state != ClientState.Shutdown )
			{
				IProcEventData eventData = null;

				try
				{
					// See if there are any sync events to process.
					lock ( syncEventMessageQueue )
					{
						if ( syncEventMessageQueue.Count > 0 )
						{
							eventData = syncEventMessageQueue.Dequeue() as IProcEventData;
						}
					}

					if ( eventData != null )
					{
						ProcessEventData( eventData );
					}
					else
					{
						// See if there are any node events to process.
						lock ( nodeEventMessageQueue )
						{
							if ( nodeEventMessageQueue.Count > 0 )
							{
								eventData = nodeEventMessageQueue.Dequeue() as IProcEventData;
							}
						}

						if ( eventData != null )
						{
							ProcessEventData( eventData );
						}
					}
				}
				catch ( Exception e )
				{
					// Don't let the thread terminate because of an exception.
					ReportError( new ApplicationException( "Error in event thread.", e ) );
				}

				// See if there are any message left to process.
				if ( eventData == null )
				{
					messageReadyEvent.WaitOne();
					messageReadyEvent.Reset();
				}
			}
		}

		/// <summary>
		/// Returns the address of the host where the event service is running.
		/// </summary>
		/// <returns>An IPEndPoint object that contains the host address.</returns>
		private IPEndPoint GetHostAddress()
		{
			IPEndPoint ep = null;

			// See if the local application data directory has been created.
			if ( !Directory.Exists( Path.GetDirectoryName( configFileName ) ) )
			{
				Directory.CreateDirectory( Path.GetDirectoryName( configFileName ) );
			}

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
						WaitForConfigFileChange();
						if ( state == ClientState.Shutdown )
						{
							break;
						}
					}
				}
				else
				{
					// Wait and then try again if the service has not been shutdown.
					WaitForConfigFileChange();
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
										ProcessMessage( new UTF8Encoding().GetString( receiveBuffer, msgIndex + 4, msgLength - 4 ) );
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
						catch ( SocketException e )
						{
							throw e;
						}
						catch ( Exception e )
						{
							ReportError( new ApplicationException( "Error processing event message from server.", e ) );
						}
					}
					else
					{
						// The server has gone away or has terminated our connection.
						throw new SocketException( WSAEHOSTDOWN );
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
						ReportError( new ApplicationException( "Error processing event message from server.", e ) );
						RestartClient();
					}
				}
			}
		}

		/// <summary>
		/// Dequeues any event action items and submits them to the server.
		/// </summary>
		private void ProcessEventActionQueue()
		{
			// Go through each item on the queue and submit it to the server.
			foreach ( EventActionQueueItem qi in eventActionQueue )
			{
				SubscribeEvent( qi.action, qi.handler );
			}
		}

		/// <summary>
		/// Processes queued event data messages by calling the delegates registered for the respective events.
		/// </summary>
		/// <param name="eventData">Event message received from the server.</param>
		private void ProcessEventData( IProcEventData eventData )
		{
			switch ( eventData.Type )
			{
				case "NodeEventArgs":
				{
					// Get the node arguments from the document.
					NodeEventArgs nodeArgs = eventData.ToNodeEventArgs();

					// Determine the type of event that occurred.
					switch ( ( EventType )Enum.Parse( typeof( EventType ), nodeArgs.EventData ) )
					{
						case EventType.NodeChanged:
						{
							if ( onChangedNodeEvent != null )
							{
								Delegate[] cbList = onChangedNodeEvent.GetInvocationList();
								foreach ( IProcEventHandler cb in cbList )
								{
									try 
									{ 
										cb( nodeArgs );
									}
									catch ( Exception ex )
									{
										ReportError( new ApplicationException( "Removing subscriber because of exception", ex ) );
										onChangedNodeEvent -= cb;
									}
								}
							}

							break;
						}

						case EventType.NodeCreated:
						{
							if ( onCreatedNodeEvent != null )
							{
								Delegate[] cbList = onCreatedNodeEvent.GetInvocationList();
								foreach ( IProcEventHandler cb in cbList )
								{
									try 
									{ 
										cb( nodeArgs );
									}
									catch ( Exception ex )
									{
										ReportError( new ApplicationException( "Removing subscriber because of exception", ex ) );
										onCreatedNodeEvent -= cb;
									}
								}
							}

							break;
						}

						case EventType.NodeDeleted:
						{
							if ( onDeletedNodeEvent != null )
							{
								Delegate[] cbList = onDeletedNodeEvent.GetInvocationList();
								foreach ( IProcEventHandler cb in cbList )
								{
									try 
									{ 
										cb( nodeArgs );
									}
									catch ( Exception ex )
									{
										ReportError( new ApplicationException( "Removing subscriber because of exception", ex ) );
										onDeletedNodeEvent -= cb;
									}
								}
							}

							break;
						}
					}
			
					break;
				}

				case "CollectionSyncEventArgs":
				{
					if ( onCollectionSyncEvent != null )
					{
						// Get the collection sync arguments from the document.
						CollectionSyncEventArgs collectionArgs = eventData.ToCollectionSyncEventArgs();
						Delegate[] cbList = onCollectionSyncEvent.GetInvocationList();
						foreach ( IProcEventHandler cb in cbList )
						{
							try 
							{ 
								cb( collectionArgs );
							}
							catch ( Exception ex )
							{
								ReportError( new ApplicationException( "Removing subscriber because of exception", ex ) );
								onCollectionSyncEvent -= cb;
							}
						}
					}
					break;
				}

				case "FileSyncEventArgs":
				{
					if ( onFileSyncEvent != null )
					{
						// Get the file sync arguments from the document.
						FileSyncEventArgs fileArgs = eventData.ToFileSyncEventArgs();
						Delegate[] cbList = onFileSyncEvent.GetInvocationList();
						foreach ( IProcEventHandler cb in cbList )
						{
							try 
							{ 
								cb( fileArgs );
							}
							catch ( Exception ex )
							{
								ReportError( new ApplicationException( "Removing subscriber because of exception", ex ) );
								onFileSyncEvent -= cb;
							}
						}
					}
					break;
				}

				case "NotifyEventArgs":
				{
					if ( onNotifyEvent != null )
					{
						// Get the notify arguments from the document.
						NotifyEventArgs notifyArgs = eventData.ToNotifyEventArgs();
						Delegate[] cbList = onNotifyEvent.GetInvocationList();
						foreach ( IProcEventHandler cb in cbList )
						{
							try
							{
								cb( notifyArgs );
							}
							catch ( Exception ex )
							{
								ReportError( new ApplicationException( "Removing subscriber because of exception", ex ) );
								onNotifyEvent -= cb;
							}
						}
					}
					break;
				}
			}
		}

		/// <summary>
		/// Dequeues any event filter items and submits them to the server.
		/// </summary>
		private void ProcessEventFilterQueue()
		{
			// Go through each item on the queue and submit it to the server.
			foreach ( IProcEventFilter[] filters in eventFilterQueue )
			{
				SubscribeFilter( filters );
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
					lock ( nodeEventMessageQueue )
					{
						nodeEventMessageQueue.Enqueue( ed );
					}
					break;
				}

				case "CollectionSyncEventArgs":
				case "FileSyncEventArgs":
				case "NotifyEventArgs":
				{
					lock ( syncEventMessageQueue )
					{
						syncEventMessageQueue.Enqueue( ed );
					}
					break;
				}
			}

			// Indicate that a message is ready to process.
			messageReadyEvent.Set();
		}

		/// <summary>
		/// Thread service routine that registers with the event service and then dies.
		/// </summary>
		private void RegistrationService()
		{
			try
			{
				// Start monitoring the configuration file for changes.
				StartMonitorConfigFileChange();
				try
				{
					// Sit here until we connect.
					while ( !eventSocket.Connected && ( state != ClientState.Shutdown ) )
					{
						// Connect to the event service and send an initialization message.
						IPEndPoint host = GetHostAddress();

						// Make sure that the thread was not told to shutdown.
						if ( state != ClientState.Shutdown )
						{
							try
							{
								// Connect to the server.
								eventSocket.Connect( host );

								// Get the local end point information
								localEndPoint = eventSocket.LocalEndPoint as IPEndPoint;

								// Post the received before the registration message is sent.
								eventSocket.BeginReceive( receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, new AsyncCallback( MessageHandler ), null );

								// Register our client.
								SendMessage( new IProcEventRegistration( localEndPoint, true ).ToBuffer() );

								// Process the queues within the lock so that any new actions or filters
								// are sent to the server and not just queued and forgotten.
								lock ( subscribeLock )
								{
									// Check to see if any set filter items have been queue.
									ProcessEventFilterQueue();

									// Check to see if any set event actions have been queued.
									ProcessEventActionQueue();

									// Set the state as running.
									state = ClientState.Running;
								}
							}
							catch ( SocketException e )
							{
								// See if this is a case of the server not listening on the socket anymore.
								// It may have gone down hard and left the configuration file with an invalid
								// socket. Keep watching the file until the socket changes and it can be
								// connected.
								if ( e.ErrorCode == WSAECONNREFUSED )
								{
									// Wait for the configuration file to change before continuing.
									WaitForConfigFileChange();
								}
								else
								{
									throw e;
								}
							}
						}
					}
				}
				finally
				{
					// Don't watch for anymore changes.
					EndMonitorConfigFileChange();
				}

				// This thread is going away.
				regThreadState = RegThreadState.Terminated;
			}
			catch ( SocketException e )
			{
				ReportError( new ApplicationException( "Error registering with the event service.", e ) );
				RestartClient();
			}
		}

		/// <summary>
		/// Sends an error message to the error handling delegate if registered.
		/// </summary>
		/// <param name="exception">Error to hand to the delegate.</param>
		private void ReportError( ApplicationException exception )
		{
			if ( errorCallback != null )
			{
				errorCallback( exception, errorContext );
			}
		}

		/// <summary>
		/// Puts the client back into a waiting to connect to the server state after having
		/// received a socket exception.
		/// </summary>
		private void RestartClient()
		{
			// Reset the state back to initializing.
			state = ClientState.Initializing;

			// Reset the class variables.
			bufferLength = 0;
			localEndPoint = null;

			// All events will be reset.
			onCreatedNodeEvent = null;
			onDeletedNodeEvent = null;
			onChangedNodeEvent = null;
			onCollectionSyncEvent = null;
			onFileSyncEvent = null;
			onNotifyEvent = null;

			// Reinitialize the socket.
			eventSocket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

			// Re-register for the events.
			Register();
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
				ReportError( new ApplicationException( "Failed to send message to server.", e ) );
				RestartClient();
			}
		}

		/// <summary>
		/// Closes the socket and cleans up the client.
		/// </summary>
		private void Shutdown()
		{
			lock ( this )
			{
				if ( state != ClientState.Shutdown )
				{
					state = ClientState.Shutdown;
					try
					{
						// Signal the registration thread to shutdown.
						if ( regThreadState == RegThreadState.Initializing )
						{
							regServiceEvent.Set();
						}

						// Take care of the socket if it is connected.
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
		}

		/// <summary>
		/// Begins monitoring the configuration file for changes.
		/// </summary>
		private void StartMonitorConfigFileChange()
		{
			if ( fsw == null )
			{
				// Set the event to not signaled.
				regServiceEvent.Reset();

				// Wait for the configuration directory to be created.
				while ( !Directory.Exists( Path.GetDirectoryName( configFileName ) ) )
				{
					Thread.Sleep( 1000 );
				}

				// Watcher on the configuration file to monitor it when it changes.
				fsw = new FileSystemWatcher( Path.GetDirectoryName( configFileName ) );
				fsw.Filter = Path.GetFileName( configFileName );
				fsw.NotifyFilter = NotifyFilters.LastWrite;
				fsw.Created += new FileSystemEventHandler( ConfigFileChanged );
				fsw.Changed += new FileSystemEventHandler( ConfigFileChanged );
			}

			// Start the watcher monitoring.
			fsw.EnableRaisingEvents = true;
		}

		/// <summary>
		/// Starts subscribing to or unsubscribing from the specified event.
		/// </summary>
		/// <param name="action">Action to take regarding the event.</param>
		/// <param name="handler">Delegate that gets called when specified event happens or is
		/// to be removed.</param>
		private void SubscribeEvent( IProcEventAction action, IProcEventHandler handler )
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

				case IProcEventAction.AddCollectionSync:
				{
					duplicateSubscriber = ( onCollectionSyncEvent != null ) ? true : false;
					onCollectionSyncEvent += handler;
					break;
				}

				case IProcEventAction.AddFileSync:
				{
					duplicateSubscriber = ( onFileSyncEvent != null ) ? true : false;
					onFileSyncEvent += handler;
					break;
				}

				case IProcEventAction.AddNotifyMessage:
				{
					duplicateSubscriber = ( onNotifyEvent != null ) ? true : false;
					onNotifyEvent += handler;
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

				case IProcEventAction.RemoveCollectionSync:
				{
					onCollectionSyncEvent -= handler;
					duplicateSubscriber = ( onCollectionSyncEvent != null ) ? true : false;
					break;
				}

				case IProcEventAction.RemoveFileSync:
				{
					onFileSyncEvent -= handler;
					duplicateSubscriber = ( onFileSyncEvent != null ) ? true : false;
					break;
				}

				case IProcEventAction.RemoveNotifyMessage:
				{
					onNotifyEvent -= handler;
					duplicateSubscriber = ( onNotifyEvent != null ) ? true : false;
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
		/// <param name="filters">An array of EventFilter objects.</param>
		private void SubscribeFilter( IProcEventFilter[] filters )
		{
			IProcEventListener message = new IProcEventListener();
			foreach ( IProcEventFilter filter in filters )
			{
				message.AddFilter( filter );
			}

			// Send the message.
			SendMessage( message.ToBuffer() );
		}

		/// <summary>
		/// Waits for changes to the configuration file.
		/// </summary>
		private void WaitForConfigFileChange()
		{
			// Wait for the change to occur and then reset the event.
			regServiceEvent.WaitOne();
			regServiceEvent.Reset();
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

			// Shutdown the connection.
			Shutdown();
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
				thread.IsBackground = true;
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
			lock ( subscribeLock )
			{
				// Queue the event so that if the client is not connected to the server, the event
				// can be registered after the connection is made.
				eventActionQueue.Enqueue( new EventActionQueueItem( action, handler ) );

				// Only let through if connected.
				if ( state == ClientState.Running )
				{
					SubscribeEvent( action, handler );
				}
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
			// Need to synchronize between this thread and the registration thread
			// so the filter always gets sent to the server.
			lock ( subscribeLock )
			{
				// Queue the event so that if the client is not connected to the server, 
				// the event can be registered after the connection is made.
				eventFilterQueue.Enqueue( filters );

				// See if the filter can be set right away.
				if ( state == ClientState.Running )
				{
					SubscribeFilter( filters );
				}
			}
		}
		#endregion
	}
}
