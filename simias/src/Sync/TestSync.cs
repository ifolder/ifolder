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
	const string invitationFile = "SyncTestInvitation.ifi";
	const string scpCmd = "scp.exe";
	const string sshCmd = "ssh.exe";
	const string monoCmd = "/usr/bin/mono";
	const string monoSyncCmd = "--debug SyncCmd.exe";
	const bool useTCP = true;
	const string relClientDir = "SyncTestClientData";
	const string remoteBinDir = "$IFOLDER_BIN";
	static readonly string serverDir = Path.GetFullPath("SyncTestServerData");
	static readonly string clientDir = Path.GetFullPath(relClientDir);
	static readonly string clientFolder = Path.Combine(clientDir, folderName);
	static readonly string serverFolder = Path.Combine(serverDir, folderName);

	bool runChildProcess = true;
	string clientAddress = null; // for use in SSH commandlines, can be user@hostname, raw IP, etc.
	string host = "127.0.0.1"; // must be set to name or external address of this machine if clientAddress is set

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
	bool RemoteRun(string cmdLine)
	{
		Assert(clientAddress != null);
		int err = Run(sshCmd, String.Format("{0} 'cd {1}; {2}'",
				clientAddress, remoteBinDir, cmdLine));
		if (err != 0)
			Console.WriteLine("remote command execution failed, error {0}", err);
		return err == 0;
	}

	//---------------------------------------------------------------------------
	static string SSHPath(string localPath)
	{
		localPath = localPath.Replace(Path.DirectorySeparatorChar, '/');
		return localPath.Replace(Path.AltDirectorySeparatorChar, '/');
	}

	//---------------------------------------------------------------------------
	bool RemoteCopy(string path, string target, bool toRemoteHost)
	{
		Assert(clientAddress != null);
		string opts = String.Format(toRemoteHost? "-p -r {1}/{2} {0}:{3}/{1}": "-p -r {0}:{3}/{1}/{2} {1}",
				clientAddress, SSHPath(path), SSHPath(target), remoteBinDir);
		int err = Run(scpCmd, opts);
		if (err != 0)
			Console.WriteLine("'{0} {1}' failed, error {2}", scpCmd, opts, err);
		return err == 0;
	}

	//---------------------------------------------------------------------------
	bool RunClient()
	{
		// run client code within this process
		if (!runChildProcess)
			return CmdClient.RunOnce(new Uri(clientDir), new Uri(clientFolder), serverDir, useTCP);

		// bring up a server object within this process
		CmdServer cmdServer = new CmdServer(host, serverPort, new Uri(serverDir), useTCP);
		bool ok = false;

		if (clientAddress == null)
		{
			// run client code as local child process
			int err;
			string syncCmdLine = String.Format(" -s {0} {1} sync {2}", clientDir, useTCP? "": "-h", clientFolder);

			//TODO: very gross check to determine if we are on mono, but what to do?
			if (Path.DirectorySeparatorChar == '/')
				err = Run(monoCmd, monoSyncCmd + syncCmdLine);
			else
				err = Run("SyncCmd.exe", syncCmdLine);
			if (err != 0)
				Console.WriteLine("child process execution failed, error {0}", err);
			ok = err == 0;
		}
		else
		{
			/* run client code on remote machine. requires ssh client here and
			 * ssh server on remote machine -- with authorized keys already set up.
			 * Since SSH servers don't run on windows, assume the remote client
			 * will be on mono.
			 */
			ok = RemoteCopy(relClientDir, folderName, true)
					&& RemoteRun(String.Format("{0} {4} -s {1} {2} sync {1}/{3}",
							monoCmd, relClientDir, useTCP? "": "-h", folderName, monoSyncCmd))
					&& RemoteCopy(relClientDir, folderName, false);
		}

		cmdServer.Stop();
		cmdServer = null;
		GC.Collect();
		return ok;
	}

	//---------------------------------------------------------------------------
	void DeleteFileData()
	{
		Directory.Delete(serverDir, true);
		Directory.Delete(clientDir, true);
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
			Log.SetLevel("verbose");

			// set up server store and collections, and some ifolder file data
			if (Directory.Exists(serverDir) || Directory.Exists(clientDir)
					|| File.Exists(serverDir) || File.Exists(clientDir))
			{
				//throw new ApplicationException(String.Format("can't run tests: {0} or {1} already exist, remove and retry", serverDir, clientDir));
				DeleteFileData();
			}

			Directory.CreateDirectory(serverFolder);
			Directory.CreateDirectory(clientFolder);
			Log.Spew("Init: created store, folders and files");
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
		Log.Spew("Creating collection and invitation");
		FileInviter fi = new FileInviter(new Uri(serverDir));
		return fi.Invite(null, new Uri(serverFolder), host, serverPort, Path.Combine(clientDir, invitationFile));
	}

	//---------------------------------------------------------------------------
	[Test] public void NUAccept()  { Assert(Accept()); }

	public bool Accept()
	{
		Log.Spew("accepting collection and invitation");
		if (clientAddress == null)
		{
			FileInviter fi = new FileInviter(new Uri(clientDir));
			return fi.Accept(clientDir, Path.Combine(clientDir, invitationFile));
		}

		/* run client code on remote machine. requires ssh client here and
		 * ssh server on remote machine -- with authorized keys already set up.
		 * Since SSH servers don't run on windows, assume the remote client
		 * will be on mono.
		 */
		RemoteRun(String.Format("rm -rf {0}", relClientDir));
		RemoteRun(String.Format("mkdir {0}", relClientDir));

		if (!RemoteCopy(relClientDir, invitationFile, true))
			return false;

		string cmdLine = String.Format("{0} {4} -s {1} {2} accept {1}/{3} {1}",
				monoCmd, relClientDir, useTCP? "": "-h", invitationFile, monoSyncCmd);

		return RemoteRun(cmdLine);
	}

	//---------------------------------------------------------------------------
	[Test] public void NUFirstSync() { Assert(FirstSync()); }

	public bool FirstSync()
	{
		string dir1 = Path.Combine(serverFolder, "subdir1");
		string dir2 = Path.Combine(serverFolder, "sub dir with spaces2");
		string dir3 = Path.Combine(serverFolder, "subdir3");
		Directory.CreateDirectory(dir1);
		Directory.CreateDirectory(dir2);
		Differ.CreateFile(Path.Combine(dir1, "file1"), "file 1 contents");

		Log.Spew("+++++++++++ creating large file");
		Directory.CreateDirectory(dir3);
		Differ.CreateFile(Path.Combine(dir3, "file3"), 800*1024);

		Log.Spew("+++++++++++ first run of client for FirstSync");
		if (!RunClient())
		{
			Log.Spew("failed first sync");
			return false;
		}
		Log.Spew("+++++++++++ second run of client for FirstSync");
		if (!RunClient())
		{
			Log.Spew("failed first sync");
			return false;
		}
		return Differ.CompareDirectories(serverFolder, clientFolder);
	}

	//---------------------------------------------------------------------------
	// test deletes: delete a file from each end, and cause deletion collision
	// sync and make sure everything got sorted out
	[Test] public void NUSimpleAdds() { Assert(SimpleAdds()); }

	public bool SimpleAdds()
	{
		string dirS = Path.Combine(serverFolder, "simpleTestDir");
		string dirC = Path.Combine(clientFolder, "simpleTestDir");
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
			Log.Spew("failed simpleAdds sync");
			return false;
		}
		return Differ.CompareDirectories(serverFolder, clientFolder);
	}


	//---------------------------------------------------------------------------
	// test deletes: delete a file from each end, and cause deletion collision
	// sync and make sure everything got sorted out
	[Test] public void NUSimpleDeletes() { Assert(SimpleDeletes()); }

	public bool SimpleDeletes()
	{
		string dirS = Path.Combine(serverFolder, "simpleTestDir");
		string dirC = Path.Combine(clientFolder, "simpleTestDir");
		int[] delNums = { 8, 12, 14, 16, 22, 28 };

		// delete all but last two from client
		for (int i = 0; i < delNums.Length - 2; ++i)
			File.Delete(Path.Combine(dirC, "simple-file-" + delNums[i] + ".txt"));

		// delete last four from server (middle two deleted on both client and server)
		for (int i = delNums.Length - 4; i < delNums.Length; ++i)
			File.Delete(Path.Combine(dirS, "simple-file-" + delNums[i] + ".txt"));

		if (!RunClient())
		{
			Log.Spew("failed simple deletes sync");
			return false;
		}

		bool worked = true;
		string fname;
		for (int i = 0; i < delNums.Length; ++i)
			if (File.Exists(fname = Path.Combine(dirS, "simple-file-" + delNums[i] + ".txt")))
			{
				Log.Spew("deleted file still exists on server after simple deletes: {0}", fname);
				worked = false;
			}

		for (int i = 0; i < delNums.Length; ++i)
			if (File.Exists(fname = Path.Combine(dirC, "simple-file-" + delNums[i] + ".txt")))
			{
				Log.Spew("deleted file still exists on client after simple deletes: {0}", fname);
				worked = false;
			}

		return worked && Differ.CompareDirectories(serverFolder, clientFolder);
	}

	//---------------------------------------------------------------------------
	// test file creation collisions: create duplicate and non-dup files in multiple
	//     level directory structure
	// sync and make sure everything got sorted out
	[Test] public void NUFileCreationCollision() { Assert(FileCreationCollision()); }

	public bool FileCreationCollision()
	{
		string dirS = Path.Combine(serverFolder, "simpleTestDir");
		string dirC = Path.Combine(clientFolder, "simpleTestDir");
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
		
		Log.Spew("+++++++++++ first run of client for FileCreationCollision");
		if (!RunClient())
		{
			Log.Spew("failed sync after FileCreationCollision");
			return false;
		}
		Log.Spew("+++++++++++ second run of client for FileCreationCollision");
		if (!RunClient())
		{
			Log.Spew("failed sync after FileCreationCollision");
			return false;
		}
		Log.Spew("+++++++++++ third run of client for FileCreationCollision");
		if (!RunClient())
		{
			Log.Spew("failed sync after FileCreationCollision");
			return false;
		}
		Log.Spew("+++++++++++ fourth run of client for FileCreationCollision");
		if (!RunClient())
		{
			Log.Spew("failed sync after FileCreationCollision");
			return false;
		}

		return Differ.CompareDirectories(serverFolder, clientFolder);
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
		AccountTest("simpleAdds", SimpleAdds());
		AccountTest("simpleDeletes", SimpleDeletes());
		AccountTest("FileCreationCollision", FileCreationCollision());
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
				{
					t.clientAddress = args[0];
					t.host = args.Length > 1? args[1]: MyDns.GetHostName();
				}
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
