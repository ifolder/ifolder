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
		/// Saves the specified SystemPolicy object that will be applied system-wide for the specified domain.
		/// The caller must possess Admin rights in order to commit a system policy.
		/// </summary>
		/// <param name="policy">SystemPolicy object to be saved.</param>
		/// <param name="domainID">Identifier of domain to associate system policy with.</param>
		public void CommitPolicy( SystemPolicy policy, string domainID )
		{
		}

		/// <summary>
		/// Saves the specified Policy object and associates it with the specified user.
		/// The caller must possess Admin rights in order to commit a system policy.
		/// The caller must have read-write rights to commit a user policy.
		/// </summary>
		/// <param name="policy">Policy object to be saved.</param>
		/// <param name="member">Member object to associate this policy with.</param>
		public void CommitPolicy( IPolicy policy, Member member )
		{
		}

		/// <summary>
		/// Creates a system policy that can be applied to the entire Simias system or a specified user.
		/// </summary>
		/// <param name="strongName">Strong name of the policy. This typically should be a well-known GUID.</param>
		/// <param name="shortDescription">A short friendly description of the policy.</param>
		/// <returns>A reference to a system policy object.</returns>
		public SystemPolicy CreateSystemPolicy( string strongName, string shortDescription )
		{
			return null;
		}

		/// <summary>
		/// Creates a user policy that can be applied to a specified user.
		/// </summary>
		/// <param name="strongName">Strong name of the policy. This typically should be a well-known GUID.</param>
		/// <param name="shortDescription">A short friendly description of the policy.</param>
		/// <returns>A reference to a user policy object.</returns>
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
		/// Gets the specified system policy for the specified domain.
		/// </summary>
		/// <param name="strongName">Strong name of the system policy.</param>
		/// <param name="domainID">Identifier for the domain to use to lookup the specified policy.</param>
		/// <returns>A reference to the associated SystemPolicy object if successful. A null is returned if the specifed
		/// system policy does not exist.</returns>
		public SystemPolicy GetPolicy( string strongName, string domainID )
		{
			return null;
		}

		/// <summary>
		/// Gets the specified policy that is associated with the specified user.
		/// </summary>
		/// <param name="strongName">Strong name of the policy.</param>
		/// <param name="member">Member object to use to lookup the specified policy.</param>
		/// <returns>A reference to the associated IPolicy object if successful. A null is returned if the specifed
		/// policy does not exist.</returns>
		public IPolicy GetPolicy( string strongName, Member member )
		{
			return null;
		}

		/// <summary>
		/// Gets a list of all Policy objects for the specified user.
		/// </summary>
		/// <param name="member">Member object to get policies for.</param>
		/// <returns>A reference to an ICSList object that contains all of the IPolicy objects that apply to the
		/// specified user.</returns>
		public ICSList GetPolicyList( Member member )
		{
			return new ICSList();
		}

		/// <summary>
		/// Gets a list of all the SystemPolicy objects for the specified domain.
		/// </summary>
		/// <param name="domainID">Identifier for a domain.</param>
		/// <returns>A reference to an ICSList object that contains all of the SystemPolicy objects for the
		/// specified domain.</returns>
		public ICSList GetPolicyList( string domainID )
		{
			return new ICSList();
		}
		#endregion
	}
}
