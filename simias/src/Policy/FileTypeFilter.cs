using System;
using System.Collections;
using System.IO;

using Simias;
using Simias.Storage;

namespace Simias.Policy
{
	/// <summary>
	/// List used to specify a filter list to the policy.
	/// </summary>
	public struct FileTypeEntry
	{
		#region Class Members
		/// <summary>
		/// File extension to add as filter.
		/// </summary>
		private string fileNameExtension;

		/// <summary>
		/// If true then file extension will be allowed to pass through the filter
		/// If false then file will be disallowed to pass through the filter.
		/// </summary>
		private bool allowed;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the filter extension name.
		/// </summary>
		public string Name
		{
			get { return fileNameExtension; }
		}

		/// <summary>
		/// Gets whether filter extension is allowed or disallowed through the filter.
		/// </summary>
		public bool Allowed
		{
			get { return allowed; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="fileNameExtension">Filename extension to use as a filter.</param>
		/// <param name="allowed">If true then all files that have extensions that match the 
		/// fileNameExtension parameter will be allowed to pass through the filter.</param>
		public FileTypeEntry( string fileNameExtension, bool allowed )
		{
			this.fileNameExtension = fileNameExtension;
			this.allowed = allowed;
		}
		#endregion
	}

	/// <summary>
	/// Implements the file type filter policy.
	/// </summary>
	public class FileTypeFilter
	{
		#region Class Members
		/// <summary>
		/// Well known name for the file type filter policy.
		/// </summary>
		static public string FileTypeFilterPolicyID = "e69ff680-3f75-412e-a929-1b0247ed4041";

		/// <summary>
		/// Well known name for the file type filter policy description.
		/// </summary>
		static public string FileTypeFilterShortDescription = "File type filter";

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
		/// Gets the file type filter list.
		/// </summary>
		public FileTypeEntry[] FilterList
		{
			get { return FileTypeFilter.GetPatterns( policy ); }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of an object.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that this file type filter is associated with.</param>
		/// <param name="manager">Policy manager object to control the filter policy.</param>
		/// <param name="policy">The aggregate policy object.</param>
		private FileTypeFilter( Store store, Member member, PolicyManager manager, Policy policy )
		{
			this.store = store;
			this.manager = manager;
			this.policy = policy;
			this.member = member;
		}
		#endregion

		#region Factory Methods
		/// <summary>
		/// Creates a system wide file filter policy.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="domainID">Domain that the filter will be associated with.</param>
		/// <param name="patterns">File type patterns that will be used to filter files.</param>
		static public void Create( Store store, string domainID, FileTypeEntry[] patterns )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, domainID );
			if ( policy == null )
			{
				// The policy does not exist, create a new one and add the rules.
				policy = new Policy( FileTypeFilterPolicyID, FileTypeFilterShortDescription );
			}
			else
			{
				// The policy already exists, delete the old rules.
				foreach ( Rule r in policy.Rules )
				{
					policy.DeleteRule( r );
				}
			}

			// Add the new rules and save the policy.
			foreach( FileTypeEntry fte in patterns )
			{
				policy.AddRule( new Rule( fte.Name, Rule.Operation.Equal, fte.Allowed ? Rule.Result.Allow : Rule.Result.Deny ) );
			}

			pm.CommitPolicy( policy, domainID );
		}

		/// <summary>
		/// Creates a file type filter policy for the specified member.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that the filter will be associated with.</param>
		/// <param name="patterns">File type patterns that will be used to filter files.</param>
		static public void Create( Store store, Member member, FileTypeEntry[] patterns )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, member );
			if ( policy == null )
			{
				// The policy does not exist, create a new one and add the rules.
				policy = new Policy( FileTypeFilterPolicyID, FileTypeFilterShortDescription );
			}
			else
			{
				// The policy already exists, delete the old rules.
				foreach ( Rule r in policy.Rules )
				{
					policy.DeleteRule( r );
				}
			}

			// Add the new rules and save the policy.
			foreach( FileTypeEntry fte in patterns )
			{
				policy.AddRule( new Rule( fte.Name, Rule.Operation.Equal, fte.Allowed ? Rule.Result.Allow : Rule.Result.Deny ) );
			}

			pm.CommitPolicy( policy, member );
		}

		/// <summary>
		/// Creates a file type filter policy for the specified collection.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="collection">Collection that the filter will be associated with.</param>
		/// <param name="patterns">File type patterns that will be used to filter files.</param>
		static public void Create( Store store, Collection collection, FileTypeEntry[] patterns )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, collection );
			if ( policy == null )
			{
				// The policy does not exist, create a new one and add the rules.
				policy = new Policy( FileTypeFilterPolicyID, FileTypeFilterShortDescription );
			}
			else
			{
				// The policy already exists, delete the old rules.
				foreach ( Rule r in policy.Rules )
				{
					policy.DeleteRule( r );
				}
			}

			// Add the new rules and save the policy.
			foreach( FileTypeEntry fte in patterns )
			{
				policy.AddRule( new Rule( fte.Name, Rule.Operation.Equal, fte.Allowed ? Rule.Result.Allow : Rule.Result.Deny ) );
			}

			pm.CommitPolicy( policy, collection );
		}

		/// <summary>
		/// Creates a file type filter policy for the current user on the current machine.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="patterns">File type patterns that will be used to filter files.</param>
		static public void Create( Store store, FileTypeEntry[] patterns )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID );
			if ( policy == null )
			{
				// The policy does not exist, create a new one and add the rules.
				policy = new Policy( FileTypeFilterPolicyID, FileTypeFilterShortDescription );
			}
			else
			{
				// The policy already exists, delete the old rules.
				foreach ( Rule r in policy.Rules )
				{
					policy.DeleteRule( r );
				}
			}

			// Add the new rules and save the policy.
			foreach( FileTypeEntry fte in patterns )
			{
				policy.AddRule( new Rule( fte.Name, Rule.Operation.Equal, fte.Allowed ? Rule.Result.Allow : Rule.Result.Deny ) );
			}

			pm.CommitLocalMachinePolicy( policy );
		}

		/// <summary>
		/// Deletes a system wide file filter policy.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="domainID">Domain that the filter will be associated with.</param>
		static public void Delete( Store store, string domainID )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, domainID );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a file type filter policy for the specified member.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that the filter will be associated with.</param>
		static public void Delete( Store store, Member member )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, member );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a file type filter policy for the specified collection.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="collection">Collection that the filter will be associated with.</param>
		static public void Delete( Store store, Collection collection )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, collection );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a file type filter policy for the current user on the current machine.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		static public void Delete( Store store )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager( store );
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Gets the file type filter patterns associated with the specified domain.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="domainID">Domain that the filter is associated with.</param>
		/// <returns>Array of file type filter patterns for this policy if successful. If there are no
		/// filter patterns then null is returned.</returns>
		static public FileTypeEntry[] GetPatterns( Store store, string domainID )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, domainID );
			return ( policy != null ) ? FileTypeFilter.GetPatterns( policy ) : null;
		}

		/// <summary>
		/// Gets the file type filter patterns associated with the specified member.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that the filter is associated with.</param>
		/// <returns>Array of file type filter patterns for this policy if successful. If there are no
		/// filter patterns then null is returned.</returns>
		static public FileTypeEntry[] GetPatterns( Store store, Member member )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, member );
			return ( policy != null ) ? FileTypeFilter.GetPatterns( policy ) : null;
		}

		/// <summary>
		/// Gets the file type filter patterns associated with the specified collection.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="collection">Collection that the limit is associated with.</param>
		/// <returns>Array of file type filter patterns for this policy if successful. If there are no
		/// filter patterns then null is returned.</returns>
		static public FileTypeEntry[] GetPatterns( Store store, Collection collection )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, collection );
			return ( policy != null ) ? FileTypeFilter.GetPatterns( policy ) : null;
		}

		/// <summary>
		/// Gets the file type filter patterns associated with the current user on the current machine.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <returns>Array of file type filter patterns for this policy if successful. If there are no
		/// filter patterns then null is returned.</returns>
		static public FileTypeEntry[] GetPatterns( Store store )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID );
			return ( policy != null ) ? FileTypeFilter.GetPatterns( policy ) : null;
		}

		/// <summary>
		/// Gets the aggregate file type filter policy for the specified member.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that filter is associated with.</param>
		/// <returns>A FileTypeFilter object that contains the policy for the specified member.</returns>
		static public FileTypeFilter Get( Store store, Member member )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetAggregatePolicy( FileTypeFilterPolicyID, member );
			return new FileTypeFilter( store, member, pm, policy );
		}

		/// <summary>
		/// Gets the aggregate file type filter policy for the specified member and collection.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that filter is associated with.</param>
		/// <param name="collection">Collection to add to the aggregate quota policy.</param>
		/// <returns>A FileTypeFilter object that contains the policy for the specified member.</returns>
		static public FileTypeFilter Get( Store store, Member member, Collection collection )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetAggregatePolicy( FileTypeFilterPolicyID, member, collection );
			return new FileTypeFilter( store, member, pm, policy );
		}

		/// <summary>
		/// Sets the file type filter associated with the specified domain.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="domainID">Domain that the filter is associated with.</param>
		/// <param name="patterns">File type patterns that will be used to filter files.</param>
		static public void Set( Store store, string domainID, FileTypeEntry[] patterns )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, domainID );
			if ( policy == null )
			{
				FileTypeFilter.Create( store, domainID, patterns );
			}
			else
			{
				// Remove the existing rules.
				foreach ( Rule r in policy.Rules )
				{
					policy.DeleteRule( r );
				}

				// Add the new rules.
				foreach ( FileTypeEntry fte in patterns )
				{
					policy.AddRule( new Rule( fte.Name, Rule.Operation.Equal, fte.Allowed ? Rule.Result.Allow : Rule.Result.Deny ) );
				}

				pm.CommitPolicy( policy, domainID );
			}
		}

		/// <summary>
		/// Sets the file type filter associated with the specified member.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="member">Member that the filter is associated with.</param>
		/// <param name="patterns">File type patterns that will be used to filter files.</param>
		static public void Set( Store store, Member member, FileTypeEntry[] patterns )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, member );
			if ( policy == null )
			{
				FileTypeFilter.Create( store, member, patterns );
			}
			else
			{
				// Remove the existing rules.
				foreach ( Rule r in policy.Rules )
				{
					policy.DeleteRule( r );
				}

				// Add the new rules.
				foreach ( FileTypeEntry fte in patterns )
				{
					policy.AddRule( new Rule( fte.Name, Rule.Operation.Equal, fte.Allowed ? Rule.Result.Allow : Rule.Result.Deny ) );
				}

				pm.CommitPolicy( policy, member );
			}
		}

		/// <summary>
		/// Sets the file type filter associated with the specified collection.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="collection">Collection that the filter is associated with.</param>
		/// <param name="patterns">File type patterns that will be used to filter files.</param>
		static public void Set( Store store, Collection collection, FileTypeEntry[] patterns )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, collection );
			if ( policy == null )
			{
				FileTypeFilter.Create( store, collection, patterns );
			}
			else
			{
				// Remove the existing rules.
				foreach ( Rule r in policy.Rules )
				{
					policy.DeleteRule( r );
				}

				// Add the new rules.
				foreach ( FileTypeEntry fte in patterns )
				{
					policy.AddRule( new Rule( fte.Name, Rule.Operation.Equal, fte.Allowed ? Rule.Result.Allow : Rule.Result.Deny ) );
				}

				pm.CommitPolicy( policy, collection );
			}
		}

		/// <summary>
		/// Sets the file type filter associated with the current user on the current machine.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <param name="patterns">File type patterns that will be used to filter files.</param>
		static public void Set( Store store, FileTypeEntry[] patterns )
		{
			PolicyManager pm = new PolicyManager( store );
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID );
			if ( policy == null )
			{
				FileTypeFilter.Create( store, patterns );
			}
			else
			{
				// Remove the existing rules.
				foreach ( Rule r in policy.Rules )
				{
					policy.DeleteRule( r );
				}

				// Add the new rules.
				foreach ( FileTypeEntry fte in patterns )
				{
					policy.AddRule( new Rule( fte.Name, Rule.Operation.Equal, fte.Allowed ? Rule.Result.Allow : Rule.Result.Deny ) );
				}

				pm.CommitLocalMachinePolicy( policy );
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the file type filter patterns for the specified policy.
		/// </summary>
		/// <param name="policy">Policy to retrieve the filter patterns from.</param>
		/// <returns>An array of file type filter patterns for the given policy.</returns>
		static private FileTypeEntry[] GetPatterns( Policy policy )
		{
			// List to hold the temporary results.
			ArrayList tempList = new ArrayList();

			// Return all of the rules.
			if ( policy != null )
			{
				foreach ( Rule rule in policy.Rules )
				{
					FileTypeEntry fte = new FileTypeEntry( rule.Operand as string, ( rule.RuleResult == Rule.Result.Allow ) ? true : false );
					tempList.Add( fte );
				}
			}

			return tempList.ToArray( typeof( FileTypeEntry ) ) as FileTypeEntry[];
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns whether the specified file is allowed to pass through the filter.
		/// </summary>
		/// <param name="fileName">Name of the file including its extension.</param>
		/// <returns>True if the file is allowed to pass through the filter. Otherwise false is returned.</returns>
		public bool Allowed( string fileName )
		{
			return ( ( policy == null ) || ( policy.Apply( Path.GetExtension( fileName ) ) == Rule.Result.Allow ) ) ? true : false;
		}
		#endregion
	}
}
