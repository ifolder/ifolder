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
 *  Authors:
 *		Boyd Timothy <btimothy@novell.com>
 *		Brady Anderson <banderso@novell.com>
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


namespace Simias.Gaim
{
	/// <summary>
	/// Summary description for Auth.
	/// </summary>
	public class ClientAuthentication
	{
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public ClientAuthentication()
		{
		}

		/// <summary>
		/// Login to a remote domain using username and password
		/// Assumes a slave domain has been provisioned locally
		/// </summary>
		/// <param name="domainID">ID of the remote domain.</param>
		/// <param name="user">Member to login as</param>
		/// <param name="password">Password to validate user.</param>
		/// <returns>
		/// The status of the remote authentication
		/// </returns>
		public
		Simias.Authentication.Status
		Authenticate( string collectionID )
		{
			HttpWebResponse response = null;

			Simias.Authentication.Status status =	
				new Simias.Authentication.Status( SCodes.Unknown );

			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain( GaimDomain.ID );
			Simias.Storage.Member member = domain.GetCurrentMember();

			GaimDomainProvider gaimDomainProvider = new GaimDomainProvider();

			Uri remoteUri = gaimDomainProvider.ResolveLocation( GaimDomain.ID, collectionID );
			if ( remoteUri == null )
			{
				return status;
			}

			Uri loginUri = new Uri( remoteUri.ToString() + "/Login.ashx" );
			//auth.Url = loginUri.ToString() + "/Login.ashx";
			//auth.MemberID = domain.GetCurrentMember().UserID;
		//	auth.DomainID = Simias.mDns.Domain.ID;

			HttpWebRequest request = WebRequest.Create( loginUri ) as HttpWebRequest;
			WebState webState = new WebState();
			webState.InitializeWebRequest( request );
			//request.Credentials = networkCredential;
			
			request.Headers.Add( 
				Simias.Security.Web.AuthenticationService.Login.DomainIDHeader,
				GaimDomain.ID );

			request.Headers.Add( "gaim-member", member.UserID );
			
			request.Method = "POST";
			request.ContentLength = 0;

			try
			{
				request.GetRequestStream().Close();
				response = request.GetResponse() as HttpWebResponse;
				if ( response != null )
				{
					status.statusCode = SCodes.Success;
				}
			}
			catch(WebException webEx)
			{
				response = webEx.Response as HttpWebResponse;
				if (response != null)
				{
					log.Debug( response.StatusCode.ToString() );
					if ( response.StatusCode == System.Net.HttpStatusCode.Unauthorized )
					{
						string oneTimeChallenge = response.Headers[ "gaim-secret" ];
						if ( oneTimeChallenge != null && oneTimeChallenge != "" )
						{
							HttpWebRequest request2 = WebRequest.Create( loginUri ) as HttpWebRequest;
							WebState webState2 = new WebState();
							webState2.InitializeWebRequest( request2 );
							//request.Credentials = networkCredential;
			
							request2.Headers.Add( 
								Simias.Security.Web.AuthenticationService.Login.DomainIDHeader,
								GaimDomain.ID );
							request2.Headers.Add( "gaim-member", member.UserID );
							request2.Headers.Add( "gaim-secret", oneTimeChallenge );
			
							request2.Method = "POST";
							request2.ContentLength = 0;

							// Try again with the challenge
							try
							{
								request2.GetRequestStream().Close();
								response = request2.GetResponse() as HttpWebResponse;
								if ( response != null )
								{
									status.statusCode = SCodes.Success;
								}
							}
							catch( WebException webEx2 )
							{
								log.Debug( webEx2.Status.ToString() );
							}
							catch( Exception e2 )
							{
								log.Debug( e2.Message );
								log.Debug( e2.StackTrace );
							}
						}
					}
				}
				else
				{
					log.Debug(webEx.Message);
					log.Debug(webEx.StackTrace);
				}
			}
			catch(Exception ex)
			{
				log.Debug(ex.Message);
				log.Debug(ex.StackTrace);
			}

			return status;
		}
	}
}
