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
using Simias.Storage;

namespace Simias.POBox
{
	/// <summary>
	/// A POBox object is a specialized collection used to hold messages.
	/// </summary>
	public class POBox : Collection
	{
		#region Class Members
		#endregion

		#region Properties
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
		/// Constructor to create a POBox object.
		/// </summary>
		/// <param name="storeObject">The Store object that the POBox will belong to.</param>
		/// <param name="collectionName">The name of the POBox.</param>
		/// <param name="domainName">The name of the domain that the POBox belongs to.</param>
		internal POBox(Store storeObject, string collectionName, string domainName) :
			base (storeObject, collectionName, domainName)
		{
			SetType(this, typeof(POBox).Name);
		}
		#endregion

		#region Private Methods
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
			POBox poBox = null;

			// Build the name of the POBox.
			string name = domainId + ":" + userId;

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

			// If the POBox cannot be found, create it.
			if (poBox == null)
			{
				poBox = new POBox(storeObject, name, domainId);
				poBox.Commit();
			}

			return poBox;
		}

		/// <summary>
		/// POBox factory method that constructs a POBox object from it's id.
		/// </summary>
		/// <param name="storeObject">The Store object that the POBox belongs to.</param>
		/// <param name="domainId">The ID of the domain that the POBox belongs to.</param>
		/// <param name="userId">The ID of the user that the POBox belongs to.</param>
		/// <returns></returns>
		public static POBox GetPOBoxByID(Store store, string id)
		{
			POBox poBox = null;
			Collection collection = store.GetCollectionByID(id);
	
			if (collection != null)
			{
				poBox = new POBox(store, collection);
			}

			return poBox;
		}
		
		/// <summary>
		/// Adds a message to the POBox object.
		/// </summary>
		/// <param name="message">The message to add to the collection.</param>
		public void AddMessage(Message message)
		{
			this.SetType(message, typeof(Message).Name);
			Commit(message);
		}

		/// <summary>
		/// Adds an array of Message objects to the POBox object.
		/// </summary>
		/// <param name="messageList">An array of Message objects to add to the POBox object.</param>
		public void AddMessage(Message[] messageList)
		{
			foreach (Message message in messageList)
			{
				this.SetType(message, typeof(Message).Name);
			}

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
		/// Creates a Subscription object for the specified collection.
		/// </summary>
		/// <param name="collection">The Collection object that will be shared.</param>
		/// <param name="fromMember">The Member that is sharing the collection.</param>
		/// <returns>A Subscription object.  This object must be added to the POBox using one of the AddMessage() methods.</returns>
		public Subscription CreateSubscription(Collection collection, Member fromMember)
		{
			Subscription subscription = new Subscription("Subscription Message", Message.OutboundMessage, fromMember.UserID);

			subscription.FromName = fromMember.Name;
			subscription.SubscriptionCollectionId = collection.ID;
			subscription.SubscriptionCollectionDomainId = collection.Domain;
			subscription.SubscriptionCollectionDomainName = collection.StoreReference.GetDomain(collection.Domain).Name;

			// TODO: fromAddress, toAddress, toName

			return subscription;
		}
		#endregion
	}
}
