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
using System.Collections;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using Simias.Client;
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
		/// Property names to store rules and time condition on the policy Node.
		/// </summary>
		static private string RuleList = "RuleList";
		static private string TimeCondition = "TimeCondition";

		/// <summary>
		/// Internal flags.
		/// </summary>
		[ FlagsAttribute ]
		private enum PolicyFlags
		{
			/// <summary>
			/// Indicates that a Property object is a policy value.
			/// </summary>
			ValueFlag = 1
		};

		/// <summary>
		/// Used to hold all Policies associated with a Member.
		/// </summary>
		private ArrayList aggregatePolicy = new ArrayList();
		#endregion

		#region Properties
		/// <summary>
		/// Gets whether this policy is a stand-alone or aggregate policy.
		/// </summary>
		private bool IsAggregate
		{
			get { return ( aggregatePolicy.Count > 0 ) ? true : false; }
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
		/// Gets or sets whether this is a system policy.
		/// </summary>
		public bool IsSystemPolicy
		{
			get
			{
				Property p = properties.GetSingleProperty( PropertyTags.SystemPolicy );
				return ( p != null ) ? ( bool )p.Value : false;
			}

			set { properties.ModifyNodeProperty( PropertyTags.SystemPolicy, value ); }
		}

		/// <summary>
		/// Returns the PolicyTime object for this policy. If there is no time condition,
		/// null is returned. 
		/// </summary>
		public PolicyTime PolicyTime
		{
			get 
			{
				Property p = properties.GetSingleProperty( TimeCondition );
				return ( p != null ) ? new PolicyTime( p.Value as string ) : null;
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
				Policy[] policyArray = IsAggregate ? aggregatePolicy.ToArray( typeof( Policy ) ) as Policy[] : new Policy[] { this };

				foreach( Policy policy in policyArray )
				{
					// Get the rule list.
					MultiValuedList mvl = policy.properties.GetProperties( RuleList );
					foreach( Property p in mvl )
					{
						ruleList.Add( new Rule( p.Value ) );
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
		/// Returns a collection of policy values. If this is an aggregated policy,
		/// all values from the aggregated policies are returned.
		/// </summary>
		public PolicyValue[] Values
		{
			get
			{
				ArrayList valueList = new ArrayList();
				Policy[] policyArray = IsAggregate ? aggregatePolicy.ToArray( typeof( Policy ) ) as Policy[] : new Policy[] { this };

				foreach( Policy policy in policyArray )
				{
					// Get a list of all properties that contain the PolicyValue flag.
					MultiValuedList mvl = new MultiValuedList( policy.Properties, ( uint )PolicyFlags.ValueFlag );
					foreach ( Property p in mvl )
					{
						valueList.Add( new PolicyValue( p.Name, p.Value ) );
					}
				}

				return valueList.ToArray( typeof( PolicyValue ) ) as PolicyValue[];
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for creating a new Policy object.
		/// </summary>
		/// <param name="policyID">Strong name of the policy. This should be a well-known GUID.</param>
		/// <param name="shortDescription">A short friendly description of the policy.</param>
		public Policy( string policyID, string shortDescription ) :
			base( shortDescription, Guid.NewGuid().ToString(), NodeTypes.PolicyType )
		{
			properties.AddNodeProperty( PropertyTags.PolicyID, policyID );
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

		#region Private Methods
		/// <summary>
		/// Finds the property object that represents the specified rule.
		/// </summary>
		/// <param name="rule">Rule to find in the policy.</param>
		/// <returns>The property object that represents the rule or null if the rule does not exist.</returns>
		private Property FindRule( Rule rule )
		{
			Property property = null;

			// See if the rule already exists so we don't add duplicate rules.
			MultiValuedList mvl = properties.GetProperties( RuleList );
			foreach( Property p in mvl )
			{
				if ( rule == new Rule( p.Value ) )
				{
					property = p;
					break;
				}
			}

			return property;
		}

		/// <summary>
		/// Finds the policy value as a property that represents the specified name.
		/// </summary>
		/// <param name="name">Name of the policy value.</param>
		/// <returns>A Property object that contains the policy value.</returns>
		private Property FindValue( string name )
		{
			Property property = null;

			MultiValuedList mvl = properties.GetProperties( name );
			foreach( Property p in mvl )
			{
				if ( ( p.Flags & ( ushort )PolicyFlags.ValueFlag ) == ( ushort )PolicyFlags.ValueFlag )
				{
					property = p;
					break;
				}
			}

			return property;
		}
		#endregion

		#region Internal Methods
		/// <summary>
		///  Adds a policy to the aggregate list.
		/// </summary>
		/// <param name="policy">Policy that gets added to the aggregate list.</param>
		internal void AddAggregatePolicy( Policy policy )
		{
			aggregatePolicy.Add( policy );
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds a rule to the policy.
		/// </summary>
		/// <param name="rule">Object that is used to match against the input in the policy.</param>
		public void AddRule( Rule rule )
		{
			// Make sure the rule doesn't already exist.
			if ( FindRule( rule ) == null )
			{
				// Add the new rule.
				properties.AddNodeProperty( RuleList, rule.ToXml() );
			}
		}

		/// <summary>
		/// Adds a time condition to the rule to indicate when the policy is effective.
		/// </summary>
		/// <param name="time">PolicyTime object that determines when the policy is effective.</param>
		public void AddTimeCondition( PolicyTime time )
		{
			properties.ModifyNodeProperty( TimeCondition, time.ToString() );
		}

		/// <summary>
		/// Adds a named value to the policy.
		/// </summary>
		/// <param name="name">Name of the value.</param>
		/// <param name="value">Value to set on the policy.</param>
		public void AddValue( string name, object value )
		{
			// See if there is already a value by this name.
			Property p = FindValue( name );
			if ( p != null )
			{
				// Just change the current value.
				p.Value = value;
			}
			else
			{
				// Set the flag for this property indicating that it is a policy value.
				p = new Property( name, value );
				p.Flags |= ( ushort )PolicyFlags.ValueFlag;
				properties.AddProperty( p );
			}
		}

		/// <summary>
		/// Applies the policy rules on the specified object to determine if the result is allowed or denied.
		/// </summary>
		/// <param name="input">Object that is used to match against the policy rules. The type of object must be
		/// one of the Simias.Syntax types.</param>
		/// <returns>True if the policy allows the operation, otherwise false is returned.</returns>
		public Rule.Result Apply( object input )
		{
			bool allowedByAllowRule = false;
			bool hasAllowRule = false;
			Rule.Result returnResult = Rule.Result.Allow;

			// Walk through the aggregate policy list in order if it is enabled. 
			// Otherwise just use the current policy.
			Policy[] policyArray = IsAggregate ? aggregatePolicy.ToArray( typeof( Policy ) ) as Policy[] : new Policy[] { this };

			// Check the deny rules first.
			foreach ( Policy policy in policyArray )
			{
				// See if there is a time condition as to when this policy is effective.
				Property p = policy.Properties.GetSingleProperty( TimeCondition );
				if ( ( p == null ) || ( new PolicyTime( p.Value as string ).Apply() == Rule.Result.Allow ) )
				{
					// Get all of the deny rules for this policy.
					MultiValuedList mvl = policy.Properties.GetProperties( RuleList );
					foreach ( Property rp in mvl )
					{
						// Apply the rule to see if it passes.
						Rule rule = new Rule( rp.Value );
						Rule.Result test = rule.Apply( input );

						// All allow rules are checked unless a deny rule is found and the
						// result is deny. Otherwise, if a single allow rule result is
						// found the return result is allowed.
						if ( rule.RuleResult == Rule.Result.Allow )
						{
							// There is at least one allow rule.
							hasAllowRule = true;

							// An allow rule always needs to continue to check all the rules
							// unless a deny rule is found.
							if ( test == Rule.Result.Allow )
							{
								// This flag says that there was an allow rule that returned
								// an allowed status.
								allowedByAllowRule = true;
							}
						}
						else
						{
							// The deny rule overrides all other rules. However, if the
							// result is allow it does not explicitly allow the operation,
							// unless there are no other allow rules.
							if ( test == Rule.Result.Deny )
							{
								// A deny result for a Deny Rule always final.
								returnResult = Rule.Result.Deny;
								break;
							}
						}
					}

					// Don't continue to check other policies if the previous policy denied the rule.
					if ( returnResult == Rule.Result.Deny )
					{
						break;
					}
				}
			}

			// The return result is denied if there were no deny results and there was at least
			// one rule in the policy, but none of the allow rules passed.
			if ( hasAllowRule && !allowedByAllowRule )
			{
				returnResult = Rule.Result.Deny;
			}

			return returnResult;
		}

		/// <summary>
		/// Removes the specified rule from the policy.
		/// </summary>
		/// <param name="rule">Rule that is used to match against the input in the policy.</param>
		public void DeleteRule( Rule rule )
		{
			// Find the rule in the policy.
			Property p = FindRule( rule );
			if ( p != null )
			{
				p.Delete();
			}
		}

		/// <summary>
		/// Removes the time condition for the policy.
		/// </summary>
		public void DeleteTimeCondition()
		{
			properties.DeleteSingleProperty( TimeCondition );
		}

		/// <summary>
		/// Removes the specified value from the policy.
		/// </summary>
		/// <param name="name">Name of the value to delete.</param>
		public void DeleteValue( string name )
		{
			// Find the value in the policy.
			Property p = FindValue( name );
			if ( p != null )
			{
				p.Delete();
			}
		}

		/// <summary>
		/// Gets the policy value from its name.
		/// </summary>
		/// <param name="name">Name of the policy value.</param>
		/// <returns>An object containing the policy value. If the policy value does not exist
		/// a null is returned.</returns>
		public object GetValue( string name )
		{
			Property p = FindValue( name );
			return ( p != null ) ? p.Value : null;
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
		public PolicyManager()
		{
			this.store = Store.GetStore();
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Saves and associates the Policy with the current user on the current machine. 
		/// This Policy will not be effective for any other user that is being impersonated.
		/// The caller must possess Admin rights in order to commit a Policy.
		/// </summary>
		/// <param name="policy">Policy to be saved.</param>
		public void CommitLocalMachinePolicy( Policy policy )
		{
			// Add a relationship property to the LocalDatabase object.
			LocalDatabase localDb = store.GetDatabaseObject();
			policy.IsSystemPolicy = false;
			policy.Properties.ModifyNodeProperty( PropertyTags.PolicyAssociation, new Relationship( localDb ) );
			localDb.Commit( policy );
		}

		/// <summary>
		/// Saves and associates the Policy how it was previously committed. If the policy 
		/// has not been previously committed an exception is thrown.
		/// The caller must possess Admin rights in order to commit a Policy.
		/// </summary>
		/// <param name="policy">Policy to be saved.</param>
		public void CommitPolicy( Policy policy )
		{
			// Get the associated collection from the relationship.
			Property p = policy.Properties.GetSingleProperty( PropertyTags.PolicyAssociation );
			if ( p == null )
			{
				throw new CollectionStoreException( "Policy was not previously committed." );
			}

			// Get the collection object from the relationship.
			Collection c = store.GetCollectionByID( ( p.Value as Relationship ).CollectionID );
			if ( c != null )
			{
				c.Commit( policy );
			}
		}

		/// <summary>
		/// Saves and associates the Policy with the domain.
		/// The caller must possess Admin rights in order to commit a Policy.
		/// </summary>
		/// <param name="policy">Policy to be saved.</param>
		/// <param name="domainID">Identifier of domain to associate Policy with.</param>
		public void CommitPolicy( Policy policy, string domainID )
		{
			// Add a relationship property to the Domain object.
			Domain domain = store.GetDomain( domainID );
			if ( domain == null )
			{
				throw new CollectionStoreException( String.Format( "Domain {0} does not exist.", domainID ) );
			}

			policy.IsSystemPolicy = true;
			policy.Properties.ModifyNodeProperty( PropertyTags.PolicyAssociation, new Relationship( domain ) );
			domain.Commit( policy );
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
			POBox.POBox poBox = POBox.POBox.GetPOBox( store, domainID, member.UserID );

			policy.IsSystemPolicy = true;
			policy.Properties.ModifyNodeProperty( PropertyTags.PolicyAssociation, new Relationship( poBox ) );
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
			policy.IsSystemPolicy = false;
			policy.Properties.ModifyNodeProperty( PropertyTags.PolicyAssociation, new Relationship( collection ) );
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
		/// <param name="policyID">Strong name for the Policy.</param>
		/// <param name="member">Member used to lookup the associated aggregate Policy.</param>
		/// <returns>A reference to the associated aggregate Policy if successful. A null is
		/// returned if the Policy does not exist.</returns>
		public Policy GetAggregatePolicy( string policyID, Member member )
		{
			// First look for an exception policy for the member object. If a policy is found,
			// then we don't need to look for a domain policy since the exception policy will
			// override the domain policy.
			Policy policy = GetPolicy( policyID, member );
			if ( policy == null )
			{
				// Look for a domain policy since there is no exception policy.
				string domainID = member.GetDomainID( store );
				if ( domainID != null )
				{
					policy = GetPolicy( policyID, domainID );
				}
			}

			// Set the aggregation.
			if ( policy != null )
			{
				policy.AddAggregatePolicy( policy );
			}

			// Check for a local policy.
			Policy localPolicy = GetPolicy( policyID );
			if ( localPolicy != null )
			{
				// If there is no exception or domain policy return the local policy.
				if ( policy == null )
				{
					policy = localPolicy;
				}

				// Set the aggregation.
				policy.AddAggregatePolicy( localPolicy );
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
		/// <param name="policyID">Strong name of the Policy.</param>
		/// <param name="member">Member used to lookup the associated aggregate Policy.</param>
		/// <param name="collection">Collection used to lookup the associated aggregate Policy.</param>
		/// <returns>A reference to the associated aggregate Policy if successful. A null is
		/// returned if the Policy does not exist.</returns>
		public Policy GetAggregatePolicy( string policyID, Member member, Collection collection )
		{
			// Get the aggregate for the member.
			Policy policy = GetAggregatePolicy( policyID, member );

			// Check for a collection policy.
			Policy collectionPolicy = GetPolicy( policyID, collection );
			if ( collectionPolicy != null )
			{
				// If there is no exception or domain policy return the local policy.
				if ( policy == null )
				{
					policy = collectionPolicy;
				}

				// Set the aggregation.
				policy.AddAggregatePolicy( collectionPolicy );
			}

			return policy;
		}

		/// <summary>
		/// Gets the Policy associated with the current user on the current machine.
		/// </summary>
		/// <param name="policyID">Strong name of the Policy.</param>
		/// <returns>A reference to the associated Policy if successful. A null is 
		/// returned if the Policy does not exist.</returns>
		public Policy GetPolicy( string policyID )
		{
			Policy policy = null;

			// Search the local database for the specified policy.
			LocalDatabase localDb = store.GetDatabaseObject();
			ICSList list = localDb.Search( PropertyTags.PolicyID, policyID, SearchOp.Equal );
			foreach ( ShallowNode sn in list )
			{
				Policy tempPolicy = new Policy( localDb, sn );
				if ( !tempPolicy.IsSystemPolicy )
				{
					policy = tempPolicy;
					break;
				}
			}

			return policy;
		}

		/// <summary>
		/// Gets the Policy that is associated with the domain.
		/// </summary>
		/// <param name="policyID">Strong name of the Policy.</param>
		/// <param name="domainID">Identifier for the domain to use to lookup the Policy.</param>
		/// <returns>A reference to the associated Policy if successful. A null is
		/// returned if the Policy does not exist.</returns>
		public Policy GetPolicy( string policyID, string domainID )
		{
			Policy policy = null;

			// Search the domain for the specified policy.
			Domain domain = store.GetDomain( domainID );
			if ( domain != null )
			{
				ICSList list = domain.Search( PropertyTags.PolicyID, policyID, SearchOp.Equal );
				foreach ( ShallowNode sn in list )
				{
					Policy tempPolicy = new Policy( domain, sn );
					if ( tempPolicy.IsSystemPolicy )
					{
						policy = tempPolicy;
						break;
					}
				}
			}

			return policy;
		}

		/// <summary>
		/// Gets the Policy that is associated with the user.
		/// </summary>
		/// <param name="policyID">Strong name of the Policy.</param>
		/// <param name="member">Member used to lookup the associated Policy.</param>
		/// <returns>A reference to the associated Policy if successful. A null is
		/// returned if the Policy does not exist.</returns>
		public Policy GetPolicy( string policyID, Member member )
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
					ICSList list = poBox.Search( PropertyTags.PolicyID, policyID, SearchOp.Equal );
					foreach ( ShallowNode sn in list )
					{
						Policy tempPolicy = new Policy( poBox, sn );
						if ( tempPolicy.IsSystemPolicy )
						{
							policy = tempPolicy;
							break;
						}
					}
				}
			}

			return policy;
		}

		/// <summary>
		/// Gets the Policy that is associated with the collection.
		/// </summary>
		/// <param name="policyID">Strong name of the Policy.</param>
		/// <param name="collection">Collection used to lookup the associated Policy.</param>
		/// <returns>A reference to the associated Policy if successful. A null is
		/// returned if the Policy does not exist.</returns>
		public Policy GetPolicy( string policyID, Collection collection )
		{
			Policy policy = null;

			// Search the collection for the specified policy.
			ICSList list = collection.Search( PropertyTags.PolicyID, policyID, SearchOp.Equal );
			foreach ( ShallowNode sn in list )
			{
				Policy tempPolicy = new Policy( collection, sn );
				if ( !tempPolicy.IsSystemPolicy )
				{
					policy = tempPolicy;
					break;
				}
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
					if ( !p.IsSystemPolicy )
					{
						policyHash[ p.StrongName ] = p;
					}
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
						if ( p.IsSystemPolicy )
						{
							policyHash[ p.StrongName ] = p;
						}
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
				if ( !p.IsSystemPolicy )
				{
					policyHash[ p.StrongName ] = p;
				}
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

			// Get the specified domain.
			Domain domain = store.GetDomain( domainID );
			if ( domain != null )
			{
				ICSList tempList = domain.Search( BaseSchema.ObjectType, NodeTypes.PolicyType, SearchOp.Equal );
				foreach ( ShallowNode sn in tempList )
				{
					Policy p = new Policy( domain, sn );
					if ( p.IsSystemPolicy )
					{
						policyList.Add( p );
					}
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

			/// <summary>
			/// Used to compare two values using regular expression syntax.
			/// </summary>
			RegExp,

			/// <summary>
			/// Used to compare two values without regard for case using regular expression syntax.
			/// </summary>
			RegExp_IgnoreCase
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

		/// <summary>
		/// Used by the Operation.RegExp.
		/// </summary>
		private Regex regExpRule;
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

			InitializeSearchPattern( operand, operation );
		}

		/// <summary>
		/// Initializes the object with the specified rule document.
		/// </summary>
		/// <param name="rule">Xml document that contains a rule representation.</param>
		internal Rule( object rule ) :
			this( rule as XmlDocument )
		{
		}

		/// <summary>
		/// Initializes the object with the specified rule document.
		/// </summary>
		/// <param name="ruleDocument">Xml document that contains a rule representation.</param>
		internal Rule( XmlDocument ruleDocument )
		{
			XmlElement element = ruleDocument.DocumentElement;

			operation = ( Operation )Enum.Parse( typeof( Operation ), element.GetAttribute( RuleOperationTag ) );
			result = ( Result )Enum.Parse( typeof( Result ), element.GetAttribute( RuleResultTag ) );

			syntax = ( Syntax )Enum.Parse( typeof( Syntax ), element.GetAttribute( RuleSyntaxTag ) );
			Property p = new Property( String.Empty, syntax, element.InnerText );
			operand = p.Value;

			InitializeSearchPattern( operand, operation );
		}

		/// <summary>
		/// Special constructor used to deserialize a Rule object.
		/// </summary>
		/// <param name="info">The SerializationInfo populated with the data.</param>
		/// <param name="context">The source (see StreamingContext) for this serialization.</param>
		protected Rule( SerializationInfo info, StreamingContext context )
		{
			// Get the string that represents the serialized rule.
			XmlDocument ruleDocument = new XmlDocument();
			ruleDocument.LoadXml( info.GetString( "RuleData" ) );
			XmlElement element = ruleDocument.DocumentElement;

			operation = ( Operation )Enum.Parse( typeof( Operation ), element.GetAttribute( RuleOperationTag ) );
			result = ( Result )Enum.Parse( typeof( Result ), element.GetAttribute( RuleResultTag ) );

			syntax = ( Syntax )Enum.Parse( typeof( Syntax ), element.GetAttribute( RuleSyntaxTag ) );
			Property p = new Property( String.Empty, syntax, element.InnerText );
			operand = p.Value;

			InitializeSearchPattern( operand, operation );
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
				case Operation.RegExp:
				case Operation.RegExp_IgnoreCase:
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
		/// Initializes search pattern for specified operand.
		/// </summary>
		/// <param name="operand">Object that is used to match against the input object when the Apply method is called.
		/// The type of object must be one of the Simias.Syntax types.</param>
		/// <param name="operation">Operation to perform between input and operand.</param>
		private void InitializeSearchPattern( object operand, Operation operation )
		{
			// If the operation is Operation.RegExp create the regular expression now.
			if ( operation == Operation.RegExp )
			{
				regExpRule = new Regex( operand as string );
			}
			else if ( operation == Operation.RegExp_IgnoreCase )
			{
				regExpRule = new Regex( operand as string, RegexOptions.IgnoreCase );
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
					// Only equals, !equals and no operation are supported.
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
				{
					// Regular expression matching and ignore case is not allowed.
					if ( ( operation == Operation.RegExp ) || ( operation == Operation.RegExp_IgnoreCase ) )
					{
						throw new CollectionStoreException( "Invalid operation for rule object type." );
					}
					break;
				}

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

		#region Internal Methods
		/// <summary>
		/// Converts the value of this instance to an XML document.
		/// </summary>
		/// <returns>The value of this instance.</returns>
		internal XmlDocument ToXml()
		{
			// Create an XML document to hold the data.
			XmlDocument document = new XmlDocument();

			// Create the root and only element.
			XmlElement element = document.CreateElement( RuleNameTag );
			element.SetAttribute( RuleOperationTag, Enum.GetName( typeof( Operation ), operation ) );
			element.SetAttribute( RuleResultTag, result.ToString() );
			element.SetAttribute( RuleSyntaxTag, syntax.ToString() );

			// Convert the rule into a property with no name.
			Property p = new Property( String.Empty, operand );
			element.InnerText = p.ValueString;

			document.AppendChild( element );
			return document;
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
			if ( !input.GetType().Equals( operand.GetType() ) )
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
					if ( ( operation == Operation.RegExp ) || ( operation == Operation.RegExp_IgnoreCase ) )
					{
						ruleResult = GetResult( regExpRule.IsMatch( input as string ) ? 0 : 1 );
					}
					else
					{
						ruleResult = GetResult( ( input as string ).CompareTo( operand ) );
					}
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
			return ToXml().InnerXml;
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
		/// Initializes a new instance of the object class.
		/// </summary>
		public PolicyTime( PolicyTime time )
		{
			table = new BitArray( time.table );
		}

		/// <summary>
		/// Initializes a new instance of the object class with the specified time array.
		/// </summary>
		/// <param name="timeArray">Array of bytes that represent time values in a 7 day week, 24 hour period.</param>
		internal PolicyTime( byte[] timeArray )
		{
			table = new BitArray( timeArray );
		}

		/// <summary>
		/// Initializes a new instance of the object class with the specified time string.
		/// </summary>
		/// <param name="timeString">String that represent time values in a 7 day week, 24 hour period.</param>
		internal PolicyTime( string timeString ) :
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
			return IsSet( time.DayOfWeek, time.Hour ) ? Rule.Result.Allow : Rule.Result.Deny;
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

	/// <summary>
	/// Object used to store policy name value pairs.
	/// </summary>
	public class PolicyValue
	{
		#region Class Members
		/// <summary>
		/// The name of the value.
		/// </summary>
		private string name;

		/// <summary>
		/// The value.
		/// </summary>
		private object policyValue;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the name of this value.
		/// </summary>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		/// Gets the value for this object.
		/// </summary>
		public object Value
		{
			get { return policyValue; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object.
		/// </summary>
		/// <param name="name">Name of the value.</param>
		/// <param name="value">Value to store in the object.</param>
		public PolicyValue( string name, object value )
		{
			this.name = name;
			this.policyValue = value;
		}
		#endregion
	}
}
