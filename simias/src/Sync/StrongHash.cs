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
using System.Collections;
using System.Security.Cryptography;

namespace Simias.Sync.Delta
{
	/// <summary>
	/// Class to compute a strong Hash for a block of data.
	/// </summary>
	public class StrongHash
	{
		MD5		md5 = new MD5CryptoServiceProvider();
			
		/// <summary>
		/// Computes an MD5 hash of the data block passed in.
		/// </summary>
		/// <param name="buffer">The data to hash.</param>
		/// <param name="offset">The offset in the byte array to start hashing.</param>
		/// <param name="count">The number of bytes to include in the hash.</param>
		/// <returns>The hash code.</returns>
		public byte[] ComputeHash(byte[] buffer, int offset, int count)
		{
			return md5.ComputeHash(buffer, offset, count);
		}
	}
}
