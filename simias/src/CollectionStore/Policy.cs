using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

using Simias.Storage;
using Simias.POBox;

namespace Simias.Policy
{
	/// <summary>
	/// Policy object definition.
	/// </summary>
	[ Serializable ]
	public class Policy : Node
	{
		#region Class Members
		/// <summary>
		/// Property names to store rules and times on the policy Node.
		/// </summary>
		static private string AllowRule = "AllowRule";
		static private string DenyRule = "DenyRule";
		static private string TimeCondition = "TimeCondition";

		/// <summary>
		/// Order in which an aggregate Policy is evaluated.
		/// </summary>
		private enum AggregatePolicyOrder
		{
			/// <summary>
			/// This Policy is either contained on the Roster or the POBox for the Member.
			/// </summary>
			User,

			/// <summary>
			/// This Policy is contained on the LocalDatabase.
			/// </summary>
			Local,

			/// <summary>
			/// This Policy is contained on the Collection.
			/// </summary>
			Collection
		};

		/// <summary>
		/// Used to hold all Policies associated with a Member.
		/// </summary>
		[ NonSerialized() ]
		private Policy[] aggregatePolicy = null;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or set an aggregate collection policy.
		/// </summary>
		internal Policy CollectionPolicy
		{
			get { return (aggregatePolicy != null ) ? aggregatePolicy[ ( int )AggregatePolicyOrder.Collection ] : null; }
			set 
			{ 
				if ( aggregatePolicy == null )
				{
					aggregatePolicy = new Policy[ Enum.GetValues( typeof( AggregatePolicyOrder ) ).Length ];
				}

				aggregatePolicy[ ( int )AggregatePolicyOrder.Collection ] = value; 
			}
		}

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
		/// Gets or set an aggregate local policy.
		/// </summary>
		internal Policy LocalPolicy
		{
			get { return ( aggregatePolicy != null ) ? aggregatePolicy[ ( int )AggregatePolicyOrder.Local ] : null; }
			set 
			{ 
				if ( aggregatePolicy == null )
				{
					aggregatePolicy = new Policy[ Enum.GetValues( typeof( AggregatePolicyOrder ) ).Length ];
				}

				aggregatePolicy[ ( int )AggregatePolicyOrder.Local ] = value; 
			}
		}

		/// <summary>
		/// Returns the PolicyTime object for this policy. If there is no time condition,
		/// null is returned. If this is an aggregated Policy, all rules from the aggregated 
		/// Policies are returned.
		/// </summary>
		public ICSList PolicyTimes
		{
			get 
			{ 
				ICSList timeList = new ICSList();
				Policy[] policyArray = ( aggregatePolicy != null ) ? aggregatePolicy : new Policy[] { this };

				foreach( Policy policy in policyArray )
				{
					if ( policy != null )
					{
						Property p = policy.properties.GetSingleProperty( TimeCondition );
						if ( p != null )
						{
							timeList.Add( new PolicyTime( p.Value as string ) );
						}
					}
				}

				return timeList;
			}
		}

		/// <summary>
		/// Returns the list of Rule objects for this policy. If this is an aggregated Policy,
		/// all rules from the aggregated Policies are returned.
		/// </summary>
		public ICSList Rules
		{
			get 
			{ 
				ICSList ruleList = new ICSList();
				Policy[] policyArray = ( aggregatePolicy != null ) ? aggregatePolicy : new Policy[] { this };

				foreach( Policy policy in policyArray )
				{
					if ( policy != null )
					{
						// Get the deny rule list.
						MultiValuedList mvl = policy.properties.GetProperties( DenyRule );
						foreach( Property p in mvl )
						{
							ruleList.Add( new Rule( p.Value as string ) );
						}

						// Get the allow rule list.
						mvl = policy.properties.GetProperties( AllowRule );
						foreach( Property p in mvl )
						{
							ruleList.Add( new Rule( p.Value as string ) );
						}
					}
				}

				return ruleList;
			}
		}

		/// <summary>
		/// Gets the strong name for this system policy.
		/// </summary>
		public string StrongName
		{
			get { return properties.GetSingleProperty( PropertyTags.PolicyID ).Value as string; }
		}

		/// <summary>
		/// Gets or sets the title for the system policy.
		/// </summary>
		public string Title
		{
			get { return Name; }
			set { Name = value; }
		}

		/// <summary>
		/// Gets or sets an aggregate user policy.
		/// </summary>
		internal Policy UserPolicy
		{
			get { return ( aggregatePolicy != null ) ? aggregatePolicy[ ( int )AggregatePolicyOrder.User ] : null; }
			set 
			{ 
				if ( aggregatePolicy == null )
				{
					aggregatePolicy = new Policy[ Enum.GetValues( typeof( AggregatePolicyOrder ) ).Length ];
				}

				aggregatePolicy[ ( int )AggregatePolicyOrder.User ] = value; 
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for creating a new Policy object.
		/// </summary>
		/// <param name="strongName">Strong name of the policy. This should be a well-known GUID.</param>
		/// <param name="shortDescription">A short friendly description of the policy.</param>
		public Policy( string strongName, string shortDescription ) :
			base( shortDescription, Guid.NewGuid().ToString(), NodeTypes.PolicyType )
		{
			properties.AddNodeProperty( PropertyTags.PolicyID, strongName );
		}

		/// <summary>
		/// Constructor that creates a Policy object from a Node object.
		/// </summary>
		/// <param name="node">Node object to create the Policy object from.</param>
		public Policy( Node node ) :
			base( node )
		{
			if ( type != NodeTypes.PolicyType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.PolicyType, type ) );
			}
		}

		/// <summary>
		/// Constructor that creates a Policy object from a ShallowNode object.
		/// </summary>
		/// <param name="collection">Collection that the specified Node object belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create the Policy object from.</param>
		public Policy( Collection collection, ShallowNode shallowNode ) :
			base( collection, shallowNode )
		{
			if ( type != NodeTypes.PolicyType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.PolicyType, type ) );
			}
		}

		/// <summary>
		/// Constructor that creates a Policy object from an Xml document object.
		/// </summary>
		/// <param name="document">Xml document object to create the Policy object from.</param>
		internal Policy( XmlDocument document ) :
			base( document )
		{
			if ( type != NodeTypes.PolicyType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.PolicyType, type ) );
			}
		}
		#endregion

		#region Internal Methods
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds a rule to the policy.
		/// </summary>
		/// <param name="rule">Object that is used to match against the input in the policy.</param>
		public void AddRule( Rule rule )
		{
			properties.ModifyProperty( ( rule.RuleResult == Rule.Result.Allow ) ? AllowRule : DenyRule, rule.ToString() );
		}

		/// <summary>
		/// Adds a time condition to the rule to indicate when the policy is effective.
		/// </summary>
		/// <param name="time">PolicyTime object that determines when the policy is effective.</param>
		public void AddTimeCondition( PolicyTime time )
		{
			properties.ModifyProperty( TimeCondition, time.ToString() );
		}

		/// <summary>
		/// Applies the policy rules on the specified object to determine if the result is allowed or denied.
		/// </summary>
		/// <param name="input">Object that is used to match against the policy rules. The type of object must be
		/// one of the Simias.Syntax types.</param>
		/// <returns>True if the policy allows the operation, otherwise false is returned.</returns>
		public Rule.Result Apply( object input )
		{
			Rule.Result result = Rule.Result.Allow;

			// Walk through the aggregate policy list in order if it is enabled. Otherwise just use the
			// current policy.
			Policy[] policyArray = ( aggregatePolicy != null ) ? aggregatePolicy : new Policy[] { this };

			try
			{
				// Check the deny rules first.
				foreach ( Policy policy in policyArray )
				{
					// Could be holes.
					if ( policy != null )
					{
						// Get all of the deny rules for this policy.
						MultiValuedList mvl = policy.Properties.GetProperties( DenyRule );
						foreach ( Property p in mvl )
						{
							// Apply the rule to see if it passes.
							Rule rule = new Rule( p.Value as string );
							result = rule.Apply( input );
							if ( result == Rule.Result.Deny )
							{
								throw new PolicyException();
							}
						}
					}
				}

				// Check the allow rules next.
				foreach ( Policy policy in policyArray )
				{
					// Could be holes.
					if ( policy != null )
					{
						// Get all of the allow rules for this policy.
						MultiValuedList mvl = policy.Properties.GetProperties( AllowRule );
						foreach( Property p in mvl )
						{
							// Apply the rule to see if it passes.
							Rule rule = new Rule( p.Value as string );
							result = rule.Apply( input );
							if ( result == Rule.Result.Deny )
							{
								throw new PolicyException();
							}
						}
					}
				}

				// Apply the time condition for this policy.
				foreach ( Policy policy in policyArray )
				{
					// Could be holes.
					if ( policy != null )
					{
						// Get the time rule if it exists.
						Property p = policy.Properties.GetSingleProperty( TimeCondition );
						if ( p != null )
						{
							// Apply the time rule to see if it passes.
							PolicyTime time = new PolicyTime( p.Value as string );
							result = time.Apply();
							if ( result == Rule.Result.Deny )
							{
								throw new PolicyException();
							}
						}
					}
				}
			}
			catch ( PolicyException )
			{}

			return result;
		}

		/// <summary>
		/// Removes the specified rule from the policy.
		/// </summary>
		/// <param name="rule">Rule that is used to match against the input in the policy.</param>
		public void DeleteRule( Rule rule )
		{
			MultiValuedList mvl = properties.GetProperties( ( rule.RuleResult == Rule.Result.Allow ) ? AllowRule : DenyRule );
			foreach( Property p in mvl )
			{
				Rule r = new Rule( p.Value as string );
				if ( r == rule )
				{
					p.Delete();
					break;
				}
			}
		}

		/// <summary>
		/// Removes the time condition for the policy.
		/// </summary>
		public void DeleteTimeCondition()
		{
			properties.DeleteSingleProperty( TimeCondition );
		}
		#endregion
	}


	/// <summary>
	/// Collection Store policy manager.
	/// </summary>
	public class PolicyManager
	{
		#region Class Members
		/// <summary>
		/// Store handle.
		/// </summary>
		private Store store;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of this object.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		public PolicyManager( Store store )
		{
			this.store = store;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Saves and associates the Policy with the current user on the current machine. 
		/// This Policy will not be effective for any other user that is being impersonated.
		/// The caller must possess Admin rights in order to commit a Policy.
		/// </summary>
		/// <param name="policy">Policy to be saved.</param>
		public void CommitPolicy( Policy policy )
		{
			// Add a relationship property to the LocalDatabase object.
			LocalDatabase localDb = store.GetDatabaseObject();
			policy.Properties.ModifyNodeProperty( PropertyTags.PolicyAssociation, new Relationship( localDb.ID ) );
			localDb.Commit( policy );
		}

		/// <summary>
		/// Saves and associates the Policy with the domain.
		/// The caller must possess Admin rights in order to commit a Policy.
		/// </summary>
		/// <param name="policy">Policy to be saved.</param>
		/// <param name="domainID">Identifier of domain to associate Policy with.</param>
		public void CommitPolicy( Policy policy, string domainID )
		{
			// Add a relationship property to the Roster object.
			Roster roster = store.GetRoster( domainID );
			if ( roster == null )
			{
				throw new CollectionStoreException( String.Format( "Roster does not exist for domain {0}.", domainID ) );
			}

			policy.Properties.ModifyNodeProperty( PropertyTags.PolicyAssociation, new Relationship( roster.ID ) );
			roster.Commit( policy );
		}

		/// <summary>
		/// Saves and associates the Policy with the user.
		/// The caller must possess Admin rights in order to commit a Policy.
		/// </summary>
		/// <param name="policy">Policy to be saved.</param>
		/// <param name="member">Member to associate the Policy with.</param>
		public void CommitPolicy( Policy policy, Member member )
		{
			// Get the domain ID for the member.
			string domainID = member.GetDomainID( store );
			if ( domainID == null )
			{
				throw new CollectionStoreException( "Member does not belong to any domain." );
			}

			// Add a relationship property to the POBox object.
			POBox.POBox poBox = POBox.POBox.FindPOBox( store, domainID, member.UserID );
			if ( poBox == null )
			{
				throw new CollectionStoreException( String.Format( "POBox does not exist for member {0}", member.UserID ) );
			}

			policy.Properties.ModifyNodeProperty( PropertyTags.PolicyAssociation, new Relationship( poBox.ID ) );
			poBox.Commit( policy );
		}

		/// <summary>
		/// Saves and associates the Policy with the collection.
		/// The caller must possess Admin rights in order to commit a Policy.
		/// </summary>
		/// <param name="policy">Policy to be saved.</param>
		/// <param name="collection">Collection to associate the Policy with.</param>
		public void CommitPolicy( Policy policy, Collection collection )
		{
			// Add a relationship property to the Collection object.
			policy.Properties.ModifyNodeProperty( PropertyTags.PolicyAssociation, new Relationship( collection.ID ) );
			collection.Commit( policy );
		}

		/// <summary>
		/// Deletes the Policy.
		/// </summary>
		/// <param name="policy">Policy to delete.</param>
		public void DeletePolicy( Policy policy )
		{
			// See if this policy has been committed to a collection. If it hasn't, there
			// is nothing to delete.
			Property p = policy.Properties.GetSingleProperty( BaseSchema.CollectionId );
			if ( p != null )
			{
				Collection c = store.GetCollectionByID( p.Value as string );
				c.Commit( c.Delete( policy ) );
			}
		}

		/// <summary>
		/// Gets an aggregate Policy that is associated with the user. This routine will check for
		/// an associated Policy on the Member first. If no Policy is found, the domain will
		/// be searched for an associated Policy. If there is a local Policy for the user it will
		/// be aggregated with the other Policy if one was found. Otherwise it will be returned.
		/// </summary>
		/// <param name="strongName">Strong name of the Policy.</param>
		/// <param name="member">Member used to lookup the associated aggregate Policy.</param>
		/// <returns>A reference to the associated aggregate Policy if successful. A null is
		/// returned if the Policy does not exist.</returns>
		public Policy GetAggregatePolicy( string strongName, Member member )
		{
			// First look for an exception policy for the member object. If a policy is found,
			// then we don't need to look for a domain policy since the exception policy will
			// override the domain policy.
			Policy policy = GetPolicy( strongName, member );
			if ( policy == null )
			{
				// Look for a domain policy since there is no exception policy.
				string domainID = member.GetDomainID( store );
				if ( domainID != null )
				{
					policy = GetPolicy( strongName, domainID );
				}
			}

			// Set the aggregation.
			if ( policy != null )
			{
				policy.UserPolicy = policy;
			}

			// Check for a local policy.
			Policy localPolicy = GetPolicy( strongName );
			if ( localPolicy != null )
			{
				// If there is no exception or domain policy return the local policy.
				if ( policy == null )
				{
					policy = localPolicy;
				}

				// Set the aggregation.
				policy.LocalPolicy = localPolicy;
			}

			return policy;
		}

		/// <summary>
		/// Gets an aggregate Policy that is associated with the user and the specified Collection.
		/// This routine will check for an associated Policy on the Member first. If no Policy is 
		/// found, the domain will be searched for an associated Policy. If there is a local Policy 
		/// for the user it will be aggregated with the other Policy if one was found.  Otherwise 
		/// it will be returned. The procedure for the local Policy is also followed for an 
		/// associated Collection Policy.
		/// </summary>
		/// <param name="strongName">Strong name of the Policy.</param>
		/// <param name="member">Member used to lookup the associated aggregate Policy.</param>
		/// <param name="collection">Collection used to lookup the associated aggregate Policy.</param>
		/// <returns>A reference to the associated aggregate Policy if successful. A null is
		/// returned if the Policy does not exist.</returns>
		public Policy GetAggregatePolicy( string strongName, Member member, Collection collection )
		{
			// Get the aggregate for the member.
			Policy policy = GetAggregatePolicy( strongName, member );

			// Check for a collection policy.
			Policy collectionPolicy = GetPolicy( strongName, collection );
			if ( collectionPolicy != null )
			{
				// If there is no exception or domain policy return the local policy.
				if ( policy == null )
				{
					policy = collectionPolicy;
				}

				// Set the aggregation.
				policy.CollectionPolicy = collectionPolicy;
			}

			return policy;
		}

		/// <summary>
		/// Gets the Policy associated with the current user on the current machine.
		/// </summary>
		/// <param name="strongName">Strong name of the Policy.</param>
		/// <returns>A reference to the associated Policy if successful. A null is 
		/// returned if the Policy does not exist.</returns>
		public Policy GetPolicy( string strongName )
		{
			Policy policy = null;

			// Search the local database for the specified policy.
			LocalDatabase localDb = store.GetDatabaseObject();
			ICSList list = localDb.Search( PropertyTags.PolicyID, strongName, SearchOp.Equal );
			foreach ( ShallowNode sn in list )
			{
				policy = new Policy( localDb, sn );
				break;
			}

			return policy;
		}

		/// <summary>
		/// Gets the Policy that is associated with the domain.
		/// </summary>
		/// <param name="strongName">Strong name of the Policy.</param>
		/// <param name="domainID">Identifier for the domain to use to lookup the Policy.</param>
		/// <returns>A reference to the associated Policy if successful. A null is
		/// returned if the Policy does not exist.</returns>
		public Policy GetPolicy( string strongName, string domainID )
		{
			Policy policy = null;

			// Search the domain roster for the specified policy.
			Roster roster = store.GetRoster( domainID );
			if ( roster != null )
			{
				ICSList list = roster.Search( PropertyTags.PolicyID, strongName, SearchOp.Equal );
				foreach ( ShallowNode sn in list )
				{
					policy = new Policy( roster, sn );
					break;
				}
			}

			return policy;
		}

		/// <summary>
		/// Gets the Policy that is associated with the user.
		/// </summary>
		/// <param name="strongName">Strong name of the Policy.</param>
		/// <param name="member">Member used to lookup the associated Policy.</param>
		/// <returns>A reference to the associated Policy if successful. A null is
		/// returned if the Policy does not exist.</returns>
		public Policy GetPolicy( string strongName, Member member )
		{
			Policy policy = null;

			// Lookup the domain ID for the member.
			string domainID = member.GetDomainID( store );
			if ( domainID != null )
			{
				// Search the member's POBox for the specified policy.
				POBox.POBox poBox = POBox.POBox.FindPOBox( store, domainID, member.UserID );
				if ( poBox != null )
				{
					ICSList list = poBox.Search( PropertyTags.PolicyID, strongName, SearchOp.Equal );
					foreach ( ShallowNode sn in list )
					{
						policy = new Policy( poBox, sn );
						break;
					}
				}
			}

			return policy;
		}

		/// <summary>
		/// Gets the Policy that is associated with the collection.
		/// </summary>
		/// <param name="strongName">Strong name of the Policy.</param>
		/// <param name="collection">Collection used to lookup the associated Policy.</param>
		/// <returns>A reference to the associated Policy if successful. A null is
		/// returned if the Policy does not exist.</returns>
		public Policy GetPolicy( string strongName, Collection collection )
		{
			Policy policy = null;

			// Search the collection for the specified policy.
			ICSList list = collection.Search( PropertyTags.PolicyID, strongName, SearchOp.Equal );
			foreach ( ShallowNode sn in list )
			{
				policy = new Policy( collection, sn );
				break;
			}

			return policy;
		}

		/// <summary>
		/// Gets a list of all the Policies for the user.
		/// </summary>
		/// <param name="member">Member to get Policies for.</param>
		/// <returns>An ICSList that contains all of the Policies that are associated with the user.</returns>
		public ICSList GetPolicyList( Member member )
		{
			ICSList policyList = new ICSList();

			// Get the domain from the member object. If the member has not been committed, it could return
			// a null in which case no policy can be applied.
			string domainID = member.GetDomainID( store );
			if ( domainID != null )
			{
				// Table to keep aggregate policies in.
				Hashtable policyHash = new Hashtable();

				// Get a list of all the local policies first.
				LocalDatabase localDb = store.GetDatabaseObject();
				ICSList tempList = localDb.Search( BaseSchema.ObjectType, NodeTypes.PolicyType, SearchOp.Equal );
				foreach( ShallowNode sn in tempList )
				{
					Policy p = new Policy( localDb, sn );
					policyHash[ p.StrongName ] = p;
				}

				// Get a list of all the policies contained in the domain replacing any overridden
				// local policies.
				tempList = GetPolicyList( domainID );
				foreach ( Policy p in tempList )
				{
					policyHash[ p.StrongName ] = p;
				}

				// Get all of the user policies replacing any overridden domain policies.
				POBox.POBox poBox = POBox.POBox.FindPOBox( store, domainID, member.UserID );
				if ( poBox != null )
				{
					tempList = poBox.Search( BaseSchema.ObjectType, NodeTypes.PolicyType, SearchOp.Equal );
					foreach( ShallowNode sn in tempList )
					{
						Policy p = new Policy( poBox, sn );
						policyHash[ p.StrongName ] = p;
					}
				}

				// Now build the final policy list.
				foreach( Policy p in policyHash.Values )
				{
					policyList.Add( p );
				}
			}

			return policyList;
		}

		/// <summary>
		/// Gets a list of all the Policies for the user on the collection.
		/// </summary>
		/// <param name="member">Member to get Policies for.</param>
		/// <param name="collection">Collection that may have associated policies.</param>
		/// <returns>An ICSList that contains all of the Policies that are associated with the user.</returns>
		public ICSList GetPolicyList( Member member, Collection collection )
		{
			ICSList policyList = new ICSList();

			// Table to keep aggregate policies in.
			Hashtable policyHash = new Hashtable();

			// Get all policies associated with this collection.
			ICSList tempList = collection.Search( BaseSchema.ObjectType, NodeTypes.PolicyType, SearchOp.Equal );
			foreach( ShallowNode sn in tempList )
			{
				Policy p = new Policy( collection, sn );
				policyHash[ p.StrongName ] = p;
			}

			// Get the policy list for the member.
			tempList = GetPolicyList( member );
			foreach( Policy p in tempList )
			{
				policyHash[ p.StrongName ] = p;
			}

			// Now build the final policy list.
			foreach( Policy p in policyHash.Values )
			{
				policyList.Add( p );
			}

			return policyList;
		}

		/// <summary>
		/// Gets a list of all the Policies for the domain.
		/// </summary>
		/// <param name="domainID">Identifier for a domain.</param>
		/// <returns>An ICSList that contains all of the Policies that are associated with the domain.</returns>
		public ICSList GetPolicyList( string domainID )
		{
			ICSList policyList = new ICSList();

			// Get the roster for the specified domain.
			Roster roster = store.GetRoster( domainID );
			if ( roster != null )
			{
				ICSList tempList = roster.Search( BaseSchema.ObjectType, NodeTypes.PolicyType, SearchOp.Equal );
				foreach ( ShallowNode sn in tempList )
				{
					policyList.Add( new Policy( roster, sn ) );
				}
			}

			return policyList;
		}
		#endregion
	}


	/// <summary>
	/// Object used to encapsulate rule into a more friendly form.
	/// </summary>
	[ Serializable ]
	public class Rule : ISerializable
	{
		#region Class Members
		/// <summary>
		/// Static string used to format the XML used to store this rule as a string.
		/// </summary>
		static private string RuleNameTag = "Rule";
		static private string RuleOperationTag = "Operation";
		static private string RuleResultTag = "Result";
		static private string RuleSyntaxTag = "Syntax";

		/// <summary>
		/// Result of applying input value to policy rule.
		/// </summary>
		public enum Result
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
		public enum Operation
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

		/// <summary>
		/// Object that is used to match against the input object when the Apply method is called.
		/// The type of object must be one of the Simias.Syntax types.
		/// </summary>
		private object operand;

		/// <summary>
		/// Syntax type of the operand.
		/// </summary>
		private Syntax syntax;

		/// <summary>
		/// Operation to perform between input and operand.
		/// </summary>
		private Operation operation;

		/// <summary>
		/// Result to return when operation is true.
		/// </summary>
		private Result result;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the object that is used to match against the input object when the
		/// Apply method is called.
		/// </summary>
		public object Operand
		{
			get { return operand; }
			set 
			{ 
				// Validate the object type and operation.
				syntax = ValidateOperation( value, operation );
				operand = value; 
			}
		}

		/// <summary>
		/// Gets or sets the match type operation to perform between the input and operand.
		/// </summary>
		public Operation RuleOperation
		{
			get { return operation; }
			set 
			{ 
				// Validate the operation.
				ValidateOperation( operand, value );
				operation = value; 
			}
		}

		/// <summary>
		/// Gets or set the result to return when operation is true.
		/// </summary>
		public Result RuleResult
		{
			get { return result; }
			set { result = value; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes the object with the specified parameters.
		/// </summary>
		/// <param name="operand">Object that is used to match against the input object when the Apply method is called.
		/// The type of object must be one of the Simias.Syntax types.</param>
		/// <param name="operation">Operation to perform between input and operand.</param>
		/// <param name="result">Result to return when operation is true.</param>
		public Rule( object operand, Operation operation, Result result )
		{
			// Validate the operation is valid for the object type.
			this.syntax = ValidateOperation( operand, operation );

			// Initialize the object.
			this.operand = operand;
			this.operation = operation;
			this.result = result;
		}

		/// <summary>
		/// Initializes the object with the specified rule string.
		/// </summary>
		/// <param name="ruleString">String that contains a rule representation.</param>
		public Rule( string ruleString )
		{
			// The rule string should be a valid XML document.
			XmlDocument document = new XmlDocument();
			document.LoadXml( ruleString );
			XmlElement element = document.DocumentElement;

			operation = ( Operation )Enum.Parse( typeof( Operation ), element.GetAttribute( RuleOperationTag ) );
			result = ( Result )Enum.Parse( typeof( Result ), element.GetAttribute( RuleResultTag ) );

			syntax = ( Syntax )Enum.Parse( typeof( Syntax ), element.GetAttribute( RuleSyntaxTag ) );
			Property p = new Property( String.Empty, syntax, element.InnerText );
			operand = p.Value;
		}

		/// <summary>
		/// Special constructor used to deserialize a Rule object.
		/// </summary>
		/// <param name="info">The SerializationInfo populated with the data.</param>
		/// <param name="context">The source (see StreamingContext) for this serialization.</param>
		protected Rule( SerializationInfo info, StreamingContext context ) :
			this( info.GetString( "RuleData" ) )
		{
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Given the result of a comparision, returns either the store result value or its
		/// opposite.
		/// </summary>
		/// <param name="cmpValue">Result of comparision.</param>
		/// <returns>Stored result or its opposite.</returns>
		private Result GetResult( int cmpValue )
		{
			bool status = true;

			// Now see how the operation compares.
			switch ( operation )
			{
				case Operation.Equal:
				{
					status = ( cmpValue == 0 ) ? true : false;
					break;
				}

				case Operation.Not_Equal:
				{
					status = ( cmpValue != 0 ) ? true : false;
					break;
				}

				case Operation.Greater:
				{
					status = ( cmpValue > 0 ) ? true : false;
					break;
				}
							
				case Operation.Greater_Equal:
				{
					status = ( ( cmpValue == 0 ) || ( cmpValue > 0 ) ) ? true : false;
					break;
				}

				case Operation.Less:
				{
					status = ( cmpValue < 0 ) ? true : false;
					break;
				}

				case Operation.Less_Equal:
				{
					status = ( ( cmpValue < 0 ) || ( cmpValue == 0 ) ) ? true : false;
					break;
				}
			}

			if ( result == Result.Allow )
			{
				return status ? result : Result.Deny;
			}
			else
			{
				return status ? result : Result.Allow;
			}
		}

		/// <summary>
		/// Checks if the object type is valid for the operation.
		/// </summary>
		/// <param name="operand">Object to perform operation on.</param>
		/// <param name="operation">Operation to perform.</param>
		/// <returns>The syntax type for the operand.</returns>
		private Syntax ValidateOperation( object operand, Operation operation )
		{
			Syntax syntax = Property.GetSyntaxType( operand );

			// Make sure that the object type is correct for the corresponding operation.
			switch ( syntax )
			{
				case Syntax.Relationship:
				case Syntax.Uri:
				case Syntax.XmlDocument:
				{
					// Only equals and !equals are supported.
					if ( ( operation != Operation.Equal ) && ( operation != Operation.Not_Equal ) )
					{
						throw new CollectionStoreException( "Invalid operation for rule object type." );
					}
					break;
				}

				case Syntax.Boolean:
				case Syntax.Byte:
				case Syntax.Char:
				case Syntax.Int16:
				case Syntax.Int32:
				case Syntax.Int64:
				case Syntax.SByte:
				case Syntax.Single:
				case Syntax.UInt16:
				case Syntax.UInt32:
				case Syntax.UInt64:
				case Syntax.DateTime:
				case Syntax.TimeSpan:
				case Syntax.String:
				{
					// All operations are supported.
					break;
				}

				default:
				{
					throw new CollectionStoreException( "Invalid operand type for rule." );
				}
			}

			return syntax;
		}
		#endregion

		#region ISerializable Members
		/// <summary>
		/// Called by the ISerializable interface to serialize a Rule object.
		/// </summary>
		/// <param name="info">The SerializationInfo to populate with data.</param>
		/// <param name="context">The destination (see StreamingContext) for this serialization.</param>
		public virtual void GetObjectData( SerializationInfo info, StreamingContext context )
		{
			// Turn this instance into a string.
			info.AddValue( "RuleData", ToString() );
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Determines whether two Object instances are equal.
		/// </summary>
		/// <param name="rule1">First object to compare.</param>
		/// <param name="rule2">Second object to compare.</param>
		/// <returns>True if the first object is equal to the second object.
		/// Otherwise false is returned.</returns>
		public static bool operator==( Rule rule1, Rule rule2 )
		{
			Property p1 = new Property( String.Empty, rule1.Operand );
			Property p2 = new Property( String.Empty, rule2.Operand );

			return ( ( p1.Type == p2.Type ) && 
				( p1.ValueString == p2.ValueString ) &&
				( rule1.operation == rule2.operation ) &&
				( rule1.result == rule2.result ) ) ? true : false;
		}

		/// <summary>
		/// Determines whether two Object instances are not equal.
		/// </summary>
		/// <param name="rule1">First object to compare.</param>
		/// <param name="rule2">Second object to compare.</param>
		/// <returns>True if the first object is not equal to the second object.
		/// Otherwise false is returned.</returns>
		public static bool operator!=( Rule rule1, Rule rule2 )
		{
			return !( rule1 == rule2 );
		}

		/// <summary>
		/// Applies the rule to the input object and returns the result.
		/// </summary>
		/// <param name="input">Object to apply rule to.</param>
		/// <returns>The predefined result if the operation is true.</returns>
		public Result Apply( object input )
		{
			Result ruleResult = Result.Deny;

			// Make sure that the object is of the correct type for the rule.
			if ( !input.GetType().Equals( operand ) )
			{
				throw new CollectionStoreException( "The object type is incorrect for this rule." );
			}

			// Perform the operation based on the type of object.
			switch ( syntax )
			{
				case Syntax.Boolean:
				{
					ruleResult = GetResult( ( ( bool )input ).CompareTo( operand ) );
					break;
				}

				case Syntax.Relationship:
				case Syntax.Uri:
				case Syntax.XmlDocument:
				{
					if ( operation == Operation.Equal )
					{
						ruleResult = GetResult( input.Equals( operand ) ? 0 : 1 );
					}
					else
					{
						ruleResult = GetResult( input.Equals( operand ) ? 1 : 0 );
					}
					break;
				}

				case Syntax.Byte:
				{
					ruleResult = GetResult( ( ( byte )input ).CompareTo( operand ) );
					break;
				}

				case Syntax.Char:
				{
					ruleResult = GetResult( ( ( char )input ).CompareTo( operand ) );
					break;
				}

				case Syntax.Int16:
				{
					ruleResult = GetResult( ( ( short )input ).CompareTo( operand ) );
					break;
				}

				case Syntax.Int32:
				{
					ruleResult = GetResult( ( ( int )input ).CompareTo( operand ) );
					break;
				}

				case Syntax.Int64:
				{
					ruleResult = GetResult( ( ( long )input ).CompareTo( operand ) );
					break;
				}

				case Syntax.SByte:
				{
					ruleResult = GetResult( ( ( sbyte )input ).CompareTo( operand ) );
					break;
				}

				case Syntax.Single:
				{
					ruleResult = GetResult( ( ( float )input ).CompareTo( operand ) );
					break;
				}

				case Syntax.UInt16:
				{
					ruleResult = GetResult( ( ( ushort )input ).CompareTo( operand ) );
					break;
				}

				case Syntax.UInt32:
				{
					ruleResult = GetResult( ( ( uint )input ).CompareTo( operand ) );
					break;
				}

				case Syntax.UInt64:
				{
					ruleResult = GetResult( ( ( ulong )input ).CompareTo( operand ) );
					break;
				}

				case Syntax.DateTime:
				{
					ruleResult = GetResult( ( ( DateTime )input ).CompareTo( operand ) );
					break;
				}

				case Syntax.TimeSpan:
				{
					ruleResult = GetResult( ( ( TimeSpan )input ).CompareTo( operand ) );
					break;
				}

				case Syntax.String:
				{
					ruleResult = GetResult( ( input as string ).CompareTo( operand ) );
					break;
				}
			}

			return ruleResult;
		}

		/// <summary>
		/// Determines whether two Object instances are equal.
		/// </summary>
		/// <param name="rule">Object to compare to the current object.</param>
		/// <returns>True if the specified object is equal to the current object.
		/// Otherwise false is returned.</returns>
		public override bool Equals( object rule )
		{
			return ( this == rule as Rule );
		}

		/// <summary>
		/// Serves as a hash function for a particular type, suitable for use in hashing algorithms 
		/// and data structures like a hash table.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		/// <summary>
		/// Converts the value of this instance to its equivalent string.
		/// </summary>
		/// <returns>The value of this instance.</returns>
		public override string ToString()
		{
			// Convert the rule into a property with no name.
			Property p = new Property( String.Empty, operand );

			// Create an XML document to hold the data.
			XmlDocument document = new XmlDocument();

			// Create the root and only element.
			XmlElement element = document.CreateElement( RuleNameTag );
			element.SetAttribute( RuleOperationTag, Enum.GetName( typeof( Operation ), operation ) );
			element.SetAttribute( RuleResultTag, result.ToString() );
			element.SetAttribute( RuleSyntaxTag, syntax.ToString() );
			element.InnerText = p.ValueString;
			document.AppendChild( element );

			return document.InnerXml;
		}
		#endregion
	}

	/// <summary>
	/// Object used to validate when a policy is valid.
	/// </summary>
	[ Serializable ]
	public class PolicyTime
	{
		#region Class Members
		/// <summary>
		/// Size of the bit table used to keep track of the time condition. 
		/// 7 Days in a week - 24 hours in a day.
		/// </summary>
		private const int tableSize = 7 * 24;

		/// <summary>
		/// Table used to represent the time in a week.
		/// </summary>
		private BitArray table;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		public PolicyTime()
		{
			table = new BitArray( tableSize );
		}

		/// <summary>
		/// Initializes a new instance of the object class with the specified time array.
		/// </summary>
		/// <param name="timeArray">Array of bytes that represent time values in a 7 day week, 24 hour period.</param>
		public PolicyTime( byte[] timeArray )
		{
			table = new BitArray( timeArray );
		}

		/// <summary>
		/// Initializes a new instance of the object class with the specified time string.
		/// </summary>
		/// <param name="timeString">String that represent time values in a 7 day week, 24 hour period.</param>
		public PolicyTime( string timeString ) :
			this( new ASCIIEncoding().GetBytes( timeString ) )
		{
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the table value stored at the specified time.
		/// </summary>
		/// <param name="day">Day of week to get value for.</param>
		/// <param name="hour">Hour of day ( 0 - 23 ) to get value for.</param>
		private bool GetTableValue( DayOfWeek day, int hour )
		{
			if ( ( hour < 0 ) || ( hour > 23 ) )
			{
				throw new SimiasException( "The hour parameter is out of range.", new ArgumentOutOfRangeException( "hour", hour, "Must be between 0 and 23." ) );
			}

			return table[ ( ( int )day * 24 ) + hour ];
		}

		/// <summary>
		/// Set or clears the specified day of week and the hour.
		/// </summary>
		/// <param name="day">Day of week to set or clear.</param>
		/// <param name="hour">Hour of day to set or clear ( 0 - 23 ).</param>
		/// <param name="value">Value to set in table.</param>
		private void SetTableValue( DayOfWeek day, int hour, bool value )
		{
			if ( ( hour < 0 ) || ( hour > 23 ) )
			{
				throw new SimiasException( "The hour parameter is out of range.", new ArgumentOutOfRangeException( "hour", hour, "Must be between 0 and 23." ) );
			}

			table.Set( ( ( int )day * 24 ) + hour, value );
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Applies the time condition to determine if the policy is valid.
		/// </summary>
		/// <returns>Rule.Result.Allow is returned if the policy is valid.
		/// Otherwise Rule.Result.Deny is returned.</returns>
		public Rule.Result Apply()
		{
			return Apply( DateTime.Now );
		}

		/// <summary>
		/// Applies the time condition to determine if the policy is valid.
		/// </summary>
		/// <param name="time">Time to check if policy is valid.</param>
		/// <returns>Rule.Result.Allow is returned if the policy is valid.
		/// Otherwise Rule.Result.Deny is returned.</returns>
		public Rule.Result Apply( DateTime time )
		{
			return ( IsSet( time.DayOfWeek, time.Hour ) ) ? Rule.Result.Allow : Rule.Result.Deny;
		}

		/// <summary>
		/// Clears all selected times.
		/// </summary>
		public void Clear()
		{
			table.SetAll( false );
		}

		/// <summary>
		/// Clears the specified selected time.
		/// </summary>
		/// <param name="day">Day of week to clear.</param>
		/// <param name="hour">Hour of day to clear ( 0 - 23 ).</param>
		public void Clear( DayOfWeek day, int hour )
		{
			SetTableValue( day, hour, false );
		}

		/// <summary>
		/// Clears the specified selected range of time.
		/// </summary>
		/// <param name="startDay">Day of week to start clear.</param>
		/// <param name="endDay">Day of week to end clear.</param>
		/// <param name="startHour">Hour of day to start clear.</param>
		/// <param name="endHour">Hour of day to end clear.</param>
		public void Clear( DayOfWeek startDay, DayOfWeek endDay, int startHour, int endHour )
		{
			for ( DayOfWeek day = startDay; day <= endDay; ++day )
			{
				for ( int hour = startHour; hour < endHour; ++hour )
				{
					Clear( day, hour );
				}
			}
		}

		/// <summary>
		/// Indicates whether the specified time is clear.
		/// </summary>
		/// <param name="day">Day of week.</param>
		/// <param name="hour">Hour of day ( 0 - 23 ).</param>
		/// <returns>True if the time is clear, otherwise false is returned.</returns>
		public bool IsClear( DayOfWeek day, int hour )
		{
			return !GetTableValue( day, hour );
		}

		/// <summary>
		/// Indicates whether the specified time is selected.
		/// </summary>
		/// <param name="day">Day of week.</param>
		/// <param name="hour">Hour of day ( 0 - 23 ).</param>
		/// <returns>True if the time is selected, otherwise false is returned.</returns>
		public bool IsSet( DayOfWeek day, int hour )
		{
			return GetTableValue( day, hour );
		}

		/// <summary>
		/// Sets all time values to selected.
		/// </summary>
		public void Set()
		{
			table.SetAll( true );
		}

		/// <summary>
		/// Sets the specified time as selected.
		/// </summary>
		/// <param name="day">Day of week to select.</param>
		/// <param name="hour">Hour of day to select ( 0 - 23 ).</param>
		public void Set( DayOfWeek day, int hour )
		{
			SetTableValue( day, hour, true );
		}

		/// <summary>
		/// Sets the specified range of time as selected.
		/// </summary>
		/// <param name="startDay">Day of week to start selection.</param>
		/// <param name="endDay">Day of week to end selection.</param>
		/// <param name="startHour">Hour of day to start selection.</param>
		/// <param name="endHour">Hour of day to end selection.</param>
		public void Set( DayOfWeek startDay, DayOfWeek endDay, int startHour, int endHour )
		{
			for ( DayOfWeek day = startDay; day <= endDay; ++day )
			{
				for ( int hour = startHour; hour < endHour; ++hour )
				{
					Set( day, hour );
				}
			}
		}

		/// <summary>
		/// Converts the object to a byte array that represents time values in a 7 day week, 24 hour period.
		/// </summary>
		/// <returns>A byte array that represents time values in a 7 day week, 24 hour period.</returns>
		public byte[] ToByteArray()
		{
			byte[] byteArray = new byte[ tableSize / 8 ];
			table.CopyTo( byteArray, 0 );
			return byteArray;
		}

		/// <summary>
		/// Converts the value of this instance to its equivalent string.
		/// </summary>
		/// <returns>The value of this instance.</returns>
		public override string ToString()
		{
			ASCIIEncoding encode = new ASCIIEncoding();
			return encode.GetString( ToByteArray() );
		}
		#endregion
	}
}
