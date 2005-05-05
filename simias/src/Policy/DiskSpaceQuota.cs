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
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;

using Simias;
using Simias.Storage;

namespace Simias.Policy
{
	/// <summary>
	/// Implements the disk space limit policy.
	/// </summary>
	public class DiskSpaceQuota
	{
		#region Class Members
		/// <summary>
		/// Well known name for the disk space quota policy.
		/// </summary>
		static public string DiskSpaceQuotaPolicyID = "d023b45d-9559-49cd-9b9f-3e41e7fdcf0d";

		/// <summary>
		/// Well known name for the disk space quota description.
		/// </summary>
		static public string DiskSpaceQuotaShortDescription = "Disk space quota";

		/// <summary>
		/// Policy object that is used to manage quota.
		/// </summary>
		private Policy policy;

		/// <summary>
		/// Member that is associated with this policy.
		/// </summary>
		private Member member;

		/// <summary>
		/// A snapshot of the amount of spaced used by the member.
		/// </summary>
		private long usedDiskSpace = 0;
		#endregion

		#region Properties
		/// <summary>
		/// Returns the available disk space for the associated member.
		/// </summary>
		public long AvailableSpace
		{
			get 
			{ 
				long sa = Limit - UsedSpace;
				return ( sa > 0 ) ? sa : 0;
			}
		}

		/// <summary>
		/// Gets the disk space quota limit.
		/// </summary>
		public long Limit
		{
			get
			{
				// Set to no limit.
				long limit = 0;

				// If there is a policy find the most restrictive limit.
				if ( policy != null )
				{
					foreach ( Rule rule in policy.Rules )
					{
						long ruleLimit = ( long )rule.Operand;
						if ( ( limit == 0 ) || ( ruleLimit < limit ) )
						{
							limit = ruleLimit;
						}
					}
				}

				return limit;
			}
		}

		/// <summary>
		/// Gets the amount of disk space in use for the associated member.
		/// </summary>
		public long UsedSpace
		{
			get 
			{ 
				usedDiskSpace = GetUsedDiskSpace( member ); 
				return usedDiskSpace;
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of an object.
		/// </summary>
		/// <param name="member">Member that this disk space quota is associated with.</param>
		/// <param name="policy">The aggregate quota policy object.</param>
		private DiskSpaceQuota( Member member, Policy policy )
		{
			this.policy = policy;
			this.member = member;
			this.usedDiskSpace = GetUsedDiskSpace( member );
		}
		#endregion

		#region Factory Methods
		/// <summary>
		/// Creates a system wide disk space quota policy.
		/// </summary>
		/// <param name="domainID">Domain that the limit will be associated with.</param>
		/// <param name="limit">Amount of disk space that all users in the domain will be limited to.</param>
		static public void Create( string domainID, long limit )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, domainID );
			if ( limit > 0 )
			{
				if ( policy == null )
				{
					// The quota policy does not exist, create a new one and add the rule.
					policy = new Policy( DiskSpaceQuotaPolicyID, DiskSpaceQuotaShortDescription );
				}
				else
				{
					// The policy already exists, delete the old rule.
					policy.DeleteRule( DiskSpaceQuota.GetRule( policy ) );
				}

				// Add the new rule and save the policy.
				policy.AddRule( new Rule( limit, Rule.Operation.Greater, Rule.Result.Deny ) );
				pm.CommitPolicy( policy, domainID );
			}
			else if ( policy != null )
			{
				// Setting the limit to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Creates a disk space quota policy for the specified member.
		/// </summary>
		/// <param name="member">Member that the limit will be associated with.</param>
		/// <param name="limit">Amount of disk space that this member will be limited to.</param>
		static public void Create( Member member, long limit )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, member );
			if ( limit > 0 )
			{
				if ( policy == null )
				{
					// The quota policy does not exist, create a new one and add the rule.
					policy = new Policy( DiskSpaceQuotaPolicyID, DiskSpaceQuotaShortDescription );
				}
				else
				{
					// The policy already exists, delete the old rule.
					policy.DeleteRule( DiskSpaceQuota.GetRule( policy ) );
				}

				// Add the new rule and save the policy.
				policy.AddRule( new Rule( limit, Rule.Operation.Greater, Rule.Result.Deny ) );
				pm.CommitPolicy( policy, member );
			}
			else if ( policy != null )
			{
				// Setting the limit to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Creates a disk space quota policy for the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the limit will be associated with.</param>
		/// <param name="limit">Amount of disk space that this collection will be limited to.</param>
		static public void Create( Collection collection, long limit )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, collection );
			if ( limit > 0 )
			{
				if ( policy == null )
				{
					// The quota policy does not exist, create a new one and add the rule.
					policy = new Policy( DiskSpaceQuotaPolicyID, DiskSpaceQuotaShortDescription );
				}
				else
				{
					// The policy already exists, delete the old rule.
					policy.DeleteRule( DiskSpaceQuota.GetRule( policy ) );
				}

				// Add the new rule and save the policy.
				policy.AddRule( new Rule( limit, Rule.Operation.Greater, Rule.Result.Deny ) );
				pm.CommitPolicy( policy, collection );
			}
			else if ( policy != null )
			{
				// Setting the limit to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a system wide disk space quota policy.
		/// </summary>
		/// <param name="domainID">Domain that the limit will be associated with.</param>
		static public void Delete( string domainID )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, domainID );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a disk space quota policy for the specified member.
		/// </summary>
		/// <param name="member">Member that the limit will be associated with.</param>
		static public void Delete( Member member )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, member );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a disk space quota policy for the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the limit will be associated with.</param>
		static public void Delete( Collection collection )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, collection );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Gets the aggregate disk space quota policy for the specified member.
		/// </summary>
		/// <param name="member">Member that quota is associated with.</param>
		/// <returns>A DiskSpaceQuota object that contains the policy for the specified member.</returns>
		static public DiskSpaceQuota Get( Member member )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetAggregatePolicy( DiskSpaceQuotaPolicyID, member );
			return new DiskSpaceQuota( member, policy );
		}

		/// <summary>
		/// Gets the aggregate disk space quota policy for the specified member and collection.
		/// </summary>
		/// <param name="member">Member that quota is associated with.</param>
		/// <param name="collection">Collection to add to the aggregate quota policy.</param>
		/// <returns>A DiskSpaceQuota object that contains the policy for the specified member.</returns>
		[ Obsolete( "This method is obsolete. Please use DiskSpaceQuota.Get( Collection collection ) instead.", false ) ]
		static public DiskSpaceQuota Get( Member member, Collection collection )
		{
			return Get( collection );
		}

		/// <summary>
		/// Gets the aggregate disk space quota policy for the specified collection. This includes
		/// the collection owner's quota, if one exists, or the system-wide quota and any quota
		/// set specifically on the collection.
		/// </summary>
		/// <param name="collection">Collection to add to the aggregate quota policy.</param>
		/// <returns>A DiskSpaceQuota object that contains the policy for the specified member.</returns>
		static public DiskSpaceQuota Get( Collection collection )
		{
			Member owner = collection.Owner;
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetAggregatePolicy( DiskSpaceQuotaPolicyID, owner, collection );
			return new DiskSpaceQuota( owner, policy );
		}

		/// <summary>
		/// Gets the disk space quota limit associated with the specified domain.
		/// </summary>
		/// <param name="domainID">Domain that the limit is associated with.</param>
		/// <returns>Amount of disk space that all users in the domain are limited to.</returns>
		static public long GetLimit( string domainID )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, domainID );
			return ( policy != null ) ? ( long )GetRule( policy ).Operand : 0;
		}

		/// <summary>
		/// Gets the disk space quota limit associated with the specified member.
		/// </summary>
		/// <param name="member">Member that the limit is associated with.</param>
		/// <returns>Amount of disk space that the member is limited to.</returns>
		static public long GetLimit( Member member )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, member );
			return ( policy != null ) ? ( long )GetRule( policy ).Operand : 0;
		}

		/// <summary>
		/// Gets the disk space quota limit associated with the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the limit is associated with.</param>
		/// <returns>Amount of disk space that the collection is limited to.</returns>
		static public long GetLimit( Collection collection )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, collection );
			return ( policy != null ) ? ( long )GetRule( policy ).Operand : 0;
		}

		/// <summary>
		/// Sets the disk space quota limit associated with the specified domain.
		/// </summary>
		/// <param name="domainID">Domain that the limit is associated with.</param>
		/// <param name="limit">Amount of disk space that all users in the domain will be limited to.</param>
		static public void Set( string domainID, long limit )
		{
			Create( domainID, limit );
		}

		/// <summary>
		/// Sets the disk space quota limit associated with the specified member.
		/// </summary>
		/// <param name="member">Member that the limit is associated with.</param>
		/// <param name="limit">Amount of disk space that all users in the domain will be limited to.</param>
		static public void Set( Member member, long limit )
		{
			Create( member, limit );
		}

		/// <summary>
		/// Sets the disk space quota limit associated with the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the limit is associated with.</param>
		/// <param name="limit">Amount of disk space that all users in the domain will be limited to.</param>
		static public void Set( Collection collection, long limit )
		{
			Create( collection, limit );
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the disk space quota rule for the specified policy.
		/// </summary>
		/// <param name="policy">Policy to retrieve the quota rule from.</param>
		/// <returns>The quota Rule from the policy.</returns>
		static private Rule GetRule( Policy policy )
		{
			// There should only be one rule in the quota policy.
			IEnumerator e = policy.Rules.GetEnumerator();
			if ( !e.MoveNext() )
			{
				throw new SimiasException( "No policy rule on quota." );
			}

			return e.Current as Rule;
		}

		/// <summary>
		/// Returns the total amount of disk space used by this member.
		/// </summary>
		/// <param name="member">Member to find used disk space for.</param>
		/// <returns>Amount of used disk space in bytes.</returns>
		private long GetUsedDiskSpace( Member member )
		{
			long collectionSpace = 0;

			// Get all of the collections that are owned by the member.
			Store store = Store.GetStore();
			ICSList collectionList = store.GetCollectionsByOwner( member.UserID, member.GetDomainID( store ) );
			foreach ( ShallowNode sn in collectionList )
			{
				Collection c = new Collection( store, sn );
				collectionSpace += c.StorageSize;
			}

			return collectionSpace;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns whether there is sufficient disk space quota for the requested space.
		/// </summary>
		/// <param name="space">Amount of space requested.</param>
		/// <returns>True if the requested space is under the quota limit. Otherwise false is returned.</returns>
		public bool Allowed( long space )
		{
			bool hasSpace = true;

			if ( policy != null )
			{
				// Apply the rule to see if there is space available.
				Rule.Result result = policy.Apply( usedDiskSpace + space );
				if ( result == Rule.Result.Allow )
				{
					// Update the snapshot.
					usedDiskSpace += space;
				}
				else
				{
					hasSpace = false;
				}
			}

			return hasSpace;
		}
		#endregion
	}
}
