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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

using System;

using Novell.iFolder;

/// <summary>
/// Provides a command-line interface into the iFolder APIs.
/// </summary>
public class iFolderCmd
{

	static bool CreateiFolder(string [] args)
	{
		if(args[0] != "create")
			return false;

		if(args.Length != 2)
		{
			DisplayUsage();
			return true;
		}

		try
		{
			iFolderManager manager = iFolderManager.Connect();
			iFolder newifolder = manager.CreateiFolder(args[1]);
			Console.WriteLine("Created ifolder: {0}:{1}:{2}",newifolder.Name, newifolder.LocalPath,newifolder.ID);
		}
		catch(Exception e)
		{
			Console.WriteLine("Failed to create iFolder: " + args[1]);
			Console.WriteLine(e);
		}
		return true;
	}




	static bool ListiFolders(string [] args)
	{
		if(args[0] != "list")
			return false;

		Console.WriteLine("iFolders:");
		try
		{
			iFolderManager manager = iFolderManager.Connect();
			foreach(iFolder ifolder in manager)
			{
				Console.WriteLine("{0}:{1}:{2}", ifolder.Name,ifolder.LocalPath,ifolder.ID);
			}
		}
		catch(Exception e)
		{
			Console.WriteLine("Unable to list iFolders");
			Console.WriteLine(e);
		}
		return true;
	}




	static bool DeleteiFolder(string [] args)
	{
		if(args[0] != "delete")
			return false;

		if(args.Length != 2)
		{
			DisplayUsage();
			return true;
		}

		try
		{
			iFolderManager manager = iFolderManager.Connect();
			foreach(iFolder ifolder in manager)
			{
				if(ifolder.Name == args[1])
				{
					manager.DeleteiFolderById(ifolder.ID);
					Console.WriteLine("Deleted iFolder: " + args[1]);
					return true;
				}
			}
		}
		catch(Exception e)
		{
			Console.WriteLine("Unable to delete iFolder: " + args[1]);
			Console.WriteLine(e);
		}

		Console.WriteLine("iFolder not found: " + args[1]);
		return true;
	}




	static bool RevertiFolder(string [] args)
	{
		if(args[0] != "revert")
			return false;

		if(args.Length != 2)
		{
			DisplayUsage();
			return true;
		}

		try
		{
			iFolderManager manager = iFolderManager.Connect();
			if(manager.IsiFolder(args[1]))
			{
				manager.DeleteiFolderByPath(args[1]);
				Console.WriteLine("Reverted to a normal folder: {0}", args[1]);
			}
			else
			{
				Console.WriteLine("Not an iFolder: {0}", args[1]);
			}
		}
		catch(Exception e)
		{
			Console.WriteLine("Unable to revert iFolder: " + args[1]);
		}
		return true;
	}



	static void DisplayUsage()
	{
		Console.WriteLine("Usage: iFolderCmd <command> [options]");
		Console.WriteLine("  list                 : Lists all iFolders");
		Console.WriteLine("  create <path>        : Converts <path> to an iFolder");
		Console.WriteLine("  delete <name>        : Deletes the iFolder <name>");
		Console.WriteLine("  revert <path>        : Reverts <path> to a normal folder");
		return;
	}


	static void Main(string [] args)
	{
		if(args.Length < 1)
		{
			DisplayUsage();
			return;
		}
		else
		{
			if(CreateiFolder(args) == true)
				return;
			if(ListiFolders(args) == true)
				return;
			if(DeleteiFolder(args) == true)
				return;
			if(RevertiFolder(args) == true)
				return;

			Console.WriteLine("Unknown command: " + args[0]);
		}
	}
}
