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
		store.CurrentIdentity.CreateAlias(invitation.Domain, invitation.Identity, invitation.PublicKey);
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
			c = store.CreateCollection(di.Name, Dredger.NodeTypeDir, docRoot);
			sc = new SyncCollection(c);
			sc.Port = port;
			sc.Host = host;
			sc.Commit();
			Log.Assert(c.Id == c.CollectionNode.Id && c.Id == sc.ID);
			Log.Spew("Created new master collection for {0}, id {1}, {2}:{3}", docRoot.LocalPath, c.Id, sc.Host, sc.Port);
		}
		Invitation invitation = sc.CreateInvitation(user == null? store.CurrentUser: user);
		invitation.Domain = store.DomainName;
		invitation.Save(fileName);
		return true;
	}
}

//---------------------------------------------------------------------------
public class CmdService: MarshalByRefObject
{
	private Uri storeLocation;

	public CmdService(Uri storeLocation)
	{
		this.storeLocation = storeLocation;
	}

	public SynkerServiceA StartSession(string collectionId)
	{
		try
		{
			SyncStore store = new SyncStore(storeLocation.LocalPath);
			SyncCollection c = store.OpenCollection(collectionId);
			return c == null? null: new SynkerServiceA(c, true);
		}
		catch (Exception e) { Log.Uncaught(e); }
		return null;
	}
}

//---------------------------------------------------------------------------
public class CmdServer
{
	CmdService obj = null;
	IChannel channel = null;
	ObjRef objRef = null;
	string uri = null;

	const string serviceTag = "sync.rem";
	const string channelName = "SyncCmdServer";

	public static string MakeUri(string host, int port, bool useTCP)
	{
		return String.Format("{0}://{1}:{2}/{3}",
				(useTCP? "tcp": "http"), host, port, serviceTag);
	}

	public CmdServer(string host, int port, Uri storeLocation, bool useTCP)
	{
		uri = MakeUri(host, port, useTCP);
		obj = new CmdService(storeLocation);
		channel = useTCP? (IChannel)new TcpServerChannel(channelName, port):
				(IChannel)new HttpServerChannel(channelName, port);
		ChannelServices.RegisterChannel(channel);
		objRef = RemotingServices.Marshal(obj, serviceTag);
		Log.Info("CmdServer {0} is up and running from store '{1}'", uri, storeLocation);
	}

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

	~CmdServer() { Stop(); }
}

//---------------------------------------------------------------------------
public class CmdClient
{
	CmdService service = null;
	IChannel channel = null;
	public SynkerServiceA session = null;

	const string channelName = "SyncCmdClient";

	public CmdClient(string host, int port, string collectionId, bool useTCP)
	{
		if (useTCP)
			channel = new TcpClientChannel(channelName, new BinaryClientFormatterSinkProvider());
		else
			channel = new HttpClientChannel(channelName, new SoapClientFormatterSinkProvider());

		ChannelServices.RegisterChannel(channel);
		string serverURL = CmdServer.MakeUri(host, port, useTCP);
		service = (CmdService)Activator.GetObject(typeof(CmdService), serverURL);
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

	public static bool RunOnce(Uri storeLocation, Uri docRoot, string serverStoreLocation, bool useTCP)
	{
		Store store = Store.Connect(new Configuration(
				storeLocation == null? null: storeLocation.LocalPath));
		Collection c = FileInviter.FindCollection(store, docRoot);
		if (c == null)
			return false;

		SyncCollection csc = new SyncCollection(c);
		if (serverStoreLocation != null)
		{
			SyncStore servStore = new SyncStore(serverStoreLocation);
			SyncCollection servCollection = servStore.OpenCollection(csc.ID);
			Log.Spew("server collection {0}", servCollection == null? "null": servCollection.ID);
			SynkerServiceA ssa = new SynkerServiceA(servCollection);
			new SynkerWorkerA(ssa, csc).DoSyncWork();
		}
		else
		{
			CmdClient client = new CmdClient(csc.Host, csc.Port, csc.ID, useTCP);
			new SynkerWorkerA(client.session, csc).DoSyncWork();
			client.Stop();
		}
		return true;
	}
}

//===========================================================================
}
