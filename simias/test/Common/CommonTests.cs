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

using NUnit.Framework;

using Simias.Client;

namespace Simias.Tests
{
	/// <summary>
	/// Common Tests
	/// </summary>
	[TestFixture]
	public class CommonTests : Assertion
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(CommonTests));

		/// <summary>
		/// Default Constructor
		/// </summary>
		public CommonTests()
		{
		}

		/// <summary>
		/// Test fixture setup
		/// </summary>
		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			string path = Path.GetFullPath("./common1");
			Directory.CreateDirectory(path);
			SimiasLogManager.Configure(path);
		}

		/// <summary>
		/// Log Test 1
		/// </summary>
		[Test]
		public void TestLog1()
		{
			log.Debug("Test 1");
		}

		/// <summary>
		/// Log Test 2
		/// </summary>
		[Test]
		public void TestTrace2()
		{
			log.Debug("Test 2 : {0}", "hello");
		}

		/// <summary>
		/// Log Test 3
		/// </summary>
		[Test]
		public void TestTrace3()
		{
			log.Debug(new Exception(), "Test 3");
		}

		/// <summary>
		/// Dns Test
		/// </summary>
		[Test]
		public void TestDns()
		{
			log.Debug("Host: {0}", MyDns.GetHostName());
		}

		/// <summary>
		/// Environment Test
		/// </summary>
		[Test]
		public void TestEnvironment()
		{
			log.Debug("Platform: {0}", MyEnvironment.Platform);
			log.Debug("Runtime: {0}", MyEnvironment.Runtime);
		}

		/// <summary>
		/// Path Test
		/// </summary>
		[Test]
		public void TestPath()
		{
			string path1 = @"/home/jdoe";
			string path2 = @"c:\home\jdoe";

			log.Debug("Full Local Path: {0} ({1})", MyPath.GetFullLocalPath(path1), path1);
			log.Debug("Full Local Path: {0} ({1})", MyPath.GetFullLocalPath(path2), path2);
		}
	}
}
