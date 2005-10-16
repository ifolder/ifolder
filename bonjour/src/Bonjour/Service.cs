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
using System.Text;
using System.Threading;

using Simias;
using Simias.Authentication;
using Simias.Client;
using Simias.Event;
using Simias.POBox;
using Simias.Service;
using Simias.Storage;
using Simias.Sync;

using SCodes = Simias.Authentication.StatusCodes;

//using Mono.P2p.mDnsResponder;
//using Mono.P2p.mDnsResponderApi;
using Novell.Security.ClientPasswordManager;

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
			SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

		private static string inCredentialEvent = "true";
		//private Simias.mDns.User mDnsUser = null;
		private Simias.mDnsProvider mDnsProvider = null;
		private Simias.mDns.Register register = null;
		private Simias.mDns.Browser browser = null;
		private Simias.mDns.Publish publish = null;
		private Simias.mDns.Domain domain = null;

		// For controlling the monitor thread
		private AutoResetEvent monitorEvent = null;
		private bool up = false;
		private Thread monitorThread = null;

		// Register for authentication events
		private Simias.Authentication.NeedCredentialsEventSubscriber needsCreds;

		/// <summary>
		/// Configuration object for the Collection Store.
		/// </summary>
		internal Configuration config;
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
			log.Debug( "Start called" );
			this.config = config;

			try
			{
				// Newing up the mDns domain will create the
				// Bonjour domain if it does not already exist.
				domain = new Simias.mDns.Domain( null );

				log.Debug( "domain.UserName: " + domain.UserName );
				log.Debug( "domain.UserID: " + domain.UserID );

				// Registers our iFolder member with the Bonjour
				// service daemon.
				register = new Register( domain.UserID, domain.UserName );
				register.RegisterUser();

				// Start up an mDns browse session which watches
				// for the coming and going of iFolder Bonjour users.
				browser = new Simias.mDns.Browser();

				// Load up all Bonjour iFolder users that are persisted
				// in the Bonjour (p2p) domain
				browser.LoadMembersFromDomain( false );

				// Start browsing which manages the coming and going of
				// iFolder Bonjour users.
				browser.StartBrowsing();

				// Register all P2P collections with MDNS
				publish = new Simias.mDns.Publish();
				publish.StartPublishing();

				// Might not be needed in the future but today
				// the sync thread collects all the Member meta
				// from the Bonjour daemon when a new member
				// is added to the list.
				//Simias.mDns.Sync.StartSyncThread();

				StartMonitorThread();

				// Register with the DomainProvider service
				// Provides location resolution, member searching
				// and authentication to the Bonjour domain
				this.mDnsProvider = new Simias.mDnsProvider();
				Simias.DomainProvider.RegisterProvider( this.mDnsProvider );

				// The Simias architecture perfroms all client authentication
				// out of band which means when a client service requests remote 
				// objects and fails because of authentication errors or because
				// credentials don't exist a "Need Credentials" event is generated
				// The mDnsDomain service watches for these events and performs
				// authentication when an event signals authentication is needed
				// to a remote Bonjour domain.
				this.needsCreds = new NeedCredentialsEventSubscriber();
				this.needsCreds.NeedCredentials += 
					new Simias.Authentication.NeedCredentialsEventHandler( OnCredentialsEventHandler );
			}
			catch(Exception e)
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
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
		public int Custom(int message, string data)
		{
			return 0;
		}

		/// <summary>
		/// Stops the service from executing.
		/// </summary>
		public void Stop()
		{
			log.Debug( "Stop called" );

			// Unregister from the Domain container
			if ( this.mDnsProvider != null )
			{
				Simias.DomainProvider.Unregister( this.mDnsProvider );
			}

			if ( this.publish != null )
			{
				this.publish.StopPublishing();
			}

			if ( this.browser != null )
			{
				browser.StopBrowsing();
			}

			if ( register != null )
			{
				register.UnregisterUser();
				register = null;
			}

			StopMonitorThread();
		}

		/// <summary>
		/// Handler that's called for all NeedCredential events
		/// If the collection is a mDns slave, authenticate
		/// to the remote server.
		/// </summary>
		public 
		static 
		void 
		OnCredentialsEventHandler( 
			Simias.Client.Event.NeedCredentialsEventArgs args)
		{
			if ( args.DomainID == Simias.mDns.Domain.ID )
			{
				Simias.Authentication.Status authStatus;

				lock ( Simias.mDns.Service.inCredentialEvent )
				{
					Simias.mDns.ClientAuthentication clientAuth =
						new Simias.mDns.ClientAuthentication();

					authStatus = clientAuth.Authenticate( args.CollectionID );
					if ( authStatus.statusCode == SCodes.Success )
					{
						string userID = 
							Store.GetStore().GetDomain( args.DomainID ).GetCurrentMember().UserID;

						// Set credentials for this collection
						new NetCredential( 
							"iFolder", 
							args.CollectionID, 
							true, 
							userID,
							"@PPK@" );
					}
					else
					{
						log.Error( 
							"Failed authentication to Collection: " + 
							args.CollectionID + 
							" Status: " + 
							authStatus.statusCode.ToString() );
					}
				}
			}
		}

		#endregion

		#region Private Methods
		internal int StartMonitorThread()
		{
			int status = 0;

			try
			{
				//mdnsUser = new Simias.mDns.User();
				monitorEvent = new AutoResetEvent(false);
				up = true;
				monitorThread = new Thread( new ThreadStart( MonitorThread ) );
				monitorThread.IsBackground = true;
				monitorThread.Start();
			}
			catch( SimiasException e )
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
				status = -1;
			}

			return status;
		}

		internal int StopMonitorThread()
		{
			int status = 0;
			up = false;
			try
			{
				monitorEvent.Set();
				Thread.Sleep(32);
			}
			catch( SimiasException e )
			{
				log.Error( "StopMonitorThread failed with an exception" );
				log.Error( e.Message );
				status = -1;
			}

			return status;
		}

		private void MonitorThread()
		{
			while ( up == true )
			{
				monitorEvent.WaitOne( 60000, false );
				log.Debug( "Checking for host change" );
				if( domain.CheckForHostChange() == true )
				{
					log.Debug( "iFolder Bonjour detected a host change" );
					register.UnregisterUser();
					register = null;
					Thread.Sleep( 32 );

					// Registers our iFolder member with the Bonjour
					// service daemon.
					log.Debug( "  reregistering service" );
					register = new Register( domain.UserID, domain.UserName );
					register.RegisterUser();
				}
			}

			monitorEvent.Close();
		}
		#endregion
	}
}
