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
	[TestFixture]
	public class Sync02Tests : Assertion
	{
		private static readonly string basePath = "./sync02-";
		private static readonly int basePort = 2000;
		private static readonly string collectionName = "testcollection";

		private SyncTestStore store1;
		private SyncTestStore store2;
		private SyncTestCollection collection1;
		private SyncTestCollection collection2;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public Sync02Tests()
		{
		}

		/// <summary>
		/// Test fixture level setup
		/// </summary>
		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			store1 = new SyncTestStore(basePath + 1, basePort + 1);
			store2 = new SyncTestStore(basePath + 2, basePort + 2);

			collection1 = store1.CreateCollection(collectionName);
			collection1.CreateFile("start.txt", 1);

			store2.SubscribeToCollection(collection1);
			
			// start synker
			store1.StartSynker();
			store2.StartSynker();

			// wait for collection to sync
			Thread.Sleep(TimeSpan.FromSeconds(6));
			collection2 = store2.OpenCollection(collection1.Name);
		}

		/// <summary>
		/// Test fixture level tear down
		/// </summary>
		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			try
			{
				store2.StopSynker();
				store2.Dispose();
				store2 = null;
			}
			catch(Exception e)
			{
				MyTrace.WriteLine(e);
			}

			// kludge - shutdown the client first
			GC.Collect();

			try
			{
				store1.StopSynker();
				store1.Dispose();
				store1 = null;
			}
			catch(Exception e)
			{
				MyTrace.WriteLine(e);
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
			string file = "testaddfile.txt";

			// create the file
			collection1.CreateFile(file, 8);

			// let the magic happen
			Thread.Sleep(TimeSpan.FromSeconds(10));

			// check the file
			Assert(File.Exists(collection2.GetFilePath(file)));
			Assert(collection1.GetFileSize(file) == collection2.GetFileSize(file));
		}

		[Test]
		public void TestChangeFile()
		{
			string file = "testchangefile.txt";

			// create the file
			collection1.CreateFile(file, 8);

			// let the magic happen
			Thread.Sleep(TimeSpan.FromSeconds(10));

			// check the file
			Assert(File.Exists(collection2.GetFilePath(file)));
			Assert(collection1.GetFileSize(file) == collection2.GetFileSize(file));

			// change the file
			collection1.CreateFile(file, 16);
			
			// let the magic happen
			Thread.Sleep(TimeSpan.FromSeconds(10));

			// check the file
			Assert(File.Exists(collection2.GetFilePath(file)));
			Assert(collection1.GetFileSize(file) == collection2.GetFileSize(file));
		}

		[Test]
		public void TestDeleteFile()
		{
			string file = "testdeletefile.txt";

			// create the file
			collection1.CreateFile(file, 8);

			// let the magic happen
			Thread.Sleep(TimeSpan.FromSeconds(10));

			// check the file
			Assert(File.Exists(collection2.GetFilePath(file)));
			Assert(collection1.GetFileSize(file) == collection2.GetFileSize(file));

			// delete the file
			collection1.DeleteFile(file);
			
			// let the magic happen
			Thread.Sleep(TimeSpan.FromSeconds(10));

			// check the file
			Assert(!File.Exists(collection2.GetFilePath(file)));
		}

		[Test]
		public void TestRenameFile()
		{
			string srcFile = "testsrcfile.txt";
			string dstFile = "testdstfile.txt";

			// create the file
			collection1.CreateFile(srcFile, 8);

			// let the magic happen
			Thread.Sleep(TimeSpan.FromSeconds(10));

			// check the file
			Assert(File.Exists(collection2.GetFilePath(srcFile)));
			Assert(collection1.GetFileSize(srcFile) == collection2.GetFileSize(srcFile));

			// rename the file
			collection1.RenameFile(srcFile, dstFile);
			
			// let the magic happen
			Thread.Sleep(TimeSpan.FromSeconds(10));

			// check the file
			Assert(File.Exists(collection2.GetFilePath(dstFile)));
			Assert(!File.Exists(collection2.GetFilePath(srcFile)));
			Assert(collection1.GetFileSize(dstFile) == collection2.GetFileSize(dstFile));
		}

		#endregion
	}
}
