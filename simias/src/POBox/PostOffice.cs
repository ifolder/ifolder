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

using Simias;
using Simias.Storage;
using Simias.Sync;

namespace Simias.POBox
{
	/// <summary>
	/// Post Office
	/// </summary>
	public class PostOffice : MarshalByRefObject
	{
		public static readonly string EndPoint = "PostOffice.rem";

		/// <summary>
		/// The suggested service url for the current machine.
		/// </summary>
		public static readonly Uri DefaultServiceUrl = (new UriBuilder("http",
			MyDns.GetHostName(), 6446, EndPoint)).Uri;

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
		/// Get a lifetime service object to control the lifetime policy.
		/// </summary>
		/// <returns>An ILease object used to control the lifetime policy.</returns>
		public override object InitializeLifetimeService()
		{
			// infinite lease time
			return null;
		}

		/// <summary>
		/// Post a message
		/// </summary>
		/// <param name="message">A message object</param>
		/// <returns>true if the message was posted</returns>
		public bool Post(string user, Message message)
		{
			bool result = false;
			bool workgroup = (message.DomainID == Simias.Storage.Domain.WorkGroupDomainID);

			string userID = System.Threading.Thread.CurrentPrincipal.Identity.Name;
			
			if ((userID == null) || (userID.Length == 0))
			{
				// Kludge: for now trust the client.  this need to be removed before shipping.
				userID = user;
			}

			POBox box = null;
			
			// temporary, in memory only, subscription object
			Subscription temp = new Subscription(message);
				
			// new subscription
			Subscription subscription = null;

			// create a new subscription object with some of the information from temp
			switch (temp.SubscriptionState)
			{
				case SubscriptionStates.Received:
					// Make sure the from field matches the authenticated userid.
					if (!workgroup && userID == temp.FromIdentity)
					{
						box = POBox.GetPOBox(store, message.DomainID, temp.ToIdentity);
						if (box != null)
						{
							subscription = new Subscription(Guid.NewGuid().ToString(), message);
							subscription.SubscriptionState =  SubscriptionStates.Received;
							result = true;
						}
					}
					break;
				case SubscriptionStates.Pending:
					if (workgroup)
					{
                        box = POBox.GetPOBox(store, message.DomainID);
					}
					else
					{
						box = POBox.GetPOBox(store, message.DomainID, temp.FromIdentity);
					}
					if (box != null)
					{
						ICSList list = box.Search(Message.MessageIDProperty, message.MessageID, SearchOp.Equal);
			
						ICSEnumerator e = (ICSEnumerator)list.GetEnumerator();
						ShallowNode sn = null;

						if (e.MoveNext())
						{
							sn = (ShallowNode) e.Current;
						}

						if (sn != null)
						{
							subscription = new Subscription(box, sn);
							string ToID;
							if (workgroup)
							{
								ToID = temp.ToIdentity;
							}
							else
							{
								ToID = subscription.ToIdentity;
							}
					
							// Make sure the to field matches the authenticated user.
							if (userID == ToID /* TODO: Also check world for a published collection.*/)
							{
								subscription.SubscriptionState =  SubscriptionStates.Pending;
								subscription.ToName = temp.ToName;
								subscription.ToIdentity = temp.ToIdentity;
								if (workgroup)
								{
									subscription.ToPublicKey = temp.ToPublicKey;
								}
								result = true;
							}
						}
					}
					break;
			}
				
			// commit
			if (result && box != null)
			{
				box.Commit(subscription);
			}
			
			return result;
		}

		public void AckSubscription(string domain, string identity, string message)
		{
			// open the post office box
			// TODO: POBox box = POBox.GetPOBox(store, domain, identity);
			POBox box = POBox.GetPOBox(store, domain);

			// check the post office box
			if (box == null)
				throw new ApplicationException("PO Box not found.");

			// check that the message has already not been posted
			ICSList list = box.Search(Message.MessageIDProperty, message, SearchOp.Equal);
			
			ICSEnumerator e = (ICSEnumerator)list.GetEnumerator();
			ShallowNode sn = null;

			if (e.MoveNext())
			{
				sn = (ShallowNode) e.Current;
			}

			if (sn == null)
				throw new ApplicationException("Subscription does not exist.");

			// get the subscription object
			Subscription subscription = new Subscription(box, sn);
			subscription.SubscriptionState = SubscriptionStates.Acknowledged;
			box.Commit(subscription);

			// TODO: remove the subscription object?
			box.Commit(box.Delete(subscription));
		}

		public SubscriptionStatus GetSubscriptionStatus(string domain, string identity, string message)
		{
			SubscriptionStatus status = null;

			// open the post office box
			// TODO: POBox box = POBox.GetPOBox(store, domain, identity);
			POBox box = POBox.GetPOBox(store, domain);

			// check the post office box
			if (box == null)
				throw new ApplicationException("PO Box not found.");

			// check that the message has already not been posted
			// check that the message has already not been posted
			ICSList list = box.Search(Message.MessageIDProperty, message, SearchOp.Equal);
			
			ICSEnumerator e = (ICSEnumerator)list.GetEnumerator();
			ShallowNode sn = null;

			if (e.MoveNext())
			{
				sn = (ShallowNode) e.Current;
			}

			if (sn == null)
				throw new ApplicationException("Subscription already exists.");

			// get the status object
			Subscription subscription = new Subscription(box, sn);
			status = subscription.GenerateStatus();

			return status;
		}

		public SubscriptionDetails GetSubscriptionDetails(string domain, string identity, string collection)
		{
			SubscriptionDetails details = new SubscriptionDetails();

			// open the collection
			Collection c = store.GetCollectionByID(collection);

			// check the collection
			if (c == null)
				throw new ApplicationException("Collection not found.");

			SyncCollection sc = new SyncCollection(c);

			// details
			DirNode dn = sc.GetRootDirectory();
			details.DirNodeID = ( dn != null ) ? dn.ID : null;
			details.DirNodeName = ( dn != null ) ? dn.Name : null;
			details.CollectionUrl = sc.MasterUrl.ToString();

			return details;
		}
	}
}
