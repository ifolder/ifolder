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
	public class SyncManagerService : IThreadService
	{
		private Configuration config;
		private SyncProperties properties;
		private SyncManager manager;

		public SyncManagerService()
		{
			config = null;
			properties = null;
			manager = null;
		}

		#region IThreadService Members

		public void Start(Configuration config)
		{
			Debug.Assert(config != null);

			this.config = config;

			properties = new SyncProperties(config);
			manager = new SyncManager(properties);

			manager.Start();
		}

		public void Resume()
		{
			Debug.Assert(manager != null);

			manager.Start();
		}

		public void Pause()
		{
			Debug.Assert(manager != null);

			manager.Stop();
		}

		public void Custom(int message, string data)
		{
		}

		public void Stop()
		{
			Debug.Assert(manager != null);

			manager.Stop();
		}

		#endregion
	}
}
