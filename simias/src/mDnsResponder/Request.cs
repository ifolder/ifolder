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
//using log4net.Appender;
using log4net.Config;
//using log4net.Layout;
//using log4net.Repository;
//using log4net.Repository.Hierarchy;

namespace Mono.P2p.mDnsResponder
{
	internal class Question
	{
		#region Class Members
		
		string		domain = "";
		mDnsClass	rClass;
		mDnsType	rType;

		#endregion

		#region Properties
		/// <summary>
		/// DomainName - Domain Name to query on
		/// !NOTE! Doc incomplete
		/// </summary>
		public string DomainName
		{
			get
			{
				return(this.domain);
			}

			set
			{
				this.domain = value;
			}
		}
		
		public mDnsClass RequestClass
		{
			get
			{
				return(this.rClass);
			}

			set
			{
				this.rClass = value;
			}
		}
		
		public mDnsType RequestType
		{
			get
			{
				return(this.rType);
			}

			set
			{
				this.rType = value;
			}
		}
	
		#endregion

		#region Constructors
		public Question()
		{
		}

		public Question(string domain, mDnsType rType, mDnsClass rClass)
		{
			this.domain = domain;
			this.rType = rType;
			this.rClass = rClass;
		}
		#endregion
	}

	/// <summary>
	/// Summary description for DnsRequest
	/// </summary>
	class DnsRequest
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#region Class Members


		static internal Queue		requestsQueue = new Queue();
		static internal Mutex		requestsMtx = new Mutex(false);

		static private	bool		receivingDnsRequests = false;
		static private	bool		stoppingDnsRequests = false;
		static private	IPEndPoint	multiep = new IPEndPoint(IPAddress.Parse("224.0.0.251"), 5353);
		static private	Thread		dnsReceiveThread = null;
		static private	Socket		dnsReceiveSocket = null;

		DnsFlags	flags;
		int			transactionID;
		int			timeToLive;
		short		questions;
		short		answers;
		short		authorities;
		short		additionalAnswers;
		short		requestType;
		short		requestClass;
		string		domain;
		string		sender;
		protected ArrayList	questionList;
		protected ArrayList	answerList;
		#endregion

		#region Properties

		/// <summary>
		/// Domain -
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
		
		public ArrayList QuestionList
		{
			get
			{
				return(this.questionList);
			}
		}
		
		public ArrayList AnswerList
		{
			get
			{
				return(this.answerList);
			}
		}
		
		public DnsFlags Flags
		{
			get
			{
				return(this.flags);
			}
		}
		
		public string Sender
		{
			get
			{
				return(this.sender);
			}
		}
		
		public int TransactionID
		{
			get
			{
				return(this.transactionID);
			}
		}
		
		#endregion


		#region Constructors
		public DnsRequest()
		{
			this.questionList = new ArrayList();
			this.answerList = new ArrayList();
		}

		public DnsRequest(string domain, short rType, short rClass)
		{
			this.questionList = new ArrayList();
			this.answerList = new ArrayList();
			this.domain = domain;
			this.requestType = rType;
			this.requestClass = rClass;
		}
		#endregion


		#region Private Methods
		internal static int	StartDnsReceive()
		{
			BasicConfigurator.Configure();
			log.Info("StartDnsReceive called");

			IPEndPoint iep = new IPEndPoint(IPAddress.Any, 5353);
			EndPoint ep = (EndPoint) iep;

			try
			{
				dnsReceiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				dnsReceiveSocket.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReuseAddress,
					true);
					
				dnsReceiveSocket.Bind(iep);
				dnsReceiveSocket.SetSocketOption(
					SocketOptionLevel.IP, 
					SocketOptionName.AddMembership,
					new MulticastOption(IPAddress.Parse("224.0.0.251")));
			}
			catch(Exception e)
			{
				log.Debug(e.Message);
				log.Debug(e.StackTrace);
				return(-1);
			}

			dnsReceiveThread = new Thread(new ThreadStart(DnsReceive));
			dnsReceiveThread.IsBackground = true;
			dnsReceiveThread.Start();
			log.Info("StartDnsReceive finished");
			return(0);
		}

		internal static int	StopDnsReceive()
		{
			log.Info("StopDnsReceive called");
			stoppingDnsRequests = true;
			dnsReceiveSocket.Close();
			Thread.Sleep(0);
			dnsReceiveThread.Abort();
			receivingDnsRequests = false;
			log.Info("StopDnsReceive finished");
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

					DnsRequest	dnsRequest = new DnsRequest();
					
					//
					// Parse the mDNS packet
					//

					dnsRequest.transactionID = BitConverter.ToInt16(receiveData, 0);
					flags = BitConverter.ToUInt16(receiveData, 2);
					
					//UInt16 queryResponse = 0x0080;
					if((flags & 0x0080) == 0x0080)
					{
						dnsRequest.flags |= DnsFlags.response;
						Console.WriteLine("Standard Response");
					}
					else
					{
						dnsRequest.flags |= DnsFlags.request;
						Console.WriteLine("Standard Query");
					}

					dnsRequest.questions = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, 4));
					dnsRequest.answers = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, 6));
					dnsRequest.authorities = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, 8));
					dnsRequest.additionalAnswers = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, 10));
					offset = 12;

					dnsRequest.sender = ep.ToString();
					
					Console.WriteLine("Sender        : {0}", dnsRequest.sender);
					Console.WriteLine("Transaction ID: {0}", dnsRequest.transactionID);
					string tmpString = Convert.ToString(flags, 16);
					Console.WriteLine("Flags         : {0}", tmpString);
					Console.WriteLine("Questions     : {0}", dnsRequest.questions);
					Console.WriteLine("Answers       : {0}", dnsRequest.answers);
					Console.WriteLine("Authority     : {0}", dnsRequest.authorities);
					Console.WriteLine("Additional    : {0}", dnsRequest.additionalAnswers);

					questions = dnsRequest.questions;
					if (questions > 0 &&
						((flags & 0x0080) == 0x0080) == false)
					{
						string	domainName = "";

						while(questions-- > 0)
						{
							Question question = new Question();
							
							Console.WriteLine("");
							Console.WriteLine("   Question");
							Common.BuildDomainName(receiveData, offset, ref offset, ref domainName);
							question.DomainName = domainName;
							Console.WriteLine("   Domain Name: " + domainName);

							// Move past the class and type
							question.RequestType = (mDnsType) IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, offset));
							question.RequestClass = (mDnsClass) IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, offset + 2));

							Console.WriteLine("   Class: {0}  Type: {1}", question.RequestClass, question.RequestType);
							offset += 4;

							dnsRequest.questionList.Add(question);
						}
					}
					
					// Now get the class and type

					if (dnsRequest.answers > 0)
					{
						string		tmpDomain = "";
						ushort		recClass;
						mDnsClass	rClass;
						mDnsType	rType;
						short answers = dnsRequest.answers;
						while(answers-- > 0)
						{
							Console.WriteLine("");
							Console.WriteLine("   Answer");
							Common.BuildDomainName(receiveData, offset, ref offset, ref tmpDomain);
							Console.WriteLine("   Domain Name: " + tmpDomain);

							// Move past the class and type
							rType = (mDnsType) BitConverter.ToInt16(receiveData, offset);
							rType = (mDnsType) IPAddress.NetworkToHostOrder((short) rType);
							rClass = (mDnsClass) IPAddress.NetworkToHostOrder(BitConverter.ToInt16(receiveData, offset + 2));
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

								if(rType == mDnsType.hostAddress)
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
									HostAddress cHostAddr = new HostAddress(tmpDomain, timeToLive, rType, rClass, false);
									Resources.AddHostAddress(cHostAddr);
								}
								else
								if(rType == mDnsType.ipv6)
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
								if (rType == mDnsType.ptr)
								{
									int		lOffset = offset;
									string	ptrDomain = "";
									Common.BuildDomainName(receiveData, lOffset, ref lOffset, ref ptrDomain);
									Console.WriteLine("   PTR Domain:  {0}", ptrDomain);

									offset += dataLength;
								}
								else
								if (rType == mDnsType.textStrings)
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
								if (rType == mDnsType.serviceLocation)
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
					
					dnsRequest.Queue();
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
