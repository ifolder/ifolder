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
using System.IO;
using Simias.Event;

namespace Simias.Storage
{
	/// <summary>
	/// The event arguments for file events.
	/// </summary>
	[Serializable]
	public class FileEventArgs : NodeEventArgs
	{
		/// <summary>
		/// Constructs a SimiasEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="fullPath">The full path of the modified file.</param>
		/// <param name="collectionId">The collection that this file belongs to.</param>
		/// <param name="changeType">The FileChangeType for this event.</param>
		/// <param name="time">The time of the event.</param>
		public FileEventArgs(string source, string fullPath, string collectionId, EventType changeType, DateTime time):
			base(source, fullPath, collectionId, Path.GetExtension(fullPath), changeType, 0, time)
		{
		}

		/// <summary>
		/// Constructs a SimiasEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="fullPath">The full path of the modified file.</param>
		/// <param name="collectionId">The collection that this file belongs to.</param>
		/// <param name="changeType">The FileChangeType for this event.</param>
		public FileEventArgs(string source, string fullPath, string collectionId, EventType changeType):
			this(source, fullPath, collectionId, changeType, DateTime.Now)
		{
		}

		/// <summary>
		/// Gets the full path of the file.
		/// </summary>
		public string FullPath
		{
			get {return ID;}
		}

		/// <summary>
		/// Gets the leaf name with extension.
		/// </summary>
		public string Name
		{
			get {return Path.GetFileName(ID);}
		}
	}
}
