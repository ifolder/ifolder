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
		private IServerChannelSink nextSink;

		public SnifferServerChannelSink(IServerChannelSink nextSink)
		{
			this.nextSink = nextSink;
		}
		
		#region IServerChannelSink Members

		public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			StringBuilder message = new StringBuilder();

			foreach(DictionaryEntry entry in requestHeaders)
			{
				message.AppendFormat("{0} = {1}, ", entry.Key, entry.Value);
			}

			MyTrace.WriteLine("Processing Message: {0}", message);

			ServerProcessing result;
			
			try
			{
				result = nextSink.ProcessMessage(sinkStack, requestMsg, requestHeaders, requestStream, out responseMsg, out responseHeaders, out responseStream );
			}
			catch(Exception e)
			{
				MyTrace.WriteLine(e);

				throw e;
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
