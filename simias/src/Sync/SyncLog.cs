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
 *  Author: Dale Olds <olds@novell.com>
 * 
 ***********************************************************************/
using System;
using System.IO;
using System.Collections;
using System.Diagnostics;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
/// <summary> catch-all log for misc sync classes</summary>
public class Log
{
	/// <summary>
	/// The logging object.
	/// </summary>
	public static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(Log));

	// don't always get line numbers from stack dumps, so force it here
	static void DumpStack()
	{
		StackTrace st = new StackTrace(2, true);
		for (int i = 0; i < st.FrameCount; ++i)
		{
			StackFrame sf = st.GetFrame(i);
			log.Debug(String.Format("  called from {0}, {1}: {2}",
					sf.GetFileName(), sf.GetFileLineNumber(),
					sf.GetMethod().ToString()));
		}
	}

	/// <summary>
	/// Prints out the stack frame.
	/// </summary>
	public static void Here()
	{
		StackFrame sf = new StackTrace(1, true).GetFrame(0);
		log.Debug("Here: {0}:{1} {2}", sf.GetFileName(), sf.GetFileLineNumber(), sf.GetMethod().ToString());
	}

	/// <summary>
	/// Prints the exception.
	/// </summary>
	/// <param name="e"></param>
	public static void Uncaught(Exception e)
	{
		log.Debug(String.Format("Uncaught exception: {0}\n{1}", e.Message, e.StackTrace));
	}

	/// <summary>
	/// Logs an assertion.
	/// </summary>
	/// <param name="assertion"></param>
	public static void Assert(bool assertion)
	{
		if (!assertion)
		{
			log.Error("Assertion failed ------------");
			DumpStack();
		}
	}

}

//===========================================================================
}
