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
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

using Simias;
using Simias.Authentication;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;

// shorty
using SCodes = Simias.Authentication.StatusCodes;

namespace Simias
{
	/// <summary>
	/// Rendezvous implementation of the DomainProvider Interface
	/// </summary>
	public class mDnsProvider : IDomainProvider
	{
		private string providerName = "Rendezvous Domain Provider";
		private string description = "Simias Location provider which uses the mDns protocol to resolve objects";
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

		private Hashtable searchContexts;
		private mDnsProviderLock mdnsLock;

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

		#region Constructor
		public mDnsProvider()
		{
			searchContexts = new Hashtable();
			mdnsLock = new mDnsProviderLock();
		}
		#endregion


		#region Private Members

		private class mDnsSearchCtx
		{
			public ArrayList memberList = new ArrayList();
			public string id = Guid.NewGuid().ToString();
			public int index = 0;
		}

		private class mDnsProviderLock
		{
			string lockit = "mdns-lock";
		}

		private Uri MemberIDToUri( string memberID )
		{
			Uri locationUri = null;

			// Have we seen this member??
			lock( Simias.mDns.User.memberListLock )
			{
				foreach( Simias.mDns.RendezvousUser rUser in Simias.mDns.User.memberList )
				{
					if ( rUser.ID == memberID && rUser.Host != null )
					{
						// Resolve the address
						//Simias.mDns.User.kErrorType err;
						//char[] textualIP = new char[ 64 ];

						IPHostEntry host = Dns.GetHostByName( rUser.Host );
						if ( host != null )
						{
							long addr = host.AddressList[0].Address;
							string ipAddr = 
								String.Format( "{0}.{1}.{2}.{3}", 
								( addr & 0x000000FF ),
								( ( addr >> 8 ) & 0x000000FF ),
								( ( addr >> 16 ) & 0x000000FF ),
								( ( addr >> 24 ) & 0x000000FF ) );

							string fullPath = 
								"http://" + 
								ipAddr + 
								":" + 
								System.Convert.ToString( (ushort) rUser.Port ) +
								rUser.ServicePath;

							log.Debug( "fullPath: " + fullPath );
							locationUri = new Uri( fullPath );
						}

						/*
						err = 
							Simias.mDns.User.ResolveAddress( rUser.Host, textualIP.Length, textualIP ); 
						if ( err == Simias.mDns.User.kErrorType.kDNSServiceErr_NoError )
						{
							string addr = new string( textualIP );
						}
						*/

						break;
					}
				}
			}

			return locationUri;
		}

		private class MDnsSession
		{
			public string	MemberID;
			public string	OneTimePassword;
			public int		State;

			public MDnsSession()
			{
			}
		}

		#endregion


		#region Public Methods
		/// <summary>
		/// Performs authentication to the specified domain.
		/// </summary>
		/// <param name="domain">Domain to authenticate to.</param>
		/// <param name="httpContext">HTTP-specific request information. This is passed as a parameter so that a domain 
		/// provider may modify the HTTP request by adding special headers as necessary.
		/// 
		/// NOTE: The domain provider must NOT end the HTTP request.
		/// </param>
		/// <returns>The status from the authentication.</returns>
		public Authentication.Status Authenticate( Domain domain, HttpContext ctx )
		{
			string mdnsSessionTag = "mdns";

			Simias.Storage.Member member = null;
			Simias.Authentication.Status status = 
				new Simias.Authentication.Status( SCodes.Unknown );

			// Rendezvous domain requires session support
			if ( ctx.Session != null )
			{
				MDnsSession mdnsSession;

				// State should be 1
				string memberID = ctx.Request.Headers[ "mdns-member" ];
				if ( memberID == null || memberID == "" )
				{
					status.statusCode = SCodes.InvalidCredentials;
					return status;
				}

				member = domain.GetMemberByID( memberID );
				if ( member == null )
				{
					status.statusCode = SCodes.InvalidCredentials;
					return status;
				}

				status.UserName = member.Name;
				status.UserID = member.UserID;

				mdnsSession = ctx.Session[ mdnsSessionTag ] as MDnsSession;
				if ( mdnsSession == null )
				{
					mdnsSession = new MDnsSession();
					mdnsSession.MemberID = member.UserID;
					mdnsSession.State = 1;

					// Fixme
					mdnsSession.OneTimePassword = DateTime.Now.ToString();

					// Set the one time password in the response
					ctx.Response.AddHeader(
						"mdns-secret",
						mdnsSession.OneTimePassword);

					ctx.Session[ mdnsSessionTag ] = mdnsSession;
					status.statusCode = SCodes.InvalidCredentials;
				}
				else
				if ( status.UserID == mdnsSession.MemberID )
				{
					// State should be 1
					string oneTime = ctx.Request.Headers[ "mdns-secret" ];
					if ( oneTime != null && oneTime != "" )
					{
						// decrypt with user's public key
						if ( oneTime.Equals( mdnsSession.OneTimePassword ) == true )
						{
							status.statusCode = SCodes.Success;
							mdnsSession.State = 2;
						}
						else
						{
							status.statusCode = SCodes.InvalidCredentials;
						}				
					}
					else
					{
						// Fixme
						mdnsSession.OneTimePassword = DateTime.Now.ToString();
						mdnsSession.State = 1;

						// Set the one time password in the response
						ctx.Response.AddHeader(
							"mdns-secret",
							mdnsSession.OneTimePassword);

						status.statusCode = SCodes.InvalidCredentials;
					}
				}
			}

			return status;
		}

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
		}

		/// <summary>
		/// End the search for domain members.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers or
		/// FindNextDomainMembers methods.</param>
		public void FindCloseDomainMembers( string ctx )
		{
			log.Debug( "FindCloseDomainMembers called" );
			lock( this.mdnsLock )
			{
				if ( searchContexts.Contains( ctx ) )
				{
					searchContexts.Remove( ctx );
				}
			}
			return;
		}

		/// <summary>
		/// Starts a search for a specific set of domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="attributeName">Attribute name to search.</param>
		/// <param name="searchString">String that contains a pattern to search for.</param>
		/// <param name="operation">Type of search operation to perform.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindFirstDomainMembers( string domainID, string attributeName, string searchString, SearchOp operation, int count, out string searchContext, out Member[] memberList, out int total )
		{
			bool moreMembers = false;
			searchContext = null;
			memberList = null;
			total = 0;

			log.Debug( "FindFirstDomainMembers (with search op) called" );

			mDnsSearchCtx searchCtx = new mDnsSearchCtx();

			// Add the members in the store
			Simias.Storage.Domain domain = Store.GetStore().GetDomain( Simias.mDns.Domain.ID );
			foreach( Member member in domain.GetMemberList() )
			{
				total++;
				if ( searchCtx.index < count )
				{
					searchCtx.memberList.Add( member );
					searchCtx.index++;
				}
			}

			total += Simias.mDns.User.memberList.Count;
			if ( searchCtx.index < count )
			{
				// Now add the members in Rendezvous list - some will be duplicates
				lock( Simias.mDns.User.memberListLock )
				{
					bool foundIt;
					foreach( Simias.mDns.RendezvousUser rUser in Simias.mDns.User.memberList )
					{
						foundIt = false;
						foreach( Member member in searchCtx.memberList )
						{
							if ( rUser.ID == member.UserID )
							{
								break;
							}
						}

						if ( foundIt == false )
						{
							if ( searchCtx.index < count )
							{
								Member nMember = 
									new Member( rUser.FriendlyName, rUser.ID, Access.Rights.ReadWrite );
								searchCtx.memberList.Add( nMember );
								searchCtx.index++;
							}
							else
							{
								break;
							}
						}
					}
				}
			}

			if ( searchCtx.index < total )
			{
				moreMembers = true;
			}

			memberList = searchCtx.memberList.ToArray( typeof( Member ) ) as Member[];
			searchContext = searchCtx.id;
			lock( this.mdnsLock )
			{
				searchContexts.Add( searchCtx.id, searchCtx );
			}

			return moreMembers;
		}

		/// <summary>
		/// Starts a search for all domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindFirstDomainMembers( string domainID, int count, out string searchContext, out Member[] memberList, out int total )
		{
			return FindFirstDomainMembers(domainID, PropertyTags.FullName, String.Empty, SearchOp.Contains, count, out searchContext, out memberList, out total);
		}


		/// <summary>
		/// Continues the search for domain members previous to the current record location.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindPreviousDomainMembers( ref string searchContext, int count, out Member[] memberList )
		{
			memberList = null;

			log.Debug( "FindPreviousDomainMembers (with search op) called" );

			return false;
		}


		/// <summary>
		/// Continues the search for domain members from the current record location.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindNextDomainMembers( ref string searchContext, int count, out Member[] memberList )
		{
			memberList = null;

			log.Debug( "FindNextDomainMembers called" );

			return false;
		}

		/// <summary>
		/// Continues the search for domain members from the specified record location.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="offset">Record offset to return members from.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindSeekDomainMembers( ref string searchContext, int offset, int count, out Member[] memberList )
		{
			memberList = null;

			log.Debug( "FindSeekDomainMembers called" );

			return false;
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
			return ( domainID.ToLower() == Simias.mDns.Domain.ID ) ? true : false;
		}

		/// <summary>
		/// Returns the network location for the the specified
		/// domain.
		/// </summary>
		/// <param name="domainID">Identifier for the domain.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolveLocation( string domainID )
		{
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
			Uri locationUri = null;

			log.Debug( "ResolveLocation with domainID and collectionID" );
			if( domainID.ToLower() == Simias.mDns.Domain.ID )
			{
				try
				{
					Store store = Store.GetStore();
					Domain domain = store.GetDomain( domainID.ToLower() );

					Simias.POBox.POBox poBox = 
						Simias.POBox.POBox.FindPOBox( store, domainID, domain.GetCurrentMember().UserID );

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
			Uri locationUri = null;
			if( domainID.ToLower() == Simias.mDns.Domain.ID )
			{
				try
				{
					locationUri = MemberIDToUri( userID );
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
		/// Returns the network location of where to the specified user's POBox is located.
		/// </summary>
		/// <param name="domainID">Identifier of the domain where a 
		/// collection is to be created.</param>
		/// <param name="userID">The member that will owns the POBox.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolvePOBoxLocation( string domainID, string userID )
		{
			Uri locationUri = null;
			if( domainID.ToLower() == Simias.mDns.Domain.ID )
			{
				try
				{
					locationUri = MemberIDToUri( userID );
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
		/// Informs the domain provider that the specified member object is about to be
		/// committed to the domain's member list. This allows an opportunity for the 
		/// domain provider to add any domain specific attributes to the member object.
		/// </summary>
		/// <param name="domainID">Identifier of a domain.</param>
		/// <param name="member">Member object that is about to be committed to the domain's member list.</param>
		public void PreCommit( string domainID, Member member )
		{
			return;
		}
		#endregion
	}
}
