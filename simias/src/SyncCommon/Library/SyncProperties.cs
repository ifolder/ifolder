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
using System.IO;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Properties
	/// </summary>
	/// <remarks>The clone() method must be updated and verified
	/// with any changes to this objects fields.
	/// </remarks>
	[Serializable]
	public class SyncProperties : ICloneable
	{
		/// <summary>
		/// The suggested sync host or ip address for the current machine.
		/// </summary>
		public static readonly string SuggestedHost = MyDns.GetHostName();

		/// <summary>
		/// The suggested collection store path for the current machine.
		/// </summary>
		public static readonly string SuggestedStorePath = null;

		/// <summary>
		/// The suggested sync port for the current machine.
		/// </summary>
		public static readonly int SuggestedPort = 6436;

		/// <summary>
		/// The suggested sync logic factory for syncing.
		/// </summary>
		public static readonly Type SuggestedLogicFactory = typeof(SyncLogicFactory);
		
		/// <summary>
		/// The suggested sync interval in seconds.
		/// </summary>
		public static readonly int SuggestedSyncInterval = 5;

		/// <summary>
		/// The suggested sync channel sinks.
		/// </summary>
		public static readonly SyncChannelSinks SuggestedChannelSinks =
#if DEBUG
			SyncChannelSinks.Binary | SyncChannelSinks.Monitor | SyncChannelSinks.Security;
#else
			SyncChannelSinks.Binary | SyncChannelSinks.Security;
#endif

		// properties
		private string host	= SuggestedHost;
		private string path	= SuggestedStorePath;
		private int port = SuggestedPort;
		private Type logicFactory = SuggestedLogicFactory;
		private int interval = SuggestedSyncInterval;
		private SyncChannelSinks sinks = SuggestedChannelSinks;

		/// <summary>
		/// Constructor
		/// </summary>
		public SyncProperties()
		{
		}

		#region ICloneable Members

		/// <summary>
		/// Create a copy of the sync properties object.
		/// </summary>
		/// <returns>A copy of the sync properties object.</returns>
		public object Clone()
		{
			// note: watch carefully!
			SyncProperties clone = new SyncProperties();
			
			clone.host = host;
			clone.path = path;
			clone.port = port;
			clone.logicFactory = logicFactory;
			clone.interval = interval;
			clone.sinks = sinks;

			return clone;
		}

		#endregion

		#region Properties
		
		/// <summary>
		/// The default sync host.
		/// </summary>
		public string DefaultHost
		{
			get { return host; }
			set { host = value; }
		}

		/// <summary>
		/// The collection store path.
		/// </summary>
		public string StorePath
		{
			get { return path; }
			set
			{
				if (value == null)
				{
					path = null;
				}
				else
				{
					path = Path.GetFullPath(value);
				}
			}
		}

		/// <summary>
		/// The default sync port.
		/// </summary>
		public int DefaultPort
		{
			get { return port; }
			set { port = value; }
		}

		/// <summary>
		/// The default sync interval in seconds.
		/// </summary>
		public int DefaultSyncInterval
		{
			get { return interval; }
			set { interval = value; }
		}

		/// <summary>
		/// The default sync logic factory.
		/// </summary>
		public Type DefaultLogicFactory
		{
			get { return logicFactory; }
			set { logicFactory = value; }
		}

		/// <summary>
		/// The default channel sinks.
		/// </summary>
		public SyncChannelSinks DefaultChannelSinks
		{
			get { return sinks; }
			set { sinks = value; }
		}

		#endregion
	}
}
