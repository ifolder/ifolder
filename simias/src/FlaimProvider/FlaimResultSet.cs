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
using Simias.Storage.Provider;
using System.Runtime.InteropServices;

namespace Simias.Storage.Provider.Flaim
{
	/// <summary>
	/// Summary description for FlaimObjectIterator.
	/// </summary>
	public class FlaimResultSet : MarshalByRefObject, IResultSet
	{
		private bool AlreadyDisposed = false;
		int			count;
		IntPtr		pResults;

		[DllImport("FlaimWrapper")]
		private static extern int FWCloseSearch(IntPtr ipResults);

		[DllImport("FlaimWrapper", CharSet=CharSet.Unicode)]
		private static extern int FWGetNextObjectList(IntPtr pResults, [In, Out] char[] buffer, int nChars);

		[DllImport("FlaimWrapper", CharSet=CharSet.Unicode)]
		private static extern bool FWSetListIndex(IntPtr pResults, IndexOrigin origin, int offset);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pResultSet"></param>
		/// <param name="count"></param>
		public FlaimResultSet(IntPtr pResultSet, int count)
		{
			this.count = count;
			this.pResults = pResultSet;
		}

		/// <summary>
		/// 
		/// </summary>
		~FlaimResultSet()
		{
			Dispose(true);
		}

		#region IObjectIterator Members

		/// <summary>
		/// Method to return the next set of objects.
		/// All the objects that can fit in the buffer will be returned.
		/// returns false when no more objects exist.
		/// </summary>
		/// <param name="buffer">Buffer used to return the objects.</param>
		/// <returns>true - objects returned. false - no more objects</returns>
		public int GetNext(ref char[] buffer)
		{
			// TODO:  Add FlaimObjectIterator.GetNext implementation
			if (AlreadyDisposed || pResults == IntPtr.Zero)
			{
				return 0;
			}
			else
			{
				int length = FWGetNextObjectList(pResults, buffer, buffer.Length);
				if (length == 0)
				{
					Dispose();
				}
				return length;
			}
		}

		/// <summary>
		/// Set the Index to the specified offset from the origin.
		/// </summary>
		/// <param name="origin">The origin to move from</param>
		/// <param name="offset">The offset to move the index by.</param>
		/// <returns>True if successful.</returns>
		public bool SetIndex(IndexOrigin origin, int offset)
		{
			return FWSetListIndex(pResults, origin, offset);
		}

		/// <summary>
		/// Property to get the count of available objects.
		/// </summary>
		public int Count
		{
			get
			{
				if (AlreadyDisposed || pResults == IntPtr.Zero)
				{
					return 0;
				}
				else
				{
					return count;
				}
			}
		}



		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inFinalize"></param>
		private void Dispose(bool inFinalize)
		{
			if (!AlreadyDisposed)
			{
				AlreadyDisposed = true;
				FWCloseSearch(pResults);
			
				if (!inFinalize)
				{
					GC.SuppressFinalize(this);
				}
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Method to cleanup any Flaim resources held.
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
		}

		#endregion
	}
}
