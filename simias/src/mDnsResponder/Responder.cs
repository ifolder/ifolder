using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using log4net;
using log4net.Config;

namespace Mono.P2p.mDnsResponder
{
	/// <summary>
	/// Summary description for Responder
	/// </summary>
	class Responder
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			BasicConfigurator.Configure();
			log.Info("mDnsResponder starting up...");
			
			log.Info("getting host address");
			// Create a host record for this instance of the responder
			IPHostEntry ihe = Dns.GetHostByName(Dns.GetHostName());

			string localHost = Environment.MachineName + ".local";
			mDnsHost thisHost = new mDnsHost(Environment.MachineName, (int) ihe.AddressList[0].Address, true);
		
			log.Info("adding host");
//			mDnsHost.Add(thisHost);
			
			HostAddress addrHost = new HostAddress(localHost, 300, mDnsType.hostAddress, mDnsClass.iNet, true);
			addrHost.AddIPAddress(ihe.AddressList[0].Address);
			Resources.AddHostAddress(addrHost);

			string slName = "banderso@";
			slName += Environment.MachineName;
			slName += "._presence._tcp.local";
			ServiceLocation sl = new ServiceLocation(slName, 300, mDnsType.serviceLocation, mDnsClass.iNet, true);
			sl.Port = 5298;
			sl.Priority = 5;
			sl.Weight = 10;
			Resources.AddServiceLocation(sl);

			log.Info("starting RequestHandler thread");
			Thread reqHandler = new Thread(new ThreadStart(RequestHandler.RequestHandlerThread));
			reqHandler.IsBackground = true;
			reqHandler.Start();

			log.Info("starting Maintenance thread");
			Thread maintThread = new Thread(new ThreadStart(mDnsHost.MaintenanceThread));
			maintThread.IsBackground = true;
			maintThread.Start();

			log.Info("starting up DnsReceive");
			// Startup up the Dns Receive thread
			DnsRequest.StartDnsReceive();

			log.Info("waiting for console input...");
			Console.ReadLine();

			DnsRequest.StopDnsReceive();
			Thread.Sleep(0);
			reqHandler.Abort();
		}
	}
}
