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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/


using Gtk;
using Gdk;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Novell.iFolder
{
	public class NotifyWindow : Gtk.Window
	{
		private Widget parentWidget;
		private Pixbuf	background = null;
		private Pixbuf	activebackground = null;
		private Pixbuf	inactivebackground = null;
		private uint	closeWindowTimeoutID;
		private bool	isSelected = false;
		private int		wbsize = 16;
		private uint	timeout;


		public NotifyWindow(Gtk.Widget parent, string message, string details,
							Gtk.MessageType messageType, uint timeout)
			: base(Gtk.WindowType.Popup)
 		{
			this.AppPaintable = true;
			parentWidget = parent;
			this.timeout = timeout;

			Gtk.HBox outBox = new HBox();
			this.Add(outBox);
			outBox.BorderWidth = (uint)wbsize;

			Gtk.VBox closeBox = new VBox();
			closeBox.BorderWidth = 3;
			outBox.PackEnd(closeBox, true, true, 0);
			EventBox eBox = new EventBox();
			eBox.ButtonPressEvent += 
				new ButtonPressEventHandler(OnCloseEvent);
			Gtk.Image closeImg = new Gtk.Image();
			closeImg.SetFromStock(Gtk.Stock.Close, 
											IconSize.Menu);
			eBox.Add(closeImg);
			closeBox.PackStart(eBox, false, false, 0);

			Label padder = new Label("");
			outBox.PackStart(padder, false, false, 5);

			Gtk.VBox vbox = new VBox();
			outBox.PackStart(vbox, true, true, 0);
			vbox.BorderWidth = 10;
		
			Gtk.HBox hbox = new HBox();
			hbox.Spacing = 5;
			vbox.PackStart(hbox, false, false, 0);

			Gtk.Image msgImage = new Gtk.Image();
			switch(messageType)
			{
				case Gtk.MessageType.Info:
					msgImage.SetFromStock(Gtk.Stock.DialogInfo, 
											IconSize.Button);
					break;
				case Gtk.MessageType.Warning:
					msgImage.SetFromStock(Gtk.Stock.DialogWarning, 
											IconSize.Button);
					break;
				case Gtk.MessageType.Question:
					msgImage.SetFromStock(Gtk.Stock.DialogQuestion, 
											IconSize.Button);
					break;
				case Gtk.MessageType.Error:
					msgImage.SetFromStock(Gtk.Stock.DialogError, 
											IconSize.Button);
					break;
			}

			hbox.PackStart(msgImage, false, true, 0);
//			vbox.Spacing = 5;
			// Fix for Bug #116812: iFolders w/ underscore characters are not
			// displayed correctly in bubble.
			// If we put the text in the constructor of the Label widget,
			// the markup is parsed to look for mnemonics.  If we just set
			// the Markup property later, the underscore character is
			// interpreted correctly.
			Label l = new Label();
			l.Markup = "<span size=\"small\" weight=\"bold\">" + message + "</span>";
			l.LineWrap = false;
			l.UseMarkup = true;
			l.Selectable = false;
			l.Xalign = 0;
			l.Yalign = 0;
			hbox.PackStart(l, false, true, 0);

			Label l2 = new Label("<span size=\"small\">" + details + "</span>");
			l2.WidthRequest = 400;
			l2.UseMarkup = true;
			l2.LineWrap = true;
			l2.Xalign = 0;
			l.Yalign = 0;
			vbox.PackStart(l2, false, true, 0);
/*
			l = new Label("<span size=\"small\" underline=\"single\">" +
				"Go There" + "</span>");
			l.UseMarkup = true;
			l.LineWrap = false;
			l.Xalign = 1;
			l.Yalign = 0;
			fxd.Put(l, 160, 80);
			l.ButtonPressEvent += new ButtonPressEventHandler(
						OnLabelClicked);
*/
/*
			HBox buttonBox = new HBox();
			buttonBox.Spacing = 10;
			vbox.PackStart(buttonBox, true, true, 0);

			Button but = new Button("_Close");
			but.Clicked += new EventHandler(OnCloseButton);
			but.Relief = Gtk.ReliefStyle.None;
			buttonBox.PackEnd(but, false, true, 0);
*/

			closeWindowTimeoutID = 0;
		}



		protected override void OnShown()
		{
			base.OnShown();

			if(closeWindowTimeoutID == 0)
			{
				closeWindowTimeoutID = Gtk.Timeout.Add(timeout, new Gtk.Function(
						HideWindowCallback));
			}
		}
/*
		[GLib.ConnectBefore]
		public void OnLabelClicked(	object obj, ButtonPressEventArgs args)
		{
			Console.WriteLine("Mouse on test");
		}
*/
		private bool HideWindowCallback()
		{
			this.Hide();
			this.Destroy();
			return false;
		}

		private void OnCloseEvent(object obj, ButtonPressEventArgs args)
		{
			this.Hide();
			this.Destroy();
		}

		protected override void OnSizeAllocated(Gdk.Rectangle sized)
		{
			base.OnSizeAllocated(sized);	

			if(background == null)
			{
				Gdk.Bitmap mask;

				RenderBubbles(this.RootWindow, sized, out activebackground, 
							out inactivebackground, out mask);

				background = inactivebackground;

//				Gtk.Style style = new Gtk.Style();

				if(mask != null)
					this.ShapeCombineMask(mask, 0, 0);
				else
					Console.WriteLine("mask was null");
			}

		}
		
		private void RenderBubbles(Gdk.Window win, Gdk.Rectangle size, 
						out Pixbuf pbactive, out Pixbuf pbinactive, 
						out Bitmap pbbm)
		{
			int pmHeight, pmWidth;
			Gdk.Pixmap daPixmap;
			Gdk.Bitmap daBitmap;

			pmHeight = size.Height - (wbsize * 2);
			pmWidth = size.Width - (wbsize * 2);

			Gdk.GC gc = new Gdk.GC(win);

			// Build active Pixbuf
//			Gdk.Pixmap pm = new Pixmap(win, pmWidth, pmHeight, -1);
			Gdk.Pixmap pm = new Pixmap(win, size.Width, size.Height, -1);

			// Paint the background white
			gc.RgbFgColor = new Gdk.Color(255, 255, 255);
			pm.DrawRectangle(gc, true, 0, 0, size.Width, size.Height);

			/***********************************
				draw painted oval window
			***********************************/
			// Paint the inside of the window
			gc.RgbFgColor = new Gdk.Color(249, 253, 202);
			Gdk.Point[] roundedSquare = CalculateRect(wbsize, wbsize, 
										pmWidth, pmHeight);
			pm.DrawPolygon(gc, true, roundedSquare);

			// Paint the border of the window
			Gdk.Point[] roundedborder = CalculateRect(wbsize, wbsize, 
												pmWidth - 1, pmHeight - 1);
			gc.RgbFgColor = new Gdk.Color(0, 0, 0);
			pm.DrawPolygon(gc, false, roundedborder);

			/***********************************
				add tab to bitmap
			***********************************/
			Gdk.Point[] balloonptr = CalcPointerMoveWindow( size.Width,
													size.Height );
			// Draw colored pointer
			gc.RgbFgColor = new Gdk.Color(249, 253, 202);
			pm.DrawPolygon(gc, true, balloonptr);
			gc.RgbFgColor = new Gdk.Color(0, 0, 0);
			// subtract one because the fill above used and extra line
			pm.DrawLine(gc, balloonptr[0].X, balloonptr[0].Y-1, balloonptr[1].X,
							balloonptr[1].Y);
			pm.DrawLine(gc, balloonptr[1].X, balloonptr[1].Y, balloonptr[2].X,
							balloonptr[2].Y-1);

			Gdk.Pixbuf pb = new Pixbuf(Gdk.Colorspace.Rgb, false, 
						8, size.Width, size.Height);

			pb = pb.CreateFromDrawable(	pm, pm.Colormap, 0, 0, 0, 0, 
						size.Width, size.Height);
			pb = pb.AddAlpha(true, 255, 255,255);

			RenderPixmapAndMask(pb, out daPixmap, out daBitmap, 2);
			pbactive = pb;
			pbbm = daBitmap;

			// Reset backgound to white and get next bitmap
			gc.RgbFgColor = new Gdk.Color(255, 255, 255);
			pm.DrawRectangle(gc, true, 0, 0, size.Width, size.Height);

			// Paint the border of the window
			gc.RgbFgColor = new Gdk.Color(0, 0, 0);
			pm.DrawPolygon(gc, false, roundedborder);

			// Draw white pointer
			gc.RgbFgColor = new Gdk.Color(255, 255, 255);
			pm.DrawPolygon(gc, true, balloonptr);
			gc.RgbFgColor = new Gdk.Color(0, 0, 0);
			// subtract one because the fill above used and extra line
			pm.DrawLine(gc, balloonptr[0].X, balloonptr[0].Y-1, balloonptr[1].X,
							balloonptr[1].Y);
			pm.DrawLine(gc, balloonptr[1].X, balloonptr[1].Y, balloonptr[2].X,
							balloonptr[2].Y - 1);

			pb = pb.CreateFromDrawable(	pm, 
						pm.Colormap, 0, 0, 0, 0, size.Width, size.Height);

			pbinactive = pb;
		}



		protected Gdk.Point[] CalcPointerMoveWindow( int width, int height )
		{
			int parentX, parentY, parentWidth, parentHeight, parentDepth;
			int midParentX, midParentY, posX, posY;
			int ptsize = wbsize;
			bool drawRight, drawDown;

			parentWidget.GdkWindow.GetGeometry(out parentX, out parentY,
												out parentWidth,
												out parentHeight,
												out parentDepth);
			parentWidget.GdkWindow.GetOrigin(out parentX, out parentY);

			midParentX = parentX + (parentWidth / 2);
			midParentY = parentY + (parentHeight / 2);

			// Do we draw to the left or to the right of the icon
			if(parentX >= (this.Screen.Width / 2) )
			{
				drawRight = false;
				posX = midParentX - width;
			}
			else
			{
				drawRight = true;
				posX = midParentX;
			}

			// Do we draw above or below the icon
			if(parentY >= (this.Screen.Height / 2) )
			{
				drawDown = false;
				posY = midParentY - height;
			}
			else
			{
				drawDown = true;
				posY = midParentY;
			}

			Move(posX, posY);

			ArrayList list = new ArrayList();

			if(drawRight)
			{
				if(drawDown)
				{
					list.Add(new Point( (wbsize),
										(wbsize + ptsize) ));
					list.Add(new Point( 0, 0 ));
					list.Add(new Point( (wbsize + ptsize),
										(wbsize) ));
				}
				else
				{
					list.Add(new Point( (wbsize + ptsize),
										(height - wbsize) ));
					list.Add(new Point( 0, height ));
					list.Add(new Point( (wbsize),
										(height - wbsize - ptsize) ));
				}
			}
			else
			{
				if(drawDown)
				{
					list.Add(new Point( (width - wbsize - ptsize),
										(wbsize) ));
					list.Add(new Point( width, 0 ));
					list.Add(new Point( (width - wbsize),
										(wbsize + ptsize) ));
				}
				else
				{
					list.Add(new Point( (width - wbsize - ptsize),
										(height - wbsize) ));
					list.Add(new Point( width, height ));
					list.Add(new Point( (width - wbsize),
										(height - wbsize - ptsize) ));
				}
			}

			return (Gdk.Point[]) (list.ToArray(typeof(Gdk.Point)));
		}


		protected Gdk.Point[] CalculateRect(	int xorg, int yorg, 
												int height, int width)
		{
			ArrayList list = new ArrayList();

			// top left corner
			list.Add(new Point(xorg, yorg + 4));
			list.Add(new Point(xorg + 1, yorg + 4));
			list.Add(new Point(xorg + 1, yorg + 2));
			list.Add(new Point(xorg + 2, yorg + 2));
			list.Add(new Point(xorg + 2, yorg + 1));
			list.Add(new Point(xorg + 4, yorg + 1));
			list.Add(new Point(xorg + 4, yorg));
			
			// top Right corner
			list.Add(new Point( (xorg + height) - 4, yorg));
			list.Add(new Point( (xorg + height) - 4, yorg + 1));
			list.Add(new Point( (xorg + height) - 2, yorg + 1));
			list.Add(new Point( (xorg + height) - 2, yorg + 2));
			list.Add(new Point( (xorg + height) - 1, yorg + 2));
			list.Add(new Point( (xorg + height) - 1, yorg + 4));
			list.Add(new Point( (xorg + height), yorg + 4));

			// bottom Right corner
			list.Add(new Point( (xorg + height), (yorg + width) - 4));
			list.Add(new Point( (xorg + height) - 1, (yorg + width) - 4));
			list.Add(new Point( (xorg + height) - 1, (yorg + width) - 2));
			list.Add(new Point( (xorg + height) - 2, (yorg + width) - 2));
			list.Add(new Point( (xorg + height) - 2, (yorg + width) - 1));
			list.Add(new Point( (xorg + height) - 4, (yorg + width) - 1));
			list.Add(new Point( (xorg + height) - 4, (yorg + width)));
			
			// bottom Left corner
			list.Add(new Point( xorg + 4, (yorg + width)));
			list.Add(new Point( xorg + 4, (yorg + width) - 1));
			list.Add(new Point( xorg + 2, (yorg + width) - 1));
			list.Add(new Point( xorg + 2, (yorg + width) - 2));
			list.Add(new Point( xorg + 1, (yorg + width) - 2));
			list.Add(new Point( xorg + 1, (yorg + width) - 4));
			list.Add(new Point( xorg, (yorg + width) - 4));

			return (Gdk.Point[]) (list.ToArray(typeof(Gdk.Point)));
		}


		protected override bool OnExposeEvent(Gdk.EventExpose args)
		{
			if(isSelected)
				background = activebackground;
			else
				background = inactivebackground;

			GdkWindow.DrawPixbuf(Style.BackgroundGC(State), background, 0, 0,
				0, 0, background.Width, background.Height, 
				Gdk.RgbDither.None, 0, 0);

			return base.OnExposeEvent(args);
		}

		protected override bool OnEnterNotifyEvent(Gdk.EventCrossing evnt)
		{
			// if we just left or entered from our own child widget
			if(evnt.Detail == Gdk.NotifyType.Inferior)
				return false;

			if(closeWindowTimeoutID != 0)
			{
				Gtk.Timeout.Remove(closeWindowTimeoutID);
				closeWindowTimeoutID = 0;
			}

			isSelected = true;
			QueueDraw();

			return false;
		}
		
		protected override bool OnLeaveNotifyEvent(Gdk.EventCrossing evnt)
		{
			// if we just left or entered from our own child widget
			if(evnt.Detail == Gdk.NotifyType.Inferior)
				return false;

			isSelected = false;
			QueueDraw();

			if(closeWindowTimeoutID != 0)
			{
				Gtk.Timeout.Remove(closeWindowTimeoutID);
				closeWindowTimeoutID = 0;
			}

			closeWindowTimeoutID = Gtk.Timeout.Add(timeout, new Gtk.Function(
						HideWindowCallback));

			return false;
		}


		[DllImport("libgtk-x11-2.0.so.0")]
		static extern IntPtr gdk_pixmap_create_from_xpm(IntPtr drawable, 
			out IntPtr mask, ref Gdk.Color transparent_color, string filename);

		public static Gdk.Pixmap CreateFromXpm(Gdk.Drawable drawable, 
			out Gdk.Bitmap mask, Gdk.Color transparent_color, string filename) 
		{
			IntPtr mask_handle;
			IntPtr raw_ret = gdk_pixmap_create_from_xpm(drawable.Handle, 
				out mask_handle, ref transparent_color, filename);

			Gdk.Pixmap ret;
			if (raw_ret == IntPtr.Zero)
				ret = null;
			else
				ret = new Gdk.Pixmap(raw_ret);
//				ret = (Gdk.Pixmpa) GLib.Object.GetObject(raw_ret);

//			GLib.Object obj = GLib.Object.GetObject(mask_handle);
//			Console.WriteLine(obj);
			mask = new Gdk.Bitmap(mask_handle);

			return ret;
		}

		[DllImport("libgtk-x11-2.0.so.0")]
		static extern void gdk_pixbuf_render_pixmap_and_mask(IntPtr raw, 
				out IntPtr pixmap_return, out IntPtr mask_return, 
				int alpha_threshold);

		public void RenderPixmapAndMask(Gdk.Pixbuf pixbuf, 
				out Gdk.Pixmap pixmap_return, out Gdk.Bitmap mask_return, 
				int alpha_threshold) 
		{
			IntPtr pm_handle;
			IntPtr bm_handle;

			gdk_pixbuf_render_pixmap_and_mask(pixbuf.Handle, out pm_handle,
					out bm_handle, alpha_threshold);

			pixmap_return = new Gdk.Pixmap(pm_handle);
			mask_return = new Gdk.Bitmap(bm_handle);
		}


	}
}
