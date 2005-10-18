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
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Xml;

using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.Sync;

namespace Simias.POBox
{
	/// <summary>
	/// Subscription states.
	/// </summary>
	public enum SubscriptionStates
	{
		/// <summary>
		/// The Subscription has been created but not sent.
		/// </summary>
		Invited,

		/// <summary>
		/// The Subscription has been sent.
		/// </summary>
		Posted,

		/// <summary>
		/// The Subscription has been received.
		/// </summary>
		Received,

		/// <summary>
		/// The Subscription has been replied to.
		/// </summary>
		Replied,

		/// <summary>
		/// The Subscription reply has been delivered.
		/// </summary>
		Delivered,

		/// <summary>
		/// The Subscription is waiting to be accepted/declined by the owner.
		/// </summary>
		Pending,

		/// <summary>
		/// The Subscription has been accepted/declined.
		/// </summary>
		Responded,

		/// <summary>
		/// The Subscription acceptance/denial has been acknowledged.
		/// </summary>
		Acknowledged,

		/// <summary>
		/// The Subscription is ready and can be used to start syncing.
		/// </summary>
		Ready,

		/// <summary>
		/// The subscription state is unknown.
		/// </summary>
		Unknown
	};

	/// <summary>
	/// The disposition of a subscription
	/// </summary>
	public enum SubscriptionDispositions
	{
		/// <summary>
		/// The subscription was accepted.
		/// </summary>
		Accepted,

		/// <summary>
		/// The subscription was declined.
		/// </summary>
		Declined,

		/// <summary>
		/// The subscription was rejected.
		/// </summary>
		Rejected,

		/// <summary>
		/// The disposition is unknown.
		/// </summary>
		Unknown
	};

	/// <summary>
	/// An Subscription object is a specialized message used for inviting someone to a team space.
	/// </summary>
	[Serializable]
	public class Subscription : Message
	{
		#region Class Members
		
		/// <summary>
		/// default root path for collections
		/// </summary>
		public static string DefaultRootPath = Path.Combine(
			Environment.GetFolderPath(
			Environment.SpecialFolder.Personal),
			"My Collections");

		/// <summary>
		/// The name of the property storing the SubscriptionState.
		/// </summary>
		public const string SubscriptionStateProperty = "SbState";

		/// <summary>
		/// The name of the property storing the recipient's public key.
		/// </summary>
		public const string ToPublicKeyProperty = "ToPKey";

		/// <summary>
		/// The name of the property storing the sender's public key.
		/// </summary>
		public const string FromPublicKeyProperty = "FromPKey";

		/// <summary>
		/// The name of the property storing the collection name.
		/// </summary>
		public const string SubscriptionCollectionNameProperty = "SbColName";

		/// <summary>
		/// The name of the property storing the collection ID.
		/// </summary>
		public static readonly string SubscriptionCollectionIDProperty = "SbColID";

		/// <summary>
		/// The name of the property storing the shared collection type.
		/// </summary>
		public const string SubscriptionCollectionTypeProperty = "SbColType";

		/// <summary>
		/// The name of the property storing the value that tells if the collection has a DirNode.
		/// </summary>
		public static readonly string SubscriptionCollectionHasDirNodeProperty = "HasDirNode";

		/// <summary>
		/// The name of the property storing the collection description.
		/// </summary>
		public const string CollectionDescriptionProperty = "ColDesc";

		/// <summary>
		/// The name of the property storing the root path of the collection (on the slave).
		/// </summary>
		public static readonly string CollectionRootProperty = "ColRoot";

		/// <summary>
		/// The name of the property storing the DirNode ID.
		/// </summary>
		public const string DirNodeIDProperty = "DirNodeID";

		/// <summary>
		/// The name of the property storing the DirNode name.
		/// </summary>
		public const string DirNodeNameProperty = "DirNodeName";

		/// <summary>
		/// The name of the property storing the rights requested/granted.
		/// </summary>
		public const string SubscriptionRightsProperty = "SbRights";

		/// <summary>
		/// The name of the property storing the status of the subscription (accepted, declined, etc.).
		/// </summary>
		public static readonly string SubscriptionDispositionProperty = "SbDisposition";
		
		/// <summary>
		/// The name of the property storing the subscription key.
		/// </summary>
		public static readonly string SubscriptionKeyProperty = "SbKey";

		/// <summary>
		/// The To member's node ID in the collection.
		/// </summary>
		public static readonly string ToMemberNodeIDProperty = "SbMemberNode";
		
		#endregion

		#region Constructors
		
		/// <summary>
		/// Constructor for creating a Subscription object from a SubscriptionInfo object.
		/// </summary>
		/// <param name="subscriptionName">The friendly name of the Subscription.</param>
		/// <param name="subscriptionInfo">The SubscriptionInfo object to create the Subscription object from.</param>
		public Subscription(string subscriptionName, SubscriptionInfo subscriptionInfo) :
			this (subscriptionName, subscriptionInfo.SubscriptionID)
		{
			DomainID = subscriptionInfo.DomainID;
			DomainName = subscriptionInfo.DomainName;
			SubscriptionCollectionID = subscriptionInfo.SubscriptionCollectionID;
			SubscriptionCollectionName = subscriptionInfo.SubscriptionCollectionName;
			SubscriptionCollectionType = subscriptionInfo.SubscriptionCollectionType;
			HasDirNode = subscriptionInfo.SubscriptionCollectionHasDirNode;
		}

		/// <summary>
		/// Constructor for creating a new Subscription object with a specific ID.
		/// </summary>
		/// <param name="messageName">The friendly name of the Subscription object.</param>
		/// <param name="messageID">The ID of the Subscription object.</param>
		public Subscription(string messageName, string messageID) :
			base (messageName, NodeTypes.SubscriptionType, messageID)
		{
		}
	
		/// <summary>
		/// Constructor for creating a new Subscription object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="fromIdentity">The identity of the sender.</param>
		public Subscription(string messageName, string messageType, string fromIdentity) :
			this (messageName, messageType, fromIdentity, null, null, null)
		{
		}

		/// <summary>
		/// Constructor for creating a new Subscription object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="fromIdentity">The sender's identity.</param>
		/// <param name="fromAddress">The sender's address.</param>
		public Subscription(string messageName, string messageType, string fromIdentity, string fromAddress) :
			this (messageName, messageType, fromIdentity, fromAddress, null, null)
		{
		}

		/// <summary>
		/// Constructor for creating a new Subscription object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="fromIdentity">The sender's identity.</param>
		/// <param name="fromAddress">The sender's address.</param>
		/// <param name="toAddress">The recipient's address.</param>
		public Subscription(string messageName, string messageType, string fromIdentity, string fromAddress, string toAddress) :
			this (messageName, messageType, fromIdentity, fromAddress, toAddress, null)
		{
		}

		/// <summary>
		/// Constructor for creating a new Subscription object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="fromIdentity">The sender's identity.</param>
		/// <param name="fromAddress">The sender's address.</param>
		/// <param name="toAddress">The recipient's address.</param>
		/// <param name="toIdentity">The recipient's identity.</param>
		public Subscription(string messageName, string messageType, string fromIdentity, string fromAddress, string toAddress, string toIdentity) :
			base (messageName, NodeTypes.SubscriptionType, messageType, fromIdentity, fromAddress, toAddress, toIdentity)
		{
			SubscriptionState = SubscriptionStates.Invited;
		}

		/// <summary>
		/// Clone the message with a new ID.
		/// </summary>
		/// <param name="ID">The new node ID.</param>
		/// <param name="message">The message to clone.</param>
		public Subscription(string ID, Message message) : 
			base(ID, message)
		{
		}

		/// <summary>
		/// Constructor for creating an existing Subscription object from a Node object.
		/// </summary>
		/// <param name="node">The Node object to create the Subscription object from.</param>
		public Subscription(Node node) :
			base (node)
		{
		}

		/// <summary>
		/// Constructor for creating an existing Subscription object.
		/// </summary>
		/// <param name="collection">Collection that the ShallowNode belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create the Subscription object from.</param>
		public Subscription(Collection collection, ShallowNode shallowNode) :
			base (collection, shallowNode)
		{
		}

		/// <summary>
		/// Constructor for creating an existing Subscription object from an Xml document.
		/// </summary>
		/// <param name="document">Xml document object to create Subscription object from.</param>
		internal Subscription(XmlDocument document) :
			base (document)
		{
		}

		#endregion

		#region Properties
		
		/// <summary>
		/// Gets/sets the state of the Subscription object.
		/// </summary>
		public SubscriptionStates SubscriptionState
		{
			get
			{
				Property p = properties.FindSingleValue(SubscriptionStateProperty);

				return (p != null) ? (SubscriptionStates)p.Value : SubscriptionStates.Unknown;
			}
			set
			{
				properties.ModifyNodeProperty(SubscriptionStateProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the recipient's public key
		/// </summary>
		public RSACryptoServiceProvider ToPublicKey
		{
			get
			{
				RSACryptoServiceProvider pk = null;

				Property p = properties.GetSingleProperty( ToPublicKeyProperty );
				if ( p != null )
				{
					pk = Identity.DummyCsp;
					pk.FromXmlString( p.ToString() );
				}

				return pk;
			}
			set
			{
				if ( value != null )
				{
					properties.ModifyNodeProperty( ToPublicKeyProperty, value.ToXmlString( false ) );
				}
			}
		}

		/// <summary>
		/// Gets/sets the sender's public key.
		/// </summary>
		public RSACryptoServiceProvider FromPublicKey
		{
			get
			{
				RSACryptoServiceProvider pk = null;

				Property p = properties.GetSingleProperty( FromPublicKeyProperty );
				if ( p != null )
				{
					pk = Identity.DummyCsp;
					pk.FromXmlString( p.ToString() );
				}

				return pk;
			}
			set
			{
				if ( value != null )
				{
					properties.ModifyNodeProperty( FromPublicKeyProperty, value.ToXmlString( false ) );
				}
			}
		}

		/// <summary>
		/// Gets/sets the name of the collection to share.
		/// </summary>
		public string SubscriptionCollectionName
		{
			get
			{
				Property p = properties.FindSingleValue(SubscriptionCollectionNameProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(SubscriptionCollectionNameProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the ID of the collection to share.
		/// </summary>
		public string SubscriptionCollectionID
		{
			get
			{
				Property p = properties.FindSingleValue(SubscriptionCollectionIDProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(SubscriptionCollectionIDProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the type of the collection to share.
		/// </summary>
		public string SubscriptionCollectionType
		{
			get
			{
				Property p = properties.FindSingleValue(SubscriptionCollectionTypeProperty);

				return (p != null) ? p.ToString() : null;
			}
			
			set
			{
				Properties.AddProperty(SubscriptionCollectionTypeProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the description of the collection to share.
		/// </summary>
		public string CollectionDescription
		{
			get
			{
				Property p = properties.FindSingleValue(CollectionDescriptionProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(CollectionDescriptionProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the collection root path on the slave.
		/// </summary>
		public string CollectionRoot
		{
			get
			{
				Property p = properties.FindSingleValue(CollectionRootProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				Property property = new Property(CollectionRootProperty, value);
				property.LocalProperty = true;
				properties.ModifyNodeProperty(property);
			}
		}

		/// <summary>
		/// Gets/sets the ID of the collection's root DirNode.
		/// </summary>
		public string DirNodeID
		{
			get
			{
				Property p = properties.FindSingleValue(DirNodeIDProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(DirNodeIDProperty, value);
			}
		}

		/// <summary>
		/// Gets a value indicating if the collection contains a DirNode.
		/// </summary>
		public bool HasDirNode
		{
			get
			{
				Property p = properties.FindSingleValue(SubscriptionCollectionHasDirNodeProperty);

				return (p != null) ? (bool)p.Value : false;
			}
			set
			{
				properties.ModifyNodeProperty(SubscriptionCollectionHasDirNodeProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the name of the collection's root DirNode.
		/// </summary>
		public string DirNodeName
		{
			get
			{
				Property p = properties.FindSingleValue(DirNodeNameProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(DirNodeNameProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the rights that will be granted on the shared collection.
		/// </summary>
		public Access.Rights SubscriptionRights
		{
			get
			{
				Property p = properties.FindSingleValue(SubscriptionRightsProperty);

				return (p != null) ? (Access.Rights)p.Value : Access.Rights.Deny;
			}
			set
			{
				properties.ModifyNodeProperty(SubscriptionRightsProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the disposition of the subscription.
		/// </summary>
		public SubscriptionDispositions SubscriptionDisposition
		{
			get
			{
				Property p = properties.FindSingleValue(SubscriptionDispositionProperty);

				return (p != null) ? (SubscriptionDispositions)p.Value : SubscriptionDispositions.Unknown;
			}
			set
			{
				properties.ModifyNodeProperty(SubscriptionDispositionProperty, value);
			}
		}


		/// <summary>
		/// Gets/sets the subscription key.
		/// </summary>
		public string SubscriptionKey
		{
			get
			{
				Property p = properties.FindSingleValue(SubscriptionKeyProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(SubscriptionKeyProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the To user's NODE ID.
		/// </summary>
		public string ToMemberNodeID
		{
			get
			{
				Property p = properties.FindSingleValue(ToMemberNodeIDProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(ToMemberNodeIDProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the originator of this subscription.
		/// </summary>
		public string Originator
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.Originator);
				return (p != null) ? p.ToString() : null;
			}

			set
			{
				Property p = new Property( PropertyTags.Originator, value );
				p.LocalProperty = true;
				properties.ModifyNodeProperty( p );
			}
		}

		#endregion

		#region Public Methods
/*	DEADCODE - Used by the old invitation wizard and GtkAddrBook.	
		/// <summary>
		/// Gets or creates a Subscription object based on a SubscriptionInfo file.
		/// </summary>
		/// <param name="store">The store to use when creating/finding the Subscription object.</param>
		/// <param name="subscriptionInfoFileName">The file used to create/find the Subscription object.</param>
		/// <returns>A Subscription object constructed from the SubscriptionInfo file.</returns>
		public static Subscription GetSubscriptionFromSubscriptionInfo(Store store, string subscriptionInfoFileName)
		{
			SubscriptionInfo subscriptionInfo = new SubscriptionInfo(subscriptionInfoFileName);

			return GetSubscriptionFromSubscriptionInfo(store, subscriptionInfo);
		}

		/// <summary>
		/// Gets or creates a Subscription object based on a SubscriptionInfo object.
		/// </summary>
		/// <param name="store">The store to use when creating/finding the Subscription object.</param>
		/// <param name="subscriptionInfo">The SubscriptionInfo object used to create/find the Subscription object.</param>
		/// <returns>A Subscription object constructed from the SubscriptionInfo object.</returns>
		public static Subscription GetSubscriptionFromSubscriptionInfo(Store store, SubscriptionInfo subscriptionInfo)
		{
			Subscription subscription;

			// check for existing subscription object in the POBox
			POBox poBox = POBox.GetPOBox(store, subscriptionInfo.DomainID);
			
			ICSList list = poBox.Search(Message.MessageIDProperty, subscriptionInfo.SubscriptionID, SearchOp.Equal);
			ICSEnumerator e = list.GetEnumerator() as ICSEnumerator;
			if (e.MoveNext())
			{
				// new up the subscription from the existing node.
				subscription = new Subscription(poBox, e.Current as ShallowNode);
				e.Dispose();
			}
			else
			{
				// Create a new subscription object.
				subscription = new Subscription(subscriptionInfo.SubscriptionCollectionName + " Subscription", subscriptionInfo);

				// Set the state to received.
				subscription.SubscriptionState = SubscriptionStates.Received;

				// Add the subscription to the POBox.
				poBox.AddMessage(subscription);
			}


			return subscription;
		}
*/
		/// <summary>
		/// Generates a SubscriptionInfo object from the Subscription object
		/// </summary>
		/// <returns>A SubscriptionInfo object</returns>
		public SubscriptionInfo GenerateInfo(Store store)
		{
			SubscriptionInfo si = new SubscriptionInfo();

			si.DomainID = DomainID;
			si.DomainName = DomainName;
			
			si.SubscriptionCollectionID = SubscriptionCollectionID;
			si.SubscriptionCollectionName = SubscriptionCollectionName;
			si.SubscriptionCollectionType = SubscriptionCollectionType;
			si.SubscriptionID = MessageID;

			// dir node ?
			Collection c = store.GetCollectionByID(SubscriptionCollectionID);

			si.SubscriptionCollectionHasDirNode = 
				(c != null) ? (c.GetRootDirectory() != null) : false;
			
			return si;
		}

		/// <summary>
		/// Generates a SubscriptionInfo object from the Subscription object
		/// </summary>
		/// <returns>A SubscriptionInfo object</returns>
		public SubscriptionMsg GenerateSubscriptionMessage()
		{

			SubscriptionMsg subMsg = new SubscriptionMsg();
			subMsg.DomainID = this.DomainID;
			subMsg.FromID = this.FromIdentity;
			subMsg.FromName = this.FromName;
			subMsg.ToID = this.ToIdentity;
			subMsg.SharedCollectionID = this.SubscriptionCollectionID;
			subMsg.SharedCollectionType = this.SubscriptionCollectionType;
			subMsg.SharedCollectionName = this.SubscriptionCollectionName;
			subMsg.AccessRights = (int) this.SubscriptionRights;
			subMsg.SubscriptionID = this.MessageID;

			Collection collection = Store.GetStore().GetCollectionByID( subMsg.SharedCollectionID );
			if ( collection != null )
			{
				DirNode dirNode = collection.GetRootDirectory();
				if( dirNode != null )
				{
					subMsg.DirNodeID = dirNode.ID;
					subMsg.DirNodeName = dirNode.Name;
				}
			}

			return subMsg;
		}
		
		/// <summary>
		/// Generates a SubscriptionStatus object from the Subscription object
		/// </summary>
		/// <returns>A SubscriptionStatus object</returns>
		public SubscriptionStatus GenerateStatus()
		{
			SubscriptionStatus status = new SubscriptionStatus();

			status.State = this.SubscriptionState;
			status.Disposition = this.SubscriptionDisposition;

			return status;
		}

		/// <summary>
		/// Add the details to the subscription
		/// </summary>
		/// <param name="details">The details object</param>
		public void AddDetails(SubscriptionDetails details)
		{
			if (details != null)
			{
				if ((details.DirNodeID != null) && (details.DirNodeID.Length > 0))
				{
					this.DirNodeID = details.DirNodeID;
				}

				if ((details.DirNodeName != null) && (details.DirNodeName.Length > 0))
				{
					this.DirNodeName = details.DirNodeName;
				}
			}
		}

		/// <summary>
		/// Create the slave collection (stub for syncing)
		/// </summary>
		public void CreateSlave(Store store)
		{
			ArrayList commitList = new ArrayList();

			Collection c = new Collection(store, this.SubscriptionCollectionName,
				this.SubscriptionCollectionID, this.DomainID);

			commitList.Add(c);
			
			// collection type
			// TODO: sc.SetType(this, this.SubscriptionCollectionTypes);
			c.SetType(c, this.SubscriptionCollectionType);
			
			// Create the member as well
			if (this.ToMemberNodeID != null && this.ToMemberNodeID != "")
			{
				Member member = new Member(this.ToName, this.ToMemberNodeID, this.ToIdentity, this.SubscriptionRights, null);
				member.IsOwner = true;
				member.Proxy = true;
				commitList.Add(member);
			}

			// check for a dir node
			if (((this.DirNodeID != null) && (this.DirNodeID.Length > 0))
				&& (this.DirNodeName != null) && (this.DirNodeName.Length > 0)
				&& (this.CollectionRoot != null) && (this.CollectionRoot.Length > 0))
			{
				string path = Path.Combine(this.CollectionRoot, this.DirNodeName);
				DirNode dn = new DirNode(c, path, this.DirNodeID);
				if (!Directory.Exists(path)) Directory.CreateDirectory(path);

				dn.Proxy = true;
				commitList.Add(dn);
			}

			c.Proxy = true;
			c.Commit((Node[]) commitList.ToArray(typeof(Node)));
		}

		/// <summary>
		/// Accept the subscription on the slave side.
		/// </summary>
		/// <param name="store">The store that the POBox belongs to.</param>
		/// <param name="disposition">The disposition to set on the subscription.</param>
		public void Accept(Store store, SubscriptionDispositions disposition)
		{
			Collection c = store.GetCollectionByID(this.Properties.GetSingleProperty(BaseSchema.CollectionId).ToString());

			SubscriptionState = SubscriptionStates.Replied;
			SubscriptionDisposition = disposition;
			Member member = c.GetCurrentMember();
			ToName = member.Name;
			ToIdentity = member.UserID;
			ToPublicKey = member.PublicKey;
			//FromName = member.Name;
			//FromIdentity = member.UserID;
		}

		/// <summary>
		/// Accept the subscription on the master side
		/// </summary>
		public Member Accept(Store store, Access.Rights rights)
		{
			Collection c = store.GetCollectionByID(this.SubscriptionCollectionID);

			// check collection
			if (c == null)
				throw new DoesNotExistException("Collection does not exist.");

			// member
			Member member = new Member(this.ToName, this.ToIdentity, rights, this.ToPublicKey);

			// commit
			c.Commit(member);

			// state update
			this.SubscriptionState = SubscriptionStates.Responded;
			this.SubscriptionDisposition = SubscriptionDispositions.Accepted;
			this.ToMemberNodeID = member.ID;

			return member;
		}

		/// <summary>
		/// Decline the subscription on the master side
		/// </summary>
		public void Decline()
		{
			// state update
			this.SubscriptionState = SubscriptionStates.Responded;
			this.SubscriptionDisposition = SubscriptionDispositions.Declined;
		}

		#endregion
	}
}
