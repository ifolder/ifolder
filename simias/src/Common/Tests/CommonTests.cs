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
			MyTrace.SendTraceToStandardOutput();
			MyTrace.Switch.Level = TraceLevel.Info;
		}

		/// <summary>
		/// My Trace Test 1
		/// </summary>
		[Test]
		public void TestMyTrace1()
		{
			MyTrace.WriteLine("Test 1");
		}

		/// <summary>
		/// My Trace Test 2
		/// </summary>
		[Test]
		public void TestMyTrace2()
		{
			MyTrace.WriteLine("Test 2 : {0}", "hello");
		}

		/// <summary>
		/// My Trace Test 3
		/// </summary>
		[Test]
		public void TestMyTrace3()
		{
			MyTrace.WriteLine(new Exception());
		}

		/// <summary>
		/// My Trace Test 1a
		/// </summary>
		[Test]
		public void TestMyTrace1a()
		{
			MyTrace.Switch.Level = TraceLevel.Verbose;
			MyTrace.WriteLine("Test 1");
		}

		/// <summary>
		/// My Trace Test 2a
		/// </summary>
		[Test]
		public void TestMyTrace2a()
		{
			MyTrace.Switch.Level = TraceLevel.Verbose;
			MyTrace.WriteLine("Test 2 : {0}", "hello");
		}

		/// <summary>
		/// My Trace Test 3a
		/// </summary>
		[Test]
		public void TestMyTrace3a()
		{
			MyTrace.Switch.Level = TraceLevel.Verbose;
			MyTrace.WriteLine(new Exception());
		}

		/// <summary>
		/// My Dns Test
		/// </summary>
		[Test]
		public void TestMyDns()
		{
			Console.WriteLine("My Host: {0}", MyDns.GetHostName());
		}

		/// <summary>
		/// My Dns Test
		/// </summary>
		[Test]
		public void TestMyDnsExternal()
		{
			Console.WriteLine("My External Host: {0}", MyDns.GetExternalHostName());
		}

		/// <summary>
		/// My Environment Test
		/// </summary>
		[Test]
		public void TestMyEnvironment()
		{
			Console.WriteLine("My Platform: {0}", MyEnvironment.Platform);
			Console.WriteLine("My Runtime: {0}", MyEnvironment.Runtime);
		}

		/// <summary>
		/// My Path Test
		/// </summary>
		[Test]
		public void TestMyPath()
		{
			string path1 = @"/home/jdoe";
			string path2 = @"c:\home\jdoe";

			Console.WriteLine("My Full Local Path: {0} ({1})", MyPath.GetFullLocalPath(path1), path1);
			Console.WriteLine("My Full Local Path: {0} ({1})", MyPath.GetFullLocalPath(path2), path2);
		}
	}
}
