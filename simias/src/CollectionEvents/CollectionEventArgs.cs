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
using System.Text;

namespace Simias.Event
{
	/// <summary>
	/// The Event types supported.
	/// </summary>
	public enum EventType : short
	{
		/// <summary>
		/// The event is for a node create.
		/// </summary>
		NodeCreated = 1,
		/// <summary>
		/// The event is for a node delete.
		/// </summary>
		NodeDeleted,
		/// <summary>
		/// The event is for a node change.
		/// </summary>
		NodeChanged,
		/// <summary>
		/// The event is for a collection root path change.
		/// </summary>
		CollectionRootChanged,
		/// <summary>
		/// The event is for a file create.
		/// </summary>
		FileCreated,
		/// <summary>
		/// The event is for a file delete.
		/// </summary>
		FileDeleted,
		/// <summary>
		/// The event is for a file change.
		/// </summary>
		FileChanged,
		/// <summary>
		/// The event is for a file rename.
		/// </summary>
		FileRenamed,
		/// <summary>
		/// A service event.
		/// </summary>
		ServiceEvent
	};

	/// <summary>
	/// The event arguments for a Collection event.
	/// </summary>
	[Serializable]
	public abstract class CollectionEventArgs : EventArgs
	{
		EventType				changeType;
		internal char			seperatorChar = '\0';

		/// <summary>
		/// Constructs a CollectionEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		/// <param name="changeType">The type of change that occured.</param>
		internal CollectionEventArgs(EventType changeType)
		{
			this.changeType = changeType;
		}

		internal virtual string MarshallToString()
		{
        	return (changeType.ToString() + seperatorChar);
		}

		internal virtual void MarshallFromString(string sArgs)
		{
			int i = 0;
			string [] sArg = sArgs.Split(seperatorChar);
			changeType = (EventType)Enum.Parse(typeof(EventType), sArg[i++], false);
		}

		#region Properties
		
		/// <summary>
		/// Gets the ChangeType for the event.
		/// </summary>
		public EventType ChangeType
		{
			get {return changeType;}
			set {changeType = value;}
		}

		#endregion
	}
}
