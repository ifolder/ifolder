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
using System.Collections;
using System.Diagnostics;

using Simias.Storage;
using Simias.Identity;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
// a class to hold some related info about a SyncPoint and
// to isolate all interaction with the collection store about
// "Shares", invitations, etc.

//TODO: all the static methods of this class should be replaced Rob's real invitation stuff

public class SyncPoint
{
	public Uri masterUri;
	public string collectionId, userId, credential;

	public SyncPoint(Uri masterUri, string collectionId, string userId, string credential)
	{
		this.masterUri = masterUri;
		this.collectionId = collectionId;
		this.userId = userId;
		this.credential = credential;
	}

	// build current list of targets (Collections to sync)
	public static Hashtable GetTargets(Uri storeLocation)
	{
		Hashtable targets = new Hashtable();
		IIdentity id = IdentityManager.Connect().CurrentId;

		Store store = Store.Connect(storeLocation);
		store.ImpersonateUser(Access.SyncOperatorRole, null);
		
		foreach (Collection coll in store.GetCollectionsByType(Dredger.NodeTypeDir))
		{
			Property p = coll.Properties.GetSingleProperty("MasterUri");
			Uri uri = p == null? null: (Uri)p.Value;
			targets.Add(coll.Id, new SyncPoint(uri, coll.Id, id.UserId, id.Credential));
		}
		Log.Spew("found {0} targets", targets.Count);
		return targets;
	}

	public const string scheme = "nifp";

	//TODO: replace with Rob's real invitation stuff
	public static void InitMaster(Store store, string host, int port, Uri docRoot)
	{
		DirectoryInfo di = new DirectoryInfo(docRoot.LocalPath);
		di.Create();
		Collection coll = store.CreateCollection(di.Name, Dredger.NodeTypeDir, docRoot);
		Uri uri = new Uri(String.Format("{0}://{1}:{2}/{3}", scheme, host, port, coll.Id));
		coll.Properties.AddProperty("MasterUri", uri);
		coll.Commit();

		Log.Spew("Created new master collection for {0}, id {1}", docRoot.LocalPath, coll.Id);
		Debug.Assert(coll.Id == coll.CollectionNode.Id);

		//coll = store.GetCollectionById(coll.Id);
		//Property p = coll.Properties.GetSingleProperty("MasterUri");
		//Uri masterUri = p == null? null: (Uri)p.Value;
		//p = coll.Properties.GetSingleProperty(Property.DocumentRoot);
		//Uri docRootUri = p == null? null: (Uri)p.Value;
		//Log.Spew("created collection: set {0}, got back {1}, for master Uri {2}", docRoot, docRootUri, masterUri);
	}

	//TODO: replace with Rob's real invitation stuff
	public static Uri GetMasterUri(Collection collection)
	{
		Property p = collection.Properties.GetSingleProperty("MasterUri");
		return p == null? null: (Uri)p.Value;
	}


	//TODO: replace with Rob's real invitation stuff
	public static bool FindCollection(Store store, Uri docRoot, out Collection collection)
	{
		foreach (Collection coll in store.GetCollectionsByType(Dredger.NodeTypeDir))
		{
			if (coll.DocumentRoot == null)
				Log.Spew("skipping collection {0}, no DocumentRoot", coll.Name);
			else if (coll.DocumentRoot.LocalPath == docRoot.LocalPath)
			{
				collection = coll;
				return true;
			}
		}
		collection = null;
		return false;
	}

	//TODO: replace with Rob's real invitation stuff
	public static bool FindCollection(Store store, Uri docRoot, out Uri masterUri, out string name, out string id)
	{
		Collection coll;
		if (FindCollection(store, docRoot, out coll))
		{
			masterUri = GetMasterUri(coll);
			Debug.Assert(masterUri != null);
			name = coll.Name;
			id = coll.Id;
			return true;
		}
		masterUri = null;
		name = null;
		id = null;
		return false;
	}

	//TODO: replace with Rob's real invitation stuff
	public static void Accept(Store store, string docRootParent, string invitFile)
	{
		StreamReader s = new StreamReader(File.OpenRead(invitFile));
		Uri masterUri = new Uri(s.ReadLine());
		string userId = s.ReadLine(), collectionName = s.ReadLine();
		s.Close();
		Uri docRoot = new Uri(Path.Combine(Path.GetFullPath(docRootParent), collectionName));
		string serverCollectionId = (new UriBuilder(masterUri)).Path.Trim('/');

		Collection coll = store.GetCollectionById(serverCollectionId);
		if (coll != null)
		{
			Uri oldMasterUri = GetMasterUri(coll);
			if (oldMasterUri == masterUri && docRoot.LocalPath == coll.DocumentRoot.LocalPath)
				Log.Info("ignoring duplicate invitation for folder {0}", docRoot.LocalPath);
			else
			{
				if (docRoot.LocalPath != coll.DocumentRoot.LocalPath)
					Log.Warn("Collection already exists rooted in different local folder: {0}", coll.DocumentRoot.LocalPath);

				if (masterUri != GetMasterUri(coll))
					Log.Warn("Collection already exists with different master Uri: {0}", GetMasterUri(coll));
			}
		}
		else if (!FindCollection(store, docRoot, out coll))
		{
			coll = new Collection(store, collectionName, serverCollectionId, Dredger.NodeTypeDir, docRoot);
			coll.Properties.AddProperty("MasterUri", masterUri);
			coll.Commit();

			Log.Spew("Created new client collection for {0}, id {1}", docRoot.LocalPath, coll.Id);
			Debug.Assert(coll.Id == coll.CollectionNode.Id);
		}
		else
			Log.Error("ignoring invitation, folder already linked to a different collection");
	}

	//TODO: replace with Rob's real invitation stuff
	public static bool Invite(Store store, Uri docRoot, string invitFile)
	{
		Uri masterUri;
		string collectionName, id;
		if (!FindCollection(store, docRoot, out masterUri, out collectionName, out id))
			return false;

		StreamWriter s = new StreamWriter(File.Create(invitFile));
		s.WriteLine(masterUri);
		s.WriteLine(IdentityManager.Connect().CurrentId.UserGuid); //TODO: userId???
		s.WriteLine(collectionName);
		s.Close();
		return true;
	}
}

//===========================================================================
}
