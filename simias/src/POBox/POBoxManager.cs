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
using System.Collections;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Diagnostics;

using Simias;
using Simias.Client.Event;
using Simias.Storage;

namespace Simias.POBox
{
	/// <summary>
	/// POBox Manager
	/// </summary>
	public class POBoxManager : IDisposable
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(POBoxManager));

		private POManager poManager;
		private Configuration config;
		private POBox poBox;
		private Store store;
		private EventSubscriber subscriber;
		private Hashtable threads;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="poManager"></param>
		/// <param name="id"></param>
		public POBoxManager(POManager poManager, string id)
		{
			this.poManager = poManager;
			this.config = poManager.Config;

			// new store
			store = Store.GetStore();

			// open POBox
			poBox = POBox.GetPOBoxByID(store, id);

			// threads
			threads = new Hashtable();

			// events
			subscriber = new EventSubscriber(poBox.ID);
			subscriber.Enabled = false;
			subscriber.NodeTypeFilter = NodeTypes.NodeType;
			subscriber.NodeCreated += new NodeEventHandler(OnMessageChanged);
			subscriber.NodeChanged += new NodeEventHandler(OnMessageChanged);
		}

		/// <summary>
		/// Start the PO Box manager.
		/// </summary>
		public void Start()
		{
			subscriber.Enabled = true;
			
			foreach(ShallowNode n in poBox)
			{
				UpdateMessage(n.ID);
			}
		}

		/// <summary>
		/// Stop the PO Box manager.
		/// </summary>
		public void Stop()
		{
			subscriber.Enabled = false;
		}

		private void OnMessageChanged(NodeEventArgs args)
		{
			UpdateMessage(args.ID);
		}

		private void UpdateMessage(string id)
		{
			Node node = poBox.GetNodeByID(id);

			if (node != null)
			{
				if (poBox.IsType(node, typeof(Subscription).Name))
				{
					UpdateSubscription(node);
				}
			}
		}

		private void UpdateSubscription(Node node)
		{
			Subscription subscription = new Subscription(node);

			switch(subscription.SubscriptionState)
			{
				// invited (master)
				case SubscriptionStates.Invited:
				// replied (slave)
				case SubscriptionStates.Replied:
				// delivered (slave)
				case SubscriptionStates.Delivered:
					
					lock(threads.SyncRoot)
					{
						// start threads only on new subscription updates
						if (!threads.Contains(subscription.ID))
						{
							// subscription thread
							SubscriptionThread st = new SubscriptionThread(poBox, subscription, threads);
							Thread thread = new Thread(new ThreadStart(st.Run));
							thread.IsBackground = true;
							thread.Priority = ThreadPriority.BelowNormal;
							thread.Start();

							// save thread
							threads.Add(subscription.ID, thread);
						}
					}
					break;

				default:
					break;
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
		}

		#endregion
	}
}
