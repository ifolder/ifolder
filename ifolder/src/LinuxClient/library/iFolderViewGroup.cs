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
	public class iFolderViewGroup : VBox
	{
		private VBox						contentVBox;

		private string						name;
		private TreeModel					model;

		private Hashtable					items;

		private Label						nameLabel;
		private Table						table;
		
		private iFolderViewGroupSelection	selection;

		// Resize Table Functionality
		private static uint					resizeTimeout = 20;
		private uint						resizeTableTimeoutID;
		
		// Rebuild Table Functionality (add/remove items without resizing)
		private static uint					rebuildTimeout = 20;
		private uint						rebuildTableTimeoutID;
		
		private static int					ItemMaxWidth = 300;
		
		private int							currentWidth;
		
		private bool						bFirstTableBuild;
		
		public new string Name
		{
			get{ return name; }
		}
		
		public TreeModel Model
		{
			get{ return model; }
		}
		
		public iFolderViewGroupSelection	Selection
		{
			get{ return selection; }
		}
		
		public iFolderViewItem[]			Items
		{
			get
			{
Console.WriteLine("iFolderViewGroup.Items");
Console.WriteLine("\tCount: {0}", items.Count);
				int i = 0;
				iFolderViewItem[] itemsA = new iFolderViewItem[items.Count];
				foreach(iFolderViewItem item in items.Values)
				{
					itemsA[i] = item;
				
					i++;
				}

Console.WriteLine("iFolderViewGroup.Items returning {0} items", itemsA.Length);				
				return itemsA;
			}
		}
	
		public iFolderViewGroup(string name, TreeModel model)
		{
			this.name = name;
			this.model = model;

			items = new Hashtable();
			
			selection = new iFolderViewGroupSelection(this);
			
			resizeTableTimeoutID = 0;
			rebuildTableTimeoutID = 0;
			
			currentWidth = 1;
			bFirstTableBuild = true;
			
			this.PackStart(CreateWidgets(), true, true, 0);
			
			this.Realized +=
				new EventHandler(OnWidgetRealized);
		}
		
		private Widget CreateWidgets()
		{
			contentVBox = new VBox(false, 0);
			contentVBox.BorderWidth = 12;
			
			nameLabel = new Label(
				string.Format(
					"<span size=\"xx-large\">{0}</span>",
					name));
			contentVBox.PackStart(nameLabel, false, false, 0);
			nameLabel.UseMarkup = true;
			nameLabel.UseUnderline = false;
			nameLabel.ModifyFg(StateType.Normal, this.Style.Base(StateType.Selected));
			nameLabel.Xalign = 0;
			
			table = new Table(1, 1, true);
			contentVBox.PackStart(table, true, true, 0);
			table.ColumnSpacing = 12;
			table.RowSpacing = 12;
			table.BorderWidth = 12;
			table.ModifyBase(StateType.Normal, this.Style.Base(StateType.Prelight));

//			this.SizeAllocated += OnSizeAllocated;
			
			return contentVBox;
		}
		
		public void OnSizeAllocated(object o, SizeAllocatedArgs args)
		{
Console.WriteLine("iFolderViewGroup.OnSizeAllocated: Width: {0}", args.Allocation.Width);
			if (currentWidth != args.Allocation.Width)
			{
				// Don't delay the rebuild the first time up
				bool bDelay = currentWidth == 1 ? false : true;
				
				currentWidth = args.Allocation.Width;

				if (bDelay)
				{
					if (resizeTableTimeoutID != 0)
					{
						GLib.Source.Remove(resizeTableTimeoutID);
						resizeTableTimeoutID = 0;
					}
					
					resizeTableTimeoutID = GLib.Timeout.Add(
						resizeTimeout, new GLib.TimeoutHandler(ResizeTableCallback));
				}
				else
					ResizeTable();
			}
		}
		
		private void OnWidgetRealized(object o, EventArgs args)
		{
			///
			/// Register for TreeModel events
			///
			model.RowChanged +=
				new RowChangedHandler(OnRowChanged);
			model.RowDeleted +=
				new RowDeletedHandler(OnRowDeleted);
			model.RowInserted +=
				new RowInsertedHandler(OnRowInserted);
		}
		
		private void ResizeTable()
		{
Console.WriteLine("iFolderViewGroup.ResizeTable({0})", name);
			int numOfItems = model.IterNChildren();
Console.WriteLine("\tNum of Items: {0}", numOfItems);

			if (numOfItems > 0)
			{
				int availableWidth = currentWidth
									 - (int)(contentVBox.BorderWidth * 2)
									 - (int)(table.BorderWidth * 2);
Console.WriteLine("\tWidth Available: {0}", availableWidth);
				int numOfColumns = availableWidth / ItemMaxWidth;
				if (numOfColumns < 1)
					numOfColumns = 1;	// Force at least one column
				int numOfRows = numOfItems / numOfColumns;
				if ((numOfItems % numOfColumns) > 0)
					numOfRows++;

				// Only resize the table if the number of columns is different
				if (!bFirstTableBuild && numOfColumns == (int)table.NColumns)
				{
Console.WriteLine("\tNumber of columns hasn't changed: {0}", numOfColumns);
					return;
				}
				else
				{
					RebuildTable();
				}
			}
		}
		
		private void RebuildTable()
		{
Console.WriteLine("iFolderViewGroup.ResizeTable({0})", name);
			int numOfItems = model.IterNChildren();
Console.WriteLine("\tNum of Items: {0}", numOfItems);

			if (numOfItems > 0)
			{
				int availableWidth = currentWidth
									 - (int)(contentVBox.BorderWidth * 2)
									 - (int)(table.BorderWidth * 2);
Console.WriteLine("\tWidth Available: {0}", availableWidth);
				int numOfColumns = availableWidth / ItemMaxWidth;
				if (numOfColumns < 1)
					numOfColumns = 1;	// Force at least one column
				int numOfRows = numOfItems / numOfColumns;
				if ((numOfItems % numOfColumns) > 0)
					numOfRows++;

				bFirstTableBuild = false;

				///
				/// Clear out the old items
				///
				items.Clear();

				foreach (Widget w in table.Children)
				{
					table.Remove(w);
					w.Destroy();
				}

Console.WriteLine("\tResizing table to {0} rows and {1} columns", numOfRows, numOfColumns);

				table.Resize((uint)numOfRows, (uint)numOfColumns);
	
				int currentRow = 0;
				int currentColumn = 0;
				
				TreeIter iter;
				if (model.GetIterFirst(out iter))
				{
					do
					{
						iFolderHolder holder = (iFolderHolder)model.GetValue(iter, 0);
						if (holder != null)
						{
							iFolderViewItem item = new iFolderViewItem(holder, this, iter, ItemMaxWidth);
							table.Attach(item,
										 (uint)currentColumn, (uint)currentColumn + 1,
										 (uint)currentRow, (uint)currentRow + 1,
										 AttachOptions.Shrink | AttachOptions.Fill,
										 0, 0, 0);
							
							// Save off the item so we can quickly reference it later
							TreePath path = model.GetPath(iter);
Console.WriteLine("\tSaving off item using path: {0}", path.ToString());
							items[path.ToString()] = item;
							
							// Register for the click events
							item.LeftClicked +=
								new EventHandler(OnItemLeftClicked);
							item.RightClicked +=
								new EventHandler(OnItemRightClicked);
							item.DoubleClicked +=
								new EventHandler(OnItemDoubleClicked);
							
							currentColumn = ((currentColumn + 1) % numOfColumns);
							if (currentColumn == 0)
								currentRow++;
						}
					} while (model.IterNext(ref iter));
				}
				
				table.ShowAll();
			}
			else
			{
				items.Clear();
				foreach(Widget w in table.Children)
				{
					table.Remove(w);
					w.Destroy();
				}
			}
		}
		
		private void OnRowChanged(object o, RowChangedArgs args)
		{
			iFolderViewItem item = (iFolderViewItem)items[args.Path.ToString()];
			if (item != null)
			{
Console.WriteLine("iFolderViewGroup.OnRowChanged: {0}", item.Holder.iFolder.Name);
				item.Refresh();
			}
		}
		
		private void OnRowDeleted(object o, RowDeletedArgs args)
		{
			iFolderViewItem item = (iFolderViewItem)items[args.Path.ToString()];
			if (item != null)
			{
Console.WriteLine("iFolderViewGroup.OnRowDeleted: {0}", item.Holder.iFolder.Name);
//				items.Remove(args.Path.ToString());
			}

			if (rebuildTableTimeoutID != 0)
			{
				GLib.Source.Remove(rebuildTableTimeoutID);
				rebuildTableTimeoutID = 0;
			}
			
			rebuildTableTimeoutID = GLib.Timeout.Add(
				rebuildTimeout, new GLib.TimeoutHandler(RebuildTableCallback));
		}
		
		private void OnRowInserted(object o, RowInsertedArgs args)
		{
			iFolderHolder ifHolder = (iFolderHolder)model.GetValue(args.Iter, 0);
			if (ifHolder != null)
			{
Console.WriteLine("iFolderViewGroup.OnRowInserted: {0}", ifHolder.iFolder.Name);
//				iFolderViewItem item = new iFolderViewItem(ifHolder, this, args.Iter, ItemMaxWidth);
//				items[args.Path.ToString()] = item;

				// Register for the click events
//				item.LeftClicked +=
//					new EventHandler(OnItemLeftClicked);
//				item.RightClicked +=
//					new EventHandler(OnItemRightClicked);
//				item.DoubleClicked +=
//					new EventHandler(OnItemDoubleClicked);
			}

			if (rebuildTableTimeoutID != 0)
			{
				GLib.Source.Remove(rebuildTableTimeoutID);
				rebuildTableTimeoutID = 0;
			}
			
			rebuildTableTimeoutID = GLib.Timeout.Add(
				rebuildTimeout, new GLib.TimeoutHandler(RebuildTableCallback));
		}

		private bool ResizeTableCallback()
		{
			ResizeTable();
			return false;
		}
		
		private bool RebuildTableCallback()
		{
			RebuildTable();
			return false;
		}
		
		private void OnItemLeftClicked(object o, EventArgs args)
		{
			iFolderViewItem item = (iFolderViewItem)o;
			selection.SelectItem(item);
		}
		
		private void OnItemRightClicked(object o, EventArgs args)
		{
		}
		
		private void OnItemDoubleClicked(object o, EventArgs args)
		{
		}
	}
	
	public class iFolderViewGroupSelection
	{
		private iFolderViewGroup 	group;
		private SelectionMode		mode;
		
		public iFolderViewGroup ViewGroup
		{
			get{ return group; }
		}
		
		public SelectionMode Mode
		{
			get{ return mode; }
			set
			{
				mode = value;
				UnselectAll();
			}
		}

		///
		/// Events
		///
		public event EventHandler			SelectionChanged;
		
		public iFolderViewGroupSelection(iFolderViewGroup group) : base()
		{
Console.WriteLine("iFolderViewGroupSelection.iFolderViewGroupSelection");
			this.group = group;
			
			// Set up the defaults
			this.Mode = SelectionMode.Single;
		}
		
		public int CountSelectedRows()
		{
Console.WriteLine("iFolderViewGroupSelection.CountSelectedRows");
			int count = 0;
			
			foreach(iFolderViewItem item in group.Items)
			{
				if (item.Selected)
					count++;
			}
			
			return count;
		}
		
		public iFolderViewItem[] GetSelectedItems()
		{
Console.WriteLine("iFolderViewGroupSelection.GetSelectedItems");
			ArrayList arrayList = new ArrayList();
			foreach(iFolderViewItem item in group.Items)
			{
				if (item.Selected)
				{
					arrayList.Add(item);
				}
			}
			
			iFolderViewItem[] items =
				(iFolderViewItem[])arrayList.ToArray(typeof(iFolderViewItem));
			
			return items;
		}
		
		public void SelectAll()
		{
Console.WriteLine("iFolderViewGroupSelection.SelectAll");
			bool bSelectionChanged = false;

			foreach(iFolderViewItem item in group.Items)
			{
				if (!item.Selected)
				{
					item.Selected = true;
					bSelectionChanged = true;
				}
			}

			if (bSelectionChanged && SelectionChanged != null)
				SelectionChanged(this, EventArgs.Empty);
		}
		
		public void UnselectAll()
		{
Console.WriteLine("iFolderViewGroupSelection.UnselectAll");
			bool bSelectionChanged = false;

			foreach(iFolderViewItem item in group.Items)
			{
Console.WriteLine("\t{0}", item.Holder.iFolder.Name);
				if (item.Selected)
				{
					item.Selected = false;
					bSelectionChanged = true;
				}
			}
			
			if (bSelectionChanged && SelectionChanged != null)
				SelectionChanged(this, EventArgs.Empty);
Console.WriteLine("\tUnselectAll returning");
		}
		
		public void SelectItem(iFolderViewItem item)
		{
			if (item != null)
			{
				// Select this item
				if (!item.Selected)
				{
					if (Mode == SelectionMode.Single)
					{
						// Unselect the old item
						iFolderViewItem oldItem;
						if (GetSelected(out oldItem))
							oldItem.Selected = false;
					}
					
					item.Selected = true;
					if (SelectionChanged != null)
						SelectionChanged(this, EventArgs.Empty);
				}
			}
		}
		
		public bool GetSelected(out iFolderViewItem itemOut)
		{
Console.WriteLine("iFolderViewGroupSelection.GetSelected");
			itemOut = null;

			// Look for the first selection
			foreach(iFolderViewItem item in group.Items)
			{
				if (item.Selected)
				{
					itemOut = item;
					return true;
				}
			}
			
			return false;
		}
		
		public bool GetSelected(out TreeModel modelOut, out iFolderViewItem itemOut)
		{
Console.WriteLine("iFolderViewGroupSelection.GetSelected2");
			modelOut = group.Model;
			
			return GetSelected(out itemOut);
		}
		
		///
		/// Utility functions
		///
	}
}