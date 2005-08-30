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
 *	Author: Federico Mena-Quintero <federico@ximian.com>
 *  minor patches: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/

/*
 * CompatFileChooser.cs - file chooser that uses GtkFileSelection or
 * GtkFileChooser if it is available
 */

using System;
using Gtk;
using System.Runtime.InteropServices;

namespace Novell.iFolder
{
	public class CompatFileChooserDialog
	{
		/* Public interface */

		public enum Action
		{
			Open,
			Save,
			SelectFolder
		}

		public CompatFileChooserDialog (string title, Gtk.Window parent, 
										Action action)
		{
			// CG: this will return a null or a zero length string it it's ok
			string gtkResults = Gtk.Global.CheckVersion(2,4,0);
			if (gtkResults == null || gtkResults.Length == 0)
			{
				// bht: we have to do this extra check to make this work in openSUSE
				// FIXME: Fix CompatFileChooserDialog to work correctly in openSUSE
				gtkResults = Gtk.Global.CheckVersion(2,8,0);
				if (gtkResults == null || gtkResults.Length == 0)
					use_file_chooser = false;
				else
					use_file_chooser = true;
			}
			else
			{
				use_file_chooser = false;
			}

//			if( (gtkResults == null) || (gtkResults.Length == 0) )
//				use_file_chooser = true;
//			else
//				use_file_chooser = false;

			if (use_file_chooser)
				create_with_file_chooser (title, parent, action);
			else
				create_with_file_selection (title, parent, action);
		}

		public bool SelectMultiple
		{
			get
			{
				if (use_file_chooser)
					return gtk_file_chooser_get_select_multiple(chooser.Handle);
				else
					return filesel.SelectMultiple;
			}

			set
			{
				if (use_file_chooser)
					gtk_file_chooser_set_select_multiple(chooser.Handle, value);
				else
					filesel.SelectMultiple = value;
			}
		}

		public string Filename
		{
			get
			{
				if (use_file_chooser) 
					return gtk_file_chooser_get_filename (chooser.Handle);
				else
					return filesel.Filename;
			}

			set
			{
				if (use_file_chooser)
					// FIXME: This returns a boolean success code.
					// We can't do much if it fails (e.g. when the
					// file does not exist), so do we need to
					// actually check the return value?
					gtk_file_chooser_set_filename (chooser.Handle, value);
				else
					filesel.Filename = value;
			}
		}

		public string CurrentFolder
		{
			get
			{
				if (use_file_chooser)
					return gtk_file_chooser_get_current_folder (chooser.Handle);
				else
					return null;
			}
			set
			{
				if (use_file_chooser)
					gtk_file_chooser_set_current_folder(chooser.Handle, value);
			}
		}

		public string[] Selections
		{
			get
			{
				if (use_file_chooser)
				{
					IntPtr ptr = gtk_file_chooser_get_filenames(chooser.Handle);
					if (ptr == IntPtr.Zero)
						return null;

					GLib.SList slist = new GLib.SList (ptr, typeof (string));

					string [] paths = new string [slist.Count];
					for (int i = 0; i < slist.Count; i ++)
						paths [i] = (string) slist [i];

					return paths;
				} else
					return filesel.Selections;
			}
		}

		public void Destroy ()
		{
			if (use_file_chooser)
			{
				// FIXME
			}
			else
				filesel.Destroy ();
		}

		// CG: added hide to be more consistent with Dialogs
		public void Hide()
		{
			if (use_file_chooser)
				chooser.Hide();
			else
				filesel.Hide();
		}

		public int Run ()
		{
			int response;

			if (use_file_chooser)
				response = chooser.Run ();
			else
				response = filesel.Run ();

			return response;
		}

		/* Private implementation */

		private bool use_file_chooser;

		private Gtk.FileSelection filesel;
		private Gtk.Dialog chooser;

		private void create_with_file_chooser (string title, Gtk.Window parent,
												Action action)
		{
			int a = 0;
			string stock = Gtk.Stock.Open;

			/* FIXME: here we use the raw enum values from
			 * GtkFileChooserAction, because I'm too lazy to do something
			 * like this:
			 *
			 * GType t = gtk_file_chooser_action_get_type ();
			 * GEnumClass *c = g_type_class_ref (t);
			 * GEnumValue *v = g_enum_get_value_by_name (c, "GTK_FILE_CHOOSER_ACTION_FOO");
			 * int intval = v->value;
			 * g_type_class_unref (c);
			 * ... use intval ...
			 */
			switch (action)
			{
				case Action.Open:
					a = 0;
					stock = Gtk.Stock.Open;
					break;

				case Action.Save:
					a = 1;
					stock = Gtk.Stock.Save;
					break;

				case Action.SelectFolder:
					a = 2;
					stock = Gtk.Stock.Open;
					break;
			}

			IntPtr ptr = gtk_file_chooser_dialog_new (title,
					parent != null ? parent.Handle : IntPtr.Zero,
					a,
					IntPtr.Zero);

			chooser = GLib.Object.GetObject (ptr, false) as Gtk.Dialog;

			chooser.AddButton (Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);

			/* Note that we use Ok rather than the preferred Accept because
			 * that's what GtkFileSelection uses.  Rather than have two
			 * separate tests for each case (chooser/filesel), we'll rather
			 * just test for a single response code, which is Ok.
			 */
			chooser.AddButton (stock, Gtk.ResponseType.Ok);
			chooser.DefaultResponse = Gtk.ResponseType.Ok;
		}

		private void create_with_file_selection (string title, 
								Gtk.Window parent, Action action)
		{
			filesel = new Gtk.FileSelection (title);
			filesel.TransientFor = parent;

			/* We try to present as similar a UI as possible with both file
			 * selection widgets, so we frob the file operation buttons for
			 * each special case.
			 */
			switch (action)
			{
				case Action.Open:
					filesel.ShowFileops = false;
					break;

				case Action.Save:
					filesel.FileopDelFile.Hide ();
					filesel.FileopRenFile.Hide ();
					break;

				case Action.SelectFolder:
					filesel.FileList.Parent.Hide ();
					filesel.SelectionEntry.Hide ();
					filesel.FileopDelFile.Hide ();
					filesel.FileopRenFile.Hide ();
					break;
			}
		}

		/* Imports */

		[DllImport("libgtk-x11-2.0.so.0")]
//		[DllImport ("libgtk-win32-2.0-0.dll")]
			extern static IntPtr gtk_file_chooser_dialog_new (string title,
						IntPtr parent, int action, IntPtr varargs);

		[DllImport("libgtk-x11-2.0.so.0")]
//		[DllImport ("libgtk-win32-2.0-0.dll")]
			extern static string gtk_file_chooser_get_filename (IntPtr handle);

		[DllImport("libgtk-x11-2.0.so.0")]
//		[DllImport ("libgtk-win32-2.0-0.dll")]
			extern static int gtk_file_chooser_set_filename (IntPtr handle,
						string filename);

		[DllImport("libgtk-x11-2.0.so.0")]
//		[DllImport ("libgtk-win32-2.0-0.dll")]
			extern static IntPtr gtk_file_chooser_get_filenames (IntPtr handle);

		[DllImport("libgtk-x11-2.0.so.0")]
//		[DllImport ("libgtk-win32-2.0-0.dll")]
			extern static bool gtk_file_chooser_get_select_multiple (
						IntPtr handle);

		[DllImport("libgtk-x11-2.0.so.0")]
//		[DllImport ("libgtk-win32-2.0-0.dll")]
			extern static void gtk_file_chooser_set_select_multiple (
						IntPtr handle, bool multi);

		[DllImport("libgtk-x11-2.0.so.0")]
//		[DllImport ("libgtk-win32-2.0-0.dll")]
			extern static bool gtk_file_chooser_set_current_folder (
						IntPtr handle, string filename);

		[DllImport("libgtk-x11-2.0.so.0")]
//		[DllImport ("libgtk-win32-2.0-0.dll")]
			extern static string gtk_file_chooser_get_current_folder (
						IntPtr handle);
	}
}

