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
 *  Author: Russ Young
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

		EventBroker broker = null;
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
			//if (EventBroker.RegisterClientChannel(conf))
				broker = EventBroker.GetBroker(conf); //new EventBroker();
		}

		#endregion

		#region Publish Call.

		/// <summary>
		/// Called to publish a collection event.
		/// </summary>
		/// <param name="args"></param>
		public void RaiseEvent(CollectionEventArgs args)
		{
			try
			{
				if (broker == null)
				{
					//if (EventBroker.RegisterClientChannel(conf))
					{
						broker = EventBroker.GetBroker(conf); //new EventBroker();
						broker.RaiseEvent(args);
					}
				}
				else
				{
					broker.RaiseEvent(args);
				}
			}
			catch {}
		}

		#endregion
	}
}
