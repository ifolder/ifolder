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
 *  Author: Dale Olds <olds@novell.com>
 * 
 ***********************************************************************/
using System;
using System.Collections;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.Identity;

namespace Simias.Sync.Tests
{

//---------------------------------------------------------------------------

/// <summary>
/// Sync Tests
/// </summary>
[TestFixture]
public class Tests : Assertion
{
	private const string host = "127.0.0.1";
	private const int portS = 1100, portC = 1101;
	private const string storeLocS = "tmpStoreS", storeLocC = "tmpStoreC";

	private Store storeS = null, storeC = null;

	// initialization helper methods
	private static Store InitStore(string name)
	{
		//Console.WriteLine("connecting to store {0}", name);
		Uri loc = new Uri(Path.Combine(Directory.GetCurrentDirectory(), name));
		Store tmp = Store.Connect(loc);
		return tmp;
	}

	private static void AddFile(string pathA, string pathB, string name, string contents)
	{
		//Console.WriteLine("adding file {0}", name);
		string path = Path.Combine(Directory.GetCurrentDirectory(), pathA);
		path = Path.Combine(path, pathB);
		Directory.CreateDirectory(path);
		path = Path.Combine(path, name);
		//Console.WriteLine("adding file {0} to {1}", name, fpath);
		StreamWriter s = new StreamWriter(File.Create(path));
		s.Write(contents);
		s.Close();
	}

	private static string AddCollection(Store store, string name, string path)
	{
		//Console.WriteLine("adding collection {0}", name);
		string fullp = Path.Combine(Directory.GetCurrentDirectory(), path);
		fullp = Path.Combine(fullp, name);
		Directory.CreateDirectory(fullp);
		Collection c = store.CreateCollection(name, new Uri(fullp));
		c.Commit(true);
		return c.Id;
	}

	private static void AddShareInfo(Store store, string host, int port,
			string collectionId, string localPath)
	{
		//Console.WriteLine("adding share info {0}", localPath);
		store.ImpersonateUser(Access.SyncOperatorRole, null);
		Collection dbo = store.GetDatabaseObject();
		if (dbo == null)
			Console.WriteLine("no dbo");
		IIdentity id = IdentityManager.Connect().CurrentId;
		Node shares = dbo.GetSingleNodeByName("Shares");
		if (shares == null)
			shares = dbo.CreateChild("Shares", "Shares");
			
		Console.WriteLine("adding share info {0} 1 ", localPath);
		Node share = shares.CreateChild(id.UserGuid, "Share");
		share.Properties.AddProperty("ShareUri", "nifp://" + host + ":"
				+ port + "/" + collectionId);
		share.Properties.AddProperty("RootPath",
				Path.Combine(Directory.GetCurrentDirectory(), localPath));
		dbo.Commit(true);
	}

	/// <summary>
	/// Performs pre-initialization tasks.
	/// </summary>
	[TestFixtureSetUp]
	public void Init()
	{
		try
		{
			// set up server store and collections, and some ifolder file data
			storeS = InitStore(storeLocS);
			string c1 = AddCollection(storeS, "tmpFolder1", "tmpS");
			AddFile("tmpS", "tmpFolder1", "content1.txt", "content1 -- blah, blah");
	
			string c2 = AddCollection(storeS, "tmpFolder2", "tmpS");
			AddFile("tmpS", "tmpFolder2", "content2.txt", "content1 -- blah, blah");
	
			string c3 = AddCollection(storeS, "tmpFolder3", "tmpS");
			AddFile("tmpS", "tmpFolder3", "content3.txt", "content3 -- blah, blah");
	
			// set up the server collection share info
			AddShareInfo(storeS, host, portS, c1, "tmpS");
			AddShareInfo(storeS, host, portS, c2, "tmpS");
			AddShareInfo(storeS, host, portS, c3, "tmpS");
	
			// set up client store and just share info to get data from server
			storeC = InitStore(storeLocC);
			AddShareInfo(storeC, host, portS, c1, "tmpC");
			AddShareInfo(storeC, host, portS, c2, "tmpC");
			AddShareInfo(storeC, host, portS, c3, "tmpC");
		}
		catch (System.Exception e)
		{
			Console.WriteLine("Uncaught exception in Init: {0}", e.Message);
			throw;
		}
		catch
		{
			Console.WriteLine("Foreign exception in Init");
			throw;
		}
	}

	public void StatusUpdateC(bool active)
	{
		Console.WriteLine("Changing client sync status to {0}",
				active? "Active": "Inactive");
	}

	public void StatusUpdateS(bool active)
	{
		Console.WriteLine("Changing server sync status to {0}",
				active? "Active": "Inactive");
	}

	public void T0() {}
/*
	CRG - This test just ain't right for simias
#if true
	public void T0() {}
#else
	[Test]
	public void T0()
	{
		string storeName = "tmpStore0";
		Store store = InitStore(storeName);
		string c1 = AddCollection(store, "Collection0", "tmpFolder0");
		AddFile("tmpFolder0", "content0.txt", "content0 -- blah, blah");
		AddFile("tmpFolder0", "content1.txt", "content1 -- blah, blah");
		AddFile("tmpFolder0", "content2.txt", "content2 -- blah, blah");

		iFolderManager man = iFolderManager.Connect(new Uri(Path.Combine(
				Directory.GetCurrentDirectory(), storeName)));
		man.UpdateiFolder(c1);
		man = null;

		Console.WriteLine("T0: created ifolder, looking for 3 streams");
		Collection c = store.GetCollectionById(c1);

		Property p = c.Properties.GetSingleProperty(Property.DocumentRoot);
		Uri docRoot = p == null? null: (Uri)p.Value;
		string fileRoot = docRoot == null? "<no doc root>": docRoot.LocalPath;

		int streamCount = 0;
		Console.WriteLine("T0: looking through all nodes for attached streams, root in {0}", fileRoot);
		foreach (Node node in c)
		{
			AssertEquals(node.IsStream, false);
			ICSList streams = node.GetStreamList();
			streamCount = 0;
			string relPath = null;
			foreach (NodeStream ns in streams)
			{
				streamCount++;
				if (relPath == null)
					relPath = ns.RelativePath;
			}
			Console.WriteLine("node {0} has {1} streams, path '{2}'", node.Name, streamCount, relPath);
		}

//		store.ImpersonateUser(Access.StoreAdminRole, null);
		store.Delete();
		store = null;
		
		Thread.Sleep(100);
		GC.Collect();
		Thread.Sleep(200);
	}
#endif
*/

#if true
	public void T1() {}
#else
	[Test]
	public void T1()
	{
		Synker syS = new Synker();
		Uri locS = new Uri(Path.Combine(Directory.GetCurrentDirectory(),
				storeLocS));
		Console.WriteLine("Calling Synker.Start({0})", storeLocS);
		syS.Start(new StatusUpdate(StatusUpdateS), locS, host, portS, 2);

		Thread.Sleep(100);

		Synker syC = new Synker();
		Uri locC = new Uri(Path.Combine(Directory.GetCurrentDirectory(),
				storeLocC));
		Console.WriteLine("Calling Synker.Start({0})", storeLocC);
		syC.Start(new StatusUpdate(StatusUpdateC), locC, host, portC, 3);

		Thread.Sleep(6000);

		//TODO: check that the contents of one directory is as expected
		//TODO: check that the contents of both client directories match the server

		//TODO: delete file 1 from client, delete file 2 from server, modify file 3
		//TODO: check that the contents of one directory is as expected
		//TODO: check that the contents of both client directories match the server

		Thread.Sleep(6000);
		

		Console.WriteLine("Calling Synker.stop()");
		syS.Stop();
	}
#endif

	[TestFixtureTearDown]
	public void Cleanup()
	{
		Console.WriteLine("Deleting stores");
		
		storeC.ImpersonateUser(Access.StoreAdminRole, null);
		storeC.Delete();
		storeC = null;
		
		storeS.ImpersonateUser(Access.StoreAdminRole, null);
		storeS.Delete();
		storeS = null;
		
		Thread.Sleep(100);
		GC.Collect();
		Thread.Sleep(200);
	}

	static void Main()
	{
		Tests t = new Tests();
		t.Init();
		t.T0();
		t.T1();
		t.Cleanup();
	}
}

//===========================================================================
}
