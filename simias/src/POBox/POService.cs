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
using System.Diagnostics;

using Simias.Client;
using Simias.Client.Event;
using Simias.DomainServices;
using Simias.Service;
using Simias.Storage;

namespace Simias.POBox
{
	/// <summary>
	/// PO Service
	/// </summary>
	public class POService : IThreadService
	{
		#region Class Members

		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( POService ) );

		/// <summary>
		/// Subscribers used to watch for important POBox events.
		/// </summary>
		private EventSubscriber poBoxSubscriber;
		private EventSubscriber noAccessSubscriber;

		/// <summary>
		/// Hashtable used to map from a POBox ID to its domain.
		/// </summary>
		private Hashtable poBoxTable = Hashtable.Synchronized( new Hashtable() );

		/// <summary>
		/// Store object.
		/// </summary>
		private Store store;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		public POService()
		{
			// events
			poBoxSubscriber = new EventSubscriber();
			poBoxSubscriber.NodeTypeFilter = NodeTypes.POBoxType;
			poBoxSubscriber.NodeCreated += new NodeEventHandler(OnPOBoxCreated);
			poBoxSubscriber.NodeDeleted += new NodeEventHandler(OnPOBoxDeleted);

			// Removes invitations from POBoxes.
			noAccessSubscriber = new EventSubscriber();
			noAccessSubscriber.Enabled = true;
			noAccessSubscriber.NoAccess += new NodeEventHandler(OnCollectionNoAccess);

			store = Store.GetStore();
		}

		#endregion

		#region Private Methods

		private void OnPOBoxCreated(NodeEventArgs args)
		{
			// Get the POBox that caused the event.
			POBox poBox = POBox.GetPOBoxByID( store, args.ID );
			if ( poBox != null )
			{
				// Save the domain ID for this POBox.
				poBoxTable[ args.ID ] = poBox.Domain;
			}
		}

		private void OnPOBoxDeleted(NodeEventArgs args)
		{
			// Get the domain ID for this PO box from its name.
			string domainID = poBoxTable[ args.ID ] as string;
			if ( domainID != null )
			{
				// This POBox is being deleted. Call to get rid of the domain information.
				new DomainAgent().RemoveDomainInformation( domainID );
				poBoxTable.Remove( args.ID );
			}
		}

		/// <summary>
		/// Removes all subscriptions for the collection that is contained in the event.
		/// </summary>
		/// <param name="args">Node event arguments.</param>
		private void OnCollectionNoAccess( NodeEventArgs args )
		{
			// Make sure that this is an event for a collection.
			if ( args.Collection == args.ID )
			{
				// Search the POBox collections for a subscription for this collection.
				Property p = new Property( Subscription.SubscriptionCollectionIDProperty, args.ID );

				// Find all of the subscriptions for this POBox.
				ICSList list = store.GetNodesByProperty( p, SearchOp.Equal );
				foreach (ShallowNode sn in list)
				{
					// Make sure that this node is a subscription.
					if ( sn.Type == NodeTypes.SubscriptionType )
					{
						// Get the collection (POBox) for this subscription.
						POBox poBox = POBox.GetPOBoxByID( store, sn.CollectionID );
						if ( poBox != null )
						{
							// Delete this subscription from the POBox.
							poBox.Commit( poBox.Delete( new Subscription( poBox, sn ) ) );
						}
					}
				}
			}
		}

		#endregion

		#region BaseProcessService Members

		/// <summary>
		/// Start the PO service.
		/// </summary>
		public void Start()
		{
			// Get a list of all POBoxes.
			ICSList poBoxList = store.GetCollectionsByType( NodeTypes.POBoxType );
			foreach( ShallowNode sn in poBoxList )
			{
				// Add the existing POBoxes to the mapping table.
				POBox poBox = new POBox( store, sn );
				poBoxTable[ poBox.ID ] = poBox.Domain;
			}

			poBoxSubscriber.Enabled = true;
		}

		/// <summary>
		/// Stop the PO service.
		/// </summary>
		public void Stop()
		{
			poBoxSubscriber.Enabled = false;
		}

		/// <summary>
		/// Resume the PO service.
		/// </summary>
		public void Resume()
		{
			poBoxSubscriber.Enabled = true;
		}

		/// <summary>
		/// Pause the PO service.
		/// </summary>
		public void Pause()
		{
			poBoxSubscriber.Enabled = false;
		}

		/// <summary>
		/// A custom event for the PO service.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public int Custom( int message, string data )
		{
			return 0;
		}

		#endregion
	}
}
