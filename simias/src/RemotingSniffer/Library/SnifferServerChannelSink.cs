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
	/// Sniffer Server Channel Sink
	/// </summary>
	public class SnifferServerChannelSink : IServerChannelSink
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SnifferServerChannelSink));

		private IServerChannelSink nextSink;

		public SnifferServerChannelSink(IServerChannelSink nextSink)
		{
			this.nextSink = nextSink;
		}
		
		#region IServerChannelSink Members

		public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			responseMsg = null;

			log.Debug("****** SNIFFER PROCESS [SERVER] MESSAGE START ******");

			ServerProcessing result;
			
			try
			{
				result = nextSink.ProcessMessage(sinkStack, requestMsg, requestHeaders, requestStream, out responseMsg, out responseHeaders, out responseStream );
			}
			catch(Exception e)
			{
				log.Error(e, "Server Sink Process Exception");

				throw e;
			}
			finally
			{
				log.Debug("Responding to Remote Call...{0}{1}", Environment.NewLine, SnifferMessage.ToString(responseMsg));
				log.Debug("******* SNIFFER PROCESS [SERVER] MESSAGE END *******");
			}

			return result;
		}

		public IServerChannelSink NextChannelSink
		{
			get { return nextSink; }
		}

		public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers)
		{
			return null;
		}

		public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers, Stream stream)
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
