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
 *  Author: Bruce Getter
 *
 ***********************************************************************/

using System;
using Simias.Event;

namespace Simias.Sync
{
	/// <summary>
	/// The direction of the file sync.
	/// </summary>
	[Flags]
	public enum Direction : short
	{
		/// <summary>
		/// The event is for an upload.
		/// </summary>
		Uploading = 1,
		/// <summary>
		/// The event is for a download.
		/// </summary>
		Downloading = 2
	};

	/// <summary>
	/// The arguments for a file sync event.
	/// </summary>
	[Serializable]
	public class FileSyncEventArgs : SimiasEventArgs
	{
		#region Fields

		private string name;
		private long size;
		private long sizeToSync;
		private long sizeRemaining;
		private Direction direction;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructs a FileSyncEventArgs that will be used by FileSyncHandler delegates.
		/// </summary>
		/// <param name="name">The name of the file that the event belongs to.</param>
		/// <param name="size">The size of the file that the event belongs to.</param>
		/// <param name="sizeToSync">The total amount of data to be synced.</param>
		/// <param name="sizeRemaining">The amount of data that still needs to be synced.</param>
		/// <param name="direction">The direction of the sync.</param>
		public FileSyncEventArgs(string name, long size, long sizeToSync, long sizeRemaining, Direction direction) :
			base()
		{
			this.name = name;
			this.size = size;
			this.sizeToSync = sizeToSync;
			this.sizeRemaining = sizeRemaining;
			this.direction = direction;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the name of the file that the event belongs to.
		/// </summary>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		/// Gets the size of the file that the event belongs to.
		/// </summary>
		public long Size
		{
			get { return size; }
		}

		/// <summary>
		/// Gets the total amount of data to be synced.
		/// </summary>
		public long SizeToSync
		{
			get { return sizeToSync; }
		}

		/// <summary>
		/// Gets the amount of data that still needs to be synced.
		/// </summary>
		public long SizeRemaining
		{
			get { return sizeRemaining; }
		}

		/// <summary>
		/// Gets the direction of the sync.
		/// </summary>
		public Direction Direction
		{
			get { return direction; }
		}
		#endregion
	}
}
