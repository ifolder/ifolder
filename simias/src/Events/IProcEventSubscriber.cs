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
using Simias.Storage;

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
		/// Subscriber to simias events.
		/// </summary>
		private EventSubscriber simiasEventSubscriber = null;
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
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Callback used to indicate that a Node object has changed.
		/// </summary>
		/// <param name="args">Arguments that give information about the Node object that has changed.</param>
		private void NodeEventHandler( NodeEventArgs args )
		{
			try
			{
				// Send the event to the client.
				eventSocket.Send( new IProcEventData( args ).ToBuffer() );
			}
			catch ( SocketException e )
			{
				log.Error( e, "Error processing NodeEventHandler event for client {0}.", ( eventSocket.RemoteEndPoint as IPEndPoint ).Address );
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
						simiasEventSubscriber.CollectionId = filter.Data;
						break;
					}

					case IProcFilterType.NodeID:
					{
						simiasEventSubscriber.NodeIDFilter = filter.Data;
						break;
					}

					case IProcFilterType.NodeType:
					{
						simiasEventSubscriber.NodeTypeFilter = filter.Data;
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
						simiasEventSubscriber.NodeChanged += new NodeEventHandler( NodeEventHandler );
						break;
					}

					case IProcEventAction.AddNodeCreated:
					{
						simiasEventSubscriber.NodeCreated += new NodeEventHandler( NodeEventHandler );
						break;
					}

					case IProcEventAction.AddNodeDeleted:
					{
						simiasEventSubscriber.NodeDeleted += new NodeEventHandler( NodeEventHandler );
						break;
					}

					case IProcEventAction.AddCollectionSync:
					{
						break;
					}

					case IProcEventAction.AddFileSync:
					{
						break;
					}

					case IProcEventAction.RemoveNodeChanged:
					{
						simiasEventSubscriber.NodeChanged -= new NodeEventHandler( NodeEventHandler );
						break;
					}

					case IProcEventAction.RemoveNodeCreated:
					{
						simiasEventSubscriber.NodeCreated -= new NodeEventHandler( NodeEventHandler );
						break;
					}

					case IProcEventAction.RemoveNodeDeleted:
					{
						simiasEventSubscriber.NodeDeleted -= new NodeEventHandler( NodeEventHandler );
						break;
					}

					case IProcEventAction.RemoveCollectionSync:
					{
						break;
					}

					case IProcEventAction.RemoveFileSync:
					{
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
				}

				// See if the client is registering or deregistering.
				if ( er.Registering )
				{
					// Create an event subscriber.
					simiasEventSubscriber = new EventSubscriber();
					log.Debug( "Client {0}:{1} has registered for interprocess events", er.RemoteAddress, er.Port );
				}
				else
				{
					if ( simiasEventSubscriber != null )
					{
						simiasEventSubscriber.Dispose();
						simiasEventSubscriber = null;
					}

					log.Debug( "Client {0}:{1} has deregistered for interprocess events", er.RemoteAddress, er.Port );
				}
			}
			else if ( IProcEventListener.IsValidRequest( document ) )
			{
				// Make sure that registration has occurred.
				if ( simiasEventSubscriber == null )
				{
					log.Error( "Client must be registered before subscribing for events." );
					throw new SimiasException( "Client must be registered before subscribing for events." );
				}

				IProcEventListener el = new IProcEventListener( document );
				ProcessEventListener( el );
			}
			else
			{
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
						log.Error( e, "Error processing event message from client." );
					}

					// Repost the buffer.
					sub.eventSocket.BeginReceive( sub.receiveBuffer, sub.bufferLength, sub.receiveBuffer.Length - sub.bufferLength, SocketFlags.None, new AsyncCallback( MessageHandler ), sub );
				}
				else
				{
					// The client is going away. Clean up his events.
					log.Debug( "Client {0} has terminated the connection.", ( sub.eventSocket.RemoteEndPoint as IPEndPoint ).Address );

					if ( sub.simiasEventSubscriber != null )
					{
						sub.simiasEventSubscriber.Dispose();
					}

					sub.eventSocket.Shutdown( SocketShutdown.Both );
					sub.eventSocket.Close();
				}
			}
			catch ( SocketException e )
			{
				log.Error( e, "Error communication failure." );
				sub.eventSocket.Close();
			}
		}
		#endregion

		#region Public Methods
		#endregion
	}
}
