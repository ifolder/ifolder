using System;
using System.Runtime.Remoting;
//using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using Mono.P2p.mDnsResponderApi;

namespace mDnsEventPublisher
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class EventPublisher
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
//			HttpChannel chnl = new HttpChannel();
//			ChannelServices.RegisterChannel(chnl);
			TcpChannel chnl = new TcpChannel();
			ChannelServices.RegisterChannel(chnl);
			
			IMDnsEvent cEvent = 
				(IMDnsEvent) Activator.GetObject(
				typeof(IMDnsEvent),
				"tcp://localhost:8092/IMDnsEvent.tcp");

			//IMDnsEvent cEvent = factory.GetEventInstance();
			//cEvent.Publish(mDnsEvent.added, mDnsType.hostAddress, "5");
			cEvent.Publish("5");
		}
	}
}
