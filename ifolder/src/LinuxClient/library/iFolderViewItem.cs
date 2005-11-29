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
	public class iFolderViewItem : EventBox
	{
		private iFolderHolder		holder;
		private iFolderViewGroup	group;
		private TreeIter			iter;
		private int					maxWidth;
		
		private int					widthForLabels;
		
		private bool				bSelected;
		
		private static Gdk.Pixbuf	OKFolder					= null;
		private static Gdk.Pixbuf	OKFolderSpotlight			= null;
		
		private static Gdk.Pixbuf	AvailableFolder				= null;
		private static Gdk.Pixbuf	AvailableFolderSpotlight	= null;
		
		private static Gdk.Pixbuf	ConflictFolder				= null;
		private static Gdk.Pixbuf	ConflictFolderSpotlight		= null;
		
		private static Gdk.Pixbuf	SyncFolder					= null;
		private static Gdk.Pixbuf	SyncFolderSpotlight			= null;

		private Gdk.Pixbuf			normalPixbuf;
		private Gdk.Pixbuf			spotlightPixbuf;
		
		private Table				table;

		public Image				image;

		public Label				nameLabel;
		public Label				locationLabel;
		public Label				statusLabel;
		
		///
		/// Events
		///
		public event EventHandler	LeftClicked;
		public event EventHandler	RightClicked;
		
		/// <summary>
		/// Note: LeftClicked events will also be sent out in addition with the
		/// DoubleClicked event.
		/// </summary>
		public event EventHandler	DoubleClicked;
		
		public iFolderHolder Holder
		{
			get{ return holder; }
			set
			{
				holder = value;
			}
		}
		
		public iFolderViewGroup Group
		{
			get{ return group; }
		}
		
		public TreeIter TreeIter
		{
			get{ return iter; }
		}
		
		public bool Selected
		{
			get{ return bSelected; }
			set
			{
				bSelected = value;
				
				if (bSelected)
					this.State = StateType.Selected;
				else
					this.State = StateType.Normal;
			}
		}
	
		public iFolderViewItem(iFolderHolder holder, iFolderViewGroup group, TreeIter iter, int maxWidth)
		{
			this.holder = holder;
			this.group = group;
			this.iter = iter;
			this.maxWidth = maxWidth;
			
			this.bSelected = false;

			LoadImages();
			SetPixbufs();
			
			this.Add(CreateWidgets());
			
			this.WidthRequest = this.maxWidth;
		}
		
		public void LoadImages()
		{
			if (OKFolder == null)
			{
				OKFolder = 
					new Gdk.Pixbuf(
						Util.ImagesPath("synchronized-folder64.png"));
			}
			
			if (OKFolderSpotlight == null)
			{
				OKFolderSpotlight = 
					new Gdk.Pixbuf(
						Util.ImagesPath("synchronized-folder-spotlight64.png"));
			}
			
			if (AvailableFolder == null)
			{
				AvailableFolder =
					new Gdk.Pixbuf(
						Util.ImagesPath("available-folder64.png"));
			}

			if (AvailableFolderSpotlight == null)
			{
				AvailableFolderSpotlight =
					new Gdk.Pixbuf(
						Util.ImagesPath("available-folder-spotlight64.png"));
			}			
		}
		
		public void Refresh()
		{
			// Refresh the icon and the text based on the current state of the
			// iFolder.
			Console.WriteLine("iFolderViewItem.Refresh() called: {0}", holder.iFolder.Name);

			// Update the normalPixbuf and spotlightPixbuf to show proper state
			SetPixbufs();

			// Update the labels
			EllipseNameLabel(holder.iFolder.Name);
			if (holder.iFolder.IsSubscription)
			{
				EllipseOwnerLabel(holder.iFolder.Owner);
				EllipseSizeLabel("47");	// FIXME: Read the real size of the subcription
			}
			else
			{
				EllipseLocationLabel(holder.iFolder.UnManagedPath);
				EllipseStatusLabel(holder.StateString);
			}
		}
		
		private Widget CreateWidgets()
		{
			table = new Table(1, 2, false);
			table.ColumnSpacing = 12;
			table.BorderWidth = 4;
			
			image = new Image(normalPixbuf);
			table.Attach(image,
						 0, 1,
						 0, 1,
						 AttachOptions.Shrink,
						 0, 0, 0);
			
			VBox vbox = new VBox(false, 0);
			table.Attach(vbox,
						 1, 2,
						 0, 1,
						 AttachOptions.Expand | AttachOptions.Fill,
						 0, 0, 0);
			
			widthForLabels = (int)maxWidth - (int)normalPixbuf.Width
							 - (int)table.ColumnSpacing - (int)(table.BorderWidth * 2);
			
			nameLabel = new Label("");
			vbox.PackStart(nameLabel, false, false, 0);
			nameLabel.UseMarkup = true;
			nameLabel.UseUnderline = false;
			nameLabel.Xalign = 0;
			EllipseNameLabel(holder.iFolder.Name);
			
			locationLabel = new Label("");
			vbox.PackStart(locationLabel, false, false, 0);
			locationLabel.UseMarkup = true;
			locationLabel.UseUnderline = false;
			locationLabel.ModifyFg(StateType.Normal, this.Style.Base(StateType.Active));
			locationLabel.Xalign = 0;
			if (holder.iFolder.IsSubscription)
				EllipseOwnerLabel(holder.iFolder.Owner);
			else
				EllipseLocationLabel(holder.iFolder.UnManagedPath);
			

			statusLabel = new Label("");
			vbox.PackStart(statusLabel, false, false, 0);
			statusLabel.UseMarkup = true;
			statusLabel.UseUnderline = false;
			statusLabel.ModifyFg(StateType.Normal, this.Style.Base(StateType.Active));
			statusLabel.Xalign = 0;
			if (holder.iFolder.IsSubscription)
				EllipseSizeLabel("47");	// FIXME: Read the real size of the subcription
			else
				EllipseStatusLabel(holder.StateString);
			
			return table;
		}
		
		///
		/// Event Handlers
		///		
		protected override bool OnEnterNotifyEvent(Gdk.EventCrossing evnt)
		{
			image.Pixbuf = spotlightPixbuf;
			
			// Reset to the normal color
			locationLabel.ModifyFg(StateType.Normal, this.Style.Foreground(StateType.Normal));
			statusLabel.ModifyFg(StateType.Normal, this.Style.Foreground(StateType.Normal));

//			if (bSelected)
//				this.State = StateType.Selected;
//			else
//				this.State = StateType.Prelight;
			return false;
		}
		
		protected override bool OnLeaveNotifyEvent(Gdk.EventCrossing evnt)
		{
			image.Pixbuf = normalPixbuf;
			
			locationLabel.ModifyFg(StateType.Normal, this.Style.Base(StateType.Active));
			statusLabel.ModifyFg(StateType.Normal, this.Style.Base(StateType.Active));

//			if (bSelected)
//				this.State = StateType.Selected;
//			else
//				this.State = StateType.Normal;

			return false;
		}
		
		protected override bool OnButtonPressEvent(Gdk.EventButton evnt)
		{
			switch(evnt.Button)
			{
				case 1:		// left-click
					if (LeftClicked != null)
						LeftClicked(this, EventArgs.Empty);

					if (evnt.Type == Gdk.EventType.TwoButtonPress)
					{
						if (DoubleClicked != null)
							DoubleClicked(this, EventArgs.Empty);
					}
					break;
				case 3:		// right-click
					if (RightClicked != null)
						RightClicked(this, EventArgs.Empty);
					break;
				default:
					break;
			}
			
			return false;
		}
		
		///
		/// Utility Methods
		///
		
		private void SetPixbufs()
		{
Console.WriteLine("SetPixbufs()");
			if (holder.iFolder.IsSubscription)
			{
				normalPixbuf = AvailableFolder;
				spotlightPixbuf = AvailableFolderSpotlight;
			}
			else
			{
				// FIXME: Set the pixbufs based on current state
				normalPixbuf = OKFolder;
				spotlightPixbuf = OKFolderSpotlight;
			}
Console.WriteLine("\tSetPixbufs exiting");
		}
		
		private void EllipseNameLabel(string text)
		{
			int attempts = 0;
			bool bDone = false;
			do
			{
				string potentialMarkup;
				if (attempts == 0)
				{
					potentialMarkup =
						string.Format("<span size=\"large\">{0}</span>",
									  text);
					attempts++;
				}
				else
				{
					potentialMarkup =
						string.Format("<span size=\"large\">{0}...</span>",
									  text);
				}
				nameLabel.Markup = potentialMarkup;
				
				Requisition req = nameLabel.SizeRequest();
				
				if (req.Width <= widthForLabels)
					bDone = true;
				else
				{
					try
					{
						text = text.Substring(0, text.Length - 1);
					}
					catch
					{
						bDone = true;
					}
				}
			} while (!bDone);
		}
		
		private void EllipseOwnerLabel(string text)
		{
			int attempts = 0;
			bool bDone = false;
			do
			{
				string potentialMarkup;
				if (attempts == 0)
				{
					potentialMarkup =
						string.Format("<span size=\"small\">{0}: {1}</span>",
									  Util.GS("Owner"),
									  text);
					attempts++;
				}
				else
				{
					potentialMarkup =
						string.Format("<span size=\"small\">{0}: {1}...</span>",
									  Util.GS("Owner"),
									  text);
				}
				locationLabel.Markup = potentialMarkup;
				
				Requisition req = locationLabel.SizeRequest();
				
				if (req.Width <= widthForLabels)
					bDone = true;
				else
				{
					try
					{
						text = text.Substring(0, text.Length - 1);
					}
					catch
					{
						bDone = true;
					}
				}
			} while (!bDone);
		}
		
		private void EllipseLocationLabel(string text)
		{
			int attempts = 0;
			bool bDone = false;
			do
			{
				string potentialMarkup;
				if (attempts == 0)
				{
					potentialMarkup =
						string.Format("<span size=\"small\">{0}</span>",
									  text);
					attempts++;
				}
				else
				{
					potentialMarkup =
						string.Format("<span size=\"small\">{0}...</span>",
									  text);
				}
				locationLabel.Markup = potentialMarkup;
				
				Requisition req = locationLabel.SizeRequest();
				
				if (req.Width <= widthForLabels)
					bDone = true;
				else
				{
					try
					{
						text = text.Substring(0, text.Length - 1);
					}
					catch
					{
						bDone = true;
					}
				}
			} while (!bDone);
		}
		
		private void EllipseSizeLabel(string text)
		{
			int attempts = 0;
			bool bDone = false;
			do
			{
				string potentialMarkup;
				if (attempts == 0)
				{
					potentialMarkup =
						string.Format("<span size=\"small\">{0} {1}</span>",
									  text,
									  Util.GS("MB"));
					attempts++;
				}
				else
				{
					potentialMarkup =
						string.Format("<span size=\"small\">{0} {1}...</span>",
									  text,
									  Util.GS("MB"));
				}
				statusLabel.Markup = potentialMarkup;
				
				Requisition req = statusLabel.SizeRequest();
				
				if (req.Width <= widthForLabels)
					bDone = true;
				else
				{
					try
					{
						text = text.Substring(0, text.Length - 1);
					}
					catch
					{
						bDone = true;
					}
				}
			} while (!bDone);
		}
		
		private void EllipseStatusLabel(string text)
		{
			int attempts = 0;
			bool bDone = false;
			do
			{
				string potentialMarkup;
				if (attempts == 0)
				{
					potentialMarkup =
						string.Format("<span size=\"small\">{0}: {1}</span>",
									  Util.GS("Status"),
									  text);
					attempts++;
				}
				else
				{
					potentialMarkup =
						string.Format("<span size=\"small\">{0}: {1}...</span>",
									  Util.GS("Status"),
									  text);
				}
				statusLabel.Markup = potentialMarkup;
				
				Requisition req = statusLabel.SizeRequest();
				
				if (req.Width <= widthForLabels)
					bDone = true;
				else
				{
					try
					{
						text = text.Substring(0, text.Length - 1);
					}
					catch
					{
						bDone = true;
					}
				}
			} while (!bDone);
		}
	}
}