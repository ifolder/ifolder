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

using Simias.Client;

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
					string timeString = store.Config.Get( NodeCacheSectionTag, TimeToLiveTag );
					cacheTimeToLive = ( timeString != null ) ? Convert.ToInt32( timeString ) : defaultTimeToLive;

					// Make sure the time to live is between 1 second and 30 minutes.
					if ( ( cacheTimeToLive < 1 ) || ( cacheTimeToLive > 1800 ) )
					{
						cacheTimeToLive = defaultTimeToLive;
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
		/// Creates a key to use in the cache table.
		/// </summary>
		/// <param name="collectionID">Collection ID</param>
		/// <param name="nodeID">Node ID</param>
		/// <returns>A string to use as a key in the cache table.</returns>
		private string CreateKey( string collectionID, string nodeID )
		{
			return collectionID + ":" + nodeID;
		}

		/// <summary>
		/// Determines if the node may be cached.
		/// </summary>
		/// <param name="collection">Collection that the node belongs to.</param>
		/// <param name="node">Node object to be cached.</param>
		/// <returns>True if the node may be cached, otherwise false is returned.</returns>
		private bool IsCacheable( Collection collection, Node node )
		{
			bool cacheable = false;
			bool isCollectionNode = false;
			bool isMemberNode = false;
			bool isFileNode = false;
			bool isDirNode = false;

			// Get a list of all the node types for this node. This is done rather than
			// calling the IsType method on the collection for each type to be checked.
			// That way the type properties are only searched for one time instead of
			// multiple times.
			MultiValuedList mvl = node.Properties.FindValues( PropertyTags.Types );
			foreach( Property p in mvl )
			{
				string s = p.ToString();
				if ( s.Equals( NodeTypes.CollectionType ) )
				{
					isCollectionNode = true;
					break;
				}
				else if ( s.Equals( NodeTypes.MemberType ) )
				{
					isMemberNode = true;
					break;
				}
				else if ( s.Equals( NodeTypes.BaseFileNodeType ) )
				{
					isFileNode = true;
					break;
				}
				else if ( s.Equals( NodeTypes.DirNodeType ) )
				{
					isDirNode = true;
					break;
				}
			}

			// Is the node a collection?
			if ( isCollectionNode )
			{
				// All collections may be cached.
				cacheable = true;
			}
			else
			{
				// Is the node a member?
				if ( isMemberNode )
				{
					// Is this collection a domain?
					if ( !collection.IsBaseType( collection, NodeTypes.DomainType ) )
					{
						// All member nodes that do not belong to a domain are cacheable.
						cacheable = true;
					}
				}
				else
				{
					// Is the node a file node or a dirNode?
					if ( !isFileNode && !isDirNode )
					{
						// All other nodes that are not file nodes or dir nodes may be cached.
						cacheable = true;
					}
				}
			}

			return cacheable;
		}

		/// <summary>
		/// Determines if a key belongs to the specified collection.
		/// </summary>
		/// <param name="key">Cache table key.</param>
		/// <param name="collectionID">Collection ID.</param>
		/// <returns>True if key belongs to collection. Otherwise False is returned.</returns>
		private bool IsFromCollection( string collectionID, string key )
		{
			return key.StartsWith( collectionID );
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
		/// <param name="collection">The collection that the node object belongs to.</param>
		/// <param name="node">The node object to add to the cache.</param>
		public void Add( Collection collection, Node node )
		{
			// Make sure that the node can be cached.
			if ( IsCacheable( collection, node ) )
			{
				// Generate a key to use with the cache table.
				string key = CreateKey( collection.ID, node.ID );

				lock ( typeof( NodeCache ) )
				{
					// Check to see if the entry is already in the cache.
					NodeCacheEntry entry = cacheTable[ key ] as NodeCacheEntry;
					if ( entry == null )
					{
						cacheTable[ key ] = new NodeCacheEntry( node.Properties.ToString(), cacheTimeToLive );
					}
					else
					{
						entry.Value = node.Properties.ToString();
					}
				}
			}
		}

		/// <summary>
		/// Empties out the cache.
		/// </summary>
		public void DumpCache()
		{
			lock ( typeof( NodeCache ) )
			{
				cacheTable.Clear();
			}
		}

		/// <summary>
		/// Removes all nodes from the specified collection from the cache.
		/// </summary>
		/// <param name="collectionID">The ID of the collection to remove.</param>
		public void DumpCache( string collectionID )
		{
			lock ( typeof( NodeCache ) )
			{
				ArrayList keyList = new ArrayList( cacheTable.Keys );
				foreach( string key in keyList )
				{
					if ( IsFromCollection( collectionID, key ) )
					{
						cacheTable.Remove( key );
					}
				}
			}
		}

		/// <summary>
		/// Gets a node object from the cache table.
		/// </summary>
		/// <param name="collectionID">The ID of the collection that the node belongs to.</param>
		/// <param name="nodeID">The ID of the node object.</param>
		/// <returns>A node object if the ID exists in the cache. Otherwise a null is returned.</returns>
		public Node Get( string collectionID, string nodeID )
		{
			string cacheValue = null;
			string key = CreateKey( collectionID, nodeID );

			lock ( typeof( NodeCache ) )
			{
				// TODO: Remove
				accessCount++;

				NodeCacheEntry entry = cacheTable[ key ] as NodeCacheEntry;
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
		/// <param name="collectionID">The ID of the collection that the node belongs to.</param>
		/// <param name="nodeID">The ID of the node object to remove from the cache.</param>
		public void Remove( string collectionID, string nodeID )
		{
			string key = CreateKey( collectionID, nodeID );
			lock ( typeof( NodeCache ) )
			{
				cacheTable.Remove( key );
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
