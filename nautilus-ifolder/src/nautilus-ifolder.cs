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
 *	Author: Boyd Timothy <btimothy@novell.com>
 ***********************************************************************/

using System;
using System.Net;
using Gtk;
using Gnome;
using Novell.iFolder;
using Simias.Client;

namespace Novell.iFolder.Nautilus
{
	public class NautilusiFolder
	{
		public static int Main (string[] args)
		{
			// Don't do anything if nothing was specified to do
			if (args.Length == 0)
				return 0;

			// Make sure this process is a gnome program
			Gnome.Program program = 
				new Program ("Nautilus-Extension-UI", "0.1.0", Modules.UI, args);
				
			// Get the localized strings loaded
			Util.InitCatalog();
			
			switch (args [0]) {
				case "WebServiceURL":
					return getWebServiceURL (args);
				case "share":
					return showShareDialog (args);
				case "properties":
					return showPropertiesDialog (args);
				case "help":
					return showHelp (args);
			}
			
			program.Run ();
			return 0;
		}
		
		public static void on_dialog_closed (object o, EventArgs args)
		{
			Application.Quit ();
		}
		
		private static int getWebServiceURL (string[] args)
		{
			Uri localServiceUri = Manager.LocalServiceUrl;
			if (localServiceUri == null)
				return -1;
				
			System.Console.Write (localServiceUri.ToString ());
			
			return 0;
		}
		
		private static int showShareDialog (string[] args)
		{
			if (args.Length < 2) {
				System.Console.Write ("ERROR: iFolder ID not specified\n");
				return -1;
			}
			
			iFolderPropertiesDialog propsDialog;
			propsDialog = new iFolderPropertiesDialog (args [1]);
			propsDialog.CurrentPage = 1;
			propsDialog.Run ();
			propsDialog.Hide ();
			propsDialog.Destroy ();
			return 0;
		}
		
		private static int showPropertiesDialog (string[] args)
		{
			if (args.Length < 2) {
				System.Console.Write ("ERROR: iFolder ID not specified\n");
				return -1;
			}
			
			iFolderPropertiesDialog propsDialog;
			propsDialog = new iFolderPropertiesDialog (args [1]);
			propsDialog.CurrentPage = 0;
			propsDialog.Run ();
			propsDialog.Hide ();
			propsDialog.Destroy ();
			return 0;
		}
		
		private static int showHelp (string[] args)
		{
			Util.ShowHelp("front.html", null);
			return 0;
		}
	}
}