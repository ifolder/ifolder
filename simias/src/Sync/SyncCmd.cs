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
 *  Author: Dale Olds <olds@novell.com>
 * 
 ***********************************************************************/
using System;
using System.IO;
using System.Diagnostics;
using System.Threading;

//using Simias;
using Simias.Storage;
//using Simias.InviteAgent;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
/// <summary>
/// command line execution of sync operations
/// </summary>

/*
public class FileInviteAgent: InviteAgent
{
	private string fileName;

	/// <summary>
	/// public constructor must specify fileName and path to local collection store
	/// </summary>
	public FileInviteAgent(string fileName, string storePath)
	{
		this.fileName = fileName;
		base.StorePath = storePath;
	}

	/// <summary>
	/// Invite a user to a collection via a file
	/// </summary>
	/// <param name="invitation">The invitation to save into filename</param>
	public override void Invite(Invitation invitation)
	{
		invitation.Save(fileName);
	}
}
*/

//---------------------------------------------------------------------------
/// <summary>
/// command line execution of sync operations
/// </summary>
public class SyncCmd
{
	Uri storeLocation = null;
	string userName = null, credential = "novell";
	int port = 8088;
	bool useTCP = true;
	string host = MyDns.GetHostName();

	int RunSync(Uri docRoot, string serverStoreLocation)
	{
		Store store = Store.Connect(storeLocation);
		Client client = null;
		string userId = "sam", credential = "password", collectionName, collectionId;
		Uri masterUri;

		if (!SyncPoint.FindCollection(store, docRoot, out masterUri, out collectionName, out collectionId))
		{
			Log.Error("Could not find collection {0}", docRoot);
			return 2;
		}
		store.Dispose();
		store = null;
		UriBuilder ub = new UriBuilder(masterUri);
		string serverCollectionId = ub.Path.Trim('/');

		SyncSession session;
		if (serverStoreLocation != null)
		{
			//TODO: check other parts of ub -- scheme, host, etc.
			session = (SyncSession)SyncSession.SessionFactory(
					new Uri(Path.GetFullPath(serverStoreLocation)),
					userId, credential, serverCollectionId, null);
		}
		else
		{
			client = new Client(ub.Host, ub.Port, userId, credential, serverCollectionId, useTCP);
			session = (SyncSession)client.session;
		}

		SyncPass.Run(storeLocation, collectionId, session);
		return 0;
	}

	//TODO: rights? userToInvite is ignored -- just use collection owner
	int Invite(Uri docRoot, string invitationFile)
	{
		Store store = Store.Connect(storeLocation);
		if (!SyncPoint.Invite(store, docRoot, invitationFile))
		{
			SyncPoint.InitMaster(store, host, port, docRoot);
			if (!SyncPoint.Invite(store, docRoot, invitationFile))
			{
				Log.Error("could not make invitation");
				return 3;
			}
		}
		return 0;
	}

	int Accept(string invitationFile, string docRootParent)
	{
		Store store = Store.Connect(storeLocation);
		SyncPoint.Accept(store, docRootParent, invitationFile);
		return 0;
	}

	int RunServer()
	{
		Server server = new Server(host, port, storeLocation, new SessionFactory(SyncSession.SessionFactory), useTCP);
		server.Start();
		Console.WriteLine("server {0} started, press enter to exit", port);
		Console.ReadLine();
		return 0;
	}

	int Usage(string errMsg)
	{
		string name = Process.GetCurrentProcess().ProcessName;
		string[] usage =
		{
			"",
			"Usage: " + name + " [options] operation operationParams",
			"",
			"    operations:",
			"        invite folder invitationFile",
			"            prints the invitation info to the invitationFile",
			"            initializes the collection store if necessary",
			"",
			"        accept invitationFile folderRoot",
			"            accepts the invitation into the collection store",
			"",
			"        sync folder",
			"            does one sync of specified folder",
			"",
			"        localsync folder localServerStore",
			"            does one sync of specified folder",
			"",
			"        server",
			"            runs as simple server",
			"",
			"    options:",
			"        -s storeLocation",
//			"        -u userName (to impersonate to collection store)",
			"        -l traceLevel (off, error, warning, info, verbose)",
			"        -c traceClass (all or class name)",
//			"        -w password (yeah, I know this is bad -- fix soon)",
			"        -p port (used by server and invite operations)",
			"        -h (use http and soap)",
			"        -n (name to use for local host, can be ip address)",
			"",
		};
		if (errMsg != null)
		{
			Console.WriteLine();
			Console.WriteLine(errMsg);
		}
		foreach (string s in usage)
			Console.WriteLine(s);
		return errMsg == null? 0: 10;
	}

	int Run(string[] args)
	{
		if (args.Length == 0)
			return Usage(null);

		Log.SetLevel("verbose");

		for (int i = 0; i < args.Length;)
		{
			string s = args[i++];
			if (s.StartsWith("-"))
			{
				if (i == args.Length)
					return Usage("incomplete option");
				if (s == "-s")
					storeLocation = new Uri(Path.GetFullPath(args[i++]));
				else if (s == "-u")
					userName = args[i++];
				else if (s == "-w")
					credential = args[i++];
				else if (s == "-p")
				{
					port = Int32.Parse(args[i++]);
					if (port == 0)
						return Usage("invalid port: 0");
				}
				else if (s == "-l")
				{
					if (!Log.SetLevel(args[i++]))
						return Usage("Unknown trace level");
				}
				else if (s == "-c")
					Log.SetCategory(args[i++]);
				else if (s == "-h")
					useTCP = false;
				else if (s == "-n")
					host = args[i++];
				else
					return Usage("unknown option");
			}
			else switch (s)
			{
				case "invite":
					if (args.Length - i != 2)
						return Usage("operation 'invite' takes 2 params");
					return Invite(new Uri(Path.GetFullPath(args[i])), args[i + 1]);
				case "accept":
					if (args.Length - i != 2)
						return Usage("operation 'accept' takes 2 params");
					return Accept(args[i], args[i + 1]);
				case "sync":
					if (args.Length - i != 1)
						return Usage("operation 'sync' takes 1 param");
					return RunSync(new Uri(Path.GetFullPath(args[i])), null);
				case "localsync":
					if (args.Length - i != 2)
						return Usage("operation 'localsync' takes 2 params");
					return RunSync(new Uri(Path.GetFullPath(args[i])), args[i + 1]);
				case "server":
					if (args.Length - i != 0)
						return Usage("operation 'server' takes 0 params");
					return RunServer();
				default:
					return Usage("unknown operation '" + s + "'");
			}
		}
		return Usage("no operation specified");
	}

	/// <summary>
	/// command line execution of sync operations, run with no args for usage
	/// </summary>
	public static int Main(string[] args)
	{
		try
		{
			Trace.Listeners.Add(new TextWriterTraceListener(System.Console.Out));
			SyncCmd cmd = new SyncCmd();
			return cmd.Run(args);
		}
		catch (Exception e)
		{
			Console.WriteLine("Uncaught exception in main: {0}", e.Message);
			Console.WriteLine(e.StackTrace);
		}
		catch
		{
			Console.WriteLine("Uncaught foreign exception in main");
		}
		finally
		{
			// just trying to get repeatable results
			GC.Collect();
			Thread.Sleep(1000);
			GC.Collect();
			Thread.Sleep(1000);
		}
		return 42;
	}

}

//===========================================================================
}
