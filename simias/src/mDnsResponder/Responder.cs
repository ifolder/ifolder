using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mono.P2p.mDnsResponder
{
	/// <summary>
	/// Summary description for Responder
	/// </summary>
	class Responder
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			// Create a host record for this instance of the responder
			IPHostEntry ihe = Dns.GetHostByName(Dns.GetHostName());

			//string localHost = Environment.MachineName + ".local";
			mDnsHost thisHost = new mDnsHost(Environment.MachineName, (int) ihe.AddressList[0].Address, true);
		
			mDnsHost.Add(thisHost);

			Thread reqHandler = new Thread(new ThreadStart(RequestHandler.RequestHandlerThread));
			reqHandler.IsBackground = true;
			reqHandler.Start();

			Thread maintThread = new Thread(new ThreadStart(mDnsHost.MaintenanceThread));
			maintThread.IsBackground = true;
			maintThread.Start();

			// Startup up the Dns Receive thread
			DnsRequest.StartDnsReceive();

			Console.ReadLine();

			DnsRequest.StopDnsReceive();
			Thread.Sleep(0);
			reqHandler.Abort();
		}


	}
}
