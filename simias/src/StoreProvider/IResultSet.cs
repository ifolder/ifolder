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

namespace Simias.Storage.Provider
{
	public enum IndexOrigin
	{
		CUR = 0,
		END,
		SET
	}

	/// <summary>
	/// Result set interface.
	/// </summary>
	public interface IResultSet : IDisposable
	{
		/// <summary>
		/// Method to return the next set of objects.
		/// All the objects that can fit in the buffer will be returned.
		/// returns false when no more objects exist.
		/// </summary>
		/// <param name="buffer">Buffer used to return the objects.</param>
		/// <returns>true - objects returned. false - no more objects</returns>
		int GetNext(ref char[] buffer);

		/// <summary>
		/// Set the Index the specified offset from the origin.
		/// </summary>
		/// <param name="origin">The origin to move from</param>
		/// <param name="offset">The offset to move the index by.</param>
		/// <returns>True if successful.</returns>
		bool SetIndex(IndexOrigin origin, int offset);

		/// <summary>
		/// Gets the number of entries in the result set.
		/// </summary>
		int Count
		{
			get;
		}
	}
}
