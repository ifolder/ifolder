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
using Simias.Agent;

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
	public static Collection FindCollection(Store store, Uri docRoot)
	{
		foreach (Collection c in store.GetCollectionsByType(Dredger.NodeTypeDir))
			if (c.DocumentRoot != null && c.DocumentRoot.LocalPath == docRoot.LocalPath)
				return c;
		return null;
	}

	/* TODO: this method uses store.CreateCollection rather than via SyncStore
	 * so that we can pass in a type. Should we ensure that multiple
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

//===========================================================================
}
