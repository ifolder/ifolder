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
using System.Text;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;

namespace Simias.Channels
{
	/// <summary>
	/// Simias Channel
	/// </summary>
	public class SimiasChannel : IDisposable
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SimiasChannel));

		private IChannel channel;

		internal SimiasChannel(IChannel channel)
		{
			this.channel = channel;
			
			// register channel
			ChannelServices.RegisterChannel(channel);

			log.Debug("Channel Created: {0}", this);
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
	
			builder.Append(channel.ChannelName);

			IDictionary properties = null;

			// find properties
			if (typeof(BaseChannelWithProperties).IsInstanceOfType(channel))
			{
				BaseChannelWithProperties c = (BaseChannelWithProperties) channel;
				properties = c.Properties;
			}
			
			if (properties != null)
			{
				foreach(DictionaryEntry entry in properties)
				{
					builder.AppendFormat("{2}{0,20}: {1}", entry.Key,
						entry.Value, Environment.NewLine);
				}
			}

			return builder.ToString();
		}

		#region IDisposable Members

		/// <summary>
		/// Dispose the channel
		/// </summary>
		public void Dispose()
		{
			lock(this)
			{
				if (channel != null)
				{
					ChannelServices.UnregisterChannel(channel);

					log.Debug("Channel Closed: {0}", this);

					channel = null;
				}
			}
		}

		#endregion

		#region Properties
		
		/// <summary>
		/// The channel object
		/// </summary>
		public IChannel Channel
		{
			get { return channel; }
		}
		
		/// <summary>
		/// The channel name
		/// </summary>
		public string Name
		{
			get { return channel.ChannelName; }
		}
		
		#endregion
	}
}
