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

using Simias.Invite;
using Simias.Sync;
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
			Console.WriteLine("Created ifolder: {0}", newifolder.LocalPath);
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

		try
		{
			iFolderManager manager = iFolderManager.Connect();
			foreach(iFolder ifolder in manager)
			{
				Console.WriteLine("{0}", ifolder.LocalPath);
			}
		}
		catch(Exception e)
		{
			Console.WriteLine("Unable to list iFolders");
			Console.WriteLine(e);
		}
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




	static bool SetRights(string [] args)
	{
		if(args[0] != "setrights")
			return false;

		if( (args.Length != 4) ||
			((args[3] != "full") && 
			(args[3] != "rw")  && 
			(args[3] != "ro")) )
		{
			DisplayUsage();
			return true;
		}

		try
		{
			iFolderManager manager = iFolderManager.Connect();
			if(manager.IsiFolder(args[1]) == false)
			{
				Console.WriteLine("Not an iFolder: {0}", args[1]);
				return true;
			}

			Contact contact = GetContact(manager, args[2]);
			if(contact == null)
			{
				Console.WriteLine("Not a contact: {0}", args[2]);
				return true;
			}

			iFolder ifolder = manager.GetiFolderByPath(args[1]);

			if(args[3] == "full")
				ifolder.SetRights(contact, iFolder.Rights.Admin);
			if(args[3] == "rw")
				ifolder.SetRights(contact, iFolder.Rights.ReadWrite);
			if(args[3] == "ro")
				ifolder.SetRights(contact, iFolder.Rights.ReadOnly);

			Console.WriteLine("\"{0}\" rights were set for {1}", args[3], args[2]);
		}
		catch(Exception e)
		{
			Console.WriteLine("Unable to set rights on iFolder: " + args[1]);
		}
		return true;
	}




	static bool RemoveRights(string [] args)
	{
		if(args[0] != "removerights")
			return false;

		if(args.Length != 3)
		{
			DisplayUsage();
			return true;
		}

		try
		{
			iFolderManager manager = iFolderManager.Connect();
			if(manager.IsiFolder(args[1]) == false)
			{
				Console.WriteLine("Not an iFolder: {0}", args[1]);
				return true;
			}

			Contact contact = GetContact(manager, args[2]);
			if(contact == null)
			{
				Console.WriteLine("Not a contact: {0}", args[2]);
				return true;
			}

			iFolder ifolder = manager.GetiFolderByPath(args[1]);

			ifolder.RemoveRights(contact);

			Console.WriteLine("Rights were removed for {0}", args[2]);
		}
		catch(Exception e)
		{
			Console.WriteLine("Unable to remove rights for {0}: " + args[2]);
		}
		return true;
	}




	static Contact GetContact(iFolderManager ifMan, string name)
	{
		Novell.AddressBook.Manager abMan = ifMan.AddressBookManager;
		if(abMan != null)
		{
			AddressBook ab = abMan.OpenDefaultAddressBook();

			IABList cList = ab.SearchUsername(name, 
								Simias.Storage.SearchOp.Equal);

			foreach(Contact tmpContact in cList)
			{
				return tmpContact;
			}
		}	
		return null;
	}
 



	static bool CreateInviteFile(string [] args)
	{
		if(args[0] != "createinvitefile")
			return false;

		if(args.Length != 4)
		{
			DisplayUsage();
			return true;
		}

		try
		{
			iFolderManager manager = iFolderManager.Connect();
			if(manager.IsiFolder(args[1]) == false)
			{
				Console.WriteLine("Not an iFolder: {0}", args[1]);
				return true;
			}

			Contact contact = GetContact(manager, args[2]);
			if(contact == null)
			{
				Console.WriteLine("Not a contact: {0}", args[2]);
				return true;
			}

			iFolder ifolder = manager.GetiFolderByPath(args[1]);

			ifolder.CreateInvitationFile(contact, args[3]);

			Console.WriteLine("Invitation file was created: {0}", args[3]);
		}
		catch(Exception e)
		{
			Console.WriteLine("Unable to create invitation file: " + args[3]);
		}
		return true;
	}




	static bool ViewInviteFile(string [] args)
	{
		if(args[0] != "viewinvitefile")
			return false;

		if(args.Length != 2)
		{
			DisplayUsage();
			return true;
		}

		try
		{
			Invitation invitation = new Invitation();

			invitation.Load(args[1]);
			Console.WriteLine("  iFolder  : {0}", invitation.CollectionName);
			Console.WriteLine("  From     : {0}", invitation.FromName);
			Console.WriteLine("  Sender   : {0}", invitation.FromEmail);
			Console.WriteLine("  Rights   : {0}", invitation.CollectionRights);
		}
		catch(Exception e)
		{
			Console.WriteLine("Unable to load file: {0}", args[1]);
		}
		return true;
	}




	static bool AcceptInviteFile(string [] args)
	{
		if(args[0] != "acceptinvitefile")
			return false;

		if(args.Length != 3)
		{
			DisplayUsage();
			return true;
		}

		try
		{
			iFolderManager manager = iFolderManager.Connect();
			if(manager.IsPathIniFolder(args[2]))
			{
				Console.WriteLine("Path is located within an existing iFolder: {0}", args[2]);
				return true;
			}

			Invitation invitation = new Invitation();

			invitation.Load(args[1]);

			manager.AcceptInvitation(invitation, args[2]);

			Console.WriteLine("Invitation file was Accepted");
		}
		catch(Exception e)
		{
			Console.WriteLine("Unable to accept invitation file: " + args[3]);
		}
		return true;
	}




	static void DisplayUsage()
	{
		Console.WriteLine("Usage: iFolderCmd <command> [options]");
		Console.WriteLine("  list");
		Console.WriteLine("  create <folder path>");
		Console.WriteLine("  revert <folder path>");
		Console.WriteLine("  setrights <ifolder path> <contact name> <full|rw|ro>");
		Console.WriteLine("  removerights <ifolder path> <contact name>");
		Console.WriteLine("  createinvitefile <ifolder path> <contact name> <filename>");
		Console.WriteLine("  viewinvitefile <filename>");
		Console.WriteLine("  acceptinvitefile <filename> <new ifolder parent path>");
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
			if(RevertiFolder(args) == true)
				return;
			if(SetRights(args) == true)
				return;
			if(RemoveRights(args) == true)
				return;
			if(CreateInviteFile(args) == true)
				return;
			if(ViewInviteFile(args) == true)
				return;
			if(AcceptInviteFile(args) == true)
				return;

			Console.WriteLine("Unknown command: " + args[0]);
		}
	}
}
