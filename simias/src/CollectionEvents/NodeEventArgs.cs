/***********************************************************************
 *  NodeEventArgs.cs - Argument class for node events.
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Russ Young <ryoung@novell.com>
 * 
 ***********************************************************************/

using System;

namespace Simias.Event
{
	/// <summary>
	/// The event arguments for a Collection event.
	/// </summary>
	[Serializable]
	public class NodeEventArgs : CollectionEventArgs
	{
		/// <summary>
		/// Constructs a CollectionEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="node">The object of the event.</param>
		/// <param name="collection">The Collection that the node belongs to.</param>\
		/// <param name="domainName">The domainName from the store that the collection belongs to.</param>
		/// <param name="type">The Type of the Node.</param>
		/// <param name="changeType">The type of change that occured.</param>
		public NodeEventArgs(string source, string node, string collection, string domainName, string type, EventType changeType) :
			this(source, node, collection, domainName, type, changeType, 0)
		{
		}

		/// <summary>
		/// Constructs a CollectionEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="node">The object of the event.</param>
		/// <param name="collection">The Collection that the node belongs to.</param>\
		/// <param name="domainName">The domainName from the store that the collection belongs to.</param>
		/// <param name="type">The Type of the Node.</param>
		/// <param name="changeType">The type of change that occured.</param>
		/// <param name="eventId">A user defined event ID. Only has meaning to a publisher.</param>
		public NodeEventArgs(string source, string node, string collection, string domainName, string type, EventType changeType, int eventId) :
			base(source, node, collection, domainName, type, changeType, eventId)
		{
		}
		
		/// <summary>
		/// Gets the Node ID.
		/// </summary>
		public string Node
		{
			get {return ID;}
		}
	}
}
