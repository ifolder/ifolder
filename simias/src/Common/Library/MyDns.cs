/***********************************************************************
 *  $RCSfile$
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
 *  Author: Rob
 * 
 ***********************************************************************/

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Simias
{
	/// <summary>
	/// My Dns
	/// </summary>
	public class MyDns
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		private MyDns()
		{
		}

		/// <summary>
		/// Lookup the host name of the local computer.
		/// </summary>
		/// <returns>The DNS host name of the local computer.</returns>
		public static string GetHostName()
		{
			// machine host name
			string host = Dns.GetHostName();
			IPHostEntry ipHostEntry = Dns.Resolve(host);
			host = ipHostEntry.HostName;

			MyTrace.WriteLine("Host Name: {0}", host);

			// loop through addresses
			foreach(IPAddress ipAddress in ipHostEntry.AddressList)
			{
				MyTrace.WriteLine("IP Address: {0}", ipAddress);
			
				// skip loop-back addresses
				if (IPAddress.IsLoopback(ipAddress)) continue;

				// use the address
				host = ipAddress.ToString();

				// take the first one
				break;
			}

			return host;
		}
	}
}
