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
using System.Threading;

namespace Simias
{
	/// <summary>
	/// My Trace
	/// </summary>
	public sealed class MyTrace
	{
		/// <summary>
		/// Trace Switch
		/// </summary>
		public static TraceSwitch Switch;

		private static bool stdout = false;

		private static object lockRoot = new object();

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
		/// Add a trace listener to send trace message to standard out.
		/// </summary>
		public static void SendTraceToStandardOutput()
		{
			if (stdout == false)
			{
				lock(lockRoot)
				{
					if (stdout == false)
					{
						stdout = true;
						Trace.Listeners.Add(new TextWriterTraceListener(
							Console.Out));
					}
				}
			}
		}

		/// <summary>
		/// Writes a message to the trace listeners.
		/// </summary>
		/// <param name="e">The exception for the message.</param>
		public static void WriteLine(Exception e)
		{
			string message;

			if (Switch.Level == TraceLevel.Verbose)
			{
				message = e.ToString();
			}
			else
			{
				message = e.Message;
			}

			WriteLine(1, message);
		}
		
		/// <summary>
		/// Writes a message to the trace listeners.
		/// </summary>
		/// <param name="format">The formatting string.</param>
		/// <param name="arg">An array of object for the formatting string.</param>
		public static void WriteLine(string format, params object[] arg)
		{
			WriteLine(1, format, arg);
		}

		/// <summary>
		/// Writes a message to the trace listeners.
		/// </summary>
		/// <param name="index">A stack frame index.</param>
		/// <param name="format">The formatting string.</param>
		/// <param name="arg">An array of object for the formatting string.</param>
		private static void WriteLine(int index, string format, params object[] arg)
		{
			string message = String.Format(format, arg);
			string category = "?";

			StackTrace trace = new StackTrace(index + 1, true);

			if (trace.FrameCount > 0)
			{
				StackFrame frame = trace.GetFrame(0);
				
				category = frame.GetMethod().DeclaringType.Name;
				string path = frame.GetFileName();

				// if verbose mode and we have debugging information - show it
				if ((Switch.Level == TraceLevel.Verbose) && (path != null) && (path.Length > 0))
				{
					message = String.Format("[{0}(), {1}:{2}] {3}",
						frame.GetMethod().Name,
						Path.GetFileName(frame.GetFileName()),
						frame.GetFileLineNumber(), message);
				}
				else
				{
					message = String.Format("[{0}()] {1}",
						frame.GetMethod().Name, message);
				}
			}

			if (Switch.Level == TraceLevel.Verbose)
			{
				Trace.WriteLine(DateTime.Now);
			}
			
			Trace.WriteLine(message, category);
		}
	}
}
