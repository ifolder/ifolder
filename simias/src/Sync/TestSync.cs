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
 *  Author: Dale Olds <olds@novell.com>
 *
 ***********************************************************************/
using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Diagnostics;
using NUnit.Framework;
using Simias;
using Simias.Storage;
using Simias.Sync;

namespace Simias.Sync.Tests
{

//---------------------------------------------------------------------------
/// <summary>
/// Sync Tests
/// </summary>
[TestFixture]
public class SyncTests: Assertion
{
	const int serverPort = 1100;
	const string folderName = "testFolder";
	const string monoCmd = "/usr/bin/mono";
	const string monoSyncCmd = "--debug SyncCmd.exe";
	static readonly string storeDirA = Path.GetFullPath("SyncTestDataA");
	static readonly string storeDirB = Path.GetFullPath("SyncTestDataB");
	static readonly string folderB = Path.Combine(storeDirB, folderName);
	static readonly string folderA = Path.Combine(storeDirA, folderName);

	string host = "127.0.0.1";
	string invitationFile = Path.Combine(storeDirB, "SyncTestInvitation.ifi");
	bool runChildProcess = true;
	bool useRemoteServer = false;

	static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncTests));

	//---------------------------------------------------------------------------
	static int Run(string program, string args)
	{
		ProcessStartInfo psi = new ProcessStartInfo(program, args);
		psi.UseShellExecute = false;
		Process p = Process.Start(psi);
		p.WaitForExit();
		int exitCode = p.ExitCode;
		p.Close();
		return exitCode;
	}

	//---------------------------------------------------------------------------
	bool RunClient()
	{
		// run client code within this process
		if (!runChildProcess)
			return CmdClient.RunOnce(new Uri(storeDirB), new Uri(folderB), storeDirA);

		if (useRemoteServer)
			return CmdClient.RunOnce(new Uri(storeDirB), new Uri(folderB), null)
					&& CmdClient.RunOnce(new Uri(storeDirA), new Uri(folderA), null);
		
		// running local child client process
		CmdServer cmdServer = new CmdServer(host, serverPort, new Uri(storeDirA));
		string syncCmdLine = String.Format(" -s {0} sync {1}", storeDirB, folderB);

		//TODO: very gross check to determine if we are on mono, find a better way
		int err = Path.DirectorySeparatorChar == '/'?
				Run(monoCmd, monoSyncCmd + syncCmdLine):
				Run("SyncCmd.exe", syncCmdLine);

		if (err != 0)
			Console.WriteLine("child process execution failed, error {0}", err);

		cmdServer.Stop();
		cmdServer = null;
		GC.Collect(); //TODO: is this necessary?
		return err == 0;
	}

	//---------------------------------------------------------------------------
	void DeleteFileData()
	{
		Directory.Delete(storeDirA, true);
		Directory.Delete(storeDirB, true);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Performs pre-initialization tasks.
	/// </summary>
	[TestFixtureSetUp]
	public void Init()
	{
		try
		{
			Trace.Listeners.Add(new TextWriterTraceListener(System.Console.Out));

			// set up server store and collections, and some ifolder file data
			if (Directory.Exists(storeDirA) || Directory.Exists(storeDirB)
					|| File.Exists(storeDirA) || File.Exists(storeDirB))
			{
				//throw new ApplicationException(String.Format("can't run tests: {0} or {1} already exist, remove and retry", storeDirA, storeDirB));
				DeleteFileData();
			}

			Directory.CreateDirectory(folderA);
			Directory.CreateDirectory(folderB);
			log.Debug("Init: created store, folders and files");
		}
		catch (System.Exception e)
		{
			Console.WriteLine("Uncaught exception in SyncTests.Init: {0}", e.Message);
			throw;
		}
		catch
		{
			Console.WriteLine("Foreign exception in SyncTests.Init");
			throw;
		}
	}

	//---------------------------------------------------------------------------
	[Test] public void NUInvite() { Assert(Invite()); }

	public bool Invite()
	{
		if (useRemoteServer)
		{
			log.Info("Skipping creation of local invitation, must come from remote server");
			return true;
		}
		log.Debug("Creating collection and invitation");
		FileInviter fi = new FileInviter(new Uri(storeDirA));
		return fi.Invite(null, new Uri(folderA), host, serverPort, invitationFile);
	}

	//---------------------------------------------------------------------------
	[Test] public void NUAccept()  { Assert(Accept()); }

	public bool Accept()
	{
		log.Debug("accepting collection and invitation");
		if (!useRemoteServer)
		{
			FileInviter fi = new FileInviter(new Uri(storeDirB));
			return fi.Accept(storeDirB, invitationFile);
		}

		// using remote server: accept specified invitation file into both local stores.
		FileInviter fiA = new FileInviter(new Uri(storeDirA));
		FileInviter fiB = new FileInviter(new Uri(storeDirB));
		return fiA.Accept(storeDirA, invitationFile) && fiB.Accept(storeDirB, invitationFile);
	}

	//---------------------------------------------------------------------------
	[Test] public void NUFirstSync() { Assert(FirstSync()); }

	public bool FirstSync()
	{
		string dir1 = Path.Combine(folderA, "subdir1");
		string dir2 = Path.Combine(folderA, "sub dir with spaces2");
		string dir3 = Path.Combine(folderA, "subdir3");
		Directory.CreateDirectory(dir1);
		Directory.CreateDirectory(dir2);
		Differ.CreateFile(Path.Combine(dir1, "file1"), "file 1 contents");

		log.Debug("+++++++++++ creating large file");
		Directory.CreateDirectory(dir3);
		Differ.CreateFile(Path.Combine(dir3, "file3"), 800*1024);

		log.Debug("+++++++++++ first run of client for FirstSync");
		if (!RunClient())
		{
			log.Debug("failed first sync");
			return false;
		}
		log.Debug("+++++++++++ second run of client for FirstSync");
		if (!RunClient())
		{
			log.Debug("failed first sync");
			return false;
		}
		return Differ.CompareDirectories(folderA, folderB);
	}

	//---------------------------------------------------------------------------
	[Test] public void NUSizeSync() { Assert(SizeSync()); }

	public bool SizeSync()
	{
		log.Debug("+++++++++++ creating subdir to check syncSize");
		string dir1 = Path.Combine(folderB, "subdirSized");
		Directory.CreateDirectory(dir1);

		int[] sizes = { 113*1024, 5, 42, 888, 255 * 1024 + 5 };
		Differ.CreateFile(Path.Combine(dir1, "sizedFile0"), sizes[0]);
		Differ.CreateFile(Path.Combine(dir1, "sizedFile1"), sizes[1]);
		Differ.CreateFile(Path.Combine(dir1, "sizedFile2"), sizes[2]);
		Differ.CreateFile(Path.Combine(dir1, "sizedFile3"), sizes[3]);
		Differ.CreateFile(Path.Combine(dir1, "sizedFile4"), sizes[4]);
		ulong totalSize = 0;
		foreach (int size in sizes)
			totalSize += (uint)size;

        Store store = new Store(new Configuration(storeDirB));
		store.Revert();
		Collection collection = FileInviter.FindCollection(store, new Uri(folderB));

		uint nodeCount;
		ulong maxBytesToSend;
		SyncSize.CalculateSendSize(collection, out nodeCount, out maxBytesToSend);

		log.Debug("+++++++++++ syncSize: {0} nodes, {1} totalSize", nodeCount, maxBytesToSend);

		return nodeCount == 7
				&& maxBytesToSend > totalSize + 7 * 1000
				&& maxBytesToSend < totalSize + 7 * 2000;
	}

	//---------------------------------------------------------------------------
	// test deletes: delete a file from each end, and cause deletion collision
	// sync and make sure everything got sorted out
	[Test] public void NUSimpleAdds() { Assert(SimpleAdds()); }

	public bool SimpleAdds()
	{
		string dirS = Path.Combine(folderA, "simpleTestDir");
		string dirC = Path.Combine(folderB, "simpleTestDir");
		Directory.CreateDirectory(dirS);
		Directory.CreateDirectory(dirC);
		const int fileCount = 43;

		// add some files
		for (int i = 0; i < fileCount; ++i)
		{
			if ((i & 1) == 0)
				Differ.CreateFile(Path.Combine(dirS, "simple-file-" + i + ".txt"), "even simple file contents" + i + "\n");
			else
				Differ.CreateFile(Path.Combine(dirS, i.ToString() + "-simple-file.txt"), "odd simple file contents" + i + "\n");
		}

		// cause some collisions
		for (int i = 0; i < fileCount; i += 3)
		{
			if ((i & 1) == 0)
				Differ.CreateFile(Path.Combine(dirS, "simple-file-" + i + ".txt"), "even simple file contents" + i + " from collision\n");
			else
				Differ.CreateFile(Path.Combine(dirS, i.ToString() + "-simple-file.txt"), "odd simple file contents" + i + " from collision\n");
		}

		if (!RunClient())
		{
			log.Debug("failed simpleAdds sync");
			return false;
		}
		return Differ.CompareDirectories(folderA, folderB);
	}


	//---------------------------------------------------------------------------
	// test deletes: delete a file from each end, and cause deletion collision
	// sync and make sure everything got sorted out
	[Test] public void NUSimpleDeletes() { Assert(SimpleDeletes()); }

	public bool SimpleDeletes()
	{
		string dirS = Path.Combine(folderA, "simpleTestDir");
		string dirC = Path.Combine(folderB, "simpleTestDir");
		int[] delNums = { 8, 12, 14, 16, 22, 28 };

		// delete all but last two from client
		for (int i = 0; i < delNums.Length - 2; ++i)
			File.Delete(Path.Combine(dirC, "simple-file-" + delNums[i] + ".txt"));

		// delete last four from server (middle two deleted on both client and server)
		for (int i = delNums.Length - 4; i < delNums.Length; ++i)
			File.Delete(Path.Combine(dirS, "simple-file-" + delNums[i] + ".txt"));

		if (!RunClient())
		{
			log.Debug("failed simple deletes sync");
			return false;
		}

		bool worked = true;
		string fname;
		for (int i = 0; i < delNums.Length; ++i)
			if (File.Exists(fname = Path.Combine(dirS, "simple-file-" + delNums[i] + ".txt")))
			{
				log.Debug("deleted file still exists on server after simple deletes: {0}", fname);
				worked = false;
			}

		for (int i = 0; i < delNums.Length; ++i)
			if (File.Exists(fname = Path.Combine(dirC, "simple-file-" + delNums[i] + ".txt")))
			{
				log.Debug("deleted file still exists on client after simple deletes: {0}", fname);
				worked = false;
			}

		return worked && Differ.CompareDirectories(folderA, folderB);
	}

	//---------------------------------------------------------------------------
	// test file creation collisions: create duplicate and non-dup files in multiple
	//     level directory structure
	// sync and make sure everything got sorted out
	[Test] public void NUFileCreationCollision() { Assert(FileCreationCollision()); }

	public bool FileCreationCollision()
	{
		string dirS = Path.Combine(folderA, "simpleTestDir");
		string dirC = Path.Combine(folderB, "simpleTestDir");
		string subdirS1 = Path.Combine(dirS, "collisionDir1");
		string subdirS2 = Path.Combine(dirS, "collisionDir2");
		string subdirC1 = Path.Combine(dirC, "collisionDir1");
		string subdirC2 = Path.Combine(dirC, "collisionDir2");

		Directory.CreateDirectory(subdirS1);
		Directory.CreateDirectory(subdirS2);
		Directory.CreateDirectory(subdirC1);
		Directory.CreateDirectory(subdirC2);

		Differ.CreateFile(Path.Combine(subdirS1, "collision-file-1.txt"), "server collision file contents 1\n");
		Differ.CreateFile(Path.Combine(subdirS2, "collision-file-2.txt"), "server collision file contents 2\n");
		Differ.CreateFile(Path.Combine(subdirC1, "collision-file-1.txt"), "client collision file contents 1\n");
		Differ.CreateFile(Path.Combine(subdirC2, "collision-file-2.txt"), "client collision file contents 2\n");

		Differ.CreateFile(Path.Combine(subdirS1, "non-collision-file-1.txt"), "server non-collision file contents 1\n");
		Differ.CreateFile(Path.Combine(subdirS2, "non-collision-file-2.txt"), "server non-collision file contents 2\n");
		Differ.CreateFile(Path.Combine(subdirC1, "non-collision-file-3.txt"), "client non-collision file contents 3\n");
		Differ.CreateFile(Path.Combine(subdirC2, "non-collision-file-4.txt"), "client non-collision file contents 4\n");
		
		log.Debug("+++++++++++ first run of client for FileCreationCollision");
		if (!RunClient())
		{
			log.Debug("failed sync after FileCreationCollision");
			return false;
		}
		log.Debug("+++++++++++ second run of client for FileCreationCollision");
		if (!RunClient())
		{
			log.Debug("failed sync after FileCreationCollision");
			return false;
		}
		log.Debug("+++++++++++ third run of client for FileCreationCollision");
		if (!RunClient())
		{
			log.Debug("failed sync after FileCreationCollision");
			return false;
		}
		log.Debug("+++++++++++ fourth run of client for FileCreationCollision");
		if (!RunClient())
		{
			log.Debug("failed sync after FileCreationCollision");
			return false;
		}

		return Differ.CompareDirectories(folderA, folderB);
	}

	//---------------------------------------------------------------------------
	// create many files in deep subdirs, sync and make sure everything got sorted out

	[Test] public void NUDeepSubDirs() { Assert(DeepSubDirs()); }

	public bool DeepSubDirs()
	{
		const int maxDepth = 15;
		string curDir = folderA;
		for (int i = 0; i < maxDepth; i++)
		{
			log.Debug("creating DeepSubDirs, depth {0}", i);
			curDir = Path.Combine(curDir, String.Format("subDepth{0}", i));
			Directory.CreateDirectory(curDir);
			Differ.CreateFile(Path.Combine(curDir, String.Format("depth{0}-file-1.txt", i)), "small depth file contents 1\n");
			Differ.CreateFile(Path.Combine(curDir, String.Format("depth{0}-file-2.txt", i)), 199*1024);
			Differ.CreateFile(Path.Combine(curDir, String.Format("depth{0}-file-3.txt", i)), 35*1024);
			Differ.CreateFile(Path.Combine(curDir, String.Format("depth{0}-file-4.txt", i)), "very small depth file contents 4\n");
			for (int j = 1; j <= 50; j++)
				Differ.CreateFile(Path.Combine(curDir, String.Format("depth{0}-blast{1}.txt", i, j)), j);
		}

		for (int i = 0; i < 5; ++i)
		{
			log.Debug("+++++++++++ run {0} of client for DeepSubDirs", i);
			if (!RunClient())
			{
				log.Debug("failed sync {0} of DeepSubDirs", i);
				return false;
			}
		}
		return Differ.CompareDirectories(folderA, folderB);
	}

	//---------------------------------------------------------------------------
	[TestFixtureTearDown]
	public void Cleanup()
	{
		//DeleteFileData();
	}

	//---------------------------------------------------------------------------
	int successCount = 0, failedCount = 0;
	void AccountTest(string name, bool succeeded)
	{
		if (succeeded)
			successCount++;
		else
			failedCount++;
		Console.WriteLine("{0}: {1}", name, succeeded);
	}

	//---------------------------------------------------------------------------
	void Run()
	{
		Console.WriteLine("init");
		Init();
		AccountTest("invite", Invite());
		AccountTest("accept", Accept());
		AccountTest("firstSync", FirstSync());
		//AccountTest("syncSize", SizeSync());
		//AccountTest("simpleAdds", SimpleAdds());
		//AccountTest("simpleDeletes", SimpleDeletes());
		//AccountTest("FileCreationCollision", FileCreationCollision());
		//AccountTest("DeepSubDirs", DeepSubDirs());
		Cleanup();
		Console.WriteLine("{0} tests succeeded, {1} failed", successCount, failedCount);
	}

	//---------------------------------------------------------------------------
	public static int Main(string[] args)
	{
		int err = 0;
		try
		{
			if (args.Length > 2)
			{
				Console.WriteLine("unknown usage, too many command line args");
				err = 1;
			}
			else
			{
				SyncTests t = new SyncTests();
 				if (args.Length > 0)
					t.invitationFile = Path.GetFullPath(args[0]);
				t.Run();
			}
		}
		catch (Exception e)
		{
			Console.WriteLine("Uncaught exception in TestSync.Main: {0}", e.Message);
			Console.WriteLine(e.StackTrace);
			err = 99;
		}
		catch
		{
			Console.WriteLine("Uncaught foreign exception in TestSync.Main");
			err = 98;
		}
		return err;
	}
}

//===========================================================================
}
