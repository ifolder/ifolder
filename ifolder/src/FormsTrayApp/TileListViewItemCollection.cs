/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004-2006 Novell, Inc.
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
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;

namespace Novell.FormsTrayApp
{
	public class ListViewArrayList : IComparer
	{
		int IComparer.Compare(Object X, Object Y)
		{
			string str1 = ((iFolderObject)((TileListViewItem) X).Tag).iFolderWeb.Name;
			string str2 = ((iFolderObject)((TileListViewItem) Y).Tag).iFolderWeb.Name;
			return String.Compare(str1, str2 );
		}
	}
	/// <summary>
	/// Summary description for TileListViewItemCollection.
	/// </summary>
	public class TileListViewItemCollection 
	{
		private TileListView owner;
		private ArrayList list = new ArrayList();

		public TileListViewItemCollection( TileListView owner )
		{
			this.owner = owner;
		}

		#region IList Members

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public void Sort()
		{
			IComparer myComparer = new ListViewArrayList();
			list.Sort(myComparer);
		}

		public TileListViewItem this[int index]
		{
			get
			{
				return (TileListViewItem)list[index];
			}
			set
			{
				list[index] = value;
			}
		}

		public void RemoveAt(int index)
		{
			Remove( (TileListViewItem)list[index] );
		}

		public void Insert(int index, TileListViewItem value)
		{
			value.Owner = this.owner;
			value.ItemSelected += new EventHandler(owner.item_Selected);
			value.DoubleClick += new EventHandler(owner.item_DoubleClick);
			owner.Controls.Add( value );
			owner.ReCalculateItems();
			list.Insert( index, value );
		}

		public void Remove(TileListViewItem value)
		{
			list.Remove( value );
			owner.Controls.Remove( value );
			owner.ReCalculateItems();
		}

		public bool Contains(TileListViewItem value)
		{
			return list.Contains( value );
		}

		public void Clear()
		{
			list.Clear();
			owner.Controls.Clear();
			if(owner.isReCalculateNeeded)
				owner.ReCalculateItems();
		}

		public int IndexOf(TileListViewItem value)
		{
			return list.IndexOf( value );
		}

		public TileListViewItem Add(TileListViewItem value)
		{
			list.Add( value );
			value.Owner = this.owner;
			value.ItemSelected += new EventHandler(owner.item_Selected);
			value.DoubleClick += new EventHandler(owner.item_DoubleClick);
			owner.Controls.Add( value );
			owner.ReCalculateItems();
			return value;
		}

		#endregion

		#region ICollection Members

		public int Count
		{
			get
			{
				return list.Count;
			}
		}

		public void CopyTo(Array array, int index)
		{
			list.CopyTo( array, index );
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return list.GetEnumerator();
		}

		#endregion
	}
}
