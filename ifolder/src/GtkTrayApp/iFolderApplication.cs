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
using System.Collections;
//using Simias;
//using Simias.Sync;
//using Simias.Domain;
using System.Diagnostics;
using System.Threading;

using Gtk;
using Gdk;
using Gnome;
//using Glade;
using GtkSharp;
using GLib;
using Egg;
using Simias.Event;
using Simias;
using Simias.Storage;



namespace Novell.iFolder
{
	public enum iFolderState : uint
	{
		Starting		= 0x0001,
		Stopping		= 0x0002,
		Running			= 0x0003,
		Stopped			= 0x0004
	}


	public enum iFolderEventTypes : uint
	{
		NodeCreated		= 0x0001,
		NodeChanged		= 0x0002,
		NodeDeleted		= 0x0003,
		Exception		= 0x0004
	}

	public class iFolderEvent
	{
		private NodeEventArgs		eventArgs;
		private iFolderEventTypes	eventType;
		private string				exceptionMessage;
	
		public iFolderEvent(NodeEventArgs args, iFolderEventTypes type)
		{
			this.eventArgs = args;
			this.eventType = type;
		}

		public iFolderEvent(string exceptionMessage)
		{
			this.exceptionMessage = exceptionMessage;
			this.eventType = iFolderEventTypes.Exception;
		}

		public string Message
		{
			get{ return exceptionMessage; }
		}

		public iFolderEventTypes EventType
		{
			get{ return eventType; }
		}

		public string NodeID
		{
			get{return eventArgs.Node;}
		}

		public string CollectionID
		{
			get{return eventArgs.Collection;}
		}

		public string NodeType
		{
			get{return eventArgs.Type;}
		}

		public bool LocalOnly
		{
			get{ return eventArgs.LocalOnly; }
		}
	}
	

	public class iFolderApplication : Gnome.Program
	{
		private Gtk.Image			gAppIcon;
		private Gdk.Pixbuf			RunningPixbuf;
		private Gdk.Pixbuf			StartingPixbuf;
		private Gdk.Pixbuf			StoppingPixbuf;
		private Gtk.EventBox		eBox;
		private TrayIcon			tIcon;
		private iFolderWebService	ifws;
		private iFolderWindow 		ifwin;
		private iFolderSettings		ifSettings;
		private IProcEventClient	simiasEventClient;

		private iFolderState 		CurrentState;
		private Gtk.ThreadNotify	iFolderStateChanged;
		private Gtk.ThreadNotify	SimiasEventFired;
		private Queue				EventQueue;


		public iFolderApplication(string[] args)
			: base("iFolder", "1.0", Modules.UI, args)
		{

			Util.InitCatalog();

			tIcon = new TrayIcon("iFolder");

			eBox = new EventBox();
			eBox.ButtonPressEvent += 
				new ButtonPressEventHandler(trayapp_clicked);

			RunningPixbuf = 
					new Pixbuf(Util.ImagesPath("ifolder.png"));
			StartingPixbuf = 
					new Pixbuf(Util.ImagesPath("ifolder-startup.png"));
			StoppingPixbuf = 
					new Pixbuf(Util.ImagesPath("ifolder-shutdown.png"));

			gAppIcon = new Gtk.Image(RunningPixbuf);

			eBox.Add(gAppIcon);
			tIcon.Add(eBox);
			tIcon.ShowAll();	

			EventQueue = new Queue();

			iFolderStateChanged = new Gtk.ThreadNotify(
							new Gtk.ReadyEvent(OniFolderStateChanged));
			SimiasEventFired = new Gtk.ThreadNotify(
							new Gtk.ReadyEvent(OnSimiasEventFired) );
		}




		public new void Run()
		{
			System.Threading.Thread startupThread = 
					new System.Threading.Thread(new ThreadStart(StartiFolder));
			startupThread.Start();


			base.Run();
		}




		private void SetupSimiasEventHandlers()
		{
			simiasEventClient = new IProcEventClient( 
					new IProcEventError( ErrorHandler), null);

			simiasEventClient.Register();

			simiasEventClient.SetEvent( IProcEventAction.AddNodeCreated,
				new IProcEventHandler( SimiasEventNodeCreatedHandler ) );

			simiasEventClient.SetEvent( IProcEventAction.AddNodeChanged,
				new IProcEventHandler( SimiasEventNodeChangedHandler ) );

			simiasEventClient.SetEvent( IProcEventAction.AddNodeDeleted,
				new IProcEventHandler( SimiasEventNodeDeletedHandler ) );
		}




		private void ErrorHandler( SimiasException e, object context )
		{
			lock(EventQueue)
			{
				EventQueue.Enqueue(new iFolderEvent(e.Message));
				SimiasEventFired.WakeupMain();
			}
		}




		private void SimiasEventNodeCreatedHandler(SimiasEventArgs args)
		{
			NodeEventArgs nargs = args as NodeEventArgs;
//			Console.WriteLine("Received a Node Created Event");
			lock(EventQueue)
			{
				EventQueue.Enqueue(new iFolderEvent(nargs, 
						iFolderEventTypes.NodeCreated));
				SimiasEventFired.WakeupMain();
			}
		}




		private void SimiasEventNodeChangedHandler(SimiasEventArgs args)
		{
			NodeEventArgs nargs = args as NodeEventArgs;
//			Console.WriteLine("Received a Node Changed Event");
			lock(EventQueue)
			{
				EventQueue.Enqueue(new iFolderEvent(nargs, 
						iFolderEventTypes.NodeChanged));
				SimiasEventFired.WakeupMain();
			}
		}




		private void SimiasEventNodeDeletedHandler(SimiasEventArgs args)
		{
			NodeEventArgs nargs = args as NodeEventArgs;
//			Console.WriteLine("Received a Node Deleted Event");
			lock(EventQueue)
			{
				EventQueue.Enqueue(new iFolderEvent(nargs, 
						iFolderEventTypes.NodeDeleted));
				SimiasEventFired.WakeupMain();
			}
		}





		private void StartiFolder()
		{
			CurrentState = iFolderState.Starting;
			iFolderStateChanged.WakeupMain();

/*			Process[] processes = 
				System.Diagnostics.Process.GetProcessesByName("SimiasApp");

			if(processes.Length > 1)
			{
				Console.WriteLine("Simias is already running!");
				return;
			}
			else
			//if(SimiasProcess == null)
			{
				SimiasProcess = new Process();
				SimiasProcess.StartInfo.RedirectStandardOutput = false;
				SimiasProcess.StartInfo.CreateNoWindow = true;
				SimiasProcess.StartInfo.UseShellExecute = false;
				SimiasProcess.StartInfo.FileName = 
					"/home/calvin/test/bin/simias";
				SimiasProcess.Start();
			}
*/

			if(ifws == null)
			{
				try
				{
					ifws = new iFolderWebService();
	
					ifSettings = ifws.GetSettings();
				}
				catch(Exception e)
				{
					ifws = null;
					ifSettings = null;
				}
			}

			CurrentState = iFolderState.Running;
			iFolderStateChanged.WakeupMain();
		}




		private void StopiFolder()
		{
			CurrentState = iFolderState.Stopping;
			iFolderStateChanged.WakeupMain();

			simiasEventClient.Deregister();

/*			if(SimiasProcess != null)
			{
				SimiasProcess.CloseMainWindow();
				if(!SimiasProcess.WaitForExit(10000))
				{
					SimiasProcess.Kill();
					if(!SimiasProcess.WaitForExit(100))
					{
						Console.WriteLine("The process won't die");
					}
				}
			}
*/

			CurrentState = iFolderState.Stopped;
			iFolderStateChanged.WakeupMain();
		}




		// ThreadNotify Method that will react to a fired event
		private void OnSimiasEventFired()
		{
			iFolderEvent iEvent;
			bool hasmore = false;
			// at this point, we are running in the same thread
			// so we can safely show events
			lock(EventQueue)
			{
				hasmore = (EventQueue.Count > 0);
			}

			while(hasmore)
			{
				lock(EventQueue)
				{
					iEvent = (iFolderEvent)EventQueue.Dequeue();
					hasmore = (EventQueue.Count > 0);
				}
				
				switch(iEvent.EventType)
				{
					case iFolderEventTypes.Exception:
					{
						// TODO: Not sure what to do here
						break;
					}
	
					case iFolderEventTypes.NodeCreated:
					{
						HandleNodeCreatedEvent(iEvent);
						break;
					}
	
					case iFolderEventTypes.NodeChanged:
					{
						HandleNodeChangedEvent(iEvent);
						break;
					}
	
					case iFolderEventTypes.NodeDeleted:
					{
						HandleNodeDeletedEvent(iEvent);
						break;
					}
				}
			}
		}




		private void HandleNodeChangedEvent(iFolderEvent iEvent)
		{
			switch(iEvent.NodeType)
			{
				case "Collection":
				{
					try
					{
						iFolder ifolder = 
								ifws.GetiFolder(iEvent.CollectionID);
						if( (ifolder != null) && (ifolder.HasConflicts) )
						{
							NotifyWindow notifyWin = new NotifyWindow(
									tIcon, Util.GS("Action Required"),
									string.Format(Util.GS("A collision has been detected in iFolder \"{0}\""), ifolder.Name),
									Gtk.MessageType.Info, 5000);
							notifyWin.ShowAll();

							if(ifwin != null)
								ifwin.iFolderHasConflicts(ifolder);
						}
					}
					catch(Exception e)
					{
//						iFolderExceptionDialog ied = 
//								new iFolderExceptionDialog(null, e);
//						ied.Run();
//						ied.Hide();
//						ied.Destroy();
//						ied = null;
					}

					break;
				}

				case "Node":
				{
					// Check to see if the Node that changed is part of
					// the POBox
					if(iEvent.CollectionID == ifSettings.DefaultPOBoxID)
					{
						if(ifwin != null)
							ifwin.iFolderChanged(iEvent.NodeID);
					}
					break;
				}					
			}
		}




		private void HandleNodeCreatedEvent(iFolderEvent iEvent)
		{
			switch(iEvent.NodeType)
			{
				case "Node":
				{
//					Console.WriteLine("Handling a node CreatedEvent");
					// Check to see if the Node that changed is part of
					// the POBox
					if((ifSettings != null) && (iEvent.CollectionID == ifSettings.DefaultPOBoxID) )
					{
						iFolder ifolder;
						try
						{
							ifolder = ifws.GetiFolder(iEvent.NodeID);
						}
						catch(Exception e)
						{
							ifolder = null;
						}

						if(	(ifolder != null) &&
							(ifolder.State == "Available") )
						{
							// At this point we know it's a new subscription
							// that's available, now check to make sure
							// the corresponding iFolder isn't on the
							// machine already (it was created here)
							iFolder localiFolder;
							try
							{
								localiFolder = 
									ifws.GetiFolder(ifolder.CollectionID);
							}
							catch(Exception e)
							{
								localiFolder = null;
							}

							if(localiFolder != null)
								return;
								
							NotifyWindow notifyWin = new NotifyWindow(
								tIcon, 
								string.Format(Util.GS("New iFolder \"{0}\""), 
													ifolder.Name),
								string.Format(Util.GS("This iFolder is owned by {0} and is available to sync on this computer"), ifolder.Owner),
									Gtk.MessageType.Info, 5000);
								notifyWin.ShowAll();
	
							if(ifwin != null)
								ifwin.iFolderCreated(ifolder);
						}
					}
					break;
				}					

				case "Member":
				{
					try
					{
						iFolderUser newuser = ifws.GetiFolderUserFromNodeID(
							iEvent.CollectionID, iEvent.NodeID);
						if( (newuser != null) &&
							(newuser.UserID != ifSettings.CurrentUserID) )
						{
							iFolder ifolder = 
									ifws.GetiFolder(iEvent.CollectionID);
						
							NotifyWindow notifyWin = new NotifyWindow(
									tIcon, Util.GS("New iFolder User"), 
									string.Format(Util.GS("{0} has just joined iFolder {1}"), newuser.Name, ifolder.Name),
									Gtk.MessageType.Info, 5000);

							notifyWin.ShowAll();
							
							// TODO: update any open windows?
//							if(ifwin != null)
//								ifwin.NewiFolderUser(ifolder, newuser);
						}
					}
					catch(Exception e)
					{
//						iFolderExceptionDialog ied = 
//								new iFolderExceptionDialog(null, e);
//						ied.Run();
//						ied.Hide();
//						ied.Destroy();
//						ied = null;
					}
					break;
				}

				case "Collection":
				{
					try
					{
						iFolder ifolder = 
								ifws.GetiFolder(iEvent.CollectionID);
						if(ifolder != null)
						{
							// This is happening because we have an iFolder
							// that we accepted down on this machine
							if(ifwin != null)
								ifwin.iFolderCreated(ifolder);
						}
					}
					catch(Exception e)
					{
//						iFolderExceptionDialog ied = 
//								new iFolderExceptionDialog(null, e);
//						ied.Run();
//						ied.Hide();
//						ied.Destroy();
//						ied = null;
					}

					break;
				}
			}
		}


		private void HandleNodeDeletedEvent(iFolderEvent iEvent)
		{
			switch(iEvent.NodeType)
			{
				case "Node":
				{
					if( (ifSettings != null) && (iEvent.CollectionID == ifSettings.DefaultPOBoxID) )
					{
						if(ifwin != null)
							ifwin.iFolderDeleted(iEvent.NodeID);
					}
					break;
				}
				case "Collection":
				{
					if(ifwin != null)
						ifwin.iFolderDeleted(iEvent.NodeID);
					break;
				}
			}
		}



		// ThreadNotify Method that will react to a fired event
		private void OniFolderStateChanged()
		{
			switch(CurrentState)
			{
				case iFolderState.Starting:
					gAppIcon.Pixbuf = StartingPixbuf;
					break;

				case iFolderState.Running:
					gAppIcon.Pixbuf = RunningPixbuf;
					SetupSimiasEventHandlers();
					break;

				case iFolderState.Stopping:
					gAppIcon.Pixbuf = StoppingPixbuf;
					break;

				case iFolderState.Stopped:
					Application.Quit();
					break;
/*				case iFolderState.NewFolder:
				{
					NotifyWindow notifyWin = new NotifyWindow(tIcon,
									"Node Created!",
									"Simias Reports a new node!", 4000);
					notifyWin.ShowAll();
					break;
				}
				case iFolderState.NewMember:
				case iFolderEvent.NewConflict:
				case iFolderEvent.NewERROR:
				{
					NotifyWindow notifyWin = new NotifyWindow(tIcon,
									"Simias Error Occurred!",
									"Something very baaaad happened", 4000);
					notifyWin.ShowAll();
					break;
				}
*/
			}
		}




		private bool CheckWebService()
		{
			if(ifws == null)
			{
				try
				{
					ifws = new iFolderWebService();
	
					ifSettings = ifws.GetSettings();
				}
				catch(System.Net.WebException we)
				{
					ifSettings = null;
					ifws = null;

					if(we.Message == "Error: ConnectFailure")
					{
						iFolderMsgDialog mDialog = new iFolderMsgDialog(
							null,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.Ok,
							Util.GS("iFolder Connect Error"),
							Util.GS("Unable to locate Simias Process"),
							Util.GS("The Simias process must be running in order for iFolder to run.  Start the Simias process and try again"));
						mDialog.Run();
						mDialog.Hide();
						mDialog.Destroy();
						mDialog = null;
					}
					else
						throw we;
				}
				catch(Exception e)
				{
					ifSettings = null;
					ifws = null;

					iFolderExceptionDialog ied = new iFolderExceptionDialog(
													null, e);
					ied.Run();
					ied.Hide();
					ied.Destroy();
					ied = null;
				}
			}
			return(ifws != null);
		}





		private void trayapp_clicked(object obj, ButtonPressEventArgs args)
		{
			switch(args.Event.Button)
			{
				case 1: // first mouse button
					if(args.Event.Type == Gdk.EventType.TwoButtonPress)
					{
						show_properties(obj, args);
					}
					break;
				case 2: // second mouse button
					break;
				case 3: // third mouse button
					show_tray_menu();
					break;
			}
		}




		private void show_tray_menu()
		{
			AccelGroup agrp = new AccelGroup();
			Menu trayMenu = new Menu();

			MenuItem iFolders_item = new MenuItem (Util.GS("My iFolders..."));
			trayMenu.Append (iFolders_item);
			iFolders_item.Activated += 
					new EventHandler(show_properties);
			
			if( (ifSettings != null) && (!ifSettings.HaveEnterprise) )
			{
				MenuItem connect_item = 
						new MenuItem (Util.GS("Join Enterprise Server"));
				trayMenu.Append (connect_item);
				connect_item.Activated += new EventHandler(OnJoinEnterprise);
			}

			ImageMenuItem help_item = new ImageMenuItem (Gtk.Stock.Help, agrp);
			trayMenu.Append (help_item);
			help_item.Activated += 
					new EventHandler(show_help);

			trayMenu.Append(new SeparatorMenuItem());


			ImageMenuItem quit_item = new ImageMenuItem (Gtk.Stock.Quit, agrp);
			quit_item.Activated += new EventHandler(quit_ifolder);
			trayMenu.Append (quit_item);

			trayMenu.ShowAll();

			trayMenu.Popup(null, null, null, IntPtr.Zero, 3, 
					Gtk.Global.CurrentEventTime);
		}




		private void quit_ifolder(object o, EventArgs args)
		{
			if(CurrentState == iFolderState.Stopping)
			{
				System.Environment.Exit(1);
			}
			else
			{
				System.Threading.Thread stopThread = 
					new System.Threading.Thread(new ThreadStart(StopiFolder));
				stopThread.Start();
			}
		}




		private void OnJoinEnterprise(object o, EventArgs args)
		{
			iFolderLoginDialog loginDialog = new iFolderLoginDialog();

			int rc = loginDialog.Run();
			loginDialog.Hide();
			loginDialog.Destroy();
			if(rc == -5)
			{
				try
				{
					iFolderSettings tmpSettings;
					tmpSettings = ifws.ConnectToEnterpriseServer(
													loginDialog.UserName,
													loginDialog.Password,
													loginDialog.Host);
					ifSettings = tmpSettings;

					if(ifwin != null)
					{
						ifwin.GlobalSettings = ifSettings;
					}
				}
				catch(Exception e)
				{
					iFolderExceptionDialog ied = new iFolderExceptionDialog(
													null, e);
					ied.Run();
					ied.Hide();
					ied.Destroy();
					ied = null;
				}
			}
		}




		private void show_help(object o, EventArgs args)
		{
			Util.ShowHelp("front.html", null);
		}




		private void show_about(object o, EventArgs args)
		{
			Util.ShowAbout();
		}




		private void show_properties(object o, EventArgs args)
		{
			if(CheckWebService())
			{
				if(ifwin == null)
				{
					ifwin = new iFolderWindow(ifws, ifSettings);
					ifwin.ShowAll();
				}
				else
				{
					// this will raise the window to the front
					ifwin.Present();
				}
//				iFolderWindow win;

//				win = new iFolderWindow(ifws);
//				win.ShowAll();
			}
/*
			ApplicationProperties propDialog;

			propDialog = new ApplicationProperties();
			propDialog.Run();
*/
		}




		public static void Main (string[] args)
		{
			Process[] processes = 
				System.Diagnostics.Process.GetProcessesByName("iFolderGtkApp");

			if(processes.Length > 1)
			{
				Console.WriteLine("iFolder is already running!");
				return;
			}

			try
			{
				iFolderApplication app = new iFolderApplication(args);
				app.Run();
			}
			catch(Exception bigException)
			{
				iFolderCrashDialog cd = new iFolderCrashDialog(bigException);
				cd.Run();
				cd.Hide();
				cd.Destroy();
				cd = null;
				Application.Quit();
			}
		}


	}
}
