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
using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Text;

using Simias;

namespace Simias.Sync
{
	/// <summary>
	/// Sniffer Message
	/// </summary>
	public class SnifferMessage
	{
		private string uri;
		private string method;
		private ArrayList args;

		public SnifferMessage(IMessage msg)
		{
			IDictionary properties = msg.Properties;

			uri = properties["__Uri"].ToString();
			method = properties["__MethodName"].ToString();

			object[] oArgs = (object[])properties["__Args"];
			args = new ArrayList();

			if (oArgs != null)
			{
				foreach(object o in oArgs)
				{
					args.Add(o.ToString());
				}
			}
		}

		public override string ToString()
		{
			StringBuilder buffer = new StringBuilder();

			buffer.AppendFormat("   Uri: {0}\n", uri);
			buffer.AppendFormat("Method: {0}\n", method);

			foreach(string arg in args)
			{
				buffer.AppendFormat("   Arg: {0}\n", arg);
			}

			return buffer.ToString();
		}

		public static string ToString(IMessage msg)
		{
			SnifferMessage smsg = new SnifferMessage(msg);

			return smsg.ToString();
		}
	}
}
