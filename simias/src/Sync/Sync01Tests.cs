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
	public class Sync01Tests : Assertion
	{
		private static readonly string basePath = "./sync01-";
		private static readonly int basePort = 1000;

		int index = 0;

		SyncTestStore store1;
		SyncTestStore store2;
		
		/// <summary>
		/// Default Constructor
		/// </summary>
		public Sync01Tests()
		{
		}

		/// <summary>
		/// Test case level setup
		/// </summary>
		[SetUp]
		public void CaseSetUp()
		{
			store1 = null;
			store2 = null;
		}

		/// <summary>
		/// Test case level tear down
		/// </summary>
		[TearDown]
		public void CaseTearDown()
		{
			if (store1 != null) store1.Dispose();
			if (store2 != null) store2.Dispose();
		}

		#region Simple Tests

		[Test]
		public void TestStart1()
		{
			int id = index++;

			store1 = new SyncTestStore(basePath + id,
				basePort + id);

			store1.StartSynker();

			Thread.Sleep(TimeSpan.FromSeconds(3));

			store1.StopSynker();

			Assert(store1.SynkerActivations > 1);
		}

		[Test]
		public void TestStart2()
		{
			int id1 = index++;
			int id2 = index++;

			SyncTestStore store1 = new SyncTestStore(basePath + id1,
				basePort + id1);

			SyncTestStore store2 = new SyncTestStore(basePath + id2,
				basePort + id2);

			store1.StartSynker();
			store2.StartSynker();

			Thread.Sleep(TimeSpan.FromSeconds(5));

			store1.StopSynker();
			store2.StopSynker();

			Assert(store1.SynkerActivations > 1);
			Assert(store2.SynkerActivations > 1);
		}

		[Test]
		public void TestAddFile()
		{
			int id = index++;
			string collection = "testcollection" + id;
			string file = "testfile" + id;

			store1 = new SyncTestStore(basePath + id,
				basePort + id);

			SyncTestCollection collection1 = store1.CreateCollection(collection);
			
			collection1.CreateFile(file, 1);

			store1.StartSynker();

			Thread.Sleep(TimeSpan.FromSeconds(3));

			store1.StopSynker();

			Assert(store1.SynkerActivations > 1);
		}

		#endregion

		#region Existing File Tests

		[Test]
		public void TestSyncExistingFile1K()
		{
			HelpTestSyncExistingFile(1);
		}

		[Test]
		public void TestSyncExistingFile8K()
		{
			HelpTestSyncExistingFile(8);
		}

		[Test]
		public void TestSyncExistingFile32K()
		{
			HelpTestSyncExistingFile(32);
		}

		[Test]
		public void TestSyncExistingFile1M()
		{
			HelpTestSyncExistingFile(1024);
		}

		[Test]
		public void TestSyncExistingFile8M()
		{
			HelpTestSyncExistingFile(8 * 1024);
		}

		private void HelpTestSyncExistingFile(long size)
		{
			int id1 = index++;
			int id2 = index++;
			string collection = "testcollection" + id1;
			string file = "testfile" + id1;

			store1 = new SyncTestStore(basePath + id1,
				basePort + id1);

			store2 = new SyncTestStore(basePath + id2,
				basePort + id2);

			SyncTestCollection collection1 = store1.CreateCollection(collection);
			store2.SubscribeToCollection(collection1);
			
			collection1.CreateFile(file, size);

			store1.StartSynker();

			store2.StartSynker();

			// let the magic happen
			Thread.Sleep(TimeSpan.FromMilliseconds(size)
				+ TimeSpan.FromSeconds(4));

			store2.StopSynker();
			store1.StopSynker();

			Assert(store1.SynkerActivations > 1);
			Assert(store2.SynkerActivations > 1);

			SyncTestCollection collection2 = store2.OpenCollection(collection1.Name);

			Assert(File.Exists(collection2.GetFilePath(file)));
			Assert(collection1.GetFileSize(file) == collection2.GetFileSize(file));
		}

		#endregion

		#region New File Tests

		[Test]
		public void TestSyncNewFile4K()
		{
			HelpTestSyncNewFile(4);
		}

		[Test]
		public void TestSyncNewFile8M()
		{
			HelpTestSyncNewFile(8 * 1024);
		}

		private void HelpTestSyncNewFile(long size)
		{
			int id1 = index++;
			int id2 = index++;
			string collection = "testcollection" + id1;
			string file = "testfile" + id1;

			SyncTestStore store1 = new SyncTestStore(basePath + id1,
				basePort + id1);

			SyncTestStore store2 = new SyncTestStore(basePath + id2,
				basePort + id2);

			SyncTestCollection collection1 = store1.CreateCollection(collection);
			store2.SubscribeToCollection(collection1);
			
			store1.StartSynker();

			store2.StartSynker();

			// let the synkers start
			Thread.Sleep(TimeSpan.FromSeconds(1));

			// create the file
			collection1.CreateFile(file, size);

			// let the magic happen
			Thread.Sleep(TimeSpan.FromMilliseconds(size)
				+ TimeSpan.FromSeconds(4));

			store2.StopSynker();
			store1.StopSynker();

			Assert(store1.SynkerActivations > 1);
			Assert(store2.SynkerActivations > 1);

			SyncTestCollection collection2 = store2.OpenCollection(collection1.Name);

			Assert(File.Exists(collection2.GetFilePath(file)));
			Assert(collection1.GetFileSize(file) == collection2.GetFileSize(file));
		}

		#endregion

		#region Delete File Tests

		[Test]
		public void TestSyncDeleteFile1K()
		{
			HelpTestSyncDeleteFile(1);
		}

		[Test]
		public void TestSyncDeleteFile8M()
		{
			HelpTestSyncDeleteFile(8 * 1024);
		}

		private void HelpTestSyncDeleteFile(long size)
		{
			int id1 = index++;
			int id2 = index++;
			string collection = "testcollection" + id1;
			string file = "testfile" + id1;

			SyncTestStore store1 = new SyncTestStore(basePath + id1,
				basePort + id1);

			SyncTestStore store2 = new SyncTestStore(basePath + id2,
				basePort + id2);

			SyncTestCollection collection1 = store1.CreateCollection(collection);
			store2.SubscribeToCollection(collection1);
			
			collection1.CreateFile(file, size);

			store1.StartSynker();

			store2.StartSynker();

			// let the magic happen
			Thread.Sleep(TimeSpan.FromMilliseconds(size)
				+ TimeSpan.FromSeconds(10));

			store2.StopSynker();
			store1.StopSynker();

			Assert(store1.SynkerActivations > 1);
			Assert(store2.SynkerActivations > 1);

			SyncTestCollection collection2 = store2.OpenCollection(collection1.Name);

			Assert(File.Exists(collection2.GetFilePath(file)));
			Assert(collection1.GetFileSize(file) == collection2.GetFileSize(file));

			// delete the file
			File.Delete(collection2.GetFilePath(file));

			// let the magic happen
			Thread.Sleep(TimeSpan.FromMilliseconds(size)
				+ TimeSpan.FromSeconds(4));

			Assert(!File.Exists(collection2.GetFilePath(file)));
			Assert(!File.Exists(collection1.GetFilePath(file)));
		}

		#endregion

		#region Rename File Tests

		[Test]
		public void TestSyncRenameFile1K()
		{
			HelpTestSyncRenameFile(1);
		}

		[Test]
		public void TestSyncRenameFile8M()
		{
			HelpTestSyncRenameFile(8 * 1024);
		}

		private void HelpTestSyncRenameFile(long size)
		{
			int id1 = index++;
			int id2 = index++;
			string collection = "testcollection" + id1;
			string file = "testfile" + id1;
			string newFile = "newtestfile" + id1;

			SyncTestStore store1 = new SyncTestStore(basePath + id1,
				basePort + id1);

			SyncTestStore store2 = new SyncTestStore(basePath + id2,
				basePort + id2);

			SyncTestCollection collection1 = store1.CreateCollection(collection);
			store2.SubscribeToCollection(collection1);
			
			collection1.CreateFile(file, size);

			store1.StartSynker();

			store2.StartSynker();

			// let the magic happen
			Thread.Sleep(TimeSpan.FromMilliseconds(size)
				+ TimeSpan.FromSeconds(10));

			store2.StopSynker();
			store1.StopSynker();

			Assert(store1.SynkerActivations > 1);
			Assert(store2.SynkerActivations > 1);

			SyncTestCollection collection2 = store2.OpenCollection(collection1.Name);

			Assert(File.Exists(collection2.GetFilePath(file)));
			Assert(collection1.GetFileSize(file) == collection2.GetFileSize(file));

			// delete the file
			File.Move(collection2.GetFilePath(file), collection2.GetFilePath(newFile));

			// let the magic happen
			Thread.Sleep(TimeSpan.FromMilliseconds(size)
				+ TimeSpan.FromSeconds(4));

			Assert(!File.Exists(collection2.GetFilePath(file)));
			Assert(!File.Exists(collection1.GetFilePath(file)));

			Assert(File.Exists(collection2.GetFilePath(newFile)));
			Assert(File.Exists(collection1.GetFilePath(newFile)));

			Assert(collection1.GetFileSize(newFile) == collection2.GetFileSize(newFile));
		}

		#endregion
	}
}
