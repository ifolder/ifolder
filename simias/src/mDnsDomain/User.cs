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
using Simias.Client;
using Simias.Storage;

using Mono.P2p.mDnsResponderApi;


namespace Simias.mDns
{
	/// <summary>
	/// Class used to broadcast the current user
	/// to the Rendezvous/mDns network
	/// </summary>
	public class User
	{
		#region Class Members

		private bool	mDnsChannelUp = false;
		private string  mDnsUserName;
		private string  mDnsUserID = "";
		private IResourceRegistration rr = null;
		//private	IMDnsEvent cEvent = null;


		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#endregion

		#region Properties

		/// <summary>
		/// Gets the current user's mDns ID
		/// </summary>
		public string ID
		{
			get { return( mDnsUserID ); }
		}

		/// <summary>
		/// Gets the mDnsDomain's friendly ID
		/// </summary>
		public string Name
		{
			get { return( this.mDnsUserID ); }
		}
		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for newing up an mDns user object.
		/// </summary>
		internal User()
		{
			Simias.mDns.Domain mdnsDomain = new Simias.mDns.Domain( true );
			mDnsUserName = Environment.UserName + "@" + mdnsDomain.Name;

			//
			// Setup the remoting channel to the mDnsResponder
			//

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
				TcpChannel tcpChnl = 
					new TcpChannel( propsTcp, clientBinaryProvider, serverBinaryProvider );
				ChannelServices.RegisterChannel( tcpChnl );

				IRemoteFactory factory = 
					(IRemoteFactory) Activator.GetObject(
					typeof(IRemoteFactory),
					"tcp://localhost:8091/mDnsRemoteFactory.tcp");
					
				this.rr = factory.GetRegistrationInstance();

				mDnsChannelUp = true;
			}
			catch( Exception e )
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );

				// rethrow the exception
			}
		}
		#endregion


		#region Internal Methods

		internal void BroadcastUp()
		{
			bool registered = false;

			if ( mDnsChannelUp == false )
			{
				throw new SimiasException( "Remoting channel not setup" );
				return;
			}

			//
			// Register the user and as an iFolder member with
			// the mDnsResponder
			//
			try
			{
				/*
					TcpChannel chnl = new TcpChannel();
					ChannelServices.RegisterChannel(chnl);
					*/

				//
				// Temp code automatically add new Rendezvous
				// _ifolder_members to my Rendezvous Workgroup Roster
				//

				/*

				this.cEvent = 
					(IMDnsEvent) Activator.GetObject(
					typeof(Mono.P2p.mDnsResponderApi.IMDnsEvent),
					"tcp://localhost:8091/IMDnsEvent.tcp");

				mDnsEventWrapper eventWrapper = new mDnsEventWrapper();
				eventWrapper.OnLocalEvent += new mDnsEventHandler(OnMDnsEvent);
				mDnsEventHandler evntHandler = new mDnsEventHandler(eventWrapper.LocalOnEvent);
				cEvent.OnEvent += evntHandler;
				*/

				Simias.mDns.Domain mdnsDomain = new Simias.mDns.Domain( true );
				int status = rr.RegisterHost( mdnsDomain.Host, MyDns.GetHostName() );
				if ( status == 0 )
				{
					// Register member as a service location
					status = 
						rr.RegisterServiceLocation(
						mdnsDomain.Host,
						this.mDnsUserName,
						(int) 8086,  // FIXME:: need to call Simias.config to get the port
						0, 
						0 );
					if ( status == 0 )
					{
						status = 
							rr.RegisterPointer(
							"_ifolder_member._tcp.local", 
							this.mDnsUserName );
						if ( status == 0 )
						{
							// Register my TXT strings
							registered = true;
						}
						else
						{
							rr.DeregisterServiceLocation( mdnsDomain.Host, this.mDnsUserName );
							rr.DeregisterHost( mdnsDomain.Host );
						}
					}
					else
					{
						rr.DeregisterHost( mdnsDomain.Host );
					}
				}
			}
			catch(Exception e2)
			{
				log.Error( e2.Message );
				log.Error( e2.StackTrace );
			}			
		}

		internal void BroadcastDown()
		{
			if ( mDnsChannelUp == false )
			{
				throw new SimiasException( "Remoting channel not setup" );
				return;
			}

			if ( this.rr != null )
			{
				Simias.mDns.Domain mdnsDomain = new Simias.mDns.Domain( false );
				rr.DeregisterPointer( "_ifolder_member._tcp.local", this.mDnsUserName );
				rr.DeregisterServiceLocation( mdnsDomain.Host, this.mDnsUserName );
				rr.DeregisterHost( mdnsDomain.Host );
			}
		}

		#endregion


		#region Public Methods
		/// <summary>
		/// Obtains the string representation of this instance.
		/// </summary>
		/// <returns>The friendly name of the domain.</returns>
		public override string ToString()
		{
			return this.mDnsUserName;
		}
		#endregion
	}
}
