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
	const string scpCmd = "/usr/bin/scp";
	const string sshCmd = "/usr/bin/ssh";
	const string monoCmd = "/usr/bin/mono";
	const bool useTCP = true;
	static readonly string serverDir = Path.GetFullPath("SyncTestServerData");
	static readonly string clientDir = Path.GetFullPath("SyncTestClientData");
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
		int err = Run(sshCmd, String.Format("{0} {1}", clientAddress, cmdLine));
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
		int err = Run(scpCmd, String.Format(toRemoteHost?
				"-p -r {1}/{2} {0}:{1}": "-p -r {0}:{1}/{2} {1}",
				clientAddress, SSHPath(path), SSHPath(target)));
		if (err != 0)
			Console.WriteLine("remote file copy failed, error {0}", err);
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
		string syncCmdLine = String.Format("-s {0} {1} sync {2}", clientDir, useTCP? "": "-h", clientFolder);
		bool ok = false;

		if (clientAddress == null)
		{
			// run client code as local child process
			int err;

			//TODO: very gross check to determine if we are on mono, but what to do?
			if (Path.DirectorySeparatorChar == '/')
				err = Run(monoCmd, "--debug SyncCmd.exe " + syncCmdLine);
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
			ok = RemoteCopy(clientDir, folderName, true)
					&& RemoteRun(String.Format("{0} --debug SyncCmd.exe {1}", monoCmd, syncCmdLine))
					&& RemoteCopy(clientDir, folderName, false);
		}

		cmdServer.Stop();
		return ok;
	}

	//---------------------------------------------------------------------------
	void DeleteFileData()
	{
		Directory.Delete(serverDir, true);
		Directory.Delete(clientDir, true);
		File.Delete(invitationFile);
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
			//Log.SetLevel("verbose");

			// set up server store and collections, and some ifolder file data
			if (Directory.Exists(serverDir) || Directory.Exists(clientDir)
					|| File.Exists(serverDir) || File.Exists(clientDir))
			{
				//throw new ApplicationException(String.Format("can't run tests: {0} or {1} already exist, remove and retry", serverDir, clientDir));
				DeleteFileData();
			}

			Directory.CreateDirectory(clientDir);
			Directory.CreateDirectory(serverFolder);
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
		return fi.Invite(null, new Uri(serverFolder), host, serverPort, invitationFile);
	}

	//---------------------------------------------------------------------------
	[Test] public void NUAccept()  { Assert(Accept()); }

	public bool Accept()
	{
		Log.Spew("accepting collection and invitation");
		if (clientAddress == null)
		{
			FileInviter fi = new FileInviter(new Uri(clientDir));
			return fi.Accept(clientDir, invitationFile);
		}

		/* run client code on remote machine. requires ssh client here and
		 * ssh server on remote machine -- with authorized keys already set up.
		 * Since SSH servers don't run on windows, assume the remote client
		 * will be on mono.
		 */
		if (!RemoteCopy(clientDir, invitationFile, true))
			return false;

		string cmdLine = String.Format("{0} --debug SyncCmd.exe -s {1} {2} accept {1}/{3} {1}",
				monoCmd, clientDir, useTCP? "": "-h", clientDir, invitationFile);

		return RemoteRun(cmdLine);
	}

	//---------------------------------------------------------------------------
	[Test] public void NUFirstLocalSync() { Assert(FirstLocalSync()); }

	public bool FirstLocalSync()
	{
		string dir1 = Path.Combine(serverFolder, "subdir1");
		string dir2 = Path.Combine(serverFolder, "sub dir with spaces");
		Directory.CreateDirectory(dir1);
		Directory.CreateDirectory(dir2);
		Differ.CreateFile(Path.Combine(dir1, "file1"), "file 1 contents");
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

		for (int i = 0; i < delNums.Length - 2; ++i)
			File.Delete(Path.Combine(dirC, "simple-file-" + delNums[i] + ".txt"));

		for (int i = delNums.Length - 4; i < delNums.Length; ++i)
			File.Delete(Path.Combine(dirS, "simple-file-" + delNums[i] + ".txt"));

		if (!RunClient())
		{
			Log.Spew("failed simple deletes sync");
			return false;
		}

		for (int i = 0; i < delNums.Length; ++i)
			if (File.Exists(Path.Combine(dirS, "simple-file-" + delNums[i] + ".txt")))
			{
				Log.Spew("deleted file still exists on server after simple deletes");
				return false;
			}

		for (int i = 0; i < delNums.Length; ++i)
			if (File.Exists(Path.Combine(dirC, "simple-file-" + delNums[i] + ".txt")))
			{
				Log.Spew("deleted file still exists on client after simple deletes");
				return false;
			}

		return Differ.CompareDirectories(serverFolder, clientFolder);
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
	void Run()
	{
		Console.WriteLine("init");
		Init();
		Console.WriteLine("invite: {0}", Invite());
		Console.WriteLine("accept: {0}", Accept());
		Console.WriteLine("firstLocalSync: {0}", FirstLocalSync());
		Console.WriteLine("simpleAdds: {0}", SimpleAdds());
		Console.WriteLine("simpleDeletes: {0}", SimpleDeletes());
		Console.WriteLine("FileCreationCollision: {0}", FileCreationCollision());
		Cleanup();
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
