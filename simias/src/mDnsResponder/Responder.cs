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
		private static readonly log4net.ILog log =
			log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			BasicConfigurator.Configure();
			log.Info("mDnsResponder starting up...");
		
			/*	
			log.Info("getting host address");
			// Create a host record for this instance of the responder
			IPHostEntry ihe = Dns.GetHostByName(Dns.GetHostName());
			
			foreach(IPAddress addr in ihe.AddressList)
			{
				Console.WriteLine("IP Address: " + addr.ToString());
			}
			*/

			string localHost = Environment.MachineName + ".local";
			log.Info("adding host");
		
			/*
				FIXME - temporarily load some records
			*/	

			/*
			localHost = "DELL2BALL.local";
			HostAddress addrHost = new HostAddress(localHost, 300, mDnsType.hostAddress, mDnsClass.iNet, true);
			addrHost.AddIPAddress(IPAddress.Parse("137.65.58.216"));
			Resources.AddHostAddress(addrHost);

			string slName = "jacko@";
			//slName += Environment.MachineName;
			slName += "BRADY-T21";
			slName += "._presence._tcp.local";
			ServiceLocation sl = new ServiceLocation(slName, 300, mDnsType.serviceLocation, mDnsClass.iNet, true);
			sl.Port = 5298;
			sl.Priority = 0;
			sl.Weight = 0;
			sl.Target = localHost;
			Resources.AddServiceLocation(sl);
			
			Ptr ptr = new Ptr("_presence._tcp.local", 300, mDnsType.ptr, mDnsClass.iNet, true);
			ptr.Target = slName;
			Resources.AddPtr(ptr);
			
			TextStrings txtStrs = new TextStrings(slName, 300, mDnsType.textStrings, mDnsClass.iNet, true);
			txtStrs.AddTextString("txtvers=1");
			txtStrs.AddTextString("vc=A!");
			txtStrs.AddTextString("status=avail");
			txtStrs.AddTextString("1st=Michael");
			txtStrs.AddTextString("email=jacko@hotmail.com");
			txtStrs.AddTextString("AIM=jacko33");
			txtStrs.AddTextString("version=1");
			txtStrs.AddTextString("port.p2pj=5298");
			txtStrs.AddTextString("last=Jackson");
			Resources.AddTextStrings(txtStrs);
			*/
			
			Registration.Startup();
			RequestHandler.StartRequestHandler();
			Resources.StartMaintenanceThread();
			DnsRequest.StartDnsReceive();

			log.Info("waiting for console input...");
			Console.ReadLine();

			DnsRequest.StopDnsReceive();
			Thread.Sleep(0);
			RequestHandler.StopRequestHandler();
			Resources.StopMaintenanceThread();
			Thread.Sleep(1000);
		}
	}
}
