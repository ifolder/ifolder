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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.Diagnostics;

using Simias.Service;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Manager Service
	/// </summary>
	public class SyncManagerService : BaseProcessService
	{
		private static SyncManagerService process;
		
		private Configuration config;
		private SyncManager manager;
		private FileMonitorService fileMonitor;

		/// <summary>
		/// Constructor
		/// </summary>
		public SyncManagerService() : base()
		{
			this.config = GetConfiguration();
			this.manager = new SyncManager(config);
			this.fileMonitor = new FileMonitorService();
		}

		static void Main(string[] args)
		{
			process = new SyncManagerService();
			process.Run();
		}

		#region BaseProcessService Members

		/// <summary>
		/// Start the sync manager service.
		/// </summary>
		protected override void Start()
		{
			Debug.Assert(manager != null);

			manager.Start();
			fileMonitor.Start(GetConfiguration());
		}

		/// <summary>
		/// Stop the sync manager service.
		/// </summary>
		protected override void Stop()
		{
			Debug.Assert(manager != null);

			manager.Stop();
			fileMonitor.Stop();
		}

		/// <summary>
		/// Resume the sync manager service.
		/// </summary>
		protected override void Resume()
		{
			Debug.Assert(manager != null);

			manager.Start();
			fileMonitor.Resume();
		}

		/// <summary>
		/// Pause the sync manager service.
		/// </summary>
		protected override void Pause()
		{
			Debug.Assert(manager != null);

			manager.Stop();
			fileMonitor.Pause();
		}

		/// <summary>
		/// A custom event for the sync manager service.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		protected override void Custom(int message, string data)
		{
			SyncMessages msg = (SyncMessages)message;

			switch(msg)
			{
				case SyncMessages.SyncCollectionNow:
					manager.SyncCollectionNow(data);
					break;
	
				case SyncMessages.SyncAllNow:
					manager.SyncAllNow();
					break;
	
				default:
					break;
			}
		}

		#endregion
	}
}
