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
using System.Collections;
using System.Runtime.Remoting.Channels;

using Simias;

namespace Simias.Sniffer
{
	/// <summary>
	/// Sniffer Client Channel Sink Provider
	/// </summary>
	public class SnifferClientChannelSinkProvider : IClientChannelSinkProvider
	{
		private IClientChannelSinkProvider nextProvider;
		
		/// <summary>
		/// Constructor
		/// </summary>
		public SnifferClientChannelSinkProvider()
		{
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		public SnifferClientChannelSinkProvider(IDictionary properties, ICollection providerData)
		{
		}
		
		#region IClientChannelSinkProvider Members

		IClientChannelSink IClientChannelSinkProvider.CreateSink(IChannelSender channel, string url, object remoteChannelData)
		{
			IClientChannelSink nextSink = null;
			
			if (nextProvider != null)
			{
				nextSink = nextProvider.CreateSink(channel, url, remoteChannelData);
			}

			return new SnifferClientChannelSink(nextSink);
		}

		IClientChannelSinkProvider IClientChannelSinkProvider.Next
		{
			get { return nextProvider; }

			set { nextProvider = value; }
		}

		#endregion
	}
}
