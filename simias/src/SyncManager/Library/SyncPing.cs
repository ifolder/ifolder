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
using System.Net;

using Simias;
using Simias.Storage;
using Simias.Sync;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Ping
	/// </summary>
	public class SyncPing
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncPing));

		/// <summary>
		/// Default Constructor
		/// </summary>
		private SyncPing()
		{
		}

		/// <summary>
		/// Ping the sync store server.
		/// </summary>
		/// <param name="store">The sync store object.</param>
		/// <param name="host">The sync store server host.</param>
		/// <returns>A ping object from the server.</returns>
		public static SyncStoreInfo PingStore(Store store, string host)
		{
			return PingStore(store, host, SyncProperties.SuggestedPort);
		}

		/// <summary>
		/// Ping the sync store server.
		/// </summary>
		/// <param name="store">The store object.</param>
		/// <param name="host">The sync store server host.</param>
		/// <param name="port">The sync store sever port.</param>
		/// <returns>A ping object from the server.</returns>
		public static SyncStoreInfo PingStore(Store store, string host, int port)
		{
			SyncStoreInfo info = null;

			// create channel
			SyncChannel channel = SyncChannelFactory.GetInstance().GetChannel(store, SyncProperties.SuggestedChannelSinks);

			// service URL
			string serviceUrl = new UriBuilder("http", host, port, SyncStoreService.EndPoint).ToString();

			try
			{
				// get a proxy to the sync store service object
				SyncStoreService service = (SyncStoreService)Activator.GetObject(
					typeof(SyncStoreService), serviceUrl);

				// ping
				info = service.Ping();
			}
			catch(Exception e)
			{
				log.Error(e, "Ping Failed");
			}

			// close
			channel.Dispose();

			log.Debug("Ping: {0}", info);

			return info;
		}
	}
}
