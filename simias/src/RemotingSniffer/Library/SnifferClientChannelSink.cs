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
using System.Collections;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Text;

using Simias;

namespace Simias.Sync
{
	/// <summary>
	/// Sniffer Client Channel Sink
	/// </summary>
	public class SnifferClientChannelSink : IClientChannelSink
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SnifferClientChannelSink));

		private IClientChannelSink nextSink;

		public SnifferClientChannelSink(IClientChannelSink nextSink)
		{
			this.nextSink = nextSink;
		}
		
		#region IClientChannelSink Members

		void IClientChannelSink.ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			log.Debug("****** SNIFFER PROCESS [CLIENT] MESSAGE START ******");

			log.Debug("Initiating Remote Call...{0}{1}", Environment.NewLine, SnifferMessage.ToString(msg));

			try
			{
				nextSink.ProcessMessage(msg, requestHeaders, requestStream, out responseHeaders, out responseStream);
			}
			catch(Exception e)
			{
				log.Error(e, "Client Sink Process Exception");

				throw e;
			}
			finally
			{
				log.Debug("******* SNIFFER PROCESS [CLIENT] MESSAGE END *******");
			}
		}

		IClientChannelSink IClientChannelSink.NextChannelSink
		{
			get { return nextSink; }
		}

		public Stream GetRequestStream(IMessage msg, ITransportHeaders headers)
		{
			return null;
		}

		public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage msg, ITransportHeaders headers, Stream stream)
		{
		}

		void IClientChannelSink.AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders headers, Stream stream)
		{
		}

		#endregion

		#region IChannelSinkBase Members

		public IDictionary Properties
		{
			get { return null; }
		}

		#endregion
	}
}
