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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;

using Simias.Storage;
using Simias.Agent;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
/// <summary>
/// class to handle invitation generation and acceptance via files
/// </summary>
public class FileInviter
{
	Store store;
	SyncStore syncStore;

	public FileInviter(Uri storeLocation)
	{
		syncStore = new SyncStore(storeLocation == null? null: storeLocation.LocalPath);
		store = syncStore.BaseStore;
	}

	// TODO: is the following path comparison correct? should it be case insensitive?
	internal static Collection FindCollection(Store store, Uri docRoot)
	{
		foreach (Collection c in store.GetCollectionsByType(Dredger.NodeTypeDir))
			if (c.DocumentRoot != null && c.DocumentRoot.LocalPath == docRoot.LocalPath)
				return c;
		return null;
	}

	/* TODO: this method uses store.CreateCollection rather than via SyncStore
	 * so that we can pass in a type. Should we enforce that these Collections
	 * are of some distinguishable 'ifolder' type and ensure that multiple
	 * collections of the same docRoot are not allowed?
	 */
	public void InitMaster(string host, int port, Uri docRoot)
	{
		DirectoryInfo di = new DirectoryInfo(docRoot.LocalPath);
		di.Create();
		Collection c = store.CreateCollection(di.Name, Dredger.NodeTypeDir, docRoot);
		SyncCollection scoll = new SyncCollection(c);
		scoll.Port = port;
		scoll.Host = host;
		scoll.Commit();
		Log.Assert(c.Id == c.CollectionNode.Id && c.Id == scoll.ID);
		Log.Spew("Created new master collection for {0}, id {1}, {2}:{3}", docRoot.LocalPath, c.Id, scoll.Host, scoll.Port);
	}

	/// <summary>
	/// Accept an invitation file for user to a collection
	/// </summary>
	public bool Accept(string docRootParent, string fileName)
	{
		/* TODO: the checks in this method that ensure that multiple collections
		 * of the same docRoot or collectionIdare not allowed should be
		 * done at a lower level
		 */
		Invitation invitation = new Invitation(fileName);
		invitation.RootPath = docRootParent;

		if (invitation.CollectionType != Dredger.NodeTypeDir)
		{
			Log.Error("This utility only handles invitations to collections of type {0}", Dredger.NodeTypeDir);
			Log.Error("This invitation is of type {0}", invitation.CollectionType);
			return false;
		}

		Uri docRoot = new Uri(Path.Combine(Path.GetFullPath(docRootParent), invitation.CollectionName));
		Collection c = FindCollection(store, docRoot);
		if (c != null)
		{
			if ((Nid)c.Id == (Nid)invitation.CollectionId)
				Log.Info("ignoring duplicate invitation for folder {0}", docRoot.LocalPath);
			else
				Log.Error("ignoring invitation, folder {0} already linked to a different collection", docRoot.LocalPath);
			return false;
		}

		if ((c = store.GetCollectionById(invitation.CollectionId)) != null)
		{
			Log.Warn("Collection already exists rooted in different local folder", c.DocumentRoot.LocalPath);
			return false;
		}

		// add the secret to the current identity chain
		store.CurrentIdentity.CreateAlias(invitation.Domain, invitation.Identity);
		store.CurrentIdentity.Commit();

		// add the invitation information to the store collection
		SyncCollection sc = syncStore.CreateCollection(invitation);
		sc.Commit();
		Log.Spew("Created new client collection for {0}, id {1}", docRoot.LocalPath, sc.ID);
		return true;
	}

	/// <summary>
	/// Create an invitation file for user to a collection
	/// </summary>
	public bool Create(Uri docRoot, string fileName)
	{
		Collection c = FindCollection(store, docRoot);
		if (c == null)
			return false;
		SyncCollection sc = new SyncCollection(c);
		Invitation invitation = sc.CreateInvitation(store.CurrentUser);
		invitation.Domain = store.DomainName;
		invitation.Save(fileName);
		return true;
	}
}

//---------------------------------------------------------------------------
public class Service: MarshalByRefObject
{
	private Uri storeLocation;

	public Service(Uri storeLocation)
	{
		this.storeLocation = storeLocation;
	}

	public SynkerServiceA StartSession(string collectionId)
	{
		try
		{
			SyncStore store = new SyncStore(storeLocation.LocalPath);
			SyncCollection c = store.OpenCollection(collectionId);
			return c == null? null: new SynkerServiceA(c);
		}
		catch (Exception e) { Log.Uncaught(e); }
		return null;
	}
}

//---------------------------------------------------------------------------
public class Server
{
	Service obj = null;
	IChannel channel = null;
	ObjRef objRef = null;
	string uri;

	const string serviceTag = "sync.rem";
	const string channelName = "SyncCmdServer";

	public static string MakeUri(string host, int port, bool useTCP)
	{
		return String.Format("{0}://{1}:{2}/{3}",
				(useTCP? "tcp": "http"), host, port, serviceTag);
	}

	public Server(string host, int port, Uri storeLocation, bool useTCP)
	{
		uri = MakeUri(host, port, useTCP);
		obj = new Service(storeLocation);
		channel = useTCP? (IChannel)new TcpServerChannel(channelName, port):
				(IChannel)new HttpServerChannel(channelName, port);
		ChannelServices.RegisterChannel(channel);
		objRef = RemotingServices.Marshal(obj, serviceTag);
		Log.Info("Server {0} is up and running from store '{1}'", uri, storeLocation);
	}

	public void Stop()
	{
		Log.Spew("Server {0} stopping", uri);
		if (obj != null)
			RemotingServices.Disconnect(obj);
		if (channel != null)
			ChannelServices.UnregisterChannel(channel);
	}
}

//---------------------------------------------------------------------------
public class Client
{
	Service service = null;
	IChannel channel = null;
	public SynkerServiceA session = null;

	const string channelName = "SyncCmdClient";

	public Client(string host, int port, string collectionId, bool useTCP)
	{
		if (useTCP)
			channel = new TcpClientChannel(channelName, new BinaryClientFormatterSinkProvider());
		else
			channel = new HttpClientChannel(channelName, new SoapClientFormatterSinkProvider());

		ChannelServices.RegisterChannel(channel);
		string serverURL = Server.MakeUri(host, port, useTCP);
		service = (Service)Activator.GetObject(typeof(Service), serverURL);
		session = service.StartSession(collectionId);
		Log.Spew("connected to server at {0}", serverURL);
	}

	public void Stop()
	{
		//session.Done();
		session = null;
		service = null;
		if (channel != null)
		{
			ChannelServices.UnregisterChannel(channel);
			channel = null;
		}
	}
}

//---------------------------------------------------------------------------
/// <summary>
/// command line execution of sync operations
/// </summary>
public class SyncCmd
{
	Uri storeLocation = null;
	int port = 8088;
	bool useTCP = true;
	string host = MyDns.GetHostName();

	int RunSync(Uri docRoot, string serverStoreLocation)
	{
		Store store = Store.Connect(storeLocation, "SyncCmd");
		Collection c = FileInviter.FindCollection(store, docRoot);
		if (c == null)
		{
			Log.Error("Could not find collection {0}", docRoot);
			return 2;
		}

		SyncCollection csc = new SyncCollection(c);
		if (serverStoreLocation != null)
		{
			SyncStore servStore = new SyncStore(serverStoreLocation);
			SynkerServiceA ssa = new SynkerServiceA(servStore.OpenCollection(csc.ID));
			new SynkerWorkerA(ssa, csc).DoSyncWork();
		}
		else
		{
			Client client = new Client(csc.Host, csc.Port, csc.ID, useTCP);
			new SynkerWorkerA(client.session, csc).DoSyncWork();
			client.Stop();
		}
		return 0;
	}

	//TODO: doesn't specify user -- just use collection owner
	int Invite(Uri docRoot, string invitationFile)
	{
		FileInviter fi = new FileInviter(storeLocation);
		if (!fi.Create(docRoot, invitationFile))
		{
			fi.InitMaster(host, port, docRoot);
			if (!fi.Create(docRoot, invitationFile))
			{
				Log.Error("could not make invitation");
				return 3;
			}
		}
		return 0;
	}

	int Accept(string invitationFile, string docRootParent)
	{
		FileInviter fi = new FileInviter(storeLocation);
		return fi.Accept(docRootParent, invitationFile)? 0: 4;
	}

	int RunServer()
	{
		Server server = new Server(host, port, storeLocation, useTCP);
		Console.WriteLine("server {0} started, press enter to exit", port);
		Console.ReadLine();
		server.Stop();
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
				//else if (s == "-u")
				//	userName = args[i++];
				//else if (s == "-w")
				//	credential = args[i++];
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
