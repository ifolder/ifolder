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
	/// <summary>
	/// Summary description for Iteration0Tests.
	/// </summary>
	[TestFixture]
	public class iFolderTests
	{
		private iFolderManager manager;
		private string path;



/*		private iFolder CreateiFolder(string name)
		{
			// Create a private (not shared) iFolder
			string iFolderPath = Path.Combine(path, name);
			Console.WriteLine("CreateiFolder({0})", iFolderPath);
			iFolder newiFolder = manager.CreateiFolder(iFolderPath);
			Console.WriteLine("  Name={0}, ID={1}", newiFolder.Name, newiFolder.ID);
			Console.WriteLine("  LocalPath={0}", newiFolder.LocalPath);
			return newiFolder;
		}
*/
		private iFolder CreateSharediFolder(string name)
		{
			// Create a Collection.
			iFolder sharediFolder = 
					manager.CreateiFolder(Path.Combine(path, name));
				
			// Create a user that can be impersonated.

			
			IIdentityFactory idFactory = IdentityManager.Connect();
			idFactory.Create( "testuser", "novell" );
			IIdentity identity = idFactory.Authenticate( "testuser", "novell" );

			// Create a Contact.
			// Share the Collection with the contact. Read-only.
			sharediFolder.Share(identity.UserGuid, Access.Rights.ReadOnly, false);
			Access.Rights rights = sharediFolder.GetShareAccess(identity.UserGuid);
			if (rights != Access.Rights.ReadOnly)
			{
				throw new Exception("Failed granting access.");
			}

			try
			{
				sharediFolder.CurrentNode.CollectionNode.LocalStore.ImpersonateUser( "testuser", "novell" );
				if (sharediFolder.IsShareable() == true)
				{
					throw new Exception("Failed not shareable test.");
				}
			}
			finally
			{
				sharediFolder.CurrentNode.CollectionNode.LocalStore.Revert(); 
			}

			// Grant admin rights to the iFolder.
			sharediFolder.Share(identity.UserGuid, Access.Rights.Admin, false);

			try
			{
				sharediFolder.CurrentNode.CollectionNode.LocalStore.ImpersonateUser( "testuser", "novell" );
				if (sharediFolder.IsShareable() == false )
				{
					throw new Exception("Failed shareable test.");
				}
			}
			finally
			{
				sharediFolder.CurrentNode.CollectionNode.LocalStore.Revert(); 
			}
			return sharediFolder;
		}

		private void AssertFileCount(string path, int count)
		{
			string[] files = Directory.GetFiles(path, "*");
			if (files.Length != count)
			{
				throw new ApplicationException("bad file count: expected=" +
					count + ", actual=" + files.Length + " in " + path);
			}
		}

		private void AssertFileCount(string path, int count, string searchPattern)
		{
			string[] files = Directory.GetFiles(path, searchPattern);
			if (files.Length != count)
			{
				throw new ApplicationException("bad file count: expected=" +
					count + ", actual=" + files.Length + " in " + path);
			}
		}

		private void AssertFileIsIniFolder(iFolder ifolder, string path, bool shouldBeThere)
		{
			// Verify that the file is in the iFolder
			iFolderFile iFolderFile = ifolder.GetiFolderFileByName(path);
			if ((iFolderFile == null) && shouldBeThere)
			{
				throw new ApplicationException("Assert failed: " + path + " is not in iFolder");
			}
			if ((iFolderFile != null) && !shouldBeThere)
			{
				throw new ApplicationException("Assert failed: " + path + " should not be in iFolder");
			}
		}
		private void DeleteiFolder(iFolder ifolder)
		{
			Console.WriteLine("DeleteiFolderById({0})", ifolder.ID);
			manager.DeleteiFolderById(ifolder.ID);
		}

		private void CreateOpenDeleteFiles(iFolder newiFolder)
		{
			int createdFileCnt = 0;
			string openFileTestPath = newiFolder.LocalPath;

			//
			// Create some files using CreateFile
			//
			string newFile = "create_2args";
			string newFilePath = Path.Combine(openFileTestPath, newFile);
			Console.WriteLine("CreateFile(" + newFile + ",1024)");
			FileStream stream = newiFolder.CreateFile(newFile, 1024);
			stream.Close();
			createdFileCnt++;
			// Verify that it has been added to the iFolder
			iFolderFile iFolderFile = newiFolder.GetiFolderFileByName(newFilePath);
			if (iFolderFile == null)
			{
				throw new ApplicationException("CreateFile(" + newFile + ") failed: not added to iFolder");
			}

			newFile = "create_1arg";
			newFilePath = Path.Combine(openFileTestPath, newFile);
			Console.WriteLine("CreateFile(" + newFile + ")");
			stream = newiFolder.CreateFile(newFile);
			stream.Close();
			createdFileCnt++;
			// Verify that it has been added to the iFolder
			iFolderFile = newiFolder.GetiFolderFileByName(newFilePath);
			if (iFolderFile == null)
			{
				throw new ApplicationException("CreateFile(" + newFile + ") failed: not added to iFolder");
			}

			//
			// Create some files using OpenFile
			//
			// Create a file using FileMode.Append (relative path)
			newFile = "open_append";
			newFilePath = Path.Combine(openFileTestPath, newFile);
			Console.WriteLine("OpenFile(" + newFile + ",Append,Write,None)");
			stream = newiFolder.OpenFile(newFile, FileMode.Append, FileAccess.Write, FileShare.None);
			stream.Close();
			createdFileCnt++;
			// Verify that it has been added to the iFolder
			iFolderFile = newiFolder.GetiFolderFileByName(newFilePath);
			if (iFolderFile == null)
			{
				throw new ApplicationException("OpenFile(" + newFile + ") failed: not added to iFolder");
			}

			// Create a file using FileMode.Create (full path)
			newFile = "open_create";
			newFilePath = Path.Combine(openFileTestPath, newFile);
			Console.WriteLine("OpenFile(" + newFilePath + ",Append,Write,None)");
			stream = newiFolder.OpenFile(newFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
			stream.Close();
			createdFileCnt++;
			// Verify that it has been added to the iFolder
			iFolderFile = newiFolder.GetiFolderFileByName(newFilePath);
			if (iFolderFile == null)
			{
				throw new ApplicationException("OpenFile(" + newFile + ") failed: not added to iFolder");
			}

			// Create a file using FileMode.CreateNew (relative path, no share argument)
			newFile = "open_create-new";
			newFilePath = Path.Combine(openFileTestPath, newFile);
			Console.WriteLine("OpenFile(" + newFile + ",CreateNew,Write)");
			stream = newiFolder.OpenFile(newFile, FileMode.CreateNew, FileAccess.Write);
			stream.Close();
			createdFileCnt++;
			// Verify that it has been added to the iFolder
			iFolderFile = newiFolder.GetiFolderFileByName(newFilePath);
			if (iFolderFile == null)
			{
				throw new ApplicationException("OpenFile(" + newFile + ") failed: not added to iFolder");
			}

			// Open a file using FileMode.Open (relative path, no share argument)
			newFile = "open_create-new";
			newFilePath = Path.Combine(openFileTestPath, newFile);
			Console.WriteLine("OpenFile(" + newFile + ",Open,Write)");
			stream = newiFolder.OpenFile(newFile, FileMode.Open, FileAccess.Write);
			stream.Close();
			// Verify that it has been added to the iFolder
			iFolderFile = newiFolder.GetiFolderFileByName(newFilePath);
			if (iFolderFile == null)
			{
				throw new ApplicationException("OpenFile(" + newFile + ") failed: no longer in iFolder");
			}

			// Create a file using FileMode.OpenOrCreate (full path, no access or share arguments)
			newFile = "open_open-or-create";
			newFilePath = Path.Combine(openFileTestPath, newFile);
			Console.WriteLine("OpenFile(" + newFilePath + ",OpenOrCreate)");
			stream = newiFolder.OpenFile(newFilePath, FileMode.OpenOrCreate);
			stream.Close();
			createdFileCnt++;
			// Verify that it has been added to the iFolder
			iFolderFile = newiFolder.GetiFolderFileByName(newFilePath);
			if (iFolderFile == null)
			{
				throw new ApplicationException("OpenFile(" + newFile + ") failed: not added to iFolder");
			}

			// Open a file using FileMode.Truncate (relative path, no access or share arguments)
			newFile = "open_open-or-create";
			newFilePath = Path.Combine(openFileTestPath, newFile);
			Console.WriteLine("OpenFile(" + newFile + ",Truncate)");
			stream = newiFolder.OpenFile(newFile, FileMode.Truncate);
			stream.Close();
			// Verify that it has been added to the iFolder
			iFolderFile = newiFolder.GetiFolderFileByName(newFilePath);
			if (iFolderFile == null)
			{
				throw new ApplicationException("OpenFile(" + newFile + ") failed: no longer in iFolder");
			}

			// Verify that all files were created
			string[] createdFiles = Directory.GetFiles(openFileTestPath);
			if (createdFiles.Length != createdFileCnt)
			{
				throw new ApplicationException("OpenFile failed: expected created files: " + createdFileCnt + ", actual: " + createdFiles.Length);
			}

			//
			// Delete the files
			//
			string deleteFile = "create_2args";
			string deleteFilePath = Path.Combine(openFileTestPath, deleteFile);
			Console.WriteLine("DeleteFile(" + deleteFile + ")");
			newiFolder.DeleteFile(deleteFile);
			// Verify that it has been removed from the iFolder
			iFolderFile = newiFolder.GetiFolderFileByName(deleteFilePath);
			if (iFolderFile != null)
			{
				throw new ApplicationException("DeleteFile(" + deleteFile + ") failed: file still in iFolder");
			}
			
			deleteFile = "create_1arg";
			deleteFilePath = Path.Combine(openFileTestPath, deleteFile);
			Console.WriteLine("DeleteFile(" + deleteFilePath + ")");
			newiFolder.DeleteFile(deleteFilePath);
			// Verify that it has been removed from the iFolder
			iFolderFile = newiFolder.GetiFolderFileByName(deleteFilePath);
			if (iFolderFile != null)
			{
				throw new ApplicationException("DeleteFile(" + deleteFile + ") failed: file still in iFolder");
			}
			
			deleteFile = "open_append";
			deleteFilePath = Path.Combine(openFileTestPath, deleteFile);
			Console.WriteLine("DeleteFile(" + deleteFile + ")");
			newiFolder.DeleteFile(deleteFile);
			// Verify that it has been removed from the iFolder
			iFolderFile = newiFolder.GetiFolderFileByName(deleteFilePath);
			if (iFolderFile != null)
			{
				throw new ApplicationException("DeleteFile(" + deleteFile + ") failed: file still in iFolder");
			}
			
			deleteFile = "open_create";
			deleteFilePath = Path.Combine(openFileTestPath, deleteFile);
			Console.WriteLine("DeleteFile(" + deleteFilePath + ")");
			newiFolder.DeleteFile(deleteFilePath);
			// Verify that it has been removed from the iFolder
			iFolderFile = newiFolder.GetiFolderFileByName(deleteFilePath);
			if (iFolderFile != null)
			{
				throw new ApplicationException("DeleteFile(" + deleteFile + ") failed: file still in iFolder");
			}
			
			deleteFile = "open_create-new";
			deleteFilePath = Path.Combine(openFileTestPath, deleteFile);
			Console.WriteLine("DeleteFile(" + deleteFile + ")");
			newiFolder.DeleteFile(deleteFile);
			// Verify that it has been removed from the iFolder
			iFolderFile = newiFolder.GetiFolderFileByName(deleteFilePath);
			if (iFolderFile != null)
			{
				throw new ApplicationException("DeleteFile(" + deleteFile + ") failed: file still in iFolder");
			}
			
			deleteFile = "open_open-or-create";
			deleteFilePath = Path.Combine(openFileTestPath, deleteFile);
			Console.WriteLine("DeleteFile(" + deleteFilePath + ")");
			newiFolder.DeleteFile(deleteFilePath);
			// Verify that it has been removed from the iFolder
			iFolderFile = newiFolder.GetiFolderFileByName(deleteFilePath);
			if (iFolderFile != null)
			{
				throw new ApplicationException("DeleteFile(" + deleteFile + ") failed: file still in iFolder");
			}

			// Verify that all files were deleted
			string[] deletedFiles = Directory.GetFiles(openFileTestPath);
			if (deletedFiles.Length != 0)
			{
				throw new ApplicationException("OpenFile failed: " + deletedFiles.Length + " files not deleted");
			}
		}

		private void CreateFile(string path, string contents)
		{
			Console.WriteLine("File.Create(" + path + ")");
			FileStream stream = File.Create(path);
			Byte[] buffer = new UTF8Encoding(true).GetBytes(contents);
			stream.Write(buffer, 0, buffer.Length);
			stream.Close();
		}

		private void MoveFiles(iFolder newiFolder)
		{
			// Create a subdirectory in the iFolder
			string subdir = "subdir";
			Console.WriteLine("CreateDirectory(" + subdir + ")");
			newiFolder.CreateDirectory(subdir);

			string origPath = path;
			string searchPattern = "moveFile*";
			string ifolderPath = newiFolder.LocalPath;
			string ifolderSubdirPath = Path.Combine(ifolderPath, subdir);
			
			int origFileCnt = 0;
			int ifolderFileCnt= 0;
			int ifolderSubdirFileCnt = 0;

			// Create test files to move (outside of the iFolder)
			string moveFile1 = "moveFile1";
			string moveFile1Path = Path.Combine(path, moveFile1);
			string moveFile1iFolderPath = Path.Combine(newiFolder.LocalPath, moveFile1);
			string newMoveFile1 = Path.Combine(subdir, "new" + moveFile1);
			string newMoveFile1Path = Path.Combine(path, newMoveFile1);
			string newMoveFile1iFolderPath = Path.Combine(newiFolder.LocalPath, newMoveFile1);
			CreateFile(moveFile1Path, "Contents of first move file");
			origFileCnt++;

			string moveFile2 = "moveFile2";
			string moveFile2Path = Path.Combine(path, moveFile2);
			string moveFile2iFolderPath = Path.Combine(newiFolder.LocalPath, moveFile2);
			string newMoveFile2 = Path.Combine(subdir, "new" + moveFile2);
			string newMoveFile2Path = Path.Combine(path, newMoveFile2);
			string newMoveFile2iFolderPath = Path.Combine(newiFolder.LocalPath, newMoveFile2);
			CreateFile(moveFile2Path, "Contents of second move file");
			origFileCnt++;

			string moveFile3 = "moveFile3";
			string moveFile3Path = Path.Combine(path, moveFile3);
			string moveFile3iFolderPath = Path.Combine(newiFolder.LocalPath, moveFile3);
			string newMoveFile3 = Path.Combine(subdir, "new" + moveFile3);
			string newMoveFile3Path = Path.Combine(path, newMoveFile3);
			string newMoveFile3iFolderPath = Path.Combine(newiFolder.LocalPath, newMoveFile3);
			CreateFile(moveFile3Path, "Contents of third move file");
			origFileCnt++;

			string moveFile4 = "moveFile4";
			string moveFile4Path = Path.Combine(path, moveFile4);
			string moveFile4iFolderPath = Path.Combine(newiFolder.LocalPath, moveFile4);
			string newMoveFile4 = Path.Combine(subdir, "new" + moveFile4);
			string newMoveFile4Path = Path.Combine(path, newMoveFile4);
			string newMoveFile4iFolderPath = Path.Combine(newiFolder.LocalPath, newMoveFile4);
			CreateFile(moveFile4Path, "Contents of fourth move file");
			origFileCnt++;

			// Verify that all the files were created
			AssertFileCount(origPath, origFileCnt, searchPattern);

			// Move the first file into the iFolder, same name, relative path
			newiFolder.MoveFile(moveFile1Path, moveFile1);
			AssertFileIsIniFolder(newiFolder, moveFile1iFolderPath, true);
			origFileCnt--;
			ifolderFileCnt++;

			// Move the second file into the iFolder, same name, full path
			newiFolder.MoveFile(moveFile2Path, moveFile2iFolderPath);
			AssertFileIsIniFolder(newiFolder, moveFile2iFolderPath, true);
			origFileCnt--;
			ifolderFileCnt++;

			// Verify that files were moved
			AssertFileCount(origPath, origFileCnt, searchPattern);
			AssertFileCount(ifolderPath, ifolderFileCnt);

			// Move the third file into the iFolder, subdir/newname, relative path
			newiFolder.MoveFile(moveFile3Path, newMoveFile3);
			AssertFileIsIniFolder(newiFolder, newMoveFile3iFolderPath, true);
			origFileCnt--;
			ifolderSubdirFileCnt++;

			// Move the fourth file into the iFolder, subdir/newname, full path
			newiFolder.MoveFile(moveFile4Path, newMoveFile4iFolderPath);
			AssertFileIsIniFolder(newiFolder, newMoveFile4iFolderPath, true);
			origFileCnt--;
			ifolderSubdirFileCnt++;

			// Verify that files were moved
			AssertFileCount(origPath, origFileCnt, searchPattern);
			AssertFileCount(ifolderSubdirPath, ifolderSubdirFileCnt);

			// Move the first file into subdir, new name, relative
			newiFolder.MoveFile(moveFile1, newMoveFile1);
			AssertFileIsIniFolder(newiFolder, newMoveFile1iFolderPath, true);
			ifolderFileCnt--;
			ifolderSubdirFileCnt++;

			// Move the second file into subdir, new name, full path
			newiFolder.MoveFile(moveFile2iFolderPath, newMoveFile2iFolderPath);
			AssertFileIsIniFolder(newiFolder, newMoveFile2iFolderPath, true);
			ifolderFileCnt--;
			ifolderSubdirFileCnt++;

			// Verify that files were moved
			AssertFileCount(origPath, origFileCnt, searchPattern);
			AssertFileCount(ifolderPath, ifolderFileCnt);
			AssertFileCount(ifolderSubdirPath, ifolderSubdirFileCnt);

			// Move the first file out of the iFolder, orig name, relative path
			newiFolder.MoveFile(newMoveFile1, moveFile1Path);
			AssertFileIsIniFolder(newiFolder, newMoveFile1iFolderPath, false);
			ifolderSubdirFileCnt--;
			origFileCnt++;

			// Move the third file out of the iFolder, orig name, full path
			newiFolder.MoveFile(newMoveFile3, moveFile3Path);
			AssertFileIsIniFolder(newiFolder, newMoveFile3iFolderPath, false);
			ifolderSubdirFileCnt--;
			origFileCnt++;

			// Verify that files were moved
			AssertFileCount(origPath, origFileCnt, searchPattern);
			AssertFileCount(ifolderPath, ifolderFileCnt);
			AssertFileCount(ifolderSubdirPath, ifolderSubdirFileCnt);

			// Delete the files
			File.Delete(moveFile1Path);
			newiFolder.DeleteFile(newMoveFile2iFolderPath);
			File.Delete(moveFile3Path);
			newiFolder.DeleteFile(newMoveFile4iFolderPath);

			// Verify that no files remain in iFolder
			AssertFileIsIniFolder(newiFolder, moveFile1iFolderPath, false);
			AssertFileIsIniFolder(newiFolder, newMoveFile1iFolderPath, false);
			AssertFileIsIniFolder(newiFolder, moveFile2iFolderPath, false);
			AssertFileIsIniFolder(newiFolder, newMoveFile2iFolderPath, false);
			AssertFileIsIniFolder(newiFolder, moveFile3iFolderPath, false);
			AssertFileIsIniFolder(newiFolder, newMoveFile3iFolderPath, false);
			AssertFileIsIniFolder(newiFolder, moveFile4iFolderPath, false);
			AssertFileIsIniFolder(newiFolder, newMoveFile4iFolderPath, false);

			// Verify that files were deleted
			AssertFileCount(origPath, 0, searchPattern);
			AssertFileCount(ifolderPath, 0);
			AssertFileCount(ifolderSubdirPath, 0);
		}

		/// <summary>
		/// This method performs a recursive print of the nodes contained in an iFolder.
		/// </summary>
		/// <param name="parentNode"></param>
		private void PrintNodes(Node parentNode)
		{
			foreach (Node node in parentNode)
			{
				// Print information for this node.
				Console.WriteLine("node in collection:\n\tName: {0}\n\tType: {1}\n\tParent node: {2}",
					node.Name, node.Type, parentNode.Name);

				if (node.Type == "Directory")
				{
					// If this node is a directory, recurse and print the information for it's child nodes.
					PrintNodes(node);
				}
			}
		}

		[TestFixtureSetUp]
		public void Init()
		{
			string currentPath = Directory.GetCurrentDirectory();

			// Create directory to perform tests in.
			path = Path.Combine(currentPath, "iFolderTest");
			Directory.CreateDirectory(path);

			string[] dirs= Directory.GetFiles(currentPath);
			foreach (string file in dirs)
			{
				string newFile = Path.Combine(path, Path.GetFileName(file));
				File.Copy(file, newFile, true);
			}

			manager = iFolderManager.Connect(new Uri(currentPath));
		}

		/// <summary>
		/// Test accepting an invitation
		/// </summary>
		[Test]
		public void AcceptInvitation()
		{
			Console.WriteLine("TEST: AcceptInvitation");

			Invitation invitation = new Invitation();

			invitation.CollectionId = "9876543210";
			invitation.CollectionName = "Team Folder";
			invitation.Identity = "1234567890";
			invitation.Domain = "novell";
			invitation.MasterHost = "192.168.2.1";
			invitation.MasterPort = "6437";
			invitation.CollectionRights = Access.Rights.ReadWrite.ToString();
			invitation.Message = "Bogus test iFolder Dude!";

			invitation.FromName = "Chuck Debouvie";
			invitation.FromEmail = "chuck@debouvie.com";
			invitation.ToName = "Burt Lancaster";
			invitation.ToEmail = "burt@bugusdeaddude.com";

			string path1 = Path.Combine(path, "iftester");
			iFolder newiFolder1 = manager.CreateiFolder("My iFolder Test", path1);
			Console.WriteLine("iFolder Name : {0}", newiFolder1.Name);
			Console.WriteLine("iFolder ID   : {0}", newiFolder1.ID);
			Console.WriteLine("iFolder Path : {0}", newiFolder1.LocalPath);

			string path2 = Path.Combine(path, "iftester/other/newiFolder");
			Console.WriteLine("Accepting to : {0}", path2);
			try
			{
				manager.AcceptInvitation(invitation, path2);

				// the API should have thrown an exception, throw one now
				// so we can fail the test
				throw new Exception("AcceptInvitation failed to recognize a path that has already been made an iFolder.");
			}
			catch(Exception e)
			{
				// This is the expected result
			}

			string path3 = Path.Combine(path, "myiFolders");
			Console.WriteLine("Accepting to : {0}", path3);
			manager.AcceptInvitation(invitation, path3);

			manager.DeleteiFolderById(newiFolder1.ID);
			Console.WriteLine("PASSED");
		}

		[Test]
		public void AddFileNodeToExistingParent()
		{
			Console.WriteLine("TEST: AddFileNodeToExistingParent");

			iFolder ifolder;
			ifolder = manager.CreateiFolder(path);

			try
			{
				// Create a subdirectory where we can copy an existing file.
				string newPath= Path.Combine(path, @"SubDir1");
				Directory.CreateDirectory(newPath);
				
				// Add the nodes for the existing files/directories.
				int count= ifolder.AddiFolderFileNodes(path);
				Console.WriteLine("Nodes created: {0}", count);
				//PrintNodes(ifolder.CurrentNode);

				// Copy one of the files from the current directory to the new directory.
				string[] files= Directory.GetFiles(path);
				string newFile= Path.Combine(newPath, Path.GetFileName(files[0]));
				File.Copy(files[0], newFile, true);

				// Now try to add the node for this file.
				iFolderFile ifolderfile= ifolder.CreateiFolderFile(newFile);

				iFolderFile ifolderfile2 = ifolder.GetiFolderFileByName(newFile);

				// TODO - use node overridden Equals when it becomes available.
				//if (!ifolderfile.ThisNode.Equals(ifolderfile2.ThisNode))
				if (ifolderfile.ThisNode.Id != ifolderfile2.ThisNode.Id)
				{
					throw new ApplicationException("Search for iFolderFile node failed");
				}

				// Delete the new directory.
				Directory.Delete(newPath, true);

				if (ifolderfile == null)
				{
					manager.DeleteiFolderById(ifolder.ID);
					throw new ApplicationException("Failed to create node for file " + newFile);
				}

				Console.WriteLine("New node added\n");
				//PrintNodes(ifolder.CurrentNode);

				Console.WriteLine("\nAll nodes:\n");
				ifolder.SetCurrentNodeToRoot();
				//PrintNodes(ifolder.CurrentNode);
			}
			finally
			{
				manager.DeleteiFolderById(ifolder.ID);
				if (manager.IsiFolder(path))
				{
					throw new ApplicationException("iFolder not deleted: " + path);
				}
			}
			Console.WriteLine("PASSED");
		}

		[Test]
		[ExpectedException( typeof( ApplicationException ) )]
		public void AddInvalidNode()
		{
			Console.WriteLine("TEST: AddInvalidNode");

			iFolder ifolder;
			ifolder = manager.CreateiFolder(path);

			try
			{
				string file= "junkfilename";
				iFolderFile ifolderfile= ifolder.CreateiFolderFile(file);
				if (ifolderfile == null)
				{
					throw new ApplicationException("Failed to create node for file " + file);
				}
			}
			finally
			{
				manager.DeleteiFolderById(ifolder.ID);
				if (manager.IsiFolder(path))
				{
					throw new ApplicationException("iFolder not deleted: " + path);
				}
			}
			Console.WriteLine("PASSED");
		}

		[Test]
		public void AddNodesRecursively()
		{
			Console.WriteLine("TEST: AddNodesRecursively");

			iFolder ifolder;
			ifolder = manager.CreateiFolder(path);

			try						
			{
				int count= ifolder.AddiFolderFileNodes(path);
				Console.WriteLine("Nodes created: {0}", count);

				//PrintNodes(ifolder.CurrentNode);
			}
			finally
			{
				manager.DeleteiFolderByPath(ifolder.LocalPath);
				if (manager.IsiFolder(path))
				{
					throw new ApplicationException("iFolder not deleted: " + path);
				}
			}
			Console.WriteLine("PASSED");
		}

		[Test]
		public void AddNodesSingly()
		{
			Console.WriteLine("TEST: AddNodesSingly");

			iFolder ifolder;
			ifolder = manager.CreateiFolder("My iFolder", path);

			try
			{
				string[] dirs= Directory.GetFiles(path);
				foreach (string file in dirs)
				{
					// Create the iFolderFile of type File.
					iFolderFile ifolderfile= ifolder.CreateiFolderFile(file);
					if (ifolderfile == null)
					{
						throw new ApplicationException("Failed to create node for file " + file);
					}
				}

				//PrintNodes(ifolder.CurrentNode);
			}
			finally
			{
				manager.DeleteiFolderById(ifolder.ID);
			}
			Console.WriteLine("PASSED");
		}

		/// <summary>
		/// Test Case to check if a Path and be made an iFolder
		/// </summary>
		[Test]
		public void CanBeiFolder()
		{
			Console.WriteLine("TEST: CanBeiFolder");

			string path1 = Path.Combine(path, "iftester/stuff/ifolder");
			iFolder newiFolder1 = manager.CreateiFolder("My iFolder Test", 
				path1);
			Console.WriteLine("iFolder Name : {0}", newiFolder1.Name);
			Console.WriteLine("iFolder ID   : {0}", newiFolder1.ID);
			Console.WriteLine("iFolder Path : {0}", newiFolder1.LocalPath);

			string cbPath1 = Path.Combine(path, 
				"iftester/stuff/ifolder/myFile.exe");	
			Console.WriteLine("Checking path: " + cbPath1);

			if(manager.CanBeiFolder(cbPath1))	
				throw new Exception("CanBeiFolder failed to recognized file in iFolder.");

			string cbPath2 = Path.Combine(path, "iftester/stuff");
			Console.WriteLine("Checking path: " + cbPath2);
			if(manager.CanBeiFolder(cbPath2))	
				throw new Exception("CanBeiFolder failed to recognized path above an iFolder");

			string cbPath3 = Path.Combine(path, "iftesterdude");
			Console.WriteLine("Checking path: " + cbPath3);
			if(!manager.CanBeiFolder(cbPath3))	
				throw new Exception("CanBeiFolder failed to allow a valid folder be made an iFolder.");

			string cbPath4 = Path.Combine(path, 
				"iftester/stuff/ifolder");
			Console.WriteLine("Checking path: " + cbPath4);
			if(manager.CanBeiFolder(cbPath4))	
				throw new Exception("CanBeiFolder failed to recognize a path that has already been made an iFolder.");

			manager.DeleteiFolderById(newiFolder1.ID);
			Console.WriteLine("PASSED");
		}

		[Test]
		public void CreateDeleteiFolder()
		{
			Console.WriteLine("TEST: CreateDeleteiFolder");

			//
			// Create iFolders
			//
			Console.WriteLine("Creating iFolders");

			string path0 = Path.Combine(path, "testpath0");
			Console.WriteLine("CreateiFolder({0})", path0);
			iFolder newiFolder0 = manager.CreateiFolder(path0);
			Console.WriteLine("  Name={0}, ID={1}", newiFolder0.Name, newiFolder0.ID);
			Console.WriteLine("  LocalPath={0}", newiFolder0.LocalPath);

			string path1 = Path.Combine(path, "sp1");
			Console.WriteLine("CreateiFolder({0})", path1);
			iFolder newiFolder1 = manager.CreateiFolder(path1);
			Console.WriteLine("  Name={0}, ID={1}", newiFolder1.Name, newiFolder1.ID);
			Console.WriteLine("  LocalPath={0}", newiFolder1.LocalPath);

			string path2 = Path.Combine(path, "etc/opt/novell/user/data/shared/libraries/mono/denali/libspace/longfilepath2");
			Console.WriteLine("CreateiFolder(\"Mid iFolder\", {0})", path2);
			iFolder newiFolder2 = manager.CreateiFolder("Mid iFolder", path2);
			Console.WriteLine("  Name={0}, ID={1}", newiFolder2.Name, newiFolder2.ID);
			Console.WriteLine("  LocalPath={0}", newiFolder2.LocalPath);

			string path3 = Path.Combine(path, "etc/opt/novell/user/data/shared/libraries/mono/denali/libspace/longfilepath/opt/novell/user/data/shared/libraries/mono/denali/libspace/longfilepath2");
			Console.WriteLine("CreateiFolder(\"Deep iFolder\", {0})", path3);
			iFolder newiFolder3 = manager.CreateiFolder("Deep iFolder", path3);
			Console.WriteLine("  Name={0}, ID={1}", newiFolder3.Name, newiFolder3.ID);
			Console.WriteLine("  LocalPath={0}", newiFolder3.LocalPath);

			// Test creating an iFolder below an existing iFolder
			string path4 = Path.Combine(path, "testpath0/badPathiFolder");
			Console.WriteLine("CreateiFolder({0})", path4);
			try
			{
				iFolder newiFolder4 = manager.CreateiFolder(path4);

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
			string path5 = Path.Combine(path, "etc/opt");
			Console.WriteLine("CreateiFolder({0})", path5);
			try
			{
				iFolder newiFolder5 = manager.CreateiFolder(path5);

				// if we made it here, the API is broken, we shouldn't be
				// able to create an iFolder above an existing ifolder
				throw new ApplicationException("CreateiFolder failed to restrict the creation of an iFolder above an existing iFolder");
			}
			catch(Exception e)
			{
				// Normal, we expect it to throw an exception here
				Console.WriteLine("  Prevented creation of iFolder {0} above an existing iFolder", path5);
			}

			iFolder newiFolder6 = CreateSharediFolder("CreateSharediFolder");

			//
			// Delete iFolders
			//
			Console.WriteLine("Deleting iFolders");

			Console.WriteLine("DeleteiFolderById({0})", newiFolder0.ID);
			manager.DeleteiFolderById(newiFolder0.ID);

			Console.WriteLine("DeleteiFolderByPath({0})", path1);
			manager.DeleteiFolderByPath(path1);

			Console.WriteLine("DeleteiFolderById({0})", newiFolder2.ID);
			manager.DeleteiFolderById(newiFolder2.ID);

			Console.WriteLine("DeleteiFolderByPath({0})", path3);
			manager.DeleteiFolderByPath(path3);

			Console.WriteLine("DeleteiFolderById({0})", newiFolder6.ID);
			manager.DeleteiFolderById(newiFolder6.ID);

			Console.WriteLine("PASSED");
		}

		[Test]
		public void CreateOpenDeleteFilePrivate()
		{
			Console.WriteLine("TEST: CreateOpenDeleteFilePrivate");
			iFolder newiFolder = CreateiFolder("CreateOpenDeleteFilePrivate");
			CreateOpenDeleteFiles(newiFolder);
			DeleteiFolder(newiFolder);
			Console.WriteLine("PASSED");
		}

		[Test]
		public void CreateOpenDeleteFileShared()
		{
			Console.WriteLine("TEST: CreateOpenDeleteFileShared");
			iFolder newiFolder = CreateSharediFolder("CreateOpenDeleteFileShared");
			CreateOpenDeleteFiles(newiFolder);
			DeleteiFolder(newiFolder);
			Console.WriteLine("PASSED");
		}

		[Test]
		public void EnumerateiFolders()
		{
			Console.WriteLine("TEST: EnumerateiFolders");

			string path1 = Path.Combine(path, "path1");
			iFolder newiFolder1 = manager.CreateiFolder(path1);
			if (!manager.IsiFolder(path1))
			{
				throw new ApplicationException("iFolder not found for path: " + path1);
			}

			string path2 = Path.Combine(path, "path2");
			iFolder newiFolder2 = manager.CreateiFolder(path2);
			if (!manager.IsiFolder(path2))
			{
				throw new ApplicationException("iFolder not found for path: " + path2);
			}

			string path3 = Path.Combine(path, "path3");
			iFolder newiFolder3 = manager.CreateiFolder(path3);
			if (!manager.IsiFolder(path3))
			{
				throw new ApplicationException("iFolder not found for path: " + path3);
			}

			string path4 = Path.Combine(path, "path4");
			iFolder newiFolder4 = manager.CreateiFolder(path4);
			if (!manager.IsiFolder(path4))
			{
				throw new ApplicationException("iFolder not found for path: " + path4);
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
			Console.WriteLine("PASSED");
		}

		/// <summary>
		/// Test Case to check if a node is in an iFolder
		/// </summary>
		[Test]
		public void IsPathIniFolder()
		{
			Console.WriteLine("TEST: IsPathIniFolder");

			string path1 = Path.Combine(path, "iftester");
			iFolder newiFolder1 = manager.CreateiFolder("My iFolder Test", path1);
			Console.WriteLine("iFolder Name : {0}", newiFolder1.Name);
			Console.WriteLine("iFolder ID   : {0}", newiFolder1.ID);
			Console.WriteLine("iFolder Path : {0}", newiFolder1.LocalPath);

			string isiFolderPath1 = Path.Combine(path, 
				"iftester/myFile.exe");	
			Console.WriteLine("Checking path: " + isiFolderPath1);

			if(!manager.IsPathIniFolder(isiFolderPath1))	
				throw new Exception("Failed to recognized file in iFolder.");

			string isiFolderPath2 = Path.Combine(path, 
				"iftester/path with spaces/new/folder/test/test with spaces");	
			Console.WriteLine("Checking path: " + isiFolderPath2);
			if(!manager.IsPathIniFolder(isiFolderPath2))	
				throw new Exception("Failed to recognized iFolder with spaces in path.");

			string isiFolderPath3 = Path.Combine(path, 
				"notanifolder/path with spaces/new/folder/test/test with spaces");	
			Console.WriteLine("Checking path: " + isiFolderPath3);
			if(manager.IsPathIniFolder(isiFolderPath3))	
				throw new Exception("Recognized path in iFolder when it should have failed.");

			string isiFolderPath3a = Path.Combine(path, 
				"iftester");	
			Console.WriteLine("Checking path: " + isiFolderPath3a);
			if(!manager.IsPathIniFolder(isiFolderPath3a))	
				throw new Exception("Recognized path in iFolder when it should have failed.");


			string path2 = Path.Combine(path, "iftest with space");
			iFolder newiFolder2 = manager.CreateiFolder("My iFolder Test With Space", path2);
			Console.WriteLine("iFolder Name : {0}", newiFolder2.Name);
			Console.WriteLine("iFolder ID   : {0}", newiFolder2.ID);
			Console.WriteLine("iFolder Path : {0}", newiFolder2.LocalPath);

			string isiFolderPath4 = Path.Combine(path, 
				"iftest with space/myFile.exe");	
			Console.WriteLine("Checking path: " + isiFolderPath4);

			if(!manager.IsPathIniFolder(isiFolderPath4))	
				throw new Exception("Failed to recognized file in iFolder.");

			string isiFolderPath5 = Path.Combine(path, 
				"iftest with space/path with spaces/new/folder/test/test with spaces");	
			Console.WriteLine("Checking path: " + isiFolderPath5);
			if(!manager.IsPathIniFolder(isiFolderPath5))	
				throw new Exception("Failed to recognized iFolder with spaces in path.");

			string isiFolderPath6 = Path.Combine(path, 
				"iftestwithspace/path with spaces/new/folder/test/test with spaces");	
			Console.WriteLine("Checking path: " + isiFolderPath6);
			if(manager.IsPathIniFolder(isiFolderPath6))	
				throw new Exception("Recognized path in iFolder when it should have failed.");
			manager.DeleteiFolderById(newiFolder1.ID);
			manager.DeleteiFolderById(newiFolder2.ID);
			Console.WriteLine("PASSED");
		}

		[Test]
		public void MoveFilePrivate()
		{
			Console.WriteLine("TEST: MoveFilePrivate");
			iFolder newiFolder = CreateiFolder("MoveFilePrivate");
			MoveFiles(newiFolder);
			DeleteiFolder(newiFolder);
			Console.WriteLine("PASSED");
		}

		[Test]
		public void MoveFileShared()
		{
			Console.WriteLine("TEST: MoveFileShared");
			iFolder newiFolder = CreateSharediFolder("MoveFileShared");
			MoveFiles(newiFolder);
			DeleteiFolder(newiFolder);
			Console.WriteLine("PASSED");
		}

		[Test]
		public void RecursiveAddNodesToExistingParent()
		{
			Console.WriteLine("TEST: RecursiveAddNodesToExistingParent");

			iFolder ifolder;
			ifolder = manager.CreateiFolder(path);

			try
			{
				// Create a subdirectory where we can copy an existing file.
				string subDir1= Path.Combine(path, @"SubDir1");
				Directory.CreateDirectory(subDir1);

				// Add the nodes for the existing files/directories.
				int count= ifolder.AddiFolderFileNodes(path);
				Console.WriteLine("Nodes created: {0}", count);
				//PrintNodes(ifolder.CurrentNode);

				// Create another subdirectory.
				string subDir2= Path.Combine(subDir1, @"SubDir2");
				Directory.CreateDirectory(subDir2);

				// ... and another.
				string subDir3= Path.Combine(subDir2, @"SubDir3");
				Directory.CreateDirectory(subDir3);

				// Copy one of the files from the current directory to the new directory.
				string[] files= Directory.GetFiles(path);
				string newFile= Path.Combine(subDir3, Path.GetFileName(files[0]));
				File.Copy(files[0], newFile, true);

				// Now try to add the nodes for the new subdirectory structure.
				count= ifolder.AddiFolderFileNodes(subDir2);

				// Delete the new directory.
				Directory.Delete(subDir1, true);

				Console.WriteLine("Nodes created: {0}", count);
				//PrintNodes(ifolder.CurrentNode);

				Console.WriteLine("\nAll nodes:\n");
				ifolder.SetCurrentNodeToRoot();
				//PrintNodes(ifolder.CurrentNode);
			}
			finally
			{
				manager.DeleteiFolderById(ifolder.ID);
				if (manager.IsiFolder(path))
				{
					throw new ApplicationException("iFolder not deleted: " + path);
				}
			}
			Console.WriteLine("PASSED");
		}

		/// <summary>
		/// Test Case to share and iFolder.
		/// </summary>
		[Test]
		public void ShareiFolder()
		{
			Console.WriteLine("TEST: ShareiFolder");

			// Create a Collection.
			iFolder sharediFolder = manager.CreateiFolder(Path.Combine(path, "Shared iFolder"));
				
			try
			{
				// Create a user that can be impersonated.
				IIdentityFactory idFactory = IdentityManager.Connect();
				idFactory.Create( "testuser", "novell" );
				IIdentity identity = idFactory.Authenticate( "testuser", "novell" );

				// Create a Contact.
				// Share the Collection with the contact. Read-only.
				sharediFolder.Share(identity.UserGuid, Access.Rights.ReadOnly, false);
				Access.Rights rights = sharediFolder.GetShareAccess(identity.UserGuid);
				if (rights != Access.Rights.ReadOnly)
				{
					throw new Exception("Failed granting access.");
				}

				try
				{
					sharediFolder.CurrentNode.CollectionNode.LocalStore.ImpersonateUser( "testuser", "novell" );
					if (sharediFolder.IsShareable() == true)
					{
						throw new Exception("Failed not shareable test.");
					}
				}
				finally
				{
					sharediFolder.CurrentNode.CollectionNode.LocalStore.Revert(); 
				}

				// Grant admin rights to the iFolder.
				sharediFolder.Share(identity.UserGuid, Access.Rights.Admin, false);

				try
				{
					sharediFolder.CurrentNode.CollectionNode.LocalStore.ImpersonateUser( "testuser", "novell" );
					if (sharediFolder.IsShareable() == false )
					{
						throw new Exception("Failed shareable test.");
					}
				}
				finally
				{
					sharediFolder.CurrentNode.CollectionNode.LocalStore.Revert(); 
				}

				// Remove the rights
				sharediFolder.Share(identity.UserGuid, Access.Rights.Deny, false);
				rights = sharediFolder.GetShareAccess(identity.UserGuid);
				if (rights != Access.Rights.Deny)
				{
					throw new Exception("Failed removing access.");
				}
			}

			finally
			{
				manager.DeleteiFolderById(sharediFolder.ID);
			}
			Console.WriteLine("PASSED");
		}

		[Test]
		public void UpdateiFolder()
		{
			Console.WriteLine("TEST: UpdateiFolder");

			iFolder ifolder;
			ifolder = manager.CreateiFolder(path);

			try
			{
				// Create a subdirectory where we can copy an existing file.
				string newPath= Path.Combine(path, @"SubDir1");
				Directory.CreateDirectory(newPath);
				
				// Add the nodes for the existing files/directories.
				int count= ifolder.AddiFolderFileNodes(path);
				Console.WriteLine("Nodes created: {0}", count);
				//PrintNodes(ifolder.CurrentNode);

				// Copy one of the files from the current directory to the new directory.
				string[] files= Directory.GetFiles(path);
				string newFile= Path.Combine(newPath, Path.GetFileName(files[0]));
				File.Copy(files[0], newFile, true);

				// Now try to update the iFolder.
				manager.UpdateiFolder(ifolder.ID);

				iFolderFile ifolderfile = ifolder.GetiFolderFileByName(newFile);
				if (ifolderfile == null)
				{
					manager.DeleteiFolderById(ifolder.ID);
					throw new ApplicationException("Search for iFolderFile node failed");
				}

				// Delete the new directory.
				Directory.Delete(newPath, true);

				Console.WriteLine("New node added\n");
				//PrintNodes(ifolder.CurrentNode);

				Console.WriteLine("\nAll nodes:\n");
				ifolder.SetCurrentNodeToRoot();
				//PrintNodes(ifolder.CurrentNode);
			}
			finally
			{
				manager.DeleteiFolderByPath(ifolder.LocalPath);
				if (manager.IsiFolder(path))
				{
					throw new ApplicationException("iFolder not deleted: " + path);
				}
			}
			Console.WriteLine("PASSED");
		}


		[TestFixtureTearDown]
		public void Cleanup()
		{
			foreach(iFolder ifolder in manager)
			{
				manager.DeleteiFolderByPath(ifolder.LocalPath);
			}
			Directory.Delete(path, true);
		}
	}
}


