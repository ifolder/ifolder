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
using System.Windows.Forms;

using Simias;
using Simias.Storage;

namespace Simias.Editor
{
	/// <summary>
	/// Collection Tree Node
	/// </summary>
	public class CollectionTreeNode : NodeTreeNode
	{
		private Collection collection;

		public CollectionTreeNode(Collection collection) : base(collection, collection)
		{
			this.collection = collection;
		}

		public override void Refresh()
		{
			base.Refresh();

			//Relationship relationship = new Relationship(collection.ID);

			foreach(ShallowNode sn in collection)
			{
				// TODO: remove
				if (sn.ID != collection.ID)
				{
					this.Nodes.Add(new NodeTreeNode(collection, collection.GetNodeByID(sn.ID)));
				}
			}
		}

		public override void Show(ListView listView)
		{
			base.Show(listView);
		}

	}
}
