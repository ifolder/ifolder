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
	class mDnsHost
	{
		#region Class Members

		static internal ArrayList	hosts = new ArrayList();
		static internal Mutex		hostsMtx = new Mutex(false);

		ArrayList	services;
		bool		local = false;
		string		hostName;
		int			timeToLive = Defaults.timeToLive;
		int			ipAddress;
		#endregion

		#region Properties

		/// <summary>
		/// Local - local to this box
		/// !NOTE! Doc incomplete
		/// </summary>
		public bool Local
		{
			get
			{
				return(local);
			}

			set
			{
				this.local = value;
			}
		}

		public int	IPV4Address
		{
			get
			{
				return(ipAddress);
			}

			set
			{
				this.ipAddress = value;
			}
		}

		public int	TTL
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

		public byte[] IPV6Address
		{
			get
			{
				return(null);
			}
		}
		#endregion

		#region Constructors
		public mDnsHost()
		{
			this.services = new ArrayList();
		}

		public mDnsHost(string host, int ipAddress, bool local)
		{
			this.hostName = host;
			this.ipAddress = ipAddress;
			this.local = local;
			this.services = new ArrayList();
		}
		#endregion

		#region Private Methods
		#endregion

		#region Static Methods

		static internal void Add(mDnsHost hostRecord)
		{
			mDnsHost.hostsMtx.WaitOne();
			mDnsHost.hosts.Add(hostRecord);
			mDnsHost.hostsMtx.ReleaseMutex();
		}

		static internal void Remove(mDnsHost hostRecord)
		{
			mDnsHost.hostsMtx.WaitOne();
			mDnsHost.hosts.Remove(hostRecord);
			mDnsHost.hostsMtx.ReleaseMutex();
		}

		static internal ArrayList GetHostList()
		{
			return(mDnsHost.hosts);
		}

		// Used to send out the gratutious answers
		internal static void MaintenanceThread()
		{
			byte[]		answer = new byte[4096];
			int			index;
			ushort		flags = 0x0084;
			string		localLabel = "local";
			string		presenceLabel = "_presence";
			string		tcpLabel = "_tcp";

			// Setup an endpoint to multi-cast datagrams
			UdpClient server = new UdpClient("224.0.0.251", 5353);

			while(true)
			{
//				Thread.Sleep(120000);
				Thread.Sleep(1000000);

				mDnsHost.hostsMtx.WaitOne();
				IEnumerator	enumHosts = mDnsHost.hosts.GetEnumerator();
				while(enumHosts.MoveNext())
				{
					mDnsHost host = (mDnsHost) enumHosts.Current;
					if (host.Local == false)
					{
						continue;
					}

					index = 0;

					// Send the host answer
					// Transaction ID
					Buffer.BlockCopy(
						BitConverter.GetBytes(0),
						0,
						answer,
						index,
						2);

					index += 2;

					// Flags
					Buffer.BlockCopy(
						BitConverter.GetBytes(flags), 
						0, 
						answer, 
						index, 
						2);
					index += 2;

					// Questions
					Buffer.BlockCopy(
						BitConverter.GetBytes(0),
						0,
						answer,
						index,
						2);

					index += 2;

					// Answers
					short answers = 1;
					answers = IPAddress.HostToNetworkOrder(answers);
					Buffer.BlockCopy(BitConverter.GetBytes(answers), 0,	answer, index, 2);
					index += 2;

					// Authorities
					Buffer.BlockCopy(BitConverter.GetBytes(0), 0, answer, index, 2);
					index += 2;

					// Additional
					Buffer.BlockCopy(BitConverter.GetBytes(0), 0, answer, index, 2);
					index += 2;

					// Host Name
					answer[index++] = (byte) host.hostName.Length;
					for(int i = 0; i < host.hostName.Length; i++)
					{
						answer[index++] = (byte) host.hostName[i];
					}

					answer[index++] = (byte) localLabel.Length;
					for(int i = 0; i < localLabel.Length; i++)
					{
						answer[index++] = (byte) localLabel[i];
					}

					answer[index++] = 0;

					// Type
					short	hostType = 1;
					hostType = IPAddress.HostToNetworkOrder(hostType);
					Buffer.BlockCopy(BitConverter.GetBytes(hostType), 0, answer, index, 2);
					index += 2;

					// Class
					//Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(1)), 0, answer, index, 2);
					short	hostClass = 1;
					hostClass = IPAddress.HostToNetworkOrder(hostClass);
					Buffer.BlockCopy(BitConverter.GetBytes(hostClass), 0, answer, index, 2);
					index += 2;

					// Time To Live
					Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(host.TTL)), 0, answer, index, 4);
					index += 4;

					// Data Length
					short dataLength = 4;
					dataLength = IPAddress.HostToNetworkOrder(dataLength);
					Buffer.BlockCopy(BitConverter.GetBytes(dataLength), 0, answer, index, 2);
					index += 2;

					// IP Address
					Buffer.BlockCopy(
//						BitConverter.GetBytes(IPAddress.HostToNetworkOrder(host.IPV4Address)),
						BitConverter.GetBytes(host.IPV4Address),
						0, 
						answer, 
						index, 
						4);

					index += 4;

					try
					{
						server.Send(answer, index);
					}
					catch(Exception e)
					{
						Console.WriteLine(e.Message);
						Console.WriteLine(e.StackTrace);
						continue;
					}

					//
					// Send a BOGUS presence service packet
					//


					index = 0;

					// Send the host answer
					// Transaction ID
					Buffer.BlockCopy(
						BitConverter.GetBytes(0),
						0,
						answer,
						index,
						2);

					index += 2;

					// Flags
					Buffer.BlockCopy(
						BitConverter.GetBytes(flags), 
						0, 
						answer, 
						index, 
						2);
					index += 2;

					// Questions
					Buffer.BlockCopy(
						BitConverter.GetBytes(0),
						0,
						answer,
						index,
						2);

					index += 2;

					// Answers
					short answers1 = 1;
					Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(answers1)), 0,	answer, index, 2);
					index += 2;

					// Authorities
					Buffer.BlockCopy(BitConverter.GetBytes(0), 0, answer, index, 2);
					index += 2;

					// Additional
					Buffer.BlockCopy(BitConverter.GetBytes(0), 0, answer, index, 2);
					index += 2;

					// Full Presence Name
					byte	nLength = (byte) Environment.UserName.Length;
					nLength += 1;
					nLength += (byte) host.hostName.Length;
					
					answer[index++] = nLength;
					for(int i = 0; i < Environment.UserName.Length; i++)
					{
						answer[index++] = (byte) Environment.UserName[i];
					}

					answer[index++] = (byte) '@';

					// Host Name
					for(int i = 0; i < host.hostName.Length; i++)
					{
						answer[index++] = (byte) host.hostName[i];
					}

					// Presence Label
					answer[index++] = (byte) presenceLabel.Length;
					for(int i = 0; i < presenceLabel.Length; i++)
					{
						answer[index++] = (byte) presenceLabel[i];
					}

					// TCP Label
					answer[index++] = (byte) tcpLabel.Length;
					for(int i = 0; i < tcpLabel.Length; i++)
					{
						answer[index++] = (byte) tcpLabel[i];
					}

					answer[index++] = (byte) localLabel.Length;
					for(int i = 0; i < localLabel.Length; i++)
					{
						answer[index++] = (byte) localLabel[i];
					}

					answer[index++] = 0;

					// Type
					short	hType = 33;
					Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(hType)), 0, answer, index, 2);
					index += 2;

					// Class
					//Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(1)), 0, answer, index, 2);
					hostClass = 1;
					Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(hostClass)), 0, answer, index, 2);
					index += 2;

					// Time To Live
					Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(host.TTL)), 0, answer, index, 4);
					index += 4;

					// Data Length
					dataLength = 8;
					dataLength += (short) host.hostName.Length;
					dataLength += (short) localLabel.Length;
					dataLength++;
					Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(dataLength)), 0, answer, index, 2);
					index += 2;

					// Priority - 2
					short	priority = 1;
					Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(priority)), 0, answer, index, 2);
					index += 2;

					// Weight - 2
					short	weight = 100;
					Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(weight)), 0, answer, index, 2);
					index += 2;

					// Port - 2
					short port = 80;
					Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(port)), 0, answer, index, 2);
					index += 2;

					// Host Name
					answer[index++] = (byte) host.hostName.Length;
					for(int i = 0; i < host.hostName.Length; i++)
					{
						answer[index++] = (byte) host.hostName[i];
					}

					answer[index++] = (byte) localLabel.Length;
					for(int i = 0; i < localLabel.Length; i++)
					{
						answer[index++] = (byte) localLabel[i];
					}

					answer[index++] = 0;

					server.Send(answer, index);
				}

				mDnsHost.hostsMtx.ReleaseMutex();
			}
		}

		#endregion

		#region Public Methods
		public string	GetTextualIPAddress()
		{
			return("192.168.1.101");
		}
		#endregion
	}
}
