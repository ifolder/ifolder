/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2005 Novell, Inc.
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

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;

using Simias;
using Simias.Event;
using Simias.Service;
using Simias.Storage;

using Mono.P2p.mDnsResponderApi;


namespace Simias.Domain.mDns
{
	/// <summary>
	/// Class the handles presence as a service
	/// </summary>
	public class Service : IThreadService
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Configuration object for the Collection Store.
		/// </summary>
		private Configuration config;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		public Service()
		{
		}
		#endregion

		#region IThreadService Members
		/// <summary>
		/// Starts the thread service.
		/// </summary>
		/// <param name="config">
		/// Configuration file object for the configured store 
		/// Store to use.
		/// </param>
		public void Start( Configuration config )
		{
			log.Debug("Start called");
			
			bool registered = false;
			this.config = config;
			Store store = Store.GetStore();

			try
			{
				TcpChannel chnl = new TcpChannel();
				ChannelServices.RegisterChannel(chnl);
			
				IRemoteFactory factory = 
					(IRemoteFactory) Activator.GetObject(
						typeof(IRemoteFactory),
						"tcp://localhost:8091/mDnsRemoteFactory.tcp");
						
				// Get my IP address
//				IPHostEntry me = Dns.GetHostByName(Dns.GetHostName());
				IPHostEntry me = Dns.GetHostByName(Environment.MachineName);
				string myAddress = "";
				foreach(IPAddress ipAddr in me.AddressList)
				{
					myAddress = ipAddr.ToString();
					if (myAddress.StartsWith("127.") == false)
					{
						break;
					}
				}

				IResourceRegistration rr = factory.GetRegistrationInstance();
				rr.RegisterHost((string) Environment.MachineName, myAddress);
				
				// Register member as a service location
				rr.RegisterServiceLocation(
					Environment.MachineName,
					Environment.UserName + "@" + Environment.MachineName,
					(int) 8086, 
					0, 
					0);
					
				rr.RegisterPointer(
					"_ifolder_member._tcp.local",
					Environment.UserName + "@" + Environment.MachineName);

				registered = true;
			}
			catch(Exception e1)
			{
				log.Error(e1.Message);
				log.Error(e1.StackTrace);
			}			
		}

		/// <summary>
		/// Resumes a paused service. 
		/// </summary>
		public void Resume()
		{
		}

		/// <summary>
		/// Pauses a service's execution.
		/// </summary>
		public void Pause()
		{
		}

		/// <summary>
		/// Custom.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public void Custom(int message, string data)
		{
		}

		/// <summary>
		/// Stops the service from executing.
		/// </summary>
		public void Stop()
		{
			log.Debug("Stop called");
			try
			{
				IRemoteFactory factory = 
					(IRemoteFactory) Activator.GetObject(
						typeof(IRemoteFactory),
						"tcp://localhost:8091/mDnsRemoteFactory.tcp");
						
				// Get my IP address
				IPHostEntry me = Dns.GetHostByName(Dns.GetHostName());
				string myAddress = "";
				foreach(IPAddress ipAddr in me.AddressList)
				{
					myAddress = ipAddr.ToString();
					break;
				}

				IResourceRegistration rr = factory.GetRegistrationInstance();
				rr.DeregisterPointer(
					"_ifolder_member._tcp.local",
					Environment.UserName + "@" + Environment.MachineName);
				rr.DeregisterHost(Environment.MachineName);
				rr.DeregisterServiceLocation(
					Environment.MachineName,
					Environment.UserName + "@" + Environment.MachineName);
			}
			catch(Exception e1)
			{
				log.Error(e1.Message);
				log.Error(e1.StackTrace);
			}			
		
		}
		#endregion
	}
}
