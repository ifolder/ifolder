/***********************************************************************
 *  GtkTraceWindowTests.cs - Gtk-Sharp Invitation Wizard Application
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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/

using System;

using Gtk;
using Gdk;
using GtkSharp;
using System.Threading;
using NUnit.Framework;

namespace Simias
{
	[TestFixture]
	public class GtkTraceWindowTests 
	{
		[TestFixtureSetUp]
		public void Init()
		{
			Application.Init();
		}

		public void WriteToWindow(string str, int waitTime)
		{
			MyTrace.WriteLine(str);
			while(Application.EventsPending())
				Application.RunIteration(false);

			if(waitTime == 0)
				return;
			Thread.Sleep(TimeSpan.FromSeconds(waitTime));
		}

		[Test]
		public void TestGtkTraceWindow()
		{
			GtkTraceWindow twin = new GtkTraceWindow();
			WriteToWindow("test1", 0);
			WriteToWindow("test2", 0);
			WriteToWindow("test3", 0);
			WriteToWindow("test4", 0);
			WriteToWindow("test5", 0);
			WriteToWindow("test6", 0);
			WriteToWindow("test7", 0);
			WriteToWindow("test8", 0);
			WriteToWindow("test9", 0);
			WriteToWindow("test10", 0);
			WriteToWindow("test11", 0);
			WriteToWindow("test12", 0);
			WriteToWindow("test13", 0);
			twin.ShowAll();

			WriteToWindow("test14", 1);
			WriteToWindow("test15", 1);
			WriteToWindow("test16", 1);
		}
	}

	public class Tests
	{
		static void Main()
		{
			GtkTraceWindowTests tests = new GtkTraceWindowTests();
			tests.Init();
			tests.TestGtkTraceWindow();
		}
	}
}
