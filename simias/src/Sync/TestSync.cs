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

/// <summary>
/// Sync Tests
/// </summary>
[TestFixture]
public class SyncTests: Assertion
{
	private const string host = "127.0.0.1";
	private const int serverPort = 1100;
	private const string folderName = "testFolder";
	private const string invitationFile = "SyncTestInvitation.ifi";
	private const bool useTCP = true;
	private static readonly string serverDir = Path.GetFullPath("SyncTestServerData");
	private static readonly string clientDir = Path.GetFullPath("SyncTestClientData");
	private static readonly string clientFolder = Path.Combine(clientDir, folderName);
	private static readonly string serverFolder = Path.Combine(serverDir, folderName);

	//private CmdServer cmdServer = null;

	public static int Run(string program, string args)
	{
		ProcessStartInfo psi = new ProcessStartInfo(program, args);
		psi.UseShellExecute = false;
		Process p = Process.Start(psi);
		p.WaitForExit();
		int exitCode = p.ExitCode;
		p.Close();
		return exitCode;
	}

	private void DeleteFileData()
	{
		Directory.Delete(serverDir, true);
		Directory.Delete(clientDir, true);
		File.Delete(invitationFile);
	}

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

			string dir1 = Path.Combine(serverFolder, "subdir1");
			string dir2 = Path.Combine(serverFolder, "sub dir with spaces");
			Directory.CreateDirectory(clientDir);
			Directory.CreateDirectory(dir1);
			Directory.CreateDirectory(dir2);
			Differ.CreateFile(Path.Combine(dir1, "file1"), "file 1 contents");
			//cmdServer = new CmdServer(host, serverPort, new Uri(serverDir), useTCP);
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

	public bool Invite()
	{
		Log.Spew("Creating collection and invitation");
		FileInviter fi = new FileInviter(new Uri(serverDir));
		return fi.Invite(null, new Uri(serverFolder), host, serverPort, invitationFile);
	}

	[Test] public void T0() { Assert(Invite()); }

	public bool Accept()
	{
		Log.Spew("accepting collection and invitation");
		FileInviter fi = new FileInviter(new Uri(clientDir));
		return fi.Accept(clientDir, invitationFile);
	}

	[Test] public void T1()  { Assert(Accept()); }

	public bool FirstLocalSync()
	{
		if (!CmdClient.RunOnce(new Uri(clientDir), new Uri(clientFolder), serverDir, useTCP))
		{
			Log.Spew("failed first sync");
			return false;
		}
		return Differ.CompareDirectories(serverFolder, clientFolder);
	}

	[Test] public void T2() { Assert(FirstLocalSync()); }

//   
//   #---------------------------------------------------------------------------
//   # test deletes: delete a file from each end, and cause deletion collision
//   # sync and make sure everything got sorted out
//   
//   rm -f tmpClientData/testFolder/SyncPoint.cs
//   rm -f tmpServerData/testFolder/SyncPass.cs
//   rm -f tmpServerData/testFolder/SyncOps.cs
//   rm -f tmpClientData/testFolder/SyncOps.cs
//   #rm -f tmpClientData/testFolder/subdir/*.ogg
//   rm -f tmpServerData/testFolder/subdir/Sync.dll
//   rmdir tmpServerData/testFolder/emptydir
//   
//   #localsync
//   mono --debug SyncCmd.exe -s tmpClientData localsync tmpClientData/testFolder tmpServerData
//   RETVAL=$?
//   if [ $RETVAL -ne 0 ]; then printf "FAILED: localsync 2 returned %s\n" $RETVAL; exit $RETVAL; fi
//   
//   diff tmpClientData/testFolder tmpServerData/testFolder
//   RETVAL=$?
//   if [ $RETVAL -ne 0 ]; then printf "FAILED: diff of testFolder after localsync 2 returned %s\n" $RETVAL; exit $RETVAL; fi 
//   
//   diff tmpClientData/testFolder/subdir tmpServerData/testFolder/subdir
//   RETVAL=$?
//   if [ $RETVAL -ne 0 ]; then printf "FAILED: diff of subdir after localsync 2 returned %s\n" $RETVAL; exit $RETVAL; fi 
//   
//   if [ -f tmpClientData/testFolder/SyncPoint.cs ]; then printf "FAILED: deleted file still exists\n"; exit 1; fi 
//   if [ -f tmpClientData/testFolder/SyncPass.cs ]; then printf "FAILED: deleted file still exists\n"; exit 2; fi 
//   if [ -f tmpClientData/testFolder/SyncOps.cs ]; then printf "FAILED: deleted file still exists\n"; exit 3; fi 
//   #if [ -f tmpClientData/testFolder/subdir/*.ogg ]; then printf "FAILED: deleted file still exists\n"; exit 4; fi 
//   if [ -f tmpClientData/testFolder/subdir/Sync.dll ]; then printf "FAILED: deleted file still exists\n"; exit 5; fi 
//   if [ -d tmpClientData/testFolder/emptydir ]; then printf "FAILED: deleted file still exists\n"; exit 6; fi
//   
//   #---------------------------------------------------------------------------
//   # test duplicate adds: add different files of the same name both sides
//   # sync and make sure everything got sorted out
//   
//   echo "This is file 1" > tmpServerData/testFolder/CollisionFile.txt
//   echo "This is file 2 and is longer" > tmpClientData/testFolder/CollisionFile.txt
//   
//   # localsync
//   mono --debug SyncCmd.exe -s tmpClientData localsync tmpClientData/testFolder tmpServerData
//   RETVAL=$?
//   if [ $RETVAL -ne 0 ]; then printf "FAILED: localsync 3 returned %s\n" $RETVAL; exit $RETVAL; fi
//   
//   # localsync
//   mono --debug SyncCmd.exe -s tmpClientData localsync tmpClientData/testFolder tmpServerData
//   RETVAL=$?
//   if [ $RETVAL -ne 0 ]; then printf "FAILED: localsync 3 returned %s\n" $RETVAL; exit $RETVAL; fi
//   
//   # localsync
//   mono --debug SyncCmd.exe -s tmpClientData localsync tmpClientData/testFolder tmpServerData
//   RETVAL=$?
//   if [ $RETVAL -ne 0 ]; then printf "FAILED: localsync 3 returned %s\n" $RETVAL; exit $RETVAL; fi
//   
//   diff tmpClientData/testFolder tmpServerData/testFolder
//   RETVAL=$?
//   if [ $RETVAL -ne 0 ]; then printf "FAILED: diff of testFolder after localsync 3 returned %s\n" $RETVAL; exit $RETVAL; fi 
//   
//   if ! [ -f tmpClientData/testFolder/CollisionFile.txt ]; then printf "FAILED: added file does not exists\n"; exit 1; fi
//   
//   cat tmpServerData/testFolder/CollisionFile.txt
//   

	[TestFixtureTearDown]
	public void Cleanup()
	{
		//cmdServer.Stop();
		//DeleteFileData();
	}

	void Run()
	{
		Console.WriteLine("init");
		Init();
		Console.WriteLine("invite: {0}", Invite());
		Console.WriteLine("accept: {0}", Accept());
		Console.WriteLine("firstLocalSync: {0}", FirstLocalSync());
		Cleanup();
	}

	static void Main()
	{
		SyncTests t = new SyncTests();
		t.Run();
	}
}

//===========================================================================
}
