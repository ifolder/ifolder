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
 *			Code based on examples from the book "Mono A Develper's Notebook"
 				by Edd Dumbill and Niel M. Bornstein
 * 
 ***********************************************************************/


using Gtk;
using Gdk;
using System;
using System.Runtime.InteropServices;

namespace Novell.iFolder
{
	public class NotifyWindow : Gtk.Window
	{
		private Pixbuf	background;
		private Pixbuf	activebackground;
		private Pixbuf	inactivebackground;
		private uint	closeWindowTimeoutID;

		public NotifyWindow(Gtk.Widget parent) : base(Gtk.WindowType.Popup)
 		{
			int parentX, parentY, parentWidth, parentHeight, parentDepth;
			this.AppPaintable = true;

			parent.GdkWindow.GetGeometry(out parentX, out parentY,
												out parentWidth,
												out parentHeight,
												out parentDepth);

			parent.GdkWindow.GetOrigin(out parentX, out parentY);

			Move(parentX + (parentWidth / 2) - 55, parentY + parentHeight - 5);

			inactivebackground = new Gdk.Pixbuf(Util.ImagesPath("notify.xpm"));
			activebackground = 
					new Gdk.Pixbuf(Util.ImagesPath("notify-active.xpm"));
			background = inactivebackground;

			this.SetDefaultSize(320, 110);

			Gtk.Fixed fxd = new Gtk.Fixed();
			this.Add(fxd);

			Gtk.Image iFolderImage = new Gtk.Image(
					new Gdk.Pixbuf(Util.ImagesPath("ifolder48.png")));

			fxd.Put(iFolderImage, 15, 30);
		
			Label l = new Label("<span weight=\"bold\" size=\"large\">" +
							"New iFolder Recieved" + "</span>");
			l.LineWrap = false;
			l.UseMarkup = true;
			l.Selectable = false;
			l.Xalign = 0;
			l.Yalign = 0;
			fxd.Put(l, 80, 22);

			l = new Label("<span size=\"small\">" +
				"Brady has shared an iFolder named\n\"Vespa Scooters\" with you." + "</span>");
			l.UseMarkup = true;
			l.LineWrap = false;
			l.Xalign = 0;
			l.Yalign = 0;
			fxd.Put(l, 80, 45);
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

			Button but = new Button("_Close");
			but.Clicked += new EventHandler(OnCloseButton);
			but.Relief = Gtk.ReliefStyle.None;
			fxd.Put(but, 255, 70);


			Gdk.Bitmap mask;
			Gtk.Style style = new Gtk.Style();

			CreateFromXpm(RootWindow,
					out mask, style.Background(Gtk.StateType.Normal), 
					Util.ImagesPath("notify.xpm"));
			if(mask != null)
				this.ShapeCombineMask(mask, 0, 0);
			else
				Console.WriteLine("mask was null");

			closeWindowTimeoutID = 0;
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

		private void OnCloseButton(object o, EventArgs args)
		{
			this.Hide();
			this.Destroy();
		}

		protected override bool OnExposeEvent(Gdk.EventExpose args)
		{
			GdkWindow.DrawPixbuf(Style.BackgroundGC(State), background, 0, 0,
				0, 0, background.Width, background.Height, Gdk.RgbDither.None, 
				0, 0);
			return base.OnExposeEvent(args);
		}

		protected override void OnShown()
		{
			base.OnShown();

			if(closeWindowTimeoutID == 0)
			{
				closeWindowTimeoutID = Gtk.Timeout.Add(5000, new Gtk.Function(
						HideWindowCallback));
			}
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

			background = activebackground;
			QueueDraw();

			return false;
		}
		
		protected override bool OnLeaveNotifyEvent(Gdk.EventCrossing evnt)
		{
			// if we just left or entered from our own child widget
			if(evnt.Detail == Gdk.NotifyType.Inferior)
				return false;
			background = inactivebackground;
			QueueDraw();

			if(closeWindowTimeoutID != 0)
			{
				Gtk.Timeout.Remove(closeWindowTimeoutID);
				closeWindowTimeoutID = 0;
			}

			closeWindowTimeoutID = Gtk.Timeout.Add(5000, new Gtk.Function(
						HideWindowCallback));

			return false;
		}


		[DllImport("libgtk-x11-2.0.so")]
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

	}
}
