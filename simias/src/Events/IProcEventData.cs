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

using Simias.Storage;

namespace Simias.Event
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
		private const string ActionTag = "Action";
		private const string TimeTag = "Time";
		private const string SourceTag = "Source";
		private const string CollectionTag = "Collection";
		private const string TypeTag = "Type";
		private const string EventIDTag = "EventID";
		private const string NodeTag = "Node";
		private const string FlagsTag = "Flags";
		private const string MasterRevTag = "MasterRev";
		private const string SlaveRevTag = "SlaveRev";
		private const string FileSizeTag = "FileSize";

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
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="args">Information regarding the node event.</param>
//		public IProcEventData( SyncEventArgs args )
//		{
//			document = new XmlDocument();
//			XmlElement element = document.CreateElement( EventTag );
//			element.SetAttribute( EventTypeTag, typeof( SyncEventArgs ).Name );
//			document.AppendChild( element );
//			FromSyncEventArgs( args );
//		}

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
			AddData( new IProcEventNameValue( ActionTag, args.EventData ) );
			AddData( new IProcEventNameValue( TimeTag, args.TimeStamp.Ticks.ToString() ) );
			AddData( new IProcEventNameValue( SourceTag, args.Source ) );
			AddData( new IProcEventNameValue( CollectionTag, args.Collection ) );
			AddData( new IProcEventNameValue( TypeTag, args.Type ) );
			AddData( new IProcEventNameValue( EventIDTag, args.EventId.ToString() ) );
			AddData( new IProcEventNameValue( NodeTag, args.Node ) );
			AddData( new IProcEventNameValue( FlagsTag, args.Flags.ToString() ) );
			AddData( new IProcEventNameValue( MasterRevTag, args.MasterRev.ToString() ) );
			AddData( new IProcEventNameValue( SlaveRevTag, args.SlaveRev.ToString() ) );
			AddData( new IProcEventNameValue( FileSizeTag, args.FileSize.ToString() ) );
		}

		/// <summary>
		/// Translates the information in the SyncEventArgs object into the IProcEventData object.
		/// </summary>
		/// <param name="args">SyncEventArgs containing Sync event information.</param>
//		private void FromSyncEventArgs( SyncEventArgs args )
//		{
//			AddData( new IProcEventNameValue( ActionTag, args.EventData ) );
//			AddData( new IProcEventNameValue( TimeTag, args.TimeStamp.Ticks.ToString() ) );
//			AddData( new IProcEventNameValue( SourceTag, args.Source ) );
//			AddData( new IProcEventNameValue( CollectionTag, args.Collection ) );
//			AddData( new IProcEventNameValue( TypeTag, args.Type ) );
//			AddData( new IProcEventNameValue( EventIDTag, args.EventId.ToString() ) );
//			AddData( new IProcEventNameValue( NodeTag, args.Node ) );
//			AddData( new IProcEventNameValue( FlagsTag, args.Flags.ToString() ) );
//			AddData( new IProcEventNameValue( MasterRevTag, args.MasterRev.ToString() ) );
//			AddData( new IProcEventNameValue( SlaveRevTag, args.SlaveRev.ToString() ) );
//			AddData( new IProcEventNameValue( FileSizeTag, args.FileSize.ToString() ) );
//		}
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
			byte[] msgHeader = BitConverter.GetBytes( msgString.Length );
			byte[] buffer = new byte[ msgHeader.Length + msgString.Length ];

			// Copy the message length and the message into the buffer.
			msgHeader.CopyTo( buffer, 0 );
			new ASCIIEncoding().GetBytes( msgString, 0, msgString.Length, buffer, 4 );
			return buffer;
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
					case ActionTag:
					{
						changeType = ( EventType )Enum.Parse( typeof( EventType ), xn.InnerText );
						break;
					}

					case TimeTag:
					{
						time = new DateTime( Convert.ToInt64( xn.InnerText ) );
						break;
					}

					case SourceTag:
					{
						source = xn.InnerText;
						break;
					}

					case CollectionTag:
					{
						collection = xn.InnerText;
						break;
					}

					case TypeTag:
					{
						type = xn.InnerText;
						break;
					}

					case EventIDTag:
					{
						eventID = Convert.ToInt32( xn.InnerText );
						break;
					}

					case NodeTag:
					{
						node = xn.InnerText;
						break;
					}
						
					case FlagsTag:
					{
						flags = Convert.ToUInt16( xn.InnerText );
						break;
					}
						
					case MasterRevTag:
					{
						masterRev = Convert.ToUInt64( xn.InnerText );
						break;
					}

					case SlaveRevTag:
					{
						slaveRev = Convert.ToUInt64( xn.InnerText );
						break;
					}

					case FileSizeTag:
					{
						fileSize = Convert.ToInt64( xn.InnerText );
						break;
					}
				}
			}
			
			// Create the object and set the flags.
			NodeEventArgs args = new NodeEventArgs( source, node, collection, type, changeType, eventID, time, masterRev, slaveRev, fileSize );
//			args.Flags = flags;
			return args;
		}

		/// <summary>
		/// Converts an IProcEventData object to a string representation.
		/// </summary>
		/// <returns>A string that represents the IProcEventData object.</returns>
		public override string ToString()
		{
			return document.InnerXml;
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
