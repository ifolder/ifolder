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
using System.Diagnostics;

using NUnit.Framework;

using Simias.Storage;
using Simias.Sync;

namespace Simias.Sync.Tests
{
	/// <summary>
	/// Collection Watcher Tests
	/// </summary>
	[TestFixture]
	public class CollectionWatcherTests : Assertion
	{
		private CollectionWatcher watcher;
		private Store store;
		private Collection collection;
		private int addedFile;
		private int changedFile;
		private int deletedFile;
		private int addedDir;
		private int deletedDir;
		private string collectionPath;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public CollectionWatcherTests()
		{
			MyTrace.SendToConsole();
		}

		/// <summary>
		/// Test Fixture Set Up
		/// </summary>
		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			string name = "collection1";

			string path = Path.GetFullPath("./cwatcher");

			collectionPath = Path.Combine(path, name);

			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}

			Directory.CreateDirectory(collectionPath);

			store = Store.Connect(new Uri(path), null);
			Trace.Assert(store != null);
			collection = store.CreateCollection(name, new Uri(collectionPath));
			Trace.Assert(collection != null);
			collection.Commit();

			watcher = new CollectionWatcher(path, collection.Id);

			watcher.AddedFile +=new AddedFileEventHandler(this.OnAddedFile);
			watcher.ChangedFile +=new ChangedFileEventHandler(this.OnChangedFile);
			watcher.DeletedFile +=new DeletedFileEventHandler(this.OnDeletedFile);

			watcher.AddedDirectory +=new AddedDirectoryEventHandler(this.OnAddedDirectory);
			watcher.DeletedDirectory +=new DeletedDirectoryEventHandler(this.OnDeletedDirectory);

			watcher.Start();
		}

		/// <summary>
		/// Test Fixture Tear Down
		/// </summary>
		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			watcher.Stop();
			watcher.Dispose();
			watcher = null;
			collection = null;

			// kludge for the store provider
			GC.Collect();

			// remove store
			store.ImpersonateUser(Access.StoreAdminRole);
			store.Delete();
		}

		/// <summary>
		/// Test Case Set Up
		/// </summary>
		[SetUp]
		public void SetUp()
		{
			addedFile = 0;
			changedFile = 0;
			deletedFile = 0;
			addedDir = 0;
			deletedDir = 0;
			
		}

		/// <summary>
		/// Called when a file is added.
		/// </summary>
		/// <param name="id">The node id.</param>
		public void OnAddedFile(string id)
		{
			++addedFile;
		}

		/// <summary>
		/// Called when a file is changed.
		/// </summary>
		/// <param name="id">The node id.</param>
		public void OnChangedFile(string id)
		{
			++changedFile;
		}

		/// <summary>
		/// Called when a file is deleted.
		/// </summary>
		/// <param name="id">The node id.</param>
		public void OnDeletedFile(string id)
		{
			++deletedFile;
		}

		/// <summary>
		/// Called when a directory is added.
		/// </summary>
		/// <param name="id">The node id.</param>
		public void OnAddedDirectory(string id)
		{
			++addedDir;
		}

		/// <summary>
		/// Called when a directory is deleted.
		/// </summary>
		/// <param name="id">The node id.</param>
		public void OnDeletedDirectory(string id)
		{
			++deletedDir;
		}

		/// <summary>
		/// Test the CollectionWatcher.
		/// </summary>
		public void TestCollectionWatcher1()
		{
			string filePath = Path.Combine(collectionPath, "testfile1.txt");

			// create a file
			FileStream fs = File.Create(filePath);
			fs.WriteByte(1);
			fs.WriteByte(2);
			fs.Close();

			// wait and check
			Thread.Sleep(TimeSpan.FromSeconds(watcher.Interval + 2));
			Assert(addedFile == 1);

			// modify file
			fs = File.OpenWrite(filePath);
			fs.WriteByte(3);
			fs.WriteByte(4);
			fs.Close();

			// wait and check
			Thread.Sleep(TimeSpan.FromSeconds(watcher.Interval + 2));
			Assert(changedFile == 1);

			// delete file
			File.Delete(filePath);

			// wait and check
			Thread.Sleep(TimeSpan.FromSeconds(watcher.Interval + 2));
			Assert(deletedFile == 1);
		}

		/// <summary>
		/// Test the CollectionWatcher.
		/// </summary>
		public void TestCollectionWatcher2()
		{
			string dirPath = Path.Combine(collectionPath, "testdirectory1");
			string filePath = Path.Combine(dirPath, "testfile1.txt");

			// create a directory
			Directory.CreateDirectory(dirPath);

			// wait and check
			Thread.Sleep(TimeSpan.FromSeconds(watcher.Interval + 2));
			Assert(addedDir == 1);

			// create a file
			FileStream fs = File.Create(filePath);
			fs.WriteByte(1);
			fs.WriteByte(2);
			fs.Close();

			// wait and check
			Thread.Sleep(TimeSpan.FromSeconds(watcher.Interval + 2));
			Assert(addedFile == 1);

			// modify file
			fs = File.OpenWrite(filePath);
			fs.WriteByte(3);
			fs.WriteByte(4);
			fs.Close();

			// wait and check
			Thread.Sleep(TimeSpan.FromSeconds(watcher.Interval + 2));
			Assert("Changed File", changedFile == 1);

			// delete directory
			Directory.Delete(dirPath, true);

			// wait and check
			Thread.Sleep(TimeSpan.FromSeconds(watcher.Interval + 2));
			Assert("Deleted File", deletedFile == 1);
			Assert("Deleted Directory", deletedDir == 1);
		}
	}
}
