using System;
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
			DnsRequest		rRequest;

			// Setup an endpoint to multi-cast datagrams
			UdpClient server = new UdpClient("224.0.0.251", 5353);

			while(true)
			{
				// Need an event to kick us alive
				rRequest = null;
				DnsRequest.requestsMtx.WaitOne();
				if (DnsRequest.requestsQueue.Count > 0)
				{
					try
					{
						rRequest = (DnsRequest) DnsRequest.requestsQueue.Dequeue();
					}
					catch{}

					DnsRequest.requestsMtx.ReleaseMutex();

					if (rRequest != null)
					{
						Console.WriteLine("Dequeued Request");
						Console.WriteLine("  Domain: " + rRequest.Domain);
						Console.WriteLine("  Type:   {0}", rRequest.Type);
						Console.WriteLine("  Class:  {0}", rRequest.Class);
					}
				}
				else
				{
					DnsRequest.requestsMtx.ReleaseMutex();
					Thread.Sleep(5000);
					//Console.WriteLine("RequestHandlerThread - alive");
				}
			}
		}
	}
}
