/***********************************************************************
 *  ServiceTests.cs - A unit test suite for Service Manager.
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
 *  Author: Russ Young <ryoung@novell.com>
 * 
 ***********************************************************************/
using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using NUnit.Framework;
using Simias;

namespace Simias
{
	/// <summary>
	/// Test Fixture for the Collection Events.
	/// </summary>
	[TestFixture]
	public class ServiceTests
	{
		#region Fields
		SystemManager systemManager;
		Configuration conf = new Configuration(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath));
		#endregion

		#region Setup/TearDown

		/// <summary>
		/// Test Setup.
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			systemManager = new SystemManager(conf);
		}

		/// <summary>
		/// Test cleanup.
		/// </summary>
		[TestFixtureTearDown]
		public void Cleanup()
		{
			try
			{
				if (systemManager != null)
					systemManager.StopServices();
			}
			catch {}
		}

		#endregion

		#region Tests

		/// <summary>
		/// Change event test.
		/// </summary>
		[Test]
		public void StartServices()
		{
			systemManager.StartServices();
			Thread.Sleep(1000);
		}

		/// <summary>
		/// Create event test.
		/// </summary>
		[Test]
		public void StopServices()
		{
			systemManager.StopServices();
		}

		#endregion

		#region Main

		/// <summary>
		/// Main entry.
		/// </summary>
		/// <param name="args"></param>
		public static void Main(string [] args)
		{
			ServiceTests test = new ServiceTests();
			test.Init();
			try
			{
				test.StartServices();
				Console.WriteLine("Press Enter to exit");
				Console.ReadLine();
				test.StopServices();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}
		
		#endregion
	}
}
