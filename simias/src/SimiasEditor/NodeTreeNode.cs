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
using System.Drawing;
using System.Windows.Forms;

using Simias;
using Simias.Storage;

namespace Simias.Editor
{
	/// <summary>
	/// Node Tree Node
	/// </summary>
	public class NodeTreeNode : BaseTreeNode
	{
		private Node node;
		private Collection collection;

		public NodeTreeNode(Collection collection, Node node)
		{
			this.collection = collection;
			this.node = node;

			this.Text = node.Name;
			
			this.Nodes.Add("Working...");
		}

		public override void Refresh()
		{
			base.Refresh();

			Relationship relationship = new Relationship(collection.ID, node.ID);

			foreach(ShallowNode sn in collection.Search("Parent", relationship))
			{
				this.Nodes.Add(new NodeTreeNode(collection, collection.GetNodeByID(sn.ID)));
			}
		}

		public override void Show(ListView listView)
		{
			base.Show(listView);
			
			foreach(Property property in node.Properties)
			{
				string name = property.Name;
				string value = property.Value.ToString();

				ListViewItem lvi = new ListViewItem(new string[] { name, value });
				
				if (property.IsSystemProperty())
				{
					lvi.ForeColor = Color.DarkGray;
				}
				
				lvi.Tag = this;
				listView.Items.Add(lvi);
			}
		}

	}
}
