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
using System.Windows.Forms;
using System.Diagnostics;

namespace Simias
{
	/// <summary>
	/// My Trace Listener
	/// </summary>
	public class MyTraceListener : TraceListener
	{
		ListView list;
		bool pause = false;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public MyTraceListener(ListView list)
		{
			this.list = list;
		}

		/// <summary>
		/// Write a message to the trace listeners.
		/// </summary>
		/// <param name="message">The trace message.</param>
		public override void Write(string message)
		{
			WriteLine(message);
		}

		/// <summary>
		/// Write a message to the trace listeners.
		/// </summary>
		/// <param name="message">The trace message.</param>
		public override void WriteLine(string message)
		{
			ListViewItem item = list.Items.Add(message);
			if (!pause)
			{
				item.ListView.EnsureVisible(item.Index);
			}
		}

		public bool Pause
		{
			set
			{
				pause = value;
			}
		}
	}
}
