using System;
using Simias.Storage;

namespace Simias.Policy
{
	/// <summary>
	/// This object contains the IPolicyFactory interface implementation for the WorkGroup model.
	/// </summary>
	public class WorkGroupPolicyFactory : IPolicyFactory
	{
		#region Public Methods
		/// <summary>
		/// Saves the specified Policy object. The caller must possess Admin rights in order to commit a system policy.
		/// The caller must have read-write rights to commit a user policy.
		/// </summary>
		/// <param name="policy">Policy object to be saved.</param>
		public void CommitPolicy( IPolicy policy )
		{
		}

		/// <summary>
		/// Creates a system policy that can be applied to the entire Simias system or a specified user.
		/// </summary>
		/// <param name="strongName">Strong name of the policy. This typically should be a well-known GUID.</param>
		/// <param name="shortDescription">A short friendly description of the policy.</param>
		/// <returns>A reference to a system policy object if successful, otherwise a null is returned.</returns>
		public SystemPolicy CreateSystemPolicy( string strongName, string shortDescription )
		{
			return null;
		}

		/// <summary>
		/// Creates a user policy that can be applied to a specified user.
		/// </summary>
		/// <param name="strongName">Strong name of the policy. This typically should be a well-known GUID.</param>
		/// <param name="shortDescription">A short friendly description of the policy.</param>
		/// <returns>A reference to a user policy object if successful, otherwise a null is returned.</returns>
		public UserPolicy CreateUserPolicy( string strongName, string shortDescription )
		{
			return null;
		}

		/// <summary>
		/// Deletes the specified policy.
		/// </summary>
		/// <param name="policy">Policy object to delete.</param>
		public void DeletePolicy( IPolicy policy )
		{
		}

		/// <summary>
		/// Deletes the specified policy.
		/// </summary>
		/// <param name="strongName">Strong name of the policy to delete.</param>
		public void DeletePolicy( string strongName )
		{
		}

		/// <summary>
		/// Gets the specified policy.
		/// </summary>
		/// <param name="strongName">Strong name of the policy.</param>
		/// <returns>A reference to the associated Policy object if successful. A null is returned if the specifed
		/// policy does not exist.</returns>
		public IPolicy GetPolicy( string strongName )
		{
			return null;
		}

		/// <summary>
		/// Gets a list of all Policy objects for the default domain.
		/// </summary>
		/// <returns>A reference to an ICSList object that contains all of the IPolicy objects for the
		/// default domain.</returns>
		public ICSList GetPolicyList()
		{
			return new ICSList();
		}

		/// <summary>
		/// Gets a list of all Policy objects for the specified domain.
		/// </summary>
		/// <param name="domainID">Identifier for a domain.</param>
		/// <returns>A reference to an ICSList object that contains all of the IPolicy objects for the
		/// specified domain.</returns>
		public ICSList GetPolicyList( string domainID )
		{
			return new ICSList();
		}
		#endregion
	}
}
