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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections;
using System.Text.RegularExpressions;

using Mono.P2p.mDnsResponderApi;

using Simias.Storage;
using Simias.Sync;
using Simias.POBox;

namespace Simias.Location
{
	/// <summary>
	/// Class to support Simias' plugin Location Provider interface
	/// </summary>
	public class mDnsProvider : ILocationProvider
	{
		#region Class Members
		private string providerName = "Rendezvous Domain Provider";
		private string description = "Simias Location provider which uses the mDns protocol to resolve objects";
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

		//private static IResourceRegistration rr = null;

		//public static readonly string ID = "74d3a71f-daae-4a36-b9f3-6466081f6401";
		private IResourceQuery mDnsQuery = null;
		private IRemoteFactory mDnsFactory = null;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the name of the location provider.
		/// </summary>
		public string Name
		{
			get { return( providerName ); }
		}

		/// <summary>
		/// Gets the description of the location provider.
		/// </summary>
		public string Description
		{
			get { return( description ); }
		}
		#endregion

		#region Constructors

		/*
		static mDnsProvider()
		{
			log.Debug( "Static constructor called" );

			try
			{
				IRemoteFactory factory = 
					(IRemoteFactory) Activator.GetObject(
					typeof(IRemoteFactory),
					"tcp://localhost:8091/mDnsRemoteFactory.tcp");
					
				mDnsQuery = factory.GetQueryInstance();
			}
			catch( Exception e )
			{
				log.Debug( e.Message );
				log.Debug( e.StackTrace );
			}
		}
		*/

		public mDnsProvider()
		{
			log.Debug( "Instance constructor called" );
			try
			{
				mDnsFactory = 
					(IRemoteFactory) Activator.GetObject(
					typeof(IRemoteFactory),
					"tcp://localhost:8091/mDnsRemoteFactory.tcp");
					
				mDnsQuery = mDnsFactory.GetQueryInstance();
			}
			catch( Exception e )
			{
				log.Debug( e.Message );
				log.Debug( e.StackTrace );
			}
		}

		#endregion

		private Uri MemberIDToUri( string memberID )
		{
			//Mono.P2p.mDnsResponderApi.ServiceLocation sl = null;
			//Mono.P2p.mDnsResponderApi.HostAddress ha = null;
			Uri locationUri = null;
			string webServicePath = null;
			Char[] sepChar = new Char [] {'='};

			IResourceQuery mQuery = mDnsFactory.GetQueryInstance();

			Mono.P2p.mDnsResponderApi.ServiceLocation[] sls = null;
			if ( mQuery.GetServiceLocationResources( out sls ) == 0 )
			{
				foreach(ServiceLocation sl in sls)
				{
					if ( sl.Name == memberID )
					{
						Mono.P2p.mDnsResponderApi.TextStrings[] txts = null;
						if ( mQuery.GetTextStringResources( out txts ) == 0 )
						{
							foreach( TextStrings ts in txts )
							{
								if ( ts.Name == memberID )
								{
									foreach( string s in ts.GetTextStrings() )
									{
										string[] nameValues = s.Split( sepChar );
										if ( nameValues[0] == "ServicePath" )
										{
											webServicePath = nameValues[1];
											break;
										}
									}
									break;
								}
							}
						}

						if ( webServicePath != null )
						{
							Mono.P2p.mDnsResponderApi.HostAddress[] has = null;
							if ( mQuery.GetHostAddressResources( out has ) == 0 )
							{
								foreach( HostAddress ha in has )
								{
									if ( ha.Name == sl.Target )
									{
										ArrayList ipAddrs = ha.GetIPAddresses();
										if ( ipAddrs.Count > 0 )
										{
											string fullPath = 
													"http://" + 
													ipAddrs[0].ToString() + 
													":" + 
													System.Convert.ToString( (ushort) sl.Port ) +
													webServicePath;

											log.Debug( "fullPath: " + fullPath );
											locationUri = new Uri( fullPath );
										}
										break;
									}
								}
							}
						}

						break;
					}
				}
			}

			return locationUri;
		}

		#region ILocationProvider Members

		/// <summary>
		/// Indicates to the provider that the specified collection has
		/// been deleted and a mapping is no longer required.
		/// </summary>
		/// <param name="domainID">The identifier for the domain from
		/// where the collection has been deleted.</param>
		/// <param name="collectionID">Identifier of the collection that
		/// is being deleted.</param>
		public void DeleteLocation( string domainID, string collectionID )
		{
			log.Debug( "DeleteLocation called" );
			
			// mDnsProvider does not keep location state
			// so wave it on through

			return;
		}

		/// <summary>
		/// Determines if the provider claims ownership for the 
		/// specified domain.
		/// </summary>
		/// <param name="domainID">Identifier of a domain.</param>
		/// <returns>True if the provider claims ownership for the 
		/// specified domain. Otherwise, False is returned.</returns>
		public bool OwnsDomain( string domainID )
		{
			log.Debug( "OwnsDomain called" );
			return ( domainID.ToLower() == Simias.mDns.Domain.ID ) ? true : false;
		}

		/// <summary>
		/// Returns the network location for the the specified
		/// collection.
		/// </summary>
		/// <param name="domainID">Identifier for the domain that the
		/// collection belongs to.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolveLocation( string domainID )
		{
			log.Debug( "ResolveLocation called" );

			Uri locationUri = null;
			if( domainID.ToLower() == Simias.mDns.Domain.ID )
			{
				Member member = Store.GetStore().GetDomain( domainID ).GetCurrentMember();
				locationUri = MemberIDToUri( member.UserID );
			}

			return locationUri;
		}


		/// <summary>
		/// Returns the network location for the the specified
		/// collection.
		/// </summary>
		/// <param name="domainID">Identifier for the domain that the
		/// collection belongs to.</param>
		/// <param name="collectionID">Identifier of the collection to
		/// find the network location for.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolveLocation( string domainID, string collectionID )
		{
			log.Debug( "ResolveLocation called" );
			log.Debug( "  DomainID: " + domainID );
			log.Debug( "  CollectionID: " + collectionID );

			Uri locationUri = null;
			if( domainID.ToLower() == Simias.mDns.Domain.ID )
			{
				try
				{
					Store store = Store.GetStore();
					Collection col = store.GetCollectionByID( collectionID );

					Simias.POBox.POBox poBox = 
						Simias.POBox.POBox.FindPOBox( store, domainID, col.GetCurrentMember().UserID );

					Subscription sub = poBox.GetSubscriptionByCollectionID( collectionID );
					locationUri = MemberIDToUri( sub.FromIdentity );
				}
				catch ( Exception e )
				{
					log.Debug( e.Message );
					log.Debug( e.StackTrace );
				}
			}

			return locationUri;
		}

		/// <summary>
		/// Returns the network location of where to create a collection.
		/// </summary>
		/// <param name="domainID">Identifier of the domain where a 
		/// collection is to be created.</param>
		/// <param name="userID">The member that will own the 
		/// collection.</param>
		/// <param name="collectionID">Identifier of the collection that
		/// is being created.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolveLocation( string domainID, string userID, string collectionID )
		{
			log.Debug( "ResolveLocation with userID called" );

			Uri locationUri = null;
			if( domainID.ToLower() == Simias.mDns.Domain.ID )
			{
				try
				{
					Collection collection = Store.GetStore().GetCollectionByID( collectionID );
					Member member = collection.GetMemberByID( userID );
					if ( member != null )
					{
						locationUri = MemberIDToUri( member.UserID );
					}
				}
				catch ( Exception e )
				{
					log.Debug( e.Message );
					log.Debug( e.StackTrace );
				}
			}

			return locationUri;
		}

		/// <summary>
		/// Returns the network location of where to the POBox is located.
		/// </summary>
		/// <param name="domainID">Identifier of the domain where a 
		/// collection is to be created.</param>
		/// <param name="userID">The member that will owns the POBox.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolvePOBoxLocation( string domainID, string userID )
		{
			log.Debug( "ResolveLocation with userID called" );

			Uri locationUri = null;
			if( domainID.ToLower() == Simias.mDns.Domain.ID )
			{
				try
				{
					Simias.Storage.Domain domain = Store.GetStore().GetDomain( Simias.mDns.Domain.ID );
					Member member = domain.GetMemberByID( userID );
					if ( member != null )
					{
						locationUri = MemberIDToUri( member.UserID );
					}
				}
				catch ( Exception e )
				{
					log.Debug( e.Message );
					log.Debug( e.StackTrace );
				}
			}

			return locationUri;
		}

		#endregion
	}
}
