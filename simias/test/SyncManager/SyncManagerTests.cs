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
using System.Threading;
using System.Collections;
using System.Collections.Specialized;
using System.IO;

using NUnit.Framework;

using Simias;
using Simias.Storage;
using Simias.Sync;

namespace Simias.Sync.Tests
{
	/// <summary>
	/// Sync Manager Tests
	/// </summary>
	[TestFixture]
	public class SyncManagerTests
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public SyncManagerTests()
		{
		}

		/// <summary>
		/// Simple test of the sync manager.
		/// </summary>
		[Test]
		public void TestSyncManager()
		{
			string path = Path.GetFullPath("./manager1");

			// clean-up
			if (Directory.Exists(path)) Directory.Delete(path, true);
			
			// configuration
			Configuration config = Configuration.CreateDefaultConfig(path);

			SyncProperties properties = new SyncProperties(config);

			properties.ChannelSinks = SyncChannelSinks.Binary | SyncChannelSinks.Monitor;

			SyncManager manager = new SyncManager(config);
			
			manager.Start();

			Thread.Sleep(TimeSpan.FromSeconds(5));

			manager.Stop();

			manager.Dispose();
		}

		/// <summary>
		/// A new collection with the sync manager.
		/// </summary>
		[Test]
		public void TestSyncManager2()
		{
			string path = Path.GetFullPath("./manager2");

			// clean-up
			if (Directory.Exists(path)) Directory.Delete(path, true);
			
			// configuration
			Configuration config = Configuration.CreateDefaultConfig(path);

			// store
			Store store = Store.GetStore();
			
			// note: we need to revert any internal impersonations
			store.Revert();

			// new collection
			Collection collection = new Collection(store, "Test Collection");
			collection.Commit();

			// sync
			SyncProperties properties = new SyncProperties(config);

			properties.ChannelSinks = SyncChannelSinks.Binary | SyncChannelSinks.Monitor;

			SyncManager manager = new SyncManager(config);
			
			manager.Start();

			Thread.Sleep(TimeSpan.FromSeconds(5));

			manager.Stop();

			manager.Dispose();
		}
	}
}
