using System;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mono.P2p.mDnsResponder
{
	/// <summary>
	/// Summary description for mDnsHost
	/// </summary>
	class mDnsService
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
		public mDnsService()
		{
			this.nameValues = new ArrayList();
		}

		public mDnsService(string service, int port, int priority, int weight)
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
