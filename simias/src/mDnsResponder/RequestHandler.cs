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
			DnsRequest		dnsRequest;
			byte[]			buffer = new byte[32768];

			// Setup an endpoint to multi-cast datagrams
			UdpClient server = new UdpClient("224.0.0.251", 5353);

			while(reqHandlerEvent.WaitOne(120000, false))
			{
				if (mDnsStopping == true)
				{
					return;
				}

				Console.WriteLine("RequestHandler alive!");
				
				// Need an event to kick us alive
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
									int	index = 0;
									
									Resources.resourceMtx.WaitOne();
									foreach(BaseResource cResource in Resources.resourceList)
									{
										if (cResource.Owner == true && 
											cResource.Type == mDnsType.ptr &&
											cResource.Name == cQuestion.DomainName)
    									{
    										log.Info("Sending a response to a direct HA Query");
											HostAddress ha = (HostAddress) cResource;
											
											index =
												Resources.ResponseHeaderToBuffer(
													cResource,
													(short) dnsRequest.TransactionID,
													(short) 0x0084,
													1,
													buffer);
												
											index = Resources.HostAddressToBuffer(ha, index, buffer);		
										}	
									}
									Resources.resourceMtx.ReleaseMutex();
									
									if (index != 0)
									{
										try
										{
											server.Send(buffer, index);
										}
										catch(Exception e)
										{
										
											log.Info("Failed sending HA record", e);
										}
									}
								}
								else
								if (cQuestion.RequestType == mDnsType.serviceLocation)
								{
									int	index = 0;
									
									Resources.resourceMtx.WaitOne();
									foreach(BaseResource cResource in Resources.resourceList)
									{
										if (cResource.Owner == true && 
											cResource.Type == mDnsType.ptr &&
											cResource.Name == cQuestion.DomainName)
    									{
    										log.Info("Sending a response to a direct SL Query");
											ServiceLocation sl = (ServiceLocation) cResource;
											
											index =
												Resources.ResponseHeaderToBuffer(
													cResource,
													(short) dnsRequest.TransactionID,
													(short) 0x0084,
													1,
													buffer);
												
											index = Resources.ServiceLocationToBuffer(sl, index, buffer);		
										}	
									}
									Resources.resourceMtx.ReleaseMutex();
									
									if (index != 0)
									{
										try
										{
											server.Send(buffer, index);
										}
										catch(Exception e)
										{
										
											log.Info("Failed sending SL record", e);
										}
									}
								}
								else
								if (cQuestion.RequestType == mDnsType.ptr)
								{
									int	index = 0;
									
									Resources.resourceMtx.WaitOne();
									foreach(BaseResource cResource in Resources.resourceList)
									{
										if (cResource.Owner == true && 
											cResource.Type == mDnsType.ptr &&
											cResource.Name == cQuestion.DomainName)
    									{
    										log.Info("Sending a response to a direct PTR Query");
											Ptr ptr = (Ptr) cResource;
											
											index =
												Resources.ResponseHeaderToBuffer(
													cResource,
													(short) dnsRequest.TransactionID,
													(short) 0x0084,
													1,
													buffer);
												
											index = Resources.PtrToBuffer(ptr, index, buffer);		
										}	
									}
									Resources.resourceMtx.ReleaseMutex();
									
									if (index != 0)
									{
										try
										{
											server.Send(buffer, index);
										}
										catch(Exception e)
										{
										
											log.Info("Failed sending PTR record", e);
										}
									}
								}
								else								
								if (cQuestion.RequestType == mDnsType.textStrings)
								{
									int	index = 0;
									
									Resources.resourceMtx.WaitOne();
									foreach(BaseResource cResource in Resources.resourceList)
									{
										if (cResource.Owner == true && 
											cResource.Type == mDnsType.textStrings &&
											cResource.Name == cQuestion.DomainName)
    									{
    										log.Info("Sending a response to a direct TextStrings Query");
											TextStrings txtStrs = (TextStrings) cResource;
											
											index =
												Resources.ResponseHeaderToBuffer(
													cResource,
													(short) dnsRequest.TransactionID,
													(short) 0x0084,
													1,
													buffer);
												
											index = Resources.TextStringsToBuffer(txtStrs, index, buffer);		
										}	
									}
									Resources.resourceMtx.ReleaseMutex();
									
									if (index != 0)
									{
										try
										{
											server.Send(buffer, index);
										}
										catch(Exception e)
										{
										
											log.Info("Failed sending Host Address record", e);
										}
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
					//Thread.Sleep(1000);
					//Console.WriteLine("RequestHandlerThread - alive");
				}
			}
		}
	}
}
