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
using System.Diagnostics;
using System.Threading;

using log4net;

namespace Simias
{
	/// <summary>
	/// My Trace
	/// </summary>
	[Obsolete("Use log4net instead.")]
	public sealed class MyTrace
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(MyTrace));

		/// <summary>
		/// Trace Switch
		/// </summary>
		public static TraceSwitch Switch;

		/// <summary>
		/// Static Constructor
		/// </summary>
		static MyTrace()
		{
			Trace.AutoFlush = true;
			Debug.AutoFlush = true;

			Switch = new TraceSwitch("Denali", "Entire Application");
			Switch.Level = TraceLevel.Info;
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		private MyTrace()
		{
		}

		/// <summary>
		/// Add a trace listener to send trace message to the console.
		/// </summary>
		public static void SendToConsole()
		{
		}

		/// <summary>
		/// Writes a message to the trace listeners.
		/// </summary>
		/// <param name="e">The exception for the message.</param>
		public static void WriteLine(Exception e)
		{
			log.Debug(e, "");
			log.Debug(e.StackTrace);
		}
		
		/// <summary>
		/// Writes a message to the trace listeners.
		/// </summary>
		/// <param name="format">The formatting string.</param>
		/// <param name="arg">An array of object for the formatting string.</param>
		public static void WriteLine(string format, params object[] arg)
		{
			log.Debug(format, arg);
		}
	}
}
