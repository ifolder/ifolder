// project created on 4/15/04 at 10:03 a
using System;
using System.Collections;
using System.Net;

namespace Mono.P2p.mDnsResponderApi
{
	public enum mDnsType : short
	{
		hostAddress = 1,
		ptr = 12,
		textStrings = 16,
		ipv6 = 28,
		hostInfo = 31,
		serviceLocation = 33,
		dumpYourGuts = 1189
	}
	
	public enum mDnsClass : short
	{
		iNet = 1,
		allResources = 255
		//iNetFlush = 0x8001
	}


	/// <summary>
	/// Summary description for Base Resource
	/// </summary>
	[Serializable]
	public class RBaseResource
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

		public RBaseResource(string name, int ttl, mDnsType dnsType, mDnsClass dnsClass, bool owner)
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
	[Serializable]
	public class RHostAddress : RBaseResource
	{
		#region Class Members
		protected ArrayList	ipAddresses = null;
		#endregion

		#region Properties

		// Returns the preferred address
		public IPAddress PrefAddress
		{
			get
			{
				return((IPAddress) this.ipAddresses[0]);
			}
			
			set
			{
				this.ipAddresses.Remove((IPAddress) value);
				this.ipAddresses.Insert(0, (IPAddress) value);
				this.Update();
			}
		}

		#endregion

		#region Constructors
		public RHostAddress(string name, int ttl, mDnsType dnsType, mDnsClass dnsClass, bool owner) : base(name, ttl, dnsType, dnsClass, owner)
		{
			this.ipAddresses = new ArrayList();
		}
		#endregion

		#region Private Methods
		#endregion

		#region Static Methods
		#endregion

		#region Public Methods
		public void	AddIPAddress(IPAddress ipAddress)
		{
			if (this.ipAddresses.Contains(ipAddress) == false)
			{
				this.ipAddresses.Add(ipAddress);
				this.Update();
			}
		}

		public ArrayList GetIPAddresses()
		{
			return(this.ipAddresses);
		}

		public void	RemoveIPAddress(IPAddress ipAddress)
		{
			this.ipAddresses.Remove(ipAddress);
		}
		#endregion
	}

	public interface IResourceRegistration
	{
		int	RegisterHost(string host, string ipaddress);
		int	DeregisterHost(string host);
		int	RegisterServiceLocation(string host, string serviceName, int port, int priority, int weight);
		int	DeregisterServiceLocation(string host, string serviceName);
		int	RegisterPointer(string domain, string target);
		int	DeregisterPointer(string domain, string target);
	}
	
	public interface IResourceQuery
	{
		int LookupResource(string name, ref short rtype);
		int	LookupServiceLocation(string serviceName, ref string host, ref int port, ref int priority, ref int weight);
		int	LookupPtr(string domain, out string target);
		int LookupHost(string host, out RHostAddress ha);	
	}
	
	public interface IRemoteFactory
	{
		IResourceRegistration	GetRegistrationInstance();
		IResourceQuery			GetQueryInstance();
	}
}