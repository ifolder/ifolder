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
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Authors:
 *		Calvin Gaisford <cgaisford@novell.com>
 *		Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/
	 
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using System.Threading;

using Simias.Client;
using Simias.Client.Event;
using Novell.iFolder.Events;

using Gtk;

namespace Novell.iFolder
{
	/// <summary>
	/// iFolder Data used in Linux Client
	/// </summary>
	public sealed class iFolderData
	{
		/// <summary>
		/// Member that ensures single instance
		/// </summary>
		private static iFolderData instance = null;


		/// <summary>
		/// Web Service to access the ifolder data
		/// </summary>
		private iFolderWebService ifws = null;


		/// <summary>
		/// Web Service to access the simias data
		/// </summary>
		private SimiasWebService simws = null;
		
		
		/// <summary>
		/// Handle to the SimiasEventBroker
		/// </summary>
		private SimiasEventBroker eventBroker;


		/// <summary>
		/// Hashtable to hold the domains
		/// </summary>
		private Hashtable		keyedDomains = null;

		/// <summary>
		/// Hashtable to hold the domains
		/// </summary>
		private DomainInformation	defDomain = null;
		
		private Hashtable		ifolderIters = null;
		private Hashtable		subToiFolderMap = null;
		
		/// <summary>
		/// TreeModel to hold all of the iFolders/Subscriptions.  All classes
		/// wanting to know about new/changed/deleted iFolders should add
		/// event handlers onto this TreeModel.
		/// </summary>
		private ListStore iFolderListStore;

		// These variables are used to keep track of how many
		// outstanding objects there are during a sync so that we don't
		// have to call CalculateSyncSize() over and over needlessly.
		private uint objectsToSync = 0;
		private bool startingSync  = false;

		public TreeModel iFolders
		{
			get
			{
				return iFolderListStore;
			}
		}


		/// <summary>
		/// Constructor.
		/// </summary>
		private iFolderData(Manager simiasManager)
		{
			try
			{
				ifws = new iFolderWebService();
				ifws.Url = 
					simiasManager.WebServiceUri.ToString() +
					"/iFolder.asmx";
				LocalService.Start(ifws, simiasManager.WebServiceUri, simiasManager.DataPath);
			}
			catch(Exception e)
			{
				ifws = null;
				throw new Exception("Unable to create ifolder web service");
			}
			try
			{
				simws = new SimiasWebService();
				simws.Url = 
					simiasManager.WebServiceUri.ToString() +
					"/Simias.asmx";
				LocalService.Start(simws, simiasManager.WebServiceUri, simiasManager.DataPath);
			}
			catch(Exception e)
			{
				simws = null;
				throw new Exception("Unable to create simias web service");
			}

			keyedDomains = new Hashtable();
			
			iFolderListStore = new ListStore(typeof(iFolderHolder));
			ifolderIters = new Hashtable();
			subToiFolderMap = new Hashtable();

			// Register with the SimiasEventBroker to get Simias Events
			eventBroker = SimiasEventBroker.GetSimiasEventBroker();
			if (eventBroker != null)
			{
//				eventBroker.iFolderAdded				+= OniFolderAddedEvent;
//				eventBroker.iFolderChanged				+= OniFolderChangedEvent;
				eventBroker.iFolderDeleted				+= OniFolderDeletedEvent;
				eventBroker.CollectionSyncEventFired 	+= OniFolderSyncEvent;
				eventBroker.FileSyncEventFired			+= OniFolderFileSyncEvent;
			}

			Refresh();
		}




		//===================================================================
		// GetData
		// Gets the current instance of iFolderData
		//===================================================================
		static public iFolderData GetData(Manager simiasManager)
		{
			lock (typeof(iFolderData))
			{
				if (instance == null)
				{
					instance = new iFolderData(simiasManager);
				}

				return instance;
			}
		}




		//===================================================================
		// Refresh
		// Reads the current ifolder and domains from Simias
		//===================================================================
		public void Refresh()
		{
Console.WriteLine("Refresh()");
			lock (typeof(iFolderData) )
			{
				// Refresh the current iFolders
				iFolderWeb[] ifolders;
				try
				{
					ifolders = ifws.GetAlliFolders();
				}
				catch(Exception e)
				{
					ifolders = null;
				}

				if(ifolders != null)
				{
					// Populate/update iFolderListStore
					foreach (iFolderWeb ifolder in ifolders)
					{
						// Check to see if an iFolder already exists and if so
						// update the current one.  Otherwise, add a new one to
						// the iFolderListStore.  We can't just replace or
						// recreate an iFolderHolder object because we lose
						// count of the number of objects that are left to
						// synchronize (Bug #82690).
						string ifolderID =
							ifolder.IsSubscription ?
								ifolder.CollectionID :
								ifolder.ID;
						if (ifolderIters.ContainsKey(ifolderID))
						{
							// We already have this iFolder in the ListStore so
							// just update the iFolderWeb object in the
							// iFolderHolder.
							TreeIter iter = (TreeIter)ifolderIters[ifolderID];
							iFolderHolder existingHolder = (iFolderHolder)
								iFolderListStore.GetValue(iter, 0);
							existingHolder.iFolder = ifolder;

							TreePath path = iFolderListStore.GetPath(iter);
							iFolderListStore.EmitRowChanged(path, iter);
						}
						else
						{
							// This is a new one we haven't seen before
							AddiFolder(ifolder);
						}
					}
				}

Console.WriteLine("\tRefresh: calling RefreshDomains");
				// Refresh the Domains
				RefreshDomains();
Console.WriteLine("\tRefresh: exiting");
			}
		}




		//===================================================================
		// RefreshDomains
		// Reads the current domains from Simias
		//===================================================================
		public void RefreshDomains()
		{
			lock (typeof(iFolderData) )
			{
				// Refresh the Domains
				keyedDomains.Clear();
				DomainInformation[] domains = null;
				try
				{
					domains = simws.GetDomains(false);
				}
				catch(Exception e)
				{
					domains = null;
				}

				if(domains != null)
				{
					foreach(DomainInformation domain in domains)
					{
						if(domain.IsDefault)
							defDomain = domain;

						AddDomain(domain);
					}
				}
			}
		}


		

		//===================================================================
		// AddDomain
		// adds the domain to the iFolderData internal tables
		//===================================================================
		public void AddDomain(DomainInformation newDomain)
		{
			lock (typeof(iFolderData) )
			{
				if(newDomain != null)
				{
					keyedDomains[newDomain.ID] = newDomain;
				}
			}
		}




		//===================================================================
		// AddiFolder
		// adds the ifolder to the iFolderData internal tables
		//===================================================================
		private iFolderHolder AddiFolder(iFolderWeb ifolder)
		{
Console.WriteLine("AddiFolder()");
			lock (typeof(iFolderData) )
			{
				iFolderHolder ifHolder = new iFolderHolder(ifolder);
				TreeIter iter = iFolderListStore.AppendValues(ifHolder);
				if (ifolder.IsSubscription)
				{
Console.WriteLine("\tSubscription");
					ifolderIters[ifolder.CollectionID] = iter;
					subToiFolderMap[ifolder.ID] = ifolder.CollectionID;
				}
				else
				{
Console.WriteLine("\tiFolder");
					ifolderIters[ifolder.ID] = iter;
				}
				
				return ifHolder;
			}
		}

		


		//===================================================================
		// DeliFolder
		// removes the ifolder from the iFolderData internal tables
		//===================================================================
		public void DeliFolder(string ifolderID)
		{
			lock (typeof(iFolderData) )
			{
				string realID = ifolderID;

				if(!IsiFolder(realID))
				{
					realID = GetiFolderID(ifolderID);
					if( (realID == null) || (!IsiFolder(realID)) )
						return;

					subToiFolderMap.Remove(ifolderID);
				}

				if (ifolderIters.ContainsKey(realID))
				{
					TreeIter iter = (TreeIter)ifolderIters[realID];
					iFolderListStore.Remove(ref iter);
					ifolderIters.Remove(realID);
				}
			}
		}

		


		//===================================================================
		// IsDomain
		// Checks to see if the ID passed is a Domain
		//===================================================================
		public bool IsDomain(string domainID)
		{
			lock(typeof(iFolderWeb))
			{
				return keyedDomains.ContainsKey(domainID);
			}
		}




		//===================================================================
		// IsiFolder
		// Checks to see if the ID passed is an iFolder
		//===================================================================
		public bool IsiFolder(string ifolderID)
		{
			lock(typeof(iFolderWeb))
			{
				return ifolderIters.ContainsKey(ifolderID);
			}
		}




		//===================================================================
		// ISPOBox
		// Checks to see if the ID passed is a pobox in a current domain
		//===================================================================
		public bool ISPOBox(string poBoxID)
		{
			lock(typeof(iFolderWeb))
			{
				ICollection icol = keyedDomains.Values;
				foreach(DomainInformation domain in icol)
				{
					if(domain.POBoxID.Equals(poBoxID))
						return true;
				}
				return false;
			}
		}




		//===================================================================
		// GetiFolderID
		// Gets the iFolder ID for a subscription that's been added
		//===================================================================
		public string GetiFolderID(string subscriptionID)
		{
			lock(typeof(iFolderWeb))
			{
				return (string)subToiFolderMap[subscriptionID];
			}
		}




		//===================================================================
		// GetiFolder
		// Gets the iFolderHolder from the iFolderData structures
		//===================================================================
		public iFolderHolder GetiFolder(string ifolderID)
		{
			lock(typeof(iFolderWeb))
			{
				iFolderHolder ifHolder = null;

				if (ifolderIters.ContainsKey(ifolderID))
				{
					TreeIter iter = (TreeIter)ifolderIters[ifolderID];
					ifHolder = (iFolderHolder)
						iFolderListStore.GetValue(iter, 0);
				}

				return ifHolder;
			}
		}




		//===================================================================
		// ReadiFolder
		// Reads and returns the iFolderHolder for the specificed ifolderID
		//===================================================================
		public iFolderHolder ReadiFolder(string ifolderID)
		{
			lock(typeof(iFolderWeb))
			{
				iFolderHolder ifHolder = null;

				if (ifolderIters.ContainsKey(ifolderID))
				{
					TreeIter iter = (TreeIter)ifolderIters[ifolderID];
					ifHolder = (iFolderHolder)
						iFolderListStore.GetValue(iter, 0);
				}
				
				// at this point, if we had the iFolder, we'll have the
				// original iFolderHolder to update
				try
				{
					iFolderWeb ifolder =
						ifws.GetiFolder(ifolderID);
					if (ifolder != null)
					{
						if (ifHolder != null)
						{
							ifHolder.iFolder = ifolder;

							if (ifolderIters.ContainsKey(ifolder.ID))
							{
								// Emit a TreeModel RowChanged Event
								TreeIter iter = (TreeIter)
									ifolderIters[ifolder.ID];
								TreePath path = iFolderListStore.GetPath(iter);
								iFolderListStore.EmitRowChanged(path, iter);
							}
						}
						else
						{
							ifHolder = AddiFolder(ifolder);
						}
					}
				}
				catch(Exception e)
				{
					ifHolder = null;
				}
				
				return ifHolder;
			}
		}




		//===================================================================
		// GetiFolders
		// Returns an array of the current iFolderHolders
		//===================================================================
		public iFolderHolder[] GetiFolders()
		{
			lock(typeof(iFolderWeb))
			{
				ArrayList arrayList = new ArrayList();
				TreeIter iter;
				
				if (iFolderListStore.GetIterFirst(out iter))
				{
					do
					{
						iFolderHolder ifHolder =
							(iFolderHolder)iFolderListStore.GetValue(iter, 0);
						if (ifHolder != null)
							arrayList.Add(ifHolder);
					} while (iFolderListStore.IterNext(ref iter));
				}

				iFolderHolder[] ifolderA = (iFolderHolder[])arrayList.ToArray(typeof(iFolderHolder));
//				foreach (iFolderHolder ifHolder in ifolderA)
//				{
//Console.WriteLine("\t{0}", ifHolder.iFolder.Name);
//				}

				return ifolderA;
			}
		}




		//===================================================================
		// GetAvailableiFolder
		// Returns the subscription for an iFolder or null if it isn't there
		//===================================================================
		public iFolderHolder GetAvailableiFolder(string ifolderID)
		{
			lock(typeof(iFolderWeb))
			{
				string realID;
				iFolderHolder ifHolder = null;

				realID = GetiFolderID(ifolderID);
				if(realID != null)
				{
					if (ifolderIters.ContainsKey(realID))
					{
						TreeIter iter = (TreeIter)ifolderIters[realID];
						ifHolder =
							(iFolderHolder)iFolderListStore.GetValue(iter, 0);
					}
				}

				return ifHolder;
			}
		}




		//===================================================================
		// ReadAvailableiFolder
		// Returns the subscription for an iFolder and reads the update version
		//===================================================================
		public iFolderHolder ReadAvailableiFolder(	string ifolderID,
													string collectionID)
		{
			lock(typeof(iFolderWeb))
			{
				iFolderHolder ifHolder = GetAvailableiFolder(ifolderID);

				try
				{
					iFolderWeb ifolder = ifws.GetiFolderInvitation(
								collectionID, ifolderID);

					if(ifHolder != null)
					{
						if(!IsiFolder(ifolder.CollectionID))
						{
							ifHolder.iFolder = ifolder;
							
							if (ifolderIters.ContainsKey(ifolder.CollectionID))
							{
								// Emit a TreeModel RowChanged Event
								TreeIter iter = (TreeIter)
									ifolderIters[ifolder.CollectionID];
								TreePath path = iFolderListStore.GetPath(iter);
								iFolderListStore.EmitRowChanged(path, iter);
							}
						}
					}
					else
					{
						if(!IsiFolder(ifolder.CollectionID))
							ifHolder = AddiFolder(ifolder);
					}
				}
				catch(Exception e)
				{
					ifHolder = null;
				}

				return ifHolder;
			}
		}




		//===================================================================
		// CreateiFolder
		// creates an iFolder in the domain at the path specified
		//===================================================================
		public iFolderHolder CreateiFolder(string path, string domainID)
		{
			lock(typeof(iFolderWeb))
			{
   				iFolderWeb newiFolder = 
								ifws.CreateiFolderInDomain(path, domainID);

				iFolderHolder ifHolder = AddiFolder(newiFolder);
				return ifHolder;
			}	
		}




		//===================================================================
		// AcceptiFolderInvitation
		// accepts an iFolder Invitation and updates the iFoler Information
		//===================================================================
		public iFolderHolder AcceptiFolderInvitation(	string ifolderID,
														string domainID,
														string localPath)
		{
			lock(typeof(iFolderWeb))
			{
				iFolderHolder ifHolder = null;
				string collectionID = GetiFolderID(ifolderID);

   		 		iFolderWeb newifolder = ifws.AcceptiFolderInvitation(
											domainID,
											ifolderID,
											localPath);
				if(newifolder.ID != ifolderID)
				{
					subToiFolderMap.Remove(ifolderID);
					if (newifolder.IsSubscription)
						subToiFolderMap[newifolder.ID]
							= newifolder.CollectionID;
				}

				ifHolder = GetiFolder(collectionID);
				ifHolder.iFolder = newifolder;

				if (ifolderIters.ContainsKey(newifolder.ID))
				{
					TreeIter iter =
						(TreeIter)ifolderIters[newifolder.ID];
					TreePath path = iFolderListStore.GetPath(iter);
					iFolderListStore.EmitRowChanged(path, iter);
				}

				return ifHolder;
			}
		}




		//===================================================================
		// RevertiFolder
		// reverts an iFolder to an invitation
		//===================================================================
		public iFolderHolder RevertiFolder(	string ifolderID)
		{
			lock(typeof(iFolderWeb))
			{
				iFolderHolder ifHolder = null;

				ifHolder = GetiFolder(ifolderID);
				if(ifHolder == null)
				{
					throw new Exception("iFolder did not exist");
				}

    			iFolderWeb reviFolder = 
								ifws.RevertiFolder(ifHolder.iFolder.ID);

				ifHolder.iFolder = reviFolder;
				if(reviFolder.IsSubscription)
				{
					subToiFolderMap[reviFolder.ID] = reviFolder.CollectionID;
					if (ifolderIters.ContainsKey(reviFolder.CollectionID))
					{
						TreeIter iter =
							(TreeIter)ifolderIters[reviFolder.CollectionID];
						TreePath path = iFolderListStore.GetPath(iter);
						iFolderListStore.EmitRowChanged(path, iter);
					}
				}

				return ifHolder;
			}
		}




		//===================================================================
		// DeleteiFolder
		// reverts and/or declines an iFolder invitation
		//===================================================================
		public void DeleteiFolder(string ifolderID)
		{
			lock(typeof(iFolderWeb))
			{
				iFolderHolder ifHolder = null;

				if(IsiFolder(ifolderID))
				{
					ifHolder = GetiFolder(ifolderID);
					if (ifHolder.iFolder.Role.Equals("Master"))
					{
						ifws.DeleteiFolder(ifHolder.iFolder.ID);
						DeliFolder(ifHolder.iFolder.ID);
						return;
					}

					ifHolder = RevertiFolder(ifolderID);
				}
				else
				{
					string realID = GetiFolderID(ifolderID);
					if(realID != null)
					{
						ifHolder = GetiFolder(realID);
					}
				}

				if(	(ifHolder != null) && 
								(ifHolder.iFolder.IsSubscription) )
				{
   		 			ifws.DeclineiFolderInvitation(
										ifHolder.iFolder.DomainID,
										ifHolder.iFolder.ID);
					
					DeliFolder(ifHolder.iFolder.ID);
				}
			}	
		}




		public iFolderUser GetiFolderUserFromNodeID(string collectionID, 
														string nodeID)
		{
			lock(typeof(iFolderWeb))
			{
				iFolderUser user = null;

				try
				{
					user = ifws.GetiFolderUserFromNodeID(
						collectionID, nodeID);
				}
				catch(Exception e)
				{
					user = null;	
				}
				return user;
			}
		}



		public bool IsCurrentUser(string UserID)
		{
			lock(typeof(iFolderWeb))
			{
				ICollection icol = keyedDomains.Values;
				foreach(DomainInformation domain in icol)
				{
					if(domain.MemberUserID.Equals(UserID))
						return true;
				}
				return false;
			}
		}



		public void RemoveDomain(string domainID)
		{
			lock (typeof(iFolderData) )
			{
				if(keyedDomains.ContainsKey(domainID))
				{
					DomainInformation dom = (DomainInformation)keyedDomains[domainID];
					keyedDomains.Remove(domainID);

					// If the domain we just removed was the default, ask
					// simias for the new default domain (if any domains still
					// exist).
					if (dom.IsDefault)
					{
						try
						{
							string newDefaultDomainID = simws.GetDefaultDomainID();
							if (newDefaultDomainID != null)
							{
								// Update the default domain
								if (keyedDomains.ContainsKey(newDefaultDomainID))
								{
									DomainInformation newDefaultDomain =
										(DomainInformation)keyedDomains[newDefaultDomainID];
									newDefaultDomain.IsDefault = true;
									defDomain = newDefaultDomain;
								}
							}
							else
								defDomain = null;
						}
						catch {}
					}
				}
			}
		}




		public DomainInformation[] GetDomains()
		{
			lock(typeof(iFolderWeb))
			{
				DomainInformation[] domains = new DomainInformation[keyedDomains.Count];

				ICollection icol = keyedDomains.Values;
				icol.CopyTo(domains, 0);

				return domains;
			}
		}



		public DomainInformation GetDefaultDomain()
		{
			lock(typeof(iFolderWeb))
			{
				return defDomain;
			}
		}



		public DomainInformation GetDomain(string domainID)
		{
			lock(typeof(iFolderWeb))
			{
				if (keyedDomains.Contains(domainID))
					return (DomainInformation)keyedDomains[domainID];
				else
					return null;
			}
		}



		public bool SetDefaultDomain(DomainInformation domain)
		{
			lock(typeof(iFolderWeb))
			{
				try
				{
					simws.SetDefaultDomain(domain.ID);
					if (defDomain != null && defDomain.ID != domain.ID)
						defDomain.IsDefault = false;

					domain.IsDefault = true;
					defDomain = domain;
				}
				catch (Exception ex)
				{
					return false;
				}
				return true;
			}
		}




		public DiskSpace GetUserDiskSpace(string UserID)
		{
			lock(typeof(iFolderWeb))
			{
				DiskSpace ds = null;
				try
				{
					ds = ifws.GetUserDiskSpace(UserID);
				}
				catch(Exception e)
				{
					// Let this one go
				}
				return ds;
			}
		}


		public int GetDomainCount()
		{
			lock(typeof(iFolderWeb))
			{
				return keyedDomains.Count;
			}
		}
		
		///
		/// Event Handlers
		///
		private void OniFolderDeletedEvent(object o, iFolderDeletedEventArgs args)
		{
			if (args == null || args.iFolderID == null)
				return;	// Prevent an exception

			DeliFolder(args.iFolderID);
		}

		private void OniFolderSyncEvent(object o, CollectionSyncEventArgs args)
		{
			if (args == null || args.ID == null || args.Name == null)
				return;	// Prevent an exception

			TreeIter iter = TreeIter.Zero;
			iFolderHolder ifHolder = null;

			if (ifolderIters.ContainsKey(args.ID))
			{
				iter = (TreeIter)ifolderIters[args.ID];
				ifHolder = (iFolderHolder)iFolderListStore.GetValue(iter, 0);
			}
			
			if (ifHolder == null) return;

			switch(args.Action)
			{
				case Simias.Client.Event.Action.StartLocalSync:
					ifHolder.State = iFolderState.SynchronizingLocal;
					break;
				case Simias.Client.Event.Action.StartSync:
					// Keep track of when a sync starts
					startingSync = true;

					ifHolder.State = iFolderState.Synchronizing;
					break;
				case Simias.Client.Event.Action.StopSync:
					try
					{
						SyncSize syncSize = ifws.CalculateSyncSize(args.ID);
						objectsToSync = syncSize.SyncNodeCount;
						ifHolder.ObjectsToSync = objectsToSync;
					}
					catch
					{}

					if (ifHolder.ObjectsToSync > 0)
						ifHolder.State = iFolderState.Normal;
					else
					{
						if (args.Connected)
							ifHolder.State = iFolderState.Normal;
						else
							ifHolder.State = iFolderState.Disconnected;
					}

					objectsToSync = 0;
					break;
				default:
					break;
			}
			
			// Emit a TreeModel RowChanged Event
			TreePath path = iFolderListStore.GetPath(iter);
			iFolderListStore.EmitRowChanged(path, iter);
		}

		private void OniFolderFileSyncEvent(object o, FileSyncEventArgs args)
		{
			if (args == null || args.CollectionID == null || args.Name == null)
				return;	// Prevent an exception

			if (args.SizeRemaining == args.SizeToSync)
			{
				if (startingSync || (objectsToSync <= 0))
				{
					startingSync = false;
					try
					{
						SyncSize syncSize = ifws.CalculateSyncSize(args.CollectionID);
						objectsToSync = syncSize.SyncNodeCount;
					}
					catch(Exception e)
					{
						objectsToSync = 1;
					}
				}

				if (!args.Direction.Equals(Simias.Client.Event.Direction.Local))
				{
					// Decrement the count whether we're showing the iFolder
					// in the current list or not.  We'll need this if the
					// user switches back to the list that contains the iFolder
					// that is actually synchronizing.
					if (objectsToSync <= 0)
						objectsToSync = 0;
					else
						objectsToSync--;
	

					// Get the iFolderHolder and set the objectsToSync
					TreeIter iter = TreeIter.Zero;
					iFolderHolder ifHolder = null;
					if (ifolderIters.ContainsKey(args.CollectionID))
					{
						iter = (TreeIter)ifolderIters[args.CollectionID];
						ifHolder = (iFolderHolder)iFolderListStore.GetValue(iter, 0);
					}
					
					if (ifHolder != null)
					{
						ifHolder.ObjectsToSync = objectsToSync;

						// Emit a TreeModel RowChanged Event
						TreePath path = iFolderListStore.GetPath(iter);
						iFolderListStore.EmitRowChanged(path, iter);
					}
				}
			}
		}
	}
}


