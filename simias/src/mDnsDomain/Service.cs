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
using Simias.POBox;
using Simias.Service;
using Simias.Storage;

using Mono.P2p.mDnsResponderApi;


namespace Simias.mDns
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

		//private string mDnsUserID;
		//private string mDnsPOBoxID;
		private	IMDnsEvent cEvent = null;

		private	Store store = null;
		private Simias.mDns.User mDnsUser = null;

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
			this.config = config;

			string myAddress = MyDns.GetHostName();
			log.Debug("  My Address: " + myAddress);
//			Thread.Sleep(20000);
			store = Store.GetStore();

			//
			// Make sure the mDnsDomain exists
			//

			Simias.mDns.Domain mdnsDomain = null;
			try
			{
				mdnsDomain = new Simias.mDns.Domain( true );
				this.mDnsUser = new Simias.mDns.User();
				this.mDnsUser.BroadcastUp();
			}
			catch(Exception e)
			{
				log.Error(e.Message);
				log.Error(e.StackTrace);
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

			if ( this.mDnsUser != null )
			{
				this.mDnsUser.BroadcastDown();
			}
		}

		/// <summary>
		/// Event handler for capturing new _ifolder_members
		/// </summary>
		public
		static
		void 
		OnMDnsEvent(mDnsEventAction action, mDnsType rType, BaseResource cResource)
		{
			log.Debug("");
			log.Debug("Event Type:    {0}", action);
			log.Debug("Resource Type: {0}", rType);
			log.Debug("Resource Name: " + cResource.Name);
			log.Debug("Resource ID:   " + cResource.ID);
			log.Debug("TTL:           " + cResource.Ttl);

			/*
			if (rType == mDnsType.textStrings)
			{
				foreach(string s in ((TextStrings) cResource).GetTextStrings())
				{
					Console.WriteLine("TXT:           " + s);
				}
			}
			else
			if (rType == mDnsType.hostAddress)
			{
				Console.WriteLine(String.Format("Address:       {0}", ((HostAddress) cResource).PrefAddress));
			}
			else
			if (rType == mDnsType.serviceLocation)
			{
				Console.WriteLine(String.Format("Host:          {0}", ((ServiceLocation) cResource).Target));
				Console.WriteLine(String.Format("Port:          {0}", ((ServiceLocation) cResource).Port));
				Console.WriteLine(String.Format("Priority:      {0}", ((ServiceLocation) cResource).Priority));
				Console.WriteLine(String.Format("Weight:        {0}", ((ServiceLocation) cResource).Weight));
			}
			else
			*/
			if (rType == mDnsType.ptr && action == Mono.P2p.mDnsResponderApi.mDnsEventAction.added)
			{
				log.Debug(String.Format("Target:        {0}", ((Ptr) cResource).Target));

				if (cResource.Name == "_ifolder_member._tcp.local")
				{
					try
					{
						Store store = Store.GetStore();
						Simias.mDns.Domain mdnsDomain = new Simias.mDns.Domain(false);
						Simias.Storage.Domain rDomain = store.GetDomain( mdnsDomain.ID );
						Simias.Storage.Roster mdnsRoster = rDomain.Roster;

						Member mdnsMember = mdnsRoster.GetMemberByName( ((Ptr) cResource).Target );
						if ( mdnsMember != null )
						{
							// Update IP Address?
						}
						else
						{
							log.Debug(String.Format("Adding member: {0} to the Rendezvous Roster", ((Ptr) cResource).Target));

							// FIXME::Need to query mDns to get the address where the user is located
							// and to get his ID from the TTL resource
							mdnsMember = 
								new Member( ((Ptr) cResource).Target, Guid.NewGuid().ToString(), Access.Rights.ReadOnly );

							mdnsRoster.Commit( new Node[] { mdnsMember } );
						}
					}
					catch(Exception e)
					{
						log.Error( e.Message );
						log.Error( e.StackTrace );
					}
				}
			}
		}

		#endregion
	}
}
