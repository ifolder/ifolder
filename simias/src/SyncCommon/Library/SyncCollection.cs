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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Net;

using Simias;
using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// A sync wrapper for collection objects.  The wrapper contains property names (scheme) used
	/// by syncing and implements serveral common tasks used by syncing on collections.
	/// </summary>
	public class SyncCollection : Collection
	{
		/// <summary>
		/// A collection property name for the sync role of the collection.
		/// </summary>
		public static readonly string RolePropertyName = "Sync Role";

		/// <summary>
		/// A collection property name for the master URL of the collection.
		/// </summary>
		public static readonly string MasterUriPropertyName = "Master Uri";
		
		/// <summary>
		/// A collection property name for the sync interval to be used with the collection.
		/// The interval is specified in seconds.
		/// </summary>
		public static readonly string IntervalPropertyName = "Sync Interval";
		
		/// <summary>
		/// A collection property name of the sync logic class and assembly of the collection.
		/// Only collection using the same sync logic can communicate.
		/// </summary>
		public static readonly string LogicTypePropertyName = "Sync Logic";

		/// <summary>
		/// Copy Constructor
		/// </summary>
		/// <param name="collection">The collection object.</param>
		public SyncCollection(Collection collection) : base	(collection)
		{
		}

		/// <summary>
		/// Invitation Constructor
		/// </summary>
		/// <param name="store">The store object.</param>
		/// <param name="invitation">An invitation object.</param>
		public SyncCollection(Store store, Invitation invitation)
			: base(store, invitation.CollectionName, invitation.CollectionID,
					invitation.Owner, invitation.Domain)
		{
			this.MasterUri = invitation.MasterUri;
			this.Role = SyncCollectionRoles.Slave;
			
			// add any secret to the current identity chain
			if ((invitation.PublicKey != null) && (invitation.PublicKey.Length > 0))
			{
				BaseContact identity = store.CurrentIdentity;
				identity.CreateAlias(invitation.Domain, invitation.Identity, invitation.PublicKey);
				store.GetLocalAddressBook().Commit(identity);
			}

			// commit
			Commit();

			// check for a dir node
			if (((invitation.DirNodeID != null) && (invitation.DirNodeID.Length > 0))
				&& (invitation.RootPath != null) && (invitation.RootPath.Length > 0))
			{
				DirNode dn = new DirNode(this, invitation.RootPath, invitation.DirNodeID);

				Commit(dn);
			}
		}

		/// <summary>
		/// Create an invitation object for the given identity.
		/// </summary>
		/// <param name="identity">The identity for the invitation.</param>
		/// <returns>A new invitation object.</returns>
		public Invitation CreateInvitation(string identity)
		{
			// validate the master URL
			if (MasterUri == null)
			{
				throw new ArgumentException("An invitation requires the master URL for the collection.");
			}

			// create the invitation
			Invitation invitation = new Invitation();

			invitation.CollectionID = ID;
			invitation.CollectionName = Name;
			invitation.Owner = Owner;
			invitation.Domain = Domain;
			invitation.MasterUri = MasterUri;
			invitation.Identity = identity;
			invitation.CollectionRights = GetUserAccess(identity).ToString();
			invitation.PublicKey = StoreReference.ServerPublicKey.ToXmlString(false);

			// check for a dir node
			DirNode dn = this.GetRootDirectory();

			if (dn != null)
			{
				// TODO: ?
				// invitation.RootPath = dn.GetFullPath(this);
				invitation.DirNodeID = dn.ID;
			}

			return invitation;
		}

		/// <summary>
		/// Get a property value from the base node.
		/// </summary>
		/// <param name="name">The name of the property.</param>
		/// <returns>The value of the property.</returns>
		public object GetProperty(string name)
		{
			return GetProperty(name, null);
		}

		/// <summary>
		/// Get a poperty value from the base node.
		/// </summary>
		/// <param name="name">The name of the property.</param>
		/// <param name="value">A default value to return if the property has no value.</param>
		/// <returns>The property value, if it exists, or the default value.</returns>
		public object GetProperty(string name, object value)
		{
			object result = value;

			Property p = Properties.GetSingleProperty(name);

			if (p != null)
			{
				result = p.Value;
			}

			return result;
		}

		/// <summary>
		/// Set the value of the given property of the base node.
		/// </summary>
		/// <param name="name">The property name.</param>
		/// <param name="value">The new property value.</param>
		public void SetProperty(string name, object value)
		{
			SetProperty(name, value, false);
		}

		/// <summary>
		/// Set the value of the given property of the base node.
		/// </summary>
		/// <param name="name">The property name.</param>
		/// <param name="value">The new property value.</param>
		/// <param name="local">Is this a local only property? (non-synced)</param>
		public void SetProperty(string name, object value, bool local)
		{
			if (value != null)
			{
				Property p = new Property(name, value);
				p.LocalProperty = local;

				Properties.ModifyProperty(p);
			}
			else
			{
				Properties.DeleteSingleProperty(name);
			}

			Commit();
		}

		#region Properties
		
		/// <summary>
		/// The syncing role of the base collection.
		/// </summary>
		public SyncCollectionRoles Role
		{
			get
			{
				SyncCollectionRoles role;
				
				role = (SyncCollectionRoles)GetProperty(RolePropertyName, SyncCollectionRoles.None);
				
				if (role == SyncCollectionRoles.None)
				{
					// note: slave collections are always marked by the invitation
					role = Synchronizable ? SyncCollectionRoles.Master : SyncCollectionRoles.Local;
				}

				return role;
			}


			set
			{
				SetProperty(RolePropertyName, value, true);
			}
		}

		/// <summary>
		/// The syncing URL of the master collection.
		/// </summary>
		public Uri MasterUri
		{
			get { return (Uri)GetProperty(MasterUriPropertyName); }
			set { SetProperty(MasterUriPropertyName, value, true); }
		}

		/// <summary>
		/// The syncing interval of the collection.
		/// </summary>
		public int Interval
		{
			get { return (int)GetProperty(IntervalPropertyName, -1); }
			set { SetProperty(IntervalPropertyName, value, true); }
		}

		/// <summary>
		/// The syncing logic of the base collection.
		/// </summary>
		public string LogicType
		{
			get { return (string)GetProperty(LogicTypePropertyName); }
			set { SetProperty(LogicTypePropertyName, value, true); }
		}

		/// <summary>
		/// The syncing URL for the collection store service.
		/// </summary>
		public string ServiceUrl
		{
			get
			{
				UriBuilder uri = new UriBuilder(MasterUri);
				
				uri.Path = String.Format("SyncStoreService{0}.rem", uri.Port);

				return uri.ToString();
			}
		}

		/// <summary>
		/// The store path of the base collection.
		/// </summary>
		public string StorePath
		{
			get { return base.StoreReference.StorePath; }
		}

		#endregion
	}
}
