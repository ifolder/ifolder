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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
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
		/// Hashtable to hold the ifolders
		/// </summary>
		private Hashtable		keyediFolders = null;

		/// <summary>
		/// Hashtable to hold the domains
		/// </summary>
		private Hashtable		keyedDomains = null;

		/// <summary>
		/// Hashtable to hold the subscription to ifolder map
		/// </summary>
		private Hashtable		keyedSubscriptions = null;


		/// <summary>
		/// Hashtable to hold the domains
		/// </summary>
		private DomainInformation	defDomain = null;


		/// <summary>
		/// Constructor.
		/// </summary>
		private iFolderData()
		{
			try
			{
				ifws = new iFolderWebService();
				ifws.Url = 
					Simias.Client.Manager.LocalServiceUrl.ToString() +
					"/iFolder.asmx";
				LocalService.Start(ifws);
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
					Simias.Client.Manager.LocalServiceUrl.ToString() +
					"/Simias.asmx";
				LocalService.Start(simws);
			}
			catch(Exception e)
			{
				simws = null;
				throw new Exception("Unable to create simias web service");
			}


			keyediFolders = new Hashtable();
			keyedDomains = new Hashtable();
			keyedSubscriptions = new Hashtable();

			Refresh();
		}




		//===================================================================
		// GetData
		// Gets the current instance of iFolderData
		//===================================================================
		static public iFolderData GetData()
		{
			lock (typeof(iFolderData))
			{
				if (instance == null)
				{
					instance = new iFolderData();
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
					// clear out the map from subscription to iFolder
					keyediFolders.Clear();
					keyedSubscriptions.Clear();

					foreach(iFolderWeb ifolder in ifolders)
					{
						AddiFolder(ifolder);
					}
				}
				// Refresh the Domains
				RefreshDomains();
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
		private void AddDomain(DomainInformation newDomain)
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
			lock (typeof(iFolderData) )
			{
				iFolderHolder ifHolder = null;
				if(ifolder.IsSubscription)
				{
					ifHolder = new iFolderHolder(ifolder);
					keyediFolders[ifolder.CollectionID] = ifHolder;
					keyedSubscriptions[ifolder.ID] = ifolder.CollectionID;
				}
				else
				{
					ifHolder = new iFolderHolder(ifolder);
					keyediFolders[ifolder.ID] = ifHolder;
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

					keyedSubscriptions.Remove(ifolderID);
				}

				keyediFolders.Remove(realID);
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
				return keyediFolders.ContainsKey(ifolderID);
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
				return (string)keyedSubscriptions[subscriptionID];
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

				if(keyediFolders.ContainsKey(ifolderID))
				{
					ifHolder = (iFolderHolder)keyediFolders[ifolderID];
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

				if(keyediFolders.ContainsKey(ifolderID))
					ifHolder = (iFolderHolder)keyediFolders[ifolderID];

				// at this point if we had the iFolder, we'll have the
				// original iFolderHolder that we can update
				try
				{
					iFolderWeb ifolder = 
							ifws.GetiFolder(ifolderID);
					if(ifolder != null)
					{
						if(ifHolder != null)
							ifHolder.iFolder = ifolder;
						else
						{
							ifHolder = new iFolderHolder(ifolder);
							keyediFolders[ifolder.ID] = ifHolder;
//							keyediFolders.Add(ifolder.ID, ifHolder);
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
				iFolderHolder[] ifolders = 
					new iFolderHolder[keyediFolders.Count];

				ICollection icol = keyediFolders.Values;
				icol.CopyTo(ifolders, 0);

				return ifolders;
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
					if(keyediFolders.ContainsKey(realID))
					{
						ifHolder = (iFolderHolder)keyediFolders[realID];
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
							ifHolder.iFolder = ifolder;
					}
					else
					{
						if(!IsiFolder(ifolder.CollectionID))
						{
							ifHolder = new iFolderHolder(ifolder);

							keyediFolders[ifolder.CollectionID] = ifHolder;
//							keyediFolders.Add(ifolder.CollectionID, ifHolder);
							keyedSubscriptions.Add(ifolder.ID, 
														ifolder.CollectionID);
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
					keyedSubscriptions.Remove(ifolderID);
					if(newifolder.IsSubscription)
						keyedSubscriptions[newifolder.ID] = 
													newifolder.CollectionID;
				}

				ifHolder = GetiFolder(collectionID);
				ifHolder.iFolder = newifolder;
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
					keyedSubscriptions[reviFolder.ID] = reviFolder.CollectionID;
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
				iFolderHolder revertediFolder = null;

				if(IsiFolder(ifolderID))
				{
					revertediFolder = RevertiFolder(ifolderID);
				}
				else
				{
					string realID = GetiFolderID(ifolderID);
					if(realID != null)
					{
						revertediFolder = GetiFolder(realID);
					}
				}

				if(	(revertediFolder != null) && 
								(revertediFolder.iFolder.IsSubscription) )
				{
   		 			ifws.DeclineiFolderInvitation(
										revertediFolder.iFolder.DomainID,
										revertediFolder.iFolder.ID);
					DeliFolder(revertediFolder.iFolder.ID);
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
					keyedDomains.Remove(domainID);
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
					if(defDomain.ID != domain.ID)
					{
						defDomain.IsDefault = false;
						defDomain = domain;
					}
					domain.IsDefault = true;
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

	}
}


