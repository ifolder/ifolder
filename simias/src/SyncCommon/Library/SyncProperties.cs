/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
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
		public static readonly string _DefaultHost = MyDns.GetHostName();
		public static readonly string _DefaultStorePath = null;
		public static readonly int _DefaultPort = 6436;
		public static readonly Type _DefaultLogicFactory = typeof(SyncLogicFactory);
		public static readonly int _DefaultSyncInterval = 5;
		public static readonly SyncChannelFormatters _DefaultChannelFormatter = SyncChannelFormatters.Binary;

		// properties
		private string host	= _DefaultHost;
		private string path	= _DefaultStorePath;
		private int port = _DefaultPort;
		private Type logicFactory = _DefaultLogicFactory;
		private int interval = _DefaultSyncInterval;
		private SyncChannelFormatters formatter = _DefaultChannelFormatter;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public SyncProperties()
		{
		}

		#region ICloneable Members

		public object Clone()
		{
			// wach carefully!
			SyncProperties clone = new SyncProperties();
			
			clone.host = host;
			clone.path = path;
			clone.port = port;
			clone.logicFactory = logicFactory;
			clone.interval = interval;
			clone.formatter = formatter;

			return clone;
		}

		#endregion

		#region Properties
		
		public string DefaultHost
		{
			get { return host; }
			set { host = value; }
		}

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

		public int DefaultPort
		{
			get { return port; }
			set { port = value; }
		}

		public int DefaultSyncInterval
		{
			get { return interval; }
			set { interval = value; }
		}

		public Type DefaultLogicFactory
		{
			get { return logicFactory; }
			set { logicFactory = value; }
		}

		public SyncChannelFormatters DefaultChannelFormatter
		{
			get { return formatter; }
			set { formatter = value; }
		}

		#endregion
	}
}
