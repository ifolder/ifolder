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
using System.Text;
using System.Xml;

namespace Simias.Client.Event
{
	/// <summary>
	/// Describes the name-value pair contained in an IProcEventData object.
	/// </summary>
	public class IProcEventNameValue
	{
		#region Class Members
		/// <summary>
		/// Name of the value.
		/// </summary>
		private string name;

		/// <summary>
		/// Value.
		/// </summary>
		private string value;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the name.
		/// </summary>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		public string Value
		{
			get { return value; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="name">Name of the value.</param>
		/// <param name="value">String that represents the value.</param>
		public IProcEventNameValue( string name, string value )
		{
			this.name = name;
			this.value = value;
		}
		#endregion
	}

	/// <summary>
	/// Implements the parsing of event data to and from is serialized
	/// representation to an object.
	/// </summary>
	public class IProcEventData
	{
		#region Class Members
		/// <summary>
		/// Xml tags that define the event data.
		/// </summary>
		private static string EventTag = "Event";
		private static string EventTypeTag = "type";

		/// <summary>
		/// Xml tags used to describe a NodeEventArgs object.
		/// </summary>
		private const string NEA_ActionTag = "Action";
		private const string NEA_TimeTag = "Time";
		private const string NEA_SourceTag = "Source";
		private const string NEA_CollectionTag = "Collection";
		private const string NEA_TypeTag = "Type";
		private const string NEA_EventIDTag = "EventID";
		private const string NEA_NodeTag = "Node";
		private const string NEA_FlagsTag = "Flags";
		private const string NEA_MasterRevTag = "MasterRev";
		private const string NEA_SlaveRevTag = "SlaveRev";
		private const string NEA_FileSizeTag = "FileSize";

		/// <summary>
		/// Xml tags used to describe a CollectionSyncEventArgs object.
		/// </summary>
		private const string CEA_NameTag = "Name";
		private const string CEA_IDTag = "ID";
		private const string CEA_ActionTag = "Action";
		private const string CEA_ConnectedTag = "Connected";

		/// <summary>
		/// Xml tags used to describe a FileSyncEventArgs object.
		/// </summary>
		private const string FEA_CollectionIDTag = "CollectionID";
		private const string FEA_ObjectTypeTag = "ObjectType";
		private const string FEA_DeleteTag = "Delete";
		private const string FEA_NameTag = "Name";
		private const string FEA_SizeTag = "Size";
		private const string FEA_SizeToSyncTag = "SizeToSync";
		private const string FEA_SizeRemainingTag = "SizeRemaining";
		private const string FEA_DirectionTag = "Direction";

		/// <summary>
		/// Xml tags used to describe an NotifyEventArgs object.
		/// </summary>
		private const string NMA_MessageTag = "Message";
		private const string NMA_TimeTag = "Time";
		private const string NMA_TypeTag = "Type";
		
		/// <summary>
		/// Xml document used to hold event data.
		/// </summary>
		private XmlDocument document;
		#endregion

		#region Properties
		/// <summary>
		/// Returns the type of event this object represents.
		/// </summary>
		public string Type
		{
			get { return document.DocumentElement.GetAttribute( EventTypeTag ); }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="args">Information regarding the collection sync event.</param>
		public IProcEventData( CollectionSyncEventArgs args )
		{
			document = new XmlDocument();
			XmlElement element = document.CreateElement( EventTag );
			element.SetAttribute( EventTypeTag, typeof( CollectionSyncEventArgs ).Name );
			document.AppendChild( element );
			FromCollectionSyncEventArgs( args );
		}

		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="args">Information regarding the file sync event.</param>
		public IProcEventData( FileSyncEventArgs args )
		{
			document = new XmlDocument();
			XmlElement element = document.CreateElement( EventTag );
			element.SetAttribute( EventTypeTag, typeof( FileSyncEventArgs ).Name );
			document.AppendChild( element );
			FromFileSyncEventArgs( args );
		}

		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="args">Information regarding the node event.</param>
		public IProcEventData( NodeEventArgs args )
		{
			document = new XmlDocument();
			XmlElement element = document.CreateElement( EventTag );
			element.SetAttribute( EventTypeTag, typeof( NodeEventArgs ).Name );
			document.AppendChild( element );
			FromNodeEventArgs( args );
		}

		/// <summary>
		/// Initalizes an instance of the object.
		/// </summary>
		/// <param name="args">Information regarding the notify event.</param>
		public IProcEventData( NotifyEventArgs args )
		{
			document = new XmlDocument();
			XmlElement element = document.CreateElement( EventTag );
			element.SetAttribute( EventTypeTag, typeof( NotifyEventArgs ).Name );
			document.AppendChild( element );
			FromNotifyEventArgs( args );
		}

		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="document">Xml document that contains an IProcEventData message.</param>
		public IProcEventData( XmlDocument document )
		{
			this.document = document;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Translates the information in the NodeEventArgs object into the IProcEventData object.
		/// </summary>
		/// <param name="args">NodeEventArgs containing Node event information.</param>
		private void FromNodeEventArgs( NodeEventArgs args )
		{
			AddData( new IProcEventNameValue( NEA_ActionTag, args.EventData ) );
			AddData( new IProcEventNameValue( NEA_TimeTag, args.TimeStamp.Ticks.ToString() ) );
			AddData( new IProcEventNameValue( NEA_SourceTag, args.Source ) );
			AddData( new IProcEventNameValue( NEA_CollectionTag, args.Collection ) );
			AddData( new IProcEventNameValue( NEA_TypeTag, args.Type ) );
			AddData( new IProcEventNameValue( NEA_EventIDTag, args.EventId.ToString() ) );
			AddData( new IProcEventNameValue( NEA_NodeTag, args.Node ) );
			AddData( new IProcEventNameValue( NEA_FlagsTag, args.Flags.ToString() ) );
			AddData( new IProcEventNameValue( NEA_MasterRevTag, args.MasterRev.ToString() ) );
			AddData( new IProcEventNameValue( NEA_SlaveRevTag, args.SlaveRev.ToString() ) );
			AddData( new IProcEventNameValue( NEA_FileSizeTag, args.FileSize.ToString() ) );
		}

		/// <summary>
		/// Translates the information in the CollectionSyncEventArgs object into the IProcEventData object.
		/// </summary>
		/// <param name="args">CollectionSyncEventArgs containing Sync event information.</param>
		private void FromCollectionSyncEventArgs( CollectionSyncEventArgs args )
		{
			AddData( new IProcEventNameValue( CEA_NameTag, args.Name ) );
			AddData( new IProcEventNameValue( CEA_IDTag, args.ID ) );
			AddData( new IProcEventNameValue( CEA_ActionTag, args.Action.ToString() ) );
			AddData( new IProcEventNameValue( CEA_ConnectedTag, args.Connected.ToString() ) );
		}

		/// <summary>
		/// Translates the information in the FileSyncEventArgs object into the IProcEventData object.
		/// </summary>
		/// <param name="args">FileSyncEventArgs containing Sync event information.</param>
		private void FromFileSyncEventArgs( FileSyncEventArgs args )
		{
			AddData( new IProcEventNameValue( FEA_CollectionIDTag, args.CollectionID ) );
			AddData( new IProcEventNameValue( FEA_ObjectTypeTag, args.ObjectType.ToString() ) );
			AddData( new IProcEventNameValue( FEA_DeleteTag, args.Delete.ToString() ) );
			AddData( new IProcEventNameValue( FEA_NameTag, args.Name ) );
			AddData( new IProcEventNameValue( FEA_SizeTag, args.Size.ToString() ) );
			AddData( new IProcEventNameValue( FEA_SizeToSyncTag, args.SizeToSync.ToString() ) );
			AddData( new IProcEventNameValue( FEA_SizeRemainingTag, args.SizeRemaining.ToString() ) );
			AddData( new IProcEventNameValue( FEA_DirectionTag, args.Direction.ToString() ) );
		}

		/// <summary>
		/// Translates the information in the NotifyEventArgs object into the IProcEventData object.
		/// </summary>
		/// <param name="args">NotifyEventArgs containing the notify event information.</param>
		private void FromNotifyEventArgs( NotifyEventArgs args )
		{
			AddData( new IProcEventNameValue( NMA_TypeTag, args.EventData ) );
			AddData( new IProcEventNameValue( NMA_TimeTag, args.TimeStamp.Ticks.ToString() ) );
			AddData( new IProcEventNameValue( NMA_MessageTag, args.Message ) );
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds a name value pair to the event data.
		/// </summary>
		/// <param name="data">IProcEventNameValue object that contains the name value pair.</param>
		public void AddData( IProcEventNameValue data )
		{
			XmlElement element = document.CreateElement( data.Name );
			element.InnerText = data.Value;
			document.DocumentElement.AppendChild( element );
		}

		/// <summary>
		/// Method used by clients to enumerate the data in the IProcEventData object.
		/// </summary>
		/// <returns>An IEnumerator object that can be used to get a list of IProcEventNameValue objects.</returns>
		public IEnumerator GetEventEnumerator()
		{
			return new DataEnumerator( document ).GetEnumerator();
		}

		/// <summary>
		/// Converts an IProcEventRegistration object to a buffer representation.
		/// </summary>
		/// <returns>A byte array that represents the IProcEventRegistration object.</returns>
		public byte[] ToBuffer()
		{
			string msgString = ToString();
			UTF8Encoding utf8 = new UTF8Encoding();
			int msgLength = utf8.GetByteCount( msgString );
			byte[] msgHeader = BitConverter.GetBytes( msgLength );
			byte[] buffer = new byte[ msgHeader.Length + msgLength ];

			// Copy the message length and the message into the buffer.
			msgHeader.CopyTo( buffer, 0 );
			utf8.GetBytes( msgString, 0, msgString.Length, buffer, 4 );
			return buffer;
		}

		/// <summary>
		/// Converts the IProcEventData object into a CollectionSyncEventArgs object.
		/// </summary>
		/// <returns>A CollectionSyncEventArgs object.</returns>
		public CollectionSyncEventArgs ToCollectionSyncEventArgs()
		{
			// Preinitialize all of the node event arguments.
			string name = string.Empty;
			string ID = string.Empty;
			Action action = Action.StartSync;
			bool successful = true;

			// Walk through each named/value pair and convert the xml data back into CollectionSyncEventArgs data.
			foreach ( XmlNode xn in document.DocumentElement )
			{
				switch ( xn.Name )
				{
					case CEA_NameTag:
					{
						name = xn.InnerText;
						break;
					}

					case CEA_IDTag:
					{
						ID = xn.InnerText;
						break;
					}

					case CEA_ActionTag:
					{
						action = ( Action )Enum.Parse( typeof( Action ), xn.InnerText, false );
						break;
					}

					case CEA_ConnectedTag:
					{
						successful = Boolean.Parse( xn.InnerText );
						break;
					}
				}
			}
			
			// Create the object and set the flags.
			return new CollectionSyncEventArgs( name, ID, action, successful );
		}

		/// <summary>
		/// Converts the IProcEventData object into a FileSyncEventArgs object.
		/// </summary>
		/// <returns>A FileSyncEventArgs object.</returns>
		public FileSyncEventArgs ToFileSyncEventArgs()
		{
			// Preinitialize all of the node event arguments.
			string collectionID = string.Empty;
			ObjectType objectType = ObjectType.File;
			bool delete = false;
			string name = string.Empty;
			long size = 0;
			long sizeToSync = 0;
			long sizeRemaining = 0;
			Direction direction = Direction.Downloading;

			// Walk through each named/value pair and convert the xml data back into FileSyncEventArgs data.
			foreach ( XmlNode xn in document.DocumentElement )
			{
				switch ( xn.Name )
				{
					case FEA_CollectionIDTag:
					{
						collectionID = xn.InnerText;
						break;
					}
					case FEA_ObjectTypeTag:
					{
						objectType = ( ObjectType )Enum.Parse( typeof( ObjectType ), xn.InnerText, false );
						break;
					}
					case FEA_DeleteTag:
					{
						delete = Boolean.Parse( xn.InnerText );
						break;
					}
					case FEA_NameTag:
					{
						name = xn.InnerText;
						break;
					}

					case FEA_SizeTag:
					{
						size = Convert.ToInt64( xn.InnerText );
						break;
					}

					case FEA_SizeToSyncTag:
					{
						sizeToSync = Convert.ToInt64( xn.InnerText );
						break;
					}

					case FEA_SizeRemainingTag:
					{
						sizeRemaining = Convert.ToInt64( xn.InnerText );
						break;
					}

					case FEA_DirectionTag:
					{
						direction = ( Direction )Enum.Parse( typeof( Direction ), xn.InnerText, false );
						break;
					}
				}
			}
			
			// Create the object and set the flags.
			return new FileSyncEventArgs( collectionID, objectType, delete, name, size, sizeToSync, sizeRemaining, direction );
		}

		/// <summary>
		/// Converts the IProcEventData object into a NodeEventArgs object.
		/// </summary>
		/// <returns>A NodeEventArgs object.</returns>
		public NodeEventArgs ToNodeEventArgs()
		{
			// Preinitialize all of the node event arguments.
			EventType changeType = EventType.NodeChanged;
			DateTime time = DateTime.MinValue;
			string source = string.Empty;
			string node = string.Empty;
			string collection = string.Empty;
			string type = string.Empty;
			int eventID = 0;
			ushort flags = 0;
			ulong masterRev = 0;
			ulong slaveRev = 0;
			long fileSize = 0;

			// Walk through each named/value pair and convert the xml data back into NodeEventArgs data.
			foreach ( XmlNode xn in document.DocumentElement )
			{
				switch ( xn.Name )
				{
					case NEA_ActionTag:
					{
						changeType = ( EventType )Enum.Parse( typeof( EventType ), xn.InnerText );
						break;
					}

					case NEA_TimeTag:
					{
						time = new DateTime( Convert.ToInt64( xn.InnerText ) );
						break;
					}

					case NEA_SourceTag:
					{
						source = xn.InnerText;
						break;
					}

					case NEA_CollectionTag:
					{
						collection = xn.InnerText;
						break;
					}

					case NEA_TypeTag:
					{
						type = xn.InnerText;
						break;
					}

					case NEA_EventIDTag:
					{
						eventID = Convert.ToInt32( xn.InnerText );
						break;
					}

					case NEA_NodeTag:
					{
						node = xn.InnerText;
						break;
					}
						
					case NEA_FlagsTag:
					{
						flags = Convert.ToUInt16( xn.InnerText );
						break;
					}
						
					case NEA_MasterRevTag:
					{
						masterRev = Convert.ToUInt64( xn.InnerText );
						break;
					}

					case NEA_SlaveRevTag:
					{
						slaveRev = Convert.ToUInt64( xn.InnerText );
						break;
					}

					case NEA_FileSizeTag:
					{
						fileSize = Convert.ToInt64( xn.InnerText );
						break;
					}
				}
			}
			
			// Create the object and set the flags.
			NodeEventArgs args = new NodeEventArgs( source, node, collection, type, changeType, eventID, time, masterRev, slaveRev, fileSize );
			args.Flags = flags;
			return args;
		}

		/// <summary>
		/// Converts the IProcEventData object into a NotifyEventArgs object.
		/// </summary>
		/// <returns>A NotifyEventArgs object.</returns>
		public NotifyEventArgs ToNotifyEventArgs()
		{
			// Preinitialize all of the node event arguments.
			string type = string.Empty;
			string message = string.Empty;
			DateTime time = DateTime.MinValue;

			// Walk through each named/value pair and convert the xml data back into NotifyEventArgs data.
			foreach ( XmlNode xn in document.DocumentElement )
			{
				switch ( xn.Name )
				{
					case NMA_TypeTag:
					{
						type = xn.InnerText;
						break;
					}

					case NMA_MessageTag:
					{
						message = xn.InnerText;
						break;
					}

					case NMA_TimeTag:
					{
						time = new DateTime( Convert.ToInt64( xn.InnerText ) );
						break;
					}
				}
			}
			
			// Create the object and set the flags.
			return new NotifyEventArgs( type, message, time );
		}

		/// <summary>
		/// Converts an IProcEventData object to a string representation.
		/// </summary>
		/// <returns>A string that represents the IProcEventData object.</returns>
		public override string ToString()
		{
			return document.OuterXml;
		}
		#endregion

		#region IEnumerator Members
		private class DataEnumerator : IEnumerable, IEnumerator
		{
			#region Class Members
			/// <summary>
			/// Contains the enumeration for all data elements.
			/// </summary>
			private IEnumerator dataEnumerator;
			#endregion

			#region Constructor
			/// <summary>
			/// Initializes an instance of the object.
			/// </summary>
			/// <param name="document">Xml document that represents an IProcEventData object.</param>
			public DataEnumerator( XmlDocument document )
			{
				dataEnumerator = document.DocumentElement.GetEnumerator();
			}
			#endregion

			#region IEnumerable Members
			/// <summary>
			/// Method used by clients to enumerate the data in the IProcEventData object.
			/// </summary>
			/// <returns>An IEnumerator.</returns>
			public IEnumerator GetEnumerator()
			{
				return this;
			}
			#endregion

			#region IEnumerator Members
			/// <summary>
			/// Sets the enumerator to its initial position, which is before
			/// the first element in the collection.
			/// </summary>
			public void Reset()
			{
				dataEnumerator.Reset();
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public object Current
			{
				get 
				{ 
					XmlElement element = dataEnumerator.Current as XmlElement; 
					return new IProcEventNameValue( element.Name, element.InnerText );
				}
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>
			/// true if the enumerator was successfully advanced to the next element; 
			/// false if the enumerator has passed the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				return dataEnumerator.MoveNext();
			}
			#endregion
		}
		#endregion
	}
}
