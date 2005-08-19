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
		private Simias.mDns.User mDnsUser = null;
		private Simias.mDnsProvider mDnsProvider = null;

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
				new Simias.mDns.Domain( null );
				this.mDnsUser = new Simias.mDns.User();

				// Registers our iFolder member with the Bonjour
				// service daemon.
				Simias.mDns.User.RegisterUser();

				// Load the members in the Bonjour domain into
				// our current list which is kept in memory
				if ( BonjourUsers.LoadMembersFromDomain( false ) == false )
				{
					log.Error( "Failed loading the members from the Rendezvous domain" );
				}

				// Start up an mDns browse session which watches
				// for the coming and going of iFolder Rendezvous users.
				Simias.mDns.User.StartMemberBrowsing();

				// Might not be needed in the future but today
				// the sync thread collects all the Member meta
				// from the Bonjour daemon when a new member
				// is added to the list.
				Simias.mDns.Sync.StartSyncThread();

				// Register with the DomainProvider service
				// Provides location resolution, member searching
				// and authentication to the Rendezvous domain
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

			if ( this.mDnsProvider != null )
			{
				Simias.DomainProvider.Unregister( this.mDnsProvider );
			}

			if ( this.mDnsUser != null )
			{
				Simias.mDns.User.StopMemberBrowsing();
				Simias.mDns.User.UnregisterUser();
			}

			Simias.mDns.Sync.StopSyncThread();
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
				}
			}
		}

		#endregion

		#region Private Methods
		#endregion
	}

	/// <summary>
	/// Temporary
	/// Class for controlling the synchronization thread
	/// </summary>
	public class Sync
	{
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		static AutoResetEvent syncEvent = null;
		static bool up = false;
		static bool syncOnStart = true;
		static int syncInterval = 60 * 1000;
		static Thread syncThread = null;

		internal static Simias.mDns.User mdnsUser = null;

		internal static int StartSyncThread()
		{
			int status = 0;

			try
			{
				mdnsUser = new Simias.mDns.User();
				syncEvent = new AutoResetEvent(false);
				up = true;
				syncThread = new Thread( new ThreadStart( Sync.SyncThread ) );
				syncThread.IsBackground = true;
				syncThread.Start();
			}
			catch( SimiasException e )
			{
				log.Debug( e.Message );
				log.Debug( e.StackTrace );
				status = -1;
			}

			return status;
		}

		internal static int StopSyncThread()
		{
			int status = 0;
			up = false;
			try
			{
				syncEvent.Set();
				Thread.Sleep(32);
				mdnsUser = null;
				log.Debug("StopSyncThread finished");
			}
			catch( SimiasException e )
			{
				log.Debug("StopSyncThread failed with an exception");
				log.Debug(e.Message);
				status = -1;
			}

			return status;
		}

		public static int SyncNow(string data)
		{
			log.Debug( "SyncNow called" );
			syncEvent.Set();
			log.Debug( "SyncNow finished" );
			return 0;
		}

		internal static void SyncThread()
		{
			while ( up == true )
			{
				if ( syncOnStart == false )
				{
					syncEvent.WaitOne( syncInterval, false );
				}

				// Always wait after the first iteration
				syncOnStart = false;
				mdnsUser.SynchronizeMembers();
			}

			syncEvent.Close();
		}
	}
}
