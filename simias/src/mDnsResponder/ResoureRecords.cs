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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

//
// This source file contains classes for the different DNS Resource
// Records used by the multi-cast DNS responder.
//

using System;
using System.Collections;
using System.Threading;
using System.Text;

namespace Mono.P2p.mDnsResponder
{
	internal class	Resources
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		static internal Mutex		resourceMtx = new Mutex(false);
		static	private ArrayList	resourceList = new ArrayList();
		static	private	ArrayList	serviceList = new ArrayList();

		static public void DumpYourGuts(Question cQuestion)
		{
			log.Info("DumpYourGuts called");
			
			Resources.resourceMtx.WaitOne();
			
			foreach(BaseResource cResource in Resources.resourceList)
			{
				log.Info("");
				log.Info("  RESOURCE");
				log.Info("  Name:      " + cResource.Name);
				log.Info("  Type:      " + cResource.Type);
				log.Info("  TTL:       " + cResource.Ttl.ToString());
				log.Info("  Owner:     " + cResource.Owner.ToString());
				
				if (cResource.Type == mDnsType.hostAddress)
				{
					HostAddress	hostAddr = (HostAddress) cResource;
					
					try
					{
						if (hostAddr.TextualIPAddress != "")
						{
							log.Info("  IP:        " + hostAddr.TextualIPAddress);
						}
						
						log.Info("  IPADDR:      " + hostAddr.IPAddress.ToString());
					}
					catch{}
				}
				else
				if (cResource.Type == mDnsType.serviceLocation)
				{
					ServiceLocation svcLoc = (ServiceLocation) cResource;
					
					log.Info("  Priority:  " + svcLoc.Priority.ToString());
					log.Info("  Weight:    " + svcLoc.Weight.ToString());
					log.Info("  Port:      " + svcLoc.Port.ToString());
					log.Info("  Target:    " + svcLoc.Target);
				}
				else
				if (cResource.Type == mDnsType.ptr)
				{
					Ptr ptr = (Ptr) cResource;
					log.Info("  Target:    " + ptr.Target);
				}
				log.Info("");
			}
			Resources.resourceMtx.ReleaseMutex();
			log.Info("DumpYourGuts exit");
		}
		
		static public void AddHostAddress(HostAddress hostAddr)
		{
			log.Info("AddHostAddress called");
			bool	foundOne = false;
			
			Resources.resourceMtx.WaitOne();
			foreach(BaseResource cResource in Resources.resourceList)
			{
				if (cResource.Name == hostAddr.Name &&
					cResource.Type == hostAddr.Type)
				{
					foundOne = true;
					break;
				}
			}

			if (foundOne == false)
			{
				log.Info("   Adding " + hostAddr.Name);
				Resources.resourceList.Add(hostAddr);
			}
			
			Resources.resourceMtx.ReleaseMutex();
		}

		static public void AddServiceLocation(ServiceLocation serviceLocation)
		{
			log.Info("AddServiceLocation called");
			bool	foundOne = false;
			
			Resources.resourceMtx.WaitOne();
			foreach(BaseResource cResource in Resources.resourceList)
			{
				if (cResource.Name == serviceLocation.Name &&
					cResource.Type == serviceLocation.Type)
				{
					foundOne = true;
					
					// Update the record
					ServiceLocation cLocation = (ServiceLocation) cResource;
					cLocation.Port = serviceLocation.Port;
					cLocation.Weight = serviceLocation.Weight;
					cLocation.Priority = serviceLocation.Priority;
					cLocation.Target = serviceLocation.Target;
					break;
				}
			}

			if (foundOne == false)
			{
				log.Info("   Adding " + serviceLocation.Name);
				Resources.resourceList.Add(serviceLocation);
			}
			
			Resources.resourceMtx.ReleaseMutex();
		}

		static public void AddPtr(Ptr ptr)
		{
			log.Info("AddPtr called");
			bool	foundOne = false;
			
			Resources.resourceMtx.WaitOne();
			foreach(BaseResource cResource in Resources.resourceList)
			{
				if (cResource.Name == ptr.Name &&
					cResource.Type == ptr.Type)
				{
					Ptr cPtr = (Ptr) cResource;
					if(cPtr.Target == ptr.Target)
					{
						foundOne = true;
						if (ptr.Ttl == 0)
						{
							log.Info("   Removing " + ptr.Name);
							Resources.resourceList.Remove(cPtr);	
						}
							
						break;
					}
				}
			}

			if (foundOne == false)
			{
				if (ptr.Ttl != 0)
				{
					log.Info("   Adding " + ptr.Name);
					Resources.resourceList.Add(ptr);
				}
			}
			
			Resources.resourceMtx.ReleaseMutex();
		}
	}

	/// <summary>
	/// Summary description for Base Resource
	/// </summary>
	class BaseResource
	{
		#region Class Members
		DateTime			update;
		protected bool		owner = false;
		protected string	name = null;
		protected int		ttl = 0;
		protected mDnsType	dnsType;
		protected mDnsClass dnsClass;
		#endregion

		#region Properties
		public string Name
		{
			get
			{
				try
				{
					return(this.name);
				}
				catch{}
				return("");
			}
		}

		// True if this record's Time-To-Live
		// has expired
		public bool Expired
		{
			get
			{
				return(false);
			}
		}

		// Are we authoritative for this record?
		public bool Owner
		{
			get
			{
				return(owner);
			}
		}

		// Returns the current ttl
		public int Ttl
		{
			get
			{
				return(this.ttl);
			}
		}
		
		public mDnsType Type
		{
			get
			{
				return(this.dnsType);
			}
		}
		
		public mDnsClass Class
		{
			get
			{
				return(this.dnsClass);
			}
		}

		#endregion

		#region Constructors

		public BaseResource(string name, int ttl, mDnsType dnsType, mDnsClass dnsClass, bool owner)
		{
			this.name = name;
			this.ttl = ttl;
			this.dnsType = dnsType;
			this.dnsClass = dnsClass;
			this.owner = owner;

			this.update = DateTime.Now;
		}
		#endregion

		#region Private Methods
		internal void	Update()
		{
			update = DateTime.Now;
		}
		#endregion

		#region Static Methods
		#endregion

		#region Public Methods
		#endregion
	}

	/// <summary>
	/// Summary description for Host Resource
	/// </summary>
	class HostAddress : BaseResource
	{
		#region Class Members
		protected ArrayList	ipAddresses = null;
		#endregion

		#region Properties

		// Returns the preferred address
		public long IPAddress
		{
			get
			{
				return((long) this.ipAddresses[0]);
			}
		}

		public string TextualIPAddress
		{
			get
			{
			
				return("192.168.1.102");
				/*
				string	textualIP = "";
				
				if (this.ipAddresses.Count >= 1)
				{
					long	ip = (long) this.ipAddresses[0];

					textualIP += ((long)((ip & 0xFF000000) >> 24)).ToString();
					textualIP += ".";
					textualIP += ((long)((ip & 0x00FF0000) >> 16)).ToString();
					textualIP += ".";
					textualIP += ((long)((ip & 0x0000FF00) >> 8)).ToString();
					textualIP += ".";
					textualIP += ((long)(ip & 0x000000FF)).ToString();
				}
				return(textualIP);
				*/
			}
		}

		#endregion

		#region Constructors

		/*
		public HostAddress()
		{
		}
		*/

		public HostAddress(string name, int ttl, mDnsType dnsType, mDnsClass dnsClass, bool owner) : base(name, ttl, dnsType, dnsClass, owner)
		{
			this.ipAddresses = new ArrayList();
		}

		/*
		public HostAddress(string hostName, int ttl, long ipAddress, bool owner)
		{
			this.name = hostName;
			this.ttl = ttl;
			this.ipAddresses = new ArrayList();
			this.ipAddresses.Add(ipAddress);
			this.owner = owner;

			this.update = DateTime.Now;
		}
		*/
		#endregion

		#region Private Methods
		#endregion

		#region Static Methods
		#endregion

		#region Public Methods
		public void	AddIPAddress(long ipAddress)
		{
			if (this.ipAddresses.Contains(ipAddress) == false)
			{
				this.ipAddresses.Add(ipAddress);
				this.Update();
				//this.update = DateTime.Now;
			}
		}

		public ArrayList GetIPAddresses()
		{
			ArrayList returnedList = new ArrayList(this.ipAddresses.Count + 1);
			foreach(long addr in this.ipAddresses)
			{
				returnedList.Add(addr);
			}

			return(returnedList);
		}

		public void PreferredIPAddress(long ipAddress)
		{
			this.ipAddresses.Remove(ipAddress);
			this.ipAddresses.Insert(0, ipAddress);
			this.Update();
			//this.update = DateTime.Now;
		}

		public void	RemoveIPAddress(long ipAddress)
		{
			this.ipAddresses.Remove(ipAddress);
			//this.update = DateTime.Now;
		}

		#endregion
	}

	/// <summary>
	/// Summary description for Service Resource
	/// </summary>
	class ServiceLocation : BaseResource
	{
		#region Class Members

		/*
		static internal ArrayList	hosts = new ArrayList();
		static internal Mutex		hostsMtx = new Mutex(false);
		*/

		ArrayList	nameValues;
		int			port;
		int			priority;
		int			weight;
		string		target = "";
		#endregion

		#region Properties

		public int	Port
		{
			get
			{
				return(this.port);
			}

			set
			{
				this.port = value;
				this.Update();
			}
		}

		public int	Priority
		{
			get
			{
				return(this.priority);
			}

			set
			{
				this.priority = value;
				this.Update();
			}
		}

		public int	Weight
		{
			get
			{
				return(this.weight);
			}

			set
			{
				this.weight = value;
				this.Update();
			}
		}
		
		public string	Target
		{
			get
			{
				return(this.target);
			}
			
			set
			{
				this.target = value;
				this.Update();
			}
		}
		#endregion

		#region Constructors
		/*
		public ServiceLocation() : base()
		{
			this.nameValues = new ArrayList();
		}
		*/

/*
		public ServiceResource(string service, int port, int priority, int weight)
		{
			this.serviceName = service;
			this.port = port;
			this.priority = priority;
			this.weight = weight;
			this.nameValues = new ArrayList();
		}
*/
		
		public ServiceLocation(string name, int ttl, mDnsType dnsType, mDnsClass dnsClass, bool owner) : base(name, ttl, dnsType, dnsClass, owner)
		{
			this.nameValues = new ArrayList();
		}
		
		#endregion

		#region Private Methods
		#endregion

		#region Static Methods
		#endregion

		#region Public Methods

		public bool	AddNameValue(string name, string nameValue)
		{
			string	full = name;

			if (nameValue != null)
			{
				full += "=" + nameValue;
			}

			this.nameValues.Add(full);
			return(true);
		}

		public bool RemoveNameValue(string name)
		{
			bool	removed = false;
			string	tmpString;

			IEnumerator		enumNames = this.nameValues.GetEnumerator();
			while(enumNames.MoveNext())
			{
				tmpString = (string) enumNames.Current;
				string[] nameS = tmpString.Split('=');
				if(nameS[0] == name)
				{
					this.nameValues.Remove(tmpString);
					removed = true;
					break;
				}
			}

			return(removed);
		}

		public ArrayList GetNameValues()
		{
			return(this.nameValues);
		}
		#endregion
	}

	/// <summary>
	/// Summary description for Ptr
	/// </summary>
	class Ptr : BaseResource
	{
		#region Class Members

		/*
		static internal ArrayList	hosts = new ArrayList();
		static internal Mutex		hostsMtx = new Mutex(false);
		*/

		string		target = "";
		#endregion

		#region Properties

		public string	Target
		{
			get
			{
				return(this.target);
			}
			
			set
			{
				this.target = value;
				this.Update();
			}
		}
		#endregion

		#region Constructors

/*
		public ServiceResource(string service, int port, int priority, int weight)
		{
			this.serviceName = service;
			this.port = port;
			this.priority = priority;
			this.weight = weight;
			this.nameValues = new ArrayList();
		}
*/
		
		public Ptr(string name, int ttl, mDnsType dnsType, mDnsClass dnsClass, bool owner) : base(name, ttl, dnsType, dnsClass, owner)
		{
			
		}
		
		#endregion

		#region Private Methods
		#endregion

		#region Static Methods
		#endregion

		#region Public Methods

		#endregion
	}
}
