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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Net;
using System.Xml;
using System.Text.RegularExpressions;

namespace StoreBrowser
{
	public class Relationship
	{
		#region Class Members
		private const string RootID = "a9d9a742-fd42-492c-a7f2-4ec4f023c625";
		private string collectionID;
		private string nodeID;
		#endregion

		#region Properties
		public string CollectionID
		{
			get { return collectionID; }
		}

		public bool IsRoot
		{
			get { return ( nodeID == RootID ) ? true : false; }
		}

		public string NodeID
		{
			get { return nodeID; }
		}
		#endregion

		#region Constructor
		public Relationship( string relationString )
		{
			int index = relationString.IndexOf( ':' );
			if ( index == -1 )
			{
				throw new ApplicationException( String.Format( "Invalid relationship format: {0}.", relationString ) );
			}

			nodeID = relationString.Substring( 0, index );
			collectionID = relationString.Substring( index + 1 );
		}
		#endregion
	}

	public class DisplayProperty
	{
		#region Class Members
		private static string NameTag = "name";
		private static string TypeTag = "type";
		private static string FlagTag = "flags";

		private XmlElement element;
		#endregion

		#region Properties
		public string Name
		{
			get { return element.GetAttribute(NameTag); }
		}

		public string Type
		{
			get { return element.GetAttribute(TypeTag); }
		}

		public string Value
		{
			get { return (Type == "XmlDocument") ? element.InnerXml : element.InnerText; }
		}

		public uint Flags
		{
			get 
			{ 
				string flagValue = element.GetAttribute( FlagTag );
				return (flagValue != null && flagValue != String.Empty) ? Convert.ToUInt32(flagValue) : 0; 
			}
		}

		public bool IsLocal
		{
			get { return ((Flags & 0x00020000) == 0x00020000) ? true : false; }
		}

		public bool IsMultiValued
		{
			get { return ((Flags & 0x00040000) == 0x00040000) ? true : false; }
		}
		#endregion

		#region Constructor
		public DisplayProperty( XmlElement element )
		{
			this.element = element;
		}
		#endregion
	}

	public class DisplayNode : IEnumerable
	{
		#region Class Members
		private static string ObjectTag = "Object";
		private static string NameTag = "name";
		private static string IDTag = "id";
		private static string TypeTag = "type";
		private static string CollectionIDTag = "CollectionId";

		private XmlDocument document;
		#endregion

		#region Properties
		public string Name
		{
			get { return document.DocumentElement[ ObjectTag ].GetAttribute( NameTag ); }
		}

		public string ID
		{
			get { return document.DocumentElement[ ObjectTag ].GetAttribute( IDTag ); }
		}

		public string Type
		{
			get { return document.DocumentElement[ ObjectTag ].GetAttribute( TypeTag ); }
		}

		public string CollectionID
		{
			get	{ return FindSingleValue( CollectionIDTag ); }
		}

		public bool IsCollection
		{
			get { return ( CollectionID == ID ) ? true : false; }
		}

		public XmlDocument Document
		{
			get { return document; } 
		}
		#endregion

		#region Constructor
		public DisplayNode( BrowserNode bNode )
		{
			document = new XmlDocument();
			document.LoadXml( bNode.NodeData );
		}
		#endregion

		#region Private Methods
		internal string FindSingleValue( string name )
		{
			string singleValue = null;

			// Create a regular expression to use as the search string.
			Regex searchName = new Regex( "^" + name + "$", RegexOptions.IgnoreCase );

			// Walk each property node and do a case-insensitive compare on the names.
			foreach ( XmlElement x in document.DocumentElement[ ObjectTag ] )
			{
				if ( searchName.IsMatch( x.GetAttribute( NameTag ) ) )
				{
					DisplayProperty p = new DisplayProperty( x );
					singleValue = p.Value;
					break;
				}
			}

			return singleValue;
		}
		#endregion

		#region IEnumerable Members
		public IEnumerator GetEnumerator()
		{
			return new DisplayNodeEnum( document );
		}

		private class DisplayNodeEnum : IEnumerator
		{
			#region Class Members
			private IEnumerator e;
			#endregion

			#region Constructor
			public DisplayNodeEnum( XmlDocument document )
			{
				e = document.DocumentElement[ ObjectTag ].GetEnumerator();
			}
			#endregion

			#region IEnumerator Members

			public void Reset()
			{
				e.Reset();
			}

			public object Current
			{
				get { return new DisplayProperty( e.Current as XmlElement ); }
			}

			public bool MoveNext()
			{
				return e.MoveNext();
			}
			#endregion
		}
		#endregion
	}

	/// <summary>
	/// Summary description for NodeBrowser.
	/// </summary>
	public class NodeBrowser : IStoreBrowser
	{
		BrowserService browser;
		TreeView tView;
		ListView lView;
		bool alreadyDisposed;

		public NodeBrowser(TreeView view, ListView lView, string host, string username, string password)
		{
			tView = view;
			this.lView = lView;
			lView.BringToFront();
			lView.Show();
			browser = new BrowserService();
			browser.Url = host + "/SimiasBrowser.asmx";
			tView.Dock = DockStyle.Left;
			alreadyDisposed = false;

			if (username != null && username != "")
			{
				this.browser.Credentials = new NetworkCredential(username, password, host);
				this.browser.CookieContainer = new CookieContainer();
			}
			else
			{
				Simias.Client.LocalService.Start(browser);
			}
		}

		~NodeBrowser()
		{
			Dispose(true);
		}

		#region IStoreBrowser Members

		public BrowserService StoreBrowser
		{
			get { return browser; }
		}

		public void Show()
		{
			tView.Nodes.Clear();
			tView.BeginUpdate();
			TreeNode storeNode = new TreeNode("Store");
            tView.Nodes.Add(storeNode);
			storeNode.Nodes.Add("temp");
			tView.EndUpdate();
		}

		public void ShowNode(TreeNode tNode)
		{
			lView.Items.Clear();
			if (tNode.Tag != null)
			{
				DisplayNode dspNode = (DisplayNode)tNode.Tag;
				lView.Tag = dspNode;

				// Add the name;
				ListViewItem item = new ListViewItem("Name");
				item.BackColor = Color.LightBlue;
				item.SubItems.Add(dspNode.Name);
				item.SubItems.Add("String");
				lView.Items.Add(item);

				// Add the ID.
				item = new ListViewItem("ID");
				item.BackColor = Color.White;
				item.SubItems.Add(dspNode.ID);
				item.SubItems.Add("String");
				lView.Items.Add(item);

				// Add the type.
				item = new ListViewItem("Type");
				item.BackColor = Color.LightBlue;
				item.SubItems.Add(dspNode.Type);
				item.SubItems.Add("String");
				lView.Items.Add(item);

				Color c = Color.LightBlue;
				foreach (DisplayProperty p in dspNode)
				{
					c = (c == Color.LightBlue) ? Color.White : Color.LightBlue;
					item = new ListViewItem(p.Name);
					item.Tag = p;
					item.BackColor = c;

					if (p.Type == "Relationship")
					{
						string valueStr = p.Value;
						Relationship r = new Relationship(valueStr);
						BrowserNode cbn = browser.GetCollectionByID( r.CollectionID );
						if ( cbn != null )
						{
							BrowserNode nbn = !r.IsRoot ? browser.GetNodeByID( r.CollectionID, r.NodeID ) : null;
							valueStr = String.Format("{0}{1}", new DisplayNode(cbn).Name, (nbn != null) ? ":" + new DisplayNode(nbn).Name : null);
						}

						item.SubItems.Add(valueStr);
					}
					else
					{
						item.SubItems.Add(p.Value);
					}

					item.SubItems.Add(p.Type);
					string flags = p.IsLocal ? "(Local) " : "";
					flags += p.IsMultiValued ? "(MV) " : "";
					flags += string.Format("0x{0}", p.Flags.ToString("X4"));
					item.SubItems.Add(flags);
					lView.Items.Add(item);
				}
			}
		}

		#endregion

		private void addCollections(TreeNode tNode)
		{
			tView.BeginUpdate();
			BrowserNode[] bList = browser.EnumerateCollections();
			foreach(BrowserNode bn in bList )
			{
				DisplayNode dspNode = new DisplayNode( bn );
				TreeNode colNode = new TreeNode(dspNode.Name);
				tNode.Nodes.Add(colNode);
				colNode.Tag = dspNode;
				colNode.Nodes.Add("temp");
			}
			tView.EndUpdate();
		}

		public void AddChildren(TreeNode tNode)
		{
			if (tNode.Tag == null)
			{
				addCollections(tNode);
			}
			else
			{
				DisplayNode dspNode = (DisplayNode)tNode.Tag;
				if (dspNode.IsCollection)
				{
					BrowserNode[] bList = browser.EnumerateNodes(dspNode.ID);
					foreach (BrowserNode bn in bList)
					{
						DisplayNode n = new DisplayNode( bn );
						if (n.ID != dspNode.ID)
						{
							TreeNode nNode = new TreeNode(n.Name);
							nNode.Tag = n;
							tNode.Nodes.Add(nNode);
						}
					}
				}
			}
		}

		private void Dispose(bool inFinalize)
		{
			if (!alreadyDisposed)
			{
				if (!inFinalize)
				{
					GC.SuppressFinalize(this);
				}
				alreadyDisposed = true;
			}
		}
        
		#region IDisposable Members

		public void Dispose()
		{
			Dispose(false);
		}

		#endregion
	}

}
