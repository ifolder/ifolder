using System;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using log4net;
using log4net.Config;
using log4net.Appender;
using log4net.Repository;
using log4net.spi;
using log4net.Layout;
using Mono.P2p.mDnsResponderApi;


namespace Mono.P2p.mDnsResponder
{
	/// <summary>
	/// Summary description for RequestHandler
	/// </summary>
	class RequestHandler
	{
		private static readonly log4net.ILog log = 
			log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		static bool	mDnsStopping = false;
		static Thread reqHandlerThread = null;
		static AutoResetEvent reqHandlerEvent = null;
		
		static internal Queue requestsQueue = new Queue();
		static internal Mutex requestsMtx = new Mutex(false);
		
		internal static void QueueRequest(DnsRequest dnsRequest)
		{
			requestsMtx.WaitOne();
			requestsQueue.Enqueue(dnsRequest);
			requestsMtx.ReleaseMutex();
			reqHandlerEvent.Set();
		}
		
		internal static int	StartRequestHandler()
		{
			log.Info("StartRequestHandler called");

			reqHandlerEvent = new AutoResetEvent(false);
			reqHandlerThread = new Thread(new ThreadStart(RequestHandler.RequestHandlerThread));
			reqHandlerThread.IsBackground = true;
			reqHandlerThread.Start();
			
			log.Info("StartRequestHandler finished");
			return(0);
		}

		internal static int	StopRequestHandler()
		{
			log.Info("StopRequestHandler called");
			
			try
			{
				mDnsStopping = true;
				reqHandlerEvent.Set();
				Thread.Sleep(0);
				reqHandlerThread.Abort();
				Thread.Sleep(0);
				reqHandlerEvent.Close();
			}
			catch{}
			
			log.Info("StopRequestHandler finished");
			return(0);
		}
		
		//
		// TODO need to add any answer resources that come in with a question to
		// the internal resource lists
		//
		internal static void RequestHandlerThread()
		{
			DnsRequest		dnsRequest;

			// Setup an endpoint to multi-cast datagrams
			UdpClient server = new UdpClient(Defaults.multiCastAddress, Defaults.mDnsPort);

			while(reqHandlerEvent.WaitOne(120000, false))
			{
				if (mDnsStopping == true)
				{
					return;
				}

				dnsRequest = null;
				requestsMtx.WaitOne();
				if (requestsQueue.Count > 0)
				{
					try
					{
						dnsRequest = (DnsRequest) requestsQueue.Dequeue();
					}
					catch{}

					requestsMtx.ReleaseMutex();

					if (dnsRequest != null)
					{
						if (log.IsInfoEnabled == true)
						{
							RequestHandler.LogRequest(dnsRequest);
						}

						if ((dnsRequest.Flags & DnsFlags.request) == DnsFlags.request)
						{
							foreach(Question cQuestion in dnsRequest.QuestionList)
							{
								/*
								if (cQuestion.RequestType == mDnsType.dumpYourGuts)
								{
									Resources.DumpYourGuts(cQuestion);
								}
								else
								*/
								if (cQuestion.RequestType == mDnsType.hostAddress)
								{
									mDnsResponse cResponse = null;
									Resources.resourceMtx.WaitOne();
									foreach(BaseResource cResource in Resources.resourceList)
									{
										if (cResource.Owner == true && 
											cResource.Type == mDnsType.hostAddress &&
											cResource.Name == cQuestion.DomainName)
										{
											if (cResponse == null)
											{
												cResponse = new mDnsResponse(server);
											}
											cResponse.AddAnswer(cResource);
										}	
									}
									Resources.resourceMtx.ReleaseMutex();
									
									if (cResponse != null)
									{
										log.Info("Sending a response to a direct HA Query");
										cResponse.Send();
									}
								}
								else
								if (cQuestion.RequestType == mDnsType.serviceLocation)
								{
									mDnsResponse cResponse = null;
									Resources.resourceMtx.WaitOne();
									foreach(BaseResource cResource in Resources.resourceList)
									{
										if (cResource.Owner == true && 
											cResource.Type == mDnsType.serviceLocation &&
											cResource.Name == cQuestion.DomainName)
										{
											if (cResponse == null)
											{
												cResponse = new mDnsResponse(server);
											}
											cResponse.AddAnswer(cResource);
										}	
									}
									Resources.resourceMtx.ReleaseMutex();
									
									if (cResponse != null)
									{
										log.Info("Sending a response to a direct SL Query");
										cResponse.Send();
									}
								}
								else
								if (cQuestion.RequestType == mDnsType.ptr)
								{
									mDnsResponse cResponse = null;
									Resources.resourceMtx.WaitOne();
									foreach(BaseResource cResource in Resources.resourceList)
									{
										if (cResource.Owner == true && 
											cResource.Type == mDnsType.ptr)
										{
											if (((Ptr)cResource).Name == cQuestion.DomainName)
											{
												if (cResponse == null)
												{
													cResponse = new mDnsResponse(server);
												}
												cResponse.AddAnswer(cResource);
											}
										}	
									}
									Resources.resourceMtx.ReleaseMutex();
									
									if (cResponse != null)
									{
										log.Info("Sending a response to a direct PTR Query");
										cResponse.Send();
									}
								}
								else								
								if (cQuestion.RequestType == mDnsType.textStrings)
								{
									mDnsResponse cResponse = null;
									Resources.resourceMtx.WaitOne();
									foreach(BaseResource cResource in Resources.resourceList)
									{
										if (cResource.Owner == true && 
											cResource.Type == mDnsType.textStrings &&
											cResource.Name == cQuestion.DomainName)
										{
											if (cResponse == null)
											{
												cResponse = new mDnsResponse(server);
											}
											cResponse.AddAnswer(cResource);
										}	
									}
									Resources.resourceMtx.ReleaseMutex();
									
									if (cResponse != null)
									{
										log.Info("Sending a response to a direct TextStrings Query");
										cResponse.Send();
									}
								}
							}
						}
						else
						{
							// Handle any responses
							foreach(BaseResource cResource in dnsRequest.AnswerList)
							{
								if (cResource.Type == mDnsType.hostAddress)
								{
									if (cResource.Ttl != 0)
									{
										Resources.AddHostAddress((HostAddress) cResource);
									}
									else
									{
										Resources.RemoveHostAddress(cResource.Name);
									}
								}
								else
								if (cResource.Type == mDnsType.serviceLocation)
								{
									if (cResource.Ttl != 0)
									{
										Resources.AddServiceLocation((ServiceLocation) cResource);
									}
									else
									{
										Resources.RemoveServiceLocation((ServiceLocation) cResource);
									}
								}
								else
								if (cResource.Type == mDnsType.ptr)
								{
									if (cResource.Ttl != 0)
									{
										Resources.AddPtr((Ptr) cResource);
									}
									else
									{
										Resources.RemovePtr((Ptr) cResource);
									}
								}
								else
								if (cResource.Type == mDnsType.textStrings)
								{
									if (cResource.Ttl != 0)
									{
										Resources.AddTextStrings((TextStrings) cResource);
									}
									else
									{
										Resources.RemoveTextStrings((TextStrings) cResource);
									}
								}
							}

							foreach(BaseResource cResource in dnsRequest.AdditionalList)
							{
								if (cResource.Type == mDnsType.hostAddress)
								{
									if (cResource.Ttl != 0)
									{
										Resources.AddHostAddress((HostAddress) cResource);
									}
									else
									{
										Resources.RemoveHostAddress(cResource.Name);
									}
								}
								else
								if (cResource.Type == mDnsType.serviceLocation)
								{
									if (cResource.Ttl != 0)
									{
										Resources.AddServiceLocation((ServiceLocation) cResource);
									}
									else
									{
										Resources.RemoveServiceLocation((ServiceLocation) cResource);
									}
								}
								else
								if (cResource.Type == mDnsType.ptr)
								{
									if (cResource.Ttl != 0)
									{
										Resources.AddPtr((Ptr) cResource);
									}
									else
									{
										Resources.RemovePtr((Ptr) cResource);
									}
								}
								else
								if (cResource.Type == mDnsType.textStrings)
								{
									if (cResource.Ttl != 0)
									{
										Resources.AddTextStrings((TextStrings) cResource);
									}
									else
									{
										Resources.RemoveTextStrings((TextStrings) cResource);
									}
								}
							}
						}
					}
				}
				else
				{
					requestsMtx.ReleaseMutex();
				}
			}
		}

		internal static void LogRequest(DnsRequest req)
		{
			log.Info("");
			log.Info("   MDNS-REQUEST");
			log.Info("   Response from : " + req.Sender.ToString());
			log.Info(String.Format("   Transaction ID: {0}", req.TransactionID));
			//log.Info("Transaction ID: " + dnsRequest.TransactionID.ToString());
			log.Info("   Flags:          " + req.Flags.ToString());
			if (req.QuestionList.Count >= 1)
			{
				log.Info("   Questions:      " + req.QuestionList.Count.ToString());
			}
			if (req.AnswerList.Count >= 1)
			{
				log.Info("   Answers:        " + req.AnswerList.Count.ToString());
			}
			if (req.AuthorityList.Count >= 1)
			{
				log.Info("   Authorities:    " + req.AuthorityList.Count.ToString());
			}
			if (req.AdditionalList.Count >= 1)
			{
				log.Info("   Additional:     " + req.AdditionalList.Count.ToString());
			}

			foreach(Question cQuestion in req.QuestionList)
			{
				log.Info("   QUESTION");
				log.Info("      Domain: " + cQuestion.DomainName);
				log.Info("      Type:   " + cQuestion.RequestType.ToString());
				log.Info("      Class:  " + cQuestion.RequestClass.ToString());
			}

			foreach(BaseResource cResource in req.AnswerList)
			{
				log.Info("   ANSWER");
				RequestHandler.LogResourceRecord(cResource);
			}

			foreach(BaseResource cResource in req.AuthorityList)
			{
				log.Info("   AUTHORITY");
				RequestHandler.LogResourceRecord(cResource);
			}

			foreach(BaseResource cResource in req.AdditionalList)
			{
				log.Info("   ADDITIONAL");
				RequestHandler.LogResourceRecord(cResource);
			}

			log.Info("");
		}

		internal static void LogResourceRecord(BaseResource cResource)
		{
			if (cResource.Type == mDnsType.ptr)
			{
				log.Info("      Source:  " + cResource.Name);
			}
			else
			{
				log.Info("      Domain:  " + cResource.Name);
			}
			log.Info(String.Format("      Type:    {0}", cResource.Type));
			log.Info(String.Format("      Class:   {0}", cResource.Class));
			log.Info(String.Format("      TTL:     {0}", cResource.Ttl));
			if (cResource.Type == mDnsType.hostAddress)
			{
				log.Info(String.Format("      Address: {0}", ((HostAddress) cResource).PrefAddress));
			}
			else
			if (cResource.Type == mDnsType.ptr)
			{
				log.Info(String.Format("      Target:  {0}", ((Ptr) cResource).Target));
			}
			else
			if (cResource.Type == mDnsType.serviceLocation)
			{
				log.Info(String.Format("      Host:    {0}", ((ServiceLocation) cResource).Target));
				log.Info(String.Format("      Port:    {0}", ((ServiceLocation) cResource).Port));
				log.Info(String.Format("      Priority:{0}", ((ServiceLocation) cResource).Priority));
				log.Info(String.Format("      Weight:  {0}", ((ServiceLocation) cResource).Weight));
			}
			else
			if (cResource.Type == mDnsType.textStrings)
			{
				foreach(string s in ((TextStrings) cResource).GetTextStrings())
				{
					log.Info("      TXT:     " + s);
				}
			}
		}
	}
}
