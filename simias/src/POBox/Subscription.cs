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
using System.IO;

using Simias;
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
		/// The name of the property storing the collection types.
		/// </summary>
		public const string SubscriptionCollectionTypeProperty = "SbColType";

		/// <summary>
		/// The name of the property storing the collection types.
		/// </summary>
		public const string SubscriptionCollectionTypesProperty = "SbColTypes";

		/// <summary>
		/// The name of the property storing the collection master URL.
		/// </summary>
		public const string SubscriptionCollectionURLProperty = "SbColURL";

		/// <summary>
		/// The name of the property storing the post office service url.
		/// </summary>
		public static readonly string POServiceURLProperty = "POSvcURL";

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
		
		#endregion

		#region Constructors
		
		/// <summary>
		/// Constructor for creating a Subscription object from a Node object.
		/// </summary>
		/// <param name="node">The Node object to create the Subscription object from.</param>
		public Subscription(Node node) :
			base (node)
		{
		}

		/// <summary>
		/// Constructor for creating a Subscription object from a SubscriptionInfo object.
		/// </summary>
		/// <param name="subscriptionName">The friendly name of the Subscription.</param>
		/// <param name="subscriptionInfo">The SubscriptionInfo object to create the Subscription object from.</param>
		public Subscription(string subscriptionName, SubscriptionInfo subscriptionInfo) :
			base (subscriptionName, subscriptionInfo.SubscriptionID)
		{
			DomainID = subscriptionInfo.DomainID;
			DomainName = subscriptionInfo.DomainName;
			SubscriptionCollectionID = subscriptionInfo.SubscriptionCollectionID;
			SubscriptionCollectionName = subscriptionInfo.SubscriptionCollectionName;
			SubscriptionCollectionType = subscriptionInfo.SubscriptionCollectionType;
			POServiceURL = subscriptionInfo.POServiceUrl;
			Properties.AddNodeProperty(PropertyTags.Types, typeof(Subscription).Name);
		}

		/// <summary>
		/// Constructor for creating a new Subscription object.
		/// </summary>
		/// <param name="collection">Collection that the ShallowNode belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create the Subscription object from.</param>
		public Subscription(Collection collection, ShallowNode shallowNode) :
			base (collection, shallowNode)
		{
		}

		/// <summary>
		/// Constructor for creating a new Subscription object with a specific ID.
		/// </summary>
		/// <param name="messageName">The friendly name of the Subscription object.</param>
		/// <param name="messageID">The ID of the Subscription object.</param>
		public Subscription(string messageName, string messageID) :
			base (messageName, messageID)
		{
			Properties.AddNodeProperty(PropertyTags.Types, typeof(Subscription).Name);
		}
	
		/// <summary>
		/// Constructor for creating a new Subscription object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="fromIdentity">The identity of the sender.</param>
		public Subscription(string messageName, string messageType, string fromIdentity) :
			base (messageName, messageType, fromIdentity)
		{
			SubscriptionState = SubscriptionStates.Invited;
			Properties.AddNodeProperty(PropertyTags.Types, typeof(Subscription).Name);
		}

		/// <summary>
		/// Constructor for creating a new Subscription object.
		/// </summary>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="fromIdentity">The sender's identity.</param>
		/// <param name="fromAddress">The sender's address.</param>
		public Subscription(string messageName, string messageType, string fromIdentity, string fromAddress) :
			base (messageName, messageType, fromIdentity, fromAddress)
		{
			SubscriptionState = SubscriptionStates.Invited;
			Properties.AddNodeProperty(PropertyTags.Types, typeof(Subscription).Name);
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
			base (messageName, messageType, fromIdentity, fromAddress, toAddress)
		{
			SubscriptionState = SubscriptionStates.Invited;
			Properties.AddNodeProperty(PropertyTags.Types, typeof(Subscription).Name);
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
			base (messageName, messageType, fromIdentity, fromAddress, toAddress, toIdentity)
		{
			SubscriptionState = SubscriptionStates.Invited;
			Properties.AddNodeProperty(PropertyTags.Types, typeof(Subscription).Name);
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
				Property p = Properties.GetSingleProperty(SubscriptionStateProperty);

				return (p != null) ? (SubscriptionStates)p.Value : SubscriptionStates.Unknown;
			}
			set
			{
				Properties.ModifyProperty(SubscriptionStateProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the recipient's public key
		/// </summary>
		public string ToPublicKey
		{
			get
			{
				Property p = Properties.GetSingleProperty(ToPublicKeyProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				Properties.ModifyProperty(ToPublicKeyProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the sender's public key.
		/// </summary>
		public string FromPublicKey
		{
			get
			{
				Property p = Properties.GetSingleProperty(FromPublicKeyProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				Properties.ModifyProperty(FromPublicKeyProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the name of the collection to share.
		/// </summary>
		public string SubscriptionCollectionName
		{
			get
			{
				Property p = Properties.GetSingleProperty(SubscriptionCollectionNameProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				Properties.ModifyProperty(SubscriptionCollectionNameProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the ID of the collection to share.
		/// </summary>
		public string SubscriptionCollectionID
		{
			get
			{
				Property p = Properties.GetSingleProperty(SubscriptionCollectionIDProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				Properties.ModifyProperty(SubscriptionCollectionIDProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the type of the collection to share.
		/// </summary>
		public string SubscriptionCollectionType
		{
			get
			{
				Property p = Properties.GetSingleProperty(SubscriptionCollectionTypeProperty);

				return (p != null) ? p.ToString() : null;
			}
			
			set
			{
				Properties.AddProperty(SubscriptionCollectionTypeProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the types of the collection to share.
		/// </summary>
		public MultiValuedList SubscriptionCollectionTypes
		{
			get
			{
				return Properties.GetProperties(SubscriptionCollectionTypesProperty);
			}
			set
			{
				foreach (Property p in value)
				{
					Properties.AddProperty(SubscriptionCollectionTypesProperty, p);
				}
			}
		}

		/// <summary>
		/// Gets/sets the master URL of the collection to share.
		/// </summary>
		public string SubscriptionCollectionURL
		{
			get
			{
				Property p = Properties.GetSingleProperty(SubscriptionCollectionURLProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				Properties.ModifyProperty(SubscriptionCollectionURLProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the URL of the post office service.
		/// </summary>
		public Uri POServiceURL
		{
			get
			{
				Property p = Properties.GetSingleProperty(POServiceURLProperty);

				return (p != null) ? (Uri)p.Value : null;
			}
			set
			{
				Properties.ModifyProperty(POServiceURLProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the description of the collection to share.
		/// </summary>
		public string CollectionDescription
		{
			get
			{
				Property p = Properties.GetSingleProperty(CollectionDescriptionProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				Properties.ModifyProperty(CollectionDescriptionProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the collection root path on the slave.
		/// </summary>
		public string CollectionRoot
		{
			get
			{
				Property p = Properties.GetSingleProperty(CollectionRootProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				Property property = new Property(CollectionRootProperty, value);
				property.LocalProperty = true;
				Properties.ModifyProperty(property);
			}
		}

		/// <summary>
		/// Gets/sets the ID of the collection's root DirNode.
		/// </summary>
		public string DirNodeID
		{
			get
			{
				Property p = Properties.GetSingleProperty(DirNodeIDProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				Properties.ModifyProperty(DirNodeIDProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the name of the collection's root DirNode.
		/// </summary>
		public string DirNodeName
		{
			get
			{
				Property p = Properties.GetSingleProperty(DirNodeNameProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				Properties.ModifyProperty(DirNodeNameProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the rights that will be granted on the shared collection.
		/// </summary>
		public Access.Rights SubscriptionRights
		{
			get
			{
				Property p = Properties.GetSingleProperty(SubscriptionRightsProperty);

				return (p != null) ? (Access.Rights)p.Value : Access.Rights.Deny;
			}
			set
			{
				Properties.ModifyProperty(SubscriptionRightsProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the disposition of the subscription.
		/// </summary>
		public SubscriptionDispositions SubscriptionDisposition
		{
			get
			{
				Property p = Properties.GetSingleProperty(SubscriptionDispositionProperty);

				return (p != null) ? (SubscriptionDispositions)p.Value : SubscriptionDispositions.Unknown;
			}
			set
			{
				Properties.ModifyProperty(SubscriptionDispositionProperty, value);
			}
		}

		#endregion

		#region Public Methods
		
		/// <summary>
		/// Generates a SubscriptionInfo object from the Subscription object
		/// </summary>
		/// <returns>A SubscriptionInfo object</returns>
		public SubscriptionInfo GenerateInfo(Store store)
		{
			SubscriptionInfo si = new SubscriptionInfo();

			si.DomainID = DomainID;
			si.DomainName = DomainName;
			
			// TODO: where should this be set?
			// TODO: si.POServiceUrl = POServiceURL;
			si.POServiceUrl = PostOffice.DefaultServiceUrl;
			
			si.SubscriptionCollectionID = SubscriptionCollectionID;
			si.SubscriptionCollectionName = SubscriptionCollectionName;
			si.SubscriptionCollectionType = SubscriptionCollectionType;
			si.SubscriptionID = ID;

			// dir node ?
			Collection c = store.GetCollectionByID(SubscriptionCollectionID);

			si.SubscriptionCollectionHasDirNode = 
				(c != null) ? (c.GetRootDirectory() != null) : false;
			
			return si;
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
				this.DirNodeID = details.DirNodeID;
				this.DirNodeName = details.DirNodeName;
				this.SubscriptionCollectionURL = details.CollectionUrl;
			}
		}

		/// <summary>
		/// Create the slave collection (stub for syncing)
		/// </summary>
		public void CreateSlaveCollection(Store store)
		{
			Collection c = new Collection(store, this.SubscriptionCollectionName,
				this.SubscriptionCollectionID, this.DomainID);
			
			// collection type
			// TODO: sc.SetType(this, this.SubscriptionCollectionTypes);
			c.SetType(c, "iFolder");
			
			// sync information
			Property pr = new Property(SyncCollection.RolePropertyName,
				SyncCollectionRoles.Slave);
			pr.LocalProperty = true;
			c.Properties.AddProperty(pr);
			
			Property pu = new Property(SyncCollection.MasterUrlPropertyName,
				new Uri(this.SubscriptionCollectionURL));
			pu.LocalProperty = true;
			c.Properties.AddProperty(pu);

			// commit
			c.IsStub = true;
			c.Commit();

			// check for a dir node
			if (((this.DirNodeID != null) && (this.DirNodeID.Length > 0))
				&& (this.DirNodeName != null) && (this.DirNodeName.Length > 0)
				&& (this.CollectionRoot != null) && (this.CollectionRoot.Length > 0))
			{
				string path = Path.Combine(this.CollectionRoot, this.DirNodeName);

				DirNode dn = new DirNode(c, path, this.DirNodeID);

				if (!Directory.Exists(path)) Directory.CreateDirectory(path);

				c.Impersonate(new Member("Root", c.ID, Access.Rights.Admin));
				c.Commit(dn);
			}
		}

		/// <summary>
		/// Accept the subscription on the master side
		/// </summary>
		public void Accept(Store store, Access.Rights rights)
		{
			Collection c = store.GetCollectionByID(this.SubscriptionCollectionID);

			// check collection
			if (c == null)
				throw new ApplicationException("Collection does not exist.");

			// member
			Member member = new Member(this.FromName, this.FromIdentity, rights);

			// commit
			c.Commit(member);

			// state update
			this.SubscriptionState = SubscriptionStates.Responded;
			this.SubscriptionDisposition = SubscriptionDispositions.Accepted;
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
