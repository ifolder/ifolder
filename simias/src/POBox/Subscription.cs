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

using Simias.Storage;

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
		Acknowledged
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
		Rejected
	};

	/// <summary>
	/// An Subscription object is a specialized message used for inviting someone to a team space.
	/// </summary>
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
			POServiceURL = subscriptionInfo.POServiceUrl;
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
				return (SubscriptionStates)Properties.GetSingleProperty(SubscriptionStateProperty).Value;
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
				return (string)Properties.GetSingleProperty(ToPublicKeyProperty).Value;
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
				return (string)Properties.GetSingleProperty(FromPublicKeyProperty).Value;
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
				return (string)Properties.GetSingleProperty(SubscriptionCollectionNameProperty).Value;
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
				return (string)Properties.GetSingleProperty(SubscriptionCollectionIDProperty).Value;
			}
			set
			{
				Properties.ModifyProperty(SubscriptionCollectionIDProperty, value);
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
				return (string)Properties.GetSingleProperty(SubscriptionCollectionURLProperty).Value;
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
				return (Uri)Properties.GetSingleProperty(POServiceURLProperty).Value;
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
				return (string)Properties.GetSingleProperty(CollectionDescriptionProperty).Value;
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
				return (string)Properties.GetSingleProperty(CollectionRootProperty).Value;
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
				return (string)Properties.GetSingleProperty(DirNodeIDProperty).Value;
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
				return (string)Properties.GetSingleProperty(DirNodeNameProperty).Value;
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
				return (Access.Rights)Properties.GetSingleProperty(SubscriptionRightsProperty).Value;
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
				return (SubscriptionDispositions)Properties.GetSingleProperty(SubscriptionDispositionProperty).Value;
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
		public SubscriptionInfo GenerateInfo()
		{
			SubscriptionInfo si = new SubscriptionInfo();

			si.DomainID = DomainID;
			si.DomainName = DomainName;
			si.POServiceUrl = POServiceURL;
			si.SubscriptionCollectionID = SubscriptionCollectionID;
			si.SubscriptionID = ID;

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

		#endregion
	}
}
