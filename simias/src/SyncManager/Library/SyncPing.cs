/***********************************************************************
 *  $RCSfile$
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
 *  Author: Rob
 * 
 ***********************************************************************/

using System;
using System.Threading;
using System.Net;

using Simias;
using Simias.Sync;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Ping
	/// </summary>
	public class SyncPing
	{
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
		public static SyncStoreInfo PingStore(SyncStore store, string host)
		{
			return PingStore(store, host, SyncProperties.SuggestedPort);
		}

		/// <summary>
		/// Ping the sync store server.
		/// </summary>
		/// <param name="syncStore">The sync store object.</param>
		/// <param name="host">The sync store server host.</param>
		/// <param name="port">The sync store sever port.</param>
		/// <returns>A ping object from the server.</returns>
		public static SyncStoreInfo PingStore(SyncStore syncStore, string host, int port)
		{
			SyncStoreInfo info = null;

			// create channel
			SyncChannel channel = SyncChannelFactory.GetInstance().GetChannel(syncStore);

			// server uri
			string serverUrl = new UriBuilder("http", host, port,
				syncStore.EndPoint).ToString();

			try
			{
				// get a proxy to the sync store service object
				SyncStoreService store = (SyncStoreService)Activator.GetObject(
				typeof(SyncStoreService), serverUrl);

				// ping
				info = store.Ping();
			}
			catch(Exception e)
			{
				MyTrace.WriteLine("Ping Failed: {0}", e.Message);
			}

			// close
			channel.Dispose();

			MyTrace.WriteLine("Ping: {0}", info);

			return info;
		}
	}
}
