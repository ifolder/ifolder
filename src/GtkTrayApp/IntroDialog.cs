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

namespace Novell.iFolder
{
	using System;
	using System.IO;
	using System.Drawing;
	using Simias.Storage;
	using Simias;

	using Gtk;
	using Gdk;
	using Glade;
	using GtkSharp;
	using GLib;

	public class IntroDialog 
	{
		[Glade.Widget] Gtk.CheckButton ShowCheckButton = null;

		Gtk.Dialog dialog; 
		string path;

		public Gtk.Window TransientFor
		{
			set
			{
				if(dialog != null)
					dialog.TransientFor = value;
			}
		}

		public string iFolderPath
		{
			get
			{
				return path;
			}

			set
			{
				path = value;
			}
		}

		public IntroDialog() 
		{
			InitGlade();
		}

		public void InitGlade()
		{
			Glade.XML gxml = new Glade.XML (Util.GladePath("ifolder-confirmation.glade"), 
					"ConfirmationDialog", 
					null);
			gxml.Autoconnect (this);

			dialog = (Gtk.Dialog) gxml.GetWidget("ConfirmationDialog");
		}

		public int Run()
		{
			int rc = 0;
			if(dialog != null)
			{
				rc = dialog.Run();

				if(ShowCheckButton.Active)
				{
					Console.WriteLine("Don't show");
					new Configuration().Set("iFolderTrayApp", 
							"Show wizard", "false");
				}
				else
				{
					Console.WriteLine("show");
					new Configuration().Set("iFolderTrayApp", 
							"Show wizard", "true");
				}

				dialog.Hide();
				dialog.Destroy();
				dialog = null;
			}

			return rc;
		}

		public void on_ShareButton_clicked(object obj, EventArgs args)
		{
			iFolderProperties ifProp = new iFolderProperties();
			ifProp.TransientFor = dialog;
			ifProp.iFolderPath = path;
			ifProp.ActiveTag = 1;
			ifProp.Run();
/*
/*		
			PropertiesDialog pd = new PropertiesDialog();
			pd.iFolderPath = path;
			pd.TransientFor = dialog;
			pd.ActiveTag = 1;
			pd.Run();
*/
		}

		static public bool UseDialog()
		{
			Configuration config = new Configuration();
			string showWizard = config.Get("iFolderTrayApp", 
					"Show wizard", "true");

			if (showWizard == "true")
			{
				return true;
			}
			return false;
		}
	}
}
