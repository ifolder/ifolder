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
 *		Brady Anderson <banderso@novell.com>
 *		(this code is a mostly copy-n-paste from SimpleServer code, which
 *		 Brady wrote)
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

			Simias.Gaim.Sync.StartSyncThread();
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
			Simias.Gaim.Sync.StopSyncThread();
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
		static int syncInterval = 60 * 1000;
		static Thread syncThread = null;

		internal static Simias.Gaim.GaimDomain gaimDomain = null;
		//internal static DateTime lastSyncTime;

		internal static int StartSyncThread()
		{
			log.Debug("StartSyncThread called");

			try
			{
				gaimDomain = new Simias.Gaim.GaimDomain(true);
			}
			catch{}

			if ( gaimDomain != null )
			{
				exiting = false;
				syncEvent = new AutoResetEvent(false);
				syncThread = new Thread( new ThreadStart( Sync.SyncThread ) );
				syncThread.IsBackground = true;
				syncThread.Start();
			}
			else
			{
				log.Debug("Failed to initialize the Gaim domain");
			}

			log.Debug("StartSyncThread finished");
			return(0);
		}

		internal static int StopSyncThread()
		{
			log.Debug("StopSyncThread called");
			exiting = true;
			try
			{
				syncEvent.Set();
				Thread.Sleep(32);
				syncEvent.Close();
				Thread.Sleep(0);
				gaimDomain = null;
				log.Debug("StopSyncThread finished");
				return(0);
			}
			catch(Exception e)
			{
				log.Debug("StopSyncThread failed with an exception");
				log.Debug(e.Message);
			}
			return(-1);
		}

		public static int SyncNow(string data)
		{
			log.Debug("SyncNow called");
			syncEvent.Set();
			log.Debug("SyncNow finished");
			return(0);
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
				gaimDomain.SynchronizeMembers();
			}
		}
	}
}
