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

namespace Simias.Event
{
	/// <summary>
	/// Test Fixture for the Collection Events.
	/// </summary>
	[TestFixture]
	public class EventsTests
	{
		#region Fields

		Process				monitor = null;
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
			Process [] process = Process.GetProcessesByName("CsEventBroker.exe");
			if (process.Length == 0)
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
			}
				PublishSubscribe();
		}

		public void PublishSubscribe()
		{
			publisher = new EventPublisher();
			subscriber = new EventSubscriber();
			subscriber.NodeChanged += new NodeEventHandler(OnNodeChange);
			subscriber.NodeCreated += new NodeEventHandler(OnNodeCreate);
			subscriber.NodeDeleted += new NodeEventHandler(OnNodeDelete);
			subscriber.CollectionRootChanged += new CollectionEventHandler(OnCollectionRootChanged);
			subscriber.FileChanged += new FileEventHandler(OnFileChange);
			subscriber.FileCreated += new FileEventHandler(OnFileCreate);
			subscriber.FileDeleted += new FileEventHandler(OnFileDelete);
			subscriber.FileRenamed += new FileRenameEventHandler(OnFileRenamed);
			subscriber.ServiceControl += new ServiceEventHandler(ServiceCtlHandler);
		}

		/// <summary>
		/// Test cleanup.
		/// </summary>
		[TestFixtureTearDown]
		public void Cleanup()
		{
			subscriber.Dispose();
			if (monitor != null)
				monitor.Kill();
		}

		#endregion

		#region Event Handlers

		void OnNodeChange(NodeEventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("Change: {0} {1} {2}", args.Node, args.Collection, args.Type);
		}

		void OnNodeCreate(NodeEventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("Create: {0} {1} {2}", args.Node, args.Collection, args.Type);
		}

		void OnNodeDelete(NodeEventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("Delete: {0} {1} {2}", args.Node, args.Collection, args.Type);
		}

		void OnCollectionRootChanged(CollectionRootChangedEventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("Collection Root Changed: from {0} to {1}", args.OldRoot, args.NewRoot);
		}

		void OnFileChange(FileEventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("File Change: {0} {1} {2}", args.FullPath, args.Collection, args.Type);
		}

		void OnFileCreate(FileEventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("File Create: {0} {1} {2}", args.FullPath, args.Collection, args.Type);
		}

		void OnFileDelete(FileEventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("File Delete: {0} {1} {2}", args.FullPath, args.Collection, args.Type);
		}

		void OnFileRenamed(FileRenameEventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("File Rename: {0} {1} {2}", args.OldName, args.FullPath, args.Collection);
		}

		void ServiceCtlHandler(ServiceEventArgs args)
		{
			mre.Set();
			Console.WriteLine("Service Control Event = {0}", args.EventType); 
		}

		#endregion

		#region Tests

		/// <summary>
		/// Change event test.
		/// </summary>
		[Test]
		public void NodeChangeTest()
		{
			args = null;
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", "1", "0", "Node", NodeEventArgs.EventType.Changed));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Create event test.
		/// </summary>
		[Test]
		public void NodeCreateTest()
		{
			args = null;
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", "2", "0", "Node", NodeEventArgs.EventType.Created));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Delete event test.
		/// </summary>
		[Test]
		public void NodeDeleteTest()
		{
			args = null;
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", "3", "0", "Node", NodeEventArgs.EventType.Deleted));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Rename event test.
		/// </summary>
		[Test]
		public void CollectionRootChangedTest()
		{
			args = null;
			publisher.RaiseCollectionRootChangedEvent(new CollectionRootChangedEventArgs("CollectionEventsTests", "0", "collection", @"c:\path\oldroot", @"c:\path\newRoot"));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Change event test.
		/// </summary>
		[Test]
		public void FileChangeTest()
		{
			args = null;
			publisher.RaiseFileEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\file.txt", "0", FileEventArgs.EventType.Changed));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Create event test.
		/// </summary>
		[Test]
		public void FileCreateTest()
		{
			args = null;
			publisher.RaiseFileEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\file.txt", "0", FileEventArgs.EventType.Created));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Delete event test.
		/// </summary>
		[Test]
		public void FileDeleteTest()
		{
			args = null;
			publisher.RaiseFileEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\file.txt", "0", FileEventArgs.EventType.Deleted));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Rename event test.
		/// </summary>
		[Test]
		public void FileRenamedTest()
		{
			args = null;
			publisher.RaiseFileEvent(new FileRenameEventArgs("CollectionEventsTests", @"c:\path\newfile.txt", "0", @"c:\path\oldname.txt"));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}


		/// <summary>
		/// Name filter test.
		/// </summary>
		[Test]
		public void NodeIDFilterTest()
		{
			// Check for a hit.
			string nodeId = "123456789";
			args = null;
			subscriber.NodeIDFilter = nodeId;
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", nodeId, "0", "Node", NodeEventArgs.EventType.Created));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			// Check for a miss.
			args = null;
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", "987654321", "0", "Node", NodeEventArgs.EventType.Created));
			if (recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			subscriber.NodeIDFilter = null;
		}

		/// <summary>
		/// Type filter test.
		/// </summary>
		[Test]
		public void NodeTypeFilterTest()
		{
			// Check for a hit.
			string nodeId = "123456789";
			args = null;
			subscriber.NodeTypeFilter = "Node";
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", nodeId, "0", "Node", NodeEventArgs.EventType.Created));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			// Check for a miss.
			args = null;
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", nodeId, "0", "Collection", NodeEventArgs.EventType.Created));
			if (recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			subscriber.NodeTypeFilter = null;
		}

		/// <summary>
		/// Name filter test.
		/// </summary>
		[Test]
		public void FileNameFilterTest()
		{
			// Check for a hit.
			args = null;
			subscriber.FileNameFilter = "test.*";
			publisher.RaiseFileEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\testNode.txt", "0", FileEventArgs.EventType.Created));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			// Check for a miss.
			args = null;
			publisher.RaiseFileEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\tastNode.txt", "0", FileEventArgs.EventType.Created));
			if (recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			subscriber.FileNameFilter = null;
		}


		/// <summary>
		/// Type filter test.
		/// </summary>
		[Test]
		public void FileTypeFilterTest()
		{
			// Check for a hit.
			args = null;
			subscriber.FileTypeFilter = ".txt";
			publisher.RaiseFileEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\file.txt", "0", FileEventArgs.EventType.Created));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			// Check for a miss.
			args = null;
			publisher.RaiseFileEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\file.doc", "0", FileEventArgs.EventType.Created));
			if (recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			subscriber.FileTypeFilter = null;
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
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", "123456789", "0", "Node", NodeEventArgs.EventType.Created));
			if (recievedCallback)
			{
				throw new ApplicationException("Failed disable");
			}

			// Check reenabled.
			args = null;
			subscriber.Enabled = true;
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", "123456789", "0", "Node", NodeEventArgs.EventType.Created));
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
			publisher.RaiseServiceEvent(new ServiceEventArgs(ServiceEventArgs.TargetAll, ServiceEventArgs.ServiceEvent.Shutdown));
			publisher.RaiseServiceEvent(new ServiceEventArgs(ServiceEventArgs.TargetAll, ServiceEventArgs.ServiceEvent.Reconfigure));
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
							t.publisher.RaiseNodeEvent(new NodeEventArgs("nifp", i.ToString(), "0", "Node", NodeEventArgs.EventType.Created));
						}
					}
					break;

				case "PS":
					t.publisher = new EventPublisher();
					t.publisher.RaiseServiceEvent(new ServiceEventArgs(ServiceEventArgs.TargetAll, ServiceEventArgs.ServiceEvent.Shutdown));
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
