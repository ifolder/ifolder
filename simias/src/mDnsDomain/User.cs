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
//using Simias.Client;
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

		private static bool	mDnsChannelUp = false;
		private string  mDnsUserName;
		private string  mDnsUserID = "";
		private static IResourceRegistration rr = null;
		private static IResourceQuery query = null;
		private readonly string memberTag = "_ifolder_member._tcp.local";
		private	IMDnsEvent cEvent = null;


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
		/// Static constructor for mDns
		/// </summary>
		static User()
		{
			//
			// Setup the remoting channel to the mDnsResponder
			// this isn't thread safe but I know it won't be
			// re-entered
			//

			IRemoteFactory factory = 
				(IRemoteFactory) Activator.GetObject(
					typeof(IRemoteFactory),
					"tcp://localhost:8091/mDnsRemoteFactory.tcp");
				
			Simias.mDns.User.rr = factory.GetRegistrationInstance();
			Simias.mDns.User.query = factory.GetQueryInstance();

			mDnsChannelUp = true;
		}

		/// <summary>
		/// Constructor for newing up an mDns user object.
		/// </summary>
		internal User()
		{
			Simias.mDns.Domain mdnsDomain = new Simias.mDns.Domain( true );
			mDnsUserName = Environment.UserName + "@" + mdnsDomain.Host;
		}
		#endregion


		#region Internal Methods

		internal void BroadcastUp()
		{
			bool registered = false;

			if ( mDnsChannelUp == false )
			{
				throw new SimiasException( "Remoting channel not setup" );
			}

			//
			// Register the user and as an iFolder member with
			// the mDnsResponder
			//
			try
			{
				// Temporary to auto add all ifolder_members
				this.AutoMembers();

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
						status = rr.RegisterPointer( memberTag, this.mDnsUserName );
						if ( status == 0 )
						{
							registered = this.RegisterTextStrings();
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
			}

			if ( Simias.mDns.User.rr != null )
			{
				Simias.mDns.Domain mdnsDomain = new Simias.mDns.Domain( false );
				rr.DeregisterPointer( memberTag, this.mDnsUserName );
				rr.DeregisterServiceLocation( mdnsDomain.Host, this.mDnsUserName );
				rr.DeregisterHost( mdnsDomain.Host );
			}
		}

		#endregion

		#region Private Methods
		private bool RegisterTextStrings()
		{
			bool status = false;

			Simias.Storage.Domain mdnsDomain = Store.GetStore().GetDomain( Simias.mDns.Domain.ID );
			if ( mdnsDomain != null )
			{
				//Roster roster = mdnsDomain.Roster;
				Member member = mdnsDomain.Roster.GetMemberByName( mDnsUserName );
				string[] txtStrings = new string[1];
				string memberTxtString = "MemberID=" + member.ID;
				txtStrings[0] = memberTxtString;

				// Register the 
				if ( rr.RegisterTextStrings( mDnsUserName, txtStrings ) == 0 )
				{
					status = true;
				}
				else
				{
					log.Debug( "Failed registering TXT strings" );
				}
			}
			else
			{
				log.Debug( "Failed to get the mDns domain" );
			}

			return status;
		}
		#endregion

		#region Public Methods

		/// <summary>
		/// Event handler for capturing new _ifolder_members
		/// </summary>
		public
		static
		void 
		OnMDnsEvent(mDnsEventAction action, mDnsType rType, BaseResource cResource)
		{

			/*
			if (rType == mDnsType.textStrings)
			{
				foreach(string s in ((TextStrings) cResource).GetTextStrings())
				{
					Console.WriteLine("TXT:           " + s);
				}
			}
			else
			if (rType == mDnsType.hostAddress)
			{
				Console.WriteLine(String.Format("Address:       {0}", ((HostAddress) cResource).PrefAddress));
			}
			else
			if (rType == mDnsType.serviceLocation)
			{
				Console.WriteLine(String.Format("Host:          {0}", ((ServiceLocation) cResource).Target));
				Console.WriteLine(String.Format("Port:          {0}", ((ServiceLocation) cResource).Port));
				Console.WriteLine(String.Format("Priority:      {0}", ((ServiceLocation) cResource).Priority));
				Console.WriteLine(String.Format("Weight:        {0}", ((ServiceLocation) cResource).Weight));
			}
			else
			*/
			if ( rType == mDnsType.ptr && action == Mono.P2p.mDnsResponderApi.mDnsEventAction.added )
			{ 
				log.Debug("");
				log.Debug("Event Type:    {0}", action);
				log.Debug("Resource Type: {0}", rType);
				log.Debug("Resource Name: " + cResource.Name);
				log.Debug("Resource ID:   " + cResource.ID);
				log.Debug("TTL:           " + cResource.Ttl);
				log.Debug(String.Format("Target:        {0}", ((Ptr) cResource).Target));

				if ( cResource.Name == "_ifolder_member._tcp.local" )
				{
					try
					{
						Store store = Store.GetStore();
						//Simias.mDns.Domain mdnsDomain = new Simias.mDns.Domain( false );
						Simias.Storage.Domain rDomain = store.GetDomain( Simias.mDns.Domain.ID );
						Simias.Storage.Roster mdnsRoster = rDomain.Roster;

						Member mdnsMember = mdnsRoster.GetMemberByName( ((Ptr) cResource).Target );
						if ( mdnsMember != null )
						{
							// Update IP Address?
						}
						else
						{
							log.Debug(String.Format("Adding member: {0} to the Rendezvous Roster", ((Ptr) cResource).Target));

							// FIXME::Need to query mDns to get the address where the user is located
							// and to get his ID from the TTL resource

							Mono.P2p.mDnsResponderApi.TextStrings txtStrings = null;
							string memberID = null;

							if ( query.GetTextStringsByName( ((Ptr) cResource).Target, ref txtStrings ) == 0 )
							{
								foreach( string s in txtStrings.GetTextStrings() )
								{
									string[] nameValues = s.Split( new char['='], 2 );
									if ( nameValues[0] == "MemberID" )
									{
										memberID = nameValues[1];
										break;
									}

								}

								if ( memberID != null )
								{
									mdnsMember = 
										new Member( ((Ptr) cResource).Target, memberID, Access.Rights.ReadOnly );

									mdnsRoster.Commit( new Node[] { mdnsMember } );
								}
							}
						}
					}
					catch(Exception e)
					{
						log.Error( e.Message );
						log.Error( e.StackTrace );
					}
				}
			}
		}

		/// <summary>
		/// FIXME::Temporary method to automatically synchronize all mDns users
		/// </summary>
		/// <returns>n/a</returns>
		public void SynchronizeMembers()
		{
			//
			// Get the mDns roster
			//

			Simias.Storage.Member mdnsMember = null;
			Store store = Store.GetStore();
			Simias.Storage.Domain rDomain = store.GetDomain( Simias.mDns.Domain.ID );
			Simias.Storage.Roster mdnsRoster = rDomain.Roster;

			Char[] sepChar = new Char [] {'='};

			//
			// next get all the ifolder-members from mDnsResponder
			//

			Mono.P2p.mDnsResponderApi.Ptr[] ifolderMembers;
			if ( query.GetPtrResourcesByName( memberTag, out ifolderMembers ) == 0 )
			{
				foreach( Mono.P2p.mDnsResponderApi.Ptr member in ifolderMembers )
				{
					Mono.P2p.mDnsResponderApi.TextStrings txtStrings = null;
					string memberID = null;

					if ( query.GetTextStringsByName( member.Target, ref txtStrings ) == 0 )
					{
						foreach( string s in txtStrings.GetTextStrings() )
						{
							string[] nameValues = s.Split( sepChar );
							if ( nameValues[0] == "MemberID" )
							{
								memberID = nameValues[1];
								break;
							}

						}

						if ( memberID != null )
						{
							mdnsMember = mdnsRoster.GetMemberByID( memberID );
							if ( mdnsMember == null )
							{
								mdnsMember = 
									new Member( member.Target, memberID, Access.Rights.ReadOnly );

								mdnsRoster.Commit( new Node[] { mdnsMember } );
							}
							else
							{
								// Update other info
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Obtains the string representation of this instance.
		/// </summary>
		/// <returns>The friendly name of the domain.</returns>
		public override string ToString()
		{
			return this.mDnsUserName;
		}
		#endregion

		#region Private Methods
		// This is a temporary thing but for now
		// we'll add any ifolder member we see to our member list
		private void AutoMembers()
		{
			this.cEvent = 
				(IMDnsEvent) Activator.GetObject(
				typeof(Mono.P2p.mDnsResponderApi.IMDnsEvent),
				"tcp://localhost:8091/IMDnsEvent.tcp");

			mDnsEventWrapper eventWrapper = new mDnsEventWrapper();
			eventWrapper.OnLocalEvent += new mDnsEventHandler(OnMDnsEvent);
			mDnsEventHandler evntHandler = new mDnsEventHandler(eventWrapper.LocalOnEvent);
			cEvent.OnEvent += evntHandler;
		}
		#endregion
	}


}
