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
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using NUnit.Framework;
using Simias;

namespace Simias.Service
{
	/// <summary>
	/// Test Fixture for the Collection Events.
	/// </summary>
	[TestFixture]
	public class ServiceTests
	{
		#region Fields
		Simias.Service.Manager serviceManager;
        Configuration conf = Configuration.CreateDefaultConfig(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath));
		const string threadServiceName = "My Thread Service";
		const string processServiceName = "My Process Service";
		#endregion

		#region Setup/TearDown

		/// <summary>
		/// Test Setup.
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			serviceManager = Simias.Service.Manager.GetManager();
			serviceManager.Install(new ThreadServiceCtl(conf, threadServiceName, "ThreadServiceTest.dll", "Simias.Service.ThreadServiceTest"));
			serviceManager.Install(new ProcessServiceCtl(conf, processServiceName, "ProcessServiceTest.exe"));
		}

		/// <summary>
		/// Test cleanup.
		/// </summary>
		[TestFixtureTearDown]
		public void Cleanup()
		{
			try
			{
				if (serviceManager != null)
				{
					serviceManager.StopServices();
					serviceManager.WaitForServicesStopped();
					serviceManager.Uninstall(threadServiceName);
					serviceManager.Uninstall(processServiceName);
					serviceManager = null;
				}
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
			serviceManager.StartServices();
			serviceManager.WaitForServicesStarted();
		}

		/// <summary>
		/// Create event test.
		/// </summary>
		[Test]
		public void StopServices()
		{
			serviceManager.StopServices();
			serviceManager.WaitForServicesStopped();
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
				test.Cleanup();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}
		
		#endregion
	}
}
