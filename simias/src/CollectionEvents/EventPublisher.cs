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
		#region Fields

		EventBroker broker;
		Configuration	conf;

		#endregion
			
		#region Constructor

		/// <summary>
		/// Creates an Event Publisher.
		/// </summary>
		/// <param name="conf">Configuration object.</param>
		public EventPublisher(Configuration conf)
		{
			this.conf = conf;
			EventBroker.RegisterClientChannel(conf);
			broker = new EventBroker();
		}

		#endregion

		#region Publish Call.

		/// <summary>
		/// Called to publish a collection event.
		/// </summary>
		/// <param name="args"></param>
		public void RaiseEvent(CollectionEventArgs args)
		{
			if (broker != null)
				broker.RaiseEvent(args);
		}

		#endregion
	}
}
