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
	public delegate void iFolderItemActivatedHandler(object o, iFolderItemActivatedArgs args);

	public class iFolderIconView : Gnome.IconList
	{
		private TreeModel 	model;
		
		private int 		numOfColumns;
		private int 		pixbufColumn;
		private int 		textColumn;
		
		private int 		columnSpacing;
		
		private Hashtable	treePaths;
		
		// FIXME: Fix this hack when the ItemSelected & ItemUnselected events don't cause crashes
		private int			currentSelection;
		
		public event iFolderItemActivatedHandler	ItemActivated;
		public event System.EventHandler			SelectionChanged;
		
		
		/// <summary>
		/// The default number of columns is 3.
		/// </summary>
		public int Columns
		{
			get
			{
				return numOfColumns;
			}
			set
			{
				numOfColumns = value;
			}
		}
		
		/// <summary>
		/// Contains the index of the TreeModel column containing the pixbufs
		/// which should be displayed.  The default index is 0.
		/// </summary>
		public int PixbufColumn
		{
			get
			{
				return pixbufColumn;
			}
			set
			{
				pixbufColumn = value;
			}
		}
		
		/// <summary>
		/// Contains the index of the TreeModel column containing the texts
		/// which should be displayed.  The default index is 1.
		/// </summary>
		public int TextColumn
		{
			get
			{
				return textColumn;
			}
			set
			{
				textColumn = value;
			}
		}
		
		public TreeModel Model
		{
			get
			{
				return model;
			}
		}
		
		public TreePath[] SelectedItems
		{
			get
			{
				TreePath[] treePathA = new TreePath[this.Selection.Length];
				
				for (int i = 0; i < this.Selection.Length; i++)
				{
					treePathA[i] = (TreePath)treePaths[this.Selection[i]];
				}
				
				return treePathA;
			}
		}
		
		
		
		public iFolderIconView(TreeModel model) :
					base(48, new Gtk.Adjustment(0,0, 50, 1,1,1), 0)
		{
		
			this.model = model;
			
			this.columnSpacing = 24;
			this.ColSpacing = columnSpacing;
			this.currentSelection = -1;
			
			treePaths = new Hashtable();
			
			numOfColumns = 3;
			pixbufColumn = 0;
			textColumn = 1;
			
			this.Separators = " ";	// Characters that can be used as
									// separators in the icon captions.
			
			this.Realized +=
				new EventHandler(OnWidgetRealized);
		}
		
		
		
		private void OnWidgetRealized(object o, EventArgs args)
		{
			Console.WriteLine("iFolderIconView.OnWidgetRealized");

			RefreshIcons();
		}
		
		
		
		public void RefreshIcons()
		{
			treePaths.Clear();
			this.Clear();

			TreeIter iter;
			if (model.GetIterFirst(out iter))
			{
				do
				{
					Gdk.Pixbuf pixbuf =
						(Gdk.Pixbuf) model.GetValue(iter, pixbufColumn);
					string name =
						(string) model.GetValue(iter, textColumn);
					
					int pos = this.AppendPixbuf(pixbuf, "", name);
					treePaths[pos] = model.GetPath(iter);
					
					// Add an icon representing
				} while (model.IterNext(ref iter));
			}
		}
		
		
		
		
		
		public void ActivateItem(TreePath path)
		{
			if (ItemActivated != null)
			{
				ItemActivated(this, new iFolderItemActivatedArgs(path));
			}
		}
		
		public TreePath GetPathAtPos(int x, int y)
		{
			TreePath path = null;

			int pos = this.GetIconAt(x, y);
			if (pos >= 0 && treePaths.ContainsKey(pos))
				path = (TreePath) treePaths[pos];

			return path;
		}
		
		public bool PathIsSelected(TreePath path)
		{
			for (int i = 0; i < this.Selection.Length; i++)
			{
				TreePath tempPath =
					(TreePath)treePaths[this.Selection[i]];
				if (tempPath == path)
					return true;
			}
			
			return false;
		}
		
		public void SelectPath(TreePath path)
		{
			for (int i = 0; i < (int)this.NumIcons; i++)
			{
				TreePath tempPath =
					(TreePath)treePaths[i];
				if (tempPath == path)
				{
					this.SelectIcon(i);
					break;
				}
			}
		}


//		protected override void OnIconSelected(int num, Gdk.Event evnt)
//		{
//			base.OnIconSelected(num, evnt);

//			if (SelectionChanged == null) return;

//			SelectionChanged(this, EventArgs.Empty);
//		}

//		protected override void OnIconUnselected(int num, Gdk.Event evnt)
//		{
//			base.OnIconSelected(num, evnt);

//			if (SelectionChanged == null) return;
			
//			if (this.Selection.Length == 0)
//				SelectionChanged(this, EventArgs.Empty);
//		}
		
		protected override void OnSizeAllocated(Gdk.Rectangle sized)
		{
			base.OnSizeAllocated(sized);
			
			int widthConsumedByColSpacing = ((numOfColumns + 2) * columnSpacing);
			int availableWidthForIcons = (int)sized.Width - widthConsumedByColSpacing;
				
			int columnWidth = availableWidthForIcons / numOfColumns;
			int totalWidth = (columnWidth * numOfColumns) + widthConsumedByColSpacing;
				
			if (totalWidth > sized.Width)
			{
				columnWidth--;
			}

			this.IconWidth = columnWidth;
		}
		
		private bool DidSelectionChange()
		{
			bool bSelectionChanged = false;

			if (currentSelection >= 0)
			{
				// There was previously a valid selection
				if (this.Selection.Length == 0)
				{
					currentSelection = -1;
					bSelectionChanged = true;
				}
				else
				{
					if (this.Selection[0] != currentSelection)
					{
						currentSelection = this.Selection[0];
						bSelectionChanged = true;
					}
				}
			}
			else
			{
				// There was previously no selection
				if (this.Selection.Length > 0)
				{
					currentSelection = this.Selection[0];
					bSelectionChanged = true;
				}
			}
			
			return bSelectionChanged;
		}

		/// <summary>
		/// This is overridden so that ButtonPressEvent can be listened for by
		/// the parent of this widget.
		/// </summary>		
		protected override bool OnButtonPressEvent(Gdk.EventButton evnt)
		{
			base.OnButtonPressEvent(evnt);

			if (this.DidSelectionChange() && SelectionChanged != null)
				SelectionChanged(this, EventArgs.Empty);

			return false;
		}
		
		protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
		{
			base.OnKeyPressEvent(evnt);

			// If the key pressed is on of the arrow keys, check to see
			// if a SelectionChanged event should be raised.
			switch(evnt.Key)
			{
				case Gdk.Key.Up:
				case Gdk.Key.Down:
				case Gdk.Key.Left:
				case Gdk.Key.Right:
					if (DidSelectionChange() && SelectionChanged != null)
						SelectionChanged(this, EventArgs.Empty);
					break;

				case Gdk.Key.Return:
					// FIXME: Raise an ItemActivated event if an icon is selected
					if (this.Selection.Length == 1)
						this.ActivateItem((TreePath)treePaths[this.Selection[0]]);
					break;

				default:
					break;
			}
			
			return false;
		}
	}
	
	public class iFolderItemActivatedArgs
	{
		private TreePath treePath;
		
		public TreePath Path
		{
			get
			{
				return treePath;
			}
		}
		
		public iFolderItemActivatedArgs(TreePath path)
		{
			this.treePath = path;
		}
	}
}