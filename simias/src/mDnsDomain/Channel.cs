/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2005 Novell, Inc.
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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using System.Xml;

using Simias;
using Simias.Storage;

namespace Simias.mDns
{
	/// <summary>
	/// Class used to register and unregister the .NET remoting channel
	/// </summary>
	public class Channel
	{
		#region Class Members
		private static TcpChannel tcpChannel = null;

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#endregion

		#region Constructors
		#endregion

		#region Internal Methods

		/// <summary>
		/// Static method to register the TCP remoting channel
		/// </summary>
		internal static bool RegisterChannel()
		{
			if ( Channel.tcpChannel == null )
			{
				try
				{
					Hashtable propsTcp = new Hashtable();
					propsTcp[ "port" ] = 0;
					propsTcp[ "rejectRemoteRequests" ] = true;

					BinaryServerFormatterSinkProvider
						serverBinaryProvider = new BinaryServerFormatterSinkProvider();

					BinaryClientFormatterSinkProvider
						clientBinaryProvider = new BinaryClientFormatterSinkProvider();
#if !MONO
					serverBinaryProvider.TypeFilterLevel =
						System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
#endif
					Channel.tcpChannel = 
						new TcpChannel( propsTcp, clientBinaryProvider, serverBinaryProvider );
					ChannelServices.RegisterChannel( Channel.tcpChannel );

					/*
					IRemoteFactory factory = 
						(IRemoteFactory) Activator.GetObject(
						typeof(IRemoteFactory),
						"tcp://localhost:8091/mDnsRemoteFactory.tcp");
					*/	
				}
				catch( Exception e )
				{
					log.Error( e.Message );
					log.Error( e.StackTrace );
				}
			}

			return ( Channel.tcpChannel != null ) ? true : false;
		}

		/// <summary>
		/// Static method to unregister the TCP remoting channel
		/// </summary>
		internal static void UnregisterChannel()
		{
			if ( Channel.tcpChannel != null )
			{
				ChannelServices.UnregisterChannel( Channel.tcpChannel );
			}
		}

		#endregion
	}
}
