using System;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
		
		static internal Queue		requestsQueue = new Queue();
		static internal Mutex		requestsMtx = new Mutex(false);
		
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
		
		internal static void RequestHandlerThread()
		{
			BaseResource	sResource;
			DnsRequest		dnsRequest;
			//byte[]			buffer = new byte[32768];

			// Setup an endpoint to multi-cast datagrams
			UdpClient server = new UdpClient("224.0.0.251", 5353);

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
						if ((dnsRequest.Flags & DnsFlags.request) == DnsFlags.request)
						{
							Console.WriteLine("  DNS:    Query");
						}
						else
						{
							Console.WriteLine("  DNS:    Response");
						}
						
						Console.WriteLine("Response from : {0}", dnsRequest.Sender);
						Console.WriteLine("Transaction ID: {0}", dnsRequest.TransactionID);
						Console.WriteLine("Questions     : {0}", dnsRequest.QuestionList.Count);
						
						if ((dnsRequest.Flags & DnsFlags.request) == DnsFlags.request)
						{
							foreach(Question cQuestion in dnsRequest.QuestionList)
							{
								if (cQuestion.RequestType == mDnsType.dumpYourGuts)
								{
									Resources.DumpYourGuts(cQuestion);
								}
								else
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
											if (((Ptr)cResource).Target == cQuestion.DomainName)
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
							ArrayList aList = dnsRequest.AnswerList;
							foreach(BaseResource cResource in aList)
							{
								Console.WriteLine("");
								Console.WriteLine("   Domain: " + cResource.Name);
								Console.WriteLine("   Type:   {0}", cResource.Type);
								Console.WriteLine("   Class:  {0}", cResource.Class);
								Console.WriteLine("   TTL:    {0}", cResource.Ttl.ToString());
							
								if (cResource.Type == mDnsType.hostAddress)
								{
									HostAddress hostAddr = (HostAddress) cResource;
									Resources.AddHostAddress(hostAddr);
								}
								else
								if (cResource.Type == mDnsType.serviceLocation)
								{
									ServiceLocation sl = (ServiceLocation) cResource;
									Resources.AddServiceLocation(sl);								
								}
								else
								if (cResource.Type == mDnsType.ptr)
								{
									Ptr ptr = (Ptr) cResource;
									Resources.AddPtr(ptr);
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
	}
}
