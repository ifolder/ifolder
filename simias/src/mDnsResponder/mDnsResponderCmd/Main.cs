// project created on 4/15/04 at 9:12 a
using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels;
//using Mono.P2p.mDnsResponder;
using Mono.P2p.mDnsResponderApi;

class MainClass
{
	public static void Main(string[] args)
	{
		if (args.Length >= 1)
		{
			HttpChannel chnl = new HttpChannel();
			ChannelServices.RegisterChannel(chnl);
			
			IRemoteFactory factory = 
				(IRemoteFactory) Activator.GetObject(
					typeof(IRemoteFactory),
					"http://localhost:8091/factory.soap");

			if (args[0].ToLower() == "lookupresource" && args.Length >= 2)
			{
				int			status;
				short		rType = 0;
				
				IResourceQuery rq = factory.GetQueryInstance();
				status = rq.LookupResource(args[1], ref rType);
				if (status == 0)
				{
					if (rType == (short) mDnsType.serviceLocation)
					{
			 			int		port = 0;
			 			int		priority = 0;
			 			int		weight = 0;
			 			string 	host = "";
			 			
						rq.LookupServiceLocation(
							"bradys-ifolder._collection._tcp.local",
							ref host,
							ref port,
							ref priority,
							ref weight);
					}
				}
			}
			else
			if (args[0].ToLower() == "registerhost")
			{
				IResourceRegistration rr = factory.GetRegistrationInstance();		
 				rr.RegisterHost("BRADYS-FUNKY-MACX-MACHINE.local", "192.168.1.101");
			}
			else
			if (args[0].ToLower() == "servicelocation")
			{
				IResourceRegistration rr = factory.GetRegistrationInstance();		
 				rr.RegisterServiceLocation("BRADY-T21.local", "bradys-ifolder._collection._tcp.local", 6666, 0, 0);
 			}
 			else
 			if (args[0].ToLower() == "lookup")
 			{
 				string	host = null;
 				int		port = 0;
 				int		priority = 0;
 				int		weight = 0;
 				
				IResourceQuery rq = factory.GetQueryInstance();
				rq.LookupServiceLocation(
					"bradys-ifolder._collection._tcp.local",
					ref host,
					ref port,
					ref priority,
					ref weight);
					
				Console.WriteLine("Host: " + host);
				Console.WriteLine("Port: " + port.ToString());		
 			}
 			else
 			if (args[0].ToLower() == "host")
 			{
 				RHostAddress	ha = null;

				IResourceQuery rq = factory.GetQueryInstance();
				rq.LookupHost("BRADY-T21", out ha);
					
				Console.WriteLine("IPADDRESS: " + ha.PrefAddress);
 			}
		}
	}
}