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

using System;
using System.Drawing;

using Gtk;
using Gdk;
using Glade;
using GtkSharp;
using GLib;

namespace Novell.iFolder
{
	public class CrashReport
	{
		[Glade.Widget] internal Gtk.TextView CrashTextView;
		[Glade.Widget] internal Gtk.Button OKButton;

		Gtk.Dialog 		crDlg = null;

		public string CrashText
		{
			set
			{
				if(CrashTextView != null)
					CrashTextView.Buffer.Text = value;
			}
		}

		public Gtk.Window TransientFor
		{
			set
			{
				if(crDlg != null)
					crDlg.TransientFor = value;
			}
		}

		public CrashReport() 
		{
			Glade.XML gxml = 
					new Glade.XML (Util.GladePath("crash-report.glade"),
					"CrashDialog", null);
			gxml.Autoconnect (this);

			crDlg = (Gtk.Dialog) gxml.GetWidget("CrashDialog");
		}

		public int Run()
		{
			int rc = 0;
			if(crDlg != null)
			{
				rc = crDlg.Run();
				crDlg.Hide();
				crDlg.Destroy();
				crDlg = null;
			}
			return rc;
		}
	}
}
