// project created on 4/15/04 at 10:03 a
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;

namespace Mono.P2p.mDnsResponderApi
{
	[Serializable]
	public enum mDnsEvent
	{
		added,
		removed,
		updated
	}

	[Serializable]
	public enum mDnsType : short
	{
		hostAddress = 1,
		wellknownService = 11,
		ptr = 12,
		hostInfo = 13,
		textStrings = 16,
		ipv6 = 28,
		serviceLocation = 33,
		all = 255
		//dumpYourGuts = 1189
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
		protected string	id = null;
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

		public string ID
		{
			get
			{
				return(this.id);
			}

			set
			{
				this.id = value;
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

	/// <summary>
	/// Summary description for Service Resource
	/// </summary>
	[Serializable]
	public class RServiceLocation : RBaseResource
	{
		#region Class Members
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
		public RServiceLocation(string name, int ttl, mDnsType dnsType, mDnsClass dnsClass, bool owner) : base(name, ttl, dnsType, dnsClass, owner)
		{
			this.priority = 0;
		}
		
		#endregion

		#region Private Methods
		#endregion

		#region Static Methods
		#endregion

		#region Public Methods
		#endregion
	}

	[Serializable]
	public class RPtr : RBaseResource
	{
		#region Class Members
		string		target;
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
		public RPtr(string name, int ttl, mDnsType dnsType, mDnsClass dnsClass, bool owner) : base(name, ttl, dnsType, dnsClass, owner)
		{
			this.target = "";			
		}
		#endregion
	}

	/// <summary>
	/// Summary description for TextString
	/// </summary>
	[Serializable]
	public class RTextStrings : RBaseResource
	{
		#region Class Members
		ArrayList	stringList = null;
		#endregion

		#region Properties
		#endregion

		#region Constructors
		public RTextStrings(string name, int ttl, mDnsType dnsType, mDnsClass dnsClass, bool owner) : base(name, ttl, dnsType, dnsClass, owner)
		{
			this.stringList = new ArrayList();
		}
		#endregion

		#region Public Methods
		public bool	AddTextString(string txtString)
		{
			this.stringList.Add(txtString);
			return(true);
		}

		public bool RemoveTextString(string txtString)
		{
			this.stringList.Remove(txtString);
			return(true);
		}
		
		public bool RemoveAllTextStrings()
		{
			this.stringList.Clear();
			return(true);
		}

		public ArrayList GetTextStrings()
		{
			return(this.stringList);
		}
		#endregion
	}

	public interface IResourceRegistration
	{
		int	RegisterHost(string host, string ipaddress);
		int	DeregisterHost(string host);
		int	RegisterServiceLocation(string host, string serviceName, int port, int priority, int weight);
		int	DeregisterServiceLocation(string host, string serviceName);
		int	RegisterPointer(string ptr, string target);
		int	DeregisterPointer(string domain, string target);
		int	RegisterTextStrings(string serviceName, string[] textStrings);
		int	DeregisterTextStrings(string serviceName);
	}
	
	public interface IResourceQuery
	{
		int GetDefaultHost(ref RHostAddress ha);
		int GetResourceRecords(mDnsResponderApi.mDnsType rType, out string[] IDs);
		int	GetHostAddressResources(out RHostAddress[] has);
		int GetServiceLocationResources(out RServiceLocation[] sls);
		int	GetPtrResources(out RPtr[] ptrs);
		int	GetPtrResourcesByName(string sourceName, out RPtr[] ptrs);
		int	GetTextStringResources(out RTextStrings[] tss);
		int	GetHostById(string id, ref RHostAddress ha);
		int	GetHostByName(string hostName, ref RHostAddress ha);
		int	GetServiceById(string id, ref RServiceLocation sl);
		int GetServiceByName(string serviceName, ref RServiceLocation sl);
		int	GetPtrById(string id, ref RPtr ptr);
		int GetTextStringsById(string id, ref RTextStrings ts);
	}

	public interface IMDnsLog
	{
		int	DumpResourceRecords();
		int	DumpLocalRecords();
		int	DumpStatistics();
	}
    
	public interface IRemoteFactory
	{
		IResourceRegistration	GetRegistrationInstance();
		IResourceQuery			GetQueryInstance();
		IMDnsLog				GetLogInstance();
	}

//	public delegate void mDnsEventHandler(mDnsEvent mEvent, mDnsType mType, string resourceID);
	public delegate void mDnsEventHandler(string resourceID);

	public interface IMDnsEvent
	{
		event	mDnsEventHandler OnEvent;
		void	Publish(string resourceID);
	}

	public class mDnsEventWrapper: MarshalByRefObject
	{
		public event mDnsEventHandler OnLocalEvent;

		public void LocalOnEvent(string resourceID)
		{
			OnLocalEvent(resourceID);
		}

		public override object InitializeLifetimeService()
		{
			return(null);
		}
	}
}