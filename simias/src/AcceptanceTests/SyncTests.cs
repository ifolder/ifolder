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
using System.Threading;
using System.Diagnostics;

using NUnit.Framework;

using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.Invite;
using Simias.Service;

namespace Simias.Tests
{
	/// <summary>
	/// Sync Acceptance Tests
	/// </summary>
	[TestFixture]
	public class SyncTests : Assertion
	{
		// store paths
		private static string storePathA = Path.GetFullPath("./storeA");
		private static string storePathB = Path.GetFullPath("./storeB");
		private static string storePathC = Path.GetFullPath("./storeC");
		
		// configs
		private Configuration configA;
		private Configuration configB;
		private Configuration configC;

		// collection names
		private static string collectionName1 = "SharedFolder1";
		
		// collection paths
		private static string collectionPath1A = Path.Combine(storePathA, collectionName1);
		private static string collectionPath1B = Path.Combine(storePathB, collectionName1);
		private static string collectionPath1C = Path.Combine(storePathC, collectionName1);

		// sync logic
		private static Type syncLogicType = typeof(Simias.Sync.SynkerA);

		// stores
		private Store storeA;
		private Store storeB;
		private Store storeC;

		// properties
		private SyncProperties syncPropsA;
		private SyncProperties syncPropsB;
		private SyncProperties syncPropsC;

		// service managers
		private Manager managerA;
		private Manager managerB;
		private Manager managerC;

		// collections
		private Collection collection1A;
		private Collection collection1B;
		private Collection collection1C;

		// sync collections
		private SyncCollection sc1A;
		private SyncCollection sc1B;
		private SyncCollection sc1C;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public SyncTests()
		{
		}

		/// <summary>
		/// Test Fixture Setup
		/// </summary>
		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			try
			{
				// check directories
				DeleteDirectory(storePathA);
				DeleteDirectory(storePathB);
				DeleteDirectory(storePathC);

				// configs
				configA = new Configuration(storePathA);
				configB = new Configuration(storePathB);
				configC = new Configuration(storePathC);

				// create stores
				storeA = new Store(configA);
				storeB = new Store(configB);
				storeC = new Store(configC);

				// note: we need to revert any internal impersonations
				storeA.Revert();
				storeB.Revert();
				storeC.Revert();

				// set the sync properties for store A
				syncPropsA = new SyncProperties(configA);
				syncPropsA.LogicFactory = syncLogicType;
				syncPropsA.ChannelSinks = SyncChannelSinks.Binary | SyncChannelSinks.Monitor | SyncChannelSinks.Security;
				
				// start the service for store A
				managerA = new Manager(configA);
				managerA.StartServices();

				// set the sync properties for store B
				syncPropsB = new SyncProperties(configB);
				syncPropsB.LogicFactory = syncLogicType;
				syncPropsB.Port = syncPropsA.Port + 1;
				syncPropsB.ChannelSinks = SyncChannelSinks.Binary | SyncChannelSinks.Monitor | SyncChannelSinks.Security;
				
				// start the service for store B
				managerB = new Manager(configB);
				managerB.StartServices();

				// set the sync properties for store C
				syncPropsC = new SyncProperties(configC);
				syncPropsC.LogicFactory = syncLogicType;
				syncPropsC.Port = syncPropsA.Port + 2;
				syncPropsC.ChannelSinks = SyncChannelSinks.Binary | SyncChannelSinks.Monitor | SyncChannelSinks.Security;
				
				// start the service for store A
				managerC = new Manager(configC);
				managerC.StartServices();

				// create a master collection on A
				collection1A = new Collection(storeA, collectionName1);
				sc1A = new SyncCollection(collection1A);
				DirNode dn = new DirNode(sc1A, collectionPath1A);
				sc1A.Commit(dn);
				Console.WriteLine("Created Master Collection 1A: {0}", sc1A);

				// create an invitation for B and C
				Invitation invitation1 = sc1A.CreateInvitation(storeA.CurrentUserGuid);
				Console.WriteLine("Created Master Collection Invitation.");

				// accept the invitation on B and C
				invitation1.RootPath = storePathB;
                InvitationService.Accept(storeB, invitation1);
				Console.WriteLine("Accepted Invitation 1 on Store B.");
				
				invitation1.RootPath = storePathC;
				InvitationService.Accept(storeC, invitation1);
				Console.WriteLine("Accepted Invitation 1 on Store C.");

				// sleep for the sync interval
				Thread.Sleep(TimeSpan.FromSeconds(syncPropsA.Interval + 1));

				// get slave collection on B
				collection1B = storeB.GetCollectionByID(collection1A.ID);
				Assert("Sync collection not found on store B.", collection1B != null);
                Assert("Sync collection directory not found in store B.", Directory.Exists(collectionPath1B));
				sc1B = new SyncCollection(collection1B);
				Console.WriteLine("Found Slave Collection 1B: {0}", sc1B);

				// get slave collection on C
				collection1C = storeC.GetCollectionByID(collection1A.ID);
				Assert("Sync collection not found on store C.", collection1C != null);
                Assert("Sync collection directory not found in store C.", Directory.Exists(collectionPath1C));
				sc1C = new SyncCollection(collection1C);
				Console.WriteLine("Found Slave Collection 1C: {0}", sc1C);
			}
			catch(Exception e)
			{
				Console.WriteLine(e);

				throw e;
			}
		}

		private void DeleteDirectory(string path)
		{
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}

			Assert("Unable to delete directory: " + path, !Directory.Exists(path));
		}

		/// <summary>
		/// Test Fixture Tear-down
		/// </summary>
		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			// stop syncing
			managerC.StopServices();
			managerB.StopServices();
			managerA.StopServices();
		}

		/// <summary>
		/// Test Case Setup
		/// </summary>
		[SetUp]
		public void CaseSetup()
		{
		}

		/// <summary>
		/// Test Case Tear-down
		/// </summary>
		[TearDown]
		public void CaseTearDown()
		{
		}

		/// <summary>
		/// Test creating a new file in the master collection.
		/// </summary>
		[Test]
		public void TestCreateMasterFile()
		{
			const string file = "hello.txt";

			StreamWriter writer = File.CreateText(Path.Combine(collectionPath1A, file));
			writer.WriteLine("Hello, World!");
			writer.Close();

			// sleep for the sync interval
			Thread.Sleep(TimeSpan.FromSeconds((syncPropsA.Interval + 2) * 1));
			
			Assert("File not found in store B.", File.Exists(Path.Combine(collectionPath1B, file)));
			
			Assert("File not found in store C.", File.Exists(Path.Combine(collectionPath1C, file)));
		}

		/// <summary>
		/// Test modifying a file in the master collection.
		/// </summary>
		[Test]
		public void TestModifyMasterFile()
		{
			const string file = "hello.txt";

			StreamWriter writer = File.AppendText(Path.Combine(collectionPath1A, file));
			writer.WriteLine("Hello, Again!");
			writer.Close();

			long length = File.OpenRead(Path.Combine(collectionPath1A, file)).Length;

			// sleep for the sync interval
			Thread.Sleep(TimeSpan.FromSeconds((syncPropsA.Interval + 2) * 1));
			
			Assert("File not modified in store B.", File.OpenRead(Path.Combine(collectionPath1B, file)).Length == length);
			
			Assert("File not modified in store C.", File.OpenRead(Path.Combine(collectionPath1C, file)).Length == length);
		}

		/// <summary>
		/// Test deleting a file in the master collection.
		/// </summary>
		[Test]
		public void TestDeleteMasterFile()
		{
			const string file = "hello.txt";

			File.Delete(Path.Combine(collectionPath1A, file));

			// sleep for the sync interval
			Thread.Sleep(TimeSpan.FromSeconds((syncPropsA.Interval + 2) * 1));
			
			Assert("File not deleted in store B.", !File.Exists(Path.Combine(collectionPath1B, file)));
			
			Assert("File not deleted in store C.", !File.Exists(Path.Combine(collectionPath1C, file)));
		}

		/// <summary>
		/// Test creating a file in the slave collection.
		/// </summary>
		[Test]
		public void TestCreateSlaveFile()
		{
			const string file = "hello2.txt";

			StreamWriter writer = File.CreateText(Path.Combine(collectionPath1B, file));
			writer.WriteLine("Hello, World!");
			writer.Close();

			// sleep for the sync interval
			Thread.Sleep(TimeSpan.FromSeconds((syncPropsA.Interval + 2) * 2));
			
			Assert("File not found in store A.", File.Exists(Path.Combine(collectionPath1A, file)));
			
			Assert("File not found in store C.", File.Exists(Path.Combine(collectionPath1C, file)));
		}

		/// <summary>
		/// Test modifying a file in the slave collection.
		/// </summary>
		[Test]
		public void TestModifySlaveFile()
		{
			const string file = "hello2.txt";

			StreamWriter writer = File.AppendText(Path.Combine(collectionPath1B, file));
			writer.WriteLine("Hello, Again!");
			writer.Close();

			long length = File.OpenRead(Path.Combine(collectionPath1B, file)).Length;

			// sleep for the sync interval
			Thread.Sleep(TimeSpan.FromSeconds((syncPropsA.Interval + 2) * 2));
			
			Assert("File not modified in store A.", File.OpenRead(Path.Combine(collectionPath1A, file)).Length == length);
			
			Assert("File not modified in store C.", File.OpenRead(Path.Combine(collectionPath1C, file)).Length == length);
		}

		/// <summary>
		/// Test deleting a file in the slave collection.
		/// </summary>
		[Test]
		public void TestDeleteSlaveFile()
		{
			const string file = "hello2.txt";

			File.Delete(Path.Combine(collectionPath1B, file));

			// sleep for the sync interval
			Thread.Sleep(TimeSpan.FromSeconds((syncPropsA.Interval + 2) * 2));
			
			Assert("File not deleted in store A.", !File.Exists(Path.Combine(collectionPath1A, file)));
			
			Assert("File not deleted in store C.", !File.Exists(Path.Combine(collectionPath1C, file)));
		}
	}
}
