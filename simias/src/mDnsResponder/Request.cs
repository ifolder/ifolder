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
// This source file is responsible for taking multi-cast DNS requests
// off the wire, building a DNS request object and then submitting
// it to the request handler queue.
//


using System;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;


namespace Mono.P2p.mDnsResponder
{
	/// <summary>
	/// Summary description for DnsRequest
	/// </summary>
	class DnsRequest
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#region Class Members

		/*
		static private enum DnsFlags : ushort
		{
			recursion =		0x0100,
			response =		0x1000
		}
		*/

		static internal Queue		requestsQueue = new Queue();
		static internal Mutex		requestsMtx = new Mutex(false);

		static private	bool		receivingDnsRequests = false;
		static private	bool		stoppingDnsRequests = false;
		static private	IPEndPoint	multiep = new IPEndPoint(IPAddress.Parse("224.0.0.251"), 5353);
		static private	Thread		dnsReceiveThread = null;
		static private	Socket		dnsReceiveSocket = null;

		int			timeToLive;
		short		requestType;
		short		requestClass;
		string		domain;
		ArrayList	knownAnswers;
		#endregion

		#region Properties

		/// <summary>
		/// TTL - time to live in seconds
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

		/// <summary>
		/// Title -
		/// !NOTE! Doc incomplete
		/// </summary>
		public short Type
		{
			get
			{
				return(requestType);
			}

			set
			{
				requestType = value;
			}
		}

		/// <summary>
		/// Title -
		/// !NOTE! Doc incomplete
		/// </summary>
		public short Class
		{
			get
			{
				return(requestClass);
			}

			set
			{
				requestClass = value;
			}
		}

		/// <summary>
		/// Title -
		/// !NOTE! Doc incomplete
		/// </summary>
		public string Domain
		{
			get
			{
				return(domain);
			}

			set
			{
				domain = value;
			}
		}
		#endregion


		#region Constructors
		public DnsRequest()
		{
		}

		public DnsRequest(string domain, short rType, short rClass)
		{
			this.domain = domain;
			this.requestType = rType;
			this.requestClass = rClass;
		}
		#endregion


		#region Private Methods
		internal bool	FinalConstructor(short rType)
		{
			// Validate
			this.requestType = rType;
			return(true);
		}

		internal static int	StartDnsReceive()
		{
			BasicConfigurator.Configure();
			if (log.IsInfoEnabled) log.Info("StartDnsReceive called");

			IPEndPoint iep = new IPEndPoint(IPAddress.Any, 5353);
			EndPoint ep = (EndPoint) iep;

			try
			{
				dnsReceiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				dnsReceiveSocket.Bind(iep);
				dnsReceiveSocket.SetSocketOption(
					SocketOptionLevel.IP, 
					SocketOptionName.AddMembership,
					new MulticastOption(IPAddress.Parse("224.0.0.251")));
			}
			catch(Exception e)
			{
				if (log.IsDebugEnabled) log.Debug(e.Message);
				if (log.IsDebugEnabled) log.Debug(e.StackTrace);
				return(-1);
			}

			dnsReceiveThread = new Thread(new ThreadStart(DnsReceive));
			dnsReceiveThread.IsBackground = true;
			dnsReceiveThread.Start();
			if (log.IsInfoEnabled) log.Info("StartDnsReceive finished");
			return(0);
		}

		internal static int	StopDnsReceive()
		{
			if (log.IsInfoEnabled) log.Info("StopDnsReceive called");
			stoppingDnsRequests = true;
			dnsReceiveSocket.Close();
			Thread.Sleep(0);
			dnsReceiveThread.Abort();
			receivingDnsRequests = false;
			if (log.IsInfoEnabled) log.Info("StopDnsReceive finished");
			return(0);
		}

		private static void DnsReceive()
		{
			byte[]		receiveData = new byte[32768];
			EndPoint	ep = (EndPoint) multiep;
			int			bytesReceived;
			int			offset = 0;
			int			cOffset = 0;
			short		transactionID;
			UInt16		flags;
			short		dataLength;
			short		questions;
			short		answers;
			short		authorities;
			short		additional;
			short		rClass;
			short		rType;
			
			receivingDnsRequests = true;

			while(true)
			{
				bytesReceived = 0;
				try
				{
					bytesReceived = dnsReceiveSocket.ReceiveFrom(receiveData, ref ep);
				}
				catch(Exception e)
				{
					log.Debug("Failed calling ReceiveFrom", e);
				}

				// Outside forces telling us to stop?
				if (stoppingDnsRequests == true)
				{
					return;
				}

				if(bytesReceived != 0)
				{
					Console.WriteLine("");

					//
					// Parse the mDNS packet
					//

					//transactionID = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, 0));
					transactionID = BitConverter.ToInt16(receiveData, 0);
					flags = BitConverter.ToUInt16(receiveData, 2);

					questions = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, 4));
					answers = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, 6));
					authorities = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, 8));
					additional = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, 10));
					offset = 12;

					UInt16 queryResponse = 0x0080;
					if((flags & queryResponse) == queryResponse)
					{
						Console.WriteLine("Standard Response");
					}
					else
					{
						Console.WriteLine("Standard Query");
					}

					//Console.WriteLine("Response from : {0}.{1}.{2}.{3}", ep.
					Console.WriteLine("Transaction ID: {0}", transactionID);
					string tmpString = Convert.ToString(flags, 16);
					Console.WriteLine("Flags         : {0}", tmpString);
					Console.WriteLine("Questions     : {0}", questions);
					Console.WriteLine("Answers       : {0}", answers);
					Console.WriteLine("Authority     : {0}", authorities);
					Console.WriteLine("Additional    : {0}", additional);

					//
					// Print the questions
					//

					if (questions > 0 &&
						((flags & queryResponse) == queryResponse) == false)
					{
						string	domainName = "";

						while(questions-- > 0)
						{
							Console.WriteLine("");
							Console.WriteLine("   Question");
							Common.BuildDomainName(receiveData, offset, ref offset, ref domainName);
							Console.WriteLine("   Domain Name: " + domainName);

							// Move past the class and type
							rType = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, offset));
							rClass = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, offset + 2));

							Console.WriteLine("   Class: {0}  Type: {1}", rClass, rType);
							offset += 4;

							DnsRequest rRequest = new DnsRequest(domainName, rType, rClass);
							rRequest.Queue();
						}
					}

					// Now get the class and type

					if (answers > 0)
					{
						string	tmpDomain = "";
						ushort	recClass;
						while(answers-- > 0)
						{
							Console.WriteLine("");
							Console.WriteLine("   Answer");
							Common.BuildDomainName(receiveData, offset, ref offset, ref tmpDomain);
							Console.WriteLine("   Domain Name: " + tmpDomain);

							// Move past the class and type
							rType = BitConverter.ToInt16(receiveData, offset);
							rType = IPAddress.NetworkToHostOrder(rType);
							rClass = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, offset + 2));
							offset += 4;

							Console.WriteLine("   Type:        {0}", rType);
							Console.WriteLine("   Class:       {0}", rClass);

							int timeToLive = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(receiveData, offset));
							offset += 4;
							Console.WriteLine("   TTL:         {0}", timeToLive);

							// Get the datalength
							dataLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, offset));
							offset += 2;
							if (dataLength > 0)
							{
								Console.WriteLine("   DLEN:        {0}", dataLength);

								if (dataLength > 1300)
								{
									offset += dataLength;
									continue;
								}

								if(rType == mDnsTypes.hostType)
								{
									long ipAddress = 
										IPAddress.NetworkToHostOrder(BitConverter.ToUInt32(receiveData, offset));
									//int		ipAddress = BitConverter.ToInt32(receiveData, offset);

									Console.WriteLine(
										"   IP Address:  {0}.{1}.{2}.{3}",
										receiveData[offset],
										receiveData[offset + 1],
										receiveData[offset + 2],
										receiveData[offset + 3]);

									offset += 4;

									// Build a host record and add it to the list
									HostAddress cHostAddr = new HostAddress(tmpDomain, timeToLive, ipAddress, false);
									Resources.AddHostAddress(cHostAddr);
								}
								else
								if(rType == mDnsTypes.ipv6Type)
								{
									long ipAddress = 
										IPAddress.NetworkToHostOrder(BitConverter.ToUInt32(receiveData, offset));
									//int		ipAddress = BitConverter.ToInt32(receiveData, offset);

									Console.Write("   IPV6 Addr:   ");

									for( int i = 0; i < dataLength; i++)
									{
										Console.Write("{0,2:x}", receiveData[offset + i]);
									}

									Console.WriteLine("");
									offset += dataLength;
								}
								else
								if (rType == mDnsTypes.ptrType)
								{
									int		lOffset = offset;
									string	ptrDomain = "";
									Common.BuildDomainName(receiveData, lOffset, ref lOffset, ref ptrDomain);
									Console.WriteLine("   PTR Domain:  {0}", ptrDomain);

									offset += dataLength;
								}
								else
								if (rType == mDnsTypes.textStringsType)
								{
									//Console.WriteLine("Found TXT RR");

									int	totalStringsLength = 0;
									cOffset = offset;

									int sLength = receiveData[cOffset++];
									while(sLength > 0)
									{
										if (totalStringsLength + sLength > dataLength)
										{
											break;
										}

										Console.WriteLine("   TXT:         {0}", Encoding.UTF8.GetString(receiveData, cOffset, sLength));
										cOffset += sLength;
										totalStringsLength += sLength;

										sLength = receiveData[cOffset++];
									}

									Console.WriteLine("   ALEN:        {0}", totalStringsLength);
									offset += dataLength;
								}
								else
									if (rType == mDnsTypes.serviceLocationType)
								{
									int		lOffset = offset;
									short	priority;
									short	weight;
									short	port;
									string	target = "";

									priority = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, lOffset));
									weight = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, lOffset + 2));
									port = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, lOffset + 4));

									lOffset += 6;
									Common.BuildDomainName(receiveData, lOffset, ref lOffset, ref target);

									offset += dataLength;

									//Console.WriteLine("Found a SERVICE LOCATION RR");
									Console.WriteLine("   Priority:    {0}", priority);
									Console.WriteLine("   Weight:      {0}", weight);
									Console.WriteLine("   Port:        {0}", port);
									Console.WriteLine("   Target:      {0}", target);
								}
								else
								{
									if (dataLength <= 1300)
									{
										Console.WriteLine("   DN:          {0}", Encoding.ASCII.GetString(receiveData, offset, dataLength));
										offset += dataLength;
									}
									else
									{
										Console.WriteLine("Data Length too high!  length: {0]", dataLength);
										break;
									}
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#region Static Methods

		#endregion

		#region Public Methods
		public	void	Queue()
		{
			DnsRequest.requestsMtx.WaitOne();
			DnsRequest.requestsQueue.Enqueue(this);
			DnsRequest.requestsMtx.ReleaseMutex();
		}

		public void		Dequeue()
		{
		}
		#endregion
	}
}
