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

namespace Simias.Client.Event
{
	/// <summary>
	/// Actions that indicate what to do with the simias events.
	/// </summary>
	public enum IProcEventAction
	{
		/// <summary>
		/// Add Node object created event.
		/// </summary>
		AddNodeCreated,

		/// <summary>
		/// Add Node object changed event.
		/// </summary>
		AddNodeChanged,

		/// <summary>
		/// AddNode object deleted event.
		/// </summary>
		AddNodeDeleted,

		/// <summary>
		/// Add Collection synchronization events.
		/// </summary>
		AddCollectionSync,

		/// <summary>
		/// Add File synchronization events.
		/// </summary>
		AddFileSync,

		/// <summary>
		/// Add Notify message events.
		/// </summary>
		AddNotifyMessage,

		/// <summary>
		/// Remove Node object created event.
		/// </summary>
		RemoveNodeCreated,

		/// <summary>
		/// Remove Node object changed event.
		/// </summary>
		RemoveNodeChanged,

		/// <summary>
		/// Remove Node object deleted event.
		/// </summary>
		RemoveNodeDeleted,

		/// <summary>
		/// Remove Collection synchronization event.
		/// </summary>
		RemoveCollectionSync,

		/// <summary>
		/// Remove File synchronization event.
		/// </summary>
		RemoveFileSync,

		/// <summary>
		/// Remove Notify message event.
		/// </summary>
		RemoveNotifyMessage
};

	/// <summary>
	/// Used to specify to indicate only certain types of events. These filters
	/// only apply to Simias "node" events. Sync events cannot be filtered.
	/// </summary>
	public enum IProcFilterType
	{
		/// <summary>
		/// Subscribe to all changes in the specified collection.
		/// </summary>
		Collection,

		/// <summary>
		/// Subscribe to all changes to the specified node.
		/// </summary>
		NodeID,

		/// <summary>
		/// Subscribe to all changes to nodes of the specified type.
		/// </summary>
		NodeType
	};

	/// <summary>
	/// Describes the event filters for Simias nodes.
	/// </summary>
	public class IProcEventFilter
	{
		#region Class Members
		/// <summary>
		/// Type of event filter.
		/// </summary>
		private IProcFilterType type;

		/// <summary>
		/// Data that is used to filter the events.
		/// </summary>
		private string data;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the filter type.
		/// </summary>
		public IProcFilterType Type
		{
			get { return type; }
		}

		/// <summary>
		/// Gets the event data.
		/// </summary>
		public string Data
		{
			get { return data; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="type">Type of filter.</param>
		/// <param name="data">Filter data.</param>
		public IProcEventFilter( string type, string data )
		{
			this.type = ( IProcFilterType )Enum.Parse( typeof( IProcFilterType), type, true );
			this.data = data;
		}

		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="type">Type of filter</param>
		/// <param name="data">Filter data.</param>
		public IProcEventFilter( IProcFilterType type, string data )
		{
			this.type = type;
			this.data = data;
		}
		#endregion
	}
}
