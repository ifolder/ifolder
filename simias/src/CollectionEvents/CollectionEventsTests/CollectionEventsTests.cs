/***********************************************************************
 *  CollectionEventTests.cs - A unit test suite for events.
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
	public class EventsTests
	{
		#region Fields

		Process				monitor;
		EventSubscriber		subscriber;
		EventPublisher		publisher;
		EventArgs			args;
		ManualResetEvent	mre = new ManualResetEvent(false);

		#endregion

		#region Setup/TearDown

		/// <summary>
		/// Test Setup.
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			monitor = new Process();
			monitor.StartInfo.RedirectStandardInput = true;
			monitor.StartInfo.RedirectStandardInput = true;
			monitor.StartInfo.CreateNoWindow = true;
			monitor.StartInfo.UseShellExecute = false;
			if (MyEnvironment.Mono)
			{
				monitor.StartInfo.FileName = "mono";
				monitor.StartInfo.Arguments = "CsEventBroker.exe";
			}
			else
			{
				monitor.StartInfo.FileName = "CsEventBroker.exe";
			}
			monitor.Start();
			//Console.ReadLine();
			System.Threading.Thread.Sleep(2000);
			
			publisher = new EventPublisher();
			subscriber = new EventSubscriber();
// CRG - What is an iFolder.EventHandler... This ain't right
			subscriber.Changed += new EventHandler(ChangeHandler);
			subscriber.Created += new EventHandler(CreateHandler);
			subscriber.Deleted += new EventHandler(DeleteHandler);
			subscriber.Renamed += new EventHandler(RenameHandler);
			subscriber.ServiceControl += new ServiceEventHandler(ServiceCtlHandler);
		}

		public void PublishSubscribe()
		{
			publisher = new EventPublisher();
			subscriber = new EventSubscriber();
			subscriber.Changed += new EventHandler(ChangeHandler);
			subscriber.Created += new EventHandler(CreateHandler);
			subscriber.Deleted += new EventHandler(DeleteHandler);
			subscriber.Renamed += new EventHandler(RenameHandler);
			subscriber.ServiceControl += new ServiceEventHandler(ServiceCtlHandler);
		}

		/// <summary>
		/// Test cleanup.
		/// </summary>
		[TestFixtureTearDown]
		public void Cleanup()
		{
			subscriber.Dispose();
			monitor.Kill();
		}

		#endregion

		#region Event Handlers

		void CreateHandler(EventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("Create: {0} {1} {2}", args.Node, args.Path, args.Type);
		}

		void DeleteHandler(EventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("Delete: {0} {1} {2}", args.Node, args.Path, args.Type);
		}

		void RenameHandler(EventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("Rename: {0} {1} {2}", args.Node, args.Path, args.Type);
		}

		void ChangeHandler(EventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("Change: {0} {1} {2}", args.Node, args.Path, args.Type);
		}

		void ServiceCtlHandler(int targetProcess, ServiceEventType t)
		{
			mre.Set();
			Console.WriteLine("Service Control Event = {0}", t.ToString()); 
		}

		#endregion

		#region Tests

		/// <summary>
		/// Change event test.
		/// </summary>
		[Test]
		public void ChangedTest()
		{
			args = null;
			publisher.FireChanged(new EventArgs("nifp", "1", "0", "Node"));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Create event test.
		/// </summary>
		[Test]
		public void CreatedTest()
		{
			args = null;
			publisher.FireCreated(new EventArgs("file", "2", "0", "Node"));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Delete event test.
		/// </summary>
		[Test]
		public void DeletedTest()
		{
			args = null;
			publisher.FireDeleted(new EventArgs("nifp", "3", "0", "Node"));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Rename event test.
		/// </summary>
		[Test]
		public void RenamedTest()
		{
			args = null;
			publisher.FireRenamed(new EventArgs("file", "4", "0", "Node"));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Name filter test.
		/// </summary>
		[Test]
		public void NameFilterTest()
		{
			// Check for a hit.
			args = null;
			subscriber.NameFilter = "test.*";
			publisher.FireChanged(new EventArgs("nifp", "testNode", "0", "Node"));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			// Check for a miss.
			args = null;
			subscriber.NameFilter = "test.*";
			publisher.FireChanged(new EventArgs("file", "aestNode", "0", "Node"));
			if (recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			subscriber.NameFilter = null;
		}

		/// <summary>
		/// Type filter test.
		/// </summary>
		[Test]
		public void TypeFilterTest()
		{
			// Check for a hit.
			args = null;
			subscriber.TypeFilter = "Node";
			publisher.FireChanged(new EventArgs("nifp", "testNode", "0", "Node"));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			// Check for a miss.
			args = null;
			subscriber.TypeFilter = "Collection";
			publisher.FireChanged(new EventArgs("file", "aestNode", "0", "Node"));
			if (recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			subscriber.TypeFilter = null;
		}

		/// <summary>
		/// Disable test.
		/// </summary>
		[Test]
		public void DisableTest()
		{
			// Check disabled.
			args = null;
			subscriber.Enabled = false;
			publisher.FireChanged(new EventArgs("file", "testNode", "0", "Node"));
			if (recievedCallback)
			{
				throw new ApplicationException("Failed disable");
			}

			// Check reenabled.
			args = null;
			subscriber.Enabled = true;
			publisher.FireChanged(new EventArgs("file", "testNode", "0", "Node"));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed enable");
			}
		}

		/// <summary>
		/// Service Control Test.
		/// </summary>
		[Test]
		public void ServiceControlTest()
		{
			publisher.FireServiceControl(EventPublisher.TargetAll, ServiceEventType.Shutdown);
			publisher.FireServiceControl(EventPublisher.TargetAll, ServiceEventType.Reconfigure);
		}

		#endregion

		#region privates

		private bool recievedCallback
		{
			get
			{
				bool b = mre.WaitOne(500, false);
				mre.Reset();
				return b;
			}
		}

		static void usage()
		{
			Console.WriteLine("Usage: CollectionEventsTest.exe (mode) [event count]");
			Console.WriteLine("      where mode = P (Publish)");
			Console.WriteLine("      or    mode = S (Subscribe)");
			Console.WriteLine("      where event count = number of events to publish");
		}

		#endregion

		#region Main

		/// <summary>
		/// Main entry.
		/// </summary>
		/// <param name="args"></param>
		public static void Main(string [] args)
		{
			if (args.Length == 0)
			{
				usage();
				return;
			}

			EventsTests t = new EventsTests();
			switch (args[0])
			{
				case "P":
			
					if (args.Length > 1)
					{
						t.publisher = new EventPublisher();
						int count = Int32.Parse(args[1]);
						for (int i = 0; i < count; ++i)
						{
							t.publisher.FireCreated(new EventArgs("nifp", i.ToString(), "0", "Node"));
						}
					}
					break;

				case "PS":
					t.publisher = new EventPublisher();
					t.publisher.FireServiceControl(EventPublisher.TargetAll, ServiceEventType.Shutdown);
					break;

				case "S":
					t.PublishSubscribe();
					Console.WriteLine("Press enter to exit");
					Console.ReadLine();
					t.subscriber.Dispose();
					break;

				default:
					usage();
					break;
			}
		}
		
		#endregion
	}
}
