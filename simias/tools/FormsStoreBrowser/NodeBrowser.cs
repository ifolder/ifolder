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
using Simias;
using Simias.Storage;


namespace StoreBrowser
{
	/// <summary>
	/// Summary description for NodeBrowser.
	/// </summary>
	public class NodeBrowser : IStoreBrowser
	{
		Store store;
		TreeView tView;
		RichTextBox rBox;
		ListView lView;
		bool alreadyDisposed;

		public NodeBrowser(TreeView view, ListView lView, string dbPath)
		{
			tView = view;
			this.lView = lView;
			lView.BringToFront();
			lView.Show();
			store = Store.GetStore();
			tView.Dock = DockStyle.Left;
			alreadyDisposed = true;
		}

		~NodeBrowser()
		{
			Dispose(true);
		}

		#region IStoreBrowser Members

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
				Node cNode = (Node)tNode.Tag;
				lView.Tag = cNode;
				// Add the name;
				ListViewItem item = new ListViewItem("Name");
				item.BackColor = Color.LightBlue;
				item.SubItems.Add(cNode.Name);
				item.SubItems.Add(Syntax.String.ToString());
				lView.Items.Add(item);

				// Add the ID.
				item = new ListViewItem("ID");
				item.BackColor = Color.White;
				item.SubItems.Add(cNode.ID);
				item.SubItems.Add(Syntax.String.ToString());
				lView.Items.Add(item);

				// Add the type.
				item = new ListViewItem("Type");
				item.BackColor = Color.LightBlue;
				item.SubItems.Add(cNode.Type);
				item.SubItems.Add(Syntax.String.ToString());
				lView.Items.Add(item);

				Color c = Color.LightBlue;
				foreach (Property p in cNode.Properties)
				{
					c = c == Color.LightBlue ? Color.White : Color.LightBlue;
					item = new ListViewItem(p.Name);
					item.Tag = p;
					item.BackColor = c;

					if (p.Type == Syntax.Relationship)
					{
						Node rNode = null;
						Collection rCol = store.GetCollectionByID((p.Value as Relationship).CollectionID);
						if (rCol != null )
						{
							rNode = rCol.GetNodeByID((p.Value as Relationship).NodeID);
						}

						if (rCol == null || rNode == null)
						{
							item.SubItems.Add(p.ToString());
						}
						else
						{
							item.SubItems.Add(rCol.Name + ":" + rNode.Name);
						}
					}
					else
					{
						item.SubItems.Add(p.ToString());
					}

					item.SubItems.Add(p.Type.ToString());
					string flags = p.LocalProperty ? "(Local) " : "";
					flags += p.MultiValuedProperty ? "(MV) " : "";
					flags += string.Format("0x{0}", p.Flags.ToString("X4"));
					item.SubItems.Add(flags);
					lView.Items.Add(item);
				}
			}
			
		}

		#endregion

		private void AddProperties(TreeNode tNode, Node node)
		{	
			tNode.Nodes.Add(new TreeNode("ID : " + node.ID, 1, 1));
			tNode.Nodes.Add(new TreeNode("Type : " + node.Type, 1, 1));
			Color c = Color.LightBlue;
			foreach (Property p in node.Properties)
			{
//				if (p.Type == Syntax.Relationship)
//				{
//				}
//				else
//				{
					TreeNode pNode = new TreeNode(string.Format("{0,-20} : {1,40}", p.Name, p.Value.ToString()), 1, 1);
					c = pNode.BackColor = (c == Color.LightBlue) ? Color.White : Color.LightBlue;
					pNode.Nodes.Add(new TreeNode("Value: " + p.Value.ToString(), 1, 1));
					pNode.Nodes.Add(new TreeNode("Type : " + p.Type.ToString(), 1, 1));
					pNode.Nodes.Add(new TreeNode("Flags: " + p.Flags.ToString(), 1, 1));
					tNode.Nodes.Add(pNode);
//				}
			}
		}

		private void addCollections(TreeNode tNode)
		{
			tView.BeginUpdate();
			foreach(ShallowNode sn in store)
			{
				Collection col = new Collection(store, sn);
				TreeNode colNode = new TreeNode(col.Name);
				tNode.Nodes.Add(colNode);
				colNode.Tag = col;
				colNode.Nodes.Add("temp");
				//AddChildren(colNode, col);
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
				Node node = (Node)tNode.Tag;
				if ( node is Collection )
				{
					foreach (ShallowNode sn in (Collection)node)
					{
						if (sn.ID != node.ID)
						{
							Node n = new Node((Collection)node, sn);
							TreeNode nNode = new TreeNode(n.Name);
							nNode.Tag = n;
							tNode.Nodes.Add(nNode);
							//nNode.Nodes.Add("temp");
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
