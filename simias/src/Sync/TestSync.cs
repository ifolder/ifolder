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
class FileTestOps
{
	/// <summary>
	/// Compare two files, specific differences are printed to Console.
	/// returns false if comparison fails.
	/// </summary>
	// what about uid/gid/permissions?
	public static bool CompareFiles(string fileA, string fileB)
	{
		bool same = true;

		FileInfo fiA = new FileInfo(fileA), fiB = new FileInfo(fileB);

		if (fiA.Attributes != fiB.Attributes)
		{
			Console.WriteLine("{0} attributes {1} != {2} attributes {3}", fileA, fiA.Attributes, fileB, fiB.Attributes);
			same = false;
		}
		if (fiA.CreationTimeUtc != fiB.CreationTimeUtc)
		{
			Console.WriteLine("{0} creation time {1} != {2} creation time {3}", fileA, fiA.CreationTimeUtc, fileB, fiB.CreationTimeUtc);
			same = false;
		}
		if (fiA.LastAccessTimeUtc != fiB.LastAccessTimeUtc)
		{
			Console.WriteLine("{0} access time {1} != {2} access time {3}", fileA, fiA.LastAccessTimeUtc, fileB, fiB.LastAccessTimeUtc);
			same = false;
		}
		if (fiA.LastWriteTimeUtc != fiB.LastWriteTimeUtc)
		{
			Console.WriteLine("{0} write time {1} != {2} write time {3}", fileA, fiA.LastWriteTimeUtc, fileB, fiB.LastWriteTimeUtc);
			same = false;
		}

		FileStream fsA = fiA.OpenRead(), fsB = fiB.OpenRead();
		byte[] bufferA = new byte[64 * 1024];
		byte[] bufferB = new byte[bufferA.Length];
		int readA, readB;
		do
		{
			readA = fsA.Read(bufferA, 0, bufferA.Length);
			readB = fsB.Read(bufferB, 0, bufferB.Length);

			/* I would like to find a better way to compare two byte arrays,
			 * for now just compare the whole array every time. This is a waste
			 * for the last read when the array is not full, but it should work OK
			 * since the bytes from init or previous read should compare.
			 */
			if (readA != readB || bufferA != bufferB)
			{
				Console.WriteLine("{0} contents != {2} contents", fileA, fileB);
				same = false;
				break;
			}
		} while (readA != 0);
		fsA.Close();
		fsB.Close();
		return same;
	}

	/// <summary>
	/// Compare two files, specific differences are printed to Console.
	/// returns false if comparison fails.
	/// </summary>
	// what about uid/gid/permissions?
	public static bool CompareDirectories(string dirA, string dirB)
	{
		// Console.WriteLine( differences in files and subdirs);
		bool same = true;

		string[] filesA = Directory.GetFiles(dirA);
		string[] filesB = Directory.GetFiles(dirB);
		if (filesA.Length != filesB.Length)
		{
			Console.WriteLine("{0} contains {1} files != {2} contains {3} files", dirA, filesA.Length, dirB, filesB.Length);
			same = false;
		}
		else
		{
			Array.Sort(filesA);
			Array.Sort(filesB);
			for (uint i = 0; i < filesA.Length; ++i)
			{
				if (!CompareFiles(filesA[i], filesB[i]))
					same = false;
			}
		}

		string[] dirsA = Directory.GetDirectories(dirA);
		string[] dirsB = Directory.GetDirectories(dirB);
		if (dirsA.Length != dirsB.Length)
		{
			Console.WriteLine("{0} contains {1} subdirs != {2} contains {3} subdirs", dirA, dirsA.Length, dirB, dirsB.Length);
			same = false;
		}
		else
		{
			Array.Sort(dirsA);
			Array.Sort(dirsB);
			for (uint i = 0; i < dirsA.Length; ++i)
			{
				//TODO: compare dir names here
				if (!CompareDirectories(dirsA[i], dirsB[i]))
					same = false;
			}
		}
		return same;
	}

	public static void CreateFile(string name, string contents)
	{
		StreamWriter s = new StreamWriter(File.Create(name));
		s.Write(contents);
		s.Close();
	}

	public static void CreateFile(string name, byte[] contents)
	{
		BinaryWriter s = new BinaryWriter(File.Create(name));
		s.Write(contents);
		s.Close();
	}

	public static void CreateFile(string name, uint size)
	{
		// Console.WriteLine( differences in contents and attributes);
	}
}

//---------------------------------------------------------------------------
/// <summary>
/// Sync Tests
/// </summary>
[TestFixture]
public class Tests : Assertion
{
	private const string host = "127.0.0.1";
	private const int serverPort = 1100;
	private const string srvDir = "SyncTestServerData";
	private const string cltDir = "SyncTestClientData";

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

	// initialization helper methods
	//private static Store InitStore(string name)
	//{
	//	//Console.WriteLine("connecting to store {0}", name);
	//	Uri loc = new Uri(Path.Combine(Directory.GetCurrentDirectory(), name));
	//	Store tmp = Store.Connect(loc);
	//	return tmp;
	//}

	/// <summary>
	/// Performs pre-initialization tasks.
	/// </summary>
	[TestFixtureSetUp]
	public void Init()
	{
		try
		{
			// set up server store and collections, and some ifolder file data
			/*


			if exists srvDir or cltDir
				abort, previous not cleaned up

			mkdir srvDir cltDir srvDir/testFolder
			mkdir srvDir/testFolder/emptydir
			mkdir srvDir/testFolder/subdir

			TestOps.CreateFile(srvDir/testFolder, "small file 1");
			TestOps.CreateFile(srvDir/testFolder, "small file 2");
			TestOps.CreateFile(srvDir/testFolder, "small file 3");
			TestOps.CreateFile(srvDir/testFolder, 1024 * 1024);

			open server

			*/
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

	[Test]
	public void T0()
	{
	}

	[Test]
	public void T1()
	{
	}

	[TestFixtureTearDown]
	public void Cleanup()
	{
		Console.WriteLine("Deleting stores");
		Directory.Delete(srvDir, true);
		Directory.Delete(cltDir, true);
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
