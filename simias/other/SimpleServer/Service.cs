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
using Simias.Event;
using Simias.POBox;
using Simias.Service;
using Simias.Storage;


namespace Simias.SimpleServer
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

		private Simias.SimpleServer.Authentication authProvider = null;
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

			// Register with the domain provider service.
			this.authProvider = new Simias.SimpleServer.Authentication();
			DomainProvider.RegisterProvider( this.authProvider );
			Simias.SimpleServer.Sync.StartSyncThread();
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
			Simias.SimpleServer.Sync.StopSyncThread();
			DomainProvider.Unregister( this.authProvider );
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
		static bool quit;
		static bool syncOnStart = true;
		static int syncInterval = 60 * 1000;
		static Thread syncThread = null;

		internal static Simias.SimpleServer.Domain ssDomain = null;
		//internal static DateTime lastSyncTime;

		internal static int StartSyncThread()
		{
			log.Debug("StartSyncThread called");

			try
			{
				ssDomain = new Simias.SimpleServer.Domain( true );
			}
			catch{}

			if ( ssDomain != null )
			{
				quit = false;
				syncEvent = new AutoResetEvent( false );
				syncThread = new Thread( new ThreadStart( Sync.SyncThread ) );
				syncThread.IsBackground = true;
				syncThread.Start();
			}
			else
			{
				log.Debug( "Failed to initialize the SimpleServer domain" );
			}

			log.Debug( "StartSyncThread finished" );
			return 0;
		}

		internal static int StopSyncThread()
		{
			log.Debug( "StopSyncThread called" );
			quit = true;
			try
			{
				syncEvent.Set();
				Thread.Sleep( 32 );
				syncEvent.Close();
				Thread.Sleep( 0 );
				ssDomain = null;
				log.Debug( "StopSyncThread finished" );
				return 0;
			}
			catch(Exception e)
			{
				log.Debug( "StopSyncThread failed with an exception" );
				log.Debug( e.Message );
				log.Debug( e.StackTrace );
			}
			return -1;
		}

		/// <summary>
		/// </summary>
		public static int SyncNow( string data )
		{
			log.Debug( "SyncNow called" );
			syncEvent.Set();
			log.Debug( "SyncNow finished" );
			return 0;
		}

		internal static void SyncThread()
		{
			while ( quit == false )
			{
				if ( syncOnStart == false )
				{
					syncEvent.WaitOne( syncInterval, false );
					if ( quit == true )
					{
						return;
					}
				}

				// Always wait after the first iteration
				syncOnStart = false;
				ssDomain.SynchronizeMembers();
			}
		}
	}
}
