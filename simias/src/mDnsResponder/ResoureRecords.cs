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
		private class	MyHostAddress : HostAddress
		{
			ArrayList	serviceList = null;
			ArrayList	ptrList = null;

			public MyHostAddress(string hostName, int ttl, long ipAddress, bool owner) : base(hostName, ttl, ipAddress, owner)
			{
				serviceList = new ArrayList();
				ptrList = new ArrayList();
			}
		}


		static internal Mutex		resourceMtx = new Mutex(false);
		static	private ArrayList	hostList = new ArrayList();

		static public void AddHostAddress(HostAddress hostAddr)
		{
			Console.WriteLine("Resources::AddHostAddress called");
			bool	foundOne = false;
			Resources.resourceMtx.WaitOne();
			foreach(MyHostAddress myHost in Resources.hostList)
			{
				if (myHost.Name == hostAddr.Name)
				{
					foundOne = true;
					break;
				}
			}

			if (foundOne == false)
			{
				Console.WriteLine("   Adding " + hostAddr.Name);
				MyHostAddress cHost = new MyHostAddress(hostAddr.Name, hostAddr.Ttl, hostAddr.IPAddress, hostAddr.Owner);

				ArrayList ipList = hostAddr.GetIPAddresses();
				if (ipList.Count > 1)
				{
					foreach(long ipAddr in ipList)
					{
						cHost.AddIPAddress(ipAddr);
					}
				}

				Resources.hostList.Add(cHost);
			}
			
			Resources.resourceMtx.ReleaseMutex();
		}
	}

	/// <summary>
	/// Summary description for Host Resource
	/// </summary>
	class HostAddress
	{
		#region Class Members
		DateTime	update;
		protected bool		owner = false;
		protected string	name = null;
		protected int		ttl = 0;
		protected ArrayList	ipAddresses = null;
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
				string	textualIP = "";
				long	ip = (long) this.ipAddresses[0];

				textualIP += ((long)((ip & 0xFF000000) >> 24)).ToString();
				textualIP += ".";
				textualIP += ((long)((ip & 0x00FF0000) >> 16)).ToString();
				textualIP += ".";
				textualIP += ((long)((ip & 0x0000FF00) >> 8)).ToString();
				textualIP += ".";
				textualIP += ((long)(ip & 0x000000FF)).ToString();

				return(textualIP);
			}
		}

		#endregion

		#region Constructors

		/*
		public HostAddress()
		{
		}
		*/

		public HostAddress(string hostName, int ttl, long ipAddress, bool owner)
		{
			this.name = hostName;
			this.ttl = ttl;
			this.ipAddresses = new ArrayList();
			this.ipAddresses.Add(ipAddress);
			this.owner = owner;

			this.update = DateTime.Now;
		}
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
				this.update = DateTime.Now;
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
			this.update = DateTime.Now;
		}

		public void	RemoveIPAddress(long ipAddress)
		{
			this.ipAddresses.Remove(ipAddress);
			this.update = DateTime.Now;
		}

		#endregion
	}

	/// <summary>
	/// Summary description for Service Resource
	/// </summary>
	class ServiceResource
	{
		#region Class Members

		/*
		static internal ArrayList	hosts = new ArrayList();
		static internal Mutex		hostsMtx = new Mutex(false);
		*/

		ArrayList	nameValues;
		string		serviceName;
		int			timeToLive;
		int			port;
		int			priority;
		int			weight;
		#endregion

		#region Properties

		/// <summary>
		/// Local - local to this box
		/// !NOTE! Doc incomplete
		/// </summary>
		public int TTL
		{
			get
			{
				return(timeToLive);
			}

			set
			{
				this.timeToLive = value;
			}
		}

		public int	Port
		{
			get
			{
				return(port);
			}

			set
			{
				this.port = value;
			}
		}

		public int	Priority
		{
			get
			{
				return(priority);
			}

			set
			{
				this.priority = value;
			}
		}

		public int	Weight
		{
			get
			{
				return(weight);
			}

			set
			{
				this.weight = value;
			}
		}
		#endregion

		#region Constructors
		public ServiceResource()
		{
			this.nameValues = new ArrayList();
		}

		public ServiceResource(string service, int port, int priority, int weight)
		{
			this.serviceName = service;
			this.port = port;
			this.priority = priority;
			this.weight = weight;
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
}
