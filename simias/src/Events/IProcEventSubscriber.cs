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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;

using Simias;
using Simias.Client.Event;
using Simias.Event;
using Simias.Storage;
using Simias.Sync;

namespace Simias.Event
{
	/// <summary>
	/// Summary description for IProcEventSubscriber.
	/// </summary>
	internal class IProcEventSubscriber
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( IProcEventSubscriber ) );

		/// <summary>
		/// Table used to keep track of all subscriber objects.
		/// </summary>
		static private Hashtable subscriberTable = Hashtable.Synchronized( new Hashtable() );

		/// <summary>
		/// Socket that is used to communicate the event to the client.
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
		/// Port that is used as key to identify this client.
		/// </summary>
		private int port;

		/// <summary>
		/// Subscribers to simias events.
		/// </summary>
		private EventSubscriber simiasNodeEventSubscriber = null;
		private SyncEventSubscriber simiasSyncEventSubscriber = null;
		private NotifyEventSubscriber simiasNotifyEventSubscriber = null;

		/// <summary>
		/// Event handlers defined for this server.
		/// </summary>
		private NodeEventHandler nodeChangedHandler = null;
		private NodeEventHandler nodeCreatedHandler = null;
		private NodeEventHandler nodeDeletedHandler = null;
		private CollectionSyncEventHandler collectionSyncHandler = null;
		private FileSyncEventHandler fileSyncHandler = null;
		private NotifyEventHandler notifyHandler = null;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the receive buffer for the subscriber.
		/// </summary>
		public byte[] ReceiveBuffer
		{
			get { return receiveBuffer; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object.
		/// </summary>
		/// <param name="socket">The Socket object that is associated with this request.</param>
		public IProcEventSubscriber( Socket socket )
		{
			eventSocket = socket;
			
			// Turn off nagle.
			socket.SetSocketOption( SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1 );

			// Add this object to the table.
			port = ( socket.LocalEndPoint as IPEndPoint ).Port;
			if ( subscriberTable.ContainsKey( port ) )
			{
				log.Debug( "{0} : Old subscriber was left around", port );
			}

			subscriberTable[ port ] = this;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Callback used to indicate that a collection is beginning or ending a sync cycle.
		/// </summary>
		/// <param name="args">Arguments that give information about the collection.</param>
		private void CollectionSyncEventCallback( CollectionSyncEventArgs args )
		{
			try
			{
				// Send the event to the client.
				byte[] buffer = new IProcEventData( args ).ToBuffer();
				eventSocket.BeginSend( buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback( EventSendComplete ), this );
			}
			catch ( Exception e )
			{
				DisposeSubscribers();
				log.Error( e, String.Format( "{0} : Error processing CollectionSyncEventCallback event for client.", port ) );
			}
		}

		/// <summary>
		/// Disposes the event subscribers.
		/// </summary>
		private void DisposeSubscribers()
		{
			log.Debug( "{0} : Disposing all subscribers.", port );

			if ( simiasNodeEventSubscriber != null )
			{
				simiasNodeEventSubscriber.Dispose();
				simiasNodeEventSubscriber = null;
			}

			if ( simiasSyncEventSubscriber != null )
			{
				simiasSyncEventSubscriber.Dispose();
				simiasSyncEventSubscriber = null;
			}

			if ( simiasNotifyEventSubscriber != null )
			{
				simiasNotifyEventSubscriber.Dispose();
				simiasNotifyEventSubscriber = null;
			}
		}

		/// <summary>
		/// Callback routine for handling send completes.
		/// </summary>
		/// <param name="result">The result of the asynchronous operation.</param>
		static private void EventSendComplete( IAsyncResult result )
		{
			// Get the subscriber object as context for this request.
			IProcEventSubscriber sub = null;
			try
			{
				sub = ( IProcEventSubscriber )result.AsyncState;
				sub.eventSocket.EndSend( result );
			}
			catch ( Exception e )
			{
				log.Error( e, String.Format( "{0} : Error in send complete.", sub.port ) );
			}
		}

		/// <summary>
		/// Callback used to indicate that a file is being synchronized.
		/// </summary>
		/// <param name="args">Arguments that give information about the file.</param>
		private void FileSyncEventCallback( FileSyncEventArgs args )
		{
			try
			{
				// Send the event to the client.
				byte[] buffer = new IProcEventData( args ).ToBuffer();
				eventSocket.BeginSend( buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback( EventSendComplete ), this );
			}
			catch ( Exception e )
			{
				DisposeSubscribers();
				log.Error( e, String.Format( "{0} : Error processing FileSyncEventCallback event for client.", port ) );
			}
		}

		/// <summary>
		/// Callback used to indicate that a Node object has changed.
		/// </summary>
		/// <param name="args">Arguments that give information about the Node object that has changed.</param>
		private void NodeEventCallback( NodeEventArgs args )
		{
			try
			{
				// Send the event to the client.
				byte[] buffer = new IProcEventData( args ).ToBuffer();
				eventSocket.BeginSend( buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback( EventSendComplete ), this );
			}
			catch ( Exception e )
			{
				DisposeSubscribers();
				log.Error( e, String.Format( "{0} : Error processing NodeEventCallback event for client.", port ) );
			}
		}

		/// <summary>
		/// Callback used to indicate that a notify message has been generated.
		/// </summary>
		/// <param name="args">Arguments that give information about the notification.</param>
		private void NotifyEventCallback( NotifyEventArgs args )
		{
			try
			{
				// Send the event to the client.
				byte[] buffer = new IProcEventData( args ).ToBuffer();
				eventSocket.BeginSend( buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback( EventSendComplete ), this );
			}
			catch ( Exception e )
			{
				DisposeSubscribers();
				log.Error( e, String.Format( "{0} : Error processing NotifyEventCallback event for client.", port ) );
			}
		}

		/// <summary>
		/// Processes event add, delete and filter requests.
		/// </summary>
		/// <param name="document">Document that contains the event message.</param>
		private void ProcessEventListener( IProcEventListener document )
		{
			// See if there are any filter events to set.
			IEnumerator e = document.GetFilterEnumerator();
			while ( e.MoveNext() )
			{
				IProcEventFilter filter = e.Current as IProcEventFilter;
				switch ( filter.Type )
				{
					case IProcFilterType.Collection:
					{
						simiasNodeEventSubscriber.CollectionId = filter.Data;
						break;
					}

					case IProcFilterType.NodeID:
					{
						simiasNodeEventSubscriber.NodeIDFilter = filter.Data;
						break;
					}

					case IProcFilterType.NodeType:
					{
						simiasNodeEventSubscriber.NodeTypeFilter = filter.Data;
						break;
					}
				}
			}

			// See if there are any events to add or delete.
			e = document.GetEventEnumerator();
			while ( e.MoveNext() )
			{
				switch ( ( IProcEventAction )Enum.Parse( typeof( IProcEventAction ), e.Current as string ) )
				{
					case IProcEventAction.AddNodeChanged:
					{
						if ( nodeChangedHandler == null )
						{
							log.Debug( "{0} : Added node change event.", port );
							nodeChangedHandler = new NodeEventHandler( NodeEventCallback );
							simiasNodeEventSubscriber.NodeChanged += nodeChangedHandler;
						}
						break;
					}

					case IProcEventAction.AddNodeCreated:
					{
						if ( nodeCreatedHandler == null )
						{
							log.Debug( "{0} : Added node create event.", port );
							nodeCreatedHandler = new NodeEventHandler( NodeEventCallback );
							simiasNodeEventSubscriber.NodeCreated += nodeCreatedHandler;
						}
						break;
					}

					case IProcEventAction.AddNodeDeleted:
					{
						if ( nodeDeletedHandler == null )
						{
							log.Debug( "{0} : Added node delete event.", port );
							nodeDeletedHandler = new NodeEventHandler( NodeEventCallback );
							simiasNodeEventSubscriber.NodeDeleted += nodeDeletedHandler;
						}
						break;
					}

					case IProcEventAction.AddCollectionSync:
					{
						if ( collectionSyncHandler == null )
						{
							log.Debug( "{0} : Added collection sync event.", port );
							collectionSyncHandler = new CollectionSyncEventHandler( CollectionSyncEventCallback );
							simiasSyncEventSubscriber.CollectionSync += collectionSyncHandler;
						}
						break;
					}

					case IProcEventAction.AddFileSync:
					{
						if ( fileSyncHandler == null )
						{
							log.Debug( "{0} : Added file sync event.", port );
							fileSyncHandler = new FileSyncEventHandler( FileSyncEventCallback );
							simiasSyncEventSubscriber.FileSync += fileSyncHandler;
						}
						break;
					}

					case IProcEventAction.AddNotifyMessage:
					{
						if ( notifyHandler == null )
						{
							log.Debug( "{0} : Added notify event.", port );
							notifyHandler = new NotifyEventHandler( NotifyEventCallback );
							simiasNotifyEventSubscriber.NotifyEvent += notifyHandler;
						}
						break;
					}

					case IProcEventAction.RemoveNodeChanged:
					{
						if ( nodeChangedHandler != null )
						{
							log.Debug( "{0} : Removed node change event.", port );
							simiasNodeEventSubscriber.NodeChanged -= nodeChangedHandler;
							nodeChangedHandler = null;
						}
						break;
					}

					case IProcEventAction.RemoveNodeCreated:
					{
						if ( nodeCreatedHandler != null )
						{
							log.Debug( "{0} : Removed node created event.", port );
							simiasNodeEventSubscriber.NodeCreated -= nodeCreatedHandler;
							nodeCreatedHandler = null;
						}
						break;
					}

					case IProcEventAction.RemoveNodeDeleted:
					{
						if ( nodeDeletedHandler != null )
						{
							log.Debug( "{0} : Removed node deleted event.", port );
							simiasNodeEventSubscriber.NodeDeleted -= nodeDeletedHandler;
							nodeDeletedHandler = null;
						}
						break;
					}

					case IProcEventAction.RemoveCollectionSync:
					{
						if ( collectionSyncHandler != null )
						{
							log.Debug( "{0} : Removed collection sync event.", port );
							simiasSyncEventSubscriber.CollectionSync -= collectionSyncHandler;
							collectionSyncHandler = null;
						}
						break;
					}

					case IProcEventAction.RemoveFileSync:
					{
						if ( fileSyncHandler != null )
						{
							log.Debug( "{0} : Removed file sync event.", port );
							simiasSyncEventSubscriber.FileSync -= fileSyncHandler;
							fileSyncHandler = null;
						}
						break;
					}

					case IProcEventAction.RemoveNotifyMessage:
					{
						if ( notifyHandler != null )
						{
							log.Debug( "{0} : Removed notify event.", port );
							simiasNotifyEventSubscriber.NotifyEvent -= notifyHandler;
							notifyHandler = null;
						}
						break;
					}
				}
			}
		}

		/// <summary>
		/// Processes the messages received from the event client.
		/// </summary>
		/// <param name="message">Message received from the client.</param>
		private void ProcessMessage( string message )
		{
			// Turn the message into an xml document.
			XmlDocument document = new XmlDocument();
			document.LoadXml( message );

			// Find out what type of request message was sent.
			if ( IProcEventRegistration.IsValidRequest( document ) )
			{
				IProcEventRegistration er = new IProcEventRegistration( document );
				
				// Make sure that the host address is loopback for now.
				if ( !IPAddress.IsLoopback( er.RemoteAddress ) )
				{
					log.Error( String.Format( "An invalid address was specified in the registration message {0}.", er.RemoteAddress ) );
					eventSocket.Shutdown( SocketShutdown.Both );
					eventSocket.Close();
					subscriberTable.Remove( port );
				}

				// See if the client is registering or deregistering.
				if ( er.Registering )
				{
					// Create the event subscribers.
					simiasNodeEventSubscriber = new EventSubscriber();
					simiasSyncEventSubscriber = new SyncEventSubscriber();
					simiasNotifyEventSubscriber = new NotifyEventSubscriber();
					log.Debug( "Client {0}:{1} has registered for interprocess events", er.RemoteAddress, er.Port );
				}
				else
				{
					DisposeSubscribers();
					log.Debug( "Client {0}:{1} has deregistered for interprocess events", er.RemoteAddress, er.Port );
				}
			}
			else if ( IProcEventListener.IsValidRequest( document ) )
			{
				// Make sure that registration has occurred.
				if ( ( simiasNodeEventSubscriber == null ) || 
					 ( simiasSyncEventSubscriber == null ) ||
					 ( simiasNodeEventSubscriber == null ) )
				{
					log.Error( "Client must be registered before subscribing for events." );
					throw new SimiasException( "Client must be registered before subscribing for events." );
				}

				IProcEventListener el = new IProcEventListener( document );
				ProcessEventListener( el );
			}
			else
			{
				log.Debug( "{0} : An invalid request message was received.", port );
				throw new SimiasException( "An invalid request message was received." );
			}
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Processes incoming messages intended for this subscriber.
		/// </summary>
		/// <param name="result">The result of the asynchronous operation.</param>
		static internal void MessageHandler( IAsyncResult result )
		{
			// Get the subscriber object as context for this request.
			IProcEventSubscriber sub = ( IProcEventSubscriber )result.AsyncState;

			try
			{
				// End the pending read operation.
				int bytesReceived = sub.eventSocket.EndReceive( result );
				if ( bytesReceived > 0 )
				{
					try
					{
						// Keep track of how much data is in the buffer.
						sub.bufferLength += bytesReceived;
						int bytesToProcess = sub.bufferLength;
						int msgIndex = 0;

						// Process as much as is available. 
						while ( bytesToProcess > 0 )
						{
							// There needs to be atleast 4 bytes for the message length.
							if ( bytesToProcess >= 4 )
							{
								// Get the length of the message, add in the prepended length.
								int msgLength = BitConverter.ToInt32( sub.receiveBuffer, msgIndex ) + 4;
							
								// See if the entire message is represented in the buffer.
								if ( bytesToProcess >= msgLength )
								{
									// Process the message received from the client. Skip the message length.
									sub.ProcessMessage( new ASCIIEncoding().GetString( sub.receiveBuffer, msgIndex + 4, msgLength - 4 ) );
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
						sub.bufferLength = bytesToProcess;

						// If any data has been processed at the beginning of the buffer, the unprocessed
						// data needs to be moved to the front.
						if ( ( bytesToProcess > 0 ) && ( msgIndex > 0 ) )
						{
							// Move any existing data up.
							Buffer.BlockCopy( sub.receiveBuffer, msgIndex, sub.receiveBuffer, 0, bytesToProcess );
						}
					}
					catch ( Exception e )
					{
						log.Error( e, "{0} : Error processing event message from client.", sub.port );
					}

					// Repost the buffer.
					sub.eventSocket.BeginReceive( sub.receiveBuffer, sub.bufferLength, sub.receiveBuffer.Length - sub.bufferLength, SocketFlags.None, new AsyncCallback( MessageHandler ), sub );
				}
				else
				{
					// The client is going away. Clean up his events.
					log.Debug( "Client {0} has terminated the connection.", ( sub.eventSocket.RemoteEndPoint as IPEndPoint ).Address );

					sub.DisposeSubscribers();
					sub.eventSocket.Shutdown( SocketShutdown.Both );
					sub.eventSocket.Close();
					IProcEventSubscriber.subscriberTable.Remove( sub.port );
				}
			}
			catch ( SocketException e )
			{
				log.Error( e, "{0} : Error communication failure.", sub.port );
				IProcEventSubscriber.subscriberTable.Remove( sub.port );
			}
			catch ( Exception e )
			{
				log.Error( e, "{0} : Error exception occurred.", sub.port );
				IProcEventSubscriber.subscriberTable.Remove( sub.port );
			}
		}
		#endregion
	}
}
