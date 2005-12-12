/***********************************************************************
 *  $RCSfile: iFolderIconView.cs,v $
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
		private VBox							vbox;
		
		private ArrayList						viewGroups;
		
		private iFolderHolder					currentSelection;
		
		public event System.EventHandler		SelectionChanged;
	
		public event iFolderClickedHandler		iFolderClicked;
		public event iFolderClickedHandler		iFolderDoubleClicked;
		public event iFolderClickedHandler		BackgroundClicked;
		public event iFolderActivatedHandler	iFolderActivated;
		
		// FIXME: Remove this thread-checking debug code
		// The purpose of this code is to make sure that we're not attempting to
		// update the UI on a non-GUI thread.
		private static object threadCheckLockObject = new object();
		private static int CurrentThreadID = 0;
		private static void CheckThread()
		{
			lock(threadCheckLockObject)
			{
				int newThreadID = System.Threading.Thread.CurrentThread.GetHashCode();
				if (newThreadID != CurrentThreadID)
				{
					if (CurrentThreadID == 0)	// Assume that the first thread in is the GUI thread
						CurrentThreadID = newThreadID;
					else
					{
						Console.WriteLine("****** WARNING: iFolderIconView called from different thread: {0}", newThreadID);
						Console.WriteLine("\tOld Thread: {0}", CurrentThreadID);
						Console.WriteLine("\tNew Thread: {0}", newThreadID);
						Console.WriteLine(Environment.StackTrace);
					}
				}
			}
		}
		
		public iFolderHolder		SelectedFolder
		{
			get
			{
				iFolderIconView.CheckThread();
				return currentSelection;
			}
		}
		
		public TreeSelection Selection
		{
			get
			{
				iFolderIconView.CheckThread();
				// FIXME: Implement iFolderIconView.Selection
				return null;
			}
		}
		
		
		
		public iFolderIconView(Widget parentWidget)
		{
			iFolderIconView.CheckThread();
			this.ModifyBg(StateType.Normal, this.Style.Base(StateType.Normal));
			this.ModifyBase(StateType.Normal, this.Style.Base(StateType.Normal));
			this.CanFocus = true;
			
			vbox = new VBox(false, 0);
			this.Add(vbox);

			currentSelection = null;
			
			viewGroups = new ArrayList();

			this.Realized +=
				new EventHandler(OnWidgetRealized);
			
			parentWidget.SizeAllocated += OnSizeAllocated;
		}
		
		public void AddWidget(Widget w)
		{
			iFolderIconView.CheckThread();
			vbox.PackStart(w, false, false, 0);
			vbox.ShowAll();
		}
		
		public void RemoveWidget(Widget w)
		{
			iFolderIconView.CheckThread();
			if (w != null)
			{
				try
				{
					vbox.Remove(w);
				}
				catch{}
			}
		}
		
		public void AddGroup(iFolderViewGroup group)
		{
			iFolderIconView.CheckThread();
			viewGroups.Add(group);

			group.Selection.SelectionChanged += SelectionChangedHandler;
			
			vbox.PackStart(group, false, false, 0);

			group.RebuildTable();

			vbox.ShowAll();
		}
		
		public void RemoveGroup(iFolderViewGroup group)
		{
			iFolderIconView.CheckThread();
			if (viewGroups.Contains(group))
			{
				viewGroups.Remove(group);
				group.Selection.SelectionChanged -= SelectionChangedHandler;
				vbox.Remove(group);
			}
		}
		
		private void OnWidgetRealized(object o, EventArgs args)
		{
			iFolderIconView.CheckThread();
			Console.WriteLine("iFolderIconView.OnWidgetRealized");
		}
		
		private void OnSizeAllocated(object o, SizeAllocatedArgs args)
		{
			iFolderIconView.CheckThread();
			foreach(iFolderViewGroup group in viewGroups)
			{
				group.OnSizeAllocated(o, args);
			}
		}

		public TreePath GetPathAtPos(int x, int y)
		{
			iFolderIconView.CheckThread();
			return null;
		}
		
		public bool PathIsSelected(TreePath path)
		{
			iFolderIconView.CheckThread();
			return false;
		}
		
		public void SelectPath(TreePath path)
		{
			iFolderIconView.CheckThread();
		}
		
		public void UnselectAll()
		{
			iFolderIconView.CheckThread();
			foreach(iFolderViewGroup group in viewGroups)
			{
				group.Selection.SelectionChanged -= SelectionChangedHandler;
				group.Selection.UnselectAll();
				group.Selection.SelectionChanged += SelectionChangedHandler;
			}
			
			if (SelectionChanged != null)
			{
				currentSelection = null;
				SelectionChanged(null, EventArgs.Empty);
			}
		}
		
		public iFolderHolder GetiFolderAtPos(int x, int y)
		{
			iFolderIconView.CheckThread();
			Console.WriteLine("iFolderIconView.GetiFolderAtPos({0}, {1}", x, y);

			foreach(iFolderViewGroup group in viewGroups)
			{
				Gdk.Rectangle allocation = group.Allocation;
				int yLowerBound = allocation.Y;
				int yUpperBound = allocation.Y + allocation.Height;
				if (y >= yLowerBound
					&& y < yUpperBound)
				{
					return group.GetiFolderAtPos(x, y);
				}
			}
			
			return null;
		}
		
		private void SelectionChangedHandler(object o, EventArgs args)
		{
			iFolderIconView.CheckThread();
Console.WriteLine("iFolderIconView.SelectionChangedHandler()");
			iFolderViewGroupSelection gSelect = (iFolderViewGroupSelection)o;
			iFolderViewGroup group = gSelect.ViewGroup;
		
			// Deselect all other items from other groups
			foreach(iFolderViewGroup tempGroup in viewGroups)
			{
				if (group != tempGroup)
				{
					tempGroup.Selection.SelectionChanged -= SelectionChangedHandler;
					tempGroup.Selection.UnselectAll();
					tempGroup.Selection.SelectionChanged += SelectionChangedHandler;
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
		
		///
		/// Event Box Overrides
		///
		protected override bool OnButtonPressEvent(Gdk.EventButton evnt)
		{
			iFolderIconView.CheckThread();
			iFolderHolder holder;

			this.GrabFocus();
			
			Gdk.Window win = evnt.Window;
			int winXPos = 0;
			int winYPos = 0;
			win.GetPosition(out winXPos, out winYPos);
			
			int realX;
			int realY;
			
			if (winXPos > 0)
				realX = (int)evnt.X + winXPos;
			else
				realX = (int)evnt.X;
			
			if (winYPos > 0)
				realY = (int)evnt.Y + winYPos;
			else
				realY = (int)evnt.Y;
			
			holder = GetiFolderAtPos(realX, realY);
			if (holder == null)
			{
				if (BackgroundClicked != null)
					BackgroundClicked(this,
						new iFolderClickedArgs(null, evnt.Button));
			}
			else
			{
				if (iFolderClicked != null)
					iFolderClicked(this,
						new iFolderClickedArgs(holder, evnt.Button));
						
				if (evnt.Type == Gdk.EventType.TwoButtonPress)
				{
					if (iFolderDoubleClicked != null)
						iFolderDoubleClicked(this,
							new iFolderClickedArgs(holder, evnt.Button));
					if (iFolderActivated != null)
						iFolderActivated(this,
							new iFolderActivatedArgs(holder));
				}
			}
		
			return false;
		}
		
		protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
		{
			// FIXME: Implement iFolderIconView.OnKeyPressEvent
			// Control which item has keyboard focus, activating an item,
			// providing a context menu, etc..
	
			Console.WriteLine("iFolderIconView.OnKeyPressEvent(): {0}", evnt.Key);
			
			switch(evnt.Key)
			{
				case Gdk.Key.Return:
					if (currentSelection != null && iFolderActivated != null)
						iFolderActivated(this,
							new iFolderActivatedArgs(currentSelection));
					break;
				case Gdk.Key.Delete:
					// FIXME: Implement delete or remove action by keyboard
					break;
				case Gdk.Key.Home:
					// FIXME: Change the scroll to the very top and highlight the first item
					break;
				case Gdk.Key.End:
					break;
				case Gdk.Key.Left:
					break;
				case Gdk.Key.Right:
					break;
				case Gdk.Key.Up:
					break;
				case Gdk.Key.Down:
					break;
				case Gdk.Key.Page_Up:
//				case Gdk.Key.Prior:		// This is the exact same key (int) as Page_Up
					break;
				case Gdk.Key.Page_Down:
//				case Gdk.Key.Next:		// This is the exact same key (int) as Page_Down
					break;
				default:
					break;
			}
			
			return false;	// Allow this to continue on to other event handlers		
		}
	}

	public delegate void iFolderClickedHandler(object o, iFolderClickedArgs args);
	public class iFolderClickedArgs
	{
		private iFolderHolder	holder;
		private uint			button;
		
		public iFolderHolder	Holder
		{
			get{ return holder; }
		}
		
		public uint				Button
		{
			get{ return button; }
		}
		
		public iFolderClickedArgs(iFolderHolder holder, uint button)
		{
			this.holder = holder;
			this.button = button;
		}
	}
	
	public delegate void iFolderActivatedHandler(object o, iFolderActivatedArgs args);
	public class iFolderActivatedArgs
	{
		private iFolderHolder	holder;
		
		public iFolderHolder	Holder
		{
			get{ return holder; }
		}
		
		public iFolderActivatedArgs(iFolderHolder holder)
		{
			this.holder = holder;
		}
	}
}