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
using System.Net;
using System.Collections;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Credential Cache
	/// </summary>
	public class SyncCredentialCache : ICredentials, IEnumerable
	{
		private Hashtable cache;

		public SyncCredentialCache()
		{
			cache = new Hashtable();
		}

		public void Add(string domain, string identity)
		{
			NetworkCredential credential =
				new NetworkCredential(identity, null);
			
			lock(cache.SyncRoot)
			{
				cache.Add(domain, credential);
			}
		}

		public void Remove(string domain)
		{
			lock(cache.SyncRoot)
			{
				cache.Remove(domain);
			}
		}

		#region ICredentials Members

		public NetworkCredential GetCredential(Uri uri, string type)
		{
			NetworkCredential credential =
				(NetworkCredential)cache[uri.PathAndQuery];

			return credential;
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return (cache.Clone() as IEnumerable).GetEnumerator();
		}

		#endregion
	}
}
