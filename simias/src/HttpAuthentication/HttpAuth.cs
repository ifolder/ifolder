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
 *  Author: Brady Anderson (banderso@novell.com)
 *
 ***********************************************************************/

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.ComponentModel;
//using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

using Simias;
using Simias.Storage;


namespace Simias.Authentication
{
	public class Session
	{
		public Session()
		{
			Requests = 0;
		}

		public string	MemberID;
		public UInt64	Requests;
	}

	/// <summary>
	/// Summary description for Http
	/// </summary>
	public class Http
	{
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		// Response headers set by the Http Authentication Module
		public readonly static string GraceTotalHeader = "Simias-Grace-Total";
		public readonly static string GraceRemainingHeader = "Simias-Grace-Remaining";
		public readonly static string SimiasErrorHeader = "Simias-Error";
		//public readonly static string DomainIDHeader = "Domain-ID";

		private static readonly string sessionTag = "simias";
		private static readonly string[] rolesArray = { "users" };

		public Http()
		{
		}

		private 
		static 
		void 
		SetResponseHeaders( HttpContext ctx, Simias.Authentication.Status status )
		{
			switch ( status.statusCode )
			{
				case StatusCodes.SuccessInGrace:
				{
					ctx.Response.AppendHeader(
						GraceTotalHeader,
						status.TotalGraceLogins.ToString() );

					ctx.Response.AppendHeader(
						GraceRemainingHeader,
						status.RemainingGraceLogins.ToString() );
					break;
				}

				case StatusCodes.AccountDisabled:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.AccountDisabled.ToString() );
					break;
				}

				case StatusCodes.AccountLockout:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.AccountLockout.ToString() );
					break;
				}

				case StatusCodes.AmbiguousUser:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.AmbiguousUser.ToString() );
					break;
				}

				case StatusCodes.UnknownUser:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.UnknownUser.ToString() );
					break;
				}

				case StatusCodes.Unknown:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.Unknown.ToString() );
					break;
				}

				case StatusCodes.InvalidCredentials:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.InvalidCredentials.ToString() );
					break;
				}

				case StatusCodes.InvalidPassword:
				{
					ctx.Response.StatusCode = 401;
					/*
					context.Response.AppendHeader(
						Login.SimiasErrorHeader,
						StatusCodes.InvalidPassword.ToString() );
					*/
					break;
				}

				case StatusCodes.InternalException:
				{
					ctx.Response.StatusCode = 500;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.InternalException.ToString() );
					break;
				}

				default:
					ctx.Response.StatusCode = 401;
					break;
			}
		}

		/// <summary>
		/// Summary description for Http
		/// </summary>
		static public Simias.Storage.Member GetMember( HttpContext ctx )
		{
			Simias.Authentication.Session simiasSession;
			Simias.Authentication.Status status;
			Simias.Storage.Domain domain;
			Simias.Storage.Member member = null;
			Store store = Store.GetStore();

			ctx.Response.Cache.SetCacheability( HttpCacheability.NoCache );

			//
			// Look for the special domain ID header in the request.  If the
			// header doesn't exist use the default domain
			//

			string domainID = ctx.Request.Headers[ "DomainID" ];
			if ( domainID != null && domainID != "" )
			{
				domain = store.GetDomain( domainID );
			}
			else
			{
				domain = store.GetDomain( store.DefaultDomain );
			}

			if ( domain == null )
			{
				ctx.Response.StatusCode = 500;
				ctx.Response.StatusDescription = "Invalid Domain";
				ctx.ApplicationInstance.CompleteRequest();
				return null;
			}

			if ( ctx.Session != null )
			{
				if ( ctx.User.Identity.IsAuthenticated == false )
				{
					// Temporary hard coded for mDns

					// Temporary
					status = Simias.Authentication.Http.Authenticate( domain, ctx );
					if ( status.statusCode != StatusCodes.Success &&
						status.statusCode != StatusCodes.SuccessInGrace )
					{
						Simias.Authentication.Http.SetResponseHeaders( ctx, status );
						if ( ctx.Response.StatusCode == 401 )
						{
							ctx.Response.AddHeader( 
								"WWW-Authenticate", 
								String.Concat("Basic realm=\"", domain.Name, "\""));
						}

						ctx.ApplicationInstance.CompleteRequest();
						return null;
					}

					// Authentication modules are required to set the member's
					// userID on successful authentication - let's make sure
					if ( status.UserID == null || status.UserID == "")
					{
						ctx.Response.StatusCode = 500;
						ctx.Response.StatusDescription = "Unknown";
						ctx.ApplicationInstance.CompleteRequest();
						return null;
					}

					member = domain.GetMemberByID( status.UserID );
					if ( member == null )
					{
						ctx.Response.StatusCode = 500;
						ctx.Response.StatusDescription = "Unknown";
						ctx.ApplicationInstance.CompleteRequest();
						return null;
					}

					simiasSession = new Simias.Authentication.Session();
					simiasSession.MemberID = member.UserID;
					simiasSession.Requests++;
					ctx.Session[ sessionTag ] = simiasSession;

					// Setup a principal
					ctx.User = 
						new GenericPrincipal(
							new GenericIdentity(
								member.UserID,
								"Simias Authentication"), 
								//"Basic authentication"), 
								rolesArray);

					Thread.CurrentPrincipal = ctx.User;
				}
				else
				{
					simiasSession = ctx.Session[ sessionTag ] as Simias.Authentication.Session;
					simiasSession.Requests++;
					member = domain.GetMemberByID( simiasSession.MemberID );
				}
			}
			else
			{
				// No session exists so "authenticate" every request
			}

			return member;
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
		static 
		public 
		Simias.Authentication.Status 
		Authenticate( Simias.Storage.Domain domain, HttpContext ctx )
		{
			string mdnsSessionTag = "mdns";

			Simias.Storage.Member member = null;
			Simias.Authentication.Status status = 
				new Simias.Authentication.Status( StatusCodes.Unknown );

			// Rendezvous domain requires session support
			if ( ctx.Session != null )
			{
				MDnsSession mdnsSession;

				// State should be 1
				string memberID = ctx.Request.Headers[ "mdns-member" ];
				if ( memberID == null || memberID == "" )
				{
					status.statusCode = StatusCodes.InvalidCredentials;
					return status;
				}

				member = domain.GetMemberByID( memberID );
				if ( member == null )
				{
					status.statusCode = StatusCodes.InvalidCredentials;
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
					status.statusCode = StatusCodes.InvalidCredentials;
				}
				else
				{
					if ( status.UserID == mdnsSession.MemberID )
					{
						// State should be 1
						string oneTime = ctx.Request.Headers[ "mdns-secret" ];

						// decrypt with user's public key

						if ( oneTime.Equals( mdnsSession.OneTimePassword ) == true )
						{
							status.statusCode = StatusCodes.Success;
							mdnsSession.State = 2;
						}
						else
						{
							status.statusCode = StatusCodes.InvalidCredentials;
						}
					}
				}
			}

			return status;
		}
	}
}
