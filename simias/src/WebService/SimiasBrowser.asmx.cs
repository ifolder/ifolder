using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

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
	/// Summary description for Service1.
	/// </summary>
	public class Browser : System.Web.Services.WebService
	{
		public Browser()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

		#region Component Designer generated code
		
		//Required by the Web Services Designer 
		private IContainer components = null;
				
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

		/// <summary>
		/// Returns a list of collections in the store.
		/// </summary>
		/// <returns>An array of BrowserNode objects.</returns>
		[ WebMethod( true ) ]
		[ SoapDocumentMethod ]
		public BrowserNode[] EnumerateCollections()
		{
			ArrayList list = new ArrayList();
			Store store = Store.GetStore();
			
			foreach ( ShallowNode sn in store )
			{
				Collection c = new Collection( store, sn );
				list.Add( new BrowserNode( c.Properties.ToString() ) );
			}

			return list.ToArray( typeof( BrowserNode ) ) as BrowserNode[];
		}
	}
}
