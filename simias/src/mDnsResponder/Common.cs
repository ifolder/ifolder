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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

//
// This source file contains all the common methods, classes etc.
// for the mDnsResponder component.
//

using System;
using System.Collections;
using System.Text;


namespace Mono.P2p.mDnsResponder
{
	public enum DnsFlags : ushort
	{
		request	= 		0x0001,
		response =		0x0002,
		recursion =		0x0004
	}
	

	/// <summary>
	/// Summary description for Defaults
	/// </summary>
	class Common
	{
		#region Class Members
		#endregion

		#region Properties
		#endregion

		#region Constructors
		#endregion

		#region Private Methods

		internal static void	BuildDomainName(byte[] buffer, int startOffset, ref int endOffset, ref string domainName)
		{
			bool	compressed = false;
			int		length;
			int		cOffset = 0;
			string	tmpString;

			domainName = "";
			length = buffer[startOffset++];
			while(length > 0)
			{
				if (length == 192)
				{
					if(compressed == true)
					{
						cOffset = buffer[cOffset++];
					}
					else
					{
						cOffset = buffer[startOffset++];
					}

					length = buffer[cOffset++];
					compressed = true;
				}

				if (compressed == true)
				{
					tmpString = Encoding.UTF8.GetString(buffer, cOffset, length);
					cOffset += length;
					length = buffer[cOffset++];
				}
				else
				{
					tmpString = Encoding.UTF8.GetString(buffer, startOffset, length);
					startOffset += length;
					length = buffer[startOffset++];
				}

				domainName += tmpString;
				if (length > 0)
				{
					domainName += ".";
				}
			}

			endOffset = startOffset;
		}

		#endregion

		#region Static Methods
		#endregion

		#region Public Methods
		#endregion
	}
}
