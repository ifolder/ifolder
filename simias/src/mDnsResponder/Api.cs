/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

//
// Source file contains all the APIs for registering/quering
// the multi-cast DNS responder
//

using System;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using Mono.P2p.mDnsResponderApi;

namespace Mono.P2p.mDnsResponder
{
	/// <summary>
	/// Summary description for ResourceRegistration
	/// </summary>'
	public class ResourceRegistration : MarshalByRefObject, IResourceRegistration
	{
		#region Class Members
		#endregion

		#region Properties
		#endregion

		#region Constructors
		public ResourceRegistration()
		{
		}
		#endregion

		#region Private Methods
		#endregion

		#region Static Methods
		#endregion
		
		#region Public Methods
		public int	RegisterHost(string host, string ipAddress)
		{
			HostAddress hostAddr = new
				HostAddress(
					host, 
					Defaults.timeToLive, 
					mDnsType.hostAddress, 
					mDnsClass.iNet, 
					true);
					
			hostAddr.AddIPAddress(IPAddress.Parse(ipAddress));
			Resources.AddHostAddress(hostAddr);
			return(0);
		}

		public int	DeregisterHost(string host)
		{
			Resources.RemoveHostAddress(host);
			return(0);
		}
		
		public int	RegisterServiceLocation(string host, string serviceName, int port, int priority, int weight)
		{
			ServiceLocation	sl = new
				ServiceLocation(
					serviceName, 
					Defaults.timeToLive, 
					mDnsType.serviceLocation, 
					mDnsClass.iNet, 
					true);
					
			sl.Target = host;
			sl.Port = port;
			sl.Priority = priority;
			sl.Weight = weight;
			Resources.AddServiceLocation(sl);
			return(0);
		}
		
		public int	DeregisterServiceLocation(string host, string serviceName)
		{
			ServiceLocation	sl = new
				ServiceLocation(
					serviceName, 
					Defaults.timeToLive, 
					mDnsType.serviceLocation, 
					mDnsClass.iNet, 
					true);
					
			sl.Target = host;
			Resources.RemoveServiceLocation(sl);
			return(0);
		}

		public int	RegisterPointer(string domain, string target)
		{
			Ptr	ptr = new Ptr(domain, Defaults.ptrTimeToLive, mDnsType.ptr, mDnsClass.iNet, true);
			ptr.Target = target;
			Resources.AddPtr(ptr); 
			return(0);
		}
		
		public int	DeregisterPointer(string domain, string target)
		{
			Ptr	ptr = new Ptr(domain, Defaults.timeToLive, mDnsType.ptr, mDnsClass.iNet, true);
			ptr.Target = target;
			Resources.RemovePtr(ptr); 
			return(0);
		}

		public int	RegisterTextStrings(string serviceName, string[] txtStrings)
		{
			Console.WriteLine("ResourceRegistration::RegisterTextStrings called");
			TextStrings ts = new
				TextStrings(
					serviceName,
					Defaults.timeToLive,
					mDnsType.textStrings,
					mDnsClass.iNet,
					true);

			foreach(string ss in txtStrings)
			{
				ts.AddTextString(ss);
			}
					
			Resources.AddTextStrings(ts);
			return(0);
		}
		
		public int	DeregisterTextStrings(string serviceName)
		{
			TextStrings ts = new
				TextStrings(
					serviceName,
					Defaults.timeToLive,
					mDnsType.textStrings,
					mDnsClass.iNet,
					true);
			// FIXME - add a a remove text string to Resources
			// Resources.RemoveTextStrings(ts);
			return(0);
		}

		#endregion
	}

	/// <summary>
	/// Summary description for ResourceQuery
	/// </summary>'
	public class ResourceQuery : MarshalByRefObject, IResourceQuery
	{
		#region Constructors
		public ResourceQuery()
		{
		}
		#endregion

		#region Public Methods

		public int LookupResource(string name, ref short rtype)
		{
			Console.WriteLine("LookupResource called");
			int status = 0;
			rtype = 0;
			BaseResource cResource = Resources.GetResource(name);
			if (cResource != null)
			{
				rtype = (short) cResource.Type;
			}
			else
			{
				status = -1;
			}
			
			return(status);
		}

		public int LookupServiceLocation(string serviceName, ref string host, ref int port, ref int priority, ref int weight)
		{
			int status = 0;
			ServiceLocation cLoc = Resources.GetServiceLocation(serviceName);
			if (cLoc != null)
			{
				host = cLoc.Target;
				port = cLoc.Port;
				priority = cLoc.Priority;
				weight = cLoc.Weight;
			}
			else
			{
				status = -1;
			}	

			return(status);	
		}

		public int LookupHost(string host, ref RHostAddress ha)
		{
			Console.WriteLine("LookupHost called");
			int status = 0;
			ha = null;
			HostAddress cHost = Resources.GetHostAddress(host);
			if (cHost != null)
			{
				ha = new RHostAddress(cHost.Name, cHost.Ttl, (Mono.P2p.mDnsResponderApi.mDnsType) cHost.Type, (Mono.P2p.mDnsResponderApi.mDnsClass) cHost.Class, false);
				ha.AddIPAddress(cHost.PrefAddress);
			}
			
			return(status);
		}

		public int	LookupPtr(string domain, ref string target)
		{
			return(-1);
		}	
	
		#endregion
	}

	/// <summary>
	/// Summary description for ResourceQuery
	/// </summary>'
	public class mDnsLog : MarshalByRefObject, IMDnsLog
	{
		#region Constructors
		public mDnsLog()
		{
		}
		#endregion

		#region Public Methods

		public int DumpResourceRecords()
		{
			Question cQuestion = new Question();
			Resources.DumpYourGuts(cQuestion);
			return(0);
		}

		public int DumpLocalRecords()
		{
			Resources.DumpLocalRecords();
			return(0);
		}

		public int DumpStatistics()
		{
			//Resources.DumpYourGuts();
			return(0);
		}
		#endregion
	}
	
	public class mDnsRemoteFactory: MarshalByRefObject, IRemoteFactory
	{
		public IResourceRegistration GetRegistrationInstance()
		{
			return(new ResourceRegistration());
		}
		
		public IResourceQuery GetQueryInstance()
		{
			return(new ResourceQuery());
		}

		public IMDnsLog GetLogInstance()
		{
			return(new mDnsLog());
		}

		/*
		public IMDnsEvent GetEventInstance()
		{
			return(new MDnsEvent());
		}
		*/
	}

	public class MDnsEvent : MarshalByRefObject, IMDnsEvent
	{
		public event Mono.P2p.mDnsResponderApi.mDnsEventHandler OnEvent;

		/*
		public void Publish(mDnsEvent mEvent, Mono.P2p.mDnsResponderApi.mDnsType mType, string resourceID)
		{
			Console.WriteLine("Publish called");
			this.MyPublish(mEvent, mType, resourceID);
		}

		private void MyPublish(mDnsEvent mEvent, Mono.P2p.mDnsResponderApi.mDnsType mType, string resourceID)
		{
			if (OnEvent != null)
			{
				mDnsEventHandler lEvent = null;
				foreach( Delegate cDelegate in OnEvent.GetInvocationList())
				{
					try
					{
						lEvent = (mDnsEventHandler) cDelegate;
						lEvent(mEvent, mType, resourceID);
					}
					catch
					{
						OnEvent -= lEvent;
					}
				}
			}
		}
		*/

		public void Publish(string resourceID)
		{
			Console.WriteLine("Publish called");
			this.MyPublish(resourceID);
		}

		private void MyPublish(string resourceID)
		{
			if (OnEvent != null)
			{
				Console.WriteLine("OnEvent is good here");
				mDnsEventHandler lEvent = null;
				foreach( Delegate cDelegate in OnEvent.GetInvocationList())
				{
					try
					{
						lEvent = (mDnsEventHandler) cDelegate;
						lEvent(resourceID);
//						lEvent(mEvent, mType, resourceID);
					}
					catch
					{
						Console.WriteLine("failing in delegate call");
						OnEvent -= lEvent;
					}
				}
			}
		}

		public override object InitializeLifetimeService()
		{
			// live forever and ever and ever
			return(null);
		}
	}
	
	/// <summary>
	/// Summary description for ServiceRegistrationmDnsHost
	/// </summary>'
	public class Registration
	{
		#region Class Members
		#endregion

		#region Properties
		#endregion

		#region Constructors
		#endregion

		#region Private Methods
		#endregion

		#region Static Methods
		public static int Startup()
		{
			/*
			Hashtable props = new Hashtable();
			props["port"] = 8091;

			SoapServerFormatterSinkProvider
				serverProvider = new SoapServerFormatterSinkProvider();

			SoapClientFormatterSinkProvider
				clientProvider = new SoapClientFormatterSinkProvider();
#if !MONO
			serverProvider.TypeFilterLevel =
				System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
#endif
			HttpChannel chnl = new HttpChannel(props, clientProvider, serverProvider);

			//HttpChannel chnl = new HttpChannel(8091);

			ChannelServices.RegisterChannel(chnl);
			
			RemotingConfiguration.RegisterWellKnownServiceType(
				typeof(mDnsRemoteFactory),
				"factory.soap",
				WellKnownObjectMode.Singleton);
			*/

			Hashtable propsTcp = new Hashtable();
			propsTcp["port"] = 8092;
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
			
			RemotingConfiguration.RegisterWellKnownServiceType(
				typeof(MDnsEvent),
				"IMDnsEvent.tcp",
				WellKnownObjectMode.Singleton);
				
			return(0);
		}
		
		public static int Shutdown()
		{
			return(0);
		}

		#endregion

		#region Public Methods
		#endregion
	}
	
}
