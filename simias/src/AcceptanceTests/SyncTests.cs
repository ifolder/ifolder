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
using System.IO;
using System.Threading;

using NUnit.Framework;

using Simias.Agent;
using Simias.Storage;
using Simias.Sync;

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
			// send trace messages to the console
			MyTrace.SendToConsole();
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
				sc1A.Host = SyncProperties.SuggestedHost;
				sc1A.Port = SyncProperties.SuggestedPort;
				sc1A.Commit();

				// create an invitation for B and C
				Invitation invitation1 = sc1A.CreateInvitation(storeA.CurrentUser);
				MyTrace.WriteLine("Created Master Collection Invitation.");

				// accept the invitation on B and C
				IInviteAgent agent = AgentFactory.GetInviteAgent();
				agent.Accept(storeB, invitation1);
				MyTrace.WriteLine("Accepted Invitation 1 on Store B.");
				agent.Accept(storeC, invitation1);
				MyTrace.WriteLine("Accepted Invitation 1 on Store C.");

				// sleep for the sync interval
				Thread.Sleep(TimeSpan.FromSeconds(SyncProperties.SuggestedSyncInterval + 1));

				// get slave collection on B
				collection1B = storeB.GetCollectionById(collection1A.Id);
				Assert("Sync collection on found on store B.", collection1B != null);
				sc1B = new SyncCollection(collection1B);
				MyTrace.WriteLine("Found Slave Collection 1B: {0}", sc1B);

				// get slave collection on C
				collection1C = storeC.GetCollectionById(collection1A.Id);
				Assert("Sync collection on found on store C.", collection1C != null);
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

		[Test]
		public void TestCollectionSync()
		{
			

		}
	}
}
