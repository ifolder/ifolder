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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/


using System;
using System.IO;
using System.Text;

using NUnit.Framework;

using Simias.Storage;
using Simias.Agent;

using Novell.iFolder;

namespace Novell.iFolder.Tests
{

	[TestFixture]
	public class iFolderTests
	{
		private iFolderManager manager;
		private Novell.AddressBook.Manager abMan;
		private Novell.AddressBook.AddressBook ab;
		private string path;
		private string rootPath;
		private string simiasPath;




		/// <summary>
		/// Setup method for all tests
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			rootPath = Directory.GetCurrentDirectory();

			simiasPath = Path.Combine(rootPath, ".simias");

			// Create directory to perform tests in.
			path = Path.Combine(rootPath, "iFolderTest");

			try
			{
			// Celan up any store that may be there before the tests
			Directory.Delete(path, true);
			Directory.Delete(simiasPath, true);
			}
			catch(Exception e)
			{}


			Directory.CreateDirectory(path);

			manager = iFolderManager.Connect(
					new Uri(rootPath));

			abMan = manager.AddressBookManager;

			ab = abMan.OpenDefaultAddressBook();
		}




		/// <summary>
		/// Test to check if a directory can be an ifolder
		/// </summary>
		[Test]
		public void CanBeiFolder()
		{
			string testPath = Path.Combine(path, "canbeifoldertest");

			Console.WriteLine("\nTEST: CanBeiFolder");
			Console.WriteLine("-------------------------------------------");

			string path1 = Path.Combine(testPath, "iftester/stuff/ifolder");
			iFolder newiFolder1 = manager.CreateiFolder(path1);

			string cbPath1 = Path.Combine(testPath, 
					"iftester/stuff/ifolder/myFile.exe");	
			Console.WriteLine("Checking path: " + cbPath1);

			if(manager.CanBeiFolder(cbPath1))	
				throw new Exception("CanBeiFolder failed to recognized file in iFolder.");

			string cbPath2 = Path.Combine(testPath, "iftester/stuff");
			Console.WriteLine("Checking path: " + cbPath2);
			if(manager.CanBeiFolder(cbPath2))	
				throw new Exception("CanBeiFolder failed to recognized path above an iFolder");

			string cbPath3 = Path.Combine(testPath, "iftesterdude");
			Console.WriteLine("Checking path: " + cbPath3);
			if(!manager.CanBeiFolder(cbPath3))	
				throw new Exception("CanBeiFolder failed to allow a valid folder be made an iFolder.");

			string cbPath4 = Path.Combine(testPath, "iftester/stuff/ifolder");
			Console.WriteLine("Checking path: " + cbPath4);
			if(manager.CanBeiFolder(cbPath4))	
				throw new Exception("CanBeiFolder failed to recognize a path that has already been made an iFolder.");

			manager.DeleteiFolderById(newiFolder1.ID);
		}




		/// <summary>
		/// Test to create and delete iFolders
		/// </summary>
		[Test]
		public void CreateDeleteiFolder()
		{
			string testPath = Path.Combine(path, "c&difoldertest");

			Console.WriteLine("\nTEST: CreateDeleteiFolder");
			Console.WriteLine("-------------------------------------------");

			//
			// Create iFolders
			//
			Console.WriteLine("Creating iFolders");

			string path0 = Path.Combine(testPath, "testpath0");
			Console.WriteLine("CreateiFolder({0})", path0);
			iFolder newiFolder0 = manager.CreateiFolder(path0);
			Console.WriteLine("  Name={0}, ID={1}", newiFolder0.Name, 
					newiFolder0.ID);
			Console.WriteLine("  LocalPath={0}", newiFolder0.LocalPath);

			string path1 = Path.Combine(testPath, "sp1");
			Console.WriteLine("CreateiFolder({0})", path1);
			iFolder newiFolder1 = manager.CreateiFolder(path1);
			Console.WriteLine("  Name={0}, ID={1}", newiFolder1.Name, 
					newiFolder1.ID);
			Console.WriteLine("  LocalPath={0}", newiFolder1.LocalPath);

			string path2 = Path.Combine(testPath, "etc/opt/novell/user/data/shared/libraries/mono/denali/libspace/longfilepath2");
			Console.WriteLine("CreateiFolder(\"Mid iFolder\", {0})", path2);
			iFolder newiFolder2 = manager.CreateiFolder(path2);
			Console.WriteLine("  Name={0}, ID={1}", newiFolder2.Name, 
					newiFolder2.ID);
			Console.WriteLine("  LocalPath={0}", newiFolder2.LocalPath);

			string path3 = Path.Combine(testPath, "etc/opt/novell/user/data/shared/libraries/mono/denali/libspace/longfilepath/opt/novell/user/data/shared/libraries/mono/denali/libspace/longfilepath2");
			Console.WriteLine("CreateiFolder(\"Deep iFolder\", {0})", path3);
			iFolder newiFolder3 = manager.CreateiFolder(path3);
			Console.WriteLine("  Name={0}, ID={1}", newiFolder3.Name, 
					newiFolder3.ID);
			Console.WriteLine("  LocalPath={0}", newiFolder3.LocalPath);

			// Clean up iFolders Created above
			manager.DeleteiFolderById(newiFolder0.ID);
			manager.DeleteiFolderById(newiFolder1.ID);
			manager.DeleteiFolderById(newiFolder2.ID);
			manager.DeleteiFolderById(newiFolder3.ID);

			// Test creating an iFolder below an existing iFolder
			string path4 = Path.Combine(testPath, "testpath0/badPathiFolder");
			Console.WriteLine("CreateiFolder({0})", path4);
			try
			{
				manager.CreateiFolder(path4);

				// if we made it here, the API is broken, we shouldn't be
				// able to create an iFolder within an ifolder
				throw new ApplicationException("CreateiFolder failed to restrict the creation of an iFolder within an existing iFolder");
			}
			catch(Exception e)
			{
				// Normal, we expect it to throw an exception here
				Console.WriteLine("  Prevented creation of iFolder {0} within an existing iFolder", path4);
			}

			// Test creating an iFolder above an existing iFolder
			string path5 = Path.Combine(testPath, "etc/opt");
			Console.WriteLine("CreateiFolder({0})", path5);
			try
			{
				manager.CreateiFolder(path5);

				// if we made it here, the API is broken, we shouldn't be
				// able to create an iFolder above an existing ifolder
				throw new ApplicationException("CreateiFolder failed to restrict the creation of an iFolder above an existing iFolder");
			}
			catch(Exception e)
			{
				// Normal, we expect it to throw an exception here
				Console.WriteLine("  Prevented creation of iFolder {0} above an existing iFolder", path5);
			}
		}




		/// <summary>
		/// Test to enumerate iFolders
		/// </summary>
		[Test]
		public void EnumerateiFolders()
		{
			string testPath = Path.Combine(path, "enumifoldertest");

			Console.WriteLine("\nTEST: EnumerateiFolders");
			Console.WriteLine("-------------------------------------------");

			string path1 = Path.Combine(testPath, "path1");
			iFolder newiFolder1 = manager.CreateiFolder(path1);
			if (!manager.IsiFolder(path1))
			{
				throw new ApplicationException("iFolder not found: " + path1);
			}

			string path2 = Path.Combine(testPath, "path2");
			iFolder newiFolder2 = manager.CreateiFolder(path2);
			if (!manager.IsiFolder(path2))
			{
				throw new ApplicationException("iFolder not found: " + path2);
			}

			string path3 = Path.Combine(testPath, "path3");
			iFolder newiFolder3 = manager.CreateiFolder(path3);
			if (!manager.IsiFolder(path3))
			{
				throw new ApplicationException("iFolder not found: " + path3);
			}

			string path4 = Path.Combine(testPath, "path4");
			iFolder newiFolder4 = manager.CreateiFolder(path4);
			if (!manager.IsiFolder(path4))
			{
				throw new ApplicationException("iFolder not found: " + path4);
			}

			Console.WriteLine("Enumerating iFolders");
			foreach(iFolder ifolder in manager)
			{
				Console.WriteLine("iFolder Name: {0}", ifolder.Name);
				Console.WriteLine("iFolder ID  : {0}", ifolder.ID);
				Console.WriteLine("iFolder Path: {0}", ifolder.LocalPath);
			}
			Console.WriteLine("");

			manager.DeleteiFolderById(newiFolder1.ID);
			if (manager.IsiFolder(path1))
			{
				throw new ApplicationException("iFolder not deleted: " + path1);
			}

			manager.DeleteiFolderById(newiFolder2.ID);
			if (manager.IsiFolder(path2))
			{
				throw new ApplicationException("iFolder not deleted: " + path2);
			}

			manager.DeleteiFolderById(newiFolder3.ID);
			if (manager.IsiFolder(path3))
			{
				throw new ApplicationException("iFolder not deleted: " + path3);
			}

			manager.DeleteiFolderById(newiFolder4.ID);
			if (manager.IsiFolder(path4))
			{
				throw new ApplicationException("iFolder not deleted: " + path4);
			}
		}




		/// <summary>
		/// Test Case to check if a node is in an iFolder
		/// </summary>
		[Test]
		public void IsPathIniFolder()
		{
			string testPath = Path.Combine(path, "ispathinifoldertest");

			Console.WriteLine("\nTEST: IsPathIniFolder");
			Console.WriteLine("-------------------------------------------");

			string path1 = Path.Combine(testPath, "iftester");
			iFolder newiFolder1 = manager.CreateiFolder(path1);
			Console.WriteLine("iFolder Name : {0}", newiFolder1.Name);
			Console.WriteLine("iFolder ID   : {0}", newiFolder1.ID);
			Console.WriteLine("iFolder Path : {0}", newiFolder1.LocalPath);

			string isiFolderPath1 = Path.Combine(testPath, 
							"iftester/myFile.exe");	
			Console.WriteLine("Checking path: " + isiFolderPath1);

			if(!manager.IsPathIniFolder(isiFolderPath1))	
				throw new Exception("Failed to recognized file in iFolder.");

			string isiFolderPath2 = Path.Combine(testPath, 
				"iftester/path with spaces/new/folder/test/test with spaces");	
			Console.WriteLine("Checking path: " + isiFolderPath2);
			if(!manager.IsPathIniFolder(isiFolderPath2))	
				throw new Exception("Failed to recognized iFolder with spaces in path.");

			string isiFolderPath3 = Path.Combine(testPath, 
					"notanifolder/path with spaces/new/folder/test/test with spaces");	
			Console.WriteLine("Checking path: " + isiFolderPath3);
			if(manager.IsPathIniFolder(isiFolderPath3))	
				throw new Exception("Recognized path in iFolder when it should have failed.");

			string isiFolderPath3a = Path.Combine(testPath, "iftester");	
			Console.WriteLine("Checking path: " + isiFolderPath3a);
			if(!manager.IsPathIniFolder(isiFolderPath3a))	
				throw new Exception("Recognized path in iFolder when it should have failed.");


			string path2 = Path.Combine(testPath, "iftest with space");
			iFolder newiFolder2 = manager.CreateiFolder(path2);
			Console.WriteLine("iFolder Name : {0}", newiFolder2.Name);
			Console.WriteLine("iFolder ID   : {0}", newiFolder2.ID);
			Console.WriteLine("iFolder Path : {0}", newiFolder2.LocalPath);

			string isiFolderPath4 = Path.Combine(testPath, 
							"iftest with space/myFile.exe");	
			Console.WriteLine("Checking path: " + isiFolderPath4);

			if(!manager.IsPathIniFolder(isiFolderPath4))	
				throw new Exception("Failed to recognized file in iFolder.");

			string isiFolderPath5 = Path.Combine(testPath, 
					"iftest with space/path with spaces/new/folder/test/test with spaces");	
			Console.WriteLine("Checking path: " + isiFolderPath5);
			if(!manager.IsPathIniFolder(isiFolderPath5))	
				throw new Exception("Failed to recognized iFolder with spaces in path.");

			string isiFolderPath6 = Path.Combine(testPath, 
					"iftestwithspace/path with spaces/new/folder/test/test with spaces");	
			Console.WriteLine("Checking path: " + isiFolderPath6);
			if(manager.IsPathIniFolder(isiFolderPath6))	
				throw new Exception("Recognized path in iFolder when it should have failed.");
			
			Console.WriteLine("Tests done... Cleaning up iFolders");
			manager.DeleteiFolderById(newiFolder1.ID);
			manager.DeleteiFolderById(newiFolder2.ID);
			Console.WriteLine("Done cleaning up");
		}




		/// <summary>
		/// Test to add all child files to an iFolder
		/// </summary>
		[Test]
		public void AddiFolderNodesTest()
		{
			string testPath = Path.Combine(path, "addifoldernodestest");

			Console.WriteLine("\nTEST: AddiFolderNodes");
			Console.WriteLine("-------------------------------------------");


			Directory.CreateDirectory(testPath);

			// Copy all of the files in the test directory to this
			// iFolder's test area
			string[] dirs = Directory.GetFiles(rootPath);
			int fileCount = dirs.Length;

			foreach (string file in dirs)
			{
				string newFile = Path.Combine(testPath,
						Path.GetFileName(file));

				File.Copy(file, newFile, true);
			}

			iFolder addifolder = manager.CreateiFolder(testPath);

			// compensate for the current directory
			fileCount++;

			try
			{
				// Add the nodes for the existing files/directories.
				int count = addifolder.AddiFolderNodes(testPath);
				Console.WriteLine("Nodes created: {0}", count);
				if(count != fileCount)
					throw new ApplicationException(string.Format("The number of files in the directory was {0} and iFolder added {1}", fileCount, count));


				// Create a subdirectory where we can copy an existing file.
				string subDir1 = Path.Combine(testPath, "SubDir1");
				Directory.CreateDirectory(subDir1);

				// Create another subdirectory.
				string subDir2 = Path.Combine(subDir1, "SubDir2");
				Directory.CreateDirectory(subDir2);

				// ... and another.
				string subDir3 = Path.Combine(subDir2, "SubDir3");
				Directory.CreateDirectory(subDir3);

				// Copy one of the files from the current directory to 
				// the new directory.
				string[] files = Directory.GetFiles(testPath);
				string newFile = 
						Path.Combine(subDir3, Path.GetFileName(files[0]));
				File.Copy(files[0], newFile, true);


				// Now try to add the nodes for the new subdirectory structure.
				count = 0;
				Console.WriteLine("About to add subDir1 : " + subDir1);
				count = addifolder.AddiFolderNodes(subDir1);

				// Delete the new directory.
				Directory.Delete(subDir1, true);

				Console.WriteLine("Nodes created: {0}", count);

				Console.WriteLine("\nAll nodes:\n");
			}
			finally
			{
				manager.DeleteiFolderById(addifolder.ID);
				if (manager.IsiFolder(testPath))
				{
					throw new ApplicationException("iFolder not deleted: " + 
						testPath);
				}
			}
		}




		/// <summary>
		/// Test to add all child files to an iFolder
		/// </summary>
		[Test]
		public void UpdateiFolder()
		{
			string testPath = Path.Combine(path, "updateifoldertest");


			Console.WriteLine("\nTEST: UpdateiFolder");
			Console.WriteLine("-------------------------------------------");

			Directory.CreateDirectory(testPath);

			// Copy all of the files in the test directory to this
			// iFolder's test area
			string[] dirs = Directory.GetFiles(rootPath);

			foreach (string file in dirs)
			{
				string newFile = Path.Combine(testPath,
						Path.GetFileName(file));

				File.Copy(file, newFile, true);
			}

			iFolder upifolder = manager.CreateiFolder(testPath);

			try
			{
				// Create a subdirectory where we can copy an existing file.
				string newPath = Path.Combine(testPath, "SubDir1");
				Directory.CreateDirectory(newPath);

				// Add the nodes for the existing files/directories.
				upifolder.Update();

				// Copy one of the files from the current directory to the 
				// new directory.
				string[] files = Directory.GetFiles(testPath);
				string newFile = Path.Combine(newPath, 
						Path.GetFileName(files[0]));
				File.Copy(files[0], newFile, true);

				// Now try to update the iFolder.
				manager.UpdateiFolder(upifolder.ID);

				iFolderNode ifolderNode = 
						upifolder.GetiFolderNodeByPath(newFile);

				if (ifolderNode == null)
				{
					manager.DeleteiFolderById(upifolder.ID);
					throw new ApplicationException("Search for iFolderFile node failed");
				}

				// Delete the new directory.
				Directory.Delete(newPath, true);
			}
			finally
			{
				manager.DeleteiFolderByPath(upifolder.LocalPath);
				if (manager.IsiFolder(testPath))
				{
					throw new ApplicationException("iFolder not deleted: " + 
							testPath);
				}
			}
		}

		public int CountUsers(iFolder sharediFolder)
		{
			int usercount = 0;

			IFAccessControlList ifacl = sharediFolder.GetAccessControlList();

			foreach(IFAccessControlEntry iface in ifacl)
			{
				usercount++;
				switch(iface.Rights)
				{
					case iFolder.Rights.Deny:
						Console.WriteLine("{0}  Deny", 
								iface.Contact.UserName);
						break;
					case iFolder.Rights.ReadOnly:
						Console.WriteLine("{0}  ReadOnly", 
								iface.Contact.UserName);
						break;
					case iFolder.Rights.ReadWrite:
						Console.WriteLine("{0}  ReadWrite", 
								iface.Contact.UserName);
						break;
					case iFolder.Rights.Admin:
						Console.WriteLine("{0}  Admin", 
								iface.Contact.UserName);
						break;
				}
			}
			return usercount;
		}


		/// <summary>
		/// Test Access Control Lists
		/// </summary>
		[Test]
		public void AccessControlListTest()
		{
			string testPath = Path.Combine(path, "aclfoldertest");
			int usercount;


			Console.WriteLine("\nTEST: iFolder GetAccessControlList");
			Console.WriteLine("-------------------------------------------");

			Directory.CreateDirectory(testPath);

			iFolder aclifolder = manager.CreateiFolder(testPath);

			Console.WriteLine("New iFolder, should have only one user");
			usercount = CountUsers(aclifolder);
			if(usercount == 1)
				Console.WriteLine("One user... looks good");
			else
			{
				throw new ApplicationException("More than one user existed in anew iFolder");
			}

			Novell.AddressBook.Contact c = new Novell.AddressBook.Contact();
			c.UserName = "TestDude";
			ab.AddContact(c);
			c.Commit();

			aclifolder.SetRights(c, iFolder.Rights.ReadWrite);
			Console.WriteLine("Added users, should have two users");
			usercount = CountUsers(aclifolder);
			if(usercount == 2)
				Console.WriteLine("Two users... looks good");
			else
			{
				throw new ApplicationException("More than one user existed in anew iFolder");
			}

			for(int x = 0; x < 200; x++)
			{
				Novell.AddressBook.Contact newC = new Novell.AddressBook.Contact();
				newC.UserName = "TestUser:" + x;
				Console.WriteLine("Created user {0}", newC.UserName);
				ab.AddContact(newC);
				newC.Commit();

				aclifolder.SetRights(newC, iFolder.Rights.ReadWrite);
			}

			usercount = CountUsers(aclifolder);
			if(usercount == 202)
				Console.WriteLine("202 Users... looks good");
			else
			{
				throw new ApplicationException("There should be 202 users share on this iFolder and there were not");
			}
		}




		[TestFixtureTearDown]
		public void Cleanup()
		{
			foreach(iFolder ifolder in manager)
			{
				manager.DeleteiFolderByPath(ifolder.LocalPath);
			}
			Directory.Delete(path, true);
			Directory.Delete(simiasPath, true);
		}
	}

	public class Tests
	{
		static void Main()
		{
			iFolderTests tests = new iFolderTests();
			tests.Init();
			tests.CanBeiFolder();
			tests.CreateDeleteiFolder();
			tests.EnumerateiFolders();
			tests.IsPathIniFolder();
			tests.AddiFolderNodesTest();
			tests.UpdateiFolder();
			tests.AccessControlListTest();
			tests.Cleanup();
		}
	}
}


