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

using Simias;
using Simias.Storage;

namespace Simias.Policy
{
	/// <summary>
	/// Implements the synchronization interval policy.
	/// </summary>
	public class SyncInterval
	{
		#region Class Members
		/// <summary>
		/// Well known name for the sync interval policy.
		/// </summary>
		static public readonly string SyncIntervalPolicyID = "bee14cb3-f323-40cb-a948-44a0c3275f2f";

		/// <summary>
		/// Well known name for the sync interval policy description.
		/// </summary>
		static public readonly string SyncIntervalShortDescription = "Sync Interval Setting";

		/// <summary>
		/// Tag used to lookup and store the interval value on the policy.
		/// </summary>
		static private readonly string IntervalTag = "Interval";

		/// <summary>
		/// Implies to never synchronize.
		/// </summary>
		static public readonly int InfiniteSyncInterval = -1;

		/// <summary>
		/// Used to hold the aggregate policy.
		/// </summary>
		private Policy policy;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the sync interval in seconds. If the policy is aggregated, the largest
		/// sync interval will be returned.
		/// </summary>
		public int Interval
		{
			get
			{
				// Set to the default interval.
				int interval = 0;

				// If there is a policy find the greatest interval.
				if ( policy != null )
				{
					// Get the initial interval.
					object objValue = policy.GetValue( IntervalTag );
					if ( objValue != null )
					{
						interval = ( int )objValue;
						foreach ( PolicyValue pv in policy.Values )
						{
							// Make sure that it is the right tag.
							if ( pv.Name == IntervalTag )
							{
								int policyInterval = ( int )pv.Value;
								if ( policyInterval != InfiniteSyncInterval )
								{
									if ( policyInterval > interval )
									{
										interval = policyInterval;
									}
								}
								else
								{
									interval = policyInterval;
									break;
								}
							}
						}
					}
				}

				return interval;
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object.
		/// </summary>
		/// <param name="policy">The aggregate policy. This may be null if no policy exists.</param>
		private SyncInterval( Policy policy )
		{
			this.policy = policy;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Creates a system wide sync interval policy.
		/// </summary>
		/// <param name="domainID">Domain that the interval will be associated with.</param>
		/// <param name="interval">Sync interval in seconds that all users in the domain will be set to.</param>
		static public void Create( string domainID, int interval )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( SyncIntervalPolicyID, domainID );
			if ( ( interval == InfiniteSyncInterval ) || ( interval > 0 ) )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( SyncIntervalPolicyID, SyncIntervalShortDescription );
				}

				// Add the new value and save the policy.
				policy.AddValue( IntervalTag, interval );
				pm.CommitPolicy( policy, domainID );
			}
			else if ( policy != null )
			{
				// Setting the interval to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Creates a sync interval policy for the specified member.
		/// </summary>
		/// <param name="member">Member that the filter will be associated with.</param>
		/// <param name="interval">Sync interval in seconds that all users in the domain will be set to.</param>
		static public void Create( Member member, int interval )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( SyncIntervalPolicyID, member );
			if ( ( interval == InfiniteSyncInterval ) || ( interval > 0 ) )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( SyncIntervalPolicyID, SyncIntervalShortDescription );
				}

				// Add the new value and save the policy.
				policy.AddValue( IntervalTag, interval );
				pm.CommitPolicy( policy, member );
			}
			else if ( policy != null )
			{
				// Setting the interval to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Creates a sync interval policy for the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the filter will be associated with.</param>
		/// <param name="interval">Sync interval in seconds that all users in the domain will be set to.</param>
		static public void Create( Collection collection, int interval )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( SyncIntervalPolicyID, collection );
			if ( ( interval == InfiniteSyncInterval ) || ( interval > 0 ) )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( SyncIntervalPolicyID, SyncIntervalShortDescription );
				}

				// Add the new value and save the policy.
				policy.AddValue( IntervalTag, interval );
				pm.CommitPolicy( policy, collection );
			}
			else if ( policy != null )
			{
				// Setting the interval to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Creates a sync interval policy for the current user on the current machine.
		/// </summary>
		/// <param name="interval">Sync interval in seconds that all users in the domain will be set to.</param>
		static public void Create( int interval )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( SyncIntervalPolicyID );
			if ( ( interval == InfiniteSyncInterval ) || ( interval > 0 ) )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( SyncIntervalPolicyID, SyncIntervalShortDescription );
				}

				// Add the new value and save the policy.
				policy.AddValue( IntervalTag, interval );
				pm.CommitLocalMachinePolicy( policy );
			}
			else if ( policy != null )
			{
				// Setting the interval to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a system wide sync interval policy.
		/// </summary>
		/// <param name="domainID">Domain that the interval will be associated with.</param>
		static public void Delete( string domainID )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( SyncIntervalPolicyID, domainID );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a sync interval policy for the specified member.
		/// </summary>
		/// <param name="member">Member that the filter will be associated with.</param>
		static public void Delete( Member member )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( SyncIntervalPolicyID, member );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a sync interval policy for the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the filter will be associated with.</param>
		static public void Delete( Collection collection )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( SyncIntervalPolicyID, collection );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a sync interval policy for the current user on the current machine.
		/// </summary>
		static public void Delete()
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( SyncIntervalPolicyID );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Gets the aggregate sync interval for the specified member.
		/// </summary>
		/// <param name="member">Member that policy is associated with.</param>
		/// <returns>A SyncInterval object that contains the policy for the specified member.</returns>
		static public SyncInterval Get( Member member )
		{
			// Get the aggregate policy.
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetAggregatePolicy( SyncIntervalPolicyID, member );
			return new SyncInterval( policy );
		}

		/// <summary>
		/// Gets the aggregate sync interval for the specified member and collection.
		/// </summary>
		/// <param name="member">Member that policy is associated with.</param>
		/// <param name="collection">Collection to add to the aggregate policy.</param>
		/// <returns>A SyncInterval object that contains the policy for the specified member.</returns>
		static public SyncInterval Get( Member member, Collection collection )
		{
			// Get the aggregate policy.
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetAggregatePolicy( SyncIntervalPolicyID, member, collection );
			return new SyncInterval( policy );
		}

		/// <summary>
		/// Gets the interval associated with the specified domain.
		/// </summary>
		/// <param name="domainID">Domain that the interval is associated with.</param>
		/// <returns>The sync interval that all users in the domain are limited to.</returns>
		static public int GetInterval( string domainID )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( SyncIntervalPolicyID, domainID );
			return ( policy != null ) ? ( int )policy.GetValue( IntervalTag ) : 0;
		}

		/// <summary>
		/// Gets the sync interval associated with the specified member.
		/// </summary>
		/// <param name="member">Member that the interval is associated with.</param>
		/// <returns>The sync interval that the member is limited to.</returns>
		static public int GetInterval( Member member )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( SyncIntervalPolicyID, member );
			return ( policy != null ) ? ( int )policy.GetValue( IntervalTag ) : 0;
		}

		/// <summary>
		/// Gets the sync interval associated with the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the interval is associated with.</param>
		/// <returns>The sync interval that all users in the collection are limited to.</returns>
		static public int GetInterval( Collection collection )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( SyncIntervalPolicyID, collection );
			return ( policy != null ) ? ( int )policy.GetValue( IntervalTag ) : 0;
		}

		/// <summary>
		/// Gets the sync interval associated with the current user on the current machine.
		/// </summary>
		/// <returns>The sync interval that the current user is limited to.</returns>
		static public int GetInterval()
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( SyncIntervalPolicyID );
			return ( policy != null ) ? ( int )policy.GetValue( IntervalTag ) : 0;
		}

		/// <summary>
		/// Sets the sync interval associated with the specified domain.
		/// </summary>
		/// <param name="domainID">Domain that the interval is associated with.</param>
		/// <param name="interval">Sync interval in seconds that all users in the domain will be set to.</param>
		static public void Set( string domainID, int interval )
		{
			Create( domainID, interval );
		}

		/// <summary>
		/// Sets the sync interval associated with the specified member.
		/// </summary>
		/// <param name="member">Member that the filter is associated with.</param>
		/// <param name="interval">Sync interval in seconds that the associated member will be set to.</param>
		static public void Set( Member member, int interval )
		{
			Create( member, interval );
		}

		/// <summary>
		/// Sets the sync interval associated with the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the policy is associated with.</param>
		/// <param name="interval">Sync interval in seconds that the collection will be set to.</param>
		static public void Set( Collection collection, int interval )
		{
			Create( collection, interval );
		}

		/// <summary>
		/// Sets the sync interval associated with the current user on the current machine.
		/// </summary>
		/// <param name="interval">Sync interval in seconds that the current user will be set to.</param>
		static public void Set( int interval )
		{
			Create( interval );
		}
		#endregion
	}
}
