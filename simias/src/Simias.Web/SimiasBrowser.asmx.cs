using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;

using Simias;
using Simias.Storage;

namespace Simias.Web
{
	/// <summary>
	/// Class that implements a node that can be XML serialized via the web service.
	/// </summary>
	public class BrowserNode
	{
		#region Class Members
		/// <summary>
		/// XML data that represents a Node object.
		/// </summary>
		private string nodeData;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the string containing XML data that represents a Node object.
		/// </summary>
		public string NodeData
		{
			get { return nodeData; }
			set { nodeData = value; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor required by the XML serialization.
		/// </summary>
		public BrowserNode()
		{
			// Just so its not null.
			this.nodeData = String.Empty;
		}

		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="nodeData">A string that contains XML data that represents a Node object.</param>
		public BrowserNode( string nodeData )
		{
			this.nodeData = nodeData;
		}
		#endregion
	}

	/// <summary>
	/// Class that implements a shallow node.
	/// </summary>
	public class BrowserShallowNode
	{
		#region Class Members

		private string name;
		private string id;
		private string type;
		private string cid;

		#endregion

		#region Properties

		/// <summary>
		/// Gets and sets the name of the shallow node.
		/// </summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		/// <summary>
		/// Gets and sets the ID of the shallow node.
		/// </summary>
		public string ID
		{
			get { return id; }
			set { id = value; }
		}

		/// <summary>
		/// Gets and sets the Type of the shallow node.
		/// </summary>
		public string Type
		{
			get { return type; }
			set { type = value; }
		}

		/// <summary>
		/// Gets and sets the collection ID of the shallow node.
		/// </summary>
		public string CID
		{
			get { return cid; }
			set { cid = value; }
		}

		#endregion

		#region Constructor

		public BrowserShallowNode()
		{
			this.name = String.Empty;
			this.id = String.Empty;
			this.type = String.Empty;
			this.cid = String.Empty;
		}

		public BrowserShallowNode( ShallowNode sn )
		{
			this.name = sn.Name;
			this.id = sn.ID;
			this.type = sn.Type;
			this.cid = sn.CollectionID;
		}

		#endregion
	}

	/// <summary>
	/// Summary description for Service1.
	/// </summary>
	[WebService(
		 Namespace="http://novell.com/simias/browser",
		 Name="Browser Service",
		 Description="Web Service providing access to the simias database.")]
	public class Browser : System.Web.Services.WebService
	{
		#region Class Members

		private string webServiceVersion = "1.1.0.0";

		#endregion

		#region WebMethods
		/// <summary>
		/// Returns a list of collections in the store.
		/// </summary>
		/// <returns>An array of BrowserNode objects.</returns>
		[ WebMethod(EnableSession = true) ]
		[ SoapDocumentMethod ]
		public BrowserNode[] EnumerateCollections()
		{
			ArrayList list = new ArrayList();
			Store store = Store.GetStore();
			
			foreach ( ShallowNode sn in store )
			{
				list.Add( new BrowserNode( new Collection( store, sn ).Properties.ToString() ) );
			}

			return list.ToArray( typeof( BrowserNode ) ) as BrowserNode[];
		}

		/// <summary>
		/// Enumerates the nodes in a collection.
		/// </summary>
		/// <param name="collectionID"></param>
		/// <returns></returns>
		[ WebMethod(EnableSession = true) ]
		[ SoapDocumentMethod ]
		public BrowserNode[] EnumerateNodes( string collectionID )
		{
			ArrayList list = new ArrayList();
			Store store = Store.GetStore();

			Collection c = store.GetCollectionByID( collectionID );
			if ( c != null )
			{
				foreach ( ShallowNode sn in c )
				{
					list.Add( new BrowserNode( new Node( c, sn ).Properties.ToString() ) );
				}
			}

			return list.ToArray( typeof( BrowserNode ) ) as BrowserNode[];
		}

		/// <summary>
		/// Gets a collection by its ID.
		/// </summary>
		/// <param name="collectionID"></param>
		/// <returns></returns>
		[ WebMethod(EnableSession = true) ]
		[ SoapDocumentMethod ]
		public BrowserNode GetCollectionByID( string collectionID )
		{
			BrowserNode bn = null;
			Store store = Store.GetStore();

			Collection c = store.GetCollectionByID( collectionID );
			if ( c != null )
			{
				bn = new BrowserNode( c.Properties.ToString() );
			}

			return bn;
		}

		/// <summary>
		/// Gets a node by its ID.
		/// </summary>
		/// <param name="collectionID"></param>
		/// <param name="nodeID"></param>
		/// <returns></returns>
		[ WebMethod(EnableSession = true) ]
		[ SoapDocumentMethod ]
		public BrowserNode GetNodeByID( string collectionID, string nodeID )
		{
			BrowserNode bn = null;
			Store store = Store.GetStore();

			Collection c = store.GetCollectionByID( collectionID );
			if ( c != null )
			{
				Node n = c.GetNodeByID( nodeID );
				if ( n != null )
				{
					bn = new BrowserNode( n.Properties.ToString() );
				}
			}

			return bn;
		}

		/// <summary>
		/// Modifies an existing property on the specified Node object.
		/// </summary>
		/// <param name="collectionID"></param>
		/// <param name="nodeID"></param>
		/// <param name="propertyName"></param>
		/// <param name="propertyType"></param>
		/// <param name="oldPropertyValue"></param>
		/// <param name="newPropertyValue"></param>
		/// <param name="propertyFlags"></param>
		[ WebMethod(EnableSession = true) ]
		[ SoapDocumentMethod ]
		public void ModifyProperty( string collectionID, string nodeID, string propertyName, string propertyType, string oldPropertyValue, string newPropertyValue, uint propertyFlags )
		{
			Store store = Store.GetStore();
			Collection c = store.GetCollectionByID( collectionID );
			if ( c != null )
			{
				Node n = c.GetNodeByID( nodeID );
				if ( n != null )
				{
					MultiValuedList mvl = n.Properties.GetProperties( propertyName );
					foreach( Property p in mvl )
					{
						if ( ( p.Type.ToString() == propertyType ) && ( p.ToString() == oldPropertyValue ) )
						{
							if ( ( propertyFlags & 0x0002000 ) == 0x00020000 ) p.LocalProperty = true;
							if ( ( propertyFlags & 0x0004000 ) == 0x00040000 ) p.MultiValuedProperty = true;
							p.SetPropertyValue( new Property( p.Name, p.Type, newPropertyValue ) );
							c.Commit( n );
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Adds a new property to the specified node.
		/// </summary>
		/// <param name="collectionID"></param>
		/// <param name="nodeID"></param>
		/// <param name="propertyName"></param>
		/// <param name="propertyType"></param>
		/// <param name="propertyValue"></param>
		/// <param name="propertyFlags"></param>
		[ WebMethod(EnableSession = true) ]
		[ SoapDocumentMethod ]
		public void AddProperty( string collectionID, string nodeID, string propertyName, string propertyType, string propertyValue, uint propertyFlags )
		{
			Store store = Store.GetStore();
			Collection c = store.GetCollectionByID( collectionID );
			if ( c != null )
			{
				Node n = c.GetNodeByID( nodeID );
				if ( n != null )
				{
					Property p = new Property( propertyName, ( Syntax )Enum.Parse( typeof( Syntax ), propertyType ), propertyValue );
					if ( ( propertyFlags & 0x00020000 ) == 0x00020000 ) p.LocalProperty = true;
					if ( ( propertyFlags & 0x00040000 ) == 0x00040000 ) p.MultiValuedProperty = true;

					n.Properties.AddNodeProperty( p );
					c.Commit( n );
				}
			}
		}

		/// <summary>
		/// Deletes the specified property from the specified Node object.
		/// </summary>
		/// <param name="collectionID"></param>
		/// <param name="nodeID"></param>
		/// <param name="propertyName"></param>
		/// <param name="propertyType"></param>
		/// <param name="propertyValue"></param>
		[ WebMethod(EnableSession = true) ]
		[ SoapDocumentMethod ]
		public void DeleteProperty( string collectionID, string nodeID, string propertyName, string propertyType, string propertyValue )
		{
			Store store = Store.GetStore();
			Collection c = store.GetCollectionByID( collectionID );
			if ( c != null )
			{
				Node n = c.GetNodeByID( nodeID );
				if ( n != null )
				{
					MultiValuedList mvl = n.Properties.GetProperties( propertyName );
					foreach( Property p in mvl )
					{
						if ( ( p.Type.ToString() == propertyType ) && ( p.ToString() == propertyValue ) )
						{
							p.DeleteProperty();
							c.Commit( n );
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Deletes the specified collection.
		/// </summary>
		/// <param name="collectionID"></param>
		[ WebMethod(EnableSession = true) ]
		[ SoapDocumentMethod ]
		public void DeleteCollection( string collectionID )
		{
			Store store = Store.GetStore();
			Collection c = store.GetCollectionByID( collectionID );
			if ( c != null )
			{
				c.Commit( c.Delete() );
			}
		}

		/// <summary>
		/// Deletes the specified Node object.
		/// </summary>
		/// <param name="collectionID"></param>
		/// <param name="nodeID"></param>
		[ WebMethod(EnableSession = true) ]
		[ SoapDocumentMethod ]
		public void DeleteNode( string collectionID, string nodeID )
		{
			Store store = Store.GetStore();
			Collection c = store.GetCollectionByID( collectionID );
			if ( c != null )
			{
				Node n = c.GetNodeByID( nodeID );
				if ( n != null )
				{
					c.Commit( c.Delete( n ) );
				}
			}
		}

		/// <summary>
		/// Returns a list of shallow collection nodes in the store.
		/// </summary>
		/// <returns>An array of BrowserShallowNode objects.</returns>
		[ WebMethod(EnableSession = true) ]
		[ SoapDocumentMethod ]
		public BrowserShallowNode[] EnumerateShallowCollections()
		{
			ArrayList list = new ArrayList();
			Store store = Store.GetStore();
			
			foreach ( ShallowNode sn in store )
			{
				list.Add( new BrowserShallowNode( sn ) );
			}

			return list.ToArray( typeof( BrowserShallowNode ) ) as BrowserShallowNode[];
		}

		/// <summary>
		/// Enumerates the shallow nodes in a collection.
		/// </summary>
		/// <param name="collectionID"></param>
		/// <returns></returns>
		[ WebMethod(EnableSession = true) ]
		[ SoapDocumentMethod ]
		public BrowserShallowNode[] EnumerateShallowNodes( string collectionID )
		{
			ArrayList list = new ArrayList();
			Store store = Store.GetStore();

			Collection c = store.GetCollectionByID( collectionID );
			if ( c != null )
			{
				foreach ( ShallowNode sn in c )
				{
					list.Add( new BrowserShallowNode( sn ) );
				}
			}

			return list.ToArray( typeof( BrowserShallowNode ) ) as BrowserShallowNode[];
		}

		/// <summary>
		/// Searches for shallow nodes containing a specific property.
		/// </summary>
		/// <param name="collectionID"></param>
		/// <param name="propertyName"></param>
		/// <param name="propertyType"></param>
		/// <param name="propertyValue"></param>
		/// <returns></returns>
		[ WebMethod(EnableSession = true) ]
		[ SoapDocumentMethod ]
		public BrowserShallowNode[] SearchForShallowNodes( string collectionID, string propertyName, string propertyType, string propertyValue, string operation )
		{
			ArrayList list = new ArrayList();
			Store store = Store.GetStore();

			// Build the property to search for.
			Property p = new Property( propertyName, ( Syntax )Enum.Parse( typeof( Syntax ), propertyType ), propertyValue );
			if ( collectionID == null || collectionID.Equals( string.Empty ) )
			{
				// The collection ID was not specified ... perform a store-wide search.
				ICSList nodeList = store.GetNodesByProperty( p, ( SearchOp )Enum.Parse( typeof( SearchOp ), operation ) );
				foreach ( ShallowNode sn in nodeList )
				{
					list.Add( new BrowserShallowNode( sn ) );
				}
			}
			else
			{
				// Get the collection.
				Collection c = store.GetCollectionByID( collectionID );
				if ( c != null )
				{
					// Search the collection.
					ICSList nodeList = c.Search( p, ( SearchOp )Enum.Parse( typeof( SearchOp ), operation ) );
					foreach ( ShallowNode sn in nodeList )
					{
						list.Add( new BrowserShallowNode( sn ) );
					}
				}
			}

			return list.ToArray( typeof( BrowserShallowNode ) ) as BrowserShallowNode[];
		}

		/// <summary>
		/// Gets the web service version.
		/// </summary>
		/// <returns>A string containing the web service version.</returns>
		[ WebMethod(EnableSession = true) ]
		[ SoapDocumentMethod ]
		public string GetVersion()
		{
			return webServiceVersion;
		}
		#endregion
	}
}
