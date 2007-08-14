/****************************************************************************
 |
 | Copyright (c) [2007] Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 | Author: Bruce Getter <bgetter@novell.com>
 |
 |***************************************************************************/

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
	/// Summary description for iFoldersListView.
	/// </summary>
	public class iFoldersListView : System.Windows.Forms.UserControl
	{
		#region Class Members

		private DomainInformation domainInfo;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private Novell.FormsTrayApp.TileListView tileListView1;
		private static System.Resources.ResourceManager Resource = new System.Resources.ResourceManager( typeof(FormsTrayApp));
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#endregion

		#region Constructor

		public iFoldersListView( DomainInformation domainInfo, ImageList largeImageList )
		{
			this.domainInfo = domainInfo;

			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			tileListView1.LargeImageList = largeImageList;

			this.richTextBox1.Text = string.Format( richTextBox1.Text, domainInfo.Name );
			Graphics g = richTextBox1.CreateGraphics();
			try
			{
				SizeF textSize = g.MeasureString(richTextBox1.Text, richTextBox1.Font);
				this.Width = (int)(textSize.Width * 1.1);
			}
			finally
			{
				g.Dispose();
			}

			base.DoubleClick += new EventHandler(iFoldersListView_DoubleClick);
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

		private void iFoldersListView_DoubleClick(object sender, EventArgs e)
		{
			if ( DoubleClick != null )
			{
				DoubleClick( sender, e );
			}
		}

		private void tileListView1_DoubleClick(object sender, System.EventArgs e)
		{
			if ( DoubleClick != null )
			{
				DoubleClick( sender, e );
			}
		}

		private void tileListView1_SizeChanged(object sender, System.EventArgs e)
		{
			Height = richTextBox1.Height + tileListView1.Height;
		}

		private void tileListView1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if ( SelectedIndexChanged != null )
			{
				SelectedIndexChanged( sender, e );
			}
		}

		private void tileListView1_LastItemRemoved(object sender, System.EventArgs e)
		{
			if ( LastItemRemoved != null )
			{
				LastItemRemoved( sender, e );
			}
		}

		private bool tileListView1_NavigateItem(object sender, Novell.FormsTrayApp.NavigateItemEventArgs e)
		{
			if ( NavigateItem != null )
			{
				return NavigateItem( this, e );
			}

			return false;
		}

		private void listView1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
//			Point point = new Point(e.X, e.Y);
//			point = listView1.PointToClient(point);
//			ListViewItem lvi = listView1.GetItemAt(point.X, point.Y);
//			if ( ItemSelected != null )
//			{
//				ItemSelectedEventArgs args = new ItemSelectedEventArgs( lvi );
//				ItemSelected( this, args );
//			}
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
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.tileListView1 = new Novell.FormsTrayApp.TileListView();
			this.SuspendLayout();
			// 
			// richTextBox1
			// 
			this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox1.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.richTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.richTextBox1.ForeColor = System.Drawing.SystemColors.Desktop;
			this.richTextBox1.Location = new System.Drawing.Point(0, 0);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new System.Drawing.Size(288, 40);
			this.richTextBox1.TabIndex = 0;
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.TabStop = false;
			this.richTextBox1.Text = Resource.GetString("iFoldersOnServerHeading");
			// 
			// tileListView1
			// 
			this.tileListView1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.tileListView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tileListView1.HorizontalSpacing = 5;
			this.tileListView1.ItemHeight = 72;
			this.tileListView1.ItemWidth = 280;
			this.tileListView1.LargeImageList = null;
			this.tileListView1.Location = new System.Drawing.Point(0, 40);
			this.tileListView1.Name = "tileListView1";
			this.tileListView1.SelectedItem = null;
			this.tileListView1.Size = new System.Drawing.Size(288, 32);
			this.tileListView1.TabIndex = 1;
			this.tileListView1.VerticleSpacing = 5;
			this.tileListView1.LastItemRemoved += new Novell.FormsTrayApp.TileListView.LastItemRemovedDelegate(this.tileListView1_LastItemRemoved);
			this.tileListView1.NavigateItem += new Novell.FormsTrayApp.TileListView.NavigateItemDelegate(this.tileListView1_NavigateItem);
			this.tileListView1.SizeChanged += new System.EventHandler(this.tileListView1_SizeChanged);
			this.tileListView1.DoubleClick += new System.EventHandler(this.tileListView1_DoubleClick);
			this.tileListView1.SelectedIndexChanged += new Novell.FormsTrayApp.TileListView.SelectedIndexChangedDelegate(this.tileListView1_SelectedIndexChanged);
			// 
			// iFoldersListView
			// 
			this.Controls.Add(this.tileListView1);
			this.Controls.Add(this.richTextBox1);
			this.Name = "iFoldersListView";
			this.Size = new System.Drawing.Size(288, 72);
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties

		/// <summary>
		/// Gets the DomainInformation.
		/// </summary>
		public DomainInformation DomainInfo
		{
			get
			{
				return domainInfo;
			}
		}

		/// <summary>
		/// Gets the TileListViewItemCollection.
		/// </summary>
		public TileListViewItemCollection Items
		{
			get { return tileListView1.Items; }
		}

		/// <summary>
		/// Gets/sets the selected item in the listview.
		/// </summary>
		public TileListViewItem SelectedItem
		{
			get { return tileListView1.SelectedItem; }
			set { tileListView1.SelectedItem = value; }
		}

		public bool setVisible
		{
			get
			{
				return this.richTextBox1.Visible;
			}
			set
			{
				this.richTextBox1.Visible = value;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Add an iFolder to the listview.
		/// </summary>
		/// <param name="ifolderObject">The iFolder object to add to the listview.</param>
		/// <returns>A TileListViewItem object which represents the iFolder.</returns>
		public TileListViewItem AddiFolderToListView( iFolderObject ifolderObject )
		{
			TileListViewItem tlvi = null;

			tlvi = new TileListViewItem( ifolderObject );
			tlvi = tileListView1.Items.Add( tlvi );
			tileListView1.Items.Sort();
			return tlvi;
		}

		/// <summary>
		/// Finalizes an update.
		/// </summary>
		public void FinalizeUpdate()
		{
			this.ResumeLayout();
		}

		/// <summary>
		/// Initializes an update.
		/// </summary>
		public void InitializeUpdate()
		{
			tileListView1.Items.Clear();

			this.SuspendLayout();
		}

		/// <summary>
		/// Move to a specific item in the listview.
		/// </summary>
		/// <param name="row">The row of the item to move to.</param>
		/// <param name="column">The column of the item to move to.</param>
		/// <returns><b>True</b> if successful in moving to the object; otherwise, <b>False</b> is returned.</returns>
		public bool MoveToItem( int row, int column )
		{
			return tileListView1.MoveToItem( row, column );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="iFolderID"></param>
		/// <param name="state"></param>
		public void UpdateiFolderStatus( string iFolderID, iFolderState state )
		{
			// TODO:
		}

		#endregion
	}
}
