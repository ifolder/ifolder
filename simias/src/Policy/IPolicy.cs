using System;
using Simias.Storage;

namespace Simias.Policy
{
	/// <summary>
	/// Collection Store policy factory interface.
	/// </summary>
	public interface IPolicyFactory
	{
		/// <summary>
		/// Saves the specified SystemPolicy object that will be applied system-wide for the specified domain.
		/// The caller must possess Admin rights in order to commit a system policy.
		/// </summary>
		/// <param name="policy">SystemPolicy object to be saved.</param>
		/// <param name="domainID">Identifier of domain to associate system policy with.</param>
		void CommitPolicy( SystemPolicy policy, string domainID );

		/// <summary>
		/// Saves the specified Policy object and associates it with the specified user.
		/// The caller must possess Admin rights in order to commit a system policy.
		/// The caller must have read-write rights to commit a user policy.
		/// </summary>
		/// <param name="policy">Policy object to be saved.</param>
		/// <param name="member">Member object to associate this policy with.</param>
		void CommitPolicy( IPolicy policy, Member member );

		/// <summary>
		/// Creates a system policy that can be applied to the entire Simias system or a specified user.
		/// </summary>
		/// <param name="strongName">Strong name of the policy. This typically should be a well-known GUID.</param>
		/// <param name="shortDescription">A short friendly description of the policy.</param>
		/// <returns>A reference to a system policy object.</returns>
		SystemPolicy CreateSystemPolicy( string strongName, string shortDescription );

		/// <summary>
		/// Creates a user policy that can be applied to a specified user.
		/// </summary>
		/// <param name="strongName">Strong name of the policy. This typically should be a well-known GUID.</param>
		/// <param name="shortDescription">A short friendly description of the policy.</param>
		/// <returns>A reference to a user policy object.</returns>
		UserPolicy CreateUserPolicy( string strongName, string shortDescription );

		/// <summary>
		/// Deletes the specified policy.
		/// </summary>
		/// <param name="policy">Policy object to delete.</param>
		void DeletePolicy( IPolicy policy );

		/// <summary>
		/// Gets the specified system policy for the specified domain.
		/// </summary>
		/// <param name="strongName">Strong name of the system policy.</param>
		/// <param name="domainID">Identifier for the domain to use to lookup the specified policy.</param>
		/// <returns>A reference to the associated SystemPolicy object if successful. A null is returned if the specifed
		/// system policy does not exist.</returns>
		SystemPolicy GetPolicy( string strongName, string domainID );

		/// <summary>
		/// Gets the specified policy that is associated with the specified user.
		/// </summary>
		/// <param name="strongName">Strong name of the policy.</param>
		/// <param name="member">Member object to use to lookup the specified policy.</param>
		/// <returns>A reference to the associated IPolicy object if successful. A null is returned if the specifed
		/// policy does not exist.</returns>
		IPolicy GetPolicy( string strongName, Member member );

		/// <summary>
		/// Gets a list of all Policy objects for the specified user.
		/// </summary>
		/// <param name="member">Member object to get policies for.</param>
		/// <returns>A reference to an ICSList object that contains all of the IPolicy objects that apply to the
		/// specified user.</returns>
		ICSList GetPolicyList( Member member );

		/// <summary>
		/// Gets a list of all the SystemPolicy objects for the specified domain.
		/// </summary>
		/// <param name="domainID">Identifier for a domain.</param>
		/// <returns>A reference to an ICSList object that contains all of the SystemPolicy objects for the
		/// specified domain.</returns>
		ICSList GetPolicyList( string domainID );
	}


	/// <summary>
	/// Policy object definition.
	/// </summary>
	public interface IPolicy
	{
		/// <summary>
		/// Adds a rule to the policy.
		/// </summary>
		/// <param name="rule">Object that is used to match against in the policy.</param>
		/// <param name="compareResult">Result to return when rule is applied successfully.</param>
		void AddRule( object rule, Policy.RuleResult compareResult );

		/// <summary>
		/// Applies the policy rules on the specified object to determine if the result is allowed or denied.
		/// </summary>
		/// <param name="input">Object that is used to match against the policy rules. The type of object must be
		/// one of the Simias.Syntax types.</param>
		/// <param name="operation">Operation to perform on rule.</param>
		/// <returns>True if the policy allows the operation, otherwise false is returned.</returns>
		bool Apply( object input, Policy.RuleOperation operation );
	}


	/// <summary>
	/// Contains global definitions for Policy functionality.
	/// </summary>
	public class Policy
	{
		#region Class Members
		/// <summary>
		/// Result of applying input value to policy rule.
		/// </summary>
		public enum RuleResult
		{
			/// <summary>
			/// If rule operation comparision is true, return allowed.
			/// </summary>
			Allow,

			/// <summary>
			/// If rule operation comparison is true, return denied.
			/// </summary>
			Deny
		};

		/// <summary>
		/// Operation type to perform on policy rules.
		/// </summary>
		public enum RuleOperation
		{
			/// <summary>
			/// Used to compare if two values are equal.
			/// </summary>
			Equal,

			/// <summary>
			/// Used to compare if two values are not equal.
			/// </summary>
			Not_Equal,

			/// <summary>
			/// Used to compare if a value is greater than another value.
			/// </summary>
			Greater,

			/// <summary>
			/// Used to compare if a value is less than another value.
			/// </summary>
			Less,

			/// <summary>
			/// Used to compare if a value is greater than or equal to another value.
			/// </summary>
			Greater_Equal,

			/// <summary>
			/// Used to compare if a value is less than or equal to another value.
			/// </summary>
			Less_Equal,
		};
		#endregion
	}
}
