using System;
using System.Collections;
using System.IO;

using Simias;
using Simias.Storage;

namespace Simias.Policy
{
	/// <summary>
	/// Implements the file size filter policy.
	/// </summary>
	public class FileSizeFilter
	{
		#region Class Members
		/// <summary>
		/// Well known name for the file size filter policy.
		/// </summary>
		static public string FileSizeFilterPolicyID = "e33e0a4a-d272-4bd0-9f35-b5a4cbe5f237";

		/// <summary>
		/// Well known name for the file size filter policy description.
		/// </summary>
		static public string FileSizeFilterShortDescription = "File size filter";

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
		#endregion

		#region Properties
		/// <summary>
		/// Gets the file size limit.
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
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of an object.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that this file size filter is associated with.</param>
		/// <param name="manager">Policy manager object to control the filter policy.</param>
		/// <param name="policy">The aggregate policy object.</param>
		private FileSizeFilter( Store store, Member member, PolicyManager manager, Policy policy )
		{
			this.store = store;
			this.manager = manager;
			this.policy = policy;
			this.member = member;
		}
		#endregion

		#region Factory Methods
		/// <summary>
		/// Creates a system wide file size filter policy.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="domainID">Domain that the filter will be associated with.</param>
		/// <param name="limit">Size of file in bytes that all users in the domain will be limited to.</param>
		static public void CreateFileSizeFilter( Store store, string domainID, long limit )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, domainID );
			if ( policy == null )
			{
				// The policy does not exist, create a new one and add the rules.
				policy = new Policy( FileSizeFilterPolicyID, FileSizeFilterShortDescription );
			}
			else
			{
				// The policy already exists, delete the old rule.
				policy.DeleteRule( FileSizeFilter.GetFileSizeRule( policy ) );
			}

			// Add the new rule and save the policy.
			policy.AddRule( new Rule( limit, Rule.Operation.Less_Equal, Rule.Result.Allow ) );
			pm.CommitPolicy( policy, domainID );
		}

		/// <summary>
		/// Creates a file size filter policy for the specified member.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that the filter will be associated with.</param>
		/// <param name="limit">Size of file in bytes that all users in the domain will be limited to.</param>
		static public void CreateFileSizeFilter( Store store, Member member, long limit )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, member );
			if ( policy == null )
			{
				// The policy does not exist, create a new one and add the rules.
				policy = new Policy( FileSizeFilterPolicyID, FileSizeFilterShortDescription );
			}
			else
			{
				// The policy already exists, delete the old rule.
				policy.DeleteRule( FileSizeFilter.GetFileSizeRule( policy ) );
			}

			// Add the new rule and save the policy.
			policy.AddRule( new Rule( limit, Rule.Operation.Less_Equal, Rule.Result.Allow ) );
			pm.CommitPolicy( policy, member );
		}

		/// <summary>
		/// Creates a file size filter policy for the specified collection.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="collection">Collection that the filter will be associated with.</param>
		/// <param name="limit">Size of file in bytes that all users in the domain will be limited to.</param>
		static public void CreateFileSizeFilter( Store store, Collection collection, long limit )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, collection );
			if ( policy == null )
			{
				// The policy does not exist, create a new one and add the rules.
				policy = new Policy( FileSizeFilterPolicyID, FileSizeFilterShortDescription );
			}
			else
			{
				// The policy already exists, delete the old rules.
				policy.DeleteRule( FileSizeFilter.GetFileSizeRule( policy ) );
			}

			// Add the new rules and save the policy.
			policy.AddRule( new Rule( limit, Rule.Operation.Less_Equal, Rule.Result.Allow ) );
			pm.CommitPolicy( policy, collection );
		}

		/// <summary>
		/// Creates a file size filter policy for the current user on the current machine.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="limit">Size of file in bytes that all users in the domain will be limited to.</param>
		static public void CreateFileSizeFilter( Store store, long limit )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID );
			if ( policy == null )
			{
				// The policy does not exist, create a new one and add the rules.
				policy = new Policy( FileSizeFilterPolicyID, FileSizeFilterShortDescription );
			}
			else
			{
				// The policy already exists, delete the old rules.
				policy.DeleteRule( FileSizeFilter.GetFileSizeRule( policy ) );
			}

			// Add the new rules and save the policy.
			policy.AddRule( new Rule( limit, Rule.Operation.Less_Equal, Rule.Result.Allow ) );
			pm.CommitLocalMachinePolicy( policy );
		}

		/// <summary>
		/// Gets the file size limit associated with the specified domain.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="domainID">Domain that the filter is associated with.</param>
		/// <returns>Size of files that all users in the domain are limited to.</returns>
		static public long GetFileSizeLimit( Store store, string domainID )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, domainID );
			return ( policy != null ) ? ( long )FileSizeFilter.GetFileSizeRule( policy ).Operand : long.MaxValue;
		}

		/// <summary>
		/// Gets the file size limit associated with the specified member.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that the filter is associated with.</param>
		/// <returns>Size of files that all users in the domain are limited to.</returns>
		static public long GetFileSizeLimit( Store store, Member member )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, member );
			return ( policy != null ) ? ( long )FileSizeFilter.GetFileSizeRule( policy ).Operand : long.MaxValue;
		}

		/// <summary>
		/// Gets the file size limit associated with the specified collection.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="collection">Collection that the limit is associated with.</param>
		/// <returns>Size of files that all users in the domain are limited to.</returns>
		static public long GetFileSizeLimit( Store store, Collection collection )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, collection );
			return ( policy != null ) ? ( long )FileSizeFilter.GetFileSizeRule( policy ).Operand : long.MaxValue;
		}

		/// <summary>
		/// Gets the file size limit associated with the current user on the current machine.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <returns>Size of files that all users in the domain are limited to.</returns>
		static public long GetFileSizeLimit( Store store )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID );
			return ( policy != null ) ? ( long )FileSizeFilter.GetFileSizeRule( policy ).Operand : long.MaxValue;
		}

		/// <summary>
		/// Gets the aggregate file size filter policy for the specified member.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that filter is associated with.</param>
		/// <returns>A FileSizeFilter object that contains the policy for the specified member.</returns>
		static public FileSizeFilter GetFileSizeFilter( Store store, Member member )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetAggregatePolicy( FileSizeFilterPolicyID, member );
			return new FileSizeFilter( store, member, pm, policy );
		}

		/// <summary>
		/// Gets the aggregate file size filter policy for the specified member and collection.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that filter is associated with.</param>
		/// <param name="collection">Collection to add to the aggregate size policy.</param>
		/// <returns>A FileSizeFilter object that contains the policy for the specified member.</returns>
		static public FileSizeFilter GetFileSizeFilter( Store store, Member member, Collection collection )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetAggregatePolicy( FileSizeFilterPolicyID, member, collection );
			return new FileSizeFilter( store, member, pm, policy );
		}

		/// <summary>
		/// Sets the file size limit associated with the specified domain.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="domainID">Domain that the filter is associated with.</param>
		/// <param name="limit">Size of files that all users in the domain will be limited to.</param>
		static public void SetFileSizeFilter( Store store, string domainID, long limit )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, domainID );
			if ( policy == null )
			{
				FileSizeFilter.CreateFileSizeFilter( store, domainID, limit );
			}
			else
			{
				// Remove the existing rule and add the new one.
				policy.DeleteRule( FileSizeFilter.GetFileSizeRule( policy ) );
				policy.AddRule( new Rule( limit, Rule.Operation.Less_Equal, Rule.Result.Allow ) );
				pm.CommitPolicy( policy, domainID );
			}
		}

		/// <summary>
		/// Sets the file size limit associated with the specified member.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that the filter is associated with.</param>
		/// <param name="limit">Size of files that all users in the domain will be limited to.</param>
		static public void SetFileSizeFilter( Store store, Member member, long limit )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, member );
			if ( policy == null )
			{
				FileSizeFilter.CreateFileSizeFilter( store, member, limit );
			}
			else
			{
				// Remove the existing rule and add the new one.
				policy.DeleteRule( FileSizeFilter.GetFileSizeRule( policy ) );
				policy.AddRule( new Rule( limit, Rule.Operation.Less_Equal, Rule.Result.Allow ) );
				pm.CommitPolicy( policy, member );
			}
		}

		/// <summary>
		/// Sets the file size limit associated with the specified collection.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="collection">Collection that the filter is associated with.</param>
		/// <param name="limit">Size of files that all users in the domain will be limited to.</param>
		static public void SetFileSizeFilter( Store store, Collection collection, long limit )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, collection );
			if ( policy == null )
			{
				FileSizeFilter.CreateFileSizeFilter( store, collection, limit );
			}
			else
			{
				// Remove the existing rule and add the new one.
				policy.DeleteRule( FileSizeFilter.GetFileSizeRule( policy ) );
				policy.AddRule( new Rule( limit, Rule.Operation.Less_Equal, Rule.Result.Allow ) );
				pm.CommitPolicy( policy, collection );
			}
		}

		/// <summary>
		/// Sets the file size limit associated with the current user on the current machine.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="limit">Size of files that all users in the domain will be limited to.</param>
		static public void SetFileSizeFilter( Store store, long limit )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID );
			if ( policy == null )
			{
				FileSizeFilter.CreateFileSizeFilter( store, limit );
			}
			else
			{
				// Remove the existing rule and add the new one.
				policy.DeleteRule( FileSizeFilter.GetFileSizeRule( policy ) );
				policy.AddRule( new Rule( limit, Rule.Operation.Less_Equal, Rule.Result.Allow ) );
				pm.CommitLocalMachinePolicy( policy );
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the file size rule for the specified policy.
		/// </summary>
		/// <param name="policy">Policy to retrieve the file size rule from.</param>
		/// <returns>The file size Rule from the policy.</returns>
		static private Rule GetFileSizeRule( Policy policy )
		{
			// There should only be one rule in the file size policy.
			IEnumerator e = policy.Rules.GetEnumerator();
			if ( !e.MoveNext() )
			{
				throw new SimiasException( "No policy rule on file size." );
			}

			return e.Current as Rule;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns whether the specified file size is allowed to pass through the filter.
		/// </summary>
		/// <param name="fileSize">Size in bytes of a file.</param>
		/// <returns>True if the file size is allowed to pass through the filter. Otherwise false is returned.</returns>
		public bool Allowed( long fileSize )
		{
			return ( ( policy == null ) || ( policy.Apply( fileSize ) == Rule.Result.Allow ) ) ? true : false;
		}
		#endregion
	}
}
