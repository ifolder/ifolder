/***********************************************************************
 *  Node.cs - Class that contains properties that describe objects. This
 *  class is the base unit of storage in the collection store.
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
 *  Author: Mike Lasky <mlasky@novell.com>
 * 
 ***********************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

using Simias;
using Simias.Event;
using Persist = Simias.Storage.Provider;

namespace Simias.Storage
{
	/// <summary>
	/// Represents a node in the hierarchy that makes up a Collection Store.
	/// A node can be a leaf or a container.  A node can also contain properties.
	/// </summary>
	public class Node : IEnumerable
	{
		#region Class Members
		/// <summary>
		/// Well known type of node.
		/// </summary>
		internal const string CollectionType = "Collection.";

		/// <summary>
		/// Well known type of node.
		/// </summary>
		private const string NodeType = "Node.";

		/// <summary>
		/// Well known type of node.  Represents a deleted node.
		/// </summary>
		private const string TombstoneType = "Tombstone.";

		/// <summary>
		/// Generic type.
		/// </summary>
		public const string Generic = "Generic";

		/// <summary>
		/// Handle to the store object.
		/// </summary>
		internal Store store;

		/// <summary>
		/// Reference to node data. 
		/// </summary>
		internal CacheNode cNode = null;
		#endregion

		#region Properties
		/// <summary>
		/// Allows the collection object to set the collection object during construction.
		/// </summary>
		internal Collection InternalCollectionHandle
		{
			set { cNode.collection = value; }
		}

		/// <summary>
		/// Gets the type of node with its accompanying name space.
		/// </summary>
		internal string NameSpaceType
		{
			get { return cNode.type; }

			set 
			{ 
				cNode.type = value;
				SetNodeAttribute( Properties.PropertyRoot, Property.TypeAttr, cNode.type );
				CollectionNode.AddDirtyNodeToList( this );
			}
		}

		/// <summary>
		/// Gets the internal ID path property for this node.
		/// </summary>
		internal string IDPath
		{
			get
			{
				Property p = Properties.GetSingleProperty( Property.IDPath );
				if ( p != null )
				{
					return p.ToString();
				}
				else
				{
					return IsCollection ? "/" + Id : Id;
				}
			}
		}

		/// <summary>
		/// Gets or sets whether this node has been committed to the database.
		/// </summary>
		internal bool IsPersisted
		{
			get { return cNode.isPersisted; }
			set { cNode.isPersisted = value; }
		}

		/// <summary>
		/// Gets the collection that this node belongs to.
		/// </summary>
		public Collection CollectionNode
		{
			get 
			{ 
				lock ( store )
				{
					return cNode.collection; 
				}
			}
		}

		/// <summary>
		/// Gets whether this node has children.
		/// </summary>
		public bool HasChildren
		{
			get
			{
				lock ( store )
				{
					// Search for any children belonging to this node.
					ICSEnumerator e = ( ICSEnumerator )CollectionNode.Search( Property.ParentID, Id, Property.Operator.Equal ).GetEnumerator();
					bool isParent = e.MoveNext();
					e.Dispose();
					return isParent;
				}
			}
		}

		/// <summary>
		/// Gets the globally unique identifier for this node.
		/// </summary>
		public string Id
		{
			get 
			{ 
				lock ( store )
				{
					return cNode.id; 
				}
			}
		}

		/// <summary>
		/// Gets or sets the name of this node.
		/// </summary>
		public string Name
		{
			get 
			{ 
				lock ( store )
				{
					return cNode.name; 
				}
			}

			set 
			{ 
				lock ( store )
				{
					cNode.name = value;
					SetNodeAttribute( Properties.PropertyRoot, Property.NameAttr, cNode.name );
					CollectionNode.AddDirtyNodeToList( this );
				}
			}
		}

		/// <summary>
		/// Gets the type of node (Collection, Node, etc.) for this node.
		/// </summary>
		public string Type
		{
			get 
			{ 
				lock ( store )
				{
					return NameSpaceType.Substring( NameSpaceType.IndexOf( '.' ) + 1 ); 
				}
			}
		}

		/// <summary>
		/// Gets whether this is a Collection type node.
		/// </summary>
		public bool IsCollection
		{
			get 
			{ 
				lock ( store )
				{
					return NameSpaceType.StartsWith( CollectionType ); 
				}
			}
		}

		/// <summary>
		/// Gets whether this is a Node type node.
		/// </summary>
		public bool IsNode
		{
			get 
			{ 
				lock ( store )
				{
					return NameSpaceType.StartsWith( NodeType ); 
				}
			}
		}

		/// <summary>
		/// Gets whether this is a Tombstone type node.
		/// </summary>
		public bool IsTombstone
		{
			get 
			{ 
				lock ( store )
				{
					return NameSpaceType.StartsWith( TombstoneType ); 
				}
			}
		}

		/// <summary>
		/// Gets the list of properties for this node.
		/// </summary>
		public PropertyList Properties
		{
			get 
			{ 
				lock ( store )
				{
					if ( cNode.properties == null )
					{
						// The node is not fully instantiated.  Go and get the properties for this node.
						SetNodeProperties();
					}

					return cNode.properties; 
				}
			}
		}

		/// <summary>
		/// Gets the path name for this node.  The path name for a node consists of the hierarchy of
		/// names from the collection to the leaf node.
		/// </summary>
		public string PathName
		{
			get
			{
				lock ( store )
				{
					StringBuilder sb = new StringBuilder();
					Node parentNode = this;

					while ( parentNode != null )
					{
						// Insert the friendly name into the path.
						sb.Insert( 0, "/" + parentNode.Name );

						// Get this node's parent.
						parentNode = parentNode.GetParent();
					}

					return sb.ToString();
				}
			}
		}

		/// <summary>
		/// Gets the local incarnation value from the node.
		/// </summary>
		public ulong LocalIncarnation
		{
			get 
			{
				lock ( store )
				{
					Property p = Properties.GetSingleProperty( Property.LocalIncarnation );
					if ( p == null )
					{
						throw new ApplicationException( "Node does not have a local incarnation property." );
					}

					return ( ulong )p.Value;
				}
			}
		}

		/// <summary>
		/// Gets the master incarnation value from the node.
		/// </summary>
		public ulong MasterIncarnation
		{
			get 
			{
				lock ( store )
				{
					Property p = Properties.GetSingleProperty( Property.MasterIncarnation );
					if ( p == null )
					{
						throw new ApplicationException( "Node does not have a master incarnation property." );
					}

					return ( ulong )p.Value;
				}
			}
		}
		#endregion

		#region Constructors / Finalizer
		/// <summary>
		/// Constructor for creating new and existing node objects where the collection is not known ahead of time.
		/// If this is a new node, a new property list is instaniated, otherwise the node is built without properties.
		/// NOTE: The collection member must be set after construction of this object and the cNode object must be added
		/// to the cacheTable.
		/// </summary>
		/// <param name="store">Object that represents the store.</param>
		/// <param name="name">This is the name that is used by applications to describe the node.</param>
		/// <param name="id">The globally unique identifier for this node.</param>
		/// <param name="type">Type of node to create.</param>
		/// <param name="persisted">Set to true if this is a node that already exists in the database, otherwise false.</param>
		internal Node( Store store, string name, string id, string type, bool persisted )
		{
			this.store = store;

			cNode = new CacheNode( store, id );
			cNode.name = name;
			cNode.type = type;
			cNode.isPersisted = persisted;

			if ( !persisted )
			{
				// This is a new node, create a property list.
				cNode.properties = new PropertyList( this );
			}
		}

		/// <summary>
		/// Constructor for creating a new node object. The newly created node has no parental association.
		/// This node will need to be inserted into a parent node before it can be committed.
		/// </summary>
		/// <param name="collection">The collection that this node belongs to.</param>
		/// <param name="name">This is the name that is used by applications to describe the node.</param>
		/// <param name="id">The globally unique identifier for this node.</param>
		/// <param name="type">Type of node to create.</param>
		public Node( Collection collection, string name, string id, string type )
		{
			store = collection.LocalStore;

			cNode = new CacheNode( store, id.ToLower() );
			cNode.collection = collection;
			cNode.name = name;
			cNode.type = NodeType + type;
			cNode.properties = new PropertyList( this );
			cNode.isPersisted = false;
			
			// Set the default properties for this node.
			Properties.AddNodeProperty( Property.CreationTime, DateTime.UtcNow );
			Properties.AddNodeProperty( Property.ModifyTime, DateTime.UtcNow );
			Properties.AddNodeProperty( Property.CollectionID, collection.Id );

			Property mvProp = new Property( Property.MasterIncarnation, ( ulong )0 );
			mvProp.LocalProperty = true;
			Properties.AddNodeProperty( mvProp );

			Property lvProp = new Property( Property.LocalIncarnation, ( ulong )0 );
			lvProp.LocalProperty = true;
			Properties.AddNodeProperty( lvProp );

			cNode = cNode.AddToCacheTable();
		}

		/// <summary>
		/// Constructor for creating an existing node object without properties.
		/// </summary>
		/// <param name="collection">The collection that this node belongs to.</param>
		/// <param name="name">This is the name that is used by applications to describe the node.</param>
		/// <param name="id">The globally unique identifier for this node.</param>
		/// <param name="type">Type of node to create.</param>
		/// <param name="persisted">Set to true if this is a node that already exists in the database, otherwise false.</param>
		internal Node( Collection collection, string name, string id, string type, bool persisted )
		{
			store = collection.LocalStore;

			cNode = new CacheNode( store, id.ToLower() );
			cNode.collection = collection;
			cNode.name = name;
			cNode.type = type;
			cNode.isPersisted = persisted;
			cNode = cNode.AddToCacheTable();
		}

		/// <summary>
		/// Constructor for creating an existing node object where the collection has not been
		/// constructed yet.
		/// NOTE: The collection member must be set after construction of this object and the cNode object must be added
		/// to the cacheTable.
		/// </summary>
		/// <param name="store">Object that represents the local store.</param>
		/// <param name="xmlProperties">List of properties that belong to this node.</param>
		internal Node( Store store, XmlElement xmlProperties )
		{
			this.store = store;

			cNode = new CacheNode( store, xmlProperties.GetAttribute( Property.IDAttr ) );
			cNode.name = xmlProperties.GetAttribute( Property.NameAttr );
			cNode.type = xmlProperties.GetAttribute( Property.TypeAttr );
			cNode.properties = new PropertyList( this, xmlProperties );
			cNode.isPersisted = true;
		}

		/// <summary>
		/// Constructor for creating a node from an imported xml document.  This node will be put onto the dirty list
		/// after it is constructed.
		/// </summary>
		/// <param name="collection">Collection that this new node will belong to.</param>
		/// <param name="xmlProperties">Xml document that describes the node.</param>
		/// <param name="persisted">Set to true if this is a node that already exists in the database, otherwise false.</param>
		/// <param name="imported">Set to true if this node is being imported from another location.</param>
		/// <param name="useCache">Set to true if the node cache is to be used.</param>
		internal Node( Collection collection, XmlElement xmlProperties, bool persisted, bool imported, bool useCache )
		{
			store = collection.LocalStore;

			cNode = new CacheNode( store, xmlProperties.GetAttribute( Property.IDAttr ) );
			cNode.collection = collection;
			cNode.name = xmlProperties.GetAttribute( Property.NameAttr );
			cNode.type = xmlProperties.GetAttribute( Property.TypeAttr );
			cNode.isPersisted = persisted;

			if ( imported )
			{
				XmlDocument nodeDocument = new XmlDocument();
				nodeDocument.AppendChild( nodeDocument.CreateElement( Property.ObjectListTag ) );
				nodeDocument.DocumentElement.AppendChild( nodeDocument.ImportNode( xmlProperties, true ) );
				cNode.properties = new PropertyList( this, nodeDocument.DocumentElement[ Property.ObjectTag ] );

				// See if this node already has a master and local version property.  If it doesn't then add them.
				Property mvProp = Properties.GetSingleProperty( Property.MasterIncarnation );
				if ( mvProp == null )
				{
					mvProp = new Property( Property.MasterIncarnation, ( ulong )0 );
					mvProp.LocalProperty = true;
					Properties.AddNodeProperty( mvProp );
				}

				Property lvProp = Properties.GetSingleProperty( Property.LocalIncarnation );
				if ( lvProp == null )
				{
					lvProp = new Property( Property.LocalIncarnation, ( ulong )0 );
					lvProp.LocalProperty = true;
					Properties.AddNodeProperty( lvProp );
				}

				// Because this node is being imported, it needs to be the one in the cache node table.
				// Otherwise all import changes will be lost.
				CacheNode tempCacheNode = store.GetCacheNode( Id );
				if ( tempCacheNode != null )
				{
					// Copy this cache node to the one in the table so that all node will see the import
					// changes.
					tempCacheNode.Copy( cNode );

					// GetCacheNode() incremented the reference count.  Need to decrement it.
					store.RemoveCacheNode( tempCacheNode, false );
				}

				// Add the node to the cache table.
				cNode = cNode.AddToCacheTable();

				// Add this node to the dirty list.
				collection.AddDirtyNodeToList( this );
			}
			else
			{
				// Create the property list.
				cNode.properties = new PropertyList( this, xmlProperties );

				if ( useCache )
				{
					// Add the node to the cache table.
					cNode = cNode.AddToCacheTable();
				}
			}
		}

		/// <summary>
		/// Constructor for creating a node from an existing cache node.
		/// </summary>
		/// <param name="cNode">Cache node that contains the node data.</param>
		/// <param name="incReference">Increments the reference count on cNode if true.</param>
		internal Node( CacheNode cNode, bool incReference )
		{
			lock ( cNode.store )
			{
				this.store = cNode.store;
				this.cNode = cNode;

				if ( incReference )
				{
					++cNode.referenceCount;
				}
			}
		}

		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// It gives your base class the opportunity to finalize.
		/// Do not provide destructors in types derived from this class.
		/// </summary>
		~Node()
		{
			if ( cNode != null )
			{
				cNode.Dispose();
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Changes a node into a tombstone.
		/// </summary>
		/// <param name="commit">If true, tombstone is committed to disk.</param>
		private void ChangeToTombstone( bool commit )
		{
			// Change the name to Tombstone.
			cNode.name = "Tombstone:" + cNode.name;

			// Change the type of node.
			cNode.type = TombstoneType;

			// Remove the current properties on the node.
			cNode.properties = new PropertyList( this );
			cNode.mergeList.Clear();

			// Create some default properties.
			Properties.AddNodeProperty( Property.CreationTime, DateTime.UtcNow );
			Properties.AddNodeProperty( Property.ModifyTime, DateTime.UtcNow );
			Properties.AddNodeProperty( Property.CollectionID, CollectionNode.Id );

			Property mvProp = new Property( Property.MasterIncarnation, ( ulong )0 );
			mvProp.LocalProperty = true;
			Properties.AddNodeProperty( mvProp );

			Property lvProp = new Property( Property.LocalIncarnation, ( ulong )0 );
			lvProp.LocalProperty = true;
			Properties.AddNodeProperty( lvProp );

			// Initialize the in-memory node fields.
			IsPersisted = true;

			if ( commit )
			{
				// Commit the node now.
				Commit();
			}

			// Remove this tombstone out of the cache table.
			store.RemoveCacheNode( cNode, true );
		}

		/// <summary>
		/// Gets a node directly from the database.
		/// </summary>
		/// <param name="nodeId">Identifier of node to get.</param>
		/// <returns>A node that contains the current object from the database.</returns>
		private Node GetNodeFromDatabase( string nodeId )
		{
			Node dbNode = null;

			// Get the current node from the database.
			string xmlString = store.StorageProvider.GetRecord( nodeId, CollectionNode.Id );
			if ( xmlString != null )
			{
				XmlDocument xmlNode = new XmlDocument();
				xmlNode.LoadXml( xmlString );
				dbNode = new Node( CollectionNode, xmlNode.DocumentElement[ Property.ObjectTag ], true, false, false );
			}

			return dbNode;
		}

		/// <summary>
		/// Gets a list of all nodes that contain this node's id in their ID path list.
		/// </summary>
		/// <returns>An ICSList object that contains a list of matching nodes.</returns>
		private ICSList GetIdPathList()
		{
			// Search for all nodes that contain the id of this node in its id path.
			return CollectionNode.Search( Property.IDPath, Id, Property.Operator.Contains );
		}

		/// <summary>
		/// Gets the name space prefix for the specified type.
		/// </summary>
		/// <param name="nodeType">Node type.</param>
		/// <returns>A string containing the name space prefix.</returns>
		private string GetNameSpacePrefix( string nodeType )
		{
			string prefix = null;
			int index = nodeType.IndexOf( '.' );
			if ( index != -1 )
			{
				prefix = nodeType.Remove( index + 1, nodeType.Length - ( index + 1 ) );
			}

			return prefix;
		}

		/// <summary>
		/// Creates a node from an XML document that doesn't have any instantiated property information.
		/// </summary>
		/// <param name="collection">The collection that the XML described node belongs to.</param>
		/// <param name="xmlNode">The XML node that describes this node.</param>
		/// <returns>Node object</returns>
		private Node GetShallowNode( Collection collection, XmlNode xmlNode )
		{
			// Get the type of Node, so we can create the right type.
			string nodeType = xmlNode.Attributes[ Property.TypeAttr ].Value;
			string prefix = GetNameSpacePrefix( nodeType );
			if ( prefix != null )
			{
				switch ( prefix )
				{
					case CollectionType:
						return new Collection( store, xmlNode.Attributes[ Property.NameAttr ].Value, xmlNode.Attributes[ Property.IDAttr ].Value, nodeType );

					case NodeType:
						return new Node( collection, xmlNode.Attributes[ Property.NameAttr ].Value, xmlNode.Attributes[ Property.IDAttr ].Value, nodeType, true );

					case TombstoneType:
						return new Node( collection, xmlNode.Attributes[ Property.NameAttr ].Value, xmlNode.Attributes[ Property.IDAttr ].Value, nodeType, true );

					default:
						throw new ApplicationException( "Invalid namespace type" );
				}
			}
			else
			{
				throw new ApplicationException( "Invalid namespace type" );
			}
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Increments the local incarnation property.
		/// NOTE: The store mutex must be held before making this call and must continue to be held until
		/// this node has been committed to disk.
		/// </summary>
		/// <returns>The new local incarnation value.</returns>
		internal ulong IncrementLocalIncarnation()
		{
			if ( !IsTombstone )
			{
				// Increment the property value.
				ulong incarnationValue = LocalIncarnation;
				Properties.ModifyNodeProperty( Property.LocalIncarnation, ++incarnationValue );
				return incarnationValue;
			}
			else
			{
				return UInt64.MaxValue;
			}
		}

		/// <summary>
		/// Merges all property changes on the current node with the current object in the database.
		/// 
		/// Note: The database lock must be acquired before making this call.
		/// </summary>
		/// <param name="clearMergeList">If true then the node's merge list will be cleared.</param>
		/// <returns>A node that contains the current object from the database with all of the property
		/// changes of the current node.</returns>
		internal Node MergeNodeProperties( bool clearMergeList )
		{
			// Get this node from the database.
			Node mergedNode = GetNodeFromDatabase( Id );
			if ( mergedNode != null )
			{
				// If this node is not a tombstone and the merged node is, then the node has been deleted and delete wins.
				if ( !IsTombstone && mergedNode.IsTombstone )
				{
					mergedNode = null;
				}
				else if ( IsTombstone && !mergedNode.IsTombstone )
				{
					// If this node is a tombstone and the merged node is not, then delete wins again and the merged node
					// will be turned into a tombstone.
					mergedNode = this;
				}
				else
				{
					// If this node is a tombstone and the merged node is a tombstone, then merge the changes or
					// if this node is not a tombstone and the merged node is not a tombstone, merge the changes.
					// Walk the merge list and perform the changes specified there to the mergedNode.
					foreach ( Property p in cNode.mergeList )
					{
						p.ApplyMergeInformation( mergedNode );
					}
				}
			}

			if ( clearMergeList )
			{
				// Clear the mergeList.
				cNode.mergeList.Clear();
			}

			return mergedNode;
		}

		/// <summary>
		/// Converts the current node to an XML text representation.
		/// </summary>
		/// <param name="xmlNodeDoc">Xml document where node(s) are to be added.</param>
		/// <param name="node">Collection node to add.</param>
		/// <param name="deep">If true then all child node(s) are also added to the destination document.</param>
		internal void NodeToXml( XmlDocument xmlNodeDoc, Node node, bool deep )
		{
			if ( deep )
			{
				// Get a list of all nodes that contain this node's id in its ID list.
				ICSList idList = node.GetIdPathList();
				foreach ( Node tempNode in idList )
				{
					// Add the node and all of its properties.
					XmlNode child = xmlNodeDoc.ImportNode( tempNode.Properties.PropertyRoot, true );
					xmlNodeDoc.DocumentElement.AppendChild( child );
				}
			}
			else
			{
				// Import this child node into the destination document.
				XmlNode child = xmlNodeDoc.ImportNode( node.Properties.PropertyRoot, true );
				xmlNodeDoc.DocumentElement.AppendChild( child );
			}

			// Strip out any non-transient values.
			XmlNodeList nonTransList = xmlNodeDoc.DocumentElement.SelectNodes( "//Property[@flags]" );
			foreach( XmlNode tempNode in nonTransList )
			{
				uint flags = Convert.ToUInt32( tempNode.Attributes[ Property.FlagsAttr ].Value );
				if ( ( flags & Property.Local ) == Property.Local )
				{
					// Remove this property out of the document.
					tempNode.ParentNode.RemoveChild( tempNode );
				}
			}
		}

		/// <summary>
		/// Rolls back changes made to the last time the node was committed.  If the node has never been committed,
		/// no changes are made.
		/// </summary>
		internal void RollbackNode()
		{
			if ( IsPersisted )
			{
				SetNodeProperties();
				Name = Properties.PropertyRoot.GetAttribute( Property.NameAttr );
			}
		}

		/// <summary>
		/// Sets the specified attribute on the xml node.  This guarantees attribute order.
		/// </summary>
		/// <param name="xmlNode">Node to set the attribute on.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="attributeValue">Attribute value.</param>
		internal void SetNodeAttribute( XmlNode xmlNode, string attributeName, string attributeValue )
		{
			// Create the attribute.
			XmlAttribute xmlAttr = xmlNode.OwnerDocument.CreateAttribute( attributeName );
			xmlAttr.Value = attributeValue;

			// Need to guarantee attribute order on the node.
			switch ( attributeName )
			{
				case Property.NameAttr:
					xmlNode.Attributes.Prepend( xmlAttr );
					break;

				case Property.IDAttr:
					xmlNode.Attributes.InsertAfter( xmlAttr, xmlNode.Attributes[ Property.NameAttr ] );
					break;

				case Property.TypeAttr:
					xmlNode.Attributes.InsertAfter( xmlAttr, xmlNode.Attributes[ Property.IDAttr ] );
					break;
			}
		}

		/// <summary>
		/// Gets the properties for this node and instaniates a PropertyList.
		/// </summary>
		internal void SetNodeProperties()
		{
			// Call the provider to get an XML string that represents this node.
			string xmlNode = store.StorageProvider.GetRecord( Id, CollectionNode.Id );
			if ( xmlNode != null )
			{
				// Covert the XML string into a DOM that we can then parse.
				XmlDocument nodeDoc = new XmlDocument();
				nodeDoc.LoadXml( xmlNode );
				cNode.properties = new PropertyList( this, nodeDoc.DocumentElement[ Property.ObjectTag ] );
				cNode.mergeList.Clear();

				// If this node is a collection object, the owner needs to be set since this is the first
				// time that the properties have been asked for.
				if ( IsCollection )
				{
					CollectionNode.UpdateAccessControl();
				}
			}
			else
			{
				throw new ApplicationException( "No Properties exist for Node" );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds a stream node to this node.
		/// </summary>
		/// <param name="name">Friendly name of the stream node.</param>
		/// <param name="relativePath">Path to the stream that is relative to the collection document root.</param>
		/// <returns>A Stream object representing a stream in the file system.</returns>
		[ Obsolete( "This method is marked for removal. Use AddFileEntry() instead.", false ) ]
		public NodeStream AddStream( string name, string relativePath )
		{
			return AddStream( name, Generic, relativePath );
		}

		/// <summary>
		/// Adds a stream node to this node.
		/// </summary>
		/// <param name="name">Friendly name of the stream node.</param>
		/// <param name="type">Type of stream node.</param>
		/// <param name="relativePath">Path to the stream that is relative to the collection document root.</param>
		/// <returns>A Stream object representing a stream in the file system.</returns>
		[ Obsolete( "This method is marked for removal. Use AddFileEntry() instead.", false ) ]
		public NodeStream AddStream( string name, string type, string relativePath )
		{
			lock ( store )
			{
				return new NodeStream( this, name, type, relativePath );
			}
		}

		/// <summary>
		/// Adds a file to this node.
		/// </summary>
		/// <param name="name">Display name of the file.</param>
		/// <param name="relativePath">Path to the file that is relative to the collection document root.</param>
		/// <returns>A FileEntry object representing the file in the file system.</returns>
		public FileEntry AddFileEntry( string name, string relativePath )
		{
			return AddFileEntry( name, Generic, relativePath );
		}

		/// <summary>
		/// Adds a file to this node.
		/// </summary>
		/// <param name="name">Display name of the file.</param>
		/// <param name="type">Type of file.</param>
		/// <param name="relativePath">Path to the file that is relative to the collection document root.</param>
		/// <returns>A FileEntry object representing the file in the file system.</returns>
		public FileEntry AddFileEntry( string name, string type, string relativePath )
		{
			lock ( store )
			{
				return new FileEntry( this, name, type, relativePath );
			}
		}

		/// <summary>
		/// Adds a directory to this node.
		/// </summary>
		/// <param name="name">Display name of the directory.</param>
		/// <param name="relativePath">Path to the directory that is relative to the collection document root.</param>
		/// <returns>A DirectoryEntry object representing the directory in the file system.</returns>
		public DirectoryEntry AddDirectoryEntry( string name, string relativePath )
		{
			return AddDirectoryEntry( name, Generic, relativePath );
		}

		/// <summary>
		/// Adds a directory to this node.
		/// </summary>
		/// <param name="name">Display name of the directory.</param>
		/// <param name="type">Type of directory.</param>
		/// <param name="relativePath">Path to the directory that is relative to the collection document root.</param>
		/// <returns>A DirectoryEntry object representing the directory in the file system.</returns>
		public DirectoryEntry AddDirectoryEntry( string name, string type, string relativePath )
		{
			lock ( store )
			{
				return new DirectoryEntry( this, name, type, relativePath );
			}
		}

		/// <summary>
		/// Commits all changes in the node to persistent storage. After a node has been committed, it
		/// will be updated to reflect any new changes that occurred if it had to be merged with the current
		/// node in the database.
		/// </summary>
		public void Commit()
		{
			lock ( store )
			{
				Node commitNode = null;

				// Make sure that current user has write rights to this collection.
				if ( !CollectionNode.IsAccessAllowed( Access.Rights.ReadWrite ) )
				{
					throw new UnauthorizedAccessException( "Current user does not have collection modify right." );
				}

				try
				{
					// Acquire the store lock.
					store.LockStore();

					// If this node has not been persisted, no need to do a merge.
					commitNode = IsPersisted ? MergeNodeProperties( true ) : this;
					if ( commitNode != null )
					{
						// Set the modify time for this node.
						commitNode.Properties.ModifyNodeProperty( "ModifyTime", DateTime.UtcNow );

						// Increment the local incarnation number on the local node .
						commitNode.IncrementLocalIncarnation();

						// If this is a new collection, create it.
						if ( !commitNode.IsPersisted && commitNode.IsCollection )
						{
							store.StorageProvider.CreateCollection( Id );
						}

						// Call the store provider to update the records.
						store.StorageProvider.CreateRecord( commitNode.Properties.PropertyDocument.OuterXml, CollectionNode.Id );
					}
				}
				finally
				{
					// Release the store lock.
					store.UnlockStore();
				}

				// Remove this node from the collection's dirty list.  Don't add any new properties
				// after this or it will go back onto the dirty list.
				CollectionNode.RemoveDirtyNodeFromList( Id );

				// Make sure that the node was written to the database.
				if ( commitNode != null )
				{
					// Fire an event for this commit action.
					if ( IsPersisted )
					{
						// Update this node to reflect the latest changes.
						cNode.Copy( commitNode.cNode );

						// Fire an event to notify that this node has been changed.
						store.Publisher.RaiseEvent( new NodeEventArgs( store.ComponentId, Id, CollectionNode.Id, NameSpaceType, EventType.NodeChanged, store.Instance ) );
					}
					else
					{
						// Fire an event to notify that this node has been created.
						store.Publisher.RaiseEvent( new NodeEventArgs( store.ComponentId, Id, CollectionNode.Id, NameSpaceType, EventType.NodeCreated, store.Instance ) );

						// This node has been successfully committed to the database.
						IsPersisted = true;
					}

					if ( IsCollection )
					{
						// Update the access control list.
						CollectionNode.UpdateAccessControl();
					}
					else
					{
						// Need to update the incarnation number on the collection.  Can do this just by committing
						// the collection object.
						CollectionNode.Commit();
					}
				}
			}
		}

		/// <summary>
		/// Creates a node subordinate to this node that is of type NodeType.
		/// </summary>
		/// <param name="name">This is the name that is used by applications to describe the node.</param>
		public Node CreateChild( string name )
		{
			return CreateChild( name, Generic );
		}

		/// <summary>
		/// Creates a node subordinate to this node.
		/// </summary>
		/// <param name="name">This is the name that is used by applications to describe the node.</param>
		/// <param name="type">Type of node to create.</param>
		public Node CreateChild( string name, string type )
		{
			lock ( store )
			{
				// Create the new child node.
				Node child = new Node( CollectionNode, name, Guid.NewGuid().ToString(), type );
				return child.SetParent( this );
			}
		}

		/// <summary>
		/// Deletes the specified node from the persistent store.  If there are nodes
		/// subordinate to this node, an exception will be thrown.
		/// </summary>
		/// <return>Returns the node as a tombstone object.</return>
		public Node Delete()
		{
			return Delete( false );
		}

		/// <summary>
		/// Deletes the specified node from the persistent store.
		/// </summary>
		/// <param name="deep">Indicates whether to all children nodes of this node are deleted also.</param>
		/// <returns>Returns the node as a tombstone object.</returns>
		public Node Delete( bool deep )
		{
			lock ( store )
			{
				// No sense in deleting a node that has not been persisted.
				if ( IsPersisted )
				{
					// Only node deletes are access checked.
					if ( !IsCollection )
					{
						if ( !CollectionNode.IsAccessAllowed( Access.Rights.ReadWrite ) )
						{
							throw new UnauthorizedAccessException( "Current user does not have collection modify right." );
						}
					}

					if ( deep )
					{
						if ( IsCollection )
						{
							// Generate a delete event.
							store.Publisher.RaiseEvent( new NodeEventArgs( store.ComponentId, Id, CollectionNode.Id, NameSpaceType, EventType.NodeDeleted, store.Instance ) );

							// Just delete the collection, the store provider will remove all of the nodes.
							ChangeToTombstone( false );
							store.StorageProvider.DeleteCollection( Id );
							CollectionNode.ClearDirtyList();
						}
						else
						{
							if ( !IsTombstone )
							{
								// Build a node list for this node and all of its children and then turn each
								// of them into a tombstone.
								ICSList idList = GetIdPathList();
								foreach ( Node delNode in idList )
								{
									// Generate a delete event.
									store.Publisher.RaiseEvent( new NodeEventArgs( store.ComponentId, delNode.Id, CollectionNode.Id, delNode.NameSpaceType, EventType.NodeDeleted, store.Instance ) );

									// Change the current this object into a tombstone rather than using the
									// enumerated object.  That way the tombstone will be passed back to the caller.
									if ( delNode.Id == Id )
									{
										ChangeToTombstone( true );
									}
									else
									{
										delNode.ChangeToTombstone( true );
									}
								}
							}
							else
							{
								// A Tombstone has no children.  Therefore just delete it from the database.
								store.StorageProvider.DeleteRecord( Id, CollectionNode.Id );

								// Remove this object from the commitList.
								CollectionNode.RemoveDirtyNodeFromList( Id );
							}
						}
					}
					else
					{
						if ( !IsTombstone )
						{
							if ( HasChildren )
							{
								throw new ApplicationException( "Node has children" );	
							}

							if ( IsCollection )
							{
								// Generate a delete event.
								store.Publisher.RaiseEvent( new NodeEventArgs( store.ComponentId, Id, CollectionNode.Id, NameSpaceType, EventType.NodeDeleted, store.Instance ) );

								// Find the node object and delete it from the persistent store.
								ChangeToTombstone( false );
								store.StorageProvider.DeleteRecord( Id, CollectionNode.Id );
							}
							else
							{
								// Generate a delete event.
								store.Publisher.RaiseEvent( new NodeEventArgs( store.ComponentId, Id, CollectionNode.Id, NameSpaceType, EventType.NodeDeleted, store.Instance ) );

								// Convert this node to a tombstone and immediately commit it.
								ChangeToTombstone( true );
							}
						}
						else
						{
							// Find the node object and delete it from the persistent store.
							store.StorageProvider.DeleteRecord( Id, CollectionNode.Id );
						}

						// Remove this object from the commitList.
						CollectionNode.RemoveDirtyNodeFromList( Id );
					}
				}
				else
				{
					// Convert the nodes to a tombstone.
					ChangeToTombstone( false );
					if ( IsCollection )
					{
						// This is a collection all changed nodes should be released.
						CollectionNode.ClearDirtyList();
					}
					else
					{
						// Remove this object from the commitList.
						CollectionNode.RemoveDirtyNodeFromList( Id );
					}
				}

				return this;
			}
		}

		/// <summary>
		/// Gets all file system entry objects that match the specified name.
		/// </summary>
		/// <param name="name">File system entry name.</param>
		/// <returns>An ICSList object containing a list of file system entry objects that matched the
		/// specified name.</returns>
		public ICSList GetFileSystemEntriesByName( string name )
		{
			lock ( store )
			{
				ICSList fseList = new ICSList();
				foreach( FileSystemEntry fse in GetFileSystemEntryList() )
				{
					if ( fse.Name == name )
					{
						fseList.Add( fse );
					}
				}

				return fseList;
			}
		}

		/// <summary>
		/// Gets all file system entry objects that match the specified type.
		/// </summary>
		/// <param name="type">File system entry type.</param>
		/// <returns>An ICSList object containing a list of file system entry objects that matched the
		/// specified type.</returns>
		public ICSList GetFileSystemEntriesByType( string type )
		{
			lock ( store )
			{
				ICSList fseList = new ICSList();
				foreach( FileSystemEntry fse in GetFileSystemEntryList() )
				{
					if ( fse.Type == type )
					{
						fseList.Add( fse );
					}
				}

				return fseList;
			}
		}

		/// <summary>
		/// Gets the specified file system entry object by its ID.
		/// </summary>
		/// <param name="id">Entry ID.</param>
		/// <returns>A FileSystemEntry object that represents the specified ID.</returns>
		public FileSystemEntry GetFileSystemEntryById( string id )
		{
			lock ( store )
			{
				string lowerId = id.ToLower();
				ICSList entryList = GetFileSystemEntryList();
				foreach( FileSystemEntry fse in entryList )
				{
					if ( fse.Id == lowerId )
					{
						return fse;
					}
				}

				return null;
			}
		}

		/// <summary>
		/// Gets a list of the file system entries that belong to this node.
		/// </summary>
		/// <returns>An ICSList object that contains the file system entry objects.</returns>
		public ICSList GetFileSystemEntryList()
		{
			lock ( store )
			{
				return new ICSList( new FileSystemEntry.FileSystemEntryEnumerator( this ) );
			}
		}

		/// <summary>
		/// Retrieves a node from the persistent store and converts it back into an in memory node object.
		/// </summary>
		/// <param name="nodeId">Identifier uniquely naming the node.</param>
		/// <returns>Node object</returns>
		public Node GetNodeById( string nodeId )
		{
			lock ( store )
			{
				Node node = null;
				string normalizedId = nodeId.ToLower();

				// See if there is already an instance of this data in the cache table.
				CacheNode cNode = store.GetCacheNode( normalizedId );
				if ( cNode == null )
				{
					// Call the provider to get an XML string that represents this node.
					string xmlNode = store.StorageProvider.GetRecord( normalizedId, CollectionNode.Id );
					if ( xmlNode != null )
					{
						// Covert the XML string into a DOM that we can then parse.
						XmlDocument nodeDoc = new XmlDocument();
						nodeDoc.LoadXml( xmlNode );

						// Do not allow retrieval of nodes outside of this collection.
						XmlNode propNode = nodeDoc.DocumentElement.FirstChild.SelectSingleNode( Property.PropertyTag + "[@" + Property.NameAttr + "='" + Property.CollectionID + "']" );
						if ( ( propNode != null ) && ( propNode.InnerText == CollectionNode.Id ) )
						{
							// Point to the node tag.
							XmlElement element = nodeDoc.DocumentElement[ Property.ObjectTag ];

							// Get the type of Node, so we can create the right type.
							string nodeType = element.GetAttribute( Property.TypeAttr );
							string prefix = GetNameSpacePrefix( nodeType );
							if ( prefix != null )
							{
								switch ( prefix )
								{
									case CollectionType:
										node = new Collection( store, element, false );
										break;

									case NodeType:
										node = new Node( CollectionNode, element, true, false, true );
										break;

									case TombstoneType:
										node = new Node( CollectionNode, element, true, false, true );
										break;

									default:
										throw new ApplicationException( "Invalid namespace type" );
								}
							}
							else
							{
								throw new ApplicationException( "Invalid namespace type" );
							}
						}
						else
						{
							throw new ApplicationException( "Specified ID does not belong to this Collection" );
						}
					}
				}
				else
				{
					// An instance of the data already exists.
					switch ( GetNameSpacePrefix( cNode.type ) )
					{
						case CollectionType:
							node = new Collection( store, cNode, false );
							break;

						case NodeType:
							node = new Node( cNode, false );
							break;

						case TombstoneType:
							node = new Node( cNode, false );
							break;

						default:
							throw new ApplicationException( "Invalid namespace type" );
					}
				}

				return node;
			}
		}

		/// <summary>
		/// Gets all nodes that have the specified name.
		/// </summary>
		/// <param name="name">A string containing the name for the node(s).</param>
		/// <returns>An ICSList object containing the node(s) that that have the specified name.</returns>
		public ICSList GetNodesByName( string name )
		{
			lock ( store )
			{
				return CollectionNode.Search( Property.ObjectName, name, Property.Operator.Equal );
			}
		}

		/// <summary>
		/// Gets all nodes that correspond to the specified node path name.
		/// </summary>
		/// <param name="pathName">A string containing a node path name for the node(s). The path name for a node 
		/// consists of the hierarchy of names from the collection to the leaf node.</param>
		/// <returns>An ICSList object containing the node(s) that correspond to the specified path name.</returns>
		public ICSList GetNodesByPathName( string pathName )
		{
			lock ( store )
			{
				ICSList nodeList = new ICSList();
				string leafName = pathName.Substring( pathName.LastIndexOf( '/' ) + 1 );

				// Find all nodes in this collection with this leaf name.
				ICSList tempList = GetNodesByName( leafName );
				foreach ( Node tempNode in tempList )
				{
					if ( pathName == tempNode.PathName )
					{
						nodeList.Add( tempNode );
					}
				}

				return nodeList;
			}
		}

		/// <summary>
		/// Gets all nodes that have the specified type.
		/// </summary>
		/// <param name="type">A string containing the type for the node(s).</param>
		/// <returns>An ICSList object containing the node(s) that that have the specified type.</returns>
		public ICSList GetNodesByType( string type )
		{
			lock ( store )
			{
				return CollectionNode.Search( Property.ObjectType, NodeType + type , Property.Operator.Begins );
			}
		}

		/// <summary>
		/// Returns the parent for this node.
		/// </summary>
		/// <returns>Node that represents the parent of this node.  Null is return if node has no parent.</returns>
		public Node GetParent()
		{
			lock ( store )
			{
				Property p = Properties.GetSingleProperty( Property.ParentID );
				if ( p != null )
				{
					return GetNodeById( p.ToString() );
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Gets the first file system entry object that matches the specified name.
		/// </summary>
		/// <param name="name">File system entry name.</param>
		/// <returns>A FileSystemEntry object that represents the specified name.</returns>
		public FileSystemEntry GetSingleFileSystemEntryByName( string name )
		{
			lock ( store )
			{
				ICSList entryList = GetFileSystemEntryList();
				foreach( FileSystemEntry fse in entryList )
				{
					if ( fse.Name == name )
					{
						return fse;
					}
				}

				return null;
			}
		}

		/// <summary>
		/// Gets the first file system entry object that matches the specified type.
		/// </summary>
		/// <param name="type">File system entry type.</param>
		/// <returns>A FileSystemEntry object that represents the specified type.</returns>
		public FileSystemEntry GetSingleFileSystemEntryByType( string type )
		{
			lock ( store )
			{
				ICSList entryList = GetFileSystemEntryList();
				foreach( FileSystemEntry fse in entryList )
				{
					if ( fse.Type == type )
					{
						return fse;
					}
				}

				return null;
			}
		}

		/// <summary>
		/// Gets the first node that matches the specified name.
		/// </summary>
		/// <param name="name">A string containing the name for the node.</param>
		/// <returns>The first node object that matches the specified name.  A null is returned if no matching nodes are found.</returns>
		public Node GetSingleNodeByName( string name )
		{
			lock ( store )
			{
				Node node = null;
				ICSList nodeList = CollectionNode.Search( Property.ObjectName, name, Property.Operator.Equal );
				foreach ( Node tempNode in nodeList )
				{
					node = tempNode;
					break;
				}

				return node;
			}
		}

		/// <summary>
		/// Gets the first node that corresponds to the specified node path name.
		/// </summary>
		/// <param name="pathName">A string containing the node path name for a node.  The path name for a node 
		/// consists of the hierarchy of names from the collection to the leaf node.</param>
		/// <returns>The first node that corresponds to the specified node path name.  A null is returned if no matching nodes are found.</returns>
		public Node GetSingleNodeByPathName( string pathName )
		{
			lock ( store )
			{
				Node node = null;

				// Find at least one node that corresponds to the node path name.
				ICSList nodeList = GetNodesByPathName( pathName );
				foreach ( Node tempNode in nodeList )
				{
					node = tempNode;
					break;
				}

				return node;
			}
		}

		/// <summary>
		///  Gets the first node that corresponds to the specified node type.
		/// </summary>
		/// <param name="type">String that contains the type of the node.</param>
		/// <returns>The first node that corresponds to the specified node path name.  A null is returned if no matching nodes are found.</returns>
		public Node GetSingleNodeByType( string type )
		{
			lock ( store )
			{
				Node node = null;

				// Find at least one node that corresponds to the node type.
				ICSList nodeList = GetNodesByType( type );
				foreach ( Node tempNode in nodeList )
				{
					node = tempNode;
					break;
				}

				return node;
			}
		}

		/// <summary>
		/// Gets the specified stream object by its ID.
		/// </summary>
		/// <param name="id">Stream ID.</param>
		/// <returns>A NodeStream object that represents the specified ID.</returns>
		[ Obsolete( "This method is marked for removal. Use GetFileSystemEntryById() instead.", false ) ]
		public NodeStream GetStreamById( string id )
		{
			lock ( store )
			{
				string lowerId = id.ToLower();
				ICSList streamList = GetStreamList();
				foreach( NodeStream ns in streamList )
				{
					if ( ns.Id == lowerId )
					{
						return ns;
					}
				}

				return null;
			}
		}

		/// <summary>
		/// Gets a list of the streams that belong to this node.
		/// </summary>
		/// <returns>An ICSList object that contains the stream objects.</returns>
		[ Obsolete( "This method is marked for removal. Use GetFileSystemEntryList() instead.", false ) ]
		public ICSList GetStreamList()
		{
			lock ( store )
			{
				return new ICSList( new NodeStream.NodeStreamEnumerator( this ) );
			}
		}

		/// <summary>
		/// Changes the parent of this node to point to a new parent node.
		/// </summary>
		/// <param name="newParent">New parent node.</param>
		public void Move( Node newParent )
		{
			lock ( store )
			{
				// This node cannot be a collection object.
				if ( IsCollection )
				{
					throw new ApplicationException( "Cannot move collections or collections cannot have parents." );
				}

				// Make sure that this node is in the same collection.
				if ( newParent.CollectionNode.Id != CollectionNode.Id )
				{
					throw new ApplicationException( "Nodes must exist in the same collection" );
				}

				if ( IsPersisted )
				{
					// If the new parent is the same as the existing parent, do nothing.
					Property p = Properties.GetSingleProperty( Property.ParentID );
					if ( ( p == null ) || ( p.ToString() != newParent.Id ) )
					{
						// Get a list of all children nodes that contain this id in its ID path.
						ICSList idList = GetIdPathList();
						foreach ( Node tempNode in idList )
						{
							// Get the ID path property for this subordinate node.
							p = tempNode.Properties.GetSingleProperty( Property.IDPath );
							if ( p != null )
							{
								// Strip off the parent of the current node and add the new parent ID path.
								string idPath = p.ToString();
								p.SetPropertyValue( newParent.IDPath + "/" + idPath.Substring( idPath.IndexOf( Id ) ) );
							}
							else
							{
								throw new ApplicationException( "Node does not contain an ID path" );
							}
						}

						// Set the parent Id of this node to point to the new parent.
						Properties.ModifyNodeProperty( Property.ParentID, newParent.Id );
					}
				}
				else
				{
					Properties.AddNodeProperty( Property.ParentID, newParent.Id );
					Properties.AddNodeProperty( Property.IDPath, newParent.IDPath + "/" + Id );
				}
			}
		}

		/// <summary>
		/// Refreshes this node from the disk and merges all pending changes to this node.
		/// </summary>
		public void Refresh()
		{
			lock ( store )
			{
				Node node = MergeNodeProperties( false );
				if ( node != null )
				{
					cNode.Copy( node.cNode );
				}
			}
		}

		/// <summary>
		/// Rolls back changes made to the last time the node was committed.  If the node has never been committed,
		/// it is just removed from the transaction list.
		/// </summary>
		public void Rollback()
		{
			lock ( store )
			{
				RollbackNode();
				CollectionNode.RemoveDirtyNodeFromList( Id );
			}
		}

		/// <summary>
		/// Sets the current node as a child of the specified parent node.
		/// </summary>
		/// <param name="parentNode">Node to reference as the parent.</param>
		/// <returns>Node where parent was set.</returns>
		public Node SetParent( Node parentNode )
		{
			lock ( store )
			{
				Move( parentNode );
				return this;
			}
		}

		/// <summary>
		/// Updates the master and local incarnation properties on this node.
		/// Note: This operation performs a commit on this node before returning.  The incarnation numbers will be
		/// updated immediately along with any other changes made to the node.  The collection incarnation numbers
		/// will not be updated.
		/// </summary>
		/// <param name="master">New master incarnation to be set on the node.</param>
		/// <returns>True if incarnation value was updated, otherwise false is returned indicating a collision.</returns>
		public bool UpdateIncarnation( ulong master )
		{
			lock ( store )
			{
				bool updated = false;

				// Only the synker role has rights to make this call.
				if ( store.CurrentUser != Access.SyncOperatorRole )
				{
					throw new UnauthorizedAccessException( "Current user does not have collection synchronization right." );
				}

				try
				{
					// Acquire the store lock.
					store.LockStore();

					// See if the node has been updated since the last time we checked.
					Node currentNode = GetNodeFromDatabase( Id );
					if ( ( currentNode == null ) || ( currentNode.LocalIncarnation == LocalIncarnation ) )
					{
						// Update both incarnation values to the specified value.
						Properties.ModifyNodeProperty( Property.MasterIncarnation, master );
						Properties.ModifyNodeProperty( Property.LocalIncarnation, master );

						// If this is a new collection, create it.
						if ( !IsPersisted && IsCollection )
						{
							store.StorageProvider.CreateCollection( Id );
						}

						// Call the store provider to update the records.
						store.StorageProvider.CreateRecord( Properties.PropertyDocument.OuterXml, CollectionNode.Id );

						// Node has been updated.
						updated = true;
					}
				}
				finally
				{
					// Release the store lock.
					store.UnlockStore();
				}

				// If the node has been updated, a little more processing outside of the store lock is needed.
				if ( updated )
				{
					// Remove this node from the collection's dirty list.  Don't add any new properties
					// after this or it will go back onto the dirty list.
					CollectionNode.RemoveDirtyNodeFromList( Id );

					// This node has been successfully committed to the database.
					IsPersisted = true;

					if ( IsCollection )
					{
						// Update the access control list.
						CollectionNode.UpdateAccessControl();
					}
				}

				return updated;
			}
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Mandatory method used by clients to enumerate child node objects.
		/// </summary>
		/// <remarks>
		/// The client must call Dispose() to free up system resources before releasing
		/// the reference to the ICSEnumerator.
		/// </remarks>
		/// <returns>IEnumerator object used to enumerate nodes within nodes.</returns>
		public IEnumerator GetEnumerator()
		{
			// Get an iterator from the storage provider that will return all children of this node.
			return new NodeEnumerator( this, new Property( Property.ParentID, Id ), Persist.Query.Operator.Equal );
		}

		/// <summary>
		/// Enumerator class for the node object that allows enumeration of specified node objects
		/// within the collection.
		/// </summary>
		protected class NodeEnumerator : ICSEnumerator
		{
			#region Class Members
			/// <summary>
			/// The allocation size of the results array.
			/// </summary>
			private const int resultsArraySize = 4096;

			/// <summary>
			/// The initial size of the hash table used to filter duplicate nodes.
			/// </summary>
			private const int initialFilterNodesSize = 64;

			/// <summary>
			/// Indicates whether this object has been disposed.
			/// </summary>
			private bool disposed = false;

			/// <summary>
			/// Property containing the data to search for.
			/// </summary>
			private Property property;

			/// <summary>
			/// Type of search operation.
			/// </summary>
			private Persist.Query.Operator queryOperator;

			/// <summary>
			/// List of child nodes that exist under this node.
			/// </summary>
			private XmlDocument nodeList;

			/// <summary>
			/// Enumerator used to enumerate each returned item in the chunk enumerator list.
			/// </summary>
			private IEnumerator nodeListEnumerator;

			/// <summary>
			/// The internal enumerator to use to enumerate all of the child nodes belonging to this node.
			/// </summary>
			private Persist.IResultSet chunkIterator = null;

			/// <summary>
			/// The node that this enumerator belongs to.
			/// </summary>
			private Node node;

			/// <summary>
			/// Array where the query results are stored.
			/// </summary>
			private char[] results = new char[ resultsArraySize ];

			/// <summary>
			/// Hashtable used to filter out duplicate nodes returned by the chunkIterator.
			/// </summary>
			private Hashtable filterNodes;
			#endregion

			#region Constructor
			/// <summary>
			/// Constructor for the NodeEnumerator object.
			/// </summary>
			/// <param name="node">Node that this enumerator belongs to.</param>
			/// <param name="property">Property object containing the data to search for.</param>
			/// <param name="queryOperator">Query operator to use when comparing value.</param>
			public NodeEnumerator( Node node, Property property, Persist.Query.Operator queryOperator )
			{
				this.node = node;
				this.property = property;
				this.queryOperator = queryOperator;
				Reset();
			}
			#endregion

			#region Private Methods
			/// <summary>
			/// Determines if the specified ID already has been returned to the user during enumeration.
			/// </summary>
			/// <param name="nodeId">Node identifier to check for duplicity.</param>
			/// <returns>True if the ID is a duplicate, otherwise false.</returns>
			private bool IsDuplicate( string nodeId )
			{
				bool keyExists = filterNodes.ContainsKey( nodeId );
				if ( !keyExists )
				{
					filterNodes.Add( nodeId, null );
				}

				return keyExists;
			}
			#endregion

			#region IEnumerator Members
			/// <summary>
			/// Sets the enumerator to its initial position, which is before
			/// the first element in the collection.
			/// </summary>
			public void Reset()
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				// Create a new hashtable instance.
				filterNodes = new Hashtable( initialFilterNodesSize );

				// Release previously allocated chunkIterator.
				if ( chunkIterator != null )
				{
					chunkIterator.Dispose();
				}

				// Create a query object that will return a result set containing the children of this node.
				Persist.Query query = new Persist.Query( node.CollectionNode.Id, property.Name, queryOperator, property.ToString(), property.Type );
				chunkIterator = node.store.StorageProvider.Search( query );
				if ( chunkIterator != null )
				{
					// Get the first set of results from the query.
					int length = chunkIterator.GetNext( ref results );
					if ( length > 0 )
					{
						// Set up the XML document that we will use as the granular query to the client.
						nodeList = new XmlDocument();
						nodeList.LoadXml( new string( results, 0, length ) );
						nodeListEnumerator = nodeList.DocumentElement.GetEnumerator();
					}
					else
					{
						nodeListEnumerator = null;
					}
				}
				else
				{
					nodeListEnumerator = null;
				}
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public object Current
			{
				get
				{
					if ( disposed )
					{
						throw new ObjectDisposedException( this.ToString() );
					}

					if ( nodeListEnumerator == null )
					{
						throw new InvalidOperationException( "Empty enumeration" );
					}

					return node.GetShallowNode( node.CollectionNode, ( XmlNode )nodeListEnumerator.Current );
				}
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>
			/// true if the enumerator was successfully advanced to the next element; 
			/// false if the enumerator has passed the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				bool moreData;

				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				// Make sure that there is data in the list.
				if ( nodeListEnumerator != null )
				{
					// Prime the loop.
					bool duplicate = true;

					// There is a valid enumerator, assume there is more data.
					moreData = true;

					// Look for the next unique node in the enumeration list.
					while ( moreData && duplicate )
					{
						// See if there is anymore data left in this result set.
						moreData = nodeListEnumerator.MoveNext();
						if ( !moreData )
						{
							// Get the next page of the results set.
							int length = chunkIterator.GetNext( ref results );
							if ( length > 0 )
							{
								// Set up the XML document that we will use as the granular query to the client.
								nodeList = new XmlDocument();
								nodeList.LoadXml( new string( results, 0, length ) );
								nodeListEnumerator = nodeList.DocumentElement.GetEnumerator();

								// Move to the first entry in the document.
								moreData = nodeListEnumerator.MoveNext();
								if ( moreData )
								{
									// Filter out nodes that are duplicates.
									duplicate = IsDuplicate( ( ( XmlNode )nodeListEnumerator.Current ).Attributes[ Property.IDAttr ].Value );
								}
								else
								{
									// Out of data.
									nodeListEnumerator = null;
								}
							}
							else
							{
								// Out of data.
								nodeListEnumerator = null;
								moreData = false;
							}
						}
						else
						{
							// Filter out nodes that are duplicates.
							duplicate = IsDuplicate( ( ( XmlNode )nodeListEnumerator.Current ).Attributes[ Property.IDAttr ].Value );
						}
					}
				}
				else
				{
					moreData = false;
				}

				return moreData;
			}
			#endregion

			#region IDisposable Members
			/// <summary>
			/// Allows for quick release of managed and unmanaged resources.
			/// Called by applications.
			/// </summary>
			public void Dispose()
			{
				Dispose( true );
				GC.SuppressFinalize( this );
			}

			/// <summary>
			/// Dispose( bool disposing ) executes in two distinct scenarios.
			/// If disposing equals true, the method has been called directly
			/// or indirectly by a user's code. Managed and unmanaged resources
			/// can be disposed.
			/// If disposing equals false, the method has been called by the 
			/// runtime from inside the finalizer and you should not reference 
			/// other objects. Only unmanaged resources can be disposed.
			/// </summary>
			/// <param name="disposing">Specifies whether called from the finalizer or from the application.</param>
			protected virtual void Dispose( bool disposing )
			{
				// Check to see if Dispose has already been called.
				if ( !disposed )
				{
					// Set disposed here to protect callers from accessing freed members.
					disposed = true;

					// If disposing equals true, dispose all managed and unmanaged resources.
					if ( disposing )
					{
						// Dispose managed resources.
						if ( chunkIterator != null )
						{
							chunkIterator.Dispose();
						}
					}
				}
			}

			/// <summary>
			/// Use C# destructor syntax for finalization code.
			/// This destructor will run only if the Dispose method does not get called.
			/// It gives your base class the opportunity to finalize.
			/// Do not provide destructors in types derived from this class.
			/// </summary>
			~NodeEnumerator()      
			{
				Dispose( false );
			}
			#endregion
		}
		#endregion
	}
}
