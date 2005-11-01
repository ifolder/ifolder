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
	public class DisplayShallowNode : DisplayNode
	{
		#region Class Members
		private BrowserShallowNode bsn;
		#endregion

		#region Properties
		public override string Name
		{
			get { return bsn.Name; }
		}

		public override string ID
		{
			get { return bsn.ID; }
		}

		public override string Type
		{
			get { return bsn.Type; }
		}

		public override string CollectionID
		{
			get	{ return bsn.CID; }
		}

		public override bool IsCollection
		{
			get { return ( CollectionID == ID ) ? true : false; }
		}
		#endregion

		#region Constructor
		public DisplayShallowNode( BrowserShallowNode bsNode )
		{
			this.bsn = bsNode;
		}

		public DisplayShallowNode ( BrowserNode bNode )
		{
			DisplayNode dspNode = new DisplayNode( bNode );
			this.bsn = new BrowserShallowNode();
			this.bsn.Name = dspNode.Name;
			this.bsn.ID = dspNode.ID;
			this.bsn.Type = dspNode.Type;
			this.bsn.CID = dspNode.CollectionID;
			dspNode = null;
		}
		#endregion
	}


	/// <summary>
	/// Summary description for NodeBrowser.
	/// </summary>
	public class NodeBrowser2 : IStoreBrowser
	{
		private TreeNode searchNode;
		BrowserService browser;
		Form1 form;
		TreeView tView;
		ListView lView;
		RichTextBox rView;
		bool alreadyDisposed;
		string host;
		string storePath;
		string userName = String.Empty;
		string password = String.Empty;

		public NodeBrowser2(Form1 form, TreeView view, ListView lView, RichTextBox rView, string host, string storePath)
		{
			tView = view;
			this.form = form;
			this.lView = lView;
			this.rView = rView;
			this.host = host;
			this.storePath = storePath;
			rView.Show();
			lView.Show();

			if ( form.IsXmlView )
			{
				rView.BringToFront();
			}
			else
			{
				lView.BringToFront();
			}

			browser = new BrowserService();
			browser.Url = host + "/SimiasBrowser.asmx";
			tView.Dock = DockStyle.Left;
			alreadyDisposed = false;
		}

		~NodeBrowser2()
		{
			Dispose(true);
		}

		#region IStoreBrowser Members

		public BrowserService StoreBrowser
		{
			get { return browser; }
		}
		
		public string UserName
		{
			get { return userName; }
		}

		public string Password
		{
			get { return password; }
		}

		public void Search( SearchExpression searchExpression )
		{
			TreeNode tNode;

			// Build the name of the node.
			string name = buildDisplayName( searchExpression );

			if ( tView.SelectedNode.Equals( searchNode ) )
			{
				// Create a tree node to place this search under.
				tNode = new TreeNode( name, nodeImage( "Search" ), nodeImage( "Search" ) );
				tNode.Tag = searchNode.Tag;
				searchNode.Nodes.Add( tNode );
			}
			else
			{
				// A new search is being performed ... clear the current tree node.
				tNode = tView.SelectedNode;
				tNode.Text = name;
				tNode.Nodes.Clear();
			}

			try
			{
				string searchTarget = searchExpression.Target == null ? string.Empty : searchExpression.Target.CollectionID;

				BrowserShallowNode[] bsnList = 
					browser.SearchForShallowNodes(
					searchTarget,
					searchExpression.PropertyName,
					searchExpression.PropertyType,
					searchExpression.PropertyValue,
					searchExpression.Operation);
				foreach (BrowserShallowNode bsn in bsnList)
				{
					DisplayShallowNode n = new DisplayShallowNode( bsn );
					TreeNode nNode = new TreeNode(n.Name, nodeImage(n.Type), nodeImage(n.Type));
					nNode.Tag = n;
					tNode.Nodes.Add(nNode);
					if ( n.IsCollection )
					{
						nNode.Nodes.Add("temp");
					}
				}

				tView.SelectedNode = tNode;
			}
			catch (Exception e)
			{
				MessageBox.Show("Exception caught during search\n" + e.Message);
			}
		}

		public void AddChildren(TreeNode tNode)
		{
			if (tNode.Tag == null)
			{
				addCollections(tNode);
			}
			else
			{
				DisplayShallowNode dspNode = (DisplayShallowNode)tNode.Tag;
				if (dspNode.IsCollection)
				{
					BrowserShallowNode[] bsList = browser.EnumerateShallowNodes(dspNode.ID);
					foreach (BrowserShallowNode bsn in bsList)
					{
						DisplayShallowNode n = new DisplayShallowNode( bsn );
						if (n.ID != dspNode.ID)
						{
							TreeNode nNode = new TreeNode(n.Name, nodeImage(n.Type), nodeImage(n.Type));
							nNode.Tag = n;
							tNode.Nodes.Add(nNode);
						}
					}
				}
			}
		}

		public bool NeedsAuthentication()
		{
			bool needsAuth = true;

			try
			{
				this.browser.GetVersion();
				needsAuth = false;
			}
			catch ( System.Web.Services.Protocols.SoapHeaderException ex )
			{
				// Rethrow the exception.
				throw ex;
			}
			catch ( System.Net.WebException ex )
			{
				if ( ex.Message.IndexOf( "401" ) == -1 )
				{
					throw ex;
				}
			}
			catch ( Exception ex )
			{
				throw ex;
			}

			return needsAuth;
		}

		public bool ValidateCredentials(string storePath)
		{
			Simias.Client.LocalService.Start(browser, new Uri(host), storePath);
			return !NeedsAuthentication();
		}

		public bool ValidateCredentials( string userName, string password )
		{
			this.browser.Credentials = new NetworkCredential(userName, password, this.host);
			this.browser.CookieContainer = new CookieContainer();
			bool result = NeedsAuthentication();
			if ( result == false )
			{
				this.userName = userName;
				this.password = password;
			}

			return !result;
		}

		public void Show()
		{
			tView.Nodes.Clear();
			tView.BeginUpdate();
			searchNode = new TreeNode( "Queries", nodeImage("Search"), nodeImage("Search") );
			searchNode.Tag = new SearchExpression( null );
			tView.Nodes.Add( searchNode );
			TreeNode storeNode = new TreeNode("Store", nodeImage("Store"), nodeImage("Store"));
            tView.Nodes.Add(storeNode);
			storeNode.Nodes.Add("temp");
			tView.EndUpdate();
		}

		public void ShowNode(TreeNode tNode)
		{
			if ( tNode.Tag != null )
			{
				DisplayShallowNode sNode = (DisplayShallowNode)tNode.Tag;
				BrowserNode bNode = browser.GetNodeByID( sNode.CollectionID, sNode.ID );
				if ( bNode == null )
				{
					MessageBox.Show( "The node does not exist.", "Store Browser Error", MessageBoxButtons.OK );
					return;
				}

				DisplayNode dspNode = new DisplayNode( bNode );
				sNode.Document = dspNode.Document;

				if ( form.IsXmlView )
				{
					rView.Clear();
					rView.BringToFront();

					StringWriter sw = new StringWriter();
					writeXml(dspNode.Document, sw);
					rView.AppendText(sw.ToString());
				}
				else
				{
					lView.Items.Clear();
					lView.BringToFront();
					lView.Tag = dspNode;

					// Add the name;
					ListViewItem item = new ListViewItem("Name");
					item.SubItems.Add(dspNode.Name);
					item.SubItems.Add("String");
					lView.Items.Add(item);

					// Add the ID.
					item = new ListViewItem("ID");
					item.SubItems.Add(dspNode.ID);
					item.SubItems.Add("String");
					lView.Items.Add(item);

					// Add the type.
					item = new ListViewItem("Type");
					item.SubItems.Add(dspNode.Type);
					item.SubItems.Add("String");
					lView.Items.Add(item);

					foreach (DisplayProperty p in dspNode)
					{
						item = new ListViewItem(p.Name);
						item.Tag = p;

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
		}

		#endregion

		#region Private Methods
		private string buildDisplayName( SearchExpression searchExpression )
		{
			string s;

			switch ( searchExpression.Operation )
			{
				case "Equal":
					s = "{0} = {1} (case-insensitive)";
					break;
				case "Not_Equal":
					s = "{0} != {1}";
					break;
				case "Begins":
					s = "{0} begins with {1}";
					break;
				case "Ends":
					s = "{0} ends with {1}";
					break;
				case "Contains":
					s = "{0} contains {1}";
					break;
				case "Greater":
					s = "{0} > {1}";
					break;
				case "Less":
					s = "{0} < {1}";
					break;
				case "Greater_Equal":
					s = "{0} >= {1}";
					break;
				case "Less_Equal":
					s = "{0} <= {1}";
					break;
				case "Exists":
					s = "{0} exists";
					break;
				case "CaseEqual":
					s = "{0} = {1} (case-sensitive)";
					break;
				default:
					s = string.Empty;
					break;
			}

			s = string.Format(
				s + " ({2})", 
				searchExpression.PropertyName,
				searchExpression.PropertyValue,
				searchExpression.Target == null ? "Store" : searchExpression.Target.Name);

			return s;
		}

		private void writeXml(XmlDocument doc, TextWriter tw)
		{
			if (doc != null)
			{
				XmlTextWriter w = new XmlTextWriter(tw);
				w.Formatting = Formatting.Indented;
				doc.WriteTo(w);
			}
		}

		private int nodeImage( string type )
		{
			switch( type )
			{
				case "Store":
					return 0;

				case "Collection":
					return 1;

				case "POBox":
					return 2;

				case "LocalDatabase":
					return 3;

				case "Domain":
					return 4;

				case "DirNode":
					return 5;

				case "Subscription":
					return 6;

				case "Member":
					return 7;

				case "Identity":
					return 8;

				case "Policy":
					return 9;

				case "Search":
					return 12;

				default:
					return 10;
			}
		}

		private void addCollections(TreeNode tNode)
		{
			tView.BeginUpdate();
			try
			{
				BrowserShallowNode[] bsnList = browser.EnumerateShallowCollections();
				foreach(BrowserShallowNode bsn in bsnList )
				{
					DisplayShallowNode dspNode = new DisplayShallowNode( bsn );
					TreeNode colNode = new TreeNode(dspNode.Name, nodeImage(dspNode.Type), nodeImage(dspNode.Type));
					tNode.Nodes.Add(colNode);
					colNode.Tag = dspNode;
					colNode.Nodes.Add("temp");
				}
			}
			catch ( Exception ex )
			{
				MessageBox.Show( ex.Message, "Store Browser Error", MessageBoxButtons.OK );
				tView.Nodes.Clear();
				TreeNode storeNode = new TreeNode("Store", nodeImage("Store"), nodeImage("Store"));
				tView.Nodes.Add(storeNode);
				storeNode.Nodes.Add("temp");
			}

			tView.EndUpdate();
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
		#endregion
        
		#region IDisposable Members

		public void Dispose()
		{
			Dispose(false);
		}

		#endregion
	}

}
