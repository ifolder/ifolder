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
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;

using Simias;
using Simias.Storage;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
/// <summary>
/// class to handle invitation generation and acceptance via files
/// TODO: this class does not belong here, but is useful for
/// test and command line utilities. Should be merged with other, common code.
/// </summary>
public class FileInviter
{
	Configuration config;
    Store store;

	/// <summary>
	/// public constructor when specifying store location
	/// </summary>
	public FileInviter(Uri storeLocation)
	{
		config = new Configuration(storeLocation.LocalPath);
        store = new Store(config);
		store.Revert();
	}

	// TODO: is the following path comparison correct? should it be case insensitive?
	internal static Collection FindCollection(Store store, Uri docRoot)
	{
		foreach (ShallowNode sn in store)
		{
			Collection c = new Collection(store, sn);
			DirNode dn = c.GetRootDirectory();
			//Log.Spew("Collection {0} {1} {2}", c.Name, c.ID, dn == null? "<null>": dn.GetFullPath(c));
			if ((dn = c.GetRootDirectory()) != null && dn.GetFullPath(c) == docRoot.LocalPath)
				return c;
		}
		return null;
	}

	/// <summary>
	/// Accept an invitation file for user to a collection
	/// </summary>
	public bool Accept(string docRootParent, string fileName)
	{
		/* TODO: the checks in this method that ensure that multiple collections
		 * of the same docRoot or collectionId are not allowed should be
		 * done at a lower level
		 */
		Invitation invitation = new Invitation(fileName);
		docRootParent = Path.GetFullPath(docRootParent);
		invitation.RootPath = docRootParent;

		Uri docRoot = new Uri(Path.Combine(docRootParent, invitation.CollectionName));
		Collection c = FindCollection(store, docRoot);
		if (c != null)
		{
			if ((Nid)c.ID == (Nid)invitation.CollectionID)
				Log.Info("ignoring duplicate invitation for folder {0}", docRoot.LocalPath);
			else
				Log.Error("ignoring invitation, folder {0} already linked to a different collection", docRoot.LocalPath);
			return false;
		}

		if ((c = store.GetCollectionByID(invitation.CollectionID)) != null)
		{
			Log.Warn("Collection already exists rooted in different local folder");
			return false;
		}

		// add the invitation information to the store collection
		SyncCollection sc = new SyncCollection(store, invitation);
		sc.Commit();
		sc.Commit(sc);

		DirNode dn = sc.GetRootDirectory();
		Log.Spew("Created new client collection {0} {1} {2}", sc.Name, sc.ID, dn == null? "<null>": dn.GetFullPath(sc));

		return true;
	}

	/// <summary>
	/// Create an invitation file for user to a collection
	/// </summary>
	public bool Invite(string user, Uri docRoot, string host, int port, string fileName)
	{
		Collection c = FindCollection(store, docRoot);
		SyncCollection sc;
		if (c != null)
			sc = new SyncCollection(c);
		else
		{
			DirectoryInfo di = new DirectoryInfo(docRoot.LocalPath);
			di.Create();
			c = new Collection(store, di.Name);
			DirNode dn = new DirNode(c, docRoot.LocalPath);
			c.Commit(c);
			c.Commit(dn);
			sc = new SyncCollection(c);
			//UriBuilder builder = new UriBuilder("http", host, port);
			UriBuilder builder = new UriBuilder("tcp", host, port);
			sc.MasterUri = builder.Uri;
			sc.Commit();
			Log.Spew("Created new master collection for {0}, id {1}, {2}:{3}",
					docRoot.LocalPath, c.ID, sc.MasterUri.Host, sc.MasterUri.Port);
		}
		Invitation invitation = sc.CreateInvitation(user == null? store.CurrentUserGuid: user);
		invitation.Domain = store.LocalDomain;
		invitation.Save(fileName);
		return true;
	}
}

//---------------------------------------------------------------------------
/// <summary>
/// Classreate an invitation file for user to a collection
/// </summary>
public class CmdService: MarshalByRefObject
{
	Uri storeLocation;

	internal CmdService(Uri storeLocation)
	{
		this.storeLocation = storeLocation;
	}

	/// <summary>
	/// Classreate an invitation file for user to a collection
	/// </summary>
	public SynkerServiceA StartSession(string collectionId)
	{
		try
		{
			Store store = new Store(new Configuration(storeLocation.LocalPath));
			store.Revert();
			Collection c = store.GetCollectionByID(collectionId);
			return c == null? null: new SynkerServiceA(new SyncCollection(c), true);
		}
		catch (Exception e) { Log.Uncaught(e); }
		return null;
	}
}

//---------------------------------------------------------------------------
/// <summary>
/// a simple sync server, used for testing and command line tools
/// </summary>
public class CmdServer
{
	CmdService obj = null;
	IChannel channel = null;
	ObjRef objRef = null;
	string uri = null;

	const string serviceTag = "sync.rem";
	const string channelName = "SyncCmdServer";

	internal static string MakeUri(string host, int port)
	{
		//return String.Format("http://{0}:{1}/{2}", host, port, serviceTag);
		return String.Format("tcp://{0}:{1}/{2}", host, port, serviceTag);
	}

	/// <summary>
	/// Create server on specified port using specified store and remoting channel type.
	/// host is only used to generate a URI for debug messages.
	/// </summary>
	public CmdServer(string host, int port, Uri storeLocation)
	{
		uri = MakeUri(host, port);
		obj = new CmdService(storeLocation);
		//channel = new HttpServerChannel(channelName, port, new BinaryServerFormatterSinkProvider());
		channel = new TcpServerChannel(channelName, port, new BinaryServerFormatterSinkProvider());
		ChannelServices.RegisterChannel(channel);
		objRef = RemotingServices.Marshal(obj, serviceTag);
		Log.Info("CmdServer {0} is up and running from store '{1}'", uri, storeLocation);
	}

	/// <summary>
	/// immediately stop the server, unregister and clean up all resources.
	/// </summary>
	public void Stop()
	{
		if (uri != null)
			Log.Spew("CmdServer {0} stopping", uri);
		if (obj != null)
			RemotingServices.Disconnect(obj);
		if (channel != null)
			ChannelServices.UnregisterChannel(channel);
		obj = null;
		channel = null;
		objRef = null;
		uri = null;
	}

	
	/// <summary>
	/// clean up in case caller did not call Stop()
	/// </summary>
	~CmdServer() { Stop(); }
}

//---------------------------------------------------------------------------
/// <summary>
/// a simple sync client, used for testing and command line tools
/// </summary>
public class CmdClient
{
	CmdService service = null;
	IChannel channel = null;
	SynkerServiceA session = null;

	const string channelName = "SyncCmdClient";

	CmdClient(string host, int port, string collectionId)
	{
		//channel = new HttpClientChannel(channelName, new BinaryClientFormatterSinkProvider());
		channel = new TcpClientChannel(channelName, new BinaryClientFormatterSinkProvider());
		ChannelServices.RegisterChannel(channel);
		string serverURL = CmdServer.MakeUri(host, port);
		service = (CmdService)Activator.GetObject(typeof(CmdService), serverURL);
		session = service.StartSession(collectionId);
		Log.Spew("connected to server at {0}", serverURL);
	}

	void Stop()
	{
		session = null;
		service = null;
		if (channel != null)
		{
			ChannelServices.UnregisterChannel(channel);
			channel = null;
		}
	}

	/// <summary>
	/// instantiates a client and runs one sync pass for the specified collection to the specified server
	/// </summary>
	public static bool RunOnce(Uri storeLocation, Uri docRoot, string serverStoreLocation)
	{
		Store store = new Store(new Configuration(storeLocation == null? null: storeLocation.LocalPath));
		store.Revert();
		Collection c = FileInviter.FindCollection(store, docRoot);
		if (c == null)
		{
			Log.Error("could not find collection for folder: {0}", storeLocation.LocalPath);
			return false;
		}

		SyncCollection csc = new SyncCollection(c);
		if (serverStoreLocation != null)
		{
			Store servStore = new Store(new Configuration(serverStoreLocation));
			servStore.Revert();
			Collection servCollection = servStore.GetCollectionByID(csc.ID);
			Log.Spew("server collection {0}", servCollection == null? "null": servCollection.ID);
			SynkerServiceA ssa = new SynkerServiceA(new SyncCollection(servCollection));
			new SynkerWorkerA(ssa, csc).DoSyncWork();
		}
		else
		{
			CmdClient client = new CmdClient(csc.MasterUri.Host, csc.MasterUri.Port, csc.ID);
			new SynkerWorkerA(client.session, csc).DoSyncWork();
			client.Stop();
		}
		return true;
	}
}

//===========================================================================
}
