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
using System.Xml;

using Simias.Client;
using Simias.Storage;
using Simias.Sync;

namespace Simias.POBox
{
	/// <summary>
	/// A POBox object is a specialized collection used to hold messages.
	/// </summary>
	public class POBox : Collection
	{
		#region Class Members

		/// <summary>
		/// The name of the property storing the DirNode name.
		/// </summary>
		public const string POServiceUrlProperty = "POServiceUrl";

		#endregion

		#region Properties
		
		/// <summary>
		/// Gets/sets the post-office service url.
		/// </summary>
		public string POServiceUrl
		{
			get
			{
				string result = null;

				Property p = Properties.GetSingleProperty(POServiceUrlProperty);

				if (p != null)
				{
					result = p.ToString();
				}
				else
				{
					result = this.MasterUrl.ToString() + "/POService.asmx";
				}

				return result;
			}
			set
			{
				Property p = new Property(POServiceUrlProperty, value);
				p.LocalProperty = true;

				Properties.ModifyProperty(p);
			}
		}

		#endregion

		#region Constructors
		/// <summary>
		/// Constructor to create a POBox object from a Node object.
		/// </summary>
		/// <param name="storeObject">Store object that this POBox belongs to.</param>
		/// <param name="node">Node object to construct POBox object from.</param>
		internal POBox(Store storeObject, Node node) :
			base (storeObject, node)
		{
		}

		/// <summary>
		/// Constructor to create a POBox object from a ShallowNode object.
		/// </summary>
		/// <param name="storeObject">The Store object that this POBox belongs to.</param>
		/// <param name="shallowNode">The ShallowNode object to contruct the POBox object from.</param>
		internal POBox(Store storeObject, ShallowNode shallowNode) :
			base (storeObject, shallowNode)
		{
		}

		/// <summary>
		/// Constructor to create an existing POBox object from an Xml document object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="document">Xml document object to construct this object from.</param>
		internal POBox( Store storeObject, XmlDocument document ) :
			base( storeObject, document )
		{
		}

		/// <summary>
		/// Constructor to create a POBox object.
		/// </summary>
		/// <param name="storeObject">The Store object that the POBox will belong to.</param>
		/// <param name="collectionName">The name of the POBox.</param>
		/// <param name="domainName">The name of the domain that the POBox belongs to.</param>
		public POBox(Store storeObject, string collectionName, string domainName) :
			this (storeObject, collectionName, Guid.NewGuid().ToString(), domainName)
		{
		}

		/// <summary>
		/// Constructor to create a POBox object.
		/// </summary>
		/// <param name="storeObject">The Store object that the POBox will belong to.</param>
		/// <param name="collectionName">The name of the POBox.</param>
		/// <param name="collectionID">The identifier of the POBox.</param>
		/// <param name="domainName">The name of the domain that the POBox belongs to.</param>
		internal POBox(Store storeObject, string collectionName, string collectionID, string domainName) :
			base( storeObject, collectionName, collectionID, NodeTypes.POBoxType, domainName )
		{
		}

		#endregion

		#region Private Methods
		
		#endregion

		#region Internal Methods
		/// <summary>
		/// POBox factory method that constructs a POBox object for the specified user in the specified domain.
		/// </summary>
		/// <param name="storeObject">The Store object that the POBox belongs to.</param>
		/// <param name="domainId">The ID of the domain that the POBox belongs to.</param>
		/// <param name="userId">The ID of the user that the POBox belongs to.</param>
		/// <returns></returns>
		public static POBox FindPOBox(Store storeObject, string domainId, string userId)
		{
			POBox poBox = null;

			// Build the name of the POBox.
			string name = "POBox:" + domainId + ":" + userId;

			// Search for the POBox.
			ICSEnumerator listEnum = storeObject.GetCollectionsByName(name).GetEnumerator() as ICSEnumerator;

			// There should only be one value returned...
			if (listEnum.MoveNext())
			{
				ShallowNode shallowNode = (ShallowNode)listEnum.Current;

				if (listEnum.MoveNext())
				{
					// TODO: multiple values were returned ... throw an exception.
				}

				poBox = new POBox(storeObject, shallowNode);
			}

			listEnum.Dispose();
			return poBox;
		}
		#endregion

		#region Public Methods
		
		/// <summary>
		/// POBox factory method that constructs a POBox object for the specified domain ID.
		/// </summary>
		/// <param name="storeObject">The Store object that the POBox belongs to.</param>
		/// <param name="domainId">The ID of the domain that the POBox belongs to.</param>
		/// <returns></returns>
		public static POBox GetPOBox(Store storeObject, string domainId)
		{
			return GetPOBox(storeObject, domainId, storeObject.GetUserIDFromDomainID(domainId));
		}

		/// <summary>
		/// POBox factory method that constructs a POBox object for the specified user in the specified domain.
		/// </summary>
		/// <param name="storeObject">The Store object that the POBox belongs to.</param>
		/// <param name="domainId">The ID of the domain that the POBox belongs to.</param>
		/// <param name="userId">The ID of the user that the POBox belongs to.</param>
		/// <returns></returns>
		public static POBox GetPOBox(Store storeObject, string domainId, string userId)
		{
			POBox poBox = FindPOBox( storeObject, domainId, userId );
			if (poBox == null)
			{
				// If the POBox cannot be found, create it.
				// Build the name of the POBox.
				string name = "POBox:" + domainId + ":" + userId;
				poBox = new POBox(storeObject, name, domainId);
				
				Domain domain = storeObject.GetDomain( domainId );
				Member current = domain.GetMemberByID(userId);

				Member member = new Member(current.Name, current.UserID, Access.Rights.ReadWrite);
				member.IsOwner = true;

				poBox.Commit(new Node[] { poBox, member });
			}

			return poBox;
		}

		/// <summary>
		/// POBox factory method that constructs a POBox object from it's id.
		/// </summary>
		/// <param name="store">The Store object that the POBox belongs to.</param>
		/// <param name="id">The ID of the POBox collection.</param>
		/// <returns>The POBox object.</returns>
		public static POBox GetPOBoxByID(Store store, string id)
		{
			return store.GetCollectionByID(id) as POBox;
		}
		
		/// <summary>
		/// Adds a message to the POBox object.
		/// </summary>
		/// <param name="message">The message to add to the collection.</param>
		public void AddMessage(Message message)
		{
			Commit(message);
		}

		/// <summary>
		/// Adds an array of Message objects to the POBox object.
		/// </summary>
		/// <param name="messageList">An array of Message objects to add to the POBox object.</param>
		public void AddMessage(Message[] messageList)
		{
			Commit(messageList);
		}

		/// <summary>
		/// Get all the Message objects that have the specified name.
		/// </summary>
		/// <param name="name">A string containing the name to search for.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the
		/// Message object(s) that have the specified name.</returns>
		public ICSList GetMessagesByName(string name)
		{
			return this.GetNodesByName(name);		
		}

		/// <summary>
		/// Get all the Message objects that have the specified type.
		/// </summary>
		/// <param name="type">A string containing the type to search for.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the
		/// Message object(s) that have the specified type.</returns>
		public ICSList GetMessagesByMessageType(string type)
		{
			return this.Search(Message.MessageTypeProperty, type, SearchOp.Equal);
		}



		/// <summary>
		/// Locates a Subscription in the POBox by CollectionID
		/// </summary>
		/// <param name="collectionID">The ID of the collection.</param>
		/// <returns>A Subscription object.</returns>
		public Subscription GetSubscriptionByCollectionID(string collectionID)
		{
			ICSList subList = this.Search(
						Subscription.SubscriptionCollectionIDProperty,
						collectionID,
						SearchOp.Equal);

			foreach(ShallowNode sNode in subList)
			{
				return new Subscription(this, sNode);
			}
			return null;
		}

		/// <summary>
		/// Locates a Subscription in the POBox by CollectionID and UserID.
		/// </summary>
		/// <param name="collectionID">The ID of the collection.</param>
		/// <param name="userID">The ID of the user to whom the subscription is addressed.</param>
		/// <returns>A Subscription object.</returns>
		public Subscription GetSubscriptionByCollectionID(string collectionID, string userID)
		{
			ICSList subList = this.Search(
				Subscription.SubscriptionCollectionIDProperty,
				collectionID,
				SearchOp.Equal);

			foreach (ShallowNode sn in subList)
			{
				Subscription sub = new Subscription(this, sn);
				if (sub.ToIdentity.Equals(userID))
				{
					return sub;
				}
			}

			return null;
		}


		/// <summary>
		/// Creates a Subscription object for the specified collection.
		/// </summary>
		/// <param name="collection">The Collection object that will be shared.</param>
		/// <param name="fromMember">The Member that is sharing the collection.</param>
		/// <param name="type"></param>
		/// <returns>A Subscription object.  This object must be added to the POBox using one of the AddMessage() methods.</returns>
		public Subscription CreateSubscription(Collection collection, Member fromMember, string type)
		{
			Subscription subscription = new Subscription(collection.Name + " Subscription", Message.OutboundMessage, fromMember.UserID);

			subscription.FromName = fromMember.Name;
			subscription.FromIdentity = fromMember.UserID;
			subscription.FromPublicKey = fromMember.PublicKey;
			subscription.SubscriptionCollectionName = collection.Name;
			subscription.SubscriptionCollectionID = collection.ID;
			subscription.DomainID = collection.Domain;
			subscription.DomainName = collection.StoreReference.GetDomain(collection.Domain).Name;
			subscription.SubscriptionCollectionType = type;
			subscription.SubscriptionKey = Guid.NewGuid().ToString();

			// TODO: clean this up
			subscription.HasDirNode = (collection != null) ? (collection.GetRootDirectory() != null) : false;

			return subscription;
		}

		#endregion
	}
}
