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
using System.Threading;
using System.Collections;
using System.Runtime.Remoting;
using System.Diagnostics;

using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.DomainServices;
using Simias.Storage;
using Simias.Sync;

namespace Simias.POBox
{
	/// <summary>
	/// PO Manager
	/// </summary>
	public class POManager : IDisposable
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(POManager));

		private Store store;
		private Hashtable boxManagers;
		private EventSubscriber subscriber;
		private EventSubscriber invitationSubscriber;
		private Uri serviceUrl = null;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="config">Simias configuration</param>
		public POManager()
		{
			// store
			store = Store.GetStore();

			// box managers
			boxManagers = new Hashtable();

			// events
			subscriber = new EventSubscriber();
			subscriber.Enabled = true;
			subscriber.NodeTypeFilter = NodeTypes.POBoxType;
			subscriber.NodeCreated += new NodeEventHandler(OnPOBoxCreated);
			subscriber.NodeDeleted += new NodeEventHandler(OnPOBoxDeleted);

			// Removes invitations from POBoxes.
			invitationSubscriber = new EventSubscriber();
			invitationSubscriber.Enabled = true;
			invitationSubscriber.NoAccess += new NodeEventHandler(OnCollectionNoAccess);
		}
		/// <summary>
		/// Start the PO Box manager.
		/// </summary>
		public void Start()
		{
			try
			{
				lock(this)
				{
					log.Debug("Starting PO Service: {0}", ServiceUrl);

					// Get a list of all POBoxes.
					ICSList poBoxList = store.GetCollectionsByType(NodeTypes.POBoxType);
					foreach(ShallowNode sn in poBoxList)
					{
						// Get the domain for this POBox.
						POBox poBox = new POBox(store, sn);
						Simias.Storage.Domain domain = store.GetDomain(poBox.Domain);
						//if (domain.Role == SyncRoles.Slave)
						//{
							// start collection managers
							AddPOBoxManager(poBox.ID);
						//}
					}
				}
			}
			catch(Exception e)
			{
				log.Error(e, "Unable to start PO manager.");

				throw e;
			}
		}

		/// <summary>
		/// Stop the PO Box manager.
		/// </summary>
		public void Stop()
		{
			try
			{
				lock(this)
				{
					// stop collection managers
					subscriber.Enabled = false;
					invitationSubscriber.Enabled = false;

					foreach(string id in new ArrayList(boxManagers.Keys))
					{
						RemovePOBoxManager(id);
					}

					subscriber.Dispose();
					invitationSubscriber.Dispose();
				}
			}
			catch(Exception e)
			{
				log.Error(e, "Unable to stop store manager.");
				throw e;
			}
		}

		private void AddPOBoxManager(string id)
		{
			POBoxManager manager;
			
			lock(boxManagers.SyncRoot)
			{
				if (!boxManagers.Contains(id))
				{
					log.Debug("Adding PO Box Manager: {0}", id);

					try
					{
						manager = new POBoxManager(this, id);
			
						manager.Start();

						boxManagers.Add(id, manager);
					}
					catch(Exception e)
					{
						log.Debug(e, "Ignored");
					}
				}
			}
		}

		private void RemovePOBoxManager(string id)
		{
			POBoxManager manager;
			
			lock(boxManagers.SyncRoot)
			{
				if (boxManagers.Contains(id))
				{
					log.Debug("Removing PO Box Manager: {0}", id);

					try
					{
						manager = (POBoxManager)boxManagers[id];
			
						manager.Stop();

						manager.Dispose();

						boxManagers.Remove(id);
					}
					catch(Exception e)
					{
						log.Debug(e, "Ignored");
					}
				}
			}
		}

		private void OnPOBoxCreated(NodeEventArgs args)
		{
			Collection c = store.GetCollectionByID(args.ID);
			if (c.IsBaseType(c, NodeTypes.POBoxType))
			{
				AddPOBoxManager(args.ID);
			}
		}

		private void OnPOBoxDeleted(NodeEventArgs args)
		{
			lock(boxManagers.SyncRoot)
			{
				if (boxManagers.Contains(args.ID))
				{
					// This POBox is being deleted. Call to get rid of the domain information.
					DomainAgent agent = new DomainAgent();
					agent.RemoveDomainInformation((boxManagers[args.ID] as POBoxManager).Domain);
					RemovePOBoxManager(args.ID);
				}
			}
		}

		/// <summary>
		/// Removes all subscriptions for the collection that is contained in the event.
		/// </summary>
		/// <param name="args">Node event arguments.</param>
		private void OnCollectionNoAccess(NodeEventArgs args)
		{
			// Make sure that this is an event for a collection.
			if (args.Collection == args.ID)
			{
				// Search the POBox collections for a subscription for this collection.
				Property p = new Property(Subscription.SubscriptionCollectionIDProperty, args.ID);

				// Find all of the subscriptions for this POBox.
				ICSList list = store.GetNodesByProperty(p, SearchOp.Equal);
				foreach (ShallowNode sn in list)
				{
					// Make sure that this node is a subscription.
					if (sn.Type == NodeTypes.SubscriptionType)
					{
						// Get the collection (POBox) for this subscription.
						POBox poBox = POBox.GetPOBoxByID(store, sn.CollectionID);
						if ( poBox != null )
						{
							// Delete this subscription from the POBox.
							poBox.Commit(poBox.Delete(new Subscription(poBox, sn)));
						}
					}
				}
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			Stop();
		}

		#endregion

		#region Properties
		
		/// <summary>
		/// Service URL
		/// </summary>
		public Uri ServiceUrl
		{
			get { return serviceUrl; }
			set { serviceUrl = value; }
		}

		#endregion
	}
}
