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
using System.Diagnostics;

using NUnit.Framework;

using Simias;

namespace Simias.Tests
{
	/// <summary>
	/// Common Tests
	/// </summary>
	[TestFixture]
	public class CommonTests : Assertion
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public CommonTests()
		{
		}

		/// <summary>
		/// Test case setup
		/// </summary>
		[SetUp]
		public void CaseSetup()
		{
			MyTrace.SendToConsole();
			MyTrace.Switch.Level = TraceLevel.Verbose;
		}

		/// <summary>
		/// Trace Test 1
		/// </summary>
		[Test]
		public void TestTrace1()
		{
			MyTrace.WriteLine("Test 1");
		}

		/// <summary>
		/// Trace Test 2
		/// </summary>
		[Test]
		public void TestTrace2()
		{
			MyTrace.WriteLine("Test 2 : {0}", "hello");
		}

		/// <summary>
		/// Trace Test 3
		/// </summary>
		[Test]
		public void TestTrace3()
		{
			MyTrace.WriteLine(new Exception());
		}

		/// <summary>
		/// Trace Test 1a
		/// </summary>
		[Test]
		public void TestTrace1a()
		{
			MyTrace.Switch.Level = TraceLevel.Verbose;
			MyTrace.WriteLine("Test 1");
		}

		/// <summary>
		/// Trace Test 2a
		/// </summary>
		[Test]
		public void TestTrace2a()
		{
			MyTrace.Switch.Level = TraceLevel.Verbose;
			MyTrace.WriteLine("Test 2 : {0}", "hello");
		}

		/// <summary>
		/// Trace Test 3a
		/// </summary>
		[Test]
		public void TestTrace3a()
		{
			MyTrace.Switch.Level = TraceLevel.Verbose;
			MyTrace.WriteLine(new Exception());
		}

		/// <summary>
		/// Dns Test
		/// </summary>
		[Test]
		public void TestDns()
		{
			Console.WriteLine("Host: {0}", MyDns.GetHostName());
		}

		/// <summary>
		/// Environment Test
		/// </summary>
		[Test]
		public void TestEnvironment()
		{
			Console.WriteLine("Platform: {0}", MyEnvironment.Platform);
			Console.WriteLine("Runtime: {0}", MyEnvironment.Runtime);
		}

		/// <summary>
		/// Path Test
		/// </summary>
		[Test]
		public void TestPath()
		{
			string path1 = @"/home/jdoe";
			string path2 = @"c:\home\jdoe";

			Console.WriteLine("Full Local Path: {0} ({1})", MyPath.GetFullLocalPath(path1), path1);
			Console.WriteLine("Full Local Path: {0} ({1})", MyPath.GetFullLocalPath(path2), path2);
		}
	}
}
