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

using Simias.Storage;
using Simias.Sync;
using Simias.Invite;

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
		
		// store URIs
		private static Uri storeUriA = new Uri(storePathA);
		private static Uri storeUriB = new Uri(storePathB);
		private static Uri storeUriC = new Uri(storePathC);

		// collection names
		private static string collectionName1 = "SharedFolder1";
		
		// collection paths
		private static string collectionPath1A = Path.Combine(storePathA, collectionName1);
		private static string collectionPath1B = Path.Combine(storePathB, collectionName1);
		private static string collectionPath1C = Path.Combine(storePathC, collectionName1);

		// collection URIs
		private static Uri collectionUri1A = new Uri(collectionPath1A);
		private static Uri collectionUri1B = new Uri(collectionPath1B);
		private static Uri collectionUri1C = new Uri(collectionPath1C);

		// sync logic
		private static Type syncLogicType = typeof(Simias.Sync.SynkerA);

		// stores
		private Store storeA;
		private Store storeB;
		private Store storeC;

		// sync managers
		private SyncManager syncManagerA;
		private SyncManager syncManagerB;
		private SyncManager syncManagerC;

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

				// create stores
				storeA = Store.Connect(storeUriA);
				storeB = Store.Connect(storeUriB);
				storeC = Store.Connect(storeUriC);

				// start the sync manager for store A
				SyncProperties syncPropsA = new SyncProperties();
				syncPropsA.DefaultLogicFactory = syncLogicType;
				syncPropsA.StorePath = storePathA;
				syncPropsA.DefaultChannelSinks = SyncChannelSinks.Binary | SyncChannelSinks.Monitor | SyncChannelSinks.Security;
				syncManagerA = new SyncManager(syncPropsA);
				syncManagerA.Start();

				// start the sync manager for store B
				SyncProperties syncPropsB = new SyncProperties();
				syncPropsB.DefaultLogicFactory = syncLogicType;
				syncPropsB.StorePath = storePathB;
				syncPropsB.DefaultPort = SyncProperties.SuggestedPort + 1;
				syncPropsB.DefaultChannelSinks = SyncChannelSinks.Binary | SyncChannelSinks.Monitor | SyncChannelSinks.Security;
				syncManagerB = new SyncManager(syncPropsB);
				syncManagerB.Start();

				// start the sync manager for store C
				SyncProperties syncPropsC = new SyncProperties();
				syncPropsC.DefaultLogicFactory = syncLogicType;
				syncPropsC.StorePath = storePathC;
				syncPropsC.DefaultPort = SyncProperties.SuggestedPort + 2;
				syncPropsC.DefaultChannelSinks = SyncChannelSinks.Binary | SyncChannelSinks.Monitor | SyncChannelSinks.Security;
				syncManagerC = new SyncManager(syncPropsC);
				syncManagerC.Start();

				// create a master collection on A
				collection1A = storeA.CreateCollection(collectionName1, collectionUri1A);
				sc1A = new SyncCollection(collection1A);
				MyTrace.WriteLine("Created Master Collection 1A: {0}", sc1A);

				// set the host and port on the master collection
				UriBuilder builder = new UriBuilder("http", SyncProperties.SuggestedHost, SyncProperties.SuggestedPort);
				sc1A.MasterUri = builder.Uri;
				sc1A.Commit();

				// create an invitation for B and C
				Invitation invitation1 = sc1A.CreateInvitation(storeA.CurrentUser);
				MyTrace.WriteLine("Created Master Collection Invitation.");

				// accept the invitation on B and C
				invitation1.RootPath = storePathB;
                InvitationService.Accept(storeB, invitation1);
				MyTrace.WriteLine("Accepted Invitation 1 on Store B.");
				
				invitation1.RootPath = storePathC;
				InvitationService.Accept(storeC, invitation1);
				MyTrace.WriteLine("Accepted Invitation 1 on Store C.");

				// sleep for the sync interval
				Thread.Sleep(TimeSpan.FromSeconds(SyncProperties.SuggestedSyncInterval + 1));

				// get slave collection on B
				collection1B = storeB.GetCollectionById(collection1A.Id);
				Assert("Sync collection not found on store B.", collection1B != null);
                Assert("Sync collection directory not found in store B.", Directory.Exists(collectionPath1B));
				sc1B = new SyncCollection(collection1B);
				MyTrace.WriteLine("Found Slave Collection 1B: {0}", sc1B);

				// get slave collection on C
				collection1C = storeC.GetCollectionById(collection1A.Id);
				Assert("Sync collection not found on store C.", collection1C != null);
                Assert("Sync collection directory not found in store C.", Directory.Exists(collectionPath1C));
				sc1C = new SyncCollection(collection1C);
				MyTrace.WriteLine("Found Slave Collection 1C: {0}", sc1C);
			}
			catch(Exception e)
			{
				MyTrace.WriteLine(e);

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
			syncManagerC.Stop();
			syncManagerB.Stop();
			syncManagerA.Stop();

			// delete stores
			DeleteStore(ref storeC);
			DeleteStore(ref storeB);
			DeleteStore(ref storeA);
		}

		private void DeleteStore(ref Store store)
		{
			if (store != null)
			{
				store.ImpersonateUser(Access.StoreAdminRole);
				store.Delete();
				store = null;
			}
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
			Thread.Sleep(TimeSpan.FromSeconds((SyncProperties.SuggestedSyncInterval + 2) * 1));
			
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
			Thread.Sleep(TimeSpan.FromSeconds((SyncProperties.SuggestedSyncInterval + 2) * 1));
			
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
			Thread.Sleep(TimeSpan.FromSeconds((SyncProperties.SuggestedSyncInterval + 2) * 1));
			
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
			Thread.Sleep(TimeSpan.FromSeconds((SyncProperties.SuggestedSyncInterval + 2) * 2));
			
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
			Thread.Sleep(TimeSpan.FromSeconds((SyncProperties.SuggestedSyncInterval + 2) * 2));
			
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
			Thread.Sleep(TimeSpan.FromSeconds((SyncProperties.SuggestedSyncInterval + 2) * 2));
			
			Assert("File not deleted in store A.", !File.Exists(Path.Combine(collectionPath1A, file)));
			
			Assert("File not deleted in store C.", !File.Exists(Path.Combine(collectionPath1C, file)));
		}
	}
}
