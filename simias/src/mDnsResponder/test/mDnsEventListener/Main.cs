using System;
using System.Collections;
using System.Runtime.Remoting;
//using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using Mono.P2p.mDnsResponderApi;

namespace mDnsEventListener
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class EventListener
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Hashtable propsTcp = new Hashtable();
			propsTcp["port"] = 0;
			propsTcp["rejectRemoteRequests"] = true;

			BinaryServerFormatterSinkProvider
				serverBinaryProvider = new BinaryServerFormatterSinkProvider();

			BinaryClientFormatterSinkProvider
				clientBinaryProvider = new BinaryClientFormatterSinkProvider();
#if !MONO
			serverBinaryProvider.TypeFilterLevel =
				System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
#endif
			TcpChannel tcpChnl = new TcpChannel(propsTcp, clientBinaryProvider, serverBinaryProvider);
			ChannelServices.RegisterChannel(tcpChnl);
			
			IMDnsEvent cEvent = 
				(IMDnsEvent) Activator.GetObject(
					typeof(Mono.P2p.mDnsResponderApi.IMDnsEvent),
					"tcp://localhost:8092/IMDnsEvent.tcp");

			mDnsEventWrapper eventWrapper = new mDnsEventWrapper();
			eventWrapper.OnLocalEvent += new mDnsEventHandler(OnClientEvent);
			mDnsEventHandler evntHandler = new mDnsEventHandler(eventWrapper.LocalOnEvent);
			cEvent.OnEvent += evntHandler;
//			cEvent.OnEvent += new mDnsEventHandler(eventWrapper.LocalOnEvent);
			Console.WriteLine("Registered for DNS events");
			Console.ReadLine();
		}

		public static void OnClientEvent(string resourceID)
		{
			Console.WriteLine("");
			Console.WriteLine("Resource ID:   {0}", resourceID);
		}

		/*
		public static void OnClientEvent(mDnsEvent lEvent, mDnsType lType, string resourceID)
		{
			Console.WriteLine("");
			Console.WriteLine("Event Type:    {0}", lEvent);
			Console.WriteLine("Resource Type: {0}", lType);
			Console.WriteLine("Resource ID:   {0}", resourceID);
		}
		*/
	}
}
