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
	/// Base Tree Node
	/// </summary>
	public abstract class BaseTreeNode : TreeNode
	{
		public virtual void Refresh()
		{
			this.Nodes.Clear();
		}

		public virtual void Show(ListView listView)
		{
			listView.Items.Clear();
		}
	}
}
