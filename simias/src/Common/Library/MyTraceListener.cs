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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections;

namespace Simias
{
	/// <summary>
	/// My Trace Listener
	/// </summary>
	public class MyTraceListener : TraceListener
	{
		private static readonly string NO_CATEGORY = "?";

		private TextWriter writer;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public MyTraceListener(TextWriter writer)
		{
			this.writer = writer;
		}

		/// <summary>
		/// Write a message to the trace listener.
		/// </summary>
		/// <param name="message">The trace message.</param>
		public override void Write(string message)
		{
			WriteLine(message, NO_CATEGORY);
		}

		/// <summary>
		/// Write a message to the trace listener.
		/// </summary>
		/// <param name="message">The trace message.</param>
		public override void WriteLine(string message)
		{
			WriteLine(message, NO_CATEGORY);
		}

		/// <summary>
		/// Write a message to the trace listener.
		/// </summary>
		/// <param name="message">The trace message.</param>
		/// <param name="category">The category of the message.</param>
		public override void Write(string message, string category)
		{
			WriteLine(message, category);
		}

		/// <summary>
		/// Write a message to the trace listener.
		/// </summary>
		/// <param name="message">The trace message.</param>
		/// <param name="category">The category of the message.</param>
		public override void WriteLine(string message, string category)
		{
			string[] list = Regex.Split(category, @"(\.)", RegexOptions.Compiled);

			if (list.Length > 2)
			{
				category = String.Format("{0}.{1}()", list[list.Length - 3],
					list[list.Length - 1]);
			}

			writer.WriteLine("\n{0}\n\t[{1} {2} {3}]", message, category,
				DateTime.Now.ToShortTimeString(), DateTime.Now.ToShortDateString());
		}
	}
}
