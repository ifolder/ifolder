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

namespace Simias.Sync.Tests
{
	/// <summary>
	/// Store Watcher Tests
	/// </summary>
	[TestFixture]
	public class StoreWatcherTests : Assertion
	{
		private StoreWatcher watcher;
		private Store store;
		private int count;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public StoreWatcherTests()
		{
			MyTrace.SendToConsole();
		}

		/// <summary>
		/// Test Fixture Set Up
		/// </summary>
		[TestFixtureSetUp]
		public void SetUp()
		{
			count = 0;
			
			string path = Path.GetFullPath("./cwatcher");

			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}

			Configuration config = new Configuration(path);
			
			store = new Store(config);

			watcher = new StoreWatcher(config);

			watcher.Created +=new CreatedCollectionEventHandler(this.OnCreated);
			watcher.Deleted +=new DeletedCollectionEventHandler(this.OnDeleted);

			watcher.Start();
			
			Thread.Sleep(TimeSpan.FromSeconds(watcher.Interval + 1));
		}

		/// <summary>
		/// Test Fixture Tear Down
		/// </summary>
		[TestFixtureTearDown]
		public void TearDown()
		{
			watcher.Stop();
			watcher.Dispose();

			// remove store
			store.Delete();
			store = null;
		}

		/// <summary>
		/// Called when a collection is created (or discovered).
		/// </summary>
		/// <param name="id">The collection id.</param>
		public void OnCreated(string id)
		{
			++count;
		}

		/// <summary>
		/// Called when a collection is deleted.
		/// </summary>
		/// <param name="id">The collecion id.</param>
		public void OnDeleted(string id)
		{
			--count;
		}

		/// <summary>
		/// Test the Store Watcher
		/// </summary>
		public void TestStoreWatcher()
		{
			Assert(count == 2);

			Collection c = new Collection(store, "Test Collection 1");
			c.Commit();

			Thread.Sleep(TimeSpan.FromSeconds(watcher.Interval + 1));

			Assert(count == 3);

			c.Delete();
			
			// kludge
			GC.Collect();

			// check delete
			Thread.Sleep(TimeSpan.FromSeconds(watcher.Interval + 1));
			
			Assert(count == 2);

			// check maintained delete
			Thread.Sleep(TimeSpan.FromSeconds(watcher.Interval + 1));
			
			Assert(count == 2);
		}
	}
}
