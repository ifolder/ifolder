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
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections;

namespace Simias
{
	/// <summary>
	/// My Trace Listener
	/// </summary>
	public class MyTraceFormListener : TraceListener
	{
		private ListView list;
		private bool scrollLock = false;
		private decimal sizeLimit = 100;

		public delegate void WriteDelegate(string message);

		/// <summary>
		/// Default Constructor
		/// </summary>
		public MyTraceFormListener(ListView list)
		{
			this.list = list;
		}

		/// <summary>
		/// Write a message to the trace listener.
		/// </summary>
		/// <param name="message">The trace message.</param>
		public override void Write(string message)
		{
			WriteLine(message);
		}

		/// <summary>
		/// Write a message to the trace listener.
		/// </summary>
		/// <param name="message">The trace message.</param>
		public override void WriteLine(string message)
		{
			list.Invoke(new WriteDelegate(WriteCallBack), new string[] { message });
		}

		public void WriteCallBack(string message)
		{
			list.Items.Add(message.Trim());

			if ((sizeLimit != -1) && (list.Items.Count > sizeLimit))
			{
				list.BeginUpdate();

				while (list.Items.Count > sizeLimit)
				{
					list.Items.RemoveAt(0);
				}

				list.EndUpdate();
			}
			
		}

		/// <summary>
		/// The state of the scroll lock.
		/// </summary>
		public bool ScrollLock
		{
			set { scrollLock = value; }
		}

		/// <summary>
		/// The number of entries the list view will hold.
		/// </summary>
		public decimal SizeLimit
		{
			get { return sizeLimit; }
			set { sizeLimit = value; }
		}
	}
}
