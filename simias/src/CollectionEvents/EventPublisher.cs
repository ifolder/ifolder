/***********************************************************************
 *  EventPublisher.cs - An event publisher class.
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Russ Young <ryoung@novell.com>
 * 
 ***********************************************************************/
using System;
using System.Diagnostics;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Simias.Event
{
	/// <summary>
	/// Class used to publish events.
	/// </summary>
	public class EventPublisher
	{
		EventBroker broker;
		string			domain;
		Configuration	conf;
			
		#region Constructor

		/// <summary>
		/// Creates an Event Publisher.
		/// </summary>
		/// <param name="domain">The domain for this publisher obtained from the CollectionStore.</param>
		public EventPublisher(Configuration conf, string domain)
		{
			this.conf = conf;
			this.domain = domain;
			EventBroker.RegisterClientChannel(conf, domain);
			broker = new EventBroker();
		}

		#endregion

		#region Publish Calls.

		/// <summary>
		/// Called to publish a Collection root change event.
		/// </summary>
		/// <param name="args"></param>
		public void RaiseCollectionRootChangedEvent(CollectionRootChangedEventArgs args)
		{
			if (broker != null)
				broker.RaiseCollectionRootChangedEvent(args);
		}

		/// <summary>
		/// Called to publish a Node event.
		/// </summary>
		/// <param name="args"></param>
		public void RaiseNodeEvent(NodeEventArgs args)
		{
			if (broker != null)
			{
				//broker.RaiseEvent(args.ChangeType, args);
				broker.RaiseNodeEvent(args);
			}
		}

		/// <summary>
		/// Called to publish a File event.
		/// </summary>
		/// <param name="args"></param>
		public void RaiseFileEvent(FileEventArgs args)
		{
			if (broker != null)
				broker.RaiseFileEvent(args);
		}

		/// <summary>
		/// Called to publish a service event to listening services.
		/// </summary>
		/// <param name="args">Arguments for the event.</param>
		public void RaiseServiceEvent(ServiceEventArgs args)
		{
			if (broker != null)
				broker.RaiseServiceEvent(args);
		}

		#endregion
	}
}
