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
		private Hashtable		curiFolders = null;

		/// <summary>
		/// Hashtable to hold the domains
		/// </summary>
		private Hashtable		curDomains = null;


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
			}
			catch(Exception e)
			{
				simws = null;
				throw new Exception("Unable to create simias web service");
			}


			curiFolders = new Hashtable();
			curDomains = new Hashtable();
			RefreshData();
		}




		/// <summary>
		/// Gets the instance of the iFolderData object for this process.
		/// </summary>
		/// <returns>A reference to the iFolderData object.</returns>
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




		public void RefreshData()
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
					Hashtable newiFolders = new Hashtable();

					foreach(iFolderWeb ifolder in ifolders)
					{
						iFolderHolder ifHolder;

						if(curiFolders.ContainsKey(ifolder.ID))
						{
							ifHolder = (iFolderHolder)curiFolders[ifolder.ID];
							ifHolder.iFolder = ifolder;
						}
						else
							ifHolder = new iFolderHolder(ifolder);
						
						newiFolders.Add(ifolder.ID, ifHolder);
					}
					// set the new hash table to be the current one
					curiFolders.Clear();
					curiFolders = newiFolders;
				}


				// Refresh the Domains
				curDomains.Clear();
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
						curDomains.Add(domain.ID, domain);

						if(domain.IsDefault)
							defDomain = domain;
					}
				}
			}
		}




		public bool IsDomain(string domainID)
		{
			lock(typeof(iFolderWeb))
			{
				return curDomains.ContainsKey(domainID);
			}
		}




		public bool IsiFolder(string ifolderID)
		{
			lock(typeof(iFolderWeb))
			{
				return curiFolders.ContainsKey(ifolderID);
			}
		}




		public bool ISPOBox(string poBoxID)
		{
			lock(typeof(iFolderWeb))
			{
				ICollection icol = curDomains.Values;
				foreach(DomainInformation domain in icol)
				{
					if(domain.POBoxID.Equals(poBoxID))
						return true;
				}
				return false;
			}
		}




		public iFolderHolder GetiFolder(string ifolderID, bool updateData)
		{
			lock(typeof(iFolderWeb))
			{
				iFolderHolder ifHolder = null;

				if(curiFolders.ContainsKey(ifolderID))
				{
					ifHolder = (iFolderHolder)curiFolders[ifolderID];
					if(!updateData)
						return ifHolder;
				}

				// at this point if we had the iFolder, we'll have the
				// original iFolderHolder that we can update
				try
				{
					iFolderWeb ifolder = 
							ifws.GetiFolder(ifolderID);
					if(ifolder != null)
					{
						if(ifHolder == null)
							ifHolder = new iFolderHolder(ifolder);
						else
							ifHolder.iFolder = ifolder;

						curiFolders[ifolder.ID] = ifHolder;
					}
				}
				catch(Exception e)
				{
					ifHolder = null;
				}
			
				return ifHolder;
			}
		}




		public iFolderHolder[] GetiFolders()
		{
			lock(typeof(iFolderWeb))
			{
				iFolderHolder[] ifolders = 
					new iFolderHolder[curiFolders.Count];

				ICollection icol = curiFolders.Values;
				icol.CopyTo(ifolders, 0);

				return ifolders;
			}
		}




		public iFolderHolder GetAvailableiFolder(string collectionID,
													string ifolderID,
													bool updateData)
		{
			lock(typeof(iFolderWeb))
			{
				iFolderHolder ifHolder = null;

				if((curiFolders.ContainsKey(ifolderID)) && (!updateData) )
				{
					ifHolder = (iFolderHolder)curiFolders[ifolderID];
					return ifHolder;
				}

				try
				{
					iFolderWeb ifolder = ifws.GetiFolderInvitation(
								collectionID, ifolderID);

					if(ifolder != null)
					{
						ifHolder = new iFolderHolder(ifolder);

						curiFolders[ifolder.ID] = ifHolder;
					}
				}
				catch(Exception e)
				{
					ifHolder = null;
				}
				return ifHolder;
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
				ICollection icol = curDomains.Values;
				foreach(DomainInformation domain in icol)
				{
					if(domain.MemberUserID.Equals(UserID))
						return true;
				}
				return false;
			}
		}



		public void AddDomain(DomainInformation newDomain)
		{
			lock (typeof(iFolderData) )
			{
				if(newDomain != null)
				{
					curDomains.Add(newDomain.ID, newDomain);
					if(defDomain != null)
						defDomain.IsDefault = false;
					defDomain = newDomain;
				}
			}
		}




		public void RemoveDomain(string domainID)
		{
			lock (typeof(iFolderData) )
			{
				if(curDomains.ContainsKey(domainID))
					curDomains.Remove(domainID);
			}
		}




		public DomainInformation[] GetDomains()
		{
			lock(typeof(iFolderWeb))
			{
				DomainInformation[] domains = new DomainInformation[curDomains.Count];

				ICollection icol = curDomains.Values;
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


	}
}


