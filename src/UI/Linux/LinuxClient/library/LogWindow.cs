/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
  *
  *                 $Author: Calvin Gaisford <cgaisford@novell.com>
  *                          Boyd Timothy <btimothy@novell.com> 
  *                 $Modified by: <Modifier>
  *                 $Mod Date: <Date Modified>
  *                 $Revision: 0.0
  *-----------------------------------------------------------------------------
  * This module is used to:
  *        <Description of the functionality of the file >
  *
  *
  *******************************************************************************/

using System;
using System.IO;
using System.Collections;
using Gtk;
using Simias.Client;
using Simias.Client.Event;
using Novell.iFolder.Events;
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
//		private Manager				simiasManager;
		private SimiasEventBroker	simiasEventBroker;
		

		/// <summary>
		/// Default constructor for LogWindow
		/// </summary>
		public LogWindow(Manager simiasManager)
			: base (Util.GS("iFolder Synchronization Log"))
		{
//			this.simiasManager = simiasManager;

			CreateWidgets();

			// Bind ESC and C-w to close the window
			ControlKeyPressed = false;
			KeyPressEvent += new KeyPressEventHandler(KeyPressHandler);
			KeyReleaseEvent += new KeyReleaseEventHandler(KeyReleaseHandler);
			
			simiasEventBroker = SimiasEventBroker.GetSimiasEventBroker();
			if (simiasEventBroker != null)
			{
				simiasEventBroker.CollectionSyncEventFired +=
					new CollectionSyncEventHandler(OniFolderSyncEvent);
				simiasEventBroker.FileSyncEventFired +=
					new FileSyncEventHandler(OniFolderFileSyncEvent);
			}
		}

        /// <summary>
        /// Destructor
        /// </summary>
		~LogWindow()
		{
			if (simiasEventBroker != null)
			{
				simiasEventBroker.CollectionSyncEventFired -=
					new CollectionSyncEventHandler(OniFolderSyncEvent);
				simiasEventBroker.FileSyncEventFired -=
					new FileSyncEventHandler(OniFolderFileSyncEvent);
			}
		}

        /// <summary>
        /// Event Handler for Key Press Handler
        /// </summary>
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
		
        /// <summary>
        /// Event Handler for Key Release handler
        /// </summary>
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
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));
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


        /// <summary>
        /// Log Message
        /// </summary>
        /// <param name="logEntry">message</param>
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



        /// <summary>
        /// On iFolder Sync Event
        /// </summary>
        private void OniFolderSyncEvent(object o, CollectionSyncEventArgs args)
		{
			if (args == null || args.ID == null || args.Name == null)
				return;	// Prevent a null object exception

			switch(args.Action)
			{
				case Simias.Client.Event.Action.StartLocalSync:
					if (args.Name != null && args.Name.StartsWith("POBox:"))
					{
						DomainController domainController = DomainController.GetDomainController();
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
						DomainController domainController = DomainController.GetDomainController();
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


        /// <summary>
        /// On iFolder File Sync Event
        /// </summary>
        private void OniFolderFileSyncEvent(object o, FileSyncEventArgs args)
		{
			if (args == null || args.CollectionID == null || args.Name == null)
				return;	// Prevent a null object exception

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
					case SyncStatus.Busy:
						message = string.Format(
							Util.GS("Could not synchronize because the server is busy: {0}"),
							args.Name);
						break;
					case SyncStatus.ClientError:
						message = string.Format(
							Util.GS("Client sent bad data and could not synchronize: {0}"),
							args.Name);
						break;
					case SyncStatus.InUse:
						message = string.Format(
							Util.GS("Could not synchronize because this file is in use: {0}"),
							args.Name);
						break;
					case SyncStatus.ServerFailure:
						message = string.Format(
							Util.GS("Updating the metadata for this file failed: {0}"),
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

        /// <summary>
        /// Event Handler for Save Log
        /// </summary>
        private void SaveLogHandler(object sender, EventArgs args)
		{
			SaveLog();
		}

        /// <summary>
        /// Save Log
        /// </summary>
		private void SaveLog()
		{
			try
			{
				int rc = 0;
				bool saveFile = true;
				string filename = null;
				
				string initialPath = Util.LastSavedSyncLogPath;
				
				FileChooserDialog fcd = new FileChooserDialog(
					Util.GS("Save as..."), this,
					FileChooserAction.Save,
					Stock.Cancel, ResponseType.Cancel,
	                Stock.Save, ResponseType.Ok);
	
	            fcd.SelectMultiple = false;
	            
	            fcd.CurrentName = Util.GS("iFolder Synchronization Log.txt");
	            
	            if (initialPath != null)
	            	fcd.SetCurrentFolder(initialPath);
	
				while (saveFile)
				{
					rc = fcd.Run();
					fcd.Hide();
		
					if(rc == (int)ResponseType.Ok)
					{
						filename = fcd.Filename;
						
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
	
				fcd.Destroy();
			}
			catch(Exception e)
			{
				iFolderMsgDialog dg = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Error,
					iFolderMsgDialog.ButtonSet.Ok,
					"",
					Util.GS("Error saving the log file"),
					Util.GS("The iFolder Client experienced an exception trying to save the log.  Please report this bug.")
					+ "\n\n" +
					Util.GS("Please ensure you have the lastest updates of gtk2 and gtk-sharp2 installed on your system."));
				dg.Run();
				dg.Hide();
				dg.Destroy();
			}
		}

        /// <summary>
        /// Event Handler for Clear Log Handler
        /// </summary>
        private void ClearLogHandler(object sender, EventArgs args)
		{
			ClearLog();
		}


        /// <summary>
        /// Clear Log
        /// </summary>
		private void ClearLog()
		{
			LogTreeStore.Clear();
			SaveButton.Sensitive = false;
			ClearButton.Sensitive = false;
		}

	}
}
