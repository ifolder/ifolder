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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;

using Simias;

namespace Simias.Sniffer
{
	/// <summary>
	/// Sniffer Message
	/// </summary>
	public class SnifferMessage
	{
		private string type;
		private string assembly;
		private string method;
		//private ArrayList args;
		private ArrayList keys;
		//private string result;
		private string uri;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="msg">A message object.</param>
		public SnifferMessage(IMessage msg)
		{
			// note: the message properties must be accessed by the enumerator
			
			if (msg != null)
			{
				keys = new ArrayList();

				foreach(object key in msg.Properties.Keys)
				{
					string keyString = key.ToString();
					keys.Add(keyString);
				
					object value = msg.Properties[key];

					// skip all null valued keys
					if (value != null)
					{
						if (keyString.Equals("__TypeName"))
						{
							string typeName = value.ToString();

							string[] data = Regex.Split(typeName, "(, )", RegexOptions.Compiled);

							if ((data != null) && (data.Length >= 3))
							{
								type = data[0];
								assembly = data[2];
							}
						}
						else if (keyString.Equals("__MethodName"))
						{
							method = value.ToString();
						}
						/* TODO: ?
						else if (keyString.Equals("__Args"))
						{
							args = new ArrayList();

							object[] oArray = (object[])value;

							foreach(object o in oArray)
							{
								if (o != null)
								{
									args.Add(o.ToString());
								}
							}
						}
						else if (keyString.Equals("__Return"))
						{
							result = value.ToString();
						}
						*/
						else if (keyString.Equals("__Uri"))
						{
							uri = value.ToString();
						}
					}
				}
			}
		}

		/// <summary>
		/// A string representation of the sniffer message.
		/// </summary>
		/// <returns>The string</returns>
		public override string ToString()
		{
			StringBuilder buffer = new StringBuilder();

			if (assembly != null) buffer.AppendFormat("Assembly: {0}\n", assembly);

			if (type != null) buffer.AppendFormat("    Type: {0}\n", type);

			if (method != null) buffer.AppendFormat("  Method: {0}\n", method);

			//if (result != null) buffer.AppendFormat("  Result: {0}\n", result);

			/*if (args != null)
			{
				for(int i=0; i < args.Count; i++)
				{
					buffer.AppendFormat("  Arg {0,2}: {1}\n", i, args[i]);
				}
			}*/

			if (uri != null) buffer.AppendFormat("     Uri: {0}\n", uri);

			return buffer.ToString();
		}

		/// <summary>
		/// A string representation of the sniffer message.
		/// </summary>
		/// <returns>The string</returns>
		public static string ToString(IMessage msg)
		{
			SnifferMessage smsg = new SnifferMessage(msg);

			return smsg.ToString();
		}
	}
}
