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
using System.Collections;
using System.Threading;

using NUnit.Framework;

using Simias.Storage;
using Simias.Sync;

namespace Simias.Sync.Tests
{
	/// <summary>
	/// Sync NUnit test cases
	/// </summary>
	//[TestFixture]
	public class Sync03Tests : Assertion
	{
		private static readonly int storeCount = 5;
		private static readonly string basePath = "./sync03-";
		private static readonly int basePort = 3000;

		private SyncTestStore[] stores;
		SyncTestCollection collection;
		string collectionName = "testcollection";

		/// <summary>
		/// Default Constructor
		/// </summary>
		public Sync03Tests()
		{
			stores = new SyncTestStore[storeCount];
		}

		/// <summary>
		/// Test fixture level setup
		/// </summary>
		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			// setup stores
			for(int i=0; i < storeCount; i++)
			{
				stores[i] = new SyncTestStore(basePath + i, basePort + i);
			}

			// create the master collection
			collection = stores[0].CreateCollection(collectionName);
			stores[0].StartSynker();

			// start the slave collections
			for(int i=1; i < storeCount; i++)
			{
				stores[1].SubscribeToCollection(collection);
				stores[0].StartSynker();
			}
		}

		/// <summary>
		/// Test fixture level tear down
		/// </summary>
		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			// tear down stores
			for(int i=0; i < storeCount; i++)
			{
				if (stores[i] != null)
				{
					stores[i].StopSynker();
					stores[i].Dispose();
				}
			}
		}

		/// <summary>
		/// Test case level setup
		/// </summary>
		[SetUp]
		public void CaseSetUp()
		{
		}

		/// <summary>
		/// Test case level tear down
		/// </summary>
		[TearDown]
		public void CaseTearDown()
		{
		}

		#region Tests

		[Test]
		public void TestAddFile()
		{
			string file = "testfile.txt";

			// create the file
			collection.CreateFile(file, 8);

			// let the magic happen
			Thread.Sleep(TimeSpan.FromSeconds(10));

			// check the slave collections
			for(int i=1; i < storeCount; i++)
			{
				SyncTestCollection slaveCollection =
					stores[i].OpenCollection(collection.Name);

				Assert(File.Exists(slaveCollection.GetFilePath(file)));
				Assert(collection.GetFileSize(file) == slaveCollection.GetFileSize(file));
			}
		}

		#endregion
	}
}
