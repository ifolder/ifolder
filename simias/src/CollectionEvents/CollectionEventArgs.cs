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
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;
using System.Text;

namespace Simias.Event
{
	/// <summary>
	/// The Event types supported.
	/// </summary>
	[Flags]
	public enum EventType : short
	{
		/// <summary>
		/// The event is for a node create.
		/// </summary>
		NodeCreated = 1,
		/// <summary>
		/// The event is for a node delete.
		/// </summary>
		NodeDeleted = 2,
		/// <summary>
		/// The event is for a node change.
		/// </summary>
		NodeChanged = 4,
		/// <summary>
		/// The event is for a collection root path change.
		/// </summary>
		CollectionRootChanged = 8,
		/// <summary>
		/// The event is for a file create.
		/// </summary>
		FileCreated = 16,
		/// <summary>
		/// The event is for a file delete.
		/// </summary>
		FileDeleted = 32,
		/// <summary>
		/// The event is for a file change.
		/// </summary>
		FileChanged = 64,
		/// <summary>
		/// The event is for a file rename.
		/// </summary>
		FileRenamed = 128,
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

		internal CollectionEventArgs()
		{
		}
	
		internal virtual string MarshallToString()
		{
			return (changeType.ToString() + seperatorChar);
		}

		internal virtual void MarshallFromString(string [] args, ref int index)
		{
			//int i = 0;
			//string [] sArg = sArgs.Split(seperatorChar);
			changeType = (EventType)Enum.Parse(typeof(EventType), args[index++], false);
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
