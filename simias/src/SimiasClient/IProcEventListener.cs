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
	/// Implements the parsing of event listener message to and from its serialized
	/// representation.
	/// </summary>
	public class IProcEventListener
	{
		#region Class Members
		/// <summary>
		/// Xml tags that define the event listener message.
		/// </summary>
		private static string EventListenerTag = "EventListener";
		private static string EventTag = "Event";
		private static string EventActionTag = "action";
		private static string FilterTag = "Filter";
		private static string FilterTypeTag = "type";

		/// <summary>
		/// Xml document used to hold event data.
		/// </summary>
		private XmlDocument document;
		#endregion

		#region Properties
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		public IProcEventListener()
		{
			document = new XmlDocument();
			XmlElement element = document.CreateElement( EventListenerTag );
			document.AppendChild( element );
		}

		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="document">Xml document that describes an IProcEventListener message.</param>
		public IProcEventListener( XmlDocument document )
		{
			this.document = document;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds the specified event to the registration.
		/// </summary>
		/// <param name="type">IProcEventAction to add.</param>
		public void AddEvent( IProcEventAction type )
		{
			XmlElement element = document.CreateElement( EventTag );
			element.SetAttribute( EventActionTag, type.ToString() );
			document.DocumentElement.AppendChild( element );
		}

		/// <summary>
		/// Adds the specified filter to the registration.
		/// </summary>
		/// <param name="filter">Filter to add.</param>
		public void AddFilter( IProcEventFilter filter )
		{
			XmlElement element = document.CreateElement( FilterTag );
			element.SetAttribute( FilterTypeTag, filter.Type.ToString() );
			element.InnerText = filter.Data;
			document.DocumentElement.AppendChild( element );
		}

		/// <summary>
		/// Method used by clients to enumerate the events in the IProcEventListener object.
		/// </summary>
		/// <returns>An IEnumerator object that can be used to get a list of EventTypes.</returns>
		public IEnumerator GetEventEnumerator()
		{
			return new EventEnumerator( document ).GetEnumerator();
		}

		/// <summary>
		/// Method used by clients to enumerate the filters in the IProcEventListener object.
		/// </summary>
		/// <returns>An IEnumerator object that can be used to get a list of IProcEventFilter objects.</returns>
		public IEnumerator GetFilterEnumerator()
		{
			return new FilterEnumerator( document ).GetEnumerator();
		}

		/// <summary>
		/// Returns whether document is a listen for event request.
		/// </summary>
		/// <param name="document">Xml document to check type for.</param>
		/// <returns>true if document is a listen for event request document, otherwise false is returned.</returns>
		static public bool IsValidRequest( XmlDocument document )
		{
			return ( document.DocumentElement.Name == EventListenerTag ) ? true : false;
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
		/// Converts an IProcEventListener object to a string representation.
		/// </summary>
		/// <returns>A string that represents the IProcEventListener object.</returns>
		public override string ToString()
		{
			return document.InnerXml;
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Implements the enumeration interface for events.
		/// </summary>
		private class EventEnumerator: IEnumerable, IEnumerator
		{
			#region Class Members
			/// <summary>
			/// Contains the enumeration for all event elements.
			/// </summary>
			private IEnumerator eventEnumerator;
			#endregion

			#region Constructor
			/// <summary>
			/// Initializes an instance of the object.
			/// </summary>
			/// <param name="document">Xml document that represents an IProcEventListener object.</param>
			public EventEnumerator( XmlDocument document )
			{
				XmlNodeList xnl = document.DocumentElement.SelectNodes( "child::" + EventTag );
				eventEnumerator = xnl.GetEnumerator();
			}
			#endregion

			#region IEnumerable Members
			/// <summary>
			/// Method used by clients to enumerate the events in the IProcEventListener object.
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
				eventEnumerator.Reset();
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public object Current
			{
				get 
				{ 
					XmlElement element = eventEnumerator.Current as XmlElement;
					return element.GetAttribute( EventActionTag );
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
				return eventEnumerator.MoveNext();
			}
			#endregion
		}

		/// <summary>
		/// Implements the enumeration interface for filters.
		/// </summary>
		private class FilterEnumerator : IEnumerable, IEnumerator
		{
			#region Class Members
			/// <summary>
			/// Contains the enumeration for all filter elements.
			/// </summary>
			private IEnumerator filterEnumerator;
			#endregion

			#region Constructor
			/// <summary>
			/// Initializes an instance of the object.
			/// </summary>
			/// <param name="document">Xml document that represents an IProcEventListener object.</param>
			public FilterEnumerator( XmlDocument document )
			{
				XmlNodeList xnl = document.DocumentElement.SelectNodes( "child::" + FilterTag );
				filterEnumerator = xnl.GetEnumerator();
			}
			#endregion

			#region IEnumerable Members
			/// <summary>
			/// Method used by clients to enumerate the events in the IProcEventListener object.
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
				filterEnumerator.Reset();
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public object Current
			{
				get 
				{
					XmlElement element = filterEnumerator.Current as XmlElement;
					return new IProcEventFilter( element.GetAttribute( FilterTypeTag ), element.InnerText );
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
				return filterEnumerator.MoveNext();
			}
			#endregion
		}
		#endregion
	}
}
