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
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Simias;
using Simias.Storage;
using SCodes = Simias.Authentication.StatusCodes;


namespace Simias.mDns
{
	/// <summary>
	/// Summary description for Auth.
	/// </summary>
	public class ClientAuthentication
	{
		public ClientAuthentication()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public bool Authenticate( string collectionID )
		{
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain( Simias.mDns.Domain.ID );
			Simias.Storage.Collection collection = store.GetCollectionByID( collectionID );
			if ( domain == null || collection == null )
			{
				return false;
			}

			Simias.Location.mDnsProvider mdnsProv = new Simias.Location.mDnsProvider();
			Uri remoteUri = mdnsProv.ResolveLocation( Simias.mDns.Domain.ID, collectionID );
			if ( remoteUri == null )
			{
				return false;
			}

			SimiasAuthenticationProxy auth = new SimiasAuthenticationProxy();
			auth.Url = remoteUri.ToString() + "/mDns.asmx";
			auth.MemberID = collection.GetCurrentMember().UserID;
			auth.DomainID = Simias.mDns.Domain.ID;
			
			try
			{
				auth.Authenticate( auth.MemberID );
			}
			catch( WebException webEx )
			{
				Console.WriteLine( webEx.Status );

				HttpWebResponse response = webEx.Response as HttpWebResponse;
				if ( response != null )
				{
					Console.WriteLine( response.StatusCode.ToString() );
					if ( response.StatusCode == System.Net.HttpStatusCode.Unauthorized )
					{
						string oneTimeChallenge = response.Headers[ "mdns-secret" ];
						if ( oneTimeChallenge != null && oneTimeChallenge != "" )
						{
							auth.ChallengeSecret = oneTimeChallenge;
							
							// Try again with the challenge
							try
							{
								auth.Authenticate( auth.MemberID );
								Console.WriteLine( "Successfull authentication" );
							}
							catch( WebException webEx2 )
							{
								Console.WriteLine( webEx2.Status );
							}
							catch( Exception e2 )
							{
								Console.WriteLine( e2.Message );
								Console.WriteLine( e2.StackTrace );
							}
						}
					}
				}
			}
			catch( Exception e )
			{
				Console.WriteLine( e.Message );
				Console.WriteLine( e.StackTrace );
			}

			return false;
		}
	}

	/// <summary>
	/// Summary description for SimiasClientProtocol.
	/// </summary>
	public class SimiasAuthenticationProxy : SimiasmDnsAuthenticationWebService
	{
		string memberID;
		string domainID;
		string secret;

		protected override WebRequest GetWebRequest(Uri uri)
		{
			WebRequest request = base.GetWebRequest( uri );
			if ( memberID != null )
				request.Headers.Add( "mdns-member", memberID );
			if ( domainID != null )
				request.Headers.Add( "DomainID", domainID );
			if ( secret != null )
				request.Headers.Add( "mdns-secret", secret );
				
			return request;
		}

		public string MemberID
		{
			get { return memberID; }
			set { memberID = value; }
		}

		public string DomainID
		{
			set { domainID = value; }
		}

		public string ChallengeSecret
		{
			set { secret = value; }
		}
	}


	/// <summary>
	/// Summary description for Auth.
	/// </summary>
	public class ServerAuthentication
	{
		public ServerAuthentication()
		{
			//
			// TODO: Add constructor logic here
			//
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

		/// <summary>
		/// TEMP function
		/// </summary>
		public 
		static
		Simias.Authentication.Status 
		Authenticate( Simias.Storage.Domain domain, HttpContext ctx )
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
			}

			return status;
		}
	}

	/// <summary>
	/// Rendezvous authentication web service
	/// </summary>
	[WebService(
		 Namespace="http://novell.com/simias/mdns-auth/",
		 Name="Simias mDns Authentication Web Service",
		 Description="Web Service providing remote authentication to an mDns domain")]
	public class AuthService : WebService
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( AuthService ) );

		/// <summary>
		/// Yep
		/// </summary>
		public AuthService()
		{
		}

		/// <summary>
		/// Authenticate
		/// </summary>
		/// 
		[WebMethod(EnableSession = true, Description="Authenticate to a remote mDns domain.")]
		[SoapDocumentMethod]
		public bool Authenticate( string memberID )
		{
			Simias.Storage.Member member =
				Simias.Authentication.Http.GetMember( HttpContext.Current );

			if ( member != null && member.UserID == memberID )
			{
				return true;
			}

			return false;
		}
	}
}
