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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for TileListViewItem.
	/// </summary>
	[Serializable]
	public class TileListViewItem : System.Windows.Forms.UserControl
	{
		#region Class Members

		public event EventHandler ItemSelected;
		private bool selected = false;
//		private bool success = false;
		private Color selectionColor = Color.FromKnownColor( KnownColor.InactiveCaptionText );
		private Color normalColor = Color.White;
		private TileListView owner;
		private int imageIndex;
		private Color activeTextColor = Color.FromKnownColor( KnownColor.ControlText );
		private Color inactiveTextColor = Color.FromKnownColor( KnownColor.InactiveCaption );
		private System.Windows.Forms.Label name;
		private System.Windows.Forms.PictureBox icon;
		private System.Windows.Forms.Label location;
		private System.Windows.Forms.Label status;

		#endregion
		private System.Windows.Forms.Timer timer1;
		private System.ComponentModel.IContainer components;

		#region Constructor

		public TileListViewItem( iFolderObject ifolderObject ) :
			this()
		{
			Tag = ifolderObject;
			ItemName = ifolderObject.iFolderWeb.Name;

			if ( ifolderObject.iFolderWeb.IsSubscription )
			{
				// TODO: Localize.
				ItemLocation = string.Format( "Owner: {0}", ifolderObject.iFolderWeb.Owner );
			}
			else
			{
				ItemLocation = ifolderObject.iFolderWeb.UnManagedPath;
			}
		}

		public TileListViewItem( string[] items, int imageIndex ) :
			this()
		{
			ImageIndex = imageIndex;
			ItemName = items.Length > 0 ? items[0] : string.Empty;
			ItemLocation = items.Length > 1 ? items[1] : string.Empty;
			Status = items.Length > 2 ? items[2] : string.Empty;
		}

		private TileListViewItem()
		{
			InitializeComponent();
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
			this.components = new System.ComponentModel.Container();
			this.icon = new System.Windows.Forms.PictureBox();
			this.name = new System.Windows.Forms.Label();
			this.location = new System.Windows.Forms.Label();
			this.status = new System.Windows.Forms.Label();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// icon
			// 
			this.icon.Location = new System.Drawing.Point(8, 12);
			this.icon.Name = "icon";
			this.icon.Size = new System.Drawing.Size(48, 48);
			this.icon.TabIndex = 0;
			this.icon.TabStop = false;
			this.icon.MouseEnter += new System.EventHandler(this.TileListViewItem_MouseEnter);
			this.icon.MouseLeave += new System.EventHandler(this.TileListViewItem_MouseLeave);
			this.icon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TileListViewItem_MouseDown);
			// 
			// name
			// 
			this.name.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.name.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.name.Location = new System.Drawing.Point(64, 12);
			this.name.Name = "name";
			this.name.Size = new System.Drawing.Size(206, 20);
			this.name.TabIndex = 1;
			this.name.Text = "Name";
			this.name.MouseEnter += new System.EventHandler(this.TileListViewItem_MouseEnter);
			this.name.MouseLeave += new System.EventHandler(this.TileListViewItem_MouseLeave);
			this.name.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TileListViewItem_MouseDown);
			// 
			// location
			// 
			this.location.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.location.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.location.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
			this.location.Location = new System.Drawing.Point(64, 32);
			this.location.Name = "location";
			this.location.Size = new System.Drawing.Size(206, 16);
			this.location.TabIndex = 2;
			this.location.Text = "location";
			this.location.MouseEnter += new System.EventHandler(this.TileListViewItem_MouseEnter);
			this.location.MouseLeave += new System.EventHandler(this.TileListViewItem_MouseLeave);
			this.location.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TileListViewItem_MouseDown);
			// 
			// status
			// 
			this.status.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.status.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.status.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
			this.status.Location = new System.Drawing.Point(64, 48);
			this.status.Name = "status";
			this.status.Size = new System.Drawing.Size(206, 16);
			this.status.TabIndex = 3;
			this.status.Text = "status";
			this.status.MouseEnter += new System.EventHandler(this.TileListViewItem_MouseEnter);
			this.status.MouseLeave += new System.EventHandler(this.TileListViewItem_MouseLeave);
			this.status.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TileListViewItem_MouseDown);
			// 
			// timer1
			// 
			this.timer1.Interval = 60000;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// TileListViewItem
			// 
			this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.Controls.Add(this.status);
			this.Controls.Add(this.location);
			this.Controls.Add(this.name);
			this.Controls.Add(this.icon);
			this.Name = "TileListViewItem";
			this.Size = new System.Drawing.Size(280, 72);
			this.MouseEnter += new System.EventHandler(this.TileListViewItem_MouseEnter);
			this.MouseHover += new System.EventHandler(this.TileListViewItem_MouseHover);
			this.MouseLeave += new System.EventHandler(this.TileListViewItem_MouseLeave);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TileListViewItem_MouseDown);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers

		private void TileListViewItem_MouseEnter(object sender, System.EventArgs e)
		{
			if ( !selected )
			{
				location.ForeColor = status.ForeColor = activeTextColor;
			}

			// TODO: Change the icon.
		}

		private void TileListViewItem_MouseLeave(object sender, System.EventArgs e)
		{
			if ( !selected )
			{
				location.ForeColor = status.ForeColor = inactiveTextColor;
			}

			// TODO: Change the icon.
		}

		private void TileListViewItem_MouseHover(object sender, System.EventArgs e)
		{
//			location.ForeColor = Color.FromKnownColor( KnownColor.ControlText );
//			status.ForeColor = Color.FromKnownColor( KnownColor.ControlText );
		}

		private void TileListViewItem_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if ( !Selected )
			{
				Selected = true;
			}
		}

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			// TODO: if we want to show the fancy sync times.
/*			if ( success )
			{
				iFolderWeb ifolder = ((iFolderObject)Tag).iFolderWeb;
				status.Text = "Synchronized: less than a minute ago";
			}*/
		}

		#endregion

		#region Properties

		public TileListView iFoldersListView
		{
			get { return owner; }
		}

/*		public iFolderState iFolderState
		{
			get { return ((iFolderObject)Tag).iFolderState; }
			set
			{
				iFolderObject ifolder = (iFolderObject)Tag;
				ifolder.iFolderState = value;
				switch ( value )
				{
					case iFolderState.SynchronizingLocal:
					case iFolderState.Synchronizing:
						ImageIndex = 1;
						break;
					case iFolderState.Normal:
						if ( ifolder.iFolderWeb.IsSubscription )
						{
							ImageIndex = 2;
						}
						else
						{
							switch ( ifolder.iFolderWeb.State )
							{
								case "Local":
									ImageIndex = 0;
									break;
								default:
									ImageIndex = 4;
									break;
							}
						}
						break;
					case iFolderState.FailedSync:
					case iFolderState.Disconnected:
						ImageIndex = 5;
						break;
				}
			}
		}*/

		public int ImageIndex
		{
			get { return imageIndex; }
			set 
			{ 
				imageIndex = value;
				if ( owner != null )
				{
					icon.Image = ImageList.Images[ imageIndex ];
				}
			}
		}

		public ImageList ImageList
		{
			get { return owner.LargeImageList; }
		}

		public string ItemLocation
		{
			get { return location.Text; }
			set { location.Text = value; }
		}

		public string ItemName
		{
			get { return name.Text; }
			set { name.Text = value; }
		}

		public string Status
		{
			get { return status.Text; }
			set 
			{
/*				if ( value.Equals( "success" ) )
				{
					timer1.Start();
					success = true;
					status.Text = "Synchronized: less than a minute ago";
				}
				else*/
				{
//					timer1.Stop();
//					success = false;
					status.Text = value;
				}
			}
		}

		internal TileListView Owner
		{
			set 
			{
				owner = value;
				ImageIndex = imageIndex;
			}
		}

		public bool Selected
		{
			get
			{
				return selected;
			}
			set
			{
				selected = value;
				if ( selected )
				{
					BackColor = selectionColor;
					name.ForeColor = location.ForeColor = status.ForeColor =  normalColor;

					
					if (ItemSelected != null)
					{
						ItemSelected(this, new EventArgs());
					}
				}
				else
				{
					BackColor = normalColor;
					name.ForeColor = activeTextColor;
					location.ForeColor = status.ForeColor = inactiveTextColor;
				}
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Removes the item from its associated list view control.
		/// </summary>
		public void Remove()
		{
			owner.Items.Remove( this );
		}

		#endregion
	}
}
