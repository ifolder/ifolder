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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

using Simias;
using Simias.Event;
using Simias.Service;
using Simias.Storage;

namespace Simias.Presence
{
	/// <summary>
	/// Class the handles presence as a service
	/// </summary>
	public class PresenceService : IThreadService
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
				SimiasLogManager.GetLogger( typeof( ChangeLog ) );

		/// <summary>
		/// Configuration object for the Collection Store.
		/// </summary>
		private Configuration config;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		public PresenceService()
		{
		}
		#endregion

		#region IThreadService Members
		/// <summary>
		/// Starts the thread service.
		/// </summary>
		/// <param name="config">
		/// Configuration file object that indicates which Collection 
		/// Store to use.
		/// </param>
		public void Start( Configuration config )
		{
			this.config = config;

			Store store = Store.GetStore();

			ICSList list = store.GetCollectionsByType("iFolder");	
			foreach(ShallowNode sn in list)
			{
				try
				{
					PublicSubscription.AddSubscription(sn.ID);
					log.Info( "Added presence for {0}", sn.ID);
				}
				catch(Exception e)
				{
					log.Info( "!! Unable to add presence for {0}", sn.ID);
				}
			}





		}

		/// <summary>
		/// Resumes a paused service. 
		/// </summary>
		public void Resume()
		{
		}

		/// <summary>
		/// Pauses a service's execution.
		/// </summary>
		public void Pause()
		{
		}

		/// <summary>
		/// Custom.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public void Custom(int message, string data)
		{
		}

		/// <summary>
		/// Stops the service from executing.
		/// </summary>
		public void Stop()
		{
		}
		#endregion
	}
}
