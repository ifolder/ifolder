/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Bruce Getter <bgetter@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Collections;

namespace Novell.FormsTrayApp
{
    /// <summary>
    /// class ListViewArrayList
    /// </summary>
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
        /// <summary>
        /// Owner of TileListView
        /// </summary>
		private TileListView owner;
		private ArrayList list = new ArrayList();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="owner">TileListview owner</param>
		public TileListViewItemCollection( TileListView owner )
		{
			this.owner = owner;
		}

		#region IList Members

        /// <summary>
        /// Gets if its read only
        /// </summary>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

        /// <summary>
        /// Sort the array
        /// </summary>
		public void Sort()
		{
			IComparer myComparer = new ListViewArrayList();
			list.Sort(myComparer);
		}

        /// <summary>
        /// Gets / Sets the TileListViewItem item
        /// </summary>
        /// <param name="index">index of the item</param>
        /// <returns>TileLIstViewItem object</returns>
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

        /// <summary>
        /// RemoevAt
        /// </summary>
        /// <param name="index">index of the item</param>
		public void RemoveAt(int index)
		{
			Remove( (TileListViewItem)list[index] );
		}

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="index">index of the item</param>
        /// <param name="value">TileListViewItem to be inserted</param>
		public void Insert(int index, TileListViewItem value)
		{
			value.Owner = this.owner;
			value.ItemSelected += new EventHandler(owner.item_Selected);
			value.DoubleClick += new EventHandler(owner.item_DoubleClick);
			owner.Controls.Add( value );
			owner.ReCalculateItems();
			list.Insert( index, value );
		}

        /// <summary>
        /// Remove the TileListViewItem
        /// </summary>
        /// <param name="value">TileListViewItem which is being removed</param>
		public void Remove(TileListViewItem value)
		{
			list.Remove( value );
			owner.Controls.Remove( value );
			owner.ReCalculateItems();
		}

        /// <summary>
        /// Contains
        /// </summary>
        /// <param name="value">TileListViewItem object</param>
        /// <returns>true if contains</returns>
		public bool Contains(TileListViewItem value)
		{
			return list.Contains( value );
		}

        /// <summary>
        /// Clear
        /// </summary>
		public void Clear()
		{
			list.Clear();
			owner.Controls.Clear();
			if(owner.isReCalculateNeeded)
				owner.ReCalculateItems();
		}

        /// <summary>
        /// Index Of
        /// </summary>
        /// <param name="value">TileListViewItem object</param>
        /// <returns>index of the object</returns>
		public int IndexOf(TileListViewItem value)
		{
			return list.IndexOf( value );
		}

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="value">TileListViewItem</param>
        /// <returns>TileListViewItem</returns>
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

        /// <summary>
        /// Gets the count int the list
        /// </summary>
		public int Count
		{
			get
			{
				return list.Count;
			}
		}

        /// <summary>
        /// Copy to
        /// </summary>
        /// <param name="array">Destination array</param>
        /// <param name="index">index in the array</param>
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
