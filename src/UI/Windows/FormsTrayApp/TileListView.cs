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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using Simias.Client;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for TileListView.
	/// </summary>
	public class TileListView : System.Windows.Forms.UserControl
	{
		#region Class Members

		private int itemsPerRow;
		private int horizontalSpacing = 5;
		private int verticleSpacing = 5;
		private int itemWidth = 280;
		private int itemHeight = 72;
		private TileListViewItemCollection items;
		private TileListViewItem selectedItem = null;
		private ImageList largeImageList;
		private Size oldSize;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#endregion

		#region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
		public TileListView()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			items = new TileListViewItemCollection( this );
			oldSize = Size;
		}

		#endregion

		#region Overrides

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TileListView));
			// 
			// TileListView
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackColor = System.Drawing.SystemColors.Window;
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.Name = "TileListView";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
			this.Click += new System.EventHandler(this.TileListView_Click);
			this.SizeChanged += new System.EventHandler(this.TileListView_SizeChanged);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TileListView_MouseDown);

		}
		#endregion

		#region Events

		public new event EventHandler DoubleClick;

		public delegate bool NavigateItemDelegate( object sender, NavigateItemEventArgs e );
		public event NavigateItemDelegate NavigateItem;

		public delegate void LastItemRemovedDelegate( object sender, EventArgs e);
		public event LastItemRemovedDelegate LastItemRemoved;

		/// <summary>
		/// Delegate used when an item is selected.
		/// </summary>
		public delegate void SelectedIndexChangedDelegate( object sender, EventArgs e );
		/// <summary>
		/// Occurs when an item is selected.
		/// </summary>
		public event SelectedIndexChangedDelegate SelectedIndexChanged;

		#endregion

		#region Event Handlers

        /// <summary>
        /// Event Handler for tile list view click event
        /// </summary>
        private void TileListView_Click(object sender, System.EventArgs e)
		{
			if ( selectedItem != null )
			{
				selectedItem.Selected = false;
				selectedItem = null;
			}

			if ( SelectedIndexChanged != null )
			{
				SelectedIndexChanged( this, e );
			}
		}

		private void TileListView_DoubleClick(object sender, System.EventArgs e)
		{
			if ( DoubleClick != null )
			{
				DoubleClick( sender, e );
			}
		}

		private void TileListView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if ( selectedItem != null )
			{
				selectedItem.Selected = false;
				selectedItem = null;
			}

			if ( SelectedIndexChanged != null )
			{
				SelectedIndexChanged( this, new EventArgs() );
			}
		}

        /// <summary>
        /// Event Handler for tile list view size changed event
        /// </summary>
        private void TileListView_SizeChanged(object sender, System.EventArgs e)
		{
			if ( isReCalculateNeeded )
			{
				ReCalculateItems();
			}

			oldSize = Size;
		}
		
		internal void item_DoubleClick(object sender, EventArgs e)
		{
			if ( DoubleClick != null )
			{
				DoubleClick( sender, e );
			}
		}

		internal void item_Selected(object sender, EventArgs e)
		{
			TileListViewItem tlvi = (TileListViewItem)sender;
			
			if (selectedItem != null)
			{
				if ( !tlvi.Equals(selectedItem) )
				{
					selectedItem.Selected = false;
				}
			}
			selectedItem = tlvi;

			if ( SelectedIndexChanged != null )
			{
				SelectedIndexChanged( this, e );
			}
		}

		#endregion

		#region Private Methods

        /// <summary>
        /// Adjust the height
        /// </summary>
		private void adjustHeight()
		{
			if (items.Count == 0)
			{
				if ( LastItemRemoved != null )
				{
					LastItemRemoved( this, new EventArgs() );
				}

				Height = 50;
			}
			else
			{
				int height = 0;
				height = HeightToShowAll;
				if(Height != height + 10)
					Height = height + 10;
			}
		}
		
        /// <summary>
        /// Returns the location of the item
        /// </summary>
        /// <param name="index">index of the item</param>
        /// <returns>POint - The location of the item</returns>
		private System.Drawing.Point getItemLocation(int index)
		{
			int rowIndex = index / itemsPerRow;
			int colIndex = index - rowIndex * itemsPerRow;
			Point p = new Point(
				colIndex * ( ItemWidth + verticleSpacing ) + 10,
				rowIndex * ( ItemHeight + horizontalSpacing ) + 10 );
			return p;
		}
		
		#endregion

		#region Properties

        /// <summary>
        /// Gets the Height to show all
        /// </summary>
		public int HeightToShowAll
		{
			get
			{
				System.Drawing.Point p = getItemLocation(items.Count - 1);
				return p.Y + ItemHeight + horizontalSpacing;
			}
		}

        /// <summary>
        /// Gets / Sets the Horixontal spacing
        /// </summary>
		public int HorizontalSpacing
		{
			get
			{
				return horizontalSpacing;
			}
			set
			{
				horizontalSpacing = value;
			}
		}

        /// <summary>
        /// Gets if recalculate id needed
        /// </summary>
		public bool isReCalculateNeeded
		{
			get
			{
				int oldItemsPerRow = (oldSize.Width - 20) / ItemWidth;
				if ( itemsPerRow == oldItemsPerRow)
				{
					return false;
				}	
				else
					return true;
			}
		}

        /// <summary>
        /// Gets the item height
        /// </summary>
		public int ItemHeight
		{
			get
			{
				return itemHeight;
			}
			set
			{
				itemHeight = value;
			}
		}

        /// <summary>
        /// Gets the TileListViewCollItems
        /// </summary>
		public TileListViewItemCollection Items
		{
			get { return items; }
		}

        /// <summary>
        /// Gets / Sets the item width
        /// </summary>
		public int ItemWidth
		{
			get
			{
				return itemWidth;
			}
			set
			{
				itemWidth = value;
			}
		}
		
        /// <summary>
        /// Gets  / Sets the Image list
        /// </summary>
		public ImageList LargeImageList
		{
			get { return largeImageList; }
			set { largeImageList = value; }
		}

        /// <summary>
        /// Gets / Sets the selected item
        /// </summary>
		public TileListViewItem SelectedItem
		{
			get { return selectedItem; }
			set { selectedItem = value; }
		}

		/// <summary>
		/// Gets / Sets the vertical spacing
		/// </summary>
        public int VerticleSpacing
		{
			get
			{
				return verticleSpacing;
			}
			set
			{
				verticleSpacing = value;
			}
		}

		#endregion

		#region Public Methods
        /// <summary>
        /// Move to Item
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <returns>true on success</returns>
		public bool MoveToItem( int row, int column )
		{
			bool result = false;
			bool traversingListView = false;

			if ( row == -1 )
			{
				traversingListView = true;
				row = Items.Count / itemsPerRow;
			}

			if ( column < itemsPerRow )
			{
				int index = row * itemsPerRow + column;
				if ( ( index >= Items.Count ) && traversingListView )
				{
					if ( --row >= 0 )
					{
						index = row * itemsPerRow + column;
					}
				}

				if ( index < Items.Count )
				{
					TileListViewItem tlvi = Items[ index ];
					tlvi.Selected = true;
					tlvi.Focus();

					result = true;
				}
			}

			return result;
		}

        /// <summary>
        /// Gets the position of the Item
        /// </summary>
        /// <param name="tlvi"></param>
        /// <param name="row">Row number</param>
        /// <param name="column">Column Number</param>
		private void getItemPosition( TileListViewItem tlvi, out int row, out int column )
		{
			int index = Items.IndexOf( tlvi );
			if ( itemsPerRow == 0 )
				itemsPerRow = 1;

			row = index / itemsPerRow;
			column = index - row * itemsPerRow;
		}

        /// <summary>
        /// Move Down
        /// </summary>
        /// <param name="tlvi">Tile List View Item</param>
		internal void MoveDown( TileListViewItem tlvi )
		{
			int row;
			int column;
			getItemPosition( tlvi, out row, out column );

			if ( !MoveToItem( ++row, column ) && NavigateItem != null )
			{
				NavigateItem( this, new NavigateItemEventArgs( 0, column, MoveDirection.Down ) );
			}
		}

        /// <summary>
        /// Move Left
        /// </summary>
        /// <param name="tlvi">Tile List View Item</param>
		internal void MoveLeft( TileListViewItem tlvi )
		{
			int row;
			int column;
			getItemPosition( tlvi, out row, out column );

			if ( column > 0 )
				column--;

			MoveToItem( row, column );
		}

        /// <summary>
        /// Move Right
        /// </summary>
        /// <param name="tlvi">Tile List View Item</param>
		internal void MoveRight( TileListViewItem tlvi )
		{
			int row;
			int column;
			getItemPosition( tlvi, out row, out column );

			if ( column < itemsPerRow - 1 )
				column++;

			if ( !MoveToItem( row, column ) )
			{
				if ( NavigateItem == null || 
					!NavigateItem( this, new NavigateItemEventArgs( 0, column, MoveDirection.Right ) ) )
				{
					row--;
					if ( row >= 0 )
					{
						MoveToItem( row, column );
					}
				}
			}
		}

        /// <summary>
        /// Move Up
        /// </summary>
        /// <param name="tlvi">Tile List View Item</param>
		internal void MoveUp( TileListViewItem tlvi )
		{
			int row;
			int column;
			getItemPosition( tlvi, out row, out column );

			row--;

			if ( row < 0 )
			{
				if ( NavigateItem != null )
				{
					NavigateItem( this, new NavigateItemEventArgs( row, column, MoveDirection.Up ) );
				}
			}
			else
			{
				MoveToItem( row, column );
			}
		}

        /// <summary>
        /// Recalculate size
        /// </summary>
		public void ReCalculateItems()
		{
			SuspendLayout();
			
			itemsPerRow = (Width - 20) / ItemWidth;
			if ( itemsPerRow == 0 )
			{
				itemsPerRow = 1;
			}

			TileListViewItem tlvi;
			for (int i = 0; i < items.Count; i++)
			{
				tlvi = items[i];
				tlvi.Location = getItemLocation(i);
			}
			adjustHeight();
			
			ResumeLayout();
		}

		#endregion
	}
}
