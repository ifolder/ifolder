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
using Simias.Domain;

namespace Simias.Sync
{
	/// <summary>
	/// A sync wrapper for collection objects.  The wrapper contains property names (scheme) used
	/// by syncing and implements serveral common tasks used by syncing on collections.
	/// </summary>
	public class SyncCollection : Collection, IDisposable
	{
		/// <summary>
		/// A collection property name for the sync role of the collection.
		/// </summary>
		public static readonly string RolePropertyName = "Sync Role";

		/// <summary>
		/// A collection property name for the master URL of the collection.
		/// </summary>
		public static readonly string MasterUrlPropertyName = "Master Url";
		
		/// <summary>
		/// A collection property name for the URL of the domain service.
		/// </summary>
		public static readonly string DomainUrlPropertyName = "Domain Service URL";
		
		/// <summary>
		/// Does the master collection need to be created?
		/// </summary>
		public static readonly string CreateMasterPropertyName = "Create Master Collection";
		
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

		private SyncProperties props;

		/// <summary>
		/// Copy Constructor
		/// </summary>
		/// <param name="collection">The collection object.</param>
		public SyncCollection(Collection collection) : base	(collection)
		{
			props = new SyncProperties(this.StoreReference.Config);
		}

		/// <summary>
		/// Invitation Constructor
		/// </summary>
		/// <param name="store">The store object.</param>
		/// <param name="invitation">An invitation object.</param>
		/// <remarks>The collection is originally created with local ownership.</remarks>
		public SyncCollection(Store store, Invitation invitation)
			: base(store, invitation.CollectionName, invitation.CollectionID,
					store.CurrentUserGuid, store.LocalDomain)
		{
			// add any secret to the current identity chain
			if ((invitation.PublicKey != null) && (invitation.PublicKey.Length > 0))
			{
				BaseContact identity = store.CurrentIdentity;
				identity.CreateAlias(invitation.Domain, invitation.Identity, invitation.PublicKey);
				store.GetLocalAddressBook().Commit(identity);
			}

			this.MasterUrl = invitation.MasterUri;
			this.Role = SyncCollectionRoles.Slave;
			
			// commit
			Commit();

			// check for a dir node
			if (((invitation.DirNodeID != null) && (invitation.DirNodeID.Length > 0))
				&& (invitation.DirNodeName != null) && (invitation.DirNodeName.Length > 0)
				&& (invitation.RootPath != null) && (invitation.RootPath.Length > 0))
			{
				string path = Path.Combine(invitation.RootPath, invitation.DirNodeName);

				DirNode dn = new DirNode(this, path, invitation.DirNodeID);

				if (!Directory.Exists(path)) Directory.CreateDirectory(path);

				Commit(dn);
			}

			props = new SyncProperties(this.StoreReference.Config);
		}

		/// <summary>
		/// Create an invitation object for the given identity.
		/// </summary>
		/// <param name="identity">The identity for the invitation.</param>
		/// <returns>A new invitation object.</returns>
		public Invitation CreateInvitation(string identity)
		{
			// create the invitation
			Invitation invitation = new Invitation();

			invitation.CollectionID = ID;
			invitation.CollectionName = Name;
			invitation.Owner = Owner;
			invitation.Domain = Domain;
			invitation.MasterUri = MasterUrl;
			invitation.Identity = identity;
			invitation.CollectionRights = GetUserAccess(identity).ToString();
			invitation.PublicKey = StoreReference.ServerPublicKey.ToXmlString(false);

			// check for a dir node
			DirNode dn = this.GetRootDirectory();

			if (dn != null)
			{
				invitation.DirNodeID = dn.ID;
				invitation.DirNodeName = dn.Name;
			}

			return invitation;
		}

		/// <summary>
		/// Get a property value from the base node.
		/// </summary>
		/// <param name="name">The name of the property.</param>
		/// <returns>The value of the property.</returns>
		private object GetProperty(string name)
		{
			return GetProperty(name, null);
		}

		/// <summary>
		/// Get a poperty value from the base node.
		/// </summary>
		/// <param name="name">The name of the property.</param>
		/// <param name="value">A default value to return if the property has no value.</param>
		/// <returns>The property value, if it exists, or the default value.</returns>
		private object GetProperty(string name, object value)
		{
			object result = value;

			// refresh the collection to get the latest information
			this.Refresh();

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
		private void SetProperty(string name, object value)
		{
			SetProperty(name, value, false);
		}

		/// <summary>
		/// Set the value of the given property of the base node.
		/// </summary>
		/// <param name="name">The property name.</param>
		/// <param name="value">The new property value.</param>
		/// <param name="local">Is this a local only property? (non-synced)</param>
		private void SetProperty(string name, object value, bool local)
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

		#region IDisposable Members

		public void Dispose()
		{
			// dispose store properties
			if (props != null)
			{
				props.Dispose();
				props = null;
			}
		}

		#endregion

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
					DomainAgent agent = new DomainAgent(this.StoreReference.Config);

					if (Synchronizable && (agent.ServiceUrl != null))
					{
						role = SyncCollectionRoles.Slave;
						DomainUrl = agent.ServiceUrl;
						CreateMaster = true;
					}
					else
					{
						// note: slave collections are always marked by the invitation
						role = Synchronizable ? SyncCollectionRoles.Master : SyncCollectionRoles.Local;
					}

					// save
					Role = role;
				}

				return role;
			}

			set	{ SetProperty(RolePropertyName, value, true); }
		}

		/// <summary>
		/// The syncing URL of the master collection.
		/// </summary>
		public Uri MasterUrl
		{
			get
			{
				Uri result = (Uri)GetProperty(MasterUrlPropertyName);

				if (result == null)
				{
					// default
					UriBuilder ub = new UriBuilder("http", props.Host, props.Port);
					result = ub.Uri;
				}

				return result;
			}
			
			set { SetProperty(MasterUrlPropertyName, value, true); }
		}

		/// <summary>
		/// The URL of the domain service.
		/// </summary>
		public Uri DomainUrl
		{
			get { return (Uri)GetProperty(DomainUrlPropertyName); }
			set { SetProperty(DomainUrlPropertyName, value, true); }
		}

		/// <summary>
		/// Does the master collection need to be created?
		/// </summary>
		public bool CreateMaster
		{
			get { return (bool)GetProperty(CreateMasterPropertyName, false); }
			set { SetProperty(CreateMasterPropertyName, value, true); }
		}

		/// <summary>
		/// The syncing interval of the collection.
		/// </summary>
		public int Interval
		{
			get
			{
				int result = (int)GetProperty(IntervalPropertyName, 0);

				if (result < 1)
				{
					// default
					result = props.Interval;
				}

				return result;
			}

			set { SetProperty(IntervalPropertyName, value, true); }
		}

		/// <summary>
		/// The syncing URL for the collection store service.
		/// </summary>
		public string ServiceUrl
		{
			get
			{
				UriBuilder uri = new UriBuilder(MasterUrl);
				
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
