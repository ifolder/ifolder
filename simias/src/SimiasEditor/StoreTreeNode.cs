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
	/// Store Tree Node
	/// </summary>
	public class StoreTreeNode : BaseTreeNode
	{
		private Configuration config;
		private Store store;

		public StoreTreeNode() : this(null)
		{
		}

		public StoreTreeNode(string path)
		{
			config = Configuration.CreateDefaultConfig(path);
			store = Store.GetStore();

			this.Text = config.StorePath;
			
			this.Nodes.Add("Working...");
		}


		public override void Refresh()
		{
			base.Refresh();

			foreach(ShallowNode sn in store)
			{
				this.Nodes.Add(new CollectionTreeNode(store.GetCollectionByID(sn.ID)));
			}
		}

		public override void Show(ListView listView)
		{
			base.Show(listView);

			Add(listView, "ID", store.ID);
			Add(listView, "Local Domain", store.LocalDomain);
			Add(listView, "Store Path", store.StorePath);
			Add(listView, "Current Identity", store.CurrentIdentity.Name);
		}

		private void Add(ListView listView, string name, string value)
		{
			ListViewItem lvi = new ListViewItem(new string[] { name, value });
			lvi.ForeColor = Color.DarkGray;
			lvi.Tag = this;
			listView.Items.Add(lvi);
		}
	}
}
