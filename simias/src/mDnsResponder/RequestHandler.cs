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
		internal static void RequestHandlerThread()
		{
			DnsRequest		dnsRequest;

			// Setup an endpoint to multi-cast datagrams
			UdpClient server = new UdpClient("224.0.0.251", 5353);

			while(true)
			{
				// Need an event to kick us alive
				dnsRequest = null;
				DnsRequest.requestsMtx.WaitOne();
				if (DnsRequest.requestsQueue.Count > 0)
				{
					try
					{
						dnsRequest = (DnsRequest) DnsRequest.requestsQueue.Dequeue();
					}
					catch{}

					DnsRequest.requestsMtx.ReleaseMutex();

					if (dnsRequest != null)
					{
						Console.WriteLine("Dequeued Request");
						if ((dnsRequest.Flags & DnsFlags.request) == DnsFlags.request)
						{
							Console.WriteLine("  DNS:    Query");
						}
						else
						{
							Console.WriteLine("  DNS:    Response");
						}
						
						ArrayList qList = dnsRequest.QuestionList;
						Console.WriteLine("Response from : {0}", dnsRequest.Sender);
						Console.WriteLine("Transaction ID: {0}", dnsRequest.TransactionID);
						Console.WriteLine("Questions     : {0}", qList.Count);
						/*
						Console.WriteLine("Answers       : {0}", dnsRequest.answers);
						Console.WriteLine("Authority     : {0}", dnsRequest.authorities);
						Console.WriteLine("Additional    : {0}", dnsRequest.additionalAnswers);
						*/
						/*
						Console.WriteLine("  Domain: " + rRequest.Domain);
						Console.WriteLine("  Type:   {0}", rRequest.Type);
						Console.WriteLine("  Class:  {0}", rRequest.Class);
						*/
						
						foreach(Question cQuestion in qList)
						{
							Console.WriteLine("");
							Console.WriteLine("   Domain: " + cQuestion.DomainName);
							Console.WriteLine("   Type:   {0}", cQuestion.RequestType);
							Console.WriteLine("   Class:  {0}", cQuestion.RequestClass);
							
							if (cQuestion.RequestType == mDnsType.dumpYourGuts)
							{
								Resources.DumpYourGuts(cQuestion);
							}
						}
					}
				}
				else
				{
					DnsRequest.requestsMtx.ReleaseMutex();
					Thread.Sleep(1000);
					//Console.WriteLine("RequestHandlerThread - alive");
				}
			}
		}
	}
}
