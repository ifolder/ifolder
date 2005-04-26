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
 *  Authors:
 *		Calvin Gaisford <cgaisford@novell.com>
 *		Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/


using System;
using System.IO;
using System.Collections;
using Gtk;
using Simias.Client.Event;

namespace Novell.iFolder
{
	/// <summary>
	/// This is the iFolder Log Window
	/// </summary>
	public class LogWindow : Window
	{
//		private string				SyncFileName = "";

//		private iFolderWebService	iFolderWS;

		private Toolbar				toolbar;
		private TreeView			LogTreeView;
		private ListStore			LogTreeStore;
		private Widget				SaveButton;
		private Widget				ClearButton;
		

		/// <summary>
		/// Default constructor for LogWindow
		/// </summary>
		public LogWindow()
			: base (Util.GS("iFolder Synchronization Log"))
		{
			CreateWidgets();
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void CreateWidgets()
		{
			this.SetDefaultSize (500, 400);
			this.DeleteEvent += new DeleteEventHandler(WindowDeleteHandler);
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder24.png"));
			this.WindowPosition = Gtk.WindowPosition.Center;

			VBox vbox = new VBox (false, 0);
			this.Add (vbox);

			//-----------------------------
			// Create the Toolbar
			//-----------------------------
			toolbar = CreateToolbar();
			vbox.PackStart (toolbar, false, false, 0);

			//-----------------------------
			// Create the Tree View
			//-----------------------------
			vbox.PackStart(SetupTreeView(), true, true, 0);
		}




		/// <summary>
		/// Creates the Toolbar for the iFolder Window
		/// </summary>
		/// <returns>
		/// Toolbar for the window
		/// </returns>
		private Toolbar CreateToolbar()
		{
			Toolbar tb = new Toolbar();

			SaveButton = tb.AppendItem(Util.GS("Save"), 
				Util.GS("Save the Log to a file"), "Toolbar/Save Log",
				new Image(Stock.Save, Gtk.IconSize.LargeToolbar),
				new SignalFunc(SaveLog));

			ClearButton = tb.AppendItem(Util.GS("Clear"), 
				Util.GS("Clear the log"), "Toolbar/Clear Log",
				new Image(Stock.Clear, Gtk.IconSize.LargeToolbar),
				new SignalFunc(ClearLog));

			SaveButton.Sensitive = false;
			ClearButton.Sensitive = false;
			return tb;
		}




		/// <summary>
		/// Creates the TreeView for the Log
		/// </summary>
		/// <returns>
		/// Widget to display
		/// </returns>
		private Widget SetupTreeView()
		{
			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.None;
			LogTreeView = new TreeView();
			sw.Add(LogTreeView);
			LogTreeView.HeadersVisible = false;

			// Setup the iFolder TreeView
			LogTreeStore = new ListStore(typeof(string));
			LogTreeView.Model = LogTreeStore;

			CellRendererText logcr = new CellRendererText();
			logcr.Xpad = 10;
			LogTreeView.AppendColumn(Util.GS("Log"), logcr, "text", 0);

			return sw;
		}



		private void LogMessage(string logEntry)
		{
			TreeIter iter;

			while(LogTreeStore.IterNChildren() > 500)
			{
				if(LogTreeStore.GetIterFirst(out iter))
				{
					LogTreeStore.Remove(ref iter);
				}
			}

			iter = LogTreeStore.AppendValues(string.Format(
							"{0} {1}", DateTime.Now.ToString(), logEntry));

			TreePath path = LogTreeStore.GetPath(iter);

			LogTreeView.ScrollToCell(path, null, true, 1, 1);	

			SaveButton.Sensitive = true;
			ClearButton.Sensitive = true;
		}




		// This message is sent when the window is deleted 
		// or the X is clicked.  We just want to hide it so
		// we set the args.RetVal to true saying we handled the
		// delete even when we didn't
		private void WindowDeleteHandler(object o, DeleteEventArgs args)
		{
			this.Hide ();
			args.RetVal = true;
		}




		public void HandleSyncEvent(CollectionSyncEventArgs args)
		{
			switch(args.Action)
			{
				case Action.StartLocalSync:
					LogMessage(string.Format(Util.GS(
						"Checking for local changes: {0}"), args.Name));
					break;
				case Action.StartSync:
				{
					LogMessage(string.Format(Util.GS(
						"Started sync of: {0}"), args.Name));
					break;
				}
				case Action.StopSync:
				{
					LogMessage(string.Format(Util.GS(
						"Finished sync of: {0}"), args.Name));
					break;
				}
			}
		}


		public void HandleFileSyncEvent(FileSyncEventArgs args)
		{
			if(args.SizeRemaining == args.SizeToSync)
			{
				string message = null;
				switch(args.ObjectType)
				{
					case ObjectType.File:
						if(args.Delete)
						{
							message = string.Format(Util.GS(
								"Deleting file on client: {0}"), args.Name);
						}
						else if (args.Direction == Simias.Client.Event.Direction.Local)
						{
							message = string.Format(
								Util.GS("Found local change in file: {0}"),
								args.Name);
						}
						else if (args.SizeToSync < args.Size)
						{
							// Note: Delta sync works on 4KB blocks, so you won't see delta
							// sync messages until files reach at least 4KB.
							int savings = (int)((1 - ((double)args.SizeToSync / (double)args.Size)) * 100);
							if (args.Direction == Simias.Client.Event.Direction.Uploading)
								message = string.Format(
									Util.GS("Uploading file: {0}.  Synchronizing changes only: {1}% savings."),
									args.Name,
									savings);
							else
								message = string.Format(
									Util.GS("Downloading file: {0}.  Synchronizing changes only: {1}% savings."),
								args.Name,
								savings);
						}
						else
						{
							if (args.Direction == Simias.Client.Event.Direction.Uploading)
								message = string.Format(
									Util.GS("Uploading file: {0}"),
									args.Name);
							else
								message = string.Format(
									Util.GS("Downloading file: {0}"),
									args.Name);
						}
						break;
					case ObjectType.Directory:
						if (args.Delete)
						{
							message = string.Format(
								Util.GS("Deleting directory on client: {0}"),
								args.Name);
						}
						else if (args.Direction == Simias.Client.Event.Direction.Local)
						{
							message = string.Format(
								Util.GS("Found local change in directory: {0}"),
								args.Name);
						}
						else
						{
							if (args.Direction == Simias.Client.Event.Direction.Uploading)
								message = string.Format(
									Util.GS("Uploading directory: {0}"),
									args.Name);
							else
								message = string.Format(
									Util.GS("Downloading directory: {0}"),
									args.Name);
						}
						break;
					case ObjectType.Unknown:
						message = string.Format(
							Util.GS("Deleting on server: {0}"),
							args.Name);
						break;
				}

				LogMessage(message);
			}
		}


		private void SaveLog()
		{
			int rc = 0;
			bool saveFile = false;
			string filename = null;

			// Switched out to use the compatible file selector
			CompatFileChooserDialog cfcd = new CompatFileChooserDialog(
				Util.GS("Save iFolder Log..."), this, 
				CompatFileChooserDialog.Action.Save);

			rc = cfcd.Run();
			cfcd.Hide();

			if(rc == -5)
			{
				filename = cfcd.Selections[0];

				if(File.Exists(filename))
				{
					iFolderMsgDialog dialog = new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Question,
						iFolderMsgDialog.ButtonSet.YesNo,
						Util.GS("iFolder Save Log"),
						Util.GS("Overwrite existing file?"),
						Util.GS("The file you have selected exists.  Selecting yes will overwrite the contents of this file.  Do you want to overwrite this file?"));
					rc = dialog.Run();
					dialog.Hide();
					dialog.Destroy();
					if(rc == -8)
					{
						saveFile = true;
					}
				}
				else
					saveFile = true;
			}

			if(saveFile)
			{
				FileStream fs = File.Create(filename);
				if(fs != null)
				{
					TreeIter iter;
					StreamWriter w = new StreamWriter(fs);

					if(LogTreeStore.GetIterFirst(out iter))
					{
						string logEntry = 
							(string)LogTreeStore.GetValue(iter, 0);

						w.WriteLine(logEntry);

						while(LogTreeStore.IterNext(ref iter))
						{
							logEntry = 
								(string)LogTreeStore.GetValue(iter, 0);

							w.WriteLine(logEntry);
						}
					}
					
					w.Close();
				}
			}
		}



		private void ClearLog()
		{
			LogTreeStore.Clear();
			SaveButton.Sensitive = false;
			ClearButton.Sensitive = false;
		}

	}
}
