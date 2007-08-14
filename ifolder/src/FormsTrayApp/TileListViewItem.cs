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

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for TileListViewItem.
	/// </summary>
	[Serializable]
	public class TileListViewItem : System.Windows.Forms.UserControl
	{
		#region Class Members

		private ToolTip toolTip;
		private bool selected = false;
		private Image icon;
		private Rectangle iconRect;
		//private PictureBox picture;
		private SolidBrush nameBrush;
		private Font nameFont;
		private string completeName = string.Empty;
		private Rectangle nameRect;
		private SolidBrush descriptionBrush;
		private Font descriptionFont;
		private string completeLocation = string.Empty;
		private Rectangle locationRect;
		private string completeStatus = string.Empty;
		private Rectangle statusRect;
//		private bool success = false;
		private Color selectionColor = Color.FromKnownColor( KnownColor.InactiveCaption );
		private Color normalColor = Color.White;
		private Color activeTextColor = Color.FromKnownColor( KnownColor.ControlText );
		private Color inactiveTextColor = Color.FromKnownColor( KnownColor.InactiveCaptionText );
		private TileListView owner;
		private int imageIndex;
		private System.Windows.Forms.Timer timer1;
		private System.ComponentModel.IContainer components;

		#endregion

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

			iconRect = new Rectangle( 8, 12, 48, 48 );
			nameRect = new Rectangle( 64, 12, 206, 20 );
			locationRect = new Rectangle( 64, 32, 206, 16 );
			statusRect = new Rectangle( 64, 48, 206, 16 );

			nameBrush = new SolidBrush( activeTextColor );
			nameFont = new Font( "Microsoft Sans Serif", 11, FontStyle.Bold );
			descriptionBrush = new SolidBrush( inactiveTextColor );
			descriptionFont = new Font( "Microsoft Sans Serif", 8, FontStyle.Regular );

			// Minimize flicker during repainting.
			SetStyle( ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer | ControlStyles.UserPaint, true );

			toolTip = new ToolTip();
			toolTip.Active = true;
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

		protected override bool IsInputKey(Keys keyData)
		{
			if ( (keyData & Keys.Up) == Keys.Up ||
				(keyData & Keys.Down) == Keys.Down ||
				(keyData & Keys.Left) == Keys.Left ||
				(keyData & Keys.Right) == Keys.Right ||
				(keyData & Keys.PageDown) == Keys.PageDown ||
				(keyData & Keys.PageUp) == Keys.PageUp )
			{
				return true;
			}
			else
			{
				return base.IsInputKey (keyData);
			}
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TileListViewItem));
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			// 
			// timer1
			// 
			this.timer1.Interval = 60000;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// TileListViewItem
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.Name = "TileListViewItem";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.TileListViewItem_Paint);
			this.MouseEnter += new System.EventHandler(this.TileListViewItem_MouseEnter);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TileListViewItem_KeyDown);
			this.MouseLeave += new System.EventHandler(this.TileListViewItem_MouseLeave);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TileListViewItem_MouseDown);

		}
		#endregion

		#region Events

		public event EventHandler ItemSelected;

		#endregion

		#region Event Handlers

		private void TileListViewItem_MouseEnter(object sender, System.EventArgs e)
		{
			if ( !selected )
			{
				descriptionBrush = new SolidBrush( activeTextColor );
				Invalidate();
			}

			// TODO: Change the icon.
		}

		private void TileListViewItem_MouseLeave(object sender, System.EventArgs e)
		{
			if ( !selected )
			{
				descriptionBrush = new SolidBrush( inactiveTextColor );
				Invalidate();
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

		private void TileListViewItem_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			// Draw the icon.
			e.Graphics.DrawImage( icon, iconRect );
			
			//ImageList.Draw(e.Graphics, iconRect.X, iconRect.Y, iconRect.Width, iconRect.Height, imageIndex);
			// Draw the name string.
			drawFormattedString( e.Graphics, completeName, nameFont, nameBrush, nameRect );

			// Draw the location string.
			drawFormattedString( e.Graphics, completeLocation, descriptionFont, descriptionBrush, locationRect );

			// Draw the status string.
			drawFormattedString( e.Graphics, completeStatus, descriptionFont, descriptionBrush, statusRect );
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
					icon = ImageList.Images[ imageIndex ];
					Invalidate();
				}
			}
		}

		public ImageList ImageList
		{
			get { return owner.LargeImageList; }
		}

		public string ItemLocation
		{
			get { return completeLocation; }
			set 
			{
				completeLocation = value;
				setToolTipText();
				Invalidate();
			}
		}

		public string ItemName
		{
			get { return completeName; }
			set 
			{
				completeName = value;
				setToolTipText();
				Invalidate();
			}
		}

		public string Status
		{
			get { return completeStatus; }
			set 
			{
/*				if ( value.Equals( "success" ) )
				{
					timer1.Start();
					success = true;
					status.Text = "Synchronized: less than a minute ago";
				}
				else
				{
					timer1.Stop();
					success = false;
					status.Text = value;
				}*/

				completeStatus = value;
				setToolTipText();
				Invalidate();
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
					nameBrush = descriptionBrush = new SolidBrush( normalColor );
					Invalidate();
					
					if (ItemSelected != null)
					{
						ItemSelected(this, new EventArgs());
					}
				}
				else
				{
					BackColor = normalColor;
					nameBrush = new SolidBrush( activeTextColor );
					descriptionBrush = new SolidBrush( inactiveTextColor );
					Invalidate();
				}
			}
		}

		#endregion

		#region Private Methods

		private void drawFormattedString( Graphics graphics, string s, Font font, SolidBrush brush, Rectangle rectangle )
		{
			string formattedString = s;

			// Measure the string.
			SizeF stringSize = graphics.MeasureString(s, font);
			if (stringSize.Width > rectangle.Width)
			{
				// Calculate the length of the string that can be displayed ... this will get us in the
				// ballpark.
				int length = (int)(rectangle.Width * s.Length / stringSize.Width);
				string tmp = String.Empty;

				// Remove one character at a time until we fit in the box.  This should only loop 3 or 4 times at most.
				while (stringSize.Width > rectangle.Width)
				{
					tmp = s.Substring(0, length) + "...";
					stringSize = graphics.MeasureString(tmp, font);
					length -= 1;
				}

				formattedString = tmp;
			}

			graphics.DrawString( formattedString, font, brush, rectangle );
		}

		private void setToolTipText()
		{
			// TODO: Localize.
			string text = "Name: " + completeName + "\n";

			iFolderObject ifolderObject = (iFolderObject)Tag;

			if ( ifolderObject.iFolderWeb.IsSubscription )
			{
				// TODO: Localize.
				//MessageBox.Show(string.Format("the size is: {0}", ifolderObject.iFolderWeb.Size));
				text += completeLocation + "\n" + 
					"Size: " + completeStatus/*ifolderObject.iFolderWeb.Size*/ + "\n";
			}
			else
			{
				// TODO: Localize.
				text += "Location: " + completeLocation + "\n";

				if ( ifolderObject.iFolderState.Equals( iFolderState.Normal ) )
				{
					text += completeStatus + "\n";
				}
				else
				{
					// TODO: Localize.
					text += "Status: " + completeStatus + "\n";
				}
			}
            
			toolTip.SetToolTip( this, text );
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

		private void TileListViewItem_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			switch ( e.KeyCode )
			{
				case Keys.Up:
					owner.MoveUp( this );
					break;
				case Keys.Down:
					owner.MoveDown( this );
					break;
				case Keys.Right:
					owner.MoveRight( this );
					break;
				case Keys.Left:
					owner.MoveLeft( this );
					break;
				case Keys.PageDown:
					break;
				case Keys.PageUp:
					break;
			}
		}
	}
}
