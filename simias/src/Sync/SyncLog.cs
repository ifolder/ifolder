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
/// <summary> controlled tracing and debugging output by class categories </summary>
//[Obsolete("Use log4net instead.")]
public class Log
{
	static TraceSwitch traceSwitch = new TraceSwitch("SyncTrace", "sync trace switch");
	static ArrayList categories = null;

	static bool Categorical
	{
		get
		{
			if (categories == null)
				return true;

			StackFrame sf = (new StackTrace(2, true)).GetFrame(0);
			Type klass = sf.GetMethod().ReflectedType;

			//Trace.WriteLine(String.Format("  called from {0}.{1}",
			//	sf.GetMethod().ReflectedType.Name,
			//	sf.GetMethod().ToString()));

			foreach (string s in categories)
			{
				if (s == klass.Name)
					return true;
			}
			return false;
		}
	}

	internal static void Here()
	{
		if (traceSwitch.TraceVerbose)
		{
			StackFrame sf = new StackTrace(1, true).GetFrame(0);
			Trace.WriteLine(String.Format("Here: {0}:{1} {2}",
					sf.GetFileName(), sf.GetFileLineNumber(), sf.GetMethod().ToString()));
		}
	}

	internal static void Uncaught(Exception e)
	{
		Trace.WriteLine("Uncaught exception: " + e.Message);
		Trace.WriteLine(e.StackTrace);
	}

	static void DumpStack()
	{
		StackTrace st = new StackTrace(2, true);
		for (int i = 0; i < st.FrameCount; ++i)
		{
			StackFrame sf = st.GetFrame(i);
			Trace.WriteLine(String.Format("  called from {0}, {1}: {2}",
					sf.GetFileName(), sf.GetFileLineNumber(),
					sf.GetMethod().ToString()));
		}
	}

	internal static void Info(string format, params object[] args)
	{
		if (Categorical && traceSwitch.TraceInfo)
		{
			Trace.WriteLine(String.Format(format, args));
		}
	}

	internal static void Warn(string format, params object[] args)
	{
		if (Categorical && traceSwitch.TraceWarning)
		{
			Trace.WriteLine(String.Format(format, args));
			DumpStack();
		}
	}

	/// <summary> controlled tracing and debugging output by class categories </summary>
	public static void Error(string format, params object[] args)
	{
		if (Categorical && traceSwitch.TraceError)
		{
			Trace.WriteLine(String.Format(format, args));
			DumpStack();
		}
	}

	internal static void Assert(bool assertion)
	{
		if (!assertion)
		{
			Trace.WriteLine("Assertion failed ------------");
			DumpStack();
		}
	}

	/// <summary> controlled tracing and debugging output by class categories </summary>
	public static void Spew(string format, params object[] args)
	{
		if (Categorical && traceSwitch.TraceVerbose)
		{
			Trace.WriteLine(String.Format(format, args));
		}
	}

	/// <summary> controlled tracing and debugging output by class categories </summary>
	public static bool SetLevel(string level)
	{
		switch (level)
		{
			case "off": traceSwitch.Level = TraceLevel.Off; break;
			case "error": traceSwitch.Level = TraceLevel.Error; break;
			case "warning": traceSwitch.Level = TraceLevel.Warning; break;
			case "info": traceSwitch.Level = TraceLevel.Info; break;
			case "verbose": traceSwitch.Level = TraceLevel.Verbose; break;
			default: return false;
		}
		return true;
	}

	/// <summary> controlled tracing and debugging output by class categories </summary>
	public static void SetCategory(string category)
	{
		if (category == "all")
			categories = null;
		else
		{
			if (categories == null)
				categories = new ArrayList();
			categories.Add(category);
		}
	}
}

//===========================================================================
}
