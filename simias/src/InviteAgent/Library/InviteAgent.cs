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
 *  Author: Rob
 * 
 ***********************************************************************/

using System;
using System.IO;
using System.Web.Mail;
using System.Net;
using System.Text;

using Simias;
using Simias.Agent;
using Simias.Storage;
using Simias.Sync;

namespace Simias.Agent
{
	/// <summary>
	/// Invitation Agent Base Class
	/// </summary>
	public abstract class InviteAgent : IInviteAgent
	{
		private string storePath;

		/// <summary>
		/// Generate an inviation for a user to a collection
		/// </summary>
		/// <param name="collection">The collection object</param>
		/// <param name="identity">The user identity</param>
		/// <returns>The generated invitation object</returns>
		public Invitation CreateInvitation(Collection collection, string identity)
		{
			// open the store
			SyncStore syncStore = new SyncStore(storePath);

			// open the collection
			SyncCollection syncCollection = syncStore.OpenCollection(collection.Id);

			// generate the invitation
			Invitation invitation = syncCollection.CreateInvitation(identity);

			return invitation;
		}

		/// <summary>
		/// Invite a user to a collection
		/// </summary>
		/// <param name="invitation">The invitation</param>
		public abstract void Invite(Invitation invitation);
		
		/// <summary>
		/// Accept a collection on this machine
		/// </summary>
		/// <param name="invitation">The invitation</param>
		public void Accept(Invitation invitation)
		{
			// default local path ?
			if ((invitation.RootPath == null) || (invitation.RootPath.Length == 0))
			{
				invitation.RootPath = Invitation.DefaultRootPath;
			}
			
			// add the invitation information to the store collection
			SyncStore store = new SyncStore(storePath);

			// add the secret to the current identity chain
			Identity identity = store.BaseStore.CurrentIdentity;
			identity.CreateAlias(invitation.Domain, invitation.Identity, invitation.PublicKey);
			identity.Commit();
				
			// create the slave collection with the invitation
			SyncCollection collection = store.CreateCollection(invitation);

			// save the new collection
			collection.Commit();

			// TODO: Create a public/private pair and post the public key
			// to the collection source

			MyTrace.WriteLine("Invitation Accepted: {0}", invitation);
		}

		#region Properties

		/// <summary>
		/// The store path
		/// </summary>
		public string StorePath
		{
			get { return storePath; }
			
			set
			{
				if (value == null)
				{
					// a null store path is considered a default by collection store
					storePath = null;
				}
				else
				{
					storePath = Path.GetFullPath(value);
				}
			}
		}

		#endregion
	}
}
