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
using System.Text;

namespace Simias.Event
{
	/// <summary>
	/// The event arguments for a Collection event.
	/// </summary>
	[Serializable]
	public class NodeEventArgs : CollectionEventArgs
	{
		#region Fields

		string					source;
		string					id;
		string					collection;
		string					type;
		int						eventId;
		
		#endregion

		#region Constructor

		/// <summary>
		/// Constructs a CollectionEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="node">The object of the event.</param>
		/// <param name="collection">The Collection that the node belongs to.</param>\
		/// <param name="type">The Type of the Node.</param>
		/// <param name="changeType">The type of change that occured.</param>
		public NodeEventArgs(string source, string node, string collection, string type, EventType changeType) :
			this(source, node, collection, type, changeType, 0)
		{
		}

		/// <summary>
		/// Constructs a CollectionEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="node">The object of the event.</param>
		/// <param name="collection">The Collection that the node belongs to.</param>\
		/// <param name="type">The Type of the Node.</param>
		/// <param name="changeType">The type of change that occured.</param>
		/// <param name="eventId">A user defined event ID. Only has meaning to a publisher.</param>
		public NodeEventArgs(string source, string node, string collection, string type, EventType changeType, int eventId) :
			base(changeType)
		{
			this.source = source;
			this.id = node;
			this.collection = collection;
			this.type = type;
			this.eventId = eventId;
		}

		internal NodeEventArgs(string args)
		{
			int index = 0;
			string [] aArgs = args.Split(seperatorChar);
			MarshallFromString(aArgs, ref index);
		}

		internal NodeEventArgs()
		{
		}

		#endregion

		#region Marshallers

		internal override string MarshallToString()
		{
            StringBuilder sb = new StringBuilder(base.MarshallToString());
			sb.Append(source + seperatorChar);
			sb.Append(id + seperatorChar);
			sb.Append(collection + seperatorChar);
			sb.Append(type + seperatorChar);
			sb.Append(eventId.ToString() + seperatorChar);
			return sb.ToString();
		}

		internal override void MarshallFromString(string [] args, ref int index)
		{
			base.MarshallFromString(args, ref index);
			source = args[index++];
			id = args[index++];
			collection = args[index++];
			type = args[index++];
			eventId = int.Parse(args[index++]);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the string that represents the source of the event.
		/// </summary>
		public string Source
		{
			get {return source;}
		}

		/// <summary>
		/// Gets the ID of the affected Node/Collection.
		/// </summary>
		public string ID
		{
			get {return id;}
		}
		
		/// <summary>
		/// Gets the containing collection ID.
		/// </summary>
		public string Collection
		{
			get {return collection;}
		}

		/// <summary>
		/// Gets the Type of the affected Node.
		/// </summary>
		public string Type
		{
			get {return type;}
		}

		/// <summary>
		/// Gets a Sets an event ID.  Usually 0. 
		/// Used by a publisher. Can be used to detect circular events.
		/// </summary>
		public int EventId
		{
			get {return eventId;}
		}

		/// <summary>
		/// Gets the Node ID.
		/// </summary>
		public string Node
		{
			get {return ID;}
		}

		#endregion
	}
}
