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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

// This is a temporary class we are using because the serialization of
// GUIDs in mono is broken between architectures

using System;
using System.IO;


namespace Simias
{
	/// <summary>
	/// A temporary class to serialize Guids
	/// </summary>
	public class SimGuid
	{
		/// <summary>
		/// Swaps the first 4 bytes, next 2, and next 2 bytes to go from
		/// being big or little endian to being not big or little endian
		/// </summary>
		/// <param name="bytes">The guid bytes to swap.</param>
		/// <returns>A new swapped byte array.</returns>
		private static byte[] SwapGuidBytes(byte[] bytes)
		{
			byte[] newBytes = new byte[16];

			for(int i=0; i<=3; i++)
			{
				newBytes[i] = bytes[3-i];
			}
			for(int i=0; i<=1; i++)
			{
				newBytes[4+i] = bytes[5-i];
			}
			for(int i=0; i<=1; i++)
			{
				newBytes[6+i] = bytes[7-i];
			}
			for(int i=8; i<=15; i++)
			{
				newBytes[i] = bytes[i];
			}
			return newBytes;
		}




		/// <summary>
		/// Takes a Guid structure and converts it to a byte array.
		/// The array will always hold little endian values
		/// </summary>
		/// <param name="convGuid">The guid to convert.</param>
		/// <returns>A byte array storing little endian values.</returns>
		public static byte[] ToByteArray(Guid convGuid)
		{
			byte[] bytes = convGuid.ToByteArray();

			if(!System.BitConverter.IsLittleEndian)
			{
				bytes = SwapGuidBytes(bytes);
			}
			return bytes;
		}




		/// <summary>
		/// Takes bytes in little endian and converts them into a guid
		/// </summary>
		/// <param name="guidBytes">Guid bytes in little endian.</param>
		/// <returns>Resulting Guid built from guidBytes.</returns>
		public static Guid FromByteArray(byte[] guidBytes)
		{
			byte[] newBytes;

			if(!System.BitConverter.IsLittleEndian)
				newBytes = SwapGuidBytes(guidBytes);
			else
				newBytes = guidBytes;

			return (new Guid(newBytes));
		}
	}
}
