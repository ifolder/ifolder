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
 *  Author(s):
 *		Boyd Timothy <btimothy@novell.com>
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
using Simias.Event;
using Simias.POBox;
using Simias.Service;
using Simias.Storage;

using Novell.Security.ClientPasswordManager;

namespace Simias.Gaim
{
	/// <summary>
	/// Class the handles presence as a service
	/// </summary>
	public class GaimService : IThreadService
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static string inCredentialEvent = "true";
		private Store store = null;
//		private static Simias.Location.GaimProvider gaimProvider = null;
		private static Simias.Gaim.GaimDomainProvider gaimDomainProvider = null;

		/// <summary>
		/// Configuration object for the Collection Store.
		/// </summary>
		private Configuration config;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		public GaimService()
		{
		}
		#endregion

		#region Internal Methods
//		internal static void RegisterLocationProvider()
//		{
//			log.Debug("RegisterLocationProvider called");
//			
//			gaimProvider = new Simias.Location.GaimProvider();
//			
//			Simias.Location.Locate.RegisterProvider(gaimProvider);
//			
//			// Fake some credentials for the domain
//			new NetCredential("iFolder", Simias.Gaim.GaimDomain.ID, true, "gaim-user", "joulupukki");
//		}

		internal static void RegisterDomainProvider()
		{
			log.Debug("RegisterDomainProvider called");

			gaimDomainProvider = new Simias.Gaim.GaimDomainProvider();

			Simias.DomainProvider.RegisterProvider(gaimDomainProvider);

			// Fake some credentials for the domain
			new NetCredential("iFolder", Simias.Gaim.GaimDomain.ID, true, "gaim-user", "joulupukki");
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

			store = Store.GetStore();

			//
			// Start the SyncThread
			//
			try
			{
				Simias.Storage.Domain domain = GaimDomain.GetDomain();
				if (domain != null)
				{
//					RegisterLocationProvider();
					RegisterDomainProvider();
				}
				
				Simias.Gaim.Sync.StartSyncThread();
				
				// Register for authentication events
				Simias.Authentication.NeedCredentialsEventSubscriber needCreds =
					new NeedCredentialsEventSubscriber();
				needCreds.NeedCredentials +=
					new Simias.Authentication.NeedCredentialsEventHandler(OnCredentialsEventHandler);
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
			if (gaimDomainProvider != null)
			{
//				Simias.Location.Locate.Unregister(gaimProvider);
				Simias.DomainProvider.Unregister(gaimDomainProvider);
			}
			Simias.Gaim.Sync.StopSyncThread();
		}
		
		/// <summary>
		/// Handler that's called for all NeedCredential events.
		/// If the collection is a slave, authenticate
		/// to the remote server.
		/// </summary>
		public static void OnCredentialsEventHandler( 
			Simias.Client.Event.NeedCredentialsEventArgs args)
		{
			if ( args.DomainID == Simias.Gaim.GaimDomain.ID )
			{
				Simias.Authentication.Status authStatus;
				lock (inCredentialEvent)
				{
					ClientAuthentication clientAuth =
						new ClientAuthentication();

					authStatus = clientAuth.Authenticate(args.CollectionID);
					if (authStatus.statusCode == Simias.Authentication.StatusCodes.Success)
					{
						string userID = Store.GetStore().GetUserIDFromDomainID(GaimDomain.ID);

						// Set credentials for this collection
						new NetCredential( 
							"iFolder", 
							args.CollectionID, 
							true, 
							userID,
							"@GaimDomainPPK@" );
					}
				}
			}
		}

		#endregion
	}

	/// <summary>
	/// Class for controlling the synchronization thread
	/// </summary>
	public class Sync
	{
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		static AutoResetEvent syncEvent = null;
		static bool exiting;
		static bool syncOnStart = true;
		static int syncInterval = 60 * 1000; // 1 minute
		static Thread syncThread = null;

//		internal static Simias.Gaim.GaimDomain gaimDomain = null;
		//internal static DateTime lastSyncTime;

		internal static int StartSyncThread()
		{
			log.Debug("StartSyncThread called");
			int status = 0;

			try
			{
				
				// Start the thread regardless of whether the domain exists.
				// The domain should be created in GaimDomain.SynchronizeMembers
				exiting = false;
				syncEvent = new AutoResetEvent(false);
				syncThread = new Thread(new ThreadStart(Sync.SyncThread));
				syncThread.IsBackground = true;
				syncThread.Start();
				
//				gaimDomain = new Simias.Gaim.GaimDomain(false);
//				if ( gaimDomain != null )
//				{
//					exiting = false;
//					syncEvent = new AutoResetEvent(false);
//					syncThread = new Thread( new ThreadStart( Sync.SyncThread ) );
//					syncThread.IsBackground = true;
//					syncThread.Start();
//				}
//				else
//				{
//					log.Debug("Failed to initialize the Gaim domain");
//				}
			}
			catch(SimiasException e)
			{
				log.Debug( e.Message );
				log.Debug( e.StackTrace );
				status = -1;
			}

			log.Debug("StartSyncThread finished");
			return status;
		}

		internal static int StopSyncThread()
		{
			int status = 0;
			log.Debug("StopSyncThread called");
			exiting = true;
			try
			{
				syncEvent.Set();
				Thread.Sleep(32);
				syncEvent.Close();
				Thread.Sleep(0);
//				gaimDomain = null;
				log.Debug("StopSyncThread finished");
			}
			catch(Exception e)
			{
				log.Debug("StopSyncThread failed with an exception");
				log.Debug(e.Message);
				status = -1;
			}

			return status;
		}

		public static int SyncNow(string data)
		{
			log.Debug("SyncNow called");
			syncEvent.Set();
			log.Debug("SyncNow finished");
			return(0);
		}
		
		public static void UpdateSyncInterval(int newSyncInterval)
		{
			int syncIntervalInMillis = newSyncInterval * 60 * 1000;
		
			if (syncIntervalInMillis != syncInterval)
			{
				syncInterval = syncIntervalInMillis;
			}
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
				GaimDomain.UpdatePreferences();
				GaimDomain.SynchronizeMembers();
			}
		}
	}
}
