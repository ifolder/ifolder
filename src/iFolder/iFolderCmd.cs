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
			Console.WriteLine("Usage: iFolderCmd create <iFolderLocalPath>");
			return true;
		}

		try
		{
			iFolderManager manager = iFolderManager.Connect();
			manager.CreateiFolder(args[1]);
		}
		catch(Exception e)
		{
			Console.WriteLine("Failed to create iFolder: " + args[1]);
		}
		return true;
	}




	static bool ListiFolders(string [] args)
	{
		if(args[0] != "list")
			return false;

		try
		{
			Console.WriteLine(" ");
			iFolderManager manager = iFolderManager.Connect();
			foreach(iFolder ifolder in manager)
			{
				Console.WriteLine("iFolder Name: {0}", ifolder.Name);
				Console.WriteLine("iFolder ID  : {0}", ifolder.ID);
				Console.WriteLine("iFolder Path: {0}", ifolder.LocalPath);
			}
		}
		catch(Exception e)
		{
			Console.WriteLine("Unable to list iFolders");
		}
		return true;
	}




	static bool DeleteiFolder(string [] args)
	{
		if(args[0] != "delete")
			return false;

		if(args.Length != 2)
		{
			Console.WriteLine("Usage: iFolderCmd delete <iFolderName>");
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
					return true;
				}
			}
		}
		catch(Exception e)
		{
			Console.WriteLine("Unable to delete iFolder: " + args[1]);
		}
		Console.WriteLine("iFolder not found: " + args[1]);
		return true;
	}




	static void Main(string [] args)
	{
		if(args.Length < 1)
		{
			Console.WriteLine("Usage: iFolderCmd <command> [options]");
			Console.WriteLine("Available commands: create list delete");
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

			Console.WriteLine("Unknown command: " + args[0]);
		}
	}
}
