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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Channel
	/// </summary>
	public class SyncChannel : IDisposable
	{
		private SyncChannelFactory factory;
		private IChannel channel;
		private string name;
		private int port;
		private SyncChannelSinks sinks;
		private int count;

		internal SyncChannel(SyncChannelFactory factory, IChannel channel,
			string name, int port, SyncChannelSinks sinks)
		{
			this.factory = factory;
			this.channel = channel;
			this.name = name;
			this.port = port;
			this.sinks = sinks;
			this.count = 0;
		}

		internal SyncChannel Open()
		{
			lock(this)
			{
				++count;
			}

			return this;
		}

		private void Close()
		{
			lock(this)
			{
				--count;
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			Close();

			lock(this)
			{
				if ((count <= 0) && (channel != null))
				{
					factory.ReleaseChannel(this);
					
					factory = null;
					channel = null;
				}
			}
		}

		#endregion

		#region Properties
		
		public IChannel Channel
		{
			get { return channel; }
		}
		
		public string Name
		{
			get { return name; }
		}
		
		public int Port
		{
			get { return port; }
		}

		public SyncChannelSinks Sinks
		{
			get { return sinks; }
		}
		
		#endregion
	}
}
