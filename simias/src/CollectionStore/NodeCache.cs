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
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/
using System;
using System.Collections;
using System.Threading;
using System.Xml;

namespace Simias.Storage
{
	/// <summary>
	/// Object that handles cache life cycle of node objects.
	/// </summary>
	class NodeCache
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( NodeCache ) );

		/// <summary>
		/// Cache configuration values.
		/// </summary>
		private const string NodeCacheSectionTag = "NodeCache";
		private const string TimeToLiveTag = "TimeToLive";
		private const int defaultTimeToLive = 60;				// 60 seconds.
		private int cachePurgeTimeout;
		private int cacheTimeToLive;

		/// <summary>
		/// Hashtable used to hold the cached node entries.
		/// </summary>
		static private Hashtable cacheTable = null;

		/// <summary>
		/// Store handle.
		/// </summary>
		static private Store store;

		// TODO: Remove
		static private float hitCount = 0;
		static private float accessCount = 0;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object.
		/// </summary>
		/// <param name="store">Reference to the store.</param>
		public NodeCache( Store store )
		{
			lock ( typeof( NodeCache ) )
			{
				// Don't allow more than one instance.
				if ( cacheTable == null )
				{
					// Initialize the hashtable.
					cacheTable = new Hashtable();
					NodeCache.store = store;

					// Get the cache entry time to live.
					string timeString = store.Config.Get( NodeCacheSectionTag, TimeToLiveTag, defaultTimeToLive.ToString() );
					cacheTimeToLive = Convert.ToInt32( timeString );

					// Make sure the time to live is between 1 second and 30 minutes.
					if ( ( cacheTimeToLive < 1 ) || ( cacheTimeToLive > 1800 ) )
					{
						cacheTimeToLive = defaultTimeToLive;
						store.Config.Set( NodeCacheSectionTag, TimeToLiveTag, defaultTimeToLive.ToString() );
					}

					// Set the time to wait to purge the cache to 1/2 of the time to live.
					cachePurgeTimeout = ( cacheTimeToLive == 1 ) ? 1000 : ( ( cacheTimeToLive / 2 ) * 1000 );

					// Spin up the thread that will timeout the cache entries.
					Thread thread = new Thread( new ThreadStart( CacheThread ) );
					thread.IsBackground = true;
					thread.Priority = ThreadPriority.BelowNormal;
					thread.Start();
				}
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Thread that ages the cache and purges expired entries.
		/// </summary>
		private void CacheThread()
		{
			// Don't ever exit.
			while ( true )
			{
				try
				{
					lock ( typeof( NodeCache ) )
					{
						// TODO: Remove
//						log.Debug( "Cache efficency = {0}%, Current count = {1}, Access Count = {2}, Hit Count = {3}", ( ( accessCount > 0 ) ? ( hitCount / accessCount ) * 100 : 0 ), cacheTable.Count, accessCount, hitCount );

						ArrayList keyList = new ArrayList( cacheTable.Keys );
						foreach( string key in keyList )
						{
							NodeCacheEntry entry = cacheTable[ key ] as NodeCacheEntry;
							if ( !entry.IsValid )
							{
								cacheTable.Remove( key );
							}
						}
					}

					Thread.Sleep( cachePurgeTimeout );
				}
				catch ( Exception ex )
				{
					log.Error( ex, "NodeCache Thread exception" );
				}
			}
		}

		/// <summary>
		/// Constructs a node object from its string representation.
		/// </summary>
		/// <param name="nodeString">String containing node object information.</param>
		/// <returns>A node object.</returns>
		private Node NodeFromString( string nodeString )
		{
			// Construct the proper type of node object.
			XmlDocument document = new XmlDocument();
			document.LoadXml( nodeString );
			return Node.NodeFactory( store, document );
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds a node object to the cache.
		/// </summary>
		/// <param name="node">The node object to add to the cache.</param>
		public void Add( Node node )
		{
			lock ( typeof( NodeCache ) )
			{
				// Check to see if the entry is already in the cache.
				NodeCacheEntry entry = cacheTable[ node.ID ] as NodeCacheEntry;
				if ( entry == null )
				{
					cacheTable[ node.ID ] = new NodeCacheEntry( node.Properties.ToString(), cacheTimeToLive );
				}
				else
				{
					entry.Value = node.Properties.ToString();
				}
			}
		}

		/// <summary>
		/// Gets a node object from the cache table.
		/// </summary>
		/// <param name="nodeID">The ID of the node object.</param>
		/// <returns>A node object if the ID exists in the cache. Otherwise a null is returned.</returns>
		public Node Get( string nodeID )
		{
			string cacheValue = null;

			lock ( typeof( NodeCache ) )
			{
				// TODO: Remove
				accessCount++;

				NodeCacheEntry entry = cacheTable[ nodeID ] as NodeCacheEntry;
				if ( entry != null )
				{
					// Keep this entry in the cache.
					entry.UpdateExpirationTime();
					cacheValue = entry.Value as string;

					// TODO: Remove
					hitCount++;
				}
			}

			return ( cacheValue != null ) ? NodeFromString( cacheValue ) : null;
		}

		/// <summary>
		/// Removes a node object from the cache.
		/// </summary>
		/// <param name="nodeID">The ID of the node object to remove from the cache.</param>
		public void Remove( string nodeID )
		{
			lock ( typeof( NodeCache ) )
			{
				cacheTable.Remove( nodeID );
			}
		}
		#endregion

		#region NodeCacheEntry
		/// <summary>
		/// Structure used to hold cache entry data.
		/// </summary>
		private class NodeCacheEntry
		{
			#region Class Members
			/// <summary>
			/// The number of seconds that the entry is valid in the cache.
			/// </summary>
			public TimeSpan cacheExpirationInterval;

			/// <summary>
			/// The value of the cache entry.
			/// </summary>
			public string nodeString;

			/// <summary>
			/// The absolute time when the entry should be purged from
			/// the cache.
			/// </summary>
			public DateTime expirationTime;
			#endregion

			#region Properties
			/// <summary>
			/// Gets the cache expiration interval.
			/// </summary>
			public TimeSpan CacheExpirationInterval
			{
				get { return cacheExpirationInterval; }
			}

			/// <summary>
			/// Gets the expiration time.
			/// </summary>
			public DateTime ExpirationTime
			{
				get { return expirationTime; }
			}

			/// <summary>
			/// Gets whether this entry is valid.
			/// </summary>
			public bool IsValid
			{
				get { return ( DateTime.Now >= expirationTime ) ? false : true; } 
			}

			/// <summary>
			/// Gets or sets the entry value.
			/// </summary>
			public string Value
			{
				get { return nodeString; }
				set 
				{
					nodeString = value; 					
					UpdateExpirationTime();
				}
			}
			#endregion

			#region Constructor
			/// <summary>
			/// Initializes a new instance of the object.
			/// </summary>
			/// <param name="nodeString">The string representation of a node object.</param>
			/// <param name="cacheTimeToLive">Number of seconds object should remain in the cache.</param>
			public NodeCacheEntry( string nodeString, int cacheTimeToLive )
			{
				cacheExpirationInterval = new TimeSpan( 0, 0, cacheTimeToLive );
				Value = nodeString;
			}
			#endregion

			#region Public Methods
			/// <summary>
			/// Updates the expiration time of an entry to keep it valid in the cache.
			/// </summary>
			public void UpdateExpirationTime()
			{
				expirationTime = DateTime.Now + cacheExpirationInterval;
			}
			#endregion
		}
		#endregion
	}
}
