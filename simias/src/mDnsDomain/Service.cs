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
using Simias.Authentication;
using Simias.Client;
using Simias.Event;
using Simias.POBox;
using Simias.Service;
using Simias.Storage;
using Simias.Sync;

using Mono.P2p.mDnsResponder;
using Mono.P2p.mDnsResponderApi;
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
		private	Store store = null;
		private Simias.mDns.User mDnsUser = null;
		private Simias.mDnsProvider mDnsProvider = null;
		private Simias.Location.mDnsProvider locProvider = null;

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
			log.Debug("Start called");
			this.config = config;

			string myAddress = MyDns.GetHostName();
			store = Store.GetStore();

			//
			// Startup up the mDnsResponder
			//

			// TODO: Need to issue a ping call to see if the mDnsResponder
			// is already running
			//Responder.Startup( config.StorePath );

			//
			// Make sure the mDnsDomain exists
			//

			Simias.mDns.Domain mdnsDomain = null;
			try
			{
				mdnsDomain = new Simias.mDns.Domain( true );
				this.mDnsUser = new Simias.mDns.User();

				Simias.mDns.User.RegisterUser();
				Simias.mDns.User.StartMemberBrowsing();

				// Temp
				Simias.mDns.Sync.StartSyncThread();

				// Register with the DomainProvider service
				this.mDnsProvider = new Simias.mDnsProvider();
				Simias.DomainProvider.RegisterProvider( this.mDnsProvider );

				// Temp
				this.locProvider = new Simias.Location.mDnsProvider( this.mDnsProvider );
				Simias.Location.Locate.RegisterProvider( this.locProvider );

				// Register for authentication events
				Simias.Authentication.NeedCredentialsEventSubscriber needCreds =
					new NeedCredentialsEventSubscriber();
				needCreds.NeedCredentials += 
					new Simias.Authentication.NeedCredentialsEventHandler( OnCredentialsEventHandler );

				// Last add some fake credentials for the mDns domain
				/*
				string mDnsTagAndPassword = "@ppk@" + "blah";
				new NetCredential( "iFolder", Simias.mDns.Domain.ID, true, mDnsUser.ID, mDnsTagAndPassword );
				*/
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
		public void Custom(int message, string data)
		{
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
				Simias.Location.Locate.Unregister( this.locProvider );
			}

			if ( this.mDnsUser != null )
			{
				Simias.mDns.User.StopMemberBrowsing();
				Simias.mDns.User.UnregisterUser();
			}

			// Temp
			Simias.mDns.Sync.StopSyncThread();

			//Channel.UnregisterChannel();
			//Responder.Shutdown();
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
				lock ( Simias.mDns.Service.inCredentialEvent )
				{
					// Attempt to authenticate
					//Simias.Storage.Collection collection =
					//	Store.GetStore().GetCollectionByID( args.CollectionID );
					//if ( collection != null )
					//{
						Simias.mDns.ClientAuthentication clientAuth =
							new Simias.mDns.ClientAuthentication();

						if ( clientAuth.Authenticate( args.CollectionID ) == true )
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
					//}
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
		static bool exiting;
		static bool syncOnStart = true;
		static int syncInterval = 30 * 1000;
		static Thread syncThread = null;

		internal static Simias.mDns.User mdnsUser = null;

		internal static int StartSyncThread()
		{
			int status = 0;

			try
			{
				mdnsUser = new Simias.mDns.User();
				exiting = false;
				syncEvent = new AutoResetEvent(false);
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
			exiting = true;
			try
			{
				syncEvent.Set();
				Thread.Sleep(32);
				syncEvent.Close();
				Thread.Sleep(0);
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
			while (!exiting)
			{
				if (syncOnStart == false)
				{
					syncEvent.WaitOne(syncInterval, false);
				}

				// Always wait after the first iteration
				syncOnStart = false;
				mdnsUser.SynchronizeMembers();
			}
		}
	}

}
