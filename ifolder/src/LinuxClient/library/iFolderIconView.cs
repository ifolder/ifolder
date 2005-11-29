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
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author:
 *		Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/


using System;
using System.Collections;
using Gtk;

namespace Novell.iFolder
{
	public class iFolderIconView : EventBox
	{
		private ScrolledWindow		sw;
		private VBox				vbox;
		
		private ArrayList			viewGroups;
		
		private iFolderHolder		currentSelection;
		
		private int					availableYPos = 0;

		public event System.EventHandler			SelectionChanged;
		
		public iFolderHolder		SelectedFolder
		{
			get{ return currentSelection; }
		}
		
		public TreeSelection Selection
		{
			get
			{
				// FIXME: Implement iFolderIconView.Selection
				return null;
			}
		}
		
		
		
		public iFolderIconView()
		{
			sw = new ScrolledWindow();
			this.Add(sw);
			sw.ShadowType = ShadowType.None;
			
			vbox = new VBox(false, 0);
			sw.AddWithViewport(vbox);

			currentSelection = null;
			
			viewGroups = new ArrayList();
			
			this.Realized +=
				new EventHandler(OnWidgetRealized);
			
			this.SizeAllocated += OnSizeAllocated;
		}
		
		public void AddGroup(iFolderViewGroup group)
		{
			viewGroups.Add(group);

			group.Selection.SelectionChanged += SelectionChangedHandler;
			
			vbox.PackStart(group, false, false, 0);

			vbox.ShowAll();
		}
		
		public void RemoveGroup(iFolderViewGroup group)
		{
			if (viewGroups.Contains(group))
			{
				viewGroups.Remove(group);
				group.Selection.SelectionChanged -= SelectionChangedHandler;
				vbox.Remove(group);
			}
		}
		
		private void OnWidgetRealized(object o, EventArgs args)
		{
			Console.WriteLine("iFolderIconView.OnWidgetRealized");
		}
		
		private void OnSizeAllocated(object o, SizeAllocatedArgs args)
		{
			foreach(iFolderViewGroup group in viewGroups)
			{
				group.OnSizeAllocated(o, args);
			}
		}

		public TreePath GetPathAtPos(int x, int y)
		{
			return null;
		}
		
		public bool PathIsSelected(TreePath path)
		{
			return false;
		}
		
		public void SelectPath(TreePath path)
		{
		}
		
		private void SelectionChangedHandler(object o, EventArgs args)
		{
Console.WriteLine("iFolderIconView.SelectionChangedHandler()");
			iFolderViewGroupSelection gSelect = (iFolderViewGroupSelection)o;
			iFolderViewGroup group = gSelect.ViewGroup;
		
			// Deselect all other items from other groups
			foreach(iFolderViewGroup tempGroup in viewGroups)
			{
				if (group != tempGroup)
				{
					tempGroup.Selection.UnselectAll();
				}
			}
		
			if (SelectionChanged != null)
			{
				iFolderViewItem selectedItem = null;
				if (group.Selection.GetSelected(out selectedItem))
				{
					currentSelection = selectedItem.Holder;
					SelectionChanged(selectedItem.Holder, EventArgs.Empty);
				}
				else
				{
					currentSelection = null;
					SelectionChanged(null, EventArgs.Empty);
				}
			}
		}
	}
}