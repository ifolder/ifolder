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
using System.Text;
using System.Xml;

namespace Simias.Client.Event
{
	/// <summary>
	/// Implements the parsing of event registration data to and from is serialized
	/// representation.
	/// </summary>
	public class IProcEventRegistration
	{
		#region Class Members
		/// <summary>
		/// Xml tags that define the event registration.
		/// </summary>
		private static string EventRegistrationTag = "EventRegistration";
		private static string HostTag = "host";
		private static string PortTag = "port";

		/// <summary>
		/// Xml document used to hold event data.
		/// </summary>
		private XmlDocument document;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the remote address from the registration message.
		/// </summary>
		public IPAddress RemoteAddress
		{
			get { return IPAddress.Parse( document.DocumentElement.GetAttribute( HostTag ) ); }
		}

		/// <summary>
		/// Gets the remote port number from the registration message.
		/// </summary>
		public int Port
		{
			get { return Convert.ToInt32( document.DocumentElement.GetAttribute( PortTag ) ); }
		}

		/// <summary>
		/// Gets whether the client is registering.
		/// </summary>
		public bool Registering
		{
			get { return ( String.Compare( document.DocumentElement.InnerText, "true", true ) == 0 ) ? true : false; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="endPoint">End point that will listen for subscribed events.</param>
		/// <param name="registering">true if client is registering otherwise false is set.</param>
		public IProcEventRegistration( IPEndPoint endPoint, bool registering )
		{
			// Make sure that the loopback address is specified. For now we don't want to have
			// to deal with off-box events.
			if ( !IPAddress.IsLoopback( endPoint.Address ) )
			{
				throw new ApplicationException( "Only loopback address is allowed." );
			}

			document = new XmlDocument();
			XmlElement element = document.CreateElement( EventRegistrationTag );
			element.SetAttribute( HostTag, endPoint.Address.ToString() );
			element.SetAttribute( PortTag, endPoint.Port.ToString() );
			element.InnerText = registering.ToString();
			document.AppendChild( element );
		}

		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="document">An event registration request document.</param>
		public IProcEventRegistration( XmlDocument document )
		{
			this.document = document;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns whether document is a registration form request.
		/// </summary>
		/// <param name="document">Xml document to check type for.</param>
		/// <returns>true if document is a registration document, otherwise false is returned.</returns>
		static public bool IsValidRequest( XmlDocument document )
		{
			return ( document.DocumentElement.Name == EventRegistrationTag ) ? true : false;
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
		/// Converts an IProcEventRegistration object to a string representation.
		/// </summary>
		/// <returns>A string that represents the IProcEventRegistration object.</returns>
		public override string ToString()
		{
			return document.InnerXml;
		}
		#endregion
	}
}
