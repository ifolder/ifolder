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
using System.Diagnostics;
using System.Threading;
using Gtk;

using Simias.Event;
using Simias;
using Simias.Storage;



namespace Novell.iFolder
{

	internal enum SimiasEventType : uint
	{
		NewUser			= 0x0001,
		NewiFolder		= 0x0002,
		ChangedUser		= 0x0003,
		ChangediFolder	= 0x0004,
		DelUser			= 0x0005,
		DeliFolder		= 0x0006
	}


	internal class SimiasEvent
	{
		private iFolder				ifolder;
		private iFolderUser			ifUser;
		private string				ifolderID;
		private string				userID;
		private SimiasEventType		type;
	
		public SimiasEvent(iFolder ifldr, iFolderUser ifldrUser, 
					string iFolderID, string UserID, SimiasEventType type)
		{
			this.ifolder = ifldr;
			this.ifUser = ifldrUser;
			this.ifolderID = iFolderID;
			this.userID = UserID;
			this.type = type;
		}

		public iFolder iFolder
		{
			get{ return this.ifolder; }
		}

		public iFolderUser iFolderUser
		{
			get{ return this.ifUser; }
		}

		public string iFolderID
		{
			get{return this.ifolderID;}
		}

		public string UserID
		{
			get{return this.userID;}
		}

		public SimiasEventType EventType
		{
			get{return this.type;}
		}
	}


	public class iFolderAddedEventArgs : EventArgs
	{
		private iFolder	ifolder;

		public iFolderAddedEventArgs(iFolder ifldr)
		{
			this.ifolder = ifldr;
		}

		public iFolder iFolder
		{
			get{ return this.ifolder; }
		}
	}
	public delegate void iFolderAddedEventHandler(object sender,
							iFolderAddedEventArgs args);
	

	public class iFolderChangedEventArgs : EventArgs
	{
		private iFolder	ifolder;

		public iFolderChangedEventArgs(iFolder ifldr)
		{
			this.ifolder = ifldr;
		}

		public iFolder iFolder
		{
			get{ return this.ifolder; }
		}
	}
	public delegate void iFolderChangedEventHandler(object sender,
							iFolderChangedEventArgs args);


	public class iFolderDeletedEventArgs : EventArgs
	{
		private string ifolderID;

		public iFolderDeletedEventArgs(string ifldID)
		{
			this.ifolderID = ifldID;
		}

		public string iFolderID
		{
			get{ return this.ifolderID; }
		}
	}
	public delegate void iFolderDeletedEventHandler(object sender,
							iFolderDeletedEventArgs args);


	public class iFolderUserAddedEventArgs : EventArgs
	{
		private iFolderUser user;
		private string ifolderID;

		public iFolderUserAddedEventArgs(iFolderUser ifUser, string iFolderID)
		{
			this.user = ifUser;
			this.ifolderID = iFolderID;
		}

		public iFolderUser iFolderUser
		{
			get{ return this.user; }
		}

		public string iFolderID
		{
			get{ return this.ifolderID; }
		}
	}
	public delegate void iFolderUserAddedEventHandler(object sender,
							iFolderUserAddedEventArgs args);


	public class iFolderUserChangedEventArgs : EventArgs
	{
		private iFolderUser user;
		private string ifolderID;

		public iFolderUserChangedEventArgs(iFolderUser ifUser, string iFolderID)
		{
			this.user = ifUser;
			this.ifolderID = iFolderID;
		}

		public iFolderUser iFolderUser
		{
			get{ return this.user; }
		}

		public string iFolderID
		{
			get{ return this.ifolderID; }
		}
	}
	public delegate void iFolderUserChangedEventHandler(object sender,
							iFolderUserChangedEventArgs args);


	public class iFolderUserDeletedEventArgs : EventArgs
	{
		private string userID;
		private string ifolderID;

		public iFolderUserDeletedEventArgs(string ifUserID, string iFolderID)
		{
			this.userID = ifUserID;
			this.ifolderID = iFolderID;
		}

		public string UserID
		{
			get{ return this.userID; }
		}

		public string iFolderID
		{
			get{ return this.ifolderID; }
		}
	}
	public delegate void iFolderUserDeletedEventHandler(object sender,
							iFolderUserDeletedEventArgs args);


	public class SimiasEventBroker
	{
		private iFolderWebService	ifws;
		private iFolderSettings		ifSettings;
		private IProcEventClient	simiasEventClient;

		private Gtk.ThreadNotify	SimiasEventFired;
		private Queue				EventQueue;

		public event iFolderAddedEventHandler iFolderAdded;
		public event iFolderChangedEventHandler iFolderChanged;
		public event iFolderDeletedEventHandler iFolderDeleted;
		public event iFolderUserAddedEventHandler iFolderUserAdded;
		public event iFolderUserChangedEventHandler iFolderUserChanged;
		public event iFolderUserDeletedEventHandler iFolderUserDeleted;

		public SimiasEventBroker()
		{
			EventQueue = new Queue();

			SimiasEventFired = new Gtk.ThreadNotify(
							new Gtk.ReadyEvent(OnSimiasEventFired) );
		}


		public void RefreshSettings()
		{
			// the SimiasEventBroker needs it's own connection to
			// the web service so it can verify data independent
			// of the main GUI
			lock(ifws)
			{
				try
				{
					ifSettings = ifws.GetSettings();
				}
				catch(Exception e)
				{
					ifSettings = null;
				}
			}
		}



		public void Register()
		{
			// the SimiasEventBroker needs it's own connection to
			// the web service so it can verify data independent
			// of the main GUI
			if(ifws == null)
			{
				try
				{
					ifws = new iFolderWebService();
					ifws.Url = 
						Simias.Client.Manager.LocalServiceUrl.ToString() +
							"/iFolder.asmx";
//					ifws.Ping();
	
					ifSettings = ifws.GetSettings();
				}
				catch(Exception e)
				{
//					ifws = null;
					ifSettings = null;
				}
			}

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




		public void Deregister()
		{
			try
			{
				simiasEventClient.Deregister();
			}
			catch(Exception e)
			{
				// ignore
			}
		}




		private void ErrorHandler( ApplicationException e, object context )
		{
/*
			lock(EventQueue)
			{
				EventQueue.Enqueue(new iFolderEvent(e.Message));
				SimiasEventFired.WakeupMain();
			}
*/
		}




		private void SimiasEventNodeCreatedHandler(SimiasEventArgs args)
		{
			NodeEventArgs nargs = args as NodeEventArgs;

			switch(nargs.Type)
			{
				case "Node":
				{
					lock(ifws)
					{
						// Check to see if the Node that changed is part of
						// the POBox
						if(	(ifSettings != null) && 
							(nargs.Collection == ifSettings.DefaultPOBoxID) )
						{
							iFolder ifolder;

							try
							{
								ifolder = ifws.GetiFolder(nargs.ID);
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
									localiFolder = ifws.GetiFolder(
												ifolder.CollectionID);
								}
								catch(Exception e)
								{
									localiFolder = null;
								}

								if(localiFolder != null)
									return;
								
								lock(EventQueue)
								{
									EventQueue.Enqueue(new SimiasEvent(
										ifolder, null, ifolder.ID, null,
										SimiasEventType.NewiFolder));
									SimiasEventFired.WakeupMain();
								}
							}
						}
					}
					break;
				}					

				case "Member":
				{
					lock(ifws)
					{
						try
						{
							// first test to see if this is an
							// ifolder.  We don't care if it is
							// not an iFolder
							iFolder localiFolder = ifws.GetiFolder(
											nargs.Collection);
							if(localiFolder != null)
							{
								iFolderUser newuser = 
									ifws.GetiFolderUserFromNodeID(
										nargs.Collection, nargs.ID);

								if( (newuser != null) &&
									(newuser.UserID != 
											ifSettings.CurrentUserID) )
								{
									lock(EventQueue)
									{
										EventQueue.Enqueue(new SimiasEvent(
											null, newuser, nargs.Collection,
											newuser.UserID,
											SimiasEventType.NewUser));
										SimiasEventFired.WakeupMain();
									}
								}
							}
						}
						catch(Exception e)
						{
						}
					}
					break;
				}

				case "Collection":
				{
					lock(ifws)
					{
						try
						{
							iFolder ifolder = 
									ifws.GetiFolder(nargs.Collection);
							if(ifolder != null)
							{
								lock(EventQueue)
								{
									EventQueue.Enqueue(new SimiasEvent(
										ifolder, null, ifolder.ID, null,
										SimiasEventType.NewiFolder));
									SimiasEventFired.WakeupMain();
								}
							}
						}
						catch(Exception e)
						{
						}
					}
					break;
				}
			}
		}




		private void SimiasEventNodeChangedHandler(SimiasEventArgs args)
		{
			NodeEventArgs nargs = args as NodeEventArgs;

			switch(nargs.Type)
			{
				case "Collection":
				{
					lock(ifws)
					{
						try
						{
							iFolder ifolder = 
									ifws.GetiFolder(nargs.Collection);
							if( (ifolder != null) && (ifolder.HasConflicts) )
							{
								lock(EventQueue)
								{
									EventQueue.Enqueue(new SimiasEvent(
										ifolder, null, ifolder.ID, null,
										SimiasEventType.ChangediFolder));
									SimiasEventFired.WakeupMain();
								}
							}
						}
						catch(Exception e)
						{
						}
					}
					break;
				}

				case "Member":
				{
					lock(ifws)
					{
						try
						{
							iFolderUser newuser = ifws.GetiFolderUserFromNodeID(
								nargs.Collection, nargs.ID);

							if( (newuser != null) &&
								(newuser.UserID != 
										ifSettings.CurrentUserID) )
							{
								lock(EventQueue)
								{
									EventQueue.Enqueue(new SimiasEvent(
										null, newuser, nargs.Collection,
										newuser.UserID,
										SimiasEventType.ChangedUser));
									SimiasEventFired.WakeupMain();
								}
							}
						}
						catch(Exception e)
						{
						}
					}
					break;
				}


				case "Node":
				{
					lock(ifws)
					{
						// Check to see if the Node that changed is part of
						// the POBox
						if(nargs.Collection == ifSettings.DefaultPOBoxID)
						{
							try
							{
								iFolder ifolder = 
									ifws.GetiFolder(nargs.ID);

								if(ifolder != null)
								{
									lock(EventQueue)
									{
										EventQueue.Enqueue(new SimiasEvent(
											ifolder, null, ifolder.ID, null,
											SimiasEventType.ChangediFolder));
										SimiasEventFired.WakeupMain();
									}
								}
							}
							catch(Exception e)
							{
							}
						}
					}
					break;
				}					
			}
		}



		private void SimiasEventNodeDeletedHandler(SimiasEventArgs args)
		{
			NodeEventArgs nargs = args as NodeEventArgs;

			switch(nargs.Type)
			{
				case "Node":
				{
					lock(ifws)
					{
						if( (ifSettings != null) && 
							(nargs.Collection == ifSettings.DefaultPOBoxID) )
						{
							lock(EventQueue)
							{
								EventQueue.Enqueue(new SimiasEvent(
									null, null, nargs.ID, null,
									SimiasEventType.DeliFolder));
								SimiasEventFired.WakeupMain();
							}
						}
					}
					break;
				}
				case "Collection":
				{	
					lock(EventQueue)
					{
						EventQueue.Enqueue(new SimiasEvent(
							null, null, nargs.Collection, null,
							SimiasEventType.DeliFolder));
						SimiasEventFired.WakeupMain();
					}
					break;
				}
				case "Member":
				{
					lock(ifws)
					{

						try
						{
							iFolder ifolder = 
								ifws.GetiFolder(nargs.Collection);

							if(ifolder != null)
							{
								lock(EventQueue)
								{
									EventQueue.Enqueue(new SimiasEvent(
										null, null, nargs.Collection, nargs.ID,
										SimiasEventType.DelUser));
									SimiasEventFired.WakeupMain();
								}
							}
						}
						catch(Exception e)
						{
						}
					}
					break;
				}

			}
		}


		private void OnSimiasEventFired()
		{
			bool hasmore = false;
			// at this point, we are running in the same thread
			// so we can safely show events
			lock(EventQueue)
			{
				hasmore = (EventQueue.Count > 0);
			}

			while(hasmore)
			{
				SimiasEvent sEvent;

				lock(EventQueue)
				{
					sEvent = (SimiasEvent)EventQueue.Dequeue();
				}
				
				switch(sEvent.EventType)
				{
					case SimiasEventType.NewUser:
						if(iFolderUserAdded != null)
							iFolderUserAdded(this,
								new iFolderUserAddedEventArgs(
										sEvent.iFolderUser,
										sEvent.iFolderID));
						break;
					case SimiasEventType.NewiFolder:
						if(iFolderAdded != null)
							iFolderAdded(this,
								new iFolderAddedEventArgs(sEvent.iFolder));
						break;
					case SimiasEventType.ChangedUser:
						if(iFolderUserChanged != null)
							iFolderUserChanged(this,
								new iFolderUserChangedEventArgs(
										sEvent.iFolderUser,
										sEvent.iFolderID));
						break;
					case SimiasEventType.ChangediFolder:
						if(iFolderChanged != null)
							iFolderChanged(this,
								new iFolderChangedEventArgs(sEvent.iFolder));
						break;
					case SimiasEventType.DelUser:
						if(iFolderUserDeleted != null)
							iFolderUserDeleted(this,
								new iFolderUserDeletedEventArgs(
										sEvent.UserID,
										sEvent.iFolderID));
						break;
					case SimiasEventType.DeliFolder:
						if(iFolderDeleted != null)
							iFolderDeleted(this,
								new iFolderDeletedEventArgs(sEvent.iFolderID));
						break;
				}

				lock(EventQueue)
				{
					hasmore = (EventQueue.Count > 0);
				}
			}
		}
	}
}
