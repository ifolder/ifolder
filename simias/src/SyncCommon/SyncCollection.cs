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
using Simias.Policy;

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

					// TODO: this should be set when the collection is being created
					if (Synchronizable && agent.Enabled)
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
					result = SimiasRemoting.GetServiceUrl(SyncStoreService.EndPoint);
				}

				return result;
			}
			
			set
			{
				SetProperty(MasterUrlPropertyName, value, true); }
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
				// get the policy sync interval
				return SyncInterval.Get(GetCurrentMember(), this).Interval;
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
