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
using Gtk;

namespace Novell.iFolder
{
	public class iFolderIconView : Gnome.IconList
	{
		private TreeModel model;
		
		private int columns;
		private SelectionMode selectionMode;
		private int pixbufColumn;
		private int textColumn;
		
		/// <summary>
		/// The default number of columns is 3.
		/// </summary>
		public int Columns
		{
			get
			{
				return columns;
			}
			set
			{
				columns = value;
			}
		}
		
		/// <summary>
		/// The default selction mode is SelectionMode.Single.
		/// </summary>
		public SelectionMode TheSelectionMode
		{
			get
			{
				return selectionMode;
			}
			set
			{
				selectionMode = value;
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
		
		
		
		public iFolderIconView(TreeModel model) :
					base(48, new Gtk.Adjustment(0,0, 50, 1,1,1), 0)
		{
		
			this.model = model;
			
			// Set up the default values of the IconView's properties
			columns = 3;
			selectionMode = SelectionMode.Single;
			pixbufColumn = 0;
			textColumn = 1;
			
			this.IconWidth = 48;
			this.ColSpacing = 8;
			
			this.Realized +=
				new EventHandler(OnWidgetRealized);
		}
		
		
		
		private void OnWidgetRealized(object o, EventArgs args)
		{
			Console.WriteLine("iFolderIconView.OnWidgetRealized");

			TreeIter iter;
			if (model.GetIterFirst(out iter))
			{
				do
				{
				
					Gdk.Pixbuf pixbuf =
						(Gdk.Pixbuf) model.GetValue(iter, pixbufColumn);
					string name =
						(string) model.GetValue(iter, textColumn);
					
					
					this.AppendPixbuf(pixbuf, "", name);
					
					// Add an icon representing
				} while (model.IterNext(ref iter));
			}
		}
		
		
		
		
		
		public void ActivateItem(TreePath path)
		{
		}
		
		public TreePath GetPathAtPos(int x, int y)
		{
			return null;
		}
		
		public bool PathIsSelected(TreePath path)
		{
			return false;
		}
		
		public void SelectAll()
		{
		}
		
		public void SelectPath(TreePath path)
		{
		}
		
		public void UnselectAll()
		{
		}
	}
}