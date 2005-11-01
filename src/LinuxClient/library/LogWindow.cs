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
using Simias.Client;
using Simias.Client.Event;
using Novell.iFolder.Controller;

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
		private Tooltips			ToolbarTooltips;
		private TreeView			LogTreeView;
		private ListStore			LogTreeStore;
		private ToolButton			SaveButton;
		private ToolButton			ClearButton;
		private bool				ControlKeyPressed;
		private Manager				simiasManager;
		

		/// <summary>
		/// Default constructor for LogWindow
		/// </summary>
		public LogWindow(Manager simiasManager)
			: base (Util.GS("iFolder Synchronization Log"))
		{
			this.simiasManager = simiasManager;

			CreateWidgets();

			// Bind ESC and C-w to close the window
			ControlKeyPressed = false;
			KeyPressEvent += new KeyPressEventHandler(KeyPressHandler);
			KeyReleaseEvent += new KeyReleaseEventHandler(KeyReleaseHandler);
		}

		void KeyPressHandler(object o, KeyPressEventArgs args)
		{
			args.RetVal = true;
			
			switch(args.Event.Key)
			{
				case Gdk.Key.Escape:
					Hide();
					Destroy();
					break;
				case Gdk.Key.Control_L:
				case Gdk.Key.Control_R:
					ControlKeyPressed = true;
					args.RetVal = false;
					break;
				case Gdk.Key.W:
				case Gdk.Key.w:
					if (ControlKeyPressed)
					{
						Hide();
						Destroy();
					}
					else
						args.RetVal = false;
					break;
				default:
					args.RetVal = false;
					break;
			}
		}
		
		void KeyReleaseHandler(object o, KeyReleaseEventArgs args)
		{
			args.RetVal = false;
			
			switch(args.Event.Key)
			{
				case Gdk.Key.Control_L:
				case Gdk.Key.Control_R:
					ControlKeyPressed = false;
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Set up the UI inside the Window
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
			ToolbarTooltips = new Tooltips();

			SaveButton = new ToolButton(Gtk.Stock.Save);
			SaveButton.SetTooltip(ToolbarTooltips, Util.GS("Save the synchronization log"), "Toolbar/Save Log");
			SaveButton.Clicked += new EventHandler(SaveLogHandler);
			tb.Insert(SaveButton, -1);

			ClearButton = new ToolButton(Gtk.Stock.Clear);
			ClearButton.SetTooltip(ToolbarTooltips, Util.GS("Clear the synchronization log"), "Toolbar/Clear Log");
			ClearButton.Clicked += new EventHandler(ClearLogHandler);
			tb.Insert(ClearButton, -1);

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

			// Set up the iFolder TreeView
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
				case Simias.Client.Event.Action.StartLocalSync:
					if (args.Name != null && args.Name.StartsWith("POBox:"))
					{
						DomainController domainController = DomainController.GetDomainController(simiasManager);
						DomainInformation domain = domainController.GetPOBoxDomain(args.ID);
						if (domain != null)
							LogMessage(string.Format(Util.GS("Checking for new iFolders: {0}"), domain.Name));
						else
							LogMessage(Util.GS("Checking for new iFolders..."));
					}
					else
					{
						LogMessage(string.Format(Util.GS(
							"Checking for changes: {0}"), args.Name));
					}
					break;
				case Simias.Client.Event.Action.StartSync:
				{
					// We only need to add a log entry for the PO Box in the
					// StartLocalSync case.  Moved that code there.
					if (args.Name != null && !args.Name.StartsWith("POBox:"))
					{
						LogMessage(string.Format(Util.GS(
							"Started synchronization: {0}"), args.Name));
					}
					break;
				}
				case Simias.Client.Event.Action.StopSync:
				{
					if (args.Name != null && args.Name.StartsWith("POBox:"))
					{
						DomainController domainController = DomainController.GetDomainController(simiasManager);
						DomainInformation domain = domainController.GetPOBoxDomain(args.ID);
						if (domain != null)
							LogMessage(string.Format(Util.GS("Done checking for new iFolders: {0}"), domain.Name));
						else
							LogMessage(Util.GS("Done checking for new iFolders"));
					}
					else
					{
						LogMessage(string.Format(Util.GS(
							"Finished synchronization: {0}"), args.Name));
					}
					break;
				}
			}
		}


		public void HandleFileSyncEvent(FileSyncEventArgs args)
		{
			try
			{
				string message = null;
				switch (args.Status)
				{
					case SyncStatus.Success:
						if(args.SizeRemaining == args.SizeToSync)
						{
							switch(args.ObjectType)
							{
								case ObjectType.File:
									if(args.Delete)
									{
										message = string.Format(Util.GS(
											"Deleting file: {0}"), args.Name);
									}
									else if (args.Direction == Simias.Client.Event.Direction.Local)
									{
										message = string.Format(
											Util.GS("Found changes in file: {0}"),
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
											Util.GS("Deleting directory: {0}"),
											args.Name);
									}
									else if (args.Direction == Simias.Client.Event.Direction.Local)
									{
										message = string.Format(
											Util.GS("Found changes in directory: {0}"),
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
						}

						break;
					case SyncStatus.UpdateConflict:
					case SyncStatus.FileNameConflict:
						message = string.Format(
							Util.GS("Conflict occurred: {0}"),
							args.Name);
						break;
					case SyncStatus.Policy:
						message = string.Format(
							Util.GS("Policy prevented synchronization: {0}"),
							args.Name);
						break;
					case SyncStatus.Access:
						message = string.Format(
							Util.GS("Insuficient rights prevented synchronization: {0}"),
							args.Name);
						break;
					case SyncStatus.Locked:
						message = string.Format(
							Util.GS("Locked iFolder prevented synchronization: {0}"),
							args.Name);
						break;
					case SyncStatus.PolicyQuota:
						message = string.Format(
							Util.GS("Full iFolder prevented synchronization: {0}"),
							args.Name);
						break;
					case SyncStatus.PolicySize:
						message = string.Format(
							Util.GS("Size restriction policy prevented synchronization: {0}"),
							args.Name);
						break;
					case SyncStatus.PolicyType:
						message = string.Format(
							Util.GS("File type restriction policy prevented synchronization: {0}"),
							args.Name);
						break;
					case SyncStatus.DiskFull:
						if (args.Direction == Simias.Client.Event.Direction.Uploading)
						{
							message = string.Format(
								Util.GS("Insufficient disk space on the server prevented synchronization: {0}"),
								args.Name);
						}
						else
						{
							message = string.Format(
								Util.GS("Insufficient disk space on this computer prevented synchronization: {0}"),
								args.Name);
						}
						break;
					case SyncStatus.ReadOnly:
						message = string.Format(
							Util.GS("Read-only iFolder prevented synchronization: {0}"),
							args.Name);
						break;
					default:
						message = string.Format(
							Util.GS("iFolder failed synchronization: {0}"),
							args.Name);
						break;
				}

				if (message != null && message.Length > 0)
					LogMessage(message);
			}
			catch {}
		}

		private void SaveLogHandler(object sender, EventArgs args)
		{
			SaveLog();
		}

		private void SaveLog()
		{
			int rc = 0;
			bool saveFile = true;
			string filename = null;
			
			string initialPath = Util.LastSavedSyncLogPath;

			// Switched out to use the compatible file selector
			CompatFileChooserDialog cfcd = new CompatFileChooserDialog(
				Util.GS("Save iFolder Log..."), this, 
				CompatFileChooserDialog.Action.Save);

			if (initialPath != null)
				cfcd.CurrentFolder = initialPath;

			while (saveFile)
			{
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
							"",
							Util.GS("Overwrite the existing file?"),
							Util.GS("The file you selected exists.  Selecting yes will overwrite the contents of this file."));
						rc = dialog.Run();
						dialog.Hide();
						dialog.Destroy();
						if(rc != -8)
							saveFile = false;
					}
				}
				else
					break;	// out of the while loop
	
				if(saveFile)
				{
					FileStream fs = null;
					try
					{
						fs = File.Create(filename);
					}
					catch (System.UnauthorizedAccessException uae)
					{
						iFolderMsgDialog dg = new iFolderMsgDialog(
							this,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.Ok,
							"",
							Util.GS("Insufficient access"),
							Util.GS("You do not have access to save the file in the location you specified.  Please select a different location."));
						dg.Run();
						dg.Hide();
						dg.Destroy();

						continue;	// To the next iteration of the while loop
					}
					catch (Exception e)
					{
						iFolderMsgDialog dg = new iFolderMsgDialog(
							this,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.Ok,
							"",
							Util.GS("Error saving the log file"),
							string.Format(Util.GS("An exception occurred trying to save the log: {0}"), e.Message),
							e.StackTrace);
						dg.Run();
						dg.Hide();
						dg.Destroy();

						continue;	// To the next iteration of the while loop
					}
	
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
	
						Util.LastSavedSyncLogPath = filename;
	
						break;
					}
					else
					{
						iFolderMsgDialog dg = new iFolderMsgDialog(
							this,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.Ok,
							"",
							Util.GS("Error saving the log file"),
							Util.GS("The iFolder Client experienced an error trying to save the log.  Please report this bug."));
						dg.Run();
						dg.Hide();
						dg.Destroy();
					}
				}
			}

			cfcd.Destroy();
		}


		private void ClearLogHandler(object sender, EventArgs args)
		{
			ClearLog();
		}


		private void ClearLog()
		{
			LogTreeStore.Clear();
			SaveButton.Sensitive = false;
			ClearButton.Sensitive = false;
		}

	}
}
