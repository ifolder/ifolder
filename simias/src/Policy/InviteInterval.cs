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
	/// Implements the invitation synchronization interval policy.
	/// </summary>
	public class InviteInterval
	{
		#region Class Members
		/// <summary>
		/// Well known name for the invite interval policy.
		/// </summary>
		static public string InviteIntervalPolicyID = "e71b7173-e7a1-4ccf-9630-d452fb163bff";

		/// <summary>
		/// Well known name for the invite interval policy description.
		/// </summary>
		static public string InviteIntervalShortDescription = "Invite Interval Setting";

		/// <summary>
		/// Tag used to lookup and store the interval value on the policy.
		/// </summary>
		static private readonly string IntervalTag = "Interval";

		/// <summary>
		/// Tags used to look up values in the configuration file.
		/// </summary>
		static private readonly string SectionTag = "ServiceManager";
		static private readonly string KeyTag = "InviteInterval";

		/// <summary>
		/// Implies to never synchronize.
		/// </summary>
		static public readonly int InfiniteSyncInterval = -1;

		/// <summary>
		/// The default and minimum intervals that are used to sync invitations.
		/// </summary>
		static private readonly int DefaultInviteInterval = 60;
		static private readonly int MinimumInviteInterval = 5;

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
				int interval = GetDefaultIntervalValue();

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
		private InviteInterval( Policy policy )
		{
			this.policy = policy;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the default invite interval value.
		/// </summary>
		/// <returns>The default invite interval.</returns>
		static private int GetDefaultIntervalValue()
		{
			int interval = DefaultInviteInterval;

			// See if there is an override for default invite interval in the configuration file.
			Configuration config = Configuration.GetConfiguration();
			if ( config.Exists( SectionTag, KeyTag ) )
			{
				string configValue = config.Get( SectionTag, KeyTag, null );
				if ( configValue != null )
				{
					interval = Convert.ToInt32( configValue );
					if ( ( interval != InfiniteSyncInterval ) && ( interval < MinimumInviteInterval ) )
					{
						interval = DefaultInviteInterval;
					}
				}
			}

			return interval;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Creates a system wide invite interval policy.
		/// </summary>
		/// <param name="domainID">Domain that the interval will be associated with.</param>
		/// <param name="interval">Invite interval in seconds that all users in the domain will be set to.</param>
		static public void Create( string domainID, int interval )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( InviteIntervalPolicyID, domainID );
			if ( ( interval == InfiniteSyncInterval ) || ( interval > 0 ) )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( InviteIntervalPolicyID, InviteIntervalShortDescription );
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
		/// Deletes a system wide invite interval policy.
		/// </summary>
		/// <param name="domainID">Domain that the interval will be associated with.</param>
		static public void Delete( string domainID )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( InviteIntervalPolicyID, domainID );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Gets the aggregate invite interval for the specified member.
		/// </summary>
		/// <param name="member">Member that policy is associated with.</param>
		/// <returns>An InviteInterval object that contains the policy for the specified member.</returns>
		static public InviteInterval Get( Member member )
		{
			// Get the aggregate policy.
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetAggregatePolicy( InviteIntervalPolicyID, member );
			return new InviteInterval( policy );
		}

		/// <summary>
		/// Gets the aggregate invite interval for the specified member and collection.
		/// </summary>
		/// <param name="member">Member that policy is associated with.</param>
		/// <param name="collection">Collection to add to the aggregate policy.</param>
		/// <returns>An InviteInterval object that contains the policy for the specified member.</returns>
		static public InviteInterval Get( Member member, Collection collection )
		{
			// Get the aggregate policy.
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetAggregatePolicy( InviteIntervalPolicyID, member, collection );
			return new InviteInterval( policy );
		}

		/// <summary>
		/// Gets the interval associated with the specified domain.
		/// </summary>
		/// <param name="domainID">Domain that the interval is associated with.</param>
		/// <returns>The sync interval that all users in the domain are limited to.</returns>
		static public int GetInterval( string domainID )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( InviteIntervalPolicyID, domainID );
			return ( policy != null ) ? ( int )policy.GetValue( IntervalTag ) : GetDefaultIntervalValue();
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
		#endregion
	}
}
