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

using NUnit.Framework;

using Simias.Sync;

namespace Simias.Sync.Tests
{
	/// <summary>
	/// Sync Common Tests
	/// </summary>
	[TestFixture]
	public class SyncCommonTests : Assertion
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public SyncCommonTests()
		{
		}

		/// <summary>
		/// Test channel sinks enumeration.
		/// </summary>
		[Test]
		public void TestSinksEnum()
		{
			string[] names = Enum.GetNames(typeof(SyncChannelSinks));

			foreach(string name in names)
			{
				Console.WriteLine("Sink Enum: {0}", name);
			}
		}

		/// <summary>
		/// Test the sync store class.
		/// </summary>
		[Test]
		public void TestSyncStore()
		{
			string path = "./syncstore1";

			SyncStore store = new SyncStore(path);

			Assert(store.StorePath == Path.GetFullPath(path));
			Assert(store.ID == store.BaseStore.GetDatabaseObject().Id);

			store.Delete();
		}

		/// <summary>
		/// Test the sync collection class.
		/// </summary>
		[Test]
		public void TestSyncCollection()
		{
			string storePath = "./syncstore2";
			string collectionPath = "./synccollection2";

			SyncStore store = new SyncStore(storePath);

			SyncCollection sc = store.CreateCollection("synccollection2", collectionPath);

			Assert(sc != null);

			Console.WriteLine("Collection \"{0}\" Domain: {1}", sc.Name, sc.Domain);
			
			Assert(sc.Domain != null);
			
			Console.WriteLine("Collection \"{0}\" Access Identity: {1}", sc.Name, sc.AccessIdentity);
			
			Console.WriteLine("Current Identity: {0}", store.BaseStore.CurrentUser);

			Assert(sc.AccessIdentity != null);

			store.Delete();
		}
	}
}
