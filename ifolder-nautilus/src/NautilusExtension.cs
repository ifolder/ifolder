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
 *  Author: Dave Camp <dave@ximian.com>
 *  Based on the File Browser component by:
 *          Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/

using System;

using Gtk;
using GtkSharp;
using GLib;

using Nautilus;

namespace Novell.iFolder
{
	public class NautilusProvider : GLib.Object, 
									Nautilus.PropertyPageProvider, 
									Nautilus.MenuProvider,
									Nautilus.InfoProvider 

/*	public class NautilusProvider : Nautilus.PropertyPageProvider, 
									Nautilus.MenuProvider,
									Nautilus.InfoProvider 
*/
	{
		iFolderManager ifmgr;

		public NautilusProvider () : base(IntPtr.Zero)
		{
			ifmgr = iFolderManager.Connect();
		}

		private string GetLocalPath (Nautilus.FileInfo file) 
		{
			if (file.Uri.StartsWith ("file://")) 
			{
				return file.Uri.Substring (7);
			}
			else 
			{
				return null;
			}
		}

		private Nautilus.FileInfo GetSingleFile (GLib.List files) 
		{
			if (files.Count != 1) 
			{
				return null;
			}

			return (Nautilus.FileInfo)files[0];
		}			

		// Nautilus.PropertyPageProvider interface
		public GLib.List GetPages (GLib.List files) 
		{
			Nautilus.FileInfo file;

			file = GetSingleFile (files);
			if (file == null) 
			{
				return null;
			}

			GLib.List ret = new GLib.List 
				((IntPtr) 0, typeof (Nautilus.PropertyPage));

			if (file.UriScheme.Equals ("file") 
					&& file.IsDirectory) 
			{
				string path = GetLocalPath (file);
				if (ifmgr.IsiFolder (path))
				{
					iFolder ifolder = ifmgr.GetiFolderByPath (path);

					SharingPage sharing_page = new SharingPage (ifolder);
					sharing_page.GetWidget().Show ();
					Nautilus.PropertyPage page = new Nautilus.PropertyPage
						("iFolder::sharing_page",
						 new Label ("iFolder Sharing"),
						 sharing_page.GetWidget ());

					ret.Append (page.Handle);

					NodePropertyPage node_page = new NodePropertyPage (
							ifolder.CurrentNode);
					node_page.GetWidget ().Show ();

					page = new Nautilus.PropertyPage
						("iFolder::node_property_page",
						 new Label ("iFolder"),
						 node_page.GetWidget ());

					ret.Append (page.Handle);

				}
			}
			return ret;
		}

		// Nautilus.MenuProvider interface
		private void HandleConvertActivated (object sender, EventArgs args)
		{
			Nautilus.MenuItem item = (Nautilus.MenuItem)sender;
			Nautilus.FileInfo file = (Nautilus.FileInfo)item.Data["file"];

			try
			{
				ifmgr.CreateiFolder (GetLocalPath (file));
				file.InvalidateExtensionInfo ();
			}
			catch (Exception e)
			{
				Console.WriteLine ("Failed to create iFolder " + e);
			}
		}

		private void HandleRevertActivated (object sender, EventArgs args)
		{
			Nautilus.MenuItem item = (Nautilus.MenuItem)sender;
			Nautilus.FileInfo file = (Nautilus.FileInfo)item.Data["file"];

			try
			{
				ifmgr.DeleteiFolderByPath (GetLocalPath (file));
				file.InvalidateExtensionInfo ();
			}
			catch (Exception e)
			{
				Console.WriteLine ("Failed to delete iFolder " + e);
			}
		}

		public GLib.List GetToolbarItems (Gtk.Widget window,
				Nautilus.FileInfo current_folder)
		{
			// No toolbar items
			return null;
		}


		public GLib.List GetFileItems (Gtk.Widget window,
				GLib.List files)
		{
			Nautilus.FileInfo file = GetSingleFile (files);
			if (file == null)
			{ 
				return null;
			}

			GLib.List ret = new GLib.List 
				((IntPtr) 0, typeof (Nautilus.MenuItem));

			string path = GetLocalPath (file);

			if (file.IsDirectory)
			{
				if (ifmgr.IsiFolder (path))
				{
					Nautilus.MenuItem item = new Nautilus.MenuItem
						("iFolder::revert",
						 "Revert to a Normal Folder",
						 "Revert this folder to a normal folder",
						 null);
					item.Data["file"] = file;
					item.Activated += new EventHandler (HandleRevertActivated);
					ret.Append (item.Handle);

				}
				else
				{
					Nautilus.MenuItem item = new Nautilus.MenuItem
						("iFolder::convert",
						 "Convert to an iFolder",
						 "Convert this folder to an iFolder",
						 null);
					item.Data["file"] = file;
					item.Activated += new EventHandler (HandleConvertActivated);
					ret.Append (item.Handle);
				}
			}

			return ret;
		}

		public GLib.List GetBackgroundItems (Gtk.Widget window,
				Nautilus.FileInfo current_folder)
		{
			// No background items
			return null;
		}

		// InfoProvider interface
		public void CancelUpdate (Nautilus.OperationHandle handle)
		{
			// Don't do any async updates, don't implement Cancel
		}

		public Nautilus.OperationResult UpdateFileInfo (Nautilus.FileInfo file,
				IntPtr update_complete,
				Nautilus.OperationHandle handle)
		{
			if (file.UriScheme.Equals ("file") && file.IsDirectory)
			{
				if(ifmgr.IsiFolder(GetLocalPath (file)))
				{
					file.AddEmblem ("ifolder");
				}
			}

			return Nautilus.OperationResult.Complete;
		}
	}
}



namespace Nautilus
{
	class Extension
	{
		public static System.Type[] GetTypes () 
		{
			System.Console.WriteLine ("Getting classes from iFolder");
			System.Type[] ret = new System.Type[1];	
			ret[0] = typeof (Novell.iFolder.NautilusProvider);
			return ret;
		}
	}
}


