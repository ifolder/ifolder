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
		/// Store handle to the collection store.
		/// </summary>
		private Store store;

		/// <summary>
		/// Used to manage the policy.
		/// </summary>
		private PolicyManager manager;

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
				long limit = long.MaxValue;

				// If there is a policy find the most restrictive limit.
				if ( policy != null )
				{
					foreach ( Rule rule in policy.Rules )
					{
						long ruleLimit = ( long )rule.Operand;
						if ( ruleLimit < limit )
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
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that this disk space quota is associated with.</param>
		/// <param name="manager">Policy manager object to control the quota policy.</param>
		/// <param name="policy">The aggregate quota policy object.</param>
		private DiskSpaceQuota( Store store, Member member, PolicyManager manager, Policy policy )
		{
			this.store = store;
			this.manager = manager;
			this.policy = policy;
			this.member = member;
			this.usedDiskSpace = GetUsedDiskSpace( member );
		}
		#endregion

		#region Factory Methods
		/// <summary>
		/// Creates a system wide disk space quota policy.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="domainID">Domain that the limit will be associated with.</param>
		/// <param name="limit">Amount of disk space that all users in the domain will be limited to.</param>
		static public void Create( Store store, string domainID, long limit )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, domainID );
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
			policy.AddRule( new Rule( limit, Rule.Operation.Less_Equal, Rule.Result.Allow ) );
			pm.CommitPolicy( policy, domainID );
		}

		/// <summary>
		/// Creates a disk space quota policy for the specified member.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that the limit will be associated with.</param>
		/// <param name="limit">Amount of disk space that this member will be limited to.</param>
		static public void Create( Store store, Member member, long limit )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, member );
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
			policy.AddRule( new Rule( limit, Rule.Operation.Less_Equal, Rule.Result.Allow ) );
			pm.CommitPolicy( policy, member );
		}

		/// <summary>
		/// Creates a disk space quota policy for the specified collection.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="collection">Collection that the limit will be associated with.</param>
		/// <param name="limit">Amount of disk space that this collection will be limited to.</param>
		static public void Create( Store store, Collection collection, long limit )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, collection );
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
			policy.AddRule( new Rule( limit, Rule.Operation.Less_Equal, Rule.Result.Allow ) );
			pm.CommitPolicy( policy, collection );
		}

		/// <summary>
		/// Deletes a system wide disk space quota policy.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="domainID">Domain that the limit will be associated with.</param>
		static public void Delete( Store store, string domainID )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
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
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that the limit will be associated with.</param>
		static public void Delete( Store store, Member member )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
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
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="collection">Collection that the limit will be associated with.</param>
		static public void Delete( Store store, Collection collection )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, collection );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Gets the disk space quota limit associated with the specified domain.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="domainID">Domain that the limit is associated with.</param>
		/// <returns>Amount of disk space that all users in the domain are limited to.</returns>
		static public long GetLimit( Store store, string domainID )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, domainID );
			return ( policy != null ) ? ( long )DiskSpaceQuota.GetRule( policy ).Operand : long.MaxValue;
		}

		/// <summary>
		/// Gets the disk space quota limit associated with the specified member.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that the limit is associated with.</param>
		/// <returns>Amount of disk space that the member is limited to.</returns>
		static public long GetLimit( Store store, Member member )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, member );
			return ( policy != null ) ? ( long )DiskSpaceQuota.GetRule( policy ).Operand : long.MaxValue;
		}

		/// <summary>
		/// Gets the disk space quota limit associated with the specified collection.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="collection">Collection that the limit is associated with.</param>
		/// <returns>Amount of disk space that the collection is limited to.</returns>
		static public long GetLimit( Store store, Collection collection )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, collection );
			return ( policy != null ) ? ( long )DiskSpaceQuota.GetRule( policy ).Operand : long.MaxValue;
		}

		/// <summary>
		/// Gets the aggregate disk space quota policy for the specified member.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that quota is associated with.</param>
		/// <returns>A DiskSpaceQuota object that contains the policy for the specified member.</returns>
		static public DiskSpaceQuota Get( Store store, Member member )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetAggregatePolicy( DiskSpaceQuotaPolicyID, member );
			return new DiskSpaceQuota( store, member, pm, policy );
		}

		/// <summary>
		/// Gets the aggregate disk space quota policy for the specified member and collection.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that quota is associated with.</param>
		/// <param name="collection">Collection to add to the aggregate quota policy.</param>
		/// <returns>A DiskSpaceQuota object that contains the policy for the specified member.</returns>
		static public DiskSpaceQuota Get( Store store, Member member, Collection collection )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetAggregatePolicy( DiskSpaceQuotaPolicyID, member, collection );
			return new DiskSpaceQuota( store, member, pm, policy );
		}

		/// <summary>
		/// Sets the disk space quota limit associated with the specified domain.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="domainID">Domain that the limit is associated with.</param>
		/// <param name="limit">Amount of disk space that all users in the domain will be limited to.</param>
		static public void Set( Store store, string domainID, long limit )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, domainID );
			if ( policy == null )
			{
				DiskSpaceQuota.Create( store, domainID, limit );
			}
			else
			{
				policy.DeleteRule( DiskSpaceQuota.GetRule( policy ) );
				policy.AddRule( new Rule( limit, Rule.Operation.Less_Equal, Rule.Result.Allow ) );
				pm.CommitPolicy( policy, domainID );
			}
		}

		/// <summary>
		/// Sets the disk space quota limit associated with the specified member.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that the limit is associated with.</param>
		/// <param name="limit">Amount of disk space that all users in the domain will be limited to.</param>
		static public void Set( Store store, Member member, long limit )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, member );
			if ( policy == null )
			{
				DiskSpaceQuota.Create( store, member, limit );
			}
			else
			{
				policy.DeleteRule( DiskSpaceQuota.GetRule( policy ) );
				policy.AddRule( new Rule( limit, Rule.Operation.Less_Equal, Rule.Result.Allow ) );
				pm.CommitPolicy( policy, member );
			}
		}

		/// <summary>
		/// Sets the disk space quota limit associated with the specified collection.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="collection">Collection that the limit is associated with.</param>
		/// <param name="limit">Amount of disk space that all users in the domain will be limited to.</param>
		static public void Set( Store store, Collection collection, long limit )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( DiskSpaceQuotaPolicyID, collection );
			if ( policy == null )
			{
				DiskSpaceQuota.Create( store, collection, limit );
			}
			else
			{
				policy.DeleteRule( DiskSpaceQuota.GetRule( policy ) );
				policy.AddRule( new Rule( limit, Rule.Operation.Less_Equal, Rule.Result.Allow ) );
				pm.CommitPolicy( policy, collection );
			}
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
