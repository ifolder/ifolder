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
 *  Author: Rob
 *
 ***********************************************************************/

using System;

using Simias.Storage;

namespace Simias.POBox
{
	/// <summary>
	/// Post Office
	/// </summary>
	public class PostOffice : MarshalByRefObject
	{
		private Configuration config;
		private Store store;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="config">Simias configuration object</param>
		public PostOffice(Configuration config)
		{
			this.config = config;
			store = new Store(config);
		}

		/// <summary>
		/// Post a message
		/// </summary>
		/// <param name="message">A message object</param>
		/// <returns>true if the message was posted</returns>
		public bool Post(Message message)
		{
			bool result = false;
			
			// open the post office box
			POBox box = POBox.GetPOBox(store, message.DomainID, message.ToIdentity);

			// check the post office box
			if (box == null)
				throw new ApplicationException("PO Box not found.");

			// check that the message has already not been posted
			if (box.GetNodeByID(message.ID) != null)
				throw new ApplicationException("Subscription already exists.");

			// subscription
			if (message.GetType().Name == typeof(Subscription).Name)
			{
				// temporary, in memory only, subscription object
				Subscription temp = new Subscription(message);

				// does the collection exist
				if (store.GetCollectionByID(temp.SubscriptionCollectionID) == null)
					throw new ApplicationException("The subscription collection does not exist.");

				// create a new subscription object with some of the information from temp
				Subscription subscription = new Subscription(temp.Name, temp.ID);
				subscription.SubscriptionState =  SubscriptionStates.Pending;
				subscription.FromPublicKey = temp.FromPublicKey;
				subscription.FromName = temp.FromAddress;
				subscription.FromAddress = temp.FromAddress;
				subscription.FromIdentity = temp.FromIdentity;
				subscription.SubscriptionCollectionID = temp.SubscriptionCollectionID;
				
				// commit
				box.Commit(subscription);

				// done
				result = true;
			}
			else
			{
				// ignore for now
			}

			return result;
		}

		public SubscriptionDetails GetSubscriptionDetails(string domain, string identity, string collection)
		{
			return null;
		}

		public void AckSubscription(string domain, string identity, string message)
		{
			// open the post office box
			POBox box = POBox.GetPOBox(store, domain, identity);

			// check the post office box
			if (box == null)
				throw new ApplicationException("PO Box not found.");

			// check that the message has already not been posted
			Node node = box.GetNodeByID(message);

			if (node == null)
				throw new ApplicationException("Subscription does not exist.");

			// get the subscription object
			Subscription subscription = new Subscription(node);
			subscription.SubscriptionState = SubscriptionStates.Acknowledged;
			box.Commit();
		}

		public SubscriptionStatus GetSubscriptionStatus(string domain, string identity, string message)
		{
			SubscriptionStatus status = null;

			// open the post office box
			POBox box = POBox.GetPOBox(store, domain, identity);

			// check the post office box
			if (box == null)
				throw new ApplicationException("PO Box not found.");

			// check that the message has already not been posted
			Node node = box.GetNodeByID(message);

			if (node != null)
				throw new ApplicationException("Subscription already exists.");

			// get the status object
			Subscription subscription = new Subscription(node);
			status = subscription.GenerateStatus();

			return status;
		}
	}
}
