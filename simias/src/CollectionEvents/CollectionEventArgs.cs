/***********************************************************************
 *  CollectionEventArgs.cs - Argument class for collection events.
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
	public abstract class CollectionEventArgs : EventArgs
	{
		string					source;
		string					id;
		string					collection;
		string					domainName;
		string					type;
		EventType				changeType;

		/// <summary>
		/// The Event types supported.
		/// </summary>
		[FlagsAttribute] 
		public enum EventType : short
		{
			/// <summary>
			/// The event is for a create.
			/// </summary>
			Created = 1,
			/// <summary>
			/// The event is for a delete.
			/// </summary>
			Deleted = 2,
			/// <summary>
			/// The event is for a change.
			/// </summary>
			Changed = 4,
			/// <summary>
			/// The event is for a rename.
			/// </summary>
			Renamed = 8,
			/// <summary>
			/// The event is for a collection root path change.
			/// </summary>
			RootChanged = 16
		};


		/// <summary>
		/// Constructs a CollectionEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="id">The object of the event.</param>
		/// <param name="collection">The Collection that the node belongs to.</param>
		/// <param name="domainName">The domainName from the store that the collection belongs to.</param>
		/// <param name="type">The Type of the Node.</param>
		/// <param name="changeType">The type of change that occured.</param>
		internal CollectionEventArgs(string source, string id, string collection, string domainName, string type, EventType changeType)
		{
			this.source = source;
			this.id = id;
			this.collection = collection;
			this.domainName = domainName;
			this.type = type;
			this.changeType = changeType;
		}

		/// <summary>
		/// Gets the string that represents the source of the event.
		/// </summary>
		public string Source
		{
			get {return Source;}
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
		/// Gets the DomainName for the event.
		/// </summary>
		public string DomainName
		{
			get {return domainName;}
		}

		/// <summary>
		/// Gets the Type of the affected Node.
		/// </summary>
		public string Type
		{
			get {return type;}
		}

		/// <summary>
		/// Gets the ChangeType for the event.
		/// </summary>
		public EventType ChangeType
		{
			get {return changeType;}
			set {changeType = value;}
		}
	}
}
