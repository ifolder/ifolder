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
 * 
 * Thanks to Andrew Tridgell for the rsync algorythm.
 * http://samba.anu.edu.au/rsync/tech_report/
 ***********************************************************************/
using System;
using System.IO;
using System.Collections;
using System.Security.Cryptography;

namespace Simias.Sync.Delta
{
	/// <summary>
	/// Class to compute a weak hash for a block of data.
	/// The hash can be calculated quickly, and can be rolled.
	/// That is recaculated as the window is slid one byte at a time through a buffer.
	/// </summary>
	public class WeakHash
	{
		UInt16 a;
		UInt16 b;

		/// <summary>
		/// Computes the weak hash for a block of data.
		/// </summary>
		/// <param name="buffer">The buffer containing the data.</param>
		/// <param name="offset">The offset in the block to start.</param>
		/// <param name="count">The number of bytes to include in the hash.</param>
		/// <returns>The weak hash code.</returns>
		public UInt32 ComputeHash(byte[] buffer, int offset, UInt16 count)
		{
			a = 0;
			b = 0;
			UInt16 l = (UInt16)(count);
			
			for (int i = offset; i < offset + count; ++i)
			{
				a += buffer[i];
				b += (UInt16)(l-- * buffer[i]);
			}

			return a + (UInt32)(b * 0x10000);
		}

		/// <summary>
		/// Recalculates the weak hash droping the dropvalue and including the addValue.
		/// </summary>
		/// <param name="count"></param>
		/// <param name="dropValue">The byte to drop from the hash.  This byte was at the 
		/// begining of the last hash.</param>
		/// <param name="addValue">The byte to add to the hash. This byte was one byte
		/// past the last hash block.</param>
		/// <returns>The weak hash code.</returns>
		public UInt32 RollHash(int count, byte dropValue, byte addValue)
		{
			a = (UInt16)(a - dropValue + addValue);
			b = (UInt16)(b - ((count) * dropValue) + a);
			return a + (UInt32)(b * 0x10000);
		}
	}
}
