using System;
using System.Xml;

using Simias.Policy;

namespace Simias.Storage
{
	/// <summary>
	/// This object defines a user policy that is used to make run-time decisions in regards to certain
	/// conditions or behaviors that are applied to a user object.
	/// </summary>
	public class UserPolicy : Node, IPolicy
	{
		#region Properties
		/// <summary>
		/// Gets the strong name for this user policy.
		/// </summary>
		public string StrongName
		{
			get { return properties.GetSingleProperty( PropertyTags.PolicyID ).Value as string; }
		}

		/// <summary>
		/// Gets or sets the title for the user policy.
		/// </summary>
		public string Title
		{
			get { return Name; }
			set { Name = value; }
		}

		/// <summary>
		/// Gets or sets the detailed description for this user policy.
		/// </summary>
		public string Description
		{
			get
			{
				Property p = properties.GetSingleProperty( PropertyTags.Description );
				return ( p != null ) ? p.Value as string : null;
			}

			set { properties.ModifyNodeProperty( PropertyTags.Description, value ); }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for creating a new UserPolicy object.
		/// </summary>
		/// <param name="strongName">Strong name of the policy. This should be a well-known GUID.</param>
		/// <param name="shortDescription">A short friendly description of the policy.</param>
		public UserPolicy( string strongName, string shortDescription ) :
			base( shortDescription, Guid.NewGuid().ToString(), NodeTypes.UserPolicyType )
		{
			properties.AddNodeProperty( PropertyTags.PolicyID, strongName );
		}

		/// <summary>
		/// Constructor that creates a UserPolicy object from a Node object.
		/// </summary>
		/// <param name="node">Node object to create the UserPolicy object from.</param>
		public UserPolicy( Node node ) :
			base( node )
		{
			if ( type != NodeTypes.UserPolicyType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.UserPolicyType, type ) );
			}
		}

		/// <summary>
		/// Constructor that creates a UserPolicy object from a ShallowNode object.
		/// </summary>
		/// <param name="collection">Collection that the specified Node object belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create the UserPolicy object from.</param>
		public UserPolicy( Collection collection, ShallowNode shallowNode ) :
			base( collection, shallowNode )
		{
			if ( type != NodeTypes.UserPolicyType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.UserPolicyType, type ) );
			}
		}

		/// <summary>
		/// Constructor that creates a UserPolicy object from an Xml document object.
		/// </summary>
		/// <param name="document">Xml document object to create the UserPolicy object from.</param>
		internal UserPolicy( XmlDocument document ) :
			base( document )
		{
			if ( type != NodeTypes.UserPolicyType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.UserPolicyType, type ) );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds a rule to the policy.
		/// </summary>
		/// <param name="rule">Object that is used to match against in the policy.</param>
		/// <param name="compareResult">Result to return when rule is applied successfully.</param>
		public virtual void AddRule( object rule, Policy.Policy.RuleResult compareResult )
		{
		}

		/// <summary>
		/// Applies the policy rules on the specified object to determine if the result is allowed or denied.
		/// </summary>
		/// <param name="input">Object that is used to match against the policy rules. The type of object must be
		/// one of the Simias.Syntax types.</param>
		/// <param name="operation">Operation to perform on rule.</param>
		/// <returns>True if the policy allows the operation, otherwise false is returned.</returns>
		public virtual bool Apply( object input, Policy.Policy.RuleOperation operation )
		{
			return true;
		}
		#endregion
	}
}
