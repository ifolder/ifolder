using System;
using System.Xml;

using Simias.Policy;

namespace Simias.Storage
{
	/// <summary>
	/// This object defines an administrative policy that is used to make run-time decisions in regards to certain
	/// conditions or behaviors that are applied to a user object.
	/// </summary>
	public class SystemPolicy : Node, IPolicy
	{
		#region Properties
		/// <summary>
		/// Gets or sets the detailed description for this system policy.
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

		/// <summary>
		/// Returns the list of Rule objects for this policy.
		/// </summary>
		public ICSList Rules
		{
			get { return new ICSList(); }
		}

		/// <summary>
		/// Gets the strong name for this system policy.
		/// </summary>
		public string StrongName
		{
			get { return properties.GetSingleProperty( PropertyTags.PolicyID ).Value as string; }
		}

		/// <summary>
		/// Returns the list of TimeCondition objects for this policy.
		/// </summary>
		public ICSList TimeConditions
		{
			get { return new ICSList(); }
		}

		/// <summary>
		/// Gets or sets the title for the system policy.
		/// </summary>
		public string Title
		{
			get { return Name; }
			set { Name = value; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for creating a new SystemPolicy object.
		/// </summary>
		/// <param name="strongName">Strong name of the policy. This should be a well-known GUID.</param>
		/// <param name="shortDescription">A short friendly description of the policy.</param>
		public SystemPolicy( string strongName, string shortDescription ) :
			base( shortDescription, Guid.NewGuid().ToString(), NodeTypes.SystemPolicyType )
		{
			properties.AddNodeProperty( PropertyTags.PolicyID, strongName );
		}

		/// <summary>
		/// Constructor that creates a SystemPolicy object from a Node object.
		/// </summary>
		/// <param name="node">Node object to create the SystemPolicy object from.</param>
		public SystemPolicy( Node node ) :
			base( node )
		{
			if ( type != NodeTypes.SystemPolicyType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.SystemPolicyType, type ) );
			}
		}

		/// <summary>
		/// Constructor that creates a SystemPolicy object from a ShallowNode object.
		/// </summary>
		/// <param name="collection">Collection that the specified Node object belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create the SystemPolicy object from.</param>
		public SystemPolicy( Collection collection, ShallowNode shallowNode ) :
			base( collection, shallowNode )
		{
			if ( type != NodeTypes.SystemPolicyType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.SystemPolicyType, type ) );
			}
		}

		/// <summary>
		/// Constructor that creates a SystemPolicy object from an Xml document object.
		/// </summary>
		/// <param name="document">Xml document object to create the SystemPolicy object from.</param>
		internal SystemPolicy( XmlDocument document ) :
			base( document )
		{
			if ( type != NodeTypes.SystemPolicyType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.SystemPolicyType, type ) );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds a time condition to the rule to indicate when the rule is effective.
		/// </summary>
		public void AddTimeCondition( TimeCondition time )
		{
		}

		/// <summary>
		/// Adds a rule to the policy.
		/// </summary>
		/// <param name="rule">Object that is used to match against the input in the policy.</param>
		public void AddRule( Rule rule )
		{
		}

		/// <summary>
		/// Applies the policy rules on the specified object to determine if the result is allowed or denied.
		/// </summary>
		/// <param name="input">Object that is used to match against the policy rules. The type of object must be
		/// one of the Simias.Syntax types.</param>
		/// <returns>True if the policy allows the operation, otherwise false is returned.</returns>
		public Policy.Policy.RuleResult Apply( object input )
		{
			return Policy.Policy.RuleResult.Allow;
		}

		/// <summary>
		/// Removes the time condition for the policy.
		/// </summary>
		public void DeleteTimeCondition( TimeCondition time )
		{
		}

		/// <summary>
		/// Removes the specified rule from the policy.
		/// </summary>
		/// <param name="rule">Rule that is used to match against the input in the policy.</param>
		public void DeleteRule( Rule rule )
		{
		}
		#endregion
	}
}
